using UnityEngine;

namespace Surject.Abstractions.Registrations;

[DisallowMultipleComponent]
public abstract class ScopeContext : MonoBehaviour {
    public abstract void Configure(IServiceRegistry registry);
}