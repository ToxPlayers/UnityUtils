#if UNITY_EDITOR 
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[InitializeOnLoad]
static public class LayersCodeGenerator
{
    static LayersCodeGenerator () { }
    static private readonly string LayersFileName = @"Layers.cs";

    static readonly string NameReplacer = "<%name%>";
    static readonly string ValueReplacer = "<%value%>";
    static readonly string ClassTemplate =
@"static public class Layers
{
    static public bool ContainsLayer(int mask, int layer)
    {
        return ( mask & (1 << layer)) != 0;
    }
}
";
    static readonly string LayerIndexTemplate =
$@" 
    public const int {NameReplacer} = {ValueReplacer};
    public const int {NameReplacer}Mask = 1 << {ValueReplacer};

";


    [MenuItem("Tools/Generate Layers Constants")]
    private static void Generate()
    {
        var filePath = GetFilePath();
        if (string.IsNullOrEmpty(filePath))
        {
            filePath = Application.dataPath + $"/{LayersFileName}";
            File.WriteAllText(filePath, "");
        } 
        var content = File.Exists(filePath) ? File.ReadAllText(filePath) : null;
        var genContent = GenerateCode();
        if (content != genContent)
        {
            File.WriteAllText(filePath, genContent);
            var relativePath =
                filePath.TrimStart(Application.dataPath.TrimEnd("Assets".ToCharArray()).ToCharArray());
            AssetDatabase.ImportAsset(relativePath, ImportAssetOptions.ForceUpdate);
        }
        GC.Collect();
    }

    static string GetFilePath()
    {
        var assets = AssetDatabase.FindAssets("t:script Layers");
        foreach (var asset in assets)
        {
            var path = AssetDatabase.GUIDToAssetPath(asset);
            if (Path.GetFileName(path) == LayersFileName)
                return path;
        }
        return Application.dataPath + $"/{LayersFileName}";
    }


    static string GenerateCode()
    {
        var fieldsCode = "";
        List<string> layersAdded = new List<string>();
        for (int i = 0; i < 32; i++)
        {
            var layerName = InternalEditorUtility.GetLayerName(i);
            if (!string.IsNullOrEmpty(layerName))
            {
                layerName = layerName.Replace(" ", "_");
                if (layersAdded.Contains(layerName))
                {
                    Debug.LogError($"Multiple layers with the same name. ({layerName})");
                    continue;
                }

                layersAdded.Add(layerName);
                var layerCode = LayerIndexTemplate
                 .Replace(NameReplacer, layerName).Replace(ValueReplacer, i.ToString());
                fieldsCode += layerCode;
            }
        } 
        var index = ClassTemplate.Length - 3;
        return ClassTemplate.Insert(index, fieldsCode);
    } 

}
#endif
