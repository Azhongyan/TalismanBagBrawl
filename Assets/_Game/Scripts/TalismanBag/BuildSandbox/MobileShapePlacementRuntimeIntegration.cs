using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.UI;
using UnityEngine;

namespace TalismanBag.BuildSandbox
{
    public sealed class MobileShapePlacementRuntimeIntegration
    {
        public const string PackageName = "V0.4-MobileShapePlacementRuntimeIntegrationFix01";

        public MobileShapePlacementRuntimeIntegration(
            ShapeAwareItemTrayGrid trayGrid,
            ShapeGridReceiver boardReceiver,
            MobileShapePlacementInputSettings settings = null)
        {
            Session = new ShapePlacementSession();
            Input = new MobileShapePlacementInputExtension(Session, settings);
            TrayGrid = trayGrid ?? new ShapeAwareItemTrayGrid();
            BoardReceiver = boardReceiver;
        }

        public ShapePlacementSession Session { get; }
        public MobileShapePlacementInputExtension Input { get; }
        public ShapeAwareItemTrayGrid TrayGrid { get; private set; }
        public ShapeGridReceiver BoardReceiver { get; private set; }
        public bool RuntimePlaytestConnected { get; private set; }
        public bool UsesSessionAuthority => Input.Session == Session;
        public bool UsesShapeAwareItemTrayGrid => TrayGrid != null;
        public bool UsesMobileInputExtension => Input != null;

        public void BindRuntimePlaytest(ShapeAwareItemTrayGrid trayGrid, ShapeGridReceiver boardReceiver)
        {
            TrayGrid = trayGrid ?? TrayGrid ?? new ShapeAwareItemTrayGrid();
            BoardReceiver = boardReceiver;
            RuntimePlaytestConnected = BoardReceiver != null;
        }

        public ShapeItemPayload BuildPayload(
            string itemId,
            ItemShapeConfig shape,
            ItemShapeRotation rotation,
            ShapePlacementSource source)
        {
            return new ShapeItemPayload(
                itemId,
                shape != null ? shape.shapeId : string.Empty,
                rotation,
                shape != null ? shape.occupiedOffsets : Array.Empty<ItemShapeCell>(),
                source);
        }

        public ShapePlacementResult PackTrayPayload(ShapeItemPayload payload)
        {
            if (TrayGrid == null)
            {
                return new ShapePlacementResult(
                    payload.ItemId,
                    payload.ShapeId,
                    default,
                    Array.Empty<ItemShapeCell>(),
                    false,
                    ShapePlacementInvalidReason.ShapeInvalid);
            }

            TrayGrid.TryPack(payload, out ShapePlacementResult result);
            return result;
        }
    }

    public sealed class RuntimeBattlePrepareBoardShapeGridReceiver : ShapeGridReceiver
    {
        private readonly Func<IReadOnlyDictionary<Vector2Int, TalismanGridSlotView>> boardSlotsProvider;
        private readonly Func<DraggableTalismanItemView> selectedViewProvider;
        private readonly Dictionary<ItemShapeCell, string> committedByCell = new();

        public RuntimeBattlePrepareBoardShapeGridReceiver(
            Func<IReadOnlyDictionary<Vector2Int, TalismanGridSlotView>> boardSlotsProvider,
            Func<DraggableTalismanItemView> selectedViewProvider,
            string receiverId = "runtime_battleprepare_board_receiver")
        {
            this.boardSlotsProvider = boardSlotsProvider;
            this.selectedViewProvider = selectedViewProvider;
            ReceiverId = string.IsNullOrWhiteSpace(receiverId)
                ? "runtime_battleprepare_board_receiver"
                : receiverId;
        }

        public string ReceiverId { get; }
        public ShapePlacementSource ReceiverSource => ShapePlacementSource.Board;
        public ShapePlacementResult LastPreviewResult { get; private set; }
        public ShapePlacementResult LastCommitResult { get; private set; }
        public int PreviewCount { get; private set; }
        public int CommitCount { get; private set; }
        public int CancelCount { get; private set; }
        public IReadOnlyDictionary<ItemShapeCell, string> DevOnlyCommittedCells => committedByCell;

        public bool ScreenPointToCell(Vector2 screenPoint, Camera eventCamera, out ItemShapeCell anchorCell)
        {
            anchorCell = default;
            foreach (KeyValuePair<Vector2Int, TalismanGridSlotView> pair in GetBoardSlots())
            {
                RectTransform rect = pair.Value != null ? pair.Value.transform as RectTransform : null;
                if (rect == null || !RectTransformUtility.RectangleContainsScreenPoint(rect, screenPoint, eventCamera))
                {
                    continue;
                }

                anchorCell = new ItemShapeCell(pair.Key.x, pair.Key.y);
                return true;
            }

            return false;
        }

