using System.Collections;
using System.Collections.Generic;
using TalismanBag.Combat;
using TalismanBag.Items;
using TalismanBag.Shop;
using TalismanBag.UI;
using TalismanBag.V02.Rewards;
using TalismanBag.V02.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.V03.BattlePrepare
{
    public sealed class V03BattlePrepareInteractionController : MonoBehaviour
    {
        private const float BoardSize = 800f;
        private const float ItemTraySize = 800f;
        private const float MoveSpeed = 9f;
        private static readonly Vector2 NormalPosition = new(0f, -520f);
        private static readonly Vector2 PreparePosition = new(0f, -320f);

        private readonly List<CategoryButtonBinding> categoryButtons = new();
        private AutoCombatController combatController;
        private Canvas rootCanvas;
        private RectTransform safeAreaRoot;
        private RectTransform motionRoot;
        private RectTransform boardFrame;
        private RectTransform itemTrayRoot;
        private RectTransform itemTrayContent;
        private Transform itemTrayTemplateRoot;
        private RectTransform actionBar;
        private GameObject legacyBottomOperationArea;
        private Image itemTrayLockedOverlay;
        private Image prepareDarkOverlay;
        private Text emptyStateText;
        private Text stateButtonText;
        private Text prepareButtonText;
        private Button prepareButton;
        private bool setupComplete;
        private bool prepareStateActive;
        private bool continueStateActive;
        private bool preparedFromFighting;
        private int knownItemViewCount = -1;
        private float nextItemViewScanTime;
        private ItemTrayCategory currentCategory = ItemTrayCategory.All;

        private enum ItemTrayCategory
        {
            All,
            Talisman,
            Tool,
            Material,
            Consumable,
            Special
        }

        private sealed class CategoryButtonBinding
        {
            public ItemTrayCategory Category;
            public Button Button;
            public Text Text;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            TryInstallController();
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            TryInstallController();
        }

        private static void TryInstallController()
        {
            AutoCombatController combat = UnityEngine.Object.FindObjectOfType<AutoCombatController>();
            if (combat == null ||
                combat.GetComponent<V03BattlePrepareInteractionController>() != null ||
                GameObject.Find("V02FormationGridFrame") == null)
            {
                return;
            }

            combat.gameObject.AddComponent<V03BattlePrepareInteractionController>();
        }

        private IEnumerator Start()
        {
            yield return null;
            Setup();
        }

        private void Update()
        {
            if (!setupComplete || motionRoot == null)
            {
                return;
            }

            Vector2 target = prepareStateActive ? PreparePosition : NormalPosition;
            motionRoot.anchoredPosition = Vector2.Lerp(
                motionRoot.anchoredPosition,
                target,
                Mathf.Clamp01(Time.unscaledDeltaTime * MoveSpeed));

            if (continueStateActive && Vector2.Distance(motionRoot.anchoredPosition, NormalPosition) <= 1f)
            {
                CompleteContinueState();
            }

            RefreshTrayItemsIfInventoryChanged();
            RefreshActionButtonState();
        }

        private void OnDestroy()
        {
            if (combatController != null && combatController.IsBattlePreparePaused)
            {
                combatController.EndBattlePreparePause();
            }
        }

        private void Setup()
        {
            if (setupComplete)
            {
                return;
            }

            combatController = GetComponent<AutoCombatController>();
            rootCanvas = UnityEngine.Object.FindObjectOfType<Canvas>();
            GameObject safeRootObject = GameObject.Find("MobileSafeAreaRoot");
            safeAreaRoot = safeRootObject != null ? safeRootObject.transform as RectTransform : rootCanvas != null ? rootCanvas.transform as RectTransform : null;
            GameObject boardObject = GameObject.Find("V02FormationGridFrame");
            boardFrame = boardObject != null ? boardObject.transform as RectTransform : null;

            if (combatController == null || rootCanvas == null || safeAreaRoot == null || boardFrame == null)
            {
                enabled = false;
                return;
            }

            CaptureLegacyBottomOperationArea();
            CreatePrepareDarkOverlay();
            CreateMotionRoot();
            ConfigureBoardFrame();
            CreateItemTray();
            CreateActionBar();
            ReparentLegacyTooltipPanel();
            RedirectInventoryWritersToItemTray();
            MoveExistingItemsIntoTray();
            DestroyLegacyBottomOperationArea();
            RefreshTrayItems();
            RefreshVisualState(true);
            setupComplete = true;
        }

        private void CaptureLegacyBottomOperationArea()
        {
            legacyBottomOperationArea = GameObject.Find("V02BottomOperationArea");
            if (legacyBottomOperationArea != null)
            {
                legacyBottomOperationArea.name = "V02BottomOperationArea_Legacy";
            }
        }

        private void CreatePrepareDarkOverlay()
        {
            GameObject overlay = new("V03BattlePrepareDarkOverlay", typeof(RectTransform), typeof(Image));
            overlay.transform.SetParent(safeAreaRoot, false);
            RectTransform rect = overlay.GetComponent<RectTransform>();
            SetRect(rect, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            prepareDarkOverlay = overlay.GetComponent<Image>();
            prepareDarkOverlay.color = new Color(0f, 0f, 0f, 0.58f);
            prepareDarkOverlay.raycastTarget = true;
            overlay.SetActive(false);
        }

        private void CreateMotionRoot()
        {
            GameObject root = new("V03BattlePrepareMotionRoot", typeof(RectTransform));
            root.transform.SetParent(safeAreaRoot, false);
            motionRoot = root.GetComponent<RectTransform>();
            SetRect(
                motionRoot,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                NormalPosition,
                new Vector2(BoardSize, BoardSize + ItemTraySize));
        }

        private void ConfigureBoardFrame()
        {
            boardFrame.SetParent(motionRoot, false);
            boardFrame.SetAsFirstSibling();
            SetRect(
                boardFrame,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                Vector2.zero,
                new Vector2(BoardSize, BoardSize));

            Image boardImage = boardFrame.GetComponent<Image>();
            if (boardImage != null)
            {
                boardImage.color = new Color(0.075f, 0.09f, 0.078f, 0.98f);
            }

            RectTransform gridSlots = GameObject.Find("GridSlots_5x5")?.transform as RectTransform;
            if (gridSlots != null)
            {
                SetRect(
                    gridSlots,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, -34f),
                    new Vector2(700f, 700f));

                GridLayoutGroup layout = gridSlots.GetComponent<GridLayoutGroup>();
                if (layout != null)
                {
                    layout.cellSize = new Vector2(132f, 132f);
                    layout.spacing = new Vector2(10f, 10f);
                    layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                    layout.constraintCount = 5;
                    layout.childAlignment = TextAnchor.MiddleCenter;
                }
            }

            Text caption = boardFrame.Find("GridCaption")?.GetComponent<Text>();
            if (caption != null)
            {
                caption.text = "阵盘";
                SetRect(
                    caption.rectTransform,
                    new Vector2(0.5f, 0f),
                    new Vector2(0.5f, 0f),
                    new Vector2(0.5f, 0f),
                    new Vector2(0f, 26f),
                    new Vector2(560f, 42f));
            }
        }

        private void CreateItemTray()
        {
            GameObject tray = CreatePanel("V02BottomOperationArea", motionRoot, new Vector2(0f, -BoardSize), new Vector2(ItemTraySize, ItemTraySize), new Color(0.065f, 0.076f, 0.07f, 0.98f));
            itemTrayRoot = tray.GetComponent<RectTransform>();
            Outline outline = tray.AddComponent<Outline>();
            outline.effectColor = new Color(0.42f, 0.76f, 1f, 0.9f);
            outline.effectDistance = new Vector2(4f, -4f);

            Text title = CreateText("ItemTrayTitle", tray.transform, "道具栏", 30, FontStyle.Bold, new Color(0.88f, 0.96f, 1f));
            title.alignment = TextAnchor.MiddleLeft;
            SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -32f), new Vector2(-56f, 44f));

            CreateCategoryTabs(tray.transform);
            CreateItemTrayScroll(tray.transform);
            CreateItemTrayLockedOverlay(tray.transform);
            CreateItemTrayTemplateRoot();
        }

        private void CreateItemTrayTemplateRoot()
        {
            GameObject templateRoot = new("V03ItemTrayTemplates", typeof(RectTransform));
            templateRoot.transform.SetParent(safeAreaRoot, false);
            itemTrayTemplateRoot = templateRoot.transform;
            templateRoot.SetActive(false);
        }

        private void CreateCategoryTabs(Transform parent)
        {
            GameObject tabs = new("ItemTrayCategoryTabs", typeof(RectTransform), typeof(GridLayoutGroup));
            tabs.transform.SetParent(parent, false);
            RectTransform rect = tabs.GetComponent<RectTransform>();
            SetRect(rect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -88f), new Vector2(760f, 54f));

            GridLayoutGroup layout = tabs.GetComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(116f, 48f);
            layout.spacing = new Vector2(10f, 0f);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 6;
            layout.childAlignment = TextAnchor.MiddleCenter;

            AddCategoryButton(tabs.transform, ItemTrayCategory.All, "全部");
            AddCategoryButton(tabs.transform, ItemTrayCategory.Talisman, "符箓");
            AddCategoryButton(tabs.transform, ItemTrayCategory.Tool, "法器");
            AddCategoryButton(tabs.transform, ItemTrayCategory.Material, "材料");
            AddCategoryButton(tabs.transform, ItemTrayCategory.Consumable, "消耗");
            AddCategoryButton(tabs.transform, ItemTrayCategory.Special, "特殊");
        }

        private void AddCategoryButton(Transform parent, ItemTrayCategory category, string label)
        {
            Button button = CreateButton($"ItemTrayTab_{category}", parent, label, new Color(0.18f, 0.22f, 0.24f, 0.95f), 21);
            Text text = button.GetComponentInChildren<Text>(true);
            ItemTrayCategory capturedCategory = category;
            button.onClick.AddListener(() =>
            {
                currentCategory = capturedCategory;
                RefreshTrayItems();
            });

            categoryButtons.Add(new CategoryButtonBinding
            {
                Category = category,
                Button = button,
                Text = text
            });
        }

        private void CreateItemTrayScroll(Transform parent)
        {
            GameObject scrollObject = new("ItemTrayScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scrollObject.transform.SetParent(parent, false);
            RectTransform scrollRect = scrollObject.GetComponent<RectTransform>();
            SetRect(scrollRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -452f), new Vector2(760f, 606f));

            Image scrollImage = scrollObject.GetComponent<Image>();
            scrollImage.color = new Color(0.04f, 0.049f, 0.046f, 0.96f);

            GameObject viewport = new("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scrollObject.transform, false);
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            SetRect(viewportRect, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            viewportRect.offsetMin = new Vector2(14f, 14f);
            viewportRect.offsetMax = new Vector2(-14f, -14f);
            Image viewportImage = viewport.GetComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            GameObject content = new("Content", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            itemTrayContent = content.GetComponent<RectTransform>();
            SetRect(itemTrayContent, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(0f, 0f));

            GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(104f, 104f);
            grid.spacing = new Vector2(14f, 14f);
            grid.padding = new RectOffset(8, 8, 8, 8);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 6;
            grid.childAlignment = TextAnchor.UpperCenter;

            ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect scroll = scrollObject.GetComponent<ScrollRect>();
            scroll.viewport = viewportRect;
            scroll.content = itemTrayContent;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 32f;

            emptyStateText = CreateText("ItemTrayEmptyState", viewport.transform, "当前分类暂无道具", 24, FontStyle.Bold, new Color(0.7f, 0.78f, 0.82f));
            SetRect(emptyStateText.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            emptyStateText.gameObject.SetActive(false);
        }

        private void CreateItemTrayLockedOverlay(Transform parent)
        {
            GameObject overlay = new("ItemTrayBattleLockedOverlay", typeof(RectTransform), typeof(Image));
            overlay.transform.SetParent(parent, false);
            RectTransform rect = overlay.GetComponent<RectTransform>();
            SetRect(rect, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            itemTrayLockedOverlay = overlay.GetComponent<Image>();
            itemTrayLockedOverlay.color = new Color(0f, 0f, 0f, 0.62f);
            itemTrayLockedOverlay.raycastTarget = true;

            Text hint = CreateText("Text", overlay.transform, "战斗中不可调整\n点击「整备」后点亮道具栏", 28, FontStyle.Bold, new Color(0.92f, 0.96f, 1f));
            SetRect(hint.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        }

        private void CreateActionBar()
        {
            GameObject bar = CreatePanel("V03BattleBottomActions", safeAreaRoot, new Vector2(0f, 34f), new Vector2(800f, 92f), new Color(0.045f, 0.052f, 0.055f, 0.94f), TextAnchor.LowerCenter);
            actionBar = bar.GetComponent<RectTransform>();

            GridLayoutGroup layout = bar.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(246f, 78f);
            layout.spacing = new Vector2(16f, 0f);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 3;
            layout.childAlignment = TextAnchor.MiddleCenter;

            Button homeButton = CreateButton("V03BackHomeButton", bar.transform, "回首页", new Color(0.22f, 0.28f, 0.32f), 25);
            homeButton.onClick.AddListener(BackToMainHome);

            Button stateButton = CreateButton("V03BattleStateButton", bar.transform, "挂机中", new Color(0.28f, 0.31f, 0.34f), 25);
            stateButton.interactable = false;
            stateButtonText = stateButton.GetComponentInChildren<Text>(true);

            prepareButton = CreateButton("V03PrepareToggleButton", bar.transform, "整备", new Color(0.5f, 0.32f, 0.16f), 27);
            prepareButton.onClick.AddListener(TogglePrepareState);
            prepareButtonText = prepareButton.GetComponentInChildren<Text>(true);
        }

        private void MoveExistingItemsIntoTray()
        {
            DraggableTalismanItemView[] views = UnityEngine.Object.FindObjectsOfType<DraggableTalismanItemView>(true);
            foreach (DraggableTalismanItemView view in views)
            {
                if (view == null || IsInventoryTemplate(view) || IsPlacedOnBoard(view))
                {
                    continue;
                }

                MoveItemToTray(view);
            }
        }

        private void RedirectInventoryWritersToItemTray()
        {
            if (itemTrayContent == null)
            {
                return;
            }

            V02RewardInventoryAdapter[] rewardAdapters = UnityEngine.Object.FindObjectsOfType<V02RewardInventoryAdapter>(true);
            foreach (V02RewardInventoryAdapter adapter in rewardAdapters)
            {
                adapter?.RedirectInventoryParent(itemTrayContent, itemTrayTemplateRoot);
            }

            ShopControllerV2[] shopControllersV2 = UnityEngine.Object.FindObjectsOfType<ShopControllerV2>(true);
            foreach (ShopControllerV2 shopController in shopControllersV2)
            {
                shopController?.RedirectInventoryParent(itemTrayContent, itemTrayTemplateRoot);
            }

            ShopController[] shopControllers = UnityEngine.Object.FindObjectsOfType<ShopController>(true);
            foreach (ShopController shopController in shopControllers)
            {
                shopController?.RedirectInventoryParent(itemTrayContent, itemTrayTemplateRoot);
            }
        }

        private void DestroyLegacyBottomOperationArea()
        {
            GameObject legacyButtons = GameObject.Find("V02PrimaryActionButtons");
            if (legacyButtons != null)
            {
                legacyButtons.SetActive(false);
            }

            if (legacyBottomOperationArea != null)
            {
                UnityEngine.Object.Destroy(legacyBottomOperationArea);
                legacyBottomOperationArea = null;
            }
        }

        private void TogglePrepareState()
        {
            if (continueStateActive)
            {
                return;
            }

            if (prepareStateActive)
            {
                ExitPrepareState();
                return;
            }

            EnterPrepareState();
        }

        private void EnterPrepareState()
        {
            prepareStateActive = true;
            preparedFromFighting = combatController != null && combatController.CurrentCombatState == TalismanCombatState.Fighting;
            if (preparedFromFighting)
            {
                combatController.BeginBattlePreparePause();
            }

            RefreshTrayItems();
            RefreshVisualState(false);
        }

        private void ExitPrepareState()
        {
            combatController?.CommitBattlePrepareLayout();
            prepareStateActive = false;
            continueStateActive = true;
            RefreshTrayItems();
            RefreshVisualState(false);
        }

        private void CompleteContinueState()
        {
            continueStateActive = false;

            if (preparedFromFighting)
            {
                combatController?.EndBattlePreparePause();
            }
            else if (combatController != null && combatController.CurrentCombatState != TalismanCombatState.Fighting)
            {
                combatController.StartBattle();
            }

            preparedFromFighting = false;
            RefreshTrayItems();
            RefreshVisualState(true);
        }

        private void BackToMainHome()
        {
            if (prepareStateActive || continueStateActive)
            {
                prepareStateActive = false;
                continueStateActive = false;
                if (preparedFromFighting)
                {
                    combatController?.EndBattlePreparePause();
                }

                preparedFromFighting = false;
            }

            SceneManager.LoadScene("Scene_TalismanBag_V03_MainHome");
        }

        private void RefreshVisualState(bool snapMotion)
        {
            if (motionRoot != null && snapMotion)
            {
                motionRoot.anchoredPosition = prepareStateActive ? PreparePosition : NormalPosition;
            }

            if (prepareDarkOverlay != null)
            {
                prepareDarkOverlay.gameObject.SetActive(prepareStateActive || continueStateActive);
                prepareDarkOverlay.transform.SetAsLastSibling();
            }

            if (motionRoot != null)
            {
                motionRoot.SetAsLastSibling();
            }

            if (actionBar != null)
            {
                actionBar.SetAsLastSibling();
            }

            if (itemTrayLockedOverlay != null)
            {
                itemTrayLockedOverlay.gameObject.SetActive(!prepareStateActive);
                itemTrayLockedOverlay.transform.SetAsLastSibling();
            }

            RefreshCategoryButtonState();
            RefreshActionButtonState();
            KeepPopupPanelsOnTop();
        }

        private void ReparentLegacyTooltipPanel()
        {
            V02TalismanTooltipUI tooltipUI = UnityEngine.Object.FindObjectOfType<V02TalismanTooltipUI>(true);
            tooltipUI?.ReparentPanel(safeAreaRoot);
        }

        private static void KeepPopupPanelsOnTop()
        {
            V02TalismanTooltipUI tooltipUI = UnityEngine.Object.FindObjectOfType<V02TalismanTooltipUI>(true);
            tooltipUI?.BringToFront();

            V02DebugPopupController debugPopup = UnityEngine.Object.FindObjectOfType<V02DebugPopupController>(true);
            debugPopup?.BringToFront();
        }

        private void RefreshTrayItems()
        {
            if (itemTrayContent == null)
            {
                return;
            }

            int visibleCount = 0;
            DraggableTalismanItemView[] views = UnityEngine.Object.FindObjectsOfType<DraggableTalismanItemView>(true);
            knownItemViewCount = 0;
            foreach (DraggableTalismanItemView view in views)
            {
                if (IsInventoryTemplate(view))
                {
                    continue;
                }

                knownItemViewCount++;
                if (view == null || view.Definition == null)
                {
                    continue;
                }

                view.SetInventoryDropZone(itemTrayRoot);
                combatController?.RegisterItemView(view);

                if (IsPlacedOnBoard(view))
                {
                    view.gameObject.SetActive(true);
                    continue;
                }

                MoveItemToTray(view);
                bool show = MatchesCategory(view.Definition, currentCategory);
                view.gameObject.SetActive(show);
                if (show)
                {
                    visibleCount++;
                }
            }

            if (emptyStateText != null)
            {
                emptyStateText.gameObject.SetActive(visibleCount == 0);
            }

            RefreshCategoryButtonState();
            LayoutRebuilder.ForceRebuildLayoutImmediate(itemTrayContent);
        }

        private void RefreshTrayItemsIfInventoryChanged()
        {
            if (Time.unscaledTime < nextItemViewScanTime)
            {
                return;
            }

            nextItemViewScanTime = Time.unscaledTime + 0.2f;
            int currentCount = CountInventoryItemViews();
            if (currentCount == knownItemViewCount)
            {
                return;
            }

            RefreshTrayItems();
        }

        private static int CountInventoryItemViews()
        {
            int count = 0;
            DraggableTalismanItemView[] views = UnityEngine.Object.FindObjectsOfType<DraggableTalismanItemView>(true);
            foreach (DraggableTalismanItemView view in views)
            {
                if (!IsInventoryTemplate(view))
                {
                    count++;
                }
            }

            return count;
        }

        private static bool IsInventoryTemplate(DraggableTalismanItemView view)
        {
            if (view == null)
            {
                return true;
            }

            string objectName = view.gameObject.name;
            return !string.IsNullOrWhiteSpace(objectName) && objectName.Contains("Template");
        }

        private void MoveItemToTray(DraggableTalismanItemView view)
        {
            RectTransform rect = view.transform as RectTransform;
            view.transform.SetParent(itemTrayContent, false);
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(104f, 104f);
                rect.localScale = Vector3.one;
            }

            view.CaptureHome();
        }

        private static bool IsPlacedOnBoard(DraggableTalismanItemView view)
        {
            return view.CurrentSlot != null || view.RuntimeItem != null && view.RuntimeItem.isPlaced;
        }

        private static bool MatchesCategory(TalismanItemDefinition definition, ItemTrayCategory category)
        {
            if (definition == null || category == ItemTrayCategory.All)
            {
                return true;
            }

            return category switch
            {
                ItemTrayCategory.Talisman => definition.itemType is TalismanItemType.AttackTalisman or TalismanItemType.ShieldTalisman or TalismanItemType.SupportTalisman,
                ItemTrayCategory.Tool => definition.itemType == TalismanItemType.PassiveTool,
                ItemTrayCategory.Material => definition.itemType == TalismanItemType.SpiritStone || ContainsAny(definition.itemId, "stone", "wood", "material"),
                ItemTrayCategory.Consumable => definition.itemType == TalismanItemType.Pill,
                ItemTrayCategory.Special => ContainsAny(definition.itemId, "seal", "soul", "special"),
                _ => false
            };
        }

        private static bool ContainsAny(string value, params string[] tokens)
        {
            if (string.IsNullOrWhiteSpace(value) || tokens == null)
            {
                return false;
            }

            string normalized = value.ToLowerInvariant();
            foreach (string token in tokens)
            {
                if (!string.IsNullOrWhiteSpace(token) && normalized.Contains(token))
                {
                    return true;
                }
            }

            return false;
        }

        private void RefreshCategoryButtonState()
        {
            foreach (CategoryButtonBinding binding in categoryButtons)
            {
                if (binding?.Button == null)
                {
                    continue;
                }

                bool selected = binding.Category == currentCategory;
                Image image = binding.Button.GetComponent<Image>();
                if (image != null)
                {
                    image.color = selected ? new Color(0.5f, 0.32f, 0.16f, 0.98f) : new Color(0.18f, 0.22f, 0.24f, 0.95f);
                }

                if (binding.Text != null)
                {
                    binding.Text.color = selected ? new Color(1f, 0.92f, 0.74f) : Color.white;
                }
            }
        }

        private void RefreshActionButtonState()
        {
            if (stateButtonText != null)
            {
                if (continueStateActive)
                {
                    stateButtonText.text = "继续中";
                }
                else if (prepareStateActive)
                {
                    stateButtonText.text = "整备中";
                }
                else if (combatController != null && combatController.CurrentCombatState == TalismanCombatState.Fighting)
                {
                    stateButtonText.text = "战斗中";
                }
                else
                {
                    stateButtonText.text = "挂机中";
                }
            }

            if (prepareButtonText != null)
            {
                prepareButtonText.text = continueStateActive ? "继续中" : prepareStateActive ? "继续战斗" : "整备";
            }

            if (prepareButton != null)
            {
                prepareButton.interactable = !continueStateActive;
            }
        }

        private GameObject CreatePanel(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, Color color, TextAnchor anchor = TextAnchor.UpperCenter)
        {
            GameObject panel = new(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            Vector2 anchorVector = AnchorToVector(anchor);
            SetRect(rect, anchorVector, anchorVector, anchorVector, anchoredPosition, size);
            Image image = panel.GetComponent<Image>();
            image.color = color;
            return panel;
        }

        private Button CreateButton(string name, Transform parent, string label, Color color, int fontSize)
        {
            GameObject buttonObject = new(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.GetComponent<Image>();
            image.color = color;
            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            Text text = CreateText("Text", buttonObject.transform, label, fontSize, FontStyle.Bold, Color.white);
            SetRect(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            return button;
        }

        private Text CreateText(string name, Transform parent, string content, int fontSize, FontStyle fontStyle, Color color)
        {
            GameObject textObject = new(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.GetComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            return text;
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
        }

        private static Vector2 AnchorToVector(TextAnchor anchor)
        {
            return anchor switch
            {
                TextAnchor.UpperLeft => new Vector2(0f, 1f),
                TextAnchor.UpperCenter => new Vector2(0.5f, 1f),
                TextAnchor.UpperRight => new Vector2(1f, 1f),
                TextAnchor.MiddleLeft => new Vector2(0f, 0.5f),
                TextAnchor.MiddleCenter => new Vector2(0.5f, 0.5f),
                TextAnchor.MiddleRight => new Vector2(1f, 0.5f),
                TextAnchor.LowerLeft => new Vector2(0f, 0f),
                TextAnchor.LowerCenter => new Vector2(0.5f, 0f),
                TextAnchor.LowerRight => new Vector2(1f, 0f),
                _ => new Vector2(0.5f, 0.5f)
            };
        }
    }
}
