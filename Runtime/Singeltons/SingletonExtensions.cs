using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
static public class SingletonExtensions
{
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Singletons/Remove All Singeltons Instances")]
    [UnityEditor.InitializeOnEnterPlayMode] 
    static void RemoveAllInstances()
	{ 
        var log = "";
        int removedCount = 0; 
        var singTypes = CExtensions.GetDerivingTypes(typeof(SingletonMono<>));
        foreach (var singType in singTypes)
        {
            try
            {
                var prop = singType.BaseType?.GetRuntimeProperty("Instance");
                if (prop == null)
                    log += LogUtil.Color(singType + " Doesnt have instance prop", Color.red);
                else
                {
                    log += "Removed Singleton For " + singType + '\n';
                    prop.SetValue(null, null);
                    removedCount++;
                }
            }
            catch (Exception ex) { Debug.LogException(ex); }
        }  
        Debug.Log($"Removed {removedCount} singletons\n{log}");  
    }

    [UnityEditor.MenuItem("Tools/Singletons/Force All Singeltons Instances")]
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ForceAllBeforeSceneLoad() => ForceAll(); 
#endif
    public static void ForceAll()
    {
        string log = ""; 
        int forcedInstancesCount = 0;
        string logWarnings = "";
        var singTypes = CExtensions.GetDerivingTypes(typeof(SingletonMono<>));
        var propertyFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;
        try
        { 

            var colorLog = new Color(0, 0.6f, 0); 
			foreach (Type type in singTypes)
            { 
                var hasInstance = (bool)type.GetProperty("HasInstance" , propertyFlags).GetValue(null, null);
                 
                if (hasInstance)
                {
                    log += $"{type.Name} allready has instance";
                    continue;
                }
				var singObj = UnityEngine.Object.FindFirstObjectByType(type);
                if (singObj)
                {
                    type.GetMethod("ForceInstance").Invoke(singObj, null); 
                    forcedInstancesCount++;
                    log += LogUtil.Color("\n"+ type.Name +".cs", colorLog);
                }
                else
                    logWarnings += "Singelton Object not found for " + LogUtil.Color(type.Name, colorLog) + ".cs" +"\n";
            }  
        } 
        catch (Exception ex) { Debug.LogException(ex); }
        finally
        {
            log = $"Forced {forcedInstancesCount}/{singTypes.Count()} instances:\n{log} \n";
            log += LogUtil.Color(logWarnings, Color.yellow);
			Debug.Log(log); 
		} 
    }
     
    static bool IsSubclassOfGeneric(Type type, Type baseType)
    {
        if (type == null || baseType == null || type == baseType)
            return false;

        if (baseType.IsGenericType == false)
        {
            if (type.IsGenericType == false)
                return type.IsSubclassOf(baseType);
        }
        else
        {
            baseType = baseType.GetGenericTypeDefinition();
        }

        type = type.BaseType;
        Type objectType = typeof(object);

        while (type != objectType && type != null)
        {
            Type curentType = type.IsGenericType ?
                type.GetGenericTypeDefinition() : type;
            if (curentType == baseType)
                return true;

            type = type.BaseType;
        }

        return false;
    }

}
