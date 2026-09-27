using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SearchService;
using UnityEngine;
using UObj = UnityEngine.Object;
namespace Files
{
    public class AssetNamedBasedRegisteryJsonConverter<TAsset> : AssetRegisteryJsonConverter<string, TAsset> where TAsset : UObj { }
    public class AssetRegisteryJsonConverter<TKey, TAsset> : JsonConverter<TAsset> where TAsset : UObj 
    {
        static public AssetRegistryBase<TKey, TAsset> AssetRegistry;
        

        bool ValidateRegistry() {
            if (!AssetRegistry) {
                AssetRegistry = AssetRegistryBase<TKey, TAsset>.Instance;
                if (!AssetRegistry) {
                    Debug.LogError("No instance of " + typeof(AssetRegistryBase<TKey, TAsset>).Name + " was found");
                    return false;
                }
            }
            return true;
        }

        public override TAsset ReadJson(JsonReader reader, Type objectType, TAsset existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (!ValidateRegistry()) {
                return default;
            }
            var key = serializer.Deserialize<TKey>(reader);
            if (AssetRegistry.Assets.TryGetValue(key, out TAsset val))
                return val;
            return null; 
        }
        static public TKey StaticReadJson(JsonReader reader, JsonSerializer serializer) {
            return serializer.Deserialize<TKey>(reader);    
        }

        public override void WriteJson(JsonWriter writer, TAsset value, JsonSerializer serializer)
        {
            if (!ValidateRegistry()) {
                return;
            }
            writer.WriteValue(AssetRegistry.GetKeyAddress(value));
        }

    }
}
