using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace Supercent.UI.Util
{
    [DisallowMultipleComponent]
    public sealed class SpriteFlip : BaseMeshEffect
    {
        static readonly Vector2 Center = new Vector2(0.5f, 0.5f);
        static readonly Quaternion RotZ90 = Quaternion.Euler(0, 0, 90);

        [SerializeField] bool isLay = false;
        public bool IsLay
        {
            set
            {
                if (isLay != value)
                {
                    isLay = value;
                    Dirty();
                }
            }
            get => isLay;
        }

        [SerializeField] bool horizontal = false;
        public bool Horizontal
        {
            set
            {
                if (horizontal != value)
                {
                    horizontal = value;
                    Dirty();
                }
            }
            get => horizontal;
        }

        [SerializeField] bool vertical = false;
        public bool Vertical
        {
            set
            {
                if (vertical != value)
                {
                    vertical = value;
                    Dirty();
                }
            }
            get => vertical;
        }

        FieldInfo indicesInfo = null;



        protected override void Awake()
        {
            base.Awake();
            indicesInfo = typeof(VertexHelper).GetField("m_Indices", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            indicesInfo = null;
        }



        public void Set(bool horizontal, bool vertical)
        {
            this.horizontal = horizontal;
            this.vertical = vertical;
            Dirty();
        }
        public void Set(bool horizontal, bool vertical, bool isLay)
        {
            this.isLay = isLay;
            this.horizontal = horizontal;
            this.vertical = vertical;
            Dirty();
        }

        void Dirty()
        {
            if (graphic != null)
                graphic.SetVerticesDirty();
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive()) return;

            if (horizontal || vertical || isLay)
            {
                var vertex = default(UIVertex);

                var size = graphic.rectTransform.rect.size;
                var offsetPivot = Center - graphic.rectTransform.pivot;
                var offsetInversePos = size * (offsetPivot / Center);

                if (isLay)
                {
                    var offsetPos = size * offsetPivot;
                    var ratio = size.x / size.y;

                    for (int index = 0, count = vh.currentVertCount; index < count; ++index)
                    {
                        vh.PopulateUIVertex(ref vertex, index);
                        {
                            vertex.position.x -= offsetPos.x;
                            vertex.position.y -= offsetPos.y;

                            vertex.position = RotZ90 * vertex.position;
                            vertex.position.x *= ratio;
                            vertex.position.y /= ratio;
                            vertex.position.x += offsetPos.x;
                            vertex.position.y += offsetPos.y;

                            // offset + Inverse vertext
                            if (horizontal) vertex.position.x = offsetInversePos.x - vertex.position.x;
                            if (vertical) vertex.position.y = offsetInversePos.y - vertex.position.y;
                        }
                        vh.SetUIVertex(vertex, index);
                    };
                }
                else
                {
                    for (int index = 0, count = vh.currentVertCount; index < count; ++index)
                    {
                        vh.PopulateUIVertex(ref vertex, index);
                        {
                            // offset + Inverse vertext
                            if (horizontal) vertex.position.x = offsetInversePos.x - vertex.position.x;
                            if (vertical) vertex.position.y = offsetInversePos.y - vertex.position.y;
                        }
                        vh.SetUIVertex(vertex, index);
                    };
                }

                if (horizontal ^ vertical)
                {
                    if (indicesInfo?.GetValue(vh) is List<int> indices)
                        indices.Reverse();
                }
            }
        }
    }
}