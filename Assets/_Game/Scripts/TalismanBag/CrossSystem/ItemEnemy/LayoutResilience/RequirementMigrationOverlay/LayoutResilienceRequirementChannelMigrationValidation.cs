using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.AuthoredPressure;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.DevAuthoring;
using TalismanBag.EnemySystem.RequirementChannel;

namespace TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.RequirementMigrationOverlay
{
    public static class LayoutResilienceRequirementChannelMigrationValidation
    {
        private const string N01BSnapshotId =
            "overlay.layout_resilience.requirement_channel_migration.v1";

        public static LayoutResilienceRequirementChannelMigrationResult
            ValidateAndCreateOverlay(
                LayoutResilienceRequirementChannelMigrationSource source)
        {
            return ValidateCore(source, input =>
                DefaultEnemyRequirementChannelApplicabilitySnapshotProvider.Instance
                    .CreateSnapshot(input));
        }

        private static LayoutResilienceRequirementChannelMigrationResult ValidateCore(
            LayoutResilienceRequirementChannelMigrationSource source,
            Func<EnemyRequirementChannelApplicabilitySnapshotInput,
                EnemyRequirementChannelApplicabilitySnapshot> createN01BSnapshot)
        {
            List<LayoutResilienceRequirementChannelMigrationIssue> issues =
                new List<LayoutResilienceRequirementChannelMigrationIssue>();
            if (source == null)
            {
                Unknown(issues, "SOURCE_MISSING", "$",
                    "The overlay source is missing.");
                return Finish(source, null, issues);
            }

            ValidateSource(source, issues);
            DevEncounterLayoutPressureAuthoringResult p5A =
                DevEncounterLayoutPressureValidation.ValidateAndProject(
                    DevEncounterLayoutPressureCatalog.CreateCandidateSource());
            ValidateP5A(p5A, issues);

            List<RowBinding> bindings = new List<RowBinding>();
            if (source.Rows != null)
            {
                for (int index = 0; index < source.Rows.Count; index++)
                {
                    string path = "rows[" + index.ToString(
                        CultureInfo.InvariantCulture) + "]";
                    LayoutResilienceRequirementChannelMigrationSourceRow row =
                        source.Rows[index];
                    if (row == null)
                    {
                        Unknown(issues, "ROW_MISSING", path,
                            "An explicit route row is missing.");
                        continue;
                    }
                    RouteSpec spec = ValidateRoute(row, path, issues);
                    DevEncounterLayoutPressureAuthoringRowSnapshot authoring =
                        FindP5ARow(row, p5A, path, issues);
                    if (spec != null && authoring != null)
                        ValidateAuthoringBinding(row, spec, authoring, path, issues);
                    bindings.Add(new RowBinding(row, authoring));
                }
                ValidateUniqueness(source.Rows, issues);
                ValidateCandidateTupleStability(source.Rows, issues);
                ValidateExpectedCoverage(source.Rows, issues);
            }

            if (issues.Count != 0 || bindings.Count != 4 ||
                bindings.Any(value => value.Authoring == null))
                return Finish(source, null, issues);

            EnemyRequirementChannelApplicabilitySnapshot channelSnapshot;
            try
            {
                channelSnapshot = createN01BSnapshot(
                    new EnemyRequirementChannelApplicabilitySnapshotInput(
                        N01BSnapshotId,
                        EnemyRequirementChannel.StructuralPredicate,
                        bindings.Select(value =>
                            new EnemyRequirementChannelApplicabilityRowSnapshot(
                                value.Source.MigrationRouteId,
                                value.Source.DeclaredChannel,
                                value.Source.ApplicabilityState))));
            }
            catch (EnemyRequirementChannelApplicabilityValidationException)
            {
                Invalid(issues, "N01B_SNAPSHOT_REJECTED", "n01b",
                    "The authoritative N01B provider rejected the overlay input.");
                return Finish(source, null, issues);
            }

            List<LayoutResilienceRequirementChannelMigrationRowSnapshot> snapshots =
                new List<LayoutResilienceRequirementChannelMigrationRowSnapshot>();
            foreach (RowBinding binding in bindings)
            {
                EnemyRequirementChannelApplicabilityRowSnapshot channelRow =
                    channelSnapshot.Rows.Single(value => string.Equals(
                        value.RequirementId, binding.Source.MigrationRouteId,
                        StringComparison.Ordinal));
                snapshots.Add(
                    new LayoutResilienceRequirementChannelMigrationRowSnapshot(
                        binding.Source, binding.Authoring, channelRow));
            }

            LayoutResilienceRequirementChannelMigrationPayload payload =
                new LayoutResilienceRequirementChannelMigrationPayload(
                    snapshots,
                    channelSnapshot,
                    p5A.CanonicalSignature,
                    channelSnapshot.CanonicalSignature,
                    false,
                    false);
            return Finish(source, payload, issues);
        }

