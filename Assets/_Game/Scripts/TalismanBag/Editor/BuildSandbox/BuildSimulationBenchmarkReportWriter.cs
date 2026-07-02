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
    public static class BuildSimulationBenchmarkReportWriter
    {
        public const string BuildSimulationReportPath =
            "Docs/V0.4/Reports/BuildSimulationReport.md";

        public const string BuildComparisonReportPath =
            "Docs/V0.4/Reports/BuildComparison.csv";

        public const string SynergyActivationReportPath =
            "Docs/V0.4/Reports/SynergyActivationReport.csv";

        public const string AffixImpactReportPath =
            "Docs/V0.4/Reports/AffixImpactReport.csv";

        public static string[] WriteReports(IReadOnlyList<BuildSandboxValidationReport> reports)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string simulationPath = Path.Combine(projectRoot, BuildSimulationReportPath);
            string comparisonPath = Path.Combine(projectRoot, BuildComparisonReportPath);
            string synergyPath = Path.Combine(projectRoot, SynergyActivationReportPath);
            string affixPath = Path.Combine(projectRoot, AffixImpactReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(simulationPath) ?? projectRoot);

            BuildSimulationBenchmarkReport benchmark = BuildSimulationBenchmarkValidator.BuildSampleBenchmark();
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            File.WriteAllText(
                simulationPath,
                BuildSimulationMarkdown(benchmark, reports, errors, warnings),
                new UTF8Encoding(false));
            File.WriteAllText(
                comparisonPath,
                BuildComparisonCsv(benchmark.comparisons),
                new UTF8Encoding(false));
            File.WriteAllText(
                synergyPath,
                BuildSynergyActivationCsv(benchmark),
                new UTF8Encoding(false));
            File.WriteAllText(
                affixPath,
                BuildAffixImpactCsv(benchmark),
                new UTF8Encoding(false));
            AssetDatabase.Refresh();
            return new[] { simulationPath, comparisonPath, synergyPath, affixPath };
        }

        private static string BuildSimulationMarkdown(
            BuildSimulationBenchmarkReport benchmark,
            IReadOnlyList<BuildSandboxValidationReport> reports,
            int errors,
            int warnings)
        {
            StringBuilder builder = new();
            builder.AppendLine("# Build Simulation Benchmark Report");
            builder.AppendLine();
            builder.AppendLine("Package: `V0.3/V0.4-BuildSandbox-BuildSimulationBenchmark01`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine("Data label: `sandbox estimate / devOnly / disabled`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Pure BuildSandbox batch simulation and benchmark reporting.");
            builder.AppendLine("- Uses `BuildSandboxLayoutSnapshot`, `SynergyEvaluator`, Affix preview, `CombatModifierBundle` preview, and `EffectEventBundle` preview.");
            builder.AppendLine("- Enemy and Boss inputs are devOnly placeholder tags, not formal enemy/Boss pools.");
            builder.AppendLine("- No formal combat, UI, RunFlow, PageState, FormationState, save, reward, boss, drop, or numeric config connection.");
            builder.AppendLine("- Every scenario and result remains `devOnly=true`, `isEnabled=false`, and `sandboxEstimate=true`.");
            builder.AppendLine();
            builder.AppendLine("## Scenario Inputs");
            builder.AppendLine();
            builder.AppendLine("| Build | Enemy Type | Boss Mechanic | Item Rarity | Affixes | Energy | Placement | devOnly | isEnabled |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- | --- | --- |");
            foreach (BuildSimulationScenario scenario in benchmark.scenarios)
            {
                builder.AppendLine(
                    $"| `{Escape(scenario.buildId)}` | `{Escape(scenario.enemyType)}` | `{Escape(scenario.bossMechanic)}` | `{Escape(scenario.itemRarity)}` | `{Escape(FormatStrings(scenario.affixCombination))}` | `{Escape(scenario.energyCondition)}` | `{Escape(scenario.placementRelation)}` | `{scenario.devOnly}` | `{scenario.isEnabled}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## Simulation Results");
            builder.AppendLine();
            builder.AppendLine("| Build | Active Synergies | Synergy Level | Clear Time (sandbox estimate sec) | Win Rate (sandbox estimate) | Remaining HP (sandbox estimate) | Total Damage (sandbox estimate) | Shield Break Efficiency | Energy Coverage | Triggered Events | Failure Reason | Numeric Anomaly |");
            builder.AppendLine("| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |");
            foreach (BuildSimulationResult result in benchmark.results)
            {
                builder.AppendLine(
                    $"| `{Escape(result.buildId)}` | `{Escape(FormatStrings(result.activeSynergies))}` | {result.synergyLevel} | {result.simulatedClearTimeSeconds:0.##} | {result.simulatedWinRate:0.####} | {result.simulatedRemainingHpPercent:0.####} | {result.simulatedTotalDamage:0.##} | {result.shieldBreakEfficiency:0.####} | {result.energyCoverage:0.####} | {result.triggeredEventCount} | `{Escape(result.failureReason)}` | `{result.numericAnomaly}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## Comparisons");
            builder.AppendLine();
            builder.AppendLine("| Comparison | Baseline | Challenger | Dimension | Clear Time Delta | Win Rate Delta | Remaining HP Delta | Total Damage Delta | Shield Break Delta | Event Delta |");
            builder.AppendLine("| --- | --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |");
            foreach (BuildComparisonResult comparison in benchmark.comparisons)
            {
                builder.AppendLine(
                    $"| `{Escape(comparison.comparisonName)}` | `{Escape(comparison.baselineBuildId)}` | `{Escape(comparison.challengerBuildId)}` | `{Escape(comparison.comparedDimension)}` | {comparison.clearTimeDeltaSeconds:0.####} | {comparison.winRateDelta:0.####} | {comparison.remainingHpDelta:0.####} | {comparison.totalDamageDelta:0.####} | {comparison.shieldBreakEfficiencyDelta:0.####} | {comparison.triggeredEventDelta} |");
            }

            builder.AppendLine();
            builder.AppendLine("## Preview Usage");
            builder.AppendLine();
            builder.AppendLine("| Build | Modifier Preview Used | Event Preview Used | Affects Formal Combat |");
            builder.AppendLine("| --- | --- | --- | --- |");
            foreach (BuildSimulationResult result in benchmark.results)
            {
                builder.AppendLine(
                    $"| `{Escape(result.buildId)}` | `{result.combatModifierPreviewUsed}` | `{result.effectEventPreviewUsed}` | `{result.affectsFormalCombat}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## FeatureFlag Defaults");
            builder.AppendLine();
            builder.AppendLine("| Flag | Default | Scope |");
            builder.AppendLine("| --- | --- | --- |");
            foreach (BuildSandboxFeatureFlagDefinition flag in BuildSandboxFeatureFlags.All)
            {
                builder.AppendLine($"| `{flag.Key}` | `{flag.DefaultValue}` | {Escape(flag.Scope)} |");
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
            builder.AppendLine("- Formal combat connection: not touched; all benchmark values are `sandbox estimate`.");
            builder.AppendLine("- Formal player save / PlayerPrefs / MainTrialProgressData: not read.");
            builder.AppendLine("- Formal enemy / Boss / reward / drop / numeric configs: not referenced.");
            builder.AppendLine("- Current UI scene / RectTransform / hand-tuned layout: not touched.");
            builder.AppendLine("- RunFlow / PageState / FormationState / V02RunFlowController / V02FormationGridFrame / DamageText: not touched.");
            builder.AppendLine("- FeatureFlag default true: none when this report is PASS.");
            builder.AppendLine();
            builder.AppendLine("## User Check");
            builder.AppendLine();
            builder.AppendLine("- Confirm the reports are marked sandbox / devOnly.");
            builder.AppendLine("- Confirm FeatureFlags remain false.");
            builder.AppendLine("- Confirm current UI hand tune and formal battle are not connected to this benchmark.");
            return builder.ToString();
        }

        private static string BuildComparisonCsv(IReadOnlyList<BuildComparisonResult> comparisons)
        {
            StringBuilder csv = new();
            csv.AppendLine("comparisonName,baselineBuild,challengerBuild,clearTimeDeltaSeconds,winRateDelta,remainingHpDelta,totalDamageDelta,shieldBreakEfficiencyDelta,eventCountDelta,comparedDimension,devOnly,isEnabled,sandboxEstimate");
            foreach (BuildComparisonResult comparison in comparisons)
            {
                csv.AppendLine(Csv(
                    comparison.comparisonName,
                    comparison.baselineBuildId,
                    comparison.challengerBuildId,
                    comparison.clearTimeDeltaSeconds.ToString("0.####"),
                    comparison.winRateDelta.ToString("0.####"),
                    comparison.remainingHpDelta.ToString("0.####"),
                    comparison.totalDamageDelta.ToString("0.####"),
                    comparison.shieldBreakEfficiencyDelta.ToString("0.####"),
                    comparison.triggeredEventDelta.ToString(),
                    comparison.comparedDimension,
                    comparison.devOnly.ToString(),
                    comparison.isEnabled.ToString(),
                    comparison.sandboxEstimate.ToString()));
            }

            return csv.ToString();
        }

        private static string BuildSynergyActivationCsv(BuildSimulationBenchmarkReport benchmark)
        {
            StringBuilder csv = new();
            csv.AppendLine("testBuild,synergyId,activeThresholds,synergyLevel,sourceItems,placementSatisfied,energySatisfied,simulationWinRateEstimate,simulationClearTimeEstimate,devOnly,isEnabled,sandboxEstimate");
            foreach (BuildSimulationScenario scenario in benchmark.scenarios)
            {
                BuildSimulationResult result = benchmark.results.FirstOrDefault(item =>
                    string.Equals(item.buildId, scenario.buildId, StringComparison.Ordinal));
                foreach (ActiveSynergyResult synergy in scenario.buildEvaluation?.activeSynergies ?? Enumerable.Empty<ActiveSynergyResult>())
                {
                    csv.AppendLine(Csv(
                        scenario.buildId,
                        synergy.synergyId,
                        FormatStrings(synergy.activeThresholds),
                        result?.synergyLevel.ToString() ?? "0",
                        FormatStrings(synergy.sourceItems),
                        synergy.placementSatisfied.ToString(),
                        synergy.energySatisfied.ToString(),
                        result?.simulatedWinRate.ToString("0.####") ?? "0",
                        result?.simulatedClearTimeSeconds.ToString("0.##") ?? "0",
                        scenario.devOnly.ToString(),
                        scenario.isEnabled.ToString(),
                        "sandbox estimate"));
                }
            }

            return csv.ToString();
        }

        private static string BuildAffixImpactCsv(BuildSimulationBenchmarkReport benchmark)
        {
            StringBuilder csv = new();
            csv.AppendLine("testBuild,itemId,rarity,selectedAffixes,totalPreviewPower,simulationDamageEstimate,simulationWinRateEstimate,sourceTags,devOnly,isEnabled,sandboxEstimate");
            foreach (BuildSimulationScenario scenario in benchmark.scenarios)
            {
                BuildSimulationResult result = benchmark.results.FirstOrDefault(item =>
                    string.Equals(item.buildId, scenario.buildId, StringComparison.Ordinal));
                foreach (AffixRarityItemResult item in scenario.affixEvaluation?.itemResults ?? Enumerable.Empty<AffixRarityItemResult>())
                {
                    csv.AppendLine(Csv(
                        scenario.buildId,
                        item.itemId,
                        item.rarityId,
                        FormatStrings(item.selectedAffixes),
                        item.totalPreviewPower.ToString(),
                        result?.simulatedTotalDamage.ToString("0.##") ?? "0",
                        result?.simulatedWinRate.ToString("0.####") ?? "0",
                        FormatStrings(item.sourceTags),
                        scenario.devOnly.ToString(),
                        scenario.isEnabled.ToString(),
                        "sandbox estimate"));
                }
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
