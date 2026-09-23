using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Surject.Abstractions.Attributes;
using Surject.Generators.Emitters.InjectableContainers;
using Surject.Generators.Emitters.Scopes.ContainerInner;
using Surject.Generators.Emitters.Scopes.Outer;
using Surject.Generators.Models.Concepts;
using GeneratedSource = (string name, Microsoft.CodeAnalysis.Text.SourceText sourceText);

namespace Surject.Generators;

[Generator]
internal sealed class SurjectGenerator : IIncrementalGenerator {
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    internal static class TrackingNames {
        internal const string InjectableContainers = nameof(InjectableContainers);
        internal const string ApplicationRootContainerParse = nameof(ApplicationRootContainerParse);
        internal const string SceneRootContainerParse = nameof(SceneRootContainerParse);
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
                    return new ContainerModel(in context, ContainerKind.ApplicationRoot);
                }
            )
            .Collect()
            .Select(static (all, _) => all.FirstOrDefault())
            .WithTrackingName(TrackingNames.ApplicationRootContainerParse);

        context.RegisterSourceOutput(rootContainer, static (ctx, value) => {
            if (value is null) {
                return;
            }
            
            EmitScopeOuterClass(in ctx, value);
        });
        
        IncrementalValuesProvider<ContainerModel> sceneContainers =
            context.SyntaxProvider.ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: typeof(SceneRootAttribute).FullName!,
                predicate: static (_, _) => true,
                transform: static (context, ct) => {
                    ct.ThrowIfCancellationRequested();
                    return new ContainerModel(in context, ContainerKind.SceneRoot);
                }
            )
            .WithTrackingName(TrackingNames.SceneRootContainerParse);
        
        context.RegisterSourceOutput(sceneContainers, static (ctx, value) => EmitScopeOuterClass(in ctx, value));

        IncrementalValuesProvider<ContainerModel> subContainers =
            context.SyntaxProvider.ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: typeof(SubScopeAttribute).FullName!,
                predicate: static (_, _) => true,
                transform: static (context, ct) => {
                    ct.ThrowIfCancellationRequested();
                    return new ContainerModel(in context, ContainerKind.SubScope);
                }
            )
            .WithTrackingName(TrackingNames.SubContainersParse);

        context.RegisterSourceOutput(subContainers, static (ctx, value) => EmitScopeOuterClass(in ctx, value));
        
        context.RegisterSourceOutput(rootContainer, static (ctx, value) => {
            if (value is null) {
                return;
            }
            
            EmitContainerInnerNoOpenGeneric(in ctx, value);
        });
        context.RegisterSourceOutput(sceneContainers, static (ctx, value) => EmitContainerInnerNoOpenGeneric(in ctx, value));
        context.RegisterSourceOutput(subContainers, static (ctx, value) => EmitContainerInnerNoOpenGeneric(in ctx, value));
    }

    private static void EmitScopeOuterClass(in SourceProductionContext context, ContainerModel container) {
        GeneratedSource source = ScopeOuterClassEmitter.Emit(container);
        context.AddSource(source.name, source.sourceText);
    }

    private static void EmitContainerInnerNoOpenGeneric(in SourceProductionContext context, ContainerModel container) {
        GeneratedSource source = ContainerInnerEmitter.EmitNoOpenGenerics(container);
        context.AddSource(source.name, source.sourceText);
    }
    
    private static void EmitContainerInnerNoOpenGeneric(in SourceProductionContext context, ContainerModel container, OpenGenericInjectionLinkage linkage) {
        GeneratedSource source = ContainerInnerEmitter.EmitOpenGeneric(container, linkage);
        context.AddSource(source.name, source.sourceText);
    }
}