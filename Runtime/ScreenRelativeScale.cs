using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class ScreenRelativeScale : MonoBehaviour
{
    static Bounds ViewportBounds = new Bounds()
    {
        min = Vector3.zero,
        max = Vector3.one
    };

    static public float GlobalScaling = 0.7f;
    public const float MinScaling = 0.1f;
    public const float MaxScaling = 2f;
    public float Scaling = 1f;
    public float _minScaling = 0f;
    public float _maxScaling = float.MaxValue;
    public bool ConformToScreenUsingParent;
    public float ConformZExtent;
    public bool LookAtCam;
    public float ScreenConformExtentsOffset;
    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += Rescale;
    }
    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= Rescale;
    }

#if UNITY_EDITOR
    bool IsEditorCamera(Camera cam)
    {
        var cams = UnityEditor.SceneView.GetAllSceneCameras();
        foreach(var editorCam in cams)
            if(editorCam == cam)
                return true;
        return false;
    }
#endif

    void Rescale(ScriptableRenderContext _, Camera cam)
    {
#if UNITY_EDITOR
        if (Application.isPlaying && IsEditorCamera(cam))
            return; 
#endif
        if (ConformToScreenUsingParent)
        {
            transform.localPosition = new Vector3();
            var viewPort = cam.WorldToViewportPoint(transform.position);

            ViewportBounds.SetMinMax(
                new Vector3(ScreenConformExtentsOffset, ScreenConformExtentsOffset, cam.nearClipPlane),
                new Vector3(1f - ScreenConformExtentsOffset, 1f - ScreenConformExtentsOffset, cam.farClipPlane));

            if (!ViewportBounds.Contains(viewPort))
            {
                viewPort = ViewportBounds.ClosestPoint(viewPort);
                transform.position = cam.ViewportToWorldPoint(viewPort);
            }
        }

        if (LookAtCam)
            transform.LookAt(cam.transform);

        var scale = Vector3.Distance(cam.transform.position, transform.position) * Scaling / 15f * GlobalScaling;
        scale = Mathf.Clamp( scale , _minScaling, _maxScaling);
        var size = Vector3.one * scale;
        transform.localScale = size;
    }

}
