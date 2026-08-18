using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.AuthoredPressure;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.ItemFactProjection;
using TalismanBag.EnemySystem.RequirementChannel;

namespace TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.EvaluationInputAssembly
{
    public static class LayoutResilienceEvaluationInputAssemblerSchema
    {
        public const string SchemaId = "LayoutResilienceEvaluationInputAssembler.v1";
        public const int SchemaVersion = 1;
    }

    public enum LayoutResilienceEvaluationInputAssemblerStatus
    {
        Complete = 1,
        Unknown = 2,
        Invalid = 3
    }

    public sealed class LayoutResilienceEvaluationInputAssemblerIssue
    {
        public LayoutResilienceEvaluationInputAssemblerIssue(
            string code,
            LayoutResilienceEvaluationInputAssemblerStatus status,
            string path,
            string message)
        {
            Code = code ?? string.Empty;
            Status = status;
            Path = path ?? string.Empty;
            Message = message ?? string.Empty;
        }

        public string Code { get; }
        public LayoutResilienceEvaluationInputAssemblerStatus Status { get; }
        public string Path { get; }
        public string Message { get; }

        internal LayoutResilienceEvaluationInputAssemblerIssue Clone()
        {
            return new LayoutResilienceEvaluationInputAssemblerIssue(
                Code, Status, Path, Message);
        }
    }

    public sealed class LayoutResilienceEvaluationInputAssemblerInput
    {
        public LayoutResilienceEvaluationInputAssemblerInput(
            string evaluationId,
            string requirementId,
            EnemyRequirementChannelApplicabilitySnapshot applicabilitySnapshot,
            LayoutResilienceItemFactProjectionResult buildProjectionResult,
            AuthoredLayoutPressureSourceResult pressureSourceResult,
            string schemaId = LayoutResilienceEvaluationInputAssemblerSchema.SchemaId,
            int schemaVersion = LayoutResilienceEvaluationInputAssemblerSchema.SchemaVersion)
        {
            SchemaId = schemaId;
            SchemaVersion = schemaVersion;
            EvaluationId = evaluationId;
            RequirementId = requirementId;
            ApplicabilitySnapshot = applicabilitySnapshot;
            BuildProjectionResult = buildProjectionResult;
            PressureSourceResult = pressureSourceResult;
        }

        public string SchemaId { get; }
        public int SchemaVersion { get; }
        public string EvaluationId { get; }
        public string RequirementId { get; }
        public EnemyRequirementChannelApplicabilitySnapshot ApplicabilitySnapshot { get; }
        public LayoutResilienceItemFactProjectionResult BuildProjectionResult { get; }
        public AuthoredLayoutPressureSourceResult PressureSourceResult { get; }
    }

    public sealed class LayoutResilienceEvaluationInputAssemblerResult
    {
        private readonly ReadOnlyCollection<LayoutResilienceEvaluationInputAssemblerIssue>
            issues;

        internal LayoutResilienceEvaluationInputAssemblerResult(
            LayoutResilienceEvaluationInputAssemblerStatus status,
            LayoutResilienceEvaluationInput evaluationInput,
            IEnumerable<LayoutResilienceEvaluationInputAssemblerIssue> issues,
            string canonicalSignature)
        {
            SchemaId = LayoutResilienceEvaluationInputAssemblerSchema.SchemaId;
            SchemaVersion = LayoutResilienceEvaluationInputAssemblerSchema.SchemaVersion;
            Status = status;
            EvaluationInput = LayoutResilienceEvaluationInputAssemblerReadOnly.CloneInput(
                evaluationInput);
            this.issues = Array.AsReadOnly((issues ??
                    Array.Empty<LayoutResilienceEvaluationInputAssemblerIssue>())
                .Where(value => value != null)
                .Select(value => value.Clone())
                .OrderBy(value => value.Path, StringComparer.Ordinal)
                .ThenBy(value => value.Code, StringComparer.Ordinal)
                .ThenBy(value => value.Message, StringComparer.Ordinal)
                .ToArray());
            CanonicalSignature = canonicalSignature ?? string.Empty;
        }

        public string SchemaId { get; }
        public int SchemaVersion { get; }
        public LayoutResilienceEvaluationInputAssemblerStatus Status { get; }
        public LayoutResilienceEvaluationInput EvaluationInput { get; }
        public IReadOnlyList<LayoutResilienceEvaluationInputAssemblerIssue> Issues => issues;
        public string CanonicalSignature { get; }
    }

    public interface ILayoutResilienceEvaluationInputAssembler
    {
        LayoutResilienceEvaluationInputAssemblerResult Assemble(
            LayoutResilienceEvaluationInputAssemblerInput input);
    }

