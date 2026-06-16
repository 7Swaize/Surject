using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Surject.Generators.Discovery.ServiceRegistration;

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