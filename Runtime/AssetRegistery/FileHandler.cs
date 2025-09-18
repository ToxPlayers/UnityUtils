using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

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
        public bool IsPreferencesFile;
        public bool IsEncrypted = false;
        public bool Backup = true;
    }

    [Serializable]
#if UNITY_EDITOR
    [LabelText(@"@""File<"" + $property.ValueEntry.TypeOfValue.GetGenericArguments()[0].FullName.Replace(""+"",""."") + ""> "" + $property.NiceName", 
        Icon = SdfIconType.FileText)]
#endif
    public class FileHandler<T> where T : new()
    {
        [NonSerialized] public UnityEvent OnLoaded = new(), OnSaved = new();
        [SerializeField, FoldoutGroup("Save Settings"), HideLabel]
        public SaveSettings Settings = new() { FileName = typeof(T).Name };
        Notifier<T> _value = new();
        public IReadOnlyNotifier<T> Notifier => _value;
        [ShowInInspector, HideLabel, HideReferenceObjectPicker] 
        public T Value  
        {
            get
            {
                if (_value == null && ! TryLoad())
                    _value = new();
                return _value;
            }
            set => SetValue(value, true);
        }
        public void SetValue(T value, bool shouldSave)
        {
            _value.Value = value;
            try
            {
                if (shouldSave)
                    Save(shouldSave);
            }
            catch(Exception ex) { Debug.LogException(ex); }
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
            if (Settings.IsPreferencesFile)
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
        public string Save(bool log) => StaticSave(Settings, _value.Value ??= new(), GetFullPath(), log); 
        static public string StaticSave(SaveSettings settings, T value, string fullPath, bool log = true)
        {
            var content = JsonUnity.Serialize(value);
            StaticSave(settings, content, fullPath, log);
            return content;
        }
        static public void StaticSave(SaveSettings settings, string content, string fullPath, bool log = true)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                if (settings.IsEncrypted)
                    Encrypt(fullPath, content);
                else
                    File.WriteAllText(fullPath, content); 
                if (log)
                    Debug.Log($"saved {typeof(T).Name} -> {fullPath}");
            }
            catch (Exception ex) { Debug.LogException(ex); }

            if (settings.Backup && !IsBackupPath(fullPath))
                StaticSave(settings, content, BackupPath(fullPath), log);
        }

        [HorizontalGroup("Buttons",Order = 1000), Button]
        public bool TryLoad()
        {  
            var loaded = StaticTryLoad(Settings, GetFullPath(), out var val, true);
            if (!loaded)
                val = new();
            _value.Value = val;
            return loaded;
        }

        static bool StaticTryLoad(SaveSettings settings, string path, out T value, bool log = true)
        {
            string content;
            if (settings.IsEncrypted)
            {
                var bytes = File.ReadAllBytes(path);
                content = Decrypt(bytes);
            }
            else content = File.Exists(path) ? File.ReadAllText(path) : "";
            var loaded = !string.IsNullOrEmpty(content);
            value = default;
            if (loaded)
            {
                value = JsonUnity.Read<T>(content);
                if (value == null)
                    return false;
            }

            if(!loaded && !IsBackupPath(path))
                return StaticTryLoad(settings, BackupPath(path), out value, log);

            if (loaded)
                Debug.Log($"Loaded file <{typeof(T).Name}> from:\n" + path); 
            else
                Debug.LogWarning($"Failed to load file <{typeof(T).Name}> from:\n" + path);

            return loaded;
        }

        const byte EncryptionKey = 0x42;
        static void Encrypt(string path, string input)
        {
            using var file = File.OpenWrite(path);
            using var writer = new BinaryWriter(file);
            for (int i = 0; i < input.Length; i++)
                writer.Write((byte)(input[i] ^ EncryptionKey));
        }
        static string Decrypt(byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
                data[i] = (byte)(data[i] ^ EncryptionKey);
            return Encoding.UTF8.GetString(data);
        }
        static string BackupPath(string path) => path + SaveSettings.BackupExtension; 
        static bool IsBackupPath(string path) => path.EndsWith(SaveSettings.BackupExtension); 
    }
}
