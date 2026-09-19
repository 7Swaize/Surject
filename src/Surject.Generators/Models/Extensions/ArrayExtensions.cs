using System;
using System.Runtime.CompilerServices;
using Surject.Generators.Models.Collections;

internal static class ArrayExtensions {
    extension<T>(T[] array) where T : IEquatable<T> {
        internal EquatableArray<T> AsEquatableArrayUnsafe() {
            return Unsafe.As<T[], EquatableArray<T>>(ref array);
        }
    }
}