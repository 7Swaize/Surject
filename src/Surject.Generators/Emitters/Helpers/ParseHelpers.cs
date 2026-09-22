using System.Collections.Generic;
using Surject.Generators.Discovery.ServiceRegistration;
using Surject.Generators.Emitters.Helpers.Visitors;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;
using Surject.Shared.Helpers;

namespace Surject.Generators.Emitters.Helpers;

internal static class ParseHelpers {
    internal static string? GetKeyExprOrNull(RegistrationModel registration) {
        if ((registration.ModifiersDescriptor & ModifierKind.WithId) == 0) {
            return null;
        }
        
        foreach (ref readonly ModifierCommandModel modifier in registration.Modifiers) {
            if (modifier.Kind == ModifierKind.WithId) {
                return modifier.StringArg1;
            }
        }

        return ThrowHelpers.ThrowUnreachable<string>(string.Empty);
    }

    internal static void GetAllContractsOf(RegistrationModel registration, ITypeReferenceModel type, List<ITypeReferenceModel> outBuffer) {
        ContractCollectorVisitor contractCollectorVisitor = new ContractCollectorVisitor(type, outBuffer);

        foreach (ref readonly ModifierCommandModel modifier in registration.Modifiers) {
            modifier.Accept<ContractCollectorVisitor, VoidVisitor>(ref contractCollectorVisitor);
        }
    }
}