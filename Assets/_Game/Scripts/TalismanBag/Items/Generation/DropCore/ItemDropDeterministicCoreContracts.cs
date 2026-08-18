using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using TalismanBag.Items.Generation.Affixes;
using TalismanBag.Items.Generation.Potential;
using TalismanBag.Items.Generation.Rolling;
using TalismanBag.Items.Generation.Stats;

namespace TalismanBag.Items.Generation.DropCore
{
    public static class ItemDropCoreDataStatus
    {
        public const string DevOnlyCandidate = "DEV_ONLY_CANDIDATE";
    }

    public static class ItemDropCoreValidationCodes
    {
        public const string None = "NONE";
        public const string RequestNull = "REQUEST_NULL";
        public const string RequestIdEmpty = "REQUEST_ID_EMPTY";
        public const string ItemInstanceIdEmpty = "ITEM_INSTANCE_ID_EMPTY";
        public const string GenerationVersionUnsupported = "GENERATION_VERSION_UNSUPPORTED";
        public const string DataStatusInvalid = "DATA_STATUS_INVALID";
        public const string SourceIdentityNull = "SOURCE_IDENTITY_NULL";
        public const string SourceContextIdEmpty = "SOURCE_CONTEXT_ID_EMPTY";
        public const string SourceKeyEmpty = "SOURCE_KEY_EMPTY";
        public const string SourceOrdinalInvalid = "SOURCE_ORDINAL_INVALID";
        public const string CandidatePoolIdentityEmpty = "CANDIDATE_POOL_IDENTITY_EMPTY";
        public const string RarityTierProfileIdentityEmpty = "RARITY_TIER_PROFILE_IDENTITY_EMPTY";
        public const string AttributeRollBandProfileIdentityEmpty = "ATTRIBUTE_ROLL_BAND_PROFILE_IDENTITY_EMPTY";
        public const string RollPolicyVersionEmpty = "ROLL_POLICY_VERSION_EMPTY";
        public const string RollPolicyVersionUnsupported = "ROLL_POLICY_VERSION_UNSUPPORTED";
        public const string FoundationNull = "FOUNDATION_NULL";
        public const string FoundationInvalid = "FOUNDATION_INVALID";
        public const string CandidatePoolNull = "CANDIDATE_POOL_NULL";
        public const string CandidatePoolIdMismatch = "CANDIDATE_POOL_ID_MISMATCH";
        public const string CandidatePoolEmpty = "CANDIDATE_POOL_EMPTY";
        public const string CandidateEntryInvalid = "CANDIDATE_ENTRY_INVALID";
        public const string CandidateDuplicate = "CANDIDATE_DUPLICATE";
        public const string CandidateKindInvalid = "CANDIDATE_KIND_INVALID";
        public const string CandidateBaseItemUnknown = "CANDIDATE_BASE_ITEM_UNKNOWN";
        public const string I031Forbidden = "I031_FORBIDDEN";
        public const string CandidateWeightInvalid = "CANDIDATE_WEIGHT_INVALID";
        public const string CandidateWeightSumOverflow = "CANDIDATE_WEIGHT_SUM_OVERFLOW";
        public const string RarityTierProfileNull = "RARITY_TIER_PROFILE_NULL";
        public const string RarityTierProfileIdMismatch = "RARITY_TIER_PROFILE_ID_MISMATCH";
        public const string RarityTierModeInvalid = "RARITY_TIER_MODE_INVALID";
        public const string RarityTierProfileEmpty = "RARITY_TIER_PROFILE_EMPTY";
        public const string RarityTierEntryInvalid = "RARITY_TIER_ENTRY_INVALID";
        public const string RarityTierDuplicate = "RARITY_TIER_DUPLICATE";
        public const string RarityTierWeightInvalid = "RARITY_TIER_WEIGHT_INVALID";
        public const string RarityTierWeightSumOverflow = "RARITY_TIER_WEIGHT_SUM_OVERFLOW";
        public const string FixedRarityTierEntryCountInvalid = "FIXED_RARITY_TIER_ENTRY_COUNT_INVALID";
        public const string DropDomainInvalid = "DROP_DOMAIN_INVALID";
        public const string StatSchemaMissing = "STAT_SCHEMA_MISSING";
        public const string AffixSchemaMissing = "AFFIX_SCHEMA_MISSING";
        public const string CoreSchemaMissing = "CORE_SCHEMA_MISSING";
        public const string CoreProfileMissing = "CORE_PROFILE_MISSING";
        public const string BuildRollProfileMissing = "BUILD_ROLL_PROFILE_MISSING";
        public const string BuildRollProfileDuplicate = "BUILD_ROLL_PROFILE_DUPLICATE";
        public const string IdentityCreationFailed = "IDENTITY_CREATION_FAILED";
        public const string InstanceRollFailed = "INSTANCE_ROLL_FAILED";
    }

