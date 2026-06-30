using System.Linq;
using Microsoft.CodeAnalysis;
using Surject.Generators.Models.Collections;
using Surject.Generators.Models.Factories;

namespace Surject.Generators.Models.Primitives;

internal sealed record ConstructorModel {
    internal ConstructorModel(IMethodSymbol methodSymbol, TypeReferenceModelFactory typeRefFactory) {
        Parameters = [
            .. methodSymbol.Parameters.Select(parameterSymbol =>
                new ParameterModel(parameterSymbol, typeRefFactory))
        ];
    }

    internal EquatableArray<ParameterModel> Parameters { get; init; }
}