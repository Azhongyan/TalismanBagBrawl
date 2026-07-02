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
    public static class MobileShapePlacementReportWriter
    {
        public const string MainReportPath =
            "Docs/V0.4/Reports/MobileShapePlacementInteractionReport.md";

        public const string StateMachineReportPath =
            "Docs/V0.4/Reports/MobileShapePlacementStateMachineReport.csv";

        public const string LeakCheckReportPath =
            "Docs/V0.4/Reports/MobileShapePlacementLeakCheckReport.md";

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            MobileShapePlacementValidationPlan plan = null)
        {
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();
            MobileShapePlacementValidationPlan safePlan =
                plan ?? MobileShapePlacementValidator.BuildDefaultPlan();

            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string mainPath = Path.Combine(projectRoot, MainReportPath);
            string stateMachinePath = Path.Combine(projectRoot, StateMachineReportPath);
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
                stateMachinePath,
                BuildStateMachineCsv(safePlan),
                new UTF8Encoding(false));
            File.WriteAllText(
                leakPath,
                BuildLeakCheckReport(safeReports, safePlan, errors, warnings, leakCount),
                new UTF8Encoding(false));

            AssetDatabase.Refresh();
            return new[] { mainPath, stateMachinePath, leakPath };
        }

        private static string BuildMainReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            MobileShapePlacementValidationPlan plan,
            int errors,
            int warnings,
            int leakCount)
        {
            string status = errors == 0 && leakCount == 0
                ? "PASS_DEVONLY_MOBILE_PLACEMENT_READY"
                : "BLOCKED_MOBILE_PLACEMENT_ERRORS";

            StringBuilder builder = new();
            builder.AppendLine("# Mobile Shape Placement Interaction Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{MobileShapePlacementValidator.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{status}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine($"Leak Count: `{leakCount}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Adds a BuildSandbox/devOnly mobile placement input protocol.");
            builder.AppendLine("- Reuses `ShapePlacementSession`, `ShapeItemPayload`, `ShapeGridReceiver`, and `ShapeAwareItemTrayGrid` direction as placement authority.");
            builder.AppendLine("- Models tap select, tap rotate, drag ghost preview, release-to-lock preview, tap ghost confirm, invalid feedback, and cancel.");
            builder.AppendLine("- Does not redraw the mature BattlePrepare board, item tray, drag feel, or pull-up animation.");
            builder.AppendLine("- Does not write formal SaveData, MainTrialProgressData, PlayerPrefs, RunFlow, Boss, reward, drop, or numeric systems.");
            builder.AppendLine();
            builder.AppendLine("## Input Thresholds");
            builder.AppendLine();
            builder.AppendLine("| Setting | Value | Required |");
            builder.AppendLine("| --- | ---: | ---: |");
            builder.AppendLine($"| `tapMoveThresholdPixels` | {plan.tapMoveThresholdPixels:0.##} | 15 |");
            builder.AppendLine($"| `tapTimeThresholdMilliseconds` | {plan.tapTimeThresholdMilliseconds:0.##} | 250 |");
            builder.AppendLine($"| `fingerGhostOffsetPixels` | {plan.fingerGhostOffsetPixels:0.##} | 40-60 |");
            builder.AppendLine($"| `tapGestureSample` | `{plan.tapGestureKind}` | `Tap` |");
            builder.AppendLine($"| `dragGestureSample` | `{plan.dragGestureKind}` | `Drag` |");
            builder.AppendLine();
            builder.AppendLine("## Protocol Coverage");
            builder.AppendLine();
            builder.AppendLine("| Check | Result |");
            builder.AppendLine("| --- | --- |");
            builder.AppendLine($"| `PreviewLocked` implemented | `{plan.previewLockedImplemented}` |");
            builder.AppendLine($"| release does not commit | `{plan.releaseDoesNotCommit}` |");
            builder.AppendLine($"| tap ghost commits | `{plan.tapGhostCommits}` |");
            builder.AppendLine($"| cancel available | `{plan.cancelAvailable}` |");
            builder.AppendLine($"| PlacedItemEdit | `{(plan.placedItemEditImplemented ? "IMPLEMENTED" : PlacedItemEditExtension.Status)}` |");
            builder.AppendLine();
            builder.AppendLine("## State Machine Samples");
            builder.AppendLine();
            builder.AppendLine("| Sample | Action | From | To | Anchor | Cells | Ghost | Hint | Release Submitted | Commit Count | Note |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |");
            foreach (MobileShapePlacementStateMachineRow row in plan.Rows ?? new List<MobileShapePlacementStateMachineRow>())
            {
                builder.AppendLine(
                    $"| `{Escape(row.SampleId)}` | `{Escape(row.Action)}` | `{row.FromState}` | `{row.ToState}` | `{Escape(row.Anchor)}` | `{Escape(row.OccupiedCells)}` | `{row.GhostOutline}` | {Escape(row.GhostHint)} | `{row.ReleaseSubmitted}` | `{row.ReceiverCommitBefore}->{row.ReceiverCommitAfter}` | {Escape(row.Note)} |");
            }

            builder.AppendLine();
            builder.AppendLine("## Shape Support");
            builder.AppendLine();
            builder.AppendLine("| Shape | Cells | Unique Rotations | Can Rotate | Offsets Source |");
            builder.AppendLine("| --- | ---: | ---: | --- | --- |");
            foreach (MobileShapePlacementShapeSupportRow row in plan.ShapeRows ?? new List<MobileShapePlacementShapeSupportRow>())
            {
                builder.AppendLine(
                    $"| `{Escape(row.ShapeId)}` | {row.CellCount} | {row.UniqueRotationCount} | `{row.CanRotate}` | `{Escape(row.Offsets)}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## Reuse / Isolation");
            builder.AppendLine();
            builder.AppendLine($"- Mature BattlePrepare reused as base component: `{plan.reusesMatureBattlePrepare}`.");
            builder.AppendLine($"- Redraws board: `{plan.redrawsBoard}`.");
            builder.AppendLine($"- Redraws item tray: `{plan.redrawsItemTray}`.");
            builder.AppendLine($"- Rewrites drag feel: `{plan.rewritesDragFeel}`.");
            builder.AppendLine($"- Rewrites pull-up animation: `{plan.rewritesPullUpAnimation}`.");
            builder.AppendLine($"- Feature flags default true: `{plan.featureFlagDefaultEnabled}`.");
            builder.AppendLine("- Preview stage only updates ghost state, hints, rotation preview, and occupied-cell preview.");
            builder.AppendLine("- Confirm stage commits only to the devOnly in-memory receiver sample.");

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildStateMachineCsv(MobileShapePlacementValidationPlan plan)
        {
            StringBuilder builder = new();
            builder.AppendLine("sampleId,action,fromState,toState,expectedState,expectedValid,actualValid,anchor,occupiedCells,invalidReason,ghostVisible,ghostOutline,ghostHint,previewLocked,releaseSubmitted,receiverCommitBefore,receiverCommitAfter,note");
            foreach (MobileShapePlacementStateMachineRow row in plan.Rows ?? new List<MobileShapePlacementStateMachineRow>())
            {
                builder.AppendLine(string.Join(
                    ",",
                    Csv(row.SampleId),
                    Csv(row.Action),
                    row.FromState.ToString(),
                    row.ToState.ToString(),
                    row.ExpectedState.ToString(),
                    row.ExpectedValid.ToString(),
                    row.ActualValid.ToString(),
                    Csv(row.Anchor),
                    Csv(row.OccupiedCells),
                    row.InvalidReason.ToString(),
                    row.GhostVisible.ToString(),
                    row.GhostOutline.ToString(),
                    Csv(row.GhostHint),
                    row.PreviewLocked.ToString(),
                    row.ReleaseSubmitted.ToString(),
                    row.ReceiverCommitBefore.ToString(),
                    row.ReceiverCommitAfter.ToString(),
                    Csv(row.Note)));
            }

            builder.AppendLine();
            builder.AppendLine("shapeId,offsets,cellCount,expectedCellCount,uniqueRotationCount,expectedUniqueRotationCount,canRotate");
            foreach (MobileShapePlacementShapeSupportRow row in plan.ShapeRows ?? new List<MobileShapePlacementShapeSupportRow>())
            {
                builder.AppendLine(string.Join(
                    ",",
                    Csv(row.ShapeId),
                    Csv(row.Offsets),
                    row.CellCount.ToString(),
                    row.ExpectedCellCount.ToString(),
                    row.UniqueRotationCount.ToString(),
                    row.ExpectedUniqueRotationCount.ToString(),
                    row.CanRotate.ToString()));
            }

            return builder.ToString();
        }

        private static string BuildLeakCheckReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            MobileShapePlacementValidationPlan plan,
            int errors,
            int warnings,
            int leakCount)
        {
            string status = errors == 0 && leakCount == 0
                ? "PASS_DEVONLY_ISOLATED"
                : "BLOCKED_LEAK_CHECK";

            StringBuilder builder = new();
            builder.AppendLine("# Mobile Shape Placement Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{MobileShapePlacementValidator.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{status}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine();
            builder.AppendLine("## Leak Counters");
            builder.AppendLine();
            builder.AppendLine("| Check | Count | Expected |");
            builder.AppendLine("| --- | ---: | ---: |");
            builder.AppendLine($"| `featureFlagDefaultTrue` | {(plan.featureFlagDefaultEnabled ? 1 : 0)} | 0 |");
            builder.AppendLine($"| `uiRewriteLeaks` | {CountUiRewriteLeaks(plan)} | 0 |");
            builder.AppendLine($"| `formalSystemLeaks` | {CountFormalSystemLeaks(plan)} | 0 |");
            builder.AppendLine($"| `devOnlyLeaks` | {CountDevOnlyLeaks(plan)} | 0 |");
            builder.AppendLine($"| `totalLeaks` | {leakCount} | 0 |");
            builder.AppendLine();
            builder.AppendLine("## Scope Confirmation");
            builder.AppendLine();
            builder.AppendLine("- The package remains BuildSandbox/devOnly and disabled by default.");
            builder.AppendLine("- No formal V02/V03 scenes, hand-tuned RectTransform values, mature BattlePrepare layouts, or Build Settings are written.");
            builder.AppendLine("- No formal RunFlow, PageState, FormationState, V02RunFlowController, V02FormationGridFrame, DamageText, SaveData, Boss, reward, drop, or numeric systems are touched.");
            builder.AppendLine("- The old Fix03 overlay / ignoreLayout / delayed one-frame drop route is not used as placement authority.");
            builder.AppendLine("- PlacedItemEdit is intentionally deferred instead of half-implemented.");

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static int CountLeaks(MobileShapePlacementValidationPlan plan)
        {
            if (plan == null)
            {
                return 1;
            }

            int leaks = 0;
            if (plan.featureFlagDefaultEnabled) leaks++;
            leaks += CountUiRewriteLeaks(plan);
            leaks += CountFormalSystemLeaks(plan);
            leaks += CountDevOnlyLeaks(plan);
            return leaks;
        }

        private static int CountUiRewriteLeaks(MobileShapePlacementValidationPlan plan)
        {
            if (plan == null)
            {
                return 1;
            }

            int leaks = 0;
            if (plan.redrawsBoard) leaks++;
            if (plan.redrawsItemTray) leaks++;
            if (plan.rewritesDragFeel) leaks++;
            if (plan.rewritesPullUpAnimation) leaks++;
            if (plan.writesFormalScene) leaks++;
            if (plan.writesFormalUi) leaks++;
            if (plan.overwritesHandTunedRectTransform) leaks++;
            return leaks;
        }

        private static int CountFormalSystemLeaks(MobileShapePlacementValidationPlan plan)
        {
            if (plan == null)
            {
                return 1;
            }

            int leaks = 0;
            if (plan.touchesRunFlow) leaks++;
            if (plan.touchesPageState) leaks++;
            if (plan.touchesFormationState) leaks++;
            if (plan.touchesSaveData) leaks++;
            if (plan.touchesBossRewardDropNumeric) leaks++;
            return leaks;
        }

        private static int CountDevOnlyLeaks(MobileShapePlacementValidationPlan plan)
        {
            if (plan == null)
            {
                return 1;
            }

            int leaks = 0;
            if (!plan.devOnly) leaks++;
            if (plan.isEnabled) leaks++;
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
