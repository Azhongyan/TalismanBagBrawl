using System.Collections.Generic;
using System.Text;
using TalismanBag.V02.CoreLoop.MainTrial;
using TalismanBag.V02.CoreLoop.Resources;
using TalismanBag.V02.CoreLoop.Save;
using TalismanBag.V02.CoreLoop.Upgrades;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.V03.Forge
{
    [ExecuteAlways]
    public sealed class V03TalismanUpgradeSceneController : MonoBehaviour
    {
        private const string DefaultFirstUpgradeItemId = "fire_talisman_basic";
        private const string HomeSceneName = "Scene_TalismanBag_V03_MainHome";
        private const string HomeScenePath = "Assets/_Game/Scenes/Scene_TalismanBag_V03_MainHome.unity";
        private const string TrialSceneName = "Scene_TalismanBag_V02_FormationCounter";
        private const string TrialScenePath = "Assets/_Game/Scenes/Scene_TalismanBag_V02_FormationCounter.unity";
        private const string PageRootName = "V03TalismanUpgradePageRoot";

        private static readonly ResourceType[] ResourceOrder =
        {
            ResourceType.SpiritStone,
            ResourceType.TalismanPaper,
            ResourceType.Cinnabar,
            ResourceType.BasicTalismanEmbryo,
            ResourceType.Cultivation
        };

        [SerializeField] private string firstUpgradeItemId = DefaultFirstUpgradeItemId;

        private readonly List<ItemCardView> itemCards = new();
        private Canvas canvas;
        private RectTransform pageRoot;
        private SaveService saveService;
        private UpgradeService upgradeService;
        private MainTrialFlowService mainTrialFlowService;
        private string selectedItemId;
        private Text resourceText;
        private Text itemNameText;
        private Text levelText;
        private Text beforeText;
        private Text afterText;
        private Text costText;
        private Text statusText;
        private Text upgradeButtonText;
        private Button upgradeButton;
        private GameObject infoPopupRoot;
        private Text popupTitleText;
        private Text popupBodyText;
        private GameObject guideRoot;
        private Text guideSlotText;
        private bool usingEditorPreviewData;

        private void Awake()
        {
            if (!Application.isPlaying)
            {
                BuildEditablePreview();
                return;
            }

            EnsureServices();
            EnsureCanvas();
            EnsureEventSystem();
            BuildPage();
            SelectInitialItem();
            RefreshAll();
        }

        public void BuildEditablePreview()
        {
            if (Application.isPlaying)
            {
                return;
            }

            usingEditorPreviewData = true;
            EnsureCanvas();
            BuildPage();
            SelectInitialItem();
            RefreshEditorPreview();
            usingEditorPreviewData = false;
        }

        private void BuildPage()
        {
            if (pageRoot != null)
            {
                return;
            }

            pageRoot = FindPageRoot();
            if (pageRoot != null && TryBindExistingPage())
            {
                return;
            }

            GameObject root = new(PageRootName, typeof(RectTransform), typeof(Image));
            root.transform.SetParent(canvas.transform, false);
            pageRoot = root.GetComponent<RectTransform>();
            Stretch(pageRoot);
            root.GetComponent<Image>().color = new Color(0.035f, 0.04f, 0.038f, 1f);

            CreateBackgroundSlot(pageRoot);
            CreateTopBar(pageRoot);
            CreatePageHeader(pageRoot);
            CreateDevelopTabBar(pageRoot);
            CreateResourceStrip(pageRoot);
            CreateItemList(pageRoot);
            CreateDetailPanel(pageRoot);
            CreateBottomBar(pageRoot);
            CreateInfoPopup(pageRoot);
            CreateGuideOverlay(pageRoot);
        }

        private RectTransform FindPageRoot()
        {
            if (canvas == null)
            {
                return null;
            }

            Transform found = FindDeepChild(canvas.transform, PageRootName);
            return found != null ? found as RectTransform : null;
        }

        private bool TryBindExistingPage()
        {
            if (pageRoot == null)
            {
                return false;
            }

            resourceText = FindText(pageRoot, "ResourceText");
            itemNameText = FindText(pageRoot, "ItemName");
            levelText = FindText(pageRoot, "ItemLevel");
            beforeText = FindText(FindDeepChild(pageRoot, "BeforeBlock"), "Body");
            afterText = FindText(FindDeepChild(pageRoot, "AfterBlock"), "Body");
            costText = FindText(pageRoot, "CostText");
            statusText = FindText(pageRoot, "StatusText");
            upgradeButton = FindButton(pageRoot, "UpgradeButton");
            upgradeButtonText = upgradeButton != null ? upgradeButton.GetComponentInChildren<Text>(true) : null;
            infoPopupRoot = FindGameObject(pageRoot, "V03Upgrade_ItemInfoPopup");
            popupTitleText = FindText(pageRoot, "PopupTitle");
            popupBodyText = FindText(pageRoot, "PopupBody");
            guideRoot = FindGameObject(pageRoot, "V03Upgrade_GuideOverlay");
            guideSlotText = FindText(FindDeepChild(pageRoot, "V03Upgrade_GuideImageSlot"), "Text");

            BindExistingItemCards();
            BindExistingButtons();

            return resourceText != null &&
                   itemNameText != null &&
                   levelText != null &&
                   beforeText != null &&
                   afterText != null &&
                   costText != null &&
                   statusText != null &&
                   upgradeButton != null &&
                   infoPopupRoot != null &&
                   guideRoot != null &&
                   itemCards.Count > 0;
        }

        private void BindExistingItemCards()
        {
            itemCards.Clear();
            GameObject listPanel = FindGameObject(pageRoot, "V03Upgrade_TalismanListPanel");
            if (listPanel == null)
            {
                return;
            }

            foreach (Transform child in listPanel.transform)
            {
                if (!child.name.StartsWith("TalismanCard_", System.StringComparison.Ordinal))
                {
                    continue;
                }

                Text idText = FindText(child, "ItemId");
                string itemId = idText != null ? idText.text : string.Empty;
                if (string.IsNullOrWhiteSpace(itemId))
                {
                    continue;
                }

                ItemCardView card = new()
                {
                    root = child.gameObject,
                    image = child.GetComponent<Image>(),
                    itemId = itemId.Trim(),
                    titleText = FindText(child, "Name"),
                    levelText = FindText(child, "Level"),
                    idText = idText
                };
                itemCards.Add(card);

                Button button = child.GetComponent<Button>();
                if (Application.isPlaying && button != null)
                {
                    string capturedItemId = card.itemId;
                    button.onClick.AddListener(() => SelectItem(capturedItemId));
                }
            }
        }

        private void BindExistingButtons()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            FindButton(pageRoot, "V03Upgrade_BackHomeButton")?.onClick.AddListener(LoadHome);
            FindButton(pageRoot, "InfoButton")?.onClick.AddListener(ShowInfoPopup);
            FindButton(pageRoot, "PopupCloseButton")?.onClick.AddListener(() => infoPopupRoot.SetActive(false));
            upgradeButton?.onClick.AddListener(UpgradeSelected);

            FindButton(pageRoot, "Tab_Reroll_Locked")?.onClick.AddListener(ShowLockedModuleHint);
            FindButton(pageRoot, "Tab_Synthesis_Locked")?.onClick.AddListener(ShowLockedModuleHint);
            FindButton(pageRoot, "Tab_Forge_Locked")?.onClick.AddListener(ShowLockedModuleHint);
            FindButton(pageRoot, "BottomNav_Home")?.onClick.AddListener(LoadHome);
            FindButton(pageRoot, "BottomNav_Develop")?.onClick.AddListener(() => SetStatus("当前已在养成页。"));
            FindButton(pageRoot, "BottomNav_Trial")?.onClick.AddListener(EnterTrialFromBottomBar);
            FindButton(pageRoot, "BottomNav_Explore")?.onClick.AddListener(ShowLockedModuleHint);
            FindButton(pageRoot, "BottomNav_More")?.onClick.AddListener(ShowLockedModuleHint);
        }

        private void CreateBackgroundSlot(Transform parent)
        {
            GameObject slot = CreatePanel(
                "V03Upgrade_BackgroundImageSlot",
                parent,
                new Color(0.07f, 0.075f, 0.07f, 1f));
            Stretch(slot.GetComponent<RectTransform>());

            Text text = CreateText(
                "Text",
                slot.transform,
                "背景图插槽",
                44,
                FontStyle.Bold,
                new Color(0.35f, 0.38f, 0.34f, 0.72f),
                TextAnchor.MiddleCenter);
            Stretch(text.rectTransform);
        }

        private void CreateTopBar(Transform parent)
        {
            GameObject titleBar = CreatePanel(
                "TopBar_Global",
                parent,
                new Color(0.02f, 0.025f, 0.024f, 0.82f));
            RectTransform rect = titleBar.GetComponent<RectTransform>();
            AnchorTop(rect, new Vector2(0f, 0f), new Vector2(1080f, 170f));

            CreateText(
                "PlayerIdentity",
                titleBar.transform,
                "小满  Lv.12",
                32,
                FontStyle.Bold,
                new Color(0.92f, 0.9f, 0.78f),
                TextAnchor.MiddleLeft,
                new Vector2(-305f, -86f),
                new Vector2(320f, 72f));

            CreateText(
                "TopResourceHint",
                titleBar.transform,
                "灵石  符纸  朱砂",
                25,
                FontStyle.Bold,
                new Color(0.78f, 0.86f, 0.74f),
                TextAnchor.MiddleRight,
                new Vector2(220f, -106f),
                new Vector2(360f, 54f));

            CreateButton(
                "V03Upgrade_BackHomeButton",
                titleBar.transform,
                "返回首页",
                new Vector2(-430f, -86f),
                new Vector2(220f, 76f),
                new Color(0.17f, 0.21f, 0.2f, 0.98f),
                LoadHome);

            CreateButton(
                "V03Upgrade_SettingsButton",
                titleBar.transform,
                "设置",
                new Vector2(430f, -68f),
                new Vector2(140f, 58f),
                new Color(0.13f, 0.16f, 0.15f, 0.9f),
                ShowLockedModuleHint);
        }

        private void CreatePageHeader(Transform parent)
        {
            GameObject header = CreatePanel(
                "PageHeader",
                parent,
                new Color(0.055f, 0.066f, 0.056f, 0.82f));
            RectTransform rect = header.GetComponent<RectTransform>();
            AnchorTop(rect, new Vector2(0f, -188f), new Vector2(960f, 96f));
            AddOutline(header, new Color(0.28f, 0.34f, 0.25f, 0.7f), new Vector2(1.5f, -1.5f));

            CreateText(
                "Title",
                header.transform,
                "符桌",
                38,
                FontStyle.Bold,
                new Color(0.94f, 0.88f, 0.64f),
                TextAnchor.MiddleLeft,
                new Vector2(-210f, -50f),
                new Vector2(240f, 62f));

            CreateText(
                "Subtitle",
                header.transform,
                "符箓升级",
                28,
                FontStyle.Bold,
                new Color(0.78f, 0.86f, 0.74f),
                TextAnchor.MiddleLeft,
                new Vector2(70f, -50f),
                new Vector2(260f, 62f));
        }

        private void CreateDevelopTabBar(Transform parent)
        {
            GameObject tabBar = CreatePanel(
                "DevelopTabBar",
                parent,
                new Color(0.075f, 0.088f, 0.078f, 0.92f));
            RectTransform rect = tabBar.GetComponent<RectTransform>();
            AnchorTop(rect, new Vector2(0f, -294f), new Vector2(960f, 86f));

            CreateButton(
                "Tab_Upgrade_Selected",
                tabBar.transform,
                "升级",
                new Vector2(-330f, -43f),
                new Vector2(180f, 62f),
                new Color(0.32f, 0.25f, 0.12f, 0.98f),
                () => SetStatus("当前已在符箓升级。"));
            CreateButton(
                "Tab_Reroll_Locked",
                tabBar.transform,
                "洗练",
                new Vector2(-110f, -43f),
                new Vector2(180f, 62f),
                new Color(0.12f, 0.14f, 0.13f, 0.9f),
                ShowLockedModuleHint);
            CreateButton(
                "Tab_Synthesis_Locked",
                tabBar.transform,
                "合成",
                new Vector2(110f, -43f),
                new Vector2(180f, 62f),
                new Color(0.12f, 0.14f, 0.13f, 0.9f),
                ShowLockedModuleHint);
            CreateButton(
                "Tab_Forge_Locked",
                tabBar.transform,
                "锻造",
                new Vector2(330f, -43f),
                new Vector2(180f, 62f),
                new Color(0.12f, 0.14f, 0.13f, 0.9f),
                ShowLockedModuleHint);
        }

        private void CreateResourceStrip(Transform parent)
        {
            GameObject strip = CreatePanel(
                "V03Upgrade_ResourceStrip",
                parent,
                new Color(0.11f, 0.13f, 0.12f, 0.96f));
            RectTransform rect = strip.GetComponent<RectTransform>();
            AnchorTop(rect, new Vector2(0f, -396f), new Vector2(960f, 72f));
            AddOutline(strip, new Color(0.35f, 0.4f, 0.33f, 0.85f), new Vector2(2f, -2f));

            resourceText = CreateText(
                "ResourceText",
                strip.transform,
                string.Empty,
                25,
                FontStyle.Bold,
                new Color(0.9f, 0.92f, 0.82f),
                TextAnchor.MiddleCenter);
            Stretch(resourceText.rectTransform);
        }

        private void CreateItemList(Transform parent)
        {
            GameObject panel = CreatePanel(
                "V03Upgrade_TalismanListPanel",
                parent,
                new Color(0.085f, 0.105f, 0.098f, 0.98f));
            RectTransform rect = panel.GetComponent<RectTransform>();
            AnchorTop(rect, new Vector2(-280f, -490f), new Vector2(420f, 1030f));
            AddOutline(panel, new Color(0.45f, 0.52f, 0.42f, 0.85f), new Vector2(2f, -2f));

            CreateText(
                "ListTitle",
                panel.transform,
                "符箓栏",
                34,
                FontStyle.Bold,
                new Color(0.92f, 0.9f, 0.76f),
                TextAnchor.MiddleCenter,
                new Vector2(0f, -58f),
                new Vector2(360f, 62f));

            List<TalismanLevelConfig> rows = BuildTalismanRows();
            for (int i = 0; i < rows.Count; i++)
            {
                TalismanLevelConfig row = rows[i];
                ItemCardView card = CreateItemCard(panel.transform, row, i);
                itemCards.Add(card);
            }
        }

        private void CreateDetailPanel(Transform parent)
        {
            GameObject panel = CreatePanel(
                "V03Upgrade_DetailPanel",
                parent,
                new Color(0.09f, 0.095f, 0.088f, 0.985f));
            RectTransform rect = panel.GetComponent<RectTransform>();
            AnchorTop(rect, new Vector2(245f, -490f), new Vector2(530f, 1030f));
            AddOutline(panel, new Color(0.58f, 0.52f, 0.34f, 0.85f), new Vector2(2f, -2f));

            itemNameText = CreateText(
                "ItemName",
                panel.transform,
                string.Empty,
                38,
                FontStyle.Bold,
                new Color(0.96f, 0.92f, 0.75f),
                TextAnchor.MiddleCenter,
                new Vector2(0f, -65f),
                new Vector2(450f, 68f));

            levelText = CreateText(
                "ItemLevel",
                panel.transform,
                string.Empty,
                25,
                FontStyle.Bold,
                new Color(0.78f, 0.86f, 0.76f),
                TextAnchor.MiddleCenter,
                new Vector2(0f, -126f),
                new Vector2(430f, 48f));

            beforeText = CreateCompareBlock(
                panel.transform,
                "BeforeBlock",
                "升级前",
                new Vector2(-132f, -305f));
            afterText = CreateCompareBlock(
                panel.transform,
                "AfterBlock",
                "升级后",
                new Vector2(132f, -305f));

            costText = CreateText(
                "CostText",
                panel.transform,
                string.Empty,
                24,
                FontStyle.Normal,
                new Color(0.9f, 0.9f, 0.82f),
                TextAnchor.UpperLeft,
                new Vector2(0f, -565f),
                new Vector2(440f, 190f));

            CreateButton(
                "InfoButton",
                panel.transform,
                "道具信息",
                new Vector2(-136f, -815f),
                new Vector2(220f, 76f),
                new Color(0.18f, 0.22f, 0.24f, 0.98f),
                ShowInfoPopup);

            upgradeButton = CreateButton(
                "UpgradeButton",
                panel.transform,
                "升级符箓",
                new Vector2(136f, -815f),
                new Vector2(220f, 76f),
                new Color(0.36f, 0.28f, 0.12f, 0.98f),
                UpgradeSelected);
            upgradeButtonText = upgradeButton.GetComponentInChildren<Text>(true);

            statusText = CreateText(
                "StatusText",
                panel.transform,
                string.Empty,
                25,
                FontStyle.Bold,
                new Color(0.82f, 0.88f, 0.76f),
                TextAnchor.UpperCenter,
                new Vector2(0f, -930f),
                new Vector2(450f, 210f));
        }

        private Text CreateCompareBlock(Transform parent, string objectName, string title, Vector2 position)
        {
            GameObject block = CreatePanel(
                objectName,
                parent,
                new Color(0.06f, 0.07f, 0.066f, 0.98f));
            RectTransform rect = block.GetComponent<RectTransform>();
            AnchorTop(rect, position, new Vector2(238f, 280f));
            AddOutline(block, new Color(0.33f, 0.38f, 0.31f, 0.78f), new Vector2(1.5f, -1.5f));

            CreateText(
                "Title",
                block.transform,
                title,
                26,
                FontStyle.Bold,
                new Color(0.9f, 0.84f, 0.64f),
                TextAnchor.MiddleCenter,
                new Vector2(0f, -38f),
                new Vector2(210f, 48f));

            return CreateText(
                "Body",
                block.transform,
                string.Empty,
                22,
                FontStyle.Normal,
                new Color(0.86f, 0.88f, 0.8f),
                TextAnchor.UpperCenter,
                new Vector2(0f, -98f),
                new Vector2(206f, 164f));
        }

        private ItemCardView CreateItemCard(Transform parent, TalismanLevelConfig row, int index)
        {
            GameObject cardObject = CreatePanel(
                $"TalismanCard_{index + 1:00}",
                parent,
                new Color(0.13f, 0.16f, 0.14f, 0.98f));
            RectTransform rect = cardObject.GetComponent<RectTransform>();
            AnchorTop(rect, new Vector2(0f, -145f - index * 138f), new Vector2(360f, 112f));
            AddOutline(cardObject, new Color(0.34f, 0.42f, 0.34f, 0.8f), new Vector2(1.5f, -1.5f));

            Button button = cardObject.AddComponent<Button>();
            button.targetGraphic = cardObject.GetComponent<Image>();
            string itemId = row.itemId;
            if (Application.isPlaying)
            {
                button.onClick.AddListener(() => SelectItem(itemId));
            }

            Text title = CreateText(
                "Name",
                cardObject.transform,
                row.displayName,
                25,
                FontStyle.Bold,
                new Color(0.95f, 0.91f, 0.72f),
                TextAnchor.MiddleLeft,
                new Vector2(-58f, -32f),
                new Vector2(220f, 42f));

            Text level = CreateText(
                "Level",
                cardObject.transform,
                string.Empty,
                22,
                FontStyle.Bold,
                new Color(0.8f, 0.88f, 0.75f),
                TextAnchor.MiddleRight,
                new Vector2(110f, -32f),
                new Vector2(110f, 42f));

            Text id = CreateText(
                "ItemId",
                cardObject.transform,
                row.itemId,
                17,
                FontStyle.Normal,
                new Color(0.58f, 0.66f, 0.58f),
                TextAnchor.MiddleLeft,
                new Vector2(0f, -76f),
                new Vector2(300f, 34f));

            return new ItemCardView
            {
                root = cardObject,
                image = cardObject.GetComponent<Image>(),
                itemId = itemId,
                titleText = title,
                levelText = level,
                idText = id
            };
        }

        private void CreateInfoPopup(Transform parent)
        {
            infoPopupRoot = new GameObject("V03Upgrade_ItemInfoPopup", typeof(RectTransform), typeof(Image));
            infoPopupRoot.transform.SetParent(parent, false);
            Stretch(infoPopupRoot.GetComponent<RectTransform>());
            Image mask = infoPopupRoot.GetComponent<Image>();
            mask.color = new Color(0f, 0f, 0f, 0.68f);
            mask.raycastTarget = true;

            GameObject panel = CreatePanel(
                "PopupPanel",
                infoPopupRoot.transform,
                new Color(0.08f, 0.095f, 0.088f, 1f));
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            Center(panelRect, Vector2.zero, new Vector2(760f, 780f));
            AddOutline(panel, new Color(0.74f, 0.62f, 0.34f, 0.95f), new Vector2(3f, -3f));

            popupTitleText = CreateText(
                "PopupTitle",
                panel.transform,
                string.Empty,
                38,
                FontStyle.Bold,
                new Color(0.94f, 0.88f, 0.64f),
                TextAnchor.MiddleCenter,
                new Vector2(0f, -70f),
                new Vector2(660f, 80f));

            popupBodyText = CreateText(
                "PopupBody",
                panel.transform,
                string.Empty,
                26,
                FontStyle.Normal,
                new Color(0.88f, 0.9f, 0.82f),
                TextAnchor.UpperLeft,
                new Vector2(0f, -170f),
                new Vector2(620f, 430f));

            CreateButton(
                "PopupCloseButton",
                panel.transform,
                "关闭",
                new Vector2(0f, -650f),
                new Vector2(220f, 78f),
                new Color(0.22f, 0.25f, 0.22f, 0.98f),
                () => infoPopupRoot.SetActive(false));

            infoPopupRoot.SetActive(false);
        }

        private void CreateGuideOverlay(Transform parent)
        {
            guideRoot = new GameObject("V03Upgrade_GuideOverlay", typeof(RectTransform), typeof(CanvasGroup));
            guideRoot.transform.SetParent(parent, false);
            Stretch(guideRoot.GetComponent<RectTransform>());
            CanvasGroup canvasGroup = guideRoot.GetComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            GameObject mask = CreatePanel(
                "BlackMask",
                guideRoot.transform,
                new Color(0f, 0f, 0f, 0.36f));
            Stretch(mask.GetComponent<RectTransform>());
            mask.GetComponent<Image>().raycastTarget = false;

            GameObject slot = CreatePanel(
                "V03Upgrade_GuideImageSlot",
                guideRoot.transform,
                new Color(0.055f, 0.06f, 0.055f, 0.9f));
            RectTransform slotRect = slot.GetComponent<RectTransform>();
            AnchorTop(slotRect, new Vector2(0f, -165f), new Vector2(760f, 350f));
            AddOutline(slot, new Color(0.9f, 0.78f, 0.42f, 0.95f), new Vector2(3f, -3f));
            slot.GetComponent<Image>().raycastTarget = false;

            guideSlotText = CreateText(
                "Text",
                slot.transform,
                "图片插槽占位",
                34,
                FontStyle.Bold,
                new Color(0.92f, 0.86f, 0.66f),
                TextAnchor.MiddleCenter);
            Stretch(guideSlotText.rectTransform);
            guideRoot.SetActive(false);
        }

        private void CreateBottomBar(Transform parent)
        {
            GameObject bottomBar = CreatePanel(
                "BottomBar_Global",
                parent,
                new Color(0.018f, 0.022f, 0.022f, 0.92f));
            RectTransform rect = bottomBar.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 42f);
            rect.sizeDelta = new Vector2(900f, 124f);

            CreateButton(
                "BottomNav_Home",
                bottomBar.transform,
                "首页",
                new Vector2(-336f, -22f),
                new Vector2(146f, 76f),
                new Color(0.12f, 0.15f, 0.14f, 0.95f),
                LoadHome);
            CreateButton(
                "BottomNav_Develop",
                bottomBar.transform,
                "养成",
                new Vector2(-168f, -22f),
                new Vector2(146f, 76f),
                new Color(0.33f, 0.25f, 0.12f, 0.98f),
                () => SetStatus("当前已在养成页。"));
            CreateButton(
                "BottomNav_Trial",
                bottomBar.transform,
                "试炼",
                new Vector2(0f, -22f),
                new Vector2(146f, 76f),
                new Color(0.12f, 0.15f, 0.14f, 0.95f),
                EnterTrialFromBottomBar);
            CreateButton(
                "BottomNav_Explore",
                bottomBar.transform,
                "探索",
                new Vector2(168f, -22f),
                new Vector2(146f, 76f),
                new Color(0.12f, 0.15f, 0.14f, 0.95f),
                ShowLockedModuleHint);
            CreateButton(
                "BottomNav_More",
                bottomBar.transform,
                "更多",
                new Vector2(336f, -22f),
                new Vector2(146f, 76f),
                new Color(0.12f, 0.15f, 0.14f, 0.95f),
                ShowLockedModuleHint);
        }

        private void SelectInitialItem()
        {
            string preferredItemId = ResolveFirstUpgradeItemId();
            List<TalismanLevelConfig> rows = BuildTalismanRows();
            selectedItemId = rows.Count > 0 ? rows[0].itemId : preferredItemId;
            foreach (TalismanLevelConfig row in rows)
            {
                if (string.Equals(row.itemId, preferredItemId, System.StringComparison.Ordinal))
                {
                    selectedItemId = row.itemId;
                    return;
                }
            }
        }

        private void SelectItem(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return;
            }

            selectedItemId = itemId.Trim();
            RefreshAll();
        }

        private void RefreshAll()
        {
            if (!Application.isPlaying)
            {
                RefreshEditorPreview();
                return;
            }

            RefreshResources();
            RefreshItemCards();
            RefreshDetail();
            RefreshGuide();
        }

        private void RefreshEditorPreview()
        {
            List<TalismanLevelConfig> rows = BuildPreviewTalismanRows();
            TalismanLevelConfig row = FindPreviewRow(selectedItemId);
            row ??= rows.Count > 0 ? rows[0] : null;
            selectedItemId = row != null ? row.itemId : ResolveFirstUpgradeItemId();

            SetText(resourceText, "灵石 120    符纸 60    朱砂 10    初阶符胚 1    修为 0");

            foreach (ItemCardView card in itemCards)
            {
                bool selected = string.Equals(card.itemId, selectedItemId, System.StringComparison.Ordinal);
                if (card.image != null)
                {
                    card.image.color = selected
                        ? new Color(0.34f, 0.28f, 0.13f, 0.98f)
                        : new Color(0.13f, 0.16f, 0.14f, 0.98f);
                }

                SetText(card.levelText, "Lv.1");
            }

            SetText(itemNameText, GetDisplayName(row, selectedItemId));
            SetText(levelText, "当前等级 Lv.1");
            SetText(beforeText, "Lv.1\n基础倍率 x1.00");
            SetText(afterText, BuildAfterText(row));
            SetText(costText, BuildCostTextForPreview(row));
            SetText(statusText, "编辑预览：物资满足，可升级。");
            SetText(upgradeButtonText, "升级符箓");

            if (guideRoot != null)
            {
                guideRoot.SetActive(true);
                guideRoot.transform.SetAsLastSibling();
            }

            SetText(guideSlotText, "图片插槽占位");
        }

        private void RefreshResources()
        {
            StringBuilder builder = new();
            for (int i = 0; i < ResourceOrder.Length; i++)
            {
                ResourceType resourceType = ResourceOrder[i];
                if (i > 0)
                {
                    builder.Append("    ");
                }

                builder.Append(GetResourceDisplayName(resourceType));
                builder.Append(' ');
                builder.Append(EnsureUpgradeService().GetResourceAmount(resourceType));
            }

            resourceText.text = builder.ToString();
        }

        private void RefreshItemCards()
        {
            foreach (ItemCardView card in itemCards)
            {
                bool selected = string.Equals(card.itemId, selectedItemId, System.StringComparison.Ordinal);
                int level = EnsureUpgradeService().GetLevel(card.itemId);
                card.image.color = selected
                    ? new Color(0.34f, 0.28f, 0.13f, 0.98f)
                    : new Color(0.13f, 0.16f, 0.14f, 0.98f);
                card.levelText.text = $"Lv.{level}";
            }
        }

        private void RefreshDetail()
        {
            TalismanLevelConfig row = FindConfiguredRow(selectedItemId);
            string displayName = GetDisplayName(row, selectedItemId);
            int currentLevel = EnsureUpgradeService().GetLevel(selectedItemId);
            TalismanLevelConfig nextUpgrade = EnsureUpgradeService().GetNextUpgrade(selectedItemId);

            itemNameText.text = displayName;
            levelText.text = $"当前等级 Lv.{currentLevel}";
            beforeText.text = BuildBeforeText(currentLevel, row);

            if (nextUpgrade == null)
            {
                afterText.text = currentLevel >= 2
                    ? $"Lv.{currentLevel}\n首次升级已完成"
                    : "无可用升级配置";
                costText.text = "消耗\n无";
                statusText.text = currentLevel >= 2
                    ? "首次升级已完成，返回首页后可点击试炼进入 2-1。"
                    : "当前符箓没有可用升级配置。";
                upgradeButton.interactable = currentLevel >= 2;
                upgradeButtonText.text = currentLevel >= 2 ? "返回首页" : "升级符箓";
                return;
            }

            afterText.text = BuildAfterText(nextUpgrade);
            costText.text = BuildCostText(nextUpgrade);
            bool canUpgrade = EnsureUpgradeService().CanUpgrade(selectedItemId, out string failureReason);
            statusText.text = canUpgrade
                ? "物资满足，可升级。"
                : $"物资不足：{failureReason}";
            upgradeButton.interactable = canUpgrade;
            upgradeButtonText.text = "升级符箓";
        }

        private void RefreshGuide()
        {
            MainTrialPhase phase = EnsureMainTrialFlowService().GetCurrentPhase();
            bool shouldGuide = phase == MainTrialPhase.FirstUpgradeRequired ||
                               phase == MainTrialPhase.Chapter1RewardClaimed;
            guideRoot.SetActive(shouldGuide);
            if (shouldGuide)
            {
                guideRoot.transform.SetAsLastSibling();
                guideSlotText.text = "图片插槽占位";
            }
        }

        private void UpgradeSelected()
        {
            if (string.IsNullOrWhiteSpace(selectedItemId))
            {
                return;
            }

            if (EnsureUpgradeService().GetLevel(selectedItemId) >= 2)
            {
                CompleteFirstUpgradeAndReturn(selectedItemId);
                return;
            }

            TalismanUpgradeResult result = EnsureUpgradeService().TryUpgrade(selectedItemId);
            if (result == null || !result.success)
            {
                statusText.text = result != null ? result.message : "升级失败";
                RefreshAll();
                return;
            }

            CompleteFirstUpgradeAndReturn(result.itemId);
        }

        private void ShowInfoPopup()
        {
            TalismanLevelConfig row = FindConfiguredRow(selectedItemId);
            TalismanLevelConfig nextUpgrade = EnsureUpgradeService().GetNextUpgrade(selectedItemId);
            int currentLevel = EnsureUpgradeService().GetLevel(selectedItemId);
            string displayName = GetDisplayName(row, selectedItemId);

            popupTitleText.text = displayName;
            popupBodyText.text = string.Join(
                "\n",
                new[]
                {
                    $"道具ID：{selectedItemId}",
                    $"当前等级：Lv.{currentLevel}",
                    $"升级前：{BuildBeforeText(currentLevel, row).Replace("\n", " / ")}",
                    nextUpgrade != null
                        ? $"升级后：{BuildAfterText(nextUpgrade).Replace("\n", " / ")}"
                        : "升级后：无可用升级配置",
                    nextUpgrade != null
                        ? BuildCostText(nextUpgrade).Replace("\n", " / ")
                        : "消耗：无"
                });
            infoPopupRoot.SetActive(true);
            infoPopupRoot.transform.SetAsLastSibling();
        }

        private void ShowLockedModuleHint()
        {
            SetStatus("后续开放");
        }

        private void EnterTrialFromBottomBar()
        {
            MainTrialPhase phase = EnsureMainTrialFlowService().GetCurrentPhase();
            if (phase == MainTrialPhase.FirstUpgradeRequired ||
                phase == MainTrialPhase.Chapter1RewardClaimed)
            {
                SetStatus("先完成升级符箓，再从首页进入试炼。");
                RefreshGuide();
                return;
            }

            LoadTrial();
        }

        private void SetStatus(string message)
        {
            SetText(statusText, message);
        }

        private void CompleteFirstUpgradeAndReturn(string itemId)
        {
            EnsureMainTrialFlowService().OnFirstUpgradeCompleted(itemId);
            LoadHome();
        }

        private void LoadHome()
        {
            if (UnityEngine.SceneManagement.SceneUtility.GetBuildIndexByScenePath(HomeScenePath) < 0)
            {
                Debug.LogError($"[V0.3-TalismanUpgrade] Home scene is missing from Build Settings: {HomeScenePath}", this);
                return;
            }

            SceneManager.LoadScene(HomeSceneName, LoadSceneMode.Single);
        }

        private void LoadTrial()
        {
            if (UnityEngine.SceneManagement.SceneUtility.GetBuildIndexByScenePath(TrialScenePath) < 0)
            {
                Debug.LogError($"[V0.3-TalismanUpgrade] Trial scene is missing from Build Settings: {TrialScenePath}", this);
                return;
            }

            SceneManager.LoadScene(TrialSceneName, LoadSceneMode.Single);
        }

        private List<TalismanLevelConfig> BuildTalismanRows()
        {
            if (usingEditorPreviewData || !Application.isPlaying)
            {
                return BuildPreviewTalismanRows();
            }

            List<TalismanLevelConfig> rows = new();
            HashSet<string> seen = new();
            IReadOnlyList<TalismanLevelConfig> configuredLevels = EnsureUpgradeService().GetConfiguredLevels();
            if (configuredLevels != null)
            {
                foreach (TalismanLevelConfig level in configuredLevels)
                {
                    if (level == null ||
                        string.IsNullOrWhiteSpace(level.itemId) ||
                        !seen.Add(level.itemId.Trim()))
                    {
                        continue;
                    }

                    rows.Add(level);
                }
            }

            if (rows.Count == 0)
            {
                rows.Add(new TalismanLevelConfig
                {
                    itemId = ResolveFirstUpgradeItemId(),
                    displayName = "火符",
                    fromLevel = 1,
                    toLevel = 2,
                    statModifier = new StatModifier { summary = "配置缺失" }
                });
            }

            return rows;
        }

        private List<TalismanLevelConfig> BuildPreviewTalismanRows()
        {
            TalismanUpgradeConfig config = Resources.Load<TalismanUpgradeConfig>(TalismanUpgradeConfig.DefaultResourcePath);
            if (config != null && config.levels != null && config.levels.Count > 0)
            {
                return new List<TalismanLevelConfig>(config.levels);
            }

            return TalismanUpgradeConfig.BuildDefaultLevels();
        }

        private TalismanLevelConfig FindPreviewRow(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return null;
            }

            List<TalismanLevelConfig> rows = BuildPreviewTalismanRows();
            foreach (TalismanLevelConfig row in rows)
            {
                if (row != null &&
                    string.Equals(row.itemId, itemId, System.StringComparison.Ordinal))
                {
                    return row;
                }
            }

            return null;
        }

        private TalismanLevelConfig FindConfiguredRow(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return null;
            }

            IReadOnlyList<TalismanLevelConfig> configuredLevels = EnsureUpgradeService().GetConfiguredLevels();
            if (configuredLevels == null)
            {
                return null;
            }

            foreach (TalismanLevelConfig level in configuredLevels)
            {
                if (level != null &&
                    string.Equals(level.itemId, itemId, System.StringComparison.Ordinal))
                {
                    return level;
                }
            }

            return null;
        }

        private string BuildBeforeText(int currentLevel, TalismanLevelConfig row)
        {
            if (currentLevel <= 1)
            {
                return "Lv.1\n基础倍率 x1.00";
            }

            return row != null ? BuildAfterText(row) : $"Lv.{currentLevel}\n属性已提升";
        }

        private static string BuildAfterText(TalismanLevelConfig levelConfig)
        {
            if (levelConfig == null)
            {
                return "无可用升级配置";
            }

            List<string> lines = new() { $"Lv.{levelConfig.toLevel}" };
            StatModifier modifier = levelConfig.statModifier;
            if (modifier != null)
            {
                AppendMultiplier(lines, "伤害", modifier.damageMultiplier);
                AppendMultiplier(lines, "冷却", modifier.cooldownMultiplier);
                AppendMultiplier(lines, "护盾", modifier.shieldMultiplier);
                AppendMultiplier(lines, "破盾", modifier.breakShieldMultiplier);
                AppendMultiplier(lines, "压制", modifier.controlDurationMultiplier);
            }

            if (lines.Count == 1)
            {
                lines.Add(GetSummary(levelConfig));
            }

            return string.Join("\n", lines);
        }

        private static void AppendMultiplier(List<string> lines, string label, float value)
        {
            if (Mathf.Approximately(value, 1f))
            {
                return;
            }

            lines.Add($"{label} x{value:0.##}");
        }

        private string BuildCostText(TalismanLevelConfig levelConfig)
        {
            StringBuilder builder = new();
            builder.AppendLine("消耗");
            if (levelConfig?.costs == null || levelConfig.costs.Count == 0)
            {
                builder.Append("无");
                return builder.ToString();
            }

            foreach (ResourceCost cost in levelConfig.costs)
            {
                if (!cost.IsValid)
                {
                    continue;
                }

                int current = EnsureUpgradeService().GetResourceAmount(cost.resourceType);
                builder.AppendLine($"{GetResourceDisplayName(cost.resourceType)} {current}/{cost.amount}");
            }

            return builder.ToString().TrimEnd();
        }

        private static string BuildCostTextForPreview(TalismanLevelConfig levelConfig)
        {
            StringBuilder builder = new();
            builder.AppendLine("消耗");
            if (levelConfig?.costs == null || levelConfig.costs.Count == 0)
            {
                builder.Append("无");
                return builder.ToString();
            }

            foreach (ResourceCost cost in levelConfig.costs)
            {
                if (!cost.IsValid)
                {
                    continue;
                }

                builder.AppendLine($"{GetResourceDisplayName(cost.resourceType)} {GetPreviewResourceAmount(cost.resourceType)}/{cost.amount}");
            }

            return builder.ToString().TrimEnd();
        }

        private static int GetPreviewResourceAmount(ResourceType resourceType)
        {
            return resourceType switch
            {
                ResourceType.SpiritStone => 120,
                ResourceType.TalismanPaper => 60,
                ResourceType.Cinnabar => 10,
                ResourceType.BasicTalismanEmbryo => 1,
                ResourceType.Cultivation => 0,
                _ => 0
            };
        }

        private string ResolveFirstUpgradeItemId()
        {
            return string.IsNullOrWhiteSpace(firstUpgradeItemId)
                ? DefaultFirstUpgradeItemId
                : firstUpgradeItemId.Trim();
        }

        private static string GetDisplayName(TalismanLevelConfig row, string itemId)
        {
            if (!string.IsNullOrWhiteSpace(row?.displayName))
            {
                return row.displayName.Trim();
            }

            return string.IsNullOrWhiteSpace(itemId) ? "符箓" : itemId.Trim();
        }

        private static string GetSummary(TalismanLevelConfig row)
        {
            if (!string.IsNullOrWhiteSpace(row?.statModifier?.summary))
            {
                return row.statModifier.summary.Trim();
            }

            return "属性增强";
        }

        private static string GetResourceDisplayName(ResourceType resourceType)
        {
            return resourceType switch
            {
                ResourceType.SpiritStone => "灵石",
                ResourceType.TalismanPaper => "符纸",
                ResourceType.Cinnabar => "朱砂",
                ResourceType.BasicTalismanEmbryo => "初阶符胚",
                ResourceType.Cultivation => "修为",
                _ => resourceType.ToString()
            };
        }

        private void EnsureServices()
        {
            saveService = SaveService.GetOrCreate();
            EnsureUpgradeService();
            EnsureMainTrialFlowService();
        }

        private UpgradeService EnsureUpgradeService()
        {
            if (upgradeService != null)
            {
                return upgradeService;
            }

            upgradeService = FindObjectOfType<UpgradeService>(true);
            if (upgradeService == null)
            {
                GameObject serviceObject = new("V03TalismanUpgradeService_Runtime");
                upgradeService = serviceObject.AddComponent<UpgradeService>();
            }

            upgradeService.Bind(saveService ?? SaveService.GetOrCreate(), null);
            return upgradeService;
        }

        private MainTrialFlowService EnsureMainTrialFlowService()
        {
            if (mainTrialFlowService != null)
            {
                return mainTrialFlowService;
            }

            mainTrialFlowService = FindObjectOfType<MainTrialFlowService>(true);
            if (mainTrialFlowService == null)
            {
                GameObject serviceObject = new("V03MainTrialFlowService_Runtime");
                mainTrialFlowService = serviceObject.AddComponent<MainTrialFlowService>();
            }

            mainTrialFlowService.Bind(saveService ?? SaveService.GetOrCreate());
            return mainTrialFlowService;
        }

        private void EnsureCanvas()
        {
            canvas = FindObjectOfType<Canvas>(true);
            if (canvas == null)
            {
                GameObject canvasObject = new("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            if (canvas.GetComponent<GraphicRaycaster>() == null)
            {
                canvas.gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        private static void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>(true) != null)
            {
                return;
            }

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private static GameObject CreatePanel(string objectName, Transform parent, Color color)
        {
            GameObject panel = new(objectName, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            Image image = panel.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = true;
            return panel;
        }

        private Button CreateButton(
            string objectName,
            Transform parent,
            string label,
            Vector2 position,
            Vector2 size,
            Color color,
            UnityEngine.Events.UnityAction onClick)
        {
            GameObject buttonObject = new(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            AnchorTop(rect, position, size);

            Image image = buttonObject.GetComponent<Image>();
            image.color = color;
            AddOutline(buttonObject, new Color(0.58f, 0.62f, 0.48f, 0.75f), new Vector2(1.5f, -1.5f));

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            if (Application.isPlaying)
            {
                button.onClick.AddListener(onClick);
            }

            Text labelText = CreateText(
                "Text",
                buttonObject.transform,
                label,
                25,
                FontStyle.Bold,
                Color.white,
                TextAnchor.MiddleCenter);
            Stretch(labelText.rectTransform);
            return button;
        }

        private static void SetText(Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }

        private static GameObject FindGameObject(Transform parent, string objectName)
        {
            Transform found = FindDeepChild(parent, objectName);
            return found != null ? found.gameObject : null;
        }

        private static Button FindButton(Transform parent, string objectName)
        {
            Transform found = FindDeepChild(parent, objectName);
            return found != null ? found.GetComponent<Button>() : null;
        }

        private static Text FindText(Transform parent, string objectName)
        {
            Transform found = FindDeepChild(parent, objectName);
            return found != null ? found.GetComponent<Text>() : null;
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

        private static Text CreateText(
            string objectName,
            Transform parent,
            string value,
            int fontSize,
            FontStyle style,
            Color color,
            TextAnchor alignment)
        {
            GameObject textObject = new(objectName, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            text.text = value;
            return text;
        }

        private static Text CreateText(
            string objectName,
            Transform parent,
            string value,
            int fontSize,
            FontStyle style,
            Color color,
            TextAnchor alignment,
            Vector2 position,
            Vector2 size)
        {
            Text text = CreateText(objectName, parent, value, fontSize, style, color, alignment);
            AnchorTop(text.rectTransform, position, size);
            return text;
        }

        private static void AddOutline(GameObject target, Color color, Vector2 distance)
        {
            Outline outline = target.GetComponent<Outline>();
            if (outline == null)
            {
                outline = target.AddComponent<Outline>();
            }

            outline.effectColor = color;
            outline.effectDistance = distance;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
        }

        private static void AnchorTop(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void Center(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private sealed class ItemCardView
        {
            public GameObject root;
            public Image image;
            public string itemId;
            public Text titleText;
            public Text levelText;
            public Text idText;
        }
    }
}
