#if UNITY_EDITOR 
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[InitializeOnLoad]
static public class LayersCodeGenerator {
    static LayersCodeGenerator() { }
    static private readonly string LayersFileName = @"Layers.cs";

    static readonly string NameReplacer = "<%name%>";
    static readonly string ValueReplacer = "<%value%>";
    static readonly string ClassTemplate =
@"using System.Collections.Immutable;
static public class Layers
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
    static readonly string LayerInfoTemplate = $@"new LayerInfo(""{NameReplacer}"", {ValueReplacer})," + '\n';
    static readonly string AllLayersTemplate =
    $@"    static public readonly ImmutableArray<LayerInfo> AllLayers = ImmutableArray.Create(
          {'\t' + LayerInfoTemplate}
    );" + '\n';

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

    static string SetLayerAndIdx(string template, string name, string value) {
        return template.Replace(NameReplacer, name).Replace(ValueReplacer, value);
    }

    static string GenerateCode()
    {
        var fieldsCode = "";
        List<LayerInfo> layersAdded = new List<LayerInfo>();
        var allLayersFields = "";
        for (int i = 0; i < 32; i++)
        {
            var layerName = InternalEditorUtility.GetLayerName(i);
            if (string.IsNullOrEmpty(layerName))
                continue; 
            layerName = layerName.Replace(" ", "_");
            if (layersAdded.Contains(new LayerInfo(layerName, i)))
            {
                Debug.LogError($"Multiple layers with the same name. ({layerName})");
                continue;
            }

            layersAdded.Add(new LayerInfo(layerName, i));
            var layerCode = SetLayerAndIdx(LayerIndexTemplate, layerName, i.ToString());
            fieldsCode += layerCode;

            allLayersFields += '\n' + SetLayerAndIdx(LayerInfoTemplate, layerName, i.ToString());
        }
        var replaceLastCommaIdx = allLayersFields.LastIndexOf(',');
        allLayersFields = allLayersFields.Remove(replaceLastCommaIdx, 1); 
        var allLayerStr = AllLayersTemplate.Replace(LayerInfoTemplate, allLayersFields);
        fieldsCode += allLayerStr;


        var index = ClassTemplate.Length - 3;
        return ClassTemplate.Insert(index, fieldsCode);
    } 

}
#endif
