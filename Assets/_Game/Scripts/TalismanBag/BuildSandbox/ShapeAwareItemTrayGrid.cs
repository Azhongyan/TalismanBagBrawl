using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TalismanBag.BuildSandbox
{
    public sealed class ShapeAwareItemTrayGrid : ShapeGridReceiver
    {
        public const int DefaultColumnCount = 5;
        public const int DefaultSlotCount = 40;

        private readonly Dictionary<ItemShapeCell, string> occupiedByItemId = new();
        private readonly Dictionary<string, ShapeAwareItemTrayGridPlacement> placementsByItemId =
            new(StringComparer.Ordinal);
        private readonly Vector2 screenOrigin;
        private readonly Vector2 cellSize;

        public ShapeAwareItemTrayGrid(
            string receiverId = "dev_shape_aware_item_tray_grid",
            int columnCount = DefaultColumnCount,
            int slotCount = DefaultSlotCount,
            bool commitAllowed = true,
            Vector2? screenOrigin = null,
            Vector2? cellSize = null)
        {
            if (columnCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(columnCount), "Column count must be positive.");
            }

            if (slotCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(slotCount), "Slot count must be positive.");
            }

            ReceiverId = string.IsNullOrWhiteSpace(receiverId)
                ? "dev_shape_aware_item_tray_grid"
                : receiverId;
            ColumnCount = columnCount;
            SlotCount = slotCount;
            RowCount = Mathf.CeilToInt(slotCount / (float)columnCount);
            CommitAllowed = commitAllowed;
            this.screenOrigin = screenOrigin ?? Vector2.zero;
            this.cellSize = cellSize ?? Vector2.one;
        }

        public string ReceiverId { get; }
        public ShapePlacementSource ReceiverSource => ShapePlacementSource.Tray;
        public int ColumnCount { get; }
        public int SlotCount { get; }
        public int RowCount { get; }
        public bool CommitAllowed { get; }
        public IReadOnlyDictionary<ItemShapeCell, string> OccupiedCells => occupiedByItemId;
        public IReadOnlyDictionary<string, ShapeAwareItemTrayGridPlacement> Placements => placementsByItemId;
        public ShapePlacementResult LastPreviewResult { get; private set; }
        public ShapePlacementResult LastCommitResult { get; private set; }
        public int PreviewCount { get; private set; }
        public int CommitCount { get; private set; }
        public int CancelCount { get; private set; }

        public bool ScreenPointToCell(Vector2 screenPoint, Camera eventCamera, out ItemShapeCell anchorCell)
        {
            anchorCell = default;
            if (cellSize.x <= 0f || cellSize.y <= 0f)
            {
                return false;
            }

            ItemShapeCell candidate = new(
                Mathf.FloorToInt((screenPoint.x - screenOrigin.x) / cellSize.x),
                Mathf.FloorToInt((screenPoint.y - screenOrigin.y) / cellSize.y));
            if (!IsWithinBounds(candidate))
            {
                return false;
            }

            anchorCell = candidate;
            return true;
        }

        public ShapePlacementResult CanPlace(ShapeItemPayload payload, ItemShapeCell anchorCell)
        {
            if (!payload.IsValid)
            {
                return BuildResult(payload, anchorCell, Array.Empty<ItemShapeCell>(), false, ShapePlacementInvalidReason.ShapeInvalid);
            }

            IReadOnlyList<ItemShapeCell> occupiedCells = BuildOccupiedCells(payload, anchorCell);
            if (occupiedCells.Count == 0)
            {
                return BuildResult(payload, anchorCell, occupiedCells, false, ShapePlacementInvalidReason.ShapeInvalid);
            }

            if (occupiedCells.Any(cell => !IsWithinBounds(cell)))
            {
                return BuildResult(payload, anchorCell, occupiedCells, false, ShapePlacementInvalidReason.OutOfGrid);
            }

            if (occupiedCells.Any(cell =>
                    occupiedByItemId.TryGetValue(cell, out string itemId)
                    && !string.Equals(itemId, payload.ItemId, StringComparison.Ordinal)))
            {
                return BuildResult(payload, anchorCell, occupiedCells, false, ShapePlacementInvalidReason.CellOccupied);
            }

            return BuildResult(payload, anchorCell, occupiedCells, true, ShapePlacementInvalidReason.None);
        }

        public void Preview(ShapePlacementSession session, ShapePlacementResult result)
        {
            LastPreviewResult = result;
            PreviewCount++;
        }

        public ShapePlacementResult Commit(ShapePlacementSession session)
        {
            if (session == null || !session.HasActivePayload)
            {
                LastCommitResult = BuildResult(
                    default,
                    default,
                    Array.Empty<ItemShapeCell>(),
                    false,
                    ShapePlacementInvalidReason.ShapeInvalid);
                CommitCount++;
                return LastCommitResult;
            }

            ItemShapeCell anchor = session.TrayAnchorCell
                ?? session.LastLegalTrayAnchor
                ?? session.PreviewResult?.AnchorCell
                ?? default;
            ShapePlacementResult result = CanPlace(session.CurrentPayload, anchor);
            if (!CommitAllowed && result.IsValid)
            {
                result = BuildResult(
                    session.CurrentPayload,
                    anchor,
                    result.OccupiedCells,
                    false,
                    ShapePlacementInvalidReason.CommitDisabled);
            }

            if (result.IsValid)
            {
                ReleaseItem(session.CurrentPayload.ItemId);
                foreach (ItemShapeCell cell in result.OccupiedCells)
                {
                    occupiedByItemId[cell] = session.CurrentPayload.ItemId;
                }

                placementsByItemId[session.CurrentPayload.ItemId] =
                    new ShapeAwareItemTrayGridPlacement(
                        session.CurrentPayload.ItemId,
                        session.CurrentPayload.ShapeId,
                        session.CurrentPayload.Rotation,
                        result.AnchorCell,
                        result.OccupiedCells,
                        ColumnCount);
            }

            LastCommitResult = result;
            CommitCount++;
            return result;
        }

        public void Cancel(ShapePlacementSession session)
        {
            LastPreviewResult = null;
            CancelCount++;
        }

        public bool TryPack(ShapeItemPayload payload, out ShapePlacementResult result)
        {
            result = null;
            if (!TryFindFirstLegalAnchor(payload, out ItemShapeCell anchorCell))
            {
                result = BuildResult(
                    payload,
                    default,
                    Array.Empty<ItemShapeCell>(),
                    false,
                    ShapePlacementInvalidReason.OutOfGrid);
                return false;
            }

            ShapePlacementSession session = new();
            session.Begin(payload, trayAnchorCell: anchorCell);
            result = session.Commit(this);
            return result.IsValid;
        }

        public IReadOnlyList<ShapePlacementResult> PackPayloads(IEnumerable<ShapeItemPayload> payloads)
        {
            List<ShapePlacementResult> results = new();
            foreach (ShapeItemPayload payload in payloads ?? Array.Empty<ShapeItemPayload>())
            {
                TryPack(payload, out ShapePlacementResult result);
                results.Add(result);
            }

            return results;
        }

        public bool TryFindFirstLegalAnchor(ShapeItemPayload payload, out ItemShapeCell anchorCell)
        {
            for (int slotIndex = 0; slotIndex < SlotCount; slotIndex++)
            {
                anchorCell = SlotIndexToCell(slotIndex);
                ShapePlacementResult result = CanPlace(payload, anchorCell);
                if (result.IsValid)
                {
                    return true;
                }
            }

            anchorCell = default;
            return false;
        }

        public bool TryGetPlacement(string itemId, out ShapeAwareItemTrayGridPlacement placement)
        {
            return placementsByItemId.TryGetValue(itemId ?? string.Empty, out placement);
        }

        public bool RemoveItem(string itemId)
        {
            bool existed = !string.IsNullOrWhiteSpace(itemId)
                && placementsByItemId.ContainsKey(itemId);
            ReleaseItem(itemId);
            return existed;
        }

        public void Clear()
        {
            occupiedByItemId.Clear();
            placementsByItemId.Clear();
            LastPreviewResult = null;
            LastCommitResult = null;
            PreviewCount = 0;
            CommitCount = 0;
            CancelCount = 0;
        }

        public bool IsWithinBounds(ItemShapeCell cell)
        {
            if (cell.x < 0 || cell.y < 0 || cell.x >= ColumnCount)
            {
                return false;
            }

            int slotIndex = CellToSlotIndex(cell);
            return slotIndex >= 0 && slotIndex < SlotCount;
        }

        public int CellToSlotIndex(ItemShapeCell cell)
        {
            return cell.y * ColumnCount + cell.x;
        }

        public ItemShapeCell SlotIndexToCell(int slotIndex)
        {
            return new ItemShapeCell(slotIndex % ColumnCount, slotIndex / ColumnCount);
        }

        public IReadOnlyList<ItemShapeCell> BuildOccupiedCells(ShapeItemPayload payload, ItemShapeCell anchorCell)
        {
            IReadOnlyList<ItemShapeCell> offsets = BuildNormalizedOffsets(payload);
            if (offsets.Count == 0)
            {
                return Array.Empty<ItemShapeCell>();
            }

            return offsets
                .Select(offset => new ItemShapeCell(anchorCell.x + offset.x, anchorCell.y + offset.y))
                .OrderBy(cell => cell.y)
                .ThenBy(cell => cell.x)
                .ToArray();
        }

        private IReadOnlyList<ItemShapeCell> BuildNormalizedOffsets(ShapeItemPayload payload)
        {
            if (!payload.IsValid)
            {
                return Array.Empty<ItemShapeCell>();
            }

            ItemShapeCell[] rotatedOffsets = payload.OccupiedOffsets
                .Select(offset => ApplyRotation(offset, payload.Rotation))
                .ToArray();
            if (rotatedOffsets.Length == 0)
            {
                return Array.Empty<ItemShapeCell>();
            }

            int minX = rotatedOffsets.Min(cell => cell.x);
            int minY = rotatedOffsets.Min(cell => cell.y);
            return rotatedOffsets
                .Select(cell => new ItemShapeCell(cell.x - minX, cell.y - minY))
                .Distinct()
                .OrderBy(cell => cell.y)
                .ThenBy(cell => cell.x)
                .ToArray();
        }

        private void ReleaseItem(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return;
            }

            foreach (ItemShapeCell cell in occupiedByItemId
                         .Where(pair => string.Equals(pair.Value, itemId, StringComparison.Ordinal))
                         .Select(pair => pair.Key)
                         .ToArray())
            {
                occupiedByItemId.Remove(cell);
            }

            placementsByItemId.Remove(itemId);
        }

        private ShapePlacementResult BuildResult(
            ShapeItemPayload payload,
            ItemShapeCell anchorCell,
            IReadOnlyList<ItemShapeCell> occupiedCells,
            bool isValid,
            ShapePlacementInvalidReason invalidReason)
        {
            string itemId = payload.ItemId ?? string.Empty;
            return new ShapePlacementResult(
                itemId,
                payload.ShapeId ?? string.Empty,
                anchorCell,
                occupiedCells ?? Array.Empty<ItemShapeCell>(),
                isValid,
                invalidReason,
                CollectAdjacentItems(occupiedCells, itemId),
                energyConnected: false,
                energyConnectionNote: "shape_aware_item_tray_grid_devonly");
        }

        private IReadOnlyList<string> CollectAdjacentItems(
            IReadOnlyList<ItemShapeCell> cells,
            string selfItemId)
        {
            HashSet<ItemShapeCell> ownCells = new(cells ?? Array.Empty<ItemShapeCell>());
            HashSet<string> adjacent = new(StringComparer.Ordinal);
            foreach (ItemShapeCell cell in ownCells)
            {
                foreach (ItemShapeCell neighbor in EnumerateCardinalNeighbors(cell))
                {
                    if (ownCells.Contains(neighbor))
                    {
                        continue;
                    }

                    if (occupiedByItemId.TryGetValue(neighbor, out string itemId)
                        && !string.IsNullOrWhiteSpace(itemId)
                        && !string.Equals(itemId, selfItemId, StringComparison.Ordinal))
                    {
                        adjacent.Add(itemId);
                    }
                }
            }

            return adjacent.OrderBy(id => id, StringComparer.Ordinal).ToArray();
        }

        private static IEnumerable<ItemShapeCell> EnumerateCardinalNeighbors(ItemShapeCell cell)
        {
            yield return new ItemShapeCell(cell.x + 1, cell.y);
            yield return new ItemShapeCell(cell.x - 1, cell.y);
            yield return new ItemShapeCell(cell.x, cell.y + 1);
            yield return new ItemShapeCell(cell.x, cell.y - 1);
        }

        private static ItemShapeCell ApplyRotation(ItemShapeCell offset, ItemShapeRotation rotation)
        {
            switch (rotation)
            {
                case ItemShapeRotation.Rotation90:
                    return new ItemShapeCell(offset.y, -offset.x);
                case ItemShapeRotation.Rotation180:
                    return new ItemShapeCell(-offset.x, -offset.y);
                case ItemShapeRotation.Rotation270:
                    return new ItemShapeCell(-offset.y, offset.x);
                default:
                    return offset;
            }
        }
    }

    public sealed class ShapeAwareItemTrayGridPlacement
    {
        private readonly ItemShapeCell[] occupiedCells;
        private readonly int[] occupiedSlotIndexes;

        public ShapeAwareItemTrayGridPlacement(
            string itemId,
            string shapeId,
            ItemShapeRotation rotation,
            ItemShapeCell anchorCell,
            IReadOnlyList<ItemShapeCell> occupiedCells,
            int columnCount)
        {
            ItemId = itemId ?? string.Empty;
            ShapeId = shapeId ?? string.Empty;
            Rotation = rotation;
            AnchorCell = anchorCell;
            this.occupiedCells = (occupiedCells ?? Array.Empty<ItemShapeCell>())
                .OrderBy(cell => cell.y)
                .ThenBy(cell => cell.x)
                .ToArray();
            occupiedSlotIndexes = this.occupiedCells
                .Select(cell => cell.y * columnCount + cell.x)
                .OrderBy(index => index)
                .ToArray();
        }

        public string ItemId { get; }
        public string ShapeId { get; }
        public ItemShapeRotation Rotation { get; }
        public ItemShapeCell AnchorCell { get; }
        public IReadOnlyList<ItemShapeCell> OccupiedCells => occupiedCells;
        public IReadOnlyList<int> OccupiedSlotIndexes => occupiedSlotIndexes;
        public int CellCount => occupiedCells.Length;
    }
}
