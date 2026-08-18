using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience;

namespace TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.AuthoredPressure
{
    public static class AuthoredLayoutPressureSourceSchema
    {
        public const string SchemaId = "AuthoredLayoutPressureSourceAdapter.v1";
        public const int SchemaVersion = 1;
    }

    public enum AuthoredLayoutPressureAuthoringCompleteness
    {
        Complete = 1,
        Incomplete = 2
    }

    public enum AuthoredLayoutPressureSourceStatus
    {
        Complete = 1,
        Unknown = 2,
        Invalid = 3
    }

    public sealed class AuthoredLayoutPressureSourceInput
    {
        private readonly ReadOnlyCollection<LayoutPressureKind> pressureKinds;
        private readonly ReadOnlyCollection<LayoutCellCoordinate> layoutDomainCells;
        private readonly ReadOnlyCollection<LayoutCellCoordinate> usableCells;
        private readonly ReadOnlyCollection<LayoutCellConnection> connections;
        private readonly ReadOnlyCollection<LayoutResiliencePredicateClauseKind> clauses;

        public AuthoredLayoutPressureSourceInput(
            AuthoredLayoutPressureAuthoringCompleteness authoringCompleteness,
            string pressureInputId,
            int? declaredBoardSize,
            IEnumerable<LayoutPressureKind> pressureKinds,
            IEnumerable<LayoutCellCoordinate> layoutDomainCells,
            IEnumerable<LayoutCellCoordinate> usableCellsAfterPressure,
            LayoutCellCoordinate effectiveEyeAnchorCellAfterPressure,
            IEnumerable<LayoutCellConnection> preservedStructuralConnectionsAfterPressure,
            IEnumerable<LayoutResiliencePredicateClauseKind> requiredPredicateClauses,
            string schemaId = AuthoredLayoutPressureSourceSchema.SchemaId,
            int schemaVersion = AuthoredLayoutPressureSourceSchema.SchemaVersion)
        {
            SchemaId = schemaId;
            SchemaVersion = schemaVersion;
            AuthoringCompleteness = authoringCompleteness;
            PressureInputId = pressureInputId;
            DeclaredBoardSize = declaredBoardSize;
            this.pressureKinds = FreezeNullable(pressureKinds, value => value);
            this.layoutDomainCells = FreezeNullable(layoutDomainCells, CloneCell);
            this.usableCells = FreezeNullable(usableCellsAfterPressure, CloneCell);
            EffectiveEyeAnchorCellAfterPressure = CloneCell(
                effectiveEyeAnchorCellAfterPressure);
            this.connections = FreezeNullable(
                preservedStructuralConnectionsAfterPressure, CloneConnection);
            this.clauses = FreezeNullable(requiredPredicateClauses, value => value);
        }

        public string SchemaId { get; }
        public int SchemaVersion { get; }
        public AuthoredLayoutPressureAuthoringCompleteness AuthoringCompleteness { get; }
        public string PressureInputId { get; }
        public int? DeclaredBoardSize { get; }
        public IReadOnlyList<LayoutPressureKind> PressureKinds => pressureKinds;
        public IReadOnlyList<LayoutCellCoordinate> LayoutDomainCells => layoutDomainCells;
        public IReadOnlyList<LayoutCellCoordinate> UsableCellsAfterPressure => usableCells;
        public LayoutCellCoordinate EffectiveEyeAnchorCellAfterPressure { get; }
        public IReadOnlyList<LayoutCellConnection>
            PreservedStructuralConnectionsAfterPressure => connections;
        public IReadOnlyList<LayoutResiliencePredicateClauseKind>
            RequiredPredicateClauses => clauses;

        private static ReadOnlyCollection<TOutput> FreezeNullable<TInput, TOutput>(
            IEnumerable<TInput> values,
            Func<TInput, TOutput> clone)
        {
            return values == null
                ? null
                : Array.AsReadOnly(values.Select(clone).ToArray());
        }

        internal static LayoutCellCoordinate CloneCell(LayoutCellCoordinate value)
        {
            return value == null ? null : new LayoutCellCoordinate(value.X, value.Y);
        }

