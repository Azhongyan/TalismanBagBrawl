using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.Items.Canonical;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Rolling;
using TalismanBag.V04.RewardDrop.Contracts;
using TalismanBag.V04.RewardDrop.Runtime.CampaignLoot;
using UnityEngine;

namespace TalismanBag.Items.CampaignBaseline
{
    public static class C1FormalItemSessionContract
    {
        public const string SessionSchemaId = "C1FormalItemSessionSnapshot.v1";
        public const string BattleInputSchemaId = "C1FormalItemBattleInputSnapshot.v1";
        public const string OrdinaryInstanceSchemaId = "C1FormalItemOrdinaryInstanceSnapshot.v1";
        public const string TrayLayoutSchemaId = "C1FormalItemTrayLayoutSnapshot.v1";
        public const string ProductContext = "CAMPAIGN_NORMAL_LV1";
        public const string ChapterId = "bone_aspect_chapter_1";
        public const string InitialStageId = "1-1";
        public const int BoardSize = 5;
        public const int TrayColumnCount = 5;
        public const int TrayRowCount = 13;
        public const int TrayCellCount = TrayColumnCount * TrayRowCount;
        public const int CommandHistoryCapacity = 256;
    }

    public static class C1FormalItemSessionDiagnosticCodes
    {
        public const string None = "NONE";
        public const string DuplicateAcceptedNoOp = "DUPLICATE_ACCEPTED_NO_OP";
        public const string RequestNull = "REQUEST_NULL";
        public const string SessionTokenRequired = "SESSION_TOKEN_REQUIRED";
        public const string SessionTokenMismatch = "SESSION_TOKEN_MISMATCH";
        public const string ResetGenerationInvalid = "RESET_GENERATION_INVALID";
        public const string ResetGenerationStale = "RESET_GENERATION_STALE";
        public const string CommandIdRequired = "COMMAND_ID_REQUIRED";
        public const string CommandConflict = "COMMAND_CONFLICT";
        public const string CommandHistoryCapacityReached = "COMMAND_HISTORY_CAPACITY_REACHED";
        public const string ExpectedSessionSignatureRequired = "EXPECTED_SESSION_SIGNATURE_REQUIRED";
        public const string ExpectedSessionSignatureStale = "EXPECTED_SESSION_SIGNATURE_STALE";
        public const string FixedBaselineUnavailable = "FIXED_BASELINE_UNAVAILABLE";
        public const string FixedIdentityUnavailable = "FIXED_IDENTITY_UNAVAILABLE";
        public const string FixedIdentitySignatureMismatch = "FIXED_IDENTITY_SIGNATURE_MISMATCH";
        public const string ItemSystemSnapshotInvalid = "ITEM_SYSTEM_SNAPSHOT_INVALID";
        public const string ItemSystemSnapshotMismatch = "ITEM_SYSTEM_SNAPSHOT_MISMATCH";
        public const string ItemInstanceRequired = "ITEM_INSTANCE_REQUIRED";
        public const string ItemInstanceUnknown = "ITEM_INSTANCE_UNKNOWN";
        public const string ItemUnavailable = "ITEM_UNAVAILABLE";
        public const string ItemAlreadyPlaced = "ITEM_ALREADY_PLACED";
        public const string ItemNotPlaced = "ITEM_NOT_PLACED";
        public const string PlacementCandidateRequired = "PLACEMENT_CANDIDATE_REQUIRED";
        public const string PlacementCandidateForbidden = "PLACEMENT_CANDIDATE_FORBIDDEN";
        public const string TrayLayoutInvalid = "TRAY_LAYOUT_INVALID";
        public const string TrayPlacementMissing = "TRAY_PLACEMENT_MISSING";
        public const string TrayPlacementInactive = "TRAY_PLACEMENT_INACTIVE";
        public const string TrayPlacementBounds = "TRAY_PLACEMENT_BOUNDS";
        public const string TrayPlacementCollision = "TRAY_PLACEMENT_COLLISION";
        public const string TrayPlacementUnavailable = "TRAY_PLACEMENT_UNAVAILABLE";
        public const string CommandKindUnsupported = "COMMAND_KIND_UNSUPPORTED";
        public const string RewardFactsRequired = "REWARD_FACTS_REQUIRED";
        public const string RewardContractInvalid = "REWARD_CONTRACT_INVALID";
        public const string RewardSourceMismatch = "REWARD_SOURCE_MISMATCH";
        public const string RewardEntryMismatch = "REWARD_ENTRY_MISMATCH";
        public const string RewardClaimMismatch = "REWARD_CLAIM_MISMATCH";
        public const string RewardDedupeMismatch = "REWARD_DEDUPE_MISMATCH";
        public const string EntitlementConflict = "ENTITLEMENT_CONFLICT";
        public const string EntitlementCarrierUnavailable = "ENTITLEMENT_CARRIER_UNAVAILABLE";
    }

    public enum C1FormalItemArrangementCommandKind
    {
        Unspecified = 0,
        PlaceFromTray = 1,
        MoveOnBoard = 2,
        ReturnToTray = 3,
        MoveWithinTray = 4,
        ReplaceSameBaseFromTray = 5
    }

    public static class C1FormalItemTrayLayoutRules
    {
        public static bool TryBuildNormalizedOffsets(
            IReadOnlyList<Vector2Int> authoritativeRawShapeCells,
            int formalDegrees,
            out IReadOnlyList<Vector2Int> normalizedOffsets)
        {
            normalizedOffsets = Array.Empty<Vector2Int>();
            if (authoritativeRawShapeCells == null
                || authoritativeRawShapeCells.Count == 0
                || authoritativeRawShapeCells.Distinct().Count()
                   != authoritativeRawShapeCells.Count)
                return false;

            List<Vector2Int> rotated = new List<Vector2Int>();
            foreach (Vector2Int rawCell in authoritativeRawShapeCells)
            {
                if (!TryRotate(rawCell, formalDegrees, out Vector2Int value))
                    return false;
                rotated.Add(value);
            }

            if (rotated.Distinct().Count() != rotated.Count) return false;
            int minX = rotated.Min(value => value.x);
            int minY = rotated.Min(value => value.y);
            int maxY = rotated.Max(value => value.y);
            Vector2Int[] offsets = rotated
                .Select(value => new Vector2Int(
                    value.x - minX,
                    maxY - value.y))
                .Distinct()
                .OrderBy(value => value.y)
                .ThenBy(value => value.x)
                .ToArray();
            if (offsets.Length != rotated.Count) return false;
            normalizedOffsets = Array.AsReadOnly(offsets);
            return true;
        }

        public static bool TryBuildCandidate(
            IReadOnlyList<Vector2Int> authoritativeRawShapeCells,
            Vector2Int rawGrabbedCell,
            Vector2Int pointerCell,
            int formalDegrees,
            out C1FormalItemPlacementCandidate candidate,
            out IReadOnlyList<Vector2Int> orderedOccupiedCells)
        {
            candidate = null;
            orderedOccupiedCells = Array.Empty<Vector2Int>();
            if (authoritativeRawShapeCells == null
                || !authoritativeRawShapeCells.Contains(rawGrabbedCell)
                || !TryBuildNormalizedOffsets(
                    authoritativeRawShapeCells,
                    formalDegrees,
                    out IReadOnlyList<Vector2Int> offsets))
                return false;

            if (!TryRotate(rawGrabbedCell, formalDegrees, out Vector2Int rotatedGrabbed))
                return false;
            List<Vector2Int> rotated = new List<Vector2Int>();
            foreach (Vector2Int rawCell in authoritativeRawShapeCells)
            {
                if (!TryRotate(rawCell, formalDegrees, out Vector2Int value))
                    return false;
                rotated.Add(value);
            }

            int minX = rotated.Min(value => value.x);
            int maxY = rotated.Max(value => value.y);
            Vector2Int grabbedOffset = new Vector2Int(
                rotatedGrabbed.x - minX,
                maxY - rotatedGrabbed.y);
            Vector2Int anchor = pointerCell - grabbedOffset;
            Vector2Int[] occupied = offsets
                .Select(value => anchor + value)
                .OrderBy(value => value.y)
                .ThenBy(value => value.x)
                .ToArray();
            candidate = new C1FormalItemPlacementCandidate(anchor, formalDegrees);
            orderedOccupiedCells = Array.AsReadOnly(occupied);
            return true;
        }

        public static bool TryBuildOccupiedCells(
            IReadOnlyList<Vector2Int> authoritativeRawShapeCells,
            C1FormalItemPlacementCandidate candidate,
            out IReadOnlyList<Vector2Int> orderedOccupiedCells)
        {
            orderedOccupiedCells = Array.Empty<Vector2Int>();
            if (candidate == null
                || !TryBuildNormalizedOffsets(
                    authoritativeRawShapeCells,
                    candidate.rotation,
                    out IReadOnlyList<Vector2Int> offsets))
                return false;
            Vector2Int[] occupied = offsets
                .Select(value => candidate.anchorCell + value)
                .OrderBy(value => value.y)
                .ThenBy(value => value.x)
                .ToArray();
            orderedOccupiedCells = Array.AsReadOnly(occupied);
            return true;
        }

        public static bool IsInsideTray(IReadOnlyList<Vector2Int> cells)
        {
            return cells != null && cells.Count > 0 && cells.All(value =>
                value.x >= 0
                && value.x < C1FormalItemSessionContract.TrayColumnCount
                && value.y >= 0
                && value.y < C1FormalItemSessionContract.TrayRowCount);
        }

        private static bool TryRotate(
            Vector2Int cell,
            int formalDegrees,
            out Vector2Int rotated)
        {
            switch (formalDegrees)
            {
                case 0:
                    rotated = cell;
                    return true;
                case 90:
                    rotated = new Vector2Int(-cell.y, cell.x);
                    return true;
                case 180:
                    rotated = new Vector2Int(-cell.x, -cell.y);
                    return true;
                case 270:
                    rotated = new Vector2Int(cell.y, -cell.x);
                    return true;
                default:
                    rotated = default;
                    return false;
            }
        }
    }

    public sealed class C1FormalItemPlacementCandidate
    {
        public C1FormalItemPlacementCandidate(Vector2Int anchorCell, int rotation)
        {
            this.anchorCell = anchorCell;
            this.rotation = rotation;
        }

        public Vector2Int anchorCell { get; }
        public int rotation { get; }
    }

    public sealed class C1FormalItemArrangementCommand
    {
        public C1FormalItemArrangementCommand(
            C1FormalItemArrangementCommandKind commandKind,
            string sessionToken,
            long resetGeneration,
            string commandId,
            string itemInstanceId,
            string expectedPriorSessionCanonicalSignature,
            C1FormalItemPlacementCandidate placementCandidate)
        {
            this.commandKind = commandKind;
            this.sessionToken = C1FormalItemCanonical.Normalize(sessionToken);
            this.resetGeneration = resetGeneration;
            this.commandId = C1FormalItemCanonical.Normalize(commandId);
            this.itemInstanceId = C1FormalItemCanonical.Normalize(itemInstanceId);
            this.expectedPriorSessionCanonicalSignature =
                C1FormalItemCanonical.Normalize(expectedPriorSessionCanonicalSignature);
            this.placementCandidate = placementCandidate == null
                ? null
                : new C1FormalItemPlacementCandidate(
                    placementCandidate.anchorCell,
                    placementCandidate.rotation);
            canonicalSignature = C1FormalItemCanonical.Hash(string.Join("|", new[]
            {
                "kind=" + ((int)this.commandKind).ToString(CultureInfo.InvariantCulture),
                "sessionToken=" + this.sessionToken,
                "resetGeneration=" + this.resetGeneration.ToString(CultureInfo.InvariantCulture),
                "commandId=" + this.commandId,
                "itemInstanceId=" + this.itemInstanceId,
                "expectedPrior=" + this.expectedPriorSessionCanonicalSignature,
                "candidate=" + (this.placementCandidate == null
                    ? "absent"
                    : C1FormalItemCanonical.Cell(this.placementCandidate.anchorCell)
                      + "@" + this.placementCandidate.rotation.ToString(CultureInfo.InvariantCulture))
            }));
        }

