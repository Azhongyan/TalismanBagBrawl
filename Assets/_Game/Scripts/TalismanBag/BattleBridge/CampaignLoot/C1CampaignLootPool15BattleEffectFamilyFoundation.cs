using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.Items.CampaignLoot;

namespace TalismanBag.BattleBridge.CampaignLoot
{
    public static class C1CampaignLootPool15BattleDiagnosticCodes
    {
        public const string None = "NONE";
        public const string PoolRejected = "POOL_REJECTED";
        public const string SchemaMismatch = "SCHEMA_MISMATCH";
        public const string ProductContextRejected = "PRODUCT_CONTEXT_REJECTED";
        public const string PoolSignatureMismatch = "POOL_SIGNATURE_MISMATCH";
        public const string SourceCatalogSignatureMismatch =
            "SOURCE_CATALOG_SIGNATURE_MISMATCH";
        public const string IdentitySetMismatch = "IDENTITY_SET_MISMATCH";
        public const string FamilyContractMismatch = "FAMILY_CONTRACT_MISMATCH";
        public const string DescriptorRejected = "DESCRIPTOR_REJECTED";
        public const string NumericProfilePending = "NUMERIC_PROFILE_PENDING";
        public const string AmountRejected = "AMOUNT_REJECTED";
        public const string TriggerRejected = "TRIGGER_REJECTED";
        public const string ConditionRejected = "CONDITION_REJECTED";
        public const string OperationRejected = "OPERATION_REJECTED";
        public const string TargetRejected = "TARGET_REJECTED";
        public const string UnitRejected = "UNIT_REJECTED";
        public const string SourceUnknown = "SOURCE_UNKNOWN";
        public const string SourceDuplicate = "SOURCE_DUPLICATE";
        public const string EventDuplicate = "EVENT_DUPLICATE";
        public const string EventIdRequired = "EVENT_ID_REQUIRED";
        public const string SourceRequired = "SOURCE_REQUIRED";
        public const string BattleTimeRejected = "BATTLE_TIME_REJECTED";
        public const string FixedPointOverflow = "FIXED_POINT_OVERFLOW";
    }

    public sealed class C1CampaignLootPool15BattleFoundationResult
    {
        internal C1CampaignLootPool15BattleFoundationResult(
            C1CampaignLootPool15BattleEffectFamilyFoundation foundation,
            string diagnosticCode,
            string diagnosticMessage)
        {
            this.foundation = foundation;
            this.diagnosticCode = diagnosticCode ?? string.Empty;
            this.diagnosticMessage = diagnosticMessage ?? string.Empty;
        }

        public bool accepted => foundation != null
            && string.Equals(
                diagnosticCode,
                C1CampaignLootPool15BattleDiagnosticCodes.None,
                StringComparison.Ordinal);
        public C1CampaignLootPool15BattleEffectFamilyFoundation foundation { get; }
        public string diagnosticCode { get; }
        public string diagnosticMessage { get; }
    }

    public sealed class C1CampaignLootPool15BattleSourceRequest
    {
        public C1CampaignLootPool15BattleSourceRequest(
            string sourceItemInstanceId,
            string sourceFactCanonicalSignature,
            string baseItemId,
            string targetScopeId,
            string targetIdentity,
            long? percentageBaseValueUnits)
        {
            this.sourceItemInstanceId = sourceItemInstanceId ?? string.Empty;
            this.sourceFactCanonicalSignature =
                sourceFactCanonicalSignature ?? string.Empty;
            this.baseItemId = baseItemId ?? string.Empty;
            this.targetScopeId = targetScopeId ?? string.Empty;
            this.targetIdentity = targetIdentity ?? string.Empty;
            this.percentageBaseValueUnits = percentageBaseValueUnits;
        }

        public string sourceItemInstanceId { get; }
        public string sourceFactCanonicalSignature { get; }
        public string baseItemId { get; }
        public string targetScopeId { get; }
        public string targetIdentity { get; }
        public long? percentageBaseValueUnits { get; }

        internal C1CampaignLootPool15BattleSourceRequest Clone()
        {
            return new C1CampaignLootPool15BattleSourceRequest(
                sourceItemInstanceId,
                sourceFactCanonicalSignature,
                baseItemId,
                targetScopeId,
                targetIdentity,
                percentageBaseValueUnits);
        }
    }

    public sealed class C1CampaignLootPool15BattleExecutionRequest
    {
        private readonly ReadOnlyCollection<string> satisfiedConditionIds;
        private readonly ReadOnlyCollection<C1CampaignLootPool15BattleSourceRequest>
            sources;

        public C1CampaignLootPool15BattleExecutionRequest(
            string eventId,
            string triggerId,
            long baseBattleTimeMs,
            IEnumerable<string> conditionIds,
            IEnumerable<C1CampaignLootPool15BattleSourceRequest> sourceRows)
        {
            this.eventId = eventId ?? string.Empty;
            this.triggerId = triggerId ?? string.Empty;
            this.baseBattleTimeMs = baseBattleTimeMs;
            satisfiedConditionIds = Array.AsReadOnly((conditionIds
                ?? Enumerable.Empty<string>())
                .Select(value => value ?? string.Empty)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray());
            sources = Array.AsReadOnly((sourceRows
                ?? Enumerable.Empty<C1CampaignLootPool15BattleSourceRequest>())
                .Where(row => row != null)
                .Select(row => row.Clone())
                .ToArray());
        }

