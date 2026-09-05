using Microsoft.CodeAnalysis;
using Surject.Generators.Models.Factories;

namespace Surject.Generators.Models.Primitives;

internal sealed record TypeDeclModel {
    internal TypeDeclModel(INamedTypeSymbol symbol, TypeReferenceModelFactory typeRefFactory) {
        AsTypeRef = typeRefFactory.CreateOrGetTypeReferenceModel(symbol);
        AccessModifier = symbol.DeclaredAccessibility;
        IsPartial = symbol.IsPartialDeclaration();
        IsStatic = symbol.IsStatic;
        IsSealed = symbol.IsSealed;
        
        TypeNameNoArityNoFQN = symbol.Name;
    }
    
    internal ITypeReferenceModel AsTypeRef { get; init; }
    internal Accessibility AccessModifier { get; init; }
    internal bool IsPartial { get; init; }
    internal bool IsStatic { get; init; }
    internal bool IsSealed { get; init; }
    
    internal string TypeNameNoArityNoFQN { get; init; }
}