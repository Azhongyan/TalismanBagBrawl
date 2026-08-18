using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.Items.Generation;
using TalismanBag.Items.InnerCatalog;

namespace TalismanBag.Items.CampaignLoot
{
    public static class C1CampaignLootDependencyStatuses
    {
        public const string Supported = "SUPPORTED";
        public const string Unsupported = "UNSUPPORTED";
        public const string NotExecuted = "NOT_EXECUTED";
        public const string Unknown = "UNKNOWN";
        public const string NumericProfilePending = "NUMERIC_PROFILE_PENDING";
        public const string CandidatePendingBalance = "CANDIDATE_PENDING_BALANCE";
        public const string CandidateLineageOnly = "CANDIDATE_LINEAGE_ONLY";
        public const string AuthoritativeExternalReferenceNotEmbedded =
            "AUTHORITATIVE_EXTERNAL_REFERENCE_NOT_EMBEDDED";
    }

    public static class C1CampaignLootEligibleItemPool15DiagnosticCodes
    {
        public const string None = "NONE";
        public const string IdentitySourceNull = "IDENTITY_SOURCE_NULL";
        public const string IdentityEnumerationFailed = "IDENTITY_ENUMERATION_FAILED";
        public const string IdentityRequired = "IDENTITY_REQUIRED";
        public const string IdentityCountInvalid = "IDENTITY_COUNT_INVALID";
        public const string IdentityDuplicate = "IDENTITY_DUPLICATE";
        public const string IdentitySetMismatch = "IDENTITY_SET_MISMATCH";
        public const string CatalogInvalid = "CATALOG_INVALID";
        public const string CatalogIdentityMissing = "CATALOG_IDENTITY_MISSING";
        public const string CatalogIdentityInvalid = "CATALOG_IDENTITY_INVALID";
        public const string RarityFoundationInvalid = "RARITY_FOUNDATION_INVALID";
        public const string OrdinaryIdentityInvalid = "ORDINARY_IDENTITY_INVALID";
        public const string FamilyContractInvalid = "FAMILY_CONTRACT_INVALID";
    }

    public sealed class C1CampaignLootDependencyEntry
    {
        internal C1CampaignLootDependencyEntry(
            string dependencyId,
            string status,
            string owner,
            string statement)
        {
            this.dependencyId = dependencyId ?? string.Empty;
            this.status = status ?? string.Empty;
            this.owner = owner ?? string.Empty;
            this.statement = statement ?? string.Empty;
            canonicalSignature = C1CampaignLootCanonical.Hash(
                C1CampaignLootCanonical.Build(builder =>
                {
                    builder.AddString("dependencyId", this.dependencyId);
                    builder.AddString("status", this.status);
                    builder.AddString("owner", this.owner);
                    builder.AddString("statement", this.statement);
                }));
        }

        public string dependencyId { get; }
        public string status { get; }
        public string owner { get; }
        public string statement { get; }
        public string canonicalSignature { get; }

        internal C1CampaignLootDependencyEntry Clone()
        {
            return new C1CampaignLootDependencyEntry(
                dependencyId,
                status,
                owner,
                statement);
        }
    }

    public sealed class C1CampaignLootItemProfileSnapshot
    {
        internal C1CampaignLootItemProfileSnapshot(
            string baseItemId,
            string rarityKey,
            string catalogDisplayName,
            string itemFamilyKey,
            string faMenKey,
            string qiLeiKey,
            string shapeId,
            string shapeCellsIdentity,
            string coreCellIdentity,
            string catalogDefaultRarityKey,
            string artworkIdentity,
            string familyId,
            string familyVariantId,
            string candidateSemanticSourceIdentity,
            string candidateSemanticSourceRevision,
            string candidateSemanticSourceSignature,
            string candidateSemanticMaturity,
            string effectCategoryId,
            string triggerId,
            string conditionId,
            string operationId,
            string targetStatId,
            string targetScopeId,
            string nativeUnitId,
            string cueId,
            string presentationProfileId,
            string numericPayloadStatus,
            long? primaryValueUnits,
            long? secondaryValueUnits,
            decimal? cooldownSeconds,
            decimal? firstTriggerOffsetSeconds,
            string authoritativeNumericReferenceId)
        {
            schemaId = C1CampaignLootEligibleItemPool15Carrier.ProfileSchemaId;
            this.baseItemId = baseItemId ?? string.Empty;
            this.rarityKey = rarityKey ?? string.Empty;
            rarityVersionIdentity = this.baseItemId + "@" + this.rarityKey;
            this.catalogDisplayName = catalogDisplayName ?? string.Empty;
            this.itemFamilyKey = itemFamilyKey ?? string.Empty;
            this.faMenKey = faMenKey ?? string.Empty;
            this.qiLeiKey = qiLeiKey ?? string.Empty;
            this.shapeId = shapeId ?? string.Empty;
            this.shapeCellsIdentity = shapeCellsIdentity ?? string.Empty;
            this.coreCellIdentity = coreCellIdentity ?? string.Empty;
            this.catalogDefaultRarityKey = catalogDefaultRarityKey ?? string.Empty;
            this.artworkIdentity = artworkIdentity ?? string.Empty;
            this.familyId = familyId ?? string.Empty;
            this.familyVariantId = familyVariantId ?? string.Empty;
            this.candidateSemanticSourceIdentity =
                candidateSemanticSourceIdentity ?? string.Empty;
            this.candidateSemanticSourceRevision =
                candidateSemanticSourceRevision ?? string.Empty;
            this.candidateSemanticSourceSignature =
                candidateSemanticSourceSignature ?? string.Empty;
            this.candidateSemanticMaturity = candidateSemanticMaturity ?? string.Empty;
            this.effectCategoryId = effectCategoryId ?? string.Empty;
            this.triggerId = triggerId ?? string.Empty;
            this.conditionId = conditionId ?? string.Empty;
            this.operationId = operationId ?? string.Empty;
            this.targetStatId = targetStatId ?? string.Empty;
            this.targetScopeId = targetScopeId ?? string.Empty;
            this.nativeUnitId = nativeUnitId ?? string.Empty;
            this.cueId = cueId ?? string.Empty;
            this.presentationProfileId = presentationProfileId ?? string.Empty;
            this.numericPayloadStatus = numericPayloadStatus ?? string.Empty;
            this.primaryValueUnits = primaryValueUnits;
            this.secondaryValueUnits = secondaryValueUnits;
            this.cooldownSeconds = cooldownSeconds;
            this.firstTriggerOffsetSeconds = firstTriggerOffsetSeconds;
            this.authoritativeNumericReferenceId =
                authoritativeNumericReferenceId ?? string.Empty;
            canonicalPayload = BuildCanonicalPayload();
            profileCanonicalSignature = C1CampaignLootCanonical.Hash(canonicalPayload);
        }

