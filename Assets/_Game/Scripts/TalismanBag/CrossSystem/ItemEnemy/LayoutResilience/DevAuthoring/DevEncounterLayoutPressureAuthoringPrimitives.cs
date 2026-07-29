using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.AuthoredPressure;

namespace TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.DevAuthoring
{
    public static class DevEncounterLayoutPressureAuthoringSchema
    {
        public const string SchemaId = "DevEncounterLayoutPressureAuthoring.v1";
        public const int SchemaVersion = 1;
    }

    public enum DevEncounterLayoutPressureAuthoringStatus
    {
        CandidateComplete = 1,
        Unknown = 2,
        Invalid = 3
    }

    public sealed class DevEncounterLayoutPressureAuthoringSource
    {
        private readonly ReadOnlyCollection<DevEncounterLayoutPressureAuthoringSourceRow> rows;

        public DevEncounterLayoutPressureAuthoringSource(
            string schemaId, int schemaVersion, bool devOnly, bool isEnabled,
            IEnumerable<DevEncounterLayoutPressureAuthoringSourceRow> rows)
        {
            SchemaId = schemaId;
            SchemaVersion = schemaVersion;
            DevOnly = devOnly;
            IsEnabled = isEnabled;
            this.rows = rows == null ? null : Array.AsReadOnly(rows.Select(value =>
                value == null ? null : value.Clone()).ToArray());
        }

        public string SchemaId { get; }
        public int SchemaVersion { get; }
        public bool DevOnly { get; }
        public bool IsEnabled { get; }
        public IReadOnlyList<DevEncounterLayoutPressureAuthoringSourceRow> Rows => rows;
    }

    public sealed class DevEncounterLayoutPressureAuthoringSourceRow
    {
        public DevEncounterLayoutPressureAuthoringSourceRow(
            string candidateMigrationRequirementId, string ownerId,
            string requirementGroupId, string buildCapabilityKey, string seedId,
            string encounterId, string mapRuleId, string pressureInputId,
            bool devOnly, bool isEnabled, bool userCoordinateAccepted,
            bool activated, AuthoredLayoutPressureSourceInput pressureSource)
        {
            CandidateMigrationRequirementId = candidateMigrationRequirementId;
            OwnerId = ownerId;
            RequirementGroupId = requirementGroupId;
            BuildCapabilityKey = buildCapabilityKey;
            SeedId = seedId;
            EncounterId = encounterId;
            MapRuleId = mapRuleId;
            PressureInputId = pressureInputId;
            DevOnly = devOnly;
            IsEnabled = isEnabled;
            UserCoordinateAccepted = userCoordinateAccepted;
            Activated = activated;
            PressureSource = DevEncounterLayoutPressureCopies.Input(pressureSource);
        }

        public string CandidateMigrationRequirementId { get; }
        public string OwnerId { get; }
        public string RequirementGroupId { get; }
        public string BuildCapabilityKey { get; }
        public string SeedId { get; }
        public string EncounterId { get; }
        public string MapRuleId { get; }
        public string PressureInputId { get; }
        public bool DevOnly { get; }
        public bool IsEnabled { get; }
        public bool UserCoordinateAccepted { get; }
        public bool Activated { get; }
        public AuthoredLayoutPressureSourceInput PressureSource { get; }

        internal DevEncounterLayoutPressureAuthoringSourceRow Clone()
        {
            return new DevEncounterLayoutPressureAuthoringSourceRow(
                CandidateMigrationRequirementId, OwnerId, RequirementGroupId,
                BuildCapabilityKey, SeedId, EncounterId, MapRuleId, PressureInputId,
                DevOnly, IsEnabled, UserCoordinateAccepted, Activated, PressureSource);
        }
    }

    public sealed class DevEncounterLayoutPressureAuthoringRowSnapshot
    {
        private readonly ReadOnlyCollection<AuthoredLayoutPressureSourceIssue> p2Issues;

