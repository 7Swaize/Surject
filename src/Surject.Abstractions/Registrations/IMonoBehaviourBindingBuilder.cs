namespace Surject.Abstractions.Registrations;

public interface IMonoBehaviourBindingBuilder<in T> {
    public IMonoBehaviourBindingBuilder<T> To<TContract>() where TContract : class;
    public IMonoBehaviourBindingBuilder<T> ToImplementedInterfaces();
    public IMonoBehaviourBindingBuilder<T> WithId(string id);
    
    public IBindingBuilder<T> Pooled(int initialSize = 1);
    
    public IMonoBehaviourBindingBuilder<T> AsCollection(int order = 0);
    public IMonoBehaviourBindingBuilder<T> AsPrimary();
}