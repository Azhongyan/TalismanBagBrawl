using System;
using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.BuildSandbox
{
    [Serializable]
    public sealed class BuildSimulationScenario
    {
        public string buildId = "buildsimulation_sandbox_scenario";
        public string displayName = "BuildSimulation Sandbox Scenario";
        public bool devOnly = true;
        public bool isEnabled;
        public string enemyType = "devOnly_training_dummy";
        public string bossMechanic = "none";
        public string itemRarity = "sandbox_mixed";
        public List<string> buildItemIds = new();
        public List<string> affixCombination = new();
        public string energyCondition = "sandbox_powered";
        public string placementRelation = "sandbox_sample_layout";
        public string enemyProfileId = string.Empty;
        public string enemyChineseRole = string.Empty;
        public string bossProfileId = string.Empty;
        public string bossChineseRole = string.Empty;
        public List<string> validationTags = new();
        public List<string> recommendedSynergies = new();
        public bool simulatorReadable = true;
        public bool entersFormalFlow;
        public int batchSeed;
        public BuildSandboxLayoutSnapshot layoutSnapshot = new();
        public BuildEvaluationResult buildEvaluation = new();
        public AffixRarityEvaluationResult affixEvaluation = new();
        public CombatModifierBundle modifierBundle = new();
        public EffectEventBundle eventBundle = new();
        public bool referencesFormalEnemyPool;
        public bool referencesFormalBossPool;
        public bool readsFormalPlayerData;
        public string notes = "sandbox estimate; devOnly; disabled; not formal combat data.";
    }

    [Serializable]
    public sealed class BuildSimulationResult
    {
        public string buildId = string.Empty;
        public string buildName = string.Empty;
        public string enemyType = string.Empty;
        public string bossMechanic = string.Empty;
        public string enemyProfileId = string.Empty;
        public string enemyChineseRole = string.Empty;
        public string bossProfileId = string.Empty;
        public string bossChineseRole = string.Empty;
        public List<string> validationTags = new();
        public List<string> recommendedSynergies = new();
        public List<string> activeSynergies = new();
        public List<string> activeThresholds = new();
        public int synergyLevel;
        public float simulatedClearTimeSeconds;
        public float simulatedWinRate;
        public float simulatedRemainingHpPercent;
        public float simulatedTotalDamage;
        public float shieldBreakEfficiency;
        public float energyCoverage;
        public int triggeredEventCount;
        public string failureReason = "none";
        public bool numericAnomaly;
        public bool sandboxEstimate = true;
        public bool devOnly = true;
        public bool isEnabled;
        public bool combatModifierPreviewUsed;
        public bool effectEventPreviewUsed;
        public bool affectsFormalCombat;
        public bool simulatorReadable = true;
        public bool entersFormalFlow;
    }

    [Serializable]
    public sealed class BuildComparisonResult
    {
        public string comparisonName = string.Empty;
        public string baselineBuildId = string.Empty;
        public string challengerBuildId = string.Empty;
        public float clearTimeDeltaSeconds;
        public float winRateDelta;
        public float remainingHpDelta;
        public float totalDamageDelta;
        public float shieldBreakEfficiencyDelta;
        public int triggeredEventDelta;
        public string comparedDimension = string.Empty;
        public bool sandboxEstimate = true;
        public bool devOnly = true;
        public bool isEnabled;
    }

    [Serializable]
    public sealed class BuildSimulationBenchmarkReport
    {
        public string packageName = "V0.3/V0.4-BuildSandbox-BuildSimulationBenchmark01";
        public bool sandboxEstimate = true;
        public bool devOnly = true;
        public bool isEnabled;
        public List<BuildSimulationScenario> scenarios = new();
        public List<BuildSimulationResult> results = new();
        public List<BuildComparisonResult> comparisons = new();
    }

    public static class BuildSimulationRunner
    {
        public static BuildSimulationResult RunSingle(BuildSimulationScenario scenario)
        {
            BuildSimulationScenario safeScenario = scenario ?? new BuildSimulationScenario();
            BuildEvaluationResult buildEvaluation = safeScenario.buildEvaluation ?? new BuildEvaluationResult();
            AffixRarityEvaluationResult affixEvaluation = safeScenario.affixEvaluation ?? new AffixRarityEvaluationResult();
            CombatModifierBundle modifierBundle = safeScenario.modifierBundle ?? new CombatModifierBundle();
            EffectEventBundle eventBundle = safeScenario.eventBundle ?? new EffectEventBundle();

            int synergyLevel = ExtractMaxThreshold(buildEvaluation);
            float modifierPower = SumModifierPower(modifierBundle);
            int affixPower = SumAffixPower(affixEvaluation);
            float energyCoverage = CalculateEnergyCoverage(safeScenario.layoutSnapshot);
            int eventCount = eventBundle.events?.Count ?? 0;
            float bossPenalty = ResolveBossPenalty(safeScenario.bossMechanic);
            float enemyPenalty = ResolveEnemyPenalty(safeScenario.enemyType);
            float validationPressure = ResolveValidationPressure(safeScenario.validationTags);

            float clearTime = Clamp(
                300f
                - synergyLevel * 8f
                - modifierPower * 18f
                - affixPower * 0.08f
                - eventCount * 1.4f
                - energyCoverage * 16f
                + bossPenalty * 70f
                + enemyPenalty * 30f,
                90f,
                480f);

            float winRate = Clamp01(
                0.42f
                + synergyLevel * 0.035f
                + modifierPower * 0.035f
                + affixPower * 0.0012f
                + eventCount * 0.005f
                + energyCoverage * 0.08f
                - bossPenalty
                - enemyPenalty
                - validationPressure);

            float remainingHp = Clamp01(
                0.10f
                + winRate * 0.54f
                + SumModifierType(modifierBundle, ModifierEventBridge.ShieldBonus) * 0.035f
                + CountEventType(eventBundle, ModifierEventBridge.OnLowHp) * 0.01f);

            float totalDamage = (float)Math.Round(
                850f
                + synergyLevel * 95f
                + modifierPower * 300f
                + affixPower * 1.35f
                + eventCount * 18f,
                2);

            float shieldBreak = Clamp01(
                0.18f
                + synergyLevel * 0.015f
                + SumModifierType(modifierBundle, ModifierEventBridge.ShieldBreakBonus) * 0.12f
                + CountEventType(eventBundle, ModifierEventBridge.OnShieldBreak) * 0.025f);

            BuildSimulationResult result = new()
            {
                buildId = safeScenario.buildId ?? string.Empty,
                buildName = safeScenario.displayName ?? string.Empty,
                enemyType = safeScenario.enemyType ?? string.Empty,
                bossMechanic = safeScenario.bossMechanic ?? string.Empty,
                enemyProfileId = safeScenario.enemyProfileId ?? string.Empty,
                enemyChineseRole = safeScenario.enemyChineseRole ?? string.Empty,
                bossProfileId = safeScenario.bossProfileId ?? string.Empty,
                bossChineseRole = safeScenario.bossChineseRole ?? string.Empty,
                validationTags = safeScenario.validationTags?
                    .Where(tag => !string.IsNullOrWhiteSpace(tag))
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(tag => tag, StringComparer.Ordinal)
                    .ToList() ?? new List<string>(),
                recommendedSynergies = safeScenario.recommendedSynergies?
                    .Where(synergy => !string.IsNullOrWhiteSpace(synergy))
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(synergy => synergy, StringComparer.Ordinal)
                    .ToList() ?? new List<string>(),
                activeSynergies = buildEvaluation.activeSynergies?
                    .Select(synergy => synergy.synergyId)
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(id => id, StringComparer.Ordinal)
                    .ToList() ?? new List<string>(),
                activeThresholds = buildEvaluation.activeThresholds?
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToList() ?? new List<string>(),
                synergyLevel = synergyLevel,
                simulatedClearTimeSeconds = (float)Math.Round(clearTime, 2),
                simulatedWinRate = (float)Math.Round(winRate, 4),
                simulatedRemainingHpPercent = (float)Math.Round(remainingHp, 4),
                simulatedTotalDamage = totalDamage,
                shieldBreakEfficiency = (float)Math.Round(shieldBreak, 4),
                energyCoverage = (float)Math.Round(energyCoverage, 4),
                triggeredEventCount = eventCount,
                failureReason = ResolveFailureReason(safeScenario, winRate),
                devOnly = safeScenario.devOnly,
                isEnabled = safeScenario.isEnabled,
                combatModifierPreviewUsed = modifierBundle.modifiers != null && modifierBundle.modifiers.Count > 0,
                effectEventPreviewUsed = eventBundle.events != null && eventBundle.events.Count > 0,
                affectsFormalCombat = modifierBundle.affectsFormalCombat || eventBundle.affectsFormalCombat,
                simulatorReadable = safeScenario.simulatorReadable,
                entersFormalFlow = safeScenario.entersFormalFlow
            };

            result.numericAnomaly = HasNumericAnomaly(result);
            return result;
        }

        public static BuildSimulationBenchmarkReport RunBatch(IEnumerable<BuildSimulationScenario> scenarios)
        {
            BuildSimulationBenchmarkReport report = new();
            report.scenarios = (scenarios ?? Enumerable.Empty<BuildSimulationScenario>())
                .Where(scenario => scenario != null)
                .ToList();
            report.results = report.scenarios.Select(RunSingle).ToList();

            TryAddComparison(report, "with_vs_without_synergy", "sandbox_no_synergy", "sandbox_full_preview", "synergy");
            TryAddComparison(report, "with_vs_without_affix", "sandbox_no_affix", "sandbox_full_preview", "affix");
            TryAddComparison(report, "training_vs_boss_placeholder", "sandbox_full_preview", "sandbox_boss_placeholder", "boss placeholder");
            return report;
        }

        public static BuildComparisonResult Compare(
            BuildSimulationResult baseline,
            BuildSimulationResult challenger,
            string comparisonName,
            string comparedDimension)
        {
            BuildSimulationResult safeBaseline = baseline ?? new BuildSimulationResult();
            BuildSimulationResult safeChallenger = challenger ?? new BuildSimulationResult();
            return new BuildComparisonResult
            {
                comparisonName = comparisonName ?? string.Empty,
                baselineBuildId = safeBaseline.buildId ?? string.Empty,
                challengerBuildId = safeChallenger.buildId ?? string.Empty,
                clearTimeDeltaSeconds = Round(safeChallenger.simulatedClearTimeSeconds - safeBaseline.simulatedClearTimeSeconds),
                winRateDelta = Round(safeChallenger.simulatedWinRate - safeBaseline.simulatedWinRate),
                remainingHpDelta = Round(safeChallenger.simulatedRemainingHpPercent - safeBaseline.simulatedRemainingHpPercent),
                totalDamageDelta = Round(safeChallenger.simulatedTotalDamage - safeBaseline.simulatedTotalDamage),
                shieldBreakEfficiencyDelta = Round(safeChallenger.shieldBreakEfficiency - safeBaseline.shieldBreakEfficiency),
                triggeredEventDelta = safeChallenger.triggeredEventCount - safeBaseline.triggeredEventCount,
                comparedDimension = comparedDimension ?? string.Empty,
                devOnly = safeBaseline.devOnly && safeChallenger.devOnly,
                isEnabled = safeBaseline.isEnabled || safeChallenger.isEnabled
            };
        }

        private static void TryAddComparison(
            BuildSimulationBenchmarkReport report,
            string comparisonName,
            string baselineBuildId,
            string challengerBuildId,
            string comparedDimension)
        {
            BuildSimulationResult baseline = report.results.FirstOrDefault(result =>
                string.Equals(result.buildId, baselineBuildId, StringComparison.Ordinal));
            BuildSimulationResult challenger = report.results.FirstOrDefault(result =>
                string.Equals(result.buildId, challengerBuildId, StringComparison.Ordinal));
            if (baseline == null || challenger == null)
            {
                return;
            }

            report.comparisons.Add(Compare(baseline, challenger, comparisonName, comparedDimension));
        }

        private static int ExtractMaxThreshold(BuildEvaluationResult result)
        {
            return (result?.activeThresholds ?? Enumerable.Empty<string>())
                .Select(ParseThresholdPieceCount)
                .DefaultIfEmpty(0)
                .Max();
        }

        private static int ParseThresholdPieceCount(string thresholdKey)
        {
            if (string.IsNullOrWhiteSpace(thresholdKey))
            {
                return 0;
            }

            int index = thresholdKey.LastIndexOf(':');
            return index >= 0
                && index < thresholdKey.Length - 1
                && int.TryParse(thresholdKey.Substring(index + 1), out int value)
                ? value
                : 0;
        }

        private static float SumModifierPower(CombatModifierBundle bundle)
        {
            return (bundle?.modifiers ?? Enumerable.Empty<BuildModifierPreview>())
                .Where(modifier => modifier != null)
                .Sum(modifier => Math.Max(0f, modifier.previewValue));
        }

        private static float SumModifierType(CombatModifierBundle bundle, string modifierType)
        {
            return (bundle?.modifiers ?? Enumerable.Empty<BuildModifierPreview>())
                .Where(modifier => modifier != null)
                .Where(modifier => string.Equals(modifier.modifierType, modifierType, StringComparison.Ordinal))
                .Sum(modifier => Math.Max(0f, modifier.previewValue));
        }

        private static int CountEventType(EffectEventBundle bundle, string eventType)
        {
            return (bundle?.events ?? Enumerable.Empty<BuildEventPreview>())
                .Count(eventPreview => eventPreview != null
                    && string.Equals(eventPreview.eventType, eventType, StringComparison.Ordinal));
        }

        private static int SumAffixPower(AffixRarityEvaluationResult result)
        {
            return (result?.itemResults ?? Enumerable.Empty<AffixRarityItemResult>())
                .Where(item => item != null)
                .Sum(item => Math.Max(0, item.totalPreviewPower));
        }

        private static float CalculateEnergyCoverage(BuildSandboxLayoutSnapshot snapshot)
        {
            List<BuildSandboxPlacedItemSnapshot> items = snapshot?.placedItems?
                .Where(item => item != null)
                .ToList() ?? new List<BuildSandboxPlacedItemSnapshot>();
            if (items.Count == 0)
            {
                return 0f;
            }

            return Clamp01(items.Count(item => item.isPowered) / (float)items.Count);
        }

        private static string ResolveFailureReason(BuildSimulationScenario scenario, float winRate)
        {
            if (scenario == null)
            {
                return "scenario missing (sandbox validation)";
            }

            if (scenario.buildItemIds == null || scenario.buildItemIds.Count == 0)
            {
                return "build input empty (sandbox validation)";
            }

            if (!scenario.simulatorReadable)
            {
                return "profile is not simulator readable";
            }

            if (scenario.entersFormalFlow)
            {
                return "profile leaked into formal flow";
            }

            if (winRate < 0.5f)
            {
                return "sandbox estimate low win rate";
            }

            return "none";
        }

        private static bool HasNumericAnomaly(BuildSimulationResult result)
        {
            return result == null
                || IsBad(result.simulatedClearTimeSeconds)
                || IsBad(result.simulatedWinRate)
                || IsBad(result.simulatedRemainingHpPercent)
                || IsBad(result.simulatedTotalDamage)
                || IsBad(result.shieldBreakEfficiency)
                || result.simulatedClearTimeSeconds <= 0f
                || result.simulatedWinRate < 0f
                || result.simulatedWinRate > 1f
                || result.simulatedRemainingHpPercent < 0f
                || result.simulatedRemainingHpPercent > 1f
                || result.shieldBreakEfficiency < 0f
                || result.shieldBreakEfficiency > 1f;
        }

        private static float ResolveValidationPressure(IEnumerable<string> tags)
        {
            int pressureTags = (tags ?? Enumerable.Empty<string>())
                .Count(tag => !string.IsNullOrWhiteSpace(tag)
                    && (tag.IndexOf("burst", StringComparison.OrdinalIgnoreCase) >= 0
                        || tag.IndexOf("boss", StringComparison.OrdinalIgnoreCase) >= 0
                        || tag.IndexOf("disrupt", StringComparison.OrdinalIgnoreCase) >= 0
                        || tag.IndexOf("high_hp", StringComparison.OrdinalIgnoreCase) >= 0));
            return Clamp01(pressureTags * 0.015f);
        }

        private static bool IsBad(float value)
        {
            return float.IsNaN(value) || float.IsInfinity(value);
        }

        private static float ResolveBossPenalty(string bossMechanic)
        {
            if (string.IsNullOrWhiteSpace(bossMechanic) || string.Equals(bossMechanic, "none", StringComparison.OrdinalIgnoreCase))
            {
                return 0f;
            }

            return bossMechanic.IndexOf("boss", StringComparison.OrdinalIgnoreCase) >= 0
                || bossMechanic.IndexOf("shield", StringComparison.OrdinalIgnoreCase) >= 0
                ? 0.08f
                : 0.04f;
        }

        private static float ResolveEnemyPenalty(string enemyType)
        {
            if (string.IsNullOrWhiteSpace(enemyType))
            {
                return 0.03f;
            }

            if (enemyType.IndexOf("boss", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return 0.08f;
            }

            if (enemyType.IndexOf("elite", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return 0.04f;
            }

            return 0f;
        }

        private static float Round(float value)
        {
            return (float)Math.Round(value, 4);
        }

        private static float Clamp01(float value)
        {
            return Clamp(value, 0f, 1f);
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value < min)
            {
                return min;
            }

            return value > max ? max : value;
        }
    }
}
