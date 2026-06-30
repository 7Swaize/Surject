using Microsoft.CodeAnalysis;
using Surject.Generators.Models.Factories;
using Surject.Generators.Models.Primitives;

namespace Surject.Generators.Models.Concepts;

internal enum ServiceCreationModelType {
    Constructor,
    FactoryMethod,
    ImplicitThroughMonoBehaviour
}

internal abstract record ServiceCreationModel {
    internal abstract ServiceCreationModelType CreationType { get; }
}

internal sealed record ConstructorCreationModel : ServiceCreationModel {
    internal ConstructorCreationModel(IMethodSymbol methodSymbol, TypeReferenceModelFactory typeRefFactory) {
        Constructor = new ConstructorModel(methodSymbol, typeRefFactory);
    }

    internal override ServiceCreationModelType CreationType => ServiceCreationModelType.Constructor;
    internal ConstructorModel Constructor { get; init; }
}

internal sealed record FactoryMethodCreationModel : ServiceCreationModel {
    internal FactoryMethodCreationModel(IMethodSymbol methodSymbol, TypeReferenceModelFactory typeRefFactory) {
        Method = new MethodModel(methodSymbol, typeRefFactory);
    }

    internal override ServiceCreationModelType CreationType => ServiceCreationModelType.FactoryMethod;
    internal MethodModel Method { get; init; }
}

internal sealed record MonoBehaviourCreationModel : ServiceCreationModel {
    internal override ServiceCreationModelType CreationType => ServiceCreationModelType.ImplicitThroughMonoBehaviour;
}