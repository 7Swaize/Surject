using System.CodeDom.Compiler;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;
using Surject.Unity;
using static Surject.Generators.Emitters.BuildHelpers;

namespace Surject.Generators.Emitters.ScopeContainers.Outer;

internal readonly ref struct SceneRootScopeOuterClassEmitter : IChainedEmitter {
    private readonly ContainerModel _model;
    
    internal SceneRootScopeOuterClassEmitter(ContainerModel model) => _model = model;
    
    public void Emit(IndentedTextWriter writer) {
        EmitHelpers.EmitDefaultExecutionOrderAttribute(writer, SurjectExecutionOrder.SceneRoot);
        EmitHelpers.EmitGeneratedCodeAttribute(writer);
        EmitHelpers.EmitExcludeFromCodeCoverageAttribute(writer);
        EmitHelpers.EmitTypeDeclarationFromModel(_model.Decl, writer);
        writer.Indent++;
        
        EmitMembers(writer);
        EmitAwake(writer);
        
        writer.Indent--;
        writer.WriteLine("}");
    }
    
    private void EmitMembers(IndentedTextWriter writer) {
        ITypeReferenceModel modelType = _model.Decl.AsTypeRef;
        
        EmitHelpers.EmitEditorBrowsableNeverAttribute(writer);
        writer.WriteLine($"private {BuildContainerType(modelType)} __container");
        writer.WriteLine();
        
        EmitHelpers.EmitEditorBrowsableNeverAttribute(writer);
        writer.WriteLine($"public {BuildResolverType(modelType)} Resolver => this.__container.Resolver");
        writer.WriteLine();
    } 

    private void EmitAwake(IndentedTextWriter writer) {
        EmitHelpers.EmitEditorBrowsableNeverAttribute(writer);
        writer.WriteLine($"private void Awake {{");
        writer.Indent++;
        
        ITypeReferenceModel modelType = _model.Decl.AsTypeRef;
        
        writer.WriteLine($"var parent = global::{typeof(SurjectRuntime)}.{nameof(SurjectRuntime.Instance)}.{nameof(SurjectRuntime.RootResolver)}();");
        writer.WriteLine($"this.__container = new {BuildContainerType(modelType)}(parent, this);");
        writer.WriteLine($"var scene = global::UnityEngine.SceneManagement.SceneManager.GetActiveScene();");
        writer.WriteLine($"global::{typeof(SurjectRuntime).FullName}.{nameof(SurjectRuntime.Instance)}.{nameof(SurjectRuntime.RegisterSceneResolver)}(scene, this);");
        writer.WriteLine();
        
        writer.WriteMultiline(
            $$"""
                  
              """
        );
        
        writer.Indent--;
        writer.WriteLine("}");
        writer.WriteLine();
    }

    private static void EmitOnDestroy(IndentedTextWriter writer) {
        writer.WriteLine($"private async Awaitable OnDestroy() {{");
        writer.Indent++;
        
        writer.WriteLine($"global::{typeof(SurjectRuntime)}.{nameof(SurjectRuntime.Instance)}.{nameof(SurjectRuntime.UnregisterSceneResolver)}(");
        writer.Indent++;
        writer.WriteLine("global::UnityEngine.SceneManagement.SceneManager.GetActiveScene()");
        writer.Indent--;
        writer.WriteLine(")");
        writer.WriteLine();
        
        writer.WriteLine("await this.__container.DisposeAsync();");
        
        writer.Indent--;
        writer.WriteLine("}");
    }
}