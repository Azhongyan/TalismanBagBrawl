using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.V04.ChapterFlow.Chapter1.Presentation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image))]
    public sealed class C1AuthoredEnemyCalibrationMeshEffect :
        BaseMeshEffect
    {
        private float runtimeScale = 1f;
        private Vector2 runtimeOffset;
        private Vector2 runtimePivot = new(0.5f, 0.5f);

        public void Apply(C1EnemyVisualCalibration calibration)
        {
            RectInt reference = calibration.ReferencePixelBounds;
            RectInt union = calibration.UnionPixelBounds;
            float boundsScale = 1f;
            Vector2 boundsOffset = Vector2.zero;
            if (reference.width > 0
                && reference.height > 0
                && union.width > 0
                && union.height > 0)
            {
                boundsScale = Mathf.Min(
                    (float)reference.width / union.width,
                    (float)reference.height / union.height);
                Vector2 referenceCenter = new(
                    reference.x + reference.width * 0.5f,
                    reference.y + reference.height * 0.5f);
                Vector2 unionCenter = new(
                    union.x + union.width * 0.5f,
                    union.y + union.height * 0.5f);
                boundsOffset = new Vector2(
                    (referenceCenter.x - unionCenter.x)
                    / reference.width,
                    (referenceCenter.y - unionCenter.y)
                    / reference.height);
            }
            runtimeScale = Mathf.Max(
                0.01f,
                boundsScale
                * calibration.StateScaleMultiplier);
            runtimeOffset = boundsOffset
                + calibration.NormalizedLocalOffset;
            runtimePivot = calibration.NormalizedPivot;
            graphic?.SetVerticesDirty();
        }

        public void RestoreIdentity()
        {
            runtimeScale = 1f;
            runtimeOffset = Vector2.zero;
            runtimePivot = new Vector2(0.5f, 0.5f);
            graphic?.SetVerticesDirty();
        }

        public override void ModifyMesh(VertexHelper vertexHelper)
        {
            if (!IsActive()
                || vertexHelper == null
                || vertexHelper.currentVertCount == 0)
            {
                return;
            }

            Rect rect = graphic.rectTransform.rect;
            Vector2 pivot = new(
                Mathf.Lerp(rect.xMin, rect.xMax, runtimePivot.x),
                Mathf.Lerp(rect.yMin, rect.yMax, runtimePivot.y));
            Vector2 offset = new(
                rect.width * runtimeOffset.x,
                rect.height * runtimeOffset.y);
            UIVertex vertex = default;
            for (int index = 0;
                 index < vertexHelper.currentVertCount;
                 index++)
            {
                vertexHelper.PopulateUIVertex(ref vertex, index);
                Vector2 position = vertex.position;
                position = pivot
                    + ((position - pivot) * runtimeScale)
                    + offset;
                vertex.position = new Vector3(
                    position.x,
                    position.y,
                    vertex.position.z);
                vertexHelper.SetUIVertex(vertex, index);
            }
        }
    }
}
