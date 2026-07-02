using System;
using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.BuildSandbox
{
    [Serializable]
    public sealed class BattlePrepareComponentAdapterPlaytestController
    {
        public const string PackageName = "V0.4-BattlePrepareComponentAdapterPlaytest01";
        public const string ValidationMode = "StaticDevOnlyAdapterHandFeelProbe";

        public string packageName = PackageName;
        public bool devOnly = true;
        public bool isEnabled;
        public bool realRuntimeInteractionExecuted;
        public bool staticPlaytestOnly = true;
        public bool writesFormalScene;
        public bool writesFormalUi;
        public bool touchesFormalPrepareController;
        public bool overwritesHandTunedRectTransform;
        public bool touchesRunFlow;
        public bool touchesPageState;
        public bool touchesFormationState;
        public bool touchesSaveData;
        public bool touchesBossRewardDropNumeric;
        public bool redrewV04Board;
        public bool redrewV04ItemTray;
        public bool rewroteDragFeel;
        public bool rewrotePullUpAnimation;
        public bool promotesTemporaryPreviewUi;
        public bool featureFlagDefaultEnabled;
        public string validationMode = ValidationMode;
        public string sourceAdapterPackageName = BattlePrepareComponentAdapter.PackageName;
        public string sourcePreviewBuildId = string.Empty;
        public string baseComponentName = BattlePageViewFormalReferenceKeys.PrepareController;
        public string handFeelConclusion =
            "Adapter data reaches mature BattlePrepare extension targets; this package is static/report-only and does not execute a live drag session.";
        public List<BattlePrepareComponentAdapterPlaytestProbeRow> probes = new();
        public List<BattlePrepareComponentAdapterPlaytestManualCheckRow> manualChecks = new();

        public int ProbeCount => probes?.Count ?? 0;
        public int PassedProbeCount => probes?.Count(row => row != null && row.passed) ?? 0;
        public int ManualCheckCount => manualChecks?.Count ?? 0;
        public bool AllProbesPassed => ProbeCount > 0 && PassedProbeCount == ProbeCount;

        public static BattlePrepareComponentAdapterPlaytestController Build(
            BattlePrepareComponentAdapter adapter,
            BattlePageViewSpec spec = null)
        {
            BattlePrepareComponentAdapter safeAdapter =
                adapter ?? BattlePrepareComponentAdapter.Build(new BuildSandboxPreviewContext(), BattlePageViewAdapter.CreateDefault());
            BattlePageViewSpec safeSpec = spec ?? BattlePageViewSpec.CreateDefault();
            BattlePrepareComponentAdapterViewModel viewModel =
                safeAdapter.viewModel ?? new BattlePrepareComponentAdapterViewModel();

            BattlePrepareComponentAdapterPlaytestController controller = new()
            {
                packageName = PackageName,
                devOnly = true,
                isEnabled = false,
                realRuntimeInteractionExecuted = false,
                staticPlaytestOnly = true,
                writesFormalScene = false,
                writesFormalUi = false,
                touchesFormalPrepareController = false,
                overwritesHandTunedRectTransform = false,
                touchesRunFlow = false,
                touchesPageState = false,
                touchesFormationState = false,
                touchesSaveData = false,
                touchesBossRewardDropNumeric = false,
                redrewV04Board = false,
                redrewV04ItemTray = false,
                rewroteDragFeel = false,
                rewrotePullUpAnimation = false,
                promotesTemporaryPreviewUi = false,
                featureFlagDefaultEnabled = BuildSandboxFeatureFlags.All.Any(flag => flag.DefaultValue),
                validationMode = ValidationMode,
                sourceAdapterPackageName = safeAdapter.packageName ?? string.Empty,
                sourcePreviewBuildId = safeAdapter.sourcePreviewBuildId ?? string.Empty,
                baseComponentName = safeAdapter.baseComponentName ?? BattlePageViewFormalReferenceKeys.PrepareController
            };

            controller.probes = BuildProbeRows(viewModel, safeSpec);
            controller.manualChecks = BuildManualChecks();
            controller.handFeelConclusion = ResolveConclusion(controller);
            return controller;
        }

        private static List<BattlePrepareComponentAdapterPlaytestProbeRow> BuildProbeRows(
            BattlePrepareComponentAdapterViewModel viewModel,
            BattlePageViewSpec spec)
        {
            List<BattlePrepareComponentAdapterPlaytestProbeRow> rows = new();

            BoardOccupancyExtensionViewModel board = viewModel.boardOccupancy ?? new BoardOccupancyExtensionViewModel();
            rows.Add(Probe(
                "boardOccupancy",
                "棋盘占格预览",
                "V02FormationGridFrame",
                board.extensionId,
                board.adapterOutputKey,
                "V0.4 occupied cells map to the mature board reference without moving the board.",
                board.occupiedCellCount > 0
                    && string.Equals(board.targetNodeName, "V02FormationGridFrame", StringComparison.Ordinal)
                    && !board.canWriteFormalUi,
                $"occupiedCells={board.occupiedCellCount}, writesFormalUi={board.canWriteFormalUi}"));

            ItemTrayShapeExtensionViewModel tray = viewModel.itemTrayShape ?? new ItemTrayShapeExtensionViewModel();
            rows.Add(Probe(
                "itemTrayShape",
                "道具栏形状信息",
                BattlePageViewFormalReferenceKeys.PrepareItemTrayRoot,
                tray.extensionId,
                tray.adapterOutputKey,
                "V0.4 item shape rows target the mature pull-up item tray, category tabs, and scroll language.",
                tray.items.Count > 0
                    && tray.categoryLabels.Count >= 6
                    && string.Equals(tray.targetNodeName, BattlePageViewFormalReferenceKeys.PrepareItemTrayRoot, StringComparison.Ordinal)
                    && !tray.canWriteFormalUi,
                $"items={tray.items.Count}, categories={tray.categoryLabels.Count}, writesFormalUi={tray.canWriteFormalUi}"));

            DragRotationPlacementExtensionViewModel drag =
                viewModel.dragRotationPlacement ?? new DragRotationPlacementExtensionViewModel();
            rows.Add(Probe(
                "dragRotationPlacement",
                "拖拽与旋转摆放",
                "DraggableTalismanItemView",
                drag.extensionId,
                drag.adapterOutputKey,
                "V0.4 rotation and placement feedback are adapter rows for the mature drag gesture, not a new drag system.",
                drag.rows.Count > 0
                    && string.Equals(drag.targetComponentName, "DraggableTalismanItemView", StringComparison.Ordinal)
                    && !drag.canWriteFormalUi,
                $"rows={drag.rows.Count}, placementLegal={drag.placementLegal}, writesFormalUi={drag.canWriteFormalUi}"));

            ItemInfoBuildFieldsExtensionViewModel info =
                viewModel.itemInfoBuildFields ?? new ItemInfoBuildFieldsExtensionViewModel();
            int infoFieldCount = info.items.Sum(item => item.fields?.Count ?? 0);
            rows.Add(Probe(
                "itemInfoBuildFields",
                "道具信息字段扩展",
                "V02TalismanTooltipUI",
                info.extensionId,
                info.adapterOutputKey,
                "Build shape, affix, and synergy fields stay as tooltip/info adapter data.",
                info.items.Count > 0
                    && infoFieldCount >= info.items.Count * 5
                    && !info.canWriteFormalUi,
                $"items={info.items.Count}, fields={infoFieldCount}, writesFormalUi={info.canWriteFormalUi}"));

            BattleFeedbackMechanicHintExtensionViewModel feedback =
                viewModel.battleFeedbackMechanicHint ?? new BattleFeedbackMechanicHintExtensionViewModel();
            rows.Add(Probe(
                "battleFeedbackMechanicHint",
                "摆放与机制反馈",
                "BattleLog / Tooltip / FloatingCombatText",
                feedback.extensionId,
                feedback.adapterOutputKey,
                "Placement legality and readiness hints reuse existing battle feedback language.",
                feedback.rows.Count > 0 && !feedback.canWriteFormalUi,
                $"rows={feedback.rows.Count}, writesFormalUi={feedback.canWriteFormalUi}"));

            BattlePrepareVisualSpec prepare = spec?.battlePrepare ?? BattlePrepareVisualSpec.CreateDefault();
            ItemTrayVisualSpec traySpec = spec?.itemTray ?? ItemTrayVisualSpec.CreateDefault();
            rows.Add(Probe(
                "pullUpHandFeel",
                "上拉整备手感",
                BattlePageViewFormalReferenceKeys.PrepareMotionRoot,
                "MatureBattlePrepareMotion",
                "battlePrepare.motion",
                "The playtest keeps mature board+tray synchronized motion and only probes the adapter target contract.",
                prepare.boardAndTrayMoveTogether
                    && prepare.overlayBehindBoardAndTray
                    && Math.Abs(prepare.boardSize - 800f) <= 0.01f
                    && Math.Abs(prepare.itemTraySize - 800f) <= 0.01f,
                $"boardAndTrayMoveTogether={prepare.boardAndTrayMoveTogether}, overlayBehind={prepare.overlayBehindBoardAndTray}, board={prepare.boardSize}, tray={prepare.itemTraySize}"));

            rows.Add(Probe(
                "trayScrollHandFeel",
                "道具栏滚动手感",
                BattlePageViewFormalReferenceKeys.PrepareItemTrayRoot,
                "MatureItemTrayScroll",
                "itemTray.scroll",
                "The playtest keeps the mature 5x8 tray and vertical scroll contract.",
                traySpec.columnCount >= 5
                    && traySpec.rowCount >= 8
                    && traySpec.slotCount >= 40
                    && traySpec.verticalScroll,
                $"columns={traySpec.columnCount}, rows={traySpec.rowCount}, slots={traySpec.slotCount}, scroll={traySpec.verticalScroll}"));

            return rows;
        }

        private static BattlePrepareComponentAdapterPlaytestProbeRow Probe(
            string probeId,
            string chineseDisplayName,
            string matureComponent,
            string extensionId,
            string adapterOutputKey,
            string handFeelClaim,
            bool passed,
            string evidence)
        {
            return new BattlePrepareComponentAdapterPlaytestProbeRow
            {
                probeId = probeId,
                chineseDisplayName = chineseDisplayName,
                matureComponent = matureComponent,
                extensionId = extensionId,
                adapterOutputKey = adapterOutputKey,
                handFeelClaim = handFeelClaim,
                passed = passed,
                evidence = evidence,
                writesFormalUi = false,
                modifiesFormalScene = false,
                overwritesRectTransform = false
            };
        }

        private static List<BattlePrepareComponentAdapterPlaytestManualCheckRow> BuildManualChecks()
        {
            return new List<BattlePrepareComponentAdapterPlaytestManualCheckRow>
            {
                Manual(
                    "runMenu",
                    "运行 QA 菜单",
                    "Tools/Talisman Bag/V0.4/BuildSandbox/BattlePrepareComponentAdapterPlaytest01/[QA Only] Run Battle Prepare Component Adapter Playtest",
                    "三份 Playtest 报告生成且 Status 为 PASS。"),
                Manual(
                    "compareMaturePrepare",
                    "比较成熟整备手感",
                    "在现有战斗整备页中进入整备、滚动道具栏、拖拽道具、继续战斗。",
                    "棋盘、道具栏、拖拽、上拉/下收仍使用 V0.2/V0.3 成熟表现；没有出现另一套 V04 UI。"),
                Manual(
                    "checkV04DataLayer",
                    "检查 V0.4 扩展数据",
                    "查看 PlaytestMap.csv 中 BoardOccupancy / ItemTrayShape / DragRotation / ItemInfo / Feedback 行。",
                    "多格、旋转、占格反馈以 Extension / Adapter 行出现，且 canWriteFormalUi=False。"),
                Manual(
                    "console",
                    "Console 检查",
                    "运行菜单后查看 Unity Console。",
                    "本包无红色 Error / 黄色 Warning；若只能完成静态报告，则报告必须如实显示 staticPlaytestOnly=True。")
            };
        }

        private static BattlePrepareComponentAdapterPlaytestManualCheckRow Manual(
            string checkId,
            string chineseDisplayName,
            string action,
            string expected)
        {
            return new BattlePrepareComponentAdapterPlaytestManualCheckRow
            {
                checkId = checkId,
                chineseDisplayName = chineseDisplayName,
                action = action,
                expected = expected
            };
        }

        private static string ResolveConclusion(BattlePrepareComponentAdapterPlaytestController controller)
        {
            if (controller == null || !controller.AllProbesPassed)
            {
                return "Static devOnly playtest failed; do not treat the adapter as hand-feel-ready.";
            }

            return "Static devOnly playtest passed: V0.4 shape, rotation, occupancy, item info, and feedback data target mature BattlePrepare extension surfaces without redrawing UI or writing formal flow.";
        }
    }

    [Serializable]
    public sealed class BattlePrepareComponentAdapterPlaytestProbeRow
    {
        public string probeId = string.Empty;
        public string chineseDisplayName = string.Empty;
        public string matureComponent = string.Empty;
        public string extensionId = string.Empty;
        public string adapterOutputKey = string.Empty;
        public string handFeelClaim = string.Empty;
        public bool passed;
        public string evidence = string.Empty;
        public bool writesFormalUi;
        public bool modifiesFormalScene;
        public bool overwritesRectTransform;
    }

    [Serializable]
    public sealed class BattlePrepareComponentAdapterPlaytestManualCheckRow
    {
        public string checkId = string.Empty;
        public string chineseDisplayName = string.Empty;
        public string action = string.Empty;
        public string expected = string.Empty;
    }
}
