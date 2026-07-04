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
    public static class BattleSandboxManaLoopRuntimeReportWriter
    {
        public const string MainReportPath =
            "Docs/V0.4/Reports/BattleSandboxManaLoopRuntimeReport.md";

        public const string RowReportPath =
            "Docs/V0.4/Reports/BattleSandboxManaLoopRuntimeRows.csv";

        public const string LeakCheckReportPath =
            "Docs/V0.4/Reports/BattleSandboxManaLoopRuntimeLeakCheckReport.md";

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattleSandboxManaLoopPreview preview = null)
        {
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();
            BattleSandboxManaLoopPreview safePreview =
                preview ?? BattleSandboxManaLoopRuntimeValidator.BuildDefaultPreview();

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
            BattleSandboxManaLoopPreview preview,
            int errors,
            int warnings,
            int leakCount)
        {
            StringBuilder builder = new();
            builder.AppendLine("# BattleSandbox Mana Loop Runtime Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattleSandboxManaLoopRuntimeValidator.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 && leakCount == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine($"Leak Count: `{leakCount}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- User-specified devOnly direct package for Scene_TalismanBag_V04_BattleSandboxPreview.");
            builder.AppendLine("- Runtime reads V0.4 BuildSandboxItemStat mana fields from placed item snapshots.");
            builder.AppendLine("- It updates only existing mana text/fill and runtime floating text under existing anchors.");
            builder.AppendLine("- It does not connect formal V0.2/V0.3 combat, saves, rewards, Boss configs, chapters, or formal progression.");
            builder.AppendLine("- It does not author RectTransform or sibling-order changes to the hand-tuned V04 UI.");
            builder.AppendLine();
            builder.AppendLine("## Counters");
            builder.AppendLine();
            builder.AppendLine($"- max mana: `{preview?.maxMana ?? 0}`");
            builder.AppendLine($"- initial mana: `{preview?.initialMana ?? 0}`");
            builder.AppendLine($"- final mana: `{preview?.finalMana ?? 0}`");
            builder.AppendLine($"- generated mana total: `{preview?.generatedManaTotal ?? 0}`");
            builder.AppendLine($"- spent mana total: `{preview?.spentManaTotal ?? 0}`");
            builder.AppendLine($"- source ItemStat profile count: `{preview?.sourceItemStatProfileCount ?? 0}`");
            builder.AppendLine($"- mana gain row count: `{preview?.ManaGainRowCount ?? 0}`");
            builder.AppendLine($"- mana spend row count: `{preview?.ManaSpendRowCount ?? 0}`");
            builder.AppendLine($"- player-side answer leak count: `{preview?.PlayerSideAnswerLeakCount ?? 1}`");
            builder.AppendLine($"- formal leak count: `{preview?.FormalLeakCount ?? 1}`");
            builder.AppendLine($"- feature flag default true count: `{preview?.FeatureFlagDefaultTrueCount ?? 1}`");
            builder.AppendLine($"- UI layout write count: `{preview?.UiLayoutWriteCount ?? 1}`");
            builder.AppendLine();
            AppendRows(builder, preview);
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildRowsCsv(BattleSandboxManaLoopPreview preview)
        {
            StringBuilder csv = new();
            csv.AppendLine("rowId,rowKind,itemId,statProfileId,manaDelta,currentManaAfter,maxMana,playerHudTextChinese,floatingTextChinese,sourceDataPath,developerDataPanelFieldKey,readsBuildSandboxItemStat,playerSideAnswerLeak,formalFlowLeak,uiLayoutWrite");
            foreach (BattleSandboxManaLoopPreviewRow row in BattleSandboxManaLoopRuntimeValidator.Rows(preview))
            {
                csv.AppendLine(Csv(
                    row.rowId,
                    row.rowKind,
                    row.itemId,
                    row.statProfileId,
                    row.manaDelta.ToString(),
                    row.currentManaAfter.ToString(),
                    row.maxMana.ToString(),
                    row.playerHudTextChinese,
                    row.floatingTextChinese,
                    row.sourceDataPath,
                    row.developerDataPanelFieldKey,
                    row.readsBuildSandboxItemStat.ToString(),
                    row.playerSideAnswerLeak.ToString(),
                    row.formalFlowLeak.ToString(),
                    row.uiLayoutWrite.ToString()));
            }

            return csv.ToString();
        }

        private static string BuildLeakCheckReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattleSandboxManaLoopPreview preview,
            int errors,
            int warnings,
            int leakCount)
        {
            StringBuilder builder = new();
            builder.AppendLine("# BattleSandbox Mana Loop Runtime Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattleSandboxManaLoopRuntimeValidator.PackageName}`");
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
            builder.AppendLine($"| `formalV02V03CombatReads` | {(preview?.readsFormalV02V03Combat == true ? 1 : 0)} | 0 |");
            builder.AppendLine($"| `saveWrites` | {(preview?.writesFormalSaveData == true ? 1 : 0)} | 0 |");
            builder.AppendLine($"| `rewardWrites` | {(preview?.writesFormalReward == true ? 1 : 0)} | 0 |");
            builder.AppendLine($"| `bossWrites` | {(preview?.writesFormalBossProgress == true ? 1 : 0)} | 0 |");
            builder.AppendLine($"| `chapterAdvances` | {(preview?.writesFormalChapterProgress == true ? 1 : 0)} | 0 |");
            builder.AppendLine($"| `playerSideAnswerLeaks` | {preview?.PlayerSideAnswerLeakCount ?? 1} | 0 |");
            builder.AppendLine($"| `uiLayoutWrites` | {preview?.UiLayoutWriteCount ?? 1} | 0 |");
            builder.AppendLine($"| `totalLeaks` | {leakCount} | 0 |");
            builder.AppendLine();
            builder.AppendLine("## Scope Confirmation");
            builder.AppendLine();
            builder.AppendLine("- Mana source and mana cost are read from BuildSandbox ItemStat only.");
            builder.AppendLine("- Runtime does not call formal damage settlement, RunFlow, SaveData, rewards, Boss, or chapter progression APIs.");
            builder.AppendLine("- Runtime does not persist UI layout edits; floating text objects are runtime-only.");
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static int CountLeaks(BattleSandboxManaLoopPreview preview)
        {
            return (preview?.FeatureFlagDefaultTrueCount ?? 1)
                + (preview?.FormalLeakCount ?? 1)
                + (preview?.PlayerSideAnswerLeakCount ?? 1)
                + (preview?.UiLayoutWriteCount ?? 1);
        }

        private static void AppendRows(
            StringBuilder builder,
            BattleSandboxManaLoopPreview preview)
        {
            builder.AppendLine("## Mana Loop Rows");
            builder.AppendLine();
            builder.AppendLine("| Row | Kind | Item | Stat Profile | Delta | Mana | Floating | Source |");
            builder.AppendLine("| --- | --- | --- | --- | ---: | ---: | --- | --- |");
            foreach (BattleSandboxManaLoopPreviewRow row in BattleSandboxManaLoopRuntimeValidator.Rows(preview))
            {
                builder.AppendLine(
                    $"| `{Escape(row.rowId)}` | `{Escape(row.rowKind)}` | `{Escape(row.itemId)}` | `{Escape(row.statProfileId)}` | {row.manaDelta} | {row.currentManaAfter}/{row.maxMana} | {Escape(row.floatingTextChinese)} | `{Escape(row.sourceDataPath)}` |");
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
