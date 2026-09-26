using System.CodeDom.Compiler;
using System.Threading.Tasks;
using Surject.Generators.Discovery.ServiceRegistration;
using Surject.Generators.Emitters.Helpers;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;

namespace Surject.Generators.Emitters.Scopes.ContainerInner.NoOpenGeneric;

internal readonly struct SingletonFieldEmitterNoOpenGenericVisitor : IEntryCommandVisitor<VoidVisitor> {
    private readonly IndentedTextWriter _writer;
    private readonly ITypeReferenceModel _entryType;
    private readonly RegistrationModel _registration;

    internal SingletonFieldEmitterNoOpenGenericVisitor(IndentedTextWriter writer, RegistrationModel registration, ITypeReferenceModel entryType) {
        _writer = writer;
        _registration = registration;
        _entryType = entryType;
    }

    public VoidVisitor VisitAdd(in EntryCommandModel cmd) => WriteSingletonFieldDefault();
    public VoidVisitor VisitAddFactory(in EntryCommandModel cmd) => WriteSingletonFieldDefault();
    public VoidVisitor VisitAddToCollection(in EntryCommandModel cmd) => WriteSingletonFieldDefault();
    public VoidVisitor VisitAddPrimaryToCollection(in EntryCommandModel cmd) => WriteSingletonFieldDefault();
    public VoidVisitor VisitAddFromHierarchy(in EntryCommandModel cmd) => WriteSingletonFieldDefault();
    public VoidVisitor VisitAddFromSibling(in EntryCommandModel cmd) => WriteSingletonFieldDefault();
    public VoidVisitor VisitAddFromChildren(in EntryCommandModel cmd) => WriteSingletonFieldDefault();
    public VoidVisitor VisitAddFromParent(in EntryCommandModel cmd) => WriteSingletonFieldDefault();
    public VoidVisitor VisitAddNewComponent(in EntryCommandModel cmd) => WriteSingletonFieldDefault();
    public VoidVisitor VisitAddFromPrefab(in EntryCommandModel cmd) => WriteSingletonFieldDefault();
    public VoidVisitor VisitAddAmbient(in EntryCommandModel cmd) => WriteSingletonFieldDefault();
    
    public VoidVisitor VisitAddAllFromHierarchy(in EntryCommandModel cmd) => VoidVisitor.Default;
    public VoidVisitor VisitAddAllFromChildren(in EntryCommandModel cmd) => VoidVisitor.Default;
    public VoidVisitor VisitAddAllFromParent(in EntryCommandModel cmd) => VoidVisitor.Default;
    
    public VoidVisitor VisitAddOpenGeneric(in EntryCommandModel cmd) => VoidVisitor.Default;

    public VoidVisitor VisitAddAsyncFactory(in EntryCommandModel cmd) {
        string? key = ParseHelpers.GetKeyExprOrNull(_registration);

        string fieldName = key is null
            ? BuildHelpers.BuildSingletonFieldNameNotKeyed(_entryType)
            : BuildHelpers.BuildSingletonFieldNameKeyed(_entryType, key);

        string taskFieldName = key is null
            ? BuildHelpers.BuildTaskFieldNameNotKeyed(_entryType)
            : BuildHelpers.BuildTaskFieldNameKeyed(_entryType, key);

        _writer.WriteLine($"internal {_entryType.FQNConstructedArgBased}? {fieldName};");
        _writer.WriteLine($"internal global::{typeof(Task).FullName}<{_entryType.FQNConstructedArgBased}>? {taskFieldName};");

        return VoidVisitor.Default;
    }
    
    private VoidVisitor WriteSingletonFieldDefault() {
        string? key = ParseHelpers.GetKeyExprOrNull(_registration);

        string fieldName = key is null
            ? BuildHelpers.BuildSingletonFieldNameNotKeyed(_entryType)
            : BuildHelpers.BuildSingletonFieldNameKeyed(_entryType, key);

        _writer.WriteLine($"internal {_entryType.FQNConstructedArgBased}? {fieldName};");
        
        return VoidVisitor.Default;
    }
}