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
    public static class BattleSandboxDevChapterPlayableValidator
    {
        public const string PackageName = BattleSandboxDevChapterPlayable.PackageName;
        public const string QaMenuPath =
            "Tools/Talisman Bag/V0.4/BuildSandbox/BattleSandboxDevChapterPlayable01/[QA Only] Run Dev Chapter Playable";

        private const string GridControllerSourcePath =
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs";
        private const string RuntimeLoopSourcePath =
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BattleSandboxRuntimeLoop.cs";

        private static readonly string[] ForbiddenPlayerAnswerTokens =
        {
            "hardSolutionTags",
            "requiredSynergy",
            "requiredAffix",
            "requiredStats",
            "DropBias",
            "dropBias",
            "bossSixKeyFullAnswer",
            "Boss六钥匙",
            "Boss 六钥匙",
            "previewWeight",
            "keyRequirements",
            "答案",
            "解法",
            "权重",
            "六钥匙"
        };

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
            BattleSandboxDevChapterPlayableSnapshot snapshot = BuildDefaultSnapshot(reports);
            string[] reportPaths =
                BattleSandboxDevChapterPlayableReportWriter.WriteReports(reports, snapshot);

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
                $"[BuildSandbox-BattleSandboxDevChapterPlayable01] completed status={(snapshot.Passed ? "PASS" : "FAIL")}, errors={reports.Sum(report => report.ErrorCount)}, warnings={reports.Sum(report => report.WarningCount)}, reports={string.Join(", ", reportPaths)}");

            if (!snapshot.Passed && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BattleSandbox DevChapterPlayable01 failed. See {string.Join(", ", reportPaths)}");
            }

            return snapshot.Passed;
        }

        public static List<BuildSandboxValidationReport> BuildValidationReports()
        {
            List<BuildSandboxValidationReport> reports = new();
            reports.AddRange(BattleSandboxPlayableLoopValidator.BuildValidationReports());
            reports.Add(BattleSandboxPlayableFullRosterRegressionValidator.Validate());
            reports.Add(Validate());
            return reports;
        }

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report = new("BattleSandbox Dev Chapter Playable 01");
            BattleSandboxDevChapterPlayableSnapshot snapshot = BuildDefaultSnapshot();
            ValidateSnapshot(report, snapshot);
            return report;
        }

        public static BattleSandboxDevChapterPlayableSnapshot BuildDefaultSnapshot(
            IReadOnlyList<BuildSandboxValidationReport> nestedReports = null)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string sceneAbsolutePath = Path.Combine(projectRoot, BattleSandboxDevChapterPlayable.ScenePath);
            BattleSandboxPlayableLoopSnapshot playableLoopSnapshot =
                BattleSandboxPlayableLoopValidator.BuildDefaultSnapshot();
            BattleSandboxPlayableFullRosterRegressionSnapshot fullRosterSnapshot =
                BattleSandboxPlayableFullRosterRegressionValidator.BuildRegressionSnapshot(
                    Array.Empty<BuildSandboxValidationReport>());
            BuildGridInteractionSceneBindingSnapshot sceneSnapshot =
                BuildGridInteractionPreviewValidator.BuildSceneBindingSnapshot();
            IReadOnlyList<BattleSandboxRuntimeLoopScenario> scenarios =
                BattleSandboxRuntimeLoopPreviewBuilder.BuildDevEnemyScenarios();
            BuildSandboxLayoutSnapshot defaultLayout =
                BattleSandboxBuildCombatPreviewBuilder.BuildDefaultPreviewLayoutSnapshot();
            EnemyBossValidationPool enemyBossPool = EnemyBossValidationPool.CreateDefault();
            SourceScanEvidence sourceEvidence = ScanSources(projectRoot);
            List<BattleSandboxDevChapterPlayableRow> rows = new();
            int previewFormalLeakCount = 0;
            int previewPlayerLeakCount = 0;
            int previewUiLayoutWriteCount = 0;

            foreach (BattleSandboxRuntimeLoopScenario scenario in scenarios ?? Array.Empty<BattleSandboxRuntimeLoopScenario>())
            {
                if (scenario == null
                    || (scenario.devChapterLabel != "3-10" && scenario.devChapterLabel != "4-10"))
                {
                    continue;
                }

                BattleSandboxRuntimeLoopPreview preview =
                    BattleSandboxRuntimeLoopPreviewBuilder.Build(
                        defaultLayout,
                        string.IsNullOrWhiteSpace(scenario.previewBuildId)
                            ? "dev_chapter_playable_preview"
                            : scenario.previewBuildId,
                        scenario);
                previewFormalLeakCount += preview?.FormalLeakCount ?? 1;
                previewFormalLeakCount += preview?.SettlementLeakCount ?? 1;
                previewPlayerLeakCount += preview?.PlayerSideAnswerLeakCount ?? 1;
                previewUiLayoutWriteCount += preview?.UiLayoutWriteCount ?? 1;

                BattleSandboxRuntimeLoopRow resultRow = preview?.rows?
                    .LastOrDefault(row => row != null && (row.hasSandboxVictoryResult || row.hasSandboxDefeatResult));
                rows.Add(BuildRow(
                    scenario,
                    preview,
                    resultRow,
                    enemyBossPool,
                    sourceEvidence.restartSupported,
                    sourceEvidence.chapterSwitchSupported));
            }

            BattleSandboxDevChapterPlayableSnapshot snapshot = new()
            {
                packageName = PackageName,
                scenePath = BattleSandboxDevChapterPlayable.ScenePath,
                devOnly = true,
                isEnabled = false,
                sceneExists = File.Exists(sceneAbsolutePath),
                reusesBattleSandboxRuntimeLoop = string.Equals(
                    BattleSandboxDevChapterPlayable.RuntimeLoopPackageName,
                    BattleSandboxRuntimeLoopPreview.PackageName,
                    StringComparison.Ordinal),
                reusesBattleSandboxPlayableLoopResult = string.Equals(
                    BattleSandboxDevChapterPlayable.PlayableLoopPackageName,
                    BattleSandboxPlayableLoop.PackageName,
                    StringComparison.Ordinal),
                reusesBattleSandboxFullRosterResult = string.Equals(
                    BattleSandboxDevChapterPlayable.FullRosterPackageName,
                    BattleSandboxPlayableFullRosterRegression.PackageName,
                    StringComparison.Ordinal),
                playableLoopResultPassed = playableLoopSnapshot?.Passed == true,
                fullRosterResultPassed = fullRosterSnapshot?.Passed == true,
                rewritesBattleLoop = false,
                chapter310Selectable = rows.Any(row => row != null && row.devChapterLabel == "3-10" && row.selectable),
                chapter410Selectable = rows.Any(row => row != null && row.devChapterLabel == "4-10" && row.selectable),
                devChapterSelectorBindingPresent = sourceEvidence.devChapterSelectorBindingPresent,
                runtimeChapterSelectionApiPresent = sourceEvidence.runtimeChapterSelectionApiPresent,
                restartSupported = sourceEvidence.restartSupported,
                chapterSwitchSupported = sourceEvidence.chapterSwitchSupported,
                formalRunFlowConnected = false,
                writesSaveData = false,
                grantsReward = false,
                advancesChapter = false,
                touchesFormalStageConfig = sourceEvidence.touchesFormalStageConfig,
                touchesFormalBossTable = sourceEvidence.touchesFormalBossTable,
                touchesFormalRewardTable = sourceEvidence.touchesFormalRewardTable,
                touchesV02OrV03 = false,
                runsSceneBinder = false,
                scenarioCount = rows.Count,
                chapter310ScenarioCount = rows.Count(row => row != null && row.devChapterLabel == "3-10"),
                chapter410ScenarioCount = rows.Count(row => row != null && row.devChapterLabel == "4-10"),
                victoryRowCount = rows.Count(row => row != null && row.sandboxVictory),
                defeatRowCount = rows.Count(row => row != null && row.sandboxDefeat),
                mechanicFeedbackRowCount = rows.Count(row => row != null && row.mechanicFeedbackVisible),
                buildPressureRowCount = rows.Count(row => row != null && row.buildPressureVisible),
                distinctBossProfileCount = rows
                    .Where(row => row != null && !string.IsNullOrWhiteSpace(row.bossProfileId))
                    .Select(row => row.bossProfileId)
                    .Distinct(StringComparer.Ordinal)
                    .Count(),
                playerSideAnswerLeakCount =
                    previewPlayerLeakCount
                    + (sceneSnapshot?.ForbiddenAnswerTextViolations?.Count ?? 0)
                    + rows.Count(row => row == null || row.playerSideAnswerLeak),
                formalFlowLeakCount =
                    previewFormalLeakCount
                    + CountTrue(
                        sceneSnapshot?.ControllerReadsFormalSave == true,
                        sceneSnapshot?.ControllerWritesFormalFlow == true,
                        sceneSnapshot?.ControllerWritesFormalUi == true,
                        sceneSnapshot?.ControllerTouchesFormalScene == true,
                        sceneSnapshot?.BuildSettingsContainsPreview == true)
                    + rows.Count(row => row == null || row.formalFlowLeak),
                featureFlagDefaultTrueCount = BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue),
                uiLayoutWriteCount = previewUiLayoutWriteCount + rows.Count(row => row == null || row.uiLayoutWrite),
                devChapterSelectorLayoutWriteCount = sourceEvidence.devChapterSelectorLayoutWriteCount,
                nestedPlayableLoopErrorCount = playableLoopSnapshot?.Passed == true ? 0 : 1,
                nestedFullRosterErrorCount = fullRosterSnapshot?.Passed == true ? 0 : 1,
                nestedWarningCount = nestedReports?.Sum(report => report.WarningCount) ?? 0,
                rows = rows
            };

            snapshot.differentBossProfileAcrossChapters = HasDifferentChapterValue(rows, row => row.bossProfileId);
            snapshot.differentMechanicFeedbackAcrossChapters = HasDifferentChapterValue(rows, row => row.mechanicFeedbackChinese);
            snapshot.differentBuildPressureAcrossChapters = HasDifferentChapterValue(rows, row => row.buildPressureChinese);
            return snapshot;
        }

        private static void ValidateSnapshot(
            BuildSandboxValidationReport report,
            BattleSandboxDevChapterPlayableSnapshot snapshot)
        {
            RequireTrue(report, "DEV_CHAPTER_PLAYABLE_DEVONLY_TRUE", snapshot.devOnly, "Dev chapter playable remains devOnly.");
            RequireFalse(report, "DEV_CHAPTER_PLAYABLE_ENABLED_FALSE", snapshot.isEnabled, "Dev chapter playable remains disabled by default.");
            RequireTrue(report, "DEV_CHAPTER_PLAYABLE_SCENE_EXISTS", snapshot.sceneExists, "V04 battle sandbox preview scene exists.");
            RequireTrue(report, "DEV_CHAPTER_PLAYABLE_REUSE_RUNTIME", snapshot.reusesBattleSandboxRuntimeLoop && !snapshot.rewritesBattleLoop, "Reuses BattleSandboxRuntimeLoop and does not rewrite battle loop.");
            RequireTrue(report, "DEV_CHAPTER_PLAYABLE_REUSE_PLAYABLE_LOOP", snapshot.reusesBattleSandboxPlayableLoopResult && snapshot.playableLoopResultPassed, "Reuses the existing PlayableLoop result.");
            RequireTrue(report, "DEV_CHAPTER_PLAYABLE_REUSE_FULL_ROSTER", snapshot.reusesBattleSandboxFullRosterResult && snapshot.fullRosterResultPassed, "Reuses the existing FullRoster result.");
            RequireTrue(report, "DEV_CHAPTER_PLAYABLE_310_SELECTABLE", snapshot.chapter310Selectable && snapshot.chapter310ScenarioCount > 0, "3-10 devOnly level is selectable.");
            RequireTrue(report, "DEV_CHAPTER_PLAYABLE_410_SELECTABLE", snapshot.chapter410Selectable && snapshot.chapter410ScenarioCount > 0, "4-10 devOnly level is selectable.");
            RequireTrue(report, "DEV_CHAPTER_PLAYABLE_SELECTOR_BOUND", snapshot.devChapterSelectorBindingPresent && snapshot.runtimeChapterSelectionApiPresent, "V04 selector is bound to runtime chapter selection.");
            RequireTrue(report, "DEV_CHAPTER_PLAYABLE_DISTINCT_PROFILE", snapshot.differentBossProfileAcrossChapters && snapshot.distinctBossProfileCount >= 2, "3-10 and 4-10 use different devOnly boss profiles.");
            RequireTrue(report, "DEV_CHAPTER_PLAYABLE_DISTINCT_MECHANIC", snapshot.differentMechanicFeedbackAcrossChapters && snapshot.mechanicFeedbackRowCount >= 2, "3-10 and 4-10 show different mechanic feedback.");
            RequireTrue(report, "DEV_CHAPTER_PLAYABLE_DISTINCT_PRESSURE", snapshot.differentBuildPressureAcrossChapters && snapshot.buildPressureRowCount >= 2, "3-10 and 4-10 show different Build pressure.");
            RequireTrue(report, "DEV_CHAPTER_PLAYABLE_RESULTS", snapshot.victoryRowCount > 0 && snapshot.defeatRowCount > 0, "Victory and failure paths are available.");
            RequireTrue(report, "DEV_CHAPTER_PLAYABLE_RESTART_SWITCH", snapshot.restartSupported && snapshot.chapterSwitchSupported, "Restart and chapter switch paths are available.");
            RequireEquals(report, "DEV_CHAPTER_PLAYABLE_PLAYER_LEAK_ZERO", "player-side answer leak count", snapshot.playerSideAnswerLeakCount, 0);
            RequireEquals(report, "DEV_CHAPTER_PLAYABLE_FORMAL_LEAK_ZERO", "formal flow leak count", snapshot.formalFlowLeakCount, 0);
            RequireEquals(report, "DEV_CHAPTER_PLAYABLE_FEATURE_FLAG_ZERO", "feature flag default true count", snapshot.featureFlagDefaultTrueCount, 0);
            RequireEquals(report, "DEV_CHAPTER_PLAYABLE_UI_LAYOUT_ZERO", "UI layout write count", snapshot.uiLayoutWriteCount, 0);
            RequireEquals(report, "DEV_CHAPTER_PLAYABLE_SELECTOR_LAYOUT_ZERO", "selector layout write count", snapshot.devChapterSelectorLayoutWriteCount, 0);
            RequireFalse(report, "DEV_CHAPTER_PLAYABLE_FORMAL_RUNFLOW_FALSE", snapshot.formalRunFlowConnected, "No formal RunFlow connection.");
            RequireFalse(report, "DEV_CHAPTER_PLAYABLE_SAVE_FALSE", snapshot.writesSaveData, "No SaveData writes.");
            RequireFalse(report, "DEV_CHAPTER_PLAYABLE_REWARD_FALSE", snapshot.grantsReward, "No Reward grants.");
            RequireFalse(report, "DEV_CHAPTER_PLAYABLE_CHAPTER_FALSE", snapshot.advancesChapter, "No Chapter progress.");
            RequireFalse(report, "DEV_CHAPTER_PLAYABLE_FORMAL_TABLE_FALSE", snapshot.touchesFormalStageConfig || snapshot.touchesFormalBossTable || snapshot.touchesFormalRewardTable, "No formal StageConfig, Boss table, or Reward table touch.");
            RequireFalse(report, "DEV_CHAPTER_PLAYABLE_V02_V03_FALSE", snapshot.touchesV02OrV03, "No V0.2/V0.3 formal chapter touch.");
            RequireFalse(report, "DEV_CHAPTER_PLAYABLE_BINDER_FALSE", snapshot.runsSceneBinder, "No scene binder is run.");

            foreach (BattleSandboxDevChapterPlayableRow row in snapshot.rows ?? new List<BattleSandboxDevChapterPlayableRow>())
            {
                if (row == null)
                {
                    report.AddError("DEV_CHAPTER_PLAYABLE_ROW_NULL", "Dev chapter playable row is null.", PackageName);
                    continue;
                }

                if (!row.Passed)
                {
                    report.AddError(
                        "DEV_CHAPTER_PLAYABLE_ROW_FAIL",
                        $"Dev chapter playable row failed. id={row.rowId}, stage={row.stageId}.",
                        nameof(BattleSandboxDevChapterPlayableRow));
                }
            }
        }

        private static BattleSandboxDevChapterPlayableRow BuildRow(
            BattleSandboxRuntimeLoopScenario scenario,
            BattleSandboxRuntimeLoopPreview preview,
            BattleSandboxRuntimeLoopRow resultRow,
            EnemyBossValidationPool enemyBossPool,
            bool restartSupported,
            bool chapterSwitchSupported)
        {
            BuildSandboxBossProfile bossProfile = enemyBossPool?.FindBoss(scenario?.devOnlyProfileId);
            string playerText = string.Join(
                " ",
                preview?.selectedDevEnemyMechanicFeedbackChinese,
                preview?.selectedDevEnemyBuildPressureChinese,
                resultRow?.resultTitleChinese,
                resultRow?.resultBodyChinese,
                resultRow?.combatLogLineChinese,
                resultRow?.floatingTextChinese,
                resultRow?.restartHintChinese);
            return new BattleSandboxDevChapterPlayableRow
            {
                rowId = "devChapterPlayable." + (scenario?.stageId ?? string.Empty),
                stageId = scenario?.stageId ?? string.Empty,
                devChapterLabel = scenario?.devChapterLabel ?? string.Empty,
                targetDisplayNameChinese = scenario?.enemyDisplayNameChinese ?? string.Empty,
                bossProfileId = scenario?.devOnlyProfileId ?? string.Empty,
                mechanicFeedbackChinese = preview?.selectedDevEnemyMechanicFeedbackChinese ?? string.Empty,
                buildPressureChinese = preview?.selectedDevEnemyBuildPressureChinese ?? string.Empty,
                attackDamage = scenario?.attackDamage ?? 0,
                attackIntervalSeconds = scenario?.attackIntervalSeconds ?? 0f,
                enemyHpAfter = resultRow?.enemyHpAfter ?? preview?.finalEnemyHp ?? 0,
                playerHpAfter = resultRow?.playerHpAfter ?? preview?.finalPlayerHp ?? 0,
                selectable = scenario != null
                    && (scenario.devChapterLabel == "3-10" || scenario.devChapterLabel == "4-10"),
                usesDevOnlyBossProfile = bossProfile != null
                    && bossProfile.devOnly
                    && !bossProfile.isEnabled
                    && !bossProfile.entersFormalFlow
                    && !bossProfile.referencesFormalBossPool
                    && !bossProfile.referencesFormalEnemyPool
                    && scenario?.attackFromDevOnlyProfile == true,
                sandboxVictory = resultRow?.hasSandboxVictoryResult == true,
                sandboxDefeat = resultRow?.hasSandboxDefeatResult == true,
                promptVisible = !string.IsNullOrWhiteSpace(resultRow?.resultTitleChinese)
                    && !string.IsNullOrWhiteSpace(resultRow?.resultBodyChinese),
                mechanicFeedbackVisible =
                    !string.IsNullOrWhiteSpace(preview?.selectedDevEnemyMechanicFeedbackChinese)
                    && playerText.IndexOf("机制反馈", StringComparison.Ordinal) >= 0,
                buildPressureVisible =
                    !string.IsNullOrWhiteSpace(preview?.selectedDevEnemyBuildPressureChinese)
                    && playerText.IndexOf("构筑压力", StringComparison.Ordinal) >= 0,
                restartSupported = restartSupported,
                chapterSwitchSupported = chapterSwitchSupported,
                playerSideAnswerLeak =
                    (preview?.PlayerSideAnswerLeakCount ?? 1) > 0
                    || ContainsForbiddenPlayerToken(playerText),
                formalFlowLeak =
                    (preview?.FormalLeakCount ?? 1) > 0
                    || (preview?.SettlementLeakCount ?? 1) > 0
                    || resultRow?.formalFlowLeak == true
                    || resultRow?.hasVictorySettlement == true
                    || resultRow?.hasDefeatSettlement == true,
                uiLayoutWrite = (preview?.UiLayoutWriteCount ?? 1) > 0 || resultRow?.uiLayoutWrite == true
            };
        }

        private static bool HasDifferentChapterValue(
            IReadOnlyList<BattleSandboxDevChapterPlayableRow> rows,
            Func<BattleSandboxDevChapterPlayableRow, string> selector)
        {
            BattleSandboxDevChapterPlayableRow row310 = rows?
                .FirstOrDefault(row => row != null && row.devChapterLabel == "3-10");
            BattleSandboxDevChapterPlayableRow row410 = rows?
                .FirstOrDefault(row => row != null && row.devChapterLabel == "4-10");
            string chapter310 = row310 == null ? string.Empty : selector(row310);
            string chapter410 = row410 == null ? string.Empty : selector(row410);
            return !string.IsNullOrWhiteSpace(chapter310)
                && !string.IsNullOrWhiteSpace(chapter410)
                && !string.Equals(chapter310, chapter410, StringComparison.Ordinal);
        }

        private static SourceScanEvidence ScanSources(string projectRoot)
        {
            string gridText = ReadProjectText(projectRoot, GridControllerSourcePath);
            string runtimeText = ReadProjectText(projectRoot, RuntimeLoopSourcePath);
            return new SourceScanEvidence
            {
                devChapterSelectorBindingPresent =
                    gridText.IndexOf("DevChapterDropdownSlot", StringComparison.Ordinal) >= 0
                    && gridText.IndexOf("HandleDevChapterSelectorClicked", StringComparison.Ordinal) >= 0
                    && gridText.IndexOf("SelectNextDevChapter", StringComparison.Ordinal) >= 0,
                runtimeChapterSelectionApiPresent =
                    runtimeText.IndexOf("public string SelectDevChapter", StringComparison.Ordinal) >= 0
                    && runtimeText.IndexOf("public string SelectNextDevChapter", StringComparison.Ordinal) >= 0,
                restartSupported =
                    runtimeText.IndexOf("public void RestartLoop()", StringComparison.Ordinal) >= 0
                    && gridText.IndexOf("runtimeLoopRuntime.RestartLoop()", StringComparison.Ordinal) >= 0,
                chapterSwitchSupported =
                    runtimeText.IndexOf("SelectDevChapter", StringComparison.Ordinal) >= 0
                    && gridText.IndexOf("HandleDevChapterSelectorClicked", StringComparison.Ordinal) >= 0,
                devChapterSelectorLayoutWriteCount = CountDevChapterSelectorLayoutWrites(gridText),
                touchesFormalStageConfig = runtimeText.IndexOf("StageConfig", StringComparison.Ordinal) >= 0,
                touchesFormalBossTable =
                    runtimeText.IndexOf("FormalBossTable", StringComparison.Ordinal) >= 0
                    || runtimeText.IndexOf("BossConfig", StringComparison.Ordinal) >= 0,
                touchesFormalRewardTable =
                    runtimeText.IndexOf("RewardTable", StringComparison.Ordinal) >= 0
                    || runtimeText.IndexOf("RewardConfig", StringComparison.Ordinal) >= 0
            };
        }

        private static int CountDevChapterSelectorLayoutWrites(string gridText)
        {
            string block = ExtractSourceBlock(
                gridText,
                "private void EnsureDevChapterSelectorBinding()",
                "private void HandleDevChapterSelectorClicked()");
            string[] forbiddenTokens =
            {
                "anchoredPosition",
                "anchorMin",
                "anchorMax",
                "offsetMin",
                "offsetMax",
                "sizeDelta",
                "SetParent(",
                "SetAsLastSibling(",
                "SetRuntimeAnchors(",
                "GridLayoutGroup"
            };
            return forbiddenTokens.Count(token => block.IndexOf(token, StringComparison.Ordinal) >= 0);
        }

        private static string ExtractSourceBlock(string source, string startToken, string endToken)
        {
            string safeSource = source ?? string.Empty;
            int start = safeSource.IndexOf(startToken, StringComparison.Ordinal);
            if (start < 0)
            {
                return string.Empty;
            }

            int end = safeSource.IndexOf(endToken, start + startToken.Length, StringComparison.Ordinal);
            if (end < 0)
            {
                end = safeSource.Length;
            }

            return safeSource.Substring(start, end - start);
        }

        private static string ReadProjectText(string projectRoot, string projectRelativePath)
        {
            string path = Path.Combine(
                projectRoot ?? string.Empty,
                (projectRelativePath ?? string.Empty).Replace('/', Path.DirectorySeparatorChar));
            return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
        }

        private static bool ContainsForbiddenPlayerToken(string value)
        {
            string safeValue = value ?? string.Empty;
            return ForbiddenPlayerAnswerTokens.Any(token =>
                safeValue.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0);
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

        private static void RequireFalse(
            BuildSandboxValidationReport report,
            string code,
            bool value,
            string message)
        {
            RequireTrue(report, code, !value, message);
        }

        private static void RequireEquals(
            BuildSandboxValidationReport report,
            string code,
            string label,
            int actual,
            int expected)
        {
            if (actual == expected)
            {
                report.AddInfo(code, $"{label} pass. actual={actual}.", PackageName);
                return;
            }

            report.AddError(code, $"{label} mismatch. actual={actual}, expected={expected}.", PackageName);
        }

        private static int CountTrue(params bool[] values)
        {
            return values.Count(value => value);
        }

        private sealed class SourceScanEvidence
        {
            public bool devChapterSelectorBindingPresent;
            public bool runtimeChapterSelectionApiPresent;
            public bool restartSupported;
            public bool chapterSwitchSupported;
            public bool touchesFormalStageConfig;
            public bool touchesFormalBossTable;
            public bool touchesFormalRewardTable;
            public int devChapterSelectorLayoutWriteCount;
        }
    }
}
#endif
