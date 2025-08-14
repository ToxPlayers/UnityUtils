Added saving ScriptableObjects and Prefabs refrences including components of prefabs.
```cs
public class FilesManager : ScriptableSingleton<FilesManager>
{ 
    [Serializable] public class Prefrences { }
    public FileHandler<GameSave> GameSaveFile = new();  
    public FileHandler<Prefrences> PrefFile = new(); 
}

[Serializable]
public class GameSave 
{ 
    [SerializeField] List<ModuleData> _modules=new();
    [SerializeField] List<Module> _unlockedModulePrefabs=new(); 
}

[JsonConverter(typeof(Files.UnityObjectJsonConverter<ModuleData>))]
[CreateAssetMenu(fileName = "ModuleData", menuName = "Module Data")]
public class ModuleData : ScriptableObject {}

[JsonConverter(typeof(Files.UnityObjectJsonConverter<Module>))] 
public class Module : MonoBehaviour {}

public class ModuleRegistry : Files.AssetRegistry<Module> { }
```
dependencies<br> 
Odin Inspector <br> 
```com.unity.nuget.newtonsoft-json```
