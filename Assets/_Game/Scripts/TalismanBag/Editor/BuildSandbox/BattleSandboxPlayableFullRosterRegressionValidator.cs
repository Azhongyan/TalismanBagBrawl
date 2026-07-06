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
    public static class BattleSandboxPlayableFullRosterRegressionValidator
    {
        public const string PackageName = BattleSandboxPlayableFullRosterRegression.PackageName;
        public const string QaMenuPath =
            "Tools/Talisman Bag/V0.4/BuildSandbox/BattleSandboxPlayableFullRosterRegression01/[QA Only] Run Full Roster Regression";

        private const string GridControllerSourcePath =
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs";
        private const string RuntimeLoopSourcePath =
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BattleSandboxRuntimeLoop.cs";

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
            BattleSandboxPlayableFullRosterRegressionSnapshot snapshot = BuildRegressionSnapshot(reports);
            string[] reportPaths =
                BattleSandboxPlayableFullRosterRegressionReportWriter.WriteReports(reports, snapshot);

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
                $"[BuildSandbox-BattleSandboxPlayableFullRosterRegression01] completed status={(snapshot.Passed ? "PASS" : "FAIL")}, errors={snapshot.errorCount}, warnings={snapshot.warningCount}, reports={string.Join(", ", reportPaths)}");

            if (!snapshot.Passed && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BattleSandbox PlayableFullRosterRegression01 failed. See {string.Join(", ", reportPaths)}");
            }

            return snapshot.Passed;
        }

        public static List<BuildSandboxValidationReport> BuildValidationReports()
        {
            List<BuildSandboxValidationReport> reports = new();
            reports.AddRange(BuildGridInteractionPreviewValidator.BuildValidationReports());
            reports.AddRange(BattleSandboxRuntimeLoopValidator.BuildValidationReports());
            reports.Add(Validate());
            return reports;
        }

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report = new("BattleSandbox Playable Full Roster Regression 01");
            BattleSandboxPlayableFullRosterRegressionSnapshot snapshot =
                BuildRegressionSnapshot(Array.Empty<BuildSandboxValidationReport>());
            ValidateSnapshotEvidence(report, snapshot);
            return report;
        }

        public static BattleSandboxPlayableFullRosterRegressionSnapshot BuildRegressionSnapshot(
            IReadOnlyList<BuildSandboxValidationReport> reports)
        {
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();
            BuildGridInteractionSceneBindingSnapshot sceneSnapshot =
                BuildGridInteractionPreviewValidator.BuildSceneBindingSnapshot();
            IReadOnlyList<BuildGridInteractionPreviewController.PreviewItem> trayItems =
                BuildGridInteractionPreviewController.CreatePreviewItems();
            Dictionary<string, ItemShapeConfig> shapeById = BuildGridInteractionPreviewController
                .CreatePreviewShapeConfigs()
                .ToDictionary(shape => shape.shapeId, StringComparer.Ordinal);
            IReadOnlyList<BuildSandboxLegacyAndAdvancedItemRosterRow> roster =
                BuildSandboxLegacyAndAdvancedItemRosterCatalog.AllItems;
            HashSet<string> trayItemIds = new(
                trayItems
                    .Where(item => item != null)
                    .Select(item => item.ItemId),
                StringComparer.Ordinal);
            Dictionary<string, bool> trayPackResults = BuildTrayPackResults(trayItems, shapeById);
            List<BattleSandboxPlayableFullRosterItemRegressionRow> itemRows =
                BuildItemRows(roster, trayItemIds, shapeById, trayPackResults);

            BattleSandboxRuntimeLoopPreview defaultPreview =
                BattleSandboxRuntimeLoopValidator.BuildDefaultPreview();
            BattleSandboxRuntimeLoopPreview emptyBoardPreview =
                BattleSandboxRuntimeLoopPreviewBuilder.Build(
                    new BuildSandboxLayoutSnapshot(),
                    "full_roster_empty_board_defeat",
                    BuildDefeatScenario("full_roster_empty_board_defeat"));
            BuildSandboxLayoutSnapshot attackSnapshot = BuildAttackSampleSnapshot(shapeById);
            BattleSandboxRuntimeLoopPreview attackPreview =
                BattleSandboxRuntimeLoopPreviewBuilder.Build(
                    attackSnapshot,
                    "full_roster_attack_item_damage",
                    BuildDefeatScenario("full_roster_attack_item_damage"));
            BattleSandboxRuntimeLoopPreview shieldPreview =
                BattleSandboxRuntimeLoopPreviewBuilder.Build(
                    BuildSingleItemSnapshot("shield_talisman_basic", shapeById),
                    "full_roster_shield_first_damage",
                    BuildDefeatScenario("full_roster_shield_first_damage"));
            BattleSandboxRuntimeLoopPreview victoryPreview =
                BattleSandboxRuntimeLoopPreviewBuilder.Build(
                    attackSnapshot,
                    "full_roster_sandbox_victory",
                    BuildVictoryScenario("full_roster_sandbox_victory"));
            IReadOnlyList<BattleSandboxRuntimeLoopScenario> scenarios =
                BattleSandboxRuntimeLoopPreviewBuilder.BuildDevEnemyScenarios();
            List<BattleSandboxRuntimeLoopPreview> switchPreviews = BuildSwitchTargetPreviews(
                attackSnapshot,
                scenarios);

            BattleSandboxPlayableFullRosterRegressionSnapshot snapshot = new()
            {
                devOnly = true,
                isEnabled = false,
                nestedReportCount = safeReports.Count,
                errorCount = safeReports.Sum(report => report.ErrorCount),
                warningCount = safeReports.Sum(report => report.WarningCount),
                sceneExists = sceneSnapshot.SceneExists,
                rosterItemCount = roster.Count,
                trayItemCount = trayItems.Count,
                trayPackedItemCount = trayPackResults.Count(pair => pair.Value),
                missingTrayItemCount = roster.Count(row => row != null && !trayItemIds.Contains(row.ItemId)),
                trayPackFailureCount = trayPackResults.Count(pair => !pair.Value),
                basicItemCount = itemRows.Count(row => row.basicDefaultSingle1 || IsBasicItemId(row.itemId)),
                basicSingle1ItemCount = itemRows.Count(row => row.basicDefaultSingle1),
                basicPlacementValidCount = itemRows.Count(row => row.basicDefaultSingle1 && row.boardPlacementValid),
                x2PlacementValidCount = itemRows.Count(row => row.shapeCellCount == 2 && row.boardPlacementValid),
                x3PlacementValidCount = itemRows.Count(row => row.shapeCellCount == 3 && row.boardPlacementValid),
                x4PlacementValidCount = itemRows.Count(row => row.shapeCellCount == 4 && row.boardPlacementValid),
                vertical3PlacementValidCount = itemRows.Count(row =>
                    string.Equals(row.shapeId, "vertical_3", StringComparison.Ordinal)
                    && row.boardPlacementValid),
                multiCellPlacementFailureCount = itemRows.Count(row => row.shapeCellCount > 1 && !row.boardPlacementValid),
                emptyBoardEnemyHpInitial = emptyBoardPreview.initialEnemyHp,
                emptyBoardEnemyHpFinal = emptyBoardPreview.finalEnemyHp,
                emptyBoardPlayerHpInitial = emptyBoardPreview.initialPlayerHp,
                emptyBoardPlayerHpFinal = emptyBoardPreview.finalPlayerHp,
                emptyBoardPlayerHpRows = emptyBoardPreview.PlayerHpRowCount,
                emptyBoardDefeatRows = emptyBoardPreview.SandboxDefeatResultRowCount,
                attackCandidateCount = itemRows.Count(row => row.expectedAttackDamage),
                attackDamageItemCount = itemRows.Count(row => row.expectedAttackDamage && row.enemyHpDamageTotal > 0),
                attackEnemyHpDamageTotal =
                    attackPreview.playerItemEnemyHpDamageTotal
                    + itemRows.Where(row => row.expectedAttackDamage).Sum(row => row.enemyHpDamageTotal),
                shieldAttackRowCount = shieldPreview.EnemyAttackRowCount,
                shieldFirstAttackRowCount = CountShieldFirstAttackRows(shieldPreview),
                supportNoDamageCandidateCount = itemRows.Count(row => row.expectedNoDamageSupport),
                supportDamageLeakCount = itemRows.Count(row =>
                    row.expectedNoDamageSupport
                    && (row.enemyHpDamageTotal > 0 || row.finalEnemyHp != row.initialEnemyHp)),
                sandboxVictoryRows = victoryPreview.SandboxVictoryResultRowCount,
                sandboxDefeatRows = emptyBoardPreview.SandboxDefeatResultRowCount,
                devEnemyScenarioCount = scenarios.Count,
                switchTargetPlayablePreviewCount = switchPreviews.Count(preview =>
                    preview != null && preview.SandboxResultRowCount > 0),
                playerSideAnswerLeakCount =
                    sceneSnapshot.PlayerTextLatinViolations.Count
                    + sceneSnapshot.ForbiddenAnswerTextViolations.Count
                    + defaultPreview.PlayerSideAnswerLeakCount
                    + emptyBoardPreview.PlayerSideAnswerLeakCount
                    + attackPreview.PlayerSideAnswerLeakCount
                    + shieldPreview.PlayerSideAnswerLeakCount
                    + victoryPreview.PlayerSideAnswerLeakCount
                    + switchPreviews.Sum(preview => preview?.PlayerSideAnswerLeakCount ?? 1)
                    + itemRows.Sum(row => row.playerSideAnswerLeakCount),
                formalFlowLeakCount =
                    defaultPreview.FormalLeakCount
                    + emptyBoardPreview.FormalLeakCount
                    + attackPreview.FormalLeakCount
                    + shieldPreview.FormalLeakCount
                    + victoryPreview.FormalLeakCount
                    + switchPreviews.Sum(preview => preview?.FormalLeakCount ?? 1)
                    + itemRows.Sum(row => row.formalFlowLeakCount)
                    + CountTrue(
                        sceneSnapshot.ControllerReadsFormalSave,
                        sceneSnapshot.ControllerWritesFormalFlow,
                        sceneSnapshot.ControllerWritesFormalUi,
                        sceneSnapshot.ControllerTouchesFormalScene,
                        sceneSnapshot.BuildSettingsContainsPreview),
                featureFlagDefaultTrueCount = BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue),
                devOnlyFalseCount = CountFalse(
                    sceneSnapshot.ControllerDevOnly,
                    defaultPreview.devOnly,
                    emptyBoardPreview.devOnly,
                    attackPreview.devOnly,
                    shieldPreview.devOnly,
                    victoryPreview.devOnly)
                    + switchPreviews.Count(preview => preview?.devOnly != true),
                isEnabledTrueCount = CountTrue(
                    sceneSnapshot.ControllerIsEnabled,
                    defaultPreview.isEnabled,
                    emptyBoardPreview.isEnabled,
                    attackPreview.isEnabled,
                    shieldPreview.isEnabled,
                    victoryPreview.isEnabled)
                    + switchPreviews.Count(preview => preview?.isEnabled == true),
                itemRows = itemRows
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
            snapshot.fullRosterTrayCoveragePass =
                snapshot.rosterItemCount == BattleSandboxPlayableFullRosterRegression.ExpectedRosterItemCount
                && snapshot.trayItemCount == snapshot.rosterItemCount
                && snapshot.missingTrayItemCount == 0
                && snapshot.trayPackFailureCount == 0
                && snapshot.trayPackedItemCount == snapshot.rosterItemCount;
            snapshot.basicSinglePlacementPass =
                snapshot.basicItemCount > 0
                && snapshot.basicSingle1ItemCount == snapshot.basicItemCount
                && snapshot.basicPlacementValidCount == snapshot.basicItemCount;
            snapshot.multiShapePlacementPass =
                snapshot.x2PlacementValidCount > 0
                && snapshot.x3PlacementValidCount > 0
                && snapshot.x4PlacementValidCount > 0
                && snapshot.vertical3PlacementValidCount > 0
                && snapshot.multiCellPlacementFailureCount == 0;
            snapshot.emptyBoardDefeatPass =
                emptyBoardPreview.currentBoardPlacedItemCount == 0
                && emptyBoardPreview.EnemyHpRowCount == 0
                && emptyBoardPreview.playerItemEnemyHpDamageTotal == 0
                && emptyBoardPreview.finalEnemyHp == emptyBoardPreview.initialEnemyHp
                && emptyBoardPreview.PlayerHpRowCount > 0
                && emptyBoardPreview.finalPlayerHp == 0
                && emptyBoardPreview.SandboxDefeatResultRowCount > 0
                && emptyBoardPreview.SandboxVictoryResultRowCount == 0;
            snapshot.attackItemDamagePass =
                snapshot.attackCandidateCount > 0
                && snapshot.attackDamageItemCount > 0
                && snapshot.attackEnemyHpDamageTotal > 0
                && attackPreview.finalEnemyHp < attackPreview.initialEnemyHp;
            snapshot.shieldDamageOrderPass =
                snapshot.shieldAttackRowCount > 0
                && snapshot.shieldFirstAttackRowCount > 0;
            snapshot.supportNoDamagePass =
                snapshot.supportNoDamageCandidateCount > 0
                && snapshot.supportDamageLeakCount == 0;
            snapshot.sandboxResultPass =
                snapshot.sandboxVictoryRows > 0
                && snapshot.sandboxDefeatRows > 0;
            snapshot.restartAndSwitchTargetPass =
                ScanRestartSwitchTargetSource()
                && snapshot.devEnemyScenarioCount >= 2
                && snapshot.switchTargetPlayablePreviewCount >= 2
                && switchPreviews
                    .Select(preview => preview?.selectedDevEnemyStageId ?? string.Empty)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.Ordinal)
                    .Count() >= 2;
            snapshot.playerLeakPass = snapshot.playerSideAnswerLeakCount == 0;
            snapshot.formalScopePass = snapshot.formalFlowLeakCount == 0;
            snapshot.noFormalSceneOrLayoutWritePass =
                !sceneSnapshot.BuildSettingsContainsPreview
                && !sceneSnapshot.ControllerWritesFormalUi
                && !sceneSnapshot.ControllerTouchesFormalScene
                && defaultPreview.UiLayoutWriteCount == 0
                && emptyBoardPreview.UiLayoutWriteCount == 0
                && attackPreview.UiLayoutWriteCount == 0
                && shieldPreview.UiLayoutWriteCount == 0
                && victoryPreview.UiLayoutWriteCount == 0
                && switchPreviews.All(preview => preview != null && preview.UiLayoutWriteCount == 0);
            snapshot.featureFlagsDefaultFalsePass =
                snapshot.featureFlagDefaultTrueCount == 0
                && BuildSandboxFeatureFlags.AreAllDefaultsDisabled();
            snapshot.devOnlyDisabledPass =
                snapshot.devOnlyFalseCount == 0
                && snapshot.isEnabledTrueCount == 0;
            snapshot.checklistRows = BuildChecklistRows(snapshot);
            return snapshot;
        }

        private static void ValidateSnapshotEvidence(
            BuildSandboxValidationReport report,
            BattleSandboxPlayableFullRosterRegressionSnapshot snapshot)
        {
            RequireTrue(report, "FULL_ROSTER_SCENE_READY", snapshot.sceneExists && snapshot.sceneBindingPass, "V04 battle sandbox preview scene has required bindings.");
            RequireTrue(report, "FULL_ROSTER_TRAY_23_READY", snapshot.fullRosterTrayCoveragePass, "All 23 roster items appear and pack into the V04 item tray.");
            RequireTrue(report, "FULL_ROSTER_BASIC_1X1_READY", snapshot.basicSinglePlacementPass, "Basic items default to Single1 and have legal board placements.");
            RequireTrue(report, "FULL_ROSTER_MULTI_SHAPES_READY", snapshot.multiShapePlacementPass, "x2, x3, x4, and vertical_3 items have legal placements.");
            RequireTrue(report, "FULL_ROSTER_EMPTY_BOARD_DEFEAT_READY", snapshot.emptyBoardDefeatPass, "Empty board keeps enemy HP unchanged, damages player, and ends in sandbox defeat.");
            RequireTrue(report, "FULL_ROSTER_ATTACK_DAMAGE_READY", snapshot.attackItemDamagePass, "Attack item sample reduces enemy HP.");
            RequireTrue(report, "FULL_ROSTER_SHIELD_FIRST_READY", snapshot.shieldDamageOrderPass, "Enemy attacks consume player shield before HP.");
            RequireTrue(report, "FULL_ROSTER_SUPPORT_NO_DAMAGE_READY", snapshot.supportNoDamagePass, "Support/heal/cleanse/control/aura/rhythm rows do not damage enemy HP.");
            RequireTrue(report, "FULL_ROSTER_RESULT_READY", snapshot.sandboxResultPass, "Sandbox victory and defeat result rows are both present.");
            RequireTrue(report, "FULL_ROSTER_RESTART_SWITCH_READY", snapshot.restartAndSwitchTargetPass, "Restart and switch-target paths can produce another runtime preview.");
            RequireTrue(report, "FULL_ROSTER_PLAYER_LEAK_CLEAR", snapshot.playerLeakPass, "Player-side answer leak counters are zero.");
            RequireTrue(report, "FULL_ROSTER_FORMAL_SCOPE_CLEAR", snapshot.formalScopePass && snapshot.noFormalSceneOrLayoutWritePass, "No SaveData, Reward, Chapter, RunFlow, scene, or layout write scope is used.");
            RequireTrue(report, "FULL_ROSTER_FLAGS_FALSE", snapshot.featureFlagsDefaultFalsePass, "All BuildSandbox feature flags default false.");
            RequireTrue(report, "FULL_ROSTER_DEVONLY_DISABLED", snapshot.devOnlyDisabledPass, "Sandbox surfaces remain devOnly and disabled by default.");
        }

        private static List<BattleSandboxPlayableFullRosterItemRegressionRow> BuildItemRows(
            IReadOnlyList<BuildSandboxLegacyAndAdvancedItemRosterRow> roster,
            ISet<string> trayItemIds,
            IReadOnlyDictionary<string, ItemShapeConfig> shapeById,
            IReadOnlyDictionary<string, bool> trayPackResults)
        {
            HashSet<string> attackIds = new(
                BattleSandboxPlayableFullRosterRegression.ExpectedAttackDamageItemIds,
                StringComparer.Ordinal);
            HashSet<string> supportIds = new(
                BattleSandboxPlayableFullRosterRegression.ExpectedNoDamageSupportItemIds,
                StringComparer.Ordinal);
            List<BattleSandboxPlayableFullRosterItemRegressionRow> rows = new();
            foreach (BuildSandboxLegacyAndAdvancedItemRosterRow rosterRow in roster ?? Array.Empty<BuildSandboxLegacyAndAdvancedItemRosterRow>())
            {
                if (rosterRow == null)
                {
                    continue;
                }

                shapeById.TryGetValue(rosterRow.ShapeId, out ItemShapeConfig shape);
                BuildSandboxItemStat stat = BuildSandboxItemStatCatalog.Resolve(rosterRow.ItemId);
                ShapePlacementResult boardPlacement = shape == null
                    ? null
                    : new GridOccupancyMap(
                            BuildGridInteractionPreviewController.BoardColumns,
                            BuildGridInteractionPreviewController.BoardRows)
                        .TryPlace(
                            new ItemShapePlacementRequest(
                                rosterRow.ItemId,
                                rosterRow.ShapeId,
                                new ItemShapeCell(0, 0)),
                            shape);
                BattleSandboxRuntimeLoopPreview runtimePreview =
                BattleSandboxRuntimeLoopPreviewBuilder.Build(
                    BuildSingleItemSnapshot(rosterRow.ItemId, shapeById),
                    "full_roster_item_" + rosterRow.ItemId,
                    BuildItemScenario("full_roster_item_" + rosterRow.ItemId));

                rows.Add(new BattleSandboxPlayableFullRosterItemRegressionRow
                {
                    itemId = rosterRow.ItemId,
                    shapeId = rosterRow.ShapeId,
                    categoryId = rosterRow.PrimaryCategoryId,
                    statProfileId = stat.statProfileId,
                    trayPresent = trayItemIds != null && trayItemIds.Contains(rosterRow.ItemId),
                    trayPacked = trayPackResults != null
                        && trayPackResults.TryGetValue(rosterRow.ItemId, out bool packed)
                        && packed,
                    boardPlacementValid = boardPlacement?.IsValid == true,
                    basicDefaultSingle1 =
                        IsBasicItemId(rosterRow.ItemId)
                        && string.Equals(rosterRow.ShapeId, "Single1", StringComparison.Ordinal)
                        && (shape?.cellCount ?? 0) == 1,
                    expectedAttackDamage = attackIds.Contains(rosterRow.ItemId),
                    expectedNoDamageSupport = supportIds.Contains(rosterRow.ItemId),
                    shapeCellCount = shape?.cellCount ?? 0,
                    enemyHpDamageTotal = runtimePreview.playerItemEnemyHpDamageTotal,
                    initialEnemyHp = runtimePreview.initialEnemyHp,
                    finalEnemyHp = runtimePreview.finalEnemyHp,
                    playerSideAnswerLeakCount = runtimePreview.PlayerSideAnswerLeakCount,
                    formalFlowLeakCount = runtimePreview.FormalLeakCount
                });
            }

            return rows;
        }

        private static Dictionary<string, bool> BuildTrayPackResults(
            IReadOnlyList<BuildGridInteractionPreviewController.PreviewItem> trayItems,
            IReadOnlyDictionary<string, ItemShapeConfig> shapeById)
        {
            Dictionary<string, bool> results = new(StringComparer.Ordinal);
            ShapeAwareItemTrayGrid trayGrid = new(
                "full_roster_regression_tray_pack",
                BuildGridInteractionPreviewController.TrayColumns,
                BuildGridInteractionPreviewController.TrayColumns * BuildGridInteractionPreviewController.TrayRows,
                commitAllowed: true);
            foreach (BuildGridInteractionPreviewController.PreviewItem item in trayItems
                         ?? Array.Empty<BuildGridInteractionPreviewController.PreviewItem>())
            {
                if (item == null
                    || shapeById == null
                    || !shapeById.TryGetValue(item.ShapeId, out ItemShapeConfig shape))
                {
                    continue;
                }

                ShapeItemPayload payload = new(
                    item.ItemId,
                    item.ShapeId,
                    ItemShapeRotation.Rotation0,
                    shape.occupiedOffsets,
                    ShapePlacementSource.Tray);
                results[item.ItemId] = trayGrid.TryPack(payload, out ShapePlacementResult result)
                    && result != null
                    && result.IsValid;
            }

            return results;
        }

        private static BuildSandboxLayoutSnapshot BuildAttackSampleSnapshot(
            IReadOnlyDictionary<string, ItemShapeConfig> shapeById)
        {
            BuildSandboxLayoutSnapshot snapshot = new();
            snapshot.placedItems.Add(BattleSandboxBuildCombatPreviewBuilder.CreatePlacedItemSnapshot(
                "preview_fire_talisman",
                "Single1",
                ItemShapeRotation.Rotation0,
                new[] { new ItemShapeCell(0, 0) }));
            snapshot.placedItems.Add(BattleSandboxBuildCombatPreviewBuilder.CreatePlacedItemSnapshot(
                "preview_energy_incense",
                "Vertical2",
                ItemShapeRotation.Rotation0,
                new[] { new ItemShapeCell(1, 0), new ItemShapeCell(1, 1) }));
            BattleSandboxBuildCombatPreviewBuilder.ApplyPreviewEnergyLinks(snapshot);
            return snapshot;
        }

        private static BuildSandboxLayoutSnapshot BuildSingleItemSnapshot(
            string itemId,
            IReadOnlyDictionary<string, ItemShapeConfig> shapeById)
        {
            BuildSandboxLegacyAndAdvancedItemRosterRow rosterRow =
                BuildSandboxLegacyAndAdvancedItemRosterCatalog.Resolve(itemId);
            IReadOnlyList<ItemShapeCell> cells = Array.Empty<ItemShapeCell>();
            if (shapeById != null && shapeById.TryGetValue(rosterRow.ShapeId, out ItemShapeConfig shape))
            {
                cells = ItemShapePlacementValidator.CalculateOccupiedCells(
                    shape,
                    new ItemShapeCell(0, 0),
                    ItemShapeRotation.Rotation0);
            }

            BuildSandboxLayoutSnapshot snapshot = new();
            snapshot.placedItems.Add(BattleSandboxBuildCombatPreviewBuilder.CreatePlacedItemSnapshot(
                rosterRow.ItemId,
                rosterRow.ShapeId,
                ItemShapeRotation.Rotation0,
                cells));
            BattleSandboxBuildCombatPreviewBuilder.ApplyPreviewEnergyLinks(snapshot);
            return snapshot;
        }

        private static BattleSandboxRuntimeLoopScenario BuildDefeatScenario(string stageId)
        {
            return new BattleSandboxRuntimeLoopScenario
            {
                stageId = stageId ?? string.Empty,
                devChapterLabel = "V0.4",
                previewBuildId = stageId ?? string.Empty,
                enemyDisplayNameChinese = "FullRosterRegression",
                simulatedWinRate = 0.1f,
                expectsSandboxVictory = false,
                devOnlyProfileId = "dev_boss_full_roster_regression",
                attackSourcePath = "EnemyBossValidationPool.bosses[].attackDamage",
                attackDamage = 55,
                attackIntervalSeconds = 0.75f,
                attackFromDevOnlyProfile = true
            };
        }

        private static BattleSandboxRuntimeLoopScenario BuildItemScenario(string stageId)
        {
            BattleSandboxRuntimeLoopScenario scenario = BuildDefeatScenario(stageId);
            scenario.attackDamage = 16;
            scenario.attackIntervalSeconds = 2.4f;
            return scenario;
        }

        private static BattleSandboxRuntimeLoopScenario BuildVictoryScenario(string stageId)
        {
            BattleSandboxRuntimeLoopScenario scenario = BuildDefeatScenario(stageId);
            scenario.simulatedWinRate = 0.9f;
            scenario.expectsSandboxVictory = true;
            scenario.attackDamage = 12;
            scenario.attackIntervalSeconds = 2.4f;
            return scenario;
        }

        private static List<BattleSandboxRuntimeLoopPreview> BuildSwitchTargetPreviews(
            BuildSandboxLayoutSnapshot snapshot,
            IReadOnlyList<BattleSandboxRuntimeLoopScenario> scenarios)
        {
            List<BattleSandboxRuntimeLoopPreview> previews = new();
            foreach (BattleSandboxRuntimeLoopScenario scenario in (scenarios ?? Array.Empty<BattleSandboxRuntimeLoopScenario>()).Take(2))
            {
                previews.Add(BattleSandboxRuntimeLoopPreviewBuilder.Build(
                    snapshot,
                    string.IsNullOrWhiteSpace(scenario?.previewBuildId)
                        ? "full_roster_switch_target"
                        : scenario.previewBuildId,
                    scenario));
            }

            return previews;
        }

        private static int CountShieldFirstAttackRows(BattleSandboxRuntimeLoopPreview preview)
        {
            return BattleSandboxRuntimeLoopValidator.Rows(preview).Count(row =>
                row != null
                && row.enemyAttackDamage > 0
                && row.playerShieldBefore > 0
                && row.playerShieldDamageAbsorbed > 0
                && row.playerShieldDamageResolvedFirst);
        }

        private static bool ScanRestartSwitchTargetSource()
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string gridPath = Path.Combine(
                projectRoot,
                GridControllerSourcePath.Replace('/', Path.DirectorySeparatorChar));
            string runtimePath = Path.Combine(
                projectRoot,
                RuntimeLoopSourcePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(gridPath) || !File.Exists(runtimePath))
            {
                return false;
            }

            string gridText = File.ReadAllText(gridPath);
            string runtimeText = File.ReadAllText(runtimePath);
            return gridText.IndexOf("runtimeLoopRuntime.RestartLoop()", StringComparison.Ordinal) >= 0
                && gridText.IndexOf("runtimeLoopRuntime.SelectNextDevEnemy()", StringComparison.Ordinal) >= 0
                && runtimeText.IndexOf("public void RestartLoop()", StringComparison.Ordinal) >= 0
                && runtimeText.IndexOf("public string SelectNextDevEnemy()", StringComparison.Ordinal) >= 0
                && runtimeText.IndexOf("RestartLoop();", StringComparison.Ordinal) >= 0;
        }

        private static List<BattleSandboxPlayableFullRosterRegressionChecklistRow> BuildChecklistRows(
            BattleSandboxPlayableFullRosterRegressionSnapshot snapshot)
        {
            return new List<BattleSandboxPlayableFullRosterRegressionChecklistRow>
            {
                Row("FR-01", "Tray", "All 23 current roster items appear in the V04 tray.", snapshot.fullRosterTrayCoveragePass, "Roster catalog + preview tray creation + ShapeAwareItemTrayGrid pack scan.", $"roster={snapshot.rosterItemCount}; tray={snapshot.trayItemCount}; packed={snapshot.trayPackedItemCount}; missing={snapshot.missingTrayItemCount}", true, "User should visually page/filter the tray once in Play mode."),
                Row("FR-02", "Basic Items", "Basic items default to 1x1 and can be dragged to legal board cells.", snapshot.basicSinglePlacementPass, "Roster shape scan + board placement validator.", $"basic={snapshot.basicItemCount}; single1={snapshot.basicSingle1ItemCount}; placed={snapshot.basicPlacementValidCount}", true, "Try at least two basic items manually."),
                Row("FR-03", "Multi Shape", "x2, x3, x4, and vertical_3 items place stably.", snapshot.multiShapePlacementPass, "Board placement validator over full roster.", $"x2={snapshot.x2PlacementValidCount}; x3={snapshot.x3PlacementValidCount}; x4={snapshot.x4PlacementValidCount}; vertical_3={snapshot.vertical3PlacementValidCount}; multiFails={snapshot.multiCellPlacementFailureCount}", true, "Manual pass should include each listed footprint."),
                Row("FR-04", "Empty Board", "Empty board battle leaves enemy HP unchanged, damages player HP, and ends in defeat.", snapshot.emptyBoardDefeatPass, "Runtime loop preview built from an empty layout.", $"enemy={snapshot.emptyBoardEnemyHpInitial}->{snapshot.emptyBoardEnemyHpFinal}; player={snapshot.emptyBoardPlayerHpInitial}->{snapshot.emptyBoardPlayerHpFinal}; defeatRows={snapshot.emptyBoardDefeatRows}", true, string.Empty),
                Row("FR-05", "Attack Item", "An attack item battle reduces enemy HP.", snapshot.attackItemDamagePass, "Runtime loop preview with attack item + energy support.", $"attackCandidates={snapshot.attackCandidateCount}; attackDamageRows={snapshot.attackDamageItemCount}; totalDamage={snapshot.attackEnemyHpDamageTotal}", true, "Use an obvious attack item such as preview_fire_talisman or preview_thunder_sword."),
                Row("FR-06", "Shield", "Enemy attack consumes shield before HP.", snapshot.shieldDamageOrderPass, "Shield-item runtime loop enemy attack rows.", $"attackRows={snapshot.shieldAttackRowCount}; shieldFirstRows={snapshot.shieldFirstAttackRowCount}", true, string.Empty),
                Row("FR-07", "Support Damage", "Heal, cleanse, control, aura, shield, and rhythm support rows do not damage enemy HP.", snapshot.supportNoDamagePass, "Per-item runtime loop previews for expected non-damage support IDs.", $"supportCandidates={snapshot.supportNoDamageCandidateCount}; damageLeaks={snapshot.supportDamageLeakCount}", true, "Manual pass should include at least one heal/cleanse/control/energy item."),
                Row("FR-08", "Result Flow", "Sandbox victory, defeat, and restart result surfaces are present.", snapshot.sandboxResultPass, "Runtime loop victory and empty-board defeat previews.", $"victoryRows={snapshot.sandboxVictoryRows}; defeatRows={snapshot.sandboxDefeatRows}", true, string.Empty),
                Row("FR-09", "Switch Target", "Switching test target can produce another playable preview.", snapshot.restartAndSwitchTargetPass, "Dev enemy scenario scan + restart/switch source scan.", $"scenarios={snapshot.devEnemyScenarioCount}; playableSwitchPreviews={snapshot.switchTargetPlayablePreviewCount}", true, string.Empty),
                Row("FR-10", "Player Leak", "Player side does not show complete answers, DropBias weights, or Boss six-key answers.", snapshot.playerLeakPass, "Scene text scan + runtime/player leak counters.", $"playerLeaks={snapshot.playerSideAnswerLeakCount}", true, string.Empty),
                Row("FR-11", "Formal Scope", "No SaveData, Reward, Chapter, or formal RunFlow is written or advanced.", snapshot.formalScopePass && snapshot.noFormalSceneOrLayoutWritePass, "Runtime formal leak counters + scene/layout write scan.", $"formalLeaks={snapshot.formalFlowLeakCount}; featureFlagDefaultTrue={snapshot.featureFlagDefaultTrueCount}; devOnlyFalse={snapshot.devOnlyFalseCount}; isEnabledTrue={snapshot.isEnabledTrueCount}", false, "This package does not run scene binders.")
            };
        }

        private static BattleSandboxPlayableFullRosterRegressionChecklistRow Row(
            string id,
            string area,
            string check,
            bool pass,
            string method,
            string evidence,
            bool userHandtestRequired,
            string notes)
        {
            return new BattleSandboxPlayableFullRosterRegressionChecklistRow
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

        private static bool IsBasicItemId(string itemId)
        {
            return BuildSandboxItemIdentityFamilyCatalog.BasicItems
                .Any(row => row != null && string.Equals(row.ItemId, itemId, StringComparison.Ordinal));
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
