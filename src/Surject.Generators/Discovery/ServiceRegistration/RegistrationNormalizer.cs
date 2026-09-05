using System.Collections.Immutable;
using Surject.Generators.Models.Concepts;

namespace Surject.Generators.Discovery.ServiceRegistration;

internal readonly struct RegistrationNormalizer : IEntryCommandVisitor<RegistrationModel> {
    private readonly ImmutableArray<ModifierCommandModel> _parsedModifiers;

    internal RegistrationNormalizer(ImmutableArray<ModifierCommandModel> parsedModifiers) {
        _parsedModifiers = parsedModifiers;
    }
    
    public RegistrationModel VisitAdd(in EntryCommandModel cmd) => Wrap(in cmd);
    public RegistrationModel VisitAddFactory(in EntryCommandModel cmd) => Wrap(in cmd);
    public RegistrationModel VisitAddOpenGeneric(in EntryCommandModel cmd) => Wrap(in cmd);
    public RegistrationModel VisitAddAsyncFactory(in EntryCommandModel cmd) => Wrap(in cmd);
    public RegistrationModel VisitAddFromHierarchy(in EntryCommandModel cmd) => Wrap(in cmd);
    public RegistrationModel VisitAddAllFromHierarchy(in EntryCommandModel cmd) => Wrap(in cmd);
    public RegistrationModel VisitAddFromSibling(in EntryCommandModel cmd) => Wrap(in cmd);
    public RegistrationModel VisitAddFromChildren(in EntryCommandModel cmd) => Wrap(in cmd);
    public RegistrationModel VisitAddAllFromChildren(in EntryCommandModel cmd) => Wrap(in cmd);
    public RegistrationModel VisitAddFromParent(in EntryCommandModel cmd) => Wrap(in cmd);
    public RegistrationModel VisitAddAllFromParent(in EntryCommandModel cmd) => Wrap(in cmd);
    public RegistrationModel VisitAddNewComponent(in EntryCommandModel cmd) => Wrap(in cmd);
    public RegistrationModel VisitAddFromPrefab(in EntryCommandModel cmd) => Wrap(in cmd);

    public RegistrationModel VisitAddToCollection(in EntryCommandModel cmd) {
        EntryCommandModel normalized = EntryCommandModel.Add(cmd.Service, cmd.Lifetime);
        ImmutableArray<ModifierCommandModel> modifiers = _parsedModifiers.Insert(0, ModifierCommandModel.AsCollection(cmd.OrderHint));
        return new RegistrationModel(in normalized, modifiers);
    }

    public RegistrationModel VisitAddPrimaryToCollection(in EntryCommandModel cmd) {
        EntryCommandModel normalized = EntryCommandModel.Add(cmd.Service, cmd.Lifetime);
        ImmutableArray<ModifierCommandModel> modifiers = _parsedModifiers
            .Insert(0, ModifierCommandModel.AsCollection(cmd.OrderHint))
            .Insert(0, ModifierCommandModel.AsPrimary());
        return new RegistrationModel(in normalized, modifiers);
    }

    private RegistrationModel Wrap(in EntryCommandModel cmd) {
        return new RegistrationModel(in cmd, _parsedModifiers);
    }
}