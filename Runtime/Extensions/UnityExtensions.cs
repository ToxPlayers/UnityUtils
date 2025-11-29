using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine.SceneManagement;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using UObj = UnityEngine.Object;
using System.Linq;
using System.Threading;

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
static public class UnityExtensions
{
    const int INLINE = (int)MethodImplOptions.AggressiveInlining;

    #region GameObject

#if UNITY_EDITOR
    static public bool IsInPrefabStage(this GameObject go)
    {
        var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
        if (stage)
            return stage.IsPartOfPrefabContents(go);
        return false;
    }
#endif

    static public Texture2D ResizeFormat(this Texture2D texture2D, int resoulation, TextureFormat format)
                         => ResizeFormat(texture2D, resoulation, resoulation, format);
    static public Texture2D ResizeFormat(this Texture2D source, int targetWidth, int targetHeight, TextureFormat format)
    {
        Texture2D result = new Texture2D(targetWidth, targetHeight, format, true);
        Color[] rpixels = result.GetPixels(0);
        float incX = (1.0f / targetWidth);
        float incY = (1.0f / targetHeight);
        for (int px = 0; px < rpixels.Length; px++)
            rpixels[px] = source.GetPixelBilinear(incX * ((float)px % targetWidth), incY * ((float)Mathf.Floor(px / targetWidth)));
        result.SetPixels(rpixels, 0);
        result.Apply();
        return result;
    }
    [MethodImpl(INLINE)]
    static public void MatchPositionAndRotation(this Transform dest, in Transform src)
    {
        src.GetPositionAndRotation(out var pos, out var rot);
        dest.SetPositionAndRotation(pos, rot);
    }
    [MethodImpl(INLINE)]
	static public void SetEularX(this Transform tf, float x)
	{
		var eular = tf.eulerAngles;
		eular.x = x;
		tf.eulerAngles = eular;
	}
	[MethodImpl(INLINE)]
	static public void SetEularY(this Transform tf, float y)
	{
		var eular = tf.eulerAngles;
		eular.y = y;
		tf.eulerAngles = eular;

	}
	[MethodImpl(INLINE)]
	static public void SetEularZ(this Transform tf, float z)
	{
		var eular = tf.eulerAngles;
		eular.z = z;
		tf.eulerAngles = eular;
	}
	[MethodImpl(INLINE)]
	static public float GetEularX(this Transform tf) => tf.eulerAngles.x;
	[MethodImpl(INLINE)]
	static public float GetEularY(this Transform tf) => tf.eulerAngles.y;
	[MethodImpl(INLINE)]
	static public float GetEularZ(this Transform tf) => tf.eulerAngles.z;
	[MethodImpl(INLINE)]
	static public void SetPosX(this Transform tf, float x)
	{
		var pos = tf.position;
		pos.x = x;
		tf.position = pos;
	}
	[MethodImpl(INLINE)]
	static public void SetPosY(this Transform tf, float y)
	{
		var pos = tf.position;
		pos.y = y;
		tf.position = pos; 
	}
	[MethodImpl(INLINE)]
	static public void SetPosZ(this Transform tf, float z)
	{
		var pos = tf.position;
		pos.z = z;
		tf.position = pos;
	}
	[MethodImpl(INLINE)]
	static public float GetPosX(this Transform tf) => tf.position.x;
	[MethodImpl(INLINE)]
	static public float GetPosY(this Transform tf) => tf.position.y;
	[MethodImpl(INLINE)]
	static public float GetPosZ(this Transform tf) => tf.position.z;
	static public void MoveRelativeToChild(this Transform parent, Transform child, Transform target)
	{
		var childOriginParent = child.parent;
		var parentOriginParent = parent.parent;
		child.SetParent(null, true);
		parent.SetParent(child, true);
		target.GetPositionAndRotation(out var pos, out var rot);
		child.transform.SetPositionAndRotation(pos,rot);
		parent.SetParent(parentOriginParent, true);
		child.SetParent(childOriginParent, true);
	} 
    [MethodImpl(INLINE)]
    static public Transform AddChild(this Component comp, string name = "GameObject")
    {
        var childtf = new GameObject(name).transform;
        childtf.SetParent(comp.transform);
        return childtf;
    }
    [MethodImpl(INLINE)]
    static public T AddChildInstantiate<T>(this Transform tf, T prefab, string name = null) where T : Component
    { 
        var t = UObj.Instantiate(prefab, tf);
        if(name != null)
            t.gameObject.name = name;
        return t;
    }
    [MethodImpl(INLINE)]
    static public GameObject AddChildInstantiate(this Transform tf, GameObject prefab, string name = null)
    {
        var go = UObj.Instantiate(prefab, tf);
        if (name != null)
            go.name = name;
        return go;
    }
    [MethodImpl(INLINE)]
    static public Transform AddChild(this Transform tf, string name = "New GameObject")
    {
        var child = new GameObject(name).transform;
        child.parent = tf;
        return child;
    }
    #endregion

