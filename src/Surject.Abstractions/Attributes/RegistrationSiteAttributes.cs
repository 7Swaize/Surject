using System;

namespace Surject.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ScopeAttribute(Type? parentScope = null) : Attribute {
    public Type? ParentScope { get; } = parentScope;
}