        public C1FormalItemArrangementCommandKind commandKind { get; }
        public string sessionToken { get; }
        public long resetGeneration { get; }
        public string commandId { get; }
        public string itemInstanceId { get; }
        public string expectedPriorSessionCanonicalSignature { get; }
        public C1FormalItemPlacementCandidate placementCandidate { get; }
        public string canonicalSignature { get; }
    }

    public sealed class C1FormalItemResetCommand
    {
        public C1FormalItemResetCommand(
            string sessionToken,
            long resetGeneration,
            string commandId,
            string expectedPriorSessionCanonicalSignature)
        {
            this.sessionToken = C1FormalItemCanonical.Normalize(sessionToken);
            this.resetGeneration = resetGeneration;
            this.commandId = C1FormalItemCanonical.Normalize(commandId);
            this.expectedPriorSessionCanonicalSignature =
                C1FormalItemCanonical.Normalize(expectedPriorSessionCanonicalSignature);
            canonicalSignature = C1FormalItemCanonical.Hash(string.Join("|", new[]
            {
                "kind=RESET",
                "sessionToken=" + this.sessionToken,
                "resetGeneration=" + this.resetGeneration.ToString(CultureInfo.InvariantCulture),
                "commandId=" + this.commandId,
                "expectedPrior=" + this.expectedPriorSessionCanonicalSignature
            }));
        }

        public string sessionToken { get; }
        public long resetGeneration { get; }
        public string commandId { get; }
        public string expectedPriorSessionCanonicalSignature { get; }
        public string canonicalSignature { get; }
    }

    public sealed class C1FormalItemOrdinaryInstanceSnapshot
    {
        internal C1FormalItemOrdinaryInstanceSnapshot(
            ItemInstanceIdentitySnapshot ordinaryIdentity,
            string identityCanonicalSignature,
            string baselineProjectionCanonicalSignature,
            string itemCatalogCanonicalSignature,
            string sourceKind,
            string instanceCanonicalSignature,
            ItemGeneratedInstanceSnapshot generatedInstance,
            CanonicalItemDefinition canonicalDefinition)
        {
            this.ordinaryIdentity = ordinaryIdentity
                ?? throw new ArgumentNullException(nameof(ordinaryIdentity));
            schemaId = C1FormalItemSessionContract.OrdinaryInstanceSchemaId;
            itemInstanceId = ordinaryIdentity.itemInstanceId;
            baseItemId = ordinaryIdentity.baseItemId;
            rarity = ordinaryIdentity.rarity;
            rarityKey = ordinaryIdentity.rarity.ToStableKey();
            this.identityCanonicalSignature = identityCanonicalSignature ?? string.Empty;
            this.baselineProjectionCanonicalSignature =
                baselineProjectionCanonicalSignature ?? string.Empty;
            this.itemCatalogCanonicalSignature = itemCatalogCanonicalSignature ?? string.Empty;
            this.sourceKind = sourceKind ?? string.Empty;
            this.instanceCanonicalSignature = instanceCanonicalSignature ?? string.Empty;
            this.generatedInstance = generatedInstance;
            this.canonicalDefinition = canonicalDefinition;
        }

        public string schemaId { get; }
        public ItemInstanceIdentitySnapshot ordinaryIdentity { get; }
        public string itemInstanceId { get; }
        public string baseItemId { get; }
        public ItemInstanceRarity rarity { get; }
        public string rarityKey { get; }
        public string identityCanonicalSignature { get; }
        public string baselineProjectionCanonicalSignature { get; }
        public string itemCatalogCanonicalSignature { get; }
        public string sourceKind { get; }
        public string instanceCanonicalSignature { get; }
        public ItemGeneratedInstanceSnapshot generatedInstance { get; }
        public CanonicalItemDefinition canonicalDefinition { get; }
    }

    public sealed class C1FormalItemRosterEntrySnapshot
    {
        internal C1FormalItemRosterEntrySnapshot(
            string itemInstanceId,
            string baseItemId,
            bool isSpecialLightingSource,
            string rarityKey,
            string baselineProjectionCanonicalSignature,
            string itemCatalogCanonicalSignature,
            string instanceCanonicalSignature,
            C1FormalItemOrdinaryInstanceSnapshot ordinaryInstance)
        {
            this.itemInstanceId = itemInstanceId ?? string.Empty;
            this.baseItemId = baseItemId ?? string.Empty;
            this.isSpecialLightingSource = isSpecialLightingSource;
            this.rarityKey = rarityKey ?? string.Empty;
            this.baselineProjectionCanonicalSignature =
                baselineProjectionCanonicalSignature ?? string.Empty;
            this.itemCatalogCanonicalSignature = itemCatalogCanonicalSignature ?? string.Empty;
            this.instanceCanonicalSignature = instanceCanonicalSignature ?? string.Empty;
            this.ordinaryInstance = ordinaryInstance;
        }

        public string itemInstanceId { get; }
        public string baseItemId { get; }
        public bool isSpecialLightingSource { get; }
        public string rarityKey { get; }
        public string baselineProjectionCanonicalSignature { get; }
        public string itemCatalogCanonicalSignature { get; }
        public string instanceCanonicalSignature { get; }
        public C1FormalItemOrdinaryInstanceSnapshot ordinaryInstance { get; }
    }

    public sealed class C1FormalItemPlacementSnapshot
    {
        internal C1FormalItemPlacementSnapshot(
            string itemInstanceId,
            string placementId,
            string baseItemId,
            Vector2Int anchorCell,
            int rotation)
        {
            this.itemInstanceId = itemInstanceId ?? string.Empty;
            this.placementId = placementId ?? string.Empty;
            this.baseItemId = baseItemId ?? string.Empty;
            this.anchorCell = anchorCell;
            this.rotation = rotation;
        }

        public string itemInstanceId { get; }
        public string placementId { get; }
        public string baseItemId { get; }
        public Vector2Int anchorCell { get; }
        public int rotation { get; }
    }

    public sealed class C1FormalItemTrayPlacementSnapshot
    {
        private readonly ReadOnlyCollection<Vector2Int> occupiedCellsValue;

        internal C1FormalItemTrayPlacementSnapshot(
            string itemInstanceId,
            Vector2Int anchorCell,
            int rotation,
            IEnumerable<Vector2Int> occupiedCells,
            bool isActiveInTray)
        {
            this.itemInstanceId = itemInstanceId ?? string.Empty;
            this.anchorCell = anchorCell;
            this.rotation = rotation;
            occupiedCellsValue = Array.AsReadOnly((occupiedCells
                ?? Enumerable.Empty<Vector2Int>())
                .Distinct()
                .OrderBy(value => value.y)
                .ThenBy(value => value.x)
                .ToArray());
            this.isActiveInTray = isActiveInTray;
            canonicalSignature = C1FormalItemCanonical.Hash(string.Join("|", new[]
            {
                this.itemInstanceId,
                C1FormalItemCanonical.Cell(this.anchorCell),
                this.rotation.ToString(CultureInfo.InvariantCulture),
                this.isActiveInTray ? "active" : "remembered",
                string.Join(";", occupiedCellsValue.Select(
                    C1FormalItemCanonical.Cell))
            }));
        }

        public string itemInstanceId { get; }
        public Vector2Int anchorCell { get; }
        public int rotation { get; }
        public IReadOnlyList<Vector2Int> occupiedCells => occupiedCellsValue;
        public bool isActiveInTray { get; }
        public string canonicalSignature { get; }
    }

    public sealed class C1FormalItemTrayLayoutSnapshot
    {
        private readonly ReadOnlyCollection<C1FormalItemTrayPlacementSnapshot>
            placementsValue;

        internal C1FormalItemTrayLayoutSnapshot(
            IEnumerable<C1FormalItemTrayPlacementSnapshot> placements)
        {
            schemaId = C1FormalItemSessionContract.TrayLayoutSchemaId;
            columnCount = C1FormalItemSessionContract.TrayColumnCount;
            rowCount = C1FormalItemSessionContract.TrayRowCount;
            cellCount = C1FormalItemSessionContract.TrayCellCount;
            placementsValue = Array.AsReadOnly((placements
                ?? Enumerable.Empty<C1FormalItemTrayPlacementSnapshot>())
                .Where(value => value != null)
                .OrderBy(value => value.itemInstanceId, StringComparer.Ordinal)
                .ToArray());
            canonicalSignature = C1FormalItemCanonical.Hash(string.Join("\n",
                placementsValue.Select(value => value.canonicalSignature)));
        }

        public string schemaId { get; }
        public int columnCount { get; }
        public int rowCount { get; }
        public int cellCount { get; }
        public IReadOnlyList<C1FormalItemTrayPlacementSnapshot> placements =>
            placementsValue;
        public string canonicalSignature { get; }

        public C1FormalItemTrayPlacementSnapshot FindPlacementByInstanceId(
            string itemInstanceId)
        {
            return placementsValue.FirstOrDefault(value => string.Equals(
                value.itemInstanceId,
                itemInstanceId,
                StringComparison.Ordinal));
        }
    }

    public sealed class C1FormalItemBattleItemRow
    {
        internal C1FormalItemBattleItemRow(
            C1FormalItemRosterEntrySnapshot roster,
            C1FormalItemPlacementSnapshot placement,
            bool isPlaced,
            bool? isLit,
            bool? isDirectLit)
        {
            itemInstanceId = roster.itemInstanceId;
            baseItemId = roster.baseItemId;
            rarityKey = roster.rarityKey;
            baselineProjectionCanonicalSignature =
                roster.baselineProjectionCanonicalSignature;
            itemCatalogCanonicalSignature = roster.itemCatalogCanonicalSignature;
            instanceCanonicalSignature = roster.instanceCanonicalSignature;
            generatedInstance = roster.ordinaryInstance?.generatedInstance;
            canonicalDefinition = roster.ordinaryInstance?.canonicalDefinition;
            placementId = placement?.placementId ?? string.Empty;
            anchorCell = placement == null
                ? (Vector2Int?)null
                : placement.anchorCell;
            rotation = placement?.rotation ?? 0;
            if (C1Lv1FormalItemActionCostCatalog.DefinesActiveAction(
                    roster.baseItemId))
            {
                C1FormalItemActionCostProjectionResult actionCost =
                    C1Lv1FormalItemActionCostCatalog.TryGetActionCost(
                        C1FormalItemSessionContract.ProductContext,
                        roster.baseItemId,
                        roster.baseItemId + "@" + roster.rarityKey,
                        C1Lv1StarterItemBaselineCatalog.ProfileRevision,
                        C1Lv1FormalItemActionCostCatalog.ActionDefinitionId,
                        C1Lv1FormalItemActionCostCatalog.CadenceMilliseconds,
                        C1Lv1FormalItemActionCostCatalog
                            .FirstOffsetMilliseconds);
                if (actionCost == null || !actionCost.isSuccess
                    || actionCost.fact == null)
                {
                    throw new InvalidOperationException(
                        "The released Item action cost projection rejected "
                        + roster.itemInstanceId + ".");
                }

                actionCostFact = actionCost.fact;
            }
            this.isPlaced = isPlaced;
            this.isLit = isLit;
            this.isDirectLit = isDirectLit;
        }

