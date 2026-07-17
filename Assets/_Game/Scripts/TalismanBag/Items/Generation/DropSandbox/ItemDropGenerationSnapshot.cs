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

namespace TalismanBag.Items.Generation.DropSandbox
{
    public static class ItemDropGenerationValidationCodes
    {
        public const string None = "NONE";
        public const string RequestNull = "REQUEST_NULL";
        public const string DropRequestIdEmpty = "DROP_REQUEST_ID_EMPTY";
        public const string ItemInstanceIdEmpty = "ITEM_INSTANCE_ID_EMPTY";
        public const string GenerationVersionUnsupported = "GENERATION_VERSION_UNSUPPORTED";
        public const string GenerationDataStatusInvalid = "GENERATION_DATA_STATUS_INVALID";
        public const string FoundationNull = "FOUNDATION_NULL";
        public const string FoundationInvalid = "FOUNDATION_INVALID";
        public const string SourceContextNull = "SOURCE_CONTEXT_NULL";
        public const string SourceContextIdEmpty = "SOURCE_CONTEXT_ID_EMPTY";
        public const string SourceKeyEmpty = "SOURCE_KEY_EMPTY";
        public const string SourceContextDataStatusInvalid = "SOURCE_CONTEXT_DATA_STATUS_INVALID";
        public const string StageNumberInvalid = "STAGE_NUMBER_INVALID";
        public const string CandidatePoolNull = "CANDIDATE_POOL_NULL";
        public const string CandidatePoolIdMismatch = "CANDIDATE_POOL_ID_MISMATCH";
        public const string CandidatePoolDataStatusInvalid = "CANDIDATE_POOL_DATA_STATUS_INVALID";
        public const string CandidatePoolEmpty = "CANDIDATE_POOL_EMPTY";
        public const string CandidateDuplicate = "CANDIDATE_DUPLICATE";
        public const string CandidateBaseItemEmpty = "CANDIDATE_BASE_ITEM_EMPTY";
        public const string CandidateBaseItemUnknown = "CANDIDATE_BASE_ITEM_UNKNOWN";
        public const string I031Forbidden = "I031_FORBIDDEN";
        public const string CandidateWeightInvalid = "CANDIDATE_WEIGHT_INVALID";
        public const string CandidateWeightSumOverflow = "CANDIDATE_WEIGHT_SUM_OVERFLOW";
        public const string RarityPolicyUnresolved = "RARITY_POLICY_UNRESOLVED";
        public const string EarlyStageRarityProfileForbidden = "EARLY_STAGE_RARITY_PROFILE_FORBIDDEN";
        public const string RarityProfileIdMismatch = "RARITY_PROFILE_ID_MISMATCH";
        public const string RarityProfileDataStatusInvalid = "RARITY_PROFILE_DATA_STATUS_INVALID";
        public const string RarityEntryDuplicate = "RARITY_ENTRY_DUPLICATE";
        public const string RarityEntryInvalid = "RARITY_ENTRY_INVALID";
        public const string RarityWeightInvalid = "RARITY_WEIGHT_INVALID";
        public const string RarityWeightSumOverflow = "RARITY_WEIGHT_SUM_OVERFLOW";
        public const string DropDomainInvalid = "DROP_DOMAIN_INVALID";
        public const string CoreProfileMissing = "CORE_PROFILE_MISSING";
        public const string IdentityCreationFailed = "IDENTITY_CREATION_FAILED";
        public const string BuildRollProfileMissing = "BUILD_ROLL_PROFILE_MISSING";
        public const string BuildRollProfileDuplicate = "BUILD_ROLL_PROFILE_DUPLICATE";
        public const string InstanceRollFailed = "INSTANCE_ROLL_FAILED";
    }

    public static class ItemDropRarityPolicySources
    {
        public const string LockedStage1To10White = "LOCKED_STAGE_1_10_WHITE";
        public const string QaWeightProfile = "QA_WEIGHT_PROFILE";
    }

    public sealed class ItemDropGenerationValidationError
    {
        public ItemDropGenerationValidationError(string code, string message)
        {
            this.code = code ?? string.Empty;
            this.message = message ?? string.Empty;
        }

        public string code { get; }
        public string message { get; }
    }

