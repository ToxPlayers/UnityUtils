using UnityEngine;
using Conditional = System.Diagnostics.ConditionalAttribute;


[System.Serializable]
public struct CustomLogger {
    public Object PingObj;
    public string Prefix;
    public string Suffix;
    public bool AlwaysDisabled;
    public bool DisableInBuild;
    public readonly bool IsDisabled => AlwaysDisabled || (DisableInBuild && !Application.isEditor);
    public CustomLogger(Object pingObj, Color prefixColor, string prefix, Color suffixColor, string suffix) {
        PingObj = pingObj;
        Prefix = LogUtil.Color(prefix, prefixColor);
        Suffix = LogUtil.Color(suffix, suffixColor);
        DisableInBuild = false; AlwaysDisabled = false;
    }
    public CustomLogger(Object pingObj) {
        PingObj = pingObj;
        Prefix = "[" + PingObj.GetType().Name + "] "; Suffix = "";
        DisableInBuild = false; AlwaysDisabled = false;
    }
    public CustomLogger(Object pingObjTypeAsSuffix, Color prefixColor) {
        PingObj = pingObjTypeAsSuffix;
        Prefix = "[" + pingObjTypeAsSuffix.GetType().Name + "] ";
        Prefix = LogUtil.Color(Prefix, prefixColor);
        Suffix = "";
        DisableInBuild = false; AlwaysDisabled = false;
    }
    public CustomLogger(Object pingObj, Color prefixColor, string prefix) {
        PingObj = pingObj;
        Prefix = LogUtil.Color(prefix, prefixColor);
        Suffix = "";
        DisableInBuild = false; AlwaysDisabled = false;
    }

    [HideInCallstack]
    public string Format(string msg) {
        if (IsDisabled)
            return "";
        Prefix ??= "";
        Suffix ??= "";
        return Prefix + msg + Suffix;
    }
    [HideInCallstack]
    public void Log(string msg) => Log(msg, PingObj);
    [HideInCallstack]
    public void Log(string msg, Object ping) {
        if (IsDisabled)
            return;
        msg = Format(msg);
        if (ping)
            Debug.Log(msg, ping);
        else Debug.Log(msg);
    }
    [HideInCallstack]
    public void LogWarning(string msg) => LogWarning(msg, PingObj);
    [HideInCallstack]
    public void LogWarning(string msg, Object ping) {
        if (IsDisabled)
            return;
        msg = Format(msg);
        if (ping)
            Debug.LogWarning(msg, ping);
        else Debug.LogWarning(msg);
    }
    [HideInCallstack]
    public void LogError(string msg) => LogError(msg, PingObj);
    [HideInCallstack]
    public void LogError(string msg, Object ping) {
        if (IsDisabled)
            return;
        msg = Format(msg);
        if (ping)
            Debug.LogError(msg, ping);
        else Debug.LogError(msg);
    }
    [HideInCallstack]
    public readonly void LogException(System.Exception ex) => LogException(ex, PingObj);
    [HideInCallstack]
    public readonly void LogException(System.Exception ex, Object ping) {
        Debug.LogException(ex, ping);
    }
}