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
        INamedTypeSymbol containerSymbol = context.TargetSymbol.As<INamedTypeSymbol>();
        TypeReferenceModelFactory typeRefFactory = TypeReferenceModelFactory.GetFactory(context.SemanticModel.Compilation);

        IMethodSymbol configureMethod = containerSymbol
            .GetMembers()
            .OfType<IMethodSymbol>()
            .First(m => m.Name == nameof(ScopeContext.Configure));

        IEnumerable<InvocationExpressionSyntax> chainRoots = configureMethod.DeclaringSyntaxReferences[0]
            .GetSyntax()
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Where(inv => !inv.IsPartOfLargerChain());

        List<RegistrationModel> registrations = [];
        HashSet<ITypeReferenceModel> entryTypes = [];
        UniqueEntryBindingTypeVisitor visitor = new();

        foreach (InvocationExpressionSyntax invocation in chainRoots) {
            RegistrationModel? model = RegistrationBindingParser.Parse(invocation, typeRefFactory, context.SemanticModel);
            if (model is null) {
                continue;
            }

            registrations.Add(model);

            if (model.Entry.Accept<UniqueEntryBindingTypeVisitor, ITypeReferenceModel?>(ref visitor) is { } entryType) {
                entryTypes.Add(entryType);
            }
        }

        Bindings = [.. registrations];
        UniqueEntryTypes = [.. entryTypes];
        Decl = new TypeDeclModel(containerSymbol, typeRefFactory);
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
    internal EquatableArray<ITypeReferenceModel> UniqueEntryTypes { get; init; }
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