using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Capability;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Projection;

namespace TalismanBag.Items.Awakening.RuntimeState
{
    public enum ItemCoreEffectRuntimeStateStatus
    {
        Valid = 0,
        Unknown = 1,
        Invalid = 2
    }

    public enum ItemCoreEffectRosterCompleteness
    {
        Unknown = 0,
        Partial = 1,
        Complete = 2
    }

    public enum ItemCoreEffectRuntimeLocation
    {
        Unknown = 0,
        Inventory = 1,
        Board = 2
    }

    public enum ItemCoreEffectFactCompleteness
    {
        Unknown = 0,
        Complete = 1,
        NotApplicable = 2
    }

    public enum ItemCoreEffectBooleanFact
    {
        Unknown = 0,
        KnownFalse = 1,
        KnownTrue = 2,
        NotApplicable = 3
    }

    public sealed class ItemCoreEffectRuntimeStateValidationError
    {
        public ItemCoreEffectRuntimeStateValidationError(
            string code,
            ItemCoreEffectRuntimeStateStatus status,
            string itemInstanceId,
            string placementId,
            string baseItemId,
            string candidateDefinitionId,
            string awakeningNodeId,
            string message)
        {
            this.code = ItemCoreEffectRuntimeStateCanonical.Required(code);
            this.status = status;
            this.itemInstanceId = ItemCoreEffectRuntimeStateCanonical.Optional(itemInstanceId);
            this.placementId = ItemCoreEffectRuntimeStateCanonical.Optional(placementId);
            this.baseItemId = ItemCoreEffectRuntimeStateCanonical.Optional(baseItemId);
            this.candidateDefinitionId =
                ItemCoreEffectRuntimeStateCanonical.Optional(candidateDefinitionId);
            this.awakeningNodeId =
                ItemCoreEffectRuntimeStateCanonical.Optional(awakeningNodeId);
            this.message = message ?? string.Empty;
        }

        public string code { get; }
        public ItemCoreEffectRuntimeStateStatus status { get; }
        public string itemInstanceId { get; }
        public string placementId { get; }
        public string baseItemId { get; }
        public string candidateDefinitionId { get; }
        public string awakeningNodeId { get; }
        public string message { get; }
    }

    public sealed class ItemCoreEffectIdentityRow
    {
        public ItemCoreEffectIdentityRow(
            string baseItemId,
            ItemCoreAwakeningNodeKind nodeKind,
            string candidateDefinitionId,
            string awakeningNodeId,
            string displayName,
            string description,
            int candidateUnlockLevel,
            int awakeningUnlockLevel,
            ItemInstanceRarity requiredRarity,
            string requiredRarityKey,
            string candidateSourceIdentity,
            string awakeningSourceIdentity,
            string candidatePayloadIdentity,
            string candidateEffectCategory,
            string candidateEffectOperation)
        {
            this.baseItemId = ItemCoreEffectRuntimeStateCanonical.Required(baseItemId);
            this.nodeKind = nodeKind;
            this.candidateDefinitionId =
                ItemCoreEffectRuntimeStateCanonical.Required(candidateDefinitionId);
            this.awakeningNodeId =
                ItemCoreEffectRuntimeStateCanonical.Required(awakeningNodeId);
            this.displayName = displayName ?? string.Empty;
            this.description = description ?? string.Empty;
            this.candidateUnlockLevel = candidateUnlockLevel;
            this.awakeningUnlockLevel = awakeningUnlockLevel;
            this.requiredRarity = requiredRarity;
            this.requiredRarityKey =
                ItemCoreEffectRuntimeStateCanonical.Required(requiredRarityKey);
            this.candidateSourceIdentity =
                ItemCoreEffectRuntimeStateCanonical.Required(candidateSourceIdentity);
            this.awakeningSourceIdentity =
                ItemCoreEffectRuntimeStateCanonical.Required(awakeningSourceIdentity);
            this.candidatePayloadIdentity =
                ItemCoreEffectRuntimeStateCanonical.Required(candidatePayloadIdentity);
            this.candidateEffectCategory =
                ItemCoreEffectRuntimeStateCanonical.Required(candidateEffectCategory);
            this.candidateEffectOperation =
                ItemCoreEffectRuntimeStateCanonical.Required(candidateEffectOperation);
        }

