using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

static public class JsonUnity
{ 
    class UnityJsonResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);
            prop.Ignored = prop.Ignored || member.MemberType == MemberTypes.Property;
            return prop;
        }
    }
    static public readonly JsonSerializerSettings DefaultJsonSettings = new JsonSerializerSettings()
    {
        TypeNameHandling = TypeNameHandling.Auto,
        ContractResolver = new UnityJsonResolver(),
        Formatting = Formatting.Indented,
        PreserveReferencesHandling = PreserveReferencesHandling.None,
        ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
        NullValueHandling = NullValueHandling.Include
    };
    static public string Serialize<T>(T obj) => JsonConvert.SerializeObject(obj, DefaultJsonSettings);
    static public T Read<T>(string json) => JsonConvert.DeserializeObject<T>(json, DefaultJsonSettings);
}
