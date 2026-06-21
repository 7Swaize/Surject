using System;

namespace Surject.Abstractions.Resolutions;

public interface IContainer : IDisposable, IAsyncDisposable {
    public IResolver Resolver { get; }
    public IResolver ParentResolver { get; }
}