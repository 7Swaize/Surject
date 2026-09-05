using System;

namespace Surject.Abstractions.Registrations;

public interface IBindingBuilder<in T> {
    public IBindingBuilder<T> To<TContract>() where TContract : class?;
    public IBindingBuilder<T> ToImmediateImplementedInterfaces();
    public IBindingBuilder<T> ToAllImplementedInterfaces();
    public IBindingBuilder<T> WithId<TId>(TId id) where TId : IEquatable<TId>;

    public IBindingBuilder<T> Eager();
    public IBindingBuilder<T> Lazy();
    
    public IBindingBuilder<T> WithArgument<TArg>(string parameterName, TArg value);
    
    public IBindingBuilder<T> OverrideExisting();
    public IBindingBuilder<T> AsCollection(int order = 0);
    public IBindingBuilder<T> AsPrimary();
    
    public IBindingBuilder<T> DoNotDispose();
    public IBindingBuilder<T> TrackDisposable();
}