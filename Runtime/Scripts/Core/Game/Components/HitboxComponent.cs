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

            float distance = math.length( C1 - C0 );
            return distance <= myRadius+otherRadius;
        }

        public bool CheckCollision(Vector3 _point, float customRadius = 0.01f)
        {
            ToWorldSpaceCapsule(out float3 myStart, out float3 myEnd, out float myRadius);
            SegmentSegmentCPA(myStart, myEnd, _point, _point, out float3 C0, out float3 C1, out bool parallel);
            
            float distance = math.length( C1 - C0 );
            return distance <= myRadius + customRadius;
        }

        private void OnDrawGizmos()
        {
            #if UNITY_EDITOR
            // Draw gizmos only on Editor (uses an editor only class)
            float3 center = transform.TransformPoint(Center);
            DrawWireCapsule(center, transform.rotation, radius, length);
            #endif
        }

        #if UNITY_EDITOR
        private void DrawWireCapsule(Vector3 _pos, Quaternion _rot, float _radius, float _height, Color _color = default(Color))
        {
            if (_color != default(Color))
                Handles.color = _color;

            Matrix4x4 angleMatrix = Matrix4x4.TRS(_pos, _rot, Handles.matrix.lossyScale);
            using (new Handles.DrawingScope(angleMatrix))
            {
                var pointOffset = (_height - (_radius * 2)) / 2;
    
                //draw sideways
                Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.left, Vector3.back, -180, _radius);
                Handles.DrawLine(new Vector3(0, pointOffset, -_radius), new Vector3(0, -pointOffset, -_radius));
                Handles.DrawLine(new Vector3(0, pointOffset, _radius), new Vector3(0, -pointOffset, _radius));
                Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.left, Vector3.back, 180, _radius);
                //draw frontways
                Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.back, Vector3.left, 180, _radius);
                Handles.DrawLine(new Vector3(-_radius, pointOffset, 0), new Vector3(-_radius, -pointOffset, 0));
                Handles.DrawLine(new Vector3(_radius, pointOffset, 0), new Vector3(_radius, -pointOffset, 0));
                Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.back, Vector3.left, -180, _radius);
                //draw center
                Handles.DrawWireDisc(Vector3.up * pointOffset, Vector3.up, _radius);
                Handles.DrawWireDisc(Vector3.down * pointOffset, Vector3.up, _radius);

            }
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