using System.CodeDom.Compiler;
using Surject.Abstractions.Resolutions;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;
using static Surject.Generators.Emitters.BuildHelpers;

namespace Surject.Generators.Emitters.Scopes.Outer;

internal readonly ref struct SubScopeAmbientOuterClassEmitter : IChainedEmitter {
    private readonly ContainerModel _model;
    
    internal SubScopeAmbientOuterClassEmitter(ContainerModel model) => _model = model;
    
    public void Emit(IndentedTextWriter writer) {
        EmitHelpers.EmitGeneratedCodeAttribute(writer);
        EmitHelpers.EmitExcludeFromCodeCoverageAttribute(writer);
        EmitHelpers.EmitTypeDeclarationFromModel(_model.Decl, writer);
        writer.Indent++;
        
        EmitMembers(writer);
        EmitBeginScope(writer);
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

    private void EmitBeginScope(IndentedTextWriter writer) {
        EmitHelpers.EmitEditorBrowsableNeverAttribute(writer);
        writer.WriteMultiline(
            $$"""
              public void BeginScope(global::{{typeof(IResolver)}} parent, 
              """
        );
    }
}