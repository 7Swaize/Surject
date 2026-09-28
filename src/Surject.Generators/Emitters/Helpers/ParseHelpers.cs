using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Surject.Generators.Discovery.ServiceRegistration;
using Surject.Generators.Emitters.Helpers.Visitors;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;
using Surject.Shared.Helpers;

namespace Surject.Generators.Emitters.Helpers;

internal static class ParseHelpers {
    private const string KIAsyncDisposableFQN = "global::System.IAsyncDisposable";
    
    internal static string? GetKeyExprOrNull(RegistrationModel registration) {
        if ((registration.ModifiersDescriptor & ModifierKind.WithId) != ModifierKind.WithId) {
            return null;
        }
        
        foreach (ref readonly ModifierCommandModel modifier in registration.Modifiers) {
            if (modifier.Kind == ModifierKind.WithId) {
                return modifier.StringArg1;
            }
        }

        return ThrowHelpers.ThrowUnreachable<string>(string.Empty);
    }

    internal static (ITypeReferenceModel? Type, string? Expr) GetKeyTypeAndExprOrNull(RegistrationModel registration) {
        if ((registration.ModifiersDescriptor & ModifierKind.WithId) != ModifierKind.WithId) {
            return (null, null);
        }
        
        foreach (ref readonly ModifierCommandModel modifier in registration.Modifiers) {
            if (modifier.Kind == ModifierKind.WithId) {
                return (modifier.TypeArg, modifier.StringArg1);
            }
        }
        
        return ThrowHelpers.ThrowUnreachable<(ITypeReferenceModel?, string?)>(string.Empty);
    }
    
    internal static string? GetUnderTransformExprOrNull(RegistrationModel registration) {
        if ((registration.ModifiersDescriptor & ModifierKind.UnderTransform) != ModifierKind.UnderTransform) {
            return null;
        }
        
        foreach (ref readonly ModifierCommandModel modifier in registration.Modifiers) {
            if (modifier.Kind == ModifierKind.UnderTransform) {
                return modifier.StringArg1;
            }
        }
        
        return ThrowHelpers.ThrowUnreachable<string?>(string.Empty);
    }

    internal static ITypeReferenceModel? GetUnderObjectOfTypeOrNull(RegistrationModel registration) {
        if ((registration.ModifiersDescriptor & ModifierKind.UnderObjectOfType) != ModifierKind.UnderObjectOfType) {
            return null;
        }

        foreach (ref readonly ModifierCommandModel modifier in registration.Modifiers) {
            if (modifier.Kind == ModifierKind.UnderObjectOfType) {
                return modifier.TypeArg;
            }
        }
        
        return ThrowHelpers.ThrowUnreachable<ITypeReferenceModel?>(string.Empty);
    }

    internal static string? GetGameObjectNameExprOrNull(RegistrationModel registration) {
        if ((registration.ModifiersDescriptor & ModifierKind.WithGameObjectName) != ModifierKind.WithGameObjectName) {
            return null;
        }

        foreach (ref readonly ModifierCommandModel modifier in registration.Modifiers) {
            if (modifier.Kind == ModifierKind.WithGameObjectName) {
                return modifier.StringArg1;
            }
        }

        return ThrowHelpers.ThrowUnreachable<string?>(string.Empty);
    }

    internal static bool TryGetArgumentOverride(
        RegistrationModel registration,
        string paramName,
        [NotNullWhen(true)] out string? valueExprText)
    {
        if ((registration.ModifiersDescriptor & ModifierKind.WithArgument) != ModifierKind.WithArgument) {
            valueExprText = null;
            return false;
        }
        
        foreach (ref readonly ModifierCommandModel modifier in registration.Modifiers) {
            if (modifier.Kind == ModifierKind.WithArgument && modifier.StringArg1 == paramName) {
                valueExprText = modifier.StringArg2;
                return true;
            }
        }
        
        valueExprText = null;
        return false;
    }

    internal static bool TryGetCollectionOrder(RegistrationModel registration, [NotNullWhen(true)] out int? order) {
        if ((registration.ModifiersDescriptor & ModifierKind.AsCollection) != ModifierKind.AsCollection) {
            order = null;
            return false;
        }
        
        foreach (ref readonly ModifierCommandModel modifier in registration.Modifiers) {
            if (modifier.Kind != ModifierKind.AsCollection) {
                continue;
            }
            
            order = modifier.IntArg;
            return true;
        }
        
        order = null;
        return false;
    }

    internal static void GetAllContractsOf(RegistrationModel registration, ITypeReferenceModel type, List<ITypeReferenceModel> outBuffer) {
        ContractCollectorVisitor contractCollectorVisitor = new ContractCollectorVisitor(type, outBuffer);

        foreach (ref readonly ModifierCommandModel modifier in registration.Modifiers) {
            modifier.Accept<ContractCollectorVisitor, VoidVisitor>(ref contractCollectorVisitor);
        }
    }

    internal static bool ShouldTrackTransientDisposal(ContainerModel container) {
        foreach (RegistrationModel registration in container.Registrations) {
            if (registration.Entry.Lifetime != LifetimeKind.Transient) {
                continue;
            }
            
            foreach (ref readonly ModifierCommandModel modifier in registration.Modifiers) {
                if (modifier.Kind != ModifierKind.TrackDisposable) {
                    continue;
                }

                return true;
            }
        }

        return false;
    }

    internal static bool InheritsFromIDisposable(ITypeReferenceModel type) 
        => type.AllInterfaces.AsArrayUnsafe().Any(iface => iface.SpecialType == SpecialType.System_IDisposable);

    internal static bool InheritsFromIAsyncDisposable(ITypeReferenceModel type)
        => type.AllInterfaces.AsArrayUnsafe().Any(iface => iface.FQNGenericOmitted.Equals(KIAsyncDisposableFQN));
}