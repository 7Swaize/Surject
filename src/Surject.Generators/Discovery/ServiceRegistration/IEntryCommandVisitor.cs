using Surject.Generators.Models.Concepts;

namespace Surject.Generators.Discovery.ServiceRegistration;

internal interface IEntryCommandVisitor<out TResult> {
    public TResult VisitAdd(in EntryCommandModel cmd);
    public TResult VisitAddFactory(in EntryCommandModel cmd);
    public TResult VisitAddOpenGeneric(in EntryCommandModel cmd);
    public TResult VisitAddAsyncFactory(in EntryCommandModel cmd);
    public TResult VisitAddToCollection(in EntryCommandModel cmd);
    public TResult VisitAddPrimaryToCollection(in EntryCommandModel cmd);
    public TResult VisitAddFromHierarchy(in EntryCommandModel cmd);
    public TResult VisitAddAllFromHierarchy(in EntryCommandModel cmd);
    public TResult VisitAddFromSibling(in EntryCommandModel cmd);
    public TResult VisitAddFromChildren(in EntryCommandModel cmd);
    public TResult VisitAddAllFromChildren(in EntryCommandModel cmd);
    public TResult VisitAddFromParent(in EntryCommandModel cmd);
    public TResult VisitAddAllFromParent(in EntryCommandModel cmd);
    public TResult VisitAddNewComponent(in EntryCommandModel cmd);
    public TResult VisitAddFromPrefab(in EntryCommandModel cmd);
    public TResult VisitDecorate(in EntryCommandModel cmd);
}