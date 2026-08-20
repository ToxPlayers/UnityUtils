
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
        var prefix = Prefix ?? "";
        var suffix = Suffix ?? "";
        return prefix + msg + suffix;
#else
        return "";
#endif
    }

    [HideInCallstack] public readonly void Log(string msg, Object ping = null)  => Log(0, msg, ping);
    [HideInCallstack] public readonly void LogWarning(string msg) => Log(1, msg);
    [HideInCallstack] public readonly void LogError(string msg) => Log(2, msg);

    [Conditional("UNITY_EDITOR"), HideInCallstack]
    public readonly void Log(int logLevel, string msg, Object context = null) {
#if UNITY_EDITOR
        if (Disable)
            return;

        if(!context)
            context = PingObj;
        msg = Format(msg);
        if(logLevel <= 0) {
            if (context)
                Debug.Log(msg, context);
            else Debug.Log(msg);
        }
        else if(logLevel == 1) {
            if (context)
                Debug.LogWarning(msg, context);
            else Debug.LogWarning(msg);
        } else if (logLevel >= 2) {
            if (context)
                Debug.LogError(msg, context);
            else Debug.LogError(msg);
        } 
#endif
    }

    [HideInCallstack]
    public readonly void LogException(System.Exception ex, Object ping) {
#if UNITY_EDITOR
        if(!ping)
            ping = PingObj;
#endif
        if(ping)
            Debug.LogException(ex, ping);
        else Debug.LogException(ex);
    }

}
