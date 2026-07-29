using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.EnemySystem.CapabilityRead;
using TalismanBag.EnemySystem.Vocabulary;
using TalismanBag.Items;
using TalismanBag.Items.Generation.Affixes;
using TalismanBag.Items.Generation.Potential;
using TalismanBag.Items.Generation.Projection;
using TalismanBag.Items.Generation.Stats;

namespace TalismanBag.CrossSystem.ItemEnemy
{
    public enum ItemBuildCapabilityMappingStatus
    {
        SUPPORTED = 0,
        KNOWN_ZERO = 1,
        UNKNOWN_NOT_MAPPED = 2,
        OUT_OF_SCOPE = 3
    }

    public sealed class ItemBuildCapabilityPlacementBindingSnapshot
    {
        public ItemBuildCapabilityPlacementBindingSnapshot(
            string itemInstanceId,
            string placementId)
        {
            ItemInstanceId = Text(itemInstanceId);
            PlacementId = Text(placementId);
        }

        public string ItemInstanceId { get; }
        public string PlacementId { get; }

        internal ItemBuildCapabilityPlacementBindingSnapshot Clone()
        {
            return new ItemBuildCapabilityPlacementBindingSnapshot(
                ItemInstanceId,
                PlacementId);
        }

        private static string Text(string value)
        {
            return value == null ? string.Empty : value.Trim();
        }
    }

    public sealed class ItemBuildCapabilityProjectionInput
    {
        private readonly ReadOnlyCollection<ItemInstanceProjectionContractSnapshot>
            itemInstances;
        private readonly ReadOnlyCollection<ItemBuildCapabilityPlacementBindingSnapshot>
            placementBindings;

        public ItemBuildCapabilityProjectionInput(
            IEnumerable<ItemInstanceProjectionContractSnapshot> itemInstances,
            ItemAffixPoolAndRangeSchemaSnapshot affixSchema,
            ItemSystemSnapshot itemSystemSnapshot = null,
            IEnumerable<ItemBuildCapabilityPlacementBindingSnapshot> placementBindings = null)
        {
            HasItemInstanceInput = itemInstances != null;
            this.itemInstances = Array.AsReadOnly(
                (itemInstances ?? Array.Empty<ItemInstanceProjectionContractSnapshot>())
                .OrderBy(value => value == null ? string.Empty : value.itemInstanceId,
                    StringComparer.Ordinal)
                .ThenBy(value => value == null ? string.Empty : value.sourceCanonicalSignature,
                    StringComparer.Ordinal)
                .ToArray());
            AffixSchema = affixSchema;
            ItemSystemSnapshot = itemSystemSnapshot;
            HasPlacementBindingInput = placementBindings != null;
            this.placementBindings = Array.AsReadOnly(
                (placementBindings ?? Array.Empty<ItemBuildCapabilityPlacementBindingSnapshot>())
                .Select(value => value == null ? null : value.Clone())
                .OrderBy(value => value == null ? string.Empty : value.ItemInstanceId,
                    StringComparer.Ordinal)
                .ThenBy(value => value == null ? string.Empty : value.PlacementId,
                    StringComparer.Ordinal)
                .ToArray());
        }

        public bool HasItemInstanceInput { get; }
        public IReadOnlyList<ItemInstanceProjectionContractSnapshot> ItemInstances =>
            itemInstances;
        public ItemAffixPoolAndRangeSchemaSnapshot AffixSchema { get; }
        public ItemSystemSnapshot ItemSystemSnapshot { get; }
        public bool HasPlacementBindingInput { get; }
        public IReadOnlyList<ItemBuildCapabilityPlacementBindingSnapshot> PlacementBindings =>
            placementBindings;
    }

