using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.EnemySystem.RequirementChannel;

namespace TalismanBag.CrossSystem.ItemEnemy.LayoutResilience
{
    public sealed class LayoutResiliencePlacedItemFactSnapshot
    {
        private readonly ReadOnlyCollection<LayoutCellCoordinate> shapeCells;
        private readonly ReadOnlyCollection<LayoutCellCoordinate> occupiedCells;

        public LayoutResiliencePlacedItemFactSnapshot(
            string placementId,
            string itemId,
            LayoutCellCoordinate anchorCell,
            int rotation,
            IEnumerable<LayoutCellCoordinate> shapeCells,
            IEnumerable<LayoutCellCoordinate> occupiedCells,
            LayoutCellCoordinate coreCellWorld,
            bool isLit,
            bool isCountedInBuild)
        {
            PlacementId = LayoutResilienceReadOnly.Text(placementId);
            ItemId = LayoutResilienceReadOnly.Text(itemId);
            AnchorCell = anchorCell == null ? null : anchorCell.Clone();
            Rotation = rotation;
            this.shapeCells = LayoutResilienceReadOnly.FreezeCells(shapeCells);
            this.occupiedCells = LayoutResilienceReadOnly.FreezeCells(occupiedCells);
            CoreCellWorld = coreCellWorld == null ? null : coreCellWorld.Clone();
            IsLit = isLit;
            IsCountedInBuild = isCountedInBuild;
        }

        public string PlacementId { get; }
        public string ItemId { get; }
        public LayoutCellCoordinate AnchorCell { get; }
        public int Rotation { get; }
        public IReadOnlyList<LayoutCellCoordinate> ShapeCells => shapeCells;
        public IReadOnlyList<LayoutCellCoordinate> OccupiedCells => occupiedCells;
        public LayoutCellCoordinate CoreCellWorld { get; }
        public bool IsLit { get; }
        public bool IsCountedInBuild { get; }

        internal LayoutResiliencePlacedItemFactSnapshot Clone()
        {
            return new LayoutResiliencePlacedItemFactSnapshot(
                PlacementId, ItemId, AnchorCell, Rotation, ShapeCells,
                OccupiedCells, CoreCellWorld, IsLit, IsCountedInBuild);
        }
    }

    public sealed class LayoutResilienceBuildFactSnapshot
    {
        private readonly ReadOnlyCollection<LayoutResiliencePlacedItemFactSnapshot>
            placementRows;

        public LayoutResilienceBuildFactSnapshot(
            int? boardSize,
            LayoutCellCoordinate baselineEyeCell,
            IEnumerable<LayoutResiliencePlacedItemFactSnapshot> placementRows)
        {
            BoardSize = boardSize;
            BaselineEyeCell = baselineEyeCell == null ? null : baselineEyeCell.Clone();
            placementRows = placementRows ??
                Array.Empty<LayoutResiliencePlacedItemFactSnapshot>();
            this.placementRows = Array.AsReadOnly(placementRows
                .Select(value => value == null ? null : value.Clone())
                .OrderBy(value => value == null ? string.Empty : value.PlacementId,
                    StringComparer.Ordinal)
                .ToArray());
        }

        public int? BoardSize { get; }
        public LayoutCellCoordinate BaselineEyeCell { get; }
        public IReadOnlyList<LayoutResiliencePlacedItemFactSnapshot> PlacementRows =>
            placementRows;

        internal LayoutResilienceBuildFactSnapshot Clone()
        {
            return new LayoutResilienceBuildFactSnapshot(
                BoardSize, BaselineEyeCell, PlacementRows);
        }
    }

    public sealed class LayoutPressureSnapshot
    {
        private readonly ReadOnlyCollection<LayoutPressureKind> pressureKinds;
        private readonly ReadOnlyCollection<LayoutCellCoordinate> layoutDomainCells;
        private readonly ReadOnlyCollection<LayoutCellCoordinate> usableCellsAfterPressure;
        private readonly ReadOnlyCollection<LayoutCellConnection>
            preservedStructuralConnectionsAfterPressure;
        private readonly ReadOnlyCollection<LayoutResiliencePredicateClauseKind>
            requiredPredicateClauses;

