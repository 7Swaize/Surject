using Surject.Generators.Models.Primitives;

namespace Surject.Generators.Emitters;

internal static class BuildHelpers {
    internal static string BuildResolverType(ITypeReferenceModel containingType) {
        return $"Resolver_{containingType.FlattenedNameArityBased}";
    }

    internal static string BuildResolverTypeFQN(ITypeReferenceModel containingType) {
        return $"global::{containingType.Namespace}.Resolver{containingType.FlattenedNameArityBased}";
    }

    internal static string BuildContainerType(ITypeReferenceModel containingType) {
        return $"Container_{containingType.FlattenedNameArityBased}";
    }

    internal static string BuildContainerTypeFQN(ITypeReferenceModel containingType) {
        return $"global::{containingType.Namespace}.Container_{containingType.FlattenedNameArityBased}";
    }
}