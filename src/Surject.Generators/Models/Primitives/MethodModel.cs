using System.Linq;
using Microsoft.CodeAnalysis;
using Surject.Generators.Models.Collections;
using Surject.Generators.Models.Factories;

namespace Surject.Generators.Models.Primitives;

internal sealed record MethodModel {
    internal MethodModel(IMethodSymbol methodSymbol, TypeReferenceModelFactory typeRefFactory) {
        ContainingType = typeRefFactory.CreateOrGetTypeReferenceModel(methodSymbol.ContainingType);
        NameWithGenericParams = methodSymbol.NameWithGenericParameters;
        NameWithoutGenerics = methodSymbol.NameWithoutGenericParameters;

        TypeParameters = [
            .. methodSymbol.TypeParameters.Select(typeParam =>
                typeRefFactory.CreateOrGetTypeReferenceModel(typeParam))
        ];
        Parameters = [
            .. methodSymbol.Parameters.Select(parameterSymbol =>
                new ParameterModel(parameterSymbol, typeRefFactory))
        ];

        IsReturnTypeOfVoid = methodSymbol.ReturnType.SpecialType == SpecialType.System_Void;
        if (!IsReturnTypeOfVoid) {
            ReturnType = typeRefFactory.CreateOrGetTypeReferenceModel(methodSymbol.ReturnType);
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