        public string schemaId { get; }
        public string baseItemId { get; }
        public string rarityKey { get; }
        public string rarityVersionIdentity { get; }
        public string catalogDisplayName { get; }
        public string itemFamilyKey { get; }
        public string faMenKey { get; }
        public string qiLeiKey { get; }
        public string shapeId { get; }
        public string shapeCellsIdentity { get; }
        public string coreCellIdentity { get; }
        public string catalogDefaultRarityKey { get; }
        public string artworkIdentity { get; }
        public string familyId { get; }
        public string familyVariantId { get; }
        public string candidateSemanticSourceIdentity { get; }
        public string candidateSemanticSourceRevision { get; }
        public string candidateSemanticSourceSignature { get; }
        public string candidateSemanticMaturity { get; }
        public string effectCategoryId { get; }
        public string triggerId { get; }
        public string conditionId { get; }
        public string operationId { get; }
        public string targetStatId { get; }
        public string targetScopeId { get; }
        public string nativeUnitId { get; }
        public string cueId { get; }
        public string presentationProfileId { get; }
        public string numericPayloadStatus { get; }
        public long? primaryValueUnits { get; }
        public long? secondaryValueUnits { get; }
        public decimal? cooldownSeconds { get; }
        public decimal? firstTriggerOffsetSeconds { get; }
        public string authoritativeNumericReferenceId { get; }
        public string canonicalPayload { get; }
        public string profileCanonicalSignature { get; }

        internal C1CampaignLootItemProfileSnapshot Clone()
        {
            return new C1CampaignLootItemProfileSnapshot(
                baseItemId,
                rarityKey,
                catalogDisplayName,
                itemFamilyKey,
                faMenKey,
                qiLeiKey,
                shapeId,
                shapeCellsIdentity,
                coreCellIdentity,
                catalogDefaultRarityKey,
                artworkIdentity,
                familyId,
                familyVariantId,
                candidateSemanticSourceIdentity,
                candidateSemanticSourceRevision,
                candidateSemanticSourceSignature,
                candidateSemanticMaturity,
                effectCategoryId,
                triggerId,
                conditionId,
                operationId,
                targetStatId,
                targetScopeId,
                nativeUnitId,
                cueId,
                presentationProfileId,
                numericPayloadStatus,
                primaryValueUnits,
                secondaryValueUnits,
                cooldownSeconds,
                firstTriggerOffsetSeconds,
                authoritativeNumericReferenceId);
        }

        private string BuildCanonicalPayload()
        {
            return C1CampaignLootCanonical.Build(builder =>
            {
                builder.AddString("schemaId", schemaId);
                builder.AddString("baseItemId", baseItemId);
                builder.AddString("rarityKey", rarityKey);
                builder.AddString("rarityVersionIdentity", rarityVersionIdentity);
                builder.AddString("catalogDisplayName", catalogDisplayName);
                builder.AddString("itemFamilyKey", itemFamilyKey);
                builder.AddString("faMenKey", faMenKey);
                builder.AddString("qiLeiKey", qiLeiKey);
                builder.AddString("shapeId", shapeId);
                builder.AddString("shapeCellsIdentity", shapeCellsIdentity);
                builder.AddString("coreCellIdentity", coreCellIdentity);
                builder.AddString("catalogDefaultRarityKey", catalogDefaultRarityKey);
                builder.AddString("artworkIdentity", artworkIdentity);
                builder.AddString("familyId", familyId);
                builder.AddString("familyVariantId", familyVariantId);
                builder.AddString("candidateSemanticSourceIdentity",
                    candidateSemanticSourceIdentity);
                builder.AddString("candidateSemanticSourceRevision",
                    candidateSemanticSourceRevision);
                builder.AddString("candidateSemanticSourceSignature",
                    candidateSemanticSourceSignature);
                builder.AddString("candidateSemanticMaturity", candidateSemanticMaturity);
                builder.AddString("effectCategoryId", effectCategoryId);
                builder.AddString("triggerId", triggerId);
                builder.AddString("conditionId", conditionId);
                builder.AddString("operationId", operationId);
                builder.AddString("targetStatId", targetStatId);
                builder.AddString("targetScopeId", targetScopeId);
                builder.AddString("nativeUnitId", nativeUnitId);
                builder.AddString("cueId", cueId);
                builder.AddString("presentationProfileId", presentationProfileId);
                builder.AddString("numericPayloadStatus", numericPayloadStatus);
                builder.AddNullableLong("primaryValueUnits", primaryValueUnits);
                builder.AddNullableLong("secondaryValueUnits", secondaryValueUnits);
                builder.AddNullableDecimal("cooldownSeconds", cooldownSeconds);
                builder.AddNullableDecimal("firstTriggerOffsetSeconds",
                    firstTriggerOffsetSeconds);
                builder.AddString("authoritativeNumericReferenceId",
                    authoritativeNumericReferenceId);
            });
        }
    }

    public sealed class C1CampaignLootEffectFamilyDescriptor
    {
        private readonly ReadOnlyCollection<string> orderedBaseItemIds;
        private readonly ReadOnlyCollection<string> orderedVariantIds;

        internal C1CampaignLootEffectFamilyDescriptor(
            string familyId,
            IEnumerable<C1CampaignLootItemProfileSnapshot> profiles)
        {
            this.familyId = familyId ?? string.Empty;
            C1CampaignLootItemProfileSnapshot[] ordered = (profiles
                ?? Enumerable.Empty<C1CampaignLootItemProfileSnapshot>())
                .Where(profile => profile != null)
                .OrderBy(profile => profile.baseItemId, StringComparer.Ordinal)
                .ToArray();
            orderedBaseItemIds = Array.AsReadOnly(
                ordered.Select(profile => profile.baseItemId).ToArray());
            orderedVariantIds = Array.AsReadOnly(
                ordered.Select(profile => profile.familyVariantId).ToArray());
            canonicalSignature = C1CampaignLootCanonical.Hash(
                C1CampaignLootCanonical.Build(builder =>
                {
                    builder.AddString("familyId", this.familyId);
                    builder.AddInt("variantCount", ordered.Length);
                    foreach (C1CampaignLootItemProfileSnapshot profile in ordered)
                    {
                        builder.AddString("baseItemId", profile.baseItemId);
                        builder.AddString("familyVariantId", profile.familyVariantId);
                        builder.AddString("profileCanonicalSignature",
                            profile.profileCanonicalSignature);
                    }
                }));
        }

