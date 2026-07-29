using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.EnemySystem.CapabilityRead;
using TalismanBag.EnemySystem.PressureWindow;
using TalismanBag.EnemySystem.ReadinessEvaluation;
using TalismanBag.EnemySystem.SeedData;
using TalismanBag.Items.Generation.Affixes;
using TalismanBag.Items.Generation.Projection;

namespace TalismanBag.CrossSystem.ItemEnemy
{
    public enum ItemEnemyMatchupEvaluationStatus
    {
        EVALUABLE_COMPLETE = 0,
        EVALUABLE_PARTIAL = 1,
        BLOCKED_BY_UNKNOWN = 2,
        NO_RELEVANT_SUPPORTED_CAPABILITY = 3,
        INVALID_INPUT = 4
    }

    public enum ItemEnemyMatchupUnknownBlockerKind
    {
        ITEM_CAPABILITY_MAPPING_MISSING = 0,
        ITEM_RUNTIME_FACT_MISSING = 1,
        ENCOUNTER_REQUIREMENT_UNKNOWN = 2,
        OUT_OF_SCOPE_RUNTIME_TIMING = 3,
        FIXTURE_SOURCE_BLOCKED = 4
    }

    public sealed class ItemEnemyMatchupScenarioInput
    {
        private readonly ReadOnlyCollection<ItemInstanceProjectionContractSnapshot> itemInstances;

        public ItemEnemyMatchupScenarioInput(
            string scenarioId,
            IEnumerable<ItemInstanceProjectionContractSnapshot> itemInstances,
            ItemAffixPoolAndRangeSchemaSnapshot affixSchema)
        {
            ScenarioId = Text(scenarioId);
            this.itemInstances = Array.AsReadOnly(
                (itemInstances ?? Array.Empty<ItemInstanceProjectionContractSnapshot>())
                .OrderBy(value => value == null ? string.Empty : value.itemInstanceId,
                    StringComparer.Ordinal)
                .ToArray());
            AffixSchema = affixSchema;
        }

        public string ScenarioId { get; }
        public IReadOnlyList<ItemInstanceProjectionContractSnapshot> ItemInstances => itemInstances;
        public ItemAffixPoolAndRangeSchemaSnapshot AffixSchema { get; }

        private static string Text(string value)
        {
            return value == null ? string.Empty : value.Trim();
        }
    }

    public sealed class ItemEnemyMatchupMatrixRowSnapshot
    {
        internal ItemEnemyMatchupMatrixRowSnapshot(
            string scenarioId,
            string itemSourceSignature,
            string mappingVersion,
            string enemySnapshotSignature,
            string encounterId,
            string chapterLabelDevOnly,
            ItemEnemyMatchupEvaluationStatus evaluationStatus,
            string readinessBand,
            int knownRequirementCount,
            int unknownRequirementCount,
            int metRequirementCount,
            int gapCount,
            IEnumerable<string> supportedCapabilities,
            IEnumerable<string> unknownCapabilities,
            IEnumerable<string> pressureResultSignatures,
            string notes)
        {
            ScenarioId = scenarioId ?? string.Empty;
            ItemSourceSignature = itemSourceSignature ?? string.Empty;
            MappingVersion = mappingVersion ?? string.Empty;
            EnemySnapshotSignature = enemySnapshotSignature ?? string.Empty;
            EncounterId = encounterId ?? string.Empty;
            ChapterLabelDevOnly = chapterLabelDevOnly ?? string.Empty;
            EvaluationStatus = evaluationStatus;
            ReadinessBand = readinessBand ?? string.Empty;
            KnownRequirementCount = knownRequirementCount;
            UnknownRequirementCount = unknownRequirementCount;
            MetRequirementCount = metRequirementCount;
            GapCount = gapCount;
            SupportedCapabilities = FreezeStrings(supportedCapabilities);
            UnknownCapabilities = FreezeStrings(unknownCapabilities);
            PressureResultSignatures = FreezeStrings(pressureResultSignatures);
            Notes = notes ?? string.Empty;
            ResultCanonicalSignature = ItemEnemyMatchupCanonical.Hash(CanonicalPayload());
        }

