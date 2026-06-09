using Surject.Abstractions.Attributes;
using Surject.Abstractions.Modifiers;
using Surject.Abstractions.Registrations;
using UnityEngine;

namespace Surject.Sandbox;

[ScopeProvider]
internal sealed partial class ScopeProvider : MonoBehaviour, IScopeProvider {
    public void Configure(IServiceRegistry registry) {
        registry.Add<Foo>(Lifetime.Singleton)
            .To<IFoo>();
    }
}

internal interface IFoo { }

internal sealed class Foo { }