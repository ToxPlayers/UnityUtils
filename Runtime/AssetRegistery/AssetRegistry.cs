using Files;
using Newtonsoft.Json;
using NUnit.Framework;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Files
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public abstract class AssetRegistry<T> : ScriptableSingleton<AssetRegistry<T>> where T : UnityEngine.Object
    {
        static public JsonConverter Converter => new UnityObjectJsonConverter<T>();
        public bool IsRegistryOfType(Type type) => typeof(T).IsAssignableFrom(type);
        [OdinSerialize, ReadOnly] Dictionary<string, T> _assets = new();
        public IReadOnlyDictionary<string, T> Assets => _assets;
        public virtual string GetKeyAddress(T asset) => asset ? asset.name : null;
        bool IsComponentType => typeof(Component).IsAssignableFrom(typeof(T));
        bool IsPrefab => typeof(GameObject).IsAssignableFrom(typeof(T));
        public virtual string SearchString
        {
            get
            {
                var t = typeof(T);
                var name = IsComponentType || IsPrefab ? "prefab" : t.FullName;
                return "t:" + name;
            }
        } 

        public virtual T Register(T asset)
        {
            if (!asset)
                return null;
            if (IsComponentType)
                if (asset is GameObject prefab && prefab.TryGetComponent(out T comp))
                    return comp;
            return asset;
        }

        public override void OnSingletonEnable() { }

#if UNITY_EDITOR

        [NonSerialized] bool _isHooked = false;
        protected override void OnEditorPreloaded()
        {
            base.OnEditorPreloaded();
            if (!_isHooked)
            {
                _isHooked = true;
                EditorApplication.projectChanged += ReregisterAllAssets;
                ReregisterAllAssets();
            }
        } 

        public virtual void OnValidate()
        {
            if (_assets == null || _assets.Count == 0)
                ReregisterAllAssets();
        } 

        public void ClearNulls()
        {
            var toRemove = new List<string>();
            foreach (var keyValue in _assets)
                if (!keyValue.Value)
                    toRemove.Add(keyValue.Key);
            foreach (var key in toRemove)
                _assets.Remove(key);
        }

         [Button,PropertyOrder(-100)]
		 public void ReregisterAllAssets()
		 {
			 _assets ??= new();
			 ClearNulls();
			 var guids = AssetDatabase.FindAssets(SearchString, new string[] { "Assets" });
			 var values = new HashSet<T>(_assets.Values);
			 foreach (var guid in guids)
			 {
				 var path = AssetDatabase.GUIDToAssetPath(guid);
				 var asset = AssetDatabase.LoadAssetAtPath<T>(path);
				 if (!asset)
					 continue;
				 var keyAddress = GetKeyAddress(asset);
				 if (values.Contains(asset))
				 {
					 foreach (var k in _assets.Where(kv => kv.Value == asset && kv.Key != keyAddress).ToArray())
					 {
						 Debug.Log(name + ": Removed asset " + k.Value + " asset added under new key (" + GetKeyAddress(asset) + ")");
						 _assets.Remove(k.Key);
					 }
				 }
				 TryRegister(asset, keyAddress); 
			 }

			 EditorUtility.SetDirty(this);
		 }

		 
		 public void TryRegister(T asset)
		 { 
			 var address = GetKeyAddress(asset);
			 TryRegister(asset, address);
		 }
		 public void TryRegister(T asset, string address)
		 {
			 if (_assets.TryGetValue(address, out T containAsset))
			 {
				 var containedAddress = GetKeyAddress(containAsset);
				 if (containedAddress == address && containAsset == asset)
					 return;
				 var containedPath = AssetDatabase.GetAssetPath(containAsset);
				 Debug.LogError(name + $": Cant register same asset name:\n{AssetDatabase.GetAssetPath(asset)}\nAlready Registered: {containedPath}\n", asset);
			 }
			 else
			 {
				 Debug.Log(name + $": {asset.name} Added to {GetType().Name}");
				 _assets.TryAdd(address, asset);
			 }
		 }

#endif
 
    }

}