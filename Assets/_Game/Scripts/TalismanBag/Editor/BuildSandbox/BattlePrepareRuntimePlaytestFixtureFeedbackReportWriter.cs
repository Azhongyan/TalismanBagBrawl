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
    public static class BattlePrepareRuntimePlaytestFixtureFeedbackReportWriter
    {
        public const string MainReportPath =
            "Docs/V0.4/Reports/BattlePrepareRuntimePlaytestFixtureFeedbackReport.md";

        public const string LeakCheckReportPath =
            "Docs/V0.4/Reports/BattlePrepareRuntimePlaytestFixtureFeedbackLeakCheckReport.md";

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattlePrepareComponentAdapterRuntimePlaytestPlan plan = null)
        {
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();
            BattlePrepareComponentAdapterRuntimePlaytestPlan safePlan =
                plan ?? BattlePrepareRuntimePlaytestFixtureFeedbackValidator.BuildDefaultPlan();
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string mainPath = Path.Combine(projectRoot, MainReportPath);
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
                leakPath,
                BuildLeakCheckReport(safeReports, safePlan, errors, warnings, leakCount),
                new UTF8Encoding(false));

            AssetDatabase.Refresh();
            return new[] { mainPath, leakPath };
        }

        private static string BuildMainReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattlePrepareComponentAdapterRuntimePlaytestPlan plan,
            int errors,
            int warnings,
            int leakCount)
        {
            string status = errors == 0 && leakCount == 0
                ? "READY_FIXTURE_FEEDBACK_PLAYTEST"
                : "FIXTURE_FEEDBACK_BLOCKED";

            StringBuilder builder = new();
            builder.AppendLine("# BattlePrepare Runtime Playtest Fixture Feedback Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattlePrepareComponentAdapterRuntimePlaytestPlan.FixtureFeedbackPackageName}`");
            builder.AppendLine($"Base Runtime Package: `{BattlePrepareComponentAdapterRuntimePlaytestPlan.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{status}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine($"Leak Count: `{leakCount}`");
            builder.AppendLine();
            builder.AppendLine("## Runtime Entry");
            builder.AppendLine();
            builder.AppendLine($"- Manual Menu Path: `{Escape(plan.manualMenuPath)}`");
            builder.AppendLine($"- Fixture QA Menu Path: `{Escape(BattlePrepareComponentAdapterRuntimePlaytestPlan.FixtureFeedbackQaMenuPath)}`");
            builder.AppendLine($"- Target Scene: `{Escape(plan.targetScenePath)}`");
            builder.AppendLine($"- createsRuntimeFixtureProvider: `{plan.createsRuntimeFixtureProvider}`");
            builder.AppendLine($"- injectsFixtureItemsIntoMatureTray: `{plan.injectsFixtureItemsIntoMatureTray}`");
            builder.AppendLine($"- fixtureDefinitionsDontSave: `{plan.fixtureDefinitionsDontSave}`");
            builder.AppendLine();
            builder.AppendLine("## Fixture Items");
            builder.AppendLine();
            builder.AppendLine("| Item Id | Display | Shape | Cells | Rotation | Tray Visible | devOnly | isEnabled | canDrop | Formal Config | Save Data | Manual Probe |");
            builder.AppendLine("| --- | --- | --- | ---: | --- | --- | --- | --- | --- | --- | --- | --- |");
            foreach (BattlePrepareRuntimeFixtureItemRow row in plan.fixtureItems ?? new List<BattlePrepareRuntimeFixtureItemRow>())
            {
                builder.AppendLine(
                    $"| `{Escape(row.itemId)}` | {Escape(row.displayName)} | `{Escape(row.shapeId)}` | {row.cellCount} | `{row.rotationAllowed}` | `{row.trayVisible}` | `{row.devOnly}` | `{row.isEnabled}` | `{row.canDrop}` | `{row.writesFormalConfig}` | `{row.writesSaveData}` | {Escape(row.manualProbe)} |");
            }

            builder.AppendLine();
            builder.AppendLine("## Feedback Targets");
            builder.AppendLine();
            builder.AppendLine("| Feedback | Display | Trigger | Expected Chinese Feedback | Required |");
            builder.AppendLine("| --- | --- | --- | --- | --- |");
            foreach (BattlePrepareRuntimeFixtureFeedbackRow row in plan.fixtureFeedback ?? new List<BattlePrepareRuntimeFixtureFeedbackRow>())
            {
                builder.AppendLine(
                    $"| `{Escape(row.feedbackId)}` | {Escape(row.chineseDisplayName)} | {Escape(row.trigger)} | {Escape(row.expectedChineseFeedback)} | `{row.required}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## Manual Handtest Checklist");
            builder.AppendLine();
            builder.AppendLine("- Mature V02 / V03 BattlePrepare page and pull-up hand feel still open through the Manual Only menu.");
            builder.AppendLine("- The mature tray shows Single1, Vertical2, Corner3, and Square4 devOnly fixture items.");
            builder.AppendLine("- Selecting a fixture binds the matching V0.4 shape in the extension panel.");
            builder.AppendLine("- Vertical2 and Corner3 rotate through `R` or the rotate button.");
            builder.AppendLine("- Hovering or dragging over the mature board shows green valid occupancy and red invalid occupancy.");
            builder.AppendLine("- Board edge hover reports out-of-grid feedback; occupied-cell hover reports overlap feedback.");
            builder.AppendLine("- Stopping Play leaves no formal scene, pool, drop, save, or RectTransform changes.");

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildLeakCheckReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattlePrepareComponentAdapterRuntimePlaytestPlan plan,
            int errors,
            int warnings,
            int leakCount)
        {
            string status = errors == 0 && leakCount == 0
                ? "READY_FIXTURE_FEEDBACK_PLAYTEST"
                : "FIXTURE_FEEDBACK_BLOCKED";

            StringBuilder builder = new();
            builder.AppendLine("# BattlePrepare Runtime Playtest Fixture Feedback Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattlePrepareComponentAdapterRuntimePlaytestPlan.FixtureFeedbackPackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{status}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine();
            builder.AppendLine("## Leak Counters");
            builder.AppendLine();
            builder.AppendLine("| Check | Count | Expected |");
            builder.AppendLine("| --- | ---: | ---: |");
            builder.AppendLine($"| `fixtureProviderLeaks` | {CountProviderLeaks(plan)} | 0 |");
            builder.AppendLine($"| `fixtureItemLeaks` | {CountFixtureItemLeaks(plan)} | 0 |");
            builder.AppendLine($"| `formalSystemLeaks` | {CountFormalSystemLeaks(plan)} | 0 |");
            builder.AppendLine($"| `uiRewriteLeaks` | {CountUiRewriteLeaks(plan)} | 0 |");
            builder.AppendLine($"| `featureFlagDefaultTrue` | {BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue)} | 0 |");
            builder.AppendLine($"| `totalLeaks` | {leakCount} | 0 |");
            builder.AppendLine();
            builder.AppendLine("## Scope Confirmation");
            builder.AppendLine();
            builder.AppendLine("- Fixture definitions are runtime-created ScriptableObjects with DontSave hide flags.");
            builder.AppendLine("- Fixture item views are runtime-created DraggableTalismanItemView objects inside the mature tray.");
            builder.AppendLine("- Fixture rows declare `devOnly=true`, `isEnabled=false`, `canDrop=false`, and no formal config/save writes.");
            builder.AppendLine("- The package does not redraw V04 board/tray/drag/pull-up animation and does not touch RunFlow, PageState, FormationState, SaveData, Boss, reward, drop, or numeric systems.");

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static int CountLeaks(BattlePrepareComponentAdapterRuntimePlaytestPlan plan)
        {
            return CountProviderLeaks(plan)
                + CountFixtureItemLeaks(plan)
                + CountFormalSystemLeaks(plan)
                + CountUiRewriteLeaks(plan)
                + BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue);
        }

        private static int CountProviderLeaks(BattlePrepareComponentAdapterRuntimePlaytestPlan plan)
        {
            if (plan == null)
            {
                return 1;
            }

            int leaks = 0;
            if (!plan.devOnly) leaks++;
            if (plan.isEnabled) leaks++;
            if (!plan.createsRuntimeFixtureProvider) leaks++;
            if (!plan.fixtureProviderDevOnly) leaks++;
            if (!plan.fixtureDefinitionsDontSave) leaks++;
            if (!plan.injectsFixtureItemsIntoMatureTray) leaks++;
            if (!plan.fixtureItemsVisibleInTray) leaks++;
            if (plan.writesFixtureToFormalPool) leaks++;
            if (plan.writesFixtureToSaveData) leaks++;
            return leaks;
        }

        private static int CountFixtureItemLeaks(BattlePrepareComponentAdapterRuntimePlaytestPlan plan)
        {
            return (plan?.fixtureItems ?? new List<BattlePrepareRuntimeFixtureItemRow>())
                .Count(row => row == null || !row.devOnly || row.isEnabled || row.canDrop || row.writesFormalConfig || row.writesSaveData || !row.trayVisible);
        }

        private static int CountFormalSystemLeaks(BattlePrepareComponentAdapterRuntimePlaytestPlan plan)
        {
            if (plan == null)
            {
                return 1;
            }

            int leaks = 0;
            if (plan.writesFormalScene) leaks++;
            if (plan.writesFormalUi) leaks++;
            if (plan.overwritesHandTunedRectTransform) leaks++;
            if (plan.touchesRunFlow) leaks++;
            if (plan.touchesPageState) leaks++;
            if (plan.touchesFormationState) leaks++;
            if (plan.touchesSaveData) leaks++;
            if (plan.touchesBossRewardDropNumeric) leaks++;
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
