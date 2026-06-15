using System.Collections.Immutable;
using Surject.Generators.Models.Collections;

namespace Surject.Generators.Models.Concepts;

internal record BindingModel {
    internal BindingModel(EntryCommandModel entry, ImmutableArray<ModifierCommandModel> modifiers) {
        Entry = entry;
        Modifiers = modifiers;
    }
    
    internal EntryCommandModel Entry { get; init; }
    internal EquatableArray<ModifierCommandModel> Modifiers { get; init; }
}