        public string familyId { get; }
        public IReadOnlyList<string> OrderedBaseItemIds => orderedBaseItemIds;
        public IReadOnlyList<string> OrderedVariantIds => orderedVariantIds;
        public string canonicalSignature { get; }

    }

    public sealed class C1CampaignLootRewardDependencyInventory
    {
        private readonly ReadOnlyCollection<string> orderedEligibleBaseItemIds;
        private readonly ReadOnlyCollection<C1CampaignLootDependencyEntry> entries;

        internal C1CampaignLootRewardDependencyInventory(
            string eligiblePoolId,
            string eligiblePoolCanonicalSignature,
            IEnumerable<string> orderedBaseItemIds,
            IEnumerable<C1CampaignLootDependencyEntry> dependencyEntries)
        {
            schemaId = "C1CampaignLootRewardDependencyInventory.v1";
            this.eligiblePoolId = eligiblePoolId ?? string.Empty;
            this.eligiblePoolCanonicalSignature =
                eligiblePoolCanonicalSignature ?? string.Empty;
            orderedEligibleBaseItemIds = Array.AsReadOnly((orderedBaseItemIds
                ?? Enumerable.Empty<string>()).ToArray());
            entries = Array.AsReadOnly((dependencyEntries
                ?? Enumerable.Empty<C1CampaignLootDependencyEntry>())
                .Where(entry => entry != null)
                .Select(entry => entry.Clone())
                .ToArray());
            ownerStatement = "Reward owns equal weights, seed progression, recent-two "
                + "exclusion, session history, StageClear/claim idempotency and RewardResult.";
            canonicalSignature = C1CampaignLootCanonical.Hash(BuildCanonicalPayload());
        }

        public string schemaId { get; }
        public string eligiblePoolId { get; }
        public string eligiblePoolCanonicalSignature { get; }
        public IReadOnlyList<string> OrderedEligibleBaseItemIds =>
            orderedEligibleBaseItemIds;
        public string ownerStatement { get; }
        public IReadOnlyList<C1CampaignLootDependencyEntry> Entries => entries;
        public string canonicalSignature { get; }

        private string BuildCanonicalPayload()
        {
            return C1CampaignLootCanonical.Build(builder =>
            {
                builder.AddString("schemaId", schemaId);
                builder.AddString("eligiblePoolId", eligiblePoolId);
                builder.AddInt("eligibleCount", orderedEligibleBaseItemIds.Count);
                foreach (string baseItemId in orderedEligibleBaseItemIds)
                {
                    builder.AddString("baseItemId", baseItemId);
                }
                builder.AddString("ownerStatement", ownerStatement);
                foreach (C1CampaignLootDependencyEntry entry in entries)
                {
                    builder.AddString("dependencyCanonicalSignature",
                        entry.canonicalSignature);
                }
            });
        }
    }

    public sealed class C1CampaignLootBattleDependencyRow
    {
        internal C1CampaignLootBattleDependencyRow(
            C1CampaignLootItemProfileSnapshot profile)
        {
            baseItemId = profile.baseItemId;
            familyId = profile.familyId;
            familyVariantId = profile.familyVariantId;
            operationId = profile.operationId;
            triggerId = profile.triggerId;
            conditionId = profile.conditionId;
            targetStatId = profile.targetStatId;
            targetScopeId = profile.targetScopeId;
            nativeUnitId = profile.nativeUnitId;
            cueId = profile.cueId;
            executionStatus = C1CampaignLootDependencyStatuses.NotExecuted;
            numericProfileStatus = string.Equals(
                profile.baseItemId,
                "I002",
                StringComparison.Ordinal)
                ? C1CampaignLootDependencyStatuses
                    .AuthoritativeExternalReferenceNotEmbedded
                : C1CampaignLootDependencyStatuses.NumericProfilePending;
            canonicalSignature = C1CampaignLootCanonical.Hash(
                C1CampaignLootCanonical.Build(builder =>
                {
                    builder.AddString("baseItemId", baseItemId);
                    builder.AddString("familyId", familyId);
                    builder.AddString("familyVariantId", familyVariantId);
                    builder.AddString("operationId", operationId);
                    builder.AddString("triggerId", triggerId);
                    builder.AddString("conditionId", conditionId);
                    builder.AddString("targetStatId", targetStatId);
                    builder.AddString("targetScopeId", targetScopeId);
                    builder.AddString("nativeUnitId", nativeUnitId);
                    builder.AddString("cueId", cueId);
                    builder.AddString("executionStatus", executionStatus);
                    builder.AddString("numericProfileStatus", numericProfileStatus);
                }));
        }

        public string baseItemId { get; }
        public string familyId { get; }
        public string familyVariantId { get; }
        public string operationId { get; }
        public string triggerId { get; }
        public string conditionId { get; }
        public string targetStatId { get; }
        public string targetScopeId { get; }
        public string nativeUnitId { get; }
        public string cueId { get; }
        public string executionStatus { get; }
        public string numericProfileStatus { get; }
        public string canonicalSignature { get; }
    }

    public sealed class C1CampaignLootBattleDependencyInventory
    {
        private readonly ReadOnlyCollection<C1CampaignLootBattleDependencyRow> rows;
        private readonly ReadOnlyCollection<C1CampaignLootDependencyEntry> entries;

        internal C1CampaignLootBattleDependencyInventory(
            IEnumerable<C1CampaignLootItemProfileSnapshot> profiles,
            IEnumerable<C1CampaignLootDependencyEntry> dependencyEntries)
        {
            schemaId = "C1CampaignLootBattleDependencyInventory.v1";
            rows = Array.AsReadOnly((profiles
                ?? Enumerable.Empty<C1CampaignLootItemProfileSnapshot>())
                .Where(profile => profile != null)
                .OrderBy(profile => profile.baseItemId, StringComparer.Ordinal)
                .Select(profile => new C1CampaignLootBattleDependencyRow(profile))
                .ToArray());
            entries = Array.AsReadOnly((dependencyEntries
                ?? Enumerable.Empty<C1CampaignLootDependencyEntry>())
                .Where(entry => entry != null)
                .Select(entry => entry.Clone())
                .ToArray());
            currentFormalBattleStatement = "Current formal Battle direct-damage-only "
                + "facts do not execute guard, control, spread or positional effects.";
            canonicalSignature = C1CampaignLootCanonical.Hash(
                C1CampaignLootCanonical.Build(builder =>
                {
                    builder.AddString("schemaId", schemaId);
                    builder.AddString("currentFormalBattleStatement",
                        currentFormalBattleStatement);
                    foreach (C1CampaignLootBattleDependencyRow row in rows)
                    {
                        builder.AddString("rowCanonicalSignature", row.canonicalSignature);
                    }
                    foreach (C1CampaignLootDependencyEntry entry in entries)
                    {
                        builder.AddString("dependencyCanonicalSignature",
                            entry.canonicalSignature);
                    }
                }));
        }

