using Microsoft.CodeAnalysis;
using Surject.Abstractions.Attributes;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;
using Surject.Shared.Helpers;

namespace Surject.Generators.Discovery.ServiceRegistration;

internal static class ContainerParser {
    internal static ITypeReferenceModel? GetParentScopeOverride(in GeneratorAttributeSyntaxContext context, DiscoveryUtils utils) {
        INamedTypeSymbol? target =
            context.SemanticModel.Compilation.GetTypeByMetadataName(typeof(ScopeAttribute).FullName!);

        foreach (AttributeData attr in context.Attributes) {
            if (!SymbolEqualityComparer.Default.Equals(attr.AttributeClass, target)) {
                continue;
            }

            if (attr.ConstructorArguments.Length > 1) {
                return utils.TypeReferenceModelFactory.CreateOrGetTypeReferenceModel(
                    (ITypeSymbol)attr.ConstructorArguments[1].Value!
                );
            } 
        }
        
        return null;
    }

    internal static ScopeLevelKind GetScopeLevelKind(in GeneratorAttributeSyntaxContext context) {
        INamedTypeSymbol? target =
            context.SemanticModel.Compilation.GetTypeByMetadataName(typeof(ScopeAttribute).FullName!);
        
        foreach (AttributeData attr in context.Attributes) {
            if (!SymbolEqualityComparer.Default.Equals(attr.AttributeClass, target)) {
                continue;
            }

            return (ScopeLevelKind)(byte)attr.ConstructorArguments[0].Value!;
        }
        
        ThrowHelpers.ThrowUnreachable();
        return default;
    }
}