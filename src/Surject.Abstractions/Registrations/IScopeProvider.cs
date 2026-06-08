namespace Surject.Abstractions.Registrations;

public interface IScopeProvider {
    public void Configure(IServiceRegistry registry);
}