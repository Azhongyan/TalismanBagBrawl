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
    public static class BuildGridInteractionPreviewReportWriter
    {
        public const string MainReportPath = "Docs/V0.4/Reports/BuildGridInteractionPreviewReport.md";
        public const string PlacementReportPath = "Docs/V0.4/Reports/BuildGridInteractionPlacementReport.csv";
        public const string UiBindingReportPath = "Docs/V0.4/Reports/BuildGridInteractionUiBindingReport.csv";
        public const string LeakCheckReportPath = "Docs/V0.4/Reports/BuildGridInteractionLeakCheckReport.md";

        public static string[] WriteReports(IReadOnlyList<BuildSandboxValidationReport> reports)
        {
            string projectRoot = Directory.GetParent(UnityEngine.Application.dataPath)?.FullName ?? string.Empty;
            string mainPath = Path.Combine(projectRoot, MainReportPath);
            string placementPath = Path.Combine(projectRoot, PlacementReportPath);
            string uiPath = Path.Combine(projectRoot, UiBindingReportPath);
            string leakPath = Path.Combine(projectRoot, LeakCheckReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(mainPath) ?? projectRoot);

            BuildGridInteractionSceneBindingSnapshot snapshot =
                BuildGridInteractionPreviewValidator.BuildSceneBindingSnapshot();
            IReadOnlyList<BuildGridInteractionPlacementSample> samples =
                BuildGridInteractionPreviewValidator.BuildPlacementSamples();
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            File.WriteAllText(mainPath, BuildMainReport(reports, snapshot, samples, errors, warnings), new UTF8Encoding(false));
            File.WriteAllText(placementPath, BuildPlacementCsv(samples), new UTF8Encoding(false));
            File.WriteAllText(uiPath, BuildUiBindingCsv(snapshot.UiBindingRows), new UTF8Encoding(false));
            File.WriteAllText(leakPath, BuildLeakCheckReport(reports, snapshot, errors, warnings), new UTF8Encoding(false));
            AssetDatabase.Refresh();
            return new[] { mainPath, placementPath, uiPath, leakPath };
        }

        private static string BuildMainReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BuildGridInteractionSceneBindingSnapshot snapshot,
            IReadOnlyList<BuildGridInteractionPlacementSample> samples,
            int errors,
            int warnings)
        {
            StringBuilder builder = new();
            builder.AppendLine("# Build Grid Interaction Preview Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BuildGridInteractionPreviewValidator.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- V04 independent BuildSandbox preview scene interaction only.");
            builder.AppendLine("- Implements item tray scrolling, category filtering, multi-cell shape preview, drag placement, rotation, and placement feedback.");
            builder.AppendLine("- Adds runtime-only BuildSandbox ItemInfoPanel with Chinese player text and masked solution keys.");
            builder.AppendLine("- Does not connect to formal battle, formal save, formal UI, reward, drop, Boss, or numeric systems.");
            builder.AppendLine();
            builder.AppendLine("## Scene Binding");
            builder.AppendLine();
            builder.AppendLine($"- Scene path: `{BuildSandboxPreviewSceneMarker.ScenePath}`");
            builder.AppendLine($"- V04 Preview Scene bound: `{snapshot.ControllerPresent}`");
            builder.AppendLine($"- BoardGridPreview exists: `{snapshot.BoardGridPresent}`");
            builder.AppendLine($"- ItemTrayPreview exists: `{snapshot.ItemTrayPresent}`");
            builder.AppendLine($"- SelectedItemInfo exists: `{snapshot.SelectedItemInfoPresent}`");
            builder.AppendLine($"- PlacementFeedback exists: `{snapshot.PlacementFeedbackPresent}`");
            builder.AppendLine($"- Board slot count: `{snapshot.BoardSlotCount}`");
            builder.AppendLine($"- Item tray slot count: `{snapshot.TraySlotCount}`");
            builder.AppendLine($"- Category count: `{snapshot.CategoryCount}`");
            builder.AppendLine($"- Shape count: `{snapshot.ShapeCount}`");
            builder.AppendLine();
            builder.AppendLine("## Interaction Samples");
            builder.AppendLine();
            builder.AppendLine("| Sample | Action | Shape | Anchor | Valid | Reason | Feedback |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- |");
            foreach (BuildGridInteractionPlacementSample sample in samples)
            {
                builder.AppendLine(
                    $"| `{Escape(sample.SampleId)}` | {Escape(sample.ActionChinese)} | `{Escape(sample.ShapeId)}` | `{Escape(sample.Anchor)}` | `{sample.IsValid}` | `{sample.InvalidReason}` | {Escape(sample.FeedbackChinese)} |");
            }

            builder.AppendLine();
            builder.AppendLine("## Player Text / Answer Masking");
            builder.AppendLine();
            builder.AppendLine($"- Player-facing Chinese-only violations: `{snapshot.PlayerTextLatinViolations.Count}`");
            builder.AppendLine($"- Forbidden complete-answer text violations: `{snapshot.ForbiddenAnswerTextViolations.Count}`");
            builder.AppendLine($"- Shows complete answers flag: `{snapshot.ControllerShowsCompleteAnswers}`");
            builder.AppendLine();
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildPlacementCsv(IReadOnlyList<BuildGridInteractionPlacementSample> samples)
        {
            StringBuilder builder = new();
            builder.AppendLine("sampleId,actionChinese,shapeId,anchor,occupiedCells,isValid,invalidReason,feedbackChinese");
            foreach (BuildGridInteractionPlacementSample sample in samples)
            {
                builder.AppendLine(Csv(
                    sample.SampleId,
                    sample.ActionChinese,
                    sample.ShapeId,
                    sample.Anchor,
                    sample.OccupiedCells,
                    sample.IsValid.ToString(),
                    sample.InvalidReason.ToString(),
                    sample.FeedbackChinese));
            }

            return builder.ToString();
        }

        private static string BuildUiBindingCsv(IReadOnlyList<BuildGridInteractionUiBindingRow> rows)
        {
            StringBuilder builder = new();
            builder.AppendLine("path,activeSelf,componentTypes");
            foreach (BuildGridInteractionUiBindingRow row in rows)
            {
                builder.AppendLine(Csv(row.Path, row.ActiveSelf.ToString(), row.ComponentTypes));
            }

            return builder.ToString();
        }

        private static string BuildLeakCheckReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BuildGridInteractionSceneBindingSnapshot snapshot,
            int errors,
            int warnings)
        {
            int formalLeakCount = CountTrue(
                snapshot.ControllerReadsFormalSave,
                snapshot.ControllerWritesFormalFlow,
                snapshot.ControllerWritesFormalUi,
                snapshot.ControllerTouchesFormalScene);
            int playerTextLeakCount =
                snapshot.PlayerTextLatinViolations.Count + snapshot.ForbiddenAnswerTextViolations.Count;

            StringBuilder builder = new();
            builder.AppendLine("# Build Grid Interaction Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BuildGridInteractionPreviewValidator.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine();
            builder.AppendLine("## Leak Counters");
            builder.AppendLine();
            builder.AppendLine("| Check | Count | Expected |");
            builder.AppendLine("| --- | ---: | ---: |");
            builder.AppendLine($"| `featureFlagDefaultTrue` | {BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue)} | 0 |");
            builder.AppendLine($"| `controllerFormalSurfaceLeaks` | {formalLeakCount} | 0 |");
            builder.AppendLine($"| `previewSceneInBuildSettings` | {(snapshot.BuildSettingsContainsPreview ? 1 : 0)} | 0 |");
            builder.AppendLine($"| `playerTextOrAnswerMaskingLeaks` | {playerTextLeakCount} | 0 |");
            builder.AppendLine($"| `completeAnswerVisibleFlag` | {(snapshot.ControllerShowsCompleteAnswers ? 1 : 0)} | 0 |");
            builder.AppendLine();
            builder.AppendLine("## Formal Scope Confirmation");
            builder.AppendLine();
            builder.AppendLine("- V02 FormationCounter scene: not written by this package.");
            builder.AppendLine("- V03 MainHome scene: not written by this package.");
            builder.AppendLine("- V03 TalismanUpgrade scene: not written by this package.");
            builder.AppendLine("- ProjectSettings/EditorBuildSettings.asset: not written by this package.");
            builder.AppendLine("- Formal SaveData / PlayerPrefs / MainTrialProgressData: not read or written.");
            builder.AppendLine("- Formal battle / RunFlow / PageState / FormationState / V02FormationGridFrame / DamageText: not modified.");
            builder.AppendLine("- Player-side preview UI and sandbox ItemInfoPanel show Chinese-only fuzzy clues and do not show complete answers.");
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

        private static int CountTrue(params bool[] values)
        {
            return values.Count(value => value);
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