        private static void ValidateSource(
            LayoutResilienceRequirementChannelMigrationSource source,
            ICollection<LayoutResilienceRequirementChannelMigrationIssue> issues)
        {
            if (!string.Equals(source.SchemaId,
                    LayoutResilienceRequirementChannelMigrationSchema.SchemaId,
                    StringComparison.Ordinal) ||
                source.SchemaVersion !=
                    LayoutResilienceRequirementChannelMigrationSchema.SchemaVersion)
            {
                Invalid(issues, "SCHEMA_MISMATCH", "schema",
                    "Schema identity and version must match exactly.");
            }
            if (!source.DevOnly || source.IsEnabled ||
                !source.CoordinateBaselineAccepted || source.Activated)
            {
                Invalid(issues, "SOURCE_ISOLATION_INVALID", "flags",
                    "The overlay source must be devOnly, disabled, coordinate-accepted and inactive.");
            }
            if (source.Rows == null)
            {
                Unknown(issues, "ROWS_MISSING", "rows",
                    "Four explicit route rows are required.");
                return;
            }
            if (source.Rows.Count != 4)
            {
                Invalid(issues, "ROUTE_COUNT_INVALID", "rows",
                    "The overlay source must contain exactly four explicit routes.");
            }
        }

        private static void ValidateP5A(
            DevEncounterLayoutPressureAuthoringResult result,
            ICollection<LayoutResilienceRequirementChannelMigrationIssue> issues)
        {
            if (result == null)
            {
                Unknown(issues, "P5A_RESULT_MISSING", "p5a",
                    "The authoritative P5-A result is missing.");
                return;
            }
            if (!Enum.IsDefined(typeof(DevEncounterLayoutPressureAuthoringStatus),
                    result.Status))
            {
                Invalid(issues, "P5A_STATUS_UNDEFINED", "p5a.status",
                    "An undefined P5-A status is invalid.");
            }
            else if (result.Status ==
                DevEncounterLayoutPressureAuthoringStatus.Invalid)
            {
                Invalid(issues, "P5A_INVALID", "p5a",
                    "The authoritative P5-A result is invalid.");
            }
            else if (result.Status ==
                DevEncounterLayoutPressureAuthoringStatus.Unknown)
            {
                Unknown(issues, "P5A_UNKNOWN", "p5a",
                    "The authoritative P5-A result is incomplete.");
            }
            if (result.Rows == null || result.Rows.Count != 4)
            {
                Unknown(issues, "P5A_ROWS_MISSING", "p5a.rows",
                    "The authoritative P5-A result must expose four Context rows.");
            }
        }

        private static RouteSpec ValidateRoute(
            LayoutResilienceRequirementChannelMigrationSourceRow row,
            string path,
            ICollection<LayoutResilienceRequirementChannelMigrationIssue> issues)
        {
            string[] identity =
            {
                row.MigrationRouteId, row.CandidateMigrationRequirementId,
                row.OwnerId, row.RequirementGroupId, row.Role, row.MatchMode,
                row.ReferenceKind, row.BuildCapabilityKey, row.LegacyEvidencePath,
                row.LegacyEvidenceRowIdentity, row.SeedId, row.EncounterId,
                row.MapRuleId, row.PressureInputId
            };
            if (identity.Any(string.IsNullOrEmpty))
            {
                Unknown(issues, "IDENTITY_OR_CONTEXT_MISSING", path,
                    "All exact route, provenance and Context identities are required.");
            }
            if (!IsDefined(row.DeclaredChannel) ||
                !IsDefined(row.EvaluationChannel) ||
                !IsDefined(row.ApplicabilityState))
            {
                Invalid(issues, "CHANNEL_OR_APPLICABILITY_UNDEFINED",
                    path + ".channel",
                    "All channel and applicability enum values must be defined.");
            }
            if (row.DeclaredChannel != EnemyRequirementChannel.StructuralPredicate ||
                row.EvaluationChannel !=
                    EnemyRequirementChannel.StructuralPredicate ||
                row.ApplicabilityState !=
                    EnemyRequirementApplicabilityState.Applicable)
            {
                Invalid(issues, "CHANNEL_OR_APPLICABILITY_DRIFT",
                    path + ".channel",
                    "Every overlay route must be StructuralPredicate and Applicable.");
            }
            if (!row.DevOnly || row.IsEnabled ||
                !row.CoordinateBaselineAccepted || row.Activated ||
                row.FormalRequirementSourceModified || row.BehaviorChanged)
            {
                Invalid(issues, "ROW_ISOLATION_INVALID", path + ".flags",
                    "Every overlay route must remain isolated and behavior-neutral.");
            }
            if (!row.LegacyBpQuarantined ||
                !row.LegacyBpProvenanceVerified || row.LegacyBpEvaluated ||
                row.LegacyBpConverted ||
                row.LegacyBpComparedForCapabilityDecision ||
                row.LegacyBpUsedAsThreshold)
            {
                Invalid(issues, "LEGACY_BP_USAGE_INVALID", path + ".legacy",
                    "Legacy BP must be quarantined provenance only.");
            }

            RouteSpec spec = Specs.FirstOrDefault(value => value.MatchesRoute(row));
            if (spec == null)
            {
                Invalid(issues, "ROUTE_IDENTITY_DRIFT", path,
                    "The route is not one of the four approved explicit routes.");
                return null;
            }
            if (!spec.MatchesAll(row))
            {
                Invalid(issues, "ROUTE_PROVENANCE_DRIFT", path,
                    "The route identity, Context or frozen provenance drifted.");
            }
            return spec;
        }

