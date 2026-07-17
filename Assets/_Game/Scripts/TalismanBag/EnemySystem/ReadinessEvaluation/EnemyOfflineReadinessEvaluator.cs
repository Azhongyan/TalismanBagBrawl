using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.EnemySystem.CapabilityRead;
using TalismanBag.EnemySystem.PressureWindow;

namespace TalismanBag.EnemySystem.ReadinessEvaluation
{
    public sealed class DefaultEnemyOfflineReadinessEvaluator :
        IEnemyOfflineReadinessEvaluator
    {
        public static readonly DefaultEnemyOfflineReadinessEvaluator Instance =
            new DefaultEnemyOfflineReadinessEvaluator();

        private static readonly string[] PlayerSafeForbiddenTokens =
        {
            "readinessBand",
            "buildCapabilityKey",
            "baseValueBasisPoints",
            "effectiveValueBasisPoints",
            "requiredValueBasisPoints",
            "gapBasisPoints",
            "requirementGroupId",
            "requirementRole",
            "requirementMatchMode",
            "requirementStatus",
            "groupStatus",
            "capabilityValueAvailability",
            "mapRuleId",
            "deltaBasisPoints",
            "developerReasonId",
            "buildSnapshotId",
            "buildSnapshotCanonicalSignature",
            "pressureCatalogCanonicalSignature",
            "sourceSummary",
            "hardSolutionTags",
            "requiredSynergy",
            "requiredAffix",
            "requiredStats",
            "dropBias",
            "bossKey"
        };

        public EnemyReadinessEvaluationResult Evaluate(
            EnemyReadinessEvaluationInput input,
            IEnemyReadinessReferenceResolver resolver)
        {
            IReadOnlyList<EnemyReadinessValidationIssue> issues = Validate(
                input,
                resolver);
            if (issues.Count > 0)
            {
                throw new EnemyReadinessEvaluationException(issues);
            }

            input.PressureCatalog.TryGetBuildPressureProfileById(
                input.BuildPressureProfileId,
                out BuildPressureProfileSnapshot pressure);

            Dictionary<string, long> deltaByCapability = SumAdjustments(
                input.MapRuleCapabilityAdjustments);
            List<RequirementGroupEvaluationSnapshot> groupEvaluations =
                new List<RequirementGroupEvaluationSnapshot>();
            List<RequirementCapabilityEvaluationSnapshot> requirementEvaluations =
                new List<RequirementCapabilityEvaluationSnapshot>();

            foreach (BuildCapabilityRequirementGroupSnapshot group in
                pressure.DeveloperOnly.RequirementGroups
                    .OrderBy(
                        value => value.RequirementGroupId,
                        StringComparer.Ordinal))
            {
                List<RequirementDraft> drafts = group.Requirements
                    .OrderBy(
                        value => value.BuildCapabilityKey,
                        StringComparer.Ordinal)
                    .ThenBy(value => value.MinimumCapabilityBasisPoints)
                    .Select(value => EvaluateRequirement(
                        input.BuildCapabilitySnapshot,
                        value,
                        deltaByCapability))
                    .ToList();
                RequirementGroupStatus groupStatus = EvaluateGroupStatus(
                    group.RequirementMatchMode,
                    drafts.Select(value => value.Status));
                RequirementCapabilityEvaluationSnapshot[] rows = drafts
                    .Select(value => value.ToSnapshot(group, groupStatus))
                    .ToArray();
                RequirementGroupEvaluationSnapshot groupEvaluation =
                    new RequirementGroupEvaluationSnapshot(
                        group.RequirementGroupId,
                        group.RequirementRole,
                        group.RequirementMatchMode,
                        groupStatus,
                        rows);
                groupEvaluations.Add(groupEvaluation);
                requirementEvaluations.AddRange(rows);
            }

            ReadinessBand readinessBand = EvaluateReadinessBand(groupEvaluations);
            DeveloperReadinessSnapshot developer = new DeveloperReadinessSnapshot(
                input.BuildCapabilitySnapshot.SnapshotId,
                input.BuildCapabilitySnapshot.CanonicalSignature,
                input.PressureCatalog.CanonicalSignature,
                pressure.BuildPressureProfileId,
                readinessBand,
                groupEvaluations,
                requirementEvaluations,
                input.MapRuleCapabilityAdjustments);
            EnemyReadinessPlayerHintProjection player =
                new EnemyReadinessPlayerHintProjection(
                    pressure.PlayerSafe.BuildPressureProfileId,
                    pressure.PlayerSafe.PublicPressureLabelKey,
                    pressure.PlayerSafe.PublicPressureHintKey,
                    pressure.PlayerSafe.PlayerHintCategoryKeys,
                    ToPlayerHintSeverity(readinessBand));

            List<EnemyReadinessValidationIssue> playerIssues =
                ValidatePlayerSafePayload(player.BuildPlayerSafeCanonicalPayload());
            if (playerIssues.Count > 0)
            {
                throw new EnemyReadinessEvaluationException(playerIssues);
            }

            return new EnemyReadinessEvaluationResult(developer, player);
        }

        private static IReadOnlyList<EnemyReadinessValidationIssue> Validate(
            EnemyReadinessEvaluationInput input,
            IEnemyReadinessReferenceResolver resolver)
        {
            List<EnemyReadinessValidationIssue> issues =
                new List<EnemyReadinessValidationIssue>();
            if (input == null)
            {
                Add(issues, "INPUT_NULL", "$", "Evaluation input is required.");
                return Array.AsReadOnly(issues.ToArray());
            }

            if (input.BuildCapabilitySnapshot == null)
            {
                Add(
                    issues,
                    "BUILD_SNAPSHOT_NULL",
                    "buildCapabilitySnapshot",
                    "An E07 BuildCapabilitySnapshot is required.");
            }

            if (input.PressureCatalog == null)
            {
                Add(
                    issues,
                    "PRESSURE_CATALOG_NULL",
                    "pressureCatalog",
                    "An E06 pressure catalog snapshot is required.");
            }

            if (resolver == null)
            {
                Add(
                    issues,
                    "RESOLVER_NULL",
                    "resolver",
                    "An E02/E03 reference resolver is required.");
            }

            ValidateId(
                input.BuildPressureProfileId,
                "buildPressureProfileId",
                issues);
            ValidateBuildSnapshot(input.BuildCapabilitySnapshot, resolver, issues);
            BuildPressureProfileSnapshot pressure = ValidatePressureCatalog(
                input.PressureCatalog,
                input.BuildPressureProfileId,
                resolver,
                issues);
            ValidateAdjustments(
                input.MapRuleCapabilityAdjustments,
                resolver,
                issues);

            if (pressure != null && pressure.PlayerSafe == null)
            {
                Add(
                    issues,
                    "PRESSURE_PLAYER_SAFE_NULL",
                    "selectedPressure.playerSafe",
                    "Selected pressure must contain its E06 player-safe projection.");
            }

            if (pressure != null && pressure.DeveloperOnly == null)
            {
                Add(
                    issues,
                    "PRESSURE_DEVELOPER_ONLY_NULL",
                    "selectedPressure.developerOnly",
                    "Selected pressure must contain E06 developer diagnostics.");
            }

            return Array.AsReadOnly(issues.ToArray());
        }

        private static void ValidateBuildSnapshot(
            BuildCapabilitySnapshot snapshot,
            IEnemyReadinessReferenceResolver resolver,
            ICollection<EnemyReadinessValidationIssue> issues)
        {
            if (snapshot == null)
            {
                return;
            }

            if (!string.Equals(
                snapshot.SchemaId,
                BuildCapabilityReadSchema.SchemaId,
                StringComparison.Ordinal))
            {
                Add(
                    issues,
                    "BUILD_SCHEMA_ID_MISMATCH",
                    "buildCapabilitySnapshot.schemaId",
                    "Build snapshot schema ID must match E07 exactly.");
            }

            if (snapshot.SchemaVersion != BuildCapabilityReadSchema.SchemaVersion)
            {
                Add(
                    issues,
                    "BUILD_SCHEMA_VERSION_MISMATCH",
                    "buildCapabilitySnapshot.schemaVersion",
                    "Build snapshot schema version must match E07 exactly.");
            }

            if (!snapshot.DevOnly || snapshot.IsEnabled || snapshot.EntersFormalFlow)
            {
                Add(
                    issues,
                    "BUILD_ISOLATION_INVALID",
                    "buildCapabilitySnapshot",
                    "Required isolation is devOnly=true, isEnabled=false, entersFormalFlow=false.");
            }

            for (int index = 0; index < snapshot.CapabilityValues.Count; index++)
            {
                BuildCapabilityValueSnapshot value = snapshot.CapabilityValues[index];
                if (value == null)
                {
                    Add(
                        issues,
                        "BUILD_CAPABILITY_VALUE_NULL",
                        "buildCapabilitySnapshot.capabilityValues[" + index + "]",
                        "Capability values cannot contain null entries.");
                    continue;
                }

                if (resolver != null
                    && !resolver.HasBuildCapabilityKey(value.BuildCapabilityKey))
                {
                    Add(
                        issues,
                        "BUILD_CAPABILITY_KEY_UNRESOLVED",
                        "buildCapabilitySnapshot.capabilityValues[" + index
                            + "].buildCapabilityKey",
                        "Build capability key does not resolve through E02.");
                }
            }
        }

        private static BuildPressureProfileSnapshot ValidatePressureCatalog(
            CounterWindowAndPressureCatalogSnapshot catalog,
            string pressureId,
            IEnemyReadinessReferenceResolver resolver,
            ICollection<EnemyReadinessValidationIssue> issues)
        {
            if (catalog == null)
            {
                return null;
            }

            if (!string.Equals(
                catalog.SchemaId,
                CounterWindowAndPressureSchema.SchemaId,
                StringComparison.Ordinal))
            {
                Add(
                    issues,
                    "PRESSURE_SCHEMA_ID_MISMATCH",
                    "pressureCatalog.schemaId",
                    "Pressure catalog schema ID must match E06 exactly.");
            }

            if (catalog.SchemaVersion != CounterWindowAndPressureSchema.SchemaVersion)
            {
                Add(
                    issues,
                    "PRESSURE_SCHEMA_VERSION_MISMATCH",
                    "pressureCatalog.schemaVersion",
                    "Pressure catalog schema version must match E06 exactly.");
            }

            if (!catalog.TryGetBuildPressureProfileById(
                pressureId,
                out BuildPressureProfileSnapshot pressure))
            {
                Add(
                    issues,
                    "PRESSURE_PROFILE_UNRESOLVED",
                    "buildPressureProfileId",
                    "BuildPressureProfileId must resolve by ordinal E06 lookup.");
                return null;
            }

            if (!pressure.DevOnly || pressure.IsEnabled || pressure.EntersFormalFlow)
            {
                Add(
                    issues,
                    "PRESSURE_ISOLATION_INVALID",
                    "selectedPressure",
                    "Required isolation is devOnly=true, isEnabled=false, entersFormalFlow=false.");
            }

            if (pressure.DeveloperOnly != null)
            {
                foreach (BuildCapabilityRequirementGroupSnapshot group in
                    pressure.DeveloperOnly.RequirementGroups)
                {
                    foreach (BuildCapabilityRequirementSnapshot requirement in
                        group.Requirements)
                    {
                        if (resolver != null
                            && !resolver.HasBuildCapabilityKey(
                                requirement.BuildCapabilityKey))
                        {
                            Add(
                                issues,
                                "PRESSURE_CAPABILITY_KEY_UNRESOLVED",
                                "selectedPressure.requirementGroups."
                                    + group.RequirementGroupId,
                                "Pressure requirement capability does not resolve through E02.");
                        }
                    }
                }
            }

            return pressure;
        }

        private static void ValidateAdjustments(
            IReadOnlyList<MapRuleCapabilityAdjustmentSnapshot> adjustments,
            IEnemyReadinessReferenceResolver resolver,
            ICollection<EnemyReadinessValidationIssue> issues)
        {
            HashSet<string> identities = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < adjustments.Count; index++)
            {
                MapRuleCapabilityAdjustmentSnapshot adjustment = adjustments[index];
                string path = "mapRuleCapabilityAdjustments[" + index + "]";
                if (adjustment == null)
                {
                    Add(
                        issues,
                        "MAP_ADJUSTMENT_NULL",
                        path,
                        "Map-rule adjustments cannot contain null entries.");
                    continue;
                }

                ValidateId(adjustment.MapRuleId, path + ".mapRuleId", issues);
                ValidateId(
                    adjustment.BuildCapabilityKey,
                    path + ".buildCapabilityKey",
                    issues);
                ValidateId(
                    adjustment.DeveloperReasonId,
                    path + ".developerReasonId",
                    issues);
                if (resolver != null && !resolver.HasMapRule(adjustment.MapRuleId))
                {
                    Add(
                        issues,
                        "MAP_RULE_UNRESOLVED",
                        path + ".mapRuleId",
                        "MapRuleId does not resolve through E03.");
                }

                if (resolver != null
                    && !resolver.HasBuildCapabilityKey(
                        adjustment.BuildCapabilityKey))
                {
                    Add(
                        issues,
                        "MAP_CAPABILITY_KEY_UNRESOLVED",
                        path + ".buildCapabilityKey",
                        "BuildCapabilityKey does not resolve through E02.");
                }

                if (adjustment.DeltaBasisPoints == 0
                    || adjustment.DeltaBasisPoints < -10000
                    || adjustment.DeltaBasisPoints > 10000)
                {
                    Add(
                        issues,
                        "MAP_DELTA_OUT_OF_RANGE",
                        path + ".deltaBasisPoints",
                        "DeltaBasisPoints must be non-zero and within -10000..10000.");
                }

                if (LooksLikePath(adjustment.DeveloperReasonId))
                {
                    Add(
                        issues,
                        "DEVELOPER_REASON_PATH_FORBIDDEN",
                        path + ".developerReasonId",
                        "DeveloperReasonId cannot contain disk, Asset, or Scene path semantics.");
                }

                string identity = adjustment.MapRuleId
                    + "\u001f"
                    + adjustment.BuildCapabilityKey;
                if (!identities.Add(identity))
                {
                    Add(
                        issues,
                        "MAP_ADJUSTMENT_DUPLICATE",
                        path,
                        "MapRuleId + BuildCapabilityKey must be ordinal-unique.");
                }
            }
        }

