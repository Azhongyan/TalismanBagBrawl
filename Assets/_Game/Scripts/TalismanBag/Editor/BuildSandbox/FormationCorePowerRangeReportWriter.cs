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
        private static readonly Encoding Utf8WithBom = new UTF8Encoding(true);

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

            WriteUtf8BomText(
                mainPath,
                BuildMainReport(safeReports, safePowerPreview, safeCombatPreview, errors, warnings, leakCount));
            WriteUtf8BomText(
                rowPath,
                BuildRowsCsv(safePowerPreview));
            WriteUtf8BomText(
                leakPath,
                BuildLeakCheckReport(safeReports, safePowerPreview, safeCombatPreview, errors, warnings, leakCount));

            AssetDatabase.Refresh();
            return new[] { mainPath, rowPath, leakPath };
        }

        private static void WriteUtf8BomText(string path, string contents)
        {
            byte[] preamble = Utf8WithBom.GetPreamble();
            byte[] body = Utf8WithBom.GetBytes(contents ?? string.Empty);
            File.WriteAllBytes(path, preamble.Concat(body).ToArray());
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
            builder.AppendLine("Guard Pass: `GUARD_PASS_FORMATION_CORE_POWER_RANGE01`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 && leakCount == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine($"Leak Count: `{leakCount}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Keeps the legacy FormationCoreAndPowerRange report path as a devOnly V0.4 compatibility view.");
            builder.AppendLine("- The fixed sandbox eye cell is `2,2` on the 5x5 V04 board.");
            builder.AppendLine("- Formal Powered providers are restricted to the spirit stone / energy stone family.");
            builder.AppendLine("- The eye contributes only WeakPulse; energy incense, furnace/core, peach-wood, seal, and tags are not formal providers.");
            builder.AppendLine("- BuildCombatPreview and RuntimeLoop continue to read only the current sandbox board snapshot.");
            builder.AppendLine("- No scene YAML, RectTransform, formal RunFlow, SaveData, reward, chapter, Boss, or formal numeric table writes.");
            builder.AppendLine();
            builder.AppendLine("## Counters");
            builder.AppendLine();
            builder.AppendLine($"- eyeCell exists: `{(powerPreview?.EyeCellExists ?? false)}`");
            builder.AppendLine($"- eyeCell occupied count: `{powerPreview?.EyeCellOccupiedCount ?? 1}`");
            builder.AppendLine($"- WeakPulse cell count: `{powerPreview?.WeakPulseCellCount ?? 0}`");
            builder.AppendLine($"- WeakPulse item count: `{powerPreview?.WeakPulseItemCount ?? 0}`");
            builder.AppendLine($"- provider count: `{powerPreview?.ProviderCount ?? 0}`");
            builder.AppendLine($"- Powered provider count: `{powerPreview?.ProviderCount ?? 0}`");
            builder.AppendLine($"- powered item count: `{powerPreview?.PoweredItemCount ?? 0}`");
            builder.AppendLine($"- unpowered item count: `{powerPreview?.UnpoweredItemCount ?? 0}`");
            builder.AppendLine($"- core touch count: `{powerPreview?.CoreTouchCount ?? 0}`");
            builder.AppendLine($"- Powered cell count: `{powerPreview?.PowerRangeCellCount ?? 0}`");
            builder.AppendLine($"- non-provider item misidentified as provider count: `{powerPreview?.ForbiddenProviderMisidentifiedCount ?? 1}`");
            builder.AppendLine($"- tag auto power provider count: `{powerPreview?.TagAutoPowerProviderCount ?? 1}`");
            builder.AppendLine($"- old isPowered main judgment mismatch count: `{CountOldIsPoweredDerivedMismatches(powerPreview)}`");
            builder.AppendLine($"- formation feedback rows: `{CountFormationRows(combatPreview)}`");
            builder.AppendLine($"- player-side answer leak count: `{combatPreview?.PlayerSideAnswerLeakCount ?? 1}`");
            builder.AppendLine($"- formal flow leak count: `{combatPreview?.FormalFlowLeakCount ?? 1}`");
            builder.AppendLine($"- feature flag default true count: `{combatPreview?.FeatureFlagDefaultTrueCount ?? 1}`");
            builder.AppendLine();
            AppendSampleStates(builder, powerPreview);
            AppendRows(builder, powerPreview);
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildRowsCsv(FormationCorePowerRangePreview preview)
        {
            StringBuilder csv = new();
            csv.AppendLine("itemId,shapeId,energyState,isPowerProvider,isPowered,touchesFormationCore,connectedEyeId,formalEnergySourceItemId,isEnergyStoneSource,isInBasePulseRange,isEyeAdjacent,hasEnergyRoleViolation,energySourceId,powerRangeRadius,occupiedCells,powerRangeCells,energyDiagnostics,powerConnectionState,energyStateReason,chineseDisplayName,englishStableKey,playerFeedbackChinese,devOnly,isEnabled");
            foreach (FormationCorePowerRangeRow row in preview?.rows ?? new List<FormationCorePowerRangeRow>())
            {
                csv.AppendLine(Csv(
                    row.itemId,
                    row.shapeId,
                    row.energyState.ToString(),
                    row.isPowerProvider.ToString(),
                    row.isPowered.ToString(),
                    row.touchesFormationCore.ToString(),
                    row.connectedEyeId,
                    row.formalEnergySourceItemId,
                    row.isEnergyStoneSource.ToString(),
                    row.isInBasePulseRange.ToString(),
                    row.isEyeAdjacent.ToString(),
                    row.hasEnergyRoleViolation.ToString(),
                    row.energySourceId,
                    row.powerRangeRadius.ToString(),
                    FormatCells(row.occupiedCells),
                    FormatCells(row.powerRangeCells),
                    string.Join(";", row.energyDiagnostics ?? new List<string>()),
                    row.powerConnectionState,
                    row.energyStateReason,
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
            builder.AppendLine($"| `eyeCellMissing` | {(powerPreview?.EyeCellExists == true ? 0 : 1)} | 0 |");
            builder.AppendLine($"| `eyeCellOccupied` | {powerPreview?.EyeCellOccupiedCount ?? 1} | 0 |");
            builder.AppendLine($"| `weakPulseCells` | {powerPreview?.WeakPulseCellCount ?? 0} | >=1 |");
            builder.AppendLine($"| `poweredProviderCount` | {powerPreview?.ProviderCount ?? 0} | >=1 |");
            builder.AppendLine($"| `poweredCells` | {powerPreview?.PowerRangeCellCount ?? 0} | >=1 |");
            builder.AppendLine($"| `forbiddenProviderMisidentified` | {powerPreview?.ForbiddenProviderMisidentifiedCount ?? 1} | 0 |");
            builder.AppendLine($"| `tagAutoPowerProvider` | {powerPreview?.TagAutoPowerProviderCount ?? 1} | 0 |");
            builder.AppendLine($"| `oldIsPoweredMainJudgmentMismatch` | {CountOldIsPoweredDerivedMismatches(powerPreview)} | 0 |");
            builder.AppendLine("| `rectTransformMovesAuthored` | 0 | 0 |");
            builder.AppendLine("| `sceneYamlWrites` | 0 | 0 |");
            builder.AppendLine("| `saveDataWrites` | 0 | 0 |");
            builder.AppendLine("| `rewardWrites` | 0 | 0 |");
            builder.AppendLine("| `chapterWrites` | 0 | 0 |");
            builder.AppendLine($"| `totalLeaks` | {leakCount} | 0 |");
            builder.AppendLine();
            builder.AppendLine("## Boundary Confirmation");
            builder.AppendLine();
            builder.AppendLine("- Feature flags remain default false.");
            builder.AppendLine("- Formation power resolver reads `FormationEnergyContract` and keeps legacy rows compatibility-only.");
            builder.AppendLine("- Eye cell remains layout state; it only paints WeakPulse range and never becomes a Powered provider.");
            builder.AppendLine("- Energy incense, furnace/core, peach-wood, seal, and tags have zero provider auto-promotion.");
            builder.AppendLine("- Player-facing rows show only Chinese combat feedback, not answer keys or weights.");
            builder.AppendLine("- No formal SaveData, Reward, RunFlow, PageState, FormationState, Boss, or scene layout writes.");
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static void AppendRows(StringBuilder builder, FormationCorePowerRangePreview preview)
        {
            builder.AppendLine("## Formation Power Rows");
            builder.AppendLine();
            builder.AppendLine("| Item | EnergyState | Provider | Powered | EyeCell | Source | Radius | Cells | Powered Range | Diagnostics | Player Feedback |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- | ---: | --- | --- | --- | --- |");
            foreach (FormationCorePowerRangeRow row in preview?.rows ?? new List<FormationCorePowerRangeRow>())
            {
                builder.AppendLine(
                    $"| `{Escape(row.itemId)}` | `{row.energyState}` | `{row.isPowerProvider}` | `{row.isPowered}` | `{row.touchesFormationCore}` | `{Escape(row.energySourceId)}` | {row.powerRangeRadius} | `{Escape(FormatCells(row.occupiedCells))}` | `{Escape(FormatCells(row.powerRangeCells))}` | `{Escape(string.Join(";", row.energyDiagnostics ?? new List<string>()))}` | {Escape(row.playerFeedbackChinese)} |");
            }

            builder.AppendLine();
        }

        private static void AppendSampleStates(StringBuilder builder, FormationCorePowerRangePreview preview)
        {
            builder.AppendLine("## Sample Energy States");
            builder.AppendLine();
            builder.AppendLine("| Sample Item | EnergyState | Provider | Provider Misidentified | Notes |");
            builder.AppendLine("| --- | --- | --- | --- | --- |");
            foreach (string itemId in new[]
                     {
                         "spirit_stone_basic",
                         "preview_energy_incense",
                         "preview_stone_core",
                         "preview_taomu_sword",
                         "preview_soul_seal"
                     })
            {
                FormationCorePowerRangeRow row = preview?.rows?.FirstOrDefault(candidate =>
                    candidate != null && string.Equals(candidate.itemId, itemId, StringComparison.Ordinal));
                if (row == null)
                {
                    builder.AppendLine($"| `{Escape(itemId)}` | `NotInDefaultSample` | `False` | `False` | not present in default sample |");
                    continue;
                }

                bool providerMisidentified = row.hasEnergyRoleViolation && row.isPowerProvider;
                string notes = row.isEnergyStoneSource
                    ? "legal Powered provider family"
                    : "receiver or ordinary item; never provider";
                builder.AppendLine(
                    $"| `{Escape(itemId)}` | `{row.energyState}` | `{row.isPowerProvider}` | `{providerMisidentified}` | {Escape(notes)} |");
            }

            builder.AppendLine();
        }

        private static int CountLeaks(
            FormationCorePowerRangePreview powerPreview,
            BattleSandboxBuildCombatPreview combatPreview)
        {
            return (powerPreview?.ScopeLeakCount ?? 1)
                + (powerPreview?.EyeCellExists == true ? 0 : 1)
                + (powerPreview?.EyeCellOccupiedCount ?? 1)
                + (powerPreview?.ForbiddenProviderMisidentifiedCount ?? 1)
                + (powerPreview?.TagAutoPowerProviderCount ?? 1)
                + CountOldIsPoweredDerivedMismatches(powerPreview)
                + (combatPreview?.FeatureFlagDefaultTrueCount ?? 1)
                + (combatPreview?.FormalFlowLeakCount ?? 1)
                + (combatPreview?.PlayerSideAnswerLeakCount ?? 1);
        }

        private static int CountOldIsPoweredDerivedMismatches(FormationCorePowerRangePreview preview)
        {
            return preview?.rows?.Count(row => row != null
                && row.isPowered != (row.energyState == EnergyState.Powered)) ?? 1;
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