        internal DevEncounterLayoutPressureAuthoringRowSnapshot(
            DevEncounterLayoutPressureAuthoringSourceRow source,
            AuthoredLayoutPressureSourceResult p2Result)
        {
            CandidateMigrationRequirementId = source == null ? null : source.CandidateMigrationRequirementId;
            OwnerId = source == null ? null : source.OwnerId;
            RequirementGroupId = source == null ? null : source.RequirementGroupId;
            BuildCapabilityKey = source == null ? null : source.BuildCapabilityKey;
            SeedId = source == null ? null : source.SeedId;
            EncounterId = source == null ? null : source.EncounterId;
            MapRuleId = source == null ? null : source.MapRuleId;
            PressureInputId = source == null ? null : source.PressureInputId;
            DevOnly = source != null && source.DevOnly;
            IsEnabled = source != null && source.IsEnabled;
            UserCoordinateAccepted = source != null && source.UserCoordinateAccepted;
            Activated = source != null && source.Activated;
            P2Status = p2Result == null ? AuthoredLayoutPressureSourceStatus.Unknown : p2Result.Status;
            P2Completeness = p2Result == null ? LayoutResilienceInputCompleteness.Incomplete : p2Result.PressureFactsCompleteness;
            PressureSnapshot = p2Result == null ? null : DevEncounterLayoutPressureCopies.Snapshot(p2Result.PressureSnapshot);
            P2CanonicalSignature = p2Result == null ? string.Empty : p2Result.CanonicalSignature;
            p2Issues = Array.AsReadOnly((p2Result == null
                    ? Array.Empty<AuthoredLayoutPressureSourceIssue>() : p2Result.Issues)
                .Select(DevEncounterLayoutPressureCopies.Issue)
                .OrderBy(value => value.Path, StringComparer.Ordinal)
                .ThenBy(value => value.Code, StringComparer.Ordinal)
                .ThenBy(value => (int)value.Status)
                .ThenBy(value => value.Message, StringComparer.Ordinal).ToArray());
        }

        public string CandidateMigrationRequirementId { get; }
        public string OwnerId { get; }
        public string RequirementGroupId { get; }
        public string BuildCapabilityKey { get; }
        public string SeedId { get; }
        public string EncounterId { get; }
        public string MapRuleId { get; }
        public string PressureInputId { get; }
        public bool DevOnly { get; }
        public bool IsEnabled { get; }
        public bool UserCoordinateAccepted { get; }
        public bool Activated { get; }
        public AuthoredLayoutPressureSourceStatus P2Status { get; }
        public LayoutResilienceInputCompleteness P2Completeness { get; }
        public LayoutPressureSnapshot PressureSnapshot { get; }
        public string P2CanonicalSignature { get; }
        public IReadOnlyList<AuthoredLayoutPressureSourceIssue> P2Issues => p2Issues;
    }

    public sealed class DevEncounterLayoutPressureAuthoringIssue
    {
        public DevEncounterLayoutPressureAuthoringIssue(
            string code, DevEncounterLayoutPressureAuthoringStatus status,
            string path, string message)
        {
            Code = code ?? string.Empty;
            Status = status;
            Path = path ?? string.Empty;
            Message = message ?? string.Empty;
        }

        public string Code { get; }
        public DevEncounterLayoutPressureAuthoringStatus Status { get; }
        public string Path { get; }
        public string Message { get; }
    }

    public sealed class DevEncounterLayoutPressureAuthoringResult
    {
        private readonly ReadOnlyCollection<DevEncounterLayoutPressureAuthoringRowSnapshot> rows;
        private readonly ReadOnlyCollection<DevEncounterLayoutPressureAuthoringIssue> issues;

        internal DevEncounterLayoutPressureAuthoringResult(
            DevEncounterLayoutPressureAuthoringStatus status, bool devOnly,
            bool isEnabled, IEnumerable<DevEncounterLayoutPressureAuthoringRowSnapshot> rows,
            IEnumerable<DevEncounterLayoutPressureAuthoringIssue> issues,
            string canonicalSignature)
        {
            SchemaId = DevEncounterLayoutPressureAuthoringSchema.SchemaId;
            SchemaVersion = DevEncounterLayoutPressureAuthoringSchema.SchemaVersion;
            Status = status;
            DevOnly = devOnly;
            IsEnabled = isEnabled;
            this.rows = Array.AsReadOnly((rows ?? Array.Empty<DevEncounterLayoutPressureAuthoringRowSnapshot>())
                .OrderBy(value => value == null ? string.Empty : value.CandidateMigrationRequirementId, StringComparer.Ordinal)
                .ThenBy(value => value == null ? string.Empty : value.SeedId, StringComparer.Ordinal)
                .ThenBy(value => value == null ? string.Empty : value.EncounterId, StringComparer.Ordinal)
                .ThenBy(value => value == null ? string.Empty : value.MapRuleId, StringComparer.Ordinal)
                .ThenBy(value => value == null ? string.Empty : value.PressureInputId, StringComparer.Ordinal).ToArray());
            this.issues = Array.AsReadOnly((issues ?? Array.Empty<DevEncounterLayoutPressureAuthoringIssue>())
                .OrderBy(value => value.Path, StringComparer.Ordinal)
                .ThenBy(value => value.Code, StringComparer.Ordinal)
                .ThenBy(value => (int)value.Status)
                .ThenBy(value => value.Message, StringComparer.Ordinal).ToArray());
            CanonicalSignature = canonicalSignature ?? string.Empty;
        }

