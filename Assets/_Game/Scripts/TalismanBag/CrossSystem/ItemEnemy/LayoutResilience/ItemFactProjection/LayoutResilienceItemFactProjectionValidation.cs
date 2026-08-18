using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using TalismanBag.EnemySystem.RequirementChannel;
using TalismanBag.Items;
using TalismanBag.Items.Capability;

namespace TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.ItemFactProjection
{
    public static class LayoutResilienceItemFactProjectionValidationCodes
    {
        public const string ItemSourceMissing = "ITEM_SOURCE_MISSING";
        public const string ItemSchemaMismatch = "ITEM_SCHEMA_MISMATCH";
        public const string ItemSourceInvalid = "ITEM_SOURCE_INVALID";
        public const string BoardSizeInvalid = "BOARD_SIZE_INVALID";
        public const string EyeCellOutOfBounds = "EYE_CELL_OUT_OF_BOUNDS";
        public const string CatalogItemIdMissing = "CATALOG_ITEM_ID_MISSING";
        public const string CatalogItemIdDuplicate = "CATALOG_ITEM_ID_DUPLICATE";
        public const string CatalogShapeEmpty = "CATALOG_SHAPE_EMPTY";
        public const string CatalogShapeCellDuplicate = "CATALOG_SHAPE_CELL_DUPLICATE";
        public const string PlacementIdMissing = "PLACEMENT_ID_MISSING";
        public const string PlacementIdDuplicate = "PLACEMENT_ID_DUPLICATE";
        public const string PlacementItemIdMissing = "PLACEMENT_ITEM_ID_MISSING";
        public const string PlacementCatalogMissing = "PLACEMENT_CATALOG_MISSING";
        public const string PlacementRotationInvalid = "PLACEMENT_ROTATION_INVALID";
        public const string PlacementOccupiedCellsEmpty = "PLACEMENT_OCCUPIED_CELLS_EMPTY";
        public const string PlacementOccupiedCellDuplicate =
            "PLACEMENT_OCCUPIED_CELL_DUPLICATE";
        public const string ShapeOccupiedCardinalityMismatch =
            "SHAPE_OCCUPIED_CARDINALITY_MISMATCH";
        public const string AnchorCellOutOfBounds = "ANCHOR_CELL_OUT_OF_BOUNDS";
        public const string OccupiedCellOutOfBounds = "OCCUPIED_CELL_OUT_OF_BOUNDS";
        public const string CoreCellOutOfBounds = "CORE_CELL_OUT_OF_BOUNDS";
        public const string CoreCellNotOccupied = "CORE_CELL_NOT_OCCUPIED";
        public const string CountedPlacementNotLit = "COUNTED_PLACEMENT_NOT_LIT";
        public const string BindingSourceMissing = "BINDING_SOURCE_MISSING";
        public const string BindingSchemaMismatch = "BINDING_SCHEMA_MISMATCH";
        public const string BindingStatusUndefined = "BINDING_STATUS_UNDEFINED";
        public const string BindingSourceUnknown = "BINDING_SOURCE_UNKNOWN";
        public const string BindingSourceInvalid = "BINDING_SOURCE_INVALID";
        public const string BindingErrorStatusUndefined = "BINDING_ERROR_STATUS_UNDEFINED";
        public const string BindingRowMissing = "BINDING_ROW_MISSING";
        public const string BindingItemInstanceIdMissing =
            "BINDING_ITEM_INSTANCE_ID_MISSING";
        public const string BindingPlacementIdMissing = "BINDING_PLACEMENT_ID_MISSING";
        public const string BindingBaseItemIdMissing = "BINDING_BASE_ITEM_ID_MISSING";
        public const string BindingItemInstanceIdDuplicate =
            "BINDING_ITEM_INSTANCE_ID_DUPLICATE";
        public const string BindingPlacementIdDuplicate =
            "BINDING_PLACEMENT_ID_DUPLICATE";
        public const string OrdinaryPlacementBindingMissing =
            "ORDINARY_PLACEMENT_BINDING_MISSING";
        public const string BindingPlacementOrphan = "BINDING_PLACEMENT_ORPHAN";
        public const string BindingBaseItemIdMismatch =
            "BINDING_BASE_ITEM_ID_MISMATCH";
        public const string I031OrdinaryBindingForbidden =
            "I031_ORDINARY_BINDING_FORBIDDEN";
        public const string N01CBuildFactsInvalid = "N01C_BUILD_FACTS_INVALID";
        public const string ResultSchemaInvalid = "RESULT_SCHEMA_INVALID";
        public const string ResultStatusUndefined = "RESULT_STATUS_UNDEFINED";
        public const string ResultShapeInvalid = "RESULT_SHAPE_INVALID";
        public const string ResultSignatureInvalid = "RESULT_SIGNATURE_INVALID";
    }

