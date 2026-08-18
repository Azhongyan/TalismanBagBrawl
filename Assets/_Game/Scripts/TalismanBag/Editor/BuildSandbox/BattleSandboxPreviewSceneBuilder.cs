#if UNITY_EDITOR
using TalismanBag.BuildSandbox;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BattleSandboxPreviewSceneBuilder
    {
        private static readonly Color BackgroundColor = new(0.045f, 0.052f, 0.05f, 1f);
        private static readonly Color PanelColor = new(0.12f, 0.135f, 0.13f, 0.96f);
        private static readonly Color SlotColor = new(0.22f, 0.235f, 0.22f, 0.92f);
        private static readonly Color AccentColor = new(0.46f, 0.36f, 0.18f, 1f);
        private static readonly Color TextColor = new(0.88f, 0.86f, 0.76f, 1f);

        [MenuItem("Tools/Talisman Bag/Dev Only/V0.4/BuildSandbox/BattleSandboxPreviewScene01/[Writes Scene][Manual Only] Build Preview Scene")]
        public static void BuildPreviewSceneMenu()
        {
            if (!EditorUtility.DisplayDialog(
                    "[Writes Scene][Manual Only] Build Preview Scene",
                    "This creates or replaces only:\n" +
                    BuildSandboxPreviewSceneMarker.ScenePath + "\n\n" +
                    "It does not modify Build Settings or V02/V03 product scenes.",
                    "Build Scene",
                    "Cancel"))
            {
                return;
            }

            BuildPreviewScene();
        }

        public static string BuildPreviewScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new System.InvalidOperationException("Build Preview Scene must run in Edit Mode.");
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = BuildSandboxPreviewSceneMarker.SceneName;

            CreateMainCamera();
            CreateEventSystem();
            CreateMarkerRoot();
            CreateCanvas();

            if (!EditorSceneManager.SaveScene(scene, BuildSandboxPreviewSceneMarker.ScenePath))
            {
                throw new System.InvalidOperationException(
                    "Could not save " + BuildSandboxPreviewSceneMarker.ScenePath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[V0.4-BattleSandboxPreviewScene01] PREVIEW_SCENE_BUILT path=" +
                      BuildSandboxPreviewSceneMarker.ScenePath);
            return BuildSandboxPreviewSceneMarker.ScenePath;
        }

        private static void CreateMainCamera()
        {
            GameObject cameraObject = new("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = BackgroundColor;
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.transform.position = new Vector3(0f, 0f, -10f);
        }

        private static void CreateEventSystem()
        {
            _ = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private static void CreateMarkerRoot()
        {
            GameObject root = new("BuildSandboxPreviewRoot", typeof(BuildSandboxPreviewSceneMarker));
            root.transform.position = Vector3.zero;
        }

        private static void CreateCanvas()
        {
            GameObject canvasObject = new(
                "BuildSandboxPreviewCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            ConfigureStretch(canvasRect);

            Transform safeArea = CreatePanel(canvasObject.transform, "SafeAreaRoot", BackgroundColor, Vector2.zero, Vector2.one);
            BuildBattleLikePreviewArea(safeArea);
            BuildProblemSelectorPanel(safeArea);
            BuildDataPanelDock(safeArea);
            BuildControlBar(safeArea);
            BuildBattlePrepareChrome(safeArea);
            Transform popupLayer = CreatePanel(safeArea, "PopupLayer", new Color(0f, 0f, 0f, 0f), Vector2.zero, Vector2.one);
            BuildEnemyCombatFeedbackFloatingRoot(popupLayer);
        }

        private static void BuildBattleLikePreviewArea(Transform parent)
        {
            Transform area = CreateAnchoredPanel(
                parent,
                "BattleLikePreviewArea",
                PanelColor,
                new Vector2(0.26f, 0.1f),
                new Vector2(0.66f, 0.88f),
                Vector2.zero,
                Vector2.zero);
            AddLabel(area, "Title", "Battle Like Preview Area", 26, TextAnchor.MiddleCenter, new Vector2(0f, 0.92f), new Vector2(1f, 1f));

            Transform board = CreateAnchoredPanel(
                area,
                "BoardGridPreview",
                new Color(0.15f, 0.18f, 0.17f, 1f),
                new Vector2(0.09f, 0.38f),
                new Vector2(0.91f, 0.86f),
                Vector2.zero,
                Vector2.zero);
            AddLabel(board, "BoardGridTitle", "BoardGridPreview", 22, TextAnchor.UpperCenter, new Vector2(0f, 0.87f), new Vector2(1f, 1f));
            BuildBoardCells(board);

            Transform tray = CreateAnchoredPanel(
                area,
                "ItemTrayPreview",
                new Color(0.105f, 0.12f, 0.12f, 1f),
                new Vector2(0.09f, 0.08f),
                new Vector2(0.64f, 0.32f),
                Vector2.zero,
                Vector2.zero);
            AddLabel(tray, "ItemTrayTitle", "ItemTrayPreview", 20, TextAnchor.UpperCenter, new Vector2(0f, 0.76f), new Vector2(1f, 1f));
            BuildTraySlots(tray);

            Transform info = CreateAnchoredPanel(
                area,
                "SelectedItemInfo",
                new Color(0.16f, 0.145f, 0.11f, 1f),
                new Vector2(0.68f, 0.08f),
                new Vector2(0.91f, 0.32f),
                Vector2.zero,
                Vector2.zero);
            AddLabel(info, "SelectedItemInfoTitle", "SelectedItemInfo", 18, TextAnchor.UpperCenter, new Vector2(0f, 0.72f), new Vector2(1f, 1f));
            AddLabel(info, "SelectedItemInfoBody", "Item info slot", 16, TextAnchor.MiddleCenter, new Vector2(0.05f, 0.08f), new Vector2(0.95f, 0.72f));

            Transform feedback = CreateAnchoredPanel(
                area,
                "PlacementFeedback",
                new Color(0.19f, 0.13f, 0.08f, 1f),
                new Vector2(0.09f, 0.01f),
                new Vector2(0.91f, 0.06f),
                Vector2.zero,
                Vector2.zero);
            AddLabel(feedback, "PlacementFeedbackText", "PlacementFeedback", 16, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one);

            BuildEnemyCombatFeedbackPanel(area);
        }

        private static void BuildProblemSelectorPanel(Transform parent)
        {
            Transform panel = CreateAnchoredPanel(
                parent,
                "ProblemSelectorPanel",
                PanelColor,
                new Vector2(0.02f, 0.1f),
                new Vector2(0.24f, 0.88f),
                Vector2.zero,
                Vector2.zero);
            AddLabel(panel, "Title", "沙盒选择", 24, TextAnchor.MiddleCenter, new Vector2(0f, 0.91f), new Vector2(1f, 1f));

            CreateSlot(panel, "MapRuleDropdownSlot", "地图规则", 0.82f);
            CreateSlot(panel, "EnemyProblemDropdownSlot", "敌人压力", 0.67f);
            CreateSlot(panel, "BossProblemDropdownSlot", "首领机制", 0.52f);
            CreateSlot(panel, "DevChapterDropdownSlot", "开发章节", 0.37f);
            BuildEnemyCombatFeedbackControlPanel(panel);
        }

        private static void BuildDataPanelDock(Transform parent)
        {
            Transform panel = CreateAnchoredPanel(
                parent,
                "BuildSandboxDataPanelDock",
                PanelColor,
                new Vector2(0.68f, 0.1f),
                new Vector2(0.98f, 0.88f),
                Vector2.zero,
                Vector2.zero);
            AddLabel(panel, "Title", "沙盒数据 / 开发者遮罩", 24, TextAnchor.MiddleCenter, new Vector2(0f, 0.91f), new Vector2(1f, 1f));

            string[] slots =
            {
                "BuildSummaryPanelSlot",
                "SynergyPanelSlot",
                "ShapeOccupancyPanelSlot",
                "AffixModifierPanelSlot",
                "ProblemReadinessPanelSlot",
                "SimulationResultPanelSlot"
            };

            for (int i = 0; i < slots.Length; i++)
            {
                float top = 0.84f - i * 0.055f;
                CreateSlot(panel, slots[i], slots[i], top);
            }

            BuildEnemyCombatFeedbackDeveloperPanel(panel);
        }

        private static void BuildEnemyCombatFeedbackControlPanel(Transform parent)
        {
            Transform panel = CreateAnchoredPanel(
                parent,
                "EnemyCombatFeedbackControlPanel",
                PanelColor,
                new Vector2(0.06f, 0.03f),
                new Vector2(0.94f, 0.29f),
                Vector2.zero,
                Vector2.zero);
            AddLabel(panel, "Title", "战斗反馈预览", 17, TextAnchor.MiddleCenter, new Vector2(0.04f, 0.76f), new Vector2(0.96f, 0.98f));
            CreateButtonSlot(panel, "PreviousFeedbackButton", "上一反馈", new Vector2(0.05f, 0.52f), new Vector2(0.47f, 0.72f));
            CreateButtonSlot(panel, "NextFeedbackButton", "下一反馈", new Vector2(0.53f, 0.52f), new Vector2(0.95f, 0.72f));
            CreateButtonSlot(panel, "TriggerFloatingFeedbackButton", "触发短句", new Vector2(0.05f, 0.28f), new Vector2(0.95f, 0.48f));
            AddLabel(panel, "ControlStatusText", "只显示状态、施法、机制短句", 13, TextAnchor.MiddleCenter, new Vector2(0.05f, 0.06f), new Vector2(0.95f, 0.24f));
        }

        private static void BuildEnemyCombatFeedbackPanel(Transform parent)
        {
            Transform panel = CreateAnchoredPanel(
                parent,
                "EnemyCombatFeedbackPanel",
                new Color(0.055f, 0.065f, 0.065f, 0.92f),
                new Vector2(0.08f, 0.66f),
                new Vector2(0.92f, 0.91f),
                Vector2.zero,
                Vector2.zero);
            AddLabel(panel, "Title", "战斗反馈预览", 19, TextAnchor.MiddleCenter, new Vector2(0.03f, 0.75f), new Vector2(0.97f, 0.97f));
            AddLabel(panel, "BossStateText", "首领状态反馈", 18, TextAnchor.MiddleLeft, new Vector2(0.05f, 0.55f), new Vector2(0.95f, 0.75f));
            AddLabel(panel, "BossSkillText", "施法中：锁阵冲击", 17, TextAnchor.MiddleLeft, new Vector2(0.05f, 0.36f), new Vector2(0.70f, 0.54f));

            Transform castBar = CreateAnchoredPanel(
                panel,
                "BossCastBarRoot",
                new Color(0.07f, 0.075f, 0.075f, 0.96f),
                new Vector2(0.05f, 0.20f),
                new Vector2(0.95f, 0.34f),
                Vector2.zero,
                Vector2.zero);
            Transform fill = CreateAnchoredPanel(castBar, "BossCastFill", new Color(1f, 0.48f, 0.30f, 1f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image fillImage = fill.GetComponent<Image>();
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImage.fillAmount = 0.85f;
            AddLabel(castBar, "BossCastTimerText", "2.4秒", 16, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one);
            AddLabel(panel, "CombatLogText", "【机制】阵面压力升高。", 15, TextAnchor.MiddleLeft, new Vector2(0.05f, 0.03f), new Vector2(0.95f, 0.18f));
        }

        private static void BuildEnemyCombatFeedbackFloatingRoot(Transform parent)
        {
            Transform root = CreateAnchoredPanel(parent, "EnemyCombatFeedbackFloatingRoot", new Color(0f, 0f, 0f, 0f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Text text = AddLabel(root, "MechanicFloatingText", "破绽窗口出现", 28, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            text.fontStyle = FontStyle.Bold;
            text.color = new Color(1f, 0.95f, 0.45f, 1f);
            RectTransform rect = text.rectTransform;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(120f, 110f);
            rect.sizeDelta = new Vector2(360f, 54f);
            text.gameObject.AddComponent<CanvasGroup>();
        }

        private static void BuildEnemyCombatFeedbackDeveloperPanel(Transform parent)
        {
            Transform panel = CreateAnchoredPanel(
                parent,
                "EnemyCombatFeedbackDeveloperPanel",
                PanelColor,
                new Vector2(0.05f, 0.02f),
                new Vector2(0.95f, 0.51f),
                Vector2.zero,
                Vector2.zero);
            AddLabel(panel, "Title", "开发者数据遮罩", 18, TextAnchor.MiddleCenter, new Vector2(0.03f, 0.86f), new Vector2(0.97f, 0.98f));
            AddLabel(panel, "MaskedDeveloperFieldsText", "敏感答案字段只留在这里或报告。", 13, TextAnchor.MiddleLeft, new Vector2(0.05f, 0.54f), new Vector2(0.95f, 0.84f));
            AddLabel(panel, "CombatFeedbackIsolationText", "不接正式战斗、奖励、存档、章节推进或功能开关。", 13, TextAnchor.MiddleLeft, new Vector2(0.05f, 0.30f), new Vector2(0.95f, 0.52f));
            AddLabel(panel, "CombatFeedbackReuseText", "复用浮字、敌人意图施法条与首领状态口径。", 13, TextAnchor.MiddleLeft, new Vector2(0.05f, 0.12f), new Vector2(0.95f, 0.28f));
        }

        private static void BuildControlBar(Transform parent)
        {
            Transform bar = CreateAnchoredPanel(
                parent,
                "DevOnlyControlBar",
                new Color(0.1f, 0.11f, 0.105f, 1f),
                new Vector2(0.02f, 0.9f),
                new Vector2(0.98f, 0.98f),
                Vector2.zero,
                Vector2.zero);
            AddLabel(bar, "Title", "V0.4 BuildSandbox Preview - devOnly", 24, TextAnchor.MiddleLeft, new Vector2(0.02f, 0f), new Vector2(0.48f, 1f));

            CreateButtonSlot(bar, "RunSimulationButtonSlot", "Run Simulation", new Vector2(0.55f, 0.18f), new Vector2(0.68f, 0.82f));
            CreateButtonSlot(bar, "ResetPreviewButtonSlot", "Reset Preview", new Vector2(0.70f, 0.18f), new Vector2(0.83f, 0.82f));
            CreateButtonSlot(bar, "ExportReportButtonSlot", "Export Report", new Vector2(0.85f, 0.18f), new Vector2(0.98f, 0.82f));
        }

        private static void BuildBoardCells(Transform board)
        {
            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    string name = $"BoardGridCell_{y * 5 + x + 1:00}";
                    Vector2 min = new(0.14f + x * 0.145f, 0.12f + y * 0.14f);
                    Vector2 max = min + new Vector2(0.11f, 0.105f);
                    Transform cell = CreateAnchoredPanel(board, name, SlotColor, min, max, Vector2.zero, Vector2.zero);
                    Outline outline = cell.gameObject.AddComponent<Outline>();
                    outline.effectColor = new Color(0.5f, 0.48f, 0.35f, 0.7f);
                    outline.effectDistance = new Vector2(1f, -1f);
                }
            }
        }

        private static void BuildTraySlots(Transform tray)
        {
            for (int i = 0; i < 10; i++)
            {
                int row = i / 5;
                int col = i % 5;
                Vector2 min = new(0.08f + col * 0.18f, 0.18f + row * 0.26f);
                Vector2 max = min + new Vector2(0.13f, 0.18f);
                Transform slot = CreateAnchoredPanel(tray, $"ItemTraySlot_{i + 1:00}", SlotColor, min, max, Vector2.zero, Vector2.zero);
                Outline outline = slot.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color(0.44f, 0.36f, 0.18f, 0.7f);
                outline.effectDistance = new Vector2(1f, -1f);
            }
        }

        private static void CreateSlot(Transform parent, string name, string label, float centerY)
        {
            Transform slot = CreateAnchoredPanel(
                parent,
                name,
                SlotColor,
                new Vector2(0.08f, centerY - 0.055f),
                new Vector2(0.92f, centerY + 0.055f),
                Vector2.zero,
                Vector2.zero);
            Outline outline = slot.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.43f, 0.37f, 0.24f, 0.75f);
            outline.effectDistance = new Vector2(1f, -1f);
            AddLabel(slot, "Label", label, 18, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one);
        }

        private static void CreateButtonSlot(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax)
        {
            Transform slot = CreateAnchoredPanel(parent, name, AccentColor, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            Button button = slot.gameObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = AccentColor;
            colors.highlightedColor = new Color(0.57f, 0.44f, 0.21f, 1f);
            colors.pressedColor = new Color(0.32f, 0.25f, 0.13f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;
            AddLabel(slot, "Label", label, 17, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one);
        }

        private static void BuildBattlePrepareChrome(Transform parent)
        {
            Transform overlay = CreatePanel(
                parent,
                "V04BattlePrepareDarkOverlay",
                new Color(0f, 0f, 0f, 0.42f),
                Vector2.zero,
                Vector2.one);
            overlay.gameObject.SetActive(false);

            GameObject barObject = new("V04BattlePrepareBottomActions", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(GridLayoutGroup));
            barObject.transform.SetParent(parent, false);
            RectTransform bar = barObject.GetComponent<RectTransform>();
            bar.anchorMin = new Vector2(0.5f, 0f);
            bar.anchorMax = new Vector2(0.5f, 0f);
            bar.pivot = new Vector2(0.5f, 0f);
            bar.sizeDelta = new Vector2(800f, 92f);
            bar.anchoredPosition = new Vector2(0f, 24f);
            bar.localScale = Vector3.one;

            Image image = barObject.GetComponent<Image>();
            image.color = new Color(0.10f, 0.11f, 0.105f, 0.96f);
            image.raycastTarget = true;

            GridLayoutGroup grid = barObject.GetComponent<GridLayoutGroup>();
            grid.padding = new RectOffset(12, 12, 7, 7);
            grid.spacing = new Vector2(16f, 0f);
            grid.cellSize = new Vector2(246f, 78f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;

            CreateBattlePrepareButton(
                bar,
                "V04BattlePrepareBackButton",
                "\u56de\u9996\u9875",
                new Color(0.22f, 0.28f, 0.32f, 1f));
            CreateBattlePrepareButton(
                bar,
                "V04BattlePrepareStateButton",
                "\u7ee7\u7eed\u6218\u6597",
                new Color(0.28f, 0.31f, 0.34f, 1f));
            CreateBattlePrepareButton(
                bar,
                "V04BattlePrepareToggleButton",
                "\u6574\u5907",
                new Color(0.50f, 0.32f, 0.16f, 1f));
        }

        private static void CreateBattlePrepareButton(Transform parent, string name, string label, Color color)
        {
            GameObject buttonObject = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;

            Image image = buttonObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = true;

            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.12f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.22f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(color.r * 0.55f, color.g * 0.55f, color.b * 0.55f, 0.72f);
            button.colors = colors;

            AddLabel(buttonObject.transform, "Label", label, 20, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one);
        }

        private static Transform CreatePanel(Transform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax)
        {
            return CreateAnchoredPanel(parent, name, color, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
        }

        private static Transform CreateAnchoredPanel(
            Transform parent,
            string name,
            Color color,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            GameObject panel = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.localScale = Vector3.one;
            Image image = panel.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return panel.transform;
        }

        private static Text AddLabel(
            Transform parent,
            string name,
            string value,
            int fontSize,
            TextAnchor alignment,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            GameObject textObject = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;

            Text text = textObject.GetComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = TextColor;
            text.raycastTarget = false;
            return text;
        }

        private static void ConfigureStretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.localScale = Vector3.one;
        }
    }
}
#endif
