using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.BattleBridge.CampaignLoot;
using TalismanBag.Contracts.Battle;
using TalismanBag.Items.CampaignBaseline;

namespace TalismanBag.Items.CampaignLoot
{
    public static class C1CampaignPool15CombatFactStatuses
    {
        public const string Supported = "SUPPORTED";
        public const string Unsupported = "UNSUPPORTED";
        public const string AuthoritativeExecuted = "AUTHORITATIVE_EXECUTED";
        public const string AuthoritativeStateOnly = "AUTHORITATIVE_STATE_ONLY";
        public const string Candidate = "CANDIDATE";
        public const string UnknownOrConflicted = "UNKNOWN_OR_CONFLICTED";
        public const string NotExecuted = "NOT_EXECUTED";
    }

    public static class C1CampaignPool15CombatFactDiagnosticCodes
    {
        public const string None = "NONE";
        public const string PoolRejected = "POOL_REJECTED";
        public const string PoolSignatureMismatch = "POOL_SIGNATURE_MISMATCH";
        public const string SourceCatalogSignatureMismatch =
            "SOURCE_CATALOG_SIGNATURE_MISMATCH";
        public const string DefinitionSourceNull = "DEFINITION_SOURCE_NULL";
        public const string DefinitionEnumerationFailed =
            "DEFINITION_ENUMERATION_FAILED";
        public const string RowNull = "ROW_NULL";
        public const string RowCountInvalid = "ROW_COUNT_INVALID";
        public const string IdentityRequired = "IDENTITY_REQUIRED";
        public const string IdentityDuplicate = "IDENTITY_DUPLICATE";
        public const string IdentitySetMismatch = "IDENTITY_SET_MISMATCH";
        public const string FamilyMismatch = "FAMILY_MISMATCH";
        public const string TriggerMismatch = "TRIGGER_MISMATCH";
        public const string ActivationRejected = "ACTIVATION_REJECTED";
        public const string TargetRejected = "TARGET_REJECTED";
        public const string ComponentRequired = "COMPONENT_REQUIRED";
        public const string ComponentDuplicate = "COMPONENT_DUPLICATE";
        public const string AmountRejected = "AMOUNT_REJECTED";
        public const string UnitRejected = "UNIT_REJECTED";
        public const string CapRejected = "CAP_REJECTED";
        public const string CadenceRejected = "CADENCE_REJECTED";
        public const string ProgressOperatorReleaseRequired =
            "PROGRESS_OPERATOR_RELEASE_REQUIRED";
    }

    public sealed class C1CampaignPool15CombatMagnitudeDefinition
    {
        public C1CampaignPool15CombatMagnitudeDefinition(
            string componentId,
            string operationId,
            string targetStatId,
            long signedAmount,
            string nativeUnitId,
            long absoluteCapPerAcceptedTrigger,
            string supportStatus,
            string maturity,
            string dependencyId,
            string sourceLineage)
        {
            this.componentId = componentId ?? string.Empty;
            this.operationId = operationId ?? string.Empty;
            this.targetStatId = targetStatId ?? string.Empty;
            this.signedAmount = signedAmount;
            this.nativeUnitId = nativeUnitId ?? string.Empty;
            this.absoluteCapPerAcceptedTrigger = absoluteCapPerAcceptedTrigger;
            this.supportStatus = supportStatus ?? string.Empty;
            this.maturity = maturity ?? string.Empty;
            this.dependencyId = dependencyId ?? string.Empty;
            this.sourceLineage = sourceLineage ?? string.Empty;
        }

        public string componentId { get; }
        public string operationId { get; }
        public string targetStatId { get; }
        public long signedAmount { get; }
        public string nativeUnitId { get; }
        public long absoluteCapPerAcceptedTrigger { get; }
        public string supportStatus { get; }
        public string maturity { get; }
        public string dependencyId { get; }
        public string sourceLineage { get; }

        public C1CampaignPool15CombatMagnitudeDefinition WithNativeUnit(
            string value)
        {
            return Copy(nativeUnitId: value);
        }

        public C1CampaignPool15CombatMagnitudeDefinition WithAmount(long value)
        {
            return Copy(signedAmount: value);
        }

        public C1CampaignPool15CombatMagnitudeDefinition WithCap(long value)
        {
            return Copy(absoluteCapPerAcceptedTrigger: value);
        }

        internal C1CampaignPool15CombatMagnitudeDefinition Clone()
        {
            return Copy();
        }

        private C1CampaignPool15CombatMagnitudeDefinition Copy(
            string nativeUnitId = null,
            long? signedAmount = null,
            long? absoluteCapPerAcceptedTrigger = null)
        {
            return new C1CampaignPool15CombatMagnitudeDefinition(
                componentId,
                operationId,
                targetStatId,
                signedAmount ?? this.signedAmount,
                nativeUnitId ?? this.nativeUnitId,
                absoluteCapPerAcceptedTrigger ?? this.absoluteCapPerAcceptedTrigger,
                supportStatus,
                maturity,
                dependencyId,
                sourceLineage);
        }
    }

    public sealed class C1CampaignPool15BaseCombatFactDefinition
    {
        private readonly ReadOnlyCollection<
            C1CampaignPool15CombatMagnitudeDefinition> components;

        public C1CampaignPool15BaseCombatFactDefinition(
            string baseItemId,
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
            string sourceProfileCanonicalSignature,
            IEnumerable<C1CampaignPool15CombatMagnitudeDefinition> componentRows)
        {
            this.baseItemId = baseItemId ?? string.Empty;
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
            this.sourceProfileCanonicalSignature =
                sourceProfileCanonicalSignature ?? string.Empty;
            components = Array.AsReadOnly((componentRows
                ?? Enumerable.Empty<C1CampaignPool15CombatMagnitudeDefinition>())
                .Where(row => row != null)
                .Select(row => row.Clone())
                .ToArray());
        }