        internal static LayoutCellConnection CloneConnection(LayoutCellConnection value)
        {
            return value == null
                ? null
                : new LayoutCellConnection(CloneCell(value.CellA), CloneCell(value.CellB));
        }
    }

    public sealed class AuthoredLayoutPressureSourceIssue
    {
        public AuthoredLayoutPressureSourceIssue(
            string code,
            AuthoredLayoutPressureSourceStatus status,
            string path,
            string message)
        {
            Code = code ?? string.Empty;
            Status = status;
            Path = path ?? string.Empty;
            Message = message ?? string.Empty;
        }

        public string Code { get; }
        public AuthoredLayoutPressureSourceStatus Status { get; }
        public string Path { get; }
        public string Message { get; }
    }

    public sealed class AuthoredLayoutPressureSourceResult
    {
        private readonly ReadOnlyCollection<AuthoredLayoutPressureSourceIssue> issues;

        internal AuthoredLayoutPressureSourceResult(
            AuthoredLayoutPressureSourceStatus status,
            LayoutResilienceInputCompleteness pressureFactsCompleteness,
            LayoutPressureSnapshot pressureSnapshot,
            IEnumerable<AuthoredLayoutPressureSourceIssue> issues,
            string canonicalSignature)
        {
            SchemaId = AuthoredLayoutPressureSourceSchema.SchemaId;
            SchemaVersion = AuthoredLayoutPressureSourceSchema.SchemaVersion;
            Status = status;
            PressureFactsCompleteness = pressureFactsCompleteness;
            PressureSnapshot = pressureSnapshot == null
                ? null
                : new LayoutPressureSnapshot(
                    pressureSnapshot.PressureInputId,
                    pressureSnapshot.PressureKinds,
                    pressureSnapshot.LayoutDomainCells,
                    pressureSnapshot.UsableCellsAfterPressure,
                    pressureSnapshot.EffectiveEyeAnchorCellAfterPressure,
                    pressureSnapshot.PreservedStructuralConnectionsAfterPressure,
                    pressureSnapshot.RequiredPredicateClauses);
            this.issues = Array.AsReadOnly((issues ??
                    Array.Empty<AuthoredLayoutPressureSourceIssue>())
                .Select(value => new AuthoredLayoutPressureSourceIssue(
                    value.Code, value.Status, value.Path, value.Message))
                .OrderBy(value => value.Path, StringComparer.Ordinal)
                .ThenBy(value => value.Code, StringComparer.Ordinal)
                .ToArray());
            CanonicalSignature = canonicalSignature ?? string.Empty;
        }

        public string SchemaId { get; }
        public int SchemaVersion { get; }
        public AuthoredLayoutPressureSourceStatus Status { get; }
        public LayoutResilienceInputCompleteness PressureFactsCompleteness { get; }
        public LayoutPressureSnapshot PressureSnapshot { get; }
        public IReadOnlyList<AuthoredLayoutPressureSourceIssue> Issues => issues;
        public string CanonicalSignature { get; }
    }

    public interface IAuthoredLayoutPressureSourceAdapter
    {
        AuthoredLayoutPressureSourceResult Project(
            AuthoredLayoutPressureSourceInput source);
    }

