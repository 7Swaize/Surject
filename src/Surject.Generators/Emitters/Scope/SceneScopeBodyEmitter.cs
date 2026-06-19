using System.CodeDom.Compiler;
using Surject.Abstractions.Lifecycle;
using Surject.Abstractions.Registrations;
using Surject.Generators.Models.Concepts;
using Surject.Unity;

namespace Surject.Generators.Emitters.Scope;

internal readonly struct SceneScopeBodyEmitter {
    private readonly ContainerModel _model;

    internal SceneScopeBodyEmitter(ContainerModel model) {
        _model = model;
    }

    internal void Emit(IndentedTextWriter writer) {
        EmitAwakeMethod(writer);
        writer.WriteLine();
        
        EmitAsyncAwakeMethod(writer);
        writer.WriteLine();
        
        EmitOnDestroyAsyncMethod(writer);
        writer.WriteLine();
    }

    private void EmitAwakeMethod(IndentedTextWriter writer) {
        writer.WriteLine($"private void Awake() {{");
        writer.Indent++;
        
        string containerTypeName = $"Container_{_model.Decl.ClassAsTypeRef.FlattenedNameNonArityBased}";
        
        writer.WriteLine($"var parent = global::{typeof(SurjectRuntime).FullName}.GetRootResolver();");
        writer.WriteLine($"{EmitConstants.KContainerFieldName} = new {containerTypeName}();");
        writer.WriteLine(
            $"global::{typeof(SurjectRuntime).FullName}.RegisterResolver<{_model.Decl.ClassAsTypeRef.FQNConstructedArgBased}>({EmitConstants.KResolverPropertyName});"
        );
        writer.WriteLine();

        writer.WriteMultiline(
            $$"""
            var targets =
                global::{{typeof(SurjectExtensions).FullName}}
                    .PreformTraversalWithBoundary<global::{{typeof(IInjectable).FullName}}, global::{{typeof(ScopeContext).FullName}}>(
                        this.gameObject,
                        static (global::UnityEngine.GameObject go) =>
                            global::UnityEngine.Object.FindObjectsByType<global::{{typeof(IInjectable).FullName}}>(
                                global::UnityEngine.FindObjectsSortMode.None
                            )
                    );
            """
        );

        writer.WriteLine();
        writer.WriteLine($"foreach (var mb in targets) {{");
        writer.Indent++;
        writer.WriteLine($"global::{typeof(SurjectRuntime).FullName}.InjectMonoBehaviour(mb, __container.Resolver);");
        writer.Indent--;
        writer.WriteLine("}");
        
        writer.Indent--;
        writer.WriteLine("}");
    }

    // TODO
    private void EmitAsyncAwakeMethod(IndentedTextWriter writer) {
        
    }

    private void EmitOnDestroyAsyncMethod(IndentedTextWriter writer) {
        writer.WriteLine($"private async global::UnityEngine.Awaitable OnDestroyAsync() {{");
        writer.Indent++;
        
        writer.WriteLine($"global::{typeof(SurjectRuntime).FullName}.UnregisterResolver<{_model.Decl.ClassAsTypeRef.FQNConstructedArgBased}>()");
        writer.WriteLine($"if (__container is global::System.IAsyncDisposable ad) await ad.DisposeAsync();");
        writer.WriteLine($"else __container?.Dispose();");
        
        writer.Indent--;
        writer.WriteLine("}");
    }
}