    public sealed class ItemDropSourceContextSnapshot
    {
        public ItemDropSourceContextSnapshot(
            string sourceContextId,
            string sourceKey,
            int stageNumber,
            string candidatePoolId,
            string rarityWeightProfileId,
            string dataMaturityKey)
        {
            this.sourceContextId = Normalize(sourceContextId);
            this.sourceKey = Normalize(sourceKey);
            this.stageNumber = stageNumber;
            this.candidatePoolId = Normalize(candidatePoolId);
            this.rarityWeightProfileId = Normalize(rarityWeightProfileId);
            this.dataMaturityKey = Normalize(dataMaturityKey);
        }

        public string sourceContextId { get; }
        public string sourceKey { get; }
        public int stageNumber { get; }
        public string candidatePoolId { get; }
        public string rarityWeightProfileId { get; }
        public string dataMaturityKey { get; }

        private static string Normalize(string value) =>
            string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    public sealed class ItemDropCandidateEntrySnapshot
    {
        public ItemDropCandidateEntrySnapshot(string baseItemId, long weightUnits)
        {
            this.baseItemId = Normalize(baseItemId);
            this.weightUnits = weightUnits;
        }

        public string baseItemId { get; }
        public long weightUnits { get; }

        private static string Normalize(string value) =>
            string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    public sealed class ItemDropCandidatePoolSnapshot
    {
        private readonly ReadOnlyCollection<ItemDropCandidateEntrySnapshot> candidates;

        public ItemDropCandidatePoolSnapshot(
            string poolId,
            string dataMaturityKey,
            IEnumerable<ItemDropCandidateEntrySnapshot> candidates)
        {
            this.poolId = Normalize(poolId);
            this.dataMaturityKey = Normalize(dataMaturityKey);
            this.candidates = Freeze((candidates ?? Array.Empty<ItemDropCandidateEntrySnapshot>())
                .Select(entry => entry == null
                    ? null
                    : new ItemDropCandidateEntrySnapshot(entry.baseItemId, entry.weightUnits)));
        }

        public string poolId { get; }
        public string dataMaturityKey { get; }
        public IReadOnlyList<ItemDropCandidateEntrySnapshot> Candidates => candidates;

        private static string Normalize(string value) =>
            string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

        private static ReadOnlyCollection<T> Freeze<T>(IEnumerable<T> values) =>
            Array.AsReadOnly((values ?? Array.Empty<T>()).ToArray());
    }

    public sealed class ItemDropRarityWeightEntrySnapshot
    {
        public ItemDropRarityWeightEntrySnapshot(ItemInstanceRarity rarity, long weightUnits)
        {
            this.rarity = rarity;
            this.weightUnits = weightUnits;
        }

        public ItemInstanceRarity rarity { get; }
        public long weightUnits { get; }
    }

    public sealed class ItemDropRarityWeightProfileSnapshot
    {
        private readonly ReadOnlyCollection<ItemDropRarityWeightEntrySnapshot> entries;

        public ItemDropRarityWeightProfileSnapshot(
            string profileId,
            string dataMaturityKey,
            IEnumerable<ItemDropRarityWeightEntrySnapshot> entries)
        {
            this.profileId = Normalize(profileId);
            this.dataMaturityKey = Normalize(dataMaturityKey);
            this.entries = Freeze((entries ?? Array.Empty<ItemDropRarityWeightEntrySnapshot>())
                .Select(entry => entry == null
                    ? null
                    : new ItemDropRarityWeightEntrySnapshot(entry.rarity, entry.weightUnits)));
        }

        public string profileId { get; }
        public string dataMaturityKey { get; }
        public IReadOnlyList<ItemDropRarityWeightEntrySnapshot> Entries => entries;

        private static string Normalize(string value) =>
            string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

        private static ReadOnlyCollection<T> Freeze<T>(IEnumerable<T> values) =>
            Array.AsReadOnly((values ?? Array.Empty<T>()).ToArray());
    }

    public sealed class ItemDropGenerationRequest
    {
        private readonly ReadOnlyCollection<ItemBuildQualificationRollProfile> buildQualificationRollProfiles;

        public ItemDropGenerationRequest(
            string dropRequestId,
            string itemInstanceId,
            long rootSeed,
            int generationVersion,
            string generationDataStatus,
            ItemGenerationFoundationSnapshot foundation,
            ItemDropSourceContextSnapshot sourceContext,
            ItemDropCandidatePoolSnapshot candidatePool,
            ItemDropRarityWeightProfileSnapshot rarityWeightProfile,
            ItemStatRangeSchemaSnapshot statSchema,
            ItemAffixPoolAndRangeSchemaSnapshot affixSchema,
            ItemCorePotentialAndBuildEligibilitySchemaSnapshot coreBuildSchema,
            IEnumerable<ItemBuildQualificationRollProfile> buildQualificationRollProfiles)
        {
            this.dropRequestId = Normalize(dropRequestId);
            this.itemInstanceId = Normalize(itemInstanceId);
            this.rootSeed = rootSeed;
            this.generationVersion = generationVersion;
            this.generationDataStatus = Normalize(generationDataStatus);
            this.foundation = foundation;
            this.sourceContext = Clone(sourceContext);
            this.candidatePool = Clone(candidatePool);
            this.rarityWeightProfile = Clone(rarityWeightProfile);
            this.statSchema = statSchema;
            this.affixSchema = affixSchema;
            this.coreBuildSchema = coreBuildSchema;
            this.buildQualificationRollProfiles = Freeze((buildQualificationRollProfiles
                    ?? Array.Empty<ItemBuildQualificationRollProfile>())
                .Select(Clone)
                .OrderBy(profile => profile == null ? int.MinValue : profile.rarity.ToTierIndex())
                .ThenBy(profile => profile?.profileId ?? string.Empty, StringComparer.Ordinal));
        }

        public string dropRequestId { get; }
        public string itemInstanceId { get; }
        public long rootSeed { get; }
        public int generationVersion { get; }
        public string generationDataStatus { get; }
        public ItemGenerationFoundationSnapshot foundation { get; }
        public ItemDropSourceContextSnapshot sourceContext { get; }
        public ItemDropCandidatePoolSnapshot candidatePool { get; }
        public ItemDropRarityWeightProfileSnapshot rarityWeightProfile { get; }
        public ItemStatRangeSchemaSnapshot statSchema { get; }
        public ItemAffixPoolAndRangeSchemaSnapshot affixSchema { get; }
        public ItemCorePotentialAndBuildEligibilitySchemaSnapshot coreBuildSchema { get; }
        public IReadOnlyList<ItemBuildQualificationRollProfile> BuildQualificationRollProfiles =>
            buildQualificationRollProfiles;

        private static ItemDropSourceContextSnapshot Clone(ItemDropSourceContextSnapshot value) =>
            value == null ? null : new ItemDropSourceContextSnapshot(value.sourceContextId, value.sourceKey,
                value.stageNumber, value.candidatePoolId, value.rarityWeightProfileId, value.dataMaturityKey);

        private static ItemDropCandidatePoolSnapshot Clone(ItemDropCandidatePoolSnapshot value) =>
            value == null ? null : new ItemDropCandidatePoolSnapshot(value.poolId, value.dataMaturityKey,
                value.Candidates);

        private static ItemDropRarityWeightProfileSnapshot Clone(ItemDropRarityWeightProfileSnapshot value) =>
            value == null ? null : new ItemDropRarityWeightProfileSnapshot(value.profileId,
                value.dataMaturityKey, value.Entries);

        private static ItemBuildQualificationRollProfile Clone(ItemBuildQualificationRollProfile value) =>
            value == null ? null : new ItemBuildQualificationRollProfile(value.profileId, value.rarity,
                value.dataMaturityKey, value.Entries);

        private static string Normalize(string value) =>
            string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

        private static ReadOnlyCollection<T> Freeze<T>(IEnumerable<T> values) =>
            Array.AsReadOnly((values ?? Array.Empty<T>()).ToArray());
    }

    public sealed class ItemDropGenerationSnapshot
    {
        public const string CurrentSchemaId = "ItemDropGenerationSnapshot.v1";

        internal ItemDropGenerationSnapshot(
            string generationDataStatus,
            string dropRequestId,
            ItemDropSourceContextSnapshot sourceContext,
            string rarityPolicySource,
            string rarityWeightProfileId,
            long rootSeed,
            int generationVersion,
            string selectedBaseItemId,
            ItemInstanceRarity selectedRarity,
            ItemGeneratedInstanceSnapshot generatedInstance)
        {
            schemaId = CurrentSchemaId;
            dropAlgorithmId = DeterministicItemDropRandom.AlgorithmId;
            this.generationDataStatus = generationDataStatus ?? string.Empty;
            this.dropRequestId = dropRequestId ?? string.Empty;
            sourceContextId = sourceContext?.sourceContextId ?? string.Empty;
            sourceKey = sourceContext?.sourceKey ?? string.Empty;
            stageNumber = sourceContext?.stageNumber ?? 0;
            candidatePoolId = sourceContext?.candidatePoolId ?? string.Empty;
            this.rarityPolicySource = rarityPolicySource ?? string.Empty;
            this.rarityWeightProfileId = rarityWeightProfileId ?? string.Empty;
            this.rootSeed = rootSeed;
            this.generationVersion = generationVersion;
            this.selectedBaseItemId = selectedBaseItemId ?? string.Empty;
            this.selectedRarity = selectedRarity;
            this.generatedInstance = generatedInstance;
        }

        public string schemaId { get; }
        public string dropAlgorithmId { get; }
        public string generationDataStatus { get; }
        public string dropRequestId { get; }
        public string sourceContextId { get; }
        public string sourceKey { get; }
        public int stageNumber { get; }
        public string candidatePoolId { get; }
        public string rarityPolicySource { get; }
        public string rarityWeightProfileId { get; }
        public long rootSeed { get; }
        public int generationVersion { get; }
        public string selectedBaseItemId { get; }
        public ItemInstanceRarity selectedRarity { get; }
        public ItemGeneratedInstanceSnapshot generatedInstance { get; }

        public string BuildCanonicalSignature()
        {
            StringBuilder builder = new();
            Append(builder, "schemaId", schemaId);
            Append(builder, "dropAlgorithmId", dropAlgorithmId);
            Append(builder, "generationDataStatus", generationDataStatus);
            Append(builder, "dropRequestId", dropRequestId);
            Append(builder, "sourceContextId", sourceContextId);
            Append(builder, "sourceKey", sourceKey);
            Append(builder, "stageNumber", stageNumber.ToString(CultureInfo.InvariantCulture));
            Append(builder, "candidatePoolId", candidatePoolId);
            Append(builder, "rarityPolicySource", rarityPolicySource);
            Append(builder, "rarityWeightProfileId", rarityWeightProfileId);
            Append(builder, "rootSeed", rootSeed.ToString(CultureInfo.InvariantCulture));
            Append(builder, "generationVersion", generationVersion.ToString(CultureInfo.InvariantCulture));
            Append(builder, "selectedBaseItemId", selectedBaseItemId);
            Append(builder, "selectedRarity", selectedRarity.ToStableKey());
            Append(builder, "generatedInstance", generatedInstance?.BuildCanonicalSignature() ?? string.Empty);
            return builder.ToString();
        }

        private static void Append(StringBuilder builder, string key, string value)
        {
            string safe = value ?? string.Empty;
            builder.Append(key).Append('=').Append(safe.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(safe).Append('\n');
        }
    }

    public sealed class ItemDropGenerationResult
    {
        private readonly ReadOnlyCollection<ItemDropGenerationValidationError> validationErrors;

        internal ItemDropGenerationResult(
            ItemDropGenerationSnapshot snapshot,
            IEnumerable<ItemDropGenerationValidationError> validationErrors)
        {
            this.snapshot = snapshot;
            this.validationErrors = Array.AsReadOnly((validationErrors
                ?? Array.Empty<ItemDropGenerationValidationError>()).ToArray());
        }

        public bool isSuccess => snapshot != null && validationErrors.Count == 0;
        public ItemDropGenerationSnapshot snapshot { get; }
        public IReadOnlyList<ItemDropGenerationValidationError> ValidationErrors => validationErrors;
        public string primaryValidationCode => validationErrors.Count == 0
            ? ItemDropGenerationValidationCodes.None
            : validationErrors[0].code;
    }
}