        public string itemInstanceId { get; }
        public string baseItemId { get; }
        public string rarityKey { get; }
        public string baselineProjectionCanonicalSignature { get; }
        public string itemCatalogCanonicalSignature { get; }
        public string instanceCanonicalSignature { get; }
        public C1FormalItemActionCostFact actionCostFact { get; }
        public ItemGeneratedInstanceSnapshot generatedInstance { get; }
        public CanonicalItemDefinition canonicalDefinition { get; }
        public string placementId { get; }
        public Vector2Int? anchorCell { get; }
        public int rotation { get; }
        public bool isPlaced { get; }
        public bool? isLit { get; }
        public bool? isDirectLit { get; }
    }

    public sealed class C1FormalI031NianCapacityFact
    {
        internal C1FormalI031NianCapacityFact(
            string specialItemInstanceId,
            string baseItemId,
            string stablePlacementId,
            Vector2Int anchorCell,
            string sessionCanonicalSignature,
            string arrangementCanonicalSignature,
            string itemSystemCanonicalSignature)
        {
            schemaId = C1FormalI031NianCapacityProjection.SchemaId;
            productContext = C1FormalItemSessionContract.ProductContext;
            resourceKey = C1FormalI031NianCapacityProjection.ResourceKey;
            initialNian = C1FormalI031NianCapacityProjection.InitialNian;
            maxNian = C1FormalI031NianCapacityProjection.MaxNian;
            generationAmount =
                C1FormalI031NianCapacityProjection.GenerationAmount;
            generationIntervalMilliseconds =
                C1FormalI031NianCapacityProjection
                    .GenerationIntervalMilliseconds;
            sourceRevision = C1FormalI031NianCapacityProjection.SourceRevision;
            sourceProfileId = C1FormalI031NianCapacityProjection.SourceProfileId;
            this.specialItemInstanceId = specialItemInstanceId ?? string.Empty;
            this.baseItemId = baseItemId ?? string.Empty;
            this.stablePlacementId = stablePlacementId ?? string.Empty;
            this.anchorCell = anchorCell;
            this.sessionCanonicalSignature =
                sessionCanonicalSignature ?? string.Empty;
            this.arrangementCanonicalSignature =
                arrangementCanonicalSignature ?? string.Empty;
            this.itemSystemCanonicalSignature =
                itemSystemCanonicalSignature ?? string.Empty;
        }

        public string schemaId { get; }
        public string productContext { get; }
        public string resourceKey { get; }
        public int initialNian { get; }
        public int maxNian { get; }
        public int generationAmount { get; }
        public long generationIntervalMilliseconds { get; }
        public string sourceRevision { get; }
        public string sourceProfileId { get; }
        public string specialItemInstanceId { get; }
        public string baseItemId { get; }
        public string stablePlacementId { get; }
        public Vector2Int anchorCell { get; }
        public string sessionCanonicalSignature { get; }
        public string arrangementCanonicalSignature { get; }
        public string itemSystemCanonicalSignature { get; }
    }

    public sealed class C1FormalI031NianCapacityProjectionRequest
    {
        public C1FormalI031NianCapacityProjectionRequest(
            string expectedSessionCanonicalSignature,
            string expectedArrangementCanonicalSignature,
            string expectedItemSystemCanonicalSignature,
            string specialItemInstanceId,
            string baseItemId,
            string stablePlacementId,
            Vector2Int? anchorCell)
        {
            this.expectedSessionCanonicalSignature =
                expectedSessionCanonicalSignature ?? string.Empty;
            this.expectedArrangementCanonicalSignature =
                expectedArrangementCanonicalSignature ?? string.Empty;
            this.expectedItemSystemCanonicalSignature =
                expectedItemSystemCanonicalSignature ?? string.Empty;
            this.specialItemInstanceId = specialItemInstanceId ?? string.Empty;
            this.baseItemId = baseItemId ?? string.Empty;
            this.stablePlacementId = stablePlacementId ?? string.Empty;
            this.anchorCell = anchorCell;
        }

        public string expectedSessionCanonicalSignature { get; }
        public string expectedArrangementCanonicalSignature { get; }
        public string expectedItemSystemCanonicalSignature { get; }
        public string specialItemInstanceId { get; }
        public string baseItemId { get; }
        public string stablePlacementId { get; }
        public Vector2Int? anchorCell { get; }
    }

    public sealed class C1FormalI031NianCapacityProjectionResult
    {
        private C1FormalI031NianCapacityProjectionResult(
            bool isSuccess,
            C1FormalI031NianCapacityFact fact,
            string diagnosticCode)
        {
            this.isSuccess = isSuccess;
            this.fact = fact;
            this.diagnosticCode = diagnosticCode ?? string.Empty;
        }

        public bool isSuccess { get; }
        public C1FormalI031NianCapacityFact fact { get; }
        public string diagnosticCode { get; }

        internal static C1FormalI031NianCapacityProjectionResult Accepted(
            C1FormalI031NianCapacityFact fact)
        {
            return new C1FormalI031NianCapacityProjectionResult(
                true,
                fact,
                C1FormalItemSessionDiagnosticCodes.None);
        }

        internal static C1FormalI031NianCapacityProjectionResult Rejected()
        {
            return new C1FormalI031NianCapacityProjectionResult(
                false,
                null,
                C1FormalI031NianCapacityProjection.UnavailableDiagnosticCode);
        }
    }

    public static class C1FormalI031NianCapacityProjection
    {
        public const string SchemaId = "C1FormalI031NianCapacityFact.v1";
        public const string ResourceKey = "nian";
        public const int InitialNian = 50;
        public const int MaxNian = 50;
        public const int GenerationAmount = 10;
        public const long GenerationIntervalMilliseconds = 500L;
        public const string SourceRevision =
            "C1_CAMPAIGN_LV1_I031_NIAN_ECONOMY_FORMAL_V1";
        public const string SourceProfileId =
            "campaign.normal.lv1.item.i031.nian-economy.formal.v1";
        public const string UnavailableDiagnosticCode =
            "I031_NIAN_CAPACITY_UNAVAILABLE";

        public static C1FormalI031NianCapacityProjectionRequest
            CreateCurrentRequest(C1FormalItemSessionSnapshot snapshot)
        {
            if (snapshot == null) return null;
            C1FormalItemPlacementSnapshot source =
                snapshot.FindPlacementByInstanceId(
                    I031InventoryPlacementContract.SpecialIdentityId);
            return new C1FormalI031NianCapacityProjectionRequest(
                snapshot.canonicalSignature,
                snapshot.arrangementCanonicalSignature,
                snapshot.itemSystemCanonicalSignature,
                I031InventoryPlacementContract.SpecialIdentityId,
                I031InventoryPlacementContract.ItemId,
                I031InventoryPlacementContract.StablePlacementId,
                source == null ? (Vector2Int?)null : source.anchorCell);
        }

        public static C1FormalI031NianCapacityProjectionResult TryProject(
            C1FormalItemSessionSnapshot snapshot,
            C1FormalI031NianCapacityProjectionRequest request)
        {
            if (snapshot == null || request == null
                || request.expectedSessionCanonicalSignature.Length == 0
                || request.expectedArrangementCanonicalSignature.Length == 0
                || request.expectedItemSystemCanonicalSignature.Length == 0
                || !string.Equals(
                    request.expectedSessionCanonicalSignature,
                    snapshot.canonicalSignature,
                    StringComparison.Ordinal)
                || !string.Equals(
                    request.expectedArrangementCanonicalSignature,
                    snapshot.arrangementCanonicalSignature,
                    StringComparison.Ordinal)
                || !string.Equals(
                    request.expectedItemSystemCanonicalSignature,
                    snapshot.itemSystemCanonicalSignature,
                    StringComparison.Ordinal))
            {
                return C1FormalI031NianCapacityProjectionResult.Rejected();
            }

            C1FormalItemRosterEntrySnapshot[] specialRows = snapshot.roster
                .Where(value => value != null && value.isSpecialLightingSource)
                .ToArray();
            C1FormalItemRosterEntrySnapshot sourceRoster =
                specialRows.Length == 1 ? specialRows[0] : null;
            C1FormalItemPlacementSnapshot sourcePlacement =
                snapshot.FindPlacementByInstanceId(request.specialItemInstanceId);
            ItemSystemPlacementSnapshot itemSystemPlacement =
                snapshot.itemSystemSnapshot?.FindPlacement(
                    request.stablePlacementId);
            I031InventoryPlacementStateSnapshot i031State =
                snapshot.itemSystemSnapshot?.i031State;

            bool exact = string.Equals(
                             request.specialItemInstanceId,
                             I031InventoryPlacementContract.SpecialIdentityId,
                             StringComparison.Ordinal)
                         && string.Equals(
                             request.baseItemId,
                             I031InventoryPlacementContract.ItemId,
                             StringComparison.Ordinal)
                         && string.Equals(
                             request.stablePlacementId,
                             I031InventoryPlacementContract.StablePlacementId,
                             StringComparison.Ordinal)
                         && request.anchorCell.HasValue
                         && sourceRoster != null
                         && string.Equals(
                             sourceRoster.itemInstanceId,
                             request.specialItemInstanceId,
                             StringComparison.Ordinal)
                         && string.Equals(
                             sourceRoster.baseItemId,
                             request.baseItemId,
                             StringComparison.Ordinal)
                         && sourcePlacement != null
                         && string.Equals(
                             sourcePlacement.placementId,
                             request.stablePlacementId,
                             StringComparison.Ordinal)
                         && string.Equals(
                             sourcePlacement.baseItemId,
                             request.baseItemId,
                             StringComparison.Ordinal)
                         && sourcePlacement.anchorCell == request.anchorCell.Value
                         && itemSystemPlacement != null
                         && string.Equals(
                             itemSystemPlacement.itemId,
                             request.baseItemId,
                             StringComparison.Ordinal)
                         && itemSystemPlacement.anchorCell
                            == sourcePlacement.anchorCell
                         && itemSystemPlacement.rotation
                            == sourcePlacement.rotation
                         && itemSystemPlacement.isLightingSource
                         && i031State != null
                         && i031State.isOwned
                         && i031State.isPlaced
                         && i031State.ownershipCompleteness
                            == I031OwnershipCompleteness.Complete
                         && string.Equals(
                             i031State.itemId,
                             request.baseItemId,
                             StringComparison.Ordinal)
                         && string.Equals(
                             i031State.specialIdentityId,
                             request.specialItemInstanceId,
                             StringComparison.Ordinal)
                         && string.Equals(
                             i031State.stablePlacementId,
                             request.stablePlacementId,
                             StringComparison.Ordinal);
            if (!exact)
                return C1FormalI031NianCapacityProjectionResult.Rejected();

            return C1FormalI031NianCapacityProjectionResult.Accepted(
                new C1FormalI031NianCapacityFact(
                    sourceRoster.itemInstanceId,
                    sourceRoster.baseItemId,
                    sourcePlacement.placementId,
                    sourcePlacement.anchorCell,
                    snapshot.canonicalSignature,
                    snapshot.arrangementCanonicalSignature,
                    snapshot.itemSystemCanonicalSignature));
        }
    }

    public sealed class C1FormalItemBattleInputSnapshot
    {
        private readonly ReadOnlyCollection<C1FormalItemBattleItemRow> itemRowsValue;

        internal C1FormalItemBattleInputSnapshot(
            string sessionCanonicalSignature,
            string arrangementCanonicalSignature,
            string itemSystemCanonicalSignature,
            string sourcePlacementId,
            Vector2Int? sourceAnchorCell,
            C1FormalI031NianCapacityFact i031NianCapacityFact,
            IEnumerable<C1FormalItemBattleItemRow> itemRows)
        {
            schemaId = C1FormalItemSessionContract.BattleInputSchemaId;
            productContext = C1FormalItemSessionContract.ProductContext;
            this.sessionCanonicalSignature = sessionCanonicalSignature ?? string.Empty;
            this.arrangementCanonicalSignature = arrangementCanonicalSignature ?? string.Empty;
            this.itemSystemCanonicalSignature = itemSystemCanonicalSignature ?? string.Empty;
            this.sourcePlacementId = sourcePlacementId ?? string.Empty;
            this.sourceAnchorCell = sourceAnchorCell;
            this.i031NianCapacityFact = i031NianCapacityFact;
            itemRowsValue = Array.AsReadOnly((itemRows
                ?? Enumerable.Empty<C1FormalItemBattleItemRow>())
                .Where(row => row != null)
                .OrderBy(row => row.itemInstanceId, StringComparer.Ordinal)
                .ToArray());
            canonicalSignature = C1FormalItemCanonical.Hash(BuildCanonicalPayload());
        }

