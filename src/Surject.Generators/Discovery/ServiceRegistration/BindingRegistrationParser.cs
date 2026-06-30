using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Surject.Abstractions.Attributes;
using Surject.Abstractions.Registrations;
using Surject.Generators.Models.Collections;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Factories;
using Surject.Generators.Models.Primitives;
using Surject.Shared.Helpers;

namespace Surject.Generators.Discovery.ServiceRegistration;

internal static class BindingRegistrationParser {
    internal static RegistrationModel? Parse(
        InvocationExpressionSyntax rootInvocation,
        TypeReferenceModelFactory typeRefFactory,
        SemanticModel semanticModel)
    {
        if (!TryExtractChain(rootInvocation, out var outermost, out Span<InvocationExpressionSyntax> modSyntax)) {
            return null;
        }

        if (!TryParseEntry(outermost, typeRefFactory, semanticModel, out EntryCommandModel entry)) {
            return null;
        }
        
        ImmutableArray<ModifierCommandModel> modifiers = ParseModifiers(modSyntax, typeRefFactory, semanticModel);
        
        RegistrationNormalizer normalizer = new RegistrationNormalizer(modifiers);
        return entry.Accept<RegistrationNormalizer, RegistrationModel>(ref normalizer);
    }
    
    private static bool TryExtractChain(
        InvocationExpressionSyntax root,
        out InvocationExpressionSyntax entry,
        out Span<InvocationExpressionSyntax> modifiers)
    {
        List<InvocationExpressionSyntax> chain = new(9);
        ExpressionSyntax? current = root;

        while (current is InvocationExpressionSyntax inv) {
            chain.Add(inv);
            current = inv.Expression is MemberAccessExpressionSyntax ma
                ? ma.Expression
                : null;
        }

        if (chain.Count == 0) {
            entry = root;
            modifiers = default;
            return false;
        }

        entry = chain[^1];
        modifiers = SpanHelpers.AsSpan(chain)[..^1];
        modifiers.Reverse();
        return true;
    }

