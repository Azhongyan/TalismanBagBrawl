using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using TalismanBag.Items.Awakening;
using TalismanBag.Items.Build;
using TalismanBag.Items.InnerCatalog;
using TalismanBag.Items.Lighting;
using TalismanBag.Items.Skills;
using UnityEngine;

namespace TalismanBag.Items
{
    internal static class ItemSystemSnapshotReadOnly
    {
        public static ReadOnlyCollection<T> Freeze<T>(IEnumerable<T> values)
        {
            return Array.AsReadOnly((values ?? Array.Empty<T>()).ToArray());
        }
    }

    public interface IItemSystemSnapshotProvider
    {
        ItemSystemSnapshot CreateSnapshot(ItemSystemSnapshotInput input);
    }

    public interface IItemSystemValidator
    {
        IReadOnlyList<ItemSystemValidationError> ValidateSnapshot(ItemSystemSnapshot snapshot);
    }

    public static class ItemSystemValidationCodes
    {
        public const string DuplicateBaseItemPlaced = "DUPLICATE_BASE_ITEM_PLACED";
    }

    public enum ItemSystemValidationSeverity
    {
        Info,
        Warning,
        Error
    }

    public sealed class ItemSystemValidationError
    {
        public ItemSystemValidationError(
            string code,
            ItemSystemValidationSeverity severity,
            string placementId,
            string itemId,
            string message)
        {
            this.code = code ?? string.Empty;
            this.severity = severity;
            this.placementId = placementId ?? string.Empty;
            this.itemId = itemId ?? string.Empty;
            this.message = message ?? string.Empty;
        }

        public string code { get; }
        public ItemSystemValidationSeverity severity { get; }
        public string placementId { get; }
        public string itemId { get; }
        public string message { get; }

        public string ToDiagnosticString()
        {
            return $"{severity}:{code}: placementId={Format(placementId)} itemId={Format(itemId)} {message}";
        }

        public static ItemSystemValidationError Error(string code, string placementId, string itemId, string message)
        {
            return new ItemSystemValidationError(code, ItemSystemValidationSeverity.Error, placementId, itemId, message);
        }

        private static string Format(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "None" : value;
        }
    }

    public sealed class ItemSystemBoardConfigInput
    {
        private readonly ReadOnlyCollection<Vector2Int> arrayBonusCells;

        public ItemSystemBoardConfigInput(
            int boardSize,
            Vector2Int eyeCell,
            IReadOnlyList<Vector2Int> arrayBonusCells)
        {
            this.boardSize = boardSize;
            this.eyeCell = eyeCell;
            this.arrayBonusCells = ItemSystemSnapshotReadOnly.Freeze(arrayBonusCells ?? Array.Empty<Vector2Int>());
        }

        public int boardSize { get; }
        public Vector2Int eyeCell { get; }
        public IReadOnlyList<Vector2Int> ArrayBonusCells => arrayBonusCells;

        public static ItemSystemBoardConfigInput Default()
        {
            return new ItemSystemBoardConfigInput(
                5,
                new Vector2Int(2, 2),
                new[]
                {
                    new Vector2Int(2, 1),
                    new Vector2Int(2, 3),
                    new Vector2Int(1, 2),
                    new Vector2Int(3, 2)
                });
        }
    }

    public sealed class ItemSystemPlacementInput
    {
        public ItemSystemPlacementInput(
            string placementId,
            string itemId,
            Vector2Int anchorCell,
            int rotation = 0)
        {
            this.placementId = placementId ?? string.Empty;
            this.itemId = itemId ?? string.Empty;
            this.anchorCell = anchorCell;
            this.rotation = rotation;
        }

        public string placementId { get; }
        public string itemId { get; }
        public Vector2Int anchorCell { get; }
        public int rotation { get; }
    }

    public sealed class ItemSystemSnapshotInput
    {
        private readonly ReadOnlyCollection<ItemSystemPlacementInput> placements;
        private readonly ReadOnlyCollection<ItemCoreAwakeningInput> awakeningInputs;
        private readonly ReadOnlyCollection<ItemInnerDataDefinition> catalogItems;

        public ItemSystemSnapshotInput(
            IReadOnlyList<ItemSystemPlacementInput> placements,
            ItemSystemBoardConfigInput boardConfig = null,
            IReadOnlyList<ItemCoreAwakeningInput> awakeningInputs = null,
            ItemMainBuildSelectionInput mainBuildSelectionInput = null,
            IReadOnlyList<ItemInnerDataDefinition> catalogItems = null)
        {
            this.placements = ItemSystemSnapshotReadOnly.Freeze((placements ?? Array.Empty<ItemSystemPlacementInput>())
                .Where(placement => placement != null)
                .Select(placement => new ItemSystemPlacementInput(
                    placement.placementId,
                    placement.itemId,
                    placement.anchorCell,
                    placement.rotation)));
            this.boardConfig = boardConfig ?? ItemSystemBoardConfigInput.Default();
            this.awakeningInputs = ItemSystemSnapshotReadOnly.Freeze((awakeningInputs ?? Array.Empty<ItemCoreAwakeningInput>())
                .Where(input => input != null)
                .Select(CloneAwakeningInput));
            this.mainBuildSelectionInput = mainBuildSelectionInput == null
                ? ItemMainBuildSelectionInput.None("ItemSystemSnapshotInputDefault")
                : new ItemMainBuildSelectionInput(
                    mainBuildSelectionInput.selectedMainBuildId,
                    mainBuildSelectionInput.selectionSource,
                    mainBuildSelectionInput.selectionRevision);
            this.catalogItems = ItemSystemSnapshotReadOnly.Freeze((catalogItems ?? ItemInnerDataCatalog.AllItems)
                .Where(item => item != null)
                .Select(CloneCatalogItem));
        }

        public IReadOnlyList<ItemSystemPlacementInput> Placements => placements;
        public ItemSystemBoardConfigInput boardConfig { get; }
        public IReadOnlyList<ItemCoreAwakeningInput> AwakeningInputs => awakeningInputs;
        public ItemMainBuildSelectionInput mainBuildSelectionInput { get; }
        public IReadOnlyList<ItemInnerDataDefinition> CatalogItems => catalogItems;

        private static ItemCoreAwakeningInput CloneAwakeningInput(ItemCoreAwakeningInput input)
        {
            return new ItemCoreAwakeningInput(
                input.itemId,
                input.placementId,
                input.inputLevel,
                input.highRarityUltimatePreview,
                input.inputSource,
                input.requiredRarityKey);
        }

        internal static ItemInnerDataDefinition CloneCatalogItem(ItemInnerDataDefinition source)
        {
            if (source == null)
            {
                return null;
            }

            return new ItemInnerDataDefinition
            {
                itemId = source.itemId ?? string.Empty,
                displayName = source.displayName ?? string.Empty,
                itemFamily = source.itemFamily,
                faMenTag = source.faMenTag,
                qiLeiTag = source.qiLeiTag,
                shapeId = source.shapeId ?? string.Empty,
                defaultLocalCells = (source.defaultLocalCells ?? new List<Vector2Int>()).ToList(),
                coreCellLocal = source.coreCellLocal,
                displayRarityName = source.displayRarityName ?? string.Empty,
                rarityDefault = source.rarityDefault,
                allowedRarities = (source.allowedRarities ?? new List<ItemCatalogRarity>()).ToList(),
                basePowerText = source.basePowerText ?? string.Empty,
                itemPower = source.itemPower ?? string.Empty,
                primaryStats = (source.primaryStats ?? new List<ItemInnerStatLine>())
                    .Where(stat => stat != null)
                    .Select(stat => new ItemInnerStatLine(stat.label, stat.value, stat.hint))
                    .ToList(),
                triggerText = source.triggerText ?? string.Empty,
                basicEffectText = source.basicEffectText ?? string.Empty,
                coreEffectPreviewText = source.coreEffectPreviewText ?? string.Empty,
                awakeningPreview = source.awakeningPreview ?? string.Empty,
                fixedAffixPreview = source.fixedAffixPreview ?? string.Empty,
                randomAffixPreview = source.randomAffixPreview ?? string.Empty,
                orangeAffixPreview = source.orangeAffixPreview ?? string.Empty,
                placementHint = source.placementHint ?? string.Empty,
                flavorText = source.flavorText ?? string.Empty,
                iconPlaceholderKey = source.iconPlaceholderKey ?? string.Empty,
                isLightingSource = source.isLightingSource
            };
        }
    }

    public sealed class ItemSystemCatalogItemSnapshot
    {
        private readonly ReadOnlyCollection<Vector2Int> shapeCells;

        public ItemSystemCatalogItemSnapshot(ItemInnerDataDefinition item)
        {
            itemId = item?.itemId ?? string.Empty;
            displayName = item?.displayName ?? string.Empty;
            itemFamily = item?.ItemFamilyKey ?? string.Empty;
            faMenTag = item?.FaMenKey ?? string.Empty;
            qiLeiTag = item?.QiLeiKey ?? string.Empty;
            shapeId = item?.shapeId ?? string.Empty;
            shapeCells = ItemSystemSnapshotReadOnly.Freeze((item?.ShapeCells ?? Array.Empty<Vector2Int>()).OrderByCell());
            coreCellLocal = item?.coreCellLocal ?? Vector2Int.zero;
            isLightingSource = item?.isLightingSource == true;
        }

        public string itemId { get; }
        public string displayName { get; }
        public string itemFamily { get; }
        public string faMenTag { get; }
        public string qiLeiTag { get; }
        public string shapeId { get; }
        public Vector2Int coreCellLocal { get; }
        public bool isLightingSource { get; }
        public IReadOnlyList<Vector2Int> ShapeCells => shapeCells;
    }

    public sealed class ItemSystemPlacementSnapshot
    {
        private readonly ReadOnlyCollection<Vector2Int> occupiedCells;
        private readonly ReadOnlyCollection<Vector2Int> occupiedArrayBonusCells;
        private readonly ReadOnlyCollection<string> unlockedCoreEffectIds;
        private readonly ReadOnlyCollection<string> activeCoreEffectIds;

        public ItemSystemPlacementSnapshot(
            string placementId,
            string itemId,
            string displayName,
            Vector2Int anchorCell,
            int rotation,
            IReadOnlyList<Vector2Int> occupiedCells,
            Vector2Int coreCellWorld,
            bool isLightingSource,
            bool isDirectLit,
            bool isLit,
            string litByItemId,
            string litByPlacementId,
            int litDepth,
            IReadOnlyList<Vector2Int> occupiedArrayBonusCells,
            bool isOnArrayBonusCell,
            bool isArrayBonusActive,
            bool isCountedInBuild,
            int inputLevel,
            int resolvedLevel,
            IReadOnlyList<string> unlockedCoreEffectIds,
            IReadOnlyList<string> activeCoreEffectIds)
        {
            this.placementId = placementId ?? string.Empty;
            this.itemId = itemId ?? string.Empty;
            this.displayName = displayName ?? string.Empty;
            this.anchorCell = anchorCell;
            this.rotation = rotation;
            this.occupiedCells = ItemSystemSnapshotReadOnly.Freeze((occupiedCells ?? Array.Empty<Vector2Int>()).OrderByCell());
            this.coreCellWorld = coreCellWorld;
            this.isLightingSource = isLightingSource;
            this.isDirectLit = isDirectLit;
            this.isLit = isLit;
            this.litByItemId = litByItemId ?? string.Empty;
            this.litByPlacementId = litByPlacementId ?? string.Empty;
            this.litDepth = litDepth;
            this.occupiedArrayBonusCells = ItemSystemSnapshotReadOnly.Freeze((occupiedArrayBonusCells ?? Array.Empty<Vector2Int>()).OrderByCell());
            this.isOnArrayBonusCell = isOnArrayBonusCell;
            this.isArrayBonusActive = isArrayBonusActive;
            this.isCountedInBuild = isCountedInBuild;
            this.inputLevel = inputLevel;
            this.resolvedLevel = resolvedLevel;
            this.unlockedCoreEffectIds = ItemSystemSnapshotReadOnly.Freeze((unlockedCoreEffectIds ?? Array.Empty<string>()).OrderByText());
            this.activeCoreEffectIds = ItemSystemSnapshotReadOnly.Freeze((activeCoreEffectIds ?? Array.Empty<string>()).OrderByText());
        }

        public string placementId { get; }
        public string itemId { get; }
        public string displayName { get; }
        public Vector2Int anchorCell { get; }
        public int rotation { get; }
        public IReadOnlyList<Vector2Int> OccupiedCells => occupiedCells;
        public Vector2Int coreCellWorld { get; }
        public bool isLightingSource { get; }
        public bool isDirectLit { get; }
        public bool isLit { get; }
        public string litByItemId { get; }
        public string litByPlacementId { get; }
        public int litDepth { get; }
        public IReadOnlyList<Vector2Int> OccupiedArrayBonusCells => occupiedArrayBonusCells;
        public bool isOnArrayBonusCell { get; }
        public bool isArrayBonusActive { get; }
        public bool isCountedInBuild { get; }
        public int inputLevel { get; }
        public int resolvedLevel { get; }
        public IReadOnlyList<string> UnlockedCoreEffectIds => unlockedCoreEffectIds;
        public IReadOnlyList<string> ActiveCoreEffectIds => activeCoreEffectIds;
    }

    public sealed class ItemSystemLightingResultSnapshot
    {
        private readonly ReadOnlyCollection<Vector2Int> occupiedCells;

        public ItemSystemLightingResultSnapshot(ItemLightingItemResult item)
        {
            placementId = item?.placementId ?? string.Empty;
            itemId = item?.itemId ?? string.Empty;
            displayName = item?.displayName ?? string.Empty;
            occupiedCells = ItemSystemSnapshotReadOnly.Freeze((item?.OccupiedCells ?? Array.Empty<Vector2Int>()).OrderByCell());
            coreCellWorld = item?.coreCellWorld ?? Vector2Int.zero;
            isLightingSource = item?.isLightingSource == true;
            isDirectLit = item?.isDirectLit == true;
            isLit = item?.isLit == true;
            litByItemId = item?.litByItemId ?? string.Empty;
            litByPlacementId = item?.litByPlacementId ?? string.Empty;
            litDepth = item?.litDepth ?? -1;
        }

