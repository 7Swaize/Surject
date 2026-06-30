using System.CodeDom.Compiler;
using Surject.Abstractions.Resolutions;
using Surject.Generators.Models.Collections;
using Surject.Generators.Models.Concepts;
using Surject.Shared.Helpers;
using static Surject.Generators.Emitters.EmitConstants;

namespace Surject.Generators.Emitters.InjectableTargets;

internal readonly struct InjectMethodEmitter {
    private readonly InjectableTargetModel _model;
    
    internal InjectMethodEmitter(InjectableTargetModel model) {
        _model = model;
    }

    internal void Emit(IndentedTextWriter writer) {
        EmitHelpers.EmitPreserveAttribute(writer);
        EmitHelpers.EmitEditorBrowsableNeverAttribute(writer);
        
        writer.WriteLine($"public void __Surject_Inject(global::{typeof(IResolver).FullName} __resolver) {{");
        writer.Indent++;

        foreach (InjectableMemberModel member in _model.MembersToInject) {
            switch (member.Site) {
                case InjectionSiteKind.Method:
                    ResolveMethod(in member, writer);
                    break;
                case InjectionSiteKind.Field or InjectionSiteKind.Property:
                    ResolveStandard(in member, writer);
                    break;
                default:
                    ThrowHelpers.ThrowUnhandledBranch(member.Site);
                    break;
            }
        }
        
        writer.Indent--;
        writer.WriteLine("}");
    }

    private static void ResolveStandard(in InjectableMemberModel member, IndentedTextWriter writer) {
        (string method, string ctxExpr) = BuildResolverCall(member);
        
        writer.WriteLine(
            $"this.{member.Name} = __resolver.{method}<{member.TypeToRequest}>({ctxExpr});"
        );
        
    }

    private static void ResolveMethod(in InjectableMemberModel member, IndentedTextWriter writer) {
        writer.WriteLine($"this.{member.Name}(");
        writer.Indent++;
        
        EquatableArray<InjectableMemberModel> parameters = member.Parameters!.Value;
        
        for (int i = 0; i < parameters.Length; i++) {
            InjectableMemberModel parameter = parameters[i];
            
            (string method, string ctxExpr) = BuildResolverCall(parameter);
            string suffix = (i == parameters.Length - 1) ? "" : ",";
            
            writer.WriteLine($"__resolver.{method}<{parameters[i].TypeToRequest}>({ctxExpr}){suffix}");
        }
        
        writer.Indent--;
        writer.WriteLine(");");
    }
}