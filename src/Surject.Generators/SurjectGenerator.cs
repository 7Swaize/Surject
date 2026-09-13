using System.Diagnostics.CodeAnalysis;
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
    }
    
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        IncrementalValuesProvider<InjectableContainerModel> injectableContainers =
            context.SyntaxProvider.ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: typeof(InjectableAttribute).FullName!,
                predicate: static (_, _) => true,
                transform: static (context, cts) => {
                    cts.ThrowIfCancellationRequested();
                    return new InjectableContainerModel(in context);
                }
            )
            .WithTrackingName(TrackingNames.InjectableContainers);
        
        context.RegisterSourceOutput(injectableContainers, static (ctx, value) => {
            GeneratedSource source = InjectableContainerEmitter.Emit(value);
            
            ctx.AddSource(source.name, source.sourceText);
        });
    }
}