        public string placementId { get; }
        public string itemId { get; }
        public string displayName { get; }
        public IReadOnlyList<Vector2Int> OccupiedCells => occupiedCells;
        public Vector2Int coreCellWorld { get; }
        public bool isLightingSource { get; }
        public bool isDirectLit { get; }
        public bool isLit { get; }
        public string litByItemId { get; }
        public string litByPlacementId { get; }
        public int litDepth { get; }
    }

    public sealed class ItemSystemArrayBonusResultSnapshot
    {
        private readonly ReadOnlyCollection<Vector2Int> occupiedCells;
        private readonly ReadOnlyCollection<Vector2Int> occupiedArrayBonusCells;
        private readonly ReadOnlyCollection<string> occupiedArrayBonusCellIds;

        public ItemSystemArrayBonusResultSnapshot(ItemArrayBonusItemResult item)
        {
            placementId = item?.placementId ?? string.Empty;
            itemId = item?.itemId ?? string.Empty;
            displayName = item?.displayName ?? string.Empty;
            occupiedCells = ItemSystemSnapshotReadOnly.Freeze((item?.OccupiedCells ?? Array.Empty<Vector2Int>()).OrderByCell());
            occupiedArrayBonusCells = ItemSystemSnapshotReadOnly.Freeze((item?.OccupiedArrayBonusCells ?? Array.Empty<Vector2Int>()).OrderByCell());
            occupiedArrayBonusCellIds = ItemSystemSnapshotReadOnly.Freeze((item?.OccupiedArrayBonusCellIds ?? Array.Empty<string>()).OrderByText());
            isLit = item?.isLit == true;
            isLightingSource = item?.isLightingSource == true;
            isOnArrayBonusCell = item?.isOnArrayBonusCell == true;
            isArrayBonusActive = item?.isArrayBonusActive == true;
        }

        public string placementId { get; }
        public string itemId { get; }
        public string displayName { get; }
        public IReadOnlyList<Vector2Int> OccupiedCells => occupiedCells;
        public IReadOnlyList<Vector2Int> OccupiedArrayBonusCells => occupiedArrayBonusCells;
        public IReadOnlyList<string> OccupiedArrayBonusCellIds => occupiedArrayBonusCellIds;
        public bool isLit { get; }
        public bool isLightingSource { get; }
        public bool isOnArrayBonusCell { get; }
        public bool isArrayBonusActive { get; }
    }

    public sealed class ItemSystemBuildTrackSnapshot
    {
        private readonly ReadOnlyCollection<string> sourcePlacementIds;
        private readonly ReadOnlyCollection<string> sourceItemIds;

        public ItemSystemBuildTrackSnapshot(ItemBuildTrackResult track)
        {
            buildId = track?.buildId ?? string.Empty;
            trackKind = track?.trackKind ?? ItemBuildTrackKind.FaMen;
            stableTag = track?.stableTag ?? string.Empty;
            displayName = track?.displayName ?? string.Empty;
            litItemCount = track?.litItemCount ?? 0;
            maxPieceCount = track?.maxPieceCount ?? 1;
            activeStagePieceCount = track?.activeStagePieceCount ?? 0;
            nextStagePieceCount = track?.nextStagePieceCount ?? 0;
            sourcePlacementIds = ItemSystemSnapshotReadOnly.Freeze((track?.SourcePlacementIds ?? Array.Empty<string>()).OrderByText());
            sourceItemIds = ItemSystemSnapshotReadOnly.Freeze((track?.SourceItemIds ?? Array.Empty<string>()).OrderByText());
        }

        public string buildId { get; }
        public ItemBuildTrackKind trackKind { get; }
        public string stableTag { get; }
        public string displayName { get; }
        public int litItemCount { get; }
        public int maxPieceCount { get; }
        public int activeStagePieceCount { get; }
        public int nextStagePieceCount { get; }
        public IReadOnlyList<string> SourcePlacementIds => sourcePlacementIds;
        public IReadOnlyList<string> SourceItemIds => sourceItemIds;
    }

    public sealed class ItemSystemBuildItemSnapshot
    {
        public ItemSystemBuildItemSnapshot(ItemBuildSynergyItemResult item)
        {
            itemId = item?.itemId ?? string.Empty;
            placementId = item?.placementId ?? string.Empty;
            isLit = item?.isLit == true;
            isLightingSource = item?.isLightingSource == true;
            countedInBuild = item?.countedInBuild == true;
            faMenTag = item?.faMenTag ?? string.Empty;
            qiLeiTag = item?.qiLeiTag ?? string.Empty;
            faMenBuildId = item?.faMenBuildId ?? string.Empty;
            qiLeiBuildId = item?.qiLeiBuildId ?? string.Empty;
            faMenBuildCount = item?.faMenBuildCount ?? 0;
            qiLeiBuildCount = item?.qiLeiBuildCount ?? 0;
            faMenActiveStagePieceCount = item?.faMenActiveStagePieceCount ?? 0;
            qiLeiActiveStagePieceCount = item?.qiLeiActiveStagePieceCount ?? 0;
        }

        public string itemId { get; }
        public string placementId { get; }
        public bool isLit { get; }
        public bool isLightingSource { get; }
        public bool countedInBuild { get; }
        public string faMenTag { get; }
        public string qiLeiTag { get; }
        public string faMenBuildId { get; }
        public string qiLeiBuildId { get; }
        public int faMenBuildCount { get; }
        public int qiLeiBuildCount { get; }
        public int faMenActiveStagePieceCount { get; }
        public int qiLeiActiveStagePieceCount { get; }
    }

    public sealed class ItemSystemBuildSnapshot
    {
        private readonly ReadOnlyCollection<ItemSystemBuildTrackSnapshot> faMenBuilds;
        private readonly ReadOnlyCollection<ItemSystemBuildTrackSnapshot> qiLeiBuilds;
        private readonly ReadOnlyCollection<ItemSystemBuildItemSnapshot> itemResults;
        private readonly ReadOnlyCollection<string> validationErrors;

        public ItemSystemBuildSnapshot(ItemBuildSynergyResolutionResult result)
        {
            faMenBuilds = ItemSystemSnapshotReadOnly.Freeze((result?.FaMenBuilds ?? Array.Empty<ItemBuildTrackResult>())
                .Select(track => new ItemSystemBuildTrackSnapshot(track))
                .OrderBy(track => track.stableTag, StringComparer.Ordinal));
            qiLeiBuilds = ItemSystemSnapshotReadOnly.Freeze((result?.QiLeiBuilds ?? Array.Empty<ItemBuildTrackResult>())
                .Select(track => new ItemSystemBuildTrackSnapshot(track))
                .OrderBy(track => track.stableTag, StringComparer.Ordinal));
            itemResults = ItemSystemSnapshotReadOnly.Freeze((result?.ItemResults ?? Array.Empty<ItemBuildSynergyItemResult>())
                .Select(item => new ItemSystemBuildItemSnapshot(item))
                .OrderBy(item => item.placementId, StringComparer.Ordinal)
                .ThenBy(item => item.itemId, StringComparer.Ordinal));
            validationErrors = ItemSystemSnapshotReadOnly.Freeze((result?.ValidationErrors ?? Array.Empty<string>()).OrderByText());
            selectedMainBuildId = result?.selectedMainBuildId ?? string.Empty;
        }

        public IReadOnlyList<ItemSystemBuildTrackSnapshot> FaMenBuilds => faMenBuilds;
        public IReadOnlyList<ItemSystemBuildTrackSnapshot> QiLeiBuilds => qiLeiBuilds;
        public IReadOnlyList<ItemSystemBuildItemSnapshot> ItemResults => itemResults;
        public IReadOnlyList<string> ValidationErrors => validationErrors;
        public string selectedMainBuildId { get; }

        public ItemSystemBuildTrackSnapshot FindFaMenBuild(string buildId)
        {
            return faMenBuilds.FirstOrDefault(track => string.Equals(track.buildId, buildId, StringComparison.Ordinal));
        }

        public ItemSystemBuildItemSnapshot FindPlacementResult(string placementId)
        {
            return itemResults.FirstOrDefault(item => string.Equals(item.placementId, placementId, StringComparison.Ordinal));
        }
    }

    public sealed class ItemSystemAwakeningNodeSnapshot
    {
        public ItemSystemAwakeningNodeSnapshot(ItemCoreAwakeningNodeState node)
        {
            itemId = node?.itemId ?? string.Empty;
            coreEffectId = node?.coreEffectId ?? string.Empty;
            nodeKind = node?.nodeKind ?? ItemCoreAwakeningNodeKind.Core1;
            unlockLevel = node?.unlockLevel ?? 0;
            isUnlocked = node?.isUnlocked == true;
            isActive = node?.isActive == true;
            stateKey = node?.stateKey ?? string.Empty;
            blockedReason = node?.blockedReason ?? string.Empty;
            previewText = node?.previewText ?? string.Empty;
            requiredRarityKey = node?.requiredRarityKey ?? string.Empty;
        }

        public string itemId { get; }
        public string coreEffectId { get; }
        public ItemCoreAwakeningNodeKind nodeKind { get; }
        public int unlockLevel { get; }
        public bool isUnlocked { get; }
        public bool isActive { get; }
        public string stateKey { get; }
        public string blockedReason { get; }
        public string previewText { get; }
        public string requiredRarityKey { get; }
    }

    public sealed class ItemSystemAwakeningResultSnapshot
    {
        private readonly ReadOnlyCollection<ItemSystemAwakeningNodeSnapshot> nodeStates;
        private readonly ReadOnlyCollection<string> validationErrors;
        private readonly ReadOnlyCollection<string> unlockedCoreEffectIds;
        private readonly ReadOnlyCollection<string> activeCoreEffectIds;

        public ItemSystemAwakeningResultSnapshot(ItemCoreAwakeningItemResult item)
        {
            itemId = item?.itemId ?? string.Empty;
            placementId = item?.placementId ?? string.Empty;
            inputLevel = item?.inputLevel ?? 1;
            resolvedLevel = item?.resolvedLevel ?? 1;
            highRarityUltimatePreview = item?.highRarityUltimatePreview == true;
            requiredRarityKey = item?.requiredRarityKey ?? string.Empty;
            isLit = item?.isLit == true;
            isLightingSource = item?.isLightingSource == true;
            supportsAwakening = item?.supportsAwakening == true;
            basicEffectActive = item?.basicEffectActive == true;
            coreEffectUnlocked = item?.coreEffectUnlocked == true;
            coreEffectActive = item?.coreEffectActive == true;
            inputSource = item?.inputSource ?? string.Empty;
            nodeStates = ItemSystemSnapshotReadOnly.Freeze((item?.NodeStates ?? Array.Empty<ItemCoreAwakeningNodeState>())
                .Select(node => new ItemSystemAwakeningNodeSnapshot(node))
                .OrderBy(node => node.unlockLevel)
                .ThenBy(node => node.coreEffectId, StringComparer.Ordinal));
            validationErrors = ItemSystemSnapshotReadOnly.Freeze((item?.ValidationErrors ?? Array.Empty<string>()).OrderByText());
            unlockedCoreEffectIds = ItemSystemSnapshotReadOnly.Freeze((item?.UnlockedCoreEffectIds ?? Array.Empty<string>()).OrderByText());
            activeCoreEffectIds = ItemSystemSnapshotReadOnly.Freeze((item?.ActiveCoreEffectIds ?? Array.Empty<string>()).OrderByText());
        }

        public string itemId { get; }
        public string placementId { get; }
        public int inputLevel { get; }
        public int resolvedLevel { get; }
        public bool highRarityUltimatePreview { get; }
        public string requiredRarityKey { get; }
        public bool isLit { get; }
        public bool isLightingSource { get; }
        public bool supportsAwakening { get; }
        public bool basicEffectActive { get; }
        public bool coreEffectUnlocked { get; }
        public bool coreEffectActive { get; }
        public string inputSource { get; }
        public IReadOnlyList<ItemSystemAwakeningNodeSnapshot> NodeStates => nodeStates;
        public IReadOnlyList<string> ValidationErrors => validationErrors;
        public IReadOnlyList<string> UnlockedCoreEffectIds => unlockedCoreEffectIds;
        public IReadOnlyList<string> ActiveCoreEffectIds => activeCoreEffectIds;
    }

    public sealed class ItemSystemSkillMonitorSlotSnapshot
    {
        public ItemSystemSkillMonitorSlotSnapshot(ItemSkillMonitorSlotSnapshot slot)
        {
            slotType = slot?.slotType ?? ItemSkillMonitorSlotType.BasicAttack;
            slotOrder = slot?.slotOrder ?? 0;
            displayName = slot?.displayName ?? string.Empty;
            sourceBuildId = slot?.sourceBuildId ?? string.Empty;
            sourceFaMenTag = slot?.sourceFaMenTag ?? string.Empty;
            requiredBuildStage = slot?.requiredBuildStage ?? 0;
            triggerKind = slot?.triggerKind ?? ItemSkillTriggerKind.PassiveTick;
            isUnlocked = slot?.isUnlocked == true;
            isMonitoring = slot?.isMonitoring == true;
            isTriggered = slot?.isTriggered == true;
            cooldown01 = slot?.cooldown01 ?? 0f;
            charge01 = slot?.charge01 ?? 0f;
            iconKey = slot?.iconKey ?? string.Empty;
            tooltipText = slot?.tooltipText ?? string.Empty;
            presentationCueId = slot?.presentationCueId ?? string.Empty;
            chibiActionKey = slot?.chibiActionKey ?? string.Empty;
        }

        public ItemSkillMonitorSlotType slotType { get; }
        public int slotOrder { get; }
        public string displayName { get; }
        public string sourceBuildId { get; }
        public string sourceFaMenTag { get; }
        public int requiredBuildStage { get; }
        public ItemSkillTriggerKind triggerKind { get; }
        public bool isUnlocked { get; }
        public bool isMonitoring { get; }
        public bool isTriggered { get; }
        public float cooldown01 { get; }
        public float charge01 { get; }
        public string iconKey { get; }
        public string tooltipText { get; }
        public string presentationCueId { get; }
        public string chibiActionKey { get; }
    }

