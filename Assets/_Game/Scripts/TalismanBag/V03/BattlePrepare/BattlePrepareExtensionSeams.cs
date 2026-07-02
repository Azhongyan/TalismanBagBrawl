using TalismanBag.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TalismanBag.V03.BattlePrepare
{
    public interface IBattlePrepareExtensionSeamProvider
    {
        bool IsBattlePrepareExtensionEnabled { get; }
        IShapePlacementSessionProvider ShapePlacementSessionProvider { get; }
        IShapeItemPayloadProvider ShapeItemPayloadProvider { get; }
        IShapeGridReceiverProvider ShapeGridReceiverProvider { get; }
        IBattlePrepareGhostPreviewAdapter GhostPreviewAdapter { get; }
        IBattlePreparePlacementCommitAdapter PlacementCommitAdapter { get; }
        IBattlePreparePlacementCancelAdapter PlacementCancelAdapter { get; }
    }

    public interface IShapePlacementSessionProvider
    {
        bool IsShapePlacementSessionEnabled { get; }
        object ShapePlacementSession { get; }
    }

    public interface IShapeItemPayloadProvider
    {
        bool TryBuildShapeItemPayload(
            BattlePrepareDragContext context,
            out BattlePrepareShapeItemPayload payload);
    }

    public interface IShapeGridReceiverProvider
    {
        bool TryResolveBoardReceiver(
            BattlePrepareBoardReceiverContext context,
            out object receiver);

        bool TryResolveItemTrayReceiver(
            BattlePrepareItemTrayReceiverContext context,
            out object receiver);
    }

    public interface IBattlePrepareGhostPreviewAdapter
    {
        void PreviewGhost(BattlePrepareGhostPreviewContext context);
        void ClearGhostPreview(BattlePreparePlacementCancelContext context);
    }

    public interface IBattlePreparePlacementCommitAdapter
    {
        bool TryCommitBoardPlacement(BattlePreparePlacementCommitContext context);
        bool TryCommitItemTrayPlacement(BattlePreparePlacementCommitContext context);
        void OnItemTrayPlacementCommitted(BattlePreparePlacementCommitContext context);
        void OnBattlePrepareCommitRequested(BattlePreparePlacementCommitContext context);
        void OnBattlePrepareCommitted(BattlePreparePlacementCommitContext context);
    }

    public interface IBattlePreparePlacementCancelAdapter
    {
        void CancelPlacement(BattlePreparePlacementCancelContext context);
    }

    public readonly struct BattlePrepareShapeItemPayload
    {
        public BattlePrepareShapeItemPayload(
            string itemId,
            string shapeId,
            string source,
            object runtimePayload)
        {
            ItemId = itemId ?? string.Empty;
            ShapeId = shapeId ?? string.Empty;
            Source = source ?? string.Empty;
            RuntimePayload = runtimePayload;
        }

        public string ItemId { get; }
        public string ShapeId { get; }
        public string Source { get; }
        public object RuntimePayload { get; }
        public bool IsValid =>
            !string.IsNullOrWhiteSpace(ItemId)
            && !string.IsNullOrWhiteSpace(ShapeId);
    }

    public sealed class BattlePrepareDragContext
    {
        public BattlePrepareDragContext(
            V03BattlePrepareInteractionController controller,
            DraggableTalismanItemView itemView,
            PointerEventData eventData,
            TalismanGridSlotView sourceBoardSlot,
            RectTransform sourceTraySlot,
            int sourceTraySlotIndex,
            string phase)
        {
            Controller = controller;
            ItemView = itemView;
            EventData = eventData;
            SourceBoardSlot = sourceBoardSlot;
            SourceTraySlot = sourceTraySlot;
            SourceTraySlotIndex = sourceTraySlotIndex;
            Phase = phase ?? string.Empty;
        }

        public V03BattlePrepareInteractionController Controller { get; }
        public DraggableTalismanItemView ItemView { get; }
        public PointerEventData EventData { get; }
        public TalismanGridSlotView SourceBoardSlot { get; }
        public RectTransform SourceTraySlot { get; }
        public int SourceTraySlotIndex { get; }
        public string Phase { get; }
    }

    public sealed class BattlePrepareBoardReceiverContext
    {
        public BattlePrepareBoardReceiverContext(
            V03BattlePrepareInteractionController controller,
            Vector2 screenPoint,
            Camera eventCamera,
            TalismanGridSlotView boardSlot,
            Vector2Int gridPosition)
        {
            Controller = controller;
            ScreenPoint = screenPoint;
            EventCamera = eventCamera;
            BoardSlot = boardSlot;
            GridPosition = gridPosition;
        }

        public V03BattlePrepareInteractionController Controller { get; }
        public Vector2 ScreenPoint { get; }
        public Camera EventCamera { get; }
        public TalismanGridSlotView BoardSlot { get; }
        public Vector2Int GridPosition { get; }
    }

    public sealed class BattlePrepareItemTrayReceiverContext
    {
        public BattlePrepareItemTrayReceiverContext(
            V03BattlePrepareInteractionController controller,
            Vector2 screenPoint,
            Camera eventCamera,
            RectTransform traySlot,
            int traySlotIndex)
        {
            Controller = controller;
            ScreenPoint = screenPoint;
            EventCamera = eventCamera;
            TraySlot = traySlot;
            TraySlotIndex = traySlotIndex;
        }

        public V03BattlePrepareInteractionController Controller { get; }
        public Vector2 ScreenPoint { get; }
        public Camera EventCamera { get; }
        public RectTransform TraySlot { get; }
        public int TraySlotIndex { get; }
    }

    public sealed class BattlePrepareGhostPreviewContext
    {
        public BattlePrepareGhostPreviewContext(
            V03BattlePrepareInteractionController controller,
            DraggableTalismanItemView itemView,
            Vector2 screenPoint,
            TalismanGridSlotView boardSlot,
            RectTransform traySlot,
            bool valid,
            string message,
            object runtimePayload)
        {
            Controller = controller;
            ItemView = itemView;
            ScreenPoint = screenPoint;
            BoardSlot = boardSlot;
            TraySlot = traySlot;
            Valid = valid;
            Message = message ?? string.Empty;
            RuntimePayload = runtimePayload;
        }

        public V03BattlePrepareInteractionController Controller { get; }
        public DraggableTalismanItemView ItemView { get; }
        public Vector2 ScreenPoint { get; }
        public TalismanGridSlotView BoardSlot { get; }
        public RectTransform TraySlot { get; }
        public bool Valid { get; }
        public string Message { get; }
        public object RuntimePayload { get; }
    }

    public sealed class BattlePreparePlacementCommitContext
    {
        public BattlePreparePlacementCommitContext(
            V03BattlePrepareInteractionController controller,
            DraggableTalismanItemView itemView,
            TalismanGridSlotView boardSlot,
            RectTransform traySlot,
            int traySlotIndex,
            bool isBattlePrepareContinueCommit,
            object runtimePayload)
        {
            Controller = controller;
            ItemView = itemView;
            BoardSlot = boardSlot;
            TraySlot = traySlot;
            TraySlotIndex = traySlotIndex;
            IsBattlePrepareContinueCommit = isBattlePrepareContinueCommit;
            RuntimePayload = runtimePayload;
        }

        public V03BattlePrepareInteractionController Controller { get; }
        public DraggableTalismanItemView ItemView { get; }
        public TalismanGridSlotView BoardSlot { get; }
        public RectTransform TraySlot { get; }
        public int TraySlotIndex { get; }
        public bool IsBattlePrepareContinueCommit { get; }
        public object RuntimePayload { get; }
    }

    public sealed class BattlePreparePlacementCancelContext
    {
        public BattlePreparePlacementCancelContext(
            V03BattlePrepareInteractionController controller,
            DraggableTalismanItemView itemView,
            string reason)
        {
            Controller = controller;
            ItemView = itemView;
            Reason = reason ?? string.Empty;
        }

        public V03BattlePrepareInteractionController Controller { get; }
        public DraggableTalismanItemView ItemView { get; }
        public string Reason { get; }
    }
}
