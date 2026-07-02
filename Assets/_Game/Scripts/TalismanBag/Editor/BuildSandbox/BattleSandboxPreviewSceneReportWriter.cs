#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TalismanBag.BuildSandbox;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BattleSandboxPreviewSceneReportWriter
    {
        public const string MainReportPath = "Docs/V0.4/Reports/BattleSandboxPreviewSceneReport.md";
        public const string HierarchyReportPath = "Docs/V0.4/Reports/BattleSandboxPreviewSceneHierarchyReport.csv";
        public const string LeakCheckReportPath = "Docs/V0.4/Reports/BattleSandboxPreviewSceneLeakCheckReport.md";

        public static string[] WriteReports(IReadOnlyList<BuildSandboxValidationReport> reports)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string mainPath = Path.Combine(projectRoot, MainReportPath);
            string hierarchyPath = Path.Combine(projectRoot, HierarchyReportPath);
            string leakPath = Path.Combine(projectRoot, LeakCheckReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(mainPath) ?? projectRoot);

            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);
            BuildSandboxPreviewSceneMarker marker = LoadPreviewSceneForReport();
            IReadOnlyList<HierarchyRow> hierarchy = CollectHierarchyRows();

            File.WriteAllText(mainPath, BuildMainReport(reports, errors, warnings, marker, hierarchy), new UTF8Encoding(false));
            File.WriteAllText(hierarchyPath, BuildHierarchyCsv(hierarchy), new UTF8Encoding(false));
            File.WriteAllText(leakPath, BuildLeakCheckReport(reports, errors, warnings, marker), new UTF8Encoding(false));
            AssetDatabase.Refresh();
            return new[] { mainPath, hierarchyPath, leakPath };
        }

        private static string BuildMainReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            int errors,
            int warnings,
            BuildSandboxPreviewSceneMarker marker,
            IReadOnlyList<HierarchyRow> hierarchy)
        {
            StringBuilder builder = new();
            builder.AppendLine("# BattleSandbox Preview Scene Report");
            builder.AppendLine();
            builder.AppendLine("Package: `V0.4-BattleSandboxPreviewScene01`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Independent V0.4 BuildSandbox preview scene skeleton only.");
            builder.AppendLine("- Creates graybox UI slots for later PreviewContext, data panels, problem selectors, and interaction packages.");
            builder.AppendLine("- Does not bind formal battle, formal mainline, save, reward, drop, Boss, or numeric systems.");
            builder.AppendLine("- Does not add the preview scene to Build Settings.");
            builder.AppendLine();
            builder.AppendLine("## Scene");
            builder.AppendLine();
            builder.AppendLine($"- Scene path: `{BuildSandboxPreviewSceneMarker.ScenePath}`");
            builder.AppendLine($"- Scene exists: `{File.Exists(Path.Combine(ProjectRoot, BuildSandboxPreviewSceneMarker.ScenePath))}`");
            builder.AppendLine($"- Marker devOnly: `{marker != null && marker.DevOnly}`");
            builder.AppendLine($"- Marker isEnabled: `{(marker != null && marker.IsEnabled)}`");
            builder.AppendLine($"- Marker connectedToFormalFlow: `{(marker != null && marker.ConnectedToFormalFlow)}`");
            builder.AppendLine($"- Hierarchy rows: `{hierarchy.Count}`");
            builder.AppendLine();
            builder.AppendLine("## Required Slots");
            builder.AppendLine();
            builder.AppendLine("| Required Path | Present |");
            builder.AppendLine("| --- | --- |");
            HashSet<string> presentPaths = new(hierarchy.Select(row => row.Path), StringComparer.Ordinal);
            foreach (string required in BattleSandboxPreviewSceneVerifier.GetRequiredObjectPaths())
            {
                builder.AppendLine($"| `{Escape(required)}` | `{presentPaths.Contains(required)}` |");
            }

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildHierarchyCsv(IReadOnlyList<HierarchyRow> hierarchy)
        {
            StringBuilder csv = new();
            csv.AppendLine("path,activeSelf,componentTypes");
            foreach (HierarchyRow row in hierarchy)
            {
                csv.AppendLine(Csv(row.Path, row.ActiveSelf.ToString(), string.Join(";", row.ComponentTypes)));
            }

            return csv.ToString();
        }

        private static string BuildLeakCheckReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            int errors,
            int warnings,
            BuildSandboxPreviewSceneMarker marker)
        {
            bool buildSettingsContainsPreview = EditorBuildSettings.scenes.Any(scene =>
                scene != null && string.Equals(scene.path, BuildSandboxPreviewSceneMarker.ScenePath, StringComparison.Ordinal));
            int featureFlagTrue = BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue);
            int markerLeaks = marker == null ? 1 : CountMarkerLeaks(marker);

            StringBuilder builder = new();
            builder.AppendLine("# BattleSandbox Preview Scene Leak Check Report");
            builder.AppendLine();
            builder.AppendLine("Package: `V0.4-BattleSandboxPreviewScene01`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine();
            builder.AppendLine("## Leak Counters");
            builder.AppendLine();
            builder.AppendLine("| Check | Count | Expected |");
            builder.AppendLine("| --- | ---: | ---: |");
            builder.AppendLine($"| `featureFlagDefaultTrue` | {featureFlagTrue} | 0 |");
            builder.AppendLine($"| `previewMarkerIsolationLeaks` | {markerLeaks} | 0 |");
            builder.AppendLine($"| `previewSceneInBuildSettings` | {(buildSettingsContainsPreview ? 1 : 0)} | 0 |");
            builder.AppendLine();
            builder.AppendLine("## Formal Scope Confirmation");
            builder.AppendLine();
            builder.AppendLine("- Product V02/V03 scenes: not written by this package.");
            builder.AppendLine("- ProjectSettings/EditorBuildSettings.asset: not written by this package.");
            builder.AppendLine("- V02RunFlowController, V02FormationGridFrame, DamageText, RunFlow, PageState, FormationState: not modified by this package.");
            builder.AppendLine("- SaveData, PlayerPrefs, MainTrialProgressData: not modified by this package.");
            builder.AppendLine("- Reward, drop, Boss, enemy, upgrade, and numeric configs: not modified by this package.");
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

        private static BuildSandboxPreviewSceneMarker LoadPreviewSceneForReport()
        {
            string scenePath = Path.Combine(ProjectRoot, BuildSandboxPreviewSceneMarker.ScenePath);
            if (!File.Exists(scenePath))
            {
                return null;
            }

            EditorSceneManager.OpenScene(BuildSandboxPreviewSceneMarker.ScenePath, OpenSceneMode.Single);
            return UnityEngine.Object.FindObjectOfType<BuildSandboxPreviewSceneMarker>(true);
        }

        private static IReadOnlyList<HierarchyRow> CollectHierarchyRows()
        {
            Scene scene = SceneManager.GetActiveScene();
            List<HierarchyRow> rows = new();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                CollectRows(root.transform, rows);
            }

            return rows;
        }

        private static void CollectRows(Transform target, ICollection<HierarchyRow> rows)
        {
            rows.Add(new HierarchyRow(
                BuildPath(target),
                target.gameObject.activeSelf,
                target.GetComponents<Component>().Select(component => component.GetType().Name).ToArray()));
            foreach (Transform child in target)
            {
                CollectRows(child, rows);
            }
        }

        private static int CountMarkerLeaks(BuildSandboxPreviewSceneMarker marker)
        {
            int leaks = 0;
            if (!marker.DevOnly)
            {
                leaks++;
            }

            if (marker.IsEnabled)
            {
                leaks++;
            }

            if (marker.ConnectedToFormalFlow)
            {
                leaks++;
            }

            return leaks;
        }

        private static string ProjectRoot => Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;

        private static string BuildPath(Transform target)
        {
            string path = target.name;
            Transform parent = target.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
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

        private readonly struct HierarchyRow
        {
            public HierarchyRow(string path, bool activeSelf, string[] componentTypes)
            {
                Path = path;
                ActiveSelf = activeSelf;
                ComponentTypes = componentTypes;
            }

            public string Path { get; }
            public bool ActiveSelf { get; }
            public string[] ComponentTypes { get; }
        }
    }
}
#endif
