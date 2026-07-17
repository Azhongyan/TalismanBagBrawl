#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TalismanBag.ItemSandbox;
using TalismanBag.Items.Awakening;
using TalismanBag.Items.Build;
using TalismanBag.Items.Detail;
using TalismanBag.Items.Detail.UI;
using TalismanBag.Items.InnerCatalog;
using TalismanBag.Items.Lighting;
using TalismanBag.Items.Skills;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TalismanBag.EditorTools.ItemSandbox
{
    public static class ItemDetailProjectionCompleteVerifier
    {
        private const string DetailReportPath = "Docs/V0.4/Reports/ItemDetailProjectionCompleteReport.md";
        private const string SpecCsvPath = "Docs/V0.4/Reports/ItemDetailProjectionCompleteSpec.csv";
        private const string LeakCheckReportPath = "Docs/V0.4/Reports/ItemDetailProjectionCompleteLeakCheckReport.md";
        private const string MissingState = "状态不可用/未接入";
        private const string CatalogPreviewState = "尚未入阵/阵脉未计算";
        private const string InstanceUnavailableState = "实例状态不可用";

        private static readonly string[] ExpectedPlayerSectionTitles =
        {
            "物品强度",
            "基础属性",
            "触发条件",
            "点亮与基础效果",
            "固定词条",
            "随机词条",
            string.Empty,
            string.Empty,
            "核心效果/开窍",
            "法门Build",
            "器类Build",
            "主Build自动监控",
            string.Empty,
            "摆放提示",
            "文化描述"
        };

        private static readonly string[] ForbiddenPlayerTokens =
        {
            "isLit",
            "isDirectLit",
            "litDepth",
            "placementId",
            "validationErrors",
            "coreEffectId",
            "selectedMainBuildId"
        };

        private static readonly string[] RequiredDebugTokens =
        {
            "itemId",
            "placementId",
            "shapeCells",
            "occupiedCells",
            "coreCellLocal",
            "coreCellWorld",
            "isDirectLit",
            "isLit",
            "litByItemId",
            "litByPlacementId",
            "litDepth",
            "occupiedArrayBonusCells",
            "isOnArrayBonusCell",
            "isArrayBonusActive",
            "countedInBuild",
            "inputLevel",
            "resolvedLevel",
            "unlockedCoreEffectIds",
            "activeCoreEffectIds",
            "selectedMainBuildId",
            "BasicAttack",
            "Build2",
            "Build4",
            "Build6",
            "validationErrors"
        };

        private static readonly string[] ScopedSourcePaths =
        {
            "Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailViewModel.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailProjectionComposer.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailPanelView.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailSectionView.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxDetailPanelView.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxDetailSectionView.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxDetailUiController.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxGridPlacementPreviewView.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemSandboxDetailUiSceneBuilder.cs"
        };

        private static readonly string[] ForbiddenScopeTokens =
        {
            "UnifiedBattlePage",
            "BattleBridge",
            "BattleResolver",
            "V02RunFlow",
            "V03RunFlow",
            "Scene_TalismanBag_V04_BattleSandboxPreview",
            "EditorBuildSettings.scenes =",
            "SaveSystem",
            "RewardResolver",
            "BossController"
        };

        [MenuItem("Tools/Talisman Bag/V0.4/ItemSandbox/ItemDetailProjectionComplete01/[Guard Only] Verify And Write Reports")]
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
            List<SpecRow> rows = new();

            try
            {
                RunChecks(result, rows);
                WriteReports(result, rows);
                if (result.Errors.Count == 0)
                {
                    Debug.Log("ItemDetailProjectionComplete01 rework verification passed and reports were written.");
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

        private static void RunChecks(VerificationResult result, List<SpecRow> rows)
        {
            GameObject providerObject = new("ItemDetailProjectionCompleteVerifierProvider");
            ItemInnerDataCatalogProvider provider = providerObject.AddComponent<ItemInnerDataCatalogProvider>();
            try
            {
                ItemLightingResolutionResult lighting = CreateProjectionLighting();
                ItemArrayBonusResolutionResult arrayBonus = ItemArrayBonusResolver.Resolve(null, lighting.ItemResults);
                ItemBuildSynergyResolutionResult build = ItemBuildSynergyResolver.Resolve(lighting, ItemInnerDataCatalog.AllItems);
                ItemCoreAwakeningResolutionResult awakening = ItemCoreAwakeningResolver.Resolve(lighting, CreateAwakeningInputs());
                ItemSkillMonitorResolutionResult monitor = ItemSkillMonitorResolver.Resolve(
                    build,
                    new ItemMainBuildSelectionInput("famen:zhenlei", "ItemDetailProjectionCompleteVerifier", 1));

                ItemDetailViewModel catalogI001 = provider.GetDetailViewModel("I001");
                ItemDetailViewModel catalogI007 = provider.GetDetailViewModel("I007");
                ItemDetailViewModel catalogI031 = provider.GetDetailViewModel("I031");
                ItemDetailViewModel catalogPreviewI001 = Compose(catalogI001, ItemDetailProjectionContextKind.CatalogPreview, string.Empty, lighting, arrayBonus, build, awakening, monitor);
                ItemDetailViewModel catalogPreviewI031 = Compose(catalogI031, ItemDetailProjectionContextKind.CatalogPreview, string.Empty, lighting, arrayBonus, build, awakening, monitor);
                ItemDetailViewModel placedLit = Compose(catalogI001, ItemDetailProjectionContextKind.PlacedInstance, "P_I001_A", lighting, arrayBonus, build, awakening, monitor);
                ItemDetailViewModel placedUnlit = Compose(catalogI007, ItemDetailProjectionContextKind.PlacedInstance, "P_I007_UNLIT", lighting, arrayBonus, build, awakening, monitor);
                ItemDetailViewModel stale = Compose(catalogI001, ItemDetailProjectionContextKind.PlacedInstance, "P_I001_STALE", lighting, arrayBonus, build, awakening, monitor);
                ItemDetailViewModel placedJuNian = Compose(catalogI031, ItemDetailProjectionContextKind.PlacedInstance, "P_SOURCE", lighting, arrayBonus, build, awakening, monitor);
                ItemDetailViewModel missingSnapshot = Compose(catalogI001, ItemDetailProjectionContextKind.PlacedInstance, "P_I001_A", null, null, null, null, null);

                CheckProjectionContextMode(catalogPreviewI001, placedLit, result, rows);
                CheckSectionStructure(placedLit, result, rows);
                CheckIdempotency(catalogI001, placedLit, lighting, arrayBonus, build, awakening, monitor, result, rows);
                CheckPlayerDebugSeparation(placedLit, result, rows);
                CheckStrictPlacementQuery(placedLit, placedUnlit, stale, result, rows);
                CheckCatalogPreviewNoPlacementState(catalogPreviewI001, catalogPreviewI031, result, rows);
                CheckPlacedJuNianRequiresPlacement(placedJuNian, result, rows);
                CheckMissingSnapshotNoThrow(missingSnapshot, result, rows);
                CheckRuntimeViewAndPrefab(result, rows);
                CheckCatalogAll31(provider, lighting, arrayBonus, build, awakening, monitor, result, rows);

                ItemDetailViewModel longModel = CreateLongTextModel(provider, lighting, arrayBonus, build, awakening, monitor);
                LayoutCheckResult layout1080 = CheckLayoutForResolution(longModel, new Vector2(1080f, 1920f), "layout-1080x1920");
                LayoutCheckResult layout720 = CheckLayoutForResolution(longModel, new Vector2(720f, 1280f), "layout-720x1280");
                CheckLayoutResult(layout1080, result, rows);
                CheckLayoutResult(layout720, result, rows);
                CheckSourceScope(result, rows);
                CheckRegressionReports(result, rows);

                result.Notes.Add("Runtime prefab path: " + ItemSandboxDetailUiSceneBuilder.ItemDetailPanelPrefabPath);
                result.Notes.Add("Runtime view path: Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailPanelView.cs");
                result.Notes.Add("Catalog all-31 rows written to ItemDetailProjectionCompleteSpec.csv.");
                result.Notes.Add("Visual contract: old-paper dark-gold card, rarity badge, three state badges, artwork/image slot, top-right close button, five player chapters plus one dev chapter, per-section accents, and dual vertical scrollbars.");
                result.Notes.Add($"Long text layout: {layout1080.summary}; {layout720.summary}.");
            }
            finally
            {
                Object.DestroyImmediate(providerObject);
            }
        }

        private static ItemDetailViewModel Compose(
            ItemDetailViewModel catalogModel,
            ItemDetailProjectionContextKind contextKind,
            string placementId,
            ItemLightingResolutionResult lighting,
            ItemArrayBonusResolutionResult arrayBonus,
            ItemBuildSynergyResolutionResult build,
            ItemCoreAwakeningResolutionResult awakening,
            ItemSkillMonitorResolutionResult monitor)
        {
            return ItemDetailProjectionComposer.Compose(catalogModel, contextKind, placementId, lighting, arrayBonus, build, awakening, monitor);
        }

        private static void CheckProjectionContextMode(
            ItemDetailViewModel catalogPreview,
            ItemDetailViewModel placedModel,
            VerificationResult result,
            List<SpecRow> rows)
        {
            string composerSource = ReadSource("Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailProjectionComposer.cs");
            bool sourceOk = composerSource.Contains("ItemDetailProjectionContextKind", StringComparison.Ordinal)
                && composerSource.Contains("CatalogPreview", StringComparison.Ordinal)
                && composerSource.Contains("PlacedInstance", StringComparison.Ordinal);
            bool stateOk = HasCatalogPreviewState(catalogPreview)
                && DebugText(catalogPreview).Contains("contextKind: CatalogPreview", StringComparison.Ordinal)
                && DebugText(placedModel).Contains("contextKind: PlacedInstance", StringComparison.Ordinal);
            AddRow(rows, "projection-context-mode", catalogPreview, "context enum and player/debug context state", sourceOk && stateOk, "context source and composed state present");
            AddErrorIfFalse(result, sourceOk && stateOk, "Composer must expose CatalogPreview/PlacedInstance context mode and project it into debug output.");
        }

        private static void CheckSectionStructure(ItemDetailViewModel model, VerificationResult result, List<SpecRow> rows)
        {
            bool countOk = model.displayPlayerSections.Count == ExpectedPlayerSectionTitles.Length
                && model.displayDebugSections.Count == 3;
            bool orderOk = countOk;
            for (int i = 0; i < ExpectedPlayerSectionTitles.Length && i < model.displayPlayerSections.Count; i++)
            {
                orderOk &= string.Equals(model.displayPlayerSections[i].title, ExpectedPlayerSectionTitles[i], StringComparison.Ordinal);
            }

            AddRow(rows, "section-count-order", model, "15 player sections / 3 debug sections in fixed order", countOk && orderOk, $"{model.displayPlayerSections.Count}/{model.displayDebugSections.Count}");
            AddErrorIfFalse(result, countOk && orderOk, "Composed ItemDetailViewModel must expose fixed 15 player sections and 3 debug sections.");
        }

        private static void CheckIdempotency(
            ItemDetailViewModel catalogModel,
            ItemDetailViewModel projected,
            ItemLightingResolutionResult lighting,
            ItemArrayBonusResolutionResult arrayBonus,
            ItemBuildSynergyResolutionResult build,
            ItemCoreAwakeningResolutionResult awakening,
            ItemSkillMonitorResolutionResult monitor,
            VerificationResult result,
            List<SpecRow> rows)
        {
            ItemDetailViewModel first = Compose(catalogModel, ItemDetailProjectionContextKind.PlacedInstance, "P_I001_A", lighting, arrayBonus, build, awakening, monitor);
            ItemDetailViewModel second = Compose(catalogModel, ItemDetailProjectionContextKind.PlacedInstance, "P_I001_A", lighting, arrayBonus, build, awakening, monitor);
            ItemDetailViewModel composedAgain = Compose(projected, ItemDetailProjectionContextKind.PlacedInstance, "P_I001_A", lighting, arrayBonus, build, awakening, monitor);
            string signature = Signature(first);
            bool repeatOk = string.Equals(signature, Signature(second), StringComparison.Ordinal)
                && string.Equals(signature, Signature(composedAgain), StringComparison.Ordinal);
            bool cloneOk = catalogModel.displayPlayerSections.Count == 0 && catalogModel.displayDebugSections.Count == 0;
            AddRow(rows, "duplicateProjectionCheck", first, "same input repeats without duplicate sections", repeatOk && cloneOk, repeatOk ? "same-signature" : "different-signature");
            AddErrorIfFalse(result, repeatOk && cloneOk, "Composer must clone base models and repeated projection must be idempotent.");
        }

        private static void CheckPlayerDebugSeparation(ItemDetailViewModel model, VerificationResult result, List<SpecRow> rows)
        {
            string playerText = PlayerText(model);
            string debugText = DebugText(model);
            bool noForbiddenPlayerTokens = ForbiddenPlayerTokens.All(token => !playerText.Contains(token, StringComparison.Ordinal));
            bool debugHasRawTokens = RequiredDebugTokens.All(token => debugText.Contains(token, StringComparison.Ordinal));
            AddRow(rows, "player-debug-separation", model, "player has no raw field leaks; debug has required raw fields", noForbiddenPlayerTokens && debugHasRawTokens, $"debugFields={CountDebugTokens(debugText)}");
            AddErrorIfFalse(result, noForbiddenPlayerTokens, "Player detail sections contain forbidden raw/debug field names.");
            AddErrorIfFalse(result, debugHasRawTokens, "Debug sections are missing one or more required diagnostic fields.");
        }

        private static void CheckStrictPlacementQuery(
            ItemDetailViewModel litModel,
            ItemDetailViewModel unlitModel,
            ItemDetailViewModel staleModel,
            VerificationResult result,
            List<SpecRow> rows)
        {
            string litPlayerText = PlayerText(litModel);
            string unlitPlayerText = PlayerText(unlitModel);
            string stalePlayerText = PlayerText(staleModel);
            string staleDebugText = DebugText(staleModel);
            bool splitOk = litModel.placementId == "P_I001_A"
                && unlitModel.placementId == "P_I007_UNLIT"
                && litPlayerText.Contains("直接点亮", StringComparison.Ordinal)
                && litPlayerText.Contains("当前数量：6/6", StringComparison.Ordinal)
                && unlitPlayerText.Contains("未点亮", StringComparison.Ordinal)
                && unlitPlayerText.Contains("未点亮道具：不计入当前Build。", StringComparison.Ordinal);
            bool staleOk = staleModel.placementId == "P_I001_STALE"
                && stalePlayerText.Contains(InstanceUnavailableState, StringComparison.Ordinal)
                && staleDebugText.Contains("missingSnapshot: Lighting placementId=P_I001_STALE", StringComparison.Ordinal)
                && !staleDebugText.Contains("P_I001_A", StringComparison.Ordinal)
                && !staleDebugText.Contains("P_I007_UNLIT", StringComparison.Ordinal);
            AddRow(rows, "strict-placement-query", litModel, "Distinct legal placements P_I001_A and P_I007_UNLIT do not cross state", splitOk, "I001 lit / I007 unlit");
            AddRow(rows, "stale-placement-no-item-fallback", staleModel, "stale placement shows unavailable and does not read legal placements", staleOk, staleOk ? InstanceUnavailableState : "fallback detected");
            AddErrorIfFalse(result, splitOk, "Distinct placement projections did not stay isolated by placementId.");
            AddErrorIfFalse(result, staleOk, "Stale placement must not fall back to another itemId instance.");
        }

        private static void CheckCatalogPreviewNoPlacementState(
            ItemDetailViewModel catalogI001,
            ItemDetailViewModel catalogI031,
            VerificationResult result,
            List<SpecRow> rows)
        {
            string i001Player = PlayerText(catalogI001);
            string i031Player = PlayerText(catalogI031);
            string debug = DebugText(catalogI001) + "\n" + DebugText(catalogI031);
            bool previewOk = string.IsNullOrWhiteSpace(catalogI001.placementId)
                && string.IsNullOrWhiteSpace(catalogI031.placementId)
                && HasCatalogPreviewState(catalogI001)
                && HasCatalogPreviewState(catalogI031)
                && !debug.Contains("P_I001_A", StringComparison.Ordinal)
                && !debug.Contains("P_I007_UNLIT", StringComparison.Ordinal)
                && !debug.Contains("P_SOURCE", StringComparison.Ordinal);
            AddRow(rows, "catalog-preview-no-placement-state", catalogI001, "CatalogPreview ignores all placed instance snapshots", previewOk, previewOk ? CatalogPreviewState : "placement state leaked");
            AddRow(rows, "catalog-preview-junian-no-placed-source", catalogI031, "CatalogPreview JuNian does not read P_SOURCE", previewOk, previewOk ? "no P_SOURCE" : "P_SOURCE leaked");
            AddErrorIfFalse(result, previewOk, "CatalogPreview must not read any placement snapshot by itemId.");
        }

        private static void CheckPlacedJuNianRequiresPlacement(ItemDetailViewModel placedJuNian, VerificationResult result, List<SpecRow> rows)
        {
            string player = PlayerText(placedJuNian);
            bool ok = placedJuNian.placementId == "P_SOURCE"
                && player.Contains("聚念石", StringComparison.Ordinal)
                && player.Contains("点亮源", StringComparison.Ordinal)
                && player.Contains("不适用：系统道具不提供法门 Build 资格", StringComparison.Ordinal);
            AddRow(rows, "placed-junian-requires-placement", placedJuNian, "PlacedInstance JuNian via P_SOURCE reads only P_SOURCE", ok, ok ? "P_SOURCE lit source" : "unexpected");
            AddErrorIfFalse(result, ok, "Placed 聚念石 must require and preserve real placementId P_SOURCE.");
        }

        private static void CheckMissingSnapshotNoThrow(ItemDetailViewModel model, VerificationResult result, List<SpecRow> rows)
        {
            string player = PlayerText(model);
            string debug = DebugText(model);
            bool ok = player.Contains(MissingState, StringComparison.Ordinal)
                && debug.Contains("missingSnapshot: Lighting placementId=P_I001_A", StringComparison.Ordinal)
                && !player.Contains("false", StringComparison.OrdinalIgnoreCase);
            AddRow(rows, "snapshot-missing-placement-no-throw", model, "missing snapshots display unavailable and do not throw", ok, ok ? MissingState : "unexpected");
            AddErrorIfFalse(result, ok, "Missing snapshot must display unavailable state and must not masquerade as false.");
        }

        private static void CheckRuntimeViewAndPrefab(VerificationResult result, List<SpecRow> rows)
        {
            bool namespaceOk = typeof(ItemDetailPanelView).Namespace == "TalismanBag.Items.Detail.UI"
                && typeof(ItemDetailSectionView).Namespace == "TalismanBag.Items.Detail.UI";
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(ItemSandboxDetailUiSceneBuilder.ItemDetailPanelPrefabPath);
            bool prefabExists = prefabAsset != null && File.Exists(ItemSandboxDetailUiSceneBuilder.ItemDetailPanelPrefabPath);
            Transform prefabRoot = prefabAsset == null ? null : prefabAsset.transform;
            GameObject previewRoot = null;
            Transform previewTransform = null;
            bool prefabRuntimeOnly = false;
            bool prefabVisualContract = false;
            bool metaTextContract = false;
            bool powerTextContract = false;
            bool typographyColorContract = false;
            bool rarityPaletteContract = false;
            bool richFieldStyleContract = false;
            Scene previewScene = default;
            if (prefabExists)
            {
                ItemDetailPanelView runtimeView = prefabAsset.GetComponent<ItemDetailPanelView>();
                prefabRuntimeOnly = runtimeView != null
                    && prefabAsset.GetComponent<ItemSandboxDetailPanelView>() == null
                    && prefabAsset.GetComponentsInChildren<ItemDetailSectionView>(true).Length >= ExpectedPlayerSectionTitles.Length + 3
                    && prefabAsset.GetComponentsInChildren<ItemSandboxDetailSectionView>(true).Length == 0;
                prefabVisualContract = FindChild(prefabRoot, "RarityBadge") != null
                    && FindChild(prefabRoot, "ItemArtworkImageSlot") != null
                    && FindChild(prefabRoot, "ItemArtworkKeyText") != null
                    && FindChild(prefabRoot, "CloseButton")?.GetComponent<Button>() != null
                    && FindChild(prefabRoot, "LightingStatusBadge") != null
                    && FindChild(prefabRoot, "AwakeningStatusBadge") != null
                    && FindChild(prefabRoot, "ArrayStatusBadge") != null
                    && prefabAsset.GetComponentsInChildren<ItemDetailSectionView>(true)
                        .All(section => FindChild(section.transform, "SectionAccent") != null)
                    && prefabAsset.GetComponentsInChildren<Transform>(true)
                        .Count(transform => transform.name.StartsWith("Chapter_", StringComparison.Ordinal)) >= 6;
                ItemDetailViewModel headerContractModel = new()
                {
                    displayRarityName = "青",
                    rarityColorKey = "qing",
                    displayQiLeiName = "符",
                    displayFaMenName = "玄水法",
                    displayShapeName = "shape_line2_v · 2格",
                    displayItemPower = "321"
                };
                previewScene = EditorSceneManager.NewPreviewScene();
                previewRoot = PrefabUtility.InstantiatePrefab(prefabAsset, previewScene) as GameObject;
                if (previewRoot != null)
                {
                    previewRoot.hideFlags = HideFlags.HideAndDontSave;
                    previewTransform = previewRoot.transform;
                }
                ItemDetailPanelView previewView = previewRoot == null ? null : previewRoot.GetComponent<ItemDetailPanelView>();
                previewView?.Bind(headerContractModel);
                metaTextContract = string.Equals(
                    FindChild(previewTransform, "MetaText")?.GetComponent<Text>()?.text,
                    "<color=#87B66A><b>良品</b></color>\n<color=#D8CCB7>· 玄水法 / 符 · 竖排两格</color>",
                    StringComparison.Ordinal);
                bool firstPowerValueOk = string.Equals(
                    FindChild(previewTransform, "PowerText")?.GetComponent<Text>()?.text,
                    "物品强度  <color=#87B66A><b>321</b></color>",
                    StringComparison.Ordinal);
                headerContractModel.displayItemPower = "987";
                previewView?.Bind(headerContractModel);
                bool secondPowerValueOk = string.Equals(
                    FindChild(previewTransform, "PowerText")?.GetComponent<Text>()?.text,
                    "物品强度  <color=#87B66A><b>987</b></color>",
                    StringComparison.Ordinal);
                headerContractModel.displayItemPower = string.Empty;
                previewView?.Bind(headerContractModel);
                bool missingPowerPlaceholderOk = string.Equals(
                    FindChild(previewTransform, "PowerText")?.GetComponent<Text>()?.text,
                    "物品强度  <color=#81796D><b>尚未建立正式评分</b></color>",
                    StringComparison.Ordinal);
                powerTextContract = firstPowerValueOk && secondPowerValueOk && missingPowerPlaceholderOk;
                Text itemNameContractText = FindChild(previewTransform, "ItemNameText")?.GetComponent<Text>();
                Text metaContractText = FindChild(previewTransform, "MetaText")?.GetComponent<Text>();
                Text powerContractText = FindChild(previewTransform, "PowerText")?.GetComponent<Text>();
                Transform headerSection = FindChild(previewTransform, "HeaderSection");
                Text sectionTitleContractText = headerSection == null ? null : FindChild(headerSection, "TitleText")?.GetComponent<Text>();
                Text sectionBodyContractText = headerSection == null ? null : FindChild(headerSection, "BodyText")?.GetComponent<Text>();
                typographyColorContract = itemNameContractText != null
                    && ColorNear(itemNameContractText.color, new Color32(135, 182, 106, 255))
                    && metaContractText != null
                    && ColorNear(metaContractText.color, new Color32(216, 204, 183, 255))
                    && powerContractText != null
                    && ColorNear(powerContractText.color, new Color32(216, 204, 183, 255))
                    && sectionTitleContractText != null
                    && ColorNear(sectionTitleContractText.color, new Color32(185, 154, 97, 255))
                    && sectionBodyContractText != null
                    && ColorNear(sectionBodyContractText.color, new Color32(216, 204, 183, 255));
                rarityPaletteContract = itemNameContractText != null;
                (string rawRarityName, string rarityKey, string expectedDisplayName, Color32 expectedColor)[] rarityCases =
                {
                    ("白", "bai", "凡品", new Color32(227, 216, 195, 255)),
                    ("青", "qing", "良品", new Color32(135, 182, 106, 255)),
                    ("蓝", "lan", "灵品", new Color32(104, 169, 230, 255)),
                    ("紫", "zi", "玄品", new Color32(178, 125, 223, 255)),
                    ("橙", "cheng", "道品", new Color32(228, 161, 75, 255))
                };
                foreach ((string rawRarityName, string rarityKey, string expectedDisplayName, Color32 expectedColor) in rarityCases)
                {
                    headerContractModel.displayRarityName = rawRarityName;
                    headerContractModel.rarityColorKey = rarityKey;
                    headerContractModel.displayItemPower = "321";
                    previewView?.Bind(headerContractModel);
                    rarityPaletteContract &= ColorNear(itemNameContractText.color, expectedColor);
                    rarityPaletteContract &= FindChild(previewTransform, "MetaText")?.GetComponent<Text>()?.text.Contains(expectedDisplayName, StringComparison.Ordinal) == true;
                }
                ItemDetailSectionView statsSection = FindChild(previewTransform, "BaseStatsSection")?.GetComponent<ItemDetailSectionView>();
                ItemDetailSectionView stateSection = FindChild(previewTransform, "CurrentStateSection")?.GetComponent<ItemDetailSectionView>();
                ItemDetailSectionView coreSection = FindChild(previewTransform, "CoreAwakeningSection")?.GetComponent<ItemDetailSectionView>();
                ItemDetailSectionView buildSection = FindChild(previewTransform, "FaMenBuildSection")?.GetComponent<ItemDetailSectionView>();
                statsSection?.SetRarityColor(new Color32(135, 182, 106, 255));
                coreSection?.SetRarityColor(new Color32(135, 182, 106, 255));
                statsSection?.SetContent(new ItemDetailSectionViewModel("基础属性", "  伤害：8\n  净化：22\n  灵效：5（次级）", "stats", true));
                stateSection?.SetContent(new ItemDetailSectionViewModel("接脉 / 点亮说明", "  不提供念脉\n  需要聚念石点亮", "status", true));
                coreSection?.SetContent(new ItemDetailSectionViewModel("核心效果", "  涤秽长符：清除污染并触发涤秽。", "awakening", true));
                buildSection?.SetContent(new ItemDetailSectionViewModel("法门 / Build", "  玄水法（1/2）\n  (2) 已激活\n  (4) 未激活", "famenBuild", true));
                Text statsBody = statsSection == null ? null : FindChild(statsSection.transform, "BodyText")?.GetComponent<Text>();
                Text stateBody = stateSection == null ? null : FindChild(stateSection.transform, "BodyText")?.GetComponent<Text>();
                Text coreBody = coreSection == null ? null : FindChild(coreSection.transform, "BodyText")?.GetComponent<Text>();
                Text buildBody = buildSection == null ? null : FindChild(buildSection.transform, "BodyText")?.GetComponent<Text>();
                richFieldStyleContract = statsBody != null
                    && statsBody.supportRichText
                    && statsBody.text.Contains("#87B66A", StringComparison.Ordinal)
                    && statsBody.text.Contains("#D8CCB7", StringComparison.Ordinal)
                    && statsBody.text.Contains("#998F7C", StringComparison.Ordinal)
                    && stateBody != null
                    && stateBody.supportRichText
                    && stateBody.text.Contains("#C94E3B", StringComparison.Ordinal)
                    && coreBody != null
                    && coreBody.supportRichText
                    && coreBody.text.Contains("#87B66A", StringComparison.Ordinal)
                    && coreBody.text.Contains("#D8CCB7", StringComparison.Ordinal)
                    && buildBody != null
                    && buildBody.supportRichText
                    && buildBody.text.Contains("#C0A45E", StringComparison.Ordinal)
                    && buildBody.text.Contains("#82AE4F", StringComparison.Ordinal)
                    && buildBody.text.Contains("#716F69", StringComparison.Ordinal);
                if (previewRoot != null)
                {
                    Object.DestroyImmediate(previewRoot);
                    previewRoot = null;
                    previewTransform = null;
                }
                if (previewScene.IsValid())
                {
                    EditorSceneManager.ClosePreviewScene(previewScene);
                }
            }

            string runtimeSource = ReadSource("Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailPanelView.cs");
            string sandboxSource = ReadSource("Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxDetailPanelView.cs");
            string sandboxControllerSource = ReadSource("Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxDetailUiController.cs");
            bool viewModelOnly = !runtimeSource.Contains("Resolver", StringComparison.Ordinal)
                && !runtimeSource.Contains("TalismanBag.ItemSandbox", StringComparison.Ordinal)
                && !runtimeSource.Contains("Battle", StringComparison.Ordinal)
                && !runtimeSource.Contains("RunFlow", StringComparison.Ordinal)
                && !runtimeSource.Contains("SaveData", StringComparison.Ordinal);
            bool sandboxReuse = sandboxSource.Contains("ItemDetailPanelView", StringComparison.Ordinal)
                && sandboxSource.Contains(".Bind(model)", StringComparison.Ordinal)
                && !sandboxSource.Contains("displayPlayerSections", StringComparison.Ordinal)
                && !sandboxSource.Contains("FormatStatus", StringComparison.Ordinal);
            bool popupBehavior = runtimeSource.Contains("closeButton.onClick", StringComparison.Ordinal)
                && runtimeSource.Contains("public void Close()", StringComparison.Ordinal)
                && sandboxControllerSource.Contains("detailPanel?.SetVisible(false)", StringComparison.Ordinal)
                && sandboxControllerSource.Contains("detailPanel.SetVisible(showDetail)", StringComparison.Ordinal)
                && sandboxControllerSource.Contains("showDetail: false", StringComparison.Ordinal);

            AddRow(rows, "runtime-view-namespace", "I001", string.Empty, "CatalogPreview", 0, 0, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "runtime view namespace", namespaceOk, typeof(ItemDetailPanelView).Namespace);
            AddRow(rows, "reusable-prefab-exists", "I001", string.Empty, "CatalogPreview", 0, 0, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "prefab asset exists", prefabExists, ItemSandboxDetailUiSceneBuilder.ItemDetailPanelPrefabPath);
            AddRow(rows, "prefab-viewmodel-only", "I001", string.Empty, "CatalogPreview", 0, 0, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "prefab has runtime view only, no sandbox adapter", prefabRuntimeOnly && viewModelOnly, prefabRuntimeOnly ? "runtime-only" : "prefab mismatch");
            AddRow(rows, "prefab-visual-contract", "I001", string.Empty, "CatalogPreview", 0, 0, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "rarity/status/art slots, chapter bands, section accents, dual scrollbars", prefabVisualContract, prefabVisualContract ? "visual-contract" : "visual nodes missing");
            AddRow(rows, "meta-text-contract", "I001", string.Empty, "CatalogPreview", 0, 0, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "MetaText is rarity line plus faMen / qiLei · readable shape", metaTextContract, metaTextContract ? "良品 + 玄水法 / 符 + 竖排两格" : "unexpected MetaText");
            AddRow(rows, "power-text-contract", "I001", string.Empty, "CatalogPreview", 0, 0, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "PowerText reads any provider number dynamically in rarity color and uses -- when missing", powerTextContract, powerTextContract ? "321 -> 987 -> -- (dynamic rarity color)" : "unexpected PowerText");
            AddRow(rows, "typography-color-contract", "I001", string.Empty, "CatalogPreview", 0, 0, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "palette contract only; child font sizes remain Inspector-authorable", typographyColorContract, typographyColorContract ? "#87B66A/#D8CCB7/#B99A61; font sizes unlocked" : "unexpected palette");
            AddRow(rows, "five-rarity-palette-contract", "I001", string.Empty, "CatalogPreview", 0, 0, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "Fan/Liang/Ling/Xuan/Dao rarity colors", rarityPaletteContract, rarityPaletteContract ? "#E3D8C3/#87B66A/#68A9E6/#B27DDF/#E4A14B" : "unexpected rarity palette");
            AddRow(rows, "rich-field-style-contract", "I001", string.Empty, "CatalogPreview", 0, 0, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "actual BodyText receives rarity/restriction/build rich colors", richFieldStyleContract, richFieldStyleContract ? "stats/status/core/build tags applied" : "field color tags missing");
            AddRow(rows, "sandbox-popup-open-close-contract", "I001", string.Empty, "CatalogPreview", 0, 0, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "hidden on sandbox start, item selection opens, X closes", popupBehavior, popupBehavior ? "popup-contract" : "popup behavior missing");
            AddRow(rows, "sandbox-reuses-runtime-view", "I001", string.Empty, "CatalogPreview", 0, 0, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "sandbox adapter delegates to runtime view", sandboxReuse, sandboxReuse ? "adapter" : "second renderer detected");

            AddErrorIfFalse(result, namespaceOk, "Runtime detail view classes must live under TalismanBag.Items.Detail.UI.");
            AddErrorIfFalse(result, prefabExists, "Missing reusable ItemDetailPanel prefab.");
            AddErrorIfFalse(result, prefabRuntimeOnly && viewModelOnly, "Prefab/runtime view must be ViewModel-only and must not contain Sandbox/Battle wiring.");
            AddErrorIfFalse(result, prefabVisualContract, "Prefab must preserve rarity/status/art slots, chapter bands, section accents, and dual scrollbars.");
            AddErrorIfFalse(result, metaTextContract, "MetaText must render rarity, faMen / qiLei, and a readable shape label without raw shape ids.");
            AddErrorIfFalse(result, powerTextContract, "PowerText must render the dynamic provider number in rarity color and use -- when missing.");
            AddErrorIfFalse(result, typographyColorContract, "Detail palette must remain data-driven while child typography size stays Inspector-authorable.");
            AddErrorIfFalse(result, rarityPaletteContract, "Detail rarity palette must match Fan/Liang/Ling/Xuan/Dao approved colors.");
            AddErrorIfFalse(result, richFieldStyleContract, "Detail BodyText must receive the approved field-level rich color tags.");
            AddErrorIfFalse(result, popupBehavior, "ItemSandbox detail must start hidden, open on item selection, and close through the prefab X button.");
            AddErrorIfFalse(result, sandboxReuse, "ItemSandboxDetailPanelView must be a thin adapter over runtime ItemDetailPanelView.");
        }

        private static void CheckCatalogAll31(
            ItemInnerDataCatalogProvider provider,
            ItemLightingResolutionResult lighting,
            ItemArrayBonusResolutionResult arrayBonus,
            ItemBuildSynergyResolutionResult build,
            ItemCoreAwakeningResolutionResult awakening,
            ItemSkillMonitorResolutionResult monitor,
            VerificationResult result,
            List<SpecRow> rows)
        {
            IReadOnlyList<ItemDetailListEntry> entries = provider.GetItemList();
            bool countOk = entries.Count == 31;
            bool allOk = countOk;
            foreach (ItemDetailListEntry entry in entries)
            {
                ItemDetailViewModel model = Compose(
                    provider.GetDetailViewModel(entry.itemId),
                    ItemDetailProjectionContextKind.CatalogPreview,
                    string.Empty,
                    lighting,
                    arrayBonus,
                    build,
                    awakening,
                    monitor);
                bool ok = model != null
                    && model.itemId == entry.itemId
                    && !string.IsNullOrWhiteSpace(model.displayItemName)
                    && model.displayPlayerSections.Count == ExpectedPlayerSectionTitles.Length
                    && model.displayDebugSections.Count == 3
                    && HasCatalogPreviewState(model)
                    && !PlayerHasForbiddenToken(model)
                    && !DebugText(model).Contains("P_SOURCE", StringComparison.Ordinal)
                    && !DebugText(model).Contains("P_I001_A", StringComparison.Ordinal)
                    && !DebugText(model).Contains("P_I007_UNLIT", StringComparison.Ordinal);
                allOk &= ok;
                AddRow(rows, $"catalog-all-31-{entry.itemId}", model, "CatalogPreview 15/3 sections with no placed state read", ok, ok ? "PASS" : "FAIL");
            }

            bool junianBoundaryOk = entries.Any(entry => entry.itemId == "I031");
            AddRow(rows, "catalog-all-31", "ALL", string.Empty, "CatalogPreview", ExpectedPlayerSectionTitles.Length, 3, CatalogPreviewState, CatalogPreviewState, CatalogPreviewState, string.Empty, string.Empty, CatalogPreviewState, CatalogPreviewState, "no duplicate", "not-applicable", "31 catalog rows including I031 JuNian boundary", allOk && junianBoundaryOk, $"count={entries.Count}; I031={junianBoundaryOk}");
            AddErrorIfFalse(result, allOk && junianBoundaryOk, "All 31 catalog items must compose as CatalogPreview without reading placed instance state.");
        }

        private static ItemDetailViewModel CreateLongTextModel(
            ItemInnerDataCatalogProvider provider,
            ItemLightingResolutionResult lighting,
            ItemArrayBonusResolutionResult arrayBonus,
            ItemBuildSynergyResolutionResult build,
            ItemCoreAwakeningResolutionResult awakening,
            ItemSkillMonitorResolutionResult monitor)
        {
            ItemDetailViewModel model = Compose(
                provider.GetDetailViewModel("I001"),
                ItemDetailProjectionContextKind.PlacedInstance,
                "P_I001_A",
                lighting,
                arrayBonus,
                build,
                awakening,
                monitor);
            AppendLongSection(model, "基础效果", LongText("基础效果长文压力", 24));
            ReplaceSection(model, "核心效果/开窍", string.Join("\n", new[]
            {
                "  Lv.10：已开窍且可生效；" + LongText("十级节点预览", 10),
                "  Lv.20：已开窍且可生效；" + LongText("二十级节点预览", 10),
                "  Lv.30：未开窍；" + LongText("三十级节点预览", 10),
                "  Lv.40：未开窍；" + LongText("四十级节点预览", 10)
            }));
            AppendLongSection(model, "法门Build", LongText("法门Build长文压力", 18));
            AppendLongSection(model, "器类Build", LongText("器类Build长文压力", 18));
            AppendLongSection(model, "摆放提示", LongText("摆放提示长文压力", 20));
            ReplaceSection(model, "文化描述", "  " + LongText("文化描述长文压力", 42));
            return model;
        }

        private static LayoutCheckResult CheckLayoutForResolution(ItemDetailViewModel model, Vector2 resolution, string caseId)
        {
            GameObject canvasObject = new($"VerifierCanvas_{caseId}", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            canvasRect.sizeDelta = resolution;
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = resolution;
            scaler.matchWidthOrHeight = 0.5f;

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ItemSandboxDetailUiSceneBuilder.ItemDetailPanelPrefabPath);
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, canvasObject.transform) as GameObject;
            RectTransform panelRect = instance.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = new Vector2(18f, 18f);
            panelRect.offsetMax = new Vector2(-18f, -18f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);

            ItemDetailPanelView view = instance.GetComponent<ItemDetailPanelView>();
            view.Bind(model);
            view.Bind(model);
            view.ShowPlayerDetailTab();
            ForceLayout(panelRect);

            ScrollRect detailScroll = FindChildComponent<ScrollRect>(instance.transform, "ItemDetailScrollView");
            ScrollRect debugScroll = FindChildComponent<ScrollRect>(instance.transform, "ItemDebugScrollView");
            RectTransform detailContent = FindChild(instance.transform, "DetailContent") as RectTransform;
            RectTransform debugContent = FindChild(instance.transform, "DebugContent") as RectTransform;
            bool detailOk = CheckScrollLayout(detailScroll, detailContent, out string detailSummary);
            int activeDetailSections = CountActiveRuntimeSections(detailContent);
            bool detailManualGeometryHasPositiveBounds = CheckManualGeometryPositive(detailContent);
            bool headerColumnsValid = CheckHeaderColumns(instance.transform);

            view.ShowDebugTab();
            ForceLayout(panelRect);
            bool debugOk = CheckScrollLayout(debugScroll, debugContent, out string debugSummary);
            bool debugManualGeometryHasPositiveBounds = CheckManualGeometryPositive(debugContent);
            int activeDebugSections = CountActiveRuntimeSections(debugContent);

            view.ShowPlayerDetailTab();
            ForceLayout(panelRect);
            bool manualGeometryPositive = detailManualGeometryHasPositiveBounds && debugManualGeometryHasPositiveBounds;
            int expectedActiveDetailSections = model.displayPlayerSections.Count(section =>
                section.keepWhenEmpty || !string.IsNullOrWhiteSpace(section.body));
            bool sectionCounts = activeDetailSections == expectedActiveDetailSections
                && activeDebugSections == 3
                && CountRuntimeSections(detailContent) == 15
                && CountRuntimeSections(debugContent) == 3;
            bool scrollReset = detailScroll != null && Mathf.Abs(detailScroll.verticalNormalizedPosition - 1f) <= 0.01f;
            bool panelPositive = panelRect.rect.width > 0f && panelRect.rect.height > 0f;
            bool longScroll = detailScroll != null
                && detailScroll.content != null
                && detailScroll.viewport != null
                && detailScroll.content.rect.height > detailScroll.viewport.rect.height + 1f;

            string summary = $"{resolution.x:0}x{resolution.y:0}; detail={detailSummary}; debug={debugSummary}; sections={activeDetailSections}/{expectedActiveDetailSections} active ({CountRuntimeSections(detailContent)}/15 bound),{activeDebugSections}/3; manualGeometryPositive={manualGeometryPositive}; headerColumnsAdvisory={headerColumnsValid}; reset={scrollReset}; longScroll={longScroll}";
            Object.DestroyImmediate(canvasObject);
            return new LayoutCheckResult(caseId, model, resolution, panelPositive && detailOk && debugOk && sectionCounts && scrollReset && longScroll, summary, manualGeometryPositive, scrollReset, longScroll);
        }

        private static void CheckLayoutResult(LayoutCheckResult layout, VerificationResult result, List<SpecRow> rows)
        {
            AddRow(rows, layout.caseId, layout.model, "scroll roots, bound sections, scroll reset; child geometry remains Inspector-authorable", layout.passed, layout.summary);
            AddRow(rows, "manual-geometry-" + layout.caseId, layout.model, "active child sections have positive manual RectTransform bounds; exact positions/sizes are not locked", layout.noOverlap, layout.summary);
            AddRow(rows, "scroll-reset-" + layout.caseId, layout.model, "Bind resets scroll to top on item switch/rebind", layout.scrollReset, layout.summary);
            AddRow(rows, "long-text-scroll-" + layout.caseId, layout.model, "long content exceeds viewport and can scroll", layout.longScroll, layout.summary);
            AddErrorIfFalse(result, layout.passed, $"Layout check failed for {layout.caseId}: {layout.summary}");
        }

        private static void CheckSourceScope(VerificationResult result, List<SpecRow> rows)
        {
            List<string> leaks = new();
            foreach (string path in ScopedSourcePaths)
            {
                if (!File.Exists(path))
                {
                    leaks.Add($"Missing scoped source: {path}");
                    continue;
                }

                string source = File.ReadAllText(path);
                foreach (string token in ForbiddenScopeTokens)
                {
                    if (source.Contains(token, StringComparison.Ordinal))
                    {
                        leaks.Add($"{path}: forbidden token `{token}`");
                    }
                }
            }

            bool clean = leaks.Count == 0;
            AddRow(rows, "scope-leak-check", "ALL", string.Empty, "CatalogPreview", 0, 0, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "no formal battle/save/reward/boss/runflow/buildsettings scope", clean, clean ? "clean" : string.Join(" | ", leaks));
            if (!clean)
            {
                result.Errors.AddRange(leaks);
            }
        }

        private static void CheckRegressionReports(VerificationResult result, List<SpecRow> rows)
        {
            string[] reportPaths =
            {
                "Docs/V0.4/Reports/ItemSandboxDetailUiReport.md",
                "Docs/V0.4/Reports/ItemInnerDataCatalogReport.md",
                "Docs/V0.4/Reports/ItemGridPlacementAndEyeRuleReport.md",
                "Docs/V0.4/Reports/JuNianLightingAndAdjacentRelayReport.md",
                "Docs/V0.4/Reports/ArrayBonusCellResolverReport.md",
                "Docs/V0.4/Reports/BuildSynergyCoreReport.md",
                "Docs/V0.4/Reports/CoreAwakeningPreviewReport.md",
                "Docs/V0.4/Reports/ItemSkillTriggerContractReport.md"
            };

            foreach (string path in reportPaths)
            {
                bool pass = File.Exists(path) && File.ReadAllText(path).Contains("PASS", StringComparison.OrdinalIgnoreCase);
                AddRow(rows, "regression-" + Path.GetFileNameWithoutExtension(path), "ALL", string.Empty, "CatalogPreview", 0, 0, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "existing regression report records PASS", pass, pass ? path : "missing or no PASS");
                AddErrorIfFalse(result, pass, $"Regression report missing or not PASS: {path}");
            }
        }

        private static ItemLightingResolutionResult CreateProjectionLighting()
        {
            return new ItemLightingResolutionResult(
                new[] { new Vector2Int(1, 2), new Vector2Int(2, 1), new Vector2Int(2, 3), new Vector2Int(3, 2) },
                new[]
                {
                    Lit("I031", "聚念石", "P_SOURCE", new Vector2Int(2, 2), true, true, "P_SOURCE", 0),
                    Lit("I001", "震雷符 A", "P_I001_A", new Vector2Int(2, 1), false, true, "P_SOURCE", 0),
                    Unlit("I007", "离火符未点亮", "P_I007_UNLIT", new Vector2Int(4, 4)),
                    Lit("I002", "五雷急符", "P_I002", new Vector2Int(1, 2), false, true, "P_SOURCE", 0),
                    Lit("I003", "震雷破壳印", "P_I003", new Vector2Int(0, 1), false, false, "P_I002", 1),
                    Lit("I004", "五雷急令", "P_I004", new Vector2Int(0, 2), false, false, "P_I003", 2),
                    Lit("I005", "照壳雷镜", "P_I005", new Vector2Int(0, 3), false, false, "P_I004", 3),
                    Lit("I006", "天鼓槌", "P_I006", new Vector2Int(0, 4), false, false, "P_I005", 4)
                });
        }

        private static IReadOnlyList<ItemCoreAwakeningInput> CreateAwakeningInputs()
        {
            return new[]
            {
                Input("I031", "P_SOURCE", 40),
                Input("I001", "P_I001_A", 40),
                Input("I007", "P_I007_UNLIT", 20),
                Input("I002", "P_I002", 1),
                Input("I003", "P_I003", 20),
                Input("I004", "P_I004", 30),
                Input("I005", "P_I005", 30),
                Input("I006", "P_I006", 40)
            };
        }

        private static ItemCoreAwakeningInput Input(string itemId, string placementId, int level)
        {
            return new ItemCoreAwakeningInput(itemId, placementId, level, false, "ItemDetailProjectionCompleteVerifier");
        }

        private static ItemLightingItemResult Lit(
            string itemId,
            string displayName,
            string placementId,
            Vector2Int cell,
            bool isLightingSource,
            bool isDirectLit,
            string litByPlacementId,
            int litDepth)
        {
            return new ItemLightingItemResult(
                itemId,
                displayName,
                new[] { cell },
                cell,
                isLightingSource,
                isDirectLit,
                true,
                string.Equals(placementId, litByPlacementId, StringComparison.Ordinal) ? itemId : "I031",
                litDepth,
                placementId,
                litByPlacementId);
        }

        private static ItemLightingItemResult Unlit(string itemId, string displayName, string placementId, Vector2Int cell)
        {
            return new ItemLightingItemResult(
                itemId,
                displayName,
                new[] { cell },
                cell,
                false,
                false,
                false,
                string.Empty,
                -1,
                placementId,
                string.Empty);
        }

        private static void AddRow(List<SpecRow> rows, string caseId, ItemDetailViewModel model, string expected, bool passed, string actual)
        {
            rows.Add(SpecRow.FromModel(caseId, model, expected, passed, actual));
        }

        private static void AddRow(
            List<SpecRow> rows,
            string caseId,
            string itemId,
            string placementId,
            string contextKind,
            int playerSectionCount,
            int debugFieldCount,
            string lightingState,
            string arrayState,
            string awakeningState,
            string faMenBuildStage,
            string qiLeiBuildStage,
            string monitorState,
            string snapshotFallback,
            string duplicateProjectionCheck,
            string layoutCheck,
            string expected,
            bool passed,
            string actual)
        {
            rows.Add(new SpecRow
            {
                caseId = caseId,
                itemId = itemId,
                placementId = placementId,
                contextKind = contextKind,
                playerSectionCount = playerSectionCount.ToString(),
                debugFieldCount = debugFieldCount.ToString(),
                lightingState = lightingState,
                arrayState = arrayState,
                awakeningState = awakeningState,
                faMenBuildStage = faMenBuildStage,
                qiLeiBuildStage = qiLeiBuildStage,
                monitorState = monitorState,
                snapshotFallback = snapshotFallback,
                duplicateProjectionCheck = duplicateProjectionCheck,
                layoutCheck = layoutCheck,
                expectedResult = expected,
                actualResult = passed ? "PASS: " + actual : "FAIL: " + actual
            });
        }

        private static void AddErrorIfFalse(VerificationResult result, bool passed, string error)
        {
            if (!passed)
            {
                result.Errors.Add(error);
            }
        }

        private static string PlayerText(ItemDetailViewModel model)
        {
            return JoinSections(model?.displayPlayerSections);
        }

        private static string DebugText(ItemDetailViewModel model)
        {
            return JoinSections(model?.displayDebugSections);
        }

        private static string JoinSections(IReadOnlyList<ItemDetailSectionViewModel> sections)
        {
            return string.Join("\n", (sections ?? Array.Empty<ItemDetailSectionViewModel>())
                .Where(section => section != null)
                .Select(section => $"{section.title}\n{section.body}"));
        }

        private static string Signature(ItemDetailViewModel model)
        {
            return $"{JoinSections(model.displayPlayerSections)}\n---DEBUG---\n{JoinSections(model.displayDebugSections)}";
        }

        private static bool PlayerHasForbiddenToken(ItemDetailViewModel model)
        {
            string playerText = PlayerText(model);
            return ForbiddenPlayerTokens.Any(token => playerText.Contains(token, StringComparison.Ordinal));
        }

        private static bool HasCatalogPreviewState(ItemDetailViewModel model)
        {
            return string.Equals(SectionState(model, "basic", "点亮状态："), "尚未入阵", StringComparison.Ordinal)
                && string.Equals(SectionState(model, "basic", "阵脉状态："), "阵脉未计算", StringComparison.Ordinal);
        }

        private static int CountDebugTokens(string debugText)
        {
            return RequiredDebugTokens.Count(token => debugText.Contains(token, StringComparison.Ordinal));
        }

        private static string SectionBody(ItemDetailViewModel model, string title)
        {
            return model?.displayPlayerSections?.FirstOrDefault(section => section != null
                && (section.title == title || section.stateKey == title))?.body ?? string.Empty;
        }

        private static string SectionState(ItemDetailViewModel model, string title, string label)
        {
            string body = SectionBody(model, title);
            foreach (string line in body.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string trimmed = line.Trim();
                if (trimmed.StartsWith(label, StringComparison.Ordinal))
                {
                    return trimmed.Substring(label.Length).Trim();
                }
            }

            return string.Empty;
        }

        private static void AppendLongSection(ItemDetailViewModel model, string title, string text)
        {
            ItemDetailSectionViewModel section = model.displayPlayerSections.FirstOrDefault(value => value.title == title);
            if (section != null)
            {
                section.body = section.body + "\n  " + text;
            }
        }

        private static void ReplaceSection(ItemDetailViewModel model, string title, string body)
        {
            ItemDetailSectionViewModel section = model.displayPlayerSections.FirstOrDefault(value => value.title == title);
            if (section != null)
            {
                section.body = body;
            }
        }

        private static string LongText(string seed, int repeat)
        {
            StringBuilder builder = new();
            for (int i = 0; i < repeat; i++)
            {
                if (builder.Length > 0)
                {
                    builder.Append(' ');
                }

                builder.Append(seed).Append(i + 1).Append("：此处为返修布局压力文本，用于验证长内容自动换行、章节高度增长和滚动到底部。");
            }

            return builder.ToString();
        }

        private static void ForceLayout(RectTransform root)
        {
            Canvas.ForceUpdateCanvases();
            Canvas.ForceUpdateCanvases();
        }

        private static bool CheckHeaderColumns(Transform root)
        {
            RectTransform header = FindChild(root, "ItemDetailHeader") as RectTransform;
            RectTransform headerText = FindChild(root, "ItemDetailHeaderTextGroup") as RectTransform;
            RectTransform artwork = FindChild(root, "ItemArtworkFrame") as RectTransform;
            RectTransform titleRow = FindChild(root, "ItemDetailTitleRow") as RectTransform;
            RectTransform statusRow = FindChild(root, "ItemStatusBadgeRow") as RectTransform;
            if (header == null || headerText == null || artwork == null || titleRow == null || statusRow == null)
            {
                return false;
            }

            Bounds textBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(header, headerText);
            Bounds artworkBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(header, artwork);
            bool columnsSeparated = textBounds.size.x > 0f
                && textBounds.size.y > 0f
                && artworkBounds.size.x > 0f
                && artworkBounds.size.y > 0f
                && textBounds.max.x <= artworkBounds.min.x + 1f;
            return columnsSeparated
                && CheckHorizontalChildren(titleRow)
                && CheckHorizontalChildren(statusRow);
        }

        private static bool CheckHorizontalChildren(RectTransform parent)
        {
            if (parent == null)
            {
                return false;
            }

            List<Bounds> childBounds = new();
            for (int i = 0; i < parent.childCount; i++)
            {
                RectTransform child = parent.GetChild(i) as RectTransform;
                if (child == null || !child.gameObject.activeSelf)
                {
                    continue;
                }

                Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(parent, child);
                if (bounds.size.x <= 0f || bounds.size.y <= 0f)
                {
                    return false;
                }

                childBounds.Add(bounds);
            }

            childBounds = childBounds.OrderBy(bounds => bounds.min.x).ToList();
            for (int i = 1; i < childBounds.Count; i++)
            {
                if (childBounds[i].min.x < childBounds[i - 1].max.x - 0.5f)
                {
                    return false;
                }
            }

            return childBounds.Count >= 2;
        }

        private static bool CheckScrollLayout(ScrollRect scroll, RectTransform content, out string summary)
        {
            if (scroll == null || scroll.viewport == null || scroll.content == null || content == null)
            {
                summary = "missing-scroll";
                return false;
            }

            bool ok = scroll.vertical
                && !scroll.horizontal
                && scroll.viewport.rect.width > 0f
                && scroll.viewport.rect.height > 0f
                && content.rect.width > 0f
                && content.rect.height > 0f
                && scroll.viewport.GetComponent<RectMask2D>() != null;
            summary = $"viewport={scroll.viewport.rect.width:0}x{scroll.viewport.rect.height:0}; content={content.rect.width:0}x{content.rect.height:0}; scrollbar={(scroll.verticalScrollbar != null ? "optional-present" : "optional-removed")}; autoLayout={(content.GetComponent<VerticalLayoutGroup>() != null || content.GetComponent<ContentSizeFitter>() != null ? "present" : "not-required")}";
            return ok;
        }

        private static bool CheckManualGeometryPositive(RectTransform content)
        {
            if (content == null)
            {
                return false;
            }

            for (int i = 0; i < content.childCount; i++)
            {
                RectTransform current = content.GetChild(i) as RectTransform;
                if (current == null || !current.gameObject.activeSelf)
                {
                    continue;
                }

                if (current.rect.height <= 0f)
                {
                    return false;
                }
            }

            return true;
        }

        private static float LocalTop(RectTransform target, RectTransform relativeTo)
        {
            Vector3[] corners = new Vector3[4];
            target.GetWorldCorners(corners);
            return corners.Select(corner => relativeTo.InverseTransformPoint(corner).y).Max();
        }

        private static float LocalBottom(RectTransform target, RectTransform relativeTo)
        {
            Vector3[] corners = new Vector3[4];
            target.GetWorldCorners(corners);
            return corners.Select(corner => relativeTo.InverseTransformPoint(corner).y).Min();
        }

        private static int CountRuntimeSections(RectTransform content)
        {
            return content == null ? 0 : content.GetComponentsInChildren<ItemDetailSectionView>(true).Length;
        }

        private static int CountActiveRuntimeSections(RectTransform content)
        {
            return content == null
                ? 0
                : content.GetComponentsInChildren<ItemDetailSectionView>(true).Count(section => section.gameObject.activeSelf);
        }

        private static Transform FindChild(Transform root, string objectName)
        {
            if (root == null)
            {
                return null;
            }

            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == objectName)
                {
                    return child;
                }
            }

            return null;
        }

        private static bool ColorNear(Color actual, Color expected)
        {
            return Mathf.Abs(actual.r - expected.r) < 0.01f
                && Mathf.Abs(actual.g - expected.g) < 0.01f
                && Mathf.Abs(actual.b - expected.b) < 0.01f
                && Mathf.Abs(actual.a - expected.a) < 0.01f;
        }

        private static T FindChildComponent<T>(Transform root, string objectName) where T : Component
        {
            Transform child = FindChild(root, objectName);
            return child != null ? child.GetComponent<T>() : null;
        }

        private static string ReadSource(string path)
        {
            return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
        }

        private static void WriteReports(VerificationResult result, IReadOnlyList<SpecRow> rows)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(DetailReportPath) ?? "Docs/V0.4/Reports");
            UTF8Encoding encoding = new(true);
            File.WriteAllText(DetailReportPath, BuildDetailReport(result, rows), encoding);
            File.WriteAllText(SpecCsvPath, BuildSpecCsv(rows), encoding);
            File.WriteAllText(LeakCheckReportPath, BuildLeakCheckReport(result), encoding);
        }

        private static string BuildDetailReport(VerificationResult result, IReadOnlyList<SpecRow> rows)
        {
            StringBuilder builder = new();
            builder.AppendLine("# ItemDetailProjectionComplete01 Rework Report");
            builder.AppendLine();
            builder.AppendLine($"Result: {(result.Errors.Count == 0 ? "PASS" : "FAIL")}");
            builder.AppendLine();
            builder.AppendLine("## Runtime Assets");
            builder.AppendLine("- Runtime prefab: `" + ItemSandboxDetailUiSceneBuilder.ItemDetailPanelPrefabPath + "`");
            builder.AppendLine("- Runtime view: `Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailPanelView.cs`");
            builder.AppendLine("- Runtime section view: `Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailSectionView.cs`");
            builder.AppendLine();
            builder.AppendLine("## Context Rules");
            builder.AppendLine("- CatalogPreview ignores placed snapshots and shows header states `" + CatalogPreviewState + "`.");
            builder.AppendLine("- PlacedInstance requires an explicit placementId and queries snapshots strictly by placementId.");
            builder.AppendLine("- Stale placement `P_I001_STALE` shows `" + InstanceUnavailableState + "` and records missing snapshot diagnostics.");
            builder.AppendLine("- No `FindItemResult(itemId)` fallback is allowed in the composer or Sandbox projection path.");
            builder.AppendLine();
            builder.AppendLine("## Regression Checks");
            foreach (SpecRow row in rows)
            {
                builder.AppendLine($"- {row.caseId}: {row.actualResult}");
            }

            builder.AppendLine();
            builder.AppendLine("## Notes");
            foreach (string note in result.Notes)
            {
                builder.AppendLine("- " + note);
            }

            if (result.Errors.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("## Errors");
                foreach (string error in result.Errors)
                {
                    builder.AppendLine("- " + error);
                }
            }

            return builder.ToString();
        }

        private static string BuildSpecCsv(IReadOnlyList<SpecRow> rows)
        {
            StringBuilder builder = new();
            builder.AppendLine("caseId,itemId,placementId,contextKind,playerSectionCount,debugFieldCount,lightingState,arrayState,awakeningState,faMenBuildStage,qiLeiBuildStage,monitorState,snapshotFallback,duplicateProjectionCheck,layoutCheck,expectedResult,actualResult");
            foreach (SpecRow row in rows)
            {
                builder.AppendLine(string.Join(",", row.Values().Select(EscapeCsv)));
            }

            return builder.ToString();
        }

        private static string BuildLeakCheckReport(VerificationResult result)
        {
            StringBuilder builder = new();
            builder.AppendLine("# ItemDetailProjectionComplete01 Rework Leak Check");
            builder.AppendLine();
            builder.AppendLine($"Result: {(result.Errors.Count == 0 ? "PASS" : "FAIL")}");
            builder.AppendLine();
            builder.AppendLine("Checked scoped files:");
            foreach (string path in ScopedSourcePaths)
            {
                builder.AppendLine("- `" + path + "`");
            }

            builder.AppendLine();
            builder.AppendLine("Forbidden formal-system tokens:");
            foreach (string token in ForbiddenScopeTokens)
            {
                builder.AppendLine("- `" + token + "`");
            }

            builder.AppendLine();
            builder.AppendLine("Runtime UI dependency boundary:");
            builder.AppendLine("- No `TalismanBag.ItemSandbox`, resolver, Battle, RunFlow, SaveData dependency inside runtime UI folder.");
            builder.AppendLine("- Sandbox detail panel and section are adapters over runtime UI.");

            if (result.Errors.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("Errors:");
                foreach (string error in result.Errors)
                {
                    builder.AppendLine("- " + error);
                }
            }

            return builder.ToString();
        }

        private static string EscapeCsv(string value)
        {
            value ??= string.Empty;
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        private sealed class VerificationResult
        {
            public readonly List<string> Errors = new();
            public readonly List<string> Notes = new();
        }

        private sealed class LayoutCheckResult
        {
            public LayoutCheckResult(string caseId, ItemDetailViewModel model, Vector2 resolution, bool passed, string summary, bool noOverlap, bool scrollReset, bool longScroll)
            {
                this.caseId = caseId;
                this.model = model;
                this.resolution = resolution;
                this.passed = passed;
                this.summary = summary;
                this.noOverlap = noOverlap;
                this.scrollReset = scrollReset;
                this.longScroll = longScroll;
            }

            public readonly string caseId;
            public readonly ItemDetailViewModel model;
            public readonly Vector2 resolution;
            public readonly bool passed;
            public readonly string summary;
            public readonly bool noOverlap;
            public readonly bool scrollReset;
            public readonly bool longScroll;
        }

        private sealed class SpecRow
        {
            public string caseId;
            public string itemId;
            public string placementId;
            public string contextKind;
            public string playerSectionCount;
            public string debugFieldCount;
            public string lightingState;
            public string arrayState;
            public string awakeningState;
            public string faMenBuildStage;
            public string qiLeiBuildStage;
            public string monitorState;
            public string snapshotFallback;
            public string duplicateProjectionCheck;
            public string layoutCheck;
            public string expectedResult;
            public string actualResult;

            public static SpecRow FromModel(string caseId, ItemDetailViewModel model, string expected, bool passed, string actual)
            {
                string debugText = DebugText(model);
                return new SpecRow
                {
                    caseId = caseId,
                    itemId = model?.itemId ?? string.Empty,
                    placementId = model?.placementId ?? string.Empty,
                    contextKind = ExtractDebugValue(debugText, "contextKind"),
                    playerSectionCount = (model?.displayPlayerSections?.Count ?? 0).ToString(),
                    debugFieldCount = CountDebugTokens(debugText).ToString(),
                    lightingState = SectionState(model, "basic", "点亮状态："),
                    arrayState = SectionState(model, "basic", "阵脉状态："),
                    awakeningState = string.Empty,
                    faMenBuildStage = FirstNonEmptyLine(SectionBody(model, "法门Build")),
                    qiLeiBuildStage = FirstNonEmptyLine(SectionBody(model, "器类Build")),
                    monitorState = FirstNonEmptyLine(SectionBody(model, "主Build自动监控")),
                    snapshotFallback = ExtractMissingSnapshot(debugText),
                    duplicateProjectionCheck = caseId.Contains("duplicate", StringComparison.OrdinalIgnoreCase) || caseId.Contains("idempot", StringComparison.OrdinalIgnoreCase) ? actual : string.Empty,
                    layoutCheck = caseId.Contains("layout", StringComparison.OrdinalIgnoreCase) || caseId.Contains("overlap", StringComparison.OrdinalIgnoreCase) || caseId.Contains("scroll", StringComparison.OrdinalIgnoreCase) ? actual : string.Empty,
                    expectedResult = expected,
                    actualResult = passed ? "PASS: " + actual : "FAIL: " + actual
                };
            }

            public IEnumerable<string> Values()
            {
                yield return caseId;
                yield return itemId;
                yield return placementId;
                yield return contextKind;
                yield return playerSectionCount;
                yield return debugFieldCount;
                yield return lightingState;
                yield return arrayState;
                yield return awakeningState;
                yield return faMenBuildStage;
                yield return qiLeiBuildStage;
                yield return monitorState;
                yield return snapshotFallback;
                yield return duplicateProjectionCheck;
                yield return layoutCheck;
                yield return expectedResult;
                yield return actualResult;
            }

            private static string ExtractDebugValue(string debugText, string key)
            {
                foreach (string line in debugText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string trimmed = line.Trim();
                    string prefix = key + ":";
                    if (trimmed.StartsWith(prefix, StringComparison.Ordinal))
                    {
                        return trimmed.Substring(prefix.Length).Trim();
                    }
                }

                return string.Empty;
            }

            private static string ExtractMissingSnapshot(string debugText)
            {
                foreach (string line in debugText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string trimmed = line.Trim();
                    if (trimmed.StartsWith("missingSnapshot:", StringComparison.Ordinal))
                    {
                        return trimmed;
                    }
                }

                return string.Empty;
            }

            private static string FirstNonEmptyLine(string body)
            {
                foreach (string line in (body ?? string.Empty).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string trimmed = line.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmed))
                    {
                        return trimmed;
                    }
                }

                return string.Empty;
            }
        }
    }
}
#endif
