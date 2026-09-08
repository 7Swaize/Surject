using Surject.Generators.Discovery.ServiceRegistration;
using Surject.Generators.Models.Primitives;
using Surject.Shared.Helpers;

namespace Surject.Generators.Models.Concepts;

internal readonly record struct EntryCommandModel {
    internal EntryKind Kind { get; init; }
    internal ServiceModel Service { get; init; }
    
    internal LifetimeKind Lifetime { get; init; }
    internal ITypeReferenceModel AuxType1 { get; init; }
    internal ITypeReferenceModel AuxType2 { get; init; }

    internal RewrittenDelegateArgumentModel RewrittenDelegateArgument { get; init; }
    
    internal string PrefabArg { get; init; }
    internal int OrderHint { get; init; }
    
    internal static EntryCommandModel Add(in ServiceModel service, LifetimeKind lifetime) =>
        new() { Kind = EntryKind.Add, Service = service, Lifetime = lifetime };

    internal static EntryCommandModel AddFactory(ITypeReferenceModel impl, LifetimeKind lifetime, in RewrittenDelegateArgumentModel func) =>
        new() { Kind = EntryKind.AddFactory, AuxType1 = impl, Lifetime = lifetime, RewrittenDelegateArgument = func };

    internal static EntryCommandModel AddOpenGeneric(in ServiceModel service, LifetimeKind lifetime) =>
        new() { Kind = EntryKind.AddOpenGeneric, Service = service, Lifetime = lifetime };

    internal static EntryCommandModel AddToCollection(in ServiceModel service, LifetimeKind lifetime, int order = 0) =>
        new() { Kind = EntryKind.AddToCollection, Service = service, Lifetime = lifetime, OrderHint = order };

    internal static EntryCommandModel AddPrimaryToCollection(in ServiceModel service, LifetimeKind lifetime, int order = 0) =>
        new() { Kind = EntryKind.AddPrimaryToCollection, Service = service, Lifetime = lifetime, OrderHint = order };
    
    internal static EntryCommandModel AddAsyncFactory(ITypeReferenceModel impl, LifetimeKind lifetime, in RewrittenDelegateArgumentModel func) =>
        new() { Kind = EntryKind.AddAsyncFactory, AuxType1 = impl, Lifetime = lifetime, RewrittenDelegateArgument = func };

    internal static EntryCommandModel AddFromHierarchy(in ServiceModel service, LifetimeKind lifetime) =>
        new() { Kind = EntryKind.AddFromHierarchy, Service = service, Lifetime = lifetime };

    internal static EntryCommandModel AddAllFromHierarchy(in ServiceModel service, LifetimeKind lifetime) =>
        new() { Kind = EntryKind.AddAllFromHierarchy, Service = service, Lifetime = lifetime };

    internal static EntryCommandModel AddFromSibling(in ServiceModel service, LifetimeKind lifetime) =>
        new() { Kind = EntryKind.AddFromSibling, Service = service, Lifetime = lifetime };

    internal static EntryCommandModel AddFromChildren(in ServiceModel service, LifetimeKind lifetime) =>
        new() { Kind = EntryKind.AddFromChildren, Service = service, Lifetime = lifetime };

    internal static EntryCommandModel AddAllFromChildren(in ServiceModel service, LifetimeKind lifetime) =>
        new() { Kind = EntryKind.AddAllFromChildren, Service = service, Lifetime = lifetime };

    internal static EntryCommandModel AddFromParent(in ServiceModel service, LifetimeKind lifetime) =>
        new() { Kind = EntryKind.AddFromParent, Service = service, Lifetime = lifetime };

    internal static EntryCommandModel AddAllFromParent(in ServiceModel service, LifetimeKind lifetime) =>
        new() { Kind = EntryKind.AddAllFromParent, Service = service, Lifetime = lifetime };

    internal static EntryCommandModel AddNewComponent(in ServiceModel service, LifetimeKind lifetime) =>
        new() { Kind = EntryKind.AddNewComponent, Service = service, Lifetime = lifetime };

    internal static EntryCommandModel AddFromPrefab(in ServiceModel service, LifetimeKind lifetime, string prefabArg) =>
        new() { Kind = EntryKind.AddFromPrefab, Service = service, Lifetime = lifetime, PrefabArg = prefabArg };

    internal TResult Accept<TVisitor, TResult>(ref TVisitor visitor) where TVisitor : struct, IEntryCommandVisitor<TResult> {
        return Kind switch {
            EntryKind.Add => visitor.VisitAdd(this),
            EntryKind.AddFactory => visitor.VisitAddFactory(this),
            EntryKind.AddOpenGeneric => visitor.VisitAddOpenGeneric(this),
            EntryKind.AddAsyncFactory => visitor.VisitAddAsyncFactory(this),
            EntryKind.AddToCollection => visitor.VisitAddToCollection(this),
            EntryKind.AddPrimaryToCollection => visitor.VisitAddPrimaryToCollection(this),
            EntryKind.AddFromHierarchy => visitor.VisitAddFromHierarchy(this),
            EntryKind.AddAllFromHierarchy => visitor.VisitAddAllFromHierarchy(this),
            EntryKind.AddFromSibling => visitor.VisitAddFromSibling(this),
            EntryKind.AddFromChildren => visitor.VisitAddFromChildren(this),
            EntryKind.AddAllFromChildren => visitor.VisitAddAllFromChildren(this),
            EntryKind.AddFromParent => visitor.VisitAddFromParent(this),
            EntryKind.AddAllFromParent => visitor.VisitAddAllFromParent(this),
            EntryKind.AddNewComponent => visitor.VisitAddNewComponent(this),
            EntryKind.AddFromPrefab => visitor.VisitAddFromPrefab(this),
            _ => ThrowHelpers.ThrowUnhandledBranch<TResult>(Kind)
        };
    }
}

internal enum EntryKind : byte {
    Add,
    AddFactory,
    AddOpenGeneric,
    AddAsyncFactory,
    
    AddToCollection,
    AddPrimaryToCollection,
    
    AddFromHierarchy,
    AddAllFromHierarchy,
    AddFromSibling,
    AddFromChildren,
    AddAllFromChildren,
    AddFromParent,
    AddAllFromParent,
    
    AddNewComponent,
    AddFromPrefab
}

internal enum LifetimeKind : byte {
    Transient = 0,
    Singleton = 1
}