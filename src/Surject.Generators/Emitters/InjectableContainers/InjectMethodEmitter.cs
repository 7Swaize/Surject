using System;
using System.CodeDom.Compiler;
using Surject.Abstractions.Resolutions;
using Surject.Generators.Models.Concepts;
using Surject.Shared.Helpers;

namespace Surject.Generators.Emitters.InjectableContainers;

internal readonly struct InjectMethodEmitter : IChainedEmitter {
    private readonly InjectableContainerModel _container;

    internal InjectMethodEmitter(InjectableContainerModel container) {
        _container = container;
    }
    
    public void Emit(IndentedTextWriter writer) {
        EmitHelpers.EmitEditorBrowsableNeverAttribute(writer);
        
        writer.WriteLine($"public void __Surject_Inject(global::{typeof(IResolver).FullName!} resolver) {{");
        writer.Indent++;

        foreach (ref readonly InjectionTargetModel target in _container.InjectionTargets) {
            switch (target.InjectionSiteKind) {
                case InjectionSiteKind.Field or InjectionSiteKind.Property:
                    EmitStandardInit(in target, writer);
                    break;
                case InjectionSiteKind.Method:
                    EmitMethodCall(in target, writer);
                    break;
                default:
                    ThrowHelpers.ThrowUnhandledBranch(target.InjectionSiteKind);
                    break;
            }
        }
    }

    private static void EmitStandardInit(in InjectionTargetModel target, IndentedTextWriter writer) {
        writer.WriteLine($"this.{target.Name} = {BuildResolverCall(in target)};");
    }

    private static void EmitMethodCall(in InjectionTargetModel target, IndentedTextWriter writer) {
        writer.WriteLine($"this.{target.Name}(");
        writer.Indent++;
        
        ReadOnlySpan<InjectionTargetModel> parameters = target.Parameters!.Value.AsSpan();

        for (int i = 0; i < parameters.Length; i++) {
            ref readonly InjectionTargetModel parameter = ref parameters[i];
            
            string resolverCall = BuildResolverCall(in parameter);
            string suffix = (i == parameters.Length - 1) ? "" : ",";

            writer.WriteLine($"{resolverCall}{suffix}");
        }
        
        writer.Indent--;
        writer.WriteLine(");");
    }

    private static string BuildResolverCall(in InjectionTargetModel target) {
        (string keyTypeToPass, string contextConstruction) contextCreation = BuildResolveContextConstruction(in target);
        InjectionDeferralKind deferralKind = target.InjectionDeferralKind;
        string method;

        if ((deferralKind & InjectionDeferralKind.Async) == InjectionDeferralKind.Async) {
            if ((deferralKind & InjectionDeferralKind.All) == InjectionDeferralKind.All) {
                method = nameof(IAsyncResolver.ResolveAllAsync);
            }
            else if ((deferralKind & InjectionDeferralKind.Optional) == InjectionDeferralKind.Optional) {
                method = nameof(IAsyncResolver.ResolveOptionalAsync);
            }
            else {
                method = nameof(IAsyncResolver.ResolveAsync);
            }
        }
        else if ((deferralKind & InjectionDeferralKind.Optional) == InjectionDeferralKind.Optional) {
            method = nameof(IResolver.ResolveOptional);
        }
        else {
            method = nameof(IResolver.Resolve);
        }
        
        if ((deferralKind & InjectionDeferralKind.Keyed) == InjectionDeferralKind.Keyed) {
            return
                $"resolver.{method}<{target.UnwrappedTypeToRequest!.FQNConstructedArgBased}, {target.IdType!.FQNConstructedArgBased}>("
                + $"new global::Surject.Abstractions.Resolutions.ResolveContext<{target.IdType!.FQNConstructedArgBased}> {{ Key = {target.IdAsText} }})";
        }

        return
            $"resolver.{method}<{target.UnwrappedTypeToRequest!.FQNConstructedArgBased}, global::Surject.Abstractions.Resolutions::NoneKey>("
            + $"new global::Surject.Abstractions.Resolutions.ResolveContext<global::Surject.Abstractions.Resolutions::NoneKey> {{ }})";
    }

    private static (string keyTypeToPass, string contextConstruction) BuildResolveContextConstruction(in InjectionTargetModel target) {
        if (target.IdType is null) {
            return ("global::Surject.Abstractions.Resolutions.NoneKey", string.Empty);
        }

        return (target.IdType.FQNConstructedArgBased!, target.IdAsText!);
    }
}