        public string ScenarioId { get; }
        public string ItemSourceSignature { get; }
        public string MappingVersion { get; }
        public string EnemySnapshotSignature { get; }
        public string EncounterId { get; }
        public string ChapterLabelDevOnly { get; }
        public ItemEnemyMatchupEvaluationStatus EvaluationStatus { get; }
        public string ReadinessBand { get; }
        public int KnownRequirementCount { get; }
        public int UnknownRequirementCount { get; }
        public int MetRequirementCount { get; }
        public int GapCount { get; }
        public IReadOnlyList<string> SupportedCapabilities { get; }
        public IReadOnlyList<string> UnknownCapabilities { get; }
        internal IReadOnlyList<string> PressureResultSignatures { get; }
        public string ResultCanonicalSignature { get; }
        public string Notes { get; }

        internal string CanonicalPayload()
        {
            return string.Join("|", new[]
            {
                ScenarioId,
                ItemSourceSignature,
                MappingVersion,
                EnemySnapshotSignature,
                EncounterId,
                ChapterLabelDevOnly,
                EvaluationStatus.ToString(),
                ReadinessBand,
                KnownRequirementCount.ToString(CultureInfo.InvariantCulture),
                UnknownRequirementCount.ToString(CultureInfo.InvariantCulture),
                MetRequirementCount.ToString(CultureInfo.InvariantCulture),
                GapCount.ToString(CultureInfo.InvariantCulture),
                string.Join(";", SupportedCapabilities),
                string.Join(";", UnknownCapabilities),
                string.Join(";", PressureResultSignatures),
                Notes
            });
        }

        private static ReadOnlyCollection<string> FreezeStrings(IEnumerable<string> values)
        {
            return Array.AsReadOnly((values ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray());
        }
    }

    public sealed class ItemEnemyMatchupUnknownBlockerSnapshot
    {
        internal ItemEnemyMatchupUnknownBlockerSnapshot(
            string scenarioId,
            string encounterId,
            string capabilityKey,
            ItemEnemyMatchupUnknownBlockerKind blockerKind,
            string sourceCode,
            string detail)
        {
            ScenarioId = scenarioId ?? string.Empty;
            EncounterId = encounterId ?? string.Empty;
            CapabilityKey = capabilityKey ?? string.Empty;
            BlockerKind = blockerKind;
            SourceCode = sourceCode ?? string.Empty;
            Detail = detail ?? string.Empty;
        }

        public string ScenarioId { get; }
        public string EncounterId { get; }
        public string CapabilityKey { get; }
        public ItemEnemyMatchupUnknownBlockerKind BlockerKind { get; }
        public string SourceCode { get; }
        public string Detail { get; }

        internal string Identity => string.Join("\u001f", new[]
        {
            ScenarioId, EncounterId, CapabilityKey, BlockerKind.ToString(), SourceCode, Detail
        });
    }

    public sealed class ItemEnemyMatchupCapabilityCoverageSnapshot
    {
        internal ItemEnemyMatchupCapabilityCoverageSnapshot(
            string capabilityKey,
            ItemBuildCapabilityMappingStatus mappingStatus,
            int itemScenariosWithKnownValue,
            int encounterRequirementReferences,
            int evaluableMatchupCount,
            int blockedMatchupCount,
            string nextAction)
        {
            CapabilityKey = capabilityKey ?? string.Empty;
            MappingStatus = mappingStatus;
            ItemScenariosWithKnownValue = itemScenariosWithKnownValue;
            EncounterRequirementReferences = encounterRequirementReferences;
            EvaluableMatchupCount = evaluableMatchupCount;
            BlockedMatchupCount = blockedMatchupCount;
            NextAction = nextAction ?? string.Empty;
        }

        public string CapabilityKey { get; }
        public ItemBuildCapabilityMappingStatus MappingStatus { get; }
        public int ItemScenariosWithKnownValue { get; }
        public int EncounterRequirementReferences { get; }
        public int EvaluableMatchupCount { get; }
        public int BlockedMatchupCount { get; }
        public string NextAction { get; }

        internal string CanonicalPayload()
        {
            return string.Join("|", CapabilityKey, MappingStatus.ToString(),
                ItemScenariosWithKnownValue.ToString(CultureInfo.InvariantCulture),
                EncounterRequirementReferences.ToString(CultureInfo.InvariantCulture),
                EvaluableMatchupCount.ToString(CultureInfo.InvariantCulture),
                BlockedMatchupCount.ToString(CultureInfo.InvariantCulture), NextAction);
        }
    }