        public string eventId { get; }
        public string triggerId { get; }
        public long baseBattleTimeMs { get; }
        public IReadOnlyList<string> SatisfiedConditionIds => satisfiedConditionIds;
        public IReadOnlyList<C1CampaignLootPool15BattleSourceRequest> Sources => sources;
    }

    public sealed class C1CampaignLootPool15BattleEffectRejection
    {
        internal C1CampaignLootPool15BattleEffectRejection(
            string eventId,
            string sourceBaseItemId,
            string diagnosticCode,
            string diagnosticMessage)
        {
            this.eventId = eventId ?? string.Empty;
            this.sourceBaseItemId = sourceBaseItemId ?? string.Empty;
            this.diagnosticCode = diagnosticCode ?? string.Empty;
            this.diagnosticMessage = diagnosticMessage ?? string.Empty;
            canonicalSignature = C1CampaignLootPool15BattleCanonical.Hash(
                C1CampaignLootPool15BattleCanonical.Build(builder =>
                {
                    builder.AddString("eventId", this.eventId);
                    builder.AddString("sourceBaseItemId", this.sourceBaseItemId);
                    builder.AddString("diagnosticCode", this.diagnosticCode);
                    builder.AddString("diagnosticMessage", this.diagnosticMessage);
                }));
        }

        public string eventId { get; }
        public string sourceBaseItemId { get; }
        public string diagnosticCode { get; }
        public string diagnosticMessage { get; }
        public string canonicalSignature { get; }

        internal C1CampaignLootPool15BattleEffectRejection Clone()
        {
            return new C1CampaignLootPool15BattleEffectRejection(
                eventId,
                sourceBaseItemId,
                diagnosticCode,
                diagnosticMessage);
        }
    }

    public sealed class C1CampaignLootPool15BattleEffectApplication
    {
        internal C1CampaignLootPool15BattleEffectApplication(
            long sequence,
            string eventId,
            string sourceItemInstanceId,
            string sourceFactCanonicalSignature,
            string sourceBaseItemId,
            string familyId,
            string familyVariantId,
            string triggerId,
            string conditionId,
            string operationId,
            string targetStatId,
            string targetScopeId,
            string targetIdentity,
            long requestedAmount,
            long appliedAmount,
            string nativeUnitId,
            long triggerBattleTimeMs,
            long resolveBattleTimeMs,
            string cueIdentity)
        {
            this.sequence = sequence;
            this.eventId = eventId ?? string.Empty;
            this.sourceItemInstanceId = sourceItemInstanceId ?? string.Empty;
            this.sourceFactCanonicalSignature =
                sourceFactCanonicalSignature ?? string.Empty;
            this.sourceBaseItemId = sourceBaseItemId ?? string.Empty;
            this.familyId = familyId ?? string.Empty;
            this.familyVariantId = familyVariantId ?? string.Empty;
            this.triggerId = triggerId ?? string.Empty;
            this.conditionId = conditionId ?? string.Empty;
            this.operationId = operationId ?? string.Empty;
            this.targetStatId = targetStatId ?? string.Empty;
            this.targetScopeId = targetScopeId ?? string.Empty;
            this.targetIdentity = targetIdentity ?? string.Empty;
            this.requestedAmount = requestedAmount;
            this.appliedAmount = appliedAmount;
            this.nativeUnitId = nativeUnitId ?? string.Empty;
            this.triggerBattleTimeMs = triggerBattleTimeMs;
            this.resolveBattleTimeMs = resolveBattleTimeMs;
            this.cueIdentity = cueIdentity ?? string.Empty;
            canonicalSignature = C1CampaignLootPool15BattleCanonical.Hash(
                C1CampaignLootPool15BattleCanonical.Build(builder =>
                {
                    builder.AddLong("sequence", this.sequence);
                    builder.AddString("eventId", this.eventId);
                    builder.AddString(
                        "sourceItemInstanceId",
                        this.sourceItemInstanceId);
                    builder.AddString(
                        "sourceFactCanonicalSignature",
                        this.sourceFactCanonicalSignature);
                    builder.AddString("sourceBaseItemId", this.sourceBaseItemId);
                    builder.AddString("familyId", this.familyId);
                    builder.AddString("familyVariantId", this.familyVariantId);
                    builder.AddString("triggerId", this.triggerId);
                    builder.AddString("conditionId", this.conditionId);
                    builder.AddString("operationId", this.operationId);
                    builder.AddString("targetStatId", this.targetStatId);
                    builder.AddString("targetScopeId", this.targetScopeId);
                    builder.AddString("targetIdentity", this.targetIdentity);
                    builder.AddLong("requestedAmount", this.requestedAmount);
                    builder.AddLong("appliedAmount", this.appliedAmount);
                    builder.AddString("nativeUnitId", this.nativeUnitId);
                    builder.AddLong("triggerBattleTimeMs", this.triggerBattleTimeMs);
                    builder.AddLong("resolveBattleTimeMs", this.resolveBattleTimeMs);
                    builder.AddString("cueIdentity", this.cueIdentity);
                }));
        }

