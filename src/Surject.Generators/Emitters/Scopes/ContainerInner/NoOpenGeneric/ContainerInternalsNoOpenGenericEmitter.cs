using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Threading.Tasks;
using Surject.Abstractions.Resolutions;
using Surject.Generators.Discovery.ServiceRegistration;
using Surject.Generators.Emitters.Helpers;
using Surject.Generators.Emitters.Helpers.Visitors;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;
using Surject.Unity.Handles;

namespace Surject.Generators.Emitters.Scopes.ContainerInner.NoOpenGeneric;

internal readonly ref struct ContainerInternalsNoOpenGenericEmitter : IChainedEmitter {
    private readonly ContainerModel _model;
    
    internal ContainerInternalsNoOpenGenericEmitter(ContainerModel model) => _model = model;
    
    public void Emit(IndentedTextWriter writer) {
        EmitProperties(writer);
        EmitMembers(writer);
        EmitDisposableTrackers(writer);
        EmitDisposeMethods(writer);
    }

    private void EmitProperties(IndentedTextWriter writer) {
        writer.WriteLine($"public global::{typeof(IResolver).FullName} Resolver {{ get; }}");
        writer.WriteLine($"public global::{typeof(IResolver).FullName}? ParentResolver {{ get; }}");
        writer.WriteLine();
    }

    private void EmitMembers(IndentedTextWriter writer) {
        new ContainerInternalSingletonConcreteBindingNoOpenGenericEmitter(_model).Emit(writer);
        writer.WriteLine();
        
        new ContainerInternalMultiBindingNoOpenGenericEmitter(_model).Emit(writer);
        writer.WriteLine();
        
        new ContainerInternalDiscoveryCollectionNoOpenGenericEmitter(_model).Emit(writer);
        writer.WriteLine();
    }

    // This is independent of an open-generic context, so we can emit it here.
    private void EmitDisposableTrackers(IndentedTextWriter writer) {
        if (!ParseHelpers.ShouldTrackTransientDisposal(_model)) {
            return;
        }
        
        writer.WriteLine($"internal readonly global::{typeof(DisposableTracker).FullName} __disposables = new();");
        writer.WriteLine($"internal readonly global::{typeof(AsyncDisposableTracker).FullName} __asyncDisposables = new();");
        writer.WriteLine();
    }

    private void EmitDisposeMethods(IndentedTextWriter writer) {
        new ContainerInternalDisposalEmitter(_model).Emit(writer);
    }
}

internal readonly ref struct ContainerInternalSingletonConcreteBindingNoOpenGenericEmitter : IChainedEmitter {
    private readonly ContainerModel _model;
    
    internal ContainerInternalSingletonConcreteBindingNoOpenGenericEmitter(ContainerModel model) => _model = model;

    public void Emit(IndentedTextWriter writer) {
        HashSet<ITypeReferenceModel> uniqueEntryRegistrationTypes = [];
        EntryRegistrationTypeVisitor registrationVisitor = new();

        foreach (RegistrationModel registration in _model.Registrations) {
            if (registration.Entry.Lifetime == LifetimeKind.Transient) {
                continue;
            }
            
            ITypeReferenceModel? entryType = registration.Entry.Accept<EntryRegistrationTypeVisitor, ITypeReferenceModel?>(ref registrationVisitor);

            if (entryType == null || !uniqueEntryRegistrationTypes.Add(entryType)) {
                continue;
            }
            
            SingletonFieldEmitterNoOpenGenericVisitor singletonFieldEmitterNoOpenGenericVisitor = new(writer, registration, entryType);
            registration.Entry.Accept<SingletonFieldEmitterNoOpenGenericVisitor, VoidVisitor>(ref singletonFieldEmitterNoOpenGenericVisitor);
        }
    }
}

