using System;
using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.BuildSandbox
{
    public static class SynergyEvaluator
    {
        private static readonly int[] AllowedThresholds = { 2, 4, 6, 8 };

        public static BuildEvaluationResult Evaluate(
            BuildSandboxLayoutSnapshot snapshot,
            IEnumerable<SynergyConfig> synergyConfigs)
        {
            BuildEvaluationResult result = new();
            List<BuildSandboxPlacedItemSnapshot> placedItems = snapshot?.placedItems?
                .Where(item => item != null)
                .ToList() ?? new List<BuildSandboxPlacedItemSnapshot>();

            foreach (SynergyConfig config in synergyConfigs ?? Enumerable.Empty<SynergyConfig>())
            {
                if (config == null)
                {
                    continue;
                }

                ActiveSynergyResult synergyResult = EvaluateSynergy(config, placedItems);
                AddDistinct(result.sourceItems, synergyResult.sourceItems);

                foreach (SynergyRequirementResult requirement in synergyResult.requirementResults)
                {
                    if (!requirement.satisfied)
                    {
                        result.missingRequirements.Add(requirement);
                    }
                }

                result.placementSatisfied &= synergyResult.placementSatisfied;
                result.energySatisfied &= synergyResult.energySatisfied;

                if (synergyResult.activeThresholds.Count > 0
                    && synergyResult.placementSatisfied
                    && synergyResult.energySatisfied
                    && synergyResult.requirementResults.All(requirement => requirement.satisfied))
                {
                    result.activeSynergies.Add(synergyResult);
                    AddDistinct(result.activeThresholds, synergyResult.activeThresholds);
                }

                result.nextThresholdHint = SelectNearestHint(
                    result.nextThresholdHint,
                    synergyResult.nextThresholdHint);
            }

            result.sourceItems.Sort(StringComparer.Ordinal);
            result.activeThresholds.Sort(StringComparer.Ordinal);
            return result;
        }

        private static ActiveSynergyResult EvaluateSynergy(
            SynergyConfig config,
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> placedItems)
        {
            ActiveSynergyResult result = new()
            {
                synergyId = config.synergyId ?? string.Empty,
                displayName = config.displayName ?? string.Empty
            };

            List<string> requiredTags = NormalizeTags(config.requiredTags);
            List<BuildSandboxPlacedItemSnapshot> sourceItems = placedItems
                .Where(item => MatchesAnyRequiredTag(item, requiredTags))
                .ToList();

            result.matchedCount = sourceItems.Count;
            result.sourceItems = sourceItems
                .Select(item => item.itemId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList();

            result.requirementResults.Add(EvaluateTagCoverage(config, requiredTags, sourceItems));

            IReadOnlyList<SynergyThresholdConfig> thresholds = NormalizeThresholds(config.thresholds);
            foreach (SynergyThresholdConfig threshold in thresholds)
            {
                if (sourceItems.Count >= threshold.pieceCount)
                {
                    result.activeThresholds.Add(BuildThresholdKey(config.synergyId, threshold.pieceCount));
                }
            }

            if (result.activeThresholds.Count == 0 && thresholds.Count > 0)
            {
                SynergyThresholdConfig firstThreshold = thresholds[0];
                result.requirementResults.Add(new SynergyRequirementResult
                {
                    synergyId = config.synergyId ?? string.Empty,
                    requirementId = BuildThresholdKey(config.synergyId, firstThreshold.pieceCount),
                    requirementType = "count_threshold",
                    requiredCount = firstThreshold.pieceCount,
                    actualCount = sourceItems.Count,
                    satisfied = false,
                    sourceItems = new List<string>(result.sourceItems),
                    detail = $"Needs {Math.Max(0, firstThreshold.pieceCount - sourceItems.Count)} more matching item(s)."
                });
            }

            result.placementSatisfied = EvaluatePlacementConditions(
                config,
                config.placementConditions,
                sourceItems,
                placedItems,
                result.requirementResults);

            result.energySatisfied = EvaluateEnergyConditions(
                config,
                config.energyConditions,
                sourceItems,
                placedItems,
                result.requirementResults);

            result.nextThresholdHint = BuildNextThresholdHint(config, requiredTags, sourceItems.Count, result.sourceItems);
            return result;
        }

        private static SynergyRequirementResult EvaluateTagCoverage(
            SynergyConfig config,
            IReadOnlyList<string> requiredTags,
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> sourceItems)
        {
            HashSet<string> coveredTags = new(StringComparer.Ordinal);
            foreach (BuildSandboxPlacedItemSnapshot item in sourceItems)
            {
                foreach (string tag in NormalizeTags(item.tags))
                {
                    coveredTags.Add(tag);
                }
            }

            int coveredCount = requiredTags.Count(tag => coveredTags.Contains(tag));
            bool satisfied = requiredTags.Count > 0 && coveredCount == requiredTags.Count;
            return new SynergyRequirementResult
            {
                synergyId = config.synergyId ?? string.Empty,
                requirementId = $"{config.synergyId}:required_tags",
                requirementType = "tag_coverage",
                requiredCount = requiredTags.Count,
                actualCount = coveredCount,
                satisfied = satisfied,
                sourceItems = sourceItems
                    .Select(item => item.itemId)
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(id => id, StringComparer.Ordinal)
                    .ToList(),
                detail = satisfied
                    ? "All required tags are present in the sandbox snapshot."
                    : $"Missing tag coverage: {string.Join(";", requiredTags.Where(tag => !coveredTags.Contains(tag)))}."
            };
        }

        private static bool EvaluatePlacementConditions(
            SynergyConfig config,
            IEnumerable<PlacementConditionConfig> conditions,
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> sourceItems,
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> placedItems,
            List<SynergyRequirementResult> requirements)
        {
            bool satisfied = true;
            foreach (PlacementConditionConfig condition in conditions ?? Enumerable.Empty<PlacementConditionConfig>())
            {
                SynergyRequirementResult requirement = EvaluatePlacementCondition(
                    config,
                    condition,
                    sourceItems,
                    placedItems);
                requirements.Add(requirement);
                satisfied &= requirement.satisfied;
            }

            return satisfied;
        }

        private static SynergyRequirementResult EvaluatePlacementCondition(
            SynergyConfig config,
            PlacementConditionConfig condition,
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> sourceItems,
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> placedItems)
        {
            string conditionType = NormalizeConditionType(condition?.conditionType);
            string requiredTag = condition?.requiredTag ?? string.Empty;
            int requiredCount = Math.Max(1, condition?.requiredCount ?? 1);
            List<BuildSandboxPlacedItemSnapshot> taggedItems = FilterByTag(sourceItems, requiredTag);
            int actualCount;
            bool satisfied;
            string detail;

            switch (conditionType)
            {
                case "":
                case "none":
                    actualCount = 0;
                    satisfied = true;
                    detail = "No placement condition requested.";
                    break;
                case "adjacent":
                case "adjacent_to_tag":
                case "neighbor":
                    actualCount = taggedItems.Count(item => HasAdjacentItem(item, placedItems));
                    satisfied = actualCount >= requiredCount;
                    detail = $"Adjacent tagged items={actualCount}, required={requiredCount}.";
                    break;
                case "separated":
                case "separated_from_tag":
                case "not_adjacent_to_tag":
                    bool hasAdjacentPair = HasAdjacentPair(taggedItems);
                    actualCount = taggedItems.Count;
                    satisfied = actualCount >= requiredCount && !hasAdjacentPair;
                    detail = hasAdjacentPair
                        ? "At least one tagged item is adjacent to the separated tag set."
                        : $"Separated tagged items={actualCount}, required={requiredCount}.";
                    break;
                case "same_row":
                    actualCount = MaxItemsSharingAxis(taggedItems, useRow: true);
                    satisfied = actualCount >= requiredCount;
                    detail = $"Max tagged items sharing a row={actualCount}, required={requiredCount}.";
                    break;
                case "same_column":
                    actualCount = MaxItemsSharingAxis(taggedItems, useRow: false);
                    satisfied = actualCount >= requiredCount;
                    detail = $"Max tagged items sharing a column={actualCount}, required={requiredCount}.";
                    break;
                case "edge":
                    actualCount = CountItemsAtEdge(taggedItems, placedItems);
                    satisfied = actualCount >= requiredCount;
                    detail = $"Tagged items touching inferred edge={actualCount}, required={requiredCount}.";
                    break;
                case "corner":
                    actualCount = CountItemsAtCorner(taggedItems, placedItems);
                    satisfied = actualCount >= requiredCount;
                    detail = $"Tagged items touching inferred corner={actualCount}, required={requiredCount}.";
                    break;
                case "near_center":
                case "near_eye":
                case "around_core":
                    actualCount = CountItemsNearCenter(taggedItems, placedItems);
                    satisfied = actualCount >= requiredCount;
                    detail = $"Tagged items near inferred center={actualCount}, required={requiredCount}.";
                    break;
                default:
                    actualCount = 0;
                    satisfied = false;
                    detail = $"Unsupported placement conditionType '{condition?.conditionType}'.";
                    break;
            }

            return new SynergyRequirementResult
            {
                synergyId = config.synergyId ?? string.Empty,
                requirementId = condition?.conditionId ?? string.Empty,
                requirementType = $"placement:{conditionType}",
                requiredTag = requiredTag,
                requiredCount = requiredCount,
                actualCount = actualCount,
                satisfied = satisfied,
                sourceItems = taggedItems
                    .Select(item => item.itemId)
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(id => id, StringComparer.Ordinal)
                    .ToList(),
                detail = detail
            };
        }

        private static bool EvaluateEnergyConditions(
            SynergyConfig config,
            IEnumerable<EnergyConditionConfig> conditions,
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> sourceItems,
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> placedItems,
            List<SynergyRequirementResult> requirements)
        {
            bool satisfied = true;
            foreach (EnergyConditionConfig condition in conditions ?? Enumerable.Empty<EnergyConditionConfig>())
            {
                SynergyRequirementResult requirement = EvaluateEnergyCondition(
                    config,
                    condition,
                    sourceItems,
                    placedItems);
                requirements.Add(requirement);
                satisfied &= requirement.satisfied;
            }

            return satisfied;
        }

        private static SynergyRequirementResult EvaluateEnergyCondition(
            SynergyConfig config,
            EnergyConditionConfig condition,
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> sourceItems,
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> placedItems)
        {
            string providerTag = condition?.requiredProviderTag ?? string.Empty;
            HashSet<string> providerIds = new(
                FilterByTag(placedItems, providerTag)
                    .Select(item => item.itemId)
                    .Where(id => !string.IsNullOrWhiteSpace(id)),
                StringComparer.Ordinal);

            List<BuildSandboxPlacedItemSnapshot> poweredItems = sourceItems
                .Where(item => item.isPowered)
                .Where(item => string.IsNullOrWhiteSpace(providerTag)
                    || HasTag(item, providerTag)
                    || providerIds.Contains(item.energySourceId ?? string.Empty))
                .ToList();

            int minimum = Math.Max(0, condition?.minimumEnergy ?? 0);
            if (condition != null && condition.requiresPositiveNetEnergy && minimum == 0)
            {
                minimum = 1;
            }

            int maximum = Math.Max(0, condition?.maximumEnergy ?? 0);
            int actualPowered = poweredItems.Count;
            int maxSameSourceCount = poweredItems
                .Where(item => !string.IsNullOrWhiteSpace(item.energySourceId))
                .GroupBy(item => item.energySourceId, StringComparer.Ordinal)
                .Select(group => group.Count())
                .DefaultIfEmpty(0)
                .Max();

            bool minimumSatisfied = actualPowered >= minimum;
            bool maximumSatisfied = maximum == 0 || actualPowered <= maximum;
            bool sameSourceSatisfied = minimum <= 1 || maxSameSourceCount >= minimum;
            bool satisfied = minimumSatisfied && maximumSatisfied && sameSourceSatisfied;

            return new SynergyRequirementResult
            {
                synergyId = config.synergyId ?? string.Empty,
                requirementId = condition?.conditionId ?? string.Empty,
                requirementType = "energy:powered_or_same_source",
                requiredTag = providerTag,
                requiredCount = minimum,
                actualCount = actualPowered,
                satisfied = satisfied,
                sourceItems = poweredItems
                    .Select(item => item.itemId)
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(id => id, StringComparer.Ordinal)
                    .ToList(),
                detail =
                    $"Powered items={actualPowered}, maxSameSource={maxSameSourceCount}, min={minimum}, max={(maximum == 0 ? "unbounded" : maximum.ToString())}."
            };
        }

        private static NextThresholdHint BuildNextThresholdHint(
            SynergyConfig config,
            IReadOnlyList<string> requiredTags,
            int currentPieceCount,
            IReadOnlyList<string> sourceItems)
        {
            SynergyThresholdConfig nextThreshold = NormalizeThresholds(config.thresholds)
                .FirstOrDefault(threshold => threshold.pieceCount > currentPieceCount);

            if (nextThreshold == null)
            {
                return NextThresholdHint.None();
            }

            return new NextThresholdHint
            {
                synergyId = config.synergyId ?? string.Empty,
                currentPieceCount = currentPieceCount,
                nextPieceCount = nextThreshold.pieceCount,
                missingPieceCount = Math.Max(0, nextThreshold.pieceCount - currentPieceCount),
                requiredTags = new List<string>(requiredTags),
                sourceItems = new List<string>(sourceItems ?? Array.Empty<string>()),
                hasNextThreshold = true
            };
        }

        private static NextThresholdHint SelectNearestHint(
            NextThresholdHint current,
            NextThresholdHint candidate)
        {
            if (candidate == null || !candidate.hasNextThreshold)
            {
                return current ?? NextThresholdHint.None();
            }

            if (current == null || !current.hasNextThreshold)
            {
                return candidate;
            }

            if (candidate.missingPieceCount != current.missingPieceCount)
            {
                return candidate.missingPieceCount < current.missingPieceCount ? candidate : current;
            }

            return candidate.nextPieceCount < current.nextPieceCount ? candidate : current;
        }

        private static IReadOnlyList<SynergyThresholdConfig> NormalizeThresholds(
            IEnumerable<SynergyThresholdConfig> thresholds)
        {
            return (thresholds ?? Enumerable.Empty<SynergyThresholdConfig>())
                .Where(threshold => threshold != null)
                .Where(threshold => AllowedThresholds.Contains(threshold.pieceCount))
                .GroupBy(threshold => threshold.pieceCount)
                .Select(group => group.First())
                .OrderBy(threshold => threshold.pieceCount)
                .ToArray();
        }

        private static List<string> NormalizeTags(IEnumerable<string> tags)
        {
            return (tags ?? Enumerable.Empty<string>())
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Select(tag => tag.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }

        private static bool MatchesAnyRequiredTag(
            BuildSandboxPlacedItemSnapshot item,
            IReadOnlyCollection<string> requiredTags)
        {
            if (item == null || requiredTags == null || requiredTags.Count == 0)
            {
                return false;
            }

            return NormalizeTags(item.tags).Any(requiredTags.Contains);
        }

        private static bool HasTag(BuildSandboxPlacedItemSnapshot item, string tag)
        {
            return !string.IsNullOrWhiteSpace(tag) && NormalizeTags(item?.tags).Contains(tag);
        }

        private static List<BuildSandboxPlacedItemSnapshot> FilterByTag(
            IEnumerable<BuildSandboxPlacedItemSnapshot> items,
            string tag)
        {
            return (items ?? Enumerable.Empty<BuildSandboxPlacedItemSnapshot>())
                .Where(item => item != null)
                .Where(item => string.IsNullOrWhiteSpace(tag) || HasTag(item, tag))
                .ToList();
        }

        private static bool HasAdjacentItem(
            BuildSandboxPlacedItemSnapshot item,
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> placedItems)
        {
            return placedItems.Any(other => other != null
                && !ReferenceEquals(item, other)
                && !string.Equals(item.itemId, other.itemId, StringComparison.Ordinal)
                && AreAdjacent(item, other));
        }

        private static bool HasAdjacentPair(IReadOnlyList<BuildSandboxPlacedItemSnapshot> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                for (int j = i + 1; j < items.Count; j++)
                {
                    if (AreAdjacent(items[i], items[j]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool AreAdjacent(
            BuildSandboxPlacedItemSnapshot a,
            BuildSandboxPlacedItemSnapshot b)
        {
            foreach (ItemShapeCell aCell in a?.occupiedCells ?? Enumerable.Empty<ItemShapeCell>())
            {
                foreach (ItemShapeCell bCell in b?.occupiedCells ?? Enumerable.Empty<ItemShapeCell>())
                {
                    int distance = Math.Abs(aCell.x - bCell.x) + Math.Abs(aCell.y - bCell.y);
                    if (distance == 1)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static int MaxItemsSharingAxis(
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> items,
            bool useRow)
        {
            return items
                .SelectMany(item => (item.occupiedCells ?? new List<ItemShapeCell>())
                    .Select(cell => new { item.itemId, Axis = useRow ? cell.y : cell.x }))
                .GroupBy(entry => entry.Axis)
                .Select(group => group.Select(entry => entry.itemId).Distinct(StringComparer.Ordinal).Count())
                .DefaultIfEmpty(0)
                .Max();
        }

        private static int CountItemsAtEdge(
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> items,
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> placedItems)
        {
            if (!TryGetBounds(placedItems, out int minX, out int maxX, out int minY, out int maxY))
            {
                return 0;
            }

            return items.Count(item => (item.occupiedCells ?? new List<ItemShapeCell>())
                .Any(cell => cell.x == minX || cell.x == maxX || cell.y == minY || cell.y == maxY));
        }

        private static int CountItemsAtCorner(
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> items,
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> placedItems)
        {
            if (!TryGetBounds(placedItems, out int minX, out int maxX, out int minY, out int maxY))
            {
                return 0;
            }

            return items.Count(item => (item.occupiedCells ?? new List<ItemShapeCell>())
                .Any(cell => (cell.x == minX || cell.x == maxX) && (cell.y == minY || cell.y == maxY)));
        }

        private static int CountItemsNearCenter(
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> items,
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> placedItems)
        {
            if (!TryGetBounds(placedItems, out int minX, out int maxX, out int minY, out int maxY))
            {
                return 0;
            }

            int centerX = (minX + maxX) / 2;
            int centerY = (minY + maxY) / 2;
            return items.Count(item => (item.occupiedCells ?? new List<ItemShapeCell>())
                .Any(cell => Math.Abs(cell.x - centerX) + Math.Abs(cell.y - centerY) <= 1));
        }

        private static bool TryGetBounds(
            IEnumerable<BuildSandboxPlacedItemSnapshot> placedItems,
            out int minX,
            out int maxX,
            out int minY,
            out int maxY)
        {
            List<ItemShapeCell> cells = (placedItems ?? Enumerable.Empty<BuildSandboxPlacedItemSnapshot>())
                .Where(item => item != null)
                .SelectMany(item => item.occupiedCells ?? new List<ItemShapeCell>())
                .ToList();

            if (cells.Count == 0)
            {
                minX = maxX = minY = maxY = 0;
                return false;
            }

            minX = cells.Min(cell => cell.x);
            maxX = cells.Max(cell => cell.x);
            minY = cells.Min(cell => cell.y);
            maxY = cells.Max(cell => cell.y);
            return true;
        }

        private static string NormalizeConditionType(string conditionType)
        {
            return (conditionType ?? string.Empty).Trim().ToLowerInvariant();
        }

        private static string BuildThresholdKey(string synergyId, int pieceCount)
        {
            return $"{synergyId ?? string.Empty}:{pieceCount}";
        }

        private static void AddDistinct(List<string> target, IEnumerable<string> values)
        {
            foreach (string value in values ?? Enumerable.Empty<string>())
            {
                if (!string.IsNullOrWhiteSpace(value) && !target.Contains(value, StringComparer.Ordinal))
                {
                    target.Add(value);
                }
            }
        }
    }
}
