using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Surject.Abstractions.Attributes;
using Surject.Generators.Emitters.InjectableContainers;
using Surject.Generators.Models.Concepts;
using GeneratedSource = (string name, Microsoft.CodeAnalysis.Text.SourceText sourceText);

namespace Surject.Generators;

[Generator]
internal sealed class SurjectGenerator : IIncrementalGenerator {
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    internal static class TrackingNames {
        internal const string InjectableContainers = nameof(InjectableContainers);
        internal const string RootContainerParse = nameof(RootContainerParse);
        internal const string SubContainersParse = nameof(SubContainersParse);
    }
    
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        IncrementalValuesProvider<InjectableContainerModel> injectableContainers =
            context.SyntaxProvider.ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: typeof(InjectableAttribute).FullName!,
                predicate: static (_, _) => true,
                transform: static (context, ct) => {
                    ct.ThrowIfCancellationRequested();
                    return new InjectableContainerModel(in context);
                }
            )
            .WithTrackingName(TrackingNames.InjectableContainers);
        
        context.RegisterSourceOutput(injectableContainers, static (ctx, value) => {
            GeneratedSource source = InjectableContainerEmitter.Emit(value);
            
            ctx.AddSource(source.name, source.sourceText);
        });
        
        IncrementalValueProvider<ContainerModel?> rootContainer = 
            context.SyntaxProvider.ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: typeof(ApplicationRootAttribute).FullName!,
                predicate: static (_, _) => true,
                transform: static (context, ct) => {
                    ct.ThrowIfCancellationRequested();
                    return new ContainerModel(in context, isRoot: true);
                }
            )
            .Collect()
            .Select(static (all, _) => all.FirstOrDefault())
            .WithTrackingName(TrackingNames.RootContainerParse);

        context.RegisterSourceOutput(rootContainer, static (ctx, value) => {
            if (value is null) return;
        });

        IncrementalValuesProvider<ContainerModel> subContainers =
            context.SyntaxProvider.ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: typeof(ScopeAttribute).FullName!,
                predicate: static (_, _) => true,
                transform: static (context, ct) => {
                    ct.ThrowIfCancellationRequested();
                    return new ContainerModel(in context, isRoot: false);
                }
            )
            .WithTrackingName(TrackingNames.SubContainersParse);
    }
}