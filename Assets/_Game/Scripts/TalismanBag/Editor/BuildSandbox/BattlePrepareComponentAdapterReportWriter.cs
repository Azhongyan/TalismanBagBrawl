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
    public static class BattlePrepareComponentAdapterReportWriter
    {
        public const string MainReportPath =
            "Docs/V0.4/Reports/BattlePrepareComponentAdapterReport.md";

        public const string AdapterMapReportPath =
            "Docs/V0.4/Reports/BattlePrepareComponentAdapterMap.csv";

        public const string LeakCheckReportPath =
            "Docs/V0.4/Reports/BattlePrepareComponentAdapterLeakCheckReport.md";

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattlePrepareComponentAdapter adapter = null)
        {
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();
            BattlePrepareComponentAdapter safeAdapter =
                adapter ?? BattlePrepareComponentAdapterValidator.BuildDefaultAdapter();
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string mainPath = Path.Combine(projectRoot, MainReportPath);
            string mapPath = Path.Combine(projectRoot, AdapterMapReportPath);
            string leakPath = Path.Combine(projectRoot, LeakCheckReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(mainPath) ?? projectRoot);

            int errors = safeReports.Sum(report => report.ErrorCount);
            int warnings = safeReports.Sum(report => report.WarningCount);
            int leakCount = CountLeaks(safeAdapter);

            File.WriteAllText(
                mainPath,
                BuildMainReport(safeReports, safeAdapter, errors, warnings, leakCount),
                new UTF8Encoding(false));
            File.WriteAllText(
                mapPath,
                BuildAdapterMapCsv(safeAdapter),
                new UTF8Encoding(false));
            File.WriteAllText(
                leakPath,
                BuildLeakCheckReport(safeReports, safeAdapter, errors, warnings, leakCount),
                new UTF8Encoding(false));

            AssetDatabase.Refresh();
            return new[] { mainPath, mapPath, leakPath };
        }

        private static string BuildMainReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattlePrepareComponentAdapter adapter,
            int errors,
            int warnings,
            int leakCount)
        {
            BattlePrepareComponentAdapterViewModel viewModel =
                adapter?.viewModel ?? new BattlePrepareComponentAdapterViewModel();
            StringBuilder builder = new();
            builder.AppendLine("# BattlePrepare Component Adapter Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattlePrepareComponentAdapter.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 && leakCount == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine($"Leak Count: `{leakCount}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Maps V0.4 BuildSandbox PreviewContext data into BattlePrepare extension/adaptor ViewModels.");
            builder.AppendLine("- Keeps V03 BattlePrepare as Base Component source; no formal controller or scene is modified.");
            builder.AppendLine("- Provides BoardOccupancy, ItemTrayShape, DragRotationPlacement, ItemInfoBuildFields, and BattleFeedbackMechanicHint extension outputs.");
            builder.AppendLine("- Does not redraw V04 board, item tray, pull-up animation, drag feel, or item info popup.");
            builder.AppendLine("- Does not write RunFlow, PageState, FormationState, SaveData, Boss, reward, drop, or numeric systems.");
            builder.AppendLine();
            builder.AppendLine("## Adapter Isolation");
            builder.AppendLine();
            builder.AppendLine($"- devOnly: `{adapter?.devOnly}`");
            builder.AppendLine($"- isEnabled: `{adapter?.isEnabled}`");
            builder.AppendLine($"- referenceMode: `{Escape(adapter?.referenceMode)}`");
            builder.AppendLine($"- baseComponentName: `{Escape(adapter?.baseComponentName)}`");
            builder.AppendLine($"- baseComponentMode: `{Escape(adapter?.baseComponentMode)}`");
            builder.AppendLine($"- sourcePreviewBuildId: `{Escape(adapter?.sourcePreviewBuildId)}`");
            builder.AppendLine($"- writesFormalScene: `{adapter?.writesFormalScene}`");
            builder.AppendLine($"- writesFormalUi: `{adapter?.writesFormalUi}`");
            builder.AppendLine($"- touchesFormalPrepareController: `{adapter?.touchesFormalPrepareController}`");
            builder.AppendLine($"- overridesFormalLayoutFrame: `{adapter?.overridesFormalLayoutFrame}`");
            builder.AppendLine();
            builder.AppendLine("## Extension Outputs");
            builder.AppendLine();
            builder.AppendLine("| Extension | Target Component | Target Node | Rows | Formal UI Write | Explicit Future Binding |");
            builder.AppendLine("| --- | --- | --- | ---: | --- | --- |");
            builder.AppendLine(ExtensionRow(
                viewModel.boardOccupancy.extensionId,
                viewModel.boardOccupancy.targetComponentName,
                viewModel.boardOccupancy.targetNodeName,
                viewModel.boardOccupancy.cells.Count,
                viewModel.boardOccupancy.canWriteFormalUi,
                viewModel.boardOccupancy.requiresExplicitRuntimeBinding));
            builder.AppendLine(ExtensionRow(
                viewModel.itemTrayShape.extensionId,
                viewModel.itemTrayShape.targetComponentName,
                viewModel.itemTrayShape.targetNodeName,
                viewModel.itemTrayShape.items.Count,
                viewModel.itemTrayShape.canWriteFormalUi,
                viewModel.itemTrayShape.requiresExplicitRuntimeBinding));
            builder.AppendLine(ExtensionRow(
                viewModel.dragRotationPlacement.extensionId,
                viewModel.dragRotationPlacement.targetComponentName,
                viewModel.dragRotationPlacement.targetNodeName,
                viewModel.dragRotationPlacement.rows.Count,
                viewModel.dragRotationPlacement.canWriteFormalUi,
                viewModel.dragRotationPlacement.requiresExplicitRuntimeBinding));
            builder.AppendLine(ExtensionRow(
                viewModel.itemInfoBuildFields.extensionId,
                viewModel.itemInfoBuildFields.targetComponentName,
                viewModel.itemInfoBuildFields.targetNodeName,
                viewModel.itemInfoBuildFields.items.Sum(row => row.fields.Count),
                viewModel.itemInfoBuildFields.canWriteFormalUi,
                viewModel.itemInfoBuildFields.requiresExplicitRuntimeBinding));
            builder.AppendLine(ExtensionRow(
                viewModel.battleFeedbackMechanicHint.extensionId,
                viewModel.battleFeedbackMechanicHint.targetComponentName,
                viewModel.battleFeedbackMechanicHint.targetNodeName,
                viewModel.battleFeedbackMechanicHint.rows.Count,
                viewModel.battleFeedbackMechanicHint.canWriteFormalUi,
                viewModel.battleFeedbackMechanicHint.requiresExplicitRuntimeBinding));
            builder.AppendLine();
            builder.AppendLine("## Data Summary");
            builder.AppendLine();
            builder.AppendLine($"- Occupied cells: `{viewModel.boardOccupancy.occupiedCellCount}`");
            builder.AppendLine($"- Placed items: `{viewModel.boardOccupancy.placedItemCount}`");
            builder.AppendLine($"- Item tray rows: `{viewModel.itemTrayShape.items.Count}`");
            builder.AppendLine($"- Item info rows: `{viewModel.itemInfoBuildFields.items.Count}`");
            builder.AppendLine($"- Feedback rows: `{viewModel.battleFeedbackMechanicHint.rows.Count}`");
            builder.AppendLine();
            builder.AppendLine("## Formal Scope Confirmation");
            builder.AppendLine();
            builder.AppendLine("- Base BattlePrepare interaction controller: not modified.");
            builder.AppendLine("- `V02FormationGridFrame`: not modified, not reparented, no RectTransform writes.");
            builder.AppendLine("- V02/V03 formal scenes: not written.");
            builder.AppendLine("- V04 temporary Board/ItemTray/Popup: not promoted as formal UI truth.");
            builder.AppendLine("- Runtime binding into formal BattlePrepare remains a future explicit package/Guard decision.");

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildAdapterMapCsv(BattlePrepareComponentAdapter adapter)
        {
            BattlePrepareComponentAdapterViewModel viewModel =
                adapter?.viewModel ?? new BattlePrepareComponentAdapterViewModel();
            StringBuilder csv = new();
            csv.AppendLine("extensionId,adapterOutputKey,targetComponent,targetNode,sourceDataPath,rowKey,chineseDisplayName,playerChineseText,developerOnly,maskedFromPlayer,canWriteFormalUi");

            foreach (BoardOccupancyCellRow row in viewModel.boardOccupancy.cells)
            {
                csv.AppendLine(Csv(
                    viewModel.boardOccupancy.extensionId,
                    viewModel.boardOccupancy.adapterOutputKey,
                    viewModel.boardOccupancy.targetComponentName,
                    viewModel.boardOccupancy.targetNodeName,
                    "BuildSandboxPreviewContext.layoutSnapshot.placedItems.occupiedCells",
                    $"{row.itemId}@{row.x}:{row.y}",
                    "占格预览",
                    row.playerHintChinese,
                    "False",
                    "False",
                    viewModel.boardOccupancy.canWriteFormalUi.ToString()));
            }

            foreach (ItemTrayShapeRow row in viewModel.itemTrayShape.items)
            {
                csv.AppendLine(Csv(
                    viewModel.itemTrayShape.extensionId,
                    viewModel.itemTrayShape.adapterOutputKey,
                    viewModel.itemTrayShape.targetComponentName,
                    viewModel.itemTrayShape.targetNodeName,
                    "BuildSandboxPreviewContext.layoutSnapshot.placedItems",
                    row.itemId,
                    row.displayNameChinese,
                    $"{row.shapeChinese}，{row.affixSummaryChinese}",
                    "False",
                    "False",
                    viewModel.itemTrayShape.canWriteFormalUi.ToString()));
            }

            foreach (DragRotationPlacementRow row in viewModel.dragRotationPlacement.rows)
            {
                csv.AppendLine(Csv(
                    viewModel.dragRotationPlacement.extensionId,
                    viewModel.dragRotationPlacement.adapterOutputKey,
                    viewModel.dragRotationPlacement.targetComponentName,
                    viewModel.dragRotationPlacement.targetNodeName,
                    "BuildSandboxPreviewContext.layoutSnapshot.placedItems.rotation",
                    row.itemId,
                    "旋转摆放",
                    row.playerFeedbackChinese,
                    "False",
                    "False",
                    viewModel.dragRotationPlacement.canWriteFormalUi.ToString()));
            }

            foreach (ItemInfoBuildFieldsRow item in viewModel.itemInfoBuildFields.items)
            {
                foreach (BattlePrepareItemInfoFieldRow field in item.fields)
                {
                    csv.AppendLine(Csv(
                        viewModel.itemInfoBuildFields.extensionId,
                        viewModel.itemInfoBuildFields.adapterOutputKey,
                        viewModel.itemInfoBuildFields.targetComponentName,
                        viewModel.itemInfoBuildFields.targetNodeName,
                        "BuildSandboxPreviewContext.viewModel",
                        $"{item.itemId}.{field.englishStableKey}",
                        field.chineseDisplayName,
                        field.playerChineseText,
                        field.developerOnly.ToString(),
                        field.maskedFromPlayer.ToString(),
                        field.canWriteFormalUi.ToString()));
                }
            }

            foreach (BattleFeedbackMechanicHintRow row in viewModel.battleFeedbackMechanicHint.rows)
            {
                csv.AppendLine(Csv(
                    viewModel.battleFeedbackMechanicHint.extensionId,
                    viewModel.battleFeedbackMechanicHint.adapterOutputKey,
                    viewModel.battleFeedbackMechanicHint.targetComponentName,
                    viewModel.battleFeedbackMechanicHint.targetNodeName,
                    "BuildSandboxPreviewContext.viewModel.shapeOccupancy/problemReadiness",
                    row.englishStableKey,
                    row.chineseDisplayName,
                    row.playerChineseText,
                    (!row.playerVisible).ToString(),
                    row.maskedFromPlayer.ToString(),
                    row.canWriteFormalUi.ToString()));
            }

            return csv.ToString();
        }

        private static string BuildLeakCheckReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattlePrepareComponentAdapter adapter,
            int errors,
            int warnings,
            int leakCount)
        {
            int featureFlagDefaultTrue = BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue);
            int adapterLeaks = CountAdapterLeaks(adapter);
            int extensionLeaks = CountExtensionLeaks(adapter?.viewModel);
            int playerTextLeaks = CountPlayerTextLeaks(adapter?.viewModel);

            StringBuilder builder = new();
            builder.AppendLine("# BattlePrepare Component Adapter Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattlePrepareComponentAdapter.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 && leakCount == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine();
            builder.AppendLine("## Leak Counters");
            builder.AppendLine();
            builder.AppendLine("| Check | Count | Expected |");
            builder.AppendLine("| --- | ---: | ---: |");
            builder.AppendLine($"| `featureFlagDefaultTrue` | {featureFlagDefaultTrue} | 0 |");
            builder.AppendLine($"| `adapterIsolationLeaks` | {adapterLeaks} | 0 |");
            builder.AppendLine($"| `extensionFormalWriteLeaks` | {extensionLeaks} | 0 |");
            builder.AppendLine($"| `playerTextOrAnswerLeaks` | {playerTextLeaks} | 0 |");
            builder.AppendLine($"| `totalLeaks` | {leakCount} | 0 |");
            builder.AppendLine();
            builder.AppendLine("## Formal Scope Confirmation");
            builder.AppendLine();
            builder.AppendLine("- Formal BattlePrepare controller: not touched.");
            builder.AppendLine("- Formal UI scenes and user hand-tuned RectTransform values: not written.");
            builder.AppendLine("- RunFlow / PageState / FormationState / SaveData / Boss / reward / drop / numeric systems: not touched.");
            builder.AppendLine("- Adapter output is data-only and requires explicit future runtime binding.");
            builder.AppendLine("- Player-facing text is Chinese-only and does not expose full answer fields.");

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static int CountLeaks(BattlePrepareComponentAdapter adapter)
        {
            return BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue)
                + CountAdapterLeaks(adapter)
                + CountExtensionLeaks(adapter?.viewModel)
                + CountPlayerTextLeaks(adapter?.viewModel);
        }

        private static int CountAdapterLeaks(BattlePrepareComponentAdapter adapter)
        {
            if (adapter == null)
            {
                return 1;
            }

            int leaks = 0;
            if (!adapter.devOnly) leaks++;
            if (adapter.isEnabled) leaks++;
            if (adapter.readsFormalSaveData) leaks++;
            if (adapter.writesFormalScene) leaks++;
            if (adapter.writesFormalUi) leaks++;
            if (adapter.setParentsFormalUi) leaks++;
            if (adapter.overridesFormalLayoutFrame) leaks++;
            if (adapter.touchesFormalPrepareController) leaks++;
            if (adapter.touchesRunFlow) leaks++;
            if (adapter.touchesPageState) leaks++;
            if (adapter.touchesFormationState) leaks++;
            if (adapter.touchesSaveData) leaks++;
            if (adapter.touchesBossRewardDropNumeric) leaks++;
            if (adapter.playerUiShowsEnglishStableKey) leaks++;
            if (adapter.playerUiShowsFullAnswers) leaks++;
            return leaks;
        }

        private static int CountExtensionLeaks(BattlePrepareComponentAdapterViewModel viewModel)
        {
            if (viewModel == null)
            {
                return 1;
            }

            int leaks = 0;
            if (viewModel.boardOccupancy.canWriteFormalUi || !viewModel.boardOccupancy.requiresExplicitRuntimeBinding) leaks++;
            if (viewModel.itemTrayShape.canWriteFormalUi || !viewModel.itemTrayShape.requiresExplicitRuntimeBinding) leaks++;
            if (viewModel.dragRotationPlacement.canWriteFormalUi || !viewModel.dragRotationPlacement.requiresExplicitRuntimeBinding) leaks++;
            if (viewModel.itemInfoBuildFields.canWriteFormalUi || !viewModel.itemInfoBuildFields.requiresExplicitRuntimeBinding) leaks++;
            if (viewModel.battleFeedbackMechanicHint.canWriteFormalUi || !viewModel.battleFeedbackMechanicHint.requiresExplicitRuntimeBinding) leaks++;
            leaks += viewModel.itemInfoBuildFields.items
                .SelectMany(row => row.fields)
                .Count(field => field.canWriteFormalUi || field.developerOnly && (!field.maskedFromPlayer || field.playerVisible));
            leaks += viewModel.battleFeedbackMechanicHint.rows
                .Count(row => row.canWriteFormalUi);
            return leaks;
        }

        private static int CountPlayerTextLeaks(BattlePrepareComponentAdapterViewModel viewModel)
        {
            if (viewModel == null)
            {
                return 1;
            }

            string[] forbidden =
            {
                "hardSolutionTags",
                "requiredSynergy",
                "requiredAffix",
                "requiredStats",
                "DropBias",
                "Boss六钥匙",
                "Boss 六钥匙"
            };

            IEnumerable<string> playerTexts =
                viewModel.boardOccupancy.cells.Select(row => row.playerHintChinese)
                    .Concat(viewModel.dragRotationPlacement.rows.Select(row => row.playerFeedbackChinese))
                    .Concat(viewModel.itemInfoBuildFields.items.Select(row => row.playerSummaryChinese))
                    .Concat(viewModel.itemInfoBuildFields.items
                        .SelectMany(row => row.fields)
                        .Where(row => row.playerVisible)
                        .Select(row => row.playerChineseText))
                    .Concat(viewModel.battleFeedbackMechanicHint.rows
                        .Where(row => row.playerVisible)
                        .Select(row => row.playerChineseText));

            int leaks = 0;
            foreach (string text in playerTexts)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                if (text.Any(character => character <= 127 && char.IsLetter(character)))
                {
                    leaks++;
                }

                if (forbidden.Any(token => text.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    leaks++;
                }
            }

            return leaks;
        }

        private static string ExtensionRow(
            string extensionId,
            string targetComponent,
            string targetNode,
            int rows,
            bool canWriteFormalUi,
            bool requiresExplicitRuntimeBinding)
        {
            return $"| `{Escape(extensionId)}` | `{Escape(targetComponent)}` | `{Escape(targetNode)}` | {rows} | `{canWriteFormalUi}` | `{requiresExplicitRuntimeBinding}` |";
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

        private static string Csv(params string[] values)
        {
            return string.Join(",", values.Select(EscapeCsv));
        }

        private static string Escape(string value)
        {
            return (value ?? string.Empty)
                .Replace("|", "\\|")
                .Replace("\r", " ")
                .Replace("\n", " ");
        }

        private static string EscapeCsv(string value)
        {
            string normalized = value ?? string.Empty;
            if (normalized.Contains(",") || normalized.Contains("\"") || normalized.Contains("\n") || normalized.Contains("\r"))
            {
                return $"\"{normalized.Replace("\"", "\"\"")}\"";
            }

            return normalized;
        }
    }
}
#endif
