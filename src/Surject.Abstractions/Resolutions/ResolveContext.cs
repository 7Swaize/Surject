namespace Surject.Abstractions.Resolutions;

public readonly struct NoneKey { } 

public readonly ref struct ResolveContext<T> {
    public T Key { get; init; }
}
