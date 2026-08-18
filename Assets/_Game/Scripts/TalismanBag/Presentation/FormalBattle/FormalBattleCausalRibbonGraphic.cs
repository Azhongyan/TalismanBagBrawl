using System;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.Presentation.FormalBattle
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class FormalBattleCausalRibbonGraphic : MaskableGraphic
    {
        private string acceptedEventId = string.Empty;
        private string targetKey = string.Empty;
        private Vector2 startPoint;
        private Vector2 endPoint;
        private Color coreColor = Color.clear;
        private Color glowColor = Color.clear;
        private int segmentCount = 6;
        private float waveAmplitude;
        private float waveCycles = 1f;
        private float coreWidth = 6f;
        private float glowWidth = 20f;
        private float startedAt = -100f;
        private float duration = 0.1f;
        private float pausedAt;
        private bool paused;
        private bool active;
        private float reveal;
        private float visualAlpha;

        public bool Active => active;
        public float StartedAt => startedAt;

        protected override void Awake()
        {
            base.Awake();
            raycastTarget = false;
            maskable = true;
        }

        private void Update()
        {
            if (!active || paused)
            {
                return;
            }

            float progress = Mathf.Clamp01(
                (Time.unscaledTime - startedAt) / duration);
            reveal = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.Clamp01(progress / 0.46f));
            visualAlpha = progress < 0.58f
                ? 1f
                : 1f - Mathf.InverseLerp(0.58f, 1f, progress);
            SetVerticesDirty();
            if (progress >= 1f)
            {
                Clear();
            }
        }

        protected override void OnDisable()
        {
            Clear();
            base.OnDisable();
        }

        public bool Matches(string eventId, string configuredTargetKey)
        {
            return active
                   && string.Equals(
                       acceptedEventId,
                       eventId,
                       StringComparison.Ordinal)
                   && string.Equals(
                       targetKey,
                       configuredTargetKey,
                       StringComparison.Ordinal);
        }

        public void Play(
            string eventId,
            string configuredTargetKey,
            Vector2 configuredStartPoint,
            Vector2 configuredEndPoint,
            FormalBattleCausalItemStyle style,
            float configuredDuration)
        {
            if (style == null || !style.Validate())
            {
                Clear();
                return;
            }

            acceptedEventId = eventId ?? string.Empty;
            targetKey = configuredTargetKey ?? string.Empty;
            startPoint = configuredStartPoint;
            endPoint = configuredEndPoint;
            coreColor = style.RibbonCoreColor;
            glowColor = style.RibbonGlowColor;
            segmentCount = Mathf.Max(2, style.RibbonSegmentCount);
            waveAmplitude = Mathf.Max(0f, style.RibbonWaveAmplitude);
            waveCycles = Mathf.Max(0.25f, style.RibbonWaveCycles);
            coreWidth = Mathf.Max(1f, style.RibbonCoreWidth);
            glowWidth = Mathf.Max(coreWidth, style.RibbonGlowWidth);
            startedAt = Time.unscaledTime;
            duration = Mathf.Max(0.1f, configuredDuration);
            paused = false;
            pausedAt = 0f;
            active = true;
            reveal = 0.08f;
            visualAlpha = 1f;
            SetVerticesDirty();
        }

        public void SetPaused(bool value)
        {
            if (!active || paused == value)
            {
                return;
            }
            paused = value;
            if (paused)
            {
                pausedAt = Time.unscaledTime;
                return;
            }
            startedAt += Mathf.Max(0f, Time.unscaledTime - pausedAt);
            pausedAt = 0f;
        }

        public void Clear()
        {
            active = false;
            paused = false;
            acceptedEventId = string.Empty;
            targetKey = string.Empty;
            startedAt = -100f;
            duration = 0.1f;
            reveal = 0f;
            visualAlpha = 0f;
            SetVerticesDirty();
        }

        public bool ValidateAuthoredReferences()
        {
            return rectTransform != null
                   && canvasRenderer != null
                   && !raycastTarget
                   && maskable;
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();
            if (!active
                || visualAlpha <= 0f
                || reveal <= 0f
                || (endPoint - startPoint).sqrMagnitude < 1f)
            {
                return;
            }

            int visibleSegments = Mathf.Max(
                1,
                Mathf.CeilToInt(segmentCount * reveal));
            for (int index = 0; index < visibleSegments; index++)
            {
                float t0 = index / (float)segmentCount;
                float t1 = Mathf.Min(
                    reveal,
                    (index + 1f) / segmentCount);
                if (t1 <= t0)
                {
                    continue;
                }

                Vector2 point0 = EvaluatePoint(t0);
                Vector2 point1 = EvaluatePoint(t1);
                Vector2 direction = point1 - point0;
                if (direction.sqrMagnitude < 0.0001f)
                {
                    continue;
                }
                Vector2 normal = new Vector2(-direction.y, direction.x)
                    .normalized;
                float taper0 = Mathf.Lerp(1f, 0.62f, t0);
                float taper1 = Mathf.Lerp(1f, 0.62f, t1);
                AddBand(
                    vertexHelper,
                    point0,
                    point1,
                    normal,
                    glowWidth * taper0,
                    glowWidth * taper1,
                    WithAlpha(glowColor, visualAlpha));
                AddBand(
                    vertexHelper,
                    point0,
                    point1,
                    normal,
                    coreWidth * taper0,
                    coreWidth * taper1,
                    WithAlpha(coreColor, visualAlpha));
            }
        }

        private Vector2 EvaluatePoint(float value)
        {
            float t = Mathf.Clamp01(value);
            Vector2 direction = endPoint - startPoint;
            Vector2 normal = direction.sqrMagnitude < 0.0001f
                ? Vector2.up
                : new Vector2(-direction.y, direction.x).normalized;
            float envelope = Mathf.Sin(t * Mathf.PI);
            float wave = Mathf.Sin(t * Mathf.PI * 2f * waveCycles)
                         * waveAmplitude
                         * envelope;
            return Vector2.Lerp(startPoint, endPoint, t) + normal * wave;
        }

        private static void AddBand(
            VertexHelper vertexHelper,
            Vector2 point0,
            Vector2 point1,
            Vector2 normal,
            float width0,
            float width1,
            Color color)
        {
            int startIndex = vertexHelper.currentVertCount;
            float half0 = width0 * 0.5f;
            float half1 = width1 * 0.5f;
            AddVertex(vertexHelper, point0 - normal * half0, color);
            AddVertex(vertexHelper, point0 + normal * half0, color);
            AddVertex(vertexHelper, point1 + normal * half1, color);
            AddVertex(vertexHelper, point1 - normal * half1, color);
            vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vertexHelper.AddTriangle(startIndex, startIndex + 2, startIndex + 3);
        }

        private static void AddVertex(
            VertexHelper vertexHelper,
            Vector2 position,
            Color color)
        {
            UIVertex vertex = UIVertex.simpleVert;
            vertex.position = position;
            vertex.color = color;
            vertex.uv0 = Vector2.zero;
            vertexHelper.AddVert(vertex);
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a *= Mathf.Clamp01(alpha);
            return color;
        }

#if UNITY_EDITOR
        public void ShowAuthoringPreviewForEditor(
            int previewIndex,
            Vector2 configuredStartPoint,
            Vector2 configuredEndPoint)
        {
            acceptedEventId = "EDITOR_PREVIEW_" + previewIndex;
            targetKey = "EDITOR_PREVIEW_TARGET";
            startPoint = configuredStartPoint;
            endPoint = configuredEndPoint;
            coreColor = new Color(1f, 0.72f, 0.24f, 0.92f);
            glowColor = new Color(1f, 0.26f, 0.08f, 0.42f);
            segmentCount = 8;
            waveAmplitude = 12f + previewIndex * 2f;
            waveCycles = 1.15f;
            coreWidth = 7f;
            glowWidth = 24f;
            startedAt = -100f;
            duration = 1f;
            paused = true;
            active = true;
            reveal = 1f;
            visualAlpha = 1f;
            SetVerticesDirty();
        }

        public void AssignForEditor()
        {
            raycastTarget = false;
            maskable = true;
            color = Color.white;
            Clear();
        }
#endif
    }
}
