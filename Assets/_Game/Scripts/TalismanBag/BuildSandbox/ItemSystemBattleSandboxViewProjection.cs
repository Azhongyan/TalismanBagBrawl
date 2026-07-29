using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TalismanBag.ItemSandbox;
using TalismanBag.Items;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Detail;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Projection;
using TalismanBag.Items.InnerCatalog;
using UnityEngine;

namespace TalismanBag.BuildSandbox
{
    public sealed class ItemSystemBattleSandboxViewRow
    {
        private readonly ItemShapeCell[] shapeCells;
        private readonly ItemDetailViewModel baseDetailModel;

        internal ItemSystemBattleSandboxViewRow(
            int ordinal,
            ItemInnerDataDefinition catalogItem,
            long rootSeed,
            ItemInstanceProjectionContractSnapshot projection,
            ItemDetailViewModel baseDetailModel,
            Sprite ordinarySprite,
            Sprite systemUnlitSprite,
            Sprite systemLitSprite)
        {
            Ordinal = ordinal;
            BaseItemId = catalogItem?.itemId ?? string.Empty;
            DisplayName = catalogItem?.displayName ?? string.Empty;
            CategoryDisplayName = catalogItem?.FaMenDisplayName ?? string.Empty;
            ShapeId = catalogItem?.shapeId ?? string.Empty;
            shapeCells = (catalogItem?.ShapeCells ?? Array.Empty<Vector2Int>())
                .Select(cell => new ItemShapeCell(cell.x, cell.y))
                .ToArray();
            CoreCellLocal = catalogItem == null
                ? default
                : new ItemShapeCell(catalogItem.coreCellLocal.x, catalogItem.coreCellLocal.y);
            ShapeDisplayName = ShapeId + " · " + shapeCells.Length.ToString(CultureInfo.InvariantCulture) + "格";
            RotationAllowed = shapeCells.Length > 1 && shapeCells.Length < 4;
            RootSeed = rootSeed;
            Projection = projection;
            this.baseDetailModel = baseDetailModel?.Clone();
            OrdinarySprite = ordinarySprite;
            SystemUnlitSprite = systemUnlitSprite;
            SystemLitSprite = systemLitSprite;
        }

        public int Ordinal { get; }
        public string BaseItemId { get; }
        public string DisplayName { get; }
        public string CategoryDisplayName { get; }
        public string ShapeId { get; }
        public string ShapeDisplayName { get; }
        public ItemShapeCell CoreCellLocal { get; }
        public bool RotationAllowed { get; }
        public long RootSeed { get; }
        public ItemInstanceProjectionContractSnapshot Projection { get; }
        public string ItemInstanceId => Projection?.itemInstanceId ?? string.Empty;
        public bool IsSystemItem => string.Equals(BaseItemId, "I031", StringComparison.Ordinal);
        public IReadOnlyList<ItemShapeCell> ShapeCells => shapeCells;
        public Sprite OrdinarySprite { get; }
        public Sprite SystemUnlitSprite { get; }
        public Sprite SystemLitSprite { get; }

        public ItemDetailViewModel CreateBaseDetailModelClone()
        {
            return baseDetailModel?.Clone();
        }

        public Sprite ResolveSprite(bool lit)
        {
            return IsSystemItem
                ? (lit ? SystemLitSprite : SystemUnlitSprite)
                : OrdinarySprite;
        }
    }

    public sealed class ItemSystemBattleSandboxViewProjectionResult
    {
        internal ItemSystemBattleSandboxViewProjectionResult(
            IReadOnlyList<ItemSystemBattleSandboxViewRow> rows,
            IReadOnlyList<string> diagnostics)
        {
            Rows = (rows ?? Array.Empty<ItemSystemBattleSandboxViewRow>()).ToArray();
            Diagnostics = (diagnostics ?? Array.Empty<string>()).ToArray();
            OrdinaryProjectionSet = new ItemInstanceProjectionSetSnapshot(
                Rows.Where(row => row != null && !row.IsSystemItem)
                    .Select(row => row.Projection)
                    .Where(projection => projection != null)
                    .OrderBy(projection => projection.itemInstanceId,
                        StringComparer.Ordinal),
                Array.Empty<ItemInstanceProjectionValidationError>());
        }

        public bool IsValid => Rows.Count == 31
            && Diagnostics.Count == 0
            && OrdinaryProjectionSet.isValid
            && OrdinaryProjectionSet.Projections.Count == 30
            && OrdinaryProjectionSet.Projections.All(projection =>
                projection != null
                && !string.Equals(projection.baseItemId, "I031",
                    StringComparison.Ordinal));
        public IReadOnlyList<ItemSystemBattleSandboxViewRow> Rows { get; }
        public IReadOnlyList<string> Diagnostics { get; }
        public ItemInstanceProjectionSetSnapshot OrdinaryProjectionSet { get; }
    }

