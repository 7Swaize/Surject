using UnityEngine;

namespace Surject.Unity.Utility.Collections;

// See ref: https://github.com/adammyhre/Unity-Utils/blob/master/UnityUtils/Scripts/Singleton/PersistentSingleton.cs
public abstract class Singleton<T> : MonoBehaviour where T : Component {
    protected static T? _instance;
    
    public static bool HasInstance => _instance != null;
    public static T? TryGetInstance() => _instance;

    public static T Instance {
        get {
            if (_instance == null) {
                _instance = FindObjectOfType<T>();
                if (_instance == null) {
                    var go = new GameObject(typeof(T).Name + " Auto-Generated");
                    _instance = go.AddComponent<T>();
                }
            }
            
            return _instance;
        }
    }

    public virtual void Awake() {
        InitializeSingleton();
    }

    private void InitializeSingleton() {
        if (!Application.isPlaying) {
            return;
        }

        if (_instance == null) {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else {
            if (_instance != this) {
                Destroy(gameObject);
            }
        }
    }
}