        public string schemaId { get; }
        public IReadOnlyList<C1CampaignLootBattleDependencyRow> Rows => rows;
        public IReadOnlyList<C1CampaignLootDependencyEntry> Entries => entries;
        public string currentFormalBattleStatement { get; }
        public string canonicalSignature { get; }
    }

    public sealed class C1CampaignLootPresentationDependencyRow
    {
        internal C1CampaignLootPresentationDependencyRow(
            C1CampaignLootItemProfileSnapshot profile)
        {
            baseItemId = profile.baseItemId;
            presentationProfileId = profile.presentationProfileId;
            artworkIdentity = profile.artworkIdentity;
            bindingStatus = C1CampaignLootDependencyStatuses.Unknown;
            canonicalSignature = C1CampaignLootCanonical.Hash(
                C1CampaignLootCanonical.Build(builder =>
                {
                    builder.AddString("baseItemId", baseItemId);
                    builder.AddString("presentationProfileId", presentationProfileId);
                    builder.AddString("artworkIdentity", artworkIdentity);
                    builder.AddString("bindingStatus", bindingStatus);
                }));
        }

        public string baseItemId { get; }
        public string presentationProfileId { get; }
        public string artworkIdentity { get; }
        public string bindingStatus { get; }
        public string canonicalSignature { get; }
    }

    public sealed class C1CampaignLootPresentationDependencyInventory
    {
        private readonly ReadOnlyCollection<C1CampaignLootPresentationDependencyRow> rows;
        private readonly ReadOnlyCollection<C1CampaignLootDependencyEntry> entries;

        internal C1CampaignLootPresentationDependencyInventory(
            IEnumerable<C1CampaignLootItemProfileSnapshot> profiles,
            IEnumerable<C1CampaignLootDependencyEntry> dependencyEntries)
        {
            schemaId = "C1CampaignLootPresentationDependencyInventory.v1";
            sharedCardCarrierId = "C1ExactBattleSandboxItemCard.prefab";
            sharedCardRequirement = "ONE_SHARED_CARD_CARRIER_REQUIRED";
            genericBindingStatement = "Generic artwork/profile binding is still missing; "
                + "no per-Item Prefab is required.";
            rows = Array.AsReadOnly((profiles
                ?? Enumerable.Empty<C1CampaignLootItemProfileSnapshot>())
                .Where(profile => profile != null)
                .OrderBy(profile => profile.baseItemId, StringComparer.Ordinal)
                .Select(profile => new C1CampaignLootPresentationDependencyRow(profile))
                .ToArray());
            entries = Array.AsReadOnly((dependencyEntries
                ?? Enumerable.Empty<C1CampaignLootDependencyEntry>())
                .Where(entry => entry != null)
                .Select(entry => entry.Clone())
                .ToArray());
            canonicalSignature = C1CampaignLootCanonical.Hash(
                C1CampaignLootCanonical.Build(builder =>
                {
                    builder.AddString("schemaId", schemaId);
                    builder.AddString("sharedCardCarrierId", sharedCardCarrierId);
                    builder.AddString("sharedCardRequirement", sharedCardRequirement);
                    builder.AddString("genericBindingStatement", genericBindingStatement);
                    foreach (C1CampaignLootPresentationDependencyRow row in rows)
                    {
                        builder.AddString("rowCanonicalSignature", row.canonicalSignature);
                    }
                    foreach (C1CampaignLootDependencyEntry entry in entries)
                    {
                        builder.AddString("dependencyCanonicalSignature",
                            entry.canonicalSignature);
                    }
                }));
        }

        public string schemaId { get; }
        public string sharedCardCarrierId { get; }
        public string sharedCardRequirement { get; }
        public string genericBindingStatement { get; }
        public IReadOnlyList<C1CampaignLootPresentationDependencyRow> Rows => rows;
        public IReadOnlyList<C1CampaignLootDependencyEntry> Entries => entries;
        public string canonicalSignature { get; }
    }

    public sealed class C1CampaignLootEligibleItemPool15Snapshot
    {
        private readonly ReadOnlyCollection<C1CampaignLootItemProfileSnapshot> profiles;
        private readonly ReadOnlyCollection<C1CampaignLootEffectFamilyDescriptor> families;

        internal C1CampaignLootEligibleItemPool15Snapshot(
            string sourceItemCatalogCanonicalSignature,
            IEnumerable<C1CampaignLootItemProfileSnapshot> profileRows,
            IEnumerable<C1CampaignLootEffectFamilyDescriptor> familyRows,
            string poolCanonicalPayload,
            string poolCanonicalSignature,
            C1CampaignLootRewardDependencyInventory rewardDependencies,
            C1CampaignLootBattleDependencyInventory battleDependencies,
            C1CampaignLootPresentationDependencyInventory presentationDependencies)
        {
            schemaId = C1CampaignLootEligibleItemPool15Carrier.PoolSchemaId;
            carrierVersion = C1CampaignLootEligibleItemPool15Carrier.CarrierVersion;
            algorithmId = C1CampaignLootEligibleItemPool15Carrier.AlgorithmId;
            productContext = C1CampaignLootEligibleItemPool15Carrier.ProductContext;
            poolId = C1CampaignLootEligibleItemPool15Carrier.PoolId;
            this.sourceItemCatalogCanonicalSignature =
                sourceItemCatalogCanonicalSignature ?? string.Empty;
            profiles = Array.AsReadOnly((profileRows
                ?? Enumerable.Empty<C1CampaignLootItemProfileSnapshot>())
                .Where(profile => profile != null)
                .OrderBy(profile => profile.baseItemId, StringComparer.Ordinal)
                .Select(profile => profile.Clone())
                .ToArray());
            families = Array.AsReadOnly((familyRows
                ?? Enumerable.Empty<C1CampaignLootEffectFamilyDescriptor>())
                .Where(family => family != null)
                .ToArray());
            this.poolCanonicalPayload = poolCanonicalPayload ?? string.Empty;
            this.poolCanonicalSignature = poolCanonicalSignature ?? string.Empty;
            this.rewardDependencies = rewardDependencies;
            this.battleDependencies = battleDependencies;
            this.presentationDependencies = presentationDependencies;
        }

