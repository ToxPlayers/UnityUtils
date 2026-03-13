// MIT License
// 
// Copyright (c) 2017 Justin Larrabee <justonia@gmail.com>
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class PhysicsExtensions {

    #region DynamicCollider
    public static Span<Collider> OverlapNonAllocSpan(this Collider col, int layerMask, Collider[] results = null, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
        return col switch {
            BoxCollider box => OverlapBoxNonAllocSpan(box, layerMask, results, queryTriggerInteraction),
            SphereCollider sphere => OverlapSphereNonAllocSpan(sphere, layerMask, results, queryTriggerInteraction),
            CapsuleCollider cap => OverlapCapsuleNonAllocSpan(cap, layerMask, results, queryTriggerInteraction),
            _ => throw new NotImplementedException("Unknown collider type:" + col.GetType())
        };
    }
    #endregion

    #region Box

    public static bool BoxCast(this BoxCollider box, Vector3 direction, int layerMask, float maxDistance = Mathf.Infinity,  QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
        box.ToWorldSpaceBox(out Vector3 center, out Vector3 halfExtents, out Quaternion orientation);
        return Physics.BoxCast(center, halfExtents, direction, orientation, maxDistance, layerMask, queryTriggerInteraction);
    }

    public static bool BoxCast(this BoxCollider box, Vector3 direction, out RaycastHit hitInfo, int layerMask, float maxDistance = Mathf.Infinity,  QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
        box.ToWorldSpaceBox(out Vector3 center, out Vector3 halfExtents, out Quaternion orientation);
        return Physics.BoxCast(center, halfExtents, direction, out hitInfo, orientation, maxDistance, layerMask, queryTriggerInteraction);
    }

    public static RaycastHit[] BoxCastAll(this BoxCollider box, Vector3 direction, int layerMask, float maxDistance = Mathf.Infinity,  QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
        box.ToWorldSpaceBox(out Vector3 center, out Vector3 halfExtents, out Quaternion orientation);
        return Physics.BoxCastAll(center, halfExtents, direction, orientation, maxDistance, layerMask, queryTriggerInteraction);
    }

    public static int BoxCastNonAlloc(this BoxCollider box, Vector3 direction, RaycastHit[] results, int layerMask, float maxDistance = Mathf.Infinity,  QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
        box.ToWorldSpaceBox(out Vector3 center, out Vector3 halfExtents, out Quaternion orientation);
        return Physics.BoxCastNonAlloc(center, halfExtents, direction, results, orientation, maxDistance, layerMask, queryTriggerInteraction);
    }

    public static bool CheckBox(this BoxCollider box, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
        box.ToWorldSpaceBox(out Vector3 center, out Vector3 halfExtents, out Quaternion orientation);
        return Physics.CheckBox(center, halfExtents, orientation, layerMask, queryTriggerInteraction);
    }

    public static Collider[] OverlapBox(this BoxCollider box, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
        box.ToWorldSpaceBox(out Vector3 center, out Vector3 halfExtents, out Quaternion orientation);
        return Physics.OverlapBox(center, halfExtents, orientation, layerMask, queryTriggerInteraction);
    }

    static public readonly Collider[] GlobalTempColliders = new Collider[64];

    public static int OverlapBoxNonAlloc(this BoxCollider box, int layerMask, Collider[] results, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
        box.ToWorldSpaceBox(out Vector3 center, out Vector3 halfExtents, out Quaternion orientation);
        var physicScene = box.gameObject.scene.GetPhysicsScene();
        return physicScene.OverlapBox(center, halfExtents, results, orientation, layerMask, queryTriggerInteraction);
    }

    public static Span<Collider> OverlapBoxNonAllocSpan(this BoxCollider box, int layerMask, Collider[] results = null, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
        var count = OverlapBoxNonAlloc(box, layerMask, results ??= GlobalTempColliders, queryTriggerInteraction);
        return count == 0 ? Span<Collider>.Empty : new Span<Collider>(results, 0, count);
    }

    public static void ToWorldSpaceBox(this BoxCollider box, out Vector3 center, out Vector3 halfExtents, out Quaternion orientation) {
        orientation = box.transform.rotation;
        center = box.transform.TransformPoint(box.center);
        var lossyScale = box.transform.lossyScale;
        var scale = lossyScale.Abs();
        halfExtents = Vector3.Scale(scale, box.size) * 0.5f;
    }

    #endregion

    #region Sphere

    public static bool SphereCast(this SphereCollider sphere, Vector3 direction, out RaycastHit hitInfo, int layerMask, float maxDistance = Mathf.Infinity,  QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
        sphere.ToWorldSpaceSphere(out Vector3 center, out float radius);
        return Physics.SphereCast(center, radius, direction, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
    }

    public static RaycastHit[] SphereCastAll(this SphereCollider sphere, Vector3 direction, int layerMask, float maxDistance = Mathf.Infinity,  QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
        sphere.ToWorldSpaceSphere(out Vector3 center, out float radius);
        return Physics.SphereCastAll(center, radius, direction, maxDistance, layerMask, queryTriggerInteraction);
    }

    public static int SphereCastNonAlloc(this SphereCollider sphere, Vector3 direction, RaycastHit[] results, int layerMask, float maxDistance = Mathf.Infinity,  QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
        sphere.ToWorldSpaceSphere(out Vector3 center, out float radius);
        return Physics.SphereCastNonAlloc(center, radius, direction, results, maxDistance, layerMask, queryTriggerInteraction);
    }

    public static bool CheckSphere(this SphereCollider sphere, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
        sphere.ToWorldSpaceSphere(out Vector3 center, out float radius);
        return Physics.CheckSphere(center, radius, layerMask, queryTriggerInteraction);
    }

    public static Collider[] OverlapSphere(this SphereCollider sphere, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
        sphere.ToWorldSpaceSphere(out Vector3 center, out float radius);
        return Physics.OverlapSphere(center, radius, layerMask, queryTriggerInteraction);
    }

    public static int OverlapSphereNonAlloc(this SphereCollider sphere,  int layerMask, Collider[] results, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
        sphere.ToWorldSpaceSphere(out Vector3 center, out float radius);
        var physicScene = sphere.gameObject.scene.GetPhysicsScene(); 
        return physicScene.OverlapSphere(center, radius, results, layerMask, queryTriggerInteraction);
    }

    public static Span<Collider> OverlapSphereNonAllocSpan(this SphereCollider sphere,  int layerMask, Collider[] results = null, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
        var count = OverlapSphereNonAlloc(sphere, layerMask, results ??= GlobalTempColliders, queryTriggerInteraction);
        return count == 0 ? Span<Collider>.Empty : new Span<Collider>(results, 0, count);
    } 

    public static void ToWorldSpaceSphere(this SphereCollider sphere, out Vector3 center, out float radius) {
        center = sphere.transform.TransformPoint(sphere.center);
        radius = sphere.radius * sphere.transform.lossyScale.Abs().MaxValue();
    }

    #endregion

    #region Capsule

    public static bool CapsuleCast(this CapsuleCollider capsule, Vector3 direction, out RaycastHit hitInfo, int layerMask, float maxDistance = Mathf.Infinity,  QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
        capsule.ToWorldSpaceCapsule(out Vector3 point0, out Vector3 point1, out float radius);
        return Physics.CapsuleCast(point0, point1, radius, direction, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
    }

    public static RaycastHit[] CapsuleCastAll(this CapsuleCollider capsule, Vector3 direction, int layerMask, float maxDistance = Mathf.Infinity,  QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
        capsule.ToWorldSpaceCapsule(out Vector3 point0, out Vector3 point1, out float radius);
        return Physics.CapsuleCastAll(point0, point1, radius, direction, maxDistance, layerMask, queryTriggerInteraction);
    }

    public static int CapsuleCastNonAlloc(this CapsuleCollider capsule, Vector3 direction, RaycastHit[] results, int layerMask, float maxDistance = Mathf.Infinity,  QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
        capsule.ToWorldSpaceCapsule(out Vector3 point0, out Vector3 point1, out float radius);
        return Physics.CapsuleCastNonAlloc(point0, point1, radius, direction, results, maxDistance, layerMask, queryTriggerInteraction);
    }

    public static bool CheckCapsule(this CapsuleCollider capsule, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
        capsule.ToWorldSpaceCapsule(out Vector3 point0, out Vector3 point1, out float radius);
        return Physics.CheckCapsule(point0, point1, radius, layerMask, queryTriggerInteraction);
    }

    public static Collider[] OverlapCapsule(this CapsuleCollider capsule, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
        capsule.ToWorldSpaceCapsule(out Vector3 point0, out Vector3 point1, out float radius);
        return Physics.OverlapCapsule(point0, point1, radius, layerMask, queryTriggerInteraction);
    }

    public static int OverlapCapsuleNonAlloc(this CapsuleCollider capsule, int layerMask, Collider[] results, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
        capsule.ToWorldSpaceCapsule(out Vector3 point0, out Vector3 point1, out float radius);
        var physicScene = capsule.gameObject.scene.GetPhysicsScene();
        return physicScene.OverlapCapsule(point0, point1, radius, results, layerMask, queryTriggerInteraction);
    }

    public static Span<Collider> OverlapCapsuleNonAllocSpan(this CapsuleCollider capsule, int layerMask, Collider[] results, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
        var count = OverlapCapsuleNonAlloc(capsule, layerMask, results ??= GlobalTempColliders, queryTriggerInteraction);
        return count == 0 ? Span<Collider>.Empty : new Span<Collider>(results, 0, count);
    }

    public static void ToWorldSpaceCapsule(this CapsuleCollider capsule, out Vector3 point0, out Vector3 point1, out float radius) {
        var center = capsule.transform.TransformPoint(capsule.center);
        radius = 0f;
        float height = 0f;
        Vector3 lossyScale = capsule.transform.lossyScale.Abs();
        Vector3 dir = Vector3.zero;

        switch (capsule.direction) {
            case 0: // x
                radius = Mathf.Max(lossyScale.y, lossyScale.z) * capsule.radius;
                height = lossyScale.x * capsule.height;
                dir = capsule.transform.TransformDirection(Vector3.right);
                break;
            case 1: // y
                radius = Mathf.Max(lossyScale.x, lossyScale.z) * capsule.radius;
                height = lossyScale.y * capsule.height;
                dir = capsule.transform.TransformDirection(Vector3.up);
                break;
            case 2: // z
                radius = Mathf.Max(lossyScale.x, lossyScale.y) * capsule.radius;
                height = lossyScale.z * capsule.height;
                dir = capsule.transform.TransformDirection(Vector3.forward);
                break;
        }

        if (height < radius * 2f) {
            dir = Vector3.zero;
        }

        point0 = center + dir * (height * 0.5f - radius);
        point1 = center - dir * (height * 0.5f - radius);
    }

    #endregion

    #region Util 
    public static void SortClosestToFurthest(RaycastHit[] hits, int hitCount = -1) {
        if (hitCount == 0) {
            return;
        }

        if (hitCount < 0) {
            hitCount = hits.Length;
        }

        Array.Sort(hits, 0, hitCount, AscendDistanceComparer);
    }
     
    public class AscendingDistanceComparer : IComparer<RaycastHit> {
        public int Compare(RaycastHit h1, RaycastHit h2) {
            return h1.distance < h2.distance ? -1 : (h1.distance > h2.distance ? 1 : 0);
        }
    }

    public static readonly AscendingDistanceComparer AscendDistanceComparer = new AscendingDistanceComparer();
    #endregion
}