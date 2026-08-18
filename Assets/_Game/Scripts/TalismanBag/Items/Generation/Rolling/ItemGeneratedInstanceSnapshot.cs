using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using TalismanBag.Items.Generation.Affixes;
using TalismanBag.Items.Generation.Potential;
using TalismanBag.Items.Generation.Stats;

namespace TalismanBag.Items.Generation.Rolling
{
    public static class ItemGenerationDataStatus
    {
        public const string QaFixtureOnly = "QA_FIXTURE_ONLY";
        public const string NotBalanceApproved = "NOT_BALANCE_APPROVED";
        public const string NotFormalGenerationData = "NOT_FORMAL_GENERATION_DATA";
        public const string QaFixtureCanonical =
            QaFixtureOnly + "|" + NotBalanceApproved + "|" + NotFormalGenerationData;
        public const string PlaytestV1Canonical = "PLAYTEST_V1_CANONICAL";
    }

    public static class ItemInstanceRollValidationCodes
    {
        public const string None = "NONE";
        public const string RequestNull = "REQUEST_NULL";
        public const string IdentityNull = "IDENTITY_NULL";
        public const string GenerationVersionUnsupported = "GENERATION_VERSION_UNSUPPORTED";
        public const string GenerationDataStatusEmpty = "GENERATION_DATA_STATUS_EMPTY";
        public const string GenerationDataStatusInvalid = "GENERATION_DATA_STATUS_INVALID";
        public const string FoundationIdentityInvalid = "FOUNDATION_IDENTITY_INVALID";
        public const string StatSchemaNull = "STAT_SCHEMA_NULL";
        public const string StatSchemaInvalid = "STAT_SCHEMA_INVALID";
        public const string StatProfileMissing = "STAT_PROFILE_MISSING";
        public const string StatRarityRangeMissing = "STAT_RARITY_RANGE_MISSING";
        public const string AffixSchemaNull = "AFFIX_SCHEMA_NULL";
        public const string AffixSchemaInvalid = "AFFIX_SCHEMA_INVALID";
        public const string AffixGenerationProfileMissing = "AFFIX_GENERATION_PROFILE_MISSING";
        public const string AffixSlotPolicyMissing = "AFFIX_SLOT_POLICY_MISSING";
        public const string AffixPoolMissing = "AFFIX_POOL_MISSING";
        public const string AffixValueProfileMissing = "AFFIX_VALUE_PROFILE_MISSING";
        public const string AffixRarityRangeMissing = "AFFIX_RARITY_RANGE_MISSING";
        public const string AffixCandidateSetEmpty = "AFFIX_CANDIDATE_SET_EMPTY";
        public const string AffixWeightInvalid = "AFFIX_WEIGHT_INVALID";
        public const string AffixWeightSumOverflow = "AFFIX_WEIGHT_SUM_OVERFLOW";
        public const string RangeInvalid = "RANGE_INVALID";
        public const string RangeStepInvalid = "RANGE_STEP_INVALID";
        public const string RangeCardinalityUnsupported = "RANGE_CARDINALITY_UNSUPPORTED";
        public const string CoreSchemaNull = "CORE_SCHEMA_NULL";
        public const string CoreSchemaInvalid = "CORE_SCHEMA_INVALID";
        public const string CoreProfileMissing = "CORE_PROFILE_MISSING";
        public const string CoreProfileIdentityMismatch = "CORE_PROFILE_IDENTITY_MISMATCH";
        public const string CoreProfileRarityMismatch = "CORE_PROFILE_RARITY_MISMATCH";
        public const string BuildPolicyUnresolved = "BUILD_POLICY_UNRESOLVED";
        public const string BuildRollProfileMissing = "BUILD_ROLL_PROFILE_MISSING";
        public const string BuildRollProfileRarityMismatch = "BUILD_ROLL_PROFILE_RARITY_MISMATCH";
        public const string BuildRollEntryInvalid = "BUILD_ROLL_ENTRY_INVALID";
        public const string BuildWeightSumOverflow = "BUILD_WEIGHT_SUM_OVERFLOW";
        public const string CanonicalDomainInvalid = "CANONICAL_DOMAIN_INVALID";
    }

