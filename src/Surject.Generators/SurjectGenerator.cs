using Microsoft.CodeAnalysis;
using Surject.Abstractions.Attributes;
using Surject.Generators.Discovery;
using Surject.Generators.Emitters.InjectableTargets;
using Surject.Generators.Models.Concepts;
using GeneratedSource = (string name, Microsoft.CodeAnalysis.Text.SourceText sourceText);

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
        
        context.RegisterSourceOutput(injectableTargets, (ctx, injectableTarget) => {
            GeneratedSource source = InjectableTargetEmitter.Emit(injectableTarget);
            
            ctx.AddSource($"{source.name}", source.sourceText);
        });

        IncrementalValuesProvider<ContainerModel> containers = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: typeof(ScopeAttribute).FullName!,
            predicate: static (_, _) => true,
            transform: static (context, _) => {
                DiscoveryUtils utils = context.SemanticModel.Compilation.GetDiscoveryUtils();
                return new ContainerModel(in context, utils);
            }
        );
        
        // Ideally, we will emit EVERYTHING that can be constructed via an individual `ContainerModel` first.
        // Then, we will merge with `InjectableTargetModel` for emitting Container Resolver. This has to be done.
        // But, with this we get the most benefit from the incremental nature.
        // Mapping here will be 1 Container : N InjectableTargets
    }
}

