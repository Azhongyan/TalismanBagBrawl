using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.Items.Build.Qualified;
using TalismanBag.Items.Generation;

namespace TalismanBag.Items.Combat
{
    public enum ItemCombatEffectRequestSnapshotStatus
    {
        Valid = 0,
        Unknown = 1,
        Invalid = 2
    }

    public enum ItemCombatEffectRequestKind
    {
        DirectFlatDamage = 0
    }

    public enum ItemCombatEffectMagnitudeCompleteness
    {
        Complete = 0
    }

    public enum ItemCombatEffectTargetRequestKind
    {
        SingleHostileDamageableRuntimeActor = 0
    }

    public enum ItemCombatEffectContributionSourceKind
    {
        BaseStat = 0,
        Affix = 1,
        Build = 2
    }

    public enum ItemCombatEffectContributionDisposition
    {
        AppliedToRequest = 0
    }

    public enum ItemCombatEffectUnsupportedSourceKind
    {
        Affix = 0,
        Build = 1,
        Core = 2,
        TextSemantic = 3,
        Resource = 4,
        Cooldown = 5
    }

    public enum ItemCombatEffectMagnitudePresence
    {
        Present = 0,
        Missing = 1,
        Conflicted = 2
    }

    public enum ItemCombatEffectUnsupportedDisposition
    {
        NotExecuted = 0,
        Unknown = 1,
        Rejected = 2
    }

    public sealed class ItemCombatEffectRequestValidationError
    {
        public ItemCombatEffectRequestValidationError(
            string code,
            ItemCombatEffectRequestSnapshotStatus status,
            string itemInstanceId,
            string placementId,
            string message)
        {
            this.code = ItemCombatEffectRequestCanonical.Required(code);
            this.status = status;
            this.itemInstanceId = ItemCombatEffectRequestCanonical.Optional(itemInstanceId);
            this.placementId = ItemCombatEffectRequestCanonical.Optional(placementId);
            this.message = message ?? string.Empty;
        }

        public string code { get; }
        public ItemCombatEffectRequestSnapshotStatus status { get; }
        public string itemInstanceId { get; }
        public string placementId { get; }
        public string message { get; }
    }

    public sealed class ItemCombatEffectRequestContribution
    {
        public ItemCombatEffectRequestContribution(
            ItemCombatEffectContributionSourceKind sourceKind,
            string sourceEffectId,
            string operation,
            string targetStatId,
            string valueUnitKey,
            long rawUnits,
            int stackCount,
            long appliedBasisPoints,
            ItemCombatEffectContributionDisposition disposition,
            string sourceDataMaturity)
        {
            this.sourceKind = sourceKind;
            this.sourceEffectId = ItemCombatEffectRequestCanonical.Required(sourceEffectId);
            this.operation = ItemCombatEffectRequestCanonical.Required(operation);
            this.targetStatId = ItemCombatEffectRequestCanonical.Required(targetStatId);
            this.valueUnitKey = ItemCombatEffectRequestCanonical.Required(valueUnitKey);
            this.rawUnits = rawUnits;
            this.stackCount = stackCount;
            this.appliedBasisPoints = appliedBasisPoints;
            this.disposition = disposition;
            this.sourceDataMaturity =
                ItemCombatEffectRequestCanonical.Required(sourceDataMaturity);
        }

        public ItemCombatEffectContributionSourceKind sourceKind { get; }
        public string sourceEffectId { get; }
        public string operation { get; }
        public string targetStatId { get; }
        public string valueUnitKey { get; }
        public long rawUnits { get; }
        public int stackCount { get; }
        public long appliedBasisPoints { get; }
        public ItemCombatEffectContributionDisposition disposition { get; }
        public string sourceDataMaturity { get; }

        internal ItemCombatEffectRequestContribution Clone()
        {
            return new ItemCombatEffectRequestContribution(
                sourceKind,
                sourceEffectId,
                operation,
                targetStatId,
                valueUnitKey,
                rawUnits,
                stackCount,
                appliedBasisPoints,
                disposition,
                sourceDataMaturity);
        }
    }

    public sealed class ItemCombatEffectUnsupportedTelemetry
    {
        private readonly ReadOnlyCollection<string> sourceIdentities;

