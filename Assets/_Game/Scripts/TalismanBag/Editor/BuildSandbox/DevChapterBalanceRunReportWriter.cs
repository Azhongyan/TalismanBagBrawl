#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TalismanBag.BuildSandbox;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class DevChapterBalanceRunReportWriter
    {
        public const string ReportPath =
            "Docs/V0.4/Reports/DevChapterBalanceRunReport.md";

        public const string RowsCsvPath =
            "Docs/V0.4/Reports/DevChapterBalanceRunRows.csv";

        public const string LeakCheckReportPath =
            "Docs/V0.4/Reports/DevChapterBalanceRunLeakCheckReport.md";

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            DevChapterBalanceRun run = null)
        {
            DevChapterBalanceRun safeRun = run ?? DevChapterBalanceRunBuilder.BuildDefaultRun();
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string reportAbsolutePath = Path.Combine(projectRoot, ReportPath);
            string csvAbsolutePath = Path.Combine(projectRoot, RowsCsvPath);
            string leakAbsolutePath = Path.Combine(projectRoot, LeakCheckReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(reportAbsolutePath) ?? projectRoot);

            File.WriteAllText(reportAbsolutePath, BuildMarkdown(safeReports, safeRun), new UTF8Encoding(false));
            File.WriteAllText(csvAbsolutePath, BuildRowsCsv(safeRun), new UTF8Encoding(false));
            File.WriteAllText(leakAbsolutePath, BuildLeakMarkdown(safeReports, safeRun), new UTF8Encoding(false));
            AssetDatabase.Refresh();
            return new[] { reportAbsolutePath, csvAbsolutePath, leakAbsolutePath };
        }

        private static string BuildMarkdown(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            DevChapterBalanceRun run)
        {
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);
            StringBuilder builder = new();
            builder.AppendLine("# Dev Chapter Balance Run Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{DevChapterBalanceRun.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- devOnly 3-10 / 4-10 balance validation run only.");
            builder.AppendLine("- Chains BuildProblemSeedData, EnemyBossValidationPool, BuildCombatPreview, and ShapeBuildRulePreview.");
            builder.AppendLine("- Writes reports and developer data panel fields only.");
            builder.AppendLine("- Does not create formal chapter entrance, save, reward, chapter progress, FeatureFlag, V02/V03 scene change, or current V04 RectTransform change.");
            builder.AppendLine("- Player-side text stays masked Chinese combat feedback; complete answers remain out of player UI.");
            builder.AppendLine();
            builder.AppendLine("## Summary");
            builder.AppendLine();
            builder.AppendLine("| Metric | Value |");
            builder.AppendLine("| --- | ---: |");
            builder.AppendLine($"| Stage count | {run.StageCount} |");
            builder.AppendLine($"| 3-10 stages | {run.Chapter310StageCount} |");
            builder.AppendLine($"| 4-10 stages | {run.Chapter410StageCount} |");
            builder.AppendLine($"| Too easy | {run.EasyStageCount} |");
            builder.AppendLine($"| Fit | {run.FitStageCount} |");
            builder.AppendLine($"| Too hard | {run.HardStageCount} |");
            builder.AppendLine($"| Player answer leaks | {run.PlayerSideAnswerLeakCount} |");
            builder.AppendLine($"| Formal leaks | {run.FormalFlowLeakCount} |");
            builder.AppendLine($"| FeatureFlag default true | {run.FeatureFlagDefaultTrueCount} |");
            builder.AppendLine();
            builder.AppendLine("## Balance Rows");
            builder.AppendLine();
            builder.AppendLine("| Stage | Dev Chapter | Recommended Test Target | Current Build Feedback | Boss / Mechanic Feedback | Difficulty | Tuning Suggestion |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- |");
            foreach (DevChapterBalanceRunStage stage in run.stages ?? new List<DevChapterBalanceRunStage>())
            {
                if (stage == null)
                {
                    continue;
                }

                builder.AppendLine(
                    $"| `{Escape(stage.stageId)}` | `{Escape(stage.devChapterLabel)}` | {Escape(stage.recommendedTestTargetChinese)} | {Escape(stage.currentBuildFeedbackChinese)} | {Escape(stage.bossMechanicFeedbackChinese)} | {Escape(stage.difficultyTendencyChinese)} | {Escape(stage.tuningSuggestionChinese)} |");
            }

            builder.AppendLine();
            builder.AppendLine("## Developer Data Panel Fields");
            builder.AppendLine();
            builder.AppendLine("| Stage | Field | Label | Value | Source |");
            builder.AppendLine("| --- | --- | --- | --- | --- |");
            foreach (DevChapterBalanceRunStage stage in run.stages ?? new List<DevChapterBalanceRunStage>())
            {
                if (stage == null)
                {
                    continue;
                }

                foreach (DevChapterBalanceDeveloperPanelField field in stage.developerPanelFields
                             ?? new List<DevChapterBalanceDeveloperPanelField>())
                {
                    if (field == null)
                    {
                        continue;
                    }

                    builder.AppendLine(
                        $"| `{Escape(stage.stageId)}` | `{Escape(field.fieldKey)}` | {Escape(field.labelChinese)} | `{Escape(field.value)}` | `{Escape(field.sourceDataPath)}` |");
                }
            }

            builder.AppendLine();
            builder.AppendLine("## Validation Summary");
            builder.AppendLine();
            builder.AppendLine("| Check | Status | Errors | Warnings | Info |");
            builder.AppendLine("| --- | --- | ---: | ---: | ---: |");
            foreach (BuildSandboxValidationReport report in reports)
            {
                builder.AppendLine(
                    $"| {Escape(report.Name)} | `{(report.Passed ? "PASS" : "FAIL")}` | {report.ErrorCount} | {report.WarningCount} | {report.InfoCount} |");
            }

            builder.AppendLine();
            builder.AppendLine("## Issues");
            builder.AppendLine();
            builder.AppendLine("| Level | Code | Message | Path |");
            builder.AppendLine("| --- | --- | --- | --- |");
            foreach (BuildSandboxValidationIssue issue in reports.SelectMany(report => report.Issues))
            {
                builder.AppendLine(
                    $"| `{issue.Level}` | `{Escape(issue.Code)}` | {Escape(issue.Message)} | `{Escape(issue.AssetPath)}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## QA Notes");
            builder.AppendLine();
            builder.AppendLine("- Run menu: `Tools/Talisman Bag/V0.4/BuildSandbox/DevChapterBalanceRun01/[QA Only] Run Dev Chapter Balance Run`.");
            builder.AppendLine("- Batch method: `TalismanBag.EditorTools.BuildSandbox.DevChapterBalanceRunValidator.RunBatch`.");
            builder.AppendLine("- Report rows are devOnly tuning estimates; they are not formal 3-10 / 4-10 data.");
            return builder.ToString();
        }

        private static string BuildRowsCsv(DevChapterBalanceRun run)
        {
            StringBuilder csv = new();
            csv.AppendLine("stageId,devChapterLabel,recommendedTestTarget,currentBuildFeedback,bossMechanicFeedback,difficultyTendency,tuningSuggestion,mapRuleId,enemyProblemId,bossProblemId,bossProfileId,shapeRuleKey,previewBuildId,placedItems,activeSynergies,modifiers,effectEvents,shapeMatches,bossReadinessCount,readyBossCount,readinessKeys,readinessRatio,simulatedWinRate,simulatedClearTimeSeconds,shieldBreakEfficiency,playerSideAnswerLeak,formalFlowLeak");
            foreach (DevChapterBalanceRunStage stage in run?.stages ?? new List<DevChapterBalanceRunStage>())
            {
                if (stage == null)
                {
                    continue;
                }

                csv.AppendLine(Csv(
                    stage.stageId,
                    stage.devChapterLabel,
                    stage.recommendedTestTargetChinese,
                    stage.currentBuildFeedbackChinese,
                    stage.bossMechanicFeedbackChinese,
                    stage.difficultyTendencyChinese,
                    stage.tuningSuggestionChinese,
                    stage.mapRuleId,
                    stage.enemyProblemId,
                    stage.bossProblemId,
                    stage.bossProfileId,
                    stage.shapeRuleKey,
                    stage.previewBuildId,
                    stage.placedItemSnapshotCount.ToString(),
                    stage.activeSynergyCount.ToString(),
                    stage.modifierBundleCount.ToString(),
                    stage.effectEventCount.ToString(),
                    stage.shapeBuildRuleMatchCount.ToString(),
                    stage.bossReadinessCount.ToString(),
                    stage.readyBossCount.ToString(),
                    $"{stage.readinessSatisfiedKeyCount}/{stage.readinessTotalKeyCount}",
                    stage.readinessRatio.ToString("0.000"),
                    stage.simulatedWinRate.ToString("0.000"),
                    stage.simulatedClearTimeSeconds.ToString("0.0"),
                    stage.shieldBreakEfficiency.ToString("0.000"),
                    stage.PlayerSideAnswerLeak.ToString(),
                    stage.FormalFlowLeak.ToString()));
            }

            return csv.ToString();
        }

        private static string BuildLeakMarkdown(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            DevChapterBalanceRun run)
        {
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);
            StringBuilder builder = new();
            builder.AppendLine("# Dev Chapter Balance Run Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{DevChapterBalanceRun.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine();
            builder.AppendLine("## Leak Counters");
            builder.AppendLine();
            builder.AppendLine("| Counter | Value | Expected |");
            builder.AppendLine("| --- | ---: | ---: |");
            builder.AppendLine($"| Player-side answer leaks | {run.PlayerSideAnswerLeakCount} | 0 |");
            builder.AppendLine($"| Formal flow leaks | {run.FormalFlowLeakCount} | 0 |");
            builder.AppendLine($"| FeatureFlag default true | {run.FeatureFlagDefaultTrueCount} | 0 |");
            builder.AppendLine($"| Formal chapter entries | {(run.createsFormalChapterEntry ? 1 : 0)} | 0 |");
            builder.AppendLine($"| Formal save writes | {(run.writesFormalSaveData ? 1 : 0)} | 0 |");
            builder.AppendLine($"| Formal reward writes | {(run.writesFormalReward ? 1 : 0)} | 0 |");
            builder.AppendLine($"| Chapter progress writes | {(run.advancesChapterProgress ? 1 : 0)} | 0 |");
            builder.AppendLine($"| V02/V03 scene touches | {(run.touchesV02OrV03Scene ? 1 : 0)} | 0 |");
            builder.AppendLine($"| V04 RectTransform touches | {(run.touchesCurrentV04RectTransform ? 1 : 0)} | 0 |");
            builder.AppendLine();
            builder.AppendLine("## Stage Leak Rows");
            builder.AppendLine();
            builder.AppendLine("| Stage | Player Leak | Formal Leak | Combat Preview Leaks | Shape Preview Leaks | Developer Fields |");
            builder.AppendLine("| --- | --- | --- | ---: | ---: | ---: |");
            foreach (DevChapterBalanceRunStage stage in run.stages ?? new List<DevChapterBalanceRunStage>())
            {
                if (stage == null)
                {
                    continue;
                }

                builder.AppendLine(
                    $"| `{Escape(stage.stageId)}` | `{stage.PlayerSideAnswerLeak}` | `{stage.FormalFlowLeak}` | {stage.combatPreview?.FormalFlowLeakCount ?? 1} | {stage.shapeBuildRulePreview?.FormalLeakCount ?? 1} | {stage.developerPanelFields?.Count ?? 0} |");
            }

            builder.AppendLine();
            builder.AppendLine("## Player Text Guard");
            builder.AppendLine();
            builder.AppendLine("- Player-facing balance outputs are checked for empty fields, Latin letters, and answer-like tokens.");
            builder.AppendLine("- Developer ids remain in report/developer fields only and are masked from player UI.");
            builder.AppendLine("- This package does not write scene UI or RectTransform data.");
            return builder.ToString();
        }

        private static string Escape(string value)
        {
            return (value ?? string.Empty)
                .Replace("|", "\\|")
                .Replace("\r", " ")
                .Replace("\n", " ");
        }

        private static string Csv(params string[] values)
        {
            return string.Join(",", values.Select(EscapeCsv));
        }

        private static string EscapeCsv(string value)
        {
            string normalized = value ?? string.Empty;
            if (normalized.Contains(",") || normalized.Contains("\"") || normalized.Contains("\n") || normalized.Contains("\r"))
            {
                return $"\"{normalized.Replace("\"", "\"\"")}\"";
            }

            return normalized;
        }
    }
}
#endif
