using Sirenix.OdinInspector;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine; 
public class ScriptableSingleton : SerializedScriptableObject
{
    static public readonly string SingletonsResFolder = "Singletons";
#if UNITY_EDITOR
    static ScriptableSingleton()
    {
        EditorApplication.delayCall += SetAllSingletonsAsPreloaded;
    }
    private static void SetAllSingletonsAsPreloaded()
    {
        var allSingletons = Resources.LoadAll(SingletonsResFolder).ToList();
        var preloadedAssets = PlayerSettings.GetPreloadedAssets().ToList();
        var removed = preloadedAssets.RemoveAll(o => o == null);
        var nonPreloadedRegistries = allSingletons.Except(preloadedAssets).ToList();
        if (nonPreloadedRegistries.Count > 0 || removed > 0)
        {
            var arr = nonPreloadedRegistries.Union(preloadedAssets).ToArray();
            PlayerSettings.SetPreloadedAssets(arr);
        }

        foreach(var singleton in allSingletons)
        {
            if (singleton is ScriptableSingleton so)
                so.OnEditorPreloaded();
        }
    }
    protected virtual void OnEditorPreloaded() { }
#endif
}

public class ScriptableSingleton<T> : ScriptableSingleton where T : ScriptableSingleton
{ 

    private static T _instance;

    public static T Instance
    {
        get
        {
            if (!_instance)
                _instance = Load();

            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance != null)
            Debug.LogError($"ScriptableSingleton {_instance} already exists"
#if UNITY_EDITOR
                + $" at {AssetDatabase.GetAssetPath(_instance)}"
#endif
                );
        else
            _instance = this as T;
    }

    public void WakeupInstance() => Awake(); 
    static T Load()
    {
        var dirPath = SingletonsResFolder;
        var instances = Resources.LoadAll<T>(dirPath);
        T instance = null;
        if (instances.Length == 0)
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                Directory.CreateDirectory(SingletonsResFolder);
                var path = Path.Join(SingletonsResFolder, typeof(T).Name + ".asset");
                AssetDatabase.CreateAsset(CreateInstance<T>(), path);
                AssetDatabase.ImportAsset(path);
                Debug.Log("Created Scriptable singleton at " + path); 
            }
            else
#endif
                Debug.LogError($"Critical Error: Scriptable Singleton of type {typeof(T).FullName} Not Found In {dirPath}");
        }
        else
            instance = instances[0];

        return instance;
    } 
}
