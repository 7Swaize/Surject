using System;

namespace Surject.Abstractions.Resolutions;

public interface IResolver : IAsyncResolver {
    T Resolve<T>(ResolveContext ctx) where T : class;
    T? ResolveOptional<T>(ResolveContext ctx) where T : class;
    
    Lazy<T> ResolveLazy<T>(ResolveContext ctx) where T : class;
    Lazy<T?> ResolveOptionalLazy<T>(ResolveContext ctx) where T : class;
    
    T[] ResolveAll<T>(ResolveContext ctx) where T : class;
}