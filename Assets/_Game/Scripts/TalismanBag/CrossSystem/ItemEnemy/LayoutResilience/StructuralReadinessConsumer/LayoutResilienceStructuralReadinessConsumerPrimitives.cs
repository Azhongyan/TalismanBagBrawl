using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.EvaluationInputAssembly;

namespace TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.StructuralReadiness
{
    public static class LayoutResilienceStructuralReadinessConsumerSchema
    {
        public const string SchemaId =
            "LayoutResilienceStructuralReadinessConsumer.v1";
        public const int SchemaVersion = 1;
    }

    public enum LayoutResilienceStructuralReadinessConsumerStatus
    {
        Complete = 1,
        Unknown = 2,
        Invalid = 3
    }

    public sealed class LayoutResilienceStructuralReadinessConsumerIssue
    {
        public LayoutResilienceStructuralReadinessConsumerIssue(
            string code,
            LayoutResilienceStructuralReadinessConsumerStatus status,
            string path,
            string message)
        {
            Code = code ?? string.Empty;
            Status = status;
            Path = path ?? string.Empty;
            Message = message ?? string.Empty;
        }

        public string Code { get; }
        public LayoutResilienceStructuralReadinessConsumerStatus Status { get; }
        public string Path { get; }
        public string Message { get; }

        internal LayoutResilienceStructuralReadinessConsumerIssue Clone()
        {
            return new LayoutResilienceStructuralReadinessConsumerIssue(
                Code, Status, Path, Message);
        }
    }

    public sealed class LayoutResilienceStructuralReadinessConsumerInput
    {
        public LayoutResilienceStructuralReadinessConsumerInput(
            LayoutResilienceEvaluationInputAssemblerResult assemblerResult,
            string schemaId = LayoutResilienceStructuralReadinessConsumerSchema.SchemaId,
            int schemaVersion =
                LayoutResilienceStructuralReadinessConsumerSchema.SchemaVersion)
        {
            SchemaId = schemaId;
            SchemaVersion = schemaVersion;
            AssemblerResult = assemblerResult;
        }

        public string SchemaId { get; }
        public int SchemaVersion { get; }
        public LayoutResilienceEvaluationInputAssemblerResult AssemblerResult { get; }
    }

    public sealed class LayoutResilienceStructuralReadinessConsumerResult
    {
        private readonly ReadOnlyCollection<
            LayoutResilienceStructuralReadinessConsumerIssue> issues;

