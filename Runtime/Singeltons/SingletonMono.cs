using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using TriInspector;
#endif
namespace SingletonBehaviors
{
    public abstract class SingletonMono : MonoBehaviour
    {
        public virtual bool CanBeInstance { get; } = true;
        /// <summary>
        /// Called when this object is set to Instance of <typeparamref name="T"></typeparamref>
        /// </summary>
        internal virtual void OnInstanceSet() { }
        /// <summary>
        /// Called when this object is no longer the Instance object.
        /// </summary>
        internal virtual void OnInstanceRemoved() { }
    }

    abstract public class SingletonMono<T> : SingletonMono where T : SingletonMono 
    {
        private static T _instance = null;
        public static bool HasInstance => _instance; 
        public static Transform TF => Instance ? Instance.transform : null; 
        public static T Instance
        { 
            private set
            {
                if (_instance && value == _instance)
                    return;

                if (value && !value.CanBeInstance)
                    return;

                if(HasInstance)
                    _instance.OnInstanceRemoved(); 

                _instance = value;
             
                if (_instance != null)
                {
                    var instanceSing = _instance as SingletonMono<T>; 
                    instanceSing.OnInstanceSet();
                } 
            }
            get
            {
                if (!_instance)
                {
                    FindAndForceInstanceAtRootGOs();
                    _instance ??=  FindFirstObjectByType<T>(FindObjectsInactive.Include);
                    if (!_instance)
                        Debug.LogError("Could not find instance of type " + typeof(T).Name);
                }
                return _instance;
            }
        }  

        static public T FindAndForceInstanceAtRootGOs()
	    {
            if (_instance)
                return _instance;

            if ( UnityExtensions.TryGetRootComp<T>(out var rootT) ) 
                return Instance = rootT; 

            var inst = FindFirstObjectByType<T>();
            if (inst)
                (inst as SingletonMono<T>).ForceInstance();
            return inst;
        }
     
        public bool IsInstance { get => _instance && this == _instance; }
    #if UNITY_EDITOR
        string EDITOR_BTN_NAME => IsInstance ? "Instance Class" : "Force Instance";
        [PropertyOrder(100), DisableIf(nameof(IsInstance)), Button("$" + nameof(EDITOR_BTN_NAME))]
    #endif
        public void ForceInstance() => Instance = this as T;
	    protected virtual void Awake()
	    {  
    #if UNITY_EDITOR
            var prefabStatus = UnityEditor.PrefabUtility.GetPrefabInstanceStatus(gameObject);
            if(prefabStatus == UnityEditor.PrefabInstanceStatus.NotAPrefab)
		    {
    #endif
                T thisT = this as T;
                if (!HasInstance) 
                    Instance = thisT;  
    #if UNITY_EDITOR
            }
    #endif 
        } 
        protected virtual void OnDestroy()
	    {
		    if (_instance == this)
                Instance = null;
	    } 
    }
}
