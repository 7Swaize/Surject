using System;

namespace Surject.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Constructor | AttributeTargets.Method)]
public sealed class InjectAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
                AttributeTargets.Constructor | AttributeTargets.Method |
                AttributeTargets.Parameter)]
public sealed class InjectOptionalAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class InjectLazyAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class IdAttribute(string key) : Attribute {
    public string Key { get; } = key;
}


[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class InjectableAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Method)]
public sealed class InjectFactoryAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class InjectPrimaryAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class InjectAsyncAttribute : Attribute { }

