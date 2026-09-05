using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Surject.Generators.Models.Primitives;

internal static class TypeParameterSymbolExtensions {
    extension(ITypeParameterSymbol self) {
        internal ConstraintsModel? GetTypeParamConstraints() {
            List<string> constraints = [];
        
            if (self.HasUnmanagedTypeConstraint) {
                constraints.Add("unmanaged");
            }

            if (self.HasNotNullConstraint) {
                constraints.Add("notnull");
            }

            if (self.HasReferenceTypeConstraint) {
                constraints.Add(self.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.Annotated
                    ? "class?"
                    : "class");
            }

            if (self.HasValueTypeConstraint) {
                constraints.Add("struct");
            }
        
            constraints.AddRange(self.ConstraintTypes
                .Where(static typeSymbol => typeSymbol.TypeKind == TypeKind.Class)
                .Select(static typeSymbol => typeSymbol.GetConstructedTypeFQN()) // with generic arguments if any
                .OrderBy(static typeSymbol => typeSymbol)
            );
            constraints.AddRange(self.ConstraintTypes
                .Where(static typeSymbol => typeSymbol.TypeKind == TypeKind.Interface)
                .Select(static typeSymbol => typeSymbol.GetConstructedTypeFQN()) // with generic arguments if any
                .OrderBy(static typeSymbol => typeSymbol)
            );
            constraints.AddRange(self.ConstraintTypes
                .Where(static typeSymbol => typeSymbol.TypeKind == TypeKind.TypeParameter)
                .Select(static typeSymbol => typeSymbol.GetConstructedTypeFQN()) // with generic arguments if any
                .OrderBy(static typeSymbol => typeSymbol)
            );
        
            if (self.HasConstructorConstraint) {
                constraints.Add("new()");
            }
        
            // no exposing of 'allows ref struct' type constraint?
        
            return constraints.Count > 0 ? new ConstraintsModel(self.Name, [.. constraints ]) : null;
        }
    }
}