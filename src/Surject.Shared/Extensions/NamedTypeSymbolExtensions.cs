using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public static class NamedTypeSymbolExtensions {
    extension(INamedTypeSymbol self) {
        public bool IsOpenGeneric() => 
            self.TypeParameters.Any(typeSymbol => typeSymbol.TypeKind == TypeKind.TypeParameter);
        
        public bool IsPartialDeclaration() {
            return self.DeclaringSyntaxReferences.Any(syntax =>
                syntax.GetSyntax() is BaseTypeDeclarationSyntax declaration
                && declaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword))
            );
        }
    }
}