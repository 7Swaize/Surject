using UnityEngine;

namespace Surject.Abstractions.Registrations;

public interface IComponentInstantiationBindingBuilder<in T> : IComponentBindingBuilder<T> {
    public IComponentInstantiationBindingBuilder<T> UnderTransform(Transform transform);
    public IComponentInstantiationBindingBuilder<T> UnderObjectOfType<TObject>() where TObject : Component;
    
    public IComponentInstantiationBindingBuilder<T> WithGameObjectName(string name);
    
    public IComponentInstantiationBindingBuilder<T> DoNotDestroy();
}