internal readonly ref struct ContainerInternalMultiBindingNoOpenGenericEmitter : IChainedEmitter {
    private struct MultiBindSetAggregate {
        internal bool SyncAllSingleton;
        internal bool HasSync;
        internal bool AsyncAllSingleton;
        internal bool HasAsync;
    }
    
    private readonly ContainerModel _model;
    
    internal ContainerInternalMultiBindingNoOpenGenericEmitter(ContainerModel model) => _model = model;
    
    public void Emit(IndentedTextWriter writer) {
        Dictionary<(ITypeReferenceModel Contract, string? Key), MultiBindSetAggregate> aggregateBySet = new();
        List<(ITypeReferenceModel Contract, string? Key)> setOrder = [];
        List<ITypeReferenceModel> contractsBuffer = new(4);
        
        EntryRegistrationTypeVisitor registrationVisitor = new();
        
        foreach (RegistrationModel registration in _model.Registrations) {
            if (!IsEligibleForOrderedMultiBind(registration.Entry.Kind)) {
                continue;
            }
            
            if ((registration.ModifiersDescriptor & ModifierKind.AsCollection) != ModifierKind.AsCollection) {
                continue;
            }
            
            ITypeReferenceModel entryType = registration.Entry.Accept<EntryRegistrationTypeVisitor, ITypeReferenceModel?>(ref registrationVisitor)!;

            contractsBuffer.Clear();
            contractsBuffer.Add(entryType);
            ParseHelpers.GetAllContractsOf(registration, entryType, contractsBuffer);
            
            string? key = ParseHelpers.GetKeyExprOrNull(registration);
            bool isSingleton = registration.Entry.Lifetime == LifetimeKind.Singleton;
            bool isAsync = registration.Entry.Kind == EntryKind.AddAsyncFactory;

            foreach (ITypeReferenceModel contract in contractsBuffer) {
                (ITypeReferenceModel, string?) setKey = (contract, key);
                bool isNewSet = !aggregateBySet.TryGetValue(setKey, out MultiBindSetAggregate aggregate);
                
                if (isNewSet) {
                    setOrder.Add(setKey);
                }

                if (isAsync) {
                    aggregate.AsyncAllSingleton = aggregate.HasAsync ? aggregate.AsyncAllSingleton && isSingleton : isSingleton;
                    aggregate.HasAsync = true;
                }
                else {
                    aggregate.SyncAllSingleton = aggregate.HasSync ? aggregate.SyncAllSingleton && isSingleton : isSingleton;
                    aggregate.HasSync = true;
                }
                
                aggregateBySet[setKey] = aggregate;
            }
        }

        foreach ((ITypeReferenceModel contract, string? key) in setOrder) {
            MultiBindSetAggregate aggregate = aggregateBySet[(contract, key)];
            
            if (aggregate is { HasSync: true, SyncAllSingleton: true }) {
                string syncFieldName = key is null
                    ? BuildHelpers.BuildMultiBindLocalArrayFieldNotKeyed(contract)
                    : BuildHelpers.BuildMultiBindLocalArrayFieldKeyed(contract, key);

                writer.WriteLine($"internal (int Order, {contract.FQNConstructedArgBased} Instance)[]? {syncFieldName};");
            }

            if (aggregate is { HasAsync: true, AsyncAllSingleton: true }) {
                string asyncArrayFieldName = key is null
                    ? BuildHelpers.BuildMultiBindLocalAsyncArrayFieldNotKeyed(contract)
                    : BuildHelpers.BuildMultiBindLocalAsyncArrayFieldKeyed(contract, key);

                string asyncTaskFieldName = key is null
                    ? BuildHelpers.BuildMultiBindLocalArrayTaskFieldNotKeyed(contract)
                    : BuildHelpers.BuildMultiBindLocalArrayTaskFieldKeyed(contract, key);

                writer.WriteLine($"internal (int Order, {contract.FQNConstructedArgBased} Instance)[]? {asyncArrayFieldName};");
                writer.WriteLine(
                    $"internal global::{typeof(Task).FullName}<(int Order, {contract.FQNConstructedArgBased} Instance)[]>? {asyncTaskFieldName};"
                );
            }
        }
    }
    
    private static bool IsEligibleForOrderedMultiBind(EntryKind kind) => kind switch {
        EntryKind.AddOpenGeneric => false,
        EntryKind.AddAmbient => false,
        _ => true
    };
}

