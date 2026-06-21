using System.Runtime.CompilerServices;
using Surject.Generators.Models.Concepts;

namespace Surject.Generators.Emitters;

internal static class EmitConstants {
    internal const string KContainerFieldName = "__container";
    internal const string KResolverPropertyName = "__resolver";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string BuildContainerTypeName(ContainerModel container) {
        return $"Container_{container.Decl.ClassAsTypeRef.FlattenedNameNonArityBased}";
    }

    internal static string BuildResolverTypeName(ContainerModel container) {
        return $"Resolver_{container.Decl.ClassAsTypeRef.FlattenedNameNonArityBased}";
    }
}