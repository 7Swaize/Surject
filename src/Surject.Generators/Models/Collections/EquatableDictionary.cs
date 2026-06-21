using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Surject.Generators.Models.Collections;

// see ref: https://github.com/microsoft/referencesource/blob/main/mscorlib/system/collections/generic/dictionary.cs

public readonly struct EquatableDictionary<TKey, TValue> :
    IReadOnlyDictionary<TKey, TValue>,
    
    IEquatable<EquatableDictionary<TKey, TValue>>
        where TKey : IEquatable<TKey>
        where TValue : IEquatable<TValue>
{
    private readonly Dictionary<TKey, TValue>? _dictionary;
    private readonly IEqualityComparer<TKey> _keyComparer;
    private readonly IEqualityComparer<TValue> _valueComparer;
    
    private static readonly Dictionary<TKey, TValue> SharedEmptyDictionary = new();
    private Dictionary<TKey, TValue> Dictionary {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _dictionary ?? SharedEmptyDictionary;
    }

    public EquatableDictionary(
        Dictionary<TKey, TValue> dictionary,
        IEqualityComparer<TKey>? keyComparer = null,
        IEqualityComparer<TValue>? valueComparer = null
    ) {
        _keyComparer = keyComparer ?? EqualityComparer<TKey>.Default;
        _valueComparer = valueComparer ?? EqualityComparer<TValue>.Default;
        _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
    }
    
    public override bool Equals(object? obj) => obj is EquatableDictionary<TKey, TValue> other && Equals(other);

    public bool Equals(EquatableDictionary<TKey, TValue> other) {
        if (Dictionary.Count != other.Dictionary.Count) return false;

        foreach (KeyValuePair<TKey, TValue> kvp in Dictionary) {
            if (!other.Dictionary.TryGetValue(kvp.Key, out var otherValue)) return false;
            if (!_valueComparer.Equals(kvp.Value, otherValue)) return false;
        }

        return true;
    }
    
    public override int GetHashCode() {
        int hashCode = 0;

        foreach (KeyValuePair<TKey, TValue> kvp in Dictionary) {
            int keyHash = _keyComparer.GetHashCode(kvp.Key);
            int valueHash = kvp.Value is null ? 0 : _valueComparer.GetHashCode(kvp.Value);
            
            hashCode ^= HashCode.Combine(keyHash, valueHash);
        }

        return hashCode;
    }

    public TValue this[TKey key] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Dictionary[key];
    }

    public IEnumerable<TKey> Keys {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Dictionary.Keys;
    }

    public IEnumerable<TValue> Values {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Dictionary.Values;   
    }

    public int Count {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Dictionary.Count;   
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKey(TKey key) => Dictionary.ContainsKey(key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable CS8767
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => Dictionary.TryGetValue(key, out value);
#pragma warning disable CS8767
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Dictionary<TKey, TValue>.Enumerator GetEnumerator() {
        return Dictionary.GetEnumerator();
    }
    
    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() {
        return Dictionary.GetEnumerator();
    }
    
    IEnumerator IEnumerable.GetEnumerator() {
        return Dictionary.GetEnumerator();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ImmutableDictionary<TKey, TValue> ToImmutableDictionary() =>
        Dictionary.ToImmutableDictionary(_keyComparer, _valueComparer);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Dictionary<TKey, TValue> ToDictionary() => new(Dictionary, _keyComparer);

    public static bool operator ==(EquatableDictionary<TKey, TValue> left, EquatableDictionary<TKey, TValue> right) =>
        left.Equals(right);
    
    public static bool operator !=(EquatableDictionary<TKey, TValue> left, EquatableDictionary<TKey, TValue> right) =>
        !left.Equals(right);
}


public static class EquatableDictionaryExtensions {
    extension<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        where TKey : IEquatable<TKey>
        where TValue : IEquatable<TValue>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EquatableDictionary<TKey, TValue> ToEquatableDictionary(
            IEqualityComparer<TKey>? keyComparer = null,
            IEqualityComparer<TValue>? valueComparer = null
        ) =>
            new(
                new Dictionary<TKey, TValue>(dictionary),
                keyComparer ?? EqualityComparer<TKey>.Default,
                valueComparer ?? EqualityComparer<TValue>.Default
            );
    }
}
