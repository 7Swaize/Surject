using System;
using System.Runtime.CompilerServices;
using Surject.Abstractions.Resolutions;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;

namespace Surject.Generators.Emitters;

internal static class EmitConstants {
    internal const string KContainerFieldName = "__container";
    internal const string KResolverPropertyName = "__resolver";
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string FQN(Type type) => $"global::{type.FullName}";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string BuildContainerTypeName(ContainerModel container)
        => $"Container_{container.Decl.ClassAsTypeRef.FlattenedNameNonArityBased}";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string BuildResolverTypeName(ContainerModel container)
        => $"Resolver_{container.Decl.ClassAsTypeRef.FlattenedNameNonArityBased}";
    
    // Field caches

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string BuildSingletonField(ITypeReferenceModel t, string? key = null)
        => key is null ? $"__s_{t.FlattenedNameNonArityBased}" : $"__s_{t.FlattenedNameNonArityBased}_{key}";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string BuildMultiBindPrimaryField(ITypeReferenceModel t, int index, string? key = null)
        => key is null
            ? $"__mb_{t.FlattenedNameNonArityBased}_{index}_primary"
            : $"__mb_{t.FlattenedNameNonArityBased}_{key}_{index}_primary";
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string BuildMultiBindMemberField(ITypeReferenceModel t, int index, string? key = null)
        => key is null ? $"__mb_{t.FlattenedNameNonArityBased}_{index}" : $"__mb_{t.FlattenedNameNonArityBased}_{key}_{index}";
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string BuildMultiBindArrayField(ITypeReferenceModel t, string? key = null) 
        => key is null ? $"__mbarr_{t.FlattenedNameNonArityBased}" : $"__mbarr_{t.FlattenedNameNonArityBased}_{key}";
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string BuildLazyField(ITypeReferenceModel t, string? key = null)
        => key is null ? $"__lazy_{t.FlattenedNameNonArityBased}" : $"__lazy_{t.FlattenedNameNonArityBased}_{key}";
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string BuildAsyncTaskField(ITypeReferenceModel t, string? key = null)
        => key is null ? $"__async_{t.FlattenedNameNonArityBased}_task" : $"__async_{t.FlattenedNameNonArityBased}_{key}_task";
    
    // Methods
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string ResolveMethod(ITypeReferenceModel t, string? key = null)
        => key is null ? $"Resolve_{t.FlattenedNameNonArityBased}" : $"Resolve_{t.FlattenedNameNonArityBased}_{key}";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string ResolveMemberMethod(ITypeReferenceModel t, int index, string? key = null)
        => key is null ? $"Resolve_{t.FlattenedNameNonArityBased}_Member{index}" : $"Resolve_{t.FlattenedNameNonArityBased}_{key}_Member{index}";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string ResolvePrimaryMethod(ITypeReferenceModel t, string? key = null)
        => key is null ? $"Resolve_{t.FlattenedNameNonArityBased}_Primary" : $"Resolve_{t.FlattenedNameNonArityBased}_{key}_Primary";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string ResolveAllMethod(ITypeReferenceModel t, string? key = null)
        => key is null ? $"Resolve_{t.FlattenedNameNonArityBased}_All" : $"Resolve_{t.FlattenedNameNonArityBased}_{key}_All";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string ResolveLazyMethod(ITypeReferenceModel t, string? key = null)
        => key is null ? $"Resolve_{t.FlattenedNameNonArityBased}_Lazy" : $"Resolve_{t.FlattenedNameNonArityBased}_{key}_Lazy";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string ResolveAsyncMethod(ITypeReferenceModel t, string? key = null)
        => key is null ? $"ResolveAsync_{t.FlattenedNameNonArityBased}" : $"ResolveAsync_{t.FlattenedNameNonArityBased}_{key}";
    
    
    // IResolver

    internal static (string method, string ctxExpr) BuildResolverCall(InjectableMemberModel member) {
        static void AppendFlag(ref string? flags, ResolveFlags flag) {
            string value = $"{FQN(typeof(ResolveFlags))}.{flag}";
            flags = flags is null ? value : $"{flags} | {value}";
        }
        
        InjectionDeferralKind deferralKind = member.Deferral;
        
        string method = "Resolve"
            + ((deferralKind & InjectionDeferralKind.Optional) != 0 ? "Optional" : "")
            + ((deferralKind & InjectionDeferralKind.All) != 0 ? "All" : "")
            + ((deferralKind & InjectionDeferralKind.Lazy) != 0 ? "Lazy" : "")
            + ((deferralKind & InjectionDeferralKind.Async) != 0 ? "Async" : "");
        
        string? key = member.Id;
        string? flags = null;
        
        if ((member.Deferral & InjectionDeferralKind.Primary) != 0) {
            AppendFlag(ref flags, ResolveFlags.Primary);
        }

        if ((member.Deferral & InjectionDeferralKind.Keyed) != 0) {
            AppendFlag(ref flags, ResolveFlags.Keyed);
        }
        
        string ctxExpr = (flags, key) switch {
            (null, null) => $"new {FQN(typeof(ResolveFlags))}()",
            (_, null) => $"new {FQN(typeof(ResolveFlags))}({flags})",
            (null, _) => $"new {FQN(typeof(ResolveFlags))}(key: \"{key}\")",
            _ => $"new {FQN(typeof(ResolveFlags))}({flags}, \"{key}\")",
        };
        
        return (method, ctxExpr);
    }
}