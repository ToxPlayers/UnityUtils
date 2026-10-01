using Files;
using Newtonsoft.Json;
using NUnit.Framework;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic; 
using Stopwatch = System.Diagnostics.Stopwatch;
using System.IO;
using System.Linq;
using Unity.Profiling;
using UnityEditor; 
using UnityEngine;
using System.Text;
namespace Files {
    public abstract class AssetRegistryNameBased<TAsset> : AssetRegistryBase<string, TAsset> where TAsset : UnityEngine.Object {
        public override string GetKeyAddress(TAsset asset) => asset ? asset.name : null;
    }
    public abstract class AssetRegistryCustomKeyBased<TKey, TAsset> : AssetRegistryBase<TKey, TAsset> where TAsset : UnityEngine.Object, IAssetRegister<TKey> {
        public override TKey GetKeyAddress(TAsset asset) => asset ? asset.GetRegistryAddress() : default;
    }
    public interface IAssetRegister<TKey> : IEquatable<TKey> {
        public TKey GetRegistryAddress();
    }
    public abstract class AssetRegistryBase<TKey, TAsset> : ScriptableSingleton<AssetRegistryBase<TKey, TAsset>> where TAsset : UnityEngine.Object {
        static public JsonConverter Converter => new AssetRegisteryJsonConverter<TKey, TAsset>();
        public bool IsRegistryOfType(Type type) => typeof(TAsset).IsAssignableFrom(type);
        [ShowInInspector, ReadOnly, InlineProperty]
        protected abstract Dictionary<TKey, TAsset> _internalDictionary { get; set; }
        public IReadOnlyDictionary<TKey, TAsset> Assets => _internalDictionary;
        public abstract TKey GetKeyAddress(TAsset asset);
        public bool TryGetAsset(TKey address, out TAsset asset) {
            if (address == null || address.Equals(default(TKey))) {
                asset = null;
                return false;
            }
            return _internalDictionary.TryGetValue(address, out asset);
        }
        bool IsComponentType => typeof(Component).IsAssignableFrom(typeof(TAsset));
        bool IsPrefab => typeof(GameObject).IsAssignableFrom(typeof(TAsset));
        public virtual string SearchString {
            get { 
                if (IsComponentType || IsPrefab) {
                    return "t:prefab";
                } 
                return "t:" + typeof(TAsset).FullName;
            }
        }

        public virtual TAsset Register(TAsset asset) {
            if (!asset)
                return null;
            if (IsComponentType)
                if (asset is GameObject prefab && prefab.TryGetComponent(out TAsset comp))
                    return comp;
            return asset;
        }

        public override void OnSingletonEnable() { }
        protected virtual void OnDisable() {
#if UNITY_EDITOR
            EditorApplication.projectChanged -= ReregisterAllAssets;
#endif
        }
#if UNITY_EDITOR
        [NonSerialized] bool _isHooked = false;
        protected override void OnEditorPreloaded() {
            base.OnEditorPreloaded();
            if (!_isHooked) {
                _isHooked = true;
                EditorApplication.projectChanged -= ReregisterAllAssets;
                EditorApplication.projectChanged += ReregisterAllAssets;
                ReregisterAllAssets();
            }
        }

        public virtual void OnValidate() {
            if (_internalDictionary == null || _internalDictionary.Count == 0)
                ReregisterAllAssets();
        }

