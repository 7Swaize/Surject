namespace Surject.Abstractions.Resolutions;

public interface IMultiBindResolver {
    public (int Order, TTarget Instance)[] ResolveAllOrdered<TTarget, TKey>(ResolveContext<TKey> ctx = default) where TTarget : class;
}