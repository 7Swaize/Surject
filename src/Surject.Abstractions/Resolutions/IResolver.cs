using System;

namespace Surject.Abstractions.Resolutions;

public interface IResolver : IAsyncResolver {
    T Resolve<T>() where T : class;
    T? ResolveOptional<T>() where T : class;
    T ResolvePrimary<T>() where T : class;
    T? ResolveOptionalPrimary<T>() where T : class;

    T ResolveKeyed<T>(string key) where T : class;
    T? ResolveOptionalKeyed<T>(string key) where T : class;
    T ResolvePrimaryKeyed<T>(string key) where T : class;
    T? ResolveOptionalPrimaryKeyed<T>(string key) where T : class;
    
    Lazy<T> ResolveLazy<T>() where T : class;
    Lazy<T?> ResolveOptionalLazy<T>() where T : class;
    Lazy<T> ResolvePrimaryLazy<T>() where T : class;
    Lazy<T?> ResolveOptionalPrimaryLazy<T>() where T : class;

    Lazy<T> ResolveKeyedLazy<T>(string key) where T : class;
    Lazy<T?> ResolveOptionalKeyedLazy<T>(string key) where T : class;
    Lazy<T> ResolvePrimaryKeyedLazy<T>(string key) where T : class;
    Lazy<T?> ResolveOptionalPrimaryKeyedLazy<T>(string key) where T : class;

    T[] ResolveAll<T>() where T : class;
    T[] ResolveAllKeyed<T>(string key) where T : class;
}