using System.CodeDom.Compiler;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis.Text;
using Surject.Generators.Emitters.Helpers;
using Surject.Generators.Models.Concepts;
using GeneratedSource = (string name, Microsoft.CodeAnalysis.Text.SourceText sourceText);

namespace Surject.Generators.Emitters.Container;

internal static class ContainerEmitter {
    internal static GeneratedSource Emit(ContainerModel model) {
        using StringWriter sr = new();
        using IndentedTextWriter writer = new(sr);
        
        SurjectHeaderEmitter.WriteGeneratedFileHeader(writer);

        SourceText text = SourceText.From(sr.ToString(), Encoding.UTF8);
        return ($"{model.ContainerDecl.ClassAsTypeRef.FlattenedNameNonArityBased}.g.cs", text);
    }
}
