using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace TalismanBag.Items.Generation.Potential
{
    public enum ItemCorePotentialEffectKind
    {
        Standard,
        Ultimate
    }

    public enum ItemCorePotentialResolutionStatus
    {
        Unresolved,
        Defined
    }

    public enum ItemBuildQualification
    {
        Unresolved,
        None,
        FaMenOnly,
        QiLeiOnly,
        Dual
    }

    public enum ItemBuildQualificationPolicyMode
    {
        LockedNone,
        ProbabilityUnresolved,
        DefinedProbabilityTable
    }

    public static class ItemCorePotentialAndBuildValidationCodes
    {
        public const string None = "NONE";
        public const string CorePolicyNull = "CORE_POLICY_NULL";
        public const string CorePolicyDuplicate = "CORE_POLICY_DUPLICATE";
        public const string CorePolicyMissing = "CORE_POLICY_MISSING";
        public const string CoreUltimatePolicyInvalid = "CORE_ULTIMATE_POLICY_INVALID";
        public const string ProfileNull = "CORE_PROFILE_NULL";
        public const string ProfileIdEmpty = "CORE_PROFILE_ID_EMPTY";
        public const string ProfileIdDuplicate = "CORE_PROFILE_ID_DUPLICATE";
        public const string ProfileDuplicate = "CORE_PROFILE_DUPLICATE";
        public const string ProfilePolicyMissing = "CORE_PROFILE_POLICY_MISSING";
        public const string ResolutionStatusInvalid = "RESOLUTION_STATUS_INVALID";
        public const string DataMaturityKeyEmpty = "DATA_MATURITY_KEY_EMPTY";
        public const string BaseItemIdEmpty = "BASE_ITEM_ID_EMPTY";
        public const string BaseItemUnknown = "BASE_ITEM_UNKNOWN";
        public const string BaseItemNotOrdinary = "BASE_ITEM_NOT_ORDINARY";
        public const string UnresolvedProfileHasEffects = "UNRESOLVED_PROFILE_HAS_EFFECTS";
        public const string CoreEffectNull = "CORE_EFFECT_NULL";
        public const string CoreEffectIdEmpty = "CORE_EFFECT_ID_EMPTY";
        public const string CoreEffectIdDuplicate = "CORE_EFFECT_ID_DUPLICATE";
        public const string CoreEffectKindInvalid = "CORE_EFFECT_KIND_INVALID";
        public const string VisibleEffectNotEligible = "VISIBLE_EFFECT_NOT_ELIGIBLE";
        public const string UltimateEffectNotAllowed = "ULTIMATE_EFFECT_NOT_ALLOWED";
        public const string BuildPolicyNull = "BUILD_POLICY_NULL";
        public const string BuildPolicyDuplicate = "BUILD_POLICY_DUPLICATE";
        public const string BuildPolicyMissing = "BUILD_POLICY_MISSING";
        public const string BuildPolicyModeInvalid = "BUILD_POLICY_MODE_INVALID";
        public const string BuildPolicyContractInvalid = "BUILD_POLICY_CONTRACT_INVALID";
        public const string ProbabilityProfileForbidden = "PROBABILITY_PROFILE_FORBIDDEN";
        public const string RarityInvalid = "RARITY_INVALID";
        public const string QualificationSnapshotNull = "QUALIFICATION_SNAPSHOT_NULL";
        public const string ItemInstanceIdEmpty = "ITEM_INSTANCE_ID_EMPTY";
        public const string QualificationInvalid = "QUALIFICATION_INVALID";
        public const string WhiteQualificationMustBeNone = "WHITE_QUALIFICATION_MUST_BE_NONE";
        public const string QueryInputInvalid = "QUERY_INPUT_INVALID";
        public const string QueryCorePolicyNotFound = "QUERY_CORE_POLICY_NOT_FOUND";
        public const string QueryCoreProfileNotFound = "QUERY_CORE_PROFILE_NOT_FOUND";
        public const string QueryBuildPolicyNotFound = "QUERY_BUILD_POLICY_NOT_FOUND";
    }

    public sealed class ItemCorePotentialAndBuildValidationError
    {
        public ItemCorePotentialAndBuildValidationError(string code, string message)
        {
            this.code = code ?? string.Empty;
            this.message = message ?? string.Empty;
        }

        public string code { get; }
        public string message { get; }
    }

    public sealed class ItemCorePotentialQueryResult<T> where T : class
    {
        private ItemCorePotentialQueryResult(T value, ItemCorePotentialAndBuildValidationError validationError)
        {
            this.value = value;
            this.validationError = validationError;
        }

        public bool isSuccess => value != null && validationError == null;
        public T value { get; }
        public ItemCorePotentialAndBuildValidationError validationError { get; }

        internal static ItemCorePotentialQueryResult<T> Success(T value)
        {
            return new ItemCorePotentialQueryResult<T>(value, null);
        }

        internal static ItemCorePotentialQueryResult<T> Failure(string code, string message)
        {
            return new ItemCorePotentialQueryResult<T>(
                null,
                new ItemCorePotentialAndBuildValidationError(code, message));
        }
    }

    public sealed class ItemCorePotentialEffectSnapshot
    {
        public ItemCorePotentialEffectSnapshot(
            string coreEffectId,
            ItemCorePotentialEffectKind effectKind,
            bool isVisibleAtDrop)
        {
            this.coreEffectId = Normalize(coreEffectId);
            this.effectKind = effectKind;
            this.isVisibleAtDrop = isVisibleAtDrop;
        }

        public string coreEffectId { get; }
        public ItemCorePotentialEffectKind effectKind { get; }
        public bool isVisibleAtDrop { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    public sealed class ItemCorePotentialProfileSnapshot
    {
        private readonly ReadOnlyCollection<ItemCorePotentialEffectSnapshot> potentialEffects;
        private readonly ReadOnlyCollection<string> eligibleCoreEffectIds;
        private readonly ReadOnlyCollection<string> visibleCoreEffectIds;

        public ItemCorePotentialProfileSnapshot(
            string cultivationPotentialProfileId,
            string baseItemId,
            ItemInstanceRarity rarity,
            ItemCorePotentialResolutionStatus resolutionStatus,
            string dataMaturityKey,
            IEnumerable<ItemCorePotentialEffectSnapshot> potentialEffects)
            : this(cultivationPotentialProfileId, baseItemId, rarity, resolutionStatus,
                dataMaturityKey, potentialEffects, null)
        {
        }

        public ItemCorePotentialProfileSnapshot(
            string cultivationPotentialProfileId,
            string baseItemId,
            ItemInstanceRarity rarity,
            ItemCorePotentialResolutionStatus resolutionStatus,
            string dataMaturityKey,
            IEnumerable<ItemCorePotentialEffectSnapshot> potentialEffects,
            IEnumerable<string> visibleCoreEffectIds)
        {
            this.cultivationPotentialProfileId = Normalize(cultivationPotentialProfileId);
            this.baseItemId = Normalize(baseItemId);
            this.rarity = rarity;
            this.resolutionStatus = resolutionStatus;
            this.dataMaturityKey = Normalize(dataMaturityKey);
            this.potentialEffects = Freeze((potentialEffects ?? Array.Empty<ItemCorePotentialEffectSnapshot>())
                .Select(CloneEffect)
                .OrderBy(effect => effect?.coreEffectId ?? string.Empty, StringComparer.Ordinal)
                .ThenBy(effect => effect == null ? -1 : (int)effect.effectKind)
                .ThenBy(effect => effect?.isVisibleAtDrop == true));
            eligibleCoreEffectIds = Freeze(this.potentialEffects
                .Where(effect => effect != null && !string.IsNullOrWhiteSpace(effect.coreEffectId))
                .Select(effect => effect.coreEffectId)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(id => id, StringComparer.Ordinal));
            IEnumerable<string> visibleSource = visibleCoreEffectIds ?? this.potentialEffects
                .Where(effect => effect != null && effect.isVisibleAtDrop)
                .Select(effect => effect.coreEffectId);
            this.visibleCoreEffectIds = Freeze((visibleSource ?? Array.Empty<string>())
                .Select(Normalize)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(id => id, StringComparer.Ordinal));
        }

        public string cultivationPotentialProfileId { get; }
        public string baseItemId { get; }
        public ItemInstanceRarity rarity { get; }
        public ItemCorePotentialResolutionStatus resolutionStatus { get; }
        public string dataMaturityKey { get; }
        public IReadOnlyList<ItemCorePotentialEffectSnapshot> PotentialEffects => potentialEffects;
        public IReadOnlyList<string> EligibleCoreEffectIds => eligibleCoreEffectIds;
        public IReadOnlyList<string> VisibleCoreEffectIds => visibleCoreEffectIds;

        private static ItemCorePotentialEffectSnapshot CloneEffect(ItemCorePotentialEffectSnapshot effect)
        {
            return effect == null
                ? null
                : new ItemCorePotentialEffectSnapshot(effect.coreEffectId, effect.effectKind, effect.isVisibleAtDrop);
        }

        private static ReadOnlyCollection<T> Freeze<T>(IEnumerable<T> values)
        {
            return Array.AsReadOnly((values ?? Array.Empty<T>()).ToArray());
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    public sealed class ItemCoreRarityPolicySnapshot
    {
        public ItemCoreRarityPolicySnapshot(
            ItemInstanceRarity rarity,
            ItemCorePotentialResolutionStatus resolutionStatus,
            bool allowsUltimateCoreEffect,
            string dataMaturityKey)
        {
            this.rarity = rarity;
            this.resolutionStatus = resolutionStatus;
            this.allowsUltimateCoreEffect = allowsUltimateCoreEffect;
            this.dataMaturityKey = Normalize(dataMaturityKey);
        }

        public ItemInstanceRarity rarity { get; }
        public ItemCorePotentialResolutionStatus resolutionStatus { get; }
        public bool allowsUltimateCoreEffect { get; }
        public string dataMaturityKey { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    public sealed class ItemBuildQualificationRarityPolicySnapshot
    {
        public ItemBuildQualificationRarityPolicySnapshot(
            ItemInstanceRarity rarity,
            ItemBuildQualificationPolicyMode policyMode,
            string probabilityProfileId,
            string dataMaturityKey)
        {
            this.rarity = rarity;
            this.policyMode = policyMode;
            this.probabilityProfileId = Normalize(probabilityProfileId);
            this.dataMaturityKey = Normalize(dataMaturityKey);
        }

        public ItemInstanceRarity rarity { get; }
        public ItemBuildQualificationPolicyMode policyMode { get; }
        public string probabilityProfileId { get; }
        public string dataMaturityKey { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    public sealed class ItemBuildQualificationSnapshot
    {
        public ItemBuildQualificationSnapshot(
            string itemInstanceId,
            string baseItemId,
            ItemInstanceRarity rarity,
            ItemBuildQualification qualification)
        {
            this.itemInstanceId = Normalize(itemInstanceId);
            this.baseItemId = Normalize(baseItemId);
            this.rarity = rarity;
            this.qualification = qualification;
        }

        public string itemInstanceId { get; }
        public string baseItemId { get; }
        public ItemInstanceRarity rarity { get; }
        public ItemBuildQualification qualification { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    public sealed class ItemBuildQualificationValidationResult
    {
        private readonly ReadOnlyCollection<ItemCorePotentialAndBuildValidationError> validationErrors;

        internal ItemBuildQualificationValidationResult(
            ItemBuildQualificationSnapshot snapshot,
            IEnumerable<ItemCorePotentialAndBuildValidationError> validationErrors)
        {
            this.snapshot = snapshot == null
                ? null
                : new ItemBuildQualificationSnapshot(snapshot.itemInstanceId, snapshot.baseItemId,
                    snapshot.rarity, snapshot.qualification);
            this.validationErrors = Array.AsReadOnly((validationErrors
                ?? Array.Empty<ItemCorePotentialAndBuildValidationError>())
                .OrderBy(error => error.code, StringComparer.Ordinal)
                .ThenBy(error => error.message, StringComparer.Ordinal)
                .ToArray());
        }

        public bool isValid => snapshot != null && validationErrors.Count == 0;
        public ItemBuildQualificationSnapshot snapshot { get; }
        public IReadOnlyList<ItemCorePotentialAndBuildValidationError> ValidationErrors => validationErrors;
        public string primaryValidationCode => validationErrors.Count == 0
            ? ItemCorePotentialAndBuildValidationCodes.None
            : validationErrors[0].code;
    }

    public sealed class ItemCorePotentialAndBuildEligibilitySchemaSnapshot
    {
        public const string CurrentSchemaId = "ItemCorePotentialAndBuildEligibilitySchemaSnapshot.v1";

        private readonly ReadOnlyCollection<ItemCoreRarityPolicySnapshot> coreRarityPolicies;
        private readonly ReadOnlyCollection<ItemCorePotentialProfileSnapshot> corePotentialProfiles;
        private readonly ReadOnlyCollection<ItemBuildQualificationRarityPolicySnapshot> buildRarityPolicies;
        private readonly ReadOnlyCollection<ItemCorePotentialAndBuildValidationError> validationErrors;
        private readonly IReadOnlyDictionary<ItemInstanceRarity, ItemCoreRarityPolicySnapshot> corePoliciesByRarity;
        private readonly IReadOnlyDictionary<string, ItemCorePotentialProfileSnapshot> profilesById;
        private readonly IReadOnlyDictionary<string, ItemCorePotentialProfileSnapshot> profilesByBaseAndRarity;
        private readonly IReadOnlyDictionary<ItemInstanceRarity, ItemBuildQualificationRarityPolicySnapshot> buildPoliciesByRarity;

        internal ItemCorePotentialAndBuildEligibilitySchemaSnapshot(
            IEnumerable<ItemCoreRarityPolicySnapshot> corePolicies,
            IEnumerable<ItemCorePotentialProfileSnapshot> profiles,
            IEnumerable<ItemBuildQualificationRarityPolicySnapshot> buildPolicies,
            IEnumerable<ItemCorePotentialAndBuildValidationError> errors)
        {
            schemaId = CurrentSchemaId;
            coreRarityPolicies = Freeze((corePolicies ?? Array.Empty<ItemCoreRarityPolicySnapshot>())
                .OrderBy(policy => policy.rarity.ToTierIndex())
                .ThenBy(policy => (int)policy.rarity)
                .ThenBy(policy => (int)policy.resolutionStatus)
                .ThenBy(policy => policy.allowsUltimateCoreEffect)
                .ThenBy(policy => policy.dataMaturityKey, StringComparer.Ordinal));
            corePotentialProfiles = Freeze((profiles ?? Array.Empty<ItemCorePotentialProfileSnapshot>())
                .OrderBy(profile => profile.baseItemId, StringComparer.Ordinal)
                .ThenBy(profile => profile.rarity.ToTierIndex())
                .ThenBy(profile => (int)profile.rarity)
                .ThenBy(profile => profile.cultivationPotentialProfileId, StringComparer.Ordinal));
            buildRarityPolicies = Freeze((buildPolicies ?? Array.Empty<ItemBuildQualificationRarityPolicySnapshot>())
                .OrderBy(policy => policy.rarity.ToTierIndex())
                .ThenBy(policy => (int)policy.rarity)
                .ThenBy(policy => (int)policy.policyMode)
                .ThenBy(policy => policy.probabilityProfileId, StringComparer.Ordinal));
            validationErrors = Freeze((errors ?? Array.Empty<ItemCorePotentialAndBuildValidationError>())
                .OrderBy(error => error.code, StringComparer.Ordinal)
                .ThenBy(error => error.message, StringComparer.Ordinal));
            corePoliciesByRarity = coreRarityPolicies
                .Where(policy => ItemInstanceRarityCatalog.TryGetDefinition(policy.rarity, out _))
                .GroupBy(policy => policy.rarity)
                .ToDictionary(group => group.Key, group => group.First());
            profilesById = corePotentialProfiles
                .Where(profile => !string.IsNullOrWhiteSpace(profile.cultivationPotentialProfileId))
                .GroupBy(profile => profile.cultivationPotentialProfileId, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
            profilesByBaseAndRarity = corePotentialProfiles
                .GroupBy(ProfileKey, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
            buildPoliciesByRarity = buildRarityPolicies
                .Where(policy => ItemInstanceRarityCatalog.TryGetDefinition(policy.rarity, out _))
                .GroupBy(policy => policy.rarity)
                .ToDictionary(group => group.Key, group => group.First());
        }

        public string schemaId { get; }
        public bool isValid => validationErrors.Count == 0;
        public IReadOnlyList<ItemCoreRarityPolicySnapshot> CoreRarityPolicies => coreRarityPolicies;
        public IReadOnlyList<ItemCorePotentialProfileSnapshot> CorePotentialProfiles => corePotentialProfiles;
        public IReadOnlyList<ItemBuildQualificationRarityPolicySnapshot> BuildRarityPolicies => buildRarityPolicies;
        public IReadOnlyList<ItemCorePotentialAndBuildValidationError> ValidationErrors => validationErrors;

        public ItemCorePotentialQueryResult<ItemCoreRarityPolicySnapshot> QueryCoreRarityPolicy(
            ItemInstanceRarity rarity)
        {
            if (!ItemInstanceRarityCatalog.TryGetDefinition(rarity, out _))
            {
                return ItemCorePotentialQueryResult<ItemCoreRarityPolicySnapshot>.Failure(
                    ItemCorePotentialAndBuildValidationCodes.QueryInputInvalid,
                    $"Unsupported rarity value {(int)rarity}.");
            }

            return corePoliciesByRarity.TryGetValue(rarity, out ItemCoreRarityPolicySnapshot policy)
                ? ItemCorePotentialQueryResult<ItemCoreRarityPolicySnapshot>.Success(policy)
                : ItemCorePotentialQueryResult<ItemCoreRarityPolicySnapshot>.Failure(
                    ItemCorePotentialAndBuildValidationCodes.QueryCorePolicyNotFound,
                    $"Core rarity policy '{rarity.ToStableKey()}' was not found.");
        }

        public ItemCorePotentialQueryResult<ItemCorePotentialProfileSnapshot> QueryCorePotentialProfile(
            string cultivationPotentialProfileId)
        {
            string normalized = Normalize(cultivationPotentialProfileId);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return ItemCorePotentialQueryResult<ItemCorePotentialProfileSnapshot>.Failure(
                    ItemCorePotentialAndBuildValidationCodes.QueryInputInvalid,
                    "cultivationPotentialProfileId is required.");
            }

            return profilesById.TryGetValue(normalized, out ItemCorePotentialProfileSnapshot profile)
                ? ItemCorePotentialQueryResult<ItemCorePotentialProfileSnapshot>.Success(profile)
                : ItemCorePotentialQueryResult<ItemCorePotentialProfileSnapshot>.Failure(
                    ItemCorePotentialAndBuildValidationCodes.QueryCoreProfileNotFound,
                    $"Core potential profile '{normalized}' was not found.");
        }

        public ItemCorePotentialQueryResult<ItemCorePotentialProfileSnapshot> QueryCorePotentialProfile(
            string baseItemId,
            ItemInstanceRarity rarity)
        {
            string normalizedBaseItemId = Normalize(baseItemId);
            if (string.IsNullOrWhiteSpace(normalizedBaseItemId)
                || !ItemInstanceRarityCatalog.TryGetDefinition(rarity, out _))
            {
                return ItemCorePotentialQueryResult<ItemCorePotentialProfileSnapshot>.Failure(
                    ItemCorePotentialAndBuildValidationCodes.QueryInputInvalid,
                    "A valid baseItemId and rarity are required.");
            }

            string key = ProfileKey(normalizedBaseItemId, rarity);
            return profilesByBaseAndRarity.TryGetValue(key, out ItemCorePotentialProfileSnapshot profile)
                ? ItemCorePotentialQueryResult<ItemCorePotentialProfileSnapshot>.Success(profile)
                : ItemCorePotentialQueryResult<ItemCorePotentialProfileSnapshot>.Failure(
                    ItemCorePotentialAndBuildValidationCodes.QueryCoreProfileNotFound,
                    $"Core potential profile '{key}' was not found.");
        }

        public ItemCorePotentialQueryResult<ItemBuildQualificationRarityPolicySnapshot> QueryBuildRarityPolicy(
            ItemInstanceRarity rarity)
        {
            if (!ItemInstanceRarityCatalog.TryGetDefinition(rarity, out _))
            {
                return ItemCorePotentialQueryResult<ItemBuildQualificationRarityPolicySnapshot>.Failure(
                    ItemCorePotentialAndBuildValidationCodes.QueryInputInvalid,
                    $"Unsupported rarity value {(int)rarity}.");
            }

            return buildPoliciesByRarity.TryGetValue(rarity, out ItemBuildQualificationRarityPolicySnapshot policy)
                ? ItemCorePotentialQueryResult<ItemBuildQualificationRarityPolicySnapshot>.Success(policy)
                : ItemCorePotentialQueryResult<ItemBuildQualificationRarityPolicySnapshot>.Failure(
                    ItemCorePotentialAndBuildValidationCodes.QueryBuildPolicyNotFound,
                    $"Build qualification policy '{rarity.ToStableKey()}' was not found.");
        }

        public string BuildCanonicalSignature()
        {
            StringBuilder builder = new();
            builder.Append(schemaId);
            foreach (ItemCoreRarityPolicySnapshot policy in coreRarityPolicies)
            {
                builder.Append("\nC|")
                    .Append(StableRarity(policy.rarity)).Append('|')
                    .Append(((int)policy.rarity).ToString(CultureInfo.InvariantCulture)).Append('|')
                    .Append(policy.resolutionStatus).Append('|')
                    .Append(policy.allowsUltimateCoreEffect ? '1' : '0').Append('|')
                    .Append(policy.dataMaturityKey);
            }

            foreach (ItemCorePotentialProfileSnapshot profile in corePotentialProfiles)
            {
                builder.Append("\nP|")
                    .Append(profile.cultivationPotentialProfileId).Append('|')
                    .Append(profile.baseItemId).Append('|')
                    .Append(StableRarity(profile.rarity)).Append('|')
                    .Append(((int)profile.rarity).ToString(CultureInfo.InvariantCulture)).Append('|')
                    .Append(profile.resolutionStatus).Append('|')
                    .Append(profile.dataMaturityKey).Append('|')
                    .Append(string.Join(";", profile.VisibleCoreEffectIds));
                foreach (ItemCorePotentialEffectSnapshot effect in profile.PotentialEffects)
                {
                    builder.Append("\nE|")
                        .Append(profile.cultivationPotentialProfileId).Append('|')
                        .Append(effect?.coreEffectId ?? "null").Append('|')
                        .Append(effect == null ? "null" : effect.effectKind.ToString()).Append('|')
                        .Append(effect?.isVisibleAtDrop == true ? '1' : '0');
                }
            }

            foreach (ItemBuildQualificationRarityPolicySnapshot policy in buildRarityPolicies)
            {
                builder.Append("\nB|")
                    .Append(StableRarity(policy.rarity)).Append('|')
                    .Append(((int)policy.rarity).ToString(CultureInfo.InvariantCulture)).Append('|')
                    .Append(policy.policyMode).Append('|')
                    .Append(policy.probabilityProfileId).Append('|')
                    .Append(policy.dataMaturityKey);
            }

            foreach (ItemCorePotentialAndBuildValidationError error in validationErrors)
            {
                builder.Append("\nV|").Append(error.code).Append('|').Append(error.message);
            }

            return builder.ToString();
        }

        private static ReadOnlyCollection<T> Freeze<T>(IEnumerable<T> values)
        {
            return Array.AsReadOnly((values ?? Array.Empty<T>()).ToArray());
        }

        private static string ProfileKey(ItemCorePotentialProfileSnapshot profile)
        {
            return ProfileKey(profile.baseItemId, profile.rarity);
        }

        private static string ProfileKey(string baseItemId, ItemInstanceRarity rarity)
        {
            return Normalize(baseItemId) + "@" + StableRarity(rarity);
        }

        private static string StableRarity(ItemInstanceRarity rarity)
        {
            string stable = rarity.ToStableKey();
            return string.IsNullOrWhiteSpace(stable)
                ? ((int)rarity).ToString(CultureInfo.InvariantCulture)
                : stable;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    public static class ItemCorePotentialAndBuildEligibilitySchema
    {
        public static ItemCorePotentialAndBuildEligibilitySchemaSnapshot Create(
            IEnumerable<ItemCoreRarityPolicySnapshot> corePolicies,
            IEnumerable<ItemCorePotentialProfileSnapshot> profiles,
            IEnumerable<ItemBuildQualificationRarityPolicySnapshot> buildPolicies)
        {
            List<ItemCorePotentialAndBuildValidationError> errors = new();
            List<ItemCoreRarityPolicySnapshot> stableCorePolicies = CloneCorePolicies(corePolicies, errors);
            List<ItemCorePotentialProfileSnapshot> stableProfiles = CloneProfiles(profiles, errors);
            List<ItemBuildQualificationRarityPolicySnapshot> stableBuildPolicies = CloneBuildPolicies(buildPolicies, errors);
            ValidateCorePolicies(stableCorePolicies, errors);
            ValidateProfiles(stableCorePolicies, stableProfiles, errors);
            ValidateBuildPolicies(stableBuildPolicies, errors);
            return new ItemCorePotentialAndBuildEligibilitySchemaSnapshot(
                stableCorePolicies, stableProfiles, stableBuildPolicies, errors);
        }

        public static ItemBuildQualificationValidationResult ValidateQualificationSnapshot(
            ItemBuildQualificationSnapshot snapshot)
        {
            List<ItemCorePotentialAndBuildValidationError> errors = new();
            if (snapshot == null)
            {
                errors.Add(Error(ItemCorePotentialAndBuildValidationCodes.QualificationSnapshotNull,
                    "Build qualification snapshot is null."));
                return new ItemBuildQualificationValidationResult(null, errors);
            }

            if (string.IsNullOrWhiteSpace(snapshot.itemInstanceId))
            {
                errors.Add(Error(ItemCorePotentialAndBuildValidationCodes.ItemInstanceIdEmpty,
                    "itemInstanceId is required."));
            }

            ValidateOrdinaryBaseItemId(snapshot.baseItemId, errors);
            if (!ItemInstanceRarityCatalog.TryGetDefinition(snapshot.rarity, out _))
            {
                errors.Add(Error(ItemCorePotentialAndBuildValidationCodes.RarityInvalid,
                    $"Unsupported rarity value {(int)snapshot.rarity}."));
            }

            if (!Enum.IsDefined(typeof(ItemBuildQualification), snapshot.qualification))
            {
                errors.Add(Error(ItemCorePotentialAndBuildValidationCodes.QualificationInvalid,
                    $"Unsupported qualification value {(int)snapshot.qualification}."));
            }
            else if (snapshot.rarity == ItemInstanceRarity.White
                && snapshot.qualification != ItemBuildQualification.None)
            {
                errors.Add(Error(ItemCorePotentialAndBuildValidationCodes.WhiteQualificationMustBeNone,
                    "White rarity qualification is locked to None."));
            }

            return new ItemBuildQualificationValidationResult(snapshot, errors);
        }

        private static List<ItemCoreRarityPolicySnapshot> CloneCorePolicies(
            IEnumerable<ItemCoreRarityPolicySnapshot> source,
            List<ItemCorePotentialAndBuildValidationError> errors)
        {
            List<ItemCoreRarityPolicySnapshot> output = new();
            foreach (ItemCoreRarityPolicySnapshot policy in source ?? Array.Empty<ItemCoreRarityPolicySnapshot>())
            {
                if (policy == null)
                {
                    errors.Add(Error(ItemCorePotentialAndBuildValidationCodes.CorePolicyNull,
                        "A core rarity policy is null."));
                    continue;
                }

                output.Add(new ItemCoreRarityPolicySnapshot(policy.rarity, policy.resolutionStatus,
                    policy.allowsUltimateCoreEffect, policy.dataMaturityKey));
            }

            return output;
        }

        private static List<ItemCorePotentialProfileSnapshot> CloneProfiles(
            IEnumerable<ItemCorePotentialProfileSnapshot> source,
            List<ItemCorePotentialAndBuildValidationError> errors)
        {
            List<ItemCorePotentialProfileSnapshot> output = new();
            foreach (ItemCorePotentialProfileSnapshot profile in source ?? Array.Empty<ItemCorePotentialProfileSnapshot>())
            {
                if (profile == null)
                {
                    errors.Add(Error(ItemCorePotentialAndBuildValidationCodes.ProfileNull,
                        "A core potential profile is null."));
                    continue;
                }

                output.Add(new ItemCorePotentialProfileSnapshot(
                    profile.cultivationPotentialProfileId,
                    profile.baseItemId,
                    profile.rarity,
                    profile.resolutionStatus,
                    profile.dataMaturityKey,
                    profile.PotentialEffects,
                    profile.VisibleCoreEffectIds));
            }

            return output;
        }

        private static List<ItemBuildQualificationRarityPolicySnapshot> CloneBuildPolicies(
            IEnumerable<ItemBuildQualificationRarityPolicySnapshot> source,
            List<ItemCorePotentialAndBuildValidationError> errors)
        {
            List<ItemBuildQualificationRarityPolicySnapshot> output = new();
            foreach (ItemBuildQualificationRarityPolicySnapshot policy in source
                ?? Array.Empty<ItemBuildQualificationRarityPolicySnapshot>())
            {
                if (policy == null)
                {
                    errors.Add(Error(ItemCorePotentialAndBuildValidationCodes.BuildPolicyNull,
                        "A Build rarity policy is null."));
                    continue;
                }

                output.Add(new ItemBuildQualificationRarityPolicySnapshot(policy.rarity,
                    policy.policyMode, policy.probabilityProfileId, policy.dataMaturityKey));
            }

            return output;
        }

        private static void ValidateCorePolicies(
            IReadOnlyList<ItemCoreRarityPolicySnapshot> policies,
            List<ItemCorePotentialAndBuildValidationError> errors)
        {
            foreach (ItemCoreRarityPolicySnapshot policy in policies)
            {
                if (!ItemInstanceRarityCatalog.TryGetDefinition(policy.rarity, out _))
                {
                    errors.Add(Error(ItemCorePotentialAndBuildValidationCodes.RarityInvalid,
                        $"Core policy has unsupported rarity value {(int)policy.rarity}."));
                }

                if (!Enum.IsDefined(typeof(ItemCorePotentialResolutionStatus), policy.resolutionStatus))
                {
                    errors.Add(Error(ItemCorePotentialAndBuildValidationCodes.ResolutionStatusInvalid,
                        $"Core policy '{StableRarity(policy.rarity)}' has unsupported resolution status."));
                }

                if (string.IsNullOrWhiteSpace(policy.dataMaturityKey))
                {
                    errors.Add(Error(ItemCorePotentialAndBuildValidationCodes.DataMaturityKeyEmpty,
                        $"Core policy '{StableRarity(policy.rarity)}' requires dataMaturityKey."));
                }

                bool shouldAllowUltimate = policy.rarity == ItemInstanceRarity.Orange;
                if (policy.allowsUltimateCoreEffect != shouldAllowUltimate)
                {
                    errors.Add(Error(ItemCorePotentialAndBuildValidationCodes.CoreUltimatePolicyInvalid,
                        $"Only orange must allow Ultimate core potential; '{StableRarity(policy.rarity)}' is invalid."));
                }
            }

            ValidateExactlyOnePolicyPerRarity(policies.Select(policy => policy.rarity),
                ItemCorePotentialAndBuildValidationCodes.CorePolicyMissing,
                ItemCorePotentialAndBuildValidationCodes.CorePolicyDuplicate, "core", errors);
        }

        private static void ValidateProfiles(
            IReadOnlyList<ItemCoreRarityPolicySnapshot> policies,
            IReadOnlyList<ItemCorePotentialProfileSnapshot> profiles,
            List<ItemCorePotentialAndBuildValidationError> errors)
        {
            HashSet<ItemInstanceRarity> policyRarities = new(policies
                .Where(policy => ItemInstanceRarityCatalog.TryGetDefinition(policy.rarity, out _))
                .Select(policy => policy.rarity));
            foreach (IGrouping<string, ItemCorePotentialProfileSnapshot> group in profiles
                .Where(profile => !string.IsNullOrWhiteSpace(profile.cultivationPotentialProfileId))
                .GroupBy(profile => profile.cultivationPotentialProfileId, StringComparer.Ordinal)
                .Where(group => group.Count() > 1))
            {
                errors.Add(Error(ItemCorePotentialAndBuildValidationCodes.ProfileIdDuplicate,
                    $"cultivationPotentialProfileId '{group.Key}' appears {group.Count()} times."));
            }

            foreach (IGrouping<string, ItemCorePotentialProfileSnapshot> group in profiles
                .GroupBy(profile => ProfileKey(profile.baseItemId, profile.rarity), StringComparer.Ordinal)
                .Where(group => group.Count() > 1))
            {
                errors.Add(Error(ItemCorePotentialAndBuildValidationCodes.ProfileDuplicate,
                    $"Core potential profile '{group.Key}' appears {group.Count()} times."));
            }

            foreach (ItemCorePotentialProfileSnapshot profile in profiles)
            {
                if (string.IsNullOrWhiteSpace(profile.cultivationPotentialProfileId))
                {
                    errors.Add(Error(ItemCorePotentialAndBuildValidationCodes.ProfileIdEmpty,
                        "cultivationPotentialProfileId is required."));
                }

                ValidateOrdinaryBaseItemId(profile.baseItemId, errors);
                if (!ItemInstanceRarityCatalog.TryGetDefinition(profile.rarity, out _))
                {
                    errors.Add(Error(ItemCorePotentialAndBuildValidationCodes.RarityInvalid,
                        $"Profile '{profile.cultivationPotentialProfileId}' has unsupported rarity value {(int)profile.rarity}."));
                }
                else if (!policyRarities.Contains(profile.rarity))
                {
                    errors.Add(Error(ItemCorePotentialAndBuildValidationCodes.ProfilePolicyMissing,
                        $"Profile '{profile.cultivationPotentialProfileId}' has no core rarity policy."));
                }

                if (!Enum.IsDefined(typeof(ItemCorePotentialResolutionStatus), profile.resolutionStatus))
                {
                    errors.Add(Error(ItemCorePotentialAndBuildValidationCodes.ResolutionStatusInvalid,
                        $"Profile '{profile.cultivationPotentialProfileId}' has unsupported resolution status."));
                }

                if (string.IsNullOrWhiteSpace(profile.dataMaturityKey))
                {
                    errors.Add(Error(ItemCorePotentialAndBuildValidationCodes.DataMaturityKeyEmpty,
                        $"Profile '{profile.cultivationPotentialProfileId}' requires dataMaturityKey."));
                }

                if (profile.resolutionStatus == ItemCorePotentialResolutionStatus.Unresolved
                    && (profile.PotentialEffects.Count > 0 || profile.VisibleCoreEffectIds.Count > 0))
                {
                    errors.Add(Error(ItemCorePotentialAndBuildValidationCodes.UnresolvedProfileHasEffects,
                        $"Unresolved profile '{profile.cultivationPotentialProfileId}' must not carry effect data."));
                }

                ValidateEffects(profile, errors);
            }
        }

        private static void ValidateEffects(
            ItemCorePotentialProfileSnapshot profile,
            List<ItemCorePotentialAndBuildValidationError> errors)
        {
            foreach (ItemCorePotentialEffectSnapshot effect in profile.PotentialEffects)
            {
                if (effect == null)
                {
                    errors.Add(Error(ItemCorePotentialAndBuildValidationCodes.CoreEffectNull,
                        $"Profile '{profile.cultivationPotentialProfileId}' contains a null effect."));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(effect.coreEffectId))
                {
                    errors.Add(Error(ItemCorePotentialAndBuildValidationCodes.CoreEffectIdEmpty,
                        $"Profile '{profile.cultivationPotentialProfileId}' contains an empty coreEffectId."));
                }

                if (!Enum.IsDefined(typeof(ItemCorePotentialEffectKind), effect.effectKind))
                {
                    errors.Add(Error(ItemCorePotentialAndBuildValidationCodes.CoreEffectKindInvalid,
                        $"Profile '{profile.cultivationPotentialProfileId}' contains an unsupported effect kind."));
                }

                if (effect.effectKind == ItemCorePotentialEffectKind.Ultimate
                    && profile.rarity != ItemInstanceRarity.Orange)
                {
                    errors.Add(Error(ItemCorePotentialAndBuildValidationCodes.UltimateEffectNotAllowed,
                        $"Profile '{profile.cultivationPotentialProfileId}' is not orange and cannot contain Ultimate potential."));
                }
            }

            foreach (IGrouping<string, ItemCorePotentialEffectSnapshot> group in profile.PotentialEffects
                .Where(effect => effect != null && !string.IsNullOrWhiteSpace(effect.coreEffectId))
                .GroupBy(effect => effect.coreEffectId, StringComparer.Ordinal)
                .Where(group => group.Count() > 1))
            {
                errors.Add(Error(ItemCorePotentialAndBuildValidationCodes.CoreEffectIdDuplicate,
                    $"Profile '{profile.cultivationPotentialProfileId}' repeats coreEffectId '{group.Key}'."));
            }

            HashSet<string> eligible = new(profile.EligibleCoreEffectIds, StringComparer.Ordinal);
            foreach (string visibleId in profile.VisibleCoreEffectIds.Where(id => !eligible.Contains(id)))
            {
                errors.Add(Error(ItemCorePotentialAndBuildValidationCodes.VisibleEffectNotEligible,
                    $"Visible coreEffectId '{visibleId}' is not eligible in profile '{profile.cultivationPotentialProfileId}'."));
            }
        }

        private static void ValidateBuildPolicies(
            IReadOnlyList<ItemBuildQualificationRarityPolicySnapshot> policies,
            List<ItemCorePotentialAndBuildValidationError> errors)
        {
            foreach (ItemBuildQualificationRarityPolicySnapshot policy in policies)
            {
                if (!ItemInstanceRarityCatalog.TryGetDefinition(policy.rarity, out _))
                {
                    errors.Add(Error(ItemCorePotentialAndBuildValidationCodes.RarityInvalid,
                        $"Build policy has unsupported rarity value {(int)policy.rarity}."));
                }

                if (!Enum.IsDefined(typeof(ItemBuildQualificationPolicyMode), policy.policyMode))
                {
                    errors.Add(Error(ItemCorePotentialAndBuildValidationCodes.BuildPolicyModeInvalid,
                        $"Build policy '{StableRarity(policy.rarity)}' has unsupported policy mode."));
                }

                ItemBuildQualificationPolicyMode expected = policy.rarity == ItemInstanceRarity.White
                    ? ItemBuildQualificationPolicyMode.LockedNone
                    : ItemBuildQualificationPolicyMode.ProbabilityUnresolved;
                if (policy.policyMode != expected)
                {
                    errors.Add(Error(ItemCorePotentialAndBuildValidationCodes.BuildPolicyContractInvalid,
                        $"Build policy '{StableRarity(policy.rarity)}' must remain {expected}."));
                }

                if (!string.IsNullOrWhiteSpace(policy.probabilityProfileId))
                {
                    errors.Add(Error(ItemCorePotentialAndBuildValidationCodes.ProbabilityProfileForbidden,
                        $"Build policy '{StableRarity(policy.rarity)}' must not reference probability data in this package."));
                }

                if (string.IsNullOrWhiteSpace(policy.dataMaturityKey))
                {
                    errors.Add(Error(ItemCorePotentialAndBuildValidationCodes.DataMaturityKeyEmpty,
                        $"Build policy '{StableRarity(policy.rarity)}' requires dataMaturityKey."));
                }
            }

            ValidateExactlyOnePolicyPerRarity(policies.Select(policy => policy.rarity),
                ItemCorePotentialAndBuildValidationCodes.BuildPolicyMissing,
                ItemCorePotentialAndBuildValidationCodes.BuildPolicyDuplicate, "Build", errors);
        }

        private static void ValidateExactlyOnePolicyPerRarity(
            IEnumerable<ItemInstanceRarity> rarities,
            string missingCode,
            string duplicateCode,
            string label,
            List<ItemCorePotentialAndBuildValidationError> errors)
        {
            ItemInstanceRarity[] values = (rarities ?? Array.Empty<ItemInstanceRarity>()).ToArray();
            foreach (ItemInstanceRarityDefinition definition in ItemInstanceRarityCatalog.All)
            {
                int count = values.Count(rarity => rarity == definition.rarity);
                if (count == 0)
                {
                    errors.Add(Error(missingCode, $"{label} policy '{definition.stableKey}' is missing."));
                }
                else if (count > 1)
                {
                    errors.Add(Error(duplicateCode,
                        $"{label} policy '{definition.stableKey}' appears {count} times."));
                }
            }
        }

        private static void ValidateOrdinaryBaseItemId(
            string baseItemId,
            List<ItemCorePotentialAndBuildValidationError> errors)
        {
            if (string.IsNullOrWhiteSpace(baseItemId))
            {
                errors.Add(Error(ItemCorePotentialAndBuildValidationCodes.BaseItemIdEmpty,
                    "baseItemId is required."));
            }
            else if (string.Equals(baseItemId,
                ItemRarityInstanceFoundation.CoreProgressionBaseItemId, StringComparison.Ordinal))
            {
                errors.Add(Error(ItemCorePotentialAndBuildValidationCodes.BaseItemNotOrdinary,
                    "I031 is excluded from ordinary core potential and Build qualification generation."));
            }
            else if (!ItemRarityInstanceFoundation.IsOrdinaryBaseItemId(baseItemId))
            {
                errors.Add(Error(ItemCorePotentialAndBuildValidationCodes.BaseItemUnknown,
                    $"Unknown ordinary baseItemId '{baseItemId}'."));
            }
        }

        private static string ProfileKey(string baseItemId, ItemInstanceRarity rarity)
        {
            return (baseItemId ?? string.Empty) + "@" + StableRarity(rarity);
        }

        private static string StableRarity(ItemInstanceRarity rarity)
        {
            string stable = rarity.ToStableKey();
            return string.IsNullOrWhiteSpace(stable)
                ? ((int)rarity).ToString(CultureInfo.InvariantCulture)
                : stable;
        }

        private static ItemCorePotentialAndBuildValidationError Error(string code, string message)
        {
            return new ItemCorePotentialAndBuildValidationError(code, message);
        }
    }
}
