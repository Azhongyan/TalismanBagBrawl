#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TalismanBag.BuildSandbox;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BuildSandboxPlayableRegressionValidator
    {
        public const string PackageName = BuildSandboxPlayableRegression.PackageName;
        public const string QaMenuPath =
            "Tools/Talisman Bag/V0.4/BuildSandbox/BuildSandboxPlayableRegression01/[QA Only] Run Playable Regression";

        private const string GridControllerSourcePath =
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs";

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
            List<BuildSandboxValidationReport> reports = BuildValidationReports();
            BuildSandboxPlayableRegressionSnapshot snapshot = BuildRegressionSnapshot(reports);
            string[] reportPaths = BuildSandboxPlayableRegressionReportWriter.WriteReports(reports, snapshot);

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
                $"[BuildSandbox-PlayableRegression01] completed status={(snapshot.Passed ? "PASS" : "FAIL")}, errors={snapshot.errorCount}, warnings={snapshot.warningCount}, reports={string.Join(", ", reportPaths)}");

            if (!snapshot.Passed && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox PlayableRegression01 failed. See {string.Join(", ", reportPaths)}");
            }

            return snapshot.Passed;
        }

        public static List<BuildSandboxValidationReport> BuildValidationReports()
        {
            List<BuildSandboxValidationReport> reports = new();
            reports.AddRange(BuildGridInteractionPreviewValidator.BuildValidationReports());
            reports.AddRange(BattleSandboxBuildCombatPreviewValidator.BuildValidationReports());
            reports.AddRange(BattleSandboxManaLoopRuntimeValidator.BuildValidationReports());
            reports.AddRange(DevChapterBalanceRunValidator.BuildValidationReports());
            reports.Add(Validate());
            return reports;
        }

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report = new("BuildSandbox Playable Regression 01");
            BuildSandboxPlayableRegressionSnapshot snapshot =
                BuildRegressionSnapshot(Array.Empty<BuildSandboxValidationReport>());
            ValidateSnapshotEvidence(report, snapshot);
            return report;
        }

        public static BuildSandboxPlayableRegressionSnapshot BuildRegressionSnapshot(
            IReadOnlyList<BuildSandboxValidationReport> reports)
        {
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();
            BuildGridInteractionSceneBindingSnapshot sceneSnapshot =
                BuildGridInteractionPreviewValidator.BuildSceneBindingSnapshot();
            IReadOnlyList<BuildGridInteractionPlacementSample> placementSamples =
                BuildGridInteractionPreviewValidator.BuildPlacementSamples();
            IReadOnlyList<BuildGridInteractionPreviewController.PreviewItem> trayItems =
                BuildGridInteractionPreviewController.CreatePreviewItems();
            Dictionary<string, ItemShapeConfig> shapeById = BuildGridInteractionPreviewController
                .CreatePreviewShapeConfigs()
                .ToDictionary(shape => shape.shapeId, StringComparer.Ordinal);
            BattleSandboxBuildCombatPreview combatPreview =
                BattleSandboxBuildCombatPreviewValidator.BuildDefaultPreview();
            BattleSandboxManaLoopPreview manaLoopPreview =
                BattleSandboxManaLoopRuntimeValidator.BuildDefaultPreview();
            DevChapterBalanceRun balanceRun =
                DevChapterBalanceRunValidator.BuildDefaultRun();

            BuildSandboxPlayableRegressionSnapshot snapshot = new()
            {
                devOnly = true,
                isEnabled = false,
                nestedReportCount = safeReports.Count,
                errorCount = safeReports.Sum(report => report.ErrorCount),
                warningCount = safeReports.Sum(report => report.WarningCount),
                sceneExists = sceneSnapshot.SceneExists,
                x2TrayItemCount = CountTrayItemsByCellCount(trayItems, shapeById, 2),
                x3TrayItemCount = CountTrayItemsByCellCount(trayItems, shapeById, 3),
                x4TrayItemCount = CountTrayItemsByCellCount(trayItems, shapeById, 4),
                legalPlacementSampleCount = placementSamples.Count(sample => sample != null && sample.IsValid),
                illegalReturnSampleCount = placementSamples.Count(sample =>
                    sample != null
                    && !sample.IsValid
                    && (sample.InvalidReason == ShapePlacementInvalidReason.OutOfGrid
                        || sample.InvalidReason == ShapePlacementInvalidReason.CellOccupied)),
                bossStateRowCount = CountCombatRows(combatPreview, BattleSandboxEnemyCombatFeedbackKinds.BossState),
                castBarRowCount = CountCombatRows(combatPreview, BattleSandboxEnemyCombatFeedbackKinds.BossSkillCast),
                mechanicFloatingRowCount = CountCombatRows(combatPreview, BattleSandboxEnemyCombatFeedbackKinds.MechanicFeedback),
                combatLogRowCount = combatPreview?.rows?.Count(row =>
                    row != null && !string.IsNullOrWhiteSpace(row.combatLogLineChinese)) ?? 0,
                shapeRuleFeedbackRowCount = combatPreview?.ShapeBuildRuleFeedbackRowCount ?? 0,
                manaLoopGainRowCount = manaLoopPreview?.ManaGainRowCount ?? 0,
                manaLoopSpendRowCount = manaLoopPreview?.ManaSpendRowCount ?? 0,
                manaLoopGeneratedManaTotal = manaLoopPreview?.generatedManaTotal ?? 0,
                manaLoopSpentManaTotal = manaLoopPreview?.spentManaTotal ?? 0,
                manaLoopUiLayoutWriteCount = manaLoopPreview?.UiLayoutWriteCount ?? 1,
                distinctBuildFeedbackCount = balanceRun?.stages?
                    .Where(stage => stage != null)
                    .Select(stage => stage.currentBuildFeedbackChinese)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.Ordinal)
                    .Count() ?? 0,
                devBalance310StageCount = balanceRun?.Chapter310StageCount ?? 0,
                devBalance410StageCount = balanceRun?.Chapter410StageCount ?? 0,
                featureFlagDefaultTrueCount = BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue)
            };

            snapshot.sceneBindingPass =
                sceneSnapshot.SceneExists
                && sceneSnapshot.ControllerPresent
                && sceneSnapshot.BoardGridPresent
                && sceneSnapshot.ItemTrayPresent
                && sceneSnapshot.SelectedItemInfoPresent
                && sceneSnapshot.PlacementFeedbackPresent
                && sceneSnapshot.BoardSlotCount >= BuildGridInteractionPreviewController.BoardColumns
                    * BuildGridInteractionPreviewController.BoardRows
                && sceneSnapshot.TraySlotCount >= BuildGridInteractionPreviewController.TrayColumns
                    * BuildGridInteractionPreviewController.TrayRows
                && sceneSnapshot.CategoryCount >= BuildGridInteractionPreviewController.CategoryLabels.Length;
            snapshot.itemShapeCoveragePass =
                snapshot.x2TrayItemCount > 0
                && snapshot.x3TrayItemCount > 0
                && snapshot.x4TrayItemCount > 0;
            snapshot.rotationScopePass = ScanRotationScope();
            snapshot.placementSamplesPass =
                snapshot.legalPlacementSampleCount >= 2
                && snapshot.illegalReturnSampleCount >= 2;
            snapshot.currentBoardReadPass =
                combatPreview != null
                && combatPreview.readsCurrentBoardSnapshot
                && combatPreview.PlacedItemSnapshotCount > 0
                && combatPreview.BossReadinessCount > 0;
            snapshot.buildFeedbackVariationPass =
                snapshot.bossStateRowCount > 0
                && snapshot.castBarRowCount > 0
                && snapshot.mechanicFloatingRowCount > 0
                && snapshot.combatLogRowCount >= 4
                && snapshot.shapeRuleFeedbackRowCount >= 3
                && snapshot.distinctBuildFeedbackCount >= 4;
            snapshot.devChapterBalancePass =
                balanceRun != null
                && balanceRun.StageCount >= 4
                && balanceRun.Chapter310StageCount >= 2
                && balanceRun.Chapter410StageCount >= 2
                && balanceRun.EasyStageCount > 0
                && balanceRun.FitStageCount > 0
                && balanceRun.HardStageCount > 0
                && balanceRun.DevOnlyIsolationPass;
            snapshot.manaLoopPass =
                manaLoopPreview != null
                && manaLoopPreview.DevOnlyIsolationPass
                && manaLoopPreview.sourceItemStatProfileCount > 0
                && manaLoopPreview.ManaGainRowCount > 0
                && manaLoopPreview.ManaSpendRowCount > 0
                && manaLoopPreview.generatedManaTotal > 0
                && manaLoopPreview.spentManaTotal > 0
                && manaLoopPreview.UiLayoutWriteCount == 0;

            snapshot.playerSideAnswerLeakCount =
                (combatPreview?.PlayerSideAnswerLeakCount ?? 1)
                + (combatPreview?.shapeBuildRulePreview?.PlayerSideAnswerLeakCount ?? 1)
                + (manaLoopPreview?.PlayerSideAnswerLeakCount ?? 1)
                + (balanceRun?.PlayerSideAnswerLeakCount ?? 1)
                + sceneSnapshot.PlayerTextLatinViolations.Count
                + sceneSnapshot.ForbiddenAnswerTextViolations.Count;
            snapshot.formalFlowLeakCount =
                (combatPreview?.FormalFlowLeakCount ?? 1)
                + (manaLoopPreview?.FormalLeakCount ?? 1)
                + (balanceRun?.FormalFlowLeakCount ?? 1)
                + CountTrue(
                    sceneSnapshot.ControllerReadsFormalSave,
                    sceneSnapshot.ControllerWritesFormalFlow,
                    sceneSnapshot.ControllerWritesFormalUi,
                    sceneSnapshot.ControllerTouchesFormalScene,
                    sceneSnapshot.BuildSettingsContainsPreview);
            snapshot.devOnlyFalseCount = CountFalse(
                sceneSnapshot.ControllerDevOnly,
                combatPreview?.devOnly ?? false,
                combatPreview?.feedbackPreview?.devOnly ?? false,
                combatPreview?.shapeBuildRulePreview?.devOnly ?? false,
                manaLoopPreview?.devOnly ?? false,
                balanceRun?.devOnly ?? false);
            snapshot.isEnabledTrueCount = CountTrue(
                sceneSnapshot.ControllerIsEnabled,
                combatPreview?.isEnabled ?? true,
                combatPreview?.feedbackPreview?.isEnabled ?? true,
                combatPreview?.shapeBuildRulePreview?.isEnabled ?? true,
                manaLoopPreview?.isEnabled ?? true,
                balanceRun?.isEnabled ?? true);

            snapshot.playerLeakPass = snapshot.playerSideAnswerLeakCount == 0;
            snapshot.featureFlagsDefaultFalsePass =
                snapshot.featureFlagDefaultTrueCount == 0
                && BuildSandboxFeatureFlags.AreAllDefaultsDisabled();
            snapshot.devOnlyDisabledPass =
                snapshot.devOnlyFalseCount == 0
                && snapshot.isEnabledTrueCount == 0;
            snapshot.formalScopePass = snapshot.formalFlowLeakCount == 0;
            snapshot.noFormalSceneOrLayoutWritePass =
                !sceneSnapshot.BuildSettingsContainsPreview
                && !(balanceRun?.touchesV02OrV03Scene ?? true)
                && !(balanceRun?.touchesCurrentV04RectTransform ?? true)
                && (manaLoopPreview?.UiLayoutWriteCount ?? 1) == 0
                && !(manaLoopPreview?.touchesFormalSceneUiLayout ?? true)
                && !sceneSnapshot.ControllerTouchesFormalScene;
            snapshot.checklistRows = BuildChecklistRows(snapshot);
            return snapshot;
        }

        private static void ValidateSnapshotEvidence(
            BuildSandboxValidationReport report,
            BuildSandboxPlayableRegressionSnapshot snapshot)
        {
            RequireTrue(report, "PLAYABLE_REGRESSION_SCENE_READY", snapshot.sceneExists && snapshot.sceneBindingPass, "V04 sandbox scene can be opened and has required battle sandbox bindings.");
            RequireTrue(report, "PLAYABLE_REGRESSION_SHAPES_READY", snapshot.itemShapeCoveragePass, "Tray contains x2/x3/x4 multi-cell item coverage.");
            RequireTrue(report, "PLAYABLE_REGRESSION_ROTATION_SCOPE_READY", snapshot.rotationScopePass, "Rotation remains scoped to selected tray item info controls.");
            RequireTrue(report, "PLAYABLE_REGRESSION_PLACEMENT_READY", snapshot.placementSamplesPass, "Legal placement and illegal-return samples are covered.");
            RequireTrue(report, "PLAYABLE_REGRESSION_BOARD_READ_READY", snapshot.currentBoardReadPass, "Build combat preview reads the current board snapshot.");
            RequireTrue(report, "PLAYABLE_REGRESSION_FEEDBACK_READY", snapshot.buildFeedbackVariationPass, "Different build layouts produce boss state, cast, mechanic, and combat log feedback.");
            RequireTrue(report, "PLAYABLE_REGRESSION_MANA_LOOP_READY", snapshot.manaLoopPass, "devOnly mana loop reads BuildSandbox ItemStat and produces mana gain/spend rows.");
            RequireTrue(report, "PLAYABLE_REGRESSION_DEV_BALANCE_READY", snapshot.devChapterBalancePass, "3-10 and 4-10 devOnly balance run rows are present and readable.");
            RequireTrue(report, "PLAYABLE_REGRESSION_PLAYER_LEAK_CLEAR", snapshot.playerLeakPass, "Player-side forbidden answer leak counters are zero.");
            RequireTrue(report, "PLAYABLE_REGRESSION_FLAGS_FALSE", snapshot.featureFlagsDefaultFalsePass, "All BuildSandbox feature flags still default false.");
            RequireTrue(report, "PLAYABLE_REGRESSION_DEVONLY_DISABLED", snapshot.devOnlyDisabledPass, "All checked sandbox surfaces stay devOnly=true and isEnabled=false.");
            RequireTrue(report, "PLAYABLE_REGRESSION_FORMAL_SCOPE_CLEAR", snapshot.formalScopePass && snapshot.noFormalSceneOrLayoutWritePass, "No formal flow, save, reward, drop, chapter, scene, or layout-write scope is used.");
        }

        private static List<BuildSandboxPlayableRegressionChecklistRow> BuildChecklistRows(
            BuildSandboxPlayableRegressionSnapshot snapshot)
        {
            return new List<BuildSandboxPlayableRegressionChecklistRow>
            {
                Row("REG-01", "Scene", "Open V04 sandbox scene from a cold editor state.", snapshot.sceneExists && snapshot.sceneBindingPass, "Scene verifier + grid scene binding snapshot.", $"sceneExists={snapshot.sceneExists}; binding={snapshot.sceneBindingPass}", true, "User should still press Play once to confirm no runtime Console noise."),
                Row("REG-02", "Tray", "x2 / x3 / x4 items are present in the item tray.", snapshot.itemShapeCoveragePass, "Preview item shape cell-count scan.", $"x2={snapshot.x2TrayItemCount}; x3={snapshot.x3TrayItemCount}; x4={snapshot.x4TrayItemCount}", true, "Visual size and readability stay under user handtest."),
                Row("REG-03", "Rotation", "Rotation is only triggered from selected tray item info controls.", snapshot.rotationScopePass, "Controller source scan for tray-info rotation path and drag/board rotation guard text.", "RotateSelectedItem/RotateTrayItem/TryCommitTrayRotation found.", true, "User should try dragging and board items to confirm no accidental rotation."),
                Row("REG-04", "Placement", "Legal placement succeeds and illegal placement returns cleanly.", snapshot.placementSamplesPass, "Placement sample validator.", $"legal={snapshot.legalPlacementSampleCount}; illegalReturn={snapshot.illegalReturnSampleCount}", true, "User should test overlap and out-of-board release in Play mode."),
                Row("REG-05", "Continue Battle", "Prepare to continue battle reads the current board and refreshes Boss feedback.", snapshot.currentBoardReadPass, "BuildCombatPreview default board snapshot and readiness scan.", $"bossReadinessRows={snapshot.bossStateRowCount}; placedSnapshotRead={snapshot.currentBoardReadPass}", true, "User should move a build, press continue battle, and observe refreshed feedback."),
                Row("REG-06", "Build Feedback", "Different builds produce different Boss state, cast, floating text, and combat log lines.", snapshot.buildFeedbackVariationPass, "BuildCombatPreview rows + DevChapterBalanceRun distinct feedback scan.", $"bossState={snapshot.bossStateRowCount}; cast={snapshot.castBarRowCount}; mechanic={snapshot.mechanicFloatingRowCount}; logs={snapshot.combatLogRowCount}; distinctBuildFeedback={snapshot.distinctBuildFeedbackCount}", true, "User should compare at least two layouts before and after continuing battle."),
                Row("REG-14", "Mana Loop", "devOnly mana loop produces and spends mana from V0.4 BuildSandbox ItemStat.", snapshot.manaLoopPass, "BattleSandboxManaLoopRuntime preview validator.", $"gainRows={snapshot.manaLoopGainRowCount}; spendRows={snapshot.manaLoopSpendRowCount}; generated={snapshot.manaLoopGeneratedManaTotal}; spent={snapshot.manaLoopSpentManaTotal}; layoutWrites={snapshot.manaLoopUiLayoutWriteCount}", true, "User should confirm ManaText, mana bar fill, and mana floating text update after continuing battle."),
                Row("REG-07", "Dev Balance", "3-10 and 4-10 devOnly balance recommendations are generated and readable.", snapshot.devChapterBalancePass, "DevChapterBalanceRun validator.", $"3-10={snapshot.devBalance310StageCount}; 4-10={snapshot.devBalance410StageCount}", false, "Covers over-easy, fit, and over-hard buckets."),
                Row("REG-08", "Player Leak", "Player UI does not leak complete answer fields or forbidden solution keys.", snapshot.playerLeakPass, "Grid text leak scan + BuildCombatPreview + ShapeBuildRule + DevBalance counters.", $"playerLeaks={snapshot.playerSideAnswerLeakCount}", true, "User should confirm player-visible text remains masked and Chinese-only."),
                Row("REG-09", "Feature Flags", "All BuildSandbox feature flags remain default false.", snapshot.featureFlagsDefaultFalsePass, "BuildSandboxFeatureFlags.All scan.", $"defaultTrue={snapshot.featureFlagDefaultTrueCount}", false, string.Empty),
                Row("REG-10", "devOnly", "All checked sandbox surfaces stay devOnly=true and isEnabled=false.", snapshot.devOnlyDisabledPass, "Scene controller + combat preview + shape preview + dev balance flags.", $"devOnlyFalse={snapshot.devOnlyFalseCount}; isEnabledTrue={snapshot.isEnabledTrueCount}", false, string.Empty),
                Row("REG-11", "Formal Scope", "No formal run flow, save, reward, drop, chapter, or settings write is present.", snapshot.formalScopePass, "Formal leak counters and Build Settings scan.", $"formalLeaks={snapshot.formalFlowLeakCount}", false, string.Empty),
                Row("REG-12", "Scene/Layout Scope", "No formal scene or current V04 hand-tuned layout write is required by regression.", snapshot.noFormalSceneOrLayoutWritePass, "No scene binder is executed; balance run scene/layout touch flags are false.", "formalSceneTouched=false; currentLayoutWrite=false", false, "Git status should remain free of V02/V03 scene changes."),
                Row("REG-13", "Console", "Regression validator has zero Error and zero Warning issues.", snapshot.errorCount == 0 && snapshot.warningCount == 0, "Aggregated validator issue counts.", $"errors={snapshot.errorCount}; warnings={snapshot.warningCount}", true, "User Play-mode Console should also stay clear.")
            };
        }

        private static BuildSandboxPlayableRegressionChecklistRow Row(
            string id,
            string area,
            string check,
            bool pass,
            string method,
            string evidence,
            bool userHandtestRequired,
            string notes)
        {
            return new BuildSandboxPlayableRegressionChecklistRow
            {
                id = id,
                area = area,
                check = check,
                method = method,
                result = pass ? "PASS" : "FAIL",
                evidence = evidence,
                userHandtestRequired = userHandtestRequired,
                notes = notes ?? string.Empty
            };
        }

        private static int CountTrayItemsByCellCount(
            IReadOnlyList<BuildGridInteractionPreviewController.PreviewItem> items,
            IReadOnlyDictionary<string, ItemShapeConfig> shapeById,
            int cellCount)
        {
            return (items ?? Array.Empty<BuildGridInteractionPreviewController.PreviewItem>())
                .Count(item => item != null
                    && shapeById != null
                    && shapeById.TryGetValue(item.ShapeId, out ItemShapeConfig shape)
                    && shape.cellCount == cellCount);
        }

        private static int CountCombatRows(
            BattleSandboxBuildCombatPreview preview,
            string feedbackKind)
        {
            return preview?.rows?.Count(row =>
                row != null
                && string.Equals(row.feedbackKind, feedbackKind, StringComparison.Ordinal)) ?? 0;
        }

        private static bool ScanRotationScope()
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string fullPath = Path.Combine(
                projectRoot,
                GridControllerSourcePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(fullPath))
            {
                return false;
            }

            string text = File.ReadAllText(fullPath);
            return text.IndexOf("RotateSelectedItem", StringComparison.Ordinal) >= 0
                && text.IndexOf("RotateTrayItem", StringComparison.Ordinal) >= 0
                && text.IndexOf("TryCommitTrayRotation", StringComparison.Ordinal) >= 0
                && text.IndexOf("拖动中、预览中和棋盘上均禁止旋转", StringComparison.Ordinal) >= 0
                && text.IndexOf("该道具已经放到棋盘上；取消后才可重新旋转", StringComparison.Ordinal) >= 0;
        }

        private static void RequireTrue(
            BuildSandboxValidationReport report,
            string code,
            bool value,
            string message)
        {
            if (value)
            {
                report.AddInfo(code, message, PackageName);
                return;
            }

            report.AddError(code, message, PackageName);
        }

        private static int CountTrue(params bool[] values)
        {
            return values.Count(value => value);
        }

        private static int CountFalse(params bool[] values)
        {
            return values.Count(value => !value);
        }
    }
}
#endif
