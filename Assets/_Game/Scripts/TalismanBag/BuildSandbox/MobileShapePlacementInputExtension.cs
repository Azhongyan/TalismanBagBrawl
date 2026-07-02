using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TalismanBag.BuildSandbox
{
    public enum MobileShapePlacementInputState
    {
        Idle = 0,
        HoldingItem = 1,
        RotateHoldingItem = 2,
        DraggingPreview = 3,
        PreviewLocked = 4,
        InvalidPreview = 5,
        Placed = 6,
        Cancelled = 7
    }

    public enum MobileShapeGestureKind
    {
        Tap = 0,
        Drag = 1
    }

    public enum GhostPlacementOutlineStyle
    {
        Hidden = 0,
        Valid = 1,
        Invalid = 2,
        Locked = 3,
        Confirmed = 4
    }

    public sealed class MobileShapePlacementInputSettings
    {
        public const float DefaultTapMoveThresholdPixels = 15f;
        public const float DefaultTapTimeThresholdSeconds = 0.25f;
        public const float DefaultFingerGhostOffsetPixels = 50f;

        public float tapMoveThresholdPixels = DefaultTapMoveThresholdPixels;
        public float tapTimeThresholdSeconds = DefaultTapTimeThresholdSeconds;
        public float fingerGhostOffsetPixels = DefaultFingerGhostOffsetPixels;
    }

    public sealed class GhostPlacementPreviewSnapshot
    {
        public string itemId = string.Empty;
        public string shapeId = string.Empty;
        public MobileShapePlacementInputState inputState;
        public bool visible;
        public bool valid;
        public bool locked;
        public GhostPlacementOutlineStyle outlineStyle;
        public string chineseHint = string.Empty;
        public Vector2 ghostScreenPosition;
        public IReadOnlyList<ItemShapeCell> occupiedCells = Array.Empty<ItemShapeCell>();
        public ShapePlacementInvalidReason invalidReason;
    }

    public sealed class MobileShapePlacementInputExtension
    {
        private readonly ShapePlacementSession session;
        private readonly MobileShapePlacementInputSettings settings;

        public MobileShapePlacementInputExtension(
            ShapePlacementSession session = null,
            MobileShapePlacementInputSettings settings = null)
        {
            this.session = session ?? new ShapePlacementSession();
            this.settings = settings ?? new MobileShapePlacementInputSettings();
            CurrentState = MobileShapePlacementInputState.Idle;
            LastGhostPreview = GhostPlacementPreviewExtension.BuildSnapshot(
                this.session,
                CurrentState,
                this.settings);
        }

        public ShapePlacementSession Session => session;
        public MobileShapePlacementInputSettings Settings => settings;
        public MobileShapePlacementInputState CurrentState { get; private set; }
        public GhostPlacementPreviewSnapshot LastGhostPreview { get; private set; }
        public ShapePlacementResult LastCommitResult { get; private set; }
        public string LastHint { get; private set; } = string.Empty;

        public MobileShapeGestureKind ClassifyGesture(
            Vector2 pressPosition,
            Vector2 releasePosition,
            float pressDurationSeconds)
        {
            float moveDistance = Vector2.Distance(pressPosition, releasePosition);
            return moveDistance < settings.tapMoveThresholdPixels
                && pressDurationSeconds < settings.tapTimeThresholdSeconds
                    ? MobileShapeGestureKind.Tap
                    : MobileShapeGestureKind.Drag;
        }

        public bool TapTrayItem(ShapeItemPayload payload, ItemShapeCell? trayAnchorCell = null)
        {
            if (!payload.IsValid)
            {
                LastHint = "道具形态数据缺失";
                CurrentState = MobileShapePlacementInputState.Idle;
                UpdateGhostSnapshot();
                return false;
            }

            session.Begin(payload, trayAnchorCell: trayAnchorCell);
            CurrentState = MobileShapePlacementInputState.HoldingItem;
            LastCommitResult = null;
            LastHint = "已拿起";
            UpdateGhostSnapshot();
            return true;
        }

        public bool TapSelectedItemToRotate(
            ShapeGridReceiver activeReceiver = null,
            ItemShapeCell? anchorCell = null)
        {
            if (!session.HasActivePayload)
            {
                LastHint = "没有选中的道具";
                return false;
            }

            if (!ItemRotationInputExtension.CanRotate(session.CurrentPayload))
            {
                LastHint = "该形态无需旋转";
                UpdateGhostSnapshot();
                return false;
            }

            bool rotated = session.RotateClockwise();
            CurrentState = rotated
                ? MobileShapePlacementInputState.RotateHoldingItem
                : CurrentState;
            LastHint = rotated ? "已旋转" : "该形态无需旋转";

            if (rotated && activeReceiver != null && anchorCell.HasValue)
            {
                ShapePlacementResult result = session.Preview(activeReceiver, anchorCell.Value);
                if (result != null && result.IsValid)
                {
                    session.LockPreview();
                    CurrentState = MobileShapePlacementInputState.PreviewLocked;
                }
                else
                {
                    CurrentState = MobileShapePlacementInputState.InvalidPreview;
                }
            }

            UpdateGhostSnapshot();
            return rotated;
        }

        public ShapePlacementResult DragToReceiver(
            ShapeGridReceiver receiver,
            Vector2 screenPoint,
            Camera eventCamera = null)
        {
            session.UpdatePointer(screenPoint);
            if (receiver == null || !session.HasActivePayload)
            {
                LastHint = "当前位置无法放置";
                CurrentState = MobileShapePlacementInputState.InvalidPreview;
                UpdateGhostSnapshot();
                return null;
            }

            if (!receiver.ScreenPointToCell(screenPoint, eventCamera, out ItemShapeCell anchorCell))
            {
                ShapePlacementResult invalid = BuildInvalidResult(
                    default,
                    ShapePlacementInvalidReason.OutOfGrid);
                CurrentState = MobileShapePlacementInputState.InvalidPreview;
                LastHint = "当前位置无法放置";
                LastGhostPreview = GhostPlacementPreviewExtension.BuildSnapshot(
                    session,
                    CurrentState,
                    settings,
                    invalid);
                return invalid;
            }

            ShapePlacementResult result = session.Preview(receiver, anchorCell);
            CurrentState = result != null && result.IsValid
                ? MobileShapePlacementInputState.DraggingPreview
                : MobileShapePlacementInputState.InvalidPreview;
            LastHint = result != null && result.IsValid
                ? "再次点击放下"
                : "当前位置无法放置";
            LastGhostPreview = GhostPlacementPreviewExtension.BuildSnapshot(
                session,
                CurrentState,
                settings,
                result);
            return result;
        }

        public bool ReleaseDragLockPreview()
        {
            bool locked = session.LockPreview();
            CurrentState = locked
                ? MobileShapePlacementInputState.PreviewLocked
                : MobileShapePlacementInputState.InvalidPreview;
            LastHint = locked ? "再次点击放下" : "当前位置无法放置";
            UpdateGhostSnapshot();
            return locked;
        }

        public ShapePlacementResult DragLockedGhostToReceiver(
            ShapeGridReceiver receiver,
            Vector2 screenPoint,
            Camera eventCamera = null)
        {
            ShapePlacementResult result = DragToReceiver(receiver, screenPoint, eventCamera);
            if (result != null && result.IsValid && session.LockPreview())
            {
                CurrentState = MobileShapePlacementInputState.PreviewLocked;
                LastHint = "再次点击放下";
                UpdateGhostSnapshot(result);
            }

            return result;
        }

        public ShapePlacementResult TapGhostToConfirm(ShapeGridReceiver receiver)
        {
            if (CurrentState != MobileShapePlacementInputState.PreviewLocked || !session.IsPreviewLocked)
            {
                LastHint = "请先松手锁定虚影";
                UpdateGhostSnapshot();
                return BuildInvalidResult(
                    session.PreviewResult?.AnchorCell ?? default,
                    ShapePlacementInvalidReason.CommitDisabled);
            }

            ShapePlacementResult result = session.Commit(receiver);
            LastCommitResult = result;
            CurrentState = result != null && result.IsValid
                ? MobileShapePlacementInputState.Placed
                : MobileShapePlacementInputState.InvalidPreview;
            LastHint = result != null && result.IsValid
                ? "已放下"
                : "当前位置无法放置";
            UpdateGhostSnapshot(result);
            return result;
        }

        public void Cancel(ShapeGridReceiver receiver = null)
        {
            session.Cancel(receiver);
            CurrentState = MobileShapePlacementInputState.Cancelled;
            LastHint = "已取消";
            UpdateGhostSnapshot();
        }

        private ShapePlacementResult BuildInvalidResult(
            ItemShapeCell anchorCell,
            ShapePlacementInvalidReason reason)
        {
            ShapeItemPayload payload = session.CurrentPayload;
            return new ShapePlacementResult(
                payload.ItemId,
                payload.ShapeId,
                anchorCell,
                Array.Empty<ItemShapeCell>(),
                isValid: false,
                invalidReason: reason);
        }

        private void UpdateGhostSnapshot(ShapePlacementResult result = null)
        {
            LastGhostPreview = GhostPlacementPreviewExtension.BuildSnapshot(
                session,
                CurrentState,
                settings,
                result);
        }
    }

    public static class GhostPlacementPreviewExtension
    {
        public static GhostPlacementPreviewSnapshot BuildSnapshot(
            ShapePlacementSession session,
            MobileShapePlacementInputState inputState,
            MobileShapePlacementInputSettings settings,
            ShapePlacementResult overrideResult = null)
        {
            ShapePlacementResult result = overrideResult ?? session?.PreviewResult;
            bool hasPayload = session != null && session.HasActivePayload;
            bool visible = hasPayload
                && (inputState == MobileShapePlacementInputState.DraggingPreview
                    || inputState == MobileShapePlacementInputState.PreviewLocked
                    || inputState == MobileShapePlacementInputState.InvalidPreview);
            bool valid = result != null && result.IsValid;
            bool locked = inputState == MobileShapePlacementInputState.PreviewLocked;

            return new GhostPlacementPreviewSnapshot
            {
                itemId = session?.SelectedItemId ?? string.Empty,
                shapeId = session?.ShapeId ?? string.Empty,
                inputState = inputState,
                visible = visible,
                valid = valid,
                locked = locked,
                outlineStyle = ResolveOutlineStyle(inputState, visible, valid, locked),
                chineseHint = ResolveHint(inputState, valid, result?.InvalidReason ?? ShapePlacementInvalidReason.None),
                ghostScreenPosition = (session?.LastPointerScreenPosition ?? Vector2.zero)
                    + new Vector2(0f, settings?.fingerGhostOffsetPixels ?? MobileShapePlacementInputSettings.DefaultFingerGhostOffsetPixels),
                occupiedCells = (result?.OccupiedCells ?? session?.OccupiedCells ?? Array.Empty<ItemShapeCell>()).ToArray(),
                invalidReason = result?.InvalidReason ?? ShapePlacementInvalidReason.None
            };
        }

        private static GhostPlacementOutlineStyle ResolveOutlineStyle(
            MobileShapePlacementInputState inputState,
            bool visible,
            bool valid,
            bool locked)
        {
            if (!visible)
            {
                return inputState == MobileShapePlacementInputState.Placed
                    ? GhostPlacementOutlineStyle.Confirmed
                    : GhostPlacementOutlineStyle.Hidden;
            }

            if (!valid || inputState == MobileShapePlacementInputState.InvalidPreview)
            {
                return GhostPlacementOutlineStyle.Invalid;
            }

            return locked
                ? GhostPlacementOutlineStyle.Locked
                : GhostPlacementOutlineStyle.Valid;
        }

        private static string ResolveHint(
            MobileShapePlacementInputState inputState,
            bool valid,
            ShapePlacementInvalidReason invalidReason)
        {
            if (inputState == MobileShapePlacementInputState.Placed)
            {
                return "已放下";
            }

            if (inputState == MobileShapePlacementInputState.Cancelled)
            {
                return "已取消";
            }

            if (!valid && invalidReason != ShapePlacementInvalidReason.None)
            {
                return "当前位置无法放置";
            }

            if (inputState == MobileShapePlacementInputState.PreviewLocked)
            {
                return "再次点击放下";
            }

            return valid ? "再次点击放下" : string.Empty;
        }
    }

    public static class ItemRotationInputExtension
    {
        public static bool CanRotate(ShapeItemPayload payload)
        {
            return CountUniqueRotations(payload) > 1;
        }

        public static int CountUniqueRotations(ShapeItemPayload payload)
        {
            if (!payload.IsValid)
            {
                return 0;
            }

            HashSet<string> keys = new(StringComparer.Ordinal);
            foreach (ItemShapeRotation rotation in Enum.GetValues(typeof(ItemShapeRotation)))
            {
                ShapeItemPayload rotated = payload.WithRotation(rotation);
                keys.Add(BuildNormalizedOffsetKey(rotated));
            }

            return keys.Count;
        }

        private static string BuildNormalizedOffsetKey(ShapeItemPayload payload)
        {
            ItemShapeCell[] rotatedOffsets = payload.OccupiedOffsets
                .Select(offset => ApplyRotation(offset, payload.Rotation))
                .ToArray();
            if (rotatedOffsets.Length == 0)
            {
                return string.Empty;
            }

            int minX = rotatedOffsets.Min(cell => cell.x);
            int minY = rotatedOffsets.Min(cell => cell.y);
            return string.Join(
                ";",
                rotatedOffsets
                    .Select(cell => new ItemShapeCell(cell.x - minX, cell.y - minY))
                    .Distinct()
                    .OrderBy(cell => cell.y)
                    .ThenBy(cell => cell.x)
                    .Select(cell => cell.ToString()));
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

    public static class PlacementConfirmExtension
    {
        public static ShapePlacementResult ConfirmLockedPreview(
            MobileShapePlacementInputExtension input,
            ShapeGridReceiver receiver)
        {
            return input?.TapGhostToConfirm(receiver);
        }
    }

    public static class PlacedItemEditExtension
    {
        public const bool Implemented = false;
        public const string Status = "DEFERRED";
    }
}
