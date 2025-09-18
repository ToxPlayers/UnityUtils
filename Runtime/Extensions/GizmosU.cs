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
    static void Arrow(bool isGizmos, in Vector3 pos, in Vector3 direction, in Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        var right = Quaternion.LookRotation(direction) * Quaternion.Euler(arrowHeadAngle, 0, 0) * Vector3.back * arrowHeadLength;
        var left = Quaternion.LookRotation(direction) * Quaternion.Euler(-arrowHeadAngle, 0, 0) * Vector3.back * arrowHeadLength;
        var up = Quaternion.LookRotation(direction) * Quaternion.Euler(0, arrowHeadAngle, 0) * Vector3.back * arrowHeadLength;
        var down = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -arrowHeadAngle, 0) * Vector3.back * arrowHeadLength;
        var end = pos + direction;
        Color colorPrew = Gizmos.color;

        if (isGizmos)
        {
            Gizmos.color = color;
            Gizmos.DrawRay(pos, direction);
            Gizmos.DrawRay(end, right);
            Gizmos.DrawRay(end, left);
            Gizmos.DrawRay(end, up);
            Gizmos.DrawRay(end, down);
            Gizmos.color = colorPrew;
        }
        else
        {
            Debug.DrawRay(end, right, colorPrew);
            Debug.DrawRay(end, left, colorPrew);
            Debug.DrawRay(end, up, colorPrew);
            Debug.DrawRay(end, down, colorPrew);
        } 
    } 
}
