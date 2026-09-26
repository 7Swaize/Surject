using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Threading.Tasks;
using Surject.Generators.Discovery.ServiceRegistration;
using Surject.Generators.Emitters.Helpers.Visitors;
using Surject.Generators.Emitters.Scopes.ContainerInner.SharedVisitors;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;

namespace Surject.Generators.Emitters.Scopes.ContainerInner.OpenGeneric;

internal readonly ref struct ContainerInternalDisposalOpenGenericEmitter : IChainedEmitter {
    private readonly ContainerModel _container;
    private readonly OpenGenericInjectionLinkage _linkage;

    internal ContainerInternalDisposalOpenGenericEmitter(ContainerModel container, OpenGenericInjectionLinkage linkage) {
        _container = container;
        _linkage = linkage;
    }

    public void Emit(IndentedTextWriter writer) {
        EmitOpenGenericDisposePartial(writer);
        EmitOpenGenericDisposeAsyncPartial(writer);
    }

    private void EmitOpenGenericDisposePartial(IndentedTextWriter writer) {
        writer.WriteLine($"partial void DisposeOpenGenerics() {{");
        writer.Indent++;
        
        HashSet<ITypeReferenceModel> uniqueEntryRegistrationTypes = [];
        EntryRegistrationTypeVisitor registrationVisitor = new();

        foreach (RegistrationModel registration in _container.Registrations) {
            if (registration.Entry.Kind != EntryKind.AddOpenGeneric) {
                continue;
            }
            
            if (registration.Entry.Lifetime == LifetimeKind.Transient) {
                continue;
            }
            
            if ((registration.ModifiersDescriptor & ModifierKind.DoNotDispose) == ModifierKind.DoNotDispose) {
                continue;
            }
            
            ITypeReferenceModel? entryType = registration.Entry.Accept<EntryRegistrationTypeVisitor, ITypeReferenceModel?>(ref registrationVisitor);

            if (entryType == null || !uniqueEntryRegistrationTypes.Add(entryType)) {
                continue;
            }
            
            foreach (ITypeReferenceModel impl in _linkage.Linkage[entryType]) {
                SingletonFieldSyncDisposalEmitterVisitor syncDisposalEmitter = new(writer, impl, registration);
                registration.Entry.Accept<SingletonFieldSyncDisposalEmitterVisitor, VoidVisitor>(ref syncDisposalEmitter);
            }
        }
        
        writer.Indent--;
        writer.WriteLine("}");
        writer.WriteLine();
    }

    private void EmitOpenGenericDisposeAsyncPartial(IndentedTextWriter writer) {
        writer.WriteLine($"private async partial {typeof(Task).FullName} DisposeOpenGenericsAsync() {{");
        writer.Indent++;
        
        HashSet<ITypeReferenceModel> uniqueEntryRegistrationTypes = [];
        EntryRegistrationTypeVisitor registrationVisitor = new();
        
        foreach (RegistrationModel registration in _container.Registrations) {
            if (registration.Entry.Kind != EntryKind.AddOpenGeneric) {
                continue;
            }
            
            if (registration.Entry.Lifetime == LifetimeKind.Transient) {
                continue;
            }
            
            if ((registration.ModifiersDescriptor & ModifierKind.DoNotDispose) == ModifierKind.DoNotDispose) {
                continue;
            }
            
            ITypeReferenceModel? entryType = registration.Entry.Accept<EntryRegistrationTypeVisitor, ITypeReferenceModel?>(ref registrationVisitor);

            if (entryType == null || !uniqueEntryRegistrationTypes.Add(entryType)) {
                continue;
            }
            
            foreach (ITypeReferenceModel impl in _linkage.Linkage[entryType]) {
                SingletonFieldAsyncDisposalEmitterVisitor asyncDisposalEmitter = new(writer, impl, registration);
                registration.Entry.Accept<SingletonFieldAsyncDisposalEmitterVisitor, VoidVisitor>(ref asyncDisposalEmitter);
            }
        }
        
        writer.Indent--;
        writer.WriteLine("}");
        writer.WriteLine();
    }
}