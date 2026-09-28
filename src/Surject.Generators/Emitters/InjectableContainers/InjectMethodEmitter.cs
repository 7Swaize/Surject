using System;
using System.CodeDom.Compiler;
using Surject.Abstractions.Lifecycle;
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
        
        writer.WriteLine($"public void {nameof(IInjectable.__Surject_Inject)}(global::{typeof(IResolver).FullName!} resolver) {{");
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
        writer.WriteMultiline($"this.{target.Name} = {BuildHelpers.BuildResolverCall(in target)};");
    }

    private static void EmitMethodCall(in InjectionTargetModel target, IndentedTextWriter writer) {
        writer.WriteLine($"this.{target.Name}(");
        writer.Indent++;
        
        ReadOnlySpan<InjectionTargetModel> parameters = target.Parameters!.Value.AsSpan();

        for (int i = 0; i < parameters.Length; i++) {
            ref readonly InjectionTargetModel parameter = ref parameters[i];
            
            string resolverCall = BuildHelpers.BuildResolverCall(in parameter);
            string suffix = (i == parameters.Length - 1) ? "" : ",";

            writer.WriteMultiline($"{resolverCall}{suffix}");
        }
        
        writer.Indent--;
        writer.WriteLine(");");
    }
}