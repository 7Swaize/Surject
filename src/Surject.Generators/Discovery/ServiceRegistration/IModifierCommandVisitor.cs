using Surject.Generators.Models.Concepts;

namespace Surject.Generators.Discovery.ServiceRegistration;

internal interface IModifierCommandVisitor<out TResult> {
    public TResult VisitTo(in ModifierCommandModel cmd);
    public TResult ToImmediateImplementedInterfaces(in ModifierCommandModel cmd);
    public TResult VisitToAllImplementedInterfaces(in ModifierCommandModel cmd);
    public TResult VisitWithId(in ModifierCommandModel cmd);
    public TResult VisitEager(in ModifierCommandModel cmd);
    public TResult VisitLazy(in ModifierCommandModel cmd);
    public TResult VisitWithArgument(in ModifierCommandModel cmd);
    public TResult VisitOverrideExisting(in ModifierCommandModel cmd);
    public TResult VisitAsCollection(in ModifierCommandModel cmd);
    public TResult VisitAsPrimary(in ModifierCommandModel cmd);
    public TResult VisitDoNotDispose(in ModifierCommandModel cmd);
    public TResult VisitTrackDisposable(in ModifierCommandModel cmd);
    public TResult VisitUnderTransform(in ModifierCommandModel cmd);
    public TResult VisitUnderObjectOfType(in ModifierCommandModel cmd);
    public TResult VisitWithGameObjectName(in ModifierCommandModel cmd);
    public TResult VisitDoNotDestroy(in ModifierCommandModel cmd);
}