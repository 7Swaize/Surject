using System.CodeDom.Compiler;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis.Text;
using Surject.Generators.Models.Concepts;
using GeneratedSource = (string name, Microsoft.CodeAnalysis.Text.SourceText sourceText);


namespace Surject.Generators.Emitters.Container;

internal static class ContainerEmitter {
    internal static GeneratedSource Emit(ContainerModel container, InjectionLinkage linkage) {
        using StringWriter sr = new();
        using IndentedTextWriter writer = new(sr);
        
        EmitHelpers.EmitGeneratedFileHeader(writer);
        
        // In same namespace for now
        if (container.Decl.ClassAsTypeRef.Namespace is not null) {
            writer.WriteLine($"namespace {container.Decl.ClassAsTypeRef.Namespace} {{");
            writer.Indent++;
        }
        
        // TODO
        
        // closes class
        writer.Indent--;
        writer.WriteLine("}");
        
        if (container.Decl.ClassAsTypeRef.Namespace is not null) {
            writer.Indent--;
            writer.WriteLine("}");
        }
        
        SourceText text = SourceText.From(sr.ToString(), Encoding.UTF8);
        return ($"{container.Decl.ClassAsTypeRef.FlattenedNameNonArityBased}.g.cs", text);
    }
}