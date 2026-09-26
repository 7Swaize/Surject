using System.CodeDom.Compiler;
using Surject.Generators.Models.Concepts;

namespace Surject.Generators.Emitters.Scopes.ResolverInner;

internal readonly ref struct ResolverInternalsEmitterCore : IChainedEmitter {
    private readonly ContainerModel _container;
    private readonly OpenGenericInjectionLinkage _linkage;

    internal ResolverInternalsEmitterCore(ContainerModel container, OpenGenericInjectionLinkage linkage) {
        _container = container;
        _linkage = linkage;
    }

    public void Emit(IndentedTextWriter writer) { }
}