    internal static class AuthoredLayoutPressureCanonical
    {
        public static string Create(
            AuthoredLayoutPressureSourceInput source,
            AuthoredLayoutPressureSourceStatus status,
            LayoutResilienceInputCompleteness completeness,
            LayoutPressureSnapshot snapshot,
            IEnumerable<AuthoredLayoutPressureSourceIssue> issues)
        {
            StringBuilder builder = new StringBuilder(8192);
            Field(builder, "schemaId", AuthoredLayoutPressureSourceSchema.SchemaId);
            Field(builder, "schemaVersion", Int(AuthoredLayoutPressureSourceSchema.SchemaVersion));
            Field(builder, "source.present", Bool(source != null));
            if (source != null)
            {
                Field(builder, "source.schemaId.present", Bool(source.SchemaId != null));
                Field(builder, "source.schemaId", source.SchemaId);
                Field(builder, "source.schemaVersion", Int(source.SchemaVersion));
                Field(builder, "source.authoringCompleteness",
                    Int((int)source.AuthoringCompleteness));
                Field(builder, "source.pressureInputId.present",
                    Bool(source.PressureInputId != null));
                Field(builder, "source.pressureInputId", source.PressureInputId);
                Field(builder, "source.declaredBoardSize.present",
                    Bool(source.DeclaredBoardSize.HasValue));
                Field(builder, "source.declaredBoardSize",
                    source.DeclaredBoardSize.HasValue
                        ? Int(source.DeclaredBoardSize.Value)
                        : null);
                Enums(builder, "source.pressureKinds", source.PressureKinds,
                    value => (int)value);
                Cells(builder, "source.layoutDomainCells", source.LayoutDomainCells);
                Cells(builder, "source.usableCells", source.UsableCellsAfterPressure);
                Field(builder, "source.eye.present",
                    Bool(source.EffectiveEyeAnchorCellAfterPressure != null));
                Cell(builder, "source.eye", source.EffectiveEyeAnchorCellAfterPressure);
                Connections(builder, "source.connections",
                    source.PreservedStructuralConnectionsAfterPressure);
                Enums(builder, "source.clauses", source.RequiredPredicateClauses,
                    value => (int)value);
            }
            Field(builder, "result.status", Int((int)status));
            Field(builder, "result.completeness", Int((int)completeness));
            Snapshot(builder, snapshot);
            IssueRows(builder, issues);
            return Hash(builder.ToString());
        }

        private static void Snapshot(StringBuilder builder, LayoutPressureSnapshot value)
        {
            Field(builder, "result.snapshot.present", Bool(value != null));
            if (value == null)
            {
                return;
            }
            Field(builder, "result.snapshot.id", value.PressureInputId);
            Enums(builder, "result.snapshot.kinds", value.PressureKinds,
                item => (int)item);
            Cells(builder, "result.snapshot.domain", value.LayoutDomainCells);
            Cells(builder, "result.snapshot.usable", value.UsableCellsAfterPressure);
            Cell(builder, "result.snapshot.eye",
                value.EffectiveEyeAnchorCellAfterPressure);
            Connections(builder, "result.snapshot.connections",
                value.PreservedStructuralConnectionsAfterPressure);
            Enums(builder, "result.snapshot.clauses", value.RequiredPredicateClauses,
                item => (int)item);
        }

        private static void IssueRows(
            StringBuilder builder,
            IEnumerable<AuthoredLayoutPressureSourceIssue> values)
        {
            AuthoredLayoutPressureSourceIssue[] rows = (values ??
                    Array.Empty<AuthoredLayoutPressureSourceIssue>())
                .OrderBy(value => value.Path, StringComparer.Ordinal)
                .ThenBy(value => value.Code, StringComparer.Ordinal).ToArray();
            Field(builder, "result.issues.count", Int(rows.Length));
            for (int index = 0; index < rows.Length; index++)
            {
                string prefix = "result.issues[" + Int(index) + "]";
                Field(builder, prefix + ".code", rows[index].Code);
                Field(builder, prefix + ".status", Int((int)rows[index].Status));
                Field(builder, prefix + ".path", rows[index].Path);
                Field(builder, prefix + ".message", rows[index].Message);
            }
        }

        private static void Enums<T>(
            StringBuilder builder,
            string name,
            IReadOnlyList<T> values,
            Func<T, int> number)
        {
            Field(builder, name + ".present", Bool(values != null));
            if (values == null)
            {
                return;
            }
            T[] ordered = values.OrderBy(number).ToArray();
            Field(builder, name + ".count", Int(ordered.Length));
            for (int index = 0; index < ordered.Length; index++)
            {
                Field(builder, name + "[" + Int(index) + "]", Int(number(ordered[index])));
            }
        }

