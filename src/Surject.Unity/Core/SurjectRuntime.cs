using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Surject.Abstractions.Resolutions;
using Surject.Unity.Utility.Collections;
using Surject.Unity.Utility.Exceptions;
using UnityEngine.SceneManagement;

namespace Surject.Unity;

public sealed class SurjectRuntime : Singleton<SurjectRuntime> {
    private IResolver? _rootResolver;
    private readonly Dictionary<Scene, IResolver> _sceneResolvers = new();
    private readonly DeferredTypeRegistry<IResolver> _staticScopedResolvers = new();

    public IResolver RootResolver {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _rootResolver
               ?? ThrowHelpers.ThrowSurjectRuntimeException<IResolver>(
                   "ApplicationRoot was accessed before the Application Root scope was registered.");
    }

    public void RegisterRootResolver(IResolver resolver) {
        if (_rootResolver != null) {
            ThrowHelpers.ThrowSurjectRuntimeException(
                "ApplicationRoot resolver was accessed before the ApplicationRoot scope was registered."    
            );
        }
        
        _rootResolver = resolver;
    }

    public void UnregisterRootResolver() {
        _rootResolver = null;
    }

    public void RegisterSceneResolver(Scene scene, IResolver resolver) => _sceneResolvers[scene] = resolver;
    public void UnregisterSceneResolver(Scene scene) => _sceneResolvers.Remove(scene);
    public IResolver? GetSceneResolver(Scene scene) => _sceneResolvers.TryGetValue(scene, out var r) ? r : null;
    
#if UNITY_EDITOR
    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void __ClearStatics() {
        _rootResolver = null;
        _sceneResolvers.Clear();
        _staticScopedResolvers.Clear();
    }
#endif
    
    public void QueueStaticScopedResolverContinuation<TProvideConcrete, TDependConcrete>(Func<TDependConcrete, TProvideConcrete> continuation)
        where TProvideConcrete : IResolver
        where TDependConcrete : IResolver
    {
        _staticScopedResolvers.RegisterDeferred(continuation);
    }

    public void UnregisterStaticScopedResolver<TConcrete>() where TConcrete : IResolver {
        _staticScopedResolvers.Unregister<TConcrete>();
    }

    public IDisposable EnqueuePendingParent(IResolver parent) {
        throw new NotImplementedException();
    }

    public bool TryConsumePendingParent(out IResolver parent) {
        throw new NotImplementedException();
    }
}