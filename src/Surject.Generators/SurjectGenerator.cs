using System.ComponentModel;
using Microsoft.CodeAnalysis;
using Surject.Abstractions.Attributes;
using Surject.Generators.Discovery;
using Surject.Generators.Emitters.InjectableTargets;
using Surject.Generators.Emitters.Scope;
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
            
            // ctx.AddSource($"{source.name}", source.sourceText);
            ctx.AddSource($"{source.name}", $"/*\n {source.sourceText} \n*/");
        });
        

        IncrementalValuesProvider<ContainerModel> containers = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: typeof(ScopeAttribute).FullName!,
            predicate: static (_, _) => true,
            transform: static (context, _) => {
                DiscoveryUtils utils = context.SemanticModel.Compilation.GetDiscoveryUtils();
                return new ContainerModel(in context, utils);
            }
        );

        context.RegisterSourceOutput(containers, (ctx, container) => {
            GeneratedSource source = ScopeEmitter.Emit(container);

            // ctx.AddSource($"{source.name}", source.sourceText);
            ctx.AddSource($"{source.name}", $"/*\n {source.sourceText} \n*/");
        });
        

        IncrementalValuesProvider<(ContainerModel Left, InjectionLinkage Right)> fullLinkage = containers.Combine(
            injectableTargets.Collect().Select(
                static (collection, _) => new InjectionLinkage(collection)
            )
        );
        
        context.RegisterSourceOutput(fullLinkage, (ctx, source) => {
            // TODO
        });
    }
}

