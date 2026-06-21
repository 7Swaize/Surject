using System;
using System.Runtime.CompilerServices;

namespace Surject.Generators.Emitters;

internal static class TypeNames {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string FQN(Type type) => $"global::{type.FullName}";
}