    public sealed class ItemDropCoreValidationError
    {
        public ItemDropCoreValidationError(string code, string message)
        {
            this.code = code ?? string.Empty;
            this.message = message ?? string.Empty;
        }

        public string code { get; }
        public string message { get; }
    }

    public sealed class ItemDropCoreSourceIdentity
    {
        public ItemDropCoreSourceIdentity(string sourceContextId, string sourceKey, int sourceOrdinal)
        {
            this.sourceContextId = Normalize(sourceContextId);
            this.sourceKey = Normalize(sourceKey);
            this.sourceOrdinal = sourceOrdinal;
        }

        public string sourceContextId { get; }
        public string sourceKey { get; }
        public int sourceOrdinal { get; }

        internal ItemDropCoreSourceIdentity Clone() =>
            new(sourceContextId, sourceKey, sourceOrdinal);

        private static string Normalize(string value) =>
            string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    public enum ItemDropCoreCandidateKind
    {
        Unspecified = 0,
        OrdinaryGeneratedItem = 1,
        Other = 2
    }

    public sealed class ItemDropCoreCandidateEntry
    {
        public ItemDropCoreCandidateEntry(
            string baseItemId,
            ItemDropCoreCandidateKind candidateKind,
            long weightUnits)
        {
            this.baseItemId = Normalize(baseItemId);
            this.candidateKind = candidateKind;
            this.weightUnits = weightUnits;
        }

        public string baseItemId { get; }
        public ItemDropCoreCandidateKind candidateKind { get; }
        public long weightUnits { get; }

        internal ItemDropCoreCandidateEntry Clone() =>
            new(baseItemId, candidateKind, weightUnits);

