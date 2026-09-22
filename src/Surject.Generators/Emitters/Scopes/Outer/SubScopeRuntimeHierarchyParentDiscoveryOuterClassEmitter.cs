using System.CodeDom.Compiler;
using Surject.Abstractions.Lifecycle;
using Surject.Abstractions.Registrations;
using Surject.Abstractions.Resolutions;
using Surject.Generators.Emitters.Helpers;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;
using Surject.Unity;

namespace Surject.Generators.Emitters.Scopes.Outer;

internal readonly ref struct SubScopeRuntimeHierarchyParentDiscoveryOuterClassEmitter : IChainedEmitter {
    private readonly ContainerModel _model;
    
    internal SubScopeRuntimeHierarchyParentDiscoveryOuterClassEmitter(ContainerModel model) => _model = model;
    
    public void Emit(IndentedTextWriter writer) {
        EmitHelpers.EmitDefaultExecutionOrderAttribute(writer, SurjectExecutionOrder.SubScopeRuntimeRegistration);
        EmitHelpers.EmitGeneratedCodeAttribute(writer);
        EmitHelpers.EmitExcludeFromCodeCoverageAttribute(writer);
        EmitHelpers.EmitTypeDeclarationFromModel(_model.Decl, writer);
        writer.Indent++;
        
        EmitMembers(writer);
        EmitAwake(writer);
        EmitDiscoverParentResolver(writer);
        EmitOnDestroy(writer);
    }
    
    private void EmitMembers(IndentedTextWriter writer) {
        ITypeReferenceModel modelType = _model.Decl.AsTypeRef;
        
        EmitHelpers.EmitEditorBrowsableNeverAttribute(writer);
        writer.WriteLine($"private {BuildHelpers.BuildContainerType(modelType)} __container;");
        writer.WriteLine();
        
        EmitHelpers.EmitEditorBrowsableNeverAttribute(writer);
        writer.WriteLine($"public {BuildHelpers.BuildResolverType(modelType)} Resolver => this.__container.Resolver;");
        writer.WriteLine();
    }

    private void EmitAwake(IndentedTextWriter writer) {
        EmitHelpers.EmitEditorBrowsableNeverAttribute(writer);
        writer.WriteLine($"private void Awake {{");
        writer.Indent++;
        
        ITypeReferenceModel modelType = _model.Decl.AsTypeRef;
        
        writer.WriteLine($"var parent = __DiscoverParentResolver();");
        writer.WriteLine($"this.__container = new {BuildHelpers.BuildContainerType(modelType)}(parent, this);");
        writer.WriteLine();
        
        writer.WriteLine(
            $"""
             var targetsToInject =
                 global::{typeof(SurjectExtensions).FullName}
                     .{nameof(SurjectExtensions.PreformTraversalWithBoundary)}<global::{typeof(IInjectable).FullName}, global::{typeof(ScopeContext).FullName}>(
                         this.gameObject,
                         static (global::UnityEngine.GameObject go) => 
                             go.GetComponentsInChildren<global::{typeof(IInjectable).FullName}>()
                     );
             """
        );
        writer.WriteLine();
        
        writer.WriteLine($"foreach (var target in targetsToInject) {{");
        writer.Indent++;
        writer.WriteLine($"target.{nameof(IInjectable.__Surject_Inject)}(this.__container.Resolver);");
    }

    private void EmitDiscoverParentResolver(IndentedTextWriter writer) {
        EmitHelpers.EmitEditorBrowsableNeverAttribute(writer);
        writer.WriteLine($"private global::{typeof(IResolver).FullName} __DiscoverParentResolver() {{");
        writer.Indent++;
        
        writer.WriteLine($"if (global::{typeof(SurjectRuntime).FullName}.{nameof(SurjectRuntime.Instance)}.{nameof(SurjectRuntime.TryConsumePendingParent)}(out var pushed) {{");
        writer.Indent++;
        writer.WriteLine("return pushed;");
        writer.Indent--;
        
        writer.WriteMultiline(
            $"""
             var parent = this.transform.parent != null
                 ? this.transform.parent.GetComponentInParent<global::{typeof(IResolver).FullName}>()
                 : null;
             """
        );
        writer.WriteLine();
        
        writer.WriteLine($"if (parent == null) {{");
        writer.Indent++;
        writer.WriteMultiline(
            $"""
             parent = global::{typeof(SurjectRuntime)}.{nameof(SurjectRuntime.Instance)}.{nameof(SurjectRuntime.GetSceneResolver)}(global::UnityEngine.SceneManagement.SceneManager.GetActiveScene())
                 ?? global::{typeof(SurjectRuntime).FullName}.{nameof(SurjectRuntime.Instance)}.{nameof(SurjectRuntime.RootResolver)}()!;
             """
        );
        writer.WriteLine();
        
        writer.WriteLine("return parent;");
        writer.Indent--;
        writer.WriteLine("}");
    }
    
    private void EmitOnDestroy(IndentedTextWriter writer) {
        EmitHelpers.EmitEditorBrowsableNeverAttribute(writer);
        writer.WriteLine($"private async global::UnityEngine.Awaitable OnDestroy() {{");
        writer.Indent++;
        
        writer.WriteLine($"await this.__container.DisposeAsync();");
        
        writer.Indent--;
        writer.WriteLine("}");
    }
}