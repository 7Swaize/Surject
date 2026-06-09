using System;
using Surject.Abstractions.Modifiers;
using Surject.Abstractions.Resolutions;

namespace Surject.Abstractions.Registrations;

public interface IServiceRegistry {
    public IBindingBuilder<T> Add<T>(Lifetime lifetime) where T : class;
    public IBindingBuilder<T> AddFactory<T>(Func<IResolver, T> factory, Lifetime lifetime) where T : class;
    
    public IOpenGenericBindingBuilder AddOpenGeneric(Type openImplType, Lifetime lifetime);
    
    public IServiceRegistry Decorate<TContract, TDecorator>() where TDecorator : class, TContract;

    public IBindingBuilder<TImpl> AddToCollection<TImpl>(Lifetime lifetime, int order = 0) where TImpl : class;
    public IBindingBuilder<TImpl> AddPrimaryToCollection<TImpl, TContract>(int order = 0) where TImpl : class;
}