    public sealed class ItemSystemSkillMonitorSnapshot
    {
        private readonly ReadOnlyCollection<ItemSystemSkillMonitorSlotSnapshot> slots;
        private readonly ReadOnlyCollection<string> validationErrors;

        public ItemSystemSkillMonitorSnapshot(ItemSkillMonitorResolutionResult result)
        {
            selectedMainBuildId = result?.selectedMainBuildId ?? string.Empty;
            selectedMainBuildFound = result?.selectedMainBuildFound == true;
            selectedMainBuildCount = result?.selectedMainBuildCount ?? 0;
            selectedMainBuildStage = result?.selectedMainBuildStage ?? 0;
            slots = ItemSystemSnapshotReadOnly.Freeze((result?.Slots ?? Array.Empty<ItemSkillMonitorSlotSnapshot>())
                .Select(slot => new ItemSystemSkillMonitorSlotSnapshot(slot))
                .OrderBy(slot => slot.slotOrder));
            validationErrors = ItemSystemSnapshotReadOnly.Freeze((result?.ValidationErrors ?? Array.Empty<string>()).OrderByText());
        }

        public string selectedMainBuildId { get; }
        public bool selectedMainBuildFound { get; }
        public int selectedMainBuildCount { get; }
        public int selectedMainBuildStage { get; }
        public IReadOnlyList<ItemSystemSkillMonitorSlotSnapshot> Slots => slots;
        public IReadOnlyList<string> ValidationErrors => validationErrors;
    }

    public sealed class ItemSystemSnapshot
    {
        public const string CurrentSchemaVersion = "ItemSystemSnapshot.v1";

        private readonly ReadOnlyCollection<Vector2Int> arrayBonusCells;
        private readonly ReadOnlyCollection<ItemSystemCatalogItemSnapshot> catalogItemSnapshots;
        private readonly ReadOnlyCollection<ItemSystemPlacementSnapshot> placementSnapshots;
        private readonly ReadOnlyCollection<ItemSystemLightingResultSnapshot> lightingResultSnapshots;
        private readonly ReadOnlyCollection<ItemSystemArrayBonusResultSnapshot> arrayBonusResultSnapshots;
        private readonly ReadOnlyCollection<ItemSystemAwakeningResultSnapshot> awakeningResultSnapshots;
        private readonly ReadOnlyCollection<ItemSystemValidationError> validationErrorSnapshots;
        private readonly ReadOnlyCollection<Vector2Int> litRangeCells;

        public ItemSystemSnapshot(
            int boardSize,
            Vector2Int eyeCell,
            IReadOnlyList<Vector2Int> arrayBonusCells,
            IReadOnlyList<ItemSystemCatalogItemSnapshot> catalogItems,
            IReadOnlyList<ItemSystemPlacementSnapshot> placements,
            IReadOnlyList<Vector2Int> litRangeCells,
            IReadOnlyList<ItemSystemLightingResultSnapshot> lightingResults,
            IReadOnlyList<ItemSystemArrayBonusResultSnapshot> arrayBonusResults,
            ItemSystemBuildSnapshot buildSnapshot,
            IReadOnlyList<ItemSystemAwakeningResultSnapshot> awakeningResults,
            ItemSystemSkillMonitorSnapshot skillMonitorSnapshot,
            string selectedMainBuildId,
            bool selectedMainBuildIsExplicit,
            string selectedMainBuildSource,
            IReadOnlyList<ItemSystemValidationError> validationErrors)
        {
            schemaVersion = CurrentSchemaVersion;
            this.boardSize = boardSize;
            this.eyeCell = eyeCell;
            this.arrayBonusCells = ItemSystemSnapshotReadOnly.Freeze((arrayBonusCells ?? Array.Empty<Vector2Int>()).OrderByCell());
            catalogItemSnapshots = ItemSystemSnapshotReadOnly.Freeze((catalogItems ?? Array.Empty<ItemSystemCatalogItemSnapshot>())
                .OrderBy(item => item.itemId, StringComparer.Ordinal)
                .ThenBy(item => item.displayName, StringComparer.Ordinal));
            placementSnapshots = ItemSystemSnapshotReadOnly.Freeze((placements ?? Array.Empty<ItemSystemPlacementSnapshot>())
                .OrderBy(placement => placement.placementId, StringComparer.Ordinal)
                .ThenBy(placement => placement.itemId, StringComparer.Ordinal)
                .ThenBy(placement => placement.anchorCell.y)
                .ThenBy(placement => placement.anchorCell.x));
            this.litRangeCells = ItemSystemSnapshotReadOnly.Freeze((litRangeCells ?? Array.Empty<Vector2Int>()).Distinct().OrderByCell());
            lightingResultSnapshots = ItemSystemSnapshotReadOnly.Freeze((lightingResults ?? Array.Empty<ItemSystemLightingResultSnapshot>())
                .OrderBy(item => item.placementId, StringComparer.Ordinal)
                .ThenBy(item => item.itemId, StringComparer.Ordinal));
            arrayBonusResultSnapshots = ItemSystemSnapshotReadOnly.Freeze((arrayBonusResults ?? Array.Empty<ItemSystemArrayBonusResultSnapshot>())
                .OrderBy(item => item.placementId, StringComparer.Ordinal)
                .ThenBy(item => item.itemId, StringComparer.Ordinal));
            this.buildSnapshot = buildSnapshot ?? new ItemSystemBuildSnapshot(null);
            awakeningResultSnapshots = ItemSystemSnapshotReadOnly.Freeze((awakeningResults ?? Array.Empty<ItemSystemAwakeningResultSnapshot>())
                .OrderBy(item => item.placementId, StringComparer.Ordinal)
                .ThenBy(item => item.itemId, StringComparer.Ordinal));
            this.skillMonitorSnapshot = skillMonitorSnapshot ?? new ItemSystemSkillMonitorSnapshot(null);
            this.selectedMainBuildId = selectedMainBuildId ?? string.Empty;
            this.selectedMainBuildIsExplicit = selectedMainBuildIsExplicit;
            this.selectedMainBuildSource = selectedMainBuildSource ?? string.Empty;
            validationErrorSnapshots = ItemSystemSnapshotReadOnly.Freeze(NormalizeErrors(validationErrors));
        }

        public string schemaVersion { get; }
        public bool isValid => validationErrorSnapshots.Count == 0;
        public int boardSize { get; }
        public Vector2Int eyeCell { get; }
        public IReadOnlyList<Vector2Int> ArrayBonusCells => arrayBonusCells;
        public IReadOnlyList<ItemSystemCatalogItemSnapshot> catalogItems => catalogItemSnapshots;
        public IReadOnlyList<ItemSystemPlacementSnapshot> placements => placementSnapshots;
        public IReadOnlyList<Vector2Int> LitRangeCells => litRangeCells;
        public IReadOnlyList<ItemSystemLightingResultSnapshot> lightingResults => lightingResultSnapshots;
        public IReadOnlyList<ItemSystemArrayBonusResultSnapshot> arrayBonusResults => arrayBonusResultSnapshots;
        public ItemSystemBuildSnapshot buildSnapshot { get; }
        public IReadOnlyList<ItemSystemAwakeningResultSnapshot> awakeningResults => awakeningResultSnapshots;
        public ItemSystemSkillMonitorSnapshot skillMonitorSnapshot { get; }
        public string selectedMainBuildId { get; }
        public bool selectedMainBuildIsExplicit { get; }
        public string selectedMainBuildSource { get; }
        public IReadOnlyList<ItemSystemValidationError> validationErrors => validationErrorSnapshots;

        public ItemSystemPlacementSnapshot FindPlacement(string placementId)
        {
            return placements.FirstOrDefault(placement => string.Equals(placement.placementId, placementId, StringComparison.Ordinal));
        }

        public string BuildDebugSignature()
        {
            StringBuilder builder = new();
            builder.Append("SN|")
                .Append(schemaVersion).Append('|')
                .Append(FormatBool(isValid)).Append('|')
                .Append(boardSize.ToString(CultureInfo.InvariantCulture)).Append('|')
                .Append(FormatCell(eyeCell)).Append('|')
                .Append(FormatCells(arrayBonusCells)).Append('|')
                .Append(FormatCells(litRangeCells)).Append('|')
                .Append(selectedMainBuildId).Append('|')
                .Append(FormatBool(selectedMainBuildIsExplicit)).Append('|')
                .Append(selectedMainBuildSource);

            foreach (ItemSystemCatalogItemSnapshot item in catalogItems)
            {
                builder.Append("\nC:")
                    .Append(item.itemId).Append('|')
                    .Append(item.displayName).Append('|')
                    .Append(item.itemFamily).Append('|')
                    .Append(item.faMenTag).Append('|')
                    .Append(item.qiLeiTag).Append('|')
                    .Append(item.shapeId).Append('|')
                    .Append(FormatCells(item.ShapeCells)).Append('|')
                    .Append(FormatCell(item.coreCellLocal)).Append('|')
                    .Append(FormatBool(item.isLightingSource));
            }

            foreach (ItemSystemPlacementSnapshot placement in placements)
            {
                builder.Append("\nP:")
                    .Append(placement.placementId).Append('|')
                    .Append(placement.itemId).Append('|')
                    .Append(placement.displayName).Append('|')
                    .Append(FormatCell(placement.anchorCell)).Append('|')
                    .Append(placement.rotation.ToString(CultureInfo.InvariantCulture)).Append('|')
                    .Append(FormatCells(placement.OccupiedCells)).Append('|')
                    .Append(FormatCell(placement.coreCellWorld)).Append('|')
                    .Append(FormatBool(placement.isLightingSource)).Append('|')
                    .Append(FormatBool(placement.isDirectLit)).Append('|')
                    .Append(FormatBool(placement.isLit)).Append('|')
                    .Append(placement.litByItemId).Append('|')
                    .Append(placement.litByPlacementId).Append('|')
                    .Append(placement.litDepth.ToString(CultureInfo.InvariantCulture)).Append('|')
                    .Append(FormatCells(placement.OccupiedArrayBonusCells)).Append('|')
                    .Append(FormatBool(placement.isOnArrayBonusCell)).Append('|')
                    .Append(FormatBool(placement.isArrayBonusActive)).Append('|')
                    .Append(FormatBool(placement.isCountedInBuild)).Append('|')
                    .Append(placement.inputLevel.ToString(CultureInfo.InvariantCulture)).Append('|')
                    .Append(placement.resolvedLevel.ToString(CultureInfo.InvariantCulture)).Append('|')
                    .Append(FormatTextList(placement.UnlockedCoreEffectIds)).Append('|')
                    .Append(FormatTextList(placement.ActiveCoreEffectIds));
            }

            foreach (ItemSystemLightingResultSnapshot item in lightingResults)
            {
                builder.Append("\nL:")
                    .Append(item.placementId).Append('|')
                    .Append(item.itemId).Append('|')
                    .Append(item.displayName).Append('|')
                    .Append(FormatCells(item.OccupiedCells)).Append('|')
                    .Append(FormatCell(item.coreCellWorld)).Append('|')
                    .Append(FormatBool(item.isLightingSource)).Append('|')
                    .Append(FormatBool(item.isDirectLit)).Append('|')
                    .Append(FormatBool(item.isLit)).Append('|')
                    .Append(item.litByItemId).Append('|')
                    .Append(item.litByPlacementId).Append('|')
                    .Append(item.litDepth.ToString(CultureInfo.InvariantCulture));
            }

            foreach (ItemSystemArrayBonusResultSnapshot item in arrayBonusResults)
            {
                builder.Append("\nA:")
                    .Append(item.placementId).Append('|')
                    .Append(item.itemId).Append('|')
                    .Append(item.displayName).Append('|')
                    .Append(FormatCells(item.OccupiedCells)).Append('|')
                    .Append(FormatCells(item.OccupiedArrayBonusCells)).Append('|')
                    .Append(FormatTextList(item.OccupiedArrayBonusCellIds)).Append('|')
                    .Append(FormatBool(item.isLit)).Append('|')
                    .Append(FormatBool(item.isLightingSource)).Append('|')
                    .Append(FormatBool(item.isOnArrayBonusCell)).Append('|')
                    .Append(FormatBool(item.isArrayBonusActive));
            }

            builder.Append("\nB0:")
                .Append(buildSnapshot.selectedMainBuildId).Append('|')
                .Append(FormatTextList(buildSnapshot.ValidationErrors));

            foreach (ItemSystemBuildTrackSnapshot track in buildSnapshot.FaMenBuilds)
            {
                AppendBuildTrack(builder, "BF", track);
            }

            foreach (ItemSystemBuildTrackSnapshot track in buildSnapshot.QiLeiBuilds)
            {
                AppendBuildTrack(builder, "BQ", track);
            }

            foreach (ItemSystemBuildItemSnapshot item in buildSnapshot.ItemResults)
            {
                builder.Append("\nBI:")
                    .Append(item.placementId).Append('|')
                    .Append(item.itemId).Append('|')
                    .Append(FormatBool(item.isLit)).Append('|')
                    .Append(FormatBool(item.isLightingSource)).Append('|')
                    .Append(FormatBool(item.countedInBuild)).Append('|')
                    .Append(item.faMenTag).Append('|')
                    .Append(item.qiLeiTag).Append('|')
                    .Append(item.faMenBuildId).Append('|')
                    .Append(item.qiLeiBuildId).Append('|')
                    .Append(item.faMenBuildCount.ToString(CultureInfo.InvariantCulture)).Append('|')
                    .Append(item.qiLeiBuildCount.ToString(CultureInfo.InvariantCulture)).Append('|')
                    .Append(item.faMenActiveStagePieceCount.ToString(CultureInfo.InvariantCulture)).Append('|')
                    .Append(item.qiLeiActiveStagePieceCount.ToString(CultureInfo.InvariantCulture));
            }

            foreach (ItemSystemAwakeningResultSnapshot item in awakeningResults)
            {
                builder.Append("\nW:")
                    .Append(item.placementId).Append('|')
                    .Append(item.itemId).Append('|')
                    .Append(item.inputLevel.ToString(CultureInfo.InvariantCulture)).Append('|')
                    .Append(item.resolvedLevel.ToString(CultureInfo.InvariantCulture)).Append('|')
                    .Append(FormatBool(item.highRarityUltimatePreview)).Append('|')
                    .Append(item.requiredRarityKey).Append('|')
                    .Append(FormatBool(item.isLit)).Append('|')
                    .Append(FormatBool(item.isLightingSource)).Append('|')
                    .Append(FormatBool(item.supportsAwakening)).Append('|')
                    .Append(FormatBool(item.basicEffectActive)).Append('|')
                    .Append(FormatBool(item.coreEffectUnlocked)).Append('|')
                    .Append(FormatBool(item.coreEffectActive)).Append('|')
                    .Append(item.inputSource).Append('|')
                    .Append(FormatTextList(item.ValidationErrors)).Append('|')
                    .Append(FormatTextList(item.UnlockedCoreEffectIds)).Append('|')
                    .Append(FormatTextList(item.ActiveCoreEffectIds));

                foreach (ItemSystemAwakeningNodeSnapshot node in item.NodeStates)
                {
                    builder.Append("\nWN:")
                        .Append(item.placementId).Append('|')
                        .Append(node.itemId).Append('|')
                        .Append(node.coreEffectId).Append('|')
                        .Append(node.nodeKind).Append('|')
                        .Append(node.unlockLevel.ToString(CultureInfo.InvariantCulture)).Append('|')
                        .Append(FormatBool(node.isUnlocked)).Append('|')
                        .Append(FormatBool(node.isActive)).Append('|')
                        .Append(node.stateKey).Append('|')
                        .Append(node.blockedReason).Append('|')
                        .Append(node.previewText).Append('|')
                        .Append(node.requiredRarityKey);
                }
            }

            builder.Append("\nM0:")
                .Append(skillMonitorSnapshot.selectedMainBuildId).Append('|')
                .Append(FormatBool(skillMonitorSnapshot.selectedMainBuildFound)).Append('|')
                .Append(skillMonitorSnapshot.selectedMainBuildCount.ToString(CultureInfo.InvariantCulture)).Append('|')
                .Append(skillMonitorSnapshot.selectedMainBuildStage.ToString(CultureInfo.InvariantCulture)).Append('|')
                .Append(FormatTextList(skillMonitorSnapshot.ValidationErrors));

            foreach (ItemSystemSkillMonitorSlotSnapshot slot in skillMonitorSnapshot.Slots)
            {
                builder.Append("\nS:")
                    .Append(slot.slotOrder.ToString(CultureInfo.InvariantCulture)).Append('|')
                    .Append(slot.slotType).Append('|')
                    .Append(slot.displayName).Append('|')
                    .Append(slot.sourceBuildId).Append('|')
                    .Append(slot.sourceFaMenTag).Append('|')
                    .Append(slot.requiredBuildStage.ToString(CultureInfo.InvariantCulture)).Append('|')
                    .Append(slot.triggerKind).Append('|')
                    .Append(FormatBool(slot.isUnlocked)).Append('|')
                    .Append(FormatBool(slot.isMonitoring)).Append('|')
                    .Append(FormatBool(slot.isTriggered)).Append('|')
                    .Append(FormatFloat(slot.cooldown01)).Append('|')
                    .Append(FormatFloat(slot.charge01)).Append('|')
                    .Append(slot.iconKey).Append('|')
                    .Append(slot.tooltipText).Append('|')
                    .Append(slot.presentationCueId).Append('|')
                    .Append(slot.chibiActionKey);
            }

            foreach (ItemSystemValidationError error in validationErrors)
            {
                builder.Append("\nE:")
                    .Append(error.code).Append('|')
                    .Append(error.severity).Append('|')
                    .Append(error.placementId).Append('|')
                    .Append(error.itemId).Append('|')
                    .Append(error.message);
            }

            return builder.ToString();
        }