        public ItemCombatEffectUnsupportedTelemetry(
            string sourceItemInstanceId,
            string sourceBaseItemId,
            string sourcePlacementId,
            string sourceEffectId,
            ItemCombatEffectUnsupportedSourceKind sourceKind,
            string machineOperation,
            string machineUnit,
            ItemCombatEffectMagnitudePresence magnitudePresence,
            long? magnitudeUnits,
            ItemCombatEffectUnsupportedDisposition disposition,
            string reasonCode,
            IEnumerable<string> sourceIdentities)
        {
            this.sourceItemInstanceId =
                ItemCombatEffectRequestCanonical.Optional(sourceItemInstanceId);
            this.sourceBaseItemId =
                ItemCombatEffectRequestCanonical.Optional(sourceBaseItemId);
            this.sourcePlacementId =
                ItemCombatEffectRequestCanonical.Optional(sourcePlacementId);
            this.sourceEffectId = ItemCombatEffectRequestCanonical.Required(sourceEffectId);
            this.sourceKind = sourceKind;
            this.machineOperation =
                ItemCombatEffectRequestCanonical.Required(machineOperation);
            this.machineUnit = ItemCombatEffectRequestCanonical.Required(machineUnit);
            this.magnitudePresence = magnitudePresence;
            this.magnitudeUnits = magnitudeUnits;
            this.disposition = disposition;
            this.reasonCode = ItemCombatEffectRequestCanonical.Required(reasonCode);
            this.sourceIdentities = ItemCombatEffectRequestReadOnly.FreezeText(
                sourceIdentities);
        }

        public string sourceItemInstanceId { get; }
        public string sourceBaseItemId { get; }
        public string sourcePlacementId { get; }
        public string sourceEffectId { get; }
        public ItemCombatEffectUnsupportedSourceKind sourceKind { get; }
        public string machineOperation { get; }
        public string machineUnit { get; }
        public ItemCombatEffectMagnitudePresence magnitudePresence { get; }
        public long? magnitudeUnits { get; }
        public ItemCombatEffectUnsupportedDisposition disposition { get; }
        public string reasonCode { get; }
        public IReadOnlyList<string> SourceIdentities => sourceIdentities;

        internal ItemCombatEffectUnsupportedTelemetry Clone()
        {
            return new ItemCombatEffectUnsupportedTelemetry(
                sourceItemInstanceId,
                sourceBaseItemId,
                sourcePlacementId,
                sourceEffectId,
                sourceKind,
                machineOperation,
                machineUnit,
                magnitudePresence,
                magnitudeUnits,
                disposition,
                reasonCode,
                sourceIdentities);
        }
    }

    public sealed class ItemCombatEffectRequestRow
    {
        private readonly ReadOnlyCollection<string> sourceEffectIds;
        private readonly ReadOnlyCollection<string> activeCoreEffectIds;
        private readonly ReadOnlyCollection<ItemCombatEffectRequestContribution> contributions;

