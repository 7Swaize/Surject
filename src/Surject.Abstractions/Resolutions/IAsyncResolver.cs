using System.Threading;
using System.Threading.Tasks;

namespace Surject.Abstractions.Resolutions;

public interface IAsyncResolver {
    public ValueTask<T> ResolveAsync<T>(CancellationToken c = default) where T : class;
    public ValueTask<T?> ResolveAsyncOptional<T>(CancellationToken c = default) where T : class;
    public ValueTask<T> ResolveAsyncPrimary<T>(CancellationToken c = default) where T : class;
    public ValueTask<T[]> ResolveAsyncAll<T>(CancellationToken c = default) where T : class;
}