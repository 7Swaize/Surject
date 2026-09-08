using System;

namespace Surject.Abstractions.Resolutions;

public readonly ref struct ResolveContext(string? key = null) {
    public string? Key { get; init; } = key;
}