        public string baseItemId { get; }
        public ItemCoreAwakeningNodeKind nodeKind { get; }
        public string candidateDefinitionId { get; }
        public string awakeningNodeId { get; }
        public string displayName { get; }
        public string description { get; }
        public int candidateUnlockLevel { get; }
        public int awakeningUnlockLevel { get; }
        public ItemInstanceRarity requiredRarity { get; }
        public string requiredRarityKey { get; }
        public string candidateSourceIdentity { get; }
        public string awakeningSourceIdentity { get; }
        public string candidatePayloadIdentity { get; }
        public string candidateEffectCategory { get; }
        public string candidateEffectOperation { get; }

        internal ItemCoreEffectIdentityRow Clone()
        {
            return new ItemCoreEffectIdentityRow(
                baseItemId,
                nodeKind,
                candidateDefinitionId,
                awakeningNodeId,
                displayName,
                description,
                candidateUnlockLevel,
                awakeningUnlockLevel,
                requiredRarity,
                requiredRarityKey,
                candidateSourceIdentity,
                awakeningSourceIdentity,
                candidatePayloadIdentity,
                candidateEffectCategory,
                candidateEffectOperation);
        }
    }

    public sealed class ItemCoreEffectIdentityCatalogSnapshot
    {
        public const string LegacySchemaIdV1 =
            "ItemCoreEffectIdentityCatalogSnapshot.v1";
        public const string CurrentSchemaId =
            "ItemCoreEffectIdentityCatalogSnapshot.v2";
        public const string CurrentAuthorityRevision =
            "ITEM_FOUR_CORE_AWAKENING_RUNTIME_CONTRACT_CORRECTION01_R1";

        private readonly ReadOnlyCollection<ItemCoreEffectIdentityRow> rows;
        private readonly ReadOnlyCollection<ItemCoreEffectRuntimeStateValidationError>
            validationErrors;

        public ItemCoreEffectIdentityCatalogSnapshot(
            ItemCoreEffectRuntimeStateStatus status,
            IEnumerable<ItemCoreEffectIdentityRow> rows,
            IEnumerable<ItemCoreEffectRuntimeStateValidationError> validationErrors)
        {
            schemaId = CurrentSchemaId;
            authorityRevision = CurrentAuthorityRevision;
            this.rows = ItemCoreEffectRuntimeStateReadOnly.Freeze(
                (rows ?? Array.Empty<ItemCoreEffectIdentityRow>())
                .Where(value => value != null)
                .Select(value => value.Clone())
                .OrderBy(value => value.baseItemId, StringComparer.Ordinal)
                .ThenBy(value => ItemCoreEffectRuntimeStateCanonical.NodeRank(
                    value.nodeKind)));
            this.validationErrors = ItemCoreEffectRuntimeStateReadOnly.FreezeErrors(
                (validationErrors ??
                    Array.Empty<ItemCoreEffectRuntimeStateValidationError>())
                .Concat(ItemInstanceCoreEffectRuntimeStateValidation
                    .ValidateIdentityRows(this.rows)));
            this.status =
                ItemInstanceCoreEffectRuntimeStateValidation.ResolveStatus(
                    this.validationErrors, status);
            canonicalSignature = ItemCoreEffectRuntimeStateCanonical.Sha256(
                BuildCanonicalPayload());
        }

        public string schemaId { get; }
        public string authorityRevision { get; }
        public ItemCoreEffectRuntimeStateStatus status { get; }
        public bool isValid => status == ItemCoreEffectRuntimeStateStatus.Valid
            && validationErrors.Count == 0;
        public IReadOnlyList<ItemCoreEffectIdentityRow> Rows => rows;
        public IReadOnlyList<ItemCoreEffectRuntimeStateValidationError>
            ValidationErrors => validationErrors;
        public string canonicalSignature { get; }

        public ItemCoreEffectIdentityRow Find(
            string baseItemId,
            ItemCoreAwakeningNodeKind nodeKind)
        {
            return rows.FirstOrDefault(value =>
                string.Equals(value.baseItemId, baseItemId,
                    StringComparison.Ordinal)
                && value.nodeKind == nodeKind);
        }

