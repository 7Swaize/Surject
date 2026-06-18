using Surject.Generators.Discovery.ServiceRegistration;
using Surject.Generators.Models.Primitives;
using Surject.Shared.Helpers;

namespace Surject.Generators.Models.Concepts;

internal readonly record struct ModifierCommandModel {
    internal ModifierKind Kind { get; init; }
    
    internal ITypeReferenceModel TypeArg { get; init; }
    internal string ExprArg { get; init; }
    internal string StringArg { get; init; }
    internal int IntArg { get; init; }
    
    internal static ModifierCommandModel To(ITypeReferenceModel contract) =>
        new() { Kind = ModifierKind.To, TypeArg = contract };
    
    internal static ModifierCommandModel ToImplementedInterfaces() =>
        new() { Kind = ModifierKind.ToImplementedInterfaces };
    
    internal static ModifierCommandModel WithId(string id) =>
        new() { Kind = ModifierKind.WithId, StringArg = id };

    internal static ModifierCommandModel Pooled(int initialSize = 1) =>
        new() { Kind = ModifierKind.Pooled, IntArg = initialSize };
    
    internal static ModifierCommandModel Eager() =>
        new() { Kind = ModifierKind.Eager };
    
    internal static ModifierCommandModel Lazy() =>
        new() { Kind = ModifierKind.Lazy };
    
    internal static ModifierCommandModel FromFactory(string lambdaExprText) =>
        new() { Kind = ModifierKind.FromFactory, ExprArg = lambdaExprText };
    
    internal static ModifierCommandModel FromInjectFactory() =>
        new() { Kind = ModifierKind.FromInjectFactory };
    
    internal static ModifierCommandModel WithArgument(string paramName, ITypeReferenceModel argType, string valueExprText) =>
        new() { Kind = ModifierKind.WithArgument, StringArg = paramName, TypeArg = argType, ExprArg = valueExprText };
    
    internal static ModifierCommandModel WhenInjectedInto(ITypeReferenceModel consumer) =>
        new() { Kind = ModifierKind.WhenInjectedInto, TypeArg = consumer };
    
    internal static ModifierCommandModel When(string conditionExprText) =>
        new() { Kind = ModifierKind.When, ExprArg = conditionExprText };
    
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
        new() { Kind = ModifierKind.UnderTransform, ExprArg = transformExprText };

    internal static ModifierCommandModel UnderObjectOfType(ITypeReferenceModel componentType) =>
        new() { Kind = ModifierKind.UnderObjectOfType, TypeArg = componentType };

    internal static ModifierCommandModel WithGameObjectName(string name) =>
        new() { Kind = ModifierKind.WithGameObjectName, StringArg = name };

    internal static ModifierCommandModel DoNotDestroy() =>
        new() { Kind = ModifierKind.DoNotDestroy };
    
    internal TResult Accept<TVisitor, TResult>(ref TVisitor visitor) where TVisitor : struct, IModifierCommandVisitor<TResult> {
        return Kind switch {
            ModifierKind.To => visitor.VisitTo(this),
            ModifierKind.ToImplementedInterfaces => visitor.VisitToImplementedInterfaces(this),
            ModifierKind.WithId => visitor.VisitWithId(this),
            ModifierKind.Pooled => visitor.VisitPooled(this),
            ModifierKind.Eager => visitor.VisitEager(this),
            ModifierKind.Lazy => visitor.VisitLazy(this),
            ModifierKind.FromFactory => visitor.VisitFromFactory(this),
            ModifierKind.FromInjectFactory => visitor.VisitFromInjectFactory(this),
            ModifierKind.WithArgument => visitor.VisitWithArgument(this),
            ModifierKind.WhenInjectedInto => visitor.VisitWhenInjectedInto(this),
            ModifierKind.When => visitor.VisitWhen(this),
            ModifierKind.OverrideExisting => visitor.VisitOverrideExisting(this),
            ModifierKind.AsCollection => visitor.VisitAsCollection(this),
            ModifierKind.AsPrimary => visitor.VisitAsPrimary(this),
            ModifierKind.DoNotDispose => visitor.VisitDoNotDispose(this),
            ModifierKind.TrackDisposable => visitor.VisitTrackDisposable(this),
            ModifierKind.UnderTransform => visitor.VisitUnderTransform(this),
            ModifierKind.UnderObjectOfType => visitor.VisitUnderObjectOfType(this),
            ModifierKind.WithGameObjectName => visitor.VisitWithGameObjectName(this),
            ModifierKind.DoNotDestroy => visitor.VisitDoNotDestroy(this),
            _ => ThrowHelper.ThrowUnhandledBranch<TResult>(Kind),
        };
    }
}

internal enum ModifierKind : byte {
    To,
    ToImplementedInterfaces,
    WithId,
    
    Pooled,
    Eager,
    Lazy,
    
    FromFactory,
    FromInjectFactory,
    WithArgument,
    
    WhenInjectedInto,
    When,
    
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