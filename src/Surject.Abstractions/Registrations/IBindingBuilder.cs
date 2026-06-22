using System;
using JetBrains.Annotations;
using Surject.Abstractions.Resolutions;

namespace Surject.Abstractions.Registrations;

public interface IBindingBuilder<in T> {
    public IBindingBuilder<T> To<TContract>() where TContract : class;
    public IBindingBuilder<T> ToImplementedInterfaces();
    public IBindingBuilder<T> WithId(string id);
    
    public IBindingBuilder<T> Pooled(int initialSize = 1);

    public IBindingBuilder<T> Eager();
    public IBindingBuilder<T> Lazy();
    
    public IBindingBuilder<T> FromFactory([RequireStaticDelegate(IsError = true)] Func<IResolver, T> factory);
    public IBindingBuilder<T> FromInjectFactory();
    public IBindingBuilder<T> WithArgument<TArg>(string parameterName, TArg value);
    
    public IBindingBuilder<T> WhenInjectedInto<TConsumer>();
    public IBindingBuilder<T> When([RequireStaticDelegate(IsError = true)] Func<IBindingContext, bool> condition);
    
    public IBindingBuilder<T> OverrideExisting();
    public IBindingBuilder<T> AsCollection(int order = 0);
    public IBindingBuilder<T> AsPrimary();
    
    public IBindingBuilder<T> DoNotDispose();
    public IBindingBuilder<T> TrackDisposable();
}