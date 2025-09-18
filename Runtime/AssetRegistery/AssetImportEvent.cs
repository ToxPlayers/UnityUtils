#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Events;

public class AssetImportEvent : AssetPostprocessor
{
    public struct Data
    {
        public string[] importedAssets, deletedAssets, movedAssets, movedFromAssetPaths;
    }

    static public readonly UnityEvent<Data> OnImport = new();
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
        string[] movedAssets,  string[] movedFromAssetPaths)
    {
        OnImport.Invoke(new Data { importedAssets = importedAssets, deletedAssets = deletedAssets, 
            movedAssets = movedAssets, movedFromAssetPaths = movedFromAssetPaths });
    }
}
#endif