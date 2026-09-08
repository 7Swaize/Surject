using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Surject.Shared.Helpers;

public static class NoAllocHelpers {
    private class ListPrivateFieldAccess<T> {
        internal T[] _items;
        internal int _size;
        internal int _version;
    }
    
    public static Span<T> AsSpan<T>(List<T>? list) {
        return list == null
            ? default
            : Unsafe.As<ListPrivateFieldAccess<T>>(list)._items.AsSpan();
    }
}