using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using TalismanBag.Items.Generation.Affixes;
using TalismanBag.Items.Generation.Potential;
using TalismanBag.Items.Generation.Rolling;

namespace TalismanBag.Items.Generation.Projection
{
    public static class ItemInstanceProjectionValidationCodes
    {
        public const string None = "NONE";
        public const string RequestNull = "REQUEST_NULL";
        public const string GeneratedInstanceNull = "GENERATED_INSTANCE_NULL";
        public const string SourceSchemaInvalid = "SOURCE_SCHEMA_INVALID";
        public const string SourceAlgorithmIdEmpty = "SOURCE_ALGORITHM_ID_EMPTY";
        public const string GenerationDataStatusEmpty = "GENERATION_DATA_STATUS_EMPTY";
        public const string ItemInstanceIdEmpty = "ITEM_INSTANCE_ID_EMPTY";
        public const string BaseItemIdEmpty = "BASE_ITEM_ID_EMPTY";
        public const string BaseItemUnknown = "BASE_ITEM_UNKNOWN";
        public const string BaseItemNotOrdinary = "BASE_ITEM_NOT_ORDINARY";
        public const string I031Forbidden = "I031_FORBIDDEN";
        public const string RarityInvalid = "RARITY_INVALID";
        public const string RarityKeyInvalid = "RARITY_KEY_INVALID";
        public const string GenerationVersionInvalid = "GENERATION_VERSION_INVALID";
        public const string CultivationProfileIdEmpty = "CULTIVATION_PROFILE_ID_EMPTY";
        public const string StatEntryNull = "STAT_ENTRY_NULL";
        public const string StatIdEmpty = "STAT_ID_EMPTY";
        public const string StatIdDuplicate = "STAT_ID_DUPLICATE";
        public const string AffixEntryNull = "AFFIX_ENTRY_NULL";
        public const string AffixSlotIdEmpty = "AFFIX_SLOT_ID_EMPTY";
        public const string AffixSlotIdDuplicate = "AFFIX_SLOT_ID_DUPLICATE";
        public const string AffixSlotKindInvalid = "AFFIX_SLOT_KIND_INVALID";
        public const string AffixIdEmpty = "AFFIX_ID_EMPTY";
        public const string AffixValueProfileIdEmpty = "AFFIX_VALUE_PROFILE_ID_EMPTY";
        public const string CorePotentialNull = "CORE_POTENTIAL_NULL";
        public const string CoreEligibleIdEmpty = "CORE_ELIGIBLE_ID_EMPTY";
        public const string CoreEligibleIdDuplicate = "CORE_ELIGIBLE_ID_DUPLICATE";
        public const string CoreVisibleIdEmpty = "CORE_VISIBLE_ID_EMPTY";
        public const string CoreVisibleIdDuplicate = "CORE_VISIBLE_ID_DUPLICATE";
        public const string CoreVisibleNotEligible = "CORE_VISIBLE_NOT_ELIGIBLE";
        public const string BuildQualificationInvalid = "BUILD_QUALIFICATION_INVALID";
        public const string WhiteBuildQualificationMustBeNone = "WHITE_BUILD_QUALIFICATION_MUST_BE_NONE";
        public const string SetInputNull = "SET_INPUT_NULL";
        public const string SetEntryNull = "SET_ENTRY_NULL";
        public const string ItemInstanceIdDuplicate = "ITEM_INSTANCE_ID_DUPLICATE";
        public const string QueryItemInstanceIdEmpty = "QUERY_ITEM_INSTANCE_ID_EMPTY";
        public const string QueryBaseItemIdEmpty = "QUERY_BASE_ITEM_ID_EMPTY";
        public const string QueryRarityInvalid = "QUERY_RARITY_INVALID";
        public const string QueryNotFound = "QUERY_NOT_FOUND";
        public const string ProjectionInternalError = "PROJECTION_INTERNAL_ERROR";
    }

    public sealed class ItemInstanceProjectionValidationError
    {
        public ItemInstanceProjectionValidationError(string code, string message)
        {
            this.code = code ?? string.Empty;
            this.message = message ?? string.Empty;
        }

        public string code { get; }
        public string message { get; }
    }

    public sealed class ItemInstanceProjectionRequest
    {
        public ItemInstanceProjectionRequest(ItemGeneratedInstanceSnapshot generatedInstance)
        {
            this.generatedInstance = generatedInstance;
        }

        public ItemGeneratedInstanceSnapshot generatedInstance { get; }
    }

