using System.CodeDom.Compiler;
using System.Collections.Generic;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;

namespace Surject.Generators.Emitters.Container;

internal readonly struct ContainerFieldEmitter {
    private readonly ContainerModel _container;

    internal ContainerFieldEmitter(ContainerModel container) {
        _container = container;
    }

    internal void Emit(IndentedTextWriter writer) {
        HashSet<ITypeReferenceModel> emittedTypes = [];
        
        foreach (RegistrationModel binding in _container.Bindings) {
            if (binding.Entry.Kind == EntryKind.Decorate) {
                continue;
            }
        }
    }
}