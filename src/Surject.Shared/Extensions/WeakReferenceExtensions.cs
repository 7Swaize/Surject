using System;
using Surject.Shared.Helpers;

public static class WeakReferenceExtensions {
    extension<T>(WeakReference<T> weakReference) where T : class {
        public T GetTargetOrThrow() {
            return weakReference.TryGetTarget(out T? target)
                ? target 
                : ThrowHelpers.ThrowWeakReferenceCollected<T>();
        }
    }
}