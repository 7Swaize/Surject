using System.CodeDom.Compiler;
using System.Collections.Generic;
using Surject.Abstractions.Resolutions;
using Surject.Generators.Discovery.ServiceRegistration;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;
using Surject.Unity.Handles;

namespace Surject.Generators.Emitters.Scopes.ContainerInner;

internal readonly ref struct ContainerInternalsNoOpenGenericEmitter : IChainedEmitter {
    private readonly ContainerModel _model;
    
    internal ContainerInternalsNoOpenGenericEmitter(ContainerModel model) => _model = model;
    
    public void Emit(IndentedTextWriter writer) {
        EmitProperties(writer);
        EmitMembers(writer);
    }

    private void EmitProperties(IndentedTextWriter writer) {
        writer.WriteLine($"public global::{typeof(IResolver).FullName} Resolver {{ get; }}");
        writer.WriteLine($"public global::{typeof(IResolver).FullName}? ParentResolver {{ get; }}");
        writer.WriteLine();
    }

    private void EmitMembers(IndentedTextWriter writer) {
        writer.WriteLine($"internal readonly global::{typeof(DisposableTracker).FullName} __disposables = new();");
        writer.WriteLine($"internal readonly global::{typeof(AsyncDisposableTracker).FullName} __asyncDisposables = new();");
        writer.WriteLine();

        HashSet<ITypeReferenceModel> uniqueEntryRegistrationTypes = [];
        EntryRegistrationTypeVisitor registrationVisitor = new();

        foreach (RegistrationModel registration in _model.Registrations) {
            ITypeReferenceModel? entryType = registration.Entry.Accept<EntryRegistrationTypeVisitor, ITypeReferenceModel?>(ref registrationVisitor);

            if (entryType == null || !uniqueEntryRegistrationTypes.Add(entryType)) {
                continue;
            }
            
            FieldEmitterNoOpenGenericVisitor fieldEmitterNoOpenGenericVisitor = new(writer, registration, entryType);
            registration.Entry.Accept<FieldEmitterNoOpenGenericVisitor, VoidVisitor>(ref fieldEmitterNoOpenGenericVisitor);
        }
    }
}

internal readonly struct MultiBindCollectionEmitterNoOpenGenericVisitor : IModifierCommandVisitor<VoidVisitor> {
    private readonly HashSet<ITypeReferenceModel> _emittedMultiBindCollections;

    internal MultiBindCollectionEmitterNoOpenGenericVisitor(HashSet<ITypeReferenceModel> emittedMultiBindCollections)
        => _emittedMultiBindCollections = emittedMultiBindCollections;


    public VoidVisitor VisitTo(in ModifierCommandModel cmd) {
        throw new System.NotImplementedException();
    }
    public VoidVisitor ToImmediateImplementedInterfaces(in ModifierCommandModel cmd) {
        throw new System.NotImplementedException();
    }
    public VoidVisitor VisitToAllImplementedInterfaces(in ModifierCommandModel cmd) {
        throw new System.NotImplementedException();
    }
    public VoidVisitor VisitWithId(in ModifierCommandModel cmd) {
        throw new System.NotImplementedException();
    }
    public VoidVisitor VisitEager(in ModifierCommandModel cmd) {
        throw new System.NotImplementedException();
    }
    public VoidVisitor VisitLazy(in ModifierCommandModel cmd) {
        throw new System.NotImplementedException();
    }
    public VoidVisitor VisitWithArgument(in ModifierCommandModel cmd) {
        throw new System.NotImplementedException();
    }
    public VoidVisitor VisitOverrideExisting(in ModifierCommandModel cmd) {
        throw new System.NotImplementedException();
    }
    public VoidVisitor VisitAsCollection(in ModifierCommandModel cmd) {
        throw new System.NotImplementedException();
    }
    public VoidVisitor VisitAsPrimary(in ModifierCommandModel cmd) {
        throw new System.NotImplementedException();
    }
    public VoidVisitor VisitDoNotDispose(in ModifierCommandModel cmd) {
        throw new System.NotImplementedException();
    }
    public VoidVisitor VisitTrackDisposable(in ModifierCommandModel cmd) {
        throw new System.NotImplementedException();
    }
    public VoidVisitor VisitUnderTransform(in ModifierCommandModel cmd) {
        throw new System.NotImplementedException();
    }
    public VoidVisitor VisitUnderObjectOfType(in ModifierCommandModel cmd) {
        throw new System.NotImplementedException();
    }
    public VoidVisitor VisitWithGameObjectName(in ModifierCommandModel cmd) {
        throw new System.NotImplementedException();
    }
    public VoidVisitor VisitDoNotDestroy(in ModifierCommandModel cmd) {
        throw new System.NotImplementedException();
    }
}