    public static class ItemSystemBattleSandboxViewProjection
    {
        public const long PackageSeed = 404310000L;
        public const int OrdinaryRarityIndex = 5;
        public const string OrdinaryRarityKey = "orange";
        public const string SystemItemId = "I031";

        public static ItemSystemBattleSandboxViewProjectionResult Build(
            ItemBalanceWorkbenchCatalog workbenchCatalog,
            IItemDetailViewModelProvider catalogProvider)
        {
            List<string> diagnostics = new();
            ItemInnerDataDefinition[] catalog = (ItemInnerDataCatalog.AllItems ??
                    Array.Empty<ItemInnerDataDefinition>())
                .Where(item => item != null)
                .OrderBy(item => item.itemId, StringComparer.Ordinal)
                .ToArray();
            string[] expected = Enumerable.Range(1, 31)
                .Select(index => "I" + index.ToString("000", CultureInfo.InvariantCulture))
                .ToArray();
            if (!catalog.Select(item => item.itemId).SequenceEqual(expected, StringComparer.Ordinal))
            {
                diagnostics.Add("ROSTER_NOT_EXACT_I001_I031");
                return new ItemSystemBattleSandboxViewProjectionResult(
                    Array.Empty<ItemSystemBattleSandboxViewRow>(), diagnostics);
            }

            if (workbenchCatalog == null)
            {
                diagnostics.Add("WORKBENCH_CATALOG_MISSING");
            }
            if (catalogProvider == null)
            {
                diagnostics.Add("ITEM_DETAIL_PROVIDER_MISSING");
            }
            if (diagnostics.Count > 0)
            {
                return new ItemSystemBattleSandboxViewProjectionResult(
                    Array.Empty<ItemSystemBattleSandboxViewRow>(), diagnostics);
            }

            ItemBalanceCandidateDetailSandboxAdapter candidateAdapter =
                new(workbenchCatalog, catalogProvider);
            List<ItemSystemBattleSandboxViewRow> rows = new();
            for (int index = 0; index < catalog.Length; index++)
            {
                ItemInnerDataDefinition item = catalog[index];
                int ordinal = index + 1;
                if (string.Equals(item.itemId, SystemItemId, StringComparison.Ordinal))
                {
                    ItemDetailViewModel baseDetailModel =
                        catalogProvider.GetDetailViewModel(SystemItemId);
                    if (baseDetailModel == null)
                    {
                        diagnostics.Add("DETAIL_MODEL_I031_MISSING");
                    }
                    Sprite unlit = Resources.Load<Sprite>(
                        "item_daoju/聚念石/聚念石_未激活");
                    Sprite lit = Resources.Load<Sprite>(
                        "item_daoju/聚念石/聚念石_激活");
                    if (unlit == null)
                    {
                        diagnostics.Add("ART_I031_UNLIT_MISSING");
                    }
                    if (lit == null)
                    {
                        diagnostics.Add("ART_I031_LIT_MISSING");
                    }
                    rows.Add(new ItemSystemBattleSandboxViewRow(
                        ordinal, item, 0L, null, baseDetailModel,
                        null, unlit, lit));
                    continue;
                }

                long rootSeed = PackageSeed + ordinal;
                ItemBalanceCandidateDetailResult candidate = candidateAdapter.Request(
                    new ItemBalanceCandidateDetailRequest
                    {
                        baseItemId = item.itemId,
                        rarityKey = OrdinaryRarityKey,
                        rootSeedText = rootSeed.ToString(CultureInfo.InvariantCulture)
                    });
                ItemInstanceProjectionContractSnapshot projection =
                    candidate?.preview?.ProjectionResult?.snapshot;
                if (candidate?.isSuccess != true || projection == null)
                {
                    diagnostics.Add("CANDIDATE_FAILED_" + item.itemId);
                }
                else if (projection.rarity != ItemInstanceRarity.Orange
                    || !string.Equals(projection.baseItemId, item.itemId, StringComparison.Ordinal)
                    || projection.rootSeed != rootSeed
                    || string.IsNullOrWhiteSpace(projection.itemInstanceId))
                {
                    diagnostics.Add("CANDIDATE_IDENTITY_INVALID_" + item.itemId);
                }
                if (candidate?.viewModel == null)
                {
                    diagnostics.Add("DETAIL_MODEL_MISSING_" + item.itemId);
                }

                string resourcePath = "item_daoju/" + item.FaMenDisplayName + "/"
                    + item.itemId + "/" + item.itemId + "_"
                    + OrdinaryRarityIndex.ToString(CultureInfo.InvariantCulture);
                Sprite sprite = Resources.Load<Sprite>(resourcePath);
                if (sprite == null)
                {
                    diagnostics.Add("ART_ORANGE_MISSING_" + item.itemId);
                }
                rows.Add(new ItemSystemBattleSandboxViewRow(
                    ordinal, item, rootSeed, projection, candidate?.viewModel,
                    sprite, null, null));
            }

            if (rows.Where(row => row != null && row.IsSystemItem).Count() != 1
                || rows.Where(row => row != null && !row.IsSystemItem
                    && row.Projection != null).Count() != 30)
            {
                diagnostics.Add("ROSTER_PROJECTION_CARDINALITY_INVALID");
            }
            return new ItemSystemBattleSandboxViewProjectionResult(rows, diagnostics);
        }

