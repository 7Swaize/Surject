using System;
using System.Threading;
using System.Threading.Tasks;

namespace Surject.Abstractions.Resolutions;

public interface IScope : IDisposable, IAsyncDisposable {
    public IResolver Resolver { get; }
    public IResolver ParentResolver { get; }

    public IScope CreateChildScope<TScopeProvider>(IResolver? parentOverride = null) where TScopeProvider : class;
    public ValueTask<IScope> CreateChildScopeAsync<TScopeProvider>(
        IResolver? parentOverride = null,
        CancellationToken c = default) 
            where TScopeProvider : class;
}