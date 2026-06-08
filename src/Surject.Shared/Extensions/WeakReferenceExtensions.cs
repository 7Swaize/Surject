using System;

public static class WeakReferenceExtensions {
    extension<T>(WeakReference<T> weakReference) where T : class {
        public T GetTargetOrThrow() {
            return weakReference.TryGetTarget(out T? target)
                ? target 
                : throw new ObjectDisposedException(nameof(T));
        }
    }
}