#if UNITY_EDITOR && UNITY_6000_3_OR_NEWER
using UnityEditor; 

[InitializeOnLoad]
public class MainToolbarCommonBase {
    public const string ToolsDirectory = "Common/";
}

#endif