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
        
        writer.WriteLine();
    }

    private void EmitDisposableTrackers(IndentedTextWriter writer) {
        if (!ParseHelpers.ShouldTrackTransientDisposal(_model)) {
            return;
        }
        
        writer.WriteLine($"internal readonly global::{typeof(DisposableTracker).FullName} __disposables = new();");
        writer.WriteLine($"internal readonly global::{typeof(AsyncDisposableTracker).FullName} __asyncDisposables = new();");
        writer.WriteLine();
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
        List<(ITypeReferenceModel Contract, string? Key)> setOrder = new();
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