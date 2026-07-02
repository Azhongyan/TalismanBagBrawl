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
    public static class EnemyBossValidationPoolReportWriter
    {
        public const string EnemyBossValidationPoolReportPath =
            "Docs/V0.4/Reports/EnemyBossValidationPoolReport.md";

        public const string EnemyBuildMappingReportPath =
            "Docs/V0.4/Reports/EnemyBuildMappingReport.md";

        public const string BossBuildValidationReportPath =
            "Docs/V0.4/Reports/BossBuildValidationReport.csv";

        public static string[] WriteReports(IReadOnlyList<BuildSandboxValidationReport> reports)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string poolPath = Path.Combine(projectRoot, EnemyBossValidationPoolReportPath);
            string mappingPath = Path.Combine(projectRoot, EnemyBuildMappingReportPath);
            string bossPath = Path.Combine(projectRoot, BossBuildValidationReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(poolPath) ?? projectRoot);

            EnemyBossValidationPool pool = EnemyBossValidationPool.CreateDefault();
            BuildSimulationBenchmarkReport benchmark = EnemyBossValidationPoolValidator.BuildPoolSimulationBenchmark();
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            File.WriteAllText(
                poolPath,
                BuildPoolMarkdown(pool, benchmark, reports, errors, warnings),
                new UTF8Encoding(false));
            File.WriteAllText(
                mappingPath,
                BuildEnemyMappingMarkdown(pool, benchmark),
                new UTF8Encoding(false));
            File.WriteAllText(
                bossPath,
                BuildBossValidationCsv(pool, benchmark),
                new UTF8Encoding(false));
            AssetDatabase.Refresh();
            return new[] { poolPath, mappingPath, bossPath };
        }

        private static string BuildPoolMarkdown(
            EnemyBossValidationPool pool,
            BuildSimulationBenchmarkReport benchmark,
            IReadOnlyList<BuildSandboxValidationReport> reports,
            int errors,
            int warnings)
        {
            StringBuilder builder = new();
            builder.AppendLine("# Enemy/Boss Validation Pool Report");
            builder.AppendLine();
            builder.AppendLine("Package: `V0.3/V0.4-BuildSandbox-EnemyBossValidationPool01`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine("Data label: `devOnly / disabled / simulator readable / not formal flow`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Dev-only enemy and Boss validation profiles for BuildSimulationBenchmark sandbox reads.");
            builder.AppendLine("- No formal 1-10 / 2-10, formal enemy pool, formal Boss config, reward, drop, numeric, save, UI, RunFlow, PageState, FormationState, DamageText, or V02FormationGridFrame connection.");
            builder.AppendLine("- Every profile remains `devOnly=true`, `isEnabled=false`, `simulatorReadable=true`, and `entersFormalFlow=false`.");
            builder.AppendLine();
            builder.AppendLine("## Enemy Profiles");
            builder.AppendLine();
            builder.AppendLine("| enemyId | 中文定位 | 验证目标 Build | 验证标签 | 推荐羁绊 | devOnly | isEnabled | 进入正式流程 | 模拟器可读取 |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- | --- | --- |");
            foreach (BuildSandboxEnemyProfile enemy in pool.enemies)
            {
                builder.AppendLine(
                    $"| `{Escape(enemy.enemyId)}` | {Escape(enemy.chineseRole)} | `{Escape(FormatStrings(enemy.validationTargetBuilds))}` | `{Escape(FormatStrings(enemy.validationTags))}` | {Escape(FormatStrings(enemy.recommendedSynergies))} | `{enemy.devOnly}` | `{enemy.isEnabled}` | `{enemy.entersFormalFlow}` | `{enemy.simulatorReadable}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## Boss Profiles");
            builder.AppendLine();
            builder.AppendLine("| bossId | 中文定位 | 验证目标 Build | 验证标签 | 推荐羁绊 | devOnly | isEnabled | 进入正式流程 | 模拟器可读取 |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- | --- | --- |");
            foreach (BuildSandboxBossProfile boss in pool.bosses)
            {
                builder.AppendLine(
                    $"| `{Escape(boss.bossId)}` | {Escape(boss.chineseRole)} | `{Escape(FormatStrings(boss.validationTargetBuilds))}` | `{Escape(FormatStrings(boss.validationTags))}` | {Escape(FormatStrings(boss.recommendedSynergies))} | `{boss.devOnly}` | `{boss.isEnabled}` | `{boss.entersFormalFlow}` | `{boss.simulatorReadable}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## Simulator Read Check");
            builder.AppendLine();
            builder.AppendLine("| Scenario | enemyId | bossId | 中文定位 | Win Rate Estimate | Clear Time Estimate | Formal Flow | Simulator Readable |");
            builder.AppendLine("| --- | --- | --- | --- | ---: | ---: | --- | --- |");
            foreach (BuildSimulationResult result in benchmark.results)
            {
                string role = string.IsNullOrWhiteSpace(result.enemyChineseRole)
                    ? result.bossChineseRole
                    : result.enemyChineseRole;
                builder.AppendLine(
                    $"| `{Escape(result.buildId)}` | `{Escape(result.enemyProfileId)}` | `{Escape(result.bossProfileId)}` | {Escape(role)} | {result.simulatedWinRate:0.####} | {result.simulatedClearTimeSeconds:0.##} | `{result.entersFormalFlow}` | `{result.simulatorReadable}` |");
            }

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

            builder.AppendLine();
            builder.AppendLine("## Forbidden Scope Check");
            builder.AppendLine();
            builder.AppendLine("- Formal 1-10 / 2-10: not connected.");
            builder.AppendLine("- Formal enemy / Boss / reward / drop / numeric configs: not referenced.");
            builder.AppendLine("- RunFlow / PageState / FormationState / SaveData / PlayerPrefs / MainTrialProgressData: not touched.");
            builder.AppendLine("- Current UI scene / RectTransform / hand-tuned layout: not touched.");
            builder.AppendLine("- FeatureFlags: remain false.");
            builder.AppendLine();
            builder.AppendLine("## User Check");
            builder.AppendLine();
            builder.AppendLine("- Confirm profiles are marked devOnly and disabled.");
            builder.AppendLine("- Confirm reports say simulator-readable only.");
            builder.AppendLine("- Confirm no formal 1-10 / 2-10 content uses this pool.");
            return builder.ToString();
        }

        private static string BuildEnemyMappingMarkdown(
            EnemyBossValidationPool pool,
            BuildSimulationBenchmarkReport benchmark)
        {
            StringBuilder builder = new();
            builder.AppendLine("# Enemy Build Mapping Report");
            builder.AppendLine();
            builder.AppendLine("Package: `V0.3/V0.4-BuildSandbox-EnemyBossValidationPool01`");
            builder.AppendLine("Data label: `devOnly / disabled / not formal flow`");
            builder.AppendLine();
            builder.AppendLine("| enemyId | 中文定位 | enemyType | 验证目标 Build | 验证标签 | 推荐羁绊 | 对应模拟场景 | 模拟器可读取 | 进入正式流程 |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- | --- | --- |");
            foreach (BuildSandboxEnemyProfile enemy in pool.enemies)
            {
                BuildSimulationResult result = benchmark.results.FirstOrDefault(item =>
                    string.Equals(item.enemyProfileId, enemy.enemyId, StringComparison.Ordinal));
                builder.AppendLine(
                    $"| `{Escape(enemy.enemyId)}` | {Escape(enemy.chineseRole)} | `{Escape(enemy.enemyType)}` | `{Escape(FormatStrings(enemy.validationTargetBuilds))}` | `{Escape(FormatStrings(enemy.validationTags))}` | {Escape(FormatStrings(enemy.recommendedSynergies))} | `{Escape(result?.buildId)}` | `{enemy.simulatorReadable}` | `{enemy.entersFormalFlow}` |");
            }

            return builder.ToString();
        }

        private static string BuildBossValidationCsv(
            EnemyBossValidationPool pool,
            BuildSimulationBenchmarkReport benchmark)
        {
            StringBuilder csv = new();
            csv.AppendLine("bossId,chineseRole,bossMechanic,validationTargetBuild,validationTags,recommendedSynergies,devOnly,isEnabled,entersFormalFlow,simulatorReadable,scenarioId,simulationWinRateEstimate,simulationClearTimeEstimate");
            foreach (BuildSandboxBossProfile boss in pool.bosses)
            {
                BuildSimulationResult result = benchmark.results.FirstOrDefault(item =>
                    string.Equals(item.bossProfileId, boss.bossId, StringComparison.Ordinal));
                csv.AppendLine(Csv(
                    boss.bossId,
                    boss.chineseRole,
                    boss.bossMechanic,
                    FormatStrings(boss.validationTargetBuilds),
                    FormatStrings(boss.validationTags),
                    FormatStrings(boss.recommendedSynergies),
                    boss.devOnly.ToString(),
                    boss.isEnabled.ToString(),
                    boss.entersFormalFlow.ToString(),
                    boss.simulatorReadable.ToString(),
                    result?.buildId ?? string.Empty,
                    result?.simulatedWinRate.ToString("0.####") ?? string.Empty,
                    result?.simulatedClearTimeSeconds.ToString("0.##") ?? string.Empty));
            }

            return csv.ToString();
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
