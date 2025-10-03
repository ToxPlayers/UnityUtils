using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using NUnit.Framework;
namespace SingletonBehaviors
{
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
                        log += singType + " Doesnt have instance prop";
                    else
                    {
                        log += "Removed Singleton For " + singType + '\n';
                        prop.SetValue(null, null);
                        removedCount++;
                    }
                }
                catch (Exception ex) { Debug.LogException(ex); }
            }  
            if(removedCount > 0)
                Debug.Log($"Removed {removedCount} singletons\n{log}");  
        }
        /*[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]*/
        [UnityEditor.MenuItem("Tools/Singletons/Force All Singeltons Instances")]
        static void ForceAllBeforeSceneLoad() => ForceAll();
    #endif

        public static void ForceAll()
        {
            string log = ""; 
            var singTypes = CExtensions.GetDerivingTypes(typeof(SingletonMono<>)); 
            if(singTypes.Count() == 0)
                return;
            
            var propertyFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;
            List<Type> instanceForced = new();
            List<Type> alreadyHasInstance = new();
            List<Type> notFound = new();
            List<Type> errorTypes = new();
		    foreach (Type type in singTypes)
            {
           
                try
                {
                    var hasInstancPropName = nameof(SingletonMono<SingletonMono>.HasInstance);
                    var hasInstance = (bool)type.GetProperty(hasInstancPropName, propertyFlags).GetValue(null, null);
                    if (hasInstance)
                    {
                        alreadyHasInstance.Add(type);
                        log += $"{type.Name} \n";
                        continue;
                    }
                    var findAndForceRootGosName = nameof(SingletonMono<SingletonMono>.FindAndForceInstanceAtRootGOs);
                    var method = type.GetMethod(findAndForceRootGosName, propertyFlags);
                    var singObj = (UnityEngine.Object) method.Invoke(null, null); 
                    if (singObj)
                    {
                        type.GetMethod(nameof(SingletonMono<SingletonMono>.ForceInstance)).Invoke(singObj, null);
                        instanceForced.Add(type);
                    }
                    else
                        notFound.Add(type);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    errorTypes.Add(type);
                }
            }

            AddStringifyList("Forced instances:", instanceForced, ref log);
            AddStringifyList("Allready has instance:", alreadyHasInstance, ref log);
            AddStringifyList("Singelton not found:", notFound, ref log);
            AddStringifyList( LogUtil.ColorRed("Errors:"),errorTypes, ref log);

            log = $"Forced {instanceForced.Count}/{singTypes.Count()} instances:\n{log}\n";
		    Debug.Log(log); 
        }
    
        static string GetColoredStringName(Type t) => LogUtil.Color(t.Name, new Color(0, 0.6f, 0)) + ".cs";
        static void AddStringifyList(string header, List<Type> types, ref string log)
        {
            log += '\n';
            if (types.Count > 0)
                log += header + '\n';

            foreach (var nf in types)
                log += GetColoredStringName(nf) + '\n';
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
}