        private static List<EnemyReadinessValidationIssue>
            ValidatePlayerSafePayload(string payload)
        {
            List<EnemyReadinessValidationIssue> issues =
                new List<EnemyReadinessValidationIssue>();
            foreach (string token in PlayerSafeForbiddenTokens)
            {
                if ((payload ?? string.Empty).IndexOf(
                    token,
                    StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Add(
                        issues,
                        "PLAYER_SAFE_PAYLOAD_LEAK",
                        "playerHintProjection",
                        "Player-safe payload contains forbidden developer field token: "
                            + token
                            + ".");
                }
            }

            return issues;
        }

        private static RequirementDraft EvaluateRequirement(
            BuildCapabilitySnapshot build,
            BuildCapabilityRequirementSnapshot requirement,
            IReadOnlyDictionary<string, long> deltaByCapability)
        {
            long mapDelta = deltaByCapability.TryGetValue(
                requirement.BuildCapabilityKey,
                out long value)
                ? value
                : 0L;
            if (!build.TryGetCapabilityValue(
                requirement.BuildCapabilityKey,
                out BuildCapabilityValueSnapshot capability))
            {
                return new RequirementDraft(
                    requirement,
                    CapabilityValueAvailability.Unknown,
                    0,
                    mapDelta,
                    0,
                    0,
                    RequirementEvaluationStatus.Unknown);
            }

            int effective = ClampBasisPoints(
                (long)capability.ValueBasisPoints + mapDelta);
            bool met = effective >= requirement.MinimumCapabilityBasisPoints;
            int gap = met
                ? 0
                : requirement.MinimumCapabilityBasisPoints - effective;
            return new RequirementDraft(
                requirement,
                CapabilityValueAvailability.Known,
                capability.ValueBasisPoints,
                mapDelta,
                effective,
                gap,
                met
                    ? RequirementEvaluationStatus.Met
                    : RequirementEvaluationStatus.Unmet);
        }

        private static Dictionary<string, long> SumAdjustments(
            IEnumerable<MapRuleCapabilityAdjustmentSnapshot> adjustments)
        {
            Dictionary<string, long> result =
                new Dictionary<string, long>(StringComparer.Ordinal);
            foreach (MapRuleCapabilityAdjustmentSnapshot adjustment in adjustments)
            {
                result.TryGetValue(
                    adjustment.BuildCapabilityKey,
                    out long current);
                result[adjustment.BuildCapabilityKey] =
                    SaturatingAdd(current, adjustment.DeltaBasisPoints);
            }

            return result;
        }

        private static long SaturatingAdd(long left, int right)
        {
            if (right > 0 && left > long.MaxValue - right)
            {
                return long.MaxValue;
            }

            if (right < 0 && left < long.MinValue - right)
            {
                return long.MinValue;
            }

            return left + right;
        }

        private static int ClampBasisPoints(long value)
        {
            if (value <= BuildCapabilityScale.MinimumValueBasisPoints)
            {
                return BuildCapabilityScale.MinimumValueBasisPoints;
            }

            if (value >= BuildCapabilityScale.MaximumValueBasisPoints)
            {
                return BuildCapabilityScale.MaximumValueBasisPoints;
            }

            return (int)value;
        }

        private static RequirementGroupStatus EvaluateGroupStatus(
            RequirementMatchMode mode,
            IEnumerable<RequirementEvaluationStatus> statuses)
        {
            RequirementEvaluationStatus[] values = statuses.ToArray();
            if (mode == RequirementMatchMode.Any)
            {
                if (values.Any(value => value == RequirementEvaluationStatus.Met))
                {
                    return RequirementGroupStatus.Met;
                }

                return values.Any(
                    value => value == RequirementEvaluationStatus.Unknown)
                    ? RequirementGroupStatus.Unknown
                    : RequirementGroupStatus.Unmet;
            }

            if (values.Any(value => value == RequirementEvaluationStatus.Unmet))
            {
                return RequirementGroupStatus.Unmet;
            }

            return values.Any(value => value == RequirementEvaluationStatus.Unknown)
                ? RequirementGroupStatus.Unknown
                : RequirementGroupStatus.Met;
        }

        private static ReadinessBand EvaluateReadinessBand(
            IReadOnlyList<RequirementGroupEvaluationSnapshot> groups)
        {
            RequirementGroupEvaluationSnapshot[] required = groups
                .Where(value =>
                    value.RequirementRole == CapabilityRequirementRole.Required)
                .ToArray();
            RequirementGroupEvaluationSnapshot[] recommended = groups
                .Where(value =>
                    value.RequirementRole == CapabilityRequirementRole.Recommended)
                .ToArray();

            if (required.Any(value => value.GroupStatus == RequirementGroupStatus.Unmet))
            {
                return ReadinessBand.Blocked;
            }

            if (required.Any(value => value.GroupStatus == RequirementGroupStatus.Unknown))
            {
                return ReadinessBand.Unknown;
            }

            if (recommended.Length == 0)
            {
                return ReadinessBand.Ready;
            }

            return recommended.All(
                value => value.GroupStatus == RequirementGroupStatus.Met)
                ? ReadinessBand.Strong
                : ReadinessBand.Strained;
        }

        private static PlayerHintSeverity ToPlayerHintSeverity(
            ReadinessBand readinessBand)
        {
            switch (readinessBand)
            {
                case ReadinessBand.Unknown:
                    return PlayerHintSeverity.Notice;
                case ReadinessBand.Blocked:
                    return PlayerHintSeverity.Critical;
                case ReadinessBand.Strained:
                    return PlayerHintSeverity.Warning;
                case ReadinessBand.Ready:
                case ReadinessBand.Strong:
                    return PlayerHintSeverity.None;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(readinessBand),
                        readinessBand,
                        "ReadinessBand is not defined by E07.");
            }
        }

