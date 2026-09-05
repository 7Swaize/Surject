using System.Collections.Immutable;
using Surject.Generators.Models.Collections;

namespace Surject.Generators.Models.Concepts;

internal sealed record RegistrationModel {
    internal RegistrationModel(in EntryCommandModel entry, ImmutableArray<ModifierCommandModel> modifiers) {
        _entry = entry;
        Modifiers = modifiers;
    }

    private readonly EntryCommandModel _entry;
    
    internal ref readonly EntryCommandModel Entry => ref _entry;
    internal EquatableArray<ModifierCommandModel> Modifiers { get; init; }
}