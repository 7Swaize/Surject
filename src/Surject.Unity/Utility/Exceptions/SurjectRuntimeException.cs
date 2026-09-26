using System;

namespace Surject.Unity.Utility.Exceptions;

public sealed class SurjectRuntimeException(string message) : Exception(message);