        public string SchemaId { get; }
        public int SchemaVersion { get; }
        public DevEncounterLayoutPressureAuthoringStatus Status { get; }
        public bool DevOnly { get; }
        public bool IsEnabled { get; }
        public IReadOnlyList<DevEncounterLayoutPressureAuthoringRowSnapshot> Rows => rows;
        public IReadOnlyList<DevEncounterLayoutPressureAuthoringIssue> Issues => issues;
        public string CanonicalSignature { get; }
    }

    internal static class DevEncounterLayoutPressureCopies
    {
        public static AuthoredLayoutPressureSourceInput Input(AuthoredLayoutPressureSourceInput value)
        {
            return value == null ? null : new AuthoredLayoutPressureSourceInput(
                value.AuthoringCompleteness, value.PressureInputId, value.DeclaredBoardSize,
                value.PressureKinds, value.LayoutDomainCells, value.UsableCellsAfterPressure,
                value.EffectiveEyeAnchorCellAfterPressure,
                value.PreservedStructuralConnectionsAfterPressure,
                value.RequiredPredicateClauses, value.SchemaId, value.SchemaVersion);
        }

        public static LayoutPressureSnapshot Snapshot(LayoutPressureSnapshot value)
        {
            return value == null ? null : new LayoutPressureSnapshot(
                value.PressureInputId, value.PressureKinds, value.LayoutDomainCells,
                value.UsableCellsAfterPressure, value.EffectiveEyeAnchorCellAfterPressure,
                value.PreservedStructuralConnectionsAfterPressure,
                value.RequiredPredicateClauses);
        }

        public static AuthoredLayoutPressureSourceIssue Issue(AuthoredLayoutPressureSourceIssue value)
        {
            return value == null
                ? new AuthoredLayoutPressureSourceIssue("P2_ISSUE_NULL",
                    AuthoredLayoutPressureSourceStatus.Invalid, string.Empty,
                    "A P2 issue row was null.")
                : new AuthoredLayoutPressureSourceIssue(value.Code, value.Status,
                    value.Path, value.Message);
        }
    }

    internal static class DevEncounterLayoutPressureAuthoringCanonical
    {
        public static string Create(
            DevEncounterLayoutPressureAuthoringSource source,
            DevEncounterLayoutPressureAuthoringStatus status, bool devOnly,
            bool isEnabled, IEnumerable<DevEncounterLayoutPressureAuthoringRowSnapshot> rows,
            IEnumerable<DevEncounterLayoutPressureAuthoringIssue> issues)
        {
            StringBuilder payload = new StringBuilder(65536);
            Field(payload, "schemaId", DevEncounterLayoutPressureAuthoringSchema.SchemaId);
            Field(payload, "schemaVersion", Int(DevEncounterLayoutPressureAuthoringSchema.SchemaVersion));
            Field(payload, "source.present", Bool(source != null));
            if (source != null)
            {
                Text(payload, "source.schemaId", source.SchemaId);
                Field(payload, "source.schemaVersion", Int(source.SchemaVersion));
                Field(payload, "source.devOnly", Bool(source.DevOnly));
                Field(payload, "source.isEnabled", Bool(source.IsEnabled));
                SourceRows(payload, source.Rows);
            }
            Field(payload, "result.status", Int((int)status));
            Field(payload, "result.devOnly", Bool(devOnly));
            Field(payload, "result.isEnabled", Bool(isEnabled));
            ResultRows(payload, rows);
            ResultIssues(payload, issues);
            using (SHA256 sha = SHA256.Create())
            {
                return "sha256:" + string.Concat(sha.ComputeHash(Encoding.UTF8.GetBytes(payload.ToString()))
                    .Select(value => value.ToString("x2", CultureInfo.InvariantCulture)));
            }
        }

