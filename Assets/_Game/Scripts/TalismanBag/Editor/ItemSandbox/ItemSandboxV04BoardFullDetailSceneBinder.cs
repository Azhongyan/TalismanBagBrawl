using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BuildSandbox;
using TalismanBag.Items.Detail.UI;
using TalismanBag.Items.InnerCatalog;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.ItemSandbox.Editor
{
    public static class ItemSandboxV04BoardFullDetailSceneBinder
    {
        public const string ScenePath = "Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity";

        [MenuItem("Tools/Talisman Bag/Dev Only/V0.4 ItemSandbox/[Writes Scene][Manual Only] Bind Board Full Detail Adapter")]
        public static void BindMenu()
        {
            BindBatch();
        }

        public static void BindBatch()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject sandboxRoot = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Select(transform => transform.gameObject)
                .FirstOrDefault(value => string.Equals(value.name, "ItemSandboxRoot", StringComparison.Ordinal));
            if (sandboxRoot == null)
            {
                throw new InvalidOperationException("ItemSandboxRoot was not found in the target V0.4 ItemSandbox scene.");
            }

            RectTransform battleLike = Require<RectTransform>(sandboxRoot, "BattleLikePreviewArea");
            RectTransform popupLayer = Require<RectTransform>(sandboxRoot, "PopupLayer");
            RectTransform feedbackRoot = Require<RectTransform>(sandboxRoot, "FeedbackRoot");
            ItemSandboxV04ProtectedGeometryBaseline[] before =
            {
                Capture(battleLike), Capture(popupLayer), Capture(feedbackRoot)
            };

            ItemInnerDataCatalogProvider catalogProvider = sandboxRoot.GetComponent<ItemInnerDataCatalogProvider>();
            ItemBalanceCandidateDetailSandboxProvider candidateProvider = sandboxRoot.GetComponent<ItemBalanceCandidateDetailSandboxProvider>();
            ItemDetailPanelView detailPanel = sandboxRoot.GetComponentInChildren<ItemDetailPanelView>(true);
            if (catalogProvider == null || candidateProvider == null || detailPanel == null)
            {
                throw new InvalidOperationException("Existing Item catalog/candidate/detail providers are incomplete.");
            }

            ScrollRect trayScroll = Require<ScrollRect>(battleLike.gameObject, "ItemTrayPreview");
            Transform trayContent = RequireTransform(battleLike.gameObject, "ItemTrayContent");
            Transform legacyLayer = RequireTransform(battleLike.gameObject, "ItemCardLayer");
            ItemSandboxV04TraySlotView[] traySlots = ConfigureTraySlots(trayContent, trayScroll);
            ItemSandboxV04BoardCellView[] boardCells = ConfigureBoardCells(battleLike.gameObject);
            Text feedbackText = ConfigureFeedbackText(feedbackRoot);

            Button[] rarityButtons =
            {
                Require<Button>(sandboxRoot, "WhiteRarityButton"),
                Require<Button>(sandboxRoot, "GreenRarityButton"),
                Require<Button>(sandboxRoot, "BlueRarityButton"),
                Require<Button>(sandboxRoot, "PurpleRarityButton"),
                Require<Button>(sandboxRoot, "OrangeRarityButton")
            };
            Text[] rarityLabels = rarityButtons.Select(button => button.GetComponentInChildren<Text>(true)).ToArray();
            InputField seedInput = Require<InputField>(sandboxRoot, "CandidateSeedInput");
            Button regenerateButton = Require<Button>(sandboxRoot, "RegenerateCandidateButton");
            Text candidateStatus = Require<Text>(sandboxRoot, "CandidateValidationText");
            GameObject candidateControlsRoot = RequireTransform(sandboxRoot, "CandidateInstancePreviewControls").gameObject;
            Button levelDown = Require<Button>(sandboxRoot, "SandboxLevelDownButton");
            Button levelUp = Require<Button>(sandboxRoot, "SandboxLevelUpButton");
            Text levelText = Require<Text>(sandboxRoot, "SandboxLevelText");
            Text buildSummary = Require<Text>(sandboxRoot, "PlacementStatusText");

            Button[] mainBuildButtons = Enumerable.Range(0, 6)
                .Select(index => Require<Button>(sandboxRoot, $"MainBuildSelectionButton_{index:00}"))
                .ToArray();
            Text[] mainBuildLabels = mainBuildButtons
                .Select(button => button.GetComponentsInChildren<Text>(true)
                    .FirstOrDefault(text => string.Equals(text.name, "MainBuildLabelText", StringComparison.Ordinal))
                    ?? button.GetComponentInChildren<Text>(true))
                .ToArray();
            Text[] monitorTexts = new[]
            {
                "SkillMonitorIcon_00_BasicAttack", "SkillMonitorIcon_01_Build2",
                "SkillMonitorIcon_02_Build4", "SkillMonitorIcon_03_Build6"
            }.Select(name => RequireTransform(sandboxRoot, name).GetComponentsInChildren<Text>(true)
                .FirstOrDefault(text => string.Equals(text.name, "MonitorText", StringComparison.Ordinal))
                ?? RequireTransform(sandboxRoot, name).GetComponentInChildren<Text>(true)).ToArray();

            Button rotate = Require<Button>(popupLayer.gameObject, "V04BattlePrepareStateButton");
            Button remove = Require<Button>(popupLayer.gameObject, "V04BattlePrepareBackButton");
            Button close = Require<Button>(popupLayer.gameObject, "V04BattlePrepareToggleButton");
            SetButtonText(rotate, "旋转选中");
            SetButtonText(remove, "移除选中");
            SetButtonText(close, "关闭详情");

            DisableLegacyDriversAndPanels(sandboxRoot, legacyLayer.gameObject);

            ItemSandboxV04BoardFullDetailAdapter adapter = sandboxRoot.GetComponent<ItemSandboxV04BoardFullDetailAdapter>();
            if (adapter == null)
            {
                adapter = Undo.AddComponent<ItemSandboxV04BoardFullDetailAdapter>(sandboxRoot);
            }
            adapter.ConfigureEditor(
                catalogProvider, candidateProvider, detailPanel,
                battleLike, popupLayer, feedbackRoot, trayScroll, legacyLayer.gameObject,
                traySlots, boardCells, rarityButtons, rarityLabels, seedInput, regenerateButton,
                candidateStatus, candidateControlsRoot, levelDown, levelUp, levelText, buildSummary,
                mainBuildButtons, mainBuildLabels, monitorTexts, rotate, remove, close,
                feedbackText, before);

            AssertProtectedGeometry(before, new[] { Capture(battleLike), Capture(popupLayer), Capture(feedbackRoot) });
            EditorUtility.SetDirty(adapter);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("ITEM_SANDBOX_V04_BOARD_FULL_DETAIL_ADAPTER01_BIND_PASS");
        }

        private static ItemSandboxV04TraySlotView[] ConfigureTraySlots(Transform trayContent, ScrollRect scrollRect)
        {
            ConfigureTrayScrollRect(trayContent, scrollRect);
            List<ItemSandboxV04TraySlotView> output = new();
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            for (int index = 1; index <= 40; index++)
            {
                Transform slotTransform = RequireTransform(trayContent.gameObject, $"TrayGridSlot_{index:00}");
                Image image = slotTransform.GetComponent<Image>();
                if (image == null)
                {
                    throw new InvalidOperationException(slotTransform.name + " has no serialized Image.");
                }
                Transform labelTransform = slotTransform.Find("ItemSandboxCatalogLabel");
                Text label;
                if (labelTransform == null)
                {
                    GameObject labelObject = new("ItemSandboxCatalogLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                    Undo.RegisterCreatedObjectUndo(labelObject, "Add ItemSandbox catalog label");
                    labelObject.transform.SetParent(slotTransform, false);
                    RectTransform rect = (RectTransform)labelObject.transform;
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.offsetMin = new Vector2(4f, 4f);
                    rect.offsetMax = new Vector2(-4f, -4f);
                    label = labelObject.GetComponent<Text>();
                    label.font = font;
                    label.fontSize = 15;
                    label.resizeTextForBestFit = true;
                    label.resizeTextMinSize = 9;
                    label.resizeTextMaxSize = 16;
                    label.alignment = TextAnchor.MiddleCenter;
                    label.horizontalOverflow = HorizontalWrapMode.Wrap;
                    label.verticalOverflow = VerticalWrapMode.Truncate;
                    label.raycastTarget = false;
                }
                else
                {
                    label = labelTransform.GetComponent<Text>();
                }
                ItemSandboxV04TraySlotView slot = slotTransform.GetComponent<ItemSandboxV04TraySlotView>();
                if (slot == null)
                {
                    slot = Undo.AddComponent<ItemSandboxV04TraySlotView>(slotTransform.gameObject);
                }
                slot.ConfigureEditor(index - 1, scrollRect, image, label);
                slotTransform.gameObject.SetActive(index <= 31);
                EditorUtility.SetDirty(slot);
                output.Add(slot);
            }
            return output.ToArray();
        }

        private static void ConfigureTrayScrollRect(Transform trayContent, ScrollRect scrollRect)
        {
            RectTransform content = trayContent as RectTransform;
            RectTransform scrollRectTransform = scrollRect == null ? null : scrollRect.transform as RectTransform;
            if (content == null || scrollRect == null || scrollRectTransform == null)
            {
                throw new InvalidOperationException("ItemTrayPreview ScrollRect/content binding is incomplete.");
            }

            scrollRect.content = content;
            scrollRect.viewport = scrollRect.viewport != null ? scrollRect.viewport : scrollRectTransform;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = Mathf.Max(scrollRect.scrollSensitivity, 24f);

            GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>();
            if (grid != null)
            {
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = Mathf.Max(1, grid.constraintCount);
            }

            ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
            if (fitter == null)
            {
                fitter = Undo.AddComponent<ContentSizeFitter>(content.gameObject);
            }
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            EditorUtility.SetDirty(scrollRect);
            EditorUtility.SetDirty(content);
            EditorUtility.SetDirty(fitter);
            if (grid != null)
            {
                EditorUtility.SetDirty(grid);
            }
        }

        private static ItemSandboxV04BoardCellView[] ConfigureBoardCells(GameObject battleLike)
        {
            List<ItemSandboxV04BoardCellView> output = new();
            for (int index = 1; index <= 25; index++)
            {
                Transform cellTransform = RequireTransform(battleLike, $"BoardGridCell_{index:00}");
                BuildGridPreviewSlotView copiedSlot = cellTransform.GetComponent<BuildGridPreviewSlotView>();
                Vector2Int cell = copiedSlot != null
                    ? new Vector2Int(copiedSlot.Cell.x, copiedSlot.Cell.y)
                    : new Vector2Int((index - 1) % 5, (index - 1) / 5);
                Image image = cellTransform.GetComponent<Image>();
                Text label = cellTransform.GetComponentInChildren<Text>(true);
                if (image == null || label == null)
                {
                    throw new InvalidOperationException(cellTransform.name + " lacks its copied Image/Text binding.");
                }
                ItemSandboxV04BoardCellView view = cellTransform.GetComponent<ItemSandboxV04BoardCellView>();
                if (view == null)
                {
                    view = Undo.AddComponent<ItemSandboxV04BoardCellView>(cellTransform.gameObject);
                }
                view.ConfigureEditor(cell, image, label);
                EditorUtility.SetDirty(view);
                output.Add(view);
            }
            return output.OrderBy(value => value.Cell.y).ThenBy(value => value.Cell.x).ToArray();
        }

        private static Text ConfigureFeedbackText(RectTransform feedbackRoot)
        {
            Transform existing = feedbackRoot.Find("ItemSandboxFeedbackText");
            if (existing != null)
            {
                return existing.GetComponent<Text>();
            }
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            GameObject feedbackObject = new("ItemSandboxFeedbackText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            Undo.RegisterCreatedObjectUndo(feedbackObject, "Add ItemSandbox feedback text");
            feedbackObject.transform.SetParent(feedbackRoot, false);
            RectTransform rect = (RectTransform)feedbackObject.transform;
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 14f);
            rect.sizeDelta = new Vector2(820f, 88f);
            Text text = feedbackObject.GetComponent<Text>();
            text.font = font;
            text.fontSize = 22;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.color = Color.white;
            text.raycastTarget = false;
            text.text = "ItemSandbox：点击候选查看详情，拖到阵盘生成独立实例。";
            return text;
        }

        private static void DisableLegacyDriversAndPanels(GameObject sandboxRoot, GameObject legacyLayer)
        {
            SetEnabled(sandboxRoot.GetComponentInChildren<ItemFullDetailBuildSandboxWorkbenchView>(true), false);
            SetEnabled(sandboxRoot.GetComponentInChildren<ItemSandboxGridPlacementPreviewView>(true), false);
            SetEnabled(sandboxRoot.GetComponentInChildren<BuildItemTrayPreviewView>(true), false);
            SetEnabled(sandboxRoot.GetComponentInChildren<BuildSandboxItemInfoPanel>(true), false);
            legacyLayer.SetActive(false);

            SetActiveIfFound(sandboxRoot, "ItemFullDetailBuildSandboxWorkbenchPanel", false);
            SetActiveIfFound(sandboxRoot, "ItemDetailContextModeBar", false);
            SetActiveIfFound(sandboxRoot, "ItemNameListPanel", false);
            SetActiveIfFound(sandboxRoot, "PlacementBoardGrid", false);
            SetActiveIfFound(sandboxRoot, "LightingScenarioButtonGrid", false);
            SetActiveIfFound(sandboxRoot, "LightingScenarioTitleText", false);
            SetActiveIfFound(sandboxRoot, "PlacementActionRow", false);
            SetActiveIfFound(sandboxRoot, "BuildSandboxItemInfoPanel_Runtime", false);
            SetActiveIfFound(sandboxRoot, "PlacementFeedback_Runtime", false);
        }

        private static void SetEnabled(Behaviour behaviour, bool enabled)
        {
            if (behaviour != null)
            {
                behaviour.enabled = enabled;
                EditorUtility.SetDirty(behaviour);
            }
        }

        private static void SetActiveIfFound(GameObject root, string name, bool active)
        {
            Transform transform = FindTransform(root, name);
            if (transform != null)
            {
                transform.gameObject.SetActive(active);
            }
        }

        private static void SetButtonText(Button button, string value)
        {
            Text label = button?.GetComponentInChildren<Text>(true);
            if (label != null)
            {
                label.text = value;
                EditorUtility.SetDirty(label);
            }
        }

        private static ItemSandboxV04ProtectedGeometryBaseline Capture(RectTransform transform)
        {
            return new ItemSandboxV04ProtectedGeometryBaseline
            {
                objectName = transform.name,
                parentName = transform.parent == null ? string.Empty : transform.parent.name,
                activeSelf = transform.gameObject.activeSelf,
                siblingIndex = transform.GetSiblingIndex(),
                anchorMin = transform.anchorMin,
                anchorMax = transform.anchorMax,
                pivot = transform.pivot,
                anchoredPosition = transform.anchoredPosition,
                sizeDelta = transform.sizeDelta,
                localScale = transform.localScale,
                localEulerAngles = transform.localEulerAngles
            };
        }

        private static void AssertProtectedGeometry(
            IReadOnlyList<ItemSandboxV04ProtectedGeometryBaseline> before,
            IReadOnlyList<ItemSandboxV04ProtectedGeometryBaseline> after)
        {
            if (before.Count != after.Count)
            {
                throw new InvalidOperationException("Protected geometry root count changed.");
            }
            for (int index = 0; index < before.Count; index++)
            {
                ItemSandboxV04ProtectedGeometryBaseline left = before[index];
                ItemSandboxV04ProtectedGeometryBaseline right = after[index];
                bool equal = left.objectName == right.objectName
                    && left.parentName == right.parentName
                    && left.activeSelf == right.activeSelf
                    && left.siblingIndex == right.siblingIndex
                    && left.anchorMin == right.anchorMin
                    && left.anchorMax == right.anchorMax
                    && left.pivot == right.pivot
                    && left.anchoredPosition == right.anchoredPosition
                    && left.sizeDelta == right.sizeDelta
                    && left.localScale == right.localScale
                    && left.localEulerAngles == right.localEulerAngles;
                if (!equal)
                {
                    throw new InvalidOperationException(left.objectName + " protected hierarchy/geometry changed during binding.");
                }
            }
        }

        private static T Require<T>(GameObject root, string objectName) where T : Component
        {
            Transform transform = RequireTransform(root, objectName);
            T component = transform.GetComponent<T>();
            if (component == null)
            {
                component = transform.GetComponentInChildren<T>(true);
            }
            return component != null
                ? component
                : throw new InvalidOperationException(objectName + " has no " + typeof(T).Name + ".");
        }

        private static Transform RequireTransform(GameObject root, string objectName)
        {
            return FindTransform(root, objectName)
                ?? throw new InvalidOperationException(objectName + " was not found under " + root.name + ".");
        }

        private static Transform FindTransform(GameObject root, string objectName)
        {
            return root.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(transform => string.Equals(transform.name, objectName, StringComparison.Ordinal));
        }
    }
}
