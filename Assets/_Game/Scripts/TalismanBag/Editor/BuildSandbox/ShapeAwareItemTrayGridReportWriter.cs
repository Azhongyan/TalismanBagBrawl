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
    public static class ShapeAwareItemTrayGridReportWriter
    {
        public const string MainReportPath =
            "Docs/V0.4/Reports/ShapeAwareItemTrayGridReport.md";

        public const string MapReportPath =
            "Docs/V0.4/Reports/ShapeAwareItemTrayGridMap.csv";

        public const string LeakCheckReportPath =
            "Docs/V0.4/Reports/ShapeAwareItemTrayGridLeakCheckReport.md";

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            ShapeAwareItemTrayGridValidationPlan plan = null)
        {
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();
            ShapeAwareItemTrayGridValidationPlan safePlan =
                plan ?? ShapeAwareItemTrayGridValidator.BuildDefaultPlan();
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string mainPath = Path.Combine(projectRoot, MainReportPath);
            string mapPath = Path.Combine(projectRoot, MapReportPath);
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
                BuildMapReport(safePlan),
                new UTF8Encoding(false));
            File.WriteAllText(
                leakPath,
                BuildLeakCheckReport(safeReports, safePlan, errors, warnings, leakCount),
                new UTF8Encoding(false));

            AssetDatabase.Refresh();
            return new[] { mainPath, mapPath, leakPath };
        }

        private static string BuildMainReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            ShapeAwareItemTrayGridValidationPlan plan,
            int errors,
            int warnings,
            int leakCount)
        {
            string status = errors == 0 && leakCount == 0
                ? "PASS_DEVONLY_TRAY_GRID_READY"
                : "BLOCKED_TRAY_GRID_ERRORS";

            StringBuilder builder = new();
            builder.AppendLine("# Shape-Aware ItemTray Grid Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{ShapeAwareItemTrayGridValidator.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{status}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine($"Leak Count: `{leakCount}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Adds a BuildSandbox/devOnly `ShapeAwareItemTrayGrid` receiver.");
            builder.AppendLine("- Uses `ShapePlacementSession`, `ShapeItemPayload`, and `ShapeGridReceiver` as the placement authority.");
            builder.AppendLine("- Covers 1/2/3/4-cell tray shapes, row-major packing, bounds checks, overlap checks, and stable tray anchors.");
            builder.AppendLine("- Does not redraw the mature BattlePrepare item tray, replace formal UI, touch formal scenes, or continue the Fix03 overlay route.");
            builder.AppendLine();
            builder.AppendLine("## Grid Samples");
            builder.AppendLine();
            builder.AppendLine("| Sample | Action | Item | Shape | Cells | Anchor | Occupied Cells | Occupied Slots | Expected Valid | Actual Valid | Reason | State | Mutated | Stable Anchor | Note |");
            builder.AppendLine("| --- | --- | --- | --- | ---: | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |");
            foreach (ShapeAwareItemTrayGridSampleRow row in plan.Rows ?? new List<ShapeAwareItemTrayGridSampleRow>())
            {
                builder.AppendLine(
                    $"| `{Escape(row.SampleId)}` | `{Escape(row.Action)}` | `{Escape(row.ItemId)}` | `{Escape(row.ShapeId)}` | {row.CellCount} | `{Escape(row.Anchor)}` | `{Escape(row.OccupiedCells)}` | `{Escape(row.OccupiedSlots)}` | `{row.ExpectedValid}` | `{row.ActualValid}` | `{row.InvalidReason}` | `{row.StateAfter}` | `{row.ReceiverMutated}` | `{row.AnchorStable}` | {Escape(row.Note)} |");
            }

            builder.AppendLine();
            builder.AppendLine("## State Authority");
            builder.AppendLine();
            builder.AppendLine("- The grid owns tray occupancy and committed tray anchors.");
            builder.AppendLine("- Preview is side-effect-light; commit is the only path that mutates tray placement.");
            builder.AppendLine("- Illegal drops keep the previous committed anchor, so the item does not snap to an unrelated initial slot.");
            builder.AppendLine("- Rotated offsets are normalized inside the tray receiver to keep the anchor stable.");

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildMapReport(ShapeAwareItemTrayGridValidationPlan plan)
        {
            StringBuilder builder = new();
            builder.AppendLine("sampleId,action,itemId,shapeId,cellCount,anchor,occupiedCells,occupiedSlots,expectedValid,actualValid,invalidReason,stateAfter,receiverMutated,anchorStable,note");
            foreach (ShapeAwareItemTrayGridSampleRow row in plan.Rows ?? new List<ShapeAwareItemTrayGridSampleRow>())
            {
                builder.AppendLine(string.Join(
                    ",",
                    Csv(row.SampleId),
                    Csv(row.Action),
                    Csv(row.ItemId),
                    Csv(row.ShapeId),
                    row.CellCount.ToString(),
                    Csv(row.Anchor),
                    Csv(row.OccupiedCells),
                    Csv(row.OccupiedSlots),
                    row.ExpectedValid.ToString(),
                    row.ActualValid.ToString(),
                    row.InvalidReason.ToString(),
                    row.StateAfter.ToString(),
                    row.ReceiverMutated.ToString(),
                    row.AnchorStable.ToString(),
                    Csv(row.Note)));
            }

            return builder.ToString();
        }

        private static string BuildLeakCheckReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            ShapeAwareItemTrayGridValidationPlan plan,
            int errors,
            int warnings,
            int leakCount)
        {
            string status = errors == 0 && leakCount == 0
                ? "PASS_DEVONLY_ISOLATED"
                : "BLOCKED_LEAK_CHECK";

            StringBuilder builder = new();
            builder.AppendLine("# Shape-Aware ItemTray Grid Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{ShapeAwareItemTrayGridValidator.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{status}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine();
            builder.AppendLine("## Leak Counters");
            builder.AppendLine();
            builder.AppendLine("| Check | Count | Expected |");
            builder.AppendLine("| --- | ---: | ---: |");
            builder.AppendLine($"| `featureFlagDefaultTrue` | {(plan.FeatureFlagDefaultEnabled ? 1 : 0)} | 0 |");
            builder.AppendLine($"| `fix03RouteLeaks` | {CountFix03RouteLeaks(plan)} | 0 |");
            builder.AppendLine($"| `uiRewriteLeaks` | {CountUiRewriteLeaks(plan)} | 0 |");
            builder.AppendLine($"| `formalSystemLeaks` | {CountFormalSystemLeaks(plan)} | 0 |");
            builder.AppendLine($"| `totalLeaks` | {leakCount} | 0 |");
            builder.AppendLine();
            builder.AppendLine("## Scope Confirmation");
            builder.AppendLine();
            builder.AppendLine("- Feature flags remain default false.");
            builder.AppendLine("- The package remains BuildSandbox/devOnly and disabled by default.");
            builder.AppendLine("- No formal scenes, hand-tuned RectTransforms, RunFlow, PageState, FormationState, SaveData, Boss, rewards, drops, or numeric config are touched.");
            builder.AppendLine("- No overlay, `ignoreLayout`, or delayed one-frame drop read is used as placement authority.");

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static int CountLeaks(ShapeAwareItemTrayGridValidationPlan plan)
        {
            if (plan == null)
            {
                return 1;
            }

            int leaks = 0;
            if (!plan.DevOnly) leaks++;
            if (plan.IsEnabled) leaks++;
            if (plan.FeatureFlagDefaultEnabled) leaks++;
            leaks += CountFix03RouteLeaks(plan);
            leaks += CountUiRewriteLeaks(plan);
            leaks += CountFormalSystemLeaks(plan);
            return leaks;
        }

        private static int CountFix03RouteLeaks(ShapeAwareItemTrayGridValidationPlan plan)
        {
            int leaks = 0;
            if (plan.ContinuesFix03OverlayRoute) leaks++;
            if (plan.UsesIgnoreLayoutAuthority) leaks++;
            if (plan.UsesDelayedDropRead) leaks++;
            return leaks;
        }

        private static int CountUiRewriteLeaks(ShapeAwareItemTrayGridValidationPlan plan)
        {
            int leaks = 0;
            if (plan.RedrawsItemTray) leaks++;
            if (plan.ReplacesMatureItemTray) leaks++;
            if (plan.WritesFormalScene) leaks++;
            if (plan.WritesFormalUi) leaks++;
            return leaks;
        }

        private static int CountFormalSystemLeaks(ShapeAwareItemTrayGridValidationPlan plan)
        {
            int leaks = 0;
            if (plan.TouchesRunFlow) leaks++;
            if (plan.TouchesPageState) leaks++;
            if (plan.TouchesFormationState) leaks++;
            if (plan.TouchesSaveData) leaks++;
            if (plan.TouchesBossRewardDropNumeric) leaks++;
            return leaks;
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

        private static string Escape(string value)
        {
            return (value ?? string.Empty)
                .Replace("|", "\\|")
                .Replace("\r", " ")
                .Replace("\n", " ");
        }

        private static string Csv(string value)
        {
            string safeValue = value ?? string.Empty;
            if (safeValue.IndexOfAny(new[] { ',', '"', '\r', '\n' }) < 0)
            {
                return safeValue;
            }

            return $"\"{safeValue.Replace("\"", "\"\"")}\"";
        }
    }
}
#endif
