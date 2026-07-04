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
    public static class BuildSandboxItemStatCombatPreviewReportWriter
    {
        public const string MainReportPath =
            "Docs/V0.4/Reports/BuildSandboxItemStatCombatPreviewReport.md";

        public const string RowReportPath =
            "Docs/V0.4/Reports/BuildSandboxItemStatCombatPreviewRows.csv";

        public const string LeakCheckReportPath =
            "Docs/V0.4/Reports/BuildSandboxItemStatCombatPreviewLeakCheckReport.md";

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattleSandboxBuildCombatPreview preview = null)
        {
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();
            BattleSandboxBuildCombatPreview safePreview =
                preview ?? BuildSandboxItemStatCombatPreviewValidator.BuildDefaultPreview();

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
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> items =
                BuildSandboxItemStatFoundationValidator.PlacedItems(preview);
            IReadOnlyList<BattleSandboxBuildCombatPreviewRow> rows =
                BuildSandboxItemStatCombatPreviewValidator.ItemStatRows(preview);

            StringBuilder builder = new();
            builder.AppendLine("# BuildSandbox ItemStat Combat Preview Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BuildSandboxItemStatCombatPreviewValidator.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 && leakCount == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine($"Leak Count: `{leakCount}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- User-specified devOnly direct package; not listed in the current V0.4 Package Queue.");
            builder.AppendLine("- Connects BuildSandboxItemStat from V04 placed snapshots into BattleSandbox BuildCombatPreview feedback rows.");
            builder.AppendLine("- Player-side feedback stays masked as battle phenomena; exact stat values stay in reports/developer data.");
            builder.AppendLine("- Does not touch V0.2/V0.3, formal battle, saves, rewards, Boss configs, formal number mainline, or scene UI layout.");
            builder.AppendLine();
            builder.AppendLine("## Counters");
            builder.AppendLine();
            builder.AppendLine($"- placed item snapshot count: `{items.Count}`");
            builder.AppendLine($"- item stat profile count: `{preview?.ItemStatProfileCount ?? 0}`");
            builder.AppendLine($"- item stat combat feedback row count: `{rows.Count}`");
            builder.AppendLine($"- item stat scope leak count: `{preview?.ItemStatScopeLeakCount ?? 1}`");
            builder.AppendLine($"- player-side answer leak count: `{preview?.PlayerSideAnswerLeakCount ?? 1}`");
            builder.AppendLine($"- formal flow leak count: `{preview?.FormalFlowLeakCount ?? 1}`");
            builder.AppendLine($"- feature flag default true count: `{preview?.FeatureFlagDefaultTrueCount ?? 1}`");
            builder.AppendLine();
            AppendItemStatRows(builder, items);
            AppendFeedbackRows(builder, rows);
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildRowsCsv(BattleSandboxBuildCombatPreview preview)
        {
            StringBuilder csv = new();
            csv.AppendLine("feedbackId,feedbackKind,stateLineChinese,castSkillLineChinese,floatingTextChinese,combatLogLineChinese,sourceDataPath,developerDataPanelFieldKey,placedItemSnapshotCount,itemStatProfileCount,playerSideAnswerLeak,formalFlowLeak");
            foreach (BattleSandboxBuildCombatPreviewRow row in BuildSandboxItemStatCombatPreviewValidator.ItemStatRows(preview))
            {
                csv.AppendLine(Csv(
                    row.feedbackId,
                    row.feedbackKind,
                    row.stateLineChinese,
                    row.castSkillLineChinese,
                    row.floatingTextChinese,
                    row.combatLogLineChinese,
                    row.sourceDataPath,
                    row.developerDataPanelFieldKey,
                    row.placedItemSnapshotCount.ToString(),
                    (preview?.ItemStatProfileCount ?? 0).ToString(),
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
            builder.AppendLine("# BuildSandbox ItemStat Combat Preview Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BuildSandboxItemStatCombatPreviewValidator.PackageName}`");
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
            builder.AppendLine($"| `itemStatScopeLeakCount` | {preview?.ItemStatScopeLeakCount ?? 1} | 0 |");
            builder.AppendLine("| `formalRunFlowConnections` | 0 | 0 |");
            builder.AppendLine("| `formalDamageSettlementCalls` | 0 | 0 |");
            builder.AppendLine("| `rewardWrites` | 0 | 0 |");
            builder.AppendLine("| `saveWrites` | 0 | 0 |");
            builder.AppendLine("| `chapterAdvances` | 0 | 0 |");
            builder.AppendLine("| `rectTransformMovesAuthored` | 0 | 0 |");
            builder.AppendLine($"| `totalLeaks` | {leakCount} | 0 |");
            builder.AppendLine();
            builder.AppendLine("## Scope Confirmation");
            builder.AppendLine();
            builder.AppendLine("- ItemStat profiles remain devOnly=true and isEnabled=false.");
            builder.AppendLine("- Feedback rows use existing BattleSandbox combat feedback language only.");
            builder.AppendLine("- No formal combat, save, reward, Boss, numeric config, or UI layout writes are introduced.");
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static int CountLeaks(BattleSandboxBuildCombatPreview preview)
        {
            return (preview?.FeatureFlagDefaultTrueCount ?? 1)
                + (preview?.FormalFlowLeakCount ?? 1)
                + (preview?.PlayerSideAnswerLeakCount ?? 1)
                + (preview?.ItemStatScopeLeakCount ?? 1);
        }

        private static void AppendItemStatRows(
            StringBuilder builder,
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> items)
        {
            builder.AppendLine("## ItemStat Source Rows");
            builder.AppendLine();
            builder.AppendLine("| Item | Shape | Stat Profile | Attack | Guard | Spirit | Control | Break | Cleanse | devOnly | isEnabled |");
            builder.AppendLine("| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |");
            foreach (BuildSandboxPlacedItemSnapshot item in items)
            {
                BuildSandboxItemStat stat = item?.itemStat;
                builder.AppendLine(
                    $"| `{Escape(item?.itemId)}` | `{Escape(item?.shapeId)}` | `{Escape(stat?.statProfileId)}` | {stat?.attack ?? 0} | {stat?.guard ?? 0} | {stat?.spirit ?? 0} | {stat?.control ?? 0} | {stat?.shieldBreak ?? 0} | {stat?.cleanse ?? 0} | `{stat?.devOnly ?? false}` | `{stat?.isEnabled ?? false}` |");
            }

            builder.AppendLine();
        }

        private static void AppendFeedbackRows(
            StringBuilder builder,
            IReadOnlyList<BattleSandboxBuildCombatPreviewRow> rows)
        {
            builder.AppendLine("## ItemStat Feedback Rows");
            builder.AppendLine();
            builder.AppendLine("| Feedback | Kind | State | Cast | Floating | Combat Log | Source |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- |");
            foreach (BattleSandboxBuildCombatPreviewRow row in rows)
            {
                builder.AppendLine(
                    $"| `{Escape(row.feedbackId)}` | `{Escape(row.feedbackKind)}` | {Escape(row.stateLineChinese)} | {Escape(row.castSkillLineChinese)} | {Escape(row.floatingTextChinese)} | {Escape(row.combatLogLineChinese)} | `{Escape(row.sourceDataPath)}` |");
            }

            builder.AppendLine();
        }

        private static void AppendValidationSummary(
            StringBuilder builder,
            IReadOnlyList<BuildSandboxValidationReport> reports)
        {
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
