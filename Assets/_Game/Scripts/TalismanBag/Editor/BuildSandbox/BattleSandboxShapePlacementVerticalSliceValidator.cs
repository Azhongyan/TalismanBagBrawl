#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TalismanBag.BuildSandbox;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BattleSandboxShapePlacementVerticalSliceValidator
    {
        public const string PackageName = BuildGridInteractionPreviewController.VerticalSlicePackageName;
        public const string QaMenuPath =
            "Tools/Talisman Bag/V0.4/BuildSandbox/BattleSandboxShapePlacementVerticalSlice01/[QA Only] Run Vertical Slice";

        private const string RuntimeControllerPath =
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs";
        private const string GridSlotViewPath =
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridPreviewSlotView.cs";
        private const string ItemCardViewPath =
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs";
        private const string ItemTrayViewPath =
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs";
        private const string TrayPlacementViewModelPath =
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/TrayPlacementViewModel.cs";
        private const string TrayItemLayoutViewPath =
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/TrayItemLayoutView.cs";
        private const string TrayGridReservationViewPath =
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/TrayGridReservationView.cs";
        private const string ItemInfoPanelPath =
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildSandboxItemInfoPanel.cs";

        private static readonly string[] RequiredSamples =
        {
            "initial_idle",
            "tray_vertical_two_cell_default",
            "tray_rotate_to_horizontal",
            "tray_rotate_back_to_vertical",
            "tray_rotate_invalid_keeps_direction",
            "click_item_info_only",
            "drag_starts_placement",
            "drag_to_board_ghost",
            "release_valid_commits",
            "release_invalid_returns_to_tray",
            "no_ghost_click_confirm",
            "cancel_available",
        };

        private static readonly string[] ForbiddenRuntimeTokens =
        {
            "V03BattlePrepareInteractionController",
            "V02RunFlowController",
            "V02FormationGridFrame",
            "MainTrialProgressData",
            "PlayerPrefs",
            "RunFlow",
            "PageState",
            "FormationState",
            "BossHand",
            "BossReward",
            "RewardDrop",
            "DropBias",
            "Scene_TalismanBag_V02",
            "Scene_TalismanBag_V03"
        };

        [MenuItem(QaMenuPath)]
        public static void RunMenu()
        {
            Run(throwOnFailure: false);
        }

        public static void RunBatch()
        {
            bool passed = Run(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static bool Run(bool throwOnFailure)
        {
            BattleSandboxShapePlacementVerticalSlicePlan plan = BuildDefaultPlan();
            List<BuildSandboxValidationReport> reports = BuildValidationReports(plan);
            string[] reportPaths =
                BattleSandboxShapePlacementVerticalSliceReportWriter.WriteReports(reports, plan);
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            foreach (BuildSandboxValidationIssue issue in reports.SelectMany(report => report.Issues))
            {
                switch (issue.Level)
                {
                    case BuildSandboxValidationLevel.Error:
                        Debug.LogError(issue.ToString());
                        break;
                    case BuildSandboxValidationLevel.Warning:
                        Debug.LogWarning(issue.ToString());
                        break;
                    default:
                        Debug.Log(issue.ToString());
                        break;
                }
            }

            Debug.Log(
                $"[{PackageName}] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"{PackageName} failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static List<BuildSandboxValidationReport> BuildValidationReports()
        {
            return BuildValidationReports(BuildDefaultPlan());
        }

        public static List<BuildSandboxValidationReport> BuildValidationReports(
            BattleSandboxShapePlacementVerticalSlicePlan plan)
        {
            return new List<BuildSandboxValidationReport>
            {
                BuildSandboxConfigValidator.Validate(),
                BuildSandboxDevOnlyValidator.Validate(),
                BattleSandboxPreviewSceneVerifier.Validate(),
                BuildGridInteractionPreviewValidator.ValidateSceneBinding(),
                ValidateSceneBinding(plan),
                ValidateProtocol(plan),
                ValidateLeakCheck(plan)
            };
        }

        public static BattleSandboxShapePlacementVerticalSlicePlan BuildDefaultPlan()
        {
            BattleSandboxShapePlacementVerticalSlicePlan plan = new()
            {
                packageName = PackageName,
                targetScenePath = BuildSandboxPreviewSceneMarker.ScenePath,
                featureFlagsDefaultEnabled = !BuildSandboxFeatureFlags.AreAllDefaultsDisabled(),
                usesShapePlacementSession = true,
                usesShapeAwareItemTrayGrid = true,
                usesMobileShapePlacementInputExtension = true,
                usesFormalRuntimeCore = false,
                touchesFormalScene = false,
                touchesRunFlow = false,
                touchesSaveData = false,
                touchesBossRewardDropNumeric = false
            };

            BuildItemAndShapeSnapshot(plan);
            BuildInteractionSourceSnapshot(plan);
            BuildProtocolRows(plan);
            plan.sceneSnapshot = BuildSceneSnapshot();
            BuildLeakRows(plan);
            return plan;
        }

        public static BuildSandboxValidationReport ValidateSceneBinding(
            BattleSandboxShapePlacementVerticalSlicePlan plan)
        {
            BuildSandboxValidationReport report = new("Battle Sandbox Shape Placement Vertical Slice Scene");
            BattleSandboxShapePlacementVerticalSlicePlan safePlan = plan ?? BuildDefaultPlan();
            BattleSandboxShapePlacementSceneSnapshot snapshot =
                safePlan.sceneSnapshot ?? new BattleSandboxShapePlacementSceneSnapshot();

            RequireTrue(report, snapshot.sceneExists, "BATTLE_SANDBOX_VSLICE_SCENE_MISSING", "V04 preview scene must exist.", safePlan.targetScenePath);
            RequireTrue(report, snapshot.controllerPresent, "BATTLE_SANDBOX_VSLICE_CONTROLLER_MISSING", "Vertical slice controller must be present in V04 preview scene.", safePlan.targetScenePath);
            RequireTrue(report, snapshot.battleLikePreviewAreaPresent, "BATTLE_SANDBOX_VSLICE_SURFACE_MISSING", "Battle-like preview area must exist for the V0.2/V0.3 visual surface.", safePlan.targetScenePath);
            RequireTrue(report, snapshot.boardGridPresent, "BATTLE_SANDBOX_VSLICE_BOARD_MISSING", "BoardGridPreview must exist.", safePlan.targetScenePath);
            RequireTrue(report, snapshot.itemTrayPresent, "BATTLE_SANDBOX_VSLICE_TRAY_MISSING", "ItemTrayPreview must exist.", safePlan.targetScenePath);
            RequireTrue(report, snapshot.placementFeedbackPresent, "BATTLE_SANDBOX_VSLICE_FEEDBACK_MISSING", "PlacementFeedback must exist.", safePlan.targetScenePath);
            RequireTrue(report, snapshot.rotateButtonPresent, "BATTLE_SANDBOX_VSLICE_ROTATE_BUTTON_MISSING", "Rotate button must exist.", safePlan.targetScenePath);
            RequireTrue(report, snapshot.cancelButtonPresent, "BATTLE_SANDBOX_VSLICE_CANCEL_BUTTON_MISSING", "Cancel button must exist.", safePlan.targetScenePath);

            RequireTrue(report, snapshot.controllerDevOnly, "BATTLE_SANDBOX_VSLICE_NOT_DEVONLY", "Controller must remain devOnly=true.", nameof(BuildGridInteractionPreviewController));
            RequireFalse(report, snapshot.controllerIsEnabled, "BATTLE_SANDBOX_VSLICE_ENABLED_TRUE", "Controller must remain isEnabled=false.", nameof(BuildGridInteractionPreviewController));
            RequireFalse(report, snapshot.controllerReadsFormalSave, "BATTLE_SANDBOX_VSLICE_READS_FORMAL_SAVE", "Controller must not read formal save data.", nameof(BuildGridInteractionPreviewController));
            RequireFalse(report, snapshot.controllerWritesFormalFlow, "BATTLE_SANDBOX_VSLICE_WRITES_FORMAL_FLOW", "Controller must not write formal flow.", nameof(BuildGridInteractionPreviewController));
            RequireFalse(report, snapshot.controllerWritesFormalUi, "BATTLE_SANDBOX_VSLICE_WRITES_FORMAL_UI", "Controller must not write formal UI.", nameof(BuildGridInteractionPreviewController));
            RequireFalse(report, snapshot.controllerTouchesFormalScene, "BATTLE_SANDBOX_VSLICE_TOUCHES_FORMAL_SCENE", "Controller must not touch formal scenes.", nameof(BuildGridInteractionPreviewController));
            RequireFalse(report, snapshot.controllerShowsCompleteAnswers, "BATTLE_SANDBOX_VSLICE_SHOWS_COMPLETE_ANSWERS", "Controller must not show complete answers.", nameof(BuildGridInteractionPreviewController));
            RequireFalse(report, snapshot.buildSettingsContainsPreview, "BATTLE_SANDBOX_VSLICE_BUILD_SETTINGS_LEAK", "V04 preview scene must not be added to Build Settings.", "ProjectSettings/EditorBuildSettings.asset");

            RequireAtLeast(report, safePlan.previewItemCount, 9, "BATTLE_SANDBOX_VSLICE_ITEM_COUNT", "Preview tray must expose the primary x2 item plus additional test items.", RuntimeControllerPath);
            RequireEqual(report, safePlan.shapeCellCount, 2, "BATTLE_SANDBOX_VSLICE_SHAPE_CELL_COUNT", "Vertical slice item must be an x2 shape.", RuntimeControllerPath);
            RequireTrue(report, safePlan.trayTwoCellDisplay, "BATTLE_SANDBOX_VSLICE_TRAY_X2_DISPLAY", "Tray footprint must occupy two cells.", RuntimeControllerPath);
            RequireTrue(report, safePlan.trayVisualTwoCellDisplay, "BATTLE_SANDBOX_VSLICE_TRAY_X2_VISUAL", "Tray view must render the x2 item as a real two-cell span.", ItemTrayViewPath);
            RequireTrue(report, string.Equals(safePlan.previewShapeId, "Vertical2", StringComparison.Ordinal), "BATTLE_SANDBOX_VSLICE_SHAPE_ID", "Vertical slice item must use Vertical2 shape.", RuntimeControllerPath);

            return report;
        }

        public static BuildSandboxValidationReport ValidateProtocol(
            BattleSandboxShapePlacementVerticalSlicePlan plan)
        {
            BuildSandboxValidationReport report = new("Battle Sandbox Shape Placement Protocol");
            BattleSandboxShapePlacementVerticalSlicePlan safePlan = plan ?? BuildDefaultPlan();
            IReadOnlyList<BattleSandboxShapePlacementStateRow> rows =
                safePlan.rows != null
                    ? safePlan.rows
                    : Array.Empty<BattleSandboxShapePlacementStateRow>();

            RequireTrue(report, safePlan.usesShapePlacementSession, "BATTLE_SANDBOX_VSLICE_NO_SESSION", "ShapePlacementSession must be the interaction authority.", RuntimeControllerPath);
            RequireTrue(report, safePlan.usesShapeAwareItemTrayGrid, "BATTLE_SANDBOX_VSLICE_NO_SHAPE_TRAY", "ShapeAwareItemTrayGrid must back tray packing/display.", RuntimeControllerPath);
            RequireTrue(report, safePlan.usesMobileShapePlacementInputExtension, "BATTLE_SANDBOX_VSLICE_NO_MOBILE_INPUT", "MobileShapePlacementInputExtension must drive the input protocol.", RuntimeControllerPath);
            RequireFalse(report, safePlan.usesFormalRuntimeCore, "BATTLE_SANDBOX_VSLICE_FORMAL_CORE_CONNECTED", "V0.2/V0.3 runtime core must not be connected.", RuntimeControllerPath);
            RequireTrue(report, safePlan.clickItemInfoOnly, "BATTLE_SANDBOX_VSLICE_CLICK_NOT_INFO_ONLY", "Single item click must keep placement input/session idle and only refresh item info.", ItemCardViewPath);
            RequireTrue(report, safePlan.cardClickDoesNotRotate, "BATTLE_SANDBOX_VSLICE_CARD_CLICK_ROTATES", "Item card click must not call RotateSelectedItem.", ItemCardViewPath);
            RequireTrue(report, safePlan.dragStartsPlacement, "BATTLE_SANDBOX_VSLICE_DRAG_DOES_NOT_START_PLACEMENT", "Dragging a tray item must start the placement session.", RuntimeControllerPath);
            RequireTrue(report, safePlan.rotateUsesExplicitButton, "BATTLE_SANDBOX_VSLICE_ROTATE_NOT_EXPLICIT_BUTTON", "Rotation must be driven by the item info popup Rotate button path.", RuntimeControllerPath);
            RequireTrue(report, safePlan.trayRotateOnly, "BATTLE_SANDBOX_VSLICE_ROTATE_NOT_TRAY_ONLY", "Rotation must be tray-only.", RuntimeControllerPath);
            RequireTrue(report, safePlan.noRotationWhileDragging, "BATTLE_SANDBOX_VSLICE_DRAG_ROTATE_ENABLED", "Rotation must be disabled while dragging/previewing.", RuntimeControllerPath);
            RequireTrue(report, safePlan.noRotationOnBoard, "BATTLE_SANDBOX_VSLICE_BOARD_ROTATE_ENABLED", "Rotation must be disabled after board placement.", RuntimeControllerPath);
            RequireTrue(report, safePlan.trayVerticalDefault, "BATTLE_SANDBOX_VSLICE_TRAY_VERTICAL_DEFAULT", "Vertical2 must default to a vertical two-cell tray footprint.", ItemTrayViewPath);
            RequireTrue(report, safePlan.trayHorizontalAfterRotate, "BATTLE_SANDBOX_VSLICE_TRAY_ROTATE_HORIZONTAL", "Popup Rotate must turn Vertical2 into a horizontal two-cell tray footprint.", ItemTrayViewPath);
            RequireTrue(report, safePlan.trayRotateBackToVertical, "BATTLE_SANDBOX_VSLICE_TRAY_ROTATE_BACK_VERTICAL", "Popup Rotate must toggle back to vertical.", ItemTrayViewPath);
            RequireTrue(report, safePlan.trayRotationFailsInvalid, "BATTLE_SANDBOX_VSLICE_TRAY_ROTATE_INVALID_NOT_REJECTED", "Popup Rotate must fail on out-of-bounds or overlap and keep the old direction.", ItemTrayViewPath);

            foreach (string requiredSample in RequiredSamples)
            {
                RequireTrue(
                    report,
                    rows.Any(row => row.sampleId == requiredSample),
                    "BATTLE_SANDBOX_VSLICE_SAMPLE_MISSING",
                    $"Missing required protocol sample: {requiredSample}.",
                    PackageName);
            }

            foreach (BattleSandboxShapePlacementStateRow row in rows)
            {
                if (row == null)
                {
                    report.AddError("BATTLE_SANDBOX_VSLICE_ROW_NULL", "Null protocol row.", PackageName);
                    continue;
                }

                if (row.expectedInputState != row.toInputState)
                {
                    report.AddError(
                        "BATTLE_SANDBOX_VSLICE_INPUT_STATE_MISMATCH",
                        $"{row.sampleId} expected input={row.expectedInputState}, actual={row.toInputState}.",
                        PackageName);
                }

                if (row.expectedSessionState != row.toSessionState)
                {
                    report.AddError(
                        "BATTLE_SANDBOX_VSLICE_SESSION_STATE_MISMATCH",
                        $"{row.sampleId} expected session={row.expectedSessionState}, actual={row.toSessionState}.",
                        PackageName);
                }

                if (row.expectedValid != row.actualValid)
                {
                    report.AddError(
                        "BATTLE_SANDBOX_VSLICE_VALIDITY_MISMATCH",
                        $"{row.sampleId} expected valid={row.expectedValid}, actual={row.actualValid}.",
                        PackageName);
                }

                report.AddInfo(
                    "BATTLE_SANDBOX_VSLICE_SAMPLE",
                    $"{row.sampleId}: {row.action} {row.fromInputState}/{row.fromSessionState}->{row.toInputState}/{row.toSessionState}, valid={row.actualValid}.",
                    PackageName);
            }

            RequireTrue(report, safePlan.releaseCommitsIfValid, "BATTLE_SANDBOX_VSLICE_RELEASE_VALID_NOT_COMMITTED", "Valid drag release must commit immediately.", PackageName);
            RequireTrue(report, safePlan.releaseReturnsToTrayIfInvalid, "BATTLE_SANDBOX_VSLICE_RELEASE_INVALID_NOT_RETURNED", "Invalid drag release must return the item to tray.", PackageName);
            RequireTrue(report, safePlan.noGhostClickConfirm, "BATTLE_SANDBOX_VSLICE_GHOST_CONFIRM_STILL_REQUIRED", "Ghost click confirm must not be required.", PackageName);
            RequireTrue(report, safePlan.cancelAvailable, "BATTLE_SANDBOX_VSLICE_CANCEL_UNAVAILABLE", "Cancel must be available.", PackageName);

            return report;
        }

        public static BuildSandboxValidationReport ValidateLeakCheck(
            BattleSandboxShapePlacementVerticalSlicePlan plan)
        {
            BuildSandboxValidationReport report = new("Battle Sandbox Shape Placement Leak Check");
            BattleSandboxShapePlacementVerticalSlicePlan safePlan = plan ?? BuildDefaultPlan();

            RequireFalse(report, safePlan.featureFlagsDefaultEnabled, "BATTLE_SANDBOX_VSLICE_FEATURE_DEFAULT_ON", "BuildSandbox feature flags must default to disabled.", nameof(BuildSandboxFeatureFlags));
            RequireFalse(report, safePlan.touchesFormalScene, "BATTLE_SANDBOX_VSLICE_FORMAL_SCENE_TOUCH", "Formal V02/V03 scenes must not be touched.", PackageName);
            RequireFalse(report, safePlan.touchesRunFlow, "BATTLE_SANDBOX_VSLICE_RUNFLOW_TOUCH", "RunFlow must not be touched.", PackageName);
            RequireFalse(report, safePlan.touchesSaveData, "BATTLE_SANDBOX_VSLICE_SAVE_TOUCH", "Save data must not be touched.", PackageName);
            RequireFalse(report, safePlan.touchesBossRewardDropNumeric, "BATTLE_SANDBOX_VSLICE_BOSS_REWARD_NUMERIC_TOUCH", "Boss/reward/drop/numeric systems must not be touched.", PackageName);

            foreach (BattleSandboxShapePlacementLeakRow row in safePlan.leakRows ?? new List<BattleSandboxShapePlacementLeakRow>())
            {
                if (row.isLeak)
                {
                    report.AddError(
                        "BATTLE_SANDBOX_VSLICE_LEAK",
                        $"{row.checkId}: {row.detail}",
                        row.assetPath);
                }
                else
                {
                    report.AddInfo(
                        "BATTLE_SANDBOX_VSLICE_LEAK_CLEAR",
                        $"{row.checkId}: {row.detail}",
                        row.assetPath);
                }
            }

            return report;
        }

        private static void BuildItemAndShapeSnapshot(BattleSandboxShapePlacementVerticalSlicePlan plan)
        {
            string cardSource = ReadProjectText(ItemCardViewPath);
            string traySource = ReadProjectText(ItemTrayViewPath);
            string placementModelSource = ReadProjectText(TrayPlacementViewModelPath);
            string itemLayoutSource = ReadProjectText(TrayItemLayoutViewPath);
            string reservationSource = ReadProjectText(TrayGridReservationViewPath);
            bool trayUsesReservedSlotVisuals =
                traySource.IndexOf("TrayGridReservationView", StringComparison.Ordinal) >= 0
                && reservationSource.IndexOf("occupiedSlotIndexes", StringComparison.Ordinal) >= 0
                && reservationSource.IndexOf("SetSlotReserved", StringComparison.Ordinal) >= 0;
            bool cardUsesTwoCellOverlay =
                cardSource.IndexOf("ConfigureTrayFootprint", StringComparison.Ordinal) < 0
                && cardSource.IndexOf("ApplyTraySpanRect", StringComparison.Ordinal) < 0
                && cardSource.IndexOf("SetSlotReservedVisual", StringComparison.Ordinal) < 0
                && placementModelSource.IndexOf("occupiedSlotIndexes", StringComparison.Ordinal) >= 0
                && itemLayoutSource.IndexOf("ApplyCardRect", StringComparison.Ordinal) >= 0
                && itemLayoutSource.IndexOf("sizeDelta", StringComparison.Ordinal) >= 0;
            IReadOnlyList<BuildGridInteractionPreviewController.PreviewItem> items =
                BuildGridInteractionPreviewController.CreatePreviewItems();
            Dictionary<string, ItemShapeConfig> shapes = BuildGridInteractionPreviewController
                .CreatePreviewShapeConfigs()
                .ToDictionary(shape => shape.shapeId, StringComparer.Ordinal);

            BuildGridInteractionPreviewController.PreviewItem item = items.FirstOrDefault();
            plan.previewItemCount = items.Count;
            plan.previewItemId = item?.ItemId ?? string.Empty;
            plan.previewItemName = item?.DisplayName ?? string.Empty;
            plan.previewShapeId = item?.ShapeId ?? string.Empty;

            if (item != null && shapes.TryGetValue(item.ShapeId, out ItemShapeConfig shape))
            {
                plan.shapeCellCount = shape.cellCount;
                plan.shapeRotationAllowed = shape.rotationAllowed;
                ShapeItemPayload payload = BuildPayload(item, shape, ShapePlacementSource.Tray);
                ShapeAwareItemTrayGrid trayGrid = new(
                    receiverId: "battle_sandbox_vertical_slice_tray",
                    columnCount: BuildGridInteractionPreviewController.TrayColumns,
                    slotCount: BuildGridInteractionPreviewController.TrayColumns * BuildGridInteractionPreviewController.TrayRows,
                    commitAllowed: true);
                plan.trayPacked = trayGrid.TryPack(payload, out ShapePlacementResult trayResult);
                trayGrid.TryGetPlacement(item.ItemId, out ShapeAwareItemTrayGridPlacement placement);
                plan.trayPlacementCellCount = placement?.CellCount ?? 0;
                plan.trayOccupiedCells = FormatCells(trayResult?.OccupiedCells);
                plan.trayOccupiedSlots = placement == null
                    ? string.Empty
                    : string.Join(";", placement.OccupiedSlotIndexes);
                plan.trayTwoCellDisplay =
                    plan.trayPacked
                    && plan.trayPlacementCellCount == 2
                    && trayResult != null
                    && trayResult.IsValid;
                plan.trayVerticalDefault =
                    plan.trayTwoCellDisplay
                    && placement != null
                    && placement.Rotation == ItemShapeRotation.Rotation0
                    && string.Join(";", placement.OccupiedSlotIndexes) == "0;5";
                plan.trayVisualTwoCellDisplay =
                    plan.trayTwoCellDisplay
                    && BuildItemTrayPreviewView.SupportsShapeAwareCellSpans
                    && placement != null
                    && placement.OccupiedSlotIndexes.Count == 2
                    && trayUsesReservedSlotVisuals
                    && cardUsesTwoCellOverlay;

                plan.rows.Add(new BattleSandboxShapePlacementStateRow
                {
                    sampleId = "tray_vertical_two_cell_default",
                    action = "ShapeAwareItemTrayGrid.TryPack",
                    fromInputState = MobileShapePlacementInputState.Idle,
                    toInputState = MobileShapePlacementInputState.Idle,
                    expectedInputState = MobileShapePlacementInputState.Idle,
                    fromSessionState = ShapePlacementState.Idle,
                    toSessionState = ShapePlacementState.Idle,
                    expectedSessionState = ShapePlacementState.Idle,
                    expectedValid = true,
                    actualValid = plan.trayVerticalDefault && plan.trayVisualTwoCellDisplay,
                    occupiedCells = plan.trayOccupiedCells,
                    receiverCommitBefore = 0,
                    receiverCommitAfter = trayGrid.CommitCount,
                    note = $"Vertical2 defaults to a real vertical two-cell tray span. slots={plan.trayOccupiedSlots}"
                });

                ShapePlacementSession rotateSession = new();
                ShapeItemPayload horizontalPayload = payload.WithRotation(ItemShapeRotation.Rotation90);
                rotateSession.Begin(horizontalPayload, trayAnchorCell: placement?.AnchorCell ?? new ItemShapeCell(0, 0));
                ShapePlacementResult horizontalResult = rotateSession.Commit(trayGrid);
                trayGrid.TryGetPlacement(item.ItemId, out ShapeAwareItemTrayGridPlacement horizontalPlacement);
                plan.trayHorizontalAfterRotate =
                    horizontalResult != null
                    && horizontalResult.IsValid
                    && horizontalPlacement != null
                    && horizontalPlacement.Rotation == ItemShapeRotation.Rotation90
                    && string.Join(";", horizontalPlacement.OccupiedSlotIndexes) == "0;1";
                plan.rows.Add(new BattleSandboxShapePlacementStateRow
                {
                    sampleId = "tray_rotate_to_horizontal",
                    action = "Info popup Rotate button",
                    fromInputState = MobileShapePlacementInputState.Idle,
                    toInputState = MobileShapePlacementInputState.Idle,
                    expectedInputState = MobileShapePlacementInputState.Idle,
                    fromSessionState = ShapePlacementState.Idle,
                    toSessionState = ShapePlacementState.Committed,
                    expectedSessionState = ShapePlacementState.Committed,
                    expectedValid = true,
                    actualValid = plan.trayHorizontalAfterRotate,
                    anchor = horizontalResult?.AnchorCell.ToString() ?? string.Empty,
                    occupiedCells = FormatCells(horizontalResult?.OccupiedCells),
                    receiverCommitBefore = 1,
                    receiverCommitAfter = trayGrid.CommitCount,
                    note = "Popup Rotate changes Vertical2 to a horizontal two-cell tray span."
                });

                ShapePlacementSession rotateBackSession = new();
                rotateBackSession.Begin(payload, trayAnchorCell: horizontalPlacement?.AnchorCell ?? new ItemShapeCell(0, 0));
                ShapePlacementResult rotateBackResult = rotateBackSession.Commit(trayGrid);
                trayGrid.TryGetPlacement(item.ItemId, out ShapeAwareItemTrayGridPlacement rotateBackPlacement);
                plan.trayRotateBackToVertical =
                    rotateBackResult != null
                    && rotateBackResult.IsValid
                    && rotateBackPlacement != null
                    && rotateBackPlacement.Rotation == ItemShapeRotation.Rotation0
                    && string.Join(";", rotateBackPlacement.OccupiedSlotIndexes) == "0;5";
                plan.rows.Add(new BattleSandboxShapePlacementStateRow
                {
                    sampleId = "tray_rotate_back_to_vertical",
                    action = "Info popup Rotate button again",
                    fromInputState = MobileShapePlacementInputState.Idle,
                    toInputState = MobileShapePlacementInputState.Idle,
                    expectedInputState = MobileShapePlacementInputState.Idle,
                    fromSessionState = ShapePlacementState.Idle,
                    toSessionState = ShapePlacementState.Committed,
                    expectedSessionState = ShapePlacementState.Committed,
                    expectedValid = true,
                    actualValid = plan.trayRotateBackToVertical,
                    anchor = rotateBackResult?.AnchorCell.ToString() ?? string.Empty,
                    occupiedCells = FormatCells(rotateBackResult?.OccupiedCells),
                    receiverCommitBefore = 2,
                    receiverCommitAfter = trayGrid.CommitCount,
                    note = "Popup Rotate toggles the x2 item back to vertical."
                });

                ShapeAwareItemTrayGrid edgeTrayGrid = new(
                    receiverId: "battle_sandbox_vertical_slice_edge_tray",
                    columnCount: BuildGridInteractionPreviewController.TrayColumns,
                    slotCount: BuildGridInteractionPreviewController.TrayColumns * BuildGridInteractionPreviewController.TrayRows,
                    commitAllowed: true);
                ShapePlacementSession edgeSession = new();
                ItemShapeCell edgeAnchor = new(BuildGridInteractionPreviewController.TrayColumns - 1, 0);
                edgeSession.Begin(payload, trayAnchorCell: edgeAnchor);
                ShapePlacementResult edgeVerticalResult = edgeSession.Commit(edgeTrayGrid);
                ShapePlacementSession invalidRotateSession = new();
                invalidRotateSession.Begin(horizontalPayload, trayAnchorCell: edgeAnchor);
                ShapePlacementResult invalidRotateResult = invalidRotateSession.Commit(edgeTrayGrid);
                edgeTrayGrid.TryGetPlacement(item.ItemId, out ShapeAwareItemTrayGridPlacement edgePlacementAfterInvalid);
                plan.trayRotationFailsInvalid =
                    edgeVerticalResult != null
                    && edgeVerticalResult.IsValid
                    && invalidRotateResult != null
                    && !invalidRotateResult.IsValid
                    && invalidRotateResult.InvalidReason == ShapePlacementInvalidReason.OutOfGrid
                    && edgePlacementAfterInvalid != null
                    && edgePlacementAfterInvalid.Rotation == ItemShapeRotation.Rotation0;
                plan.rows.Add(new BattleSandboxShapePlacementStateRow
                {
                    sampleId = "tray_rotate_invalid_keeps_direction",
                    action = "Info popup Rotate button at right edge",
                    fromInputState = MobileShapePlacementInputState.Idle,
                    toInputState = MobileShapePlacementInputState.Idle,
                    expectedInputState = MobileShapePlacementInputState.Idle,
                    fromSessionState = ShapePlacementState.Idle,
                    toSessionState = ShapePlacementState.InvalidPreview,
                    expectedSessionState = ShapePlacementState.InvalidPreview,
                    expectedValid = false,
                    actualValid = invalidRotateResult != null && invalidRotateResult.IsValid,
                    anchor = invalidRotateResult?.AnchorCell.ToString() ?? string.Empty,
                    occupiedCells = FormatCells(invalidRotateResult?.OccupiedCells),
                    invalidReason = invalidRotateResult?.InvalidReason ?? ShapePlacementInvalidReason.None,
                    receiverCommitBefore = 1,
                    receiverCommitAfter = edgeTrayGrid.CommitCount,
                    note = "Out-of-bounds tray rotation is rejected and the previous vertical placement remains."
                });
            }
        }

        private static void BuildInteractionSourceSnapshot(BattleSandboxShapePlacementVerticalSlicePlan plan)
        {
            string controllerSource = ReadProjectText(RuntimeControllerPath);
            string cardSource = ReadProjectText(ItemCardViewPath);
            string itemInfoPanelSource = ReadProjectText(ItemInfoPanelPath);

            plan.cardClickDoesNotRotate =
                cardSource.IndexOf("RotateSelectedItem", StringComparison.Ordinal) < 0;
            plan.rotateUsesExplicitButton =
                controllerSource.IndexOf("RotateTrayItem", StringComparison.Ordinal) >= 0
                && controllerSource.IndexOf("SetRotateHandler", StringComparison.Ordinal) >= 0
                && controllerSource.IndexOf("RotateInfoPanelItem", StringComparison.Ordinal) >= 0
                && itemInfoPanelSource.IndexOf("RotateButtonObjectName", StringComparison.Ordinal) >= 0
                && itemInfoPanelSource.IndexOf("OnRotateClicked", StringComparison.Ordinal) >= 0
                && plan.cardClickDoesNotRotate;
            plan.trayRotateOnly =
                plan.rotateUsesExplicitButton
                && controllerSource.IndexOf("AddListener(RotateSelectedItem)", StringComparison.Ordinal) < 0
                && controllerSource.IndexOf("TapSelectedItemToRotate", StringComparison.Ordinal) < 0;
            plan.noRotationWhileDragging =
                controllerSource.IndexOf("activeDragItemId", StringComparison.Ordinal) >= 0
                && controllerSource.IndexOf("CanRotateItemFromInfoPanel", StringComparison.Ordinal) >= 0
                && controllerSource.IndexOf("string.IsNullOrEmpty(activeDragItemId)", StringComparison.Ordinal) >= 0;
            plan.noRotationOnBoard =
                controllerSource.IndexOf("placedItemIds", StringComparison.Ordinal) >= 0
                && controllerSource.IndexOf("placedItemIds.Contains(item.ItemId)", StringComparison.Ordinal) >= 0;
            plan.noGhostClickConfirm =
                controllerSource.IndexOf("TapGhostToConfirm", StringComparison.Ordinal) < 0
                && controllerSource.IndexOf("ReleaseDragLockPreview", StringComparison.Ordinal) < 0;
        }

        private static void BuildProtocolRows(BattleSandboxShapePlacementVerticalSlicePlan plan)
        {
            IReadOnlyList<BuildGridInteractionPreviewController.PreviewItem> items =
                BuildGridInteractionPreviewController.CreatePreviewItems();
            Dictionary<string, ItemShapeConfig> shapes = BuildGridInteractionPreviewController
                .CreatePreviewShapeConfigs()
                .ToDictionary(shape => shape.shapeId, StringComparer.Ordinal);
            BuildGridInteractionPreviewController.PreviewItem item = items.FirstOrDefault();
            if (item == null || !shapes.TryGetValue(item.ShapeId, out ItemShapeConfig shape))
            {
                return;
            }

            ShapeItemPayload payload = BuildPayload(item, shape, ShapePlacementSource.Tray);
            ShapePlacementSession session = new();
            MobileShapePlacementInputExtension input = new(session);
            InMemoryShapeGridReceiver board = new(
                "battle_sandbox_vertical_slice_board",
                ShapePlacementSource.Board,
                width: BuildGridInteractionPreviewController.BoardColumns,
                height: BuildGridInteractionPreviewController.BoardRows,
                commitAllowed: true,
                cellSize: new Vector2(40f, 40f));

            plan.rows.Add(Row(
                "initial_idle",
                "Open scene",
                MobileShapePlacementInputState.Idle,
                input.CurrentState,
                ShapePlacementState.Idle,
                session.CurrentState,
                expectedInputState: MobileShapePlacementInputState.Idle,
                expectedSessionState: ShapePlacementState.Idle,
                expectedValid: true,
                actualValid: input.CurrentState == MobileShapePlacementInputState.Idle
                    && session.CurrentState == ShapePlacementState.Idle,
                result: null,
                board,
                note: "Initial state is idle before pickup."));

            MobileShapePlacementInputState beforeInput = input.CurrentState;
            ShapePlacementState beforeSession = session.CurrentState;
            plan.clickItemInfoOnly =
                input.CurrentState == MobileShapePlacementInputState.Idle
                && session.CurrentState == ShapePlacementState.Idle
                && board.CommitCount == 0;
            plan.rows.Add(Row(
                "click_item_info_only",
                "Click item info",
                beforeInput,
                input.CurrentState,
                beforeSession,
                session.CurrentState,
                expectedInputState: MobileShapePlacementInputState.Idle,
                expectedSessionState: ShapePlacementState.Idle,
                expectedValid: true,
                actualValid: plan.clickItemInfoOnly,
                result: session.PreviewResult,
                board,
                note: "Single click opens or refreshes item info only; it does not start placement or rotate."));

            beforeInput = input.CurrentState;
            beforeSession = session.CurrentState;
            bool pickedUp = input.TapTrayItem(payload, new ItemShapeCell(0, 0));
            plan.dragStartsPlacement =
                pickedUp
                && input.CurrentState == MobileShapePlacementInputState.HoldingItem
                && session.CurrentState == ShapePlacementState.HoldingItem;
            plan.rows.Add(Row(
                "drag_starts_placement",
                "BeginDrag tray item",
                beforeInput,
                input.CurrentState,
                beforeSession,
                session.CurrentState,
                expectedInputState: MobileShapePlacementInputState.HoldingItem,
                expectedSessionState: ShapePlacementState.HoldingItem,
                expectedValid: true,
                actualValid: plan.dragStartsPlacement,
                result: session.PreviewResult,
                board,
                note: "Dragging the x2 item is the only pickup entry for placement."));

            beforeInput = input.CurrentState;
            beforeSession = session.CurrentState;
            ShapePlacementResult dragResult = input.DragToReceiver(board, new Vector2(80f, 80f));
            plan.rows.Add(Row(
                "drag_to_board_ghost",
                "Drag to board",
                beforeInput,
                input.CurrentState,
                beforeSession,
                session.CurrentState,
                expectedInputState: MobileShapePlacementInputState.DraggingPreview,
                expectedSessionState: ShapePlacementState.Previewing,
                expectedValid: true,
                actualValid: dragResult != null
                    && dragResult.IsValid
                    && input.LastGhostPreview.visible
                    && input.LastGhostPreview.outlineStyle == GhostPlacementOutlineStyle.Valid,
                result: dragResult,
                board,
                note: "Dragging over board creates a valid ghost preview."));

            beforeInput = input.CurrentState;
            beforeSession = session.CurrentState;
            int releaseCommitBefore = board.CommitCount;
            ShapePlacementResult commitResult = session.Commit(board);
            int releaseCommitAfter = board.CommitCount;
            plan.releaseCommitsIfValid =
                commitResult != null
                && commitResult.IsValid
                && session.CurrentState == ShapePlacementState.Committed
                && releaseCommitAfter == releaseCommitBefore + 1;
            plan.rows.Add(Row(
                "release_valid_commits",
                "Release drag",
                beforeInput,
                MobileShapePlacementInputState.Placed,
                beforeSession,
                session.CurrentState,
                expectedInputState: MobileShapePlacementInputState.Placed,
                expectedSessionState: ShapePlacementState.Committed,
                expectedValid: true,
                actualValid: plan.releaseCommitsIfValid,
                result: commitResult,
                board,
                note: "Valid release commits directly without ghost click confirm."));
            BattleSandboxShapePlacementStateRow releaseRow = plan.rows[plan.rows.Count - 1];
            releaseRow.receiverCommitBefore = releaseCommitBefore;
            releaseRow.receiverCommitAfter = releaseCommitAfter;

            plan.rows.Add(Row(
                "no_ghost_click_confirm",
                "No ghost click confirm",
                MobileShapePlacementInputState.Placed,
                MobileShapePlacementInputState.Placed,
                ShapePlacementState.Committed,
                ShapePlacementState.Committed,
                expectedInputState: MobileShapePlacementInputState.Placed,
                expectedSessionState: ShapePlacementState.Committed,
                expectedValid: true,
                actualValid: plan.noGhostClickConfirm,
                result: commitResult,
                board,
                note: "Controller does not call TapGhostToConfirm or ReleaseDragLockPreview."));

            BuildInvalidReleaseSample(plan, payload);

            BuildCancelSample(plan, payload);
        }

        private static void BuildInvalidReleaseSample(
            BattleSandboxShapePlacementVerticalSlicePlan plan,
            ShapeItemPayload payload)
        {
            ShapePlacementSession session = new();
            MobileShapePlacementInputExtension input = new(session);
            InMemoryShapeGridReceiver board = new(
                "battle_sandbox_vertical_slice_invalid_release_board",
                ShapePlacementSource.Board,
                width: BuildGridInteractionPreviewController.BoardColumns,
                height: BuildGridInteractionPreviewController.BoardRows,
                commitAllowed: true,
                cellSize: new Vector2(40f, 40f));
            input.TapTrayItem(payload, new ItemShapeCell(0, 0));
            ShapePlacementResult invalidResult = input.DragToReceiver(board, new Vector2(240f, 240f));
            MobileShapePlacementInputState beforeInput = input.CurrentState;
            ShapePlacementState beforeSession = session.CurrentState;
            int commitCountBefore = board.CommitCount;
            input.Cancel(board);
            int commitCountAfter = board.CommitCount;
            plan.releaseReturnsToTrayIfInvalid =
                invalidResult != null
                && !invalidResult.IsValid
                && invalidResult.InvalidReason == ShapePlacementInvalidReason.OutOfGrid
                && input.CurrentState == MobileShapePlacementInputState.Cancelled
                && session.CurrentState == ShapePlacementState.Cancelled
                && commitCountBefore == commitCountAfter;
            plan.rows.Add(Row(
                "release_invalid_returns_to_tray",
                "Release invalid drag",
                beforeInput,
                input.CurrentState,
                beforeSession,
                session.CurrentState,
                expectedInputState: MobileShapePlacementInputState.Cancelled,
                expectedSessionState: ShapePlacementState.Cancelled,
                expectedValid: true,
                actualValid: plan.releaseReturnsToTrayIfInvalid,
                result: invalidResult,
                board,
                note: "Invalid release cancels the active drag and returns the item to tray without board commit."));
            BattleSandboxShapePlacementStateRow row = plan.rows[plan.rows.Count - 1];
            row.receiverCommitBefore = commitCountBefore;
            row.receiverCommitAfter = commitCountAfter;
        }

        private static void BuildCancelSample(
            BattleSandboxShapePlacementVerticalSlicePlan plan,
            ShapeItemPayload payload)
        {
            ShapePlacementSession session = new();
            MobileShapePlacementInputExtension input = new(session);
            InMemoryShapeGridReceiver board = new(
                "battle_sandbox_vertical_slice_cancel_board",
                ShapePlacementSource.Board,
                width: BuildGridInteractionPreviewController.BoardColumns,
                height: BuildGridInteractionPreviewController.BoardRows,
                commitAllowed: true,
                cellSize: new Vector2(40f, 40f));
            input.TapTrayItem(payload, new ItemShapeCell(0, 0));
            input.DragToReceiver(board, new Vector2(40f, 40f));
            MobileShapePlacementInputState beforeInput = input.CurrentState;
            ShapePlacementState beforeSession = session.CurrentState;
            int cancelCountBefore = board.CancelCount;
            input.Cancel(board);
            plan.cancelAvailable =
                input.CurrentState == MobileShapePlacementInputState.Cancelled
                && session.CurrentState == ShapePlacementState.Cancelled
                && board.CancelCount == cancelCountBefore + 1;
            plan.rows.Add(Row(
                "cancel_available",
                "Cancel",
                beforeInput,
                input.CurrentState,
                beforeSession,
                session.CurrentState,
                expectedInputState: MobileShapePlacementInputState.Cancelled,
                expectedSessionState: ShapePlacementState.Cancelled,
                expectedValid: true,
                actualValid: plan.cancelAvailable,
                result: session.PreviewResult,
                board,
                note: "Cancel clears the active ghost/session without board commit."));
        }

        private static BattleSandboxShapePlacementStateRow Row(
            string sampleId,
            string action,
            MobileShapePlacementInputState fromInputState,
            MobileShapePlacementInputState toInputState,
            ShapePlacementState fromSessionState,
            ShapePlacementState toSessionState,
            MobileShapePlacementInputState expectedInputState,
            ShapePlacementState expectedSessionState,
            bool expectedValid,
            bool actualValid,
            ShapePlacementResult result,
            InMemoryShapeGridReceiver board,
            string note)
        {
            return new BattleSandboxShapePlacementStateRow
            {
                sampleId = sampleId ?? string.Empty,
                action = action ?? string.Empty,
                fromInputState = fromInputState,
                toInputState = toInputState,
                expectedInputState = expectedInputState,
                fromSessionState = fromSessionState,
                toSessionState = toSessionState,
                expectedSessionState = expectedSessionState,
                expectedValid = expectedValid,
                actualValid = actualValid,
                anchor = result?.AnchorCell.ToString() ?? string.Empty,
                occupiedCells = FormatCells(result?.OccupiedCells),
                invalidReason = result?.InvalidReason ?? ShapePlacementInvalidReason.None,
                ghostVisible =
                    toInputState == MobileShapePlacementInputState.DraggingPreview
                    || toInputState == MobileShapePlacementInputState.PreviewLocked
                    || toInputState == MobileShapePlacementInputState.InvalidPreview,
                receiverCommitBefore = board?.CommitCount ?? 0,
                receiverCommitAfter = board?.CommitCount ?? 0,
                note = note ?? string.Empty
            };
        }

        private static BattleSandboxShapePlacementSceneSnapshot BuildSceneSnapshot()
        {
            BattleSandboxShapePlacementSceneSnapshot snapshot = new();
            string projectRoot = ProjectRoot;
            string sceneAbsolutePath = Path.Combine(projectRoot, BuildSandboxPreviewSceneMarker.ScenePath);
            snapshot.sceneExists = File.Exists(sceneAbsolutePath);
            snapshot.buildSettingsContainsPreview = EditorBuildSettings.scenes.Any(scene =>
                scene != null
                && string.Equals(scene.path, BuildSandboxPreviewSceneMarker.ScenePath, StringComparison.Ordinal));

            if (!snapshot.sceneExists)
            {
                return snapshot;
            }

            Scene previousScene = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.OpenScene(BuildSandboxPreviewSceneMarker.ScenePath, OpenSceneMode.Single);
            try
            {
                snapshot.safeAreaRootPresent = FindDeepChildInScene(scene, "SafeAreaRoot") != null;
                snapshot.battleLikePreviewAreaPresent = FindDeepChildInScene(scene, "BattleLikePreviewArea") != null;
                snapshot.boardGridPresent = FindDeepChildInScene(scene, "BoardGridPreview") != null;
                snapshot.itemTrayPresent = FindDeepChildInScene(scene, "ItemTrayPreview") != null;
                snapshot.placementFeedbackPresent = FindDeepChildInScene(scene, "PlacementFeedback") != null;
                snapshot.rotateButtonPresent = FindDeepChildInScene(scene, "RotatePreviewButtonSlot") != null;
                snapshot.cancelButtonPresent = FindDeepChildInScene(scene, "ResetPreviewButtonSlot") != null;

                BuildGridInteractionPreviewController controller =
                    UnityEngine.Object.FindObjectOfType<BuildGridInteractionPreviewController>(true);
                snapshot.controllerPresent = controller != null;
                if (controller != null)
                {
                    snapshot.controllerDevOnly = controller.DevOnly;
                    snapshot.controllerIsEnabled = controller.IsEnabled;
                    snapshot.controllerReadsFormalSave = controller.ReadsFormalSaveData;
                    snapshot.controllerWritesFormalFlow = controller.WritesFormalFlow;
                    snapshot.controllerWritesFormalUi = controller.WritesFormalUi;
                    snapshot.controllerTouchesFormalScene = controller.TouchesFormalScene;
                    snapshot.controllerShowsCompleteAnswers = controller.ShowsCompleteAnswers;
                    snapshot.boardSlotCount = controller.BoardSlotCount;
                    snapshot.traySlotCount = controller.TraySlotCount;
                    snapshot.categoryCount = controller.CategoryCount;
                    snapshot.previewItemCount = controller.PreviewItemCount;
                }
            }
            finally
            {
                if (previousScene.IsValid()
                    && !string.IsNullOrEmpty(previousScene.path)
                    && previousScene.path != scene.path)
                {
                    EditorSceneManager.OpenScene(previousScene.path, OpenSceneMode.Single);
                }
            }

            return snapshot;
        }

        private static void BuildLeakRows(BattleSandboxShapePlacementVerticalSlicePlan plan)
        {
            BattleSandboxShapePlacementSceneSnapshot snapshot =
                plan.sceneSnapshot ?? new BattleSandboxShapePlacementSceneSnapshot();

            plan.leakRows.Add(new BattleSandboxShapePlacementLeakRow
            {
                checkId = "feature_flags_default_disabled",
                assetPath = nameof(BuildSandboxFeatureFlags),
                isLeak = plan.featureFlagsDefaultEnabled,
                detail = plan.featureFlagsDefaultEnabled
                    ? "At least one BuildSandbox feature flag defaults true."
                    : "All BuildSandbox feature flags default false."
            });
            plan.leakRows.Add(new BattleSandboxShapePlacementLeakRow
            {
                checkId = "preview_scene_not_in_build_settings",
                assetPath = "ProjectSettings/EditorBuildSettings.asset",
                isLeak = snapshot.buildSettingsContainsPreview,
                detail = snapshot.buildSettingsContainsPreview
                    ? "V04 preview scene is present in Build Settings."
                    : "V04 preview scene is not present in Build Settings."
            });
            plan.leakRows.Add(new BattleSandboxShapePlacementLeakRow
            {
                checkId = "controller_formal_surface_flags",
                assetPath = RuntimeControllerPath,
                isLeak = snapshot.controllerReadsFormalSave
                    || snapshot.controllerWritesFormalFlow
                    || snapshot.controllerWritesFormalUi
                    || snapshot.controllerTouchesFormalScene
                    || snapshot.controllerShowsCompleteAnswers,
                detail = "Controller formal surface flags remain false."
            });

            foreach (string assetPath in RuntimeScanPaths())
            {
                string fullPath = Path.Combine(ProjectRoot, assetPath);
                string text = File.Exists(fullPath) ? File.ReadAllText(fullPath) : string.Empty;
                foreach (string token in ForbiddenRuntimeTokens)
                {
                    bool contains = text.IndexOf(token, StringComparison.Ordinal) >= 0;
                    plan.leakRows.Add(new BattleSandboxShapePlacementLeakRow
                    {
                        checkId = "runtime_forbidden_token_" + token,
                        assetPath = assetPath,
                        isLeak = contains,
                        detail = contains
                            ? $"Found forbidden runtime token `{token}`."
                            : $"Forbidden runtime token `{token}` absent."
                    });
                }
            }
        }

        private static IEnumerable<string> RuntimeScanPaths()
        {
            yield return BuildSandboxPreviewSceneMarker.ScenePath;
            yield return RuntimeControllerPath;
            yield return GridSlotViewPath;
            yield return ItemCardViewPath;
            yield return ItemTrayViewPath;
            yield return ItemInfoPanelPath;
        }

        private static string ReadProjectText(string assetPath)
        {
            string fullPath = Path.Combine(ProjectRoot, assetPath ?? string.Empty);
            return File.Exists(fullPath) ? File.ReadAllText(fullPath) : string.Empty;
        }

        private static ShapeItemPayload BuildPayload(
            BuildGridInteractionPreviewController.PreviewItem item,
            ItemShapeConfig shape,
            ShapePlacementSource source)
        {
            IReadOnlyList<ItemShapeCell> occupiedOffsets =
                shape != null ? shape.occupiedOffsets : Array.Empty<ItemShapeCell>();
            return new ShapeItemPayload(
                item?.ItemId ?? string.Empty,
                item?.ShapeId ?? string.Empty,
                ItemShapeRotation.Rotation0,
                occupiedOffsets,
                source);
        }

        private static Transform FindDeepChildInScene(Scene scene, string objectName)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name == objectName)
                {
                    return root.transform;
                }

                Transform found = FindDeepChild(root.transform, objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static Transform FindDeepChild(Transform parent, string objectName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == objectName)
                {
                    return child;
                }

                Transform found = FindDeepChild(child, objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static string FormatCells(IReadOnlyList<ItemShapeCell> cells)
        {
            return cells == null || cells.Count == 0
                ? string.Empty
                : string.Join(";", cells.Select(cell => cell.ToString()));
        }

        private static void RequireTrue(
            BuildSandboxValidationReport report,
            bool value,
            string code,
            string message,
            string assetPath)
        {
            if (value)
            {
                report.AddInfo(code, message, assetPath);
                return;
            }

            report.AddError(code, message, assetPath);
        }

        private static void RequireFalse(
            BuildSandboxValidationReport report,
            bool value,
            string code,
            string message,
            string assetPath)
        {
            if (value)
            {
                report.AddError(code, message, assetPath);
                return;
            }

            report.AddInfo(code, message, assetPath);
        }

        private static void RequireEqual(
            BuildSandboxValidationReport report,
            int actual,
            int expected,
            string code,
            string message,
            string assetPath)
        {
            if (actual == expected)
            {
                report.AddInfo(code, $"{message} actual={actual}.", assetPath);
                return;
            }

            report.AddError(code, $"{message} actual={actual}, expected={expected}.", assetPath);
        }

        private static void RequireAtLeast(
            BuildSandboxValidationReport report,
            int actual,
            int expectedMinimum,
            string code,
            string message,
            string assetPath)
        {
            if (actual >= expectedMinimum)
            {
                report.AddInfo(code, $"{message} actual={actual}, expected>={expectedMinimum}.", assetPath);
                return;
            }

            report.AddError(code, $"{message} actual={actual}, expected>={expectedMinimum}.", assetPath);
        }

        private static string ProjectRoot =>
            Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
    }

    public sealed class BattleSandboxShapePlacementVerticalSlicePlan
    {
        public string packageName = string.Empty;
        public string targetScenePath = string.Empty;
        public bool devOnly = true;
        public bool isEnabled;
        public bool featureFlagsDefaultEnabled;
        public bool usesShapePlacementSession;
        public bool usesShapeAwareItemTrayGrid;
        public bool usesMobileShapePlacementInputExtension;
        public bool usesFormalRuntimeCore;
        public bool touchesFormalScene;
        public bool touchesRunFlow;
        public bool touchesSaveData;
        public bool touchesBossRewardDropNumeric;
        public string previewItemId = string.Empty;
        public string previewItemName = string.Empty;
        public string previewShapeId = string.Empty;
        public int previewItemCount;
        public int shapeCellCount;
        public bool shapeRotationAllowed;
        public bool trayPacked;
        public int trayPlacementCellCount;
        public string trayOccupiedCells = string.Empty;
        public string trayOccupiedSlots = string.Empty;
        public bool trayTwoCellDisplay;
        public bool trayVisualTwoCellDisplay;
        public bool trayVerticalDefault;
        public bool trayHorizontalAfterRotate;
        public bool trayRotateBackToVertical;
        public bool trayRotationFailsInvalid;
        public bool clickItemInfoOnly;
        public bool cardClickDoesNotRotate;
        public bool dragStartsPlacement;
        public bool rotateUsesExplicitButton;
        public bool trayRotateOnly;
        public bool noRotationWhileDragging;
        public bool noRotationOnBoard;
        public bool releaseCommitsIfValid;
        public bool releaseReturnsToTrayIfInvalid;
        public bool noGhostClickConfirm;
        public bool cancelAvailable;
        public BattleSandboxShapePlacementSceneSnapshot sceneSnapshot = new();
        public List<BattleSandboxShapePlacementStateRow> rows = new();
        public List<BattleSandboxShapePlacementLeakRow> leakRows = new();
    }

    public sealed class BattleSandboxShapePlacementSceneSnapshot
    {
        public bool sceneExists;
        public bool safeAreaRootPresent;
        public bool battleLikePreviewAreaPresent;
        public bool boardGridPresent;
        public bool itemTrayPresent;
        public bool placementFeedbackPresent;
        public bool rotateButtonPresent;
        public bool cancelButtonPresent;
        public bool controllerPresent;
        public bool controllerDevOnly = true;
        public bool controllerIsEnabled;
        public bool controllerReadsFormalSave;
        public bool controllerWritesFormalFlow;
        public bool controllerWritesFormalUi;
        public bool controllerTouchesFormalScene;
        public bool controllerShowsCompleteAnswers;
        public bool buildSettingsContainsPreview;
        public int boardSlotCount;
        public int traySlotCount;
        public int categoryCount;
        public int previewItemCount;
    }

    public sealed class BattleSandboxShapePlacementStateRow
    {
        public string sampleId = string.Empty;
        public string action = string.Empty;
        public MobileShapePlacementInputState fromInputState;
        public MobileShapePlacementInputState toInputState;
        public MobileShapePlacementInputState expectedInputState;
        public ShapePlacementState fromSessionState;
        public ShapePlacementState toSessionState;
        public ShapePlacementState expectedSessionState;
        public bool expectedValid;
        public bool actualValid;
        public string anchor = string.Empty;
        public string occupiedCells = string.Empty;
        public ShapePlacementInvalidReason invalidReason;
        public bool ghostVisible;
        public int receiverCommitBefore;
        public int receiverCommitAfter;
        public string note = string.Empty;
    }

    public sealed class BattleSandboxShapePlacementLeakRow
    {
        public string checkId = string.Empty;
        public string assetPath = string.Empty;
        public bool isLeak;
        public string detail = string.Empty;
    }
}
#endif
