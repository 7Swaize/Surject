using Microsoft.CodeAnalysis;
using Surject.Generators.Models;
using Surject.Shared;

namespace Surject.Generators;

[Generator]
internal sealed class SurjectGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
#if DEBUG
        System.Diagnostics.Debugger.Launch();
#endif

        IncrementalValuesProvider<ContainerModel> containers = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: SymbolConstants.ScopeProviderAttributeFQN,
            predicate: static (_, _) => true,
            transform: static (context, _) => {
                
            }
        );
    }
}