    public sealed class ItemBuildCapabilityProjectionFieldMapSnapshot
    {
        public ItemBuildCapabilityProjectionFieldMapSnapshot(
            string targetCapabilityKey,
            ItemBuildCapabilityMappingStatus mappingStatus,
            string sourceContract,
            string sourceFieldOrStableKey,
            string calculationRuleId,
            string availabilityPolicy,
            string valueScale,
            string sourceCategoryId,
            string notes)
        {
            TargetCapabilityKey = Text(targetCapabilityKey);
            MappingStatus = mappingStatus;
            SourceContract = Text(sourceContract);
            SourceFieldOrStableKey = Text(sourceFieldOrStableKey);
            CalculationRuleId = Text(calculationRuleId);
            AvailabilityPolicy = Text(availabilityPolicy);
            ValueScale = Text(valueScale);
            SourceCategoryId = Text(sourceCategoryId);
            Notes = notes ?? string.Empty;
        }

        public string TargetCapabilityKey { get; }
        public ItemBuildCapabilityMappingStatus MappingStatus { get; }
        public string SourceContract { get; }
        public string SourceFieldOrStableKey { get; }
        public string CalculationRuleId { get; }
        public string AvailabilityPolicy { get; }
        public string ValueScale { get; }
        public string SourceCategoryId { get; }
        public string Notes { get; }

        private static string Text(string value)
        {
            return value == null ? string.Empty : value.Trim();
        }
    }

    public sealed class ItemBuildCapabilityUnknownDiagnosticSnapshot
    {
        public ItemBuildCapabilityUnknownDiagnosticSnapshot(
            string code,
            string targetCapabilityKey,
            string sourceStableKey,
            string detail)
        {
            Code = Text(code);
            TargetCapabilityKey = Text(targetCapabilityKey);
            SourceStableKey = Text(sourceStableKey);
            Detail = detail ?? string.Empty;
        }

        public string Code { get; }
        public string TargetCapabilityKey { get; }
        public string SourceStableKey { get; }
        public string Detail { get; }

        private static string Text(string value)
        {
            return value == null ? string.Empty : value.Trim();
        }
    }

    public sealed class ItemBuildCapabilityProjectionResult
    {
        private readonly ReadOnlyCollection<ItemBuildCapabilityProjectionFieldMapSnapshot>
            fieldMap;
        private readonly ReadOnlyCollection<ItemBuildCapabilityUnknownDiagnosticSnapshot>
            unknownDiagnostics;

        internal ItemBuildCapabilityProjectionResult(
            string mappingVersion,
            string sourceRevisionId,
            BuildCapabilitySnapshot snapshot,
            IEnumerable<ItemBuildCapabilityProjectionFieldMapSnapshot> fieldMap,
            IEnumerable<ItemBuildCapabilityUnknownDiagnosticSnapshot> unknownDiagnostics)
        {
            MappingVersion = mappingVersion ?? string.Empty;
            SourceRevisionId = sourceRevisionId ?? string.Empty;
            Snapshot = snapshot;
            this.fieldMap = Array.AsReadOnly((fieldMap ??
                    Array.Empty<ItemBuildCapabilityProjectionFieldMapSnapshot>())
                .OrderBy(value => value.TargetCapabilityKey, StringComparer.Ordinal)
                .ToArray());
            this.unknownDiagnostics = Array.AsReadOnly((unknownDiagnostics ??
                    Array.Empty<ItemBuildCapabilityUnknownDiagnosticSnapshot>())
                .OrderBy(value => value.Code, StringComparer.Ordinal)
                .ThenBy(value => value.TargetCapabilityKey, StringComparer.Ordinal)
                .ThenBy(value => value.SourceStableKey, StringComparer.Ordinal)
                .ThenBy(value => value.Detail, StringComparer.Ordinal)
                .ToArray());
        }

        public string MappingVersion { get; }
        public string SourceRevisionId { get; }
        public BuildCapabilitySnapshot Snapshot { get; }
        public IReadOnlyList<ItemBuildCapabilityProjectionFieldMapSnapshot> FieldMap =>
            fieldMap;
        public IReadOnlyList<ItemBuildCapabilityUnknownDiagnosticSnapshot> UnknownDiagnostics =>
            unknownDiagnostics;
    }

