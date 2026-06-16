
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Surject.Generators.Discovery;

internal static class CompilationExtensions {
    private static readonly ConditionalWeakTable<Compilation, DiscoveryUtils> _utilityCache = new();

    extension(Compilation compilation) {
        internal DiscoveryUtils GetDiscoveryUtils() =>
            _utilityCache.GetValue(compilation, static comp => new DiscoveryUtils(comp));
    }
}