    internal static class LayoutResilienceEvaluationInputAssemblerReadOnly
    {
        public static LayoutResilienceEvaluationInput CloneInput(
            LayoutResilienceEvaluationInput value)
        {
            return value == null
                ? null
                : new LayoutResilienceEvaluationInput(
                    value.EvaluationId,
                    value.ApplicabilityState,
                    value.BuildFactsCompleteness,
                    value.PressureFactsCompleteness,
                    value.BuildFacts,
                    value.Pressure,
                    value.SchemaId,
                    value.SchemaVersion);
        }
    }

    internal static class LayoutResilienceEvaluationInputAssemblerCanonical
    {
        public static string Create(
            LayoutResilienceEvaluationInputAssemblerInput source,
            EnemyRequirementChannelApplicabilityRowSnapshot selectedRow,
            LayoutResilienceEvaluationInputAssemblerStatus status,
            LayoutResilienceEvaluationInput output,
            IEnumerable<LayoutResilienceEvaluationInputAssemblerIssue> issues)
        {
            StringBuilder builder = new StringBuilder(32768);
            Field(builder, "assembler.schemaId",
                LayoutResilienceEvaluationInputAssemblerSchema.SchemaId);
            Field(builder, "assembler.schemaVersion", Integer(
                LayoutResilienceEvaluationInputAssemblerSchema.SchemaVersion));
            Field(builder, "input.present", Boolean(source != null));
            if (source != null)
            {
                Field(builder, "input.schemaId.present", Boolean(source.SchemaId != null));
                Field(builder, "input.schemaId", source.SchemaId);
                Field(builder, "input.schemaVersion", Integer(source.SchemaVersion));
                Field(builder, "input.evaluationId.present",
                    Boolean(source.EvaluationId != null));
                Field(builder, "input.evaluationId", source.EvaluationId);
                Field(builder, "input.requirementId.present",
                    Boolean(source.RequirementId != null));
                Field(builder, "input.requirementId", source.RequirementId);
                Applicability(builder, source.ApplicabilitySnapshot, selectedRow);
                BuildSource(builder, source.BuildProjectionResult);
                PressureSource(builder, source.PressureSourceResult);
            }

            Field(builder, "result.status", Integer((int)status));
            EvaluationInput(builder, output);
            IssueRows(builder, issues);
            return Hash(builder.ToString());
        }

        private static void Applicability(
            StringBuilder builder,
            EnemyRequirementChannelApplicabilitySnapshot value,
            EnemyRequirementChannelApplicabilityRowSnapshot selectedRow)
        {
            Field(builder, "n01b.present", Boolean(value != null));
            if (value != null)
            {
                Field(builder, "n01b.schemaId", value.SchemaId);
                Field(builder, "n01b.schemaVersion", Integer(value.SchemaVersion));
                Field(builder, "n01b.snapshotId", value.SnapshotId);
                Field(builder, "n01b.evaluationChannel", Integer(
                    (int)value.EvaluationChannel));
                Field(builder, "n01b.canonicalSignature", value.CanonicalSignature);
            }
            Field(builder, "n01b.selected.present", Boolean(selectedRow != null));
            if (selectedRow != null)
            {
                Field(builder, "n01b.selected.requirementId", selectedRow.RequirementId);
                Field(builder, "n01b.selected.declaredChannel", Integer(
                    (int)selectedRow.DeclaredChannel));
                Field(builder, "n01b.selected.applicabilityState", Integer(
                    (int)selectedRow.ApplicabilityState));
            }
        }

        private static void BuildSource(
            StringBuilder builder,
            LayoutResilienceItemFactProjectionResult value)
        {
            Field(builder, "p1.present", Boolean(value != null));
            if (value == null)
            {
                return;
            }
            Field(builder, "p1.schemaId", value.SchemaId);
            Field(builder, "p1.schemaVersion", Integer(value.SchemaVersion));
            Field(builder, "p1.status", Integer((int)value.Status));
            Field(builder, "p1.completeness", Integer((int)value.BuildFactsCompleteness));
            Field(builder, "p1.canonicalSignature", value.CanonicalSignature);
            BuildFacts(builder, "p1.build", value.BuildFacts);
            Field(builder, "p1.issues.count", Integer(value.Issues.Count));
            LayoutResilienceItemFactProjectionIssue[] rows = value.Issues
                .OrderBy(item => item.Code, StringComparer.Ordinal)
                .ThenBy(item => item.ItemInstanceId, StringComparer.Ordinal)
                .ThenBy(item => item.PlacementId, StringComparer.Ordinal)
                .ThenBy(item => item.ItemId, StringComparer.Ordinal)
                .ThenBy(item => item.Message, StringComparer.Ordinal)
                .ToArray();
            for (int index = 0; index < rows.Length; index++)
            {
                LayoutResilienceItemFactProjectionIssue row = rows[index];
                string prefix = "p1.issues[" + Integer(index) + "].";
                Field(builder, prefix + "code", row.Code);
                Field(builder, prefix + "status", Integer((int)row.Status));
                Field(builder, prefix + "itemInstanceId", row.ItemInstanceId);
                Field(builder, prefix + "placementId", row.PlacementId);
                Field(builder, prefix + "itemId", row.ItemId);
                Field(builder, prefix + "message", row.Message);
            }
        }

