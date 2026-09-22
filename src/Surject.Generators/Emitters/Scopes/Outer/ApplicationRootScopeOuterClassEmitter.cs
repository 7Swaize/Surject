using System.CodeDom.Compiler;
using Surject.Generators.Emitters.Helpers;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;
using Surject.Unity;

namespace Surject.Generators.Emitters.Scopes.Outer;

internal readonly ref struct ApplicationRootScopeOuterClassEmitter : IChainedEmitter {
    private readonly ContainerModel _model;

    internal ApplicationRootScopeOuterClassEmitter(ContainerModel model) => _model = model;

    public void Emit(IndentedTextWriter writer) {
        EmitHelpers.EmitDefaultExecutionOrderAttribute(writer, SurjectExecutionOrder.ApplicationRoot);
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
        writer.WriteLine($"private {BuildHelpers.BuildContainerType(modelType)} __container;");
        writer.WriteLine();
        
        EmitHelpers.EmitEditorBrowsableNeverAttribute(writer);
        writer.WriteLine($"public {BuildHelpers.BuildResolverType(modelType)} Resolver => this.__container.Resolver;");
        writer.WriteLine();
    } 

    private void EmitAwake(IndentedTextWriter writer) {
        EmitHelpers.EmitEditorBrowsableNeverAttribute(writer);
        writer.WriteLine($"private void Awake() {{");
        writer.Indent++;
        
        ITypeReferenceModel modelType = _model.Decl.AsTypeRef;
        
        writer.WriteLine($"global::UnityEngine.Object.DontDestroyOnLoad(this);");
        writer.WriteLine($"this.__container = new {BuildHelpers.BuildContainerType(modelType)}(this);");
        writer.WriteLine($"global::{typeof(SurjectRuntime).FullName}.{nameof(SurjectRuntime.Instance)}.{nameof(SurjectRuntime.RegisterRootResolver)}(this.__container.Resolver);");

        writer.Indent--;
        writer.WriteLine("}");
        writer.WriteLine();
    }

    private void EmitOnDestroy(IndentedTextWriter writer) {
        EmitHelpers.EmitEditorBrowsableNeverAttribute(writer);
        writer.WriteLine($"private async global::UnityEngine.Awaitable OnDestroy() {{");
        writer.Indent++;
        
        writer.WriteLine($"global::{typeof(SurjectRuntime).FullName}.{nameof(SurjectRuntime.Instance)}.{nameof(SurjectRuntime.UnregisterRootResolver)}();");
        writer.WriteLine($"await this.__container.DisposeAsync();");
        
        writer.Indent--;
        writer.WriteLine("}");
    }
}