    public sealed class ItemEnemyMatchupSimulationResult
    {
        internal ItemEnemyMatchupSimulationResult(
            IEnumerable<ItemEnemyMatchupMatrixRowSnapshot> matrixRows,
            IEnumerable<ItemEnemyMatchupCapabilityCoverageSnapshot> coverageRows,
            IEnumerable<ItemEnemyMatchupUnknownBlockerSnapshot> unknownBlockers,
            IEnumerable<ItemBuildCapabilityProjectionResult> projections,
            string enemySnapshotSignature)
        {
            MatrixRows = Array.AsReadOnly((matrixRows ?? Array.Empty<ItemEnemyMatchupMatrixRowSnapshot>())
                .OrderBy(value => value.ScenarioId, StringComparer.Ordinal)
                .ThenBy(value => value.EncounterId, StringComparer.Ordinal)
                .ToArray());
            CoverageRows = Array.AsReadOnly((coverageRows ?? Array.Empty<ItemEnemyMatchupCapabilityCoverageSnapshot>())
                .OrderBy(value => value.CapabilityKey, StringComparer.Ordinal)
                .ToArray());
            UnknownBlockers = Array.AsReadOnly((unknownBlockers ?? Array.Empty<ItemEnemyMatchupUnknownBlockerSnapshot>())
                .GroupBy(value => value.Identity, StringComparer.Ordinal)
                .Select(group => group.First())
                .OrderBy(value => value.ScenarioId, StringComparer.Ordinal)
                .ThenBy(value => value.EncounterId, StringComparer.Ordinal)
                .ThenBy(value => value.CapabilityKey, StringComparer.Ordinal)
                .ThenBy(value => value.BlockerKind)
                .ThenBy(value => value.SourceCode, StringComparer.Ordinal)
                .ToArray());
            Projections = Array.AsReadOnly((projections ?? Array.Empty<ItemBuildCapabilityProjectionResult>())
                .OrderBy(value => value.SourceRevisionId, StringComparer.Ordinal)
                .ToArray());
            EnemySnapshotSignature = enemySnapshotSignature ?? string.Empty;
            CanonicalSignature = ItemEnemyMatchupCanonical.Hash(string.Join("\n",
                MatrixRows.Select(value => value.ResultCanonicalSignature)
                    .Concat(CoverageRows.Select(value => value.CanonicalPayload()))
                    .Concat(UnknownBlockers.Select(value => value.Identity))
                    .Concat(new[] { EnemySnapshotSignature })));
        }

        public IReadOnlyList<ItemEnemyMatchupMatrixRowSnapshot> MatrixRows { get; }
        public IReadOnlyList<ItemEnemyMatchupCapabilityCoverageSnapshot> CoverageRows { get; }
        public IReadOnlyList<ItemEnemyMatchupUnknownBlockerSnapshot> UnknownBlockers { get; }
        public IReadOnlyList<ItemBuildCapabilityProjectionResult> Projections { get; }
        public string EnemySnapshotSignature { get; }
        public string CanonicalSignature { get; }
        public bool DevOnly => true;
        public bool IsEnabled => false;
        public bool EntersFormalFlow => false;
    }

    public sealed class DefaultItemEnemyMatchupSimulation
    {
        public const string SchemaId = "ItemEnemyMatchupSimulation.v1";
        public const int SchemaVersion = 1;

        public static readonly DefaultItemEnemyMatchupSimulation Instance =
            new DefaultItemEnemyMatchupSimulation();

        private readonly DefaultItemBuildCapabilityProjectionAdapter adapter;
        private readonly IEnemyOfflineReadinessEvaluator readinessEvaluator;

        public DefaultItemEnemyMatchupSimulation(
            DefaultItemBuildCapabilityProjectionAdapter adapter = null,
            IEnemyOfflineReadinessEvaluator readinessEvaluator = null)
        {
            this.adapter = adapter ?? new DefaultItemBuildCapabilityProjectionAdapter();
            this.readinessEvaluator = readinessEvaluator ??
                DefaultEnemyOfflineReadinessEvaluator.Instance;
        }