        public void ClearNulls() {
            var toRemove = new List<TKey>();
            foreach (var keyValue in _internalDictionary)
                if (!keyValue.Value)
                    toRemove.Add(keyValue.Key);
            foreach (var key in toRemove)
                _internalDictionary.Remove(key);
        }
        ProfilerMarker _reregisterAllAssetsMarker = new ProfilerMarker("AssetRegistry.ReregisterAllAssets");
        ProfilerMarker _unloadAssetsMarker = new ProfilerMarker("AssetRegistry.UnloadAssets");
        [Button, PropertyOrder(-100)]
        public void ReregisterAllAssets() {
            using var profilerMarker = _reregisterAllAssetsMarker.Auto();
            var timer = new Stopwatch();
            timer.Restart();
            _internalDictionary ??= new();
            ClearNulls();
            var query = SearchString;
            var guids = AssetDatabase.FindAssets(query, new string[] { "Assets" });
            bool isDirty = false;
            HashSet<TKey> checkedKeys = new(_internalDictionary.Count);
            var isCompType = IsComponentType;
            var typeName = typeof(TAsset).Name;
            foreach (var guid in guids) {
                var enterAsset = AssetDatabase.LoadAssetByGUID<TAsset>(new GUID(guid));
                if (!enterAsset)
                    continue;

                var keyAddress = GetKeyAddress(enterAsset);
                checkedKeys.Add(keyAddress);

                if (TryGetAsset(keyAddress, out TAsset existingAsset)) {
                    if (existingAsset == enterAsset)
                        continue;

                    var sameKey = 
                        _internalDictionary.Comparer.Equals(GetKeyAddress(existingAsset), keyAddress);
                    if (sameKey) {
                        Debug.LogError(name + ": Duplicate asset key " + keyAddress + " for assets:\n" + AssetDatabase.GetAssetPath(existingAsset) + "\n" + AssetDatabase.GetAssetPath(enterAsset), enterAsset);
                        continue;
                    }

                    Debug.Log("Replacing " + existingAsset.name + " in address: " + keyAddress + " with:" + enterAsset.name);
                    _internalDictionary.Remove(keyAddress);
                    isDirty = true;
                }

                foreach (var k in _internalDictionary.Where(kv => kv.Value.Equals(enterAsset)).ToArray()) {
                    Debug.Log(name + ": Replaced asset key" + k.Value + " new key (" + keyAddress + ")");
                    _internalDictionary.Remove(k.Key);
                    isDirty = true;
                }

                if (TryRegister(keyAddress, enterAsset)) {
                    isDirty = true;
                }
                else 
                    Debug.LogError(name + $": Failed to register {enterAsset.name} to {GetType().Name}", enterAsset);
            }

            //keys not found 
            var keysToRemove = _internalDictionary.Keys.Where(k => !checkedKeys.Contains(k)).ToArray();
            foreach (var key in keysToRemove) {
                _internalDictionary.Remove(key);
                isDirty = true;
            }
            EditorApplication.delayCall += () => {
                using var unloadMarker = _unloadAssetsMarker.Auto();
                if (isDirty && this)
                    EditorUtility.SetDirty(this);
                EditorUtility.UnloadUnusedAssetsImmediate(true);
            };
            if(timer.ElapsedMilliseconds > 500) {
                Debug.Log($"RegisterAllAssets took {timer.ElapsedMilliseconds}ms for {guids.Length} assets (query={query})");
            }
        }
        public bool TryRegister(TAsset asset) {
            var address = GetKeyAddress(asset);
            return TryRegister(address, asset);
        }
        public bool TryRegister(TKey address, TAsset asset) {
            if (_internalDictionary.TryGetValue(address, out TAsset containAsset)) {
                var containedAddress = GetKeyAddress(containAsset);
                if (containedAddress.Equals(address) && containAsset.Equals(asset))
                    return false;
                var containedPath = AssetDatabase.GetAssetPath(containAsset);
                Debug.LogError(name + $": Cant register same asset name:\n{AssetDatabase.GetAssetPath(asset)}\nAlready Registered:\n{containedPath}\n", asset);
                return false;
            } else {
                if (_internalDictionary.TryAdd(address, asset)) {
                    Debug.Log(name + $": {asset.name} Registered in {GetType().Name}");
                    return true;
                }
                Debug.LogError(name + $": Failed to register {asset.name} in {GetType().Name}", asset);
                return false;
            }
        }
#endif
    }

}