        public long sequence { get; }
        public string eventId { get; }
        public string sourceItemInstanceId { get; }
        public string sourceFactCanonicalSignature { get; }
        public string sourceBaseItemId { get; }
        public string familyId { get; }
        public string familyVariantId { get; }
        public string triggerId { get; }
        public string conditionId { get; }
        public string operationId { get; }
        public string targetStatId { get; }
        public string targetScopeId { get; }
        public string targetIdentity { get; }
        public long requestedAmount { get; }
        public long appliedAmount { get; }
        public string nativeUnitId { get; }
        public long triggerBattleTimeMs { get; }
        public long resolveBattleTimeMs { get; }
        public string cueIdentity { get; }
        public string canonicalSignature { get; }

        internal C1CampaignLootPool15BattleEffectApplication Clone()
        {
            return new C1CampaignLootPool15BattleEffectApplication(
                sequence,
                eventId,
                sourceItemInstanceId,
                sourceFactCanonicalSignature,
                sourceBaseItemId,
                familyId,
                familyVariantId,
                triggerId,
                conditionId,
                operationId,
                targetStatId,
                targetScopeId,
                targetIdentity,
                requestedAmount,
                appliedAmount,
                nativeUnitId,
                triggerBattleTimeMs,
                resolveBattleTimeMs,
                cueIdentity);
        }
    }

    public sealed class C1CampaignLootPool15BattleExecutionResult
    {
        private readonly ReadOnlyCollection<
            C1CampaignLootPool15BattleEffectApplication> applications;
        private readonly ReadOnlyCollection<C1CampaignLootPool15BattleEffectRejection>
            rejections;

        internal C1CampaignLootPool15BattleExecutionResult(
            string eventId,
            IEnumerable<C1CampaignLootPool15BattleEffectApplication> acceptedRows,
            IEnumerable<C1CampaignLootPool15BattleEffectRejection> rejectedRows)
        {
            this.eventId = eventId ?? string.Empty;
            applications = Array.AsReadOnly((acceptedRows
                ?? Enumerable.Empty<C1CampaignLootPool15BattleEffectApplication>())
                .Where(row => row != null)
                .Select(row => row.Clone())
                .ToArray());
            rejections = Array.AsReadOnly((rejectedRows
                ?? Enumerable.Empty<C1CampaignLootPool15BattleEffectRejection>())
                .Where(row => row != null)
                .Select(row => row.Clone())
                .ToArray());
            canonicalSignature = C1CampaignLootPool15BattleCanonical.Hash(
                C1CampaignLootPool15BattleCanonical.Build(builder =>
                {
                    builder.AddString("eventId", this.eventId);
                    builder.AddInt("applicationCount", applications.Count);
                    foreach (C1CampaignLootPool15BattleEffectApplication row in applications)
                    {
                        builder.AddString("applicationCanonicalSignature",
                            row.canonicalSignature);
                    }
                    builder.AddInt("rejectionCount", rejections.Count);
                    foreach (C1CampaignLootPool15BattleEffectRejection row in rejections)
                    {
                        builder.AddString("rejectionCanonicalSignature",
                            row.canonicalSignature);
                    }
                }));
        }

        public bool accepted => applications.Count > 0 && rejections.Count == 0;
        public string eventId { get; }
        public IReadOnlyList<C1CampaignLootPool15BattleEffectApplication> Applications =>
            applications;
        public IReadOnlyList<C1CampaignLootPool15BattleEffectRejection> Rejections =>
            rejections;
        public string canonicalSignature { get; }
    }

    public static class C1CampaignLootPool15BattleFixedPoint
    {
        public const long BasisPointDivisor = 10000L;
        public const string RoundingMode = "HALF_AWAY_FROM_ZERO";

        public static bool TryApplyBasisPoints(
            long baseValueUnits,
            long basisPoints,
            out long appliedUnits)
        {
            try
            {
                long product = checked(baseValueUnits * basisPoints);
                long quotient = product / BasisPointDivisor;
                long remainder = product % BasisPointDivisor;
                long absoluteRemainder = Math.Abs(remainder);
                if (checked(absoluteRemainder * 2L) >= BasisPointDivisor)
                {
                    quotient = checked(quotient + Math.Sign(product));
                }

                appliedUnits = quotient;
                return true;
            }
            catch (OverflowException)
            {
                appliedUnits = 0L;
                return false;
            }
        }
    }

    public sealed class C1CampaignLootPool15BattleEffectFamilyFoundation
    {
        public const string SchemaId =
            "C1CampaignLootPool15BattleEffectFamilyFoundation.v1";
        public const string ProductContext = "CAMPAIGN_NORMAL_LV1";
        public const int SourceStaggerMs = 120;
        public const string FormalIntegrationStatus = "NOT_INTEGRATED";

        private readonly Dictionary<string, C1CampaignLootItemProfileSnapshot>
            descriptors;
        private readonly C1CampaignLootPool15BattleCandidateProfileSnapshot
            candidateProfiles;
        private readonly Dictionary<string, IFamilyExecutor> familyExecutors;
        private readonly HashSet<string> acceptedEventSources =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly List<C1CampaignLootPool15BattleEffectApplication> ledger =
            new List<C1CampaignLootPool15BattleEffectApplication>();

