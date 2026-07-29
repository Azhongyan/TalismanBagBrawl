using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.AuthoredPressure;

namespace TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.DevAuthoring
{
    public static class DevEncounterLayoutPressureValidation
    {
        private const string FormationCandidate =
            "layout_resilience.formation_eye.placement_shape";
        private const string FormationOwner = "dev_enemy_formation_eye_problem";
        private const string FormationGroup =
            "dev_enemy_formation_eye_problem.required";
        private const string PollutionCandidate =
            "layout_resilience.polluted_tile.placement_shape";
        private const string PollutionOwner = "dev_enemy_polluted_tile_problem";
        private const string PollutionGroup =
            "dev_enemy_polluted_tile_problem.required";
        private const string PlacementShapeKey = "capability.placement_shape";

        public static DevEncounterLayoutPressureAuthoringResult ValidateAndProject(
            DevEncounterLayoutPressureAuthoringSource source)
        {
            List<DevEncounterLayoutPressureAuthoringIssue> issues =
                new List<DevEncounterLayoutPressureAuthoringIssue>();
            List<DevEncounterLayoutPressureAuthoringRowSnapshot> snapshots =
                new List<DevEncounterLayoutPressureAuthoringRowSnapshot>();
            if (source == null)
            {
                Unknown(issues, "SOURCE_MISSING", "$", "The authoring source is missing.");
                return Finish(source, snapshots, issues);
            }

            if (!string.Equals(source.SchemaId,
                    DevEncounterLayoutPressureAuthoringSchema.SchemaId,
                    StringComparison.Ordinal) ||
                source.SchemaVersion != DevEncounterLayoutPressureAuthoringSchema.SchemaVersion)
            {
                Invalid(issues, "SCHEMA_MISMATCH", "schema",
                    "Schema identity and version must match exactly.");
            }
            if (!source.DevOnly || source.IsEnabled)
            {
                Invalid(issues, "SOURCE_ISOLATION_INVALID", "flags",
                    "The source must remain devOnly=true and isEnabled=false.");
            }
            if (source.Rows == null)
            {
                Unknown(issues, "ROWS_MISSING", "rows", "Four explicit Context rows are required.");
                return Finish(source, snapshots, issues);
            }
            if (source.Rows.Count != 4)
            {
                Invalid(issues, "ROW_COUNT_INVALID", "rows",
                    "The candidate source must contain exactly four Context rows.");
            }

            for (int index = 0; index < source.Rows.Count; index++)
            {
                string path = "rows[" + index.ToString(CultureInfo.InvariantCulture) + "]";
                DevEncounterLayoutPressureAuthoringSourceRow row = source.Rows[index];
                if (row == null)
                {
                    Unknown(issues, "ROW_MISSING", path, "A Context row is missing.");
                    continue;
                }

                ContextSpec spec = ValidatePackageRow(row, path, issues);
                AuthoredLayoutPressureSourceResult p2 =
                    DefaultAuthoredLayoutPressureSourceAdapter.Instance.Project(
                        row.PressureSource);
                snapshots.Add(new DevEncounterLayoutPressureAuthoringRowSnapshot(row, p2));
                PromoteP2(path, p2, issues);
                if (spec != null && p2 != null &&
                    p2.Status == AuthoredLayoutPressureSourceStatus.Complete)
                {
                    ValidateFixedResult(row, spec, p2.PressureSnapshot, path, issues);
                }
            }

            ValidateUniqueness(source.Rows, issues);
            return Finish(source, snapshots, issues);
        }

        private static ContextSpec ValidatePackageRow(
            DevEncounterLayoutPressureAuthoringSourceRow row,
            string path,
            ICollection<DevEncounterLayoutPressureAuthoringIssue> issues)
        {
            string[] identity =
            {
                row.CandidateMigrationRequirementId, row.OwnerId,
                row.RequirementGroupId, row.BuildCapabilityKey,
                row.SeedId, row.EncounterId, row.MapRuleId, row.PressureInputId
            };
            if (identity.Any(string.IsNullOrEmpty))
            {
                Unknown(issues, "IDENTITY_OR_CONTEXT_MISSING", path,
                    "All exact identity and Context fields are required.");
            }
            if (!row.DevOnly || row.IsEnabled || row.UserCoordinateAccepted || row.Activated)
            {
                Invalid(issues, "ROW_ISOLATION_INVALID", path + ".flags",
                    "Rows must remain devOnly, disabled, unaccepted and inactive.");
            }

            ContextSpec spec = Specs.FirstOrDefault(value => value.MatchesContext(row));
            if (spec == null)
            {
                if (!string.IsNullOrEmpty(row.CandidateMigrationRequirementId))
                {
                    Invalid(issues, "CONTEXT_IDENTITY_DRIFT", path,
                        "The candidate/context tuple is not one of the four approved rows.");
                }
                return null;
            }
            if (!spec.MatchesOwner(row))
            {
                Invalid(issues, "OWNER_GROUP_KEY_DRIFT", path,
                    "The exact owner/group/key tuple drifted for this candidate.");
            }
            if (row.PressureSource != null && !string.Equals(
                    row.PressureInputId, row.PressureSource.PressureInputId,
                    StringComparison.Ordinal))
            {
                Invalid(issues, "PRESSURE_ID_BINDING_DRIFT", path + ".pressureSource",
                    "The row and P2 source PressureInputId must match exactly.");
            }
            return spec;
        }

