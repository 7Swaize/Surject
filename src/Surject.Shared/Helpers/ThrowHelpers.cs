using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Surject.Shared.Helpers;

public static class ThrowHelpers {
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static TResult ThrowUnhandledBranch<TResult>(object value) =>
        throw new InvalidOperationException($"Unhandled value '{value}' in switch statement");
    
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowUnhandledBranch(object value) =>
        throw new InvalidOperationException($"Unhandled value '{value}' in switch statement");
    
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowUnreachable() =>
        throw new UnreachableException("Code should not be reachable");
    
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static TResult ThrowUnreachable<TResult>(object value) =>
        throw new UnreachableException("Code should not be reachable");
    
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static TReturn ThrowWeakReferenceCollected<TReturn>() where TReturn : class =>
        throw new ObjectDisposedException(typeof(TReturn).Name, "Weak reference has been collected by the GC");

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static TReturn ThrowNonConstantExpressionException<TReturn>() =>
        throw new NonConstantExpressionException("Expected a compile-time constant expression");
}


public sealed class UnreachableException(string message) : Exception(message);

public sealed class NonConstantExpressionException(string message) : Exception(message);