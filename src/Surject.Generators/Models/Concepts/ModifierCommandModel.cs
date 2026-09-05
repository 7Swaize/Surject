using Surject.Generators.Discovery.ServiceRegistration;
using Surject.Generators.Models.Primitives;
using Surject.Shared.Helpers;

namespace Surject.Generators.Models.Concepts;

internal readonly record struct ModifierCommandModel {
    internal ModifierKind Kind { get; init; }
    
    internal ITypeReferenceModel TypeArg { get; init; }
    internal string StringArg1 { get; init; }
    internal string StringArg2 { get; init; }
    internal int IntArg { get; init; }
    
    internal static ModifierCommandModel To(ITypeReferenceModel contract) =>
        new() { Kind = ModifierKind.To, TypeArg = contract };
    
    internal static ModifierCommandModel ToImmediateImplementedInterfaces() =>
        new() { Kind = ModifierKind.ToImmediateImplementedInterfaces };
    
    internal static ModifierCommandModel ToAllImplementedInterfaces() =>
        new() { Kind = ModifierKind.ToAllImplementedInterfaces };
    
    internal static ModifierCommandModel WithId(string id) =>
        new() { Kind = ModifierKind.WithId, StringArg1 = id };
    
    internal static ModifierCommandModel Eager() =>
        new() { Kind = ModifierKind.Eager };
    
    internal static ModifierCommandModel Lazy() =>
        new() { Kind = ModifierKind.Lazy };
    
    internal static ModifierCommandModel WithArgument(string paramName, ITypeReferenceModel argType, string valueExprText) =>
        new() { Kind = ModifierKind.WithArgument, StringArg1 = paramName, TypeArg = argType, StringArg2 = valueExprText };
        
    internal static ModifierCommandModel OverrideExisting() =>
        new() { Kind = ModifierKind.OverrideExisting };

    internal static ModifierCommandModel AsCollection(int order = 0) =>
        new() { Kind = ModifierKind.AsCollection, IntArg = order };
    
    internal static ModifierCommandModel AsPrimary() =>
        new() { Kind = ModifierKind.AsPrimary };

    internal static ModifierCommandModel DoNotDispose() =>
        new() { Kind = ModifierKind.DoNotDispose };

    internal static ModifierCommandModel TrackDisposable() =>
        new() { Kind = ModifierKind.TrackDisposable };

    internal static ModifierCommandModel UnderTransform(string transformExprText) =>
        new() { Kind = ModifierKind.UnderTransform, StringArg1 = transformExprText };

    internal static ModifierCommandModel UnderObjectOfType(ITypeReferenceModel componentType) =>
        new() { Kind = ModifierKind.UnderObjectOfType, TypeArg = componentType };

    internal static ModifierCommandModel WithGameObjectName(string name) =>
        new() { Kind = ModifierKind.WithGameObjectName, StringArg1 = name };

    internal static ModifierCommandModel DoNotDestroy() =>
        new() { Kind = ModifierKind.DoNotDestroy };

    internal TResult Accept<TVisitor, TResult>(ref TVisitor visitor) where TVisitor : struct, IModifierCommandVisitor<TResult> {
        return Kind switch {
            ModifierKind.To => visitor.VisitTo(this),
            ModifierKind.ToImmediateImplementedInterfaces => visitor.ToImmediateImplementedInterfaces(this),
            ModifierKind.ToAllImplementedInterfaces => visitor.VisitToAllImplementedInterfaces(this),
            ModifierKind.WithId => visitor.VisitWithId(this),
            ModifierKind.Eager => visitor.VisitEager(this),
            ModifierKind.Lazy => visitor.VisitLazy(this),
            ModifierKind.WithArgument => visitor.VisitWithArgument(this),
            ModifierKind.OverrideExisting => visitor.VisitOverrideExisting(this),
            ModifierKind.AsCollection => visitor.VisitAsCollection(this),
            ModifierKind.AsPrimary => visitor.VisitAsPrimary(this),
            ModifierKind.DoNotDispose => visitor.VisitDoNotDispose(this),
            ModifierKind.TrackDisposable => visitor.VisitTrackDisposable(this),
            ModifierKind.UnderTransform => visitor.VisitUnderTransform(this),
            ModifierKind.UnderObjectOfType => visitor.VisitUnderObjectOfType(this),
            ModifierKind.WithGameObjectName => visitor.VisitWithGameObjectName(this),
            ModifierKind.DoNotDestroy => visitor.VisitDoNotDestroy(this),
            _ => ThrowHelpers.ThrowUnhandledBranch<TResult>(Kind)
        };
    }
}

internal enum ModifierKind : byte {
    To,
    ToImmediateImplementedInterfaces,
    ToAllImplementedInterfaces,
    WithId,
    
    Eager,
    Lazy,
    
    WithArgument,
    
    OverrideExisting,
    AsCollection,
    AsPrimary,
    
    DoNotDispose,
    TrackDisposable,
    
    UnderTransform,
    UnderObjectOfType,
    WithGameObjectName,
    DoNotDestroy,
}