        private static DevEncounterLayoutPressureAuthoringRowSnapshot FindP5ARow(
            LayoutResilienceRequirementChannelMigrationSourceRow row,
            DevEncounterLayoutPressureAuthoringResult p5A,
            string path,
            ICollection<LayoutResilienceRequirementChannelMigrationIssue> issues)
        {
            if (p5A == null || p5A.Rows == null) return null;
            DevEncounterLayoutPressureAuthoringRowSnapshot[] matches = p5A.Rows
                .Where(value => value != null &&
                    string.Equals(value.CandidateMigrationRequirementId,
                        row.CandidateMigrationRequirementId,
                        StringComparison.Ordinal) &&
                    string.Equals(value.SeedId, row.SeedId,
                        StringComparison.Ordinal) &&
                    string.Equals(value.EncounterId, row.EncounterId,
                        StringComparison.Ordinal) &&
                    string.Equals(value.MapRuleId, row.MapRuleId,
                        StringComparison.Ordinal) &&
                    string.Equals(value.PressureInputId, row.PressureInputId,
                        StringComparison.Ordinal)).ToArray();
            if (matches.Length == 0)
            {
                Unknown(issues, "P5A_MATCH_MISSING", path + ".p5a",
                    "No exact authoritative P5-A Context row matched the route.");
                return null;
            }
            if (matches.Length != 1)
            {
                Invalid(issues, "P5A_MATCH_DUPLICATE", path + ".p5a",
                    "The authoritative P5-A Context match must be unique.");
                return null;
            }
            return matches[0];
        }

        private static void ValidateAuthoringBinding(
            LayoutResilienceRequirementChannelMigrationSourceRow row,
            RouteSpec spec,
            DevEncounterLayoutPressureAuthoringRowSnapshot authoring,
            string path,
            ICollection<LayoutResilienceRequirementChannelMigrationIssue> issues)
        {
            if (!string.Equals(authoring.OwnerId, row.OwnerId,
                    StringComparison.Ordinal) ||
                !string.Equals(authoring.RequirementGroupId,
                    row.RequirementGroupId, StringComparison.Ordinal) ||
                !string.Equals(authoring.BuildCapabilityKey,
                    row.BuildCapabilityKey, StringComparison.Ordinal))
            {
                Invalid(issues, "P5A_PROVENANCE_DRIFT", path + ".p5a",
                    "The authoritative P5-A owner/group/key tuple drifted.");
            }
            if (authoring.P2Status != AuthoredLayoutPressureSourceStatus.Complete ||
                authoring.P2Completeness !=
                    LayoutResilienceInputCompleteness.Complete ||
                authoring.PressureSnapshot == null)
            {
                Unknown(issues, "P5A_PRESSURE_INCOMPLETE", path + ".p5a",
                    "The authoritative P5-A/P2 pressure facts are incomplete.");
                return;
            }
            if (!authoring.PressureSnapshot.RequiredPredicateClauses
                    .OrderBy(value => (int)value)
                    .SequenceEqual(spec.Clauses.OrderBy(value => (int)value)))
            {
                Invalid(issues, "P5A_CLAUSE_DRIFT", path + ".p5a.clauses",
                    "The accepted structural predicate clauses drifted.");
            }
        }

