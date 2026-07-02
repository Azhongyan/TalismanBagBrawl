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
    public static class ShapePlacementSessionReportWriter
    {
        public const string MainReportPath =
            "Docs/V0.4/Reports/ShapePlacementSessionProtocolReport.md";

        public const string ReceiverContractReportPath =
            "Docs/V0.4/Reports/ShapePlacementSessionReceiverContractReport.csv";

        public const string LeakCheckReportPath =
            "Docs/V0.4/Reports/ShapePlacementSessionLeakCheckReport.md";

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            ShapePlacementSessionValidationPlan plan = null)
        {
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();
            ShapePlacementSessionValidationPlan safePlan =
                plan ?? ShapePlacementSessionValidator.BuildDefaultPlan();

            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string mainPath = Path.Combine(projectRoot, MainReportPath);
            string receiverPath = Path.Combine(projectRoot, ReceiverContractReportPath);
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
                receiverPath,
                BuildReceiverContractCsv(safePlan),
                new UTF8Encoding(false));
            File.WriteAllText(
                leakPath,
                BuildLeakCheckReport(safeReports, safePlan, errors, warnings, leakCount),
                new UTF8Encoding(false));

            AssetDatabase.Refresh();
            return new[] { mainPath, receiverPath, leakPath };
        }

        private static string BuildMainReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            ShapePlacementSessionValidationPlan plan,
            int errors,
            int warnings,
            int leakCount)
        {
            string status = errors == 0 && leakCount == 0
                ? "PASS_DEVONLY_PROTOCOL_READY"
                : "BLOCKED_PROTOCOL_ERRORS";

            StringBuilder builder = new();
            builder.AppendLine("# ShapePlacementSession Protocol Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{ShapePlacementSessionValidator.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{status}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine($"Leak Count: `{leakCount}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Adds a BuildSandbox/devOnly placement protocol only.");
            builder.AppendLine("- Adds `ShapePlacementSession`, `ShapeItemPayload`, `ShapeGridReceiver`, and `ShapePlacementState`.");
            builder.AppendLine("- Covers `CanPlace`, `Preview`, `Commit`, and `Cancel` in a pure in-memory receiver.");
            builder.AppendLine("- Does not build UI, redraw item tray, redraw board, or touch formal scenes.");
            builder.AppendLine("- Does not continue the Fix03 overlay, `ignoreLayout`, or delayed post-drop inference route.");
            builder.AppendLine();
            builder.AppendLine("## Protocol Samples");
            builder.AppendLine();
            builder.AppendLine("| Sample | Action | Receiver | Source | Anchor | Occupied Cells | Expected Valid | Actual Valid | Reason | State | Receiver Mutated | Note |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |");
            foreach (ShapePlacementSessionSampleRow row in plan.Rows ?? new List<ShapePlacementSessionSampleRow>())
            {
                builder.AppendLine(
                    $"| `{Escape(row.SampleId)}` | `{Escape(row.Action)}` | `{Escape(row.ReceiverId)}` | `{Escape(row.ReceiverSource)}` | `{Escape(row.Anchor)}` | `{Escape(row.OccupiedCells)}` | `{row.ExpectedValid}` | `{row.ActualValid}` | `{row.InvalidReason}` | `{row.StateAfter}` | `{row.ReceiverMutated}` | {Escape(row.Note)} |");
            }

            builder.AppendLine();
            builder.AppendLine("## State Authority");
            builder.AppendLine();
            builder.AppendLine("- `ShapePlacementSession` owns selected item, shape id, rotation, source, anchors, occupied cells, active receiver, preview result, and commit/cancel state.");
            builder.AppendLine("- `ShapeItemPayload` is the explicit payload; drag/drop visuals are no longer implied as placement authority.");
            builder.AppendLine("- `ShapeGridReceiver` is the only placement-facing receiver contract for tray and board adapters.");
            builder.AppendLine("- Renderers should read a session snapshot and should never write placement authority.");

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildReceiverContractCsv(ShapePlacementSessionValidationPlan plan)
        {
            StringBuilder csv = new();
            csv.AppendLine("receiverId,source,canPlace,preview,commit,cancel,screenPointToCell,commitMode,formalUiTouched,formalFlowTouched");
            csv.AppendLine("dev_tray_receiver,Tray,yes,yes,yes,yes,yes,devOnly in-memory,false,false");
            csv.AppendLine("dev_board_receiver,Board,yes,yes,disabled,yes,yes,preview-only,false,false");
            csv.AppendLine("future_v03_item_tray_adapter,Tray,planned,planned,devOnly only,planned,slot enumeration,adapter boundary only,false,false");
            csv.AppendLine("future_v03_board_adapter,Board,planned,planned,disabled until authorized,planned,slot enumeration,preview-only,false,false");
            return csv.ToString();
        }

        private static string BuildLeakCheckReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            ShapePlacementSessionValidationPlan plan,
            int errors,
            int warnings,
            int leakCount)
        {
            string status = errors == 0 && leakCount == 0
                ? "PASS_DEVONLY_ISOLATED"
                : "BLOCKED_LEAK_CHECK";

            StringBuilder builder = new();
            builder.AppendLine("# ShapePlacementSession Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{ShapePlacementSessionValidator.PackageName}`");
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
            builder.AppendLine("- The package does not touch formal RunFlow, PageState, FormationState, SaveData, PlayerPrefs, Boss, rewards, drops, numeric config, DamageText, or V02FormationGridFrame.");
            builder.AppendLine("- The package does not modify formal V02/V03 scene layout or hand-tuned RectTransform values.");
            builder.AppendLine("- Board commit remains disabled in the protocol sample; formal multi-cell commit is deferred to a separately authorized package.");

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static int CountLeaks(ShapePlacementSessionValidationPlan plan)
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

        private static int CountFix03RouteLeaks(ShapePlacementSessionValidationPlan plan)
        {
            if (plan == null)
            {
                return 1;
            }

            int leaks = 0;
            if (plan.ContinuesFix03OverlayRoute) leaks++;
            if (plan.UsesIgnoreLayoutAuthority) leaks++;
            if (plan.UsesDelayedDropRead) leaks++;
            return leaks;
        }

        private static int CountUiRewriteLeaks(ShapePlacementSessionValidationPlan plan)
        {
            if (plan == null)
            {
                return 1;
            }

            int leaks = 0;
            if (plan.RedrawsItemTray) leaks++;
            if (plan.RedrawsBoard) leaks++;
            if (plan.WritesFormalScene) leaks++;
            if (plan.WritesFormalUi) leaks++;
            return leaks;
        }

        private static int CountFormalSystemLeaks(ShapePlacementSessionValidationPlan plan)
        {
            if (plan == null)
            {
                return 1;
            }

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
    }
}
#endif
