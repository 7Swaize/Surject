using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Surject.Unity.Handles;

internal readonly struct AsyncDisposableTracker {
    private readonly List<IAsyncDisposable> _items;
    
    internal AsyncDisposableTracker(int capacity) {
        _items = new List<IAsyncDisposable>(capacity);
    }

    internal void Track(IAsyncDisposable item) {
        _items.Add(item);
    }
    
    internal async ValueTask DisposeAllAsync() {
        foreach (IAsyncDisposable item in _items) {
            await item.DisposeAsync();
        }
        
        _items.Clear();
    }
}