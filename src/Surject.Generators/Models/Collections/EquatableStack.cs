using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Surject.Generators.Models.Collections;

public readonly struct EquatableStack<T> :
    IReadOnlyCollection<T>,
    IEquatable<EquatableStack<T>>
        where T : IEquatable<T>
{
    private readonly Stack<T>? _stack;
    
    private static readonly Stack<T> SharedEmptyStack = new();
    private Stack<T> Stack {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _stack ?? SharedEmptyStack;
    }

    public EquatableStack(Stack<T> stack) {
        _stack = stack;
    }

    public override bool Equals(object? obj) => obj is EquatableStack<T> other && Equals(other);
    
    public bool Equals(EquatableStack<T> other) {
        if (Count != other.Count) {
            return false;
        }
        
        return Stack.SequenceEqual(other.Stack);
    }

    public override int GetHashCode() {
        HashCode hash = new HashCode();

        foreach (T value in Stack) {
            hash.Add(value);
        }

        return hash.ToHashCode();
    }

    public int Count {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Stack.Count;
    }

    public bool IsEmpty {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Stack.Count == 0;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Peek() => Stack.Peek();
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Stack<T>.Enumerator GetEnumerator() => Stack.GetEnumerator();

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static bool operator ==(EquatableStack<T> left, EquatableStack<T> right) => left.Equals(right);
    public static bool operator !=(EquatableStack<T> left, EquatableStack<T> right) => !left.Equals(right);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Stack<T> ToStack() => new(Stack);
}
