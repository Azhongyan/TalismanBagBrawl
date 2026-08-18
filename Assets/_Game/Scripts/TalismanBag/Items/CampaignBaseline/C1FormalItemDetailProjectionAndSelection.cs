using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.Items.Canonical;
using TalismanBag.Items.Detail;
using TalismanBag.Items.CampaignLoot;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Rolling;
using UnityEngine;

namespace TalismanBag.Items.CampaignBaseline
{
    public enum C1FormalItemDetailIntent
    {
        Open = 1,
        Close = 2
    }

    public enum C1FormalItemDetailState
    {
        Closed = 0,
        Open = 1
    }

    public enum C1FormalItemArtworkLightingState
    {
        PlacementAbsent = 0,
        Unlit = 1,
        Lit = 2
    }

    public enum C1FormalItemCombatContributionState
    {
        NotApplicable = 0,
        Unplaced = 1,
        Unlit = 2,
        Contributing = 3
    }

    public interface IC1FormalItemArtworkResolver
    {
        bool TryResolveFormalItemArtwork(
            string authoritativeBaseItemId,
            C1FormalItemArtworkLightingState lightingState,
            out Sprite artwork,
            out string artworkIdentity);
    }

    public static class C1FormalItemArtworkIdentity
    {
        public const string SchemaId = "C1FormalItemArtworkIdentity.v1";

        public static string Create(
            string authoritativeBaseItemId,
            C1FormalItemArtworkLightingState lightingState)
        {
            return string.Join("|", new[]
            {
                SchemaId,
                authoritativeBaseItemId ?? string.Empty,
                lightingState.ToString()
            });
        }
    }

    public static class C1FormalItemDetailDiagnostics
    {
        public const string None = "NONE";
        public const string DuplicateAcceptedNoOp =
            "C1_FORMAL_ITEM_DETAIL_DUPLICATE_ACCEPTED_NO_OP";
        public const string RequestNull =
            "C1_FORMAL_ITEM_DETAIL_REQUEST_NULL";
        public const string IntentInvalid =
            "C1_FORMAL_ITEM_DETAIL_INTENT_INVALID";
        public const string ProductContextMismatch =
            "C1_FORMAL_ITEM_DETAIL_PRODUCT_CONTEXT_MISMATCH";
        public const string SessionSnapshotMissing =
            "C1_FORMAL_ITEM_DETAIL_SESSION_SNAPSHOT_MISSING";
        public const string SessionTokenMissing =
            "C1_FORMAL_ITEM_DETAIL_SESSION_TOKEN_MISSING";
        public const string SessionTokenMismatch =
            "C1_FORMAL_ITEM_DETAIL_SESSION_TOKEN_MISMATCH";
        public const string ResetGenerationInvalid =
            "C1_FORMAL_ITEM_DETAIL_RESET_GENERATION_INVALID";
        public const string ResetGenerationMismatch =
            "C1_FORMAL_ITEM_DETAIL_RESET_GENERATION_MISMATCH";
        public const string SessionSignatureMissing =
            "C1_FORMAL_ITEM_DETAIL_SESSION_SIGNATURE_MISSING";
        public const string SessionSignatureMismatch =
            "C1_FORMAL_ITEM_DETAIL_SESSION_SIGNATURE_MISMATCH";
        public const string ItemInstanceIdMissing =
            "C1_FORMAL_ITEM_DETAIL_ITEM_INSTANCE_ID_MISSING";
        public const string CloseItemInstanceIdMustBeEmpty =
            "C1_FORMAL_ITEM_DETAIL_CLOSE_ITEM_INSTANCE_ID_MUST_BE_EMPTY";
        public const string SelectedInstanceMismatch =
            "C1_FORMAL_ITEM_DETAIL_SELECTED_INSTANCE_MISMATCH";
        public const string ItemInstanceUnknown =
            "C1_FORMAL_ITEM_DETAIL_ITEM_INSTANCE_UNKNOWN";
        public const string RosterIdentityInvalid =
            "C1_FORMAL_ITEM_DETAIL_ROSTER_IDENTITY_INVALID";
        public const string PlacementIdentityMismatch =
            "C1_FORMAL_ITEM_DETAIL_PLACEMENT_IDENTITY_MISMATCH";
        public const string PlacementRotationInvalid =
            "C1_FORMAL_ITEM_DETAIL_PLACEMENT_ROTATION_INVALID";
        public const string ItemSystemPlacementMissing =
            "C1_FORMAL_ITEM_DETAIL_ITEMSYSTEM_PLACEMENT_MISSING";
        public const string ItemSystemPlacementMismatch =
            "C1_FORMAL_ITEM_DETAIL_ITEMSYSTEM_PLACEMENT_MISMATCH";
        public const string StaleItemSystemPlacement =
            "C1_FORMAL_ITEM_DETAIL_STALE_ITEMSYSTEM_PLACEMENT";
        public const string ArtworkResolverMissing =
            "C1_FORMAL_ITEM_DETAIL_ARTWORK_RESOLVER_MISSING";
        public const string ArtworkMissing =
            "C1_FORMAL_ITEM_DETAIL_ARTWORK_MISSING";
        public const string ArtworkIdentityMismatch =
            "C1_FORMAL_ITEM_DETAIL_ARTWORK_IDENTITY_MISMATCH";
        public const string CombatBaselineUnavailable =
            "C1_FORMAL_ITEM_DETAIL_COMBAT_BASELINE_UNAVAILABLE";
        public const string CombatBaselineInvalid =
            "C1_FORMAL_ITEM_DETAIL_COMBAT_BASELINE_INVALID";
        public const string CombatBaselineSignatureMismatch =
            "C1_FORMAL_ITEM_DETAIL_COMBAT_BASELINE_SIGNATURE_MISMATCH";
        public const string LifecycleUnbound =
            "C1_FORMAL_ITEM_DETAIL_LIFECYCLE_UNBOUND";
        public const string LifecycleRebound =
            "C1_FORMAL_ITEM_DETAIL_LIFECYCLE_REBOUND";
        public const string LifecycleReset =
            "C1_FORMAL_ITEM_DETAIL_LIFECYCLE_RESET";
        public const string SelectedInstanceRemoved =
            "C1_FORMAL_ITEM_DETAIL_SELECTED_INSTANCE_REMOVED";
    }

    public sealed class C1FormalItemDetailSelectionRequest
    {
        public const string SchemaId =
            "C1FormalItemDetailSelectionRequest.v1";

        public C1FormalItemDetailSelectionRequest(
            C1FormalItemDetailIntent intent,
            string productContext,
            string sessionToken,
            long resetGeneration,
            string expectedSessionCanonicalSignature,
            string itemInstanceId)
        {
            schemaId = SchemaId;
            this.intent = intent;
            this.productContext = productContext ?? string.Empty;
            this.sessionToken = sessionToken ?? string.Empty;
            this.resetGeneration = resetGeneration;
            this.expectedSessionCanonicalSignature =
                expectedSessionCanonicalSignature ?? string.Empty;
            this.itemInstanceId = itemInstanceId ?? string.Empty;
            canonicalSignature = C1FormalItemDetailCanonical.Hash(
                BuildCanonicalPayload());
        }

        public string schemaId { get; }
        public C1FormalItemDetailIntent intent { get; }
        public string productContext { get; }
        public string sessionToken { get; }
        public long resetGeneration { get; }
        public string expectedSessionCanonicalSignature { get; }
        public string itemInstanceId { get; }
        public string canonicalSignature { get; }

        public static C1FormalItemDetailSelectionRequest Open(
            C1FormalItemSessionSnapshot snapshot,
            string itemInstanceId)
        {
            return FromSnapshot(
                snapshot,
                C1FormalItemDetailIntent.Open,
                itemInstanceId);
        }

        public static C1FormalItemDetailSelectionRequest Close(
            C1FormalItemSessionSnapshot snapshot)
        {
            return FromSnapshot(
                snapshot,
                C1FormalItemDetailIntent.Close,
                string.Empty);
        }

        private static C1FormalItemDetailSelectionRequest FromSnapshot(
            C1FormalItemSessionSnapshot snapshot,
            C1FormalItemDetailIntent intent,
            string itemInstanceId)
        {
            return new C1FormalItemDetailSelectionRequest(
                intent,
                snapshot?.productContext,
                snapshot?.sessionToken,
                snapshot?.resetGeneration ?? 0L,
                snapshot?.canonicalSignature,
                itemInstanceId);
        }

        private string BuildCanonicalPayload()
        {
            StringBuilder builder = new StringBuilder();
            C1FormalItemDetailCanonical.Append(builder, "schema", schemaId);
            C1FormalItemDetailCanonical.Append(
                builder,
                "intent",
                ((int)intent).ToString(CultureInfo.InvariantCulture));
            C1FormalItemDetailCanonical.Append(
                builder,
                "productContext",
                productContext);
            C1FormalItemDetailCanonical.Append(
                builder,
                "sessionToken",
                sessionToken);
            C1FormalItemDetailCanonical.Append(
                builder,
                "resetGeneration",
                resetGeneration.ToString(CultureInfo.InvariantCulture));
            C1FormalItemDetailCanonical.Append(
                builder,
                "expectedSessionCanonicalSignature",
                expectedSessionCanonicalSignature);
            C1FormalItemDetailCanonical.Append(
                builder,
                "itemInstanceId",
                itemInstanceId);
            return builder.ToString();
        }
    }

    public sealed class C1FormalItemDetailPlacementFacts
    {
        private C1FormalItemDetailPlacementFacts(
            bool isPresent,
            string placementId,
            Vector2Int? anchorCell,
            int? formalRotationDegrees)
        {
            this.isPresent = isPresent;
            this.placementId = placementId ?? string.Empty;
            this.anchorCell = anchorCell;
            this.formalRotationDegrees = formalRotationDegrees;
            canonicalSignature = C1FormalItemDetailCanonical.Hash(
                BuildCanonicalPayload());
        }

