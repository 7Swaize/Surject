using Microsoft.CodeAnalysis;
using Surject.Abstractions.Attributes;
using Surject.Generators.Models.Primitives;

namespace Surject.Generators.Discovery.ServiceRegistration;

internal static class ContainerParser {
    internal static ITypeReferenceModel? TryGetParentScopeOverride(in GeneratorAttributeSyntaxContext context, DiscoveryUtils utils) {
        INamedTypeSymbol? target =
            context.SemanticModel.Compilation.GetTypeByMetadataName(typeof(ScopeAttribute).FullName!);

        foreach (AttributeData attr in context.Attributes) {
            if (!SymbolEqualityComparer.Default.Equals(attr.AttributeClass, target)) {
                continue;
            }

            if (attr.ConstructorArguments.Length > 0) {
                return utils.TypeReferenceModelFactory.CreateOrGetTypeReferenceModel(
                    (ITypeSymbol)attr.ConstructorArguments[0].Value!
                );
            } 
        }
        
        
        return null;
    }
}