    public sealed class ItemInstanceProjectionSetRequest
    {
        private readonly ReadOnlyCollection<ItemGeneratedInstanceSnapshot> generatedInstances;

        public ItemInstanceProjectionSetRequest(IEnumerable<ItemGeneratedInstanceSnapshot> generatedInstances)
        {
            hasInput = generatedInstances != null;
            this.generatedInstances = hasInput
                ? ProjectionReadOnly.Freeze(generatedInstances)
                : null;
        }

        public bool hasInput { get; }
        public IReadOnlyList<ItemGeneratedInstanceSnapshot> GeneratedInstances => generatedInstances;
    }

    public sealed class ItemInstanceProjectionStatSnapshot
    {
        internal ItemInstanceProjectionStatSnapshot(string statId, long rawUnits)
        {
            this.statId = statId ?? string.Empty;
            this.rawUnits = rawUnits;
        }

        public string statId { get; }
        public long rawUnits { get; }
    }

    public sealed class ItemInstanceProjectionAffixSnapshot
    {
        internal ItemInstanceProjectionAffixSnapshot(
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

    public sealed class ItemInstanceProjectionContractSnapshot
    {
        public const string CurrentSchemaId = "ItemInstanceProjectionContractSnapshot.v1";

        private readonly ReadOnlyCollection<ItemInstanceProjectionStatSnapshot> stats;
        private readonly ReadOnlyCollection<ItemInstanceProjectionAffixSnapshot> affixes;
        private readonly ReadOnlyCollection<string> eligibleCoreEffectIds;
        private readonly ReadOnlyCollection<string> visibleCoreEffectIds;

        internal ItemInstanceProjectionContractSnapshot(
            string sourceSchemaId,
            string sourceGenerationAlgorithmId,
            string generationDataStatus,
            string itemInstanceId,
            string baseItemId,
            ItemInstanceRarity rarity,
            string rarityKey,
            int generationVersion,
            long rootSeed,
            string cultivationPotentialProfileId,
            IEnumerable<ItemInstanceProjectionStatSnapshot> stats,
            IEnumerable<ItemInstanceProjectionAffixSnapshot> affixes,
            IEnumerable<string> eligibleCoreEffectIds,
            IEnumerable<string> visibleCoreEffectIds,
            ItemBuildQualification buildQualification,
            string sourceCanonicalSignature)
        {
            schemaId = CurrentSchemaId;
            this.sourceSchemaId = sourceSchemaId ?? string.Empty;
            this.sourceGenerationAlgorithmId = sourceGenerationAlgorithmId ?? string.Empty;
            this.generationDataStatus = generationDataStatus ?? string.Empty;
            this.itemInstanceId = itemInstanceId ?? string.Empty;
            this.baseItemId = baseItemId ?? string.Empty;
            this.rarity = rarity;
            this.rarityKey = rarityKey ?? string.Empty;
            this.generationVersion = generationVersion;
            this.rootSeed = rootSeed;
            this.cultivationPotentialProfileId = cultivationPotentialProfileId ?? string.Empty;
            this.stats = ProjectionReadOnly.Freeze((stats ?? Array.Empty<ItemInstanceProjectionStatSnapshot>())
                .Where(value => value != null)
                .OrderBy(value => value.statId, StringComparer.Ordinal)
                .Select(value => new ItemInstanceProjectionStatSnapshot(value.statId, value.rawUnits)));
            this.affixes = ProjectionReadOnly.Freeze((affixes ?? Array.Empty<ItemInstanceProjectionAffixSnapshot>())
                .Where(value => value != null)
                .OrderBy(value => value.slotId, StringComparer.Ordinal)
                .ThenBy(value => (int)value.slotKind)
                .ThenBy(value => value.affixId, StringComparer.Ordinal)
                .Select(value => new ItemInstanceProjectionAffixSnapshot(value.slotId, value.slotKind,
                    value.affixId, value.affixValueProfileId, value.rawUnits)));
            this.eligibleCoreEffectIds = ProjectionReadOnly.Freeze((eligibleCoreEffectIds ?? Array.Empty<string>())
                .Select(value => value ?? string.Empty)
                .OrderBy(value => value, StringComparer.Ordinal));
            this.visibleCoreEffectIds = ProjectionReadOnly.Freeze((visibleCoreEffectIds ?? Array.Empty<string>())
                .Select(value => value ?? string.Empty)
                .OrderBy(value => value, StringComparer.Ordinal));
            this.buildQualification = buildQualification;
            this.sourceCanonicalSignature = sourceCanonicalSignature ?? string.Empty;
        }

        public string schemaId { get; }
        public string sourceSchemaId { get; }
        public string sourceGenerationAlgorithmId { get; }
        public string generationDataStatus { get; }
        public string itemInstanceId { get; }
        public string baseItemId { get; }
        public ItemInstanceRarity rarity { get; }
        public string rarityKey { get; }
        public int generationVersion { get; }
        public long rootSeed { get; }
        public string cultivationPotentialProfileId { get; }
        public IReadOnlyList<ItemInstanceProjectionStatSnapshot> Stats => stats;
        public IReadOnlyList<ItemInstanceProjectionAffixSnapshot> Affixes => affixes;
        public IReadOnlyList<string> EligibleCoreEffectIds => eligibleCoreEffectIds;
        public IReadOnlyList<string> VisibleCoreEffectIds => visibleCoreEffectIds;
        public ItemBuildQualification buildQualification { get; }
        public string sourceCanonicalSignature { get; }

        public string BuildCanonicalSignature()
        {
            StringBuilder builder = new();
            ProjectionCanonical.AppendField(builder, "schemaId", schemaId);
            ProjectionCanonical.AppendField(builder, "sourceSchemaId", sourceSchemaId);
            ProjectionCanonical.AppendField(builder, "sourceGenerationAlgorithmId", sourceGenerationAlgorithmId);
            ProjectionCanonical.AppendField(builder, "generationDataStatus", generationDataStatus);
            ProjectionCanonical.AppendField(builder, "itemInstanceId", itemInstanceId);
            ProjectionCanonical.AppendField(builder, "baseItemId", baseItemId);
            ProjectionCanonical.AppendField(builder, "rarity", rarity.ToString());
            ProjectionCanonical.AppendField(builder, "rarityKey", rarityKey);
            ProjectionCanonical.AppendField(builder, "generationVersion",
                generationVersion.ToString(CultureInfo.InvariantCulture));
            ProjectionCanonical.AppendField(builder, "rootSeed", rootSeed.ToString(CultureInfo.InvariantCulture));
            ProjectionCanonical.AppendField(builder, "cultivationPotentialProfileId", cultivationPotentialProfileId);
            foreach (ItemInstanceProjectionStatSnapshot stat in stats)
            {
                ProjectionCanonical.AppendField(builder, "statId", stat.statId);
                ProjectionCanonical.AppendField(builder, "statRawUnits", stat.rawUnits.ToString(CultureInfo.InvariantCulture));
            }

            foreach (ItemInstanceProjectionAffixSnapshot affix in affixes)
            {
                ProjectionCanonical.AppendField(builder, "affixSlotId", affix.slotId);
                ProjectionCanonical.AppendField(builder, "affixSlotKind", affix.slotKind.ToString());
                ProjectionCanonical.AppendField(builder, "affixId", affix.affixId);
                ProjectionCanonical.AppendField(builder, "affixValueProfileId", affix.affixValueProfileId);
                ProjectionCanonical.AppendField(builder, "affixRawUnits", affix.rawUnits.ToString(CultureInfo.InvariantCulture));
            }

            foreach (string effectId in eligibleCoreEffectIds)
            {
                ProjectionCanonical.AppendField(builder, "eligibleCoreEffectId", effectId);
            }

            foreach (string effectId in visibleCoreEffectIds)
            {
                ProjectionCanonical.AppendField(builder, "visibleCoreEffectId", effectId);
            }

            ProjectionCanonical.AppendField(builder, "buildQualification", buildQualification.ToString());
            ProjectionCanonical.AppendField(builder, "sourceCanonicalSignature", sourceCanonicalSignature);
            return builder.ToString();
        }
    }

    public sealed class ItemInstanceProjectionQueryResult
    {
        private readonly ReadOnlyCollection<ItemInstanceProjectionContractSnapshot> snapshots;
        private readonly ReadOnlyCollection<ItemInstanceProjectionValidationError> validationErrors;

        internal ItemInstanceProjectionQueryResult(
            ItemInstanceProjectionContractSnapshot snapshot,
            IEnumerable<ItemInstanceProjectionContractSnapshot> snapshots,
            IEnumerable<ItemInstanceProjectionValidationError> validationErrors)
        {
            this.snapshot = snapshot;
            this.snapshots = ProjectionReadOnly.Freeze((snapshots ?? Array.Empty<ItemInstanceProjectionContractSnapshot>())
                .Where(value => value != null)
                .OrderBy(value => value.itemInstanceId, StringComparer.Ordinal));
            this.validationErrors = ProjectionReadOnly.Freeze(validationErrors);
        }

        public bool isSuccess => validationErrors.Count == 0 && (snapshot != null || snapshots.Count > 0);
        public ItemInstanceProjectionContractSnapshot snapshot { get; }
        public IReadOnlyList<ItemInstanceProjectionContractSnapshot> Snapshots => snapshots;
        public IReadOnlyList<ItemInstanceProjectionValidationError> ValidationErrors => validationErrors;
        public string primaryValidationCode => validationErrors.Count == 0
            ? ItemInstanceProjectionValidationCodes.None
            : validationErrors[0].code;
    }

    public sealed class ItemInstanceProjectionSetSnapshot
    {
        public const string CurrentSchemaId = "ItemInstanceProjectionSetSnapshot.v1";

        private readonly ReadOnlyCollection<ItemInstanceProjectionContractSnapshot> projections;
        private readonly ReadOnlyCollection<ItemInstanceProjectionValidationError> validationErrors;

        internal ItemInstanceProjectionSetSnapshot(
            IEnumerable<ItemInstanceProjectionContractSnapshot> projections,
            IEnumerable<ItemInstanceProjectionValidationError> validationErrors)
        {
            schemaId = CurrentSchemaId;
            this.projections = ProjectionReadOnly.Freeze((projections ?? Array.Empty<ItemInstanceProjectionContractSnapshot>())
                .Where(value => value != null)
                .OrderBy(value => value.itemInstanceId, StringComparer.Ordinal));
            this.validationErrors = ProjectionReadOnly.Freeze(validationErrors);
        }

        public string schemaId { get; }
        public bool isValid => validationErrors.Count == 0;
        public IReadOnlyList<ItemInstanceProjectionContractSnapshot> Projections => projections;
        public IReadOnlyList<ItemInstanceProjectionValidationError> ValidationErrors => validationErrors;

        public ItemInstanceProjectionQueryResult QueryByItemInstanceId(string itemInstanceId)
        {
            string normalized = ProjectionCanonical.Normalize(itemInstanceId);
            if (normalized.Length == 0)
            {
                return QueryFailure(ItemInstanceProjectionValidationCodes.QueryItemInstanceIdEmpty,
                    "itemInstanceId is required for an exact projection query.");
            }

            ItemInstanceProjectionContractSnapshot match = projections.FirstOrDefault(value =>
                string.Equals(value.itemInstanceId, normalized, StringComparison.Ordinal));
            return match == null
                ? QueryFailure(ItemInstanceProjectionValidationCodes.QueryNotFound,
                    $"No projection exists for itemInstanceId '{normalized}'.")
                : new ItemInstanceProjectionQueryResult(match, new[] { match },
                    Array.Empty<ItemInstanceProjectionValidationError>());
        }

        public ItemInstanceProjectionQueryResult QueryByBaseItemId(string baseItemId)
        {
            string normalized = ProjectionCanonical.Normalize(baseItemId);
            if (normalized.Length == 0)
            {
                return QueryFailure(ItemInstanceProjectionValidationCodes.QueryBaseItemIdEmpty,
                    "baseItemId is required for a projection query.");
            }

            ItemInstanceProjectionContractSnapshot[] matches = projections.Where(value =>
                    string.Equals(value.baseItemId, normalized, StringComparison.Ordinal))
                .OrderBy(value => value.itemInstanceId, StringComparer.Ordinal)
                .ToArray();
            return matches.Length == 0
                ? QueryFailure(ItemInstanceProjectionValidationCodes.QueryNotFound,
                    $"No projection exists for baseItemId '{normalized}'.")
                : new ItemInstanceProjectionQueryResult(null, matches,
                    Array.Empty<ItemInstanceProjectionValidationError>());
        }

        public ItemInstanceProjectionQueryResult QueryByBaseItemIdAndRarity(
            string baseItemId,
            ItemInstanceRarity rarity)
        {
            string normalized = ProjectionCanonical.Normalize(baseItemId);
            if (normalized.Length == 0)
            {
                return QueryFailure(ItemInstanceProjectionValidationCodes.QueryBaseItemIdEmpty,
                    "baseItemId is required for a projection query.");
            }

            if (!ItemInstanceRarityCatalog.TryGetDefinition(rarity, out _))
            {
                return QueryFailure(ItemInstanceProjectionValidationCodes.QueryRarityInvalid,
                    $"Rarity value '{rarity}' is not supported.");
            }

            ItemInstanceProjectionContractSnapshot[] matches = projections.Where(value =>
                    string.Equals(value.baseItemId, normalized, StringComparison.Ordinal) && value.rarity == rarity)
                .OrderBy(value => value.itemInstanceId, StringComparer.Ordinal)
                .ToArray();
            return matches.Length == 0
                ? QueryFailure(ItemInstanceProjectionValidationCodes.QueryNotFound,
                    $"No projection exists for baseItemId '{normalized}' and rarity '{rarity.ToStableKey()}'.")
                : new ItemInstanceProjectionQueryResult(null, matches,
                    Array.Empty<ItemInstanceProjectionValidationError>());
        }

        public string BuildCanonicalSignature()
        {
            StringBuilder builder = new();
            ProjectionCanonical.AppendField(builder, "schemaId", schemaId);
            foreach (ItemInstanceProjectionContractSnapshot projection in projections)
            {
                ProjectionCanonical.AppendField(builder, "projection", projection.BuildCanonicalSignature());
            }

            foreach (ItemInstanceProjectionValidationError error in validationErrors)
            {
                ProjectionCanonical.AppendField(builder, "validationCode", error.code);
                ProjectionCanonical.AppendField(builder, "validationMessage", error.message);
            }

            return builder.ToString();
        }

        private static ItemInstanceProjectionQueryResult QueryFailure(string code, string message)
        {
            return new ItemInstanceProjectionQueryResult(null,
                Array.Empty<ItemInstanceProjectionContractSnapshot>(),
                new[] { new ItemInstanceProjectionValidationError(code, message) });
        }
    }

    public sealed class ItemInstanceProjectionResult
    {
        private readonly ReadOnlyCollection<ItemInstanceProjectionValidationError> validationErrors;

        internal ItemInstanceProjectionResult(
            ItemInstanceProjectionContractSnapshot snapshot,
            IEnumerable<ItemInstanceProjectionValidationError> validationErrors)
        {
            this.snapshot = snapshot;
            this.validationErrors = ProjectionReadOnly.Freeze(validationErrors);
        }

        public bool isSuccess => snapshot != null && validationErrors.Count == 0;
        public ItemInstanceProjectionContractSnapshot snapshot { get; }
        public IReadOnlyList<ItemInstanceProjectionValidationError> ValidationErrors => validationErrors;
        public string primaryValidationCode => validationErrors.Count == 0
            ? ItemInstanceProjectionValidationCodes.None
            : validationErrors[0].code;
    }

    public sealed class ItemInstanceProjectionSetResult
    {
        private readonly ReadOnlyCollection<ItemInstanceProjectionValidationError> validationErrors;

        internal ItemInstanceProjectionSetResult(
            ItemInstanceProjectionSetSnapshot snapshot,
            IEnumerable<ItemInstanceProjectionValidationError> validationErrors)
        {
            this.snapshot = snapshot;
            this.validationErrors = ProjectionReadOnly.Freeze(validationErrors);
        }

        public bool isSuccess => snapshot != null && snapshot.isValid && validationErrors.Count == 0;
        public ItemInstanceProjectionSetSnapshot snapshot { get; }
        public IReadOnlyList<ItemInstanceProjectionValidationError> ValidationErrors => validationErrors;
        public string primaryValidationCode => validationErrors.Count == 0
            ? ItemInstanceProjectionValidationCodes.None
            : validationErrors[0].code;
    }

    internal static class ProjectionReadOnly
    {
        public static ReadOnlyCollection<T> Freeze<T>(IEnumerable<T> values)
        {
            return Array.AsReadOnly((values ?? Array.Empty<T>()).ToArray());
        }
    }

    internal static class ProjectionCanonical
    {
        public static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        public static void AppendField(StringBuilder builder, string key, string value)
        {
            string safeKey = key ?? string.Empty;
            string safeValue = value ?? string.Empty;
            builder.Append(safeKey.Length.ToString(CultureInfo.InvariantCulture)).Append(':').Append(safeKey)
                .Append('=')
                .Append(safeValue.Length.ToString(CultureInfo.InvariantCulture)).Append(':').Append(safeValue)
                .Append('\n');
        }
    }
}
