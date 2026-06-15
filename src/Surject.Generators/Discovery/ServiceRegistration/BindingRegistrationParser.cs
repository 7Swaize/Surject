using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Surject.Generators.Models;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;
using Surject.Shared.Helpers;

namespace Surject.Generators.Discovery.ServiceRegistration;

internal static class BindingRegistrationParser {
    internal static BindingModel Parse(InvocationExpressionSyntax rootInvocation, DiscoveryUtils utils, SemanticModel semanticModel) {
        if (!TryExtractChain(rootInvocation, out var outermost, out Span<InvocationExpressionSyntax> modSyntax)) {
            return null!;
        }

        EntryCommandModel entry = ParseEntry(outermost, utils, semanticModel);
        ImmutableArray<ModifierCommandModel> modifiers = ParseModifiers(modSyntax, utils, semanticModel);
        
        RegistrationNormalizer normalizer = new RegistrationNormalizer(modifiers);
        return entry.Accept<RegistrationNormalizer, BindingModel>(ref normalizer);
    }
    
    private static bool TryExtractChain(
        InvocationExpressionSyntax root,
        out InvocationExpressionSyntax entry,
        out Span<InvocationExpressionSyntax> modifiers)
    {
        List<InvocationExpressionSyntax> chain = new(9);
        ExpressionSyntax? current = root;

        while (current is InvocationExpressionSyntax inv) {
            chain.Add(inv);
            current = inv.Expression is MemberAccessExpressionSyntax ma
                ? ma.Expression
                : null;
        }

        if (chain.Count == 0) {
            entry = root;
            modifiers = default;
            return false;
        }

        entry = chain[^1];
        modifiers = SpanHelpers.AsSpan(chain)[..^1];
        modifiers.Reverse();
        return true;
    }

    private static EntryCommandModel ParseEntry(
        InvocationExpressionSyntax syntax,
        DiscoveryUtils utils,
        SemanticModel semanticModel) 
    {
        if (ModelExtensions.GetSymbolInfo(semanticModel, syntax).Symbol is not IMethodSymbol method) {
            return default;
        }

        if (!IsRegistryMethod(method, utils)) {
            return default;
        }

        LifetimeKind lifetime = ExtractLifetime(syntax, semanticModel);
        ITypeReferenceModel? implType = ExtractNthTypeArg(method, 0, utils);

        return method.Name switch {
            "Add" => EntryCommandModel.Add(implType!, lifetime),
            "AddFactory" => EntryCommandModel.AddFactory(
                implType!,
                lifetime,
                InvokeAnonymousExprFQNRewriter(ExtractAnonymousExpr(syntax, 1), semanticModel)
            ),
            "AddOpenGeneric" => EntryCommandModel.AddOpenGeneric(
                ExtractNthArgTypeOf(syntax, 0, utils, semanticModel),
                lifetime
            ),
            "AddToCollection" => EntryCommandModel.AddToCollection(
                implType!,
                lifetime,
                ExtractNthIntArg(syntax, 1, semanticModel)
            ),
            "AddPrimaryToCollection" => EntryCommandModel.AddPrimaryToCollection(
                implType!,
                lifetime,
                ExtractNthIntArg(syntax, 1, semanticModel)
            ),
            "AddAsyncFactory" => EntryCommandModel.AddAsyncFactory(
                implType!,
                lifetime,
                InvokeAnonymousExprFQNRewriter(ExtractAnonymousExpr(syntax, 1), semanticModel)
            ),
            "AddFromHierarchy" => EntryCommandModel.AddFromHierarchy(implType!, lifetime),
            "AddAllFromHierarchy" => EntryCommandModel.AddAllFromHierarchy(implType!, lifetime),
            "AddFromSibling" => EntryCommandModel.AddFromSibling(implType!, lifetime),
            "AddFromChildren" => EntryCommandModel.AddAllFromChildren(implType!, lifetime),
            "AddAllFromChildren" => EntryCommandModel.AddAllFromChildren(implType!, lifetime),
            "AddFromParent" => EntryCommandModel.AddFromParent(implType!, lifetime),
            "AddAllFromParent" => EntryCommandModel.AddAllFromParent(implType!, lifetime),
            "AddNewComponent" => EntryCommandModel.AddNewComponent(implType!, lifetime),
            "AddFromPrefab" => EntryCommandModel.AddFromPrefab(
                implType!,
                lifetime,
                ExtractNthArgAsString(syntax, 1)
            ),
            "Decorate" => EntryCommandModel.Decorate(
                contract: ExtractNthTypeArg(method, 0, utils)!,
                decorator: ExtractNthTypeArg(method, 1, utils)!
            ),
            _ => throw new InvalidOperationException($"Unhandled Entry method name '{method.Name}'.")
        };
    }

