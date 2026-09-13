using System.Threading;
using System.Threading.Tasks;

namespace Surject.Abstractions.Resolutions;

public interface IAsyncResolver {
    ValueTask<TTarget> ResolveAsync<TTarget, TKey>(ResolveContext<TKey> ctx = default, CancellationToken ct = default) where TTarget : class;
    ValueTask<TTarget?> ResolveOptionalAsync<TTarget, TKey>(ResolveContext<TKey> ctx = default, CancellationToken ct = default) where TTarget : class;
    ValueTask<TTarget[]> ResolveAllAsync<TTarget, TKey>(ResolveContext<TKey> ctx = default, CancellationToken ct = default) where TTarget : class;
}