        public string schemaId { get; }
        public int carrierVersion { get; }
        public string algorithmId { get; }
        public string productContext { get; }
        public string poolId { get; }
        public string sourceItemCatalogCanonicalSignature { get; }
        public IReadOnlyList<C1CampaignLootItemProfileSnapshot> Profiles => profiles;
        public IReadOnlyList<C1CampaignLootEffectFamilyDescriptor> Families => families;
        public string poolCanonicalPayload { get; }
        public string poolCanonicalSignature { get; }
        public C1CampaignLootRewardDependencyInventory rewardDependencies { get; }
        public C1CampaignLootBattleDependencyInventory battleDependencies { get; }
        public C1CampaignLootPresentationDependencyInventory presentationDependencies { get; }

        public C1CampaignLootItemProfileSnapshot FindProfile(string baseItemId)
        {
            C1CampaignLootItemProfileSnapshot profile = profiles.FirstOrDefault(row =>
                string.Equals(row.baseItemId, baseItemId, StringComparison.Ordinal));
            return profile == null ? null : profile.Clone();
        }
    }

    public sealed class C1CampaignLootEligibleItemPool15Result
    {
        private C1CampaignLootEligibleItemPool15Result(
            C1CampaignLootEligibleItemPool15Snapshot snapshot,
            string diagnosticCode,
            string diagnosticMessage)
        {
            this.snapshot = snapshot;
            this.diagnosticCode = diagnosticCode ?? string.Empty;
            this.diagnosticMessage = diagnosticMessage ?? string.Empty;
        }

        public bool accepted => snapshot != null
            && string.Equals(
                diagnosticCode,
                C1CampaignLootEligibleItemPool15DiagnosticCodes.None,
                StringComparison.Ordinal);
        public C1CampaignLootEligibleItemPool15Snapshot snapshot { get; }
        public string diagnosticCode { get; }
        public string diagnosticMessage { get; }

        internal static C1CampaignLootEligibleItemPool15Result Success(
            C1CampaignLootEligibleItemPool15Snapshot snapshot)
        {
            return new C1CampaignLootEligibleItemPool15Result(
                snapshot,
                C1CampaignLootEligibleItemPool15DiagnosticCodes.None,
                string.Empty);
        }

        internal static C1CampaignLootEligibleItemPool15Result Failure(
            string code,
            string message)
        {
            return new C1CampaignLootEligibleItemPool15Result(null, code, message);
        }
    }

    public static class C1CampaignLootEligibleItemPool15Carrier
    {
        public const string PoolSchemaId =
            "C1CampaignLootEligibleItemPool15Snapshot.v1";
        public const string ProfileSchemaId =
            "C1CampaignLootEligibleItemProfile.v1";
        public const int CarrierVersion = 1;
        public const string AlgorithmId =
            "c1-campaign-loot-pool15-canonical-sha256-v1";
        public const string ProductContext = "CAMPAIGN_NORMAL_LV1";
        public const string PoolId = "c1.campaign.ordinary.white.pool15.v1";
        public const string WhiteRarityKey = "white";
        public const string CandidateSourceRevision =
            "ITEM_FOUR_CORE_CANDIDATE_DATA_CORRECTION01_R1";
        public const string I002AuthoritativeNumericReferenceId =
            "C1LV1_STARTER_ITEM_BASELINE_R1/I002@white";

        private static readonly ReadOnlyCollection<string> FrozenBaseItemIdRows =
            Array.AsReadOnly(new[]
            {
                "I002", "I003", "I004", "I007", "I010",
                "I011", "I013", "I014", "I015", "I022",
                "I024", "I025", "I026", "I028", "I030"
            });

        private static readonly ReadOnlyCollection<string> FrozenFamilyIdRows =
            Array.AsReadOnly(new[]
            {
                "DIRECT_TEMPO",
                "TARGET_SPREAD",
                "GUARD_SUSTAIN",
                "ENEMY_CONTROL",
                "POSITIONAL_ADJACENCY"
            });

        private static readonly ReadOnlyCollection<SemanticSeed> SemanticRows =
            Array.AsReadOnly(new[]
            {
                S("I002", "CooldownEffect", "Refund", "nian", "on_consecutive_trigger", "within_2_turns", "point"),
                S("I003", "ConditionalModifier", "AddPercent", "break", "on_hit", "target_has_shield", "basisPoint"),
                S("I004", "TriggeredEffect", "ExtraTarget", "targetCount", "on_direct_lit", "next_zhenlei_trigger", "count"),
                S("I007", "TriggeredEffect", "AddFlat", "burn", "on_first_hit", "is_lit", "stack"),
                S("I010", "SpatialEffect", "ReduceFlat", "nianCost", "on_lit", "adjacent_lihuo", "point"),
                S("I011", "MechanicConversion", "Convert", "burn", "on_mirror_trigger", "target_has_burn", "stack"),
                S("I013", "ConditionalModifier", "AddFlat", "guard", "on_damaged", "guard_below_threshold", "flat"),
                S("I014", "SpatialEffect", "AddPercent", "guard", "on_layout_evaluate", "horizontal_adjacent", "basisPoint"),
                S("I015", "ResourceEffect", "AddFlat", "guard", "on_direct_lit", "is_direct_lit", "flat"),
                S("I022", "CooldownEffect", "ReduceFlat", "cooldown", "on_effect_end", "adjacent_lit_item", "turn"),
                S("I024", "ResourceEffect", "ExtraTrigger", "heal", "on_cleanse", "water_charge_3", "flat"),
                S("I025", "TriggeredEffect", "ReduceFlat", "castProgress", "on_hit", "target_casting", "turn"),
                S("I026", "ConditionalModifier", "AddPercent", "damage", "on_slash", "target_has_flaw", "basisPoint"),
                S("I028", "ConditionalModifier", "Override", "targeting", "on_command_trigger", "next_taibai_hit", "count"),
                S("I030", "MechanicConversion", "ExtraTarget", "targetCount", "on_kill_flaw", "next_slash", "count")
            });

        private static readonly C1CampaignLootEligibleItemPool15Result Resolved =
            TryCreateSnapshot(FrozenBaseItemIdRows);

        public static IReadOnlyList<string> FrozenBaseItemIds =>
            Array.AsReadOnly(FrozenBaseItemIdRows.ToArray());

        public static C1CampaignLootEligibleItemPool15Result Resolve()
        {
            return Resolved;
        }

