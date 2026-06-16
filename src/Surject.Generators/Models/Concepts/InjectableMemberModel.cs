using Surject.Generators.Models.Collections;
using Surject.Generators.Models.Primitives;

namespace Surject.Generators.Models.Concepts;

internal readonly record struct InjectableMemberModel {
    internal InjectionMode Mode { get; init; }
    internal InjectionSiteKind Site { get; init; }
    internal MethodModel? MethodRef { get; init; }
    internal ITypeReferenceModel? TypeRef { get; init; }
    internal string? Id { get; init; }
    
    internal EquatableArray<InjectableMemberModel>? Parameters { get; init; }
}

internal enum InjectionMode : byte {
    Standard,
    Optional,
    Lazy,
    Primary,
    Async
}

internal enum InjectionSiteKind : byte {
    Field,
    Property,
    Method,
    Parameter,
}