        public LayoutPressureSnapshot(
            string pressureInputId,
            IEnumerable<LayoutPressureKind> pressureKinds,
            IEnumerable<LayoutCellCoordinate> layoutDomainCells,
            IEnumerable<LayoutCellCoordinate> usableCellsAfterPressure,
            LayoutCellCoordinate effectiveEyeAnchorCellAfterPressure,
            IEnumerable<LayoutCellConnection> preservedStructuralConnectionsAfterPressure,
            IEnumerable<LayoutResiliencePredicateClauseKind> requiredPredicateClauses)
        {
            PressureInputId = LayoutResilienceReadOnly.Text(pressureInputId);
            this.pressureKinds = LayoutResilienceReadOnly.Freeze(
                (pressureKinds ?? Array.Empty<LayoutPressureKind>())
                    .OrderBy(value => (int)value));
            this.layoutDomainCells = LayoutResilienceReadOnly.FreezeCells(layoutDomainCells);
            this.usableCellsAfterPressure = LayoutResilienceReadOnly.FreezeCells(
                usableCellsAfterPressure);
            EffectiveEyeAnchorCellAfterPressure = effectiveEyeAnchorCellAfterPressure == null
                ? null
                : effectiveEyeAnchorCellAfterPressure.Clone();
            this.preservedStructuralConnectionsAfterPressure =
                LayoutResilienceReadOnly.FreezeConnections(
                    preservedStructuralConnectionsAfterPressure);
            this.requiredPredicateClauses = LayoutResilienceReadOnly.Freeze(
                (requiredPredicateClauses ??
                    Array.Empty<LayoutResiliencePredicateClauseKind>())
                    .OrderBy(value => (int)value));
        }

        public string PressureInputId { get; }
        public IReadOnlyList<LayoutPressureKind> PressureKinds => pressureKinds;
        public IReadOnlyList<LayoutCellCoordinate> LayoutDomainCells => layoutDomainCells;
        public IReadOnlyList<LayoutCellCoordinate> UsableCellsAfterPressure =>
            usableCellsAfterPressure;
        public LayoutCellCoordinate EffectiveEyeAnchorCellAfterPressure { get; }
        public IReadOnlyList<LayoutCellConnection>
            PreservedStructuralConnectionsAfterPressure =>
                preservedStructuralConnectionsAfterPressure;
        public IReadOnlyList<LayoutResiliencePredicateClauseKind>
            RequiredPredicateClauses => requiredPredicateClauses;

        internal LayoutPressureSnapshot Clone()
        {
            return new LayoutPressureSnapshot(
                PressureInputId, PressureKinds, LayoutDomainCells,
                UsableCellsAfterPressure, EffectiveEyeAnchorCellAfterPressure,
                PreservedStructuralConnectionsAfterPressure, RequiredPredicateClauses);
        }
    }

    public sealed class LayoutResilienceEvaluationInput
    {
        public LayoutResilienceEvaluationInput(
            string evaluationId,
            EnemyRequirementApplicabilityState applicabilityState,
            LayoutResilienceInputCompleteness buildFactsCompleteness,
            LayoutResilienceInputCompleteness pressureFactsCompleteness,
            LayoutResilienceBuildFactSnapshot buildFacts,
            LayoutPressureSnapshot pressure,
            string schemaId = LayoutResilienceStructuralPredicateSchema.SchemaId,
            int schemaVersion = LayoutResilienceStructuralPredicateSchema.SchemaVersion)
        {
            SchemaId = LayoutResilienceReadOnly.Text(schemaId);
            SchemaVersion = schemaVersion;
            EvaluationId = LayoutResilienceReadOnly.Text(evaluationId);
            ApplicabilityState = applicabilityState;
            BuildFactsCompleteness = buildFactsCompleteness;
            PressureFactsCompleteness = pressureFactsCompleteness;
            BuildFacts = buildFacts == null ? null : buildFacts.Clone();
            Pressure = pressure == null ? null : pressure.Clone();
        }

        public string SchemaId { get; }
        public int SchemaVersion { get; }
        public string EvaluationId { get; }
        public EnemyRequirementApplicabilityState ApplicabilityState { get; }
        public LayoutResilienceInputCompleteness BuildFactsCompleteness { get; }
        public LayoutResilienceInputCompleteness PressureFactsCompleteness { get; }
        public LayoutResilienceBuildFactSnapshot BuildFacts { get; }
        public LayoutPressureSnapshot Pressure { get; }
    }