        public static C1CampaignLootEligibleItemPool15Result TryCreateSnapshot(
            IEnumerable<string> eligibleBaseItemIds)
        {
            if (eligibleBaseItemIds == null)
            {
                return Failure(
                    C1CampaignLootEligibleItemPool15DiagnosticCodes.IdentitySourceNull,
                    "eligibleBaseItemIds is required.");
            }

            string[] requested;
            try
            {
                requested = eligibleBaseItemIds
                    .Select(C1CampaignLootCanonical.Normalize)
                    .ToArray();
            }
            catch (Exception exception)
            {
                return Failure(
                    C1CampaignLootEligibleItemPool15DiagnosticCodes
                        .IdentityEnumerationFailed,
                    "eligibleBaseItemIds enumeration failed: "
                        + exception.GetType().Name);
            }

            if (requested.Any(string.IsNullOrEmpty))
            {
                return Failure(
                    C1CampaignLootEligibleItemPool15DiagnosticCodes.IdentityRequired,
                    "Every eligible base Item identity is required.");
            }

            if (requested.Length != FrozenBaseItemIdRows.Count)
            {
                return Failure(
                    C1CampaignLootEligibleItemPool15DiagnosticCodes
                        .IdentityCountInvalid,
                    "The eligible Item pool must contain exactly fifteen identities.");
            }

            if (requested.Distinct(StringComparer.Ordinal).Count() != requested.Length)
            {
                return Failure(
                    C1CampaignLootEligibleItemPool15DiagnosticCodes.IdentityDuplicate,
                    "The eligible Item pool contains a duplicate identity.");
            }

            string[] ordered = requested.OrderBy(value => value, StringComparer.Ordinal).ToArray();
            if (!ordered.SequenceEqual(FrozenBaseItemIdRows, StringComparer.Ordinal))
            {
                return Failure(
                    C1CampaignLootEligibleItemPool15DiagnosticCodes
                        .IdentitySetMismatch,
                    "The eligible Item pool does not match the frozen fifteen identities.");
            }

            ItemGenerationFoundationSnapshot foundation =
                ItemRarityInstanceFoundation.Create();
            if (foundation == null || !foundation.isValid
                || !ItemInstanceRarityCatalog.TryParseStableKey(
                    WhiteRarityKey,
                    out ItemInstanceRarity whiteRarity)
                || whiteRarity != ItemInstanceRarity.White)
            {
                return Failure(
                    C1CampaignLootEligibleItemPool15DiagnosticCodes
                        .RarityFoundationInvalid,
                    "The current Item rarity foundation does not expose ordinary white identities.");
            }

            string sourceCatalogSignature = BuildSourceCatalogCanonicalSignature();
            if (string.IsNullOrEmpty(sourceCatalogSignature))
            {
                return Failure(
                    C1CampaignLootEligibleItemPool15DiagnosticCodes.CatalogInvalid,
                    "The current Item catalog canonical identity could not be built.");
            }

            List<C1CampaignLootItemProfileSnapshot> profiles = new();
            foreach (string baseItemId in ordered)
            {
                ItemInnerDataDefinition item = ItemInnerDataCatalog.FindById(baseItemId);
                if (item == null)
                {
                    return Failure(
                        C1CampaignLootEligibleItemPool15DiagnosticCodes
                            .CatalogIdentityMissing,
                        "The current Item catalog is missing " + baseItemId + ".");
                }

                if (!item.HasRequiredFields() || item.isLightingSource
                    || !string.Equals(
                        item.ItemFamilyKey,
                        "famenCombat",
                        StringComparison.Ordinal))
                {
                    return Failure(
                        C1CampaignLootEligibleItemPool15DiagnosticCodes
                            .CatalogIdentityInvalid,
                        "The current Item catalog identity is not an ordinary combat Item: "
                            + baseItemId + ".");
                }

                ItemArchetypeIdentitySnapshot archetype =
                    foundation.FindArchetype(baseItemId);
                if (archetype == null || !archetype.isOrdinaryGeneratedItem
                    || archetype.isCoreProgressionItem
                    || !ItemRarityInstanceFoundation.IsOrdinaryBaseItemId(baseItemId))
                {
                    return Failure(
                        C1CampaignLootEligibleItemPool15DiagnosticCodes
                            .OrdinaryIdentityInvalid,
                        "The Item rarity foundation rejected ordinary identity "
                            + baseItemId + ".");
                }

                profiles.Add(BuildProfile(item));
            }

            List<C1CampaignLootEffectFamilyDescriptor> families =
                FrozenFamilyIdRows.Select(familyId =>
                    new C1CampaignLootEffectFamilyDescriptor(
                        familyId,
                        profiles.Where(profile => string.Equals(
                            profile.familyId,
                            familyId,
                            StringComparison.Ordinal))))
                    .ToList();
            if (families.Count != 5
                || families.Any(family => family.OrderedBaseItemIds.Count != 3))
            {
                return Failure(
                    C1CampaignLootEligibleItemPool15DiagnosticCodes
                        .FamilyContractInvalid,
                    "The five-family contract must contain exactly three variants per family.");
            }

            C1CampaignLootRewardDependencyInventory preliminaryReward =
                BuildRewardDependencies(string.Empty, ordered);
            C1CampaignLootBattleDependencyInventory battle =
                BuildBattleDependencies(profiles);
            C1CampaignLootPresentationDependencyInventory presentation =
                BuildPresentationDependencies(profiles);
            string poolPayload = BuildPoolCanonicalPayload(
                sourceCatalogSignature,
                profiles,
                families,
                preliminaryReward,
                battle,
                presentation);
            string poolSignature = C1CampaignLootCanonical.Hash(poolPayload);
            C1CampaignLootRewardDependencyInventory reward =
                BuildRewardDependencies(poolSignature, ordered);

            return C1CampaignLootEligibleItemPool15Result.Success(
                new C1CampaignLootEligibleItemPool15Snapshot(
                    sourceCatalogSignature,
                    profiles,
                    families,
                    poolPayload,
                    poolSignature,
                    reward,
                    battle,
                    presentation));
        }