        private static void Cells(
            StringBuilder builder,
            string name,
            IReadOnlyList<LayoutCellCoordinate> values)
        {
            Field(builder, name + ".present", Bool(values != null));
            if (values == null)
            {
                return;
            }
            LayoutCellCoordinate[] ordered = values
                .OrderBy(value => value == null ? int.MinValue : value.Y)
                .ThenBy(value => value == null ? int.MinValue : value.X).ToArray();
            Field(builder, name + ".count", Int(ordered.Length));
            for (int index = 0; index < ordered.Length; index++)
            {
                Cell(builder, name + "[" + Int(index) + "]", ordered[index]);
            }
        }

        private static void Cell(
            StringBuilder builder,
            string name,
            LayoutCellCoordinate value)
        {
            Field(builder, name + ".present", Bool(value != null));
            if (value != null)
            {
                Field(builder, name + ".x", Int(value.X));
                Field(builder, name + ".y", Int(value.Y));
            }
        }

        private static void Connections(
            StringBuilder builder,
            string name,
            IReadOnlyList<LayoutCellConnection> values)
        {
            Field(builder, name + ".present", Bool(values != null));
            if (values == null)
            {
                return;
            }
            ConnectionRow[] ordered = values.Select(Normalize)
                .OrderBy(value => value, ConnectionRowComparer.Instance).ToArray();
            Field(builder, name + ".count", Int(ordered.Length));
            for (int index = 0; index < ordered.Length; index++)
            {
                Cell(builder, name + "[" + Int(index) + "].a", ordered[index].A);
                Cell(builder, name + "[" + Int(index) + "].b", ordered[index].B);
            }
        }

        internal static LayoutCellConnection NormalizeConnection(LayoutCellConnection value)
        {
            ConnectionRow row = Normalize(value);
            return value == null ? null : new LayoutCellConnection(row.A, row.B);
        }

        internal static int CompareCells(LayoutCellCoordinate left, LayoutCellCoordinate right)
        {
            if (ReferenceEquals(left, right)) return 0;
            if (left == null) return -1;
            if (right == null) return 1;
            int y = left.Y.CompareTo(right.Y);
            return y != 0 ? y : left.X.CompareTo(right.X);
        }

        internal static string CellKey(LayoutCellCoordinate value)
        {
            return value == null ? "null" : Int(value.Y) + ":" + Int(value.X);
        }

        internal static string ConnectionKey(LayoutCellConnection value)
        {
            ConnectionRow row = Normalize(value);
            return CellKey(row.A) + ">" + CellKey(row.B);
        }

        private static ConnectionRow Normalize(LayoutCellConnection value)
        {
            if (value == null)
            {
                return new ConnectionRow(null, null);
            }
            return CompareCells(value.CellA, value.CellB) <= 0
                ? new ConnectionRow(value.CellA, value.CellB)
                : new ConnectionRow(value.CellB, value.CellA);
        }

        private static void Field(StringBuilder builder, string name, string value)
        {
            string text = value ?? string.Empty;
            builder.Append(name).Append('=').Append(Int(text.Length)).Append(':')
                .Append(text).Append('\n');
        }

        private static string Int(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static string Bool(bool value)
        {
            return value ? "true" : "false";
        }

        private static string Hash(string payload)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return "sha256:" + string.Concat(sha.ComputeHash(
                        Encoding.UTF8.GetBytes(payload ?? string.Empty))
                    .Select(value => value.ToString("x2", CultureInfo.InvariantCulture)));
            }
        }

        private readonly struct ConnectionRow
        {
            public ConnectionRow(LayoutCellCoordinate a, LayoutCellCoordinate b)
            {
                A = a;
                B = b;
            }

            public LayoutCellCoordinate A { get; }
            public LayoutCellCoordinate B { get; }
        }

        private sealed class ConnectionRowComparer : IComparer<ConnectionRow>
        {
            public static readonly ConnectionRowComparer Instance =
                new ConnectionRowComparer();

            public int Compare(ConnectionRow left, ConnectionRow right)
            {
                int first = CompareCells(left.A, right.A);
                return first != 0 ? first : CompareCells(left.B, right.B);
            }
        }
    }
}
