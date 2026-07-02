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
    public static class BattlePrepareComponentAdapterPlaytestReportWriter
    {
        public const string MainReportPath =
            "Docs/V0.4/Reports/BattlePrepareComponentAdapterPlaytestReport.md";

        public const string MapReportPath =
            "Docs/V0.4/Reports/BattlePrepareComponentAdapterPlaytestMap.csv";

        public const string LeakCheckReportPath =
            "Docs/V0.4/Reports/BattlePrepareComponentAdapterPlaytestLeakCheckReport.md";

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattlePrepareComponentAdapterPlaytestController playtest = null)
        {
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();
            BattlePrepareComponentAdapterPlaytestController safePlaytest =
                playtest ?? BattlePrepareComponentAdapterPlaytestValidator.BuildDefaultPlaytest();
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string mainPath = Path.Combine(projectRoot, MainReportPath);
            string mapPath = Path.Combine(projectRoot, MapReportPath);
            string leakPath = Path.Combine(projectRoot, LeakCheckReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(mainPath) ?? projectRoot);

            int errors = safeReports.Sum(report => report.ErrorCount);
            int warnings = safeReports.Sum(report => report.WarningCount);
            int leakCount = CountLeaks(safePlaytest);

            File.WriteAllText(
                mainPath,
                BuildMainReport(safeReports, safePlaytest, errors, warnings, leakCount),
                new UTF8Encoding(false));
            File.WriteAllText(
                mapPath,
                BuildMapCsv(safePlaytest),
                new UTF8Encoding(false));
            File.WriteAllText(
                leakPath,
                BuildLeakCheckReport(safeReports, safePlaytest, errors, warnings, leakCount),
                new UTF8Encoding(false));

            AssetDatabase.Refresh();
            return new[] { mainPath, mapPath, leakPath };
        }

        private static string BuildMainReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattlePrepareComponentAdapterPlaytestController playtest,
            int errors,
            int warnings,
            int leakCount)
        {
            StringBuilder builder = new();
            builder.AppendLine("# BattlePrepare Component Adapter Playtest Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattlePrepareComponentAdapterPlaytestController.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 && leakCount == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine($"Leak Count: `{leakCount}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- devOnly / sandbox hand-feel probe for mature BattlePrepare component adaptation.");
            builder.AppendLine("- Reuses V0.2 / V0.3 BattlePrepare board, item tray, drag gesture, pull-up motion, tooltip, and feedback language as Base Component targets.");
            builder.AppendLine("- Adds only Extension / Adapter probe rows; no V04 board, item tray, drag system, pull-up animation, or item info popup is redrawn.");
            builder.AppendLine("- Does not write V02 / V03 formal scenes, user hand-tuned RectTransform values, RunFlow, PageState, FormationState, SaveData, Boss, reward, drop, or numeric systems.");
            builder.AppendLine("- This package is static/report-only unless a future Guard-approved devOnly runtime seam is added; it does not execute a live drag session.");
            builder.AppendLine();
            builder.AppendLine("## Playtest Mode");
            builder.AppendLine();
            builder.AppendLine($"- validationMode: `{Escape(playtest.validationMode)}`");
            builder.AppendLine($"- staticPlaytestOnly: `{playtest.staticPlaytestOnly}`");
            builder.AppendLine($"- realRuntimeInteractionExecuted: `{playtest.realRuntimeInteractionExecuted}`");
            builder.AppendLine($"- devOnly: `{playtest.devOnly}`");
            builder.AppendLine($"- isEnabled: `{playtest.isEnabled}`");
            builder.AppendLine($"- sourceAdapterPackageName: `{Escape(playtest.sourceAdapterPackageName)}`");
            builder.AppendLine($"- sourcePreviewBuildId: `{Escape(playtest.sourcePreviewBuildId)}`");
            builder.AppendLine($"- baseComponentName: `{Escape(playtest.baseComponentName)}`");
            builder.AppendLine($"- handFeelConclusion: {Escape(playtest.handFeelConclusion)}");
            builder.AppendLine();
            builder.AppendLine("## Guardrail Answers");
            builder.AppendLine();
            builder.AppendLine("| Question | Answer |");
            builder.AppendLine("| --- | --- |");
            builder.AppendLine($"| Reuses mature BattlePrepare components | `{playtest.probes.Any(row => row.matureComponent.Contains("V02") || row.matureComponent.Contains("V03") || row.matureComponent.Contains("Draggable"))}` |");
            builder.AppendLine($"| Only adds Extension / Adapter rows | `{!playtest.writesFormalUi && !playtest.writesFormalScene}` |");
            builder.AppendLine($"| Redrew UI | `{playtest.redrewV04Board || playtest.redrewV04ItemTray || playtest.promotesTemporaryPreviewUi}` |");
            builder.AppendLine($"| Modified V02 / V03 formal scenes | `{playtest.writesFormalScene}` |");
            builder.AppendLine($"| Overwrote user hand-tuned RectTransform | `{playtest.overwritesHandTunedRectTransform}` |");
            builder.AppendLine($"| Connected formal RunFlow / SaveData / Boss / reward / numeric systems | `{playtest.touchesRunFlow || playtest.touchesSaveData || playtest.touchesBossRewardDropNumeric}` |");
            builder.AppendLine($"| FeatureFlag default enabled | `{playtest.featureFlagDefaultEnabled}` |");
            builder.AppendLine();
            builder.AppendLine("## Probe Summary");
            builder.AppendLine();
            builder.AppendLine($"- Probes: `{playtest.PassedProbeCount}/{playtest.ProbeCount}` passed");
            builder.AppendLine($"- Manual check rows: `{playtest.ManualCheckCount}`");
            builder.AppendLine();
            builder.AppendLine("| Probe | Mature Component | Extension | Adapter Output | Passed | Evidence |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- |");
            foreach (BattlePrepareComponentAdapterPlaytestProbeRow row in playtest.probes ?? new List<BattlePrepareComponentAdapterPlaytestProbeRow>())
            {
                builder.AppendLine(
                    $"| {Escape(row.chineseDisplayName)} | `{Escape(row.matureComponent)}` | `{Escape(row.extensionId)}` | `{Escape(row.adapterOutputKey)}` | `{row.passed}` | {Escape(row.evidence)} |");
            }
            builder.AppendLine();
            builder.AppendLine("## User Handtest");
            builder.AppendLine();
            builder.AppendLine("| Check | Action | Expected |");
            builder.AppendLine("| --- | --- | --- |");
            foreach (BattlePrepareComponentAdapterPlaytestManualCheckRow row in playtest.manualChecks ?? new List<BattlePrepareComponentAdapterPlaytestManualCheckRow>())
            {
                builder.AppendLine(
                    $"| {Escape(row.chineseDisplayName)} | {Escape(row.action)} | {Escape(row.expected)} |");
            }

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildMapCsv(BattlePrepareComponentAdapterPlaytestController playtest)
        {
            StringBuilder csv = new();
            csv.AppendLine("probeId,chineseDisplayName,matureComponent,extensionId,adapterOutputKey,handFeelClaim,passed,evidence,writesFormalUi,modifiesFormalScene,overwritesRectTransform");
            foreach (BattlePrepareComponentAdapterPlaytestProbeRow row in playtest?.probes ?? new List<BattlePrepareComponentAdapterPlaytestProbeRow>())
            {
                csv.AppendLine(Csv(
                    row.probeId,
                    row.chineseDisplayName,
                    row.matureComponent,
                    row.extensionId,
                    row.adapterOutputKey,
                    row.handFeelClaim,
                    row.passed.ToString(),
                    row.evidence,
                    row.writesFormalUi.ToString(),
                    row.modifiesFormalScene.ToString(),
                    row.overwritesRectTransform.ToString()));
            }

            return csv.ToString();
        }

        private static string BuildLeakCheckReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattlePrepareComponentAdapterPlaytestController playtest,
            int errors,
            int warnings,
            int leakCount)
        {
            int featureFlagDefaultTrue = BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue);
            int isolationLeaks = CountIsolationLeaks(playtest);
            int uiRewriteLeaks = CountUiRewriteLeaks(playtest);
            int formalSystemLeaks = CountFormalSystemLeaks(playtest);
            int probeLeaks = CountProbeLeaks(playtest);

            StringBuilder builder = new();
            builder.AppendLine("# BattlePrepare Component Adapter Playtest Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattlePrepareComponentAdapterPlaytestController.PackageName}`");
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
            builder.AppendLine($"| `playtestIsolationLeaks` | {isolationLeaks} | 0 |");
            builder.AppendLine($"| `uiRewriteLeaks` | {uiRewriteLeaks} | 0 |");
            builder.AppendLine($"| `formalSystemLeaks` | {formalSystemLeaks} | 0 |");
            builder.AppendLine($"| `probeFormalWriteLeaks` | {probeLeaks} | 0 |");
            builder.AppendLine($"| `totalLeaks` | {leakCount} | 0 |");
            builder.AppendLine();
            builder.AppendLine("## Formal Scope Confirmation");
            builder.AppendLine();
            builder.AppendLine("- Formal BattlePrepare scripts: not modified by this playtest path.");
            builder.AppendLine("- V02 / V03 formal scenes and hand-tuned RectTransform values: not written.");
            builder.AppendLine("- V04 temporary preview UI: not promoted as formal UI truth.");
            builder.AppendLine("- RunFlow / PageState / FormationState / SaveData / Boss / reward / drop / numeric systems: not touched.");
            builder.AppendLine("- FeatureFlags remain default false; playtest remains devOnly and disabled by default.");
            builder.AppendLine("- Static-only scope is disclosed; this report does not claim a live runtime drag session was executed.");

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static int CountLeaks(BattlePrepareComponentAdapterPlaytestController playtest)
        {
            return BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue)
                + CountIsolationLeaks(playtest)
                + CountUiRewriteLeaks(playtest)
                + CountFormalSystemLeaks(playtest)
                + CountProbeLeaks(playtest);
        }

        private static int CountIsolationLeaks(BattlePrepareComponentAdapterPlaytestController playtest)
        {
            if (playtest == null)
            {
                return 1;
            }

            int leaks = 0;
            if (!playtest.devOnly) leaks++;
            if (playtest.isEnabled) leaks++;
            if (playtest.writesFormalScene) leaks++;
            if (playtest.writesFormalUi) leaks++;
            if (playtest.touchesFormalPrepareController) leaks++;
            if (playtest.overwritesHandTunedRectTransform) leaks++;
            if (playtest.featureFlagDefaultEnabled) leaks++;
            return leaks;
        }

        private static int CountUiRewriteLeaks(BattlePrepareComponentAdapterPlaytestController playtest)
        {
            if (playtest == null)
            {
                return 1;
            }

            int leaks = 0;
            if (playtest.redrewV04Board) leaks++;
            if (playtest.redrewV04ItemTray) leaks++;
            if (playtest.rewroteDragFeel) leaks++;
            if (playtest.rewrotePullUpAnimation) leaks++;
            if (playtest.promotesTemporaryPreviewUi) leaks++;
            return leaks;
        }

        private static int CountFormalSystemLeaks(BattlePrepareComponentAdapterPlaytestController playtest)
        {
            if (playtest == null)
            {
                return 1;
            }

            int leaks = 0;
            if (playtest.touchesRunFlow) leaks++;
            if (playtest.touchesPageState) leaks++;
            if (playtest.touchesFormationState) leaks++;
            if (playtest.touchesSaveData) leaks++;
            if (playtest.touchesBossRewardDropNumeric) leaks++;
            return leaks;
        }

        private static int CountProbeLeaks(BattlePrepareComponentAdapterPlaytestController playtest)
        {
            return (playtest?.probes ?? new List<BattlePrepareComponentAdapterPlaytestProbeRow>())
                .Count(row => row == null || row.writesFormalUi || row.modifiesFormalScene || row.overwritesRectTransform);
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
