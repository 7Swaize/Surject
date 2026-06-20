using System.Diagnostics.CodeAnalysis;

namespace Surject.Unity.RuntimeExceptions;

internal static class ThrowHelpers {
    [DoesNotReturn]
    internal static TReturn ThrowSurjectRuntimeException<TReturn>(string message) {
        throw new SurjectRuntimeException(message);
    }
    
    [DoesNotReturn]
    internal static void ThrowSurjectRuntimeException(string message) {
        throw new SurjectRuntimeException(message);
    }
}