using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Surject.Generators.Models.Primitives;

internal static class NamedTypeSymbolExtensions {
    extension(INamedTypeSymbol self) {
        internal ImmutableArray<ConstraintsModel> GetTypeParamConstraints() {
            if (self.TypeParameters.IsEmpty) {
                return ImmutableArray<ConstraintsModel>.Empty;
            }
            
            ImmutableArray<ConstraintsModel>.Builder builder = ImmutableArray.CreateBuilder<ConstraintsModel>();

            foreach (ITypeParameterSymbol typeParameterSymbol in self.TypeParameters) {
                ConstraintsModel? constraints = typeParameterSymbol.GetTypeParamConstraints();
                if (constraints != null) {
                    builder.Add(constraints);
                }
            }

            return builder.ToImmutable();
        }
    }
}