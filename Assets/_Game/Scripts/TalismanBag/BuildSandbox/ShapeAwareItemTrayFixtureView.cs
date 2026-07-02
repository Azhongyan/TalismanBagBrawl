using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.BuildSandbox
{
    public sealed class ShapeAwareItemTrayFixtureView : MonoBehaviour
    {
        private const float CellInset = 5f;
        private readonly List<Image> cellImages = new();
        private readonly List<ItemShapeCell> occupiedOffsets = new();
        private RectTransform rectTransform;
        private Text label;

        public string ShapeId { get; private set; } = string.Empty;
        public int AnchorSlotIndex { get; private set; } = -1;
        public int FootprintWidth { get; private set; } = 1;
        public int FootprintHeight { get; private set; } = 1;
        public IReadOnlyList<ItemShapeCell> OccupiedOffsets => occupiedOffsets;

        public void Configure(
            string shapeId,
            string displayName,
            IEnumerable<ItemShapeCell> offsets,
            bool rotationAllowed,
            Color cellColor,
            Vector2 slotSize,
            Vector2 slotSpacing,
            Vector2 anchorSlotCenter,
            int anchorSlotIndex)
        {
            ShapeId = shapeId ?? string.Empty;
            AnchorSlotIndex = anchorSlotIndex;
            occupiedOffsets.Clear();
            occupiedOffsets.AddRange(offsets ?? Enumerable.Empty<ItemShapeCell>());
            if (occupiedOffsets.Count == 0)
            {
                occupiedOffsets.Add(new ItemShapeCell(0, 0));
            }

            FootprintWidth = Mathf.Max(1, occupiedOffsets.Max(cell => cell.x) + 1);
            FootprintHeight = Mathf.Max(1, occupiedOffsets.Max(cell => cell.y) + 1);
            rectTransform = GetComponent<RectTransform>();
            ConfigureRoot(slotSize, slotSpacing, anchorSlotCenter);
            ConfigureRootRaycast();
            RebuildCells(cellColor, slotSize, slotSpacing);
            ConfigureLabel(displayName, rotationAllowed, slotSize, slotSpacing);
        }

        public IEnumerable<int> GetReservedSlotIndexes(int columnCount, int slotCount)
        {
            if (AnchorSlotIndex < 0 || columnCount <= 0)
            {
                yield break;
            }

            int anchorColumn = AnchorSlotIndex % columnCount;
            int anchorRow = AnchorSlotIndex / columnCount;
            foreach (ItemShapeCell offset in occupiedOffsets)
            {
                int column = anchorColumn + offset.x;
                int row = anchorRow + offset.y;
                int index = row * columnCount + column;
                if (column >= 0 && column < columnCount && index >= 0 && index < slotCount)
                {
                    yield return index;
                }
            }
        }

        public void SetShapeSelectedVisual(bool selected)
        {
            foreach (Image image in cellImages)
            {
                if (image == null)
                {
                    continue;
                }

                Outline outline = image.GetComponent<Outline>();
                if (outline == null)
                {
                    outline = image.gameObject.AddComponent<Outline>();
                }

                outline.effectColor = new Color(0.34f, 0.95f, 1f, 1f);
                outline.effectDistance = new Vector2(4f, -4f);
                outline.useGraphicAlpha = false;
                outline.enabled = selected;
            }
        }

        private void ConfigureRoot(Vector2 slotSize, Vector2 slotSpacing, Vector2 anchorSlotCenter)
        {
            if (rectTransform == null)
            {
                return;
            }

            Vector2 footprintSize = new(
                FootprintWidth * slotSize.x + Mathf.Max(0, FootprintWidth - 1) * slotSpacing.x,
                FootprintHeight * slotSize.y + Mathf.Max(0, FootprintHeight - 1) * slotSpacing.y);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.anchoredPosition = anchorSlotCenter + new Vector2(-slotSize.x * 0.5f, slotSize.y * 0.5f);
            rectTransform.sizeDelta = footprintSize;
            rectTransform.localScale = Vector3.one;
        }

        private void ConfigureRootRaycast()
        {
            Image image = GetComponent<Image>();
            if (image == null)
            {
                return;
            }

            image.color = Color.clear;
            image.raycastTarget = false;
        }

        private void RebuildCells(Color cellColor, Vector2 slotSize, Vector2 slotSpacing)
        {
            while (cellImages.Count < occupiedOffsets.Count)
            {
                GameObject cellObject = new($"ShapeCell_{cellImages.Count:00}", typeof(RectTransform), typeof(Image));
                cellObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                cellObject.transform.SetParent(transform, false);
                cellImages.Add(cellObject.GetComponent<Image>());
            }

            for (int i = 0; i < cellImages.Count; i++)
            {
                Image image = cellImages[i];
                if (image == null)
                {
                    continue;
                }

                bool active = i < occupiedOffsets.Count;
                image.gameObject.SetActive(active);
                if (!active)
                {
                    continue;
                }

                ItemShapeCell offset = occupiedOffsets[i];
                RectTransform cellRect = image.transform as RectTransform;
                if (cellRect != null)
                {
                    cellRect.anchorMin = new Vector2(0f, 1f);
                    cellRect.anchorMax = new Vector2(0f, 1f);
                    cellRect.pivot = new Vector2(0f, 1f);
                    cellRect.anchoredPosition = new Vector2(
                        offset.x * (slotSize.x + slotSpacing.x) + CellInset,
                        -offset.y * (slotSize.y + slotSpacing.y) - CellInset);
                    cellRect.sizeDelta = new Vector2(
                        Mathf.Max(12f, slotSize.x - CellInset * 2f),
                        Mathf.Max(12f, slotSize.y - CellInset * 2f));
                    cellRect.localScale = Vector3.one;
                }

                image.color = cellColor;
                image.raycastTarget = true;
            }
        }

        private void ConfigureLabel(string displayName, bool rotationAllowed, Vector2 slotSize, Vector2 slotSpacing)
        {
            if (label == null)
            {
                GameObject labelObject = new("ShapeAwareLabel", typeof(RectTransform), typeof(Text));
                labelObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                labelObject.transform.SetParent(transform, false);
                label = labelObject.GetComponent<Text>();
                label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                label.fontStyle = FontStyle.Bold;
                label.alignment = TextAnchor.MiddleCenter;
                label.raycastTarget = false;
            }

            RectTransform labelRect = label.transform as RectTransform;
            if (labelRect != null)
            {
                labelRect.anchorMin = new Vector2(0f, 1f);
                labelRect.anchorMax = new Vector2(0f, 1f);
                labelRect.pivot = new Vector2(0f, 1f);
                labelRect.anchoredPosition = new Vector2(CellInset, -CellInset);
                labelRect.sizeDelta = new Vector2(
                    FootprintWidth * slotSize.x + Mathf.Max(0, FootprintWidth - 1) * slotSpacing.x - CellInset * 2f,
                    FootprintHeight * slotSize.y + Mathf.Max(0, FootprintHeight - 1) * slotSpacing.y - CellInset * 2f);
                labelRect.localScale = Vector3.one;
            }

            label.text = $"{ShapeId}\n{displayName}";
            label.fontSize = FootprintWidth > 1 || FootprintHeight > 1 ? 14 : 13;
            label.color = rotationAllowed
                ? new Color(0.03f, 0.05f, 0.07f, 1f)
                : new Color(0.05f, 0.04f, 0.07f, 1f);
        }
    }
}
