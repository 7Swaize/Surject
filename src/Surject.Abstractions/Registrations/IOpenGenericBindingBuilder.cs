using System;

namespace Surject.Abstractions.Registrations;

public interface IOpenGenericBindingBuilder {
    public IOpenGenericBindingBuilder To(Type openContractType);
    public IOpenGenericBindingBuilder ToImmediateImplementedInterfaces();
    public IOpenGenericBindingBuilder ToAllImplementedInterfaces();
    public IOpenGenericBindingBuilder WithId<TId>(TId id) where TId : IEquatable<TId>;

    public IOpenGenericBindingBuilder Nullable();
    
    public IOpenGenericBindingBuilder Eager();
    public IOpenGenericBindingBuilder Lazy();

    public IOpenGenericBindingBuilder OverrideExisting();
    public IOpenGenericBindingBuilder AsCollection(int order = 0);
    public IOpenGenericBindingBuilder AsPrimary();

    public IOpenGenericBindingBuilder DoNotDispose();
    public IOpenGenericBindingBuilder TrackDisposable();
}