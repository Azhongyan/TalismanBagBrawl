using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.Items.Detail;
using TalismanBag.Items.InnerCatalog;
using TalismanBag.ItemSandbox;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TalismanBag.EditorTools.ItemSandbox
{
    public static class ItemFullDetailBuildSandboxWorkbenchSceneBuilder
    {
        public const string ScenePath = "Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity";
        private const string PanelName = "ItemFullDetailBuildSandboxWorkbenchPanel";
        private static readonly Color PanelColor = new(0.10f, 0.075f, 0.05f, 0.98f);
        private static readonly Color ButtonColor = new(0.29f, 0.21f, 0.13f, 0.98f);
        private static readonly Color TextColor = new(0.86f, 0.80f, 0.70f, 1f);
        private static readonly Color AccentColor = new(0.72f, 0.48f, 0.18f, 1f);

        [MenuItem("Tools/Talisman Bag/V0.4/ItemSandbox/ItemFullDetailBuildSandboxWorkbench01/[Writes Scene][Manual Only] Apply Workbench")]
        public static void ApplyMenu()
        {
            Apply();
        }

        public static void ApplyBatch()
        {
            try
            {
                Apply();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        public static void Apply()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject root = FindSceneObject(scene, "ItemSandboxRoot")
                ?? throw new InvalidOperationException("Missing ItemSandboxRoot. Build the base Item Sandbox scene first.");
            ItemInnerDataCatalogProvider catalogProvider = root.GetComponent<ItemInnerDataCatalogProvider>()
                ?? throw new InvalidOperationException("Missing ItemInnerDataCatalogProvider.");
            ItemBalanceCandidateDetailSandboxProvider candidateProvider = root.GetComponent<ItemBalanceCandidateDetailSandboxProvider>()
                ?? throw new InvalidOperationException("Missing ItemBalanceCandidateDetailSandboxProvider.");
            ItemSandboxDetailPanelView detailPanel = Object.FindObjectOfType<ItemSandboxDetailPanelView>(true)
                ?? throw new InvalidOperationException("Missing ItemSandboxDetailPanelView.");

            Transform existing = root.transform.Find(PanelName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            ItemSandboxDetailUiController legacyController = root.GetComponent<ItemSandboxDetailUiController>();
            if (legacyController != null)
            {
                legacyController.enabled = false;
            }

            GameObject legacyPanel = FindSceneObject(scene, "ItemGridPlacementPreviewPanel");
            if (legacyPanel != null)
            {
                ItemSandboxGridPlacementPreviewView legacyPreview = legacyPanel.GetComponent<ItemSandboxGridPlacementPreviewView>();
                if (legacyPreview != null)
                {
                    legacyPreview.enabled = false;
                }

                CanvasGroup group = legacyPanel.GetComponent<CanvasGroup>();
                if (group == null)
                {
                    group = legacyPanel.AddComponent<CanvasGroup>();
                }
                group.alpha = 0f;
                group.interactable = false;
                group.blocksRaycasts = false;
            }

            Font font = FindFont(root) ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            GameObject panel = CreateUi(PanelName, root.transform);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            if (legacyPanel != null)
            {
                CopyRect(legacyPanel.GetComponent<RectTransform>(), panelRect);
            }
            else
            {
                SetRect(panelRect, new Vector2(0.255f, 0.045f), new Vector2(0.465f, 0.955f), Vector2.zero, Vector2.zero);
            }

            panel.AddComponent<Image>().color = PanelColor;
            VerticalLayoutGroup panelLayout = panel.AddComponent<VerticalLayoutGroup>();
            panelLayout.padding = new RectOffset(10, 10, 10, 10);
            panelLayout.spacing = 7f;
            panelLayout.childControlWidth = true;
            panelLayout.childControlHeight = true;
            panelLayout.childForceExpandWidth = true;
            panelLayout.childForceExpandHeight = false;

            CreateText("WorkbenchTitle", panel.transform, "完整实例 / 摆放 / Build 工作台", font, 22, AccentColor, TextAnchor.MiddleCenter, 44f);
            CreateText("WorkbenchBoundary", panel.transform,
                "BALANCE_CANDIDATE · NOT_LIVE_LOCKED · NOT_BATTLE_CONNECTED", font, 11, TextColor, TextAnchor.MiddleCenter, 34f);

            GameObject creationRow = CreateRow("InstanceCreationRow", panel.transform, 42f, 5f);
            Button createButton = CreateButton("CreateGeneratedInstanceButton", creationRow.transform, "创建实例", font);
            Button regenerateButton = CreateButton("RegenerateGeneratedInstanceButton", creationRow.transform, "Seed+1并生成", font);
            Button juNianButton = CreateButton("SelectFixedJuNianButton", creationRow.transform, "I031固定核心", font);

            CreateText("InstanceListTitle", panel.transform, "生成实例（同base可多实例）", font, 14, TextColor, TextAnchor.MiddleLeft, 28f);
            GameObject instanceList = CreateUi("GeneratedInstanceSlotList", panel.transform);
            VerticalLayoutGroup instanceLayout = instanceList.AddComponent<VerticalLayoutGroup>();
            instanceLayout.spacing = 3f;
            instanceLayout.childControlHeight = true;
            instanceLayout.childControlWidth = true;
            instanceLayout.childForceExpandHeight = false;
            AddLayout(instanceList, 0f, 350f, 0f);
            Button[] instanceButtons = new Button[12];
            Text[] instanceTexts = new Text[12];
            for (int index = 0; index < instanceButtons.Length; index++)
            {
                instanceButtons[index] = CreateButton($"GeneratedInstanceSlot_{index:00}", instanceList.transform, "unused", font, 25f);
                instanceTexts[index] = instanceButtons[index].GetComponentInChildren<Text>(true);
            }

            CreateText("BoardTitle", panel.transform, "5×5 手动棋盘（点选/放置/移动）", font, 14, TextColor, TextAnchor.MiddleLeft, 28f);
            GameObject board = CreateUi("FullWorkbenchBoardGrid", panel.transform);
            GridLayoutGroup boardLayout = board.AddComponent<GridLayoutGroup>();
            boardLayout.cellSize = new Vector2(54f, 54f);
            boardLayout.spacing = new Vector2(4f, 4f);
            boardLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            boardLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            boardLayout.childAlignment = TextAnchor.MiddleCenter;
            boardLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            boardLayout.constraintCount = 5;
            AddLayout(board, 286f, 286f, 0f);
            List<ItemFullDetailWorkbenchGridCellView> cells = new();
            for (int y = 4; y >= 0; y--)
            {
                for (int x = 0; x < 5; x++)
                {
                    GameObject cell = CreateUi($"FullWorkbenchCell_{x}_{y}", board.transform);
                    Image image = cell.AddComponent<Image>();
                    image.color = PanelColor;
                    Button button = cell.AddComponent<Button>();
                    button.targetGraphic = image;
                    Text label = CreateText("CellLabel", cell.transform, $"{x},{y}", font, 10, TextColor, TextAnchor.MiddleCenter, 0f);
                    Stretch(label.rectTransform);
                    ItemFullDetailWorkbenchGridCellView view = cell.AddComponent<ItemFullDetailWorkbenchGridCellView>();
                    view.ConfigureEditor(new Vector2Int(x, y), button, image, label);
                    cells.Add(view);
                }
            }

            GameObject placementActions = CreateRow("PlacementActionRow", panel.transform, 40f, 5f);
            Button rotateButton = CreateButton("RotateSelectedPlacementButton", placementActions.transform, "旋转90°", font);
            Button removeButton = CreateButton("RemoveSelectedPlacementButton", placementActions.transform, "移除摆放", font);
            Button loadLayoutButton = CreateButton("LoadEditableBuild6ValidationLayoutButton", placementActions.transform, "载入Build6验证布局", font);

            GameObject levelRow = CreateRow("SandboxLevelRow", panel.transform, 38f, 5f);
            Button levelDownButton = CreateButton("SandboxLevelDownButton", levelRow.transform, "Lv-", font);
            Text levelText = CreateText("SandboxLevelText", levelRow.transform, "Sandbox Lv.1", font, 13, TextColor, TextAnchor.MiddleCenter, 0f);
            AddLayout(levelText.gameObject, 92f, 32f, 1f);
            Button levelUpButton = CreateButton("SandboxLevelUpButton", levelRow.transform, "Lv+", font);

            CreateText("MainBuildTitle", panel.transform, "MainBuildSelectionPanel（默认None / 仅显式点击）", font, 13, TextColor, TextAnchor.MiddleLeft, 28f);
            GameObject mainBuildPanel = CreateUi("FullWorkbenchMainBuildSelectionPanel", panel.transform);
            GridLayoutGroup mainBuildGrid = mainBuildPanel.AddComponent<GridLayoutGroup>();
            mainBuildGrid.cellSize = new Vector2(91f, 37f);
            mainBuildGrid.spacing = new Vector2(4f, 4f);
            mainBuildGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            mainBuildGrid.constraintCount = 3;
            AddLayout(mainBuildPanel, 286f, 82f, 0f);
            string[] buildNames = { "None", "震雷", "离火", "中岳", "玄水", "太白" };
            Button[] mainBuildButtons = new Button[buildNames.Length];
            Text[] mainBuildTexts = new Text[buildNames.Length];
            for (int index = 0; index < buildNames.Length; index++)
            {
                mainBuildButtons[index] = CreateButton($"FullWorkbenchMainBuildButton_{index:00}", mainBuildPanel.transform, buildNames[index], font);
                mainBuildTexts[index] = mainBuildButtons[index].GetComponentInChildren<Text>(true);
            }

            CreateText("SkillMonitorTitle", panel.transform, "只读技能监控位", font, 13, TextColor, TextAnchor.MiddleLeft, 28f);
            GameObject monitors = CreateUi("FullWorkbenchSkillMonitorGrid", panel.transform);
            GridLayoutGroup monitorGrid = monitors.AddComponent<GridLayoutGroup>();
            monitorGrid.cellSize = new Vector2(68f, 60f);
            monitorGrid.spacing = new Vector2(4f, 4f);
            monitorGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            monitorGrid.constraintCount = 4;
            AddLayout(monitors, 286f, 64f, 0f);
            string[] monitorNames = { "普攻", "Build2", "Build4", "Build6" };
            Text[] monitorTexts = new Text[monitorNames.Length];
            for (int index = 0; index < monitorNames.Length; index++)
            {
                GameObject monitor = CreateUi($"FullWorkbenchSkillMonitor_{index:00}", monitors.transform);
                monitor.AddComponent<Image>().color = ButtonColor;
                monitorTexts[index] = CreateText("MonitorText", monitor.transform, monitorNames[index], font, 9, TextColor, TextAnchor.MiddleCenter, 0f);
                Stretch(monitorTexts[index].rectTransform);
            }

            Text statusText = CreateText("FullWorkbenchStatusText", panel.transform, string.Empty, font, 10, TextColor, TextAnchor.UpperLeft, 150f);
            statusText.horizontalOverflow = HorizontalWrapMode.Wrap;
            statusText.verticalOverflow = VerticalWrapMode.Truncate;

            ItemSandboxItemButtonView[] itemButtons = root.GetComponentsInChildren<ItemSandboxItemButtonView>(true)
                .Where(value => !string.Equals(value.ItemId, "I031", StringComparison.Ordinal))
                .OrderBy(value => value.ItemId, StringComparer.Ordinal)
                .ToArray();
            string[] rarityButtonNames =
            {
                "WhiteRarityButton", "GreenRarityButton", "BlueRarityButton",
                "PurpleRarityButton", "OrangeRarityButton"
            };
            Button[] rarityButtons = rarityButtonNames
                .Select(name => FindRecursive(root.transform, name)?.GetComponent<Button>())
                .Where(value => value != null)
                .ToArray();
            Text[] rarityTexts = rarityButtons.Select(value => value.GetComponentInChildren<Text>(true)).ToArray();
            InputField seedInput = root.GetComponentInChildren<InputField>(true);

            ItemFullDetailBuildSandboxWorkbenchView workbench = panel.AddComponent<ItemFullDetailBuildSandboxWorkbenchView>();
            workbench.ConfigureEditor(
                catalogProvider,
                candidateProvider,
                detailPanel,
                itemButtons,
                rarityButtons,
                rarityTexts,
                seedInput,
                createButton,
                regenerateButton,
                juNianButton,
                loadLayoutButton,
                instanceButtons,
                instanceTexts,
                cells.ToArray(),
                rotateButton,
                removeButton,
                levelDownButton,
                levelUpButton,
                levelText,
                mainBuildButtons,
                mainBuildTexts,
                monitorTexts,
                statusText);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"ItemFullDetailBuildSandboxWorkbench01 scene applied: {ScenePath}");
        }

        private static Button[] FindButtons(Transform root, string prefix)
        {
            return root.GetComponentsInChildren<Button>(true)
                .Where(button => button.name.StartsWith(prefix, StringComparison.Ordinal))
                .OrderBy(button => button.name, StringComparer.Ordinal)
                .ToArray();
        }

        private static GameObject FindSceneObject(Scene scene, string objectName)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Transform found = FindRecursive(root.transform, objectName);
                if (found != null)
                {
                    return found.gameObject;
                }
            }

            return null;
        }

        private static Transform FindRecursive(Transform root, string objectName)
        {
            if (string.Equals(root.name, objectName, StringComparison.Ordinal))
            {
                return root;
            }

            for (int index = 0; index < root.childCount; index++)
            {
                Transform found = FindRecursive(root.GetChild(index), objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static Font FindFont(GameObject root)
        {
            return root.GetComponentsInChildren<Text>(true).Select(value => value.font).FirstOrDefault(value => value != null);
        }

        private static GameObject CreateRow(string name, Transform parent, float height, float spacing)
        {
            GameObject row = CreateUi(name, parent);
            HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            AddLayout(row, 0f, height, 0f);
            return row;
        }

        private static Button CreateButton(string name, Transform parent, string label, Font font, float height = 34f)
        {
            GameObject buttonObject = CreateUi(name, parent);
            Image image = buttonObject.AddComponent<Image>();
            image.color = ButtonColor;
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            Text text = CreateText("Label", buttonObject.transform, label, font, 10, TextColor, TextAnchor.MiddleCenter, 0f);
            Stretch(text.rectTransform);
            AddLayout(buttonObject, 0f, height, 1f);
            return button;
        }

        private static Text CreateText(
            string name,
            Transform parent,
            string value,
            Font font,
            int fontSize,
            Color color,
            TextAnchor alignment,
            float height)
        {
            GameObject textObject = CreateUi(name, parent);
            Text text = textObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.text = value;
            text.raycastTarget = false;
            if (height > 0f)
            {
                AddLayout(textObject, 0f, height, 0f);
            }

            return text;
        }

        private static GameObject CreateUi(string name, Transform parent)
        {
            GameObject result = new(name, typeof(RectTransform));
            result.transform.SetParent(parent, false);
            return result;
        }

        private static void AddLayout(GameObject target, float preferredWidth, float preferredHeight, float flexibleWidth)
        {
            LayoutElement layout = target.GetComponent<LayoutElement>() ?? target.AddComponent<LayoutElement>();
            if (preferredWidth > 0f)
            {
                layout.preferredWidth = preferredWidth;
            }

            if (preferredHeight > 0f)
            {
                layout.preferredHeight = preferredHeight;
            }

            layout.flexibleWidth = flexibleWidth;
        }

        private static void CopyRect(RectTransform source, RectTransform target)
        {
            target.anchorMin = source.anchorMin;
            target.anchorMax = source.anchorMax;
            target.pivot = source.pivot;
            target.anchoredPosition = source.anchoredPosition;
            target.sizeDelta = source.sizeDelta;
            target.offsetMin = source.offsetMin;
            target.offsetMax = source.offsetMax;
        }

        private static void SetRect(RectTransform rect, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
