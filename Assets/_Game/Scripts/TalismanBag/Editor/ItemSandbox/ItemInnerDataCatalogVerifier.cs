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
    public static class ItemInnerDataCatalogVerifier
    {
        private const string DetailReportPath = "Docs/V0.4/Reports/ItemInnerDataCatalogReport.md";
        private const string SpecCsvPath = "Docs/V0.4/Reports/ItemInnerDataCatalogSpec.csv";
        private const string LeakCheckReportPath = "Docs/V0.4/Reports/ItemInnerDataCatalogLeakCheckReport.md";

        private static readonly string[] ExpectedIds =
        {
            "I001", "I002", "I003", "I004", "I005", "I006",
            "I007", "I008", "I009", "I010", "I011", "I012",
            "I013", "I014", "I015", "I016", "I017", "I018",
            "I019", "I020", "I021", "I022", "I023", "I024",
            "I025", "I026", "I027", "I028", "I029", "I030",
            "I031"
        };

        private static readonly string[] AllowedSourcePaths =
        {
            "Assets/_Game/Scripts/TalismanBag/Items/InnerCatalog/ItemInnerDataTags.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/InnerCatalog/ItemInnerDataDefinition.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/InnerCatalog/ItemInnerDataCatalog.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/InnerCatalog/ItemInnerDataCatalogProvider.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemSandboxDetailUiSceneBuilder.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemSandboxDetailUiVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemInnerDataCatalogVerifier.cs"
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
            "EditorBuildSettings.scenes ="
        };

        [MenuItem("Tools/Talisman Bag/V0.4/ItemSandbox/ItemInnerDataCatalog01/[Guard Only] Verify And Write Reports")]
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
            IReadOnlyList<ItemInnerDataDefinition> items = ItemInnerDataCatalog.AllItems;

            try
            {
                RunChecks(items, result);
                WriteReports(items, result);

                if (result.Errors.Count == 0)
                {
                    Debug.Log("ItemInnerDataCatalog01 verification passed and reports were written.");
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
                WriteReports(items, result);
                if (exitWhenBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
        }

        private static void RunChecks(IReadOnlyList<ItemInnerDataDefinition> items, VerificationResult result)
        {
            CheckCatalog(items, result);
            CheckScene(result);
            CheckSourceScope(result);
        }

        private static void CheckCatalog(IReadOnlyList<ItemInnerDataDefinition> items, VerificationResult result)
        {
            if (items.Count != 31)
            {
                result.Errors.Add($"Catalog item count is {items.Count}; expected 31.");
            }

            HashSet<string> ids = new(items.Select(item => item.itemId));
            foreach (string expectedId in ExpectedIds)
            {
                if (!ids.Contains(expectedId))
                {
                    result.Errors.Add($"Missing catalog item id: {expectedId}");
                }
            }

            if (ids.Count != items.Count)
            {
                result.Errors.Add("Catalog item ids are not unique.");
            }

            foreach (ItemInnerDataDefinition item in items)
            {
                if (!item.HasRequiredFields())
                {
                    result.Errors.Add($"Catalog item has missing required fields: {item.itemId} {item.displayName}");
                }

                if (!item.defaultLocalCells.Contains(item.coreCellLocal))
                {
                    result.Errors.Add($"Catalog item coreCellLocal is not included in shapeCells: {item.itemId}");
                }

                if (item.displayName.Contains("阵眼石") || item.itemId.IndexOf("eye", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    result.Errors.Add($"Forbidden eye stone catalog entry detected: {item.itemId} {item.displayName}");
                }
            }

            CheckFamilyCounts(items, result);
            CheckLightingSource(items, result);

            if (result.Errors.Count == 0)
            {
                result.Notes.Add("Catalog data check passed: 31 items, stable ids, required fields, and coreCellLocal inside shapeCells.");
            }
        }

        private static void CheckFamilyCounts(IReadOnlyList<ItemInnerDataDefinition> items, VerificationResult result)
        {
            Dictionary<ItemFaMenTag, int> expected = new()
            {
                { ItemFaMenTag.Zhenlei, 6 },
                { ItemFaMenTag.Lihuo, 6 },
                { ItemFaMenTag.Zhongyue, 6 },
                { ItemFaMenTag.Xuanshui, 6 },
                { ItemFaMenTag.Taibai, 6 },
                { ItemFaMenTag.Zhonggong, 1 }
            };

            foreach (KeyValuePair<ItemFaMenTag, int> pair in expected)
            {
                int actual = items.Count(item => item.faMenTag == pair.Key);
                if (actual != pair.Value)
                {
                    result.Errors.Add($"FaMen count mismatch for {pair.Key.ToStableKey()}: {actual}, expected {pair.Value}.");
                }
            }

            foreach (ItemFaMenTag faMenTag in expected.Keys.Where(tag => tag != ItemFaMenTag.Zhonggong))
            {
                IReadOnlyList<ItemInnerDataDefinition> familyItems = items.Where(item => item.faMenTag == faMenTag).ToList();
                if (familyItems.Count(item => item.qiLeiTag == ItemQiLeiTag.Fu) != 2
                    || familyItems.Count(item => item.qiLeiTag == ItemQiLeiTag.Yin) != 1
                    || familyItems.Count(item => item.qiLeiTag == ItemQiLeiTag.Ling) != 1
                    || familyItems.Count(item => item.qiLeiTag == ItemQiLeiTag.Jing) != 1
                    || familyItems.Count(item => item.qiLeiTag == ItemQiLeiTag.Fa) != 1)
                {
                    result.Errors.Add($"QiLei distribution mismatch in {faMenTag.ToStableKey()} family.");
                }
            }
        }

        private static void CheckLightingSource(IReadOnlyList<ItemInnerDataDefinition> items, VerificationResult result)
        {
            IReadOnlyList<ItemInnerDataDefinition> sources = items.Where(item => item.isLightingSource).ToList();
            if (sources.Count != 1)
            {
                result.Errors.Add($"Lighting source marker count is {sources.Count}; expected exactly 1.");
                return;
            }

            ItemInnerDataDefinition source = sources[0];
            if (source.itemId != "I031"
                || source.displayName != "聚念石"
                || source.itemFamily != ItemFamilyTag.ZhonggongSource
                || source.faMenTag != ItemFaMenTag.Zhonggong
                || source.qiLeiTag != ItemQiLeiTag.ZhonggongQi)
            {
                result.Errors.Add("Lighting source marker must belong only to I031 聚念石 as 中宫 / 中宫符器.");
            }
            else
            {
                result.Notes.Add("Lighting source check passed: 聚念石 is the only catalog lighting source marker.");
            }
        }

        private static void CheckScene(VerificationResult result)
        {
            if (!File.Exists(ItemSandboxDetailUiSceneBuilder.ScenePath))
            {
                result.Errors.Add($"Missing item sandbox scene: {ItemSandboxDetailUiSceneBuilder.ScenePath}");
                return;
            }

            bool sceneIsInBuildSettings = EditorBuildSettings.scenes.Any(scene =>
                string.Equals(scene.path, ItemSandboxDetailUiSceneBuilder.ScenePath, StringComparison.OrdinalIgnoreCase));
            if (sceneIsInBuildSettings)
            {
                result.Errors.Add("Item sandbox scene is present in BuildSettings; ItemInnerDataCatalog01 must not modify BuildSettings.");
            }
            else
            {
                result.Notes.Add("BuildSettings check passed: Item Sandbox scene is not registered.");
            }

            Scene scene = EditorSceneManager.OpenScene(ItemSandboxDetailUiSceneBuilder.ScenePath, OpenSceneMode.Single);
            ItemInnerDataCatalogProvider provider = Object.FindObjectsOfType<ItemInnerDataCatalogProvider>(true).FirstOrDefault();
            ItemSandboxDetailUiController controller = Object.FindObjectsOfType<ItemSandboxDetailUiController>(true).FirstOrDefault();
            ItemSandboxItemButtonView[] buttons = Object.FindObjectsOfType<ItemSandboxItemButtonView>(true);

            if (provider == null)
            {
                result.Errors.Add("Item Sandbox scene does not contain ItemInnerDataCatalogProvider.");
            }
            else if (provider.GetItemList().Count != 31)
            {
                result.Errors.Add($"Scene CatalogProvider list count is {provider.GetItemList().Count}; expected 31.");
            }
            else if (provider.ReadsFormalBattleObject || provider.ReadsFormalSaveData || provider.WritesFormalSystem)
            {
                result.Errors.Add("Scene CatalogProvider formal system guard flags are not all false.");
            }
            else
            {
                result.Notes.Add("Scene provider check passed: CatalogProvider exposes 31 readonly entries and no formal system flags.");
            }

            if (controller == null)
            {
                result.Errors.Add("Item Sandbox scene does not contain ItemSandboxDetailUiController.");
            }

            if (buttons.Length != 31)
            {
                result.Errors.Add($"Scene item button count is {buttons.Length}; expected 31.");
            }
            else
            {
                result.Notes.Add("Scene list check passed: sandbox list has 31 item buttons.");
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
            foreach (string sourcePath in AllowedSourcePaths)
            {
                if (!File.Exists(sourcePath))
                {
                    result.Errors.Add($"Missing source file: {sourcePath}");
                    continue;
                }

                if (sourcePath.EndsWith("Verifier.cs", StringComparison.Ordinal))
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

            result.Notes.Add("Source scope check completed for Item catalog and Item Sandbox adapter files.");
        }

        private static void WriteReports(IReadOnlyList<ItemInnerDataDefinition> items, VerificationResult result)
        {
            Directory.CreateDirectory("Docs/V0.4/Reports");
            File.WriteAllText(DetailReportPath, BuildDetailReport(items, result), new UTF8Encoding(false));
            File.WriteAllText(SpecCsvPath, BuildSpecCsv(items), new UTF8Encoding(false));
            File.WriteAllText(LeakCheckReportPath, BuildLeakCheckReport(result), new UTF8Encoding(false));
            AssetDatabase.Refresh();
        }

        private static string BuildDetailReport(IReadOnlyList<ItemInnerDataDefinition> items, VerificationResult result)
        {
            StringBuilder builder = new();
            builder.AppendLine("# ItemInnerDataCatalog01 Report");
            builder.AppendLine();
            builder.AppendLine("- Package: `TASK_START_ITEMINNERDATACATALOG01`");
            builder.AppendLine("- Guard receipt target: `GUARD_PASS_ITEMINNERDATACATALOG01`");
            builder.AppendLine($"- Verification: {(result.Errors.Count == 0 ? "PASS" : "FAIL")}");
            builder.AppendLine("- Scene: `Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity`");
            builder.AppendLine();
            builder.AppendLine("## Summary");
            builder.AppendLine($"- Catalog item count: {items.Count} / 31");
            builder.AppendLine("- 30 法门道具 + 1 聚念石: verified by static catalog checks.");
            builder.AppendLine("- 阵眼石: not present in catalog; eyeCell remains non-item / non-placeable / non-ordinary-cell by Guard rule.");
            builder.AppendLine("- 聚念石: only entry marked `isLightingSource=true`, with `faMenTag=zhonggong` and `qiLeiTag=zhonggongQi`.");
            builder.AppendLine("- Item Sandbox detail UI: bound through `ItemDetailViewModel` via `ItemInnerDataCatalogProvider`.");
            builder.AppendLine("- DevStubProvider boundary: retained as old greybox sample source; active sandbox scene uses CatalogProvider for 31 readonly catalog entries.");
            builder.AppendLine();
            builder.AppendLine("## Catalog Groups");
            foreach (IGrouping<ItemFaMenTag, ItemInnerDataDefinition> group in items.GroupBy(item => item.faMenTag).OrderBy(group => group.Key.ToStableKey()))
            {
                builder.AppendLine($"- {group.Key.ToStableKey()} / {group.Key.ToDisplayName()}: {group.Count()}");
            }

            builder.AppendLine();
            builder.AppendLine("## Required Fields");
            builder.AppendLine("- Every item has itemId / displayName / faMenTag / qiLeiTag / shapeId / coreCellLocal.");
            builder.AppendLine("- Every item has shapeCells, default rarity, allowed rarities, power text, stats, trigger/effect/affix/placement/flavor/icon placeholder fields.");
            builder.AppendLine("- `coreCellLocal` is included in each item's `shapeCells`.");
            builder.AppendLine();
            AppendResultList(builder, "Notes", result.Notes);
            AppendResultList(builder, "Errors", result.Errors);
            return builder.ToString();
        }

        private static string BuildSpecCsv(IReadOnlyList<ItemInnerDataDefinition> items)
        {
            StringBuilder builder = new();
            builder.AppendLine("itemId,displayName,itemFamily,faMenTag,qiLeiTag,shapeId,shapeCells,coreCellLocal,displayRarityName,rarityDefault,allowedRarities,itemPower,isLightingSource,iconPlaceholderKey,validation");
            foreach (ItemInnerDataDefinition item in items)
            {
                string[] row =
                {
                    item.itemId,
                    item.displayName,
                    item.ItemFamilyKey,
                    item.FaMenKey,
                    item.QiLeiKey,
                    item.shapeId,
                    item.FormatCells(),
                    ItemInnerDataDefinition.FormatCell(item.coreCellLocal),
                    item.displayRarityName,
                    item.rarityDefault.ToStableKey(),
                    item.FormatAllowedRarities(),
                    item.itemPower,
                    item.isLightingSource ? "true" : "false",
                    item.iconPlaceholderKey,
                    item.HasRequiredFields() ? "PASS" : "FAIL"
                };
                builder.AppendLine(string.Join(",", row.Select(EscapeCsv)));
            }

            return builder.ToString();
        }

        private static string BuildLeakCheckReport(VerificationResult result)
        {
            StringBuilder builder = new();
            builder.AppendLine("# ItemInnerDataCatalog01 Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"- Result: {(result.Errors.Count == 0 ? "PASS" : "FAIL")}");
            builder.AppendLine("- BuildSettings: checked readonly; not modified; Item Sandbox scene is not registered.");
            builder.AppendLine("- Formal flow: no V0.3 RunFlow, V0.4 Battle system, UnifiedBattlePage, Boss, Reward, SaveData, or formal scene integration.");
            builder.AppendLine("- Real systems not implemented: lighting, adjacent relay, Build, awakening, drop/acquire, upgrade/reroll, save, battle settlement.");
            builder.AppendLine("- Scene boundary: only `Scene_TalismanBag_V04_ItemSandbox.unity` is used for readonly catalog preview.");
            builder.AppendLine("- ItemDetailViewModel boundary: CatalogProvider projects catalog data into ViewModel; UI does not read formal battle/runtime objects.");
            builder.AppendLine("- Forbidden catalog entry: 阵眼石 is not in the catalog.");
            builder.AppendLine("- Lighting source marker: only 聚念石 is marked as first-version lighting source identity.");
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

        private sealed class VerificationResult
        {
            public readonly List<string> Errors = new();
            public readonly List<string> Notes = new();
        }
    }
}
#endif
