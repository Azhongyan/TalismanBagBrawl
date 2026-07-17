using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.Items.InnerCatalog;
using TalismanBag.Items.Lighting;
using UnityEngine;

namespace TalismanBag.ItemSandbox
{
    public sealed class ItemSandboxLightingScenarioDefinition
    {
        private readonly ItemSandboxLightingScenarioPlacement[] placements;

        public ItemSandboxLightingScenarioDefinition(
            string scenarioId,
            string displayName,
            string description,
            IReadOnlyList<ItemSandboxLightingScenarioPlacement> placements)
        {
            this.scenarioId = scenarioId ?? string.Empty;
            this.displayName = displayName ?? string.Empty;
            this.description = description ?? string.Empty;
            this.placements = (placements ?? Array.Empty<ItemSandboxLightingScenarioPlacement>()).ToArray();
        }

        public string scenarioId;
        public string displayName;
        public string description;
        public IReadOnlyList<ItemSandboxLightingScenarioPlacement> Placements => placements;
    }

    public sealed class ItemSandboxLightingScenarioPlacement
    {
        public ItemSandboxLightingScenarioPlacement(
            string itemId,
            Vector2Int anchorCell,
            bool expectedIsDirectLit,
            bool expectedIsLit,
            string expectedLitByItemId,
            int expectedLitDepth,
            string expectedResult,
            string placementId = null)
        {
            this.itemId = itemId ?? string.Empty;
            this.placementId = string.IsNullOrWhiteSpace(placementId)
                ? this.itemId
                : placementId;
            this.anchorCell = anchorCell;
            this.expectedIsDirectLit = expectedIsDirectLit;
            this.expectedIsLit = expectedIsLit;
            this.expectedLitByItemId = expectedLitByItemId ?? string.Empty;
            this.expectedLitDepth = expectedLitDepth;
            this.expectedResult = expectedResult ?? string.Empty;
        }

        public string itemId;
        public string placementId;
        public Vector2Int anchorCell;
        public bool expectedIsDirectLit;
        public bool expectedIsLit;
        public string expectedLitByItemId;
        public int expectedLitDepth;
        public string expectedResult;
    }

    public sealed class ItemSandboxLightingScenarioBuild
    {
        private readonly ItemGridPlacementEvaluation[] placementEvaluations;
        private readonly ItemLightingPlacedItem[] placedItems;
        private readonly string[] errors;

        public ItemSandboxLightingScenarioBuild(
            ItemSandboxLightingScenarioDefinition scenario,
            IReadOnlyList<ItemGridPlacementEvaluation> placementEvaluations,
            IReadOnlyList<ItemLightingPlacedItem> placedItems,
            IReadOnlyList<string> errors)
        {
            this.scenario = scenario;
            this.placementEvaluations = (placementEvaluations ?? Array.Empty<ItemGridPlacementEvaluation>()).ToArray();
            this.placedItems = (placedItems ?? Array.Empty<ItemLightingPlacedItem>()).ToArray();
            this.errors = (errors ?? Array.Empty<string>()).ToArray();
        }

        public ItemSandboxLightingScenarioDefinition scenario;
        public IReadOnlyList<ItemGridPlacementEvaluation> PlacementEvaluations => placementEvaluations;
        public IReadOnlyList<ItemLightingPlacedItem> PlacedItems => placedItems;
        public IReadOnlyList<string> Errors => errors;
        public bool IsValid => errors.Length == 0;
    }

