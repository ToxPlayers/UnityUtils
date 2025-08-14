using UnityEngine;

static public class ScriptableSingletonWakeup
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    static void Wakeup()
    {
        foreach (var scr in Resources.FindObjectsOfTypeAll<ScriptableSingleton>())
            scr.Wakeup();
    } 
}