        private C1CampaignLootPool15BattleEffectFamilyFoundation(
            C1CampaignLootEligibleItemPool15Snapshot snapshot,
            C1CampaignLootPool15BattleCandidateProfileSnapshot profiles)
        {
            poolCanonicalSignature = snapshot.poolCanonicalSignature;
            sourceItemCatalogCanonicalSignature =
                snapshot.sourceItemCatalogCanonicalSignature;
            candidateProfiles = profiles.Clone();
            descriptors = snapshot.Profiles.ToDictionary(
                row => row.baseItemId,
                row => snapshot.FindProfile(row.baseItemId),
                StringComparer.Ordinal);
            familyExecutors = new Dictionary<string, IFamilyExecutor>(
                StringComparer.Ordinal)
            {
                { "DIRECT_TEMPO", new DirectTempoExecutor() },
                { "TARGET_SPREAD", new TargetSpreadExecutor() },
                { "GUARD_SUSTAIN", new GuardSustainExecutor() },
                { "ENEMY_CONTROL", new EnemyControlExecutor() },
                { "POSITIONAL_ADJACENCY", new PositionalAdjacencyExecutor() }
            };
            canonicalSignature = C1CampaignLootPool15BattleCanonical.Hash(
                C1CampaignLootPool15BattleCanonical.Build(builder =>
                {
                    builder.AddString("schemaId", SchemaId);
                    builder.AddString("productContext", ProductContext);
                    builder.AddString("poolCanonicalSignature", poolCanonicalSignature);
                    builder.AddString("sourceItemCatalogCanonicalSignature",
                        sourceItemCatalogCanonicalSignature);
                    builder.AddString("candidateProfileCanonicalSignature",
                        candidateProfiles.canonicalSignature);
                    builder.AddInt("sourceStaggerMs", SourceStaggerMs);
                    builder.AddString("fixedPointRoundingMode",
                        C1CampaignLootPool15BattleFixedPoint.RoundingMode);
                    builder.AddString("formalIntegrationStatus",
                        FormalIntegrationStatus);
                    foreach (string familyId in familyExecutors.Keys.OrderBy(
                                 value => value,
                                 StringComparer.Ordinal))
                    {
                        builder.AddString("familyExecutor", familyId);
                    }
                }));
        }

        public string poolCanonicalSignature { get; }
        public string sourceItemCatalogCanonicalSignature { get; }
        public string canonicalSignature { get; }
        public int familyExecutorCount => familyExecutors.Count;
        public int descriptorCount => descriptors.Count;

        public static C1CampaignLootPool15BattleFoundationResult TryCreate()
        {
            C1CampaignLootEligibleItemPool15Result pool =
                C1CampaignLootEligibleItemPool15Carrier.Resolve();
            if (pool == null || !pool.accepted || pool.snapshot == null)
            {
                return Failure(
                    C1CampaignLootPool15BattleDiagnosticCodes.PoolRejected,
                    pool == null ? "Pool15 carrier returned null." : pool.diagnosticCode);
            }

            return TryCreate(pool.snapshot);
        }

