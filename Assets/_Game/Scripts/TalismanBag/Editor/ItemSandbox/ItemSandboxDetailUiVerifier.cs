#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TalismanBag.ItemSandbox;
using TalismanBag.Items.Detail;
using TalismanBag.Items.InnerCatalog;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace TalismanBag.EditorTools.ItemSandbox
{
    public static class ItemSandboxDetailUiVerifier
    {
        private const string DetailReportPath = "Docs/V0.4/Reports/ItemSandboxDetailUiReport.md";
        private const string ViewModelSpecPath = "Docs/V0.4/Reports/ItemDetailViewModelSpec.csv";
        private const string LeakCheckReportPath = "Docs/V0.4/Reports/ItemSandboxDetailUiLeakCheckReport.md";

        private static readonly string[] CreatedSourcePaths =
        {
            "Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailViewModel.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/InnerCatalog/ItemInnerDataTags.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/InnerCatalog/ItemInnerDataDefinition.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/InnerCatalog/ItemInnerDataCatalog.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/InnerCatalog/ItemInnerDataCatalogProvider.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxDevStubProvider.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxItemButtonView.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxDetailSectionView.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxDetailPanelView.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxDetailUiController.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemSandboxDetailUiSceneBuilder.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemSandboxDetailUiVerifier.cs"
        };

        private static readonly string[] ForbiddenSourceTokens =
        {
            "BuildSynergyResolver",
            "BattleResolver",
            "BattleBridge",
            "UnifiedBattlePage",
            "V02RunFlow",
            "V03RunFlow",
            "Scene_TalismanBag_V04_BattleSandboxPreview",
            "Scene_TalismanBag_V03",
            "Scene_TalismanBag_V02",
            "EditorBuildSettings.scenes ="
        };

        [MenuItem("Tools/Talisman Bag/V0.4/ItemSandbox/ItemSandboxDetailUi01/[Guard Only] Verify And Write Reports")]
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

            try
            {
                RunChecks(result);
                WriteReports(result);

                if (result.Errors.Count == 0)
                {
                    Debug.Log("ItemSandboxDetailUi01 verification passed and reports were written.");
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
                WriteReports(result);
                if (exitWhenBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
        }

        private static void RunChecks(VerificationResult result)
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
                result.Errors.Add("Item sandbox scene is present in BuildSettings; this package must keep it manual-only.");
            }
            else
            {
                result.Notes.Add("BuildSettings check passed: item sandbox scene is not registered.");
            }

            Scene scene = EditorSceneManager.OpenScene(ItemSandboxDetailUiSceneBuilder.ScenePath, OpenSceneMode.Single);
            CheckHierarchyNames(scene, result);
            CheckComponents(result);
            CheckSourceScope(result);
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
                result.Notes.Add("Hierarchy naming check passed: all GameObject names are English stable names.");
            }
        }

        private static void CheckComponents(VerificationResult result)
        {
            ItemInnerDataCatalogProvider catalogProvider = Object.FindObjectsOfType<ItemInnerDataCatalogProvider>(true).FirstOrDefault();
            ItemSandboxDevStubProvider stubProvider = Object.FindObjectsOfType<ItemSandboxDevStubProvider>(true).FirstOrDefault();
            IItemDetailViewModelProvider provider = catalogProvider != null ? catalogProvider : stubProvider;
            ItemSandboxDetailUiController controller = Object.FindObjectsOfType<ItemSandboxDetailUiController>(true).FirstOrDefault();
            ItemSandboxDetailPanelView detailPanel = Object.FindObjectsOfType<ItemSandboxDetailPanelView>(true).FirstOrDefault();
            ItemSandboxItemButtonView[] buttons = Object.FindObjectsOfType<ItemSandboxItemButtonView>(true);
            ItemSandboxDetailSectionView[] sections = Object.FindObjectsOfType<ItemSandboxDetailSectionView>(true);

            if (provider == null)
            {
                result.Errors.Add("Missing Item detail provider.");
                return;
            }

            if (catalogProvider != null)
            {
                if (!catalogProvider.DevOnly)
                {
                    result.Errors.Add("ItemInnerDataCatalogProvider is not marked dev-only.");
                }

                if (catalogProvider.ReadsFormalBattleObject || catalogProvider.ReadsFormalSaveData || catalogProvider.WritesFormalSystem)
                {
                    result.Errors.Add("ItemInnerDataCatalogProvider formal read/write guard flags are not all false.");
                }
                else
                {
                    result.Notes.Add("CatalogProvider guard flags passed: no formal battle object, save data, or formal system access.");
                }

                if (catalogProvider.CatalogCount == 31)
                {
                    result.Notes.Add("CatalogProvider is active and exposes 31 catalog items.");
                }
                else
                {
                    result.Errors.Add($"CatalogProvider item count is {catalogProvider.CatalogCount}; expected 31.");
                }
            }
            else if (stubProvider != null)
            {
                if (!stubProvider.DevOnly)
                {
                    result.Errors.Add("DevStubProvider is not marked dev-only.");
                }

                if (stubProvider.ReadsFormalBattleObject || stubProvider.ReadsFormalSaveData || stubProvider.WritesFormalSystem)
                {
                    result.Errors.Add("DevStubProvider formal read/write guard flags are not all false.");
                }
                else
                {
                    result.Notes.Add("DevStubProvider guard flags passed: no formal battle object, save data, or formal system access.");
                }
            }

            if (controller == null)
            {
                result.Errors.Add("Missing ItemSandboxDetailUiController.");
            }

            if (detailPanel == null)
            {
                result.Errors.Add("Missing ItemSandboxDetailPanelView.");
            }

            if (sections.Length < 12)
            {
                result.Errors.Add($"Detail section count is {sections.Length}; expected at least 12 assignment fields after the card header.");
            }

            IReadOnlyList<ItemDetailListEntry> entries = provider.GetItemList();
            if (buttons.Length != entries.Count)
            {
                result.Errors.Add($"Item button count mismatch. Buttons={buttons.Length}, entries={entries.Count}.");
            }

            Dictionary<string, string> namesById = entries.ToDictionary(entry => entry.itemId, entry => entry.displayItemName);
            foreach (ItemSandboxItemButtonView button in buttons)
            {
                if (!namesById.TryGetValue(button.ItemId, out string expectedName))
                {
                    result.Errors.Add($"Button uses unknown item id: {button.ItemId}");
                    continue;
                }

                if (!string.Equals(button.DisplayName, expectedName, StringComparison.Ordinal))
                {
                    result.Errors.Add($"Button label mismatch for {button.ItemId}. Expected only display name '{expectedName}', got '{button.DisplayName}'.");
                }
            }

            List<ItemDetailViewModel> models = entries
                .Select(entry => provider.GetDetailViewModel(entry.itemId))
                .Where(model => model != null)
                .ToList();
            if (catalogProvider != null)
            {
                CheckCatalogCoverage(catalogProvider.GetCatalogItems(), result);
            }
            else
            {
                CheckStubCoverage(models, result);
            }
        }

        private static void CheckCatalogCoverage(IReadOnlyList<ItemInnerDataDefinition> items, VerificationResult result)
        {
            if (items.Count != 31)
            {
                result.Errors.Add($"Catalog data count is {items.Count}; expected 31.");
            }

            if (items.Any(item => !item.HasRequiredFields()))
            {
                result.Errors.Add("Catalog data includes item(s) with missing required fields.");
            }

            if (items.Any(item => item.displayName.Contains("阵眼石") || item.itemId.IndexOf("eye", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                result.Errors.Add("Catalog data appears to include eye stone data, which is forbidden.");
            }

            int lightingSourceCount = items.Count(item => item.isLightingSource);
            if (lightingSourceCount != 1 || items.FirstOrDefault(item => item.isLightingSource)?.itemId != "I031")
            {
                result.Errors.Add("Catalog lighting source marker must be unique and assigned only to I031 聚念石.");
            }

            if (result.Errors.All(error => !error.StartsWith("Catalog data", StringComparison.Ordinal)
                    && !error.StartsWith("Catalog lighting", StringComparison.Ordinal)
                    && !error.Contains("eye stone")))
            {
                result.Notes.Add("Catalog coverage passed: 31 items, required fields, no eye stone, unique 聚念石 lighting source.");
            }
        }

        private static void CheckStubCoverage(IReadOnlyList<ItemDetailViewModel> models, VerificationResult result)
        {
            if (!models.Any(model => model.statusFlags.isLit))
            {
                result.Errors.Add("Stub data does not include a lit item.");
            }

            if (!models.Any(model => !model.statusFlags.isLit))
            {
                result.Errors.Add("Stub data does not include an unlit item.");
            }

            if (!models.Any(model => model.statusFlags.coreEffectUnlocked))
            {
                result.Errors.Add("Stub data does not include an awakened item.");
            }

            if (!models.Any(model => !model.statusFlags.coreEffectUnlocked))
            {
                result.Errors.Add("Stub data does not include an unawakened item.");
            }

            if (!models.Any(model => model.statusFlags.isOnArrayBonusCell && model.statusFlags.isArrayBonusActive))
            {
                result.Errors.Add("Stub data does not include an active array bonus sample.");
            }

            if (!models.Any(model => model.statusFlags.isOnArrayBonusCell && !model.statusFlags.isArrayBonusActive))
            {
                result.Errors.Add("Stub data does not include an inactive array bonus sample.");
            }

            if (result.Errors.All(error => !error.StartsWith("Stub data", StringComparison.Ordinal)))
            {
                result.Notes.Add("Stub coverage passed: lit/unlit, awakened/unawakened, and array bonus active/inactive samples exist.");
            }
        }

        private static void CheckSourceScope(VerificationResult result)
        {
            foreach (string sourcePath in CreatedSourcePaths)
            {
                if (!File.Exists(sourcePath))
                {
                    result.Errors.Add($"Missing source file: {sourcePath}");
                    continue;
                }

                if (sourcePath.EndsWith("ItemSandboxDetailUiVerifier.cs", StringComparison.Ordinal))
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

            result.Notes.Add("Source scope check completed for Item Sandbox Detail UI files.");
        }

        private static void WriteReports(VerificationResult result)
        {
            Directory.CreateDirectory("Docs/V0.4/Reports");
            File.WriteAllText(DetailReportPath, BuildDetailReport(result), new UTF8Encoding(false));
            File.WriteAllText(ViewModelSpecPath, BuildViewModelSpecCsv(), new UTF8Encoding(false));
            File.WriteAllText(LeakCheckReportPath, BuildLeakCheckReport(result), new UTF8Encoding(false));
            AssetDatabase.Refresh();
        }

        private static string BuildDetailReport(VerificationResult result)
        {
            StringBuilder builder = new();
            builder.AppendLine("# ItemSandboxDetailUi01 Report");
            builder.AppendLine();
            builder.AppendLine($"- Scene: `{ItemSandboxDetailUiSceneBuilder.ScenePath}`");
            builder.AppendLine("- Package: `TASK_START_ITEMSANDBOX_DETAILUI01`");
            builder.AppendLine("- Guard receipt target: `GUARD_PASS_ITEMSANDBOX_DETAILUI01`");
            builder.AppendLine($"- Verification: {(result.Errors.Count == 0 ? "PASS" : "FAIL")}");
            builder.AppendLine();
            builder.AppendLine("## Implemented");
            builder.AppendLine("- Created an independent V0.4 Item Sandbox scene with an old-paper single-column item detail card.");
            builder.AppendLine("- Added a greybox item name list; each button label is only the Chinese display name.");
            builder.AppendLine("- Clicking an item refreshes the detail card through `IItemDetailViewModelProvider` and `ItemDetailViewModel`.");
            builder.AppendLine("- Added dev stub data covering lit, unlit, unawakened, awakened, inactive array bonus, and active array bonus states.");
            builder.AppendLine("- Applied the approved 1080x1920 typography conversion and five-rarity palette: Fan #E3D8C3, Liang #87B66A, Ling #68A9E6, Xuan #B27DDF, Dao #E4A14B; shared #D8CCB7 / #998F7C / #B99A61 / #C94E3B / #748D62 / #82AE4F / #716F69.");
            builder.AppendLine("- Reserved future preview provider interfaces without wiring formal lighting, build, awakening, acquire, upgrade, or battle-effect systems.");
            builder.AppendLine();
            builder.AppendLine("## Field Order");
            builder.AppendLine("1. 道具名");
            builder.AppendLine("2. 品阶换行；法门 / 器类 · 中文形状");
            builder.AppendLine("3. 物品强度（品阶色动态数字）");
            builder.AppendLine("4. 主属性");
            builder.AppendLine("5. 触发说明");
            builder.AppendLine("6. 点亮 / 接亮 / 开窍状态");
            builder.AppendLine("7. 固定词条");
            builder.AppendLine("8. 随机词条");
            builder.AppendLine("9. 分隔线");
            builder.AppendLine("10. 橙色词条 / 道痕");
            builder.AppendLine("11. 基础效果");
            builder.AppendLine("12. 核心效果");
            builder.AppendLine("13. 法门 / Build");
            builder.AppendLine("14. 器类 / Build");
            builder.AppendLine("15. 分隔线");
            builder.AppendLine("16. 摆放提示");
            builder.AppendLine("17. 鉴定笺");
            builder.AppendLine();
            AppendResultList(builder, "Notes", result.Notes);
            AppendResultList(builder, "Errors", result.Errors);
            return builder.ToString();
        }

        private static string BuildViewModelSpecCsv()
        {
            string[][] rows =
            {
                new[] { "field_order", "view_model_field", "ui_label", "source", "notes" },
                new[] { "1", "displayItemName", "道具名", "ItemDetailViewModel", "玩家显示文案" },
                new[] { "2", "displayRarityName/displayFaMenName/displayQiLeiName/displayShapeName", "品阶换行；法门 / 器类 · 中文形状", "ItemDetailViewModel", "详情卡头部元信息；不显示 raw shape id" },
                new[] { "3", "displayItemPower", "物品强度（品阶色动态数字）", "ItemDetailViewModel", "动态读取 Provider 数字；缺失时显示 --；不写死数值，不参与战斗计算" },
                new[] { "4", "displayPrimaryStats", "主属性", "List<ItemDetailStatLine>", "只展示，不做放置或数值计算" },
                new[] { "5", "displayTriggerText", "触发说明", "ItemDetailViewModel", "只展示 stub 触发文本" },
                new[] { "6", "displayLightingStatusText/displayAwakeningStatusText/displayArrayBonusStatusText/statusFlags", "点亮 / 接亮 / 开窍状态", "ItemDetailViewModel", "状态来自 stub，不做真实点亮计算" },
                new[] { "7", "displayFixedAffixes", "固定词条", "List<ItemDetailTextLine>", "只展示" },
                new[] { "8", "displayRandomAffixes", "随机词条", "List<ItemDetailTextLine>", "只展示" },
                new[] { "9", "divider", "分隔线", "UI layout", "无数据绑定" },
                new[] { "10", "displayOrangeAffix", "橙色词条 / 道痕", "ItemDetailViewModel", "为空时隐藏" },
                new[] { "11", "displayBasicEffects", "基础效果", "List<ItemDetailTextLine>", "已点亮时标记 active" },
                new[] { "12", "displayCoreEffects", "核心效果", "List<ItemDetailTextLine>", "已点亮 + 已开窍才标记 active" },
                new[] { "13", "displayFaMenBuilds", "法门 / Build", "List<ItemDetailBuildPreview>", "只预览，不接 BuildSynergyResolver" },
                new[] { "14", "displayQiLeiBuilds", "器类 / Build", "List<ItemDetailBuildPreview>", "只预览，不接 BuildSynergyResolver" },
                new[] { "15", "divider", "分隔线", "UI layout", "无数据绑定" },
                new[] { "16", "displayPlacementTips", "摆放提示", "List<ItemDetailTextLine>", "只展示，不做坐标计算" },
                new[] { "17", "displayFlavorText", "鉴定笺", "ItemDetailViewModel", "旧纸符器鉴定卡收束文本" },
                new[] { "reserved", "IItemLightingPreviewProvider", "点亮预览接口", "reserved interface", "预留但不接真实点亮系统" },
                new[] { "reserved", "IItemBuildPreviewProvider", "Build 预览接口", "reserved interface", "预留但不接真实 Build 计算" },
                new[] { "reserved", "IItemAwakeningPreviewProvider", "开窍预览接口", "reserved interface", "预留但不接养成系统" },
                new[] { "reserved", "IItemAcquirePreviewProvider", "获得来源预览接口", "reserved interface", "预留但不接正式掉落 / 奖励" },
                new[] { "reserved", "IItemUpgradePreviewProvider", "升级预览接口", "reserved interface", "预留但不接升级 / 存档" },
                new[] { "reserved", "IItemBattleEffectPreviewProvider", "战斗效果预览接口", "reserved interface", "预留但不接 BattleResolver / Battle Bridge" }
            };

            StringBuilder builder = new();
            foreach (string[] row in rows)
            {
                builder.AppendLine(string.Join(",", row.Select(EscapeCsv)));
            }

            return builder.ToString();
        }

        private static string BuildLeakCheckReport(VerificationResult result)
        {
            StringBuilder builder = new();
            builder.AppendLine("# ItemSandboxDetailUi01 Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"- Result: {(result.Errors.Count == 0 ? "PASS" : "FAIL")}");
            builder.AppendLine($"- Scene path: `{ItemSandboxDetailUiSceneBuilder.ScenePath}`");
            builder.AppendLine("- BuildSettings: not modified by builder; verifier asserts the scene is not registered.");
            builder.AppendLine("- Formal flow: no V0.3 RunFlow, UnifiedBattlePage, Battle sandbox scene, Boss, Reward, or formal save integration is used by this package.");
            builder.AppendLine("- Real systems: lighting, relay, Build synergy, awakening, acquire, upgrade, BattleResolver, and Battle Bridge remain stubbed or interface-only.");
            builder.AppendLine("- Hierarchy naming: GameObject names are checked for Chinese characters; Chinese text is limited to player-facing UI labels and content.");
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