        public string baseItemId { get; }
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
        public string sourceProfileCanonicalSignature { get; }
        public IReadOnlyList<C1CampaignPool15CombatMagnitudeDefinition> Components =>
            components;

        public C1CampaignPool15BaseCombatFactDefinition WithBaseItemId(string value)
        {
            return Copy(baseItemId: value);
        }

        public C1CampaignPool15BaseCombatFactDefinition WithTriggerId(string value)
        {
            return Copy(triggerId: value);
        }

        public C1CampaignPool15BaseCombatFactDefinition WithComponents(
            IEnumerable<C1CampaignPool15CombatMagnitudeDefinition> rows)
        {
            return Copy(componentRows: rows);
        }

        internal C1CampaignPool15BaseCombatFactDefinition Clone()
        {
            return Copy();
        }

        private C1CampaignPool15BaseCombatFactDefinition Copy(
            string baseItemId = null,
            string triggerId = null,
            IEnumerable<C1CampaignPool15CombatMagnitudeDefinition> componentRows = null)
        {
            return new C1CampaignPool15BaseCombatFactDefinition(
                baseItemId ?? this.baseItemId,
                familyId,
                familyVariantId,
                effectCategoryId,
                triggerId ?? this.triggerId,
                activationConditionId,
                targetScopeId,
                cueIdentity,
                cadenceKind,
                cadenceMilliseconds,
                firstOffsetMilliseconds,
                minimumLayoutTier,
                sourceProfileCanonicalSignature,
                componentRows ?? components);
        }
    }

    public sealed class C1CampaignPool15CombatMagnitudeFact
    {
        internal C1CampaignPool15CombatMagnitudeFact(
            C1CampaignPool15CombatMagnitudeDefinition source)
        {
            componentId = source.componentId;
            operationId = source.operationId;
            targetStatId = source.targetStatId;
            signedAmount = source.signedAmount;
            nativeUnitId = source.nativeUnitId;
            absoluteCapPerAcceptedTrigger = source.absoluteCapPerAcceptedTrigger;
            supportStatus = source.supportStatus;
            maturity = source.maturity;
            dependencyId = source.dependencyId;
            sourceLineage = source.sourceLineage;
            canonicalSignature = C1CampaignPool15BaseCombatCanonical.Hash(
                C1CampaignPool15BaseCombatCanonical.Build(builder =>
                {
                    builder.S("componentId", componentId);
                    builder.S("operationId", operationId);
                    builder.S("targetStatId", targetStatId);
                    builder.L("signedAmount", signedAmount);
                    builder.S("nativeUnitId", nativeUnitId);
                    builder.L("absoluteCapPerAcceptedTrigger",
                        absoluteCapPerAcceptedTrigger);
                    builder.S("supportStatus", supportStatus);
                    builder.S("maturity", maturity);
                    builder.S("dependencyId", dependencyId);
                    builder.S("sourceLineage", sourceLineage);
                }));
        }

        public string componentId { get; }
        public string operationId { get; }
        public string targetStatId { get; }
        public long signedAmount { get; }
        public string nativeUnitId { get; }
        public long absoluteCapPerAcceptedTrigger { get; }
        public string supportStatus { get; }
        public string maturity { get; }
        public string dependencyId { get; }
        public string sourceLineage { get; }
        public string canonicalSignature { get; }

        internal C1CampaignPool15CombatMagnitudeFact Clone()
        {
            return new C1CampaignPool15CombatMagnitudeFact(
                new C1CampaignPool15CombatMagnitudeDefinition(
                    componentId,
                    operationId,
                    targetStatId,
                    signedAmount,
                    nativeUnitId,
                    absoluteCapPerAcceptedTrigger,
                    supportStatus,
                    maturity,
                    dependencyId,
                    sourceLineage));
        }
    }

    public sealed class C1CampaignPool15BaseCombatFact
    {
        private readonly ReadOnlyCollection<C1CampaignPool15CombatMagnitudeFact>
            components;

        internal C1CampaignPool15BaseCombatFact(
            C1CampaignPool15BaseCombatFactDefinition source)
        {
            schemaId = C1CampaignPool15BaseCombatFacts.RowSchemaId;
            baseItemId = source.baseItemId;
            familyId = source.familyId;
            familyVariantId = source.familyVariantId;
            effectCategoryId = source.effectCategoryId;
            triggerId = source.triggerId;
            activationConditionId = source.activationConditionId;
            targetScopeId = source.targetScopeId;
            cueIdentity = source.cueIdentity;
            cadenceKind = source.cadenceKind;
            cadenceMilliseconds = source.cadenceMilliseconds;
            firstOffsetMilliseconds = source.firstOffsetMilliseconds;
            minimumLayoutTier = source.minimumLayoutTier;
            sourceProfileCanonicalSignature = source.sourceProfileCanonicalSignature;
            components = Array.AsReadOnly(source.Components
                .Select(row => new C1CampaignPool15CombatMagnitudeFact(row))
                .ToArray());
            maturity = components.Any(row => string.Equals(
                    row.supportStatus,
                    C1CampaignPool15CombatFactStatuses.Unsupported,
                    StringComparison.Ordinal))
                ? C1CampaignPool15CombatFactStatuses.UnknownOrConflicted
                : components.All(row => string.Equals(
                    row.maturity,
                    C1CampaignPool15CombatFactStatuses.AuthoritativeExecuted,
                    StringComparison.Ordinal))
                    ? C1CampaignPool15CombatFactStatuses.AuthoritativeExecuted
                    : C1CampaignPool15CombatFactStatuses.Candidate;
            canonicalSignature = C1CampaignPool15BaseCombatCanonical.Hash(
                C1CampaignPool15BaseCombatCanonical.Build(builder =>
                {
                    builder.S("schemaId", schemaId);
                    builder.S("baseItemId", baseItemId);
                    builder.S("familyId", familyId);
                    builder.S("familyVariantId", familyVariantId);
                    builder.S("effectCategoryId", effectCategoryId);
                    builder.S("triggerId", triggerId);
                    builder.S("activationConditionId", activationConditionId);
                    builder.S("targetScopeId", targetScopeId);
                    builder.S("cueIdentity", cueIdentity);
                    builder.S("cadenceKind", cadenceKind);
                    builder.N("cadenceMilliseconds", cadenceMilliseconds);
                    builder.N("firstOffsetMilliseconds", firstOffsetMilliseconds);
                    builder.S("minimumLayoutTier", minimumLayoutTier);
                    builder.S("maturity", maturity);
                    builder.S("sourceProfileCanonicalSignature",
                        sourceProfileCanonicalSignature);
                    builder.I("componentCount", components.Count);
                    foreach (C1CampaignPool15CombatMagnitudeFact component in components)
                    {
                        builder.S("componentCanonicalSignature",
                            component.canonicalSignature);
                    }
                }));
        }