        private static void PressureSource(
            StringBuilder builder,
            AuthoredLayoutPressureSourceResult value)
        {
            Field(builder, "p2.present", Boolean(value != null));
            if (value == null)
            {
                return;
            }
            Field(builder, "p2.schemaId", value.SchemaId);
            Field(builder, "p2.schemaVersion", Integer(value.SchemaVersion));
            Field(builder, "p2.status", Integer((int)value.Status));
            Field(builder, "p2.completeness", Integer((int)value.PressureFactsCompleteness));
            Field(builder, "p2.canonicalSignature", value.CanonicalSignature);
            Pressure(builder, "p2.pressure", value.PressureSnapshot);
            AuthoredLayoutPressureSourceIssue[] rows = value.Issues
                .OrderBy(item => item.Path, StringComparer.Ordinal)
                .ThenBy(item => item.Code, StringComparer.Ordinal)
                .ThenBy(item => item.Message, StringComparer.Ordinal)
                .ToArray();
            Field(builder, "p2.issues.count", Integer(rows.Length));
            for (int index = 0; index < rows.Length; index++)
            {
                AuthoredLayoutPressureSourceIssue row = rows[index];
                string prefix = "p2.issues[" + Integer(index) + "].";
                Field(builder, prefix + "code", row.Code);
                Field(builder, prefix + "status", Integer((int)row.Status));
                Field(builder, prefix + "path", row.Path);
                Field(builder, prefix + "message", row.Message);
            }
        }

        private static void EvaluationInput(
            StringBuilder builder,
            LayoutResilienceEvaluationInput value)
        {
            Field(builder, "output.present", Boolean(value != null));
            if (value == null)
            {
                return;
            }
            Field(builder, "output.schemaId", value.SchemaId);
            Field(builder, "output.schemaVersion", Integer(value.SchemaVersion));
            Field(builder, "output.evaluationId", value.EvaluationId);
            Field(builder, "output.applicabilityState", Integer(
                (int)value.ApplicabilityState));
            Field(builder, "output.buildCompleteness", Integer(
                (int)value.BuildFactsCompleteness));
            Field(builder, "output.pressureCompleteness", Integer(
                (int)value.PressureFactsCompleteness));
            BuildFacts(builder, "output.build", value.BuildFacts);
            Pressure(builder, "output.pressure", value.Pressure);
        }

        private static void BuildFacts(
            StringBuilder builder,
            string prefix,
            LayoutResilienceBuildFactSnapshot value)
        {
            Field(builder, prefix + ".present", Boolean(value != null));
            if (value == null)
            {
                return;
            }
            Field(builder, prefix + ".boardSize.present", Boolean(value.BoardSize.HasValue));
            Field(builder, prefix + ".boardSize",
                value.BoardSize.HasValue ? Integer(value.BoardSize.Value) : string.Empty);
            Cell(builder, prefix + ".baselineEye", value.BaselineEyeCell);
            LayoutResiliencePlacedItemFactSnapshot[] rows = value.PlacementRows
                .OrderBy(item => item == null ? string.Empty : item.PlacementId,
                    StringComparer.Ordinal).ToArray();
            Field(builder, prefix + ".placements.count", Integer(rows.Length));
            for (int index = 0; index < rows.Length; index++)
            {
                LayoutResiliencePlacedItemFactSnapshot row = rows[index];
                string rowPrefix = prefix + ".placements[" + Integer(index) + "].";
                Field(builder, rowPrefix + "present", Boolean(row != null));
                if (row == null)
                {
                    continue;
                }
                Field(builder, rowPrefix + "placementId", row.PlacementId);
                Field(builder, rowPrefix + "itemId", row.ItemId);
                Cell(builder, rowPrefix + "anchor", row.AnchorCell);
                Field(builder, rowPrefix + "rotation", Integer(row.Rotation));
                Cells(builder, rowPrefix + "shape", row.ShapeCells);
                Cells(builder, rowPrefix + "occupied", row.OccupiedCells);
                Cell(builder, rowPrefix + "core", row.CoreCellWorld);
                Field(builder, rowPrefix + "isLit", Boolean(row.IsLit));
                Field(builder, rowPrefix + "isCountedInBuild",
                    Boolean(row.IsCountedInBuild));
            }
        }

