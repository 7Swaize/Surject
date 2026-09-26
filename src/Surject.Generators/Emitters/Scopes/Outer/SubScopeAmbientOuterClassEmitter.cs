using System.CodeDom.Compiler;
using System.Linq;
using Surject.Abstractions.Resolutions;
using Surject.Generators.Emitters.Helpers;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;
using Surject.Unity.Utility.Exceptions;

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
        writer.WriteLine($"private {BuildHelpers.BuildContainerType(modelType)} __container;");
        writer.WriteLine();
        
        EmitHelpers.EmitEditorBrowsableNeverAttribute(writer);
        writer.WriteLine($"public {BuildHelpers.BuildResolverType(modelType)} Resolver => this.__container.Resolver;");
        writer.WriteLine();
    }

    private void EmitBeginScope(IndentedTextWriter writer) {
        ITypeReferenceModel modelType = _model.Decl.AsTypeRef;
        ITypeReferenceModel[] ambientParams = [
            .. _model.Registrations
                .AsArrayUnsafe()
                .Where(r => r.Entry.Kind == EntryKind.AddAmbient)
                .Select(r => r.Entry.AuxType1)
        ];
        string formattedParams = string.Concat(ambientParams.Select((p, i) => $", {p.FQNConstructedArgBased} _ambient{i}"));
        
        EmitHelpers.EmitEditorBrowsableNeverAttribute(writer);
        writer.WriteMultiline(
            $$"""
              public void BeginScope(global::{{typeof(IResolver)}} parent{{formattedParams}}) {
                if (this.__container is not null)
                    throw new global::{{typeof(ThrowHelpers).FullName}}.{{nameof(ThrowHelpers.ThrowSurjectRuntimeException)}}($"'BeginScope' was called more than once");
              
              """
        );
    }

    private void AmbientParamNameBuilder(ITypeReferenceModel paramType) {
        
    }
}