        public bool isPresent { get; }
        public string placementId { get; }
        public Vector2Int? anchorCell { get; }
        public int? formalRotationDegrees { get; }
        public string canonicalSignature { get; }

        internal static C1FormalItemDetailPlacementFacts Absent()
        {
            return new C1FormalItemDetailPlacementFacts(
                false,
                string.Empty,
                null,
                null);
        }

        internal static C1FormalItemDetailPlacementFacts From(
            C1FormalItemPlacementSnapshot placement)
        {
            return new C1FormalItemDetailPlacementFacts(
                true,
                placement.placementId,
                placement.anchorCell,
                placement.rotation);
        }

        private string BuildCanonicalPayload()
        {
            StringBuilder builder = new StringBuilder();
            C1FormalItemDetailCanonical.Append(
                builder,
                "present",
                isPresent ? "true" : "false");
            C1FormalItemDetailCanonical.Append(
                builder,
                "placementId",
                placementId);
            C1FormalItemDetailCanonical.Append(
                builder,
                "anchor",
                anchorCell.HasValue
                    ? C1FormalItemDetailCanonical.Cell(anchorCell.Value)
                    : "absent");
            C1FormalItemDetailCanonical.Append(
                builder,
                "rotation",
                formalRotationDegrees.HasValue
                    ? formalRotationDegrees.Value.ToString(
                        CultureInfo.InvariantCulture)
                    : "absent");
            return builder.ToString();
        }
    }

    public sealed class C1FormalItemDetailLightingFacts
    {
        private readonly ReadOnlyCollection<Vector2Int> occupiedCellsValue;
        private readonly ReadOnlyCollection<Vector2Int>
            occupiedArrayBonusCellsValue;
        private readonly ReadOnlyCollection<string> unlockedCoreEffectIdsValue;
        private readonly ReadOnlyCollection<string> activeCoreEffectIdsValue;

        private C1FormalItemDetailLightingFacts(
            ItemSystemPlacementSnapshot placement)
        {
            isPresent = placement != null;
            placementId = placement?.placementId ?? string.Empty;
            baseItemId = placement?.itemId ?? string.Empty;
            occupiedCellsValue = FreezeCells(placement?.OccupiedCells);
            coreCellWorld = placement == null
                ? (Vector2Int?)null
                : placement.coreCellWorld;
            isLightingSource = placement?.isLightingSource;
            isDirectLit = placement?.isDirectLit;
            isLit = placement?.isLit;
            litByItemId = placement?.litByItemId ?? string.Empty;
            litByPlacementId = placement?.litByPlacementId ?? string.Empty;
            litDepth = placement == null ? (int?)null : placement.litDepth;
            occupiedArrayBonusCellsValue = FreezeCells(
                placement?.OccupiedArrayBonusCells);
            isOnArrayBonusCell = placement?.isOnArrayBonusCell;
            isArrayBonusActive = placement?.isArrayBonusActive;
            isCountedInBuild = placement?.isCountedInBuild;
            inputLevel = placement == null ? (int?)null : placement.inputLevel;
            resolvedLevel = placement == null
                ? (int?)null
                : placement.resolvedLevel;
            unlockedCoreEffectIdsValue = FreezeText(
                placement?.UnlockedCoreEffectIds);
            activeCoreEffectIdsValue = FreezeText(
                placement?.ActiveCoreEffectIds);
            canonicalSignature = C1FormalItemDetailCanonical.Hash(
                BuildCanonicalPayload());
        }

        public bool isPresent { get; }
        public string placementId { get; }
        public string baseItemId { get; }
        public IReadOnlyList<Vector2Int> occupiedCells => occupiedCellsValue;
        public Vector2Int? coreCellWorld { get; }
        public bool? isLightingSource { get; }
        public bool? isDirectLit { get; }
        public bool? isLit { get; }
        public string litByItemId { get; }
        public string litByPlacementId { get; }
        public int? litDepth { get; }
        public IReadOnlyList<Vector2Int> occupiedArrayBonusCells =>
            occupiedArrayBonusCellsValue;
        public bool? isOnArrayBonusCell { get; }
        public bool? isArrayBonusActive { get; }
        public bool? isCountedInBuild { get; }
        public int? inputLevel { get; }
        public int? resolvedLevel { get; }
        public IReadOnlyList<string> unlockedCoreEffectIds =>
            unlockedCoreEffectIdsValue;
        public IReadOnlyList<string> activeCoreEffectIds =>
            activeCoreEffectIdsValue;
        public string canonicalSignature { get; }

        internal static C1FormalItemDetailLightingFacts Absent()
        {
            return new C1FormalItemDetailLightingFacts(null);
        }

        internal static C1FormalItemDetailLightingFacts From(
            ItemSystemPlacementSnapshot placement)
        {
            return new C1FormalItemDetailLightingFacts(placement);
        }

        private string BuildCanonicalPayload()
        {
            StringBuilder builder = new StringBuilder();
            C1FormalItemDetailCanonical.Append(
                builder,
                "present",
                isPresent ? "true" : "false");
            C1FormalItemDetailCanonical.Append(builder, "placementId", placementId);
            C1FormalItemDetailCanonical.Append(builder, "baseItemId", baseItemId);
            C1FormalItemDetailCanonical.Append(
                builder,
                "occupiedCells",
                C1FormalItemDetailCanonical.Cells(occupiedCellsValue));
            C1FormalItemDetailCanonical.Append(
                builder,
                "coreCellWorld",
                coreCellWorld.HasValue
                    ? C1FormalItemDetailCanonical.Cell(coreCellWorld.Value)
                    : "absent");
            AppendNullableBool(builder, "isLightingSource", isLightingSource);
            AppendNullableBool(builder, "isDirectLit", isDirectLit);
            AppendNullableBool(builder, "isLit", isLit);
            C1FormalItemDetailCanonical.Append(builder, "litByItemId", litByItemId);
            C1FormalItemDetailCanonical.Append(
                builder,
                "litByPlacementId",
                litByPlacementId);
            AppendNullableInt(builder, "litDepth", litDepth);
            C1FormalItemDetailCanonical.Append(
                builder,
                "occupiedArrayBonusCells",
                C1FormalItemDetailCanonical.Cells(
                    occupiedArrayBonusCellsValue));
            AppendNullableBool(
                builder,
                "isOnArrayBonusCell",
                isOnArrayBonusCell);
            AppendNullableBool(
                builder,
                "isArrayBonusActive",
                isArrayBonusActive);
            AppendNullableBool(
                builder,
                "isCountedInBuild",
                isCountedInBuild);
            AppendNullableInt(builder, "inputLevel", inputLevel);
            AppendNullableInt(builder, "resolvedLevel", resolvedLevel);
            C1FormalItemDetailCanonical.Append(
                builder,
                "unlockedCoreEffectIds",
                string.Join(",", unlockedCoreEffectIdsValue));
            C1FormalItemDetailCanonical.Append(
                builder,
                "activeCoreEffectIds",
                string.Join(",", activeCoreEffectIdsValue));
            return builder.ToString();
        }

        private static void AppendNullableBool(
            StringBuilder builder,
            string key,
            bool? value)
        {
            C1FormalItemDetailCanonical.Append(
                builder,
                key,
                value.HasValue
                    ? (value.Value ? "true" : "false")
                    : "absent");
        }

        private static void AppendNullableInt(
            StringBuilder builder,
            string key,
            int? value)
        {
            C1FormalItemDetailCanonical.Append(
                builder,
                key,
                value.HasValue
                    ? value.Value.ToString(CultureInfo.InvariantCulture)
                    : "absent");
        }

        private static ReadOnlyCollection<Vector2Int> FreezeCells(
            IEnumerable<Vector2Int> source)
        {
            return Array.AsReadOnly((source ?? Enumerable.Empty<Vector2Int>())
                .OrderBy(value => value.y)
                .ThenBy(value => value.x)
                .ToArray());
        }

        private static ReadOnlyCollection<string> FreezeText(
            IEnumerable<string> source)
        {
            return Array.AsReadOnly((source ?? Enumerable.Empty<string>())
                .Select(value => value ?? string.Empty)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray());
        }
    }

    public sealed class C1FormalItemCombatDetailFacts
    {
        public const string SchemaId = "C1FormalItemCombatDetailFacts.v1";
        public const string CooldownUnitSeconds = "SECONDS";
        public const string CanonicalSourceKind = "CANONICAL_ITEM_INSTANCE";
        public const string StarterSourceKind = "C1_LV1_STARTER_RELEASE";
        public const string NotApplicableSourceKind = "NOT_APPLICABLE";

