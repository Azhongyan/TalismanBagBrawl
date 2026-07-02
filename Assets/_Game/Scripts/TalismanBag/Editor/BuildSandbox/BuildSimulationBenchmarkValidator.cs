#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TalismanBag.BuildSandbox;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BuildSimulationBenchmarkValidator
    {
        private static readonly string[] SourceFiles =
        {
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildSimulationBenchmark.cs"
        };

        private static readonly string[] ForbiddenFormalReferenceTokens =
        {
            "V02RunFlowController",
            "V02FormationGridFrame",
            "DamageText",
            "PageState",
            "FormationState",
            "MainTrialProgressData",
            "PlayerPrefs",
            "SaveData",
            "RewardConfig",
            "UpgradeConfig",
            "BossReward",
            "FormalDrop",
            "FormalForge"
        };

        private static readonly string[] FormalScenarioTokens =
        {
            "formal",
            "chapter",
            "1-10",
            "2-10",
            "BossConfig",
            "EnemyConfig",
            "MainTrial"
        };

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report = new("Build Simulation Benchmark");
            ValidateFeatureFlags(report);

            BuildSimulationBenchmarkReport benchmark = BuildSampleBenchmark();
            ValidateScenarios(report, benchmark.scenarios);
            ValidateResults(report, benchmark.results);
            ValidateComparisons(report, benchmark.comparisons);
            ValidatePreviewUsage(report, benchmark.results);
            ValidateSourceIsolation(report);
            return report;
        }

        public static BuildSimulationBenchmarkReport BuildSampleBenchmark()
        {
            ModifierEventBridgeValidator.BuildSamplePreview(
                out BuildEvaluationResult buildEvaluation,
                out AffixRarityEvaluationResult affixEvaluation,
                out CombatModifierBundle modifierBundle,
                out EffectEventBundle eventBundle);

            BuildEvaluationResult noSynergyEvaluation = new();
            AffixRarityEvaluationResult noAffixEvaluation = new();
            BuildSandboxLayoutSnapshot synergySnapshot = SynergyEvaluatorCoreValidator.BuildSeedSnapshot(
                BuildSandboxConfigValidator.CollectSynergyConfigs());

            BuildSimulationScenario noSynergy = CreateScenario(
                "sandbox_no_synergy",
                "No Synergy / With Affix Preview",
                "devOnly_training_dummy",
                "none",
                "mixed_preview",
                "powered_sample",
                "sample_layout_no_synergy",
                synergySnapshot,
                noSynergyEvaluation,
                affixEvaluation,
                ModifierEventBridge.BuildCombatModifierBundle(noSynergyEvaluation, affixEvaluation, "sandbox_no_synergy"),
                ModifierEventBridge.BuildEffectEventBundle(noSynergyEvaluation, affixEvaluation, "sandbox_no_synergy"),
                batchSeed: 1001);

            BuildSimulationScenario noAffix = CreateScenario(
                "sandbox_no_affix",
                "With Synergy / No Affix Preview",
                "devOnly_training_dummy",
                "none",
                "none",
                "powered_sample",
                "sample_layout_no_affix",
                synergySnapshot,
                buildEvaluation,
                noAffixEvaluation,
                ModifierEventBridge.BuildCombatModifierBundle(buildEvaluation, noAffixEvaluation, "sandbox_no_affix"),
                ModifierEventBridge.BuildEffectEventBundle(buildEvaluation, noAffixEvaluation, "sandbox_no_affix"),
                batchSeed: 1002);

            BuildSimulationScenario fullPreview = CreateScenario(
                "sandbox_full_preview",
                "With Synergy / With Affix Preview",
                "devOnly_training_dummy",
                "none",
                "mixed_preview",
                "powered_sample",
                "sample_adjacency_and_energy",
                synergySnapshot,
                buildEvaluation,
                affixEvaluation,
                modifierBundle,
                eventBundle,
                batchSeed: 1003);

            BuildSimulationScenario bossPlaceholder = CreateScenario(
                "sandbox_boss_placeholder",
                "Boss Placeholder / Orange Core Preview",
                "devOnly_boss_shell",
                "devOnly_shield_phase_placeholder",
                "orange_focus_preview",
                "powered_sample",
                "boss_placeholder_layout",
                synergySnapshot,
                buildEvaluation,
                affixEvaluation,
                ModifierEventBridge.BuildCombatModifierBundle(buildEvaluation, affixEvaluation, "sandbox_boss_placeholder"),
                ModifierEventBridge.BuildEffectEventBundle(buildEvaluation, affixEvaluation, "sandbox_boss_placeholder"),
                batchSeed: 1004);

            return BuildSimulationRunner.RunBatch(new[]
            {
                noSynergy,
                noAffix,
                fullPreview,
                bossPlaceholder
            });
        }

        private static BuildSimulationScenario CreateScenario(
            string buildId,
            string displayName,
            string enemyType,
            string bossMechanic,
            string itemRarity,
            string energyCondition,
            string placementRelation,
            BuildSandboxLayoutSnapshot layoutSnapshot,
            BuildEvaluationResult buildEvaluation,
            AffixRarityEvaluationResult affixEvaluation,
            CombatModifierBundle modifierBundle,
            EffectEventBundle eventBundle,
            int batchSeed)
        {
            BuildSimulationScenario scenario = new()
            {
                buildId = buildId,
                displayName = displayName,
                enemyType = enemyType,
                bossMechanic = bossMechanic,
                itemRarity = itemRarity,
                energyCondition = energyCondition,
                placementRelation = placementRelation,
                layoutSnapshot = layoutSnapshot ?? new BuildSandboxLayoutSnapshot(),
                buildEvaluation = buildEvaluation ?? new BuildEvaluationResult(),
                affixEvaluation = affixEvaluation ?? new AffixRarityEvaluationResult(),
                modifierBundle = modifierBundle ?? new CombatModifierBundle(),
                eventBundle = eventBundle ?? new EffectEventBundle(),
                batchSeed = batchSeed,
                devOnly = true,
                isEnabled = false,
                notes = "sandbox estimate; devOnly placeholder enemy/boss tags; no formal combat or save data."
            };

            scenario.buildItemIds = scenario.layoutSnapshot.placedItems?
                .Where(item => item != null && !string.IsNullOrWhiteSpace(item.itemId))
                .Select(item => item.itemId)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList() ?? new List<string>();

            scenario.affixCombination = scenario.affixEvaluation.affixIds?
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList() ?? new List<string>();

            return scenario;
        }

        private static void ValidateFeatureFlags(BuildSandboxValidationReport report)
        {
            foreach (BuildSandboxFeatureFlagDefinition flag in BuildSandboxFeatureFlags.All)
            {
                if (flag.DefaultValue)
                {
                    report.AddError(
                        "BENCHMARK_FEATURE_FLAG_TRUE",
                        $"{flag.Key} must stay false for BuildSimulationBenchmark01.",
                        nameof(BuildSandboxFeatureFlags));
                    continue;
                }

                report.AddInfo(
                    "BENCHMARK_FEATURE_FLAG_FALSE",
                    $"{flag.Key}=false.",
                    nameof(BuildSandboxFeatureFlags));
            }
        }

        private static void ValidateScenarios(
            BuildSandboxValidationReport report,
            IReadOnlyList<BuildSimulationScenario> scenarios)
        {
            if (scenarios == null || scenarios.Count == 0)
            {
                report.AddError(
                    "BENCHMARK_SCENARIO_MISSING",
                    "BuildSimulationBenchmark01 requires at least one scenario.",
                    nameof(BuildSimulationScenario));
                return;
            }

            HashSet<string> ids = new(StringComparer.Ordinal);
            foreach (BuildSimulationScenario scenario in scenarios)
            {
                string path = $"{nameof(BuildSimulationScenario)}:{scenario?.buildId}";
                if (scenario == null)
                {
                    report.AddError("BENCHMARK_SCENARIO_NULL", "Scenario is null.", nameof(BuildSimulationScenario));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(scenario.buildId))
                {
                    report.AddError("BENCHMARK_SCENARIO_ID_MISSING", "Scenario buildId is empty.", path);
                }
                else if (!ids.Add(scenario.buildId))
                {
                    report.AddError("BENCHMARK_SCENARIO_ID_DUPLICATE", $"Duplicate scenario buildId: {scenario.buildId}.", path);
                }

                if (!scenario.devOnly)
                {
                    report.AddError("BENCHMARK_SCENARIO_DEVONLY_FALSE", "Scenario must keep devOnly=true.", path);
                }

                if (scenario.isEnabled)
                {
                    report.AddError("BENCHMARK_SCENARIO_ENABLED_TRUE", "Scenario must keep isEnabled=false.", path);
                }

                if (scenario.buildItemIds == null || scenario.buildItemIds.Count == 0)
                {
                    report.AddError("BENCHMARK_SCENARIO_BUILD_EMPTY", "Scenario build input is empty.", path);
                }

                if (ContainsFormalToken(scenario.enemyType) || ContainsFormalToken(scenario.bossMechanic))
                {
                    report.AddError(
                        "BENCHMARK_SCENARIO_FORMAL_POOL_REFERENCE",
                        $"Scenario references a formal enemy/Boss/chapter token: enemyType={scenario.enemyType}, bossMechanic={scenario.bossMechanic}.",
                        path);
                }

                if (scenario.referencesFormalEnemyPool || scenario.referencesFormalBossPool || scenario.readsFormalPlayerData)
                {
                    report.AddError(
                        "BENCHMARK_SCENARIO_FORMAL_DATA_REFERENCE",
                        "Scenario must not reference formal enemy/Boss pools or formal save/player data.",
                        path);
                }

                report.AddInfo(
                    "BENCHMARK_SCENARIO_SCANNED",
                    $"Scenario {scenario.buildId}: enemy={scenario.enemyType}, boss={scenario.bossMechanic}, affixes={scenario.affixCombination.Count}.",
                    path);
            }
        }

        private static void ValidateResults(
            BuildSandboxValidationReport report,
            IReadOnlyList<BuildSimulationResult> results)
        {
            if (results == null || results.Count == 0)
            {
                report.AddError(
                    "BENCHMARK_RESULT_MISSING",
                    "BuildSimulationRunner batch should output simulation results.",
                    nameof(BuildSimulationRunner));
                return;
            }

            foreach (BuildSimulationResult result in results)
            {
                string path = $"{nameof(BuildSimulationResult)}:{result?.buildId}";
                if (result == null)
                {
                    report.AddError("BENCHMARK_RESULT_NULL", "Simulation result is null.", nameof(BuildSimulationResult));
                    continue;
                }

                if (!result.devOnly)
                {
                    report.AddError("BENCHMARK_RESULT_DEVONLY_FALSE", "Benchmark output must keep devOnly=true.", path);
                }

                if (result.isEnabled)
                {
                    report.AddError("BENCHMARK_RESULT_ENABLED_TRUE", "Benchmark output must keep isEnabled=false.", path);
                }

                if (!result.sandboxEstimate)
                {
                    report.AddError("BENCHMARK_RESULT_NOT_SANDBOX", "Benchmark output must be marked sandbox estimate.", path);
                }

                if (result.affectsFormalCombat)
                {
                    report.AddError("BENCHMARK_RESULT_AFFECTS_FORMAL_COMBAT", "Benchmark output must not affect formal combat.", path);
                }

                if (result.numericAnomaly)
                {
                    report.AddError("BENCHMARK_RESULT_NUMERIC_ANOMALY", "Simulation result reported a numeric anomaly.", path);
                }
            }

            report.AddInfo(
                "BENCHMARK_BATCH_RESULTS_PRESENT",
                $"BuildSimulationRunner produced {results.Count} result(s).",
                nameof(BuildSimulationRunner));
        }

        private static void ValidateComparisons(
            BuildSandboxValidationReport report,
            IReadOnlyList<BuildComparisonResult> comparisons)
        {
            if (comparisons == null || comparisons.Count == 0)
            {
                report.AddError(
                    "BENCHMARK_COMPARISON_MISSING",
                    "Benchmark should compare at least one build pair.",
                    nameof(BuildComparisonResult));
                return;
            }

            RequireComparison(report, comparisons, "with_vs_without_synergy");
            RequireComparison(report, comparisons, "with_vs_without_affix");
            foreach (BuildComparisonResult comparison in comparisons)
            {
                if (!comparison.devOnly || comparison.isEnabled || !comparison.sandboxEstimate)
                {
                    report.AddError(
                        "BENCHMARK_COMPARISON_ISOLATION_FAIL",
                        $"Comparison isolation flags are invalid: {comparison.comparisonName}.",
                        nameof(BuildComparisonResult));
                }
            }
        }

        private static void ValidatePreviewUsage(
            BuildSandboxValidationReport report,
            IReadOnlyList<BuildSimulationResult> results)
        {
            bool usedModifier = results.Any(result => result.combatModifierPreviewUsed);
            bool usedEvent = results.Any(result => result.effectEventPreviewUsed);
            if (!usedModifier)
            {
                report.AddError(
                    "BENCHMARK_MODIFIER_PREVIEW_UNUSED",
                    "Benchmark must use CombatModifierBundle preview results.",
                    nameof(CombatModifierBundle));
            }

            if (!usedEvent)
            {
                report.AddError(
                    "BENCHMARK_EVENT_PREVIEW_UNUSED",
                    "Benchmark must use EffectEventBundle preview results.",
                    nameof(EffectEventBundle));
            }

            if (usedModifier && usedEvent)
            {
                report.AddInfo(
                    "BENCHMARK_PREVIEW_INPUTS_USED",
                    "Benchmark uses CombatModifierBundle and EffectEventBundle preview outputs.",
                    nameof(BuildSimulationRunner));
            }
        }

        private static void RequireComparison(
            BuildSandboxValidationReport report,
            IReadOnlyList<BuildComparisonResult> comparisons,
            string comparisonName)
        {
            if (comparisons.Any(comparison =>
                    string.Equals(comparison.comparisonName, comparisonName, StringComparison.Ordinal)))
            {
                report.AddInfo(
                    "BENCHMARK_COMPARISON_PRESENT",
                    $"Required comparison present: {comparisonName}.",
                    nameof(BuildComparisonResult));
                return;
            }

            report.AddError(
                "BENCHMARK_COMPARISON_REQUIRED_MISSING",
                $"Required comparison missing: {comparisonName}.",
                nameof(BuildComparisonResult));
        }

        private static void ValidateSourceIsolation(BuildSandboxValidationReport report)
        {
            string projectRoot = Directory.GetCurrentDirectory();
            foreach (string relativePath in SourceFiles)
            {
                string path = Path.Combine(projectRoot, relativePath);
                if (!File.Exists(path))
                {
                    report.AddError(
                        "BENCHMARK_SOURCE_FILE_MISSING",
                        $"Source file is missing: {relativePath}.",
                        relativePath);
                    continue;
                }

                string text = File.ReadAllText(path);
                foreach (string token in ForbiddenFormalReferenceTokens)
                {
                    if (text.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        report.AddError(
                            "BENCHMARK_FORMAL_REFERENCE",
                            $"Benchmark source references forbidden formal system token: {token}.",
                            relativePath);
                    }
                }
            }

            report.AddInfo(
                "BENCHMARK_SOURCE_ISOLATION_SCANNED",
                "BuildSimulationBenchmark source scan completed for forbidden formal-system tokens.",
                nameof(BuildSimulationRunner));
        }

        private static bool ContainsFormalToken(string value)
        {
            return FormalScenarioTokens.Any(token =>
                !string.IsNullOrWhiteSpace(value)
                && value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
}
#endif
