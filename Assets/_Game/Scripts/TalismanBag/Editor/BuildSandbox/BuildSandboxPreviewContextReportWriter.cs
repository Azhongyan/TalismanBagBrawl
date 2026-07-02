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
    public static class BuildSandboxPreviewContextReportWriter
    {
        public const string MainReportPath = "Docs/V0.4/Reports/BuildSandboxPreviewContextReport.md";
        public const string ViewModelReportPath = "Docs/V0.4/Reports/BuildSandboxPreviewViewModelReport.csv";
        public const string ReadinessReportPath = "Docs/V0.4/Reports/BuildSandboxPreviewReadinessReport.csv";
        public const string LeakCheckReportPath = "Docs/V0.4/Reports/BuildSandboxPreviewContextLeakCheckReport.md";

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BuildSandboxPreviewContext context = null)
        {
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();
            BuildSandboxPreviewContext safeContext =
                context ?? BuildSandboxPreviewContextValidator.BuildDefaultContext();
            string projectRoot = Directory.GetParent(UnityEngine.Application.dataPath)?.FullName ?? string.Empty;
            string mainPath = Path.Combine(projectRoot, MainReportPath);
            string viewModelPath = Path.Combine(projectRoot, ViewModelReportPath);
            string readinessPath = Path.Combine(projectRoot, ReadinessReportPath);
            string leakPath = Path.Combine(projectRoot, LeakCheckReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(mainPath) ?? projectRoot);

            int errors = safeReports.Sum(report => report.ErrorCount);
            int warnings = safeReports.Sum(report => report.WarningCount);
            File.WriteAllText(
                mainPath,
                BuildMainReport(safeReports, safeContext, errors, warnings),
                new UTF8Encoding(false));
            File.WriteAllText(
                viewModelPath,
                BuildViewModelCsv(safeContext),
                new UTF8Encoding(false));
            File.WriteAllText(
                readinessPath,
                BuildReadinessCsv(safeContext),
                new UTF8Encoding(false));
            File.WriteAllText(
                leakPath,
                BuildLeakCheckReport(safeReports, safeContext, errors, warnings),
                new UTF8Encoding(false));

            AssetDatabase.Refresh();
            return new[] { mainPath, viewModelPath, readinessPath, leakPath };
        }

        private static string BuildMainReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BuildSandboxPreviewContext context,
            int errors,
            int warnings)
        {
            BuildSandboxPreviewViewModel viewModel = context?.viewModel ?? new BuildSandboxPreviewViewModel();
            BuildSummaryViewModel summary = viewModel.buildSummary ?? new BuildSummaryViewModel();
            StringBuilder builder = new();
            builder.AppendLine("# BuildSandbox Preview Context Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BuildSandboxPreviewContextValidator.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Builds a devOnly, disabled, read-only PreviewContext for later UI panel work.");
            builder.AppendLine("- Aggregates Phase 1 BuildSandbox engine outputs and Phase 2 problem seed data.");
            builder.AppendLine("- Does not create UI, write scenes, enter formal combat, read formal persistence, or write formal data.");
            builder.AppendLine();
            builder.AppendLine("## Context");
            builder.AppendLine();
            builder.AppendLine($"- previewBuildId: `{Escape(context?.previewBuildId)}`");
            builder.AppendLine($"- devOnly: `{context?.devOnly}`");
            builder.AppendLine($"- isEnabled: `{context?.isEnabled}`");
            builder.AppendLine($"- readsFormalSaveData: `{context?.readsFormalSaveData}`");
            builder.AppendLine($"- writesFormalFlow: `{context?.writesFormalFlow}`");
            builder.AppendLine($"- writesFormalData: `{context?.writesFormalData}`");
            builder.AppendLine($"- touchesFormalScene: `{context?.touchesFormalScene}`");
            builder.AppendLine($"- featureFlagsAllDisabled: `{summary.featureFlagsAllDisabled}`");
            builder.AppendLine();
            builder.AppendLine("## Output Sections");
            builder.AppendLine();
            builder.AppendLine("| Section | Primary Count | Status |");
            builder.AppendLine("| --- | ---: | --- |");
            builder.AppendLine($"| Build Summary | {summary.placedItemCount} placed / {summary.occupiedCellCount} cells | `{Pass(summary.placedItemCount > 0 && summary.occupiedCellCount > 0)}` |");
            builder.AppendLine($"| Synergy | {viewModel.synergy.activeSynergyCount} active / {viewModel.synergy.activeThresholdCount} thresholds | `{Pass(viewModel.synergy.activeSynergyCount > 0)}` |");
            builder.AppendLine($"| Shape Occupancy | {viewModel.shapeOccupancy.validPlacementCount} valid samples / {viewModel.shapeOccupancy.invalidPlacementCount} invalid QA samples | `{Pass(viewModel.shapeOccupancy.placementLegal)}` |");
            builder.AppendLine($"| Affix Modifier | {viewModel.affixModifier.modifierCount} modifiers / {viewModel.affixModifier.eventCount} events | `{Pass(viewModel.affixModifier.modifierCount > 0 && viewModel.affixModifier.eventCount > 0)}` |");
            builder.AppendLine($"| Problem Selector | {viewModel.problemSelector.mapRuleCount + viewModel.problemSelector.enemyProblemCount + viewModel.problemSelector.bossProblemCount} options | `{Pass(viewModel.problemSelector.bossProblemCount >= BuildProblemSeedDataValidator.RequiredBossProblemCount)}` |");
            builder.AppendLine($"| Problem Readiness | {viewModel.problemReadiness.bossReadinessCount} boss rows / {viewModel.problemReadiness.totalKeyCount} keys | `{Pass(viewModel.problemReadiness.bossReadinessCount >= BuildProblemSeedDataValidator.RequiredBossProblemCount)}` |");
            builder.AppendLine($"| Simulation Result | {viewModel.simulationResult.resultCount} results / {viewModel.simulationResult.comparisonCount} comparisons | `{Pass(viewModel.simulationResult.resultCount > 0)}` |");
            builder.AppendLine();
            builder.AppendLine("## Phase Inputs");
            builder.AppendLine();
            builder.AppendLine($"- Map rules: `{summary.mapRuleCount}`");
            builder.AppendLine($"- Enemy problems: `{summary.enemyProblemCount}`");
            builder.AppendLine($"- Boss problems: `{summary.bossProblemCount}`");
            builder.AppendLine($"- Active synergies: `{summary.activeSynergyCount}`");
            builder.AppendLine($"- Selected affixes: `{summary.selectedAffixCount}`");
            builder.AppendLine($"- Combat modifiers: `{summary.modifierCount}`");
            builder.AppendLine($"- Effect events: `{summary.eventCount}`");
            builder.AppendLine($"- Capability tokens: `{summary.buildTags.Count}`");

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildViewModelCsv(BuildSandboxPreviewContext context)
        {
            BuildSandboxPreviewViewModel viewModel = context?.viewModel ?? new BuildSandboxPreviewViewModel();
            BuildSummaryViewModel summary = viewModel.buildSummary ?? new BuildSummaryViewModel();
            StringBuilder csv = new();
            csv.AppendLine("section,key,value,status,devOnly,isEnabled,formalScope,summary");
            AppendCsv(csv, "buildSummary", "placedItemCount", summary.placedItemCount, summary.placedItemCount > 0, summary.devOnly, summary.isEnabled, IsFormalClear(context), $"{summary.occupiedCellCount} occupied cells");
            AppendCsv(csv, "buildSummary", "problemSeedCounts", $"{summary.mapRuleCount}/{summary.enemyProblemCount}/{summary.bossProblemCount}", summary.bossProblemCount >= BuildProblemSeedDataValidator.RequiredBossProblemCount, summary.devOnly, summary.isEnabled, IsFormalClear(context), "map/enemy/boss");
            AppendCsv(csv, "synergy", "activeSynergies", viewModel.synergy.activeSynergyCount, viewModel.synergy.activeSynergyCount > 0, true, false, true, Join(viewModel.synergy.rows.Select(row => row.synergyId)));
            AppendCsv(csv, "synergy", "activeThresholds", viewModel.synergy.activeThresholdCount, viewModel.synergy.activeThresholdCount > 0, true, false, true, Join(summary.activeThresholds));
            AppendCsv(csv, "shapeOccupancy", "placementSamples", $"{viewModel.shapeOccupancy.validPlacementCount}/{viewModel.shapeOccupancy.invalidPlacementCount}", viewModel.shapeOccupancy.placementLegal, true, false, true, "valid/invalid QA samples");
            AppendCsv(csv, "affixModifier", "affixes", viewModel.affixModifier.selectedAffixCount, viewModel.affixModifier.selectedAffixCount > 0, viewModel.affixModifier.modifierPreviewDevOnly && viewModel.affixModifier.eventPreviewDevOnly, viewModel.affixModifier.modifierPreviewEnabled || viewModel.affixModifier.eventPreviewEnabled, !viewModel.affixModifier.affectsFormalCombat, Join(viewModel.affixModifier.affixIds));
            AppendCsv(csv, "affixModifier", "modifierEvents", $"{viewModel.affixModifier.modifierCount}/{viewModel.affixModifier.eventCount}", viewModel.affixModifier.modifierCount > 0 && viewModel.affixModifier.eventCount > 0, viewModel.affixModifier.modifierPreviewDevOnly && viewModel.affixModifier.eventPreviewDevOnly, viewModel.affixModifier.modifierPreviewEnabled || viewModel.affixModifier.eventPreviewEnabled, !viewModel.affixModifier.affectsFormalCombat, "modifiers/events");
            AppendCsv(csv, "problemSelector", "options", $"{viewModel.problemSelector.mapRuleCount}/{viewModel.problemSelector.enemyProblemCount}/{viewModel.problemSelector.bossProblemCount}", viewModel.problemSelector.bossProblemCount >= BuildProblemSeedDataValidator.RequiredBossProblemCount, true, false, true, "map/enemy/boss selectors");
            AppendCsv(csv, "problemReadiness", "bossRows", viewModel.problemReadiness.bossReadinessCount, viewModel.problemReadiness.bossReadinessCount >= BuildProblemSeedDataValidator.RequiredBossProblemCount, true, false, true, $"{viewModel.problemReadiness.readyBossCount} ready");
            AppendCsv(csv, "problemReadiness", "keys", $"{viewModel.problemReadiness.satisfiedKeyCount}/{viewModel.problemReadiness.totalKeyCount}", viewModel.problemReadiness.totalKeyCount > 0, true, false, true, "satisfied/total");
            AppendCsv(csv, "simulationResult", "results", viewModel.simulationResult.resultCount, viewModel.simulationResult.resultCount > 0, true, false, CountSimulationLeaks(viewModel.simulationResult) == 0, $"best={viewModel.simulationResult.bestBuildId}");
            AppendCsv(csv, "simulationResult", "averages", $"{viewModel.simulationResult.averageWinRate}/{viewModel.simulationResult.averageClearTimeSeconds}", viewModel.simulationResult.resultCount > 0, true, false, true, "winRate/clearTimeSeconds");
            return csv.ToString();
        }

        private static string BuildReadinessCsv(BuildSandboxPreviewContext context)
        {
            ProblemReadinessViewModel readiness =
                context?.viewModel?.problemReadiness ?? new ProblemReadinessViewModel();
            StringBuilder csv = new();
            csv.AppendLine("bossProblemId,bossName,ready,minimumKeysRequired,keyCount,satisfiedKeyCount,keyId,keyCategory,requirementId,problemAttribute,requiredScore,observedScore,satisfied,failureReasons,recommendedActions,dropBiasIds,weaknessWindowIds");
            foreach (BossReadinessViewModel boss in readiness.bossRows ?? new List<BossReadinessViewModel>())
            {
                IReadOnlyList<BossReadinessKeyViewModel> keys =
                    boss.keys == null || boss.keys.Count == 0
                        ? new[] { new BossReadinessKeyViewModel() }
                        : boss.keys;
                foreach (BossReadinessKeyViewModel key in keys)
                {
                    csv.AppendLine(Csv(
                        boss.bossProblemId,
                        boss.displayName,
                        boss.ready.ToString(),
                        boss.minimumKeysRequired.ToString(),
                        boss.keyCount.ToString(),
                        boss.satisfiedKeyCount.ToString(),
                        key.keyId,
                        key.keyCategory,
                        key.requirementId,
                        key.problemAttribute,
                        key.requiredScore.ToString(),
                        key.observedScore.ToString(),
                        key.satisfied.ToString(),
                        Join(boss.failureReasons),
                        Join(boss.recommendedActions),
                        Join(boss.dropBiasIds),
                        Join(boss.weaknessWindowIds)));
                }
            }

            return csv.ToString();
        }

        private static string BuildLeakCheckReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BuildSandboxPreviewContext context,
            int errors,
            int warnings)
        {
            BuildSandboxPreviewViewModel viewModel = context?.viewModel ?? new BuildSandboxPreviewViewModel();
            int featureFlagDefaultTrue = BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue);
            int contextLeaks = CountContextLeaks(context);
            int selectorLeaks = CountSelectorLeaks(viewModel.problemSelector);
            int modifierEventLeaks = CountModifierEventLeaks(viewModel.affixModifier);
            int simulationLeaks = CountSimulationLeaks(viewModel.simulationResult);
            int seedLeaks = CountSeedLeaks(context?.problemSeedDataset);
            int totalLeaks = featureFlagDefaultTrue
                + contextLeaks
                + selectorLeaks
                + modifierEventLeaks
                + simulationLeaks
                + seedLeaks;

            StringBuilder builder = new();
            builder.AppendLine("# BuildSandbox Preview Context Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BuildSandboxPreviewContextValidator.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 && totalLeaks == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine();
            builder.AppendLine("## Leak Counters");
            builder.AppendLine();
            builder.AppendLine("| Check | Count | Expected |");
            builder.AppendLine("| --- | ---: | ---: |");
            builder.AppendLine($"| `featureFlagDefaultTrue` | {featureFlagDefaultTrue} | 0 |");
            builder.AppendLine($"| `contextIsolationLeaks` | {contextLeaks} | 0 |");
            builder.AppendLine($"| `selectorOptionLeaks` | {selectorLeaks} | 0 |");
            builder.AppendLine($"| `modifierEventFormalLeaks` | {modifierEventLeaks} | 0 |");
            builder.AppendLine($"| `simulationRowLeaks` | {simulationLeaks} | 0 |");
            builder.AppendLine($"| `phase2SeedLeaks` | {seedLeaks} | 0 |");
            builder.AppendLine();
            builder.AppendLine("## Formal Scope Confirmation");
            builder.AppendLine();
            builder.AppendLine("- UI panel: not created by this package.");
            builder.AppendLine("- Scene_TalismanBag_V04_BattleSandboxPreview: not written by this package.");
            builder.AppendLine("- Product V02/V03 scenes: not modified by this package.");
            builder.AppendLine("- Formal battle, Boss, reward, drop, upgrade, and numeric configs: not connected by this package.");
            builder.AppendLine("- SaveData, PlayerPrefs, MainTrialProgressData: not read or written by this package.");
            builder.AppendLine("- PreviewContext output is devOnly, disabled, report/read-model oriented data.");

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

        private static void AppendCsv(
            StringBuilder csv,
            string section,
            string key,
            object value,
            bool status,
            bool devOnly,
            bool isEnabled,
            bool formalScope,
            string summary)
        {
            csv.AppendLine(Csv(
                section,
                key,
                value?.ToString() ?? string.Empty,
                Pass(status),
                devOnly.ToString(),
                isEnabled.ToString(),
                Pass(formalScope),
                summary));
        }

        private static int CountContextLeaks(BuildSandboxPreviewContext context)
        {
            if (context == null)
            {
                return 1;
            }

            int leaks = 0;
            if (!context.devOnly)
            {
                leaks++;
            }

            if (context.isEnabled)
            {
                leaks++;
            }

            if (context.readsFormalSaveData)
            {
                leaks++;
            }

            if (context.writesFormalFlow)
            {
                leaks++;
            }

            if (context.writesFormalData)
            {
                leaks++;
            }

            if (context.touchesFormalScene)
            {
                leaks++;
            }

            return leaks;
        }

        private static int CountSelectorLeaks(ProblemSelectorViewModel selector)
        {
            if (selector == null)
            {
                return 1;
            }

            return CountSelectorOptionLeaks(selector.mapRuleOptions)
                + CountSelectorOptionLeaks(selector.enemyProblemOptions)
                + CountSelectorOptionLeaks(selector.bossProblemOptions);
        }

        private static int CountSelectorOptionLeaks(IEnumerable<ProblemSelectorOption> options)
        {
            return (options ?? Enumerable.Empty<ProblemSelectorOption>())
                .Count(option => option == null || !option.devOnly || option.isEnabled);
        }

        private static int CountModifierEventLeaks(AffixModifierViewModel model)
        {
            if (model == null)
            {
                return 1;
            }

            int leaks = 0;
            if (!model.modifierPreviewDevOnly || !model.eventPreviewDevOnly)
            {
                leaks++;
            }

            if (model.modifierPreviewEnabled || model.eventPreviewEnabled)
            {
                leaks++;
            }

            if (model.affectsFormalCombat)
            {
                leaks++;
            }

            return leaks;
        }

        private static int CountSimulationLeaks(SimulationResultViewModel simulation)
        {
            if (simulation == null)
            {
                return 1;
            }

            return (simulation.rows ?? new List<SimulationResultRow>())
                .Count(row => row == null || !row.devOnly || row.isEnabled || row.affectsFormalCombat);
        }

        private static int CountSeedLeaks(BuildProblemSeedDataset dataset)
        {
            if (dataset == null)
            {
                return 1;
            }

            int leaks = 0;
            if (!dataset.devOnly || dataset.isEnabled)
            {
                leaks++;
            }

            leaks += (dataset.mapRules ?? new List<MapRuleSeed>())
                .Count(rule => rule == null || !rule.devOnly || rule.isEnabled || rule.entersFormalFlow || rule.usesFormalStageData);
            leaks += (dataset.enemyProblems ?? new List<EnemyProblemSeed>())
                .Count(problem => problem == null || !problem.devOnly || problem.isEnabled || problem.entersFormalFlow || problem.referencesProductEnemyList || problem.affectsFormalCombat);
            leaks += (dataset.bossProblems ?? new List<BossProblemSeed>())
                .Count(boss => boss == null || !boss.devOnly || boss.isEnabled || boss.entersFormalFlow || boss.referencesProductBossList || boss.affectsFormalCombat);
            leaks += (dataset.weaknessWindows ?? new List<WeaknessWindowSeed>())
                .Count(window => window == null || !window.devOnly || window.isEnabled || window.entersFormalFlow || window.affectsFormalCombat);
            leaks += (dataset.dropBiases ?? new List<DropBiasSeed>())
                .Count(drop => drop == null || !drop.devOnly || drop.isEnabled || drop.entersFormalFlow || drop.referencesProductDrops || drop.grantsProductItems || !drop.reportsOnly);
            leaks += (dataset.failureHints ?? new List<FailureHintSeed>())
                .Count(hint => hint == null || !hint.devOnly || hint.isEnabled || hint.entersFormalFlow || hint.writesRuntimeUi || !hint.reportsOnly);
            return leaks;
        }

        private static bool IsFormalClear(BuildSandboxPreviewContext context)
        {
            return context != null
                && !context.readsFormalSaveData
                && !context.writesFormalFlow
                && !context.writesFormalData
                && !context.touchesFormalScene;
        }

        private static string Pass(bool value)
        {
            return value ? "PASS" : "FAIL";
        }

        private static string Join(IEnumerable<string> values)
        {
            return string.Join(";", (values ?? Enumerable.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal));
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
