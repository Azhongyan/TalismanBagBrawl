#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TalismanBag.ItemSandbox;
using TalismanBag.Items.Detail;
using TalismanBag.Items.InnerCatalog;
using TalismanBag.Items.Lighting;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TalismanBag.EditorTools.ItemSandbox
{
    public static class ArrayBonusCellResolverVerifier
    {
        private const string DetailReportPath = "Docs/V0.4/Reports/ArrayBonusCellResolverReport.md";
        private const string SpecCsvPath = "Docs/V0.4/Reports/ArrayBonusCellResolverSpec.csv";
        private const string LeakCheckReportPath = "Docs/V0.4/Reports/ArrayBonusCellResolverLeakCheckReport.md";

        private static readonly string[] ScopedSourcePaths =
        {
            "Assets/_Game/Scripts/TalismanBag/Items/Lighting/ItemArrayBonusRules.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Lighting/ItemLightingRules.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailViewModel.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemGridPlacementRulePreview.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxGridPlacementPreviewView.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxLightingScenarioCatalog.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxLightingDetailProjection.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ArrayBonusCellResolverVerifier.cs"
        };

        private static readonly string[] ModifiedFilePaths =
        {
            "Assets/_Game/Scripts/TalismanBag/Items/Lighting/ItemArrayBonusRules.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Lighting/ItemArrayBonusRules.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Items/Lighting/ItemLightingRules.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailViewModel.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemGridPlacementRulePreview.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxGridPlacementPreviewView.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxLightingScenarioCatalog.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxLightingDetailProjection.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ArrayBonusCellResolverVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ArrayBonusCellResolverVerifier.cs.meta",
            "Docs/V0.4/Reports/ArrayBonusCellResolverReport.md",
            "Docs/V0.4/Reports/ArrayBonusCellResolverSpec.csv",
            "Docs/V0.4/Reports/ArrayBonusCellResolverLeakCheckReport.md"
        };

        private static readonly string[] ForbiddenSourceTokens =
        {
            "BattleResolver",
            "BattleSnapshotAdapter",
            "UnifiedBattlePage",
            "BattleBridge",
            "V02RunFlow",
            "V03RunFlow",
            "RunFlowController",
            "SaveData",
            "PlayerPrefs",
            "RewardConfig",
            "DropTable",
            "BossInfo",
            "EditorBuildSettings.scenes ="
        };

        [MenuItem("Tools/Talisman Bag/V0.4/ItemSandbox/ArrayBonusCellResolver01/[Guard Only] Verify And Write Reports")]
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
            List<ArrayBonusSpecRow> rows = new();

            try
            {
                RunChecks(result, rows);
                WriteReports(result, rows);

                if (result.Errors.Count == 0)
                {
                    Debug.Log("ArrayBonusCellResolver01 verification passed and reports were written.");
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
                WriteReports(result, rows);
                if (exitWhenBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
        }

        private static void RunChecks(VerificationResult result, List<ArrayBonusSpecRow> rows)
        {
            CheckRuleConstants(result);
            CheckPlacementRules(result);
            ItemArrayBonusResolutionResult sampleResult = CheckArrayBonusSamples(result, rows);
            CheckSameItemPlacementIdentity(result);
            CheckArrayDoesNotLight(result);
            CheckViewModelProjection(result, sampleResult);
            CheckSourceScope(result);
        }

        private static void CheckRuleConstants(VerificationResult result)
        {
            ItemLightingBoardConfig boardConfig = ItemSandboxLightingScenarioCatalog.CreateDefaultBoardConfig();
            if (boardConfig.boardSize != 5)
            {
                result.Errors.Add($"BoardSize is {boardConfig.boardSize}; expected 5.");
            }

            if (boardConfig.eyeCell != new Vector2Int(2, 2))
            {
                result.Errors.Add($"eyeCell is {ItemGridPlacementRulePreview.FormatCell(boardConfig.eyeCell)}; expected (2,2).");
            }

            Dictionary<string, Vector2Int> expected = new()
            {
                ["AP01"] = new Vector2Int(2, 1),
                ["AP02"] = new Vector2Int(2, 3),
                ["AP03"] = new Vector2Int(1, 2),
                ["AP04"] = new Vector2Int(3, 2)
            };

            foreach (KeyValuePair<string, Vector2Int> pair in expected)
            {
                ItemArrayBonusCellDefinition definition = ItemArrayBonusResolver.FixedArrayBonusCells
                    .FirstOrDefault(cell => cell.cellId == pair.Key);
                if (definition == null || definition.cell != pair.Value)
                {
                    result.Errors.Add($"{pair.Key} mismatch; expected {ItemGridPlacementRulePreview.FormatCell(pair.Value)}.");
                }
            }

            HashSet<Vector2Int> actualCells = new(boardConfig.ArrayBonusCells);
            if (!actualCells.SetEquals(expected.Values))
            {
                result.Errors.Add($"arrayBonusCells mismatch: {ItemGridPlacementRulePreview.FormatCells(boardConfig.ArrayBonusCells)}.");
            }

            if (actualCells.Contains(boardConfig.eyeCell))
            {
                result.Errors.Add("arrayBonusCells include the fixed eyeCell.");
            }

            result.Notes.Add("Rule constants checked: boardSize=5, eyeCell=(2,2), AP01/AP02/AP03/AP04 are fixed and exclude the eye.");
        }

        private static void CheckPlacementRules(VerificationResult result)
        {
            ItemInnerDataDefinition single = ItemInnerDataCatalog.FindById("I001");
            ItemGridPlacementEvaluation onArray = ItemGridPlacementRulePreview.Evaluate(
                single,
                new Vector2Int(2, 1),
                null,
                "P_PLACE_AP01");
            if (onArray == null || !onArray.IsValid || !onArray.OccupiedCells.Contains(new Vector2Int(2, 1)))
            {
                result.Errors.Add("Legal item placement on AP01 failed.");
            }

            ItemGridPlacementEvaluation onEye = ItemGridPlacementRulePreview.Evaluate(
                single,
                new Vector2Int(2, 2),
                null,
                "P_PLACE_EYE");
            if (onEye == null || onEye.invalidReason != ItemGridPlacementInvalidReason.EyeCellCovered)
            {
                result.Errors.Add("Eye-cell placement was not blocked.");
            }

            ItemInnerDataDefinition corner = ItemInnerDataCatalog.FindById("I015");
            ItemGridPlacementEvaluation multi = ItemGridPlacementRulePreview.Evaluate(
                corner,
                new Vector2Int(1, 1),
                null,
                "P_MULTI_AP01_AP03");
            if (multi == null
                || !multi.IsValid
                || multi.OccupiedCells.Contains(ItemGridPlacementRulePreview.EyeCell)
                || !multi.OccupiedCells.Contains(new Vector2Int(2, 1))
                || !multi.OccupiedCells.Contains(new Vector2Int(1, 2)))
            {
                result.Errors.Add("Legal multi-cell AP01/AP03 placement failed.");
            }

            result.Notes.Add("Placement rules checked: AP cells are legal, eyeCell remains blocked, multi-cell item can occupy two AP cells without covering the eye.");
        }

        private static ItemArrayBonusResolutionResult CheckArrayBonusSamples(VerificationResult result, List<ArrayBonusSpecRow> rows)
        {
            ItemLightingBoardConfig boardConfig = ItemSandboxLightingScenarioCatalog.CreateDefaultBoardConfig();
            ItemGridPlacementEvaluation multi = ItemGridPlacementRulePreview.Evaluate(
                ItemInnerDataCatalog.FindById("I015"),
                new Vector2Int(1, 1),
                null,
                "P_MULTI_AP01_AP03");

            List<ItemLightingItemResult> lightingResults = new()
            {
                new ItemLightingItemResult(
                    "I015",
                    "Corner sample",
                    multi.OccupiedCells,
                    multi.coreCellWorld,
                    false,
                    false,
                    true,
                    "I031",
                    1,
                    "P_MULTI_AP01_AP03",
                    "SRC_JUNIAN"),
                new ItemLightingItemResult(
                    "I001",
                    "Single AP02",
                    new[] { new Vector2Int(2, 3) },
                    new Vector2Int(2, 3),
                    false,
                    true,
                    true,
                    "I031",
                    0,
                    "P_SINGLE_AP02",
                    "SRC_JUNIAN"),
                new ItemLightingItemResult(
                    "I007",
                    "Single AP04 unlit",
                    new[] { new Vector2Int(3, 2) },
                    new Vector2Int(3, 2),
                    false,
                    false,
                    false,
                    string.Empty,
                    -1,
                    "P_SINGLE_AP04_UNLIT",
                    string.Empty),
                new ItemLightingItemResult(
                    "I013",
                    "Single off array",
                    new[] { new Vector2Int(0, 0) },
                    new Vector2Int(0, 0),
                    false,
                    true,
                    true,
                    "I031",
                    0,
                    "P_OFF_ARRAY",
                    "SRC_JUNIAN")
            };

            ItemArrayBonusResolutionResult arrayResult = ItemArrayBonusResolver.Resolve(boardConfig, lightingResults);
            CheckItemState(result, rows, arrayResult, "P_MULTI_AP01_AP03", true, true, "AP01|AP03", "multi-cell lit item active on two AP cells");
            CheckItemState(result, rows, arrayResult, "P_SINGLE_AP02", true, true, "AP02", "single lit item active on AP02");
            CheckItemState(result, rows, arrayResult, "P_SINGLE_AP04_UNLIT", true, false, "AP04", "unlit item occupies AP04 but is inactive");
            CheckItemState(result, rows, arrayResult, "P_OFF_ARRAY", false, false, "None", "lit item off AP cells has no array state");

            foreach (string cellId in new[] { "AP01", "AP02", "AP03", "AP04" })
            {
                ItemArrayBonusCellState cellState = arrayResult.FindCellState(cellId);
                if (cellState == null || !cellState.isOccupied)
                {
                    result.Errors.Add($"{cellId} was not reported as occupied in the sample.");
                }
            }

            result.Notes.Add("Array samples checked: single-cell, unlit occupied, off-array, and legal multi-cell AP occupation all resolve as expected.");
            return arrayResult;
        }

        private static void CheckItemState(
            VerificationResult result,
            List<ArrayBonusSpecRow> rows,
            ItemArrayBonusResolutionResult arrayResult,
            string placementId,
            bool expectedOnArray,
            bool expectedActive,
            string expectedCellIds,
            string note)
        {
            ItemArrayBonusItemResult item = arrayResult.FindPlacementResult(placementId);
            string actualCellIds = item == null
                ? "Missing"
                : ItemArrayBonusResolver.FormatCellIds(item.OccupiedArrayBonusCellIds);
            bool passed = item != null
                && item.isOnArrayBonusCell == expectedOnArray
                && item.isArrayBonusActive == expectedActive
                && string.Equals(actualCellIds, expectedCellIds, StringComparison.Ordinal);

            if (!passed)
            {
                result.Errors.Add($"{placementId} expected onArray={expectedOnArray}, active={expectedActive}, cells={expectedCellIds}; actual onArray={item?.isOnArrayBonusCell}, active={item?.isArrayBonusActive}, cells={actualCellIds}.");
            }

            rows.Add(new ArrayBonusSpecRow(
                note,
                item?.placementId ?? placementId,
                item?.itemId ?? string.Empty,
                item == null ? string.Empty : ItemGridPlacementRulePreview.FormatCells(item.OccupiedCells),
                actualCellIds,
                item?.isLit == true,
                item?.isOnArrayBonusCell == true,
                item?.isArrayBonusActive == true,
                passed ? "PASS" : "FAIL"));
        }

        private static void CheckSameItemPlacementIdentity(VerificationResult result)
        {
            ItemLightingBoardConfig boardConfig = ItemSandboxLightingScenarioCatalog.CreateDefaultBoardConfig();
            ItemLightingResolutionResult lightingResult = ItemLightingResolver.Resolve(
                boardConfig,
                new[]
                {
                    new ItemLightingPlacedItem("I031", "Source", new[] { new Vector2Int(0, 0) }, new Vector2Int(0, 0), true, new Vector2Int(0, 0), "SRC_JUNIAN"),
                    new ItemLightingPlacedItem("I001", "Same item A", new[] { new Vector2Int(1, 0) }, new Vector2Int(1, 0), false, new Vector2Int(1, 0), "P_I001_A"),
                    new ItemLightingPlacedItem("I001", "Same item B", new[] { new Vector2Int(3, 0) }, new Vector2Int(3, 0), false, new Vector2Int(3, 0), "P_I001_B")
                });

            IReadOnlyList<ItemLightingItemResult> sameItemResults = lightingResult.FindItemResults("I001");
            ItemLightingItemResult first = lightingResult.FindPlacementResult("P_I001_A");
            ItemLightingItemResult second = lightingResult.FindPlacementResult("P_I001_B");
            if (sameItemResults.Count != 2
                || first == null
                || second == null
                || !first.isLit
                || second.isLit)
            {
                result.Errors.Add("placementId identity failed for two I001 placements.");
            }

            ItemArrayBonusResolutionResult arrayResult = ItemArrayBonusResolver.Resolve(boardConfig, lightingResult.ItemResults);
            if (arrayResult.FindItemResults("I001").Count != 2)
            {
                result.Errors.Add("Array snapshot did not preserve both I001 placement results.");
            }

            result.Notes.Add("placementId identity checked: two I001 placements resolve independently in lighting and array snapshots.");
        }

        private static void CheckArrayDoesNotLight(VerificationResult result)
        {
            ItemLightingBoardConfig boardConfig = ItemSandboxLightingScenarioCatalog.CreateDefaultBoardConfig();
            ItemLightingResolutionResult lightingResult = ItemLightingResolver.Resolve(
                boardConfig,
                new[]
                {
                    new ItemLightingPlacedItem("I001", "Unlit on AP01", new[] { new Vector2Int(2, 1) }, new Vector2Int(2, 1), false, new Vector2Int(2, 1), "P_UNLIT_AP01")
                });

            ItemLightingItemResult item = lightingResult.FindPlacementResult("P_UNLIT_AP01");
            ItemArrayBonusItemResult arrayItem = ItemArrayBonusResolver.Resolve(boardConfig, lightingResult.ItemResults)
                .FindPlacementResult("P_UNLIT_AP01");
            if (item == null
                || item.isLit
                || arrayItem == null
                || !arrayItem.isOnArrayBonusCell
                || arrayItem.isArrayBonusActive)
            {
                result.Errors.Add("Array cell incorrectly lit or activated an item without JuNian/relay lighting.");
            }

            result.Notes.Add("No-light rule checked: occupying AP01 does not directly light, relay, or activate by itself.");
        }

        private static void CheckViewModelProjection(VerificationResult result, ItemArrayBonusResolutionResult sampleResult)
        {
            ItemArrayBonusItemResult arrayItem = sampleResult.FindPlacementResult("P_MULTI_AP01_AP03");
            if (arrayItem == null)
            {
                result.Errors.Add("Missing sample array result for P_MULTI_AP01_AP03.");
                return;
            }

            ItemLightingItemResult lightingItem = new(
                "I015",
                "Corner sample",
                arrayItem.OccupiedCells,
                new Vector2Int(1, 1),
                false,
                false,
                true,
                "I031",
                1,
                "P_MULTI_AP01_AP03",
                "SRC_JUNIAN");

            ItemInnerDataCatalogProvider provider = new GameObject("TempArrayBonusCatalogProvider").AddComponent<ItemInnerDataCatalogProvider>();
            try
            {
                ItemDetailViewModel baseModel = provider.GetDetailViewModel("I015");
                ItemDetailViewModel projected = ItemSandboxLightingDetailProjection.Project(baseModel, lightingItem, arrayItem);
                bool hasArrayStat = projected != null
                    && projected.displayPrimaryStats.Any(line => line != null && line.label == "isArrayBonusActive" && line.value == "true");
                if (projected == null
                    || projected.placementId != "P_MULTI_AP01_AP03"
                    || !projected.statusFlags.isOnArrayBonusCell
                    || !projected.statusFlags.isArrayBonusActive
                    || !projected.lightingPreview.isArrayBonusActive
                    || !hasArrayStat)
                {
                    result.Errors.Add("ItemDetailViewModel array bonus projection failed.");
                }
                else
                {
                    result.Notes.Add("ItemDetailViewModel projection checked: placementId, AP occupancy, and active flags are visible in detail state.");
                }
            }
            finally
            {
                Object.DestroyImmediate(provider.gameObject);
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

                if (sourcePath.EndsWith("ArrayBonusCellResolverVerifier.cs", StringComparison.Ordinal))
                {
                    continue;
                }

                string content = File.ReadAllText(sourcePath, Encoding.UTF8);
                foreach (string forbiddenToken in ForbiddenSourceTokens)
                {
                    if (content.Contains(forbiddenToken))
                    {
                        result.Errors.Add($"Forbidden source token '{forbiddenToken}' found in {sourcePath}.");
                    }
                }
            }

            result.Notes.Add("Source scope check completed: Item Sandbox array bonus files do not reference formal battle, bridge, save, reward, boss, or BuildSettings writers.");
        }

        private static void WriteReports(VerificationResult result, IReadOnlyList<ArrayBonusSpecRow> rows)
        {
            Directory.CreateDirectory("Docs/V0.4/Reports");
            UTF8Encoding utf8WithBom = new(true);
            File.WriteAllText(DetailReportPath, BuildDetailReport(result), utf8WithBom);
            File.WriteAllText(SpecCsvPath, BuildSpecCsv(rows), utf8WithBom);
            File.WriteAllText(LeakCheckReportPath, BuildLeakCheckReport(result), utf8WithBom);
            AssetDatabase.Refresh();
        }

        private static string BuildDetailReport(VerificationResult result)
        {
            StringBuilder builder = new();
            builder.AppendLine("# ArrayBonusCellResolver01 Report");
            builder.AppendLine();
            builder.AppendLine("- Package: `ArrayBonusCellResolver01`");
            builder.AppendLine("- Guard receipt target: `GUARD_PASS_ARRAYBONUSCELLRESOLVER01`");
            builder.AppendLine($"- Verification: {(result.Errors.Count == 0 ? "PASS" : "FAIL")}");
            builder.AppendLine("- Scope: independent Item Sandbox only.");
            builder.AppendLine();
            builder.AppendLine("## Implemented");
            builder.AppendLine("- Fixed AP cells: AP01=(2,1), AP02=(2,3), AP03=(1,2), AP04=(3,2).");
            builder.AppendLine("- `placementId` identity for placed item previews, lighting placements, lighting results, and ItemDetailViewModel projection.");
            builder.AppendLine("- Pure `ItemArrayBonusResolver` snapshot with cell states and per-placement item states.");
            builder.AppendLine("- Active rule: `isLit && isOnArrayBonusCell`.");
            builder.AppendLine("- State only: no numeric attack, defense, cooldown, multiplier, or settlement effects.");
            builder.AppendLine();
            AppendResultList(builder, "Notes", result.Notes);
            AppendResultList(builder, "Errors", result.Errors);
            builder.AppendLine("## Modified Files");
            foreach (string sourcePath in ModifiedFilePaths)
            {
                builder.AppendLine($"- `{sourcePath}`");
            }

            return builder.ToString();
        }

        private static string BuildSpecCsv(IReadOnlyList<ArrayBonusSpecRow> rows)
        {
            StringBuilder builder = new();
            builder.AppendLine("caseId,placementId,itemId,occupiedCells,occupiedArrayBonusCellIds,isLit,isOnArrayBonusCell,isArrayBonusActive,actualResult");
            foreach (ArrayBonusSpecRow row in rows)
            {
                string[] values =
                {
                    row.caseId,
                    row.placementId,
                    row.itemId,
                    row.occupiedCells,
                    row.occupiedArrayBonusCellIds,
                    row.isLit ? "true" : "false",
                    row.isOnArrayBonusCell ? "true" : "false",
                    row.isArrayBonusActive ? "true" : "false",
                    row.actualResult
                };
                builder.AppendLine(string.Join(",", values.Select(EscapeCsv)));
            }

            return builder.ToString();
        }

        private static string BuildLeakCheckReport(VerificationResult result)
        {
            StringBuilder builder = new();
            builder.AppendLine("# ArrayBonusCellResolver01 Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"- Result: {(result.Errors.Count == 0 ? "PASS" : "FAIL")}");
            builder.AppendLine("- Scope: independent Item Sandbox array bonus state resolver and detail projection.");
            builder.AppendLine("- BuildSettings: no writes; Item Sandbox remains manual-only.");
            builder.AppendLine("- Forbidden integrations: formal battle resolver, bridge, run flow, save data, rewards, drops, boss data, and BuildSettings writers.");
            builder.AppendLine("- Not implemented: numeric bonuses, attack/defense/cooldown multipliers, combat settlement, snapshot serialization, or formal Build activation.");
            builder.AppendLine("- Known unrelated dirty file intentionally untouched: `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity`.");
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
            return needsEscape ? "\"" + value.Replace("\"", "\"\"") + "\"" : value;
        }

        private sealed class ArrayBonusSpecRow
        {
            public ArrayBonusSpecRow(
                string caseId,
                string placementId,
                string itemId,
                string occupiedCells,
                string occupiedArrayBonusCellIds,
                bool isLit,
                bool isOnArrayBonusCell,
                bool isArrayBonusActive,
                string actualResult)
            {
                this.caseId = caseId;
                this.placementId = placementId;
                this.itemId = itemId;
                this.occupiedCells = occupiedCells;
                this.occupiedArrayBonusCellIds = occupiedArrayBonusCellIds;
                this.isLit = isLit;
                this.isOnArrayBonusCell = isOnArrayBonusCell;
                this.isArrayBonusActive = isArrayBonusActive;
                this.actualResult = actualResult;
            }

            public readonly string caseId;
            public readonly string placementId;
            public readonly string itemId;
            public readonly string occupiedCells;
            public readonly string occupiedArrayBonusCellIds;
            public readonly bool isLit;
            public readonly bool isOnArrayBonusCell;
            public readonly bool isArrayBonusActive;
            public readonly string actualResult;
        }

        private sealed class VerificationResult
        {
            public readonly List<string> Errors = new();
            public readonly List<string> Notes = new();
        }
    }
}
#endif
