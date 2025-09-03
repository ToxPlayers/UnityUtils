#if ODIN_INSPECTOR && UNITY_EDITOR
using Sirenix.OdinInspector;
using System;
using UnityEngine;
namespace EditorIconer
{

    [Serializable]
    public class CamSettingsScriptable : ScriptableObject
    {
        [HideLabel, HideReferenceObjectPicker] public CamSettings Settings;
    }

    [Serializable]
    public class CamSettings
    {
        [BoxGroup("A/Camera Settings")]
        public string CamSettingName;
        [BoxGroup("A/Camera Settings/Framing")]
        public Vector3 OrbitalSphere = new(17f, 45f, 0),
    CamPosition = new Vector3(0, 0, 2f), CamRotation, ObjectPosition, ObjectRotation;
        [HorizontalGroup("A/Camera Settings/Framing/fov")]
        public bool Orthographic;
        [HorizontalGroup("A/Camera Settings/Framing/fov")]
        [ShowIf(nameof(Orthographic))] public float OrthoSize = 5;
        [HorizontalGroup("A/Camera Settings/Framing/fov")]
        [HideIf(nameof(Orthographic))] public float FOV = 60;
        [HorizontalGroup("A")]
        [BoxGroup("A/Camera Settings")]
        public LayerMask CullingMask = Physics.AllLayers;
        [BoxGroup("A/Camera Settings")]
        public bool PostProcessing, RenderShadows;
        [BoxGroup("A/Camera Settings")] 
        [OnValueChanged(nameof(ValidateRes))]
        [BoxGroup("A/Output")]
        public Vector2Int TextureResoulation = new(512, 512);
        [Range(0.1f, 2f)]
        [BoxGroup("A/Output")]
        public float PreviewSize = 0.5f;
        [BoxGroup("A/Output")]
        [InfoBox("Use $ for object name", InfoMessageType = InfoMessageType.None)]
        public string OutputFileRegex = "$_icon";
        [BoxGroup("A/Output")] public bool OutputToSameFolder = true;
        [FolderPath, HideIf(nameof(OutputToSameFolder))]
        [BoxGroup("A/Output")] public string OutputFolder;

        [NonSerialized] public int UpdatedSettingsCount;
        const int MAX_RES = 2048;

        public bool ValidateRes(Vector2Int res)
        {
            var isValid = res.x <= MAX_RES && res.y <= MAX_RES;
            TextureResoulation.x = Mathf.Clamp(res.x, 24, MAX_RES);
            TextureResoulation.y = Mathf.Clamp(res.y, 24, MAX_RES);
            return isValid;
        }

    }
}
#endif
