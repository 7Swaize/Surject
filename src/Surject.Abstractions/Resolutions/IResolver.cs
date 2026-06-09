using System;

namespace Surject.Abstractions.Resolutions;

public interface IResolver {
    T Resolve<T>() where T : class;
    T? ResolveOptional<T>() where T : class;
    T ResolveKeyed<T>(string key) where T : class;
    T? ResolveKeyedOptional<T>(string key) where T : class;
    Lazy<T> ResolveLazy<T>() where T : class;
    
    T ResolvePrimary<T>() where T : class;
    T? ResolvePrimaryOptional<T>() where T : class;
    
    T[] ResolveAll<T>() where T : class;
    T[] ResolveAllKeyed<T>(string key) where T : class;
}