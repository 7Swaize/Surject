using System.CodeDom.Compiler;
using System.Text;
using Surject.Abstractions.Resolutions;
using Surject.Generators.Models.Collections;
using Surject.Generators.Models.Concepts;
using Surject.Shared.Helpers;
using static Surject.Generators.Emitters.TypeNames;

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
        string resolveMethName = BuildResolveMethodName(member.Mode);
        string resolveContext = BuildResolveContext(member);
        
        writer.WriteLine(
            $"this.{member.Name} = __resolver.{resolveMethName}<{member.TypeToRequest}>({resolveContext});"
        );
        
    }

    private static void ResolveMethod(in InjectableMemberModel member, IndentedTextWriter writer) {
        writer.WriteLine($"this.{member.Name}(");
        writer.Indent++;
        
        EquatableArray<InjectableMemberModel> parameters = member.Parameters!.Value;
        
        for (int i = 0; i < parameters.Length; i++) {
            InjectableMemberModel parameter = parameters[i];
            
            string resolveMethName = BuildResolveMethodName(parameter.Mode);
            string resolveContext = BuildResolveContext(parameter);
            string suffix = (i == parameters.Length - 1) ? "" : ",";
            
            writer.WriteLine($"__resolver.{resolveMethName}<{parameters[i].TypeToRequest}>({resolveContext}){suffix}");
        }
        
        writer.Indent--;
        writer.WriteLine(");");
    }

    private static string BuildResolveMethodName(InjectionMode mode) {
        StringBuilder sb = new StringBuilder("Resolve");
        
        // Naming of methods follows a distinct order. Therefore, this pattern works.
        // Any invalid permutation is caught via compile time analysis 
        if ((mode & InjectionMode.Optional) != 0) sb.Append("Optional");
        if ((mode & InjectionMode.All) != 0) sb.Append("All");
        if ((mode & InjectionMode.Lazy) != 0) sb.Append("Lazy");
        if ((mode & InjectionMode.Async) != 0) sb.Append("Async");
        
        return sb.ToString();
    }

    private static string BuildResolveContext(in InjectableMemberModel member) {
        static void AppendFlag(ref string? flags, ResolveFlags flag) {
            string value = $"{FQN(typeof(ResolveFlags))}.{flag}";
            flags = flags is null ? value : $"{flags} | {value}";
        }
        
        string? key = member.Id;
        string? flags = null;
        
        if ((member.Mode & InjectionMode.Primary) != 0) {
            AppendFlag(ref flags, ResolveFlags.Primary);
        }

        if ((member.Mode & InjectionMode.Keyed) != 0) {
            AppendFlag(ref flags, ResolveFlags.Keyed);
        }

        return (flags, key) switch {
            (null, null) => $"new {FQN(typeof(ResolveFlags))}()",
            (_, null) => $"new {FQN(typeof(ResolveFlags))}({flags})",
            (null, _) => $"new {FQN(typeof(ResolveFlags))}(key: \"{key}\")",
            _ => $"new {FQN(typeof(ResolveFlags))}({flags}, \"{key}\")",
        };
    }
}