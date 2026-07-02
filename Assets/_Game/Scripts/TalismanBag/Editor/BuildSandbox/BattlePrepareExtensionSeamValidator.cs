#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using TalismanBag.BuildSandbox;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public sealed class BattlePrepareExtensionSeamMapRow
    {
        public string seamId = string.Empty;
        public string matureSurface = string.Empty;
        public string extensionProvider = string.Empty;
        public string defaultBehavior = string.Empty;
        public bool present;
    }

    public sealed class BattlePrepareExtensionSeamPlan
    {
        public string packageName = BattlePrepareExtensionSeamValidator.PackageName;
        public bool devOnly = true;
        public bool isEnabled;
        public bool formalBehaviorChanged;
        public bool featureFlagDefaultEnabled;
        public bool touchesRunFlow;
        public bool touchesSaveData;
        public bool touchesBossRewardDropNumeric;
        public bool touchesFormalSceneLayout;
        public bool rewritesBattlePrepare;
        public bool redrawsBoardOrItemTray;
        public bool restoresOverlayIgnoreLayoutDelayedDrop;
        public bool controllerSourceExists;
        public bool seamInterfaceSourceExists;
        public bool buildSandboxAdapterSourceExists;
        public bool runtimePlaytestSourceHasInjection;
        public List<BattlePrepareExtensionSeamMapRow> rows = new();
    }

    public static class BattlePrepareExtensionSeamValidator
    {
        public const string PackageName = "V0.4-BattlePrepareExtensionSeam01";
        public const string QaMenuPath =
            "Tools/Talisman Bag/V0.4/BuildSandbox/BattlePrepareExtensionSeam01/[QA Only] Run Extension Seam Validation";

        private const string ControllerSourcePath =
            "Assets/_Game/Scripts/TalismanBag/V03/BattlePrepare/V03BattlePrepareInteractionController.cs";
        private const string SeamInterfaceSourcePath =
            "Assets/_Game/Scripts/TalismanBag/V03/BattlePrepare/BattlePrepareExtensionSeams.cs";
        private const string BuildSandboxAdapterSourcePath =
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BattlePrepareShapePlacementSeamAdapter.cs";
        private const string RuntimePlaytestSourcePath =
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BattlePrepareComponentAdapterRuntimePlaytest.cs";

        public static List<BuildSandboxValidationReport> BuildValidationReports()
        {
            return new List<BuildSandboxValidationReport> { Validate(BuildDefaultPlan()) };
        }

        public static BattlePrepareExtensionSeamPlan BuildDefaultPlan()
        {
            string controller = ReadProjectText(ControllerSourcePath);
            string seamInterfaces = ReadProjectText(SeamInterfaceSourcePath);
            string adapter = ReadProjectText(BuildSandboxAdapterSourcePath);
            string runtimePlaytest = ReadProjectText(RuntimePlaytestSourcePath);

            BattlePrepareExtensionSeamPlan plan = new()
            {
                featureFlagDefaultEnabled = !BuildSandboxFeatureFlags.AreAllDefaultsDisabled(),
                controllerSourceExists = !string.IsNullOrEmpty(controller),
                seamInterfaceSourceExists = !string.IsNullOrEmpty(seamInterfaces),
                buildSandboxAdapterSourceExists = !string.IsNullOrEmpty(adapter),
                runtimePlaytestSourceHasInjection =
                    runtimePlaytest.Contains("extensionSeamAdapter", StringComparison.Ordinal)
                    && runtimePlaytest.Contains("TryInjectExtensionSeamProvider", StringComparison.Ordinal),
                restoresOverlayIgnoreLayoutDelayedDrop =
                    ContainsForbiddenRoute(controller)
                    || ContainsForbiddenRoute(seamInterfaces)
                    || ContainsForbiddenRoute(adapter)
                    || ContainsForbiddenRoute(runtimePlaytest)
            };

            plan.rows.Add(Row(
                "shapePlacementSessionProvider",
                "V03BattlePrepareInteractionController provider injection",
                "BattlePrepareShapePlacementSeamAdapter.ShapePlacementSessionProvider",
                "Provider is null in formal runtime unless devOnly playtest injects it.",
                seamInterfaces.Contains("IShapePlacementSessionProvider", StringComparison.Ordinal)
                    && controller.Contains("TryInjectExtensionSeamProvider", StringComparison.Ordinal)
                    && adapter.Contains("ShapePlacementSessionProvider", StringComparison.Ordinal)));
            plan.rows.Add(Row(
                "shapeItemPayloadProvider",
                "DraggableTalismanItemView click/drag notification",
                "BattlePrepareShapePlacementSeamAdapter.TryBuildShapeItemPayload",
                "Formal drag behavior remains owned by DraggableTalismanItemView.",
                seamInterfaces.Contains("IShapeItemPayloadProvider", StringComparison.Ordinal)
                    && controller.Contains("NotifyExtensionDragContext", StringComparison.Ordinal)
                    && adapter.Contains("TryBuildShapeItemPayload", StringComparison.Ordinal)));
            plan.rows.Add(Row(
                "shapeGridReceiverProvider",
                "Mature board and item tray slot resolving",
                "ShapeGridReceiver / ShapeAwareItemTrayGrid",
                "Board/tray RectTransforms are read only; no formal layout is rewritten.",
                seamInterfaces.Contains("IShapeGridReceiverProvider", StringComparison.Ordinal)
                    && controller.Contains("TryResolveShapeBoardReceiver", StringComparison.Ordinal)
                    && controller.Contains("TryResolveShapeItemTrayReceiver", StringComparison.Ordinal)
                    && adapter.Contains("TryResolveBoardReceiver", StringComparison.Ordinal)));
            plan.rows.Add(Row(
                "ghostPreviewAdapter",
                "Existing drag/ghost phase observation",
                "IBattlePrepareGhostPreviewAdapter",
                "Default adapter is no-op; devOnly layer can observe without replacing drag.",
                seamInterfaces.Contains("IBattlePrepareGhostPreviewAdapter", StringComparison.Ordinal)
                    && controller.Contains("PreviewGhost", StringComparison.Ordinal)
                    && adapter.Contains("ClearGhostPreview", StringComparison.Ordinal)));
            plan.rows.Add(Row(
                "placementCommitAdapter",
                "Item tray drop and continue button commit",
                "IBattlePreparePlacementCommitAdapter",
                "TryCommitItemTrayPlacement returns false in devOnly adapter so mature commit proceeds.",
                seamInterfaces.Contains("IBattlePreparePlacementCommitAdapter", StringComparison.Ordinal)
                    && controller.Contains("OnBattlePrepareCommitRequested", StringComparison.Ordinal)
                    && controller.Contains("OnBattlePrepareCommitted", StringComparison.Ordinal)
                    && controller.Contains("OnItemTrayPlacementCommitted", StringComparison.Ordinal)));
            plan.rows.Add(Row(
                "placementCancelAdapter",
                "Back-home / controller destroy cancellation",
                "IBattlePreparePlacementCancelAdapter",
                "Cancel clears devOnly session only; formal page flow stays unchanged.",
                seamInterfaces.Contains("IBattlePreparePlacementCancelAdapter", StringComparison.Ordinal)
                    && controller.Contains("NotifyExtensionPlacementCancelled", StringComparison.Ordinal)
                    && adapter.Contains("CancelPlacement", StringComparison.Ordinal)));

            return plan;
        }

        private static BuildSandboxValidationReport Validate(BattlePrepareExtensionSeamPlan plan)
        {
            BuildSandboxValidationReport report = new(PackageName);
            if (plan == null)
            {
                report.AddError("BATTLEPREPARE_SEAM_PLAN_NULL", "BattlePrepare extension seam plan was not created.", PackageName);
                return report;
            }

            RequireTrue(report, plan.devOnly, "BATTLEPREPARE_SEAM_DEVONLY_FALSE", "Seam package must remain devOnly.");
            RequireFalse(report, plan.isEnabled, "BATTLEPREPARE_SEAM_ENABLED_TRUE", "Seam package must keep isEnabled=false.");
            RequireFalse(report, plan.featureFlagDefaultEnabled, "BATTLEPREPARE_SEAM_FEATURE_DEFAULT_ON", "BuildSandbox feature flags must default to disabled.");
            RequireFalse(report, plan.formalBehaviorChanged, "BATTLEPREPARE_SEAM_FORMAL_BEHAVIOR_CHANGED", "Formal BattlePrepare behavior must remain unchanged by default.");
            RequireFalse(report, plan.touchesRunFlow, "BATTLEPREPARE_SEAM_RUNFLOW_TOUCH", "Formal RunFlow must not be modified.");
            RequireFalse(report, plan.touchesSaveData, "BATTLEPREPARE_SEAM_SAVEDATA_TOUCH", "SaveData must not be modified.");
            RequireFalse(report, plan.touchesBossRewardDropNumeric, "BATTLEPREPARE_SEAM_BOSS_REWARD_NUMERIC_TOUCH", "Boss/reward/drop/numeric systems must not be modified.");
            RequireFalse(report, plan.touchesFormalSceneLayout, "BATTLEPREPARE_SEAM_SCENE_LAYOUT_TOUCH", "Formal scene layout and RectTransforms must not be modified.");
            RequireFalse(report, plan.rewritesBattlePrepare, "BATTLEPREPARE_SEAM_REWRITE", "BattlePrepare must not be rewritten.");
            RequireFalse(report, plan.redrawsBoardOrItemTray, "BATTLEPREPARE_SEAM_REDRAW", "Board and item tray must not be redrawn.");
            RequireFalse(report, plan.restoresOverlayIgnoreLayoutDelayedDrop, "BATTLEPREPARE_SEAM_OLD_ROUTE_RESTORED", "Old overlay + ignoreLayout + delayed drop route must stay absent.");
            RequireTrue(report, plan.controllerSourceExists, "BATTLEPREPARE_SEAM_CONTROLLER_MISSING", ControllerSourcePath);
            RequireTrue(report, plan.seamInterfaceSourceExists, "BATTLEPREPARE_SEAM_INTERFACES_MISSING", SeamInterfaceSourcePath);
            RequireTrue(report, plan.buildSandboxAdapterSourceExists, "BATTLEPREPARE_SEAM_ADAPTER_MISSING", BuildSandboxAdapterSourcePath);
            RequireTrue(report, plan.runtimePlaytestSourceHasInjection, "BATTLEPREPARE_SEAM_RUNTIME_INJECTION_MISSING", RuntimePlaytestSourcePath);

            foreach (BattlePrepareExtensionSeamMapRow row in plan.rows ?? new List<BattlePrepareExtensionSeamMapRow>())
            {
                if (row == null)
                {
                    report.AddError("BATTLEPREPARE_SEAM_ROW_NULL", "Null seam map row.", PackageName);
                    continue;
                }

                if (!row.present)
                {
                    report.AddError("BATTLEPREPARE_SEAM_ROW_MISSING", $"Missing seam row: {row.seamId}.", PackageName);
                }
            }

            return report;
        }

        private static BattlePrepareExtensionSeamMapRow Row(
            string seamId,
            string matureSurface,
            string extensionProvider,
            string defaultBehavior,
            bool present)
        {
            return new BattlePrepareExtensionSeamMapRow
            {
                seamId = seamId,
                matureSurface = matureSurface,
                extensionProvider = extensionProvider,
                defaultBehavior = defaultBehavior,
                present = present
            };
        }

        private static bool ContainsForbiddenRoute(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return false;
            }

            return source.Contains("ignoreLayout", StringComparison.Ordinal)
                || source.Contains("CommitAfterMatureDragRoutine", StringComparison.Ordinal)
                || source.Contains("DelayedDrop", StringComparison.Ordinal)
                || source.Contains("delayed drop", StringComparison.OrdinalIgnoreCase);
        }

        private static string ReadProjectText(string assetPath)
        {
            string projectRoot = Directory.GetParent(UnityEngine.Application.dataPath)?.FullName ?? string.Empty;
            string fullPath = Path.Combine(projectRoot, assetPath);
            return File.Exists(fullPath) ? File.ReadAllText(fullPath) : string.Empty;
        }

        private static void RequireTrue(
            BuildSandboxValidationReport report,
            bool value,
            string code,
            string message)
        {
            if (!value)
            {
                report.AddError(code, message, PackageName);
            }
        }

        private static void RequireFalse(
            BuildSandboxValidationReport report,
            bool value,
            string code,
            string message)
        {
            if (value)
            {
                report.AddError(code, message, PackageName);
            }
        }
    }
}
#endif
