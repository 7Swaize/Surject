using System;

namespace Surject.Abstractions.Registrations;

public interface IComponentBindingBuilder<in T> {
    public IComponentBindingBuilder<T> To<TContract>() where TContract : class?;
    public IComponentBindingBuilder<T> ToImmediateImplementedInterfaces();
    public IComponentBindingBuilder<T> ToImplementedInterfaces();
    public IComponentBindingBuilder<T> WithId<TId>(TId id) where TId : IEquatable<TId>;
    
    public IComponentBindingBuilder<T> OverrideExisting();
    public IComponentBindingBuilder<T> AsCollection(int order = 0);
    public IComponentBindingBuilder<T> AsPrimary();
}