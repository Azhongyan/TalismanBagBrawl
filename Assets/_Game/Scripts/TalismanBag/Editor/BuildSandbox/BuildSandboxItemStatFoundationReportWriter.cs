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
    public static class BuildSandboxItemStatFoundationReportWriter
    {
        public const string MainReportPath =
            "Docs/V0.4/Reports/BuildSandboxItemStatFoundationReport.md";

        public const string RowReportPath =
            "Docs/V0.4/Reports/BuildSandboxItemStatFoundationRows.csv";

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattleSandboxBuildCombatPreview preview = null)
        {
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();
            BattleSandboxBuildCombatPreview safePreview =
                preview ?? BuildSandboxItemStatFoundationValidator.BuildDefaultPreview();

            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string mainPath = Path.Combine(projectRoot, MainReportPath);
            string rowPath = Path.Combine(projectRoot, RowReportPath);
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

            AssetDatabase.Refresh();
            return new[] { mainPath, rowPath };
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
            BuildSandboxPlacedItemSnapshot taomuSword = items.FirstOrDefault(item =>
                string.Equals(item?.itemId, "preview_taomu_sword", StringComparison.Ordinal));

            StringBuilder builder = new();
            builder.AppendLine("# BuildSandbox ItemStat Foundation Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BuildSandboxItemStatFoundationValidator.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 && leakCount == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine($"Leak Count: `{leakCount}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- User-specified devOnly direct package; not listed in the current V0.4 Package Queue.");
            builder.AppendLine("- Adds report-only ItemStat foundations to BuildSandbox placed snapshots.");
            builder.AppendLine("- Adds one devOnly preview item: \u6843\u6728\u5251, using shape id `vertical_3`.");
            builder.AppendLine("- Keeps all ItemStat profiles devOnly=true and isEnabled=false.");
            builder.AppendLine("- Does not touch V0.2/V0.3, formal battle, saves, rewards, Boss configs, formal number mainline, or scene UI layout.");
            builder.AppendLine();
            builder.AppendLine("## Counters");
            builder.AppendLine();
            builder.AppendLine($"- placed item snapshot count: `{items.Count}`");
            builder.AppendLine($"- item stat profile count: `{items.Count(item => item?.itemStat != null)}`");
            builder.AppendLine($"- item stat scope leak count: `{BuildSandboxItemStatFoundationValidator.CountItemStatScopeLeaks(preview)}`");
            builder.AppendLine($"- taomu sword present: `{(taomuSword != null)}`");
            builder.AppendLine($"- taomu sword shape: `{taomuSword?.shapeId ?? string.Empty}`");
            builder.AppendLine($"- taomu sword stat profile: `{taomuSword?.itemStat?.statProfileId ?? string.Empty}`");
            builder.AppendLine($"- player-side answer leak count: `{preview?.PlayerSideAnswerLeakCount ?? 1}`");
            builder.AppendLine($"- formal flow leak count: `{preview?.FormalFlowLeakCount ?? 1}`");
            builder.AppendLine($"- feature flag default true count: `{preview?.FeatureFlagDefaultTrueCount ?? 1}`");
            builder.AppendLine();
            AppendRows(builder, items);
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildRowsCsv(BattleSandboxBuildCombatPreview preview)
        {
            StringBuilder csv = new();
            csv.AppendLine("itemId,shapeId,occupiedCellCount,statProfileId,attack,guard,spirit,control,shieldBreak,cleanse,manaGainPerTick,manaCostPerCast,castIntervalSeconds,statTag,devOnly,isEnabled");
            foreach (BuildSandboxPlacedItemSnapshot item in BuildSandboxItemStatFoundationValidator.PlacedItems(preview))
            {
                BuildSandboxItemStat stat = item?.itemStat;
                csv.AppendLine(Csv(
                    item?.itemId,
                    item?.shapeId,
                    (item?.occupiedCells?.Count ?? 0).ToString(),
                    stat?.statProfileId,
                    (stat?.attack ?? 0).ToString(),
                    (stat?.guard ?? 0).ToString(),
                    (stat?.spirit ?? 0).ToString(),
                    (stat?.control ?? 0).ToString(),
                    (stat?.shieldBreak ?? 0).ToString(),
                    (stat?.cleanse ?? 0).ToString(),
                    (stat?.manaGainPerTick ?? 0).ToString(),
                    (stat?.manaCostPerCast ?? 0).ToString(),
                    (stat?.castIntervalSeconds ?? 0f).ToString("0.###"),
                    stat?.statTag,
                    (stat?.devOnly ?? false).ToString(),
                    (stat?.isEnabled ?? false).ToString()));
            }

            return csv.ToString();
        }

        private static int CountLeaks(BattleSandboxBuildCombatPreview preview)
        {
            return (preview?.FeatureFlagDefaultTrueCount ?? 1)
                + (preview?.FormalFlowLeakCount ?? 1)
                + (preview?.PlayerSideAnswerLeakCount ?? 1)
                + BuildSandboxItemStatFoundationValidator.CountItemStatScopeLeaks(preview);
        }

        private static void AppendRows(
            StringBuilder builder,
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> items)
        {
            builder.AppendLine("## ItemStat Rows");
            builder.AppendLine();
            builder.AppendLine("| Item | Shape | Cells | Stat Profile | Attack | Guard | Spirit | Control | Break | Cleanse | Mana Gain | Mana Cost | Cast Seconds | devOnly | isEnabled |");
            builder.AppendLine("| --- | --- | ---: | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |");
            foreach (BuildSandboxPlacedItemSnapshot item in items)
            {
                BuildSandboxItemStat stat = item?.itemStat;
                builder.AppendLine(
                    $"| `{Escape(item?.itemId)}` | `{Escape(item?.shapeId)}` | {item?.occupiedCells?.Count ?? 0} | `{Escape(stat?.statProfileId)}` | {stat?.attack ?? 0} | {stat?.guard ?? 0} | {stat?.spirit ?? 0} | {stat?.control ?? 0} | {stat?.shieldBreak ?? 0} | {stat?.cleanse ?? 0} | {stat?.manaGainPerTick ?? 0} | {stat?.manaCostPerCast ?? 0} | {(stat?.castIntervalSeconds ?? 0f):0.###} | `{stat?.devOnly ?? false}` | `{stat?.isEnabled ?? false}` |");
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

    public static class BuildSandboxItemIdentityFamilyCorrectionReportWriter
    {
        public const string MainReportPath =
            "Docs/V0.4/Reports/ItemIdentityFamilyCorrectionReport.md";

        public const string MappingCsvPath =
            "Docs/V0.4/Reports/ItemFamilyMapping.csv";

        public const string LeakCheckReportPath =
            "Docs/V0.4/Reports/ItemIdentityFamilyLeakCheckReport.md";

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattleSandboxBuildCombatPreview preview = null)
        {
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();
            BattleSandboxBuildCombatPreview safePreview =
                preview ?? BuildSandboxItemIdentityFamilyCorrectionValidator.BuildDefaultPreview();

            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string mainPath = Path.Combine(projectRoot, MainReportPath);
            string mappingPath = Path.Combine(projectRoot, MappingCsvPath);
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
                mappingPath,
                BuildMappingCsv(),
                new UTF8Encoding(false));
            File.WriteAllText(
                leakPath,
                BuildLeakCheckReport(safeReports, safePreview, errors, warnings, leakCount),
                new UTF8Encoding(false));

            AssetDatabase.Refresh();
            return new[] { mainPath, mappingPath, leakPath };
        }

        private static string BuildMainReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattleSandboxBuildCombatPreview preview,
            int errors,
            int warnings,
            int leakCount)
        {
            IReadOnlyList<BuildSandboxItemIdentityFamilyRecord> basics =
                BuildSandboxItemIdentityFamilyCatalog.BasicItems;
            IReadOnlyList<BuildSandboxItemIdentityFamilyRecord> v04Rows =
                BuildSandboxItemIdentityFamilyCatalog.V04PreviewItems;

            StringBuilder builder = new();
            builder.AppendLine("# Item Identity Family Correction Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BuildSandboxItemIdentityFamilyCorrectionValidator.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 && leakCount == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine($"Leak Count: `{leakCount}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- BuildSandbox devOnly correction only.");
            builder.AppendLine("- V0.2/V0.3 formal base item ids are read-only references and are not edited.");
            builder.AppendLine("- V0.4 preview item ids remain preview ids and are not promoted into formal RunFlow.");
            builder.AppendLine("- No UI layout, scene layout, SaveData, Reward, Chapter, Boss, drop, or formal numeric flow writes.");
            builder.AppendLine();
            builder.AppendLine("## Fields");
            builder.AppendLine();
            builder.AppendLine("| Field | Meaning |");
            builder.AppendLine("| --- | --- |");
            builder.AppendLine("| `itemFamily` | Stable family key used by BuildSandbox identity reports. |");
            builder.AppendLine("| `baseItemId` | Read-only V0.2/V0.3 base reference when one exists; empty for new mechanisms. |");
            builder.AppendLine("| `tier` | One of `basic`, `advanced`, `core`, `support`, `test_only`. |");
            builder.AppendLine("| `relationshipToBase` | One of `base`, `advanced_variant`, `new_mechanic`, `support_variant`, `test_only`. |");
            builder.AppendLine();
            builder.AppendLine("## Counters");
            builder.AppendLine();
            builder.AppendLine($"- basic item rows: `{basics.Count}`");
            builder.AppendLine($"- V0.4 preview item rows: `{v04Rows.Count}`");
            builder.AppendLine($"- placed snapshot rows inspected: `{BuildSandboxItemIdentityFamilyCorrectionValidator.PlacedItems(preview).Count}`");
            builder.AppendLine($"- identity scope leak count: `{BuildSandboxItemIdentityFamilyCorrectionValidator.CountIdentityScopeLeaks(preview)}`");
            builder.AppendLine($"- player-side answer leak count: `{preview?.PlayerSideAnswerLeakCount ?? 1}`");
            builder.AppendLine($"- formal flow leak count: `{preview?.FormalFlowLeakCount ?? 1}`");
            builder.AppendLine($"- feature flag default true count: `{preview?.FeatureFlagDefaultTrueCount ?? 1}`");
            builder.AppendLine();
            AppendRows(builder, "Classification A - V0.2/V0.3 Basic Items", basics);
            AppendRows(builder, "Classification B - V0.4 Advanced / New Mechanism Items", v04Rows);
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildMappingCsv()
        {
            StringBuilder csv = new();
            csv.AppendLine("classification,itemId,displayName,itemFamily,baseItemId,tier,relationshipToBase,devOnly,isEnabled,sourceScope,notes");
            foreach (BuildSandboxItemIdentityFamilyRecord row in BuildSandboxItemIdentityFamilyCatalog.AllItems)
            {
                csv.AppendLine(Csv(
                    row.Classification,
                    row.ItemId,
                    row.DisplayName,
                    row.ItemFamily,
                    row.BaseItemId,
                    row.Tier,
                    row.RelationshipToBase,
                    row.DevOnly.ToString(),
                    row.IsEnabled.ToString(),
                    row.SourceScope,
                    row.Notes));
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
            builder.AppendLine("# Item Identity Family Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BuildSandboxItemIdentityFamilyCorrectionValidator.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 && leakCount == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine($"Leak Count: `{leakCount}`");
            builder.AppendLine();
            builder.AppendLine("## Boundary Checks");
            builder.AppendLine();
            builder.AppendLine("| Check | Status | Detail |");
            builder.AppendLine("| --- | --- | --- |");
            AppendCheck(builder, "Feature flags default false", BuildSandboxFeatureFlags.AreAllDefaultsDisabled(), nameof(BuildSandboxFeatureFlags));
            AppendCheck(builder, "Preview source stays devOnly and disabled", preview != null && preview.devOnly && !preview.isEnabled, nameof(BattleSandboxBuildCombatPreview));
            AppendCheck(builder, "Preview context avoids formal save/flow/data/scene writes", ContextIsIsolated(preview?.context), nameof(BuildSandboxPreviewContext));
            AppendCheck(builder, "V0.4 preview ids do not equal base item ids", V04RowsDoNotReplaceBaseIds(), nameof(BuildSandboxItemIdentityFamilyCatalog));
            AppendCheck(builder, "V0.2/V0.3 basics remain classified as base", BasicRowsRemainBase(), nameof(BuildSandboxItemIdentityFamilyCatalog));
            AppendCheck(builder, "Old preview incense name removed", PreviewIncenseNameCorrected(), nameof(BuildGridInteractionPreviewController));
            builder.AppendLine();
            builder.AppendLine("## Forbidden Scope");
            builder.AppendLine();
            builder.AppendLine("- Formal V0.2/V0.3 assets: not modified by this package.");
            builder.AppendLine("- Old itemId replacement: not performed; V0.4 rows keep preview ids.");
            builder.AppendLine("- Preview item deletion: not performed.");
            builder.AppendLine("- UI / scene layout: not modified.");
            builder.AppendLine("- Formal RunFlow / SaveData / Reward / Chapter: not connected.");
            builder.AppendLine();
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static int CountLeaks(BattleSandboxBuildCombatPreview preview)
        {
            return (preview?.FeatureFlagDefaultTrueCount ?? 1)
                + (preview?.FormalFlowLeakCount ?? 1)
                + (preview?.PlayerSideAnswerLeakCount ?? 1)
                + BuildSandboxItemIdentityFamilyCorrectionValidator.CountIdentityScopeLeaks(preview)
                + BuildSandboxItemIdentityFamilyCatalog.V04PreviewItems.Count(row =>
                    !string.IsNullOrWhiteSpace(row.BaseItemId)
                    && string.Equals(row.ItemId, row.BaseItemId, StringComparison.Ordinal));
        }

        private static void AppendRows(
            StringBuilder builder,
            string title,
            IReadOnlyList<BuildSandboxItemIdentityFamilyRecord> rows)
        {
            builder.AppendLine($"## {title}");
            builder.AppendLine();
            builder.AppendLine("| Item | itemId | itemFamily | baseItemId | tier | relationshipToBase | Scope |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- |");
            foreach (BuildSandboxItemIdentityFamilyRecord row in rows)
            {
                builder.AppendLine(
                    $"| {Escape(row.DisplayName)} | `{Escape(row.ItemId)}` | `{Escape(row.ItemFamily)}` | `{Escape(row.BaseItemId)}` | `{Escape(row.Tier)}` | `{Escape(row.RelationshipToBase)}` | `{Escape(row.SourceScope)}` |");
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

            builder.AppendLine();
        }

        private static void AppendCheck(
            StringBuilder builder,
            string name,
            bool passed,
            string detail)
        {
            builder.AppendLine($"| {Escape(name)} | `{(passed ? "PASS" : "FAIL")}` | `{Escape(detail)}` |");
        }

        private static bool ContextIsIsolated(BuildSandboxPreviewContext context)
        {
            return context != null
                && context.devOnly
                && !context.isEnabled
                && !context.readsFormalSaveData
                && !context.writesFormalFlow
                && !context.writesFormalData
                && !context.touchesFormalScene;
        }

        private static bool V04RowsDoNotReplaceBaseIds()
        {
            return BuildSandboxItemIdentityFamilyCatalog.V04PreviewItems.All(row =>
                string.IsNullOrWhiteSpace(row.BaseItemId)
                || !string.Equals(row.ItemId, row.BaseItemId, StringComparison.Ordinal));
        }

        private static bool BasicRowsRemainBase()
        {
            return BuildSandboxItemIdentityFamilyCatalog.BasicItems.All(row =>
                string.Equals(row.Tier, BuildSandboxItemIdentityFamilyCatalog.TierBasic, StringComparison.Ordinal)
                && string.Equals(row.RelationshipToBase, BuildSandboxItemIdentityFamilyCatalog.RelationshipBase, StringComparison.Ordinal)
                && string.Equals(row.ItemId, row.BaseItemId, StringComparison.Ordinal));
        }

        private static bool PreviewIncenseNameCorrected()
        {
            return BuildGridInteractionPreviewController.CreatePreviewItems().Any(item =>
                string.Equals(item.ItemId, "preview_energy_incense", StringComparison.Ordinal)
                && string.Equals(item.DisplayName, "醒符香", StringComparison.Ordinal));
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
