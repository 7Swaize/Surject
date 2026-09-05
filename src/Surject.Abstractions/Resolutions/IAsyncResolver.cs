using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Surject.Abstractions.Resolutions;

public interface IAsyncResolver {
    public ValueTask<T> ResolveAsync<T>(ResolveContext ctx, CancellationToken cts = default) where T : class;
    public ValueTask<T?> ResolveOptionalAsync<T>(ResolveContext ctx, CancellationToken cts = default) where T : class;
    public ValueTask<T[]> ResolveAllAsync<T>(ResolveContext ctx, CancellationToken cts = default) where T : class;
}