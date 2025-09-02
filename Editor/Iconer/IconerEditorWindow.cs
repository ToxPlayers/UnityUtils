#if ODIN_INSPECTOR && UNITY_EDITOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace EditorIconer
{
    public partial class IconerEditorWindow : OdinMenuEditorWindow
    {
        [ContextMenu("Create Icon")]
        [MenuItem("Tools/Iconer")]
        static void Open()
        {
            var iconer = GetWindow<IconerEditorWindow>();
            if (!iconer)
                iconer = CreateWindow<IconerEditorWindow>();
            iconer.Show();
            iconer.Focus();
        }

        private void OnProjectChange()
        {
            ForceMenuTreeRebuild();
        }
        const string IsMenuFlattenPrefKey = "KEY_ICONER_IsMenuFlatten";
        bool isMenuFlatten;
        protected override void OnEnable()
        {
            isMenuFlatten = EditorPrefs.GetBool(IsMenuFlattenPrefKey, false);
            base.OnEnable();
        }
        protected override void DrawMenu()
        {
            if (SirenixEditorGUI.Button(isMenuFlatten ? "UnFlatten" : "Flatten", ButtonSizes.Medium))
            {
                isMenuFlatten = !isMenuFlatten;
                EditorPrefs.SetBool(IsMenuFlattenPrefKey, isMenuFlatten);
                ForceMenuTreeRebuild();
            }
            base.DrawMenu();
        }
        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree();
            tree.AddAllAssetsAtPath("", "", typeof(GameObject), true, isMenuFlatten);

            foreach (var item in tree.MenuItems)
            {
                if (item.Value == null)
                    item.AddIcon(SdfIconType.Folder);
                else
                    item.AddThumbnailIcon(true);
                var prefab = item.Value as GameObject;
                item.Value = new IconerScene(AssetDatabase.GetAssetPath(prefab), prefab);
            }

            DrawMenuSearchBar = true;
            tree.Selection.SupportsMultiSelect = true;
            tree.Selection.SelectionChanged += Selection_SelectionChanged;
            return tree;
        }

        private void Selection_SelectionChanged(SelectionChangedType obj)
        {
            _cachedSelection.Clear();

            if (MenuTree.Selection.Count == 1 && MenuTree.Selection[0].Value == null)
            {
                foreach (var child in MenuTree.Selection[0].GetChildMenuItemsRecursive(false))
                    if (child.Value is IconerScene iconer)
                        _cachedSelection.Add(iconer);
            }
            else foreach (var sel in MenuTree.Selection)
                {
                    if (sel.Value is IconerScene iconer)
                        _cachedSelection.Add(iconer);
                }
            Selection.objects = _cachedSelection.Select(s => s.Prefab).ToArray();
        } 
        public const string CamSettingsCollectionName = "CamSettingsCollection";

        CamSettingsCollection _camSettingCollection;
        CamSettingsScriptable _camSettingsScriptable;
        CamSettings Settings => _camSettingCollection.Current;
        CamSettingsScriptable _camSettings
        {
            get
            {
                if(!_camSettingsScriptable)
                    _camSettingsScriptable = CreateInstance<CamSettingsScriptable>();
                if(_camSettingsScriptable.Settings != _camSettingCollection.Current)
                    _camSettingsScriptable.Settings = _camSettingCollection.Current;
                return _camSettingsScriptable;
            }
        }

        PropertyTree _camSettingsEditor;
        const string default_settings_key = "_ICONER_EDITOR_WIN_SETT";

        void ValidateCamSettings()
        {
            if (!_camSettingCollection)
                _camSettingCollection = Resources.Load<CamSettingsCollection>(CamSettingsCollectionName); 
        }

        void SaveCamSettings()
        {
            EditorUtility.SetDirty(_camSettingCollection);
            AssetDatabase.SaveAssetIfDirty(_camSettingCollection);
        }

        List<IconerScene> _cachedSelection = new();

        void DrawCamSettingsDropdown()
        {
            EditorGUILayout.BeginHorizontal();
            var settings = Settings;

            EditorGUILayout.LabelField("Preset");
            if(EditorGUILayout.DropdownButton(new GUIContent("Preset") , FocusType.Passive))
            {

            }
            SirenixEditorGUI.Button("+", ButtonSizes.Medium);
            EditorGUILayout.EndHorizontal();

        }
        bool _undoCamSettClicked;
        void DrawSettings()
        {
            DrawCamSettingsDropdown();
            SirenixEditorGUI.BeginBox("Framing", false);
            var settings = _camSettings.Settings;


            try
            {
                if (_camSettingsEditor == null)
                {
                    _camSettingsEditor = PropertyTree.Create(_camSettings);
                    _camSettingsEditor.OnUndoRedoPerformed += () => _undoCamSettClicked = true;
                }
                EditorGUI.BeginChangeCheck();
                _camSettingsEditor.Draw();
                if (EditorGUI.EndChangeCheck() || _undoCamSettClicked)
                {
                    _undoCamSettClicked = false;
                    _camSettings.Settings.ValidateRes(_camSettings.Settings.TextureResoulation);
                    SaveCamSettings();
                    settings.UpdatedSettingsCount++;
                }
            }
            catch (Exception ex) { Debug.LogException(ex); }
            SirenixEditorGUI.EndBox();
        }
        List<Awaitable> _generationAwaits = new();
        protected override void OnBeginDrawEditors()
        {
            base.OnBeginDrawEditors();

            if (_generationAwaits.Count > 0)
            {
                var countAwaitsOver = 0;
                for (int gen = 0; gen < _generationAwaits.Count;)
                {
                    if (_generationAwaits[gen].IsCompleted)
                    {
                        _generationAwaits.RemoveAt(gen);
                        countAwaitsOver++;
                    }
                    else gen++;
                }
                SirenixEditorFields.ProgressBarField("Generating...", countAwaitsOver, 0, _generationAwaits.Count);
                return;
            }

            ValidateCamSettings();
            DrawSettings();

            SirenixEditorGUI.BeginBox("Previews");
            var generate = SirenixEditorGUI.Button("Generate Icons", ButtonSizes.Medium);

            var settings = _camSettings.Settings;
            int colCount = (int)Math.Ceiling(Math.Sqrt(_cachedSelection.Count));
            GUILayout.BeginHorizontal();
            var scaling = settings.TextureResoulation.ToFloat() * settings.PreviewSize;
            var widthInt = Mathf.RoundToInt(scaling.x);
            var width = GUILayout.Width(widthInt);
            var height = GUILayout.Height(Mathf.RoundToInt(scaling.y));
            int i = 0;

            for (; i < _cachedSelection.Count; i++)
            {
                if (i % colCount == 0)
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }

                var iconer = _cachedSelection[i];
                iconer.RenderWithCamSettings(settings);

                var align = SirenixGUIStyles.BoxContainer.alignment;
                SirenixGUIStyles.BoxContainer.alignment = TextAnchor.UpperLeft;
                SirenixGUIStyles.Label.alignment = TextAnchor.UpperLeft;
                SirenixGUIStyles.None.alignment = TextAnchor.UpperLeft;

                SirenixEditorGUI.BeginBox(iconer.GetFileName(settings), true, GUILayout.Width(widthInt));
                var rect = EditorGUILayout.GetControlRect(width, height);
                SirenixEditorFields.PreviewObjectField(rect, iconer.CamTex,
                    false, false, false, false);

                if (generate)
                    _generationAwaits.Add(iconer.Generate(settings));
                SirenixEditorGUI.EndBox();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            SirenixEditorGUI.EndBox();
        }

        protected override void OnDisable()
        {
            foreach (var item in MenuTree.MenuItems)
            {
                if (item.Value is IconerScene iconer)
                    iconer.Dispose();
            }
            _camSettingsEditor.Dispose();
            base.OnDisable();
        }

        public class DrawableIconerTable
        {
            [TableList(AlwaysExpanded = true)] public List<IconerScene> Icons;
        }
        public class IconerScene : IDisposable
        {
            string _prefabPath;
            [NonSerialized, HideInInspector] public GameObject Prefab;
            GameObject _camParent;
            Camera _cam;
            [NonSerialized, HideInInspector] public RenderTexture CamTex;
            Scene _scene;
            bool _isSceneOpen;
            int _lastSettingsId = -1;
            public IconerScene(string prefabPath, GameObject prefab)
            {
                _prefabPath = prefabPath;
                Prefab = prefab;
            }

            void UpdateCamSettings(CamSettings settings)
            {
                try
                {
                    if (!_isSceneOpen)
                    {
                        _isSceneOpen = true;
                        _scene = EditorSceneManager.NewPreviewScene();

                        PrefabUtility.InstantiatePrefab(Prefab, _scene);

                        _camParent = new GameObject();
                        _cam = new GameObject().AddComponent<Camera>();
                        _cam.enabled = false;
                        _cam.cameraType = CameraType.Preview;
                        _cam.clearFlags = CameraClearFlags.SolidColor;
                        _cam.forceIntoRenderTexture = true;
                        _cam.backgroundColor = new(0, 0, 0, 0);
                        _cam.transform.parent = _camParent.transform;
                        _cam.scene = _scene;
                        SceneManager.MoveGameObjectToScene(_camParent, _scene);
                    }
                    _lastSettingsId = settings.UpdatedSettingsCount;
                    if (CamTex == null || !CamTex.IsCreated() ||
                        settings.TextureResoulation.x != CamTex.width || settings.TextureResoulation.y != CamTex.height)
                        CamTex = new RenderTexture(settings.TextureResoulation.x, settings.TextureResoulation.y, 1);
                    _cam.targetTexture = CamTex;
                    var urp = _cam.GetUniversalAdditionalCameraData();
                    urp.renderPostProcessing = settings.PostProcessing;
                    urp.renderShadows = settings.RenderShadows;
                    _cam.orthographic = settings.Orthographic;
                    if (settings.Orthographic)
                        _cam.orthographicSize = settings.OrthoSize;
                    else
                        _cam.fieldOfView = settings.FOV;
                    _cam.nearClipPlane = 0.01f;
                    _cam.farClipPlane = 5000;
                    _cam.transform.localPosition = Vector3.forward * -settings.Distance;
                    _camParent.transform.localEulerAngles = settings.OrbitalRotation;
                    _camParent.transform.localPosition = settings.FrameOffset;
                    _cam.cullingMask = settings.CullingMask;

                }
                catch (Exception ex) { Debug.LogException(ex); }
            }

            public void RenderWithCamSettings(CamSettings settings)
            {
                if (settings.UpdatedSettingsCount != _lastSettingsId)
                {
                    UpdateCamSettings(settings);
                    _cam.Render();
                }
            }

            internal string GetFileName(CamSettings settings)
            {
                var prefabName = Path.GetFileNameWithoutExtension(_prefabPath);
                return settings.OutputFileRegex.Replace("$", prefabName);
            }
            string GetOutputPath(CamSettings settings)
            {
                var folder = settings.OutputToSameFolder ?
                    Path.GetDirectoryName(_prefabPath) : settings.OutputFolder;
                var fileName = GetFileName(settings);
                fileName += ".png";
                var fullFilePath = Path.Join(folder, fileName);
                return fullFilePath;
            }
            public async Awaitable Generate(CamSettings settings)
            {
                try
                {
                    var width = CamTex.width;
                    var height = CamTex.height;
                    var format = GraphicsFormatUtility.GetGraphicsFormat(CamTex.format, false); ;
                    var buffer = new NativeArray<byte>(width * height * 4, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                    var req = await AsyncGPUReadback.RequestIntoNativeArrayAsync(ref buffer, CamTex, 0);
                    if (req.hasError)
                    {
                        Debug.LogError("GPU Readback Error outputting icon from " + _prefabPath);
                        return;
                    }
                    await Awaitable.BackgroundThreadAsync();
                    var encode = ImageConversion.EncodeNativeArrayToPNG(buffer, format, (uint)width, (uint)height);
                    var outputPath = GetOutputPath(settings);
                    await File.WriteAllBytesAsync(outputPath, encode.ToArray());
                    encode.Dispose();
                    await Awaitable.MainThreadAsync();
                    AssetDatabase.ImportAsset(outputPath);
                    TextureImporter textureImporter = AssetImporter.GetAtPath(outputPath) as TextureImporter;
                    textureImporter.textureType = TextureImporterType.Sprite;
                    textureImporter.spriteImportMode = SpriteImportMode.Single;
                    textureImporter.SaveAndReimport();
                    EditorGUIUtility.PingObject(textureImporter.GetInstanceID());
                }
                catch (Exception ex) { Debug.LogException(ex); }
            }

            public void Dispose()
            {
                if (_isSceneOpen)
                {
                    EditorSceneManager.ClosePreviewScene(_scene);
                    _isSceneOpen = false;
                }
                if (_cam)
                {
                    _cam.forceIntoRenderTexture = false;
                    _cam.targetTexture = null;
                    _cam.SafeDestroy();
                }
                if (CamTex)
                    CamTex.Release();
            }

        }
    }
#endif
}
