using System;
using Microsoft.CodeAnalysis;
using Surject.Generators.Models.Collections;

namespace Surject.Generators.Models.Primitives;

internal interface ITypeReferenceModel : IEquatable<ITypeReferenceModel> {
    string FQNGenericOmitted { get; init; }
    string FQNGenericBased { get; init; }
    string FQNArityBased { get; init; }
    string FQNConstructedArgBased { get; init; }
    string FlattenedNameNonArityBased { get; init; }
    string? Namespace { get; init; }
    
    bool IsBasedOnTypeParameter { get; init; }
    bool IsGeneric { get; init; }
    bool IsOpenGeneric { get; init; }
    ITypeReferenceModel? UnboundGenericTypeRef { get; init; }
    EquatableArray<ITypeReferenceModel> TypeArguments { get; init; }
    EquatableArray<ITypeReferenceModel> TypeParameters { get; init; }
    EquatableArray<ConstraintsModel> Constraints { get; init; }
    
    bool IsTrueValueType { get; init; }
    TypeKind TypeKind { get; init; }
    SpecialType SpecialType { get; init; }
    
    ITypeReferenceModel? BaseType { get; init; }
    EquatableArray<ITypeReferenceModel> ImmediateInterfaces { get; init; }
    EquatableArray<ITypeReferenceModel> AllInterfaces { get; init; }
    
    ITypeSymbol UnderlyingTypeSymbol { get; }
}