    public sealed class DefaultItemEnemyBuildCapabilityVocabularyResolver :
        IBuildCapabilityVocabularyResolver
    {
        public static readonly DefaultItemEnemyBuildCapabilityVocabularyResolver Instance =
            new DefaultItemEnemyBuildCapabilityVocabularyResolver();

        private readonly ReadOnlyCollection<string> knownKeys;
        private readonly HashSet<string> knownKeySet;

        public DefaultItemEnemyBuildCapabilityVocabularyResolver()
        {
            string[] keys = DefaultEnemyMechanicVocabularyCatalog.CreateEntries()
                .Where(value => value != null
                    && value.Category == EnemyVocabularyCategory.BuildCapability)
                .Select(value => value.StableKey)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            knownKeys = Array.AsReadOnly(keys);
            knownKeySet = new HashSet<string>(keys, StringComparer.Ordinal);
        }

        public bool HasBuildCapabilityKey(string key)
        {
            return key != null && knownKeySet.Contains(key);
        }

        public IReadOnlyList<string> GetKnownBuildCapabilityKeys()
        {
            return knownKeys;
        }
    }

    public sealed class DefaultItemBuildCapabilityProjectionAdapter
    {
        public const string MappingVersion =
            "ItemBuildCapabilityProjectionAdapter.mapping.v1";

        private const string ProjectionAndAffixSourceContract =
            ItemInstanceProjectionContractSnapshot.CurrentSchemaId
            + "+"
            + ItemAffixPoolAndRangeSchemaSnapshot.CurrentSchemaId;
        private const string AffixSourceCategory = "item.affix.basis_point";
        private const string UnknownAvailability = "UNKNOWN_SPARSE_OMISSION";
        private const string KnownAvailability =
            "KNOWN_WHEN_PROJECTION_SET_AND_AFFIX_SCHEMA_ARE_COMPLETE";
        private const string BasisPointScale =
            "DIRECT_BASIS_POINT_SATURATING_SUM_0_10000";

        private static readonly ReadOnlyCollection<ProjectionRule> Rules =
            Array.AsReadOnly(new[]
            {
                new ProjectionRule(
                    "capability.break_power",
                    "affix_break_up",
                    "AFFIX_BREAK_BASIS_POINT_SUM_V1"),
                new ProjectionRule(
                    "capability.guard_power",
                    "affix_guard_up",
                    "AFFIX_GUARD_BASIS_POINT_SUM_V1"),
                new ProjectionRule(
                    "capability.cooldown_recovery",
                    "affix_cooldown_reduction",
                    "AFFIX_COOLDOWN_REDUCTION_BASIS_POINT_SUM_V1")
            });

        private static readonly HashSet<string> OutOfScopeCapabilityKeys =
            new HashSet<string>(new[]
            {
                "capability.interrupt_timing",
                "capability.caster_interrupt"
            }, StringComparer.Ordinal);

        private readonly IBuildCapabilityVocabularyResolver resolver;
        private readonly IBuildCapabilitySnapshotProvider snapshotProvider;

        public DefaultItemBuildCapabilityProjectionAdapter(
            IBuildCapabilityVocabularyResolver resolver = null,
            IBuildCapabilitySnapshotProvider snapshotProvider = null)
        {
            this.resolver = resolver ??
                DefaultItemEnemyBuildCapabilityVocabularyResolver.Instance;
            this.snapshotProvider = snapshotProvider ??
                DefaultBuildCapabilitySnapshotProvider.Instance;
        }

