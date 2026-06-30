using System.CodeDom.Compiler;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis.Text;
using Surject.Abstractions.Resolutions;
using Surject.Generators.Models.Concepts;
using static Surject.Generators.Emitters.EmitConstants;
using GeneratedSource = (string name, Microsoft.CodeAnalysis.Text.SourceText sourceText);


namespace Surject.Generators.Emitters.Container;

internal static class ContainerEmitter {
    internal static GeneratedSource Emit(ContainerModel container, InjectionLinkage linkage) {
        using StringWriter sr = new();
        using IndentedTextWriter writer = new(sr);
        
        EmitHelpers.EmitGeneratedFileHeader(writer);
        
        // In same namespace for now
        if (container.Decl.AsTypeRef.Namespace is not null) {
            writer.WriteLine($"namespace {container.Decl.AsTypeRef.Namespace} {{");
            writer.Indent++;
        }

        EmitHelpers.EmitGeneratedClassAttributes(writer);
        writer.WriteLine($"internal sealed class {BuildContainerTypeName(container)} : {FQN(typeof(IContainer))} {{");
        writer.Indent++;
        
        new ContainerFieldEmitter(container, linkage).Emit(writer); 
        
        // closes class
        writer.Indent--;
        writer.WriteLine("}");
        
        if (container.Decl.AsTypeRef.Namespace is not null) {
            writer.Indent--;
            writer.WriteLine("}");
        }
        
        SourceText text = SourceText.From(sr.ToString(), Encoding.UTF8);
        return ($"{container.Decl.AsTypeRef.FlattenedNameNonArityBased}.g.cs", text);
    }
}

internal readonly struct ContainerDisposalEmitter { }