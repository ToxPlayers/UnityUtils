using Newtonsoft.Json;
using UnityEngine;
using System;
using System.Collections.Generic;
using Files;

public class UObjKeyDictionaryConverter<TKey, TValue> : JsonConverter<Dictionary<TKey, TValue>> where TKey : UnityEngine.Object
{
    public override Dictionary<TKey, TValue> ReadJson(JsonReader reader, Type objectType, Dictionary<TKey, TValue> existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        // Implement custom deserialization logic for dictionary keys and values
        // For example, if keys are complex objects and need specific parsing
        // This example assumes simple string keys for demonstration
        var dictionary = new Dictionary<TKey, TValue>();
        reader.Read(); // StartObject
        while (reader.TokenType == JsonToken.PropertyName)
        { 
            var keyString = reader.Value?.ToString();
            TKey key = UnityObjectJsonConverter<TKey>.StaticReadJson(reader);
            reader.Read(); // Value
            TValue value = serializer.Deserialize<TValue>(reader);
            if(key is not null)
                dictionary.Add(key, value);
            else Debug.LogError("Json Dictionary Error: Could not read NULL key with value: " + value);
            reader.Read(); // EndObject or next PropertyName
        }
        return dictionary;
    }

    public override void WriteJson(JsonWriter writer, Dictionary<TKey, TValue> value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        foreach (var item in value)
        {
            // Implement custom serialization logic for dictionary keys
            // For example, if keys are complex objects and need specific formatting
            writer.WritePropertyName(item.Key.name); // Example conversion
            serializer.Serialize(writer, item.Value);
        }
        writer.WriteEndObject();
    }
}