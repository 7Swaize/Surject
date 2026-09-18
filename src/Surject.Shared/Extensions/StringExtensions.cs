using System.Text.RegularExpressions;

namespace Surject.Shared.Extensions;

public static class StringExtensions {
    extension(string self) {
        public string CollapseRedundantWhitespace() {
            return Regex.Replace(self, @"[ \t]+", " ");
        }
    }
}