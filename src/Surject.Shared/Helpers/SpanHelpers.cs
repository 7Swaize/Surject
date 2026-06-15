using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Surject.Shared.Helpers;

public static class SpanHelpers {
    public static Span<T> AsSpan<T>(List<T>? list) {
        if (list is null) {
            return default;
        }

        // TODO: This should be safe. Member '_list' should pretty much always be the first field.
        return new Span<T>(Unsafe.As<StrongBox<T[]>>(list).Value, 0, list.Count);
    }
}