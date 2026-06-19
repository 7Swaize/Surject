using System;
using Surject.Abstractions.Modifiers;

namespace Surject.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ScopeAttribute(ScopeLevel scopeLevel, Type? parentScope = null) : Attribute {
    public ScopeLevel ScopeLevel { get; init; } = scopeLevel; 
    public Type? ParentScope { get; init; } = parentScope;
}