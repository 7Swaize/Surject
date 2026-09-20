using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Surject.Generators.Models.Collections;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;

namespace Surject.Generators.Discovery.Injection;

internal static class OpenGenericInjectionLinkageBuilder {
    internal static EquatableDictionary<ITypeReferenceModel, EquatableArray<ITypeReferenceModel>> BuildInjectionLinkage(
        ImmutableArray<ContainerModel> scopeContainers,
        ImmutableArray<InjectableContainerModel> injectableContainers)
    {
        var bindingToConcretes = BuildBindingToConcretesMap(scopeContainers);
        var unboundToImpls = BuildUnboundToImplsMap(bindingToConcretes, injectableContainers);

        Dictionary<ITypeReferenceModel, EquatableArray<ITypeReferenceModel>> ret = unboundToImpls.
            ToDictionary<KeyValuePair<ITypeReferenceModel, HashSet<ITypeReferenceModel>>, ITypeReferenceModel, EquatableArray<ITypeReferenceModel>>(
                kvp => kvp.Key, 
                kvp => [.. kvp.Value]
            );

        return ret.AsEquatableDictionary();
    }

    private static Dictionary<ITypeReferenceModel, HashSet<ITypeReferenceModel>> BuildBindingToConcretesMap(
        ImmutableArray<ContainerModel> scopeContainers)
    {
        Dictionary<ITypeReferenceModel, HashSet<ITypeReferenceModel>> bindingToConcretes = new();
        RegistrationModel[] registrations = [
            .. scopeContainers.AsArrayUnsafe()!.SelectMany(
                c => c.Registrations.AsArrayUnsafe().Where(b => b.Entry.Kind is EntryKind.AddOpenGeneric)
            )
        ];

        foreach (RegistrationModel registration in registrations) {
            ITypeReferenceModel rootUnbound = registration.Entry.AuxType1.UnboundGenericTypeRef!;
            HashSet<ITypeReferenceModel> concretes = bindingToConcretes.GetOrAdd(rootUnbound, () => []);
            
            concretes.Add(rootUnbound);

            foreach (ref readonly ModifierCommandModel modifier in registration.Modifiers) {
                switch (modifier.Kind) {
                    case ModifierKind.To:
                        concretes = bindingToConcretes.GetOrAdd(modifier.TypeArg.UnboundGenericTypeRef!, () => []);
                        concretes.Add(rootUnbound);
                        break;
                    case ModifierKind.ToImmediateImplementedInterfaces:
                        foreach (ITypeReferenceModel iface in rootUnbound.ImmediateInterfaces) {
                            if (!iface.IsGeneric || iface.UnboundGenericTypeRef!.TypeParameters.Length != rootUnbound.TypeParameters.Length) {
                                continue;
                            }
                            
                            concretes = bindingToConcretes.GetOrAdd(iface.UnboundGenericTypeRef!, () => []);
                            concretes.Add(rootUnbound);
                        }
                        break;
                    case ModifierKind.ToAllImplementedInterfaces:
                        foreach (ITypeReferenceModel iface in rootUnbound.AllInterfaces) {
                            if (!iface.IsGeneric || iface.UnboundGenericTypeRef!.TypeParameters.Length != rootUnbound.TypeParameters.Length) {
                                continue;
                            }
                            
                            concretes = bindingToConcretes.GetOrAdd(iface.UnboundGenericTypeRef!, () => []);
                            concretes.Add(rootUnbound);
                        }
                        break;
                    default:
                        continue;
                }
            }
        }

        return bindingToConcretes;
    }

    private static Dictionary<ITypeReferenceModel, HashSet<ITypeReferenceModel>> BuildUnboundToImplsMap(
        Dictionary<ITypeReferenceModel, HashSet<ITypeReferenceModel>> bindingToConcretes,
        ImmutableArray<InjectableContainerModel> injectableContainers)
    {
        Dictionary<ITypeReferenceModel, HashSet<ITypeReferenceModel>> unboundToImpls = new();
        EquatableArray<InjectionTargetModel> injectionTargets = [
            .. injectableContainers.AsArrayUnsafe()!.SelectMany(c => c.InjectionTargets.AsArrayUnsafe())
        ];

        foreach (ref readonly InjectionTargetModel target in injectionTargets) {
            // Hopefully the JIT can allocate this worst-case array on the stack via escape analysis
            ReadOnlySpan<InjectionTargetModel> flattenedTargets = target.InjectionSiteKind is InjectionSiteKind.Method
                ? target.Parameters!.Value.AsSpan()
                : new[] { target };

            foreach (ref readonly InjectionTargetModel current in flattenedTargets) {
                ITypeReferenceModel injectionTargetType = current.UnwrappedTypeToRequest!;

                if (!injectionTargetType.IsGeneric) {
                    continue;
                }

                if (!bindingToConcretes.TryGetValue(injectionTargetType.UnboundGenericTypeRef!, out var concretes)) {
                    continue;
                }
                
                var requestedCache = unboundToImpls.GetOrAdd(injectionTargetType, () => []);
                requestedCache.Add(injectionTargetType);

                foreach (ITypeReferenceModel concreteUnbound in concretes) {
                    if (concreteUnbound.Equals(injectionTargetType)) {
                        continue;
                    }
                    
                    var concreteUnboundCache = unboundToImpls.GetOrAdd(concreteUnbound, () => []);
                    concreteUnboundCache.Add(
                        concreteUnbound.ConstructFromTypeArguments(injectionTargetType.TypeArguments)
                    );
                }
            }
        }

        return unboundToImpls;
    }
}