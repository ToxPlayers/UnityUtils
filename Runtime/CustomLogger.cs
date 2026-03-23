using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public struct CustomLogger {
#if UNITY_EDITOR
    public Object PingObj; 
    public string Prefix; 
    public string Suffix;
#endif
    public CustomLogger(Object pingObj, Color prefixColor, string prefix, Color suffixColor, string suffix) {
#if UNITY_EDITOR
        PingObj = pingObj;
        Prefix = LogUtil.Color(prefix, prefixColor);
        Suffix = LogUtil.Color(suffix, suffixColor);
#endif
    }
    public CustomLogger(Object pingObj) {
#if UNITY_EDITOR
        PingObj = pingObj;
        Prefix = ""; Suffix = "";
#endif
    }
    public CustomLogger(Object pingObj, Color prefixColor, string prefix) {
#if UNITY_EDITOR
        PingObj = pingObj;
        Prefix = LogUtil.Color(prefix, prefixColor);
        Suffix = "";
#endif
    }

    [HideInCallstack]
    public string Format(string msg) {
#if UNITY_EDITOR
        Prefix ??= "";
        Suffix ??= ""; 
        return Prefix + msg + Suffix;
#else
        return "";
#endif
    }

    [HideInCallstack]
    public void Log(string msg) {
#if UNITY_EDITOR 
        msg = Format(msg);
        if (PingObj)
            Debug.Log(msg, PingObj);
        else Debug.Log(msg);
#endif
    }
    [HideInCallstack]
    public void Log(string msg, Object ping) => Debug.Log(Format(msg), ping);

    [HideInCallstack]
    public void LogException(System.Exception ex) {
        Debug.LogException(ex, PingObj);
    }

}
