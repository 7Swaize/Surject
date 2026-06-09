using System;

namespace Surject.Abstractions.Resolutions;

public interface IResolver {
    public T Resolve<T>() where T : class;
    public T? ResolveOptional<T>() where T : class;
    public T ResolveKeyed<T>(string key) where T : class;
    public T? ResolveKeyedOptional<T>(string key) where T : class;
    public Lazy<T> ResolveLazy<T>() where T : class;
    
    public T ResolvePrimary<T>() where T : class;
    public T? ResolvePrimaryOptional<T>() where T : class;
    
    public T[] ResolveAll<T>() where T : class;
    public T[] ResolveAllKeyed<T>(string key) where T : class;
}