    public static class ItemSandboxLightingScenarioCatalog
    {
        private static readonly ItemSandboxLightingScenarioDefinition[] Scenarios =
        {
            new(
                "S01_DirectSingle",
                "1 直亮单格",
                "聚念石直接点亮一个单格普通道具。",
                new[]
                {
                    P("I031", 0, 1, false, true, "I031", 0, "source"),
                    P("I001", 1, 1, true, true, "I031", 0, "direct_lit_single")
                }),
            new(
                "S02_MultiCoreInRange",
                "2 多格核心入范围",
                "多格道具只有 coreCellWorld 进入点亮范围，也应直亮。",
                new[]
                {
                    P("I031", 2, 0, false, true, "I031", 0, "source"),
                    P("I004", 0, 0, true, true, "I031", 0, "direct_lit_core_only")
                }),
            new(
                "S03_MultiPartOnlyUnlit",
                "3 多格非核心入范围",
                "多格道具部分 occupiedCells 在范围内，但 coreCellWorld 未进入，保持未亮。",
                new[]
                {
                    P("I031", 0, 0, false, true, "I031", 0, "source"),
                    P("I004", 0, 1, false, false, string.Empty, -1, "unlit_core_outside_range")
                }),
            new(
                "S04_RelayChain",
                "4 链式接亮",
                "聚念石直亮 A，A 接亮 B，B 接亮 C。",
                new[]
                {
                    P("I031", 0, 1, false, true, "I031", 0, "source"),
                    P("I001", 1, 1, true, true, "I031", 0, "direct_A"),
                    P("I007", 2, 1, false, true, "I001", 1, "relay_B"),
                    P("I013", 3, 1, false, true, "I007", 2, "relay_C")
                }),
            new(
                "S05_DiagonalNoRelay",
                "5 斜角不接亮",
                "斜角相邻不算接亮。",
                new[]
                {
                    P("I031", 0, 0, false, true, "I031", 0, "source"),
                    P("I001", 1, 0, true, true, "I031", 0, "direct_A"),
                    P("I007", 2, 1, false, false, string.Empty, -1, "diagonal_unlit")
                }),
            new(
                "S06_EmptyGapNoRelay",
                "6 空格不跨接",
                "中间隔着空格不能跨格接亮。",
                new[]
                {
                    P("I031", 0, 0, false, true, "I031", 0, "source"),
                    P("I001", 1, 0, true, true, "I031", 0, "direct_A"),
                    P("I007", 3, 0, false, false, string.Empty, -1, "gap_unlit")
                }),
            new(
                "S07_EyeNoBridge",
                "7 阵眼不传导",
                "中间隔着阵眼石，不能穿过阵眼接亮；点亮范围几何可包含阵眼。",
                new[]
                {
                    P("I031", 1, 2, false, true, "I031", 0, "source"),
                    P("I001", 0, 2, true, true, "I031", 0, "direct_A"),
                    P("I007", 3, 2, false, false, string.Empty, -1, "eye_blocked_unlit")
                }),
            new(
                "S08_MultiPathMinDepth",
                "8 多路径稳定选源",
                "同一道具可被两条同深度路径接亮时，按稳定 itemId 选择来源。",
                new[]
                {
                    P("I031", 0, 0, false, true, "I031", 0, "source"),
                    P("I001", 0, 1, true, true, "I031", 0, "direct_low_id"),
                    P("I007", 1, 0, true, true, "I031", 0, "direct_high_id"),
                    P("I013", 1, 1, false, true, "I001", 1, "relay_min_depth_stable_id")
                }),
            new(
                "S09_NoSourceAllUnlit",
                "9 无聚念全未亮",
                "没有聚念石时，普通道具即使相邻也全部未亮。",
                new[]
                {
                    P("I001", 0, 0, false, false, string.Empty, -1, "unlit_no_source"),
                    P("I007", 1, 0, false, false, string.Empty, -1, "unlit_no_source")
                }),
            new(
                "S10_OrdinaryRelayOnly",
                "10 普通道具只接亮",
                "普通道具不能产生直接点亮范围，但已点亮后可以继续接亮相邻道具。",
                new[]
                {
                    P("I031", 0, 0, false, true, "I031", 0, "source"),
                    P("I001", 1, 0, true, true, "I031", 0, "direct_A"),
                    P("I007", 2, 0, false, true, "I001", 1, "ordinary_relay_B"),
                    P("I013", 4, 4, false, false, string.Empty, -1, "ordinary_no_direct_range")
                })
        };

        public static IReadOnlyList<ItemSandboxLightingScenarioDefinition> AllScenarios => Scenarios;