        private static void SourceRows(StringBuilder payload,
            IReadOnlyList<DevEncounterLayoutPressureAuthoringSourceRow> values)
        {
            Field(payload, "source.rows.present", Bool(values != null));
            if (values == null) return;
            DevEncounterLayoutPressureAuthoringSourceRow[] rows = values
                .OrderBy(value => value == null ? string.Empty : value.CandidateMigrationRequirementId, StringComparer.Ordinal)
                .ThenBy(value => value == null ? string.Empty : value.SeedId, StringComparer.Ordinal)
                .ThenBy(value => value == null ? string.Empty : value.EncounterId, StringComparer.Ordinal)
                .ThenBy(value => value == null ? string.Empty : value.MapRuleId, StringComparer.Ordinal)
                .ThenBy(value => value == null ? string.Empty : value.PressureInputId, StringComparer.Ordinal).ToArray();
            Field(payload, "source.rows.count", Int(rows.Length));
            for (int index = 0; index < rows.Length; index++)
            {
                string prefix = "source.rows[" + Int(index) + "]";
                Field(payload, prefix + ".present", Bool(rows[index] != null));
                if (rows[index] != null) SourceRow(payload, prefix, rows[index]);
            }
        }

        private static void SourceRow(StringBuilder payload, string prefix,
            DevEncounterLayoutPressureAuthoringSourceRow value)
        {
            Text(payload, prefix + ".candidate", value.CandidateMigrationRequirementId);
            Text(payload, prefix + ".owner", value.OwnerId);
            Text(payload, prefix + ".group", value.RequirementGroupId);
            Text(payload, prefix + ".key", value.BuildCapabilityKey);
            Text(payload, prefix + ".seed", value.SeedId);
            Text(payload, prefix + ".encounter", value.EncounterId);
            Text(payload, prefix + ".map", value.MapRuleId);
            Text(payload, prefix + ".pressureInputId", value.PressureInputId);
            Field(payload, prefix + ".devOnly", Bool(value.DevOnly));
            Field(payload, prefix + ".isEnabled", Bool(value.IsEnabled));
            Field(payload, prefix + ".userCoordinateAccepted", Bool(value.UserCoordinateAccepted));
            Field(payload, prefix + ".activated", Bool(value.Activated));
            PressureInput(payload, prefix + ".pressureSource", value.PressureSource);
        }

        private static void PressureInput(StringBuilder payload, string prefix,
            AuthoredLayoutPressureSourceInput value)
        {
            Field(payload, prefix + ".present", Bool(value != null));
            if (value == null) return;
            Text(payload, prefix + ".schemaId", value.SchemaId);
            Field(payload, prefix + ".schemaVersion", Int(value.SchemaVersion));
            Field(payload, prefix + ".completeness", Int((int)value.AuthoringCompleteness));
            Text(payload, prefix + ".pressureInputId", value.PressureInputId);
            Field(payload, prefix + ".board.present", Bool(value.DeclaredBoardSize.HasValue));
            Field(payload, prefix + ".board", value.DeclaredBoardSize.HasValue ? Int(value.DeclaredBoardSize.Value) : string.Empty);
            Enums(payload, prefix + ".kinds", value.PressureKinds, item => (int)item);
            Cells(payload, prefix + ".domain", value.LayoutDomainCells);
            Cells(payload, prefix + ".usable", value.UsableCellsAfterPressure);
            Cell(payload, prefix + ".eye", value.EffectiveEyeAnchorCellAfterPressure);
            Edges(payload, prefix + ".edges", value.PreservedStructuralConnectionsAfterPressure);
            Enums(payload, prefix + ".clauses", value.RequiredPredicateClauses, item => (int)item);
        }

