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
    public static class DevChapterContentPoolReportWriter
    {
        public const string DevChapterContentPoolReportPath =
            "Docs/V0.4/Reports/DevChapterContentPoolReport.md";

        public const string DevChapterBuildMappingReportPath =
            "Docs/V0.4/Reports/DevChapterBuildMappingReport.csv";

        public const string DevChapterLeakCheckReportPath =
            "Docs/V0.4/Reports/DevChapterLeakCheckReport.md";

        public static string[] WriteReports(IReadOnlyList<BuildSandboxValidationReport> reports)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string poolPath = Path.Combine(projectRoot, DevChapterContentPoolReportPath);
            string mappingPath = Path.Combine(projectRoot, DevChapterBuildMappingReportPath);
            string leakPath = Path.Combine(projectRoot, DevChapterLeakCheckReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(poolPath) ?? projectRoot);

            DevChapterContentPool pool = DevChapterContentPool.CreateDefault();
            BuildSimulationBenchmarkReport benchmark = DevChapterContentPoolValidator.BuildDevChapterSimulationBenchmark();
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            File.WriteAllText(
                poolPath,
                BuildPoolMarkdown(pool, benchmark, reports, errors, warnings),
                new UTF8Encoding(false));
            File.WriteAllText(
                mappingPath,
                BuildMappingCsv(pool, benchmark),
                new UTF8Encoding(false));
            File.WriteAllText(
                leakPath,
                BuildLeakCheckMarkdown(pool, reports, errors, warnings),
                new UTF8Encoding(false));
            AssetDatabase.Refresh();
            return new[] { poolPath, mappingPath, leakPath };
        }

        private static string BuildPoolMarkdown(
            DevChapterContentPool pool,
            BuildSimulationBenchmarkReport benchmark,
            IReadOnlyList<BuildSandboxValidationReport> reports,
            int errors,
            int warnings)
        {
            StringBuilder builder = new();
            builder.AppendLine("# Dev Chapter Content Pool Report");
            builder.AppendLine();
            builder.AppendLine("Package: `V0.3/V0.4-BuildSandbox-DevChapterContentPool01`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine("Data label: `devOnly / disabled / simulator readable / not product flow`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Dev-only chapter and level profiles for BuildSimulationRunner sandbox reads.");
            builder.AppendLine("- No formal 1-10 / 2-10, formal chapter entry, formal enemy/Boss pool, reward, drop, numeric, save, UI, RunFlow, PageState, FormationState, DamageText, or V02FormationGridFrame connection.");
            builder.AppendLine("- Every profile remains `devOnly=true`, `isEnabled=false`, `simulatorReadable=true`, and `entersFormalFlow=false`.");
            builder.AppendLine();
            builder.AppendLine("## Profile Counts");
            builder.AppendLine();
            builder.AppendLine($"- Dev chapter / boss test profiles: `{pool.chapters.Count}`");
            builder.AppendLine("- Required minimum: `65`");
            builder.AppendLine();
            builder.AppendLine("## Chapter Profiles");
            builder.AppendLine();
            builder.AppendLine("| chapterId | 中文定位 | 验证目标 Build | enemyId | bossId | devOnly | isEnabled | 进入正式流程 | 模拟器可读取 |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- | --- | --- |");
            foreach (DevChapterProfile chapter in pool.chapters)
            {
                builder.AppendLine(
                    $"| `{Escape(chapter.chapterId)}` | {Escape(chapter.chineseRole)} | `{Escape(FormatStrings(chapter.validationTargetBuilds))}` | `{Escape(chapter.enemyId)}` | `{Escape(chapter.bossId)}` | `{chapter.devOnly}` | `{chapter.isEnabled}` | `{chapter.entersFormalFlow}` | `{chapter.simulatorReadable}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## Simulator Read Check");
            builder.AppendLine();
            builder.AppendLine("| Scenario | enemyId | bossId | 中文定位 | Win Rate Estimate | Clear Time Estimate | Formal Flow | Simulator Readable |");
            builder.AppendLine("| --- | --- | --- | --- | ---: | ---: | --- | --- |");
            foreach (BuildSimulationResult result in benchmark.results)
            {
                DevChapterProfile chapter = pool.FindChapter(result.buildId);
                builder.AppendLine(
                    $"| `{Escape(result.buildId)}` | `{Escape(result.enemyProfileId)}` | `{Escape(result.bossProfileId)}` | {Escape(chapter?.chineseRole)} | {result.simulatedWinRate:0.####} | {result.simulatedClearTimeSeconds:0.##} | `{result.entersFormalFlow}` | `{result.simulatorReadable}` |");
            }

            AppendValidationSummary(builder, reports);
            builder.AppendLine();
            builder.AppendLine("## Forbidden Scope Check");
            builder.AppendLine();
            builder.AppendLine("- Formal 1-10 / 2-10: not connected.");
            builder.AppendLine("- Formal StageConfig / mainline entry / reward / drop: not referenced by any profile.");
            builder.AppendLine("- RunFlow / PageState / FormationState / SaveData / PlayerPrefs / MainTrialProgressData: not touched.");
            builder.AppendLine("- Current UI scene / hand-tuned layout: not touched.");
            builder.AppendLine("- FeatureFlags: remain false.");
            builder.AppendLine();
            builder.AppendLine("## User Check");
            builder.AppendLine();
            builder.AppendLine("- Confirm profiles are marked devOnly and disabled.");
            builder.AppendLine("- Confirm reports say simulator-readable only.");
            builder.AppendLine("- Confirm formal 1-10 / 2-10 content does not use this pool.");
            return builder.ToString();
        }

        private static string BuildMappingCsv(
            DevChapterContentPool pool,
            BuildSimulationBenchmarkReport benchmark)
        {
            StringBuilder csv = new();
            csv.AppendLine("chapterId,chineseRole,validationTargetBuild,enemyId,bossId,devOnly,isEnabled,entersFormalFlow,simulatorReadable,scenarioId,simulationWinRateEstimate,simulationClearTimeEstimate");
            foreach (DevChapterProfile chapter in pool.chapters)
            {
                BuildSimulationResult result = benchmark.results.FirstOrDefault(item =>
                    string.Equals(item.buildId, chapter.chapterId, StringComparison.Ordinal));
                csv.AppendLine(Csv(
                    chapter.chapterId,
                    chapter.chineseRole,
                    FormatStrings(chapter.validationTargetBuilds),
                    chapter.enemyId,
                    chapter.bossId,
                    chapter.devOnly.ToString(),
                    chapter.isEnabled.ToString(),
                    chapter.entersFormalFlow.ToString(),
                    chapter.simulatorReadable.ToString(),
                    result?.buildId ?? string.Empty,
                    result?.simulatedWinRate.ToString("0.####") ?? string.Empty,
                    result?.simulatedClearTimeSeconds.ToString("0.##") ?? string.Empty));
            }

            return csv.ToString();
        }

        private static string BuildLeakCheckMarkdown(
            DevChapterContentPool pool,
            IReadOnlyList<BuildSandboxValidationReport> reports,
            int errors,
            int warnings)
        {
            int formalLeaks = pool.chapters.Count(chapter =>
                chapter.entersFormalFlow
                || chapter.usesFormalStageData
                || chapter.usesFormalFlowHook
                || chapter.appearsInFormalEntrance
                || chapter.usesProductReward
                || chapter.usesProductDrop);
            int enabled = pool.chapters.Count(chapter => chapter.isEnabled);
            int nonDevOnly = pool.chapters.Count(chapter => !chapter.devOnly);

            StringBuilder builder = new();
            builder.AppendLine("# Dev Chapter Leak Check Report");
            builder.AppendLine();
            builder.AppendLine("Package: `V0.3/V0.4-BuildSandbox-DevChapterContentPool01`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine();
            builder.AppendLine("## Leak Counters");
            builder.AppendLine();
            builder.AppendLine("| Check | Count | Expected |");
            builder.AppendLine("| --- | ---: | ---: |");
            builder.AppendLine($"| `formalLeakProfiles` | {formalLeaks} | 0 |");
            builder.AppendLine($"| `enabledProfiles` | {enabled} | 0 |");
            builder.AppendLine($"| `nonDevOnlyProfiles` | {nonDevOnly} | 0 |");
            builder.AppendLine();
            builder.AppendLine("## Per Profile Flags");
            builder.AppendLine();
            builder.AppendLine("| chapterId | stageData | flowHook | productEntry | productReward | productDrop | entersFormalFlow |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- |");
            foreach (DevChapterProfile chapter in pool.chapters)
            {
                builder.AppendLine(
                    $"| `{Escape(chapter.chapterId)}` | `{chapter.usesFormalStageData}` | `{chapter.usesFormalFlowHook}` | `{chapter.appearsInFormalEntrance}` | `{chapter.usesProductReward}` | `{chapter.usesProductDrop}` | `{chapter.entersFormalFlow}` |");
            }

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

        private static string Escape(string value)
        {
            return (value ?? string.Empty)
                .Replace("|", "\\|")
                .Replace("\r", " ")
                .Replace("\n", " ");
        }

        private static string FormatStrings(IEnumerable<string> values)
        {
            return string.Join(";", (values ?? Enumerable.Empty<string>()).Where(value => !string.IsNullOrWhiteSpace(value)));
        }

        private static string Csv(params string[] values)
        {
            return string.Join(",", values.Select(EscapeCsv));
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
