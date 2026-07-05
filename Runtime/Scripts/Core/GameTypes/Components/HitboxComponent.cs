using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace core.gameplay
{
    [AddComponentMenu("CoreManager/HitboxComponent")]
    public class HitboxComponent : MonoBehaviour
    {
        public enum Axis
        {
            X,
            Y,
            Z,
        }

        public Axis axis = Axis.X;

        [Range(0f, 5f)]
        public float length = 0.2f;

        [Range(0.0f, 3f)]
        public float radius = 0.1f;

        public Vector3 Center
        {
            get
            {
                return Vector3.zero;
            }
            private set { }
        }

        private float GetScale()
        {
            var scl = transform.lossyScale;
            if (axis == Axis.X)
                return scl.x;
            else if (axis == Axis.Y)
                return scl.y;
            else
                return scl.z;
        }

        private Vector3 GetLocalDir()
        {
            if (axis == Axis.X)
                return Vector3.right;
            else if (axis == Axis.Y)
                return Vector3.up;
            else
                return Vector3.forward;
        }

        private Vector3 GetLocalUp()
        {
            if (axis == Axis.X)
                return Vector3.up;
            else if (axis == Axis.Y)
                return Vector3.forward;
            else
                return Vector3.up;
        }

        private void ToWorldSpaceCapsule(out float3 start, out float3 end, out float r)
        {
            float3 center = transform.TransformPoint(Center);
            r = 0f;
            float height = 0f;
            float3 lossyScale = math.abs(GetScale());
            float3 dir = float3.zero;

            switch (axis)
            {
                case Axis.X:
                    r = math.max(lossyScale.y, lossyScale.z) * radius;
                    height = lossyScale.x * length;
                    dir = transform.TransformDirection(Vector3.right);
                    break;
                case Axis.Y:
                    r = math.max(lossyScale.x, lossyScale.z) * radius;
                    height = lossyScale.y * length;
                    dir = transform.TransformDirection(Vector3.up);
                    break;
                case Axis.Z:
                    r = math.max(lossyScale.x, lossyScale.y) * radius;
                    height = lossyScale.z * length;
                    dir = transform.TransformDirection(Vector3.forward);
                    break;
            }

            if (height < r * 2f)
                dir = float3.zero;

            start = center + dir * (height * 0.5f - r);
            end = center - dir * (height * 0.5f - r);
        }

        public bool CheckCollision(HitboxComponent _other)
        {
            ToWorldSpaceCapsule(out float3 myStart, out float3 myEnd, out float myRadius);
            _other.ToWorldSpaceCapsule(out float3 otherStart, out float3 otherEnd, out float otherRadius);
            SegmentSegmentCPA(myStart, myEnd, otherStart, otherEnd, out float3 C0, out float3 C1, out bool parallel);

            float distance = math.length(C1 - C0);
            return distance <= myRadius + otherRadius;
        }

        public bool CheckCollision(Vector3 _point, float customRadius = 0.01f)
        {
            ToWorldSpaceCapsule(out float3 myStart, out float3 myEnd, out float myRadius);
            SegmentSegmentCPA(myStart, myEnd, _point, _point, out float3 C0, out float3 C1, out bool parallel);

            float distance = math.length(C1 - C0);
            return distance <= myRadius + customRadius;
        }

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            float3 center = transform.TransformPoint(Center);

            DrawWireCapsule(
                transform.TransformPoint(Center),
                transform.rotation,
                Vector3.one * GetScale(),
                GetLocalDir(),
                GetLocalUp(),
                length,
                radius
                );
#endif
        }

#if UNITY_EDITOR
        public static void DrawWireCapsule(
            Vector3 pos, Quaternion rot, Vector3 scl,
            Vector3 ldir, Vector3 lup,
            float length, float radius,
            bool resetMatrix = true
            )
        {
            Gizmos.matrix = Matrix4x4.TRS(pos, rot, scl);
            var l = ldir * ((length - (radius * 2)) / 2);

            Gizmos.DrawWireSphere(-l, radius);
            Gizmos.DrawWireSphere(l, radius);

            for (int i = 0; i < 360; i += 45)
            {
                var q = Quaternion.AngleAxis(i, ldir);
                var up1 = q * (lup * radius);
                var up2 = q * (lup * radius);
                Gizmos.DrawLine(-l + up1, l + up2);
            }

            Gizmos.matrix = Matrix4x4.TRS(pos, rot * Quaternion.AngleAxis(45, ldir), scl);
            Gizmos.DrawWireSphere(-l, radius);
            Gizmos.DrawWireSphere(l, radius);

            if (resetMatrix)
                Gizmos.matrix = Matrix4x4.identity;
        }

#endif
        private void SegmentSegmentCPA
        (
            float3 startA, float3 endA, float3 startB, float3 endB,
            out float3 c0, out float3 c1, out bool parallel
        )
        {
            var r = startB - startA;
            var u = endA - startA;
            var v = endB - startB;

            var ru = math.dot(r, u);
            var rv = math.dot(r, v);
            var uu = math.dot(u, u);
            var uv = math.dot(u, v);
            var vv = math.dot(v, v);
            var det = uu * vv - uv * uv;
            float3 s, t;
            if (det < (1e-6f * uu * vv))
            {
                s = math.clamp(ru / uu, 0, 1);
                t = 0;
                parallel = true;
            }
            else
            {
                s = math.clamp((ru * vv - rv * uv) / det, 0, 1);
                t = math.clamp((ru * uv - rv * uu) / det, 0, 1);
                parallel = false;
            }
            var S = math.clamp((t * uv + ru) / uu, 0, 1);
            var T = math.clamp((s * uv - rv) / vv, 0, 1);
            c0 = startA + S * u;
            c1 = startB + T * v;
        }
    }
}