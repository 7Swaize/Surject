using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Surject.Abstractions.Resolutions;
using Surject.Generators.Emitters.Helpers;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;
using Surject.Unity.Utility.Exceptions;

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
        EmitExceptionHelpers(writer);
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

    private void EmitExceptionHelpers(IndentedTextWriter writer) {
        ITypeReferenceModel containerType = _container.Decl.AsTypeRef;
        
        EmitHelpers.EmitDoesNotReturnAttribute(writer);
        EmitHelpers.EmitMethodImplAttribute(writer, MethodImplOptions.NoInlining);
        writer.WriteMultiline(
            $$"""
              private static T __ThrowMissing<T>()
                  => throw new {{typeof(SurjectRuntimeException).FullName}}(
                      $"No binding for {typeof(T).FullName} in type {{containerType.FQNGenericOmitted}} or its parent.");
              """
        );
        
        writer.WriteLine();
        
        EmitHelpers.EmitDoesNotReturnAttribute(writer);
        EmitHelpers.EmitMethodImplAttribute(writer, MethodImplOptions.NoInlining);
        writer.WriteMultiline(
            $$"""
              private static {{typeof(ValueTask).FullName}}<T> __ThrowMissingAsync<T>()
                  => throw new {{typeof(SurjectRuntimeException).FullName}}(
                         $"No async binding for {typeof(T).FullName} in type {{containerType.FQNGenericOmitted}} or its parent.");
              """
        );
    }
}