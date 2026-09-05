using Microsoft.CodeAnalysis;
using Surject.Generators.Models.Collections;
using Surject.Generators.Models.Factories;
using Surject.Generators.Models.Primitives;

namespace Surject.Generators.Models.Concepts;

internal sealed record ContainerModel {
    internal ContainerModel(in GeneratorAttributeSyntaxContext context) {
        TypeReferenceModelFactory typeRefFactory = TypeReferenceModelFactory.GetFactory(context.SemanticModel.Compilation);
        SemanticModel semanticModel = context.SemanticModel;
        
    }
    
    internal TypeDeclModel Decl { get; init; }
    internal ScopeLevelKind ScopeLevelKind { get; init; }

    internal EquatableArray<RegistrationModel> Bindings { get; init; }
}

internal enum ScopeLevelKind : byte {
    Application = 0,
    Scene = 1,
    GameObject = 2,
}