    #region Components    
    static public float GetHPNormalized(this IHealth hp) => hp.HPValue / hp.MaxHP; 

#if UNITY_EDITOR
    [MenuItem("Tools/Remove Selected Missing Scripts")]
    static public void RemoveSelectedMissingScripts()
    {
        var deeperSelection = Selection.gameObjects.SelectMany(go => go.GetComponentsInChildren<Transform>(true))
            .Select(t => t.gameObject);
        var prefabs = new HashSet<UObj>();
        int compCount = 0;
        int goCount = 0;
        foreach (var go in deeperSelection)
        {
            int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
            if (count > 0)
            {
                if (PrefabUtility.IsPartOfAnyPrefab(go))
                {
                    RecursivePrefabSource(go, prefabs, ref compCount, ref goCount);
                    count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
                    // if count == 0 the missing scripts has been removed from prefabs
                    if (count == 0)
                        continue;
                    // if not the missing scripts must be prefab overrides on this instance
                }

                Undo.RegisterCompleteObjectUndo(go, "Remove missing scripts");
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                compCount += count;
                goCount++;
            }
        }

        Debug.Log($"Found and removed {compCount} missing scripts from {goCount} GameObjects");

        void RecursivePrefabSource(GameObject instance, HashSet<UnityEngine.Object> prefabs, ref int compCount, ref int goCount)
        {
            var source = PrefabUtility.GetCorrespondingObjectFromSource(instance);
            // Only visit if source is valid, and hasn't been visited before
            if (source == null || !prefabs.Add(source))
                return;

            // go deep before removing, to differantiate local overrides from missing in source
            RecursivePrefabSource(source, prefabs, ref compCount, ref goCount);

            int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(source);
            if (count > 0)
            {
                Undo.RegisterCompleteObjectUndo(source, "Remove missing scripts");
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(source);
                compCount += count;
                goCount++;
            }
        }

    } 

#endif
#if UNITY_EDITOR
    [MethodImpl(INLINE)]
    static public void ForceDirtyAndSave(this UnityEngine.Object obj)
    {
        UnityEditor.EditorUtility.SetDirty(obj);
        UnityEditor.AssetDatabase.SaveAssetIfDirty(obj);
    }
#endif
    static public void StaticDisposeHook(Action onDispose)
    {
        Application.quitting += onDispose;
#if UNITY_EDITOR 
        AssemblyReloadEvents.beforeAssemblyReload += () => onDispose.Invoke();
#endif
    } 
   
    static public void StaticDisposeHook(this IDisposable disposable) 
    {  
        Application.quitting += () => TryDispose(disposable);
#if UNITY_EDITOR
        AssemblyReloadEvents.beforeAssemblyReload += () => TryDispose(disposable);
#endif
    }
    [MethodImpl(INLINE)] 
    static public Quaternion DirectionToAsRotation(this Transform tf, Transform target)
    {
        return DirectionToAsRotation(tf, target.position, Vector3.up);
    }
    [MethodImpl(INLINE)]
    static public Quaternion DirectionToAsRotation(this Transform tf, Vector3 position)
    {
        return DirectionToAsRotation(tf, position, Vector3.up);
    }
    [MethodImpl(INLINE)]
    static public Quaternion DirectionToAsRotation(this Transform tf, Vector3 position, Vector3 up)
    {
        return Quaternion.LookRotation(DirectionTo(tf, position), up);
    }
    [MethodImpl(INLINE)]
    static public Vector3 DirectionTo(this Transform tf, Transform target)
    {
        return DirectionTo(tf, target.position);
    }
    [MethodImpl(INLINE)]
    static public Vector3 DirectionTo(this Transform tf, Vector3 to)
    {
        return DirectionTo(tf.position, to);
    }
    [MethodImpl(INLINE)]
    static public Vector3 DirectionTo(this Vector3 start, Vector3 end)
    {
        return (end - start).normalized;
    } 
    [MethodImpl(INLINE)]
    static public Quaternion DirectionToAsRotation(this Vector3 start, Vector3 end)
    {
        return Quaternion.LookRotation((end - start).normalized);
    }
    static void TryDispose(IDisposable disposable)
    {
        try
        { disposable?.Dispose(); }
        catch (Exception ex) { Debug.Log($"Faied to dispose ({disposable})\n{ex.Message}"); }
    }


