using Surject.Generators.Models.Collections;

namespace Surject.Generators.Models.Primitives;

internal sealed record ConstraintsModel {
    internal ConstraintsModel(string typeName, EquatableArray<string> values) {
        TypeName = typeName;
        Values = values;
    }

    public override string ToString() {
        return $"where {TypeName} : {string.Join(", ", Values)}";
    }

    internal string TypeName { get; }
    internal EquatableArray<string> Values { get; }
}