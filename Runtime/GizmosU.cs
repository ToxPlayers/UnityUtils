using UnityEngine;

static public class GizmosU
{
    static public void DrawCircle(Vector3 circleCenter, Vector3 circleNormal, float circleRadius, int segments = 32)
    {
        Vector3 radiusVector = Mathf.Abs(Vector3.Dot(circleNormal, Vector3.right)) - 1f <= Mathf.Epsilon
            ? Vector3.Cross(circleNormal, Vector3.forward).normalized
            : Vector3.Cross(circleNormal, Vector3.right).normalized;
        radiusVector *= circleRadius;
        float angleBetweenSegments = 360f / segments;
        Vector3 previousCircumferencePoint = circleCenter + radiusVector;
        for (int i = 0; i < segments; ++i)
        {
            radiusVector = Quaternion.AngleAxis(angleBetweenSegments, circleNormal) * radiusVector;
            Vector3 newCircumferencePoint = circleCenter + radiusVector;
            Gizmos.DrawLine(previousCircumferencePoint, newCircumferencePoint);
            previousCircumferencePoint = newCircumferencePoint;
        }
    }
}
