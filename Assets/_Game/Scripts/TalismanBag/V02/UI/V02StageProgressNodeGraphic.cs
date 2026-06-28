using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.V02.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class V02StageProgressNodeGraphic : MaskableGraphic
    {
        [SerializeField] private Color fillColor = new(0.07f, 0.08f, 0.07f, 1f);
        [SerializeField] private Color ringColor = new(0.02f, 0.025f, 0.02f, 1f);
        [SerializeField] private float ringWidth = 6f;

        protected override void OnEnable()
        {
            EnsureCanvasRenderer();
            base.OnEnable();
            SetAllDirty();
        }

        public void SetVisual(bool isBoss, Color fill, Color ring, float ringThickness, bool preserveStroke = false)
        {
            fillColor = fill;
            if (!preserveStroke)
            {
                ringColor = ring;
                ringWidth = Mathf.Max(1f, ringThickness);
            }

            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect rect = GetPixelAdjustedRect();
            AddEllipse(vh, rect, ringColor, 48);

            Rect inner = new(
                rect.x + ringWidth,
                rect.y + ringWidth,
                Mathf.Max(0f, rect.width - ringWidth * 2f),
                Mathf.Max(0f, rect.height - ringWidth * 2f));
            AddEllipse(vh, inner, fillColor, 48);
        }

        private static void AddEllipse(VertexHelper vh, Rect rect, Color color, int segmentCount)
        {
            if (rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            int centerIndex = vh.currentVertCount;
            Vector2 center = rect.center;
            vh.AddVert(center, color, Vector2.zero);

            int firstOuterIndex = vh.currentVertCount;
            float radiusX = rect.width * 0.5f;
            float radiusY = rect.height * 0.5f;
            for (int i = 0; i < segmentCount; i++)
            {
                float angle = Mathf.PI * 2f * i / segmentCount;
                Vector2 point = new(center.x + Mathf.Cos(angle) * radiusX, center.y + Mathf.Sin(angle) * radiusY);
                vh.AddVert(point, color, Vector2.zero);
            }

            for (int i = 0; i < segmentCount; i++)
            {
                int current = firstOuterIndex + i;
                int next = firstOuterIndex + ((i + 1) % segmentCount);
                vh.AddTriangle(centerIndex, current, next);
            }
        }

        private void EnsureCanvasRenderer()
        {
            if (GetComponent<CanvasRenderer>() == null)
            {
                gameObject.AddComponent<CanvasRenderer>();
            }
        }
    }
}
