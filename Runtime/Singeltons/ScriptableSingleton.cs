using Sirenix.OdinInspector;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public abstract class ScriptableSingleton : SerializedScriptableObject
{
    static public readonly string SingletonsResFolder = "Singletons";
    static public readonly string AssetsSingletonsResFolder = "Assets/Resources/" + SingletonsResFolder;
    bool _isSingletonStart;
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

        foreach (var singleton in allSingletons)
        {
            if (singleton is ScriptableSingleton so)
                so.OnEditorPreloaded();
        }
    }
    protected virtual void OnEditorPreloaded() { }
#endif

    public void VerifySingletonEnable()
    {
        if (_isSingletonStart)
            return;
        _isSingletonStart = true;
        OnSingletonEnable();
#if UNITY_EDITOR
        OnSingletonEditorAwake();
#endif
    }

    public abstract void OnSingletonEnable();

#if UNITY_EDITOR
    public virtual void OnSingletonEditorAwake() { }
#endif
}
#if UNITY_EDITOR
[InfoBox("Scriptable Instance<" + nameof(T) + ">", nameof(IsInstance), InfoMessageType = InfoMessageType.Info)]
[InfoBox( "@" + nameof(GetNonInstanceInspectorInfobox) + "()", "@!" + nameof(IsInstance),InfoMessageType = InfoMessageType.Warning)]
#endif
public abstract class ScriptableSingleton<T> : ScriptableSingleton where T : ScriptableSingleton
{

#if UNITY_EDITOR
    string GetNonInstanceInspectorInfobox()
    {
        var str = $"Not instance of <" + typeof(T).Name + ">";
        if (_instance)
            str += $"({AssetDatabase.GetAssetPath(_instance)})";
        return str;
    }
#endif

    static T _instance;
    public bool IsInstance => _instance == this;
    public bool HasInstance => _instance;
    public static T Instance
    {
        get
        {
            try
            { 
                if (!_instance)
                    LoadOrCreateInstance();
                return _instance;
            }
            catch (Exception ex) { Debug.LogException(ex); }


            return _instance;
        }
    }

    protected virtual void Awake() { }

    public void WakeupInstance() => Awake();
    int i = 0;
    protected virtual void OnEnable()
    {
        if (_instance != null)
        {
            _instance = this as T;
            VerifySingletonEnable();
        }
    }

    static void LoadOrCreateInstance()
    {
        if (_instance)
            return;
        var dirPath = SingletonsResFolder;
        var instances = Resources.LoadAll<T>(dirPath);
        T instance = null;
        if (instances.Length == 0)
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                Directory.CreateDirectory(AssetsSingletonsResFolder);
                var path = Path.Join(AssetsSingletonsResFolder, typeof(T).Name + ".asset");
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

        _instance = instance;
        _instance.VerifySingletonEnable();
    }
}
