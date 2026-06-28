using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.V02.UI
{
    public enum V02StageProgressNodeState
    {
        Cleared,
        Current,
        Locked
    }

    [Serializable]
    public sealed class V02StageProgressNodeData
    {
        public string stageId;
        public int stageIndex;
        public bool isBoss;
        public V02StageProgressNodeState state;
        public string label;

        public V02StageProgressNodeData()
        {
        }

        public V02StageProgressNodeData(
            string stageId,
            int stageIndex,
            bool isBoss,
            V02StageProgressNodeState state,
            string label = "")
        {
            this.stageId = stageId;
            this.stageIndex = stageIndex;
            this.isBoss = isBoss;
            this.state = state;
            this.label = label ?? string.Empty;
        }
    }

    [ExecuteAlways]
    public sealed class V02StageProgressBar : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private RectTransform nodeRoot;
        [SerializeField] private Image baseLineImage;
        [SerializeField] private Image progressLineImage;
        [SerializeField] private bool showLabels = true;
        [SerializeField] private bool preserveManualNodeLayout = true;
        [SerializeField] private Vector2 normalDotSize = new(28f, 28f);
        [SerializeField] private Vector2 bossDotSize = new(56f, 76f);
        [SerializeField] private Vector2 dotOffsetWithLabels = new(0f, 9f);
        [SerializeField] private Vector2 dotOffsetWithoutLabels = Vector2.zero;
        [SerializeField] private float edgePadding = 8f;
        [SerializeField] private float lineHeight = 7f;
        [SerializeField] private float normalRingWidth = 6f;
        [SerializeField] private float bossRingWidth = 7f;
        [SerializeField] private Color clearedColor = new(0.78f, 0.92f, 0.74f, 1f);
        [SerializeField] private Color currentColor = new(0.42f, 0.96f, 0.82f, 1f);
        [SerializeField] private Color lockedColor = new(0.07f, 0.08f, 0.07f, 1f);
        [SerializeField] private Color lineColor = new(0.02f, 0.025f, 0.02f, 1f);
        [SerializeField] private Color progressLineColor = new(0.28f, 0.62f, 0.5f, 1f);
        [SerializeField] private Color labelColor = new(0.82f, 0.88f, 0.82f, 1f);

        private readonly List<GameObject> runtimeNodes = new();
        private readonly List<V02StageProgressNodeData> lastNodes = new();
        private int lastCurrentStageIndex = 1;
        private int lastBossStageIndex = 10;

        private void Awake()
        {
            ResolveReferences();
            if (lastNodes.Count == 0)
            {
                SetProgress("1", 1, 9, 10);
            }
        }

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                return;
            }

            ResolveReferences();
            if (lastNodes.Count == 0)
            {
                SetProgress("1", 1, 9, 10);
            }
        }

        public void SetProgress(string chapterId, int currentStageIndex, int normalStageCount, int bossStageIndex)
        {
            lastBossStageIndex = Mathf.Max(1, bossStageIndex);
            lastCurrentStageIndex = Mathf.Clamp(currentStageIndex, 1, lastBossStageIndex);
            SetNodes(BuildNodes(chapterId, currentStageIndex, normalStageCount, bossStageIndex));
        }

        public void SetNodes(IReadOnlyList<V02StageProgressNodeData> nodeData)
        {
            ResolveReferences();
            lastNodes.Clear();
            if (nodeData != null)
            {
                for (int i = 0; i < nodeData.Count; i++)
                {
                    V02StageProgressNodeData node = nodeData[i];
                    if (node == null)
                    {
                        continue;
                    }

                    lastNodes.Add(new V02StageProgressNodeData(
                        node.stageId,
                        node.stageIndex,
                        node.isBoss,
                        node.state,
                        node.label));
                }
            }

            RenderNodes();
        }

        public void SetShowLabels(bool visible)
        {
            showLabels = visible;
            RenderNodes();
        }

        [ContextMenu("Refresh Preview Nodes")]
        private void RefreshPreviewNodes()
        {
            int safeBossStageIndex = Mathf.Max(1, lastBossStageIndex);
            SetProgress("1", Mathf.Clamp(lastCurrentStageIndex, 1, safeBossStageIndex), safeBossStageIndex - 1, safeBossStageIndex);
        }

        public static List<V02StageProgressNodeData> BuildNodes(
            string chapterId,
            int currentStageIndex,
            int normalStageCount,
            int bossStageIndex)
        {
            string safeChapterId = string.IsNullOrWhiteSpace(chapterId) ? "1" : chapterId.Trim();
            int safeBossIndex = Mathf.Max(1, bossStageIndex);
            List<int> keyStageIndexes = BuildKeyStageIndexes(safeBossIndex);

            List<V02StageProgressNodeData> nodes = new(keyStageIndexes.Count);
            for (int i = 0; i < keyStageIndexes.Count; i++)
            {
                int stageIndex = keyStageIndexes[i];
                bool isBoss = stageIndex == safeBossIndex;
                V02StageProgressNodeState state = ResolveState(stageIndex, currentStageIndex, safeBossIndex);
                string stageId = $"{safeChapterId}-{stageIndex}";
                nodes.Add(new V02StageProgressNodeData(stageId, stageIndex, isBoss, state, stageId));
            }

            return nodes;
        }

        private static List<int> BuildKeyStageIndexes(int bossStageIndex)
        {
            List<int> stageIndexes = new() { 1 };
            AddKeyStageIndex(stageIndexes, 5, bossStageIndex);
            AddKeyStageIndex(stageIndexes, bossStageIndex, bossStageIndex);
            return stageIndexes;
        }

        private static void AddKeyStageIndex(List<int> stageIndexes, int stageIndex, int bossStageIndex)
        {
            int clampedStageIndex = Mathf.Clamp(stageIndex, 1, Mathf.Max(1, bossStageIndex));
            if (!stageIndexes.Contains(clampedStageIndex))
            {
                stageIndexes.Add(clampedStageIndex);
            }
        }

        public static V02StageProgressBar CreateRuntime(
            Transform parent,
            Vector2 anchoredPosition,
            Vector2 size,
            bool showStageLabels = true)
        {
            if (parent == null)
            {
                return null;
            }

            GameObject rootObject = new("V02StageProgressBar_Runtime", typeof(RectTransform), typeof(LayoutElement));
            rootObject.transform.SetParent(parent, false);
            RectTransform rootRect = rootObject.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = anchoredPosition;
            rootRect.sizeDelta = size;
            rootObject.GetComponent<LayoutElement>().ignoreLayout = true;

            GameObject baseLineObject = new("BaseLine", typeof(RectTransform), typeof(Image));
            baseLineObject.transform.SetParent(rootObject.transform, false);
            Image baseLine = baseLineObject.GetComponent<Image>();

            GameObject progressLineObject = new("ProgressLine", typeof(RectTransform), typeof(Image));
            progressLineObject.transform.SetParent(rootObject.transform, false);
            Image progressLine = progressLineObject.GetComponent<Image>();

            GameObject nodeRootObject = new("NodeRoot", typeof(RectTransform));
            nodeRootObject.transform.SetParent(rootObject.transform, false);
            RectTransform nodeRootRect = nodeRootObject.GetComponent<RectTransform>();
            FillRect(nodeRootRect);

            V02StageProgressBar progressBar = rootObject.AddComponent<V02StageProgressBar>();
            progressBar.root = rootRect;
            progressBar.nodeRoot = nodeRootRect;
            progressBar.baseLineImage = baseLine;
            progressBar.progressLineImage = progressLine;
            progressBar.showLabels = showStageLabels;
            progressBar.SetProgress("1", 1, 9, 10);
            return progressBar;
        }

        private static V02StageProgressNodeState ResolveState(int stageIndex, int currentStageIndex, int bossStageIndex)
        {
            if (currentStageIndex > bossStageIndex)
            {
                return V02StageProgressNodeState.Cleared;
            }

            if (currentStageIndex <= 0 || stageIndex > currentStageIndex)
            {
                return V02StageProgressNodeState.Locked;
            }

            return stageIndex < currentStageIndex
                ? V02StageProgressNodeState.Cleared
                : V02StageProgressNodeState.Current;
        }

        private void ResolveReferences()
        {
            root ??= transform as RectTransform;
            nodeRoot ??= root;
            EnsureIgnoredByParentLayout();

            if (baseLineImage == null)
            {
                Transform baseLine = transform.Find("BaseLine");
                baseLineImage = baseLine != null ? baseLine.GetComponent<Image>() : null;
            }

            if (progressLineImage == null)
            {
                Transform progressLine = transform.Find("ProgressLine");
                progressLineImage = progressLine != null ? progressLine.GetComponent<Image>() : null;
            }
        }

        private void EnsureIgnoredByParentLayout()
        {
            if (!TryGetComponent(out LayoutElement layoutElement))
            {
                layoutElement = gameObject.AddComponent<LayoutElement>();
            }

            layoutElement.ignoreLayout = true;
        }

        private void RenderNodes()
        {
            if (root == null || nodeRoot == null)
            {
                return;
            }

            if (lastNodes.Count == 0)
            {
                ClearRuntimeNodes();
                SetProgressLine(0f, 0f, 0f);
                return;
            }

            RemoveExtraRuntimeNodes();
            runtimeNodes.Clear();

            GetBaseLineMetrics(out float startX, out float endX, out float lineY);
            int axisEndStageIndex = Mathf.Max(1, lastBossStageIndex);
            for (int i = 0; i < lastNodes.Count; i++)
            {
                axisEndStageIndex = Mathf.Max(axisEndStageIndex, lastNodes[i].stageIndex);
            }

            float progressT = GetStageProgressT(lastCurrentStageIndex, axisEndStageIndex);
            float progressEndX = Mathf.Lerp(startX, endX, progressT);

            for (int i = 0; i < lastNodes.Count; i++)
            {
                V02StageProgressNodeData node = lastNodes[i];
                float t = GetStageProgressT(node.stageIndex, axisEndStageIndex);
                float x = Mathf.Lerp(startX, endX, t);
                CreateNode(node, new Vector2(x, lineY));
            }

            SetProgressLine(startX, progressEndX, lineY);
        }

        private static float GetStageProgressT(int stageIndex, int axisEndStageIndex)
        {
            int safeAxisEnd = Mathf.Max(1, axisEndStageIndex);
            int safeStageIndex = Mathf.Clamp(stageIndex, 1, safeAxisEnd);
            return safeStageIndex / (float)safeAxisEnd;
        }

        private void GetBaseLineMetrics(out float startX, out float endX, out float y)
        {
            RectTransform baseLineRect = baseLineImage != null ? baseLineImage.GetComponent<RectTransform>() : null;
            if (baseLineRect != null)
            {
                float width = GetWidth(baseLineRect);
                y = baseLineRect.anchoredPosition.y;
                startX = baseLineRect.anchoredPosition.x - width * baseLineRect.pivot.x;
                endX = startX + width;
                return;
            }

            float rootWidth = GetWidth(root);
            Vector2 safeNormalDotSize = GetNormalDotSize();
            Vector2 safeBossDotSize = GetBossDotSize();
            float safeEdgePadding = Mathf.Max(0f, edgePadding);
            startX = -rootWidth * 0.5f + safeNormalDotSize.x * 0.5f + safeEdgePadding;
            endX = rootWidth * 0.5f - safeBossDotSize.x * 0.5f - safeEdgePadding;
            y = showLabels ? 10f : 0f;
        }

        private void CreateNode(V02StageProgressNodeData node, Vector2 anchoredPosition)
        {
            Vector2 shapeSize = node.isBoss ? GetBossDotSize() : GetNormalDotSize();
            Vector2 dotOffset = showLabels ? dotOffsetWithLabels : dotOffsetWithoutLabels;
            Vector2 containerSize = new(
                Mathf.Max(56f, shapeSize.x + Mathf.Abs(dotOffset.x) + 14f),
                showLabels
                    ? Mathf.Max(66f, shapeSize.y + Mathf.Abs(dotOffset.y) + 22f)
                    : Mathf.Max(46f, shapeSize.y + Mathf.Abs(dotOffset.y)));

            string nodeName = $"Node_{node.stageId}";
            Transform existingNode = nodeRoot.Find(nodeName);
            bool nodeCreated = existingNode == null;
            GameObject nodeObject;
            if (existingNode != null)
            {
                nodeObject = existingNode.gameObject;
            }
            else
            {
                nodeObject = new GameObject(nodeName, typeof(RectTransform));
                nodeObject.transform.SetParent(nodeRoot, false);
            }

            RectTransform nodeRect = nodeObject.GetComponent<RectTransform>();
            bool resetNodeLayout = nodeCreated || !preserveManualNodeLayout;
            if (resetNodeLayout)
            {
                nodeRect.anchorMin = new Vector2(0.5f, 0.5f);
                nodeRect.anchorMax = new Vector2(0.5f, 0.5f);
                nodeRect.pivot = new Vector2(0.5f, 0.5f);
                nodeRect.anchoredPosition = anchoredPosition;
                nodeRect.sizeDelta = containerSize;
            }

            string shapeName = node.isBoss ? $"BossDot_{node.stageId}" : $"Dot_{node.stageId}";
            V02StageProgressNodeGraphic graphic = GetOrCreateNodeGraphic(nodeObject.transform, shapeName, out RectTransform shapeRect, out bool shapeCreated);
            bool resetShapeLayout = shapeCreated || !preserveManualNodeLayout;
            if (resetShapeLayout)
            {
                shapeRect.anchorMin = new Vector2(0.5f, 0.5f);
                shapeRect.anchorMax = new Vector2(0.5f, 0.5f);
                shapeRect.pivot = new Vector2(0.5f, 0.5f);
                shapeRect.anchoredPosition = dotOffset;
                shapeRect.sizeDelta = shapeSize;
            }

            graphic.raycastTarget = false;
            graphic.SetVisual(
                node.isBoss,
                GetNodeFillColor(node.state),
                lineColor,
                node.isBoss ? Mathf.Max(1f, bossRingWidth) : Mathf.Max(1f, normalRingWidth),
                preserveManualNodeLayout && !shapeCreated);

            Transform labelTransform = nodeObject.transform.Find("Label");
            if (showLabels && !string.IsNullOrWhiteSpace(node.label))
            {
                bool labelCreated = false;
                Text label = labelTransform != null ? labelTransform.GetComponent<Text>() : null;
                if (label == null)
                {
                    label = CreateText("Label", nodeObject.transform, node.label, 12, FontStyle.Bold, labelColor);
                    labelCreated = true;
                }

                RectTransform labelRect = label.GetComponent<RectTransform>();
                if (labelCreated || !preserveManualNodeLayout)
                {
                    labelRect.anchorMin = new Vector2(0.5f, 0f);
                    labelRect.anchorMax = new Vector2(0.5f, 0f);
                    labelRect.pivot = new Vector2(0.5f, 0f);
                    labelRect.anchoredPosition = new Vector2(0f, 0f);
                    labelRect.sizeDelta = new Vector2(containerSize.x, 18f);
                }

                label.text = node.label;
                label.color = labelColor;
            }
            else if (labelTransform != null)
            {
                DestroyNodeObject(labelTransform.gameObject);
            }

            runtimeNodes.Add(nodeObject);
        }

        private V02StageProgressNodeGraphic GetOrCreateNodeGraphic(
            Transform parent,
            string shapeName,
            out RectTransform shapeRect,
            out bool created)
        {
            created = false;
            Transform shapeTransform = parent.Find(shapeName);
            if (shapeTransform == null)
            {
                shapeTransform = parent.Find("Shape");
            }

            V02StageProgressNodeGraphic graphic = shapeTransform != null
                ? shapeTransform.GetComponent<V02StageProgressNodeGraphic>()
                : null;

            if (graphic == null && shapeTransform != null)
            {
                graphic = shapeTransform.gameObject.AddComponent<V02StageProgressNodeGraphic>();
            }

            if (graphic == null)
            {
                GameObject shapeObject = new GameObject(shapeName, typeof(RectTransform), typeof(CanvasRenderer), typeof(V02StageProgressNodeGraphic));
                shapeObject.transform.SetParent(parent, false);
                graphic = shapeObject.GetComponent<V02StageProgressNodeGraphic>();
                shapeTransform = shapeObject.transform;
                created = true;
            }

            if (shapeTransform.name == "Shape")
            {
                shapeTransform.name = shapeName;
            }

            if (shapeTransform.GetComponent<CanvasRenderer>() == null)
            {
                shapeTransform.gameObject.AddComponent<CanvasRenderer>();
            }

            shapeRect = shapeTransform.GetComponent<RectTransform>();
            return graphic;
        }

        private Color GetNodeFillColor(V02StageProgressNodeState state)
        {
            return state switch
            {
                V02StageProgressNodeState.Cleared => clearedColor,
                V02StageProgressNodeState.Current => currentColor,
                _ => lockedColor
            };
        }

        private void SetProgressLine(float startX, float endX, float y)
        {
            if (progressLineImage == null)
            {
                return;
            }

            progressLineImage.color = progressLineColor;
            progressLineImage.raycastTarget = false;
            RectTransform rect = progressLineImage.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2((startX + endX) * 0.5f, y);
            rect.sizeDelta = new Vector2(Mathf.Max(0f, endX - startX), Mathf.Max(1f, lineHeight));
        }

        private Vector2 GetNormalDotSize()
        {
            return EnsurePositiveSize(normalDotSize, new Vector2(28f, 28f));
        }

        private Vector2 GetBossDotSize()
        {
            return EnsurePositiveSize(bossDotSize, new Vector2(56f, 76f));
        }

        private static Vector2 EnsurePositiveSize(Vector2 value, Vector2 fallback)
        {
            float width = value.x > 0f ? value.x : fallback.x;
            float height = value.y > 0f ? value.y : fallback.y;
            return new Vector2(Mathf.Max(1f, width), Mathf.Max(1f, height));
        }

        private void RemoveExtraRuntimeNodes()
        {
            if (nodeRoot == null)
            {
                return;
            }

            HashSet<string> desiredNodeNames = new();
            for (int i = 0; i < lastNodes.Count; i++)
            {
                desiredNodeNames.Add($"Node_{lastNodes[i].stageId}");
            }

            for (int i = nodeRoot.childCount - 1; i >= 0; i--)
            {
                Transform child = nodeRoot.GetChild(i);
                if (child != null &&
                    child.name.StartsWith("Node_", StringComparison.Ordinal) &&
                    !desiredNodeNames.Contains(child.name))
                {
                    DestroyNodeObject(child.gameObject);
                }
            }
        }

        private void ClearRuntimeNodes()
        {
            for (int i = 0; i < runtimeNodes.Count; i++)
            {
                GameObject nodeObject = runtimeNodes[i];
                if (nodeObject != null && (nodeRoot == null || nodeObject.transform.parent != nodeRoot))
                {
                    DestroyNodeObject(nodeObject);
                }
            }

            runtimeNodes.Clear();

            if (nodeRoot == null)
            {
                return;
            }

            for (int i = nodeRoot.childCount - 1; i >= 0; i--)
            {
                Transform child = nodeRoot.GetChild(i);
                if (child != null && child.name.StartsWith("Node_", StringComparison.Ordinal))
                {
                    DestroyNodeObject(child.gameObject);
                }
            }
        }

        private static void DestroyNodeObject(GameObject nodeObject)
        {
            if (nodeObject == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(nodeObject);
            }
            else
            {
                DestroyImmediate(nodeObject);
            }
        }

        private static float GetWidth(RectTransform rect)
        {
            if (rect == null)
            {
                return 1f;
            }

            float width = rect.rect.width;
            if (width <= 1f)
            {
                width = rect.sizeDelta.x;
            }

            return Mathf.Max(1f, width);
        }

        private static Text CreateText(string name, Transform parent, string value, int fontSize, FontStyle style, Color color)
        {
            GameObject textObject = new(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.text = value;
            text.raycastTarget = false;
            return text;
        }

        private static void FillRect(RectTransform rect)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
        }
    }

}
