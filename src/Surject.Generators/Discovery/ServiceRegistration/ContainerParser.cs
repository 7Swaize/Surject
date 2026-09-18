using Microsoft.CodeAnalysis;
using Surject.Abstractions.Attributes;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Factories;
using Surject.Generators.Models.Primitives;
using Surject.Shared.Helpers;

namespace Surject.Generators.Discovery.ServiceRegistration;

internal static class ContainerParser {
    internal static (ParentDiscoveryKind, ITypeReferenceModel?) ExtractFromScopeAttribute(
        in GeneratorAttributeSyntaxContext ctx,
        TypeReferenceModelFactory typeRefFactory)
    {
        INamedTypeSymbol? targetAttr =
            ctx.SemanticModel.Compilation.GetTypeByMetadataName(typeof(SubScopeAttribute).FullName!);

        foreach (AttributeData attr in ctx.Attributes) {
            if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, targetAttr)) {
                ITypeReferenceModel? model = attr.ConstructorArguments.Length > 1
                    ? typeRefFactory.CreateOrGetTypeReferenceModel((attr.ConstructorArguments[1].Value as ITypeSymbol)!)
                    : null;

                return ((ParentDiscoveryKind)(byte)attr.ConstructorArguments[0].Value!, model);
            }
        }
        
        ThrowHelpers.ThrowUnreachable();
        return default;
    }
}