    public sealed class DefaultLayoutResilienceItemFactProjectionValidator
    {
        public static readonly DefaultLayoutResilienceItemFactProjectionValidator Instance =
            new DefaultLayoutResilienceItemFactProjectionValidator();

        public IReadOnlyList<LayoutResilienceItemFactProjectionIssue> ValidateResult(
            LayoutResilienceItemFactProjectionResult result)
        {
            List<LayoutResilienceItemFactProjectionIssue> issues =
                new List<LayoutResilienceItemFactProjectionIssue>();
            if (result == null)
            {
                Add(issues, LayoutResilienceItemFactProjectionValidationCodes.ResultShapeInvalid,
                    LayoutResilienceItemFactProjectionStatus.Invalid, string.Empty,
                    string.Empty, string.Empty, "Projection result is required.");
                return Freeze(issues);
            }
            if (!string.Equals(result.SchemaId,
                    LayoutResilienceItemFactProjectionSchema.SchemaId,
                    StringComparison.Ordinal) ||
                result.SchemaVersion != LayoutResilienceItemFactProjectionSchema.SchemaVersion)
            {
                Add(issues, LayoutResilienceItemFactProjectionValidationCodes.ResultSchemaInvalid,
                    LayoutResilienceItemFactProjectionStatus.Invalid, string.Empty,
                    string.Empty, string.Empty,
                    "Result schema identity and version must match exactly.");
            }
            if (!IsProjectionStatusDefined(result.Status))
            {
                Add(issues,
                    LayoutResilienceItemFactProjectionValidationCodes.ResultStatusUndefined,
                    LayoutResilienceItemFactProjectionStatus.Invalid, string.Empty,
                    string.Empty, string.Empty, "Result status must be a named value.");
            }

            bool correctShape =
                (result.Status == LayoutResilienceItemFactProjectionStatus.Complete &&
                 result.BuildFactsCompleteness == LayoutResilienceInputCompleteness.Complete &&
                 result.BuildFacts != null && result.Issues.Count == 0) ||
                (result.Status == LayoutResilienceItemFactProjectionStatus.Unknown &&
                 result.BuildFactsCompleteness == LayoutResilienceInputCompleteness.Incomplete) ||
                (result.Status == LayoutResilienceItemFactProjectionStatus.Invalid &&
                 result.BuildFactsCompleteness == LayoutResilienceInputCompleteness.Incomplete &&
                 result.BuildFacts == null);
            if (!correctShape)
            {
                Add(issues, LayoutResilienceItemFactProjectionValidationCodes.ResultShapeInvalid,
                    LayoutResilienceItemFactProjectionStatus.Invalid, string.Empty,
                    string.Empty, string.Empty,
                    "Status, completeness, payload, and issue shape is inconsistent.");
            }
            if (!IsSignature(result.CanonicalSignature))
            {
                Add(issues,
                    LayoutResilienceItemFactProjectionValidationCodes.ResultSignatureInvalid,
                    LayoutResilienceItemFactProjectionStatus.Invalid, string.Empty,
                    string.Empty, string.Empty,
                    "Canonical signature must be sha256 plus 64 lowercase hex digits.");
            }
            if (result.Issues.Any(issue => issue == null ||
                !IsProjectionStatusDefined(issue.Status) ||
                issue.Status == LayoutResilienceItemFactProjectionStatus.Complete))
            {
                Add(issues, LayoutResilienceItemFactProjectionValidationCodes.ResultShapeInvalid,
                    LayoutResilienceItemFactProjectionStatus.Invalid, string.Empty,
                    string.Empty, string.Empty,
                    "Issues must be non-null Unknown or Invalid diagnostics.");
            }
            return Freeze(issues);
        }

