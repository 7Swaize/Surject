using Microsoft.CodeAnalysis;
using Surject.Generators.Discovery.Injection;
using Surject.Generators.Models.Collections;
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
    internal ConstructorCreationModel(IMethodSymbol methodSymbol, Compilation compilation, TypeReferenceModelFactory typeRefFactory) {
        Constructor = new ConstructorModel(methodSymbol, typeRefFactory);
        ConstructWithInjectionTargets = InjectionTargetParser.GetConstructWithInjectionTargets(methodSymbol, compilation, typeRefFactory);
    }

    internal override ServiceCreationModelType CreationType => ServiceCreationModelType.Constructor;
    
    internal ConstructorModel Constructor { get; init; }
    internal EquatableArray<InjectionTargetModel> ConstructWithInjectionTargets { get; init; }
}

internal sealed record FactoryMethodCreationModel : ServiceCreationModel {
    internal FactoryMethodCreationModel(IMethodSymbol methodSymbol, Compilation compilation, TypeReferenceModelFactory typeRefFactory) {
        Method = new MethodModel(methodSymbol, typeRefFactory);
        ConstructWithInjectionTargets = InjectionTargetParser.GetConstructWithInjectionTargets(methodSymbol, compilation, typeRefFactory);
    }

    internal override ServiceCreationModelType CreationType => ServiceCreationModelType.FactoryMethod;
    
    internal MethodModel Method { get; init; }
    internal EquatableArray<InjectionTargetModel> ConstructWithInjectionTargets { get; init; }
}

internal sealed record MonoBehaviourCreationModel : ServiceCreationModel {
    internal override ServiceCreationModelType CreationType => ServiceCreationModelType.ImplicitThroughMonoBehaviour;
}