        public ShapePlacementResult CanPlace(ShapeItemPayload payload, ItemShapeCell anchorCell)
        {
            if (!payload.IsValid)
            {
                return BuildResult(payload, anchorCell, Array.Empty<ItemShapeCell>(), false, ShapePlacementInvalidReason.ShapeInvalid);
            }

            IReadOnlyList<ItemShapeCell> occupiedCells = BuildNormalizedOccupiedCells(payload, anchorCell);
            if (occupiedCells.Count == 0)
            {
                return BuildResult(payload, anchorCell, occupiedCells, false, ShapePlacementInvalidReason.ShapeInvalid);
            }

            if (occupiedCells.Any(cell => !GetBoardSlots().ContainsKey(new Vector2Int(cell.x, cell.y))))
            {
                return BuildResult(payload, anchorCell, occupiedCells, false, ShapePlacementInvalidReason.OutOfGrid);
            }

            DraggableTalismanItemView selected = selectedViewProvider?.Invoke();
            foreach (ItemShapeCell cell in occupiedCells)
            {
                if (committedByCell.TryGetValue(cell, out string committedItemId)
                    && !string.Equals(committedItemId, payload.ItemId, StringComparison.Ordinal))
                {
                    return BuildResult(payload, anchorCell, occupiedCells, false, ShapePlacementInvalidReason.CellOccupied);
                }

                if (GetBoardSlots().TryGetValue(new Vector2Int(cell.x, cell.y), out TalismanGridSlotView slot)
                    && slot != null
                    && slot.CurrentItemView != null
                    && slot.CurrentItemView != selected)
                {
                    return BuildResult(payload, anchorCell, occupiedCells, false, ShapePlacementInvalidReason.CellOccupied);
                }
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
                LastCommitResult = BuildResult(default, default, Array.Empty<ItemShapeCell>(), false, ShapePlacementInvalidReason.ShapeInvalid);
                CommitCount++;
                return LastCommitResult;
            }

            ItemShapeCell anchor = session.BoardAnchorCell
                ?? session.LastLegalBoardAnchor
                ?? session.PreviewResult?.AnchorCell
                ?? default;
            ShapePlacementResult result = CanPlace(session.CurrentPayload, anchor);
            if (result.IsValid)
            {
                ReleaseItem(session.CurrentPayload.ItemId);
                foreach (ItemShapeCell cell in result.OccupiedCells)
                {
                    committedByCell[cell] = session.CurrentPayload.ItemId;
                }
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

        public void ClearDevOnlyCommits()
        {
            committedByCell.Clear();
            LastPreviewResult = null;
            LastCommitResult = null;
            PreviewCount = 0;
            CommitCount = 0;
            CancelCount = 0;
        }

        private IReadOnlyDictionary<Vector2Int, TalismanGridSlotView> GetBoardSlots()
        {
            return boardSlotsProvider?.Invoke() ?? new Dictionary<Vector2Int, TalismanGridSlotView>();
        }

        private void ReleaseItem(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return;
            }

            foreach (ItemShapeCell cell in committedByCell
                         .Where(pair => string.Equals(pair.Value, itemId, StringComparison.Ordinal))
                         .Select(pair => pair.Key)
                         .ToArray())
            {
                committedByCell.Remove(cell);
            }
        }

        private ShapePlacementResult BuildResult(
            ShapeItemPayload payload,
            ItemShapeCell anchorCell,
            IReadOnlyList<ItemShapeCell> occupiedCells,
            bool isValid,
            ShapePlacementInvalidReason invalidReason)
        {
            return new ShapePlacementResult(
                payload.ItemId,
                payload.ShapeId,
                anchorCell,
                occupiedCells ?? Array.Empty<ItemShapeCell>(),
                isValid,
                invalidReason,
                energyConnected: false,
                energyConnectionNote: "mobile_shape_placement_runtime_integration_devonly");
        }

        private static IReadOnlyList<ItemShapeCell> BuildNormalizedOccupiedCells(
            ShapeItemPayload payload,
            ItemShapeCell anchorCell)
        {
            ItemShapeCell[] rotatedOffsets = (payload.OccupiedOffsets ?? Array.Empty<ItemShapeCell>())
                .Select(offset => ApplyRotation(offset, payload.Rotation))
                .ToArray();
            if (rotatedOffsets.Length == 0)
            {
                return Array.Empty<ItemShapeCell>();
            }

            int minX = rotatedOffsets.Min(cell => cell.x);
            int minY = rotatedOffsets.Min(cell => cell.y);
            return rotatedOffsets
                .Select(cell => new ItemShapeCell(
                    anchorCell.x + cell.x - minX,
                    anchorCell.y + cell.y - minY))
                .Distinct()
                .OrderBy(cell => cell.y)
                .ThenBy(cell => cell.x)
                .ToArray();
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
}
