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
    public abstract class AssetRegistryNameBased<TAsset> : AssetRegistryBase<string, TAsset> where TAsset : UnityEngine.Object {
        public override string GetKeyAddress(TAsset asset) => asset ? asset.name : null;
    }
    public abstract class AssetRegistryCustomKeyBased<TKey, TAsset>  : AssetRegistryBase<TKey, TAsset> where TAsset : UnityEngine.Object, IAssetRegister<TKey> {
        public override TKey GetKeyAddress(TAsset asset) => asset ? asset.GetRegistryAddress() : default;
    }
    public interface IAssetRegister<TKey> : IEquatable<TKey> { 
        public TKey GetRegistryAddress(); 
    }
    public abstract class AssetRegistryBase<TKey, TAsset> : ScriptableSingleton<AssetRegistryBase<TKey, TAsset>> where TAsset : UnityEngine.Object{
        static public JsonConverter Converter => new AssetRegisteryJsonConverter<TKey, TAsset>();
        public bool IsRegistryOfType(Type type) => typeof(TAsset).IsAssignableFrom(type);
        [ShowInInspector, ReadOnly, InlineProperty]
        protected abstract Dictionary<TKey, TAsset> _internalDictionary { get; set; }
        public IReadOnlyDictionary<TKey, TAsset> Assets => _internalDictionary;
        public abstract TKey GetKeyAddress(TAsset asset);
        public bool TryGetAsset(TKey address, out TAsset asset) {
			if(address == null || address.Equals(default(TKey))) {
				asset = null;
				return false;
			}
			return _internalDictionary.TryGetValue(address, out asset);
		} 
        bool IsComponentType => typeof(Component).IsAssignableFrom(typeof(TAsset));
        bool IsPrefab => typeof(GameObject).IsAssignableFrom(typeof(TAsset));
        public virtual string SearchString
        {
            get
            {
                var t = typeof(TAsset);
                var name = IsComponentType || IsPrefab ? "prefab" : t.FullName;
                return "t:" + name;
            }
        } 

        public virtual TAsset Register(TAsset asset)
        {
            if (!asset)
                return null;
            if (IsComponentType)
                if (asset is GameObject prefab && prefab.TryGetComponent(out TAsset comp))
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
            if (_internalDictionary == null || _internalDictionary.Count == 0)
                ReregisterAllAssets();
        } 

        public void ClearNulls()
        {
            var toRemove = new List<TKey>();
            foreach (var keyValue in _internalDictionary)
                if (!keyValue.Value)
                    toRemove.Add(keyValue.Key);
            foreach (var key in toRemove)
                _internalDictionary.Remove(key);
        }

         [Button,PropertyOrder(-100)]
		 public void ReregisterAllAssets()
		 { 
			 _internalDictionary ??= new();
			 ClearNulls();
			 var guids = AssetDatabase.FindAssets(SearchString, new string[] { "Assets" });
			 var values = new HashSet<TAsset>(_internalDictionary.Values);
			 foreach (var guid in guids)
			 {
				 var path = AssetDatabase.GUIDToAssetPath(guid);
				 var enterAsset = AssetDatabase.LoadAssetAtPath<TAsset>(path);
				 if (!enterAsset)
					 continue;
				 var keyAddress = GetKeyAddress(enterAsset);

                 if(TryGetAsset(keyAddress, out TAsset existingAsset))
                 {
                     if (existingAsset.Equals(enterAsset))
                         continue;  
                 }

				 if (values.Contains(enterAsset))
				 {
					 foreach (var k in _internalDictionary.Where(kv => kv.Value.Equals(enterAsset) && kv.Key.Equals(keyAddress)).ToArray())
					 {
						 Debug.Log(name + ": Removed asset " + k.Value + " asset added under new key (" + keyAddress + ")");
						 _internalDictionary.Remove(k.Key);
					 }
				 }

				 if( ! TryRegister(keyAddress, enterAsset))
					 Debug.LogError(name + $": Failed to register {enterAsset.name} to {GetType().Name}", enterAsset);
			 }

			 EditorUtility.SetDirty(this);
		 }
		 public bool TryRegister(TAsset asset)
		 { 
			 var address = GetKeyAddress(asset);
			 return TryRegister(address, asset);
		 }
		 public bool TryRegister(TKey address, TAsset asset)
		 {
			 if (_internalDictionary.TryGetValue(address, out TAsset containAsset))
			 {
				 var containedAddress = GetKeyAddress(containAsset);
				 if (containedAddress.Equals(address) && containAsset.Equals(asset))
					 return false;
				 var containedPath = AssetDatabase.GetAssetPath(containAsset);
				 Debug.LogError(name + $": Cant register same asset name:\n{AssetDatabase.GetAssetPath(asset)}\nAlready Registered:\n{containedPath}\n", asset);
                return false;
            } else
			 {
				 Debug.Log(name + $": {asset.name} Added to {GetType().Name}");
				 if(!_internalDictionary.TryAdd(address, asset)) {
                    Debug.LogError(name + $": Failed to add {asset.name} to {GetType().Name}", asset);
                    return false;
                }
                return true;
            }
		 } 
#endif 
    }

}