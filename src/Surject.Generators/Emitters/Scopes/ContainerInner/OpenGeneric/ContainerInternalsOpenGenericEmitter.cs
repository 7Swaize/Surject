using System.CodeDom.Compiler;
using Surject.Generators.Models.Concepts;

namespace Surject.Generators.Emitters.Scopes.ContainerInner.OpenGeneric;

internal readonly ref struct ContainerInternalsOpenGenericEmitter : IChainedEmitter {
    private readonly ContainerModel _container;
    private readonly OpenGenericInjectionLinkage _linkage;

    internal ContainerInternalsOpenGenericEmitter(ContainerModel model, OpenGenericInjectionLinkage linkage) {
        _container = model;
        _linkage = linkage;
    }

    public void Emit(IndentedTextWriter writer) {
        EmitMembers(writer);
        EmitDisposalMethods(writer);
    }

    private void EmitMembers(IndentedTextWriter writer) {
        new ContainerInternalSingletonConcreteBindingOpenGenericEmitter(_container, _linkage).Emit(writer);
        writer.WriteLine();
        
        new ContainerInternalMultiBindingOpenGenericEmitter(_container, _linkage).Emit(writer);
        writer.WriteLine();
    }

    private void EmitDisposalMethods(IndentedTextWriter writer) {
        new ContainerInternalDisposalOpenGenericEmitter(_container, _linkage).Emit(writer);
    }
}