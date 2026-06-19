using System;
using System.Diagnostics.CodeAnalysis;

namespace Surject.Shared.Helpers;

public static class ThrowHelpers {
    [DoesNotReturn]
    public static TResult ThrowUnhandledBranch<TResult>(object value) =>
        throw new InvalidOperationException($"Unhandled value '{value}' in switch statement.");
    
    [DoesNotReturn]
    public static void ThrowUnhandledBranch(object value) =>
        throw new InvalidOperationException($"Unhandled value '{value}' in switch statement.");
    
    [DoesNotReturn]
    public static void ThrowUnreachable() =>
        throw new UnreachableException("Code should not be reachable.");
    
    [DoesNotReturn]
    public static TReturn ThrowWeakReferenceCollected<TReturn>() where TReturn : class =>
        throw new ObjectDisposedException(typeof(TReturn).Name, "The weak ref has been collected by the GC.");
}


public sealed class UnreachableException(string message) : Exception(message);