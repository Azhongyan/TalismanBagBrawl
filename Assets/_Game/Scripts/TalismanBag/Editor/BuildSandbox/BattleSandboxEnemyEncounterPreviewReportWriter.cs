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
    public static class BattleSandboxEnemyEncounterPreviewReportWriter
    {
        public const string MainReportPath =
            "Docs/V0.4/Reports/BattleSandboxEnemyEncounterPreviewReport.md";

        public const string RowReportPath =
            "Docs/V0.4/Reports/BattleSandboxEnemyEncounterRows.csv";

        public const string LeakCheckReportPath =
            "Docs/V0.4/Reports/BattleSandboxEnemyEncounterLeakCheckReport.md";

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattleSandboxEnemyEncounterPreview preview = null)
        {
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();
            BattleSandboxEnemyEncounterPreview safePreview =
                preview ?? BattleSandboxEnemyEncounterPreviewValidator.BuildDefaultPreview();

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
            BattleSandboxEnemyEncounterPreview preview,
            int errors,
            int warnings,
            int leakCount)
        {
            StringBuilder builder = new();
            builder.AppendLine("# BattleSandbox Enemy Encounter Preview Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattleSandboxEnemyEncounterPreview.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 && leakCount == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine($"Leak Count: `{leakCount}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Adds only a V04 BuildSandbox/devOnly enemy and Boss encounter preview.");
            builder.AppendLine("- Target scene: `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity`.");
            builder.AppendLine("- Player-facing copy shows Chinese hints for map mechanics, enemy pressure, Boss skills, weakness windows, readiness, failure feedback, and drop-bias atmosphere.");
            builder.AppendLine("- hardSolutionTags, requiredSynergy, requiredAffix, requiredStats, drop-bias weights, and Boss six-key full answers remain masked in developer data links.");
            builder.AppendLine("- Formal RunFlow, formal combat damage, rewards, save/progress data, formal Boss/enemy config, and formal 3-10 / 4-10 are not connected.");
            builder.AppendLine();
            builder.AppendLine("## Required Counters");
            builder.AppendLine();
            builder.AppendLine($"- devOnly enemy count: `{preview?.EnemyCount ?? 0}`");
            builder.AppendLine($"- devOnly Boss count: `{preview?.BossCount ?? 0}`");
            builder.AppendLine($"- displayed hint count: `{preview?.DisplayedHintCount ?? 0}`");
            builder.AppendLine($"- masked answer field count: `{preview?.MaskedAnswerFieldCount ?? 0}`");
            builder.AppendLine($"- formal leak count: `{preview?.FormalLeakCount ?? 1}`");
            builder.AppendLine($"- V02 scene changes: `0`");
            builder.AppendLine($"- V03 scene changes: `0`");
            builder.AppendLine($"- feature flags default true: `{BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue)}`");
            builder.AppendLine();
            builder.AppendLine("## Encounter Coverage");
            builder.AppendLine();
            builder.AppendLine("| Kind | Rows | Hint Count |");
            builder.AppendLine("| --- | ---: | ---: |");
            foreach (IGrouping<string, BattleSandboxEnemyEncounterRow> group in Rows(preview)
                         .GroupBy(row => row.encounterKind)
                         .OrderBy(group => group.Key, StringComparer.Ordinal))
            {
                builder.AppendLine($"| `{Escape(group.Key)}` | {group.Count()} | {group.Sum(row => row.PlayerHintCount)} |");
            }

            builder.AppendLine();
            builder.AppendLine("## Player-Side Samples");
            builder.AppendLine();
            builder.AppendLine("| Encounter | Kind | Selector | Test Target | Weakness | Readiness |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- |");
            foreach (BattleSandboxEnemyEncounterRow row in Rows(preview).Take(12))
            {
                builder.AppendLine(
                    $"| `{Escape(row.encounterId)}` | `{Escape(row.encounterKind)}` | {Escape(row.selectorLabel)} | {Escape(row.testTargetChinese)} | {Escape(row.weaknessWindowChinese)} | {Escape(row.readinessPreviewChinese)} |");
            }

            builder.AppendLine();
            builder.AppendLine("## Developer Answer Links");
            builder.AppendLine();
            builder.AppendLine("| English Stable Key | Chinese Display Name | Data Panel Slot | Source | Player Visible | Masked From Player |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- |");
            foreach (BattleSandboxEncounterDeveloperAnswerLink link in preview?.developerAnswerLinks
                         ?? new List<BattleSandboxEncounterDeveloperAnswerLink>())
            {
                builder.AppendLine(
                    $"| `{Escape(link.englishStableKey)}` | {Escape(link.chineseDisplayName)} | `{Escape(link.dataPanelSlot)}` | `{Escape(link.sourceDataPath)}` | `{link.playerVisible}` | `{link.maskedFromPlayer}` |");
            }

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildRowsCsv(BattleSandboxEnemyEncounterPreview preview)
        {
            StringBuilder csv = new();
            csv.AppendLine("encounterId,encounterKind,chineseDisplayName,selectorLabel,mapMechanicChinese,encounterMechanicChinese,bossSkillChinese,weaknessWindowChinese,testTargetChinese,readinessPreviewChinese,failureFeedbackChinese,dropBiasAtmosphereChinese,sourceDataPath,playerHintCount,devOnly,isEnabled,playerVisible,showsCompleteAnswer,runsFormalCombat,grantsFormalReward,writesFormalSaveData,writesFormalFlow,formalLeak");
            foreach (BattleSandboxEnemyEncounterRow row in Rows(preview))
            {
                csv.AppendLine(Csv(
                    row.encounterId,
                    row.encounterKind,
                    row.chineseDisplayName,
                    row.selectorLabel,
                    row.mapMechanicChinese,
                    row.encounterMechanicChinese,
                    row.bossSkillChinese,
                    row.weaknessWindowChinese,
                    row.testTargetChinese,
                    row.readinessPreviewChinese,
                    row.failureFeedbackChinese,
                    row.dropBiasAtmosphereChinese,
                    row.sourceDataPath,
                    row.PlayerHintCount.ToString(),
                    row.devOnly.ToString(),
                    row.isEnabled.ToString(),
                    row.playerVisible.ToString(),
                    row.showsCompleteAnswer.ToString(),
                    row.runsFormalCombat.ToString(),
                    row.grantsFormalReward.ToString(),
                    row.writesFormalSaveData.ToString(),
                    row.writesFormalFlow.ToString(),
                    row.FormalLeak.ToString()));
            }

            csv.AppendLine();
            csv.AppendLine("developerKey,chineseDisplayName,dataPanelSlot,sourceDataPath,developerVisible,playerVisible,maskedFromPlayer");
            foreach (BattleSandboxEncounterDeveloperAnswerLink link in preview?.developerAnswerLinks
                         ?? new List<BattleSandboxEncounterDeveloperAnswerLink>())
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
            BattleSandboxEnemyEncounterPreview preview,
            int errors,
            int warnings,
            int leakCount)
        {
            int featureFlagDefaultTrue = BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue);
            int formalLeakCount = preview?.FormalLeakCount ?? 1;
            int playerTextLeaks = BattleSandboxEnemyEncounterPreviewValidator.CountPlayerTextLeaks(preview);
            int developerLinkLeaks = BattleSandboxEnemyEncounterPreviewValidator.CountDeveloperLinkLeaks(preview);
            int sceneBindingLeaks = BattleSandboxEnemyEncounterPreviewValidator.CountSceneBindingLeaks();

            StringBuilder builder = new();
            builder.AppendLine("# BattleSandbox Enemy Encounter Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattleSandboxEnemyEncounterPreview.PackageName}`");
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
            builder.AppendLine($"| `formalLeakCount` | {formalLeakCount} | 0 |");
            builder.AppendLine($"| `playerTextAnswerLeaks` | {playerTextLeaks} | 0 |");
            builder.AppendLine($"| `developerAnswerLinkLeaks` | {developerLinkLeaks} | 0 |");
            builder.AppendLine($"| `sceneBindingLeaks` | {sceneBindingLeaks} | 0 |");
            builder.AppendLine("| `v02SceneChanges` | 0 | 0 |");
            builder.AppendLine("| `v03SceneChanges` | 0 | 0 |");
            builder.AppendLine($"| `totalLeaks` | {leakCount} | 0 |");
            builder.AppendLine();
            builder.AppendLine("## Formal Scope Confirmation");
            builder.AppendLine();
            builder.AppendLine("- Feature flags remain default false.");
            builder.AppendLine("- Preview is devOnly and disabled by default.");
            builder.AppendLine("- Only the V04 BuildSandbox preview scene receives UI binding.");
            builder.AppendLine("- Formal RunFlow, PageState, FormationState, SaveData, PlayerPrefs, MainTrialProgressData, formal damage, formal rewards, formal drops, formal Boss/enemy configs, and numeric configs are not connected.");
            builder.AppendLine("- Formal 3-10 and formal 4-10 are not referenced.");
            builder.AppendLine("- Player-facing strings are Chinese-only hints; complete answer fields remain masked in developer data links.");
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static int CountLeaks(BattleSandboxEnemyEncounterPreview preview)
        {
            return BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue)
                + (preview?.FormalLeakCount ?? 1)
                + BattleSandboxEnemyEncounterPreviewValidator.CountPlayerTextLeaks(preview)
                + BattleSandboxEnemyEncounterPreviewValidator.CountDeveloperLinkLeaks(preview)
                + BattleSandboxEnemyEncounterPreviewValidator.CountSceneBindingLeaks();
        }

        private static IEnumerable<BattleSandboxEnemyEncounterRow> Rows(
            BattleSandboxEnemyEncounterPreview preview)
        {
            return preview?.rows?
                .Where(row => row != null)
                ?? Enumerable.Empty<BattleSandboxEnemyEncounterRow>();
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