        public ItemCombatEffectRequestRow(
            string requestId,
            ItemCombatEffectRequestKind requestKind,
            string sourceItemInstanceId,
            string sourceBaseItemId,
            string sourcePlacementId,
            ItemInstanceRarity sourceRarity,
            long sourceRootSeed,
            string sourceFaMenTag,
            string sourceQiLeiTag,
            IEnumerable<string> sourceEffectIds,
            long baseDamageRawUnits,
            long additiveBasisPoints,
            long resolvedPreMitigationDamageUnits,
            ItemCombatEffectMagnitudeCompleteness magnitudeCompleteness,
            bool requiresBattleTargetValidation,
            ItemCombatEffectTargetRequestKind targetRequestKind,
            ItemInstanceQualifiedBuildBooleanFact isLitFact,
            ItemInstanceQualifiedBuildBooleanFact sourceIsCountedFact,
            ItemInstanceQualifiedBuildBooleanFact qualifiedIsCountedFact,
            string faMenBuildId,
            int faMenBuildCount,
            int faMenActiveStagePieceCount,
            IEnumerable<string> activeCoreEffectIds,
            string sourceProjectionCanonicalSignature,
            string sourcePlacementCanonicalFacts,
            IEnumerable<ItemCombatEffectRequestContribution> contributions)
        {
            this.requestId = ItemCombatEffectRequestCanonical.Required(requestId);
            this.requestKind = requestKind;
            this.sourceItemInstanceId =
                ItemCombatEffectRequestCanonical.Required(sourceItemInstanceId);
            this.sourceBaseItemId =
                ItemCombatEffectRequestCanonical.Required(sourceBaseItemId);
            this.sourcePlacementId =
                ItemCombatEffectRequestCanonical.Required(sourcePlacementId);
            this.sourceRarity = sourceRarity;
            this.sourceRootSeed = sourceRootSeed;
            this.sourceFaMenTag =
                ItemCombatEffectRequestCanonical.Optional(sourceFaMenTag);
            this.sourceQiLeiTag =
                ItemCombatEffectRequestCanonical.Optional(sourceQiLeiTag);
            this.sourceEffectIds =
                ItemCombatEffectRequestReadOnly.FreezeText(sourceEffectIds);
            this.baseDamageRawUnits = baseDamageRawUnits;
            this.additiveBasisPoints = additiveBasisPoints;
            this.resolvedPreMitigationDamageUnits =
                resolvedPreMitigationDamageUnits;
            this.magnitudeCompleteness = magnitudeCompleteness;
            this.requiresBattleTargetValidation = requiresBattleTargetValidation;
            this.targetRequestKind = targetRequestKind;
            this.isLitFact = isLitFact;
            this.sourceIsCountedFact = sourceIsCountedFact;
            this.qualifiedIsCountedFact = qualifiedIsCountedFact;
            this.faMenBuildId =
                ItemCombatEffectRequestCanonical.Optional(faMenBuildId);
            this.faMenBuildCount = faMenBuildCount;
            this.faMenActiveStagePieceCount = faMenActiveStagePieceCount;
            this.activeCoreEffectIds =
                ItemCombatEffectRequestReadOnly.FreezeText(activeCoreEffectIds);
            this.sourceProjectionCanonicalSignature =
                ItemCombatEffectRequestCanonical.Required(
                    sourceProjectionCanonicalSignature);
            this.sourcePlacementCanonicalFacts =
                sourcePlacementCanonicalFacts ?? string.Empty;
            this.contributions = ItemCombatEffectRequestReadOnly.Freeze(
                (contributions ?? Array.Empty<ItemCombatEffectRequestContribution>())
                .Where(value => value != null)
                .Select(value => value.Clone())
                .OrderBy(value => (int)value.sourceKind)
                .ThenBy(value => value.sourceEffectId, StringComparer.Ordinal));
        }

        public string requestId { get; }
        public ItemCombatEffectRequestKind requestKind { get; }
        public string sourceItemInstanceId { get; }
        public string sourceBaseItemId { get; }
        public string sourcePlacementId { get; }
        public ItemInstanceRarity sourceRarity { get; }
        public long sourceRootSeed { get; }
        public string sourceFaMenTag { get; }
        public string sourceQiLeiTag { get; }
        public IReadOnlyList<string> SourceEffectIds => sourceEffectIds;
        public long baseDamageRawUnits { get; }
        public long additiveBasisPoints { get; }
        public long resolvedPreMitigationDamageUnits { get; }
        public ItemCombatEffectMagnitudeCompleteness magnitudeCompleteness { get; }
        public bool requiresBattleTargetValidation { get; }
        public ItemCombatEffectTargetRequestKind targetRequestKind { get; }
        public ItemInstanceQualifiedBuildBooleanFact isLitFact { get; }
        public ItemInstanceQualifiedBuildBooleanFact sourceIsCountedFact { get; }
        public ItemInstanceQualifiedBuildBooleanFact qualifiedIsCountedFact { get; }
        public string faMenBuildId { get; }
        public int faMenBuildCount { get; }
        public int faMenActiveStagePieceCount { get; }
        public IReadOnlyList<string> ActiveCoreEffectIds => activeCoreEffectIds;
        public string sourceProjectionCanonicalSignature { get; }
        public string sourcePlacementCanonicalFacts { get; }
        public IReadOnlyList<ItemCombatEffectRequestContribution> Contributions =>
            contributions;

        internal ItemCombatEffectRequestRow Clone()
        {
            return new ItemCombatEffectRequestRow(
                requestId,
                requestKind,
                sourceItemInstanceId,
                sourceBaseItemId,
                sourcePlacementId,
                sourceRarity,
                sourceRootSeed,
                sourceFaMenTag,
                sourceQiLeiTag,
                sourceEffectIds,
                baseDamageRawUnits,
                additiveBasisPoints,
                resolvedPreMitigationDamageUnits,
                magnitudeCompleteness,
                requiresBattleTargetValidation,
                targetRequestKind,
                isLitFact,
                sourceIsCountedFact,
                qualifiedIsCountedFact,
                faMenBuildId,
                faMenBuildCount,
                faMenActiveStagePieceCount,
                activeCoreEffectIds,
                sourceProjectionCanonicalSignature,
                sourcePlacementCanonicalFacts,
                contributions);
        }
    }

