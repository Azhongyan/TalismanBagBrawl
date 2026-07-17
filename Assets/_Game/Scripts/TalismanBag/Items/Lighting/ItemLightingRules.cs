using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TalismanBag.Items.Lighting
{
    public interface IItemLightingRangeRule
    {
        IReadOnlyList<Vector2Int> DirectLitRangeOffsets { get; }
        IReadOnlyList<Vector2Int> BuildLitRangeCells(ItemLightingBoardConfig boardConfig, ItemLightingPlacedItem sourceItem);
    }

    public interface IItemLightingSnapshotProvider
    {
        ItemLightingResolutionResult GetLightingSnapshot();
    }

    public sealed class ItemLightingBoardConfig
    {
        private readonly Vector2Int[] arrayBonusCells;

        public ItemLightingBoardConfig(
            int boardSize,
            Vector2Int eyeCell,
            IReadOnlyList<Vector2Int> arrayBonusCells)
        {
            this.boardSize = boardSize;
            this.eyeCell = eyeCell;
            this.arrayBonusCells = (arrayBonusCells ?? Array.Empty<Vector2Int>()).ToArray();
        }

        public int boardSize;
        public Vector2Int eyeCell;
        public IReadOnlyList<Vector2Int> ArrayBonusCells => arrayBonusCells;

        public bool IsWithinBoard(Vector2Int cell)
        {
            return cell.x >= 0
                && cell.y >= 0
                && cell.x < boardSize
                && cell.y < boardSize;
        }
    }

    public sealed class OrthogonalAdjacentLightingRangeRule : IItemLightingRangeRule
    {
        private static readonly Vector2Int[] DefaultOffsets =
        {
            new(0, 1),
            new(1, 0),
            new(0, -1),
            new(-1, 0)
        };

        public IReadOnlyList<Vector2Int> DirectLitRangeOffsets => DefaultOffsets;

        public IReadOnlyList<Vector2Int> BuildLitRangeCells(
            ItemLightingBoardConfig boardConfig,
            ItemLightingPlacedItem sourceItem)
        {
            if (boardConfig == null || sourceItem == null)
            {
                return Array.Empty<Vector2Int>();
            }

            HashSet<Vector2Int> cells = new();
            foreach (Vector2Int offset in DefaultOffsets)
            {
                Vector2Int cell = sourceItem.lightingSourceCell + offset;
                if (boardConfig.IsWithinBoard(cell))
                {
                    cells.Add(cell);
                }
            }

            return cells
                .OrderBy(cell => cell.y)
                .ThenBy(cell => cell.x)
                .ToArray();
        }
    }

    public sealed class ItemLightingPlacedItem
    {
        private readonly Vector2Int[] occupiedCells;

        public ItemLightingPlacedItem(
            string itemId,
            string displayName,
            IReadOnlyList<Vector2Int> occupiedCells,
            Vector2Int coreCellWorld,
            bool isLightingSource,
            Vector2Int lightingSourceCell,
            string placementId = null)
        {
            this.itemId = itemId ?? string.Empty;
            this.placementId = NormalizePlacementId(placementId, this.itemId);
            this.displayName = displayName ?? string.Empty;
            this.occupiedCells = (occupiedCells ?? Array.Empty<Vector2Int>()).ToArray();
            this.coreCellWorld = coreCellWorld;
            this.isLightingSource = isLightingSource;
            this.lightingSourceCell = lightingSourceCell;
        }

        public string itemId;
        public string placementId;
        public string displayName;
        public Vector2Int coreCellWorld;
        public bool isLightingSource;
        public Vector2Int lightingSourceCell;
        public IReadOnlyList<Vector2Int> OccupiedCells => occupiedCells;

        private static string NormalizePlacementId(string placementId, string itemId)
        {
            return string.IsNullOrWhiteSpace(placementId)
                ? itemId ?? string.Empty
                : placementId;
        }
    }

    public sealed class ItemLightingItemResult
    {
        private readonly Vector2Int[] occupiedCells;

        public ItemLightingItemResult(
            string itemId,
            string displayName,
            IReadOnlyList<Vector2Int> occupiedCells,
            Vector2Int coreCellWorld,
            bool isLightingSource,
            bool isDirectLit,
            bool isLit,
            string litByItemId,
            int litDepth,
            string placementId = null,
            string litByPlacementId = null)
        {
            this.itemId = itemId ?? string.Empty;
            this.placementId = NormalizePlacementId(placementId, this.itemId);
            this.displayName = displayName ?? string.Empty;
            this.occupiedCells = (occupiedCells ?? Array.Empty<Vector2Int>()).ToArray();
            this.coreCellWorld = coreCellWorld;
            this.isLightingSource = isLightingSource;
            this.isDirectLit = isDirectLit;
            this.isLit = isLit;
            this.litByItemId = litByItemId ?? string.Empty;
            this.litByPlacementId = string.IsNullOrWhiteSpace(litByPlacementId)
                ? this.litByItemId
                : litByPlacementId;
            this.litDepth = litDepth;
        }

        public string itemId;
        public string placementId;
        public string displayName;
        public Vector2Int coreCellWorld;
        public bool isLightingSource;
        public bool isDirectLit;
        public bool isLit;
        public string litByItemId;
        public string litByPlacementId;
        public int litDepth;
        public IReadOnlyList<Vector2Int> OccupiedCells => occupiedCells;
        public bool IsRelayLit => isLit && !isLightingSource && !isDirectLit;

        private static string NormalizePlacementId(string placementId, string itemId)
        {
            return string.IsNullOrWhiteSpace(placementId)
                ? itemId ?? string.Empty
                : placementId;
        }
    }

    public sealed class ItemLightingResolutionResult
    {
        private readonly Vector2Int[] litRangeCells;
        private readonly ItemLightingItemResult[] itemResults;
        private readonly string[] directLitItemIds;
        private readonly string[] directLitPlacementIds;
        private readonly string[] relayLitItemIds;
        private readonly string[] relayLitPlacementIds;
        private readonly string[] unlitItemIds;
        private readonly string[] unlitPlacementIds;

        public ItemLightingResolutionResult(
            IReadOnlyList<Vector2Int> litRangeCells,
            IReadOnlyList<ItemLightingItemResult> itemResults)
        {
            this.litRangeCells = (litRangeCells ?? Array.Empty<Vector2Int>())
                .Distinct()
                .OrderBy(cell => cell.y)
                .ThenBy(cell => cell.x)
                .ToArray();
            this.itemResults = (itemResults ?? Array.Empty<ItemLightingItemResult>())
                .OrderBy(item => item.placementId, StringComparer.Ordinal)
                .ThenBy(item => item.itemId, StringComparer.Ordinal)
                .ToArray();
            directLitItemIds = this.itemResults
                .Where(item => item.isDirectLit)
                .Select(item => item.itemId)
                .ToArray();
            directLitPlacementIds = this.itemResults
                .Where(item => item.isDirectLit)
                .Select(item => item.placementId)
                .ToArray();
            relayLitItemIds = this.itemResults
                .Where(item => item.IsRelayLit)
                .Select(item => item.itemId)
                .ToArray();
            relayLitPlacementIds = this.itemResults
                .Where(item => item.IsRelayLit)
                .Select(item => item.placementId)
                .ToArray();
            unlitItemIds = this.itemResults
                .Where(item => !item.isLightingSource && !item.isLit)
                .Select(item => item.itemId)
                .ToArray();
            unlitPlacementIds = this.itemResults
                .Where(item => !item.isLightingSource && !item.isLit)
                .Select(item => item.placementId)
                .ToArray();
        }

        public IReadOnlyList<Vector2Int> LitRangeCells => litRangeCells;
        public IReadOnlyList<ItemLightingItemResult> ItemResults => itemResults;
        public IReadOnlyList<string> DirectLitItemIds => directLitItemIds;
        public IReadOnlyList<string> DirectLitPlacementIds => directLitPlacementIds;
        public IReadOnlyList<string> RelayLitItemIds => relayLitItemIds;
        public IReadOnlyList<string> RelayLitPlacementIds => relayLitPlacementIds;
        public IReadOnlyList<string> UnlitItemIds => unlitItemIds;
        public IReadOnlyList<string> UnlitPlacementIds => unlitPlacementIds;

        public ItemLightingItemResult FindItemResult(string itemId)
        {
            return itemResults.FirstOrDefault(item => string.Equals(item.itemId, itemId, StringComparison.Ordinal));
        }

        public IReadOnlyList<ItemLightingItemResult> FindItemResults(string itemId)
        {
            return itemResults
                .Where(item => string.Equals(item.itemId, itemId, StringComparison.Ordinal))
                .ToArray();
        }

        public ItemLightingItemResult FindPlacementResult(string placementId)
        {
            return itemResults.FirstOrDefault(item => string.Equals(item.placementId, placementId, StringComparison.Ordinal));
        }
    }

    public static class ItemLightingResolver
    {
        public static readonly IItemLightingRangeRule DefaultRangeRule = new OrthogonalAdjacentLightingRangeRule();

        public static ItemLightingResolutionResult Resolve(
            ItemLightingBoardConfig boardConfig,
            IReadOnlyList<ItemLightingPlacedItem> placedItems,
            IItemLightingRangeRule rangeRule = null)
        {
            rangeRule ??= DefaultRangeRule;
            boardConfig ??= new ItemLightingBoardConfig(5, new Vector2Int(2, 2), Array.Empty<Vector2Int>());
            List<ItemLightingPlacedItem> sortedItems = (placedItems ?? Array.Empty<ItemLightingPlacedItem>())
                .Where(item => item != null && !string.IsNullOrWhiteSpace(item.itemId))
                .OrderBy(item => item.placementId, StringComparer.Ordinal)
                .ThenBy(item => item.itemId, StringComparer.Ordinal)
                .ToList();

            Dictionary<string, MutableLightingState> states = sortedItems.ToDictionary(
                item => item.placementId,
                item => new MutableLightingState(item),
                StringComparer.Ordinal);

            List<ItemLightingPlacedItem> sourceItems = sortedItems
                .Where(item => item.isLightingSource)
                .ToList();
            Dictionary<string, HashSet<Vector2Int>> sourceRanges = BuildSourceRanges(boardConfig, sourceItems, rangeRule);
            HashSet<Vector2Int> allLitRangeCells = new();
            foreach (HashSet<Vector2Int> sourceRange in sourceRanges.Values)
            {
                allLitRangeCells.UnionWith(sourceRange);
            }

            foreach (ItemLightingPlacedItem sourceItem in sourceItems)
            {
                MutableLightingState sourceState = states[sourceItem.placementId];
                sourceState.isLit = true;
                sourceState.isDirectLit = false;
                sourceState.litByItemId = sourceItem.itemId;
                sourceState.litByPlacementId = sourceItem.placementId;
                sourceState.litDepth = 0;
            }

            foreach (ItemLightingPlacedItem item in sortedItems.Where(item => !item.isLightingSource))
            {
                ItemLightingPlacedItem directSource = FindDirectSource(item, sourceItems, sourceRanges);
                if (directSource == null)
                {
                    continue;
                }

                MutableLightingState state = states[item.placementId];
                state.isLit = true;
                state.isDirectLit = true;
                state.litByItemId = directSource.itemId;
                state.litByPlacementId = directSource.placementId;
                state.litDepth = 0;
            }

            ResolveRelay(sortedItems, states);

            List<ItemLightingItemResult> results = sortedItems
                .Select(item => states[item.placementId].ToResult())
                .ToList();
            return new ItemLightingResolutionResult(allLitRangeCells.ToArray(), results);
        }

        private static Dictionary<string, HashSet<Vector2Int>> BuildSourceRanges(
            ItemLightingBoardConfig boardConfig,
            IReadOnlyList<ItemLightingPlacedItem> sourceItems,
            IItemLightingRangeRule rangeRule)
        {
            Dictionary<string, HashSet<Vector2Int>> ranges = new(StringComparer.Ordinal);
            foreach (ItemLightingPlacedItem source in sourceItems)
            {
                IReadOnlyList<Vector2Int> rangeCells = rangeRule.BuildLitRangeCells(boardConfig, source);
                ranges[source.placementId] = new HashSet<Vector2Int>(rangeCells);
            }

            return ranges;
        }

        private static ItemLightingPlacedItem FindDirectSource(
            ItemLightingPlacedItem item,
            IReadOnlyList<ItemLightingPlacedItem> sourceItems,
            IReadOnlyDictionary<string, HashSet<Vector2Int>> sourceRanges)
        {
            foreach (ItemLightingPlacedItem source in sourceItems)
            {
                if (sourceRanges.TryGetValue(source.placementId, out HashSet<Vector2Int> range)
                    && range.Contains(item.coreCellWorld))
                {
                    return source;
                }
            }

            return null;
        }

        private static void ResolveRelay(
            IReadOnlyList<ItemLightingPlacedItem> sortedItems,
            Dictionary<string, MutableLightingState> states)
        {
            bool changed;
            do
            {
                changed = false;
                foreach (ItemLightingPlacedItem target in sortedItems.Where(item => !item.isLightingSource))
                {
                    MutableLightingState targetState = states[target.placementId];
                    RelayCandidate bestCandidate = RelayCandidate.FromState(targetState);

                    foreach (ItemLightingPlacedItem source in sortedItems.Where(item => !item.isLightingSource))
                    {
                        if (source.placementId == target.placementId)
                        {
                            continue;
                        }

                        MutableLightingState sourceState = states[source.placementId];
                        if (!sourceState.isLit || !AreAdjacent(source, target))
                        {
                            continue;
                        }

                        RelayCandidate candidate = new(sourceState.litDepth + 1, source.itemId, source.placementId);
                        if (candidate.IsBetterThan(bestCandidate))
                        {
                            bestCandidate = candidate;
                        }
                    }

                    if (bestCandidate.IsResolved
                        && (!targetState.isLit
                            || !targetState.isDirectLit && bestCandidate.IsBetterThan(RelayCandidate.FromState(targetState))))
                    {
                        targetState.isLit = true;
                        targetState.isDirectLit = false;
                        targetState.litByItemId = bestCandidate.litByItemId;
                        targetState.litByPlacementId = bestCandidate.litByPlacementId;
                        targetState.litDepth = bestCandidate.litDepth;
                        changed = true;
                    }
                }
            }
            while (changed);
        }

        private static bool AreAdjacent(ItemLightingPlacedItem a, ItemLightingPlacedItem b)
        {
            foreach (Vector2Int aCell in a.OccupiedCells)
            {
                foreach (Vector2Int bCell in b.OccupiedCells)
                {
                    int manhattan = Mathf.Abs(aCell.x - bCell.x) + Mathf.Abs(aCell.y - bCell.y);
                    if (manhattan == 1)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private sealed class MutableLightingState
        {
            private readonly ItemLightingPlacedItem source;

            public MutableLightingState(ItemLightingPlacedItem source)
            {
                this.source = source;
                isLit = false;
                isDirectLit = false;
                litByItemId = string.Empty;
                litByPlacementId = string.Empty;
                litDepth = -1;
            }

            public bool isLit;
            public bool isDirectLit;
            public string litByItemId;
            public string litByPlacementId;
            public int litDepth;

            public ItemLightingItemResult ToResult()
            {
                return new ItemLightingItemResult(
                    source.itemId,
                    source.displayName,
                    source.OccupiedCells,
                    source.coreCellWorld,
                    source.isLightingSource,
                    isDirectLit,
                    isLit,
                    litByItemId,
                    litDepth,
                    source.placementId,
                    litByPlacementId);
            }
        }

        private readonly struct RelayCandidate
        {
            public RelayCandidate(int litDepth, string litByItemId, string litByPlacementId)
            {
                this.litDepth = litDepth;
                this.litByItemId = litByItemId ?? string.Empty;
                this.litByPlacementId = litByPlacementId ?? string.Empty;
            }

            public readonly int litDepth;
            public readonly string litByItemId;
            public readonly string litByPlacementId;
            public bool IsResolved => litDepth >= 0 && !string.IsNullOrEmpty(litByPlacementId);

            public static RelayCandidate FromState(MutableLightingState state)
            {
                if (state == null || !state.isLit)
                {
                    return new RelayCandidate(-1, string.Empty, string.Empty);
                }

                return new RelayCandidate(state.litDepth, state.litByItemId, state.litByPlacementId);
            }

            public bool IsBetterThan(RelayCandidate other)
            {
                if (!IsResolved)
                {
                    return false;
                }

                if (!other.IsResolved)
                {
                    return true;
                }

                if (litDepth != other.litDepth)
                {
                    return litDepth < other.litDepth;
                }

                int placementCompare = string.Compare(litByPlacementId, other.litByPlacementId, StringComparison.Ordinal);
                if (placementCompare != 0)
                {
                    return placementCompare < 0;
                }

                return string.Compare(litByItemId, other.litByItemId, StringComparison.Ordinal) < 0;
            }
        }
    }
}
