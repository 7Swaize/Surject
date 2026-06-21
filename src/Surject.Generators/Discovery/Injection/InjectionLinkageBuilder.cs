using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Surject.Generators.Models.Concepts;
using Surject.Generators.Models.Primitives;

namespace Surject.Generators.Discovery.Injection;

internal static class InjectionLinkageBuilder {
    internal static Dictionary<ITypeReferenceModel, List<InjectableMemberModel>> BuildLinkageAsDictionary(
        ImmutableArray<InjectableTargetModel> targets)
    {
        Dictionary<ITypeReferenceModel, List<InjectableMemberModel>> map = [];

        foreach (InjectableTargetModel target in targets) {
            foreach (InjectableMemberModel member in target.MembersToInject) {
                ReadOnlySpan<InjectableMemberModel> members = member.Site == InjectionSiteKind.Method
                    ? member.Parameters!.Value.AsSpan()
                    : [member];
                
                foreach (InjectableMemberModel current in members) {
                    ITypeReferenceModel key = current.TypeToRequest!.UnboundGenericTypeRef!;

                    if (!map.TryGetValue(key, out List<InjectableMemberModel>? list)) {
                        list = [];
                        map.Add(key, list);
                    }

                    list.Add(current);
                }
            }
        }

        return map;
    }
}