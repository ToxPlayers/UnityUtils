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
    public abstract class AssetRegistry<T> : ScriptableSingleton<AssetRegistry<T>> where T : UnityEngine.Object
    {
        static public JsonConverter Converter => new UnityObjectJsonConverter<T>();
        public bool IsRegisteryOfType(Type type) => typeof(T).IsAssignableFrom(type);
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

        public override void OnSingletonAwake() { }
#if UNITY_EDITOR

        [NonSerialized] bool _isHooked = false;
        public override void OnSingletonEditorAwake()
        {
            base.OnSingletonEditorAwake();
            if (!_isHooked)
            {
                _isHooked = true;
                EditorApplication.projectChanged += ReregisterAllAssets;
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
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                TryRegister(path);
            }
        }

        public void TryRegister(string path)
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            asset = Register(asset);
            if (!asset)
                return;
            var address = GetKeyAddress(asset);

            if (_assets.TryGetValue(address, out T containAsset))
            {
                var containedAddress = GetKeyAddress(containAsset);
                if (containedAddress == address && containAsset == asset)
                    return;
                var containedPath = AssetDatabase.GetAssetPath(containAsset);
                Debug.LogError($"Cant register same asset name:\n{path}\nAlready Registered: {containedPath}\n", asset);
            }
            else
            {
                Debug.Log($"{asset.name} Added to {GetType().Name}");
                _assets.TryAdd(address, asset);
            }
        }

#endif

        protected override void Awake()
        {
            base.Awake();
#if UNITY_EDITOR
            var removeLst = new List<string>();
            foreach (var keyValue in _assets)
            {
                if (!keyValue.Value)
                    removeLst.Add(keyValue.Key);
            }
            foreach (var rmvLst in removeLst)
                _assets.Remove(rmvLst); 
#endif
        }
    }

}