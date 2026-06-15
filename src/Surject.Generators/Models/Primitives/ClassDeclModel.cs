using Microsoft.CodeAnalysis;

namespace Surject.Generators.Models.Primitives;

internal record ClassDeclModel {
    internal ClassDeclModel(INamedTypeSymbol symbol, DiscoveryUtils uGrouping) {
        ClassAsTypeRef = uGrouping.TypeReferenceModelFactory.CreateOrGetTypeReferenceModel(symbol);
        AccessModifier = symbol.DeclaredAccessibility;
        IsPartial = symbol.IsPartialDeclaration();
        IsStatic = symbol.IsStatic;
        IsSealed = symbol.IsSealed;
        
        FQNNoGlobal = symbol.GetConstructedTypeFQN(false);
    }
    
    internal ITypeReferenceModel ClassAsTypeRef { get; init; }
    internal Accessibility AccessModifier { get; init; }
    internal bool IsPartial { get; init; }
    internal bool IsStatic { get; init; }
    internal bool IsSealed { get; init; }
    
    internal string FQNNoGlobal { get; init; }
}