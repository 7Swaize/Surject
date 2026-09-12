using Surject.Generators.Models.Collections;

namespace Surject.Generators.Models.Concepts;

internal sealed record RegistrationModel {
    internal RegistrationModel(
        in EntryCommandModel entry, 
        ModifierKind modifiersDescriptor,
        EquatableArray<ModifierCommandModel> modifiers)
    {
        _entry = entry;
        
        ModifiersDescriptor = modifiersDescriptor;
        Modifiers = modifiers;
    }

    private readonly EntryCommandModel _entry;
    
    internal ref readonly EntryCommandModel Entry => ref _entry;
    internal ModifierKind ModifiersDescriptor { get; init; }
    internal EquatableArray<ModifierCommandModel> Modifiers { get; init; }
}