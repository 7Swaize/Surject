using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Surject.Abstractions.Attributes;
using Surject.Generators.Models.Collections;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Factories;
using Surject.Generators.Models.Primitives;
using Surject.Shared.Helpers;

namespace Surject.Generators.Discovery.Injection;

internal static class InjectableTargetParser {
    private static readonly (Type AttributeType, InjectionDeferralKind Deferral)[] InjectionAttributes = [
        (typeof(InjectAttribute), InjectionDeferralKind.Standard),
        (typeof(InjectOptionalAttribute), InjectionDeferralKind.Optional),
        (typeof(InjectLazyAttribute), InjectionDeferralKind.Lazy),
        (typeof(InjectPrimaryAttribute), InjectionDeferralKind.Primary),
        (typeof(InjectAsyncAttribute), InjectionDeferralKind.Async)
        
    ];

    internal static EquatableArray<InjectableMemberModel> GetMembersToInject(
        INamedTypeSymbol target,
        Compilation compilation,
        TypeReferenceModelFactory typeRefFactory)
    {
        ImmutableArray<InjectableMemberModel>.Builder builder = ImmutableArray.CreateBuilder<InjectableMemberModel>();
        Dictionary<INamedTypeSymbol, InjectionDeferralKind> deferralMap = new(SymbolEqualityComparer.Default);

        foreach (var attr in InjectionAttributes) {
            INamedTypeSymbol? symbol = compilation.GetTypeByMetadataName(attr.AttributeType.FullName!);
            
            if (symbol is not null) {
                deferralMap[symbol] = attr.Deferral;
            }
        }

        foreach (ISymbol member in target.GetMembers()) {
            if (GetDeferral(member, deferralMap) is var deferral && deferral == InjectionDeferralKind.None) {
                continue;
            }
            
            builder.Add(Parse(member, deferral, compilation, typeRefFactory, deferralMap));
        }
        
        return builder.ToImmutable().AsEquatableArray();
    }

    private static InjectionDeferralKind GetDeferral(ISymbol symbol, Dictionary<INamedTypeSymbol, InjectionDeferralKind> deferralMap) {
        InjectionDeferralKind res = InjectionDeferralKind.None;
        
        foreach (AttributeData attr in symbol.GetAttributes()) {
            if (attr.AttributeClass is not null && deferralMap.TryGetValue(attr.AttributeClass, out InjectionDeferralKind deferral)) {
                res |= deferral;
            }
        }

        ITypeSymbol? fieldType = symbol switch {
            IFieldSymbol field => field.Type,
            IPropertySymbol prop => prop.Type,
            IParameterSymbol param => param.Type,
            _ => null
        };

        if (fieldType is IArrayTypeSymbol) {
            res |= InjectionDeferralKind.All;
        }

        return res;
    }
    
    private static InjectableMemberModel Parse(
        ISymbol targetSymbol,
        InjectionDeferralKind deferralKind,
        Compilation compilation,
        TypeReferenceModelFactory typeRefFactory,
        Dictionary<INamedTypeSymbol, InjectionDeferralKind> deferralMap)
    {
        string? id = CheckId(targetSymbol, compilation);
        InjectionDeferralKind effectiveDeferralKind = id is not null 
            ? deferralKind | InjectionDeferralKind.Keyed
            : deferralKind;
        
        return targetSymbol switch {
            IFieldSymbol field => new InjectableMemberModel {
                Name = field.Name,
                Deferral = effectiveDeferralKind,
                Site = InjectionSiteKind.Field,
                TypeToRequest = GetTypeToRequest(field.Type, deferralKind, typeRefFactory),
                Id = id
            },
            IPropertySymbol property => new InjectableMemberModel {
                Name = property.Name,
                Deferral = effectiveDeferralKind,
                Site = InjectionSiteKind.Property,
                TypeToRequest = GetTypeToRequest(property.Type, deferralKind, typeRefFactory),
                Id = id
            },
            IParameterSymbol param => new InjectableMemberModel {
                Name = param.Name,
                Deferral = effectiveDeferralKind,
                Site = InjectionSiteKind.Parameter,
                TypeToRequest = GetTypeToRequest(param.Type, deferralKind, typeRefFactory),
                Id = id
            },
            IMethodSymbol method => new InjectableMemberModel {
                Name = method.Name,
                Deferral = effectiveDeferralKind,
                Site = InjectionSiteKind.Method,
                MethodRef = new MethodModel(method, typeRefFactory),
                Id = id,
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
            _ => ThrowHelpers.ThrowUnhandledBranch<InjectableMemberModel>(targetSymbol.Kind)
        };
    }

    private static string? CheckId(ISymbol targetSymbol, Compilation compilation) {
        INamedTypeSymbol? attr = compilation.GetTypeByMetadataName(typeof(IdAttribute).FullName!);

        if (!targetSymbol.ValidateAnnotatedWith(attr!, out AttributeData? data)) {
            return null;
        }
        
        return data.ConstructorArguments[0].Value?.ToString();
    }

    private static ITypeReferenceModel GetTypeToRequest(
        ITypeSymbol target,
        InjectionDeferralKind deferralKind,
        TypeReferenceModelFactory typeRefFactory)
    {
        return deferralKind switch {
            _ when (deferralKind & (InjectionDeferralKind.Lazy | InjectionDeferralKind.Async)) == (InjectionDeferralKind.Lazy | InjectionDeferralKind.Async) =>
                typeRefFactory.CreateOrGetTypeReferenceModel(
                    target.As<INamedTypeSymbol>().TypeArguments[0].As<IArrayTypeSymbol>().ElementType
                ),
            _ when (deferralKind & InjectionDeferralKind.Lazy) != 0 =>
                typeRefFactory.CreateOrGetTypeReferenceModel(
                    target.As<INamedTypeSymbol>().TypeArguments[0]
                ),
            _ when (deferralKind & InjectionDeferralKind.Async) != 0 =>
                typeRefFactory.CreateOrGetTypeReferenceModel(
                    target.As<INamedTypeSymbol>().TypeArguments[0]
                ),
            _ when (deferralKind & InjectionDeferralKind.All) != 0 =>
                typeRefFactory.CreateOrGetTypeReferenceModel(
                    target.As<IArrayTypeSymbol>().ElementType
                ),
            _ => typeRefFactory.CreateOrGetTypeReferenceModel(target)
        };
    }
}