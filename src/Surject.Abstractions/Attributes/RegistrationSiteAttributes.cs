using System;
using Surject.Abstractions.Modifiers;

namespace Surject.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ScopeAttribute(ScopeLevel scopeLevel) : Attribute {
    public ScopeLevel ScopeLevel { get; init; } = scopeLevel;
}