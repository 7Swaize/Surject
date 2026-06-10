using System;

namespace Surject.Generators.Models.Collections;

public readonly record struct EquatablePair<T1, T2>
    where T1 : IEquatable<T1>
    where T2 : IEquatable<T2>
{
    public T1 First { get; init; }
    public T2 Second { get; init; }

    public EquatablePair(T1 first, T2 second) {
        First = first;
        Second = second;
    }
}

public readonly record struct EquatablePair<T>
    where T : IEquatable<T>
{
    public T First { get; init; }
    public T Second { get; init; }

    public EquatablePair(T first, T second) {
        First = first;
        Second = second;
    }
}
