using System;

namespace Surject.Unity.RuntimeExceptions;

internal sealed class SurjectRuntimeException(string message) : Exception(message);