        private C1FormalItemCombatDetailFacts(
            string productContext,
            string baseItemId,
            string rarityKey,
            string rarityVersionKey,
            string profileRevision,
            int? directDamage,
            decimal? cooldownSeconds,
            string cooldownUnit,
            string lightingRequirement,
            bool isPlaced,
            bool? isLit,
            C1FormalItemCombatContributionState contributionState,
            string sourceProjectionCanonicalSignature,
            string combatSourceKind,
            string familyId,
            string familyVariantId,
            string effectCategoryId,
            string triggerId,
            string activationConditionId,
            string targetScopeId,
            string cueIdentity,
            string cadenceKind,
            long? cadenceMilliseconds,
            long? firstOffsetMilliseconds,
            string minimumLayoutTier,
            string factMaturity,
            string sourceSnapshotCanonicalSignature,
            string sourceProfileCanonicalSignature)
        {
            schemaId = SchemaId;
            this.productContext = productContext ?? string.Empty;
            this.baseItemId = baseItemId ?? string.Empty;
            this.rarityKey = rarityKey ?? string.Empty;
            this.rarityVersionKey = rarityVersionKey ?? string.Empty;
            this.profileRevision = profileRevision ?? string.Empty;
            this.directDamage = directDamage;
            this.cooldownSeconds = cooldownSeconds;
            this.cooldownUnit = cooldownUnit ?? string.Empty;
            this.lightingRequirement = lightingRequirement ?? string.Empty;
            this.isPlaced = isPlaced;
            this.isLit = isLit;
            this.contributionState = contributionState;
            this.sourceProjectionCanonicalSignature =
                sourceProjectionCanonicalSignature ?? string.Empty;
            this.combatSourceKind = combatSourceKind ?? string.Empty;
            this.familyId = familyId ?? string.Empty;
            this.familyVariantId = familyVariantId ?? string.Empty;
            this.effectCategoryId = effectCategoryId ?? string.Empty;
            this.triggerId = triggerId ?? string.Empty;
            this.activationConditionId = activationConditionId ?? string.Empty;
            this.targetScopeId = targetScopeId ?? string.Empty;
            this.cueIdentity = cueIdentity ?? string.Empty;
            this.cadenceKind = cadenceKind ?? string.Empty;
            this.cadenceMilliseconds = cadenceMilliseconds;
            this.firstOffsetMilliseconds = firstOffsetMilliseconds;
            this.minimumLayoutTier = minimumLayoutTier ?? string.Empty;
            this.factMaturity = factMaturity ?? string.Empty;
            this.sourceSnapshotCanonicalSignature =
                sourceSnapshotCanonicalSignature ?? string.Empty;
            this.sourceProfileCanonicalSignature =
                sourceProfileCanonicalSignature ?? string.Empty;
            canonicalSignature = C1FormalItemDetailCanonical.Hash(
                BuildCanonicalPayload());
        }

        public string schemaId { get; }
        public string productContext { get; }
        public string baseItemId { get; }
        public string rarityKey { get; }
        public string rarityVersionKey { get; }
        public string profileRevision { get; }
        public int? directDamage { get; }
        public decimal? cooldownSeconds { get; }
        public string cooldownUnit { get; }
        public string lightingRequirement { get; }
        public bool isPlaced { get; }
        public bool? isLit { get; }
        public C1FormalItemCombatContributionState contributionState { get; }
        public string sourceProjectionCanonicalSignature { get; }
        public string combatSourceKind { get; }
        public string familyId { get; }
        public string familyVariantId { get; }
        public string effectCategoryId { get; }
        public string triggerId { get; }
        public string activationConditionId { get; }
        public string targetScopeId { get; }
        public string cueIdentity { get; }
        public string cadenceKind { get; }
        public long? cadenceMilliseconds { get; }
        public long? firstOffsetMilliseconds { get; }
        public string minimumLayoutTier { get; }
        public string factMaturity { get; }
        public string sourceSnapshotCanonicalSignature { get; }
        public string sourceProfileCanonicalSignature { get; }
        public string canonicalSignature { get; }
        public bool isApplicable =>
            contributionState != C1FormalItemCombatContributionState.NotApplicable;

        internal static C1FormalItemCombatDetailFacts FromCanonical(
            string productContext,
            ItemGeneratedInstanceSnapshot generated,
            CanonicalItemDefinition definition,
            C1FormalItemRosterEntrySnapshot roster,
            bool isPlaced,
            bool? isLit)
        {
            C1FormalItemCombatContributionState state = !isPlaced
                ? C1FormalItemCombatContributionState.Unplaced
                : isLit == true
                    ? C1FormalItemCombatContributionState.Contributing
                    : C1FormalItemCombatContributionState.Unlit;
            ItemGeneratedStatSnapshot damage = generated.GeneratedStats
                .SingleOrDefault(value => string.Equals(
                    value.statId,
                    "damage",
                    StringComparison.Ordinal));
            ItemGeneratedStatSnapshot cooldown = generated.GeneratedStats
                .SingleOrDefault(value => string.Equals(
                    value.statId,
                    "cooldown",
                    StringComparison.Ordinal));
            int? directDamage = damage != null
                                && damage.rawUnits >= 0L
                                && damage.rawUnits <= int.MaxValue
                ? (int?)damage.rawUnits
                : null;
            long? cadenceMilliseconds = cooldown != null
                                             && cooldown.rawUnits > 0L
                                             && cooldown.rawUnits
                                             <= long.MaxValue / 400L
                ? (long?)(cooldown.rawUnits * 400L)
                : null;
            CanonicalItemEffectDefinition effect = definition.combatEffect;
            string generatedSignature = generated.BuildCanonicalSignature();
            return new C1FormalItemCombatDetailFacts(
                productContext,
                definition.baseItemId,
                roster.rarityKey,
                definition.baseItemId + "@" + roster.rarityKey,
                CanonicalItemCatalogContract.CatalogVersion,
                directDamage,
                cadenceMilliseconds.HasValue
                    ? cadenceMilliseconds.Value / 1000m
                    : (decimal?)null,
                cadenceMilliseconds.HasValue ? CooldownUnitSeconds : string.Empty,
                "PLACED_AND_LIT",
                isPlaced,
                isPlaced ? isLit : null,
                state,
                generatedSignature,
                CanonicalSourceKind,
                definition.faMenKey,
                definition.qiLeiKey,
                effect.effectCategory.ToString(),
                effect.triggerEventId,
                effect.conditionId,
                effect.targetSelector,
                effect.effectId,
                cadenceMilliseconds.HasValue ? "PERIODIC" : string.Empty,
                cadenceMilliseconds,
                cadenceMilliseconds,
                "PLACED_LIT",
                CanonicalItemCatalogContract.CatalogVersion,
                CanonicalItemCatalogContract.CatalogId,
                CanonicalItemCatalogContract.CatalogVersion);
        }

        internal static C1FormalItemCombatDetailFacts NotApplicable(
            string productContext,
            C1FormalItemRosterEntrySnapshot roster,
            bool isPlaced,
            bool? isLit)
        {
            return new C1FormalItemCombatDetailFacts(
                productContext,
                roster.baseItemId,
                roster.rarityKey,
                roster.baseItemId + "@" + roster.rarityKey,
                string.Empty,
                null,
                null,
                string.Empty,
                string.Empty,
                isPlaced,
                isPlaced ? isLit : null,
                C1FormalItemCombatContributionState.NotApplicable,
                string.Empty,
                NotApplicableSourceKind,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                null,
                null,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty);
        }

        private string BuildCanonicalPayload()
        {
            StringBuilder builder = new StringBuilder();
            C1FormalItemDetailCanonical.Append(builder, "schema", schemaId);
            C1FormalItemDetailCanonical.Append(
                builder,
                "productContext",
                productContext);
            C1FormalItemDetailCanonical.Append(builder, "baseItemId", baseItemId);
            C1FormalItemDetailCanonical.Append(builder, "rarityKey", rarityKey);
            C1FormalItemDetailCanonical.Append(
                builder,
                "rarityVersionKey",
                rarityVersionKey);
            C1FormalItemDetailCanonical.Append(
                builder,
                "profileRevision",
                profileRevision);
            C1FormalItemDetailCanonical.Append(
                builder,
                "directDamage",
                directDamage.HasValue
                    ? directDamage.Value.ToString(CultureInfo.InvariantCulture)
                    : "absent");
            C1FormalItemDetailCanonical.Append(
                builder,
                "cooldownSeconds",
                cooldownSeconds.HasValue
                    ? cooldownSeconds.Value.ToString(
                        "0.############################",
                        CultureInfo.InvariantCulture)
                    : "absent");
            C1FormalItemDetailCanonical.Append(builder, "cooldownUnit", cooldownUnit);
            C1FormalItemDetailCanonical.Append(
                builder,
                "lightingRequirement",
                lightingRequirement);
            C1FormalItemDetailCanonical.Append(
                builder,
                "isPlaced",
                isPlaced ? "true" : "false");
            C1FormalItemDetailCanonical.Append(
                builder,
                "isLit",
                isLit.HasValue ? (isLit.Value ? "true" : "false") : "absent");
            C1FormalItemDetailCanonical.Append(
                builder,
                "contributionState",
                ((int)contributionState).ToString(CultureInfo.InvariantCulture));
            C1FormalItemDetailCanonical.Append(
                builder,
                "sourceProjectionCanonicalSignature",
                sourceProjectionCanonicalSignature);
            C1FormalItemDetailCanonical.Append(
                builder,
                "combatSourceKind",
                combatSourceKind);
            C1FormalItemDetailCanonical.Append(builder, "familyId", familyId);
            C1FormalItemDetailCanonical.Append(
                builder,
                "familyVariantId",
                familyVariantId);
            C1FormalItemDetailCanonical.Append(
                builder,
                "effectCategoryId",
                effectCategoryId);
            C1FormalItemDetailCanonical.Append(builder, "triggerId", triggerId);
            C1FormalItemDetailCanonical.Append(
                builder,
                "activationConditionId",
                activationConditionId);
            C1FormalItemDetailCanonical.Append(
                builder,
                "targetScopeId",
                targetScopeId);
            C1FormalItemDetailCanonical.Append(builder, "cueIdentity", cueIdentity);
            C1FormalItemDetailCanonical.Append(
                builder,
                "cadenceKind",
                cadenceKind);
            C1FormalItemDetailCanonical.Append(
                builder,
                "cadenceMilliseconds",
                cadenceMilliseconds.HasValue
                    ? cadenceMilliseconds.Value.ToString(
                        CultureInfo.InvariantCulture)
                    : "absent");
            C1FormalItemDetailCanonical.Append(
                builder,
                "firstOffsetMilliseconds",
                firstOffsetMilliseconds.HasValue
                    ? firstOffsetMilliseconds.Value.ToString(
                        CultureInfo.InvariantCulture)
                    : "absent");
            C1FormalItemDetailCanonical.Append(
                builder,
                "minimumLayoutTier",
                minimumLayoutTier);
            C1FormalItemDetailCanonical.Append(
                builder,
                "factMaturity",
                factMaturity);
            C1FormalItemDetailCanonical.Append(
                builder,
                "sourceSnapshotCanonicalSignature",
                sourceSnapshotCanonicalSignature);
            C1FormalItemDetailCanonical.Append(
                builder,
                "sourceProfileCanonicalSignature",
                sourceProfileCanonicalSignature);
            return builder.ToString();
        }
    }

