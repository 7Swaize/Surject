using System;
using System.CodeDom.Compiler;
using Surject.Abstractions.Resolutions;
using Surject.Generators.Emitters.Helpers;
using Surject.Generators.Models.Concepts;
using Surject.Shared.Helpers;

namespace Surject.Generators.Emitters.InjectableContainers;

internal readonly ref struct InjectMethodEmitter : IChainedEmitter {
    private readonly InjectableContainerModel _container;

    internal InjectMethodEmitter(InjectableContainerModel container) => _container = container;

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
        
        writer.Indent--;
        writer.WriteLine("}");
    }

    private static void EmitStandardInit(in InjectionTargetModel target, IndentedTextWriter writer) {
        writer.WriteMultiline($"this.{target.Name} = {BuildResolverCall(in target)};");
    }

    private static void EmitMethodCall(in InjectionTargetModel target, IndentedTextWriter writer) {
        writer.WriteLine($"this.{target.Name}(");
        writer.Indent++;
        
        ReadOnlySpan<InjectionTargetModel> parameters = target.Parameters!.Value.AsSpan();

        for (int i = 0; i < parameters.Length; i++) {
            ref readonly InjectionTargetModel parameter = ref parameters[i];
            
            string resolverCall = BuildResolverCall(in parameter);
            string suffix = (i == parameters.Length - 1) ? "" : ",";

            writer.WriteMultiline($"{resolverCall}{suffix}");
        }
        
        writer.Indent--;
        writer.WriteLine(");");
    }

    private static string BuildResolverCall(in InjectionTargetModel target) {
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
        else if ((deferralKind & InjectionDeferralKind.All) == InjectionDeferralKind.All) {
            method = nameof(IResolver.ResolveAll);
        } 
        else {
            method = nameof(IResolver.Resolve);
        }
        
        if ((deferralKind & InjectionDeferralKind.Keyed) == InjectionDeferralKind.Keyed) {
            return
                $$"""
                  resolver.{{method}}<{{target.UnwrappedTypeToRequest!.FQNConstructedArgBased}}, {{target.IdType!.FQNConstructedArgBased}}>(
                      new global::Surject.Abstractions.Resolutions.ResolveContext<{{target.IdType!.FQNConstructedArgBased}}> { Key = {{target.IdAsText}} }
                  )
                  """;
        }
        
        return
            $$"""
              resolver.{{method}}<{{target.UnwrappedTypeToRequest!.FQNConstructedArgBased}}, global::Surject.Abstractions.Resolutions.NoneKey>(
                  new global::Surject.Abstractions.Resolutions.ResolveContext<global::Surject.Abstractions.Resolutions.NoneKey> { }
              )
              """;
    }
}