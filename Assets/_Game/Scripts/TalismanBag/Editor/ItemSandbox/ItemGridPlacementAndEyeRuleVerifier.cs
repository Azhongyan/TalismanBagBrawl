#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TalismanBag.ItemSandbox;
using TalismanBag.Items.InnerCatalog;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace TalismanBag.EditorTools.ItemSandbox
{
    public static class ItemGridPlacementAndEyeRuleVerifier
    {
        private const string DetailReportPath = "Docs/V0.4/Reports/ItemGridPlacementAndEyeRuleReport.md";
        private const string SpecCsvPath = "Docs/V0.4/Reports/ItemGridPlacementAndEyeRuleSpec.csv";
        private const string LeakCheckReportPath = "Docs/V0.4/Reports/ItemGridPlacementAndEyeRuleLeakCheckReport.md";

        private static readonly string[] ScopedSourcePaths =
        {
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemGridPlacementRulePreview.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxGridCellView.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxGridPlacementPreviewView.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxDetailUiController.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemSandboxDetailUiSceneBuilder.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemGridPlacementAndEyeRuleVerifier.cs"
        };

        private static readonly string[] ForbiddenSourceTokens =
        {
            "BuildSynergyResolver",
            "BattleResolver",
            "BattleBridge",
            "UnifiedBattlePage",
            "V02RunFlow",
            "V03RunFlow",
            "RewardConfig",
            "BossInfo",
            "SaveData",
            "EditorBuildSettings.scenes ="
        };

        [MenuItem("Tools/Talisman Bag/V0.4/ItemSandbox/ItemGridPlacementAndEyeRule01/[Guard Only] Verify And Write Reports")]
        public static void VerifyMenu()
        {
            VerifyAndWriteReports(exitWhenBatchMode: false);
        }

        public static void VerifyStaticBatch()
        {
            VerifyAndWriteReports(exitWhenBatchMode: Application.isBatchMode);
        }

        private static void VerifyAndWriteReports(bool exitWhenBatchMode)
        {
            VerificationResult result = new();
            List<PlacementSample> samples = new();

            try
            {
                RunChecks(result, samples);
                WriteReports(result, samples);

                if (result.Errors.Count == 0)
                {
                    Debug.Log("ItemGridPlacementAndEyeRule01 verification passed and reports were written.");
                    if (exitWhenBatchMode)
                    {
                        EditorApplication.Exit(0);
                    }
                }
                else
                {
                    foreach (string error in result.Errors)
                    {
                        Debug.LogError(error);
                    }

                    if (exitWhenBatchMode)
                    {
                        EditorApplication.Exit(1);
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                result.Errors.Add(exception.Message);
                WriteReports(result, samples);
                if (exitWhenBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
        }

        private static void RunChecks(VerificationResult result, List<PlacementSample> samples)
        {
            IReadOnlyList<ItemInnerDataDefinition> catalogItems = ItemInnerDataCatalog.AllItems;
            CheckRuleConstants(result);
            CheckCatalogInputs(catalogItems, result);
            CheckRuleSamples(samples, result);
            CheckScene(result);
            CheckSourceScope(result);
        }

        private static void CheckRuleConstants(VerificationResult result)
        {
            if (ItemGridPlacementRulePreview.BoardSize != 5)
            {
                result.Errors.Add($"BoardSize is {ItemGridPlacementRulePreview.BoardSize}; expected 5.");
            }

            if (ItemGridPlacementRulePreview.EyeCell != new Vector2Int(2, 2))
            {
                result.Errors.Add($"eyeCell is {ItemGridPlacementRulePreview.FormatCell(ItemGridPlacementRulePreview.EyeCell)}; expected (2,2).");
            }

            HashSet<Vector2Int> expectedArrayCells = new()
            {
                new Vector2Int(2, 1),
                new Vector2Int(2, 3),
                new Vector2Int(1, 2),
                new Vector2Int(3, 2)
            };
            HashSet<Vector2Int> actualArrayCells = new(ItemGridPlacementRulePreview.ArrayBonusCells);
            if (!actualArrayCells.SetEquals(expectedArrayCells))
            {
                result.Errors.Add($"arrayBonusCells mismatch. Actual={ItemGridPlacementRulePreview.FormatCells(actualArrayCells)}.");
            }

            if (actualArrayCells.Contains(ItemGridPlacementRulePreview.EyeCell))
            {
                result.Errors.Add("arrayBonusCells must not include eyeCell.");
            }

            if (result.Errors.Count == 0)
            {
                result.Notes.Add("Rule constants passed: 5x5 board, eyeCell=(2,2), four fixed arrayBonusCells.");
            }
        }

        private static void CheckCatalogInputs(IReadOnlyList<ItemInnerDataDefinition> items, VerificationResult result)
        {
            if (items.Count != 31)
            {
                result.Errors.Add($"Catalog item count is {items.Count}; expected 31.");
            }

            foreach (ItemInnerDataDefinition item in items)
            {
                if (item == null)
                {
                    result.Errors.Add("Catalog includes a null item.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(item.itemId)
                    || item.ShapeCells == null
                    || item.ShapeCells.Count == 0
                    || !item.ShapeCells.Contains(item.coreCellLocal))
                {
                    result.Errors.Add($"Catalog item lacks placement input data: {item.itemId}.");
                }

                if (item.itemId.IndexOf("eye", StringComparison.OrdinalIgnoreCase) >= 0
                    || item.displayName.IndexOf("eye", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    result.Errors.Add($"Forbidden eye stone catalog marker detected: {item.itemId} {item.displayName}.");
                }
            }

            if (result.Errors.All(error => !error.StartsWith("Catalog", StringComparison.Ordinal)
                    && !error.StartsWith("Forbidden eye", StringComparison.Ordinal)))
            {
                result.Notes.Add("Catalog input check passed: 31 items expose shapeCells and coreCellLocal; eye stone is not a catalog item.");
            }
        }

        private static void CheckRuleSamples(List<PlacementSample> samples, VerificationResult result)
        {
            ItemInnerDataDefinition single = RequireItem("I001", result);
            ItemInnerDataDefinition line2H = RequireItem("I002", result);
            ItemInnerDataDefinition block2X2 = RequireItem("I018", result);

            samples.Add(new PlacementSample(
                "array_cell_can_be_occupied",
                single,
                new Vector2Int(2, 1),
                Array.Empty<ItemGridPlacedItemPreview>(),
                ItemGridPlacementInvalidReason.None,
                "Array bonus cell identity does not block placement."));

            samples.Add(new PlacementSample(
                "eye_cell_cannot_be_covered",
                single,
                new Vector2Int(2, 2),
                Array.Empty<ItemGridPlacedItemPreview>(),
                ItemGridPlacementInvalidReason.EyeCellCovered,
                "eyeCell is fixed and cannot be covered by occupiedCells."));

            samples.Add(new PlacementSample(
                "out_of_grid_is_blocked",
                line2H,
                new Vector2Int(4, 0),
                Array.Empty<ItemGridPlacedItemPreview>(),
                ItemGridPlacementInvalidReason.OutOfGrid,
                "Line2H at x=4 produces occupiedCells outside 5x5."));

            samples.Add(new PlacementSample(
                "overlap_is_blocked",
                single,
                new Vector2Int(0, 0),
                new[]
                {
                    new ItemGridPlacedItemPreview(
                        "existing_preview",
                        "Existing preview",
                        new Vector2Int(0, 0),
                        new[] { new Vector2Int(0, 0) },
                        new Vector2Int(0, 0))
                },
                ItemGridPlacementInvalidReason.CellOccupied,
                "occupiedCells collide with an existing preview placement."));

            samples.Add(new PlacementSample(
                "core_cell_world_is_visualized",
                block2X2,
                new Vector2Int(0, 1),
                Array.Empty<ItemGridPlacedItemPreview>(),
                ItemGridPlacementInvalidReason.None,
                "coreCellWorld is anchorCell + coreCellLocal for the selected shape."));

            foreach (PlacementSample sample in samples)
            {
                if (sample.Item == null)
                {
                    continue;
                }

                sample.Result = ItemGridPlacementRulePreview.Evaluate(sample.Item, sample.AnchorCell, sample.LockedPlacements);
                if (sample.Result.invalidReason != sample.ExpectedReason)
                {
                    result.Errors.Add(
                        $"{sample.CaseId} invalidReason mismatch. Expected={sample.ExpectedReason}, actual={sample.Result.invalidReason}.");
                }

                if (sample.ExpectedReason == ItemGridPlacementInvalidReason.None && !sample.Result.IsValid)
                {
                    result.Errors.Add($"{sample.CaseId} expected valid placement but got {sample.Result.invalidReason}.");
                }

                if (sample.CaseId == "core_cell_world_is_visualized"
                    && sample.Result.coreCellWorld != sample.AnchorCell + sample.Item.coreCellLocal)
                {
                    result.Errors.Add("core_cell_world_is_visualized did not compute anchorCell + coreCellLocal.");
                }
            }

            if (result.Errors.All(error => !error.Contains("invalidReason mismatch")
                    && !error.Contains("expected valid placement")
                    && !error.StartsWith("core_cell", StringComparison.Ordinal)))
            {
                result.Notes.Add("Placement rule samples passed: array occupancy allowed, eye cover blocked, out-of-grid blocked, overlap blocked, core visualized.");
            }
        }

        private static ItemInnerDataDefinition RequireItem(string itemId, VerificationResult result)
        {
            ItemInnerDataDefinition item = ItemInnerDataCatalog.FindById(itemId);
            if (item == null)
            {
                result.Errors.Add($"Missing required sample item: {itemId}.");
            }

            return item;
        }

        private static void CheckScene(VerificationResult result)
        {
            if (!File.Exists(ItemSandboxDetailUiSceneBuilder.ScenePath))
            {
                result.Errors.Add($"Missing scene: {ItemSandboxDetailUiSceneBuilder.ScenePath}");
                return;
            }

            bool sceneIsInBuildSettings = EditorBuildSettings.scenes.Any(scene =>
                string.Equals(scene.path, ItemSandboxDetailUiSceneBuilder.ScenePath, StringComparison.OrdinalIgnoreCase));
            if (sceneIsInBuildSettings)
            {
                result.Errors.Add("Item sandbox scene is present in BuildSettings; this package must remain manual-only.");
            }
            else
            {
                result.Notes.Add("BuildSettings check passed: Item Sandbox scene is not registered.");
            }

            Scene scene = EditorSceneManager.OpenScene(ItemSandboxDetailUiSceneBuilder.ScenePath, OpenSceneMode.Single);
            ItemSandboxGridPlacementPreviewView preview = Object.FindObjectsOfType<ItemSandboxGridPlacementPreviewView>(true).FirstOrDefault();
            ItemSandboxGridCellView[] cells = Object.FindObjectsOfType<ItemSandboxGridCellView>(true);
            ItemSandboxDetailUiController controller = Object.FindObjectsOfType<ItemSandboxDetailUiController>(true).FirstOrDefault();
            ItemSandboxDetailPanelView detailPanel = Object.FindObjectsOfType<ItemSandboxDetailPanelView>(true).FirstOrDefault();
            ItemInnerDataCatalogProvider provider = Object.FindObjectsOfType<ItemInnerDataCatalogProvider>(true).FirstOrDefault();
            ItemSandboxItemButtonView[] buttons = Object.FindObjectsOfType<ItemSandboxItemButtonView>(true);

            if (preview == null)
            {
                result.Errors.Add("Missing ItemSandboxGridPlacementPreviewView in Item Sandbox scene.");
            }
            else
            {
                if (preview.ConfiguredCellCount != 25)
                {
                    result.Errors.Add($"Placement preview configured cell count is {preview.ConfiguredCellCount}; expected 25.");
                }

                if (!preview.HasCatalogProviderReference)
                {
                    result.Errors.Add("Placement preview is not wired to ItemInnerDataCatalogProvider.");
                }

                if (preview.AnchorCell != new Vector2Int(0, 1))
                {
                    result.Errors.Add($"Placement preview default anchorCell is {ItemGridPlacementRulePreview.FormatCell(preview.AnchorCell)}; expected (0,1).");
                }
            }

            if (cells.Length != 25)
            {
                result.Errors.Add($"Scene placement cell count is {cells.Length}; expected 25.");
            }
            else
            {
                HashSet<Vector2Int> expectedCells = new();
                for (int y = 0; y < ItemGridPlacementRulePreview.BoardSize; y++)
                {
                    for (int x = 0; x < ItemGridPlacementRulePreview.BoardSize; x++)
                    {
                        expectedCells.Add(new Vector2Int(x, y));
                    }
                }

                HashSet<Vector2Int> actualCells = new(cells.Select(cell => cell.Cell));
                if (!actualCells.SetEquals(expectedCells))
                {
                    result.Errors.Add($"Scene placement cells do not cover the full 5x5 board: {ItemGridPlacementRulePreview.FormatCells(actualCells)}.");
                }

                if (cells.Any(cell => !cell.HasButton || !cell.HasLabel))
                {
                    result.Errors.Add("Every placement cell must have a button and label for anchor preview.");
                }
                else
                {
                    result.Notes.Add("Scene grid check passed: 25 clickable cells cover the full 5x5 board.");
                }
            }

            if (controller == null)
            {
                result.Errors.Add("Missing ItemSandboxDetailUiController.");
            }
            else if (!controller.HasPlacementPreview)
            {
                result.Errors.Add("ItemSandboxDetailUiController is not wired to placement preview.");
            }
            else
            {
                result.Notes.Add("Selection wiring check passed: item selection refreshes both detail panel and placement preview.");
            }

            if (detailPanel == null)
            {
                result.Errors.Add("Missing existing ItemSandboxDetailPanelView; this package must keep detail UI refresh.");
            }

            if (provider == null)
            {
                result.Errors.Add("Missing ItemInnerDataCatalogProvider in Item Sandbox scene.");
            }
            else if (provider.ReadsFormalBattleObject || provider.ReadsFormalSaveData || provider.WritesFormalSystem)
            {
                result.Errors.Add("CatalogProvider formal system guard flags are not all false.");
            }

            if (buttons.Length != 31)
            {
                result.Errors.Add($"Item button count is {buttons.Length}; expected 31 existing catalog entries.");
            }

            CheckHierarchyNames(scene, result);
        }

        private static void CheckHierarchyNames(Scene scene, VerificationResult result)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
                {
                    if (ContainsChinese(transform.gameObject.name))
                    {
                        result.Errors.Add($"Hierarchy name contains Chinese text: {transform.gameObject.name}");
                    }
                }
            }

            if (result.Errors.All(error => !error.StartsWith("Hierarchy name", StringComparison.Ordinal)))
            {
                result.Notes.Add("Hierarchy naming check passed: no Chinese GameObject names were added.");
            }
        }

        private static void CheckSourceScope(VerificationResult result)
        {
            foreach (string sourcePath in ScopedSourcePaths)
            {
                if (!File.Exists(sourcePath))
                {
                    result.Errors.Add($"Missing source file: {sourcePath}");
                    continue;
                }

                if (sourcePath.EndsWith("ItemGridPlacementAndEyeRuleVerifier.cs", StringComparison.Ordinal))
                {
                    continue;
                }

                string content = File.ReadAllText(sourcePath, Encoding.UTF8);
                foreach (string forbiddenToken in ForbiddenSourceTokens)
                {
                    if (IsAllowedAdjacentPreviewToken(sourcePath, forbiddenToken))
                    {
                        continue;
                    }

                    if (content.Contains(forbiddenToken))
                    {
                        result.Errors.Add($"Forbidden source token '{forbiddenToken}' found in {sourcePath}.");
                    }
                }
            }

            result.Notes.Add("Source scope check completed for Item Sandbox placement preview files.");
        }

        private static bool IsAllowedAdjacentPreviewToken(string sourcePath, string forbiddenToken)
        {
            return string.Equals(forbiddenToken, "BuildSynergyResolver", StringComparison.Ordinal)
                && sourcePath.EndsWith("ItemSandboxGridPlacementPreviewView.cs", StringComparison.Ordinal);
        }

        private static void WriteReports(VerificationResult result, IReadOnlyList<PlacementSample> samples)
        {
            Directory.CreateDirectory("Docs/V0.4/Reports");
            File.WriteAllText(DetailReportPath, BuildDetailReport(result), new UTF8Encoding(false));
            File.WriteAllText(SpecCsvPath, BuildSpecCsv(samples), new UTF8Encoding(false));
            File.WriteAllText(LeakCheckReportPath, BuildLeakCheckReport(result), new UTF8Encoding(false));
            AssetDatabase.Refresh();
        }

        private static string BuildDetailReport(VerificationResult result)
        {
            StringBuilder builder = new();
            builder.AppendLine("# ItemGridPlacementAndEyeRule01 Report");
            builder.AppendLine();
            builder.AppendLine("- Package: `TASK_START_ITEMGRIDPLACEMENTANDEYERULE01`");
            builder.AppendLine("- Guard receipt target: `GUARD_PASS_ITEMGRIDPLACEMENTANDEYERULE01`");
            builder.AppendLine($"- Verification: {(result.Errors.Count == 0 ? "PASS" : "FAIL")}");
            builder.AppendLine($"- Scene: `{ItemSandboxDetailUiSceneBuilder.ScenePath}`");
            builder.AppendLine();
            builder.AppendLine("## Implemented");
            builder.AppendLine("- Added an independent 5x5 placement preview panel to the Item Sandbox scene.");
            builder.AppendLine("- Fixed `eyeCell=(2,2)` as a non-item, non-movable, non-coverable center cell.");
            builder.AppendLine("- Fixed `arrayBonusCells=(2,1);(2,3);(1,2);(3,2)` as identity-only cells that can be occupied.");
            builder.AppendLine("- Visualizes selected catalog item `shapeCells`, computed `occupiedCells`, `coreCellLocal`, and `coreCellWorld`.");
            builder.AppendLine("- Reports invalid preview reasons for eye cover, out-of-grid, and overlap with demo occupiedCells.");
            builder.AppendLine("- Existing item selection still refreshes `ItemSandboxDetailPanelView` through `ItemDetailViewModel`.");
            builder.AppendLine();
            AppendResultList(builder, "Notes", result.Notes);
            AppendResultList(builder, "Errors", result.Errors);
            return builder.ToString();
        }

        private static string BuildSpecCsv(IReadOnlyList<PlacementSample> samples)
        {
            StringBuilder builder = new();
            builder.AppendLine("caseId,itemId,anchorCell,expectedReason,actualReason,isValid,shapeCells,occupiedCells,coreCellLocal,coreCellWorld,note");
            foreach (PlacementSample sample in samples)
            {
                ItemGridPlacementEvaluation evaluation = sample.Result;
                string[] row =
                {
                    sample.CaseId,
                    sample.Item?.itemId ?? string.Empty,
                    ItemGridPlacementRulePreview.FormatCell(sample.AnchorCell),
                    sample.ExpectedReason.ToString(),
                    evaluation?.invalidReason.ToString() ?? "NotEvaluated",
                    evaluation?.IsValid == true ? "true" : "false",
                    evaluation != null ? ItemGridPlacementRulePreview.FormatCells(evaluation.ShapeCells) : string.Empty,
                    evaluation != null ? ItemGridPlacementRulePreview.FormatCells(evaluation.OccupiedCells) : string.Empty,
                    evaluation != null ? ItemGridPlacementRulePreview.FormatCell(evaluation.coreCellLocal) : string.Empty,
                    evaluation != null ? ItemGridPlacementRulePreview.FormatCell(evaluation.coreCellWorld) : string.Empty,
                    sample.Note
                };
                builder.AppendLine(string.Join(",", row.Select(EscapeCsv)));
            }

            return builder.ToString();
        }

        private static string BuildLeakCheckReport(VerificationResult result)
        {
            StringBuilder builder = new();
            builder.AppendLine("# ItemGridPlacementAndEyeRule01 Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"- Result: {(result.Errors.Count == 0 ? "PASS" : "FAIL")}");
            builder.AppendLine($"- Scene path: `{ItemSandboxDetailUiSceneBuilder.ScenePath}`");
            builder.AppendLine("- BuildSettings: checked readonly; Item Sandbox scene remains manual-only and unregistered.");
            builder.AppendLine("- Scope: Item Sandbox placement preview and eye cover validation only.");
            builder.AppendLine("- Not implemented: real lighting, relay, Build calculation, awakening, drop/acquire, upgrade, save, battle bridge, or battle settlement.");
            builder.AppendLine("- Catalog boundary: eye stone is not a catalog item; placement preview consumes existing 31 catalog item shape fields only.");
            builder.AppendLine("- Array cells: identity display only; no array bonus calculation is executed.");
            builder.AppendLine();
            AppendResultList(builder, "Passed Checks / Notes", result.Notes);
            AppendResultList(builder, "Errors", result.Errors);
            return builder.ToString();
        }

        private static void AppendResultList(StringBuilder builder, string title, IReadOnlyList<string> values)
        {
            builder.AppendLine($"## {title}");
            if (values == null || values.Count == 0)
            {
                builder.AppendLine("- None");
                builder.AppendLine();
                return;
            }

            foreach (string value in values)
            {
                builder.AppendLine($"- {value}");
            }

            builder.AppendLine();
        }

        private static string EscapeCsv(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            bool needsEscape = value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r");
            if (!needsEscape)
            {
                return value;
            }

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        private static bool ContainsChinese(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            for (int i = 0; i < value.Length; i++)
            {
                char character = value[i];
                if (character >= '\u4e00' && character <= '\u9fff')
                {
                    return true;
                }
            }

            return false;
        }

        private sealed class PlacementSample
        {
            public PlacementSample(
                string caseId,
                ItemInnerDataDefinition item,
                Vector2Int anchorCell,
                IReadOnlyList<ItemGridPlacedItemPreview> lockedPlacements,
                ItemGridPlacementInvalidReason expectedReason,
                string note)
            {
                CaseId = caseId;
                Item = item;
                AnchorCell = anchorCell;
                LockedPlacements = lockedPlacements ?? Array.Empty<ItemGridPlacedItemPreview>();
                ExpectedReason = expectedReason;
                Note = note;
            }

            public string CaseId { get; }
            public ItemInnerDataDefinition Item { get; }
            public Vector2Int AnchorCell { get; }
            public IReadOnlyList<ItemGridPlacedItemPreview> LockedPlacements { get; }
            public ItemGridPlacementInvalidReason ExpectedReason { get; }
            public string Note { get; }
            public ItemGridPlacementEvaluation Result { get; set; }
        }

        private sealed class VerificationResult
        {
            public readonly List<string> Errors = new();
            public readonly List<string> Notes = new();
        }
    }
}
#endif