internal readonly ref struct ContainerInternalDiscoveryCollectionNoOpenGenericEmitter : IChainedEmitter {
    private readonly ContainerModel _model;
    
    internal ContainerInternalDiscoveryCollectionNoOpenGenericEmitter(ContainerModel model) => _model = model;

    public void Emit(IndentedTextWriter writer) {
        HashSet<(ITypeReferenceModel Contract, string? Key)> emitted = new();
        List<ITypeReferenceModel> contractsBuffer = new(4);
        
        EntryRegistrationTypeVisitor registrationVisitor = new();

        foreach (RegistrationModel registration in _model.Registrations) {
            if (!IsDiscoveredCollectionEntry(registration.Entry.Kind)) {
                continue;
            }
            
            ITypeReferenceModel entryType = registration.Entry.Accept<EntryRegistrationTypeVisitor, ITypeReferenceModel?>(ref registrationVisitor)!;

            contractsBuffer.Clear();
            contractsBuffer.Add(entryType);
            ParseHelpers.GetAllContractsOf(registration, entryType, contractsBuffer);
            
            string? key = ParseHelpers.GetKeyExprOrNull(registration);

            foreach (ITypeReferenceModel contract in contractsBuffer) {
                if (!emitted.Add((contract, key))) {
                    continue;
                }
                
                string fieldName = key is null
                    ? BuildHelpers.BuildFoundArrayFieldNotKeyed(contract)
                    : BuildHelpers.BuildFoundArrayFieldKeyed(contract, key);

                writer.WriteLine($"internal {contract.FQNConstructedArgBased}[]? {fieldName};");
            }
        }
    }
    
    private static bool IsDiscoveredCollectionEntry(EntryKind kind) => kind switch {
        EntryKind.AddAllFromChildren => true,
        EntryKind.AddAllFromHierarchy => true,
        EntryKind.AddAllFromParent => true,
        _ => false
    };

}

