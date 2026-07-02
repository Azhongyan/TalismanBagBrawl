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
    public static class BattleSandboxShapePlacementVerticalSliceReportWriter
    {
        public const string MainReportPath =
            "Docs/V0.4/Reports/BattleSandboxShapePlacementVerticalSliceReport.md";
        public const string StateReportPath =
            "Docs/V0.4/Reports/BattleSandboxShapePlacementStateReport.csv";
        public const string LeakCheckReportPath =
            "Docs/V0.4/Reports/BattleSandboxShapePlacementLeakCheckReport.md";

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattleSandboxShapePlacementVerticalSlicePlan plan = null)
        {
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();
            BattleSandboxShapePlacementVerticalSlicePlan safePlan =
                plan ?? BattleSandboxShapePlacementVerticalSliceValidator.BuildDefaultPlan();

            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string mainPath = Path.Combine(projectRoot, MainReportPath);
            string statePath = Path.Combine(projectRoot, StateReportPath);
            string leakPath = Path.Combine(projectRoot, LeakCheckReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(mainPath) ?? projectRoot);

            int errors = safeReports.Sum(report => report.ErrorCount);
            int warnings = safeReports.Sum(report => report.WarningCount);
            int leakCount = safePlan.leakRows.Count(row => row.isLeak);

            File.WriteAllText(
                mainPath,
                BuildMainReport(safeReports, safePlan, errors, warnings, leakCount),
                new UTF8Encoding(false));
            File.WriteAllText(
                statePath,
                BuildStateCsv(safePlan),
                new UTF8Encoding(false));
            File.WriteAllText(
                leakPath,
                BuildLeakReport(safeReports, safePlan, errors, warnings, leakCount),
                new UTF8Encoding(false));

            AssetDatabase.Refresh();
            return new[] { mainPath, statePath, leakPath };
        }

        private static string BuildMainReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattleSandboxShapePlacementVerticalSlicePlan plan,
            int errors,
            int warnings,
            int leakCount)
        {
            string status = errors == 0 && leakCount == 0
                ? "PASS_VERTICAL_SLICE_READY"
                : "BLOCKED_VERTICAL_SLICE_ERRORS";
            BattleSandboxShapePlacementSceneSnapshot scene =
                plan.sceneSnapshot ?? new BattleSandboxShapePlacementSceneSnapshot();

            StringBuilder builder = new();
            builder.AppendLine("# Battle Sandbox Shape Placement Vertical Slice Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattleSandboxShapePlacementVerticalSliceValidator.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{status}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine($"Leak Count: `{leakCount}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine($"- Target scene: `{plan.targetScenePath}`.");
            builder.AppendLine("- Visual surface stays in the V04 preview scene and uses the existing battle-like preview area as the V0.2/V0.3 BattlePrepare-facing shell.");
            builder.AppendLine("- Interaction authority is V0.4 ShapePlacement: `ShapePlacementSession`, `ShapeAwareItemTrayGrid`, and `MobileShapePlacementInputExtension`.");
            builder.AppendLine("- The preview tray exposes the primary x2 item plus additional test items; click opens item info only, the item-info popup Rotate changes tray orientation, and drag starts placement.");
            builder.AppendLine("- `Vertical2` is rendered in the tray as a real two-cell span: vertical by default, horizontal after popup Rotate, and back to vertical on the next popup Rotate.");
            builder.AppendLine("- The tray reserves the secondary occupied slot visually, hides its empty card, and draws the spanning item card above slot backgrounds.");
            builder.AppendLine("- Popup Rotate is the only rotation path; dragging, previewing, and board placement cannot rotate.");
            builder.AppendLine("- Valid release commits immediately; invalid release returns the item to tray. There is no Ghost click confirm requirement.");
            builder.AppendLine("- No formal V02/V03 scene, RunFlow, save, Boss, reward, drop, or numeric system is connected by this package.");
            builder.AppendLine();
            builder.AppendLine("## Scene Binding");
            builder.AppendLine();
            builder.AppendLine("| Check | Value |");
            builder.AppendLine("| --- | --- |");
            builder.AppendLine($"| Scene exists | `{scene.sceneExists}` |");
            builder.AppendLine($"| Controller present | `{scene.controllerPresent}` |");
            builder.AppendLine($"| Battle-like preview area | `{scene.battleLikePreviewAreaPresent}` |");
            builder.AppendLine($"| Board grid present | `{scene.boardGridPresent}` |");
            builder.AppendLine($"| Item tray present | `{scene.itemTrayPresent}` |");
            builder.AppendLine($"| Rotate button present | `{scene.rotateButtonPresent}` |");
            builder.AppendLine($"| Cancel button present | `{scene.cancelButtonPresent}` |");
            builder.AppendLine($"| Build Settings contains preview scene | `{scene.buildSettingsContainsPreview}` |");
            builder.AppendLine($"| Board slots | `{scene.boardSlotCount}` |");
            builder.AppendLine($"| Tray slots | `{scene.traySlotCount}` |");
            builder.AppendLine($"| Categories | `{scene.categoryCount}` |");
            builder.AppendLine();
            builder.AppendLine("## Primary X2 Slice");
            builder.AppendLine();
            builder.AppendLine("| Field | Value |");
            builder.AppendLine("| --- | --- |");
            builder.AppendLine($"| Item count | `{plan.previewItemCount}` |");
            builder.AppendLine($"| Item id | `{Escape(plan.previewItemId)}` |");
            builder.AppendLine($"| Shape id | `{Escape(plan.previewShapeId)}` |");
            builder.AppendLine($"| Shape cell count | `{plan.shapeCellCount}` |");
            builder.AppendLine($"| Tray x2 display | `{plan.trayTwoCellDisplay}` |");
            builder.AppendLine($"| Tray x2 visual cell span | `{plan.trayVisualTwoCellDisplay}` |");
            builder.AppendLine($"| Tray vertical default | `{plan.trayVerticalDefault}` |");
            builder.AppendLine($"| Tray horizontal after Rotate | `{plan.trayHorizontalAfterRotate}` |");
            builder.AppendLine($"| Popup Rotate back to vertical | `{plan.trayRotateBackToVertical}` |");
            builder.AppendLine($"| Tray invalid Rotate rejected | `{plan.trayRotationFailsInvalid}` |");
            builder.AppendLine($"| Tray occupied cells | `{Escape(plan.trayOccupiedCells)}` |");
            builder.AppendLine($"| Tray occupied slots | `{Escape(plan.trayOccupiedSlots)}` |");
            builder.AppendLine($"| Rotation allowed | `{plan.shapeRotationAllowed}` |");
            builder.AppendLine();
            builder.AppendLine("## Protocol Summary");
            builder.AppendLine();
            builder.AppendLine("| Check | Value |");
            builder.AppendLine("| --- | --- |");
            builder.AppendLine($"| Uses ShapePlacementSession | `{plan.usesShapePlacementSession}` |");
            builder.AppendLine($"| Uses ShapeAwareItemTrayGrid | `{plan.usesShapeAwareItemTrayGrid}` |");
            builder.AppendLine($"| Uses MobileShapePlacementInputExtension | `{plan.usesMobileShapePlacementInputExtension}` |");
            builder.AppendLine($"| Uses formal V02/V03 runtime core | `{plan.usesFormalRuntimeCore}` |");
            builder.AppendLine($"| Click opens item info only | `{plan.clickItemInfoOnly}` |");
            builder.AppendLine($"| Item card click does not rotate | `{plan.cardClickDoesNotRotate}` |");
            builder.AppendLine($"| Drag starts placement | `{plan.dragStartsPlacement}` |");
            builder.AppendLine($"| Rotate uses item info popup Rotate button | `{plan.rotateUsesExplicitButton}` |");
            builder.AppendLine($"| Popup Rotate only | `{plan.trayRotateOnly}` |");
            builder.AppendLine($"| No rotation while dragging | `{plan.noRotationWhileDragging}` |");
            builder.AppendLine($"| No rotation on board | `{plan.noRotationOnBoard}` |");
            builder.AppendLine($"| Release commits if valid | `{plan.releaseCommitsIfValid}` |");
            builder.AppendLine($"| Release returns to tray if invalid | `{plan.releaseReturnsToTrayIfInvalid}` |");
            builder.AppendLine($"| No Ghost click confirm | `{plan.noGhostClickConfirm}` |");
            builder.AppendLine($"| Cancel available | `{plan.cancelAvailable}` |");
            builder.AppendLine();
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildStateCsv(BattleSandboxShapePlacementVerticalSlicePlan plan)
        {
            StringBuilder builder = new();
            builder.AppendLine("sampleId,action,fromInputState,toInputState,expectedInputState,fromSessionState,toSessionState,expectedSessionState,expectedValid,actualValid,anchor,occupiedCells,invalidReason,ghostVisible,receiverCommitBefore,receiverCommitAfter,note");
            foreach (BattleSandboxShapePlacementStateRow row in plan.rows ?? new List<BattleSandboxShapePlacementStateRow>())
            {
                builder.AppendLine(string.Join(
                    ",",
                    Csv(row.sampleId),
                    Csv(row.action),
                    row.fromInputState.ToString(),
                    row.toInputState.ToString(),
                    row.expectedInputState.ToString(),
                    row.fromSessionState.ToString(),
                    row.toSessionState.ToString(),
                    row.expectedSessionState.ToString(),
                    row.expectedValid.ToString(),
                    row.actualValid.ToString(),
                    Csv(row.anchor),
                    Csv(row.occupiedCells),
                    row.invalidReason.ToString(),
                    row.ghostVisible.ToString(),
                    row.receiverCommitBefore.ToString(),
                    row.receiverCommitAfter.ToString(),
                    Csv(row.note)));
            }

            return builder.ToString();
        }

        private static string BuildLeakReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattleSandboxShapePlacementVerticalSlicePlan plan,
            int errors,
            int warnings,
            int leakCount)
        {
            string status = errors == 0 && leakCount == 0
                ? "PASS_DEVONLY_ISOLATED"
                : "BLOCKED_LEAK_CHECK";

            StringBuilder builder = new();
            builder.AppendLine("# Battle Sandbox Shape Placement Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattleSandboxShapePlacementVerticalSliceValidator.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{status}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine($"Leak Count: `{leakCount}`");
            builder.AppendLine();
            builder.AppendLine("## Leak Rows");
            builder.AppendLine();
            builder.AppendLine("| Check | Leak | Asset | Detail |");
            builder.AppendLine("| --- | --- | --- | --- |");
            foreach (BattleSandboxShapePlacementLeakRow row in plan.leakRows ?? new List<BattleSandboxShapePlacementLeakRow>())
            {
                builder.AppendLine(
                    $"| `{Escape(row.checkId)}` | `{row.isLeak}` | `{Escape(row.assetPath)}` | {Escape(row.detail)} |");
            }

            builder.AppendLine();
            builder.AppendLine("## Formal Scope Confirmation");
            builder.AppendLine();
            builder.AppendLine("- `Scene_TalismanBag_V02_FormationCounter` is not a target of this package.");
            builder.AppendLine("- `Scene_TalismanBag_V03_MainHome` is not a target of this package.");
            builder.AppendLine("- `Scene_TalismanBag_V03_TalismanUpgrade` is not a target of this package.");
            builder.AppendLine("- Formal RunFlow, PageState, FormationState, save data, Boss, reward, drop, and numeric systems are not touched.");
            builder.AppendLine("- The V0.4 preview scene remains outside Build Settings.");
            builder.AppendLine("- The old V02/V03 BattlePrepare runtime core is not used as placement authority.");
            builder.AppendLine();
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static void AppendValidationSummary(
            StringBuilder builder,
            IReadOnlyList<BuildSandboxValidationReport> reports)
        {
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

            builder.AppendLine();
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
            string safe = value ?? string.Empty;
            if (safe.IndexOfAny(new[] { ',', '"', '\r', '\n' }) < 0)
            {
                return safe;
            }

            return $"\"{safe.Replace("\"", "\"\"")}\"";
        }
    }
}
#endif