        private static C1CampaignLootItemProfileSnapshot BuildProfile(
            ItemInnerDataDefinition item)
        {
            SemanticSeed seed = SemanticRows.First(row => string.Equals(
                row.baseItemId,
                item.itemId,
                StringComparison.Ordinal));
            string familyId = FamilyFor(item.itemId);
            string familyVariantId = "c1.campaign."
                + familyId.ToLowerInvariant().Replace('_', '.')
                + "." + item.itemId.ToLowerInvariant() + ".v1";
            string candidateIdentity = "signature_" + item.itemId.ToLowerInvariant();
            string semanticSignature = C1CampaignLootCanonical.Hash(
                C1CampaignLootCanonical.Build(builder =>
                {
                    builder.AddString("sourceRevision", CandidateSourceRevision);
                    builder.AddString("sourceIdentity", candidateIdentity);
                    builder.AddString("baseItemId", item.itemId);
                    builder.AddString("effectCategoryId", seed.effectCategoryId);
                    builder.AddString("operationId", seed.operationId);
                    builder.AddString("targetStatId", seed.targetStatId);
                    builder.AddString("triggerId", seed.triggerId);
                    builder.AddString("conditionId", seed.conditionId);
                    builder.AddString("targetScopeId", "effect_primary_target");
                    builder.AddString("nativeUnitId", seed.nativeUnitId);
                    builder.AddString("maturity",
                        C1CampaignLootDependencyStatuses.CandidateLineageOnly);
                }));
            string shapeCells = string.Join(";", item.defaultLocalCells
                .OrderBy(cell => cell.x)
                .ThenBy(cell => cell.y)
                .Select(ItemInnerDataDefinition.FormatCell));
            bool isI002 = string.Equals(item.itemId, "I002", StringComparison.Ordinal);
            return new C1CampaignLootItemProfileSnapshot(
                item.itemId,
                WhiteRarityKey,
                item.displayName,
                item.ItemFamilyKey,
                item.FaMenKey,
                item.QiLeiKey,
                item.shapeId,
                shapeCells,
                ItemInnerDataDefinition.FormatCell(item.coreCellLocal),
                item.rarityDefault.ToStableKey(),
                item.iconPlaceholderKey,
                familyId,
                familyVariantId,
                candidateIdentity,
                CandidateSourceRevision,
                semanticSignature,
                C1CampaignLootDependencyStatuses.CandidateLineageOnly,
                seed.effectCategoryId,
                seed.triggerId,
                seed.conditionId,
                seed.operationId,
                seed.targetStatId,
                "effect_primary_target",
                seed.nativeUnitId,
                "cue.c1.campaign." + familyId.ToLowerInvariant()
                    + "." + item.itemId.ToLowerInvariant(),
                "presentation.c1.campaign.item."
                    + item.itemId.ToLowerInvariant() + ".white.v1",
                isI002
                    ? C1CampaignLootDependencyStatuses
                        .AuthoritativeExternalReferenceNotEmbedded
                    : C1CampaignLootDependencyStatuses.CandidatePendingBalance,
                null,
                null,
                null,
                null,
                isI002 ? I002AuthoritativeNumericReferenceId : string.Empty);
        }

        private static string FamilyFor(string baseItemId)
        {
            if (new[] { "I002", "I007", "I026" }.Contains(baseItemId))
            {
                return "DIRECT_TEMPO";
            }
            if (new[] { "I004", "I011", "I030" }.Contains(baseItemId))
            {
                return "TARGET_SPREAD";
            }
            if (new[] { "I013", "I015", "I024" }.Contains(baseItemId))
            {
                return "GUARD_SUSTAIN";
            }
            if (new[] { "I003", "I025", "I028" }.Contains(baseItemId))
            {
                return "ENEMY_CONTROL";
            }
            return "POSITIONAL_ADJACENCY";
        }

        private static string BuildSourceCatalogCanonicalSignature()
        {
            ItemInnerDataDefinition[] items = ItemInnerDataCatalog.AllItems
                .Where(item => item != null)
                .OrderBy(item => item.itemId, StringComparer.Ordinal)
                .ToArray();
            if (items.Length != 31)
            {
                return string.Empty;
            }

            string payload = C1CampaignLootCanonical.Build(builder =>
            {
                builder.AddString("schemaId", "ItemInnerDataCatalog.identity.v1");
                builder.AddInt("itemCount", items.Length);
                foreach (ItemInnerDataDefinition item in items)
                {
                    builder.AddString("itemId", item.itemId);
                    builder.AddString("displayName", item.displayName);
                    builder.AddString("itemFamilyKey", item.ItemFamilyKey);
                    builder.AddString("faMenKey", item.FaMenKey);
                    builder.AddString("qiLeiKey", item.QiLeiKey);
                    builder.AddString("shapeId", item.shapeId);
                    builder.AddString("shapeCellsIdentity", string.Join(";",
                        item.defaultLocalCells
                            .OrderBy(cell => cell.x)
                            .ThenBy(cell => cell.y)
                            .Select(ItemInnerDataDefinition.FormatCell)));
                    builder.AddString("coreCellIdentity",
                        ItemInnerDataDefinition.FormatCell(item.coreCellLocal));
                    builder.AddString("catalogDefaultRarityKey",
                        item.rarityDefault.ToStableKey());
                    builder.AddString("allowedCatalogRarities", string.Join("|",
                        item.allowedRarities
                            .Select(rarity => rarity.ToStableKey())
                            .OrderBy(value => value, StringComparer.Ordinal)));
                    builder.AddString("artworkIdentity", item.iconPlaceholderKey);
                    builder.AddBool("isLightingSource", item.isLightingSource);
                }
            });
            return C1CampaignLootCanonical.Hash(payload);
        }

        private static C1CampaignLootRewardDependencyInventory
            BuildRewardDependencies(string poolSignature, IEnumerable<string> baseItemIds)
        {
            return new C1CampaignLootRewardDependencyInventory(
                PoolId,
                poolSignature,
                baseItemIds,
                new[]
                {
                    D("eligible_item_pool", "SUPPORTED", "Item",
                        "Item supplies the exact immutable eligible pool identity and signature."),
                    D("item_materialization", "SUPPORTED", "Item",
                        "Item supplies deterministic instance and entitlement materialization after Reward selects an identity."),
                    D("equal_weights", "UNSUPPORTED", "Reward",
                        "Equal weights are not owned or implemented by this carrier."),
                    D("seed_progression", "UNSUPPORTED", "Reward",
                        "Seed progression is not owned or implemented by this carrier."),
                    D("recent_two_exclusion", "UNSUPPORTED", "Reward",
                        "Recent-two exclusion is not owned or implemented by this carrier."),
                    D("session_drop_history", "UNSUPPORTED", "Reward",
                        "Session drop history is not owned or retained by this carrier."),
                    D("stage_clear_claim_idempotency", "UNSUPPORTED", "Reward",
                        "StageClear and claim idempotency remain Reward-owned."),
                    D("reward_result_integration", "UNKNOWN", "Reward",
                        "RewardResult integration has not been executed by this package.")
                });
        }

