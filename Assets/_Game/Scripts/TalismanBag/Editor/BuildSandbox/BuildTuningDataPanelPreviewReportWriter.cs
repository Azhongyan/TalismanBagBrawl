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
    public static class BuildTuningDataPanelPreviewReportWriter
    {
        private const string PanelMenuPath =
            "Tools/Talisman Bag/V0.4/BuildSandbox/Data/[Manual Only] Build Tuning Data Panel Preview 01";

        private const string PanelWindowTitle =
            "Build Tuning Data Panel Preview 01";

        public const string MainReportPath =
            "Docs/V0.4/Reports/BuildTuningDataPanelPreviewReport.md";

        public const string FieldReportPath =
            "Docs/V0.4/Reports/BuildTuningDataPanelFieldReport.csv";

        public const string LeakCheckReportPath =
            "Docs/V0.4/Reports/BuildTuningDataPanelLeakCheckReport.md";

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BuildTuningDataPanelPreview preview = null)
        {
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();
            BuildTuningDataPanelPreview safePreview =
                preview ?? BuildTuningDataPanelPreviewValidator.BuildDefaultPreview();
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string mainPath = Path.Combine(projectRoot, MainReportPath);
            string fieldPath = Path.Combine(projectRoot, FieldReportPath);
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
                fieldPath,
                BuildFieldCsv(safePreview),
                new UTF8Encoding(false));
            File.WriteAllText(
                leakPath,
                BuildLeakCheckReport(safeReports, safePreview, errors, warnings, leakCount),
                new UTF8Encoding(false));

            AssetDatabase.Refresh();
            return new[] { mainPath, fieldPath, leakPath };
        }

        private static string BuildMainReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BuildTuningDataPanelPreview preview,
            int errors,
            int warnings,
            int leakCount)
        {
            StringBuilder builder = new();
            builder.AppendLine("# Build Tuning Data Panel Preview Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BuildTuningDataPanelPreview.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 && leakCount == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine($"Leak Count: `{leakCount}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Developer tuning data panel only; not a player formal UI.");
            builder.AppendLine("- Reads V0.4 BuildSandbox PreviewContext and BattlePageViewSpec field bindings.");
            builder.AppendLine("- Every section and field row carries a Chinese display name plus an English stable key.");
            builder.AppendLine("- Full answers stay in developer/report/config layers; player UI does not show English keys or full answers.");
            builder.AppendLine("- Does not create mechanism-specific UI frames, write V02/V03 formal scenes, or change hand-tuned layout.");
            builder.AppendLine();
            builder.AppendLine("## Panel Summary");
            builder.AppendLine();
            builder.AppendLine($"- Reference mode: `{Escape(preview.referenceMode)}`");
            builder.AppendLine($"- Source context package: `{Escape(preview.sourcePreviewContextPackageName)}`");
            builder.AppendLine($"- Source preview build id: `{Escape(preview.sourcePreviewBuildId)}`");
            builder.AppendLine($"- Menu path: `{PanelMenuPath}`");
            builder.AppendLine($"- Window title: `{PanelWindowTitle}`");
            builder.AppendLine($"- Sections: `{preview.SectionCount}`");
            builder.AppendLine($"- Field rows: `{preview.FieldCount}`");
            builder.AppendLine($"- Developer-visible fields: `{preview.DeveloperVisibleFieldCount}`");
            builder.AppendLine($"- Player-visible fields: `{preview.PlayerVisibleFieldCount}`");
            builder.AppendLine($"- Masked-from-player fields: `{preview.MaskedFieldCount}`");
            builder.AppendLine();
            builder.AppendLine("## Sections");
            builder.AppendLine();
            builder.AppendLine("| English Stable Key | Chinese Display Name | Data Panel Slot | Source | Rows | Formal UI Write | Mechanic Frame |");
            builder.AppendLine("| --- | --- | --- | --- | ---: | --- | --- |");
            foreach (BuildTuningDataPanelSection section in preview.sections ?? new List<BuildTuningDataPanelSection>())
            {
                builder.AppendLine(
                    $"| `{Escape(section.englishStableKey)}` | {Escape(section.chineseDisplayName)} | `{Escape(section.dataPanelSlot)}` | `{Escape(section.sourceViewModelKey)}` | {section.rows.Count} | `{section.canWriteFormalUi}` | `{section.createsMechanicUiFrame}` |");
            }
            builder.AppendLine();
            builder.AppendLine("## Required Masked Developer Fields");
            builder.AppendLine();
            builder.AppendLine("| English Stable Key | Chinese Display Name | Present | Masked From Player |");
            builder.AppendLine("| --- | --- | --- | --- |");
            foreach (string key in new[]
            {
                "hardSolutionTags",
                "requiredSynergy",
                "requiredAffix",
                "requiredStats",
                "dropBiasWeights",
                "bossSixKeyFullAnswer"
            })
            {
                BuildTuningDataPanelFieldRow row =
                    preview.Rows.FirstOrDefault(field => string.Equals(field.englishStableKey, key, StringComparison.Ordinal));
                builder.AppendLine(
                    $"| `{key}` | {Escape(row?.chineseDisplayName ?? string.Empty)} | `{(row != null)}` | `{row?.maskedFromPlayer}` |");
            }

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildFieldCsv(BuildTuningDataPanelPreview preview)
        {
            StringBuilder csv = new();
            csv.AppendLine("sectionEnglishStableKey,sectionChineseDisplayName,dataPanelSlot,englishStableKey,chineseDisplayName,value,sourceDataPath,developerVisible,playerVisible,maskedFromPlayer,reportOnly,devOnly");
            foreach (BuildTuningDataPanelSection section in preview.sections ?? new List<BuildTuningDataPanelSection>())
            {
                foreach (BuildTuningDataPanelFieldRow row in section.rows ?? new List<BuildTuningDataPanelFieldRow>())
                {
                    csv.AppendLine(Csv(
                        section.englishStableKey,
                        section.chineseDisplayName,
                        section.dataPanelSlot,
                        row.englishStableKey,
                        row.chineseDisplayName,
                        row.value,
                        row.sourceDataPath,
                        row.developerVisible.ToString(),
                        row.playerVisible.ToString(),
                        row.maskedFromPlayer.ToString(),
                        row.reportOnly.ToString(),
                        row.devOnly.ToString()));
                }
            }

            return csv.ToString();
        }

        private static string BuildLeakCheckReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BuildTuningDataPanelPreview preview,
            int errors,
            int warnings,
            int leakCount)
        {
            int featureFlagDefaultTrue = BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue);
            int isolationLeaks = CountIsolationLeaks(preview);
            int sectionLeaks = CountSectionLeaks(preview);
            int fieldLeaks = CountFieldLeaks(preview);
            int sensitiveLeaks = CountSensitiveLeaks(preview);

            StringBuilder builder = new();
            builder.AppendLine("# Build Tuning Data Panel Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BuildTuningDataPanelPreview.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 && leakCount == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine();
            builder.AppendLine("## Leak Counters");
            builder.AppendLine();
            builder.AppendLine("| Check | Count | Expected |");
            builder.AppendLine("| --- | ---: | ---: |");
            builder.AppendLine($"| `featureFlagDefaultTrue` | {featureFlagDefaultTrue} | 0 |");
            builder.AppendLine($"| `panelIsolationLeaks` | {isolationLeaks} | 0 |");
            builder.AppendLine($"| `sectionScopeLeaks` | {sectionLeaks} | 0 |");
            builder.AppendLine($"| `fieldScopeLeaks` | {fieldLeaks} | 0 |");
            builder.AppendLine($"| `sensitiveFieldLeaks` | {sensitiveLeaks} | 0 |");
            builder.AppendLine($"| `totalLeaks` | {leakCount} | 0 |");
            builder.AppendLine();
            builder.AppendLine("## Formal Scope Confirmation");
            builder.AppendLine();
            builder.AppendLine("- Player formal UI: not created.");
            builder.AppendLine("- Mechanism UI frames: not created.");
            builder.AppendLine("- V02/V03 formal scenes: not written.");
            builder.AppendLine("- Hand-tuned layout: not changed.");
            builder.AppendLine("- Formal save, flow, reward, drop, Boss, numeric, and combat data: not connected.");
            builder.AppendLine("- English stable keys remain in config/report/developer panel only.");
            builder.AppendLine("- hardSolutionTags, requiredSynergy, requiredAffix, requiredStats, DropBias weights, and Boss six-key full answer are masked from player UI.");

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static int CountLeaks(BuildTuningDataPanelPreview preview)
        {
            return BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue)
                + CountIsolationLeaks(preview)
                + CountSectionLeaks(preview)
                + CountFieldLeaks(preview)
                + CountSensitiveLeaks(preview);
        }

        private static int CountIsolationLeaks(BuildTuningDataPanelPreview preview)
        {
            if (preview == null)
            {
                return 1;
            }

            int leaks = 0;
            if (!preview.devOnly) leaks++;
            if (preview.isEnabled) leaks++;
            if (preview.playerFormalUi) leaks++;
            if (preview.writesFormalUi) leaks++;
            if (preview.writesFormalScene) leaks++;
            if (preview.readsFormalSaveData) leaks++;
            if (preview.writesFormalData) leaks++;
            if (preview.createsMechanicUiFrame) leaks++;
            if (preview.changesHandTunedLayout) leaks++;
            if (preview.playerUiShowsEnglishStableKey) leaks++;
            if (preview.playerUiShowsFullAnswers) leaks++;
            return leaks;
        }

        private static int CountSectionLeaks(BuildTuningDataPanelPreview preview)
        {
            return (preview?.sections ?? new List<BuildTuningDataPanelSection>())
                .Count(section => section == null
                    || !section.developerOnly
                    || section.playerVisible
                    || !section.referenceOnly
                    || section.canWriteFormalUi
                    || section.createsMechanicUiFrame);
        }

        private static int CountFieldLeaks(BuildTuningDataPanelPreview preview)
        {
            return (preview?.Rows ?? Array.Empty<BuildTuningDataPanelFieldRow>())
                .Count(row => row == null
                    || !row.developerVisible
                    || row.playerVisible
                    || !row.reportOnly
                    || !row.devOnly);
        }

        private static int CountSensitiveLeaks(BuildTuningDataPanelPreview preview)
        {
            string[] sensitiveKeys =
            {
                "hardSolutionTags",
                "requiredSynergy",
                "requiredAffix",
                "requiredStats",
                "dropBiasWeights",
                "bossSixKeyFullAnswer"
            };

            IReadOnlyList<BuildTuningDataPanelFieldRow> rows =
                preview?.Rows ?? Array.Empty<BuildTuningDataPanelFieldRow>();
            int missing = sensitiveKeys.Count(required =>
                !rows.Any(row => row != null && string.Equals(row.englishStableKey, required, StringComparison.Ordinal)));
            int leaks = rows.Count(row => row != null
                && sensitiveKeys.Any(required =>
                    string.Equals(row.englishStableKey, required, StringComparison.Ordinal)
                    || row.englishStableKey.StartsWith(required + ".", StringComparison.Ordinal))
                && (!row.maskedFromPlayer || row.playerVisible || !row.developerVisible));
            return missing + leaks;
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
