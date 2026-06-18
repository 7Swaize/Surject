using System.Threading;
using System.Threading.Tasks;

namespace Surject.Abstractions.Resolutions;

public interface IAsyncResolver {
    ValueTask<T> ResolveAsync<T>(CancellationToken ct = default) where T : class;
    ValueTask<T?> ResolveOptionalAsync<T>(CancellationToken ct = default) where T : class;
    ValueTask<T> ResolvePrimaryAsync<T>(CancellationToken ct = default) where T : class;
    ValueTask<T?> ResolveOptionalPrimaryAsync<T>(CancellationToken ct = default) where T : class;

    ValueTask<T> ResolveKeyedAsync<T>(string key, CancellationToken ct = default) where T : class;
    ValueTask<T?> ResolveOptionalKeyedAsync<T>(string key, CancellationToken ct = default) where T : class;
    ValueTask<T> ResolvePrimaryKeyedAsync<T>(string key, CancellationToken ct = default) where T : class;
    ValueTask<T?> ResolveOptionalPrimaryKeyedAsync<T>(string key, CancellationToken ct = default) where T : class;

    ValueTask<T[]> ResolveAllAsync<T>(CancellationToken ct = default) where T : class;
    ValueTask<T[]> ResolveAllKeyedAsync<T>(string key, CancellationToken ct = default) where T : class;
}