using System;
using System.CodeDom.Compiler;
using System.Text;
using Surject.Generators.Discovery.ServiceRegistration;
using Surject.Generators.Emitters.Helpers;
using Surject.Generators.Models.Collections;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;
using Surject.Shared.Helpers;

namespace Surject.Generators.Emitters.Scopes.ResolverInner;

internal readonly struct PerImplResolverEmitVisitor : IEntryCommandVisitor<VoidVisitor> {
    private enum UnityRuntimeBackedDiscoverySearchKind : byte {
        Hierarchy,
        Sibling,
        Children,
        Parent
    }
    
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

    public VoidVisitor VisitAddFactory(in EntryCommandModel cmd) => EmitFactoryBackedResolve();
    
    public VoidVisitor VisitAddFromHierarchy(in EntryCommandModel cmd) => EmitDiscoverBackedResolve(UnityRuntimeBackedDiscoverySearchKind.Hierarchy);
    public VoidVisitor VisitAddFromSibling(in EntryCommandModel cmd) => EmitDiscoverBackedResolve(UnityRuntimeBackedDiscoverySearchKind.Sibling);
    public VoidVisitor VisitAddFromChildren(in EntryCommandModel cmd) => EmitDiscoverBackedResolve(UnityRuntimeBackedDiscoverySearchKind.Children);
    public VoidVisitor VisitAddFromParent(in EntryCommandModel cmd) => EmitDiscoverBackedResolve(UnityRuntimeBackedDiscoverySearchKind.Parent);
    
    public VoidVisitor VisitAddOpenGeneric(in EntryCommandModel cmd) => throw new NotImplementedException();
    public VoidVisitor VisitAddAsyncFactory(in EntryCommandModel cmd) => throw new NotImplementedException();
    public VoidVisitor VisitAddAllFromHierarchy(in EntryCommandModel cmd) => throw new NotImplementedException();
    public VoidVisitor VisitAddAllFromChildren(in EntryCommandModel cmd) => throw new NotImplementedException();
    public VoidVisitor VisitAddAllFromParent(in EntryCommandModel cmd) => throw new NotImplementedException();
    public VoidVisitor VisitAddNewComponent(in EntryCommandModel cmd) => throw new NotImplementedException();
    public VoidVisitor VisitAddFromPrefab(in EntryCommandModel cmd) => throw new NotImplementedException();
    public VoidVisitor VisitAddAmbient(in EntryCommandModel cmd) => throw new NotImplementedException();

    private VoidVisitor EmitStandardResolve() {
        string? key = ParseHelpers.GetKeyExprOrNull(_registration);

        string fieldName = key is null
            ? BuildHelpers.BuildSingletonFieldNameNotKeyed(_coreType)
            : BuildHelpers.BuildSingletonFieldNameKeyed(_coreType, key);

        string methodName = key is null
            ? BuildHelpers.BuildResolveMethodNameNotKeyed(_coreType)
            : BuildHelpers.BuildResolveMethodNameKeyed(_coreType, key);
        
        string constructExpr = BuildConstructionExpr();
        
        _writer.WriteLine($"private {_coreType.FQNConstructedArgBased} {methodName}()");
        _writer.Indent++;

        if (_registration.Entry.Lifetime is LifetimeKind.Transient) {
            _writer.WriteLine("=>");
            _writer.Indent++;
            _writer.WriteMultiline($"{constructExpr}");
            _writer.Indent--;
        }
        else {
            _writer.WriteLine("=>");
            _writer.Indent++;
            _writer.WriteMultiline($"_c.{fieldName} ??=");
            _writer.Indent++;
            _writer.WriteMultiline($"{constructExpr}");
            _writer.Indent--;
            _writer.Indent--;
        }
        
        _writer.Indent--;
        _writer.WriteLine();

        return VoidVisitor.Default;
    }

    private VoidVisitor EmitFactoryBackedResolve() {
        string? key = ParseHelpers.GetKeyExprOrNull(_registration);

        string fieldName = key is null
            ? BuildHelpers.BuildSingletonFieldNameNotKeyed(_coreType)
            : BuildHelpers.BuildSingletonFieldNameKeyed(_coreType, key);

        string methodName = key is null
            ? BuildHelpers.BuildResolveMethodNameNotKeyed(_coreType)
            : BuildHelpers.BuildResolveMethodNameKeyed(_coreType, key);
        
        string factoryMethodName = key is null
            ? BuildHelpers.BuildSyncFactoryMethodNameNotKeyed(_coreType)
            : BuildHelpers.BuildSyncFactoryMethodNameKeyed(_coreType, key);
        
        _writer.WriteLine($"internal {_coreType.FQNConstructedArgBased} {methodName}() {{");
        _writer.Indent++;
        
        _writer.WriteLine(
            _registration.Entry.Lifetime == LifetimeKind.Transient
                ? $"return {factoryMethodName}(this);"
                : $"return _c.{fieldName} ??= {factoryMethodName}(this);"
        );
        
        _writer.Indent--;
        _writer.WriteLine("}");
        _writer.WriteLine();

        return VoidVisitor.Default;
    }
    
    private string BuildConstructionExpr() {
        ServiceCreationModel creation = _registration.Entry.Service.CreationModel;

        if (creation is ConstructorCreationModel constructorCreationModel) {
            string headerExpr = $"new {_coreType.FQNConstructedArgBased}";
            return BuildConstructionExpr(
                headerExpr,
                constructorCreationModel.Constructor.Parameters,
                constructorCreationModel.ConstructWithInjectionTargets.AsSpan()
            );
        }

        if (creation is FactoryMethodCreationModel factoryMethodCreationModel) {
            string headerExpr = $"{_coreType.FQNConstructedArgBased}.{factoryMethodCreationModel.Method}";
            return BuildConstructionExpr(
                headerExpr,
                factoryMethodCreationModel.Method.Parameters,
                factoryMethodCreationModel.ConstructWithInjectionTargets.AsSpan()
            );
        }

        return ThrowHelpers.ThrowEmitException<string>($"Unhandled creation model: {creation.GetType().Name}");
    }
    
    private string BuildConstructionExpr(
        string headerExpr,
        EquatableArray<ParameterModel> allParams,
        ReadOnlySpan<InjectionTargetModel> injectableParams)
    {

        StringBuilder sb = new StringBuilder();
        sb.Append(headerExpr).Append('(');

        int next = 0;
        for (int i = 0; i < allParams.Length; i++) {
            string suffix = (i == allParams.Length - 1) ? "" : ",";

            string paramExpr = ParseHelpers.TryGetArgumentOverride(_registration, allParams[i].Name, out string? overrideExpr)
                ? overrideExpr
                : BuildHelpers.BuildResolverCall(in injectableParams[next++]);

            sb.Append("\n    ").Append(paramExpr).Append(suffix);
        }

        sb.Append("\n);");
        return sb.ToString();
    }

}