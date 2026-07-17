using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TalismanBag.Items.Lighting
{
    public interface IItemArrayBonusSnapshotProvider
    {
        ItemArrayBonusResolutionResult GetArrayBonusSnapshot();
    }

    public sealed class ItemArrayBonusCellDefinition
    {
        public ItemArrayBonusCellDefinition(string cellId, Vector2Int cell)
        {
            this.cellId = cellId ?? string.Empty;
            this.cell = cell;
        }

        public string cellId;
        public Vector2Int cell;
    }

    public sealed class ItemArrayBonusCellState
    {
        private readonly string[] occupyingPlacementIds;
        private readonly string[] occupyingItemIds;

        public ItemArrayBonusCellState(
            string cellId,
            Vector2Int cell,
            IReadOnlyList<string> occupyingPlacementIds,
            IReadOnlyList<string> occupyingItemIds)
        {
            this.cellId = cellId ?? string.Empty;
            this.cell = cell;
            this.occupyingPlacementIds = (occupyingPlacementIds ?? Array.Empty<string>()).ToArray();
            this.occupyingItemIds = (occupyingItemIds ?? Array.Empty<string>()).ToArray();
        }

        public string cellId;
        public Vector2Int cell;
        public bool isOccupied => occupyingPlacementIds.Length > 0;
        public IReadOnlyList<string> OccupyingPlacementIds => occupyingPlacementIds;
        public IReadOnlyList<string> OccupyingItemIds => occupyingItemIds;
    }

    public sealed class ItemArrayBonusItemResult
    {
        private readonly Vector2Int[] occupiedCells;
        private readonly Vector2Int[] occupiedArrayBonusCells;
        private readonly string[] occupiedArrayBonusCellIds;

        public ItemArrayBonusItemResult(
            string itemId,
            string placementId,
            string displayName,
            IReadOnlyList<Vector2Int> occupiedCells,
            IReadOnlyList<Vector2Int> occupiedArrayBonusCells,
            IReadOnlyList<string> occupiedArrayBonusCellIds,
            bool isLit,
            bool isLightingSource)
        {
            this.itemId = itemId ?? string.Empty;
            this.placementId = string.IsNullOrWhiteSpace(placementId) ? this.itemId : placementId;
            this.displayName = displayName ?? string.Empty;
            this.occupiedCells = (occupiedCells ?? Array.Empty<Vector2Int>()).ToArray();
            this.occupiedArrayBonusCells = (occupiedArrayBonusCells ?? Array.Empty<Vector2Int>()).ToArray();
            this.occupiedArrayBonusCellIds = (occupiedArrayBonusCellIds ?? Array.Empty<string>()).ToArray();
            this.isLit = isLit;
            this.isLightingSource = isLightingSource;
        }

        public string itemId;
        public string placementId;
        public string displayName;
        public bool isLit;
        public bool isLightingSource;
        public bool isOnArrayBonusCell => occupiedArrayBonusCells.Length > 0;
        public bool isArrayBonusActive => isLit && isOnArrayBonusCell;
        public IReadOnlyList<Vector2Int> OccupiedCells => occupiedCells;
        public IReadOnlyList<Vector2Int> OccupiedArrayBonusCells => occupiedArrayBonusCells;
        public IReadOnlyList<string> OccupiedArrayBonusCellIds => occupiedArrayBonusCellIds;
    }

    public sealed class ItemArrayBonusResolutionResult
    {
        private readonly ItemArrayBonusCellState[] cellStates;
        private readonly ItemArrayBonusItemResult[] itemResults;
        private readonly string[] onArrayPlacementIds;
        private readonly string[] activePlacementIds;

        public ItemArrayBonusResolutionResult(
            IReadOnlyList<ItemArrayBonusCellState> cellStates,
            IReadOnlyList<ItemArrayBonusItemResult> itemResults)
        {
            this.cellStates = (cellStates ?? Array.Empty<ItemArrayBonusCellState>())
                .OrderBy(state => state.cellId, StringComparer.Ordinal)
                .ToArray();
            this.itemResults = (itemResults ?? Array.Empty<ItemArrayBonusItemResult>())
                .OrderBy(item => item.placementId, StringComparer.Ordinal)
                .ThenBy(item => item.itemId, StringComparer.Ordinal)
                .ToArray();
            onArrayPlacementIds = this.itemResults
                .Where(item => item.isOnArrayBonusCell)
                .Select(item => item.placementId)
                .ToArray();
            activePlacementIds = this.itemResults
                .Where(item => item.isArrayBonusActive)
                .Select(item => item.placementId)
                .ToArray();
        }

        public IReadOnlyList<ItemArrayBonusCellState> CellStates => cellStates;
        public IReadOnlyList<ItemArrayBonusItemResult> ItemResults => itemResults;
        public IReadOnlyList<string> OnArrayPlacementIds => onArrayPlacementIds;
        public IReadOnlyList<string> ActivePlacementIds => activePlacementIds;

        public ItemArrayBonusCellState FindCellState(string cellId)
        {
            return cellStates.FirstOrDefault(state => string.Equals(state.cellId, cellId, StringComparison.Ordinal));
        }

        public ItemArrayBonusItemResult FindPlacementResult(string placementId)
        {
            return itemResults.FirstOrDefault(item => string.Equals(item.placementId, placementId, StringComparison.Ordinal));
        }

        public ItemArrayBonusItemResult FindItemResult(string itemId)
        {
            return itemResults.FirstOrDefault(item => string.Equals(item.itemId, itemId, StringComparison.Ordinal));
        }

        public IReadOnlyList<ItemArrayBonusItemResult> FindItemResults(string itemId)
        {
            return itemResults
                .Where(item => string.Equals(item.itemId, itemId, StringComparison.Ordinal))
                .ToArray();
        }
    }

    public static class ItemArrayBonusResolver
    {
        private static readonly ItemArrayBonusCellDefinition[] FixedCellDefinitions =
        {
            new("AP01", new Vector2Int(2, 1)),
            new("AP02", new Vector2Int(2, 3)),
            new("AP03", new Vector2Int(1, 2)),
            new("AP04", new Vector2Int(3, 2))
        };

        public static IReadOnlyList<ItemArrayBonusCellDefinition> FixedArrayBonusCells => FixedCellDefinitions;

        public static ItemArrayBonusResolutionResult Resolve(
            ItemLightingBoardConfig boardConfig,
            IReadOnlyList<ItemLightingItemResult> lightingResults)
        {
            boardConfig ??= new ItemLightingBoardConfig(5, new Vector2Int(2, 2), FixedCellDefinitions.Select(cell => cell.cell).ToArray());

            IReadOnlyList<ItemArrayBonusCellDefinition> cellDefinitions = BuildCellDefinitions(boardConfig.ArrayBonusCells);
            HashSet<Vector2Int> arrayCellSet = new(cellDefinitions.Select(definition => definition.cell));
            List<ItemArrayBonusItemResult> itemResults = new();
            foreach (ItemLightingItemResult lightingResult in lightingResults ?? Array.Empty<ItemLightingItemResult>())
            {
                if (lightingResult == null)
                {
                    continue;
                }

                Vector2Int[] occupiedArrayBonusCells = lightingResult.OccupiedCells
                    .Where(arrayCellSet.Contains)
                    .Distinct()
                    .OrderBy(cell => cell.y)
                    .ThenBy(cell => cell.x)
                    .ToArray();
                string[] occupiedArrayBonusCellIds = occupiedArrayBonusCells
                    .Select(cell => FindCellId(cellDefinitions, cell))
                    .Where(cellId => !string.IsNullOrEmpty(cellId))
                    .ToArray();

                itemResults.Add(new ItemArrayBonusItemResult(
                    lightingResult.itemId,
                    lightingResult.placementId,
                    lightingResult.displayName,
                    lightingResult.OccupiedCells,
                    occupiedArrayBonusCells,
                    occupiedArrayBonusCellIds,
                    lightingResult.isLit,
                    lightingResult.isLightingSource));
            }

            List<ItemArrayBonusCellState> cellStates = new();
            foreach (ItemArrayBonusCellDefinition definition in cellDefinitions)
            {
                ItemArrayBonusItemResult[] occupyingItems = itemResults
                    .Where(item => item.OccupiedArrayBonusCells.Contains(definition.cell))
                    .OrderBy(item => item.placementId, StringComparer.Ordinal)
                    .ToArray();
                cellStates.Add(new ItemArrayBonusCellState(
                    definition.cellId,
                    definition.cell,
                    occupyingItems.Select(item => item.placementId).ToArray(),
                    occupyingItems.Select(item => item.itemId).ToArray()));
            }

            return new ItemArrayBonusResolutionResult(cellStates, itemResults);
        }

        public static string FormatCellIds(IReadOnlyList<string> cellIds)
        {
            return cellIds == null || cellIds.Count == 0 ? "None" : string.Join("|", cellIds);
        }

        private static IReadOnlyList<ItemArrayBonusCellDefinition> BuildCellDefinitions(IReadOnlyList<Vector2Int> arrayBonusCells)
        {
            List<ItemArrayBonusCellDefinition> definitions = new();
            foreach (Vector2Int cell in (arrayBonusCells ?? Array.Empty<Vector2Int>()).Distinct())
            {
                definitions.Add(new ItemArrayBonusCellDefinition(
                    FindFixedCellId(cell) ?? $"AP{definitions.Count + 1:00}",
                    cell));
            }

            return definitions;
        }

        private static string FindCellId(IReadOnlyList<ItemArrayBonusCellDefinition> definitions, Vector2Int cell)
        {
            return definitions.FirstOrDefault(definition => definition.cell == cell)?.cellId ?? string.Empty;
        }

        private static string FindFixedCellId(Vector2Int cell)
        {
            return FixedCellDefinitions.FirstOrDefault(definition => definition.cell == cell)?.cellId;
        }
    }
}