        public string BuildCanonicalPayload()
        {
            var builder = new System.Text.StringBuilder();
            ItemCoreEffectRuntimeStateCanonical.Field(builder, "schemaId", schemaId);
            ItemCoreEffectRuntimeStateCanonical.Field(
                builder, "authorityRevision", authorityRevision);
            ItemCoreEffectRuntimeStateCanonical.Field(builder, "status", status.ToString());
            ItemCoreEffectRuntimeStateCanonical.Field(builder, "rowCount",
                rows.Count.ToString(CultureInfo.InvariantCulture));
            foreach (ItemCoreEffectIdentityRow row in rows)
            {
                ItemCoreEffectRuntimeStateCanonical.Field(
                    builder, "baseItemId", row.baseItemId);
                ItemCoreEffectRuntimeStateCanonical.Field(
                    builder, "nodeKind", row.nodeKind.ToString());
                ItemCoreEffectRuntimeStateCanonical.Field(
                    builder, "candidateDefinitionId", row.candidateDefinitionId);
                ItemCoreEffectRuntimeStateCanonical.Field(
                    builder, "awakeningNodeId", row.awakeningNodeId);
                ItemCoreEffectRuntimeStateCanonical.Field(
                    builder, "displayName", row.displayName);
                ItemCoreEffectRuntimeStateCanonical.Field(
                    builder, "description", row.description);
                ItemCoreEffectRuntimeStateCanonical.Field(
                    builder, "candidateUnlockLevel",
                    row.candidateUnlockLevel.ToString(CultureInfo.InvariantCulture));
                ItemCoreEffectRuntimeStateCanonical.Field(
                    builder, "awakeningUnlockLevel",
                    row.awakeningUnlockLevel.ToString(CultureInfo.InvariantCulture));
                ItemCoreEffectRuntimeStateCanonical.Field(
                    builder, "requiredRarity", row.requiredRarity.ToString());
                ItemCoreEffectRuntimeStateCanonical.Field(
                    builder, "requiredRarityKey", row.requiredRarityKey);
                ItemCoreEffectRuntimeStateCanonical.Field(
                    builder, "candidateSourceIdentity",
                    row.candidateSourceIdentity);
                ItemCoreEffectRuntimeStateCanonical.Field(
                    builder, "awakeningSourceIdentity",
                    row.awakeningSourceIdentity);
                ItemCoreEffectRuntimeStateCanonical.Field(
                    builder, "candidatePayloadIdentity",
                    row.candidatePayloadIdentity);
                ItemCoreEffectRuntimeStateCanonical.Field(
                    builder, "candidateEffectCategory",
                    row.candidateEffectCategory);
                ItemCoreEffectRuntimeStateCanonical.Field(
                    builder, "candidateEffectOperation",
                    row.candidateEffectOperation);
            }
            ItemCoreEffectRuntimeStateCanonical.AppendErrors(
                builder, validationErrors);
            return builder.ToString();
        }
    }

    public sealed class ItemCoreEffectCultivationRow
    {
        public ItemCoreEffectCultivationRow(
            string itemInstanceId,
            string baseItemId,
            int? inputLevel,
            string sourceKey,
            ItemCoreEffectFactCompleteness factCompleteness)
        {
            this.itemInstanceId =
                ItemCoreEffectRuntimeStateCanonical.Required(itemInstanceId);
            this.baseItemId =
                ItemCoreEffectRuntimeStateCanonical.Required(baseItemId);
            this.inputLevel = inputLevel;
            this.sourceKey =
                ItemCoreEffectRuntimeStateCanonical.Optional(sourceKey);
            this.factCompleteness = factCompleteness;
        }

        public string itemInstanceId { get; }
        public string baseItemId { get; }
        public int? inputLevel { get; }
        public string sourceKey { get; }
        public ItemCoreEffectFactCompleteness factCompleteness { get; }

        internal ItemCoreEffectCultivationRow Clone()
        {
            return new ItemCoreEffectCultivationRow(
                itemInstanceId,
                baseItemId,
                inputLevel,
                sourceKey,
                factCompleteness);
        }
    }

    public sealed class ItemCoreEffectCultivationRosterSnapshot
    {
        public const string CurrentSchemaId =
            "ItemCoreEffectCultivationRosterSnapshot.v1";

        private readonly ReadOnlyCollection<ItemCoreEffectCultivationRow> rows;
        private readonly ReadOnlyCollection<ItemCoreEffectRuntimeStateValidationError>
            validationErrors;

