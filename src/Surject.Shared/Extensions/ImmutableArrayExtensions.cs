using System.Collections.Immutable;
using System.Runtime.CompilerServices;

public static class ImmutableArrayExtensions {
    extension<T>(ImmutableArray<T> self) {
        public T[]? AsArrayUnsafe() {
            // BCL guarantees this case is always safe: https://github.com/dotnet/runtime/issues/83141
            return Unsafe.As<ImmutableArray<T>, T[]>(ref self);
        }
    }
}