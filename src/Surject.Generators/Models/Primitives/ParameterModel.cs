using Microsoft.CodeAnalysis;
using Surject.Generators.Models.Factories;

namespace Surject.Generators.Models.Primitives;

internal sealed record ParameterModel {
    internal ParameterModel(IParameterSymbol parameterSymbol, TypeReferenceModelFactory typeRefFactory) {
        Name = parameterSymbol.Name;
        HasExplicitDefaultValue = parameterSymbol.HasExplicitDefaultValue;
        RefKind = parameterSymbol.RefKind;

        Type = typeRefFactory.CreateOrGetTypeReferenceModel(parameterSymbol.Type);
    }

    internal string Name { get; init; }
    internal bool HasExplicitDefaultValue { get; init; }
    internal RefKind RefKind { get; init; }
    internal ITypeReferenceModel Type { get; init; }
}