        public ItemEnemyMatchupSimulationResult Run(
            IEnumerable<ItemEnemyMatchupScenarioInput> scenarioInputs,
            DevEncounterSeedDataSnapshot encounterSeedData,
            IEnemyReadinessReferenceResolver resolver)
        {
            ItemEnemyMatchupScenarioInput[] scenarios =
                (scenarioInputs ?? Array.Empty<ItemEnemyMatchupScenarioInput>())
                .OrderBy(value => value == null ? string.Empty : value.ScenarioId,
                    StringComparer.Ordinal)
                .ToArray();
            ValidateInputs(scenarios, encounterSeedData, resolver);

            List<ItemEnemyMatchupMatrixRowSnapshot> matrix =
                new List<ItemEnemyMatchupMatrixRowSnapshot>();
            List<ItemEnemyMatchupUnknownBlockerSnapshot> blockers =
                new List<ItemEnemyMatchupUnknownBlockerSnapshot>();
            List<ItemBuildCapabilityProjectionResult> projections =
                new List<ItemBuildCapabilityProjectionResult>();
            Dictionary<string, ItemBuildCapabilityProjectionResult> projectionByScenario =
                new Dictionary<string, ItemBuildCapabilityProjectionResult>(StringComparer.Ordinal);
            CounterWindowAndPressureCatalogSnapshot pressureCatalog =
                encounterSeedData.EnemySystemSnapshot.CounterWindowAndPressureCatalogSnapshot;

            foreach (ItemEnemyMatchupScenarioInput scenario in scenarios)
            {
                ItemBuildCapabilityProjectionResult projection = adapter.Project(
                    new ItemBuildCapabilityProjectionInput(
                        scenario.ItemInstances,
                        scenario.AffixSchema));
                projections.Add(projection);
                projectionByScenario.Add(scenario.ScenarioId, projection);
                AddProjectionBlockers(blockers, scenario.ScenarioId, projection);

                foreach (DevEncounterSeedProfileSnapshot seed in encounterSeedData.SeedProfiles
                    .OrderBy(value => value.InternalOnly.EncounterId, StringComparer.Ordinal))
                {
                    matrix.Add(EvaluateMatchup(
                        scenario,
                        projection,
                        seed,
                        pressureCatalog,
                        encounterSeedData.EnemySystemSnapshot.CanonicalSignature,
                        resolver,
                        blockers));
                }
            }

            IReadOnlyList<ItemEnemyMatchupCapabilityCoverageSnapshot> coverage =
                BuildCoverage(scenarios, encounterSeedData, matrix, projectionByScenario);
            return new ItemEnemyMatchupSimulationResult(
                matrix,
                coverage,
                blockers,
                projections,
                encounterSeedData.EnemySystemSnapshot.CanonicalSignature);
        }

