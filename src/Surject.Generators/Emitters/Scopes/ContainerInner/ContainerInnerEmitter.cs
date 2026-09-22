using System.CodeDom.Compiler;
using System.ComponentModel;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis.Text;
using Surject.Generators.Emitters.Helpers;
using Surject.Generators.Emitters.Scopes.ContainerInner.NoOpenGeneric;
using Surject.Generators.Models.Concepts;
using GeneratedSource = (string name, Microsoft.CodeAnalysis.Text.SourceText sourceText);
using IContainer = Surject.Abstractions.Resolutions.IContainer;

namespace Surject.Generators.Emitters.Scopes.ContainerInner;

internal static class ContainerInnerEmitter {
    internal static GeneratedSource EmitNoOpenGenerics(ContainerModel model) {
        using StringWriter sr = new();
        using IndentedTextWriter writer = new IndentedTextWriter(sr);
        
        EmitHelpers.EmitGeneratedFileHeader(writer);
        writer.WriteLine();
        
        if (model.Decl.AsTypeRef.Namespace is not null) {
            writer.WriteLine($"namespace {model.Decl.AsTypeRef.Namespace} {{");
            writer.Indent++;
        }
        
        EmitHelpers.EmitTypeDeclarationFromModel(model.Decl, writer);
        writer.Indent++;
        
        EmitHelpers.EmitGeneratedCodeAttribute(writer);
        EmitHelpers.EmitExcludeFromCodeCoverageAttribute(writer);
        EmitHelpers.EmitEditorBrowsableNeverAttribute(writer);
        writer.WriteLine($"private sealed partial class {BuildHelpers.BuildContainerType(model.Decl.AsTypeRef)} : global::{typeof(IContainer).FullName} {{");
        writer.Indent++;
        
        new ContainerInternalsNoOpenGenericEmitter(model).Emit(writer);
        
        writer.Indent--;
        writer.WriteLine("}");
        
        if (model.Decl.AsTypeRef.Namespace is not null) {
            writer.Indent--;
            writer.WriteLine("}");
        }
        
        SourceText text = SourceText.From(sr.ToString(), Encoding.UTF8);
        return ($"{model.Decl.AsTypeRef.FlattenedNameArityBased}_Container_NoGeneric.g.cs", text);
    }

    internal static GeneratedSource EmitOpenGeneric(ContainerModel model, OpenGenericInjectionLinkage linkage) {
        using StringWriter sr = new();
        using IndentedTextWriter writer = new IndentedTextWriter(sr);
        
        EmitHelpers.EmitGeneratedFileHeader(writer);
        writer.WriteLine();
        
        if (model.Decl.AsTypeRef.Namespace is not null) {
            writer.WriteLine($"namespace {model.Decl.AsTypeRef.Namespace} {{");
            writer.Indent++;
        }
        
        EmitHelpers.EmitTypeDeclarationFromModel(model.Decl, writer);
        writer.Indent++;
        
        EmitHelpers.EmitGeneratedCodeAttribute(writer);
        EmitHelpers.EmitExcludeFromCodeCoverageAttribute(writer);
        EmitHelpers.EmitEditorBrowsableNeverAttribute(writer);
        writer.WriteLine($"private sealed partial class {BuildHelpers.BuildContainerType(model.Decl.AsTypeRef)} : global::{typeof(IContainer).FullName} {{");
        writer.Indent++;
        
        new ContainerInternalsOpenGenericEmitter(model, linkage).Emit(writer);
        
        writer.Indent--;
        writer.WriteLine("}");

        if (model.Decl.AsTypeRef.Namespace is not null) {
            writer.Indent--;
            writer.WriteLine("}");
        }
        
        SourceText text = SourceText.From(sr.ToString(), Encoding.UTF8);
        return ($"{model.Decl.AsTypeRef.FlattenedNameArityBased}_Container_Generic.g.cs", text);
    }
}