    private static bool TryParseEntry(
        InvocationExpressionSyntax syntax,
        TypeReferenceModelFactory typeRefFactory,
        SemanticModel semanticModel,
        out EntryCommandModel entry) 
    {
        entry = default;
        if (semanticModel.GetSymbolInfo(syntax).Symbol is not IMethodSymbol method
            || !IsRegistryMethod(method, semanticModel)) 
        {
            return false;
        }
        
        LifetimeKind lifetime = ExtractLifetime(syntax, semanticModel);
        ITypeReferenceModel? implType = ExtractNthTypeArg(method, 0, typeRefFactory);

        entry = method.Name switch {
            nameof(IServiceRegistry.Add) 
                => EntryCommandModel.Add(CreateServiceModel(implType!), lifetime),
            nameof(IServiceRegistry.AddFactory)
                => EntryCommandModel.AddFactory(implType!, lifetime, RewriteAnonymousExpr(1)),
            nameof(IServiceRegistry.AddOpenGeneric)
                => EntryCommandModel.AddOpenGeneric(CreateServiceModel(ExtractNthArgTypeOf(syntax, 0, typeRefFactory, semanticModel)), lifetime),
            nameof(IServiceRegistry.AddToCollection)
                => EntryCommandModel.AddToCollection(CreateServiceModel(implType!), lifetime, ExtractNthIntArg(syntax, 1, semanticModel)),
            nameof(IServiceRegistry.AddPrimaryToCollection) 
                => EntryCommandModel.AddPrimaryToCollection(CreateServiceModel(implType!), lifetime, ExtractNthIntArg(syntax, 1, semanticModel)),
            nameof(IServiceRegistry.AddAsyncFactory)
                => EntryCommandModel.AddAsyncFactory(implType!, lifetime, RewriteAnonymousExpr(1)),
            nameof(IServiceRegistry.AddFromHierarchy)
                => EntryCommandModel.AddFromHierarchy(CreateServiceModel(implType!), lifetime),
            nameof(IServiceRegistry.AddAllFromHierarchy)
                => EntryCommandModel.AddAllFromHierarchy(CreateServiceModel(implType!), lifetime),
            nameof(IServiceRegistry.AddFromSibling)
                => EntryCommandModel.AddFromSibling(CreateServiceModel(implType!), lifetime),
            nameof(IServiceRegistry.AddFromChildren)
                => EntryCommandModel.AddFromChildren(CreateServiceModel(implType!), lifetime),
            nameof(IServiceRegistry.AddAllFromChildren)
                => EntryCommandModel.AddAllFromChildren(CreateServiceModel(implType!), lifetime),
            nameof(IServiceRegistry.AddFromParent)
                => EntryCommandModel.AddFromParent(CreateServiceModel(implType!), lifetime),
            nameof(IServiceRegistry.AddAllFromParent)
                => EntryCommandModel.AddAllFromParent(CreateServiceModel(implType!), lifetime),
            nameof(IServiceRegistry.AddNewComponent)
                => EntryCommandModel.AddNewComponent(CreateServiceModel(implType!), lifetime),
            nameof(IServiceRegistry.AddFromPrefab)
                => EntryCommandModel.AddFromPrefab(CreateServiceModel(implType!), lifetime, ExtractNthArgAsString(syntax, 1)),
            nameof(IServiceRegistry.Decorate)
                => EntryCommandModel.Decorate(
                    contract: implType!,
                    decorator: ExtractNthTypeArg(method, 1, typeRefFactory)!
                ),
            _ => ThrowHelpers.ThrowUnhandledBranch<EntryCommandModel>(method.Name)
        };
        
        return true;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        string RewriteAnonymousExpr(int index) => 
            InvokeAnonymousExprFQNRewriter(ExtractAnonymousExpr(syntax, index), semanticModel);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ServiceModel CreateServiceModel(ITypeReferenceModel typeRef) => new() {
            TypeRef = typeRef,
            CreationModel = ExtractServiceCreationModel(
                (INamedTypeSymbol)typeRef.UnderlyingTypeSymbol,
                typeRefFactory,
                semanticModel
            )
        };
    }

    private static ImmutableArray<ModifierCommandModel> ParseModifiers(
        Span<InvocationExpressionSyntax> modSyntax,
        TypeReferenceModelFactory typeRefFactory,
        SemanticModel semanticModel) 
    {
        if (modSyntax.IsEmpty) return ImmutableArray<ModifierCommandModel>.Empty;
        
        ImmutableArray<ModifierCommandModel>.Builder builder = 
            ImmutableArray.CreateBuilder<ModifierCommandModel>(modSyntax.Length);
        foreach (InvocationExpressionSyntax syntax in modSyntax) {
            builder.Add(ParseModifier(syntax, typeRefFactory, semanticModel));
        }

        return builder.MoveToImmutable();
    }