        internal ProjectionAssessment Assess(
            ItemSystemSnapshot itemSnapshot,
            ItemInstancePlacementBindingContractSnapshot bindingSnapshot)
        {
            List<LayoutResilienceItemFactProjectionIssue> issues =
                new List<LayoutResilienceItemFactProjectionIssue>();
            if (itemSnapshot == null)
            {
                Add(issues, LayoutResilienceItemFactProjectionValidationCodes.ItemSourceMissing,
                    LayoutResilienceItemFactProjectionStatus.Unknown, string.Empty,
                    string.Empty, string.Empty,
                    "ItemSystemSnapshot.v2 source is missing.");
                return ProjectionAssessment.Unknown(null, issues);
            }

            if (!string.Equals(itemSnapshot.schemaVersion,
                ItemSystemSnapshot.CurrentSchemaVersion, StringComparison.Ordinal))
            {
                Add(issues,
                    LayoutResilienceItemFactProjectionValidationCodes.ItemSchemaMismatch,
                    LayoutResilienceItemFactProjectionStatus.Invalid, string.Empty,
                    string.Empty, string.Empty,
                    "Item schemaVersion must match ItemSystemSnapshot.v2 exactly.");
            }
            if (!itemSnapshot.isValid || itemSnapshot.validationErrors.Count > 0)
            {
                Add(issues, LayoutResilienceItemFactProjectionValidationCodes.ItemSourceInvalid,
                    LayoutResilienceItemFactProjectionStatus.Invalid, string.Empty,
                    string.Empty, string.Empty,
                    "Item source is invalid or carries validation errors.");
            }

            Dictionary<string, ItemSystemCatalogItemSnapshot> catalog =
                ValidateItemStructure(itemSnapshot, issues);
            if (HasInvalid(issues))
            {
                return ProjectionAssessment.Invalid(issues);
            }

            LayoutResilienceBuildFactSnapshot buildFacts = ProjectBuildFacts(
                itemSnapshot, catalog);
            ValidateN01CCompatibility(buildFacts, issues);
            ValidateBindings(itemSnapshot, bindingSnapshot, issues);

            if (HasInvalid(issues))
            {
                return ProjectionAssessment.Invalid(issues);
            }
            if (issues.Any(value =>
                value.Status == LayoutResilienceItemFactProjectionStatus.Unknown))
            {
                return ProjectionAssessment.Unknown(buildFacts, issues);
            }
            return ProjectionAssessment.Complete(buildFacts);
        }

