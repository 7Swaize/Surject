using System.Collections.Immutable;
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
        internal const string OpenGenericLinkage = nameof(OpenGenericLinkage);
        internal const string ApplicationRootLinkageCombine = nameof(ApplicationRootLinkageCombine);
        internal const string SceneRootLinkageCombine = nameof(SceneRootLinkageCombine);
        internal const string SubContainersLinkageCombine = nameof(SubContainersLinkageCombine);
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
            ctx.CancellationToken.ThrowIfCancellationRequested();
            
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
        
        context.RegisterSourceOutput(rootContainer, static (ctx, value) => {
            if (value is null) {
                return;
            }
            
            EmitScopeOuterClass(in ctx, value);
        });
        context.RegisterSourceOutput(sceneContainers, static (ctx, value) => EmitScopeOuterClass(in ctx, value));
        context.RegisterSourceOutput(subContainers, static (ctx, value) => EmitScopeOuterClass(in ctx, value));
        
        context.RegisterSourceOutput(rootContainer, static (ctx, value) => {
            if (value is null) {
                return;
            }
            
            EmitContainerInnerNoOpenGeneric(in ctx, value);
        });
        context.RegisterSourceOutput(sceneContainers, static (ctx, value) => EmitContainerInnerNoOpenGeneric(in ctx, value));
        context.RegisterSourceOutput(subContainers, static (ctx, value) => EmitContainerInnerNoOpenGeneric(in ctx, value));

        IncrementalValueProvider<OpenGenericInjectionLinkage> openGenericLinkage = sceneContainers
            .Collect()
            .Combine(subContainers.Collect())
            .Combine(rootContainer)
            .Select(static (data, ct) => {
                ct.ThrowIfCancellationRequested();

                var ((scenes, subs), root) = data;
                var containerBuilder = ImmutableArray.CreateBuilder<ContainerModel>(
                    scenes.Length + subs.Length + (root is null ? 0 : 1)
                );

                if (root is not null) {
                    containerBuilder.Add(root);
                }

                containerBuilder.AddRange(scenes);
                containerBuilder.AddRange(subs);

                return containerBuilder.MoveToImmutable();
            })
            .Combine(injectableContainers.Collect())
            .Select(static (data, ct) => {
                ct.ThrowIfCancellationRequested();

                return new OpenGenericInjectionLinkage(data.Left, data.Right);
            })
            .WithTrackingName(TrackingNames.OpenGenericLinkage);

        IncrementalValueProvider<(ContainerModel?, OpenGenericInjectionLinkage)> rootPlusLinkage = rootContainer
            .Combine(openGenericLinkage)
            .WithTrackingName(TrackingNames.ApplicationRootLinkageCombine);
        
        IncrementalValuesProvider<(ContainerModel, OpenGenericInjectionLinkage)> sceneContainersPlusLinkage = sceneContainers
            .Where(container => (container.EntriesDescriptor & EntryKind.AddOpenGeneric) == EntryKind.AddOpenGeneric)
            .Combine(openGenericLinkage)
            .WithTrackingName(TrackingNames.SceneRootLinkageCombine);
        
        IncrementalValuesProvider<(ContainerModel, OpenGenericInjectionLinkage)> subScopeContainersPlusLinkage = subContainers
            .Where(container => (container.EntriesDescriptor & EntryKind.AddOpenGeneric) == EntryKind.AddOpenGeneric)
            .Combine(openGenericLinkage)
            .WithTrackingName(TrackingNames.SubContainersLinkageCombine);

        context.RegisterSourceOutput(rootPlusLinkage, static (ctx, value) => {
            if (value.Item1 is null) {
                return;
            }

            EmitContainerInnerOpenGeneric(in ctx, value.Item1, value.Item2);
        });
        context.RegisterSourceOutput(sceneContainersPlusLinkage, static (ctx, value) => EmitContainerInnerOpenGeneric(in ctx, value.Item1, value.Item2));
        context.RegisterSourceOutput(subScopeContainersPlusLinkage, static (ctx, value) => EmitContainerInnerOpenGeneric(in ctx, value.Item1, value.Item2));
    }

    private static void EmitScopeOuterClass(in SourceProductionContext context, ContainerModel container) {
        context.CancellationToken.ThrowIfCancellationRequested();
        
        GeneratedSource source = ScopeOuterClassEmitter.Emit(container);
        context.AddSource(source.name, source.sourceText);
    }

    private static void EmitContainerInnerNoOpenGeneric(in SourceProductionContext context, ContainerModel container) {
        context.CancellationToken.ThrowIfCancellationRequested();
        
        GeneratedSource source = ContainerInnerEmitter.EmitNoOpenGenerics(container);
        context.AddSource(source.name, source.sourceText);
    }
    
    private static void EmitContainerInnerOpenGeneric(in SourceProductionContext context, ContainerModel container, OpenGenericInjectionLinkage linkage) {
        context.CancellationToken.ThrowIfCancellationRequested();
        
        GeneratedSource source = ContainerInnerEmitter.EmitOpenGeneric(container, linkage);
        context.AddSource(source.name, source.sourceText);
    }
}