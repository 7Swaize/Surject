using System;
using System.Diagnostics.CodeAnalysis;
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

    internal string FuncExpr { get; init; }
    internal string PrefabArg { get; init; }

    internal int OrderHint { get; init; }

    internal static EntryCommandModel Add(ServiceModel service, LifetimeKind lifetime) =>
        new() { Kind = EntryKind.Add, Service = service, Lifetime = lifetime };

    internal static EntryCommandModel AddFactory(ITypeReferenceModel impl, LifetimeKind lifetime, string func) =>
        new() { Kind = EntryKind.AddFactory, AuxType1 = impl, Lifetime = lifetime, FuncExpr = func };

    internal static EntryCommandModel AddOpenGeneric(ServiceModel service, LifetimeKind lifetime) =>
        new() { Kind = EntryKind.AddOpenGeneric, Service = service, Lifetime = lifetime };

    internal static EntryCommandModel AddAsyncFactory(ITypeReferenceModel impl, LifetimeKind lifetime, string func) =>
        new() { Kind = EntryKind.AddAsyncFactory, AuxType1 = impl, Lifetime = lifetime, FuncExpr = func };

    internal static EntryCommandModel AddToCollection(ServiceModel service, LifetimeKind lifetime, int order = 0) =>
        new() { Kind = EntryKind.AddToCollection, Service = service, Lifetime = lifetime, OrderHint = order };

    internal static EntryCommandModel AddPrimaryToCollection(ServiceModel service, LifetimeKind lifetime, int order = 0) =>
        new() { Kind = EntryKind.AddPrimaryToCollection, Service = service, Lifetime = lifetime, OrderHint = order };

    internal static EntryCommandModel AddFromHierarchy(ServiceModel service, LifetimeKind lifetime) =>
        new() { Kind = EntryKind.AddFromHierarchy, Service = service, Lifetime = lifetime };

    internal static EntryCommandModel AddAllFromHierarchy(ServiceModel service, LifetimeKind lifetime) =>
        new() { Kind = EntryKind.AddAllFromHierarchy, Service = service, Lifetime = lifetime };

    internal static EntryCommandModel AddFromSibling(ServiceModel service, LifetimeKind lifetime) =>
        new() { Kind = EntryKind.AddFromSibling, Service = service, Lifetime = lifetime };

    internal static EntryCommandModel AddFromChildren(ServiceModel service, LifetimeKind lifetime) =>
        new() { Kind = EntryKind.AddFromChildren, Service = service, Lifetime = lifetime };

    internal static EntryCommandModel AddAllFromChildren(ServiceModel service, LifetimeKind lifetime) =>
        new() { Kind = EntryKind.AddAllFromChildren, Service = service, Lifetime = lifetime };

    internal static EntryCommandModel AddFromParent(ServiceModel service, LifetimeKind lifetime) =>
        new() { Kind = EntryKind.AddFromParent, Service = service, Lifetime = lifetime };

    internal static EntryCommandModel AddAllFromParent(ServiceModel service, LifetimeKind lifetime) =>
        new() { Kind = EntryKind.AddAllFromParent, Service = service, Lifetime = lifetime };

    internal static EntryCommandModel AddNewComponent(ServiceModel service, LifetimeKind lifetime) =>
        new() { Kind = EntryKind.AddNewComponent, Service = service, Lifetime = lifetime };

    internal static EntryCommandModel AddFromPrefab(ServiceModel service, LifetimeKind lifetime, string prefabArg) =>
        new() { Kind = EntryKind.AddFromPrefab, Service = service, Lifetime = lifetime, PrefabArg = prefabArg };

    internal static EntryCommandModel Decorate(ITypeReferenceModel contract, ITypeReferenceModel decorator) =>
        new() { Kind = EntryKind.Decorate, AuxType1 = decorator, AuxType2 = contract };

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
            EntryKind.Decorate => visitor.VisitDecorate(this),
            _ => ThrowHelper.ThrowUnhandledBranch<TResult>(Kind)
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
    AddFromPrefab,
    
    Decorate,
}

internal enum LifetimeKind : byte {
    Singleton = 0,
    Transient = 1
}