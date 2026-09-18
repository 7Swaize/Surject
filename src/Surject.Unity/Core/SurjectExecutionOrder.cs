namespace Surject.Unity;

public enum SurjectExecutionOrder {
    ApplicationRoot = int.MinValue + 1,
    SceneRoot = int.MinValue + 50,
    SubScopeStaticRegistration = int.MinValue + 75,
    SubScopeRuntimeRegistration = int.MinValue + 100,
}