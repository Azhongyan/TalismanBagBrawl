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
    public static class BattlePrepareComponentAdapterRuntimePlaytestReportWriter
    {
        public const string MainReportPath =
            "Docs/V0.4/Reports/BattlePrepareComponentAdapterRuntimePlaytestReport.md";

        public const string StepsReportPath =
            "Docs/V0.4/Reports/BattlePrepareComponentAdapterRuntimePlaytestSteps.md";

        public const string LeakCheckReportPath =
            "Docs/V0.4/Reports/BattlePrepareComponentAdapterRuntimePlaytestLeakCheckReport.md";

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattlePrepareComponentAdapterRuntimePlaytestPlan plan = null)
        {
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();
            BattlePrepareComponentAdapterRuntimePlaytestPlan safePlan =
                plan ?? BattlePrepareComponentAdapterRuntimePlaytestValidator.BuildDefaultPlan();
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string mainPath = Path.Combine(projectRoot, MainReportPath);
            string stepsPath = Path.Combine(projectRoot, StepsReportPath);
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
                stepsPath,
                BuildStepsReport(safePlan),
                new UTF8Encoding(false));
            File.WriteAllText(
                leakPath,
                BuildLeakCheckReport(safeReports, safePlan, errors, warnings, leakCount),
                new UTF8Encoding(false));

            AssetDatabase.Refresh();
            return new[] { mainPath, stepsPath, leakPath };
        }

        private static string BuildMainReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattlePrepareComponentAdapterRuntimePlaytestPlan plan,
            int errors,
            int warnings,
            int leakCount)
        {
            string status = errors == 0 && leakCount == 0
                ? "READY_RUNTIME_PLAYTEST"
                : "RUNTIME_PLAYTEST_BLOCKED";

            StringBuilder builder = new();
            builder.AppendLine("# BattlePrepare Component Adapter Runtime Playtest Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattlePrepareComponentAdapterRuntimePlaytestPlan.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{status}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine($"Leak Count: `{leakCount}`");
            builder.AppendLine();
            builder.AppendLine("## Runtime Entry");
            builder.AppendLine();
            builder.AppendLine($"- Manual Menu Path: `{Escape(plan.manualMenuPath)}`");
            builder.AppendLine($"- QA Menu Path: `{Escape(plan.qaMenuPath)}`");
            builder.AppendLine($"- Target Scene: `{Escape(plan.targetScenePath)}`");
            builder.AppendLine($"- validationMode: `{Escape(plan.validationMode)}`");
            builder.AppendLine($"- realUnityPlayTouchablePath: `{plan.realUnityPlayTouchablePath}`");
            builder.AppendLine($"- automationClaimsHandFeelPass: `{plan.automationClaimsHandFeelPass}`");
            builder.AppendLine($"- staticReportOnly: `{plan.staticReportOnly}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Opens the existing V02 FormationCounter scene and calls V03 BattlePrepare through its mature runtime entrypoint.");
            builder.AppendLine("- Installs a DontSave V0.4 extension layer in Play for multi-cell shape, rotation, and occupancy feedback.");
            builder.AppendLine("- Reuses mature BattlePrepare board, item tray, drag gesture, and pull-up motion. It does not redraw those systems.");
            builder.AppendLine("- QA validation confirms entry/isolation only. Human hand-feel pass must come from running the Manual Only menu in Unity Play.");
            builder.AppendLine();
            builder.AppendLine("## Guardrail Answers");
            builder.AppendLine();
            builder.AppendLine("| Question | Answer |");
            builder.AppendLine("| --- | --- |");
            builder.AppendLine($"| Reuses mature BattlePrepare components | `{plan.usesMatureBoardTrayDragPullup}` |");
            builder.AppendLine($"| Only adds Extension / Adapter layer | `{!plan.writesFormalUi && !plan.writesFormalScene}` |");
            builder.AppendLine($"| Redrew V04 board / tray | `{plan.redrewV04Board || plan.redrewV04ItemTray}` |");
            builder.AppendLine($"| Rewrote drag / pull-up animation | `{plan.rewroteDragFeel || plan.rewrotePullUpAnimation}` |");
            builder.AppendLine($"| Modified V02 / V03 formal scenes | `{plan.writesFormalScene}` |");
            builder.AppendLine($"| Overwrote user hand-tuned RectTransform | `{plan.overwritesHandTunedRectTransform}` |");
            builder.AppendLine($"| Connected formal RunFlow / PageState / FormationState / SaveData | `{plan.touchesRunFlow || plan.touchesPageState || plan.touchesFormationState || plan.touchesSaveData}` |");
            builder.AppendLine($"| Connected Boss / reward / drop / numeric systems | `{plan.touchesBossRewardDropNumeric}` |");
            builder.AppendLine($"| FeatureFlag default enabled | `{plan.featureFlagDefaultEnabled}` |");
            builder.AppendLine();
            builder.AppendLine("## Runtime Surfaces");
            builder.AppendLine();
            builder.AppendLine("| Surface | Mature Source | Touchable In Play | Extension Only | Evidence |");
            builder.AppendLine("| --- | --- | --- | --- | --- |");
            foreach (BattlePrepareComponentAdapterRuntimeSurfaceRow row in plan.runtimeSurfaces ?? new List<BattlePrepareComponentAdapterRuntimeSurfaceRow>())
            {
                builder.AppendLine(
                    $"| {Escape(row.chineseDisplayName)} | `{Escape(row.matureSource)}` | `{row.touchableInPlay}` | `{row.extensionOnly}` | {Escape(row.runtimeEvidence)} |");
            }

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildStepsReport(BattlePrepareComponentAdapterRuntimePlaytestPlan plan)
        {
            StringBuilder builder = new();
            builder.AppendLine("# BattlePrepare Component Adapter Runtime Playtest Steps");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattlePrepareComponentAdapterRuntimePlaytestPlan.PackageName}`");
            builder.AppendLine($"Manual Menu Path: `{Escape(plan.manualMenuPath)}`");
            builder.AppendLine();
            builder.AppendLine("## Manual Unity Play Steps");
            builder.AppendLine();
            builder.AppendLine("| Step | Action | Expected |");
            builder.AppendLine("| --- | --- | --- |");
            foreach (BattlePrepareComponentAdapterRuntimeStepRow row in plan.userSteps ?? new List<BattlePrepareComponentAdapterRuntimeStepRow>())
            {
                builder.AppendLine(
                    $"| `{Escape(row.stepId)}` | {Escape(row.action)} | {Escape(row.expected)} |");
            }

            builder.AppendLine();
            builder.AppendLine("## Handtest Checklist");
            builder.AppendLine();
            builder.AppendLine("- V03 BattlePrepare opens through Play and remains touchable.");
            builder.AppendLine("- The mature item tray can still be clicked and dragged with its previous hand feel.");
            builder.AppendLine("- The mature board can still receive hover/drag focus without layout jumps.");
            builder.AppendLine("- Pressing `R` or clicking Rotate Shape changes V0.4 occupied cells only in the devOnly layer.");
            builder.AppendLine("- Valid multi-cell placement feedback appears green; overlap/out-of-grid feedback appears red.");
            builder.AppendLine("- Stopping Play removes the DontSave launcher and leaves no scene/layout changes to save.");
            builder.AppendLine();
            builder.AppendLine("This checklist is intentionally manual. Automated QA cannot certify touch feel.");
            return builder.ToString();
        }

        private static string BuildLeakCheckReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattlePrepareComponentAdapterRuntimePlaytestPlan plan,
            int errors,
            int warnings,
            int leakCount)
        {
            int featureFlagDefaultTrue = BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue);
            int isolationLeaks = CountIsolationLeaks(plan);
            int uiRewriteLeaks = CountUiRewriteLeaks(plan);
            int formalSystemLeaks = CountFormalSystemLeaks(plan);
            int surfaceLeaks = CountSurfaceLeaks(plan);
            string status = errors == 0 && leakCount == 0
                ? "READY_RUNTIME_PLAYTEST"
                : "RUNTIME_PLAYTEST_BLOCKED";

            StringBuilder builder = new();
            builder.AppendLine("# BattlePrepare Component Adapter Runtime Playtest Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattlePrepareComponentAdapterRuntimePlaytestPlan.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{status}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine();
            builder.AppendLine("## Leak Counters");
            builder.AppendLine();
            builder.AppendLine("| Check | Count | Expected |");
            builder.AppendLine("| --- | ---: | ---: |");
            builder.AppendLine($"| `featureFlagDefaultTrue` | {featureFlagDefaultTrue} | 0 |");
            builder.AppendLine($"| `runtimeIsolationLeaks` | {isolationLeaks} | 0 |");
            builder.AppendLine($"| `uiRewriteLeaks` | {uiRewriteLeaks} | 0 |");
            builder.AppendLine($"| `formalSystemLeaks` | {formalSystemLeaks} | 0 |");
            builder.AppendLine($"| `runtimeSurfaceLeaks` | {surfaceLeaks} | 0 |");
            builder.AppendLine($"| `totalLeaks` | {leakCount} | 0 |");
            builder.AppendLine();
            builder.AppendLine("## Formal Scope Confirmation");
            builder.AppendLine();
            builder.AppendLine("- Runtime files are limited to BuildSandbox and Editor/BuildSandbox.");
            builder.AppendLine("- The manual entry opens the formal scene read-only for Play and injects only DontSave runtime objects.");
            builder.AppendLine("- V02 / V03 formal scripts, formal scenes, and hand-tuned RectTransform values are not written.");
            builder.AppendLine("- RunFlow / PageState / FormationState / SaveData / Boss / reward / drop / numeric systems are not connected.");
            builder.AppendLine("- BuildSandbox FeatureFlags remain default false.");

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static int CountLeaks(BattlePrepareComponentAdapterRuntimePlaytestPlan plan)
        {
            return BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue)
                + CountIsolationLeaks(plan)
                + CountUiRewriteLeaks(plan)
                + CountFormalSystemLeaks(plan)
                + CountSurfaceLeaks(plan);
        }

        private static int CountIsolationLeaks(BattlePrepareComponentAdapterRuntimePlaytestPlan plan)
        {
            if (plan == null)
            {
                return 1;
            }

            int leaks = 0;
            if (!plan.devOnly) leaks++;
            if (plan.isEnabled) leaks++;
            if (!plan.manualOnly) leaks++;
            if (!plan.realUnityPlayTouchablePath) leaks++;
            if (plan.staticReportOnly) leaks++;
            if (plan.automationClaimsHandFeelPass) leaks++;
            if (plan.writesFormalScene) leaks++;
            if (plan.writesFormalUi) leaks++;
            if (plan.overwritesHandTunedRectTransform) leaks++;
            if (plan.featureFlagDefaultEnabled) leaks++;
            return leaks;
        }

        private static int CountUiRewriteLeaks(BattlePrepareComponentAdapterRuntimePlaytestPlan plan)
        {
            if (plan == null)
            {
                return 1;
            }

            int leaks = 0;
            if (plan.redrewV04Board) leaks++;
            if (plan.redrewV04ItemTray) leaks++;
            if (plan.rewroteDragFeel) leaks++;
            if (plan.rewrotePullUpAnimation) leaks++;
            if (plan.promotesTemporaryPreviewUi) leaks++;
            return leaks;
        }

        private static int CountFormalSystemLeaks(BattlePrepareComponentAdapterRuntimePlaytestPlan plan)
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

        private static int CountSurfaceLeaks(BattlePrepareComponentAdapterRuntimePlaytestPlan plan)
        {
            return (plan?.runtimeSurfaces ?? new List<BattlePrepareComponentAdapterRuntimeSurfaceRow>())
                .Count(row => row == null || !row.extensionOnly || row.writesFormalUi || row.modifiesFormalScene || row.overwritesRectTransform);
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
