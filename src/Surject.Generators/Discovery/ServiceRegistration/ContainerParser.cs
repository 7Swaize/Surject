using Microsoft.CodeAnalysis;
using Surject.Abstractions.Attributes;
using Surject.Generators.Models.Concepts;
using Surject.Shared.Helpers;

namespace Surject.Generators.Discovery.ServiceRegistration;

internal static class ContainerParser {
    internal static ScopeLevelKind GetScopeLevelKind(in GeneratorAttributeSyntaxContext ctx) {
        INamedTypeSymbol? targetAttr =
            ctx.SemanticModel.Compilation.GetTypeByMetadataName(typeof(ScopeAttribute).FullName!);

        foreach (AttributeData attr in ctx.Attributes) {
            if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, targetAttr)) {
                return (ScopeLevelKind)(byte)attr.ConstructorArguments[0].Value!;
            }
        }
        
        ThrowHelpers.ThrowUnreachable();
        return default;
    }
}