        public ItemBuildCapabilityProjectionResult Project(
            ItemBuildCapabilityProjectionInput input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            IReadOnlyList<string> knownKeys = resolver.GetKnownBuildCapabilityKeys();
            if (knownKeys == null)
            {
                throw new InvalidOperationException(
                    "Build capability resolver returned a null key collection.");
            }

            List<BuildCapabilityValueSnapshot> values =
                new List<BuildCapabilityValueSnapshot>();
            List<ItemBuildCapabilityProjectionFieldMapSnapshot> fieldMap =
                new List<ItemBuildCapabilityProjectionFieldMapSnapshot>();
            List<ItemBuildCapabilityUnknownDiagnosticSnapshot> diagnostics =
                new List<ItemBuildCapabilityUnknownDiagnosticSnapshot>();

            foreach (string capabilityKey in knownKeys
                .OrderBy(value => value, StringComparer.Ordinal))
            {
                if (string.IsNullOrWhiteSpace(capabilityKey)
                    || !resolver.HasBuildCapabilityKey(capabilityKey))
                {
                    throw new InvalidOperationException(
                        "Build capability resolver emitted an invalid key.");
                }

                ProjectionRule rule = Rules.FirstOrDefault(value =>
                    string.Equals(value.TargetCapabilityKey, capabilityKey,
                        StringComparison.Ordinal));
                if (rule != null)
                {
                    EvaluateRule(input, rule, values, fieldMap, diagnostics);
                    continue;
                }

                bool outOfScope = OutOfScopeCapabilityKeys.Contains(capabilityKey);
                ItemBuildCapabilityMappingStatus status = outOfScope
                    ? ItemBuildCapabilityMappingStatus.OUT_OF_SCOPE
                    : ItemBuildCapabilityMappingStatus.UNKNOWN_NOT_MAPPED;
                string code = outOfScope
                    ? "CAPABILITY_REQUIRES_RUNTIME_TIMING_FACT"
                    : "CAPABILITY_RULE_NOT_CONFIRMED";
                string note = outOfScope
                    ? "Runtime timing or caster-state evidence is outside this read-only adapter."
                    : "No stable, unit-safe Item-to-capability rule is confirmed; value is omitted.";
                fieldMap.Add(new ItemBuildCapabilityProjectionFieldMapSnapshot(
                    capabilityKey,
                    status,
                    ItemInstanceProjectionContractSnapshot.CurrentSchemaId,
                    "NONE_CONFIRMED",
                    "NONE",
                    UnknownAvailability,
                    "NONE",
                    "NONE",
                    note));
                diagnostics.Add(new ItemBuildCapabilityUnknownDiagnosticSnapshot(
                    code,
                    capabilityKey,
                    string.Empty,
                    note));
            }

            AddUnmappedSourceDiagnostics(input, diagnostics);
            string sourceRevisionId = BuildSourceRevisionId(input, knownKeys);
            BuildCapabilitySnapshotInput snapshotInput =
                new BuildCapabilitySnapshotInput(
                    "ibcp" + sourceRevisionId.Substring(4, 24),
                    sourceRevisionId,
                    BuildCapabilityCoverageMode.Sparse,
                    values,
                    true,
                    false,
                    false);
            BuildCapabilitySnapshot snapshot =
                snapshotProvider.CreateSnapshot(snapshotInput, resolver);

            return new ItemBuildCapabilityProjectionResult(
                MappingVersion,
                sourceRevisionId,
                snapshot,
                fieldMap,
                DeduplicateDiagnostics(diagnostics));
        }