        public string schemaId { get; }
        public string baseItemId { get; }
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
        public string maturity { get; }
        public string sourceProfileCanonicalSignature { get; }
        public IReadOnlyList<C1CampaignPool15CombatMagnitudeFact> Components =>
            components;
        public string canonicalSignature { get; }

        public C1CampaignPool15CombatMagnitudeFact FindComponent(string componentId)
        {
            C1CampaignPool15CombatMagnitudeFact row = components.FirstOrDefault(
                candidate => string.Equals(
                    candidate.componentId,
                    componentId,
                    StringComparison.Ordinal));
            return row == null ? null : row.Clone();
        }

        internal C1CampaignPool15BaseCombatFact Clone()
        {
            return new C1CampaignPool15BaseCombatFact(
                new C1CampaignPool15BaseCombatFactDefinition(
                    baseItemId,
                    familyId,
                    familyVariantId,
                    effectCategoryId,
                    triggerId,
                    activationConditionId,
                    targetScopeId,
                    cueIdentity,
                    cadenceKind,
                    cadenceMilliseconds,
                    firstOffsetMilliseconds,
                    minimumLayoutTier,
                    sourceProfileCanonicalSignature,
                    components.Select(row => new
                        C1CampaignPool15CombatMagnitudeDefinition(
                            row.componentId,
                            row.operationId,
                            row.targetStatId,
                            row.signedAmount,
                            row.nativeUnitId,
                            row.absoluteCapPerAcceptedTrigger,
                            row.supportStatus,
                            row.maturity,
                            row.dependencyId,
                            row.sourceLineage))));
        }
    }

    public sealed class C1CampaignPool15DownstreamFactRow
    {
        internal C1CampaignPool15DownstreamFactRow(
            C1CampaignPool15BaseCombatFact fact)
        {
            baseItemId = fact.baseItemId;
            factCanonicalSignature = fact.canonicalSignature;
            supportedComponentIds = Array.AsReadOnly(fact.Components
                .Where(row => row.supportStatus ==
                    C1CampaignPool15CombatFactStatuses.Supported)
                .Select(row => row.componentId)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray());
            candidateComponentIds = Array.AsReadOnly(fact.Components
                .Where(row => row.maturity ==
                    C1CampaignPool15CombatFactStatuses.Candidate)
                .Select(row => row.componentId)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray());
            unsupportedDependencyIds = Array.AsReadOnly(fact.Components
                .Where(row => row.supportStatus ==
                    C1CampaignPool15CombatFactStatuses.Unsupported)
                .Select(row => row.dependencyId)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray());
        }

        public string baseItemId { get; }
        public string factCanonicalSignature { get; }
        public IReadOnlyList<string> supportedComponentIds { get; }
        public IReadOnlyList<string> candidateComponentIds { get; }
        public IReadOnlyList<string> unsupportedDependencyIds { get; }
    }

    public sealed class C1CampaignPool15DownstreamFactInventory
    {
        private readonly ReadOnlyCollection<C1CampaignPool15DownstreamFactRow> rows;

        internal C1CampaignPool15DownstreamFactInventory(
            string owner,
            IEnumerable<C1CampaignPool15BaseCombatFact> facts,
            string sourceSnapshotSignature)
        {
            schemaId = "C1CampaignPool15DownstreamFactInventory.v1";
            this.owner = owner ?? string.Empty;
            sourceLineage = C1CampaignPool15BaseCombatFacts.ProfileId + "/"
                + (sourceSnapshotSignature ?? string.Empty);
            rows = Array.AsReadOnly((facts
                ?? Enumerable.Empty<C1CampaignPool15BaseCombatFact>())
                .OrderBy(row => row.baseItemId, StringComparer.Ordinal)
                .Select(row => new C1CampaignPool15DownstreamFactRow(row))
                .ToArray());
            canonicalSignature = C1CampaignPool15BaseCombatCanonical.Hash(
                C1CampaignPool15BaseCombatCanonical.Build(builder =>
                {
                    builder.S("schemaId", schemaId);
                    builder.S("owner", this.owner);
                    builder.S("sourceLineage", sourceLineage);
                    foreach (C1CampaignPool15DownstreamFactRow row in rows)
                    {
                        builder.S("baseItemId", row.baseItemId);
                        builder.S("factCanonicalSignature",
                            row.factCanonicalSignature);
                        foreach (string value in row.supportedComponentIds)
                        {
                            builder.S("supportedComponentId", value);
                        }
                        foreach (string value in row.candidateComponentIds)
                        {
                            builder.S("candidateComponentId", value);
                        }
                        foreach (string value in row.unsupportedDependencyIds)
                        {
                            builder.S("unsupportedDependencyId", value);
                        }
                    }
                }));
        }

