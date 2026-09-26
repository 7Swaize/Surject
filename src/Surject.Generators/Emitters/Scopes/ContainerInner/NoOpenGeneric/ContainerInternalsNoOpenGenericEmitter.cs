using System.CodeDom.Compiler;
using Surject.Abstractions.Registrations;
using Surject.Abstractions.Resolutions;
using Surject.Generators.Emitters.Helpers;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;
using Surject.Unity.Handles;

namespace Surject.Generators.Emitters.Scopes.ContainerInner.NoOpenGeneric;

internal readonly ref struct ContainerInternalsNoOpenGenericEmitter : IChainedEmitter {
    private readonly ContainerModel _model;
    
    internal ContainerInternalsNoOpenGenericEmitter(ContainerModel model) => _model = model;
    
    public void Emit(IndentedTextWriter writer) {
        EmitCtor(writer);
        EmitProperties(writer);
        EmitMembers(writer);
        EmitDisposableTrackers(writer);
        EmitDisposeMethods(writer);
    }

    private void EmitCtor(IndentedTextWriter writer) {
        ITypeReferenceModel modelType = _model.Decl.AsTypeRef; 
        
        writer.WriteMultiline(
            $$"""
              internal {{BuildHelpers.BuildContainerType(modelType)}}(
                  global::{{typeof(IResolver).FullName}}? parentResolver,
                  {{modelType.FQNConstructedArgBased}} scopeProvider)
              {
                  ParentResolver = parentResolver;
                  Resolver = new {{BuildHelpers.BuildResolverType(_model.Decl.AsTypeRef)}}(this, parentResolver, scopeProvider);
              }
              """
        );
    }

    private void EmitProperties(IndentedTextWriter writer) {
        writer.WriteLine($"public global::{typeof(IResolver).FullName} Resolver {{ get; }}");
        writer.WriteLine($"public global::{typeof(IResolver).FullName}? ParentResolver {{ get; }}");
        writer.WriteLine();
    }

    private void EmitMembers(IndentedTextWriter writer) {
        new ContainerInternalSingletonConcreteBindingNoOpenGenericEmitter(_model).Emit(writer);
        writer.WriteLine();
        
        new ContainerInternalMultiBindingNoOpenGenericEmitter(_model).Emit(writer);
        writer.WriteLine();
        
        new ContainerInternalDiscoveryCollectionNoOpenGenericEmitter(_model).Emit(writer);
        writer.WriteLine();
    }

    // This is independent of an open-generic context, so we can emit it here.
    private void EmitDisposableTrackers(IndentedTextWriter writer) {
        if (!ParseHelpers.ShouldTrackTransientDisposal(_model)) {
            return;
        }
        
        writer.WriteLine($"internal readonly global::{typeof(DisposableTracker).FullName} __disposables = new();");
        writer.WriteLine($"internal readonly global::{typeof(AsyncDisposableTracker).FullName} __asyncDisposables = new();");
        writer.WriteLine();
    }

    private void EmitDisposeMethods(IndentedTextWriter writer) {
        new ContainerInternalDisposalNoOpenGenericEmitter(_model).Emit(writer);
    }
}