        public string schemaId { get; }
        public string productContext { get; }
        public string sessionCanonicalSignature { get; }
        public string arrangementCanonicalSignature { get; }
        public string itemSystemCanonicalSignature { get; }
        public string sourcePlacementId { get; }
        public Vector2Int? sourceAnchorCell { get; }
        public C1FormalI031NianCapacityFact i031NianCapacityFact { get; }
        public IReadOnlyList<C1FormalItemBattleItemRow> itemRows => itemRowsValue;
        public string canonicalSignature { get; }

        private string BuildCanonicalPayload()
        {
            StringBuilder value = new StringBuilder();
            value.Append(schemaId).Append('|').Append(productContext).Append('|')
                .Append(sessionCanonicalSignature).Append('|')
                .Append(arrangementCanonicalSignature).Append('|')
                .Append(itemSystemCanonicalSignature).Append('|')
                .Append(sourcePlacementId).Append('|')
                .Append(sourceAnchorCell.HasValue
                    ? C1FormalItemCanonical.Cell(sourceAnchorCell.Value)
                    : "absent");
            foreach (C1FormalItemBattleItemRow row in itemRowsValue)
            {
                value.Append("\nROW|").Append(row.itemInstanceId).Append('|')
                    .Append(row.baseItemId).Append('|').Append(row.rarityKey).Append('|')
                    .Append(row.baselineProjectionCanonicalSignature).Append('|')
                    .Append(row.itemCatalogCanonicalSignature).Append('|')
                    .Append(row.instanceCanonicalSignature).Append('|')
                    .Append(row.isPlaced ? "placed" : "tray").Append('|')
                    .Append(row.isLit.HasValue ? (row.isLit.Value ? "lit" : "unlit") : "absent");
            }

            return value.ToString();
        }
    }

    public sealed class C1FormalItemSessionSnapshot
    {
        private readonly ReadOnlyCollection<C1FormalItemRosterEntrySnapshot> rosterValue;
        private readonly ReadOnlyCollection<C1FormalItemPlacementSnapshot> placementsValue;

        internal C1FormalItemSessionSnapshot(
            string sessionToken,
            long resetGeneration,
            IEnumerable<C1FormalItemRosterEntrySnapshot> roster,
            IEnumerable<C1FormalItemPlacementSnapshot> placements,
            C1FormalItemTrayLayoutSnapshot trayLayout,
            ItemSystemSnapshot itemSystemSnapshot,
            string entitlementCanonicalSignature)
        {
            schemaId = C1FormalItemSessionContract.SessionSchemaId;
            productContext = C1FormalItemSessionContract.ProductContext;
            chapterId = C1FormalItemSessionContract.ChapterId;
            this.sessionToken = sessionToken ?? string.Empty;
            this.resetGeneration = resetGeneration;
            rosterValue = Array.AsReadOnly((roster
                ?? Enumerable.Empty<C1FormalItemRosterEntrySnapshot>())
                .Where(value => value != null)
                .OrderBy(value => value.itemInstanceId, StringComparer.Ordinal)
                .ToArray());
            placementsValue = Array.AsReadOnly((placements
                ?? Enumerable.Empty<C1FormalItemPlacementSnapshot>())
                .Where(value => value != null)
                .OrderBy(value => value.itemInstanceId, StringComparer.Ordinal)
                .ToArray());
            this.trayLayout = trayLayout
                ?? throw new ArgumentNullException(nameof(trayLayout));
            this.itemSystemSnapshot = itemSystemSnapshot
                ?? throw new ArgumentNullException(nameof(itemSystemSnapshot));
            this.entitlementCanonicalSignature = entitlementCanonicalSignature ?? string.Empty;
            itemSystemCanonicalSignature = C1FormalItemCanonical.Hash(
                itemSystemSnapshot.BuildDebugSignature());
            arrangementCanonicalSignature = C1FormalItemCanonical.Hash(
                BuildArrangementPayload());
            canonicalSignature = C1FormalItemCanonical.Hash(BuildSessionPayload());
        }

        public string schemaId { get; }
        public string productContext { get; }
        public string chapterId { get; }
        public string sessionToken { get; }
        public long resetGeneration { get; }
        public IReadOnlyList<C1FormalItemRosterEntrySnapshot> roster => rosterValue;
        public IReadOnlyList<C1FormalItemPlacementSnapshot> placements => placementsValue;
        public C1FormalItemTrayLayoutSnapshot trayLayout { get; }
        public ItemSystemSnapshot itemSystemSnapshot { get; }
        public string entitlementCanonicalSignature { get; }
        public string itemSystemCanonicalSignature { get; }
        public string arrangementCanonicalSignature { get; }
        public string canonicalSignature { get; }

        public C1FormalItemRosterEntrySnapshot FindRosterEntry(string itemInstanceId)
        {
            return rosterValue.FirstOrDefault(value => string.Equals(
                value.itemInstanceId,
                itemInstanceId,
                StringComparison.Ordinal));
        }

        public C1FormalItemPlacementSnapshot FindPlacementByInstanceId(
            string itemInstanceId)
        {
            return placementsValue.FirstOrDefault(value => string.Equals(
                value.itemInstanceId,
                itemInstanceId,
                StringComparison.Ordinal));
        }

        public C1FormalItemBattleInputSnapshot CreateBattleInputSnapshot()
        {
            C1FormalItemPlacementSnapshot source = FindPlacementByInstanceId(
                I031InventoryPlacementContract.SpecialIdentityId);
            C1FormalI031NianCapacityProjectionResult capacityProjection =
                C1FormalI031NianCapacityProjection.TryProject(
                    this,
                    C1FormalI031NianCapacityProjection.CreateCurrentRequest(this));
            List<C1FormalItemBattleItemRow> rows = new List<C1FormalItemBattleItemRow>();
            foreach (C1FormalItemRosterEntrySnapshot rosterRow in rosterValue.Where(
                         value => !value.isSpecialLightingSource))
            {
                C1FormalItemPlacementSnapshot placement =
                    FindPlacementByInstanceId(rosterRow.itemInstanceId);
                bool? isLit = null;
                bool? isDirectLit = null;
                if (placement != null)
                {
                    ItemSystemPlacementSnapshot itemSystemPlacement =
                        itemSystemSnapshot.FindPlacement(placement.placementId);
                    if (itemSystemPlacement != null)
                    {
                        isLit = itemSystemPlacement.isLit;
                        isDirectLit = itemSystemPlacement.isDirectLit;
                    }
                }

                rows.Add(new C1FormalItemBattleItemRow(
                    rosterRow,
                    placement,
                    placement != null,
                    isLit,
                    isDirectLit));
            }

            return new C1FormalItemBattleInputSnapshot(
                canonicalSignature,
                arrangementCanonicalSignature,
                itemSystemCanonicalSignature,
                source == null ? string.Empty : source.placementId,
                source == null ? (Vector2Int?)null : source.anchorCell,
                capacityProjection.isSuccess ? capacityProjection.fact : null,
                rows);
        }

        private string BuildArrangementPayload()
        {
            StringBuilder value = new StringBuilder();
            foreach (C1FormalItemRosterEntrySnapshot row in rosterValue)
            {
                value.Append("R|").Append(row.itemInstanceId).Append('|')
                    .Append(row.baseItemId).Append('|')
                    .Append(row.isSpecialLightingSource ? "special" : "ordinary").Append('|')
                    .Append(row.rarityKey).Append('|')
                    .Append(row.baselineProjectionCanonicalSignature).Append('|')
                    .Append(row.itemCatalogCanonicalSignature).Append('|')
                    .Append(row.instanceCanonicalSignature).Append('\n');
            }

            foreach (C1FormalItemPlacementSnapshot row in placementsValue)
            {
                value.Append("P|").Append(row.itemInstanceId).Append('|')
                    .Append(row.placementId).Append('|').Append(row.baseItemId).Append('|')
                    .Append(C1FormalItemCanonical.Cell(row.anchorCell)).Append('|')
                    .Append(row.rotation.ToString(CultureInfo.InvariantCulture)).Append('\n');
            }

            value.Append("T|").Append(trayLayout.canonicalSignature).Append('\n');

            return value.ToString();
        }

        private string BuildSessionPayload()
        {
            return string.Join("|", new[]
            {
                schemaId,
                productContext,
                chapterId,
                sessionToken,
                resetGeneration.ToString(CultureInfo.InvariantCulture),
                arrangementCanonicalSignature,
                itemSystemCanonicalSignature,
                entitlementCanonicalSignature
            });
        }
    }

    public sealed class C1FormalItemSessionOperationResult
    {
        private C1FormalItemSessionOperationResult(
            bool accepted,
            bool changed,
            string diagnosticCode,
            string diagnosticMessage,
            C1FormalItemSessionSnapshot snapshot)
        {
            this.accepted = accepted;
            this.changed = changed;
            this.diagnosticCode = diagnosticCode ?? string.Empty;
            this.diagnosticMessage = diagnosticMessage ?? string.Empty;
            this.snapshot = snapshot;
        }

        public bool accepted { get; }
        public bool changed { get; }
        public string diagnosticCode { get; }
        public string diagnosticMessage { get; }
        public C1FormalItemSessionSnapshot snapshot { get; }

        internal static C1FormalItemSessionOperationResult Accepted(
            C1FormalItemSessionSnapshot snapshot,
            bool changed,
            bool duplicate = false)
        {
            return new C1FormalItemSessionOperationResult(
                true,
                changed,
                duplicate
                    ? C1FormalItemSessionDiagnosticCodes.DuplicateAcceptedNoOp
                    : C1FormalItemSessionDiagnosticCodes.None,
                string.Empty,
                snapshot);
        }

        internal static C1FormalItemSessionOperationResult Rejected(
            C1FormalItemSessionSnapshot snapshot,
            string code,
            string message)
        {
            return new C1FormalItemSessionOperationResult(
                false,
                false,
                code,
                message,
                snapshot);
        }
    }

    public sealed class C1FormalItemSessionCreationResult
    {
        private C1FormalItemSessionCreationResult(
            C1FormalItemSessionAuthority authority,
            string diagnosticCode,
            string diagnosticMessage)
        {
            this.authority = authority;
            this.diagnosticCode = diagnosticCode ?? string.Empty;
            this.diagnosticMessage = diagnosticMessage ?? string.Empty;
        }

        public bool isSuccess => authority != null;
        public C1FormalItemSessionAuthority authority { get; }
        public C1FormalItemSessionSnapshot snapshot => authority?.Current;
        public string diagnosticCode { get; }
        public string diagnosticMessage { get; }

        internal static C1FormalItemSessionCreationResult Success(
            C1FormalItemSessionAuthority authority)
        {
            return new C1FormalItemSessionCreationResult(
                authority,
                C1FormalItemSessionDiagnosticCodes.None,
                string.Empty);
        }

        internal static C1FormalItemSessionCreationResult Failure(
            string code,
            string message)
        {
            return new C1FormalItemSessionCreationResult(null, code, message);
        }
    }

