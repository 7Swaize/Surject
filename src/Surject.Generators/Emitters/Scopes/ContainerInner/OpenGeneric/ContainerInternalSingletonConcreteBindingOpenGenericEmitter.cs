using System.CodeDom.Compiler;
using System.Collections.Generic;
using Surject.Generators.Emitters.Helpers;
using Surject.Generators.Emitters.Helpers.Visitors;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;

namespace Surject.Generators.Emitters.Scopes.ContainerInner.OpenGeneric;

internal readonly ref struct ContainerInternalSingletonConcreteBindingOpenGenericEmitter : IChainedEmitter {
    private readonly ContainerModel _container;
    private readonly OpenGenericInjectionLinkage _linkage;

    internal ContainerInternalSingletonConcreteBindingOpenGenericEmitter(ContainerModel container, OpenGenericInjectionLinkage linkage) {
        _container = container;
        _linkage = linkage;
    }
    
    public void Emit(IndentedTextWriter writer) {
        HashSet<ITypeReferenceModel> uniqueEntryRegistrationTypes = [];
        EntryRegistrationTypeVisitor registrationVisitor = new();

        foreach (RegistrationModel registration in _container.Registrations) {
            if (registration.Entry.Lifetime == LifetimeKind.Transient) {
                continue;
            }

            if (registration.Entry.Kind != EntryKind.AddOpenGeneric) {
                continue;
            }
            
            ITypeReferenceModel? entryType = registration.Entry.Accept<EntryRegistrationTypeVisitor, ITypeReferenceModel?>(ref registrationVisitor);

            if (entryType == null || !uniqueEntryRegistrationTypes.Add(entryType)) {
                continue;
            }

            foreach (ITypeReferenceModel impl in _linkage.Linkage[entryType]) {
                if ((registration.ModifiersDescriptor & ModifierKind.WithId) != ModifierKind.None) {
                    writer.WriteLine($"internal {impl.FQNConstructedArgBased}? {BuildHelpers.BuildSingletonFieldNameNotKeyed(impl)};");
                    continue;
                }

                foreach (ref readonly ModifierCommandModel modifier in registration.Modifiers) {
                    if (modifier.Kind != ModifierKind.WithId) {
                        continue;
                    }
                    
                    writer.WriteLine(
                        $"internal {impl.FQNConstructedArgBased}? {BuildHelpers.BuildSingletonFieldNameKeyed(impl, modifier.StringArg1)};"
                    );
                }
            }
        }
    }
}