        private static void AppendBuildTrack(StringBuilder builder, string prefix, ItemSystemBuildTrackSnapshot track)
        {
            builder.Append('\n')
                .Append(prefix).Append(':')
                .Append(track.buildId).Append('|')
                .Append(track.trackKind).Append('|')
                .Append(track.stableTag).Append('|')
                .Append(track.displayName).Append('|')
                .Append(track.litItemCount.ToString(CultureInfo.InvariantCulture)).Append('|')
                .Append(track.maxPieceCount.ToString(CultureInfo.InvariantCulture)).Append('|')
                .Append(track.activeStagePieceCount.ToString(CultureInfo.InvariantCulture)).Append('|')
                .Append(track.nextStagePieceCount.ToString(CultureInfo.InvariantCulture)).Append('|')
                .Append(FormatTextList(track.SourcePlacementIds)).Append('|')
                .Append(FormatTextList(track.SourceItemIds));
        }

        private static string FormatBool(bool value)
        {
            return value ? "1" : "0";
        }

        private static string FormatFloat(float value)
        {
            return value.ToString("0.########", CultureInfo.InvariantCulture);
        }

        private static string FormatTextList(IEnumerable<string> values)
        {
            return string.Join(";", (values ?? Array.Empty<string>()).Select(value => value ?? string.Empty));
        }

        public ItemLightingResolutionResult ToLightingResolutionResult()
        {
            return new ItemLightingResolutionResult(
                litRangeCells,
                lightingResults.Select(item => new ItemLightingItemResult(
                    item.itemId,
                    item.displayName,
                    item.OccupiedCells,
                    item.coreCellWorld,
                    item.isLightingSource,
                    item.isDirectLit,
                    item.isLit,
                    item.litByItemId,
                    item.litDepth,
                    item.placementId,
                    item.litByPlacementId)).ToArray());
        }

        public ItemArrayBonusResolutionResult ToArrayBonusResolutionResult()
        {
            List<ItemArrayBonusCellDefinition> cells = arrayBonusCells
                .Select((cell, index) => new ItemArrayBonusCellDefinition(FixedArrayCellId(cell, index), cell))
                .ToList();
            List<ItemArrayBonusItemResult> items = arrayBonusResults
                .Select(item => new ItemArrayBonusItemResult(
                    item.itemId,
                    item.placementId,
                    item.displayName,
                    item.OccupiedCells,
                    item.OccupiedArrayBonusCells,
                    item.OccupiedArrayBonusCellIds,
                    item.isLit,
                    item.isLightingSource))
                .ToList();
            List<ItemArrayBonusCellState> states = cells
                .Select(cell =>
                {
                    ItemArrayBonusItemResult[] occupying = items
                        .Where(item => item.OccupiedArrayBonusCells.Contains(cell.cell))
                        .OrderBy(item => item.placementId, StringComparer.Ordinal)
                        .ToArray();
                    return new ItemArrayBonusCellState(
                        cell.cellId,
                        cell.cell,
                        occupying.Select(item => item.placementId).ToArray(),
                        occupying.Select(item => item.itemId).ToArray());
                })
                .ToList();
            return new ItemArrayBonusResolutionResult(states, items);
        }

        public ItemBuildSynergyResolutionResult ToBuildSynergyResolutionResult()
        {
            ItemBuildSynergyResolutionResult result = new(
                buildSnapshot.FaMenBuilds.Select(ToTrack).ToArray(),
                buildSnapshot.QiLeiBuilds.Select(ToTrack).ToArray(),
                buildSnapshot.ItemResults.Select(item => new ItemBuildSynergyItemResult
                {
                    itemId = item.itemId,
                    placementId = item.placementId,
                    isLit = item.isLit,
                    isLightingSource = item.isLightingSource,
                    countedInBuild = item.countedInBuild,
                    faMenTag = item.faMenTag,
                    qiLeiTag = item.qiLeiTag,
                    faMenBuildId = item.faMenBuildId,
                    qiLeiBuildId = item.qiLeiBuildId,
                    faMenBuildCount = item.faMenBuildCount,
                    qiLeiBuildCount = item.qiLeiBuildCount,
                    faMenActiveStagePieceCount = item.faMenActiveStagePieceCount,
                    qiLeiActiveStagePieceCount = item.qiLeiActiveStagePieceCount
                }).ToArray(),
                buildSnapshot.ValidationErrors.ToArray());
            return result;
        }

        public ItemCoreAwakeningResolutionResult ToCoreAwakeningResolutionResult()
        {
            return new ItemCoreAwakeningResolutionResult(
                awakeningResults.Select(item => new ItemCoreAwakeningItemResult(
                    item.itemId,
                    item.placementId,
                    item.inputLevel,
                    item.resolvedLevel,
                    item.highRarityUltimatePreview,
                    item.requiredRarityKey,
                    item.isLit,
                    item.isLightingSource,
                    item.supportsAwakening,
                    item.basicEffectActive,
                    item.NodeStates.Select(node => new ItemCoreAwakeningNodeState(
                        node.itemId,
                        node.coreEffectId,
                        node.nodeKind,
                        node.unlockLevel,
                        node.isUnlocked,
                        node.isActive,
                        ParseEnum(node.stateKey, ItemCoreAwakeningStateKey.Locked),
                        ParseEnum(node.blockedReason, ItemCoreAwakeningBlockedReason.None),
                        node.previewText,
                        node.requiredRarityKey)).ToArray(),
                    item.ValidationErrors,
                    item.inputSource)).ToArray(),
                Array.Empty<string>());
        }

        public ItemSkillMonitorResolutionResult ToSkillMonitorResolutionResult()
        {
            return new ItemSkillMonitorResolutionResult(
                skillMonitorSnapshot.selectedMainBuildId,
                skillMonitorSnapshot.selectedMainBuildFound,
                skillMonitorSnapshot.selectedMainBuildCount,
                skillMonitorSnapshot.selectedMainBuildStage,
                skillMonitorSnapshot.Slots.Select(slot => new ItemSkillMonitorSlotSnapshot(
                    slot.slotType,
                    slot.slotOrder,
                    slot.displayName,
                    slot.sourceBuildId,
                    slot.sourceFaMenTag,
                    slot.requiredBuildStage,
                    slot.triggerKind,
                    slot.isUnlocked,
                    slot.isMonitoring,
                    slot.iconKey,
                    slot.tooltipText,
                    slot.presentationCueId,
                    slot.chibiActionKey)).ToArray(),
                skillMonitorSnapshot.ValidationErrors.ToArray());
        }

        internal static ItemSystemSnapshot EmptyWithError(ItemSystemValidationError error)
        {
            return new ItemSystemSnapshot(
                5,
                new Vector2Int(2, 2),
                ItemSystemBoardConfigInput.Default().ArrayBonusCells,
                Array.Empty<ItemSystemCatalogItemSnapshot>(),
                Array.Empty<ItemSystemPlacementSnapshot>(),
                Array.Empty<Vector2Int>(),
                Array.Empty<ItemSystemLightingResultSnapshot>(),
                Array.Empty<ItemSystemArrayBonusResultSnapshot>(),
                new ItemSystemBuildSnapshot(null),
                Array.Empty<ItemSystemAwakeningResultSnapshot>(),
                new ItemSystemSkillMonitorSnapshot(null),
                string.Empty,
                false,
                string.Empty,
                new[] { error });
        }

        private static ItemBuildTrackResult ToTrack(ItemSystemBuildTrackSnapshot track)
        {
            return new ItemBuildTrackResult(
                track.buildId,
                track.trackKind,
                track.stableTag,
                track.displayName,
                track.litItemCount,
                track.maxPieceCount,
                track.SourcePlacementIds,
                track.SourceItemIds);
        }

        private static T ParseEnum<T>(string value, T fallback) where T : struct
        {
            return Enum.TryParse(value, out T parsed) ? parsed : fallback;
        }

        private static ItemSystemValidationError[] NormalizeErrors(IReadOnlyList<ItemSystemValidationError> errors)
        {
            return (errors ?? Array.Empty<ItemSystemValidationError>())
                .Where(error => error != null && !string.IsNullOrWhiteSpace(error.code))
                .GroupBy(error => $"{error.code}|{error.placementId}|{error.itemId}|{error.message}", StringComparer.Ordinal)
                .Select(group => group.First())
                .OrderBy(error => error.code, StringComparer.Ordinal)
                .ThenBy(error => error.placementId, StringComparer.Ordinal)
                .ThenBy(error => error.itemId, StringComparer.Ordinal)
                .ThenBy(error => error.message, StringComparer.Ordinal)
                .ToArray();
        }

        private static string FixedArrayCellId(Vector2Int cell, int index)
        {
            if (cell == new Vector2Int(2, 1))
            {
                return "AP01";
            }

            if (cell == new Vector2Int(2, 3))
            {
                return "AP02";
            }

            if (cell == new Vector2Int(1, 2))
            {
                return "AP03";
            }

            if (cell == new Vector2Int(3, 2))
            {
                return "AP04";
            }

            return $"AP{index + 1:00}";
        }

        private static string FormatCells(IEnumerable<Vector2Int> cells)
        {
            return string.Join(";", (cells ?? Array.Empty<Vector2Int>()).Select(FormatCell));
        }

        private static string FormatCell(Vector2Int cell)
        {
            return $"({cell.x},{cell.y})";
        }
    }

    public sealed class DefaultItemSystemSnapshotProvider : IItemSystemSnapshotProvider
    {
        public static readonly DefaultItemSystemSnapshotProvider Instance = new();

        private readonly ItemSystemValidator validator = new();

