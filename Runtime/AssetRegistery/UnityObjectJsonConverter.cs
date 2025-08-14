using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UObj = UnityEngine.Object;
namespace Files
{ 
    public class UnityObjectJsonConverter<T> : JsonConverter<T> where T : UObj 
    {
        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return StaticReadJson(reader);
        }

        static public T StaticReadJson(JsonReader reader)
        {
            if (reader.Value == null)
                return null;
            var strValue = reader.Value.ToString();
            if (string.IsNullOrEmpty(strValue))
                return null;
            if (!AssetRegistry<T>.Instance)
            {
                Debug.LogError("No instance of " + typeof(AssetRegistry<T>).Name + " was found");
                return null;
            }
            if (AssetRegistry<T>.Instance.Assets.TryGetValue(strValue, out T val))
                return val;
            return null;
        }

        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
        {
            writer.WriteValue(AssetRegistry<T>.Instance.GetKeyAddress(value));
        }
    }
}