        public static C1CampaignLootPool15BattleFoundationResult TryCreate(
            C1CampaignLootEligibleItemPool15Snapshot snapshot)
        {
            if (snapshot == null)
            {
                return Failure(
                    C1CampaignLootPool15BattleDiagnosticCodes.PoolRejected,
                    "Pool15 snapshot is required.");
            }
            if (!string.Equals(
                    snapshot.schemaId,
                    C1CampaignLootEligibleItemPool15Carrier.PoolSchemaId,
                    StringComparison.Ordinal))
            {
                return Failure(
                    C1CampaignLootPool15BattleDiagnosticCodes.SchemaMismatch,
                    "Pool15 schema is unsupported.");
            }
            if (!string.Equals(
                    snapshot.productContext,
                    ProductContext,
                    StringComparison.Ordinal))
            {
                return Failure(
                    C1CampaignLootPool15BattleDiagnosticCodes.ProductContextRejected,
                    "Only CAMPAIGN_NORMAL_LV1 is accepted.");
            }
            if (!string.Equals(
                    snapshot.poolCanonicalSignature,
                    C1CampaignLootPool15BattleEffectFamilyCandidateProfiles
                        .ExpectedPoolCanonicalSignature,
                    StringComparison.Ordinal))
            {
                return Failure(
                    C1CampaignLootPool15BattleDiagnosticCodes.PoolSignatureMismatch,
                    "Pool15 canonical signature drifted.");
            }
            if (!string.Equals(
                    snapshot.sourceItemCatalogCanonicalSignature,
                    C1CampaignLootPool15BattleEffectFamilyCandidateProfiles
                        .ExpectedSourceCatalogCanonicalSignature,
                    StringComparison.Ordinal))
            {
                return Failure(
                    C1CampaignLootPool15BattleDiagnosticCodes
                        .SourceCatalogSignatureMismatch,
                    "Source Item catalog canonical signature drifted.");
            }

            C1CampaignLootPool15BattleCandidateProfileSnapshot candidates =
                C1CampaignLootPool15BattleEffectFamilyCandidateProfiles.Resolve();
            string[] expectedIds = candidates.Profiles
                .Select(row => row.baseItemId)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            string[] actualIds = snapshot.Profiles
                .Select(row => row.baseItemId)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            if (expectedIds.Length != 15
                || actualIds.Length != 15
                || !expectedIds.SequenceEqual(actualIds, StringComparer.Ordinal))
            {
                return Failure(
                    C1CampaignLootPool15BattleDiagnosticCodes.IdentitySetMismatch,
                    "Pool15 must expose the exact fifteen candidate identities.");
            }

            string[] expectedFamilies = candidates.Profiles
                .Select(row => row.familyId)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            string[] actualFamilies = snapshot.Families
                .Select(row => row.familyId)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            bool familyShapeAccepted = expectedFamilies.Length == 5
                && actualFamilies.Length == 5
                && expectedFamilies.SequenceEqual(actualFamilies, StringComparer.Ordinal)
                && snapshot.Families.All(row => row.OrderedBaseItemIds.Count == 3
                    && row.OrderedVariantIds.Count == 3
                    && row.OrderedVariantIds.Distinct(StringComparer.Ordinal).Count() == 3);
            if (!familyShapeAccepted)
            {
                return Failure(
                    C1CampaignLootPool15BattleDiagnosticCodes.FamilyContractMismatch,
                    "Pool15 must expose five families and three variants per family.");
            }

            foreach (C1CampaignLootItemProfileSnapshot descriptor in snapshot.Profiles)
            {
                C1CampaignLootPool15BattleCandidateProfile candidate =
                    candidates.Find(descriptor.baseItemId);
                if (!TryValidateDescriptor(
                        descriptor,
                        candidate,
                        out string diagnosticCode,
                        out string diagnosticMessage))
                {
                    return Failure(diagnosticCode, diagnosticMessage);
                }
                C1CampaignLootEffectFamilyDescriptor family = snapshot.Families
                    .FirstOrDefault(row => string.Equals(
                        row.familyId,
                        descriptor.familyId,
                        StringComparison.Ordinal));
                if (family == null
                    || !family.OrderedBaseItemIds.Contains(descriptor.baseItemId))
                {
                    return Failure(
                        C1CampaignLootPool15BattleDiagnosticCodes
                            .FamilyContractMismatch,
                        "Descriptor family membership does not match Pool15 truth.");
                }
            }

            return new C1CampaignLootPool15BattleFoundationResult(
                new C1CampaignLootPool15BattleEffectFamilyFoundation(
                    snapshot,
                    candidates),
                C1CampaignLootPool15BattleDiagnosticCodes.None,
                string.Empty);
        }

        public static bool TryValidateDescriptor(
            C1CampaignLootItemProfileSnapshot descriptor,
            C1CampaignLootPool15BattleCandidateProfile candidate,
            out string diagnosticCode,
            out string diagnosticMessage)
        {
            if (descriptor == null)
            {
                diagnosticCode =
                    C1CampaignLootPool15BattleDiagnosticCodes.DescriptorRejected;
                diagnosticMessage = "Item effect descriptor is required.";
                return false;
            }
            if (candidate == null)
            {
                diagnosticCode =
                    C1CampaignLootPool15BattleDiagnosticCodes.NumericProfilePending;
                diagnosticMessage = "Battle candidate numeric profile is missing.";
                return false;
            }
            if (candidate.amount <= 0L)
            {
                diagnosticCode =
                    C1CampaignLootPool15BattleDiagnosticCodes.AmountRejected;
                diagnosticMessage = "Candidate amount must remain positive and explicit.";
                return false;
            }

            C1CampaignLootEligibleItemPool15Result released =
                C1CampaignLootEligibleItemPool15Carrier.Resolve();
            C1CampaignLootItemProfileSnapshot authoritative = released != null
                && released.accepted
                && released.snapshot != null
                ? released.snapshot.FindProfile(descriptor.baseItemId)
                : null;
            if (authoritative == null)
            {
                diagnosticCode =
                    C1CampaignLootPool15BattleDiagnosticCodes.SourceUnknown;
                diagnosticMessage = "Descriptor identity is not in released Pool15 truth.";
                return false;
            }
            if (!EqualsOrdinal(descriptor.triggerId, authoritative.triggerId))
            {
                diagnosticCode =
                    C1CampaignLootPool15BattleDiagnosticCodes.TriggerRejected;
                diagnosticMessage = "Descriptor trigger is missing or unsupported.";
                return false;
            }
            if (!EqualsOrdinal(descriptor.conditionId, authoritative.conditionId))
            {
                diagnosticCode =
                    C1CampaignLootPool15BattleDiagnosticCodes.ConditionRejected;
                diagnosticMessage = "Descriptor condition is missing or unsupported.";
                return false;
            }
            if (!EqualsOrdinal(descriptor.operationId, authoritative.operationId))
            {
                diagnosticCode =
                    C1CampaignLootPool15BattleDiagnosticCodes.OperationRejected;
                diagnosticMessage = "Descriptor operation is missing or unsupported.";
                return false;
            }
            if (!EqualsOrdinal(descriptor.targetStatId, authoritative.targetStatId)
                || !EqualsOrdinal(descriptor.targetScopeId, authoritative.targetScopeId))
            {
                diagnosticCode =
                    C1CampaignLootPool15BattleDiagnosticCodes.TargetRejected;
                diagnosticMessage = "Descriptor target is missing or unsupported.";
                return false;
            }
            if (!EqualsOrdinal(descriptor.nativeUnitId, authoritative.nativeUnitId)
                || !EqualsOrdinal(candidate.nativeUnitId, descriptor.nativeUnitId))
            {
                diagnosticCode =
                    C1CampaignLootPool15BattleDiagnosticCodes.UnitRejected;
                diagnosticMessage = "Descriptor or candidate native unit is unsupported.";
                return false;
            }
            if (!EqualsOrdinal(candidate.baseItemId, descriptor.baseItemId)
                || !EqualsOrdinal(candidate.familyId, descriptor.familyId)
                || !EqualsOrdinal(descriptor.familyId, authoritative.familyId)
                || !EqualsOrdinal(
                    descriptor.familyVariantId,
                    authoritative.familyVariantId)
                || !EqualsOrdinal(descriptor.cueId, authoritative.cueId))
            {
                diagnosticCode =
                    C1CampaignLootPool15BattleDiagnosticCodes.DescriptorRejected;
                diagnosticMessage = "Descriptor identity, family, variant or cue drifted.";
                return false;
            }

            diagnosticCode = C1CampaignLootPool15BattleDiagnosticCodes.None;
            diagnosticMessage = string.Empty;
            return true;
        }