    public sealed class ItemInstanceRollValidationError
    {
        public ItemInstanceRollValidationError(string code, string message)
        {
            this.code = code ?? string.Empty;
            this.message = message ?? string.Empty;
        }

        public string code { get; }
        public string message { get; }
    }

    public sealed class ItemBuildQualificationRollEntry
    {
        public ItemBuildQualificationRollEntry(ItemBuildQualification qualification, long weightUnits)
        {
            this.qualification = qualification;
            this.weightUnits = weightUnits;
        }

        public ItemBuildQualification qualification { get; }
        public long weightUnits { get; }
    }

    public sealed class ItemBuildQualificationRollProfile
    {
        private readonly ReadOnlyCollection<ItemBuildQualificationRollEntry> entries;

        public ItemBuildQualificationRollProfile(
            string profileId,
            ItemInstanceRarity rarity,
            string dataMaturityKey,
            IEnumerable<ItemBuildQualificationRollEntry> entries)
        {
            this.profileId = Normalize(profileId);
            this.rarity = rarity;
            this.dataMaturityKey = Normalize(dataMaturityKey);
            this.entries = Freeze((entries ?? Array.Empty<ItemBuildQualificationRollEntry>())
                .Select(entry => entry == null
                    ? null
                    : new ItemBuildQualificationRollEntry(entry.qualification, entry.weightUnits))
                .OrderBy(entry => entry == null ? int.MinValue : (int)entry.qualification)
                .ThenBy(entry => entry == null ? long.MinValue : entry.weightUnits));
        }

        public string profileId { get; }
        public ItemInstanceRarity rarity { get; }
        public string dataMaturityKey { get; }
        public IReadOnlyList<ItemBuildQualificationRollEntry> Entries => entries;

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private static ReadOnlyCollection<T> Freeze<T>(IEnumerable<T> values)
        {
            return Array.AsReadOnly((values ?? Array.Empty<T>()).ToArray());
        }
    }

    public sealed class ItemInstanceRollRequest
    {
        public ItemInstanceRollRequest(
            ItemInstanceIdentitySnapshot identity,
            ItemStatRangeSchemaSnapshot statSchema,
            ItemAffixPoolAndRangeSchemaSnapshot affixSchema,
            ItemCorePotentialAndBuildEligibilitySchemaSnapshot coreBuildSchema,
            ItemBuildQualificationRollProfile buildQualificationRollProfile,
            string generationDataStatus)
        {
            this.identity = identity;
            this.statSchema = statSchema;
            this.affixSchema = affixSchema;
            this.coreBuildSchema = coreBuildSchema;
            this.buildQualificationRollProfile = buildQualificationRollProfile;
            this.generationDataStatus = Normalize(generationDataStatus);
        }

