using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Surject.Abstractions.Attributes;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;

namespace Surject.Generators.Discovery.Injection;

internal static class InjectableTargetParser {
    private static readonly (Type AttributeType, InjectionMode Mode)[] InjectionAttributes = [
        (typeof(InjectAttribute), InjectionMode.Standard),
        (typeof(InjectOptionalAttribute), InjectionMode.Optional),
        (typeof(InjectLazyAttribute), InjectionMode.Lazy),
        (typeof(InjectPrimaryAttribute), InjectionMode.Primary),
        (typeof(InjectAsyncAttribute), InjectionMode.Async)
        
    ];

    internal static ImmutableArray<InjectableMemberModel> GetMembersToInject(
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
            if (TryGetMode(member, modeMap) is { } mode) {
                builder.Add(Parse(member, mode, compilation, utils, modeMap));
            }
        }
        
        return builder.ToImmutable();
    }

    private static InjectionMode? TryGetMode(ISymbol symbol, Dictionary<INamedTypeSymbol, InjectionMode> modeMap) {
        foreach (AttributeData attr in symbol.GetAttributes()) {
            if (attr.AttributeClass is not null && modeMap.TryGetValue(attr.AttributeClass, out InjectionMode mode)) {
                return mode;
            }
        }
        
        return null;
    }
    
    private static InjectableMemberModel Parse(
        ISymbol targetSymbol,
        InjectionMode mode,
        Compilation compilation,
        DiscoveryUtils utils,
        Dictionary<INamedTypeSymbol, InjectionMode> modeMap)
    {
        return targetSymbol switch {
            IFieldSymbol field => new InjectableMemberModel {
                Mode = mode,
                Site = InjectionSiteKind.Field,
                TypeRef = utils.TypeReferenceModelFactory.CreateOrGetTypeReferenceModel(field.Type),
                Id = CheckId(targetSymbol, compilation)
            },
            IPropertySymbol prop => new InjectableMemberModel {
                Mode = mode,
                Site = InjectionSiteKind.Property,
                TypeRef = utils.TypeReferenceModelFactory.CreateOrGetTypeReferenceModel(prop.Type),
                Id = CheckId(targetSymbol, compilation)
            },
            IParameterSymbol param => new InjectableMemberModel {
                Mode = mode,
                Site = InjectionSiteKind.Parameter,
                TypeRef = utils.TypeReferenceModelFactory.CreateOrGetTypeReferenceModel(param.Type),
                Id = CheckId(targetSymbol, compilation)
            },
            IMethodSymbol meth => new InjectableMemberModel {
                Mode = mode,
                Site = InjectionSiteKind.Method,
                MethodRef = new MethodModel(meth, utils),
                Id = CheckId(targetSymbol, compilation),
                Parameters = [.. meth.Parameters.Select(
                    p => Parse(p, TryGetMode(p, modeMap) ?? InjectionMode.Standard, compilation, utils, modeMap)
                )]
            },
            _ => throw new InvalidOperationException($"Invalid target symbol kind: {targetSymbol.Kind}")
        };
    }

    private static string? CheckId(ISymbol targetSymbol, Compilation compilation) {
        INamedTypeSymbol? attr = compilation.GetTypeByMetadataName(typeof(IdAttribute).FullName!);

        if (!targetSymbol.ValidateAnnotatedWith(attr!, out AttributeData? data)) {
            return null;
        }
        
        return data.ConstructorArguments[0].Value?.ToString();
    }
}