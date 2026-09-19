using System.CodeDom.Compiler;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis.Text;
using Surject.Abstractions.Lifecycle;
using Surject.Generators.Models.Concepts;
using GeneratedSource = (string name, Microsoft.CodeAnalysis.Text.SourceText sourceText);

namespace Surject.Generators.Emitters.InjectableContainers;

internal static class InjectableContainerEmitter {
    private static readonly string[] KInheritanceToAddInDecl = [
        $"global::{typeof(IInjectable).FullName!}"
    ];
    
    internal static GeneratedSource Emit(InjectableContainerModel model) {
        using StringWriter sr = new();
        using IndentedTextWriter writer = new(sr);
        
        EmitHelpers.EmitGeneratedFileHeader(writer);
        writer.WriteLine();

        if (model.Decl.AsTypeRef.Namespace is not null) {
            writer.WriteLine($"namespace {model.Decl.AsTypeRef.Namespace} {{");
            writer.Indent++;
        }
        
        EmitHelpers.EmitGeneratedCodeAttribute(writer);
        EmitHelpers.EmitExcludeFromCodeCoverageAttribute(writer);
        EmitHelpers.EmitTypeDeclarationFromModel(model.Decl, writer, KInheritanceToAddInDecl);
        writer.Indent++;
        writer.WriteLine();
        
        new InjectMethodEmitter(model).Emit(writer);
        
        // closes class
        writer.Indent--;
        writer.WriteLine("}");
        
        if (model.Decl.AsTypeRef.Namespace is not null) {
            writer.Indent--;
            writer.WriteLine("}");
        }
        
        SourceText text = SourceText.From(sr.ToString(), Encoding.UTF8);
        return ($"{model.Decl.AsTypeRef.FlattenedNameArityBased}_Injection.g.cs", text);
    }
}