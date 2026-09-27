using Surject.Generators.Models.Primitives;

namespace Surject.Generators.Emitters.Helpers;

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

    internal static string BuildMultiBindLocalAsyncArrayFieldKeyed(ITypeReferenceModel contract, string key) {
        return $"__mbarr_local_async{contract.FlattenedNameArityBased}_{HashKey(key)}";
    }

    internal static string BuildFoundArrayFieldNotKeyed(ITypeReferenceModel contract) {
        return $"__found_{contract.FlattenedNameArityBased}";
    }

    internal static string BuildFoundArrayFieldKeyed(ITypeReferenceModel contract, string key) {
        return $"__found_{contract.FlattenedNameArityBased}_{HashKey(key)}";
    }

    internal static string BuildSyncFactoryMethodNameNotKeyed(ITypeReferenceModel contract) {
        return $"__Create_{contract.FlattenedNameArityBased}";
    }

    internal static string BuildSyncFactoryMethodNameKeyed(ITypeReferenceModel contract, string key) {
        return $"__Create_{contract.FlattenedNameArityBased}_{HashKey(key)}";
    }
    
    internal static string BuildAsyncFactoryMethodNameNotKeyed(ITypeReferenceModel contract) {
        return $"__CreateAsync_{contract.FlattenedNameArityBased}";
    }

    internal static string BuildAsyncFactoryMethodNameKeyed(ITypeReferenceModel contract, string key) {
        return $"__CreateAsync_{contract.FlattenedNameArityBased}_{HashKey(key)}";
    }
    
    internal static string BuildResolveMethodNameNotKeyed(ITypeReferenceModel type) {
        return $"__Resolve_{type.FlattenedNameArityBased}";
    }

    internal static string BuildResolveMethodNameKeyed(ITypeReferenceModel type, string key) {
        return $"__Resolve_{type.FlattenedNameArityBased}_{HashKey(key)}";
    }
    
    internal static string BuildDiscoverMethodNameNotKeyed(ITypeReferenceModel type) {
        return $"__Discover_{type.FlattenedNameArityBased}";
    }

    internal static string BuildDiscoverMethodNameKeyed(ITypeReferenceModel type, string key) {
        return $"__Discover_{type.FlattenedNameArityBased}_{HashKey(key)}";
    }
    
    internal static string BuildDiscoverAllMethodNameNotKeyed(ITypeReferenceModel contract) {
        return $"__DiscoverAll_{contract.FlattenedNameArityBased}";
    }

    internal static string BuildDiscoverAllMethodNameKeyed(ITypeReferenceModel contract, string key) {
        return $"__DiscoverAll_{contract.FlattenedNameArityBased}_{HashKey(key)}";
    }
    
    internal static string BuildInstantiateMethodNameNotKeyed(ITypeReferenceModel type) {
        return $"__Instantiate_{type.FlattenedNameArityBased}";
    }

    internal static string BuildInstantiateMethodNameKeyed(ITypeReferenceModel type, string key) {
        return $"__Instantiate_{type.FlattenedNameArityBased}_{HashKey(key)}";
    }
    
    internal static string BuildMultiBindPrimaryMethodNameNotKeyed(ITypeReferenceModel contract) {
        return $"__ResolvePrimary_{contract.FlattenedNameArityBased}";
    }

    internal static string BuildMultiBindPrimaryMethodNameKeyed(ITypeReferenceModel contract, string key) {
        return $"__ResolvePrimary_{contract.FlattenedNameArityBased}_{HashKey(key)}";
    }
    
    internal static string BuildMultiBindLocalOrderedMethodNameNotKeyed(ITypeReferenceModel contract) {
        return $"__ResolveLocalOrdered_{contract.FlattenedNameArityBased}";
    }

    internal static string BuildMultiBindLocalOrderedMethodNameKeyed(ITypeReferenceModel contract, string key) {
        return $"__ResolveLocalOrdered_{contract.FlattenedNameArityBased}_{HashKey(key)}";
    }
    
    internal static string BuildMultiBindAllOrderedMethodNameNotKeyed(ITypeReferenceModel contract) {
        return $"__ResolveAllOrdered_{contract.FlattenedNameArityBased}";
    }

    internal static string BuildMultiBindAllOrderedMethodNameKeyed(ITypeReferenceModel contract, string key) {
        return $"__ResolveAllOrdered_{contract.FlattenedNameArityBased}_{HashKey(key)}";
    }
    
    internal static string BuildMultiBindAllMethodNameNotKeyed(ITypeReferenceModel contract) {
        return $"__ResolveAll_{contract.FlattenedNameArityBased}";
    }

    internal static string BuildMultiBindAllMethodNameKeyed(ITypeReferenceModel contract, string key) {
        return $"__ResolveAll_{contract.FlattenedNameArityBased}_{HashKey(key)}";
    }
    
    internal static string BuildAsyncResolveMethodNameNotKeyed(ITypeReferenceModel type) {
        return $"__ResolveAsync_{type.FlattenedNameArityBased}";
    }

    internal static string BuildAsyncResolveMethodNameKeyed(ITypeReferenceModel type, string key) {
        return $"__ResolveAsync_{type.FlattenedNameArityBased}_{HashKey(key)}";
    }
    
    internal static string BuildAsyncResolveSlowMethodNameNotKeyed(ITypeReferenceModel type) {
        return $"__ResolveAsyncSlow_{type.FlattenedNameArityBased}";
    }

    internal static string BuildAsyncResolveSlowMethodNameKeyed(ITypeReferenceModel type, string key) {
        return $"__ResolveAsyncSlow_{type.FlattenedNameArityBased}_{HashKey(key)}";
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