        private static void ValidateUniqueness(
            IReadOnlyList<LayoutResilienceRequirementChannelMigrationSourceRow> rows,
            ICollection<LayoutResilienceRequirementChannelMigrationIssue> issues)
        {
            LayoutResilienceRequirementChannelMigrationSourceRow[] present = rows
                .Where(value => value != null).ToArray();
            Duplicate(present.Select(value => value.MigrationRouteId),
                "MIGRATION_ROUTE_ID_DUPLICATE", "rows.migrationRouteId", issues);
            Duplicate(present.Select(value => value.PressureInputId),
                "PRESSURE_INPUT_ID_DUPLICATE", "rows.pressureInputId", issues);
            Duplicate(present.Select(ContextIdentity),
                "CONTEXT_IDENTITY_DUPLICATE", "rows.contextIdentity", issues);
        }

        private static void ValidateCandidateTupleStability(
            IReadOnlyList<LayoutResilienceRequirementChannelMigrationSourceRow> rows,
            ICollection<LayoutResilienceRequirementChannelMigrationIssue> issues)
        {
            foreach (IGrouping<string,
                LayoutResilienceRequirementChannelMigrationSourceRow> group in rows
                    .Where(value => value != null)
                    .GroupBy(value => value.CandidateMigrationRequirementId,
                        StringComparer.Ordinal))
            {
                int tupleCount = group.Select(CandidateTuple)
                    .Distinct(StringComparer.Ordinal).Count();
                if (tupleCount != 1)
                {
                    Invalid(issues, "CANDIDATE_TUPLE_DRIFT", "rows." + group.Key,
                        "A reused candidate must keep one exact provenance tuple.");
                }
            }
        }

        private static void ValidateExpectedCoverage(
            IReadOnlyList<LayoutResilienceRequirementChannelMigrationSourceRow> rows,
            ICollection<LayoutResilienceRequirementChannelMigrationIssue> issues)
        {
            string[] present = rows.Where(value => value != null)
                .Select(value => value.MigrationRouteId).ToArray();
            foreach (RouteSpec spec in Specs)
            {
                if (!present.Contains(spec.RouteId, StringComparer.Ordinal))
                {
                    Unknown(issues, "APPROVED_ROUTE_MISSING", "rows." + spec.RouteId,
                        "An approved explicit overlay route is missing.");
                }
            }
        }

        private static void Duplicate(
            IEnumerable<string> values,
            string code,
            string path,
            ICollection<LayoutResilienceRequirementChannelMigrationIssue> issues)
        {
            string[] present = values.Where(value => !string.IsNullOrEmpty(value))
                .ToArray();
            if (present.Distinct(StringComparer.Ordinal).Count() != present.Length)
                Invalid(issues, code, path,
                    "The exact identity must be globally unique.");
        }

        private static string ContextIdentity(
            LayoutResilienceRequirementChannelMigrationSourceRow row)
        {
            return string.Join("|", row.CandidateMigrationRequirementId,
                row.SeedId, row.EncounterId, row.MapRuleId);
        }

        private static string CandidateTuple(
            LayoutResilienceRequirementChannelMigrationSourceRow row)
        {
            return string.Join("|", row.OwnerId, row.RequirementGroupId,
                row.Role, row.MatchMode, row.ReferenceKind,
                row.BuildCapabilityKey,
                row.LegacyMinimumCapabilityBasisPoints.ToString(
                    CultureInfo.InvariantCulture));
        }

        private static bool IsDefined(EnemyRequirementChannel value)
        {
            return value == EnemyRequirementChannel.ContinuousBP ||
                value == EnemyRequirementChannel.StructuralPredicate ||
                value == EnemyRequirementChannel.RuntimeEventSignal;
        }

        private static bool IsDefined(EnemyRequirementApplicabilityState value)
        {
            return value == EnemyRequirementApplicabilityState.Applicable ||
                value == EnemyRequirementApplicabilityState.Unknown ||
                value == EnemyRequirementApplicabilityState.NotApplicable ||
                value == EnemyRequirementApplicabilityState.NotInChannel;
        }

