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
    public static class MechanicHintFeedbackPreviewReportWriter
    {
        public const string MainReportPath =
            "Docs/V0.4/Reports/MechanicHintFeedbackPreviewReport.md";

        public const string RowReportPath =
            "Docs/V0.4/Reports/MechanicHintFeedbackPreviewRows.csv";

        public const string LeakCheckReportPath =
            "Docs/V0.4/Reports/MechanicHintFeedbackPreviewLeakCheckReport.md";

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            MechanicHintFeedbackPreview preview = null)
        {
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();
            MechanicHintFeedbackPreview safePreview =
                preview ?? MechanicHintFeedbackPreviewValidator.BuildDefaultPreview();

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
            MechanicHintFeedbackPreview preview,
            int errors,
            int warnings,
            int leakCount)
        {
            StringBuilder builder = new();
            builder.AppendLine("# Mechanic Hint Feedback Preview Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{MechanicHintFeedbackPreview.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 && leakCount == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine($"Leak Count: `{leakCount}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Adds a BuildSandbox/devOnly preview for map mechanics, enemy mechanics, boss skills, weakness windows, drop-bias atmosphere, and failure feedback.");
            builder.AppendLine("- Reuses mature battle feedback channels: `BattleHint`, `DamageFeedback`, `CombatLog`, `Tooltip`, and `BossInfo`.");
            builder.AppendLine("- Player-side rows contain Chinese hint/feedback/atmosphere copy only.");
            builder.AppendLine("- Full solution fields remain linked to the developer data panel and are masked from player UI.");
            builder.AppendLine("- Does not write formal RunFlow, SaveData, Boss, reward, drop, numeric config, V02/V03 scenes, or hand-tuned RectTransforms.");
            builder.AppendLine();
            builder.AppendLine("## Preview Summary");
            builder.AppendLine();
            builder.AppendLine($"- Reference mode: `{Escape(preview?.referenceMode)}`");
            builder.AppendLine($"- Source preview build id: `{Escape(preview?.sourcePreviewBuildId)}`");
            builder.AppendLine($"- Player visible rows: `{preview?.PlayerVisibleRowCount}`");
            builder.AppendLine($"- Reuse channels: `{preview?.ReuseChannelCount}`");
            builder.AppendLine($"- Developer data panel links: `{preview?.DeveloperLinkCount}`");
            builder.AppendLine($"- Feedback extension rows: `{preview?.feedbackExtension?.rows?.Count ?? 0}`");
            builder.AppendLine();
            builder.AppendLine("## Category Coverage");
            builder.AppendLine();
            builder.AppendLine("| Category | Rows |");
            builder.AppendLine("| --- | ---: |");
            foreach (IGrouping<string, MechanicHintFeedbackPreviewRow> group in Rows(preview)
                         .GroupBy(row => row.category)
                         .OrderBy(group => group.Key, StringComparer.Ordinal))
            {
                builder.AppendLine($"| `{Escape(group.Key)}` | {group.Count()} |");
            }

            builder.AppendLine();
            builder.AppendLine("## Reuse Channels");
            builder.AppendLine();
            builder.AppendLine("| Channel | Rows |");
            builder.AppendLine("| --- | ---: |");
            foreach (IGrouping<string, MechanicHintFeedbackPreviewRow> group in Rows(preview)
                         .GroupBy(row => row.reuseChannel)
                         .OrderBy(group => group.Key, StringComparer.Ordinal))
            {
                builder.AppendLine($"| `{Escape(group.Key)}` | {group.Count()} |");
            }

            builder.AppendLine();
            builder.AppendLine("## Player-Side Samples");
            builder.AppendLine();
            builder.AppendLine("| Key | Category | Reuse Channel | Chinese Display | Player Text | Atmosphere |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- |");
            foreach (MechanicHintFeedbackPreviewRow row in Rows(preview).Take(12))
            {
                builder.AppendLine(
                    $"| `{Escape(row.englishStableKey)}` | `{Escape(row.category)}` | `{Escape(row.reuseChannel)}` | {Escape(row.chineseDisplayName)} | {Escape(row.playerChineseText)} | {Escape(row.atmosphereChinese)} |");
            }

            builder.AppendLine();
            builder.AppendLine("## Developer Data Panel Links");
            builder.AppendLine();
            builder.AppendLine("| English Stable Key | Chinese Display Name | Data Panel Slot | Source | Player Visible | Masked From Player |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- |");
            foreach (MechanicHintDeveloperDataPanelLink link in preview?.developerDataPanelLinks
                         ?? new List<MechanicHintDeveloperDataPanelLink>())
            {
                builder.AppendLine(
                    $"| `{Escape(link.englishStableKey)}` | {Escape(link.chineseDisplayName)} | `{Escape(link.dataPanelSlot)}` | `{Escape(link.sourceDataPath)}` | `{link.playerVisible}` | `{link.maskedFromPlayer}` |");
            }

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildRowsCsv(MechanicHintFeedbackPreview preview)
        {
            StringBuilder csv = new();
            csv.AppendLine("englishStableKey,chineseDisplayName,category,reuseChannel,reuseSurface,playerChineseText,atmosphereChinese,sourceDataPath,developerDataPanelFieldKey,playerVisible,maskedFromPlayer,developerPanelVisible,canWriteFormalUi,createsDedicatedPanel,exposesFullAnswerToPlayer");
            foreach (MechanicHintFeedbackPreviewRow row in Rows(preview))
            {
                csv.AppendLine(Csv(
                    row.englishStableKey,
                    row.chineseDisplayName,
                    row.category,
                    row.reuseChannel,
                    row.reuseSurface,
                    row.playerChineseText,
                    row.atmosphereChinese,
                    row.sourceDataPath,
                    row.developerDataPanelFieldKey,
                    row.playerVisible.ToString(),
                    row.maskedFromPlayer.ToString(),
                    row.developerPanelVisible.ToString(),
                    row.canWriteFormalUi.ToString(),
                    row.createsDedicatedPanel.ToString(),
                    row.exposesFullAnswerToPlayer.ToString()));
            }

            csv.AppendLine();
            csv.AppendLine("developerKey,chineseDisplayName,dataPanelSlot,sourceDataPath,developerVisible,playerVisible,maskedFromPlayer");
            foreach (MechanicHintDeveloperDataPanelLink link in preview?.developerDataPanelLinks
                         ?? new List<MechanicHintDeveloperDataPanelLink>())
            {
                csv.AppendLine(Csv(
                    link.englishStableKey,
                    link.chineseDisplayName,
                    link.dataPanelSlot,
                    link.sourceDataPath,
                    link.developerVisible.ToString(),
                    link.playerVisible.ToString(),
                    link.maskedFromPlayer.ToString()));
            }

            return csv.ToString();
        }

        private static string BuildLeakCheckReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            MechanicHintFeedbackPreview preview,
            int errors,
            int warnings,
            int leakCount)
        {
            int featureFlagDefaultTrue = BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue);
            int isolationLeaks = CountIsolationLeaks(preview);
            int rowLeaks = CountRowLeaks(preview);
            int playerTextLeaks = CountPlayerTextLeaks(preview);
            int developerLinkLeaks = CountDeveloperLinkLeaks(preview);

            StringBuilder builder = new();
            builder.AppendLine("# Mechanic Hint Feedback Preview Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{MechanicHintFeedbackPreview.PackageName}`");
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
            builder.AppendLine($"| `previewIsolationLeaks` | {isolationLeaks} | 0 |");
            builder.AppendLine($"| `rowScopeLeaks` | {rowLeaks} | 0 |");
            builder.AppendLine($"| `playerTextLeaks` | {playerTextLeaks} | 0 |");
            builder.AppendLine($"| `developerLinkLeaks` | {developerLinkLeaks} | 0 |");
            builder.AppendLine($"| `totalLeaks` | {leakCount} | 0 |");
            builder.AppendLine();
            builder.AppendLine("## Formal Scope Confirmation");
            builder.AppendLine();
            builder.AppendLine("- Feature flags remain default false.");
            builder.AppendLine("- The preview remains BuildSandbox/devOnly, disabled by default, and data-only.");
            builder.AppendLine("- Existing battle hint, tooltip, combat log, damage feedback, and BossInfo language are referenced as reuse channels only.");
            builder.AppendLine("- No new mechanism UI frame or dedicated player panel is created.");
            builder.AppendLine("- hardSolutionTags, requiredSynergy, requiredAffix, requiredStats, dropBias weights, and boss six-key answers stay in developer data panel links.");
            builder.AppendLine("- Formal RunFlow, PageState, FormationState, SaveData, Boss, rewards, drops, numeric data, V02/V03 scenes, and hand-tuned RectTransforms are not touched.");

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static int CountLeaks(MechanicHintFeedbackPreview preview)
        {
            return BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue)
                + CountIsolationLeaks(preview)
                + CountRowLeaks(preview)
                + CountPlayerTextLeaks(preview)
                + CountDeveloperLinkLeaks(preview);
        }

        private static int CountIsolationLeaks(MechanicHintFeedbackPreview preview)
        {
            if (preview == null)
            {
                return 1;
            }

            int leaks = 0;
            if (!preview.devOnly) leaks++;
            if (preview.isEnabled) leaks++;
            if (preview.writesFormalUi) leaks++;
            if (preview.writesFormalScene) leaks++;
            if (preview.changesHandTunedLayout) leaks++;
            if (preview.createsMechanicUiFrame) leaks++;
            if (preview.createsDedicatedMechanicPanels) leaks++;
            if (preview.touchesRunFlow) leaks++;
            if (preview.touchesPageState) leaks++;
            if (preview.touchesFormationState) leaks++;
            if (preview.touchesSaveData) leaks++;
            if (preview.touchesBossRewardDropNumeric) leaks++;
            if (!preview.playerUiChineseOnly) leaks++;
            if (preview.playerUiShowsEnglishStableKey) leaks++;
            if (preview.playerUiShowsFullAnswers) leaks++;
            if (!preview.developerFullAnswersStayInDataPanel) leaks++;
            if (preview.feedbackExtension == null
                || preview.feedbackExtension.canWriteFormalUi
                || !preview.feedbackExtension.requiresExplicitRuntimeBinding)
            {
                leaks++;
            }

            return leaks;
        }

        private static int CountRowLeaks(MechanicHintFeedbackPreview preview)
        {
            return Rows(preview).Count(row =>
                !row.playerVisible
                || row.maskedFromPlayer
                || !row.developerPanelVisible
                || row.canWriteFormalUi
                || row.createsDedicatedPanel
                || row.exposesFullAnswerToPlayer);
        }

        private static int CountPlayerTextLeaks(MechanicHintFeedbackPreview preview)
        {
            string[] forbidden =
            {
                "hardSolutionTags",
                "requiredSynergy",
                "requiredAffix",
                "requiredStats",
                "DropBias",
                "Boss",
                "previewWeight",
                "keyRequirements",
                "BuildProblemSeedDataset"
            };

            int leaks = 0;
            foreach (MechanicHintFeedbackPreviewRow row in Rows(preview))
            {
                foreach (string text in new[] { row.playerChineseText, row.atmosphereChinese })
                {
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        leaks++;
                        continue;
                    }

                    if (text.Any(character => character <= 127 && char.IsLetter(character)))
                    {
                        leaks++;
                    }

                    if (forbidden.Any(token => text.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        leaks++;
                    }
                }
            }

            return leaks;
        }

        private static int CountDeveloperLinkLeaks(MechanicHintFeedbackPreview preview)
        {
            IReadOnlyList<MechanicHintDeveloperDataPanelLink> links =
                preview != null && preview.developerDataPanelLinks != null
                    ? preview.developerDataPanelLinks
                    : Array.Empty<MechanicHintDeveloperDataPanelLink>();
            int leaks = MechanicHintFeedbackPreviewBuilder.SensitiveKeys.Count(required =>
                !links.Any(link => link != null
                    && string.Equals(link.englishStableKey, required, StringComparison.Ordinal)));
            leaks += links.Count(link => link == null
                || !link.developerVisible
                || link.playerVisible
                || !link.maskedFromPlayer);
            return leaks;
        }

        private static IEnumerable<MechanicHintFeedbackPreviewRow> Rows(
            MechanicHintFeedbackPreview preview)
        {
            return preview?.playerRows?
                .Where(row => row != null)
                ?? Enumerable.Empty<MechanicHintFeedbackPreviewRow>();
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