        private static bool LooksLikePath(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            return value.IndexOf('/') >= 0
                || value.IndexOf('\\') >= 0
                || value.IndexOf(':') >= 0
                || value.IndexOf("asset", StringComparison.OrdinalIgnoreCase) >= 0
                || value.IndexOf("scene", StringComparison.OrdinalIgnoreCase) >= 0
                || value.IndexOf("disk", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void ValidateId(
            string value,
            string path,
            ICollection<EnemyReadinessValidationIssue> issues)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                Add(issues, "ID_EMPTY", path, "Stable ID is required.");
                return;
            }

            if (!string.Equals(value, value.Trim(), StringComparison.Ordinal))
            {
                Add(
                    issues,
                    "ID_OUTER_WHITESPACE",
                    path,
                    "Stable IDs are never trimmed implicitly.");
            }
        }

        private static void Add(
            ICollection<EnemyReadinessValidationIssue> issues,
            string code,
            string path,
            string message)
        {
            issues.Add(new EnemyReadinessValidationIssue(code, path, message));
        }

        private sealed class RequirementDraft
        {
            public RequirementDraft(
                BuildCapabilityRequirementSnapshot requirement,
                CapabilityValueAvailability availability,
                int baseValue,
                long mapDelta,
                int effectiveValue,
                int gap,
                RequirementEvaluationStatus status)
            {
                Requirement = requirement;
                Availability = availability;
                BaseValue = baseValue;
                MapDelta = mapDelta;
                EffectiveValue = effectiveValue;
                Gap = gap;
                Status = status;
            }

            public BuildCapabilityRequirementSnapshot Requirement { get; }
            public CapabilityValueAvailability Availability { get; }
            public int BaseValue { get; }
            public long MapDelta { get; }
            public int EffectiveValue { get; }
            public int Gap { get; }
            public RequirementEvaluationStatus Status { get; }

            public RequirementCapabilityEvaluationSnapshot ToSnapshot(
                BuildCapabilityRequirementGroupSnapshot group,
                RequirementGroupStatus groupStatus)
            {
                return new RequirementCapabilityEvaluationSnapshot(
                    group.RequirementGroupId,
                    group.RequirementRole,
                    group.RequirementMatchMode,
                    Requirement.BuildCapabilityKey,
                    Availability,
                    BaseValue,
                    MapDelta,
                    EffectiveValue,
                    Requirement.MinimumCapabilityBasisPoints,
                    Gap,
                    Status,
                    groupStatus);
            }
        }
    }
}
