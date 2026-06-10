using Surject.Abstractions.Attributes;
using Surject.Abstractions.Modifiers;
using Surject.Abstractions.Registrations;

namespace Surject.Sandbox;

[Scope]
internal sealed partial class Scope1 : ScopeContext {
    public override void Configure(IServiceRegistry registry) {
        registry.Add<Foo>(Lifetime.Singleton)
            .To<IFoo>();
    }
}


internal interface IFoo { }

internal sealed class Foo { }