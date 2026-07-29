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
using Object = UnityEngine.Object;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BattleSandboxAllItemUiAuthoringVisibilityTool
    {
        public const string TargetScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity";
        public const string PreviewMarker =
            "EDITOR_DEV_ONLY_AUTHORING_PREVIEW";
        public const string DisabledMarker =
            "EDITOR_DEV_ONLY_AUTHORING_DISABLED";

        private const string MenuRoot =
            "Talisman Bag/V0.4/BattleSandbox Authoring/";
        private const string EnableMenuPath =
            MenuRoot + "Enable Full BattleSandbox Item UI Authoring Preview";
        private const string DisableMenuPath =
            MenuRoot + "Disable Full BattleSandbox Item UI Authoring Preview";
        private const string ValidateMenuPath =
            MenuRoot + "Validate Full BattleSandbox Item UI Authoring Preview";
        private const string ArrayModifierIconToken =
            "[Icon_ArrayVeinModifier]";

        private static readonly string[] AuthoringReferenceProperties =
        {
            "itemNameText",
            "metaText",
            "powerText",
            "artworkText",
            "artworkFrameImage",
            "artworkImageSlot",
            "artworkKeyText",
            "rarityBadgeImage",
            "rarityBadgeText",
            "faMenIconImage",
            "qiLeiIconImage",
            "statusBadgeImages",
            "statusBadgeTexts",
            "closeButton",
            "rarityAccentImage",
            "cardBackgroundImage",
            "detailTabButton",
            "debugTabButton",
            "detailTabText",
            "debugTabText",
            "detailScrollRoot"
        };

        [MenuItem(EnableMenuPath)]
        private static void EnablePreviewMenu()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!TryEnablePreview(
                    scene,
                    recordUndo: true,
                    markSceneDirty: true,
                    out string diagnostic))
            {
                Debug.LogError(
                    "[BattleSandboxAllItemUiAuthoringVisibilityTool] "
                    + diagnostic);
                return;
            }

            Debug.Log(
                "[BattleSandboxAllItemUiAuthoringVisibilityTool] "
                + "Full BattleSandbox Item UI authoring preview enabled. "
                + PreviewMarker);
        }

        [MenuItem(DisableMenuPath)]
        private static void DisablePreviewMenu()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!TryDisablePreview(
                    scene,
                    recordUndo: true,
                    markSceneDirty: true,
                    out string diagnostic))
            {
                Debug.LogError(
                    "[BattleSandboxAllItemUiAuthoringVisibilityTool] "
                    + diagnostic);
                return;
            }

            Debug.Log(
                "[BattleSandboxAllItemUiAuthoringVisibilityTool] "
                + "Full BattleSandbox Item UI authoring preview disabled; "
                + "the existing authored hierarchy and geometry were preserved.");
        }

        [MenuItem(ValidateMenuPath)]
        private static void ValidatePreviewMenu()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!TryValidatePreview(scene, out string diagnostic))
            {
                Debug.LogError(
                    "[BattleSandboxAllItemUiAuthoringVisibilityTool] "
                    + diagnostic);
                return;
            }

            Debug.Log(
                "[BattleSandboxAllItemUiAuthoringVisibilityTool] "
                + "Authoring preview validation PASS. "
                + PreviewMarker);
        }

        [MenuItem(EnableMenuPath, true)]
        [MenuItem(DisableMenuPath, true)]
        [MenuItem(ValidateMenuPath, true)]
        private static bool ValidateMenuAvailability()
        {
            return !EditorApplication.isPlayingOrWillChangePlaymode
                && IsExactActiveTargetScene(SceneManager.GetActiveScene());
        }

        public static bool TryEnablePreview(
            Scene scene,
            bool recordUndo,
            bool markSceneDirty,
            out string diagnostic)
        {
            diagnostic = string.Empty;
            if (!TryResolveExactPanel(scene, out ItemDetailPanelView panel,
                    out diagnostic))
            {
                return false;
            }

            if (recordUndo)
            {
                Undo.RegisterFullObjectHierarchyUndo(
                    panel.gameObject,
                    "Enable Full BattleSandbox Item UI Authoring Preview");
            }

            ActivateExistingChain(panel.transform, panel.transform);
            panel.SetPreserveAuthoredVisualStyle(true);
            panel.Bind(BuildAuthoringPreviewModel());
            panel.ShowPlayerDetailTab();
            ActivateReferencedAuthoringObjects(panel);
            panel.SetVisible(true);

            MarkPanelObjectsDirty(panel);
            if (markSceneDirty)
            {
                EditorSceneManager.MarkSceneDirty(scene);
            }

            if (!ContainsMarker(panel, PreviewMarker))
            {
                diagnostic =
                    "AUTHORING_PREVIEW_MARKER_NOT_RENDERED";
                return false;
            }

            diagnostic = "AUTHORING_PREVIEW_ENABLED";
            return true;
        }

        public static bool TryDisablePreview(
            Scene scene,
            bool recordUndo,
            bool markSceneDirty,
            out string diagnostic)
        {
            diagnostic = string.Empty;
            if (!TryResolveExactPanel(scene, out ItemDetailPanelView panel,
                    out diagnostic))
            {
                return false;
            }

            if (recordUndo)
            {
                Undo.RegisterFullObjectHierarchyUndo(
                    panel.gameObject,
                    "Disable Full BattleSandbox Item UI Authoring Preview");
            }

            foreach (Text text in panel.GetComponentsInChildren<Text>(true))
            {
                if (text == null || string.IsNullOrEmpty(text.text)
                    || !text.text.Contains(
                        PreviewMarker,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                text.text = text.text.Replace(
                    PreviewMarker,
                    DisabledMarker,
                    StringComparison.Ordinal);
                EditorUtility.SetDirty(text);
            }

            panel.SetVisible(false);
            EditorUtility.SetDirty(panel.gameObject);
            EditorUtility.SetDirty(panel);
            if (markSceneDirty)
            {
                EditorSceneManager.MarkSceneDirty(scene);
            }

            diagnostic = "AUTHORING_PREVIEW_DISABLED";
            return true;
        }

        public static bool TryValidatePreview(
            Scene scene,
            out string diagnostic)
        {
            diagnostic = string.Empty;
            if (!TryResolveExactPanel(scene, out ItemDetailPanelView panel,
                    out diagnostic))
            {
                return false;
            }

            if (!panel.gameObject.activeInHierarchy)
            {
                diagnostic = "AUTHORING_PREVIEW_PANEL_NOT_VISIBLE";
                return false;
            }

            if (!ContainsMarker(panel, PreviewMarker))
            {
                diagnostic = "AUTHORING_PREVIEW_MARKER_MISSING";
                return false;
            }

            ItemDetailSectionView[] sections =
                panel.GetComponentsInChildren<ItemDetailSectionView>(true);
            ItemDetailSectionView[] playerSections = sections
                .Where(value => value != null
                    && IsPlayerSectionName(value.gameObject.name))
                .ToArray();
            if (playerSections.Length != 15
                || playerSections.Any(value =>
                    !value.gameObject.activeSelf
                    || string.IsNullOrWhiteSpace(value.Body)))
            {
                diagnostic =
                    "AUTHORING_PREVIEW_PLAYER_SECTIONS_INCOMPLETE";
                return false;
            }

            diagnostic = "AUTHORING_PREVIEW_VALID";
            return true;
        }

        public static ItemDetailViewModel BuildAuthoringPreviewModel()
        {
            List<ItemDetailCoreEffectRowState> coreStates = new()
            {
                ItemDetailCoreEffectRowState.UnlockedInactive,
                ItemDetailCoreEffectRowState.Active,
                ItemDetailCoreEffectRowState.Active,
                ItemDetailCoreEffectRowState.Active
            };

            return new ItemDetailViewModel
            {
                itemId = "I004",
                baseItemId = "I004",
                itemInstanceId =
                    "EDITOR_DEV_ONLY_AUTHORING_PREVIEW_I004",
                placementId =
                    "EDITOR_DEV_ONLY_AUTHORING_PREVIEW_PLACEMENT",
                rarityKey = "orange",
                rarityColorKey = "orange",
                rarityDisplayName = "orange",
                displayItemName = "五雷急令",
                displayRarityName = "橙色",
                displayFaMenName = "震雷法",
                displayQiLeiName = "令",
                displayShapeName = "竖排两格",
                displayItemPower = "999",
                displayTriggerText = "满足触发条件后催发震雷令文。",
                displayLightingStatusText = "已点亮",
                displayAwakeningStatusText = "四层核心已解锁",
                displayArrayBonusStatusText = "阵脉生效",
                displayOrangeAffix =
                    "雷痕贯壳：破盾提高 +12%",
                displayFlavorText =
                    "旧木匣里压着一张被雷火烫卷的急令。",
                iconPlaceholderKey = PreviewMarker,
                statusFlags = new ItemDetailStatusFlags
                {
                    placementId =
                        "EDITOR_DEV_ONLY_AUTHORING_PREVIEW_PLACEMENT",
                    isLit = true,
                    isDirectLit = true,
                    coreEffectUnlocked = true,
                    coreEffectActive = true,
                    isOnArrayBonusCell = true,
                    isArrayBonusActive = true,
                    countedInBuild = true,
                    basicEffectActive = true,
                    inputLevel = 40,
                    resolvedLevel = 40,
                    itemLevel = 40,
                    unlockedCoreEffectCount = 4,
                    activeCoreEffectCount = 3,
                    currentAwakeningNodeLevel = 40,
                    nextAwakeningNodeLevel = 40
                },
                awakeningPreview = new ItemAwakeningPreview
                {
                    inputLevel = 40,
                    resolvedLevel = 40,
                    itemLevel = 40,
                    coreEffectUnlocked = true,
                    coreEffectActive = true,
                    unlockedCoreEffectCount = 4,
                    activeCoreEffectCount = 3,
                    currentNodeText = "I004_CORE_ULT",
                    nextNodeText = "已全部解锁",
                    readOnlyInputText = PreviewMarker
                },
                displayPlayerSections =
                    BuildPlayerSections(coreStates),
                displayDebugSections =
                    BuildDebugSections()
            };
        }

        private static List<ItemDetailSectionViewModel>
            BuildPlayerSections(
                IReadOnlyList<ItemDetailCoreEffectRowState> coreStates)
        {
            return new List<ItemDetailSectionViewModel>
            {
                Section(
                    "道具概要",
                    PreviewMarker
                    + "\n橙色 · 五雷急令 · Lv.40 · 战力 999",
                    "header"),
                Section(
                    "核心身份",
                    "法门：震雷法\n器类：令\n形状：竖排两格",
                    "identity"),
                Section(
                    "当前状态",
                    "已入阵 · 已点亮 · 阵脉生效 · 构筑计数中",
                    "currentState"),
                Section(
                    "基础属性",
                    "  伤害：128\n  控制强度：27点\n"
                    + "  破盾：18%\n  念力返还：4点 "
                    + ArrayModifierIconToken
                    + " +2点\n  冷却：3.2秒",
                    "stats"),
                Section(
                    "触发条件",
                    "点亮后按既有震雷规则触发；本内容仅用于编辑预览。",
                    "trigger"),
                Section(
                    "基础效果",
                    "引下一记短雷并留下雷痕；数值仅为编辑预览。",
                    "basic"),
                new ItemDetailSectionViewModel(
                    "核心效果",
                    "  震雷符·初识：破盾提高 +5%\n"
                    + "  震雷符·入门：命中后追加雷痕，伤害 +12点\n"
                    + "  震雷符·贯通：雷痕目标额外击穿，控制 +7点\n"
                    + "  震雷符·终式：五雷破壳时返还念力 +4点",
                    "coreEffect",
                    true,
                    coreStates),
                Section(
                    "法门构筑",
                    "  九霄雷君的敕令\n"
                    + "  法门构筑：6/6\n"
                    + "  -五雷急令\n  -照壳雷镜\n"
                    + "  -伏雷法印\n  -鸣雷符箓\n"
                    + "  -电母令文\n  -破壳雷章\n"
                    + "  2件效果：\n  雷痕效果提高；破盾提高 +5%\n"
                    + "  4件效果：\n  震雷击穿提高；控制强度 +7点\n"
                    + "  6件效果：\n  五雷破壳生效；伤害 +18点",
                    "famenBuild"),
                Section(
                    "器类构筑",
                    "  令类构筑：4/4\n"
                    + "  2件效果：\n  令势相合；念力返还 +4点\n"
                    + "  4件效果：\n  令文成局；额外触发 +1次 "
                    + ArrayModifierIconToken + " +1次",
                    "qileiBuild"),
                Section(
                    "主构筑监测",
                    "主构筑：震雷法 6/6\n阶段：六件效果已激活",
                    "skillMonitor"),
                Section(
                    "固定词条",
                    "  控制强度增加 +7点\n"
                    + "  引雷伤害 +18点 "
                    + ArrayModifierIconToken + " +2点",
                    "fixedAffix"),
                Section(
                    "随机词条",
                    "  触发后返还念力 +4点\n  破盾提高 +5%",
                    "randomAffix"),
                Section(
                    "道痕",
                    "  雷痕贯壳：破盾提高 +12%",
                    "orange"),
                Section(
                    "推荐摆放",
                    "  推荐靠近阵眼或阵脉格；此行为只预览既有插槽。",
                    "placement"),
                Section(
                    "旧物日记",
                    "  旧木匣里压着一张被雷火烫卷的急令，"
                    + "背面仍留着借雷口诀。",
                    "flavor")
            };
        }

        private static List<ItemDetailSectionViewModel>
            BuildDebugSections()
        {
            return new List<ItemDetailSectionViewModel>
            {
                Section(
                    "Debug Identity",
                    PreviewMarker
                    + "\nsource=EditorExplicitMenu\nruntimeOwner=false",
                    "debugIdentity"),
                Section(
                    "Debug Build",
                    "FaMen=famen:zhenlei\nQiLei=qilei:ling\n"
                    + "qualification=Dual",
                    "debugBuild"),
                Section(
                    "Debug Validation",
                    "EDITOR_ONLY=true\nSAVE_DATA_WRITE=false\n"
                    + "FORMAL_RUNTIME_OVERRIDE=false",
                    "debugValidation")
            };
        }

        private static ItemDetailSectionViewModel Section(
            string title,
            string body,
            string stateKey)
        {
            return new ItemDetailSectionViewModel(
                title,
                body,
                stateKey,
                keepWhenEmpty: true);
        }

        private static bool TryResolveExactPanel(
            Scene scene,
            out ItemDetailPanelView panel,
            out string diagnostic)
        {
            panel = null;
            diagnostic = string.Empty;
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                diagnostic = "AUTHORING_TOOL_DISABLED_IN_PLAY_MODE";
                return false;
            }
            if (!IsExactActiveTargetScene(scene))
            {
                diagnostic =
                    "AUTHORING_TOOL_REQUIRES_EXACT_ACTIVE_TARGET_SCENE";
                return false;
            }

            ItemDetailPanelView[] panels = scene.GetRootGameObjects()
                .SelectMany(root =>
                    root.GetComponentsInChildren<ItemDetailPanelView>(true))
                .Where(value => value != null
                    && value.gameObject.scene == scene)
                .ToArray();
            if (panels.Length != 1
                || !string.Equals(
                    panels[0].gameObject.name,
                    "ItemDetailPanel",
                    StringComparison.Ordinal))
            {
                diagnostic =
                    "AUTHORING_TOOL_REQUIRES_ONE_AUTHORED_ITEMDETAIL_PANEL";
                return false;
            }

            panel = panels[0];
            return true;
        }

        private static bool IsExactActiveTargetScene(Scene scene)
        {
            Scene activeScene = SceneManager.GetActiveScene();
            return scene.IsValid()
                && scene.isLoaded
                && activeScene.IsValid()
                && activeScene.handle == scene.handle
                && string.Equals(
                    NormalizePath(scene.path),
                    TargetScenePath,
                    StringComparison.Ordinal);
        }

        private static void ActivateReferencedAuthoringObjects(
            ItemDetailPanelView panel)
        {
            SerializedObject serialized = new(panel);
            foreach (string propertyName in AuthoringReferenceProperties)
            {
                SerializedProperty property =
                    serialized.FindProperty(propertyName);
                if (property == null)
                {
                    continue;
                }

                if (property.isArray
                    && property.propertyType !=
                    SerializedPropertyType.String)
                {
                    for (int index = 0;
                         index < property.arraySize;
                         index++)
                    {
                        ActivateReferencedObject(
                            property.GetArrayElementAtIndex(index)
                                .objectReferenceValue,
                            panel.transform);
                    }
                }
                else
                {
                    ActivateReferencedObject(
                        property.objectReferenceValue,
                        panel.transform);
                }
            }
        }

        private static void ActivateReferencedObject(
            Object referenced,
            Transform panelRoot)
        {
            Transform target = referenced switch
            {
                GameObject gameObject => gameObject.transform,
                Component component => component.transform,
                _ => null
            };
            ActivateExistingChain(target, panelRoot);
        }

        private static void ActivateExistingChain(
            Transform target,
            Transform inclusiveStop)
        {
            Transform current = target;
            while (current != null)
            {
                if (!current.gameObject.activeSelf)
                {
                    current.gameObject.SetActive(true);
                    EditorUtility.SetDirty(current.gameObject);
                }
                if (current == inclusiveStop)
                {
                    break;
                }
                current = current.parent;
            }
        }

        private static bool ContainsMarker(
            ItemDetailPanelView panel,
            string marker)
        {
            return panel.GetComponentsInChildren<Text>(true)
                .Any(value => value != null
                    && (value.text ?? string.Empty).Contains(
                        marker,
                        StringComparison.Ordinal));
        }

        private static void MarkPanelObjectsDirty(
            ItemDetailPanelView panel)
        {
            foreach (Transform transform in
                     panel.GetComponentsInChildren<Transform>(true))
            {
                if (transform != null)
                {
                    EditorUtility.SetDirty(transform.gameObject);
                }
            }
            foreach (Text text in
                     panel.GetComponentsInChildren<Text>(true))
            {
                EditorUtility.SetDirty(text);
            }
            foreach (Image image in
                     panel.GetComponentsInChildren<Image>(true))
            {
                EditorUtility.SetDirty(image);
            }
            EditorUtility.SetDirty(panel);
        }

        private static bool IsPlayerSectionName(string value)
        {
            return value switch
            {
                "HeaderSection" => true,
                "CoreIdentitySection" => true,
                "CurrentStateSection" => true,
                "BaseStatsSection" => true,
                "TriggerConditionSection" => true,
                "BasicEffectSection" => true,
                "CoreAwakeningSection" => true,
                "FaMenBuildSection" => true,
                "QiLeiBuildSection" => true,
                "MainBuildMonitorSection" => true,
                "FixedAffixSection" => true,
                "RandomAffixSection" => true,
                "OrangeGrowthSection" => true,
                "PlacementHintSection" => true,
                "FlavorSection" => true,
                _ => false
            };
        }

        private static string NormalizePath(string value)
        {
            return (value ?? string.Empty).Replace('\\', '/');
        }
    }
}