    private static ModifierCommandModel ParseModifier(
        InvocationExpressionSyntax syntax,
        TypeReferenceModelFactory typeRefFactory,
        SemanticModel semanticModel)
    {
        if (semanticModel.GetSymbolInfo(syntax).Symbol is not IMethodSymbol method) {
            return default;
        }

        return method.Name switch {
            // Many of these have the same name, so we can get away with just checking one interface
            nameof(IBindingBuilder<>.To)
                => ModifierCommandModel.To(
                    ExtractNthTypeArg(method, 0, typeRefFactory) ?? ExtractNthArgTypeOf(syntax, 0, typeRefFactory, semanticModel)
                ),
            nameof(IBindingBuilder<>.ToImmediateImplementedInterfaces)
                => ModifierCommandModel.ToImmediateImplementedInterfaces(),
            nameof(IBindingBuilder<>.ToAllImplementedInterfaces)
                => ModifierCommandModel.ToAllImplementedInterfaces(),
            nameof(IBindingBuilder<>.WithId) 
                => ModifierCommandModel.WithId(ExtractNthStringArg(syntax, 0, semanticModel)),
            nameof(IBindingBuilder<>.Pooled) 
                => ModifierCommandModel.Pooled(ExtractNthIntArg(syntax, 0, semanticModel)),
            nameof(IBindingBuilder<>.Eager) 
                => ModifierCommandModel.Eager(),
            nameof(IBindingBuilder<>.Lazy) 
                => ModifierCommandModel.Lazy(),
            nameof(IBindingBuilder<>.FromFactory) 
                => ModifierCommandModel.FromFactory(RewriteAnonymousExpr(1)),
            nameof(IBindingBuilder<>.FromInjectFactory) 
                => ModifierCommandModel.FromInjectFactory(),
            nameof(IBindingBuilder<>.WithArgument) 
                => ModifierCommandModel.WithArgument(
                    ExtractNthStringArg(syntax, 0, semanticModel),
                    ExtractNthTypeArg(method, 0, typeRefFactory)!,
                    ExtractNthArgAsString(syntax, 1)
                ),
            nameof(IBindingBuilder<>.WhenInjectedInto)
                => ModifierCommandModel.WhenInjectedInto(ExtractNthTypeArg(method, 0, typeRefFactory)!),
            nameof(IBindingBuilder<>.When) 
                => ModifierCommandModel.When(RewriteAnonymousExpr(1)),
            nameof(IBindingBuilder<>.OverrideExisting) 
                => ModifierCommandModel.OverrideExisting(),
            nameof(IBindingBuilder<>.AsCollection) 
                => ModifierCommandModel.AsCollection(ExtractNthIntArg(syntax, 0, semanticModel)),
            nameof(IBindingBuilder<>.AsPrimary) 
                => ModifierCommandModel.AsPrimary(),
            nameof(IBindingBuilder<>.DoNotDispose)
                => ModifierCommandModel.DoNotDispose(),
            nameof(IBindingBuilder<>.TrackDisposable) 
                => ModifierCommandModel.TrackDisposable(),
            
            
            // Unity component specific
            nameof(IComponentInstantiationBindingBuilder<>.UnderTransform) 
                => ModifierCommandModel.UnderTransform(ExtractNthArgAsString(syntax, 0)),
            nameof(IComponentInstantiationBindingBuilder<>.UnderObjectOfType) 
                => ModifierCommandModel.UnderObjectOfType(ExtractNthTypeArg(method, 0, typeRefFactory)!),
            nameof(IComponentInstantiationBindingBuilder<>.WithGameObjectName)
                => ModifierCommandModel.WithGameObjectName(ExtractNthStringArg(syntax, 0, semanticModel)),
            nameof(IComponentInstantiationBindingBuilder<>.DoNotDestroy) 
                => ModifierCommandModel.DoNotDestroy(),
            _ => ThrowHelpers.ThrowUnhandledBranch<ModifierCommandModel>(method.Name)
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        string RewriteAnonymousExpr(int index) => 
            InvokeAnonymousExprFQNRewriter(ExtractAnonymousExpr(syntax, index), semanticModel);
    }

    private static bool IsRegistryMethod(IMethodSymbol method, SemanticModel semanticModel) {
        INamedTypeSymbol? type = method.ContainingType;
        INamedTypeSymbol? target = semanticModel.Compilation.GetTypeByMetadataName(typeof(IServiceRegistry).FullName!);
        if (!SymbolEqualityComparer.Default.Equals(type, target)) {
            return false;
        }
        
        return true;
    }
    
    private static LifetimeKind ExtractLifetime(InvocationExpressionSyntax syntax, SemanticModel semanticModel) {
        Optional<object?> constant = semanticModel.GetConstantValue(
            syntax.ArgumentList.Arguments[0].Expression);

        return constant.HasValue
            ? (LifetimeKind)(byte)constant.Value!
            : default; // We don't necessarily care about failure
    }

    private static ITypeReferenceModel? ExtractNthTypeArg(IMethodSymbol method, int index, TypeReferenceModelFactory typeRefFactory) {
        return method.TypeArguments.Length > index
            ? typeRefFactory.CreateOrGetTypeReferenceModel(method.TypeArguments[index])
            : null;
    }

    private static AnonymousFunctionExpressionSyntax ExtractAnonymousExpr(InvocationExpressionSyntax syntax, int index) {
        return (AnonymousMethodExpressionSyntax)syntax.ArgumentList.Arguments[index].Expression;
    }

    private static ITypeReferenceModel ExtractNthArgTypeOf(
        InvocationExpressionSyntax syntax,
        int index,
        TypeReferenceModelFactory typeRefFactory,
        SemanticModel semanticModel)
    {
        TypeOfExpressionSyntax @typeof = (TypeOfExpressionSyntax)syntax.ArgumentList.Arguments[index].Expression;
        return typeRefFactory.CreateOrGetTypeReferenceModel(
            (ITypeSymbol)semanticModel.GetSymbolInfo(@typeof.Type).Symbol!
        );
    }

    private static int ExtractNthIntArg(InvocationExpressionSyntax syntax, int index, SemanticModel semanticModel) {
        Optional<object?> constant = semanticModel.GetConstantValue(
            syntax.ArgumentList.Arguments[index].Expression);

        return constant.HasValue
            ? (int)constant.Value!
            : 0; // We don't necessarily care about failure
    }

    private static string ExtractNthStringArg(InvocationExpressionSyntax syntax, int index, SemanticModel semanticModel) {
        Optional<object?> constant = semanticModel.GetConstantValue(
            syntax.ArgumentList.Arguments[index].Expression);
        
        return constant.HasValue
            ? (string)constant.Value!
            : ""; // We don't necessarily care about failure
    }

    private static string ExtractNthArgAsString(InvocationExpressionSyntax syntax, int index) {
        return syntax.ArgumentList.Arguments[index].Expression.ToString();
    }

    private static ServiceCreationModel ExtractServiceCreationModel(
        INamedTypeSymbol serviceDef,
        TypeReferenceModelFactory typeRefFactory,
        SemanticModel semanticModel)
    {
        INamedTypeSymbol? targetAttr =
            semanticModel.Compilation.GetTypeByMetadataName(typeof(ConstructWithAttribute).FullName!);
        
        foreach (IMethodSymbol method in serviceDef.GetMembers().OfType<IMethodSymbol>()) {
            if (!method.ValidateAnnotatedWith(targetAttr!)) {
                continue;
            }
            
            return method.MethodKind switch {
                MethodKind.Constructor => new ConstructorCreationModel(method, typeRefFactory),
                MethodKind.Ordinary => new FactoryMethodCreationModel(method, typeRefFactory),
                _ => ThrowHelpers.ThrowUnhandledBranch<ServiceCreationModel>(method.MethodKind)
            };
        }
        
        return new MonoBehaviourCreationModel();
    }

    private static string InvokeAnonymousExprFQNRewriter(AnonymousFunctionExpressionSyntax expr, SemanticModel semanticModel) {
        AnonymousExprFQNRewriter rewriter = new AnonymousExprFQNRewriter(semanticModel);
        AnonymousFunctionExpressionSyntax rewritten = (AnonymousFunctionExpressionSyntax)rewriter.Visit(expr);
        return rewritten.ToFullString();
    }

    internal static CacheBuilderFlags GetCacheBuilderFlags(
        in EntryCommandModel entry,
        EquatableArray<ModifierCommandModel> modifiers)
    {
        CacheBuilderFlags result = CacheBuilderFlags.None;

        if (entry.Kind == EntryKind.AddAsyncFactory) {
            result |= CacheBuilderFlags.Async;
        }

        foreach (ref readonly ModifierCommandModel modifier in modifiers) {
            result |= modifier.Kind switch {
                ModifierKind.Lazy => CacheBuilderFlags.Lazy,
                ModifierKind.AsCollection => CacheBuilderFlags.MultiBind,
                ModifierKind.AsPrimary => CacheBuilderFlags.Primary,
                ModifierKind.WithId => CacheBuilderFlags.Keyed,
                _ => CacheBuilderFlags.None
            };
        }
        
        return result;
    }
}