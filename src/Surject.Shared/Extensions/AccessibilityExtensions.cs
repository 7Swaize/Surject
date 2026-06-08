using Microsoft.CodeAnalysis;

public static class AccessibilityExtensions {
    extension(Accessibility accessibility) {
        public string AsDeclString() =>
            accessibility switch {
                Accessibility.Public => "public",
                Accessibility.Protected => "protected",
                Accessibility.Internal => "internal",
                Accessibility.Private => "private",
                Accessibility.ProtectedAndInternal => "private protected",
                Accessibility.ProtectedOrInternal => "protected internal",
                _ => string.Empty
            };
    }
}