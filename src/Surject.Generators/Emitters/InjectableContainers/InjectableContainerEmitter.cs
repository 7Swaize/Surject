using System.CodeDom.Compiler;
using Surject.Abstractions.Lifecycle;
using Surject.Generators.Emitters.Helpers;
using Surject.Generators.Models.Concepts;
using GeneratedSource = (string name, Microsoft.CodeAnalysis.Text.SourceText sourceText);

namespace Surject.Generators.Emitters.InjectableContainers;

internal static class InjectableContainerEmitter {
    private static readonly string[] KInheritanceToAddInDecl = [
        $"global::{typeof(IInjectable).FullName!}"
    ];

    internal static GeneratedSource Emit(InjectableContainerModel model) {
        return EmitHelpers.EmitFile(
            model.Decl.AsTypeRef.Namespace,
            $"{model.Decl.AsTypeRef.FlattenedNameArityBased}_Injection.g.cs",
            writer => EmitClass(model, writer)
        );
    }

    private static void EmitClass(InjectableContainerModel model, IndentedTextWriter writer) {
        EmitHelpers.EmitGeneratedCodeAttribute(writer);
        EmitHelpers.EmitExcludeFromCodeCoverageAttribute(writer);
        EmitHelpers.EmitTypeDeclarationFromModel(model.Decl, writer, KInheritanceToAddInDecl);
        writer.Indent++;
        writer.WriteLine();
        
        new InjectMethodEmitter(model).Emit(writer);
        
        writer.Indent--;
        writer.WriteLine("}"); // closes class
    }
}