        private ItemEnemyMatchupMatrixRowSnapshot EvaluateMatchup(
            ItemEnemyMatchupScenarioInput scenario,
            ItemBuildCapabilityProjectionResult projection,
            DevEncounterSeedProfileSnapshot seed,
            CounterWindowAndPressureCatalogSnapshot pressureCatalog,
            string enemySnapshotSignature,
            IEnemyReadinessReferenceResolver resolver,
            ICollection<ItemEnemyMatchupUnknownBlockerSnapshot> blockers)
        {
            List<RequirementCapabilityEvaluationSnapshot> requirements =
                new List<RequirementCapabilityEvaluationSnapshot>();
            List<string> pressureBands = new List<string>();
            List<string> pressureSignatures = new List<string>();
            try
            {
                foreach (string pressureId in seed.InternalOnly.BuildPressureProfileIds
                    .OrderBy(value => value, StringComparer.Ordinal))
                {
                    EnemyReadinessEvaluationResult evaluated = readinessEvaluator.Evaluate(
                        new EnemyReadinessEvaluationInput(
                            projection.Snapshot,
                            pressureCatalog,
                            pressureId,
                            Array.Empty<MapRuleCapabilityAdjustmentSnapshot>()),
                        resolver);
                    DeveloperReadinessSnapshot developer = evaluated.DeveloperReadiness;
                    requirements.AddRange(developer.RequirementEvaluations);
                    pressureBands.Add(pressureId + "=" + developer.ReadinessBand);
                    pressureSignatures.Add(pressureId + "=" + developer.DeveloperCanonicalSignature);
                }
            }
            catch (Exception exception)
            {
                return new ItemEnemyMatchupMatrixRowSnapshot(
                    scenario.ScenarioId,
                    projection.SourceRevisionId,
                    projection.MappingVersion,
                    enemySnapshotSignature,
                    seed.InternalOnly.EncounterId,
                    seed.InternalOnly.DevChapterLabel,
                    ItemEnemyMatchupEvaluationStatus.INVALID_INPUT,
                    string.Empty,
                    0,
                    0,
                    0,
                    0,
                    Array.Empty<string>(),
                    Array.Empty<string>(),
                    Array.Empty<string>(),
                    "E08_INPUT_REJECTED:" + exception.GetType().Name);
            }

            string[] encounterRequirementKeys = requirements
                .Select(value => value.BuildCapabilityKey)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            string[] supported = projection.FieldMap
                .Where(value => value.MappingStatus == ItemBuildCapabilityMappingStatus.SUPPORTED
                    && encounterRequirementKeys.Contains(value.TargetCapabilityKey,
                        StringComparer.Ordinal))
                .Select(value => value.TargetCapabilityKey)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            string[] unknown = requirements
                .Where(value => value.CapabilityValueAvailability == CapabilityValueAvailability.Unknown)
                .Select(value => value.BuildCapabilityKey)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            foreach (string capabilityKey in unknown)
            {
                blockers.Add(new ItemEnemyMatchupUnknownBlockerSnapshot(
                    scenario.ScenarioId,
                    seed.InternalOnly.EncounterId,
                    capabilityKey,
                    ItemEnemyMatchupUnknownBlockerKind.ENCOUNTER_REQUIREMENT_UNKNOWN,
                    "E08_CAPABILITY_VALUE_UNKNOWN",
                    "E08 omitted a readiness conclusion for this capability requirement."));
            }

            bool decisiveUnknown = requirements.Any(value =>
                value.RequirementRole == CapabilityRequirementRole.Required
                && value.CapabilityValueAvailability == CapabilityValueAvailability.Unknown);
            bool anyUnknown = unknown.Length > 0;
            ItemEnemyMatchupEvaluationStatus status;
            if (decisiveUnknown)
            {
                status = ItemEnemyMatchupEvaluationStatus.BLOCKED_BY_UNKNOWN;
            }
            else if (supported.Length == 0)
            {
                status = ItemEnemyMatchupEvaluationStatus.NO_RELEVANT_SUPPORTED_CAPABILITY;
            }
            else if (anyUnknown)
            {
                status = ItemEnemyMatchupEvaluationStatus.EVALUABLE_PARTIAL;
            }
            else
            {
                status = ItemEnemyMatchupEvaluationStatus.EVALUABLE_COMPLETE;
            }

            bool exposeE08Bands = status == ItemEnemyMatchupEvaluationStatus.EVALUABLE_COMPLETE
                || status == ItemEnemyMatchupEvaluationStatus.EVALUABLE_PARTIAL;
            int knownCount = requirements.Count(value =>
                value.CapabilityValueAvailability == CapabilityValueAvailability.Known);
            int unknownCount = requirements.Count - knownCount;
            int metCount = requirements.Count(value =>
                value.RequirementStatus == RequirementEvaluationStatus.Met);
            int gapCount = requirements.Count(value =>
                value.CapabilityValueAvailability == CapabilityValueAvailability.Known
                && value.GapBasisPoints > 0);
            string note = exposeE08Bands
                ? "E08_PER_PRESSURE_ONLY;NO_MAP_ADJUSTMENTS_INFERRED"
                : status == ItemEnemyMatchupEvaluationStatus.BLOCKED_BY_UNKNOWN
                    ? "READINESS_SUPPRESSED_BY_DECISIVE_UNKNOWN"
                    : "READINESS_SUPPRESSED_NO_RELEVANT_SUPPORTED_CAPABILITY";
            return new ItemEnemyMatchupMatrixRowSnapshot(
                scenario.ScenarioId,
                projection.SourceRevisionId,
                projection.MappingVersion,
                enemySnapshotSignature,
                seed.InternalOnly.EncounterId,
                seed.InternalOnly.DevChapterLabel,
                status,
                exposeE08Bands ? string.Join(";", pressureBands) : string.Empty,
                knownCount,
                unknownCount,
                metCount,
                gapCount,
                supported,
                unknown,
                pressureSignatures,
                note);
        }

