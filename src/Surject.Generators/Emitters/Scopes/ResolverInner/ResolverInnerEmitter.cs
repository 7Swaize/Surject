using System.CodeDom.Compiler;
using Surject.Abstractions.Resolutions;
using Surject.Generators.Emitters.Helpers;
using Surject.Generators.Models.Concepts;
using GeneratedSource = (string name, Microsoft.CodeAnalysis.Text.SourceText sourceText);

namespace Surject.Generators.Emitters.Scopes.ResolverInner;

internal static class ResolverInnerEmitter {
    internal static GeneratedSource EmitResolver(ContainerModel container, OpenGenericInjectionLinkage linkage) {
        return EmitHelpers.EmitFile(
            container.Decl.AsTypeRef.Namespace,
            $"{container.Decl.AsTypeRef.FlattenedNameArityBased}_Resolver.g.cs",
            writer => EmitResolverClass(container, linkage, writer)
        );
    }

    private static void EmitResolverClass(
        ContainerModel container,
        OpenGenericInjectionLinkage linkage,
        IndentedTextWriter writer)
    {
        EmitHelpers.EmitTypeDeclarationFromModel(container.Decl, writer);
        writer.Indent++;
        
        EmitHelpers.EmitGeneratedCodeAttribute(writer);
        EmitHelpers.EmitExcludeFromCodeCoverageAttribute(writer);
        EmitHelpers.EmitEditorBrowsableNeverAttribute(writer);
        writer.WriteLine($"private sealed partial class {BuildHelpers.BuildContainerType(container.Decl.AsTypeRef)} : global::{typeof(IResolver).FullName} {{");
        writer.Indent++;
        
        new ResolverInternalsEmitterCore(container, linkage).Emit(writer);
        
        writer.Indent--;
        writer.WriteLine("}"); // closes container

        writer.Indent--;
        writer.WriteLine("}"); // closes scope outer
    } 
}