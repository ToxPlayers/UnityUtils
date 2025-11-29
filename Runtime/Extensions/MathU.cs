using System;
using System.Collections.Generic;
using System.Linq; 
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

static public class MathU
{
    static readonly Matrix4x4 MatOne = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
    static public readonly Vector3 ZeroVector3 = new();
    static public readonly Vector2 ZeroVector2 = new();

    const int INLINED = (int)MethodImplOptions.AggressiveInlining;

    #region Random
    static Random _pureRandom = new Random();
    static int randomUsages;
    public static int PureRandom
    {
        get
        {
            lock (_pureRandom)
            {
                randomUsages++;
                if (randomUsages >= 20)
                    _pureRandom = new();
                return _pureRandom.Next();
            }
        }
    }
    #endregion

    #region Floats & Ints

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public int WrapIndex(int index, int length)
    {
        return (index % length + length) % length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public Vector3 AsNormalizedDirection(this Quaternion rotation) => rotation * Vector3.forward;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public float FractionalPart(this float f) => f - (int)f;
    static public int AsInt<TValue>(this TValue value) where TValue : Enum => (int)(object)value;
    static public bool IsMask(this int maskOrLayer) => maskOrLayer != 0 && maskOrLayer.BitsSetCount() > 1;

    [BurstCompile]
    public static bool IsAlmostZero(this float number)
    {
        return math.abs(number) <= 0.00000003f;
    }

    [BurstCompile]
    public static bool IsAlmostEqualTo(this float numberA, float numberB, float tolerance = /* SmallNumber */ 0.00000001f)
    {
        return math.abs(numberA - numberB) <= tolerance;
    }

    static public float TimeLerp(float lerpAmount, float timeStarted)
    {
        return Mathf.Clamp01(Mathf.InverseLerp(timeStarted + lerpAmount, timeStarted, Time.time));
    }

    static public void Clamp(this int[] arr, int min, int max)
    {
        for (int i = 0; i < arr.Length; i++)
            arr[i] = Mathf.Clamp(arr[i], min, max);
    }
    static public void SafeDestroyChildren(this Transform tf)
    {
        //done this way to fix infinite looping when destroying transform isn't immdiate
        var count = tf.childCount;
        var list = new List<Transform>(count);
        for (int i = 0; i < count; i++)
            list.Add(tf.GetChild(i));

        foreach (var child in list)
            child.SafeDestroy();
    }

    static public bool IsLowerAndNotApprox(float f1, float f2)
       => (!Mathf.Approximately(f1, f1)) && f1 < f2;
    static public bool IsHigherAndNotApprox(float f1, float f2)
       => (!Mathf.Approximately(f1, f1)) && f1 > f2;
    internal static List<T> GetEnumValues<T>() where T : Enum
    {
        var type = typeof(T);
        var fields = type.GetFields((System.Reflection.BindingFlags)(-1));
        var list = new List<T>();
        foreach (var field in fields)
            if (field.IsLiteral)
                list.Add((T)field.GetValue(null));
        return list;
    }

    public static Vector2 ClosestCircleEdge(Vector2 circleCenter, float radius, Vector2 point)
    {
        Vector2 direction = point - circleCenter; // vector pointing from center of circle to point
        direction.Normalize(); // normalize to get unit vector pointing towards point
        return circleCenter + (direction * radius); // point on edge of circle closest to point
    }
    static public IEnumerable<Vector2> EnumRadius(float radiusPointCount, float pointSize)
    {
        Vector2Int posI = new Vector2Int(0, 0);
        Vector2Int zeroCenter = new Vector2Int(0, 0);
        for (; posI.x < radiusPointCount; posI.x++)
        {
            posI.y = 0;
            if (Vector2Int.Distance(posI, zeroCenter) > radiusPointCount)
                break;
            for (; posI.y < radiusPointCount; posI.y++)
            {
                if (Vector2Int.Distance(posI, zeroCenter) > radiusPointCount)
                    break;
                var xPos = posI.x * pointSize;
                var zPos = posI.y * pointSize;
                yield return new Vector2(xPos, zPos);
                if (posI.x != 0)
                    yield return new Vector2(-xPos, zPos);
                if (posI.y != 0)
                    yield return new Vector2(xPos, -zPos);
                if (posI.x != 0 && posI.y != 0)
                    yield return new Vector2(-xPos, -zPos);

            }
        }
        //for (; posI.x < radiusPointCount; posI.x++)
        //{
        //    posI.y = 0; 
        //    if (Vector2Int.Distance(posI, zeroCenter) > radiusPointCount)
        //        break;
        //    for (; posI.y < radiusPointCount ; posI.y++)
        //    {
        //        if ( Vector2Int.Distance(posI , zeroCenter) > radiusPointCount)
        //                break; 
        //        var xPos = posI.x * pointSize;
        //        var zPos = posI.y * pointSize;
        //        yield return new Vector2(xPos, zPos); 
        //        if(posI.x != 0)
        //            yield return new Vector2(-xPos, zPos);
        //        if (posI.y != 0)
        //            yield return new Vector2(xPos, -zPos);
        //        if (posI.x != 0 && posI.y != 0)
        //            yield return new Vector2(-xPos, -zPos);

        //    }
        //} 
    }
    static public void SetPosition(this ref Matrix4x4 mat, Vector3 pos)
    {
        mat[0, 3] = pos.x;
        mat[1, 3] = pos.y;
        mat[2, 3] = pos.z;
    }

    internal static List<Vector2> ResizePolygon(List<Vector2> verts, Vector2 center, float scale)
    {
        var resized = new List<Vector2>(verts.Count);
        var count = resized.Count;
        for (int i = 0; i < count; i++)
            resized[i] = Vector2.Lerp(resized[i], center, scale);

        return resized;
    }

    [MethodImpl(INLINED)]
    public static bool IsValidMinMax(in Vector2Int MinMax, float value)
    {
        return value >= MinMax.x && value <= MinMax.y;
    }


    [MethodImpl(INLINED)]
    public static bool ValidMinMax(in Vector2Int MinMax, int value)
    {
        return value >= MinMax.x && value <= MinMax.y;
    }

    [MethodImpl(INLINED)]
    public static bool IsValidMinMax(in Vector2 MinMax, float value)
    {
        return value >= MinMax.x && value <= MinMax.y;
    }

    [MethodImpl(INLINED)]
    static public Vector3 ToV3(this in Color color)
        => new Vector3()
        {
            x = color.r,
            y = color.g,
            z = color.b
        };

    [MethodImpl(INLINED)]
    static public float GetV(this in Color color)
    {
        Color.RGBToHSV(color, out _, out _, out float v);
        return v;
    }
    [MethodImpl(INLINED)]
    static public float GetS(this in Color color)
    {
        Color.RGBToHSV(color, out _, out float s, out _);
        return s;
    }
    [MethodImpl(INLINED)]
    static public float GetH(this in Color color)
    {
        Color.RGBToHSV(color, out float h, out _, out _);
        return h;
    }
    [MethodImpl(INLINED)]
    static public Color OppositeColor(this in Color color)
    {
        return new Color(1 - color.r, 1 - color.g, 1 - color.b, color.a);
    }
    [MethodImpl(INLINED)]
    static public Vector3 ToFloat(this in Vector3Int vec2Int) => new Vector3(vec2Int.x, vec2Int.y, vec2Int.z);
    [MethodImpl(INLINED)]
    static public Vector2 ToFloat(this in Vector2Int vec2Int) => new Vector2(vec2Int.x, vec2Int.y);
    [MethodImpl(INLINED)]
    static public Vector3Int RoundToInt(this in Vector3 v3)
        => new Vector3Int() { x = Mathf.RoundToInt(v3.x), y = Mathf.RoundToInt(v3.y), z = Mathf.RoundToInt(v3.z) };
    [MethodImpl(INLINED)]
    static public Vector3Int CeilToInt(this in Vector3 v3) => new(Mathf.CeilToInt(v3.x), Mathf.CeilToInt(v3.y), Mathf.CeilToInt(v3.z));
    [MethodImpl(INLINED)]
    static public Vector3Int FloorToInt(this in Vector3 v3) => new(Mathf.FloorToInt(v3.x), Mathf.FloorToInt(v3.y), Mathf.FloorToInt(v3.z));
    [MethodImpl(INLINED)]
    static public Vector2Int RoundToInt(this in Vector2 v2)
        => new Vector2Int() { x = Mathf.RoundToInt(v2.x), y = Mathf.RoundToInt(v2.y) };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Cross(this Vector2 v1, Vector2 v2)
    {
        return v1.x * v2.y
               - v1.y * v2.x;
    }
    [MethodImpl(INLINED)]
    static public Vector2Int Round(this in Vector2 v)
        => new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
    [MethodImpl(INLINED)]
    static public void AddOrSet<Key, Val>(this Dictionary<Key, Val> dic, in Key key, in Val value)
    {
        if (dic.ContainsKey(key))
            dic[key] = value;
        else dic.Add(key, value);
    }
    [MethodImpl(INLINED)]
    static public Val TryGetOrAddDefault<Key, Val>(this Dictionary<Key, Val> dic, in Key key, in Val defaultVal)
    {
        if (dic.TryGetValue(key, out var value))
            return value;
        value = defaultVal;
        dic.Add(key, value);
        return value;
    }
    [MethodImpl(INLINED)]
    static public int RoundInt(this float f) => Mathf.RoundToInt(f);
    [MethodImpl(INLINED)]
    static public void SetZero(this ref Vector3 v) { v.x = 0f; v.y = 0f; v.z = 0f; }
    [MethodImpl(INLINED)]
    static public bool IsZero(this ref Vector3 v) => v.x == 0f && v.y == 0f && v.z == 0f;
    [MethodImpl(INLINED)]
    static public void SetZero(this ref Vector2 v) { v.x = 0f; v.y = 0f; }
    [MethodImpl(INLINED)]
    static public bool IsZero(this ref Vector2 v) => v.x == 0f && v.y == 0f;

    [MethodImpl(INLINED)]
    static public bool IsNanOrInifinity(this in Vector3 v)
        => float.IsNaN(v.x) || float.IsInfinity(v.x)
        || float.IsNaN(v.y) || float.IsInfinity(v.y)
        || float.IsNaN(v.z) || float.IsInfinity(v.z);
    [MethodImpl(INLINED)]
    static public bool IsNanOrInifinity(this in Vector2 v)
        => float.IsNaN(v.x) || float.IsInfinity(v.x)
        || float.IsNaN(v.y) || float.IsInfinity(v.y);

    [BurstCompile]
    public static float Remap(this in float val, in float fromMin, float fromMax, in float toMin, in float toMax)
    {
        if (fromMax - fromMin == 0)
            fromMax = 0.0000001f;
        return (toMax - toMin) * (val - fromMin) / (fromMax - fromMin) + toMin;
    }


    [BurstCompile]
    public static float Remap(this int val, in float fromMin, float fromMax, in float toMin, in float toMax)
    {
        if (fromMax - fromMin == 0)
            fromMax = 0.0000001f;
        return (toMax - toMin) * (val - fromMin) / (fromMax - fromMin) + toMin;
    }
    [MethodImpl(INLINED), BurstCompile]
    public static float RemapTo01(this in float val, in float fromMin, float fromMax)
    {
        if (fromMax - fromMin == 0)
            fromMax = 0.000001f;
        return (val - fromMin) / (fromMax - fromMin);
    }


    #endregion

    #region Vectors  
    public const int XIndex = 0, YIndex = 1, ZIndex = 2, WIndex = 3;
    public const int RightIndex = XIndex, UpIndex = YIndex, ForwardIndex = ZIndex;
    /// <summary>
    /// Calculates the normalized projection of the Vector3 'vec'
    /// onto the horizontal plane defined by the orthogonal vector (0, 1, 0)
    /// </summary>
    /// <param name="vec">The vector to project</param>
    /// <returns>The normalized projection of 'vec' onto the horizontal plane</returns>
    public static Vector3 GetFloorProjection(in Vector3 vec)
    {
        return Vector3.ProjectOnPlane(vec, Vector3.up).normalized;
    }
    [MethodImpl(INLINED)]
    static public Quaternion InverseTransformRotation(this Transform tf, in Quaternion rotation) => Quaternion.Inverse(tf.rotation) * rotation;
    [MethodImpl(INLINED)]
    static public Quaternion Inverse(this in Quaternion quaternion) => Quaternion.Inverse(quaternion);
    [BurstCompile]
    static public NativeArray<float3> CreateAsFloat3(this NativeArray<float2> from)
    {
        var arr3 = new NativeArray<float3>(from.Length, Allocator.Persistent);
        CopyToFloat3(from, arr3);
        return arr3;
    }
    public static float GetDominant(this Vector3 v3) => v3[v3.GetDominantAxis()];
    public static int GetDominantAxis(this Vector3 v3)
    {
        var v3Abs = v3.Abs();
        int maxIdx = 0;
        for (int i = 1; i < 3; i++)
            if (v3Abs[i] > v3Abs[maxIdx])
                maxIdx = i;
        return maxIdx;
    }
    public static Vector3 ExtractDominant(this Vector3 v3)
    {
        var max = GetDominantAxis(v3.Abs());
        for (int i = 0; i < 3; i++)
            if (i != max)
                v3[i] = 0;
        return v3;
    }
    static public JointDrive Multiplied(this JointDrive joint, float mult)
    {
        joint.maximumForce *= mult;
        joint.positionDamper *= mult;
        joint.positionSpring *= mult;
        return joint;
    }

    [BurstCompile]
    static public void CopyToFloat3(this NativeArray<float2>.ReadOnly from, NativeArray<float3> to)
        => CopyToFloat3(from, to, 0, to.Length);
    [BurstCompile]
    static public void CopyToFloat3(this NativeArray<float2>.ReadOnly from, NativeArray<float3> to, int startIndex, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var f2 = from[i];
            to[i + startIndex] = new float3() { x = f2.x, y = 0, z = f2.y };
        }
    }
    [BurstCompile]
    static public void CopyToFloat3(this NativeArray<float2> from, NativeArray<float3> to)
    => CopyToFloat3(from, to, 0, to.Length);
    [BurstCompile]
    static public void CopyToFloat3(this NativeArray<float2> from, NativeArray<float3> to, int startIndex, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var f2 = from[i];
            to[i + startIndex] = new float3() { x = f2.x, y = 0, z = f2.y };
        }
    }

    [BurstCompile]
    static public float3 GetTriangleNormal(in float3 A, in float3 B, in float3 C)
    {
        var AB = B - A;
        var AC = C - A;
        var normal = math.normalize(math.cross(AB, AC));
        return new float3
        {
            x = normal.x,
            y = normal.y,
            z = normal.z
        };
    }

    [BurstCompile]
    static public float3 GetQuadNormal(in float3 A, in float3 B, in float3 C, in float3 D, out float angle)
    {
        var normalA = math.cross(C, A);
        var normalB = math.cross(D, B);
        angle = Angle(normalA, normalB);
        return math.normalize(normalA + normalB);
    }

    [BurstCompile]
    public static float Angle(in float3 from, in float3 to)
    {
        return math.degrees(math.acos(math.dot(math.normalize(from), math.normalize(to))));
    }
    public static void InCenter(in float2 p1, in float2 p2, in float2 p3, out float2 inCenter, out float inRadius)
    {
        var a = math.distance(p1, p2);
        var b = math.distance(p2, p3);
        var c = math.distance(p3, p1);

        var perimeter = (a + b + c);
        var x = (a * p1.x + b * p2.x + c * p3.x) / perimeter;
        var y = (a * p1.y + b * p2.y + c * p3.y) / perimeter;
        inCenter = new float2(x, y);

        var s = perimeter / 2;
        var triangleArea = math.sqrt(s * (s - a) * (s - b) * (s - c));
        inRadius = triangleArea / s;
    }




    static public Color ToColor(this in Vector3 v) => new Color(v.x, v.y, v.z);
    [MethodImpl(INLINED)]
    static public bool Approximately(in Vector2 v1, in Vector2 v2)
   => Mathf.Approximately(v1.x, v2.x) && Mathf.Approximately(v1.y, v2.y);

    [MethodImpl(INLINED)]
    static public Vector2 ClampValues(this in Vector2 toClamp, float maxX, float maxY)
    => new Vector2
        (
           Mathf.Clamp(toClamp.x, 0, maxX),
           Mathf.Clamp(toClamp.y, 0, maxY)
        );
    [MethodImpl(INLINED)]

    static public Vector2 ClampMinValues(this in Vector2 toClamp, in Vector2 minClamp)
       => new Vector2()
       {
           x = Mathf.Clamp(toClamp.x, minClamp.x, float.MaxValue),
           y = Mathf.Clamp(toClamp.y, minClamp.y, float.MaxValue),
       };
    static public Vector2 ClampedMinMaxValues(this Vector2 toClamp, Vector2 minClamp, Vector2 maxClamp)
        => new Vector2()
        {
            x = Mathf.Clamp(toClamp.x, minClamp.x, maxClamp.x),
            y = Mathf.Clamp(toClamp.y, minClamp.y, maxClamp.y),
        };
    static public Vector3 ClampMinValues(this Vector3 toClamp, Vector3 minClamp)
        => ClampV3(toClamp, minClamp, new Vector3(float.MaxValue, float.MaxValue, float.MaxValue));
    static public Vector3 ClampV3(this in Vector3 toClamp, Vector3 minClamp, Vector3 maxClamp)
    => new Vector3()
    {
        x = Mathf.Clamp(toClamp.x, minClamp.x, maxClamp.x),
        y = Mathf.Clamp(toClamp.y, minClamp.y, maxClamp.y),
        z = Mathf.Clamp(toClamp.z, minClamp.z, maxClamp.z)
    };
    public static Vector2 Rotate(this Vector2 v, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }
    static public void AngleTo(this in Quaternion rot, in Quaternion target, out float angle, out Vector3 axis)
    {
        Quaternion rotation = Quaternion.Inverse(rot) * target;
        rotation.ToAngleAxis(out angle, out axis);
    }
    static public Quaternion Between(this in Quaternion rot, in Quaternion subtract)
    {
        return Quaternion.Inverse(subtract) * rot;
    }

    static public Rect GetNearestRect(Vector2 posXZ, float cellSize, Vector2 cellCenter)
    {
        var rightDist = Mathf.Abs(posXZ.x - cellCenter.x);
        var topDis = Mathf.Abs(posXZ.y - cellCenter.y);
        Rect neighborRect = new() { size = new Vector2(cellSize, cellSize), center = cellCenter };
        if (rightDist > topDis)//is right or top
        {
            if (posXZ.x > cellCenter.x) // right
                neighborRect.x += cellSize;
            else //left
                neighborRect.x -= cellSize;
        }
        else
        {
            if (posXZ.y > cellCenter.y)//top
                neighborRect.y += cellSize;
            else//bottom
                neighborRect.y -= cellSize;
        }
        return neighborRect;
    }
    static public Vector2 TopCenter(this Rect rect) => rect.center + new Vector2(0, +rect.size.y / 2f);
    static public Vector2 BottomCenter(this Rect rect) => rect.center + new Vector2(0, -rect.size.y / 2f);

    [MethodImpl(INLINED)]
    public static Vector2 Abs(this in Vector2 v) => new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));
    [MethodImpl(INLINED)]
    public static Vector2 XZ(this in Vector3Int vv) => new Vector2(vv.x, vv.z);
    [MethodImpl(INLINED)]
    public static Vector2Int XZRoundToInt(this in Vector3 vv) => new Vector2Int(Mathf.RoundToInt(vv.x), Mathf.RoundToInt(vv.z));
    [MethodImpl(INLINED)]
    public static Vector2Int XZInt(this in Vector3Int vv) => new Vector2Int(vv.x, vv.z);
    [MethodImpl(INLINED)]
    public static Vector3Int XZToXYZInt(this in Vector2 v2) => new Vector3Int(Mathf.RoundToInt(v2.x), 0, Mathf.RoundToInt(v2.y));
    [MethodImpl(INLINED)]
    public static Vector3Int XZToXYZInt(this in Vector2 v2, int y) => new Vector3Int(Mathf.RoundToInt(v2.x), y, Mathf.RoundToInt(v2.y));
    [MethodImpl(INLINED)]
    public static Vector2 FlipYX(this in Vector2 v2) => new Vector2() { x = v2.y, y = v2.x };
    [MethodImpl(INLINED)]
    public static Vector2 XZ(this in Vector3 vv) => new Vector2(vv.x, vv.z);
    [MethodImpl(INLINED)]
    public static Vector3 XZToXYZ(this in Vector2 v2) => new Vector3(v2.x, 0, v2.y);

    [MethodImpl(INLINED)]
    public static Vector3Int XZToXYZInt(this in Vector2Int v2) => new Vector3Int(v2.x, 0, v2.y);
    [MethodImpl(INLINED)]
    public static Vector3 XZToXYZ(this in Vector2Int v2) => new Vector3(v2.x, 0, v2.y);
    [MethodImpl(INLINED)]
    public static Vector3 XZToXYZ(this in Vector2 v2, float Y) => new Vector3(v2.x, Y, v2.y);
    [MethodImpl(INLINED)]
    public static string Vector2Hash(this in Vector2 hash) => $"{hash.x},{hash.y}";
    [MethodImpl(INLINED)]
    public static Vector3 ToV3(this in Vector2 v2) => new Vector3(v2.x, v2.y, 0);
    [MethodImpl(INLINED)]
    static public Vector3 Abs(this in Vector3 v) => new Vector3() { x = Mathf.Abs(v.x), y = Mathf.Abs(v.y), z = Mathf.Abs(v.z) };



    [MethodImpl(INLINED)]
    public static Vector2 RadianToVector2(float radian) => new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));

    [MethodImpl(INLINED)]
    public static Vector2 DegreeToV2(float degree) => RadianToVector2(degree * Mathf.Deg2Rad);



    public static Vector2 NearestEdge(this in Rect rect, in Vector2 point)
    {
        Vector2 nearestEdgePoint = new Vector2();

        // calculate the distance to each edge of the square
        float topDistance = Math.Abs(point.y - rect.yMax);
        float bottomDistance = Math.Abs(point.y - rect.yMin);
        float leftDistance = Math.Abs(point.x - rect.xMin);
        float rightDistance = Math.Abs(point.x - rect.xMax);

        // determine the nearest edge
        float minDistance = Math.Min(topDistance, Math.Min(bottomDistance, Math.Min(leftDistance, rightDistance)));

        if (minDistance == topDistance)
        {
            nearestEdgePoint.x = point.x;
            nearestEdgePoint.y = rect.yMax;
        }
        else if (minDistance == bottomDistance)
        {
            nearestEdgePoint.x = point.x;
            nearestEdgePoint.y = rect.yMin;
        }
        else if (minDistance == leftDistance)
        {
            nearestEdgePoint.x = rect.xMin;
            nearestEdgePoint.y = point.y;
        }
        else if (minDistance == rightDistance)
        {
            nearestEdgePoint.x = rect.xMax;
            nearestEdgePoint.y = point.y;
        }

        return nearestEdgePoint;
    }

    #region Cells
    [MethodImpl(INLINED)]
    public static Vector3 GetCellCenter(in Vector3 pos, in float cellSize) => GetCellCenter(pos.x, pos.y, pos.z, cellSize);
    [MethodImpl(INLINED)]
    public static Vector3 GetCellCenter(in float x, in float y, in float z, in float cellSize)
    {
        return new Vector3()
        {
            x = GetCellCenter(x, cellSize),
            y = GetCellCenter(y, cellSize),
            z = GetCellCenter(z, cellSize)
        };
    }
    [MethodImpl(INLINED)]
    public static Vector3 GetCellMin(in Vector3 pos, in float cellSizeSqr)
    {
        return new Vector3()
        {
            x = GetCellMin(pos.x, cellSizeSqr),
            y = GetCellMin(pos.y, cellSizeSqr),
            z = GetCellMin(pos.z, cellSizeSqr)
        };
    }

    [MethodImpl(INLINED)]
    public static Vector3 GetCellMax(in Vector3 pos, in float cellSizeSqr)
    {
        return new Vector3()
        {
            x = GetCellMax(pos.x, cellSizeSqr),
            y = GetCellMax(pos.y, cellSizeSqr),
            z = GetCellMax(pos.z, cellSizeSqr)
        };
    }
    [MethodImpl(INLINED)]
    public static Vector2 GetCellCenter(in float posX, in float posZ, in float cellSize)
    {
        return new Vector2()
        {
            x = GetCellCenter(posX, cellSize),
            y = GetCellCenter(posZ, cellSize)
        };
    }
    [MethodImpl(INLINED)]
    public static float2 GetCellCenter(in float2 pos, in float cellSize)
    {
        return new float2()
        {
            x = GetCellCenter(pos.x, cellSize),
            y = GetCellCenter(pos.y, cellSize)
        };
    }
    [MethodImpl(INLINED)]
    public static Vector2 GetCellCenter(in Vector2 pos, in float cellSize)
    {
        return new Vector2()
        {
            x = GetCellCenter(pos.x, cellSize),
            y = GetCellCenter(pos.y, cellSize)
        };
    }
    [MethodImpl(INLINED)]
    public static Vector2 GetCellCenter(in Vector2 pos, in int cellSize)
    {
        return new Vector2()
        {
            x = GetCellCenterInt(pos.x, cellSize),
            y = GetCellCenterInt(pos.y, cellSize)
        };
    }

    [MethodImpl(INLINED)]
    public static Vector2 GetCellMin(in Vector2 pos, in int cellSizeSqr)
    {
        return new Vector2()
        {
            x = GetCellMinInt(pos.x, cellSizeSqr),
            y = GetCellMinInt(pos.y, cellSizeSqr)
        };
    }

    [MethodImpl(INLINED)]
    public static Vector2 GetCellMax(in Vector2 pos, in int cellSizeSqr)
    {
        return new Vector2()
        {
            x = GetCellMaxInt(pos.x, cellSizeSqr),
            y = GetCellMaxInt(pos.y, cellSizeSqr)
        };
    }
    #region float cells 
    [MethodImpl(INLINED)]
    public static float GetCellCenter(in float pos, in float cellSize)
    {
        return Mathf.Round(pos / cellSize) * cellSize;
    }
    [MethodImpl(INLINED)]
    public static float GetCellMax(in float pos, in float cellSize)
    {
        return GetCellCenter(pos, cellSize) + cellSize / 2f;
    }
    [MethodImpl(INLINED)]
    public static float GetCellMin(in float pos, in float cellSize)
    {
        return GetCellCenter(pos, cellSize) - cellSize / 2f;
    }
    #endregion 
    #region Int cells
    [MethodImpl(INLINED)]
    public static float GetCellCenterInt(in float pos, in int cellSize)
    {
        return Mathf.Round(pos / cellSize) * cellSize;
    }
    [MethodImpl(INLINED)]
    public static int GetCellCenterInt(in int pos, in int cellSize)
    {
        return pos / cellSize * cellSize;
    }
    [MethodImpl(INLINED)]
    public static float GetCellMaxInt(in float pos, in int cellSize)
    {
        return Mathf.Round(pos / cellSize) * cellSize + cellSize / 2f;
    }

    [MethodImpl(INLINED)]
    public static float GetCellMinInt(in float pos, in int cellSize)
    {
        return Mathf.Round(pos / cellSize) * cellSize - cellSize / 2f;
    }
    #endregion
    #endregion

    [MethodImpl(INLINED)]
    public static string V3Str(this in Vector3 v3) => $"{v3.x},{v3.y},{v3.z}";
    [MethodImpl(INLINED)]
    public static string V2Str(this in Vector2 v2) => $"{v2.x},{v2.y}";
    [MethodImpl(INLINED)]
    public static string XZStr(this in Vector3 v3) => $"{v3.x},{v3.z}";
    public static Vector2 XY(this in Vector3 v3) => new Vector2(v3.x, v3.y);
    public static List<Vector2> ToXZ(this List<Vector3> list)
    {
        var v2 = new List<Vector2>();
        for (int i = 0; i < list.Count; i++)
            v2.Add(list[i].XZ());
        return v2;
    }
    public static List<Vector2Int> ToInt(this List<Vector2> list)
    {
        var v2 = new List<Vector2Int>();
        for (int i = 0; i < list.Count; i++)
            v2.Add(list[i].Round());
        return v2;
    }
    #endregion

    #region Collections


    [MethodImpl(INLINED)]
    public static int RowColToIndex(int rowLen, int row, int col)
    {
        return row * rowLen + col;
    }

    [MethodImpl(INLINED)]
    public static Vector2Int IndexToRowCol(int rowLen, int index)
    {
        return new()
        {
            x = index / rowLen,
            y = index % rowLen
        };
    }
    [MethodImpl(INLINED)]
    public static int PositionToIndex(float cellSize, int rowLen, Vector2 pos)
    {
        var col = Mathf.RoundToInt(pos.y / cellSize);
        var row = Mathf.RoundToInt(pos.x / cellSize);
        return row * rowLen + col;
    }
     
    public static List<T> GetNameTagsInLocalChildren<T>(this Transform transform, string nameTag) where T : Component
    {
        List<T> list = new List<T>();
        int count = transform.childCount;
        for (int i = 0; i < count; i++)
        {
            Transform tf = transform.GetChild(i);
            T Tcomp = tf.GetComponent<T>();
            if (Tcomp != null && tf.name.Contains(nameTag))
                list.Add(Tcomp);
        }
        return list;
    }
    public static T GetComponentInFirstChildren<T>(this Transform transform) where T : Component
    {
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            T comp = transform.GetChild(i).GetComponent<T>();
            if (comp != null)
                return comp;
        }
        return null;
    }
    [MethodImpl(INLINED)]
    public static List<T> GetComponentsInFirstChildren<T>(this Transform transform) where T : Component
    {
        var list = new List<T>();
        int childCount = transform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            T comp = transform.GetChild(i).GetComponent<T>();
            if (comp != null)
                list.Add(comp);
        }

        return list;
    }


    [MethodImpl(INLINED)]
    public static T GetNameTagInAllChild<T>(this Transform transform, string nameTag) where T : Component
        => GetTypeInAllChild<T>(transform, nameTag, 0);
    [MethodImpl(INLINED)]
    public static T GetTypeInAllChild<T>(this Transform transform, string nameTag, int level) where T : Component
    {
        int childCount = transform.childCount;
        T localItem = transform.GetComponent<T>();
        if (localItem && transform.gameObject.name.Contains(nameTag))
            return localItem;

        for (int i = 0; i < childCount; i++)
        {
            T item = transform.GetChild(i).GetNameTagInAllChild<T>(nameTag);
            if (item && item.gameObject.name.Contains(nameTag))
                return item;
        }
        if (level > 0)
            Debug.LogWarning("Couldn't find by nameTag (" + nameTag + ")");
        return default(T);
    }
    static public Matrix4x4 MatrixOne { get => MatOne; }

    [MethodImpl(INLINED)]
    static public bool isInsideRect(in Vector2 bottomLeft, in Vector2 topRight, in Vector2 point)
    {
        return point.x >= bottomLeft.x && point.x <= topRight.x
                    && point.y >= bottomLeft.y && point.y <= topRight.y;
    }

    [MethodImpl(INLINED)]
    static public bool isInsideCircle(in Vector2 pos, in Vector2 circleCenter, float radius)
    {
        var dx = Mathf.Abs(pos.x - circleCenter.x);
        var dy = Mathf.Abs(pos.y - circleCenter.y);

        if (dx + dy <= radius) return true;
        if (dx > radius) return false;
        if (dy > radius) return false;
        if (Math.Pow(dx, 2) + Math.Pow(dy, 2) <= Math.Pow(radius, 2))
            return true;
        return false;
    }

    [MethodImpl(INLINED)]
    public static void CopyBlendShapesFrom(this SkinnedMeshRenderer otherRend, SkinnedMeshRenderer body)
    {
        int shapeCount1 = otherRend.sharedMesh.blendShapeCount;
        int shapeCount2 = body.sharedMesh.blendShapeCount;
        for (int i = 0; i < shapeCount1 && i < shapeCount2; i++)
            otherRend.SetBlendShapeWeight(i, body.GetBlendShapeWeight(i));
    }

    static public int[] GetAllPossibleFlags<T>() where T : System.Enum
    {
        var typeOf = typeof(T);
        if (typeOf.BaseType != typeof(Enum))
            throw new ArgumentException("T must be an Enum type");

        // The return type of Enum.GetValues is Array but it is effectively int[] per docs
        // This bit converts to int[]
        var values = Enum.GetValues(typeof(T)).Cast<int>().ToArray();

        if (!typeOf.GetCustomAttributes(typeof(FlagsAttribute), false).Any())
            return values;

        var valuesInverted = values.Select(v => ~v).ToArray();
        int max = 0;
        for (int i = 0; i < values.Length; i++)
        {
            max |= values[i];
        }

        var result = new List<int>();
        for (int i = 0; i <= max; i++)
        {
            int unaccountedBits = i;
            for (int j = 0; j < valuesInverted.Length; j++)
            {
                // This step removes each flag that is set in one of the Enums thus ensuring that an Enum with missing bits won't be passed an int that has those bits set
                unaccountedBits &= valuesInverted[j];
                if (unaccountedBits == 0)
                {
                    result.Add(i);
                    break;
                }
            }
        }

        return result.ToArray();
    }

   

    static public Dictionary<TValue, Tkey> ReverseKeyValues<TValue, Tkey>(this Dictionary<Tkey, TValue> originalDic)
    {
        var reversedDic = new Dictionary<TValue, Tkey>(originalDic.Count);
        foreach (var key in originalDic.Keys)
            reversedDic.Add(originalDic[key], key);

        return reversedDic;
    }

    static public IList<T> DistinctRefrences<T>(this IList<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            for (int j = 0; j < list.Count;)
            {  
                if (i != j && ReferenceEquals( list[i] , list[j] ) )
                    list.RemoveAt(j);
                else j++;
            }
        }
        return list;
    }

    static public void RemoveNull<T>(this List<T> list)
    {
        for (int i = 0; i < list.Count;)
            if ( list[i] == null )
                list.RemoveAt(i);
            else i++;
    }

    static public List<T> RemoveDestroyed<T>(this List<T> list) where T : UnityEngine.Object
    {
        for (int i = 0; i < list.Count;)
            if (!list[i])
                list.RemoveAt(i);
            else i++;
        return list;
    }

    static public T[] ForceLength<T>(T[] arr, int length)
    {
        if (arr == null)
            return new T[length];

        if (arr.Length == length)
            return arr.ToNewArray();

        var tmpList = new List<T>(arr);
        while (tmpList.Count < length)
            tmpList.Add(default(T));

        while (tmpList.Count > length)
            tmpList.RemoveAt(tmpList.Count - 1);

        arr = tmpList.ToArray();
        return arr;
    }

    static public T[] ToNewArray<T>(this T[] arr)
    {
        int count = arr.Length;
        var tmpArr = new T[count];
        arr.CopyTo(tmpArr, 0);
        return tmpArr;
    }
    static public List<T> ToNewList<T>(this ICollection<T> list) => new List<T>(list);
    static public T[] ToNewArray<T>(this ICollection<T> arr)
    {
        int count = arr.Count;
        var tmpArr = new T[count];
        arr.CopyTo(tmpArr, 0);
        return tmpArr;
    }
    
    static public Rect[] SubdivideFromTop(this in Rect rect , float[] ySizes , bool byPrecentage)
    {
        var rects = new Rect[ySizes.Length];
        var currentPos = rect.position;
		for (int i = 0; i < ySizes.Length; i++)
		{
            var sub = new Rect(rect);
            sub.height = byPrecentage ? sub.height * ySizes[i] : sub.height + ySizes[i];
            sub.position = currentPos;
            currentPos.y += sub.height;
            rects[i] = sub;
		}
        return rects;
    }

    public static Rect ScaleSizeBy(this in Rect rect, float scale)
    {
        return rect.ScaleSizeBy(scale, rect.center);
    }

    public static Rect ScaleSizeBy(this in Rect rect, float scale, Vector2 pivotPoint)
    {
        Rect result = rect;
        result.x -= pivotPoint.x;
        result.y -= pivotPoint.y;
        result.xMin *= scale;
        result.xMax *= scale;
        result.yMin *= scale;
        result.yMax *= scale;
        result.x += pivotPoint.x;
        result.y += pivotPoint.y;
        return result;
    }
    public static Rect ScaleSizeBy(this in Rect rect, in Vector2 scale)
    {
        return rect.ScaleSizeBy(scale, rect.center);
    }
    public static Rect ScaleSizeBy(this in Rect rect, in Vector2 scale, in Vector2 pivotPoint)
    {
        Rect result = rect;
        result.x -= pivotPoint.x;
        result.y -= pivotPoint.y;
        result.xMin *= scale.x;
        result.xMax *= scale.x;
        result.yMin *= scale.y;
        result.yMax *= scale.y;
        result.x += pivotPoint.x;
        result.y += pivotPoint.y;
        return result;
    }

    public static Color RandomColor()
    {
        var r = PureRandom; 
        var g = PureRandom;
        var b = PureRandom;
        return  new(r, g, b);   
    }

    public static Color RandomColor(float brightnessValue)
    {
        var h = PureRandom;
        var s = PureRandom;
        return Color.HSVToRGB(h, s, brightnessValue);
    }

    public static Color RandomColor(Random rnd, float saturation = 1f, float brightnessValue = 1f)
    { 
        return Color.HSVToRGB(rnd.NextFloat(), saturation, brightnessValue);
    }


    #endregion

}