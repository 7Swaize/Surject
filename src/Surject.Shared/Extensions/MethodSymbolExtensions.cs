using Microsoft.CodeAnalysis;

public static class MethodSymbolExtensions {
    extension(IMethodSymbol self) {
        public string NameWithoutGenericParameters => self.Name;

        public string NameWithGenericParameters {
            get {
                SymbolDisplayFormat format = new SymbolDisplayFormat(
                    genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
                );
        
                return self.ToDisplayString(format);
            }
        }
    }
}