    public sealed class C1FormalItemSessionAuthority
    {
        private readonly Dictionary<string, string> commandSignatures =
            new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly Dictionary<string, string>
            campaignLootCommandReceiptSignatures =
                new Dictionary<string, string>(StringComparer.Ordinal);
        private C1FormalCampaignLootEntitlementHistorySnapshot
            campaignLootEntitlementHistory =
                C1FormalCampaignLootEntitlementHistorySnapshot.Empty();
        private readonly CanonicalItemDefinitionResolver canonicalItemResolver;
        private C1FormalItemSessionSnapshot current;

        private C1FormalItemSessionAuthority(
            C1FormalItemSessionSnapshot initial,
            CanonicalItemDefinitionResolver canonicalItemResolver)
        {
            current = initial ?? throw new ArgumentNullException(nameof(initial));
            this.canonicalItemResolver = canonicalItemResolver;
        }

        public C1FormalItemSessionSnapshot Current => current;
        public CanonicalItemDefinitionResolver CanonicalItemResolver => canonicalItemResolver;
        public C1FormalCampaignLootEntitlementHistorySnapshot
            CampaignLootEntitlementHistory => campaignLootEntitlementHistory;

        public static C1FormalItemSessionCreationResult Create(
            string sessionToken,
            long resetGeneration)
        {
            return Create(sessionToken, resetGeneration, null);
        }

        public static C1FormalItemSessionCreationResult Create(
            string sessionToken,
            long resetGeneration,
            CanonicalItemDefinitionResolver canonicalItemResolver)
        {
            string normalizedToken = C1FormalItemCanonical.Normalize(sessionToken);
            if (normalizedToken.Length == 0)
            {
                return C1FormalItemSessionCreationResult.Failure(
                    C1FormalItemSessionDiagnosticCodes.SessionTokenRequired,
                    "A non-empty session token is required.");
            }

            if (resetGeneration <= 0)
            {
                return C1FormalItemSessionCreationResult.Failure(
                    C1FormalItemSessionDiagnosticCodes.ResetGenerationInvalid,
                    "resetGeneration must be positive.");
            }

            C1FormalItemSessionSnapshot initial;
            string code;
            string message;
            if (!TryBuildInitialSnapshot(
                    normalizedToken,
                    resetGeneration,
                    out initial,
                    out code,
                    out message,
                    canonicalItemResolver))
            {
                return C1FormalItemSessionCreationResult.Failure(code, message);
            }

            return C1FormalItemSessionCreationResult.Success(
                new C1FormalItemSessionAuthority(
                    initial,
                    canonicalItemResolver));
        }

        public C1FormalItemSessionOperationResult Submit(
            C1FormalItemArrangementCommand command)
        {
            if (command == null)
            {
                return Reject(C1FormalItemSessionDiagnosticCodes.RequestNull,
                    "Arrangement command is required.");
            }

            C1FormalItemSessionOperationResult duplicate =
                CheckDuplicate(command.commandId, command.canonicalSignature);
            if (duplicate != null) return duplicate;
            C1FormalItemSessionOperationResult envelope = ValidateEnvelope(
                command.sessionToken,
                command.resetGeneration,
                command.commandId,
                command.expectedPriorSessionCanonicalSignature);
            if (envelope != null) return envelope;

            C1FormalItemRosterEntrySnapshot roster =
                current.FindRosterEntry(command.itemInstanceId);
            if (command.itemInstanceId.Length == 0)
            {
                return Reject(C1FormalItemSessionDiagnosticCodes.ItemInstanceRequired,
                    "itemInstanceId is required.");
            }

            if (roster == null)
                return Reject(
                    C1FormalItemSessionDiagnosticCodes.ItemInstanceUnknown,
                    "The Item instance is not available in this session.");

            C1FormalItemPlacementSnapshot existing =
                current.FindPlacementByInstanceId(command.itemInstanceId);
            C1FormalItemTrayPlacementSnapshot trayPlacement =
                current.trayLayout.FindPlacementByInstanceId(
                    command.itemInstanceId);
            if ((command.commandKind == C1FormalItemArrangementCommandKind.PlaceFromTray
                 || command.commandKind
                 == C1FormalItemArrangementCommandKind.ReplaceSameBaseFromTray
                 || command.commandKind == C1FormalItemArrangementCommandKind.MoveOnBoard
                 || command.commandKind == C1FormalItemArrangementCommandKind.MoveWithinTray)
                && command.placementCandidate == null)
            {
                return Reject(C1FormalItemSessionDiagnosticCodes.PlacementCandidateRequired,
                    "A placement candidate is required.");
            }

            if (command.commandKind == C1FormalItemArrangementCommandKind.ReturnToTray
                && command.placementCandidate != null)
            {
                return Reject(C1FormalItemSessionDiagnosticCodes.PlacementCandidateForbidden,
                    "ReturnToTray must not carry a placement candidate.");
            }

            if ((command.commandKind == C1FormalItemArrangementCommandKind.PlaceFromTray
                 || command.commandKind
                 == C1FormalItemArrangementCommandKind.ReplaceSameBaseFromTray)
                && existing != null)
            {
                return Reject(C1FormalItemSessionDiagnosticCodes.ItemAlreadyPlaced,
                    "A Tray-to-Board command requires an unplaced Item.");
            }

            if (trayPlacement == null)
            {
                return Reject(C1FormalItemSessionDiagnosticCodes.TrayPlacementMissing,
                    "The Item has no authoritative Tray layout row.");
            }

            if ((command.commandKind == C1FormalItemArrangementCommandKind.PlaceFromTray
                 || command.commandKind
                 == C1FormalItemArrangementCommandKind.ReplaceSameBaseFromTray)
                && !trayPlacement.isActiveInTray)
            {
                return Reject(C1FormalItemSessionDiagnosticCodes.TrayPlacementInactive,
                    "A Tray-to-Board command requires active Tray membership.");
            }

            if ((command.commandKind == C1FormalItemArrangementCommandKind.MoveOnBoard
                 || command.commandKind == C1FormalItemArrangementCommandKind.ReturnToTray)
                && existing == null)
            {
                return Reject(C1FormalItemSessionDiagnosticCodes.ItemNotPlaced,
                    "The command requires an existing board placement.");
            }


            if (command.commandKind == C1FormalItemArrangementCommandKind.MoveWithinTray
                && (existing != null || !trayPlacement.isActiveInTray))
            {
                return Reject(C1FormalItemSessionDiagnosticCodes.TrayPlacementInactive,
                    "MoveWithinTray requires one active unplaced Tray Item.");
            }

            if (command.commandKind != C1FormalItemArrangementCommandKind.PlaceFromTray
                && command.commandKind
                != C1FormalItemArrangementCommandKind.ReplaceSameBaseFromTray
                && command.commandKind != C1FormalItemArrangementCommandKind.MoveOnBoard
                && command.commandKind != C1FormalItemArrangementCommandKind.ReturnToTray
                && command.commandKind != C1FormalItemArrangementCommandKind.MoveWithinTray)
            {
                return Reject(C1FormalItemSessionDiagnosticCodes.CommandKindUnsupported,
                    "The arrangement command kind is unsupported.");
            }

            List<C1FormalItemPlacementSnapshot> placements =
                current.placements.ToList();
            List<C1FormalItemTrayPlacementSnapshot> trayPlacements =
                current.trayLayout.placements.ToList();
            string trayCode = C1FormalItemSessionDiagnosticCodes.None;
            string trayMessage = string.Empty;
            C1FormalItemPlacementSnapshot replacedSameBasePlacement =
                command.commandKind
                == C1FormalItemArrangementCommandKind.ReplaceSameBaseFromTray
                    ? current.placements.FirstOrDefault(value =>
                        !string.Equals(
                            value.itemInstanceId,
                            command.itemInstanceId,
                            StringComparison.Ordinal)
                        && string.Equals(
                            value.baseItemId,
                            roster.baseItemId,
                            StringComparison.Ordinal))
                    : null;
            C1FormalItemRosterEntrySnapshot replacedSameBaseRoster =
                replacedSameBasePlacement == null
                    ? null
                    : current.FindRosterEntry(
                        replacedSameBasePlacement.itemInstanceId);
            C1FormalItemTrayPlacementSnapshot replacedSameBaseTrayPlacement =
                replacedSameBasePlacement == null
                    ? null
                    : current.trayLayout.FindPlacementByInstanceId(
                        replacedSameBasePlacement.itemInstanceId);
            if (command.commandKind
                == C1FormalItemArrangementCommandKind.ReplaceSameBaseFromTray
                && (replacedSameBasePlacement == null
                    || replacedSameBaseRoster == null
                    || replacedSameBaseTrayPlacement == null))
            {
                return Reject(C1FormalItemSessionDiagnosticCodes.ItemNotPlaced,
                    "ReplaceSameBaseFromTray requires one deployed Item with the same base identity.");
            }

            if (command.commandKind != C1FormalItemArrangementCommandKind.MoveWithinTray)
            {
                placements.RemoveAll(value => string.Equals(
                    value.itemInstanceId,
                    command.itemInstanceId,
                    StringComparison.Ordinal));
            }
            if (replacedSameBasePlacement != null)
            {
                placements.RemoveAll(value => string.Equals(
                    value.itemInstanceId,
                    replacedSameBasePlacement.itemInstanceId,
                    StringComparison.Ordinal));
            }

            if (command.commandKind == C1FormalItemArrangementCommandKind.PlaceFromTray
                || command.commandKind
                == C1FormalItemArrangementCommandKind.ReplaceSameBaseFromTray
                || command.commandKind == C1FormalItemArrangementCommandKind.MoveOnBoard)
            {
                placements.Add(new C1FormalItemPlacementSnapshot(
                    roster.itemInstanceId,
                    PlacementId(roster),
                    roster.baseItemId,
                    command.placementCandidate.anchorCell,
                    command.placementCandidate.rotation));
                ReplaceTrayPlacement(
                    trayPlacements,
                    CloneTrayPlacement(trayPlacement, false));
                if (replacedSameBasePlacement != null)
                {
                    ReplaceTrayPlacement(
                        trayPlacements,
                        new C1FormalItemTrayPlacementSnapshot(
                            replacedSameBaseRoster.itemInstanceId,
                            trayPlacement.anchorCell,
                            trayPlacement.rotation,
                            trayPlacement.occupiedCells,
                            true));
                }
            }
            else if (command.commandKind == C1FormalItemArrangementCommandKind.ReturnToTray)
            {
                if (!TryResolveReturnTrayPlacement(
                        current,
                        roster,
                        trayPlacement,
                        out C1FormalItemTrayPlacementSnapshot returned,
                        out trayCode,
                        out trayMessage))
                    return Reject(trayCode, trayMessage);
                ReplaceTrayPlacement(trayPlacements, returned);
            }
            else if (command.commandKind == C1FormalItemArrangementCommandKind.MoveWithinTray)
            {
                if (!TryCreateTrayPlacement(
                        current,
                        roster,
                        command.placementCandidate,
                        true,
                        command.itemInstanceId,
                        out C1FormalItemTrayPlacementSnapshot moved,
                        out trayCode,
                        out trayMessage))
                    return Reject(trayCode, trayMessage);
                ReplaceTrayPlacement(trayPlacements, moved);
            }

            C1FormalItemSessionSnapshot candidate;
            string candidateCode;
            string candidateMessage;
            if (!TryBuildSnapshot(
                    current.sessionToken,
                    current.resetGeneration,
                    current.roster,
                    placements,
                    trayPlacements,
                    current.entitlementCanonicalSignature,
                    out candidate,
                    out candidateCode,
                    out candidateMessage,
                    canonicalItemResolver))
            {
                return Reject(candidateCode, candidateMessage);
            }

            if (!TryRecordCommand(command.commandId, command.canonicalSignature))
            {
                return Reject(
                    C1FormalItemSessionDiagnosticCodes.CommandHistoryCapacityReached,
                    "The bounded command history is full.");
            }

            bool changed = !string.Equals(
                current.canonicalSignature,
                candidate.canonicalSignature,
                StringComparison.Ordinal);
            if (changed) current = candidate;
            return C1FormalItemSessionOperationResult.Accepted(current, changed);
        }