        public ItemCoreEffectCultivationRosterSnapshot(
            ItemCoreEffectRosterCompleteness rosterCompleteness,
            IEnumerable<ItemCoreEffectCultivationRow> rows)
        {
            schemaId = CurrentSchemaId;
            this.rosterCompleteness = rosterCompleteness;
            this.rows = ItemCoreEffectRuntimeStateReadOnly.Freeze(
                (rows ?? Array.Empty<ItemCoreEffectCultivationRow>())
                .Where(value => value != null)
                .Select(value => value.Clone())
                .OrderBy(value => value.itemInstanceId, StringComparer.Ordinal));
            validationErrors = ItemCoreEffectRuntimeStateReadOnly.FreezeErrors(
                ItemInstanceCoreEffectRuntimeStateValidation.ValidateCultivation(
                    rosterCompleteness, this.rows));
            status = ItemInstanceCoreEffectRuntimeStateValidation.ResolveStatus(
                validationErrors,
                rosterCompleteness == ItemCoreEffectRosterCompleteness.Unknown
                    ? ItemCoreEffectRuntimeStateStatus.Unknown
                    : ItemCoreEffectRuntimeStateStatus.Valid);
            canonicalSignature = ItemCoreEffectRuntimeStateCanonical.Sha256(
                BuildCanonicalPayload());
        }

        public string schemaId { get; }
        public ItemCoreEffectRuntimeStateStatus status { get; }
        public bool isValid => status == ItemCoreEffectRuntimeStateStatus.Valid
            && validationErrors.Count == 0;
        public ItemCoreEffectRosterCompleteness rosterCompleteness { get; }
        public IReadOnlyList<ItemCoreEffectCultivationRow> Rows => rows;
        public IReadOnlyList<ItemCoreEffectRuntimeStateValidationError>
            ValidationErrors => validationErrors;
        public string canonicalSignature { get; }

        public ItemCoreEffectCultivationRow Find(string itemInstanceId)
        {
            return rows.FirstOrDefault(value => string.Equals(
                value.itemInstanceId,
                itemInstanceId,
                StringComparison.Ordinal));
        }

        public string BuildCanonicalPayload()
        {
            var builder = new System.Text.StringBuilder();
            ItemCoreEffectRuntimeStateCanonical.Field(builder, "schemaId", schemaId);
            ItemCoreEffectRuntimeStateCanonical.Field(builder, "status", status.ToString());
            ItemCoreEffectRuntimeStateCanonical.Field(
                builder, "rosterCompleteness", rosterCompleteness.ToString());
            ItemCoreEffectRuntimeStateCanonical.Field(builder, "rowCount",
                rows.Count.ToString(CultureInfo.InvariantCulture));
            foreach (ItemCoreEffectCultivationRow row in rows)
            {
                ItemCoreEffectRuntimeStateCanonical.Field(
                    builder, "itemInstanceId", row.itemInstanceId);
                ItemCoreEffectRuntimeStateCanonical.Field(
                    builder, "baseItemId", row.baseItemId);
                ItemCoreEffectRuntimeStateCanonical.NullableInt(
                    builder, "inputLevel", row.inputLevel);
                ItemCoreEffectRuntimeStateCanonical.OptionalField(
                    builder, "sourceKey", row.sourceKey);
                ItemCoreEffectRuntimeStateCanonical.Field(
                    builder, "factCompleteness", row.factCompleteness.ToString());
            }
            ItemCoreEffectRuntimeStateCanonical.AppendErrors(
                builder, validationErrors);
            return builder.ToString();
        }
    }

    public sealed class ItemInstanceCoreEffectRuntimeStateInput
    {
        public ItemInstanceCoreEffectRuntimeStateInput(
            ItemCoreEffectIdentityCatalogSnapshot identityCatalog,
            ItemCoreEffectCultivationRosterSnapshot cultivationRoster,
            ItemInstanceProjectionSetSnapshot projectionSet,
            ItemInstancePlacementBindingContractSnapshot bindingSnapshot,
            ItemSystemSnapshot itemSystemSnapshot,
            ItemCoreEffectRosterCompleteness rosterCompleteness)
        {
            this.identityCatalog = identityCatalog;
            this.cultivationRoster = cultivationRoster;
            this.projectionSet = projectionSet;
            this.bindingSnapshot = bindingSnapshot;
            this.itemSystemSnapshot = itemSystemSnapshot;
            this.rosterCompleteness = rosterCompleteness;
        }

