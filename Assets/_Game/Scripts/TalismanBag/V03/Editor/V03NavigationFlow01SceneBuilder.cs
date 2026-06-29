#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.V02.UI;
using TalismanBag.V03.MainHome;
using TalismanBag.V03.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.V03.EditorTools
{
    public static class V03NavigationFlow01SceneBuilder
    {
        public const string BootEntryScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V03_BootEntry.unity";
        public const string MainHomeScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V03_MainHome.unity";
        public const string Run15MinScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_Run15Min.unity";
        private const float BottomNavWidth = 860f;
        private const float BottomNavHeight = 124f;
        private const float BottomNavBottomInset = 56f;
        private const float BottomNavButtonStep = 168f;
        private const float BottomNavButtonWidth = 148f;
        private const float BottomNavButtonHeight = 82f;
        private const string BottomNavRootName = "BottomNavBar_Root";

        private static readonly string[] SecondaryRootNames =
        {
            "RefinePageRoot",
            "ExplorePageRoot",
            "MorePageRoot"
        };

        private static readonly string[] ObsoleteRootNames =
        {
            "SecondaryBottomNavRoot"
        };

        [MenuItem("Tools/Talisman Bag/V0.3/NavigationFlow01/[Writes Scene][Deprecated][Guard Only] Build Scene")]
        public static void BuildSceneBatch()
        {
            if (!EditorUtility.DisplayDialog(
                    "[Writes Scene][Deprecated][Guard Only] Build NavigationFlow01 Scene",
                    "This deprecated Guard Only tool opens and saves:\n" +
                    MainHomeScenePath + "\n\n" +
                    "It destroys and recreates navigation roots, writes scene references, generates UI, and may update Build Settings.\n" +
                    "Use in Edit Mode only after saving or backing up open work, with Guard or user confirmation.",
                    "Proceed",
                    "Cancel"))
            {
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(MainHomeScenePath, OpenSceneMode.Single);
            GameObject sceneRoot = FindSceneObject(scene, "MainHomeSceneRoot");
            GameObject canvasObject = FindSceneObject(scene, "Canvas");
            GameObject homeRoot = FindSceneObject(scene, "MainHomeRoot");
            GameObject safeAreaRoot = FindSceneObject(scene, "MobileSafeAreaRoot");

            Require(sceneRoot != null, "MainHomeSceneRoot is missing.");
            Require(canvasObject != null, "Canvas is missing.");
            Require(homeRoot != null, "MainHomeRoot is missing.");
            Transform contentRoot = safeAreaRoot != null ? safeAreaRoot.transform : canvasObject.transform;

            foreach (string rootName in SecondaryRootNames)
            {
                DestroyNamedObject(scene, rootName);
            }

            DestroyNamedObject(scene, BottomNavRootName);

            foreach (string rootName in ObsoleteRootNames)
            {
                DestroyNamedObject(scene, rootName);
            }

            V03NavigationFlowController existingController =
                sceneRoot.GetComponent<V03NavigationFlowController>();
            if (existingController != null)
            {
                UnityEngine.Object.DestroyImmediate(existingController);
            }

            GameObject refineRoot = CreatePageRoot(
                "RefinePageRoot",
                contentRoot,
                "养成",
                "符箓升级",
                "符桌已备好。\n当前先承接首次符箓升级，后续再扩展完整养成页。",
                new Color(0.12f, 0.16f, 0.13f, 0.99f));
            GameObject exploreRoot = CreatePageRoot(
                "ExplorePageRoot",
                contentRoot,
                "探索",
                "Coming Soon",
                "青石坡外的秘境仍在勘定。\n随机节点、三选一与天机炉将在后续版本开放。",
                new Color(0.10f, 0.13f, 0.17f, 0.99f));
            GameObject moreRoot = CreatePageRoot(
                "MorePageRoot",
                contentRoot,
                "更多",
                "本地入口集合",
                "商店　活动　公告　邮件　设置\n以上入口当前均为本地占位，不接入正式系统。",
                new Color(0.16f, 0.13f, 0.10f, 0.99f));

            GameObject bottomNavRoot = CreateBottomNav(
                contentRoot,
                out Button homeButton,
                out Button trialButton,
                out Button refineButton,
                out Button exploreButton,
                out Button moreButton);

            V03NavigationFlowController controller =
                sceneRoot.AddComponent<V03NavigationFlowController>();
            SerializedObject serializedController = new(controller);
            SetReference(serializedController, "refinePageRoot", refineRoot);
            SetReference(serializedController, "explorePageRoot", exploreRoot);
            SetReference(serializedController, "morePageRoot", moreRoot);
            SetReference(serializedController, "secondaryBottomNavRoot", bottomNavRoot);
            SetReference(serializedController, "homeButton", homeButton);
            SetReference(serializedController, "trialButton", trialButton);
            SetReference(serializedController, "refineButton", refineButton);
            SetReference(serializedController, "exploreButton", exploreButton);
            SetReference(serializedController, "moreButton", moreButton);
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            V03MainHomeSceneBootstrap bootstrap =
                sceneRoot.GetComponent<V03MainHomeSceneBootstrap>();
            Require(bootstrap != null, "V03MainHomeSceneBootstrap is missing.");

            MainHomeGreyboxPanel homePanel = homeRoot.GetComponent<MainHomeGreyboxPanel>();
            Require(homePanel != null, "MainHomeRoot is missing MainHomeGreyboxPanel.");
            homePanel.ApplyV03MainHomeUiueLayout();

            SerializedObject serializedBootstrap = new(bootstrap);
            SetReference(serializedBootstrap, "navigation", controller);
            serializedBootstrap.ApplyModifiedPropertiesWithoutUndo();

            homeRoot.SetActive(true);
            refineRoot.SetActive(false);
            exploreRoot.SetActive(false);
            moreRoot.SetActive(false);
            bottomNavRoot.SetActive(true);

            EditorSceneManager.MarkSceneDirty(scene);
            Require(EditorSceneManager.SaveScene(scene), "Could not save V03 main home scene.");

            AppendBuildSettingsScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            VerifyStaticScene();

            Debug.Log(
                "[V0.3-NavigationFlow01] NF01_SCENE_BUILD_SUCCESS " +
                "roots=RefinePageRoot,ExplorePageRoot,MorePageRoot,BottomNavBar_Root " +
                "trialMode=LoadSceneMode.Single");
        }

        [MenuItem("Tools/Talisman Bag/V0.3/NavigationFlow01/[QA Only] Verify Static")]
        public static void VerifyStaticBatch()
        {
            EditorSceneManager.OpenScene(MainHomeScenePath, OpenSceneMode.Single);
            VerifyStaticScene();
        }

        public static void VerifyStaticScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != MainHomeScenePath)
            {
                scene = EditorSceneManager.OpenScene(MainHomeScenePath, OpenSceneMode.Single);
            }

            GameObject sceneRoot = FindSceneObject(scene, "MainHomeSceneRoot");
            GameObject homeRoot = FindSceneObject(scene, "MainHomeRoot");
            Require(sceneRoot != null && homeRoot != null, "Required V03 roots are missing.");
            Require(homeRoot.activeSelf, "MainHomeRoot must be active on disk.");

            foreach (string rootName in SecondaryRootNames)
            {
                GameObject root = FindSceneObject(scene, rootName);
                Require(root != null, $"{rootName} is missing.");
                Require(!root.activeSelf, $"{rootName} must be inactive on disk.");
                Require(FindAllSceneObjects(scene, rootName).Length == 1,
                    $"Scene must contain exactly one {rootName}.");
            }

            GameObject bottomNavRoot = FindSceneObject(scene, BottomNavRootName);
            Require(bottomNavRoot != null, $"{BottomNavRootName} is missing.");
            Require(bottomNavRoot.activeSelf, $"{BottomNavRootName} must be active on disk.");
            Require(FindAllSceneObjects(scene, BottomNavRootName).Length == 1,
                $"Scene must contain exactly one {BottomNavRootName}.");

            V03NavigationFlowController controller =
                sceneRoot.GetComponent<V03NavigationFlowController>();
            Require(controller != null && controller.enabled,
                "V03 navigation controller is missing or disabled.");
            Require(sceneRoot.GetComponents<V03NavigationFlowController>().Length == 1,
                "Scene must contain exactly one V03 navigation controller.");

            V03MainHomeSceneBootstrap bootstrap =
                sceneRoot.GetComponent<V03MainHomeSceneBootstrap>();
            Require(bootstrap != null && bootstrap.enabled, "V03 bootstrap is missing.");
            SerializedObject serializedBootstrap = new(bootstrap);
            Require(
                serializedBootstrap.FindProperty("navigation")?.objectReferenceValue == controller,
                "Bootstrap navigation reference is not wired.");

            Text[] texts = scene
                .GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Text>(true))
                .ToArray();
            string[] requiredLabels =
            {
                "养成",
                "探索",
                "更多",
                "Coming Soon",
                "首页",
                "试炼"
            };
            foreach (string label in requiredLabels)
            {
                Require(texts.Any(text => text.text == label),
                    $"Navigation scene is missing label '{label}'.");
            }

            Require(texts.Any(text => text.text == "梦签"),
                "Main home scene is missing the DreamSign home hotspot label.");
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
            Require(CountMissingScripts(sceneRoot) == 0,
                "V03 scene contains Missing Script components.");
            VerifyBuildSettings();
            Require(!scene.isDirty, "Static verification must not leave scene dirty.");

            Debug.Log(
                "[V0.3-NavigationFlow01] NF01_STATIC_SUCCESS " +
                "defaultHome=true secondaryRoots=false bottomNav=true missingScripts=0");
        }

        private static GameObject CreatePageRoot(
            string objectName,
            Transform parent,
            string title,
            string badge,
            string description,
            Color color)
        {
            GameObject root = new(
                objectName,
                typeof(RectTransform),
                typeof(Image),
                typeof(Outline));
            root.transform.SetParent(parent, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, 40f);
            rect.sizeDelta = new Vector2(1000f, 1540f);
            root.GetComponent<Image>().color = color;
            Outline outline = root.GetComponent<Outline>();
            outline.effectColor = new Color(0.65f, 0.7f, 0.58f, 0.9f);
            outline.effectDistance = new Vector2(3f, -3f);

            CreateText(
                "PageTitle",
                root.transform,
                title,
                54,
                FontStyle.Bold,
                new Color(0.93f, 0.96f, 0.84f),
                new Vector2(0f, -100f),
                new Vector2(850f, 80f));
            CreateText(
                "PageBadge",
                root.transform,
                badge,
                25,
                FontStyle.Bold,
                new Color(0.89f, 0.72f, 0.38f),
                new Vector2(0f, -210f),
                new Vector2(700f, 52f));
            CreateText(
                "PageDescription",
                root.transform,
                description,
                26,
                FontStyle.Normal,
                new Color(0.86f, 0.87f, 0.81f),
                new Vector2(0f, -360f),
                new Vector2(820f, 260f));
            return root;
        }

        private static GameObject CreateBottomNav(
            Transform parent,
            out Button homeButton,
            out Button trialButton,
            out Button refineButton,
            out Button exploreButton,
            out Button moreButton)
        {
            GameObject root = new(
                BottomNavRootName,
                typeof(RectTransform),
                typeof(Image));
            root.transform.SetParent(parent, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, BottomNavBottomInset);
            rect.sizeDelta = new Vector2(BottomNavWidth, BottomNavHeight);
            Image background = root.GetComponent<Image>();
            background.color = Color.clear;
            background.raycastTarget = false;

            homeButton = CreateButton("BottomNavHomeButton", root.transform, "首页", -BottomNavButtonStep * 2f);
            refineButton = CreateButton("BottomNavRefineButton", root.transform, "养成", -BottomNavButtonStep);
            trialButton = CreateButton("BottomNavTrialButton", root.transform, "试炼", 0f);
            exploreButton = CreateButton("BottomNavExploreButton", root.transform, "探索", BottomNavButtonStep);
            moreButton = CreateButton("BottomNavMoreButton", root.transform, "更多", BottomNavButtonStep * 2f);
            return root;
        }

        private static Button CreateButton(
            string objectName,
            Transform parent,
            string label,
            float x)
        {
            GameObject buttonObject = new(
                objectName,
                typeof(RectTransform),
                typeof(Image),
                typeof(Button),
                typeof(Outline));
            buttonObject.transform.SetParent(parent, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x, 0f);
            rect.sizeDelta = new Vector2(BottomNavButtonWidth, BottomNavButtonHeight);
            buttonObject.GetComponent<Image>().color =
                new Color(0.18f, 0.26f, 0.22f, 0.98f);
            Outline outline = buttonObject.GetComponent<Outline>();
            outline.effectColor = new Color(0.45f, 0.52f, 0.43f, 0.8f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            Text labelText = CreateText(
                "Text",
                buttonObject.transform,
                label,
                24,
                FontStyle.Bold,
                Color.white,
                Vector2.zero,
                new Vector2(BottomNavButtonWidth, BottomNavButtonHeight));
            RectTransform labelRect = labelText.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            labelRect.anchoredPosition = Vector2.zero;
            labelRect.sizeDelta = Vector2.zero;
            return buttonObject.GetComponent<Button>();
        }

        private static Text CreateText(
            string objectName,
            Transform parent,
            string value,
            int fontSize,
            FontStyle style,
            Color color,
            Vector2 position,
            Vector2 size)
        {
            GameObject textObject = new(objectName, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            text.text = value;
            return text;
        }

        private static void AppendBuildSettingsScenes()
        {
            List<EditorBuildSettingsScene> scenes =
                EditorBuildSettings.scenes.ToList();
            string[] currentPaths = scenes.Select(scene => scene.path).ToArray();
            string[] expected =
            {
                BootEntryScenePath,
                MainHomeScenePath,
                V03NavigationFlowController.UpgradeScenePath,
                V03NavigationFlowController.TrialScenePath
            };
            bool hasUpgradeScene = currentPaths.SequenceEqual(expected);
            bool needsUpgradeScene =
                currentPaths.SequenceEqual(new[]
                {
                    BootEntryScenePath,
                    MainHomeScenePath,
                    V03NavigationFlowController.TrialScenePath
                });
            Require(hasUpgradeScene || needsUpgradeScene,
                "Build Settings contains an unexpected scene list; refusing to reorder or overwrite it.");

            if (hasUpgradeScene)
            {
                return;
            }

            int trialIndex = scenes.FindIndex(scene => scene.path == V03NavigationFlowController.TrialScenePath);
            Require(trialIndex >= 0, "Trial scene is missing from Build Settings.");
            scenes.Insert(
                trialIndex,
                new EditorBuildSettingsScene(V03NavigationFlowController.UpgradeScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void VerifyBuildSettings()
        {
            string[] expected =
            {
                BootEntryScenePath,
                MainHomeScenePath,
                V03NavigationFlowController.UpgradeScenePath,
                V03NavigationFlowController.TrialScenePath
            };
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            Require(scenes.Select(scene => scene.path).SequenceEqual(expected),
                "Build Settings order does not match the NavigationFlow01 contract.");
            Require(scenes.All(scene => scene.enabled),
                "All four contracted Build Settings scenes must remain enabled.");
        }

        private static void SetReference(
            SerializedObject serializedObject,
            string propertyName,
            UnityEngine.Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            Require(property != null, $"Serialized field '{propertyName}' is missing.");
            property.objectReferenceValue = value;
        }

        private static void DestroyNamedObject(Scene scene, string objectName)
        {
            foreach (GameObject target in FindAllSceneObjects(scene, objectName))
            {
                UnityEngine.Object.DestroyImmediate(target);
            }
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

        private static string[] GetBottomNavLabelsByX(IEnumerable<Text> texts)
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
