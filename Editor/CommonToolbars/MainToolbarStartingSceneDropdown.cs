#if UNITY_6000_3_OR_NEWER
using System.IO;
using Unity.Scripting.LifecycleManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class MainToolbarStartingSceneDropdown : MainToolbarCommonBase {
    const string SceneSelectorElementName = ToolsDirectory + "Scene Selector";
    const string BootSceneSelectorElementName = ToolsDirectory + "Boot Scene";

    static string[] scenePaths;
     

    [MainToolbarElement(SceneSelectorElementName, defaultDockPosition = MainToolbarDockPosition.Middle)]
    public static MainToolbarElement CreateSceneSelectorDropdown() {
        string activeSceneName = SceneManager.GetActiveScene().name;
        if (activeSceneName == null || activeSceneName.Length == 0)
            activeSceneName = "Untitled";
        var icon = EditorGUIUtility.IconContent("UnityLogo").image as Texture2D;
        var content = new MainToolbarContent(activeSceneName, icon, "Select active scene");
        return new MainToolbarDropdown(content,  (rect) => ShowDropdownMenu(rect, false));
    }

    const string NoBootSceneLabel = "[Current Scene]";
    const string EditorPrefBootSceneKey = "MAIN_TOOL_BAR_PREF_BOOT_SCENE";
    [MainToolbarElement(BootSceneSelectorElementName, defaultDockPosition = MainToolbarDockPosition.Left)]
    public static MainToolbarElement CreateSceneSelectorDropdownForPlayMode() {
        var bootScene = EditorSceneManager.playModeStartScene;
        var bootSceneName = (bootScene ? bootScene.name : NoBootSceneLabel);
        var icon = EditorGUIUtility.IconContent("d_PlayButton").image as Texture2D;
        var content = new MainToolbarContent(bootSceneName, icon, "Select playMode scene");
        var bootDropdown = new MainToolbarDropdown(content,  (rect) => ShowDropdownMenu(rect, true));
        bootDropdown.displayed = !Application.isPlaying;
        return bootDropdown;
    }

    static void ShowDropdownMenu(Rect dropDownRect, bool isSetPlaymode) {
        var menu = new GenericMenu();
        if (scenePaths.Length == 0) {
            menu.AddDisabledItem(new GUIContent("No Scenes in Project"));
        }
        var currentScenePath = isSetPlaymode 
            ? (EditorSceneManager.playModeStartScene ? AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene) : null) :
                                                       SceneManager.GetActiveScene().path;

        if (isSetPlaymode) {
            menu.AddItem(new GUIContent(NoBootSceneLabel), currentScenePath == null, () => SetPlaymodeScene(null));
            menu.AddSeparator("");
        }

        foreach (string scenePath in scenePaths) {
            string sceneName = Path.GetFileNameWithoutExtension(scenePath);
            menu.AddItem(new GUIContent(sceneName), currentScenePath == scenePath, 
                () => { if(isSetPlaymode) { SetPlaymodeScene(scenePath); } else { SwitchScene(scenePath); } });
        }

        menu.DropDown(dropDownRect);
    }

    static void SaveBootScenePreference(string scenePath) {
        scenePath ??= ""; 
        EditorPrefs.SetString(EditorPrefBootSceneKey, scenePath);
    }

    static void SetPlaymodeScene(string scenePath, bool refresh = true) {
        EditorSceneManager.playModeStartScene = string.IsNullOrEmpty(scenePath) ? null :
                                                AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
        if (refresh) {
            SaveBootScenePreference(scenePath);
            MainToolbar.Refresh(BootSceneSelectorElementName);
        }
    }

    static void SwitchScene(string scenePath) {
        if (Application.isPlaying) {
            string sceneName = Path.GetFileNameWithoutExtension(scenePath);
            if (Application.CanStreamedLevelBeLoaded(sceneName)) {
                SceneManager.LoadScene(sceneName);
            } else {
                Debug.LogError($"Scene '{sceneName}' is not in the Build Settings.");
            }
        } else {
            if (File.Exists(scenePath)) {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
                    EditorSceneManager.OpenScene(scenePath);
                }
            } else {
                Debug.LogError($"Scene at path '{scenePath}' does not exist.");
            }
        }
    }

    static void RefreshSceneList() {
        scenePaths = Directory.GetFiles("Assets", "*.unity", SearchOption.AllDirectories);
    }

    static void SceneSwitched(Scene oldScene, Scene newScene) {
        MainToolbar.Refresh(SceneSelectorElementName);
    }

    static MainToolbarStartingSceneDropdown() {
        RefreshSceneList();
        EditorApplication.projectChanged += RefreshSceneList;
        SceneManager.activeSceneChanged += SceneSwitched;
        EditorSceneManager.activeSceneChangedInEditMode += SceneSwitched;
        EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;

    }

    [OnCodeInitializing] static void OnCodeLoaded() {
        if (EditorPrefs.HasKey(EditorPrefBootSceneKey)) {
            SetPlaymodeScene(EditorPrefs.GetString(EditorPrefBootSceneKey), false);
        }
    }

    private static void EditorApplication_playModeStateChanged(PlayModeStateChange obj) { 
        MainToolbar.Refresh(BootSceneSelectorElementName); 
    }
}



#endif