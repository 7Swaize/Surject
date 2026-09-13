using System;
using Surject.Abstractions.Modifiers;

namespace Surject.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ApplicationRootAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ScopeAttribute(ParentDiscovery discovery, Type? parentScope = null) : Attribute {
    public ParentDiscovery ScopeLevel { get; init; } = discovery;
    public Type? ParentScope { get; init; } = parentScope;
}