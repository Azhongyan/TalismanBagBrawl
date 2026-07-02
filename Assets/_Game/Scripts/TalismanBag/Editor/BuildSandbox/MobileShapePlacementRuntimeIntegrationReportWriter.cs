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
    public static class MobileShapePlacementRuntimeIntegrationReportWriter
    {
        public const string MainReportPath =
            "Docs/V0.4/Reports/MobileShapePlacementRuntimeIntegrationReport.md";

        public const string StateReportPath =
            "Docs/V0.4/Reports/MobileShapePlacementRuntimeIntegrationStateReport.csv";

        public const string LeakCheckReportPath =
            "Docs/V0.4/Reports/MobileShapePlacementRuntimeIntegrationLeakCheckReport.md";

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            MobileShapePlacementRuntimeIntegrationPlan plan = null)
        {
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();
            MobileShapePlacementRuntimeIntegrationPlan safePlan =
                plan ?? MobileShapePlacementRuntimeIntegrationValidator.BuildDefaultPlan();

            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string mainPath = Path.Combine(projectRoot, MainReportPath);
            string statePath = Path.Combine(projectRoot, StateReportPath);
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
                statePath,
                BuildStateCsv(safePlan),
                new UTF8Encoding(false));
            File.WriteAllText(
                leakPath,
                BuildLeakCheckReport(safeReports, safePlan, errors, warnings, leakCount),
                new UTF8Encoding(false));

            AssetDatabase.Refresh();
            return new[] { mainPath, statePath, leakPath };
        }

        private static string BuildMainReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            MobileShapePlacementRuntimeIntegrationPlan plan,
            int errors,
            int warnings,
            int leakCount)
        {
            string status = errors == 0 && leakCount == 0
                ? "PASS_RUNTIMEPLAYTEST_MOBILE_SHAPE_PLACEMENT_CONNECTED"
                : "BLOCKED_RUNTIMEPLAYTEST_MOBILE_SHAPE_PLACEMENT";

            StringBuilder builder = new();
            builder.AppendLine("# Mobile Shape Placement Runtime Integration Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{MobileShapePlacementRuntimeIntegrationValidator.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{status}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine($"Leak Count: `{leakCount}`");
            builder.AppendLine();
            builder.AppendLine("## RuntimePlaytest Wiring");
            builder.AppendLine();
            builder.AppendLine("| Check | Value |");
            builder.AppendLine("| --- | --- |");
            builder.AppendLine($"| `runtimePlaytestConnected` | `{plan.runtimePlaytestConnected}` |");
            builder.AppendLine($"| `usesShapePlacementSession` | `{plan.usesShapePlacementSession}` |");
            builder.AppendLine($"| `usesShapeAwareItemTrayGrid` | `{plan.usesShapeAwareItemTrayGrid}` |");
            builder.AppendLine($"| `usesMobileShapePlacementInputExtension` | `{plan.usesMobileShapePlacementInputExtension}` |");
            builder.AppendLine($"| `usesSessionAuthority` | `{plan.usesSessionAuthority}` |");
            builder.AppendLine($"| `usesBattlePrepareShapePlacementSeamAdapter` | `{plan.usesBattlePrepareShapePlacementSeamAdapter}` |");
            builder.AppendLine($"| `bindsBattlePrepareShapePlacementSeamCallbacks` | `{plan.bindsBattlePrepareShapePlacementSeamCallbacks}` |");
            builder.AppendLine($"| `seamAdapterConsumesPreviewGhost` | `{plan.seamAdapterConsumesPreviewGhost}` |");
            builder.AppendLine($"| `usesDirectExternalItemHooks` | `{plan.usesDirectExternalItemHooks}` |");
            builder.AppendLine($"| source path | `{Escape(plan.runtimePlaytestSourcePath)}` |");
            builder.AppendLine();
            builder.AppendLine("## User Flow Coverage");
            builder.AppendLine();
            builder.AppendLine("| Sample | Action | From | To | Anchor | Cells | Ghost | Release Submitted | Commit Count | Note |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |");
            foreach (MobileShapePlacementRuntimeIntegrationStateRow row in plan.Rows ?? new List<MobileShapePlacementRuntimeIntegrationStateRow>())
            {
                builder.AppendLine(
                    $"| `{Escape(row.SampleId)}` | `{Escape(row.Action)}` | `{row.FromState}` | `{row.ToState}` | `{Escape(row.Anchor)}` | `{Escape(row.OccupiedCells)}` | `{row.GhostOutline}` | `{row.ReleaseSubmitted}` | `{row.ReceiverCommitBefore}->{row.ReceiverCommitAfter}` | {Escape(row.Note)} |");
            }

            builder.AppendLine();
            builder.AppendLine("## Source Guard");
            builder.AppendLine();
            builder.AppendLine("| Check | Value |");
            builder.AppendLine("| --- | --- |");
            builder.AppendLine($"| RuntimePlaytest owns integration | `{plan.runtimePlaytestSourceHasIntegration}` |");
            builder.AppendLine($"| RuntimePlaytest owns board receiver | `{plan.runtimePlaytestSourceHasBoardReceiver}` |");
            builder.AppendLine($"| RuntimePlaytest owns ghost target | `{plan.runtimePlaytestSourceHasGhostTarget}` |");
            builder.AppendLine($"| Calls `TapTrayItem` | `{plan.runtimePlaytestSourceHasTapTrayItem}` |");
            builder.AppendLine($"| Calls `ReleaseDragLockPreview` | `{plan.runtimePlaytestSourceHasReleaseLock}` |");
            builder.AppendLine($"| Calls `TapGhostToConfirm` | `{plan.runtimePlaytestSourceHasTapGhostConfirm}` |");
            builder.AppendLine($"| Calls `Cancel` | `{plan.runtimePlaytestSourceHasCancel}` |");
            builder.AppendLine($"| Injects `BattlePrepareShapePlacementSeamAdapter` | `{plan.usesBattlePrepareShapePlacementSeamAdapter}` |");
            builder.AppendLine($"| Binds seam callbacks | `{plan.bindsBattlePrepareShapePlacementSeamCallbacks}` |");
            builder.AppendLine($"| Adapter consumes `PreviewGhost` | `{plan.seamAdapterConsumesPreviewGhost}` |");
            builder.AppendLine($"| RuntimePlaytest direct item hooks | `{plan.usesDirectExternalItemHooks}` |");
            builder.AppendLine($"| Restores old Fix03 route | `{plan.restoresOverlayIgnoreLayoutDelayedDrop}` |");
            builder.AppendLine();
            builder.AppendLine("## Notes");
            builder.AppendLine();
            builder.AppendLine("- RuntimePlaytest now enters placement through `BattlePrepareShapePlacementSeamAdapter` and then drives the shared session/input/tray-grid protocol.");
            builder.AppendLine("- Release locks the ghost preview; only clicking the locked ghost commits to the devOnly receiver.");
            builder.AppendLine("- Commit remains runtime-only and does not write formal RunFlow, SaveData, Boss, reward, drop, numeric, scene, or UI assets.");
            builder.AppendLine("- Direct RuntimePlaytest item-event hooks and the rejected overlay / layout-ignore / delayed drop route are guarded as absent.");

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildStateCsv(MobileShapePlacementRuntimeIntegrationPlan plan)
        {
            StringBuilder builder = new();
            builder.AppendLine("sampleId,action,fromState,toState,expectedState,expectedValid,actualValid,anchor,occupiedCells,invalidReason,ghostVisible,ghostOutline,ghostHint,releaseSubmitted,receiverCommitBefore,receiverCommitAfter,note");
            foreach (MobileShapePlacementRuntimeIntegrationStateRow row in plan.Rows ?? new List<MobileShapePlacementRuntimeIntegrationStateRow>())
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
                    row.ReleaseSubmitted.ToString(),
                    row.ReceiverCommitBefore.ToString(),
                    row.ReceiverCommitAfter.ToString(),
                    Csv(row.Note)));
            }

            return builder.ToString();
        }

        private static string BuildLeakCheckReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            MobileShapePlacementRuntimeIntegrationPlan plan,
            int errors,
            int warnings,
            int leakCount)
        {
            string status = errors == 0 && leakCount == 0
                ? "PASS_DEVONLY_ISOLATED"
                : "BLOCKED_LEAK_CHECK";

            StringBuilder builder = new();
            builder.AppendLine("# Mobile Shape Placement Runtime Integration Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{MobileShapePlacementRuntimeIntegrationValidator.PackageName}`");
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
            builder.AppendLine($"| `directExternalItemEventHooks` | {(plan.usesDirectExternalItemHooks ? 1 : 0)} | 0 |");
            builder.AppendLine($"| `oldFix03RouteRestored` | {(plan.restoresOverlayIgnoreLayoutDelayedDrop ? 1 : 0)} | 0 |");
            builder.AppendLine($"| `totalLeaks` | {leakCount} | 0 |");
            builder.AppendLine();
            builder.AppendLine("## Boundary Confirmation");
            builder.AppendLine();
            builder.AppendLine("- No mature board, mature item tray, or pull-up animation redraw is introduced.");
            builder.AppendLine("- No user-tuned RectTransform is overwritten.");
            builder.AppendLine("- No formal RunFlow, SaveData, Boss, reward, drop, numeric, or scene asset is written.");
            builder.AppendLine("- The old overlay + layout-ignore + delayed drop route remains absent.");

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static int CountLeaks(MobileShapePlacementRuntimeIntegrationPlan plan)
        {
            if (plan == null)
            {
                return 1;
            }

            int leaks = 0;
            if (plan.featureFlagDefaultEnabled) leaks++;
            if (plan.restoresOverlayIgnoreLayoutDelayedDrop) leaks++;
            if (plan.usesDirectExternalItemHooks) leaks++;
            leaks += CountUiRewriteLeaks(plan);
            leaks += CountFormalSystemLeaks(plan);
            leaks += CountDevOnlyLeaks(plan);
            return leaks;
        }

        private static int CountUiRewriteLeaks(MobileShapePlacementRuntimeIntegrationPlan plan)
        {
            if (plan == null)
            {
                return 1;
            }

            int leaks = 0;
            if (plan.redrawsBoard) leaks++;
            if (plan.redrawsItemTray) leaks++;
            if (plan.rewritesPullUpAnimation) leaks++;
            if (plan.overwritesHandTunedRectTransform) leaks++;
            return leaks;
        }

        private static int CountFormalSystemLeaks(MobileShapePlacementRuntimeIntegrationPlan plan)
        {
            if (plan == null)
            {
                return 1;
            }

            int leaks = 0;
            if (plan.touchesRunFlow) leaks++;
            if (plan.touchesSaveData) leaks++;
            if (plan.touchesBossRewardDropNumeric) leaks++;
            return leaks;
        }

        private static int CountDevOnlyLeaks(MobileShapePlacementRuntimeIntegrationPlan plan)
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
