#if ODIN_INSPECTOR && UNITY_EDITOR
using Microsoft.SqlServer.Server;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using Sirenix.Utilities.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using static IconerEditorWindow;
public class IconerEditorWindow : OdinMenuEditorWindow
{
    [ContextMenu("Create Icon")]
    [MenuItem("Tools/Iconer")]
    static void Open()
    {
        var iconer = GetWindow<IconerEditorWindow>();
        if(!iconer)
            iconer = CreateWindow<IconerEditorWindow>();
        iconer.Show();
        iconer.Focus();
    }

    private void OnProjectChange()
    { 
        ForceMenuTreeRebuild();
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree();

        var guids = AssetDatabase.FindAssets("a:assets t:prefab");
        foreach(var guid in guids)
        {
            var asset = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(asset);
            var itemName = asset.TrimStart("Assets/".ToCharArray()).TrimEnd(".prefab");
            var item = tree.Add(itemName, new IconerScene(asset, prefab));
            item.AddIcon(PrefabUtility.GetIconForGameObject(prefab));
        }

        foreach(var item in tree.MenuItems)
        {
            if (item.Value == null)
            {
                item.IsSelectable = false;
                item.AddIcon(SdfIconType.Folder);
            }
        }
        this.DrawMenuSearchBar = true; 
        tree.Selection.SupportsMultiSelect = true;
        return tree;
    }

    [Serializable] public class CamSettingsScriptable : ScriptableObject
    {
        [HideLabel, HideReferenceObjectPicker] public CamSettings Settings;
    }
    [Serializable] public class CamSettings
    {
        public Vector2Int TextureResoulation = new(512,512);
        public float Distance = 3f;
        public Vector3 OrbitalRotation = new(17f, 45f,0), 
            FrameOffset;
        [InfoBox("Use $ for object name", InfoMessageType = InfoMessageType.None)]
        public string OutputFileRegex = "$_icon";
        public bool OutputToSameFolder = true;
        [FolderPath, HideIf(nameof(OutputToSameFolder))] 
        public string OutputFolder;

        [NonSerialized] public int UpdatedSettingsCount;
    }

    CamSettingsScriptable _camSettings;
    PropertyTree _camSettingsEditor;
    const string settings_key = "_ICONER_EDITOR_WIN_SETT";

    void ValidateCamSettings()
    { 
        if (_camSettings == null)
        {
            _camSettings = CreateInstance<CamSettingsScriptable>();
            try
            {
                if (EditorPrefs.HasKey(settings_key))
                {
                    var json = EditorPrefs.GetString(settings_key);
                    _camSettings.Settings = JsonUtility.FromJson<CamSettings>(json);
                }
            } 
            catch(Exception ex) 
            {
                Debug.LogError(ex);
                _camSettings = CreateInstance<CamSettingsScriptable>();
                _camSettings.Settings = new();
                SaveCamSettings();
            }
        }
    }

    void SaveCamSettings()
    {
        var json = JsonUtility.ToJson(_camSettings);
        EditorPrefs.SetString(settings_key, json);
    }

    IEnumerable<IconerScene> SelectedIconers
    {
        get
        {
            foreach (var sel in MenuTree.Selection)
            {
                if (sel.Value is IconerScene iconer)
                {
                    yield return iconer;
                }
            }
        }
    }

    void DrawSettings()
    {
        SirenixEditorGUI.BeginIndentedHorizontal();
        SirenixEditorGUI.BeginBox("Framing");
        var settings = _camSettings.Settings;
        try
        {
            _camSettingsEditor ??= PropertyTree.Create(_camSettings);
            EditorGUI.BeginChangeCheck();
            _camSettingsEditor.Draw();
            if (EditorGUI.EndChangeCheck())
            {
                SaveCamSettings();
                settings.UpdatedSettingsCount++;
            }
        }
        catch (Exception ex) { Debug.LogException(ex); }
        SirenixEditorGUI.EndBox();
        SirenixEditorGUI.EndIndentedHorizontal();
    }
    List<Awaitable> _generationAwaits = new();
    protected override void OnBeginDrawEditors()
    {
        base.OnBeginDrawEditors();

        if (_generationAwaits.Count > 0)
        {
            var countAwaitsOver = 0;
            foreach (var await in _generationAwaits)
                if (await.IsCompleted)
                    countAwaitsOver++;
            SirenixEditorFields.ProgressBarField("Generating...", countAwaitsOver, 0, _generationAwaits.Count); 
            return;
        } 

        ValidateCamSettings();
        DrawSettings();

        var selection = MenuTree.Selection; 
        SirenixEditorGUI.BeginBox("Output");
        var generate = SirenixEditorGUI.Button("Generate Icon", ButtonSizes.Medium);
        List<Awaitable> awaits = new();
        foreach (var iconer in SelectedIconers)
        {
            iconer.RenderWithCamSettings(_camSettings.Settings);
            var res = EditorGUILayout.GetControlRect(GUILayout.Width(iconer.CamTex.width), GUILayout.Height(iconer.CamTex.height));
            SirenixEditorFields.PreviewObjectField(res, iconer.CamTex, false, false, false, false);
             
            if (generate)
                awaits.Add(iconer.Generate(_camSettings.Settings));
        }
        SirenixEditorGUI.EndBox(); 
    }

    protected override void OnDestroy()
    {
        foreach(var item in MenuTree.MenuItems)
        {
            if (item.Value is IconerScene iconer)
                iconer.Dispose();
        }
        _camSettingsEditor.Dispose();
        base.OnDestroy();
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
                    PrefabUtility.LoadPrefabContentsIntoPreviewScene(_prefabPath, _scene);

                    CamTex = new RenderTexture(settings.TextureResoulation.x, settings.TextureResoulation.y, 1);
                    _camParent = new GameObject();
                    _cam = new GameObject().AddComponent<Camera>();
                    _cam.cameraType = CameraType.Preview;
                    _cam.clearFlags = CameraClearFlags.SolidColor;
                    _cam.forceIntoRenderTexture = true;
                    _cam.targetTexture = CamTex;
                    _cam.backgroundColor = new(0, 0, 0, 0);
                    _cam.transform.parent = _camParent.transform;
                    _cam.scene = _scene;
                    SceneManager.MoveGameObjectToScene(_camParent, _scene); 
                }
                _lastSettingsId = settings.UpdatedSettingsCount;
                _cam.transform.localPosition = Vector3.forward * -settings.Distance;
                _camParent.transform.localEulerAngles = settings.OrbitalRotation;
                _camParent.transform.localPosition = settings.FrameOffset; 
                
            }catch(Exception ex) { Debug.LogException(ex); }
        }

        public void RenderWithCamSettings(CamSettings settings)
        {
            if (settings.UpdatedSettingsCount != _lastSettingsId)
            {
                UpdateCamSettings(settings);
                _cam.Render();
            }     
        }

        string GetOutputPath(CamSettings settings)
        {
            var folder = settings.OutputToSameFolder ?
                Path.GetDirectoryName(_prefabPath) : settings.OutputFolder;
            var prefabName = Path.GetFileNameWithoutExtension(_prefabPath);
            var fileName = settings.OutputFileRegex.Replace("$", prefabName);
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
                var format = GraphicsFormatUtility.GetGraphicsFormat(CamTex.format, false);;
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
            catch(Exception ex) { Debug.LogException(ex); }
        }

        public void Dispose()
        {
            if (_isSceneOpen)
            {
                EditorSceneManager.ClosePreviewScene(_scene);
                _isSceneOpen = false;
            }
            if (CamTex)
                CamTex.Release();
        }
    }
}
#endif