        private static void EvaluateRule(
            ItemBuildCapabilityProjectionInput input,
            ProjectionRule rule,
            ICollection<BuildCapabilityValueSnapshot> values,
            ICollection<ItemBuildCapabilityProjectionFieldMapSnapshot> fieldMap,
            ICollection<ItemBuildCapabilityUnknownDiagnosticSnapshot> diagnostics)
        {
            RuleEvaluation evaluation = EvaluateRule(input, rule);
            if (!evaluation.IsKnown)
            {
                fieldMap.Add(new ItemBuildCapabilityProjectionFieldMapSnapshot(
                    rule.TargetCapabilityKey,
                    ItemBuildCapabilityMappingStatus.UNKNOWN_NOT_MAPPED,
                    ProjectionAndAffixSourceContract,
                    "Affixes.affixId=" + rule.SourceAffixId,
                    rule.CalculationRuleId,
                    KnownAvailability,
                    BasisPointScale,
                    AffixSourceCategory,
                    evaluation.Detail));
                diagnostics.Add(new ItemBuildCapabilityUnknownDiagnosticSnapshot(
                    evaluation.Code,
                    rule.TargetCapabilityKey,
                    rule.SourceAffixId,
                    evaluation.Detail));
                return;
            }

            ItemBuildCapabilityMappingStatus status =
                evaluation.ValueBasisPoints == 0
                    ? ItemBuildCapabilityMappingStatus.KNOWN_ZERO
                    : ItemBuildCapabilityMappingStatus.SUPPORTED;
            List<BuildCapabilitySourceSummarySnapshot> summaries =
                new List<BuildCapabilitySourceSummarySnapshot>();
            if (evaluation.SourceCount > 0)
            {
                summaries.Add(new BuildCapabilitySourceSummarySnapshot(
                    AffixSourceCategory,
                    evaluation.SourceCount,
                    evaluation.ValueBasisPoints,
                    false));
            }

            values.Add(new BuildCapabilityValueSnapshot(
                rule.TargetCapabilityKey,
                evaluation.ValueBasisPoints,
                summaries));
            fieldMap.Add(new ItemBuildCapabilityProjectionFieldMapSnapshot(
                rule.TargetCapabilityKey,
                status,
                ProjectionAndAffixSourceContract,
                "Affixes.affixId=" + rule.SourceAffixId,
                rule.CalculationRuleId,
                KnownAvailability,
                BasisPointScale,
                AffixSourceCategory,
                evaluation.ValueBasisPoints == 0
                    ? "Complete source input proves an explicit zero."
                    : "Direct basis-point sources are summed deterministically and saturated at 10000."));
        }

        private static RuleEvaluation EvaluateRule(
            ItemBuildCapabilityProjectionInput input,
            ProjectionRule rule)
        {
            if (!input.HasItemInstanceInput)
            {
                return RuleEvaluation.Unknown(
                    "ITEM_PROJECTION_SET_MISSING",
                    "The Item projection collection is missing; omission remains Unknown.");
            }

            if (input.ItemInstances.Any(value => value == null))
            {
                return RuleEvaluation.Unknown(
                    "ITEM_PROJECTION_ENTRY_NULL",
                    "A null Item projection prevents complete-source reasoning.");
            }

            if (input.ItemInstances.Any(value => !string.Equals(
                value.schemaId,
                ItemInstanceProjectionContractSnapshot.CurrentSchemaId,
                StringComparison.Ordinal)))
            {
                return RuleEvaluation.Unknown(
                    "ITEM_PROJECTION_SCHEMA_MISMATCH",
                    "At least one Item projection does not use the required source schema.");
            }

            if (input.ItemInstances.GroupBy(value => value.itemInstanceId,
                    StringComparer.Ordinal).Any(group =>
                        string.IsNullOrWhiteSpace(group.Key) || group.Count() > 1))
            {
                return RuleEvaluation.Unknown(
                    "ITEM_PROJECTION_ID_SET_INVALID",
                    "Item instance IDs must be non-empty and unique for complete-source reasoning.");
            }

            ItemAffixPoolAndRangeSchemaSnapshot schema = input.AffixSchema;
            if (schema == null)
            {
                return RuleEvaluation.Unknown(
                    "ITEM_AFFIX_SCHEMA_MISSING",
                    "Affix unit metadata is missing; rawUnits cannot be interpreted as basis points.");
            }

            if (!schema.isValid)
            {
                return RuleEvaluation.Unknown(
                    "ITEM_AFFIX_SCHEMA_INVALID",
                    "Affix unit metadata is invalid; rawUnits remain Unknown.");
            }

            ItemAffixQueryResult<ItemAffixDefinitionSnapshot> definitionResult =
                schema.QueryAffixDefinition(rule.SourceAffixId);
            if (!definitionResult.isSuccess || definitionResult.value == null)
            {
                return RuleEvaluation.Unknown(
                    "ITEM_AFFIX_DEFINITION_MISSING",
                    "The stable affix key is absent from the supplied read-only schema.");
            }

            ItemAffixDefinitionSnapshot definition = definitionResult.value;
            if (!string.Equals(definition.valueUnitKey, "basisPoint",
                StringComparison.Ordinal))
            {
                return RuleEvaluation.Unknown(
                    "ITEM_AFFIX_UNIT_NOT_BASIS_POINT",
                    "The stable affix exists but its unit is not basisPoint; no conversion is guessed.");
            }

            long total = 0L;
            int sourceCount = 0;
            foreach (ItemInstanceProjectionAffixSnapshot affix in
                input.ItemInstances.SelectMany(value => value.Affixes))
            {
                if (affix == null || !string.Equals(
                    affix.affixId,
                    rule.SourceAffixId,
                    StringComparison.Ordinal))
                {
                    continue;
                }

                if (affix.rawUnits < 0L)
                {
                    return RuleEvaluation.Unknown(
                        "ITEM_AFFIX_NEGATIVE_BASIS_POINT_UNSUPPORTED",
                        "A negative source value cannot be projected by the positive capability scale.");
                }

                sourceCount++;
                total = Math.Min(
                    BuildCapabilityScale.MaximumValueBasisPoints,
                    total + Math.Min(
                        BuildCapabilityScale.MaximumValueBasisPoints,
                        affix.rawUnits));
            }

            return RuleEvaluation.Known((int)total, sourceCount);
        }

