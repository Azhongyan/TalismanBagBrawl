using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.V04.ChapterFlow.Chapter1.Presentation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image))]
    public sealed class C1AuthoredEnemySelectionOutlineEffect :
        Shadow
    {
        [SerializeField, Min(1f)] private float outlinePixels = 5f;

        protected override void OnEnable()
        {
            base.OnEnable();
            useGraphicAlpha = true;
            effectColor = new Color(1f, 0.82f, 0.28f, 0.92f);
            effectDistance = Vector2.one * outlinePixels;
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || vh == null)
            {
                return;
            }

            List<UIVertex> verts = new();
            vh.GetUIVertexStream(verts);
            if (verts.Count == 0)
            {
                return;
            }

            Color32 color = effectColor;
            float radius = Mathf.Max(1f, outlinePixels);
            int required = verts.Count * 9;
            if (verts.Capacity < required)
            {
                verts.Capacity = required;
            }

            int start = 0;
            ApplyShadowZeroAlloc(
                verts,
                color,
                start,
                verts.Count,
                radius,
                0f);
            start = verts.Count;
            ApplyShadowZeroAlloc(
                verts,
                color,
                0,
                start,
                -radius,
                0f);
            start = verts.Count;
            ApplyShadowZeroAlloc(
                verts,
                color,
                0,
                start,
                0f,
                radius);
            start = verts.Count;
            ApplyShadowZeroAlloc(
                verts,
                color,
                0,
                start,
                0f,
                -radius);

            float diagonal = radius * 0.72f;
            start = verts.Count;
            ApplyShadowZeroAlloc(
                verts,
                color,
                0,
                start,
                diagonal,
                diagonal);
            start = verts.Count;
            ApplyShadowZeroAlloc(
                verts,
                color,
                0,
                start,
                diagonal,
                -diagonal);
            start = verts.Count;
            ApplyShadowZeroAlloc(
                verts,
                color,
                0,
                start,
                -diagonal,
                diagonal);
            start = verts.Count;
            ApplyShadowZeroAlloc(
                verts,
                color,
                0,
                start,
                -diagonal,
                -diagonal);

            vh.Clear();
            vh.AddUIVertexTriangleStream(verts);
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            effectColor = new Color(1f, 0.82f, 0.28f, 0.92f);
            effectDistance = Vector2.one * outlinePixels;
            useGraphicAlpha = true;
        }
#endif
    }
}
