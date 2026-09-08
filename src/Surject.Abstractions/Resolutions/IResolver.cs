namespace Surject.Abstractions.Resolutions;

public interface IResolver {
    public T Resolve<T>(ResolveContext ctx = default) where T : class;
    public T? ResolveOptional<T>(ResolveContext ctx = default) where T : class;
    public T[] ResolveAll<T>(ResolveContext ctx = default) where T : class;
}