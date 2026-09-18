using System;

namespace Surject.Unity.Utility.Exceptions;

internal sealed class SurjectRuntimeException(string message) : Exception(message);