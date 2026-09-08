using Surject.Generators.Models.Primitives;

namespace Surject.Generators.Models.Concepts;

internal readonly record struct RewrittenDelegateArgumentModel {
    internal RewrittenDelegateArgumentKind Kind { get; init; }
    internal MethodModel Method { get; init; }
    internal string? RewrittenInternals { get; init; }
}


internal enum RewrittenDelegateArgumentKind : byte {
    LambdaExpr,
    MethodGroup
}