using System.CodeDom.Compiler;
using System.Collections.Generic;
using Surject.Generators.Discovery.ServiceRegistration;
using Surject.Generators.Emitters.Helpers.Visitors;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;

namespace Surject.Generators.Emitters.Scopes.ContainerInner.NoOpenGeneric;

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