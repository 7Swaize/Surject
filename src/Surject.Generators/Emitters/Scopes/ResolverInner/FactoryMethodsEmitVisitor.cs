using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Surject.Abstractions.Resolutions;
using Surject.Generators.Discovery.ServiceRegistration;
using Surject.Generators.Emitters.Helpers;
using Surject.Generators.Emitters.Helpers.Visitors;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;
using Surject.Shared.Helpers;

namespace Surject.Generators.Emitters.Scopes.ResolverInner;

internal readonly struct FactoryMethodsEmitVisitor : IEntryCommandVisitor<VoidVisitor> {
    private static readonly EntryRegistrationTypeVisitor _entryRegistrationTypeVisitor = new();
    private readonly IndentedTextWriter _writer;
    private readonly RegistrationModel _registration;

    internal FactoryMethodsEmitVisitor(IndentedTextWriter writer, RegistrationModel registration) {
        _writer = writer;
        _registration = registration;
    }

    public VoidVisitor VisitAdd(in EntryCommandModel cmd) => VoidVisitor.Default;
    public VoidVisitor VisitAddOpenGeneric(in EntryCommandModel cmd) => VoidVisitor.Default;
    public VoidVisitor VisitAddToCollection(in EntryCommandModel cmd) => VoidVisitor.Default;
    public VoidVisitor VisitAddPrimaryToCollection(in EntryCommandModel cmd) => VoidVisitor.Default;
    public VoidVisitor VisitAddFromHierarchy(in EntryCommandModel cmd) => VoidVisitor.Default;
    public VoidVisitor VisitAddAllFromHierarchy(in EntryCommandModel cmd) => VoidVisitor.Default;
    public VoidVisitor VisitAddFromSibling(in EntryCommandModel cmd) => VoidVisitor.Default;
    public VoidVisitor VisitAddFromChildren(in EntryCommandModel cmd) => VoidVisitor.Default;
    public VoidVisitor VisitAddAllFromChildren(in EntryCommandModel cmd) => VoidVisitor.Default;
    public VoidVisitor VisitAddFromParent(in EntryCommandModel cmd) => VoidVisitor.Default;
    public VoidVisitor VisitAddAllFromParent(in EntryCommandModel cmd) => VoidVisitor.Default;
    public VoidVisitor VisitAddNewComponent(in EntryCommandModel cmd) => VoidVisitor.Default;
    public VoidVisitor VisitAddFromPrefab(in EntryCommandModel cmd) => VoidVisitor.Default;
    public VoidVisitor VisitAddAmbient(in EntryCommandModel cmd) => VoidVisitor.Default;

    public VoidVisitor VisitAddFactory(in EntryCommandModel cmd) {
        ITypeReferenceModel coreType = _entryRegistrationTypeVisitor.VisitAddFromHierarchy(cmd)!;
        RewrittenDelegateArgumentModel model = cmd.RewrittenDelegateArgument;

        if (model.Kind == RewrittenDelegateArgumentKind.MethodGroup) {
            EmitHelpers.EmitMethodImplAttribute(_writer, MethodImplOptions.AggressiveInlining);
        }

        string? key = ParseHelpers.GetKeyExprOrNull(_registration);
        string methodName = key is null
            ? BuildHelpers.BuildSyncFactoryMethodNameNotKeyed(coreType)
            : BuildHelpers.BuildSyncFactoryMethodNameKeyed(coreType, key);
        
        _writer.WriteLine($"private static {coreType.FQNConstructedArgBased} {methodName}(");
        _writer.Indent++;
        _writer.WriteLine($"global::{typeof(IResolver).FullName} resolver)");
        _writer.Indent--;
        _writer.WriteLine("{");

        switch (model.Kind) {
            case RewrittenDelegateArgumentKind.MethodGroup:
                _writer.WriteLine($"return {model.Method.ContainingType.FQNConstructedArgBased}.{model.Method.NameWithGenericParams}(resolver);");
                break;
            case RewrittenDelegateArgumentKind.LambdaExprExpressionBody:
                _writer.WriteLine($"return {model.RewrittenInternals}"); // no semi colon
                break;
            case RewrittenDelegateArgumentKind.LambdaExprStatementBody:
                _writer.WriteMultiline(model.RewrittenInternals!); // no semi colon
                break;
            default:
                ThrowHelpers.ThrowUnhandledBranch(model.Kind);
                break;
        }
        
        _writer.Indent--;
        _writer.WriteLine("}");
        
        return VoidVisitor.Default;
    }

    public VoidVisitor VisitAddAsyncFactory(in EntryCommandModel cmd) {
        ITypeReferenceModel coreType = _entryRegistrationTypeVisitor.VisitAddFromHierarchy(cmd)!;
        RewrittenDelegateArgumentModel model = cmd.RewrittenDelegateArgument;
        
        string? key = ParseHelpers.GetKeyExprOrNull(_registration);
        string methodName = key is null
            ? BuildHelpers.BuildSyncFactoryMethodNameNotKeyed(coreType)
            : BuildHelpers.BuildSyncFactoryMethodNameKeyed(coreType, key);
        
        _writer.WriteLine($"private static async global::{typeof(Task).FullName}<{coreType.FQNConstructedArgBased}>");
        _writer.Indent++;
        _writer.WriteLine($"{methodName}(");
        _writer.Indent++;
        _writer.WriteLine($"global::{typeof(IResolver).FullName} resolver,");
        _writer.WriteLine($"global::{typeof(CancellationToken).FullName} ct)");
        _writer.Indent--;
        _writer.Indent--;
        _writer.WriteLine("{");
        
        switch (model.Kind) {
            case RewrittenDelegateArgumentKind.MethodGroup:
                _writer.WriteLine($"return await {model.Method.ContainingType.FQNConstructedArgBased}.{model.Method.NameWithGenericParams}(resolver);");
                break;
            case RewrittenDelegateArgumentKind.LambdaExprExpressionBody:
                _writer.WriteLine($"return {model.RewrittenInternals}"); // no semi colon
                break;
            case RewrittenDelegateArgumentKind.LambdaExprStatementBody:
                _writer.WriteMultiline(model.RewrittenInternals!); // no semi colon
                break;
            default:
                ThrowHelpers.ThrowUnhandledBranch(model.Kind);
                break;
        }
        
        _writer.Indent--;
        _writer.WriteLine("}");
        
        return VoidVisitor.Default;
    }
}