        private static string Normalize(string value) =>
            string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    public sealed class ItemDropCoreCandidatePool
    {
        private readonly ReadOnlyCollection<ItemDropCoreCandidateEntry> candidates;

        public ItemDropCoreCandidatePool(
            string candidatePoolId,
            IEnumerable<ItemDropCoreCandidateEntry> candidates)
        {
            this.candidatePoolId = Normalize(candidatePoolId);
            this.candidates = Freeze((candidates ?? Array.Empty<ItemDropCoreCandidateEntry>())
                .Select(candidate => candidate?.Clone()));
        }

        public string candidatePoolId { get; }
        public IReadOnlyList<ItemDropCoreCandidateEntry> Candidates => candidates;

        public string BuildCanonicalSignature()
        {
            StringBuilder builder = new();
            Append(builder, "candidatePoolId", candidatePoolId);
            foreach (ItemDropCoreCandidateEntry candidate in candidates
                         .OrderBy(value => value?.baseItemId ?? string.Empty, StringComparer.Ordinal)
                         .ThenBy(value => value == null ? int.MinValue : (int)value.candidateKind)
                         .ThenBy(value => value == null ? long.MinValue : value.weightUnits))
            {
                Append(builder, "baseItemId", candidate?.baseItemId ?? string.Empty);
                Append(builder, "candidateKind",
                    candidate == null ? string.Empty : candidate.candidateKind.ToString());
                Append(builder, "weightUnits",
                    candidate == null
                        ? string.Empty
                        : candidate.weightUnits.ToString(CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }

        internal ItemDropCoreCandidatePool Clone() =>
            new(candidatePoolId, candidates);

        private static string Normalize(string value) =>
            string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

        private static ReadOnlyCollection<T> Freeze<T>(IEnumerable<T> values) =>
            Array.AsReadOnly((values ?? Array.Empty<T>()).ToArray());

        private static void Append(StringBuilder builder, string key, string value)
        {
            string safe = value ?? string.Empty;
            builder.Append(key).Append('=')
                .Append(safe.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(safe).Append('\n');
        }
    }

    public enum ItemDropRarityTierMode
    {
        Unspecified = 0,
        Fixed = 1,
        Weighted = 2
    }

    public sealed class ItemDropRarityTierEntry
    {
        public ItemDropRarityTierEntry(ItemInstanceRarity rarity, long weightUnits)
        {
            this.rarity = rarity;
            this.weightUnits = weightUnits;
        }

        public ItemInstanceRarity rarity { get; }
        public long weightUnits { get; }

        internal ItemDropRarityTierEntry Clone() =>
            new(rarity, weightUnits);
    }

    public sealed class ItemDropRarityTierProfile
    {
        private readonly ReadOnlyCollection<ItemDropRarityTierEntry> entries;

        public ItemDropRarityTierProfile(
            string rarityTierProfileId,
            ItemDropRarityTierMode mode,
            IEnumerable<ItemDropRarityTierEntry> entries)
        {
            this.rarityTierProfileId = Normalize(rarityTierProfileId);
            this.mode = mode;
            this.entries = Freeze((entries ?? Array.Empty<ItemDropRarityTierEntry>())
                .Select(entry => entry?.Clone()));
        }

        public string rarityTierProfileId { get; }
        public ItemDropRarityTierMode mode { get; }
        public IReadOnlyList<ItemDropRarityTierEntry> Entries => entries;

        public string BuildCanonicalSignature()
        {
            StringBuilder builder = new();
            Append(builder, "rarityTierProfileId", rarityTierProfileId);
            Append(builder, "mode", mode.ToString());
            foreach (ItemDropRarityTierEntry entry in entries
                         .OrderBy(value => value == null ? int.MinValue : value.rarity.ToTierIndex())
                         .ThenBy(value => value == null ? int.MinValue : (int)value.rarity)
                         .ThenBy(value => value == null ? long.MinValue : value.weightUnits))
            {
                Append(builder, "rarity", entry == null ? string.Empty : entry.rarity.ToStableKey());
                Append(builder, "weightUnits",
                    entry == null
                        ? string.Empty
                        : entry.weightUnits.ToString(CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }

        internal ItemDropRarityTierProfile Clone() =>
            new(rarityTierProfileId, mode, entries);

        private static string Normalize(string value) =>
            string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

        private static ReadOnlyCollection<T> Freeze<T>(IEnumerable<T> values) =>
            Array.AsReadOnly((values ?? Array.Empty<T>()).ToArray());

        private static void Append(StringBuilder builder, string key, string value)
        {
            string safe = value ?? string.Empty;
            builder.Append(key).Append('=')
                .Append(safe.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(safe).Append('\n');
        }
    }

    public sealed class ItemDropDeterministicCoreRequest
    {
        private readonly ReadOnlyCollection<ItemBuildQualificationRollProfile>
            buildQualificationRollProfiles;

        public ItemDropDeterministicCoreRequest(
            string requestId,
            string itemInstanceId,
            long rootSeed,
            int generationVersion,
            string dataStatus,
            ItemDropCoreSourceIdentity sourceIdentity,
            string candidatePoolId,
            string rarityTierProfileId,
            string attributeRollBandProfileId,
            string rollPolicyVersion,
            ItemGenerationFoundationSnapshot foundation,
            ItemDropCoreCandidatePool candidatePool,
            ItemDropRarityTierProfile rarityTierProfile,
            ItemStatRangeSchemaSnapshot statSchema,
            ItemAffixPoolAndRangeSchemaSnapshot affixSchema,
            ItemCorePotentialAndBuildEligibilitySchemaSnapshot coreBuildSchema,
            IEnumerable<ItemBuildQualificationRollProfile> buildQualificationRollProfiles)
        {
            this.requestId = Normalize(requestId);
            this.itemInstanceId = Normalize(itemInstanceId);
            this.rootSeed = rootSeed;
            this.generationVersion = generationVersion;
            this.dataStatus = Normalize(dataStatus);
            this.sourceIdentity = sourceIdentity?.Clone();
            this.candidatePoolId = Normalize(candidatePoolId);
            this.rarityTierProfileId = Normalize(rarityTierProfileId);
            this.attributeRollBandProfileId = Normalize(attributeRollBandProfileId);
            this.rollPolicyVersion = Normalize(rollPolicyVersion);
            this.foundation = foundation;
            this.candidatePool = candidatePool?.Clone();
            this.rarityTierProfile = rarityTierProfile?.Clone();
            this.statSchema = statSchema;
            this.affixSchema = affixSchema;
            this.coreBuildSchema = coreBuildSchema;
            this.buildQualificationRollProfiles = Freeze((buildQualificationRollProfiles
                    ?? Array.Empty<ItemBuildQualificationRollProfile>())
                .Select(Clone)
                .OrderBy(profile => profile == null ? int.MinValue : profile.rarity.ToTierIndex())
                .ThenBy(profile => profile?.profileId ?? string.Empty, StringComparer.Ordinal));
        }

        public string requestId { get; }
        public string itemInstanceId { get; }
        public long rootSeed { get; }
        public int generationVersion { get; }
        public string dataStatus { get; }
        public ItemDropCoreSourceIdentity sourceIdentity { get; }
        public string candidatePoolId { get; }
        public string rarityTierProfileId { get; }
        public string attributeRollBandProfileId { get; }
        public string rollPolicyVersion { get; }
        public ItemGenerationFoundationSnapshot foundation { get; }
        public ItemDropCoreCandidatePool candidatePool { get; }
        public ItemDropRarityTierProfile rarityTierProfile { get; }
        public ItemStatRangeSchemaSnapshot statSchema { get; }
        public ItemAffixPoolAndRangeSchemaSnapshot affixSchema { get; }
        public ItemCorePotentialAndBuildEligibilitySchemaSnapshot coreBuildSchema { get; }
        public IReadOnlyList<ItemBuildQualificationRollProfile> BuildQualificationRollProfiles =>
            buildQualificationRollProfiles;

        private static ItemBuildQualificationRollProfile Clone(ItemBuildQualificationRollProfile value) =>
            value == null
                ? null
                : new ItemBuildQualificationRollProfile(
                    value.profileId,
                    value.rarity,
                    value.dataMaturityKey,
                    value.Entries);

        private static string Normalize(string value) =>
            string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

        private static ReadOnlyCollection<T> Freeze<T>(IEnumerable<T> values) =>
            Array.AsReadOnly((values ?? Array.Empty<T>()).ToArray());
    }

    public sealed class ItemDropDeterministicCoreSnapshot
    {
        public const string CurrentSchemaId = "ItemDropDeterministicCoreSnapshot.v1";

        internal ItemDropDeterministicCoreSnapshot(
            ItemDropDeterministicCoreRequest request,
            string candidatePoolSignature,
            string rarityTierProfileSignature,
            string selectedBaseItemId,
            ItemInstanceRarity selectedRarity,
            ItemGeneratedInstanceSnapshot generatedInstance)
        {
            schemaId = CurrentSchemaId;
            dropAlgorithmId = ItemDropDeterministicDomain.AlgorithmId;
            dataStatus = request?.dataStatus ?? string.Empty;
            requestId = request?.requestId ?? string.Empty;
            itemInstanceId = request?.itemInstanceId ?? string.Empty;
            rootSeed = request?.rootSeed ?? 0L;
            generationVersion = request?.generationVersion ?? 0;
            sourceContextId = request?.sourceIdentity?.sourceContextId ?? string.Empty;
            sourceKey = request?.sourceIdentity?.sourceKey ?? string.Empty;
            sourceOrdinal = request?.sourceIdentity?.sourceOrdinal ?? 0;
            candidatePoolId = request?.candidatePoolId ?? string.Empty;
            rarityTierProfileId = request?.rarityTierProfileId ?? string.Empty;
            attributeRollBandProfileId = request?.attributeRollBandProfileId ?? string.Empty;
            rollPolicyVersion = request?.rollPolicyVersion ?? string.Empty;
            this.candidatePoolSignature = candidatePoolSignature ?? string.Empty;
            this.rarityTierProfileSignature = rarityTierProfileSignature ?? string.Empty;
            this.selectedBaseItemId = selectedBaseItemId ?? string.Empty;
            this.selectedRarity = selectedRarity;
            this.generatedInstance = generatedInstance;
        }

        public string schemaId { get; }
        public string dropAlgorithmId { get; }
        public string dataStatus { get; }
        public string requestId { get; }
        public string itemInstanceId { get; }
        public long rootSeed { get; }
        public int generationVersion { get; }
        public string sourceContextId { get; }
        public string sourceKey { get; }
        public int sourceOrdinal { get; }
        public string candidatePoolId { get; }
        public string rarityTierProfileId { get; }
        public string attributeRollBandProfileId { get; }
        public string rollPolicyVersion { get; }
        public string candidatePoolSignature { get; }
        public string rarityTierProfileSignature { get; }
        public string selectedBaseItemId { get; }
        public ItemInstanceRarity selectedRarity { get; }
        public ItemGeneratedInstanceSnapshot generatedInstance { get; }

        public string BuildCanonicalSignature()
        {
            StringBuilder builder = new();
            Append(builder, "schemaId", schemaId);
            Append(builder, "dropAlgorithmId", dropAlgorithmId);
            Append(builder, "dataStatus", dataStatus);
            Append(builder, "requestId", requestId);
            Append(builder, "sourceContextId", sourceContextId);
            Append(builder, "sourceKey", sourceKey);
            Append(builder, "sourceOrdinal", sourceOrdinal.ToString(CultureInfo.InvariantCulture));
            Append(builder, "candidatePoolId", candidatePoolId);
            Append(builder, "rarityTierProfileId", rarityTierProfileId);
            Append(builder, "attributeRollBandProfileId", attributeRollBandProfileId);
            Append(builder, "rollPolicyVersion", rollPolicyVersion);
            Append(builder, "candidatePoolSignature", candidatePoolSignature);
            Append(builder, "rarityTierProfileSignature", rarityTierProfileSignature);
            Append(builder, "rootSeed", rootSeed.ToString(CultureInfo.InvariantCulture));
            Append(builder, "generationVersion", generationVersion.ToString(CultureInfo.InvariantCulture));
            Append(builder, "selectedBaseItemId", selectedBaseItemId);
            Append(builder, "selectedRarity", selectedRarity.ToStableKey());
            Append(builder, "itemInstanceId", itemInstanceId);
            Append(builder, "generatedInstance",
                generatedInstance?.BuildCanonicalSignature() ?? string.Empty);
            return builder.ToString();
        }

        private static void Append(StringBuilder builder, string key, string value)
        {
            string safe = value ?? string.Empty;
            builder.Append(key).Append('=')
                .Append(safe.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(safe).Append('\n');
        }
    }

    public sealed class ItemDropDeterministicCoreResult
    {
        private readonly ReadOnlyCollection<ItemDropCoreValidationError> validationErrors;

        internal ItemDropDeterministicCoreResult(
            ItemDropDeterministicCoreSnapshot snapshot,
            IEnumerable<ItemDropCoreValidationError> validationErrors)
        {
            this.snapshot = snapshot;
            this.validationErrors = Array.AsReadOnly((validationErrors
                ?? Array.Empty<ItemDropCoreValidationError>()).ToArray());
        }

        public bool isSuccess => snapshot != null && validationErrors.Count == 0;
        public ItemDropDeterministicCoreSnapshot snapshot { get; }
        public IReadOnlyList<ItemDropCoreValidationError> ValidationErrors => validationErrors;
        public string primaryValidationCode => validationErrors.Count == 0
            ? ItemDropCoreValidationCodes.None
            : validationErrors[0].code;
    }
}