        public ItemCoreEffectIdentityCatalogSnapshot identityCatalog { get; }
        public ItemCoreEffectCultivationRosterSnapshot cultivationRoster { get; }
        public ItemInstanceProjectionSetSnapshot projectionSet { get; }
        public ItemInstancePlacementBindingContractSnapshot bindingSnapshot { get; }
        public ItemSystemSnapshot itemSystemSnapshot { get; }
        public ItemCoreEffectRosterCompleteness rosterCompleteness { get; }
    }

    public sealed class ItemInstanceCoreEffectRuntimeRow
    {
        private readonly ReadOnlyCollection<string> sourceIdentities;

        public ItemInstanceCoreEffectRuntimeRow(
            string baseItemId,
            string itemInstanceId,
            ItemCoreAwakeningNodeKind nodeKind,
            string candidateDefinitionId,
            string awakeningNodeId,
            string displayName,
            string description,
            int requiredLevel,
            ItemInstanceRarity requiredRarity,
            string requiredRarityKey,
            ItemCoreEffectBooleanFact eligibleFact,
            ItemCoreEffectBooleanFact visibleFact,
            ItemCoreEffectBooleanFact unlockedFact,
            ItemCoreEffectBooleanFact litFact,
            ItemCoreEffectBooleanFact activeFact,
            ItemCoreEffectFactCompleteness stateCompleteness,
            IEnumerable<string> sourceIdentities)
        {
            this.baseItemId =
                ItemCoreEffectRuntimeStateCanonical.Required(baseItemId);
            this.itemInstanceId =
                ItemCoreEffectRuntimeStateCanonical.Required(itemInstanceId);
            this.nodeKind = nodeKind;
            this.candidateDefinitionId =
                ItemCoreEffectRuntimeStateCanonical.Required(candidateDefinitionId);
            this.awakeningNodeId =
                ItemCoreEffectRuntimeStateCanonical.Required(awakeningNodeId);
            this.displayName = displayName ?? string.Empty;
            this.description = description ?? string.Empty;
            this.requiredLevel = requiredLevel;
            this.requiredRarity = requiredRarity;
            this.requiredRarityKey =
                ItemCoreEffectRuntimeStateCanonical.Required(requiredRarityKey);
            this.eligibleFact = eligibleFact;
            this.visibleFact = visibleFact;
            this.unlockedFact = unlockedFact;
            this.litFact = litFact;
            this.activeFact = activeFact;
            this.stateCompleteness = stateCompleteness;
            this.sourceIdentities = ItemCoreEffectRuntimeStateReadOnly.FreezeText(
                sourceIdentities);
        }

        public string baseItemId { get; }
        public string itemInstanceId { get; }
        public ItemCoreAwakeningNodeKind nodeKind { get; }
        public string candidateDefinitionId { get; }
        public string awakeningNodeId { get; }
        public string displayName { get; }
        public string description { get; }
        public int requiredLevel { get; }
        public ItemInstanceRarity requiredRarity { get; }
        public string requiredRarityKey { get; }
        public ItemCoreEffectBooleanFact eligibleFact { get; }
        public ItemCoreEffectBooleanFact visibleFact { get; }
        public ItemCoreEffectBooleanFact unlockedFact { get; }
        public ItemCoreEffectBooleanFact litFact { get; }
        public ItemCoreEffectBooleanFact activeFact { get; }
        public ItemCoreEffectFactCompleteness stateCompleteness { get; }
        public IReadOnlyList<string> SourceIdentities => sourceIdentities;
    }

    public sealed class ItemInstanceCoreEffectRuntimeItemSnapshot
    {
        private readonly ReadOnlyCollection<ItemInstanceCoreEffectRuntimeRow>
            coreEffectRows;

