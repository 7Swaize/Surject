using System;

namespace Surject.Abstractions.Registrations;

public interface IOpenGenericBindingBuilder {
    public IOpenGenericBindingBuilder To(Type openContractType);
    public IOpenGenericBindingBuilder ToImplementedInterfaces();
    public IOpenGenericBindingBuilder WithId(string id);
    
    public IOpenGenericBindingBuilder Pooled(int initialSize = 1);
    
    public IOpenGenericBindingBuilder Eager();
    public IOpenGenericBindingBuilder Lazy();
    
    public IOpenGenericBindingBuilder OverrideExisting();
    public IOpenGenericBindingBuilder AsCollection(int order = 0);
    public IOpenGenericBindingBuilder AsPrimary();
    
    public IOpenGenericBindingBuilder DoNotDispose();
    public IOpenGenericBindingBuilder TrackDisposable();
}