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
    public static class BattleSandboxItemEffectRuntimePreviewReportWriter
    {
        private static readonly Encoding Utf8WithBom = new UTF8Encoding(true);

        public const string MainReportPath =
            "Docs/V0.4/Reports/BattleSandboxItemEffectRuntimePreviewReport.md";

        public const string RowReportPath =
            "Docs/V0.4/Reports/BattleSandboxItemEffectRuntimePreviewRows.csv";

        public const string LeakCheckReportPath =
            "Docs/V0.4/Reports/BattleSandboxItemEffectRuntimePreviewLeakCheckReport.md";

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattleSandboxItemEffectRuntimePreviewSnapshot snapshot = null)
        {
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();
            BattleSandboxItemEffectRuntimePreviewSnapshot safeSnapshot =
                snapshot ?? BattleSandboxItemEffectRuntimePreviewValidator.BuildSnapshot(safeReports);

            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string mainPath = Path.Combine(projectRoot, MainReportPath);
            string rowPath = Path.Combine(projectRoot, RowReportPath);
            string leakPath = Path.Combine(projectRoot, LeakCheckReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(mainPath) ?? projectRoot);

            int errors = safeReports.Sum(report => report.ErrorCount);
            int warnings = safeReports.Sum(report => report.WarningCount);
            int leakCount = CountLeaks(safeSnapshot);

            WriteUtf8BomText(mainPath, BuildMainReport(safeReports, safeSnapshot, errors, warnings, leakCount));
            WriteUtf8BomText(rowPath, BuildRowsCsv(safeSnapshot));
            WriteUtf8BomText(leakPath, BuildLeakCheckReport(safeReports, safeSnapshot, errors, warnings, leakCount));

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
            BattleSandboxItemEffectRuntimePreviewSnapshot snapshot,
            int errors,
            int warnings,
            int leakCount)
        {
            StringBuilder builder = new();
            builder.AppendLine("# BattleSandbox Item Effect Runtime Preview Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattleSandboxItemEffectRuntimePreviewValidator.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 && leakCount == 0 && snapshot?.Passed == true ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine($"Leak Count: `{leakCount}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Runs only inside V0.4 BuildSandbox / BattleSandbox runtime preview surfaces.");
            builder.AppendLine("- Reuses existing runtime loop HUD text, Boss state line, cast feedback, combat log, and floating text.");
            builder.AppendLine("- Adds no dedicated UI frame and does not author or reorder V04 UI layout.");
            builder.AppendLine("- Does not call or modify V0.2/V0.3 formal RunFlow, DamageText, SaveData, Reward, Boss table, Chapter, or formal numeric data.");
            builder.AppendLine("- Player-facing runtime rows use Chinese effect text; internal effect keys stay in reports only.");
            builder.AppendLine();
            builder.AppendLine("## Counters");
            builder.AppendLine();
            builder.AppendLine($"- roster items: `{snapshot?.rosterItemCount ?? 0}`");
            builder.AppendLine($"- effect profiles: `{snapshot?.profileCount ?? 0}`");
            builder.AppendLine($"- mapped roster items: `{snapshot?.mappedRosterItemCount ?? 0}`");
            builder.AppendLine($"- distinct effect families: `{snapshot?.distinctEffectFamilyCount ?? 0}`");
            builder.AppendLine($"- attack profiles: `{snapshot?.attackProfileCount ?? 0}`");
            builder.AppendLine($"- support profiles: `{snapshot?.supportProfileCount ?? 0}`");
            builder.AppendLine($"- energy profiles: `{snapshot?.energyProfileCount ?? 0}`");
            builder.AppendLine($"- guard profiles: `{snapshot?.guardProfileCount ?? 0}`");
            builder.AppendLine($"- cleanse profiles: `{snapshot?.cleanseProfileCount ?? 0}`");
            builder.AppendLine($"- control profiles: `{snapshot?.controlProfileCount ?? 0}`");
            builder.AppendLine($"- peach profiles: `{snapshot?.peachProfileCount ?? 0}`");
            builder.AppendLine($"- runtime sample rows: `{snapshot?.runtimeRowCount ?? 0}`");
            builder.AppendLine($"- item effect runtime coverage rows: `{snapshot?.itemEffectRuntimeRowCount ?? 0}`");
            builder.AppendLine($"- support damage leak count: `{snapshot?.supportDamageLeakCount ?? 1}`");
            builder.AppendLine($"- player-side answer leak count: `{snapshot?.playerSideAnswerLeakCount ?? 1}`");
            builder.AppendLine($"- formal leak count: `{snapshot?.formalLeakCount ?? 1}`");
            builder.AppendLine($"- UI layout write count: `{snapshot?.uiLayoutWriteCount ?? 1}`");
            builder.AppendLine();
            AppendProfileRows(builder, snapshot);
            AppendSampleRows(builder, snapshot);
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildRowsCsv(BattleSandboxItemEffectRuntimePreviewSnapshot snapshot)
        {
            StringBuilder csv = new();
            csv.AppendLine("rowType,itemId,displayNameChinese,itemEffectKey,effectFamilyKey,effectFamilyChinese,effectRoleChinese,statProfileId,statTag,dealsEnemyDamage,supportOnly,restoresMana,grantsShield,cleanses,controlsBoss,breaksShield,runtimeRows,effectRows,enemyHpDamageTotal,expectedAttackDamage,expectedSupportNoDamage,playerSideAnswerLeakCount,sampleFloatingChinese,sampleLogChinese,sourceDataPath,passed");
            foreach (BattleSandboxItemEffectRuntimeProfileRow row in snapshot?.profileRows ?? new List<BattleSandboxItemEffectRuntimeProfileRow>())
            {
                BattleSandboxItemEffectRuntimeSampleRow sample =
                    snapshot?.sampleRows?.FirstOrDefault(item => string.Equals(item.itemId, row.itemId, StringComparison.Ordinal));
                csv.AppendLine(Csv(
                    "profile",
                    row.itemId,
                    row.displayNameChinese,
                    row.itemEffectKey,
                    row.effectFamilyKey,
                    row.effectFamilyChinese,
                    row.effectRoleChinese,
                    row.statProfileId,
                    row.statTag,
                    row.dealsEnemyDamage.ToString(),
                    row.supportOnly.ToString(),
                    row.restoresMana.ToString(),
                    row.grantsShield.ToString(),
                    row.cleanses.ToString(),
                    row.controlsBoss.ToString(),
                    row.breaksShield.ToString(),
                    (sample?.runtimeRows ?? 0).ToString(),
                    (sample?.effectRows ?? 0).ToString(),
                    (sample?.enemyHpDamageTotal ?? 0).ToString(),
                    (sample?.expectedAttackDamage ?? false).ToString(),
                    (sample?.expectedSupportNoDamage ?? false).ToString(),
                    (sample?.playerSideAnswerLeakCount ?? 0).ToString(),
                    sample?.sampleFloatingChinese ?? string.Empty,
                    sample?.sampleLogChinese ?? string.Empty,
                    row.sourceDataPath,
                    (sample?.passed ?? false).ToString()));
            }

            return csv.ToString();
        }

        private static string BuildLeakCheckReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattleSandboxItemEffectRuntimePreviewSnapshot snapshot,
            int errors,
            int warnings,
            int leakCount)
        {
            StringBuilder builder = new();
            builder.AppendLine("# BattleSandbox Item Effect Runtime Preview Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattleSandboxItemEffectRuntimePreviewValidator.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 && leakCount == 0 && snapshot?.Passed == true ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine();
            builder.AppendLine("## Leak Counters");
            builder.AppendLine();
            builder.AppendLine("| Check | Count | Expected |");
            builder.AppendLine("| --- | ---: | ---: |");
            builder.AppendLine($"| `supportDamageLeaks` | {snapshot?.supportDamageLeakCount ?? 1} | 0 |");
            builder.AppendLine($"| `playerSideAnswerLeaks` | {snapshot?.playerSideAnswerLeakCount ?? 1} | 0 |");
            builder.AppendLine($"| `formalLeaks` | {snapshot?.formalLeakCount ?? 1} | 0 |");
            builder.AppendLine($"| `uiLayoutWrites` | {snapshot?.uiLayoutWriteCount ?? 1} | 0 |");
            builder.AppendLine($"| `createsNewUiFrame` | {(snapshot?.createsNewUiFrame == true ? 1 : 0)} | 0 |");
            builder.AppendLine($"| `writesFormalFlow` | {(snapshot?.writesFormalFlow == true ? 1 : 0)} | 0 |");
            builder.AppendLine($"| `saveWrites` | {(snapshot?.writesFormalSaveData == true ? 1 : 0)} | 0 |");
            builder.AppendLine($"| `rewardGrants` | {(snapshot?.grantsFormalReward == true ? 1 : 0)} | 0 |");
            builder.AppendLine($"| `chapterAdvances` | {(snapshot?.advancesChapter == true ? 1 : 0)} | 0 |");
            builder.AppendLine($"| `totalLeaks` | {leakCount} | 0 |");
            builder.AppendLine();
            builder.AppendLine("## Scope Confirmation");
            builder.AppendLine();
            builder.AppendLine("- Support/no-damage roster items are sampled through the V0.4 RuntimeLoop and must not reduce enemy HP.");
            builder.AppendLine("- Runtime item effect rows reuse existing feedback surfaces and keep UI layout writes at zero.");
            builder.AppendLine("- Player-facing sample text is checked against forbidden answer/progression tokens.");
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static void AppendProfileRows(
            StringBuilder builder,
            BattleSandboxItemEffectRuntimePreviewSnapshot snapshot)
        {
            builder.AppendLine("## Effect Profiles");
            builder.AppendLine();
            builder.AppendLine("| Item | Display | Effect | Role | Damage | Support | Source |");
            builder.AppendLine("| --- | --- | --- | --- | ---: | ---: | --- |");
            foreach (BattleSandboxItemEffectRuntimeProfileRow row in snapshot?.profileRows ?? new List<BattleSandboxItemEffectRuntimeProfileRow>())
            {
                builder.AppendLine(
                    $"| `{Escape(row.itemId)}` | {Escape(row.displayNameChinese)} | {Escape(row.effectFamilyChinese)} | {Escape(row.effectRoleChinese)} | `{row.dealsEnemyDamage}` | `{row.supportOnly}` | `{Escape(row.sourceDataPath)}` |");
            }

            builder.AppendLine();
        }

        private static void AppendSampleRows(
            StringBuilder builder,
            BattleSandboxItemEffectRuntimePreviewSnapshot snapshot)
        {
            builder.AppendLine("## Runtime Samples");
            builder.AppendLine();
            builder.AppendLine("| Item | Effect | Rows | Damage | Floating | Log | Result |");
            builder.AppendLine("| --- | --- | ---: | ---: | --- | --- | --- |");
            foreach (BattleSandboxItemEffectRuntimeSampleRow row in snapshot?.sampleRows ?? new List<BattleSandboxItemEffectRuntimeSampleRow>())
            {
                builder.AppendLine(
                    $"| `{Escape(row.itemId)}` | {Escape(row.effectFamilyChinese)} / {Escape(row.effectRoleChinese)} | {row.effectRows}/{row.runtimeRows} | {row.enemyHpDamageTotal} | {Escape(row.sampleFloatingChinese)} | {Escape(row.sampleLogChinese)} | `{(row.passed ? "PASS" : "FAIL")}` |");
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
            foreach (BuildSandboxValidationReport report in reports ?? Array.Empty<BuildSandboxValidationReport>())
            {
                builder.AppendLine(
                    $"| {Escape(report.Name)} | `{(report.Passed ? "PASS" : "FAIL")}` | {report.ErrorCount} | {report.WarningCount} | {report.InfoCount} |");
            }

            builder.AppendLine();
            builder.AppendLine("## Issues");
            builder.AppendLine();
            builder.AppendLine("| Level | Code | Message | Path |");
            builder.AppendLine("| --- | --- | --- | --- |");
            foreach (BuildSandboxValidationIssue issue in (reports ?? Array.Empty<BuildSandboxValidationReport>()).SelectMany(report => report.Issues))
            {
                builder.AppendLine(
                    $"| `{issue.Level}` | `{Escape(issue.Code)}` | {Escape(issue.Message)} | `{Escape(issue.AssetPath)}` |");
            }
        }

        private static int CountLeaks(BattleSandboxItemEffectRuntimePreviewSnapshot snapshot)
        {
            return (snapshot?.supportDamageLeakCount ?? 1)
                + (snapshot?.playerSideAnswerLeakCount ?? 1)
                + (snapshot?.formalLeakCount ?? 1)
                + (snapshot?.uiLayoutWriteCount ?? 1)
                + (snapshot?.createsNewUiFrame == true ? 1 : 0)
                + (snapshot?.writesFormalFlow == true ? 1 : 0)
                + (snapshot?.writesFormalSaveData == true ? 1 : 0)
                + (snapshot?.grantsFormalReward == true ? 1 : 0)
                + (snapshot?.advancesChapter == true ? 1 : 0);
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
