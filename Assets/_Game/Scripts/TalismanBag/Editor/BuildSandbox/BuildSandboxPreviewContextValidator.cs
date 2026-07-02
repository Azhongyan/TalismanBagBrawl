#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BuildSandbox;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BuildSandboxPreviewContextValidator
    {
        public const string PackageName = "V0.4-BuildSandboxPreviewContext01";
        public const string PreviewBuildId = "buildsandbox_preview_context";

        public static List<BuildSandboxValidationReport> BuildValidationReports()
        {
            return new List<BuildSandboxValidationReport>
            {
                BuildSandboxConfigValidator.Validate(),
                BuildSandboxDevOnlyValidator.Validate(),
                BuildSandboxUiLayoutGuard.Validate(),
                BuildSandboxCoreFlowSmokeEntry.ValidatePlaceholder(),
                ItemShapeOccupancyValidator.Validate(),
                SynergyEvaluatorCoreValidator.Validate(),
                AffixRaritySandboxValidator.Validate(),
                ModifierEventBridgeValidator.Validate(),
                BuildSimulationBenchmarkValidator.Validate(),
                EnemyBossValidationPoolValidator.Validate(),
                DevChapterContentPoolValidator.Validate(),
                LedgerTaskBuildHooksValidator.Validate(),
                BuildProblemRulePoolValidator.Validate(),
                BuildProblemSeedDataValidator.Validate(),
                BuildSandboxConfigPanelValidator.Validate(),
                Validate()
            };
        }

        public static BuildSandboxPreviewContext BuildDefaultContext()
        {
            BuildEvaluationResult buildEvaluation =
                SynergyEvaluatorCoreValidator.BuildSampleEvaluation(out BuildSandboxLayoutSnapshot layoutSnapshot);
            AffixRarityEvaluationResult affixEvaluation =
                AffixRaritySandboxValidator.BuildSampleEvaluation(out _);
            CombatModifierBundle modifierBundle = ModifierEventBridge.BuildCombatModifierBundle(
                buildEvaluation,
                affixEvaluation,
                PreviewBuildId);
            EffectEventBundle eventBundle = ModifierEventBridge.BuildEffectEventBundle(
                buildEvaluation,
                affixEvaluation,
                PreviewBuildId);
            BuildSimulationBenchmarkReport simulationReport =
                BuildSimulationBenchmarkValidator.BuildSampleBenchmark();
            EnemyBossValidationPool enemyBossPool = EnemyBossValidationPool.CreateDefault();
            DevChapterContentPool devChapterPool = DevChapterContentPool.CreateDefault();
            LedgerTaskBuildHooksValidator.BuildPreview(
                out LedgerBuildTaskHook ledgerHook,
                out _,
                out List<LedgerBuildTaskProgressPreview> ledgerProgress);

            return BuildSandboxPreviewContextBuilder.Build(new BuildSandboxPreviewContextBuildInput
            {
                previewBuildId = PreviewBuildId,
                problemSeedDataset = BuildProblemSeedDataset.CreateDefault(),
                layoutSnapshot = layoutSnapshot,
                shapePlacementResults = ItemShapeOccupancyValidator.BuildSamplePlacementResults().ToList(),
                buildEvaluation = buildEvaluation,
                affixEvaluation = affixEvaluation,
                modifierBundle = modifierBundle,
                eventBundle = eventBundle,
                simulationReport = simulationReport,
                enemyBossValidationPool = enemyBossPool,
                devChapterContentPool = devChapterPool,
                ledgerTaskHook = ledgerHook,
                ledgerProgress = ledgerProgress ?? new List<LedgerBuildTaskProgressPreview>()
            });
        }

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report = new("BuildSandbox Preview Context 01");
            BuildSandboxPreviewContext context = BuildDefaultContext();
            ValidateContextIsolation(report, context);
            ValidateViewModelSections(report, context?.viewModel);
            ValidateProblemSelector(report, context?.viewModel?.problemSelector);
            ValidateProblemReadiness(report, context?.viewModel?.problemReadiness);
            ValidateAffixModifierPreview(report, context?.viewModel?.affixModifier);
            ValidateSimulationResult(report, context?.viewModel?.simulationResult);
            ValidateSourceData(report, context);
            return report;
        }

        private static void ValidateContextIsolation(
            BuildSandboxValidationReport report,
            BuildSandboxPreviewContext context)
        {
            if (context == null)
            {
                report.AddError("PREVIEW_CONTEXT_NULL", "BuildSandboxPreviewContext was not created.", PackageName);
                return;
            }

            if (!string.Equals(context.packageName, PackageName, StringComparison.Ordinal))
            {
                report.AddError(
                    "PREVIEW_CONTEXT_PACKAGE_MISMATCH",
                    $"Package mismatch. actual={context.packageName}.",
                    nameof(BuildSandboxPreviewContext));
            }

            if (!string.Equals(context.previewBuildId, PreviewBuildId, StringComparison.Ordinal))
            {
                report.AddError(
                    "PREVIEW_CONTEXT_BUILD_ID_MISMATCH",
                    $"Preview build id mismatch. actual={context.previewBuildId}.",
                    nameof(BuildSandboxPreviewContext));
            }

            ValidateFalseFlag(report, "PREVIEW_CONTEXT_ENABLED_TRUE", context.isEnabled, nameof(BuildSandboxPreviewContext));
            ValidateFalseFlag(report, "PREVIEW_CONTEXT_READS_FORMAL_SAVE", context.readsFormalSaveData, nameof(BuildSandboxPreviewContext));
            ValidateFalseFlag(report, "PREVIEW_CONTEXT_WRITES_FORMAL_FLOW", context.writesFormalFlow, nameof(BuildSandboxPreviewContext));
            ValidateFalseFlag(report, "PREVIEW_CONTEXT_WRITES_FORMAL_DATA", context.writesFormalData, nameof(BuildSandboxPreviewContext));
            ValidateFalseFlag(report, "PREVIEW_CONTEXT_TOUCHES_FORMAL_SCENE", context.touchesFormalScene, nameof(BuildSandboxPreviewContext));

            if (!context.devOnly)
            {
                report.AddError(
                    "PREVIEW_CONTEXT_DEVONLY_FALSE",
                    "BuildSandboxPreviewContext must remain devOnly=true.",
                    nameof(BuildSandboxPreviewContext));
            }

            if (!BuildSandboxFeatureFlags.AreAllDefaultsDisabled())
            {
                report.AddError(
                    "PREVIEW_CONTEXT_FEATURE_FLAG_TRUE",
                    "All BuildSandbox feature flags must keep default false.",
                    nameof(BuildSandboxFeatureFlags));
            }
            else
            {
                report.AddInfo(
                    "PREVIEW_CONTEXT_ISOLATION_PASS",
                    "Context is devOnly, disabled, and not connected to formal flow/data/scene surfaces.",
                    nameof(BuildSandboxPreviewContext));
            }
        }

        private static void ValidateViewModelSections(
            BuildSandboxValidationReport report,
            BuildSandboxPreviewViewModel viewModel)
        {
            if (viewModel == null)
            {
                report.AddError("PREVIEW_VIEWMODEL_NULL", "BuildSandboxPreviewViewModel was not created.", PackageName);
                return;
            }

            if (viewModel.OutputSectionCount < 7)
            {
                report.AddError(
                    "PREVIEW_VIEWMODEL_SECTION_COUNT_LOW",
                    $"Preview ViewModel must expose at least 7 output sections; actual={viewModel.OutputSectionCount}.",
                    nameof(BuildSandboxPreviewViewModel));
            }
            else
            {
                report.AddInfo(
                    "PREVIEW_VIEWMODEL_SECTION_COUNT_PASS",
                    $"Preview ViewModel output sections={viewModel.OutputSectionCount}.",
                    nameof(BuildSandboxPreviewViewModel));
            }

            if (viewModel.buildSummary == null
                || viewModel.synergy == null
                || viewModel.shapeOccupancy == null
                || viewModel.affixModifier == null
                || viewModel.problemReadiness == null
                || viewModel.simulationResult == null
                || viewModel.problemSelector == null)
            {
                report.AddError(
                    "PREVIEW_VIEWMODEL_SECTION_NULL",
                    "All Preview ViewModel section objects must be present.",
                    nameof(BuildSandboxPreviewViewModel));
            }

            if (viewModel.buildSummary != null)
            {
                ValidateSummary(report, viewModel.buildSummary);
            }

            if (viewModel.synergy != null)
            {
                ValidateSynergy(report, viewModel.synergy);
            }

            if (viewModel.shapeOccupancy != null)
            {
                ValidateShape(report, viewModel.shapeOccupancy);
            }
        }

        private static void ValidateSummary(BuildSandboxValidationReport report, BuildSummaryViewModel summary)
        {
            ValidateMinimum(report, "PREVIEW_SUMMARY_MAP_RULE_COUNT", "MapRule", summary.mapRuleCount, BuildProblemSeedDataValidator.RequiredMapRuleCount);
            ValidateMinimum(report, "PREVIEW_SUMMARY_ENEMY_PROBLEM_COUNT", "EnemyProblem", summary.enemyProblemCount, BuildProblemSeedDataValidator.RequiredEnemyProblemCount);
            ValidateMinimum(report, "PREVIEW_SUMMARY_BOSS_PROBLEM_COUNT", "BossProblem", summary.bossProblemCount, BuildProblemSeedDataValidator.RequiredBossProblemCount);
            ValidateMinimum(report, "PREVIEW_SUMMARY_PLACED_ITEM_COUNT", "Placed item", summary.placedItemCount, 1);
            ValidateMinimum(report, "PREVIEW_SUMMARY_OCCUPIED_CELL_COUNT", "Occupied cell", summary.occupiedCellCount, 1);
            ValidateMinimum(report, "PREVIEW_SUMMARY_TAG_COUNT", "Capability token", summary.buildTags?.Count ?? 0, 1);

            if (!summary.devOnly || summary.isEnabled || summary.readsFormalSaveData || summary.writesFormalFlow)
            {
                report.AddError(
                    "PREVIEW_SUMMARY_ISOLATION_FAIL",
                    "Build summary must remain devOnly, disabled, and formal data/flow isolated.",
                    nameof(BuildSummaryViewModel));
            }
        }

        private static void ValidateSynergy(BuildSandboxValidationReport report, SynergyViewModel synergy)
        {
            ValidateMinimum(report, "PREVIEW_SYNERGY_ACTIVE_COUNT", "Active synergy", synergy.activeSynergyCount, 1);
            ValidateMinimum(report, "PREVIEW_SYNERGY_THRESHOLD_COUNT", "Active threshold", synergy.activeThresholdCount, 1);

            if (!synergy.placementSatisfied)
            {
                report.AddError(
                    "PREVIEW_SYNERGY_PLACEMENT_UNSATISFIED",
                    "PreviewContext sample should carry placement-satisfied synergy data.",
                    nameof(SynergyViewModel));
            }

            if (!synergy.energySatisfied)
            {
                report.AddError(
                    "PREVIEW_SYNERGY_ENERGY_UNSATISFIED",
                    "PreviewContext sample should carry energy-satisfied synergy data.",
                    nameof(SynergyViewModel));
            }
        }

        private static void ValidateShape(BuildSandboxValidationReport report, ShapeOccupancyViewModel shape)
        {
            ValidateMinimum(report, "PREVIEW_SHAPE_PLACED_ITEM_COUNT", "Shape placed item", shape.placedItemCount, 1);
            ValidateMinimum(report, "PREVIEW_SHAPE_OCCUPIED_CELL_COUNT", "Shape occupied cell", shape.occupiedCellCount, 1);
            ValidateMinimum(report, "PREVIEW_SHAPE_VALID_SAMPLE_COUNT", "Valid shape sample", shape.validPlacementCount, 1);

            if (!shape.placementLegal)
            {
                report.AddError(
                    "PREVIEW_SHAPE_LAYOUT_ILLEGAL",
                    "Preview layout snapshot must be legal even when QA sample data also lists invalid placement examples.",
                    nameof(ShapeOccupancyViewModel));
            }

            if (shape.invalidPlacementCount > 0)
            {
                report.AddInfo(
                    "PREVIEW_SHAPE_INVALID_SAMPLES_PRESENT",
                    $"Invalid placement samples are exposed for QA readability: {shape.invalidPlacementCount}.",
                    nameof(ShapeOccupancyViewModel));
            }
        }

        private static void ValidateProblemSelector(
            BuildSandboxValidationReport report,
            ProblemSelectorViewModel selector)
        {
            if (selector == null)
            {
                report.AddError("PREVIEW_SELECTOR_NULL", "Problem selector ViewModel is missing.", nameof(ProblemSelectorViewModel));
                return;
            }

            ValidateMinimum(report, "PREVIEW_SELECTOR_MAP_RULE_COUNT", "MapRule selector", selector.mapRuleCount, BuildProblemSeedDataValidator.RequiredMapRuleCount);
            ValidateMinimum(report, "PREVIEW_SELECTOR_ENEMY_PROBLEM_COUNT", "EnemyProblem selector", selector.enemyProblemCount, BuildProblemSeedDataValidator.RequiredEnemyProblemCount);
            ValidateMinimum(report, "PREVIEW_SELECTOR_BOSS_PROBLEM_COUNT", "BossProblem selector", selector.bossProblemCount, BuildProblemSeedDataValidator.RequiredBossProblemCount);

            int nonDevOnly = CountNonDevOnly(selector.mapRuleOptions)
                + CountNonDevOnly(selector.enemyProblemOptions)
                + CountNonDevOnly(selector.bossProblemOptions);
            int enabled = CountEnabled(selector.mapRuleOptions)
                + CountEnabled(selector.enemyProblemOptions)
                + CountEnabled(selector.bossProblemOptions);
            if (nonDevOnly > 0 || enabled > 0)
            {
                report.AddError(
                    "PREVIEW_SELECTOR_ISOLATION_FAIL",
                    $"Selector options must stay devOnly=true and isEnabled=false. nonDevOnly={nonDevOnly}, enabled={enabled}.",
                    nameof(ProblemSelectorViewModel));
            }
        }

        private static void ValidateProblemReadiness(
            BuildSandboxValidationReport report,
            ProblemReadinessViewModel readiness)
        {
            if (readiness == null)
            {
                report.AddError("PREVIEW_READINESS_NULL", "Problem readiness ViewModel is missing.", nameof(ProblemReadinessViewModel));
                return;
            }

            ValidateMinimum(report, "PREVIEW_READINESS_BOSS_COUNT", "Boss readiness row", readiness.bossReadinessCount, BuildProblemSeedDataValidator.RequiredBossProblemCount);
            ValidateMinimum(report, "PREVIEW_READINESS_KEY_COUNT", "Boss readiness key", readiness.totalKeyCount, 18);
            ValidateMinimum(report, "PREVIEW_READINESS_DROP_BIAS_COUNT", "DropBias", readiness.dropBiasCount, 12);

            foreach (BossReadinessViewModel boss in readiness.bossRows ?? new List<BossReadinessViewModel>())
            {
                if (boss.keyCount == 0 || boss.weaknessWindowIds.Count == 0 || boss.dropBiasIds.Count == 0)
                {
                    report.AddError(
                        "PREVIEW_READINESS_BOSS_COVERAGE_MISSING",
                        $"Boss readiness row lacks keys, weakness windows, or drop biases: {boss.bossProblemId}.",
                        nameof(BossReadinessViewModel));
                }
            }
        }

        private static void ValidateAffixModifierPreview(
            BuildSandboxValidationReport report,
            AffixModifierViewModel affixModifier)
        {
            if (affixModifier == null)
            {
                report.AddError("PREVIEW_AFFIX_MODIFIER_NULL", "Affix/Modifier ViewModel is missing.", nameof(AffixModifierViewModel));
                return;
            }

            ValidateMinimum(report, "PREVIEW_AFFIX_ITEM_COUNT", "Affix item", affixModifier.affixItemCount, 1);
            ValidateMinimum(report, "PREVIEW_AFFIX_SELECTED_COUNT", "Selected affix", affixModifier.selectedAffixCount, 1);
            ValidateMinimum(report, "PREVIEW_MODIFIER_COUNT", "Modifier preview", affixModifier.modifierCount, 1);
            ValidateMinimum(report, "PREVIEW_EVENT_COUNT", "Event preview", affixModifier.eventCount, 1);

            if (!affixModifier.orangeCoreAffixPresent)
            {
                report.AddError("PREVIEW_ORANGE_CORE_MISSING", "Orange core affix must be readable in PreviewContext.", nameof(AffixModifierViewModel));
            }

            if (!affixModifier.bondPlusOnePresent)
            {
                report.AddError("PREVIEW_BOND_PLUS_ONE_MISSING", "Bond +1 affix must be readable in PreviewContext.", nameof(AffixModifierViewModel));
            }

            if (!affixModifier.modifierPreviewDevOnly
                || affixModifier.modifierPreviewEnabled
                || !affixModifier.eventPreviewDevOnly
                || affixModifier.eventPreviewEnabled
                || affixModifier.affectsFormalCombat)
            {
                report.AddError(
                    "PREVIEW_MODIFIER_EVENT_ISOLATION_FAIL",
                    "Modifier/Event preview data must stay devOnly, disabled, and non-formal-combat.",
                    nameof(AffixModifierViewModel));
            }
        }

        private static void ValidateSimulationResult(
            BuildSandboxValidationReport report,
            SimulationResultViewModel simulation)
        {
            if (simulation == null)
            {
                report.AddError("PREVIEW_SIMULATION_NULL", "Simulation ViewModel is missing.", nameof(SimulationResultViewModel));
                return;
            }

            ValidateMinimum(report, "PREVIEW_SIMULATION_SCENARIO_COUNT", "Simulation scenario", simulation.scenarioCount, 1);
            ValidateMinimum(report, "PREVIEW_SIMULATION_RESULT_COUNT", "Simulation result", simulation.resultCount, 1);

            int leakCount = (simulation.rows ?? new List<SimulationResultRow>())
                .Count(row => row == null || !row.devOnly || row.isEnabled || row.affectsFormalCombat);
            if (leakCount > 0)
            {
                report.AddError(
                    "PREVIEW_SIMULATION_ISOLATION_FAIL",
                    $"Simulation result rows must stay devOnly, disabled, and non-formal-combat. leaks={leakCount}.",
                    nameof(SimulationResultViewModel));
            }
        }

        private static void ValidateSourceData(
            BuildSandboxValidationReport report,
            BuildSandboxPreviewContext context)
        {
            if (context == null)
            {
                return;
            }

            if (context.problemSeedDataset == null
                || context.problemSeedDataset.mapRules.Count < BuildProblemSeedDataValidator.RequiredMapRuleCount
                || context.problemSeedDataset.enemyProblems.Count < BuildProblemSeedDataValidator.RequiredEnemyProblemCount
                || context.problemSeedDataset.bossProblems.Count < BuildProblemSeedDataValidator.RequiredBossProblemCount)
            {
                report.AddError(
                    "PREVIEW_CONTEXT_SEED_DATA_INCOMPLETE",
                    "PreviewContext must carry Phase 2 BuildProblem seed data.",
                    nameof(BuildProblemSeedDataset));
            }

            if (context.enemyBossValidationPool == null || context.enemyBossValidationPool.bosses.Count == 0)
            {
                report.AddError(
                    "PREVIEW_CONTEXT_ENEMY_BOSS_POOL_MISSING",
                    "PreviewContext must carry Enemy/Boss validation pool summary data.",
                    nameof(EnemyBossValidationPool));
            }

            if (context.devChapterContentPool == null || context.devChapterContentPool.chapters.Count == 0)
            {
                report.AddError(
                    "PREVIEW_CONTEXT_DEVCHAPTER_POOL_MISSING",
                    "PreviewContext must carry DevChapter content pool summary data.",
                    nameof(DevChapterContentPool));
            }
        }

        private static void ValidateMinimum(
            BuildSandboxValidationReport report,
            string code,
            string label,
            int actual,
            int expected)
        {
            if (actual < expected)
            {
                report.AddError(code, $"{label} count too low. actual={actual}, expected>={expected}.", PackageName);
                return;
            }

            report.AddInfo(code, $"{label} count pass. actual={actual}, expected>={expected}.", PackageName);
        }

        private static void ValidateFalseFlag(
            BuildSandboxValidationReport report,
            string code,
            bool value,
            string path)
        {
            if (value)
            {
                report.AddError(code, "PreviewContext isolation flag must stay false.", path);
            }
        }

        private static int CountNonDevOnly(IEnumerable<ProblemSelectorOption> options)
        {
            return (options ?? Enumerable.Empty<ProblemSelectorOption>()).Count(option => option == null || !option.devOnly);
        }

        private static int CountEnabled(IEnumerable<ProblemSelectorOption> options)
        {
            return (options ?? Enumerable.Empty<ProblemSelectorOption>()).Count(option => option != null && option.isEnabled);
        }
    }
}
#endif
