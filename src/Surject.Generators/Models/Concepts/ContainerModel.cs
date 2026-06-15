using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Surject.Generators.Discovery.ServiceRegistration;
using Surject.Generators.Models.Collections;
using Surject.Generators.Models.Primitives;


namespace Surject.Generators.Models.Concepts;

internal sealed record ContainerModel {
    internal ContainerModel(GeneratorAttributeSyntaxContext context, DiscoveryUtils utils) {
        IMethodSymbol registrationMethod = context.TargetSymbol
            .As<INamedTypeSymbol>()
            .GetMembers()
            .OfType<IMethodSymbol>()
            .First(m => m.Name == KScopeContextMethodName);
        
        IEnumerable<InvocationExpressionSyntax> invocations = registrationMethod.DeclaringSyntaxReferences[0]
            .GetSyntax()
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Where(inv => !inv.IsPartOfLargerChain());
        
        Bindings = invocations
            .Select(inv => BindingRegistrationParser.Parse(inv, utils, context.SemanticModel))
            .Where(b => b != null)
            .ToImmutableArray()!;

        ContainerDecl = new ClassDeclModel((INamedTypeSymbol)context.TargetSymbol, utils);
    }
    
    private const string KScopeContextMethodName = "Configure";
    
    internal ClassDeclModel ContainerDecl { get; init; }
    internal EquatableArray<BindingModel> Bindings { get; init; }
}