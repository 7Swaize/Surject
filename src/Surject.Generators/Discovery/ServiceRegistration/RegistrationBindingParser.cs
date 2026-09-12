using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Surject.Abstractions.Attributes;
using Surject.Abstractions.Registrations;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Factories;
using Surject.Generators.Models.Primitives;
using Surject.Shared.Helpers;

namespace Surject.Generators.Discovery.ServiceRegistration;

internal static class RegistrationBindingParser {
    internal static RegistrationModel? Parse(
        InvocationExpressionSyntax rootInvocation,
        TypeReferenceModelFactory typeRefFactory,
        SemanticModel semanticModel)
    {
        if (!TryExtractChain(rootInvocation, out var outermost, out var modSyntax)) {
            return null;
        }

        if (!TryParseEntry(outermost, typeRefFactory, semanticModel, out var entryModel)) {
            return null;
        }

        ImmutableArray<ModifierCommandModel> modifiers = ParseModifiers(modSyntax, typeRefFactory, semanticModel);
    
        RegistrationNormalizer normalizer = new  RegistrationNormalizer(modifiers);
        return entryModel.Accept<RegistrationNormalizer, RegistrationModel>(ref normalizer);
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
        modifiers = chain.AsSpanUnsafe()[..^1];
        modifiers.Reverse();
        return true;
    }

    private static bool TryParseEntry(
        InvocationExpressionSyntax entrySyntax,
        TypeReferenceModelFactory typeRefFactory,
        SemanticModel semanticModel,
        out EntryCommandModel entryModel)
    {
        entryModel = default;
        if (semanticModel.GetSymbolInfo(entrySyntax).Symbol is not IMethodSymbol method
            || !IsRegistryMethod(method, semanticModel))
        {
            return false;
        }

        LifetimeKind lifetime = ExtractLifetime(entrySyntax, semanticModel);
        ITypeReferenceModel? implType = ExtractNthTypeArg(method, 0, typeRefFactory);

        entryModel = method.Name switch {
            nameof(IServiceRegistry.Add)
                => EntryCommandModel.Add(CreateServiceModelAlias(implType!), lifetime),
            nameof(IServiceRegistry.AddFactory)
                => EntryCommandModel.AddFactory(
                    implType!,
                    lifetime,
                    RewriteDelegateArgumentIntoModel(entrySyntax, 1, typeRefFactory, semanticModel)
                ),
            nameof(IServiceRegistry.AddOpenGeneric)
                => EntryCommandModel.AddOpenGeneric(
                    CreateServiceModelAlias(ExtractNthArgTypeOf(entrySyntax, 1, typeRefFactory, semanticModel)),
                    lifetime
                ),
            nameof(IServiceRegistry.AddToCollection)
                => EntryCommandModel.AddToCollection(
                    CreateServiceModelAlias(implType!),
                    lifetime,
                    ExtractNthCompileTimeConstantArg<int>(entrySyntax, 1, semanticModel)
                ),
            nameof(IServiceRegistry.AddPrimaryToCollection)
                => EntryCommandModel.AddPrimaryToCollection(
                    CreateServiceModelAlias(implType!),
                    lifetime,
                    ExtractNthCompileTimeConstantArg<int>(entrySyntax, 1, semanticModel)
                ),
            nameof(IServiceRegistry.AddAsyncFactory)
                => EntryCommandModel.AddAsyncFactory(
                    implType!,
                    lifetime,
                    RewriteDelegateArgumentIntoModel(entrySyntax, 1, typeRefFactory, semanticModel)
                ),
            nameof(IServiceRegistry.AddFromHierarchy)
                => EntryCommandModel.AddFromHierarchy(CreateServiceModelAlias(implType!), lifetime),
            nameof(IServiceRegistry.AddAllFromHierarchy)
                => EntryCommandModel.AddAllFromHierarchy(CreateServiceModelAlias(implType!), lifetime),
            nameof(IServiceRegistry.AddFromSibling)
                => EntryCommandModel.AddFromSibling(CreateServiceModelAlias(implType!), lifetime),
            nameof(IServiceRegistry.AddFromChildren)
                => EntryCommandModel.AddFromChildren(CreateServiceModelAlias(implType!), lifetime),
            nameof(IServiceRegistry.AddAllFromChildren)
                => EntryCommandModel.AddAllFromChildren(CreateServiceModelAlias(implType!), lifetime),
            nameof(IServiceRegistry.AddFromParent)
                => EntryCommandModel.AddFromParent(CreateServiceModelAlias(implType!), lifetime),
            nameof(IServiceRegistry.AddAllFromParent)
                => EntryCommandModel.AddAllFromParent(CreateServiceModelAlias(implType!), lifetime),
            nameof(IServiceRegistry.AddNewComponent)
                => EntryCommandModel.AddNewComponent(CreateServiceModelAlias(implType!), lifetime),
            nameof(IServiceRegistry.AddFromPrefab)
                => EntryCommandModel.AddFromPrefab(
                    CreateServiceModelAlias(implType!),
                    lifetime,
                    ExtractNthArgAsString(entrySyntax, 1)
                ),
            _ => ThrowHelpers.ThrowUnhandledBranch<EntryCommandModel>(method.Name)
        };

        return true;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ServiceModel CreateServiceModelAlias(ITypeReferenceModel typeRef) {
            return CreateServiceModel(typeRef, typeRefFactory, semanticModel);
        }
    }