    public sealed class ItemCombatEffectRequestSnapshot
    {
        public const string CurrentSchemaId =
            "ItemCombatEffectRequestSnapshot.v1";
        public const string CurrentAuthorityRevision =
            "ITEM_COMBAT_EFFECT_REQUEST_CONTRACT01_R1";

        private readonly ReadOnlyCollection<ItemCombatEffectRequestRow> requests;
        private readonly ReadOnlyCollection<ItemCombatEffectUnsupportedTelemetry>
            unsupportedTelemetry;
        private readonly ReadOnlyCollection<ItemCombatEffectRequestValidationError>
            validationErrors;

        internal ItemCombatEffectRequestSnapshot(
            ItemCombatEffectRequestSnapshotStatus status,
            string sourceProjectionSetCanonicalSignature,
            string sourceQualifiedProjectionSetIdentity,
            string sourceBindingCanonicalSignature,
            string sourceItemSystemCanonicalSignature,
            string sourceQualifiedBuildCanonicalSignature,
            string sourceCoreRuntimeCanonicalSignature,
            IEnumerable<ItemCombatEffectRequestRow> requests,
            IEnumerable<ItemCombatEffectUnsupportedTelemetry> unsupportedTelemetry,
            IEnumerable<ItemCombatEffectRequestValidationError> validationErrors)
        {
            schemaId = CurrentSchemaId;
            authorityRevision = CurrentAuthorityRevision;
            this.status = status;
            devOnly = true;
            entersFormalBattle = false;
            this.sourceProjectionSetCanonicalSignature =
                ItemCombatEffectRequestCanonical.Optional(
                    sourceProjectionSetCanonicalSignature);
            this.sourceQualifiedProjectionSetIdentity =
                ItemCombatEffectRequestCanonical.Optional(
                    sourceQualifiedProjectionSetIdentity);
            this.sourceBindingCanonicalSignature =
                ItemCombatEffectRequestCanonical.Optional(
                    sourceBindingCanonicalSignature);
            this.sourceItemSystemCanonicalSignature =
                ItemCombatEffectRequestCanonical.Optional(
                    sourceItemSystemCanonicalSignature);
            this.sourceQualifiedBuildCanonicalSignature =
                ItemCombatEffectRequestCanonical.Optional(
                    sourceQualifiedBuildCanonicalSignature);
            this.sourceCoreRuntimeCanonicalSignature =
                ItemCombatEffectRequestCanonical.Optional(
                    sourceCoreRuntimeCanonicalSignature);
            this.requests = ItemCombatEffectRequestReadOnly.Freeze(
                (requests ?? Array.Empty<ItemCombatEffectRequestRow>())
                .Where(value => value != null)
                .Select(value => value.Clone())
                .OrderBy(value => value.sourceItemInstanceId, StringComparer.Ordinal)
                .ThenBy(value => value.sourcePlacementId, StringComparer.Ordinal)
                .ThenBy(value => (int)value.requestKind));
            this.unsupportedTelemetry = ItemCombatEffectRequestReadOnly.Freeze(
                (unsupportedTelemetry ??
                    Array.Empty<ItemCombatEffectUnsupportedTelemetry>())
                .Where(value => value != null)
                .Select(value => value.Clone())
                .OrderBy(value => value.sourceItemInstanceId, StringComparer.Ordinal)
                .ThenBy(value => value.sourceEffectId, StringComparer.Ordinal)
                .ThenBy(value => value.reasonCode, StringComparer.Ordinal));
            this.validationErrors = ItemCombatEffectRequestReadOnly.Freeze(
                (validationErrors ??
                    Array.Empty<ItemCombatEffectRequestValidationError>())
                .Where(value => value != null)
                .GroupBy(value => value.code + "|" + value.status + "|"
                    + value.itemInstanceId + "|" + value.placementId + "|"
                    + value.message, StringComparer.Ordinal)
                .Select(group => group.First())
                .OrderBy(value => value.code, StringComparer.Ordinal)
                .ThenBy(value => value.itemInstanceId, StringComparer.Ordinal)
                .ThenBy(value => value.placementId, StringComparer.Ordinal)
                .ThenBy(value => value.message, StringComparer.Ordinal));
            canonicalSignature =
                ItemCombatEffectRequestCanonical.Sha256(BuildCanonicalPayload());
        }

