using System.CodeDom.Compiler;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis.Text;
using Surject.Abstractions.Resolutions;
using Surject.Generators.Models.Concepts;
using Surject.Shared.Helpers;
using Surject.Unity;
using static Surject.Generators.Emitters.TypeNames;
using GeneratedSource = (string name, Microsoft.CodeAnalysis.Text.SourceText sourceText);

namespace Surject.Generators.Emitters.Scope;

internal static class ScopeEmitter {
    internal static GeneratedSource Emit(ContainerModel model) {
        using StringWriter sr = new();
        using IndentedTextWriter writer = new(sr);
        
        EmitHelpers.EmitGeneratedFileHeader(writer);
        
        if (model.Decl.ClassAsTypeRef.Namespace is not null) {
            writer.WriteLine($"namespace {model.Decl.ClassAsTypeRef.Namespace} {{");
            writer.Indent++;
        }
        
        EmitExecutionOrderAttribute(model, writer);
        EmitHelpers.EmitGeneratedClassAttributes(writer);
        EmitHelpers.EmitClassDeclarationFromModel(model.Decl, writer);
        
        EmitMembers(model, writer);

        switch (model.ScopeLevel) {
            case ScopeLevelKind.Application:
                new ApplicationScopeBodyEmitter(model).Emit(writer);
                break;
            case ScopeLevelKind.Scene:
                new SceneScopeBodyEmitter(model).Emit(writer);
                break;
            case ScopeLevelKind.GameObject:
                new GameObjectScopeBodyEmitter(model).Emit(writer);
                break;
            default:
                ThrowHelpers.ThrowUnhandledBranch(model.ScopeLevel);
                break;
        }
        
        // closes class
        writer.Indent--;
        writer.WriteLine("}");
        
        if (model.Decl.ClassAsTypeRef.Namespace is not null) {
            writer.Indent--;
            writer.WriteLine("}");
        }
        

        SourceText text = SourceText.From(sr.ToString(), Encoding.UTF8);
        return ($"{model.Decl.ClassAsTypeRef.FlattenedNameNonArityBased}.g.cs", text);
    }
    
    private static void EmitMembers(ContainerModel model, IndentedTextWriter writer) {
        string containerTypeName = $"Container_{model.Decl.ClassAsTypeRef.FlattenedNameNonArityBased}";
        
        writer.WriteLine($"private {containerTypeName} {EmitConstants.KContainerFieldName};");
        writer.WriteLine(
            $"{FQN(typeof(IResolver))} {EmitConstants.KResolverPropertyName}" +
            $" => {EmitConstants.KContainerFieldName}.{EmitConstants.KResolverPropertyName};"
        );
    }

    private static void EmitExecutionOrderAttribute(ContainerModel model, IndentedTextWriter writer) {
        switch (model.ScopeLevel) {
            case ScopeLevelKind.Application:
                EmitHelpers.EmitDefaultExecutionOrderAttribute(
                    $"{FQN(typeof(SurjectExecutionOrder))}.{nameof(SurjectExecutionOrder.KApplicationRoot)}",
                    writer
                );
                break;
            case ScopeLevelKind.Scene:
                EmitHelpers.EmitDefaultExecutionOrderAttribute(
                    $"{FQN(typeof(SurjectExecutionOrder))}.{nameof(SurjectExecutionOrder.KSceneScope)}",
                    writer
                );
                break;
            case ScopeLevelKind.GameObject:
                EmitHelpers.EmitDefaultExecutionOrderAttribute(
                    $"{FQN(typeof(SurjectExecutionOrder))}.{nameof(SurjectExecutionOrder.KGOScope)}",
                    writer
                );
                break;
            default:
                ThrowHelpers.ThrowUnhandledBranch(model.ScopeLevel);
                break;
        }
    }
}