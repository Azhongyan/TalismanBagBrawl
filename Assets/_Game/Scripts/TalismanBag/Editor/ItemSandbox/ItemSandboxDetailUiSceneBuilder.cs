#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using TalismanBag.ItemSandbox;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Detail;
using TalismanBag.Items.Detail.UI;
using TalismanBag.Items.InnerCatalog;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.EditorTools.ItemSandbox
{
    public static class ItemSandboxDetailUiSceneBuilder
    {
        public const string ScenePath = "Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity";
        public const string ItemDetailPanelPrefabPath = "Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab";
        public const string ItemBalanceWorkbenchCatalogPath = "Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset";

        private static readonly Color PageBackgroundColor = new(0.11f, 0.09f, 0.07f, 1f);
        private static readonly Color DarkPanelColor = new(0.22f, 0.16f, 0.11f, 0.94f);
        private static readonly Color OldPaperSoftColor = new(0.86f, 0.77f, 0.58f, 0.92f);
        private static readonly Color SealColor = new(0.48f, 0.12f, 0.08f, 1f);
        private static readonly Color DetailCardColor = new(0.115f, 0.079f, 0.052f, 0.992f);
        private static readonly Color DetailSectionColor = new(0.22f, 0.16f, 0.10f, 0.48f);
        private static readonly Color DetailTitleColor = new Color32(185, 154, 97, 255);
        private static readonly Color DetailTextColor = new Color32(216, 204, 183, 255);
        private static readonly Color DetailMutedTextColor = new Color32(113, 111, 105, 255);
        private static readonly Color DetailDividerColor = new(0.56f, 0.42f, 0.23f, 0.88f);
        private static readonly Color DetailChapterColor = new(0.30f, 0.21f, 0.12f, 0.82f);
        private static readonly Color DetailChapterSealColor = new(0.45f, 0.13f, 0.09f, 0.96f);
        private static readonly Color DetailScrollbarColor = new(0.10f, 0.075f, 0.055f, 0.92f);
        private static readonly Color DetailScrollbarHandleColor = new(0.60f, 0.45f, 0.24f, 0.92f);
        private static readonly Vector2 ItemListAnchorMin = new(0.025f, 0.045f);
        private static readonly Vector2 ItemListAnchorMax = new(0.20f, 0.955f);
        private static readonly Vector2 PlacementAnchorMin = new(0.22f, 0.045f);
        private static readonly Vector2 PlacementAnchorMax = new(0.45f, 0.955f);
        private static readonly Vector2 DetailAnchorMin = new(0.47f, 0.045f);
        private static readonly Vector2 DetailAnchorMax = new(0.975f, 0.955f);

        [MenuItem("Tools/Talisman Bag/Dev Only/V0.4/ItemSandbox/ItemSandboxDetailUi01/[Writes Scene][Manual Only] Build Item Sandbox Scene")]
        public static void BuildSceneMenu()
        {
            BuildScene();
        }

        public static void BuildSceneBatch()
        {
            try
            {
                BuildScene();
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(0);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
        }

        [MenuItem("Tools/Talisman Bag/Dev Only/V0.4/ItemSandbox/ItemGridPlacementAndEyeRule01/[Manual Only] Validate Manual Panel Layout")]
        public static void ApplyFlexiblePanelLayoutMenu()
        {
            ApplyFlexiblePanelLayout();
        }

        public static void ApplyFlexiblePanelLayoutBatch()
        {
            try
            {
                ApplyFlexiblePanelLayout();
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(0);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
        }

        [MenuItem("Tools/Talisman Bag/Dev Only/V0.4/ItemSandbox/ItemDetailPanel/[Writes Scene][Manual Only] Apply Item Detail Card Layout")]
        public static void ApplyDetailCardLayoutMenu()
        {
            ApplyDetailCardLayout();
        }

        public static void ApplyDetailCardLayoutBatch()
        {
            try
            {
                ApplyDetailCardLayout();
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(0);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
        }

        [MenuItem("Tools/Talisman Bag/Dev Only/V0.4/ItemSandbox/ItemDetailPanel/[Writes Prefab][Manual Only] Build Runtime Prefab")]
        public static void BuildRuntimeItemDetailPanelPrefabMenu()
        {
            BuildRuntimeItemDetailPanelPrefab();
        }

        public static void BuildRuntimeItemDetailPanelPrefabBatch()
        {
            try
            {
                BuildRuntimeItemDetailPanelPrefab();
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(0);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
        }

        [MenuItem("Tools/Talisman Bag/Dev Only/V0.4/ItemSandbox/ItemSkillTriggerContract01/[Writes Scene][Manual Only] Apply Skill Monitor Preview Layout")]
        public static void ApplyItemSkillTriggerContractLayoutMenu()
        {
            ApplyItemSkillTriggerContractLayout();
        }

        public static void ApplyItemSkillTriggerContractLayoutBatch()
        {
            try
            {
                ApplyItemSkillTriggerContractLayout();
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(0);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
        }

        [MenuItem("Tools/Talisman Bag/Dev Only/V0.4/ItemSandbox/ItemDetailInstanceDataAdapter01/[Writes Scene][Manual Only] Apply Instance Modes")]
        public static void ApplyItemDetailInstanceDataAdapterLayoutMenu()
        {
            ApplyItemDetailInstanceDataAdapterLayout();
        }

        public static void ApplyItemDetailInstanceDataAdapterLayoutBatch()
        {
            try
            {
                ApplyItemDetailInstanceDataAdapterLayout();
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(0);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
        }

        [MenuItem("Tools/Talisman Bag/Dev Only/V0.4/ItemSandbox/ItemBalanceCandidateDetailSandboxAdapter01/[Writes Scene][Manual Only] Apply Candidate Preview")]
        public static void ApplyItemBalanceCandidateDetailSandboxLayoutMenu()
        {
            ApplyItemBalanceCandidateDetailSandboxLayout();
        }

        public static void ApplyItemBalanceCandidateDetailSandboxLayoutBatch()
        {
            try
            {
                ApplyItemBalanceCandidateDetailSandboxLayout();
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(0);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
        }

        public static void BuildScene()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath) ?? "Assets/_Game/Scenes");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Scene_TalismanBag_V04_ItemSandbox";
            RenderSettings.ambientLight = new Color(0.58f, 0.50f, 0.40f, 1f);

            CreateCamera();
            CreateEventSystem();
            BuildCanvas();
            BuildRuntimeItemDetailPanelPrefab();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"ItemSandboxDetailUi01 scene generated: {ScenePath}");
        }

        public static void ApplyItemDetailInstanceDataAdapterLayout()
        {
            if (!File.Exists(ScenePath))
            {
                throw new FileNotFoundException("Missing Item Sandbox scene.", ScenePath);
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject root = GameObject.Find("ItemSandboxRoot");
            GameObject listPanel = GameObject.Find("ItemNameListPanel");
            if (root == null || listPanel == null)
            {
                throw new InvalidOperationException("Item Sandbox root or item list panel is missing.");
            }

            ItemInnerDataCatalogProvider catalogProvider = root.GetComponent<ItemInnerDataCatalogProvider>();
            ItemSandboxDetailUiController controller = root.GetComponent<ItemSandboxDetailUiController>();
            if (catalogProvider == null || controller == null)
            {
                throw new InvalidOperationException("Item Sandbox catalog provider or controller is missing.");
            }

            ItemSandboxGeneratedInstanceProvider generatedProvider =
                root.GetComponent<ItemSandboxGeneratedInstanceProvider>()
                ?? root.AddComponent<ItemSandboxGeneratedInstanceProvider>();
            generatedProvider.ConfigureEditor(catalogProvider);
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            (Button catalogButton, Button generatedButton) = EnsureInstanceModeBar(listPanel.transform, font);
            controller.ConfigureInstanceModesEditor(generatedProvider, catalogButton, generatedButton);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"ItemDetailInstanceDataAdapter01 instance modes applied without rebuilding existing detail UI: {ScenePath}");
        }

        public static void ApplyItemBalanceCandidateDetailSandboxLayout()
        {
            if (!File.Exists(ScenePath))
            {
                throw new FileNotFoundException("Missing Item Sandbox scene.", ScenePath);
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject root = GameObject.Find("ItemSandboxRoot");
            GameObject listPanel = GameObject.Find("ItemNameListPanel");
            if (root == null || listPanel == null)
            {
                throw new InvalidOperationException("Item Sandbox root or item list panel is missing.");
            }

            ItemInnerDataCatalogProvider catalogProvider = root.GetComponent<ItemInnerDataCatalogProvider>();
            ItemSandboxDetailUiController controller = root.GetComponent<ItemSandboxDetailUiController>();
            ItemBalanceWorkbenchCatalog balanceCatalog =
                AssetDatabase.LoadAssetAtPath<ItemBalanceWorkbenchCatalog>(ItemBalanceWorkbenchCatalogPath);
            if (catalogProvider == null || controller == null || balanceCatalog == null)
            {
                throw new InvalidOperationException("Candidate preview requires catalog provider, controller, and workbench catalog asset.");
            }

            ItemBalanceCandidateDetailSandboxProvider candidateProvider =
                root.GetComponent<ItemBalanceCandidateDetailSandboxProvider>()
                ?? root.AddComponent<ItemBalanceCandidateDetailSandboxProvider>();
            candidateProvider.ConfigureEditor(catalogProvider, balanceCatalog);

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            (Button catalogButton, Button candidateButton) = EnsureInstanceModeBar(listPanel.transform, font);
            SetButtonLabel(catalogButton, "Catalog Preview");
            SetButtonLabel(candidateButton, "Candidate Instance Preview");
            CandidatePreviewBindings bindings = EnsureCandidatePreviewControls(listPanel.transform, font);

            controller.ConfigureInstanceModesEditor(candidateProvider, catalogButton, candidateButton);
            controller.ConfigureCandidatePreviewEditor(
                candidateProvider,
                bindings.root,
                bindings.rarityButtons,
                bindings.seedInput,
                bindings.regenerateButton,
                bindings.validationText);
            bindings.root.SetActive(false);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Item balance candidate preview applied without rebuilding ItemDetailPanel: {ScenePath}");
        }

        public static void ApplyFlexiblePanelLayout()
        {
            if (!File.Exists(ScenePath))
            {
                throw new FileNotFoundException("Missing Item Sandbox scene.", ScenePath);
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ValidateManualPanelExists("ItemNameListPanel");
            ValidateManualPanelExists("ItemGridPlacementPreviewPanel");
            ValidateManualPanelExists("ItemDetailPanel");
            Debug.Log($"ItemSandbox manual panel layout preserved; no RectTransform, Image color, spacing, or padding values were changed: {ScenePath}");
        }

        public static void ApplyDetailCardLayout()
        {
            if (!File.Exists(ScenePath))
            {
                throw new FileNotFoundException("Missing Item Sandbox scene.", ScenePath);
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject panel = GameObject.Find("ItemDetailPanel");
            if (panel == null)
            {
                throw new InvalidOperationException("Missing panel: ItemDetailPanel");
            }

            ItemSandboxDetailPanelView detailPanel = RebuildDetailPanelContent(panel, font);
            ItemInnerDataCatalogProvider provider = UnityEngine.Object.FindObjectOfType<ItemInnerDataCatalogProvider>(true);
            if (provider != null)
            {
                IReadOnlyList<ItemDetailListEntry> entries = provider.GetItemList();
                if (entries.Count > 0)
                {
                    ItemDetailViewModel model = provider.GetDetailViewModel(entries[0].itemId);
                    detailPanel.Show(ItemDetailProjectionComposer.Compose(
                        model,
                        ItemDetailProjectionContextKind.CatalogPreview,
                        string.Empty,
                        null,
                        null,
                        null,
                        null,
                        null));
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            BuildRuntimeItemDetailPanelPrefab();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"ItemDetailPanel card layout applied: {ScenePath}");
        }

        public static void BuildRuntimeItemDetailPanelPrefab()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ItemDetailPanelPrefabPath) ?? "Assets/_Game/Prefabs/TalismanBag/Items");

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            GameObject prefabRoot = CreateUiObject("ItemDetailPanel", null);
            RectTransform rectTransform = prefabRoot.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(720f, 1280f);
            RebuildDetailPanelContent(prefabRoot, font, includeSandboxAdapters: false);

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, ItemDetailPanelPrefabPath);
            UnityEngine.Object.DestroyImmediate(prefabRoot);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Runtime ItemDetailPanel prefab generated: {ItemDetailPanelPrefabPath}");
        }

        public static void ApplyItemSkillTriggerContractLayout()
        {
            if (!File.Exists(ScenePath))
            {
                throw new FileNotFoundException("Missing Item Sandbox scene.", ScenePath);
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject panel = GameObject.Find("ItemGridPlacementPreviewPanel");
            if (panel == null)
            {
                throw new InvalidOperationException("Missing panel: ItemGridPlacementPreviewPanel");
            }

            ItemSandboxGridPlacementPreviewView previewView = panel.GetComponent<ItemSandboxGridPlacementPreviewView>();
            if (previewView == null)
            {
                throw new InvalidOperationException("Missing ItemSandboxGridPlacementPreviewView on ItemGridPlacementPreviewPanel");
            }

            RemoveNamedChild(panel.transform, "MainBuildSelectionTitleText");
            RemoveNamedChild(panel.transform, "MainBuildSelectionPanel");
            RemoveNamedChild(panel.transform, "SkillMonitorIconTitleText");
            RemoveNamedChild(panel.transform, "SkillMonitorIconGrid");
            SkillMonitorPreviewBindings bindings = CreateSkillMonitorPreviewControls(panel.transform, font);
            MoveSkillMonitorPreviewControlsBeforeStatus(panel.transform);
            previewView.ConfigureSkillMonitorEditor(
                bindings.mainBuildButtons,
                bindings.mainBuildTexts,
                bindings.iconImages,
                bindings.iconTexts);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"ItemSkillTriggerContract01 preview layout applied: {ScenePath}");
        }

        private static void CreateCamera()
        {
            GameObject cameraObject = new("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = PageBackgroundColor;
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 50f;
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        }

        private static void CreateEventSystem()
        {
            GameObject eventSystemObject = new("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        private static void BuildCanvas()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            GameObject canvasObject = CreateUiObject("ItemSandboxCanvas", null);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject pageRoot = CreateUiObject("ItemSandboxRoot", canvasObject.transform);
            Stretch(pageRoot.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image pageImage = pageRoot.AddComponent<Image>();
            pageImage.color = PageBackgroundColor;

            ItemInnerDataCatalogProvider provider = pageRoot.AddComponent<ItemInnerDataCatalogProvider>();
            ItemSandboxDetailUiController controller = pageRoot.AddComponent<ItemSandboxDetailUiController>();
            ItemSandboxGeneratedInstanceProvider generatedProvider = pageRoot.AddComponent<ItemSandboxGeneratedInstanceProvider>();
            generatedProvider.ConfigureEditor(provider);
            ItemBalanceCandidateDetailSandboxProvider candidateProvider =
                pageRoot.AddComponent<ItemBalanceCandidateDetailSandboxProvider>();
            ItemBalanceWorkbenchCatalog balanceCatalog =
                AssetDatabase.LoadAssetAtPath<ItemBalanceWorkbenchCatalog>(ItemBalanceWorkbenchCatalogPath);
            candidateProvider.ConfigureEditor(provider, balanceCatalog);
            IReadOnlyList<ItemDetailListEntry> entries = provider.GetItemList();

            ItemSandboxItemButtonView[] buttonViews = CreateItemList(pageRoot.transform, font, entries);
            GameObject itemListPanel = GameObject.Find("ItemNameListPanel");
            (Button catalogModeButton, Button generatedModeButton) =
                EnsureInstanceModeBar(itemListPanel.transform, font);
            SetButtonLabel(catalogModeButton, "Catalog Preview");
            SetButtonLabel(generatedModeButton, "Candidate Instance Preview");
            CandidatePreviewBindings candidateBindings =
                EnsureCandidatePreviewControls(itemListPanel.transform, font);
            ItemSandboxGridPlacementPreviewView placementPreview = CreatePlacementPreviewPanel(pageRoot.transform, font, provider);
            ItemSandboxDetailPanelView detailPanel = CreateDetailPanel(pageRoot.transform, font);

            if (entries.Count > 0)
            {
                ItemDetailViewModel model = provider.GetDetailViewModel(entries[0].itemId);
                detailPanel.Show(ItemDetailProjectionComposer.Compose(
                    model,
                    ItemDetailProjectionContextKind.CatalogPreview,
                    string.Empty,
                    null,
                    null,
                    null,
                    null,
                    null));
                buttonViews[0].SetSelected(true);
                placementPreview.SelectItem(entries[0].itemId);
            }

            controller.ConfigureEditor(provider, buttonViews, detailPanel, placementPreview);
            controller.ConfigureInstanceModesEditor(candidateProvider, catalogModeButton, generatedModeButton);
            controller.ConfigureCandidatePreviewEditor(
                candidateProvider,
                candidateBindings.root,
                candidateBindings.rarityButtons,
                candidateBindings.seedInput,
                candidateBindings.regenerateButton,
                candidateBindings.validationText);
            candidateBindings.root.SetActive(false);
        }

        private static (Button catalogButton, Button generatedButton) EnsureInstanceModeBar(
            Transform itemListPanel,
            Font font)
        {
            if (itemListPanel == null)
            {
                throw new ArgumentNullException(nameof(itemListPanel));
            }

            Transform existing = itemListPanel.Find("ItemDetailContextModeBar");
            if (existing != null)
            {
                Button existingCatalog = existing.Find("CatalogModeButton")?.GetComponent<Button>();
                Button existingGenerated = existing.Find("GeneratedInstancesModeButton")?.GetComponent<Button>();
                if (existingCatalog != null && existingGenerated != null)
                {
                    return (existingCatalog, existingGenerated);
                }

                UnityEngine.Object.DestroyImmediate(existing.gameObject);
            }

            GameObject bar = CreateUiObject("ItemDetailContextModeBar", itemListPanel);
            bar.transform.SetSiblingIndex(Math.Min(1, itemListPanel.childCount - 1));
            HorizontalLayoutGroup layout = bar.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            AddLayout(bar, 0f, 48f, 0f);

            Button catalogButton = CreateInstanceModeButton(
                "CatalogModeButton", bar.transform, "Catalog", font);
            Button generatedButton = CreateInstanceModeButton(
                "GeneratedInstancesModeButton", bar.transform, "Generated Instances", font);
            return (catalogButton, generatedButton);
        }

        private static CandidatePreviewBindings EnsureCandidatePreviewControls(
            Transform itemListPanel,
            Font font)
        {
            Transform existing = itemListPanel.Find("CandidateInstancePreviewControls");
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing.gameObject);
            }

            GameObject root = CreateUiObject("CandidateInstancePreviewControls", itemListPanel);
            root.transform.SetSiblingIndex(Math.Min(2, itemListPanel.childCount - 1));
            VerticalLayoutGroup rootLayout = root.AddComponent<VerticalLayoutGroup>();
            rootLayout.spacing = 6f;
            rootLayout.childAlignment = TextAnchor.UpperCenter;
            rootLayout.childControlWidth = true;
            rootLayout.childControlHeight = true;
            rootLayout.childForceExpandWidth = true;
            rootLayout.childForceExpandHeight = false;
            AddLayout(root, 0f, 138f, 0f);

            GameObject rarityRow = CreateUiObject("CandidateRarityRow", root.transform);
            HorizontalLayoutGroup rarityLayout = rarityRow.AddComponent<HorizontalLayoutGroup>();
            rarityLayout.spacing = 4f;
            rarityLayout.childAlignment = TextAnchor.MiddleCenter;
            rarityLayout.childControlWidth = true;
            rarityLayout.childControlHeight = true;
            rarityLayout.childForceExpandWidth = true;
            rarityLayout.childForceExpandHeight = true;
            AddLayout(rarityRow, 0f, 38f, 0f);

            string[] objectNames = { "WhiteRarityButton", "GreenRarityButton", "BlueRarityButton", "PurpleRarityButton", "OrangeRarityButton" };
            string[] labels = { "凡\nwhite", "良\ngreen", "灵\nblue", "玄\npurple", "道\norange" };
            Button[] rarityButtons = new Button[objectNames.Length];
            for (int index = 0; index < objectNames.Length; index++)
            {
                rarityButtons[index] = CreateInstanceModeButton(
                    objectNames[index], rarityRow.transform, labels[index], font);
            }

            GameObject seedRow = CreateUiObject("CandidateSeedRow", root.transform);
            HorizontalLayoutGroup seedLayout = seedRow.AddComponent<HorizontalLayoutGroup>();
            seedLayout.spacing = 6f;
            seedLayout.childAlignment = TextAnchor.MiddleCenter;
            seedLayout.childControlWidth = true;
            seedLayout.childControlHeight = true;
            seedLayout.childForceExpandWidth = false;
            seedLayout.childForceExpandHeight = true;
            AddLayout(seedRow, 0f, 42f, 0f);

            Text seedLabel = CreateText("SeedLabelText", seedRow.transform, "Seed", font, 14,
                OldPaperSoftColor, TextAnchor.MiddleLeft);
            AddLayout(seedLabel.gameObject, 42f, 42f, 42f);
            InputField seedInput = CreateSeedInput(seedRow.transform, font);
            Button regenerate = CreateInstanceModeButton(
                "RegenerateCandidateButton", seedRow.transform, "重新生成", font);
            AddLayout(regenerate.gameObject, 78f, 42f, 78f);

            Text validation = CreateText(
                "CandidateValidationText",
                root.transform,
                "平衡候选预览 · 只读 · 未接战斗/存档",
                font,
                12,
                DetailMutedTextColor,
                TextAnchor.UpperLeft);
            validation.horizontalOverflow = HorizontalWrapMode.Wrap;
            validation.verticalOverflow = VerticalWrapMode.Overflow;
            AddLayout(validation.gameObject, 0f, 46f, 0f);

            return new CandidatePreviewBindings(
                root, rarityButtons, seedInput, regenerate, validation);
        }

        private static InputField CreateSeedInput(Transform parent, Font font)
        {
            GameObject inputObject = CreateUiObject("CandidateSeedInput", parent);
            Image background = inputObject.AddComponent<Image>();
            background.color = new Color(0.12f, 0.09f, 0.065f, 0.96f);
            InputField input = inputObject.AddComponent<InputField>();
            input.contentType = InputField.ContentType.IntegerNumber;
            input.lineType = InputField.LineType.SingleLine;
            AddLayout(inputObject, 112f, 42f, 112f);

            Text placeholder = CreateText("Placeholder", inputObject.transform, "40412001", font, 13,
                DetailMutedTextColor, TextAnchor.MiddleLeft);
            Text value = CreateText("Text", inputObject.transform,
                ItemBalanceCandidateDetailSandboxProvider.DefaultRootSeed.ToString(CultureInfo.InvariantCulture),
                font, 13, DetailTextColor, TextAnchor.MiddleLeft);
            Stretch(placeholder.rectTransform, Vector2.zero, Vector2.one, new Vector2(8f, 0f), new Vector2(-8f, 0f));
            Stretch(value.rectTransform, Vector2.zero, Vector2.one, new Vector2(8f, 0f), new Vector2(-8f, 0f));
            input.placeholder = placeholder;
            input.textComponent = value;
            input.text = ItemBalanceCandidateDetailSandboxProvider.DefaultRootSeed
                .ToString(CultureInfo.InvariantCulture);
            return input;
        }

        private static void SetButtonLabel(Button button, string label)
        {
            Text text = button != null ? button.GetComponentInChildren<Text>(true) : null;
            if (text != null)
            {
                text.text = label;
            }
        }

        private static Button CreateInstanceModeButton(
            string objectName,
            Transform parent,
            string label,
            Font font)
        {
            GameObject buttonObject = CreateUiObject(objectName, parent);
            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.25f, 0.18f, 0.12f, 0.88f);
            Button button = buttonObject.AddComponent<Button>();
            Text text = CreateText("ModeLabelText", buttonObject.transform, label, font, 14,
                OldPaperSoftColor, TextAnchor.MiddleCenter);
            text.fontStyle = FontStyle.Bold;
            Stretch(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(4f, 0f), new Vector2(-4f, 0f));
            return button;
        }

        private sealed class CandidatePreviewBindings
        {
            public CandidatePreviewBindings(
                GameObject root,
                Button[] rarityButtons,
                InputField seedInput,
                Button regenerateButton,
                Text validationText)
            {
                this.root = root;
                this.rarityButtons = rarityButtons;
                this.seedInput = seedInput;
                this.regenerateButton = regenerateButton;
                this.validationText = validationText;
            }

            public GameObject root { get; }
            public Button[] rarityButtons { get; }
            public InputField seedInput { get; }
            public Button regenerateButton { get; }
            public Text validationText { get; }
        }

        private static ItemSandboxItemButtonView[] CreateItemList(
            Transform parent,
            Font font,
            IReadOnlyList<ItemDetailListEntry> entries)
        {
            GameObject panel = CreateUiObject("ItemNameListPanel", parent);
            Stretch(
                panel.GetComponent<RectTransform>(),
                ItemListAnchorMin,
                ItemListAnchorMax,
                Vector2.zero,
                Vector2.zero);
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = DarkPanelColor;
            Outline outline = panel.AddComponent<Outline>();
            outline.effectColor = new Color(0.06f, 0.04f, 0.03f, 0.85f);
            outline.effectDistance = new Vector2(2f, -2f);

            VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(22, 22, 20, 20);
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            Text title = CreateText("PanelTitleText", panel.transform, "道具名录", font, 30, SealColor, TextAnchor.MiddleCenter);
            title.fontStyle = FontStyle.Bold;
            AddLayout(title.gameObject, 0f, 46f, 0f);

            ItemSandboxItemButtonView[] buttonViews = new ItemSandboxItemButtonView[entries.Count];
            for (int i = 0; i < entries.Count; i++)
            {
                ItemDetailListEntry entry = entries[i];
                GameObject buttonObject = CreateUiObject($"ItemNameButton_{i:00}", panel.transform);
                Image buttonImage = buttonObject.AddComponent<Image>();
                buttonImage.color = new Color(0.72f, 0.55f, 0.34f, 0.34f);
                Button button = buttonObject.AddComponent<Button>();
                AddLayout(buttonObject, 0f, 58f, 0f);

                Text nameText = CreateText("NameText", buttonObject.transform, entry.displayItemName, font, 24, OldPaperSoftColor, TextAnchor.MiddleCenter);
                nameText.fontStyle = FontStyle.Bold;
                Stretch(nameText.rectTransform, Vector2.zero, Vector2.one, new Vector2(12f, 0f), new Vector2(-12f, 0f));

                ItemSandboxItemButtonView buttonView = buttonObject.AddComponent<ItemSandboxItemButtonView>();
                buttonView.ConfigureEditor(entry.itemId, nameText, button, buttonImage);
                buttonViews[i] = buttonView;
            }

            return buttonViews;
        }

        private static ItemSandboxGridPlacementPreviewView CreatePlacementPreviewPanel(
            Transform parent,
            Font font,
            ItemInnerDataCatalogProvider provider)
        {
            GameObject panel = CreateUiObject("ItemGridPlacementPreviewPanel", parent);
            Stretch(
                panel.GetComponent<RectTransform>(),
                PlacementAnchorMin,
                PlacementAnchorMax,
                Vector2.zero,
                Vector2.zero);
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.19f, 0.13f, 0.09f, 0.96f);
            Outline outline = panel.AddComponent<Outline>();
            outline.effectColor = new Color(0.06f, 0.04f, 0.03f, 0.85f);
            outline.effectDistance = new Vector2(2f, -2f);

            VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 20, 20);
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            Text title = CreateText("PlacementTitleText", panel.transform, "5x5 Placement Preview", font, 28, OldPaperSoftColor, TextAnchor.MiddleCenter);
            title.fontStyle = FontStyle.Bold;
            AddLayout(title.gameObject, 0f, 42f, 0f);

            GameObject board = CreateUiObject("PlacementBoardGrid", panel.transform);
            AddLayout(board, 330f, 330f, 0f);
            GridLayoutGroup gridLayout = board.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(58f, 58f);
            gridLayout.spacing = new Vector2(6f, 6f);
            gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.childAlignment = TextAnchor.MiddleCenter;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = ItemGridPlacementRulePreview.BoardSize;

            List<ItemSandboxGridCellView> cellViews = new();
            for (int y = ItemGridPlacementRulePreview.BoardSize - 1; y >= 0; y--)
            {
                for (int x = 0; x < ItemGridPlacementRulePreview.BoardSize; x++)
                {
                    GameObject cellObject = CreateUiObject($"PlacementCell_{x}_{y}", board.transform);
                    Image cellImage = cellObject.AddComponent<Image>();
                    cellImage.color = new Color(0.33f, 0.24f, 0.16f, 0.96f);
                    Button button = cellObject.AddComponent<Button>();
                    button.targetGraphic = cellImage;

                    Text label = CreateText("CellLabelText", cellObject.transform, $"{x},{y}", font, 13, OldPaperSoftColor, TextAnchor.MiddleCenter);
                    label.fontStyle = FontStyle.Bold;
                    label.resizeTextForBestFit = true;
                    label.resizeTextMinSize = 9;
                    label.resizeTextMaxSize = 13;
                    label.lineSpacing = 0.85f;
                    Stretch(label.rectTransform, Vector2.zero, Vector2.one, new Vector2(3f, 3f), new Vector2(-3f, -3f));

                    ItemSandboxGridCellView cellView = cellObject.AddComponent<ItemSandboxGridCellView>();
                    cellView.ConfigureEditor(new Vector2Int(x, y), button, cellImage, label);
                    cellViews.Add(cellView);
                }
            }

            Text scenarioTitle = CreateText("LightingScenarioTitleText", panel.transform, "点亮案例", font, 22, OldPaperSoftColor, TextAnchor.MiddleCenter);
            scenarioTitle.fontStyle = FontStyle.Bold;
            AddLayout(scenarioTitle.gameObject, 0f, 30f, 0f);

            GameObject scenarioGrid = CreateUiObject("LightingScenarioButtonGrid", panel.transform);
            AddLayout(scenarioGrid, 330f, 218f, 0f);
            GridLayoutGroup scenarioGridLayout = scenarioGrid.AddComponent<GridLayoutGroup>();
            scenarioGridLayout.cellSize = new Vector2(150f, 36f);
            scenarioGridLayout.spacing = new Vector2(8f, 8f);
            scenarioGridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            scenarioGridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            scenarioGridLayout.childAlignment = TextAnchor.MiddleCenter;
            scenarioGridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            scenarioGridLayout.constraintCount = 2;

            IReadOnlyList<ItemSandboxLightingScenarioDefinition> scenarios = ItemSandboxLightingScenarioCatalog.AllScenarios;
            Button[] scenarioButtons = new Button[scenarios.Count];
            Text[] scenarioButtonTexts = new Text[scenarios.Count];
            for (int i = 0; i < scenarios.Count; i++)
            {
                GameObject buttonObject = CreateUiObject($"LightingScenarioButton_{i:00}", scenarioGrid.transform);
                Image buttonImage = buttonObject.AddComponent<Image>();
                buttonImage.color = i == 0
                    ? new Color(0.64f, 0.48f, 0.22f, 0.82f)
                    : new Color(0.36f, 0.25f, 0.14f, 0.68f);
                Button button = buttonObject.AddComponent<Button>();
                button.targetGraphic = buttonImage;

                Text buttonText = CreateText("ScenarioLabelText", buttonObject.transform, scenarios[i].displayName, font, 14, OldPaperSoftColor, TextAnchor.MiddleCenter);
                buttonText.fontStyle = FontStyle.Bold;
                buttonText.resizeTextForBestFit = true;
                buttonText.resizeTextMinSize = 9;
                buttonText.resizeTextMaxSize = 14;
                Stretch(buttonText.rectTransform, Vector2.zero, Vector2.one, new Vector2(4f, 2f), new Vector2(-4f, -2f));

                scenarioButtons[i] = button;
                scenarioButtonTexts[i] = buttonText;
            }

            SkillMonitorPreviewBindings skillBindings = CreateSkillMonitorPreviewControls(panel.transform, font);

            Text statusText = CreateText("PlacementStatusText", panel.transform, string.Empty, font, 16, OldPaperSoftColor, TextAnchor.UpperLeft);
            statusText.horizontalOverflow = HorizontalWrapMode.Wrap;
            statusText.verticalOverflow = VerticalWrapMode.Overflow;
            AddLayout(statusText.gameObject, 0f, 0f, 1f);

            ItemSandboxGridPlacementPreviewView previewView = panel.AddComponent<ItemSandboxGridPlacementPreviewView>();
            previewView.ConfigureEditor(
                provider,
                cellViews.ToArray(),
                statusText,
                scenarioButtons,
                scenarioButtonTexts,
                skillBindings.mainBuildButtons,
                skillBindings.mainBuildTexts,
                skillBindings.iconImages,
                skillBindings.iconTexts);
            return previewView;
        }

        private static SkillMonitorPreviewBindings CreateSkillMonitorPreviewControls(Transform parent, Font font)
        {
            Text selectorTitle = CreateText("MainBuildSelectionTitleText", parent, "主Build选择 / Sandbox Preview", font, 20, OldPaperSoftColor, TextAnchor.MiddleCenter);
            selectorTitle.fontStyle = FontStyle.Bold;
            AddLayout(selectorTitle.gameObject, 0f, 30f, 0f);

            GameObject selectorPanel = CreateUiObject("MainBuildSelectionPanel", parent);
            AddLayout(selectorPanel, 330f, 102f, 0f);
            GridLayoutGroup selectorGrid = selectorPanel.AddComponent<GridLayoutGroup>();
            selectorGrid.cellSize = new Vector2(102f, 44f);
            selectorGrid.spacing = new Vector2(8f, 8f);
            selectorGrid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            selectorGrid.startAxis = GridLayoutGroup.Axis.Horizontal;
            selectorGrid.childAlignment = TextAnchor.MiddleCenter;
            selectorGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            selectorGrid.constraintCount = 3;

            IReadOnlyList<string> buildIds = ItemSandboxGridPlacementPreviewView.MainBuildSelectionIds;
            string[] displayNames =
            {
                "None",
                "震雷法",
                "离火法",
                "中岳法",
                "玄水法",
                "太白法"
            };
            Button[] buildButtons = new Button[buildIds.Count];
            Text[] buildTexts = new Text[buildIds.Count];
            for (int i = 0; i < buildIds.Count; i++)
            {
                GameObject buttonObject = CreateUiObject($"MainBuildSelectionButton_{i:00}", selectorPanel.transform);
                Image buttonImage = buttonObject.AddComponent<Image>();
                buttonImage.color = i == 0
                    ? new Color(0.52f, 0.42f, 0.20f, 0.88f)
                    : new Color(0.25f, 0.20f, 0.15f, 0.78f);
                Button button = buttonObject.AddComponent<Button>();
                button.targetGraphic = buttonImage;

                string label = string.IsNullOrWhiteSpace(buildIds[i])
                    ? displayNames[i]
                    : $"{displayNames[i]}\n{buildIds[i]}";
                Text buttonText = CreateText("MainBuildLabelText", buttonObject.transform, label, font, 12, OldPaperSoftColor, TextAnchor.MiddleCenter);
                buttonText.fontStyle = FontStyle.Bold;
                buttonText.resizeTextForBestFit = true;
                buttonText.resizeTextMinSize = 8;
                buttonText.resizeTextMaxSize = 12;
                Stretch(buttonText.rectTransform, Vector2.zero, Vector2.one, new Vector2(3f, 2f), new Vector2(-3f, -2f));

                buildButtons[i] = button;
                buildTexts[i] = buttonText;
            }

            Text iconTitle = CreateText("SkillMonitorIconTitleText", parent, "自动监控位灰盒", font, 20, OldPaperSoftColor, TextAnchor.MiddleCenter);
            iconTitle.fontStyle = FontStyle.Bold;
            AddLayout(iconTitle.gameObject, 0f, 28f, 0f);

            GameObject iconGrid = CreateUiObject("SkillMonitorIconGrid", parent);
            AddLayout(iconGrid, 330f, 96f, 0f);
            GridLayoutGroup iconGridLayout = iconGrid.AddComponent<GridLayoutGroup>();
            iconGridLayout.cellSize = new Vector2(75f, 86f);
            iconGridLayout.spacing = new Vector2(8f, 8f);
            iconGridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            iconGridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            iconGridLayout.childAlignment = TextAnchor.MiddleCenter;
            iconGridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            iconGridLayout.constraintCount = 4;

            string[] iconNames = { "普攻", "Build2", "Build4", "Build6" };
            Image[] iconImages = new Image[iconNames.Length];
            Text[] iconTexts = new Text[iconNames.Length];
            for (int i = 0; i < iconNames.Length; i++)
            {
                GameObject iconObject = CreateUiObject($"SkillMonitorIcon_{i:00}_{NormalizeObjectName(iconNames[i])}", iconGrid.transform);
                Image iconImage = iconObject.AddComponent<Image>();
                iconImage.color = new Color(0.20f, 0.20f, 0.20f, 0.86f);
                Outline iconOutline = iconObject.AddComponent<Outline>();
                iconOutline.effectColor = new Color(0.08f, 0.06f, 0.04f, 0.88f);
                iconOutline.effectDistance = new Vector2(2f, -2f);

                Text iconText = CreateText("SkillMonitorIconText", iconObject.transform, $"{iconNames[i]}\nLocked\ntrigger=false", font, 11, OldPaperSoftColor, TextAnchor.MiddleCenter);
                iconText.resizeTextForBestFit = true;
                iconText.resizeTextMinSize = 7;
                iconText.resizeTextMaxSize = 11;
                iconText.lineSpacing = 0.82f;
                Stretch(iconText.rectTransform, Vector2.zero, Vector2.one, new Vector2(4f, 4f), new Vector2(-4f, -4f));

                iconImages[i] = iconImage;
                iconTexts[i] = iconText;
            }

            return new SkillMonitorPreviewBindings(buildButtons, buildTexts, iconImages, iconTexts);
        }

        private static ItemSandboxDetailPanelView CreateDetailPanel(Transform parent, Font font)
        {
            GameObject card = CreateUiObject("ItemDetailPanel", parent);
            Stretch(
                card.GetComponent<RectTransform>(),
                DetailAnchorMin,
                DetailAnchorMax,
                Vector2.zero,
                Vector2.zero);
            return RebuildDetailPanelContent(card, font);
        }

        private static ItemSandboxDetailPanelView RebuildDetailPanelContent(
            GameObject card,
            Font font,
            bool includeSandboxAdapters = true)
        {
            RemoveChildren(card.transform);

            Image cardImage = EnsureComponent<Image>(card);
            cardImage.color = DetailCardColor;
            Outline cardOutline = EnsureComponent<Outline>(card);
            cardOutline.effectColor = new Color(0.39f, 0.25f, 0.11f, 0.94f);
            cardOutline.effectDistance = new Vector2(2f, -2f);

            VerticalLayoutGroup cardLayout = EnsureComponent<VerticalLayoutGroup>(card);
            cardLayout.padding = new RectOffset(26, 26, 22, 24);
            cardLayout.spacing = 12f;
            cardLayout.childAlignment = TextAnchor.UpperLeft;
            cardLayout.childControlWidth = false;
            cardLayout.childControlHeight = false;
            cardLayout.childForceExpandWidth = false;
            cardLayout.childForceExpandHeight = false;

            ItemDetailPanelView runtimePanel = EnsureComponent<ItemDetailPanelView>(card);
            ItemSandboxDetailPanelView detailPanel = includeSandboxAdapters
                ? EnsureComponent<ItemSandboxDetailPanelView>(card)
                : null;

            GameObject rarityAccent = CreateUiObject("RarityAccentLine", card.transform);
            Image rarityAccentImage = rarityAccent.AddComponent<Image>();
            rarityAccentImage.color = DetailDividerColor;
            AddLayout(rarityAccent, 0f, 5f, 0f);

            GameObject header = CreateUiObject("ItemDetailHeader", card.transform);
            AddLayout(header, 0f, 238f, 0f);
            MakeInspectorAuthorableLayout(header, new Vector2(26f, -277f), new Vector2(-26f, -39f), new Vector2(0f, 1f), new Vector2(1f, 1f));
            HorizontalLayoutGroup headerLayout = header.AddComponent<HorizontalLayoutGroup>();
            headerLayout.spacing = 20f;
            headerLayout.childAlignment = TextAnchor.UpperLeft;
            headerLayout.childControlWidth = true;
            headerLayout.childControlHeight = true;
            headerLayout.childForceExpandWidth = false;
            headerLayout.childForceExpandHeight = false;

            GameObject headerTextGroup = CreateUiObject("ItemDetailHeaderTextGroup", header.transform);
            LayoutElement headerTextLayout = headerTextGroup.AddComponent<LayoutElement>();
            headerTextLayout.flexibleWidth = 1f;
            headerTextLayout.preferredHeight = 234f;
            VerticalLayoutGroup headerTextStack = headerTextGroup.AddComponent<VerticalLayoutGroup>();
            headerTextStack.spacing = 8f;
            headerTextStack.childAlignment = TextAnchor.UpperLeft;
            headerTextStack.childControlWidth = true;
            headerTextStack.childControlHeight = true;
            headerTextStack.childForceExpandWidth = true;
            headerTextStack.childForceExpandHeight = false;

            GameObject titleRow = CreateUiObject("ItemDetailTitleRow", headerTextGroup.transform);
            AddLayout(titleRow, 0f, 72f, 0f);
            HorizontalLayoutGroup titleRowLayout = titleRow.AddComponent<HorizontalLayoutGroup>();
            titleRowLayout.spacing = 12f;
            titleRowLayout.childAlignment = TextAnchor.MiddleLeft;
            titleRowLayout.childControlWidth = true;
            titleRowLayout.childControlHeight = true;
            titleRowLayout.childForceExpandWidth = false;
            titleRowLayout.childForceExpandHeight = true;

            Text itemNameText = CreateText("ItemNameText", titleRow.transform, string.Empty, font, 51, DetailTextColor, TextAnchor.MiddleLeft);
            itemNameText.fontStyle = FontStyle.Bold;
            LayoutElement itemNameLayout = itemNameText.gameObject.AddComponent<LayoutElement>();
            itemNameLayout.flexibleWidth = 1f;
            itemNameLayout.preferredHeight = 72f;

            GameObject rarityBadge = CreateUiObject("RarityBadge", titleRow.transform);
            Image rarityBadgeImage = rarityBadge.AddComponent<Image>();
            rarityBadgeImage.color = new Color(0.58f, 0.43f, 0.24f, 0.30f);
            Outline rarityBadgeOutline = rarityBadge.AddComponent<Outline>();
            rarityBadgeOutline.effectColor = new Color(0.74f, 0.55f, 0.28f, 0.72f);
            rarityBadgeOutline.effectDistance = new Vector2(1f, -1f);
            AddLayout(rarityBadge, 98f, 36f, 0f);
            Text rarityBadgeText = CreateText("RarityBadgeText", rarityBadge.transform, "未定阶", font, 17, DetailTitleColor, TextAnchor.MiddleCenter);
            rarityBadgeText.fontStyle = FontStyle.Bold;
            Stretch(rarityBadgeText.rectTransform, Vector2.zero, Vector2.one, new Vector2(6f, 2f), new Vector2(-6f, -2f));

            Text metaText = CreateText("MetaText", headerTextGroup.transform, string.Empty, font, 20, DetailTextColor, TextAnchor.UpperLeft);
            metaText.supportRichText = true;
            metaText.lineSpacing = 1f;
            AddLayout(metaText.gameObject, 0f, 46f, 0f);

            Text powerText = CreateText("PowerText", headerTextGroup.transform, string.Empty, font, 20, DetailTextColor, TextAnchor.MiddleLeft);
            powerText.fontStyle = FontStyle.Normal;
            powerText.supportRichText = true;
            AddLayout(powerText.gameObject, 0f, 38f, 0f);

            GameObject statusBadgeRow = CreateUiObject("ItemStatusBadgeRow", headerTextGroup.transform);
            AddLayout(statusBadgeRow, 0f, 34f, 0f);
            HorizontalLayoutGroup statusBadgeLayout = statusBadgeRow.AddComponent<HorizontalLayoutGroup>();
            statusBadgeLayout.spacing = 8f;
            statusBadgeLayout.childAlignment = TextAnchor.MiddleLeft;
            statusBadgeLayout.childControlWidth = true;
            statusBadgeLayout.childControlHeight = true;
            statusBadgeLayout.childForceExpandWidth = true;
            statusBadgeLayout.childForceExpandHeight = true;

            Image[] statusBadgeImages = new Image[3];
            Text[] statusBadgeTexts = new Text[3];
            (statusBadgeImages[0], statusBadgeTexts[0]) = CreateStatusBadge("LightingStatusBadge", statusBadgeRow.transform, "尚未入阵", font);
            (statusBadgeImages[1], statusBadgeTexts[1]) = CreateStatusBadge("AwakeningStatusBadge", statusBadgeRow.transform, "待开窍", font);
            (statusBadgeImages[2], statusBadgeTexts[2]) = CreateStatusBadge("ArrayStatusBadge", statusBadgeRow.transform, "未占阵脉", font);

            GameObject artworkFrame = CreateUiObject("ItemArtworkFrame", header.transform);
            Image artworkImage = artworkFrame.AddComponent<Image>();
            artworkImage.color = new Color(0.58f, 0.43f, 0.24f, 0.26f);
            Outline artworkOutline = artworkFrame.AddComponent<Outline>();
            artworkOutline.effectColor = new Color(0.86f, 0.68f, 0.38f, 0.72f);
            artworkOutline.effectDistance = new Vector2(2f, -2f);
            AddLayout(artworkFrame, 210f, 210f, 0f);

            GameObject artworkImageObject = CreateUiObject("ItemArtworkImageSlot", artworkFrame.transform);
            Image artworkImageSlot = artworkImageObject.AddComponent<Image>();
            artworkImageSlot.color = Color.white;
            artworkImageSlot.preserveAspect = true;
            artworkImageSlot.raycastTarget = false;
            artworkImageSlot.enabled = false;
            Stretch(artworkImageObject.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(14f, 38f), new Vector2(-14f, -14f));

            Text artworkText = CreateText("ItemArtworkText", artworkFrame.transform, "符器图", font, 18, DetailTextColor, TextAnchor.MiddleCenter);
            artworkText.fontStyle = FontStyle.Bold;
            Stretch(artworkText.rectTransform, Vector2.zero, Vector2.one, new Vector2(12f, 38f), new Vector2(-12f, -12f));

            GameObject artworkKeyPlate = CreateUiObject("ItemArtworkKeyPlate", artworkFrame.transform);
            Image artworkKeyPlateImage = artworkKeyPlate.AddComponent<Image>();
            artworkKeyPlateImage.color = new Color(0.07f, 0.05f, 0.035f, 0.86f);
            Stretch(artworkKeyPlate.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(8f, 6f), new Vector2(-8f, 34f));
            Text artworkKeyText = CreateText("ItemArtworkKeyText", artworkKeyPlate.transform, "ART SLOT", font, 11, DetailMutedTextColor, TextAnchor.MiddleCenter);
            artworkKeyText.resizeTextForBestFit = true;
            artworkKeyText.resizeTextMinSize = 8;
            artworkKeyText.resizeTextMaxSize = 11;
            Stretch(artworkKeyText.rectTransform, Vector2.zero, Vector2.one, new Vector2(4f, 2f), new Vector2(-4f, -2f));

            GameObject closeButtonObject = CreateUiObject("CloseButton", artworkFrame.transform);
            RectTransform closeButtonRect = closeButtonObject.GetComponent<RectTransform>();
            closeButtonRect.anchorMin = Vector2.one;
            closeButtonRect.anchorMax = Vector2.one;
            closeButtonRect.pivot = Vector2.one;
            closeButtonRect.sizeDelta = new Vector2(36f, 36f);
            closeButtonRect.anchoredPosition = new Vector2(-4f, -4f);
            Image closeButtonImage = closeButtonObject.AddComponent<Image>();
            closeButtonImage.color = new Color(0.46f, 0.13f, 0.09f, 0.96f);
            Outline closeButtonOutline = closeButtonObject.AddComponent<Outline>();
            closeButtonOutline.effectColor = new Color(0.88f, 0.66f, 0.35f, 0.78f);
            closeButtonOutline.effectDistance = new Vector2(1f, -1f);
            Button closeButton = closeButtonObject.AddComponent<Button>();
            closeButton.targetGraphic = closeButtonImage;
            Text closeButtonText = CreateText("CloseButtonText", closeButtonObject.transform, "X", font, 19, DetailTextColor, TextAnchor.MiddleCenter);
            closeButtonText.fontStyle = FontStyle.Bold;
            Stretch(closeButtonText.rectTransform, Vector2.zero, Vector2.one, new Vector2(3f, 2f), new Vector2(-3f, -2f));

            GameObject tabBar = CreateUiObject("ItemDetailTabBar", card.transform);
            AddLayout(tabBar, 0f, 46f, 0f);
            MakeInspectorAuthorableLayout(tabBar, new Vector2(26f, -335f), new Vector2(-26f, -289f), new Vector2(0f, 1f), new Vector2(1f, 1f));
            HorizontalLayoutGroup tabLayout = tabBar.AddComponent<HorizontalLayoutGroup>();
            tabLayout.spacing = 10f;
            tabLayout.childAlignment = TextAnchor.MiddleLeft;
            tabLayout.childControlWidth = true;
            tabLayout.childControlHeight = true;
            tabLayout.childForceExpandWidth = false;
            tabLayout.childForceExpandHeight = true;

            (Button detailTabButton, Text detailTabText) = CreateTabButton("DetailTabButton", tabBar.transform, "详情", font, true);
            (Button debugTabButton, Text debugTabText) = CreateTabButton("DebugTabButton", tabBar.transform, "调试", font, false);

            Text scrollHintText = CreateText("ScrollHintText", tabBar.transform, "上滑查看更多", font, 14, DetailMutedTextColor, TextAnchor.MiddleRight);
            LayoutElement scrollHintLayout = scrollHintText.gameObject.AddComponent<LayoutElement>();
            scrollHintLayout.minWidth = 120f;
            scrollHintLayout.flexibleWidth = 1f;
            scrollHintLayout.preferredHeight = 40f;

            string[] playerSectionNames =
            {
                "HeaderSection",
                "CoreIdentitySection",
                "CurrentStateSection",
                "BaseStatsSection",
                "TriggerConditionSection",
                "BasicEffectSection",
                "CoreAwakeningSection",
                "FaMenBuildSection",
                "QiLeiBuildSection",
                "MainBuildMonitorSection",
                "FixedAffixSection",
                "RandomAffixSection",
                "OrangeGrowthSection",
                "PlacementHintSection",
                "FlavorSection"
            };
            string[] playerSectionTitles =
            {
                "道具头部",
                "核心身份",
                "当前状态",
                "基础属性",
                "触发条件",
                "基础效果",
                "核心效果/开窍",
                "法门Build",
                "器类Build",
                "主Build自动监控",
                "固定词条",
                "随机词条",
                "橙色专属/成长路径",
                "摆放提示",
                "文化描述"
            };
            ScrollSectionSet detailSet = CreateScrollSectionSet(
                "ItemDetailScrollView",
                "DetailContent",
                card.transform,
                font,
                playerSectionNames,
                playerSectionTitles,
                includeSandboxAdapters);
            MakeInspectorAuthorableLayout(detailSet.scrollObject, new Vector2(26f, 24f), new Vector2(-26f, -347f), new Vector2(0f, 0f), new Vector2(1f, 1f));

            string[] debugSectionNames =
            {
                "DebugIdentitySection",
                "DebugStateSection",
                "DebugValidationSection"
            };
            string[] debugSectionTitles =
            {
                "调试 / 身份与坐标",
                "调试 / 状态快照",
                "调试 / validationErrors"
            };
            ScrollSectionSet debugSet = CreateScrollSectionSet(
                "ItemDebugScrollView",
                "DebugContent",
                card.transform,
                font,
                debugSectionNames,
                debugSectionTitles,
                includeSandboxAdapters);
            MakeInspectorAuthorableLayout(debugSet.scrollObject, new Vector2(26f, 24f), new Vector2(-26f, -347f), new Vector2(0f, 0f), new Vector2(1f, 1f));
            debugSet.scrollObject.SetActive(false);

            runtimePanel.ConfigureEditor(
                itemNameText,
                metaText,
                powerText,
                artworkText,
                artworkImage,
                artworkImageSlot,
                artworkKeyText,
                rarityBadgeImage,
                rarityBadgeText,
                statusBadgeImages,
                statusBadgeTexts,
                closeButton,
                rarityAccentImage,
                cardImage,
                detailSet.sections,
                debugSet.sections,
                detailTabButton,
                debugTabButton,
                detailTabText,
                debugTabText,
                detailSet.scrollObject,
                debugSet.scrollObject,
                detailSet.scrollRect,
                debugSet.scrollRect);
            if (detailPanel != null)
            {
                detailPanel.ConfigureEditor(runtimePanel);
            }

            return detailPanel;
        }

        private static (Button button, Text label) CreateTabButton(
            string objectName,
            Transform parent,
            string labelText,
            Font font,
            bool active)
        {
            GameObject buttonObject = CreateUiObject(objectName, parent);
            Image image = buttonObject.AddComponent<Image>();
            image.color = active
                ? new Color(0.58f, 0.42f, 0.16f, 0.92f)
                : new Color(0.22f, 0.17f, 0.13f, 0.78f);
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            AddLayout(buttonObject, 126f, 40f, 0f);

            Text label = CreateText("TabLabelText", buttonObject.transform, labelText, font, 19, active ? new Color(0.96f, 0.64f, 0.28f, 1f) : DetailTextColor, TextAnchor.MiddleCenter);
            label.fontStyle = active ? FontStyle.Bold : FontStyle.Normal;
            Stretch(label.rectTransform, Vector2.zero, Vector2.one, new Vector2(8f, 2f), new Vector2(-8f, -2f));
            return (button, label);
        }

        private static (Image image, Text label) CreateStatusBadge(
            string objectName,
            Transform parent,
            string labelText,
            Font font)
        {
            GameObject badge = CreateUiObject(objectName, parent);
            Image image = badge.AddComponent<Image>();
            image.color = new Color(0.33f, 0.30f, 0.25f, 0.90f);
            Outline outline = badge.AddComponent<Outline>();
            outline.effectColor = new Color(0.06f, 0.045f, 0.03f, 0.75f);
            outline.effectDistance = new Vector2(1f, -1f);
            LayoutElement badgeLayout = badge.AddComponent<LayoutElement>();
            badgeLayout.minWidth = 82f;
            badgeLayout.preferredHeight = 30f;
            badgeLayout.flexibleWidth = 1f;

            Text label = CreateText("StatusBadgeText", badge.transform, labelText, font, 14, DetailTextColor, TextAnchor.MiddleCenter);
            label.fontStyle = FontStyle.Bold;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 10;
            label.resizeTextMaxSize = 14;
            Stretch(label.rectTransform, Vector2.zero, Vector2.one, new Vector2(5f, 2f), new Vector2(-5f, -2f));
            return (image, label);
        }

        private static ScrollSectionSet CreateScrollSectionSet(
            string scrollObjectName,
            string contentObjectName,
            Transform parent,
            Font font,
            IReadOnlyList<string> sectionNames,
            IReadOnlyList<string> sectionTitles,
            bool includeSandboxAdapters)
        {
            GameObject scrollObject = CreateUiObject(scrollObjectName, parent);
            AddLayout(scrollObject, 0f, 0f, 1f);
            ScrollRect scrollRect = scrollObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.scrollSensitivity = 42f;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.inertia = true;
            scrollRect.decelerationRate = 0.12f;

            GameObject viewport = CreateUiObject("Viewport", scrollObject.transform);
            Stretch(viewport.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-18f, 0f));
            viewport.AddComponent<RectMask2D>();

            GameObject content = CreateUiObject(contentObjectName, viewport.transform);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = Vector2.zero;

            VerticalLayoutGroup contentLayout = content.AddComponent<VerticalLayoutGroup>();
            contentLayout.padding = new RectOffset(0, 0, 0, 10);
            contentLayout.spacing = 12f;
            contentLayout.childAlignment = TextAnchor.UpperLeft;
            contentLayout.childControlWidth = false;
            contentLayout.childControlHeight = false;
            contentLayout.childForceExpandWidth = false;
            contentLayout.childForceExpandHeight = false;
            ContentSizeFitter contentFitter = content.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            scrollRect.content = contentRect;

            GameObject scrollbarObject = CreateUiObject("VerticalScrollbar", scrollObject.transform);
            RectTransform scrollbarRect = scrollbarObject.GetComponent<RectTransform>();
            Stretch(
                scrollbarRect,
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(-12f, 6f),
                new Vector2(-2f, -6f));
            Image scrollbarTrack = scrollbarObject.AddComponent<Image>();
            scrollbarTrack.color = DetailScrollbarColor;

            GameObject slidingArea = CreateUiObject("SlidingArea", scrollbarObject.transform);
            Stretch(slidingArea.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(1f, 3f), new Vector2(-1f, -3f));
            GameObject handle = CreateUiObject("Handle", slidingArea.transform);
            Stretch(handle.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image handleImage = handle.AddComponent<Image>();
            handleImage.color = DetailScrollbarHandleColor;

            Scrollbar scrollbar = scrollbarObject.AddComponent<Scrollbar>();
            scrollbar.targetGraphic = handleImage;
            scrollbar.handleRect = handle.GetComponent<RectTransform>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            scrollRect.verticalScrollbar = scrollbar;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            scrollRect.verticalScrollbarSpacing = 4f;

            ItemDetailSectionView[] sections = new ItemDetailSectionView[sectionNames.Count];
            for (int i = 0; i < sectionNames.Count; i++)
            {
                if (TryGetChapter(sectionNames[i], out string chapterIndex, out string chapterTitle))
                {
                    CreateChapterBand($"Chapter_{sectionNames[i]}", chapterIndex, chapterTitle, content.transform, font);
                }

                sections[i] = CreateSection(sectionNames[i], sectionTitles[i], content.transform, font, includeSandboxAdapters);
            }

            return new ScrollSectionSet(scrollObject, scrollRect, sections);
        }

        private static bool TryGetChapter(string sectionName, out string chapterIndex, out string chapterTitle)
        {
            chapterIndex = string.Empty;
            chapterTitle = string.Empty;
            switch (sectionName)
            {
                case "HeaderSection":
                    chapterIndex = "壹";
                    chapterTitle = "符器鉴定";
                    return true;
                case "CurrentStateSection":
                    chapterIndex = "贰";
                    chapterTitle = "生效状态";
                    return true;
                case "CoreAwakeningSection":
                    chapterIndex = "叁";
                    chapterTitle = "核心与构筑";
                    return true;
                case "FixedAffixSection":
                    chapterIndex = "肆";
                    chapterTitle = "词条与成长";
                    return true;
                case "PlacementHintSection":
                    chapterIndex = "伍";
                    chapterTitle = "摆放与旧物";
                    return true;
                case "DebugIdentitySection":
                    chapterIndex = "DEV";
                    chapterTitle = "开发者快照";
                    return true;
                default:
                    return false;
            }
        }

        private static void CreateChapterBand(
            string objectName,
            string chapterIndex,
            string chapterTitle,
            Transform parent,
            Font font)
        {
            GameObject band = CreateUiObject(objectName, parent);
            Image bandImage = band.AddComponent<Image>();
            bandImage.color = DetailChapterColor;
            bandImage.raycastTarget = false;
            AddLayout(band, 0f, 36f, 0f);

            HorizontalLayoutGroup layout = band.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(5, 12, 4, 4);
            layout.spacing = 10f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            GameObject indexBadge = CreateUiObject("ChapterIndexBadge", band.transform);
            Image indexImage = indexBadge.AddComponent<Image>();
            indexImage.color = DetailChapterSealColor;
            indexImage.raycastTarget = false;
            AddLayout(indexBadge, chapterIndex == "DEV" ? 48f : 32f, 28f, 0f);
            Text indexText = CreateText("ChapterIndexText", indexBadge.transform, chapterIndex, font, chapterIndex == "DEV" ? 11 : 16, DetailTextColor, TextAnchor.MiddleCenter);
            indexText.fontStyle = FontStyle.Bold;
            Stretch(indexText.rectTransform, Vector2.zero, Vector2.one, new Vector2(3f, 1f), new Vector2(-3f, -1f));

            Text titleText = CreateText("ChapterTitleText", band.transform, chapterTitle, font, 17, DetailTitleColor, TextAnchor.MiddleLeft);
            titleText.fontStyle = FontStyle.Bold;
            LayoutElement titleLayout = titleText.gameObject.AddComponent<LayoutElement>();
            titleLayout.flexibleWidth = 1f;
            titleLayout.preferredHeight = 28f;
        }

        private static ItemDetailSectionView CreateSection(
            string objectName,
            string title,
            Transform parent,
            Font font,
            bool includeSandboxAdapter)
        {
            GameObject section = CreateUiObject(objectName, parent);
            Image sectionImage = section.AddComponent<Image>();
            sectionImage.color = DetailSectionColor;
            sectionImage.raycastTarget = false;
            Outline sectionOutline = section.AddComponent<Outline>();
            sectionOutline.effectColor = new Color(0.35f, 0.25f, 0.14f, 0.48f);
            sectionOutline.effectDistance = new Vector2(1f, -1f);
            VerticalLayoutGroup layout = section.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(22, 18, 12, 14);
            layout.spacing = 7f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            ContentSizeFitter fitter = section.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            GameObject accent = CreateUiObject("SectionAccent", section.transform);
            Image accentImage = accent.AddComponent<Image>();
            accentImage.color = DetailDividerColor;
            accentImage.raycastTarget = false;
            LayoutElement accentLayout = accent.AddComponent<LayoutElement>();
            accentLayout.ignoreLayout = true;
            Stretch(
                accent.GetComponent<RectTransform>(),
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
                new Vector2(0f, 0f),
                new Vector2(4f, 0f));

            Text titleText = CreateText("TitleText", section.transform, title, font, 23, DetailTitleColor, TextAnchor.MiddleLeft);
            titleText.fontStyle = FontStyle.Bold;
            titleText.supportRichText = true;
            AddLayout(titleText.gameObject, 0f, 32f, 0f);

            CreateDivider("SectionDivider", section.transform);

            Text bodyText = CreateText("BodyText", section.transform, string.Empty, font, 20, DetailTextColor, TextAnchor.UpperLeft);
            bodyText.supportRichText = true;
            bodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
            bodyText.verticalOverflow = VerticalWrapMode.Overflow;
            bodyText.lineSpacing = 1.12f;

            ItemDetailSectionView runtimeSection = section.AddComponent<ItemDetailSectionView>();
            runtimeSection.ConfigureEditor(titleText, bodyText, sectionImage, accentImage);
            if (includeSandboxAdapter)
            {
                ItemSandboxDetailSectionView sandboxSection = section.AddComponent<ItemSandboxDetailSectionView>();
                sandboxSection.ConfigureEditor(runtimeSection);
            }

            return runtimeSection;
        }

        private static GameObject CreateDivider(string objectName, Transform parent)
        {
            GameObject divider = CreateUiObject(objectName, parent);
            Image image = divider.AddComponent<Image>();
            image.color = DetailDividerColor;
            image.raycastTarget = false;
            AddLayout(divider, 0f, 2f, 0f);
            return divider;
        }

        private static void RemoveNamedChild(Transform parent, string childName)
        {
            if (parent == null || string.IsNullOrWhiteSpace(childName))
            {
                return;
            }

            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (child != null && child.name == childName)
                {
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
                }
            }
        }

        private static void MoveSkillMonitorPreviewControlsBeforeStatus(Transform parent)
        {
            Transform status = parent.Find("PlacementStatusText");
            if (status == null)
            {
                return;
            }

            string[] names =
            {
                "MainBuildSelectionTitleText",
                "MainBuildSelectionPanel",
                "SkillMonitorIconTitleText",
                "SkillMonitorIconGrid"
            };
            int targetIndex = status.GetSiblingIndex();
            foreach (string name in names)
            {
                Transform child = parent.Find(name);
                if (child == null)
                {
                    continue;
                }

                child.SetSiblingIndex(targetIndex);
                targetIndex++;
            }
        }

        private static string NormalizeObjectName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Slot";
            }

            return value switch
            {
                "普攻" => "BasicAttack",
                "Build2" => "Build2",
                "Build4" => "Build4",
                "Build6" => "Build6",
                _ => "Slot"
            };
        }

        private static Text CreateText(
            string objectName,
            Transform parent,
            string text,
            Font font,
            int size,
            Color color,
            TextAnchor alignment)
        {
            GameObject textObject = CreateUiObject(objectName, parent);
            Text textComponent = textObject.AddComponent<Text>();
            textComponent.text = text;
            textComponent.font = font;
            textComponent.fontSize = size;
            textComponent.color = color;
            textComponent.alignment = alignment;
            textComponent.raycastTarget = false;
            textComponent.supportRichText = true;
            textComponent.horizontalOverflow = HorizontalWrapMode.Wrap;
            textComponent.verticalOverflow = VerticalWrapMode.Overflow;
            return textComponent;
        }

        private static GameObject CreateUiObject(string objectName, Transform parent)
        {
            GameObject gameObject = new(objectName, typeof(RectTransform));
            int uiLayer = LayerMask.NameToLayer("UI");
            if (uiLayer >= 0)
            {
                gameObject.layer = uiLayer;
            }

            if (parent != null)
            {
                gameObject.transform.SetParent(parent, false);
            }

            return gameObject;
        }

        private static void Stretch(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
        }

        private static void RemoveChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(parent.GetChild(i).gameObject);
            }
        }

        private static T EnsureComponent<T>(GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }

        private static void ValidateManualPanelExists(string objectName)
        {
            if (GameObject.Find(objectName) == null)
            {
                throw new InvalidOperationException($"Missing panel: {objectName}");
            }
        }

        private static void AddLayout(GameObject gameObject, float preferredWidth, float preferredHeight, float flexibleHeight)
        {
            LayoutElement layoutElement = gameObject.AddComponent<LayoutElement>();
            if (preferredWidth > 0f)
            {
                layoutElement.preferredWidth = preferredWidth;
            }

            if (preferredHeight > 0f)
            {
                layoutElement.preferredHeight = preferredHeight;
            }

            layoutElement.flexibleHeight = flexibleHeight;
        }

        private static void MakeInspectorAuthorableLayout(
            GameObject gameObject,
            Vector2 offsetMin,
            Vector2 offsetMax,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            LayoutElement layoutElement = gameObject.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = gameObject.AddComponent<LayoutElement>();
            }

            layoutElement.ignoreLayout = true;

            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = new Vector2(0.5f, anchorMin.y == anchorMax.y && anchorMax.y >= 1f ? 1f : 0.5f);
            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
        }

        private readonly struct SkillMonitorPreviewBindings
        {
            public SkillMonitorPreviewBindings(
                Button[] mainBuildButtons,
                Text[] mainBuildTexts,
                Image[] iconImages,
                Text[] iconTexts)
            {
                this.mainBuildButtons = mainBuildButtons;
                this.mainBuildTexts = mainBuildTexts;
                this.iconImages = iconImages;
                this.iconTexts = iconTexts;
            }

            public readonly Button[] mainBuildButtons;
            public readonly Text[] mainBuildTexts;
            public readonly Image[] iconImages;
            public readonly Text[] iconTexts;
        }

        private readonly struct ScrollSectionSet
        {
            public ScrollSectionSet(GameObject scrollObject, ScrollRect scrollRect, ItemDetailSectionView[] sections)
            {
                this.scrollObject = scrollObject;
                this.scrollRect = scrollRect;
                this.sections = sections;
            }

            public readonly GameObject scrollObject;
            public readonly ScrollRect scrollRect;
            public readonly ItemDetailSectionView[] sections;
        }
    }
}
#endif
