using System;
using Surject.Abstractions.Resolutions;

namespace Surject.Abstractions.Registrations;

public interface IBindingBuilder<in T> {
    IBindingBuilder<T> To<TContract>() where TContract : class;
    IBindingBuilder<T> ToImplementedInterfaces();
    IBindingBuilder<T> WithId(string id);
    
    IBindingBuilder<T> Pooled(int initialSize = 1);

    IBindingBuilder<T> Eager();
    IBindingBuilder<T> Lazy();
    
    IBindingBuilder<T> FromFactory(Func<IResolver, T> factory);
    IBindingBuilder<T> FromInjectFactory();
    IBindingBuilder<T> WithArgument<TArg>(string parameterName, TArg value);
    
    IBindingBuilder<T> WhenInjectedInto<TConsumer>();
    IBindingBuilder<T> When(Func<IBindingContext, bool> condition);
    
    IBindingBuilder<T> OverrideExisting();
    IBindingBuilder<T> AsCollection(int order = 0);
    IBindingBuilder<T> AsPrimary();
    
    IBindingBuilder<T> DoNotDispose();
    IBindingBuilder<T> TrackDisposable();
}