using System;
using System.Collections.Generic;
    
namespace Surject.Unity.Utility.Collections;

internal sealed class DeferredTypeRegistry<TItem> {
    private readonly Dictionary<RuntimeTypeHandle, TItem> _values = new();
    private readonly Dictionary<RuntimeTypeHandle, List<Action<TItem>>> _pendingContinuations = new();

    internal void Register<TConcrete>(TConcrete value) where TConcrete : TItem {
        RuntimeTypeHandle typeHandle = typeof(TConcrete).TypeHandle;

        _values[typeHandle] = value;
        ResolveWaitersFor(typeHandle, value);
    }

    internal void RegisterDeferred<TProvideConcrete, TDependConcrete>(Func<TDependConcrete, TProvideConcrete> factory)
        where TProvideConcrete : TItem
        where TDependConcrete : TItem 
    {
        RuntimeTypeHandle depTypeHandle = typeof(TDependConcrete).TypeHandle;
        if (_values.TryGetValue(depTypeHandle, out TItem dependency)) {
            Register(factory((TDependConcrete)dependency!));
            return;
        }

        if (!_pendingContinuations.ContainsKey(depTypeHandle)) {
            _pendingContinuations[depTypeHandle] = new List<Action<TItem>>();
        }
        
        _pendingContinuations[depTypeHandle].Add(resolved => Register(factory((TDependConcrete)resolved!)));
    }

    internal void Unregister<TConcrete>() where TConcrete : TItem {
        RuntimeTypeHandle typeHandle = typeof(TConcrete).TypeHandle;
        _values.Remove(typeHandle);
    }

    internal void Clear() {
        _values.Clear();
        _pendingContinuations.Clear();
    }

    private void ResolveWaitersFor(RuntimeTypeHandle typeHandle, TItem value) {
        if (!_pendingContinuations.TryGetValue(typeHandle, out List<Action<TItem>> continuations)) {
            return;
        }
        
        foreach (Action<TItem> continuation in continuations) {
            continuation(value);
        }
        
        _pendingContinuations.Remove(typeHandle);
    }
}