    private static ImmutableArray<ModifierCommandModel> ParseModifiers(
        Span<InvocationExpressionSyntax> modSyntax,
        TypeReferenceModelFactory typeRefFactory,
        SemanticModel semanticModel) 
    {
        if (modSyntax.IsEmpty) {
            return ImmutableArray<ModifierCommandModel>.Empty;
        }

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
            nameof(IBindingBuilder<>.To)
                => ModifierCommandModel.To(
                    ExtractNthTypeArg(method, 0, typeRefFactory) ?? ExtractNthArgTypeOf(syntax, 0, typeRefFactory, semanticModel)
                ),
            nameof(IBindingBuilder<>.ToImmediateImplementedInterfaces)
                => ModifierCommandModel.ToImmediateImplementedInterfaces(),
            nameof(IBindingBuilder<>.ToAllImplementedInterfaces)
                => ModifierCommandModel.ToAllImplementedInterfaces(),
            nameof(IBindingBuilder<>.WithId)
                => ModifierCommandModel.WithId(
                    ExtractNthTypeArg(method, 0, typeRefFactory)!,
                    ExtractNthArgAsString(syntax, 0)
                ),
            nameof(IBindingBuilder<>.Eager)
                => ModifierCommandModel.Eager(),
            nameof(IBindingBuilder<>.Lazy)
                => ModifierCommandModel.Lazy(),
            nameof(IBindingBuilder<>.WithArgument)
                => ModifierCommandModel.WithArgument(
                    ExtractNthCompileTimeConstantArg<string>(syntax, 0, semanticModel),
                    ExtractNthTypeArg(method, 0, typeRefFactory)!,
                    ExtractNthArgAsString(syntax, 1)
                ),
            nameof(IBindingBuilder<>.OverrideExisting)
                => ModifierCommandModel.OverrideExisting(),
            nameof(IBindingBuilder<>.AsCollection)
                => ModifierCommandModel.AsCollection(ExtractNthCompileTimeConstantArg<int>(syntax, 0, semanticModel)),
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
                => ModifierCommandModel.WithGameObjectName(ExtractNthCompileTimeConstantArg<string>(syntax, 0, semanticModel)),
            nameof(IComponentInstantiationBindingBuilder<>.DoNotDestroy)
                => ModifierCommandModel.DoNotDestroy(),
            _ => ThrowHelpers.ThrowUnhandledBranch<ModifierCommandModel>(method.Name)
        };
    }

    private static bool IsRegistryMethod(IMethodSymbol method, SemanticModel semanticModel) {
        INamedTypeSymbol? containing = method.ContainingType;
        INamedTypeSymbol? target = semanticModel.Compilation.GetTypeByMetadataName(typeof(IServiceRegistry).FullName!);

        return SymbolEqualityComparer.Default.Equals(containing, target);
    }

    private static ServiceModel CreateServiceModel(
        ITypeReferenceModel typeRef,
        TypeReferenceModelFactory typeRefFactory,
        SemanticModel semanticModel)
    {
        ServiceCreationModel GetServiceCreationModel() {
            INamedTypeSymbol? constructAttr = 
                semanticModel.Compilation.GetTypeByMetadataName(typeof(ConstructWithAttribute).FullName!);

            foreach (IMethodSymbol method in typeRef.UnderlyingTypeSymbol.GetMembers().OfType<IMethodSymbol>()) {
                if (!method.ValidateAnnotatedWith(constructAttr!)) {
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

        return new ServiceModel {
            TypeRef = typeRef,
            CreationModel = GetServiceCreationModel()
        };
    }
    
    private static LifetimeKind ExtractLifetime(InvocationExpressionSyntax syntax, SemanticModel semanticModel) {
        Optional<object?> constant = semanticModel.GetConstantValue(
            syntax.ArgumentList.Arguments[0].Expression
        );

        return constant.HasValue
            ? (LifetimeKind)(byte)constant.Value!
            : ThrowHelpers.ThrowNonConstantExpressionException<LifetimeKind>();
    }
    
    private static ITypeReferenceModel? ExtractNthTypeArg(IMethodSymbol method, int index, TypeReferenceModelFactory typeRefFactory) {
        return method.TypeArguments.Length > 0
            ? typeRefFactory.CreateOrGetTypeReferenceModel(method.TypeArguments[index])
            : null;
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

    private static T ExtractNthCompileTimeConstantArg<T>(
        InvocationExpressionSyntax syntax,
        int index,
        SemanticModel semanticModel)
    {
        Optional<object?> constant = semanticModel.GetConstantValue(
            syntax.ArgumentList.Arguments[index].Expression
        );
        
        return constant.HasValue
            ? (T)constant.Value!
            : ThrowHelpers.ThrowNonConstantExpressionException<T>();
    }

    private static string ExtractNthArgAsString(InvocationExpressionSyntax syntax, int index) {
        return syntax.ArgumentList.Arguments[index].Expression.ToString();
    }

    private static RewrittenDelegateArgumentModel RewriteDelegateArgumentIntoModel(
        InvocationExpressionSyntax syntax,
        int index,
        TypeReferenceModelFactory typeRefFactory,
        SemanticModel semanticModel)
    {
        ExpressionSyntax argSyntax = syntax.ArgumentList.Arguments[index].Expression;
        IMethodSymbol targetSymbol = (semanticModel.GetSymbolInfo(argSyntax).Symbol as IMethodSymbol)!;
        MethodModel targetMethodAsModel = new MethodModel(targetSymbol, typeRefFactory);
        
        if (argSyntax is not AnonymousMethodExpressionSyntax expressionSyntax) {

            return new RewrittenDelegateArgumentModel {
                Kind = RewrittenDelegateArgumentKind.MethodGroup,
                Method = targetMethodAsModel,
                RewrittenInternals = null
            };
        }
        
        SyntaxNode body = expressionSyntax.Body;
        AnonymousExprInternalFQNRewriter rewriter = new AnonymousExprInternalFQNRewriter(semanticModel);
        string rewritten = ((AnonymousFunctionExpressionSyntax)rewriter.Visit(body)).ToFullString();

        return new RewrittenDelegateArgumentModel {
            Kind = RewrittenDelegateArgumentKind.LambdaExpr,
            Method = targetMethodAsModel,
            RewrittenInternals = rewritten
        };
    }
}