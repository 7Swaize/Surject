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
        ImmutableArray<ContainerModel> containers,
        ImmutableArray<InjectableContainerModel> injectableContainers)
    {
        var concreteToBindings = BuildConcreteToBindingsMap(containers);
        var bindingToConcretes = BuildBindingToConcretesMap(concreteToBindings);
    }

    private static Dictionary<ITypeReferenceModel, HashSet<ITypeReferenceModel>> BuildConcreteToBindingsMap(
        ImmutableArray<ContainerModel> containers) 
    {
        Dictionary<ITypeReferenceModel, HashSet<ITypeReferenceModel>> concreteToBindings = new();
        RegistrationModel[] registrations = [
            .. containers.SelectMany(c => c.Bindings.Where(b => b.Entry.Kind is EntryKind.AddOpenGeneric))
        ];
        
        foreach (RegistrationModel registration in registrations) {
            ITypeReferenceModel root = registration.Entry.AuxType1.UnboundGenericTypeRef!;
            
            if (!concreteToBindings.TryGetValue(root, out HashSet<ITypeReferenceModel>? bindings)) {
                bindings = new();
                concreteToBindings.Add(root, bindings);
            }
            
            bindings.Add(root);

            foreach (ref readonly ModifierCommandModel modifier in registration.Modifiers) {
                switch (modifier.Kind) {
                    case ModifierKind.To:
                        bindings.Add(modifier.TypeArg.UnboundGenericTypeRef ?? modifier.TypeArg);
                        break;
                    case ModifierKind.ToImmediateImplementedInterfaces: {
                        foreach (ITypeReferenceModel iface in root.ImmediateInterfaces) {
                            bindings.Add(iface.UnboundGenericTypeRef ?? iface);
                        }
                        break;
                    }
                    case ModifierKind.ToAllImplementedInterfaces: {
                        foreach (ITypeReferenceModel iface in root.AllInterfaces) {
                            bindings.Add(iface.UnboundGenericTypeRef ?? iface);
                        }
                        break;
                    }
                    default:
                        continue;
                }
            }
        }
        
        return concreteToBindings;
    }

    private static Dictionary<ITypeReferenceModel, List<ITypeReferenceModel>> BuildBindingToConcretesMap(
        Dictionary<ITypeReferenceModel, HashSet<ITypeReferenceModel>> concreteToBindings)
    {
        Dictionary<ITypeReferenceModel, List<ITypeReferenceModel>> bindingToConcretes = new();
        
        foreach (KeyValuePair<ITypeReferenceModel, HashSet<ITypeReferenceModel>> kvp in concreteToBindings) {
            ITypeReferenceModel concrete = kvp.Key;
            HashSet<ITypeReferenceModel> bindings = kvp.Value;
            
            foreach (ITypeReferenceModel binding in bindings) {
                if (!bindingToConcretes.TryGetValue(binding, out List<ITypeReferenceModel>? concretes)) {
                    concretes = new();
                    bindingToConcretes.Add(binding, concretes);
                }
                
                concretes.Add(concrete);
            }
        }
        
        return bindingToConcretes;
    }

    private static Dictionary<ITypeReferenceModel, List<ITypeReferenceModel>> BuildUnboundToImplsMap(
        Dictionary<ITypeReferenceModel, HashSet<ITypeReferenceModel>> concreteToBindings,
        Dictionary<ITypeReferenceModel, List<ITypeReferenceModel>> bindingToConcretes,
        ImmutableArray<InjectableContainerModel> injectableContainers)
    {
        Dictionary<ITypeReferenceModel, List<ITypeReferenceModel>> unboundToImpls = new();
        EquatableArray<InjectionTargetModel> injectionTargets = [
            .. injectableContainers.SelectMany(c => c.InjectionTargets.AsArrayUnsafe())
        ];

        foreach (ref readonly InjectionTargetModel target in injectionTargets) {
            ReadOnlySpan<InjectionTargetModel> flattenedTargets = target.InjectionSiteKind is InjectionSiteKind.Method
                ? target.Parameters!.Value.AsSpan()
                : new[] { target };

            foreach (ref readonly InjectionTargetModel current in flattenedTargets) {
                if (!current.UnwrappedTypeToRequest!.IsGeneric) {
                    continue;
                }
                
                ITypeReferenceModel request = current.UnwrappedTypeToRequest.UnboundGenericTypeRef!;
            }
        }
    }
}