        public string schemaId { get; }
        public string authorityRevision { get; }
        public ItemCombatEffectRequestSnapshotStatus status { get; }
        public bool devOnly { get; }
        public bool entersFormalBattle { get; }
        public string sourceProjectionSetCanonicalSignature { get; }
        public string sourceQualifiedProjectionSetIdentity { get; }
        public string sourceBindingCanonicalSignature { get; }
        public string sourceItemSystemCanonicalSignature { get; }
        public string sourceQualifiedBuildCanonicalSignature { get; }
        public string sourceCoreRuntimeCanonicalSignature { get; }
        public int requestCount => requests.Count;
        public int telemetryCount => unsupportedTelemetry.Count;
        public IReadOnlyList<ItemCombatEffectRequestRow> Requests => requests;
        public IReadOnlyList<ItemCombatEffectUnsupportedTelemetry>
            UnsupportedTelemetry => unsupportedTelemetry;
        public IReadOnlyList<ItemCombatEffectRequestValidationError>
            ValidationErrors => validationErrors;
        public string canonicalSignature { get; }

        public string BuildCanonicalPayload()
        {
            StringBuilder builder = new StringBuilder();
            ItemCombatEffectRequestCanonical.Field(builder, "schemaId", schemaId);
            ItemCombatEffectRequestCanonical.Field(
                builder, "authorityRevision", authorityRevision);
            ItemCombatEffectRequestCanonical.Field(builder, "status", status.ToString());
            ItemCombatEffectRequestCanonical.Field(
                builder, "devOnly", devOnly ? "true" : "false");
            ItemCombatEffectRequestCanonical.Field(
                builder, "entersFormalBattle",
                entersFormalBattle ? "true" : "false");
            ItemCombatEffectRequestCanonical.OptionalField(
                builder, "sourceProjectionSetCanonicalSignature",
                sourceProjectionSetCanonicalSignature);
            ItemCombatEffectRequestCanonical.OptionalField(
                builder, "sourceQualifiedProjectionSetIdentity",
                sourceQualifiedProjectionSetIdentity);
            ItemCombatEffectRequestCanonical.OptionalField(
                builder, "sourceBindingCanonicalSignature",
                sourceBindingCanonicalSignature);
            ItemCombatEffectRequestCanonical.OptionalField(
                builder, "sourceItemSystemCanonicalSignature",
                sourceItemSystemCanonicalSignature);
            ItemCombatEffectRequestCanonical.OptionalField(
                builder, "sourceQualifiedBuildCanonicalSignature",
                sourceQualifiedBuildCanonicalSignature);
            ItemCombatEffectRequestCanonical.OptionalField(
                builder, "sourceCoreRuntimeCanonicalSignature",
                sourceCoreRuntimeCanonicalSignature);
            ItemCombatEffectRequestCanonical.Field(
                builder, "requestCount",
                requestCount.ToString(CultureInfo.InvariantCulture));
            ItemCombatEffectRequestCanonical.Field(
                builder, "telemetryCount",
                telemetryCount.ToString(CultureInfo.InvariantCulture));
            ItemCombatEffectRequestCanonical.Field(
                builder, "validationErrorCount",
                validationErrors.Count.ToString(CultureInfo.InvariantCulture));

            foreach (ItemCombatEffectRequestRow request in requests)
            {
                AppendRequest(builder, request);
            }
            foreach (ItemCombatEffectUnsupportedTelemetry telemetry
                     in unsupportedTelemetry)
            {
                AppendTelemetry(builder, telemetry);
            }
            foreach (ItemCombatEffectRequestValidationError error
                     in validationErrors)
            {
                ItemCombatEffectRequestCanonical.Field(
                    builder, "error.code", error.code);
                ItemCombatEffectRequestCanonical.Field(
                    builder, "error.status", error.status.ToString());
                ItemCombatEffectRequestCanonical.OptionalField(
                    builder, "error.itemInstanceId", error.itemInstanceId);
                ItemCombatEffectRequestCanonical.OptionalField(
                    builder, "error.placementId", error.placementId);
                ItemCombatEffectRequestCanonical.Field(
                    builder, "error.message", error.message);
            }
            return builder.ToString();
        }

