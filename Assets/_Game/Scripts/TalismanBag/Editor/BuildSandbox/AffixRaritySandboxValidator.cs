#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BuildSandbox;
using UnityEditor;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class AffixRaritySandboxValidator
    {
        private static readonly string[] RequiredRarities = { "white", "green", "blue", "purple", "orange" };

        private static readonly string[] RequiredAffixes =
        {
            "bs_affix_orange_core",
            "bs_affix_bond_plus_one"
        };

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report = new("Affix Rarity Sandbox");
            IReadOnlyList<RarityTierConfig> rarities = BuildSandboxConfigValidator.CollectRarityTierConfigs();
            IReadOnlyList<AffixConfig> affixes = BuildSandboxConfigValidator.CollectAffixConfigs();

            ValidateRarityTiers(report, rarities);
            ValidateAffixes(report, affixes, rarities);

            BuildSandboxLayoutSnapshot snapshot = BuildSeedSnapshot();
            AffixRarityEvaluationResult result = AffixRarityEvaluator.Evaluate(snapshot, rarities, affixes);
            ValidateEvaluation(report, result);
            return report;
        }

        public static AffixRarityEvaluationResult BuildSampleEvaluation(out BuildSandboxLayoutSnapshot snapshot)
        {
            snapshot = BuildSeedSnapshot();
            return AffixRarityEvaluator.Evaluate(
                snapshot,
                BuildSandboxConfigValidator.CollectRarityTierConfigs(),
                BuildSandboxConfigValidator.CollectAffixConfigs());
        }

        public static BuildSandboxLayoutSnapshot BuildSeedSnapshot()
        {
            BuildSandboxLayoutSnapshot snapshot = new();
            snapshot.placedItems.Add(CreateItem(
                "seed_affix_white_damage",
                "Single1",
                new ItemShapeCell(0, 0),
                new[] { "damage_preview", "talisman_preview", "energy_preview" },
                "white",
                new[] { "bs_affix_lihuo_spark" }));
            snapshot.placedItems.Add(CreateItem(
                "seed_affix_green_guardian",
                "Vertical2",
                new ItemShapeCell(1, 0),
                new[] { "defense_preview", "ward_preview", "energy_preview" },
                "green",
                new[] { "bs_affix_guardian_ward", "bs_affix_focus_gather" }));
            snapshot.placedItems.Add(CreateItem(
                "seed_affix_blue_purify",
                "Corner3",
                new ItemShapeCell(3, 0),
                new[] { "cleanse_preview", "control_preview", "energy_preview", "ward_preview" },
                "blue",
                new[] { "bs_affix_purifying_seal", "bs_affix_focus_gather", "bs_affix_guardian_ward" }));
            snapshot.placedItems.Add(CreateItem(
                "seed_affix_purple_combo",
                "Square4",
                new ItemShapeCell(5, 0),
                new[] { "damage_preview", "defense_preview", "energy_preview", "control_preview" },
                "purple",
                new[]
                {
                    "bs_affix_purifying_seal",
                    "bs_affix_focus_gather",
                    "bs_affix_guardian_ward",
                    "bs_affix_lihuo_spark"
                }));
            snapshot.placedItems.Add(CreateItem(
                "seed_affix_orange_core_bond",
                "Square4",
                new ItemShapeCell(7, 0),
                new[]
                {
                    "orange_core_preview",
                    "core_preview",
                    "bond_preview",
                    "synergy_preview",
                    "energy_preview",
                    "defense_preview",
                    "control_preview"
                },
                "orange",
                new[]
                {
                    "bs_affix_orange_core",
                    "bs_affix_bond_plus_one",
                    "bs_affix_purifying_seal",
                    "bs_affix_focus_gather",
                    "bs_affix_guardian_ward"
                }));
            return snapshot;
        }

        private static void ValidateRarityTiers(
            BuildSandboxValidationReport report,
            IReadOnlyList<RarityTierConfig> rarities)
        {
            if (rarities.Count == 0)
            {
                report.AddError(
                    "RARITY_TIER_CONFIG_MISSING",
                    "AffixRaritySandbox01 requires RarityTierConfig seed assets.",
                    BuildSandboxConfigValidator.ConfigRoot);
                return;
            }

            Dictionary<string, string> rarityIds = new(StringComparer.Ordinal);
            foreach (RarityTierConfig rarity in rarities)
            {
                string path = AssetDatabase.GetAssetPath(rarity);
                if (string.IsNullOrWhiteSpace(rarity.rarityId))
                {
                    report.AddError("RARITY_ID_MISSING", "RarityTierConfig rarityId is empty.", path);
                }
                else if (rarityIds.TryGetValue(rarity.rarityId, out string existingPath))
                {
                    report.AddError(
                        "RARITY_ID_DUPLICATE",
                        $"Rarity id '{rarity.rarityId}' is already used by {existingPath}.",
                        path);
                }
                else
                {
                    rarityIds.Add(rarity.rarityId, path);
                }

                if (!rarity.devOnly)
                {
                    report.AddError("RARITY_DEVONLY_FALSE", "RarityTierConfig must keep devOnly=true.", path);
                }

                if (rarity.isEnabled)
                {
                    report.AddError("RARITY_ENABLED_TRUE", "RarityTierConfig must keep isEnabled=false.", path);
                }

                if (rarity.tierIndex < 0)
                {
                    report.AddError("RARITY_TIER_INDEX_NEGATIVE", "Rarity tierIndex cannot be negative.", path);
                }

                if (rarity.affixSlotCount < 0)
                {
                    report.AddError("RARITY_AFFIX_SLOT_NEGATIVE", "Rarity affixSlotCount cannot be negative.", path);
                }

                if (rarity.rollWeight <= 0)
                {
                    report.AddError("RARITY_ROLL_WEIGHT_INVALID", "Rarity rollWeight must be positive.", path);
                }
            }

            foreach (string required in RequiredRarities)
            {
                if (rarityIds.ContainsKey(required))
                {
                    report.AddInfo("RARITY_REQUIRED_PRESENT", $"Required sandbox rarity is present: {required}.", rarityIds[required]);
                    continue;
                }

                report.AddError(
                    "RARITY_REQUIRED_MISSING",
                    $"Required sandbox rarity is missing: {required}.",
                    BuildSandboxConfigValidator.ConfigRoot);
            }
        }

        private static void ValidateAffixes(
            BuildSandboxValidationReport report,
            IReadOnlyList<AffixConfig> affixes,
            IReadOnlyList<RarityTierConfig> rarities)
        {
            if (affixes.Count == 0)
            {
                report.AddError(
                    "AFFIX_CONFIG_MISSING",
                    "AffixRaritySandbox01 requires AffixConfig seed assets.",
                    BuildSandboxConfigValidator.ConfigRoot);
                return;
            }

            HashSet<string> rarityIds = new(
                rarities
                    .Where(rarity => !string.IsNullOrWhiteSpace(rarity.rarityId))
                    .Select(rarity => rarity.rarityId),
                StringComparer.Ordinal);
            Dictionary<string, string> affixIds = new(StringComparer.Ordinal);

            foreach (AffixConfig affix in affixes)
            {
                string path = AssetDatabase.GetAssetPath(affix);
                if (string.IsNullOrWhiteSpace(affix.affixId))
                {
                    report.AddError("AFFIX_ID_MISSING", "AffixConfig affixId is empty.", path);
                }
                else if (affixIds.TryGetValue(affix.affixId, out string existingPath))
                {
                    report.AddError(
                        "AFFIX_ID_DUPLICATE",
                        $"Affix id '{affix.affixId}' is already used by {existingPath}.",
                        path);
                }
                else
                {
                    affixIds.Add(affix.affixId, path);
                }

                if (!affix.devOnly)
                {
                    report.AddError("AFFIX_DEVONLY_FALSE", "AffixConfig must keep devOnly=true.", path);
                }

                if (affix.isEnabled)
                {
                    report.AddError("AFFIX_ENABLED_TRUE", "AffixConfig must keep isEnabled=false.", path);
                }

                if (affix.requiredTags == null || affix.requiredTags.Count == 0)
                {
                    report.AddError("AFFIX_REQUIRED_TAGS_MISSING", "AffixConfig should reserve at least one required tag.", path);
                }

                if (affix.allowedRarities == null || affix.allowedRarities.Count == 0)
                {
                    report.AddError("AFFIX_ALLOWED_RARITIES_MISSING", "AffixConfig should reserve allowed rarities.", path);
                }
                else
                {
                    foreach (string rarityId in affix.allowedRarities.Where(id => !string.IsNullOrWhiteSpace(id)))
                    {
                        if (!rarityIds.Contains(rarityId))
                        {
                            report.AddError(
                                "AFFIX_UNKNOWN_RARITY",
                                $"AffixConfig references unknown rarity: {rarityId}.",
                                path);
                        }
                    }
                }

                if (affix.minRoll < 0 || affix.maxRoll < 0 || affix.minRoll > affix.maxRoll)
                {
                    report.AddError(
                        "AFFIX_ROLL_RANGE_INVALID",
                        $"Affix roll range is invalid: {affix.minRoll}..{affix.maxRoll}.",
                        path);
                }

                if (string.IsNullOrWhiteSpace(affix.previewResultToken))
                {
                    report.AddError("AFFIX_RESULT_TOKEN_MISSING", "AffixConfig previewResultToken is empty.", path);
                }
            }

            if (affixIds.Count > 0)
            {
                report.AddInfo(
                    "AFFIX_CONFIG_COUNTS",
                    $"AffixConfig={affixIds.Count}, RarityTierConfig={rarities.Count}.",
                    BuildSandboxConfigValidator.ConfigRoot);
            }

            foreach (string required in RequiredAffixes)
            {
                if (affixIds.ContainsKey(required))
                {
                    report.AddInfo("AFFIX_REQUIRED_PRESENT", $"Required sandbox affix is present: {required}.", affixIds[required]);
                    continue;
                }

                report.AddError(
                    "AFFIX_REQUIRED_MISSING",
                    $"Required sandbox affix is missing: {required}.",
                    BuildSandboxConfigValidator.ConfigRoot);
            }
        }

        private static void ValidateEvaluation(
            BuildSandboxValidationReport report,
            AffixRarityEvaluationResult result)
        {
            if (result == null)
            {
                report.AddError(
                    "AFFIX_RARITY_RESULT_NULL",
                    "AffixRarityEvaluator returned null.",
                    BuildSandboxConfigValidator.ConfigRoot);
                return;
            }

            if (result.itemResults.Count == 0)
            {
                report.AddError(
                    "AFFIX_RARITY_ITEMS_MISSING",
                    "AffixRarity seed snapshot should evaluate at least one item.",
                    BuildSandboxConfigValidator.ConfigRoot);
                return;
            }

            if (result.missingRequirements.Count > 0)
            {
                foreach (AffixRarityRequirementResult missing in result.missingRequirements)
                {
                    report.AddError(
                        "AFFIX_RARITY_REQUIREMENT_MISSING",
                        $"{missing.itemId}: {missing.detail}",
                        BuildSandboxConfigValidator.ConfigRoot);
                }
            }
            else
            {
                report.AddInfo(
                    "AFFIX_RARITY_REQUIREMENTS_SATISFIED",
                    $"Seed snapshot evaluated {result.itemResults.Count} item(s) without missing requirements.",
                    BuildSandboxConfigValidator.ConfigRoot);
            }

            foreach (string required in RequiredRarities)
            {
                if (result.rarityIds.Contains(required, StringComparer.Ordinal))
                {
                    report.AddInfo("AFFIX_RARITY_SAMPLE_PRESENT", $"Sample includes rarity: {required}.", BuildSandboxConfigValidator.ConfigRoot);
                    continue;
                }

                report.AddError(
                    "AFFIX_RARITY_SAMPLE_MISSING",
                    $"Sample evaluation should include rarity: {required}.",
                    BuildSandboxConfigValidator.ConfigRoot);
            }

            if (result.affixIds.Count == 0)
            {
                report.AddError(
                    "AFFIX_SELECTION_MISSING",
                    "Seed snapshot should select at least one affix.",
                    BuildSandboxConfigValidator.ConfigRoot);
            }
            else
            {
                report.AddInfo(
                    "AFFIX_SELECTION_PRESENT",
                    $"Selected affixes={result.affixIds.Count}.",
                    BuildSandboxConfigValidator.ConfigRoot);
            }

            foreach (string required in RequiredAffixes)
            {
                if (result.affixIds.Contains(required, StringComparer.Ordinal))
                {
                    report.AddInfo("AFFIX_SAMPLE_PRESENT", $"Sample selects affix: {required}.", BuildSandboxConfigValidator.ConfigRoot);
                    continue;
                }

                report.AddError(
                    "AFFIX_SAMPLE_MISSING",
                    $"Sample evaluation should select affix: {required}.",
                    BuildSandboxConfigValidator.ConfigRoot);
            }
        }

        private static BuildSandboxPlacedItemSnapshot CreateItem(
            string itemId,
            string shapeId,
            ItemShapeCell cell,
            IEnumerable<string> tags,
            string rarity,
            IEnumerable<string> affixList)
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
                isPowered = true,
                energySourceId = "seed_affix_source",
                rarity = rarity,
                affixList = (affixList ?? Enumerable.Empty<string>())
                    .Where(affix => !string.IsNullOrWhiteSpace(affix))
                    .Distinct(StringComparer.Ordinal)
                    .ToList()
            };
        }
    }
}
#endif
