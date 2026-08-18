using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.Items;
using TalismanBag.Items.Capability;

namespace TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.ItemFactProjection
{
    public static class LayoutResilienceItemFactProjectionSchema
    {
        public const string SchemaId = "LayoutResilienceItemFactProjectionAdapter.v1";
        public const int SchemaVersion = 1;
    }

    public enum LayoutResilienceItemFactProjectionStatus
    {
        Complete = 1,
        Unknown = 2,
        Invalid = 3
    }

    public sealed class LayoutResilienceItemFactProjectionIssue
    {
        public LayoutResilienceItemFactProjectionIssue(
            string code,
            LayoutResilienceItemFactProjectionStatus status,
            string itemInstanceId,
            string placementId,
            string itemId,
            string message)
        {
            Code = code ?? string.Empty;
            Status = status;
            ItemInstanceId = itemInstanceId ?? string.Empty;
            PlacementId = placementId ?? string.Empty;
            ItemId = itemId ?? string.Empty;
            Message = message ?? string.Empty;
        }

        public string Code { get; }
        public LayoutResilienceItemFactProjectionStatus Status { get; }
        public string ItemInstanceId { get; }
        public string PlacementId { get; }
        public string ItemId { get; }
        public string Message { get; }

        internal LayoutResilienceItemFactProjectionIssue Clone()
        {
            return new LayoutResilienceItemFactProjectionIssue(
                Code, Status, ItemInstanceId, PlacementId, ItemId, Message);
        }
    }

    public sealed class LayoutResilienceItemFactProjectionResult
    {
        private readonly ReadOnlyCollection<LayoutResilienceItemFactProjectionIssue>
            issues;

        internal LayoutResilienceItemFactProjectionResult(
            LayoutResilienceItemFactProjectionStatus status,
            LayoutResilienceInputCompleteness buildFactsCompleteness,
            LayoutResilienceBuildFactSnapshot buildFacts,
            IEnumerable<LayoutResilienceItemFactProjectionIssue> issues,
            string canonicalSignature)
        {
            SchemaId = LayoutResilienceItemFactProjectionSchema.SchemaId;
            SchemaVersion = LayoutResilienceItemFactProjectionSchema.SchemaVersion;
            Status = status;
            BuildFactsCompleteness = buildFactsCompleteness;
            BuildFacts = LayoutResilienceItemFactProjectionReadOnly.CloneBuildFacts(
                buildFacts);
            this.issues = Array.AsReadOnly((issues ??
                    Array.Empty<LayoutResilienceItemFactProjectionIssue>())
                .Where(value => value != null)
                .Select(value => value.Clone())
                .OrderBy(value => value.Code, StringComparer.Ordinal)
                .ThenBy(value => value.ItemInstanceId, StringComparer.Ordinal)
                .ThenBy(value => value.PlacementId, StringComparer.Ordinal)
                .ThenBy(value => value.ItemId, StringComparer.Ordinal)
                .ThenBy(value => value.Message, StringComparer.Ordinal)
                .ToArray());
            CanonicalSignature = canonicalSignature ?? string.Empty;
        }

        public string SchemaId { get; }
        public int SchemaVersion { get; }
        public LayoutResilienceItemFactProjectionStatus Status { get; }
        public LayoutResilienceInputCompleteness BuildFactsCompleteness { get; }
        public LayoutResilienceBuildFactSnapshot BuildFacts { get; }
        public IReadOnlyList<LayoutResilienceItemFactProjectionIssue> Issues => issues;
        public string CanonicalSignature { get; }
    }

    internal static class LayoutResilienceItemFactProjectionReadOnly
    {
        public static LayoutResilienceBuildFactSnapshot CloneBuildFacts(
            LayoutResilienceBuildFactSnapshot value)
        {
            if (value == null)
            {
                return null;
            }
            return new LayoutResilienceBuildFactSnapshot(
                value.BoardSize,
                value.BaselineEyeCell == null
                    ? null
                    : new LayoutCellCoordinate(
                        value.BaselineEyeCell.X, value.BaselineEyeCell.Y),
                value.PlacementRows.Select(row => row == null
                    ? null
                    : new LayoutResiliencePlacedItemFactSnapshot(
                        row.PlacementId,
                        row.ItemId,
                        row.AnchorCell == null
                            ? null
                            : new LayoutCellCoordinate(row.AnchorCell.X, row.AnchorCell.Y),
                        row.Rotation,
                        row.ShapeCells.Select(cell => cell == null
                            ? null
                            : new LayoutCellCoordinate(cell.X, cell.Y)),
                        row.OccupiedCells.Select(cell => cell == null
                            ? null
                            : new LayoutCellCoordinate(cell.X, cell.Y)),
                        row.CoreCellWorld == null
                            ? null
                            : new LayoutCellCoordinate(
                                row.CoreCellWorld.X, row.CoreCellWorld.Y),
                        row.IsLit,
                        row.IsCountedInBuild)));
        }
    }

