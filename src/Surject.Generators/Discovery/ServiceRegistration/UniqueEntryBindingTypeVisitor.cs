using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;

namespace Surject.Generators.Discovery.ServiceRegistration;

internal readonly struct UniqueEntryBindingTypeVisitor : IEntryCommandVisitor<ITypeReferenceModel?> {
    public ITypeReferenceModel? VisitAdd(in EntryCommandModel cmd) => cmd.Service.TypeRef;
    public ITypeReferenceModel? VisitAddFactory(in EntryCommandModel cmd) => cmd.AuxType1;
    public ITypeReferenceModel? VisitAddOpenGeneric(in EntryCommandModel cmd) => cmd.Service.TypeRef;
    public ITypeReferenceModel? VisitAddAsyncFactory(in EntryCommandModel cmd) => cmd.AuxType1;
    public ITypeReferenceModel? VisitAddToCollection(in EntryCommandModel cmd) => cmd.Service.TypeRef;
    public ITypeReferenceModel? VisitAddPrimaryToCollection(in EntryCommandModel cmd) => cmd.Service.TypeRef;
    public ITypeReferenceModel? VisitAddFromHierarchy(in EntryCommandModel cmd) => cmd.Service.TypeRef;
    public ITypeReferenceModel? VisitAddAllFromHierarchy(in EntryCommandModel cmd) => cmd.Service.TypeRef;
    public ITypeReferenceModel? VisitAddFromSibling(in EntryCommandModel cmd) => cmd.Service.TypeRef;
    public ITypeReferenceModel? VisitAddFromChildren(in EntryCommandModel cmd) => cmd.Service.TypeRef;
    public ITypeReferenceModel? VisitAddAllFromChildren(in EntryCommandModel cmd) => cmd.Service.TypeRef;
    public ITypeReferenceModel? VisitAddFromParent(in EntryCommandModel cmd) => cmd.Service.TypeRef;
    public ITypeReferenceModel? VisitAddAllFromParent(in EntryCommandModel cmd) => cmd.Service.TypeRef;
    public ITypeReferenceModel? VisitAddNewComponent(in EntryCommandModel cmd) => cmd.Service.TypeRef;
    public ITypeReferenceModel? VisitAddFromPrefab(in EntryCommandModel cmd) => cmd.Service.TypeRef;
    public ITypeReferenceModel? AddAmbient(in EntryCommandModel cmd) => null;
}