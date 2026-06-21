using Surject.Abstractions.Resolutions;

namespace Surject.Abstractions.Lifecycle;

public interface IInjectable {
    public void __Surject_Inject(IResolver __resolver);
}