        public C1FormalCampaignLootEntitlementAcceptanceResult
            AcceptCanonicalCampaignLootEntitlement(
                C1FormalCampaignLootEntitlementCommand command)
        {
            if (command == null)
            {
                return RejectCampaignLoot(
                    C1FormalItemSessionDiagnosticCodes.RequestNull,
                    "Campaign loot entitlement command is required.");
            }

            C1FormalCampaignLootEntitlementAcceptanceResult duplicate =
                CheckCampaignLootCommandDuplicate(
                    command.commandId,
                    command.canonicalSignature);
            if (duplicate != null) return duplicate;

            C1FormalItemSessionOperationResult envelope = ValidateEnvelope(
                command.sessionToken,
                command.resetGeneration,
                command.commandId,
                command.expectedPriorSessionCanonicalSignature);
            if (envelope != null)
            {
                return RejectCampaignLoot(
                    envelope.diagnosticCode,
                    envelope.diagnosticMessage);
            }

            if (!C1FormalCampaignLootEntitlementValidator
                .TryValidateReleasedGrant(
                    command,
                    canonicalItemResolver,
                    out C1FormalItemOrdinaryInstanceSnapshot instance,
                    out C1FormalCampaignLootAcceptedEntitlementReceipt receipt,
                    out string code,
                    out string message))
            {
                return RejectCampaignLoot(code, message);
            }

            C1FormalCampaignLootAcceptedEntitlementReceipt acceptedGrant =
                campaignLootEntitlementHistory.FindByGrantCanonicalSignature(
                    receipt.grantCanonicalSignature);
            if (acceptedGrant != null)
            {
                if (!TryRecordCommand(
                        command.commandId,
                        command.canonicalSignature))
                {
                    return RejectCampaignLoot(
                        C1FormalItemSessionDiagnosticCodes
                            .CommandHistoryCapacityReached,
                        "The bounded command history is full.");
                }

                campaignLootCommandReceiptSignatures.Add(
                    command.commandId,
                    acceptedGrant.canonicalSignature);
                return C1FormalCampaignLootEntitlementAcceptanceResult.Accepted(
                    current,
                    acceptedGrant,
                    campaignLootEntitlementHistory,
                    false,
                    true);
            }

            if (TryFindCampaignLootHistoryConflict(
                    receipt,
                    out code,
                    out message))
            {
                return RejectCampaignLoot(code, message);
            }

            if (current.FindRosterEntry(instance.itemInstanceId) != null)
            {
                return RejectCampaignLoot(
                    C1FormalCampaignLootEntitlementDiagnosticCodes
                        .ItemInstanceConflict,
                    "The Item instance identity is already roster-owned by different lineage.");
            }

            List<C1FormalItemRosterEntrySnapshot> roster =
                current.roster.ToList();
            C1FormalItemRosterEntrySnapshot addedRoster =
                ToRosterEntry(instance);
            roster.Add(addedRoster);
            if (!TryBuildSnapshot(
                    current.sessionToken,
                    current.resetGeneration,
                    roster,
                    current.placements,
                    null,
                    current.entitlementCanonicalSignature,
                    out C1FormalItemSessionSnapshot layoutProbe,
                    out code,
                    out message,
                    canonicalItemResolver))
            {
                return RejectCampaignLoot(code, message);
            }

            if (!TryGetShapeCells(
                    layoutProbe.itemSystemSnapshot,
                    addedRoster,
                    out IReadOnlyList<Vector2Int> addedShapeCells))
            {
                return RejectCampaignLoot(
                    C1FormalItemSessionDiagnosticCodes.TrayLayoutInvalid,
                    "Campaign loot Item shape facts are unavailable.");
            }

            HashSet<Vector2Int> blockedTrayCells = new HashSet<Vector2Int>(
                current.trayLayout.placements
                    .Where(value => value.isActiveInTray)
                    .SelectMany(value => value.occupiedCells));
            if (!TryFindFirstLegalTrayPlacement(
                    addedRoster.itemInstanceId,
                    addedShapeCells,
                    0,
                    blockedTrayCells,
                    true,
                    out C1FormalItemTrayPlacementSnapshot addedTrayPlacement))
            {
                return RejectCampaignLoot(
                    C1FormalItemSessionDiagnosticCodes
                        .TrayPlacementUnavailable,
                    "No deterministic legal Tray position is available.");
            }

            List<C1FormalItemTrayPlacementSnapshot> trayPlacements =
                current.trayLayout.placements.ToList();
            trayPlacements.Add(addedTrayPlacement);
            if (!TryBuildSnapshot(
                    current.sessionToken,
                    current.resetGeneration,
                    roster,
                    current.placements,
                    trayPlacements,
                    current.entitlementCanonicalSignature,
                    out C1FormalItemSessionSnapshot candidate,
                    out code,
                    out message,
                    canonicalItemResolver))
            {
                return RejectCampaignLoot(code, message);
            }

            C1FormalCampaignLootEntitlementHistorySnapshot candidateHistory =
                campaignLootEntitlementHistory.Append(receipt);
            if (!TryRecordCommand(
                    command.commandId,
                    command.canonicalSignature))
            {
                return RejectCampaignLoot(
                    C1FormalItemSessionDiagnosticCodes
                        .CommandHistoryCapacityReached,
                    "The bounded command history is full.");
            }

            campaignLootCommandReceiptSignatures.Add(
                command.commandId,
                receipt.canonicalSignature);
            current = candidate;
            campaignLootEntitlementHistory = candidateHistory;
            return C1FormalCampaignLootEntitlementAcceptanceResult.Accepted(
                current,
                receipt,
                campaignLootEntitlementHistory,
                true,
                false);
        }

        public C1FormalItemSessionOperationResult Reset(C1FormalItemResetCommand command)
        {
            if (command == null)
            {
                return Reject(C1FormalItemSessionDiagnosticCodes.RequestNull,
                    "Reset command is required.");
            }

            C1FormalItemSessionOperationResult duplicate =
                CheckDuplicate(command.commandId, command.canonicalSignature);
            if (duplicate != null) return duplicate;
            C1FormalItemSessionOperationResult envelope = ValidateEnvelope(
                command.sessionToken,
                command.resetGeneration,
                command.commandId,
                command.expectedPriorSessionCanonicalSignature);
            if (envelope != null) return envelope;
            if (current.resetGeneration == long.MaxValue)
            {
                return Reject(C1FormalItemSessionDiagnosticCodes.ResetGenerationInvalid,
                    "resetGeneration cannot advance beyond Int64.MaxValue.");
            }

            C1FormalItemSessionSnapshot candidate;
            string code;
            string message;
            if (!TryBuildInitialSnapshot(
                    current.sessionToken,
                    current.resetGeneration + 1,
                    out candidate,
                    out code,
                    out message,
                    canonicalItemResolver))
            {
                return Reject(code, message);
            }

            if (!TryRecordCommand(command.commandId, command.canonicalSignature))
            {
                return Reject(
                    C1FormalItemSessionDiagnosticCodes.CommandHistoryCapacityReached,
                    "The bounded command history is full.");
            }

            current = candidate;
            campaignLootEntitlementHistory =
                C1FormalCampaignLootEntitlementHistorySnapshot.Empty();
            campaignLootCommandReceiptSignatures.Clear();
            return C1FormalItemSessionOperationResult.Accepted(current, true);
        }

        public C1FormalItemBattleInputSnapshot CreateBattleInputSnapshot()
        {
            return current.CreateBattleInputSnapshot();
        }

        private C1FormalItemSessionOperationResult ValidateEnvelope(
            string sessionToken,
            long resetGeneration,
            string commandId,
            string expectedPrior)
        {
            if (sessionToken.Length == 0)
                return Reject(C1FormalItemSessionDiagnosticCodes.SessionTokenRequired,
                    "sessionToken is required.");
            if (!string.Equals(sessionToken, current.sessionToken, StringComparison.Ordinal))
                return Reject(C1FormalItemSessionDiagnosticCodes.SessionTokenMismatch,
                    "sessionToken does not own this authority.");
            if (resetGeneration != current.resetGeneration)
                return Reject(C1FormalItemSessionDiagnosticCodes.ResetGenerationStale,
                    "resetGeneration is stale or conflicting.");
            if (commandId.Length == 0)
                return Reject(C1FormalItemSessionDiagnosticCodes.CommandIdRequired,
                    "commandId is required.");
            if (expectedPrior.Length == 0)
                return Reject(
                    C1FormalItemSessionDiagnosticCodes.ExpectedSessionSignatureRequired,
                    "expectedPriorSessionCanonicalSignature is required.");
            if (!string.Equals(expectedPrior, current.canonicalSignature,
                    StringComparison.Ordinal))
                return Reject(
                    C1FormalItemSessionDiagnosticCodes.ExpectedSessionSignatureStale,
                    "The expected prior session signature is stale.");
            return null;
        }

        private C1FormalItemSessionOperationResult CheckDuplicate(
            string commandId,
            string requestSignature)
        {
            if (string.IsNullOrEmpty(commandId)
                || !commandSignatures.TryGetValue(commandId, out string acceptedSignature))
            {
                return null;
            }

            return string.Equals(acceptedSignature, requestSignature, StringComparison.Ordinal)
                ? C1FormalItemSessionOperationResult.Accepted(current, false, true)
                : Reject(C1FormalItemSessionDiagnosticCodes.CommandConflict,
                    "commandId was already used by a different immutable request.");
        }

        private C1FormalCampaignLootEntitlementAcceptanceResult
            CheckCampaignLootCommandDuplicate(
                string commandId,
                string requestSignature)
        {
            if (string.IsNullOrEmpty(commandId)
                || !commandSignatures.TryGetValue(
                    commandId,
                    out string acceptedSignature))
            {
                return null;
            }

            if (!string.Equals(
                    acceptedSignature,
                    requestSignature,
                    StringComparison.Ordinal))
            {
                return RejectCampaignLoot(
                    C1FormalItemSessionDiagnosticCodes.CommandConflict,
                    "commandId was already used by a different immutable request.");
            }

            C1FormalCampaignLootAcceptedEntitlementReceipt receipt = null;
            if (campaignLootCommandReceiptSignatures.TryGetValue(
                    commandId,
                    out string receiptSignature))
            {
                receipt = campaignLootEntitlementHistory
                    .FindByCanonicalSignature(receiptSignature);
            }

            return C1FormalCampaignLootEntitlementAcceptanceResult.Accepted(
                current,
                receipt,
                campaignLootEntitlementHistory,
                false,
                true);
        }