        public C1CampaignLootPool15BattleExecutionResult Execute(
            C1CampaignLootPool15BattleExecutionRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.eventId))
            {
                return RejectedResult(
                    request == null ? string.Empty : request.eventId,
                    string.Empty,
                    C1CampaignLootPool15BattleDiagnosticCodes.EventIdRequired,
                    "eventId is required.");
            }
            if (request.baseBattleTimeMs < 0L)
            {
                return RejectedResult(
                    request.eventId,
                    string.Empty,
                    C1CampaignLootPool15BattleDiagnosticCodes.BattleTimeRejected,
                    "baseBattleTimeMs cannot be negative.");
            }
            if (request.Sources.Count == 0)
            {
                return RejectedResult(
                    request.eventId,
                    string.Empty,
                    C1CampaignLootPool15BattleDiagnosticCodes.SourceRequired,
                    "At least one source Item is required.");
            }

            string duplicateSource = request.Sources
                .GroupBy(
                    row => row.sourceItemInstanceId,
                    StringComparer.Ordinal)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .OrderBy(value => value, StringComparer.Ordinal)
                .FirstOrDefault();
            if (duplicateSource != null)
            {
                return RejectedResult(
                    request.eventId,
                    duplicateSource,
                    C1CampaignLootPool15BattleDiagnosticCodes.SourceDuplicate,
                    "A source Item may appear only once per event.");
            }

            List<PendingApplication> pending = new List<PendingApplication>();
            foreach (C1CampaignLootPool15BattleSourceRequest source in request.Sources
                         .OrderBy(
                             row => row.sourceItemInstanceId,
                             StringComparer.Ordinal)
                         .ThenBy(row => row.baseItemId, StringComparer.Ordinal))
            {
                if (string.IsNullOrWhiteSpace(source.sourceItemInstanceId)
                    || string.IsNullOrWhiteSpace(
                        source.sourceFactCanonicalSignature))
                {
                    return RejectedResult(
                        request.eventId,
                        source.baseItemId,
                        C1CampaignLootPool15BattleDiagnosticCodes.SourceRequired,
                        "Source Item instance identity and canonical fact are required.");
                }
                if (!descriptors.TryGetValue(
                        source.baseItemId,
                        out C1CampaignLootItemProfileSnapshot descriptor))
                {
                    return RejectedResult(
                        request.eventId,
                        source.baseItemId,
                        C1CampaignLootPool15BattleDiagnosticCodes.SourceUnknown,
                        "Source Item is not in released Pool15 truth.");
                }
                if (!EqualsOrdinal(request.triggerId, descriptor.triggerId))
                {
                    return RejectedResult(
                        request.eventId,
                        source.baseItemId,
                        C1CampaignLootPool15BattleDiagnosticCodes.TriggerRejected,
                        "Request trigger does not match the immutable descriptor.");
                }
                if (!request.SatisfiedConditionIds.Contains(
                        descriptor.conditionId,
                        StringComparer.Ordinal))
                {
                    return RejectedResult(
                        request.eventId,
                        source.baseItemId,
                        C1CampaignLootPool15BattleDiagnosticCodes.ConditionRejected,
                        "Required immutable descriptor condition is not satisfied.");
                }
                if (string.IsNullOrWhiteSpace(source.targetIdentity)
                    || !EqualsOrdinal(source.targetScopeId, descriptor.targetScopeId))
                {
                    return RejectedResult(
                        request.eventId,
                        source.baseItemId,
                        C1CampaignLootPool15BattleDiagnosticCodes.TargetRejected,
                        "Target identity or target scope is missing or unsupported.");
                }

                string eventSourceKey = EventSourceKey(
                    request.eventId,
                    source.sourceItemInstanceId);
                if (acceptedEventSources.Contains(eventSourceKey))
                {
                    return RejectedResult(
                        request.eventId,
                        source.baseItemId,
                        C1CampaignLootPool15BattleDiagnosticCodes.EventDuplicate,
                        "This event/source pair was already accepted.");
                }

                C1CampaignLootPool15BattleCandidateProfile candidate =
                    candidateProfiles.Find(source.baseItemId);
                if (!TryValidateDescriptor(
                        descriptor,
                        candidate,
                        out string descriptorCode,
                        out string descriptorMessage))
                {
                    return RejectedResult(
                        request.eventId,
                        source.baseItemId,
                        descriptorCode,
                        descriptorMessage);
                }
                if (!familyExecutors.TryGetValue(
                        descriptor.familyId,
                        out IFamilyExecutor executor))
                {
                    return RejectedResult(
                        request.eventId,
                        source.baseItemId,
                        C1CampaignLootPool15BattleDiagnosticCodes
                            .FamilyContractMismatch,
                        "No shared executor exists for the descriptor family.");
                }
                if (!executor.TryApply(
                        descriptor,
                        candidate,
                        source,
                        out long appliedAmount,
                        out string applyCode,
                        out string applyMessage))
                {
                    return RejectedResult(
                        request.eventId,
                        source.baseItemId,
                        applyCode,
                        applyMessage);
                }

                pending.Add(new PendingApplication(
                    descriptor,
                    candidate,
                    source,
                    appliedAmount));
            }