        public ItemSystemSnapshot CreateSnapshot(ItemSystemSnapshotInput input)
        {
            try
            {
                ItemSystemSnapshotInput safeInput = input ?? new ItemSystemSnapshotInput(Array.Empty<ItemSystemPlacementInput>());
                ItemSystemBoardConfigInput board = safeInput.boardConfig ?? ItemSystemBoardConfigInput.Default();
                List<ItemInnerDataDefinition> catalog = (safeInput.CatalogItems ?? ItemInnerDataCatalog.AllItems)
                    .Where(item => item != null)
                    .Select(ItemSystemSnapshotInput.CloneCatalogItem)
                    .ToList();
                Dictionary<string, ItemInnerDataDefinition> catalogById = catalog
                    .Where(item => !string.IsNullOrWhiteSpace(item.itemId))
                    .GroupBy(item => item.itemId, StringComparer.Ordinal)
                    .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

                List<ItemSystemValidationError> errors = new();
                errors.AddRange(ValidateCatalog(catalog));
                errors.AddRange(ValidateBoard(board));

                List<PlacementBuildData> placementData = BuildPlacementData(safeInput.Placements, board, catalogById, errors);
                ValidateJuNianCount(placementData, errors);

                List<ItemLightingPlacedItem> lightingInputs = placementData
                    .Where(data => data.eligibleForResolvers)
                    .OrderBy(data => data.placementId, StringComparer.Ordinal)
                    .ThenBy(data => data.itemId, StringComparer.Ordinal)
                    .Select(data => new ItemLightingPlacedItem(
                        data.itemId,
                        data.displayName,
                        data.occupiedCells,
                        data.coreCellWorld,
                        data.catalogItem?.isLightingSource == true,
                        data.anchorCell,
                        data.placementId))
                    .ToList();

                ItemLightingBoardConfig lightingBoard = new(board.boardSize, board.eyeCell, board.ArrayBonusCells);
                ItemLightingResolutionResult lightingResult = ItemLightingResolver.Resolve(lightingBoard, lightingInputs);
                ItemArrayBonusResolutionResult arrayBonusResult = ItemArrayBonusResolver.Resolve(lightingBoard, lightingResult.ItemResults);
                ItemBuildSynergyResolutionResult buildResult = ItemBuildSynergyResolver.Resolve(lightingResult, catalog);
                IReadOnlyList<ItemCoreAwakeningInput> awakeningInputs = BuildAwakeningInputs(safeInput.AwakeningInputs, lightingResult);
                ItemCoreAwakeningResolutionResult awakeningResult = ItemCoreAwakeningResolver.Resolve(lightingResult, awakeningInputs);
                ItemMainBuildSelectionInput mainBuildInput = safeInput.mainBuildSelectionInput ?? ItemMainBuildSelectionInput.None("ItemSystemSnapshotInputDefault");
                bool selectedExplicit = !string.IsNullOrWhiteSpace(mainBuildInput.selectedMainBuildId)
                    && !string.IsNullOrWhiteSpace(mainBuildInput.selectionSource)
                    && !string.Equals(mainBuildInput.selectionSource, "Auto", StringComparison.OrdinalIgnoreCase);
                ItemSkillMonitorResolutionResult skillMonitorResult = ItemSkillMonitorResolver.Resolve(buildResult, mainBuildInput);

                errors.AddRange(ToResolverErrors("BUILD_RESOLVER_VALIDATION", buildResult.ValidationErrors));
                errors.AddRange(ToResolverErrors("AWAKENING_RESOLVER_VALIDATION", awakeningResult.ValidationErrors));
                errors.AddRange(ToResolverErrors("SKILL_MONITOR_VALIDATION", skillMonitorResult.ValidationErrors));

                ItemSystemBuildSnapshot buildSnapshot = new(buildResult);
                ItemSystemSkillMonitorSnapshot skillMonitorSnapshot = new(skillMonitorResult);
                ItemSystemSnapshot snapshot = BuildSnapshot(
                    board,
                    catalog,
                    placementData,
                    lightingResult,
                    arrayBonusResult,
                    buildSnapshot,
                    awakeningResult,
                    skillMonitorSnapshot,
                    mainBuildInput.selectedMainBuildId,
                    selectedExplicit,
                    mainBuildInput.selectionSource,
                    errors);

                List<ItemSystemValidationError> combined = new(snapshot.validationErrors);
                combined.AddRange(validator.ValidateSnapshot(snapshot));
                return BuildSnapshot(
                    board,
                    catalog,
                    placementData,
                    lightingResult,
                    arrayBonusResult,
                    buildSnapshot,
                    awakeningResult,
                    skillMonitorSnapshot,
                    mainBuildInput.selectedMainBuildId,
                    selectedExplicit,
                    mainBuildInput.selectionSource,
                    combined);
            }
            catch (Exception exception)
            {
                return ItemSystemSnapshot.EmptyWithError(ItemSystemValidationError.Error(
                    "ITEM_SYSTEM_UNHANDLED_EXCEPTION",
                    string.Empty,
                    string.Empty,
                    exception.Message));
            }
        }

        private static ItemSystemSnapshot BuildSnapshot(
            ItemSystemBoardConfigInput board,
            IReadOnlyList<ItemInnerDataDefinition> catalog,
            IReadOnlyList<PlacementBuildData> placementData,
            ItemLightingResolutionResult lightingResult,
            ItemArrayBonusResolutionResult arrayBonusResult,
            ItemSystemBuildSnapshot buildSnapshot,
            ItemCoreAwakeningResolutionResult awakeningResult,
            ItemSystemSkillMonitorSnapshot skillMonitorSnapshot,
            string selectedMainBuildId,
            bool selectedExplicit,
            string selectedSource,
            IReadOnlyList<ItemSystemValidationError> errors)
        {
            List<ItemSystemPlacementSnapshot> placements = new();
            foreach (PlacementBuildData data in placementData)
            {
                ItemLightingItemResult lighting = lightingResult.FindPlacementResult(data.placementId);
                ItemArrayBonusItemResult array = arrayBonusResult.FindPlacementResult(data.placementId);
                ItemSystemBuildItemSnapshot buildItem = buildSnapshot.FindPlacementResult(data.placementId);
                ItemCoreAwakeningItemResult awakening = awakeningResult.FindPlacementResult(data.placementId);
                placements.Add(new ItemSystemPlacementSnapshot(
                    data.placementId,
                    data.itemId,
                    data.displayName,
                    data.anchorCell,
                    data.rotation,
                    data.occupiedCells,
                    data.coreCellWorld,
                    data.catalogItem?.isLightingSource == true,
                    lighting?.isDirectLit == true,
                    lighting?.isLit == true,
                    lighting?.litByItemId ?? string.Empty,
                    lighting?.litByPlacementId ?? string.Empty,
                    lighting?.litDepth ?? -1,
                    array?.OccupiedArrayBonusCells ?? Array.Empty<Vector2Int>(),
                    array?.isOnArrayBonusCell == true,
                    array?.isArrayBonusActive == true,
                    buildItem?.countedInBuild == true,
                    awakening?.inputLevel ?? 1,
                    awakening?.resolvedLevel ?? 1,
                    awakening?.UnlockedCoreEffectIds ?? Array.Empty<string>(),
                    awakening?.ActiveCoreEffectIds ?? Array.Empty<string>()));
            }

            return new ItemSystemSnapshot(
                board.boardSize,
                board.eyeCell,
                board.ArrayBonusCells,
                catalog.Select(item => new ItemSystemCatalogItemSnapshot(item)).ToArray(),
                placements,
                lightingResult.LitRangeCells,
                lightingResult.ItemResults.Select(item => new ItemSystemLightingResultSnapshot(item)).ToArray(),
                arrayBonusResult.ItemResults.Select(item => new ItemSystemArrayBonusResultSnapshot(item)).ToArray(),
                buildSnapshot,
                awakeningResult.ItemResults.Select(item => new ItemSystemAwakeningResultSnapshot(item)).ToArray(),
                skillMonitorSnapshot,
                selectedMainBuildId,
                selectedExplicit,
                selectedSource,
                errors);
        }

        private static List<PlacementBuildData> BuildPlacementData(
            IReadOnlyList<ItemSystemPlacementInput> placementInputs,
            ItemSystemBoardConfigInput board,
            IReadOnlyDictionary<string, ItemInnerDataDefinition> catalogById,
            List<ItemSystemValidationError> errors)
        {
            List<PlacementBuildData> results = new();
            HashSet<string> seenPlacementIds = new(StringComparer.Ordinal);
            Dictionary<Vector2Int, string> occupiedByPlacement = new();
            ItemSystemPlacementInput[] sortedInputs = (placementInputs ?? Array.Empty<ItemSystemPlacementInput>())
                .Where(input => input != null)
                .Select((input, index) => new { input, index })
                .OrderBy(entry => Normalize(entry.input.placementId), StringComparer.Ordinal)
                .ThenBy(entry => Normalize(entry.input.itemId), StringComparer.Ordinal)
                .ThenBy(entry => entry.input.anchorCell.y)
                .ThenBy(entry => entry.input.anchorCell.x)
                .ThenBy(entry => entry.index)
                .Select(entry => entry.input)
                .ToArray();

            foreach (ItemSystemPlacementInput input in sortedInputs)
            {
                string placementId = Normalize(input.placementId);
                string itemId = Normalize(input.itemId);
                bool eligible = true;

                if (string.IsNullOrWhiteSpace(placementId))
                {
                    errors.Add(ItemSystemValidationError.Error("PLACEMENT_ID_EMPTY", placementId, itemId, "placementId must be non-empty."));
                    eligible = false;
                }
                else if (!seenPlacementIds.Add(placementId))
                {
                    errors.Add(ItemSystemValidationError.Error("PLACEMENT_ID_DUPLICATE", placementId, itemId, "placementId must be unique."));
                    eligible = false;
                }

                if (string.IsNullOrWhiteSpace(itemId))
                {
                    errors.Add(ItemSystemValidationError.Error("ITEM_ID_EMPTY", placementId, itemId, "itemId must be non-empty."));
                    eligible = false;
                }

                catalogById.TryGetValue(itemId, out ItemInnerDataDefinition catalogItem);
                if (catalogItem == null)
                {
                    errors.Add(ItemSystemValidationError.Error("ITEM_ID_NOT_IN_CATALOG", placementId, itemId, "itemId was not found in catalog."));
                    eligible = false;
                }

                int rotation = NormalizeRotation(input.rotation, placementId, itemId, errors, ref eligible);
                Vector2Int[] shapeCells = catalogItem?.ShapeCells?.ToArray() ?? Array.Empty<Vector2Int>();
                Vector2Int[] rotatedCells = shapeCells.Select(cell => Rotate(cell, rotation)).OrderByCell().ToArray();
                Vector2Int[] occupiedCells = rotatedCells.Select(cell => input.anchorCell + cell).OrderByCell().ToArray();
                Vector2Int coreCellWorld = input.anchorCell + Rotate(catalogItem?.coreCellLocal ?? Vector2Int.zero, rotation);

                if (catalogItem != null && !HasValidShape(catalogItem))
                {
                    errors.Add(ItemSystemValidationError.Error("ITEM_SHAPE_INVALID", placementId, itemId, "catalog shapeCells must be non-empty, unique, and include coreCellLocal."));
                    eligible = false;
                }

                if (occupiedCells.Any(cell => !IsWithinBoard(cell, board.boardSize)))
                {
                    errors.Add(ItemSystemValidationError.Error("ITEM_OUT_OF_BOUNDS", placementId, itemId, "occupiedCells must stay inside the fixed board."));
                    eligible = false;
                }

                if (occupiedCells.Contains(board.eyeCell))
                {
                    errors.Add(ItemSystemValidationError.Error("EYE_CELL_COVERED", placementId, itemId, "occupiedCells must not cover eyeCell."));
                    eligible = false;
                }

                if (!occupiedCells.Contains(coreCellWorld))
                {
                    errors.Add(ItemSystemValidationError.Error("CORE_CELL_NOT_OCCUPIED", placementId, itemId, "coreCellWorld must belong to occupiedCells."));
                    eligible = false;
                }

                foreach (Vector2Int cell in occupiedCells)
                {
                    if (occupiedByPlacement.TryGetValue(cell, out string existingPlacementId))
                    {
                        errors.Add(ItemSystemValidationError.Error("PLACEMENT_OVERLAP", placementId, itemId, $"occupiedCell {FormatCell(cell)} overlaps placementId '{existingPlacementId}'."));
                        eligible = false;
                    }
                }

                if (eligible)
                {
                    foreach (Vector2Int cell in occupiedCells)
                    {
                        occupiedByPlacement[cell] = placementId;
                    }
                }

                results.Add(new PlacementBuildData(
                    placementId,
                    itemId,
                    catalogItem?.displayName ?? string.Empty,
                    input.anchorCell,
                    rotation,
                    occupiedCells,
                    coreCellWorld,
                    catalogItem,
                    eligible));
            }

            return results;
        }

        private static IReadOnlyList<ItemCoreAwakeningInput> BuildAwakeningInputs(
            IReadOnlyList<ItemCoreAwakeningInput> sourceInputs,
            ItemLightingResolutionResult lightingResult)
        {
            Dictionary<string, ItemCoreAwakeningInput> byPlacement = (sourceInputs ?? Array.Empty<ItemCoreAwakeningInput>())
                .Where(input => input != null && !string.IsNullOrWhiteSpace(input.placementId))
                .GroupBy(input => input.placementId, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => CloneAwakeningInput(group.First()), StringComparer.Ordinal);
            List<ItemCoreAwakeningInput> results = new();
            foreach (ItemLightingItemResult item in lightingResult?.ItemResults ?? Array.Empty<ItemLightingItemResult>())
            {
                if (item == null || string.IsNullOrWhiteSpace(item.placementId))
                {
                    continue;
                }

                if (byPlacement.TryGetValue(item.placementId, out ItemCoreAwakeningInput input))
                {
                    results.Add(input);
                    continue;
                }

                results.Add(new ItemCoreAwakeningInput(
                    item.itemId,
                    item.placementId,
                    1,
                    false,
                    "ItemSystemDefaultPreviewLv1"));
            }

            return results;
        }

