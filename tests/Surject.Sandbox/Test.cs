using Surject.Abstractions.Attributes;
using Surject.Abstractions.Modifiers;
using Surject.Abstractions.Registrations;

namespace Surject.Sandbox;

[Scope(ScopeLevel.Application)]
public class Test : ScopeContext {
    public override void Configure(IServiceRegistry registry) {
        registry
            .Add<Foo>(Lifetime.Singleton)
            .To<IFoo?>();

        registry
            .AddOpenGeneric(Lifetime.Singleton, typeof(Generic<,>))
            .To(typeof(IGeneric<,>))
            .Nullable();
    }
}


internal interface IFoo { }

internal class Foo : IFoo { }

internal interface IGeneric<T1, T2> { }

internal class Generic<T1, T2> : IGeneric<T1, T2> { }