using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

public static class TypeSymbolExtensions {
    private static readonly SymbolDisplayFormat GenericDefinitionFQNGlobal = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
    );

    private static readonly SymbolDisplayFormat GenericDefinitionFQNNoGlobal = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
    );

    private static readonly SymbolDisplayFormat ConstructedTypeFQNGlobal = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions:
            SymbolDisplayGenericsOptions.IncludeTypeParameters |
            SymbolDisplayGenericsOptions.IncludeVariance,
        miscellaneousOptions:
            SymbolDisplayMiscellaneousOptions.ExpandNullable |
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier |
            SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers
    );

    private static readonly SymbolDisplayFormat ConstructedTypeFQNNoGlobal = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions:
            SymbolDisplayGenericsOptions.IncludeTypeParameters |
            SymbolDisplayGenericsOptions.IncludeVariance,
        miscellaneousOptions:
            SymbolDisplayMiscellaneousOptions.ExpandNullable |
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier |
            SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers
    );

    private static readonly SymbolDisplayFormat GenericArityFQNGlobal = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.None
    );

    private static readonly SymbolDisplayFormat GenericArityFQNNoGlobal = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.None
    );
    
    
    extension(ITypeSymbol self) {
        /// <summary>
        /// Fully-qualified generic type definition.
        /// Example: global::System.Collections.Generic.Dictionary&lt;TKey, TValue&gt;
        /// </summary>
        public string GetGenericDefinitionFQN(bool includeGlobal = true) {
            return self.ToDisplayString(
                includeGlobal ? GenericDefinitionFQNGlobal : GenericDefinitionFQNNoGlobal
            );
        }

        /// <summary>
        /// Fully-qualified constructed generic type.
        /// Example: global::System.Collections.Generic.Dictionary&lt;string, int&gt;
        /// </summary>
        public string GetConstructedTypeFQN(bool includeGlobal = true) {
            return self.ToDisplayString(
                includeGlobal ? ConstructedTypeFQNGlobal : ConstructedTypeFQNNoGlobal
            );
        }
        
        /// <summary>
        /// Fully-qualified generic arity name.
        /// Example: global::System.Collections.Generic.Dictionary&lt;,&gt;
        /// </summary>
        public string GetGenericArityFQN(bool includeGlobal = true) {
            if (self is not INamedTypeSymbol named || !named.IsGenericType) {
                return self.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            }

            string baseName = named.ToDisplayString(
                includeGlobal ? GenericArityFQNGlobal : GenericArityFQNNoGlobal
            );

            return $"{baseName}<{new string(',', named.Arity - 1)}>";
        }
        
        /// <summary>
        /// Flattens constructed generic arguments into the name.
        /// Example: DictionaryStringInt32
        /// </summary>
        public string GetFlattenedConstructedName() {
            if (self is not INamedTypeSymbol named)
                return self.Name;

            StringBuilder sb = new StringBuilder();
            sb.Append(named.Name);

            if (named.IsGenericType) {
                foreach (ITypeSymbol arg in named.TypeArguments) {
                    sb.Append(arg.Name);
                }
            }

            return sb.ToString();
        }
        
        public string GetFQNWithGenericsOmitted(bool includeGlobal = true) {
            return self.ToDisplayString(
                includeGlobal ? GenericArityFQNGlobal : GenericArityFQNNoGlobal
            );
        }
        
        public bool InheritsFromClass(ITypeSymbol toFind) {
            ITypeSymbol? selfBase = self.BaseType;

            for (ITypeSymbol? current = selfBase; current != null; current = current.BaseType) {
                if (SymbolEqualityComparer.Default.Equals(current, toFind)) {
                    return true;
                }
            }

            return false;
        }
        
        public bool IsOrInheritsFromClass(ITypeSymbol toFind) =>
            SymbolEqualityComparer.Default.Equals(self, toFind) || InheritsFromClass(self, toFind);
        
        public bool IsReferenceAssignableFrom(ITypeSymbol target, Compilation compilation) {
            if (SymbolEqualityComparer.Default.Equals(self, target)) {
                return true;
            }

            // to handle unbound generic types
            if (target is INamedTypeSymbol { IsUnboundGenericType: true } nTarget) {
                for (ITypeSymbol? current = self; current is not null; current = current.BaseType) {
                    if (current is INamedTypeSymbol { IsGenericType: true } nCurrent &&
                        SymbolEqualityComparer.Default.Equals(nCurrent.OriginalDefinition, nTarget.OriginalDefinition))
                    {
                        return true;
                    }
                }

                foreach (INamedTypeSymbol iface in self.AllInterfaces) {
                    if (SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, nTarget.OriginalDefinition)) {
                        return true;
                    }
                }

                return false;
            }

            Conversion conversion = compilation.ClassifyConversion(self, target);
            return conversion.IsImplicit && conversion.IsReference;
        }
        
        public ITypeSymbol SubstituteFrom(Dictionary<ITypeSymbol, ITypeSymbol> contextMap) {
            if (self is ITypeParameterSymbol tps) {
                return contextMap.GetValueOrDefault(tps, self)!;
            }
            
            if (self is INamedTypeSymbol named) {
                ITypeSymbol[] newArgs = named.TypeArguments
                    .Select(arg => arg.SubstituteFrom(contextMap))
                    .ToArray();
        
                if (newArgs.SequenceEqual(named.TypeArguments, SymbolEqualityComparer.Default)) {
                    return self;
                }

                if (named.ConstructUnboundGenericType() is { } unboundNamed) {
                    return unboundNamed.OriginalDefinition.Construct(newArgs);
                }
            }
    
            // For arrays, pointers, etc.
            return self;
        }
    }
}