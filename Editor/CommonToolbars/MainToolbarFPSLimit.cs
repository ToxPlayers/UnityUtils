#if UNITY_EDITOR && UNITY_6000_3_OR_NEWER
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Unity.Scripting.LifecycleManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using OnLoadedAttribute =
#if UNITY_6000_5_OR_NEWER
    Unity.Scripting.LifecycleManagement.OnCodeLoadedAttribute;
#else
    UnityEngine.RuntimeInitializeOnLoadMethodAttribute
#endif  

public partial class MainToolbarFPSLimit : MainToolbarCommonBase {  

    [OnLoaded] static void OnCodeLoaded() { 
        EditorApplication.update -= OnUpdate; 
        EditorApplication.update += OnUpdate; 
    } 

    static int _fpsLimitSet = -1;
    static void OnUpdate() {
        if(_fpsLimitSet != Application.targetFrameRate) {
            _fpsLimitSet = Application.targetFrameRate;
            MainToolbar.Refresh(FPSLimitElementName);
        } 
    }

    const string FPSLimitElementName = ToolsDirectory + "FPS Limit";
    [MainToolbarElement(FPSLimitElementName, defaultDockPosition = MainToolbarDockPosition.Middle)]
    static MainToolbarElement CreateTargetFraneRateTool() {
        var content = new MainToolbarContent("FPS Limit", "Controls Application.targetFrameRate");
        var slider = new MainToolbarSlider(content, _fpsLimitSet = Application.targetFrameRate, -1, 120, 
            (i) => Application.targetFrameRate = _fpsLimitSet = Mathf.RoundToInt(i), true);
        return slider;
    }
}

#endif
