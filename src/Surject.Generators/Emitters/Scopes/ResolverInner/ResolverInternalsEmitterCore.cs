using System.CodeDom.Compiler;
using Surject.Abstractions.Resolutions;
using Surject.Generators.Emitters.Helpers;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;

namespace Surject.Generators.Emitters.Scopes.ResolverInner;

internal readonly ref struct ResolverInternalsEmitterCore : IChainedEmitter {
    private readonly ContainerModel _container;
    private readonly OpenGenericInjectionLinkage _linkage;

    internal ResolverInternalsEmitterCore(ContainerModel container, OpenGenericInjectionLinkage linkage) {
        _container = container;
        _linkage = linkage;
    }

    public void Emit(IndentedTextWriter writer) {
        EmitMembers(writer);
        EmitCtor(writer);
    }

    private void EmitMembers(IndentedTextWriter writer) {
        ITypeReferenceModel containerType = _container.Decl.AsTypeRef;
        
        writer.WriteLine($"private readonly {BuildHelpers.BuildContainerType(containerType)} _c;");
        writer.WriteLine($"private readonly global::{typeof(IResolver).FullName}? _parent;");
        writer.WriteLine($"private readonly {containerType.FQNConstructedArgBased} _scopeProvider;");
    }

    private void EmitCtor(IndentedTextWriter writer) {
        ITypeReferenceModel containerType = _container.Decl.AsTypeRef;
        
        writer.WriteMultiline(
            $$"""
              internal {{BuildHelpers.BuildResolverType(containerType)}}(
                    {{BuildHelpers.BuildContainerType(containerType)}} c,
                    global::{typeof(IResolver).FullName}? parent,
                    {{containerType.FQNConstructedArgBased}} scopeProvider)
                {
                    _c = c;
                    _parent = parent;
                    _scopeProvider = scopeProvider;
                }
                
              """
        );
    }
}