    [MethodImpl(INLINE)]
    static public float Distance(this Vector3 from, Vector3 to) => Vector3.Distance(from, to);
    [MethodImpl(INLINE)] 
    static public float Distance(this Vector2 from, Vector2 to) => Vector2.Distance(from, to);
    [MethodImpl(INLINE)]
    static public float Distance(this Transform tf, Transform target)
    => Vector3.Distance(tf.position, target.position);
    [MethodImpl(INLINE)]
    static public float Distance(this Transform tf, Vector3 target)
        => Distance(tf.position, target);
    [MethodImpl(INLINE)]
    static public float DistanceSqr(this Transform tf, Transform target)
    => (tf.position - target.position).sqrMagnitude;
    [MethodImpl(INLINE), BurstCompile]
    static public Vector3 ToV3(this float3 f3) => new Vector3(f3.x, f3.y, f3.z); 
    [MethodImpl(INLINE), BurstCompile]
    static public Vector2 ToV2(this float2 f2) => new Vector2(f2.x, f2.y);
    [MethodImpl(INLINE), BurstCompile]
    static public float2 XZFloat(this float3 f3) => new float2(f3.x, f3.z);
    [MethodImpl(INLINE), BurstCompile]
    static public float3 XZToXYZF(this float2 f2, float y) => new float3(f2.x, y,f2.y);
    [MethodImpl(INLINE), BurstCompile]
    static public float3 XZToXYZF(this float2 f2) => new float3(f2.x, 0,f2.y);
    [MethodImpl(INLINE), BurstCompile]
    static public Vector3 XZToXYZV(this float2 f2) => new Vector3(f2.x, 0, f2.y);
    [MethodImpl(INLINE), BurstCompile]
    static public Vector3 XZToXYZV(this float2 f2, float y) => new Vector3(f2.x, y, f2.y);
    [MethodImpl(INLINE), BurstCompile]
    static public string ShortString(this Vector3 v3) => $"({v3.x},{v3.y},{v3.z})";
    [MethodImpl(INLINE), BurstCompile]
    static public string ShortString(this Vector2 v2) => $"({v2.x},{v2.y})";

    [MethodImpl(INLINE)]
    static public T GetOrAddCompnent<T>(this GameObject go) where T : Component
    {
        if (go.TryGetComponent<T>(out var outT))
            return outT;
        return go.gameObject.AddComponent<T>();
    }
    [MethodImpl(INLINE)]
    static public T GetOrAddCompnent<T>(this Component comp) where T : Component
    {
        if (comp.TryGetComponent<T>(out var outT))
            return outT;
        return comp.gameObject.AddComponent<T>();
    }

    [MethodImpl(INLINE)]
    static public T GetOrCreateCompnent<T>(this GameObject go) where T : Component
    {
        if (go.TryGetComponent<T>(out var outT))
            return outT;
        return go.gameObject.AddComponent<T>();
    }

    [MethodImpl(INLINE)]
    static public T AddComponent<T>(this Component comp) where T : Component => comp.gameObject.AddComponent<T>();

    [MethodImpl(INLINE)]
    static public Component AddComponent(this Component comp, Type t) => comp.gameObject.AddComponent(t);

