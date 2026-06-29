using System.Collections.Generic;
using System.Linq;
using TalismanBag.V03.BootEntry;
using TalismanBag.V03.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.V03.Editor
{
    public static class V03BootEntryFlow01SceneBuilder
    {
        public const string BootScenePath = "Assets/_Game/Scenes/Scene_TalismanBag_V03_BootEntry.unity";
        private const string BootSceneName = "Scene_TalismanBag_V03_BootEntry";
        private static readonly Color PageBackgroundColor = new(0.035f, 0.047f, 0.043f, 1f);
        private static readonly Color PanelColor = new(0.11f, 0.14f, 0.12f, 0.96f);
        private static readonly Color ButtonColor = new(0.38f, 0.29f, 0.12f, 1f);
        private static readonly Color TextColor = new(0.92f, 0.88f, 0.74f, 1f);

        [MenuItem("Tools/Talisman Bag/V0.3/BootEntryFlow01/[Writes Scene][Guard Only] Build Boot Entry Flow 01 Scene")]
        public static void BuildScene()
        {
            if (!ConfirmSceneWrite(
                    "Build Boot Entry Flow 01 Scene",
                    "This Guard Only tool rebuilds and saves:\n" +
                    BootScenePath + "\n\n" +
                    "It creates BootEntry scene roots, camera, EventSystem, and updates Build Settings.\n" +
                    "Use in Edit Mode after saving or backing up open work, with Guard or user confirmation."))
            {
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = BootSceneName;

            GameObject cameraObject = new("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.047f, 0.043f, 1f);
            camera.orthographic = true;

            GameObject bootRoot = new("V03BootEntryFlow01", typeof(V03BootEntryFlowController));
            bootRoot.transform.position = Vector3.zero;

            _ = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            EditorSceneManager.SaveScene(scene, BootScenePath);
            EnsureBuildSettings();
            AssetDatabase.SaveAssets();
            Debug.Log("[V0.3-BootEntryFlow01] BOOT_ENTRY_SCENE_BUILT path=" + BootScenePath);
        }

        [MenuItem("Tools/Talisman Bag/V0.3/BootEntryFlow01/[Writes Scene][Guard Only] Bind Boot Entry Runtime Lock Scene Nodes")]
        public static void BindRuntimeLockSceneNodes()
        {
            if (!ConfirmSceneWrite(
                    "Bind Boot Entry Runtime Lock Scene Nodes",
                    "This Guard Only tool opens and saves:\n" +
                    BootScenePath + "\n\n" +
                    "It creates or binds BootEntry canvas, pages, buttons, Main Camera, EventSystem, and controller references.\n" +
                    "Use in Edit Mode after saving or backing up open work, with Guard or user confirmation."))
            {
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(BootScenePath, OpenSceneMode.Single);
            V03BootEntryFlowController controller = Object.FindObjectOfType<V03BootEntryFlowController>(true);
            if (controller == null)
            {
                GameObject bootRoot = new("V03BootEntryFlow01", typeof(V03BootEntryFlowController));
                bootRoot.transform.position = Vector3.zero;
                controller = bootRoot.GetComponent<V03BootEntryFlowController>();
            }

            EnsureMainCamera();
            EnsureEventSystem();

            Canvas canvas = EnsureBootCanvas(controller.transform);
            GameObject loadingPage = EnsurePage(canvas.transform, "LoadingPage");
            GameObject startGamePage = EnsurePage(canvas.transform, "StartGamePage");
            GameObject openingStoryPanel = EnsurePage(canvas.transform, "OpeningStoryPanel");

            ConfigureLoadingPage(loadingPage.transform);
            Text serverLabelText = ConfigureStartGamePage(startGamePage.transform, out Button startGameButton);
            Text serverStatusText = FindRequiredText(startGamePage.transform, "ServerStatusText");
            ConfigureOpeningStoryPage(openingStoryPanel.transform, out Button skipOpeningStoryButton);

            loadingPage.SetActive(true);
            startGamePage.SetActive(false);
            openingStoryPanel.SetActive(false);

            SerializedObject serializedController = new(controller);
            serializedController.FindProperty("bootCanvas").objectReferenceValue = canvas;
            serializedController.FindProperty("loadingPage").objectReferenceValue = loadingPage;
            serializedController.FindProperty("startGamePage").objectReferenceValue = startGamePage;
            serializedController.FindProperty("openingStoryPanel").objectReferenceValue = openingStoryPanel;
            serializedController.FindProperty("startGameButton").objectReferenceValue = startGameButton;
            serializedController.FindProperty("skipOpeningStoryButton").objectReferenceValue = skipOpeningStoryButton;
            serializedController.FindProperty("serverLabelText").objectReferenceValue = serverLabelText;
            serializedController.FindProperty("serverStatusText").objectReferenceValue = serverStatusText;
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, BootScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("[V0.3-BootGuideUpgradeRuntimeLock01] BOOT_ENTRY_SCENE_NODES_BOUND path=" + BootScenePath);
        }

        private static void EnsureMainCamera()
        {
            if (Camera.main != null)
            {
                return;
            }

            GameObject cameraObject = new("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = PageBackgroundColor;
            camera.orthographic = true;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>(true) != null)
            {
                return;
            }

            _ = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private static Canvas EnsureBootCanvas(Transform controllerRoot)
        {
            Transform existing = FindDeepChild(controllerRoot, "BootEntryCanvas");
            GameObject canvasObject = existing != null
                ? existing.gameObject
                : CreateChild(controllerRoot, "BootEntryCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = EnsureComponent<Canvas>(canvasObject);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            CanvasScaler scaler = EnsureComponent<CanvasScaler>(canvasObject);
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 1f;

            _ = EnsureComponent<GraphicRaycaster>(canvasObject);
            RectTransform rect = EnsureComponent<RectTransform>(canvasObject);
            ConfigureStretch(rect);
            return canvas;
        }

        private static GameObject EnsurePage(Transform canvasRoot, string pageName)
        {
            Transform existing = FindDeepChild(canvasRoot, pageName);
            GameObject page = existing != null
                ? existing.gameObject
                : CreateChild(canvasRoot, pageName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform rect = EnsureComponent<RectTransform>(page);
            ConfigureStretch(rect);
            Image image = EnsureComponent<Image>(page);
            image.color = PageBackgroundColor;
            return page;
        }

        private static void ConfigureLoadingPage(Transform pageRoot)
        {
            Text title = EnsureText(pageRoot, "TitleText", "Talisman Bag", 54, TextAnchor.MiddleCenter);
            ConfigureRect(title.rectTransform, new Vector2(0.5f, 0.64f), new Vector2(0.5f, 0.64f), new Vector2(760f, 96f));

            Text hint = EnsureText(pageRoot, "LoadingText", "Loading local flow...", 28, TextAnchor.MiddleCenter);
            ConfigureRect(hint.rectTransform, new Vector2(0.5f, 0.54f), new Vector2(0.5f, 0.54f), new Vector2(760f, 72f));
        }

        private static Text ConfigureStartGamePage(Transform pageRoot, out Button startGameButton)
        {
            Text title = EnsureText(pageRoot, "TitleText", "Talisman Bag", 58, TextAnchor.MiddleCenter);
            ConfigureRect(title.rectTransform, new Vector2(0.5f, 0.68f), new Vector2(0.5f, 0.68f), new Vector2(760f, 104f));

            Text serverLabelText = EnsureText(pageRoot, "ServerLabelText", "Qingshipo Area 1", 30, TextAnchor.MiddleCenter);
            ConfigureRect(serverLabelText.rectTransform, new Vector2(0.5f, 0.57f), new Vector2(0.5f, 0.57f), new Vector2(760f, 70f));

            Text serverStatusText = EnsureText(pageRoot, "ServerStatusText", "Local flow demo", 24, TextAnchor.MiddleCenter);
            ConfigureRect(serverStatusText.rectTransform, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(760f, 64f));

            startGameButton = EnsureButton(pageRoot, "StartGameButton", "Start Game");
            ConfigureRect(startGameButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.39f), new Vector2(0.5f, 0.39f), new Vector2(420f, 96f));
            return serverLabelText;
        }

        private static void ConfigureOpeningStoryPage(Transform pageRoot, out Button skipOpeningStoryButton)
        {
            Image panel = EnsureImage(pageRoot, "StoryPanel");
            panel.color = PanelColor;
            ConfigureRect(panel.rectTransform, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(800f, 640f));

            Text title = EnsureText(panel.transform, "TitleText", "Opening Story", 42, TextAnchor.MiddleCenter);
            ConfigureRect(title.rectTransform, new Vector2(0.5f, 0.82f), new Vector2(0.5f, 0.82f), new Vector2(680f, 84f));

            Text body = EnsureText(
                panel.transform,
                "StoryBodyText",
                "A new talisman holder steps into the first trial.",
                28,
                TextAnchor.MiddleCenter);
            ConfigureRect(body.rectTransform, new Vector2(0.5f, 0.56f), new Vector2(0.5f, 0.56f), new Vector2(680f, 260f));

            skipOpeningStoryButton = EnsureButton(pageRoot, "SkipOpeningStoryButton", "Skip");
            ConfigureRect(skipOpeningStoryButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), new Vector2(360f, 88f));
        }

        private static Text FindRequiredText(Transform parent, string objectName)
        {
            Transform found = FindDeepChild(parent, objectName);
            return found != null ? found.GetComponent<Text>() : null;
        }

        private static Image EnsureImage(Transform parent, string objectName)
        {
            Transform existing = FindDeepChild(parent, objectName);
            GameObject imageObject = existing != null
                ? existing.gameObject
                : CreateChild(parent, objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            return EnsureComponent<Image>(imageObject);
        }

        private static Text EnsureText(Transform parent, string objectName, string text, int fontSize, TextAnchor alignment)
        {
            Transform existing = FindDeepChild(parent, objectName);
            GameObject textObject = existing != null
                ? existing.gameObject
                : CreateChild(parent, objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            Text label = EnsureComponent<Text>(textObject);
            label.text = text;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = TextColor;
            label.raycastTarget = false;
            return label;
        }

        private static Button EnsureButton(Transform parent, string objectName, string labelText)
        {
            Transform existing = FindDeepChild(parent, objectName);
            GameObject buttonObject = existing != null
                ? existing.gameObject
                : CreateChild(parent, objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            Image image = EnsureComponent<Image>(buttonObject);
            image.color = ButtonColor;

            Button button = EnsureComponent<Button>(buttonObject);
            ColorBlock colors = button.colors;
            colors.normalColor = ButtonColor;
            colors.highlightedColor = new Color(0.48f, 0.37f, 0.16f, 1f);
            colors.pressedColor = new Color(0.28f, 0.21f, 0.09f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            Text label = EnsureText(buttonObject.transform, "Label", labelText, 30, TextAnchor.MiddleCenter);
            ConfigureStretch(label.rectTransform);
            return button;
        }

        private static GameObject CreateChild(Transform parent, string objectName, params System.Type[] componentTypes)
        {
            GameObject child = new(objectName, componentTypes);
            child.transform.SetParent(parent, false);
            return child;
        }

        private static T EnsureComponent<T>(GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }

        private static void ConfigureStretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        private static void ConfigureRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 size)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = size;
            rect.localScale = Vector3.one;
        }

        private static Transform FindDeepChild(Transform parent, string objectName)
        {
            if (parent == null || string.IsNullOrWhiteSpace(objectName))
            {
                return null;
            }

            foreach (Transform child in parent)
            {
                if (child.name == objectName)
                {
                    return child;
                }

                Transform found = FindDeepChild(child, objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static void EnsureBuildSettings()
        {
            string[] requiredPaths =
            {
                BootScenePath,
                V03BootEntryFlowController.HomeScenePath,
                V03NavigationFlowController.TrialScenePath
            };

            List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes
                .Where(scene => scene != null && !requiredPaths.Contains(scene.path))
                .ToList();

            List<EditorBuildSettingsScene> orderedScenes = requiredPaths
                .Select(path => new EditorBuildSettingsScene(path, true))
                .ToList();
            orderedScenes.AddRange(scenes);
            EditorBuildSettings.scenes = orderedScenes.ToArray();
        }

        private static bool ConfirmSceneWrite(string title, string message)
        {
            return EditorUtility.DisplayDialog(
                "[Writes Scene][Guard Only] " + title,
                message,
                "Proceed",
                "Cancel");
        }
    }
}


