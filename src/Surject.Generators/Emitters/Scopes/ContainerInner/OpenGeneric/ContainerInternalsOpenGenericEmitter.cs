using System;
using System.CodeDom.Compiler;
using Surject.Generators.Models.Concepts;

namespace Surject.Generators.Emitters.Scopes.ContainerInner;

internal readonly ref struct ContainerInternalsOpenGenericEmitter : IChainedEmitter {
    internal readonly ContainerModel _container;
    internal readonly OpenGenericInjectionLinkage _linkage;

    internal ContainerInternalsOpenGenericEmitter(ContainerModel model, OpenGenericInjectionLinkage linkage) {
        _container = model;
        _linkage = linkage;
    }

    public void Emit(IndentedTextWriter writer) {
        
    }
}