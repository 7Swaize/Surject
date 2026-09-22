using System;
using System.Collections.Generic;

namespace Surject.Unity.Handles;

internal readonly struct DisposableTracker {
    private readonly List<IDisposable> _items;

    public DisposableTracker() {
        _items = [];
    }

    internal void Track(IDisposable item) {
        _items.Add(item);
    }

    internal void DisposeAll() {
        foreach (IDisposable item in _items) {
            item.Dispose();
        }

        _items.Clear();
    }
}

