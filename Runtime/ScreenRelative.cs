using Sirenix.OdinInspector;
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
    [BoxGroup("Scaling")]
    public bool UseScaling;
    [BoxGroup("Scaling"), ShowIf(nameof(UseScaling))]
    public float Scaling = 1f, MinScaling = 0f, MaxScaling = 10_000;

    [BoxGroup("LookAt")]
    public bool LookAtCam;
    [BoxGroup("LookAt"), ShowIf(nameof(LookAtCam))]
    public SnapAxis LookAtAxis = SnapAxis.Y;
    [BoxGroup("LookAt"), ShowIf(nameof(LookAtCam))]
    public Quaternion LookAtOffset;


    [BoxGroup("Conform To Screen")]
    public bool ConformToScreenUsingParent;
    [BoxGroup("Conform To Screen"), ShowIf(nameof(ConformToScreenUsingParent))]
    public float ConformZExtent, ScreenConformExtentsOffset;


    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += UpdateIcon;
    }
    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= UpdateIcon;
        StopAllCoroutines();
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

    void UpdateIcon(ScriptableRenderContext _, Camera cam)
    {
#if UNITY_EDITOR
        bool isEditorCam = IsEditorCamera(cam);
        if (Application.isPlaying && isEditorCam)
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
        {
            var camPos = cam.transform.position;
            var lookEuler = Quaternion.LookRotation(camPos).eulerAngles;
            
            if (LookAtAxis != SnapAxis.All)
            {
                var originalEuler = transform.eulerAngles;
                var mask = LookAtAxis.AsInt(); 
                for (int i = 0; i < 3; i++)
                    if (!mask.IsSetBitFlags(i.ToFlagsMask()))
                        lookEuler[i] = originalEuler[i];
            }

            transform.rotation = LookAtOffset * Quaternion.Euler(lookEuler); 
        }

        if(UseScaling)
        {
            var distance = Vector3.Distance(cam.transform.position, transform.position);
            var scale = distance * Scaling / 15f * GlobalScaling;
            scale = Mathf.Clamp(scale, MinScaling, MaxScaling);
            var size = Vector3.one * scale;
            transform.localScale = size;
        }
    }


}
