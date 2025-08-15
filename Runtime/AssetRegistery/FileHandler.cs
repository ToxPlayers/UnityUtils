using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Files
{
    [Serializable]
    public class SaveSettings
    {   
        public virtual string FileExtension
        {
            get
            {
                if (string.IsNullOrEmpty(OverrideFileExtension))
                    return IsEncrypted ? ".bin" : ".json";
                return OverrideFileExtension;
            }
        }
        public const string BackupExtension = ".backup";
        public string OverrideFileExtension;
        public string FileName;
        public string Folder;
        public bool IsPrefrencesFile;
        public bool IsEncrypted = false;
        public bool Backup = true;
        public JsonSerializerSettings SerializerSettings => new JsonSerializerSettings()  
        {  Formatting = Formatting.Indented  };
    }

    [Serializable]
    public class FileHandler<T> where T : new()
    {
#if UNITY_EDITOR
        public string GetInspcetorName()
        {
            return $"File[{typeof(T).Name}]:"+ Path.ChangeExtension(Settings.FileName, Settings.FileExtension);
        }
#endif

        [SerializeField]
        public SaveSettings Settings = new();
        T _value;
        [ShowInInspector, HideLabel, InlineProperty]
        public T Value
        {
            get
            {
                if (_value == null && ! TryLoad(out _value))
                    _value ??= new();
                return _value;
            }
            set => SetValue(_value = value, true);
        }

        public void SetValue(T value, bool save)
        {
            _value = value;
            if (save)
                Save(_value);
        }
        public FileHandler()
        {
            Settings = new SaveSettings();
        }

        public FileHandler(SaveSettings settings)
        {
            Settings = settings; 
        } 
        public string GetParentPath()
        {
            if (Settings.IsPrefrencesFile)
                return Application.persistentDataPath;

            if (Application.isEditor)
                return Path.Join(Application.streamingAssetsPath, "Saves");
            return Application.dataPath;
        }

        public string GetFullPath()
        {
            var parentDir = GetParentPath();
            var path = Path.Join(parentDir, Settings.FileName);
            return Path.ChangeExtension(path, Settings.FileExtension);
        }

        [Button]
        public string Save(bool log = true) => Save(_value ??= new(), GetFullPath(), log);

        public string Save(T storage, bool log = true) => Save(storage, GetFullPath(), log);

        public string Save(T storage, string fullPath, bool log = true)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                var content = JsonConvert.SerializeObject(storage, Settings.SerializerSettings);

                if (Settings.IsEncrypted)
                    Encrypt(fullPath, content);
                else
                    File.WriteAllText(fullPath, content);
                if(log)
                    Debug.Log($"saved {typeof(T).Name} -> {fullPath}");
                return content;
            }
            catch (Exception ex) { Debug.LogException(ex); }
            return null;
        }

        const byte Key = 0x42;
        void Encrypt(string path, string input)
        {
            using var file = File.OpenWrite(path);
            using var writer = new BinaryWriter(file);
            for (int i = 0; i < input.Length; i++)
                writer.Write((byte)(input[i] ^ Key));
        }

        string Decrypt(byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
                data[i] = (byte)(data[i] ^ Key);
            return Encoding.UTF8.GetString(data);
        }

        [Button]
        public T TryLoad(bool log = true)
        {
            TryLoad(out T val, log);
            return val;
        }
        bool TryLoad(out T value, bool log = true)
        {
            value = default;
            var path = GetFullPath();
            try
            {
                string content;
                if (Settings.IsEncrypted)
                {
                    var bytes = File.ReadAllBytes(path);
                    content = Decrypt(bytes);
                }
                else content = File.Exists(path) ? File.ReadAllText(path) : ""; 
				if( string.IsNullOrEmpty(content))
					return false;
                value = JsonConvert.DeserializeObject<T>(content, Settings.SerializerSettings);
                if (log)
                    Debug.Log($"Loaded {typeof(T).Name} -> {path}");
                return true;
            }
            catch (Exception ex) { Debug.LogException(ex); }
            if (log)
                Debug.Log($"FAILED to load {typeof(T).Name} -> {path}");
            return false;
        }
    }
}