        public ItemInstanceIdentitySnapshot identity { get; }
        public ItemStatRangeSchemaSnapshot statSchema { get; }
        public ItemAffixPoolAndRangeSchemaSnapshot affixSchema { get; }
        public ItemCorePotentialAndBuildEligibilitySchemaSnapshot coreBuildSchema { get; }
        public ItemBuildQualificationRollProfile buildQualificationRollProfile { get; }
        public string generationDataStatus { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    public sealed class ItemGeneratedStatSnapshot
    {
        internal ItemGeneratedStatSnapshot(string statId, long rawUnits)
        {
            this.statId = statId ?? string.Empty;
            this.rawUnits = rawUnits;
        }

        public string statId { get; }
        public long rawUnits { get; }
    }

    public sealed class ItemGeneratedAffixSnapshot
    {
        internal ItemGeneratedAffixSnapshot(
            string slotId,
            ItemAffixSlotKind slotKind,
            string affixId,
            string affixValueProfileId,
            long rawUnits)
        {
            this.slotId = slotId ?? string.Empty;
            this.slotKind = slotKind;
            this.affixId = affixId ?? string.Empty;
            this.affixValueProfileId = affixValueProfileId ?? string.Empty;
            this.rawUnits = rawUnits;
        }

        public string slotId { get; }
        public ItemAffixSlotKind slotKind { get; }
        public string affixId { get; }
        public string affixValueProfileId { get; }
        public long rawUnits { get; }
    }

    public sealed class ItemGeneratedCorePotentialSnapshot
    {
        private readonly ReadOnlyCollection<string> eligibleCoreEffectIds;
        private readonly ReadOnlyCollection<string> visibleCoreEffectIds;

        internal ItemGeneratedCorePotentialSnapshot(
            string cultivationPotentialProfileId,
            IEnumerable<string> eligibleCoreEffectIds,
            IEnumerable<string> visibleCoreEffectIds)
        {
            this.cultivationPotentialProfileId = cultivationPotentialProfileId ?? string.Empty;
            this.eligibleCoreEffectIds = Freeze((eligibleCoreEffectIds ?? Array.Empty<string>())
                .OrderBy(value => value, StringComparer.Ordinal));
            this.visibleCoreEffectIds = Freeze((visibleCoreEffectIds ?? Array.Empty<string>())
                .OrderBy(value => value, StringComparer.Ordinal));
        }

        public string cultivationPotentialProfileId { get; }
        public IReadOnlyList<string> EligibleCoreEffectIds => eligibleCoreEffectIds;
        public IReadOnlyList<string> VisibleCoreEffectIds => visibleCoreEffectIds;

        private static ReadOnlyCollection<T> Freeze<T>(IEnumerable<T> values)
        {
            return Array.AsReadOnly((values ?? Array.Empty<T>()).ToArray());
        }
    }

    public sealed class ItemGeneratedInstanceSnapshot
    {
        public const string CurrentSchemaId = "ItemGeneratedInstanceSnapshot.v1";

        private readonly ReadOnlyCollection<ItemGeneratedStatSnapshot> generatedStats;
        private readonly ReadOnlyCollection<ItemGeneratedAffixSnapshot> generatedAffixes;

        internal ItemGeneratedInstanceSnapshot(
            string generationDataStatus,
            ItemInstanceIdentitySnapshot identity,
            IEnumerable<ItemGeneratedStatSnapshot> generatedStats,
            IEnumerable<ItemGeneratedAffixSnapshot> generatedAffixes,
            ItemGeneratedCorePotentialSnapshot generatedCorePotential,
            ItemBuildQualification buildQualification)
        {
            this.identity = identity ?? throw new ArgumentNullException(nameof(identity));
            schemaId = CurrentSchemaId;
            generationAlgorithmId = DeterministicItemRandom.AlgorithmId;
            this.generationDataStatus = generationDataStatus ?? string.Empty;
            itemInstanceId = identity.itemInstanceId;
            baseItemId = identity.baseItemId;
            rarity = identity.rarity;
            generationVersion = identity.generationVersion;
            rootSeed = identity.rootSeed;
            cultivationPotentialProfileId = generatedCorePotential?.cultivationPotentialProfileId
                ?? identity.cultivationPotentialProfileId;
            this.generatedStats = Freeze((generatedStats ?? Array.Empty<ItemGeneratedStatSnapshot>())
                .OrderBy(stat => stat.statId, StringComparer.Ordinal)
                .Select(stat => new ItemGeneratedStatSnapshot(stat.statId, stat.rawUnits)));
            this.generatedAffixes = Freeze((generatedAffixes ?? Array.Empty<ItemGeneratedAffixSnapshot>())
                .OrderBy(affix => affix.slotId, StringComparer.Ordinal)
                .ThenBy(affix => (int)affix.slotKind)
                .ThenBy(affix => affix.affixId, StringComparer.Ordinal)
                .Select(affix => new ItemGeneratedAffixSnapshot(affix.slotId, affix.slotKind,
                    affix.affixId, affix.affixValueProfileId, affix.rawUnits)));
            GeneratedCorePotential = generatedCorePotential == null
                ? null
                : new ItemGeneratedCorePotentialSnapshot(
                    generatedCorePotential.cultivationPotentialProfileId,
                    generatedCorePotential.EligibleCoreEffectIds,
                    generatedCorePotential.VisibleCoreEffectIds);
            this.buildQualification = buildQualification;
        }

        public string schemaId { get; }
        public ItemInstanceIdentitySnapshot identity { get; }
        public string generationAlgorithmId { get; }
        public string generationDataStatus { get; }
        public string itemInstanceId { get; }
        public string baseItemId { get; }
        public ItemInstanceRarity rarity { get; }
        public int generationVersion { get; }
        public long rootSeed { get; }
        public string cultivationPotentialProfileId { get; }
        public IReadOnlyList<ItemGeneratedStatSnapshot> GeneratedStats => generatedStats;
        public IReadOnlyList<ItemGeneratedAffixSnapshot> GeneratedAffixes => generatedAffixes;
        public ItemGeneratedCorePotentialSnapshot GeneratedCorePotential { get; }
        public ItemBuildQualification buildQualification { get; }

        public string BuildCanonicalSignature()
        {
            StringBuilder builder = new();
            AppendField(builder, "schemaId", schemaId);
            AppendField(builder, "generationAlgorithmId", generationAlgorithmId);
            AppendField(builder, "generationDataStatus", generationDataStatus);
            AppendField(builder, "itemInstanceId", itemInstanceId);
            AppendField(builder, "baseItemId", baseItemId);
            AppendField(builder, "rarity", rarity.ToStableKey());
            AppendField(builder, "generationVersion", generationVersion.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "rootSeed", rootSeed.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "cultivationPotentialProfileId", cultivationPotentialProfileId);
            foreach (ItemGeneratedStatSnapshot stat in generatedStats)
            {
                AppendField(builder, "statId", stat.statId);
                AppendField(builder, "statRawUnits", stat.rawUnits.ToString(CultureInfo.InvariantCulture));
            }

            foreach (ItemGeneratedAffixSnapshot affix in generatedAffixes)
            {
                AppendField(builder, "slotId", affix.slotId);
                AppendField(builder, "slotKind", affix.slotKind.ToString());
                AppendField(builder, "affixId", affix.affixId);
                AppendField(builder, "affixValueProfileId", affix.affixValueProfileId);
                AppendField(builder, "affixRawUnits", affix.rawUnits.ToString(CultureInfo.InvariantCulture));
            }

            foreach (string effectId in GeneratedCorePotential?.EligibleCoreEffectIds ?? Array.Empty<string>())
            {
                AppendField(builder, "eligibleCoreEffectId", effectId);
            }

            foreach (string effectId in GeneratedCorePotential?.VisibleCoreEffectIds ?? Array.Empty<string>())
            {
                AppendField(builder, "visibleCoreEffectId", effectId);
            }

            AppendField(builder, "buildQualification", buildQualification.ToString());
            return builder.ToString();
        }

        private static void AppendField(StringBuilder builder, string key, string value)
        {
            string safe = value ?? string.Empty;
            builder.Append(key).Append('=').Append(safe.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(safe).Append('\n');
        }

        private static ReadOnlyCollection<T> Freeze<T>(IEnumerable<T> values)
        {
            return Array.AsReadOnly((values ?? Array.Empty<T>()).ToArray());
        }
    }

    public sealed class ItemInstanceRollResult
    {
        private readonly ReadOnlyCollection<ItemInstanceRollValidationError> validationErrors;

        internal ItemInstanceRollResult(
            ItemGeneratedInstanceSnapshot snapshot,
            IEnumerable<ItemInstanceRollValidationError> validationErrors)
        {
            this.snapshot = snapshot;
            this.validationErrors = Array.AsReadOnly((validationErrors
                ?? Array.Empty<ItemInstanceRollValidationError>()).ToArray());
        }

        public bool isSuccess => snapshot != null && validationErrors.Count == 0;
        public ItemGeneratedInstanceSnapshot snapshot { get; }
        public IReadOnlyList<ItemInstanceRollValidationError> ValidationErrors => validationErrors;
        public string primaryValidationCode => validationErrors.Count == 0
            ? ItemInstanceRollValidationCodes.None
            : validationErrors[0].code;
    }
}