        private static void Pressure(
            StringBuilder builder,
            string prefix,
            LayoutPressureSnapshot value)
        {
            Field(builder, prefix + ".present", Boolean(value != null));
            if (value == null)
            {
                return;
            }
            Field(builder, prefix + ".pressureInputId", value.PressureInputId);
            Integers(builder, prefix + ".kinds",
                value.PressureKinds.Select(item => (int)item));
            Cells(builder, prefix + ".domain", value.LayoutDomainCells);
            Cells(builder, prefix + ".usable", value.UsableCellsAfterPressure);
            Cell(builder, prefix + ".effectiveEye",
                value.EffectiveEyeAnchorCellAfterPressure);
            LayoutCellConnection[] connections = value
                .PreservedStructuralConnectionsAfterPressure
                .OrderBy(ConnectionKey, StringComparer.Ordinal).ToArray();
            Field(builder, prefix + ".connections.count", Integer(connections.Length));
            for (int index = 0; index < connections.Length; index++)
            {
                LayoutCellConnection connection = connections[index];
                Field(builder, prefix + ".connections[" + Integer(index) + "]",
                    connection == null
                        ? "<null>"
                        : CellText(connection.CellA) + ">" + CellText(connection.CellB));
            }
            Integers(builder, prefix + ".clauses",
                value.RequiredPredicateClauses.Select(item => (int)item));
        }

        private static void IssueRows(
            StringBuilder builder,
            IEnumerable<LayoutResilienceEvaluationInputAssemblerIssue> values)
        {
            LayoutResilienceEvaluationInputAssemblerIssue[] rows = (values ??
                    Array.Empty<LayoutResilienceEvaluationInputAssemblerIssue>())
                .Where(value => value != null)
                .OrderBy(value => value.Path, StringComparer.Ordinal)
                .ThenBy(value => value.Code, StringComparer.Ordinal)
                .ThenBy(value => value.Message, StringComparer.Ordinal)
                .ToArray();
            Field(builder, "result.issues.count", Integer(rows.Length));
            for (int index = 0; index < rows.Length; index++)
            {
                string prefix = "result.issues[" + Integer(index) + "].";
                Field(builder, prefix + "code", rows[index].Code);
                Field(builder, prefix + "status", Integer((int)rows[index].Status));
                Field(builder, prefix + "path", rows[index].Path);
                Field(builder, prefix + "message", rows[index].Message);
            }
        }

        private static void Cells(
            StringBuilder builder,
            string name,
            IEnumerable<LayoutCellCoordinate> values)
        {
            LayoutCellCoordinate[] rows = (values ?? Array.Empty<LayoutCellCoordinate>())
                .OrderBy(value => value == null ? int.MinValue : value.Y)
                .ThenBy(value => value == null ? int.MinValue : value.X)
                .ToArray();
            Field(builder, name + ".count", Integer(rows.Length));
            for (int index = 0; index < rows.Length; index++)
            {
                Cell(builder, name + "[" + Integer(index) + "]", rows[index]);
            }
        }

        private static void Integers(
            StringBuilder builder,
            string name,
            IEnumerable<int> values)
        {
            int[] rows = (values ?? Array.Empty<int>()).OrderBy(value => value).ToArray();
            Field(builder, name + ".count", Integer(rows.Length));
            for (int index = 0; index < rows.Length; index++)
            {
                Field(builder, name + "[" + Integer(index) + "]", Integer(rows[index]));
            }
        }

        private static string ConnectionKey(LayoutCellConnection value)
        {
            return value == null
                ? string.Empty
                : CellText(value.CellA) + ">" + CellText(value.CellB);
        }

        private static void Cell(
            StringBuilder builder,
            string name,
            LayoutCellCoordinate value)
        {
            Field(builder, name + ".present", Boolean(value != null));
            Field(builder, name, CellText(value));
        }

        private static string CellText(LayoutCellCoordinate value)
        {
            return value == null
                ? string.Empty
                : Integer(value.X) + ":" + Integer(value.Y);
        }

        private static string Integer(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static string Boolean(bool value)
        {
            return value ? "true" : "false";
        }

        private static void Field(StringBuilder builder, string name, string value)
        {
            string safeName = name ?? string.Empty;
            string safeValue = value ?? string.Empty;
            builder.Append(Integer(safeName.Length)).Append(':').Append(safeName)
                .Append('=').Append(Integer(safeValue.Length)).Append(':')
                .Append(safeValue).Append(';');
        }

        private static string Hash(string value)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return "sha256:" + string.Concat(sha.ComputeHash(
                        Encoding.UTF8.GetBytes(value ?? string.Empty))
                    .Select(item => item.ToString("x2", CultureInfo.InvariantCulture)));
            }
        }
    }
}
