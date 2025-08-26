using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.IO;
using System.Text;
using UnityEngine;

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
        public string FileName;
        public string Folder;
        public string OverrideFileExtension;
        public bool IsPrefrencesFile;
        public bool IsEncrypted = false;
        public bool Backup = true;
        public JsonSerializerSettings SerializerSettings => new JsonSerializerSettings()  
        {  Formatting = Formatting.Indented  };
    }

    [Serializable]
#if UNITY_EDITOR
    [LabelText(@"@""File<"" + $property.ValueEntry.TypeOfValue.GetGenericArguments()[0].FullName.Replace(""+"",""."") + ""> "" + $property.NiceName", 
        Icon = SdfIconType.FileText)]
#endif
    public class FileHandler<T> where T : new()
    {
        [SerializeField, FoldoutGroup("Save Settings"), HideLabel]
        public SaveSettings Settings = new() { FileName = typeof(T).Name };
        T _value;
        [ShowInInspector, HideLabel, HideReferenceObjectPicker] 
        public T Value  
        {
            get
            {
                if (_value == null && ! TryLoad())
                    _value = new();
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
                return Path.Join(Application.streamingAssetsPath, "FileSaves");

            return Application.dataPath;
        }

        public string GetFullPath()
        {
            var path = GetParentPath();
            if( ! string.IsNullOrWhiteSpace(Settings.Folder))
                path = Path.Join(path, Settings.Folder);
            path = Path.Join(path, Settings.FileName);
            return Path.ChangeExtension(path, Settings.FileExtension);
        }

        [HorizontalGroup("Buttons"), Button, PropertyOrder(1000)]
        public string Save() => Save(true);
        public string Save(bool log) => Save(_value ??= new(), GetFullPath(), log);

        public string Save(T storage, bool log = true) => Save(storage, GetFullPath(), log);

        public string Save(T storage, string fullPath, bool log = true)
        {
            var content = JsonConvert.SerializeObject(storage, Settings.SerializerSettings);
            Save(content, fullPath, log);
            return content;
        }
        public void Save(string content, string fullPath, bool log = true)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                if (Settings.IsEncrypted)
                    Encrypt(fullPath, content);
                else
                    File.WriteAllText(fullPath, content);
                if(log)
                    Debug.Log($"saved {typeof(T).Name} -> {fullPath}");
            }
            catch (Exception ex) { Debug.LogException(ex); }

            if (Settings.Backup && !IsBackupPath(fullPath))
                Save(content, BackupPath(fullPath), log);
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

        [HorizontalGroup("Buttons",Order = 1000), Button]
        public bool TryLoad() => TryLoad(true);
        public bool TryLoad(bool log)
        {
            return TryLoad(GetFullPath(), out _value, log); 
        }

        bool TryLoad(string path, out T value, bool log = true)
        {
            string content;
            if (Settings.IsEncrypted)
            {
                var bytes = File.ReadAllBytes(path);
                content = Decrypt(bytes);
            }
            else content = File.Exists(path) ? File.ReadAllText(path) : "";
            var loaded = !string.IsNullOrEmpty(content);
            value = default;
            if (loaded)
            {
                value = JsonConvert.DeserializeObject<T>(content, Settings.SerializerSettings);
                if (value == null)
                    return false;
            }

            if(!loaded && !IsBackupPath(path))
                return TryLoad(BackupPath(path), out value);

            if (loaded)
                Debug.Log($"Loaded file <{typeof(T).Name}> from:\n" + path);
            else
                Debug.LogWarning($"Failed to load file <{typeof(T).Name}> from:\n" + path);

            return loaded;
        } 

        string BackupPath(string path) => path + SaveSettings.BackupExtension; 
        bool IsBackupPath(string path) => path.EndsWith(SaveSettings.BackupExtension); 
    }
}
