#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TalismanBag.BuildSandbox;
using UnityEditor;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BuildSandboxPlayableRegressionReportWriter
    {
        public const string MainReportPath = "Docs/V0.4/Reports/BuildSandboxPlayableRegressionReport.md";
        public const string ChecklistReportPath = "Docs/V0.4/Reports/BuildSandboxPlayableRegressionChecklist.csv";
        public const string LeakCheckReportPath = "Docs/V0.4/Reports/BuildSandboxPlayableRegressionLeakCheckReport.md";

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BuildSandboxPlayableRegressionSnapshot snapshot)
        {
            string projectRoot = Directory.GetParent(UnityEngine.Application.dataPath)?.FullName ?? string.Empty;
            string mainPath = Path.Combine(projectRoot, MainReportPath);
            string checklistPath = Path.Combine(projectRoot, ChecklistReportPath);
            string leakPath = Path.Combine(projectRoot, LeakCheckReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(mainPath) ?? projectRoot);

            BuildSandboxPlayableRegressionSnapshot safeSnapshot =
                snapshot ?? new BuildSandboxPlayableRegressionSnapshot();
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();

            File.WriteAllText(mainPath, BuildMainReport(safeReports, safeSnapshot), new UTF8Encoding(false));
            File.WriteAllText(checklistPath, BuildChecklistCsv(safeSnapshot), new UTF8Encoding(false));
            File.WriteAllText(leakPath, BuildLeakCheckReport(safeReports, safeSnapshot), new UTF8Encoding(false));
            AssetDatabase.Refresh();
            return new[] { mainPath, checklistPath, leakPath };
        }

        private static string BuildMainReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BuildSandboxPlayableRegressionSnapshot snapshot)
        {
            StringBuilder builder = new();
            builder.AppendLine("# BuildSandbox Playable Regression Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BuildSandboxPlayableRegression.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(snapshot.Passed ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{snapshot.errorCount}`");
            builder.AppendLine($"Warnings: `{snapshot.warningCount}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Final regression for the V0.4 BuildSandbox playable preview only.");
            builder.AppendLine("- Confirms the sandbox is ready for one full user handtest pass.");
            builder.AppendLine("- Does not promote 3-10 / 4-10 into formal content.");
            builder.AppendLine("- Does not open feature flags, write formal saves, write formal rewards/drops, or run formal chapter progress.");
            builder.AppendLine("- Does not run scene binders or move current hand-tuned UI layout.");
            builder.AppendLine();
            builder.AppendLine("## Regression Summary");
            builder.AppendLine();
            builder.AppendLine("| Area | Result | Evidence |");
            builder.AppendLine("| --- | --- | --- |");
            builder.AppendLine($"| V04 scene open/play readiness | `{PassFail(snapshot.sceneExists && snapshot.sceneBindingPass)}` | sceneExists={snapshot.sceneExists}; binding={snapshot.sceneBindingPass} |");
            builder.AppendLine($"| x2/x3/x4 tray coverage | `{PassFail(snapshot.itemShapeCoveragePass)}` | x2={snapshot.x2TrayItemCount}; x3={snapshot.x3TrayItemCount}; x4={snapshot.x4TrayItemCount} |");
            builder.AppendLine($"| Rotate scope | `{PassFail(snapshot.rotationScopePass)}` | tray info rotate path guarded |");
            builder.AppendLine($"| Drag/place/illegal return | `{PassFail(snapshot.placementSamplesPass)}` | legal={snapshot.legalPlacementSampleCount}; illegalReturn={snapshot.illegalReturnSampleCount} |");
            builder.AppendLine($"| Continue battle board read | `{PassFail(snapshot.currentBoardReadPass)}` | combat preview reads current board snapshot |");
            builder.AppendLine($"| Build-dependent Boss feedback | `{PassFail(snapshot.buildFeedbackVariationPass)}` | bossState={snapshot.bossStateRowCount}; cast={snapshot.castBarRowCount}; mechanic={snapshot.mechanicFloatingRowCount}; logs={snapshot.combatLogRowCount}; distinctBuildFeedback={snapshot.distinctBuildFeedbackCount} |");
            builder.AppendLine($"| 3-10 / 4-10 devOnly balance rows | `{PassFail(snapshot.devChapterBalancePass)}` | 3-10={snapshot.devBalance310StageCount}; 4-10={snapshot.devBalance410StageCount} |");
            builder.AppendLine($"| Player answer leak check | `{PassFail(snapshot.playerLeakPass)}` | leaks={snapshot.playerSideAnswerLeakCount} |");
            builder.AppendLine($"| Feature flags default false | `{PassFail(snapshot.featureFlagsDefaultFalsePass)}` | defaultTrue={snapshot.featureFlagDefaultTrueCount} |");
            builder.AppendLine($"| devOnly/isEnabled isolation | `{PassFail(snapshot.devOnlyDisabledPass)}` | devOnlyFalse={snapshot.devOnlyFalseCount}; isEnabledTrue={snapshot.isEnabledTrueCount} |");
            builder.AppendLine($"| Formal scope isolation | `{PassFail(snapshot.formalScopePass && snapshot.noFormalSceneOrLayoutWritePass)}` | formalLeaks={snapshot.formalFlowLeakCount} |");
            builder.AppendLine();
            builder.AppendLine("## User Handtest Checklist");
            builder.AppendLine();
            builder.AppendLine("1. Open `Scene_TalismanBag_V04_BattleSandboxPreview` directly, press Play, and confirm Console has no red Error or yellow Warning from this package.");
            builder.AppendLine("2. Confirm the item tray visibly contains x2, x3, and x4 multi-cell items.");
            builder.AppendLine("3. Select tray items, use the item info rotate control, and confirm dragging or board-placed items do not rotate unexpectedly.");
            builder.AppendLine("4. Drag x2/x3/x4 items to legal board cells and confirm they remain placed without snapping back, jumping, or disappearing.");
            builder.AppendLine("5. Drag an item out of bounds or over occupied cells and confirm it returns cleanly to the tray/state before the drag.");
            builder.AppendLine("6. Use prepare to continue battle after changing the board, then confirm Boss status, cast bar, floating mechanic text, and combat log refresh.");
            builder.AppendLine("7. Try at least two different build layouts and confirm Boss feedback changes in visible wording or timing.");
            builder.AppendLine("8. Review 3-10 / 4-10 devOnly balance rows and confirm the difficulty suggestions are readable.");
            builder.AppendLine("9. Confirm player-visible UI does not show complete solution keys, answer weights, required tags, required affixes, required stats, or full Boss key answers.");
            builder.AppendLine("10. Confirm no formal run flow, save data, rewards, drops, chapter progress, or feature flag activation appears during the sandbox test.");
            builder.AppendLine();
            builder.AppendLine("## Checklist Rows");
            builder.AppendLine();
            builder.AppendLine("| ID | Area | Result | User Handtest | Check | Evidence |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- |");
            foreach (BuildSandboxPlayableRegressionChecklistRow row in snapshot.checklistRows
                         ?? new List<BuildSandboxPlayableRegressionChecklistRow>())
            {
                builder.AppendLine(
                    $"| `{Escape(row.id)}` | {Escape(row.area)} | `{Escape(row.result)}` | `{row.userHandtestRequired}` | {Escape(row.check)} | {Escape(row.evidence)} |");
            }

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildChecklistCsv(BuildSandboxPlayableRegressionSnapshot snapshot)
        {
            StringBuilder builder = new();
            builder.AppendLine("id,area,check,method,result,evidence,userHandtestRequired,notes");
            foreach (BuildSandboxPlayableRegressionChecklistRow row in snapshot.checklistRows
                         ?? new List<BuildSandboxPlayableRegressionChecklistRow>())
            {
                builder.AppendLine(Csv(
                    row.id,
                    row.area,
                    row.check,
                    row.method,
                    row.result,
                    row.evidence,
                    row.userHandtestRequired.ToString(),
                    row.notes));
            }

            return builder.ToString();
        }

        private static string BuildLeakCheckReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BuildSandboxPlayableRegressionSnapshot snapshot)
        {
            StringBuilder builder = new();
            builder.AppendLine("# BuildSandbox Playable Regression Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BuildSandboxPlayableRegression.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(snapshot.Passed ? "PASS" : "FAIL")}`");
            builder.AppendLine();
            builder.AppendLine("## Leak Counters");
            builder.AppendLine();
            builder.AppendLine("| Check | Count | Expected | Result |");
            builder.AppendLine("| --- | ---: | ---: | --- |");
            AppendLeakRow(builder, "featureFlagDefaultTrue", snapshot.featureFlagDefaultTrueCount);
            AppendLeakRow(builder, "playerSideAnswerLeaks", snapshot.playerSideAnswerLeakCount);
            AppendLeakRow(builder, "formalFlowOrDataLeaks", snapshot.formalFlowLeakCount);
            AppendLeakRow(builder, "devOnlyFalse", snapshot.devOnlyFalseCount);
            AppendLeakRow(builder, "isEnabledTrue", snapshot.isEnabledTrueCount);
            AppendLeakRow(builder, "validatorErrors", snapshot.errorCount);
            AppendLeakRow(builder, "validatorWarnings", snapshot.warningCount);
            builder.AppendLine();
            builder.AppendLine("## Player-Side Forbidden Surface");
            builder.AppendLine();
            builder.AppendLine("- hard solution tags: hidden from player-facing sandbox UI.");
            builder.AppendLine("- required synergy / affix / stat fields: hidden from player-facing sandbox UI.");
            builder.AppendLine("- drop bias and answer weights: hidden from player-facing sandbox UI.");
            builder.AppendLine("- complete Boss key answers: hidden from player-facing sandbox UI.");
            builder.AppendLine();
            builder.AppendLine("## Formal Scope Confirmation");
            builder.AppendLine();
            builder.AppendLine("- Feature flags remain default false.");
            builder.AppendLine("- Sandbox surfaces remain devOnly and disabled by default.");
            builder.AppendLine("- Formal run flow, page state, formation state, save data, player prefs, reward, drop, Boss progression, and chapter progression are not written.");
            builder.AppendLine("- Formal V02/V03 scenes are not modified by this regression package.");
            builder.AppendLine("- Current V04 hand-tuned layout is not rebound or moved by this regression package.");
            AppendValidationSummary(builder, reports);
            return builder.ToString();
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
            foreach (BuildSandboxValidationReport report in reports
                         ?? Array.Empty<BuildSandboxValidationReport>())
            {
                builder.AppendLine(
                    $"| {Escape(report.Name)} | `{(report.Passed ? "PASS" : "FAIL")}` | {report.ErrorCount} | {report.WarningCount} | {report.InfoCount} |");
            }

            builder.AppendLine();
            builder.AppendLine("## Issues");
            builder.AppendLine();
            builder.AppendLine("| Level | Code | Message | Path |");
            builder.AppendLine("| --- | --- | --- | --- |");
            foreach (BuildSandboxValidationIssue issue in (reports ?? Array.Empty<BuildSandboxValidationReport>())
                         .SelectMany(report => report.Issues))
            {
                builder.AppendLine(
                    $"| `{issue.Level}` | `{Escape(issue.Code)}` | {Escape(issue.Message)} | `{Escape(issue.AssetPath)}` |");
            }
        }

        private static void AppendLeakRow(StringBuilder builder, string label, int count)
        {
            builder.AppendLine($"| `{Escape(label)}` | {count} | 0 | `{(count == 0 ? "PASS" : "FAIL")}` |");
        }

        private static string PassFail(bool passed)
        {
            return passed ? "PASS" : "FAIL";
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
