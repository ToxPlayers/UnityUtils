#define ENABLE_CUSTOM_LOGGER_BUILD
#if ENABLE_CUSTOM_LOGGER_BUILD || UNITY_EDITOR
#define ENABLE_CUSTOM_LOGGER
#endif

using UnityEngine;
using Conditional = System.Diagnostics.ConditionalAttribute;


[System.Serializable]
public struct CustomLogger {
#if ENABLE_CUSTOM_LOGGER_BUILD
    public Object PingObj; 
    public string Prefix; 
    public string Suffix;
    public bool Disable;
#endif
    public CustomLogger(Object pingObj, Color prefixColor, string prefix, Color suffixColor, string suffix) {
#if ENABLE_CUSTOM_LOGGER_BUILD
        PingObj = pingObj;
        Prefix = LogUtil.Color(prefix, prefixColor);
        Suffix = LogUtil.Color(suffix, suffixColor);
        Disable = false;
#endif
    }
    public CustomLogger(Object pingObj) {
#if ENABLE_CUSTOM_LOGGER_BUILD
        PingObj = pingObj;
        Prefix = "[" + PingObj.GetType().Name + "] "; Suffix = "";
        Disable = false;
#endif
    }
    public CustomLogger(Object pingObjTypeAsSuffix, Color prefixColor) {
#if ENABLE_CUSTOM_LOGGER_BUILD
        PingObj = pingObjTypeAsSuffix;
        Prefix = "[" + pingObjTypeAsSuffix.GetType().Name + "] "; 
        Prefix = LogUtil.Color(Prefix, prefixColor);
        Suffix = "";
        Disable = false;
#endif
    }
    public CustomLogger(Object pingObj, Color prefixColor, string prefix) {
#if ENABLE_CUSTOM_LOGGER_BUILD
        PingObj = pingObj;
        Prefix = LogUtil.Color(prefix, prefixColor);
        Suffix = "";
        Disable = false;
#endif
    }

    [HideInCallstack]
    public string Format(string msg) {
#if ENABLE_CUSTOM_LOGGER_BUILD
        Prefix ??= "";
        Suffix ??= ""; 
        return Prefix + msg + Suffix;
#else
        return "";
#endif
    }

    [Conditional("ENABLE_CUSTOM_LOGGER_BUILD"), HideInCallstack]
    public void Log(string msg) {
#if ENABLE_CUSTOM_LOGGER_BUILD
        if (Disable)
            return;
        msg = Format(msg);
        if (PingObj)
            Debug.Log(msg, PingObj);
        else Debug.Log(msg);
#endif
    }
    [HideInCallstack]
    public void LogError(string msg) {
#if ENABLE_CUSTOM_LOGGER_BUILD 
        msg = Format(msg);
        if (PingObj)
            Debug.LogError(msg, PingObj);
        else Debug.LogError(msg);
#else
        Debug.LogError(msg);
#endif
    }
	public void LogWarning(string msg) {
#if ENABLE_CUSTOM_LOGGER_BUILD
        if (Disable)
            return;
        msg = Format(msg);
        if (PingObj)
            Debug.LogWarning(msg, PingObj);
        else Debug.LogWarning(msg);
#endif
    }
    [Conditional("ENABLE_CUSTOM_LOGGER_BUILD"), HideInCallstack] 
    public void Log(Object pingOverride, string msg) {
#if ENABLE_CUSTOM_LOGGER_BUILD
        if (Disable)
            Debug.Log(Format(msg), pingOverride);
#endif
    }
    [HideInCallstack]
    public void LogException(System.Exception ex) {
#if ENABLE_CUSTOM_LOGGER_BUILD
        Debug.LogException(ex, PingObj);
#else
        Debug.LogException(ex);
#endif
    }

}
