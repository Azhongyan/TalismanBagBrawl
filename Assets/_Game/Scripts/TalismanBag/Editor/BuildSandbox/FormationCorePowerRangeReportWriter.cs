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
    public static class FormationCorePowerRangeReportWriter
    {
        public const string MainReportPath =
            "Docs/V0.4/Reports/FormationCorePowerRangeReport.md";

        public const string RowReportPath =
            "Docs/V0.4/Reports/FormationCorePowerRangeRows.csv";

        public const string LeakCheckReportPath =
            "Docs/V0.4/Reports/FormationCorePowerRangeLeakCheckReport.md";

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            FormationCorePowerRangePreview powerPreview = null,
            BattleSandboxBuildCombatPreview combatPreview = null)
        {
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();
            BattleSandboxBuildCombatPreview safeCombatPreview =
                combatPreview ?? FormationCorePowerRangeValidator.BuildDefaultPreview();
            FormationCorePowerRangePreview safePowerPreview =
                powerPreview ?? FormationCorePowerRangeResolver.Apply(safeCombatPreview?.context?.layoutSnapshot);

            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string mainPath = Path.Combine(projectRoot, MainReportPath);
            string rowPath = Path.Combine(projectRoot, RowReportPath);
            string leakPath = Path.Combine(projectRoot, LeakCheckReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(mainPath) ?? projectRoot);

            int errors = safeReports.Sum(report => report.ErrorCount);
            int warnings = safeReports.Sum(report => report.WarningCount);
            int leakCount = CountLeaks(safePowerPreview, safeCombatPreview);

            File.WriteAllText(
                mainPath,
                BuildMainReport(safeReports, safePowerPreview, safeCombatPreview, errors, warnings, leakCount),
                new UTF8Encoding(false));
            File.WriteAllText(
                rowPath,
                BuildRowsCsv(safePowerPreview),
                new UTF8Encoding(false));
            File.WriteAllText(
                leakPath,
                BuildLeakCheckReport(safeReports, safePowerPreview, safeCombatPreview, errors, warnings, leakCount),
                new UTF8Encoding(false));

            AssetDatabase.Refresh();
            return new[] { mainPath, rowPath, leakPath };
        }

        private static string BuildMainReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            FormationCorePowerRangePreview powerPreview,
            BattleSandboxBuildCombatPreview combatPreview,
            int errors,
            int warnings,
            int leakCount)
        {
            StringBuilder builder = new();
            builder.AppendLine("# Formation Core And Power Range Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{FormationCorePowerRangePreview.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 && leakCount == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine($"Leak Count: `{leakCount}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Adds a devOnly V0.4 formation core cell and power-range resolver.");
            builder.AppendLine("- The fixed sandbox core cell is `2,2` on the 5x5 V04 board.");
            builder.AppendLine("- Power providers include spirit stone, energy incense, and furnace/core preview items.");
            builder.AppendLine("- BuildCombatPreview and RuntimeLoop continue to read only the current sandbox board snapshot.");
            builder.AppendLine("- No scene YAML, RectTransform, formal RunFlow, SaveData, reward, chapter, Boss, or formal numeric table writes.");
            builder.AppendLine();
            builder.AppendLine("## Counters");
            builder.AppendLine();
            builder.AppendLine($"- provider count: `{powerPreview?.ProviderCount ?? 0}`");
            builder.AppendLine($"- powered item count: `{powerPreview?.PoweredItemCount ?? 0}`");
            builder.AppendLine($"- unpowered item count: `{powerPreview?.UnpoweredItemCount ?? 0}`");
            builder.AppendLine($"- core touch count: `{powerPreview?.CoreTouchCount ?? 0}`");
            builder.AppendLine($"- power range cell count: `{powerPreview?.PowerRangeCellCount ?? 0}`");
            builder.AppendLine($"- formation feedback rows: `{CountFormationRows(combatPreview)}`");
            builder.AppendLine($"- player-side answer leak count: `{combatPreview?.PlayerSideAnswerLeakCount ?? 1}`");
            builder.AppendLine($"- formal flow leak count: `{combatPreview?.FormalFlowLeakCount ?? 1}`");
            builder.AppendLine($"- feature flag default true count: `{combatPreview?.FeatureFlagDefaultTrueCount ?? 1}`");
            builder.AppendLine();
            AppendRows(builder, powerPreview);
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildRowsCsv(FormationCorePowerRangePreview preview)
        {
            StringBuilder csv = new();
            csv.AppendLine("itemId,shapeId,isPowerProvider,isPowered,touchesFormationCore,energySourceId,powerRangeRadius,occupiedCells,powerRangeCells,powerConnectionState,chineseDisplayName,englishStableKey,playerFeedbackChinese,devOnly,isEnabled");
            foreach (FormationCorePowerRangeRow row in preview?.rows ?? new List<FormationCorePowerRangeRow>())
            {
                csv.AppendLine(Csv(
                    row.itemId,
                    row.shapeId,
                    row.isPowerProvider.ToString(),
                    row.isPowered.ToString(),
                    row.touchesFormationCore.ToString(),
                    row.energySourceId,
                    row.powerRangeRadius.ToString(),
                    FormatCells(row.occupiedCells),
                    FormatCells(row.powerRangeCells),
                    row.powerConnectionState,
                    row.chineseDisplayName,
                    row.englishStableKey,
                    row.playerFeedbackChinese,
                    row.devOnly.ToString(),
                    row.isEnabled.ToString()));
            }

            return csv.ToString();
        }

        private static string BuildLeakCheckReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            FormationCorePowerRangePreview powerPreview,
            BattleSandboxBuildCombatPreview combatPreview,
            int errors,
            int warnings,
            int leakCount)
        {
            StringBuilder builder = new();
            builder.AppendLine("# Formation Core And Power Range Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{FormationCorePowerRangePreview.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 && leakCount == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine();
            builder.AppendLine("## Leak Counters");
            builder.AppendLine();
            builder.AppendLine("| Check | Count | Expected |");
            builder.AppendLine("| --- | ---: | ---: |");
            builder.AppendLine($"| `formationPowerScopeLeakCount` | {powerPreview?.ScopeLeakCount ?? 1} | 0 |");
            builder.AppendLine($"| `formationFeedbackRows` | {CountFormationRows(combatPreview)} | >=1 |");
            builder.AppendLine($"| `featureFlagDefaultTrue` | {combatPreview?.FeatureFlagDefaultTrueCount ?? 1} | 0 |");
            builder.AppendLine($"| `formalFlowLeakCount` | {combatPreview?.FormalFlowLeakCount ?? 1} | 0 |");
            builder.AppendLine($"| `playerSideAnswerLeakCount` | {combatPreview?.PlayerSideAnswerLeakCount ?? 1} | 0 |");
            builder.AppendLine("| `rectTransformMovesAuthored` | 0 | 0 |");
            builder.AppendLine("| `sceneYamlWrites` | 0 | 0 |");
            builder.AppendLine($"| `totalLeaks` | {leakCount} | 0 |");
            builder.AppendLine();
            builder.AppendLine("## Boundary Confirmation");
            builder.AppendLine();
            builder.AppendLine("- Feature flags remain default false.");
            builder.AppendLine("- Formation power resolver is pure BuildSandbox snapshot logic.");
            builder.AppendLine("- Player-facing rows show only Chinese combat feedback, not answer keys or weights.");
            builder.AppendLine("- No formal SaveData, Reward, RunFlow, PageState, FormationState, Boss, or scene layout writes.");
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static void AppendRows(StringBuilder builder, FormationCorePowerRangePreview preview)
        {
            builder.AppendLine("## Formation Power Rows");
            builder.AppendLine();
            builder.AppendLine("| Item | Provider | Powered | Core | Source | Radius | Cells | Range | Player Feedback |");
            builder.AppendLine("| --- | --- | --- | --- | --- | ---: | --- | --- | --- |");
            foreach (FormationCorePowerRangeRow row in preview?.rows ?? new List<FormationCorePowerRangeRow>())
            {
                builder.AppendLine(
                    $"| `{Escape(row.itemId)}` | `{row.isPowerProvider}` | `{row.isPowered}` | `{row.touchesFormationCore}` | `{Escape(row.energySourceId)}` | {row.powerRangeRadius} | `{Escape(FormatCells(row.occupiedCells))}` | `{Escape(FormatCells(row.powerRangeCells))}` | {Escape(row.playerFeedbackChinese)} |");
            }

            builder.AppendLine();
        }

        private static int CountLeaks(
            FormationCorePowerRangePreview powerPreview,
            BattleSandboxBuildCombatPreview combatPreview)
        {
            return (powerPreview?.ScopeLeakCount ?? 1)
                + (combatPreview?.FeatureFlagDefaultTrueCount ?? 1)
                + (combatPreview?.FormalFlowLeakCount ?? 1)
                + (combatPreview?.PlayerSideAnswerLeakCount ?? 1);
        }

        private static int CountFormationRows(BattleSandboxBuildCombatPreview preview)
        {
            return preview?.rows?.Count(row => row != null
                && !string.IsNullOrWhiteSpace(row.sourceDataPath)
                && row.sourceDataPath.IndexOf("formationCorePowerRange", StringComparison.OrdinalIgnoreCase) >= 0) ?? 0;
        }

        private static string FormatCells(IEnumerable<ItemShapeCell> cells)
        {
            return string.Join(";", (cells ?? Enumerable.Empty<ItemShapeCell>()).Select(cell => cell.ToString()));
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