        private static Dictionary<string, ItemSystemCatalogItemSnapshot>
            ValidateItemStructure(
                ItemSystemSnapshot itemSnapshot,
                ICollection<LayoutResilienceItemFactProjectionIssue> issues)
        {
            Dictionary<string, ItemSystemCatalogItemSnapshot> catalog =
                new Dictionary<string, ItemSystemCatalogItemSnapshot>(
                    StringComparer.Ordinal);
            if (itemSnapshot.boardSize <= 0)
            {
                Add(issues, LayoutResilienceItemFactProjectionValidationCodes.BoardSizeInvalid,
                    LayoutResilienceItemFactProjectionStatus.Invalid, string.Empty,
                    string.Empty, string.Empty, "Board size must be positive.");
            }
            if (!InBounds(itemSnapshot.eyeCell.x, itemSnapshot.eyeCell.y,
                itemSnapshot.boardSize))
            {
                Add(issues,
                    LayoutResilienceItemFactProjectionValidationCodes.EyeCellOutOfBounds,
                    LayoutResilienceItemFactProjectionStatus.Invalid, string.Empty,
                    string.Empty, string.Empty, "Eye cell must be within the board.");
            }

            foreach (ItemSystemCatalogItemSnapshot row in itemSnapshot.catalogItems)
            {
                if (row == null)
                {
                    Add(issues,
                        LayoutResilienceItemFactProjectionValidationCodes.CatalogItemIdMissing,
                        LayoutResilienceItemFactProjectionStatus.Invalid, string.Empty,
                        string.Empty, string.Empty, "Catalog rows cannot be null.");
                    continue;
                }
                if (string.IsNullOrWhiteSpace(row.itemId))
                {
                    Add(issues,
                        LayoutResilienceItemFactProjectionValidationCodes.CatalogItemIdMissing,
                        LayoutResilienceItemFactProjectionStatus.Invalid, string.Empty,
                        string.Empty, row.itemId,
                        "Catalog itemId must be explicit and non-empty.");
                }
                else if (catalog.ContainsKey(row.itemId))
                {
                    Add(issues,
                        LayoutResilienceItemFactProjectionValidationCodes
                            .CatalogItemIdDuplicate,
                        LayoutResilienceItemFactProjectionStatus.Invalid, string.Empty,
                        string.Empty, row.itemId,
                        "Catalog itemId must be unique with ordinal semantics.");
                }
                else
                {
                    catalog.Add(row.itemId, row);
                }
                if (row.ShapeCells.Count == 0)
                {
                    Add(issues,
                        LayoutResilienceItemFactProjectionValidationCodes.CatalogShapeEmpty,
                        LayoutResilienceItemFactProjectionStatus.Invalid, string.Empty,
                        string.Empty, row.itemId,
                        "Catalog shape cells must be non-empty.");
                }
                if (row.ShapeCells.Distinct().Count() != row.ShapeCells.Count)
                {
                    Add(issues,
                        LayoutResilienceItemFactProjectionValidationCodes
                            .CatalogShapeCellDuplicate,
                        LayoutResilienceItemFactProjectionStatus.Invalid, string.Empty,
                        string.Empty, row.itemId,
                        "Catalog shape cells must be unique.");
                }
            }

            HashSet<string> placementIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (ItemSystemPlacementSnapshot row in itemSnapshot.placements)
            {
                if (row == null)
                {
                    Add(issues,
                        LayoutResilienceItemFactProjectionValidationCodes.PlacementIdMissing,
                        LayoutResilienceItemFactProjectionStatus.Invalid, string.Empty,
                        string.Empty, string.Empty, "Placement rows cannot be null.");
                    continue;
                }
                if (string.IsNullOrWhiteSpace(row.placementId))
                {
                    Add(issues,
                        LayoutResilienceItemFactProjectionValidationCodes.PlacementIdMissing,
                        LayoutResilienceItemFactProjectionStatus.Invalid, string.Empty,
                        row.placementId, row.itemId,
                        "PlacementId must be explicit and non-empty.");
                }
                else if (!placementIds.Add(row.placementId))
                {
                    Add(issues,
                        LayoutResilienceItemFactProjectionValidationCodes
                            .PlacementIdDuplicate,
                        LayoutResilienceItemFactProjectionStatus.Invalid, string.Empty,
                        row.placementId, row.itemId,
                        "PlacementId must be unique with ordinal semantics.");
                }
                if (string.IsNullOrWhiteSpace(row.itemId))
                {
                    Add(issues,
                        LayoutResilienceItemFactProjectionValidationCodes
                            .PlacementItemIdMissing,
                        LayoutResilienceItemFactProjectionStatus.Invalid, string.Empty,
                        row.placementId, row.itemId,
                        "Placement itemId must be explicit and non-empty.");
                }
                ItemSystemCatalogItemSnapshot catalogRow;
                if (!catalog.TryGetValue(row.itemId, out catalogRow))
                {
                    Add(issues,
                        LayoutResilienceItemFactProjectionValidationCodes
                            .PlacementCatalogMissing,
                        LayoutResilienceItemFactProjectionStatus.Invalid, string.Empty,
                        row.placementId, row.itemId,
                        "Every placement must resolve one ordinal catalog row.");
                }
                if (row.rotation != 0 && row.rotation != 90 && row.rotation != 180 &&
                    row.rotation != 270)
                {
                    Add(issues,
                        LayoutResilienceItemFactProjectionValidationCodes
                            .PlacementRotationInvalid,
                        LayoutResilienceItemFactProjectionStatus.Invalid, string.Empty,
                        row.placementId, row.itemId,
                        "Rotation must be 0, 90, 180, or 270.");
                }
                if (row.OccupiedCells.Count == 0)
                {
                    Add(issues,
                        LayoutResilienceItemFactProjectionValidationCodes
                            .PlacementOccupiedCellsEmpty,
                        LayoutResilienceItemFactProjectionStatus.Invalid, string.Empty,
                        row.placementId, row.itemId,
                        "Occupied cells must be non-empty.");
                }
                if (row.OccupiedCells.Distinct().Count() != row.OccupiedCells.Count)
                {
                    Add(issues,
                        LayoutResilienceItemFactProjectionValidationCodes
                            .PlacementOccupiedCellDuplicate,
                        LayoutResilienceItemFactProjectionStatus.Invalid, string.Empty,
                        row.placementId, row.itemId,
                        "Occupied cells must be unique.");
                }
                if (catalogRow != null &&
                    catalogRow.ShapeCells.Count != row.OccupiedCells.Count)
                {
                    Add(issues,
                        LayoutResilienceItemFactProjectionValidationCodes
                            .ShapeOccupiedCardinalityMismatch,
                        LayoutResilienceItemFactProjectionStatus.Invalid, string.Empty,
                        row.placementId, row.itemId,
                        "Shape and occupied cell cardinalities must match.");
                }
                if (!InBounds(row.anchorCell.x, row.anchorCell.y, itemSnapshot.boardSize))
                {
                    Add(issues,
                        LayoutResilienceItemFactProjectionValidationCodes
                            .AnchorCellOutOfBounds,
                        LayoutResilienceItemFactProjectionStatus.Invalid, string.Empty,
                        row.placementId, row.itemId,
                        "Anchor cell must be within the board.");
                }
                if (row.OccupiedCells.Any(cell =>
                    !InBounds(cell.x, cell.y, itemSnapshot.boardSize)))
                {
                    Add(issues,
                        LayoutResilienceItemFactProjectionValidationCodes
                            .OccupiedCellOutOfBounds,
                        LayoutResilienceItemFactProjectionStatus.Invalid, string.Empty,
                        row.placementId, row.itemId,
                        "Occupied cells must be within the board.");
                }
                if (!InBounds(row.coreCellWorld.x, row.coreCellWorld.y,
                    itemSnapshot.boardSize))
                {
                    Add(issues,
                        LayoutResilienceItemFactProjectionValidationCodes
                            .CoreCellOutOfBounds,
                        LayoutResilienceItemFactProjectionStatus.Invalid, string.Empty,
                        row.placementId, row.itemId,
                        "Core cell must be within the board.");
                }
                if (!row.OccupiedCells.Contains(row.coreCellWorld))
                {
                    Add(issues,
                        LayoutResilienceItemFactProjectionValidationCodes
                            .CoreCellNotOccupied,
                        LayoutResilienceItemFactProjectionStatus.Invalid, string.Empty,
                        row.placementId, row.itemId,
                        "Core cell must be one of the occupied cells.");
                }
                if (row.isCountedInBuild && !row.isLit)
                {
                    Add(issues,
                        LayoutResilienceItemFactProjectionValidationCodes
                            .CountedPlacementNotLit,
                        LayoutResilienceItemFactProjectionStatus.Invalid, string.Empty,
                        row.placementId, row.itemId,
                        "A counted placement cannot be unlit.");
                }
            }
            return catalog;
        }