        private static void ResultRows(StringBuilder payload,
            IEnumerable<DevEncounterLayoutPressureAuthoringRowSnapshot> values)
        {
            DevEncounterLayoutPressureAuthoringRowSnapshot[] rows = (values ?? Array.Empty<DevEncounterLayoutPressureAuthoringRowSnapshot>())
                .OrderBy(value => value == null ? string.Empty : value.CandidateMigrationRequirementId, StringComparer.Ordinal)
                .ThenBy(value => value == null ? string.Empty : value.SeedId, StringComparer.Ordinal)
                .ThenBy(value => value == null ? string.Empty : value.EncounterId, StringComparer.Ordinal)
                .ThenBy(value => value == null ? string.Empty : value.MapRuleId, StringComparer.Ordinal)
                .ThenBy(value => value == null ? string.Empty : value.PressureInputId, StringComparer.Ordinal).ToArray();
            Field(payload, "result.rows.count", Int(rows.Length));
            for (int index = 0; index < rows.Length; index++)
            {
                string prefix = "result.rows[" + Int(index) + "]";
                DevEncounterLayoutPressureAuthoringRowSnapshot row = rows[index];
                Field(payload, prefix + ".present", Bool(row != null));
                if (row == null) continue;
                Text(payload, prefix + ".candidate", row.CandidateMigrationRequirementId);
                Text(payload, prefix + ".owner", row.OwnerId);
                Text(payload, prefix + ".group", row.RequirementGroupId);
                Text(payload, prefix + ".key", row.BuildCapabilityKey);
                Text(payload, prefix + ".seed", row.SeedId);
                Text(payload, prefix + ".encounter", row.EncounterId);
                Text(payload, prefix + ".map", row.MapRuleId);
                Text(payload, prefix + ".pressureInputId", row.PressureInputId);
                Field(payload, prefix + ".devOnly", Bool(row.DevOnly));
                Field(payload, prefix + ".isEnabled", Bool(row.IsEnabled));
                Field(payload, prefix + ".userCoordinateAccepted", Bool(row.UserCoordinateAccepted));
                Field(payload, prefix + ".activated", Bool(row.Activated));
                Field(payload, prefix + ".p2Status", Int((int)row.P2Status));
                Field(payload, prefix + ".p2Completeness", Int((int)row.P2Completeness));
                Text(payload, prefix + ".p2Canonical", row.P2CanonicalSignature);
                PressureSnapshot(payload, prefix + ".snapshot", row.PressureSnapshot);
                P2Issues(payload, prefix + ".p2Issues", row.P2Issues);
            }
        }

        private static void PressureSnapshot(StringBuilder payload, string prefix,
            LayoutPressureSnapshot value)
        {
            Field(payload, prefix + ".present", Bool(value != null));
            if (value == null) return;
            Text(payload, prefix + ".id", value.PressureInputId);
            Enums(payload, prefix + ".kinds", value.PressureKinds, item => (int)item);
            Cells(payload, prefix + ".domain", value.LayoutDomainCells);
            Cells(payload, prefix + ".usable", value.UsableCellsAfterPressure);
            Cell(payload, prefix + ".eye", value.EffectiveEyeAnchorCellAfterPressure);
            Edges(payload, prefix + ".edges", value.PreservedStructuralConnectionsAfterPressure);
            Enums(payload, prefix + ".clauses", value.RequiredPredicateClauses, item => (int)item);
        }

        private static void P2Issues(StringBuilder payload, string prefix,
            IEnumerable<AuthoredLayoutPressureSourceIssue> values)
        {
            AuthoredLayoutPressureSourceIssue[] rows = (values ?? Array.Empty<AuthoredLayoutPressureSourceIssue>())
                .OrderBy(value => value.Path, StringComparer.Ordinal)
                .ThenBy(value => value.Code, StringComparer.Ordinal)
                .ThenBy(value => (int)value.Status)
                .ThenBy(value => value.Message, StringComparer.Ordinal).ToArray();
            Field(payload, prefix + ".count", Int(rows.Length));
            for (int index = 0; index < rows.Length; index++)
            {
                string row = prefix + "[" + Int(index) + "]";
                Text(payload, row + ".path", rows[index].Path);
                Text(payload, row + ".code", rows[index].Code);
                Field(payload, row + ".status", Int((int)rows[index].Status));
                Text(payload, row + ".message", rows[index].Message);
            }
        }

        private static void ResultIssues(StringBuilder payload,
            IEnumerable<DevEncounterLayoutPressureAuthoringIssue> values)
        {
            DevEncounterLayoutPressureAuthoringIssue[] rows = (values ?? Array.Empty<DevEncounterLayoutPressureAuthoringIssue>())
                .OrderBy(value => value.Path, StringComparer.Ordinal)
                .ThenBy(value => value.Code, StringComparer.Ordinal)
                .ThenBy(value => (int)value.Status)
                .ThenBy(value => value.Message, StringComparer.Ordinal).ToArray();
            Field(payload, "result.issues.count", Int(rows.Length));
            for (int index = 0; index < rows.Length; index++)
            {
                string row = "result.issues[" + Int(index) + "]";
                Text(payload, row + ".path", rows[index].Path);
                Text(payload, row + ".code", rows[index].Code);
                Field(payload, row + ".status", Int((int)rows[index].Status));
                Text(payload, row + ".message", rows[index].Message);
            }
        }

