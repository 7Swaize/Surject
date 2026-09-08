using Microsoft.CodeAnalysis;
using Surject.Generators.Discovery.Injection;
using Surject.Generators.Models.Collections;
using Surject.Generators.Models.Factories;
using Surject.Generators.Models.Primitives;

namespace Surject.Generators.Models.Concepts;

internal sealed record InjectableContainerModel {
    internal InjectableContainerModel(in GeneratorAttributeSyntaxContext context) {
        TypeReferenceModelFactory typeRefFactory = TypeReferenceModelFactory.GetFactory(context.SemanticModel.Compilation);
        INamedTypeSymbol targetSymbol = (INamedTypeSymbol)context.TargetSymbol;
        
        Decl = new TypeDeclModel(targetSymbol, typeRefFactory);
        InjectionTargets = InjectionTargetParser.GetContainedInjectionTargets(
            targetSymbol,
            context.SemanticModel.Compilation,
            typeRefFactory
        );
    }
    
    internal TypeDeclModel Decl { get; init; }
    internal EquatableArray<InjectionTargetModel> InjectionTargets { get; init; }
}