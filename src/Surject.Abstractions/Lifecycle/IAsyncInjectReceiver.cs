using System.Threading;
using System.Threading.Tasks;

namespace Surject.Abstractions.Lifecycle;

public interface IAsyncInjectReceiver {
    ValueTask OnInjectedAsync(CancellationToken ct = default);
}