        private bool TryFindCampaignLootHistoryConflict(
            C1FormalCampaignLootAcceptedEntitlementReceipt receipt,
            out string code,
            out string message)
        {
            if (campaignLootEntitlementHistory.FindByGrantIdentity(
                    receipt.authoritativeRewardGrantIdentity) != null)
            {
                code = C1FormalCampaignLootEntitlementDiagnosticCodes
                    .GrantConflict;
                message = "Reward grant identity already exists with different lineage.";
                return true;
            }

            if (campaignLootEntitlementHistory.FindByEntitlementIdentity(
                    receipt.entitlementIdentity) != null
                || campaignLootEntitlementHistory
                    .FindByEntitlementCanonicalSignature(
                        receipt.entitlementCanonicalSignature) != null)
            {
                code = C1FormalCampaignLootEntitlementDiagnosticCodes
                    .EntitlementConflict;
                message = "Entitlement identity already exists with different lineage.";
                return true;
            }

            if (campaignLootEntitlementHistory.FindByItemInstanceId(
                    receipt.itemInstanceId) != null)
            {
                code = C1FormalCampaignLootEntitlementDiagnosticCodes
                    .ItemInstanceConflict;
                message = "Item instance identity already exists with different lineage.";
                return true;
            }

            if (campaignLootEntitlementHistory.FindByClaimId(receipt.claimId)
                    != null
                || campaignLootEntitlementHistory
                    .FindByClaimCanonicalSignature(
                        receipt.claimCanonicalSignature) != null)
            {
                code = C1FormalCampaignLootEntitlementDiagnosticCodes
                    .ClaimConflict;
                message = "Reward claim identity already exists with different lineage.";
                return true;
            }

            if (campaignLootEntitlementHistory.FindByDedupeKey(
                    receipt.dedupeKey) != null
                || campaignLootEntitlementHistory
                    .FindByDedupeCanonicalSignature(
                        receipt.dedupeCanonicalSignature) != null)
            {
                code = C1FormalCampaignLootEntitlementDiagnosticCodes
                    .DedupeConflict;
                message = "Reward dedupe identity already exists with different lineage.";
                return true;
            }

            if (campaignLootEntitlementHistory.FindByStageClearId(
                    receipt.stageClearId) != null)
            {
                code = C1FormalCampaignLootEntitlementDiagnosticCodes
                    .StageClearConflict;
                message = "StageClear identity already exists with different lineage.";
                return true;
            }

            code = C1FormalItemSessionDiagnosticCodes.None;
            message = string.Empty;
            return false;
        }

        private bool TryRecordCommand(string commandId, string requestSignature)
        {
            if (commandSignatures.Count >= C1FormalItemSessionContract.CommandHistoryCapacity)
            {
                return false;
            }

            commandSignatures.Add(commandId, requestSignature);
            return true;
        }

        private C1FormalItemSessionOperationResult Reject(string code, string message)
        {
            return C1FormalItemSessionOperationResult.Rejected(current, code, message);
        }

        private C1FormalCampaignLootEntitlementAcceptanceResult
            RejectCampaignLoot(string code, string message)
        {
            return C1FormalCampaignLootEntitlementAcceptanceResult.Rejected(
                current,
                campaignLootEntitlementHistory,
                code,
                message);
        }

        private static bool TryBuildInitialSnapshot(
            string sessionToken,
            long resetGeneration,
            out C1FormalItemSessionSnapshot snapshot,
            out string code,
            out string message,
            CanonicalItemDefinitionResolver canonicalItemResolver = null)
        {
            snapshot = null;
            C1FormalItemOrdinaryInstanceSnapshot initialItem;
            if (!TryCreateInitialCanonicalItem(
                    canonicalItemResolver,
                    out initialItem,
                    out code,
                    out message))
            {
                return false;
            }

            C1FormalItemRosterEntrySnapshot i031 = new C1FormalItemRosterEntrySnapshot(
                I031InventoryPlacementContract.SpecialIdentityId,
                I031InventoryPlacementContract.ItemId,
                true,
                string.Empty,
                string.Empty,
                CanonicalItemCatalogContract.CatalogId,
                "I031|SPECIAL_LIGHTING_SOURCE",
                null);
            C1FormalItemPlacementSnapshot[] placements =
            {
                new C1FormalItemPlacementSnapshot(
                    I031InventoryPlacementContract.SpecialIdentityId,
                    I031InventoryPlacementContract.StablePlacementId,
                    I031InventoryPlacementContract.ItemId,
                    new Vector2Int(1, 1),
                    0),
                new C1FormalItemPlacementSnapshot(
                    initialItem.itemInstanceId,
                    initialItem.itemInstanceId,
                    initialItem.baseItemId,
                    new Vector2Int(0, 1),
                    0)
            };
            return TryBuildSnapshot(
                sessionToken,
                resetGeneration,
                new[] { i031, ToRosterEntry(initialItem) },
                placements,
                null,
                string.Empty,
                out snapshot,
                out code,
                out message,
                canonicalItemResolver);
        }

        private static bool TryCreateInitialCanonicalItem(
            CanonicalItemDefinitionResolver canonicalItemResolver,
            out C1FormalItemOrdinaryInstanceSnapshot instance,
            out string code,
            out string message)
        {
            instance = null;
            if (!CanonicalInitialItemAcquisitionPolicy.TryCreate(
                    canonicalItemResolver,
                    out CanonicalItemDefinition definition,
                    out ItemGeneratedInstanceSnapshot generatedItem,
                    out string acquisitionError))
            {
                code = C1FormalItemSessionDiagnosticCodes.FixedBaselineUnavailable;
                message = "The canonical initial acquisition policy rejected the item: "
                          + acquisitionError;
                return false;
            }

            if (generatedItem.identity == null
                || !string.Equals(
                    generatedItem.baseItemId,
                    definition.baseItemId,
                    StringComparison.Ordinal))
            {
                code = C1FormalItemSessionDiagnosticCodes.FixedIdentityUnavailable;
                message = "The canonical initial item identity is invalid.";
                return false;
            }

            ItemInstanceIdentitySnapshot identity = generatedItem.identity;
            string identitySignature = identity.BuildCanonicalSignature();
            string instanceSignature = generatedItem.BuildCanonicalSignature();

            instance = new C1FormalItemOrdinaryInstanceSnapshot(
                identity,
                identitySignature,
                string.Empty,
                CanonicalItemCatalogContract.CatalogId,
                CanonicalInitialItemAcquisitionPolicy.PolicyId,
                instanceSignature,
                generatedItem,
                definition);
            code = C1FormalItemSessionDiagnosticCodes.None;
            message = string.Empty;
            return true;
        }

