using System;
using System.Net.Mail;
using UnityEditor;
using UnityEngine;
static public class GizmosU 
{  
    public static void GizmosArrow(in Vector3 pos, in Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Arrow(false, pos, direction, Gizmos.color, arrowHeadLength, arrowHeadAngle);
    }
    public static void GizmosArrow(in Vector3 pos, in Vector3 direction, in Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Arrow(false, pos, direction, color, arrowHeadLength, arrowHeadAngle);
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
    static void Arrow(bool isDebug, Vector3 pos, Vector3 direction, in Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        //var matrix = Gizmos.matrix;
        //pos = matrix.MultiplyPoint3x4(pos);
        //direction = matrix.rotation * direction; 
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
    public static void GizmosRotation(Vector3 pos, Quaternion rotation,in Color sphereColor, in float size) {
        GizmosRotation(pos, rotation, size, sphereColor, Color.darkRed, Color.darkGreen, Color.darkBlue);
    }

    public static void GizmosRotation(Vector3 pos, Quaternion rotation, in float size, in Color sphereColor, in Color rightColor, in Color upColor, in Color forwardColor) {

        var arrowLen = size * 0.2f;
        GizmosArrow(pos, rotation * Vector3.right * size, rightColor, arrowLen);
        GizmosArrow(pos, rotation * Vector3.up * size, upColor, arrowLen);
        GizmosArrow(pos, rotation * Vector3.forward * size, forwardColor, arrowLen);

        var prevColor = Gizmos.color;
        Gizmos.color = sphereColor;
        var prevMatrix = Gizmos.matrix;
        Gizmos.matrix *= Matrix4x4.TRS(pos, rotation, Vector3.one);
        Gizmos.DrawWireSphere(Vector3.zero, size * 0.8f); 
        Gizmos.color = prevColor;
        Gizmos.matrix = prevMatrix;
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
