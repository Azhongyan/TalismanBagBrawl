#if UNITY_EDITOR
using System;
using System.IO;
using TalismanBag.UnifiedBattle;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TalismanBag.EditorTools.UnifiedBattle
{
    public static class UnifiedBattlePageShellSceneBuilder
    {
        private static readonly Color BackgroundColor = new(0.055f, 0.06f, 0.055f, 1f);
        private static readonly Color RootColor = new(0.095f, 0.105f, 0.10f, 1f);
        private static readonly Color PlayerPanelColor = new(0.145f, 0.16f, 0.145f, 0.96f);
        private static readonly Color AdapterPanelColor = new(0.12f, 0.14f, 0.16f, 0.96f);
        private static readonly Color DevPanelColor = new(0.16f, 0.11f, 0.08f, 0.96f);
        private static readonly Color TextColor = new(0.88f, 0.86f, 0.76f, 1f);

        [MenuItem("Tools/Talisman Bag/V0.4/UnifiedBattle/UnifiedBattlePageShell01/[Writes Scene][Manual Only] Build Shell Scene And Prefab")]
        public static void BuildShellSceneAndPrefabMenu()
        {
            if (!EditorUtility.DisplayDialog(
                    "[Writes Scene][Manual Only] Build UnifiedBattle Shell",
                    "This creates or replaces only:\n" +
                    UnifiedBattlePageShellMarker.ScenePath + "\n" +
                    UnifiedBattlePageShellMarker.PrefabPath + "\n\n" +
                    "It does not modify Build Settings, V02/V03 formal scenes, RunFlow, SaveData, rewards, Boss flow, or chapter progression.",
                    "Build Shell",
                    "Cancel"))
            {
                return;
            }

            string[] outputs = BuildShellSceneAndPrefab();
            Debug.Log("[UnifiedBattlePageShell01] Shell scene/prefab built:\n" + string.Join("\n", outputs));
        }

        public static string[] BuildShellSceneAndPrefab()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException("UnifiedBattle shell builder must run in Edit Mode.");
            }

            string scenePath = BuildShellScene();
            string prefabPath = BuildShellPrefab();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return new[] { scenePath, prefabPath };
        }

        public static string BuildShellScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = UnifiedBattlePageShellMarker.SceneName;

            CreateMainCamera();
            CreateEventSystem();
            GameObject canvasObject = CreateCanvas();
            _ = CreateBattlePageRoot(canvasObject.transform);

            if (!EditorSceneManager.SaveScene(scene, UnifiedBattlePageShellMarker.ScenePath))
            {
                throw new InvalidOperationException("Could not save " + UnifiedBattlePageShellMarker.ScenePath);
            }

            return UnifiedBattlePageShellMarker.ScenePath;
        }

        public static string BuildShellPrefab()
        {
            EnsureAssetFolder(Path.GetDirectoryName(UnifiedBattlePageShellMarker.PrefabPath)?.Replace("\\", "/"));
            GameObject root = CreateBattlePageRoot(null).Root.gameObject;
            try
            {
                PrefabUtility.SaveAsPrefabAsset(root, UnifiedBattlePageShellMarker.PrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }

            return UnifiedBattlePageShellMarker.PrefabPath;
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

        private static GameObject CreateCanvas()
        {
            GameObject canvasObject = new(
                "UnifiedBattlePageShellCanvas",
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

            ConfigureStretch(canvasObject.GetComponent<RectTransform>());
            return canvasObject;
        }

        private static ShellBuildResult CreateBattlePageRoot(Transform parent)
        {
            GameObject rootObject = new(
                UnifiedBattlePageShellSlotNames.BattlePageRoot,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(UnifiedBattlePageShellMarker),
                typeof(UnifiedBattlePageShell));
            if (parent != null)
            {
                rootObject.transform.SetParent(parent, false);
            }

            RectTransform root = rootObject.GetComponent<RectTransform>();
            ConfigureStretch(root);
            Image rootImage = rootObject.GetComponent<Image>();
            rootImage.color = RootColor;
            rootImage.raycastTarget = false;

            Transform board = CreateSlot(
                root,
                UnifiedBattlePageShellSlotNames.BoardArea,
                "BoardArea",
                "BattleContract layout snapshot placeholder",
                PlayerPanelColor,
                new Vector2(0.04f, 0.34f),
                new Vector2(0.58f, 0.88f),
                out Text boardText);
            Transform tray = CreateSlot(
                root,
                UnifiedBattlePageShellSlotNames.ItemTrayArea,
                "ItemTrayArea",
                "BattleContract item roster placeholder",
                PlayerPanelColor,
                new Vector2(0.04f, 0.08f),
                new Vector2(0.58f, 0.30f),
                out Text trayText);
            Transform enemy = CreateSlot(
                root,
                UnifiedBattlePageShellSlotNames.EnemyInfoArea,
                "EnemyInfoArea",
                "Enemy / boss readable state placeholder",
                PlayerPanelColor,
                new Vector2(0.60f, 0.67f),
                new Vector2(0.78f, 0.88f),
                out Text enemyText);
            Transform bossCast = CreateSlot(
                root,
                UnifiedBattlePageShellSlotNames.BossCastBarSlot,
                "BossCastBarSlot",
                "Boss cast bar placeholder",
                PlayerPanelColor,
                new Vector2(0.60f, 0.56f),
                new Vector2(0.78f, 0.64f),
                out Text bossCastText);
            Transform feedback = CreateSlot(
                root,
                UnifiedBattlePageShellSlotNames.BattleFeedbackLayer,
                "BattleFeedbackLayer",
                "Player-safe battle feedback placeholder",
                PlayerPanelColor,
                new Vector2(0.60f, 0.40f),
                new Vector2(0.78f, 0.53f),
                out Text feedbackText);
            Transform storyGuide = CreateSlot(
                root,
                UnifiedBattlePageShellSlotNames.StoryGuidePopupLayer,
                "StoryGuidePopupLayer",
                "Story / guide / popup layer placeholder",
                PlayerPanelColor,
                new Vector2(0.60f, 0.25f),
                new Vector2(0.78f, 0.37f),
                out _);
            Transform resultReward = CreateSlot(
                root,
                UnifiedBattlePageShellSlotNames.ResultRewardPlaceholder,
                "ResultRewardPlaceholder",
                "Placeholder only: no reward grant, no save write",
                PlayerPanelColor,
                new Vector2(0.60f, 0.08f),
                new Vector2(0.78f, 0.22f),
                out Text resultText);
            Transform v03Adapter = CreateSlot(
                root,
                UnifiedBattlePageShellSlotNames.V03FlowAdapterSlot,
                "V03FlowAdapterSlot",
                "Future V03 flow adapter slot; disconnected",
                AdapterPanelColor,
                new Vector2(0.80f, 0.67f),
                new Vector2(0.96f, 0.88f),
                out Text v03Text);
            Transform v04Adapter = CreateSlot(
                root,
                UnifiedBattlePageShellSlotNames.V04SandboxAdapterSlot,
                "V04SandboxAdapterSlot",
                "Future V04 sandbox adapter slot; disconnected",
                AdapterPanelColor,
                new Vector2(0.80f, 0.45f),
                new Vector2(0.96f, 0.64f),
                out Text v04Text);
            Transform devDiagnostics = CreateSlot(
                root,
                UnifiedBattlePageShellSlotNames.DevOnlyDiagnosticsSlot,
                "DevOnlyDiagnosticsSlot",
                "Developer diagnostics separated from player UI",
                DevPanelColor,
                new Vector2(0.80f, 0.08f),
                new Vector2(0.96f, 0.42f),
                out Text devText);

            UnifiedBattlePageShell shell = rootObject.GetComponent<UnifiedBattlePageShell>();
            shell.AssignSlotsForEditor(
                root,
                board,
                tray,
                enemy,
                bossCast,
                feedback,
                storyGuide,
                resultReward,
                v03Adapter,
                v04Adapter,
                devDiagnostics);
            shell.AssignSampleTextTargetsForEditor(
                boardText,
                trayText,
                enemyText,
                bossCastText,
                feedbackText,
                resultText,
                v03Text,
                v04Text,
                devText);
            shell.BindSampleData();

            return new ShellBuildResult(root, shell);
        }

        private static Transform CreateSlot(
            Transform parent,
            string name,
            string title,
            string body,
            Color color,
            Vector2 anchorMin,
            Vector2 anchorMax,
            out Text bodyText)
        {
            GameObject panel = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Outline));
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;

            Image image = panel.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            Outline outline = panel.GetComponent<Outline>();
            outline.effectColor = new Color(0.46f, 0.40f, 0.24f, 0.7f);
            outline.effectDistance = new Vector2(1f, -1f);

            AddLabel(panel.transform, "Title", title, 20, TextAnchor.MiddleCenter, new Vector2(0.03f, 0.76f), new Vector2(0.97f, 0.98f));
            bodyText = AddLabel(panel.transform, "SummaryText", body, 15, TextAnchor.MiddleLeft, new Vector2(0.06f, 0.08f), new Vector2(0.94f, 0.74f));
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

        private static void EnsureAssetFolder(string assetFolder)
        {
            if (string.IsNullOrWhiteSpace(assetFolder))
            {
                return;
            }

            string[] parts = assetFolder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private readonly struct ShellBuildResult
        {
            public ShellBuildResult(RectTransform root, UnifiedBattlePageShell shell)
            {
                Root = root;
                Shell = shell;
            }

            public RectTransform Root { get; }
            public UnifiedBattlePageShell Shell { get; }
        }
    }
}
#endif
