using Microsoft.CodeAnalysis.CSharp.Syntax;

public static class InvocationExpressionSyntaxExtensions {
    extension(InvocationExpressionSyntax self) {
        public bool IsPartOfLargerChain() {
            return self.Parent is MemberAccessExpressionSyntax member &&
                   member.Parent is InvocationExpressionSyntax;
        }
    }
}