        private static IReadOnlyList<ItemEnemyMatchupCapabilityCoverageSnapshot> BuildCoverage(
            IReadOnlyList<ItemEnemyMatchupScenarioInput> scenarios,
            DevEncounterSeedDataSnapshot encounterSeedData,
            IReadOnlyList<ItemEnemyMatchupMatrixRowSnapshot> matrix,
            IReadOnlyDictionary<string, ItemBuildCapabilityProjectionResult> projectionByScenario)
        {
            string[] keys = projectionByScenario.Values
                .SelectMany(value => value.FieldMap)
                .Select(value => value.TargetCapabilityKey)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            CounterWindowAndPressureCatalogSnapshot catalog =
                encounterSeedData.EnemySystemSnapshot.CounterWindowAndPressureCatalogSnapshot;
            List<ItemEnemyMatchupCapabilityCoverageSnapshot> result =
                new List<ItemEnemyMatchupCapabilityCoverageSnapshot>();
            foreach (string key in keys)
            {
                ItemBuildCapabilityMappingStatus status = AggregateMappingStatus(
                    projectionByScenario.Values.SelectMany(value => value.FieldMap)
                        .Where(value => value.TargetCapabilityKey == key)
                        .Select(value => value.MappingStatus));
                int knownScenarios = projectionByScenario.Values.Count(value =>
                    value.Snapshot.TryGetCapabilityValue(key, out _));
                HashSet<string> encountersReferencing = new HashSet<string>(StringComparer.Ordinal);
                int references = 0;
                foreach (DevEncounterSeedProfileSnapshot seed in encounterSeedData.SeedProfiles)
                {
                    foreach (string pressureId in seed.InternalOnly.BuildPressureProfileIds)
                    {
                        if (!catalog.TryGetBuildPressureProfileById(pressureId,
                            out BuildPressureProfileSnapshot pressure))
                        {
                            continue;
                        }

                        int count = pressure.DeveloperOnly.RequirementGroups
                            .SelectMany(value => value.Requirements)
                            .Count(value => value.BuildCapabilityKey == key);
                        if (count > 0)
                        {
                            references += count;
                            encountersReferencing.Add(seed.InternalOnly.EncounterId);
                        }
                    }
                }

                int evaluable = matrix.Count(value =>
                    encountersReferencing.Contains(value.EncounterId)
                    && (value.EvaluationStatus == ItemEnemyMatchupEvaluationStatus.EVALUABLE_COMPLETE
                        || value.EvaluationStatus == ItemEnemyMatchupEvaluationStatus.EVALUABLE_PARTIAL));
                int blocked = matrix.Count(value =>
                    encountersReferencing.Contains(value.EncounterId)
                    && value.EvaluationStatus == ItemEnemyMatchupEvaluationStatus.BLOCKED_BY_UNKNOWN);
                result.Add(new ItemEnemyMatchupCapabilityCoverageSnapshot(
                    key,
                    status,
                    knownScenarios,
                    references,
                    evaluable,
                    blocked,
                    NextAction(status)));
            }

            return Array.AsReadOnly(result.ToArray());
        }

        private static ItemBuildCapabilityMappingStatus AggregateMappingStatus(
            IEnumerable<ItemBuildCapabilityMappingStatus> statuses)
        {
            ItemBuildCapabilityMappingStatus[] values = statuses.Distinct().ToArray();
            if (values.Contains(ItemBuildCapabilityMappingStatus.SUPPORTED))
            {
                return ItemBuildCapabilityMappingStatus.SUPPORTED;
            }

            if (values.Contains(ItemBuildCapabilityMappingStatus.KNOWN_ZERO))
            {
                return ItemBuildCapabilityMappingStatus.KNOWN_ZERO;
            }

            if (values.Contains(ItemBuildCapabilityMappingStatus.OUT_OF_SCOPE))
            {
                return ItemBuildCapabilityMappingStatus.OUT_OF_SCOPE;
            }

            return ItemBuildCapabilityMappingStatus.UNKNOWN_NOT_MAPPED;
        }

        private static string NextAction(ItemBuildCapabilityMappingStatus status)
        {
            switch (status)
            {
                case ItemBuildCapabilityMappingStatus.SUPPORTED:
                case ItemBuildCapabilityMappingStatus.KNOWN_ZERO:
                    return "NONE_C01_READ_ONLY";
                case ItemBuildCapabilityMappingStatus.OUT_OF_SCOPE:
                    return "RUNTIME_TIMING_FACT_REQUIRED_OUT_OF_SCOPE";
                default:
                    return "GUARD_CONFIRMED_MAPPING_FACT_REQUIRED";
            }
        }