    public sealed class C1FormalItemDetailProjection
    {
        public const string SchemaId = "C1FormalItemDetailProjection.v1";

        private readonly ItemDetailViewModel detailModel;

        internal C1FormalItemDetailProjection(
            C1FormalItemSessionSnapshot session,
            C1FormalItemRosterEntrySnapshot roster,
            string identityCanonicalSignature,
            C1FormalItemDetailPlacementFacts placement,
            C1FormalItemDetailLightingFacts lighting,
            C1FormalItemCombatDetailFacts combat,
            ItemDetailViewModel configuredDetailModel,
            Sprite artwork,
            string artworkIdentity)
        {
            schemaId = SchemaId;
            productContext = session.productContext;
            sessionToken = session.sessionToken;
            resetGeneration = session.resetGeneration;
            sourceSessionCanonicalSignature = session.canonicalSignature;
            itemInstanceId = roster.itemInstanceId;
            baseItemId = roster.baseItemId;
            rarityKey = roster.rarityKey;
            this.identityCanonicalSignature =
                identityCanonicalSignature ?? string.Empty;
            baselineProjectionCanonicalSignature =
                roster.baselineProjectionCanonicalSignature;
            itemCatalogCanonicalSignature = roster.itemCatalogCanonicalSignature;
            instanceCanonicalSignature = roster.instanceCanonicalSignature;
            placementFacts = placement;
            lightingFacts = lighting;
            combatFacts = combat;
            detailModel = configuredDetailModel.Clone();
            this.artwork = artwork;
            this.artworkIdentity = artworkIdentity ?? string.Empty;
            canonicalSignature = C1FormalItemDetailCanonical.Hash(
                BuildCanonicalPayload());
        }

        public string schemaId { get; }
        public string productContext { get; }
        public string sessionToken { get; }
        public long resetGeneration { get; }
        public string sourceSessionCanonicalSignature { get; }
        public string itemInstanceId { get; }
        public string baseItemId { get; }
        public string rarityKey { get; }
        public string identityCanonicalSignature { get; }
        public string baselineProjectionCanonicalSignature { get; }
        public string itemCatalogCanonicalSignature { get; }
        public string instanceCanonicalSignature { get; }
        public C1FormalItemDetailPlacementFacts placementFacts { get; }
        public C1FormalItemDetailLightingFacts lightingFacts { get; }
        public C1FormalItemCombatDetailFacts combatFacts { get; }
        public Sprite artwork { get; }
        public string artworkIdentity { get; }
        public string canonicalSignature { get; }

        public ItemDetailViewModel CreateViewModelClone()
        {
            return detailModel.Clone();
        }

        private string BuildCanonicalPayload()
        {
            StringBuilder builder = new StringBuilder();
            C1FormalItemDetailCanonical.Append(builder, "schema", schemaId);
            C1FormalItemDetailCanonical.Append(
                builder,
                "productContext",
                productContext);
            C1FormalItemDetailCanonical.Append(builder, "sessionToken", sessionToken);
            C1FormalItemDetailCanonical.Append(
                builder,
                "resetGeneration",
                resetGeneration.ToString(CultureInfo.InvariantCulture));
            C1FormalItemDetailCanonical.Append(
                builder,
                "sourceSessionCanonicalSignature",
                sourceSessionCanonicalSignature);
            C1FormalItemDetailCanonical.Append(
                builder,
                "itemInstanceId",
                itemInstanceId);
            C1FormalItemDetailCanonical.Append(builder, "baseItemId", baseItemId);
            C1FormalItemDetailCanonical.Append(builder, "rarityKey", rarityKey);
            C1FormalItemDetailCanonical.Append(
                builder,
                "identityCanonicalSignature",
                identityCanonicalSignature);
            C1FormalItemDetailCanonical.Append(
                builder,
                "baselineProjectionCanonicalSignature",
                baselineProjectionCanonicalSignature);
            C1FormalItemDetailCanonical.Append(
                builder,
                "itemCatalogCanonicalSignature",
                itemCatalogCanonicalSignature);
            C1FormalItemDetailCanonical.Append(
                builder,
                "instanceCanonicalSignature",
                instanceCanonicalSignature);
            C1FormalItemDetailCanonical.Append(
                builder,
                "placementFacts",
                placementFacts.canonicalSignature);
            C1FormalItemDetailCanonical.Append(
                builder,
                "lightingFacts",
                lightingFacts.canonicalSignature);
            C1FormalItemDetailCanonical.Append(
                builder,
                "combatFacts",
                combatFacts.canonicalSignature);
            C1FormalItemDetailCanonical.Append(
                builder,
                "artworkIdentity",
                artworkIdentity);
            return builder.ToString();
        }
    }

    public sealed class C1FormalItemDetailSelectionResult
    {
        public const string SchemaId = "C1FormalItemDetailSelectionResult.v1";

        private C1FormalItemDetailSelectionResult(
            bool accepted,
            bool changed,
            string diagnostic,
            C1FormalItemDetailState state,
            C1FormalItemDetailProjection projection)
        {
            schemaId = SchemaId;
            this.accepted = accepted;
            this.changed = changed;
            this.diagnostic = diagnostic ?? string.Empty;
            this.state = state;
            this.projection = projection;
            canonicalSignature = C1FormalItemDetailCanonical.Hash(
                BuildCanonicalPayload());
        }

        public string schemaId { get; }
        public bool accepted { get; }
        public bool changed { get; }
        public string diagnostic { get; }
        public C1FormalItemDetailState state { get; }
        public bool isOpen => state == C1FormalItemDetailState.Open
                              && projection != null;
        public C1FormalItemDetailProjection projection { get; }
        public string canonicalSignature { get; }

        public static C1FormalItemDetailSelectionResult InitialClosed()
        {
            return new C1FormalItemDetailSelectionResult(
                true,
                false,
                C1FormalItemDetailDiagnostics.None,
                C1FormalItemDetailState.Closed,
                null);
        }

        internal static C1FormalItemDetailSelectionResult Opened(
            C1FormalItemDetailProjection projection,
            bool changed)
        {
            return new C1FormalItemDetailSelectionResult(
                true,
                changed,
                changed
                    ? C1FormalItemDetailDiagnostics.None
                    : C1FormalItemDetailDiagnostics.DuplicateAcceptedNoOp,
                C1FormalItemDetailState.Open,
                projection);
        }

        internal static C1FormalItemDetailSelectionResult Closed(bool changed)
        {
            return new C1FormalItemDetailSelectionResult(
                true,
                changed,
                changed
                    ? C1FormalItemDetailDiagnostics.None
                    : C1FormalItemDetailDiagnostics.DuplicateAcceptedNoOp,
                C1FormalItemDetailState.Closed,
                null);
        }

        internal static C1FormalItemDetailSelectionResult LifecycleClosed(
            string diagnostic)
        {
            return new C1FormalItemDetailSelectionResult(
                true,
                true,
                diagnostic,
                C1FormalItemDetailState.Closed,
                null);
        }

        internal static C1FormalItemDetailSelectionResult Rejected(
            string diagnostic,
            C1FormalItemDetailSelectionResult current)
        {
            C1FormalItemDetailSelectionResult safeCurrent = current
                ?? InitialClosed();
            return new C1FormalItemDetailSelectionResult(
                false,
                false,
                diagnostic,
                safeCurrent.state,
                safeCurrent.projection);
        }

        private string BuildCanonicalPayload()
        {
            StringBuilder builder = new StringBuilder();
            C1FormalItemDetailCanonical.Append(builder, "schema", schemaId);
            C1FormalItemDetailCanonical.Append(
                builder,
                "accepted",
                accepted ? "true" : "false");
            C1FormalItemDetailCanonical.Append(
                builder,
                "changed",
                changed ? "true" : "false");
            C1FormalItemDetailCanonical.Append(builder, "diagnostic", diagnostic);
            C1FormalItemDetailCanonical.Append(
                builder,
                "state",
                ((int)state).ToString(CultureInfo.InvariantCulture));
            C1FormalItemDetailCanonical.Append(
                builder,
                "projection",
                projection?.canonicalSignature ?? "absent");
            return builder.ToString();
        }
    }

    public static class C1FormalItemDetailProjectionAndSelection
    {
        public static C1FormalItemDetailSelectionResult Evaluate(
            C1FormalItemDetailSelectionRequest request,
            C1FormalItemSessionSnapshot snapshot,
            IC1FormalItemArtworkResolver artworkResolver,
            C1FormalItemDetailSelectionResult current)
        {
            C1FormalItemDetailSelectionResult safeCurrent = current
                ?? C1FormalItemDetailSelectionResult.InitialClosed();
            string lineageDiagnostic = ValidateLineage(request, snapshot);
            if (!string.Equals(
                    lineageDiagnostic,
                    C1FormalItemDetailDiagnostics.None,
                    StringComparison.Ordinal))
                return C1FormalItemDetailSelectionResult.Rejected(
                    lineageDiagnostic,
                    safeCurrent);

            if (request.intent == C1FormalItemDetailIntent.Close)
            {
                if (request.itemInstanceId.Length > 0)
                    return C1FormalItemDetailSelectionResult.Rejected(
                        C1FormalItemDetailDiagnostics
                            .CloseItemInstanceIdMustBeEmpty,
                        safeCurrent);
                return C1FormalItemDetailSelectionResult.Closed(
                    safeCurrent.isOpen);
            }

            if (request.intent != C1FormalItemDetailIntent.Open)
                return C1FormalItemDetailSelectionResult.Rejected(
                    C1FormalItemDetailDiagnostics.IntentInvalid,
                    safeCurrent);
            if (request.itemInstanceId.Length == 0)
                return C1FormalItemDetailSelectionResult.Rejected(
                    C1FormalItemDetailDiagnostics.ItemInstanceIdMissing,
                    safeCurrent);

            if (!TryCreateProjection(
                    request.itemInstanceId,
                    snapshot,
                    artworkResolver,
                    out C1FormalItemDetailProjection projection,
                    out string projectionDiagnostic))
                return C1FormalItemDetailSelectionResult.Rejected(
                    projectionDiagnostic,
                    safeCurrent);

            bool changed = !safeCurrent.isOpen
                           || !string.Equals(
                               safeCurrent.projection.canonicalSignature,
                               projection.canonicalSignature,
                               StringComparison.Ordinal);
            return C1FormalItemDetailSelectionResult.Opened(
                changed ? projection : safeCurrent.projection,
                changed);
        }

