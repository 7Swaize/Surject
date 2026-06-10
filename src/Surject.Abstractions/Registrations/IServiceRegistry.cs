using System;
using System.Threading;
using System.Threading.Tasks;
using Surject.Abstractions.Modifiers;
using Surject.Abstractions.Resolutions;
using UnityEngine;

namespace Surject.Abstractions.Registrations;

public interface IServiceRegistry {
    public IBindingBuilder<TImpl> Add<TImpl>(Lifetime lifetime) where TImpl : class;
    public IBindingBuilder<TImpl> AddFactory<TImpl>(Func<IResolver, TImpl> factory, Lifetime lifetime) where TImpl : class;
    
    public IOpenGenericBindingBuilder AddOpenGeneric(Type openImplType, Lifetime lifetime);

    public IBindingBuilder<TImpl> AddToCollection<TImpl>(Lifetime lifetime, int order = 0) where TImpl : class;
    public IBindingBuilder<TImpl> AddPrimaryToCollection<TImpl>(Lifetime lifetime, int order = 0) where TImpl : class;

    public IAsyncBindingBuilder<TImpl> AddAsyncFactory<TImpl>(Lifetime lifetime, Func<IResolver, CancellationToken, ValueTask<TImpl>> factory)
        where TImpl : class;

    public IMonoBehaviourBindingBuilder<TImpl> AddFromHierarchy<TImpl>(Lifetime lifetime) where TImpl : Component;
    public IMonoBehaviourBindingBuilder<TImpl> AddAllFromHierarchy<TImpl>(Lifetime lifetime) where TImpl : Component;
    public IMonoBehaviourBindingBuilder<TImpl> AddFromSibling<TImpl>(Lifetime lifetime) where TImpl : Component;
    public IMonoBehaviourBindingBuilder<TImpl> AddFromChildren<TImpl>(Lifetime lifetime) where TImpl : Component;
    public IMonoBehaviourBindingBuilder<TImpl> AddAllFromChildren<TImpl>(Lifetime lifetime) where TImpl : Component;
    public IMonoBehaviourBindingBuilder<TImpl> AddFromParent<TImpl>(Lifetime lifetime) where TImpl : Component;
    public IMonoBehaviourBindingBuilder<TImpl> AddAllFromParent<TImpl>(Lifetime lifetime) where TImpl : Component;
    
    public IMonoBehaviourInstantiationBindingBuilder<TImpl> AddNewComponent<TImpl>(Lifetime lifetime)
        where TImpl : Component;
    public IMonoBehaviourInstantiationBindingBuilder<TImpl> AddFromPrefab<TImpl>(Lifetime lifetime, GameObject prefab)
        where TImpl : Component;
    
    public IServiceRegistry Decorate<TContract, TDecorator>() where TDecorator : class, TContract;
}