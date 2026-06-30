using System.CodeDom.Compiler;
using System.Collections.Generic;
using Surject.Generators.Models.Collections;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;

namespace Surject.Generators.Emitters.Container;

internal readonly struct ContainerFieldEmitter {
    private readonly ContainerModel _container;
    private readonly InjectionLinkage _linkage;

    internal ContainerFieldEmitter(ContainerModel container, InjectionLinkage linkage) {
        _container = container;
        _linkage = linkage;
    }

    internal void Emit(IndentedTextWriter writer) {
        HashSet<(ITypeReferenceModel, string?)> emittedConcreteFields = [];
        Dictionary<(ITypeReferenceModel, string?), int> multiBindCounter = [];

        foreach (RegistrationModel binding in _container.Bindings) {
            if (binding.Entry.Lifetime == LifetimeKind.Transient) {
                continue;
            }

            string? key = GetKey(binding.Modifiers);
        }
    }

    private string? GetKey(EquatableArray<ModifierCommandModel> modifiers) {
        foreach (ref readonly ModifierCommandModel modifier in modifiers) {
            if (modifier.Kind == ModifierKind.WithId) {
                return modifier.StringArg;
            }
        }

        return null;
    }
}