using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Surject.Generators.Models.Collections;
using Surject.Generators.Models.Primitives;

namespace Surject.Generators.Models.Factories;

internal sealed class TypeReferenceModelFactory {
    private sealed record TypeReferenceModel : ITypeReferenceModel {
        internal TypeReferenceModel(ITypeSymbol symbol, TypeReferenceModelFactory factory) {
            factory._cache.Add(symbol, this);
            
            FQNGenericOmitted = symbol.GetFQNWithGenericsOmitted();
            FQNGenericBased = symbol.GetGenericDefinitionFQN();
            FQNArityBased = symbol.GetGenericArityFQN();
            FQNConstructedArgBased = symbol.GetConstructedTypeFQN();
            FlattenedNameArityBased = symbol.GetFlattenedConstructedName();
            Namespace = symbol.ContainingNamespace?.IsGlobalNamespace == false
                ? symbol.ContainingNamespace.ToDisplayString()
                : null;
            
            IsBasedOnTypeParameter = symbol.TypeKind == TypeKind.TypeParameter;

            if (symbol is INamedTypeSymbol named) {
                IsGeneric = named.IsGenericType;
                IsUnboundGeneric = named.IsUnboundGenericType;
                Constraints = named.GetTypeParamConstraints();

                if (IsGeneric && !IsBasedOnTypeParameter && !named.IsUnboundGenericType) {
                    UnboundGenericTypeRef = 
                        factory.CreateOrGetTypeReferenceModel(named.ConstructUnboundGenericType());

                    TypeArguments = [
                        .. named.TypeArguments.Select(typeSymbol =>
                            factory.CreateOrGetTypeReferenceModel(typeSymbol))
                    ];

                    TypeParameters = [
                        .. named.TypeParameters.Select(typeSymbol =>
                            factory.CreateOrGetTypeReferenceModel(typeSymbol))
                    ];
                }
                else {
                    TypeArguments = [];
                    TypeParameters = [];
                    UnboundGenericTypeRef = null;
                }
                
                BaseType = named.BaseType != null
                    ? factory.CreateOrGetTypeReferenceModel(named.BaseType)
                    : null;

                ImmediateInterfaces = [
                    .. named.Interfaces.Select(@interface =>
                        factory.CreateOrGetTypeReferenceModel(@interface))
                ];

                AllInterfaces = [
                    .. named.AllInterfaces.Select(@interface =>
                        factory.CreateOrGetTypeReferenceModel(@interface))
                ];
            }
            
            IsTrueValueType = symbol.IsValueType;
            IsRecord = symbol.IsRecord;
            TypeKind = symbol.TypeKind;
            SpecialType = symbol.SpecialType;

            _underlyingTypeSymbol = new WeakReference<ITypeSymbol>(symbol, false);
        }

        public ITypeReferenceModel ConstructFromTypeArguments(EquatableArray<ITypeReferenceModel> targs) {
            string BuildFlattenedName() {
                StringBuilder sb = new StringBuilder();
                sb.Append(FlattenedNameArityBased);

                foreach (ITypeReferenceModel arg in targs) {
                    sb.Append(arg.FlattenedNameArityBased);
                }
                
                return sb.ToString();
            }

            return this with {
                TypeArguments = targs,
                IsUnboundGeneric = false,
                FQNConstructedArgBased = $"{FQNGenericOmitted}<{string.Join(", ", targs.Select(t => t.FQNConstructedArgBased))}>",
                FlattenedNameArityBased = BuildFlattenedName()
            };
        }
        
        // need custom hash impl so we don't get stack overflow with circular type defs. 
        public override int GetHashCode() {
            unchecked {
                int hashCode = 17;
                hashCode = hashCode * (int)0xA5555529 + FQNGenericBased.GetHashCode() +
                           TypeArguments.Length.GetHashCode();
                return hashCode;
            }
        }

        // sadly this has to be done because I want to exclude the 'INamedTypeSymbol' member
        public bool Equals(ITypeReferenceModel? other) {
            return other is not null
                   && FQNGenericOmitted == other.FQNGenericOmitted
                   && FQNGenericBased == other.FQNGenericBased
                   && TypeArguments.Length == other.TypeArguments.Length
                   && FQNArityBased == other.FQNArityBased
                   && FQNConstructedArgBased == other.FQNConstructedArgBased
                   && FlattenedNameArityBased == other.FlattenedNameArityBased
                   && IsBasedOnTypeParameter == other.IsBasedOnTypeParameter
                   && IsGeneric == other.IsGeneric
                   && IsUnboundGeneric == other.IsUnboundGeneric
                   && IsTrueValueType == other.IsTrueValueType
                   && TypeKind == other.TypeKind
                   && SpecialType == other.SpecialType;
        }

        // The generated ToString() was messing up the debugger (recursive calls?), so we can just null it.
        // It is not needed anyway.
        // TODO: Look into this.
        public override string ToString() => "";
        
        public string FQNGenericOmitted { get; init; }
        public string FQNGenericBased { get; init; }
        public string FQNArityBased { get; init; }
        public string FQNConstructedArgBased { get; init; }
        public string FlattenedNameArityBased { get; init; }
        public string? Namespace { get; init; } // excluding 'global' 

        public bool IsBasedOnTypeParameter { get; init; }
        public bool IsGeneric { get; init; }
        public bool IsUnboundGeneric { get; init; }
        public ITypeReferenceModel? UnboundGenericTypeRef { get; init; }
        public EquatableArray<ITypeReferenceModel> TypeArguments { get; init; }
        public EquatableArray<ITypeReferenceModel> TypeParameters { get; init; }
        public EquatableArray<ConstraintsModel> Constraints { get; init; }

        public bool IsTrueValueType { get; init; }
        public bool IsRecord { get; init; }
        public TypeKind TypeKind { get; init; }
        public SpecialType SpecialType { get; init; }

        public ITypeReferenceModel? BaseType { get; init; }
        public EquatableArray<ITypeReferenceModel> ImmediateInterfaces { get; init; }
        public EquatableArray<ITypeReferenceModel> AllInterfaces { get; init; }

        private readonly WeakReference<ITypeSymbol> _underlyingTypeSymbol;
        public ITypeSymbol UnderlyingTypeSymbol => _underlyingTypeSymbol.GetTargetOrThrow();
    }
    
    private static readonly ConditionalWeakTable<Compilation, TypeReferenceModelFactory> _factoryCache = new();

    internal static TypeReferenceModelFactory GetFactory(Compilation compilation) =>
        _factoryCache.GetValue(compilation, static _ => new TypeReferenceModelFactory());

    // Since lifetime of dictionary is tied to the factory and the factories lifetime is tied to the compilation, this shouldn't leak.
    private readonly Dictionary<ITypeSymbol, ITypeReferenceModel> _cache = new(SymbolEqualityComparer.Default);

    internal ITypeReferenceModel CreateOrGetTypeReferenceModel(ITypeSymbol typeSymbol) {
        if (_cache.TryGetValue(typeSymbol, out ITypeReferenceModel? cached)) {
            return cached;
        }
        
        ITypeReferenceModel @new = new TypeReferenceModel(typeSymbol, this);
        return @new;
    }
}