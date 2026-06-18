using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Surject.Abstractions.Registrations;
using Surject.Generators.Discovery;
using Surject.Generators.Discovery.ServiceRegistration;
using Surject.Generators.Models.Collections;
using Surject.Generators.Models.Primitives;

namespace Surject.Generators.Models.Concepts;

internal sealed record ContainerModel {
    internal ContainerModel(in GeneratorAttributeSyntaxContext context, DiscoveryUtils utils) {
        SemanticModel semanticModel = context.SemanticModel;
        IMethodSymbol registrationMethod = context.TargetSymbol
            .As<INamedTypeSymbol>()
            .GetMembers()
            .OfType<IMethodSymbol>()
            .First(m => m.Name == nameof(ScopeContext.Configure));
        
        IEnumerable<InvocationExpressionSyntax> invocations = registrationMethod.DeclaringSyntaxReferences[0]
            .GetSyntax()
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Where(inv => !inv.IsPartOfLargerChain());
        
        Bindings = invocations
            .Select(inv => BindingRegistrationParser.Parse(inv, utils, semanticModel))
            .Where(b => b != null)
            .ToImmutableArray()!;

        Decl = new ClassDeclModel((INamedTypeSymbol)context.TargetSymbol, utils);
        ParentOverride = ContainerParser.TryGetParentScopeOverride(in context, utils);
    }
    
    internal ClassDeclModel Decl { get; init; }
    internal ITypeReferenceModel? ParentOverride { get; init; }
    internal EquatableArray<RegistrationModel> Bindings { get; init; }
}