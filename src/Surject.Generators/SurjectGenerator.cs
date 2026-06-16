using Microsoft.CodeAnalysis;
using Surject.Abstractions.Attributes;
using Surject.Generators.Discovery;
using Surject.Generators.Models.Concepts;

namespace Surject.Generators;

[Generator]
internal sealed class SurjectGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
#if DEBUG
        System.Diagnostics.Debugger.Launch();
#endif
        
        IncrementalValuesProvider<InjectableTargetModel> injectableTargets = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: typeof(InjectableAttribute).FullName!,
            predicate: static (_, _) => true,
            transform: static (context, _) => {
                DiscoveryUtils utils = context.SemanticModel.Compilation.GetDiscoveryUtils();
                return new InjectableTargetModel(in context, utils);
            }
        );
        
        // emit

        IncrementalValuesProvider<ContainerModel> containers = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: typeof(ScopeAttribute).FullName!,
            predicate: static (_, _) => true,
            transform: static (context, _) => {
                DiscoveryUtils utils = context.SemanticModel.Compilation.GetDiscoveryUtils();
                return new ContainerModel(in context, utils);
            }
        );
        
        // emit
    }
}

