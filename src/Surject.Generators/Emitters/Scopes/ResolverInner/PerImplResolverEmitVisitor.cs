using System;
using System.CodeDom.Compiler;
using Surject.Generators.Discovery.ServiceRegistration;
using Surject.Generators.Emitters.Helpers;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;

namespace Surject.Generators.Emitters.Scopes.ResolverInner;

internal readonly struct PerImplResolverEmitVisitor : IEntryCommandVisitor<VoidVisitor> {
    private readonly IndentedTextWriter _writer;
    private readonly RegistrationModel _registration;
    private readonly ITypeReferenceModel _coreType;

    internal PerImplResolverEmitVisitor(IndentedTextWriter writer, RegistrationModel registration, ITypeReferenceModel coreType) {
        _writer = writer;
        _registration = registration;
        _coreType = coreType;
    }
    
    public VoidVisitor VisitAdd(in EntryCommandModel cmd) => EmitStandardResolve();
    public VoidVisitor VisitAddToCollection(in EntryCommandModel cmd) => EmitStandardResolve();
    public VoidVisitor VisitAddPrimaryToCollection(in EntryCommandModel cmd) => EmitStandardResolve();
    
    public VoidVisitor VisitAddFactory(in EntryCommandModel cmd) => throw new NotImplementedException();
    public VoidVisitor VisitAddOpenGeneric(in EntryCommandModel cmd) => throw new NotImplementedException();
    public VoidVisitor VisitAddAsyncFactory(in EntryCommandModel cmd) => throw new NotImplementedException();
    public VoidVisitor VisitAddFromHierarchy(in EntryCommandModel cmd) => throw new NotImplementedException();
    public VoidVisitor VisitAddAllFromHierarchy(in EntryCommandModel cmd) => throw new NotImplementedException();
    public VoidVisitor VisitAddFromSibling(in EntryCommandModel cmd) => throw new NotImplementedException();
    public VoidVisitor VisitAddFromChildren(in EntryCommandModel cmd) => throw new NotImplementedException();
    public VoidVisitor VisitAddAllFromChildren(in EntryCommandModel cmd) => throw new NotImplementedException();
    public VoidVisitor VisitAddFromParent(in EntryCommandModel cmd) => throw new NotImplementedException();
    public VoidVisitor VisitAddAllFromParent(in EntryCommandModel cmd) => throw new NotImplementedException();
    public VoidVisitor VisitAddNewComponent(in EntryCommandModel cmd) => throw new NotImplementedException();
    public VoidVisitor VisitAddFromPrefab(in EntryCommandModel cmd) => throw new NotImplementedException();
    public VoidVisitor VisitAddAmbient(in EntryCommandModel cmd) => throw new NotImplementedException();

    private VoidVisitor EmitStandardResolve() {
        string? key = ParseHelpers.GetKeyExprOrNull(_registration);
        string methodName = key is null
            ? BuildHelpers.BuildSyncFactoryMethodNameNotKeyed(_coreType)
            : BuildHelpers.BuildSyncFactoryMethodNameKeyed(_coreType, key);

        _writer.Write($"private {_coreType.FQNConstructedArgBased} {methodName}() {{");
        _writer.Indent++;
        
        return VoidVisitor.Default;
    }
}