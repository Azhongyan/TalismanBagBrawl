using System;
using TalismanBag.UI;
using TalismanBag.V03.BattlePrepare;

namespace TalismanBag.BuildSandbox
{
    public sealed class BattlePrepareShapePlacementSeamAdapter :
        IBattlePrepareExtensionSeamProvider,
        IShapePlacementSessionProvider,
        IShapeItemPayloadProvider,
        IShapeGridReceiverProvider,
        IBattlePrepareGhostPreviewAdapter,
        IBattlePreparePlacementCommitAdapter,
        IBattlePreparePlacementCancelAdapter
    {
        private readonly bool devOnly = true;
        private readonly bool isEnabled;
        private MobileShapePlacementRuntimeIntegration runtimeIntegration;
        private ShapeAwareItemTrayGrid trayGrid;
        private ShapeGridReceiver boardReceiver;
        private Func<DraggableTalismanItemView, ShapeItemPayload> payloadResolver;
        private Action<BattlePrepareGhostPreviewContext, ShapeItemPayload> ghostPreviewHandler;
        private Action<BattlePreparePlacementCancelContext> cancelHandler;

        public BattlePrepareShapePlacementSeamAdapter()
        {
        }

        public BattlePrepareShapePlacementSeamAdapter(
            MobileShapePlacementRuntimeIntegration runtimeIntegration,
            ShapeAwareItemTrayGrid trayGrid,
            ShapeGridReceiver boardReceiver,
            Func<DraggableTalismanItemView, ShapeItemPayload> payloadResolver)
        {
            BindRuntimeIntegration(runtimeIntegration, trayGrid, boardReceiver, payloadResolver);
        }

        public bool IsBattlePrepareExtensionEnabled =>
            devOnly && !isEnabled && runtimeIntegration != null;

        public IShapePlacementSessionProvider ShapePlacementSessionProvider => this;
        public IShapeItemPayloadProvider ShapeItemPayloadProvider => this;
        public IShapeGridReceiverProvider ShapeGridReceiverProvider => this;
        public IBattlePrepareGhostPreviewAdapter GhostPreviewAdapter => this;
        public IBattlePreparePlacementCommitAdapter PlacementCommitAdapter => this;
        public IBattlePreparePlacementCancelAdapter PlacementCancelAdapter => this;
        public bool IsShapePlacementSessionEnabled => IsBattlePrepareExtensionEnabled && runtimeIntegration.Session != null;
        public object ShapePlacementSession => runtimeIntegration?.Session;

        public void BindRuntimeIntegration(
            MobileShapePlacementRuntimeIntegration runtimeIntegration,
            ShapeAwareItemTrayGrid trayGrid,
            ShapeGridReceiver boardReceiver,
            Func<DraggableTalismanItemView, ShapeItemPayload> payloadResolver)
        {
            this.runtimeIntegration = runtimeIntegration;
            this.trayGrid = trayGrid;
            this.boardReceiver = boardReceiver;
            this.payloadResolver = payloadResolver;
        }

        public void BindRuntimeCallbacks(
            Action<BattlePrepareGhostPreviewContext, ShapeItemPayload> ghostPreviewHandler,
            Action<BattlePreparePlacementCancelContext> cancelHandler)
        {
            this.ghostPreviewHandler = ghostPreviewHandler;
            this.cancelHandler = cancelHandler;
        }

        public bool TryBuildShapeItemPayload(
            BattlePrepareDragContext context,
            out BattlePrepareShapeItemPayload payload)
        {
            payload = default;
            if (!IsBattlePrepareExtensionEnabled || payloadResolver == null || context == null)
            {
                return false;
            }

            ShapeItemPayload shapePayload = payloadResolver.Invoke(context.ItemView);
            if (!shapePayload.IsValid)
            {
                return false;
            }

            payload = new BattlePrepareShapeItemPayload(
                shapePayload.ItemId,
                shapePayload.ShapeId,
                shapePayload.Source.ToString(),
                shapePayload);
            return true;
        }

        public bool TryResolveBoardReceiver(
            BattlePrepareBoardReceiverContext context,
            out object receiver)
        {
            receiver = IsBattlePrepareExtensionEnabled ? boardReceiver : null;
            return receiver != null;
        }

        public bool TryResolveItemTrayReceiver(
            BattlePrepareItemTrayReceiverContext context,
            out object receiver)
        {
            receiver = IsBattlePrepareExtensionEnabled ? trayGrid : null;
            return receiver != null;
        }

        public void PreviewGhost(BattlePrepareGhostPreviewContext context)
        {
            if (!IsBattlePrepareExtensionEnabled || context == null)
            {
                return;
            }

            if (!(context.RuntimePayload is ShapeItemPayload shapePayload) || !shapePayload.IsValid)
            {
                return;
            }

            ghostPreviewHandler?.Invoke(context, shapePayload);
        }

        public void ClearGhostPreview(BattlePreparePlacementCancelContext context)
        {
        }

        public bool TryCommitBoardPlacement(BattlePreparePlacementCommitContext context)
        {
            return false;
        }

        public bool TryCommitItemTrayPlacement(BattlePreparePlacementCommitContext context)
        {
            return false;
        }

        public void OnItemTrayPlacementCommitted(BattlePreparePlacementCommitContext context)
        {
        }

        public void OnBattlePrepareCommitRequested(BattlePreparePlacementCommitContext context)
        {
        }

        public void OnBattlePrepareCommitted(BattlePreparePlacementCommitContext context)
        {
        }

        public void CancelPlacement(BattlePreparePlacementCancelContext context)
        {
            CancelRuntimeSession();
            cancelHandler?.Invoke(context);
        }

        private void CancelRuntimeSession()
        {
            if (!IsBattlePrepareExtensionEnabled)
            {
                return;
            }

            runtimeIntegration.Input?.Cancel(boardReceiver);
        }
    }
}