        private static LayoutResilienceBuildFactSnapshot ProjectBuildFacts(
            ItemSystemSnapshot itemSnapshot,
            IReadOnlyDictionary<string, ItemSystemCatalogItemSnapshot> catalog)
        {
            LayoutResiliencePlacedItemFactSnapshot[] rows = itemSnapshot.placements
                .OrderBy(value => value.placementId, StringComparer.Ordinal)
                .ThenBy(value => value.itemId, StringComparer.Ordinal)
                .Select(value => new LayoutResiliencePlacedItemFactSnapshot(
                    value.placementId,
                    value.itemId,
                    new LayoutCellCoordinate(value.anchorCell.x, value.anchorCell.y),
                    value.rotation,
                    catalog[value.itemId].ShapeCells.Select(cell =>
                        new LayoutCellCoordinate(cell.x, cell.y)),
                    value.OccupiedCells.Select(cell =>
                        new LayoutCellCoordinate(cell.x, cell.y)),
                    new LayoutCellCoordinate(value.coreCellWorld.x,
                        value.coreCellWorld.y),
                    value.isLit,
                    value.isCountedInBuild))
                .ToArray();
            return new LayoutResilienceBuildFactSnapshot(
                itemSnapshot.boardSize,
                new LayoutCellCoordinate(itemSnapshot.eyeCell.x, itemSnapshot.eyeCell.y),
                rows);
        }