        private static void ValidateFixedResult(
            DevEncounterLayoutPressureAuthoringSourceRow row,
            ContextSpec spec,
            LayoutPressureSnapshot snapshot,
            string path,
            ICollection<DevEncounterLayoutPressureAuthoringIssue> issues)
        {
            if (snapshot == null ||
                !string.Equals(snapshot.PressureInputId, row.PressureInputId,
                    StringComparison.Ordinal) ||
                snapshot.LayoutDomainCells.Count != 25 ||
                snapshot.UsableCellsAfterPressure.Count != spec.UsableCount ||
                snapshot.PreservedStructuralConnectionsAfterPressure.Count !=
                    spec.ConnectionCount ||
                snapshot.RequiredPredicateClauses.Count != 3)
            {
                Invalid(issues, "FIXED_AUTHORING_COUNT_DRIFT", path + ".p2",
                    "P2 output does not match the approved fixed counts for this Context.");
            }
        }

        private static void PromoteP2(
            string path,
            AuthoredLayoutPressureSourceResult p2,
            ICollection<DevEncounterLayoutPressureAuthoringIssue> issues)
        {
            if (p2 == null)
            {
                Unknown(issues, "P2_RESULT_MISSING", path + ".p2",
                    "The authoritative P2 result is missing.");
                return;
            }
            if (!Enum.IsDefined(typeof(AuthoredLayoutPressureSourceStatus), p2.Status))
            {
                Invalid(issues, "P2_STATUS_UNDEFINED", path + ".p2.status",
                    "An undefined P2 status is invalid.");
            }
            else if (p2.Status == AuthoredLayoutPressureSourceStatus.Invalid)
            {
                Invalid(issues, "P2_INVALID", path + ".p2",
                    "The authoritative P2 adapter rejected the pressure facts.");
            }
            else if (p2.Status == AuthoredLayoutPressureSourceStatus.Unknown)
            {
                Unknown(issues, "P2_UNKNOWN", path + ".p2",
                    "The authoritative P2 adapter found incomplete pressure facts.");
            }
        }

        private static void ValidateUniqueness(
            IReadOnlyList<DevEncounterLayoutPressureAuthoringSourceRow> rows,
            ICollection<DevEncounterLayoutPressureAuthoringIssue> issues)
        {
            DevEncounterLayoutPressureAuthoringSourceRow[] present = rows
                .Where(value => value != null).ToArray();
            Duplicate(present.Select(value => value.PressureInputId),
                "PRESSURE_INPUT_ID_DUPLICATE", "rows.pressureInputId", issues);
            Duplicate(present.Select(ContextIdentity), "CONTEXT_IDENTITY_DUPLICATE",
                "rows.contextIdentity", issues);
            foreach (IGrouping<string, DevEncounterLayoutPressureAuthoringSourceRow> group
                in present.GroupBy(value => value.CandidateMigrationRequirementId,
                    StringComparer.Ordinal))
            {
                int tuples = group.Select(value => string.Join("|", value.OwnerId,
                        value.RequirementGroupId, value.BuildCapabilityKey))
                    .Distinct(StringComparer.Ordinal).Count();
                if (tuples > 1)
                {
                    Invalid(issues, "CANDIDATE_OWNER_TUPLE_DRIFT", "rows." + group.Key,
                        "A reused candidate must keep one exact owner/group/key tuple.");
                }
            }
        }

        private static void Duplicate(
            IEnumerable<string> values,
            string code,
            string path,
            ICollection<DevEncounterLayoutPressureAuthoringIssue> issues)
        {
            string[] present = values.Where(value => !string.IsNullOrEmpty(value)).ToArray();
            if (present.Distinct(StringComparer.Ordinal).Count() != present.Length)
            {
                Invalid(issues, code, path, "The exact identity must be globally unique.");
            }
        }

        private static string ContextIdentity(
            DevEncounterLayoutPressureAuthoringSourceRow row)
        {
            return string.Join("|", row.CandidateMigrationRequirementId, row.SeedId,
                row.EncounterId, row.MapRuleId);
        }

