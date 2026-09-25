using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Threading.Tasks;
using Surject.Generators.Emitters.Helpers;
using Surject.Generators.Emitters.Helpers.Visitors;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;

namespace Surject.Generators.Emitters.Scopes.ContainerInner.NoOpenGeneric;

internal readonly ref struct ContainerInternalMultiBindingNoOpenGenericEmitter : IChainedEmitter {
    private struct MultiBindSetAggregate {
        internal bool SyncAllSingleton;
        internal bool HasSync;
        internal bool AsyncAllSingleton;
        internal bool HasAsync;
    }
    
    private readonly ContainerModel _model;
    
    internal ContainerInternalMultiBindingNoOpenGenericEmitter(ContainerModel model) => _model = model;
    
    public void Emit(IndentedTextWriter writer) {
        Dictionary<(ITypeReferenceModel Contract, string? Key), MultiBindSetAggregate> aggregateBySet = new();
        List<(ITypeReferenceModel Contract, string? Key)> setOrder = [];
        List<ITypeReferenceModel> contractsBuffer = new(4);
        
        EntryRegistrationTypeVisitor registrationVisitor = new();
        
        foreach (RegistrationModel registration in _model.Registrations) {
            if (!IsEligibleForOrderedMultiBind(registration.Entry.Kind)) {
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
            bool isAsync = registration.Entry.Kind == EntryKind.AddAsyncFactory;

            foreach (ITypeReferenceModel contract in contractsBuffer) {
                (ITypeReferenceModel, string?) setKey = (contract, key);
                bool isNewSet = !aggregateBySet.TryGetValue(setKey, out MultiBindSetAggregate aggregate);
                
                if (isNewSet) {
                    setOrder.Add(setKey);
                }

                if (isAsync) {
                    aggregate.AsyncAllSingleton = aggregate.HasAsync ? aggregate.AsyncAllSingleton && isSingleton : isSingleton;
                    aggregate.HasAsync = true;
                }
                else {
                    aggregate.SyncAllSingleton = aggregate.HasSync ? aggregate.SyncAllSingleton && isSingleton : isSingleton;
                    aggregate.HasSync = true;
                }
                
                aggregateBySet[setKey] = aggregate;
            }
        }

        foreach ((ITypeReferenceModel contract, string? key) in setOrder) {
            MultiBindSetAggregate aggregate = aggregateBySet[(contract, key)];
            
            if (aggregate is { HasSync: true, SyncAllSingleton: true }) {
                string syncFieldName = key is null
                    ? BuildHelpers.BuildMultiBindLocalArrayFieldNotKeyed(contract)
                    : BuildHelpers.BuildMultiBindLocalArrayFieldKeyed(contract, key);

                writer.WriteLine($"internal (int Order, {contract.FQNConstructedArgBased} Instance)[]? {syncFieldName};");
            }

            if (aggregate is { HasAsync: true, AsyncAllSingleton: true }) {
                string asyncArrayFieldName = key is null
                    ? BuildHelpers.BuildMultiBindLocalAsyncArrayFieldNotKeyed(contract)
                    : BuildHelpers.BuildMultiBindLocalAsyncArrayFieldKeyed(contract, key);

                string asyncTaskFieldName = key is null
                    ? BuildHelpers.BuildMultiBindLocalArrayTaskFieldNotKeyed(contract)
                    : BuildHelpers.BuildMultiBindLocalArrayTaskFieldKeyed(contract, key);

                writer.WriteLine($"internal (int Order, {contract.FQNConstructedArgBased} Instance)[]? {asyncArrayFieldName};");
                writer.WriteLine(
                    $"internal global::{typeof(Task).FullName}<(int Order, {contract.FQNConstructedArgBased} Instance)[]>? {asyncTaskFieldName};"
                );
            }
        }
    }
    
    private static bool IsEligibleForOrderedMultiBind(EntryKind kind) => kind switch {
        EntryKind.AddOpenGeneric => false,
        EntryKind.AddAmbient => false,
        _ => true
    };
}