        public ItemInstanceCoreEffectRuntimeItemSnapshot(
            string itemInstanceId,
            string baseItemId,
            ItemInstanceRarity rarity,
            ItemCoreEffectRuntimeLocation location,
            string placementId,
            int? cultivationLevel,
            string cultivationLevelSource,
            ItemCoreEffectFactCompleteness cultivationFactCompleteness,
            ItemCoreEffectBooleanFact isLitFact,
            IEnumerable<ItemInstanceCoreEffectRuntimeRow> coreEffectRows,
            ItemCoreEffectFactCompleteness stateCompleteness)
        {
            this.itemInstanceId =
                ItemCoreEffectRuntimeStateCanonical.Required(itemInstanceId);
            this.baseItemId =
                ItemCoreEffectRuntimeStateCanonical.Required(baseItemId);
            this.rarity = rarity;
            this.location = location;
            this.placementId =
                ItemCoreEffectRuntimeStateCanonical.Optional(placementId);
            this.cultivationLevel = cultivationLevel;
            this.cultivationLevelSource =
                ItemCoreEffectRuntimeStateCanonical.Optional(cultivationLevelSource);
            this.cultivationFactCompleteness = cultivationFactCompleteness;
            this.isLitFact = isLitFact;
            this.coreEffectRows = ItemCoreEffectRuntimeStateReadOnly.Freeze(
                (coreEffectRows ?? Array.Empty<ItemInstanceCoreEffectRuntimeRow>())
                .Where(value => value != null)
                .OrderBy(value => ItemCoreEffectRuntimeStateCanonical.NodeRank(
                    value.nodeKind)));
            this.stateCompleteness = stateCompleteness;
        }

        public string itemInstanceId { get; }
        public string baseItemId { get; }
        public ItemInstanceRarity rarity { get; }
        public ItemCoreEffectRuntimeLocation location { get; }
        public string placementId { get; }
        public int? cultivationLevel { get; }
        public string cultivationLevelSource { get; }
        public ItemCoreEffectFactCompleteness cultivationFactCompleteness { get; }
        public ItemCoreEffectBooleanFact isLitFact { get; }
        public IReadOnlyList<ItemInstanceCoreEffectRuntimeRow> CoreEffectRows =>
            coreEffectRows;
        public ItemCoreEffectFactCompleteness stateCompleteness { get; }
    }

    public sealed class ItemInstanceCoreEffectRuntimeStateSnapshot
    {
        public const string LegacySchemaIdV1 =
            "ItemInstanceCoreEffectRuntimeStateSnapshot.v1";
        public const string CurrentSchemaId =
            "ItemInstanceCoreEffectRuntimeStateSnapshot.v2";
        public const string CurrentAuthorityRevision =
            "ITEM_FOUR_CORE_AWAKENING_RUNTIME_CONTRACT_CORRECTION01_R1";

        private readonly ReadOnlyCollection<ItemInstanceCoreEffectRuntimeItemSnapshot>
            items;
        private readonly ReadOnlyCollection<ItemCoreEffectRuntimeStateValidationError>
            validationErrors;

        public ItemInstanceCoreEffectRuntimeStateSnapshot(
            ItemCoreEffectRuntimeStateStatus status,
            ItemCoreEffectRosterCompleteness rosterCompleteness,
            string sourceIdentityCatalogSignature,
            string sourceCultivationRosterSignature,
            string sourceProjectionSetSignature,
            string sourceBindingSignature,
            string sourceItemSystemSignature,
            IEnumerable<ItemInstanceCoreEffectRuntimeItemSnapshot> items,
            IEnumerable<ItemCoreEffectRuntimeStateValidationError> validationErrors)
        {
            schemaId = CurrentSchemaId;
            authorityRevision = CurrentAuthorityRevision;
            this.rosterCompleteness = rosterCompleteness;
            this.sourceIdentityCatalogSignature =
                ItemCoreEffectRuntimeStateCanonical.Optional(
                    sourceIdentityCatalogSignature);
            this.sourceCultivationRosterSignature =
                ItemCoreEffectRuntimeStateCanonical.Optional(
                    sourceCultivationRosterSignature);
            this.sourceProjectionSetSignature =
                ItemCoreEffectRuntimeStateCanonical.Optional(
                    sourceProjectionSetSignature);
            this.sourceBindingSignature =
                ItemCoreEffectRuntimeStateCanonical.Optional(sourceBindingSignature);
            this.sourceItemSystemSignature =
                ItemCoreEffectRuntimeStateCanonical.Optional(
                    sourceItemSystemSignature);
            this.items = ItemCoreEffectRuntimeStateReadOnly.Freeze(
                (items ?? Array.Empty<ItemInstanceCoreEffectRuntimeItemSnapshot>())
                .Where(value => value != null)
                .OrderBy(value => value.itemInstanceId, StringComparer.Ordinal));
            this.validationErrors = ItemCoreEffectRuntimeStateReadOnly.FreezeErrors(
                (validationErrors ??
                    Array.Empty<ItemCoreEffectRuntimeStateValidationError>())
                .Concat(ItemInstanceCoreEffectRuntimeStateValidation
                    .ValidateRuntimeItems(this.items)));
            this.status =
                ItemInstanceCoreEffectRuntimeStateValidation.ResolveStatus(
                    this.validationErrors, status);
            canonicalSignature = ItemCoreEffectRuntimeStateCanonical.Sha256(
                BuildCanonicalPayload());
        }

