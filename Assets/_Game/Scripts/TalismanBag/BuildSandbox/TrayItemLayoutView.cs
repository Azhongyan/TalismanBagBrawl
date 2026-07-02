using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.BuildSandbox
{
    public sealed class TrayItemLayoutView
    {
        private const int DefaultColumnCount = 5;
        private const string LayoutCellLayerName = "TrayLayoutCellLayer";

        private RectTransform contentRoot;
        private RectTransform cardLayer;
        private List<RectTransform> slotRects = new();
        private int columnCount = DefaultColumnCount;

        public void Bind(
            RectTransform trayContentRoot,
            RectTransform itemCardLayer,
            IReadOnlyList<RectTransform> traySlotRects,
            int trayColumnCount)
        {
            contentRoot = trayContentRoot;
            cardLayer = itemCardLayer;
            slotRects = (traySlotRects ?? Array.Empty<RectTransform>()).ToList();
            columnCount = Mathf.Max(1, trayColumnCount);
        }

        public void Refresh(
            IReadOnlyList<TrayPlacementViewModel> placements,
            IReadOnlyDictionary<string, BuildItemPreviewCardView> cardsByItemId,
            ISet<string> visibleItemIds)
        {
            if (cardLayer == null || cardsByItemId == null)
            {
                return;
            }

            foreach (TrayPlacementViewModel placement in placements ?? Array.Empty<TrayPlacementViewModel>())
            {
                if (placement == null
                    || !placement.isValid
                    || string.IsNullOrWhiteSpace(placement.itemId)
                    || (visibleItemIds != null && !visibleItemIds.Contains(placement.itemId))
                    || !cardsByItemId.TryGetValue(placement.itemId, out BuildItemPreviewCardView card)
                    || card == null)
                {
                    continue;
                }

                ApplyCardRect(card, placement);
            }
        }

        public bool TryScreenPointToSlotIndex(
            Vector2 screenPoint,
            Camera eventCamera,
            out int slotIndex)
        {
            for (int i = 0; i < slotRects.Count; i++)
            {
                RectTransform slotRect = slotRects[i];
                if (slotRect == null)
                {
                    continue;
                }

                if (RectTransformUtility.RectangleContainsScreenPoint(slotRect, screenPoint, eventCamera))
                {
                    slotIndex = i;
                    return true;
                }
            }

            slotIndex = -1;
            return false;
        }

        private void ApplyCardRect(BuildItemPreviewCardView card, TrayPlacementViewModel placement)
        {
            RectTransform cardRect = card.RectTransform;
            if (cardRect == null)
            {
                return;
            }

            cardRect.anchorMin = new Vector2(0f, 1f);
            cardRect.anchorMax = new Vector2(0f, 1f);
            cardRect.pivot = new Vector2(0f, 1f);
            if (TryResolveCardLayoutFromSlots(placement, out TrayCardLayout slotLayout))
            {
                ApplyCardLayout(card, cardRect, slotLayout);
                return;
            }

            GridLayoutMetrics metrics = ResolveGridLayoutMetrics();
            SlotBounds bounds = ResolveSlotBounds(placement);
            ApplyCardLayout(card, cardRect, BuildMetricsCardLayout(placement, metrics, bounds));
        }

        private void ApplyCardLayout(
            BuildItemPreviewCardView card,
            RectTransform cardRect,
            TrayCardLayout layout)
        {
            cardRect.anchoredPosition = layout.anchoredPosition;
            cardRect.sizeDelta = layout.sizeDelta;
            cardRect.localScale = Vector3.one;

            if (layout.usesCellVisuals)
            {
                ApplyLayoutCellVisuals(card, cardRect, layout.cellRects);
            }
            else
            {
                HideLayoutCellVisuals(card, cardRect);
            }
        }

        private bool TryResolveCardLayoutFromSlots(
            TrayPlacementViewModel placement,
            out TrayCardLayout layout)
        {
            layout = default;
            if (cardLayer == null || slotRects.Count == 0)
            {
                return false;
            }

            IReadOnlyList<int> occupiedSlots = placement.occupiedSlotIndexes ?? Array.Empty<int>();
            List<int> targetSlots = occupiedSlots.Count > 0
                ? occupiedSlots.Distinct().ToList()
                : new List<int> { Mathf.Max(0, placement.anchorSlotIndex) };

            float minX = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float minY = float.PositiveInfinity;
            float maxY = float.NegativeInfinity;
            Vector3[] worldCorners = new Vector3[4];
            List<LocalSlotRect> localSlotRects = new();
            foreach (int slotIndex in targetSlots)
            {
                if (slotIndex < 0 || slotIndex >= slotRects.Count || slotRects[slotIndex] == null)
                {
                    return false;
                }

                RectTransform slotRect = slotRects[slotIndex];
                slotRect.GetWorldCorners(worldCorners);
                float slotMinX = float.PositiveInfinity;
                float slotMaxX = float.NegativeInfinity;
                float slotMinY = float.PositiveInfinity;
                float slotMaxY = float.NegativeInfinity;
                for (int i = 0; i < worldCorners.Length; i++)
                {
                    Vector3 localCorner = cardLayer.InverseTransformPoint(worldCorners[i]);
                    slotMinX = Mathf.Min(slotMinX, localCorner.x);
                    slotMaxX = Mathf.Max(slotMaxX, localCorner.x);
                    slotMinY = Mathf.Min(slotMinY, localCorner.y);
                    slotMaxY = Mathf.Max(slotMaxY, localCorner.y);
                    minX = Mathf.Min(minX, localCorner.x);
                    maxX = Mathf.Max(maxX, localCorner.x);
                    minY = Mathf.Min(minY, localCorner.y);
                    maxY = Mathf.Max(maxY, localCorner.y);
                }

                localSlotRects.Add(new LocalSlotRect
                {
                    minX = slotMinX,
                    maxX = slotMaxX,
                    minY = slotMinY,
                    maxY = slotMaxY
                });
            }

            if (!IsFinite(minX)
                || !IsFinite(maxX)
                || !IsFinite(minY)
                || !IsFinite(maxY)
                || maxX <= minX
                || maxY <= minY)
            {
                return false;
            }

            Vector2 parentTopLeft = new(
                -cardLayer.rect.width * cardLayer.pivot.x,
                cardLayer.rect.height * (1f - cardLayer.pivot.y));
            layout = new TrayCardLayout
            {
                anchoredPosition = new Vector2(minX - parentTopLeft.x, maxY - parentTopLeft.y),
                sizeDelta = new Vector2(maxX - minX, maxY - minY),
                usesCellVisuals = IsNonRectangularPlacement(placement)
            };

            if (layout.usesCellVisuals)
            {
                foreach (LocalSlotRect slotRect in localSlotRects)
                {
                    layout.cellRects.Add(new TrayCardCellRect
                    {
                        anchoredPosition = new Vector2(slotRect.minX - minX, -(maxY - slotRect.maxY)),
                        sizeDelta = new Vector2(slotRect.maxX - slotRect.minX, slotRect.maxY - slotRect.minY)
                    });
                }
            }

            return true;
        }

        private TrayCardLayout BuildMetricsCardLayout(
            TrayPlacementViewModel placement,
            GridLayoutMetrics metrics,
            SlotBounds bounds)
        {
            TrayCardLayout layout = new()
            {
                anchoredPosition = new Vector2(
                    metrics.paddingLeft + bounds.minColumn * (metrics.cellSize.x + metrics.spacing.x),
                    -metrics.paddingTop - bounds.minRow * (metrics.cellSize.y + metrics.spacing.y)),
                sizeDelta = new Vector2(
                    bounds.columnSpan * metrics.cellSize.x + Mathf.Max(0, bounds.columnSpan - 1) * metrics.spacing.x,
                    bounds.rowSpan * metrics.cellSize.y + Mathf.Max(0, bounds.rowSpan - 1) * metrics.spacing.y),
                usesCellVisuals = IsNonRectangularPlacement(placement)
            };

            if (!layout.usesCellVisuals)
            {
                return layout;
            }

            foreach (int slotIndex in (placement.occupiedSlotIndexes ?? Array.Empty<int>()).Distinct())
            {
                int safeSlotIndex = Mathf.Max(0, slotIndex);
                int column = safeSlotIndex % columnCount;
                int row = safeSlotIndex / columnCount;
                layout.cellRects.Add(new TrayCardCellRect
                {
                    anchoredPosition = new Vector2(
                        (column - bounds.minColumn) * (metrics.cellSize.x + metrics.spacing.x),
                        -(row - bounds.minRow) * (metrics.cellSize.y + metrics.spacing.y)),
                    sizeDelta = metrics.cellSize
                });
            }

            return layout;
        }

        private void ApplyLayoutCellVisuals(
            BuildItemPreviewCardView card,
            RectTransform cardRect,
            IReadOnlyList<TrayCardCellRect> cellRects)
        {
            RectTransform layer = EnsureLayoutCellLayer(cardRect);
            if (layer == null)
            {
                card.SetLayoutCellVisuals(Array.Empty<Image>());
                return;
            }

            layer.sizeDelta = cardRect.sizeDelta;
            while (layer.childCount < cellRects.Count)
            {
                GameObject cellObject = new($"TrayLayoutCell_{layer.childCount:00}", typeof(RectTransform), typeof(Image));
                cellObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                cellObject.transform.SetParent(layer, false);
            }

            List<Image> activeImages = new();
            for (int i = 0; i < layer.childCount; i++)
            {
                Transform child = layer.GetChild(i);
                Image image = child.GetComponent<Image>();
                bool active = i < cellRects.Count;
                child.gameObject.SetActive(active);
                if (!active || image == null)
                {
                    continue;
                }

                TrayCardCellRect cellRectModel = cellRects[i];
                RectTransform cellRect = child as RectTransform;
                if (cellRect != null)
                {
                    cellRect.anchorMin = new Vector2(0f, 1f);
                    cellRect.anchorMax = new Vector2(0f, 1f);
                    cellRect.pivot = new Vector2(0f, 1f);
                    cellRect.anchoredPosition = cellRectModel.anchoredPosition;
                    cellRect.sizeDelta = cellRectModel.sizeDelta;
                    cellRect.localScale = Vector3.one;
                }

                activeImages.Add(image);
            }

            card.SetLayoutCellVisuals(activeImages);
        }

        private void HideLayoutCellVisuals(BuildItemPreviewCardView card, RectTransform cardRect)
        {
            Transform layer = cardRect.Find(LayoutCellLayerName);
            if (layer != null)
            {
                for (int i = 0; i < layer.childCount; i++)
                {
                    layer.GetChild(i).gameObject.SetActive(false);
                }
            }

            card.SetLayoutCellVisuals(Array.Empty<Image>());
        }

        private static RectTransform EnsureLayoutCellLayer(RectTransform cardRect)
        {
            RectTransform layer = cardRect.Find(LayoutCellLayerName) as RectTransform;
            if (layer == null)
            {
                GameObject layerObject = new(LayoutCellLayerName, typeof(RectTransform));
                layerObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                layerObject.transform.SetParent(cardRect, false);
                layer = layerObject.GetComponent<RectTransform>();
            }

            layer.SetAsFirstSibling();
            layer.anchorMin = new Vector2(0f, 1f);
            layer.anchorMax = new Vector2(0f, 1f);
            layer.pivot = new Vector2(0f, 1f);
            layer.anchoredPosition = Vector2.zero;
            layer.localScale = Vector3.one;
            return layer;
        }

        private bool IsNonRectangularPlacement(TrayPlacementViewModel placement)
        {
            IReadOnlyList<int> occupiedSlots = placement.occupiedSlotIndexes ?? Array.Empty<int>();
            int occupiedCount = occupiedSlots.Distinct().Count();
            if (occupiedCount <= 1)
            {
                return false;
            }

            SlotBounds bounds = ResolveSlotBounds(placement);
            return occupiedCount < bounds.columnSpan * bounds.rowSpan;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private GridLayoutMetrics ResolveGridLayoutMetrics()
        {
            GridLayoutGroup grid = contentRoot == null ? null : contentRoot.GetComponent<GridLayoutGroup>();
            if (grid != null)
            {
                return new GridLayoutMetrics
                {
                    cellSize = grid.cellSize,
                    spacing = grid.spacing,
                    paddingLeft = grid.padding.left,
                    paddingTop = grid.padding.top
                };
            }

            RectTransform firstSlot = slotRects.FirstOrDefault(slot => slot != null);
            Vector2 cellSize = firstSlot == null ? new Vector2(70f, 34f) : firstSlot.rect.size;
            return new GridLayoutMetrics
            {
                cellSize = cellSize,
                spacing = new Vector2(8f, 4f),
                paddingLeft = 8f,
                paddingTop = 6f
            };
        }

        private SlotBounds ResolveSlotBounds(TrayPlacementViewModel placement)
        {
            IReadOnlyList<int> occupiedSlots = placement.occupiedSlotIndexes ?? Array.Empty<int>();
            if (occupiedSlots.Count == 0)
            {
                int anchor = Mathf.Max(0, placement.anchorSlotIndex);
                return new SlotBounds
                {
                    minColumn = anchor % columnCount,
                    minRow = anchor / columnCount,
                    columnSpan = 1,
                    rowSpan = 1
                };
            }

            int minColumn = int.MaxValue;
            int maxColumn = int.MinValue;
            int minRow = int.MaxValue;
            int maxRow = int.MinValue;
            foreach (int slotIndex in occupiedSlots)
            {
                int safeSlotIndex = Mathf.Max(0, slotIndex);
                int column = safeSlotIndex % columnCount;
                int row = safeSlotIndex / columnCount;
                minColumn = Mathf.Min(minColumn, column);
                maxColumn = Mathf.Max(maxColumn, column);
                minRow = Mathf.Min(minRow, row);
                maxRow = Mathf.Max(maxRow, row);
            }

            return new SlotBounds
            {
                minColumn = minColumn,
                minRow = minRow,
                columnSpan = Mathf.Max(1, maxColumn - minColumn + 1),
                rowSpan = Mathf.Max(1, maxRow - minRow + 1)
            };
        }

        private struct GridLayoutMetrics
        {
            public Vector2 cellSize;
            public Vector2 spacing;
            public float paddingLeft;
            public float paddingTop;
        }

        private struct SlotBounds
        {
            public int minColumn;
            public int minRow;
            public int columnSpan;
            public int rowSpan;
        }

        private struct LocalSlotRect
        {
            public float minX;
            public float maxX;
            public float minY;
            public float maxY;
        }

        private struct TrayCardCellRect
        {
            public Vector2 anchoredPosition;
            public Vector2 sizeDelta;
        }

        private sealed class TrayCardLayout
        {
            public Vector2 anchoredPosition;
            public Vector2 sizeDelta;
            public bool usesCellVisuals;
            public readonly List<TrayCardCellRect> cellRects = new();
        }
    }
}
