using System;

namespace Surject.Abstractions.Resolutions;

public readonly ref struct ResolveContext(ResolveFlags context = ResolveFlags.None, string? key = null) {
    public readonly ResolveFlags Context { get; init; } = context;
    public readonly string? Key { get; init; } = key;
}

[Flags]
public enum ResolveFlags : byte {
    None = 0,
    Primary = 1 << 0,
    Keyed = 1 << 1
}