        private static void AppendRequest(
            StringBuilder builder,
            ItemCombatEffectRequestRow request)
        {
            ItemCombatEffectRequestCanonical.Field(
                builder, "request.requestId", request.requestId);
            ItemCombatEffectRequestCanonical.Field(
                builder, "request.requestKind", request.requestKind.ToString());
            ItemCombatEffectRequestCanonical.Field(
                builder, "request.sourceItemInstanceId",
                request.sourceItemInstanceId);
            ItemCombatEffectRequestCanonical.Field(
                builder, "request.sourceBaseItemId", request.sourceBaseItemId);
            ItemCombatEffectRequestCanonical.Field(
                builder, "request.sourcePlacementId", request.sourcePlacementId);
            ItemCombatEffectRequestCanonical.Field(
                builder, "request.sourceRarity", request.sourceRarity.ToString());
            ItemCombatEffectRequestCanonical.LongField(
                builder, "request.sourceRootSeed", request.sourceRootSeed);
            ItemCombatEffectRequestCanonical.OptionalField(
                builder, "request.sourceFaMenTag", request.sourceFaMenTag);
            ItemCombatEffectRequestCanonical.OptionalField(
                builder, "request.sourceQiLeiTag", request.sourceQiLeiTag);
            foreach (string effectId in request.SourceEffectIds)
            {
                ItemCombatEffectRequestCanonical.Field(
                    builder, "request.sourceEffectId", effectId);
            }
            ItemCombatEffectRequestCanonical.LongField(
                builder, "request.baseDamageRawUnits",
                request.baseDamageRawUnits);
            ItemCombatEffectRequestCanonical.LongField(
                builder, "request.additiveBasisPoints",
                request.additiveBasisPoints);
            ItemCombatEffectRequestCanonical.LongField(
                builder, "request.resolvedPreMitigationDamageUnits",
                request.resolvedPreMitigationDamageUnits);
            ItemCombatEffectRequestCanonical.Field(
                builder, "request.magnitudeCompleteness",
                request.magnitudeCompleteness.ToString());
            ItemCombatEffectRequestCanonical.Field(
                builder, "request.requiresBattleTargetValidation",
                request.requiresBattleTargetValidation ? "true" : "false");
            ItemCombatEffectRequestCanonical.Field(
                builder, "request.targetRequestKind",
                request.targetRequestKind.ToString());
            ItemCombatEffectRequestCanonical.Field(
                builder, "request.isLitFact", request.isLitFact.ToString());
            ItemCombatEffectRequestCanonical.Field(
                builder, "request.sourceIsCountedFact",
                request.sourceIsCountedFact.ToString());
            ItemCombatEffectRequestCanonical.Field(
                builder, "request.qualifiedIsCountedFact",
                request.qualifiedIsCountedFact.ToString());
            ItemCombatEffectRequestCanonical.OptionalField(
                builder, "request.faMenBuildId", request.faMenBuildId);
            ItemCombatEffectRequestCanonical.IntField(
                builder, "request.faMenBuildCount", request.faMenBuildCount);
            ItemCombatEffectRequestCanonical.IntField(
                builder, "request.faMenActiveStagePieceCount",
                request.faMenActiveStagePieceCount);
            foreach (string activeCoreEffectId in request.ActiveCoreEffectIds)
            {
                ItemCombatEffectRequestCanonical.Field(
                    builder, "request.activeCoreEffectId", activeCoreEffectId);
            }
            ItemCombatEffectRequestCanonical.Field(
                builder, "request.sourceProjectionCanonicalSignature",
                request.sourceProjectionCanonicalSignature);
            ItemCombatEffectRequestCanonical.Field(
                builder, "request.sourcePlacementCanonicalFacts",
                request.sourcePlacementCanonicalFacts);
            foreach (ItemCombatEffectRequestContribution contribution
                     in request.Contributions)
            {
                ItemCombatEffectRequestCanonical.Field(
                    builder, "contribution.sourceKind",
                    contribution.sourceKind.ToString());
                ItemCombatEffectRequestCanonical.Field(
                    builder, "contribution.sourceEffectId",
                    contribution.sourceEffectId);
                ItemCombatEffectRequestCanonical.Field(
                    builder, "contribution.operation", contribution.operation);
                ItemCombatEffectRequestCanonical.Field(
                    builder, "contribution.targetStatId",
                    contribution.targetStatId);
                ItemCombatEffectRequestCanonical.Field(
                    builder, "contribution.valueUnitKey",
                    contribution.valueUnitKey);
                ItemCombatEffectRequestCanonical.LongField(
                    builder, "contribution.rawUnits", contribution.rawUnits);
                ItemCombatEffectRequestCanonical.IntField(
                    builder, "contribution.stackCount",
                    contribution.stackCount);
                ItemCombatEffectRequestCanonical.LongField(
                    builder, "contribution.appliedBasisPoints",
                    contribution.appliedBasisPoints);
                ItemCombatEffectRequestCanonical.Field(
                    builder, "contribution.disposition",
                    contribution.disposition.ToString());
                ItemCombatEffectRequestCanonical.Field(
                    builder, "contribution.sourceDataMaturity",
                    contribution.sourceDataMaturity);
            }
        }

