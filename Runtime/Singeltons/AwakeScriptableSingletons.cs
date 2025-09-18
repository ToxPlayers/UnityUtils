using UnityEditor;
using UnityEngine;
class AwakeScriptableSingletons
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    static void ForceAllInstances()
    {
        foreach (var inst in Resources.LoadAll<ScriptableSingleton>(ScriptableSingleton.SingletonsResFolder))
            inst.VerifySingletonEnable();
    }
} 