        public static BuildSandboxLayoutSnapshot ToCompatibilitySnapshot(
            ItemSystemSnapshot snapshot,
            IReadOnlyList<ItemSystemBattleSandboxViewRow> rows)
        {
            BuildSandboxLayoutSnapshot compatibility = new();
            if (snapshot == null || !snapshot.isValid)
            {
                return compatibility;
            }

            Dictionary<string, ItemSystemBattleSandboxViewRow> byId =
                (rows ?? Array.Empty<ItemSystemBattleSandboxViewRow>())
                .Where(row => row != null && !string.IsNullOrWhiteSpace(row.BaseItemId))
                .ToDictionary(row => row.BaseItemId, row => row, StringComparer.Ordinal);
            foreach (ItemSystemPlacementSnapshot placement in snapshot.placements
                         .Where(value => value != null)
                         .OrderBy(value => value.placementId, StringComparer.Ordinal))
            {
                if (!byId.TryGetValue(placement.itemId, out ItemSystemBattleSandboxViewRow row))
                {
                    continue;
                }
                ItemShapeRotation rotation = DegreesToRotation(placement.rotation);
                List<ItemShapeCell> cells = placement.OccupiedCells
                    .Select(cell => new ItemShapeCell(cell.x, cell.y))
                    .ToList();
                BuildSandboxPlacedItemSnapshot projected =
                    BattleSandboxBuildCombatPreviewBuilder.CreatePlacedItemSnapshot(
                        row.BaseItemId, row.ShapeId, rotation, cells);
                projected.energyState = placement.isLit
                    ? EnergyState.Powered
                    : EnergyState.None;
                projected.isPowered = placement.isLit;
                projected.energySourceId = placement.isLightingSource
                    ? placement.placementId
                    : placement.litByPlacementId;
                projected.formalEnergySourceItemId = placement.isLightingSource
                    ? placement.itemId
                    : placement.litByItemId;
                projected.energyStateReason = placement.isLightingSource
                    ? "item_system_v2_source"
                    : placement.isDirectLit
                        ? "item_system_v2_direct"
                        : placement.isLit
                            ? "item_system_v2_relay"
                            : "item_system_v2_unlit";
                projected.isEnergyStoneSource = placement.isLightingSource;
                projected.touchesFormationCore = placement.OccupiedCells.Contains(
                    snapshot.eyeCell);
                projected.formationCoreId = projected.touchesFormationCore
                    ? "item_system_v2_eye"
                    : string.Empty;
                projected.powerRangeCells = placement.isLightingSource
                    ? snapshot.LitRangeCells
                        .Select(cell => new ItemShapeCell(cell.x, cell.y))
                        .ToList()
                    : new List<ItemShapeCell>();
                projected.powerRangeRadius = projected.powerRangeCells.Count > 0 ? 1 : 0;
                projected.powerConnectionState = placement.isLit
                    ? "item_system_v2_connected"
                    : "item_system_v2_disconnected";
                compatibility.placedItems.Add(projected);
            }
            return compatibility;
        }

        public static int RotationToDegrees(ItemShapeRotation rotation)
        {
            return ((int)rotation) * 90;
        }

        public static ItemShapeRotation DegreesToRotation(int degrees)
        {
            int normalized = ((degrees % 360) + 360) % 360;
            return normalized switch
            {
                90 => ItemShapeRotation.Rotation90,
                180 => ItemShapeRotation.Rotation180,
                270 => ItemShapeRotation.Rotation270,
                _ => ItemShapeRotation.Rotation0
            };
        }
    }
}
