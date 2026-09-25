using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Threading.Tasks;
using Surject.Generators.Discovery.ServiceRegistration;
using Surject.Generators.Emitters.Helpers;
using Surject.Generators.Emitters.Helpers.Visitors;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;
using Surject.Unity.Handles;

namespace Surject.Generators.Emitters.Scopes.ContainerInner.NoOpenGeneric;

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
        
        writer.Indent--;
        writer.WriteLine("}");
    }
}