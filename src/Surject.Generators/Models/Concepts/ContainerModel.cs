using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Surject.Abstractions.Registrations;
using Surject.Generators.Discovery.ServiceRegistration;
using Surject.Generators.Models.Collections;
using Surject.Generators.Models.Factories;
using Surject.Generators.Models.Primitives;

namespace Surject.Generators.Models.Concepts;

internal sealed record ContainerModel {
    internal ContainerModel(in GeneratorAttributeSyntaxContext context, ContainerKind kind) {
        TypeReferenceModelFactory typeRefFactory = TypeReferenceModelFactory.GetFactory(context.SemanticModel.Compilation);
        SemanticModel semanticModel = context.SemanticModel;
        
        IMethodSymbol registrationMethod = context.TargetSymbol
            .As<INamedTypeSymbol>()
            .GetMembers()
            .OfType<IMethodSymbol>()
            .First(m => m.Name == nameof(ScopeContext.Configure));

        Bindings = registrationMethod.DeclaringSyntaxReferences[0]
            .GetSyntax()
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Where(inv => !inv.IsPartOfLargerChain())
            .Select(inv => RegistrationBindingParser.Parse(inv, typeRefFactory, semanticModel))
            .OfType<RegistrationModel>()
            .ToImmutableArray()
            .AsEquatableArray();

        Decl = new TypeDeclModel((INamedTypeSymbol)context.TargetSymbol, typeRefFactory);
        ContainerKind = kind;
        
        if (ContainerKind == ContainerKind.SubScope) {
            (ParentDiscoveryKind, ProvidedParentScope) = ContainerParser.ExtractFromScopeAttribute(in context, typeRefFactory);
        }
    }
    
    internal TypeDeclModel Decl { get; init; }
    internal ContainerKind ContainerKind { get; init; }
    internal ParentDiscoveryKind ParentDiscoveryKind { get; init; }
    internal ITypeReferenceModel? ProvidedParentScope { get; init; }

    internal EquatableArray<RegistrationModel> Bindings { get; init; }
}

internal enum ContainerKind : byte {
    ApplicationRoot,
    SceneRoot,
    SubScope
}

internal enum ParentDiscoveryKind : byte {
    Static = 0,
    Hierarchy = 1,
    Ambient = 2
}