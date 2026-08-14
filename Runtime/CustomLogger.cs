
using UnityEngine;
using Conditional = System.Diagnostics.ConditionalAttribute;

[System.Serializable]
public struct CustomLogger {
#if UNITY_EDITOR
    public Object PingObj; 
    public string Prefix; 
    public string Suffix;
    public bool Disable;
#endif
    public CustomLogger(Object pingObj, Color prefixColor, string prefix, Color suffixColor, string suffix) {
#if UNITY_EDITOR
        PingObj = pingObj;
        Prefix = LogUtil.Color(prefix, prefixColor);
        Suffix = LogUtil.Color(suffix, suffixColor);
        Disable = false;
#endif
    }
    public CustomLogger(Object pingObj) {
#if UNITY_EDITOR
        PingObj = pingObj;
		if(pingObj)
			Prefix = "[" + PingObj.GetType().Name + "] "; 
		else Prefix = "";
		Suffix = "";
        Disable = false;
#endif
    }
    public CustomLogger(Object pingObjTypeAsSuffix, Color prefixColor) {
#if UNITY_EDITOR
        PingObj = pingObjTypeAsSuffix;
		if(pingObjTypeAsSuffix){
			 Prefix = "[" + pingObjTypeAsSuffix.GetType().Name + "] "; 
			Prefix = LogUtil.Color(Prefix, prefixColor);
		}else Prefix = "";
       
        Suffix = "";
        Disable = false;
#endif
    }
    public CustomLogger(Object pingObj, Color prefixColor, string prefix) {
#if UNITY_EDITOR
        PingObj = pingObj;
        Prefix = LogUtil.Color(prefix, prefixColor);
        Suffix = "";
        Disable = false;
#endif
    }

    [HideInCallstack]
    public readonly string Format(string msg) {
#if UNITY_EDITOR
        var prefix = Prefix == null ? "" : Prefix;
        var suffix = Suffix == null ? "" : Suffix;
        return prefix + msg + suffix;
#else
        return "";
#endif
    }

    [Conditional("UNITY_EDITOR"), HideInCallstack]
    public readonly void Log(string msg) {
#if UNITY_EDITOR
        Log(msg, PingObj);
#endif
    }

    public readonly void Log(string msg, Object context) {
#if UNITY_EDITOR
        if(Disable)
            return;
        msg = Format(msg);
        if (context)
            Debug.Log(msg, context);
        else Debug.Log(msg);
#endif
    }

    [HideInCallstack]
    public readonly void LogWarning(string msg) {
#if UNITY_EDITOR
        if (Disable)
            return;
        msg = Format(msg);
        if (PingObj)
            Debug.LogWarning(msg, PingObj);
        else Debug.LogWarning(msg);
#endif
    }

    [HideInCallstack]
    public readonly void LogError(string msg) {
#if UNITY_EDITOR 
        msg = Format(msg);
        if (PingObj)
            Debug.LogError(msg, PingObj);
        else Debug.LogError(msg);
#else
        Debug.LogError(msg);
#endif
    }


    [HideInCallstack]
    public readonly void LogException(System.Exception ex) {
#if UNITY_EDITOR
        Debug.LogException(ex, PingObj);
#else
        Debug.LogException(ex);
#endif
    }

}
