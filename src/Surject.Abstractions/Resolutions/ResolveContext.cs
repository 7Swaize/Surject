using System;

namespace Surject.Abstractions.Resolutions;

public readonly ref struct ResolveContext {
    public readonly ResolveFlags Context { get; init; }
    public readonly string? Key { get; init; }
}

[Flags]
public enum ResolveFlags : byte {
    None = 0,
    Primary = 1 << 0,
    Keyed = 1 << 1
}