#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BuildSandbox;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class ShapeAwareItemTrayValidator
    {
        public const string PackageName = BattlePrepareComponentAdapterRuntimePlaytestPlan.ShapeAwareItemTrayPackageName;
        private const int RequiredShapeAwareRowCount = 4;

        private static readonly Dictionary<string, (int width, int height, string offsets)> RequiredRows = new()
        {
            { "Single1", (1, 1, "0,0") },
            { "Vertical2", (1, 2, "0,0;0,1") },
            { "Corner3", (2, 2, "0,0;1,0;0,1") },
            { "Square4", (2, 2, "0,0;1,0;0,1;1,1") }
        };

        public static List<BuildSandboxValidationReport> BuildValidationReports()
        {
            return new List<BuildSandboxValidationReport> { Validate() };
        }

        public static BattlePrepareComponentAdapterRuntimePlaytestPlan BuildDefaultPlan()
        {
            return BattlePrepareComponentAdapterRuntimePlaytestPlan.BuildDefault();
        }

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report =
                new("Shape-Aware ItemTray Fixture Fix 02");
            BattlePrepareComponentAdapterRuntimePlaytestPlan plan = BuildDefaultPlan();

            ValidateIsolation(report, plan);
            ValidateShapeAwareRows(report, plan);
            ValidateFixtureCoverage(report, plan);
            return report;
        }

        private static void ValidateIsolation(
            BuildSandboxValidationReport report,
            BattlePrepareComponentAdapterRuntimePlaytestPlan plan)
        {
            if (plan == null)
            {
                report.AddError("SHAPE_AWARE_ITEMTRAY_PLAN_NULL", "Shape-aware item tray plan was not created.", PackageName);
                return;
            }

            if (!plan.devOnly || !plan.fixtureProviderDevOnly)
            {
                report.AddError(
                    "SHAPE_AWARE_ITEMTRAY_DEVONLY_FALSE",
                    "Shape-aware fixture tray extension must remain devOnly.",
                    nameof(BattlePrepareComponentAdapterRuntimePlaytestPlan));
            }

            ValidateFalse(report, "SHAPE_AWARE_ITEMTRAY_ENABLED_TRUE", plan.isEnabled);
            ValidateFalse(report, "SHAPE_AWARE_ITEMTRAY_WRITES_FORMAL_SCENE", plan.writesFormalScene);
            ValidateFalse(report, "SHAPE_AWARE_ITEMTRAY_WRITES_FORMAL_UI", plan.writesFormalUi);
            ValidateFalse(report, "SHAPE_AWARE_ITEMTRAY_WRITES_FORMAL_POOL", plan.writesFixtureToFormalPool);
            ValidateFalse(report, "SHAPE_AWARE_ITEMTRAY_WRITES_SAVE", plan.writesFixtureToSaveData);
            ValidateFalse(report, "SHAPE_AWARE_ITEMTRAY_TOUCHES_RUNFLOW", plan.touchesRunFlow);
            ValidateFalse(report, "SHAPE_AWARE_ITEMTRAY_TOUCHES_PAGESTATE", plan.touchesPageState);
            ValidateFalse(report, "SHAPE_AWARE_ITEMTRAY_TOUCHES_FORMATIONSTATE", plan.touchesFormationState);
            ValidateFalse(report, "SHAPE_AWARE_ITEMTRAY_TOUCHES_SAVE", plan.touchesSaveData);
            ValidateFalse(report, "SHAPE_AWARE_ITEMTRAY_TOUCHES_BOSS_REWARD_NUMERIC", plan.touchesBossRewardDropNumeric);
            ValidateFalse(report, "SHAPE_AWARE_ITEMTRAY_REDRAWS_BOARD", plan.redrewV04Board);
            ValidateFalse(report, "SHAPE_AWARE_ITEMTRAY_REDRAWS_TRAY", plan.redrewV04ItemTray);
            ValidateFalse(report, "SHAPE_AWARE_ITEMTRAY_REWRITES_DRAG", plan.rewroteDragFeel);
            ValidateFalse(report, "SHAPE_AWARE_ITEMTRAY_REWRITES_PULLUP", plan.rewrotePullUpAnimation);
            ValidateFalse(report, "SHAPE_AWARE_ITEMTRAY_FEATURE_FLAG_TRUE", plan.featureFlagDefaultEnabled);

            if (!plan.itemTrayShapeAwareDisplay
                || !plan.shapeAwareDragVisual
                || !plan.ghostPreviewMatchesTrayShape
                || !plan.selectedHighlightCoversShape)
            {
                report.AddError(
                    "SHAPE_AWARE_ITEMTRAY_RUNTIME_FLAGS_INCOMPLETE",
                    "ItemTrayShapeExtension must declare true-shape tray display, drag visual, selection, and ghost-preview consistency.",
                    nameof(BattlePrepareComponentAdapterRuntimePlaytestPlan));
                return;
            }

            report.AddInfo(
                "SHAPE_AWARE_ITEMTRAY_ISOLATION_PASS",
                "Shape-aware fixture tray extension is devOnly, disabled by default, and scoped to ItemTrayShapeExtension.",
                nameof(BattlePrepareComponentAdapterRuntimePlaytestPlan));
        }

        private static void ValidateShapeAwareRows(
            BuildSandboxValidationReport report,
            BattlePrepareComponentAdapterRuntimePlaytestPlan plan)
        {
            IReadOnlyList<BattlePrepareShapeAwareItemTrayRow> rows =
                plan != null && plan.shapeAwareItemTrayRows != null
                    ? plan.shapeAwareItemTrayRows
                    : Array.Empty<BattlePrepareShapeAwareItemTrayRow>();

            if (rows.Count < RequiredShapeAwareRowCount)
            {
                report.AddError(
                    "SHAPE_AWARE_ITEMTRAY_ROW_COUNT",
                    $"Shape-aware row count too low. actual={rows.Count}, expected>={RequiredShapeAwareRowCount}.",
                    PackageName);
            }
            else
            {
                report.AddInfo(
                    "SHAPE_AWARE_ITEMTRAY_ROW_COUNT",
                    $"Shape-aware row count pass. actual={rows.Count}, expected>={RequiredShapeAwareRowCount}.",
                    PackageName);
            }

            HashSet<string> seenShapeIds = new(StringComparer.Ordinal);
            foreach (BattlePrepareShapeAwareItemTrayRow row in rows)
            {
                if (row == null)
                {
                    report.AddError("SHAPE_AWARE_ITEMTRAY_ROW_NULL", "Null shape-aware item tray row.", PackageName);
                    continue;
                }

                if (!RequiredRows.TryGetValue(row.shapeId, out (int width, int height, string offsets) required))
                {
                    report.AddError(
                        "SHAPE_AWARE_ITEMTRAY_UNKNOWN_SHAPE",
                        $"Unexpected shape-aware row: {row.shapeId}.",
                        nameof(BattlePrepareShapeAwareItemTrayRow));
                    continue;
                }

                seenShapeIds.Add(row.shapeId);
                if (row.footprintWidth != required.width
                    || row.footprintHeight != required.height
                    || !string.Equals(row.occupiedOffsets, required.offsets, StringComparison.Ordinal))
                {
                    report.AddError(
                        "SHAPE_AWARE_ITEMTRAY_FOOTPRINT_MISMATCH",
                        $"Shape-aware footprint mismatch. shape={row.shapeId}.",
                        nameof(BattlePrepareShapeAwareItemTrayRow));
                }

                if (!row.itemTrayShowsTrueShape
                    || !row.selectedHighlightCoversShape
                    || !row.dragVisualKeepsShape
                    || !row.ghostPreviewMatchesTrayShape
                    || !row.usesMatureItemTray)
                {
                    report.AddError(
                        "SHAPE_AWARE_ITEMTRAY_BEHAVIOR_INCOMPLETE",
                        $"Shape-aware behavior flags incomplete. shape={row.shapeId}.",
                        nameof(BattlePrepareShapeAwareItemTrayRow));
                }

                if (row.redrawsItemTray || row.writesFormalConfig || row.writesSaveData)
                {
                    report.AddError(
                        "SHAPE_AWARE_ITEMTRAY_ROW_LEAK",
                        $"Shape-aware row leaks into forbidden scope. shape={row.shapeId}.",
                        nameof(BattlePrepareShapeAwareItemTrayRow));
                }
            }

            foreach (string requiredShape in RequiredRows.Keys)
            {
                if (seenShapeIds.Contains(requiredShape))
                {
                    report.AddInfo(
                        "SHAPE_AWARE_ITEMTRAY_REQUIRED_PRESENT",
                        $"Required shape-aware tray row present: {requiredShape}.",
                        nameof(BattlePrepareShapeAwareItemTrayRow));
                    continue;
                }

                report.AddError(
                    "SHAPE_AWARE_ITEMTRAY_REQUIRED_MISSING",
                    $"Missing required shape-aware tray row: {requiredShape}.",
                    nameof(BattlePrepareShapeAwareItemTrayRow));
            }
        }

        private static void ValidateFixtureCoverage(
            BuildSandboxValidationReport report,
            BattlePrepareComponentAdapterRuntimePlaytestPlan plan)
        {
            HashSet<string> fixtureShapeIds = new(
                (plan?.fixtureItems ?? new List<BattlePrepareRuntimeFixtureItemRow>())
                .Where(row => row != null)
                .Select(row => row.shapeId),
                StringComparer.Ordinal);

            foreach (string requiredShape in RequiredRows.Keys)
            {
                if (fixtureShapeIds.Contains(requiredShape))
                {
                    report.AddInfo(
                        "SHAPE_AWARE_ITEMTRAY_FIXTURE_COVERAGE",
                        $"Fixture coverage present for shape: {requiredShape}.",
                        nameof(BattlePrepareRuntimeFixtureItemRow));
                    continue;
                }

                report.AddError(
                    "SHAPE_AWARE_ITEMTRAY_FIXTURE_MISSING",
                    $"Missing fixture item for shape-aware tray display: {requiredShape}.",
                    nameof(BattlePrepareRuntimeFixtureItemRow));
            }
        }

        private static void ValidateFalse(
            BuildSandboxValidationReport report,
            string code,
            bool value)
        {
            if (value)
            {
                report.AddError(code, "This shape-aware item tray isolation flag must stay false.", PackageName);
            }
        }
    }

    public static class ShapeAwareTrayPackingValidator
    {
        public const string PackageName =
            BattlePrepareComponentAdapterRuntimePlaytestPlan.ShapeAwareTrayPackingDragPackageName;
        private const int RequiredShapeAwareRowCount = 4;
        private const int TrayColumnCount = ShapeAwareTrayPackingMap.DefaultColumnCount;
        private const int TraySlotCount = ShapeAwareTrayPackingMap.DefaultSlotCount;

        private static readonly string[] RequiredShapeIds =
        {
            "Single1",
            "Vertical2",
            "Corner3",
            "Square4"
        };

        public static List<BuildSandboxValidationReport> BuildValidationReports()
        {
            return new List<BuildSandboxValidationReport> { Validate() };
        }

        public static BattlePrepareComponentAdapterRuntimePlaytestPlan BuildDefaultPlan()
        {
            return BattlePrepareComponentAdapterRuntimePlaytestPlan.BuildDefault();
        }

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report =
                new("Shape-Aware Tray Packing Drag Fix 03");
            BattlePrepareComponentAdapterRuntimePlaytestPlan plan = BuildDefaultPlan();

            ValidateIsolation(report, plan);
            ValidateInitialPacking(report, plan);
            ValidateDragPersistence(report, plan);
            return report;
        }

        private static void ValidateIsolation(
            BuildSandboxValidationReport report,
            BattlePrepareComponentAdapterRuntimePlaytestPlan plan)
        {
            if (plan == null)
            {
                report.AddError("SHAPE_AWARE_TRAY_PACKING_PLAN_NULL", "Shape-aware tray packing plan was not created.", PackageName);
                return;
            }

            if (!plan.devOnly || !plan.fixtureProviderDevOnly)
            {
                report.AddError(
                    "SHAPE_AWARE_TRAY_PACKING_DEVONLY_FALSE",
                    "Shape-aware tray packing fix must remain devOnly.",
                    nameof(BattlePrepareComponentAdapterRuntimePlaytestPlan));
            }

            ValidateFalse(report, "SHAPE_AWARE_TRAY_PACKING_ENABLED_TRUE", plan.isEnabled);
            ValidateFalse(report, "SHAPE_AWARE_TRAY_PACKING_WRITES_FORMAL_SCENE", plan.writesFormalScene);
            ValidateFalse(report, "SHAPE_AWARE_TRAY_PACKING_WRITES_FORMAL_UI", plan.writesFormalUi);
            ValidateFalse(report, "SHAPE_AWARE_TRAY_PACKING_WRITES_FORMAL_POOL", plan.writesFixtureToFormalPool);
            ValidateFalse(report, "SHAPE_AWARE_TRAY_PACKING_WRITES_SAVE", plan.writesFixtureToSaveData);
            ValidateFalse(report, "SHAPE_AWARE_TRAY_PACKING_TOUCHES_RUNFLOW", plan.touchesRunFlow);
            ValidateFalse(report, "SHAPE_AWARE_TRAY_PACKING_TOUCHES_PAGESTATE", plan.touchesPageState);
            ValidateFalse(report, "SHAPE_AWARE_TRAY_PACKING_TOUCHES_FORMATIONSTATE", plan.touchesFormationState);
            ValidateFalse(report, "SHAPE_AWARE_TRAY_PACKING_TOUCHES_SAVE", plan.touchesSaveData);
            ValidateFalse(report, "SHAPE_AWARE_TRAY_PACKING_TOUCHES_BOSS_REWARD_NUMERIC", plan.touchesBossRewardDropNumeric);
            ValidateFalse(report, "SHAPE_AWARE_TRAY_PACKING_REDRAWS_BOARD", plan.redrewV04Board);
            ValidateFalse(report, "SHAPE_AWARE_TRAY_PACKING_REDRAWS_TRAY", plan.redrewV04ItemTray);
            ValidateFalse(report, "SHAPE_AWARE_TRAY_PACKING_REWRITES_DRAG", plan.rewroteDragFeel);
            ValidateFalse(report, "SHAPE_AWARE_TRAY_PACKING_REWRITES_PULLUP", plan.rewrotePullUpAnimation);
            ValidateFalse(report, "SHAPE_AWARE_TRAY_PACKING_FEATURE_FLAG_TRUE", plan.featureFlagDefaultEnabled);

            report.AddInfo(
                "SHAPE_AWARE_TRAY_PACKING_ISOLATION_PASS",
                "Fix03 remains an ItemTrayShapeExtension/devOnly fixture patch and does not redraw the mature BattlePrepare tray.",
                nameof(BattlePrepareComponentAdapterRuntimePlaytestPlan));
        }

        private static void ValidateInitialPacking(
            BuildSandboxValidationReport report,
            BattlePrepareComponentAdapterRuntimePlaytestPlan plan)
        {
            IReadOnlyList<BattlePrepareShapeAwareItemTrayRow> rows =
                plan != null && plan.shapeAwareItemTrayRows != null
                    ? plan.shapeAwareItemTrayRows
                    : Array.Empty<BattlePrepareShapeAwareItemTrayRow>();
            if (rows.Count < RequiredShapeAwareRowCount)
            {
                report.AddError(
                    "SHAPE_AWARE_TRAY_PACKING_ROW_COUNT",
                    $"Shape-aware row count too low. actual={rows.Count}, expected>={RequiredShapeAwareRowCount}.",
                    PackageName);
            }

            Dictionary<string, BattlePrepareShapeAwareItemTrayRow> rowByShapeId = rows
                .Where(row => row != null)
                .ToDictionary(row => row.shapeId, StringComparer.OrdinalIgnoreCase);
            HashSet<int> reserved = new();
            foreach (string shapeId in RequiredShapeIds)
            {
                if (!rowByShapeId.TryGetValue(shapeId, out BattlePrepareShapeAwareItemTrayRow row))
                {
                    report.AddError(
                        "SHAPE_AWARE_TRAY_PACKING_REQUIRED_MISSING",
                        $"Missing required shape-aware tray packing row: {shapeId}.",
                        nameof(BattlePrepareShapeAwareItemTrayRow));
                    continue;
                }

                List<ItemShapeCell> offsets = ShapeAwareTrayPackingMap.ParseOffsets(row.occupiedOffsets);
                if (!ShapeAwareTrayPackingMap.CanFit(
                        row.initialAnchorSlotIndex,
                        TrayColumnCount,
                        TraySlotCount,
                        offsets,
                        reserved,
                        out string reason))
                {
                    report.AddError(
                        "SHAPE_AWARE_TRAY_PACKING_INITIAL_ILLEGAL",
                        $"Initial tray packing is illegal. shape={shapeId}, anchor={row.initialAnchorSlotIndex}, reason={reason}.",
                        nameof(BattlePrepareShapeAwareItemTrayRow));
                    continue;
                }

                string occupiedSlots = ShapeAwareTrayPackingMap.FormatOccupiedSlots(
                    ShapeAwareTrayPackingMap.BuildOccupiedSlotIndexes(
                        row.initialAnchorSlotIndex,
                        TrayColumnCount,
                        TraySlotCount,
                        offsets));
                if (!string.Equals(row.initialOccupiedSlots, occupiedSlots, StringComparison.Ordinal))
                {
                    report.AddError(
                        "SHAPE_AWARE_TRAY_PACKING_INITIAL_OCCUPIED_MISMATCH",
                        $"Initial occupied cells mismatch. shape={shapeId}, actual={row.initialOccupiedSlots}, expected={occupiedSlots}.",
                        nameof(BattlePrepareShapeAwareItemTrayRow));
                }

                if (!row.initialPackingInBounds || row.initialPackingHasOverlap || !row.trayPackingUsesOccupiedCells)
                {
                    report.AddError(
                        "SHAPE_AWARE_TRAY_PACKING_INITIAL_FLAGS_INVALID",
                        $"Initial packing flags are not shape-aware. shape={shapeId}.",
                        nameof(BattlePrepareShapeAwareItemTrayRow));
                }

                foreach (int slotIndex in ShapeAwareTrayPackingMap.BuildOccupiedSlotIndexes(
                             row.initialAnchorSlotIndex,
                             TrayColumnCount,
                             TraySlotCount,
                             offsets))
                {
                    reserved.Add(slotIndex);
                }

                report.AddInfo(
                    "SHAPE_AWARE_TRAY_PACKING_INITIAL_PASS",
                    $"Initial packing pass. shape={shapeId}, anchor={row.initialAnchorSlotIndex}, occupied={occupiedSlots}.",
                    nameof(BattlePrepareShapeAwareItemTrayRow));
            }
        }

        private static void ValidateDragPersistence(
            BuildSandboxValidationReport report,
            BattlePrepareComponentAdapterRuntimePlaytestPlan plan)
        {
            IReadOnlyList<BattlePrepareShapeAwareItemTrayRow> rows =
                plan != null && plan.shapeAwareItemTrayRows != null
                    ? plan.shapeAwareItemTrayRows
                    : Array.Empty<BattlePrepareShapeAwareItemTrayRow>();
            Dictionary<string, BattlePrepareShapeAwareItemTrayRow> rowByShapeId = rows
                .Where(row => row != null)
                .ToDictionary(row => row.shapeId, StringComparer.OrdinalIgnoreCase);

            foreach (string shapeId in RequiredShapeIds)
            {
                if (!rowByShapeId.TryGetValue(shapeId, out BattlePrepareShapeAwareItemTrayRow row))
                {
                    continue;
                }

                List<ItemShapeCell> offsets = ShapeAwareTrayPackingMap.ParseOffsets(row.occupiedOffsets);
                HashSet<int> reservedByOthers = BuildInitialReservedExcept(rowByShapeId, shapeId);
                if (!ShapeAwareTrayPackingMap.CanFit(
                        row.trayMoveProbeAnchorSlotIndex,
                        TrayColumnCount,
                        TraySlotCount,
                        offsets,
                        reservedByOthers,
                        out string reason))
                {
                    report.AddError(
                        "SHAPE_AWARE_TRAY_PACKING_MOVE_PROBE_ILLEGAL",
                        $"Tray move probe is illegal. shape={shapeId}, anchor={row.trayMoveProbeAnchorSlotIndex}, reason={reason}.",
                        nameof(BattlePrepareShapeAwareItemTrayRow));
                }

                string moveOccupiedSlots = ShapeAwareTrayPackingMap.FormatOccupiedSlots(
                    ShapeAwareTrayPackingMap.BuildOccupiedSlotIndexes(
                        row.trayMoveProbeAnchorSlotIndex,
                        TrayColumnCount,
                        TraySlotCount,
                        offsets));
                if (!string.Equals(row.trayMoveProbeOccupiedSlots, moveOccupiedSlots, StringComparison.Ordinal))
                {
                    report.AddError(
                        "SHAPE_AWARE_TRAY_PACKING_MOVE_OCCUPIED_MISMATCH",
                        $"Move probe occupied cells mismatch. shape={shapeId}, actual={row.trayMoveProbeOccupiedSlots}, expected={moveOccupiedSlots}.",
                        nameof(BattlePrepareShapeAwareItemTrayRow));
                }

                if (!row.trayDragStatePersists
                    || !row.illegalTrayDropReturnsToLastLegal
                    || !row.boardPreviewDragDoesNotReturnToWrongInitialPosition
                    || !string.Equals(
                        row.boardCommitMode,
                        BattlePrepareComponentAdapterRuntimePlaytestPlan.DeferredBoardCommitMode,
                        StringComparison.Ordinal))
                {
                    report.AddError(
                        "SHAPE_AWARE_TRAY_PACKING_DRAG_FLAGS_INVALID",
                        $"Drag persistence flags are incomplete. shape={shapeId}.",
                        nameof(BattlePrepareShapeAwareItemTrayRow));
                }

                if (row.redrawsItemTray || row.writesFormalConfig || row.writesSaveData)
                {
                    report.AddError(
                        "SHAPE_AWARE_TRAY_PACKING_ROW_LEAK",
                        $"Shape-aware packing row leaks outside devOnly fixture scope. shape={shapeId}.",
                        nameof(BattlePrepareShapeAwareItemTrayRow));
                }

                report.AddInfo(
                    "SHAPE_AWARE_TRAY_PACKING_DRAG_PASS",
                    $"Drag persistence probe pass. shape={shapeId}, moveAnchor={row.trayMoveProbeAnchorSlotIndex}, occupied={moveOccupiedSlots}.",
                    nameof(BattlePrepareShapeAwareItemTrayRow));
            }
        }

        private static HashSet<int> BuildInitialReservedExcept(
            IReadOnlyDictionary<string, BattlePrepareShapeAwareItemTrayRow> rowByShapeId,
            string exceptShapeId)
        {
            HashSet<int> reserved = new();
            foreach (BattlePrepareShapeAwareItemTrayRow row in rowByShapeId.Values)
            {
                if (row == null || string.Equals(row.shapeId, exceptShapeId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                foreach (int slotIndex in ShapeAwareTrayPackingMap.BuildOccupiedSlotIndexes(
                             row.initialAnchorSlotIndex,
                             TrayColumnCount,
                             TraySlotCount,
                             ShapeAwareTrayPackingMap.ParseOffsets(row.occupiedOffsets)))
                {
                    reserved.Add(slotIndex);
                }
            }

            return reserved;
        }

        private static void ValidateFalse(
            BuildSandboxValidationReport report,
            string code,
            bool value)
        {
            if (value)
            {
                report.AddError(code, "This shape-aware tray packing isolation flag must stay false.", PackageName);
            }
        }
    }
}
#endif