        internal static C1FormalItemDetailSelectionResult RejectAgainstCurrent(
            string diagnostic,
            C1FormalItemDetailSelectionResult current)
        {
            return C1FormalItemDetailSelectionResult.Rejected(
                diagnostic,
                current);
        }

        internal static C1FormalItemDetailSelectionResult CloseForLifecycle(
            string diagnostic,
            C1FormalItemDetailSelectionResult current)
        {
            return current != null && current.isOpen
                ? C1FormalItemDetailSelectionResult.LifecycleClosed(diagnostic)
                : C1FormalItemDetailSelectionResult.Closed(false);
        }

        private static string ValidateLineage(
            C1FormalItemDetailSelectionRequest request,
            C1FormalItemSessionSnapshot snapshot)
        {
            if (request == null)
                return C1FormalItemDetailDiagnostics.RequestNull;
            if (!Enum.IsDefined(typeof(C1FormalItemDetailIntent), request.intent))
                return C1FormalItemDetailDiagnostics.IntentInvalid;
            if (!string.Equals(
                    request.productContext,
                    C1FormalItemSessionContract.ProductContext,
                    StringComparison.Ordinal))
                return C1FormalItemDetailDiagnostics.ProductContextMismatch;
            if (request.sessionToken.Length == 0)
                return C1FormalItemDetailDiagnostics.SessionTokenMissing;
            if (request.resetGeneration <= 0L)
                return C1FormalItemDetailDiagnostics.ResetGenerationInvalid;
            if (request.expectedSessionCanonicalSignature.Length == 0)
                return C1FormalItemDetailDiagnostics.SessionSignatureMissing;
            if (snapshot == null)
                return C1FormalItemDetailDiagnostics.SessionSnapshotMissing;
            if (!string.Equals(
                    snapshot.productContext,
                    C1FormalItemSessionContract.ProductContext,
                    StringComparison.Ordinal))
                return C1FormalItemDetailDiagnostics.ProductContextMismatch;
            if (!string.Equals(
                    request.sessionToken,
                    snapshot.sessionToken,
                    StringComparison.Ordinal))
                return C1FormalItemDetailDiagnostics.SessionTokenMismatch;
            if (request.resetGeneration != snapshot.resetGeneration)
                return C1FormalItemDetailDiagnostics.ResetGenerationMismatch;
            if (!string.Equals(
                    request.expectedSessionCanonicalSignature,
                    snapshot.canonicalSignature,
                    StringComparison.Ordinal))
                return C1FormalItemDetailDiagnostics.SessionSignatureMismatch;
            return C1FormalItemDetailDiagnostics.None;
        }

        private static bool TryCreateProjection(
            string itemInstanceId,
            C1FormalItemSessionSnapshot snapshot,
            IC1FormalItemArtworkResolver artworkResolver,
            out C1FormalItemDetailProjection projection,
            out string diagnostic)
        {
            projection = null;
            C1FormalItemRosterEntrySnapshot roster = snapshot.FindRosterEntry(
                itemInstanceId);
            if (roster == null)
                return Fail(
                    C1FormalItemDetailDiagnostics.ItemInstanceUnknown,
                    out diagnostic);
            if (!ValidateRosterIdentity(
                    roster,
                    out string identityCanonicalSignature))
                return Fail(
                    C1FormalItemDetailDiagnostics.RosterIdentityInvalid,
                    out diagnostic);
            if (!TryCreateDetailBaseModel(
                    snapshot,
                    roster,
                    out ItemDetailViewModel canonicalModel,
                    out diagnostic))
                return false;

            C1FormalItemPlacementSnapshot placement = snapshot
                .FindPlacementByInstanceId(itemInstanceId);
            ItemSystemPlacementSnapshot resolvedPlacement = null;
            if (placement == null)
            {
                string expectedPlacementId = roster.isSpecialLightingSource
                    ? I031InventoryPlacementContract.StablePlacementId
                    : roster.itemInstanceId;
                bool stalePlacement = snapshot.itemSystemSnapshot.FindPlacement(
                    expectedPlacementId) != null;
                if (stalePlacement)
                    return Fail(
                        C1FormalItemDetailDiagnostics.StaleItemSystemPlacement,
                        out diagnostic);
            }
            else
            {
                if (!string.Equals(
                        placement.itemInstanceId,
                        roster.itemInstanceId,
                        StringComparison.Ordinal)
                    || !string.Equals(
                        placement.baseItemId,
                        roster.baseItemId,
                        StringComparison.Ordinal)
                    || placement.placementId.Length == 0)
                    return Fail(
                        C1FormalItemDetailDiagnostics.PlacementIdentityMismatch,
                        out diagnostic);
                if (!C1ExactBattleSandboxItemRotationAdapter
                        .TryFormalDegreesToVisualQuarterTurns(
                            placement.rotation,
                            out _))
                    return Fail(
                        C1FormalItemDetailDiagnostics.PlacementRotationInvalid,
                        out diagnostic);
                ItemSystemPlacementSnapshot[] matches = snapshot.itemSystemSnapshot
                    .placements.Where(value => string.Equals(
                        value.placementId,
                        placement.placementId,
                        StringComparison.Ordinal)).ToArray();
                if (matches.Length == 0)
                    return Fail(
                        C1FormalItemDetailDiagnostics.ItemSystemPlacementMissing,
                        out diagnostic);
                resolvedPlacement = matches[0];
                if (matches.Length != 1
                    || !string.Equals(
                        resolvedPlacement.itemId,
                        roster.baseItemId,
                        StringComparison.Ordinal)
                    || resolvedPlacement.anchorCell != placement.anchorCell
                    || resolvedPlacement.rotation != placement.rotation
                    || resolvedPlacement.isLightingSource
                    != roster.isSpecialLightingSource)
                    return Fail(
                        C1FormalItemDetailDiagnostics.ItemSystemPlacementMismatch,
                        out diagnostic);
            }

            C1FormalItemArtworkLightingState artworkState =
                resolvedPlacement == null
                    ? C1FormalItemArtworkLightingState.PlacementAbsent
                    : resolvedPlacement.isLit
                        ? C1FormalItemArtworkLightingState.Lit
                        : C1FormalItemArtworkLightingState.Unlit;
            if (artworkResolver == null)
                return Fail(
                    C1FormalItemDetailDiagnostics.ArtworkResolverMissing,
                    out diagnostic);
            if (!artworkResolver.TryResolveFormalItemArtwork(
                    roster.baseItemId,
                    artworkState,
                    out Sprite artwork,
                    out string artworkIdentity)
                || ReferenceEquals(artwork, null))
                return Fail(
                    C1FormalItemDetailDiagnostics.ArtworkMissing,
                    out diagnostic);
            string expectedArtworkIdentity = C1FormalItemArtworkIdentity.Create(
                roster.baseItemId,
                artworkState);
            if (!string.Equals(
                    expectedArtworkIdentity,
                    artworkIdentity,
                    StringComparison.Ordinal))
                return Fail(
                    C1FormalItemDetailDiagnostics.ArtworkIdentityMismatch,
                    out diagnostic);

            if (!TryCreateCombatFacts(
                    snapshot.productContext,
                    roster,
                    placement,
                    resolvedPlacement,
                    out C1FormalItemCombatDetailFacts combatFacts,
                    out diagnostic))
                return false;

            ItemDetailViewModel formalModel = CreateFormalViewModel(
                canonicalModel,
                roster,
                placement,
                resolvedPlacement,
                combatFacts);
            C1FormalItemDetailPlacementFacts placementFacts = placement == null
                ? C1FormalItemDetailPlacementFacts.Absent()
                : C1FormalItemDetailPlacementFacts.From(placement);
            C1FormalItemDetailLightingFacts lightingFacts =
                resolvedPlacement == null
                    ? C1FormalItemDetailLightingFacts.Absent()
                    : C1FormalItemDetailLightingFacts.From(resolvedPlacement);
            projection = new C1FormalItemDetailProjection(
                snapshot,
                roster,
                identityCanonicalSignature,
                placementFacts,
                lightingFacts,
                combatFacts,
                formalModel,
                artwork,
                artworkIdentity);
            diagnostic = C1FormalItemDetailDiagnostics.None;
            return true;
        }

        private static bool ValidateRosterIdentity(
            C1FormalItemRosterEntrySnapshot roster,
            out string identityCanonicalSignature)
        {
            identityCanonicalSignature = roster?.ordinaryInstance
                ?.identityCanonicalSignature
                ?? roster?.instanceCanonicalSignature
                ?? string.Empty;
            if (roster == null
                || roster.itemInstanceId.Length == 0
                || roster.baseItemId.Length == 0
                || roster.itemCatalogCanonicalSignature.Length == 0
                || roster.instanceCanonicalSignature.Length == 0
                || identityCanonicalSignature.Length == 0)
                return false;
            C1FormalItemOrdinaryInstanceSnapshot ordinary = roster
                .ordinaryInstance;
            return ordinary == null
                   || (string.Equals(
                           ordinary.itemInstanceId,
                           roster.itemInstanceId,
                           StringComparison.Ordinal)
                       && string.Equals(
                           ordinary.baseItemId,
                           roster.baseItemId,
                           StringComparison.Ordinal)
                       && string.Equals(
                           ordinary.rarityKey,
                           roster.rarityKey,
                           StringComparison.Ordinal)
                       && string.Equals(
                           ordinary.baselineProjectionCanonicalSignature,
                           roster.baselineProjectionCanonicalSignature,
                           StringComparison.Ordinal)
                       && string.Equals(
                           ordinary.itemCatalogCanonicalSignature,
                           roster.itemCatalogCanonicalSignature,
                           StringComparison.Ordinal)
                       && string.Equals(
                           ordinary.instanceCanonicalSignature,
                           roster.instanceCanonicalSignature,
                           StringComparison.Ordinal));
        }

