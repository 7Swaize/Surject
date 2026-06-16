using System.Linq;
using Microsoft.CodeAnalysis;
using Surject.Generators.Discovery;
using Surject.Generators.Models.Collections;

namespace Surject.Generators.Models.Primitives;

internal sealed record ConstructorModel {
    internal ConstructorModel(IMethodSymbol methodSymbol, DiscoveryUtils uGrouping) {
        Parameters = [
            .. methodSymbol.Parameters.Select(parameterSymbol =>
                new ParameterModel(parameterSymbol, uGrouping))
        ];
    }

    internal EquatableArray<ParameterModel> Parameters { get; init; }
}