        private static void ValidateBindings(
            ItemSystemSnapshot itemSnapshot,
            ItemInstancePlacementBindingContractSnapshot bindingSnapshot,
            ICollection<LayoutResilienceItemFactProjectionIssue> issues)
        {
            ItemSystemPlacementSnapshot[] ordinaryPlacements = itemSnapshot.placements
                .Where(value => !string.Equals(value.itemId, "I031",
                    StringComparison.Ordinal))
                .ToArray();
            if (bindingSnapshot == null)
            {
                if (ordinaryPlacements.Length > 0)
                {
                    Add(issues,
                        LayoutResilienceItemFactProjectionValidationCodes
                            .BindingSourceMissing,
                        LayoutResilienceItemFactProjectionStatus.Unknown, string.Empty,
                        string.Empty, string.Empty,
                        "Ordinary placements require an IF01 binding source.");
                }
                return;
            }

            if (!string.Equals(bindingSnapshot.schemaId,
                ItemInstancePlacementBindingContractSnapshot.CurrentSchemaId,
                StringComparison.Ordinal))
            {
                Add(issues,
                    LayoutResilienceItemFactProjectionValidationCodes
                        .BindingSchemaMismatch,
                    LayoutResilienceItemFactProjectionStatus.Invalid, string.Empty,
                    string.Empty, string.Empty,
                    "Binding schemaId must match IF01 exactly.");
            }
            if (!IsBindingStatusDefined(bindingSnapshot.status))
            {
                Add(issues,
                    LayoutResilienceItemFactProjectionValidationCodes
                        .BindingStatusUndefined,
                    LayoutResilienceItemFactProjectionStatus.Invalid, string.Empty,
                    string.Empty, string.Empty,
                    "Binding status must be a named IF01 value.");
            }
            else if (bindingSnapshot.status == ItemInstancePlacementBindingStatus.Invalid)
            {
                Add(issues,
                    LayoutResilienceItemFactProjectionValidationCodes.BindingSourceInvalid,
                    LayoutResilienceItemFactProjectionStatus.Invalid, string.Empty,
                    string.Empty, string.Empty, "IF01 source status is Invalid.");
            }
            else if (bindingSnapshot.status == ItemInstancePlacementBindingStatus.Unknown)
            {
                Add(issues,
                    LayoutResilienceItemFactProjectionValidationCodes.BindingSourceUnknown,
                    LayoutResilienceItemFactProjectionStatus.Unknown, string.Empty,
                    string.Empty, string.Empty, "IF01 source status is Unknown.");
            }
            else if (!bindingSnapshot.isValid ||
                bindingSnapshot.ValidationErrors.Count > 0)
            {
                Add(issues,
                    LayoutResilienceItemFactProjectionValidationCodes.BindingSourceInvalid,
                    LayoutResilienceItemFactProjectionStatus.Invalid, string.Empty,
                    string.Empty, string.Empty,
                    "A Valid IF01 source cannot carry validation errors.");
            }

            foreach (ItemInstancePlacementBindingValidationError error in
                bindingSnapshot.ValidationErrors)
            {
                if (error == null || !IsBindingStatusDefined(error.status))
                {
                    Add(issues,
                        LayoutResilienceItemFactProjectionValidationCodes
                            .BindingErrorStatusUndefined,
                        LayoutResilienceItemFactProjectionStatus.Invalid, string.Empty,
                        string.Empty, string.Empty,
                        "IF01 validation error status must be named.");
                }
                else if (error.status == ItemInstancePlacementBindingStatus.Invalid)
                {
                    Add(issues,
                        LayoutResilienceItemFactProjectionValidationCodes
                            .BindingSourceInvalid,
                        LayoutResilienceItemFactProjectionStatus.Invalid,
                        error.itemInstanceId, error.placementId, error.baseItemId,
                        "IF01 carries an Invalid validation error.");
                }
            }

            Dictionary<string, ItemSystemPlacementSnapshot> placementById =
                itemSnapshot.placements
                    .GroupBy(value => value.placementId, StringComparer.Ordinal)
                    .Where(group => group.Count() == 1)
                    .ToDictionary(group => group.Key, group => group.Single(),
                        StringComparer.Ordinal);
            HashSet<string> instanceIds = new HashSet<string>(StringComparer.Ordinal);
            HashSet<string> bindingPlacementIds = new HashSet<string>(
                StringComparer.Ordinal);
            Dictionary<string, List<ItemInstancePlacementBindingSnapshot>> byPlacement =
                new Dictionary<string, List<ItemInstancePlacementBindingSnapshot>>(
                    StringComparer.Ordinal);

            foreach (ItemInstancePlacementBindingSnapshot row in bindingSnapshot.Bindings)
            {
                if (row == null)
                {
                    Add(issues,
                        LayoutResilienceItemFactProjectionValidationCodes.BindingRowMissing,
                        LayoutResilienceItemFactProjectionStatus.Invalid, string.Empty,
                        string.Empty, string.Empty, "Binding rows cannot be null.");
                    continue;
                }
                if (string.IsNullOrWhiteSpace(row.itemInstanceId))
                {
                    Add(issues,
                        LayoutResilienceItemFactProjectionValidationCodes
                            .BindingItemInstanceIdMissing,
                        LayoutResilienceItemFactProjectionStatus.Invalid,
                        row.itemInstanceId, row.placementId, row.baseItemId,
                        "Ordinary binding itemInstanceId must be explicit.");
                }
                else if (!instanceIds.Add(row.itemInstanceId))
                {
                    Add(issues,
                        LayoutResilienceItemFactProjectionValidationCodes
                            .BindingItemInstanceIdDuplicate,
                        LayoutResilienceItemFactProjectionStatus.Invalid,
                        row.itemInstanceId, row.placementId, row.baseItemId,
                        "itemInstanceId must be unique with ordinal semantics.");
                }
                if (string.IsNullOrWhiteSpace(row.placementId))
                {
                    Add(issues,
                        LayoutResilienceItemFactProjectionValidationCodes
                            .BindingPlacementIdMissing,
                        LayoutResilienceItemFactProjectionStatus.Invalid,
                        row.itemInstanceId, row.placementId, row.baseItemId,
                        "Binding placementId must be explicit.");
                }
                else if (!bindingPlacementIds.Add(row.placementId))
                {
                    Add(issues,
                        LayoutResilienceItemFactProjectionValidationCodes
                            .BindingPlacementIdDuplicate,
                        LayoutResilienceItemFactProjectionStatus.Invalid,
                        row.itemInstanceId, row.placementId, row.baseItemId,
                        "Binding placementId must be unique with ordinal semantics.");
                }
                if (string.IsNullOrWhiteSpace(row.baseItemId))
                {
                    Add(issues,
                        LayoutResilienceItemFactProjectionValidationCodes
                            .BindingBaseItemIdMissing,
                        LayoutResilienceItemFactProjectionStatus.Invalid,
                        row.itemInstanceId, row.placementId, row.baseItemId,
                        "Binding baseItemId must be explicit.");
                }
                if (string.Equals(row.baseItemId, "I031", StringComparison.Ordinal))
                {
                    Add(issues,
                        LayoutResilienceItemFactProjectionValidationCodes
                            .I031OrdinaryBindingForbidden,
                        LayoutResilienceItemFactProjectionStatus.Invalid,
                        row.itemInstanceId, row.placementId, row.baseItemId,
                        "I031 cannot be represented as an ordinary item instance.");
                }
                List<ItemInstancePlacementBindingSnapshot> bucket;
                if (!byPlacement.TryGetValue(row.placementId, out bucket))
                {
                    bucket = new List<ItemInstancePlacementBindingSnapshot>();
                    byPlacement.Add(row.placementId, bucket);
                }
                bucket.Add(row);

                ItemSystemPlacementSnapshot placement;
                if (!placementById.TryGetValue(row.placementId, out placement))
                {
                    Add(issues,
                        LayoutResilienceItemFactProjectionValidationCodes
                            .BindingPlacementOrphan,
                        LayoutResilienceItemFactProjectionStatus.Unknown,
                        row.itemInstanceId, row.placementId, row.baseItemId,
                        "Binding placementId has no Item placement.");
                }
                else if (string.Equals(placement.itemId, "I031",
                    StringComparison.Ordinal))
                {
                    Add(issues,
                        LayoutResilienceItemFactProjectionValidationCodes
                            .I031OrdinaryBindingForbidden,
                        LayoutResilienceItemFactProjectionStatus.Invalid,
                        row.itemInstanceId, row.placementId, row.baseItemId,
                        "A binding cannot point to an I031 placement.");
                }
                else if (!string.Equals(row.baseItemId, placement.itemId,
                    StringComparison.Ordinal))
                {
                    Add(issues,
                        LayoutResilienceItemFactProjectionValidationCodes
                            .BindingBaseItemIdMismatch,
                        LayoutResilienceItemFactProjectionStatus.Invalid,
                        row.itemInstanceId, row.placementId, row.baseItemId,
                        "Binding baseItemId must equal Placement.itemId exactly.");
                }
            }

            foreach (ItemSystemPlacementSnapshot placement in ordinaryPlacements)
            {
                List<ItemInstancePlacementBindingSnapshot> matches;
                if (!byPlacement.TryGetValue(placement.placementId, out matches) ||
                    matches.Count == 0)
                {
                    Add(issues,
                        LayoutResilienceItemFactProjectionValidationCodes
                            .OrdinaryPlacementBindingMissing,
                        LayoutResilienceItemFactProjectionStatus.Unknown, string.Empty,
                        placement.placementId, placement.itemId,
                        "Ordinary placement has no explicit IF01 binding.");
                }
            }
        }