        private static IReadOnlyList<ItemSystemValidationError> ValidateCatalog(IReadOnlyList<ItemInnerDataDefinition> catalog)
        {
            List<ItemSystemValidationError> errors = new();
            ItemInnerDataDefinition[] items = (catalog ?? Array.Empty<ItemInnerDataDefinition>()).Where(item => item != null).ToArray();
            if (items.Length != 31)
            {
                errors.Add(ItemSystemValidationError.Error("CATALOG_COUNT_INVALID", string.Empty, string.Empty, $"catalog must contain exactly 31 items; actual {items.Length}."));
            }

            foreach (IGrouping<string, ItemInnerDataDefinition> group in items
                .Where(item => !string.IsNullOrWhiteSpace(item.itemId))
                .GroupBy(item => item.itemId, StringComparer.Ordinal)
                .Where(group => group.Count() > 1)
                .OrderBy(group => group.Key, StringComparer.Ordinal))
            {
                errors.Add(ItemSystemValidationError.Error("CATALOG_ITEM_ID_DUPLICATE", string.Empty, group.Key, "catalog itemId must be unique."));
            }

            HashSet<string> ids = new(items.Select(item => item.itemId ?? string.Empty), StringComparer.Ordinal);
            for (int index = 1; index <= 31; index++)
            {
                string expectedId = $"I{index:000}";
                if (!ids.Contains(expectedId))
                {
                    errors.Add(ItemSystemValidationError.Error("CATALOG_STABLE_ID_MISSING", string.Empty, expectedId, "catalog must contain stable itemId I001-I031."));
                }
            }

            ItemInnerDataDefinition i031 = items.FirstOrDefault(item => string.Equals(item.itemId, "I031", StringComparison.Ordinal));
            if (i031 == null || !i031.isLightingSource)
            {
                errors.Add(ItemSystemValidationError.Error("CATALOG_I031_SOURCE_INVALID", string.Empty, "I031", "I031 must be the JuNian lighting source boundary item."));
            }

            foreach (ItemInnerDataDefinition item in items.Where(item => !string.Equals(item.itemId, "I031", StringComparison.Ordinal) && item.isLightingSource))
            {
                errors.Add(ItemSystemValidationError.Error("ORDINARY_ITEM_LIGHTING_SOURCE_FORGED", string.Empty, item.itemId, "ordinary catalog item must not be a direct lighting source."));
            }

            return errors;
        }

        private static IReadOnlyList<ItemSystemValidationError> ValidateBoard(ItemSystemBoardConfigInput board)
        {
            List<ItemSystemValidationError> errors = new();
            if (board.boardSize != 5)
            {
                errors.Add(ItemSystemValidationError.Error("BOARD_SIZE_INVALID", string.Empty, string.Empty, $"boardSize must be 5; actual {board.boardSize}."));
            }

            if (board.eyeCell != new Vector2Int(2, 2))
            {
                errors.Add(ItemSystemValidationError.Error("EYE_CELL_INVALID", string.Empty, string.Empty, $"eyeCell must be (2,2); actual {FormatCell(board.eyeCell)}."));
            }

            Vector2Int[] expected =
            {
                new(2, 1),
                new(2, 3),
                new(1, 2),
                new(3, 2)
            };
            Vector2Int[] actual = (board.ArrayBonusCells ?? Array.Empty<Vector2Int>()).OrderByCell().ToArray();
            if (actual.Length != 4 || !actual.SequenceEqual(expected.OrderByCell()))
            {
                errors.Add(ItemSystemValidationError.Error("ARRAY_BONUS_CELLS_INVALID", string.Empty, string.Empty, "arrayBonusCells must be AP01=(2,1), AP02=(2,3), AP03=(1,2), AP04=(3,2)."));
            }

            return errors;
        }

        private static void ValidateJuNianCount(IReadOnlyList<PlacementBuildData> placements, List<ItemSystemValidationError> errors)
        {
            int count = (placements ?? Array.Empty<PlacementBuildData>()).Count(placement => string.Equals(placement.itemId, "I031", StringComparison.Ordinal));
            if (count == 0)
            {
                errors.Add(ItemSystemValidationError.Error("JUNIAN_MISSING", string.Empty, "I031", "valid complete layout requires exactly one I031 JuNian stone."));
            }
            else if (count > 1)
            {
                errors.Add(ItemSystemValidationError.Error("JUNIAN_MULTIPLE", string.Empty, "I031", $"valid complete layout requires exactly one I031 JuNian stone; actual {count}."));
            }
        }

        private static IReadOnlyList<ItemSystemValidationError> ToResolverErrors(string code, IReadOnlyList<string> sourceErrors)
        {
            return (sourceErrors ?? Array.Empty<string>())
                .Where(error => !string.IsNullOrWhiteSpace(error))
                .Select(error => ItemSystemValidationError.Error(code, string.Empty, string.Empty, error))
                .ToArray();
        }

        private static int NormalizeRotation(string rotationText, string placementId, string itemId, List<ItemSystemValidationError> errors, ref bool eligible)
        {
            return int.TryParse(rotationText, out int value)
                ? NormalizeRotation(value, placementId, itemId, errors, ref eligible)
                : NormalizeRotation(0, placementId, itemId, errors, ref eligible);
        }

        private static int NormalizeRotation(int rotation, string placementId, string itemId, List<ItemSystemValidationError> errors, ref bool eligible)
        {
            int normalized = ((rotation % 360) + 360) % 360;
            if (normalized % 90 != 0)
            {
                errors.Add(ItemSystemValidationError.Error("PLACEMENT_ROTATION_INVALID", placementId, itemId, $"rotation must be 0, 90, 180, or 270; actual {rotation}."));
                eligible = false;
                return 0;
            }

            return normalized;
        }

        private static bool HasValidShape(ItemInnerDataDefinition item)
        {
            IReadOnlyList<Vector2Int> cells = item?.ShapeCells ?? Array.Empty<Vector2Int>();
            return item != null
                && cells.Count > 0
                && cells.Distinct().Count() == cells.Count
                && cells.Contains(item.coreCellLocal);
        }

        private static Vector2Int Rotate(Vector2Int cell, int rotation)
        {
            return rotation switch
            {
                90 => new Vector2Int(-cell.y, cell.x),
                180 => new Vector2Int(-cell.x, -cell.y),
                270 => new Vector2Int(cell.y, -cell.x),
                _ => cell
            };
        }

        private static bool IsWithinBoard(Vector2Int cell, int boardSize)
        {
            return cell.x >= 0 && cell.y >= 0 && cell.x < boardSize && cell.y < boardSize;
        }

        private static ItemCoreAwakeningInput CloneAwakeningInput(ItemCoreAwakeningInput input)
        {
            return new ItemCoreAwakeningInput(
                input.itemId,
                input.placementId,
                input.inputLevel,
                input.highRarityUltimatePreview,
                input.inputSource,
                input.requiredRarityKey);
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private static string FormatCell(Vector2Int cell)
        {
            return $"({cell.x},{cell.y})";
        }

        private sealed class PlacementBuildData
        {
            public PlacementBuildData(
                string placementId,
                string itemId,
                string displayName,
                Vector2Int anchorCell,
                int rotation,
                IReadOnlyList<Vector2Int> occupiedCells,
                Vector2Int coreCellWorld,
                ItemInnerDataDefinition catalogItem,
                bool eligibleForResolvers)
            {
                this.placementId = placementId ?? string.Empty;
                this.itemId = itemId ?? string.Empty;
                this.displayName = displayName ?? string.Empty;
                this.anchorCell = anchorCell;
                this.rotation = rotation;
                this.occupiedCells = (occupiedCells ?? Array.Empty<Vector2Int>()).OrderByCell().ToArray();
                this.coreCellWorld = coreCellWorld;
                this.catalogItem = catalogItem;
                this.eligibleForResolvers = eligibleForResolvers;
            }

            public readonly string placementId;
            public readonly string itemId;
            public readonly string displayName;
            public readonly Vector2Int anchorCell;
            public readonly int rotation;
            public readonly Vector2Int[] occupiedCells;
            public readonly Vector2Int coreCellWorld;
            public readonly ItemInnerDataDefinition catalogItem;
            public readonly bool eligibleForResolvers;
        }
    }

    public sealed class ItemSystemValidator : IItemSystemValidator
    {
        private static readonly ItemSkillMonitorSlotType[] ExpectedSlots =
        {
            ItemSkillMonitorSlotType.BasicAttack,
            ItemSkillMonitorSlotType.Build2,
            ItemSkillMonitorSlotType.Build4,
            ItemSkillMonitorSlotType.Build6
        };

        public IReadOnlyList<ItemSystemValidationError> ValidateSnapshot(ItemSystemSnapshot snapshot)
        {
            List<ItemSystemValidationError> errors = new();
            if (snapshot == null)
            {
                errors.Add(ItemSystemValidationError.Error("SNAPSHOT_NULL", string.Empty, string.Empty, "snapshot must not be null."));
                return errors;
            }

            if (!string.Equals(snapshot.schemaVersion, ItemSystemSnapshot.CurrentSchemaVersion, StringComparison.Ordinal))
            {
                errors.Add(ItemSystemValidationError.Error("SCHEMA_VERSION_INVALID", string.Empty, string.Empty, $"schemaVersion must be {ItemSystemSnapshot.CurrentSchemaVersion}."));
            }

            ValidateCatalog(snapshot, errors);
            ValidateBoard(snapshot, errors);
            ValidatePlacements(snapshot, errors);
            ValidateChildSnapshotIdentity(snapshot, errors);
            ValidateChildSnapshotState(snapshot, errors);
            ValidateLightingSelfConsistency(snapshot, errors);
            ValidateBuildInternalConsistency(snapshot, errors);
            ValidateBuildAndSelection(snapshot, errors);
            ValidateSkillMonitor(snapshot, errors);
            return errors
                .GroupBy(error => $"{error.code}|{error.placementId}|{error.itemId}|{error.message}", StringComparer.Ordinal)
                .Select(group => group.First())
                .OrderBy(error => error.code, StringComparer.Ordinal)
                .ThenBy(error => error.placementId, StringComparer.Ordinal)
                .ThenBy(error => error.itemId, StringComparer.Ordinal)
                .ThenBy(error => error.message, StringComparer.Ordinal)
                .ToArray();
        }

        private static void ValidateCatalog(ItemSystemSnapshot snapshot, List<ItemSystemValidationError> errors)
        {
            if (snapshot.catalogItems.Count != 31)
            {
                errors.Add(ItemSystemValidationError.Error("CATALOG_COUNT_INVALID", string.Empty, string.Empty, $"catalogItems must contain 31 items; actual {snapshot.catalogItems.Count}."));
            }

            foreach (IGrouping<string, ItemSystemCatalogItemSnapshot> group in snapshot.catalogItems
                .Where(item => !string.IsNullOrWhiteSpace(item.itemId))
                .GroupBy(item => item.itemId, StringComparer.Ordinal)
                .Where(group => group.Count() > 1)
                .OrderBy(group => group.Key, StringComparer.Ordinal))
            {
                errors.Add(ItemSystemValidationError.Error("CATALOG_ITEM_ID_DUPLICATE", string.Empty, group.Key, "catalog itemId must be unique."));
            }

            HashSet<string> ids = new(snapshot.catalogItems.Select(item => item.itemId), StringComparer.Ordinal);
            for (int index = 1; index <= 31; index++)
            {
                string expectedId = $"I{index:000}";
                if (!ids.Contains(expectedId))
                {
                    errors.Add(ItemSystemValidationError.Error("CATALOG_STABLE_ID_MISSING", string.Empty, expectedId, "catalogItems must include stable I001-I031 ids."));
                }
            }

            ItemSystemCatalogItemSnapshot i031 = snapshot.catalogItems.FirstOrDefault(item => item.itemId == "I031");
            if (i031 == null || !i031.isLightingSource)
            {
                errors.Add(ItemSystemValidationError.Error("CATALOG_I031_SOURCE_INVALID", string.Empty, "I031", "I031 must be the JuNian source boundary item."));
            }

            foreach (ItemSystemCatalogItemSnapshot item in snapshot.catalogItems
                .Where(item => !string.Equals(item.itemId, "I031", StringComparison.Ordinal) && item.isLightingSource))
            {
                errors.Add(ItemSystemValidationError.Error("ORDINARY_ITEM_LIGHTING_SOURCE_FORGED", string.Empty, item.itemId, "ordinary catalog item must not be a direct lighting source."));
            }
        }

        private static void ValidateBoard(ItemSystemSnapshot snapshot, List<ItemSystemValidationError> errors)
        {
            if (snapshot.boardSize != 5)
            {
                errors.Add(ItemSystemValidationError.Error("BOARD_SIZE_INVALID", string.Empty, string.Empty, "boardSize must be 5."));
            }

            if (snapshot.eyeCell != new Vector2Int(2, 2))
            {
                errors.Add(ItemSystemValidationError.Error("EYE_CELL_INVALID", string.Empty, string.Empty, "eyeCell must be (2,2)."));
            }

            Vector2Int[] expectedArrayCells =
            {
                new(2, 1),
                new(2, 3),
                new(1, 2),
                new(3, 2)
            };
            if (!snapshot.ArrayBonusCells.OrderByCell().SequenceEqual(expectedArrayCells.OrderByCell()))
            {
                errors.Add(ItemSystemValidationError.Error("ARRAY_BONUS_CELLS_INVALID", string.Empty, string.Empty, "arrayBonusCells must stay fixed around eyeCell."));
            }
        }

