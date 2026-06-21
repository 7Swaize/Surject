using System.CodeDom.Compiler;
using Surject.Abstractions.Lifecycle;
using Surject.Abstractions.Registrations;
using Surject.Generators.Models.Concepts;
using Surject.Unity;
using static Surject.Generators.Emitters.TypeNames;

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
        EmitHelpers.EmitEditorBrowsableNeverAttribute(writer);
        writer.WriteLine($"private void Awake() {{");
        writer.Indent++;
        
        writer.WriteLine($"var parent = {FQN(typeof(SurjectRuntime))}.GetRootResolver();");
        writer.WriteLine($"{EmitConstants.KContainerFieldName} = new {EmitConstants.BuildContainerTypeName(_model)}();");
        writer.WriteLine(
            $"{FQN(typeof(SurjectRuntime))}.RegisterSceneResolver(gameObject.scene);"
        );
        writer.WriteLine();

        writer.WriteMultiline(
            $$"""
            var targets =
                {{FQN(typeof(SurjectExtensions))}}
                    .PreformTraversalWithBoundary<{{FQN(typeof(IInjectable))}}, {{FQN(typeof(ScopeContext))}}>(
                        this.gameObject,
                        static (global::UnityEngine.GameObject go) =>
                            global::UnityEngine.Object.FindObjectsByType<{{FQN(typeof(IInjectable))}}>(
                                global::UnityEngine.FindObjectsSortMode.None
                            )
                    );
            """
        );

        writer.WriteLine();
        writer.WriteLine($"foreach (var mb in targets) {{");
        writer.Indent++;
        writer.WriteLine($"{FQN(typeof(SurjectRuntime))}.InjectMonoBehaviour(mb, __container.Resolver);");
        writer.Indent--;
        writer.WriteLine("}");
        
        writer.Indent--;
        writer.WriteLine("}");
    }

    // TODO
    private void EmitAsyncAwakeMethod(IndentedTextWriter writer) {
        
    }

    private void EmitOnDestroyAsyncMethod(IndentedTextWriter writer) {
        EmitHelpers.EmitEditorBrowsableNeverAttribute(writer);
        writer.WriteLine($"private async global::UnityEngine.Awaitable OnDestroyAsync() {{");
        writer.Indent++;
        
        writer.WriteLine(
            $"{FQN(typeof(SurjectRuntime))}.UnregisterSceneResolver(gameObject.scene));"
        );
        writer.WriteLine($"if (__container is global::System.IAsyncDisposable ad) await ad.DisposeAsync();");
        writer.WriteLine($"else __container?.Dispose();");
        
        writer.Indent--;
        writer.WriteLine("}");
    }
}