using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Factories;

namespace Surject.Generators.Discovery.ServiceRegistration;

internal static class RegistrationBindingParser {
    internal static RegistrationModel? Parse(
        InvocationExpressionSyntax rootInvocation,
        TypeReferenceModelFactory typeRefFactory,
        SemanticModel semanticModel)
    {
        if (!TryExtractChain())
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
        modifiers = SpanHe
    }
}