        public string schemaId { get; }
        public string authorityRevision { get; }
        public ItemCoreEffectRuntimeStateStatus status { get; }
        public bool isValid => status == ItemCoreEffectRuntimeStateStatus.Valid
            && validationErrors.Count == 0;
        public ItemCoreEffectRosterCompleteness rosterCompleteness { get; }
        public string sourceIdentityCatalogSignature { get; }
        public string sourceCultivationRosterSignature { get; }
        public string sourceProjectionSetSignature { get; }
        public string sourceBindingSignature { get; }
        public string sourceItemSystemSignature { get; }
        public IReadOnlyList<ItemInstanceCoreEffectRuntimeItemSnapshot> Items => items;
        public IReadOnlyList<ItemCoreEffectRuntimeStateValidationError>
            ValidationErrors => validationErrors;
        public string canonicalSignature { get; }

        public ItemInstanceCoreEffectRuntimeItemSnapshot Find(string itemInstanceId)
        {
            return items.FirstOrDefault(value => string.Equals(
                value.itemInstanceId, itemInstanceId, StringComparison.Ordinal));
        }

        public string BuildCanonicalPayload()
        {
            var builder = new System.Text.StringBuilder();
            ItemCoreEffectRuntimeStateCanonical.Field(builder, "schemaId", schemaId);
            ItemCoreEffectRuntimeStateCanonical.Field(
                builder, "authorityRevision", authorityRevision);
            ItemCoreEffectRuntimeStateCanonical.Field(builder, "status", status.ToString());
            ItemCoreEffectRuntimeStateCanonical.Field(
                builder, "rosterCompleteness", rosterCompleteness.ToString());
            ItemCoreEffectRuntimeStateCanonical.OptionalField(
                builder, "sourceIdentityCatalogSignature",
                sourceIdentityCatalogSignature);
            ItemCoreEffectRuntimeStateCanonical.OptionalField(
                builder, "sourceCultivationRosterSignature",
                sourceCultivationRosterSignature);
            ItemCoreEffectRuntimeStateCanonical.OptionalField(
                builder, "sourceProjectionSetSignature",
                sourceProjectionSetSignature);
            ItemCoreEffectRuntimeStateCanonical.OptionalField(
                builder, "sourceBindingSignature", sourceBindingSignature);
            ItemCoreEffectRuntimeStateCanonical.OptionalField(
                builder, "sourceItemSystemSignature", sourceItemSystemSignature);
            ItemCoreEffectRuntimeStateCanonical.Field(builder, "itemCount",
                items.Count.ToString(CultureInfo.InvariantCulture));
            foreach (ItemInstanceCoreEffectRuntimeItemSnapshot item in items)
            {
                ItemCoreEffectRuntimeStateCanonical.Field(
                    builder, "itemInstanceId", item.itemInstanceId);
                ItemCoreEffectRuntimeStateCanonical.Field(
                    builder, "baseItemId", item.baseItemId);
                ItemCoreEffectRuntimeStateCanonical.Field(
                    builder, "rarity", item.rarity.ToString());
                ItemCoreEffectRuntimeStateCanonical.Field(
                    builder, "location", item.location.ToString());
                ItemCoreEffectRuntimeStateCanonical.OptionalField(
                    builder, "placementId", item.placementId);
                ItemCoreEffectRuntimeStateCanonical.NullableInt(
                    builder, "cultivationLevel", item.cultivationLevel);
                ItemCoreEffectRuntimeStateCanonical.OptionalField(
                    builder, "cultivationLevelSource",
                    item.cultivationLevelSource);
                ItemCoreEffectRuntimeStateCanonical.Field(
                    builder, "cultivationFactCompleteness",
                    item.cultivationFactCompleteness.ToString());
                ItemCoreEffectRuntimeStateCanonical.Field(
                    builder, "isLitFact", item.isLitFact.ToString());
                ItemCoreEffectRuntimeStateCanonical.Field(
                    builder, "stateCompleteness",
                    item.stateCompleteness.ToString());
                foreach (ItemInstanceCoreEffectRuntimeRow row in item.CoreEffectRows)
                {
                    ItemCoreEffectRuntimeStateCanonical.Field(
                        builder, "coreBaseItemId", row.baseItemId);
                    ItemCoreEffectRuntimeStateCanonical.Field(
                        builder, "coreItemInstanceId", row.itemInstanceId);
                    ItemCoreEffectRuntimeStateCanonical.Field(
                        builder, "nodeKind", row.nodeKind.ToString());
                    ItemCoreEffectRuntimeStateCanonical.Field(
                        builder, "candidateDefinitionId",
                        row.candidateDefinitionId);
                    ItemCoreEffectRuntimeStateCanonical.Field(
                        builder, "awakeningNodeId", row.awakeningNodeId);
                    ItemCoreEffectRuntimeStateCanonical.Field(
                        builder, "displayName", row.displayName);
                    ItemCoreEffectRuntimeStateCanonical.Field(
                        builder, "description", row.description);
                    ItemCoreEffectRuntimeStateCanonical.Field(
                        builder, "requiredLevel",
                        row.requiredLevel.ToString(CultureInfo.InvariantCulture));
                    ItemCoreEffectRuntimeStateCanonical.Field(
                        builder, "requiredRarity", row.requiredRarity.ToString());
                    ItemCoreEffectRuntimeStateCanonical.Field(
                        builder, "requiredRarityKey", row.requiredRarityKey);
                    ItemCoreEffectRuntimeStateCanonical.Field(
                        builder, "eligibleFact", row.eligibleFact.ToString());
                    ItemCoreEffectRuntimeStateCanonical.Field(
                        builder, "visibleFact", row.visibleFact.ToString());
                    ItemCoreEffectRuntimeStateCanonical.Field(
                        builder, "unlockedFact", row.unlockedFact.ToString());
                    ItemCoreEffectRuntimeStateCanonical.Field(
                        builder, "litFact", row.litFact.ToString());
                    ItemCoreEffectRuntimeStateCanonical.Field(
                        builder, "activeFact", row.activeFact.ToString());
                    ItemCoreEffectRuntimeStateCanonical.Field(
                        builder, "coreStateCompleteness",
                        row.stateCompleteness.ToString());
                    foreach (string source in row.SourceIdentities)
                    {
                        ItemCoreEffectRuntimeStateCanonical.Field(
                            builder, "sourceIdentity", source);
                    }
                }
            }
            ItemCoreEffectRuntimeStateCanonical.AppendErrors(
                builder, validationErrors);
            return builder.ToString();
        }
    }