        private static void AddUnmappedSourceDiagnostics(
            ItemBuildCapabilityProjectionInput input,
            ICollection<ItemBuildCapabilityUnknownDiagnosticSnapshot> diagnostics)
        {
            HashSet<string> mappedAffixIds = new HashSet<string>(
                Rules.Select(value => value.SourceAffixId),
                StringComparer.Ordinal);
            foreach (ItemInstanceProjectionContractSnapshot item in
                input.ItemInstances.Where(value => value != null))
            {
                foreach (ItemInstanceProjectionStatSnapshot stat in item.Stats)
                {
                    diagnostics.Add(new ItemBuildCapabilityUnknownDiagnosticSnapshot(
                        "UNMAPPED_STABLE_STAT_KEY",
                        string.Empty,
                        stat.statId,
                        "Stat rawUnits has no confirmed basis-point capability conversion."));
                }

                foreach (ItemInstanceProjectionAffixSnapshot affix in
                    item.Affixes.Where(value => value != null
                        && !mappedAffixIds.Contains(value.affixId)))
                {
                    diagnostics.Add(new ItemBuildCapabilityUnknownDiagnosticSnapshot(
                        "UNMAPPED_STABLE_AFFIX_KEY",
                        string.Empty,
                        affix.affixId,
                        "Stable affix key has no confirmed target capability rule."));
                }

                foreach (string coreEffectId in item.EligibleCoreEffectIds)
                {
                    diagnostics.Add(new ItemBuildCapabilityUnknownDiagnosticSnapshot(
                        "UNMAPPED_ELIGIBLE_CORE_EFFECT_KEY",
                        string.Empty,
                        coreEffectId,
                        "Eligibility does not prove an active capability contribution."));
                }

                foreach (string coreEffectId in item.VisibleCoreEffectIds)
                {
                    diagnostics.Add(new ItemBuildCapabilityUnknownDiagnosticSnapshot(
                        "UNMAPPED_VISIBLE_CORE_EFFECT_KEY",
                        string.Empty,
                        coreEffectId,
                        "Visible core effect has no confirmed capability conversion."));
                }

                if (item.buildQualification != ItemBuildQualification.None
                    && item.buildQualification != ItemBuildQualification.Unresolved)
                {
                    diagnostics.Add(new ItemBuildCapabilityUnknownDiagnosticSnapshot(
                        "BUILD_QUALIFICATION_RULE_UNCONFIRMED",
                        string.Empty,
                        item.buildQualification.ToString(),
                        "Build qualification is categorical and is not converted to capability BP."));
                }
            }

            AddItemSystemFactDiagnostics(input, diagnostics);
        }

