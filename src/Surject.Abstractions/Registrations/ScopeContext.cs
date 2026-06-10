using UnityEngine;

namespace Surject.Abstractions.Registrations;

public abstract class ScopeContext : MonoBehaviour {
    public abstract void Configure(IServiceRegistry registry);
}