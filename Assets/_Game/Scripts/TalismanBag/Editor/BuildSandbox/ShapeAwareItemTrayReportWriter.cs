#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TalismanBag.BuildSandbox;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class ShapeAwareItemTrayReportWriter
    {
        public const string MainReportPath =
            "Docs/V0.4/Reports/ShapeAwareItemTrayFixtureReport.md";

        public const string LeakCheckReportPath =
            "Docs/V0.4/Reports/ShapeAwareItemTrayFixtureLeakCheckReport.md";

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattlePrepareComponentAdapterRuntimePlaytestPlan plan = null)
        {
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();
            BattlePrepareComponentAdapterRuntimePlaytestPlan safePlan =
                plan ?? ShapeAwareItemTrayValidator.BuildDefaultPlan();
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string mainPath = Path.Combine(projectRoot, MainReportPath);
            string leakPath = Path.Combine(projectRoot, LeakCheckReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(mainPath) ?? projectRoot);

            int errors = safeReports.Sum(report => report.ErrorCount);
            int warnings = safeReports.Sum(report => report.WarningCount);
            int leakCount = CountLeaks(safePlan);

            File.WriteAllText(
                mainPath,
                BuildMainReport(safeReports, safePlan, errors, warnings, leakCount),
                new UTF8Encoding(false));
            File.WriteAllText(
                leakPath,
                BuildLeakCheckReport(safeReports, safePlan, errors, warnings, leakCount),
                new UTF8Encoding(false));

            AssetDatabase.Refresh();
            return new[] { mainPath, leakPath };
        }

        private static string BuildMainReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattlePrepareComponentAdapterRuntimePlaytestPlan plan,
            int errors,
            int warnings,
            int leakCount)
        {
            string status = errors == 0 && leakCount == 0
                ? "READY_SHAPE_AWARE_ITEMTRAY_PLAYTEST"
                : "SHAPE_AWARE_ITEMTRAY_BLOCKED";

            StringBuilder builder = new();
            builder.AppendLine("# Shape-Aware ItemTray Fixture Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattlePrepareComponentAdapterRuntimePlaytestPlan.ShapeAwareItemTrayPackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{status}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine($"Leak Count: `{leakCount}`");
            builder.AppendLine();
            builder.AppendLine("## Runtime Entry");
            builder.AppendLine();
            builder.AppendLine($"- Manual Runtime Menu: `{Escape(plan.manualMenuPath)}`");
            builder.AppendLine($"- Shape-Aware QA Menu: `{Escape(BattlePrepareComponentAdapterRuntimePlaytestPlan.ShapeAwareItemTrayQaMenuPath)}`");
            builder.AppendLine($"- Extension: `ItemTrayShapeExtension`");
            builder.AppendLine($"- itemTrayShapeAwareDisplay: `{plan.itemTrayShapeAwareDisplay}`");
            builder.AppendLine($"- shapeAwareDragVisual: `{plan.shapeAwareDragVisual}`");
            builder.AppendLine($"- ghostPreviewMatchesTrayShape: `{plan.ghostPreviewMatchesTrayShape}`");
            builder.AppendLine($"- selectedHighlightCoversShape: `{plan.selectedHighlightCoversShape}`");
            builder.AppendLine();
            builder.AppendLine("## Shape-Aware Fixture Rows");
            builder.AppendLine();
            builder.AppendLine("| Shape | Tray Footprint | Width | Height | Occupied Offsets | Tray True Shape | Selected Highlight | Drag Visual | Ghost Matches | Mature Tray | Redraw Tray | Formal Config | Save Data |");
            builder.AppendLine("| --- | --- | ---: | ---: | --- | --- | --- | --- | --- | --- | --- | --- | --- |");
            foreach (BattlePrepareShapeAwareItemTrayRow row in plan.shapeAwareItemTrayRows ?? new List<BattlePrepareShapeAwareItemTrayRow>())
            {
                builder.AppendLine(
                    $"| `{Escape(row.shapeId)}` | {Escape(row.trayFootprintChinese)} | {row.footprintWidth} | {row.footprintHeight} | `{Escape(row.occupiedOffsets)}` | `{row.itemTrayShowsTrueShape}` | `{row.selectedHighlightCoversShape}` | `{row.dragVisualKeepsShape}` | `{row.ghostPreviewMatchesTrayShape}` | `{row.usesMatureItemTray}` | `{row.redrawsItemTray}` | `{row.writesFormalConfig}` | `{row.writesSaveData}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## Manual Handtest Checklist");
            builder.AppendLine();
            builder.AppendLine("- Mature BattlePrepare tray still opens through the runtime playtest path.");
            builder.AppendLine("- `Single1` appears as a one-cell shape.");
            builder.AppendLine("- `Vertical2` appears as a two-cell vertical shape in the tray.");
            builder.AppendLine("- `Corner3` appears as a 2x2 footprint missing one cell.");
            builder.AppendLine("- `Square4` appears as a 2x2 square.");
            builder.AppendLine("- Selecting a fixture highlights the occupied cells, not just a single-card icon.");
            builder.AppendLine("- Dragging a fixture moves the same multi-cell tray visual.");
            builder.AppendLine("- Board GhostPreview uses the same shape offsets as the tray visual.");
            builder.AppendLine("- No V04NewItemTray or replacement mature tray appears.");

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildLeakCheckReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattlePrepareComponentAdapterRuntimePlaytestPlan plan,
            int errors,
            int warnings,
            int leakCount)
        {
            string status = errors == 0 && leakCount == 0
                ? "READY_SHAPE_AWARE_ITEMTRAY_PLAYTEST"
                : "SHAPE_AWARE_ITEMTRAY_BLOCKED";

            StringBuilder builder = new();
            builder.AppendLine("# Shape-Aware ItemTray Fixture Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattlePrepareComponentAdapterRuntimePlaytestPlan.ShapeAwareItemTrayPackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{status}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine();
            builder.AppendLine("## Leak Counters");
            builder.AppendLine();
            builder.AppendLine("| Check | Count | Expected |");
            builder.AppendLine("| --- | ---: | ---: |");
            builder.AppendLine($"| `shapeAwareBehaviorLeaks` | {CountShapeAwareBehaviorLeaks(plan)} | 0 |");
            builder.AppendLine($"| `shapeAwareRowLeaks` | {CountShapeAwareRowLeaks(plan)} | 0 |");
            builder.AppendLine($"| `formalSystemLeaks` | {CountFormalSystemLeaks(plan)} | 0 |");
            builder.AppendLine($"| `uiRewriteLeaks` | {CountUiRewriteLeaks(plan)} | 0 |");
            builder.AppendLine($"| `featureFlagDefaultTrue` | {BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue)} | 0 |");
            builder.AppendLine($"| `totalLeaks` | {leakCount} | 0 |");
            builder.AppendLine();
            builder.AppendLine("## Scope Confirmation");
            builder.AppendLine();
            builder.AppendLine("- Shape-aware display is implemented as `ItemTrayShapeExtension` on devOnly fixture item views.");
            builder.AppendLine("- Mature BattlePrepare tray slots, category tabs, scroll view, drag component, and pull-up motion are reused.");
            builder.AppendLine("- The extension does not create `V04NewItemTray` and does not write formal config, save data, RunFlow, PageState, FormationState, Boss, reward, drop, or numeric systems.");

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static int CountLeaks(BattlePrepareComponentAdapterRuntimePlaytestPlan plan)
        {
            return CountShapeAwareBehaviorLeaks(plan)
                + CountShapeAwareRowLeaks(plan)
                + CountFormalSystemLeaks(plan)
                + CountUiRewriteLeaks(plan)
                + BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue);
        }

        private static int CountShapeAwareBehaviorLeaks(BattlePrepareComponentAdapterRuntimePlaytestPlan plan)
        {
            if (plan == null)
            {
                return 1;
            }

            int leaks = 0;
            if (!plan.itemTrayShapeAwareDisplay) leaks++;
            if (!plan.shapeAwareDragVisual) leaks++;
            if (!plan.ghostPreviewMatchesTrayShape) leaks++;
            if (!plan.selectedHighlightCoversShape) leaks++;
            if (!plan.devOnly) leaks++;
            if (plan.isEnabled) leaks++;
            return leaks;
        }

        private static int CountShapeAwareRowLeaks(BattlePrepareComponentAdapterRuntimePlaytestPlan plan)
        {
            return (plan?.shapeAwareItemTrayRows ?? new List<BattlePrepareShapeAwareItemTrayRow>())
                .Count(row => row == null
                    || !row.itemTrayShowsTrueShape
                    || !row.selectedHighlightCoversShape
                    || !row.dragVisualKeepsShape
                    || !row.ghostPreviewMatchesTrayShape
                    || !row.usesMatureItemTray
                    || row.redrawsItemTray
                    || row.writesFormalConfig
                    || row.writesSaveData);
        }

        private static int CountFormalSystemLeaks(BattlePrepareComponentAdapterRuntimePlaytestPlan plan)
        {
            if (plan == null)
            {
                return 1;
            }

            int leaks = 0;
            if (plan.writesFormalScene) leaks++;
            if (plan.writesFormalUi) leaks++;
            if (plan.overwritesHandTunedRectTransform) leaks++;
            if (plan.writesFixtureToFormalPool) leaks++;
            if (plan.writesFixtureToSaveData) leaks++;
            if (plan.touchesRunFlow) leaks++;
            if (plan.touchesPageState) leaks++;
            if (plan.touchesFormationState) leaks++;
            if (plan.touchesSaveData) leaks++;
            if (plan.touchesBossRewardDropNumeric) leaks++;
            return leaks;
        }

        private static int CountUiRewriteLeaks(BattlePrepareComponentAdapterRuntimePlaytestPlan plan)
        {
            if (plan == null)
            {
                return 1;
            }

            int leaks = 0;
            if (plan.redrewV04Board) leaks++;
            if (plan.redrewV04ItemTray) leaks++;
            if (plan.rewroteDragFeel) leaks++;
            if (plan.rewrotePullUpAnimation) leaks++;
            if (plan.promotesTemporaryPreviewUi) leaks++;
            return leaks;
        }

        private static void AppendValidationSummary(
            StringBuilder builder,
            IReadOnlyList<BuildSandboxValidationReport> reports)
        {
            builder.AppendLine();
            builder.AppendLine("## Validation Summary");
            builder.AppendLine();
            builder.AppendLine("| Check | Status | Errors | Warnings | Info |");
            builder.AppendLine("| --- | --- | ---: | ---: | ---: |");
            foreach (BuildSandboxValidationReport report in reports)
            {
                builder.AppendLine(
                    $"| {Escape(report.Name)} | `{(report.Passed ? "PASS" : "FAIL")}` | {report.ErrorCount} | {report.WarningCount} | {report.InfoCount} |");
            }

            builder.AppendLine();
            builder.AppendLine("## Issues");
            builder.AppendLine();
            builder.AppendLine("| Level | Code | Message | Path |");
            builder.AppendLine("| --- | --- | --- | --- |");
            foreach (BuildSandboxValidationIssue issue in reports.SelectMany(report => report.Issues))
            {
                builder.AppendLine(
                    $"| `{issue.Level}` | `{Escape(issue.Code)}` | {Escape(issue.Message)} | `{Escape(issue.AssetPath)}` |");
            }
        }

        private static string Escape(string value)
        {
            return (value ?? string.Empty)
                .Replace("|", "\\|")
                .Replace("\r", " ")
                .Replace("\n", " ");
        }
    }

    public static class ShapeAwareTrayPackingReportWriter
    {
        public const string MainReportPath =
            "Docs/V0.4/Reports/ShapeAwareTrayPackingDragReport.md";

        public const string MapReportPath =
            "Docs/V0.4/Reports/ShapeAwareTrayPackingMapReport.csv";

        public const string LeakCheckReportPath =
            "Docs/V0.4/Reports/ShapeAwareTrayPackingDragLeakCheckReport.md";

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattlePrepareComponentAdapterRuntimePlaytestPlan plan = null)
        {
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();
            BattlePrepareComponentAdapterRuntimePlaytestPlan safePlan =
                plan ?? ShapeAwareTrayPackingValidator.BuildDefaultPlan();
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string mainPath = Path.Combine(projectRoot, MainReportPath);
            string mapPath = Path.Combine(projectRoot, MapReportPath);
            string leakPath = Path.Combine(projectRoot, LeakCheckReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(mainPath) ?? projectRoot);

            int errors = safeReports.Sum(report => report.ErrorCount);
            int warnings = safeReports.Sum(report => report.WarningCount);
            int leakCount = CountLeaks(safePlan);

            File.WriteAllText(
                mainPath,
                BuildMainReport(safeReports, safePlan, errors, warnings, leakCount),
                new UTF8Encoding(false));
            File.WriteAllText(
                mapPath,
                BuildMapReport(safePlan),
                new UTF8Encoding(false));
            File.WriteAllText(
                leakPath,
                BuildLeakCheckReport(safeReports, safePlan, errors, warnings, leakCount),
                new UTF8Encoding(false));

            AssetDatabase.Refresh();
            return new[] { mainPath, mapPath, leakPath };
        }

        private static string BuildMainReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattlePrepareComponentAdapterRuntimePlaytestPlan plan,
            int errors,
            int warnings,
            int leakCount)
        {
            string status = errors == 0 && leakCount == 0
                ? "READY_SHAPE_AWARE_TRAY_PACKING_DRAG_PLAYTEST"
                : "SHAPE_AWARE_TRAY_PACKING_DRAG_BLOCKED";

            StringBuilder builder = new();
            builder.AppendLine("# Shape-Aware Tray Packing Drag Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattlePrepareComponentAdapterRuntimePlaytestPlan.ShapeAwareTrayPackingDragPackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{status}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine($"Leak Count: `{leakCount}`");
            builder.AppendLine();
            builder.AppendLine("## Runtime Entry");
            builder.AppendLine();
            builder.AppendLine($"- Manual Runtime Menu: `{Escape(plan.manualMenuPath)}`");
            builder.AppendLine($"- Fix03 QA Menu: `{Escape(BattlePrepareComponentAdapterRuntimePlaytestPlan.ShapeAwareTrayPackingDragQaMenuPath)}`");
            builder.AppendLine("- Extension: `ItemTrayShapeExtension`");
            builder.AppendLine("- Tray Grid: `5 columns x 8 rows / 40 cells`");
            builder.AppendLine($"- Board Commit: `{BattlePrepareComponentAdapterRuntimePlaytestPlan.DeferredBoardCommitMode}`");
            builder.AppendLine();
            builder.AppendLine("## Packing And Drag Rows");
            builder.AppendLine();
            builder.AppendLine("| Shape | Initial Anchor | Initial Occupied Cells | Move Probe Anchor | Move Probe Cells | In Bounds | Overlap | Drag Persists | Illegal Drop Returns | Board Preview Safe | Redraw Tray | Save/Formal Writes |");
            builder.AppendLine("| --- | ---: | --- | ---: | --- | --- | --- | --- | --- | --- | --- | --- |");
            foreach (BattlePrepareShapeAwareItemTrayRow row in plan.shapeAwareItemTrayRows ?? new List<BattlePrepareShapeAwareItemTrayRow>())
            {
                builder.AppendLine(
                    $"| `{Escape(row.shapeId)}` | {row.initialAnchorSlotIndex} | `{Escape(row.initialOccupiedSlots)}` | {row.trayMoveProbeAnchorSlotIndex} | `{Escape(row.trayMoveProbeOccupiedSlots)}` | `{row.initialPackingInBounds}` | `{row.initialPackingHasOverlap}` | `{row.trayDragStatePersists}` | `{row.illegalTrayDropReturnsToLastLegal}` | `{row.boardPreviewDragDoesNotReturnToWrongInitialPosition}` | `{row.redrawsItemTray}` | `{row.writesFormalConfig || row.writesSaveData}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## Manual Handtest Checklist");
            builder.AppendLine();
            builder.AppendLine("- The mature BattlePrepare item tray is still the V03 tray; no replacement tray is created.");
            builder.AppendLine("- Single1, Vertical2, Corner3, and Square4 initially appear inside the tray bounds without overlapping.");
            builder.AppendLine("- Dragging a multi-cell fixture moves the full footprint, not a one-cell card.");
            builder.AppendLine("- Dropping a fixture on a legal tray position persists after the runtime fixture refresh interval.");
            builder.AppendLine("- Dropping on an illegal tray position returns to the last legal tray anchor.");
            builder.AppendLine("- Dragging over the board keeps the same footprint preview and does not snap back to the wrong initial tray location.");

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildMapReport(BattlePrepareComponentAdapterRuntimePlaytestPlan plan)
        {
            StringBuilder builder = new();
            builder.AppendLine("shapeId,initialAnchorSlot,initialOccupiedSlots,moveProbeAnchorSlot,moveProbeOccupiedSlots,footprintWidth,footprintHeight,occupiedOffsets,initialPackingInBounds,initialPackingHasOverlap,trayPackingUsesOccupiedCells,trayDragStatePersists,illegalTrayDropReturnsToLastLegal,boardPreviewDragDoesNotReturnToWrongInitialPosition,boardCommitMode,usesMatureItemTray,redrawsItemTray,writesFormalConfig,writesSaveData");
            foreach (BattlePrepareShapeAwareItemTrayRow row in plan.shapeAwareItemTrayRows ?? new List<BattlePrepareShapeAwareItemTrayRow>())
            {
                builder.AppendLine(string.Join(
                    ",",
                    Csv(row.shapeId),
                    row.initialAnchorSlotIndex.ToString(),
                    Csv(row.initialOccupiedSlots),
                    row.trayMoveProbeAnchorSlotIndex.ToString(),
                    Csv(row.trayMoveProbeOccupiedSlots),
                    row.footprintWidth.ToString(),
                    row.footprintHeight.ToString(),
                    Csv(row.occupiedOffsets),
                    row.initialPackingInBounds.ToString(),
                    row.initialPackingHasOverlap.ToString(),
                    row.trayPackingUsesOccupiedCells.ToString(),
                    row.trayDragStatePersists.ToString(),
                    row.illegalTrayDropReturnsToLastLegal.ToString(),
                    row.boardPreviewDragDoesNotReturnToWrongInitialPosition.ToString(),
                    Csv(row.boardCommitMode),
                    row.usesMatureItemTray.ToString(),
                    row.redrawsItemTray.ToString(),
                    row.writesFormalConfig.ToString(),
                    row.writesSaveData.ToString()));
            }

            return builder.ToString();
        }

        private static string BuildLeakCheckReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattlePrepareComponentAdapterRuntimePlaytestPlan plan,
            int errors,
            int warnings,
            int leakCount)
        {
            string status = errors == 0 && leakCount == 0
                ? "READY_SHAPE_AWARE_TRAY_PACKING_DRAG_PLAYTEST"
                : "SHAPE_AWARE_TRAY_PACKING_DRAG_BLOCKED";

            StringBuilder builder = new();
            builder.AppendLine("# Shape-Aware Tray Packing Drag Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattlePrepareComponentAdapterRuntimePlaytestPlan.ShapeAwareTrayPackingDragPackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{status}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine();
            builder.AppendLine("## Leak Counters");
            builder.AppendLine();
            builder.AppendLine("| Check | Count | Expected |");
            builder.AppendLine("| --- | ---: | ---: |");
            builder.AppendLine($"| `packingBehaviorLeaks` | {CountPackingBehaviorLeaks(plan)} | 0 |");
            builder.AppendLine($"| `shapeAwareRowLeaks` | {CountShapeAwareRowLeaks(plan)} | 0 |");
            builder.AppendLine($"| `formalSystemLeaks` | {CountFormalSystemLeaks(plan)} | 0 |");
            builder.AppendLine($"| `uiRewriteLeaks` | {CountUiRewriteLeaks(plan)} | 0 |");
            builder.AppendLine($"| `featureFlagDefaultTrue` | {BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue)} | 0 |");
            builder.AppendLine($"| `totalLeaks` | {leakCount} | 0 |");
            builder.AppendLine();
            builder.AppendLine("## Scope Confirmation");
            builder.AppendLine();
            builder.AppendLine("- Fix03 changes runtime devOnly fixture packing and drag-state persistence only.");
            builder.AppendLine("- Mature BattlePrepare tray, grid slots, pull-up animation, and formal UI RectTransforms remain owned by V03.");
            builder.AppendLine("- Board placement commit is documented as deferred to `MobileShapePlacementInteraction01`; this package only preserves the preview/drag state.");

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static int CountLeaks(BattlePrepareComponentAdapterRuntimePlaytestPlan plan)
        {
            return CountPackingBehaviorLeaks(plan)
                + CountShapeAwareRowLeaks(plan)
                + CountFormalSystemLeaks(plan)
                + CountUiRewriteLeaks(plan)
                + BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue);
        }

        private static int CountPackingBehaviorLeaks(BattlePrepareComponentAdapterRuntimePlaytestPlan plan)
        {
            if (plan == null)
            {
                return 1;
            }

            int leaks = 0;
            if (!plan.devOnly) leaks++;
            if (plan.isEnabled) leaks++;
            if (!plan.itemTrayShapeAwareDisplay) leaks++;
            if (!plan.shapeAwareDragVisual) leaks++;
            if (!plan.ghostPreviewMatchesTrayShape) leaks++;
            if (!plan.selectedHighlightCoversShape) leaks++;
            return leaks;
        }

        private static int CountShapeAwareRowLeaks(BattlePrepareComponentAdapterRuntimePlaytestPlan plan)
        {
            return (plan?.shapeAwareItemTrayRows ?? new List<BattlePrepareShapeAwareItemTrayRow>())
                .Count(row => row == null
                    || !row.initialPackingInBounds
                    || row.initialPackingHasOverlap
                    || !row.trayPackingUsesOccupiedCells
                    || !row.trayDragStatePersists
                    || !row.illegalTrayDropReturnsToLastLegal
                    || !row.boardPreviewDragDoesNotReturnToWrongInitialPosition
                    || !row.usesMatureItemTray
                    || row.redrawsItemTray
                    || row.writesFormalConfig
                    || row.writesSaveData);
        }

        private static int CountFormalSystemLeaks(BattlePrepareComponentAdapterRuntimePlaytestPlan plan)
        {
            if (plan == null)
            {
                return 1;
            }

            int leaks = 0;
            if (plan.writesFormalScene) leaks++;
            if (plan.writesFormalUi) leaks++;
            if (plan.overwritesHandTunedRectTransform) leaks++;
            if (plan.writesFixtureToFormalPool) leaks++;
            if (plan.writesFixtureToSaveData) leaks++;
            if (plan.touchesRunFlow) leaks++;
            if (plan.touchesPageState) leaks++;
            if (plan.touchesFormationState) leaks++;
            if (plan.touchesSaveData) leaks++;
            if (plan.touchesBossRewardDropNumeric) leaks++;
            return leaks;
        }

        private static int CountUiRewriteLeaks(BattlePrepareComponentAdapterRuntimePlaytestPlan plan)
        {
            if (plan == null)
            {
                return 1;
            }

            int leaks = 0;
            if (plan.redrewV04Board) leaks++;
            if (plan.redrewV04ItemTray) leaks++;
            if (plan.rewroteDragFeel) leaks++;
            if (plan.rewrotePullUpAnimation) leaks++;
            if (plan.promotesTemporaryPreviewUi) leaks++;
            return leaks;
        }

        private static void AppendValidationSummary(
            StringBuilder builder,
            IReadOnlyList<BuildSandboxValidationReport> reports)
        {
            builder.AppendLine();
            builder.AppendLine("## Validation Summary");
            builder.AppendLine();
            builder.AppendLine("| Check | Status | Errors | Warnings | Info |");
            builder.AppendLine("| --- | --- | ---: | ---: | ---: |");
            foreach (BuildSandboxValidationReport report in reports)
            {
                builder.AppendLine(
                    $"| {Escape(report.Name)} | `{(report.Passed ? "PASS" : "FAIL")}` | {report.ErrorCount} | {report.WarningCount} | {report.InfoCount} |");
            }

            builder.AppendLine();
            builder.AppendLine("## Issues");
            builder.AppendLine();
            builder.AppendLine("| Level | Code | Message | Path |");
            builder.AppendLine("| --- | --- | --- | --- |");
            foreach (BuildSandboxValidationIssue issue in reports.SelectMany(report => report.Issues))
            {
                builder.AppendLine(
                    $"| `{issue.Level}` | `{Escape(issue.Code)}` | {Escape(issue.Message)} | `{Escape(issue.AssetPath)}` |");
            }
        }

        private static string Escape(string value)
        {
            return (value ?? string.Empty)
                .Replace("|", "\\|")
                .Replace("\r", " ")
                .Replace("\n", " ");
        }

        private static string Csv(string value)
        {
            string safeValue = value ?? string.Empty;
            if (safeValue.IndexOfAny(new[] { ',', '"', '\r', '\n' }) < 0)
            {
                return safeValue;
            }

            return $"\"{safeValue.Replace("\"", "\"\"")}\"";
        }
    }
}
#endif
