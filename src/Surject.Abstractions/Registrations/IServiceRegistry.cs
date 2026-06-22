using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Surject.Abstractions.Modifiers;
using Surject.Abstractions.Resolutions;
using UnityEngine;

namespace Surject.Abstractions.Registrations;

public interface IServiceRegistry {
    public IBindingBuilder<TImpl> Add<TImpl>(Lifetime lifetime) where TImpl : class;
    public IBindingBuilder<TImpl> AddFactory<TImpl>(
        Lifetime lifetime,
        [RequireStaticDelegate(IsError = true)] Func<IResolver, TImpl> factory
    )
        where TImpl : class;
    
    public IOpenGenericBindingBuilder AddOpenGeneric(Lifetime lifetime, Type openImplType);

    public IBindingBuilder<TImpl> AddToCollection<TImpl>(Lifetime lifetime, int order = 0) where TImpl : class;
    public IBindingBuilder<TImpl> AddPrimaryToCollection<TImpl>(Lifetime lifetime, int order = 0) where TImpl : class;
    
    public IAsyncBindingBuilder<TImpl> AddAsyncFactory<TImpl>(
        Lifetime lifetime,
        [RequireStaticDelegate(IsError = true)] Func<IResolver, CancellationToken, ValueTask<TImpl>> factory
    )
        where TImpl : class;

    public IComponentBindingBuilder<TImpl> AddFromHierarchy<TImpl>(Lifetime lifetime) where TImpl : Component;
    public IComponentBindingBuilder<TImpl> AddAllFromHierarchy<TImpl>(Lifetime lifetime) where TImpl : Component;
    public IComponentBindingBuilder<TImpl> AddFromSibling<TImpl>(Lifetime lifetime) where TImpl : Component;
    public IComponentBindingBuilder<TImpl> AddFromChildren<TImpl>(Lifetime lifetime) where TImpl : Component;
    public IComponentBindingBuilder<TImpl> AddAllFromChildren<TImpl>(Lifetime lifetime) where TImpl : Component;
    public IComponentBindingBuilder<TImpl> AddFromParent<TImpl>(Lifetime lifetime) where TImpl : Component;
    public IComponentBindingBuilder<TImpl> AddAllFromParent<TImpl>(Lifetime lifetime) where TImpl : Component;
    
    public IComponentInstantiationBindingBuilder<TImpl> AddNewComponent<TImpl>(Lifetime lifetime)
        where TImpl : Component;
    public IComponentInstantiationBindingBuilder<TImpl> AddFromPrefab<TImpl>(Lifetime lifetime, GameObject prefab)
        where TImpl : Component;
    
    public void Decorate<TContract, TDecorator>() where TDecorator : class, TContract;
}