    public interface ILayoutResilienceItemFactProjectionAdapter
    {
        LayoutResilienceItemFactProjectionResult Project(
            ItemSystemSnapshot itemSnapshot,
            ItemInstancePlacementBindingContractSnapshot bindingSnapshot);
    }

    internal static class LayoutResilienceItemFactProjectionCanonical
    {
        public static string Signature(
            ItemSystemSnapshot itemSnapshot,
            ItemInstancePlacementBindingContractSnapshot bindingSnapshot,
            LayoutResilienceItemFactProjectionStatus status,
            LayoutResilienceInputCompleteness completeness,
            LayoutResilienceBuildFactSnapshot buildFacts,
            IEnumerable<LayoutResilienceItemFactProjectionIssue> issues)
        {
            StringBuilder builder = new StringBuilder(16384);
            Field(builder, "schemaId", LayoutResilienceItemFactProjectionSchema.SchemaId);
            Field(builder, "schemaVersion", Integer(
                LayoutResilienceItemFactProjectionSchema.SchemaVersion));
            ItemSource(builder, itemSnapshot);
            BindingSource(builder, bindingSnapshot);
            Field(builder, "result.status", Integer((int)status));
            Field(builder, "result.buildFactsCompleteness", Integer((int)completeness));
            BuildFacts(builder, buildFacts);
            IssueRows(builder, issues);
            return Hash(builder.ToString());
        }

        private static void ItemSource(StringBuilder builder, ItemSystemSnapshot value)
        {
            Field(builder, "item.present", Boolean(value != null));
            if (value == null)
            {
                return;
            }

            Field(builder, "item.schemaVersion", value.schemaVersion);
            Field(builder, "item.isValid", Boolean(value.isValid));
            Field(builder, "item.boardSize", Integer(value.boardSize));
            Field(builder, "item.eyeCell", Cell(value.eyeCell.x, value.eyeCell.y));

            ItemSystemValidationError[] errors = value.validationErrors
                .OrderBy(error => error == null ? string.Empty : error.code,
                    StringComparer.Ordinal)
                .ThenBy(error => error == null ? string.Empty : error.placementId,
                    StringComparer.Ordinal)
                .ThenBy(error => error == null ? string.Empty : error.itemId,
                    StringComparer.Ordinal)
                .ThenBy(error => error == null ? string.Empty : error.message,
                    StringComparer.Ordinal)
                .ToArray();
            Field(builder, "item.validationErrors.count", Integer(errors.Length));
            for (int index = 0; index < errors.Length; index++)
            {
                ItemSystemValidationError error = errors[index];
                string prefix = "item.validationErrors[" + Integer(index) + "].";
                Field(builder, prefix + "present", Boolean(error != null));
                if (error == null)
                {
                    continue;
                }
                Field(builder, prefix + "code", error.code);
                Field(builder, prefix + "severity", Integer((int)error.severity));
                Field(builder, prefix + "placementId", error.placementId);
                Field(builder, prefix + "itemId", error.itemId);
                Field(builder, prefix + "message", error.message);
            }

            ItemSystemCatalogItemSnapshot[] catalog = value.catalogItems
                .OrderBy(row => row == null ? string.Empty : row.itemId,
                    StringComparer.Ordinal)
                .ThenBy(row => row == null ? string.Empty : Cells(row.ShapeCells),
                    StringComparer.Ordinal)
                .ToArray();
            Field(builder, "item.catalog.count", Integer(catalog.Length));
            for (int index = 0; index < catalog.Length; index++)
            {
                ItemSystemCatalogItemSnapshot row = catalog[index];
                string prefix = "item.catalog[" + Integer(index) + "].";
                Field(builder, prefix + "present", Boolean(row != null));
                if (row == null)
                {
                    continue;
                }
                Field(builder, prefix + "itemId", row.itemId);
                CellRows(builder, prefix + "shapeCells", row.ShapeCells.Select(cell =>
                    new LayoutCellCoordinate(cell.x, cell.y)));
            }

            ItemSystemPlacementSnapshot[] placements = value.placements
                .OrderBy(row => row == null ? string.Empty : row.placementId,
                    StringComparer.Ordinal)
                .ThenBy(row => row == null ? string.Empty : row.itemId,
                    StringComparer.Ordinal)
                .ToArray();
            Field(builder, "item.placements.count", Integer(placements.Length));
            for (int index = 0; index < placements.Length; index++)
            {
                ItemSystemPlacementSnapshot row = placements[index];
                string prefix = "item.placements[" + Integer(index) + "].";
                Field(builder, prefix + "present", Boolean(row != null));
                if (row == null)
                {
                    continue;
                }
                Field(builder, prefix + "placementId", row.placementId);
                Field(builder, prefix + "itemId", row.itemId);
                Field(builder, prefix + "anchorCell",
                    Cell(row.anchorCell.x, row.anchorCell.y));
                Field(builder, prefix + "rotation", Integer(row.rotation));
                CellRows(builder, prefix + "occupiedCells", row.OccupiedCells.Select(cell =>
                    new LayoutCellCoordinate(cell.x, cell.y)));
                Field(builder, prefix + "coreCellWorld",
                    Cell(row.coreCellWorld.x, row.coreCellWorld.y));
                Field(builder, prefix + "isLit", Boolean(row.isLit));
                Field(builder, prefix + "isCountedInBuild",
                    Boolean(row.isCountedInBuild));
            }
        }

