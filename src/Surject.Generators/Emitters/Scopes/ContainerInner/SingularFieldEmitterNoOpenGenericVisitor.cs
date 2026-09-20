using System.CodeDom.Compiler;
using System.Threading.Tasks;
using Surject.Generators.Discovery.ServiceRegistration;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;
using Surject.Shared.Helpers;

namespace Surject.Generators.Emitters.Scopes.ContainerInner;

internal readonly struct SingularFieldEmitterNoOpenGenericVisitor : IEntryCommandVisitor<VoidVisitor> {
    private readonly IndentedTextWriter _writer;
    private readonly ITypeReferenceModel _entryType;
    private readonly RegistrationModel _registration;

    internal SingularFieldEmitterNoOpenGenericVisitor(IndentedTextWriter writer, RegistrationModel registration, ITypeReferenceModel entryType) {
        _writer = writer;
        _registration = registration;
        _entryType = entryType;
    }

    public VoidVisitor VisitAdd(in EntryCommandModel cmd) => WriteSingletonFieldDefault();
    public VoidVisitor VisitAddFactory(in EntryCommandModel cmd) => WriteSingletonFieldDefault();
    public VoidVisitor VisitAddFromHierarchy(in EntryCommandModel cmd) => WriteSingletonFieldDefault();
    public VoidVisitor VisitAddFromSibling(in EntryCommandModel cmd) => WriteSingletonFieldDefault();
    public VoidVisitor VisitAddFromChildren(in EntryCommandModel cmd) => WriteSingletonFieldDefault();
    public VoidVisitor VisitAddFromParent(in EntryCommandModel cmd) => WriteSingletonFieldDefault();
    public VoidVisitor VisitAddNewComponent(in EntryCommandModel cmd) => WriteSingletonFieldDefault();
    public VoidVisitor VisitAddFromPrefab(in EntryCommandModel cmd) => WriteSingletonFieldDefault();
    public VoidVisitor VisitAddOpenGeneric(in EntryCommandModel cmd) => VoidVisitor.Default;
    public VoidVisitor AddAmbient(in EntryCommandModel cmd) => VoidVisitor.Default;

    public VoidVisitor VisitAddAsyncFactory(in EntryCommandModel cmd) {
        if ((_registration.ModifiersDescriptor & ModifierKind.WithId) != ModifierKind.WithId) {
            _writer.WriteLine($"internal {_entryType.FQNConstructedArgBased}? {BuildHelpers.BuildSingletonFieldNameNotKeyed(_entryType)}");
            _writer.WriteLine(
                $"internal global::{typeof(Task)}<{_entryType.FQNConstructedArgBased}>? {BuildHelpers.BuildTaskFieldNameNotKeyed(_entryType)}"
            );
            
            return VoidVisitor.Default;
        }
        
        foreach (ref readonly ModifierCommandModel modifier in _registration.Modifiers) {
            if (modifier.Kind != ModifierKind.WithId) {
                continue;
            }
            
            _writer.WriteLine(
                $"internal {_entryType.FQNConstructedArgBased}? {BuildHelpers.BuildSingletonFieldNameKeyed(_entryType, modifier.StringArg1)}"
            );
            _writer.WriteLine(
                $"internal global::{typeof(Task)}<{_entryType.FQNConstructedArgBased}>? " +
                $"{BuildHelpers.BuildSingletonFieldNameKeyed(_entryType, modifier.StringArg1)}"
            );
            
            return VoidVisitor.Default;
        }
        
        return ThrowHelpers.ThrowUnhandledBranch<VoidVisitor>(VoidVisitor.Default);
    }
    
    public VoidVisitor VisitAddAllFromHierarchy(in EntryCommandModel cmd) => WriteUnityRuntimeBasedDiscoveryAllDefault();
    public VoidVisitor VisitAddAllFromChildren(in EntryCommandModel cmd) => WriteUnityRuntimeBasedDiscoveryAllDefault();
    public VoidVisitor VisitAddAllFromParent(in EntryCommandModel cmd) => WriteUnityRuntimeBasedDiscoveryAllDefault();
    
    public VoidVisitor VisitAddToCollection(in EntryCommandModel cmd) => WriteSingletonFieldDefault();
    public VoidVisitor VisitAddPrimaryToCollection(in EntryCommandModel cmd) => WriteSingletonFieldDefault();

    private VoidVisitor WriteSingletonFieldDefault() {
        if ((_registration.ModifiersDescriptor & ModifierKind.WithId) != ModifierKind.WithId) {
            _writer.WriteLine($"internal {_entryType.FQNConstructedArgBased} {BuildHelpers.BuildSingletonFieldNameNotKeyed(_entryType)}");
            return VoidVisitor.Default;
        }
        
        foreach (ref readonly ModifierCommandModel modifier in _registration.Modifiers) {
            if (modifier.Kind != ModifierKind.WithId) {
                continue;
            }
            
            _writer.WriteLine(
                $"internal {_entryType.FQNConstructedArgBased} {BuildHelpers.BuildSingletonFieldNameKeyed(_entryType, modifier.StringArg1)}"
            );
            
            return VoidVisitor.Default;
        }
        
        return ThrowHelpers.ThrowUnhandledBranch<VoidVisitor>(VoidVisitor.Default);
    }

    private VoidVisitor WriteUnityRuntimeBasedDiscoveryAllDefault() {
        if ((_registration.ModifiersDescriptor & ModifierKind.WithId) != ModifierKind.WithId) {
            _writer.WriteLine($"internal {_entryType.FQNConstructedArgBased}[] {BuildHelpers.BuildMultiBindArrayNotKeyed(_entryType)}");
            return VoidVisitor.Default;
        }
        
        foreach (ref readonly ModifierCommandModel modifier in _registration.Modifiers) {
            if (modifier.Kind != ModifierKind.WithId) {
                continue;
            }
            
            _writer.WriteLine(
                $"internal {_entryType.FQNConstructedArgBased}[] {BuildHelpers.BuildSingletonFieldNameKeyed(_entryType, modifier.StringArg1)}"
            );
            
            return VoidVisitor.Default;
        }
        
        return ThrowHelpers.ThrowUnhandledBranch<VoidVisitor>(VoidVisitor.Default);
    }
}