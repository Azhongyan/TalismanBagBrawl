using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using TalismanBag.Items.Generation.Stats;

namespace TalismanBag.Items.Generation.Affixes
{
    public enum ItemAffixResolutionStatus { Unresolved, Defined }
    public enum ItemAffixSlotKind { Fixed, Random }
    public enum ItemAffixRepeatPolicy { Unresolved, DisallowDuplicateAffix, AllowDuplicateAffix }
    public enum ItemAffixWeightResolutionStatus { Unresolved, Defined }

    public static class ItemAffixPoolAndRangeValidationCodes
    {
        public const string None = "NONE";
        public const string DefinitionNull = "AFFIX_DEFINITION_NULL";
        public const string AffixIdEmpty = "AFFIX_ID_EMPTY";
        public const string AffixIdDuplicate = "AFFIX_ID_DUPLICATE";
        public const string DirectionInvalid = "AFFIX_DIRECTION_INVALID";
        public const string DecimalPlacesInvalid = "AFFIX_DECIMAL_PLACES_INVALID";
        public const string StepUnitsInvalid = "AFFIX_STEP_UNITS_INVALID";
        public const string RoundingModeInvalid = "AFFIX_ROUNDING_MODE_INVALID";
        public const string DataMaturityKeyEmpty = "DATA_MATURITY_KEY_EMPTY";
        public const string ValueProfileNull = "AFFIX_VALUE_PROFILE_NULL";
        public const string ValueProfileIdEmpty = "AFFIX_VALUE_PROFILE_ID_EMPTY";
        public const string ValueProfileIdDuplicate = "AFFIX_VALUE_PROFILE_ID_DUPLICATE";
        public const string AffixDefinitionUnknown = "AFFIX_DEFINITION_UNKNOWN";
        public const string ResolutionStatusInvalid = "AFFIX_RESOLUTION_STATUS_INVALID";
        public const string ValueProfileDefinitionMismatch = "AFFIX_VALUE_PROFILE_DEFINITION_MISMATCH";
        public const string UnresolvedValueProfileHasRange = "UNRESOLVED_VALUE_PROFILE_HAS_RANGE";
        public const string TotalRangeInvalid = "AFFIX_TOTAL_RANGE_INVALID";
        public const string RarityRangeInvalid = "AFFIX_RARITY_RANGE_INVALID";
        public const string RarityInvalid = "AFFIX_RARITY_INVALID";
        public const string RarityDuplicate = "AFFIX_RARITY_DUPLICATE";
        public const string RaritySetIncomplete = "AFFIX_RARITY_SET_INCOMPLETE";
        public const string RangeOutsideTotal = "AFFIX_RANGE_OUTSIDE_TOTAL";
        public const string RangeStepMisaligned = "AFFIX_RANGE_STEP_MISALIGNED";
        public const string TotalRangeEnvelopeMismatch = "AFFIX_TOTAL_RANGE_ENVELOPE_MISMATCH";
        public const string HigherMinNotMonotonic = "AFFIX_HIGHER_MIN_NOT_MONOTONIC";
        public const string HigherMaxNotMonotonic = "AFFIX_HIGHER_MAX_NOT_MONOTONIC";
        public const string LowerMinNotMonotonic = "AFFIX_LOWER_MIN_NOT_MONOTONIC";
        public const string LowerMaxNotMonotonic = "AFFIX_LOWER_MAX_NOT_MONOTONIC";
        public const string SlotPolicyNull = "AFFIX_SLOT_POLICY_NULL";
        public const string SlotPolicyIdEmpty = "AFFIX_SLOT_POLICY_ID_EMPTY";
        public const string SlotPolicyIdDuplicate = "AFFIX_SLOT_POLICY_ID_DUPLICATE";
        public const string SlotNull = "AFFIX_SLOT_NULL";
        public const string SlotIdEmpty = "AFFIX_SLOT_ID_EMPTY";
        public const string SlotIdDuplicate = "AFFIX_SLOT_ID_DUPLICATE";
        public const string SlotKindInvalid = "AFFIX_SLOT_KIND_INVALID";
        public const string UnresolvedSlotPolicyHasSlots = "UNRESOLVED_SLOT_POLICY_HAS_SLOTS";
        public const string PoolNull = "AFFIX_POOL_NULL";
        public const string PoolIdEmpty = "AFFIX_POOL_ID_EMPTY";
        public const string PoolIdDuplicate = "AFFIX_POOL_ID_DUPLICATE";
        public const string RepeatPolicyInvalid = "AFFIX_REPEAT_POLICY_INVALID";
        public const string UnresolvedPoolHasContent = "UNRESOLVED_POOL_HAS_CONTENT";
        public const string DefinedPoolRepeatPolicyUnresolved = "DEFINED_POOL_REPEAT_POLICY_UNRESOLVED";
        public const string PoolEntryNull = "AFFIX_POOL_ENTRY_NULL";
        public const string PoolEntryDuplicate = "AFFIX_POOL_ENTRY_DUPLICATE";
        public const string ValueProfileUnknown = "AFFIX_VALUE_PROFILE_UNKNOWN";
        public const string ValueProfileAffixMismatch = "AFFIX_VALUE_PROFILE_AFFIX_MISMATCH";
        public const string WeightResolutionStatusInvalid = "AFFIX_WEIGHT_STATUS_INVALID";
        public const string UnresolvedWeightNonZero = "UNRESOLVED_AFFIX_WEIGHT_NON_ZERO";
        public const string DefinedWeightNotPositive = "DEFINED_AFFIX_WEIGHT_NOT_POSITIVE";
        public const string DefinedPoolHasUnresolvedWeight = "DEFINED_POOL_HAS_UNRESOLVED_WEIGHT";
        public const string MutexGroupNull = "AFFIX_MUTEX_GROUP_NULL";
        public const string MutexGroupIdEmpty = "AFFIX_MUTEX_GROUP_ID_EMPTY";
        public const string MutexGroupIdDuplicate = "AFFIX_MUTEX_GROUP_ID_DUPLICATE";
        public const string MutexGroupTooSmall = "AFFIX_MUTEX_GROUP_TOO_SMALL";
        public const string MutexMemberEmpty = "AFFIX_MUTEX_MEMBER_EMPTY";
        public const string MutexMemberDuplicate = "AFFIX_MUTEX_MEMBER_DUPLICATE";
        public const string MutexMemberUnknown = "AFFIX_MUTEX_MEMBER_UNKNOWN";
        public const string GenerationProfileNull = "AFFIX_GENERATION_PROFILE_NULL";
        public const string BaseItemIdEmpty = "BASE_ITEM_ID_EMPTY";
        public const string BaseItemUnknown = "BASE_ITEM_UNKNOWN";
        public const string BaseItemNotOrdinary = "BASE_ITEM_NOT_ORDINARY";
        public const string GenerationProfileDuplicate = "AFFIX_GENERATION_PROFILE_DUPLICATE";
        public const string UnresolvedGenerationProfileHasContent = "UNRESOLVED_GENERATION_PROFILE_HAS_CONTENT";
        public const string SlotPolicyUnknown = "AFFIX_SLOT_POLICY_UNKNOWN";
        public const string RandomPoolUnknown = "AFFIX_RANDOM_POOL_UNKNOWN";
        public const string FixedBindingNull = "AFFIX_FIXED_BINDING_NULL";
        public const string FixedBindingSlotUnknown = "AFFIX_FIXED_BINDING_SLOT_UNKNOWN";
        public const string FixedBindingSlotNotFixed = "AFFIX_FIXED_BINDING_SLOT_NOT_FIXED";
        public const string FixedBindingDuplicate = "AFFIX_FIXED_BINDING_DUPLICATE";
        public const string RandomPoolSlotMismatch = "AFFIX_RANDOM_POOL_SLOT_MISMATCH";
        public const string QueryInputInvalid = "QUERY_INPUT_INVALID";
        public const string QueryAffixNotFound = "QUERY_AFFIX_NOT_FOUND";
        public const string QueryValueProfileNotFound = "QUERY_AFFIX_VALUE_PROFILE_NOT_FOUND";
        public const string QuerySlotPolicyNotFound = "QUERY_AFFIX_SLOT_POLICY_NOT_FOUND";
        public const string QueryPoolNotFound = "QUERY_AFFIX_POOL_NOT_FOUND";
        public const string QueryGenerationProfileNotFound = "QUERY_AFFIX_GENERATION_PROFILE_NOT_FOUND";
    }

