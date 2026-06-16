using Microsoft.CodeAnalysis;
using Surject.Generators.Discovery;

namespace Surject.Generators.Models.Primitives;

internal sealed record ParameterModel {
    internal ParameterModel(IParameterSymbol parameterSymbol, DiscoveryUtils uGrouping) {
        Name = parameterSymbol.Name;
        HasExplicitDefaultValue = parameterSymbol.HasExplicitDefaultValue;
        RefKind = parameterSymbol.RefKind;

        Type = uGrouping.TypeReferenceModelFactory.CreateOrGetTypeReferenceModel(parameterSymbol.Type);
    }

    internal string Name { get; init; }
    internal bool HasExplicitDefaultValue { get; init; }
    internal RefKind RefKind { get; init; }
    internal ITypeReferenceModel Type { get; init; }
}