        private static void AddItemSystemFactDiagnostics(
            ItemBuildCapabilityProjectionInput input,
            ICollection<ItemBuildCapabilityUnknownDiagnosticSnapshot> diagnostics)
        {
            if (input.ItemSystemSnapshot == null)
            {
                diagnostics.Add(new ItemBuildCapabilityUnknownDiagnosticSnapshot(
                    "ITEM_SYSTEM_BUILD_FACTS_MISSING",
                    string.Empty,
                    string.Empty,
                    "Shape, lighting, Build tag, and active-core facts are absent and remain Unknown."));
                return;
            }

            if (!input.HasPlacementBindingInput)
            {
                diagnostics.Add(new ItemBuildCapabilityUnknownDiagnosticSnapshot(
                    "INSTANCE_PLACEMENT_BINDING_MISSING",
                    string.Empty,
                    string.Empty,
                    "itemInstanceId is not guessed to equal placementId."));
                return;
            }

            IReadOnlyDictionary<string, ItemBuildCapabilityPlacementBindingSnapshot> bindings =
                input.PlacementBindings
                    .Where(value => value != null
                        && !string.IsNullOrWhiteSpace(value.ItemInstanceId))
                    .GroupBy(value => value.ItemInstanceId, StringComparer.Ordinal)
                    .ToDictionary(group => group.Key, group => group.First(),
                        StringComparer.Ordinal);
            IReadOnlyDictionary<string, ItemSystemPlacementSnapshot> placements =
                input.ItemSystemSnapshot.placements
                    .Where(value => value != null)
                    .GroupBy(value => value.placementId, StringComparer.Ordinal)
                    .ToDictionary(group => group.Key, group => group.First(),
                        StringComparer.Ordinal);

            foreach (ItemInstanceProjectionContractSnapshot item in
                input.ItemInstances.Where(value => value != null))
            {
                if (!bindings.TryGetValue(item.itemInstanceId,
                    out ItemBuildCapabilityPlacementBindingSnapshot binding))
                {
                    diagnostics.Add(new ItemBuildCapabilityUnknownDiagnosticSnapshot(
                        "INSTANCE_PLACEMENT_BINDING_NOT_FOUND",
                        string.Empty,
                        item.itemInstanceId,
                        "No explicit adapter-layer instance-to-placement binding was supplied."));
                    continue;
                }

                if (!placements.TryGetValue(binding.PlacementId,
                    out ItemSystemPlacementSnapshot placement))
                {
                    diagnostics.Add(new ItemBuildCapabilityUnknownDiagnosticSnapshot(
                        "BOUND_PLACEMENT_NOT_FOUND",
                        string.Empty,
                        binding.PlacementId,
                        "The explicit placement binding does not resolve in ItemSystemSnapshot."));
                    continue;
                }

                if (!string.Equals(item.baseItemId, placement.itemId,
                    StringComparison.Ordinal))
                {
                    diagnostics.Add(new ItemBuildCapabilityUnknownDiagnosticSnapshot(
                        "BOUND_BASE_ITEM_MISMATCH",
                        string.Empty,
                        item.itemInstanceId,
                        "The explicit placement resolves to a different stable base item ID."));
                    continue;
                }

                diagnostics.Add(new ItemBuildCapabilityUnknownDiagnosticSnapshot(
                    "SHAPE_LIGHTING_BUILD_RULE_UNCONFIRMED",
                    "capability.placement_shape",
                    placement.itemId,
                    "Stable placement facts were read, but no shape/lighting/Build-to-BP rule is locked."));
            }
        }