        private static void BindingSource(
            StringBuilder builder,
            ItemInstancePlacementBindingContractSnapshot value)
        {
            Field(builder, "binding.present", Boolean(value != null));
            if (value == null)
            {
                return;
            }

            Field(builder, "binding.schemaId", value.schemaId);
            Field(builder, "binding.status", Integer((int)value.status));
            Field(builder, "binding.isValid", Boolean(value.isValid));
            Field(builder, "binding.canonicalSignature", value.canonicalSignature);

            ItemInstancePlacementBindingSnapshot[] rows = value.Bindings
                .OrderBy(row => row == null ? string.Empty : row.itemInstanceId,
                    StringComparer.Ordinal)
                .ThenBy(row => row == null ? string.Empty : row.placementId,
                    StringComparer.Ordinal)
                .ThenBy(row => row == null ? string.Empty : row.baseItemId,
                    StringComparer.Ordinal)
                .ToArray();
            Field(builder, "binding.rows.count", Integer(rows.Length));
            for (int index = 0; index < rows.Length; index++)
            {
                ItemInstancePlacementBindingSnapshot row = rows[index];
                string prefix = "binding.rows[" + Integer(index) + "].";
                Field(builder, prefix + "present", Boolean(row != null));
                if (row == null)
                {
                    continue;
                }
                Field(builder, prefix + "itemInstanceId", row.itemInstanceId);
                Field(builder, prefix + "placementId", row.placementId);
                Field(builder, prefix + "baseItemId", row.baseItemId);
            }

            ItemInstancePlacementBindingValidationError[] errors = value.ValidationErrors
                .OrderBy(error => error == null ? string.Empty : error.code,
                    StringComparer.Ordinal)
                .ThenBy(error => error == null ? string.Empty : error.itemInstanceId,
                    StringComparer.Ordinal)
                .ThenBy(error => error == null ? string.Empty : error.placementId,
                    StringComparer.Ordinal)
                .ThenBy(error => error == null ? string.Empty : error.baseItemId,
                    StringComparer.Ordinal)
                .ThenBy(error => error == null ? string.Empty : error.message,
                    StringComparer.Ordinal)
                .ToArray();
            Field(builder, "binding.validationErrors.count", Integer(errors.Length));
            for (int index = 0; index < errors.Length; index++)
            {
                ItemInstancePlacementBindingValidationError error = errors[index];
                string prefix = "binding.validationErrors[" + Integer(index) + "].";
                Field(builder, prefix + "present", Boolean(error != null));
                if (error == null)
                {
                    continue;
                }
                Field(builder, prefix + "code", error.code);
                Field(builder, prefix + "status", Integer((int)error.status));
                Field(builder, prefix + "itemInstanceId", error.itemInstanceId);
                Field(builder, prefix + "placementId", error.placementId);
                Field(builder, prefix + "baseItemId", error.baseItemId);
                Field(builder, prefix + "message", error.message);
            }
        }

