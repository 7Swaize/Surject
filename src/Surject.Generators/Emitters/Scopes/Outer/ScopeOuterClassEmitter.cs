using System.CodeDom.Compiler;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis.Text;
using Surject.Generators.Emitters.Helpers;
using Surject.Generators.Models.Concepts;
using Surject.Shared.Helpers;
using GeneratedSource = (string name, Microsoft.CodeAnalysis.Text.SourceText sourceText);

namespace Surject.Generators.Emitters.Scopes.Outer;

internal static class ScopeOuterClassEmitter {
    internal static GeneratedSource Emit(ContainerModel model) {
        return EmitHelpers.EmitFile(
            model.Decl.AsTypeRef.Namespace,
            $"{model.Decl.AsTypeRef.FlattenedNameArityBased}_Scope.g.cs",
            writer => EmitClass(model, writer)
        );
    }

    private static void EmitClass(ContainerModel model, IndentedTextWriter writer) {
        switch (model.ContainerKind) {
            case ContainerKind.SubScope:
                switch (model.ParentDiscoveryKind) {
                    case ParentDiscoveryKind.Hierarchy:
                        new SubScopeRuntimeHierarchyParentDiscoveryOuterClassEmitter(model).Emit(writer);
                        break;
                    case ParentDiscoveryKind.Static:
                        new SubScopeStaticParentDiscoveryOuterClassEmitter(model).Emit(writer);
                        break;
                    case ParentDiscoveryKind.Ambient:
                        new SubScopeAmbientOuterClassEmitter(model).Emit(writer);
                        break;
                }
                break;
            case ContainerKind.SceneRoot:
                new SceneRootScopeOuterClassEmitter(model).Emit(writer);
                break;
            case ContainerKind.ApplicationRoot:
                new ApplicationRootScopeOuterClassEmitter(model).Emit(writer);
                break;
            default:
                ThrowHelpers.ThrowUnhandledBranch<ContainerKind>(model.ContainerKind);
                break;
        }
    }
}