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

    public sealed class V02StageProgressBar : MonoBehaviour
    {
        private static readonly Vector2 NormalNodeSize = new(28f, 28f);
        private static readonly Vector2 BossNodeSize = new(46f, 62f);

        [SerializeField] private RectTransform root;
        [SerializeField] private RectTransform nodeRoot;
        [SerializeField] private Image baseLineImage;
        [SerializeField] private Image progressLineImage;
        [SerializeField] private bool showLabels = true;
        [SerializeField] private Color clearedColor = new(0.78f, 0.92f, 0.74f, 1f);
        [SerializeField] private Color currentColor = new(0.42f, 0.96f, 0.82f, 1f);
        [SerializeField] private Color lockedColor = new(0.07f, 0.08f, 0.07f, 1f);
        [SerializeField] private Color lineColor = new(0.02f, 0.025f, 0.02f, 1f);
        [SerializeField] private Color progressLineColor = new(0.28f, 0.62f, 0.5f, 1f);
        [SerializeField] private Color labelColor = new(0.82f, 0.88f, 0.82f, 1f);

        private readonly List<GameObject> runtimeNodes = new();
        private readonly List<V02StageProgressNodeData> lastNodes = new();

        private void Awake()
        {
            ResolveReferences();
            if (lastNodes.Count == 0)
            {
                SetProgress("1", 1, 9, 10);
            }
        }

        public void SetProgress(string chapterId, int currentStageIndex, int normalStageCount, int bossStageIndex)
        {
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

        public static List<V02StageProgressNodeData> BuildNodes(
            string chapterId,
            int currentStageIndex,
            int normalStageCount,
            int bossStageIndex)
        {
            string safeChapterId = string.IsNullOrWhiteSpace(chapterId) ? "1" : chapterId.Trim();
            int safeBossIndex = Mathf.Max(1, bossStageIndex);
            int safeNormalCount = Mathf.Clamp(normalStageCount, 0, safeBossIndex - 1);
            int totalNodeCount = Mathf.Max(safeBossIndex, safeNormalCount + 1);

            List<V02StageProgressNodeData> nodes = new(totalNodeCount);
            for (int stageIndex = 1; stageIndex <= totalNodeCount; stageIndex++)
            {
                bool isBoss = stageIndex == safeBossIndex;
                V02StageProgressNodeState state = ResolveState(stageIndex, currentStageIndex, safeBossIndex);
                string stageId = $"{safeChapterId}-{stageIndex}";
                string label = ShouldShowStageLabel(stageIndex, safeBossIndex) ? stageId : string.Empty;
                nodes.Add(new V02StageProgressNodeData(stageId, stageIndex, isBoss, state, label));
            }

            return nodes;
        }

        private static bool ShouldShowStageLabel(int stageIndex, int bossStageIndex)
        {
            return stageIndex == 1 || stageIndex == 5 || stageIndex == bossStageIndex;
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

            GameObject rootObject = new("V02StageProgressBar_Runtime", typeof(RectTransform));
            rootObject.transform.SetParent(parent, false);
            RectTransform rootRect = rootObject.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = anchoredPosition;
            rootRect.sizeDelta = size;

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

        private void RenderNodes()
        {
            if (root == null || nodeRoot == null)
            {
                return;
            }

            ClearRuntimeNodes();
            if (lastNodes.Count == 0)
            {
                SetLine(baseLineImage, 0f, 0f, 0f);
                SetLine(progressLineImage, 0f, 0f, 0f);
                return;
            }

            float width = GetWidth(root);
            float nodeCenterY = showLabels ? 10f : 0f;
            float leftPadding = NormalNodeSize.x * 0.5f + 8f;
            float rightPadding = BossNodeSize.x * 0.5f + 8f;
            float startX = -width * 0.5f + leftPadding;
            float endX = width * 0.5f - rightPadding;
            float progressEndX = startX;

            SetLine(baseLineImage, startX, endX, nodeCenterY);

            for (int i = 0; i < lastNodes.Count; i++)
            {
                V02StageProgressNodeData node = lastNodes[i];
                float t = lastNodes.Count <= 1 ? 0.5f : i / (float)(lastNodes.Count - 1);
                float x = Mathf.Lerp(startX, endX, t);
                CreateNode(node, new Vector2(x, nodeCenterY));

                if (node.state is V02StageProgressNodeState.Cleared or V02StageProgressNodeState.Current)
                {
                    progressEndX = x;
                }
            }

            SetLine(progressLineImage, startX, progressEndX, nodeCenterY);
        }

        private void CreateNode(V02StageProgressNodeData node, Vector2 anchoredPosition)
        {
            Vector2 shapeSize = node.isBoss ? BossNodeSize : NormalNodeSize;
            Vector2 containerSize = new(Mathf.Max(56f, shapeSize.x + 14f), showLabels ? 66f : Mathf.Max(46f, shapeSize.y));

            GameObject nodeObject = new($"Node_{node.stageId}", typeof(RectTransform));
            nodeObject.transform.SetParent(nodeRoot, false);
            RectTransform nodeRect = nodeObject.GetComponent<RectTransform>();
            nodeRect.anchorMin = new Vector2(0.5f, 0.5f);
            nodeRect.anchorMax = new Vector2(0.5f, 0.5f);
            nodeRect.pivot = new Vector2(0.5f, 0.5f);
            nodeRect.anchoredPosition = anchoredPosition;
            nodeRect.sizeDelta = containerSize;

            GameObject shapeObject = new("Shape", typeof(RectTransform), typeof(V02StageProgressNodeGraphic));
            shapeObject.transform.SetParent(nodeObject.transform, false);
            RectTransform shapeRect = shapeObject.GetComponent<RectTransform>();
            shapeRect.anchorMin = new Vector2(0.5f, 0.5f);
            shapeRect.anchorMax = new Vector2(0.5f, 0.5f);
            shapeRect.pivot = new Vector2(0.5f, 0.5f);
            shapeRect.anchoredPosition = showLabels ? new Vector2(0f, 9f) : Vector2.zero;
            shapeRect.sizeDelta = shapeSize;

            V02StageProgressNodeGraphic graphic = shapeObject.GetComponent<V02StageProgressNodeGraphic>();
            graphic.raycastTarget = false;
            graphic.SetVisual(
                node.isBoss,
                GetNodeFillColor(node.state),
                lineColor,
                node.isBoss ? 7f : 6f);

            if (showLabels && !string.IsNullOrWhiteSpace(node.label))
            {
                Text label = CreateText("Label", nodeObject.transform, node.label, 12, FontStyle.Bold, labelColor);
                RectTransform labelRect = label.GetComponent<RectTransform>();
                labelRect.anchorMin = new Vector2(0.5f, 0f);
                labelRect.anchorMax = new Vector2(0.5f, 0f);
                labelRect.pivot = new Vector2(0.5f, 0f);
                labelRect.anchoredPosition = new Vector2(0f, 0f);
                labelRect.sizeDelta = new Vector2(containerSize.x, 18f);
            }

            runtimeNodes.Add(nodeObject);
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

        private void SetLine(Image image, float startX, float endX, float y)
        {
            if (image == null)
            {
                return;
            }

            image.color = image == progressLineImage ? progressLineColor : lineColor;
            image.raycastTarget = false;
            RectTransform rect = image.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2((startX + endX) * 0.5f, y);
            rect.sizeDelta = new Vector2(Mathf.Max(0f, endX - startX), 7f);
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