    public sealed class LayoutResiliencePredicateClauseSnapshot
    {
        public LayoutResiliencePredicateClauseSnapshot(
            LayoutResiliencePredicateClauseKind clauseKind,
            LayoutResiliencePredicateClauseState clauseState)
        {
            ClauseKind = clauseKind;
            ClauseState = clauseState;
        }

        public LayoutResiliencePredicateClauseKind ClauseKind { get; }
        public LayoutResiliencePredicateClauseState ClauseState { get; }

        internal LayoutResiliencePredicateClauseSnapshot Clone()
        {
            return new LayoutResiliencePredicateClauseSnapshot(ClauseKind, ClauseState);
        }
    }

    public sealed class LayoutResiliencePredicateResultSnapshot
    {
        private readonly ReadOnlyCollection<LayoutResiliencePredicateClauseSnapshot>
            clauseRows;

        public LayoutResiliencePredicateResultSnapshot(
            string schemaId,
            int schemaVersion,
            string evaluationId,
            EnemyRequirementApplicabilityState applicabilityState,
            LayoutResiliencePredicateState predicateState,
            IEnumerable<LayoutResiliencePredicateClauseSnapshot> clauseRows,
            string canonicalSignature)
        {
            SchemaId = LayoutResilienceReadOnly.Text(schemaId);
            SchemaVersion = schemaVersion;
            EvaluationId = LayoutResilienceReadOnly.Text(evaluationId);
            ApplicabilityState = applicabilityState;
            PredicateState = predicateState;
            this.clauseRows = Array.AsReadOnly((clauseRows ??
                    Array.Empty<LayoutResiliencePredicateClauseSnapshot>())
                .Select(value => value == null ? null : value.Clone())
                .OrderBy(value => value == null ? 0 : (int)value.ClauseKind)
                .ToArray());
            CanonicalSignature = LayoutResilienceReadOnly.Text(canonicalSignature);
        }

        public string SchemaId { get; }
        public int SchemaVersion { get; }
        public string EvaluationId { get; }
        public EnemyRequirementApplicabilityState ApplicabilityState { get; }
        public LayoutResiliencePredicateState PredicateState { get; }
        public IReadOnlyList<LayoutResiliencePredicateClauseSnapshot> ClauseRows => clauseRows;
        public string CanonicalSignature { get; }
    }

    internal static class LayoutResilienceCanonical
    {
        public static string Signature(
            LayoutResilienceEvaluationInput input,
            LayoutResiliencePredicateState state,
            IEnumerable<LayoutResiliencePredicateClauseSnapshot> clauses)
        {
            StringBuilder builder = new StringBuilder(8192);
            Field(builder, "schemaId", input.SchemaId);
            Field(builder, "schemaVersion", Integer(input.SchemaVersion));
            Field(builder, "evaluationId", input.EvaluationId);
            Field(builder, "applicabilityState", Integer((int)input.ApplicabilityState));
            Field(builder, "buildFactsCompleteness",
                Integer((int)input.BuildFactsCompleteness));
            Field(builder, "pressureFactsCompleteness",
                Integer((int)input.PressureFactsCompleteness));
            BuildFacts(builder, input.BuildFacts);
            Pressure(builder, input.Pressure);
            Field(builder, "predicateState", Integer((int)state));
            ClauseRows(builder, clauses);
            return Hash(builder.ToString());
        }

        private static void BuildFacts(
            StringBuilder builder,
            LayoutResilienceBuildFactSnapshot value)
        {
            Field(builder, "build.present", value == null ? "0" : "1");
            if (value == null)
            {
                return;
            }
            Field(builder, "build.boardSize",
                value.BoardSize.HasValue ? Integer(value.BoardSize.Value) : "<missing>");
            Field(builder, "build.baselineEyeCell", Cell(value.BaselineEyeCell));
            LayoutResiliencePlacedItemFactSnapshot[] rows = value.PlacementRows
                .OrderBy(row => row == null ? string.Empty : row.PlacementId,
                    StringComparer.Ordinal).ToArray();
            Field(builder, "build.placements.count", Integer(rows.Length));
            for (int index = 0; index < rows.Length; index++)
            {
                LayoutResiliencePlacedItemFactSnapshot row = rows[index];
                Field(builder, "build.placements[" + Integer(index) + "].present",
                    row == null ? "0" : "1");
                if (row == null)
                {
                    continue;
                }
                string prefix = "build.placements[" + Integer(index) + "].";
                Field(builder, prefix + "placementId", row.PlacementId);
                Field(builder, prefix + "itemId", row.ItemId);
                Field(builder, prefix + "anchorCell", Cell(row.AnchorCell));
                Field(builder, prefix + "rotation", Integer(row.Rotation));
                Cells(builder, prefix + "shapeCells", row.ShapeCells);
                Cells(builder, prefix + "occupiedCells", row.OccupiedCells);
                Field(builder, prefix + "coreCellWorld", Cell(row.CoreCellWorld));
                Field(builder, prefix + "isLit", row.IsLit ? "1" : "0");
                Field(builder, prefix + "isCountedInBuild",
                    row.IsCountedInBuild ? "1" : "0");
            }
        }

