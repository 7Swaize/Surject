using System;
using System.Collections.Generic;

namespace Surject.Unity.Handles;

internal readonly struct DisposableTracker {
    private readonly List<IDisposable> _items;

    internal DisposableTracker(int capacity) {
        _items = new List<IDisposable>(capacity);
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