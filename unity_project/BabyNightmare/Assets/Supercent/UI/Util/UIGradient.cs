using UnityEngine;
using UnityEngine.UI;

namespace Supercent.UI.Util
{
    [DisallowMultipleComponent]
    public sealed class UIGradient : BaseMeshEffect
    {
        static readonly Vector2[] VerticePositions = new Vector2[] { Vector2.up, Vector2.one, Vector2.right, Vector2.zero };
        static readonly Rect RectNormal = new Rect(0, 0, 1, 1);

        [SerializeField] Color color1 = Color.white;    public Color Color1 { set { color1 = value; Dirty(); } get => color1; }
        [SerializeField] Color color2 = Color.white;    public Color Color2 { set { color2 = value; Dirty(); } get => color2; }

        [SerializeField] float angle = 0f;              public float Angle { set => Set(value); get => angle; }
        [Range(-1f, 1f)]
        [SerializeField] float pivot = 0;               public float Pivot { set { pivot = Mathf.Clamp(value, -1f, 1f); Dirty(); } get => pivot; }



        public void Set(float angle)
        {
            this.angle = angle % 360f;
            Dirty();
        }
        public void Set(in Color color1, in Color color2)
        {
            this.color1 = color1;
            this.color2 = color2;
            Dirty();
        }
        public void Set(in Color color1, in Color color2, float angle)
        {
            this.color1 = color1;
            this.color2 = color2;
            this.angle = angle;
            Dirty();
        }

        void Dirty() { if (graphic != null) graphic.SetVerticesDirty(); }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive()) return;

            var dir = RotationDir(angle);
            var vertex = default(UIVertex);
            if (graphic is Text)
            {
                var posMatrix = LocalPositionMatrix(RectNormal, dir);

                for (int index = 0, count = vh.currentVertCount; index < count; ++index)
                {
                    vh.PopulateUIVertex(ref vertex, index);
                    {
                        var position = VerticePositions[index % 4];
                        var localPosition = posMatrix * position;
                        vertex.color *= Color.Lerp(color2, color1, localPosition.y + pivot);
                    }
                    vh.SetUIVertex(vertex, index);
                };
            }
            else
            {
                var rect = graphic.rectTransform.rect;
                dir = CompensateAspectRatio(rect, dir);
                var posMatrix = LocalPositionMatrix(rect, dir);

                for (int index = 0, count = vh.currentVertCount; index < count; ++index)
                {
                    vh.PopulateUIVertex(ref vertex, index);
                    {
                        var localPosition = posMatrix * vertex.position;
                        vertex.color *= Color.Lerp(color2, color1, localPosition.y + pivot);
                    }
                    vh.SetUIVertex(vertex, index);
                };
            }
        }


        static Vector2 RotationDir(float angle)
        {
            var angleRad = angle * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
        }
        static Vector2 CompensateAspectRatio(in Rect rect, Vector2 dir)
        {
            var ratio = rect.height / rect.width;
            dir.x *= ratio;
            return dir.normalized;
        }
        static Matrix2x3 LocalPositionMatrix(in Rect rect, Vector2 dir)
        {
            var cos = dir.x;
            var sin = dir.y;
            var rectMin = rect.min;
            var rectSize = rect.size;

            var c = 0.5f;
            var ax = rectMin.x / rectSize.x + c;
            var ay = rectMin.y / rectSize.y + c;
            var m00 = cos / rectSize.x;
            var m01 = sin / rectSize.y;
            var m02 = -(ax * cos - ay * sin - c);
            var m10 = sin / rectSize.x;
            var m11 = cos / rectSize.y;
            var m12 = -(ax * sin + ay * cos - c);
            return new Matrix2x3(m00, m01, m02, m10, m11, m12);
        }


        struct Matrix2x3
        {
            public float m00, m01, m02, m10, m11, m12;
            public Matrix2x3(float m00, float m01, float m02, float m10, float m11, float m12)
            {
                this.m00 = m00;
                this.m01 = m01;
                this.m02 = m02;
                this.m10 = m10;
                this.m11 = m11;
                this.m12 = m12;
            }

            public static Vector2 operator *(Matrix2x3 m, Vector2 v)
            {
                float x = (m.m00 * v.x) - (m.m01 * v.y) + m.m02;
                float y = (m.m10 * v.x) + (m.m11 * v.y) + m.m12;
                return new Vector2(x, y);
            }
        }



#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            angle = angle % 360f;
            pivot = Mathf.Clamp(pivot, -1f, 1f);
        }
#endif// UNITY_EDITOR
    }
}