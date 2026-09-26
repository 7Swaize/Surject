using System;
using System.CodeDom.Compiler;
using Surject.Generators.Discovery.ServiceRegistration;
using Surject.Generators.Emitters.Helpers;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;
using Surject.Shared.Helpers;

namespace Surject.Generators.Emitters.Scopes.ContainerInner.SharedVisitors;

internal readonly struct SingletonFieldAsyncDisposalEmitterVisitor : IEntryCommandVisitor<VoidVisitor> {
    private readonly IndentedTextWriter _writer;
    private readonly ITypeReferenceModel _entryType;
    private readonly RegistrationModel _registration;
    private readonly SingletonFieldSyncDisposalEmitterVisitor _syncDisposalEmitter;

    internal SingletonFieldAsyncDisposalEmitterVisitor(IndentedTextWriter writer, ITypeReferenceModel entryType, RegistrationModel registration) {
        _writer = writer;
        _entryType = entryType;
        _registration = registration;
        _syncDisposalEmitter = new SingletonFieldSyncDisposalEmitterVisitor(writer, entryType, registration);
    }
    
    public VoidVisitor VisitAdd(in EntryCommandModel cmd) => WriteSingletonFieldDisposalDefault(in cmd, _syncDisposalEmitter.VisitAdd);
    public VoidVisitor VisitAddFactory(in EntryCommandModel cmd) => WriteSingletonFieldDisposalDefault(in cmd, _syncDisposalEmitter.VisitAddFactory);
    public VoidVisitor VisitAddAsyncFactory(in EntryCommandModel cmd) => WriteSingletonFieldDisposalDefault(in cmd, _syncDisposalEmitter.VisitAddAsyncFactory);
    public VoidVisitor VisitAddToCollection(in EntryCommandModel cmd) => WriteSingletonFieldDisposalDefault(in cmd, _syncDisposalEmitter.VisitAddToCollection);
    public VoidVisitor VisitAddPrimaryToCollection(in EntryCommandModel cmd) => WriteSingletonFieldDisposalDefault(in cmd, _syncDisposalEmitter.VisitAddPrimaryToCollection);
    public VoidVisitor VisitAddFromHierarchy(in EntryCommandModel cmd) => WriteSingletonFieldDisposalDefault(in cmd, _syncDisposalEmitter.VisitAddFromHierarchy);
    public VoidVisitor VisitAddFromSibling(in EntryCommandModel cmd) => WriteSingletonFieldDisposalDefault(in cmd, _syncDisposalEmitter.VisitAddFromSibling);
    public VoidVisitor VisitAddFromChildren(in EntryCommandModel cmd) => WriteSingletonFieldDisposalDefault(in cmd, _syncDisposalEmitter.VisitAddFromChildren);
    public VoidVisitor VisitAddFromParent(in EntryCommandModel cmd) => WriteSingletonFieldDisposalDefault(in cmd, _syncDisposalEmitter.VisitAddFromParent);
    public VoidVisitor VisitAddAmbient(in EntryCommandModel cmd) => WriteSingletonFieldDisposalDefault(in cmd, _syncDisposalEmitter.VisitAddAmbient);
    
    public VoidVisitor VisitAddNewComponent(in EntryCommandModel cmd) => WriteSingletonFieldDestroyFromRuntimeDefault(in cmd, _syncDisposalEmitter.VisitAddNewComponent);
    public VoidVisitor VisitAddFromPrefab(in EntryCommandModel cmd) => WriteSingletonFieldDestroyFromRuntimeDefault(in cmd, _syncDisposalEmitter.VisitAddFromPrefab);
    
    public VoidVisitor VisitAddAllFromHierarchy(in EntryCommandModel cmd) => VoidVisitor.Default;
    public VoidVisitor VisitAddAllFromChildren(in EntryCommandModel cmd) => VoidVisitor.Default;
    public VoidVisitor VisitAddAllFromParent(in EntryCommandModel cmd) => VoidVisitor.Default;
    
    public VoidVisitor VisitAddOpenGeneric(in EntryCommandModel cmd) => WriteSingletonFieldDisposalDefault(in cmd, _syncDisposalEmitter.VisitAddOpenGeneric);
    
    private VoidVisitor WriteSingletonFieldDisposalDefault(in EntryCommandModel cmd, EntryVisitFunc<VoidVisitor> syncFallback) {
        if (!ParseHelpers.InheritsFromIAsyncDisposable(_entryType)) {
            return ParseHelpers.InheritsFromIDisposable(_entryType)
                ? syncFallback(in cmd)
                : VoidVisitor.Default;
        }

        string? key = ParseHelpers.GetKeyExprOrNull(_registration);
        string fieldName = key is null
            ? BuildHelpers.BuildSingletonFieldNameNotKeyed(_entryType)
            : BuildHelpers.BuildSingletonFieldNameKeyed(_entryType, key);

        _writer.WriteLine($"await {fieldName}?.{nameof(IAsyncDisposable.DisposeAsync)}();");
        
        return VoidVisitor.Default;
    }

    private VoidVisitor WriteSingletonFieldDestroyFromRuntimeDefault(in EntryCommandModel cmd, EntryVisitFunc<VoidVisitor> defer)
        => defer(in cmd);
}