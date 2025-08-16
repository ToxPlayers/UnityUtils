#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

[InitializeOnLoad]
static public class QuickGrayscale
{  
    static BlitRenderPass _pass;
    static public bool EnableGrayscale; 
    static QuickGrayscale()
    {
        RenderPipelineManager.beginCameraRendering -= RenderPipelineManager_beginCameraRendering; 
        RenderPipelineManager.beginCameraRendering += RenderPipelineManager_beginCameraRendering;
        EnableGrayscale = EditorPrefs.GetBool(Pref, false); 
    } 
    const string Pref = "Editor_Quick_Grayscale_ON";
    const string MenuPath = "Tools/Quick Grayscale";
    [MenuItem(MenuPath)]
    private static void ToggleGrayscale()
    {
        EnableGrayscale = !EnableGrayscale; 
        EditorPrefs.SetBool(Pref, EnableGrayscale);
        SceneView.RepaintAll();
    }
    [MenuItem(MenuPath, true)]
    private static bool ValidateGrayscale()
    {
        Menu.SetChecked(MenuPath, EnableGrayscale);
        return true;
    }
    private static void RenderPipelineManager_beginCameraRendering(ScriptableRenderContext context, Camera camera)
    {


        if (EnableGrayscale && camera.cameraType == CameraType.Game)
        {
            _pass ??= new("Editor Grayscale", RenderPassEvent.AfterRenderingPostProcessing,
                    new BlitRequest()
                    {
                        Name = "Editor Grayscale",
                        Material = new Material(Shader.Find("Shader Graphs/FullscreenGrayscale")),
                        PassEvent = RenderPassEvent.AfterRenderingPostProcessing
                    });

            var data =
                camera.GetComponent<UniversalAdditionalCameraData>();
            if(data)
                data.scriptableRenderer.EnqueuePass(_pass);
        }
    } 
     
}
#endif