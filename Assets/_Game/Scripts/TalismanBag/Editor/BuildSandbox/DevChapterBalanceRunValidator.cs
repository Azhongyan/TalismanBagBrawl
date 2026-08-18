#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BuildSandbox;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class DevChapterBalanceRunValidator
    {
        public const string QaMenuPath =
            "Tools/Talisman Bag/V0.4/BuildSandbox/DevChapterBalanceRun01/[QA Only] Run Dev Chapter Balance Run";

        private static readonly string[] RequiredShapeRules =
        {
            "vertical_defense_wall",
            "corner_cleanse_array",
            "furnace_core_array",
            "thunder_fire_cross_array"
        };
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
            List<BuildSandboxValidationReport> reports = BuildValidationReports();
            DevChapterBalanceRun run = BuildDefaultRun();
            string[] reportPaths = DevChapterBalanceRunReportWriter.WriteReports(reports, run);
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
                $"[BuildSandbox-DevChapterBalanceRun01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox DevChapterBalanceRun01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static List<BuildSandboxValidationReport> BuildValidationReports()
        {
            return new List<BuildSandboxValidationReport>
            {
                Validate()
            };
        }

        public static DevChapterBalanceRun BuildDefaultRun()
        {
            return DevChapterBalanceRunBuilder.BuildDefaultRun();
        }

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report = new("Dev Chapter Balance Run");
            DevChapterBalanceRun run = BuildDefaultRun();
            ValidateRunFlags(report, run);
            ValidateStages(report, run);
            ValidateFeatureFlags(report);
            return report;
        }

        private static void ValidateRunFlags(BuildSandboxValidationReport report, DevChapterBalanceRun run)
        {
            if (run == null)
            {
                report.AddError("DEV_BALANCE_RUN_NULL", "DevChapterBalanceRun is missing.", nameof(DevChapterBalanceRun));
                return;
            }

            if (!string.Equals(run.packageName, DevChapterBalanceRun.PackageName, StringComparison.Ordinal))
            {
                report.AddError(
                    "DEV_BALANCE_PACKAGE_NAME_MISMATCH",
                    $"Package name must be {DevChapterBalanceRun.PackageName}; actual={run.packageName}.",
                    nameof(DevChapterBalanceRun));
            }

            if (!run.devOnly || run.isEnabled)
            {
                report.AddError(
                    "DEV_BALANCE_RUN_ISOLATION_FLAG_FAIL",
                    "Balance run must stay devOnly=true and isEnabled=false.",
                    nameof(DevChapterBalanceRun));
            }

            if (!run.readsBuildProblemSeedData
                || !run.readsEnemyBossValidationPool
                || !run.readsBuildCombatPreview
                || !run.readsShapeBuildRulePreview)
            {
                report.AddError(
                    "DEV_BALANCE_SOURCE_CHAIN_INCOMPLETE",
                    "Balance run must chain BuildProblemSeedData, EnemyBossValidationPool, BuildCombatPreview, and ShapeBuildRulePreview.",
                    nameof(DevChapterBalanceRun));
            }

            if (!run.writesDeveloperDataPanelFields || !run.writesReports)
            {
                report.AddError(
                    "DEV_BALANCE_REPORT_OUTPUT_MISSING",
                    "Balance run must expose developer data panel fields and reports.",
                    nameof(DevChapterBalanceRun));
            }

            if (!run.playerUiChineseOnly || run.playerUiShowsFullAnswers)
            {
                report.AddError(
                    "DEV_BALANCE_PLAYER_UI_LEAK",
                    "Player-side feedback must stay Chinese-only and must not show complete answers.",
                    nameof(DevChapterBalanceRun));
            }

            if (run.createsFormalChapterEntry
                || run.writesFormalFlow
                || run.writesFormalSaveData
                || run.writesFormalReward
                || run.advancesChapterProgress
                || run.opensFeatureFlag
                || run.touchesV02OrV03Scene
                || run.touchesCurrentV04RectTransform)
            {
                report.AddError(
                    "DEV_BALANCE_FORMAL_SCOPE_LEAK",
                    "Balance run must not create formal chapter entry, write formal data, open flags, or touch scenes/UI transforms.",
                    nameof(DevChapterBalanceRun));
            }

            if (run.PlayerSideAnswerLeakCount > 0)
            {
                report.AddError(
                    "DEV_BALANCE_PLAYER_SIDE_ANSWER_LEAK",
                    $"Player-side leak count must be 0; actual={run.PlayerSideAnswerLeakCount}.",
                    nameof(DevChapterBalanceRun));
            }

            if (run.FormalFlowLeakCount > 0)
            {
                report.AddError(
                    "DEV_BALANCE_FORMAL_LEAK_COUNT",
                    $"Formal leak count must be 0; actual={run.FormalFlowLeakCount}.",
                    nameof(DevChapterBalanceRun));
            }

            if (!run.DevOnlyIsolationPass)
            {
                report.AddError(
                    "DEV_BALANCE_ISOLATION_PASS_FALSE",
                    "DevOnlyIsolationPass must be true.",
                    nameof(DevChapterBalanceRun));
            }

            report.AddInfo(
                "DEV_BALANCE_RUN_FLAGS_OK",
                $"Run flags scanned. stages={run.StageCount}, leaks={run.PlayerSideAnswerLeakCount}/{run.FormalFlowLeakCount}.",
                nameof(DevChapterBalanceRun));
        }

        private static void ValidateStages(BuildSandboxValidationReport report, DevChapterBalanceRun run)
        {
            IReadOnlyList<DevChapterBalanceRunStage> stages = run?.stages != null
                ? (IReadOnlyList<DevChapterBalanceRunStage>)run.stages
                : Array.Empty<DevChapterBalanceRunStage>();

            if (stages.Count < 4)
            {
                report.AddError(
                    "DEV_BALANCE_STAGE_COUNT_LOW",
                    $"DevChapterBalanceRun01 requires at least 4 stages; actual={stages.Count}.",
                    nameof(DevChapterBalanceRunStage));
            }

            if ((run?.Chapter310StageCount ?? 0) < 2 || (run?.Chapter410StageCount ?? 0) < 2)
            {
                report.AddError(
                    "DEV_BALANCE_CHAPTER_PAIR_MISSING",
                    "Balance run must include both 3-10 and 4-10 devOnly sequences.",
                    nameof(DevChapterBalanceRunStage));
            }

            ValidateDifficultyCoverage(report, stages);
            ValidateShapeCoverage(report, stages);
            ValidateBossAndBuildDifference(report, stages);

            foreach (DevChapterBalanceRunStage stage in stages)
            {
                ValidateStage(report, stage);
            }
        }

        private static void ValidateDifficultyCoverage(
            BuildSandboxValidationReport report,
            IReadOnlyList<DevChapterBalanceRunStage> stages)
        {
            HashSet<string> difficulties = new(
                stages
                    .Where(stage => stage != null)
                    .Select(stage => stage.difficultyTendencyChinese)
                    .Where(value => !string.IsNullOrWhiteSpace(value)),
                StringComparer.Ordinal);

            foreach (string difficulty in new[]
                     {
                         DevChapterBalanceRunBuilder.DifficultyTooEasy,
                         DevChapterBalanceRunBuilder.DifficultyFit,
                         DevChapterBalanceRunBuilder.DifficultyTooHard
                     })
            {
                if (!difficulties.Contains(difficulty))
                {
                    report.AddError(
                        "DEV_BALANCE_DIFFICULTY_BUCKET_MISSING",
                        $"Missing difficulty tendency: {difficulty}.",
                        nameof(DevChapterBalanceRunStage));
                }
            }
        }

        private static void ValidateShapeCoverage(
            BuildSandboxValidationReport report,
            IReadOnlyList<DevChapterBalanceRunStage> stages)
        {
            HashSet<string> shapeKeys = new(
                stages
                    .Where(stage => stage != null)
                    .Select(stage => stage.shapeRuleKey)
                    .Where(value => !string.IsNullOrWhiteSpace(value)),
                StringComparer.Ordinal);

            foreach (string key in RequiredShapeRules)
            {
                if (!shapeKeys.Contains(key))
                {
                    report.AddError(
                        "DEV_BALANCE_SHAPE_RULE_MISSING",
                        $"Missing required shape rule in balance run: {key}.",
                        nameof(DevChapterBalanceRunStage));
                }
            }
        }

        private static void ValidateBossAndBuildDifference(
            BuildSandboxValidationReport report,
            IReadOnlyList<DevChapterBalanceRunStage> stages)
        {
            int distinctBossCount = stages
                .Where(stage => stage != null)
                .Select(stage => stage.bossProblemId)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .Count();
            int distinctFeedbackCount = stages
                .Where(stage => stage != null)
                .Select(stage => stage.currentBuildFeedbackChinese)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .Count();

            if (distinctBossCount < 4)
            {
                report.AddError(
                    "DEV_BALANCE_BOSS_DIFFERENCE_LOW",
                    $"Balance run must distinguish different Boss/problem rows; distinct={distinctBossCount}.",
                    nameof(DevChapterBalanceRunStage));
            }

            if (distinctFeedbackCount < 4)
            {
                report.AddError(
                    "DEV_BALANCE_BUILD_FEEDBACK_DIFFERENCE_LOW",
                    $"Balance run must distinguish different build feedback rows; distinct={distinctFeedbackCount}.",
                    nameof(DevChapterBalanceRunStage));
            }
        }

        private static void ValidateStage(BuildSandboxValidationReport report, DevChapterBalanceRunStage stage)
        {
            string path = $"{nameof(DevChapterBalanceRunStage)}:{stage?.stageId}";
            if (stage == null)
            {
                report.AddError("DEV_BALANCE_STAGE_NULL", "Balance run stage is null.", nameof(DevChapterBalanceRunStage));
                return;
            }

            if (string.IsNullOrWhiteSpace(stage.stageId)
                || string.IsNullOrWhiteSpace(stage.devChapterLabel)
                || string.IsNullOrWhiteSpace(stage.previewBuildId))
            {
                report.AddError(
                    "DEV_BALANCE_STAGE_ID_MISSING",
                    "Stage id, dev chapter label, and preview build id must be present.",
                    path);
            }

            if (string.IsNullOrWhiteSpace(stage.recommendedTestTargetChinese)
                || string.IsNullOrWhiteSpace(stage.currentBuildFeedbackChinese)
                || string.IsNullOrWhiteSpace(stage.bossMechanicFeedbackChinese)
                || string.IsNullOrWhiteSpace(stage.difficultyTendencyChinese)
                || string.IsNullOrWhiteSpace(stage.tuningSuggestionChinese))
            {
                report.AddError(
                    "DEV_BALANCE_STAGE_OUTPUT_MISSING",
                    "Each stage must output target, build feedback, boss feedback, difficulty, and tuning suggestion.",
                    path);
            }

            if (stage.PlayerSideAnswerLeak)
            {
                report.AddError(
                    "DEV_BALANCE_STAGE_PLAYER_LEAK",
                    "Stage player-facing fields contain answer tokens, Latin letters, or empty player fields.",
                    path);
            }

            if (stage.FormalFlowLeak)
            {
                report.AddError(
                    "DEV_BALANCE_STAGE_FORMAL_LEAK",
                    "Stage leaks into formal flow, save, reward, chapter entry, flag, or scene/UI scope.",
                    path);
            }

            if (stage.combatPreview == null
                || !stage.combatPreview.devOnly
                || stage.combatPreview.isEnabled
                || stage.combatPreview.PlayerSideAnswerLeakCount > 0
                || stage.combatPreview.FormalFlowLeakCount > 0)
            {
                report.AddError(
                    "DEV_BALANCE_COMBAT_PREVIEW_INVALID",
                    "Stage must reuse isolated BattleSandboxBuildCombatPreview output with no player/formal leaks.",
                    path);
            }

            BattleSandboxShapeBuildRuleMatch targetShape = (stage.shapeBuildRulePreview?.matches
                    ?? new List<BattleSandboxShapeBuildRuleMatch>())
                .FirstOrDefault(match => match != null
                    && string.Equals(match.englishStableKey, stage.shapeRuleKey, StringComparison.Ordinal));
            if (targetShape == null || !targetShape.isMatched)
            {
                report.AddError(
                    "DEV_BALANCE_TARGET_SHAPE_NOT_MATCHED",
                    $"Target shape rule must be matched: {stage.shapeRuleKey}.",
                    path);
            }

            if (stage.developerPanelFields == null || stage.developerPanelFields.Count < 8)
            {
                report.AddError(
                    "DEV_BALANCE_DEV_PANEL_FIELDS_LOW",
                    "Stage must expose developer panel fields for report/debug review.",
                    path);
            }

            if (stage.simulationResult == null || stage.simulationResult.numericAnomaly)
            {
                report.AddError(
                    "DEV_BALANCE_SIMULATION_INVALID",
                    "Stage simulation result must be present without numeric anomaly.",
                    path);
            }

            report.AddInfo(
                "DEV_BALANCE_STAGE_SCANNED",
                $"{stage.stageId} {stage.devChapterLabel} difficulty={stage.difficultyTendencyChinese} winRate={stage.simulatedWinRate:0.000}.",
                path);
        }

        private static void ValidateFeatureFlags(BuildSandboxValidationReport report)
        {
            foreach (BuildSandboxFeatureFlagDefinition flag in BuildSandboxFeatureFlags.All)
            {
                if (flag.DefaultValue)
                {
                    report.AddError(
                        "DEV_BALANCE_FEATURE_FLAG_TRUE",
                        $"{flag.Key} must stay false for DevChapterBalanceRun01.",
                        nameof(BuildSandboxFeatureFlags));
                    continue;
                }

                report.AddInfo("DEV_BALANCE_FEATURE_FLAG_FALSE", $"{flag.Key}=false.", nameof(BuildSandboxFeatureFlags));
            }
        }
    }
}
#endif