        private static void ValidateN01CCompatibility(
            LayoutResilienceBuildFactSnapshot buildFacts,
            ICollection<LayoutResilienceItemFactProjectionIssue> issues)
        {
            LayoutResilienceEvaluationInput envelope =
                new LayoutResilienceEvaluationInput(
                    "dev_layout_item_fact_projection_validation",
                    EnemyRequirementApplicabilityState.Unknown,
                    LayoutResilienceInputCompleteness.Complete,
                    LayoutResilienceInputCompleteness.Incomplete,
                    buildFacts,
                    null);
            IReadOnlyList<LayoutResilienceValidationIssue> n01cIssues =
                DefaultLayoutResilienceStructuralPredicateValidator.Instance
                    .Validate(envelope);
            if (n01cIssues.Count > 0)
            {
                Add(issues,
                    LayoutResilienceItemFactProjectionValidationCodes.N01CBuildFactsInvalid,
                    LayoutResilienceItemFactProjectionStatus.Invalid, string.Empty,
                    string.Empty, string.Empty,
                    "N01C BuildFacts validator rejected: " + string.Join("|",
                        n01cIssues.Select(value => value.Code).OrderBy(value => value,
                            StringComparer.Ordinal)));
            }
        }

        private static bool InBounds(int x, int y, int boardSize)
        {
            return boardSize > 0 && x >= 0 && y >= 0 && x < boardSize && y < boardSize;
        }

        private static bool HasInvalid(
            IEnumerable<LayoutResilienceItemFactProjectionIssue> issues)
        {
            return issues.Any(value =>
                value.Status == LayoutResilienceItemFactProjectionStatus.Invalid);
        }

        private static bool IsProjectionStatusDefined(
            LayoutResilienceItemFactProjectionStatus value)
        {
            return value == LayoutResilienceItemFactProjectionStatus.Complete ||
                value == LayoutResilienceItemFactProjectionStatus.Unknown ||
                value == LayoutResilienceItemFactProjectionStatus.Invalid;
        }

        private static bool IsBindingStatusDefined(
            ItemInstancePlacementBindingStatus value)
        {
            return value == ItemInstancePlacementBindingStatus.Valid ||
                value == ItemInstancePlacementBindingStatus.Unknown ||
                value == ItemInstancePlacementBindingStatus.Invalid;
        }

