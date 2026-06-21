using System.Collections.Immutable;
using System.Linq;
using Surject.Generators.Discovery.Injection;
using Surject.Generators.Models.Collections;
using Surject.Generators.Models.Primitives;

namespace Surject.Generators.Models.Concepts;

internal sealed record InjectionLinkage {
    internal InjectionLinkage(ImmutableArray<InjectableTargetModel> targets) {
        Linkage = InjectionLinkageBuilder.BuildLinkageAsDictionary(targets)
            .ToDictionary(
                static kvp => kvp.Key,
                static kvp => kvp.Value.ToImmutableArray().AsEquatableArray()
            )
            .ToEquatableDictionary();
    }

    internal EquatableDictionary<ITypeReferenceModel, EquatableArray<InjectableMemberModel>> Linkage { get; init; }
}