    private static ImmutableArray<ModifierCommandModel> ParseModifiers(
        Span<InvocationExpressionSyntax> modSyntax,
        DiscoveryUtils utils,
        SemanticModel semanticModel) 
    {
        if (modSyntax.IsEmpty) return ImmutableArray<ModifierCommandModel>.Empty;
        
        ImmutableArray<ModifierCommandModel>.Builder builder = 
            ImmutableArray.CreateBuilder<ModifierCommandModel>(modSyntax.Length);
        foreach (InvocationExpressionSyntax syntax in modSyntax) {
            builder.Add(ParseModifier(syntax, utils, semanticModel));
        }

        return builder.MoveToImmutable();
    }

    private static ModifierCommandModel ParseModifier(
        InvocationExpressionSyntax syntax,
        DiscoveryUtils utils,
        SemanticModel semanticModel)
    {
        if (semanticModel.GetSymbolInfo(syntax).Symbol is not IMethodSymbol method) {
            return default;
        }

        return method.Name switch {
            "To" => ModifierCommandModel.To(
                ExtractNthTypeArg(method, 0, utils) ?? ExtractNthArgTypeOf(syntax, 0, utils, semanticModel)
            ),
            "ToImplementedInterfaces" => ModifierCommandModel.ToImplementedInterfaces(),
            "WithId" => ModifierCommandModel.WithId(ExtractNthStringArg(syntax, 0, semanticModel)),
            "Pooled" => ModifierCommandModel.Pooled(ExtractNthIntArg(syntax, 0, semanticModel)),
            "Eager" => ModifierCommandModel.Eager(),
            "Lazy" => ModifierCommandModel.Lazy(),
            "FromFactory" => ModifierCommandModel.FromFactory(
                InvokeAnonymousExprFQNRewriter(ExtractAnonymousExpr(syntax, 1), semanticModel)
            ),
            "FromInjectFactory" => ModifierCommandModel.FromInjectFactory(),
            "WithArgument" => ModifierCommandModel.WithArgument(
                ExtractNthStringArg(syntax, 0, semanticModel),
                ExtractNthTypeArg(method, 0, utils)!,
                ExtractNthArgAsString(syntax, 1)
            ),
            "WhenInjectedInto" => ModifierCommandModel.WhenInjectedInto(ExtractNthTypeArg(method, 0, utils)!),
            "When" => ModifierCommandModel.When(
                InvokeAnonymousExprFQNRewriter(ExtractAnonymousExpr(syntax, 1), semanticModel)
            ),
            "OverrideExisting" => ModifierCommandModel.OverrideExisting(),
            "AsCollection" => ModifierCommandModel.AsCollection(ExtractNthIntArg(syntax, 0, semanticModel)),
            "AsPrimary" => ModifierCommandModel.AsPrimary(),
            "DoNotDispose" => ModifierCommandModel.DoNotDispose(),
            "TrackDisposable" => ModifierCommandModel.TrackDisposable(),
            
            // Unity component specific
            "UnderTransform" => ModifierCommandModel.UnderTransform(ExtractNthArgAsString(syntax, 0)),
            "UnderObjectOfType" => ModifierCommandModel.UnderObjectOfType(ExtractNthTypeArg(method, 0, utils)!),
            "WithGameObjectName" => ModifierCommandModel.WithGameObjectName(ExtractNthStringArg(syntax, 0, semanticModel)),
            "DoNotDestroy" => ModifierCommandModel.DoNotDestroy(),
            _ => throw new InvalidOperationException($"Unhandled Modifier method name '{method.Name}'.")
        };
    }

    private static bool IsRegistryMethod(IMethodSymbol method, DiscoveryUtils utils) {
        INamedTypeSymbol? type = method.ContainingType;
        if (!SymbolEqualityComparer.Default.Equals(type, utils.ReferenceSymbols.IServiceRegistry)) {
            return false;
        }
        
        return true;
    }
    