        private static void AddProjectionBlockers(
            ICollection<ItemEnemyMatchupUnknownBlockerSnapshot> blockers,
            string scenarioId,
            ItemBuildCapabilityProjectionResult projection)
        {
            foreach (ItemBuildCapabilityProjectionFieldMapSnapshot field in projection.FieldMap)
            {
                if (field.MappingStatus == ItemBuildCapabilityMappingStatus.UNKNOWN_NOT_MAPPED)
                {
                    blockers.Add(new ItemEnemyMatchupUnknownBlockerSnapshot(
                        scenarioId,
                        string.Empty,
                        field.TargetCapabilityKey,
                        ItemEnemyMatchupUnknownBlockerKind.ITEM_CAPABILITY_MAPPING_MISSING,
                        "C01_UNKNOWN_NOT_MAPPED",
                        field.Notes));
                }
                else if (field.MappingStatus == ItemBuildCapabilityMappingStatus.OUT_OF_SCOPE)
                {
                    blockers.Add(new ItemEnemyMatchupUnknownBlockerSnapshot(
                        scenarioId,
                        string.Empty,
                        field.TargetCapabilityKey,
                        ItemEnemyMatchupUnknownBlockerKind.OUT_OF_SCOPE_RUNTIME_TIMING,
                        "C01_OUT_OF_SCOPE",
                        field.Notes));
                }
            }

            foreach (ItemBuildCapabilityUnknownDiagnosticSnapshot diagnostic in
                projection.UnknownDiagnostics)
            {
                ItemEnemyMatchupUnknownBlockerKind kind =
                    diagnostic.Code.IndexOf("RUNTIME_TIMING", StringComparison.Ordinal) >= 0
                    || diagnostic.Code.IndexOf("CASTER_STATE", StringComparison.Ordinal) >= 0
                        ? ItemEnemyMatchupUnknownBlockerKind.OUT_OF_SCOPE_RUNTIME_TIMING
                        : diagnostic.Code.IndexOf("FACTS_MISSING", StringComparison.Ordinal) >= 0
                            || diagnostic.Code.IndexOf("PLACEMENT_BINDING_MISSING",
                                StringComparison.Ordinal) >= 0
                            ? ItemEnemyMatchupUnknownBlockerKind.ITEM_RUNTIME_FACT_MISSING
                            : ItemEnemyMatchupUnknownBlockerKind.ITEM_CAPABILITY_MAPPING_MISSING;
                blockers.Add(new ItemEnemyMatchupUnknownBlockerSnapshot(
                    scenarioId,
                    string.Empty,
                    diagnostic.TargetCapabilityKey,
                    kind,
                    diagnostic.Code,
                    diagnostic.Detail));
            }
        }

        private static void ValidateInputs(
            IReadOnlyList<ItemEnemyMatchupScenarioInput> scenarios,
            DevEncounterSeedDataSnapshot encounterSeedData,
            IEnemyReadinessReferenceResolver resolver)
        {
            if (scenarios.Count == 0 || scenarios.Any(value => value == null
                || string.IsNullOrWhiteSpace(value.ScenarioId)
                || value.AffixSchema == null))
            {
                throw new ArgumentException("Complete devOnly Item scenarios are required.");
            }

            if (scenarios.GroupBy(value => value.ScenarioId, StringComparer.Ordinal)
                .Any(group => group.Count() != 1))
            {
                throw new ArgumentException("Scenario IDs must be ordinal-unique.");
            }

            if (encounterSeedData == null || encounterSeedData.EnemySystemSnapshot == null
                || encounterSeedData.SeedProfiles.Count == 0)
            {
                throw new ArgumentException("An E10 dev encounter snapshot is required.");
            }

            if (!encounterSeedData.DevOnly || encounterSeedData.IsEnabled
                || encounterSeedData.EntersFormalFlow)
            {
                throw new ArgumentException(
                    "E10 isolation must remain devOnly=true/isEnabled=false/entersFormalFlow=false.");
            }

            if (resolver == null)
            {
                throw new ArgumentNullException(nameof(resolver));
            }
        }
    }

    internal static class ItemEnemyMatchupCanonical
    {
        public static string Hash(string payload)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(payload ?? string.Empty));
                return "sha256:" + string.Concat(bytes.Select(value =>
                    value.ToString("x2", CultureInfo.InvariantCulture)));
            }
        }
    }
}