    [MethodImpl(INLINE)]
    public static void SafeDelay(this UnityEngine.Object behave, Action action)
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += () => action();
#else
        action();
#endif
    }

    [MethodImpl(INLINE)]
    static public void SafeDestroy(this Transform tf)
    {
        if (tf && tf.gameObject)
            SafeDestroy(tf.gameObject);
    }
    [MethodImpl(INLINE)]
    static public void SafeDestroy(this UnityEngine.Object obj)
    { 
        if (obj)
        {
#if UNITY_EDITOR
            if (PrefabUtility.IsPartOfAnyPrefab(obj))
                Debug.LogWarning("Can't destroy prefab instance");
#endif

            if (Application.isPlaying)
                UnityEngine.Object.Destroy(obj);
            else
                UnityEngine.Object.DestroyImmediate(obj);
        }
    }
    static public VisualElement Root(this VisualElement element)
    {
        if (element == null)
            throw new NullReferenceException();
        while (element.parent != null)
            element = element.parent;
        return element;
    }
    static public List<T> GetComponentsInFirstChildren<T>(this Component comp) where T : Component
    {
        var tf = comp.transform;
        var childCount = tf.childCount;
        var listT = new List<T>();
        for (int i = 0; i < childCount; i++)
            listT.AddRange(tf.GetChild(i).GetComponents<T>());
        return listT;
    }

    static public IEnumerable<Transform> LoopChildren(this Transform tf)
    {
        var count = tf.childCount;
        for (int i = 0; i < count; i++)
            yield return tf.GetChild(i);
    }
    static public void LoopChildren(this Transform tf, Action<Transform> action)
    {
        var count = tf.childCount;
        for (int i = 0; i < count; i++)
            action.Invoke(tf.GetChild(i));
    }
    static public IEnumerator<Transform> LoopGrandChildren(this Transform tf)
    {
        var count = tf.childCount;
        for (int i = 0; i < count; i++)
        {
            var child = tf.GetChild(i);
            if (child.childCount > 0)
            {
                var grandChildren = LoopGrandChildren(child);
                while (grandChildren.MoveNext())
                    yield return grandChildren.Current;
            }
            yield return tf.GetChild(i);
        }
    }
    static public void LoopGrandChildren(this Transform tf, Action<Transform> action)
    {
        var grandChildren = LoopGrandChildren(tf);
        while (grandChildren.MoveNext())
            action.Invoke(grandChildren.Current);
    }
    #endregion

    #region Scenes 
    static public bool TryGetRootComp<T>(out T res) where T : Component
    {
        res = null;
        var scene = SceneManager.GetActiveScene();
        if (scene.isLoaded)
        {
            var rootGos = scene.GetRootGameObjects();
            foreach (var go in rootGos)
                if (go.TryGetComponent(out res))
                    return true;
        }
        return false;
    }
    #endregion

    #region Events
    static public bool IsRepaint(this Event e) => e != null && e.type == EventType.Repaint;
    static public bool IsMousePrimaryClick(this Event e) => e.isMouse && e.type == EventType.MouseDown && e.button == 0;
    static public bool IsMouseRightClick(this Event e) => e.isMouse && e.type == EventType.MouseDown && e.button == 1;
    static public bool IsMouseMiddleHold(this Event e) => e.isMouse && e.button == 2;
    static public bool IsKey(this Event e, KeyCode k) => e.keyCode == k;
    static public bool IsKeyDown(this Event e) => e.type == EventType.KeyDown;
    static public bool IsKeyUp(this Event e) => e.type == EventType.KeyUp;
    static public bool IsMouseDown(this Event e) => e.type == EventType.MouseDown;
    static public bool MousePosDelta(this Event e) => e.isMouse && e.button == 2;
    static public Vector2 MouseScrollDelta(this Event e) => e.type == EventType.ScrollWheel ? e.delta : new Vector2();
    static public bool GetPanInput(Event e, out Vector2 pan)
    {
        pan = new Vector2();
        if (e.IsKeyDown() && e.IsKey(KeyCode.UpArrow) || e.IsKey(KeyCode.DownArrow) || e.IsKey(KeyCode.RightArrow) || e.IsKey(KeyCode.PageDown))
        {
            var zoomAmount = 15f;
            if (e.IsKey(KeyCode.UpArrow))
                pan.y += zoomAmount;
            if (e.IsKey(KeyCode.DownArrow))
                pan.y -= zoomAmount;
            if (e.IsKey(KeyCode.RightArrow))
                pan.x += zoomAmount;
            if (e.IsKey(KeyCode.LeftArrow))
                pan.x -= zoomAmount;
            return true;
        }
        if (e.isMouse && e.IsMouseMiddleHold() && e.IsMouseDown())
        {
            pan += e.delta;
            e.Use();
            return true;
        }
        return false;
    }
    #endregion

    #region Native

    static Thread _unityThread;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void CaptureUnityThread()
    {
        _unityThread = Thread.CurrentThread;
    }
    public static bool RandomBool => UnityEngine.Random.value < 0.5f;
    static public bool IsOnUnityThread
    {
        get
        {
            if (_unityThread == null)
                return false;
            return _unityThread.ManagedThreadId == Thread.CurrentThread.ManagedThreadId;
        }
    }


    [BurstCompile]
    static public void CopyTo<T>(this ref NativeArray<T> from, ref NativeArray<T> to, int start) where T : unmanaged
    {
        from.CopyTo(to.GetSubArray(start, from.Length - start ));
    }
    [BurstCompile]
    static public void CopyTo<T>(this ref NativeArray<T>.ReadOnly from, NativeArray<T> to, int start) where T : unmanaged
    {
        from.CopyTo(to.GetSubArray(start, from.Length - start));
    } 
    static public void SafeDispose<T>(this ref NativeArray<T> disposable) where T : unmanaged
    { 
        if(disposable.IsCreated)
            disposable.Dispose();
    }



    #endregion
     
}
