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
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace TalismanBag.EditorTools.ItemSandbox
{
    public static class JuNianLightingAndAdjacentRelayVerifier
    {
        private const string DetailReportPath = "Docs/V0.4/Reports/JuNianLightingAndAdjacentRelayReport.md";
        private const string SpecCsvPath = "Docs/V0.4/Reports/JuNianLightingAndAdjacentRelaySpec.csv";
        private const string LeakCheckReportPath = "Docs/V0.4/Reports/JuNianLightingAndAdjacentRelayLeakCheckReport.md";

        private static readonly string[] ScopedSourcePaths =
        {
            "Assets/_Game/Scripts/TalismanBag/Items/Lighting/ItemLightingRules.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailViewModel.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemGridPlacementRulePreview.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxGridCellView.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxGridPlacementPreviewView.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxLightingScenarioCatalog.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxLightingDetailProjection.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxDetailUiController.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxDetailPanelView.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemSandboxDetailUiSceneBuilder.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/JuNianLightingAndAdjacentRelayVerifier.cs"
        };

        private static readonly string[] ModifiedFilePaths =
        {
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity",
            "Assets/_Game/Scripts/TalismanBag/Items/Lighting/ItemLightingRules.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailViewModel.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxGridPlacementPreviewView.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxLightingScenarioCatalog.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxLightingDetailProjection.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxDetailUiController.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxDetailPanelView.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemSandboxDetailUiSceneBuilder.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemGridPlacementAndEyeRuleVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/JuNianLightingAndAdjacentRelayVerifier.cs",
            "Docs/V0.4/Reports/JuNianLightingAndAdjacentRelayReport.md",
            "Docs/V0.4/Reports/JuNianLightingAndAdjacentRelaySpec.csv",
            "Docs/V0.4/Reports/JuNianLightingAndAdjacentRelayLeakCheckReport.md"
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

        [MenuItem("Tools/Talisman Bag/V0.4/ItemSandbox/JuNianLightingAndAdjacentRelay01/[Guard Only] Verify And Write Reports")]
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
            List<LightingSpecRow> rows = new();

            try
            {
                RunChecks(result, rows);
                WriteReports(result, rows);

                if (result.Errors.Count == 0)
                {
                    Debug.Log("JuNianLightingAndAdjacentRelay01 verification passed and reports were written.");
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

        private static void RunChecks(VerificationResult result, List<LightingSpecRow> rows)
        {
            CheckRuleConstants(result);
            CheckCatalogSourceBoundary(result);
            CheckScenarioSamples(result, rows);
            CheckViewModelProjection(result);
            CheckScene(result);
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

            HashSet<Vector2Int> expectedArrayCells = new()
            {
                new Vector2Int(2, 1),
                new Vector2Int(2, 3),
                new Vector2Int(1, 2),
                new Vector2Int(3, 2)
            };
            if (!new HashSet<Vector2Int>(boardConfig.ArrayBonusCells).SetEquals(expectedArrayCells))
            {
                result.Errors.Add($"arrayBonusCells mismatch: {ItemGridPlacementRulePreview.FormatCells(boardConfig.ArrayBonusCells)}.");
            }

            HashSet<Vector2Int> expectedOffsets = new()
            {
                new Vector2Int(0, 1),
                new Vector2Int(1, 0),
                new Vector2Int(0, -1),
                new Vector2Int(-1, 0)
            };
            HashSet<Vector2Int> actualOffsets = new(ItemLightingResolver.DefaultRangeRule.DirectLitRangeOffsets);
            if (!actualOffsets.SetEquals(expectedOffsets))
            {
                result.Errors.Add($"Default direct lighting offsets mismatch: {ItemGridPlacementRulePreview.FormatCells(actualOffsets)}.");
            }

            result.Notes.Add("Rule constants checked: 5x5 board, eyeCell=(2,2), four arrayBonusCells, direct range is orthogonal adjacent only.");
        }

        private static void CheckCatalogSourceBoundary(VerificationResult result)
        {
            IReadOnlyList<ItemInnerDataDefinition> items = ItemInnerDataCatalog.AllItems;
            IReadOnlyList<ItemInnerDataDefinition> sources = items.Where(item => item.isLightingSource).ToList();
            if (sources.Count != 1 || sources[0].itemId != "I031")
            {
                result.Errors.Add($"Lighting source catalog boundary failed. Actual={string.Join("|", sources.Select(item => item.itemId))}.");
            }
            else
            {
                result.Notes.Add("Catalog source boundary passed: I031 聚念石 is the only isLightingSource=true item.");
            }
        }

        private static void CheckScenarioSamples(VerificationResult result, List<LightingSpecRow> rows)
        {
            IReadOnlyList<ItemSandboxLightingScenarioDefinition> scenarios = ItemSandboxLightingScenarioCatalog.AllScenarios;
            if (scenarios.Count != 10)
            {
                result.Errors.Add($"Scenario count is {scenarios.Count}; expected 10.");
            }

            foreach (ItemSandboxLightingScenarioDefinition scenario in scenarios)
            {
                ItemSandboxLightingScenarioBuild build = ItemSandboxLightingScenarioCatalog.BuildScenario(scenario);
                foreach (string error in build.Errors)
                {
                    result.Errors.Add(error);
                }

                ItemLightingResolutionResult lightingResult = ItemLightingResolver.Resolve(
                    ItemSandboxLightingScenarioCatalog.CreateDefaultBoardConfig(),
                    build.PlacedItems);

                foreach (ItemSandboxLightingScenarioPlacement placement in scenario.Placements)
                {
                    ItemLightingItemResult actual = lightingResult.FindItemResult(placement.itemId);
                    ItemGridPlacementEvaluation placementEvaluation = build.PlacementEvaluations
                        .FirstOrDefault(evaluation => evaluation.itemId == placement.itemId);
                    bool passed = actual != null
                        && actual.isDirectLit == placement.expectedIsDirectLit
                        && actual.isLit == placement.expectedIsLit
                        && string.Equals(actual.litByItemId, placement.expectedLitByItemId, StringComparison.Ordinal)
                        && actual.litDepth == placement.expectedLitDepth;

                    if (!passed)
                    {
                        result.Errors.Add(
                            $"{scenario.scenarioId}/{placement.itemId} expected direct={placement.expectedIsDirectLit}, lit={placement.expectedIsLit}, by={placement.expectedLitByItemId}, depth={placement.expectedLitDepth}; actual direct={actual?.isDirectLit}, lit={actual?.isLit}, by={actual?.litByItemId}, depth={actual?.litDepth}.");
                    }

                    rows.Add(new LightingSpecRow(
                        scenario.scenarioId,
                        placement.itemId,
                        placementEvaluation != null ? ItemGridPlacementRulePreview.FormatCell(placementEvaluation.coreCellWorld) : string.Empty,
                        actual?.isLightingSource == true,
                        actual?.isDirectLit == true,
                        actual?.isLit == true,
                        actual?.litByItemId ?? string.Empty,
                        actual?.litDepth ?? -1,
                        placement.expectedResult,
                        passed ? "PASS" : "FAIL"));
                }
            }

            result.Notes.Add("Scenario samples checked: direct core-cell lighting, multi-cell core-only rule, relay chain, diagonal/gap/eye blockers, stable tie source, no-source all-unlit, ordinary relay-only.");
        }

        private static void CheckViewModelProjection(VerificationResult result)
        {
            ItemSandboxLightingScenarioDefinition scenario = ItemSandboxLightingScenarioCatalog.AllScenarios
                .FirstOrDefault(item => item.scenarioId == "S04_RelayChain");
            ItemLightingResolutionResult lightingResult = ItemSandboxLightingScenarioCatalog.ResolveScenario(scenario);
            ItemInnerDataCatalogProvider provider = new GameObject("TempItemInnerDataCatalogProvider").AddComponent<ItemInnerDataCatalogProvider>();
            try
            {
                ItemDetailViewModel baseModel = provider.GetDetailViewModel("I013");
                ItemDetailViewModel projected = ItemSandboxLightingDetailProjection.Project(
                    baseModel,
                    lightingResult.FindItemResult("I013"));

                if (projected == null
                    || !projected.statusFlags.isLit
                    || !projected.statusFlags.isRelayLit
                    || projected.statusFlags.litDepth != 2
                    || projected.statusFlags.litByItemId != "I007"
                    || !projected.battleEffectPreview.basicEffectActive)
                {
                    result.Errors.Add("ItemDetailViewModel projection failed for relay-lit item I013.");
                }
                else
                {
                    result.Notes.Add("ItemDetailViewModel projection passed: relay-lit status, litByItemId, litDepth, and basicEffectActive are displayed through ViewModel fields.");
                }
            }
            finally
            {
                Object.DestroyImmediate(provider.gameObject);
            }
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
            ItemSandboxDetailUiController controller = Object.FindObjectsOfType<ItemSandboxDetailUiController>(true).FirstOrDefault();
            ItemSandboxDetailPanelView detailPanel = Object.FindObjectsOfType<ItemSandboxDetailPanelView>(true).FirstOrDefault();
            ItemInnerDataCatalogProvider provider = Object.FindObjectsOfType<ItemInnerDataCatalogProvider>(true).FirstOrDefault();
            ItemSandboxGridCellView[] cells = Object.FindObjectsOfType<ItemSandboxGridCellView>(true);

            if (preview == null)
            {
                result.Errors.Add("Missing ItemSandboxGridPlacementPreviewView.");
            }
            else
            {
                if (preview.ConfiguredCellCount != 25)
                {
                    result.Errors.Add($"Configured cell count is {preview.ConfiguredCellCount}; expected 25.");
                }

                if (preview.ConfiguredScenarioButtonCount != 10)
                {
                    result.Errors.Add($"Lighting scenario button count is {preview.ConfiguredScenarioButtonCount}; expected 10.");
                }
                else
                {
                    result.Notes.Add("Scene scenario switch check passed: 10 repeatable greybox lighting cases are configured.");
                }
            }

            if (controller == null || detailPanel == null || provider == null)
            {
                result.Errors.Add("Item Sandbox scene is missing controller, detail panel, or catalog provider.");
            }

            if (cells.Length != 25)
            {
                result.Errors.Add($"Scene grid cell count is {cells.Length}; expected 25.");
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

                if (sourcePath.EndsWith("JuNianLightingAndAdjacentRelayVerifier.cs", StringComparison.Ordinal))
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

            result.Notes.Add("Source scope check completed: Item Sandbox lighting files do not reference formal battle, bridge, save, reward, boss, or BuildSettings writers.");
        }

        private static void WriteReports(VerificationResult result, IReadOnlyList<LightingSpecRow> rows)
        {
            Directory.CreateDirectory("Docs/V0.4/Reports");
            var utf8WithBom = new UTF8Encoding(true);
            File.WriteAllText(DetailReportPath, BuildDetailReport(result), utf8WithBom);
            File.WriteAllText(SpecCsvPath, BuildSpecCsv(rows), utf8WithBom);
            File.WriteAllText(LeakCheckReportPath, BuildLeakCheckReport(result), utf8WithBom);
            AssetDatabase.Refresh();
        }

        private static string BuildDetailReport(VerificationResult result)
        {
            StringBuilder builder = new();
            builder.AppendLine("# JuNianLightingAndAdjacentRelay01 Report");
            builder.AppendLine();
            builder.AppendLine("- Package: `JuNianLightingAndAdjacentRelay01`");
            builder.AppendLine("- Guard receipt target: `GUARD_PASS_JUNIANLIGHTINGANDADJACENTRELAY01`");
            builder.AppendLine($"- Verification: {(result.Errors.Count == 0 ? "PASS" : "FAIL")}");
            builder.AppendLine($"- Scene: `{ItemSandboxDetailUiSceneBuilder.ScenePath}`");
            builder.AppendLine();
            builder.AppendLine("## Implemented");
            builder.AppendLine("- Pure rule layer: `Assets/_Game/Scripts/TalismanBag/Items/Lighting/ItemLightingRules.cs`.");
            builder.AppendLine("- Default direct range: `OrthogonalAdjacentLightingRangeRule`, offsets `(0,1);(1,0);(0,-1);(-1,0)`.");
            builder.AppendLine("- coreCell判定：普通道具只检查 `coreCellWorld in litRangeCells`，不要求整件道具进入范围。");
            builder.AppendLine("- 相邻判定：两件普通道具任意 `occupiedCells` 曼哈顿距离为 1。");
            builder.AppendLine("- 链式遍历：直亮普通道具为 depth 0，逐层接亮；多来源先选最小 `litDepth`，同深度按稳定 `itemId`。");
            builder.AppendLine("- ViewModel投影：`ItemSandboxLightingDetailProjection` 将结果写入 `ItemDetailViewModel` / `ItemLightingPreview` / `ItemBattleEffectPreview`，详情 UI 不读解析器内部对象。");
            builder.AppendLine("- Sandbox灰盒：`ItemSandboxGridPlacementPreviewView` 提供 10 个可重复切换案例，格子显示阵眼、阵脉、聚念石、点亮范围、直亮、接亮、未亮。");
            builder.AppendLine();
            builder.AppendLine("## Test Layouts");
            foreach (ItemSandboxLightingScenarioDefinition scenario in ItemSandboxLightingScenarioCatalog.AllScenarios)
            {
                builder.AppendLine($"- `{scenario.scenarioId}`: {scenario.description}");
            }

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

        private static string BuildSpecCsv(IReadOnlyList<LightingSpecRow> rows)
        {
            StringBuilder builder = new();
            builder.AppendLine("scenarioId,itemId,coreCellWorld,isLightingSource,isDirectLit,isLit,litByItemId,litDepth,expectedResult,actualResult");
            foreach (LightingSpecRow row in rows)
            {
                string[] values =
                {
                    row.scenarioId,
                    row.itemId,
                    row.coreCellWorld,
                    row.isLightingSource ? "true" : "false",
                    row.isDirectLit ? "true" : "false",
                    row.isLit ? "true" : "false",
                    row.litByItemId,
                    row.litDepth >= 0 ? row.litDepth.ToString() : "None",
                    row.expectedResult,
                    row.actualResult
                };
                builder.AppendLine(string.Join(",", values.Select(EscapeCsv)));
            }

            return builder.ToString();
        }

        private static string BuildLeakCheckReport(VerificationResult result)
        {
            StringBuilder builder = new();
            builder.AppendLine("# JuNianLightingAndAdjacentRelay01 Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"- Result: {(result.Errors.Count == 0 ? "PASS" : "FAIL")}");
            builder.AppendLine($"- Scene path: `{ItemSandboxDetailUiSceneBuilder.ScenePath}`");
            builder.AppendLine("- BuildSettings: checked readonly; Item Sandbox scene remains manual-only and unregistered.");
            builder.AppendLine("- Scope: independent Item Sandbox lighting, adjacent relay, greybox visualization, and ItemDetailViewModel projection.");
            builder.AppendLine("- Forbidden formal integrations: BattleResolver, BattleSnapshotAdapter, UnifiedBattlePage, Battle Bridge, RunFlow, SaveData, Reward, Boss, BuildSettings writers.");
            builder.AppendLine("- Not implemented: Build count / activation, four auto skill monitor icons, array bonus settlement, core awakening unlock, affix/drop/upgrade/save, battle damage/heal/shield/status settlement, BattleContract formal wiring.");
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

        private sealed class LightingSpecRow
        {
            public LightingSpecRow(
                string scenarioId,
                string itemId,
                string coreCellWorld,
                bool isLightingSource,
                bool isDirectLit,
                bool isLit,
                string litByItemId,
                int litDepth,
                string expectedResult,
                string actualResult)
            {
                this.scenarioId = scenarioId;
                this.itemId = itemId;
                this.coreCellWorld = coreCellWorld;
                this.isLightingSource = isLightingSource;
                this.isDirectLit = isDirectLit;
                this.isLit = isLit;
                this.litByItemId = litByItemId;
                this.litDepth = litDepth;
                this.expectedResult = expectedResult;
                this.actualResult = actualResult;
            }

            public readonly string scenarioId;
            public readonly string itemId;
            public readonly string coreCellWorld;
            public readonly bool isLightingSource;
            public readonly bool isDirectLit;
            public readonly bool isLit;
            public readonly string litByItemId;
            public readonly int litDepth;
            public readonly string expectedResult;
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
