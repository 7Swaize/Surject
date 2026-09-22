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

    internal static string BuildSingletonFieldNameNotKeyed(ITypeReferenceModel type) {
        return $"__s_{type.FlattenedNameArityBased}";
    }

    internal static string BuildSingletonFieldNameKeyed(ITypeReferenceModel type, string key) {
        return $"__s_{type.FlattenedNameArityBased}_{HashKey(key)}";
    }

    internal static string BuildTaskFieldNameNotKeyed(ITypeReferenceModel type) {
        return $"__s_{type.FlattenedNameArityBased}_task";
    }

    internal static string BuildTaskFieldNameKeyed(ITypeReferenceModel type, string key) {
        return $"__s_{type.FlattenedNameArityBased}_{HashKey(key)}_task";
    }
    
    internal static string BuildMultiBindLocalArrayFieldNotKeyed(ITypeReferenceModel contract) {
        return $"__mbarr_local_{contract.FlattenedNameArityBased}";
    }

    internal static string BuildMultiBindLocalArrayFieldKeyed(ITypeReferenceModel contract, string key) {
        return $"__mbarr_local_{contract.FlattenedNameArityBased}_{HashKey(key)}";
    }

    internal static string BuildMultiBindLocalArrayTaskFieldNotKeyed(ITypeReferenceModel contract) {
        return $"__mbarr_local_async_{contract.FlattenedNameArityBased}_task";
    }
    
    internal static string BuildMultiBindLocalArrayTaskFieldKeyed(ITypeReferenceModel contract, string key) {
        return $"__mbarr_local_async_{contract.FlattenedNameArityBased}_task_{HashKey(key)}";
    }

    internal static string BuildMultiBindLocalAsyncArrayFieldNotKeyed(ITypeReferenceModel contract) {
        return $"__mbarr_local_async_{contract.FlattenedNameArityBased}";
    }

    internal static string BuildMultiBindLocalAsyncArrayTaskFieldKeyed(ITypeReferenceModel contract, string key) {
        return $"__mbarr_local_async{contract.FlattenedNameArityBased}_{HashKey(key)}";
    }

    internal static string BuildFoundArrayFieldNotKeyed(ITypeReferenceModel contract) {
        return $"__found_{contract.FlattenedNameArityBased}";
    }

    internal static string BuildFoundArrayFieldKeyed(ITypeReferenceModel contract, string key) {
        return $"__found_{contract.FlattenedNameArityBased}_{HashKey(key)}";
    }

    private static ulong HashKey(string key) {
        const ulong offset = 14695981039346656037UL;
        const ulong prime = 1099511628211UL;
        
        ulong hash = offset;
        
        foreach (char c in key) {
            hash ^= c;
            hash *= prime;
        }
        
        return hash;
    }
}