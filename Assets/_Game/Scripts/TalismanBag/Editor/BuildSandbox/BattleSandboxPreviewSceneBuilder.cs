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

        [MenuItem("Tools/Talisman Bag/V0.4/BuildSandbox/BattleSandboxPreviewScene01/[Writes Scene][Manual Only] Build Preview Scene")]
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
            CreatePanel(safeArea, "PopupLayer", new Color(0f, 0f, 0f, 0f), Vector2.zero, Vector2.one);
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
            AddLabel(panel, "Title", "Problem Selector", 24, TextAnchor.MiddleCenter, new Vector2(0f, 0.91f), new Vector2(1f, 1f));

            CreateSlot(panel, "MapRuleDropdownSlot", "Map Rule", 0.75f);
            CreateSlot(panel, "EnemyProblemDropdownSlot", "Enemy Problem", 0.57f);
            CreateSlot(panel, "BossProblemDropdownSlot", "Boss Problem", 0.39f);
            CreateSlot(panel, "DevChapterDropdownSlot", "Dev Chapter", 0.21f);
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
            AddLabel(panel, "Title", "Build Sandbox Data", 24, TextAnchor.MiddleCenter, new Vector2(0f, 0.91f), new Vector2(1f, 1f));

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
                float top = 0.82f - i * 0.13f;
                CreateSlot(panel, slots[i], slots[i], top);
            }
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
