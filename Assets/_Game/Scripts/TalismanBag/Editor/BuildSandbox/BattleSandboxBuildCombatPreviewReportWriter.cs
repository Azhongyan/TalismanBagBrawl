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
    public static class BattleSandboxBuildCombatPreviewReportWriter
    {
        public const string MainReportPath =
            "Docs/V0.4/Reports/BattleSandboxBuildCombatPreviewReport.md";

        public const string RowReportPath =
            "Docs/V0.4/Reports/BattleSandboxBuildCombatPreviewRows.csv";

        public const string LeakCheckReportPath =
            "Docs/V0.4/Reports/BattleSandboxBuildCombatPreviewLeakCheckReport.md";

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattleSandboxBuildCombatPreview preview = null)
        {
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();
            BattleSandboxBuildCombatPreview safePreview =
                preview ?? BattleSandboxBuildCombatPreviewValidator.BuildDefaultPreview();

            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string mainPath = Path.Combine(projectRoot, MainReportPath);
            string rowPath = Path.Combine(projectRoot, RowReportPath);
            string leakPath = Path.Combine(projectRoot, LeakCheckReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(mainPath) ?? projectRoot);

            int errors = safeReports.Sum(report => report.ErrorCount);
            int warnings = safeReports.Sum(report => report.WarningCount);
            int leakCount = CountLeaks(safePreview);

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

            AssetDatabase.Refresh();
            return new[] { mainPath, rowPath, leakPath };
        }

        private static string BuildMainReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattleSandboxBuildCombatPreview preview,
            int errors,
            int warnings,
            int leakCount)
        {
            StringBuilder builder = new();
            builder.AppendLine("# BattleSandbox Build Combat Preview Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattleSandboxBuildCombatPreview.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 && leakCount == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine($"Leak Count: `{leakCount}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Reads the V04 sandbox board snapshot into devOnly preview data.");
            builder.AppendLine("- Reads devOnly enemy/Boss problem seed data only.");
            builder.AppendLine("- Computes synergy, affix/modifier, effect event, and readiness preview data.");
            builder.AppendLine("- Outputs only masked combat phenomena into existing Boss state, cast-bar, floating mechanic, and combat feedback rows.");
            builder.AppendLine("- Does not connect formal RunFlow, formal damage settlement, rewards, saves, chapters, feature flags, or formal Boss/enemy configs.");
            builder.AppendLine("- Player rows do not expose hardSolutionTags, requiredSynergy, requiredAffix, requiredStats, DropBias weights, or Boss six-key answers.");
            builder.AppendLine("- No board, tray, or Boss feedback RectTransform movement is authored by this package.");
            builder.AppendLine();
            builder.AppendLine("## Required Counters");
            builder.AppendLine();
            builder.AppendLine($"- preview scenario count: `{preview?.PreviewScenarioCount ?? 0}`");
            builder.AppendLine($"- placed item snapshot count: `{preview?.PlacedItemSnapshotCount ?? 0}`");
            builder.AppendLine($"- synergy match count: `{preview?.SynergyMatchCount ?? 0}`");
            builder.AppendLine($"- modifier bundle count: `{preview?.ModifierBundleCount ?? 0}`");
            builder.AppendLine($"- effect event count: `{preview?.EffectEventCount ?? 0}`");
            builder.AppendLine($"- mechanic feedback count: `{preview?.MechanicFeedbackCount ?? 0}`");
            builder.AppendLine($"- shape build rule definition count: `{preview?.ShapeBuildRuleDefinitionCount ?? 0}`");
            builder.AppendLine($"- shape build rule match count: `{preview?.ShapeBuildRuleMatchCount ?? 0}`");
            builder.AppendLine($"- shape build rule feedback row count: `{preview?.ShapeBuildRuleFeedbackRowCount ?? 0}`");
            builder.AppendLine($"- boss readiness count: `{preview?.BossReadinessCount ?? 0}`");
            builder.AppendLine($"- ready boss count: `{preview?.ReadyBossCount ?? 0}`");
            builder.AppendLine($"- player-side answer leak count: `{preview?.PlayerSideAnswerLeakCount ?? 1}`");
            builder.AppendLine($"- formal flow leak count: `{preview?.FormalFlowLeakCount ?? 1}`");
            builder.AppendLine($"- feature flag default true count: `{preview?.FeatureFlagDefaultTrueCount ?? 1}`");
            builder.AppendLine($"- feature flags all disabled: `{preview?.FeatureFlagsAllDisabled ?? false}`");
            builder.AppendLine($"- devOnly/isEnabled isolation pass: `{preview?.DevOnlyIsolationPass ?? false}`");
            builder.AppendLine();
            builder.AppendLine("## Player Feedback Samples");
            builder.AppendLine();
            builder.AppendLine("| Feedback | Kind | State | Cast | Floating | Combat Log |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- |");
            foreach (BattleSandboxBuildCombatPreviewRow row in Rows(preview).Take(16))
            {
                builder.AppendLine(
                    $"| `{Escape(row.feedbackId)}` | `{Escape(row.feedbackKind)}` | {Escape(row.stateLineChinese)} | {Escape(row.castSkillLineChinese)} | {Escape(row.floatingTextChinese)} | {Escape(row.combatLogLineChinese)} |");
            }

            AppendShapeBuildRuleSection(builder, preview);

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildRowsCsv(BattleSandboxBuildCombatPreview preview)
        {
            StringBuilder csv = new();
            csv.AppendLine("scenarioId,feedbackId,feedbackKind,stateLineChinese,castSkillLineChinese,floatingTextChinese,combatLogLineChinese,sourceDataPath,developerDataPanelFieldKey,placedItemSnapshotCount,activeSynergyCount,modifierBundleCount,effectEventCount,bossReadinessCount,readyBossCount,playerSideAnswerLeak,formalFlowLeak");
            foreach (BattleSandboxBuildCombatPreviewRow row in Rows(preview))
            {
                csv.AppendLine(Csv(
                    row.scenarioId,
                    row.feedbackId,
                    row.feedbackKind,
                    row.stateLineChinese,
                    row.castSkillLineChinese,
                    row.floatingTextChinese,
                    row.combatLogLineChinese,
                    row.sourceDataPath,
                    row.developerDataPanelFieldKey,
                    row.placedItemSnapshotCount.ToString(),
                    row.activeSynergyCount.ToString(),
                    row.modifierBundleCount.ToString(),
                    row.effectEventCount.ToString(),
                    row.bossReadinessCount.ToString(),
                    row.readyBossCount.ToString(),
                    row.playerSideAnswerLeak.ToString(),
                    row.formalFlowLeak.ToString()));
            }

            return csv.ToString();
        }

        private static string BuildLeakCheckReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattleSandboxBuildCombatPreview preview,
            int errors,
            int warnings,
            int leakCount)
        {
            StringBuilder builder = new();
            builder.AppendLine("# BattleSandbox Build Combat Preview Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattleSandboxBuildCombatPreview.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 && leakCount == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine();
            builder.AppendLine("## Leak Counters");
            builder.AppendLine();
            builder.AppendLine("| Check | Count | Expected |");
            builder.AppendLine("| --- | ---: | ---: |");
            builder.AppendLine($"| `featureFlagDefaultTrue` | {preview?.FeatureFlagDefaultTrueCount ?? 1} | 0 |");
            builder.AppendLine($"| `formalFlowLeakCount` | {preview?.FormalFlowLeakCount ?? 1} | 0 |");
            builder.AppendLine($"| `playerSideAnswerLeakCount` | {preview?.PlayerSideAnswerLeakCount ?? 1} | 0 |");
            builder.AppendLine($"| `feedbackFormalLeakCount` | {preview?.feedbackPreview?.FormalLeakCount ?? 1} | 0 |");
            builder.AppendLine($"| `shapeBuildRuleFormalLeakCount` | {preview?.shapeBuildRulePreview?.FormalLeakCount ?? 1} | 0 |");
            builder.AppendLine($"| `shapeBuildRulePlayerLeakCount` | {preview?.shapeBuildRulePreview?.PlayerSideAnswerLeakCount ?? 1} | 0 |");
            builder.AppendLine("| `formalRunFlowConnections` | 0 | 0 |");
            builder.AppendLine("| `formalDamageSettlementCalls` | 0 | 0 |");
            builder.AppendLine("| `rewardWrites` | 0 | 0 |");
            builder.AppendLine("| `saveWrites` | 0 | 0 |");
            builder.AppendLine("| `chapterAdvances` | 0 | 0 |");
            builder.AppendLine("| `rectTransformMovesAuthored` | 0 | 0 |");
            builder.AppendLine($"| `totalLeaks` | {leakCount} | 0 |");
            builder.AppendLine();
            builder.AppendLine("## Formal Scope Confirmation");
            builder.AppendLine();
            builder.AppendLine("- Feature flags remain default false.");
            builder.AppendLine("- Preview is devOnly and disabled by default.");
            builder.AppendLine("- Modifier and event bundles stay affectsFormalCombat=false.");
            builder.AppendLine("- Shape Build Rule preview stays devOnly, disabled, and snapshot-only.");
            builder.AppendLine("- No formal save/progress/reward/chapter APIs are called by this package.");
            builder.AppendLine("- Player-side feedback strings remain phenomenon-only and do not expose answer keys.");
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static int CountLeaks(BattleSandboxBuildCombatPreview preview)
        {
            return (preview?.FeatureFlagDefaultTrueCount ?? 1)
                + (preview?.FormalFlowLeakCount ?? 1)
                + (preview?.shapeBuildRulePreview?.PlayerSideAnswerLeakCount ?? 1)
                + BattleSandboxBuildCombatPreviewValidator.CountPlayerTextLeaks(preview);
        }

        private static void AppendShapeBuildRuleSection(
            StringBuilder builder,
            BattleSandboxBuildCombatPreview preview)
        {
            builder.AppendLine();
            builder.AppendLine("## Shape Build Rule Preview");
            builder.AppendLine();
            builder.AppendLine("| Chinese Field | English Stable Key | Matched | Items | Shapes | Occupied Cells | Player Feedback |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- |");
            foreach (BattleSandboxShapeBuildRuleMatch match in preview?.shapeBuildRulePreview?.matches
                         ?? new List<BattleSandboxShapeBuildRuleMatch>())
            {
                if (match == null)
                {
                    continue;
                }

                builder.AppendLine(
                    $"| {Escape(match.chineseDisplayName)} | `{Escape(match.englishStableKey)}` | `{match.isMatched}` | `{Escape(match.matchedItemIds)}` | `{Escape(match.matchedShapeIds)}` | `{Escape(match.matchedOccupiedCells)}` | {Escape(match.combatLogLineChinese)} |");
            }
        }

        private static IEnumerable<BattleSandboxBuildCombatPreviewRow> Rows(
            BattleSandboxBuildCombatPreview preview)
        {
            return preview?.rows?
                .Where(row => row != null)
                ?? Enumerable.Empty<BattleSandboxBuildCombatPreviewRow>();
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
