using System;
using Surject.Generators.Models.Collections;
using Surject.Generators.Models.Primitives;

namespace Surject.Generators.Models.Concepts;

internal readonly record struct InjectableMemberModel {
    internal string Name { get; init; }
    
    internal InjectionDeferralKind Deferral { get; init; }
    internal InjectionSiteKind Site { get; init; }
    internal MethodModel? MethodRef { get; init; }
    internal ITypeReferenceModel? TypeToRequest { get; init; }
    internal string? Id { get; init; }
    
    internal EquatableArray<InjectableMemberModel>? Parameters { get; init; }
}

[Flags]
internal enum InjectionDeferralKind : byte {
    None = 0,
    Standard = 1 << 0,
    Optional = 1 << 1,
    Lazy = 1 << 2,
    Primary = 1 << 3,
    Async = 1 << 4,
    All = 1 << 5,
    Keyed = 1 << 6,
}

internal enum InjectionSiteKind : byte {
    Field,
    Property,
    Method,
    Parameter,
}