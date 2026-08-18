#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TalismanBag.Navigation;
using TalismanBag.V04.WorldMap;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.Editor.V04.WorldMap
{
    public static class V04WorldMapSceneAuthoring
    {
        private const string MenuRoot =
            "Tools/TalismanBag/V0.4/World Map/Create New Qingshifang World Map Scene";
        private const string NavigationPatchMenu =
            "Tools/TalismanBag/V0.4/World Map/[Writes Scene][Manual Only] Add Main Navigation";
        private static readonly Color PageBackground = new(0.065f, 0.07f, 0.075f, 1f);
        private static readonly Color PanelBackground = new(0.12f, 0.115f, 0.105f, 0.96f);
        private static readonly Color WarmPanel = new(0.22f, 0.18f, 0.13f, 0.96f);
        private static readonly Color Accent = new(0.58f, 0.25f, 0.18f, 1f);
        private static readonly Color PaleText = new(0.9f, 0.86f, 0.74f, 1f);
        private static readonly Color MutedText = new(0.65f, 0.64f, 0.58f, 1f);
        public static void CreateFromMenu()
        {
            if (Application.isPlaying)
            {
                Debug.LogError(
                    "[V0.4-WorldMapAuthoring] Scene authoring is disabled in Play Mode.");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning(
                    "[V0.4-WorldMapAuthoring] Cancelled because current modified scenes were not saved.");
                return;
            }

            CreateNewScene();
        }

        public static void RunBatch()
        {
            try
            {
                CreateNewScene();
                Debug.Log(
                    "WORLD_MAP_QINGSHIFANG_SCENE_AUTHORING_PASS scene=" +
                    V04WorldMapSceneController.ScenePath);
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.Log("WORLD_MAP_QINGSHIFANG_SCENE_AUTHORING_FAIL");
                EditorApplication.Exit(1);
            }
        }

        public static void RebuildInvalidGeneratedSceneBatch()
        {
            try
            {
                if (!File.Exists(V04WorldMapSceneController.ScenePath))
                {
                    throw new InvalidOperationException(
                        "WORLD_MAP_SCENE_REBUILD_SOURCE_MISSING path=" +
                        V04WorldMapSceneController.ScenePath);
                }

                if (!AssetDatabase.DeleteAsset(V04WorldMapSceneController.ScenePath))
                {
                    throw new InvalidOperationException(
                        "WORLD_MAP_SCENE_REBUILD_DELETE_FAILED path=" +
                        V04WorldMapSceneController.ScenePath);
                }

                CreateNewScene();
                Debug.Log(
                    "WORLD_MAP_QINGSHIFANG_SCENE_REBUILD_PASS scene=" +
                    V04WorldMapSceneController.ScenePath);
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.Log("WORLD_MAP_QINGSHIFANG_SCENE_REBUILD_FAIL");
                EditorApplication.Exit(1);
            }
        }

        private static void CreateNewScene()
        {
            if (Application.isPlaying)
            {
                throw new InvalidOperationException(
                    "WORLD_MAP_SCENE_AUTHORING_PLAY_MODE_REJECTED");
            }

            if (File.Exists(V04WorldMapSceneController.ScenePath))
            {
                throw new InvalidOperationException(
                    "WORLD_MAP_SCENE_ALREADY_EXISTS_REFUSE_OVERWRITE path=" +
                    V04WorldMapSceneController.ScenePath);
            }

            string directory = Path.GetDirectoryName(V04WorldMapSceneController.ScenePath);
            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new InvalidOperationException("WORLD_MAP_SCENE_DIRECTORY_INVALID");
            }

            Directory.CreateDirectory(directory);
            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);
            scene.name = V04WorldMapSceneController.SceneName;

            CreateEventSystem();
            Canvas canvas = CreateCanvas();
            RectTransform safeArea = CreateRect(
                "WorldMapSafeAreaRoot",
                canvas.transform,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);

            Image globalBackground = CreateImage(
                "WorldMapBackgroundImageSlot",
                safeArea,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero,
                PageBackground,
                false);
            globalBackground.preserveAspect = true;

            RectTransform commonShell = CreateRect(
                "CommonNavigationShell",
                safeArea,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);
            Image topBar = CreateImage(
                "TopBar",
                commonShell,
                new Vector2(0f, 1f),
                Vector2.one,
                new Vector2(0f, -72f),
                new Vector2(0f, 144f),
                PanelBackground,
                true);
            Button backButton = CreateButton(
                "BackButton",
                topBar.rectTransform,
                "返回",
                new Vector2(82f, -72f),
                new Vector2(130f, 76f),
                Accent);
            Button upgradeNavigationButton = CreateButton(
                "WorldMapUpgradeButton",
                topBar.rectTransform,
                "符箓升级",
                new Vector2(410f, -72f),
                new Vector2(190f, 76f),
                Accent);
            Text breadcrumbText = CreateText(
                "BreadcrumbText",
                topBar.rectTransform,
                "世界舆图",
                new Vector2(0f, -53f),
                new Vector2(690f, 54f),
                30,
                TextAnchor.MiddleCenter,
                PaleText);
            Text resourceShellText = CreateText(
                "ResourceShellText",
                topBar.rectTransform,
                "资源栏：RESOURCE_OWNER_UNBOUND",
                new Vector2(0f, -106f),
                new Vector2(850f, 38f),
                18,
                TextAnchor.MiddleCenter,
                MutedText);
            Text pageDiagnosticText = CreateText(
                "PageDiagnosticText",
                commonShell,
                string.Empty,
                new Vector2(0f, 55f),
                new Vector2(980f, 92f),
                18,
                TextAnchor.MiddleCenter,
                MutedText);

            GameObject worldRegionView = CreateWorldRegionView(
                safeArea,
                out Button qingshifangRegionButton);
            GameObject qingshifangRegionView = CreateRegionView(
                safeArea,
                out V04WorldMapChapterEntryView[] chapterViews);
            GameObject chapterStageMapView = CreateChapterStageMapView(
                safeArea,
                out V04WorldMapStageNodeView[] stageNodeViews);
            V04WorldMapStageDetailDrawerView detailDrawer =
                CreateStageDetailDrawer(safeArea);

            GameObject controllerObject = new("V04WorldMapSceneController");
            V04WorldMapSceneController controller =
                controllerObject.AddComponent<V04WorldMapSceneController>();
            controller.ConfigureSceneBindings(
                worldRegionView,
                qingshifangRegionView,
                chapterStageMapView,
                detailDrawer,
                backButton,
                upgradeNavigationButton,
                qingshifangRegionButton,
                breadcrumbText,
                resourceShellText,
                pageDiagnosticText,
                chapterViews,
                stageNodeViews);

            worldRegionView.SetActive(true);
            qingshifangRegionView.SetActive(false);
            chapterStageMapView.SetActive(false);
            detailDrawer.gameObject.SetActive(false);
            commonShell.SetAsLastSibling();
            detailDrawer.transform.SetAsLastSibling();

            if (!EditorSceneManager.SaveScene(
                    scene,
                    V04WorldMapSceneController.ScenePath,
                    false))
            {
                throw new InvalidOperationException(
                    "WORLD_MAP_SCENE_SAVE_FAILED path=" +
                    V04WorldMapSceneController.ScenePath);
            }

            AssetDatabase.Refresh();
            Debug.Log(
                "[V0.4-WorldMapAuthoring] Created dedicated authored scene: " +
                V04WorldMapSceneController.ScenePath);
        }
        public static void ApplyNavigationPatchFromMenu()
        {
            if (Application.isPlaying)
            {
                Debug.LogError(
                    "[MainSceneNavigationAuthoring] Scene authoring is disabled in Play Mode.");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning(
                    "[MainSceneNavigationAuthoring] Cancelled because current modified scenes were not saved.");
                return;
            }

            ApplyNavigationPatch();
            AppendWorldMapToBuildSettings();
        }

        public static void ApplyNavigationPatchBatch()
        {
            try
            {
                ApplyNavigationPatch();
                AppendWorldMapToBuildSettings();
                Debug.Log(
                    "MAIN_SCENE_NAVIGATION_WORLDMAP_AUTHORING_PASS scene=" +
                    V04WorldMapSceneController.ScenePath +
                    " buildSettings=stable-insert");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.Log("MAIN_SCENE_NAVIGATION_WORLDMAP_AUTHORING_FAIL");
                EditorApplication.Exit(1);
            }
        }

        private static void AppendWorldMapToBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes =
                EditorBuildSettings.scenes.ToList();
            int[] matchingIndices = scenes
                .Select((scene, index) => new { scene, index })
                .Where(row =>
                    row.scene.path == V04WorldMapSceneController.ScenePath)
                .Select(row => row.index)
                .ToArray();
            if (matchingIndices.Length > 1)
            {
                throw new InvalidOperationException(
                    "MAIN_SCENE_NAVIGATION_WORLDMAP_BUILD_DUPLICATE");
            }

            if (matchingIndices.Length == 1)
            {
                int index = matchingIndices[0];
                if (!scenes[index].enabled)
                {
                    scenes[index] = new EditorBuildSettingsScene(
                        V04WorldMapSceneController.ScenePath,
                        true);
                    EditorBuildSettings.scenes = scenes.ToArray();
                }

                return;
            }

            int mainHomeIndex = scenes.FindIndex(scene =>
                scene.path == TalismanSceneNavigationOwner.MainHomeScenePath);
            if (mainHomeIndex < 0)
            {
                throw new InvalidOperationException(
                    "MAIN_SCENE_NAVIGATION_MAINHOME_BUILD_ENTRY_MISSING");
            }

            scenes.Insert(mainHomeIndex + 1, new EditorBuildSettingsScene(
                V04WorldMapSceneController.ScenePath,
                true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void ApplyNavigationPatch()
        {
            if (Application.isPlaying)
            {
                throw new InvalidOperationException(
                    "MAIN_SCENE_NAVIGATION_AUTHORING_PLAY_MODE_REJECTED");
            }

            if (!File.Exists(V04WorldMapSceneController.ScenePath))
            {
                throw new InvalidOperationException(
                    "MAIN_SCENE_NAVIGATION_WORLDMAP_MISSING path=" +
                    V04WorldMapSceneController.ScenePath);
            }

            Scene scene = EditorSceneManager.OpenScene(
                V04WorldMapSceneController.ScenePath,
                OpenSceneMode.Single);
            V04WorldMapSceneController controller =
                FindSceneComponent<V04WorldMapSceneController>(scene);
            if (controller == null)
            {
                throw new InvalidOperationException(
                    "MAIN_SCENE_NAVIGATION_WORLDMAP_CONTROLLER_MISSING");
            }

            Transform topBar = FindSceneTransform(scene, "TopBar");
            if (topBar == null)
            {
                throw new InvalidOperationException(
                    "MAIN_SCENE_NAVIGATION_WORLDMAP_TOPBAR_MISSING");
            }

            Transform existing = FindSceneTransform(scene, "WorldMapUpgradeButton");
            Button upgradeNavigationButton;
            if (existing != null)
            {
                upgradeNavigationButton = existing.GetComponent<Button>();
                if (upgradeNavigationButton == null)
                {
                    throw new InvalidOperationException(
                        "MAIN_SCENE_NAVIGATION_WORLDMAP_UPGRADE_BUTTON_COMPONENT_MISSING");
                }
            }
            else
            {
                upgradeNavigationButton = CreateButton(
                    "WorldMapUpgradeButton",
                    topBar,
                    "符箓升级",
                    new Vector2(410f, -72f),
                    new Vector2(190f, 76f),
                    Accent);
            }

            controller.ConfigureUpgradeNavigationButton(upgradeNavigationButton);
            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(
                    scene,
                    V04WorldMapSceneController.ScenePath,
                    false))
            {
                throw new InvalidOperationException(
                    "MAIN_SCENE_NAVIGATION_WORLDMAP_SAVE_FAILED");
            }
        }

        private static T FindSceneComponent<T>(Scene scene)
            where T : Component
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                T component = root.GetComponentInChildren<T>(true);
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }

        private static Transform FindSceneTransform(Scene scene, string objectName)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Transform found = FindDeepChild(root.transform, objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static Transform FindDeepChild(Transform root, string objectName)
        {
            if (root.name == objectName)
            {
                return root;
            }

            foreach (Transform child in root)
            {
                Transform found = FindDeepChild(child, objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static GameObject CreateWorldRegionView(
            Transform parent,
            out Button regionButton)
        {
            RectTransform root = CreatePageRoot("WorldRegionView", parent);
            Image backgroundSlot = CreateImage(
                "WorldRegionBackgroundImageSlot",
                root,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero,
                new Color(0.09f, 0.105f, 0.105f, 1f),
                false);
            backgroundSlot.preserveAspect = true;
            CreateText(
                "WorldTitleText",
                root,
                V04WorldMapCatalog.World.displayName,
                new Vector2(0f, 570f),
                new Vector2(860f, 100f),
                42,
                TextAnchor.MiddleCenter,
                PaleText);
            CreateText(
                "WorldHintText",
                root,
                "固定世界舆图 · 选择版本区域",
                new Vector2(0f, 500f),
                new Vector2(760f, 52f),
                23,
                TextAnchor.MiddleCenter,
                MutedText);
            regionButton = CreateButton(
                "QingshifangRegionHotspotButton",
                root,
                "青石坊《骨相》\n首版本区域",
                new Vector2(0f, 80f),
                new Vector2(520f, 300f),
                WarmPanel);
            CreateImage(
                "QingshifangRegionImageSlot",
                regionButton.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 55f),
                new Vector2(420f, 150f),
                new Color(0.19f, 0.19f, 0.17f, 1f),
                false).preserveAspect = true;
            return root.gameObject;
        }

        private static GameObject CreateRegionView(
            Transform parent,
            out V04WorldMapChapterEntryView[] chapterViews)
        {
            RectTransform root = CreatePageRoot("QingshifangRegionView", parent);
            Image backgroundSlot = CreateImage(
                "QingshifangRegionBackgroundImageSlot",
                root,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero,
                new Color(0.105f, 0.09f, 0.075f, 1f),
                false);
            backgroundSlot.preserveAspect = true;
            CreateText(
                "RegionTitleText",
                root,
                "青石坊《骨相》",
                new Vector2(0f, 590f),
                new Vector2(880f, 80f),
                40,
                TextAnchor.MiddleCenter,
                PaleText);

            chapterViews = new V04WorldMapChapterEntryView[4];
            for (int index = 0; index < chapterViews.Length; index++)
            {
                float y = 390f - index * 245f;
                Button button = CreateButton(
                    "ChapterEntry_" + (index + 1),
                    root,
                    string.Empty,
                    new Vector2(0f, y),
                    new Vector2(900f, 205f),
                    index == 0 ? WarmPanel : PanelBackground);
                Text title = CreateText(
                    "TitleText",
                    button.transform,
                    "Chapter",
                    new Vector2(-15f, 52f),
                    new Vector2(790f, 52f),
                    30,
                    TextAnchor.MiddleLeft,
                    PaleText);
                Text subtitle = CreateText(
                    "SubtitleText",
                    button.transform,
                    string.Empty,
                    new Vector2(-15f, 0f),
                    new Vector2(790f, 64f),
                    20,
                    TextAnchor.MiddleLeft,
                    MutedText);
                Text availability = CreateText(
                    "AvailabilityText",
                    button.transform,
                    string.Empty,
                    new Vector2(-15f, -62f),
                    new Vector2(790f, 38f),
                    18,
                    TextAnchor.MiddleRight,
                    index == 0 ? PaleText : MutedText);
                V04WorldMapChapterEntryView view =
                    button.gameObject.AddComponent<V04WorldMapChapterEntryView>();
                view.Configure(button, title, subtitle, availability);
                chapterViews[index] = view;
            }

            return root.gameObject;
        }

        private static GameObject CreateChapterStageMapView(
            Transform parent,
            out V04WorldMapStageNodeView[] stageNodeViews)
        {
            RectTransform root = CreatePageRoot("ChapterStageMapView", parent);
            Image backgroundSlot = CreateImage(
                "ChapterStageMapBackgroundImageSlot",
                root,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero,
                new Color(0.085f, 0.09f, 0.08f, 1f),
                false);
            backgroundSlot.preserveAspect = true;
            CreateText(
                "ChapterTitleText",
                root,
                "第一章·骨瓷与旧街",
                new Vector2(0f, 590f),
                new Vector2(880f, 70f),
                38,
                TextAnchor.MiddleCenter,
                PaleText);
            CreateText(
                "ChapterMapHintText",
                root,
                "1-1 至 1-10 · 点击节点查看详情",
                new Vector2(0f, 535f),
                new Vector2(800f, 44f),
                20,
                TextAnchor.MiddleCenter,
                MutedText);

            stageNodeViews = new V04WorldMapStageNodeView[10];
            for (int index = 0; index < stageNodeViews.Length; index++)
            {
                int row = index / 2;
                bool leftToRight = row % 2 == 0;
                bool leftColumn = index % 2 == 0 ? leftToRight : !leftToRight;
                float x = leftColumn ? -245f : 245f;
                float y = 390f - row * 240f;
                Button button = CreateButton(
                    "StageNode_" + (index + 1),
                    root,
                    string.Empty,
                    new Vector2(x, y),
                    new Vector2(260f, 160f),
                    PanelBackground);
                Text stageLabel = CreateText(
                    "StageLabelText",
                    button.transform,
                    "1-" + (index + 1),
                    new Vector2(0f, 28f),
                    new Vector2(210f, 52f),
                    31,
                    TextAnchor.MiddleCenter,
                    PaleText);
                Image stateBadge = CreateImage(
                    "StateBadge",
                    button.transform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, -47f),
                    new Vector2(180f, 38f),
                    new Color(0.35f, 0.35f, 0.38f, 1f),
                    false);
                Text stateLabel = CreateText(
                    "StateLabelText",
                    stateBadge.transform,
                    "未解锁",
                    Vector2.zero,
                    new Vector2(170f, 32f),
                    17,
                    TextAnchor.MiddleCenter,
                    Color.white);
                GameObject bossMarker = CreateText(
                    "BossMarker",
                    button.transform,
                    "BOSS",
                    new Vector2(0f, 66f),
                    new Vector2(150f, 32f),
                    17,
                    TextAnchor.MiddleCenter,
                    new Color(0.95f, 0.55f, 0.42f, 1f)).gameObject;
                bossMarker.SetActive(index == 9);
                V04WorldMapStageNodeView view =
                    button.gameObject.AddComponent<V04WorldMapStageNodeView>();
                view.Configure(button, stageLabel, stateLabel, stateBadge, bossMarker);
                stageNodeViews[index] = view;
            }

            return root.gameObject;
        }

        private static V04WorldMapStageDetailDrawerView CreateStageDetailDrawer(
            Transform parent)
        {
            Image drawerBackground = CreateImage(
                "StageDetailDrawer",
                parent,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 0f),
                new Vector2(1020f, 1010f),
                new Color(0.075f, 0.07f, 0.065f, 0.99f),
                true);
            RectTransform drawer = drawerBackground.rectTransform;
            CreateImage(
                "StageDetailHeroImageSlot",
                drawer,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -145f),
                new Vector2(920f, 230f),
                new Color(0.16f, 0.15f, 0.13f, 1f),
                false).preserveAspect = true;
            Text title = CreateText(
                "StageTitleText",
                drawer,
                "1-1",
                new Vector2(-155f, -300f),
                new Vector2(590f, 62f),
                34,
                TextAnchor.MiddleLeft,
                PaleText);
            Text state = CreateText(
                "StageStateText",
                drawer,
                "节点状态",
                new Vector2(320f, -300f),
                new Vector2(250f, 50f),
                20,
                TextAnchor.MiddleRight,
                MutedText);
            Text enemy = CreateText(
                "EnemySummaryText",
                drawer,
                "敌人概要",
                new Vector2(0f, -392f),
                new Vector2(900f, 100f),
                21,
                TextAnchor.UpperLeft,
                PaleText);
            Text rank = CreateText(
                "RankRangeText",
                drawer,
                "品阶范围",
                new Vector2(0f, -476f),
                new Vector2(900f, 48f),
                19,
                TextAnchor.MiddleLeft,
                MutedText);
            Text firstClear = CreateText(
                "FirstClearRewardText",
                drawer,
                "首通奖励",
                new Vector2(0f, -528f),
                new Vector2(900f, 48f),
                19,
                TextAnchor.MiddleLeft,
                MutedText);

            V04WorldMapDropPreviewRowView[] dropRows =
                new V04WorldMapDropPreviewRowView[3];
            for (int index = 0; index < dropRows.Length; index++)
            {
                float y = -605f - index * 64f;
                Image rowBackground = CreateImage(
                    "DropPreviewRow_" + (index + 1),
                    drawer,
                    new Vector2(0.5f, 1f),
                    new Vector2(0.5f, 1f),
                    new Vector2(0f, y),
                    new Vector2(900f, 54f),
                    PanelBackground,
                    false);
                Text rowName = CreateText(
                    "DisplayNameText",
                    rowBackground.transform,
                    "掉落槽位",
                    new Vector2(-215f, 0f),
                    new Vector2(420f, 42f),
                    18,
                    TextAnchor.MiddleLeft,
                    PaleText);
                Text rowStatus = CreateText(
                    "BindingStatusText",
                    rowBackground.transform,
                    V04WorldMapCatalog.UnboundDropStatus,
                    new Vector2(215f, 0f),
                    new Vector2(420f, 42f),
                    15,
                    TextAnchor.MiddleRight,
                    MutedText);
                V04WorldMapDropPreviewRowView rowView =
                    rowBackground.gameObject.AddComponent<V04WorldMapDropPreviewRowView>();
                rowView.Configure(rowName, rowStatus);
                dropRows[index] = rowView;
            }

            Text diagnostic = CreateText(
                "DiagnosticText",
                drawer,
                string.Empty,
                new Vector2(0f, -824f),
                new Vector2(900f, 72f),
                16,
                TextAnchor.UpperLeft,
                MutedText);
            Button challenge = CreateButton(
                "ChallengeButton",
                drawer,
                "挑战（待接入）",
                new Vector2(-240f, -935f),
                new Vector2(300f, 82f),
                Accent);
            Button patrol = CreateButton(
                "SetPatrolButton",
                drawer,
                "设为巡行点（待接入）",
                new Vector2(115f, -935f),
                new Vector2(380f, 82f),
                WarmPanel);
            Button close = CreateButton(
                "CloseDrawerButton",
                drawer,
                "关闭",
                new Vector2(405f, -935f),
                new Vector2(150f, 82f),
                PanelBackground);
            challenge.interactable = false;
            patrol.interactable = false;

            V04WorldMapStageDetailDrawerView view =
                drawerBackground.gameObject.AddComponent<V04WorldMapStageDetailDrawerView>();
            view.Configure(
                title,
                state,
                enemy,
                rank,
                firstClear,
                diagnostic,
                dropRows,
                challenge,
                patrol,
                close);
            return view;
        }

        private static Canvas CreateCanvas()
        {
            GameObject canvasObject = new(
                "WorldMapCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static void CreateEventSystem()
        {
            GameObject eventSystemObject = new(
                "EventSystem",
                typeof(EventSystem),
                typeof(StandaloneInputModule));
            eventSystemObject.GetComponent<EventSystem>().firstSelectedGameObject = null;
        }

        private static RectTransform CreatePageRoot(string name, Transform parent)
        {
            return CreateRect(
                name,
                parent,
                Vector2.zero,
                Vector2.one,
                new Vector2(0f, -20f),
                new Vector2(0f, -270f));
        }

        private static RectTransform CreateRect(
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            GameObject target = new(name, typeof(RectTransform));
            RectTransform rect = target.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            return rect;
        }

        private static Image CreateImage(
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            Color color,
            bool raycastTarget)
        {
            RectTransform rect = CreateRect(
                name,
                parent,
                anchorMin,
                anchorMax,
                anchoredPosition,
                sizeDelta);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = raycastTarget;
            return image;
        }

        private static Button CreateButton(
            string name,
            Transform parent,
            string label,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            Color color)
        {
            Image image = CreateImage(
                name,
                parent,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                anchoredPosition,
                sizeDelta,
                color,
                true);
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            if (!string.IsNullOrEmpty(label))
            {
                CreateText(
                    "Label",
                    image.transform,
                    label,
                    Vector2.zero,
                    sizeDelta - new Vector2(24f, 16f),
                    22,
                    TextAnchor.MiddleCenter,
                    PaleText);
            }

            return button;
        }

        private static Text CreateText(
            string name,
            Transform parent,
            string value,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            int fontSize,
            TextAnchor alignment,
            Color color)
        {
            RectTransform rect = CreateRect(
                name,
                parent,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                anchoredPosition,
                sizeDelta);
            Text text = rect.gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value ?? string.Empty;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }
    }
}
#endif
