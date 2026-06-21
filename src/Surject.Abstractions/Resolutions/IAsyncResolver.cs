using System.Threading;
using System.Threading.Tasks;

namespace Surject.Abstractions.Resolutions;

public interface IAsyncResolver {
    ValueTask<T> ResolveAsync<T>(ResolveContext ctx, CancellationToken ct = default) where T : class;
    ValueTask<T?> ResolveOptionalAsync<T>(ResolveContext ctx, CancellationToken ct = default) where T : class;
    
    ValueTask<T[]>  ResolveAllAsync<T>(ResolveContext ctx, CancellationToken ct = default) where T : class;
}