internal readonly ref struct ContainerInternalDisposalEmitter : IChainedEmitter {
    private readonly ContainerModel _model;
    
    internal ContainerInternalDisposalEmitter(ContainerModel model) => _model = model;
    
    public void Emit(IndentedTextWriter writer) {
        EmitOpenGenericDisposePartial(writer);
        EmitDisposeMethod(writer);
        EmitDisposeMethodCore(writer);
        EmitOpenGenericDisposeAsyncPartialAggressive(writer);
        EmitAsyncDisposeMethod(writer);
    }
    
    private void EmitOpenGenericDisposePartial(IndentedTextWriter writer) {
        writer.WriteMultiline($"partial void DisposeOpenGenerics();");
        writer.WriteLine();
    }

    private void EmitDisposeMethod(IndentedTextWriter writer) {
        writer.WriteMultiline(
            $$"""
              public void Dispose() {
                  Dispose(disposing: true);
                  global::{{typeof(GC)}}.{{nameof(GC.SuppressFinalize)}}(this);
              }
              """
        );
        writer.WriteLine();
    }

    private void EmitDisposeMethodCore(IndentedTextWriter writer) {
        writer.WriteLine($"private void Dispose(bool disposing) {{");
        writer.Indent++;
        writer.WriteLine($"if (disposing) {{");
        writer.Indent++;

        if (ParseHelpers.ShouldTrackTransientDisposal(_model)) {
            writer.WriteLine($"__disposables.{nameof(DisposableTracker.DisposeAll)}();");
        }
        
        writer.WriteLine($"DisposeOpenGenerics();");
        writer.WriteLine();
        
        Span<RegistrationModel> registrations = _model.Registrations.AsSpanMut();
        registrations.Reverse();
        
        HashSet<ITypeReferenceModel> uniqueEntryRegistrationTypes = [];
        EntryRegistrationTypeVisitor registrationVisitor = new();

        foreach (RegistrationModel registration in _model.Registrations) {
            if ((registration.ModifiersDescriptor & ModifierKind.DoNotDispose) == ModifierKind.DoNotDispose) {
                continue;
            }

            if (registration.Entry.Lifetime == LifetimeKind.Transient
                && (registration.ModifiersDescriptor & ModifierKind.TrackDisposable) != ModifierKind.TrackDisposable)
            {
                continue;
            }
            
            ITypeReferenceModel? entryType = registration.Entry.Accept<EntryRegistrationTypeVisitor, ITypeReferenceModel?>(ref registrationVisitor);

            if (entryType == null || !uniqueEntryRegistrationTypes.Add(entryType)) {
                continue;
            }
            
            SingletonFieldSyncDisposalEmitterNoOpenGenericVisitor syncDisposalEmitter = new(writer, entryType, registration);
            registration.Entry.Accept<SingletonFieldSyncDisposalEmitterNoOpenGenericVisitor, VoidVisitor>(ref syncDisposalEmitter);
        }
        
        writer.Indent--;
        writer.WriteLine("}");
        
        writer.Indent--;
        writer.WriteLine("}");
        writer.WriteLine();
    }

    // We aggressively emit a noop method here in the case there are no open generics because the pass
    // for the open generic part doesn't run, and a naive partial forward decl with no definition will be invalid.
    private void EmitOpenGenericDisposeAsyncPartialAggressive(IndentedTextWriter writer) {
        if ((_model.EntriesDescriptor & EntryKind.AddOpenGeneric) != EntryKind.AddOpenGeneric) {
            writer.WriteLine($"private {typeof(Task).FullName} DisposeOpenGenericsAsync() => {typeof(Task)}.{nameof(Task.CompletedTask)};");
            writer.WriteLine();
            return;
        }
        
        writer.WriteLine($"private partial {typeof(Task).FullName} DisposeOpenGenericsAsync();");
        writer.WriteLine();
    }

    private void EmitAsyncDisposeMethod(IndentedTextWriter writer) {
        writer.WriteLine($"public async global::{nameof(ValueTask)} DisposeAsync() {{");
        writer.Indent++;

        if (ParseHelpers.ShouldTrackTransientDisposal(_model)) {
            writer.WriteLine($"__asyncDisposables.{nameof(AsyncDisposableTracker.DisposeAllAsync)}();");
        }
        
        writer.WriteLine($"await DisposeOpenGenericsAsync();");
        
        Span<RegistrationModel> registrations = _model.Registrations.AsSpanMut();
        registrations.Reverse();
        
        HashSet<ITypeReferenceModel> uniqueEntryRegistrationTypes = [];
        EntryRegistrationTypeVisitor registrationVisitor = new();

        foreach (RegistrationModel registration in _model.Registrations) {
            if ((registration.ModifiersDescriptor & ModifierKind.DoNotDispose) == ModifierKind.DoNotDispose) {
                return;
            }
            
            if (registration.Entry.Lifetime == LifetimeKind.Transient
                && (registration.ModifiersDescriptor & ModifierKind.TrackDisposable) != ModifierKind.TrackDisposable)
            {
                continue;
            }

            ITypeReferenceModel? entryType = registration.Entry.Accept<EntryRegistrationTypeVisitor, ITypeReferenceModel?>(ref registrationVisitor);

            if (entryType == null || !uniqueEntryRegistrationTypes.Add(entryType)) {
                continue;
            }
            
            SingletonFieldAsyncDisposalEmitterNoOpenGenericVisitor asyncDisposalEmitter = new(writer, entryType, registration);
            registration.Entry.Accept<SingletonFieldAsyncDisposalEmitterNoOpenGenericVisitor, VoidVisitor>(ref asyncDisposalEmitter);
        }
    }
}