using System;
using System.Collections.Generic;

namespace Surject.Shared.Helpers;

public static class SpanHelpers {
    public static Span<T> AsSpan<T>(List<T>? list) {
        if (list == null) {
            return default;
        }
    }
}