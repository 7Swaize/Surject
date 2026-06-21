using System.CodeDom.Compiler;
using Surject.Generators.Models.Concepts;
using Surject.Unity;

namespace Surject.Generators.Emitters.Scope;

internal readonly struct ApplicationScopeBodyEmitter {
    private readonly ContainerModel _model;

    internal ApplicationScopeBodyEmitter(ContainerModel model) {
        _model = model;
    }

    internal void Emit(IndentedTextWriter writer) {
        EmitAwakeMethod(writer);
        writer.WriteLine();
        
        EmitAsyncAwakeMethod(writer);
        writer.WriteLine();
        
        EmitOnDestroyAsyncMethod(writer);
        
    }

    private void EmitAwakeMethod(IndentedTextWriter writer) {
        EmitHelpers.EmitEditorBrowsableNeverAttribute(writer);
        writer.WriteLine($"private void Awake() {{");
        writer.Indent++;
        
        string containerTypeName = $"Container_{_model.Decl.ClassAsTypeRef.FlattenedNameNonArityBased}";
        
        writer.WriteLine($"DontDestroyOnLoad(gameObject);");
        writer.WriteLine($"{EmitConstants.KContainerFieldName} = new {containerTypeName}();");
        writer.WriteLine($"{TypeNames.FQN(typeof(SurjectRuntime))}.RegisterRootResolver({EmitConstants.KResolverPropertyName});");
        
        writer.Indent--;
        writer.WriteLine("}");
    }

    private void EmitAsyncAwakeMethod(IndentedTextWriter writer) {
        EmitHelpers.EmitEditorBrowsableNeverAttribute(writer);
        writer.WriteLine($"private async global::UnityEngine.Awaitable AwakeAsync() {{");
        writer.Indent++;
        
        string containerTypeName = $"Container_{_model.Decl.ClassAsTypeRef.FlattenedNameNonArityBased}";
        
        writer.WriteLine($"DontDestroyOnLoad(gameObject);");
        writer.WriteLine(
            $"{EmitConstants.KContainerFieldName} = await {containerTypeName}.BuildAsync(destroyCancellationToken)"
        );
        writer.WriteLine($"{TypeNames.FQN(typeof(SurjectRuntime))}.RegisterRootResolver({EmitConstants.KResolverPropertyName});");
        
        writer.Indent--;
        writer.WriteLine("}");
    }

    private void EmitOnDestroyAsyncMethod(IndentedTextWriter writer) {
        EmitHelpers.EmitEditorBrowsableNeverAttribute(writer);
        writer.WriteLine($"private async global::UnityEngine.Awaitable OnDestroyAsync() {{");
        writer.Indent++;
        
        writer.WriteLine($"{TypeNames.FQN(typeof(SurjectRuntime))}.UnregisterRootResolver();");
        writer.WriteLine($"if (__container is global::System.IAsyncDisposable ad) await ad.DisposeAsync();");
        writer.WriteLine("else __container?.Dispose();");
        
        writer.Indent--;
        writer.WriteLine("}");
    }
}