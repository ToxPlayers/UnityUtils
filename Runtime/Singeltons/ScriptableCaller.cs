using UnityEngine;

public static class ScriptableCaller  
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    static void WakeupAll()
    {
        Debug.Log("??");
        foreach (var wk in Resources.LoadAll<ScriptableSingleton>(ScriptableSingleton.SingletonsResFolder))
            wk.OnAssembliesLoaded();
    }
}
