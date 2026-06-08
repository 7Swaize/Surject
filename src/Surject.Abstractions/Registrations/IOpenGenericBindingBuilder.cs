using System;

namespace Surject.Abstractions.Registrations;

public interface IOpenGenericBindingBuilder {
    IOpenGenericBindingBuilder To(Type openContractType);
    IOpenGenericBindingBuilder ToImplementedInterfaces();
    IOpenGenericBindingBuilder WithId(string id);
    
    IOpenGenericBindingBuilder Pooled(int initialSize = 1);
    
    IOpenGenericBindingBuilder Eager();
    IOpenGenericBindingBuilder Lazy();
    
    IOpenGenericBindingBuilder OverrideExisting();
    IOpenGenericBindingBuilder AsCollection(int order = 0);
    IOpenGenericBindingBuilder AsPrimary();
    
    IOpenGenericBindingBuilder DoNotDispose();
    IOpenGenericBindingBuilder TrackDisposable();
}