        public string schemaId { get; }
        public string owner { get; }
        public string sourceLineage { get; }
        public IReadOnlyList<C1CampaignPool15DownstreamFactRow> Rows => rows;
        public string canonicalSignature { get; }
    }

    public sealed class C1CampaignPool15BattleProgressOperatorReleaseEvidence
    {
        internal C1CampaignPool15BattleProgressOperatorReleaseEvidence()
        {
            relationSchemaId = C1FormalRealtimeBattleSessionContract
                .ActionProgressRelationSchemaId;
            advanceItemTriggerOperatorId =
                C1FormalRealtimeBattleActionProgressOperatorIds
                    .AdvanceItemTriggerProgressStep;
            delayEnemyCastOperatorId =
                C1FormalRealtimeBattleActionProgressOperatorIds
                    .DelayEnemyCastProgressStep;
            relationCarrierType = typeof(
                C1FormalRealtimeBattleActionProgressRelationSnapshot).FullName;
            mutationCarrierType = typeof(
                C1FormalRealtimeBattleActionProgressMutationSnapshot).FullName;
            cueKind = C1FormalRealtimeBattleCueKinds.ActionProgressMutated;
            mutationLedgerId = "ActionProgressMutationLedger";
            cueLedgerId = "ActionProgressCueLedger";
            nativeUnitId = "turn";
            actionCapPerAcceptedOperator = 1;
            sourceNativeStepCapPerAcceptedOperator = 1;
            deterministicTargetOrder =
                "EARLIEST_DUE_THEN_STABLE_SOURCE_THEN_STABLE_ACTION";
            ownershipStatement =
                "READ_ONLY_REFERENCE; Battle owns action timing, target selection, "
                + "state mutation, mutation ledger and cue ledger.";
            contractSourceSha256 = C1CampaignPool15BaseCombatFacts
                .BattleContractsSourceSha256;
            sessionSourceSha256 = C1CampaignPool15BaseCombatFacts
                .BattleSessionSourceSha256;
            adapterSourceSha256 = C1CampaignPool15BaseCombatFacts
                .BattleAdapterSourceSha256;
            verifierSourceSha256 = C1CampaignPool15BaseCombatFacts
                .BattleVerifierSourceSha256;
            dependencyContractSha256 = C1CampaignPool15BaseCombatFacts
                .BattleDependencyContractSha256;
            accepted = relationSchemaId ==
                    C1CampaignPool15BaseCombatFacts.ProgressRelationSchemaId
                && advanceItemTriggerOperatorId ==
                    C1CampaignPool15BaseCombatFacts
                        .AdvanceItemTriggerProgressStepDependencyId
                && delayEnemyCastOperatorId ==
                    C1CampaignPool15BaseCombatFacts
                        .DelayEnemyCastProgressStepDependencyId
                && relationCarrierType ==
                    "TalismanBag.Contracts.Battle."
                    + "C1FormalRealtimeBattleActionProgressRelationSnapshot"
                && mutationCarrierType ==
                    "TalismanBag.Contracts.Battle."
                    + "C1FormalRealtimeBattleActionProgressMutationSnapshot"
                && cueKind == "ACTION_PROGRESS_MUTATED";
            canonicalSignature = C1CampaignPool15BaseCombatCanonical.Hash(
                C1CampaignPool15BaseCombatCanonical.Build(builder =>
                {
                    builder.S("relationSchemaId", relationSchemaId);
                    builder.S("advanceItemTriggerOperatorId",
                        advanceItemTriggerOperatorId);
                    builder.S("delayEnemyCastOperatorId",
                        delayEnemyCastOperatorId);
                    builder.S("relationCarrierType", relationCarrierType);
                    builder.S("mutationCarrierType", mutationCarrierType);
                    builder.S("cueKind", cueKind);
                    builder.S("mutationLedgerId", mutationLedgerId);
                    builder.S("cueLedgerId", cueLedgerId);
                    builder.S("nativeUnitId", nativeUnitId);
                    builder.I("actionCapPerAcceptedOperator",
                        actionCapPerAcceptedOperator);
                    builder.I("sourceNativeStepCapPerAcceptedOperator",
                        sourceNativeStepCapPerAcceptedOperator);
                    builder.S("deterministicTargetOrder",
                        deterministicTargetOrder);
                    builder.S("ownershipStatement", ownershipStatement);
                    builder.S("contractSourceSha256", contractSourceSha256);
                    builder.S("sessionSourceSha256", sessionSourceSha256);
                    builder.S("adapterSourceSha256", adapterSourceSha256);
                    builder.S("verifierSourceSha256", verifierSourceSha256);
                    builder.S("dependencyContractSha256",
                        dependencyContractSha256);
                }));
        }

        public string relationSchemaId { get; }
        public string advanceItemTriggerOperatorId { get; }
        public string delayEnemyCastOperatorId { get; }
        public string relationCarrierType { get; }
        public string mutationCarrierType { get; }
        public string cueKind { get; }
        public string mutationLedgerId { get; }
        public string cueLedgerId { get; }
        public string nativeUnitId { get; }
        public int actionCapPerAcceptedOperator { get; }
        public int sourceNativeStepCapPerAcceptedOperator { get; }
        public string deterministicTargetOrder { get; }
        public string ownershipStatement { get; }
        public string contractSourceSha256 { get; }
        public string sessionSourceSha256 { get; }
        public string adapterSourceSha256 { get; }
        public string verifierSourceSha256 { get; }
        public string dependencyContractSha256 { get; }
        public bool accepted { get; }
        public string canonicalSignature { get; }
    }