        private static LayoutResilienceRequirementChannelMigrationResult Finish(
            LayoutResilienceRequirementChannelMigrationSource source,
            LayoutResilienceRequirementChannelMigrationPayload payload,
            IEnumerable<LayoutResilienceRequirementChannelMigrationIssue> issueValues)
        {
            LayoutResilienceRequirementChannelMigrationIssue[] issues =
                issueValues.ToArray();
            LayoutResilienceRequirementChannelMigrationStatus status = issues.Any(
                    value => value.Status ==
                        LayoutResilienceRequirementChannelMigrationStatus.Invalid)
                ? LayoutResilienceRequirementChannelMigrationStatus.Invalid
                : issues.Any(value => value.Status ==
                    LayoutResilienceRequirementChannelMigrationStatus.Unknown)
                    ? LayoutResilienceRequirementChannelMigrationStatus.Unknown
                    : LayoutResilienceRequirementChannelMigrationStatus.Complete;
            if (status != LayoutResilienceRequirementChannelMigrationStatus.Complete)
                payload = null;
            bool devOnly = source == null || source.DevOnly;
            bool isEnabled = source != null && source.IsEnabled;
            bool coordinateAccepted = source != null &&
                source.CoordinateBaselineAccepted;
            bool activated = source != null && source.Activated;
            string canonical =
                LayoutResilienceRequirementChannelMigrationCanonical.Create(
                    source, status, devOnly, isEnabled, coordinateAccepted,
                    activated, payload, issues);
            return new LayoutResilienceRequirementChannelMigrationResult(
                status, devOnly, isEnabled, coordinateAccepted, activated,
                payload, issues, canonical);
        }

        private static void Unknown(
            ICollection<LayoutResilienceRequirementChannelMigrationIssue> issues,
            string code,
            string path,
            string message)
        {
            issues.Add(new LayoutResilienceRequirementChannelMigrationIssue(
                code, LayoutResilienceRequirementChannelMigrationStatus.Unknown,
                path, message));
        }

        private static void Invalid(
            ICollection<LayoutResilienceRequirementChannelMigrationIssue> issues,
            string code,
            string path,
            string message)
        {
            issues.Add(new LayoutResilienceRequirementChannelMigrationIssue(
                code, LayoutResilienceRequirementChannelMigrationStatus.Invalid,
                path, message));
        }

        private sealed class RowBinding
        {
            public RowBinding(
                LayoutResilienceRequirementChannelMigrationSourceRow source,
                DevEncounterLayoutPressureAuthoringRowSnapshot authoring)
            {
                Source = source;
                Authoring = authoring;
            }

            public LayoutResilienceRequirementChannelMigrationSourceRow Source { get; }
            public DevEncounterLayoutPressureAuthoringRowSnapshot Authoring { get; }
        }

        private sealed class RouteSpec
        {
            public RouteSpec(
                string routeId,
                string candidateId,
                string ownerId,
                string groupId,
                int legacyBp,
                string seedId,
                string encounterId,
                string mapRuleId,
                string pressureInputId,
                params LayoutResiliencePredicateClauseKind[] clauses)
            {
                RouteId = routeId;
                CandidateId = candidateId;
                OwnerId = ownerId;
                GroupId = groupId;
                LegacyBp = legacyBp;
                SeedId = seedId;
                EncounterId = encounterId;
                MapRuleId = mapRuleId;
                PressureInputId = pressureInputId;
                Clauses = clauses;
            }

            public string RouteId { get; }
            public string CandidateId { get; }
            public string OwnerId { get; }
            public string GroupId { get; }
            public int LegacyBp { get; }
            public string SeedId { get; }
            public string EncounterId { get; }
            public string MapRuleId { get; }
            public string PressureInputId { get; }
            public IReadOnlyList<LayoutResiliencePredicateClauseKind> Clauses { get; }

            public bool MatchesRoute(
                LayoutResilienceRequirementChannelMigrationSourceRow row)
            {
                return string.Equals(RouteId, row.MigrationRouteId,
                    StringComparison.Ordinal);
            }

