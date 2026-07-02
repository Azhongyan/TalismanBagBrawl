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
    public static class BuildSandboxConfigPanelReportWriter
    {
        public const string BuildSandboxConfigPanelReportPath =
            "Docs/V0.4/Reports/BuildSandboxConfigPanelReport.md";

        public const string BuildSandboxConfigPanelTabReportPath =
            "Docs/V0.4/Reports/BuildSandboxConfigPanelTabReport.csv";

        public const string BuildSandboxConfigPanelLeakCheckReportPath =
            "Docs/V0.4/Reports/BuildSandboxConfigPanelLeakCheckReport.md";

        public static string[] WriteReports(IReadOnlyList<BuildSandboxValidationReport> reports)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string mainPath = Path.Combine(projectRoot, BuildSandboxConfigPanelReportPath);
            string tabPath = Path.Combine(projectRoot, BuildSandboxConfigPanelTabReportPath);
            string leakPath = Path.Combine(projectRoot, BuildSandboxConfigPanelLeakCheckReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(mainPath) ?? projectRoot);

            BuildProblemSeedDataset dataset = BuildProblemSeedDataset.CreateDefault();
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            File.WriteAllText(mainPath, BuildMainReport(dataset, reports, errors, warnings), new UTF8Encoding(false));
            File.WriteAllText(tabPath, BuildTabCsv(dataset), new UTF8Encoding(false));
            File.WriteAllText(leakPath, BuildLeakCheckReport(dataset, reports, errors, warnings), new UTF8Encoding(false));
            AssetDatabase.Refresh();
            return new[] { mainPath, tabPath, leakPath };
        }

        private static string BuildMainReport(
            BuildProblemSeedDataset dataset,
            IReadOnlyList<BuildSandboxValidationReport> reports,
            int errors,
            int warnings)
        {
            StringBuilder builder = new();
            builder.AppendLine("# BuildSandbox Config Panel Report");
            builder.AppendLine();
            builder.AppendLine("Package: `V0.4-BuildSandboxConfigPanel01`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Editor-only V0.4 BuildSandbox devOnly config panel.");
            builder.AppendLine("- Read-only static seed data source: `BuildProblemSeedDataset.CreateDefault()`.");
            builder.AppendLine("- Menu path: `Tools / Talisman Bag / V0.4 / BuildSandbox / Data / [Manual Only] BuildSandbox Config Panel 01`.");
            builder.AppendLine("- No V0.2 Stage Config Panel source or V0.2 / V0.3 product data source is edited.");
            builder.AppendLine("- No formal 1-10 / 2-10, UI scene, save, reward, drop, Boss, or numeric connection.");
            builder.AppendLine();
            builder.AppendLine("## Panel Summary");
            builder.AppendLine();
            builder.AppendLine($"- Window title: `{BuildSandboxConfigPanelWindow.WindowTitle}`");
            builder.AppendLine($"- Menu path exists: `{(errors == 0 || HasMenuInfo(reports))}`");
            builder.AppendLine($"- Tabs: `{Enum.GetNames(typeof(BuildSandboxConfigPanelTab)).Length}`");
            builder.AppendLine("- Editable product data: `False`");
            builder.AppendLine("- Save behavior: `Report export only`");
            builder.AppendLine();
            builder.AppendLine("## Data Counts");
            builder.AppendLine();
            builder.AppendLine($"- MapRule: `{dataset.mapRules.Count}`");
            builder.AppendLine($"- EnemyProblem: `{dataset.enemyProblems.Count}`");
            builder.AppendLine($"- BossProblem: `{dataset.bossProblems.Count}`");
            builder.AppendLine($"- WeaknessWindow: `{dataset.weaknessWindows.Count}`");
            builder.AppendLine($"- DropBias: `{dataset.dropBiases.Count}`");
            builder.AppendLine($"- FailureHint: `{dataset.failureHints.Count}`");
            builder.AppendLine();
            builder.AppendLine("## Isolation");
            builder.AppendLine();
            builder.AppendLine($"- Dataset devOnly: `{dataset.devOnly}`");
            builder.AppendLine($"- Dataset isEnabled: `{dataset.isEnabled}`");
            builder.AppendLine($"- FeatureFlag default true count: `{BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue)}`");
            builder.AppendLine($"- Seed leak count: `{CountSeedLeaks(dataset)}`");
            builder.AppendLine($"- DropBias product leak count: `{CountDropBiasLeaks(dataset)}`");
            builder.AppendLine();
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildTabCsv(BuildProblemSeedDataset dataset)
        {
            StringBuilder csv = new();
            csv.AppendLine("tab,label,dataSource,count,editable,reportOnly,formalDataSource");
            csv.AppendLine(Csv("Overview", "Overview / 总览", "BuildProblemSeedDataset", "1", "False", "True", "False"));
            csv.AppendLine(Csv("MapRule", "MapRule / 地图规则", "BuildProblemSeedDataset.mapRules", dataset.mapRules.Count.ToString(), "False", "True", "False"));
            csv.AppendLine(Csv("EnemyProblem", "EnemyProblem / 敌人题目", "BuildProblemSeedDataset.enemyProblems", dataset.enemyProblems.Count.ToString(), "False", "True", "False"));
            csv.AppendLine(Csv("BossProblem", "BossProblem / Boss 多钥匙锁", "BuildProblemSeedDataset.bossProblems", dataset.bossProblems.Count.ToString(), "False", "True", "False"));
            csv.AppendLine(Csv("WeaknessWindow", "WeaknessWindow / 弱点窗口", "BuildProblemSeedDataset.weaknessWindows", dataset.weaknessWindows.Count.ToString(), "False", "True", "False"));
            csv.AppendLine(Csv("DropBias", "DropBias / 定向倾向", "BuildProblemSeedDataset.dropBiases", dataset.dropBiases.Count.ToString(), "False", "True", "False"));
            csv.AppendLine(Csv("BuildReadiness", "BuildReadiness / 破题准备度", "Boss required attributes + key requirements", CountReadinessAttributes(dataset).ToString(), "False", "True", "False"));
            csv.AppendLine(Csv("DevChapter", "DevChapter / devOnly 章节预留", "reserved placeholder", "0", "False", "True", "False"));
            csv.AppendLine(Csv("Simulation", "Simulation / 模拟结果预留", "reserved placeholder", "0", "False", "True", "False"));
            csv.AppendLine(Csv("Validation", "Validation / 校验", "BuildSandboxConfigPanelValidator.BuildValidationReports", "7", "False", "True", "False"));
            return csv.ToString();
        }

        private static string BuildLeakCheckReport(
            BuildProblemSeedDataset dataset,
            IReadOnlyList<BuildSandboxValidationReport> reports,
            int errors,
            int warnings)
        {
            int featureFlagTrue = BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue);
            int seedLeaks = CountSeedLeaks(dataset);
            int dropBiasLeaks = CountDropBiasLeaks(dataset);
            int panelSourceErrors = reports
                .SelectMany(report => report.Issues)
                .Count(issue => issue.Code == "CONFIG_PANEL_FORBIDDEN_FORMAL_TOKEN");

            StringBuilder builder = new();
            builder.AppendLine("# BuildSandbox Config Panel Leak Check Report");
            builder.AppendLine();
            builder.AppendLine("Package: `V0.4-BuildSandboxConfigPanel01`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine();
            builder.AppendLine("## Leak Counters");
            builder.AppendLine();
            builder.AppendLine("| Check | Count | Expected |");
            builder.AppendLine("| --- | ---: | ---: |");
            builder.AppendLine($"| `featureFlagDefaultTrue` | {featureFlagTrue} | 0 |");
            builder.AppendLine($"| `seedIsolationLeaks` | {seedLeaks} | 0 |");
            builder.AppendLine($"| `dropBiasProductLeaks` | {dropBiasLeaks} | 0 |");
            builder.AppendLine($"| `panelSourceForbiddenFormalTokenHits` | {panelSourceErrors} | 0 |");
            builder.AppendLine();
            builder.AppendLine("## Confirmation");
            builder.AppendLine();
            builder.AppendLine("- Panel source is under `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/**`.");
            builder.AppendLine("- Data source is static V0.4 BuildSandbox seed data.");
            builder.AppendLine("- The panel does not edit `DataCatalogEditorWindow` or `StageConfigPanelEditorUi`.");
            builder.AppendLine("- The panel does not write V02 / V03 formal data, formal 1-10 / 2-10, UI scenes, save, reward, drop, Boss, or numeric data.");
            builder.AppendLine("- DropBias remains report-only and does not connect to product drop tables.");
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

        private static bool HasMenuInfo(IEnumerable<BuildSandboxValidationReport> reports)
        {
            return reports
                .SelectMany(report => report.Issues)
                .Any(issue => issue.Code == "CONFIG_PANEL_MENU_PATH_PRESENT");
        }

        private static int CountReadinessAttributes(BuildProblemSeedDataset dataset)
        {
            HashSet<string> attributes = new(StringComparer.Ordinal);
            foreach (BossProblemSeed boss in dataset.bossProblems)
            {
                foreach (string attribute in boss.requiredProblemAttributes ?? Enumerable.Empty<string>())
                {
                    if (!string.IsNullOrWhiteSpace(attribute))
                    {
                        attributes.Add(attribute);
                    }
                }

                foreach (BossProblemKeySeed key in boss.keyRequirements ?? Enumerable.Empty<BossProblemKeySeed>())
                {
                    if (!string.IsNullOrWhiteSpace(key.problemAttribute))
                    {
                        attributes.Add(key.problemAttribute);
                    }
                }
            }

            return attributes.Count;
        }

        private static int CountSeedLeaks(BuildProblemSeedDataset dataset)
        {
            int leaks = dataset.devOnly && !dataset.isEnabled ? 0 : 1;
            leaks += dataset.mapRules.Count(rule => !rule.devOnly || rule.isEnabled || rule.entersFormalFlow || rule.usesFormalStageData);
            leaks += dataset.enemyProblems.Count(problem => !problem.devOnly || problem.isEnabled || problem.entersFormalFlow || problem.referencesProductEnemyList || problem.affectsFormalCombat);
            leaks += dataset.bossProblems.Count(boss => !boss.devOnly || boss.isEnabled || boss.entersFormalFlow || boss.referencesProductBossList || boss.affectsFormalCombat);
            leaks += dataset.bossProblems.SelectMany(boss => boss.keyRequirements).Count(key => !key.devOnly || key.isEnabled || key.gatesProductBoss);
            leaks += dataset.weaknessWindows.Count(window => !window.devOnly || window.isEnabled || window.entersFormalFlow || window.affectsFormalCombat);
            leaks += dataset.dropBiases.Count(drop => !drop.devOnly || drop.isEnabled || drop.entersFormalFlow || drop.referencesProductDrops || drop.grantsProductItems);
            leaks += dataset.failureHints.Count(hint => !hint.devOnly || hint.isEnabled || hint.entersFormalFlow || hint.writesRuntimeUi);
            return leaks;
        }

        private static int CountDropBiasLeaks(BuildProblemSeedDataset dataset)
        {
            return dataset.dropBiases.Count(drop =>
                !drop.reportsOnly
                || drop.entersFormalFlow
                || drop.referencesProductDrops
                || drop.grantsProductItems
                || !drop.devOnly
                || drop.isEnabled);
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