        private static void Pressure(StringBuilder builder, LayoutPressureSnapshot value)
        {
            Field(builder, "pressure.present", value == null ? "0" : "1");
            if (value == null)
            {
                return;
            }
            Field(builder, "pressure.pressureInputId", value.PressureInputId);
            Integers(builder, "pressure.pressureKinds",
                value.PressureKinds.Select(item => (int)item));
            Cells(builder, "pressure.layoutDomainCells", value.LayoutDomainCells);
            Cells(builder, "pressure.usableCellsAfterPressure",
                value.UsableCellsAfterPressure);
            Field(builder, "pressure.effectiveEyeAnchorCellAfterPressure",
                Cell(value.EffectiveEyeAnchorCellAfterPressure));
            LayoutCellConnection[] connections = value
                .PreservedStructuralConnectionsAfterPressure
                .OrderBy(item => item, LayoutCellConnectionComparer.Instance).ToArray();
            Field(builder, "pressure.connections.count", Integer(connections.Length));
            for (int index = 0; index < connections.Length; index++)
            {
                LayoutCellConnection connection = connections[index];
                Field(builder, "pressure.connections[" + Integer(index) + "]",
                    connection == null
                        ? "<missing>"
                        : Cell(connection.CellA) + ">" + Cell(connection.CellB));
            }
            Integers(builder, "pressure.requiredPredicateClauses",
                value.RequiredPredicateClauses.Select(item => (int)item));
        }

        private static void ClauseRows(
            StringBuilder builder,
            IEnumerable<LayoutResiliencePredicateClauseSnapshot> values)
        {
            LayoutResiliencePredicateClauseSnapshot[] rows = (values ??
                    Array.Empty<LayoutResiliencePredicateClauseSnapshot>())
                .OrderBy(value => value == null ? 0 : (int)value.ClauseKind).ToArray();
            Field(builder, "result.clauses.count", Integer(rows.Length));
            for (int index = 0; index < rows.Length; index++)
            {
                LayoutResiliencePredicateClauseSnapshot row = rows[index];
                Field(builder, "result.clauses[" + Integer(index) + "]",
                    row == null
                        ? "<missing>"
                        : Integer((int)row.ClauseKind) + ":" +
                            Integer((int)row.ClauseState));
            }
        }

        private static void Cells(
            StringBuilder builder,
            string name,
            IEnumerable<LayoutCellCoordinate> values)
        {
            LayoutCellCoordinate[] cells = (values ?? Array.Empty<LayoutCellCoordinate>())
                .OrderBy(value => value, LayoutCellCoordinateComparer.Instance).ToArray();
            Field(builder, name + ".count", Integer(cells.Length));
            for (int index = 0; index < cells.Length; index++)
            {
                Field(builder, name + "[" + Integer(index) + "]", Cell(cells[index]));
            }
        }

        private static void Integers(
            StringBuilder builder,
            string name,
            IEnumerable<int> values)
        {
            int[] items = (values ?? Array.Empty<int>()).OrderBy(value => value).ToArray();
            Field(builder, name + ".count", Integer(items.Length));
            for (int index = 0; index < items.Length; index++)
            {
                Field(builder, name + "[" + Integer(index) + "]", Integer(items[index]));
            }
        }

        private static string Cell(LayoutCellCoordinate value)
        {
            return value == null
                ? "<missing>"
                : Integer(value.X) + ":" + Integer(value.Y);
        }

        private static string Integer(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
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
