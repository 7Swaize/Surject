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
            
            SingularFieldEmitterNoOpenGenericVisitor singularFieldEmitterNoOpenGenericVisitor = new(writer, registration, entryType);
            registration.Entry.Accept<SingularFieldEmitterNoOpenGenericVisitor, VoidVisitor>(ref singularFieldEmitterNoOpenGenericVisitor);
        }
    }
}

internal readonly struct MultiBindCollectorNoOpenGenericVisitor : IEntryCommandVisitor<VoidVisitor> {
    
}