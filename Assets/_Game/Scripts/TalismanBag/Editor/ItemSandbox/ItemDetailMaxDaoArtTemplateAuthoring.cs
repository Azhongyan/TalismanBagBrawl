using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.Items.Detail;
using TalismanBag.Items.Detail.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.EditorTools.ItemSandbox
{
    /// <summary>
    /// Explicit, one-shot scene authoring for ItemDetailMaxDaoArtTemplate01.
    /// This class intentionally has no automatic editor lifecycle callbacks.
    /// Existing slots are content-bound only; initial RectTransform values are written only when a slot is created.
    /// </summary>
    internal static class ItemDetailMaxDaoArtTemplateAuthoring
    {
        private const string ScenePath = "Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity";
        private const string PanelPath =
            "ItemSandboxCanvas/MobileSafeAreaRoot/ItemSandboxRoot/ItemDetailPanel";
        private const string PreviewId = "EDITOR_PREVIEW_MAX_DAO_I001";
        private const string PreviewLabels =
            "EDITOR_PREVIEW_ONLY\nNOT_ROLLED_INSTANCE\nNOT_LIVE_BALANCE_DATA";
        private const ulong ZhenFaIconLocalId = 957452681;
        private const ulong QiXingIconLocalId = 725680823;
        private const ulong DaoJuLocalId = 743156000;

        private static readonly Dictionary<string, string> AssetPaths = new(StringComparer.Ordinal)
        {
            ["background"] = "Assets/_Game/Resources/item/弹窗背景/background-道品.png",
            ["rarity"] = "Assets/_Game/Resources/item/品阶icon/品阶icon_道品.png",
            ["zhenfa"] = "Assets/_Game/Resources/item/阵法icon/震雷法.png",
            ["qilei"] = "Assets/_Game/Resources/item/器类icon/符类.png",
            ["item_art_i001_orange"] = "Assets/_Game/Resources/item_daoju/震雷法/I001/I001_5.png",
            ["qilei_skill1"] = "Assets/_Game/Resources/item/器类build技能icon/符类/符技能icon_1.png",
            ["qilei_skill2"] = "Assets/_Game/Resources/item/器类build技能icon/符类/符技能icon_2.png",
            ["stat_damage"] = "Assets/_Game/Resources/item/基础属性icon/伤害.png",
            ["stat_attack"] = "Assets/_Game/Resources/item/基础属性icon/攻击.png",
            ["stat_control"] = "Assets/_Game/Resources/item/基础属性icon/控制.png",
            ["stat_break"] = "Assets/_Game/Resources/item/基础属性icon/破盾.png",
            ["stat_cooldown"] = "Assets/_Game/Resources/item/基础属性icon/冷却.png",
            ["stat_cost"] = "Assets/_Game/Resources/item/基础属性icon/耗念.png",
            ["stat_guard"] = "Assets/_Game/Resources/item/基础属性icon/护势.png",
            ["placement"] = "Assets/_Game/Resources/item/基础属性icon/摆放推荐.png",
            ["flavor"] = "Assets/_Game/Resources/item/基础属性icon/旧物记.png",
            ["fixed"] = "Assets/_Game/Resources/item/词条icon/固定词条icon_道.png",
            ["random"] = "Assets/_Game/Resources/item/词条icon/随机词条icon_道.png",
            ["core1"] = "Assets/_Game/Resources/item/核心icon/震雷法/I001/I001_1.png",
            ["core2"] = "Assets/_Game/Resources/item/核心icon/震雷法/I001/I001_2.png",
            ["core3"] = "Assets/_Game/Resources/item/核心icon/震雷法/I001/I001_3.png",
            ["core4"] = "Assets/_Game/Resources/item/核心icon/震雷法/I001/I001_4.png",
            ["skill1"] = "Assets/_Game/Resources/item/build技能icon/震雷法/技能icon_1.png",
            ["skill2"] = "Assets/_Game/Resources/item/build技能icon/震雷法/技能icon_2.png",
            ["skill3"] = "Assets/_Game/Resources/item/build技能icon/震雷法/技能icon_3.png",
            ["skill4"] = "Assets/_Game/Resources/item/build技能icon/震雷法/技能icon_4.png",
            ["focus"] = "Assets/_Game/Resources/item/聚念石icon/聚念石icon_点亮.png",
            ["array"] = "Assets/_Game/Resources/item/阵脉icon/阵脉icon_点亮.png",
            ["thin"] = "Assets/_Game/Resources/item/分割线/分割线_细.png",
            ["coarse"] = "Assets/_Game/Resources/item/分割线/分割线_粗.png"
        };

        [MenuItem("TalismanBag/Item Sandbox/Author Item Detail Max Dao Art Template (Manual Only)")]
        public static void ApplyManualOnly()
        {
            Dictionary<string, Sprite> sprites = ImportAndLoadSprites();
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Transform panel = FindScenePath(scene, PanelPath)
                ?? throw new InvalidOperationException("ItemDetailPanel scene root not found: " + PanelPath);
            if (PrefabUtility.IsPartOfPrefabInstance(panel.gameObject))
            {
                throw new InvalidOperationException("ItemDetailPanel must remain a native Scene subtree.");
            }

            Transform artworkFrame = Require(panel, "ItemDetailHeader/ItemArtworkFrame");
            Transform zhenfa = Require(artworkFrame, "zhenfaicon");
            Transform qixing = Require(artworkFrame, "qixingicon");
            Transform daoju = Require(artworkFrame, "daoju");
            AssertIdentity(zhenfa.gameObject, ZhenFaIconLocalId, "zhenfaicon");
            AssertIdentity(qixing.gameObject, QiXingIconLocalId, "qixingicon");
            AssertIdentity(daoju.gameObject, DaoJuLocalId, "daoju");
            TransformSnapshot zhenfaBefore = TransformSnapshot.Capture(zhenfa as RectTransform);
            TransformSnapshot qixingBefore = TransformSnapshot.Capture(qixing as RectTransform);
            TransformSnapshot daojuBefore = TransformSnapshot.Capture(daoju as RectTransform);

            bool firstRun = artworkFrame.Find("PreviewComplianceText") == null;
            EnsureHeaderSlots(artworkFrame);
            EnsureSectionSlots(panel);

            ItemDetailPanelView view = panel.GetComponent<ItemDetailPanelView>()
                ?? throw new InvalidOperationException("ItemDetailPanelView missing.");
            panel.gameObject.SetActive(true);
            view.Bind(BuildPreviewModel(), null);
            view.ShowPlayerDetailTab();

            BindHeaderArtwork(view, artworkFrame, zhenfa, qixing, daoju, sprites, firstRun);
            BindSectionArtwork(panel, sprites);

            zhenfaBefore.AssertUnchanged(zhenfa as RectTransform, "zhenfaicon");
            qixingBefore.AssertUnchanged(qixing as RectTransform, "qixingicon");
            daojuBefore.AssertUnchanged(daoju as RectTransform, "daoju");
            AssertIdentity(zhenfa.gameObject, ZhenFaIconLocalId, "zhenfaicon");
            AssertIdentity(qixing.gameObject, QiXingIconLocalId, "qixingicon");
            AssertIdentity(daoju.gameObject, DaoJuLocalId, "daoju");

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
            {
                throw new InvalidOperationException("Failed to save ItemSandbox scene.");
            }

            Debug.Log("ITEM_DETAIL_MAX_DAO_ART_TEMPLATE_AUTHORING_PASS: " + PreviewId);
        }

        [MenuItem("TalismanBag/Item Sandbox/Unlock All Item Detail RectTransforms (Manual Only)")]
        public static void UnlockAllManualRectTransformsOnly()
        {
            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
            Transform panel = FindScenePath(scene, PanelPath)
                ?? throw new InvalidOperationException("ItemDetailPanel scene root not found: " + PanelPath);
            if (PrefabUtility.IsPartOfPrefabInstance(panel.gameObject))
            {
                throw new InvalidOperationException("ItemDetailPanel must remain a native Scene subtree.");
            }

            RectTransform panelRect = panel as RectTransform
                ?? throw new InvalidOperationException("ItemDetailPanel RectTransform missing.");
            LayoutGroup[] layoutGroups = panel.GetComponentsInChildren<LayoutGroup>(true);
            ContentSizeFitter[] contentFitters = panel.GetComponentsInChildren<ContentSizeFitter>(true);
            AspectRatioFitter[] aspectFitters = panel.GetComponentsInChildren<AspectRatioFitter>(true);
            LayoutElement[] layoutElements = panel.GetComponentsInChildren<LayoutElement>(true);
            int enabledDriverCount = layoutGroups.Count(driver => driver.enabled)
                + contentFitters.Count(driver => driver.enabled)
                + aspectFitters.Count(driver => driver.enabled)
                + layoutElements.Count(driver => driver.enabled);
            if (enabledDriverCount == 0)
            {
                Debug.Log("ITEM_DETAIL_ALL_RECTS_MANUAL_ALREADY_PASS");
                return;
            }

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);
            Canvas.ForceUpdateCanvases();
            ManualRectSnapshot[] snapshots = panel.GetComponentsInChildren<RectTransform>(true)
                .Select(ManualRectSnapshot.Capture)
                .ToArray();

            DisableDrivers(layoutGroups);
            DisableDrivers(contentFitters);
            DisableDrivers(aspectFitters);
            DisableDrivers(layoutElements);
            foreach (ManualRectSnapshot snapshot in snapshots)
            {
                snapshot.Restore();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
            {
                throw new InvalidOperationException("Failed to save fully manual ItemDetail layout.");
            }

            Debug.Log(
                "ITEM_DETAIL_ALL_RECTS_MANUAL_PASS: rects=" + snapshots.Length
                + "; disabledDrivers=" + enabledDriverCount);
        }

        [MenuItem("TalismanBag/Item Sandbox/Add Item Artwork Shadows (Once)")]
        public static void AddItemArtworkShadowsOnce()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException("Exit Play Mode before adding authored artwork shadows.");
            }

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid()
                || !scene.isLoaded
                || !string.Equals(scene.path, ScenePath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Open the ItemSandbox scene before adding artwork shadows.");
            }

            Transform panel = FindScenePath(scene, PanelPath)
                ?? throw new InvalidOperationException("ItemDetailPanel scene root not found: " + PanelPath);
            int addedCount = AddArtworkShadows(panel);
            if (addedCount > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
            }

            Debug.Log(
                "ITEM_DETAIL_ARTWORK_SHADOWS_PASS: added=" + addedCount
                + "; existing=" + (2 - addedCount)
                + "; sceneSaved=false");
        }

        private static int AddArtworkShadows(Transform panel)
        {
            Transform daoju = Require(panel, "ItemDetailHeader/ItemArtworkFrame/daoju");
            Image singleCellImage = Require(daoju, "DaojuSingleCellImage").GetComponent<Image>()
                ?? throw new InvalidOperationException("DaojuSingleCellImage Image component missing.");
            Image multiCellImage = Require(daoju, "DaojuMultiCellImage").GetComponent<Image>()
                ?? throw new InvalidOperationException("DaojuMultiCellImage Image component missing.");

            int addedCount = 0;
            addedCount += EnsureArtworkShadow(singleCellImage) ? 1 : 0;
            addedCount += EnsureArtworkShadow(multiCellImage) ? 1 : 0;
            return addedCount;
        }

        private static bool EnsureArtworkShadow(Image image)
        {
            Shadow shadow = image.GetComponent<Shadow>();
            if (shadow != null)
            {
                return false;
            }

            shadow = Undo.AddComponent<Shadow>(image.gameObject);
            shadow.effectColor = new Color32(0, 0, 0, 0x70);
            shadow.effectDistance = new Vector2(4f, -5f);
            shadow.useGraphicAlpha = true;
            EditorUtility.SetDirty(shadow);
            return true;
        }

        private static void DisableDrivers<T>(IEnumerable<T> drivers)
            where T : Behaviour
        {
            foreach (T driver in drivers)
            {
                if (driver != null && driver.enabled)
                {
                    driver.enabled = false;
                    EditorUtility.SetDirty(driver);
                }
            }
        }

        private static Dictionary<string, Sprite> ImportAndLoadSprites()
        {
            Dictionary<string, Sprite> result = new(StringComparer.Ordinal);
            foreach (KeyValuePair<string, string> asset in AssetPaths)
            {
                TextureImporter importer = AssetImporter.GetAtPath(asset.Value) as TextureImporter
                    ?? throw new InvalidOperationException("TextureImporter missing: " + asset.Value);
                if (importer.textureType != TextureImporterType.Sprite
                    || importer.spriteImportMode != SpriteImportMode.Single)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.mipmapEnabled = false;
                    importer.SaveAndReimport();
                }

                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(asset.Value)
                    ?? throw new InvalidOperationException("Sprite load failed: " + asset.Value);
                result.Add(asset.Key, sprite);
            }

            return result;
        }

        private static void EnsureHeaderSlots(Transform frame)
        {
            EnsureTextSlot(frame, "FaMenNameText", new Vector2(18f, -16f), new Vector2(200f, 30f), 20);
            EnsureTextSlot(frame, "QiLeiNameText", new Vector2(230f, -16f), new Vector2(160f, 30f), 20);
            EnsureTextSlot(frame, "PreviewComplianceText", new Vector2(18f, -232f), new Vector2(360f, 62f), 13);
        }

        private static void EnsureSectionSlots(Transform panel)
        {
            EnsureLineSlots(panel, "BaseStatsSection", "BaseStatIconSlot_", 8, 0, 26f);
            EnsureLineSlots(panel, "CoreAwakeningSection", "CoreEffectIconSlot_", 4, 0, 26f);
            EnsureLineSlots(panel, "FaMenBuildSection", "FaMenBuildStageIconSlot_", 3, 1, 26f);
            EnsureLineSlots(panel, "QiLeiBuildSection", "QiLeiBuildStageIconSlot_", 2, 1, 26f);
            EnsureLineSlots(panel, "MainBuildMonitorSection", "SkillMonitorIconSlot_", 4, 0, 26f);
            EnsureLineSlots(panel, "FixedAffixSection", "FixedAffixIconSlot_", 2, 0, 26f);
            EnsureLineSlots(panel, "RandomAffixSection", "RandomAffixIconSlot_", 4, 0, 26f);
            EnsureLineSlots(panel, "OrangeGrowthSection", "DaoTraceIconSlot_", 1, 0, 26f);
            EnsureLineSlots(panel, "PlacementHintSection", "PlacementIconSlot_", 1, 0, 26f);
            EnsureLineSlots(panel, "FlavorSection", "FlavorIconSlot_", 1, 0, 26f);

            Transform baseStatsBody = Require(FindDescendant(panel, "BaseStatsSection"), "BodyText");
            if (baseStatsBody.Find("BaseStatRowsRoot") == null)
            {
                EnsureImageSlot(
                    baseStatsBody,
                    "InlineArrayModifierIcon_0",
                    new Vector2(260f, -182f),
                    new Vector2(18f, 18f));
            }
            Transform qiLeiBuildBody = Require(FindDescendant(panel, "QiLeiBuildSection"), "BodyText");
            if (qiLeiBuildBody.Find("QiLeiBuildRowsRoot") == null)
            {
                EnsureImageSlot(
                    qiLeiBuildBody,
                    "InlineArrayModifierIcon_0",
                    new Vector2(330f, -52f),
                    new Vector2(18f, 18f));
            }

            EnsureCoarseDivider(panel, "CoreAwakeningSection");
            EnsureCoarseDivider(panel, "FaMenBuildSection");
            EnsureCoarseDivider(panel, "MainBuildMonitorSection");
        }

        private static void EnsureLineSlots(
            Transform panel,
            string sectionName,
            string prefix,
            int count,
            int firstLine,
            float lineHeight)
        {
            Transform section = FindDescendant(panel, sectionName)
                ?? throw new InvalidOperationException("Section missing: " + sectionName);
            Transform body = Require(section, "BodyText");
            string rowsRootName = string.Empty;
            string rowNamePrefix = string.Empty;
            if (string.Equals(prefix, "BaseStatIconSlot_", StringComparison.Ordinal))
            {
                rowsRootName = "BaseStatRowsRoot";
                rowNamePrefix = "BaseStatRow_";
            }
            else if (string.Equals(prefix, "FixedAffixIconSlot_", StringComparison.Ordinal))
            {
                rowsRootName = "FixedAffixRowsRoot";
                rowNamePrefix = "FixedAffixRow_";
            }
            else if (string.Equals(prefix, "RandomAffixIconSlot_", StringComparison.Ordinal))
            {
                rowsRootName = "RandomAffixRowsRoot";
                rowNamePrefix = "RandomAffixRow_";
            }
            else if (string.Equals(prefix, "DaoTraceIconSlot_", StringComparison.Ordinal))
            {
                rowsRootName = "DaoTraceRowsRoot";
                rowNamePrefix = "DaoTraceRow_";
            }
            else if (string.Equals(prefix, "CoreEffectIconSlot_", StringComparison.Ordinal))
            {
                rowsRootName = "CoreEffectRowsRoot";
                rowNamePrefix = "CoreEffectRow_";
            }
            else if (string.Equals(prefix, "FaMenBuildStageIconSlot_", StringComparison.Ordinal))
            {
                rowsRootName = "FaMenBuildRowsRoot";
                rowNamePrefix = "FaMenBuildRow_";
            }
            else if (string.Equals(prefix, "QiLeiBuildStageIconSlot_", StringComparison.Ordinal))
            {
                rowsRootName = "QiLeiBuildRowsRoot";
                rowNamePrefix = "QiLeiBuildRow_";
            }
            else if (string.Equals(prefix, "PlacementIconSlot_", StringComparison.Ordinal))
            {
                rowsRootName = "PlacementRowsRoot";
                rowNamePrefix = "PlacementRow_";
            }
            else if (string.Equals(prefix, "FlavorIconSlot_", StringComparison.Ordinal))
            {
                rowsRootName = "FlavorRowsRoot";
                rowNamePrefix = "FlavorRow_";
            }

            Transform rowsRoot = string.IsNullOrEmpty(rowsRootName) ? null : body.Find(rowsRootName);
            if (rowsRoot != null)
            {
                for (int index = 0; index < count; index++)
                {
                    Transform row = Require(rowsRoot, rowNamePrefix + index);
                    Require(row, prefix + index);
                }

                return;
            }

            for (int index = 0; index < count; index++)
            {
                EnsureImageSlot(
                    body,
                    prefix + index,
                    new Vector2(0f, -(firstLine + index) * lineHeight),
                    new Vector2(20f, 20f));
            }
        }

        private static void EnsureCoarseDivider(Transform panel, string sectionName)
        {
            Transform section = FindDescendant(panel, sectionName)
                ?? throw new InvalidOperationException("Section missing: " + sectionName);
            EnsureImageSlot(section, "CoarseDividerSlot", new Vector2(0f, 0f), new Vector2(520f, 8f));
        }

        private static Text EnsureTextSlot(
            Transform parent,
            string name,
            Vector2 anchoredPosition,
            Vector2 size,
            int fontSize)
        {
            Transform existing = parent.Find(name);
            Text text;
            if (existing == null)
            {
                GameObject slot = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                slot.transform.SetParent(parent, false);
                RectTransform rect = (RectTransform)slot.transform;
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = anchoredPosition;
                rect.sizeDelta = size;
                text = slot.GetComponent<Text>();
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                text.fontSize = fontSize;
                text.alignment = TextAnchor.UpperLeft;
                text.horizontalOverflow = HorizontalWrapMode.Wrap;
                text.verticalOverflow = VerticalWrapMode.Overflow;
                text.color = Color.white;
                text.raycastTarget = false;
            }
            else
            {
                text = existing.GetComponent<Text>() ?? existing.gameObject.AddComponent<Text>();
            }

            return text;
        }

        private static Image EnsureImageSlot(
            Transform parent,
            string name,
            Vector2 anchoredPosition,
            Vector2 size)
        {
            Transform existing = parent.Find(name);
            Image image;
            if (existing == null)
            {
                GameObject slot = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                slot.transform.SetParent(parent, false);
                RectTransform rect = (RectTransform)slot.transform;
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = anchoredPosition;
                rect.sizeDelta = size;
                image = slot.GetComponent<Image>();
                image.color = Color.white;
                image.raycastTarget = false;
                image.preserveAspect = true;
            }
            else
            {
                image = existing.GetComponent<Image>() ?? existing.gameObject.AddComponent<Image>();
            }

            return image;
        }

        private static ItemDetailViewModel BuildPreviewModel()
        {
            return new ItemDetailViewModel
            {
                itemId = "I001",
                baseItemId = "I001",
                itemInstanceId = PreviewId,
                placementId = "EDITOR_PREVIEW_LAYOUT_ONLY",
                rarityKey = "orange",
                rarityColorKey = "orange",
                rarityDisplayName = "道品",
                displayItemName = "震雷符",
                displayRarityName = "道品",
                displayFaMenName = "震雷法",
                displayQiLeiName = "符",
                displayShapeName = "单格",
                displayItemPower = "999",
                iconPlaceholderKey = "MISSING_ART_I001",
                statusFlags = new ItemDetailStatusFlags
                {
                    placementId = "EDITOR_PREVIEW_LAYOUT_ONLY",
                    isLit = true,
                    isDirectLit = true,
                    isOnArrayBonusCell = true,
                    isArrayBonusActive = true,
                    countedInBuild = true,
                    basicEffectActive = true,
                    coreEffectUnlocked = true,
                    coreEffectActive = true,
                    itemLevel = 40,
                    inputLevel = 40,
                    resolvedLevel = 40,
                    unlockedCoreEffectCount = 4,
                    activeCoreEffectCount = 4,
                    currentAwakeningNodeLevel = 4,
                    nextAwakeningNodeLevel = 4
                },
                displayPlayerSections = new List<ItemDetailSectionViewModel>
                {
                    Section("道品详情总览", "震雷符 · I001 · 道品 · 单格\n完整内容压力样本：最长文案、最多状态、全部展示槽同时开启\nEDITOR_PREVIEW_MAX_DAO_I001", "header"),
                    Section("法门与器类", "阵法：震雷法（阵法图标已接入）\n器类：符（qiLeiTag 技术语义；不是七星系统）", "identity"),
                    Section("当前状态", "点亮：聚念石直连点亮，基础效果与核心效果均可生效\n阵脉：已占阵脉格且阵脉加成生效\n开窍：4/4，四个核心效果全部点亮\n构筑：法门 6/6；器类 4/4；仅静态详情预览", "currentState"),
                    Section("完整基础属性", "伤害：128点（候选压力展示值）\n攻击：每次普攻追加震雷标记 1 层\n控制强度：27点\n破盾：18%\n冷却：3.2秒\n耗念：12点\n护势：命中后获得 24 点护势\n阵脉增幅：" + "[Icon_ArrayVeinModifier]" + " 额外伤害 +18、念力返还 +4", "stats"),
                    Section("触发条件", "摆入棋盘且被聚念石直接点亮；命中带盾目标时优先触发破盾链。阵脉、构筑与开窍状态在本模板中仅用于排版展示。", "trigger"),
                    Section("基础效果", "普攻命中后施加雷痕；若目标仍有护盾，则本次攻击额外提高破盾并记录一次震雷触发。", "basic"),
                    Section("4 个核心效果", "震雷符·初识：雷痕的基础伤害提高，并在破盾前保留一次触发计数\n震雷符·入门：引雷命中后追加雷击，对护盾目标获得更高收益\n震雷符·贯通：雷击可连锁命中额外目标，控制强度与破盾同步提高\n震雷符·成法：五雷破壳触发后返还念力，并刷新一次最长文案状态提示", "coreEffect"),
                    Section("震雷法 Build 2/4/6", "九霄雷君的敕令\n法门构筑：6/6\n-震雷符\n-五雷急符\n-震雷破壳印\n-五雷急令\n-照壳雷镜\n-天鼓槌\n2件效果：\n破盾提高 8%，雷痕对有盾目标优先结算。\n4件效果：\n破盾后追加雷击，触发系数 30%，并展示较长条件说明。\n6件效果：\n连锁命中额外目标 +1，五雷破壳完整成法。", "famenBuild"),
                    Section("符类 Build 效果", "符类构筑：4/4（qiLeiTag=符）\n2件效果：\n符箓相合：触发频率提高 6%，相邻符类计入同一器类构筑。\n4件效果：\n符阵成局：额外触发 1 次；此处仅展示排版，不计算真实激活。", "qileiBuild"),
                    Section("技能图标监看", "普攻：震雷符基础攻击与雷痕施加\nBuild2：破盾提高与低阶触发反馈\nBuild4：破盾后追加雷击\nBuild6：五雷破壳与额外连锁目标", "skillMonitor"),
                    Section("固定词条", "控制强度增加 +7 点，并提高对护盾目标的雷痕积累效率\n引雷伤害 +18 点；阵脉生效时追加念力返还说明", "fixedAffix"),
                    Section("随机词条", "触发后返还念力 +4 点\n破盾提高 +5%\n控制持续时间 +0.8 秒\n雷击连锁衰减降低 12%", "randomAffix"),
                    Section("道痕与成长", "道痕·雷痕贯壳：第四次雷痕命中穿透护盾并保留一次构筑计数。仅为 EDITOR_PREVIEW_ONLY 的排版压力文案。", "orange"),
                    Section("推荐摆放", "推荐靠近阵眼、聚念石与阵脉格；当前模板同时显示点亮、阵脉、开窍及完整 Build 状态，方便用户手调视口和章节高度。", "placement"),
                    Section("旧物记 / 合规标记", "旧木匣中压着一张被雷火烫卷的震雷符，背面仍留着借雷口诀。\nEDITOR_PREVIEW_ONLY · NOT_ROLLED_INSTANCE · NOT_LIVE_BALANCE_DATA", "flavor")
                },
                displayDebugSections = new List<ItemDetailSectionViewModel>
                {
                    Section("Preview Identity", PreviewId, "debugIdentity"),
                    Section("Preview Source", "STATIC_EDITOR_AUTHORING_SAMPLE / NO_CANDIDATE_WRITEBACK", "debugBuild"),
                    Section("Preview Guard", PreviewLabels.Replace("\n", " / "), "debugValidation")
                }
            };
        }

        private static ItemDetailSectionViewModel Section(string title, string body, string stateKey)
        {
            return new ItemDetailSectionViewModel(title, body, stateKey, true);
        }

        private static void BindHeaderArtwork(
            ItemDetailPanelView view,
            Transform artworkFrame,
            Transform zhenfa,
            Transform qixing,
            Transform daoju,
            IReadOnlyDictionary<string, Sprite> sprites,
            bool firstRun)
        {
            SerializedObject serializedView = new(view);
            BindImage(GetObjectReference<Image>(serializedView, "cardBackgroundImage"), sprites["background"]);
            BindImage(GetObjectReference<Image>(serializedView, "rarityBadgeImage"), sprites["rarity"]);

            Image zhenfaImage = zhenfa.GetComponent<Image>()
                ?? throw new InvalidOperationException("zhenfaicon Image missing.");
            bool qixingImageCreated = qixing.GetComponent<Image>() == null;
            Image qixingImage = qixing.GetComponent<Image>() ?? qixing.gameObject.AddComponent<Image>();
            Image daojuImage = daoju.GetComponent<Image>()
                ?? throw new InvalidOperationException("daoju Image missing.");
            BindImage(zhenfaImage, sprites["zhenfa"]);
            BindImage(qixingImage, sprites["qilei"]);
            if (qixingImageCreated)
            {
                qixingImage.raycastTarget = false;
                qixingImage.preserveAspect = true;
                qixingImage.color = Color.white;
            }

            Image singleCellArtwork = daoju.Find("DaojuSingleCellImage")?.GetComponent<Image>();
            Image multiCellArtwork = daoju.Find("DaojuMultiCellImage")?.GetComponent<Image>();
            if (singleCellArtwork != null && multiCellArtwork != null)
            {
                daojuImage.enabled = false;
                BindImage(singleCellArtwork, sprites["item_art_i001_orange"]);
                multiCellArtwork.sprite = sprites["item_art_i001_orange"];
                multiCellArtwork.enabled = false;
            }
            else
            {
                BindImage(daojuImage, sprites["item_art_i001_orange"]);
            }
            if (firstRun)
            {
                daojuImage.color = Color.white;
                daojuImage.preserveAspect = true;
            }

            Text artworkText = Require(artworkFrame, "ItemArtworkText").GetComponent<Text>();
            artworkText.enabled = false;
            artworkText.text = string.Empty;
            Text artworkKeyText = Require(artworkFrame, "ItemArtworkKeyPlate/ItemArtworkKeyText").GetComponent<Text>();
            artworkKeyText.text = "item_daoju/震雷法/I001/I001_5";
            Text faMenText = Require(artworkFrame, "FaMenNameText").GetComponent<Text>();
            faMenText.text = "阵法 · 震雷法";
            Text qiLeiText = Require(artworkFrame, "QiLeiNameText").GetComponent<Text>();
            qiLeiText.text = "器类 · 符";
            Text compliance = Require(artworkFrame, "PreviewComplianceText").GetComponent<Text>();
            compliance.text = PreviewId + "\n" + PreviewLabels.Replace("\n", " · ");

            BindImage(Require(view.transform, "ItemDetailHeader/ItemDetailHeaderTextGroup/ItemStatusBadgeRow/LightingStatusBadge").GetComponent<Image>(), sprites["focus"]);
            BindImage(Require(view.transform, "ItemDetailHeader/ItemDetailHeaderTextGroup/ItemStatusBadgeRow/AwakeningStatusBadge").GetComponent<Image>(), sprites["core4"]);
            BindImage(Require(view.transform, "ItemDetailHeader/ItemDetailHeaderTextGroup/ItemStatusBadgeRow/ArrayStatusBadge").GetComponent<Image>(), sprites["array"]);
        }

        private static void BindSectionArtwork(Transform panel, IReadOnlyDictionary<string, Sprite> sprites)
        {
            BindLineSprites(panel, "BaseStatsSection", "BaseStatIconSlot_", new[]
            {
                sprites["stat_damage"], sprites["stat_attack"], sprites["stat_control"], sprites["stat_break"],
                sprites["stat_cooldown"], sprites["stat_cost"], sprites["stat_guard"], sprites["array"]
            });
            BindLineSprites(panel, "CoreAwakeningSection", "CoreEffectIconSlot_", new[]
            {
                sprites["core1"], sprites["core2"], sprites["core3"], sprites["core4"]
            });
            BindLineSprites(panel, "FaMenBuildSection", "FaMenBuildStageIconSlot_", new[]
            {
                sprites["skill2"], sprites["skill3"], sprites["skill4"]
            });
            BindLineSprites(panel, "QiLeiBuildSection", "QiLeiBuildStageIconSlot_", new[]
            {
                sprites["qilei_skill1"], sprites["qilei_skill2"]
            });
            BindLineSprites(panel, "MainBuildMonitorSection", "SkillMonitorIconSlot_", new[]
            {
                sprites["skill1"], sprites["skill2"], sprites["skill3"], sprites["skill4"]
            });
            BindLineSprites(panel, "FixedAffixSection", "FixedAffixIconSlot_", new[]
            {
                sprites["fixed"], sprites["fixed"]
            });
            BindLineSprites(panel, "RandomAffixSection", "RandomAffixIconSlot_", new[]
            {
                sprites["random"], sprites["random"], sprites["random"], sprites["random"]
            });
            // Dao Trace has its own artwork slot but no approved art yet. Never reuse a fixed-affix
            // sprite as fake Dao Trace art; the user will assign the real sprite in the Inspector.
            BindLineSprites(panel, "PlacementHintSection", "PlacementIconSlot_", new[] { sprites["placement"] });
            BindLineSprites(panel, "FlavorSection", "FlavorIconSlot_", new[] { sprites["flavor"] });
            BindLineSprites(panel, "BaseStatsSection", "InlineArrayModifierIcon_", new[] { sprites["array"] });
            Transform qiLeiBuildBody = Require(FindDescendant(panel, "QiLeiBuildSection"), "BodyText");
            if (qiLeiBuildBody.Find("QiLeiBuildRowsRoot") == null)
            {
                BindLineSprites(panel, "QiLeiBuildSection", "InlineArrayModifierIcon_", new[] { sprites["array"] });
            }

            foreach (ItemDetailSectionView section in panel.GetComponentsInChildren<ItemDetailSectionView>(true))
            {
                Transform thinSlot = section.transform.Find("SectionDivider");
                if (thinSlot != null)
                {
                    BindImage(thinSlot.GetComponent<Image>(), sprites["thin"]);
                }

                Transform coarseSlot = section.transform.Find("CoarseDividerSlot");
                if (coarseSlot != null)
                {
                    BindImage(coarseSlot.GetComponent<Image>(), sprites["coarse"]);
                }
            }
        }

        private static void BindLineSprites(
            Transform panel,
            string sectionName,
            string prefix,
            IReadOnlyList<Sprite> sprites)
        {
            Transform body = Require(
                FindDescendant(panel, sectionName)
                    ?? throw new InvalidOperationException("Section missing: " + sectionName),
                "BodyText");
            for (int index = 0; index < sprites.Count; index++)
            {
                Transform slot = body.Find(prefix + index)
                    ?? FindDescendant(body, prefix + index)
                    ?? throw new InvalidOperationException(
                        "Required scene object missing: " + sectionName + "/" + prefix + index);
                BindImage(slot.GetComponent<Image>(), sprites[index]);
            }
        }

        private static void BindImage(Image image, Sprite sprite)
        {
            if (image == null)
            {
                throw new InvalidOperationException("Required Image component missing.");
            }

            image.sprite = sprite;
            image.enabled = sprite != null;
            image.gameObject.SetActive(true);
            EditorUtility.SetDirty(image);
        }

        private static T GetObjectReference<T>(SerializedObject serializedObject, string propertyName)
            where T : UnityEngine.Object
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName)
                ?? throw new InvalidOperationException("Serialized property missing: " + propertyName);
            return property.objectReferenceValue as T
                ?? throw new InvalidOperationException("Serialized reference missing: " + propertyName);
        }

        private static Transform FindScenePath(Scene scene, string path)
        {
            string[] segments = path.Split('/');
            GameObject root = scene.GetRootGameObjects()
                .FirstOrDefault(candidate => string.Equals(candidate.name, segments[0], StringComparison.Ordinal));
            if (root == null)
            {
                return null;
            }

            Transform current = root.transform;
            for (int index = 1; index < segments.Length && current != null; index++)
            {
                current = current.Find(segments[index]);
            }

            return current;
        }

        private static Transform FindDescendant(Transform root, string name)
        {
            return root.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(candidate => string.Equals(candidate.name, name, StringComparison.Ordinal));
        }

        private static Transform Require(Transform root, string relativePath)
        {
            Transform target = root != null ? root.Find(relativePath) : null;
            return target ?? throw new InvalidOperationException("Required scene object missing: " + relativePath);
        }

        private static void AssertIdentity(GameObject target, ulong expected, string label)
        {
            ulong actual = GlobalObjectId.GetGlobalObjectIdSlow(target).targetObjectId;
            if (actual != expected)
            {
                throw new InvalidOperationException($"{label} identity changed: expected {expected}, actual {actual}.");
            }
        }

        private readonly struct TransformSnapshot
        {
            private readonly Vector2 anchorMin;
            private readonly Vector2 anchorMax;
            private readonly Vector2 pivot;
            private readonly Vector2 anchoredPosition;
            private readonly Vector2 sizeDelta;
            private readonly Transform parent;
            private readonly int siblingIndex;

            private TransformSnapshot(RectTransform rect)
            {
                anchorMin = rect.anchorMin;
                anchorMax = rect.anchorMax;
                pivot = rect.pivot;
                anchoredPosition = rect.anchoredPosition;
                sizeDelta = rect.sizeDelta;
                parent = rect.parent;
                siblingIndex = rect.GetSiblingIndex();
            }

            public static TransformSnapshot Capture(RectTransform rect)
            {
                return rect == null
                    ? throw new InvalidOperationException("User slot RectTransform missing.")
                    : new TransformSnapshot(rect);
            }

            public void AssertUnchanged(RectTransform rect, string label)
            {
                bool unchanged = rect != null
                    && rect.anchorMin == anchorMin
                    && rect.anchorMax == anchorMax
                    && rect.pivot == pivot
                    && rect.anchoredPosition == anchoredPosition
                    && rect.sizeDelta == sizeDelta
                    && rect.parent == parent
                    && rect.GetSiblingIndex() == siblingIndex;
                if (!unchanged)
                {
                    throw new InvalidOperationException(label + " RectTransform or hierarchy changed.");
                }
            }
        }

        private readonly struct ManualRectSnapshot
        {
            private readonly RectTransform rect;
            private readonly Vector2 anchorMin;
            private readonly Vector2 anchorMax;
            private readonly Vector2 pivot;
            private readonly Vector3 anchoredPosition;
            private readonly Vector2 sizeDelta;
            private readonly Vector3 localScale;
            private readonly Vector3 localEulerAngles;

            private ManualRectSnapshot(RectTransform rect)
            {
                this.rect = rect;
                anchorMin = rect.anchorMin;
                anchorMax = rect.anchorMax;
                pivot = rect.pivot;
                anchoredPosition = rect.anchoredPosition3D;
                sizeDelta = rect.sizeDelta;
                localScale = rect.localScale;
                localEulerAngles = rect.localEulerAngles;
            }

            public static ManualRectSnapshot Capture(RectTransform rect)
            {
                return new ManualRectSnapshot(rect);
            }

            public void Restore()
            {
                if (rect == null)
                {
                    return;
                }

                rect.anchorMin = anchorMin;
                rect.anchorMax = anchorMax;
                rect.pivot = pivot;
                rect.anchoredPosition3D = anchoredPosition;
                rect.sizeDelta = sizeDelta;
                rect.localScale = localScale;
                rect.localEulerAngles = localEulerAngles;
                EditorUtility.SetDirty(rect);
            }
        }
    }
}
