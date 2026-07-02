using System;
using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.BuildSandbox
{
    [Serializable]
    public sealed class BuildSandboxPreviewContextBuildInput
    {
        public string previewBuildId = "buildsandbox_preview_context";
        public BuildProblemSeedDataset problemSeedDataset = BuildProblemSeedDataset.CreateDefault();
        public BuildSandboxLayoutSnapshot layoutSnapshot = new();
        public List<ShapePlacementResult> shapePlacementResults = new();
        public BuildEvaluationResult buildEvaluation = new();
        public AffixRarityEvaluationResult affixEvaluation = new();
        public CombatModifierBundle modifierBundle = new();
        public EffectEventBundle eventBundle = new();
        public BuildSimulationBenchmarkReport simulationReport = new();
        public EnemyBossValidationPool enemyBossValidationPool = EnemyBossValidationPool.CreateDefault();
        public DevChapterContentPool devChapterContentPool = DevChapterContentPool.CreateDefault();
        public LedgerBuildTaskHook ledgerTaskHook = LedgerTaskBuildHooksPreview.CreateDefaultHook();
        public List<LedgerBuildTaskProgressPreview> ledgerProgress = new();
    }

    [Serializable]
    public sealed class BuildSandboxPreviewContext
    {
        public string packageName = "V0.4-BuildSandboxPreviewContext01";
        public string previewBuildId = "buildsandbox_preview_context";
        public bool devOnly = true;
        public bool isEnabled;
        public bool readsFormalSaveData;
        public bool writesFormalFlow;
        public bool writesFormalData;
        public bool touchesFormalScene;
        public BuildProblemSeedDataset problemSeedDataset = BuildProblemSeedDataset.CreateDefault();
        public BuildSandboxLayoutSnapshot layoutSnapshot = new();
        public BuildEvaluationResult buildEvaluation = new();
        public AffixRarityEvaluationResult affixEvaluation = new();
        public CombatModifierBundle modifierBundle = new();
        public EffectEventBundle eventBundle = new();
        public BuildSimulationBenchmarkReport simulationReport = new();
        public EnemyBossValidationPool enemyBossValidationPool = EnemyBossValidationPool.CreateDefault();
        public DevChapterContentPool devChapterContentPool = DevChapterContentPool.CreateDefault();
        public BuildSandboxPreviewViewModel viewModel = new();
    }

    [Serializable]
    public sealed class BuildSandboxPreviewViewModel
    {
        public BuildSummaryViewModel buildSummary = new();
        public SynergyViewModel synergy = new();
        public ShapeOccupancyViewModel shapeOccupancy = new();
        public AffixModifierViewModel affixModifier = new();
        public ProblemReadinessViewModel problemReadiness = new();
        public SimulationResultViewModel simulationResult = new();
        public ProblemSelectorViewModel problemSelector = new();

        public int OutputSectionCount => 7;
    }

    [Serializable]
    public sealed class BuildSummaryViewModel
    {
        public string previewBuildId = string.Empty;
        public int placedItemCount;
        public int occupiedCellCount;
        public int mapRuleCount;
        public int enemyProblemCount;
        public int bossProblemCount;
        public int activeSynergyCount;
        public int activeThresholdCount;
        public int selectedAffixCount;
        public int modifierCount;
        public int eventCount;
        public bool featureFlagsAllDisabled;
        public bool devOnly = true;
        public bool isEnabled;
        public bool readsFormalSaveData;
        public bool writesFormalFlow;
        public List<string> buildTags = new();
        public List<string> activeSynergies = new();
        public List<string> activeThresholds = new();
    }

    [Serializable]
    public sealed class SynergyViewModel
    {
        public int activeSynergyCount;
        public int activeThresholdCount;
        public bool placementSatisfied = true;
        public bool energySatisfied = true;
        public int missingRequirementCount;
        public string nextThresholdHint = "none";
        public List<SynergyPreviewRow> rows = new();
        public List<string> missingRequirementSummaries = new();
    }

    [Serializable]
    public sealed class SynergyPreviewRow
    {
        public string synergyId = string.Empty;
        public string displayName = string.Empty;
        public int matchedCount;
        public List<string> activeThresholds = new();
        public List<string> sourceItems = new();
        public bool placementSatisfied = true;
        public bool energySatisfied = true;
    }

    [Serializable]
    public sealed class ShapeOccupancyViewModel
    {
        public int placedItemCount;
        public int occupiedCellCount;
        public int placementSampleCount;
        public int validPlacementCount;
        public int invalidPlacementCount;
        public bool placementLegal = true;
        public List<string> invalidReasons = new();
        public List<string> placedItemIds = new();
    }

    [Serializable]
    public sealed class AffixModifierViewModel
    {
        public int affixItemCount;
        public int rarityCount;
        public int selectedAffixCount;
        public bool orangeCoreAffixPresent;
        public bool bondPlusOnePresent;
        public int modifierCount;
        public int eventCount;
        public bool modifierPreviewDevOnly = true;
        public bool modifierPreviewEnabled;
        public bool eventPreviewDevOnly = true;
        public bool eventPreviewEnabled;
        public bool affectsFormalCombat;
        public List<string> rarityIds = new();
        public List<string> affixIds = new();
        public List<string> modifierTypes = new();
        public List<string> eventTypes = new();
    }

    [Serializable]
    public sealed class ProblemSelectorViewModel
    {
        public int mapRuleCount;
        public int enemyProblemCount;
        public int bossProblemCount;
        public List<ProblemSelectorOption> mapRuleOptions = new();
        public List<ProblemSelectorOption> enemyProblemOptions = new();
        public List<ProblemSelectorOption> bossProblemOptions = new();
    }

    [Serializable]
    public sealed class ProblemSelectorOption
    {
        public string id = string.Empty;
        public string displayName = string.Empty;
        public string summary = string.Empty;
        public bool devOnly = true;
        public bool isEnabled;
    }

    [Serializable]
    public sealed class ProblemReadinessViewModel
    {
        public int bossReadinessCount;
        public int readyBossCount;
        public int totalKeyCount;
        public int satisfiedKeyCount;
        public int failureReasonCount;
        public int recommendedActionCount;
        public int dropBiasCount;
        public List<BossReadinessViewModel> bossRows = new();
    }

    [Serializable]
    public sealed class BossReadinessViewModel
    {
        public string bossProblemId = string.Empty;
        public string displayName = string.Empty;
        public string validationGoal = string.Empty;
        public int minimumKeysRequired;
        public int keyCount;
        public int satisfiedKeyCount;
        public bool ready;
        public List<BossReadinessKeyViewModel> keys = new();
        public List<string> failureReasons = new();
        public List<string> recommendedActions = new();
        public List<string> dropBiasIds = new();
        public List<string> weaknessWindowIds = new();
    }

    [Serializable]
    public sealed class BossReadinessKeyViewModel
    {
        public string keyId = string.Empty;
        public string keyCategory = string.Empty;
        public string requirementId = string.Empty;
        public string problemAttribute = string.Empty;
        public int requiredScore;
        public int observedScore;
        public bool satisfied;
        public string hint = string.Empty;
    }

    [Serializable]
    public sealed class SimulationResultViewModel
    {
        public int scenarioCount;
        public int resultCount;
        public int comparisonCount;
        public float averageWinRate;
        public float averageClearTimeSeconds;
        public string bestBuildId = string.Empty;
        public List<SimulationResultRow> rows = new();
    }

    [Serializable]
    public sealed class SimulationResultRow
    {
        public string buildId = string.Empty;
        public string buildName = string.Empty;
        public float winRate;
        public float clearTimeSeconds;
        public float shieldBreakEfficiency;
        public int triggeredEventCount;
        public string failureReason = string.Empty;
        public bool devOnly = true;
        public bool isEnabled;
        public bool affectsFormalCombat;
    }

    public static class BuildSandboxPreviewContextBuilder
    {
        public static BuildSandboxPreviewContext Build(BuildSandboxPreviewContextBuildInput input)
        {
            BuildSandboxPreviewContextBuildInput safeInput = input ?? new BuildSandboxPreviewContextBuildInput();
            BuildProblemSeedDataset dataset = safeInput.problemSeedDataset ?? BuildProblemSeedDataset.CreateDefault();
            BuildSandboxLayoutSnapshot snapshot = safeInput.layoutSnapshot ?? new BuildSandboxLayoutSnapshot();
            BuildEvaluationResult buildEvaluation = safeInput.buildEvaluation ?? new BuildEvaluationResult();
            AffixRarityEvaluationResult affixEvaluation = safeInput.affixEvaluation ?? new AffixRarityEvaluationResult();
            CombatModifierBundle modifierBundle = safeInput.modifierBundle ?? new CombatModifierBundle();
            EffectEventBundle eventBundle = safeInput.eventBundle ?? new EffectEventBundle();
            BuildSimulationBenchmarkReport simulationReport =
                safeInput.simulationReport ?? new BuildSimulationBenchmarkReport();

            BuildSandboxPreviewContext context = new()
            {
                previewBuildId = string.IsNullOrWhiteSpace(safeInput.previewBuildId)
                    ? "buildsandbox_preview_context"
                    : safeInput.previewBuildId,
                devOnly = true,
                isEnabled = false,
                readsFormalSaveData = false,
                writesFormalFlow = false,
                writesFormalData = false,
                touchesFormalScene = false,
                problemSeedDataset = dataset,
                layoutSnapshot = snapshot,
                buildEvaluation = buildEvaluation,
                affixEvaluation = affixEvaluation,
                modifierBundle = modifierBundle,
                eventBundle = eventBundle,
                simulationReport = simulationReport,
                enemyBossValidationPool = safeInput.enemyBossValidationPool ?? EnemyBossValidationPool.CreateDefault(),
                devChapterContentPool = safeInput.devChapterContentPool ?? DevChapterContentPool.CreateDefault()
            };

            context.viewModel = BuildViewModel(
                context,
                safeInput.shapePlacementResults ?? new List<ShapePlacementResult>(),
                safeInput.ledgerProgress ?? new List<LedgerBuildTaskProgressPreview>());
            return context;
        }

        private static BuildSandboxPreviewViewModel BuildViewModel(
            BuildSandboxPreviewContext context,
            IReadOnlyList<ShapePlacementResult> shapePlacementResults,
            IReadOnlyList<LedgerBuildTaskProgressPreview> ledgerProgress)
        {
            BuildSandboxPreviewViewModel viewModel = new()
            {
                buildSummary = BuildSummary(context),
                synergy = BuildSynergy(context.buildEvaluation),
                shapeOccupancy = BuildShapeOccupancy(context.layoutSnapshot, shapePlacementResults),
                affixModifier = BuildAffixModifier(context.affixEvaluation, context.modifierBundle, context.eventBundle),
                problemSelector = BuildProblemSelector(context.problemSeedDataset),
                simulationResult = BuildSimulationResult(context.simulationReport)
            };
            viewModel.problemReadiness = BuildProblemReadiness(context, ledgerProgress);
            return viewModel;
        }

        private static BuildSummaryViewModel BuildSummary(BuildSandboxPreviewContext context)
        {
            BuildSandboxLayoutSnapshot snapshot = context.layoutSnapshot ?? new BuildSandboxLayoutSnapshot();
            BuildProblemSeedDataset dataset = context.problemSeedDataset ?? BuildProblemSeedDataset.CreateDefault();
            BuildEvaluationResult buildEvaluation = context.buildEvaluation ?? new BuildEvaluationResult();
            AffixRarityEvaluationResult affixEvaluation =
                context.affixEvaluation ?? new AffixRarityEvaluationResult();
            CombatModifierBundle modifierBundle = context.modifierBundle ?? new CombatModifierBundle();
            EffectEventBundle eventBundle = context.eventBundle ?? new EffectEventBundle();

            return new BuildSummaryViewModel
            {
                previewBuildId = context.previewBuildId,
                placedItemCount = snapshot.placedItems?.Count ?? 0,
                occupiedCellCount = CountOccupiedCells(snapshot),
                mapRuleCount = dataset.mapRules?.Count ?? 0,
                enemyProblemCount = dataset.enemyProblems?.Count ?? 0,
                bossProblemCount = dataset.bossProblems?.Count ?? 0,
                activeSynergyCount = buildEvaluation.activeSynergies?.Count ?? 0,
                activeThresholdCount = buildEvaluation.activeThresholds?.Count ?? 0,
                selectedAffixCount = affixEvaluation.affixIds?.Count ?? 0,
                modifierCount = modifierBundle.modifiers?.Count ?? 0,
                eventCount = eventBundle.events?.Count ?? 0,
                featureFlagsAllDisabled = BuildSandboxFeatureFlags.AreAllDefaultsDisabled(),
                devOnly = context.devOnly,
                isEnabled = context.isEnabled,
                readsFormalSaveData = context.readsFormalSaveData,
                writesFormalFlow = context.writesFormalFlow,
                buildTags = CollectCapabilityTokens(context).OrderBy(value => value, StringComparer.Ordinal).ToList(),
                activeSynergies = buildEvaluation.activeSynergies?
                    .Select(synergy => synergy.synergyId)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToList() ?? new List<string>(),
                activeThresholds = Clean(buildEvaluation.activeThresholds)
            };
        }

        private static SynergyViewModel BuildSynergy(BuildEvaluationResult result)
        {
            BuildEvaluationResult safeResult = result ?? new BuildEvaluationResult();
            SynergyViewModel model = new()
            {
                activeSynergyCount = safeResult.activeSynergies?.Count ?? 0,
                activeThresholdCount = safeResult.activeThresholds?.Count ?? 0,
                placementSatisfied = safeResult.placementSatisfied,
                energySatisfied = safeResult.energySatisfied,
                missingRequirementCount = safeResult.missingRequirements?.Count ?? 0,
                nextThresholdHint = FormatNextThresholdHint(safeResult.nextThresholdHint),
                rows = (safeResult.activeSynergies ?? new List<ActiveSynergyResult>())
                    .Where(synergy => synergy != null)
                    .Select(synergy => new SynergyPreviewRow
                    {
                        synergyId = synergy.synergyId ?? string.Empty,
                        displayName = synergy.displayName ?? string.Empty,
                        matchedCount = synergy.matchedCount,
                        activeThresholds = Clean(synergy.activeThresholds),
                        sourceItems = Clean(synergy.sourceItems),
                        placementSatisfied = synergy.placementSatisfied,
                        energySatisfied = synergy.energySatisfied
                    })
                    .ToList(),
                missingRequirementSummaries = (safeResult.missingRequirements ?? new List<SynergyRequirementResult>())
                    .Where(requirement => requirement != null)
                    .Select(requirement =>
                        $"{requirement.synergyId}:{requirement.requirementType} {requirement.actualCount}/{requirement.requiredCount} {requirement.detail}")
                    .ToList()
            };
            return model;
        }

        private static ShapeOccupancyViewModel BuildShapeOccupancy(
            BuildSandboxLayoutSnapshot snapshot,
            IReadOnlyList<ShapePlacementResult> shapePlacementResults)
        {
            IReadOnlyList<ShapePlacementResult> placements =
                shapePlacementResults ?? Array.Empty<ShapePlacementResult>();
            int invalidCount = placements.Count(result => result != null && !result.IsValid);
            bool snapshotLegal = snapshot?.placedItems?.All(item =>
                item != null && item.occupiedCells != null && item.occupiedCells.Count > 0) ?? true;
            return new ShapeOccupancyViewModel
            {
                placedItemCount = snapshot?.placedItems?.Count ?? 0,
                occupiedCellCount = CountOccupiedCells(snapshot),
                placementSampleCount = placements.Count,
                validPlacementCount = placements.Count(result => result != null && result.IsValid),
                invalidPlacementCount = invalidCount,
                placementLegal = snapshotLegal,
                invalidReasons = placements
                    .Where(result => result != null && !result.IsValid)
                    .Select(result => $"{result.ItemId}:{result.InvalidReason}")
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToList(),
                placedItemIds = Clean(snapshot?.placedItems?.Select(item => item?.itemId))
            };
        }

        private static AffixModifierViewModel BuildAffixModifier(
            AffixRarityEvaluationResult affixEvaluation,
            CombatModifierBundle modifierBundle,
            EffectEventBundle eventBundle)
        {
            AffixRarityEvaluationResult safeAffix = affixEvaluation ?? new AffixRarityEvaluationResult();
            CombatModifierBundle safeModifier = modifierBundle ?? new CombatModifierBundle();
            EffectEventBundle safeEvent = eventBundle ?? new EffectEventBundle();
            List<string> affixIds = Clean(safeAffix.affixIds);
            return new AffixModifierViewModel
            {
                affixItemCount = safeAffix.itemResults?.Count ?? 0,
                rarityCount = safeAffix.rarityIds?.Count ?? 0,
                selectedAffixCount = affixIds.Count,
                orangeCoreAffixPresent = affixIds.Any(value =>
                    ContainsAny(value, "orange_core", "orange", "core")),
                bondPlusOnePresent = affixIds.Any(value => ContainsAny(value, "bond_plus_one", "bond")),
                modifierCount = safeModifier.modifiers?.Count ?? 0,
                eventCount = safeEvent.events?.Count ?? 0,
                modifierPreviewDevOnly = safeModifier.devOnly
                    && (safeModifier.modifiers ?? new List<BuildModifierPreview>()).All(item => item.devOnly),
                modifierPreviewEnabled = safeModifier.isEnabled
                    || (safeModifier.modifiers ?? new List<BuildModifierPreview>()).Any(item => item.isEnabled),
                eventPreviewDevOnly = safeEvent.devOnly
                    && (safeEvent.events ?? new List<BuildEventPreview>()).All(item => item.devOnly),
                eventPreviewEnabled = safeEvent.isEnabled
                    || (safeEvent.events ?? new List<BuildEventPreview>()).Any(item => item.isEnabled),
                affectsFormalCombat = safeModifier.affectsFormalCombat
                    || safeEvent.affectsFormalCombat
                    || (safeModifier.modifiers ?? new List<BuildModifierPreview>()).Any(item => item.affectsFormalCombat)
                    || (safeEvent.events ?? new List<BuildEventPreview>()).Any(item => item.affectsFormalCombat),
                rarityIds = Clean(safeAffix.rarityIds),
                affixIds = affixIds,
                modifierTypes = Clean(safeModifier.modifiers?.Select(item => item?.modifierType)),
                eventTypes = Clean(safeEvent.events?.Select(item => item?.eventType))
            };
        }

        private static ProblemSelectorViewModel BuildProblemSelector(BuildProblemSeedDataset dataset)
        {
            BuildProblemSeedDataset safeDataset = dataset ?? BuildProblemSeedDataset.CreateDefault();
            return new ProblemSelectorViewModel
            {
                mapRuleCount = safeDataset.mapRules?.Count ?? 0,
                enemyProblemCount = safeDataset.enemyProblems?.Count ?? 0,
                bossProblemCount = safeDataset.bossProblems?.Count ?? 0,
                mapRuleOptions = (safeDataset.mapRules ?? new List<MapRuleSeed>())
                    .Select(rule => new ProblemSelectorOption
                    {
                        id = rule.mapRuleId ?? string.Empty,
                        displayName = rule.displayName ?? string.Empty,
                        summary = rule.warningText ?? string.Empty,
                        devOnly = rule.devOnly,
                        isEnabled = rule.isEnabled
                    })
                    .ToList(),
                enemyProblemOptions = (safeDataset.enemyProblems ?? new List<EnemyProblemSeed>())
                    .Select(problem => new ProblemSelectorOption
                    {
                        id = problem.problemType ?? string.Empty,
                        displayName = problem.displayName ?? string.Empty,
                        summary = problem.recommendedAction ?? string.Empty,
                        devOnly = problem.devOnly,
                        isEnabled = problem.isEnabled
                    })
                    .ToList(),
                bossProblemOptions = (safeDataset.bossProblems ?? new List<BossProblemSeed>())
                    .Select(boss => new ProblemSelectorOption
                    {
                        id = boss.bossProblemId ?? string.Empty,
                        displayName = boss.displayName ?? string.Empty,
                        summary = boss.validationGoal ?? string.Empty,
                        devOnly = boss.devOnly,
                        isEnabled = boss.isEnabled
                    })
                    .ToList()
            };
        }

        private static ProblemReadinessViewModel BuildProblemReadiness(
            BuildSandboxPreviewContext context,
            IReadOnlyList<LedgerBuildTaskProgressPreview> ledgerProgress)
        {
            BuildProblemSeedDataset dataset = context.problemSeedDataset ?? BuildProblemSeedDataset.CreateDefault();
            HashSet<string> tokens = CollectCapabilityTokens(context);
            Dictionary<string, int> attributeScores = BuildAttributeScores(tokens, context, ledgerProgress);

            List<BossReadinessViewModel> rows = new();
            foreach (BossProblemSeed boss in dataset.bossProblems ?? new List<BossProblemSeed>())
            {
                BossReadinessViewModel row = BuildBossReadiness(boss, tokens, attributeScores);
                rows.Add(row);
            }

            return new ProblemReadinessViewModel
            {
                bossReadinessCount = rows.Count,
                readyBossCount = rows.Count(row => row.ready),
                totalKeyCount = rows.Sum(row => row.keyCount),
                satisfiedKeyCount = rows.Sum(row => row.satisfiedKeyCount),
                failureReasonCount = rows.Sum(row => row.failureReasons.Count),
                recommendedActionCount = rows.Sum(row => row.recommendedActions.Count),
                dropBiasCount = rows.Sum(row => row.dropBiasIds.Count),
                bossRows = rows
            };
        }

        private static BossReadinessViewModel BuildBossReadiness(
            BossProblemSeed boss,
            HashSet<string> tokens,
            IReadOnlyDictionary<string, int> attributeScores)
        {
            BossReadinessViewModel row = new()
            {
                bossProblemId = boss?.bossProblemId ?? string.Empty,
                displayName = boss?.displayName ?? string.Empty,
                validationGoal = boss?.validationGoal ?? string.Empty,
                minimumKeysRequired = Math.Max(1, boss?.minimumKeysRequired ?? 1),
                keyCount = boss?.keyRequirements?.Count ?? 0,
                dropBiasIds = Clean(boss?.dropBiases?.Select(drop => drop?.dropBiasId)),
                weaknessWindowIds = Clean(boss?.weaknessWindows?.Select(window => window?.weaknessWindowId))
            };

            foreach (BossProblemKeySeed key in boss?.keyRequirements ?? new List<BossProblemKeySeed>())
            {
                int observedScore = ResolveObservedScore(key, tokens, attributeScores);
                BossReadinessKeyViewModel keyRow = new()
                {
                    keyId = key.keyId ?? string.Empty,
                    keyCategory = key.keyCategory ?? string.Empty,
                    requirementId = key.requirementId ?? string.Empty,
                    problemAttribute = key.problemAttribute ?? string.Empty,
                    requiredScore = Math.Max(1, key.requiredScore),
                    observedScore = observedScore,
                    satisfied = observedScore >= Math.Max(1, key.requiredScore),
                    hint = key.hint ?? string.Empty
                };
                row.keys.Add(keyRow);
            }

            row.satisfiedKeyCount = row.keys.Count(key => key.satisfied);
            row.ready = row.satisfiedKeyCount >= row.minimumKeysRequired;
            if (!row.ready)
            {
                row.failureReasons = (boss?.failureHints ?? new List<FailureHintSeed>())
                    .Select(hint => string.IsNullOrWhiteSpace(hint.headline)
                        ? hint.detail
                        : $"{hint.headline}: {hint.detail}")
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .ToList();
                row.recommendedActions = row.keys
                    .Where(key => !key.satisfied)
                    .Select(key => key.hint)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.Ordinal)
                    .ToList();
            }

            return row;
        }

        private static SimulationResultViewModel BuildSimulationResult(BuildSimulationBenchmarkReport report)
        {
            BuildSimulationBenchmarkReport safeReport = report ?? new BuildSimulationBenchmarkReport();
            List<BuildSimulationResult> results = (safeReport.results ?? new List<BuildSimulationResult>())
                .Where(result => result != null)
                .ToList();
            BuildSimulationResult best = results
                .OrderByDescending(result => result.simulatedWinRate)
                .ThenBy(result => result.simulatedClearTimeSeconds)
                .FirstOrDefault();

            return new SimulationResultViewModel
            {
                scenarioCount = safeReport.scenarios?.Count ?? 0,
                resultCount = results.Count,
                comparisonCount = safeReport.comparisons?.Count ?? 0,
                averageWinRate = results.Count == 0 ? 0f : Round(results.Average(result => result.simulatedWinRate)),
                averageClearTimeSeconds = results.Count == 0
                    ? 0f
                    : Round(results.Average(result => result.simulatedClearTimeSeconds)),
                bestBuildId = best?.buildId ?? string.Empty,
                rows = results.Select(result => new SimulationResultRow
                    {
                        buildId = result.buildId ?? string.Empty,
                        buildName = result.buildName ?? string.Empty,
                        winRate = result.simulatedWinRate,
                        clearTimeSeconds = result.simulatedClearTimeSeconds,
                        shieldBreakEfficiency = result.shieldBreakEfficiency,
                        triggeredEventCount = result.triggeredEventCount,
                        failureReason = result.failureReason ?? string.Empty,
                        devOnly = result.devOnly,
                        isEnabled = result.isEnabled,
                        affectsFormalCombat = result.affectsFormalCombat
                    })
                    .ToList()
            };
        }

        private static HashSet<string> CollectCapabilityTokens(BuildSandboxPreviewContext context)
        {
            HashSet<string> tokens = new(StringComparer.OrdinalIgnoreCase);
            AddTokens(tokens, context.layoutSnapshot?.placedItems?.SelectMany(item => item?.tags ?? new List<string>()));
            AddTokens(tokens, context.layoutSnapshot?.placedItems?.Select(item => item?.itemId));
            AddTokens(tokens, context.buildEvaluation?.activeSynergies?.Select(item => item?.synergyId));
            AddTokens(tokens, context.buildEvaluation?.activeThresholds);
            AddTokens(tokens, context.buildEvaluation?.sourceItems);
            AddTokens(tokens, context.affixEvaluation?.affixIds);
            AddTokens(tokens, context.affixEvaluation?.rarityIds);
            AddTokens(tokens, context.affixEvaluation?.itemResults?.Select(item => item?.itemId));
            AddTokens(tokens, context.affixEvaluation?.itemResults?.SelectMany(item => item?.sourceTags ?? new List<string>()));
            AddTokens(tokens, context.modifierBundle?.modifiers?.Select(item => item?.modifierType));
            AddTokens(tokens, context.modifierBundle?.modifiers?.Select(item => item?.sourceAffix));
            AddTokens(tokens, context.modifierBundle?.modifiers?.Select(item => item?.sourceSynergy));
            AddTokens(tokens, context.modifierBundle?.modifiers?.Select(item => item?.sourceItem));
            AddTokens(tokens, context.eventBundle?.events?.Select(item => item?.eventType));
            AddTokens(tokens, context.eventBundle?.events?.Select(item => item?.sourceAffix));
            AddTokens(tokens, context.eventBundle?.events?.Select(item => item?.sourceSynergy));
            AddTokens(tokens, context.simulationReport?.results?.SelectMany(item => item?.activeSynergies ?? new List<string>()));
            AddTokens(tokens, context.simulationReport?.results?.SelectMany(item => item?.activeThresholds ?? new List<string>()));
            AddTokens(tokens, context.simulationReport?.results?.SelectMany(item => item?.validationTags ?? new List<string>()));
            AddTokens(tokens, context.simulationReport?.results?.SelectMany(item => item?.recommendedSynergies ?? new List<string>()));
            return tokens;
        }

        private static Dictionary<string, int> BuildAttributeScores(
            HashSet<string> tokens,
            BuildSandboxPreviewContext context,
            IReadOnlyList<LedgerBuildTaskProgressPreview> ledgerProgress)
        {
            Dictionary<string, int> scores = new(StringComparer.Ordinal)
            {
                ["BreakPower"] = Score(tokens, "break", "shieldBreak", "shield_break", "thunder", "jing_lei", "惊雷"),
                ["CleansePower"] = Score(tokens, "cleanse", "purify", "purifying", "jing_e", "净厄"),
                ["ControlPower"] = Score(tokens, "control", "interrupt", "zhen_hun", "镇魂"),
                ["GuardPower"] = Score(tokens, "guard", "ward", "shieldBonus", "hu_zhen", "护阵"),
                ["EnergyStability"] = Score(tokens, "energy", "powered", "ju_neng", "聚能"),
                ["ClearPower"] = Score(tokens, "clear", "fire", "lihuo", "li_huo", "离火"),
                ["BurstWindow"] = Score(tokens, "burst", "orange", "core", "cooldown"),
                ["PlacementShape"] = Score(tokens, "shape", "placement", "formation", "anchor")
            };

            if (context.buildEvaluation?.placementSatisfied == true)
            {
                scores["PlacementShape"] = Math.Max(scores["PlacementShape"], 2);
            }

            if (context.buildEvaluation?.energySatisfied == true)
            {
                scores["EnergyStability"] = Math.Max(scores["EnergyStability"], 2);
            }

            if ((ledgerProgress ?? Array.Empty<LedgerBuildTaskProgressPreview>()).Any(progress => progress.completed))
            {
                scores["BurstWindow"] = Math.Max(scores["BurstWindow"], 1);
            }

            return scores;
        }

        private static int ResolveObservedScore(
            BossProblemKeySeed key,
            HashSet<string> tokens,
            IReadOnlyDictionary<string, int> attributeScores)
        {
            if (key == null)
            {
                return 0;
            }

            int requiredScore = Math.Max(1, key.requiredScore);
            if (ContainsToken(tokens, key.requirementId) || ContainsToken(tokens, key.keyId))
            {
                return requiredScore;
            }

            if (!string.IsNullOrWhiteSpace(key.problemAttribute)
                && attributeScores.TryGetValue(key.problemAttribute, out int attributeScore))
            {
                return Math.Min(requiredScore, attributeScore);
            }

            return 0;
        }

        private static int CountOccupiedCells(BuildSandboxLayoutSnapshot snapshot)
        {
            return snapshot?.placedItems?
                .Where(item => item != null)
                .Sum(item => item.occupiedCells?.Count ?? 0) ?? 0;
        }

        private static string FormatNextThresholdHint(NextThresholdHint hint)
        {
            if (hint == null || !hint.hasNextThreshold)
            {
                return "none";
            }

            return $"{hint.synergyId}:{hint.currentPieceCount}->{hint.nextPieceCount} missing={hint.missingPieceCount}";
        }

        private static void AddTokens(HashSet<string> target, IEnumerable<string> values)
        {
            foreach (string value in values ?? Enumerable.Empty<string>())
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    target.Add(value.Trim());
                }
            }
        }

        private static bool ContainsToken(IEnumerable<string> tokens, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return (tokens ?? Enumerable.Empty<string>()).Any(token => ContainsAny(token, value));
        }

        private static int Score(HashSet<string> tokens, params string[] markers)
        {
            int count = markers.Count(marker => ContainsToken(tokens, marker));
            return Math.Min(3, count);
        }

        private static bool ContainsAny(string value, params string[] tokens)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return tokens
                .Where(token => !string.IsNullOrWhiteSpace(token))
                .Any(token => value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static List<string> Clean(IEnumerable<string> values)
        {
            return (values ?? Enumerable.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToList();
        }

        private static float Round(double value)
        {
            return (float)Math.Round(value, 4);
        }
    }
}
