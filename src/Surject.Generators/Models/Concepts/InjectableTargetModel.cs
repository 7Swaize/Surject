using Microsoft.CodeAnalysis;
using Surject.Generators.Discovery;
using Surject.Generators.Discovery.Injection;
using Surject.Generators.Models.Collections;
using Surject.Generators.Models.Primitives;

namespace Surject.Generators.Models.Concepts;

internal sealed record InjectableTargetModel {
    internal InjectableTargetModel(in GeneratorAttributeSyntaxContext context, DiscoveryUtils utils) {
        Decl = new ClassDeclModel((INamedTypeSymbol)context.TargetSymbol, utils);
        MembersToInject = InjectableTargetParser.GetMembersToInject(
            (INamedTypeSymbol)context.TargetSymbol, context.SemanticModel.Compilation, utils
        );
    }
    
    internal ClassDeclModel Decl { get; init; }
    internal EquatableArray<InjectableMemberModel> MembersToInject { get; init; }
}