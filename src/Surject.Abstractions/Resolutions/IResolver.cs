namespace Surject.Abstractions.Resolutions;

public interface IResolver : IAsyncResolver {
    public TTarget Resolve<TTarget, TKey>(ResolveContext<TKey> ctx = default) where TTarget : class;
    public TTarget? ResolveOptional<TTarget, TKey>(ResolveContext<TKey> ctx = default) where TTarget : class;
    public TTarget[] ResolveAll<TTarget, TKey>(ResolveContext<TKey> ctx = default) where TTarget : class;
}