        private static void ValidatePlacements(ItemSystemSnapshot snapshot, List<ItemSystemValidationError> errors)
        {
            Dictionary<string, ItemSystemCatalogItemSnapshot> catalog = snapshot.catalogItems
                .Where(item => !string.IsNullOrWhiteSpace(item.itemId))
                .GroupBy(item => item.itemId, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
            Dictionary<Vector2Int, string> occupiedByCell = new();
            foreach (IGrouping<string, ItemSystemPlacementSnapshot> group in snapshot.placements.GroupBy(placement => placement.placementId, StringComparer.Ordinal))
            {
                if (string.IsNullOrWhiteSpace(group.Key))
                {
                    foreach (ItemSystemPlacementSnapshot placement in group)
                    {
                        errors.Add(ItemSystemValidationError.Error("PLACEMENT_ID_EMPTY", placement.placementId, placement.itemId, "placementId must be non-empty."));
                    }
                }
                else if (group.Count() > 1)
                {
                    errors.Add(ItemSystemValidationError.Error("PLACEMENT_ID_DUPLICATE", group.Key, group.First().itemId, "placementId must be unique."));
                }
            }

            int juNianCount = snapshot.placements.Count(placement => placement.itemId == "I031");
            if (juNianCount == 0)
            {
                errors.Add(ItemSystemValidationError.Error("JUNIAN_MISSING", string.Empty, "I031", "valid complete layout requires exactly one I031."));
            }
            else if (juNianCount > 1)
            {
                errors.Add(ItemSystemValidationError.Error("JUNIAN_MULTIPLE", string.Empty, "I031", "valid complete layout requires exactly one I031."));
            }

            foreach (IGrouping<string, ItemSystemPlacementSnapshot> group in snapshot.placements
                .Where(placement => !string.IsNullOrWhiteSpace(placement.itemId)
                    && !string.Equals(placement.itemId, "I031", StringComparison.Ordinal))
                .GroupBy(placement => placement.itemId, StringComparer.Ordinal)
                .Where(group => group.Count() > 1)
                .OrderBy(group => group.Key, StringComparer.Ordinal))
            {
                ItemSystemPlacementSnapshot[] ordered = group
                    .OrderBy(placement => placement.placementId, StringComparer.Ordinal)
                    .ThenBy(placement => placement.anchorCell.y)
                    .ThenBy(placement => placement.anchorCell.x)
                    .ToArray();
                string firstPlacementId = ordered[0].placementId;
                foreach (ItemSystemPlacementSnapshot duplicate in ordered.Skip(1))
                {
                    errors.Add(ItemSystemValidationError.Error(
                        ItemSystemValidationCodes.DuplicateBaseItemPlaced,
                        duplicate.placementId,
                        duplicate.itemId,
                        $"baseItemId/itemId '{duplicate.itemId}' is already placed by placementId '{firstPlacementId}'; one layout may place only one ordinary item with the same base identity."));
                }
            }

            foreach (ItemSystemPlacementSnapshot placement in snapshot.placements)
            {
                foreach (Vector2Int occupiedCell in placement.OccupiedCells.Distinct().OrderByCell())
                {
                    if (occupiedByCell.TryGetValue(occupiedCell, out string existingPlacementId)
                        && !string.Equals(existingPlacementId, placement.placementId, StringComparison.Ordinal))
                    {
                        errors.Add(ItemSystemValidationError.Error("PLACEMENT_OVERLAP", placement.placementId, placement.itemId, $"occupiedCell {FormatCell(occupiedCell)} overlaps placementId '{existingPlacementId}'."));
                    }
                    else
                    {
                        occupiedByCell[occupiedCell] = placement.placementId;
                    }
                }

                catalog.TryGetValue(placement.itemId, out ItemSystemCatalogItemSnapshot catalogItem);
                if (catalogItem == null)
                {
                    errors.Add(ItemSystemValidationError.Error("ITEM_ID_NOT_IN_CATALOG", placement.placementId, placement.itemId, "placement itemId must exist in catalog."));
                    continue;
                }

                bool rotationValid = IsValidRotation(placement.rotation);
                if (!rotationValid)
                {
                    errors.Add(ItemSystemValidationError.Error("PLACEMENT_ROTATION_INVALID", placement.placementId, placement.itemId, $"rotation must be 0, 90, 180, or 270; actual {placement.rotation}."));
                }
                else
                {
                    Vector2Int[] expectedCells = catalogItem.ShapeCells
                        .Select(cell => placement.anchorCell + Rotate(cell, placement.rotation))
                        .OrderByCell()
                        .ToArray();
                    if (!expectedCells.SequenceEqual(placement.OccupiedCells.OrderByCell()))
                    {
                        errors.Add(ItemSystemValidationError.Error("OCCUPIED_CELLS_MISMATCH", placement.placementId, placement.itemId, "occupiedCells must match catalog shape and rotation."));
                    }
                }

                if (!placement.OccupiedCells.Contains(placement.coreCellWorld))
                {
                    errors.Add(ItemSystemValidationError.Error("CORE_CELL_NOT_OCCUPIED", placement.placementId, placement.itemId, "coreCellWorld must belong to occupiedCells."));
                }

                if (placement.OccupiedCells.Any(cell => cell.x < 0 || cell.y < 0 || cell.x >= snapshot.boardSize || cell.y >= snapshot.boardSize))
                {
                    errors.Add(ItemSystemValidationError.Error("ITEM_OUT_OF_BOUNDS", placement.placementId, placement.itemId, "occupiedCells must stay inside board."));
                }

                if (placement.OccupiedCells.Contains(snapshot.eyeCell))
                {
                    errors.Add(ItemSystemValidationError.Error("EYE_CELL_COVERED", placement.placementId, placement.itemId, "occupiedCells must not cover eyeCell."));
                }

                if (placement.itemId != "I031" && placement.isLightingSource)
                {
                    errors.Add(ItemSystemValidationError.Error("ORDINARY_ITEM_LIGHTING_SOURCE_FORGED", placement.placementId, placement.itemId, "ordinary item must not become a direct lighting source."));
                }

                if (!placement.isLit && placement.isCountedInBuild)
                {
                    errors.Add(ItemSystemValidationError.Error("UNLIT_ITEM_COUNTED_IN_BUILD", placement.placementId, placement.itemId, "unlit item must not count in Build."));
                }

                if (!placement.isLit && placement.isArrayBonusActive)
                {
                    errors.Add(ItemSystemValidationError.Error("UNLIT_ITEM_ARRAY_BONUS_ACTIVE", placement.placementId, placement.itemId, "unlit item must not activate array bonus."));
                }

                if (placement.ActiveCoreEffectIds.Any(id => !placement.UnlockedCoreEffectIds.Contains(id)))
                {
                    errors.Add(ItemSystemValidationError.Error("CORE_EFFECT_ACTIVE_WHEN_LOCKED", placement.placementId, placement.itemId, "active core effect must also be unlocked."));
                }

                if (!placement.isLit && placement.ActiveCoreEffectIds.Count > 0)
                {
                    errors.Add(ItemSystemValidationError.Error("CORE_EFFECT_ACTIVE_WHEN_UNLIT", placement.placementId, placement.itemId, "core effects must not be active when item is unlit."));
                }

                if (placement.itemId == "I031" && placement.isCountedInBuild)
                {
                    errors.Add(ItemSystemValidationError.Error("I031_COUNTED_IN_BUILD", placement.placementId, placement.itemId, "I031 must not count into faMen or qiLei Build."));
                }
            }
        }

        private static void ValidateChildSnapshotIdentity(ItemSystemSnapshot snapshot, List<ItemSystemValidationError> errors)
        {
            Dictionary<string, ItemSystemPlacementSnapshot> placementsById = snapshot.placements
                .Where(placement => !string.IsNullOrWhiteSpace(placement.placementId))
                .GroupBy(placement => placement.placementId, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

            ValidateResultBranch(snapshot, errors, placementsById, "lightingResults", snapshot.lightingResults.Select(item => new SnapshotResultRef(item.placementId, item.itemId)));
            ValidateResultBranch(snapshot, errors, placementsById, "arrayBonusResults", snapshot.arrayBonusResults.Select(item => new SnapshotResultRef(item.placementId, item.itemId)));
            ValidateResultBranch(snapshot, errors, placementsById, "buildSnapshot.ItemResults", snapshot.buildSnapshot.ItemResults.Select(item => new SnapshotResultRef(item.placementId, item.itemId)));
            ValidateResultBranch(snapshot, errors, placementsById, "awakeningResults", snapshot.awakeningResults.Select(item => new SnapshotResultRef(item.placementId, item.itemId)));
        }

        private static void ValidateResultBranch(
            ItemSystemSnapshot snapshot,
            List<ItemSystemValidationError> errors,
            IReadOnlyDictionary<string, ItemSystemPlacementSnapshot> placementsById,
            string branchName,
            IEnumerable<SnapshotResultRef> resultRefs)
        {
            SnapshotResultRef[] results = (resultRefs ?? Array.Empty<SnapshotResultRef>())
                .OrderBy(item => item.placementId, StringComparer.Ordinal)
                .ThenBy(item => item.itemId, StringComparer.Ordinal)
                .ToArray();
            foreach (IGrouping<string, SnapshotResultRef> group in results.GroupBy(item => item.placementId, StringComparer.Ordinal).Where(group => group.Count() > 1))
            {
                errors.Add(ItemSystemValidationError.Error("SNAPSHOT_RESULT_DUPLICATE", group.Key, group.First().itemId, $"{branchName} must contain at most one result per placementId."));
            }

            HashSet<string> resultPlacementIds = new(results
                .Where(item => !string.IsNullOrWhiteSpace(item.placementId))
                .Select(item => item.placementId), StringComparer.Ordinal);
            foreach (ItemSystemPlacementSnapshot placement in snapshot.placements.Where(placement => !string.IsNullOrWhiteSpace(placement.placementId)))
            {
                if (!resultPlacementIds.Contains(placement.placementId))
                {
                    errors.Add(ItemSystemValidationError.Error("SNAPSHOT_RESULT_MISSING", placement.placementId, placement.itemId, $"{branchName} is missing placementId."));
                }
            }

            foreach (SnapshotResultRef result in results)
            {
                if (!placementsById.TryGetValue(result.placementId, out ItemSystemPlacementSnapshot placement))
                {
                    errors.Add(ItemSystemValidationError.Error("SNAPSHOT_RESULT_ORPHAN", result.placementId, result.itemId, $"{branchName} has no matching placementId."));
                    continue;
                }

                if (!string.Equals(placement.itemId, result.itemId, StringComparison.Ordinal))
                {
                    errors.Add(ItemSystemValidationError.Error("SNAPSHOT_ITEM_ID_MISMATCH", result.placementId, result.itemId, $"{branchName} itemId must match placement itemId '{placement.itemId}'."));
                }
            }
        }

        private static void ValidateChildSnapshotState(ItemSystemSnapshot snapshot, List<ItemSystemValidationError> errors)
        {
            Dictionary<string, ItemSystemLightingResultSnapshot> lightingById = FirstByPlacement(snapshot.lightingResults);
            Dictionary<string, ItemSystemArrayBonusResultSnapshot> arrayById = FirstByPlacement(snapshot.arrayBonusResults);
            Dictionary<string, ItemSystemBuildItemSnapshot> buildById = FirstByPlacement(snapshot.buildSnapshot.ItemResults);
            Dictionary<string, ItemSystemAwakeningResultSnapshot> awakeningById = FirstByPlacement(snapshot.awakeningResults);

            foreach (ItemSystemPlacementSnapshot placement in snapshot.placements)
            {
                if (lightingById.TryGetValue(placement.placementId, out ItemSystemLightingResultSnapshot lighting)
                    && (!CellsEqual(placement.OccupiedCells, lighting.OccupiedCells)
                        || placement.coreCellWorld != lighting.coreCellWorld
                        || placement.isLightingSource != lighting.isLightingSource
                        || placement.isDirectLit != lighting.isDirectLit
                        || placement.isLit != lighting.isLit
                        || !string.Equals(placement.litByItemId, lighting.litByItemId, StringComparison.Ordinal)
                        || !string.Equals(placement.litByPlacementId, lighting.litByPlacementId, StringComparison.Ordinal)
                        || placement.litDepth != lighting.litDepth))
                {
                    errors.Add(ItemSystemValidationError.Error("LIGHTING_SNAPSHOT_MISMATCH", placement.placementId, placement.itemId, "lightingResults state must match placement lighting fields."));
                }

                if (arrayById.TryGetValue(placement.placementId, out ItemSystemArrayBonusResultSnapshot array)
                    && (!CellsEqual(placement.OccupiedCells, array.OccupiedCells)
                        || !CellsEqual(placement.OccupiedArrayBonusCells, array.OccupiedArrayBonusCells)
                        || placement.isLit != array.isLit
                        || placement.isLightingSource != array.isLightingSource
                        || placement.isOnArrayBonusCell != array.isOnArrayBonusCell
                        || placement.isArrayBonusActive != array.isArrayBonusActive))
                {
                    errors.Add(ItemSystemValidationError.Error("ARRAY_BONUS_SNAPSHOT_MISMATCH", placement.placementId, placement.itemId, "arrayBonusResults state must match placement array bonus fields."));
                }

                if (buildById.TryGetValue(placement.placementId, out ItemSystemBuildItemSnapshot build)
                    && (placement.isLit != build.isLit
                        || placement.isLightingSource != build.isLightingSource
                        || placement.isCountedInBuild != build.countedInBuild))
                {
                    errors.Add(ItemSystemValidationError.Error("BUILD_SNAPSHOT_MISMATCH", placement.placementId, placement.itemId, "Build item state must match placement Build fields."));
                }

                if (awakeningById.TryGetValue(placement.placementId, out ItemSystemAwakeningResultSnapshot awakening)
                    && (placement.inputLevel != awakening.inputLevel
                        || placement.resolvedLevel != awakening.resolvedLevel
                        || placement.isLit != awakening.isLit
                        || placement.isLightingSource != awakening.isLightingSource
                        || !TextsEqual(placement.UnlockedCoreEffectIds, awakening.UnlockedCoreEffectIds)
                        || !TextsEqual(placement.ActiveCoreEffectIds, awakening.ActiveCoreEffectIds)))
                {
                    errors.Add(ItemSystemValidationError.Error("AWAKENING_SNAPSHOT_MISMATCH", placement.placementId, placement.itemId, "awakeningResults state must match placement awakening fields."));
                }
            }
        }

        private static void ValidateLightingSelfConsistency(ItemSystemSnapshot snapshot, List<ItemSystemValidationError> errors)
        {
            Dictionary<string, ItemSystemLightingResultSnapshot> lightingById = FirstByPlacement(snapshot.lightingResults);
            HashSet<Vector2Int> litCells = new(snapshot.LitRangeCells);
            int sourceCount = 0;
            foreach (ItemSystemLightingResultSnapshot item in snapshot.lightingResults)
            {
                if (item.isLightingSource)
                {
                    sourceCount++;
                    if (!string.Equals(item.itemId, "I031", StringComparison.Ordinal))
                    {
                        errors.Add(ItemSystemValidationError.Error("ORDINARY_ITEM_LIGHTING_SOURCE_FORGED", item.placementId, item.itemId, "ordinary lighting result must not be a source."));
                    }

                    if (!item.isLit || item.litDepth != 0)
                    {
                        errors.Add(ItemSystemValidationError.Error("LIGHTING_SNAPSHOT_MISMATCH", item.placementId, item.itemId, "lighting source must be lit at depth 0."));
                    }
                }

                if (string.Equals(item.itemId, "I031", StringComparison.Ordinal) && !item.isLightingSource)
                {
                    errors.Add(ItemSystemValidationError.Error("LIGHTING_SNAPSHOT_MISMATCH", item.placementId, item.itemId, "I031 lighting result must remain the unique source."));
                }

                if (item.isDirectLit && !item.isLightingSource)
                {
                    if (!item.isLit || !litCells.Contains(item.coreCellWorld))
                    {
                        errors.Add(ItemSystemValidationError.Error("LIGHTING_SNAPSHOT_MISMATCH", item.placementId, item.itemId, "direct-lit item coreCellWorld must be inside LitRangeCells."));
                    }
                }

                if (item.isLit && !item.isLightingSource && !item.isDirectLit)
                {
                    if (string.IsNullOrWhiteSpace(item.litByPlacementId)
                        || !lightingById.TryGetValue(item.litByPlacementId, out ItemSystemLightingResultSnapshot source)
                        || !source.isLit
                        || item.litDepth != source.litDepth + 1)
                    {
                        errors.Add(ItemSystemValidationError.Error("LIGHTING_SNAPSHOT_MISMATCH", item.placementId, item.itemId, "relay-lit item must point to a lit source with depth + 1."));
                    }
                }
            }

            if (sourceCount != 1 && snapshot.lightingResults.Count > 0)
            {
                errors.Add(ItemSystemValidationError.Error("LIGHTING_SNAPSHOT_MISMATCH", string.Empty, "I031", $"lightingResults must contain exactly one source; actual {sourceCount}."));
            }
        }

        private static void ValidateBuildInternalConsistency(ItemSystemSnapshot snapshot, List<ItemSystemValidationError> errors)
        {
            foreach (ItemSystemBuildItemSnapshot item in snapshot.buildSnapshot.ItemResults)
            {
                if (item.countedInBuild && !item.isLit)
                {
                    errors.Add(ItemSystemValidationError.Error("BUILD_SNAPSHOT_MISMATCH", item.placementId, item.itemId, "unlit Build item result must not be countedInBuild."));
                }

                if (item.countedInBuild && string.Equals(item.itemId, "I031", StringComparison.Ordinal))
                {
                    errors.Add(ItemSystemValidationError.Error("BUILD_SNAPSHOT_MISMATCH", item.placementId, item.itemId, "I031 Build item result must not be countedInBuild."));
                }
            }

            foreach (ItemSystemBuildTrackSnapshot track in snapshot.buildSnapshot.FaMenBuilds)
            {
                ValidateBuildTrack(track, ItemBuildTrackKind.FaMen, snapshot.buildSnapshot.ItemResults, errors);
            }

            foreach (ItemSystemBuildTrackSnapshot track in snapshot.buildSnapshot.QiLeiBuilds)
            {
                ValidateBuildTrack(track, ItemBuildTrackKind.QiLei, snapshot.buildSnapshot.ItemResults, errors);
            }
        }

        private static void ValidateBuildTrack(
            ItemSystemBuildTrackSnapshot track,
            ItemBuildTrackKind expectedKind,
            IReadOnlyList<ItemSystemBuildItemSnapshot> itemResults,
            List<ItemSystemValidationError> errors)
        {
            if (track.trackKind != expectedKind)
            {
                errors.Add(ItemSystemValidationError.Error("BUILD_SNAPSHOT_MISMATCH", string.Empty, string.Empty, $"{track.buildId} trackKind must be {expectedKind}."));
            }

            if (string.IsNullOrWhiteSpace(track.stableTag))
            {
                errors.Add(ItemSystemValidationError.Error("BUILD_SNAPSHOT_MISMATCH", string.Empty, string.Empty, $"{track.buildId} stableTag must be non-empty."));
            }

            string expectedBuildId = ExpectedBuildId(track.trackKind, track.stableTag);
            if (!string.IsNullOrWhiteSpace(track.stableTag)
                && !string.Equals(track.buildId, expectedBuildId, StringComparison.Ordinal))
            {
                errors.Add(ItemSystemValidationError.Error("BUILD_SNAPSHOT_MISMATCH", string.Empty, string.Empty, $"{track.buildId} buildId must match {track.trackKind}:{track.stableTag}."));
            }

            ItemSystemBuildItemSnapshot[] expectedSources = (itemResults ?? Array.Empty<ItemSystemBuildItemSnapshot>())
                .Where(item => item != null
                    && item.countedInBuild
                    && string.Equals(BuildIdForKind(item, track.trackKind), track.buildId, StringComparison.Ordinal)
                    && string.Equals(TagForKind(item, track.trackKind), track.stableTag, StringComparison.Ordinal))
                .OrderBy(item => item.placementId, StringComparer.Ordinal)
                .ThenBy(item => item.itemId, StringComparer.Ordinal)
                .ToArray();
            string[] expectedPlacementIds = expectedSources.Select(item => item.placementId).ToArray();
            string[] expectedItemIds = expectedSources
                .Select(item => item.itemId)
                .Where(itemId => !string.IsNullOrWhiteSpace(itemId))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(itemId => itemId, StringComparer.Ordinal)
                .ToArray();
            string[] actualPlacementIds = (track.SourcePlacementIds ?? Array.Empty<string>()).ToArray();
            string[] actualItemIds = (track.SourceItemIds ?? Array.Empty<string>()).ToArray();

            if (actualPlacementIds.Length != actualPlacementIds.Distinct(StringComparer.Ordinal).Count())
            {
                errors.Add(ItemSystemValidationError.Error("BUILD_SNAPSHOT_MISMATCH", string.Empty, string.Empty, $"{track.buildId} SourcePlacementIds must not contain duplicates."));
            }

            if (actualItemIds.Length != expectedItemIds.Length
                || !actualItemIds.SequenceEqual(expectedItemIds))
            {
                errors.Add(ItemSystemValidationError.Error("BUILD_SNAPSHOT_MISMATCH", string.Empty, string.Empty, $"{track.buildId} SourceItemIds must match unique itemIds for counted ItemResults in this track."));
            }

            if (actualPlacementIds.Length != expectedPlacementIds.Length
                || !actualPlacementIds.SequenceEqual(expectedPlacementIds))
            {
                errors.Add(ItemSystemValidationError.Error("BUILD_SNAPSHOT_MISMATCH", string.Empty, string.Empty, $"{track.buildId} SourcePlacementIds must match counted ItemResults for this track."));
            }

            if (track.litItemCount != expectedPlacementIds.Length)
            {
                errors.Add(ItemSystemValidationError.Error("BUILD_SNAPSHOT_MISMATCH", string.Empty, string.Empty, $"{track.buildId} litItemCount must match counted ItemResults for this track."));
            }
        }

        private static string BuildIdForKind(ItemSystemBuildItemSnapshot item, ItemBuildTrackKind kind)
        {
            return kind == ItemBuildTrackKind.QiLei ? item.qiLeiBuildId : item.faMenBuildId;
        }

        private static string TagForKind(ItemSystemBuildItemSnapshot item, ItemBuildTrackKind kind)
        {
            return kind == ItemBuildTrackKind.QiLei ? item.qiLeiTag : item.faMenTag;
        }

        private static string ExpectedBuildId(ItemBuildTrackKind kind, string stableTag)
        {
            string prefix = kind == ItemBuildTrackKind.QiLei
                ? ItemSkillMonitorResolver.QiLeiBuildIdPrefix
                : ItemSkillMonitorResolver.BuildIdPrefix;
            return prefix + (stableTag ?? string.Empty);
        }

        private static Dictionary<string, ItemSystemLightingResultSnapshot> FirstByPlacement(IReadOnlyList<ItemSystemLightingResultSnapshot> items)
        {
            return FirstByPlacement(items, item => item.placementId);
        }

        private static Dictionary<string, ItemSystemArrayBonusResultSnapshot> FirstByPlacement(IReadOnlyList<ItemSystemArrayBonusResultSnapshot> items)
        {
            return FirstByPlacement(items, item => item.placementId);
        }

        private static Dictionary<string, ItemSystemBuildItemSnapshot> FirstByPlacement(IReadOnlyList<ItemSystemBuildItemSnapshot> items)
        {
            return FirstByPlacement(items, item => item.placementId);
        }

        private static Dictionary<string, ItemSystemAwakeningResultSnapshot> FirstByPlacement(IReadOnlyList<ItemSystemAwakeningResultSnapshot> items)
        {
            return FirstByPlacement(items, item => item.placementId);
        }

        private static Dictionary<string, T> FirstByPlacement<T>(IEnumerable<T> items, Func<T, string> placementIdSelector) where T : class
        {
            return (items ?? Array.Empty<T>())
                .Where(item => item != null && !string.IsNullOrWhiteSpace(placementIdSelector(item)))
                .GroupBy(placementIdSelector, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        }

        private static bool CellsEqual(IReadOnlyList<Vector2Int> left, IReadOnlyList<Vector2Int> right)
        {
            return (left ?? Array.Empty<Vector2Int>()).OrderByCell().SequenceEqual((right ?? Array.Empty<Vector2Int>()).OrderByCell());
        }

        private static bool TextsEqual(IReadOnlyList<string> left, IReadOnlyList<string> right)
        {
            return (left ?? Array.Empty<string>()).OrderByText().SequenceEqual((right ?? Array.Empty<string>()).OrderByText());
        }

        private static bool IsValidRotation(int rotation)
        {
            int normalized = ((rotation % 360) + 360) % 360;
            return normalized == rotation && (normalized == 0 || normalized == 90 || normalized == 180 || normalized == 270);
        }

        private static string FormatCell(Vector2Int cell)
        {
            return $"({cell.x},{cell.y})";
        }

        private readonly struct SnapshotResultRef
        {
            public SnapshotResultRef(string placementId, string itemId)
            {
                this.placementId = placementId ?? string.Empty;
                this.itemId = itemId ?? string.Empty;
            }

            public readonly string placementId;
            public readonly string itemId;
        }

        private static void ValidateBuildAndSelection(ItemSystemSnapshot snapshot, List<ItemSystemValidationError> errors)
        {
            if (!string.IsNullOrWhiteSpace(snapshot.buildSnapshot.selectedMainBuildId))
            {
                errors.Add(ItemSystemValidationError.Error("BUILD_AUTO_SELECTED_MAIN", string.Empty, string.Empty, "Build snapshot must not auto-select selectedMainBuildId."));
            }

            if (!string.IsNullOrWhiteSpace(snapshot.selectedMainBuildId))
            {
                if (!snapshot.selectedMainBuildIsExplicit)
                {
                    errors.Add(ItemSystemValidationError.Error("MAIN_BUILD_NOT_EXPLICIT", string.Empty, string.Empty, "non-empty selectedMainBuildId must come from explicit input."));
                }

                if (snapshot.selectedMainBuildId.StartsWith(ItemSkillMonitorResolver.QiLeiBuildIdPrefix, StringComparison.Ordinal))
                {
                    errors.Add(ItemSystemValidationError.Error("MAIN_BUILD_QILEI_SELECTED", string.Empty, string.Empty, "main Build must be a faMen Build, not qiLei."));
                }
                else if (!snapshot.selectedMainBuildId.StartsWith(ItemSkillMonitorResolver.BuildIdPrefix, StringComparison.Ordinal))
                {
                    errors.Add(ItemSystemValidationError.Error("MAIN_BUILD_FORMAT_INVALID", string.Empty, string.Empty, "main Build id must use famen:<stableKey>."));
                }
                else if (snapshot.buildSnapshot.FindFaMenBuild(snapshot.selectedMainBuildId) == null)
                {
                    errors.Add(ItemSystemValidationError.Error("MAIN_BUILD_NOT_FOUND", string.Empty, string.Empty, "selectedMainBuildId must exist in faMen Build snapshot."));
                }
            }
        }

        private static void ValidateSkillMonitor(ItemSystemSnapshot snapshot, List<ItemSystemValidationError> errors)
        {
            IReadOnlyList<ItemSystemSkillMonitorSlotSnapshot> slots = snapshot.skillMonitorSnapshot.Slots;
            bool fixedOrder = slots.Count == 4 && slots.Select(slot => slot.slotType).SequenceEqual(ExpectedSlots);
            if (!fixedOrder)
            {
                errors.Add(ItemSystemValidationError.Error("SKILL_MONITOR_SLOT_ORDER_INVALID", string.Empty, string.Empty, "skill monitor slots must be BasicAttack / Build2 / Build4 / Build6."));
            }

            foreach (ItemSystemSkillMonitorSlotSnapshot slot in slots)
            {
                if (slot.isTriggered || Math.Abs(slot.cooldown01) > 0.0001f || Math.Abs(slot.charge01) > 0.0001f)
                {
                    errors.Add(ItemSystemValidationError.Error("SKILL_MONITOR_RUNTIME_STATE_FORBIDDEN", string.Empty, string.Empty, "skill monitor slots must not expose real trigger, cooldown, or charge state."));
                }
            }
        }

        private static Vector2Int Rotate(Vector2Int cell, int rotation)
        {
            int normalized = ((rotation % 360) + 360) % 360;
            return normalized switch
            {
                90 => new Vector2Int(-cell.y, cell.x),
                180 => new Vector2Int(-cell.x, -cell.y),
                270 => new Vector2Int(cell.y, -cell.x),
                _ => cell
            };
        }
    }

    internal static class ItemSystemSnapshotOrdering
    {
        public static IOrderedEnumerable<Vector2Int> OrderByCell(this IEnumerable<Vector2Int> cells)
        {
            return (cells ?? Array.Empty<Vector2Int>())
                .OrderBy(cell => cell.y)
                .ThenBy(cell => cell.x);
        }

        public static IOrderedEnumerable<string> OrderByText(this IEnumerable<string> values)
        {
            return (values ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal);
        }
    }
}