        private static bool TryBuildSnapshot(
            string sessionToken,
            long resetGeneration,
            IEnumerable<C1FormalItemRosterEntrySnapshot> rosterSource,
            IEnumerable<C1FormalItemPlacementSnapshot> placementSource,
            IEnumerable<C1FormalItemTrayPlacementSnapshot> trayPlacementSource,
            string entitlementCanonicalSignature,
            out C1FormalItemSessionSnapshot snapshot,
            out string code,
            out string message,
            CanonicalItemDefinitionResolver canonicalItemResolver = null)
        {
            snapshot = null;
            C1FormalItemRosterEntrySnapshot[] roster = (rosterSource
                ?? Enumerable.Empty<C1FormalItemRosterEntrySnapshot>())
                .Where(value => value != null)
                .OrderBy(value => value.itemInstanceId, StringComparer.Ordinal)
                .ToArray();
            C1FormalItemPlacementSnapshot[] placements = (placementSource
                ?? Enumerable.Empty<C1FormalItemPlacementSnapshot>())
                .Where(value => value != null)
                .OrderBy(value => value.itemInstanceId, StringComparer.Ordinal)
                .ToArray();
            Dictionary<string, C1FormalItemRosterEntrySnapshot> rosterByInstance = roster
                .GroupBy(value => value.itemInstanceId, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
            if (rosterByInstance.Count != roster.Length
                || placements.Select(value => value.itemInstanceId)
                    .Distinct(StringComparer.Ordinal).Count() != placements.Length
                || placements.Any(value => !rosterByInstance.ContainsKey(value.itemInstanceId)))
            {
                code = C1FormalItemSessionDiagnosticCodes.ItemSystemSnapshotMismatch;
                message = "Roster and placement instance identities must be unique and correlated.";
                return false;
            }

            ItemSystemPlacementInput[] itemSystemPlacements = placements
                .Select(value => new ItemSystemPlacementInput(
                    value.placementId,
                    value.baseItemId,
                    value.anchorCell,
                    value.rotation))
                .ToArray();
            bool i031Placed = placements.Any(value => string.Equals(
                value.itemInstanceId,
                I031InventoryPlacementContract.SpecialIdentityId,
                StringComparison.Ordinal));
            ItemSystemSnapshot itemSystem = DefaultItemSystemSnapshotProvider.Instance
                .CreateSnapshot(new ItemSystemSnapshotInput(
                    itemSystemPlacements,
                    ItemSystemBoardConfigInput.Default(),
                    null,
                    null,
                    canonicalItemResolver?.CreateItemSystemCatalogProjection(),
                    new[]
                    {
                        i031Placed
                            ? I031InventoryPlacementContract.OwnedBoard()
                            : I031InventoryPlacementContract.OwnedInventory()
                    }));
            if (itemSystem == null || !itemSystem.isValid)
            {
                code = C1FormalItemSessionDiagnosticCodes.ItemSystemSnapshotInvalid;
                message = itemSystem == null
                    ? "The ItemSystem snapshot is missing."
                    : string.Join(",", itemSystem.validationErrors.Select(value => value.code));
                return false;
            }

            bool exact = itemSystem.boardSize == C1FormalItemSessionContract.BoardSize
                         && itemSystem.placements.Count == placements.Length
                         && placements.All(value =>
                         {
                             ItemSystemPlacementSnapshot resolved =
                                 itemSystem.FindPlacement(value.placementId);
                             return resolved != null
                                    && string.Equals(resolved.itemId,
                                        value.baseItemId, StringComparison.Ordinal)
                                    && resolved.anchorCell == value.anchorCell
                                    && resolved.rotation == value.rotation;
                         })
                         && itemSystem.i031State != null
                         && itemSystem.i031State.isOwned
                         && itemSystem.i031State.isPlaced == i031Placed;
            if (!exact)
            {
                code = C1FormalItemSessionDiagnosticCodes.ItemSystemSnapshotMismatch;
                message = "ItemSystem did not preserve the exact accepted placement set.";
                return false;
            }

            if (!TryBuildTrayLayout(
                    roster,
                    placements,
                    itemSystem,
                    trayPlacementSource,
                    out C1FormalItemTrayLayoutSnapshot trayLayout,
                    out code,
                    out message))
                return false;

            snapshot = new C1FormalItemSessionSnapshot(
                sessionToken,
                resetGeneration,
                roster,
                placements,
                trayLayout,
                itemSystem,
                entitlementCanonicalSignature);
            code = C1FormalItemSessionDiagnosticCodes.None;
            message = string.Empty;
            return true;
        }

        private static bool TryBuildTrayLayout(
            IReadOnlyList<C1FormalItemRosterEntrySnapshot> roster,
            IReadOnlyList<C1FormalItemPlacementSnapshot> boardPlacements,
            ItemSystemSnapshot itemSystem,
            IEnumerable<C1FormalItemTrayPlacementSnapshot> trayPlacementSource,
            out C1FormalItemTrayLayoutSnapshot trayLayout,
            out string code,
            out string message)
        {
            trayLayout = null;
            C1FormalItemTrayPlacementSnapshot[] source = (trayPlacementSource
                ?? Enumerable.Empty<C1FormalItemTrayPlacementSnapshot>())
                .Where(value => value != null)
                .ToArray();
            Dictionary<string, C1FormalItemTrayPlacementSnapshot> byInstance =
                source.GroupBy(value => value.itemInstanceId, StringComparer.Ordinal)
                    .ToDictionary(group => group.Key, group => group.First(),
                        StringComparer.Ordinal);
            HashSet<string> rosterIds = new HashSet<string>(
                roster.Select(value => value.itemInstanceId),
                StringComparer.Ordinal);
            if (byInstance.Count != source.Length
                || source.Any(value => !rosterIds.Contains(value.itemInstanceId)))
            {
                code = C1FormalItemSessionDiagnosticCodes.TrayLayoutInvalid;
                message = "Tray layout identities must be unique and roster-owned.";
                return false;
            }

            HashSet<string> placedIds = new HashSet<string>(
                boardPlacements.Select(value => value.itemInstanceId),
                StringComparer.Ordinal);
            HashSet<Vector2Int> activeOccupied = new HashSet<Vector2Int>();
            List<C1FormalItemTrayPlacementSnapshot> built =
                new List<C1FormalItemTrayPlacementSnapshot>();
            foreach (C1FormalItemRosterEntrySnapshot row in roster
                         .OrderBy(value => value.itemInstanceId,
                             StringComparer.Ordinal))
            {
                bool shouldBeActive = !placedIds.Contains(row.itemInstanceId);
                if (!TryGetShapeCells(
                        itemSystem,
                        row,
                        out IReadOnlyList<Vector2Int> shapeCells))
                {
                    code = C1FormalItemSessionDiagnosticCodes.TrayLayoutInvalid;
                    message = "Tray layout shape facts are unavailable.";
                    return false;
                }

                if (byInstance.TryGetValue(
                        row.itemInstanceId,
                        out C1FormalItemTrayPlacementSnapshot existing))
                {
                    C1FormalItemPlacementCandidate candidate = new(
                        existing.anchorCell,
                        existing.rotation);
                    if (existing.isActiveInTray != shouldBeActive
                        || !C1FormalItemTrayLayoutRules.TryBuildOccupiedCells(
                            shapeCells,
                            candidate,
                            out IReadOnlyList<Vector2Int> occupied)
                        || !C1FormalItemTrayLayoutRules.IsInsideTray(occupied)
                        || !occupied.SequenceEqual(existing.occupiedCells)
                        || (shouldBeActive && occupied.Any(activeOccupied.Contains)))
                    {
                        code = C1FormalItemSessionDiagnosticCodes.TrayLayoutInvalid;
                        message = "Tray layout geometry, membership, or occupancy is invalid.";
                        return false;
                    }

                    C1FormalItemTrayPlacementSnapshot clone = new(
                        row.itemInstanceId,
                        existing.anchorCell,
                        existing.rotation,
                        occupied,
                        shouldBeActive);
                    built.Add(clone);
                    if (shouldBeActive) activeOccupied.UnionWith(occupied);
                    continue;
                }

                HashSet<Vector2Int> blocked = shouldBeActive
                    ? activeOccupied
                    : new HashSet<Vector2Int>();
                if (!TryFindFirstLegalTrayPlacement(
                        row.itemInstanceId,
                        shapeCells,
                        0,
                        blocked,
                        shouldBeActive,
                        out C1FormalItemTrayPlacementSnapshot created))
                {
                    code = C1FormalItemSessionDiagnosticCodes.TrayPlacementUnavailable;
                    message = "No deterministic legal Tray position is available.";
                    return false;
                }

                built.Add(created);
                if (shouldBeActive) activeOccupied.UnionWith(created.occupiedCells);
            }

            trayLayout = new C1FormalItemTrayLayoutSnapshot(built);
            code = C1FormalItemSessionDiagnosticCodes.None;
            message = string.Empty;
            return true;
        }

        private static bool TryResolveReturnTrayPlacement(
            C1FormalItemSessionSnapshot snapshot,
            C1FormalItemRosterEntrySnapshot roster,
            C1FormalItemTrayPlacementSnapshot remembered,
            out C1FormalItemTrayPlacementSnapshot returned,
            out string code,
            out string message)
        {
            C1FormalItemPlacementCandidate rememberedCandidate = new(
                remembered.anchorCell,
                remembered.rotation);
            if (TryCreateTrayPlacement(
                    snapshot,
                    roster,
                    rememberedCandidate,
                    true,
                    roster.itemInstanceId,
                    out returned,
                    out _,
                    out _))
            {
                code = C1FormalItemSessionDiagnosticCodes.None;
                message = string.Empty;
                return true;
            }

            if (!TryGetShapeCells(
                    snapshot.itemSystemSnapshot,
                    roster,
                    out IReadOnlyList<Vector2Int> shapeCells))
            {
                code = C1FormalItemSessionDiagnosticCodes.TrayLayoutInvalid;
                message = "The returned Item shape is unavailable.";
                returned = null;
                return false;
            }

            HashSet<Vector2Int> blocked = new HashSet<Vector2Int>(
                snapshot.trayLayout.placements
                    .Where(value => value.isActiveInTray
                                    && !string.Equals(
                                        value.itemInstanceId,
                                        roster.itemInstanceId,
                                        StringComparison.Ordinal))
                    .SelectMany(value => value.occupiedCells));
            if (TryFindFirstLegalTrayPlacement(
                    roster.itemInstanceId,
                    shapeCells,
                    remembered.rotation,
                    blocked,
                    true,
                    out returned))
            {
                code = C1FormalItemSessionDiagnosticCodes.None;
                message = string.Empty;
                return true;
            }

            code = C1FormalItemSessionDiagnosticCodes.TrayPlacementUnavailable;
            message = "No legal Tray position exists for the returned Item.";
            return false;
        }

        private static bool TryCreateTrayPlacement(
            C1FormalItemSessionSnapshot snapshot,
            C1FormalItemRosterEntrySnapshot roster,
            C1FormalItemPlacementCandidate candidate,
            bool active,
            string ignoredItemInstanceId,
            out C1FormalItemTrayPlacementSnapshot placement,
            out string code,
            out string message)
        {
            placement = null;
            if (snapshot == null
                || roster == null
                || candidate == null
                || !TryGetShapeCells(
                    snapshot.itemSystemSnapshot,
                    roster,
                    out IReadOnlyList<Vector2Int> shapeCells)
                || !C1FormalItemTrayLayoutRules.TryBuildOccupiedCells(
                    shapeCells,
                    candidate,
                    out IReadOnlyList<Vector2Int> occupied))
            {
                code = C1FormalItemSessionDiagnosticCodes.TrayLayoutInvalid;
                message = "The Tray candidate shape or rotation is invalid.";
                return false;
            }

            if (!C1FormalItemTrayLayoutRules.IsInsideTray(occupied))
            {
                code = C1FormalItemSessionDiagnosticCodes.TrayPlacementBounds;
                message = "The Tray candidate is outside the 5 x 13 grid.";
                return false;
            }

            HashSet<Vector2Int> blocked = new HashSet<Vector2Int>(
                snapshot.trayLayout.placements
                    .Where(value => value.isActiveInTray
                                    && !string.Equals(
                                        value.itemInstanceId,
                                        ignoredItemInstanceId,
                                        StringComparison.Ordinal))
                    .SelectMany(value => value.occupiedCells));
            if (occupied.Any(blocked.Contains))
            {
                code = C1FormalItemSessionDiagnosticCodes.TrayPlacementCollision;
                message = "The Tray candidate overlaps another Item.";
                return false;
            }

            placement = new C1FormalItemTrayPlacementSnapshot(
                roster.itemInstanceId,
                candidate.anchorCell,
                candidate.rotation,
                occupied,
                active);
            code = C1FormalItemSessionDiagnosticCodes.None;
            message = string.Empty;
            return true;
        }

        private static bool TryFindFirstLegalTrayPlacement(
            string itemInstanceId,
            IReadOnlyList<Vector2Int> shapeCells,
            int formalDegrees,
            ISet<Vector2Int> blocked,
            bool active,
            out C1FormalItemTrayPlacementSnapshot placement)
        {
            for (int index = 0;
                 index < C1FormalItemSessionContract.TrayCellCount;
                 index++)
            {
                Vector2Int anchor = new Vector2Int(
                    index % C1FormalItemSessionContract.TrayColumnCount,
                    index / C1FormalItemSessionContract.TrayColumnCount);
                C1FormalItemPlacementCandidate candidate = new(anchor, formalDegrees);
                if (!C1FormalItemTrayLayoutRules.TryBuildOccupiedCells(
                        shapeCells,
                        candidate,
                        out IReadOnlyList<Vector2Int> occupied)
                    || !C1FormalItemTrayLayoutRules.IsInsideTray(occupied)
                    || occupied.Any(value => blocked != null && blocked.Contains(value)))
                    continue;
                placement = new C1FormalItemTrayPlacementSnapshot(
                    itemInstanceId,
                    anchor,
                    formalDegrees,
                    occupied,
                    active);
                return true;
            }

            placement = null;
            return false;
        }

        private static bool TryGetShapeCells(
            ItemSystemSnapshot itemSystem,
            C1FormalItemRosterEntrySnapshot roster,
            out IReadOnlyList<Vector2Int> shapeCells)
        {
            shapeCells = Array.Empty<Vector2Int>();
            ItemSystemCatalogItemSnapshot catalog = itemSystem?.catalogItems
                .FirstOrDefault(value => roster != null
                    && string.Equals(
                        value.itemId,
                        roster.baseItemId,
                        StringComparison.Ordinal));
            if (catalog?.ShapeCells == null || catalog.ShapeCells.Count == 0)
                return false;
            shapeCells = catalog.ShapeCells;
            return true;
        }

        private static C1FormalItemTrayPlacementSnapshot CloneTrayPlacement(
            C1FormalItemTrayPlacementSnapshot source,
            bool active)
        {
            return new C1FormalItemTrayPlacementSnapshot(
                source.itemInstanceId,
                source.anchorCell,
                source.rotation,
                source.occupiedCells,
                active);
        }

        private static void ReplaceTrayPlacement(
            List<C1FormalItemTrayPlacementSnapshot> placements,
            C1FormalItemTrayPlacementSnapshot replacement)
        {
            placements.RemoveAll(value => string.Equals(
                value.itemInstanceId,
                replacement.itemInstanceId,
                StringComparison.Ordinal));
            placements.Add(replacement);
        }

        private static C1FormalItemRosterEntrySnapshot ToRosterEntry(
            C1FormalItemOrdinaryInstanceSnapshot instance)
        {
            return new C1FormalItemRosterEntrySnapshot(
                instance.itemInstanceId,
                instance.baseItemId,
                false,
                instance.rarityKey,
                instance.baselineProjectionCanonicalSignature,
                instance.itemCatalogCanonicalSignature,
                instance.instanceCanonicalSignature,
                instance);
        }

        private static string PlacementId(C1FormalItemRosterEntrySnapshot roster)
        {
            return roster.isSpecialLightingSource
                ? I031InventoryPlacementContract.StablePlacementId
                : roster.itemInstanceId;
        }

        private static string BuildOrdinaryInstancePayload(
            string itemInstanceId,
            string baseItemId,
            string rarityKey,
            string identityCanonicalSignature,
            string baselineProjectionCanonicalSignature,
            string itemCatalogCanonicalSignature,
            string sourceKind)
        {
            return string.Join("|", new[]
            {
                "schemaId=" + C1FormalItemSessionContract.OrdinaryInstanceSchemaId,
                "itemInstanceId=" + itemInstanceId,
                "baseItemId=" + baseItemId,
                "rarityKey=" + rarityKey,
                "identityCanonicalSignature=" + identityCanonicalSignature,
                "baselineProjectionCanonicalSignature="
                    + baselineProjectionCanonicalSignature,
                "itemCatalogCanonicalSignature=" + itemCatalogCanonicalSignature,
                "sourceKind=" + sourceKind
            });
        }
    }

    internal static class C1FormalItemCanonical
    {
        internal static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        internal static string Cell(Vector2Int value)
        {
            return "(" + value.x.ToString(CultureInfo.InvariantCulture) + ","
                   + value.y.ToString(CultureInfo.InvariantCulture) + ")";
        }

        internal static string Hash(string value)
        {
            byte[] bytes = new UTF8Encoding(false).GetBytes(value ?? string.Empty);
            byte[] hash;
            using (SHA256 algorithm = SHA256.Create())
            {
                hash = algorithm.ComputeHash(bytes);
            }

            StringBuilder result = new StringBuilder(hash.Length * 2);
            foreach (byte part in hash)
            {
                result.Append(part.ToString("x2", CultureInfo.InvariantCulture));
            }

            return result.ToString();
        }
    }
}
