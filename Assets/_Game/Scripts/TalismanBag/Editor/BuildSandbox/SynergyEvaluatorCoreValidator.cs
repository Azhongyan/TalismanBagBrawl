#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BuildSandbox;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class SynergyEvaluatorCoreValidator
    {
        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report = new("Synergy Evaluator Core");
            IReadOnlyList<SynergyConfig> synergies = BuildSandboxConfigValidator.CollectSynergyConfigs();
            if (synergies.Count == 0)
            {
                report.AddError(
                    "SYNERGY_CONFIG_MISSING",
                    "SynergyEvaluatorCore01 requires SynergyConfig seed assets.",
                    BuildSandboxConfigValidator.ConfigRoot);
                return report;
            }

            BuildSandboxLayoutSnapshot snapshot = BuildSeedSnapshot(synergies);
            BuildEvaluationResult result = SynergyEvaluator.Evaluate(snapshot, synergies);
            ValidateGeneratedResult(report, snapshot, result);
            ValidateThresholdEightSupport(report, synergies);
            ValidateMissingRequirementOutput(report, synergies);
            return report;
        }

        public static BuildEvaluationResult BuildSampleEvaluation(out BuildSandboxLayoutSnapshot snapshot)
        {
            IReadOnlyList<SynergyConfig> synergies = BuildSandboxConfigValidator.CollectSynergyConfigs();
            snapshot = BuildSeedSnapshot(synergies);
            return SynergyEvaluator.Evaluate(snapshot, synergies);
        }

        public static BuildSandboxLayoutSnapshot BuildSeedSnapshot(IReadOnlyList<SynergyConfig> synergies)
        {
            BuildSandboxLayoutSnapshot snapshot = new();
            SynergyConfig lihuotag = FindSynergy(synergies, "lihuo");
            SynergyConfig guardian = FindSynergy(synergies, "guardian");
            SynergyConfig corruption = FindSynergy(synergies, "corruption");

            List<string> lihuotags = BuildTags(lihuotag, includeEnergyProviderTags: true);
            List<string> guardTags = BuildTags(guardian, includeEnergyProviderTags: false);
            List<string> corruptionTags = BuildTags(corruption, includeEnergyProviderTags: false);

            for (int i = 0; i < 4; i++)
            {
                snapshot.placedItems.Add(CreateItem(
                    $"seed_lihuo_{i + 1}",
                    "Single1",
                    new ItemShapeCell(i, 0),
                    lihuotags,
                    isPowered: true,
                    energySourceId: "seed_shared_lihuo_source"));
            }

            snapshot.placedItems.Add(CreateItem(
                "seed_guardian_1",
                "Single1",
                new ItemShapeCell(0, 1),
                guardTags,
                isPowered: true,
                energySourceId: "seed_lihuo_1"));

            snapshot.placedItems.Add(CreateItem(
                "seed_guardian_2",
                "Single1",
                new ItemShapeCell(1, 1),
                guardTags,
                isPowered: true,
                energySourceId: "seed_lihuo_1"));

            snapshot.placedItems.Add(CreateItem(
                "seed_corruption_1",
                "Single1",
                new ItemShapeCell(6, 0),
                corruptionTags,
                isPowered: false,
                energySourceId: string.Empty));

            snapshot.placedItems.Add(CreateItem(
                "seed_corruption_2",
                "Single1",
                new ItemShapeCell(7, 1),
                corruptionTags,
                isPowered: false,
                energySourceId: string.Empty));

            return snapshot;
        }

        private static void ValidateGeneratedResult(
            BuildSandboxValidationReport report,
            BuildSandboxLayoutSnapshot snapshot,
            BuildEvaluationResult result)
        {
            if (snapshot.placedItems.Count == 0)
            {
                report.AddError(
                    "SEED_SNAPSHOT_EMPTY",
                    "SynergyEvaluatorCore01 seed snapshot must contain placed items.",
                    BuildSandboxConfigValidator.ConfigRoot);
                return;
            }

            if (result == null)
            {
                report.AddError(
                    "BUILDEVALUATION_RESULT_NULL",
                    "SynergyEvaluator returned null.",
                    BuildSandboxConfigValidator.ConfigRoot);
                return;
            }

            if (result.activeSynergies.Count == 0)
            {
                report.AddError(
                    "ACTIVE_SYNERGY_MISSING",
                    "Seed snapshot should activate at least one devOnly synergy.",
                    BuildSandboxConfigValidator.ConfigRoot);
            }
            else
            {
                report.AddInfo(
                    "ACTIVE_SYNERGY_PRESENT",
                    $"Active synergies={result.activeSynergies.Count}, activeThresholds={result.activeThresholds.Count}.",
                    BuildSandboxConfigValidator.ConfigRoot);
            }

            RequireThreshold(report, result, 2);
            RequireThreshold(report, result, 4);

            if (!result.placementSatisfied)
            {
                report.AddError(
                    "PLACEMENT_REQUIREMENT_UNSATISFIED",
                    "Seed snapshot should satisfy adjacency, separation, and row placement conditions.",
                    BuildSandboxConfigValidator.ConfigRoot);
            }
            else
            {
                report.AddInfo(
                    "PLACEMENT_REQUIREMENT_SATISFIED",
                    "Seed snapshot satisfies placement requirements.",
                    BuildSandboxConfigValidator.ConfigRoot);
            }

            if (!result.energySatisfied)
            {
                report.AddError(
                    "ENERGY_REQUIREMENT_UNSATISFIED",
                    "Seed snapshot should satisfy powered and same-source energy requirements.",
                    BuildSandboxConfigValidator.ConfigRoot);
            }
            else
            {
                report.AddInfo(
                    "ENERGY_REQUIREMENT_SATISFIED",
                    "Seed snapshot satisfies energy requirements.",
                    BuildSandboxConfigValidator.ConfigRoot);
            }

            if (result.nextThresholdHint == null || !result.nextThresholdHint.hasNextThreshold)
            {
                report.AddError(
                    "NEXT_THRESHOLD_HINT_MISSING",
                    "Seed snapshot should produce a nextThresholdHint.",
                    BuildSandboxConfigValidator.ConfigRoot);
            }
            else
            {
                report.AddInfo(
                    "NEXT_THRESHOLD_HINT_PRESENT",
                    $"{result.nextThresholdHint.synergyId} needs {result.nextThresholdHint.missingPieceCount} item(s) for {result.nextThresholdHint.nextPieceCount}.",
                    BuildSandboxConfigValidator.ConfigRoot);
            }
        }

        private static void ValidateThresholdEightSupport(
            BuildSandboxValidationReport report,
            IReadOnlyList<SynergyConfig> synergies)
        {
            SynergyConfig lihuotag = FindSynergy(synergies, "lihuo");
            if (lihuotag == null)
            {
                report.AddWarning(
                    "THRESHOLD_8_SAMPLE_SKIPPED",
                    "No lihuotag synergy seed was found for the 8-piece synthetic sample.",
                    BuildSandboxConfigValidator.ConfigRoot);
                return;
            }

            BuildSandboxLayoutSnapshot snapshot = new();
            List<string> tags = BuildTags(lihuotag, includeEnergyProviderTags: true);
            for (int i = 0; i < 8; i++)
            {
                snapshot.placedItems.Add(CreateItem(
                    $"seed_lihuo_8piece_{i + 1}",
                    "Single1",
                    new ItemShapeCell(i, 0),
                    tags,
                    isPowered: true,
                    energySourceId: "seed_lihuo_8piece_shared_source"));
            }

            BuildEvaluationResult result = SynergyEvaluator.Evaluate(snapshot, new[] { lihuotag });
            string thresholdKey = $"{lihuotag.synergyId}:8";
            if (!result.activeThresholds.Contains(thresholdKey, StringComparer.Ordinal))
            {
                report.AddError(
                    "THRESHOLD_8_NOT_ACTIVATED",
                    "Synthetic 8-piece snapshot did not activate the 8-piece threshold.",
                    BuildSandboxConfigValidator.ConfigRoot);
                return;
            }

            report.AddInfo(
                "THRESHOLD_8_ACTIVATED",
                "Synthetic 8-piece snapshot activates the 8-piece threshold.",
                BuildSandboxConfigValidator.ConfigRoot);
        }

        private static void ValidateMissingRequirementOutput(
            BuildSandboxValidationReport report,
            IReadOnlyList<SynergyConfig> synergies)
        {
            SynergyConfig lihuotag = FindSynergy(synergies, "lihuo");
            if (lihuotag == null)
            {
                report.AddWarning(
                    "MISSING_REQUIREMENT_SAMPLE_SKIPPED",
                    "No lihuotag synergy seed was found for missing requirement validation.",
                    BuildSandboxConfigValidator.ConfigRoot);
                return;
            }

            BuildSandboxLayoutSnapshot snapshot = new();
            snapshot.placedItems.Add(CreateItem(
                "seed_partial_lihuo_1",
                "Single1",
                new ItemShapeCell(0, 0),
                BuildTags(lihuotag, includeEnergyProviderTags: true),
                isPowered: false,
                energySourceId: string.Empty));

            BuildEvaluationResult result = SynergyEvaluator.Evaluate(snapshot, new[] { lihuotag });
            if (result.missingRequirements.Count == 0)
            {
                report.AddError(
                    "MISSING_REQUIREMENTS_NOT_REPORTED",
                    "Partial snapshot should output missingRequirements.",
                    BuildSandboxConfigValidator.ConfigRoot);
                return;
            }

            report.AddInfo(
                "MISSING_REQUIREMENTS_REPORTED",
                $"Partial snapshot missingRequirements={result.missingRequirements.Count}.",
                BuildSandboxConfigValidator.ConfigRoot);
        }

        private static void RequireThreshold(
            BuildSandboxValidationReport report,
            BuildEvaluationResult result,
            int threshold)
        {
            if (result.activeThresholds.Any(value => value.EndsWith($":{threshold}", StringComparison.Ordinal)))
            {
                report.AddInfo(
                    "THRESHOLD_ACTIVATED",
                    $"Seed snapshot activates a {threshold}-piece threshold.",
                    BuildSandboxConfigValidator.ConfigRoot);
                return;
            }

            report.AddError(
                "THRESHOLD_NOT_ACTIVATED",
                $"Seed snapshot should activate a {threshold}-piece threshold.",
                BuildSandboxConfigValidator.ConfigRoot);
        }

        private static SynergyConfig FindSynergy(IReadOnlyList<SynergyConfig> synergies, string token)
        {
            return synergies.FirstOrDefault(synergy =>
                synergy != null
                && !string.IsNullOrWhiteSpace(synergy.synergyId)
                && synergy.synergyId.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static List<string> BuildTags(SynergyConfig synergy, bool includeEnergyProviderTags)
        {
            HashSet<string> tags = new(StringComparer.Ordinal);
            foreach (string tag in synergy?.requiredTags ?? Enumerable.Empty<string>())
            {
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    tags.Add(tag);
                }
            }

            if (includeEnergyProviderTags)
            {
                foreach (EnergyConditionConfig condition in synergy?.energyConditions ?? Enumerable.Empty<EnergyConditionConfig>())
                {
                    if (!string.IsNullOrWhiteSpace(condition.requiredProviderTag))
                    {
                        tags.Add(condition.requiredProviderTag);
                    }
                }
            }

            return tags.OrderBy(tag => tag, StringComparer.Ordinal).ToList();
        }

        private static BuildSandboxPlacedItemSnapshot CreateItem(
            string itemId,
            string shapeId,
            ItemShapeCell cell,
            IEnumerable<string> tags,
            bool isPowered,
            string energySourceId)
        {
            return new BuildSandboxPlacedItemSnapshot
            {
                itemId = itemId,
                shapeId = shapeId,
                anchorCell = cell,
                occupiedCells = new List<ItemShapeCell> { cell },
                rotation = ItemShapeRotation.Rotation0,
                tags = (tags ?? Enumerable.Empty<string>())
                    .Where(tag => !string.IsNullOrWhiteSpace(tag))
                    .Distinct(StringComparer.Ordinal)
                    .ToList(),
                isPowered = isPowered,
                energySourceId = energySourceId ?? string.Empty,
                affixList = new List<string>(),
                rarity = "sandbox_seed"
            };
        }
    }
}
#endif