        private static bool IsSignature(string value)
        {
            return value != null && value.Length == 71 &&
                value.StartsWith("sha256:", StringComparison.Ordinal) &&
                value.Substring(7).All(character =>
                    (character >= '0' && character <= '9') ||
                    (character >= 'a' && character <= 'f'));
        }

        private static ReadOnlyCollection<LayoutResilienceItemFactProjectionIssue> Freeze(
            IEnumerable<LayoutResilienceItemFactProjectionIssue> issues)
        {
            return Array.AsReadOnly((issues ??
                    Array.Empty<LayoutResilienceItemFactProjectionIssue>())
                .Where(value => value != null)
                .Select(value => value.Clone())
                .OrderBy(value => value.Code, StringComparer.Ordinal)
                .ThenBy(value => value.ItemInstanceId, StringComparer.Ordinal)
                .ThenBy(value => value.PlacementId, StringComparer.Ordinal)
                .ThenBy(value => value.ItemId, StringComparer.Ordinal)
                .ThenBy(value => value.Message, StringComparer.Ordinal)
                .ToArray());
        }

        private static void Add(
            ICollection<LayoutResilienceItemFactProjectionIssue> issues,
            string code,
            LayoutResilienceItemFactProjectionStatus status,
            string itemInstanceId,
            string placementId,
            string itemId,
            string message)
        {
            issues.Add(new LayoutResilienceItemFactProjectionIssue(
                code, status, itemInstanceId, placementId, itemId, message));
        }
    }

    public static class LayoutResilienceItemFactProjectionValidation
    {
        public static IReadOnlyList<LayoutResilienceItemFactProjectionIssue> ValidateResult(
            LayoutResilienceItemFactProjectionResult result)
        {
            return DefaultLayoutResilienceItemFactProjectionValidator.Instance
                .ValidateResult(result);
        }

        public static ItemInstancePlacementBindingContractSnapshot
            CreateBindingContractForFixture(
                ItemInstancePlacementBindingStatus status,
                IEnumerable<string> itemInstanceIds,
                IEnumerable<string> placementIds,
                IEnumerable<string> baseItemIds,
                IEnumerable<ItemInstancePlacementBindingValidationError> validationErrors =
                    null)
        {
            string[] instances = (itemInstanceIds ?? Array.Empty<string>()).ToArray();
            string[] placements = (placementIds ?? Array.Empty<string>()).ToArray();
            string[] bases = (baseItemIds ?? Array.Empty<string>()).ToArray();
            if (instances.Length != placements.Length || instances.Length != bases.Length)
            {
                throw new ArgumentException(
                    "Fixture binding columns must have equal cardinality.");
            }
            ItemInstancePlacementBindingSnapshot[] rows = Enumerable.Range(
                    0, instances.Length)
                .Select(index => new ItemInstancePlacementBindingSnapshot(
                    instances[index], placements[index], bases[index]))
                .ToArray();
            return new ItemInstancePlacementBindingContractSnapshot(
                status, rows, validationErrors);
        }
    }

    internal sealed class ProjectionAssessment
    {
        private ProjectionAssessment(
            LayoutResilienceItemFactProjectionStatus status,
            LayoutResilienceBuildFactSnapshot buildFacts,
            IEnumerable<LayoutResilienceItemFactProjectionIssue> issues)
        {
            Status = status;
            BuildFacts = LayoutResilienceItemFactProjectionReadOnly.CloneBuildFacts(
                buildFacts);
            Issues = Array.AsReadOnly((issues ??
                    Array.Empty<LayoutResilienceItemFactProjectionIssue>())
                .Where(value => value != null)
                .Select(value => value.Clone())
                .ToArray());
        }

        public LayoutResilienceItemFactProjectionStatus Status { get; }
        public LayoutResilienceBuildFactSnapshot BuildFacts { get; }
        public IReadOnlyList<LayoutResilienceItemFactProjectionIssue> Issues { get; }

        public static ProjectionAssessment Complete(
            LayoutResilienceBuildFactSnapshot buildFacts)
        {
            return new ProjectionAssessment(
                LayoutResilienceItemFactProjectionStatus.Complete,
                buildFacts,
                Array.Empty<LayoutResilienceItemFactProjectionIssue>());
        }

        public static ProjectionAssessment Unknown(
            LayoutResilienceBuildFactSnapshot buildFacts,
            IEnumerable<LayoutResilienceItemFactProjectionIssue> issues)
        {
            return new ProjectionAssessment(
                LayoutResilienceItemFactProjectionStatus.Unknown, buildFacts, issues);
        }

        public static ProjectionAssessment Invalid(
            IEnumerable<LayoutResilienceItemFactProjectionIssue> issues)
        {
            return new ProjectionAssessment(
                LayoutResilienceItemFactProjectionStatus.Invalid, null, issues);
        }
    }
}
