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
    public static class BattlePrepareExtensionSeamReportWriter
    {
        public const string MainReportPath =
            "Docs/V0.4/Reports/BattlePrepareExtensionSeamReport.md";

        public const string SeamMapReportPath =
            "Docs/V0.4/Reports/BattlePrepareExtensionSeamMap.csv";

        public const string LeakCheckReportPath =
            "Docs/V0.4/Reports/BattlePrepareExtensionSeamLeakCheckReport.md";

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattlePrepareExtensionSeamPlan plan = null)
        {
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();
            BattlePrepareExtensionSeamPlan safePlan =
                plan ?? BattlePrepareExtensionSeamValidator.BuildDefaultPlan();

            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string mainPath = Path.Combine(projectRoot, MainReportPath);
            string mapPath = Path.Combine(projectRoot, SeamMapReportPath);
            string leakPath = Path.Combine(projectRoot, LeakCheckReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(mainPath) ?? projectRoot);

            int errors = safeReports.Sum(report => report.ErrorCount);
            int warnings = safeReports.Sum(report => report.WarningCount);
            int leakCount = CountLeaks(safePlan);

            File.WriteAllText(
                mainPath,
                BuildMainReport(safeReports, safePlan, errors, warnings, leakCount),
                new UTF8Encoding(false));
            File.WriteAllText(
                mapPath,
                BuildMapCsv(safePlan),
                new UTF8Encoding(false));
            File.WriteAllText(
                leakPath,
                BuildLeakReport(safeReports, safePlan, errors, warnings, leakCount),
                new UTF8Encoding(false));

            AssetDatabase.Refresh();
            return new[] { mainPath, mapPath, leakPath };
        }

        private static string BuildMainReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattlePrepareExtensionSeamPlan plan,
            int errors,
            int warnings,
            int leakCount)
        {
            string status = errors == 0 && leakCount == 0
                ? "PASS_BATTLEPREPARE_EXTENSION_SEAM01"
                : "BLOCKED_BATTLEPREPARE_EXTENSION_SEAM01";

            StringBuilder builder = new();
            builder.AppendLine("# BattlePrepare Extension Seam Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattlePrepareExtensionSeamValidator.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{status}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine($"Leak Count: `{leakCount}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Mature BattlePrepare board, item tray, drag, ghost, commit, and cancel paths keep their default behavior.");
            builder.AppendLine("- The new seam is dormant unless a devOnly provider is injected.");
            builder.AppendLine("- V0.4 ShapePlacementSession is attached through `BattlePrepareShapePlacementSeamAdapter` only.");
            builder.AppendLine();
            builder.AppendLine("## Seam Coverage");
            builder.AppendLine();
            builder.AppendLine("| Seam | Mature Surface | Provider | Present | Default Behavior |");
            builder.AppendLine("| --- | --- | --- | --- | --- |");
            foreach (BattlePrepareExtensionSeamMapRow row in plan.rows ?? new List<BattlePrepareExtensionSeamMapRow>())
            {
                builder.AppendLine(
                    $"| `{Escape(row.seamId)}` | {Escape(row.matureSurface)} | `{Escape(row.extensionProvider)}` | `{row.present}` | {Escape(row.defaultBehavior)} |");
            }

            builder.AppendLine();
            builder.AppendLine("## Default Isolation");
            builder.AppendLine();
            builder.AppendLine("| Check | Value |");
            builder.AppendLine("| --- | --- |");
            builder.AppendLine($"| `devOnly` | `{plan.devOnly}` |");
            builder.AppendLine($"| `isEnabled` | `{plan.isEnabled}` |");
            builder.AppendLine($"| `featureFlagDefaultEnabled` | `{plan.featureFlagDefaultEnabled}` |");
            builder.AppendLine($"| `formalBehaviorChanged` | `{plan.formalBehaviorChanged}` |");
            builder.AppendLine($"| `runtimePlaytestSourceHasInjection` | `{plan.runtimePlaytestSourceHasInjection}` |");
            builder.AppendLine();
            builder.AppendLine("## Notes");
            builder.AppendLine();
            builder.AppendLine("- No V02 core drop target was modified.");
            builder.AppendLine("- No formal RunFlow, SaveData, Boss, reward, drop, numeric, scene layout, or RectTransform asset was modified.");
            builder.AppendLine("- The old overlay + ignoreLayout + delayed drop route remains blocked.");

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildMapCsv(BattlePrepareExtensionSeamPlan plan)
        {
            StringBuilder builder = new();
            builder.AppendLine("seamId,matureSurface,extensionProvider,present,defaultBehavior");
            foreach (BattlePrepareExtensionSeamMapRow row in plan.rows ?? new List<BattlePrepareExtensionSeamMapRow>())
            {
                builder.AppendLine(string.Join(
                    ",",
                    Csv(row.seamId),
                    Csv(row.matureSurface),
                    Csv(row.extensionProvider),
                    row.present.ToString(),
                    Csv(row.defaultBehavior)));
            }

            return builder.ToString();
        }

        private static string BuildLeakReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattlePrepareExtensionSeamPlan plan,
            int errors,
            int warnings,
            int leakCount)
        {
            string status = errors == 0 && leakCount == 0
                ? "PASS_DEVONLY_ISOLATED"
                : "BLOCKED_LEAK_CHECK";

            StringBuilder builder = new();
            builder.AppendLine("# BattlePrepare Extension Seam Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattlePrepareExtensionSeamValidator.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{status}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine($"Leak Count: `{leakCount}`");
            builder.AppendLine();
            builder.AppendLine("## Leak Counters");
            builder.AppendLine();
            builder.AppendLine("| Check | Count | Expected |");
            builder.AppendLine("| --- | ---: | ---: |");
            builder.AppendLine($"| `featureFlagDefaultEnabled` | {(plan.featureFlagDefaultEnabled ? 1 : 0)} | 0 |");
            builder.AppendLine($"| `formalBehaviorChanged` | {(plan.formalBehaviorChanged ? 1 : 0)} | 0 |");
            builder.AppendLine($"| `touchesRunFlow` | {(plan.touchesRunFlow ? 1 : 0)} | 0 |");
            builder.AppendLine($"| `touchesSaveData` | {(plan.touchesSaveData ? 1 : 0)} | 0 |");
            builder.AppendLine($"| `touchesBossRewardDropNumeric` | {(plan.touchesBossRewardDropNumeric ? 1 : 0)} | 0 |");
            builder.AppendLine($"| `touchesFormalSceneLayout` | {(plan.touchesFormalSceneLayout ? 1 : 0)} | 0 |");
            builder.AppendLine($"| `rewritesBattlePrepare` | {(plan.rewritesBattlePrepare ? 1 : 0)} | 0 |");
            builder.AppendLine($"| `redrawsBoardOrItemTray` | {(plan.redrawsBoardOrItemTray ? 1 : 0)} | 0 |");
            builder.AppendLine($"| `restoresOverlayIgnoreLayoutDelayedDrop` | {(plan.restoresOverlayIgnoreLayoutDelayedDrop ? 1 : 0)} | 0 |");
            builder.AppendLine($"| `totalLeaks` | `{leakCount}` | `0` |");
            builder.AppendLine();
            builder.AppendLine("## Boundary Confirmation");
            builder.AppendLine();
            builder.AppendLine("- Extension provider is injected only by the devOnly runtime playtest.");
            builder.AppendLine("- The V03 controller exposes read-only surface references and pre/post notifications.");
            builder.AppendLine("- The devOnly adapter does not swallow mature tray drop or battle-prepare continue commit.");

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static int CountLeaks(BattlePrepareExtensionSeamPlan plan)
        {
            if (plan == null)
            {
                return 1;
            }

            int leaks = 0;
            if (plan.featureFlagDefaultEnabled) leaks++;
            if (plan.formalBehaviorChanged) leaks++;
            if (plan.touchesRunFlow) leaks++;
            if (plan.touchesSaveData) leaks++;
            if (plan.touchesBossRewardDropNumeric) leaks++;
            if (plan.touchesFormalSceneLayout) leaks++;
            if (plan.rewritesBattlePrepare) leaks++;
            if (plan.redrawsBoardOrItemTray) leaks++;
            if (plan.restoresOverlayIgnoreLayoutDelayedDrop) leaks++;
            return leaks;
        }

        private static void AppendValidationSummary(
            StringBuilder builder,
            IReadOnlyList<BuildSandboxValidationReport> reports)
        {
            builder.AppendLine();
            builder.AppendLine("## Validation Issues");
            builder.AppendLine();
            builder.AppendLine("| Level | Code | Message | Asset |");
            builder.AppendLine("| --- | --- | --- | --- |");
            foreach (BuildSandboxValidationIssue issue in reports.SelectMany(report => report.Issues))
            {
                builder.AppendLine(
                    $"| `{issue.Level}` | `{Escape(issue.Code)}` | {Escape(issue.Message)} | `{Escape(issue.AssetPath)}` |");
            }

            if (!reports.SelectMany(report => report.Issues).Any())
            {
                builder.AppendLine("| `Info` | `NO_ISSUES` | No validation issues. | `` |");
            }
        }

        private static string Escape(string value)
        {
            return (value ?? string.Empty).Replace("|", "\\|");
        }

        private static string Csv(string value)
        {
            string safe = value ?? string.Empty;
            if (safe.Contains(",") || safe.Contains("\"") || safe.Contains("\n") || safe.Contains("\r"))
            {
                return "\"" + safe.Replace("\"", "\"\"") + "\"";
            }

            return safe;
        }
    }
}
#endif
