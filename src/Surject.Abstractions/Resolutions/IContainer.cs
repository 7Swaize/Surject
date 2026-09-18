namespace Surject.Abstractions.Resolutions;

public interface IContainer {
    public IResolver Resolver { get; }
    public IResolver? ParentResolver { get; }
}