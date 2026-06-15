using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Surject.Generators.Discovery.ServiceRegistration;
using Surject.Generators.Models.Collections;


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
            .Where(invocation => !IsPartOfLargerChain(invocation));
        
        Bindings = invocations
            .Select(inv => BindingRegistrationParser.Parse(inv, utils, context.SemanticModel))
            .ToImmutableArray()
            .AsEquatableArray();
    }
    
    private static bool IsPartOfLargerChain(InvocationExpressionSyntax invocation) {
        return invocation.Parent is MemberAccessExpressionSyntax member &&
               member.Parent is InvocationExpressionSyntax;
    }
    
    private const string KScopeContextMethodName = "Configure";
    
    internal EquatableArray<BindingModel> Bindings { get; init; }
}