        public static ItemLightingBoardConfig CreateDefaultBoardConfig()
        {
            return new ItemLightingBoardConfig(
                ItemGridPlacementRulePreview.BoardSize,
                ItemGridPlacementRulePreview.EyeCell,
                ItemGridPlacementRulePreview.ArrayBonusCells);
        }

        public static ItemSandboxLightingScenarioBuild BuildScenario(ItemSandboxLightingScenarioDefinition scenario)
        {
            if (scenario == null)
            {
                return new ItemSandboxLightingScenarioBuild(null, Array.Empty<ItemGridPlacementEvaluation>(), Array.Empty<ItemLightingPlacedItem>(), new[] { "Missing scenario." });
            }

            List<ItemGridPlacementEvaluation> evaluations = new();
            List<ItemLightingPlacedItem> placedItems = new();
            List<string> errors = new();
            List<ItemGridPlacedItemPreview> lockedPlacements = new();

            foreach (ItemSandboxLightingScenarioPlacement placement in scenario.Placements)
            {
                ItemInnerDataDefinition item = ItemInnerDataCatalog.FindById(placement.itemId);
                if (item == null)
                {
                    errors.Add($"{scenario.scenarioId}: missing catalog item {placement.itemId}.");
                    continue;
                }

                ItemGridPlacementEvaluation evaluation = ItemGridPlacementRulePreview.Evaluate(
                    item,
                    placement.anchorCell,
                    lockedPlacements,
                    placement.placementId);
                evaluations.Add(evaluation);
                if (!evaluation.IsValid)
                {
                    errors.Add($"{scenario.scenarioId}: invalid placement {placement.itemId}/{placement.placementId} at {ItemGridPlacementRulePreview.FormatCell(placement.anchorCell)} -> {evaluation.invalidReason}.");
                    continue;
                }

                ItemGridPlacedItemPreview lockedPlacement = new(
                    item.itemId,
                    item.displayName,
                    placement.anchorCell,
                    evaluation.OccupiedCells,
                    evaluation.coreCellWorld,
                    placement.placementId);
                lockedPlacements.Add(lockedPlacement);
                placedItems.Add(new ItemLightingPlacedItem(
                    item.itemId,
                    item.displayName,
                    evaluation.OccupiedCells,
                    evaluation.coreCellWorld,
                    item.isLightingSource,
                    placement.anchorCell,
                    placement.placementId));
            }

            return new ItemSandboxLightingScenarioBuild(scenario, evaluations, placedItems, errors);
        }

        public static ItemLightingResolutionResult ResolveScenario(ItemSandboxLightingScenarioDefinition scenario)
        {
            ItemSandboxLightingScenarioBuild build = BuildScenario(scenario);
            return ItemLightingResolver.Resolve(CreateDefaultBoardConfig(), build.PlacedItems);
        }

        public static ItemSandboxLightingScenarioPlacement FindExpectedPlacement(
            ItemSandboxLightingScenarioDefinition scenario,
            string itemId)
        {
            return scenario?.Placements.FirstOrDefault(placement => string.Equals(placement.itemId, itemId, StringComparison.Ordinal));
        }

        public static ItemSandboxLightingScenarioPlacement FindExpectedPlacementByPlacementId(
            ItemSandboxLightingScenarioDefinition scenario,
            string placementId)
        {
            return scenario?.Placements.FirstOrDefault(placement => string.Equals(placement.placementId, placementId, StringComparison.Ordinal));
        }

        private static ItemSandboxLightingScenarioPlacement P(
            string itemId,
            int x,
            int y,
            bool expectedIsDirectLit,
            bool expectedIsLit,
            string expectedLitByItemId,
            int expectedLitDepth,
            string expectedResult)
        {
            return new ItemSandboxLightingScenarioPlacement(
                itemId,
                new Vector2Int(x, y),
                expectedIsDirectLit,
                expectedIsLit,
                expectedLitByItemId,
                expectedLitDepth,
                expectedResult);
        }
    }
}
