using System;
using System.Diagnostics.CodeAnalysis;
using Surject.Generators.Discovery.ServiceRegistration;
using Surject.Generators.Models.Primitives;

namespace Surject.Generators.Models.Concepts;

internal readonly record struct EntryCommandModel {
    internal EntryKind Kind { get; init; }
    internal ITypeReferenceModel ImplType { get; init; }

    internal ITypeReferenceModel AuxType { get; init; }
    internal LifetimeKind Lifetime { get; init; }

    internal string FuncExpr { get; init; }
    internal string PrefabArg { get; init; }

    internal int OrderHint { get; init; }

    internal static EntryCommandModel Add(ITypeReferenceModel impl, LifetimeKind lifetime) =>
        new() { Kind = EntryKind.Add, ImplType = impl, Lifetime = lifetime };

    internal static EntryCommandModel AddFactory(ITypeReferenceModel impl, LifetimeKind lifetime, string func) =>
        new() { Kind = EntryKind.AddFactory, ImplType = impl, Lifetime = lifetime, FuncExpr = func };

    internal static EntryCommandModel AddOpenGeneric(ITypeReferenceModel openImpl, LifetimeKind lifetime) =>
        new() { Kind = EntryKind.AddOpenGeneric, ImplType = openImpl, Lifetime = lifetime };

    internal static EntryCommandModel AddAsyncFactory(ITypeReferenceModel impl, LifetimeKind lifetime, string func) =>
        new() { Kind = EntryKind.AddAsyncFactory, ImplType = impl, Lifetime = lifetime, FuncExpr = func };

    internal static EntryCommandModel AddToCollection(ITypeReferenceModel impl, LifetimeKind lifetime, int order = 0) =>
        new() { Kind = EntryKind.AddToCollection, ImplType = impl, Lifetime = lifetime, OrderHint = order };

    internal static EntryCommandModel AddPrimaryToCollection(ITypeReferenceModel impl, LifetimeKind lifetime, int order = 0) =>
        new() { Kind = EntryKind.AddPrimaryToCollection, ImplType = impl, Lifetime = lifetime, OrderHint = order };

    internal static EntryCommandModel AddFromHierarchy(ITypeReferenceModel impl, LifetimeKind lifetime) =>
        new() { Kind = EntryKind.AddFromHierarchy, ImplType = impl, Lifetime = lifetime };

    internal static EntryCommandModel AddAllFromHierarchy(ITypeReferenceModel impl, LifetimeKind lifetime) =>
        new() { Kind = EntryKind.AddAllFromHierarchy, ImplType = impl, Lifetime = lifetime };

    internal static EntryCommandModel AddFromSibling(ITypeReferenceModel impl, LifetimeKind lifetime) =>
        new() { Kind = EntryKind.AddFromSibling, ImplType = impl, Lifetime = lifetime };

    internal static EntryCommandModel AddFromChildren(ITypeReferenceModel impl, LifetimeKind lifetime) =>
        new() { Kind = EntryKind.AddFromChildren, ImplType = impl, Lifetime = lifetime };

    internal static EntryCommandModel AddAllFromChildren(ITypeReferenceModel impl, LifetimeKind lifetime) =>
        new() { Kind = EntryKind.AddAllFromChildren, ImplType = impl, Lifetime = lifetime };

    internal static EntryCommandModel AddFromParent(ITypeReferenceModel impl, LifetimeKind lifetime) =>
        new() { Kind = EntryKind.AddFromParent, ImplType = impl, Lifetime = lifetime };

    internal static EntryCommandModel AddAllFromParent(ITypeReferenceModel impl, LifetimeKind lifetime) =>
        new() { Kind = EntryKind.AddAllFromParent, ImplType = impl, Lifetime = lifetime };

    internal static EntryCommandModel AddNewComponent(ITypeReferenceModel impl, LifetimeKind lifetime) =>
        new() { Kind = EntryKind.AddNewComponent, ImplType = impl, Lifetime = lifetime };

    internal static EntryCommandModel AddFromPrefab(ITypeReferenceModel impl, LifetimeKind lifetime, string prefabArg) =>
        new() { Kind = EntryKind.AddFromPrefab, ImplType = impl, Lifetime = lifetime, PrefabArg = prefabArg };

    internal static EntryCommandModel Decorate(ITypeReferenceModel contract, ITypeReferenceModel decorator) =>
        new() { Kind = EntryKind.Decorate, ImplType = decorator, AuxType = contract };

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
            _ => ThrowUnknownKind<TResult>(Kind),
        };
    }
    
    [DoesNotReturn]
    private static TResult ThrowUnknownKind<TResult>(EntryKind kind) =>
        throw new InvalidOperationException($"Unhandled EntryKind '{kind}' in visitor dispatch.");
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