        private static void BuildFacts(
            StringBuilder builder,
            LayoutResilienceBuildFactSnapshot value)
        {
            Field(builder, "result.buildFacts.present", Boolean(value != null));
            if (value == null)
            {
                return;
            }
            Field(builder, "result.buildFacts.boardSize", value.BoardSize.HasValue
                ? Integer(value.BoardSize.Value)
                : "<missing>");
            Field(builder, "result.buildFacts.baselineEyeCell", value.BaselineEyeCell == null
                ? "<missing>"
                : Cell(value.BaselineEyeCell.X, value.BaselineEyeCell.Y));
            LayoutResiliencePlacedItemFactSnapshot[] rows = value.PlacementRows
                .OrderBy(row => row == null ? string.Empty : row.PlacementId,
                    StringComparer.Ordinal)
                .ThenBy(row => row == null ? string.Empty : row.ItemId,
                    StringComparer.Ordinal)
                .ToArray();
            Field(builder, "result.buildFacts.rows.count", Integer(rows.Length));
            for (int index = 0; index < rows.Length; index++)
            {
                LayoutResiliencePlacedItemFactSnapshot row = rows[index];
                string prefix = "result.buildFacts.rows[" + Integer(index) + "].";
                Field(builder, prefix + "present", Boolean(row != null));
                if (row == null)
                {
                    continue;
                }
                Field(builder, prefix + "placementId", row.PlacementId);
                Field(builder, prefix + "itemId", row.ItemId);
                Field(builder, prefix + "anchorCell", row.AnchorCell == null
                    ? "<missing>"
                    : Cell(row.AnchorCell.X, row.AnchorCell.Y));
                Field(builder, prefix + "rotation", Integer(row.Rotation));
                CellRows(builder, prefix + "shapeCells", row.ShapeCells);
                CellRows(builder, prefix + "occupiedCells", row.OccupiedCells);
                Field(builder, prefix + "coreCellWorld", row.CoreCellWorld == null
                    ? "<missing>"
                    : Cell(row.CoreCellWorld.X, row.CoreCellWorld.Y));
                Field(builder, prefix + "isLit", Boolean(row.IsLit));
                Field(builder, prefix + "isCountedInBuild",
                    Boolean(row.IsCountedInBuild));
            }
        }

        private static void IssueRows(
            StringBuilder builder,
            IEnumerable<LayoutResilienceItemFactProjectionIssue> values)
        {
            LayoutResilienceItemFactProjectionIssue[] rows = (values ??
                    Array.Empty<LayoutResilienceItemFactProjectionIssue>())
                .Where(value => value != null)
                .OrderBy(value => value.Code, StringComparer.Ordinal)
                .ThenBy(value => value.ItemInstanceId, StringComparer.Ordinal)
                .ThenBy(value => value.PlacementId, StringComparer.Ordinal)
                .ThenBy(value => value.ItemId, StringComparer.Ordinal)
                .ThenBy(value => value.Message, StringComparer.Ordinal)
                .ToArray();
            Field(builder, "result.issues.count", Integer(rows.Length));
            for (int index = 0; index < rows.Length; index++)
            {
                LayoutResilienceItemFactProjectionIssue row = rows[index];
                string prefix = "result.issues[" + Integer(index) + "].";
                Field(builder, prefix + "code", row.Code);
                Field(builder, prefix + "status", Integer((int)row.Status));
                Field(builder, prefix + "itemInstanceId", row.ItemInstanceId);
                Field(builder, prefix + "placementId", row.PlacementId);
                Field(builder, prefix + "itemId", row.ItemId);
                Field(builder, prefix + "message", row.Message);
            }
        }

        private static void CellRows(
            StringBuilder builder,
            string name,
            IEnumerable<LayoutCellCoordinate> values)
        {
            LayoutCellCoordinate[] cells = (values ?? Array.Empty<LayoutCellCoordinate>())
                .OrderBy(value => value == null ? int.MinValue : value.Y)
                .ThenBy(value => value == null ? int.MinValue : value.X)
                .ToArray();
            Field(builder, name + ".count", Integer(cells.Length));
            for (int index = 0; index < cells.Length; index++)
            {
                LayoutCellCoordinate cell = cells[index];
                Field(builder, name + "[" + Integer(index) + "]", cell == null
                    ? "<missing>"
                    : Cell(cell.X, cell.Y));
            }
        }

        private static string Cells(IEnumerable<UnityEngine.Vector2Int> values)
        {
            return string.Join(";", (values ?? Array.Empty<UnityEngine.Vector2Int>())
                .OrderBy(value => value.y)
                .ThenBy(value => value.x)
                .Select(value => Cell(value.x, value.y)));
        }

        private static string Cell(int x, int y)
        {
            return Integer(x) + ":" + Integer(y);
        }

        private static string Boolean(bool value)
        {
            return value ? "true" : "false";
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
