using System.Collections;
using System.Collections.Generic;
using TalismanBag.Combat;
using TalismanBag.Items;
using TalismanBag.Shop;
using TalismanBag.UI;
using TalismanBag.V02.Rewards;
using TalismanBag.V02.Run;
using TalismanBag.V02.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.V03.BattlePrepare
{
    public sealed class V03BattlePrepareInteractionController : MonoBehaviour
    {
        private const float BoardSize = 800f;
        private const float ItemTraySize = 800f;
        private const float MoveSpeed = 9f;
        private const string RuntimeItemTrayRootName = "V03BattlePrepareItemTrayRoot";
        private const int ItemTrayColumnCount = 5;
        private const int ItemTrayRowCount = 8;
        private const int ItemTraySlotCount = ItemTrayColumnCount * ItemTrayRowCount;
        private static readonly Vector2 ItemTraySlotSize = new(104f, 104f);
        private static readonly Vector2 ItemTraySlotSpacing = new(14f, 14f);
        private static readonly Vector2 NormalPosition = new(0f, -920f);
        private static readonly Vector2 PreparePosition = new(0f, -220f);
        private static IBattlePrepareExtensionSeamProvider pendingExtensionSeamProvider;

        private readonly List<CategoryButtonBinding> categoryButtons = new();
        private readonly List<RectTransform> itemTraySlotRoots = new();
        private AutoCombatController combatController;
        private V02RunFlowController runFlowController;
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
        private Button stateButton;
        private Button prepareButton;
        private Coroutine preparePopupRoutine;
        private bool setupComplete;
        private bool prepareStateActive;
        private bool continueStateActive;
        private bool preparedFromFighting;
        private bool runtimeFallbackReported;
        private int knownItemViewCount = -1;
        private float nextItemViewScanTime;
        private ItemTrayCategory currentCategory = ItemTrayCategory.All;
        private IBattlePrepareExtensionSeamProvider extensionSeamProvider;

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

            Debug.LogWarning("[BattlePrepareRuntimeLock] V03BattlePrepareInteractionController is being added at runtime. This is a temporary fallback until locked BattlePrepare scene nodes are authored.");
            combat.gameObject.AddComponent<V03BattlePrepareInteractionController>();
        }

        public static bool TryOpenPrepareThen(System.Action onOpened)
        {
            V03BattlePrepareInteractionController controller = UnityEngine.Object.FindObjectOfType<V03BattlePrepareInteractionController>(true);
            if (controller == null || !controller.isActiveAndEnabled)
            {
                return false;
            }

            controller.OpenPrepareThen(onOpened);
            return true;
        }

        public static bool TryInjectExtensionSeamProvider(IBattlePrepareExtensionSeamProvider provider)
        {
            if (provider == null || !provider.IsBattlePrepareExtensionEnabled)
            {
                return false;
            }

            pendingExtensionSeamProvider = provider;
            V03BattlePrepareInteractionController controller =
                UnityEngine.Object.FindObjectOfType<V03BattlePrepareInteractionController>(true);
            if (controller == null || !controller.isActiveAndEnabled)
            {
                return false;
            }

            controller.SetExtensionSeamProvider(provider);
            return true;
        }

        public static void ClearExtensionSeamProvider(IBattlePrepareExtensionSeamProvider provider = null)
        {
            if (provider == null || ReferenceEquals(pendingExtensionSeamProvider, provider))
            {
                pendingExtensionSeamProvider = null;
            }

            V03BattlePrepareInteractionController[] controllers =
                UnityEngine.Object.FindObjectsOfType<V03BattlePrepareInteractionController>(true);
            foreach (V03BattlePrepareInteractionController controller in controllers)
            {
                if (controller == null)
                {
                    continue;
                }

                if (provider == null || ReferenceEquals(controller.extensionSeamProvider, provider))
                {
                    controller.SetExtensionSeamProvider(null);
                }
            }
        }

        public RectTransform BoardFrame => boardFrame;
        public RectTransform ItemTrayRoot => itemTrayRoot;
        public RectTransform ItemTrayContent => itemTrayContent;
        public IReadOnlyList<RectTransform> ItemTraySlots => itemTraySlotRoots;
        public Canvas RootCanvas => rootCanvas;
        public RectTransform MotionRoot => motionRoot;
        public bool IsPrepareStateActive => prepareStateActive;

        public void SetExtensionSeamProvider(IBattlePrepareExtensionSeamProvider provider)
        {
            extensionSeamProvider =
                provider != null && provider.IsBattlePrepareExtensionEnabled
                    ? provider
                    : null;
        }

        public bool TryResolveBoardSlot(
            Vector2 screenPoint,
            Camera eventCamera,
            out TalismanGridSlotView slot)
        {
            slot = null;
            TalismanGridSlotView[] slots = UnityEngine.Object.FindObjectsOfType<TalismanGridSlotView>(true);
            foreach (TalismanGridSlotView candidate in slots)
            {
                RectTransform rect = candidate != null ? candidate.transform as RectTransform : null;
                if (rect == null ||
                    !RectTransformUtility.RectangleContainsScreenPoint(rect, screenPoint, eventCamera))
                {
                    continue;
                }

                slot = candidate;
                return true;
            }

            return false;
        }

        public bool TryResolveItemTraySlot(
            Vector2 screenPoint,
            Camera eventCamera,
            out RectTransform slot,
            out int slotIndex)
        {
            slot = null;
            slotIndex = -1;
            for (int i = 0; i < itemTraySlotRoots.Count; i++)
            {
                RectTransform candidate = itemTraySlotRoots[i];
                if (candidate == null ||
                    !RectTransformUtility.RectangleContainsScreenPoint(candidate, screenPoint, eventCamera))
                {
                    continue;
                }

                slot = candidate;
                slotIndex = i;
                return true;
            }

            return false;
        }

        public bool TryGetItemTraySlotIndex(RectTransform slot, out int slotIndex)
        {
            slotIndex = itemTraySlotRoots.IndexOf(slot);
            return slotIndex >= 0;
        }

        public bool TryResolveShapeBoardReceiver(
            Vector2 screenPoint,
            Camera eventCamera,
            out object receiver,
            out TalismanGridSlotView slot)
        {
            receiver = null;
            TryResolveBoardSlot(screenPoint, eventCamera, out slot);
            if (!IsExtensionSeamEnabled ||
                extensionSeamProvider.ShapeGridReceiverProvider == null)
            {
                return false;
            }

            Vector2Int gridPosition = slot != null ? slot.GridPosition : default;
            BattlePrepareBoardReceiverContext context = new(
                this,
                screenPoint,
                eventCamera,
                slot,
                gridPosition);
            return extensionSeamProvider.ShapeGridReceiverProvider.TryResolveBoardReceiver(context, out receiver)
                && receiver != null;
        }

        public bool TryResolveShapeItemTrayReceiver(
            Vector2 screenPoint,
            Camera eventCamera,
            out object receiver,
            out RectTransform slot,
            out int slotIndex)
        {
            receiver = null;
            TryResolveItemTraySlot(screenPoint, eventCamera, out slot, out slotIndex);
            if (!IsExtensionSeamEnabled ||
                extensionSeamProvider.ShapeGridReceiverProvider == null)
            {
                return false;
            }

            BattlePrepareItemTrayReceiverContext context = new(
                this,
                screenPoint,
                eventCamera,
                slot,
                slotIndex);
            return extensionSeamProvider.ShapeGridReceiverProvider.TryResolveItemTrayReceiver(context, out receiver)
                && receiver != null;
        }

        private IEnumerator Start()
        {
            yield return null;
            Setup();
        }

        private void OnEnable()
        {
            DraggableTalismanItemView.ItemClicked += HandleExtensionItemClicked;
            DraggableTalismanItemView.ItemDragStarted += HandleExtensionItemDragStarted;
        }

        private void OnDisable()
        {
            DraggableTalismanItemView.ItemClicked -= HandleExtensionItemClicked;
            DraggableTalismanItemView.ItemDragStarted -= HandleExtensionItemDragStarted;
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
            NotifyExtensionPlacementCancelled("controller_destroyed");
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
            runFlowController = UnityEngine.Object.FindObjectOfType<V02RunFlowController>(true);
            rootCanvas = UnityEngine.Object.FindObjectOfType<Canvas>();
            GameObject safeRootObject = GameObject.Find("MobileSafeAreaRoot");
            safeAreaRoot = safeRootObject != null ? safeRootObject.transform as RectTransform : rootCanvas != null ? rootCanvas.transform as RectTransform : null;
            GameObject boardObject = GameObject.Find("V02FormationGridFrame");
            boardFrame = boardObject != null ? boardObject.transform as RectTransform : null;

            if (combatController == null || rootCanvas == null || safeAreaRoot == null || boardFrame == null)
            {
                Debug.LogError("[BattlePrepareRuntimeLock] Missing required BattlePrepare scene objects. Required: AutoCombatController, Canvas, MobileSafeAreaRoot or Canvas, V02FormationGridFrame.");
                enabled = false;
                return;
            }

            ReportRuntimeFallbackIfNeeded();
            CaptureLegacyBottomOperationArea();
            CreatePrepareDarkOverlay();
            CreateMotionRoot();
            ConfigureBoardFrame();
            CreateItemTray();
            CreateActionBar();
            ReparentLegacyTooltipPanel();
            RedirectInventoryWritersToItemTray();
            MoveExistingItemsIntoTray();
            HideLegacyBottomOperationArea();
            RefreshTrayItems();
            RefreshVisualState(true);
            setupComplete = true;
            ApplyPendingExtensionSeamProvider();
        }

        private void ReportRuntimeFallbackIfNeeded()
        {
            if (runtimeFallbackReported)
            {
                return;
            }

            runtimeFallbackReported = true;
            Debug.LogWarning("[BattlePrepareRuntimeLock] Locked BattlePrepare scene nodes are not fully authored yet. Runtime will use the existing transitional shell without destroying legacy objects. Author V03BattlePrepareDarkOverlay, V03BattlePrepareMotionRoot, V03BattlePrepareItemTrayRoot, V03BattleBottomActions, and V03ItemTrayTemplates in scene to remove this fallback.");
        }

        private void CaptureLegacyBottomOperationArea()
        {
            legacyBottomOperationArea = GameObject.Find("V02BottomOperationArea");
        }

        private void CreatePrepareDarkOverlay()
        {
            if (TryBindImage("V03BattlePrepareDarkOverlay", out prepareDarkOverlay))
            {
                prepareDarkOverlay.raycastTarget = false;
                return;
            }

            GameObject overlay = new("V03BattlePrepareDarkOverlay", typeof(RectTransform), typeof(Image));
            overlay.transform.SetParent(safeAreaRoot, false);
            RectTransform rect = overlay.GetComponent<RectTransform>();
            SetRect(rect, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            prepareDarkOverlay = overlay.GetComponent<Image>();
            prepareDarkOverlay.color = new Color(0f, 0f, 0f, 0.58f);
            prepareDarkOverlay.raycastTarget = false;
            overlay.SetActive(false);
        }

        private void CreateMotionRoot()
        {
            if (TryBindRect("V03BattlePrepareMotionRoot", out motionRoot))
            {
                return;
            }

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
            if (boardFrame.parent != motionRoot)
            {
                Debug.LogWarning("[BattlePrepareRuntimeLock] V02FormationGridFrame is still reparented at runtime because no locked V03BattlePrepareMotionRoot scene binding exists. This fallback must be removed after scene authoring.");
                boardFrame.SetParent(motionRoot, true);
            }
        }

        private void CreateItemTray()
        {
            if (TryBindExistingItemTray())
            {
                return;
            }

            GameObject tray = CreatePanel(RuntimeItemTrayRootName, motionRoot, new Vector2(0f, -BoardSize), new Vector2(ItemTraySize, ItemTraySize), new Color(0.065f, 0.076f, 0.07f, 0.98f));
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
            SetRect(scrollRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -158f), new Vector2(760f, 620f));

            Image scrollImage = scrollObject.GetComponent<Image>();
            scrollImage.color = new Color(0.04f, 0.049f, 0.046f, 0.96f);

            GameObject viewport = new("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scrollObject.transform, false);
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            SetRect(viewportRect, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            viewportRect.offsetMin = new Vector2(14f, 14f);
            viewportRect.offsetMax = new Vector2(-42f, -14f);
            Image viewportImage = viewport.GetComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            GameObject content = new("Content", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            itemTrayContent = content.GetComponent<RectTransform>();
            SetRect(itemTrayContent, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(0f, 0f));

            GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>();
            grid.cellSize = ItemTraySlotSize;
            grid.spacing = ItemTraySlotSpacing;
            grid.padding = new RectOffset(8, 8, 8, 8);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = ItemTrayColumnCount;
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

            Scrollbar scrollbar = CreateItemTrayScrollbar(scrollObject.transform);
            scroll.verticalScrollbar = scrollbar;
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;

            for (int i = 0; i < ItemTraySlotCount; i++)
            {
                CreateItemTraySlot(i);
            }

            emptyStateText = CreateText("ItemTrayEmptyState", viewport.transform, "当前分类暂无道具", 24, FontStyle.Bold, new Color(0.7f, 0.78f, 0.82f));
            SetRect(emptyStateText.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            emptyStateText.gameObject.SetActive(false);
        }

        private Scrollbar CreateItemTrayScrollbar(Transform parent)
        {
            GameObject scrollbarObject = new("ItemTrayVerticalScrollbar", typeof(RectTransform), typeof(Image), typeof(Scrollbar));
            scrollbarObject.transform.SetParent(parent, false);
            RectTransform scrollbarRect = scrollbarObject.GetComponent<RectTransform>();
            SetRect(scrollbarRect, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(-13f, 0f), new Vector2(18f, -28f));

            Image track = scrollbarObject.GetComponent<Image>();
            track.color = new Color(0.12f, 0.15f, 0.15f, 0.95f);

            GameObject slidingArea = new("Sliding Area", typeof(RectTransform));
            slidingArea.transform.SetParent(scrollbarObject.transform, false);
            RectTransform slidingRect = slidingArea.GetComponent<RectTransform>();
            SetRect(slidingRect, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            slidingRect.offsetMin = new Vector2(3f, 3f);
            slidingRect.offsetMax = new Vector2(-3f, -3f);

            GameObject handle = new("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(slidingArea.transform, false);
            RectTransform handleRect = handle.GetComponent<RectTransform>();
            SetRect(handleRect, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            Image handleImage = handle.GetComponent<Image>();
            handleImage.color = new Color(0.42f, 0.76f, 1f, 0.92f);

            Scrollbar scrollbar = scrollbarObject.GetComponent<Scrollbar>();
            scrollbar.targetGraphic = handleImage;
            scrollbar.handleRect = handleRect;
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            scrollbar.size = 0.62f;
            scrollbar.value = 1f;
            return scrollbar;
        }

        private void CreateItemTraySlot(int index)
        {
            GameObject slot = new($"ItemTrayGridSlot_{index + 1:00}", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(V03ItemTraySlotDropTarget));
            slot.transform.SetParent(itemTrayContent, false);
            RectTransform rect = slot.GetComponent<RectTransform>();
            rect.sizeDelta = ItemTraySlotSize;

            Image image = slot.GetComponent<Image>();
            image.color = new Color(0.09f, 0.11f, 0.105f, 0.96f);
            image.raycastTarget = true;

            Outline outline = slot.GetComponent<Outline>();
            outline.effectColor = new Color(0.26f, 0.38f, 0.42f, 0.9f);
            outline.effectDistance = new Vector2(2f, -2f);

            slot.GetComponent<V03ItemTraySlotDropTarget>().Bind(this, rect);
            itemTraySlotRoots.Add(rect);
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
            if (TryBindExistingActionBar())
            {
                return;
            }

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

            stateButton = CreateButton("V03BattleStateButton", bar.transform, "继续战斗", new Color(0.28f, 0.31f, 0.34f), 25);
            stateButton.onClick.AddListener(HandleBattleStateButtonClicked);
            stateButtonText = stateButton.GetComponentInChildren<Text>(true);

            prepareButton = CreateButton("V03PrepareToggleButton", bar.transform, "整备", new Color(0.5f, 0.32f, 0.16f), 27);
            prepareButton.onClick.AddListener(HandlePrepareButtonClicked);
            prepareButtonText = prepareButton.GetComponentInChildren<Text>(true);
        }

        private bool TryBindExistingItemTray()
        {
            if (!TryBindRect(RuntimeItemTrayRootName, out itemTrayRoot))
            {
                return false;
            }

            itemTrayContent = FindChildRecursive(itemTrayRoot, "Content") as RectTransform;
            itemTrayTemplateRoot = FindChildRecursive(safeAreaRoot, "V03ItemTrayTemplates");
            itemTrayLockedOverlay = FindChildRecursive(itemTrayRoot, "ItemTrayBattleLockedOverlay")?.GetComponent<Image>();
            emptyStateText = FindChildRecursive(itemTrayRoot, "ItemTrayEmptyState")?.GetComponent<Text>();

            if (itemTrayContent == null)
            {
                Debug.LogError("[BattlePrepareRuntimeLock] Existing V03BattlePrepareItemTrayRoot is missing ItemTrayScroll/Viewport/Content. Runtime will not rewrite the locked tray layout.");
                return true;
            }

            itemTraySlotRoots.Clear();
            RectTransform[] childRects = itemTrayContent.GetComponentsInChildren<RectTransform>(true);
            foreach (RectTransform child in childRects)
            {
                if (child == itemTrayContent || !child.name.StartsWith("ItemTrayGridSlot_"))
                {
                    continue;
                }

                V03ItemTraySlotDropTarget dropTarget = child.GetComponent<V03ItemTraySlotDropTarget>();
                if (dropTarget == null)
                {
                    dropTarget = child.gameObject.AddComponent<V03ItemTraySlotDropTarget>();
                }

                dropTarget.Bind(this, child);
                itemTraySlotRoots.Add(child);
            }

            BindExistingCategoryButton(ItemTrayCategory.All, "ItemTrayTab_All");
            BindExistingCategoryButton(ItemTrayCategory.Talisman, "ItemTrayTab_Talisman");
            BindExistingCategoryButton(ItemTrayCategory.Tool, "ItemTrayTab_Tool");
            BindExistingCategoryButton(ItemTrayCategory.Material, "ItemTrayTab_Material");
            BindExistingCategoryButton(ItemTrayCategory.Consumable, "ItemTrayTab_Consumable");
            BindExistingCategoryButton(ItemTrayCategory.Special, "ItemTrayTab_Special");
            return true;
        }

        private bool TryBindExistingActionBar()
        {
            if (!TryBindRect("V03BattleBottomActions", out actionBar))
            {
                return false;
            }

            Button homeButton = FindChildRecursive(actionBar, "V03BackHomeButton")?.GetComponent<Button>();
            if (homeButton != null)
            {
                homeButton.onClick.AddListener(BackToMainHome);
            }

            stateButton = FindChildRecursive(actionBar, "V03BattleStateButton")?.GetComponent<Button>();
            if (stateButton != null)
            {
                stateButton.onClick.AddListener(HandleBattleStateButtonClicked);
                stateButtonText = stateButton.GetComponentInChildren<Text>(true);
            }

            prepareButton = FindChildRecursive(actionBar, "V03PrepareToggleButton")?.GetComponent<Button>();
            if (prepareButton != null)
            {
                prepareButton.onClick.AddListener(HandlePrepareButtonClicked);
                prepareButtonText = prepareButton.GetComponentInChildren<Text>(true);
            }

            return true;
        }

        private void BindExistingCategoryButton(ItemTrayCategory category, string objectName)
        {
            Button button = FindChildRecursive(itemTrayRoot, objectName)?.GetComponent<Button>();
            if (button == null)
            {
                Debug.LogWarning($"[BattlePrepareRuntimeLock] Missing category button: {objectName}.");
                return;
            }

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

        private void MoveExistingItemsIntoTray()
        {
            if (itemTrayContent == null)
            {
                return;
            }

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

        private void HideLegacyBottomOperationArea()
        {
            GameObject legacyButtons = GameObject.Find("V02PrimaryActionButtons");
            if (legacyButtons != null)
            {
                legacyButtons.SetActive(false);
            }

            if (legacyBottomOperationArea != null)
            {
                legacyBottomOperationArea.SetActive(false);
                legacyBottomOperationArea = null;
            }
        }

        private void HandleBattleStateButtonClicked()
        {
            if (continueStateActive)
            {
                return;
            }

            if (IsCoreLoopComplete())
            {
                return;
            }

            if (prepareStateActive)
            {
                ExitPrepareState();
                return;
            }

            if (combatController != null && combatController.CurrentCombatState == TalismanCombatState.Fighting)
            {
                return;
            }

            V02RunFlowController flowController = ResolveRunFlowController();
            if (flowController != null && flowController.TriggerBottomBattleAction())
            {
                return;
            }

            if (combatController != null && combatController.CurrentCombatState != TalismanCombatState.Fighting)
            {
                combatController.StartBattle();
            }
        }

        private void HandlePrepareButtonClicked()
        {
            if (continueStateActive)
            {
                return;
            }

            if (prepareStateActive)
            {
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
            BattlePreparePlacementCommitContext extensionContext =
                BuildPlacementCommitContext(null, null, isBattlePrepareContinueCommit: true);
            extensionSeamProvider?.PlacementCommitAdapter?.OnBattlePrepareCommitRequested(extensionContext);
            combatController?.CommitBattlePrepareLayout();
            extensionSeamProvider?.PlacementCommitAdapter?.OnBattlePrepareCommitted(extensionContext);
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
                V02RunFlowController flowController = ResolveRunFlowController();
                if (flowController != null && flowController.IsCoreLoopComplete())
                {
                    preparedFromFighting = false;
                    RefreshTrayItems();
                    RefreshVisualState(true);
                    return;
                }

                if (flowController == null || !flowController.TriggerBottomBattleAction())
                {
                    combatController.StartBattle();
                }
            }

            preparedFromFighting = false;
            RefreshTrayItems();
            RefreshVisualState(true);
        }

        private void BackToMainHome()
        {
            NotifyExtensionPlacementCancelled("back_to_main_home");
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

        private void OpenPrepareThen(System.Action onOpened)
        {
            if (preparePopupRoutine != null)
            {
                StopCoroutine(preparePopupRoutine);
            }

            preparePopupRoutine = StartCoroutine(OpenPrepareThenRoutine(onOpened));
        }

        private IEnumerator OpenPrepareThenRoutine(System.Action onOpened)
        {
            if (!setupComplete)
            {
                Setup();
            }

            if (!setupComplete || motionRoot == null)
            {
                preparePopupRoutine = null;
                onOpened?.Invoke();
                yield break;
            }

            while (continueStateActive)
            {
                yield return null;
            }

            if (!prepareStateActive)
            {
                EnterPrepareState();
            }

            while (motionRoot != null && Vector2.Distance(motionRoot.anchoredPosition, PreparePosition) > 1f)
            {
                yield return null;
            }

            preparePopupRoutine = null;
            RefreshVisualState(false);
            onOpened?.Invoke();
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
            }

            if (itemTrayLockedOverlay != null)
            {
                itemTrayLockedOverlay.gameObject.SetActive(!prepareStateActive);
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

        private static bool TryBindRect(string objectName, out RectTransform rect)
        {
            rect = GameObject.Find(objectName)?.transform as RectTransform;
            return rect != null;
        }

        private static bool TryBindImage(string objectName, out Image image)
        {
            image = GameObject.Find(objectName)?.GetComponent<Image>();
            return image != null;
        }

        private static Transform FindChildRecursive(Transform parent, string objectName)
        {
            if (parent == null || string.IsNullOrWhiteSpace(objectName))
            {
                return null;
            }

            if (parent.name == objectName)
            {
                return parent;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform found = FindChildRecursive(parent.GetChild(i), objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private void RefreshTrayItems()
        {
            if (itemTrayContent == null)
            {
                return;
            }

            List<DraggableTalismanItemView> visibleTrayItems = new();
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

                if (view.IsDragging)
                {
                    continue;
                }

                if (IsPlacedOnBoard(view))
                {
                    view.gameObject.SetActive(true);
                    continue;
                }

                bool show = MatchesCategory(view.Definition, currentCategory);
                view.gameObject.SetActive(show);
                if (show)
                {
                    visibleTrayItems.Add(view);
                }
                else
                {
                    MoveItemToTrayStorage(view);
                }
            }

            for (int i = 0; i < visibleTrayItems.Count && i < itemTraySlotRoots.Count; i++)
            {
                MoveItemToTray(visibleTrayItems[i], itemTraySlotRoots[i]);
                visibleTrayItems[i].gameObject.SetActive(true);
            }

            for (int i = itemTraySlotRoots.Count; i < visibleTrayItems.Count; i++)
            {
                visibleTrayItems[i].gameObject.SetActive(false);
                MoveItemToTrayStorage(visibleTrayItems[i]);
            }

            if (emptyStateText != null)
            {
                emptyStateText.gameObject.SetActive(visibleTrayItems.Count == 0);
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
            if (itemTrayContent == null)
            {
                return;
            }

            RectTransform existingSlot = FindCurrentTraySlot(view);
            if (existingSlot != null)
            {
                MoveItemToTray(view, existingSlot);
                return;
            }

            RectTransform emptySlot = FindFirstEmptyTraySlot(null);
            if (emptySlot != null)
            {
                MoveItemToTray(view, emptySlot);
                return;
            }

            RectTransform rect = view.transform as RectTransform;
            view.transform.SetParent(itemTrayContent, false);
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = ItemTraySlotSize;
                rect.localScale = Vector3.one;
            }

            view.CaptureHome();
        }

        private void MoveItemToTrayStorage(DraggableTalismanItemView view)
        {
            if (view == null || itemTrayTemplateRoot == null)
            {
                return;
            }

            view.transform.SetParent(itemTrayTemplateRoot, false);
            view.CaptureHome();
        }

        private void MoveItemToTray(DraggableTalismanItemView view, RectTransform slot)
        {
            if (view == null || slot == null)
            {
                return;
            }

            RectTransform rect = view.transform as RectTransform;
            view.transform.SetParent(slot, false);
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = ItemTraySlotSize - new Vector2(10f, 10f);
                rect.localScale = Vector3.one;
            }

            view.CaptureHome();
        }

        internal void MoveItemWithinTray(DraggableTalismanItemView dragged, RectTransform targetSlot)
        {
            if (dragged == null || targetSlot == null || itemTrayContent == null || !itemTraySlotRoots.Contains(targetSlot))
            {
                return;
            }

            BattlePreparePlacementCommitContext extensionContext =
                BuildPlacementCommitContext(dragged, targetSlot, isBattlePrepareContinueCommit: false);
            if (extensionSeamProvider?.PlacementCommitAdapter?.TryCommitItemTrayPlacement(extensionContext) == true)
            {
                dragged.AcceptInventoryDrop();
                LayoutRebuilder.ForceRebuildLayoutImmediate(itemTrayContent);
                return;
            }

            RectTransform sourceSlot = dragged.DragReturnParent as RectTransform;
            DraggableTalismanItemView occupied = FindTrayItemInSlot(targetSlot, dragged);
            if (occupied != null && sourceSlot != null && sourceSlot != targetSlot && itemTraySlotRoots.Contains(sourceSlot))
            {
                MoveItemToTray(occupied, sourceSlot);
            }
            else if (occupied != null && sourceSlot != targetSlot)
            {
                MoveItemToFirstEmptyTraySlot(occupied, targetSlot);
            }

            MoveItemToTray(dragged, targetSlot);
            dragged.AcceptInventoryDrop();
            LayoutRebuilder.ForceRebuildLayoutImmediate(itemTrayContent);
            extensionSeamProvider?.PlacementCommitAdapter?.OnItemTrayPlacementCommitted(extensionContext);
        }

        private void MoveItemToFirstEmptyTraySlot(DraggableTalismanItemView view, RectTransform excludedSlot)
        {
            RectTransform emptySlot = FindFirstEmptyTraySlot(excludedSlot);
            if (emptySlot != null)
            {
                MoveItemToTray(view, emptySlot);
                return;
            }

            MoveItemToTray(view);
        }

        private RectTransform FindFirstEmptyTraySlot(RectTransform excludedSlot)
        {
            foreach (RectTransform slot in itemTraySlotRoots)
            {
                if (slot == null || slot == excludedSlot || FindTrayItemInSlot(slot, null) != null)
                {
                    continue;
                }

                return slot;
            }

            return null;
        }

        private RectTransform FindCurrentTraySlot(DraggableTalismanItemView view)
        {
            if (view == null)
            {
                return null;
            }

            RectTransform parent = view.transform.parent as RectTransform;
            return parent != null && itemTraySlotRoots.Contains(parent) ? parent : null;
        }

        private static DraggableTalismanItemView FindTrayItemInSlot(RectTransform slot, DraggableTalismanItemView except)
        {
            if (slot == null)
            {
                return null;
            }

            for (int i = 0; i < slot.childCount; i++)
            {
                DraggableTalismanItemView view = slot.GetChild(i).GetComponent<DraggableTalismanItemView>();
                if (view != null && view != except && view.gameObject.activeSelf)
                {
                    return view;
                }
            }

            return null;
        }

        private bool IsExtensionSeamEnabled =>
            extensionSeamProvider != null && extensionSeamProvider.IsBattlePrepareExtensionEnabled;

        private void ApplyPendingExtensionSeamProvider()
        {
            if (pendingExtensionSeamProvider != null &&
                pendingExtensionSeamProvider.IsBattlePrepareExtensionEnabled)
            {
                SetExtensionSeamProvider(pendingExtensionSeamProvider);
            }
        }

        private void HandleExtensionItemClicked(DraggableTalismanItemView view)
        {
            NotifyExtensionDragContext(view, null, "item_clicked");
        }

        private void HandleExtensionItemDragStarted(DraggableTalismanItemView view)
        {
            NotifyExtensionDragContext(view, null, "item_drag_started");
        }

        private void NotifyExtensionDragContext(
            DraggableTalismanItemView view,
            PointerEventData eventData,
            string phase)
        {
            if (!IsExtensionSeamEnabled || view == null)
            {
                return;
            }

            BattlePrepareDragContext context = BuildDragContext(view, eventData, phase);
            if (!TryBuildExtensionPayload(context, out BattlePrepareShapeItemPayload payload))
            {
                return;
            }

            Vector2 screenPoint = eventData != null ? eventData.position : Vector2.zero;
            extensionSeamProvider.GhostPreviewAdapter?.PreviewGhost(
                new BattlePrepareGhostPreviewContext(
                    this,
                    view,
                    screenPoint,
                    context.SourceBoardSlot,
                    context.SourceTraySlot,
                    payload.IsValid,
                    phase,
                    payload.RuntimePayload));
        }

        private BattlePrepareDragContext BuildDragContext(
            DraggableTalismanItemView view,
            PointerEventData eventData,
            string phase)
        {
            RectTransform traySlot = FindCurrentTraySlot(view);
            int traySlotIndex = -1;
            if (traySlot != null)
            {
                TryGetItemTraySlotIndex(traySlot, out traySlotIndex);
            }

            return new BattlePrepareDragContext(
                this,
                view,
                eventData,
                view != null ? view.CurrentSlot : null,
                traySlot,
                traySlotIndex,
                phase);
        }

        private bool TryBuildExtensionPayload(
            BattlePrepareDragContext context,
            out BattlePrepareShapeItemPayload payload)
        {
            payload = default;
            if (!IsExtensionSeamEnabled ||
                extensionSeamProvider.ShapeItemPayloadProvider == null ||
                context == null)
            {
                return false;
            }

            return extensionSeamProvider.ShapeItemPayloadProvider.TryBuildShapeItemPayload(
                context,
                out payload)
                && payload.IsValid;
        }

        private BattlePreparePlacementCommitContext BuildPlacementCommitContext(
            DraggableTalismanItemView itemView,
            RectTransform traySlot,
            bool isBattlePrepareContinueCommit)
        {
            int traySlotIndex = -1;
            if (traySlot != null)
            {
                TryGetItemTraySlotIndex(traySlot, out traySlotIndex);
            }

            object runtimePayload = null;
            if (itemView != null &&
                TryBuildExtensionPayload(
                    BuildDragContext(itemView, null, "placement_commit"),
                    out BattlePrepareShapeItemPayload payload))
            {
                runtimePayload = payload.RuntimePayload;
            }

            return new BattlePreparePlacementCommitContext(
                this,
                itemView,
                itemView != null ? itemView.CurrentSlot : null,
                traySlot,
                traySlotIndex,
                isBattlePrepareContinueCommit,
                runtimePayload);
        }

        private void NotifyExtensionPlacementCancelled(string reason)
        {
            if (!IsExtensionSeamEnabled)
            {
                return;
            }

            BattlePreparePlacementCancelContext context =
                new(this, null, reason);
            extensionSeamProvider.GhostPreviewAdapter?.ClearGhostPreview(context);
            extensionSeamProvider.PlacementCancelAdapter?.CancelPlacement(context);
        }

        internal static bool IsPlacedOnBoard(DraggableTalismanItemView view)
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
            bool coreLoopComplete = IsCoreLoopComplete();

            if (stateButtonText != null)
            {
                stateButtonText.text = ResolveBattleStateButtonLabel();
            }

            if (prepareButtonText != null)
            {
                prepareButtonText.text = prepareStateActive || continueStateActive ? "整备中" : "整备";
            }

            if (stateButton != null)
            {
                stateButton.interactable = !continueStateActive && !coreLoopComplete;
            }

            if (prepareButton != null)
            {
                prepareButton.interactable = !prepareStateActive && !continueStateActive;
            }
        }

        private string ResolveBattleStateButtonLabel()
        {
            V02RunFlowController flowController = ResolveRunFlowController();
            if (flowController != null && flowController.IsCoreLoopComplete())
            {
                return "已完成";
            }

            if (prepareStateActive)
            {
                return "继续战斗";
            }

            if (continueStateActive)
            {
                return "继续战斗";
            }

            if (combatController != null && combatController.CurrentCombatState == TalismanCombatState.Fighting)
            {
                return "战斗中";
            }

            if (flowController != null && flowController.IsWaitingForBossChallenge())
            {
                return "挑战Boss";
            }

            return "继续战斗";
        }

        private bool IsCoreLoopComplete()
        {
            V02RunFlowController flowController = ResolveRunFlowController();
            return flowController != null && flowController.IsCoreLoopComplete();
        }

        private V02RunFlowController ResolveRunFlowController()
        {
            if (runFlowController != null)
            {
                return runFlowController;
            }

            runFlowController = UnityEngine.Object.FindObjectOfType<V02RunFlowController>(true);
            return runFlowController;
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

    public sealed class V03ItemTraySlotDropTarget : MonoBehaviour, IDropHandler
    {
        private V03BattlePrepareInteractionController owner;
        private RectTransform slotRoot;

        public void Bind(V03BattlePrepareInteractionController controller, RectTransform root)
        {
            owner = controller;
            slotRoot = root;
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (owner == null || slotRoot == null || eventData.pointerDrag == null)
            {
                return;
            }

            DraggableTalismanItemView dragged = eventData.pointerDrag.GetComponent<DraggableTalismanItemView>();
            if (dragged == null || V03BattlePrepareInteractionController.IsPlacedOnBoard(dragged))
            {
                return;
            }

            owner.MoveItemWithinTray(dragged, slotRoot);
        }
    }
}