        private static IEnumerable<ItemBuildCapabilityUnknownDiagnosticSnapshot>
            DeduplicateDiagnostics(
                IEnumerable<ItemBuildCapabilityUnknownDiagnosticSnapshot> diagnostics)
        {
            return (diagnostics ?? Array.Empty<ItemBuildCapabilityUnknownDiagnosticSnapshot>())
                .Where(value => value != null)
                .GroupBy(value => string.Join("|", new[]
                {
                    value.Code,
                    value.TargetCapabilityKey,
                    value.SourceStableKey,
                    value.Detail
                }), StringComparer.Ordinal)
                .Select(group => group.First());
        }

        private static string BuildSourceRevisionId(
            ItemBuildCapabilityProjectionInput input,
            IEnumerable<string> knownKeys)
        {
            StringBuilder builder = new StringBuilder(8192);
            Append(builder, "mappingVersion", MappingVersion);
            Append(builder, "projectionInputPresent",
                input.HasItemInstanceInput ? "1" : "0");
            foreach (ItemInstanceProjectionContractSnapshot item in input.ItemInstances)
            {
                Append(builder, "projection",
                    item == null ? "<null>" : item.BuildCanonicalSignature());
            }

            Append(builder, "affixSchema",
                input.AffixSchema == null
                    ? "<missing>"
                    : input.AffixSchema.BuildCanonicalSignature());
            Append(builder, "itemSystemSnapshot",
                input.ItemSystemSnapshot == null
                    ? "<missing>"
                    : input.ItemSystemSnapshot.BuildDebugSignature());
            Append(builder, "bindingInputPresent",
                input.HasPlacementBindingInput ? "1" : "0");
            foreach (ItemBuildCapabilityPlacementBindingSnapshot binding in
                input.PlacementBindings)
            {
                Append(builder, "binding",
                    binding == null
                        ? "<null>"
                        : binding.ItemInstanceId + "=>" + binding.PlacementId);
            }

            foreach (string knownKey in (knownKeys ?? Array.Empty<string>())
                .OrderBy(value => value, StringComparer.Ordinal))
            {
                Append(builder, "resolverKey", knownKey);
            }

            return "ibcr" + Hash(builder.ToString());
        }

        private static void Append(StringBuilder builder, string name, string value)
        {
            string safeName = name ?? string.Empty;
            string safeValue = value ?? string.Empty;
            builder.Append(safeName.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':')
                .Append(safeName)
                .Append('=')
                .Append(safeValue.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':')
                .Append(safeValue)
                .Append(';');
        }

        private static string Hash(string value)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(
                    Encoding.UTF8.GetBytes(value ?? string.Empty));
                return string.Concat(bytes.Select(item =>
                    item.ToString("x2", CultureInfo.InvariantCulture)));
            }
        }

        private sealed class ProjectionRule
        {
            public ProjectionRule(
                string targetCapabilityKey,
                string sourceAffixId,
                string calculationRuleId)
            {
                TargetCapabilityKey = targetCapabilityKey;
                SourceAffixId = sourceAffixId;
                CalculationRuleId = calculationRuleId;
            }

            public string TargetCapabilityKey { get; }
            public string SourceAffixId { get; }
            public string CalculationRuleId { get; }
        }

        private sealed class RuleEvaluation
        {
            private RuleEvaluation(
                bool isKnown,
                int valueBasisPoints,
                int sourceCount,
                string code,
                string detail)
            {
                IsKnown = isKnown;
                ValueBasisPoints = valueBasisPoints;
                SourceCount = sourceCount;
                Code = code ?? string.Empty;
                Detail = detail ?? string.Empty;
            }

            public bool IsKnown { get; }
            public int ValueBasisPoints { get; }
            public int SourceCount { get; }
            public string Code { get; }
            public string Detail { get; }

            public static RuleEvaluation Known(int valueBasisPoints, int sourceCount)
            {
                return new RuleEvaluation(true, valueBasisPoints, sourceCount,
                    string.Empty, string.Empty);
            }

            public static RuleEvaluation Unknown(string code, string detail)
            {
                return new RuleEvaluation(false, 0, 0, code, detail);
            }
        }
    }
}