    private static LifetimeKind ExtractLifetime(InvocationExpressionSyntax syntax, SemanticModel semanticModel) {
        Optional<object?> constant = semanticModel.GetConstantValue(
            syntax.ArgumentList.Arguments[0].Expression);

        return constant.HasValue
            ? (LifetimeKind)(byte)constant.Value!
            : default; // We don't necessarily care about failure
    }

    private static ITypeReferenceModel? ExtractNthTypeArg(IMethodSymbol method, int index, DiscoveryUtils utils) {
        return method.TypeArguments.Length > index
            ? utils.TypeReferenceModelFactory.CreateOrGetTypeReferenceModel(method.TypeArguments[index])
            : null;
    }

    private static AnonymousFunctionExpressionSyntax ExtractAnonymousExpr(InvocationExpressionSyntax syntax, int index) {
        return (AnonymousMethodExpressionSyntax)syntax.ArgumentList.Arguments[index].Expression;
    }

    private static ITypeReferenceModel ExtractNthArgTypeOf(
        InvocationExpressionSyntax syntax,
        int index,
        DiscoveryUtils utils,
        SemanticModel semanticModel)
    {
        TypeOfExpressionSyntax @typeof = (TypeOfExpressionSyntax)syntax.ArgumentList.Arguments[index].Expression;
        return utils.TypeReferenceModelFactory.CreateOrGetTypeReferenceModel(
            (ITypeSymbol)semanticModel.GetSymbolInfo(@typeof.Type).Symbol!
        );
    }

    private static int ExtractNthIntArg(InvocationExpressionSyntax syntax, int index, SemanticModel semanticModel) {
        Optional<object?> constant = semanticModel.GetConstantValue(
            syntax.ArgumentList.Arguments[index].Expression);

        return constant.HasValue
            ? (int)constant.Value!
            : 0; // We don't necessarily care about failure
    }

    private static string ExtractNthStringArg(InvocationExpressionSyntax syntax, int index, SemanticModel semanticModel) {
        Optional<object?> constant = semanticModel.GetConstantValue(
            syntax.ArgumentList.Arguments[index].Expression);
        
        return constant.HasValue
            ? (string)constant.Value!
            : ""; // We don't necessarily care about failure
    }

    private static string ExtractNthArgAsString(InvocationExpressionSyntax syntax, int index) {
        return syntax.ArgumentList.Arguments[index].Expression.ToString();
    }

    private static string InvokeAnonymousExprFQNRewriter(AnonymousFunctionExpressionSyntax expr, SemanticModel semanticModel) {
        AnonymousExprFQNRewriter rewriter = new AnonymousExprFQNRewriter(semanticModel);
        AnonymousFunctionExpressionSyntax rewritten = (AnonymousFunctionExpressionSyntax)rewriter.Visit(expr);
        return rewritten.ToFullString();
    }
}


internal sealed class AnonymousExprFQNRewriter : CSharpSyntaxRewriter {
    private readonly SemanticModel _semanticModel;
    
    internal AnonymousExprFQNRewriter(SemanticModel semanticModel) {
        _semanticModel = semanticModel;
    }

    public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node) {
        ISymbol? symbol = _semanticModel.GetSymbolInfo(node).Symbol;

        if (symbol is INamedTypeSymbol named) {
            string fqn = named.GetConstructedTypeFQN();
            return SyntaxFactory.ParseTypeName(fqn).WithTriviaFrom(node);
        }
        
        return base.VisitIdentifierName(node);
    }

    public override SyntaxNode? VisitGenericName(GenericNameSyntax node) {
        ISymbol? symbol = _semanticModel.GetSymbolInfo(node).Symbol;

        if (symbol is INamedTypeSymbol named) {
            string fqn = named.GetConstructedTypeFQN();
            return SyntaxFactory.ParseTypeName(fqn).WithTriviaFrom(node);
        }
        
        return base.VisitGenericName(node);
    }

    public override SyntaxNode? VisitArrayType(ArrayTypeSyntax node) {
        ISymbol? symbol = _semanticModel.GetSymbolInfo(node).Symbol;
        
        if (symbol is IArrayTypeSymbol array) {
            string fqn = array.GetConstructedTypeFQN();
            return SyntaxFactory.ParseTypeName(fqn).WithTriviaFrom(node);
        }
        
        return base.VisitArrayType(node);   
    }
}