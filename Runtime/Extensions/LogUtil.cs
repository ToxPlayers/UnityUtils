using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using System.Diagnostics;
using UColor = UnityEngine.Color;

static public class LogUtil 
{
	static public readonly string ColorEnd = "</color>";

	private static readonly UColor methodLogColor = new UColor(0, 0.7f, 0.1f);
	private static readonly UColor typeLogColor = new UColor(0, 1f, 0.1f);
	private static readonly UColor lineLogColor = new UColor(0, 0.5f, 0.4f);
	private static readonly UColor RedSoft = new UColor(1f, 0.29f ,0.39f);
	private static readonly UColor Red = UColor.red;
	private static readonly UColor Green = UColor.green;
    static public string ColorStart(UColor color) 
		=>"<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">";
	static public string ColorGreen(string str) => Color(str, Green);
	static public string ColorRed(string str) => Color(str, UColor.red);
	static public string ColorRedSoft(string str) => Color(str, RedSoft);
	static public string Color(string str, UColor color)
		=> ColorStart(color) + str + ColorEnd;
	public static string KiloFormat(this int num)
    {
        if (num >= 100000000)
            return (num / 1000000).ToString("#,0M");

        if (num >= 10000000)
            return (num / 1000000).ToString("0.#") + "M";

        if (num >= 100000)
            return (num / 1000).ToString("#,0K");

        if (num >= 10000)
            return (num / 1000).ToString("0.#") + "K";

        return num.ToString("#,0");
    }
    static public string GetStackLog(int skipFrames = 1, int maxFrameCount = 10 , bool useHyperLink = true)
	{  
		var frames = new StackTrace(skipFrames , true).GetFrames();
		string traceStr = "";
#if UNITY_EDITOR
		if(useHyperLink)
		{
            var filePath = frames[0].GetFileName();
            filePath = filePath.Replace( '\\' , '/');
            filePath = filePath.TrimStartUntil("/Assets/");
            var lineNumber = frames[0].GetFileLineNumber();
			var href = $"href=\"{filePath}\""; 
			var lineRef = $"\" line=\"{lineNumber}\"";
			var hyperLink = $"<a {href} {lineRef}> {filePath}:{lineNumber} </a>"; 
            traceStr += hyperLink + "\n";
        }
#endif 
        for (int i = 0; i < frames.Length && i < maxFrameCount; i++) 
			traceStr += '\n' + Frame(frames[i]); 

        return traceStr + '\n';
	} 
	 

    static public string Frame(StackFrame frame , UColor typeColor , UColor methodColor , UColor lineColor)
	{ 
		return Color(frame.GetMethod().DeclaringType.FullName, typeColor)+
                Color('.'+frame.GetMethod().Name + "() : ", methodColor)+
                Color("line " + frame.GetFileLineNumber().ToString(), lineColor);
	}
	static public string Frame(StackFrame frame)
	{
		return Frame(frame , typeLogColor , methodLogColor, lineLogColor);
	}

	static public string ToStringFormat(this float f, int maxChars)
	{
		var format = "0.";
		for (int i = 0; i < maxChars; i++)
			format += '0';
		return f.ToString(format);
	}

    [Conditional("UNITY_ASSERTIONS")]
    public static void Assert(object log, LogType logType)
    {
		UnityEngine.Debug.unityLogger.Log(logType, log);
    }
}
