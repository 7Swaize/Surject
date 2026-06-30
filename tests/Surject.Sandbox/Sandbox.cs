using System;
using Surject.Abstractions.Attributes;
using Surject.Abstractions.Modifiers;
using Surject.Abstractions.Registrations;

namespace Surject.Sandbox;

[Scope(ScopeLevel.Application)]
internal sealed partial class Scope1 : ScopeContext {
    public override void Configure(IServiceRegistry registry) {
        registry.Add<Foo>(Lifetime.Singleton)
            .To<IFoo>();

        registry.Add<Foo>(Lifetime.Singleton).To<IFoo>().AsPrimary().AsCollection().DoNotDispose();
    }
}

internal interface IFoo { }

internal sealed class Foo { }

internal interface IGeneric<T1> { }

internal partial class Generic<T1, T2, T3> : IGeneric<T1> { }

internal partial class Generic<T1, T2, T3> { }