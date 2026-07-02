using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TalismanBag.BuildSandbox
{
    public enum ShapePlacementSource
    {
        Unknown = 0,
        Tray = 1,
        Board = 2
    }

    public enum ShapePlacementState
    {
        Idle = 0,
        HoldingItem = 1,
        Previewing = 2,
        PreviewLocked = 3,
        InvalidPreview = 4,
        Committed = 5,
        Cancelled = 6
    }

    public readonly struct ShapeItemPayload
    {
        private readonly ItemShapeCell[] occupiedOffsets;

        public ShapeItemPayload(
            string itemId,
            string shapeId,
            ItemShapeRotation rotation,
            IReadOnlyList<ItemShapeCell> occupiedOffsets,
            ShapePlacementSource source)
        {
            ItemId = itemId ?? string.Empty;
            ShapeId = shapeId ?? string.Empty;
            Rotation = rotation;
            this.occupiedOffsets = (occupiedOffsets ?? Array.Empty<ItemShapeCell>()).ToArray();
            Source = source;
        }

        public string ItemId { get; }
        public string ShapeId { get; }
        public ItemShapeRotation Rotation { get; }
        public IReadOnlyList<ItemShapeCell> OccupiedOffsets => occupiedOffsets ?? Array.Empty<ItemShapeCell>();
        public ShapePlacementSource Source { get; }
        public bool IsValid =>
            !string.IsNullOrWhiteSpace(ItemId)
            && !string.IsNullOrWhiteSpace(ShapeId)
            && OccupiedOffsets.Count > 0;

        public ShapeItemPayload WithRotation(ItemShapeRotation rotation)
        {
            return new ShapeItemPayload(ItemId, ShapeId, rotation, OccupiedOffsets, Source);
        }

        public IReadOnlyList<ItemShapeCell> BuildOccupiedCells(ItemShapeCell anchorCell)
        {
            var offsets = occupiedOffsets ?? Array.Empty<ItemShapeCell>();
            var rotation = Rotation;
            var cells = new ItemShapeCell[offsets.Length];
            for (var i = 0; i < offsets.Length; i++)
            {
                var rotatedOffset = ApplyRotation(offsets[i], rotation);
                cells[i] = new ItemShapeCell(anchorCell.x + rotatedOffset.x, anchorCell.y + rotatedOffset.y);
            }

            return cells;
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

    public interface ShapeGridReceiver
    {
        string ReceiverId { get; }
        ShapePlacementSource ReceiverSource { get; }
        bool ScreenPointToCell(Vector2 screenPoint, Camera eventCamera, out ItemShapeCell anchorCell);
        ShapePlacementResult CanPlace(ShapeItemPayload payload, ItemShapeCell anchorCell);
        void Preview(ShapePlacementSession session, ShapePlacementResult result);
        ShapePlacementResult Commit(ShapePlacementSession session);
        void Cancel(ShapePlacementSession session);
    }

    public sealed class ShapePlacementSession
    {
        private static readonly ItemShapeCell[] EmptyCells = Array.Empty<ItemShapeCell>();

        private ShapeItemPayload currentPayload;
        private ItemShapeCell[] occupiedCells = EmptyCells;

        public string SelectedItemId { get; private set; } = string.Empty;
        public string ShapeId { get; private set; } = string.Empty;
        public ItemShapeRotation Rotation { get; private set; } = ItemShapeRotation.Rotation0;
        public ShapePlacementSource SourceContainer { get; private set; } = ShapePlacementSource.Unknown;
        public ShapePlacementState CurrentState { get; private set; } = ShapePlacementState.Idle;
        public ItemShapeCell? TrayAnchorCell { get; private set; }
        public ItemShapeCell? BoardAnchorCell { get; private set; }
        public IReadOnlyList<ItemShapeCell> OccupiedCells => occupiedCells;
        public ItemShapeCell? LastLegalTrayAnchor { get; private set; }
        public ItemShapeCell? LastLegalBoardAnchor { get; private set; }
        public string ActiveReceiverId { get; private set; } = string.Empty;
        public ShapePlacementResult PreviewResult { get; private set; }
        public bool IsPreviewLocked { get; private set; }
        public Vector2 LastPointerScreenPosition { get; private set; }
        public bool HasActivePayload => currentPayload.IsValid;
        public ShapeItemPayload CurrentPayload => currentPayload;

        public void Begin(
            ShapeItemPayload payload,
            ItemShapeCell? trayAnchorCell = null,
            ItemShapeCell? boardAnchorCell = null)
        {
            currentPayload = payload;
            SelectedItemId = payload.ItemId;
            ShapeId = payload.ShapeId;
            Rotation = payload.Rotation;
            SourceContainer = payload.Source;
            CurrentState = payload.IsValid ? ShapePlacementState.HoldingItem : ShapePlacementState.Idle;
            TrayAnchorCell = trayAnchorCell;
            BoardAnchorCell = boardAnchorCell;
            occupiedCells = EmptyCells;
            LastLegalTrayAnchor = null;
            LastLegalBoardAnchor = null;
            ActiveReceiverId = string.Empty;
            PreviewResult = null;
            IsPreviewLocked = false;
            LastPointerScreenPosition = Vector2.zero;
        }

        public void UpdatePointer(Vector2 screenPoint)
        {
            LastPointerScreenPosition = screenPoint;
        }

        public ShapePlacementResult CanPlace(ShapeGridReceiver receiver, ItemShapeCell anchorCell)
        {
            if (receiver == null || !currentPayload.IsValid)
            {
                return BuildInvalid(anchorCell, ShapePlacementInvalidReason.ShapeInvalid);
            }

            return receiver.CanPlace(currentPayload, anchorCell);
        }

        public ShapePlacementResult Preview(ShapeGridReceiver receiver, ItemShapeCell anchorCell)
        {
            ShapePlacementResult result = CanPlace(receiver, anchorCell);
            ApplyReceiverAnchor(receiver, anchorCell);
            ActiveReceiverId = receiver?.ReceiverId ?? string.Empty;
            PreviewResult = result;
            occupiedCells = (result?.OccupiedCells ?? EmptyCells).ToArray();
            CurrentState = result != null && result.IsValid
                ? ShapePlacementState.Previewing
                : ShapePlacementState.InvalidPreview;

            if (result != null && result.IsValid)
            {
                RecordLastLegalAnchor(receiver, anchorCell);
            }

            receiver?.Preview(this, result);
            return result;
        }

        public bool LockPreview()
        {
            if (PreviewResult == null || !PreviewResult.IsValid)
            {
                return false;
            }

            IsPreviewLocked = true;
            CurrentState = ShapePlacementState.PreviewLocked;
            return true;
        }

        public ShapePlacementResult Commit(ShapeGridReceiver receiver)
        {
            if (receiver == null || !currentPayload.IsValid)
            {
                ShapePlacementResult invalid = BuildInvalid(
                    PreviewResult?.AnchorCell ?? new ItemShapeCell(),
                    ShapePlacementInvalidReason.ShapeInvalid);
                PreviewResult = invalid;
                CurrentState = ShapePlacementState.InvalidPreview;
                return invalid;
            }

            ShapePlacementResult result = receiver.Commit(this);
            PreviewResult = result;
            ActiveReceiverId = receiver.ReceiverId;
            occupiedCells = (result?.OccupiedCells ?? EmptyCells).ToArray();
            CurrentState = result != null && result.IsValid
                ? ShapePlacementState.Committed
                : ShapePlacementState.InvalidPreview;

            if (result != null && result.IsValid)
            {
                RecordLastLegalAnchor(receiver, result.AnchorCell);
            }

            return result;
        }

        public void Cancel(ShapeGridReceiver receiver = null)
        {
            receiver?.Cancel(this);
            CurrentState = ShapePlacementState.Cancelled;
            ActiveReceiverId = string.Empty;
            PreviewResult = null;
            IsPreviewLocked = false;
            occupiedCells = EmptyCells;
        }

        public bool RotateClockwise()
        {
            if (!currentPayload.IsValid)
            {
                return false;
            }

            ItemShapeRotation next = Rotation switch
            {
                ItemShapeRotation.Rotation0 => ItemShapeRotation.Rotation90,
                ItemShapeRotation.Rotation90 => ItemShapeRotation.Rotation180,
                ItemShapeRotation.Rotation180 => ItemShapeRotation.Rotation270,
                _ => ItemShapeRotation.Rotation0
            };

            currentPayload = currentPayload.WithRotation(next);
            Rotation = next;
            CurrentState = ShapePlacementState.HoldingItem;
            IsPreviewLocked = false;
            PreviewResult = null;
            occupiedCells = EmptyCells;
            return true;
        }

        private void ApplyReceiverAnchor(ShapeGridReceiver receiver, ItemShapeCell anchorCell)
        {
            if (receiver == null)
            {
                return;
            }

            if (receiver.ReceiverSource == ShapePlacementSource.Tray)
            {
                TrayAnchorCell = anchorCell;
            }
            else if (receiver.ReceiverSource == ShapePlacementSource.Board)
            {
                BoardAnchorCell = anchorCell;
            }
        }

        private void RecordLastLegalAnchor(ShapeGridReceiver receiver, ItemShapeCell anchorCell)
        {
            if (receiver == null)
            {
                return;
            }

            if (receiver.ReceiverSource == ShapePlacementSource.Tray)
            {
                LastLegalTrayAnchor = anchorCell;
            }
            else if (receiver.ReceiverSource == ShapePlacementSource.Board)
            {
                LastLegalBoardAnchor = anchorCell;
            }
        }

        private ShapePlacementResult BuildInvalid(
            ItemShapeCell anchorCell,
            ShapePlacementInvalidReason reason)
        {
            return new ShapePlacementResult(
                currentPayload.ItemId,
                currentPayload.ShapeId,
                anchorCell,
                EmptyCells,
                isValid: false,
                reason);
        }
    }

    public sealed class InMemoryShapeGridReceiver : ShapeGridReceiver
    {
        private readonly Dictionary<ItemShapeCell, string> occupiedByItemId = new();
        private readonly Vector2 screenOrigin;
        private readonly Vector2 cellSize;

        public InMemoryShapeGridReceiver(
            string receiverId,
            ShapePlacementSource receiverSource,
            int width,
            int height,
            bool commitAllowed,
            Vector2? screenOrigin = null,
            Vector2? cellSize = null)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Receiver width must be positive.");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), "Receiver height must be positive.");
            }

            ReceiverId = string.IsNullOrWhiteSpace(receiverId) ? "shape_grid_receiver" : receiverId;
            ReceiverSource = receiverSource;
            Width = width;
            Height = height;
            CommitAllowed = commitAllowed;
            this.screenOrigin = screenOrigin ?? Vector2.zero;
            this.cellSize = cellSize ?? Vector2.one;
        }

        public string ReceiverId { get; }
        public ShapePlacementSource ReceiverSource { get; }
        public int Width { get; }
        public int Height { get; }
        public bool CommitAllowed { get; }
        public IReadOnlyDictionary<ItemShapeCell, string> OccupiedCells => occupiedByItemId;
        public ShapePlacementResult LastPreviewResult { get; private set; }
        public ShapePlacementResult LastCommitResult { get; private set; }
        public int PreviewCount { get; private set; }
        public int CommitCount { get; private set; }
        public int CancelCount { get; private set; }

        public bool ScreenPointToCell(Vector2 screenPoint, Camera eventCamera, out ItemShapeCell anchorCell)
        {
            anchorCell = new ItemShapeCell();
            if (cellSize.x <= 0f || cellSize.y <= 0f)
            {
                return false;
            }

            int x = Mathf.FloorToInt((screenPoint.x - screenOrigin.x) / cellSize.x);
            int y = Mathf.FloorToInt((screenPoint.y - screenOrigin.y) / cellSize.y);
            ItemShapeCell candidate = new(x, y);
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

            IReadOnlyList<ItemShapeCell> occupiedCells = payload.BuildOccupiedCells(anchorCell);
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
                    new ItemShapeCell(),
                    Array.Empty<ItemShapeCell>(),
                    false,
                    ShapePlacementInvalidReason.ShapeInvalid);
                return LastCommitResult;
            }

            ItemShapeCell anchor = ReceiverSource == ShapePlacementSource.Tray
                ? session.TrayAnchorCell ?? new ItemShapeCell()
                : session.BoardAnchorCell ?? new ItemShapeCell();
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
            }

            CommitCount++;
            LastCommitResult = result;
            return result;
        }

        public void Cancel(ShapePlacementSession session)
        {
            CancelCount++;
            LastPreviewResult = null;
        }

        public void SeedOccupiedCell(ItemShapeCell cell, string itemId)
        {
            if (!IsWithinBounds(cell) || string.IsNullOrWhiteSpace(itemId))
            {
                return;
            }

            occupiedByItemId[cell] = itemId;
        }

        public bool IsOccupied(ItemShapeCell cell)
        {
            return occupiedByItemId.ContainsKey(cell);
        }

        private bool IsWithinBounds(ItemShapeCell cell)
        {
            return cell.x >= 0 && cell.y >= 0 && cell.x < Width && cell.y < Height;
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
                CollectAdjacentItems(occupiedCells, payload.ItemId),
                energyConnected: false,
                energyConnectionNote: "shape_placement_session_devonly");
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
    }
}
