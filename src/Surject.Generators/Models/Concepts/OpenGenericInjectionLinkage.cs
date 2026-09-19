using System.Collections.Immutable;
using Surject.Generators.Discovery.Injection;
using Surject.Generators.Models.Collections;
using Surject.Generators.Models.Primitives;

namespace Surject.Generators.Models.Concepts;

internal sealed record OpenGenericInjectionLinkage {
    internal OpenGenericInjectionLinkage(
        ImmutableArray<ContainerModel> containers,
        ImmutableArray<InjectableContainerModel> injectionRequests) 
    {
        Linkage = OpenGenericInjectionLinkageBuilder.BuildInjectionLinkage(containers, injectionRequests);
    }

    internal EquatableDictionary<ITypeReferenceModel, EquatableArray<ITypeReferenceModel>> Linkage { get; init; }
}