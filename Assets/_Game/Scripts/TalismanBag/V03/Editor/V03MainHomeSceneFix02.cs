#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using TalismanBag.V02.UI;
using TalismanBag.V03.MainHome;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.V03.EditorTools
{
    [InitializeOnLoad]
    public static class V03MainHomeSceneFix02
    {
        public const string ScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V03_MainHome.unity";

        private const string LegacyScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V02_FormationCounter.unity";
        private const string PlayModeSessionKey =
            "TalismanBag.V03.MainHome.Fix02.PlayModeVerification";
        private const string PlayModeExitKey =
            "TalismanBag.V03.MainHome.Fix02.ExitAfterPlayMode";
        private const string ForceEditorExitKey =
            "TalismanBag.V03.MainHome.Fix02.ForceEditorExit";
        private const string ScreenshotPath =
            "Logs/v0.3_mainhome_fix02_gameview.png";
        private const string FullBackgroundUnderlayName =
            "FullBackgroundBlackUnderlay";
        private const string FullBackgroundSlotName =
            "FullBackgroundImageSlot";
        private const string MobileSafeAreaRootName =
            "MobileSafeAreaRoot";
        private const string BottomNavRootName =
            "BottomNavBar_Root";
        private const float HomeFullBackgroundSize = 2200f;

        private static int playModeFrames;
        private static bool verificationCompleted;

        static V03MainHomeSceneFix02()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
        }

        [MenuItem("Tools/Talisman Bag/V0.3/Fix02/[Writes Scene][Deprecated][Guard Only] Build Independent Main Home Scene")]
        public static void BuildSceneBatch()
        {
            if (!EditorUtility.DisplayDialog(
                    "[Writes Scene][Deprecated][Guard Only] Build Independent Main Home Scene",
                    "This deprecated Guard Only tool rebuilds and saves:\n" +
                    ScenePath + "\n\n" +
                    "It can also open and modify legacy scene cleanup targets:\n" +
                    LegacyScenePath + "\n\n" +
                    "Use in Edit Mode only after saving or backing up open work, with Guard or user confirmation.",
                    "Proceed",
                    "Cancel"))
            {
                return;
            }

            EnsureProjectDirectories();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Scene_TalismanBag_V03_MainHome";

            GameObject sceneRoot = new("MainHomeSceneRoot");

            GameObject canvasObject = new(
                "Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(sceneRoot.transform, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            CreateFullBackgroundSlot(canvasObject.transform);

            MainHomeGreyboxPanel panel = MainHomeGreyboxPanel.CreateRuntime(canvasObject.transform);
            Require(panel != null, "Could not create MainHomeRoot.");
            panel.gameObject.name = "MainHomeRoot";
            panel.gameObject.SetActive(true);
            panel.ApplyV03MainHomeUiueLayout();

            GameObject eventSystemObject = new(
                "EventSystem",
                typeof(EventSystem),
                typeof(StandaloneInputModule));
            eventSystemObject.transform.SetParent(sceneRoot.transform, false);

            V03MainHomeSceneBootstrap bootstrap =
                sceneRoot.AddComponent<V03MainHomeSceneBootstrap>();
            SerializedObject serializedBootstrap = new(bootstrap);
            SerializedProperty homePanelProperty =
                serializedBootstrap.FindProperty("homePanel");
            Require(homePanelProperty != null, "Bootstrap homePanel field is missing.");
            homePanelProperty.objectReferenceValue = panel;
            serializedBootstrap.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            Require(
                EditorSceneManager.SaveScene(scene, ScenePath),
                $"Could not save {ScenePath}.");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            CleanupFix01LegacyScene();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            VerifyStaticScene();

            Debug.Log(
                "[V0.3-MainHomeScene01-Retry-Fix02] FIX02_SCENE_BUILT " +
                "scene=Scene_TalismanBag_V03_MainHome, defaultActive=true, " +
                "legacySceneDelivery=false");
        }

        [MenuItem("Tools/Talisman Bag/V0.3/Fix02/[QA Only] Verify Static Scene")]
        public static void VerifyStaticBatch()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            VerifyStaticScene();
        }

        public static void VerifyStaticScene()
        {
            Require(File.Exists(ScenePath), $"Scene asset is missing: {ScenePath}");

            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != ScenePath)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            GameObject sceneRoot = FindSceneObject(scene, "MainHomeSceneRoot");
            GameObject canvasObject = FindSceneObject(scene, "Canvas");
            GameObject backgroundUnderlay = FindSceneObject(scene, FullBackgroundUnderlayName);
            GameObject backgroundSlot = FindSceneObject(scene, FullBackgroundSlotName);
            GameObject homeRoot = FindSceneObject(scene, "MainHomeRoot");
            GameObject safeAreaRoot = FindSceneObject(scene, MobileSafeAreaRootName);
            GameObject bottomNavRoot = FindSceneObject(scene, BottomNavRootName);
            GameObject eventSystemObject = FindSceneObject(scene, "EventSystem");
            Transform contentRoot = safeAreaRoot != null ? safeAreaRoot.transform : canvasObject?.transform;

            Require(sceneRoot != null, "MainHomeSceneRoot is missing.");
            Require(canvasObject != null, "Canvas is missing.");
            Require(backgroundUnderlay != null, $"{FullBackgroundUnderlayName} is missing.");
            Require(backgroundSlot != null, "FullBackgroundImageSlot is missing.");
            Require(homeRoot != null, "MainHomeRoot is missing.");
            Require(bottomNavRoot != null, "BottomNavBar_Root is missing.");
            Require(eventSystemObject != null, "EventSystem is missing.");
            Require(sceneRoot.activeSelf, "MainHomeSceneRoot must be active on disk.");
            Require(canvasObject.activeSelf, "Canvas must be active on disk.");
            Require(safeAreaRoot == null || safeAreaRoot.activeSelf,
                "MobileSafeAreaRoot must be active on disk when present.");
            Require(backgroundUnderlay.activeSelf, $"{FullBackgroundUnderlayName} must be active on disk.");
            Require(backgroundSlot.activeSelf, "FullBackgroundImageSlot must be active on disk.");
            Require(homeRoot.activeSelf, "MainHomeRoot must be active on disk.");
            Require(bottomNavRoot.activeSelf, "BottomNavBar_Root must be active on disk.");
            Require(eventSystemObject.activeSelf, "EventSystem must be active on disk.");
            Require(contentRoot != null, "Main home content root is missing.");
            Require(safeAreaRoot == null || safeAreaRoot.transform.parent == canvasObject.transform,
                "MobileSafeAreaRoot must be a direct child of Canvas.");
            Require(backgroundUnderlay.transform.parent == contentRoot,
                $"{FullBackgroundUnderlayName} must be a direct child of the main home content root.");
            Require(backgroundSlot.transform.parent == contentRoot,
                "FullBackgroundImageSlot must be a direct child of the main home content root.");
            Require(homeRoot.transform.parent == contentRoot,
                "MainHomeRoot must be a direct child of the main home content root.");
            Require(bottomNavRoot.transform.parent == contentRoot,
                "BottomNavBar_Root must be a direct child of the main home content root.");
            Require(canvasObject.transform.parent == sceneRoot.transform,
                "Canvas must be a direct child of MainHomeSceneRoot.");
            Require(eventSystemObject.transform.parent == sceneRoot.transform,
                "EventSystem must be a direct child of MainHomeSceneRoot.");
            Require(backgroundUnderlay.transform.GetSiblingIndex() == 0,
                $"{FullBackgroundUnderlayName} must be the first main home content child.");
            Require(backgroundSlot.transform.GetSiblingIndex() > backgroundUnderlay.transform.GetSiblingIndex(),
                "FullBackgroundImageSlot must render above the black underlay.");

            Require(scene.GetRootGameObjects().Count(root => root.name == "MainHomeSceneRoot") == 1,
                "Scene must contain exactly one MainHomeSceneRoot.");
            Require(FindAllSceneObjects(scene, "Canvas").Length == 1,
                "Scene must contain exactly one Canvas.");
            Require(FindAllSceneObjects(scene, MobileSafeAreaRootName).Length <= 1,
                "Scene must not contain duplicate MobileSafeAreaRoot objects.");
            Require(FindAllSceneObjects(scene, "EventSystem").Length == 1,
                "Scene must contain exactly one EventSystem.");
            Require(FindAllSceneObjects(scene, FullBackgroundUnderlayName).Length == 1,
                $"Scene must contain exactly one {FullBackgroundUnderlayName}.");
            Require(FindAllSceneObjects(scene, FullBackgroundSlotName).Length == 1,
                "Scene must contain exactly one FullBackgroundImageSlot.");
            Require(FindAllSceneObjects(scene, "MainHomeRoot").Length == 1,
                "Scene must contain exactly one MainHomeRoot.");
            Require(FindAllSceneObjects(scene, BottomNavRootName).Length == 1,
                "Scene must contain exactly one BottomNavBar_Root.");

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            RectTransform underlayRect = backgroundUnderlay.GetComponent<RectTransform>();
            Image underlayImage = backgroundUnderlay.GetComponent<Image>();
            RectTransform backgroundRect = backgroundSlot.GetComponent<RectTransform>();
            Image backgroundImage = backgroundSlot.GetComponent<Image>();
            RectTransform bottomNavRect = bottomNavRoot.GetComponent<RectTransform>();
            Image bottomNavImage = bottomNavRoot.GetComponent<Image>();
            MainHomeGreyboxPanel panel = homeRoot.GetComponent<MainHomeGreyboxPanel>();
            Image homeRootImage = homeRoot.GetComponent<Image>();
            Outline homeRootOutline = homeRoot.GetComponent<Outline>();
            V03MainHomeSceneBootstrap bootstrap =
                sceneRoot.GetComponent<V03MainHomeSceneBootstrap>();
            Require(canvas != null && canvas.enabled, "Canvas must exist and be enabled.");
            Require(underlayRect != null, $"{FullBackgroundUnderlayName} must have a RectTransform.");
            Require(underlayRect.anchorMin == Vector2.zero && underlayRect.anchorMax == Vector2.one,
                $"{FullBackgroundUnderlayName} must stretch full screen.");
            Require(underlayRect.sizeDelta == Vector2.zero,
                $"{FullBackgroundUnderlayName} must not leave screen gutters.");
            Require(underlayImage != null && underlayImage.enabled,
                $"{FullBackgroundUnderlayName} must have an enabled Image.");
            Require(underlayImage.color == Color.black,
                $"{FullBackgroundUnderlayName} must be black.");
            Require(!underlayImage.raycastTarget,
                $"{FullBackgroundUnderlayName} must not block input.");
            Require(backgroundRect != null, "FullBackgroundImageSlot must have a RectTransform.");
            Require(backgroundImage != null && backgroundImage.enabled,
                "FullBackgroundImageSlot must have an enabled Image.");
            Require(!backgroundImage.raycastTarget,
                "FullBackgroundImageSlot must not block input.");
            Require(panel != null && panel.enabled, "MainHomeRoot must contain an enabled home panel.");
            Require(homeRootImage == null || !homeRootImage.enabled,
                "MainHomeRoot Image must stay disabled.");
            Require(homeRootOutline == null || !homeRootOutline.enabled,
                "MainHomeRoot Outline must stay disabled.");
            Require(bottomNavRect != null, "BottomNavBar_Root must have a RectTransform.");
            Require(bottomNavImage == null || !bottomNavImage.raycastTarget,
                "BottomNavBar_Root background must not block input.");
            Require(bootstrap != null && bootstrap.enabled,
                "MainHomeSceneRoot must contain an enabled lifecycle bootstrap.");

            string[] requiredTexts =
            {
                "照灯小铺",
                "照灯账本",
                "道藏典册",
                "旧物线索簿",
                "梦签",
                "小满",
                "后屋",
                "青石坊街口",
                "活动",
                "邮件",
                "商店",
                "公告",
                "设置"
            };
            Text[] texts = homeRoot.GetComponentsInChildren<Text>(true);
            bool hasSerializedUiueTexts = requiredTexts.All(requiredText =>
                texts.Any(text => text.text == requiredText));
            if (!hasSerializedUiueTexts)
            {
                Debug.Log(
                    "[V0.3-MainHomeScene01-Retry-Fix02] " +
                    "Static scene relies on runtime V03 UIUE layout hook until Builder is rerun.");
            }

            Text[] sceneTexts = scene
                .GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Text>(true))
                .ToArray();
            Text[] bottomNavTexts = bottomNavRoot.GetComponentsInChildren<Text>(true);
            string[] expectedBottomNavLabels = { "首页", "养成", "试炼", "探索", "更多" };
            Require(
                GetBottomNavLabelsByX(bottomNavTexts).SequenceEqual(expectedBottomNavLabels),
                "Bottom nav order must be 首页 / 养成 / 试炼 / 探索 / 更多.");
            Require(!bottomNavTexts.Any(text =>
                    text.text.Contains("BattleBackpack") ||
                    text.text.Contains("背包") ||
                    text.text.Contains("梦签")),
                "Bottom navigation exposes a forbidden DreamSign or backpack label.");
            Require(!sceneTexts.Any(text => text.text.Contains("暂时收起")),
                "Main home scene must not restore the temporary collapse button.");
            Require(!sceneTexts.Any(text => text.text.Contains("BattleBackpack")),
                "Main home scene exposes a forbidden BattleBackpack label.");
            Require(!sceneTexts.Any(text => text.text == "论道"),
                "Main home scene exposes the forbidden PVP name 论道.");
            Require(CountMissingScripts(sceneRoot) == 0,
                "Main home scene contains Missing Script components.");
            Require(!scene.isDirty,
                "Static verification must not leave the scene dirty.");

            Debug.Log(
                "[V0.3-MainHomeScene01-Retry-Fix02] FIX02_SCENE_STATIC_SUCCESS " +
                "scene=Scene_TalismanBag_V03_MainHome, hierarchy=unique, " +
                "defaultActive=true, missingScripts=0");
        }

        [MenuItem("Tools/Talisman Bag/V0.3/Fix02/[QA Only] Verify PlayMode First Frame")]
        public static void VerifyPlayModeBatch()
        {
            SessionState.SetBool(ForceEditorExitKey, false);
            BeginPlayModeVerification();
        }

        public static void VerifyPlayModeAndExit()
        {
            SessionState.SetBool(ForceEditorExitKey, true);
            BeginPlayModeVerification();
        }

        private static void BeginPlayModeVerification()
        {
            Require(!EditorApplication.isPlaying, "PlayMode verification is already running.");
            Require(File.Exists(ScenePath), $"Scene asset is missing: {ScenePath}");

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            VerifyStaticScene();

            playModeFrames = 0;
            verificationCompleted = false;
            SessionState.SetBool(PlayModeSessionKey, true);
            SessionState.SetBool(PlayModeExitKey, false);
            EditorApplication.isPlaying = true;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(PlayModeSessionKey, false))
            {
                if (state == PlayModeStateChange.EnteredEditMode &&
                    SessionState.GetBool(PlayModeExitKey, false))
                {
                    SessionState.EraseBool(PlayModeExitKey);
                    bool forceEditorExit =
                        SessionState.GetBool(ForceEditorExitKey, false);
                    SessionState.EraseBool(ForceEditorExitKey);
                    if (Application.isBatchMode || forceEditorExit)
                    {
                        EditorApplication.Exit(0);
                    }
                }

                return;
            }

            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                playModeFrames = 0;
                verificationCompleted = false;
            }
        }

        private static void OnEditorUpdate()
        {
            if (!SessionState.GetBool(PlayModeSessionKey, false) ||
                !EditorApplication.isPlaying)
            {
                return;
            }

            playModeFrames++;
            if (!verificationCompleted && playModeFrames >= 2)
            {
                try
                {
                    VerifyRuntimeFirstFrame();
                    CaptureGameViewEvidence();
                    verificationCompleted = true;
                }
                catch (Exception exception)
                {
                    SessionState.EraseBool(PlayModeSessionKey);
                    Debug.LogException(exception);
                    if (Application.isBatchMode)
                    {
                        EditorApplication.Exit(1);
                    }

                    throw;
                }
            }

            if (!verificationCompleted || playModeFrames < 90)
            {
                return;
            }

            SessionState.EraseBool(PlayModeSessionKey);
            SessionState.SetBool(
                PlayModeExitKey,
                Application.isBatchMode ||
                SessionState.GetBool(ForceEditorExitKey, false));
            EditorApplication.isPlaying = false;
        }

        private static void VerifyRuntimeFirstFrame()
        {
            Scene scene = SceneManager.GetActiveScene();
            Require(scene.path == ScenePath,
                "PlayMode verification loaded the wrong scene.");

            GameObject homeRoot = FindSceneObject(scene, "MainHomeRoot");
            GameObject canvasObject = FindSceneObject(scene, "Canvas");
            GameObject backgroundUnderlay = FindSceneObject(scene, FullBackgroundUnderlayName);
            GameObject backgroundSlot = FindSceneObject(scene, FullBackgroundSlotName);
            GameObject bottomNavRoot = FindSceneObject(scene, BottomNavRootName);
            Require(homeRoot != null, "PlayMode MainHomeRoot is missing.");
            Require(canvasObject != null, "PlayMode Canvas is missing.");
            Require(backgroundUnderlay != null, "PlayMode FullBackgroundBlackUnderlay is missing.");
            Require(backgroundSlot != null, "PlayMode FullBackgroundImageSlot is missing.");
            Require(bottomNavRoot != null, "PlayMode BottomNavBar_Root is missing.");
            Require(FindAllSceneObjects(scene, FullBackgroundUnderlayName).Length == 1,
                "PlayMode must contain exactly one FullBackgroundBlackUnderlay.");
            Require(FindAllSceneObjects(scene, FullBackgroundSlotName).Length == 1,
                "PlayMode must contain exactly one FullBackgroundImageSlot.");
            Require(FindAllSceneObjects(scene, BottomNavRootName).Length == 1,
                "PlayMode must contain exactly one BottomNavBar_Root.");
            Require(homeRoot.activeSelf,
                "PlayMode MainHomeRoot activeSelf must remain true.");
            Require(homeRoot.activeInHierarchy,
                "PlayMode MainHomeRoot must be visible in hierarchy.");
            Require(backgroundUnderlay.activeInHierarchy,
                "PlayMode black underlay must remain visible in hierarchy.");
            Require(backgroundSlot.activeInHierarchy,
                "PlayMode full background slot must remain visible in hierarchy.");
            Require(bottomNavRoot.activeInHierarchy,
                "PlayMode bottom navigation must remain visible in hierarchy.");

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            MainHomeGreyboxPanel panel = homeRoot.GetComponent<MainHomeGreyboxPanel>();
            Image underlayImage = backgroundUnderlay.GetComponent<Image>();
            Image backgroundImage = backgroundSlot.GetComponent<Image>();
            Image homeRootImage = homeRoot.GetComponent<Image>();
            Outline homeRootOutline = homeRoot.GetComponent<Outline>();
            Require(canvas != null && canvas.enabled && canvas.gameObject.activeInHierarchy,
                "PlayMode Canvas must be active and enabled.");
            Require(panel != null && panel.enabled,
                "PlayMode home panel must be enabled.");
            Require(underlayImage != null && underlayImage.enabled && !underlayImage.raycastTarget,
                "PlayMode black underlay Image state was rewritten.");
            Require(backgroundImage != null && backgroundImage.enabled && !backgroundImage.raycastTarget,
                "PlayMode full background Image state was rewritten.");
            Require(homeRootImage == null || !homeRootImage.enabled,
                "PlayMode MainHomeRoot Image must stay disabled.");
            Require(homeRootOutline == null || !homeRootOutline.enabled,
                "PlayMode MainHomeRoot Outline must stay disabled.");

            string[] requiredTexts =
            {
                "照灯小铺",
                "照灯账本",
                "道藏典册",
                "旧物线索簿",
                "梦签",
                "小满",
                "后屋",
                "青石坊街口",
                "活动",
                "邮件",
                "商店",
                "公告",
                "设置"
            };
            Text[] texts = homeRoot.GetComponentsInChildren<Text>(true);
            foreach (string requiredText in requiredTexts)
            {
                Text text = texts.FirstOrDefault(candidate => candidate.text == requiredText);
                Require(text != null && text.enabled && text.gameObject.activeInHierarchy,
                    $"PlayMode text '{requiredText}' is not visible.");
            }

            Text[] sceneTexts = scene
                .GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Text>(true))
                .ToArray();
            string[] expectedBottomNavLabels = { "首页", "养成", "试炼", "探索", "更多" };
            Require(
                GetBottomNavLabelsByX(bottomNavRoot.GetComponentsInChildren<Text>(true))
                    .SequenceEqual(expectedBottomNavLabels),
                "PlayMode bottom nav order must be 首页 / 养成 / 试炼 / 探索 / 更多.");
            Require(!sceneTexts.Any(text =>
                    text.text.Contains("暂时收起") ||
                    text.text.Contains("BattleBackpack") ||
                    text.text.Contains("背包")),
                "PlayMode restored a forbidden temporary collapse or backpack label.");

            Debug.Log(
                "[V0.3-MainHomeScene01-Retry-Fix02] " +
                "FIX02_PLAYMODE_FIRST_FRAME_SUCCESS " +
                "scene=Scene_TalismanBag_V03_MainHome, firstFrameVisible=true");
            Debug.Log(
                "[V0.3-MainHomeScene01-Retry-Fix02] FIX02_SMOKE_SUCCESS " +
                "scene=Scene_TalismanBag_V03_MainHome, " +
                "firstFrameVisible=true, reflection=false");
        }

        private static GameObject CreateFullBackgroundSlot(Transform parent)
        {
            GameObject underlayObject = new(
                FullBackgroundUnderlayName,
                typeof(RectTransform),
                typeof(Image));
            underlayObject.transform.SetParent(parent, false);
            underlayObject.transform.SetAsFirstSibling();

            RectTransform underlayRect = underlayObject.GetComponent<RectTransform>();
            underlayRect.anchorMin = Vector2.zero;
            underlayRect.anchorMax = Vector2.one;
            underlayRect.pivot = new Vector2(0.5f, 0.5f);
            underlayRect.anchoredPosition = Vector2.zero;
            underlayRect.sizeDelta = Vector2.zero;

            Image underlayImage = underlayObject.GetComponent<Image>();
            underlayImage.color = Color.black;
            underlayImage.raycastTarget = false;

            GameObject slotObject = new(
                FullBackgroundSlotName,
                typeof(RectTransform),
                typeof(Image));
            slotObject.transform.SetParent(parent, false);
            slotObject.transform.SetSiblingIndex(1);

            RectTransform rect = slotObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(HomeFullBackgroundSize, HomeFullBackgroundSize);

            Image image = slotObject.GetComponent<Image>();
            image.color = Color.clear;
            image.raycastTarget = false;
            return slotObject;
        }

        private static void CaptureGameViewEvidence()
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            Require(!string.IsNullOrWhiteSpace(projectRoot),
                "Could not resolve project root for screenshot.");
            string absolutePath = Path.Combine(projectRoot, ScreenshotPath);
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath) ?? projectRoot);
            ScreenCapture.CaptureScreenshot(absolutePath);
            Debug.Log(
                $"[V0.3-MainHomeScene01-Retry-Fix02] GAME_VIEW_CAPTURE_REQUESTED path={absolutePath}");
        }

        private static void CleanupFix01LegacyScene()
        {
            if (!File.Exists(LegacyScenePath))
            {
                return;
            }

            Scene legacyScene =
                EditorSceneManager.OpenScene(LegacyScenePath, OpenSceneMode.Single);
            MainHomeGreyboxPanel legacyPanel = Resources
                .FindObjectsOfTypeAll<MainHomeGreyboxPanel>()
                .FirstOrDefault(candidate =>
                    candidate != null &&
                    candidate.gameObject.scene == legacyScene &&
                    candidate.gameObject.name == "MainHomeGreyboxPanel_Scene");

            if (legacyPanel == null)
            {
                return;
            }

            foreach (MonoBehaviour behaviour in legacyScene
                         .GetRootGameObjects()
                         .SelectMany(root => root.GetComponentsInChildren<MonoBehaviour>(true)))
            {
                if (behaviour == null)
                {
                    continue;
                }

                SerializedObject serializedBehaviour = new(behaviour);
                SerializedProperty homePanelProperty =
                    serializedBehaviour.FindProperty("homeGreyboxPanel");
                if (homePanelProperty?.objectReferenceValue != legacyPanel)
                {
                    continue;
                }

                homePanelProperty.objectReferenceValue = null;
                serializedBehaviour.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(behaviour);
            }

            UnityEngine.Object.DestroyImmediate(legacyPanel.gameObject);
            EditorSceneManager.MarkSceneDirty(legacyScene);
            Require(
                EditorSceneManager.SaveScene(legacyScene),
                "Could not save precise Fix01 cleanup in legacy scene.");
            Debug.Log(
                "[V0.3-MainHomeScene01-Retry-Fix02] FIX01_LEGACY_SCENE_CLEANED " +
                "removed=MainHomeGreyboxPanel_Scene");
        }

        private static void EnsureProjectDirectories()
        {
            Directory.CreateDirectory("Assets/_Game/Scenes");
            Directory.CreateDirectory("Logs");
            AssetDatabase.Refresh();
        }

        private static GameObject FindSceneObject(Scene scene, string objectName)
        {
            return FindAllSceneObjects(scene, objectName).FirstOrDefault();
        }

        private static GameObject[] FindAllSceneObjects(Scene scene, string objectName)
        {
            return scene
                .GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Where(transform => transform.name == objectName)
                .Select(transform => transform.gameObject)
                .ToArray();
        }

        private static int CountMissingScripts(GameObject root)
        {
            return root
                .GetComponentsInChildren<Transform>(true)
                .Sum(transform =>
                    GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                        transform.gameObject));
        }

        private static string[] GetBottomNavLabelsByX(Text[] texts)
        {
            return texts
                .Where(text => text != null)
                .OrderBy(text =>
                {
                    RectTransform parentRect = text.transform.parent as RectTransform;
                    return parentRect != null ? parentRect.anchoredPosition.x : 0f;
                })
                .Select(text => text.text)
                .ToArray();
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
#endif
