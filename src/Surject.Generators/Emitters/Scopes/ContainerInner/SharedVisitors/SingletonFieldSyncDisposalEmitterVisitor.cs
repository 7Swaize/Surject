using System;
using System.CodeDom.Compiler;
using Surject.Generators.Discovery.ServiceRegistration;
using Surject.Generators.Emitters.Helpers;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;
using Surject.Shared.Helpers;

namespace Surject.Generators.Emitters.Scopes.ContainerInner.SharedVisitors;

internal readonly struct SingletonFieldSyncDisposalEmitterVisitor : IEntryCommandVisitor<VoidVisitor> {
    private readonly IndentedTextWriter _writer;
    private readonly ITypeReferenceModel _entryType;
    private readonly RegistrationModel _registration;

    internal SingletonFieldSyncDisposalEmitterVisitor(IndentedTextWriter writer, ITypeReferenceModel entryType, RegistrationModel registration) {
        _writer = writer;
        _entryType = entryType;
        _registration = registration;
    }
    
    public VoidVisitor VisitAdd(in EntryCommandModel cmd) => WriteSingletonFieldDisposalDefault();
    public VoidVisitor VisitAddFactory(in EntryCommandModel cmd) => WriteSingletonFieldDisposalDefault();
    public VoidVisitor VisitAddAsyncFactory(in EntryCommandModel cmd) => WriteSingletonFieldDisposalDefault();
    public VoidVisitor VisitAddToCollection(in EntryCommandModel cmd) => WriteSingletonFieldDisposalDefault();
    public VoidVisitor VisitAddPrimaryToCollection(in EntryCommandModel cmd) => WriteSingletonFieldDisposalDefault();
    public VoidVisitor VisitAddFromHierarchy(in EntryCommandModel cmd) => WriteSingletonFieldDisposalDefault();
    public VoidVisitor VisitAddFromSibling(in EntryCommandModel cmd) => WriteSingletonFieldDisposalDefault();
    public VoidVisitor VisitAddFromChildren(in EntryCommandModel cmd) => WriteSingletonFieldDisposalDefault();
    public VoidVisitor VisitAddFromParent(in EntryCommandModel cmd) => WriteSingletonFieldDisposalDefault();
    public VoidVisitor VisitAddAmbient(in EntryCommandModel cmd) => WriteSingletonFieldDisposalDefault();
    
    public VoidVisitor VisitAddNewComponent(in EntryCommandModel cmd) => WriteSingletonFieldDestroyFromRuntimeDefault();
    public VoidVisitor VisitAddFromPrefab(in EntryCommandModel cmd) => WriteSingletonFieldDestroyFromRuntimeDefault();
    
    public VoidVisitor VisitAddAllFromHierarchy(in EntryCommandModel cmd) => VoidVisitor.Default;
    public VoidVisitor VisitAddAllFromChildren(in EntryCommandModel cmd) => VoidVisitor.Default;
    public VoidVisitor VisitAddAllFromParent(in EntryCommandModel cmd) => VoidVisitor.Default;
    
    public VoidVisitor VisitAddOpenGeneric(in EntryCommandModel cmd) => WriteSingletonFieldDisposalDefault();

    private VoidVisitor WriteSingletonFieldDisposalDefault() {
        if (!ParseHelpers.InheritsFromIDisposable(_entryType)) {
            return VoidVisitor.Default;
        }

        string fieldName = ResolveFieldName();
        _writer.WriteLine($"{fieldName}?.{nameof(IDisposable.Dispose)}();");
        
        return VoidVisitor.Default;
    }

    private VoidVisitor WriteSingletonFieldDestroyFromRuntimeDefault() {
        if ((_registration.ModifiersDescriptor & ModifierKind.DoNotDestroy) == ModifierKind.DoNotDestroy) {
            return VoidVisitor.Default;
        }

        string fieldName = ResolveFieldName();

        _writer.WriteLine($"if ({fieldName} != null)");
        _writer.Indent++;
        _writer.WriteLine($"global::UnityEngine.Object.Destroy({fieldName}.gameObject);");
        _writer.Indent--;
        _writer.WriteLine();

        return VoidVisitor.Default;
    }

    private string ResolveFieldName() {
        string? key = ParseHelpers.GetKeyExprOrNull(_registration);
        
        return key is null
            ? BuildHelpers.BuildSingletonFieldNameNotKeyed(_entryType)
            : BuildHelpers.BuildSingletonFieldNameKeyed(_entryType, key);
    }
}