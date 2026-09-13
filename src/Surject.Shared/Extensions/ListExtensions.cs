using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public static class ListExtensions {
    private class ListPrivateFieldAccess<T> {
#pragma warning disable CS0649
#pragma warning disable CS8618
        internal T[] _items;
        internal int _size;
        internal int _version;
#pragma warning restore CS8618
#pragma warning restore CS0649
    }

    extension<T>(List<T>? self) {
        public Span<T> AsSpanUnsafe() {
            return self is null
                ? default
                : Unsafe.As<ListPrivateFieldAccess<T>>(self)._items.AsSpan();
        }
    }
}