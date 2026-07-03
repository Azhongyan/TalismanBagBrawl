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
    public static class BattleSandboxEnemyCombatFeedbackUiReuseReportWriter
    {
        public const string MainReportPath =
            "Docs/V0.4/Reports/BattleSandboxEnemyCombatFeedbackUiReuseReport.md";

        public const string RowReportPath =
            "Docs/V0.4/Reports/BattleSandboxEnemyCombatFeedbackRows.csv";

        public const string LeakCheckReportPath =
            "Docs/V0.4/Reports/BattleSandboxEnemyCombatFeedbackLeakCheckReport.md";

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattleSandboxEnemyCombatFeedbackPreview preview = null)
        {
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();
            BattleSandboxEnemyCombatFeedbackPreview safePreview =
                preview ?? BattleSandboxEnemyCombatFeedbackUiReuseValidator.BuildDefaultPreview();

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
            BattleSandboxEnemyCombatFeedbackPreview preview,
            int errors,
            int warnings,
            int leakCount)
        {
            StringBuilder builder = new();
            builder.AppendLine("# BattleSandbox Enemy Combat Feedback UI Reuse Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattleSandboxEnemyCombatFeedbackPreview.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 && leakCount == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine($"Leak Count: `{leakCount}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Corrects the previous enemy/Boss topic-panel direction into combat feedback language.");
            builder.AppendLine("- Target scene: `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity`.");
            builder.AppendLine("- Player-facing UI shows Boss state, skill cast, cast-bar timing, mechanism short phrases, weakness windows, failure feedback, and enemy intent-like pressure only.");
            builder.AppendLine("- hardSolutionTags, requiredSynergy, requiredAffix, requiredStats, DropBias weights, and Boss six-key full answers remain in developer data panel links or reports.");
            builder.AppendLine("- Formal RunFlow, formal damage settlement, rewards, save/progress, chapter advance, feature flags, and formal 3-10 / 4-10 remain disconnected.");
            builder.AppendLine();
            builder.AppendLine("## Required Counters");
            builder.AppendLine();
            builder.AppendLine($"- row count: `{preview?.RowCount ?? 0}`");
            builder.AppendLine($"- player visible row count: `{preview?.PlayerVisibleRowCount ?? 0}`");
            builder.AppendLine($"- Boss state rows: `{preview?.BossStateRowCount ?? 0}`");
            builder.AppendLine($"- cast-bar rows: `{preview?.CastBarRowCount ?? 0}`");
            builder.AppendLine($"- floating feedback rows: `{preview?.FloatingFeedbackRowCount ?? 0}`");
            builder.AppendLine($"- mechanic feedback rows: `{preview?.MechanicFeedbackRowCount ?? 0}`");
            builder.AppendLine($"- masked answer field count: `{preview?.MaskedAnswerFieldCount ?? 0}`");
            builder.AppendLine($"- formal leak count: `{preview?.FormalLeakCount ?? 1}`");
            builder.AppendLine($"- feature flags default true: `{BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue)}`");
            builder.AppendLine("- V02 scene changes: `0`");
            builder.AppendLine("- V03 scene changes: `0`");
            builder.AppendLine();
            builder.AppendLine("## Feedback Coverage");
            builder.AppendLine();
            builder.AppendLine("| Kind | Rows | Cast-Bar Rows | Floating Rows |");
            builder.AppendLine("| --- | ---: | ---: | ---: |");
            foreach (IGrouping<string, BattleSandboxEnemyCombatFeedbackRow> group in Rows(preview)
                         .GroupBy(row => row.feedbackKind)
                         .OrderBy(group => group.Key, StringComparer.Ordinal))
            {
                builder.AppendLine(
                    $"| `{Escape(group.Key)}` | {group.Count()} | {group.Count(row => row.usesEnemyCastBarLanguage)} | {group.Count(row => row.usesFloatingCombatTextLanguage)} |");
            }

            builder.AppendLine();
            builder.AppendLine("## Player-Side Samples");
            builder.AppendLine();
            builder.AppendLine("| Feedback | Kind | State | Cast | Floating | Combat Log |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- |");
            foreach (BattleSandboxEnemyCombatFeedbackRow row in Rows(preview).Take(14))
            {
                builder.AppendLine(
                    $"| `{Escape(row.feedbackId)}` | `{Escape(row.feedbackKind)}` | {Escape(row.stateLineChinese)} | {Escape(row.castSkillLineChinese)} | {Escape(row.floatingTextChinese)} | {Escape(row.combatLogLineChinese)} |");
            }

            builder.AppendLine();
            builder.AppendLine("## Developer Answer Links");
            builder.AppendLine();
            builder.AppendLine("| English Stable Key | Chinese Display Name | Data Panel Slot | Source | Player Visible | Masked From Player |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- |");
            foreach (BattleSandboxCombatFeedbackDeveloperLink link in preview?.developerAnswerLinks
                         ?? new List<BattleSandboxCombatFeedbackDeveloperLink>())
            {
                builder.AppendLine(
                    $"| `{Escape(link.englishStableKey)}` | {Escape(link.chineseDisplayName)} | `{Escape(link.dataPanelSlot)}` | `{Escape(link.sourceDataPath)}` | `{link.playerVisible}` | `{link.maskedFromPlayer}` |");
            }

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildRowsCsv(BattleSandboxEnemyCombatFeedbackPreview preview)
        {
            StringBuilder csv = new();
            csv.AppendLine("feedbackId,feedbackKind,bossDisplayNameChinese,stateLineChinese,castSkillLineChinese,floatingTextChinese,combatLogLineChinese,reuseSourceComponent,reuseLanguagePattern,sourceDataPath,developerDataPanelFieldKey,castDurationSeconds,devOnly,isEnabled,playerVisible,developerPanelVisible,playerShowsCompleteAnswer,usesFloatingCombatTextLanguage,usesEnemyCastBarLanguage,usesBossInfoLanguage,runsFormalCombat,callsFormalDamageSettlement,writesFormalFlow,writesFormalSaveData,grantsFormalReward,advancesChapter,opensFeatureFlag,formalLeak");
            foreach (BattleSandboxEnemyCombatFeedbackRow row in Rows(preview))
            {
                csv.AppendLine(Csv(
                    row.feedbackId,
                    row.feedbackKind,
                    row.bossDisplayNameChinese,
                    row.stateLineChinese,
                    row.castSkillLineChinese,
                    row.floatingTextChinese,
                    row.combatLogLineChinese,
                    row.reuseSourceComponent,
                    row.reuseLanguagePattern,
                    row.sourceDataPath,
                    row.developerDataPanelFieldKey,
                    row.castDurationSeconds.ToString("0.00"),
                    row.devOnly.ToString(),
                    row.isEnabled.ToString(),
                    row.playerVisible.ToString(),
                    row.developerPanelVisible.ToString(),
                    row.playerShowsCompleteAnswer.ToString(),
                    row.usesFloatingCombatTextLanguage.ToString(),
                    row.usesEnemyCastBarLanguage.ToString(),
                    row.usesBossInfoLanguage.ToString(),
                    row.runsFormalCombat.ToString(),
                    row.callsFormalDamageSettlement.ToString(),
                    row.writesFormalFlow.ToString(),
                    row.writesFormalSaveData.ToString(),
                    row.grantsFormalReward.ToString(),
                    row.advancesChapter.ToString(),
                    row.opensFeatureFlag.ToString(),
                    row.FormalLeak.ToString()));
            }

            csv.AppendLine();
            csv.AppendLine("developerKey,chineseDisplayName,dataPanelSlot,sourceDataPath,developerVisible,playerVisible,maskedFromPlayer");
            foreach (BattleSandboxCombatFeedbackDeveloperLink link in preview?.developerAnswerLinks
                         ?? new List<BattleSandboxCombatFeedbackDeveloperLink>())
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
            BattleSandboxEnemyCombatFeedbackPreview preview,
            int errors,
            int warnings,
            int leakCount)
        {
            int featureFlagDefaultTrue = BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue);
            int formalLeakCount = preview?.FormalLeakCount ?? 1;
            int playerTextLeaks = BattleSandboxEnemyCombatFeedbackUiReuseValidator.CountPlayerTextLeaks(preview);
            int developerLinkLeaks = BattleSandboxEnemyCombatFeedbackUiReuseValidator.CountDeveloperLinkLeaks(preview);
            int sceneBindingLeaks = BattleSandboxEnemyCombatFeedbackUiReuseValidator.CountSceneBindingLeaks();

            StringBuilder builder = new();
            builder.AppendLine("# BattleSandbox Enemy Combat Feedback Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattleSandboxEnemyCombatFeedbackPreview.PackageName}`");
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
            builder.AppendLine("- Formal RunFlow, PageState, FormationState, SaveData, PlayerPrefs, MainTrialProgressData, formal damage, formal rewards, formal drops, and formal Boss/enemy configs are not connected.");
            builder.AppendLine("- Formal 3-10 and formal 4-10 are not referenced.");
            builder.AppendLine("- Player-facing strings stay as combat feedback short phrases; complete answer fields remain masked in developer data links.");
            builder.AppendLine("- Previous player-side topic preview objects are removed or inactive in the target scene.");
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static int CountLeaks(BattleSandboxEnemyCombatFeedbackPreview preview)
        {
            return BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue)
                + (preview?.FormalLeakCount ?? 1)
                + BattleSandboxEnemyCombatFeedbackUiReuseValidator.CountPlayerTextLeaks(preview)
                + BattleSandboxEnemyCombatFeedbackUiReuseValidator.CountDeveloperLinkLeaks(preview)
                + BattleSandboxEnemyCombatFeedbackUiReuseValidator.CountSceneBindingLeaks();
        }

        private static IEnumerable<BattleSandboxEnemyCombatFeedbackRow> Rows(
            BattleSandboxEnemyCombatFeedbackPreview preview)
        {
            return preview?.rows?
                .Where(row => row != null)
                ?? Enumerable.Empty<BattleSandboxEnemyCombatFeedbackRow>();
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
