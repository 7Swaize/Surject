using System.CodeDom.Compiler;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis.Text;
using Surject.Generators.Models.Concepts;
using GeneratedSource = (string name, Microsoft.CodeAnalysis.Text.SourceText sourceText);


namespace Surject.Generators.Emitters.Resolver;

internal static class ResolverEmitter {
    internal static GeneratedSource Emit(ContainerModel model) {
        using StringWriter sr = new();
        using IndentedTextWriter writer = new(sr);
        
        EmitHelpers.EmitGeneratedFileHeader(writer);
        
        // We will keep it in the same namespace
        if (model.Decl.ClassAsTypeRef.Namespace is not null) {
            writer.WriteLine($"namespace {model.Decl.ClassAsTypeRef.Namespace} {{");
            writer.Indent++;
        }
        
        
        // TODO
        
        
        // closes class
        writer.Indent--;
        writer.WriteLine("}");
        
        if (model.Decl.ClassAsTypeRef.Namespace is not null) {
            writer.Indent--;
            writer.WriteLine("}");
        }
        

        SourceText text = SourceText.From(sr.ToString(), Encoding.UTF8);
        return ($"{model.Decl.ClassAsTypeRef.FlattenedNameNonArityBased}_Resolver.g.cs", text);
    }
}