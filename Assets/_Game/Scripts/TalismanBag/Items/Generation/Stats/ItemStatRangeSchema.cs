using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace TalismanBag.Items.Generation.Stats
{
    public enum ItemStatDirection
    {
        HigherIsBetter,
        LowerIsBetter,
        Neutral
    }

    public enum ItemStatRoundingMode
    {
        Nearest,
        Floor,
        Ceiling,
        Truncate
    }

    public enum ItemStatDataMaturity
    {
        SCHEMA_ONLY,
        SEED_DATA,
        BALANCE_CANDIDATE,
        PLAYTEST_ACCEPTED,
        LIVE_LOCKED
    }

    public static class ItemStatRangeValidationCodes
    {
        public const string None = "NONE";
        public const string DefinitionNull = "STAT_DEFINITION_NULL";
        public const string StatIdEmpty = "STAT_ID_EMPTY";
        public const string StatIdDuplicate = "STAT_ID_DUPLICATE";
        public const string DirectionInvalid = "STAT_DIRECTION_INVALID";
        public const string RoundingModeInvalid = "ROUNDING_MODE_INVALID";
        public const string DataMaturityInvalid = "DATA_MATURITY_INVALID";
        public const string DecimalPlacesInvalid = "DECIMAL_PLACES_INVALID";
        public const string StepUnitsInvalid = "STEP_UNITS_INVALID";
        public const string ProfileNull = "STAT_PROFILE_NULL";
        public const string ProfileDuplicate = "STAT_PROFILE_DUPLICATE";
        public const string BaseItemIdEmpty = "BASE_ITEM_ID_EMPTY";
        public const string BaseItemUnknown = "BASE_ITEM_UNKNOWN";
        public const string BaseItemNotOrdinary = "BASE_ITEM_NOT_ORDINARY";
        public const string StatDefinitionUnknown = "STAT_DEFINITION_UNKNOWN";
        public const string NumericProfileSchemaOnly = "NUMERIC_PROFILE_SCHEMA_ONLY";
        public const string TotalRangeInvalid = "TOTAL_RANGE_INVALID";
        public const string RarityRangeInvalid = "RARITY_RANGE_INVALID";
        public const string RarityInvalid = "RARITY_INVALID";
        public const string RarityDuplicate = "RARITY_DUPLICATE";
        public const string RaritySetIncomplete = "RARITY_SET_INCOMPLETE";
        public const string RangeOutsideTotal = "RANGE_OUTSIDE_TOTAL";
        public const string RangeStepMisaligned = "RANGE_STEP_MISALIGNED";
        public const string TotalRangeEnvelopeMismatch = "TOTAL_RANGE_ENVELOPE_MISMATCH";
        public const string HigherMinNotMonotonic = "HIGHER_MIN_NOT_MONOTONIC";
        public const string HigherMaxNotMonotonic = "HIGHER_MAX_NOT_MONOTONIC";
        public const string LowerMinNotMonotonic = "LOWER_MIN_NOT_MONOTONIC";
        public const string LowerMaxNotMonotonic = "LOWER_MAX_NOT_MONOTONIC";
        public const string QueryInputInvalid = "QUERY_INPUT_INVALID";
        public const string QueryStatNotFound = "QUERY_STAT_NOT_FOUND";
        public const string QueryProfileNotFound = "QUERY_PROFILE_NOT_FOUND";
        public const string QueryRarityNotFound = "QUERY_RARITY_NOT_FOUND";
    }

    public sealed class ItemStatRangeValidationError
    {
        public ItemStatRangeValidationError(string code, string message)
        {
            this.code = code ?? string.Empty;
            this.message = message ?? string.Empty;
        }

        public string code { get; }
        public string message { get; }
    }

    public sealed class ItemNumericRangeSnapshot
    {
        public ItemNumericRangeSnapshot(long minUnits, long maxUnits)
        {
            this.minUnits = minUnits;
            this.maxUnits = maxUnits;
        }

        public long minUnits { get; }
        public long maxUnits { get; }
    }

    public sealed class ItemRarityStatRangeSnapshot
    {
        public ItemRarityStatRangeSnapshot(ItemInstanceRarity rarity, long minUnits, long maxUnits)
        {
            this.rarity = rarity;
            this.minUnits = minUnits;
            this.maxUnits = maxUnits;
        }

        public ItemInstanceRarity rarity { get; }
        public long minUnits { get; }
        public long maxUnits { get; }
    }

    public sealed class ItemStatDefinitionSnapshot
    {
        public ItemStatDefinitionSnapshot(
            string statId,
            string displayName,
            string unitKey,
            ItemStatDirection direction,
            int decimalPlaces,
            long stepUnits,
            ItemStatRoundingMode roundingMode,
            ItemStatDataMaturity dataMaturity)
        {
            this.statId = Normalize(statId);
            this.displayName = displayName ?? string.Empty;
            this.unitKey = Normalize(unitKey);
            this.direction = direction;
            this.decimalPlaces = decimalPlaces;
            this.stepUnits = stepUnits;
            this.roundingMode = roundingMode;
            this.dataMaturity = dataMaturity;
        }

        public string statId { get; }
        public string displayName { get; }
        public string unitKey { get; }
        public ItemStatDirection direction { get; }
        public int decimalPlaces { get; }
        public long stepUnits { get; }
        public ItemStatRoundingMode roundingMode { get; }
        public ItemStatDataMaturity dataMaturity { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    public sealed class ItemStatRangeProfileSnapshot
    {
        private readonly ReadOnlyCollection<ItemRarityStatRangeSnapshot> rarityRanges;

        public ItemStatRangeProfileSnapshot(
            string baseItemId,
            string statId,
            ItemStatDataMaturity dataMaturity,
            ItemNumericRangeSnapshot totalRange,
            IEnumerable<ItemRarityStatRangeSnapshot> rarityRanges)
        {
            this.baseItemId = Normalize(baseItemId);
            this.statId = Normalize(statId);
            this.dataMaturity = dataMaturity;
            this.totalRange = totalRange == null
                ? null
                : new ItemNumericRangeSnapshot(totalRange.minUnits, totalRange.maxUnits);
            this.rarityRanges = Freeze((rarityRanges ?? Array.Empty<ItemRarityStatRangeSnapshot>())
                .Where(range => range != null)
                .Select(range => new ItemRarityStatRangeSnapshot(range.rarity, range.minUnits, range.maxUnits)));
        }

        public string baseItemId { get; }
        public string statId { get; }
        public ItemStatDataMaturity dataMaturity { get; }
        public ItemNumericRangeSnapshot totalRange { get; }
        public IReadOnlyList<ItemRarityStatRangeSnapshot> RarityRanges => rarityRanges;

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private static ReadOnlyCollection<T> Freeze<T>(IEnumerable<T> values)
        {
            return Array.AsReadOnly((values ?? Array.Empty<T>()).ToArray());
        }
    }

    public sealed class ItemStatQueryResult<T> where T : class
    {
        private ItemStatQueryResult(T value, ItemStatRangeValidationError validationError)
        {
            this.value = value;
            this.validationError = validationError;
        }

        public bool isSuccess => value != null && validationError == null;
        public T value { get; }
        public ItemStatRangeValidationError validationError { get; }

        internal static ItemStatQueryResult<T> Success(T value)
        {
            return new ItemStatQueryResult<T>(value, null);
        }

        internal static ItemStatQueryResult<T> Failure(string code, string message)
        {
            return new ItemStatQueryResult<T>(null, new ItemStatRangeValidationError(code, message));
        }
    }

    public sealed class ItemStatRangeSchemaSnapshot
    {
        public const string CurrentSchemaId = "ItemStatRangeSchemaSnapshot.v1";

        private readonly ReadOnlyCollection<ItemStatDefinitionSnapshot> statDefinitions;
        private readonly ReadOnlyCollection<ItemStatRangeProfileSnapshot> itemStatProfiles;
        private readonly ReadOnlyCollection<ItemStatRangeValidationError> validationErrors;
        private readonly IReadOnlyDictionary<string, ItemStatDefinitionSnapshot> definitionsById;
        private readonly IReadOnlyDictionary<string, ItemStatRangeProfileSnapshot> profilesByKey;

        internal ItemStatRangeSchemaSnapshot(
            IEnumerable<ItemStatDefinitionSnapshot> definitions,
            IEnumerable<ItemStatRangeProfileSnapshot> profiles,
            IEnumerable<ItemStatRangeValidationError> errors)
        {
            schemaId = CurrentSchemaId;
            statDefinitions = Freeze((definitions ?? Array.Empty<ItemStatDefinitionSnapshot>())
                .OrderBy(definition => definition.statId, StringComparer.Ordinal)
                .ThenBy(definition => definition.displayName, StringComparer.Ordinal)
                .ThenBy(definition => definition.unitKey, StringComparer.Ordinal)
                .ThenBy(definition => (int)definition.direction)
                .ThenBy(definition => definition.decimalPlaces)
                .ThenBy(definition => definition.stepUnits)
                .ThenBy(definition => (int)definition.roundingMode)
                .ThenBy(definition => (int)definition.dataMaturity));
            itemStatProfiles = Freeze((profiles ?? Array.Empty<ItemStatRangeProfileSnapshot>())
                .OrderBy(profile => profile.baseItemId, StringComparer.Ordinal)
                .ThenBy(profile => profile.statId, StringComparer.Ordinal)
                .ThenBy(profile => (int)profile.dataMaturity)
                .ThenBy(profile => profile.totalRange?.minUnits ?? long.MinValue)
                .ThenBy(profile => profile.totalRange?.maxUnits ?? long.MinValue)
                .ThenBy(ProfileRangeSortKey, StringComparer.Ordinal));
            validationErrors = Freeze((errors ?? Array.Empty<ItemStatRangeValidationError>())
                .OrderBy(error => error.code, StringComparer.Ordinal)
                .ThenBy(error => error.message, StringComparer.Ordinal));
            definitionsById = statDefinitions
                .GroupBy(definition => definition.statId, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
            profilesByKey = itemStatProfiles
                .GroupBy(ProfileKey, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        }

        public string schemaId { get; }
        public IReadOnlyList<ItemStatDefinitionSnapshot> StatDefinitions => statDefinitions;
        public IReadOnlyList<ItemStatRangeProfileSnapshot> ItemStatProfiles => itemStatProfiles;
        public IReadOnlyList<ItemStatRangeValidationError> ValidationErrors => validationErrors;
        public bool isValid => validationErrors.Count == 0;

        public ItemStatQueryResult<ItemStatDefinitionSnapshot> QueryStatDefinition(string statId)
        {
            string normalized = Normalize(statId);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return ItemStatQueryResult<ItemStatDefinitionSnapshot>.Failure(
                    ItemStatRangeValidationCodes.QueryInputInvalid,
                    "statId is required for a stat definition query.");
            }

            return definitionsById.TryGetValue(normalized, out ItemStatDefinitionSnapshot definition)
                ? ItemStatQueryResult<ItemStatDefinitionSnapshot>.Success(definition)
                : ItemStatQueryResult<ItemStatDefinitionSnapshot>.Failure(
                    ItemStatRangeValidationCodes.QueryStatNotFound,
                    $"Stat definition '{normalized}' was not found.");
        }

        public ItemStatQueryResult<ItemStatRangeProfileSnapshot> QueryProfile(string baseItemId, string statId)
        {
            string normalizedBaseItemId = Normalize(baseItemId);
            string normalizedStatId = Normalize(statId);
            if (string.IsNullOrWhiteSpace(normalizedBaseItemId) || string.IsNullOrWhiteSpace(normalizedStatId))
            {
                return ItemStatQueryResult<ItemStatRangeProfileSnapshot>.Failure(
                    ItemStatRangeValidationCodes.QueryInputInvalid,
                    "baseItemId and statId are required for a stat profile query.");
            }

            string key = ProfileKey(normalizedBaseItemId, normalizedStatId);
            return profilesByKey.TryGetValue(key, out ItemStatRangeProfileSnapshot profile)
                ? ItemStatQueryResult<ItemStatRangeProfileSnapshot>.Success(profile)
                : ItemStatQueryResult<ItemStatRangeProfileSnapshot>.Failure(
                    ItemStatRangeValidationCodes.QueryProfileNotFound,
                    $"Stat profile '{key}' was not found.");
        }

        public ItemStatQueryResult<ItemRarityStatRangeSnapshot> QueryRarityRange(
            string baseItemId,
            string statId,
            ItemInstanceRarity rarity)
        {
            ItemStatQueryResult<ItemStatRangeProfileSnapshot> profileResult = QueryProfile(baseItemId, statId);
            if (!profileResult.isSuccess)
            {
                return ItemStatQueryResult<ItemRarityStatRangeSnapshot>.Failure(
                    profileResult.validationError.code,
                    profileResult.validationError.message);
            }

            ItemRarityStatRangeSnapshot range = profileResult.value.RarityRanges
                .FirstOrDefault(candidate => candidate.rarity == rarity);
            return range != null
                ? ItemStatQueryResult<ItemRarityStatRangeSnapshot>.Success(range)
                : ItemStatQueryResult<ItemRarityStatRangeSnapshot>.Failure(
                    ItemStatRangeValidationCodes.QueryRarityNotFound,
                    $"Rarity '{rarity}' is not configured for '{ProfileKey(baseItemId, statId)}'.");
        }

        public string BuildCanonicalSignature()
        {
            StringBuilder builder = new();
            builder.Append(schemaId);
            foreach (ItemStatDefinitionSnapshot definition in statDefinitions)
            {
                builder.Append("\nD|")
                    .Append(definition.statId).Append('|')
                    .Append(definition.displayName).Append('|')
                    .Append(definition.unitKey).Append('|')
                    .Append(definition.direction).Append('|')
                    .Append(definition.decimalPlaces.ToString(CultureInfo.InvariantCulture)).Append('|')
                    .Append(definition.stepUnits.ToString(CultureInfo.InvariantCulture)).Append('|')
                    .Append(definition.roundingMode).Append('|')
                    .Append(definition.dataMaturity);
            }

            foreach (ItemStatRangeProfileSnapshot profile in itemStatProfiles)
            {
                builder.Append("\nP|")
                    .Append(profile.baseItemId).Append('|')
                    .Append(profile.statId).Append('|')
                    .Append(profile.dataMaturity).Append('|')
                    .Append(profile.totalRange?.minUnits.ToString(CultureInfo.InvariantCulture) ?? "null").Append('|')
                    .Append(profile.totalRange?.maxUnits.ToString(CultureInfo.InvariantCulture) ?? "null");
                foreach (ItemRarityStatRangeSnapshot range in profile.RarityRanges
                    .OrderBy(range => range.rarity.ToTierIndex())
                    .ThenBy(range => (int)range.rarity))
                {
                    builder.Append("\nB|")
                        .Append(profile.baseItemId).Append('|')
                        .Append(profile.statId).Append('|')
                        .Append(range.rarity.ToStableKey()).Append('|')
                        .Append(((int)range.rarity).ToString(CultureInfo.InvariantCulture)).Append('|')
                        .Append(range.minUnits.ToString(CultureInfo.InvariantCulture)).Append('|')
                        .Append(range.maxUnits.ToString(CultureInfo.InvariantCulture));
                }
            }

            foreach (ItemStatRangeValidationError error in validationErrors)
            {
                builder.Append("\nE|").Append(error.code).Append('|').Append(error.message);
            }

            return builder.ToString();
        }

        private static ReadOnlyCollection<T> Freeze<T>(IEnumerable<T> values)
        {
            return Array.AsReadOnly((values ?? Array.Empty<T>()).ToArray());
        }

        private static string ProfileKey(ItemStatRangeProfileSnapshot profile)
        {
            return ProfileKey(profile.baseItemId, profile.statId);
        }

        private static string ProfileKey(string baseItemId, string statId)
        {
            return Normalize(baseItemId) + "@" + Normalize(statId);
        }

        private static string ProfileRangeSortKey(ItemStatRangeProfileSnapshot profile)
        {
            return string.Join(";", profile.RarityRanges
                .OrderBy(range => range.rarity.ToTierIndex())
                .ThenBy(range => (int)range.rarity)
                .ThenBy(range => range.minUnits)
                .ThenBy(range => range.maxUnits)
                .Select(range => string.Join(":",
                    ((int)range.rarity).ToString(CultureInfo.InvariantCulture),
                    range.minUnits.ToString(CultureInfo.InvariantCulture),
                    range.maxUnits.ToString(CultureInfo.InvariantCulture))));
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    public static class ItemStatRangeSchema
    {
        public const int MinimumDecimalPlaces = 0;
        public const int MaximumDecimalPlaces = 6;

        public static ItemStatRangeSchemaSnapshot Create(
            IEnumerable<ItemStatDefinitionSnapshot> definitions,
            IEnumerable<ItemStatRangeProfileSnapshot> profiles)
        {
            List<ItemStatRangeValidationError> errors = new();
            List<ItemStatDefinitionSnapshot> stableDefinitions = NormalizeDefinitions(definitions, errors);
            List<ItemStatRangeProfileSnapshot> stableProfiles = NormalizeProfiles(profiles, errors);
            ValidateDefinitions(stableDefinitions, errors);
            ValidateProfiles(stableDefinitions, stableProfiles, errors);
            return new ItemStatRangeSchemaSnapshot(stableDefinitions, stableProfiles, errors);
        }

        private static List<ItemStatDefinitionSnapshot> NormalizeDefinitions(
            IEnumerable<ItemStatDefinitionSnapshot> definitions,
            List<ItemStatRangeValidationError> errors)
        {
            List<ItemStatDefinitionSnapshot> output = new();
            foreach (ItemStatDefinitionSnapshot definition in definitions ?? Array.Empty<ItemStatDefinitionSnapshot>())
            {
                if (definition == null)
                {
                    errors.Add(Error(ItemStatRangeValidationCodes.DefinitionNull, "A stat definition entry is null."));
                    continue;
                }

                output.Add(new ItemStatDefinitionSnapshot(
                    definition.statId,
                    definition.displayName,
                    definition.unitKey,
                    definition.direction,
                    definition.decimalPlaces,
                    definition.stepUnits,
                    definition.roundingMode,
                    definition.dataMaturity));
            }

            return output
                .OrderBy(definition => definition.statId, StringComparer.Ordinal)
                .ThenBy(definition => definition.displayName, StringComparer.Ordinal)
                .ThenBy(definition => definition.unitKey, StringComparer.Ordinal)
                .ThenBy(definition => (int)definition.direction)
                .ThenBy(definition => definition.decimalPlaces)
                .ThenBy(definition => definition.stepUnits)
                .ThenBy(definition => (int)definition.roundingMode)
                .ThenBy(definition => (int)definition.dataMaturity)
                .ToList();
        }

        private static List<ItemStatRangeProfileSnapshot> NormalizeProfiles(
            IEnumerable<ItemStatRangeProfileSnapshot> profiles,
            List<ItemStatRangeValidationError> errors)
        {
            List<ItemStatRangeProfileSnapshot> output = new();
            foreach (ItemStatRangeProfileSnapshot profile in profiles ?? Array.Empty<ItemStatRangeProfileSnapshot>())
            {
                if (profile == null)
                {
                    errors.Add(Error(ItemStatRangeValidationCodes.ProfileNull, "A stat range profile entry is null."));
                    continue;
                }

                output.Add(new ItemStatRangeProfileSnapshot(
                    profile.baseItemId,
                    profile.statId,
                    profile.dataMaturity,
                    profile.totalRange,
                    profile.RarityRanges
                        .OrderBy(range => range.rarity.ToTierIndex())
                        .ThenBy(range => (int)range.rarity)));
            }

            return output
                .OrderBy(profile => profile.baseItemId, StringComparer.Ordinal)
                .ThenBy(profile => profile.statId, StringComparer.Ordinal)
                .ThenBy(profile => (int)profile.dataMaturity)
                .ThenBy(profile => profile.totalRange?.minUnits ?? long.MinValue)
                .ThenBy(profile => profile.totalRange?.maxUnits ?? long.MinValue)
                .ThenBy(ProfileRangeSortKey, StringComparer.Ordinal)
                .ToList();
        }

        private static string ProfileRangeSortKey(ItemStatRangeProfileSnapshot profile)
        {
            return string.Join(";", profile.RarityRanges
                .OrderBy(range => range.rarity.ToTierIndex())
                .ThenBy(range => (int)range.rarity)
                .ThenBy(range => range.minUnits)
                .ThenBy(range => range.maxUnits)
                .Select(range => string.Join(":",
                    ((int)range.rarity).ToString(CultureInfo.InvariantCulture),
                    range.minUnits.ToString(CultureInfo.InvariantCulture),
                    range.maxUnits.ToString(CultureInfo.InvariantCulture))));
        }

        private static void ValidateDefinitions(
            IReadOnlyList<ItemStatDefinitionSnapshot> definitions,
            List<ItemStatRangeValidationError> errors)
        {
            foreach (ItemStatDefinitionSnapshot definition in definitions)
            {
                string identity = string.IsNullOrWhiteSpace(definition.statId) ? "<empty>" : definition.statId;
                if (string.IsNullOrWhiteSpace(definition.statId))
                {
                    errors.Add(Error(ItemStatRangeValidationCodes.StatIdEmpty, "statId must not be empty."));
                }

                if (!Enum.IsDefined(typeof(ItemStatDirection), definition.direction))
                {
                    errors.Add(Error(ItemStatRangeValidationCodes.DirectionInvalid,
                        $"Stat '{identity}' has unsupported direction value {(int)definition.direction}."));
                }

                if (!Enum.IsDefined(typeof(ItemStatRoundingMode), definition.roundingMode))
                {
                    errors.Add(Error(ItemStatRangeValidationCodes.RoundingModeInvalid,
                        $"Stat '{identity}' has unsupported rounding mode value {(int)definition.roundingMode}."));
                }

                if (!Enum.IsDefined(typeof(ItemStatDataMaturity), definition.dataMaturity))
                {
                    errors.Add(Error(ItemStatRangeValidationCodes.DataMaturityInvalid,
                        $"Stat '{identity}' has unsupported data maturity value {(int)definition.dataMaturity}."));
                }

                if (definition.decimalPlaces < MinimumDecimalPlaces || definition.decimalPlaces > MaximumDecimalPlaces)
                {
                    errors.Add(Error(ItemStatRangeValidationCodes.DecimalPlacesInvalid,
                        $"Stat '{identity}' decimalPlaces must be between {MinimumDecimalPlaces} and {MaximumDecimalPlaces}."));
                }

                if (definition.stepUnits <= 0)
                {
                    errors.Add(Error(ItemStatRangeValidationCodes.StepUnitsInvalid,
                        $"Stat '{identity}' stepUnits must be greater than zero."));
                }
            }

            foreach (IGrouping<string, ItemStatDefinitionSnapshot> group in definitions
                .Where(definition => !string.IsNullOrWhiteSpace(definition.statId))
                .GroupBy(definition => definition.statId, StringComparer.Ordinal)
                .Where(group => group.Count() > 1))
            {
                errors.Add(Error(ItemStatRangeValidationCodes.StatIdDuplicate,
                    $"statId '{group.Key}' appears {group.Count()} times."));
            }
        }

        private static void ValidateProfiles(
            IReadOnlyList<ItemStatDefinitionSnapshot> definitions,
            IReadOnlyList<ItemStatRangeProfileSnapshot> profiles,
            List<ItemStatRangeValidationError> errors)
        {
            IReadOnlyDictionary<string, ItemStatDefinitionSnapshot> definitionsById = definitions
                .Where(definition => !string.IsNullOrWhiteSpace(definition.statId))
                .GroupBy(definition => definition.statId, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

            foreach (IGrouping<string, ItemStatRangeProfileSnapshot> group in profiles
                .GroupBy(profile => ProfileKey(profile.baseItemId, profile.statId), StringComparer.Ordinal)
                .Where(group => group.Count() > 1))
            {
                errors.Add(Error(ItemStatRangeValidationCodes.ProfileDuplicate,
                    $"Stat profile '{group.Key}' appears {group.Count()} times."));
            }

            foreach (ItemStatRangeProfileSnapshot profile in profiles)
            {
                string profileKey = ProfileKey(profile.baseItemId, profile.statId);
                if (string.IsNullOrWhiteSpace(profile.baseItemId))
                {
                    errors.Add(Error(ItemStatRangeValidationCodes.BaseItemIdEmpty,
                        $"Stat profile '{profileKey}' has an empty baseItemId."));
                }
                else if (string.Equals(profile.baseItemId, ItemRarityInstanceFoundation.CoreProgressionBaseItemId, StringComparison.Ordinal))
                {
                    errors.Add(Error(ItemStatRangeValidationCodes.BaseItemNotOrdinary,
                        "I031 is a directed core-progression item and cannot have an ordinary stat range profile."));
                }
                else if (!ItemRarityInstanceFoundation.IsOrdinaryBaseItemId(profile.baseItemId))
                {
                    errors.Add(Error(ItemStatRangeValidationCodes.BaseItemUnknown,
                        $"Unknown ordinary baseItemId '{profile.baseItemId}'."));
                }

                if (!definitionsById.TryGetValue(profile.statId, out ItemStatDefinitionSnapshot definition))
                {
                    errors.Add(Error(ItemStatRangeValidationCodes.StatDefinitionUnknown,
                        $"Stat profile '{profileKey}' references unknown statId '{profile.statId}'."));
                }

                if (!Enum.IsDefined(typeof(ItemStatDataMaturity), profile.dataMaturity))
                {
                    errors.Add(Error(ItemStatRangeValidationCodes.DataMaturityInvalid,
                        $"Stat profile '{profileKey}' has unsupported data maturity value {(int)profile.dataMaturity}."));
                }
                else if (profile.dataMaturity == ItemStatDataMaturity.SCHEMA_ONLY)
                {
                    errors.Add(Error(ItemStatRangeValidationCodes.NumericProfileSchemaOnly,
                        $"Stat profile '{profileKey}' contains numeric ranges and cannot be marked SCHEMA_ONLY."));
                }

                ValidateRanges(profile, definition, errors);
            }
        }

        private static void ValidateRanges(
            ItemStatRangeProfileSnapshot profile,
            ItemStatDefinitionSnapshot definition,
            List<ItemStatRangeValidationError> errors)
        {
            string profileKey = ProfileKey(profile.baseItemId, profile.statId);
            if (profile.totalRange == null || profile.totalRange.minUnits > profile.totalRange.maxUnits)
            {
                errors.Add(Error(ItemStatRangeValidationCodes.TotalRangeInvalid,
                    $"Stat profile '{profileKey}' must have totalRange.minUnits <= totalRange.maxUnits."));
            }

            foreach (ItemRarityStatRangeSnapshot range in profile.RarityRanges)
            {
                if (!ItemInstanceRarityCatalog.TryGetDefinition(range.rarity, out _))
                {
                    errors.Add(Error(ItemStatRangeValidationCodes.RarityInvalid,
                        $"Stat profile '{profileKey}' contains unsupported rarity value {(int)range.rarity}."));
                }

                if (range.minUnits > range.maxUnits)
                {
                    errors.Add(Error(ItemStatRangeValidationCodes.RarityRangeInvalid,
                        $"Stat profile '{profileKey}' rarity '{StableRarity(range.rarity)}' has minUnits greater than maxUnits."));
                }

                if (profile.totalRange != null
                    && (range.minUnits < profile.totalRange.minUnits || range.maxUnits > profile.totalRange.maxUnits))
                {
                    errors.Add(Error(ItemStatRangeValidationCodes.RangeOutsideTotal,
                        $"Stat profile '{profileKey}' rarity '{StableRarity(range.rarity)}' lies outside totalRange."));
                }

                if (definition != null && definition.stepUnits > 0
                    && (!IsStepAligned(range.minUnits, definition.stepUnits)
                        || !IsStepAligned(range.maxUnits, definition.stepUnits)))
                {
                    errors.Add(Error(ItemStatRangeValidationCodes.RangeStepMisaligned,
                        $"Stat profile '{profileKey}' rarity '{StableRarity(range.rarity)}' endpoints must align to stepUnits {definition.stepUnits}."));
                }
            }

            if (definition != null && definition.stepUnits > 0 && profile.totalRange != null
                && (!IsStepAligned(profile.totalRange.minUnits, definition.stepUnits)
                    || !IsStepAligned(profile.totalRange.maxUnits, definition.stepUnits)))
            {
                errors.Add(Error(ItemStatRangeValidationCodes.RangeStepMisaligned,
                    $"Stat profile '{profileKey}' totalRange endpoints must align to stepUnits {definition.stepUnits}."));
            }

            foreach (IGrouping<ItemInstanceRarity, ItemRarityStatRangeSnapshot> group in profile.RarityRanges
                .Where(range => ItemInstanceRarityCatalog.TryGetDefinition(range.rarity, out _))
                .GroupBy(range => range.rarity)
                .Where(group => group.Count() > 1))
            {
                errors.Add(Error(ItemStatRangeValidationCodes.RarityDuplicate,
                    $"Stat profile '{profileKey}' rarity '{group.Key.ToStableKey()}' appears {group.Count()} times."));
            }

            ItemRarityStatRangeSnapshot[] ordered = profile.RarityRanges
                .Where(range => ItemInstanceRarityCatalog.TryGetDefinition(range.rarity, out _))
                .GroupBy(range => range.rarity)
                .Select(group => group.First())
                .OrderBy(range => range.rarity.ToTierIndex())
                .ToArray();
            if (ordered.Length != ItemInstanceRarityCatalog.All.Count)
            {
                errors.Add(Error(ItemStatRangeValidationCodes.RaritySetIncomplete,
                    $"Stat profile '{profileKey}' must contain exactly white/green/blue/purple/orange once each."));
            }

            if (ordered.Length == ItemInstanceRarityCatalog.All.Count && profile.totalRange != null)
            {
                long actualMin = ordered.Min(range => range.minUnits);
                long actualMax = ordered.Max(range => range.maxUnits);
                if (profile.totalRange.minUnits != actualMin || profile.totalRange.maxUnits != actualMax)
                {
                    errors.Add(Error(ItemStatRangeValidationCodes.TotalRangeEnvelopeMismatch,
                        $"Stat profile '{profileKey}' totalRange must equal the actual rarity-range envelope {actualMin}..{actualMax}."));
                }

                ValidateDirection(profileKey, definition, ordered, errors);
            }
        }

        private static void ValidateDirection(
            string profileKey,
            ItemStatDefinitionSnapshot definition,
            IReadOnlyList<ItemRarityStatRangeSnapshot> ordered,
            List<ItemStatRangeValidationError> errors)
        {
            if (definition == null || !Enum.IsDefined(typeof(ItemStatDirection), definition.direction))
            {
                return;
            }

            for (int index = 1; index < ordered.Count; index++)
            {
                ItemRarityStatRangeSnapshot previous = ordered[index - 1];
                ItemRarityStatRangeSnapshot current = ordered[index];
                if (definition.direction == ItemStatDirection.HigherIsBetter)
                {
                    if (current.minUnits < previous.minUnits)
                    {
                        errors.Add(Error(ItemStatRangeValidationCodes.HigherMinNotMonotonic,
                            $"Stat profile '{profileKey}' rarity '{current.rarity.ToStableKey()}' minUnits decreased from the previous tier."));
                    }

                    if (current.maxUnits < previous.maxUnits)
                    {
                        errors.Add(Error(ItemStatRangeValidationCodes.HigherMaxNotMonotonic,
                            $"Stat profile '{profileKey}' rarity '{current.rarity.ToStableKey()}' maxUnits decreased from the previous tier."));
                    }
                }
                else if (definition.direction == ItemStatDirection.LowerIsBetter)
                {
                    if (current.minUnits > previous.minUnits)
                    {
                        errors.Add(Error(ItemStatRangeValidationCodes.LowerMinNotMonotonic,
                            $"Stat profile '{profileKey}' rarity '{current.rarity.ToStableKey()}' minUnits increased from the previous tier."));
                    }

                    if (current.maxUnits > previous.maxUnits)
                    {
                        errors.Add(Error(ItemStatRangeValidationCodes.LowerMaxNotMonotonic,
                            $"Stat profile '{profileKey}' rarity '{current.rarity.ToStableKey()}' maxUnits increased from the previous tier."));
                    }
                }
            }
        }

        private static bool IsStepAligned(long value, long stepUnits)
        {
            return value % stepUnits == 0;
        }

        private static string StableRarity(ItemInstanceRarity rarity)
        {
            string stable = rarity.ToStableKey();
            return string.IsNullOrWhiteSpace(stable) ? ((int)rarity).ToString(CultureInfo.InvariantCulture) : stable;
        }

        private static string ProfileKey(string baseItemId, string statId)
        {
            return (baseItemId ?? string.Empty) + "@" + (statId ?? string.Empty);
        }

        private static ItemStatRangeValidationError Error(string code, string message)
        {
            return new ItemStatRangeValidationError(code, message);
        }
    }
}
