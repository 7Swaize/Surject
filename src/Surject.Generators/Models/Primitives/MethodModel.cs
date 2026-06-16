using System.Linq;
using Microsoft.CodeAnalysis;
using Surject.Generators.Discovery;
using Surject.Generators.Models.Collections;

namespace Surject.Generators.Models.Primitives;

internal record MethodModel {
    internal MethodModel(IMethodSymbol methodSymbol, DiscoveryUtils uGrouping) {
        ContainingType = uGrouping.TypeReferenceModelFactory.CreateOrGetTypeReferenceModel(methodSymbol.ContainingType);
        NameWithGenericParams = methodSymbol.NameWithGenericParameters;
        NameWithoutGenerics = methodSymbol.NameWithoutGenericParameters;

        TypeParameters = [
            .. methodSymbol.TypeParameters.Select(typeParam =>
                uGrouping.TypeReferenceModelFactory.CreateOrGetTypeReferenceModel(typeParam))
        ];
        Parameters = [
            .. methodSymbol.Parameters.Select(parameterSymbol =>
                new ParameterModel(parameterSymbol, uGrouping))
        ];

        IsReturnTypeOfVoid = methodSymbol.ReturnType.SpecialType == SpecialType.System_Void;
        if (!IsReturnTypeOfVoid) {
            ReturnType = uGrouping.TypeReferenceModelFactory.CreateOrGetTypeReferenceModel(methodSymbol.ReturnType);
        }
    }
    
    internal ITypeReferenceModel ContainingType { get; init; }
    internal string NameWithGenericParams { get; init; }
    internal string NameWithoutGenerics { get; init; }
    
    internal EquatableArray<ITypeReferenceModel> TypeParameters { get; init; }
    internal EquatableArray<ParameterModel> Parameters { get; init; }

    internal bool IsReturnTypeOfVoid { get; init; }
    internal ITypeReferenceModel? ReturnType { get; init; }
}