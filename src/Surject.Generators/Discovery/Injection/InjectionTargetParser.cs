using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Surject.Abstractions.Attributes;
using Surject.Generators.Models.Collections;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Factories;
using Surject.Generators.Models.Primitives;
using Surject.Shared.Helpers;

namespace Surject.Generators.Discovery.Injection;

internal static class InjectionTargetParser {
    private static readonly (Type AttributeType, InjectionDeferralKind Deferral)[] InjectionAttributes = [
        (typeof(InjectAttribute), InjectionDeferralKind.Standard),
        (typeof(InjectOptionalAttribute), InjectionDeferralKind.Optional),
        (typeof(InjectPrimaryAttribute), InjectionDeferralKind.Primary),
        (typeof(InjectAllAttribute), InjectionDeferralKind.All),
        (typeof(InjectAsyncAttribute), InjectionDeferralKind.Async)
    ];
    
    internal static EquatableArray<InjectionTargetModel> GetContainedInjectionTargets(
        INamedTypeSymbol containing,
        Compilation compilation,
        TypeReferenceModelFactory typeRefFactory) 
    {
        ImmutableArray<InjectionTargetModel>.Builder builder = ImmutableArray.CreateBuilder<InjectionTargetModel>();
        Dictionary<INamedTypeSymbol, InjectionDeferralKind> deferralMap = new(SymbolEqualityComparer.Default);

        foreach (var attr in InjectionAttributes) {
            INamedTypeSymbol? symbol = compilation.GetTypeByMetadataName(attr.AttributeType.FullName!);

            if (symbol is not null) {
                deferralMap[symbol] = attr.Deferral;
            }
        }

        foreach (ISymbol member in containing.GetMembers()) {
            if (GetDeferral(member, deferralMap) is var deferral && deferral == InjectionDeferralKind.None) {
                continue;
            }
            
            builder.Add(Parse(member, deferral, compilation, typeRefFactory, deferralMap));
        }

        return builder.ToImmutable().AsEquatableArray();
    }

    private static InjectionDeferralKind GetDeferral(ISymbol member, Dictionary<INamedTypeSymbol, InjectionDeferralKind> deferralMap) {
        InjectionDeferralKind res = InjectionDeferralKind.None;
        
        foreach (AttributeData attr in member.GetAttributes()) {
            if (attr.AttributeClass is not null && deferralMap.TryGetValue(attr.AttributeClass, out InjectionDeferralKind deferral)) {
                res |= deferral;
            }
        }

        return res;
    }

    private static InjectionTargetModel Parse(
        ISymbol targetSymbol,
        InjectionDeferralKind deferralKind,
        Compilation compilation,
        TypeReferenceModelFactory typeRefFactory,
        Dictionary<INamedTypeSymbol, InjectionDeferralKind> deferralMap) 
    {
        (ITypeReferenceModel? type, string? stringRepr) id = CheckId(targetSymbol, compilation, typeRefFactory);
        InjectionDeferralKind effectiveDeferralKind = id.type is not null
            ? deferralKind | InjectionDeferralKind.Keyed
            : deferralKind;
        
        return targetSymbol switch {
            IFieldSymbol field => new InjectionTargetModel {
                Name = field.Name,
                InjectionSiteKind = InjectionSiteKind.Field,
                InjectionDeferralKind = effectiveDeferralKind,
                UnwrappedTypeToRequest = GetTypeToRequest(field.Type, effectiveDeferralKind, typeRefFactory),
                IdType = id.type,
                IdAsText = id.stringRepr
            },
            IPropertySymbol property => new InjectionTargetModel {
                Name = property.Name,
                InjectionSiteKind = InjectionSiteKind.Property,
                InjectionDeferralKind = effectiveDeferralKind,
                UnwrappedTypeToRequest = GetTypeToRequest(property.Type, effectiveDeferralKind, typeRefFactory),
                IdType = id.type,
                IdAsText = id.stringRepr
            },
            IParameterSymbol param => new InjectionTargetModel {
                Name = param.Name,
                InjectionSiteKind = InjectionSiteKind.Parameter,
                InjectionDeferralKind = effectiveDeferralKind,
                UnwrappedTypeToRequest = GetTypeToRequest(param.Type, effectiveDeferralKind, typeRefFactory),
                IdType = id.type,
                IdAsText = id.stringRepr
            },
            IMethodSymbol method => new InjectionTargetModel {
                Name = method.Name,
                InjectionSiteKind = InjectionSiteKind.Method,
                InjectionDeferralKind = effectiveDeferralKind,
                MethodRef = new MethodModel(method, typeRefFactory),
                IdType = id.type,
                IdAsText = id.stringRepr,
                Parameters = method.Parameters.Select(param => {
                    InjectionDeferralKind parameterDeferralKind = GetDeferral(param, deferralMap);
                    return Parse(
                        param,
                        parameterDeferralKind == InjectionDeferralKind.None ? InjectionDeferralKind.Standard : parameterDeferralKind,
                        compilation,
                        typeRefFactory,
                        deferralMap
                    );
                }).ToImmutableArray().AsEquatableArray()
            },
            _ => ThrowHelpers.ThrowUnhandledBranch<InjectionTargetModel>(targetSymbol.Kind)
        };
    }
    
    private static (ITypeReferenceModel? type, string? stringRepr) CheckId(
        ISymbol symbol,
        Compilation compilation,
        TypeReferenceModelFactory typeRefFactory)
    {
        INamedTypeSymbol? attr = compilation.GetTypeByMetadataName(typeof(IdAttribute).FullName!);
        
        if (!symbol.ValidateAnnotatedWith(attr!, out AttributeData? data)) {
            return (null, null);
        }

        TypedConstant value = data.ConstructorArguments[0];

        return (
            typeRefFactory.CreateOrGetTypeReferenceModel(value.Type!),
            value.Value is string s ? SymbolDisplay.FormatLiteral(s, quote: true) : value.Value?.ToString()
        );
    }
    
    private static ITypeReferenceModel GetTypeToRequest(
        ITypeSymbol target,
        InjectionDeferralKind deferralKind,
        TypeReferenceModelFactory typeRefFactory)
    {
        if ((deferralKind & InjectionDeferralKind.Async) == InjectionDeferralKind.Async) {
            return typeRefFactory.CreateOrGetTypeReferenceModel(
                (deferralKind & InjectionDeferralKind.All) == InjectionDeferralKind.All
                    ? target.As<INamedTypeSymbol>().TypeArguments[0].As<IArrayTypeSymbol>().ElementType
                    : target.As<INamedTypeSymbol>().TypeArguments[0]
            );
        }

        if ((deferralKind & InjectionDeferralKind.All) == InjectionDeferralKind.All) {
            return typeRefFactory.CreateOrGetTypeReferenceModel(
                target.As<IArrayTypeSymbol>().ElementType
            );
        }

        return typeRefFactory.CreateOrGetTypeReferenceModel(target);
    }
}