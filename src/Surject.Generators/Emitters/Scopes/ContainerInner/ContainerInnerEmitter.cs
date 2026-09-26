using System;
using System.CodeDom.Compiler;
using Surject.Abstractions.Resolutions;
using Surject.Generators.Emitters.Helpers;
using Surject.Generators.Emitters.Scopes.ContainerInner.NoOpenGeneric;
using Surject.Generators.Emitters.Scopes.ContainerInner.OpenGeneric;
using Surject.Generators.Models.Concepts;
using GeneratedSource = (string name, Microsoft.CodeAnalysis.Text.SourceText sourceText);

namespace Surject.Generators.Emitters.Scopes.ContainerInner;

internal static class ContainerInnerEmitter {
    internal static GeneratedSource EmitNoOpenGenerics(ContainerModel model) {
        return EmitHelpers.EmitFile(
            model.Decl.AsTypeRef.Namespace,
            $"{model.Decl.AsTypeRef.FlattenedNameArityBased}_Container_NoGeneric.g.cs",
            writer => EmitContainerClass(model, writer, static (m, w) => new ContainerInternalsNoOpenGenericEmitter(m).Emit(w))
        );
    }

    internal static GeneratedSource EmitOpenGeneric(ContainerModel model, OpenGenericInjectionLinkage linkage) {
        return EmitHelpers.EmitFile(
            model.Decl.AsTypeRef.Namespace,
            $"{model.Decl.AsTypeRef.FlattenedNameArityBased}_Container_Generic.g.cs",
            writer => EmitContainerClass(model, writer, (m, w) => new ContainerInternalsOpenGenericEmitter(m, linkage).Emit(w))
        );
    }
    
    private static void EmitContainerClass(
        ContainerModel model,
        IndentedTextWriter writer,
        Action<ContainerModel, IndentedTextWriter> internalsEmitter)
    {
        EmitHelpers.EmitTypeDeclarationFromModel(model.Decl, writer);
        writer.Indent++;

        EmitHelpers.EmitGeneratedCodeAttribute(writer);
        EmitHelpers.EmitExcludeFromCodeCoverageAttribute(writer);
        EmitHelpers.EmitEditorBrowsableNeverAttribute(writer);
        writer.WriteLine($"private sealed partial class {BuildHelpers.BuildContainerType(model.Decl.AsTypeRef)} : global::{typeof(IContainer).FullName} {{");
        writer.Indent++;

        internalsEmitter(model, writer);

        writer.Indent--;
        writer.WriteLine("}"); // closes container

        writer.Indent--;
        writer.WriteLine("}"); // closes scope outer
    }
}