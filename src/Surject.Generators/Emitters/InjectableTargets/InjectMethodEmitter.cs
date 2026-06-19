using System.CodeDom.Compiler;
using System.Text;
using Surject.Generators.Models.Collections;
using Surject.Generators.Models.Concepts;
using Surject.Shared.Helpers;

namespace Surject.Generators.Emitters.InjectableTargets;

internal readonly struct InjectMethodEmitter {
    private readonly InjectableTargetModel _model;
    
    internal InjectMethodEmitter(InjectableTargetModel model) {
        _model = model;
    }

    internal void Emit(IndentedTextWriter writer) {
        EmitHelpers.EmitPreserveAttribute(writer);
        EmitHelpers.EmitEditorBrowsableNeverAttribute(writer);
        
        writer.WriteLine($"public void __Surject_Inject(global::Surject.Abstractions.Resolutions.IResolver __resolver) {{");
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
        string resolveMethName = BuildResolveMethodName(member.Mode);
        string id = member.Id ?? "";
        
        writer.WriteLine(
            $"this.{member.Name} = __resolver.{resolveMethName}<{member.TypeToRequest}>({id});"
        );
        
    }

    private static void ResolveMethod(in InjectableMemberModel member, IndentedTextWriter writer) {
        writer.WriteLine($"this.{member.Name}(");
        writer.Indent++;
        
        EquatableArray<InjectableMemberModel> parameters = member.Parameters!.Value;
        
        for (int i = 0; i < parameters.Length; i++) {
            InjectableMemberModel parameter = parameters[i];
            
            string resolveMethName = BuildResolveMethodName(parameter.Mode);
            string id = parameter.Id ?? "";
            string suffix = (i == parameters.Length - 1) ? "" : ",";
            
            writer.WriteLine($"__resolver.{resolveMethName}<{parameters[i].TypeToRequest}>({id}){suffix}");
        }
        
        writer.Indent--;
        writer.WriteLine(");");
    }

    private static string BuildResolveMethodName(InjectionMode mode) {
        StringBuilder sb = new StringBuilder("Resolve");
        
        if ((mode & InjectionMode.Optional) != 0) sb.Append("Optional");
        if ((mode & InjectionMode.Primary) != 0) sb.Append("Primary");
        if ((mode & InjectionMode.All) != 0) sb.Append("All");
        if ((mode & InjectionMode.Keyed) != 0) sb.Append("Keyed");
        if ((mode & InjectionMode.Lazy) != 0) sb.Append("Lazy");
        if ((mode & InjectionMode.Async) != 0) sb.Append("Async");
        
        return sb.ToString();
    }
}