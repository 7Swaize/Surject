using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

public static class SymbolExtensions {
    extension(ISymbol self) {
        public bool ValidateAnnotatedWith(INamedTypeSymbol annotation) {
            foreach (AttributeData attribute in self.GetAttributes()) {
                if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, annotation)) {
                    return true;
                }
            }
        
            return false;
        }
        
        public bool ValidateAnnotatedWith(INamedTypeSymbol annotation, [NotNullWhen(true)] out AttributeData? attributeData) {
            foreach (AttributeData attribute in self.GetAttributes()) {
                if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, annotation)) {
                    attributeData = attribute;
                    return true;
                }
            }
        
            attributeData = null;
            return false;
        }
        
        public bool ValidateAnnotatedWith(ImmutableArray<AttributeData> attributes, INamedTypeSymbol attribute) {
            foreach (AttributeData attributeSymbol in attributes) {
                if (SymbolEqualityComparer.Default.Equals(attributeSymbol.AttributeClass, attribute)) {
                    return true;
                }
            }
        
            return false;   
        }
        
        public bool ValidateAnnotatedWith(
            ImmutableArray<AttributeData> attributes,
            INamedTypeSymbol attribute,
            [NotNullWhen(true)] out AttributeData? attributeData)
        {
            foreach (AttributeData attributeSymbol in attributes) {
                if (SymbolEqualityComparer.Default.Equals(attributeSymbol.AttributeClass, attribute)) {
                    attributeData = attributeSymbol;
                    return true;
                }
            }
        
            attributeData = null;
            return false;   
        }
    }
}