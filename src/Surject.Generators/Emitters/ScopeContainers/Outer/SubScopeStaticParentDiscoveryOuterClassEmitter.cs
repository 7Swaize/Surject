using System.CodeDom.Compiler;
using Surject.Abstractions.Lifecycle;
using Surject.Abstractions.Registrations;
using Surject.Abstractions.Resolutions;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;
using Surject.Unity;
using static Surject.Generators.Emitters.BuildHelpers;

namespace Surject.Generators.Emitters.ScopeContainers.Outer;

internal readonly ref struct SubScopeStaticParentDiscoveryOuterClassEmitter : IChainedEmitter {
    private readonly ContainerModel _model;
    
    internal SubScopeStaticParentDiscoveryOuterClassEmitter(ContainerModel model) => _model = model;
    
    public void Emit(IndentedTextWriter writer) {
        EmitHelpers.EmitDefaultExecutionOrderAttribute(writer, SurjectExecutionOrder.SubScopeStaticRegistration);
        EmitHelpers.EmitGeneratedCodeAttribute(writer);
        EmitHelpers.EmitExcludeFromCodeCoverageAttribute(writer);
        EmitHelpers.EmitTypeDeclarationFromModel(_model.Decl, writer);
        writer.Indent++;

        EmitMembers(writer);
        EmitAwake(writer);
        EmitOnDestroy(writer);
        
        writer.Indent--;
        writer.WriteLine("}");
    }

    private void EmitMembers(IndentedTextWriter writer) {
        ITypeReferenceModel modelType = _model.Decl.AsTypeRef;
        
        EmitHelpers.EmitEditorBrowsableNeverAttribute(writer);
        writer.WriteLine($"private {BuildContainerType(modelType)} __container;");
        writer.WriteLine();
        
        EmitHelpers.EmitEditorBrowsableNeverAttribute(writer);
        writer.WriteLine($"public {BuildResolverType(modelType)} Resolver => this.__container.Resolver;");
        writer.WriteLine();
    }

    private void EmitAwake(IndentedTextWriter writer) {
        EmitHelpers.EmitEditorBrowsableNeverAttribute(writer);
        writer.WriteLine($"private void Awake() {{");
        writer.Indent++;
        
        ITypeReferenceModel modelType = _model.Decl.AsTypeRef;
        ITypeReferenceModel targetStaticScopeType = _model.ProvidedParentScope!;
        
        writer.WriteMultiline(
            $$"""
              {{typeof(SurjectRuntime).FullName}}.{{nameof(SurjectRuntime.Instance)}}.{{nameof(SurjectRuntime.QueueStaticScopedResolverContinuation)}}<
                  {{BuildResolverType(modelType)}}, 
                  {{BuildResolverTypeFQN(targetStaticScopeType)}},
              >(
                  static ({{typeof(IResolver).FullName}} parent) => {
                      this.__container = new {{BuildContainerType(modelType)}}(parent, this);
                      
                      var targetsToInject = global::{{typeof(SurjectExtensions).FullName}}
                          .{{nameof(SurjectExtensions.PreformTraversalWithBoundary)}}<
                              global::{{typeof(IInjectable).FullName}}, 
                              global::{{typeof(ScopeContext).FullName}}
                          >(
                              this.gameObject,
                              static (global::UnityEngine.GameObject go) =>
                                  go.GetComponentsInChildren<global::{{typeof(IInjectable).FullName}}>()
                          );
                              
                      foreach (var target in targetsToInject) {
                          target.{{nameof(IInjectable.__Surject_Inject)}}(this.__container.Resolver);
                      }
                      
                      return this.__container.Resolver;
                  }
              );
              """
        );

        writer.Indent--;
        writer.WriteLine("}");
    }
    
    private void EmitOnDestroy(IndentedTextWriter writer) {
        EmitHelpers.EmitEditorBrowsableNeverAttribute(writer);
        writer.WriteLine($"private async global::UnityEngine.Awaitable OnDestroy() {{");
        writer.Indent++;
        
        ITypeReferenceModel modelType = _model.Decl.AsTypeRef;
        
        writer.WriteLine(
            $$"""
              {{typeof(SurjectRuntime).FullName}}.{{nameof(SurjectRuntime.Instance)}}.{{nameof(SurjectRuntime.UnregisterStaticScopedResolver)}}<
                  {{BuildResolverTypeFQN(modelType)}},
              >(this.__container.Resolver);    
              """
        );
        writer.WriteLine($"await this.__container.DisposeAsync();");
        
        writer.Indent--;
        writer.WriteLine("}");
    }
}