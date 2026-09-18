using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Surject.Unity.Utility.Exceptions;

internal static class ThrowHelpers {
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static TReturn ThrowSurjectRuntimeException<TReturn>(string message) 
        => throw new SurjectRuntimeException(message);

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void ThrowSurjectRuntimeException(string message)
        => throw new SurjectRuntimeException(message);
}