using System.CodeDom.Compiler;
using System.Collections.Generic;
using Surject.Generators.Emitters.Helpers;
using Surject.Generators.Emitters.Helpers.Visitors;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;

namespace Surject.Generators.Emitters.Scopes.ContainerInner.NoOpenGeneric;

internal readonly ref struct ContainerInternalDiscoveryCollectionNoOpenGenericEmitter : IChainedEmitter {
    private readonly ContainerModel _model;
    
    internal ContainerInternalDiscoveryCollectionNoOpenGenericEmitter(ContainerModel model) => _model = model;

    public void Emit(IndentedTextWriter writer) {
        HashSet<(ITypeReferenceModel Contract, string? Key)> emitted = new();
        List<ITypeReferenceModel> contractsBuffer = new(4);
        
        EntryRegistrationTypeVisitor registrationVisitor = new();

        foreach (RegistrationModel registration in _model.Registrations) {
            if (!IsDiscoveredCollectionEntry(registration.Entry.Kind)) {
                continue;
            }
            
            ITypeReferenceModel entryType = registration.Entry.Accept<EntryRegistrationTypeVisitor, ITypeReferenceModel?>(ref registrationVisitor)!;

            contractsBuffer.Clear();
            contractsBuffer.Add(entryType);
            ParseHelpers.GetAllContractsOf(registration, entryType, contractsBuffer);
            
            string? key = ParseHelpers.GetKeyExprOrNull(registration);

            foreach (ITypeReferenceModel contract in contractsBuffer) {
                if (!emitted.Add((contract, key))) {
                    continue;
                }
                
                string fieldName = key is null
                    ? BuildHelpers.BuildFoundArrayFieldNotKeyed(contract)
                    : BuildHelpers.BuildFoundArrayFieldKeyed(contract, key);

                writer.WriteLine($"internal {contract.FQNConstructedArgBased}[]? {fieldName};");
            }
        }
    }
    
    private static bool IsDiscoveredCollectionEntry(EntryKind kind) => kind switch {
        EntryKind.AddAllFromChildren => true,
        EntryKind.AddAllFromHierarchy => true,
        EntryKind.AddAllFromParent => true,
        _ => false
    };

}