        private static DevEncounterLayoutPressureAuthoringResult Finish(
            DevEncounterLayoutPressureAuthoringSource source,
            IEnumerable<DevEncounterLayoutPressureAuthoringRowSnapshot> snapshots,
            IEnumerable<DevEncounterLayoutPressureAuthoringIssue> issues)
        {
            DevEncounterLayoutPressureAuthoringRowSnapshot[] rowArray = snapshots.ToArray();
            DevEncounterLayoutPressureAuthoringIssue[] issueArray = issues.ToArray();
            DevEncounterLayoutPressureAuthoringStatus status = issueArray.Any(value =>
                    value.Status == DevEncounterLayoutPressureAuthoringStatus.Invalid)
                ? DevEncounterLayoutPressureAuthoringStatus.Invalid
                : issueArray.Any(value =>
                    value.Status == DevEncounterLayoutPressureAuthoringStatus.Unknown)
                    ? DevEncounterLayoutPressureAuthoringStatus.Unknown
                    : DevEncounterLayoutPressureAuthoringStatus.CandidateComplete;
            bool devOnly = source == null || source.DevOnly;
            bool isEnabled = source != null && source.IsEnabled;
            string signature = DevEncounterLayoutPressureAuthoringCanonical.Create(
                source, status, devOnly, isEnabled, rowArray, issueArray);
            return new DevEncounterLayoutPressureAuthoringResult(status, devOnly,
                isEnabled, rowArray, issueArray, signature);
        }

        private static void Unknown(
            ICollection<DevEncounterLayoutPressureAuthoringIssue> issues,
            string code, string path, string message)
        {
            issues.Add(new DevEncounterLayoutPressureAuthoringIssue(code,
                DevEncounterLayoutPressureAuthoringStatus.Unknown, path, message));
        }

        private static void Invalid(
            ICollection<DevEncounterLayoutPressureAuthoringIssue> issues,
            string code, string path, string message)
        {
            issues.Add(new DevEncounterLayoutPressureAuthoringIssue(code,
                DevEncounterLayoutPressureAuthoringStatus.Invalid, path, message));
        }

        private static readonly ContextSpec[] Specs =
        {
            new ContextSpec(FormationCandidate, FormationOwner, FormationGroup,
                "dev_seed_4_10_furnace_core", "dev_encounter_4_10_furnace_core",
                "dev_map_furnace_ash_fall",
                "pressure.layout_resilience.formation_eye.dev_encounter_4_10_furnace_core.dev_map_furnace_ash_fall.v1",
                25, 35),
            new ContextSpec(FormationCandidate, FormationOwner, FormationGroup,
                "dev_seed_4_10_thunder_fire_cross",
                "dev_encounter_4_10_thunder_fire_cross", "dev_map_bluestone_crack",
                "pressure.layout_resilience.formation_eye.dev_encounter_4_10_thunder_fire_cross.dev_map_bluestone_crack.v1",
                25, 35),
            new ContextSpec(PollutionCandidate, PollutionOwner, PollutionGroup,
                "dev_seed_3_10_cleanse_corner", "dev_encounter_3_10_cleanse_corner",
                "dev_map_bluestone_damp",
                "pressure.layout_resilience.polluted_tile.dev_encounter_3_10_cleanse_corner.dev_map_bluestone_damp.v1",
                23, 32),
            new ContextSpec(PollutionCandidate, PollutionOwner, PollutionGroup,
                "dev_seed_4_10_furnace_core", "dev_encounter_4_10_furnace_core",
                "dev_map_furnace_ash_fall",
                "pressure.layout_resilience.polluted_tile.dev_encounter_4_10_furnace_core.dev_map_furnace_ash_fall.v1",
                21, 24)
        };

        private sealed class ContextSpec
        {
            public ContextSpec(string candidate, string owner, string group,
                string seed, string encounter, string map, string pressure,
                int usableCount, int connectionCount)
            {
                Candidate = candidate;
                Owner = owner;
                Group = group;
                Seed = seed;
                Encounter = encounter;
                Map = map;
                Pressure = pressure;
                UsableCount = usableCount;
                ConnectionCount = connectionCount;
            }

            public string Candidate { get; }
            public string Owner { get; }
            public string Group { get; }
            public string Seed { get; }
            public string Encounter { get; }
            public string Map { get; }
            public string Pressure { get; }
            public int UsableCount { get; }
            public int ConnectionCount { get; }

            public bool MatchesContext(DevEncounterLayoutPressureAuthoringSourceRow row)
            {
                return string.Equals(Candidate, row.CandidateMigrationRequirementId,
                        StringComparison.Ordinal) &&
                    string.Equals(Seed, row.SeedId, StringComparison.Ordinal) &&
                    string.Equals(Encounter, row.EncounterId, StringComparison.Ordinal) &&
                    string.Equals(Map, row.MapRuleId, StringComparison.Ordinal) &&
                    string.Equals(Pressure, row.PressureInputId, StringComparison.Ordinal);
            }

            public bool MatchesOwner(DevEncounterLayoutPressureAuthoringSourceRow row)
            {
                return string.Equals(Owner, row.OwnerId, StringComparison.Ordinal) &&
                    string.Equals(Group, row.RequirementGroupId, StringComparison.Ordinal) &&
                    string.Equals(PlacementShapeKey, row.BuildCapabilityKey,
                        StringComparison.Ordinal);
            }
        }
    }
}
