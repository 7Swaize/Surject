using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Surject.Abstractions.Attributes;
using Surject.Generators.Models.Collections;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;
using Surject.Shared.Helpers;

namespace Surject.Generators.Discovery.Injection;

internal static class InjectableTargetParser {
    private static readonly (Type AttributeType, InjectionMode Mode)[] InjectionAttributes = [
        (typeof(InjectAttribute), InjectionMode.Standard),
        (typeof(InjectOptionalAttribute), InjectionMode.Optional),
        (typeof(InjectLazyAttribute), InjectionMode.Lazy),
        (typeof(InjectPrimaryAttribute), InjectionMode.Primary),
        (typeof(InjectAsyncAttribute), InjectionMode.Async)
        
    ];

    internal static EquatableArray<InjectableMemberModel> GetMembersToInject(
        INamedTypeSymbol target,
        Compilation compilation,
        DiscoveryUtils utils)
    {
        ImmutableArray<InjectableMemberModel>.Builder builder = ImmutableArray.CreateBuilder<InjectableMemberModel>();
        Dictionary<INamedTypeSymbol, InjectionMode> modeMap = new(SymbolEqualityComparer.Default);

        foreach (var attr in InjectionAttributes) {
            INamedTypeSymbol? symbol = compilation.GetTypeByMetadataName(attr.AttributeType.FullName!);
            
            if (symbol is not null) {
                modeMap[symbol] = attr.Mode;
            }
        }

        foreach (ISymbol member in target.GetMembers()) {
            if (GetMode(member, modeMap) is var mode && mode == InjectionMode.None) {
                continue;
            }
            
            builder.Add(Parse(member, mode, compilation, utils, modeMap));
        }
        
        return builder.ToImmutable().AsEquatableArray();
    }

    private static InjectionMode GetMode(ISymbol symbol, Dictionary<INamedTypeSymbol, InjectionMode> modeMap) {
        InjectionMode res = InjectionMode.None;
        
        foreach (AttributeData attr in symbol.GetAttributes()) {
            if (attr.AttributeClass is not null && modeMap.TryGetValue(attr.AttributeClass, out InjectionMode mode)) {
                res |= mode;
            }
        }

        ITypeSymbol? fieldType = symbol switch {
            IFieldSymbol field => field.Type,
            IPropertySymbol prop => prop.Type,
            IParameterSymbol param => param.Type,
            _ => null
        };

        if (fieldType is IArrayTypeSymbol) {
            res |= InjectionMode.All;
        }

        return res;
    }
    
    private static InjectableMemberModel Parse(
        ISymbol targetSymbol,
        InjectionMode mode,
        Compilation compilation,
        DiscoveryUtils utils,
        Dictionary<INamedTypeSymbol, InjectionMode> modeMap)
    {
        string? id = CheckId(targetSymbol, compilation);
        InjectionMode effectiveMode = id is not null 
            ? mode | InjectionMode.Keyed
            : mode;
        
        return targetSymbol switch {
            IFieldSymbol field => new InjectableMemberModel {
                Name = field.Name,
                Mode = effectiveMode,
                Site = InjectionSiteKind.Field,
                TypeToRequest = GetTypeToRequest(field.Type, mode, utils),
                Id = id
            },
            IPropertySymbol property => new InjectableMemberModel {
                Name = property.Name,
                Mode = effectiveMode,
                Site = InjectionSiteKind.Property,
                TypeToRequest = GetTypeToRequest(property.Type, mode, utils),
                Id = id
            },
            IParameterSymbol param => new InjectableMemberModel {
                Name = param.Name,
                Mode = effectiveMode,
                Site = InjectionSiteKind.Parameter,
                TypeToRequest = GetTypeToRequest(param.Type, mode, utils),
                Id = id
            },
            IMethodSymbol method => new InjectableMemberModel {
                Name = method.Name,
                Mode = effectiveMode,
                Site = InjectionSiteKind.Method,
                MethodRef = new MethodModel(method, utils),
                Id = id,
                Parameters = method.Parameters.Select(param => {
                    InjectionMode parameterMode = GetMode(param, modeMap);
                    return Parse(
                        param,
                        parameterMode == InjectionMode.None ? InjectionMode.Standard : parameterMode,
                        compilation,
                        utils,
                        modeMap
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

    private static ITypeReferenceModel GetTypeToRequest(ITypeSymbol target, InjectionMode mode, DiscoveryUtils utils) {
        return mode switch {
            _ when (mode & (InjectionMode.Lazy | InjectionMode.Async)) == (InjectionMode.Lazy | InjectionMode.Async) =>
                utils.TypeReferenceModelFactory.CreateOrGetTypeReferenceModel(
                    target.As<INamedTypeSymbol>().TypeArguments[0].As<IArrayTypeSymbol>().ElementType
                ),
            _ when (mode & InjectionMode.Lazy) != 0 =>
                utils.TypeReferenceModelFactory.CreateOrGetTypeReferenceModel(
                    target.As<INamedTypeSymbol>().TypeArguments[0]
                ),
            _ when (mode & InjectionMode.Async) != 0 =>
                utils.TypeReferenceModelFactory.CreateOrGetTypeReferenceModel(
                    target.As<INamedTypeSymbol>().TypeArguments[0]
                ),
            _ when (mode & InjectionMode.All) != 0 =>
                utils.TypeReferenceModelFactory.CreateOrGetTypeReferenceModel(
                    target.As<IArrayTypeSymbol>().ElementType
                ),
            _ => utils.TypeReferenceModelFactory.CreateOrGetTypeReferenceModel(target)
        };
    }
}