        internal LayoutResilienceStructuralReadinessConsumerResult(
            LayoutResilienceStructuralReadinessConsumerStatus status,
            LayoutResiliencePredicateResultSnapshot predicateResult,
            IEnumerable<LayoutResilienceStructuralReadinessConsumerIssue> issues,
            string canonicalSignature)
        {
            SchemaId = LayoutResilienceStructuralReadinessConsumerSchema.SchemaId;
            SchemaVersion = LayoutResilienceStructuralReadinessConsumerSchema.SchemaVersion;
            Status = status;
            PredicateResult = LayoutResilienceStructuralReadinessConsumerReadOnly
                .ClonePredicateResult(predicateResult);
            this.issues = Array.AsReadOnly((issues ??
                    Array.Empty<LayoutResilienceStructuralReadinessConsumerIssue>())
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
        public LayoutResilienceStructuralReadinessConsumerStatus Status { get; }
        public LayoutResiliencePredicateResultSnapshot PredicateResult { get; }
        public IReadOnlyList<LayoutResilienceStructuralReadinessConsumerIssue> Issues =>
            issues;
        public string CanonicalSignature { get; }
    }

    public interface ILayoutResilienceStructuralReadinessConsumer
    {
        LayoutResilienceStructuralReadinessConsumerResult Consume(
            LayoutResilienceStructuralReadinessConsumerInput input);
    }

    internal static class LayoutResilienceStructuralReadinessConsumerReadOnly
    {
        public static LayoutResiliencePredicateResultSnapshot ClonePredicateResult(
            LayoutResiliencePredicateResultSnapshot value)
        {
            return value == null
                ? null
                : new LayoutResiliencePredicateResultSnapshot(
                    value.SchemaId,
                    value.SchemaVersion,
                    value.EvaluationId,
                    value.ApplicabilityState,
                    value.PredicateState,
                    value.ClauseRows,
                    value.CanonicalSignature);
        }
    }

    internal static class LayoutResilienceStructuralReadinessConsumerCanonical
    {
        public static string Create(
            LayoutResilienceStructuralReadinessConsumerInput source,
            LayoutResilienceStructuralReadinessConsumerStatus status,
            LayoutResiliencePredicateResultSnapshot predicateResult,
            IEnumerable<LayoutResilienceStructuralReadinessConsumerIssue> issues)
        {
            StringBuilder builder = new StringBuilder(32768);
            Field(builder, "consumer.schemaId",
                LayoutResilienceStructuralReadinessConsumerSchema.SchemaId);
            Field(builder, "consumer.schemaVersion", Integer(
                LayoutResilienceStructuralReadinessConsumerSchema.SchemaVersion));
            Field(builder, "input.present", Boolean(source != null));
            if (source != null)
            {
                Field(builder, "input.schemaId.present", Boolean(source.SchemaId != null));
                Field(builder, "input.schemaId", source.SchemaId);
                Field(builder, "input.schemaVersion", Integer(source.SchemaVersion));
                AssemblerResult(builder, source.AssemblerResult);
            }
            Field(builder, "result.status", Integer((int)status));
            PredicateResult(builder, predicateResult);
            IssueRows(builder, issues);
            return Hash(builder.ToString());
        }

        private static void AssemblerResult(
            StringBuilder builder,
            LayoutResilienceEvaluationInputAssemblerResult value)
        {
            Field(builder, "p3.present", Boolean(value != null));
            if (value == null)
            {
                return;
            }
            Field(builder, "p3.schemaId.present", Boolean(value.SchemaId != null));
            Field(builder, "p3.schemaId", value.SchemaId);
            Field(builder, "p3.schemaVersion", Integer(value.SchemaVersion));
            Field(builder, "p3.status", Integer((int)value.Status));
            Field(builder, "p3.canonicalSignature.present",
                Boolean(value.CanonicalSignature != null));
            Field(builder, "p3.canonicalSignature", value.CanonicalSignature);
            AssemblerIssues(builder, value.Issues);
            EvaluationInput(builder, value.EvaluationInput);
        }

        private static void AssemblerIssues(
            StringBuilder builder,
            IEnumerable<LayoutResilienceEvaluationInputAssemblerIssue> values)
        {
            LayoutResilienceEvaluationInputAssemblerIssue[] rows = (values ??
                    Array.Empty<LayoutResilienceEvaluationInputAssemblerIssue>())
                .OrderBy(value => value == null ? string.Empty : value.Path,
                    StringComparer.Ordinal)
                .ThenBy(value => value == null ? string.Empty : value.Code,
                    StringComparer.Ordinal)
                .ThenBy(value => value == null ? string.Empty : value.Message,
                    StringComparer.Ordinal)
                .ToArray();
            Field(builder, "p3.issues.count", Integer(rows.Length));
            for (int index = 0; index < rows.Length; index++)
            {
                string prefix = "p3.issues[" + Integer(index) + "].";
                LayoutResilienceEvaluationInputAssemblerIssue row = rows[index];
                Field(builder, prefix + "present", Boolean(row != null));
                if (row == null)
                {
                    continue;
                }
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
            Field(builder, "p3.evaluationInput.present", Boolean(value != null));
            if (value == null)
            {
                return;
            }
            Field(builder, "p3.evaluationInput.schemaId.present",
                Boolean(value.SchemaId != null));
            Field(builder, "p3.evaluationInput.schemaId", value.SchemaId);
            Field(builder, "p3.evaluationInput.schemaVersion",
                Integer(value.SchemaVersion));
            Field(builder, "p3.evaluationInput.evaluationId.present",
                Boolean(value.EvaluationId != null));
            Field(builder, "p3.evaluationInput.evaluationId", value.EvaluationId);
            Field(builder, "p3.evaluationInput.applicabilityState",
                Integer((int)value.ApplicabilityState));
            Field(builder, "p3.evaluationInput.buildFactsCompleteness",
                Integer((int)value.BuildFactsCompleteness));
            Field(builder, "p3.evaluationInput.pressureFactsCompleteness",
                Integer((int)value.PressureFactsCompleteness));
            BuildFacts(builder, value.BuildFacts);
            Pressure(builder, value.Pressure);
        }

        private static void BuildFacts(
            StringBuilder builder,
            LayoutResilienceBuildFactSnapshot value)
        {
            Field(builder, "p3.evaluationInput.build.present", Boolean(value != null));
            if (value == null)
            {
                return;
            }
            Field(builder, "p3.evaluationInput.build.boardSize.present",
                Boolean(value.BoardSize.HasValue));
            Field(builder, "p3.evaluationInput.build.boardSize",
                value.BoardSize.HasValue ? Integer(value.BoardSize.Value) : string.Empty);
            Cell(builder, "p3.evaluationInput.build.baselineEyeCell",
                value.BaselineEyeCell);
            LayoutResiliencePlacedItemFactSnapshot[] rows = value.PlacementRows
                .OrderBy(item => item == null ? string.Empty : item.PlacementId,
                    StringComparer.Ordinal).ToArray();
            Field(builder, "p3.evaluationInput.build.placements.count",
                Integer(rows.Length));
            for (int index = 0; index < rows.Length; index++)
            {
                LayoutResiliencePlacedItemFactSnapshot row = rows[index];
                string prefix = "p3.evaluationInput.build.placements[" +
                    Integer(index) + "].";
                Field(builder, prefix + "present", Boolean(row != null));
                if (row == null)
                {
                    continue;
                }
                Field(builder, prefix + "placementId", row.PlacementId);
                Field(builder, prefix + "itemId", row.ItemId);
                Cell(builder, prefix + "anchorCell", row.AnchorCell);
                Field(builder, prefix + "rotation", Integer(row.Rotation));
                Cells(builder, prefix + "shapeCells", row.ShapeCells);
                Cells(builder, prefix + "occupiedCells", row.OccupiedCells);
                Cell(builder, prefix + "coreCellWorld", row.CoreCellWorld);
                Field(builder, prefix + "isLit", Boolean(row.IsLit));
                Field(builder, prefix + "isCountedInBuild",
                    Boolean(row.IsCountedInBuild));
            }
        }

        private static void Pressure(
            StringBuilder builder,
            LayoutPressureSnapshot value)
        {
            Field(builder, "p3.evaluationInput.pressure.present", Boolean(value != null));
            if (value == null)
            {
                return;
            }
            Field(builder, "p3.evaluationInput.pressure.pressureInputId.present",
                Boolean(value.PressureInputId != null));
            Field(builder, "p3.evaluationInput.pressure.pressureInputId",
                value.PressureInputId);
            Integers(builder, "p3.evaluationInput.pressure.pressureKinds",
                value.PressureKinds.Select(item => (int)item));
            Cells(builder, "p3.evaluationInput.pressure.layoutDomainCells",
                value.LayoutDomainCells);
            Cells(builder, "p3.evaluationInput.pressure.usableCellsAfterPressure",
                value.UsableCellsAfterPressure);
            Cell(builder,
                "p3.evaluationInput.pressure.effectiveEyeAnchorCellAfterPressure",
                value.EffectiveEyeAnchorCellAfterPressure);
            Connections(builder,
                "p3.evaluationInput.pressure.preservedStructuralConnections",
                value.PreservedStructuralConnectionsAfterPressure);
            Integers(builder, "p3.evaluationInput.pressure.requiredPredicateClauses",
                value.RequiredPredicateClauses.Select(item => (int)item));
        }

        private static void PredicateResult(
            StringBuilder builder,
            LayoutResiliencePredicateResultSnapshot value)
        {
            Field(builder, "result.predicateResult.present", Boolean(value != null));
            if (value == null)
            {
                return;
            }
            Field(builder, "result.predicateResult.schemaId.present",
                Boolean(value.SchemaId != null));
            Field(builder, "result.predicateResult.schemaId", value.SchemaId);
            Field(builder, "result.predicateResult.schemaVersion",
                Integer(value.SchemaVersion));
            Field(builder, "result.predicateResult.evaluationId.present",
                Boolean(value.EvaluationId != null));
            Field(builder, "result.predicateResult.evaluationId", value.EvaluationId);
            Field(builder, "result.predicateResult.applicabilityState",
                Integer((int)value.ApplicabilityState));
            Field(builder, "result.predicateResult.predicateState",
                Integer((int)value.PredicateState));
            LayoutResiliencePredicateClauseSnapshot[] rows = value.ClauseRows
                .OrderBy(item => item == null ? 0 : (int)item.ClauseKind).ToArray();
            Field(builder, "result.predicateResult.clauses.count",
                Integer(rows.Length));
            for (int index = 0; index < rows.Length; index++)
            {
                LayoutResiliencePredicateClauseSnapshot row = rows[index];
                string prefix = "result.predicateResult.clauses[" +
                    Integer(index) + "].";
                Field(builder, prefix + "present", Boolean(row != null));
                if (row != null)
                {
                    Field(builder, prefix + "clauseKind", Integer((int)row.ClauseKind));
                    Field(builder, prefix + "clauseState",
                        Integer((int)row.ClauseState));
                }
            }
            Field(builder, "result.predicateResult.canonicalSignature.present",
                Boolean(value.CanonicalSignature != null));
            Field(builder, "result.predicateResult.canonicalSignature",
                value.CanonicalSignature);
        }

        private static void IssueRows(
            StringBuilder builder,
            IEnumerable<LayoutResilienceStructuralReadinessConsumerIssue> values)
        {
            LayoutResilienceStructuralReadinessConsumerIssue[] rows = (values ??
                    Array.Empty<LayoutResilienceStructuralReadinessConsumerIssue>())
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

        private static void Connections(
            StringBuilder builder,
            string name,
            IEnumerable<LayoutCellConnection> values)
        {
            LayoutCellConnection[] rows = (values ??
                Array.Empty<LayoutCellConnection>()).ToArray();
            Array.Sort(rows, CompareConnections);
            Field(builder, name + ".count", Integer(rows.Length));
            for (int index = 0; index < rows.Length; index++)
            {
                Field(builder, name + "[" + Integer(index) + "]",
                    ConnectionText(rows[index]));
            }
        }

        private static int CompareConnections(
            LayoutCellConnection left,
            LayoutCellConnection right)
        {
            if (ReferenceEquals(left, right))
            {
                return 0;
            }
            if (left == null)
            {
                return -1;
            }
            if (right == null)
            {
                return 1;
            }
            LayoutCellCoordinate leftFirst;
            LayoutCellCoordinate leftSecond;
            NormalizeEndpoints(left, out leftFirst, out leftSecond);
            LayoutCellCoordinate rightFirst;
            LayoutCellCoordinate rightSecond;
            NormalizeEndpoints(right, out rightFirst, out rightSecond);
            int first = CompareCells(leftFirst, rightFirst);
            return first != 0 ? first : CompareCells(leftSecond, rightSecond);
        }

        private static string ConnectionText(LayoutCellConnection value)
        {
            if (value == null)
            {
                return "<null>";
            }
            LayoutCellCoordinate first;
            LayoutCellCoordinate second;
            NormalizeEndpoints(value, out first, out second);
            return CellText(first) + ">" + CellText(second);
        }

        private static void NormalizeEndpoints(
            LayoutCellConnection value,
            out LayoutCellCoordinate first,
            out LayoutCellCoordinate second)
        {
            first = value == null ? null : value.CellA;
            second = value == null ? null : value.CellB;
            if (CompareCells(first, second) > 0)
            {
                LayoutCellCoordinate temporary = first;
                first = second;
                second = temporary;
            }
        }

        private static int CompareCells(
            LayoutCellCoordinate left,
            LayoutCellCoordinate right)
        {
            if (ReferenceEquals(left, right))
            {
                return 0;
            }
            if (left == null)
            {
                return -1;
            }
            if (right == null)
            {
                return 1;
            }
            int y = left.Y.CompareTo(right.Y);
            return y != 0 ? y : left.X.CompareTo(right.X);
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

        private static string Hash(string payload)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return "sha256:" + string.Concat(sha.ComputeHash(
                        Encoding.UTF8.GetBytes(payload ?? string.Empty))
                    .Select(value => value.ToString("x2", CultureInfo.InvariantCulture)));
            }
        }
    }
}