        private static void Enums<T>(StringBuilder payload, string prefix,
            IReadOnlyList<T> values, Func<T, int> number)
        {
            Field(payload, prefix + ".present", Bool(values != null));
            if (values == null) return;
            T[] rows = values.OrderBy(number).ToArray();
            Field(payload, prefix + ".count", Int(rows.Length));
            for (int index = 0; index < rows.Length; index++)
                Field(payload, prefix + "[" + Int(index) + "]", Int(number(rows[index])));
        }

        private static void Cells(StringBuilder payload, string prefix,
            IReadOnlyList<LayoutCellCoordinate> values)
        {
            Field(payload, prefix + ".present", Bool(values != null));
            if (values == null) return;
            LayoutCellCoordinate[] rows = values
                .OrderBy(value => value == null ? int.MinValue : value.Y)
                .ThenBy(value => value == null ? int.MinValue : value.X).ToArray();
            Field(payload, prefix + ".count", Int(rows.Length));
            for (int index = 0; index < rows.Length; index++)
                Cell(payload, prefix + "[" + Int(index) + "]", rows[index]);
        }

        private static void Cell(StringBuilder payload, string prefix,
            LayoutCellCoordinate value)
        {
            Field(payload, prefix + ".present", Bool(value != null));
            if (value != null)
            {
                Field(payload, prefix + ".x", Int(value.X));
                Field(payload, prefix + ".y", Int(value.Y));
            }
        }

        private static void Edges(StringBuilder payload, string prefix,
            IReadOnlyList<LayoutCellConnection> values)
        {
            Field(payload, prefix + ".present", Bool(values != null));
            if (values == null) return;
            EdgeRow[] rows = values.Select(EdgeRow.From)
                .OrderBy(value => value, EdgeRowComparer.Instance).ToArray();
            Field(payload, prefix + ".count", Int(rows.Length));
            for (int index = 0; index < rows.Length; index++)
            {
                Cell(payload, prefix + "[" + Int(index) + "].a", rows[index].A);
                Cell(payload, prefix + "[" + Int(index) + "].b", rows[index].B);
            }
        }

        private static void Text(StringBuilder payload, string name, string value)
        {
            Field(payload, name + ".present", Bool(value != null));
            Field(payload, name, value ?? string.Empty);
        }

        private static void Field(StringBuilder payload, string name, string value)
        {
            string text = value ?? string.Empty;
            payload.Append(name).Append('=').Append(Int(text.Length)).Append(':')
                .Append(text).Append('\n');
        }

        private static string Int(int value) => value.ToString(CultureInfo.InvariantCulture);
        private static string Bool(bool value) => value ? "true" : "false";

        private readonly struct EdgeRow
        {
            public EdgeRow(LayoutCellCoordinate a, LayoutCellCoordinate b) { A = a; B = b; }
            public LayoutCellCoordinate A { get; }
            public LayoutCellCoordinate B { get; }
            public static EdgeRow From(LayoutCellConnection value)
            {
                if (value == null) return new EdgeRow(null, null);
                return Compare(value.CellA, value.CellB) <= 0
                    ? new EdgeRow(value.CellA, value.CellB)
                    : new EdgeRow(value.CellB, value.CellA);
            }
            public static int Compare(LayoutCellCoordinate left, LayoutCellCoordinate right)
            {
                if (ReferenceEquals(left, right)) return 0;
                if (left == null) return -1;
                if (right == null) return 1;
                int y = left.Y.CompareTo(right.Y);
                return y != 0 ? y : left.X.CompareTo(right.X);
            }
        }

        private sealed class EdgeRowComparer : IComparer<EdgeRow>
        {
            public static readonly EdgeRowComparer Instance = new EdgeRowComparer();
            public int Compare(EdgeRow left, EdgeRow right)
            {
                int first = EdgeRow.Compare(left.A, right.A);
                return first != 0 ? first : EdgeRow.Compare(left.B, right.B);
            }
        }
    }
}
