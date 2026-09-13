using System.Collections.Immutable;
using System.Runtime.CompilerServices;

public static class ArrayExtensions {
    extension<T>(T[] self) {
        public ImmutableArray<T> AsImmutableArrayUnsafe() {
            return Unsafe.As<T[], ImmutableArray<T>>(ref self);
        }
    }
}