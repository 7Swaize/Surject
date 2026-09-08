using System;
using Surject.Generators.Models.Collections;
using Surject.Generators.Models.Primitives;

namespace Surject.Generators.Models.Concepts;

internal readonly record struct InjectionTargetModel {
    internal string Name { get; init; }
    
    internal InjectionSiteKind InjectionSiteKind { get; init; }
    internal InjectionDeferralKind InjectionDeferralKind { get; init; }
    internal ITypeReferenceModel? UnwrappedTypeToRequest { get; init; }
    internal MethodModel? MethodRef { get; init; }
    internal string? IdAsText { get; init; }
    
    internal EquatableArray<InjectionTargetModel>? Parameters { get; init; }
}

[Flags]
internal enum InjectionDeferralKind : byte {
    None = 0,
    Standard = 1 << 0,
    Optional = 1 << 1,
    Primary = 1 << 3,
    Async = 1 << 4,
    All = 1 << 5,
    Keyed = 1 << 6
}

internal enum InjectionSiteKind : byte {
    Field,
    Property,
    Method,
    Parameter,
}