using Microsoft.CodeAnalysis;
using Surject.Generators.Discovery;
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
    internal ConstructorCreationModel(IMethodSymbol methodSymbol, DiscoveryUtils uGrouping) {
        Constructor = new ConstructorModel(methodSymbol, uGrouping);
    }

    internal override ServiceCreationModelType CreationType => ServiceCreationModelType.Constructor;
    internal ConstructorModel Constructor { get; init; }
}

internal sealed record FactoryMethodCreationModel : ServiceCreationModel {
    internal FactoryMethodCreationModel(IMethodSymbol methodSymbol, DiscoveryUtils uGrouping) {
        Method = new MethodModel(methodSymbol, uGrouping);
    }

    internal override ServiceCreationModelType CreationType => ServiceCreationModelType.FactoryMethod;
    internal MethodModel Method { get; init; }
}

internal sealed record MonoBehaviourCreationModel : ServiceCreationModel {
    internal override ServiceCreationModelType CreationType => ServiceCreationModelType.ImplicitThroughMonoBehaviour;
}