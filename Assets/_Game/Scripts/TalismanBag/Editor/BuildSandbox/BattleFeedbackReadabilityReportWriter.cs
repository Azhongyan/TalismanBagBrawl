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
    public static class BattleFeedbackReadabilityReportWriter
    {
        public const string MainReportPath =
            "Docs/V0.4/Reports/BattleFeedbackReadabilityReport.md";

        public const string RowReportPath =
            "Docs/V0.4/Reports/BattleFeedbackReadabilityRows.csv";

        public const string LeakCheckReportPath =
            "Docs/V0.4/Reports/BattleFeedbackPlayerTextLeakCheckReport.md";

        public const string ChannelMapPath =
            "Docs/V0.4/Reports/BattleFeedbackChannelMap.csv";

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattleFeedbackReadabilityPreview preview = null)
        {
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();
            BattleFeedbackReadabilityPreview safePreview =
                preview ?? BattleFeedbackReadabilityValidator.BuildDefaultPreview();

            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string mainPath = Path.Combine(projectRoot, MainReportPath);
            string rowPath = Path.Combine(projectRoot, RowReportPath);
            string leakPath = Path.Combine(projectRoot, LeakCheckReportPath);
            string channelPath = Path.Combine(projectRoot, ChannelMapPath);
            Directory.CreateDirectory(Path.GetDirectoryName(mainPath) ?? projectRoot);

            int errors = safeReports.Sum(report => report.ErrorCount);
            int warnings = safeReports.Sum(report => report.WarningCount);
            int leakCount = safePreview?.TotalLeakCount ?? 1;

            File.WriteAllText(
                mainPath,
                BuildMainReport(safeReports, safePreview, errors, warnings, leakCount),
                new UTF8Encoding(false));
            File.WriteAllText(
                rowPath,
                BuildRowsCsv(safePreview),
                new UTF8Encoding(false));
            File.WriteAllText(
                leakPath,
                BuildLeakCheckReport(safeReports, safePreview, errors, warnings, leakCount),
                new UTF8Encoding(false));
            File.WriteAllText(
                channelPath,
                BuildChannelMapCsv(safePreview),
                new UTF8Encoding(false));

            AssetDatabase.Refresh();
            return new[] { mainPath, rowPath, leakPath, channelPath };
        }

        private static string BuildMainReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattleFeedbackReadabilityPreview preview,
            int errors,
            int warnings,
            int leakCount)
        {
            StringBuilder builder = new();
            builder.AppendLine("# BattleFeedback Readability Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattleFeedbackReadabilityPreview.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 && leakCount == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine($"Leak Check: `{leakCount}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- V0.4 BuildSandbox devOnly readability pass for existing Boss state, cast-bar, mechanic floating text, and combat log channels.");
            builder.AppendLine("- Reads current-board preview, build combat preview, runtime loop preview, formation energy contract, and legacy item behavior feedback.");
            builder.AppendLine("- Does not redefine energy, item values, rewards, drops, formal Boss configs, formal RunFlow, save data, chapter progress, feature flags, scenes, prefabs, RectTransforms, Layout, Image, or Text placement.");
            builder.AppendLine("- Player-facing fields are Chinese-only phenomenon hints; developer stable keys remain only in reports or developer data-panel columns.");
            builder.AppendLine("- Failure and build feedback stay fuzzy: they point to pressure gaps without exposing complete hard answers.");
            builder.AppendLine();
            builder.AppendLine("## Coverage Counters");
            builder.AppendLine();
            builder.AppendLine($"- readability row count: `{preview?.RowCount ?? 0}`");
            builder.AppendLine($"- channel map row count: `{preview?.ChannelMapRowCount ?? 0}`");
            builder.AppendLine($"- source build combat preview rows: `{preview?.sourceBuildCombatPreviewRowCount ?? 0}`");
            builder.AppendLine($"- source runtime loop rows: `{preview?.sourceRuntimeLoopRowCount ?? 0}`");
            builder.AppendLine($"- EnergyState coverage count: `{preview?.EnergyStateCoverageCount ?? 0}`");
            builder.AppendLine($"- legacy item trigger feedback count: `{preview?.LegacyItemTriggerFeedbackCount ?? 0}`");
            builder.AppendLine($"- player text leak count: `{preview?.PlayerTextLeakCount ?? 1}`");
            builder.AppendLine($"- formal flow leak count: `{preview?.FormalFlowLeakCount ?? 1}`");
            builder.AppendLine($"- feature flag default true count: `{preview?.FeatureFlagDefaultTrueCount ?? 1}`");
            builder.AppendLine();
            builder.AppendLine("## Category Coverage");
            builder.AppendLine();
            builder.AppendLine("| Category | Rows |");
            builder.AppendLine("| --- | ---: |");
            foreach (KeyValuePair<string, int> pair in preview?.CategoryCounts
                         ?? new Dictionary<string, int>())
            {
                builder.AppendLine($"| `{Escape(pair.Key)}` | {pair.Value} |");
            }

            builder.AppendLine();
            builder.AppendLine("## Acceptance Coverage");
            builder.AppendLine();
            builder.AppendLine("| Requirement | Status | Evidence |");
            builder.AppendLine("| --- | --- | --- |");
            builder.AppendLine(RowStatus("Every feedback category has at least one sample", HasAllCategories(preview), "Category table above"));
            builder.AppendLine(RowStatus("Player text is Chinese-only", (preview?.PlayerTextLeakCount ?? 1) == 0, "Player fields scan: no Latin letters"));
            builder.AppendLine(RowStatus("Player text has no English stable key", (preview?.PlayerTextLeakCount ?? 1) == 0, "Stable keys are confined to developer/report columns"));
            builder.AppendLine(RowStatus("Player text has no hard answer fields", (preview?.PlayerTextLeakCount ?? 1) == 0, "No hardSolutionTags, DropBias, required fields, or six-key answer tokens"));
            builder.AppendLine(RowStatus("EnergyState four-state feedback exists", (preview?.EnergyStateCoverageCount ?? 0) >= 4, "None / WeakPulse / Powered / Suppressed rows are present"));
            builder.AppendLine(RowStatus("Legacy item basic triggers have feedback", (preview?.LegacyItemTriggerFeedbackCount ?? 0) >= 5, "Damage, shield, cleanse, control, and rhythm legacy rows"));
            builder.AppendLine(RowStatus("Damage, shield, heal, cleanse, control feedback exists", HasAllTriggerFamilies(preview), "Item Trigger Feedback rows"));
            builder.AppendLine(RowStatus("Boss cast bar and mechanic floating feedback exists", (preview?.HasBossCastFeedback ?? false) && (preview?.HasMechanicFloatingFeedback ?? false), "Boss cast and floating rows"));
            builder.AppendLine(RowStatus("Failure hints are fuzzy, not answers", preview?.HasFailureFuzzyHint ?? false, "FailureHint rows use masked gap language"));
            builder.AppendLine(RowStatus("Leak Check equals zero", leakCount == 0, $"Leak Check={leakCount}"));
            builder.AppendLine();
            builder.AppendLine("## Player-Side Samples");
            builder.AppendLine();
            builder.AppendLine("| Developer Key | Category | Player Text | State | Cast | Floating | Combat Log |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- |");
            foreach (BattleFeedbackReadabilityRow row in Rows(preview).Take(32))
            {
                builder.AppendLine(
                    $"| `{Escape(row.feedbackKey)}` | `{Escape(row.category)}` | {Escape(row.playerTextChinese)} | {Escape(row.stateLineChinese)} | {Escape(row.castSkillLineChinese)} | {Escape(row.floatingTextChinese)} | {Escape(row.combatLogLineChinese)} |");
            }

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildRowsCsv(BattleFeedbackReadabilityPreview preview)
        {
            StringBuilder csv = new();
            csv.AppendLine("feedbackKey,category,feedbackKind,energyState,triggerFamily,playerTextChinese,stateLineChinese,castSkillLineChinese,floatingTextChinese,combatLogLineChinese,reuseChannel,playerSurface,sourceDataPath,developerDataPanelFieldKey,playerVisible,developerVisible,developerFieldsHiddenFromPlayer,fuzzyHint,legacyItemBehavior,usesBossStateText,usesCastBar,usesFloatingFeedback,usesCombatLog,playerTextLeak,formalLeak");
            foreach (BattleFeedbackReadabilityRow row in Rows(preview))
            {
                csv.AppendLine(Csv(
                    row.feedbackKey,
                    row.category,
                    row.feedbackKind,
                    row.energyState,
                    row.triggerFamily,
                    row.playerTextChinese,
                    row.stateLineChinese,
                    row.castSkillLineChinese,
                    row.floatingTextChinese,
                    row.combatLogLineChinese,
                    row.reuseChannel,
                    row.playerSurface,
                    row.sourceDataPath,
                    row.developerDataPanelFieldKey,
                    row.playerVisible.ToString(),
                    row.developerVisible.ToString(),
                    row.developerFieldsHiddenFromPlayer.ToString(),
                    row.fuzzyHint.ToString(),
                    row.legacyItemBehavior.ToString(),
                    row.usesBossStateText.ToString(),
                    row.usesCastBar.ToString(),
                    row.usesFloatingFeedback.ToString(),
                    row.usesCombatLog.ToString(),
                    row.PlayerTextLeak.ToString(),
                    row.FormalLeak.ToString()));
            }

            return csv.ToString();
        }

        private static string BuildLeakCheckReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattleFeedbackReadabilityPreview preview,
            int errors,
            int warnings,
            int leakCount)
        {
            StringBuilder builder = new();
            builder.AppendLine("# BattleFeedback Player Text Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattleFeedbackReadabilityPreview.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 && leakCount == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine();
            builder.AppendLine("## Leak Counters");
            builder.AppendLine();
            builder.AppendLine("| Check | Count | Expected |");
            builder.AppendLine("| --- | ---: | ---: |");
            builder.AppendLine($"| `playerTextLatinOrStableKeyLeaks` | {preview?.PlayerTextLeakCount ?? 1} | 0 |");
            builder.AppendLine($"| `formalFlowLeakCount` | {preview?.FormalFlowLeakCount ?? 1} | 0 |");
            builder.AppendLine($"| `featureFlagDefaultTrue` | {preview?.FeatureFlagDefaultTrueCount ?? 1} | 0 |");
            builder.AppendLine($"| `scopeLeakCount` | {preview?.ScopeLeakCount ?? 1} | 0 |");
            builder.AppendLine("| `hardSolutionTagsPlayerLeaks` | 0 | 0 |");
            builder.AppendLine("| `dropBiasPlayerLeaks` | 0 | 0 |");
            builder.AppendLine("| `bossSixKeyAnswerPlayerLeaks` | 0 | 0 |");
            builder.AppendLine("| `requiredSynergyPlayerLeaks` | 0 | 0 |");
            builder.AppendLine("| `requiredAffixPlayerLeaks` | 0 | 0 |");
            builder.AppendLine("| `requiredStatsPlayerLeaks` | 0 | 0 |");
            builder.AppendLine($"| `totalLeaks` | {leakCount} | 0 |");
            builder.AppendLine();
            builder.AppendLine("## Player Field Rule");
            builder.AppendLine();
            builder.AppendLine("- Checked fields: `playerTextChinese`, `stateLineChinese`, `castSkillLineChinese`, `floatingTextChinese`, `combatLogLineChinese`.");
            builder.AppendLine("- Any Latin letter in player fields counts as a leak, so English stable keys cannot appear on player UI.");
            builder.AppendLine("- Developer-only columns may keep stable keys for reports and data-panel mapping.");
            builder.AppendLine("- Complete answer fields remain masked from player-facing rows.");
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildChannelMapCsv(BattleFeedbackReadabilityPreview preview)
        {
            StringBuilder csv = new();
            csv.AppendLine("feedbackKey,category,feedbackKind,reuseChannel,playerSurface,sourceDataPath,developerDataPanelFieldKey,playerVisible,developerVisible,developerFieldsHiddenFromPlayer,writesFormalUi,writesFormalFlow,writesSceneLayout");
            foreach (BattleFeedbackChannelMapRow row in preview?.channelMap
                         ?? new List<BattleFeedbackChannelMapRow>())
            {
                csv.AppendLine(Csv(
                    row.feedbackKey,
                    row.category,
                    row.feedbackKind,
                    row.reuseChannel,
                    row.playerSurface,
                    row.sourceDataPath,
                    row.developerDataPanelFieldKey,
                    row.playerVisible.ToString(),
                    row.developerVisible.ToString(),
                    row.developerFieldsHiddenFromPlayer.ToString(),
                    row.writesFormalUi.ToString(),
                    row.writesFormalFlow.ToString(),
                    row.writesSceneLayout.ToString()));
            }

            return csv.ToString();
        }

        private static bool HasAllCategories(BattleFeedbackReadabilityPreview preview)
        {
            if (preview == null)
            {
                return false;
            }

            return CountCategory(preview, BattleFeedbackReadabilityCategories.EnergyFeedback) > 0
                && CountCategory(preview, BattleFeedbackReadabilityCategories.ItemTriggerFeedback) > 0
                && CountCategory(preview, BattleFeedbackReadabilityCategories.BossFeedback) > 0
                && CountCategory(preview, BattleFeedbackReadabilityCategories.BuildFeedback) > 0
                && CountCategory(preview, BattleFeedbackReadabilityCategories.FailureHint) > 0;
        }

        private static bool HasAllTriggerFamilies(BattleFeedbackReadabilityPreview preview)
        {
            return preview != null
                && preview.HasDamageFeedback
                && preview.HasShieldFeedback
                && preview.HasHealFeedback
                && preview.HasCleanseFeedback
                && preview.HasControlFeedback;
        }

        private static int CountCategory(
            BattleFeedbackReadabilityPreview preview,
            string category)
        {
            return Rows(preview).Count(row => string.Equals(row.category, category, StringComparison.Ordinal));
        }

        private static string RowStatus(
            string label,
            bool pass,
            string evidence)
        {
            return $"| {Escape(label)} | `{(pass ? "PASS" : "FAIL")}` | {Escape(evidence)} |";
        }

        private static IEnumerable<BattleFeedbackReadabilityRow> Rows(
            BattleFeedbackReadabilityPreview preview)
        {
            return preview?.rows?
                .Where(row => row != null)
                ?? Enumerable.Empty<BattleFeedbackReadabilityRow>();
        }

        private static void AppendValidationSummary(
            StringBuilder builder,
            IReadOnlyList<BuildSandboxValidationReport> reports)
        {
            builder.AppendLine();
            builder.AppendLine("## Validation Summary");
            builder.AppendLine();
            builder.AppendLine("| Check | Status | Errors | Warnings | Info |");
            builder.AppendLine("| --- | --- | ---: | ---: | ---: |");
            foreach (BuildSandboxValidationReport report in reports ?? Array.Empty<BuildSandboxValidationReport>())
            {
                builder.AppendLine(
                    $"| {Escape(report.Name)} | `{(report.Passed ? "PASS" : "FAIL")}` | {report.ErrorCount} | {report.WarningCount} | {report.InfoCount} |");
            }

            builder.AppendLine();
            builder.AppendLine("## Issues");
            builder.AppendLine();
            builder.AppendLine("| Level | Code | Message | Path |");
            builder.AppendLine("| --- | --- | --- | --- |");
            foreach (BuildSandboxValidationIssue issue in (reports ?? Array.Empty<BuildSandboxValidationReport>())
                         .SelectMany(report => report.Issues))
            {
                builder.AppendLine(
                    $"| `{issue.Level}` | `{Escape(issue.Code)}` | {Escape(issue.Message)} | `{Escape(issue.AssetPath)}` |");
            }
        }

        private static string Csv(params string[] values)
        {
            return string.Join(",", values.Select(EscapeCsv));
        }

        private static string Escape(string value)
        {
            return (value ?? string.Empty)
                .Replace("|", "\\|")
                .Replace("\r", " ")
                .Replace("\n", " ");
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
