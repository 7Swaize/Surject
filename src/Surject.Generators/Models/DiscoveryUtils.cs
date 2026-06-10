using Microsoft.CodeAnalysis;
using Surject.Generators.Models.Factories;

namespace Surject.Generators.Models;

internal sealed record DiscoveryUtils {
    internal DiscoveryUtils(Compilation compilation) {
        TypeReferenceModelFactory = new TypeReferenceModelFactory();
    }
    
    internal TypeReferenceModelFactory TypeReferenceModelFactory { get; init; }
}