using Surject.Generators.Models.Primitives;

namespace Surject.Generators.Models.Concepts;

internal readonly record struct ServiceModel {
    internal ITypeReferenceModel TypeRef { get; init; }
    internal ServiceCreationModel CreationModel { get; init; }
}