        private static bool TryGetCanonicalDetailSource(
            C1FormalItemRosterEntrySnapshot roster,
            out ItemGeneratedInstanceSnapshot generated,
            out CanonicalItemDefinition definition,
            out string diagnostic)
        {
            generated = null;
            definition = null;
            C1FormalItemOrdinaryInstanceSnapshot ordinary =
                roster?.ordinaryInstance;
            if (ordinary == null)
                return Fail(
                    C1FormalItemDetailDiagnostics.CombatBaselineUnavailable,
                    out diagnostic);
            generated = ordinary.generatedInstance;
            definition = ordinary.canonicalDefinition;
            if (generated?.identity == null
                || definition == null
                || !definition.isOrdinaryDropEligible
                || !string.Equals(
                    roster.itemInstanceId,
                    generated.itemInstanceId,
                    StringComparison.Ordinal)
                || !string.Equals(
                    roster.baseItemId,
                    generated.baseItemId,
                    StringComparison.Ordinal)
                || !string.Equals(
                    roster.baseItemId,
                    definition.baseItemId,
                    StringComparison.Ordinal)
                || !string.Equals(
                    roster.rarityKey,
                    generated.rarity.ToStableKey(),
                    StringComparison.Ordinal))
                return Fail(
                    C1FormalItemDetailDiagnostics.CombatBaselineInvalid,
                    out diagnostic);

            diagnostic = C1FormalItemDetailDiagnostics.None;
            return true;
        }

        private static bool TryCreateDetailBaseModel(
            C1FormalItemSessionSnapshot snapshot,
            C1FormalItemRosterEntrySnapshot roster,
            out ItemDetailViewModel model,
            out string diagnostic)
        {
            if (roster?.isSpecialLightingSource == true)
            {
                return TryCreateI031BaseModel(
                    snapshot,
                    roster,
                    out model,
                    out diagnostic);
            }

            return TryCreateCanonicalBaseModel(
                roster,
                out model,
                out diagnostic);
        }

        private static bool TryCreateI031BaseModel(
            C1FormalItemSessionSnapshot snapshot,
            C1FormalItemRosterEntrySnapshot roster,
            out ItemDetailViewModel model,
            out string diagnostic)
        {
            model = null;
            C1FormalI031NianCapacityProjectionResult result =
                C1FormalI031NianCapacityProjection.TryProject(
                    snapshot,
                    C1FormalI031NianCapacityProjection.CreateCurrentRequest(
                        snapshot));
            C1FormalI031NianCapacityFact fact = result?.fact;
            if (result?.isSuccess != true
                || fact == null
                || !string.Equals(
                    roster.itemInstanceId,
                    fact.specialItemInstanceId,
                    StringComparison.Ordinal)
                || !string.Equals(
                    roster.baseItemId,
                    fact.baseItemId,
                    StringComparison.Ordinal))
            {
                diagnostic = result?.diagnosticCode
                             ?? C1FormalI031NianCapacityProjection
                                 .UnavailableDiagnosticCode;
                return false;
            }

            decimal intervalSeconds =
                fact.generationIntervalMilliseconds / 1000m;
            decimal generationPerSecond =
                fact.generationAmount * 1000m
                / fact.generationIntervalMilliseconds;
            string intervalText = intervalSeconds.ToString(
                "0.###",
                CultureInfo.InvariantCulture) + " 秒";
            string generationText = fact.generationAmount.ToString(
                CultureInfo.InvariantCulture) + " / " + intervalText;

            model = new ItemDetailViewModel
            {
                itemId = roster.baseItemId,
                itemInstanceId = roster.itemInstanceId,
                baseItemId = roster.baseItemId,
                rarityKey = "system",
                rarityDisplayName = "系统道具",
                displayItemName = "聚念石",
                displayRarityName = "系统道具",
                displayFaMenName = "阵眼",
                displayQiLeiName = "念力",
                displayShapeName = "single_1 · 1格",
                displayTriggerText = "放置在棋盘后，每 " + intervalText
                                     + " 产生 "
                                     + fact.generationAmount.ToString(
                                         CultureInfo.InvariantCulture)
                                     + " 点念力。",
                displayLightingStatusText = "聚念石是阵法光源。",
                displayFlavorText = "持续为已点亮道具提供发动所需的念力。",
                iconPlaceholderKey = "SPECIAL_I031",
                rarityColorKey = "system",
                displayPrimaryStats = new List<ItemDetailStatLine>
                {
                    new ItemDetailStatLine(
                        "初始念力",
                        fact.initialNian.ToString(CultureInfo.InvariantCulture),
                        "正式战斗开始时的念力"),
                    new ItemDetailStatLine(
                        "念力上限",
                        fact.maxNian.ToString(CultureInfo.InvariantCulture),
                        "超过上限的产出不会继续累加"),
                    new ItemDetailStatLine(
                        "念力产出",
                        generationText,
                        generationPerSecond.ToString(
                            "0.###",
                            CultureInfo.InvariantCulture) + " / 秒")
                },
                displayBasicEffects = new List<ItemDetailTextLine>
                {
                    new ItemDetailTextLine(
                        "供念",
                        "按固定间隔恢复念力；已点亮道具发动时消耗念力。",
                        "formal-i031-nian-source")
                },
                displayPlacementTips = new List<ItemDetailTextLine>
                {
                    new ItemDetailTextLine(
                        "摆放说明",
                        "聚念石固定作为阵眼和光源，不进入普通掉落池。",
                        "formal-i031-placement")
                }
            };
            diagnostic = C1FormalItemDetailDiagnostics.None;
            return true;
        }

        private static bool TryCreateCanonicalBaseModel(
            C1FormalItemRosterEntrySnapshot roster,
            out ItemDetailViewModel model,
            out string diagnostic)
        {
            model = null;
            if (!TryGetCanonicalDetailSource(
                    roster,
                    out ItemGeneratedInstanceSnapshot generated,
                    out CanonicalItemDefinition definition,
                    out diagnostic))
                return false;

            CanonicalItemPresentationDefinition presentation =
                definition.presentation;
            CanonicalItemRarityProfile rarityProfile =
                definition.GetRarityProfile(generated.rarity);
            model = new ItemDetailViewModel
            {
                itemId = definition.baseItemId,
                itemInstanceId = generated.itemInstanceId,
                baseItemId = definition.baseItemId,
                rarityKey = generated.rarity.ToStableKey(),
                rarityDisplayName = generated.rarity.ToStableKey(),
                displayItemName = definition.displayName,
                displayRarityName = generated.rarity.ToStableKey(),
                displayFaMenName = definition.faMenKey,
                displayQiLeiName = definition.qiLeiKey,
                displayShapeName = definition.shapeId + " · "
                                   + definition.ShapeCells.Count.ToString(
                                       CultureInfo.InvariantCulture) + "格",
                displayItemPower = rarityProfile == null
                    ? string.Empty
                    : rarityProfile.candidateItemPower.ToString(
                        CultureInfo.InvariantCulture),
                displayTriggerText = presentation?.triggerDescription
                                     ?? string.Empty,
                displayLightingStatusText = presentation?.lightingDescription
                                            ?? string.Empty,
                displayFlavorText = presentation?.flavorText ?? string.Empty,
                iconPlaceholderKey = presentation?.iconKey ?? string.Empty,
                rarityColorKey = generated.rarity.ToStableKey()
            };

            foreach (ItemGeneratedStatSnapshot stat in generated.GeneratedStats)
            {
                model.displayPrimaryStats.Add(new ItemDetailStatLine(
                    stat.statId,
                    stat.rawUnits.ToString(CultureInfo.InvariantCulture),
                    "Canonical ItemInstance"));
            }

            foreach (ItemGeneratedAffixSnapshot affix in generated.GeneratedAffixes)
            {
                ItemDetailTextLine line = new ItemDetailTextLine(
                    affix.affixId,
                    affix.rawUnits.ToString(CultureInfo.InvariantCulture),
                    affix.slotId);
                if (affix.slotKind
                    == TalismanBag.Items.Generation.Affixes
                        .ItemAffixSlotKind.Fixed)
                    model.displayFixedAffixes.Add(line);
                else
                    model.displayRandomAffixes.Add(line);
            }

            CanonicalItemEffectDefinition effect = definition.combatEffect;
            string basicEffectText = !string.IsNullOrWhiteSpace(
                presentation?.basicEffectDescription)
                ? presentation.basicEffectDescription
                : effect?.description ?? string.Empty;
            if (basicEffectText.Length > 0)
            {
                model.displayBasicEffects.Add(new ItemDetailTextLine(
                    effect?.displayName ?? string.Empty,
                    basicEffectText,
                    effect?.effectId ?? string.Empty));
            }

            foreach (CanonicalItemCoreDefinition core in definition.CoreDefinitions)
            {
                model.displayCoreEffects.Add(new ItemDetailTextLine(
                    core.displayName,
                    core.description,
                    core.coreEffectId));
            }

            ApplyCanonicalPlayerSemantics(model, generated, definition);

            string placementRecommendation =
                presentation?.placementRecommendation ?? string.Empty;
            if (placementRecommendation.Length > 0)
            {
                model.displayPlacementTips.Add(new ItemDetailTextLine(
                    "推荐摆放",
                    placementRecommendation,
                    "canonical-placement"));
            }

            diagnostic = C1FormalItemDetailDiagnostics.None;
            return true;
        }