    public sealed class C1CampaignPool15BaseCombatFactSnapshot
    {
        private readonly ReadOnlyCollection<C1CampaignPool15BaseCombatFact> facts;

        internal C1CampaignPool15BaseCombatFactSnapshot(
            C1CampaignLootEligibleItemPool15Snapshot pool,
            IEnumerable<C1CampaignPool15BaseCombatFactDefinition> definitions)
        {
            schemaId = C1CampaignPool15BaseCombatFacts.SnapshotSchemaId;
            profileId = C1CampaignPool15BaseCombatFacts.ProfileId;
            productContext = C1CampaignPool15BaseCombatFacts.ProductContext;
            poolCanonicalSignature = pool.poolCanonicalSignature;
            sourceItemCatalogCanonicalSignature =
                pool.sourceItemCatalogCanonicalSignature;
            facts = Array.AsReadOnly(definitions
                .OrderBy(row => row.baseItemId, StringComparer.Ordinal)
                .Select(row => new C1CampaignPool15BaseCombatFact(row))
                .ToArray());
            battleProgressRelease = new
                C1CampaignPool15BattleProgressOperatorReleaseEvidence();
            progressOperatorReleaseStatus = battleProgressRelease.accepted
                ? C1CampaignPool15CombatFactStatuses.Supported
                : C1CampaignPool15CombatFactDiagnosticCodes
                    .ProgressOperatorReleaseRequired;
            canonicalSignature = C1CampaignPool15BaseCombatCanonical.Hash(
                C1CampaignPool15BaseCombatCanonical.Build(builder =>
                {
                    builder.S("schemaId", schemaId);
                    builder.S("profileId", profileId);
                    builder.S("productContext", productContext);
                    builder.S("poolCanonicalSignature", poolCanonicalSignature);
                    builder.S("sourceItemCatalogCanonicalSignature",
                        sourceItemCatalogCanonicalSignature);
                    builder.S("progressOperatorReleaseStatus",
                        progressOperatorReleaseStatus);
                    builder.S("battleProgressReleaseCanonicalSignature",
                        battleProgressRelease.canonicalSignature);
                    builder.I("factCount", facts.Count);
                    foreach (C1CampaignPool15BaseCombatFact row in facts)
                    {
                        builder.S("factCanonicalSignature",
                            row.canonicalSignature);
                    }
                }));
            enemyInventory = new C1CampaignPool15DownstreamFactInventory(
                "Enemy",
                facts,
                canonicalSignature);
            battleInventory = new C1CampaignPool15DownstreamFactInventory(
                "Battle/Bridge",
                facts,
                canonicalSignature);
        }

        public string schemaId { get; }
        public string profileId { get; }
        public string productContext { get; }
        public string poolCanonicalSignature { get; }
        public string sourceItemCatalogCanonicalSignature { get; }
        public IReadOnlyList<C1CampaignPool15BaseCombatFact> Facts => facts;
        public string progressOperatorReleaseStatus { get; }
        public C1CampaignPool15BattleProgressOperatorReleaseEvidence
            battleProgressRelease { get; }
        public string canonicalSignature { get; }
        public C1CampaignPool15DownstreamFactInventory enemyInventory { get; }
        public C1CampaignPool15DownstreamFactInventory battleInventory { get; }

        public C1CampaignPool15BaseCombatFact Find(string baseItemId)
        {
            C1CampaignPool15BaseCombatFact row = facts.FirstOrDefault(candidate =>
                string.Equals(candidate.baseItemId, baseItemId,
                    StringComparison.Ordinal));
            return row == null ? null : row.Clone();
        }
    }

    public sealed class C1CampaignPool15BaseCombatFactResult
    {
        internal C1CampaignPool15BaseCombatFactResult(
            C1CampaignPool15BaseCombatFactSnapshot snapshot,
            string diagnosticCode,
            string diagnosticMessage)
        {
            this.snapshot = snapshot;
            this.diagnosticCode = diagnosticCode ?? string.Empty;
            this.diagnosticMessage = diagnosticMessage ?? string.Empty;
        }

        public bool accepted => snapshot != null && diagnosticCode ==
            C1CampaignPool15CombatFactDiagnosticCodes.None;
        public C1CampaignPool15BaseCombatFactSnapshot snapshot { get; }
        public string diagnosticCode { get; }
        public string diagnosticMessage { get; }
    }

    public static class C1CampaignPool15BaseCombatFacts
    {
        public const string SnapshotSchemaId =
            "C1CampaignPool15BaseCombatFactSnapshot.v1";
        public const string RowSchemaId = "C1CampaignPool15BaseCombatFact.v1";
        public const string ProfileId =
            "C1_CAMPAIGN_LV1_POOL15_BASE_COMBAT_FACTS_R1";
        public const string ProductContext = "CAMPAIGN_NORMAL_LV1";
        public const string ExpectedPoolCanonicalSignature =
            "572c3421a4874ef203634e64896f59950c5746aa023026fc2503a643b99a47a0";
        public const string ExpectedSourceItemCatalogCanonicalSignature =
            "85e67946c6394a9977238a74c53969190b2472214b67bc15cec4b3f3668b0bdf";
        public const string AdvanceItemTriggerProgressStepDependencyId =
            C1FormalRealtimeBattleActionProgressOperatorIds
                .AdvanceItemTriggerProgressStep;
        public const string DelayEnemyCastProgressStepDependencyId =
            C1FormalRealtimeBattleActionProgressOperatorIds
                .DelayEnemyCastProgressStep;
        public const string ProgressRelationSchemaId =
            C1FormalRealtimeBattleSessionContract.ActionProgressRelationSchemaId;
        public const string BattleContractsSourceSha256 =
            "6D84DB765CDA2CD80A382F694FB852F78AFDBF40FF9277E33830AA7564C38171";
        public const string BattleSessionSourceSha256 =
            "054C60AE15DFDDEB7593E4AD6EC6919F197545B544A0472BA380079F9B1B958E";
        public const string BattleAdapterSourceSha256 =
            "177B77FFBB2A2761D4284FDE1C8BB580EADF7A5E2878EC806DDA954DA5C806FE";
        public const string BattleVerifierSourceSha256 =
            "992CCA6F9352FCC82F731794F60C2130BCDDAE698ABC197492A5963607C71C6D";
        public const string BattleDependencyContractSha256 =
            "2381C67427128BF2D02A1AF10581EC280F5DCB35B501B942BBF8A46531A97918";

