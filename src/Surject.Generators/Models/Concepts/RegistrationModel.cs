using System;
using System.Collections.Immutable;
using Surject.Generators.Discovery.ServiceRegistration;
using Surject.Generators.Models.Collections;

namespace Surject.Generators.Models.Concepts;

internal record RegistrationModel {
    internal RegistrationModel(in EntryCommandModel entry, in ImmutableArray<ModifierCommandModel> modifiers) {
        Entry = entry;
        Modifiers = modifiers;
        CacheBuilderFlags = BindingRegistrationParser.GetCacheBuilderFlags(in entry, modifiers);
    }
    
    internal EntryCommandModel Entry { get; init; }
    internal EquatableArray<ModifierCommandModel> Modifiers { get; init; }
    
    internal CacheBuilderFlags CacheBuilderFlags { get; init; }
}

[Flags]
internal enum CacheBuilderFlags : byte {
    None = 0,
    MultiBind = 1 << 0,
    Lazy = 1 << 1,
    Async = 1 << 2,
    Keyed = 1 << 3,
    Primary = 1 << 4
}