        private static void ApplyCanonicalPlayerSemantics(
            ItemDetailViewModel model,
            ItemGeneratedInstanceSnapshot generated,
            CanonicalItemDefinition definition)
        {
            if (model == null || generated == null || definition == null)
            {
                return;
            }

            model.displayPrimaryStats = generated.GeneratedStats
                .Where(stat => stat != null)
                .Select(stat => new ItemDetailStatLine(
                    PlayerStatLabel(stat.statId),
                    PlayerStatValue(stat.statId, stat.rawUnits),
                    "本次获得道具的实际属性"))
                .ToList();

            CanonicalItemEffectDefinition effect = definition.combatEffect;
            CanonicalItemPresentationDefinition presentation =
                definition.presentation;
            if (effect == null)
            {
                return;
            }

            model.displayTriggerText = BuildPlayerTriggerText(
                effect,
                generated);
            string effectName = string.IsNullOrWhiteSpace(effect.displayName)
                ? "道具效果"
                : effect.displayName;
            string effectDescription = !string.IsNullOrWhiteSpace(
                effect.description)
                ? effect.description
                : presentation?.basicEffectDescription ?? string.Empty;
            string effectBody = string.IsNullOrWhiteSpace(effectDescription)
                ? "作用对象：" + PlayerTargetText(effect.targetSelector) + "。"
                : effectDescription.Trim() + "\n作用对象："
                  + PlayerTargetText(effect.targetSelector) + "。";
            model.displayBasicEffects = new List<ItemDetailTextLine>
            {
                new ItemDetailTextLine(
                    effectName,
                    effectBody,
                    "canonical-player-effect")
            };
        }

        private static string BuildPlayerTriggerText(
            CanonicalItemEffectDefinition effect,
            ItemGeneratedInstanceSnapshot generated)
        {
            string trigger = PlayerTriggerText(effect?.triggerEventId);
            string condition = PlayerConditionText(effect?.conditionId);
            long? cooldown = GeneratedStat(generated, "cooldown");
            long? nianCost = GeneratedStat(generated, "nianCost");
            List<string> parts = new List<string>
            {
                "放置到棋盘并点亮后",
                trigger,
                condition
            };
            if (cooldown.HasValue)
            {
                parts.Add("触发间隔 " + cooldown.Value.ToString(
                    CultureInfo.InvariantCulture) + " 秒");
            }
            if (nianCost.HasValue)
            {
                parts.Add("每次发动消耗 " + nianCost.Value.ToString(
                    CultureInfo.InvariantCulture) + " 点念力");
            }
            return string.Join("；", parts.Where(value =>
                !string.IsNullOrWhiteSpace(value))) + "。";
        }

        private static long? GeneratedStat(
            ItemGeneratedInstanceSnapshot generated,
            string statId)
        {
            ItemGeneratedStatSnapshot stat = generated?.GeneratedStats
                .FirstOrDefault(value => value != null
                    && string.Equals(
                        value.statId,
                        statId,
                        StringComparison.Ordinal));
            return stat?.rawUnits;
        }

        private static string PlayerStatLabel(string statId)
        {
            return statId switch
            {
                "damage" => "伤害",
                "break" => "破壳",
                "control" => "控制强度",
                "cooldown" => "触发间隔",
                "nianCost" => "念力消耗",
                "duration" => "持续时间",
                "guard" => "护势",
                "cleanse" => "净化层数",
                "heal" => "治疗",
                _ => "效果数值"
            };
        }

        private static string PlayerStatValue(string statId, long rawUnits)
        {
            string value = rawUnits.ToString(CultureInfo.InvariantCulture);
            return statId == "cooldown" || statId == "duration"
                ? value + " 秒"
                : value;
        }

        private static string PlayerTargetText(string targetSelector)
        {
            return targetSelector switch
            {
                "effect_primary_target" => "当前效果目标",
                "player" => "玩家",
                "all_enemies" => "全部敌人",
                "adjacent_targets" => "相邻目标",
                _ => "符合条件的目标"
            };
        }

        private static string PlayerTriggerText(string triggerEventId)
        {
            return triggerEventId switch
            {
                "on_action_commit" => "道具行动结算时发动",
                "on_ally_damaged" => "同法门目标受到伤害时发动",
                "on_burn_expire" => "燃烧自然结束时发动",
                "on_cleanse" => "净化发生时发动",
                "on_command_trigger" => "对应法门指令触发时发动",
                "on_consecutive_trigger" => "连续触发时发动",
                "on_damaged" => "玩家受到伤害后发动",
                "on_direct_lit" => "被直接点亮时发动",
                "on_effect_end" => "效果结束时发动",
                "on_first_hit_shielded" => "首次命中有护壳的目标时发动",
                "on_hit" => "命中目标时发动",
                "on_hit_burning" => "命中燃烧目标时发动",
                "on_hit_flaw" => "命中破绽目标时发动",
                "on_kill_flaw" => "击败破绽目标时发动",
                "on_layout_evaluate" => "布局条件成立时发动",
                "on_lit" => "被点亮时发动",
                "on_mirror_trigger" => "镜像条件触发时发动",
                "on_reveal_shield" => "揭示敌方护壳时发动",
                "on_slash" => "斩击发生时发动",
                "on_tick" => "持续效果每次跳动时发动",
                "on_turn_start" => "回合开始时发动",
                "on_zhenlei_trigger" => "震雷效果触发时发动",
                _ => "满足该道具的触发事件时发动"
            };
        }

        private static string PlayerConditionText(string conditionId)
        {
            return conditionId switch
            {
                "is_lit" => "需要保持点亮",
                "same_famen" => "目标需属于同一法门",
                "duration_natural_end" => "效果必须自然结束",
                "cleanse_success" => "需要实际净化成功",
                "same_target_chain" => "需要连续净化同一目标",
                "water_charge_3" => "需要累积三次水行充能",
                "next_taibai_hit" => "作用于下一次太白命中",
                "within_2_turns" => "需要在两回合内连续触发",
                "guard_below_threshold" => "受击后护势需低于阈值",
                "is_direct_lit" => "必须由光源直接点亮",
                "next_zhenlei_trigger" => "作用于下一次震雷触发",
                "adjacent_lit_item" => "需要相邻的已点亮道具",
                "mirror_mark_active" => "需要镜像标记生效",
                "target_has_shield" => "目标必须有护壳",
                "target_casting" => "目标必须正在施法",
                "burn_stack_gt_0" => "目标必须带有余焰",
                "target_low_health" => "目标必须处于低生命",
                "next_slash" => "作用于下一次斩击",
                "horizontal_adjacent" => "横向必须有相邻道具",
                "adjacent_lihuo" => "相邻位置必须有离火道具",
                "next_incoming_hit" => "作用于下一次受到的攻击",
                "target_has_burn" => "目标必须带有燃烧",
                "target_has_debuff" => "目标必须带有负面状态",
                "next_hit" => "作用于下一次命中",
                "target_has_flaw" => "目标必须带有破绽",
                "burn_stack" => "效果随目标余焰层数变化",
                "lit_on_array_cell" => "必须在阵法格上被点亮",
                "trigger_count_3" => "每第三次触发时生效",
                _ => "还需满足道具说明中的条件"
            };
        }

        private static ItemDetailViewModel CreateFormalViewModel(
            ItemDetailViewModel catalogModel,
            C1FormalItemRosterEntrySnapshot roster,
            C1FormalItemPlacementSnapshot placement,
            ItemSystemPlacementSnapshot resolvedPlacement,
            C1FormalItemCombatDetailFacts combatFacts)
        {
            ItemDetailViewModel model = catalogModel.Clone();
            model.itemId = roster.baseItemId;
            model.itemInstanceId = roster.itemInstanceId;
            model.baseItemId = roster.baseItemId;
            model.placementId = placement?.placementId ?? string.Empty;
            model.rarityKey = roster.rarityKey;
            model.rarityDisplayName = roster.rarityKey;
            model.displayRarityName = roster.rarityKey;
            model.statusFlags = model.statusFlags ?? new ItemDetailStatusFlags();
            model.lightingPreview = model.lightingPreview
                                    ?? new ItemLightingPreview();
            model.buildPreview = model.buildPreview ?? new ItemBuildPreview();
            model.statusFlags.placementId = model.placementId;
            model.lightingPreview.placementId = model.placementId;

            if (resolvedPlacement == null)
            {
                ApplyCanonicalBaseSections(model);
                ApplyCombatPresentation(model, combatFacts);
                return model;
            }

            bool isRelayLit = resolvedPlacement.isLit
                              && !resolvedPlacement.isLightingSource
                              && !resolvedPlacement.isDirectLit;
            model.statusFlags.isLit = resolvedPlacement.isLit;
            model.statusFlags.isLightingSource =
                resolvedPlacement.isLightingSource;
            model.statusFlags.isDirectLit = resolvedPlacement.isDirectLit;
            model.statusFlags.isRelayLit = isRelayLit;
            model.statusFlags.isOnArrayBonusCell =
                resolvedPlacement.isOnArrayBonusCell;
            model.statusFlags.isArrayBonusActive =
                resolvedPlacement.isArrayBonusActive;
            model.statusFlags.countedInBuild =
                resolvedPlacement.isCountedInBuild;
            model.statusFlags.litByItemId = resolvedPlacement.litByItemId;
            model.statusFlags.litDepth = resolvedPlacement.litDepth;
            model.statusFlags.inputLevel = resolvedPlacement.inputLevel;
            model.statusFlags.resolvedLevel = resolvedPlacement.resolvedLevel;
            model.lightingPreview.isLit = resolvedPlacement.isLit;
            model.lightingPreview.isLightingSource =
                resolvedPlacement.isLightingSource;
            model.lightingPreview.isDirectLit = resolvedPlacement.isDirectLit;
            model.lightingPreview.isRelayLit = isRelayLit;
            model.lightingPreview.isOnArrayBonusCell =
                resolvedPlacement.isOnArrayBonusCell;
            model.lightingPreview.isArrayBonusActive =
                resolvedPlacement.isArrayBonusActive;
            model.lightingPreview.litByItemId = resolvedPlacement.litByItemId;
            model.lightingPreview.litDepth = resolvedPlacement.litDepth;
            model.buildPreview.countedInBuild =
                resolvedPlacement.isCountedInBuild;
            ApplyCanonicalBaseSections(model);
            ApplyCombatPresentation(model, combatFacts);
            return model;
        }

