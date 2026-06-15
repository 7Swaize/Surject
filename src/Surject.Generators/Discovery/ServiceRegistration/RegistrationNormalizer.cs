using System.Collections.Immutable;
using Surject.Generators.Models.Concepts;

namespace Surject.Generators.Discovery.ServiceRegistration;

internal readonly struct RegistrationNormalizer : IEntryCommandVisitor<BindingModel> {
    private readonly ImmutableArray<ModifierCommandModel> _parsedModifiers;
    
    internal RegistrationNormalizer(ImmutableArray<ModifierCommandModel> parsedModifiers) {
        _parsedModifiers = parsedModifiers;
    }

    public BindingModel VisitAdd(in EntryCommandModel cmd) => Wrap(cmd);
    public BindingModel VisitAddFactory(in EntryCommandModel cmd) => Wrap(cmd);
    public BindingModel VisitAddOpenGeneric(in EntryCommandModel cmd) => Wrap(cmd);
    public BindingModel VisitAddAsyncFactory(in EntryCommandModel cmd) => Wrap(cmd);
    public BindingModel VisitAddFromHierarchy(in EntryCommandModel cmd) => Wrap(cmd);
    public BindingModel VisitAddAllFromHierarchy(in EntryCommandModel cmd) => Wrap(cmd);
    public BindingModel VisitAddFromSibling(in EntryCommandModel cmd) => Wrap(cmd);
    public BindingModel VisitAddFromChildren(in EntryCommandModel cmd) => Wrap(cmd);
    public BindingModel VisitAddAllFromChildren(in EntryCommandModel cmd) => Wrap(cmd);
    public BindingModel VisitAddFromParent(in EntryCommandModel cmd) => Wrap(cmd);
    public BindingModel VisitAddAllFromParent(in EntryCommandModel cmd) => Wrap(cmd);
    public BindingModel VisitAddNewComponent(in EntryCommandModel cmd) => Wrap(cmd);
    public BindingModel VisitAddFromPrefab(in EntryCommandModel cmd) => Wrap(cmd);
    public BindingModel VisitDecorate(in EntryCommandModel cmd) => Wrap(cmd);

    public BindingModel VisitAddToCollection(in EntryCommandModel cmd) {
        EntryCommandModel normalized = EntryCommandModel.Add(cmd.ImplType, cmd.Lifetime);
        ImmutableArray<ModifierCommandModel> modifiers = _parsedModifiers.Insert(0, ModifierCommandModel.AsCollection(cmd.OrderHint));
        return new BindingModel(normalized, modifiers);
    }

    public BindingModel VisitAddPrimaryToCollection(in EntryCommandModel cmd) {
        EntryCommandModel normalized = EntryCommandModel.Add(cmd.ImplType, cmd.Lifetime);
        ImmutableArray<ModifierCommandModel> modifiers = _parsedModifiers
            .Insert(0, ModifierCommandModel.AsCollection(cmd.OrderHint))
            .Insert(0, ModifierCommandModel.AsPrimary());
        return new BindingModel(normalized, modifiers);
    }

    private BindingModel Wrap(in EntryCommandModel cmd) {
        return new BindingModel(cmd, _parsedModifiers);
    }
}