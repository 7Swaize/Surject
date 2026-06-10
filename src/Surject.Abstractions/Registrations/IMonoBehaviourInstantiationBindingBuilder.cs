using UnityEngine;

namespace Surject.Abstractions.Registrations;

public interface IMonoBehaviourInstantiationBindingBuilder<in T> : IMonoBehaviourBindingBuilder<T> {
    public IMonoBehaviourInstantiationBindingBuilder<T> UnderTransform(Transform transform);
    public IMonoBehaviourInstantiationBindingBuilder<T> UnderObjectOfType<TObject>() where TObject : Component;
    
    public IMonoBehaviourInstantiationBindingBuilder<T> WithGameObjectName(string name);
    
    public IMonoBehaviourInstantiationBindingBuilder<T> DoNotDestroy();
}