    internal static class ItemCoreEffectRuntimeStateReadOnly
    {
        public static ReadOnlyCollection<T> Freeze<T>(IEnumerable<T> values)
        {
            return Array.AsReadOnly((values ?? Array.Empty<T>()).ToArray());
        }

        public static ReadOnlyCollection<string> FreezeText(
            IEnumerable<string> values)
        {
            return Array.AsReadOnly((values ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray());
        }

        public static ReadOnlyCollection<ItemCoreEffectRuntimeStateValidationError>
            FreezeErrors(
                IEnumerable<ItemCoreEffectRuntimeStateValidationError> values)
        {
            return Array.AsReadOnly((values ??
                    Array.Empty<ItemCoreEffectRuntimeStateValidationError>())
                .Where(value => value != null)
                .GroupBy(value => value.code + "|" + value.status + "|"
                    + value.itemInstanceId + "|" + value.placementId + "|"
                    + value.baseItemId + "|" + value.candidateDefinitionId + "|"
                    + value.awakeningNodeId + "|" + value.message,
                    StringComparer.Ordinal)
                .Select(group => group.First())
                .OrderBy(value => value.code, StringComparer.Ordinal)
                .ThenBy(value => value.itemInstanceId, StringComparer.Ordinal)
                .ThenBy(value => value.placementId, StringComparer.Ordinal)
                .ThenBy(value => value.baseItemId, StringComparer.Ordinal)
                .ThenBy(value => value.candidateDefinitionId,
                    StringComparer.Ordinal)
                .ThenBy(value => value.awakeningNodeId,
                    StringComparer.Ordinal)
                .ThenBy(value => value.message, StringComparer.Ordinal)
                .ToArray());
        }
    }
}
