using System.CodeDom.Compiler;
using Surject.Generators.Discovery.ServiceRegistration;
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

    private VoidVisitor EmitStandardResolve() { }
}