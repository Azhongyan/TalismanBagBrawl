using System;
using System.IO;
using TalismanBag.V03.MainHome;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.V03.Editor
{
    public static class V03MainHomeVisualStageAuthoring
    {
        private const string ScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V03_MainHome.unity";
        private const string PrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/MainHome/MainHomeVisualStage.prefab";
        private const string DecorationFolder =
            "Assets/_Game/Resources/V03/MainHome/Decoration";
        private const float ReferenceScale = 1080f / 941f;

        [MenuItem("TalismanBag/V0.3/MainHome/Reset Layered Visual Stage To Initial Template")]
        public static void ResetFromMenu()
        {
            if (!EditorUtility.DisplayDialog(
                    "Reset MainHome visual layout?",
                    "This replaces the current hand-authored MainHomeVisualStage Prefab and Scene instance with the initial template. Use Validate for normal work.",
                    "Reset Layout",
                    "Cancel"))
            {
                return;
            }

            AuthorAndValidate();
            Debug.Log("MAINHOME_LAYERED_VISUAL_STAGE_AUTHORING_PASS");
        }

        [MenuItem("TalismanBag/V0.3/MainHome/Validate Current Hand-Authored Visual Stage")]
        public static void ValidateCurrentFromMenu()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode, "Exit Play Mode before validating MainHome visuals.");
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ValidateScene(scene);
            Debug.Log("MAINHOME_HAND_AUTHORED_VISUAL_STAGE_VALIDATION_PASS");
        }

        public static void ExecuteFromCommandLine()
        {
            try
            {
                AuthorAndValidate();
                Debug.Log("MAINHOME_LAYERED_VISUAL_STAGE_AUTHORING_PASS");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        private static void AuthorAndValidate()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode, "Exit Play Mode before authoring MainHome visuals.");
            Require(AssetDatabase.IsValidFolder(DecorationFolder), "MainHome Decoration asset folder is missing.");

            ConfigureSprites();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            string prefabFolder = Path.GetDirectoryName(PrefabPath)?.Replace('\\', '/');
            if (!AssetDatabase.IsValidFolder(prefabFolder))
            {
                AssetDatabase.CreateFolder(
                    "Assets/_Game/Prefabs/TalismanBag",
                    "MainHome");
            }

            GameObject stage = BuildVisualStage();
            try
            {
                PrefabUtility.SaveAsPrefabAsset(stage, PrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(stage);
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject safeArea = FindSceneObject(scene, "MobileSafeAreaRoot");
            GameObject background = FindSceneObject(scene, "FullBackgroundImageSlot");
            GameObject mainHome = FindSceneObject(scene, "MainHomeRoot");
            GameObject bottomNav = FindSceneObject(scene, "BottomNavBar_Root");
            Require(safeArea != null, "MobileSafeAreaRoot is missing.");
            Require(background != null, "FullBackgroundImageSlot is missing.");
            Require(mainHome != null, "MainHomeRoot is missing.");
            Require(bottomNav != null, "BottomNavBar_Root is missing.");

            GameObject existing = FindSceneObject(scene, "MainHomeVisualStage");
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing);
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Require(prefab != null, "MainHomeVisualStage Prefab could not be loaded.");
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            instance.name = "MainHomeVisualStage";
            instance.transform.SetParent(safeArea.transform, false);
            SetFullStretch(instance.GetComponent<RectTransform>());
            instance.transform.SetSiblingIndex(background.transform.GetSiblingIndex() + 1);

            StyleExistingMainHomeUi(scene);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            ValidateScene(scene);
        }

        private static GameObject BuildVisualStage()
        {
            GameObject root = new("MainHomeVisualStage", typeof(RectTransform), typeof(MainHomeVisualStage));
            SetFullStretch(root.GetComponent<RectTransform>());

            // Far scene: one uninterrupted environment plate. It never moves and
            // carries no interactive foreground props.
            CreateReferenceImage(root.transform, "FarEnvironment", Sprite("MainHome_CleanReferenceEnvironment_v03"),
                0f, 0f, 941f, 1672f, Color.white);

            CanvasGroup reading = CreateReferenceImage(root.transform, "CharacterReading", Sprite("MainHome_CharacterReading_RGBA_v01"),
                266f, 582f, 356f, 356f, Color.white).gameObject.AddComponent<CanvasGroup>();
            CanvasGroup cleaning = CreateReferenceImage(root.transform, "CharacterCleaning", Sprite("MainHome_CharacterCleaning_RGBA_v01"),
                248f, 535f, 385f, 578f, Color.white).gameObject.AddComponent<CanvasGroup>();
            CanvasGroup gaze = CreateReferenceImage(root.transform, "CharacterBranchGaze", Sprite("MainHome_CharacterBranchGaze_RGBA_v01"),
                350f, 510f, 330f, 603f, Color.white).gameObject.AddComponent<CanvasGroup>();

            // Near-mid occlusion: this is the physical counter front. It masks
            // the character's lower body instead of relying on a flat composite.
            CreateReferenceImage(root.transform, "CounterOccluder", Sprite("MainHome_CounterOccluder_TrueCutout_v01"),
                70f, 840f, 760f, 260f, Color.white);

            // Foreground: every prop is a real transparent silhouette. No crop
            // contains a piece of the background, floor, or neighbouring prop.
            CreateReferenceImage(root.transform, "LedgerBook", Sprite("MainHome_LedgerBook_TrueCutout_v02"),
                68f, 985f, 270f, 410f, Color.white, true);
            CreateReferenceImage(root.transform, "CodexBook", Sprite("MainHome_CodexBook_TrueCutout_v02"),
                345f, 990f, 280f, 410f, Color.white, true);
            CreateReferenceImage(root.transform, "ClueBook", Sprite("MainHome_ClueBook_TrueCutout_v02"),
                628f, 985f, 270f, 410f, Color.white, true);

            CreateReferenceImage(root.transform, "ForegroundBaguaChest", Sprite("MainHome_BaguaChest_RGBA_v01"),
                -25f, 1345f, 300f, 268f, Color.white);
            CreateReferenceImage(root.transform, "ForegroundLowTable", Sprite("MainHome_LowTable_RGBA_v01"),
                300f, 1395f, 370f, 155f, Color.white);
            CreateReferenceImage(root.transform, "ForegroundScrollAndCloth", Sprite("MainHome_ScrollAndCloth_RGBA_v01"),
                635f, 1285f, 350f, 300f, Color.white);
            CreateReferenceImage(root.transform, "DreamSign", Sprite("MainHome_DreamSign_TrueCutout_v02"),
                352f, 1320f, 238f, 205f, Color.white, true);

            RectTransform branch = CreateReferenceImage(root.transform, "BlossomBranch", Sprite("MainHome_BlossomBranch_RGBA_v01"),
                410f, -5f, 540f, 405f, Color.white);
            branch.pivot = new Vector2(1f, 0f);

            RectTransform petalsA = CreateReferenceImage(root.transform, "FallingPetals_A", Sprite("MainHome_FallingPetals_RGBA_v01"),
                0f, 0f, 941f, 1672f, new Color(1f, 1f, 1f, 0.12f));
            RectTransform petalsB = CreateReferenceImage(root.transform, "FallingPetals_B", Sprite("MainHome_FallingPetals_RGBA_v01"),
                0f, -1672f, 941f, 1672f, new Color(1f, 1f, 1f, 0.08f));

            RectTransform dapple = CreateReferenceImage(root.transform, "SunlightDapple", Sprite("MainHome_Dapple_RGBA_v01"),
                0f, 0f, 941f, 1672f, Color.white);
            CanvasGroup dappleGroup = dapple.gameObject.AddComponent<CanvasGroup>();
            dappleGroup.alpha = 0.16f;

            RectTransform tintRect = CreateReferenceImage(root.transform, "DaylightTint", null,
                0f, 0f, 941f, 1672f, Color.clear);
            Image tint = tintRect.GetComponent<Image>();

            CreateReferenceImage(root.transform, "BottomNavigationPaperWash", null,
                0f, 1498f, 941f, 174f, new Color(0.94f, 0.86f, 0.69f, 0.92f));

            MainHomeVisualStage stage = root.GetComponent<MainHomeVisualStage>();
            stage.AssignForEditor(
                reading,
                cleaning,
                gaze,
                branch,
                dappleGroup,
                dapple,
                petalsA,
                petalsB,
                tint,
                dapple.GetComponent<Image>(),
                null);

            return root;
        }

        private static RectTransform CreateReferenceImage(
            Transform parent,
            string name,
            Sprite sprite,
            float left,
            float top,
            float width,
            float height,
            Color color,
            bool preserveAspect = false)
        {
            return CreateImage(
                parent,
                name,
                sprite,
                left * ReferenceScale,
                top * ReferenceScale,
                width * ReferenceScale,
                height * ReferenceScale,
                color,
                preserveAspect);
        }

        private static RectTransform CreateImage(
            Transform parent,
            string name,
            Sprite sprite,
            float left,
            float top,
            float width,
            float height,
            Color color,
            bool preserveAspect = false)
        {
            GameObject gameObject = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(left, -top);
            rect.sizeDelta = new Vector2(width, height);

            Image image = gameObject.GetComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = preserveAspect;
            image.color = color;
            image.raycastTarget = false;
            return rect;
        }

        private static void ConfigureSprites()
        {
            string absoluteFolder = Path.Combine(Directory.GetCurrentDirectory(), DecorationFolder);
            foreach (string path in Directory.GetFiles(absoluteFolder, "*.png", SearchOption.TopDirectoryOnly))
            {
                string assetPath = path
                    .Substring(Directory.GetCurrentDirectory().Length + 1)
                    .Replace('\\', '/');
                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                Require(importer != null, "TextureImporter is missing for " + assetPath);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.sRGBTexture = true;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.maxTextureSize = 2048;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.SaveAndReimport();
            }
        }

        private static Sprite Sprite(string name)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{DecorationFolder}/{name}.png");
            Require(sprite != null, "Required MainHome sprite is missing: " + name);
            return sprite;
        }

        private static void ValidateScene(Scene scene)
        {
            GameObject stageObject = FindSceneObject(scene, "MainHomeVisualStage");
            GameObject background = FindSceneObject(scene, "FullBackgroundImageSlot");
            GameObject homeRoot = FindSceneObject(scene, "MainHomeRoot");
            GameObject bottomNav = FindSceneObject(scene, "BottomNavBar_Root");
            Require(stageObject != null, "Authored MainHomeVisualStage is missing.");
            Require(stageObject.GetComponent<MainHomeVisualStage>() != null, "MainHomeVisualStage component is missing.");
            Require(background.transform.GetSiblingIndex() < stageObject.transform.GetSiblingIndex(),
                "MainHomeVisualStage must render above the locked background.");
            Require(stageObject.transform.GetSiblingIndex() < homeRoot.transform.GetSiblingIndex(),
                "MainHomeVisualStage must render below business hotspots.");
            Require(stageObject.transform.GetSiblingIndex() < bottomNav.transform.GetSiblingIndex(),
                "MainHomeVisualStage must render below bottom navigation.");

            foreach (Graphic graphic in stageObject.GetComponentsInChildren<Graphic>(true))
            {
                Require(!graphic.raycastTarget, "Visual layer must not block input: " + graphic.name);
            }

            Require(stageObject.GetComponentsInChildren<Image>(true).Length >= 14,
                "Layered MainHome visual inventory is incomplete.");
            Require(!scene.isDirty, "MainHome scene must be saved after authoring.");
        }

        private static GameObject FindSceneObject(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Transform candidate in root.GetComponentsInChildren<Transform>(true))
                {
                    if (candidate != null && candidate.name == name)
                    {
                        return candidate.gameObject;
                    }
                }
            }

            return null;
        }

        private static void StyleExistingMainHomeUi(Scene scene)
        {
            GameObject topBar = FindSceneObject(scene, "TopBar_Root");
            GameObject sceneRoot = FindSceneObject(scene, "SceneHotspot_Root");
            GameObject hotspotRoot = FindSceneObject(scene, "HomeSpatialHotspots_Root");
            GameObject shortcutRoot = FindSceneObject(scene, "RightQuickBar_Root");
            GameObject bottomNav = FindSceneObject(scene, "BottomNavBar_Root");
            Require(topBar != null, "TopBar_Root is missing.");
            Require(sceneRoot != null, "SceneHotspot_Root is missing.");
            Require(hotspotRoot != null, "HomeSpatialHotspots_Root is missing.");
            Require(shortcutRoot != null, "RightQuickBar_Root is missing.");
            Require(bottomNav != null, "BottomNavBar_Root is missing.");

            EnsureObjectivePaper(scene);

            RectTransform topRect = topBar.GetComponent<RectTransform>();
            AnchorTop(topRect, new Vector2(0f, -18f), new Vector2(1010f, 132f));
            Image topPaper = EnsureImage(topBar);
            topPaper.enabled = false;
            topPaper.raycastTarget = false;
            topPaper.color = new Color(0.94f, 0.86f, 0.69f, 0.18f);

            StyleText(FindChild(topBar.transform, "HomeTitle_Text"), 30, FontStyle.Bold,
                new Color(0.18f, 0.13f, 0.09f, 0.96f), TextAnchor.MiddleLeft);
            StyleText(FindChild(topBar.transform, "PlayerIdentity_Text"), 20, FontStyle.Normal,
                new Color(0.29f, 0.22f, 0.16f, 0.92f), TextAnchor.MiddleLeft);
            StyleText(FindChild(topBar.transform, "TopBarResource_Text"), 18, FontStyle.Normal,
                new Color(0.22f, 0.16f, 0.11f, 0.94f), TextAnchor.UpperRight);

            RectTransform sceneRect = sceneRoot.GetComponent<RectTransform>();
            AnchorCenter(sceneRect, Vector2.zero, new Vector2(1080f, 1920f));
            RectTransform hotspotRect = hotspotRoot.GetComponent<RectTransform>();
            AnchorCenter(hotspotRect, Vector2.zero, new Vector2(1080f, 1920f));

            StyleHotspot(scene, "HomeHotspot_Ledger", new Vector2(-325f, -380f), new Vector2(330f, 530f), false);
            StyleHotspot(scene, "HomeHotspot_CodexBook", new Vector2(0f, -380f), new Vector2(315f, 530f), false);
            StyleHotspot(scene, "HomeHotspot_ClueBook", new Vector2(320f, -380f), new Vector2(330f, 530f), false);
            StyleHotspot(scene, "HomeHotspot_DreamSign", new Vector2(5f, -635f), new Vector2(280f, 250f), false);
            StyleHotspot(scene, "HomeHotspot_Xiaoman", new Vector2(-15f, 85f), new Vector2(420f, 430f), false);
            StyleHotspot(scene, "HomeHotspot_BackRoom", new Vector2(355f, 235f), new Vector2(210f, 320f), false);
            StyleHotspot(scene, "HomeHotspot_StreetEntrance", new Vector2(0f, -655f), new Vector2(300f, 120f), false);

            RectTransform shortcutRect = shortcutRoot.GetComponent<RectTransform>();
            shortcutRect.anchorMin = new Vector2(1f, 1f);
            shortcutRect.anchorMax = new Vector2(1f, 1f);
            shortcutRect.pivot = new Vector2(1f, 1f);
            shortcutRect.anchoredPosition = new Vector2(-20f, -185f);
            shortcutRect.sizeDelta = new Vector2(104f, 560f);
            string[] shortcuts = { "Activity", "Mail", "Store", "Notice", "Settings" };
            string[] shortcutIcons =
            {
                "BronzeLantern_RGBA",
                "MainHome_ScrollAndCloth_RGBA_v01",
                "SmallWoodChest_RGBA",
                "OpenLedger_RGBA",
                "BaguaChestTall_RGBA"
            };
            for (int i = 0; i < shortcuts.Length; i++)
            {
                GameObject shortcut = FindSceneObject(scene, "HomeHotspot_" + shortcuts[i]);
                if (shortcut == null)
                {
                    continue;
                }

                RectTransform rect = shortcut.GetComponent<RectTransform>();
                AnchorTop(rect, new Vector2(0f, -i * 106f), new Vector2(88f, 92f));
                StyleButtonSurface(shortcut, new Color(0.35f, 0.23f, 0.14f, 0.25f), 0.18f);
                CreateDecorativeIcon(shortcut.transform, Sprite(shortcutIcons[i]),
                    new Vector2(0f, 10f), new Vector2(58f, 58f));
                Text title = FindChild(shortcut.transform, "Title")?.GetComponent<Text>();
                if (title != null)
                {
                    title.fontSize = 15;
                    title.fontStyle = FontStyle.Bold;
                    title.color = new Color(0.96f, 0.88f, 0.71f, 1f);
                    RectTransform titleRect = title.GetComponent<RectTransform>();
                    titleRect.anchorMin = new Vector2(0f, 0f);
                    titleRect.anchorMax = new Vector2(1f, 0f);
                    titleRect.pivot = new Vector2(0.5f, 0f);
                    titleRect.anchoredPosition = new Vector2(0f, 4f);
                    titleRect.sizeDelta = new Vector2(-8f, 24f);
                    title.alignment = TextAnchor.MiddleCenter;
                }

                SetChildActive(shortcut.transform, "State", false);
                SetChildActive(shortcut.transform, "VisualFallback", false);
                SetChildActive(shortcut.transform, "IconFallback", false);
            }

            RectTransform navRect = bottomNav.GetComponent<RectTransform>();
            navRect.anchoredPosition = new Vector2(0f, 22f);
            navRect.sizeDelta = new Vector2(1030f, 160f);
            Image navPaper = EnsureImage(bottomNav);
            navPaper.enabled = true;
            navPaper.raycastTarget = false;
            navPaper.color = new Color(0.95f, 0.88f, 0.72f, 0.62f);

            string[] navButtons =
            {
                "BottomNavHomeButton",
                "BottomNavRefineButton",
                "BottomNavTrialButton",
                "BottomNavExploreButton",
                "BottomNavMoreButton"
            };
            string[] navIcons =
            {
                "BronzeLantern_RGBA",
                "MainHome_BaguaChest_RGBA_v01",
                "BaguaChestTall_RGBA",
                "MainHome_ScrollAndCloth_RGBA_v01",
                "SmallWoodChest_RGBA"
            };
            for (int i = 0; i < navButtons.Length; i++)
            {
                GameObject nav = FindSceneObject(scene, navButtons[i]);
                if (nav == null)
                {
                    continue;
                }

                RectTransform rect = nav.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2((i - 2) * 198f, 0f);
                rect.sizeDelta = new Vector2(178f, i == 0 ? 132f : 120f);
                StyleButtonSurface(nav,
                    i == 0
                        ? new Color(0.95f, 0.86f, 0.68f, 0.72f)
                        : new Color(0.43f, 0.34f, 0.24f, 0.03f),
                    i == 0 ? 0.24f : 0f);
                CreateDecorativeIcon(nav.transform, Sprite(navIcons[i]),
                    new Vector2(0f, 14f), new Vector2(i == 0 ? 68f : 58f, i == 0 ? 68f : 58f));
                Text label = nav.GetComponentInChildren<Text>(true);
                if (label != null)
                {
                    label.fontSize = i == 0 ? 22 : 20;
                    label.fontStyle = FontStyle.Bold;
                    label.color = new Color(0.24f, 0.17f, 0.12f, 0.96f);
                    RectTransform labelRect = label.GetComponent<RectTransform>();
                    labelRect.anchorMin = new Vector2(0f, 0f);
                    labelRect.anchorMax = new Vector2(1f, 0f);
                    labelRect.pivot = new Vector2(0.5f, 0f);
                    labelRect.anchoredPosition = new Vector2(0f, 7f);
                    labelRect.sizeDelta = new Vector2(-10f, 36f);
                    label.alignment = TextAnchor.MiddleCenter;
                }
            }
        }

        private static void EnsureObjectivePaper(Scene scene)
        {
            GameObject uiRoot = FindSceneObject(scene, "V03MainHomeUiueRoot");
            GameObject mainHomeRoot = FindSceneObject(scene, "MainHomeRoot");
            Require(uiRoot != null, "V03MainHomeUiueRoot is missing.");
            Require(mainHomeRoot != null, "MainHomeRoot is missing.");

            Transform existing = FindChild(uiRoot.transform, "MainHomeObjectivePaper");
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing.gameObject);
            }

            GameObject paper = new(
                "MainHomeObjectivePaper",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Outline));
            paper.transform.SetParent(uiRoot.transform, false);
            RectTransform paperRect = paper.GetComponent<RectTransform>();
            paperRect.anchorMin = new Vector2(0f, 1f);
            paperRect.anchorMax = new Vector2(0f, 1f);
            paperRect.pivot = new Vector2(0f, 1f);
            paperRect.anchoredPosition = new Vector2(20f, -172f);
            paperRect.sizeDelta = new Vector2(330f, 216f);
            Image paperImage = paper.GetComponent<Image>();
            paperImage.color = new Color(0.94f, 0.86f, 0.69f, 0.86f);
            paperImage.raycastTarget = false;
            Outline outline = paper.GetComponent<Outline>();
            outline.effectColor = new Color(0.34f, 0.21f, 0.12f, 0.45f);
            outline.effectDistance = new Vector2(2f, -2f);

            Text heading = CreatePaperText(
                paper.transform,
                "ObjectiveHeading_Text",
                "照灯账本",
                new Vector2(20f, -14f),
                new Vector2(290f, 42f),
                26,
                FontStyle.Bold);
            heading.color = new Color(0.20f, 0.13f, 0.08f, 0.98f);

            Text objective = CreatePaperText(
                paper.transform,
                "LedgerObjective_Text",
                string.Empty,
                new Vector2(20f, -62f),
                new Vector2(290f, 58f),
                18,
                FontStyle.Bold);
            Text progress = CreatePaperText(
                paper.transform,
                "LedgerProgress_Text",
                string.Empty,
                new Vector2(20f, -124f),
                new Vector2(290f, 38f),
                16,
                FontStyle.Normal);
            Text status = CreatePaperText(
                paper.transform,
                "HomeStatus_Text",
                string.Empty,
                new Vector2(20f, -165f),
                new Vector2(290f, 34f),
                14,
                FontStyle.Normal);

            SerializedObject panel = new(mainHomeRoot.GetComponent("MainHomeGreyboxPanel"));
            panel.FindProperty("objectiveText").objectReferenceValue = objective;
            panel.FindProperty("progressText").objectReferenceValue = progress;
            panel.FindProperty("statusText").objectReferenceValue = status;
            panel.ApplyModifiedPropertiesWithoutUndo();

            int shortcutIndex = FindSceneObject(scene, "RightQuickBar_Root").transform.GetSiblingIndex();
            paper.transform.SetSiblingIndex(Mathf.Max(0, shortcutIndex));
        }

        private static Text CreatePaperText(
            Transform parent,
            string name,
            string value,
            Vector2 position,
            Vector2 size,
            int fontSize,
            FontStyle fontStyle)
        {
            GameObject textObject = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.color = new Color(0.27f, 0.19f, 0.12f, 0.94f);
            text.alignment = TextAnchor.UpperLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            text.text = value;
            return text;
        }

        private static void CreateDecorativeIcon(
            Transform parent,
            Sprite sprite,
            Vector2 position,
            Vector2 size)
        {
            Transform existing = FindChild(parent, "DecorationIcon");
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing.gameObject);
            }

            GameObject iconObject = new(
                "DecorationIcon",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            iconObject.transform.SetParent(parent, false);
            iconObject.transform.SetAsFirstSibling();
            RectTransform rect = iconObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Image image = iconObject.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.color = Color.white;
            image.raycastTarget = false;
        }

        private static void StyleHotspot(
            Scene scene,
            string objectName,
            Vector2 position,
            Vector2 size,
            bool showLabel)
        {
            GameObject hotspot = FindSceneObject(scene, objectName);
            if (hotspot == null)
            {
                return;
            }

            AnchorCenter(hotspot.GetComponent<RectTransform>(), position, size);
            StyleButtonSurface(hotspot, new Color(0.32f, 0.20f, 0.10f, showLabel ? 0.08f : 0.015f),
                showLabel ? 0.22f : 0f);
            SetChildActive(hotspot.transform, "VisualFallback", false);
            SetChildActive(hotspot.transform, "IconFallback", false);
            Text title = FindChild(hotspot.transform, "Title")?.GetComponent<Text>();
            Text state = FindChild(hotspot.transform, "State")?.GetComponent<Text>();
            if (title != null)
            {
                title.gameObject.SetActive(showLabel);
                title.fontSize = 20;
                title.fontStyle = FontStyle.Bold;
                title.color = new Color(0.20f, 0.13f, 0.08f, 0.92f);
                title.alignment = TextAnchor.LowerCenter;
                RectTransform titleRect = title.GetComponent<RectTransform>();
                titleRect.anchorMin = new Vector2(0f, 0f);
                titleRect.anchorMax = new Vector2(1f, 0f);
                titleRect.pivot = new Vector2(0.5f, 0f);
                titleRect.anchoredPosition = new Vector2(0f, 8f);
                titleRect.sizeDelta = new Vector2(-16f, 42f);
            }

            if (state != null)
            {
                state.gameObject.SetActive(false);
            }
        }

        private static void StyleButtonSurface(GameObject target, Color color, float outlineAlpha)
        {
            Image image = EnsureImage(target);
            image.enabled = true;
            image.raycastTarget = true;
            image.color = color;
            Outline outline = target.GetComponent<Outline>();
            if (outline != null)
            {
                outline.enabled = outlineAlpha > 0f;
                outline.effectColor = new Color(0.40f, 0.24f, 0.12f, outlineAlpha);
                outline.effectDistance = new Vector2(1.5f, -1.5f);
            }
        }

        private static Image EnsureImage(GameObject target)
        {
            Image image = target.GetComponent<Image>();
            return image != null ? image : target.AddComponent<Image>();
        }

        private static Transform FindChild(Transform root, string name)
        {
            if (root == null)
            {
                return null;
            }

            foreach (Transform candidate in root.GetComponentsInChildren<Transform>(true))
            {
                if (candidate.name == name)
                {
                    return candidate;
                }
            }

            return null;
        }

        private static void SetChildActive(Transform root, string name, bool active)
        {
            Transform child = FindChild(root, name);
            if (child != null)
            {
                child.gameObject.SetActive(active);
            }
        }

        private static void StyleText(
            Transform transform,
            int fontSize,
            FontStyle style,
            Color color,
            TextAnchor alignment)
        {
            Text text = transform != null ? transform.GetComponent<Text>() : null;
            if (text == null)
            {
                return;
            }

            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
        }

        private static void AnchorCenter(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void AnchorTop(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void SetFullStretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }

    [InitializeOnLoad]
    internal static class V03MainHomeVisualCapture
    {
        private const string ScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V03_MainHome.unity";
        private const string CapturePath =
            "Logs/mainhome_reference_replica_capture_v04.png";
        private const string ActiveKey =
            "TalismanBag.V03.MainHome.VisualCapture.Active";
        private const string ExitKey =
            "TalismanBag.V03.MainHome.VisualCapture.Exit";

        private static int frames;

        static V03MainHomeVisualCapture()
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        public static void ExecuteFromCommandLine()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException("MainHome visual capture is already running.");
            }

            string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), CapturePath);
            if (File.Exists(absolutePath))
            {
                File.Delete(absolutePath);
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            SessionState.SetBool(ActiveKey, true);
            SessionState.SetBool(ExitKey, false);
            frames = 0;
            EditorApplication.isPlaying = true;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(ActiveKey, false))
            {
                frames = 0;
                return;
            }

            if (state != PlayModeStateChange.EnteredEditMode || !SessionState.GetBool(ExitKey, false))
            {
                return;
            }

            SessionState.EraseBool(ExitKey);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(0);
            }
        }

        private static void OnEditorUpdate()
        {
            if (!SessionState.GetBool(ActiveKey, false) || !EditorApplication.isPlaying)
            {
                return;
            }

            frames++;
            if (frames == 45)
            {
                ValidateLiveVisualStage();
                string capture = CaptureCanvasOffscreen();
                Debug.Log("MAINHOME_DECORATION_VISIBLE_CAPTURE_PASS path=" + capture);
                SessionState.EraseBool(ActiveKey);
                SessionState.SetBool(ExitKey, true);
                EditorApplication.isPlaying = false;
            }
        }

        private static string CaptureCanvasOffscreen()
        {
            Canvas sourceCanvas = UnityEngine.Object.FindObjectOfType<Canvas>();
            if (sourceCanvas == null)
            {
                throw new InvalidOperationException("MainHome Canvas is missing.");
            }

            GameObject cameraObject = new("MainHomeDecorationCaptureCamera", typeof(Camera));
            RenderTexture target = new(1080, 1920, 24, RenderTextureFormat.ARGB32);
            Texture2D texture = new(1080, 1920, TextureFormat.RGB24, false);
            RenderMode previousMode = sourceCanvas.renderMode;
            Camera previousCamera = sourceCanvas.worldCamera;
            Camera captureCamera = cameraObject.GetComponent<Camera>();
            try
            {
                captureCamera.clearFlags = CameraClearFlags.SolidColor;
                captureCamera.backgroundColor = Color.black;
                captureCamera.orthographic = true;
                captureCamera.orthographicSize = 960f;
                captureCamera.transform.position = new Vector3(0f, 0f, -1000f);
                captureCamera.targetTexture = target;
                captureCamera.cullingMask = ~0;

                sourceCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                sourceCanvas.worldCamera = captureCamera;
                sourceCanvas.planeDistance = 100f;
                Canvas.ForceUpdateCanvases();
                captureCamera.Render();

                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = target;
                texture.ReadPixels(new Rect(0f, 0f, 1080f, 1920f), 0, 0);
                texture.Apply(false, false);
                RenderTexture.active = previous;

                string path = Path.Combine(Directory.GetCurrentDirectory(), CapturePath);
                Directory.CreateDirectory(Path.GetDirectoryName(path) ?? Directory.GetCurrentDirectory());
                File.WriteAllBytes(path, texture.EncodeToPNG());
                return path;
            }
            finally
            {
                sourceCanvas.renderMode = previousMode;
                sourceCanvas.worldCamera = previousCamera;
                captureCamera.targetTexture = null;
                UnityEngine.Object.DestroyImmediate(texture);
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
                UnityEngine.Object.DestroyImmediate(cameraObject);
            }
        }

        private static void ValidateLiveVisualStage()
        {
            MainHomeVisualStage stage = UnityEngine.Object.FindObjectOfType<MainHomeVisualStage>();
            if (stage == null || !stage.isActiveAndEnabled)
            {
                throw new InvalidOperationException("Live MainHomeVisualStage is missing or disabled.");
            }

            Image[] images = stage.GetComponentsInChildren<Image>(true);
            if (images.Length < 14 || Array.Exists(images, image => image.sprite == null && image.name != "DaylightTint" && image.name != "BottomNavigationPaperWash"))
            {
                throw new InvalidOperationException("Live MainHome visual inventory is incomplete.");
            }

            if (Array.Exists(images, image => image.raycastTarget))
            {
                throw new InvalidOperationException("MainHome decoration must not block input.");
            }
        }
    }
}