        private static readonly HashSet<string> AllowedUnits = new HashSet<string>(
            new[]
            {
                "flat", "point", "stack", "basisPoint", "count",
                "turn"
            },
            StringComparer.Ordinal);

        private static readonly C1CampaignPool15BaseCombatFactResult Resolved =
            TryCreateSnapshot(BuildDefinitions());

        public static C1CampaignPool15BaseCombatFactResult Resolve()
        {
            return Resolved;
        }

        public static IReadOnlyList<C1CampaignPool15BaseCombatFactDefinition>
            CreateValidationFixture()
        {
            return Array.AsReadOnly(BuildDefinitions()
                .Select(row => row.Clone())
                .ToArray());
        }

        public static C1CampaignPool15BaseCombatFactResult TryCreateSnapshot(
            IEnumerable<C1CampaignPool15BaseCombatFactDefinition> source)
        {
            C1CampaignLootEligibleItemPool15Result poolResult =
                C1CampaignLootEligibleItemPool15Carrier.Resolve();
            if (poolResult == null || !poolResult.accepted
                || poolResult.snapshot == null)
            {
                return Failure(
                    C1CampaignPool15CombatFactDiagnosticCodes.PoolRejected,
                    poolResult == null ? "Pool15 carrier returned null."
                        : poolResult.diagnosticCode);
            }

            C1CampaignLootEligibleItemPool15Snapshot pool = poolResult.snapshot;
            if (pool.poolCanonicalSignature != ExpectedPoolCanonicalSignature)
            {
                return Failure(
                    C1CampaignPool15CombatFactDiagnosticCodes
                        .PoolSignatureMismatch,
                    "Pool15 canonical signature drifted.");
            }
            if (pool.sourceItemCatalogCanonicalSignature !=
                ExpectedSourceItemCatalogCanonicalSignature)
            {
                return Failure(
                    C1CampaignPool15CombatFactDiagnosticCodes
                        .SourceCatalogSignatureMismatch,
                    "Source Item catalog canonical signature drifted.");
            }
            C1CampaignPool15BattleProgressOperatorReleaseEvidence release =
                new C1CampaignPool15BattleProgressOperatorReleaseEvidence();
            if (!release.accepted)
            {
                return Failure(
                    C1CampaignPool15CombatFactDiagnosticCodes
                        .ProgressOperatorReleaseRequired,
                    "The released Battle progress operator contract drifted.");
            }
            if (source == null)
            {
                return Failure(
                    C1CampaignPool15CombatFactDiagnosticCodes.DefinitionSourceNull,
                    "Fact definitions are required.");
            }

            C1CampaignPool15BaseCombatFactDefinition[] rows;
            try
            {
                rows = source.ToArray();
            }
            catch (Exception exception)
            {
                return Failure(
                    C1CampaignPool15CombatFactDiagnosticCodes
                        .DefinitionEnumerationFailed,
                    "Fact enumeration failed: " + exception.GetType().Name);
            }
            if (rows.Any(row => row == null))
            {
                return Failure(C1CampaignPool15CombatFactDiagnosticCodes.RowNull,
                    "Fact rows cannot be null.");
            }
            if (rows.Length != 15)
            {
                return Failure(
                    C1CampaignPool15CombatFactDiagnosticCodes.RowCountInvalid,
                    "Exactly fifteen Pool15 fact rows are required.");
            }
            if (rows.Any(row => string.IsNullOrWhiteSpace(row.baseItemId)))
            {
                return Failure(
                    C1CampaignPool15CombatFactDiagnosticCodes.IdentityRequired,
                    "Every fact identity is required.");
            }
            if (rows.Select(row => row.baseItemId)
                .Distinct(StringComparer.Ordinal).Count() != rows.Length)
            {
                return Failure(
                    C1CampaignPool15CombatFactDiagnosticCodes.IdentityDuplicate,
                    "Fact identities must be unique.");
            }
            string[] expectedIds = pool.Profiles.Select(row => row.baseItemId)
                .ToArray();
            string[] actualIds = rows.Select(row => row.baseItemId)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            if (!expectedIds.SequenceEqual(actualIds, StringComparer.Ordinal))
            {
                return Failure(
                    C1CampaignPool15CombatFactDiagnosticCodes.IdentitySetMismatch,
                    "Fact identities must equal the released Pool15 roster.");
            }

            foreach (C1CampaignPool15BaseCombatFactDefinition row in rows)
            {
                C1CampaignLootItemProfileSnapshot profile =
                    pool.FindProfile(row.baseItemId);
                if (profile == null
                    || row.familyId != profile.familyId
                    || row.familyVariantId != profile.familyVariantId
                    || row.effectCategoryId != profile.effectCategoryId)
                {
                    return Failure(
                        C1CampaignPool15CombatFactDiagnosticCodes.FamilyMismatch,
                        row.baseItemId + " family lineage drifted.");
                }
                if (row.triggerId != profile.triggerId)
                {
                    return Failure(
                        C1CampaignPool15CombatFactDiagnosticCodes.TriggerMismatch,
                        row.baseItemId + " trigger drifted.");
                }
                if (row.activationConditionId != profile.conditionId
                    || string.IsNullOrWhiteSpace(row.minimumLayoutTier))
                {
                    return Failure(
                        C1CampaignPool15CombatFactDiagnosticCodes.ActivationRejected,
                        row.baseItemId + " activation legality drifted.");
                }
                if (row.targetScopeId != profile.targetScopeId
                    || string.IsNullOrWhiteSpace(row.cueIdentity))
                {
                    return Failure(
                        C1CampaignPool15CombatFactDiagnosticCodes.TargetRejected,
                        row.baseItemId + " target/cue lineage drifted.");
                }
                if (!ValidateCadence(row))
                {
                    return Failure(
                        C1CampaignPool15CombatFactDiagnosticCodes.CadenceRejected,
                        row.baseItemId + " cadence or first offset is invalid.");
                }
                if (row.Components.Count == 0)
                {
                    return Failure(
                        C1CampaignPool15CombatFactDiagnosticCodes.ComponentRequired,
                        row.baseItemId + " requires a combat component.");
                }
                if (row.Components.Select(component => component.componentId)
                    .Distinct(StringComparer.Ordinal).Count() != row.Components.Count)
                {
                    return Failure(
                        C1CampaignPool15CombatFactDiagnosticCodes.ComponentDuplicate,
                        row.baseItemId + " component identities must be unique.");
                }
                foreach (C1CampaignPool15CombatMagnitudeDefinition component in
                    row.Components)
                {
                    if (string.IsNullOrWhiteSpace(component.componentId)
                        || string.IsNullOrWhiteSpace(component.operationId)
                        || string.IsNullOrWhiteSpace(component.targetStatId))
                    {
                        return Failure(
                            C1CampaignPool15CombatFactDiagnosticCodes
                                .ComponentRequired,
                            row.baseItemId + " component identifiers are required.");
                    }
                    if (component.signedAmount == 0L)
                    {
                        return Failure(
                            C1CampaignPool15CombatFactDiagnosticCodes.AmountRejected,
                            row.baseItemId + " cannot encode unsupported as zero.");
                    }
                    if (!AllowedUnits.Contains(component.nativeUnitId))
                    {
                        return Failure(
                            C1CampaignPool15CombatFactDiagnosticCodes.UnitRejected,
                            row.baseItemId + " native unit is unknown.");
                    }
                    if (component.absoluteCapPerAcceptedTrigger <= 0L
                        || component.absoluteCapPerAcceptedTrigger <
                        Math.Abs(component.signedAmount))
                    {
                        return Failure(
                            C1CampaignPool15CombatFactDiagnosticCodes.CapRejected,
                            row.baseItemId + " per-trigger cap is invalid.");
                    }
                    bool progressDependency = component.dependencyId ==
                        AdvanceItemTriggerProgressStepDependencyId
                        || component.dependencyId ==
                        DelayEnemyCastProgressStepDependencyId;
                    if (progressDependency
                        && (component.nativeUnitId != "turn"
                            || component.supportStatus !=
                            C1CampaignPool15CombatFactStatuses.Supported
                            || component.maturity !=
                            C1CampaignPool15CombatFactStatuses.Candidate
                            || component.signedAmount != -1L
                            || component.absoluteCapPerAcceptedTrigger != 1L))
                    {
                        return Failure(
                            C1CampaignPool15CombatFactDiagnosticCodes.UnitRejected,
                            row.baseItemId
                                + " must reference one released native turn step.");
                    }
                }
            }

            return new C1CampaignPool15BaseCombatFactResult(
                new C1CampaignPool15BaseCombatFactSnapshot(pool, rows),
                C1CampaignPool15CombatFactDiagnosticCodes.None,
                string.Empty);
        }

