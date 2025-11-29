using UnityEditor;
using UnityEngine;
static public class GizmosU 
{  
    public static void GizmosArrow(in Vector3 pos, in Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Arrow(true, pos, direction, Gizmos.color, arrowHeadLength, arrowHeadAngle);
    }
    public static void GizmosArrow(in Vector3 pos, in Vector3 direction, in Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Arrow(true, pos, direction, color, arrowHeadLength, arrowHeadAngle);
    }
    public static void DebugArrow(in Vector3 pos, in Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        DebugArrow(pos, direction, Gizmos.color, arrowHeadLength = 0.25f, arrowHeadAngle = 20.0f);
    }
    public static void DebugArrow(in Vector3 pos, in Vector3 direction, in Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Debug.DrawRay(pos, direction, color);
        Arrow(false, pos, direction, color, arrowHeadLength, arrowHeadAngle);
    } 
    static void Arrow(bool isDebug, in Vector3 pos, in Vector3 direction, in Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        var right = Quaternion.LookRotation(direction) * Quaternion.Euler(arrowHeadAngle, 0, 0) * Vector3.back * arrowHeadLength;
        var left = Quaternion.LookRotation(direction) * Quaternion.Euler(-arrowHeadAngle, 0, 0) * Vector3.back * arrowHeadLength;
        var up = Quaternion.LookRotation(direction) * Quaternion.Euler(0, arrowHeadAngle, 0) * Vector3.back * arrowHeadLength;
        var down = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -arrowHeadAngle, 0) * Vector3.back * arrowHeadLength;
        var end = pos + direction;
        Color colorPrew = Gizmos.color;

        if (isDebug)
        {
            Debug.DrawRay(end, right, colorPrew);
            Debug.DrawRay(end, left, colorPrew);
            Debug.DrawRay(end, up, colorPrew);
            Debug.DrawRay(end, down, colorPrew); 
        }
        else
        {
            Gizmos.color = color;
            Gizmos.DrawRay(pos, direction);
            Gizmos.DrawRay(end, right);
            Gizmos.DrawRay(end, left);
            Gizmos.DrawRay(end, up);
            Gizmos.DrawRay(end, down);
            Gizmos.color = colorPrew;
        } 
    }



}
#if UNITY_EDITOR
static public class HandlesU
{
    public static void DrawBoneHandle(Vector3 from, Vector3 to, float fatness = 1f, float width = 1f, float arrowOffset = 1f, float lineWidth = 1f, float fillAlpha = 0f)
    {
        if(from == to)
            to = from + Vector3.forward * 0.01f;

        Vector3 dir = (to - from);
        Vector3 forward = dir.normalized;
        float ratio = dir.magnitude / 7f; ratio *= fatness;
        float baseRatio = ratio * 0.75f * arrowOffset;
        ratio *= width;
        Quaternion rot = (dir == Vector3.zero ? rot = Quaternion.identity : rot = Quaternion.LookRotation(dir, forward));
        dir.Normalize();

        Vector3 p = from + dir * baseRatio;

        if (lineWidth <= 1f)
        {
            Handles.DrawLine(from, to);
            Handles.DrawLine(to, p + rot * Vector3.right * ratio);
            Handles.DrawLine(from, p + rot * Vector3.right * ratio);
            Handles.DrawLine(to, p - rot * Vector3.right * ratio);
            Handles.DrawLine(from, p - rot * Vector3.right * ratio);
        }
        else
        {
            Handles.DrawAAPolyLine(lineWidth, from, to);
            Handles.DrawAAPolyLine(lineWidth, to, p + rot * Vector3.right * ratio);
            Handles.DrawAAPolyLine(lineWidth, from, p + rot * Vector3.right * ratio);
            Handles.DrawAAPolyLine(lineWidth, to, p - rot * Vector3.right * ratio);
            Handles.DrawAAPolyLine(lineWidth, from, p - rot * Vector3.right * ratio);
        }

        if (fillAlpha > 0f)
        {
            Color preC = Handles.color;
            Handles.color = new Color(preC.r, preC.g, preC.b, fillAlpha * preC.a);
            Handles.DrawAAConvexPolygon(from, p + rot * Vector3.right * ratio, to, p - rot * Vector3.right * ratio, from);
            Handles.color = preC;
        }
    }
}
#endif