            List<C1CampaignLootPool15BattleEffectApplication> accepted =
                new List<C1CampaignLootPool15BattleEffectApplication>();
            for (int sourceOrdinal = 0; sourceOrdinal < pending.Count; sourceOrdinal++)
            {
                PendingApplication row = pending[sourceOrdinal];
                long resolveTime;
                try
                {
                    resolveTime = checked(
                        request.baseBattleTimeMs + sourceOrdinal * SourceStaggerMs);
                }
                catch (OverflowException)
                {
                    return RejectedResult(
                        request.eventId,
                        row.Descriptor.baseItemId,
                        C1CampaignLootPool15BattleDiagnosticCodes.BattleTimeRejected,
                        "Source-level stagger overflowed battle time.");
                }

                long sequence = ledger.Count + accepted.Count + 1L;
                string cueIdentity = row.Descriptor.cueId + ":"
                    + C1CampaignLootPool15BattleCanonical.Hash(
                        request.eventId + "|"
                        + row.Source.sourceItemInstanceId + "|"
                        + sequence.ToString(CultureInfo.InvariantCulture))
                        .Substring(0, 16);
                accepted.Add(new C1CampaignLootPool15BattleEffectApplication(
                    sequence,
                    request.eventId,
                    row.Source.sourceItemInstanceId,
                    row.Source.sourceFactCanonicalSignature,
                    row.Descriptor.baseItemId,
                    row.Descriptor.familyId,
                    row.Descriptor.familyVariantId,
                    row.Descriptor.triggerId,
                    row.Descriptor.conditionId,
                    row.Descriptor.operationId,
                    row.Descriptor.targetStatId,
                    row.Descriptor.targetScopeId,
                    row.Source.targetIdentity,
                    row.Candidate.amount,
                    row.AppliedAmount,
                    row.Candidate.nativeUnitId,
                    request.baseBattleTimeMs,
                    resolveTime,
                    cueIdentity));
            }

            foreach (C1CampaignLootPool15BattleEffectApplication row in accepted)
            {
                acceptedEventSources.Add(EventSourceKey(
                    request.eventId,
                    row.sourceItemInstanceId));
                ledger.Add(row.Clone());
            }