    public sealed class ItemAffixPoolAndRangeValidationError
    {
        public ItemAffixPoolAndRangeValidationError(string code, string message)
        {
            this.code = code ?? string.Empty;
            this.message = message ?? string.Empty;
        }
        public string code { get; }
        public string message { get; }
    }

    public sealed class ItemAffixQueryResult<T> where T : class
    {
        private ItemAffixQueryResult(T value, ItemAffixPoolAndRangeValidationError error)
        { this.value = value; validationError = error; }
        public bool isSuccess => value != null && validationError == null;
        public T value { get; }
        public ItemAffixPoolAndRangeValidationError validationError { get; }
        internal static ItemAffixQueryResult<T> Success(T value) => new(value, null);
        internal static ItemAffixQueryResult<T> Failure(string code, string message) =>
            new(null, new ItemAffixPoolAndRangeValidationError(code, message));
    }

    public sealed class ItemAffixNumericRangeSnapshot
    {
        public ItemAffixNumericRangeSnapshot(long minUnits, long maxUnits)
        { this.minUnits = minUnits; this.maxUnits = maxUnits; }
        public long minUnits { get; }
        public long maxUnits { get; }
    }

    public sealed class ItemAffixDefinitionSnapshot
    {
        public ItemAffixDefinitionSnapshot(string affixId, string displayName, string valueUnitKey,
            ItemStatDirection direction, int decimalPlaces, long stepUnits,
            ItemStatRoundingMode roundingMode, string dataMaturityKey)
        {
            this.affixId = Normalize(affixId); this.displayName = displayName ?? string.Empty;
            this.valueUnitKey = Normalize(valueUnitKey); this.direction = direction;
            this.decimalPlaces = decimalPlaces; this.stepUnits = stepUnits;
            this.roundingMode = roundingMode; this.dataMaturityKey = Normalize(dataMaturityKey);
        }
        public string affixId { get; }
        public string displayName { get; }
        public string valueUnitKey { get; }
        public ItemStatDirection direction { get; }
        public int decimalPlaces { get; }
        public long stepUnits { get; }
        public ItemStatRoundingMode roundingMode { get; }
        public string dataMaturityKey { get; }
        private static string Normalize(string value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    public sealed class ItemAffixRarityValueRangeSnapshot
    {
        public ItemAffixRarityValueRangeSnapshot(ItemInstanceRarity rarity, long minUnits, long maxUnits)
        { this.rarity = rarity; this.minUnits = minUnits; this.maxUnits = maxUnits; }
        public ItemInstanceRarity rarity { get; }
        public long minUnits { get; }
        public long maxUnits { get; }
    }

    public sealed class ItemAffixValueProfileSnapshot
    {
        private readonly ReadOnlyCollection<ItemAffixRarityValueRangeSnapshot> rarityRanges;
        public ItemAffixValueProfileSnapshot(string affixValueProfileId, string affixId,
            ItemAffixResolutionStatus resolutionStatus, string dataMaturityKey,
            ItemAffixNumericRangeSnapshot totalRange, IEnumerable<ItemAffixRarityValueRangeSnapshot> rarityRanges,
            ItemStatDirection direction, int decimalPlaces, long stepUnits, ItemStatRoundingMode roundingMode)
        {
            this.affixValueProfileId = Normalize(affixValueProfileId); this.affixId = Normalize(affixId);
            this.resolutionStatus = resolutionStatus; this.dataMaturityKey = Normalize(dataMaturityKey);
            this.totalRange = totalRange == null ? null : new ItemAffixNumericRangeSnapshot(totalRange.minUnits, totalRange.maxUnits);
            this.rarityRanges = Freeze((rarityRanges ?? Array.Empty<ItemAffixRarityValueRangeSnapshot>())
                .Where(range => range != null)
                .Select(range => new ItemAffixRarityValueRangeSnapshot(range.rarity, range.minUnits, range.maxUnits))
                .OrderBy(range => range.rarity.ToTierIndex()).ThenBy(range => (int)range.rarity));
            this.direction = direction; this.decimalPlaces = decimalPlaces; this.stepUnits = stepUnits;
            this.roundingMode = roundingMode;
        }
        public string affixValueProfileId { get; }
        public string affixId { get; }
        public ItemAffixResolutionStatus resolutionStatus { get; }
        public string dataMaturityKey { get; }
        public ItemAffixNumericRangeSnapshot totalRange { get; }
        public IReadOnlyList<ItemAffixRarityValueRangeSnapshot> RarityRanges => rarityRanges;
        public ItemStatDirection direction { get; }
        public int decimalPlaces { get; }
        public long stepUnits { get; }
        public ItemStatRoundingMode roundingMode { get; }
        private static ReadOnlyCollection<T> Freeze<T>(IEnumerable<T> values) => Array.AsReadOnly((values ?? Array.Empty<T>()).ToArray());
        private static string Normalize(string value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    public sealed class ItemAffixSlotSnapshot
    {
        public ItemAffixSlotSnapshot(string slotId, ItemAffixSlotKind slotKind)
        { this.slotId = Normalize(slotId); this.slotKind = slotKind; }
        public string slotId { get; }
        public ItemAffixSlotKind slotKind { get; }
        private static string Normalize(string value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    public sealed class ItemAffixSlotPolicySnapshot
    {
        private readonly ReadOnlyCollection<ItemAffixSlotSnapshot> slots;
        public ItemAffixSlotPolicySnapshot(string slotPolicyId, ItemAffixResolutionStatus resolutionStatus,
            string dataMaturityKey, IEnumerable<ItemAffixSlotSnapshot> slots)
        {
            this.slotPolicyId = Normalize(slotPolicyId); this.resolutionStatus = resolutionStatus;
            this.dataMaturityKey = Normalize(dataMaturityKey);
            this.slots = Freeze((slots ?? Array.Empty<ItemAffixSlotSnapshot>())
                .Select(slot => slot == null ? null : new ItemAffixSlotSnapshot(slot.slotId, slot.slotKind))
                .OrderBy(slot => slot?.slotId ?? string.Empty, StringComparer.Ordinal));
        }
        public string slotPolicyId { get; }
        public ItemAffixResolutionStatus resolutionStatus { get; }
        public string dataMaturityKey { get; }
        public IReadOnlyList<ItemAffixSlotSnapshot> Slots => slots;
        private static ReadOnlyCollection<T> Freeze<T>(IEnumerable<T> values) => Array.AsReadOnly((values ?? Array.Empty<T>()).ToArray());
        private static string Normalize(string value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    public sealed class ItemFixedAffixBindingSnapshot
    {
        public ItemFixedAffixBindingSnapshot(string slotId, string affixId, string affixValueProfileId)
        { this.slotId = Normalize(slotId); this.affixId = Normalize(affixId); this.affixValueProfileId = Normalize(affixValueProfileId); }
        public string slotId { get; }
        public string affixId { get; }
        public string affixValueProfileId { get; }
        private static string Normalize(string value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    public sealed class ItemAffixPoolEntrySnapshot
    {
        public ItemAffixPoolEntrySnapshot(string affixId, string affixValueProfileId,
            ItemAffixWeightResolutionStatus weightResolutionStatus, long weightUnits)
        { this.affixId = Normalize(affixId); this.affixValueProfileId = Normalize(affixValueProfileId);
            this.weightResolutionStatus = weightResolutionStatus; this.weightUnits = weightUnits; }
        public string affixId { get; }
        public string affixValueProfileId { get; }
        public ItemAffixWeightResolutionStatus weightResolutionStatus { get; }
        public long weightUnits { get; }
        private static string Normalize(string value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    public sealed class ItemAffixMutexGroupSnapshot
    {
        private readonly ReadOnlyCollection<string> affixIds;
        public ItemAffixMutexGroupSnapshot(string mutexGroupId, IEnumerable<string> affixIds)
        { this.mutexGroupId = Normalize(mutexGroupId); this.affixIds = Freeze((affixIds ?? Array.Empty<string>()).Select(Normalize).OrderBy(id => id, StringComparer.Ordinal)); }
        public string mutexGroupId { get; }
        public IReadOnlyList<string> AffixIds => affixIds;
        private static ReadOnlyCollection<T> Freeze<T>(IEnumerable<T> values) => Array.AsReadOnly((values ?? Array.Empty<T>()).ToArray());
        private static string Normalize(string value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    public sealed class ItemRandomAffixPoolSnapshot
    {
        private readonly ReadOnlyCollection<ItemAffixPoolEntrySnapshot> entries;
        private readonly ReadOnlyCollection<ItemAffixMutexGroupSnapshot> mutexGroups;
        public ItemRandomAffixPoolSnapshot(string poolId, ItemAffixResolutionStatus resolutionStatus,
            string dataMaturityKey, ItemAffixRepeatPolicy repeatPolicy,
            IEnumerable<ItemAffixPoolEntrySnapshot> entries, IEnumerable<ItemAffixMutexGroupSnapshot> mutexGroups)
        {
            this.poolId = Normalize(poolId); this.resolutionStatus = resolutionStatus;
            this.dataMaturityKey = Normalize(dataMaturityKey); this.repeatPolicy = repeatPolicy;
            this.entries = Freeze((entries ?? Array.Empty<ItemAffixPoolEntrySnapshot>())
                .Select(entry => entry == null ? null : new ItemAffixPoolEntrySnapshot(entry.affixId,
                    entry.affixValueProfileId, entry.weightResolutionStatus, entry.weightUnits))
                .OrderBy(entry => entry?.affixId ?? string.Empty, StringComparer.Ordinal)
                .ThenBy(entry => entry?.affixValueProfileId ?? string.Empty, StringComparer.Ordinal));
            this.mutexGroups = Freeze((mutexGroups ?? Array.Empty<ItemAffixMutexGroupSnapshot>())
                .Select(group => group == null ? null : new ItemAffixMutexGroupSnapshot(group.mutexGroupId, group.AffixIds))
                .OrderBy(group => group?.mutexGroupId ?? string.Empty, StringComparer.Ordinal));
        }
        public string poolId { get; }
        public ItemAffixResolutionStatus resolutionStatus { get; }
        public string dataMaturityKey { get; }
        public ItemAffixRepeatPolicy repeatPolicy { get; }
        public IReadOnlyList<ItemAffixPoolEntrySnapshot> Entries => entries;
        public IReadOnlyList<ItemAffixMutexGroupSnapshot> MutexGroups => mutexGroups;
        private static ReadOnlyCollection<T> Freeze<T>(IEnumerable<T> values) => Array.AsReadOnly((values ?? Array.Empty<T>()).ToArray());
        private static string Normalize(string value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    public sealed class ItemAffixGenerationProfileSnapshot
    {
        private readonly ReadOnlyCollection<ItemFixedAffixBindingSnapshot> fixedAffixBindings;
        public ItemAffixGenerationProfileSnapshot(string baseItemId, ItemAffixResolutionStatus resolutionStatus,
            string dataMaturityKey, string slotPolicyId,
            IEnumerable<ItemFixedAffixBindingSnapshot> fixedAffixBindings, string randomPoolId)
        {
            this.baseItemId = Normalize(baseItemId); this.resolutionStatus = resolutionStatus;
            this.dataMaturityKey = Normalize(dataMaturityKey); this.slotPolicyId = Normalize(slotPolicyId);
            this.fixedAffixBindings = Freeze((fixedAffixBindings ?? Array.Empty<ItemFixedAffixBindingSnapshot>())
                .Select(binding => binding == null ? null : new ItemFixedAffixBindingSnapshot(binding.slotId,
                    binding.affixId, binding.affixValueProfileId))
                .OrderBy(binding => binding?.slotId ?? string.Empty, StringComparer.Ordinal));
            this.randomPoolId = Normalize(randomPoolId);
        }
        public string baseItemId { get; }
        public ItemAffixResolutionStatus resolutionStatus { get; }
        public string dataMaturityKey { get; }
        public string slotPolicyId { get; }
        public IReadOnlyList<ItemFixedAffixBindingSnapshot> FixedAffixBindings => fixedAffixBindings;
        public string randomPoolId { get; }
        private static ReadOnlyCollection<T> Freeze<T>(IEnumerable<T> values) => Array.AsReadOnly((values ?? Array.Empty<T>()).ToArray());
        private static string Normalize(string value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    public sealed class ItemAffixPoolAndRangeSchemaSnapshot
    {
        public const string CurrentSchemaId = "ItemAffixPoolAndRangeSchemaSnapshot.v1";
        private readonly ReadOnlyCollection<ItemAffixDefinitionSnapshot> definitions;
        private readonly ReadOnlyCollection<ItemAffixValueProfileSnapshot> valueProfiles;
        private readonly ReadOnlyCollection<ItemAffixSlotPolicySnapshot> slotPolicies;
        private readonly ReadOnlyCollection<ItemRandomAffixPoolSnapshot> pools;
        private readonly ReadOnlyCollection<ItemAffixGenerationProfileSnapshot> generationProfiles;
        private readonly ReadOnlyCollection<ItemAffixPoolAndRangeValidationError> errors;
        private readonly IReadOnlyDictionary<string, ItemAffixDefinitionSnapshot> definitionsById;
        private readonly IReadOnlyDictionary<string, ItemAffixValueProfileSnapshot> valueProfilesById;
        private readonly IReadOnlyDictionary<string, ItemAffixSlotPolicySnapshot> slotPoliciesById;
        private readonly IReadOnlyDictionary<string, ItemRandomAffixPoolSnapshot> poolsById;
        private readonly IReadOnlyDictionary<string, ItemAffixGenerationProfileSnapshot> generationProfilesByItem;

        internal ItemAffixPoolAndRangeSchemaSnapshot(IEnumerable<ItemAffixDefinitionSnapshot> definitions,
            IEnumerable<ItemAffixValueProfileSnapshot> valueProfiles,
            IEnumerable<ItemAffixSlotPolicySnapshot> slotPolicies, IEnumerable<ItemRandomAffixPoolSnapshot> pools,
            IEnumerable<ItemAffixGenerationProfileSnapshot> generationProfiles,
            IEnumerable<ItemAffixPoolAndRangeValidationError> errors)
        {
            schemaId = CurrentSchemaId;
            this.definitions = Freeze((definitions ?? Array.Empty<ItemAffixDefinitionSnapshot>()).OrderBy(x => x.affixId, StringComparer.Ordinal));
            this.valueProfiles = Freeze((valueProfiles ?? Array.Empty<ItemAffixValueProfileSnapshot>()).OrderBy(x => x.affixValueProfileId, StringComparer.Ordinal));
            this.slotPolicies = Freeze((slotPolicies ?? Array.Empty<ItemAffixSlotPolicySnapshot>()).OrderBy(x => x.slotPolicyId, StringComparer.Ordinal));
            this.pools = Freeze((pools ?? Array.Empty<ItemRandomAffixPoolSnapshot>()).OrderBy(x => x.poolId, StringComparer.Ordinal));
            this.generationProfiles = Freeze((generationProfiles ?? Array.Empty<ItemAffixGenerationProfileSnapshot>()).OrderBy(x => x.baseItemId, StringComparer.Ordinal));
            this.errors = Freeze((errors ?? Array.Empty<ItemAffixPoolAndRangeValidationError>()).OrderBy(x => x.code, StringComparer.Ordinal).ThenBy(x => x.message, StringComparer.Ordinal));
            definitionsById = Index(this.definitions, x => x.affixId);
            valueProfilesById = Index(this.valueProfiles, x => x.affixValueProfileId);
            slotPoliciesById = Index(this.slotPolicies, x => x.slotPolicyId);
            poolsById = Index(this.pools, x => x.poolId);
            generationProfilesByItem = Index(this.generationProfiles, x => x.baseItemId);
        }
        public string schemaId { get; }
        public bool isValid => errors.Count == 0;
        public IReadOnlyList<ItemAffixDefinitionSnapshot> AffixDefinitions => definitions;
        public IReadOnlyList<ItemAffixValueProfileSnapshot> AffixValueProfiles => valueProfiles;
        public IReadOnlyList<ItemAffixSlotPolicySnapshot> SlotPolicies => slotPolicies;
        public IReadOnlyList<ItemRandomAffixPoolSnapshot> RandomPools => pools;
        public IReadOnlyList<ItemAffixGenerationProfileSnapshot> ItemGenerationProfiles => generationProfiles;
        public IReadOnlyList<ItemAffixPoolAndRangeValidationError> ValidationErrors => errors;

        public ItemAffixQueryResult<ItemAffixDefinitionSnapshot> QueryAffixDefinition(string affixId) =>
            Query(affixId, definitionsById, ItemAffixPoolAndRangeValidationCodes.QueryAffixNotFound, "affixId");
        public ItemAffixQueryResult<ItemAffixValueProfileSnapshot> QueryAffixValueProfile(string profileId) =>
            Query(profileId, valueProfilesById, ItemAffixPoolAndRangeValidationCodes.QueryValueProfileNotFound, "affixValueProfileId");
        public ItemAffixQueryResult<ItemAffixSlotPolicySnapshot> QuerySlotPolicy(string slotPolicyId) =>
            Query(slotPolicyId, slotPoliciesById, ItemAffixPoolAndRangeValidationCodes.QuerySlotPolicyNotFound, "slotPolicyId");
        public ItemAffixQueryResult<ItemRandomAffixPoolSnapshot> QueryRandomPool(string poolId) =>
            Query(poolId, poolsById, ItemAffixPoolAndRangeValidationCodes.QueryPoolNotFound, "poolId");
        public ItemAffixQueryResult<ItemAffixGenerationProfileSnapshot> QueryItemGenerationProfile(string baseItemId) =>
            Query(baseItemId, generationProfilesByItem, ItemAffixPoolAndRangeValidationCodes.QueryGenerationProfileNotFound, "baseItemId");

        public string BuildCanonicalSignature()
        {
            StringBuilder b = new(); b.Append(schemaId);
            foreach (ItemAffixDefinitionSnapshot x in definitions) b.Append("\nD|").Append(x.affixId).Append('|').Append(x.displayName).Append('|').Append(x.valueUnitKey).Append('|').Append(x.direction).Append('|').Append(x.decimalPlaces.ToString(CultureInfo.InvariantCulture)).Append('|').Append(x.stepUnits.ToString(CultureInfo.InvariantCulture)).Append('|').Append(x.roundingMode).Append('|').Append(x.dataMaturityKey);
            foreach (ItemAffixValueProfileSnapshot x in valueProfiles) { b.Append("\nV|").Append(x.affixValueProfileId).Append('|').Append(x.affixId).Append('|').Append(x.resolutionStatus).Append('|').Append(x.dataMaturityKey).Append('|').Append(x.totalRange?.minUnits.ToString(CultureInfo.InvariantCulture) ?? "null").Append('|').Append(x.totalRange?.maxUnits.ToString(CultureInfo.InvariantCulture) ?? "null").Append('|').Append(x.direction).Append('|').Append(x.decimalPlaces.ToString(CultureInfo.InvariantCulture)).Append('|').Append(x.stepUnits.ToString(CultureInfo.InvariantCulture)).Append('|').Append(x.roundingMode); foreach (ItemAffixRarityValueRangeSnapshot r in x.RarityRanges) b.Append("\nR|").Append(x.affixValueProfileId).Append('|').Append(StableRarity(r.rarity)).Append('|').Append(r.minUnits.ToString(CultureInfo.InvariantCulture)).Append('|').Append(r.maxUnits.ToString(CultureInfo.InvariantCulture)); }
            foreach (ItemAffixSlotPolicySnapshot x in slotPolicies) { b.Append("\nS|").Append(x.slotPolicyId).Append('|').Append(x.resolutionStatus).Append('|').Append(x.dataMaturityKey); foreach (ItemAffixSlotSnapshot s in x.Slots) b.Append("\nT|").Append(x.slotPolicyId).Append('|').Append(s?.slotId ?? "null").Append('|').Append(s == null ? "null" : s.slotKind.ToString()); }
            foreach (ItemRandomAffixPoolSnapshot x in pools) { b.Append("\nP|").Append(x.poolId).Append('|').Append(x.resolutionStatus).Append('|').Append(x.dataMaturityKey).Append('|').Append(x.repeatPolicy); foreach (ItemAffixPoolEntrySnapshot e in x.Entries) b.Append("\nN|").Append(x.poolId).Append('|').Append(e?.affixId ?? "null").Append('|').Append(e?.affixValueProfileId ?? "null").Append('|').Append(e == null ? "null" : e.weightResolutionStatus.ToString()).Append('|').Append(e?.weightUnits.ToString(CultureInfo.InvariantCulture) ?? "null"); foreach (ItemAffixMutexGroupSnapshot m in x.MutexGroups) b.Append("\nM|").Append(x.poolId).Append('|').Append(m?.mutexGroupId ?? "null").Append('|').Append(m == null ? "null" : string.Join(";", m.AffixIds)); }
            foreach (ItemAffixGenerationProfileSnapshot x in generationProfiles) { b.Append("\nG|").Append(x.baseItemId).Append('|').Append(x.resolutionStatus).Append('|').Append(x.dataMaturityKey).Append('|').Append(x.slotPolicyId).Append('|').Append(x.randomPoolId); foreach (ItemFixedAffixBindingSnapshot f in x.FixedAffixBindings) b.Append("\nF|").Append(x.baseItemId).Append('|').Append(f?.slotId ?? "null").Append('|').Append(f?.affixId ?? "null").Append('|').Append(f?.affixValueProfileId ?? "null"); }
            foreach (ItemAffixPoolAndRangeValidationError e in errors) b.Append("\nE|").Append(e.code).Append('|').Append(e.message);
            return b.ToString();
        }
        private static ItemAffixQueryResult<T> Query<T>(string key, IReadOnlyDictionary<string, T> index, string missingCode, string label) where T : class
        { string normalized = Normalize(key); if (string.IsNullOrWhiteSpace(normalized)) return ItemAffixQueryResult<T>.Failure(ItemAffixPoolAndRangeValidationCodes.QueryInputInvalid, label + " is required."); return index.TryGetValue(normalized, out T value) ? ItemAffixQueryResult<T>.Success(value) : ItemAffixQueryResult<T>.Failure(missingCode, label + " '" + normalized + "' was not found."); }
        private static IReadOnlyDictionary<string, T> Index<T>(IEnumerable<T> values, Func<T, string> key) => values.GroupBy(key, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
        private static ReadOnlyCollection<T> Freeze<T>(IEnumerable<T> values) => Array.AsReadOnly((values ?? Array.Empty<T>()).ToArray());
        private static string Normalize(string value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        private static string StableRarity(ItemInstanceRarity rarity) { string key = rarity.ToStableKey(); return string.IsNullOrWhiteSpace(key) ? ((int)rarity).ToString(CultureInfo.InvariantCulture) : key; }
    }

    public static class ItemAffixPoolAndRangeSchema
    {
        public const int MinimumDecimalPlaces = 0;
        public const int MaximumDecimalPlaces = 6;

        public static ItemAffixPoolAndRangeSchemaSnapshot Create(
            IEnumerable<ItemAffixDefinitionSnapshot> definitions,
            IEnumerable<ItemAffixValueProfileSnapshot> valueProfiles,
            IEnumerable<ItemAffixSlotPolicySnapshot> slotPolicies,
            IEnumerable<ItemRandomAffixPoolSnapshot> pools,
            IEnumerable<ItemAffixGenerationProfileSnapshot> generationProfiles)
        {
            List<ItemAffixPoolAndRangeValidationError> errors = new();
            List<ItemAffixDefinitionSnapshot> d = CloneDefinitions(definitions, errors);
            List<ItemAffixValueProfileSnapshot> v = CloneValueProfiles(valueProfiles, errors);
            List<ItemAffixSlotPolicySnapshot> s = CloneSlotPolicies(slotPolicies, errors);
            List<ItemRandomAffixPoolSnapshot> p = ClonePools(pools, errors);
            List<ItemAffixGenerationProfileSnapshot> g = CloneGenerationProfiles(generationProfiles, errors);
            ValidateDefinitions(d, errors); ValidateValueProfiles(d, v, errors); ValidateSlotPolicies(s, errors);
            ValidatePools(d, v, p, errors); ValidateGenerationProfiles(d, v, s, p, g, errors);
            return new ItemAffixPoolAndRangeSchemaSnapshot(d, v, s, p, g, errors);
        }

        private static List<ItemAffixDefinitionSnapshot> CloneDefinitions(IEnumerable<ItemAffixDefinitionSnapshot> source, List<ItemAffixPoolAndRangeValidationError> errors)
        { List<ItemAffixDefinitionSnapshot> output = new(); foreach (ItemAffixDefinitionSnapshot x in source ?? Array.Empty<ItemAffixDefinitionSnapshot>()) { if (x == null) { errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.DefinitionNull, "An affix definition is null.")); continue; } output.Add(new ItemAffixDefinitionSnapshot(x.affixId, x.displayName, x.valueUnitKey, x.direction, x.decimalPlaces, x.stepUnits, x.roundingMode, x.dataMaturityKey)); } return output; }
        private static List<ItemAffixValueProfileSnapshot> CloneValueProfiles(IEnumerable<ItemAffixValueProfileSnapshot> source, List<ItemAffixPoolAndRangeValidationError> errors)
        { List<ItemAffixValueProfileSnapshot> output = new(); foreach (ItemAffixValueProfileSnapshot x in source ?? Array.Empty<ItemAffixValueProfileSnapshot>()) { if (x == null) { errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.ValueProfileNull, "An affix value profile is null.")); continue; } output.Add(new ItemAffixValueProfileSnapshot(x.affixValueProfileId, x.affixId, x.resolutionStatus, x.dataMaturityKey, x.totalRange, x.RarityRanges, x.direction, x.decimalPlaces, x.stepUnits, x.roundingMode)); } return output; }
        private static List<ItemAffixSlotPolicySnapshot> CloneSlotPolicies(IEnumerable<ItemAffixSlotPolicySnapshot> source, List<ItemAffixPoolAndRangeValidationError> errors)
        { List<ItemAffixSlotPolicySnapshot> output = new(); foreach (ItemAffixSlotPolicySnapshot x in source ?? Array.Empty<ItemAffixSlotPolicySnapshot>()) { if (x == null) { errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.SlotPolicyNull, "An affix slot policy is null.")); continue; } output.Add(new ItemAffixSlotPolicySnapshot(x.slotPolicyId, x.resolutionStatus, x.dataMaturityKey, x.Slots)); } return output; }
        private static List<ItemRandomAffixPoolSnapshot> ClonePools(IEnumerable<ItemRandomAffixPoolSnapshot> source, List<ItemAffixPoolAndRangeValidationError> errors)
        { List<ItemRandomAffixPoolSnapshot> output = new(); foreach (ItemRandomAffixPoolSnapshot x in source ?? Array.Empty<ItemRandomAffixPoolSnapshot>()) { if (x == null) { errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.PoolNull, "An affix pool is null.")); continue; } output.Add(new ItemRandomAffixPoolSnapshot(x.poolId, x.resolutionStatus, x.dataMaturityKey, x.repeatPolicy, x.Entries, x.MutexGroups)); } return output; }
        private static List<ItemAffixGenerationProfileSnapshot> CloneGenerationProfiles(IEnumerable<ItemAffixGenerationProfileSnapshot> source, List<ItemAffixPoolAndRangeValidationError> errors)
        { List<ItemAffixGenerationProfileSnapshot> output = new(); foreach (ItemAffixGenerationProfileSnapshot x in source ?? Array.Empty<ItemAffixGenerationProfileSnapshot>()) { if (x == null) { errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.GenerationProfileNull, "An item affix generation profile is null.")); continue; } output.Add(new ItemAffixGenerationProfileSnapshot(x.baseItemId, x.resolutionStatus, x.dataMaturityKey, x.slotPolicyId, x.FixedAffixBindings, x.randomPoolId)); } return output; }

        private static void ValidateDefinitions(IReadOnlyList<ItemAffixDefinitionSnapshot> definitions, List<ItemAffixPoolAndRangeValidationError> errors)
        {
            foreach (ItemAffixDefinitionSnapshot x in definitions) {
                if (string.IsNullOrWhiteSpace(x.affixId)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.AffixIdEmpty, "affixId is required."));
                if (!Enum.IsDefined(typeof(ItemStatDirection), x.direction)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.DirectionInvalid, $"Affix '{x.affixId}' direction is invalid."));
                if (x.decimalPlaces < MinimumDecimalPlaces || x.decimalPlaces > MaximumDecimalPlaces) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.DecimalPlacesInvalid, $"Affix '{x.affixId}' decimalPlaces is invalid."));
                if (x.stepUnits <= 0) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.StepUnitsInvalid, $"Affix '{x.affixId}' stepUnits must be positive."));
                if (!Enum.IsDefined(typeof(ItemStatRoundingMode), x.roundingMode)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.RoundingModeInvalid, $"Affix '{x.affixId}' rounding mode is invalid."));
                if (string.IsNullOrWhiteSpace(x.dataMaturityKey)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.DataMaturityKeyEmpty, $"Affix '{x.affixId}' requires dataMaturityKey."));
            }
            AddDuplicates(definitions.Where(x => !string.IsNullOrWhiteSpace(x.affixId)).Select(x => x.affixId), ItemAffixPoolAndRangeValidationCodes.AffixIdDuplicate, "affixId", errors);
        }

        private static void ValidateValueProfiles(IReadOnlyList<ItemAffixDefinitionSnapshot> definitions, IReadOnlyList<ItemAffixValueProfileSnapshot> profiles, List<ItemAffixPoolAndRangeValidationError> errors)
        {
            Dictionary<string, ItemAffixDefinitionSnapshot> byId = definitions.Where(x => !string.IsNullOrWhiteSpace(x.affixId)).GroupBy(x => x.affixId, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
            AddDuplicates(profiles.Where(x => !string.IsNullOrWhiteSpace(x.affixValueProfileId)).Select(x => x.affixValueProfileId), ItemAffixPoolAndRangeValidationCodes.ValueProfileIdDuplicate, "affixValueProfileId", errors);
            foreach (ItemAffixValueProfileSnapshot x in profiles) {
                if (string.IsNullOrWhiteSpace(x.affixValueProfileId)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.ValueProfileIdEmpty, "affixValueProfileId is required."));
                if (!byId.TryGetValue(x.affixId, out ItemAffixDefinitionSnapshot definition)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.AffixDefinitionUnknown, $"Value profile '{x.affixValueProfileId}' references unknown affixId '{x.affixId}'."));
                else if (definition.direction != x.direction || definition.decimalPlaces != x.decimalPlaces || definition.stepUnits != x.stepUnits || definition.roundingMode != x.roundingMode) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.ValueProfileDefinitionMismatch, $"Value profile '{x.affixValueProfileId}' numeric contract differs from affix definition '{x.affixId}'."));
                if (!Enum.IsDefined(typeof(ItemAffixResolutionStatus), x.resolutionStatus)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.ResolutionStatusInvalid, $"Value profile '{x.affixValueProfileId}' resolution status is invalid."));
                if (string.IsNullOrWhiteSpace(x.dataMaturityKey)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.DataMaturityKeyEmpty, $"Value profile '{x.affixValueProfileId}' requires dataMaturityKey."));
                if (x.resolutionStatus == ItemAffixResolutionStatus.Unresolved) { if (x.totalRange != null || x.RarityRanges.Count > 0) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.UnresolvedValueProfileHasRange, $"Unresolved value profile '{x.affixValueProfileId}' must not carry ranges.")); }
                else if (x.resolutionStatus == ItemAffixResolutionStatus.Defined) ValidateRanges(x, errors);
            }
        }

        private static void ValidateRanges(ItemAffixValueProfileSnapshot x, List<ItemAffixPoolAndRangeValidationError> errors)
        {
            if (x.totalRange == null || x.totalRange.minUnits > x.totalRange.maxUnits) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.TotalRangeInvalid, $"Value profile '{x.affixValueProfileId}' totalRange is invalid."));
            foreach (ItemAffixRarityValueRangeSnapshot r in x.RarityRanges) {
                if (!ItemInstanceRarityCatalog.TryGetDefinition(r.rarity, out _)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.RarityInvalid, $"Value profile '{x.affixValueProfileId}' has unsupported rarity {(int)r.rarity}."));
                if (r.minUnits > r.maxUnits) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.RarityRangeInvalid, $"Value profile '{x.affixValueProfileId}' rarity range is invalid."));
                if (x.totalRange != null && (r.minUnits < x.totalRange.minUnits || r.maxUnits > x.totalRange.maxUnits)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.RangeOutsideTotal, $"Value profile '{x.affixValueProfileId}' rarity range is outside totalRange."));
                if (x.stepUnits > 0 && (!Aligned(r.minUnits, x.stepUnits) || !Aligned(r.maxUnits, x.stepUnits))) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.RangeStepMisaligned, $"Value profile '{x.affixValueProfileId}' rarity endpoints are not step aligned."));
            }
            if (x.totalRange != null && x.stepUnits > 0 && (!Aligned(x.totalRange.minUnits, x.stepUnits) || !Aligned(x.totalRange.maxUnits, x.stepUnits))) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.RangeStepMisaligned, $"Value profile '{x.affixValueProfileId}' totalRange is not step aligned."));
            AddDuplicates(x.RarityRanges.Where(r => ItemInstanceRarityCatalog.TryGetDefinition(r.rarity, out _)).Select(r => r.rarity.ToStableKey()), ItemAffixPoolAndRangeValidationCodes.RarityDuplicate, "rarity", errors);
            ItemAffixRarityValueRangeSnapshot[] ordered = x.RarityRanges.Where(r => ItemInstanceRarityCatalog.TryGetDefinition(r.rarity, out _)).GroupBy(r => r.rarity).Select(g => g.First()).OrderBy(r => r.rarity.ToTierIndex()).ToArray();
            if (ordered.Length != ItemInstanceRarityCatalog.All.Count) { errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.RaritySetIncomplete, $"Value profile '{x.affixValueProfileId}' must contain five formal rarities.")); return; }
            if (x.totalRange != null && (x.totalRange.minUnits != ordered.Min(r => r.minUnits) || x.totalRange.maxUnits != ordered.Max(r => r.maxUnits))) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.TotalRangeEnvelopeMismatch, $"Value profile '{x.affixValueProfileId}' totalRange must equal rarity envelope."));
            for (int i = 1; i < ordered.Length; i++) { ItemAffixRarityValueRangeSnapshot a = ordered[i - 1], b = ordered[i]; if (x.direction == ItemStatDirection.HigherIsBetter) { if (b.minUnits < a.minUnits) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.HigherMinNotMonotonic, x.affixValueProfileId)); if (b.maxUnits < a.maxUnits) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.HigherMaxNotMonotonic, x.affixValueProfileId)); } else if (x.direction == ItemStatDirection.LowerIsBetter) { if (b.minUnits > a.minUnits) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.LowerMinNotMonotonic, x.affixValueProfileId)); if (b.maxUnits > a.maxUnits) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.LowerMaxNotMonotonic, x.affixValueProfileId)); } }
        }

        private static void ValidateSlotPolicies(IReadOnlyList<ItemAffixSlotPolicySnapshot> policies, List<ItemAffixPoolAndRangeValidationError> errors)
        {
            AddDuplicates(policies.Where(x => !string.IsNullOrWhiteSpace(x.slotPolicyId)).Select(x => x.slotPolicyId), ItemAffixPoolAndRangeValidationCodes.SlotPolicyIdDuplicate, "slotPolicyId", errors);
            foreach (ItemAffixSlotPolicySnapshot x in policies) { if (string.IsNullOrWhiteSpace(x.slotPolicyId)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.SlotPolicyIdEmpty, "slotPolicyId is required.")); if (!Enum.IsDefined(typeof(ItemAffixResolutionStatus), x.resolutionStatus)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.ResolutionStatusInvalid, x.slotPolicyId)); if (string.IsNullOrWhiteSpace(x.dataMaturityKey)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.DataMaturityKeyEmpty, x.slotPolicyId)); if (x.resolutionStatus == ItemAffixResolutionStatus.Unresolved && x.Slots.Count > 0) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.UnresolvedSlotPolicyHasSlots, x.slotPolicyId)); foreach (ItemAffixSlotSnapshot slot in x.Slots) { if (slot == null) { errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.SlotNull, x.slotPolicyId)); continue; } if (string.IsNullOrWhiteSpace(slot.slotId)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.SlotIdEmpty, x.slotPolicyId)); if (!Enum.IsDefined(typeof(ItemAffixSlotKind), slot.slotKind)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.SlotKindInvalid, slot.slotId)); } AddDuplicates(x.Slots.Where(s => s != null && !string.IsNullOrWhiteSpace(s.slotId)).Select(s => s.slotId), ItemAffixPoolAndRangeValidationCodes.SlotIdDuplicate, "slotId", errors); }
        }

        private static void ValidatePools(IReadOnlyList<ItemAffixDefinitionSnapshot> definitions, IReadOnlyList<ItemAffixValueProfileSnapshot> profiles, IReadOnlyList<ItemRandomAffixPoolSnapshot> pools, List<ItemAffixPoolAndRangeValidationError> errors)
        {
            HashSet<string> affixIds = new(definitions.Select(x => x.affixId), StringComparer.Ordinal); Dictionary<string, ItemAffixValueProfileSnapshot> profileById = profiles.Where(x => !string.IsNullOrWhiteSpace(x.affixValueProfileId)).GroupBy(x => x.affixValueProfileId, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
            AddDuplicates(pools.Where(x => !string.IsNullOrWhiteSpace(x.poolId)).Select(x => x.poolId), ItemAffixPoolAndRangeValidationCodes.PoolIdDuplicate, "poolId", errors);
            foreach (ItemRandomAffixPoolSnapshot x in pools) {
                if (string.IsNullOrWhiteSpace(x.poolId)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.PoolIdEmpty, "poolId is required.")); if (!Enum.IsDefined(typeof(ItemAffixResolutionStatus), x.resolutionStatus)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.ResolutionStatusInvalid, x.poolId)); if (string.IsNullOrWhiteSpace(x.dataMaturityKey)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.DataMaturityKeyEmpty, x.poolId)); if (!Enum.IsDefined(typeof(ItemAffixRepeatPolicy), x.repeatPolicy)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.RepeatPolicyInvalid, x.poolId));
                if (x.resolutionStatus == ItemAffixResolutionStatus.Unresolved && (x.repeatPolicy != ItemAffixRepeatPolicy.Unresolved || x.Entries.Count > 0 || x.MutexGroups.Count > 0)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.UnresolvedPoolHasContent, x.poolId)); if (x.resolutionStatus == ItemAffixResolutionStatus.Defined && x.repeatPolicy == ItemAffixRepeatPolicy.Unresolved) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.DefinedPoolRepeatPolicyUnresolved, x.poolId));
                foreach (ItemAffixPoolEntrySnapshot e in x.Entries) { if (e == null) { errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.PoolEntryNull, x.poolId)); continue; } if (!affixIds.Contains(e.affixId)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.AffixDefinitionUnknown, e.affixId)); if (!profileById.TryGetValue(e.affixValueProfileId, out ItemAffixValueProfileSnapshot vp)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.ValueProfileUnknown, e.affixValueProfileId)); else if (!string.Equals(vp.affixId, e.affixId, StringComparison.Ordinal)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.ValueProfileAffixMismatch, e.affixValueProfileId)); if (!Enum.IsDefined(typeof(ItemAffixWeightResolutionStatus), e.weightResolutionStatus)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.WeightResolutionStatusInvalid, e.affixId)); else if (e.weightResolutionStatus == ItemAffixWeightResolutionStatus.Unresolved && e.weightUnits != 0) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.UnresolvedWeightNonZero, e.affixId)); else if (e.weightResolutionStatus == ItemAffixWeightResolutionStatus.Defined && e.weightUnits <= 0) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.DefinedWeightNotPositive, e.affixId)); if (x.resolutionStatus == ItemAffixResolutionStatus.Defined && e.weightResolutionStatus != ItemAffixWeightResolutionStatus.Defined) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.DefinedPoolHasUnresolvedWeight, e.affixId)); }
                AddDuplicates(x.Entries.Where(e => e != null && !string.IsNullOrWhiteSpace(e.affixId)).Select(e => e.affixId), ItemAffixPoolAndRangeValidationCodes.PoolEntryDuplicate, "pool entry affixId", errors);
                HashSet<string> entryIds = new(x.Entries.Where(e => e != null).Select(e => e.affixId), StringComparer.Ordinal); AddDuplicates(x.MutexGroups.Where(m => m != null && !string.IsNullOrWhiteSpace(m.mutexGroupId)).Select(m => m.mutexGroupId), ItemAffixPoolAndRangeValidationCodes.MutexGroupIdDuplicate, "mutexGroupId", errors);
                foreach (ItemAffixMutexGroupSnapshot m in x.MutexGroups) { if (m == null) { errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.MutexGroupNull, x.poolId)); continue; } if (string.IsNullOrWhiteSpace(m.mutexGroupId)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.MutexGroupIdEmpty, x.poolId)); if (m.AffixIds.Count < 2 || m.AffixIds.Distinct(StringComparer.Ordinal).Count() < 2) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.MutexGroupTooSmall, m.mutexGroupId)); if (m.AffixIds.Any(string.IsNullOrWhiteSpace)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.MutexMemberEmpty, m.mutexGroupId)); AddDuplicates(m.AffixIds.Where(id => !string.IsNullOrWhiteSpace(id)), ItemAffixPoolAndRangeValidationCodes.MutexMemberDuplicate, "mutex member", errors); foreach (string id in m.AffixIds.Where(id => !string.IsNullOrWhiteSpace(id) && !entryIds.Contains(id))) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.MutexMemberUnknown, id)); }
            }
        }

        private static void ValidateGenerationProfiles(IReadOnlyList<ItemAffixDefinitionSnapshot> definitions, IReadOnlyList<ItemAffixValueProfileSnapshot> values, IReadOnlyList<ItemAffixSlotPolicySnapshot> policies, IReadOnlyList<ItemRandomAffixPoolSnapshot> pools, IReadOnlyList<ItemAffixGenerationProfileSnapshot> profiles, List<ItemAffixPoolAndRangeValidationError> errors)
        {
            HashSet<string> affixIds = new(definitions.Select(x => x.affixId), StringComparer.Ordinal); Dictionary<string, ItemAffixValueProfileSnapshot> valueById = values.Where(x => !string.IsNullOrWhiteSpace(x.affixValueProfileId)).GroupBy(x => x.affixValueProfileId, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal); Dictionary<string, ItemAffixSlotPolicySnapshot> policyById = policies.Where(x => !string.IsNullOrWhiteSpace(x.slotPolicyId)).GroupBy(x => x.slotPolicyId, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal); HashSet<string> poolIds = new(pools.Select(x => x.poolId), StringComparer.Ordinal);
            AddDuplicates(profiles.Where(x => !string.IsNullOrWhiteSpace(x.baseItemId)).Select(x => x.baseItemId), ItemAffixPoolAndRangeValidationCodes.GenerationProfileDuplicate, "baseItemId profile", errors);
            foreach (ItemAffixGenerationProfileSnapshot x in profiles) {
                ValidateBaseItemId(x.baseItemId, errors); if (!Enum.IsDefined(typeof(ItemAffixResolutionStatus), x.resolutionStatus)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.ResolutionStatusInvalid, x.baseItemId)); if (string.IsNullOrWhiteSpace(x.dataMaturityKey)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.DataMaturityKeyEmpty, x.baseItemId));
                if (x.resolutionStatus == ItemAffixResolutionStatus.Unresolved) { if (!string.IsNullOrWhiteSpace(x.slotPolicyId) || x.FixedAffixBindings.Count > 0 || !string.IsNullOrWhiteSpace(x.randomPoolId)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.UnresolvedGenerationProfileHasContent, x.baseItemId)); continue; }
                if (!policyById.TryGetValue(x.slotPolicyId, out ItemAffixSlotPolicySnapshot policy)) { errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.SlotPolicyUnknown, x.slotPolicyId)); continue; }
                Dictionary<string, ItemAffixSlotSnapshot> slots = policy.Slots.Where(s => s != null).GroupBy(s => s.slotId, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
                foreach (ItemFixedAffixBindingSnapshot f in x.FixedAffixBindings) { if (f == null) { errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.FixedBindingNull, x.baseItemId)); continue; } if (!slots.TryGetValue(f.slotId, out ItemAffixSlotSnapshot slot)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.FixedBindingSlotUnknown, f.slotId)); else if (slot.slotKind != ItemAffixSlotKind.Fixed) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.FixedBindingSlotNotFixed, f.slotId)); if (!affixIds.Contains(f.affixId)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.AffixDefinitionUnknown, f.affixId)); if (!valueById.TryGetValue(f.affixValueProfileId, out ItemAffixValueProfileSnapshot vp)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.ValueProfileUnknown, f.affixValueProfileId)); else if (!string.Equals(vp.affixId, f.affixId, StringComparison.Ordinal)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.ValueProfileAffixMismatch, f.affixValueProfileId)); }
                AddDuplicates(x.FixedAffixBindings.Where(f => f != null && !string.IsNullOrWhiteSpace(f.slotId)).Select(f => f.slotId), ItemAffixPoolAndRangeValidationCodes.FixedBindingDuplicate, "fixed slot binding", errors);
                bool hasRandom = slots.Values.Any(s => s.slotKind == ItemAffixSlotKind.Random); if (hasRandom && string.IsNullOrWhiteSpace(x.randomPoolId) || !hasRandom && !string.IsNullOrWhiteSpace(x.randomPoolId)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.RandomPoolSlotMismatch, x.baseItemId)); if (!string.IsNullOrWhiteSpace(x.randomPoolId) && !poolIds.Contains(x.randomPoolId)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.RandomPoolUnknown, x.randomPoolId));
            }
        }

        private static void ValidateBaseItemId(string id, List<ItemAffixPoolAndRangeValidationError> errors)
        { if (string.IsNullOrWhiteSpace(id)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.BaseItemIdEmpty, "baseItemId is required.")); else if (string.Equals(id, ItemRarityInstanceFoundation.CoreProgressionBaseItemId, StringComparison.Ordinal)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.BaseItemNotOrdinary, "I031 is excluded from ordinary affix generation.")); else if (!ItemRarityInstanceFoundation.IsOrdinaryBaseItemId(id)) errors.Add(Error(ItemAffixPoolAndRangeValidationCodes.BaseItemUnknown, $"Unknown ordinary baseItemId '{id}'.")); }
        private static void AddDuplicates<T>(IEnumerable<T> source, string code, string label, List<ItemAffixPoolAndRangeValidationError> errors)
        { foreach (IGrouping<T, T> g in (source ?? Array.Empty<T>()).GroupBy(x => x).Where(g => g.Count() > 1)) errors.Add(Error(code, $"{label} '{g.Key}' appears {g.Count()} times.")); }
        private static bool Aligned(long value, long step) => value % step == 0;
        private static ItemAffixPoolAndRangeValidationError Error(string code, string message) => new(code, message);
    }
}
