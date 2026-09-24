using UnityEditor;
using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using Unity.Scripting.LifecycleManagement;
#endif 
class AwakeScriptableSingletons
{
#if UNITY_6000_5_OR_NEWER
    [OnCodeInitializing]
#else  
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#endif 
    static void ForceAllInstances()
    {
        foreach (var inst in Resources.LoadAll<ScriptableSingleton>(ScriptableSingleton.SingletonsResFolder))
            inst.VerifySingletonEnable();
    }
} 