            public bool MatchesAll(
                LayoutResilienceRequirementChannelMigrationSourceRow row)
            {
                return string.Equals(CandidateId,
                        row.CandidateMigrationRequirementId,
                        StringComparison.Ordinal) &&
                    string.Equals(OwnerId, row.OwnerId, StringComparison.Ordinal) &&
                    string.Equals(GroupId, row.RequirementGroupId,
                        StringComparison.Ordinal) &&
                    string.Equals("Required", row.Role,
                        StringComparison.Ordinal) &&
                    string.Equals("All", row.MatchMode,
                        StringComparison.Ordinal) &&
                    string.Equals("BuildCapability", row.ReferenceKind,
                        StringComparison.Ordinal) &&
                    string.Equals("capability.placement_shape",
                        row.BuildCapabilityKey, StringComparison.Ordinal) &&
                    row.LegacyMinimumCapabilityBasisPoints == LegacyBp &&
                    string.Equals(
                        "Docs/V0.4/Reports/DevEncounterSeedPressureWindowRows.csv",
                        row.LegacyEvidencePath, StringComparison.Ordinal) &&
                    string.Equals("Requirement|" + OwnerId + "|" + GroupId +
                        "|Required|All|BuildCapability|capability.placement_shape",
                        row.LegacyEvidenceRowIdentity, StringComparison.Ordinal) &&
                    string.Equals(SeedId, row.SeedId, StringComparison.Ordinal) &&
                    string.Equals(EncounterId, row.EncounterId,
                        StringComparison.Ordinal) &&
                    string.Equals(MapRuleId, row.MapRuleId,
                        StringComparison.Ordinal) &&
                    string.Equals(PressureInputId, row.PressureInputId,
                        StringComparison.Ordinal);
            }
        }

        private static readonly LayoutResiliencePredicateClauseKind[]
            FormationClauses =
        {
            LayoutResiliencePredicateClauseKind.CountedLayoutPresent,
            LayoutResiliencePredicateClauseKind.EffectiveEyeAnchorUsable,
            LayoutResiliencePredicateClauseKind
                .EyeToCountedCoreStructurallyConnected
        };

        private static readonly LayoutResiliencePredicateClauseKind[]
            PollutionClauses =
        {
            LayoutResiliencePredicateClauseKind.CountedLayoutPresent,
            LayoutResiliencePredicateClauseKind.CountedPlacementCellsUsable,
            LayoutResiliencePredicateClauseKind.CountedPlacementCoresUsable
        };

        private static readonly RouteSpec[] Specs =
        {
            new RouteSpec(
                "route.layout_resilience.formation_eye.dev_encounter_4_10_furnace_core.dev_map_furnace_ash_fall.v1",
                "layout_resilience.formation_eye.placement_shape",
                "dev_enemy_formation_eye_problem",
                "dev_enemy_formation_eye_problem.required",
                5340,
                "dev_seed_4_10_furnace_core",
                "dev_encounter_4_10_furnace_core",
                "dev_map_furnace_ash_fall",
                "pressure.layout_resilience.formation_eye.dev_encounter_4_10_furnace_core.dev_map_furnace_ash_fall.v1",
                FormationClauses),
            new RouteSpec(
                "route.layout_resilience.formation_eye.dev_encounter_4_10_thunder_fire_cross.dev_map_bluestone_crack.v1",
                "layout_resilience.formation_eye.placement_shape",
                "dev_enemy_formation_eye_problem",
                "dev_enemy_formation_eye_problem.required",
                5340,
                "dev_seed_4_10_thunder_fire_cross",
                "dev_encounter_4_10_thunder_fire_cross",
                "dev_map_bluestone_crack",
                "pressure.layout_resilience.formation_eye.dev_encounter_4_10_thunder_fire_cross.dev_map_bluestone_crack.v1",
                FormationClauses),
            new RouteSpec(
                "route.layout_resilience.polluted_tile.dev_encounter_3_10_cleanse_corner.dev_map_bluestone_damp.v1",
                "layout_resilience.polluted_tile.placement_shape",
                "dev_enemy_polluted_tile_problem",
                "dev_enemy_polluted_tile_problem.required",
                5383,
                "dev_seed_3_10_cleanse_corner",
                "dev_encounter_3_10_cleanse_corner",
                "dev_map_bluestone_damp",
                "pressure.layout_resilience.polluted_tile.dev_encounter_3_10_cleanse_corner.dev_map_bluestone_damp.v1",
                PollutionClauses),
            new RouteSpec(
                "route.layout_resilience.polluted_tile.dev_encounter_4_10_furnace_core.dev_map_furnace_ash_fall.v1",
                "layout_resilience.polluted_tile.placement_shape",
                "dev_enemy_polluted_tile_problem",
                "dev_enemy_polluted_tile_problem.required",
                5383,
                "dev_seed_4_10_furnace_core",
                "dev_encounter_4_10_furnace_core",
                "dev_map_furnace_ash_fall",
                "pressure.layout_resilience.polluted_tile.dev_encounter_4_10_furnace_core.dev_map_furnace_ash_fall.v1",
                PollutionClauses)
        };
    }
}