        private static bool ValidateCadence(
            C1CampaignPool15BaseCombatFactDefinition row)
        {
            if (row.cadenceKind == "PERIODIC_MILLISECONDS")
            {
                return row.cadenceMilliseconds.HasValue
                    && row.cadenceMilliseconds.Value > 0L
                    && row.firstOffsetMilliseconds.HasValue
                    && row.firstOffsetMilliseconds.Value >= 0L;
            }
            return row.cadenceKind == "NATIVE_TRIGGER_EVENT"
                && !row.cadenceMilliseconds.HasValue
                && !row.firstOffsetMilliseconds.HasValue;
        }

        private static C1CampaignPool15BaseCombatFactDefinition[]
            BuildDefinitions()
        {
            C1CampaignLootEligibleItemPool15Snapshot pool =
                C1CampaignLootEligibleItemPool15Carrier.Resolve().snapshot;
            C1CampaignLootPool15BattleCandidateProfileSnapshot candidates =
                C1CampaignLootPool15BattleEffectFamilyCandidateProfiles.Resolve();
            return pool.Profiles.Select(profile =>
            {
                C1CampaignLootPool15BattleCandidateProfile candidate =
                    candidates.Find(profile.baseItemId);
                string layoutTier = MinimumLayoutTier(profile.conditionId);
                List<C1CampaignPool15CombatMagnitudeDefinition> components =
                    new List<C1CampaignPool15CombatMagnitudeDefinition>();
                if (profile.baseItemId == "I002")
                {
                    C1Lv1StarterItemProjectionResult projection =
                        C1Lv1StarterItemBaselineCatalog.TryGetProjection(
                            ProductContext,
                            "I002",
                            "I002@white",
                            C1Lv1StarterItemBaselineCatalog.ProfileRevision);
                    components.Add(M(
                        "DIRECT_DAMAGE",
                        "ApplyDirectDamage",
                        "damage",
                        projection.projection.directDamage,
                        "flat",
                        projection.projection.directDamage,
                        C1CampaignPool15CombatFactStatuses.Supported,
                        C1CampaignPool15CombatFactStatuses.AuthoritativeExecuted,
                        string.Empty,
                        "C1Lv1StarterItemBaseline/"
                            + projection.projection.canonicalSignature));
                    components.Add(M(
                        "NIAN_REFUND",
                        profile.operationId,
                        profile.targetStatId,
                        candidate.amount,
                        candidate.nativeUnitId,
                        candidate.amount,
                        C1CampaignPool15CombatFactStatuses.Supported,
                        C1CampaignPool15CombatFactStatuses.Candidate,
                        string.Empty,
                        "C1CampaignLootPool15BattleEffectFamilyCandidateProfiles/"
                            + candidate.canonicalSignature));
                }
                else
                {
                    bool releasedProgress = profile.baseItemId == "I022"
                        || profile.baseItemId == "I025";
                    long signedAmount = profile.operationId == "ReduceFlat"
                        ? -candidate.amount
                        : candidate.amount;
                    string dependencyId = profile.baseItemId == "I022"
                        ? AdvanceItemTriggerProgressStepDependencyId
                        : profile.baseItemId == "I025"
                            ? DelayEnemyCastProgressStepDependencyId
                            : string.Empty;
                    components.Add(M(
                        ComponentId(profile),
                        profile.operationId,
                        profile.targetStatId,
                        signedAmount,
                        candidate.nativeUnitId,
                        Math.Abs(signedAmount),
                        C1CampaignPool15CombatFactStatuses.Supported,
                        C1CampaignPool15CombatFactStatuses.Candidate,
                        dependencyId,
                        "C1CampaignLootPool15BattleEffectFamilyCandidateProfiles/"
                            + candidate.canonicalSignature
                            + (releasedProgress
                                ? "/C1_FORMAL_BATTLE_ACTION_PROGRESS_RELATION_R1"
                                : string.Empty)));
                }

                bool periodic = profile.baseItemId == "I002";
                return new C1CampaignPool15BaseCombatFactDefinition(
                    profile.baseItemId,
                    profile.familyId,
                    profile.familyVariantId,
                    profile.effectCategoryId,
                    profile.triggerId,
                    profile.conditionId,
                    profile.targetScopeId,
                    profile.cueId,
                    periodic ? "PERIODIC_MILLISECONDS" : "NATIVE_TRIGGER_EVENT",
                    periodic ? 2000L : (long?)null,
                    periodic ? 2000L : (long?)null,
                    layoutTier,
                    profile.profileCanonicalSignature,
                    components);
            }).ToArray();
        }

