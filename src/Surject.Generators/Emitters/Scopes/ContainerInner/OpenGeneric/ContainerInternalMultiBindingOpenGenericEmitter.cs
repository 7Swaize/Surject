using System.CodeDom.Compiler;
using System.Collections.Generic;
using Surject.Generators.Emitters.Helpers;
using Surject.Generators.Emitters.Helpers.Visitors;
using Surject.Generators.Models.Collections;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;

namespace Surject.Generators.Emitters.Scopes.ContainerInner.OpenGeneric;

internal readonly ref struct ContainerInternalMultiBindingOpenGenericEmitter : IChainedEmitter {
    private readonly ContainerModel _container;
    private readonly OpenGenericInjectionLinkage _linkage;

    internal ContainerInternalMultiBindingOpenGenericEmitter(ContainerModel container, OpenGenericInjectionLinkage linkage) {
        _container = container;
        _linkage = linkage;
    }

    public void Emit(IndentedTextWriter writer) {
        Dictionary<(ITypeReferenceModel Contract, string? Key), bool> aggregateBySet = new();
        List<(ITypeReferenceModel Contract, string? key)> setOrder = [];
        List<ITypeReferenceModel> contractsBuffer = new(4);
        EntryRegistrationTypeVisitor registrationVisitor = new();

        foreach (RegistrationModel registration in _container.Registrations) {
            if (registration.Entry.Kind != EntryKind.AddOpenGeneric) {
                continue;
            }
            
            if ((registration.ModifiersDescriptor & ModifierKind.AsCollection) != ModifierKind.AsCollection) {
                continue;
            }
            
            ITypeReferenceModel entryType = registration.Entry.Accept<EntryRegistrationTypeVisitor, ITypeReferenceModel?>(ref registrationVisitor)!;
            
            contractsBuffer.Clear();
            contractsBuffer.Add(entryType);
            ParseHelpers.GetAllContractsOf(registration, entryType, contractsBuffer);
            
            string? key = ParseHelpers.GetKeyExprOrNull(registration);
            bool isSingleton = registration.Entry.Lifetime == LifetimeKind.Singleton;

            foreach (ITypeReferenceModel unboundContract in contractsBuffer) {
                if (!unboundContract.IsUnboundGeneric) {
                    RegisterSetMember(unboundContract, key, isSingleton);
                    continue;
                }
                
                if (!_linkage.Linkage.TryGetValue(unboundContract, out EquatableArray<ITypeReferenceModel> closedContracts)) {
                    continue;
                }
                
                foreach (ITypeReferenceModel closedContract in closedContracts) {
                    RegisterSetMember(closedContract, key, isSingleton);
                }
            }
        }

        foreach ((ITypeReferenceModel contract, string? key) in setOrder) {
            if (!aggregateBySet[(contract, key)]) {
                continue;
            }
            
            string fieldName = key is null
                ? BuildHelpers.BuildMultiBindLocalArrayFieldNotKeyed(contract)
                : BuildHelpers.BuildMultiBindLocalArrayFieldKeyed(contract, key);
            
            writer.WriteLine($"internal (int Order, {contract.FQNConstructedArgBased} Instance)[]? {fieldName};");
        }

        return;

        void RegisterSetMember(ITypeReferenceModel closedContract, string? key, bool isSingleton) {
            (ITypeReferenceModel, string?) setKey = (closedContract, key);
            bool isNewSet = !aggregateBySet.TryGetValue(setKey, out bool valid);

            if (isNewSet) {
                setOrder.Add(setKey);
            }
            
            aggregateBySet[setKey] = isNewSet ? isSingleton : valid && isSingleton;
        }
    }
}