        private static bool TryCreateCombatFacts(
            string productContext,
            C1FormalItemRosterEntrySnapshot roster,
            C1FormalItemPlacementSnapshot placement,
            ItemSystemPlacementSnapshot resolvedPlacement,
            out C1FormalItemCombatDetailFacts combatFacts,
            out string diagnostic)
        {
            combatFacts = null;
            bool isPlaced = placement != null;
            bool? isLit = resolvedPlacement == null
                ? (bool?)null
                : resolvedPlacement.isLit;
            if (roster.isSpecialLightingSource)
            {
                combatFacts = C1FormalItemCombatDetailFacts.NotApplicable(
                    productContext,
                    roster,
                    isPlaced,
                    isLit);
                diagnostic = C1FormalItemDetailDiagnostics.None;
                return true;
            }

            if (!TryGetCanonicalDetailSource(
                    roster,
                    out ItemGeneratedInstanceSnapshot generated,
                    out CanonicalItemDefinition definition,
                    out diagnostic))
                return false;
            if (generated.GeneratedStats.Count(value => string.Equals(
                    value.statId,
                    "damage",
                    StringComparison.Ordinal)) > 1
                || generated.GeneratedStats.Count(value => string.Equals(
                    value.statId,
                    "cooldown",
                    StringComparison.Ordinal)) != 1)
                return Fail(
                    C1FormalItemDetailDiagnostics.CombatBaselineInvalid,
                    out diagnostic);

            combatFacts = C1FormalItemCombatDetailFacts.FromCanonical(
                productContext,
                generated,
                definition,
                roster,
                isPlaced,
                isLit);
            diagnostic = C1FormalItemDetailDiagnostics.None;
            return true;
        }

        private static void ApplyCombatPresentation(
            ItemDetailViewModel model,
            C1FormalItemCombatDetailFacts combatFacts)
        {
            if (model == null || combatFacts == null || !combatFacts.isApplicable)
                return;
            if (!string.Equals(
                    combatFacts.combatSourceKind,
                    C1FormalItemCombatDetailFacts.CanonicalSourceKind,
                    StringComparison.Ordinal))
                return;

            ApplyCanonicalCombatPresentation(model, combatFacts);
        }

        private static void ApplyCanonicalBaseSections(
            ItemDetailViewModel model)
        {
            if (model == null) return;

            string[] statLines = (model.displayPrimaryStats
                                  ?? new List<ItemDetailStatLine>())
                .Where(value => value != null)
                .Select(value => value.label + "：" + value.value)
                .ToArray();
            string[] triggerLines = string.IsNullOrWhiteSpace(
                model.displayTriggerText)
                ? Array.Empty<string>()
                : new[] { model.displayTriggerText };
            string[] basicLines = (model.displayBasicEffects
                                   ?? new List<ItemDetailTextLine>())
                .Where(value => value != null
                                && (!string.IsNullOrWhiteSpace(value.title)
                                    || !string.IsNullOrWhiteSpace(value.body)))
                .Select(value => string.IsNullOrWhiteSpace(value.title)
                    ? value.body
                    : value.title + "：" + value.body)
                .ToArray();

            model.displayPlayerSections = UpsertOwnedSection(
                model.displayPlayerSections,
                "stats",
                "基础属性",
                statLines,
                null,
                true);
            model.displayPlayerSections = UpsertOwnedSection(
                model.displayPlayerSections,
                "trigger",
                "触发条件",
                triggerLines,
                null,
                true);
            model.displayPlayerSections = UpsertOwnedSection(
                model.displayPlayerSections,
                "basic",
                "基础效果",
                basicLines,
                null,
                true);
        }

        private static void ApplyCanonicalCombatPresentation(
            ItemDetailViewModel model,
            C1FormalItemCombatDetailFacts combatFacts)
        {
            string contribution = ContributionText(
                combatFacts.contributionState);
            model.displayBasicEffects = ReplaceOwnedBasicEffects(
                model.displayBasicEffects,
                new ItemDetailTextLine(
                    "当前战斗状态",
                    contribution,
                    "formal-combat-contribution"));
            model.battleEffectPreview = model.battleEffectPreview
                                        ?? new ItemBattleEffectPreview();
            model.battleEffectPreview.basicEffectActive =
                combatFacts.contributionState
                == C1FormalItemCombatContributionState.Contributing;
            model.battleEffectPreview.battleStateText = contribution;
            string[] basicLines = (model.displayBasicEffects
                                   ?? new List<ItemDetailTextLine>())
                .Where(value => value != null
                                && (!string.IsNullOrWhiteSpace(value.title)
                                    || !string.IsNullOrWhiteSpace(value.body)))
                .Select(value => string.IsNullOrWhiteSpace(value.title)
                    ? value.body
                    : value.title + "：" + value.body)
                .ToArray();
            model.displayPlayerSections = UpsertOwnedSection(
                model.displayPlayerSections,
                "basic",
                "基础效果",
                basicLines,
                null,
                true);
        }

        private static List<ItemDetailTextLine> ReplaceOwnedBasicEffects(
            IEnumerable<ItemDetailTextLine> source,
            params ItemDetailTextLine[] owned)
        {
            HashSet<string> stateKeys = new HashSet<string>(
                owned.Select(value => value.stateKey),
                StringComparer.Ordinal);
            List<ItemDetailTextLine> rows = (source
                    ?? Enumerable.Empty<ItemDetailTextLine>())
                .Where(value => value != null
                                && !stateKeys.Contains(value.stateKey))
                .ToList();
            rows.AddRange(owned);
            return rows;
        }

        private static List<ItemDetailSectionViewModel> UpsertOwnedSection(
            IEnumerable<ItemDetailSectionViewModel> source,
            string stateKey,
            string fallbackTitle,
            IEnumerable<string> ownedLines,
            IEnumerable<string> ownedPrefixes,
            bool replaceBody = false)
        {
            List<ItemDetailSectionViewModel> sections = (source
                    ?? Enumerable.Empty<ItemDetailSectionViewModel>())
                .Where(value => value != null)
                .ToList();
            int firstIndex = sections.FindIndex(value => string.Equals(
                value.stateKey,
                stateKey,
                StringComparison.Ordinal));
            ItemDetailSectionViewModel existing = firstIndex >= 0
                ? sections[firstIndex]
                : null;
            string body = replaceBody
                ? string.Join("\n", ownedLines ?? Enumerable.Empty<string>())
                : MergeOwnedSectionBody(
                    existing?.body,
                    ownedLines,
                    ownedPrefixes);
            ItemDetailSectionViewModel replacement =
                new ItemDetailSectionViewModel(
                    string.IsNullOrWhiteSpace(existing?.title)
                        ? fallbackTitle
                        : existing.title,
                    body,
                    stateKey,
                    existing?.keepWhenEmpty ?? true,
                    existing?.coreEffectRowStates);
            if (firstIndex < 0)
                sections.Add(replacement);
            else
                sections[firstIndex] = replacement;
            for (int index = sections.Count - 1; index >= 0; index--)
            {
                if (index != (firstIndex < 0 ? sections.Count - 1 : firstIndex)
                    && string.Equals(
                        sections[index].stateKey,
                        stateKey,
                        StringComparison.Ordinal))
                    sections.RemoveAt(index);
            }
            return sections;
        }

        private static string MergeOwnedSectionBody(
            string existingBody,
            IEnumerable<string> ownedLines,
            IEnumerable<string> ownedPrefixes)
        {
            string[] prefixes = (ownedPrefixes ?? Enumerable.Empty<string>())
                .ToArray();
            List<string> lines = (existingBody ?? string.Empty)
                .Split(new[] { '\n' }, StringSplitOptions.None)
                .Where(value => value.Trim().Length > 0
                                && !prefixes.Any(prefix => value.TrimStart()
                                    .StartsWith(prefix, StringComparison.Ordinal)))
                .ToList();
            lines.AddRange((ownedLines ?? Enumerable.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value)));
            return string.Join("\n", lines);
        }

        private static string ContributionText(
            C1FormalItemCombatContributionState state)
        {
            switch (state)
            {
                case C1FormalItemCombatContributionState.Contributing:
                    return "正在贡献：已在棋盘并点亮。";
                case C1FormalItemCombatContributionState.Unlit:
                    return "未贡献：已在棋盘，但尚未点亮。";
                case C1FormalItemCombatContributionState.Unplaced:
                    return "未贡献：未放置在棋盘。";
                default:
                    return "不适用。";
            }
        }

        private static bool Fail(string value, out string diagnostic)
        {
            diagnostic = value;
            return false;
        }
    }

    internal static class C1FormalItemDetailCanonical
    {
        internal static string Hash(string value)
        {
            using SHA256 sha = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
            return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "");
        }

        internal static void Append(
            StringBuilder builder,
            string key,
            string value)
        {
            string safeKey = key ?? string.Empty;
            string safeValue = value ?? string.Empty;
            builder.Append(safeKey.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(safeKey).Append('=')
                .Append(safeValue.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(safeValue).Append('\n');
        }

        internal static string Cell(Vector2Int value)
        {
            return value.x.ToString(CultureInfo.InvariantCulture)
                   + ","
                   + value.y.ToString(CultureInfo.InvariantCulture);
        }

        internal static string Cells(IEnumerable<Vector2Int> values)
        {
            return string.Join(
                ";",
                (values ?? Enumerable.Empty<Vector2Int>()).Select(Cell));
        }
    }
}