        private static C1CampaignLootBattleDependencyInventory
            BuildBattleDependencies(IEnumerable<C1CampaignLootItemProfileSnapshot> profiles)
        {
            return new C1CampaignLootBattleDependencyInventory(
                profiles,
                new[]
                {
                    D("five_family_executor", "NOT_EXECUTED", "Battle",
                        "No family, trigger, condition, operation, target or cue is executed here."),
                    D("numeric_profile_promotion", "UNKNOWN", "Battle/Balance",
                        "Unsupported candidate magnitudes, cooldowns and first-trigger offsets remain absent pending promotion."),
                    D("battle_clock_and_targeting", "UNSUPPORTED", "Battle",
                        "Battle still owns clock and target selection."),
                    D("player_enemy_state", "UNSUPPORTED", "Battle",
                        "Battle still owns guard, sustain, enemy cast timing and effect application."),
                    D("event_ledger_and_cues", "NOT_EXECUTED", "Battle/Presentation",
                        "Cue identity is published, but authoritative cue timing and playback are not executed.")
                });
        }

        private static C1CampaignLootPresentationDependencyInventory
            BuildPresentationDependencies(
                IEnumerable<C1CampaignLootItemProfileSnapshot> profiles)
        {
            return new C1CampaignLootPresentationDependencyInventory(
                profiles,
                new[]
                {
                    D("shared_exact_card", "SUPPORTED", "Presentation",
                        "Presentation must reuse one shared exact Card carrier."),
                    D("generic_artwork_profile_binding", "UNKNOWN", "Presentation",
                        "The generic presentation-profile artwork binding is still missing."),
                    D("per_item_prefab", "UNSUPPORTED", "Presentation",
                        "Per-Item Prefabs are neither supplied nor required.")
                });
        }

        private static string BuildPoolCanonicalPayload(
            string sourceCatalogSignature,
            IEnumerable<C1CampaignLootItemProfileSnapshot> profiles,
            IEnumerable<C1CampaignLootEffectFamilyDescriptor> families,
            C1CampaignLootRewardDependencyInventory reward,
            C1CampaignLootBattleDependencyInventory battle,
            C1CampaignLootPresentationDependencyInventory presentation)
        {
            return C1CampaignLootCanonical.Build(builder =>
            {
                builder.AddString("schemaId", PoolSchemaId);
                builder.AddInt("carrierVersion", CarrierVersion);
                builder.AddString("algorithmId", AlgorithmId);
                builder.AddString("productContext", ProductContext);
                builder.AddString("poolId", PoolId);
                builder.AddString("sourceItemCatalogCanonicalSignature",
                    sourceCatalogSignature);
                C1CampaignLootItemProfileSnapshot[] profileRows = profiles
                    .OrderBy(profile => profile.baseItemId, StringComparer.Ordinal)
                    .ToArray();
                builder.AddInt("profileCount", profileRows.Length);
                foreach (C1CampaignLootItemProfileSnapshot profile in profileRows)
                {
                    builder.AddString("profileCanonicalSignature",
                        profile.profileCanonicalSignature);
                }
                C1CampaignLootEffectFamilyDescriptor[] familyRows = families.ToArray();
                builder.AddInt("familyCount", familyRows.Length);
                foreach (C1CampaignLootEffectFamilyDescriptor family in familyRows)
                {
                    builder.AddString("familyCanonicalSignature",
                        family.canonicalSignature);
                }
                builder.AddString("rewardDependencyCanonicalSignature",
                    reward.canonicalSignature);
                builder.AddString("battleDependencyCanonicalSignature",
                    battle.canonicalSignature);
                builder.AddString("presentationDependencyCanonicalSignature",
                    presentation.canonicalSignature);
            });
        }

        private static C1CampaignLootDependencyEntry D(
            string id,
            string status,
            string owner,
            string statement)
        {
            return new C1CampaignLootDependencyEntry(id, status, owner, statement);
        }

        private static SemanticSeed S(
            string baseItemId,
            string effectCategoryId,
            string operationId,
            string targetStatId,
            string triggerId,
            string conditionId,
            string nativeUnitId)
        {
            return new SemanticSeed(
                baseItemId,
                effectCategoryId,
                operationId,
                targetStatId,
                triggerId,
                conditionId,
                nativeUnitId);
        }

        private static C1CampaignLootEligibleItemPool15Result Failure(
            string code,
            string message)
        {
            return C1CampaignLootEligibleItemPool15Result.Failure(code, message);
        }

        private sealed class SemanticSeed
        {
            public SemanticSeed(
                string baseItemId,
                string effectCategoryId,
                string operationId,
                string targetStatId,
                string triggerId,
                string conditionId,
                string nativeUnitId)
            {
                this.baseItemId = baseItemId;
                this.effectCategoryId = effectCategoryId;
                this.operationId = operationId;
                this.targetStatId = targetStatId;
                this.triggerId = triggerId;
                this.conditionId = conditionId;
                this.nativeUnitId = nativeUnitId;
            }

            public string baseItemId { get; }
            public string effectCategoryId { get; }
            public string operationId { get; }
            public string targetStatId { get; }
            public string triggerId { get; }
            public string conditionId { get; }
            public string nativeUnitId { get; }
        }
    }

    internal static class C1CampaignLootCanonical
    {
        internal static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

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

        internal static bool IsLowerSha256(string value)
        {
            return value != null
                && value.Length == 64
                && value.All(character => (character >= '0' && character <= '9')
                    || (character >= 'a' && character <= 'f'));
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
                buffer.Append(key);
                buffer.Append("=i");
                buffer.Append(value.ToString(CultureInfo.InvariantCulture));
                buffer.Append(';');
            }

            internal void AddBool(string key, bool value)
            {
                buffer.Append(key);
                buffer.Append("=b");
                buffer.Append(value ? '1' : '0');
                buffer.Append(';');
            }

            internal void AddNullableLong(string key, long? value)
            {
                if (!value.HasValue)
                {
                    buffer.Append(key).Append("=n;");
                    return;
                }
                buffer.Append(key);
                buffer.Append("=l");
                buffer.Append(value.Value.ToString(CultureInfo.InvariantCulture));
                buffer.Append(';');
            }

            internal void AddNullableDecimal(string key, decimal? value)
            {
                if (!value.HasValue)
                {
                    buffer.Append(key).Append("=n;");
                    return;
                }
                buffer.Append(key);
                buffer.Append("=d");
                buffer.Append(value.Value.ToString(
                    "0.############################",
                    CultureInfo.InvariantCulture));
                buffer.Append(';');
            }

            internal string Build()
            {
                return buffer.ToString();
            }
        }
    }
}
