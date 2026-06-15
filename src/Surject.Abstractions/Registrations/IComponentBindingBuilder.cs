namespace Surject.Abstractions.Registrations;

public interface IComponentBindingBuilder<in T> {
    public IComponentBindingBuilder<T> To<TContract>() where TContract : class;
    public IComponentBindingBuilder<T> ToImplementedInterfaces();
    public IComponentBindingBuilder<T> WithId(string id);
    
    public IBindingBuilder<T> Pooled(int initialSize = 1);
    
    public IComponentBindingBuilder<T> AsCollection(int order = 0);
    public IComponentBindingBuilder<T> AsPrimary();
}