        private static string MinimumLayoutTier(string conditionId)
        {
            switch (conditionId)
            {
                case "adjacent_lihuo":
                case "horizontal_adjacent":
                case "adjacent_lit_item":
                    return "INTENDED";
                case "within_2_turns":
                case "next_zhenlei_trigger":
                case "is_lit":
                case "is_direct_lit":
                case "water_charge_3":
                case "next_taibai_hit":
                case "next_slash":
                    return "MIDDLE";
                default:
                    return "POOR";
            }
        }

        private static string ComponentId(C1CampaignLootItemProfileSnapshot profile)
        {
            if (profile.baseItemId == "I022")
            {
                return AdvanceItemTriggerProgressStepDependencyId;
            }
            if (profile.baseItemId == "I025")
            {
                return DelayEnemyCastProgressStepDependencyId;
            }
            return profile.targetStatId.ToUpperInvariant() + "_"
                + profile.operationId.ToUpperInvariant();
        }

        private static C1CampaignPool15CombatMagnitudeDefinition M(
            string componentId,
            string operationId,
            string targetStatId,
            long amount,
            string unit,
            long cap,
            string support,
            string maturity,
            string dependency,
            string lineage)
        {
            return new C1CampaignPool15CombatMagnitudeDefinition(
                componentId,
                operationId,
                targetStatId,
                amount,
                unit,
                cap,
                support,
                maturity,
                dependency,
                lineage);
        }

        private static C1CampaignPool15BaseCombatFactResult Failure(
            string code,
            string message)
        {
            return new C1CampaignPool15BaseCombatFactResult(null, code, message);
        }
    }

    internal static class C1CampaignPool15BaseCombatCanonical
    {
        internal static string Build(Action<Builder> append)
        {
            Builder builder = new Builder();
            append(builder);
            return builder.ToString();
        }

        internal static string Hash(string value)
        {
            byte[] bytes = new UTF8Encoding(false).GetBytes(value ?? string.Empty);
            using (SHA256 algorithm = SHA256.Create())
            {
                return string.Concat(algorithm.ComputeHash(bytes).Select(part =>
                    part.ToString("x2", CultureInfo.InvariantCulture)));
            }
        }

        internal sealed class Builder
        {
            private readonly StringBuilder buffer = new StringBuilder();

            internal void S(string key, string value)
            {
                string safe = value ?? string.Empty;
                buffer.Append(key).Append("=s")
                    .Append(Encoding.UTF8.GetByteCount(safe)
                        .ToString(CultureInfo.InvariantCulture))
                    .Append(':').Append(safe).Append(';');
            }

            internal void I(string key, int value)
            {
                buffer.Append(key).Append("=i")
                    .Append(value.ToString(CultureInfo.InvariantCulture)).Append(';');
            }

            internal void L(string key, long value)
            {
                buffer.Append(key).Append("=l")
                    .Append(value.ToString(CultureInfo.InvariantCulture)).Append(';');
            }

            internal void N(string key, long? value)
            {
                if (!value.HasValue)
                {
                    buffer.Append(key).Append("=n;");
                    return;
                }
                L(key, value.Value);
            }

            public override string ToString()
            {
                return buffer.ToString();
            }
        }
    }
}
