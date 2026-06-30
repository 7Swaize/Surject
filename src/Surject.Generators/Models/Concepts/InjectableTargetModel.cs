using Microsoft.CodeAnalysis;
using Surject.Generators.Discovery.Injection;
using Surject.Generators.Models.Collections;
using Surject.Generators.Models.Factories;
using Surject.Generators.Models.Primitives;

namespace Surject.Generators.Models.Concepts;

internal sealed record InjectableTargetModel {
    internal InjectableTargetModel(in GeneratorAttributeSyntaxContext context) {
        TypeReferenceModelFactory typeRefFactory = TypeReferenceModelFactory.GetFactory(context.SemanticModel.Compilation);
        
        Decl = new ClassDeclModel((INamedTypeSymbol)context.TargetSymbol, typeRefFactory);
        MembersToInject = InjectableTargetParser.GetMembersToInject(
            (INamedTypeSymbol)context.TargetSymbol, context.SemanticModel.Compilation, typeRefFactory
        );
    }
    
    internal ClassDeclModel Decl { get; init; }
    internal EquatableArray<InjectableMemberModel> MembersToInject { get; init; }
}