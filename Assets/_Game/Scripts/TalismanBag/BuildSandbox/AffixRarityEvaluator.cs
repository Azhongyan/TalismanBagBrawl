using System;
using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.BuildSandbox
{
    public static class AffixRarityEvaluator
    {
        public static AffixRarityEvaluationResult Evaluate(
            BuildSandboxLayoutSnapshot snapshot,
            IEnumerable<RarityTierConfig> rarityConfigs,
            IEnumerable<AffixConfig> affixConfigs)
        {
            AffixRarityEvaluationResult result = new();
            Dictionary<string, RarityTierConfig> rarityById = (rarityConfigs ?? Enumerable.Empty<RarityTierConfig>())
                .Where(config => config != null && !string.IsNullOrWhiteSpace(config.rarityId))
                .GroupBy(config => config.rarityId, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
            List<AffixConfig> affixes = (affixConfigs ?? Enumerable.Empty<AffixConfig>())
                .Where(config => config != null && !string.IsNullOrWhiteSpace(config.affixId))
                .OrderBy(config => config.affixId, StringComparer.Ordinal)
                .ToList();

            foreach (BuildSandboxPlacedItemSnapshot item in snapshot?.placedItems ?? Enumerable.Empty<BuildSandboxPlacedItemSnapshot>())
            {
                if (item == null)
                {
                    continue;
                }

                AffixRarityItemResult itemResult = EvaluateItem(item, rarityById, affixes, result.missingRequirements);
                result.itemResults.Add(itemResult);
                AddDistinct(result.rarityIds, itemResult.rarityId);
                AddDistinct(result.affixIds, itemResult.selectedAffixes);
            }

            result.rarityIds.Sort(StringComparer.Ordinal);
            result.affixIds.Sort(StringComparer.Ordinal);
            return result;
        }

        private static AffixRarityItemResult EvaluateItem(
            BuildSandboxPlacedItemSnapshot item,
            IReadOnlyDictionary<string, RarityTierConfig> rarityById,
            IReadOnlyList<AffixConfig> affixes,
            List<AffixRarityRequirementResult> missingRequirements)
        {
            string rarityId = string.IsNullOrWhiteSpace(item.rarity) ? "common" : item.rarity.Trim();
            AffixRarityItemResult result = new()
            {
                itemId = item.itemId ?? string.Empty,
                rarityId = rarityId,
                sourceTags = Normalize(item.tags)
            };

            if (!rarityById.TryGetValue(rarityId, out RarityTierConfig rarity))
            {
                missingRequirements.Add(new AffixRarityRequirementResult
                {
                    itemId = result.itemId,
                    requirementType = "rarity",
                    requiredId = rarityId,
                    satisfied = false,
                    detail = $"Unknown rarity '{rarityId}'."
                });
                return result;
            }

            result.tierIndex = rarity.tierIndex;
            result.affixSlotCount = Math.Max(0, rarity.affixSlotCount);
            result.previewPowerMultiplier = Math.Max(0f, rarity.previewPowerMultiplier);

            List<string> explicitAffixes = Normalize(item.affixList);
            IEnumerable<AffixConfig> candidates = explicitAffixes.Count > 0
                ? affixes.Where(affix => explicitAffixes.Contains(affix.affixId, StringComparer.Ordinal))
                : affixes.Where(affix => MatchesItem(affix, rarityId, result.sourceTags));

            foreach (AffixConfig affix in candidates.Take(result.affixSlotCount))
            {
                if (!MatchesItem(affix, rarityId, result.sourceTags))
                {
                    missingRequirements.Add(new AffixRarityRequirementResult
                    {
                        itemId = result.itemId,
                        requirementType = "affix_match",
                        requiredId = affix.affixId,
                        satisfied = false,
                        detail = $"Affix '{affix.affixId}' does not match item tags or rarity."
                    });
                    continue;
                }

                result.selectedAffixes.Add(affix.affixId);
                int midpoint = (Math.Max(0, affix.minRoll) + Math.Max(0, affix.maxRoll)) / 2;
                result.totalPreviewPower += (int)Math.Round(midpoint * result.previewPowerMultiplier);
            }

            if (explicitAffixes.Count > 0)
            {
                foreach (string affixId in explicitAffixes.Where(id => !result.selectedAffixes.Contains(id, StringComparer.Ordinal)))
                {
                    missingRequirements.Add(new AffixRarityRequirementResult
                    {
                        itemId = result.itemId,
                        requirementType = "explicit_affix",
                        requiredId = affixId,
                        satisfied = false,
                        detail = $"Requested affix '{affixId}' was not selected."
                    });
                }
            }

            if (result.selectedAffixes.Count == 0 && result.affixSlotCount > 0)
            {
                missingRequirements.Add(new AffixRarityRequirementResult
                {
                    itemId = result.itemId,
                    requirementType = "affix_candidate",
                    requiredId = rarityId,
                    satisfied = false,
                    detail = "No eligible affix candidate matched this item."
                });
            }

            return result;
        }

        private static bool MatchesItem(AffixConfig affix, string rarityId, IReadOnlyCollection<string> tags)
        {
            if (affix == null)
            {
                return false;
            }

            List<string> allowedRarities = Normalize(affix.allowedRarities);
            if (allowedRarities.Count > 0 && !allowedRarities.Contains(rarityId, StringComparer.Ordinal))
            {
                return false;
            }

            List<string> requiredTags = Normalize(affix.requiredTags);
            return requiredTags.Count == 0 || requiredTags.Any(tag => tags.Contains(tag, StringComparer.Ordinal));
        }

        private static List<string> Normalize(IEnumerable<string> values)
        {
            return (values ?? Enumerable.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }

        private static void AddDistinct(List<string> target, IEnumerable<string> values)
        {
            foreach (string value in values ?? Enumerable.Empty<string>())
            {
                AddDistinct(target, value);
            }
        }

        private static void AddDistinct(List<string> target, string value)
        {
            if (!string.IsNullOrWhiteSpace(value) && !target.Contains(value, StringComparer.Ordinal))
            {
                target.Add(value);
            }
        }
    }
}