            return new C1CampaignLootPool15BattleExecutionResult(
                request.eventId,
                accepted,
                Array.Empty<C1CampaignLootPool15BattleEffectRejection>());
        }

        public IReadOnlyList<C1CampaignLootPool15BattleEffectApplication>
            GetLedgerSnapshot()
        {
            return Array.AsReadOnly(ledger.Select(row => row.Clone()).ToArray());
        }

        private static C1CampaignLootPool15BattleFoundationResult Failure(
            string code,
            string message)
        {
            return new C1CampaignLootPool15BattleFoundationResult(null, code, message);
        }

        private static C1CampaignLootPool15BattleExecutionResult RejectedResult(
            string eventId,
            string sourceBaseItemId,
            string code,
            string message)
        {
            return new C1CampaignLootPool15BattleExecutionResult(
                eventId,
                Array.Empty<C1CampaignLootPool15BattleEffectApplication>(),
                new[]
                {
                    new C1CampaignLootPool15BattleEffectRejection(
                        eventId,
                        sourceBaseItemId,
                        code,
                        message)
                });
        }

        private static string EventSourceKey(
            string eventId,
            string sourceItemInstanceId)
        {
            return (eventId ?? string.Empty) + "\u001f"
                + (sourceItemInstanceId ?? string.Empty);
        }

        private static bool EqualsOrdinal(string left, string right)
        {
            return string.Equals(left, right, StringComparison.Ordinal);
        }

        private sealed class PendingApplication
        {
            public PendingApplication(
                C1CampaignLootItemProfileSnapshot descriptor,
                C1CampaignLootPool15BattleCandidateProfile candidate,
                C1CampaignLootPool15BattleSourceRequest source,
                long appliedAmount)
            {
                Descriptor = descriptor;
                Candidate = candidate;
                Source = source;
                AppliedAmount = appliedAmount;
            }

            public C1CampaignLootItemProfileSnapshot Descriptor { get; }
            public C1CampaignLootPool15BattleCandidateProfile Candidate { get; }
            public C1CampaignLootPool15BattleSourceRequest Source { get; }
            public long AppliedAmount { get; }
        }

        private interface IFamilyExecutor
        {
            bool TryApply(
                C1CampaignLootItemProfileSnapshot descriptor,
                C1CampaignLootPool15BattleCandidateProfile candidate,
                C1CampaignLootPool15BattleSourceRequest source,
                out long appliedAmount,
                out string diagnosticCode,
                out string diagnosticMessage);
        }

        private abstract class FamilyExecutorBase : IFamilyExecutor
        {
            private readonly HashSet<string> allowedOperations;

            protected FamilyExecutorBase(params string[] operations)
            {
                allowedOperations = new HashSet<string>(
                    operations ?? Array.Empty<string>(),
                    StringComparer.Ordinal);
            }

            public bool TryApply(
                C1CampaignLootItemProfileSnapshot descriptor,
                C1CampaignLootPool15BattleCandidateProfile candidate,
                C1CampaignLootPool15BattleSourceRequest source,
                out long appliedAmount,
                out string diagnosticCode,
                out string diagnosticMessage)
            {
                if (!allowedOperations.Contains(descriptor.operationId))
                {
                    appliedAmount = 0L;
                    diagnosticCode =
                        C1CampaignLootPool15BattleDiagnosticCodes.OperationRejected;
                    diagnosticMessage = "Operation is unsupported by this family executor.";
                    return false;
                }
                if (string.Equals(
                        descriptor.operationId,
                        "AddPercent",
                        StringComparison.Ordinal))
                {
                    if (!string.Equals(
                            candidate.nativeUnitId,
                            "basisPoint",
                            StringComparison.Ordinal)
                        || !source.percentageBaseValueUnits.HasValue)
                    {
                        appliedAmount = 0L;
                        diagnosticCode =
                            C1CampaignLootPool15BattleDiagnosticCodes.UnitRejected;
                        diagnosticMessage =
                            "Percentage execution requires basisPoint and an explicit base value.";
                        return false;
                    }
                    if (!C1CampaignLootPool15BattleFixedPoint.TryApplyBasisPoints(
                            source.percentageBaseValueUnits.Value,
                            candidate.amount,
                            out appliedAmount))
                    {
                        diagnosticCode =
                            C1CampaignLootPool15BattleDiagnosticCodes.FixedPointOverflow;
                        diagnosticMessage = "Checked fixed-point percentage overflowed.";
                        return false;
                    }
                }
                else
                {
                    appliedAmount = candidate.amount;
                }

                diagnosticCode = C1CampaignLootPool15BattleDiagnosticCodes.None;
                diagnosticMessage = string.Empty;
                return true;
            }
        }

        private sealed class DirectTempoExecutor : FamilyExecutorBase
        {
            public DirectTempoExecutor()
                : base("Refund", "AddFlat", "AddPercent")
            {
            }
        }

        private sealed class TargetSpreadExecutor : FamilyExecutorBase
        {
            public TargetSpreadExecutor()
                : base("ExtraTarget", "Convert")
            {
            }
        }

        private sealed class GuardSustainExecutor : FamilyExecutorBase
        {
            public GuardSustainExecutor()
                : base("AddFlat", "ExtraTrigger")
            {
            }
        }

        private sealed class EnemyControlExecutor : FamilyExecutorBase
        {
            public EnemyControlExecutor()
                : base("AddPercent", "ReduceFlat", "Override")
            {
            }
        }

        private sealed class PositionalAdjacencyExecutor : FamilyExecutorBase
        {
            public PositionalAdjacencyExecutor()
                : base("ReduceFlat", "AddPercent")
            {
            }
        }
    }

    internal static class C1CampaignLootPool15BattleCanonical
    {
        internal static string Build(Action<Builder> append)
        {
            Builder builder = new Builder();
            append(builder);
            return builder.Build();
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

        internal sealed class Builder
        {
            private readonly StringBuilder buffer = new StringBuilder();

            internal void AddString(string key, string value)
            {
                string safe = value ?? string.Empty;
                buffer.Append(key);
                buffer.Append("=s");
                buffer.Append(Encoding.UTF8.GetByteCount(safe)
                    .ToString(CultureInfo.InvariantCulture));
                buffer.Append(':');
                buffer.Append(safe);
                buffer.Append(';');
            }

            internal void AddInt(string key, int value)
            {
                buffer.Append(key).Append("=i");
                buffer.Append(value.ToString(CultureInfo.InvariantCulture));
                buffer.Append(';');
            }

            internal void AddLong(string key, long value)
            {
                buffer.Append(key).Append("=l");
                buffer.Append(value.ToString(CultureInfo.InvariantCulture));
                buffer.Append(';');
            }

            internal void AddBool(string key, bool value)
            {
                buffer.Append(key).Append("=b");
                buffer.Append(value ? '1' : '0');
                buffer.Append(';');
            }

            internal string Build()
            {
                return buffer.ToString();
            }
        }
    }
}
