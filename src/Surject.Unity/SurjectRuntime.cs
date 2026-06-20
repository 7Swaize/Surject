using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Surject.Abstractions.Resolutions;
using Surject.Unity.RuntimeExceptions;
using UnityEngine.SceneManagement;

namespace Surject.Unity;

public static class SurjectRuntime {
    private static IResolver? _rootResolver;
    
    public static IResolver RootResolver {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _rootResolver
               ?? ThrowHelpers.ThrowSurjectRuntimeException<IResolver>(
                   "SurjectRuntime.RootResolver was accessed before the Application Root scope was built.");
    }
    
    public static void RegisterRootResolver(IResolver resolver) {
        if (_rootResolver != null) {
            ThrowHelpers.ThrowSurjectRuntimeException("SurjectRuntime.RegisterRootResolver was called twice.");
        }
        
        _rootResolver = resolver;
    }

    public static void UnregisterRootResolver() {
        _rootResolver = null;
    }
    
    private static readonly ConcurrentDictionary<Scene, IResolver> _sceneResolvers = new();

    public static void RegisterSceneResolver(Scene scene, IResolver resolver) => _sceneResolvers[scene] = resolver;
    public static void UnregisterSceneResolver(Scene scene) => _sceneResolvers.TryRemove(scene, out _);
    public static IResolver? GetSceneResolver(Scene scene) => _sceneResolvers.TryGetValue(scene, out var resolver) ? resolver : null;

#if UNITY_EDITOR
    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void __ResetOnDomainReload() {
        _rootResolver = null; // safety
        _sceneResolvers.Clear();
    }
#endif
}