        private static void AppendTelemetry(
            StringBuilder builder,
            ItemCombatEffectUnsupportedTelemetry telemetry)
        {
            ItemCombatEffectRequestCanonical.OptionalField(
                builder, "telemetry.sourceItemInstanceId",
                telemetry.sourceItemInstanceId);
            ItemCombatEffectRequestCanonical.OptionalField(
                builder, "telemetry.sourceBaseItemId",
                telemetry.sourceBaseItemId);
            ItemCombatEffectRequestCanonical.OptionalField(
                builder, "telemetry.sourcePlacementId",
                telemetry.sourcePlacementId);
            ItemCombatEffectRequestCanonical.Field(
                builder, "telemetry.sourceEffectId", telemetry.sourceEffectId);
            ItemCombatEffectRequestCanonical.Field(
                builder, "telemetry.sourceKind", telemetry.sourceKind.ToString());
            ItemCombatEffectRequestCanonical.Field(
                builder, "telemetry.machineOperation",
                telemetry.machineOperation);
            ItemCombatEffectRequestCanonical.Field(
                builder, "telemetry.machineUnit", telemetry.machineUnit);
            ItemCombatEffectRequestCanonical.Field(
                builder, "telemetry.magnitudePresence",
                telemetry.magnitudePresence.ToString());
            ItemCombatEffectRequestCanonical.NullableLong(
                builder, "telemetry.magnitudeUnits",
                telemetry.magnitudeUnits);
            ItemCombatEffectRequestCanonical.Field(
                builder, "telemetry.disposition",
                telemetry.disposition.ToString());
            ItemCombatEffectRequestCanonical.Field(
                builder, "telemetry.reasonCode", telemetry.reasonCode);
            foreach (string identity in telemetry.SourceIdentities)
            {
                ItemCombatEffectRequestCanonical.Field(
                    builder, "telemetry.sourceIdentity", identity);
            }
        }
    }

    internal static class ItemCombatEffectRequestReadOnly
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
    }

    internal static class ItemCombatEffectRequestCanonical
    {
        public static string Required(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        public static string Optional(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        public static void Field(
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

        public static void OptionalField(
            StringBuilder builder,
            string key,
            string value)
        {
            Field(builder, key + ".presence", value == null ? "Missing" : "Present");
            if (value != null)
            {
                Field(builder, key, value);
            }
        }

        public static void IntField(
            StringBuilder builder,
            string key,
            int value)
        {
            Field(builder, key, value.ToString(CultureInfo.InvariantCulture));
        }

        public static void LongField(
            StringBuilder builder,
            string key,
            long value)
        {
            Field(builder, key, value.ToString(CultureInfo.InvariantCulture));
        }

        public static void NullableLong(
            StringBuilder builder,
            string key,
            long? value)
        {
            Field(builder, key + ".presence",
                value.HasValue ? "Present" : "Missing");
            if (value.HasValue)
            {
                LongField(builder, key, value.Value);
            }
        }

        public static string Sha256(string payload)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] digest =
                    sha.ComputeHash(Encoding.UTF8.GetBytes(payload ?? string.Empty));
                return "sha256:" + BitConverter.ToString(digest)
                    .Replace("-", string.Empty)
                    .ToLowerInvariant();
            }
        }
    }
}
