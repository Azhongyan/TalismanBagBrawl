using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TalismanBag.Items.Detail.UI
{
    [DisallowMultipleComponent]
    public sealed class ItemDetailPanelView : MonoBehaviour
    {
        private const int RequiredPlayerSectionCount = 15;
        private const int RequiredDebugSectionCount = 3;
        private const float MouseWheelNormalizedStep = 0.08f;
        private const float TouchDragDeadZonePixels = 2f;
        private const string FaMenIconResourcePrefix = "item/阵法icon/";
        private const string QiLeiIconResourcePrefix = "item/器类icon/";
        private const string RarityIconResourcePrefix = "item/品阶icon/品阶icon_";
        private const string RarityBackgroundResourcePrefix = "item/弹窗背景/background-";
        private const string DetailArtworkResourcePrefix = "item_daoju/";
        private const string LegacyTextFontResourceName = "LegacyRuntime.ttf";
        private const string LegacyTextFontFailureCode =
            "ITEM_DETAIL_LEGACY_TEXT_FONT_RESOLVE_FAILED";
        private const string PreferredChineseOsFontName = "SimSun";
        private const int RuntimeChineseFontSize = 20;
        private static readonly Color LegacyBuildInactiveColor =
            new Color32(0x8a, 0x8a, 0x8a, 0xff);
        private static readonly string[] ApprovedChineseFallbackFontNames =
        {
            "Microsoft YaHei",
            "Noto Sans CJK SC",
            "Droid Sans Fallback"
        };

        [SerializeField] private Text itemNameText;
        [SerializeField] private Text metaText;
        [SerializeField] private Text powerText;
        [SerializeField] private Text artworkText;
        [SerializeField] private Image artworkFrameImage;
        [SerializeField] private Image artworkImageSlot;
        [SerializeField] private Text artworkKeyText;
        [SerializeField] private Image rarityBadgeImage;
        [SerializeField] private Text rarityBadgeText;
        [SerializeField] private Image faMenIconImage;
        [SerializeField] private Image qiLeiIconImage;
        [SerializeField] private Image[] statusBadgeImages = new Image[3];
        [SerializeField] private Text[] statusBadgeTexts = new Text[3];
        [SerializeField] private Button closeButton;
        [SerializeField] private Image rarityAccentImage;
        [SerializeField] private Image cardBackgroundImage;
        [SerializeField] private ItemDetailSectionView[] playerSections = new ItemDetailSectionView[RequiredPlayerSectionCount];
        [SerializeField] private ItemDetailSectionView[] debugSections = new ItemDetailSectionView[RequiredDebugSectionCount];
        [SerializeField] private Button detailTabButton;
        [SerializeField] private Button debugTabButton;
        [SerializeField] private Text detailTabText;
        [SerializeField] private Text debugTabText;
        [SerializeField] private GameObject detailScrollRoot;
        [SerializeField] private GameObject debugScrollRoot;
        [SerializeField] private ScrollRect detailScrollRect;
        [SerializeField] private ScrollRect debugScrollRect;
        [SerializeField] private ItemDetailVisualTheme visualTheme;
        [SerializeField] private Font chineseTextFont;
        [SerializeField] private bool showDebugTabOnPlayerRoute;

        private DetailTab activeTab = DetailTab.Detail;
        private string currentModelIdentity;
        private int activeTouchFingerId = int.MinValue;
        private bool touchScrolling;
        private bool preserveAuthoredVisualStyle;
        private bool hasCapturedAuthoredActiveStates;
        [SerializeField] private Sprite lightingStatusLitSprite;
        [SerializeField] private Sprite lightingStatusUnlitSprite;
        [SerializeField] private Sprite arrayStatusLitSprite;
        [SerializeField] private Sprite arrayStatusUnlitSprite;
        private Image singleCellArtworkImageSlot;
        private Image multiCellArtworkImageSlot;
        private bool legacyTextFontFailureLogged;
        private bool legacyTextFontResolutionAttempted;
        private bool ownsCachedLegacyTextFont;
        private int legacyTextFontResolutionCount;
        private Font cachedLegacyTextFont;
        private ItemDetailVisualTheme legacyBuildPresentationTheme;
        private ItemDetailVisualTheme legacyBuildPresentationThemeSource;
        private ItemDetailOutsideDismissInputBlocker outsideDismissInputBlocker;
        private Action externalCloseRequested;
        private string boundItemInstanceId = string.Empty;
        private string boundBaseItemId = string.Empty;
        private string cachedLegacyTextFontSource = string.Empty;
        private string cachedLegacyTextFontFallbackReason = string.Empty;
        private readonly Dictionary<Transform, bool> authoredActiveSelfByTransform = new();

        public Font CachedLegacyTextFont => cachedLegacyTextFont;
        public string CachedLegacyTextFontSource => cachedLegacyTextFontSource;
        public string CachedLegacyTextFontFallbackReason =>
            cachedLegacyTextFontFallbackReason;
        public int LegacyTextFontResolutionCount => legacyTextFontResolutionCount;

#if UNITY_EDITOR
        public void ConfigureEditor(
            Text configuredItemNameText,
            Text configuredMetaText,
            Text configuredPowerText,
            Text configuredArtworkText,
            Image configuredArtworkFrameImage,
            Image configuredArtworkImageSlot,
            Text configuredArtworkKeyText,
            Image configuredRarityBadgeImage,
            Text configuredRarityBadgeText,
            Image[] configuredStatusBadgeImages,
            Text[] configuredStatusBadgeTexts,
            Button configuredCloseButton,
            Image configuredRarityAccentImage,
            Image configuredCardBackgroundImage,
            ItemDetailSectionView[] configuredPlayerSections,
            ItemDetailSectionView[] configuredDebugSections,
            Button configuredDetailTabButton,
            Button configuredDebugTabButton,
            Text configuredDetailTabText,
            Text configuredDebugTabText,
            GameObject configuredDetailScrollRoot,
            GameObject configuredDebugScrollRoot,
            ScrollRect configuredDetailScrollRect,
            ScrollRect configuredDebugScrollRect)
        {
            itemNameText = configuredItemNameText;
            metaText = configuredMetaText;
            powerText = configuredPowerText;
            artworkText = configuredArtworkText;
            artworkFrameImage = configuredArtworkFrameImage;
            artworkImageSlot = configuredArtworkImageSlot;
            artworkKeyText = configuredArtworkKeyText;
            rarityBadgeImage = configuredRarityBadgeImage;
            rarityBadgeText = configuredRarityBadgeText;
            statusBadgeImages = configuredStatusBadgeImages ?? statusBadgeImages;
            statusBadgeTexts = configuredStatusBadgeTexts ?? statusBadgeTexts;
            closeButton = configuredCloseButton;
            rarityAccentImage = configuredRarityAccentImage;
            cardBackgroundImage = configuredCardBackgroundImage;
            playerSections = configuredPlayerSections ?? playerSections;
            debugSections = configuredDebugSections ?? debugSections;
            detailTabButton = configuredDetailTabButton;
            debugTabButton = configuredDebugTabButton;
            detailTabText = configuredDetailTabText;
            debugTabText = configuredDebugTabText;
            detailScrollRoot = configuredDetailScrollRoot;
            debugScrollRoot = configuredDebugScrollRoot;
            detailScrollRect = configuredDetailScrollRect;
            debugScrollRect = configuredDebugScrollRect;
            BindTabs();
        }

        public void ConfigureThemeEditor(ItemDetailVisualTheme configuredTheme)
        {
            ReleaseLegacyBuildPresentationTheme();
            visualTheme = configuredTheme;
            ApplySectionTheme(playerSections);
            ApplySectionTheme(debugSections);
        }

        public void ConfigureStatusBadgeSpritesEditor(
            Sprite configuredLightingLit,
            Sprite configuredLightingUnlit,
            Sprite configuredArrayLit,
            Sprite configuredArrayUnlit)
        {
            SetStatusBadgeSprites(
                configuredLightingLit,
                configuredLightingUnlit,
                configuredArrayLit,
                configuredArrayUnlit);
            if (statusBadgeImages != null
                && statusBadgeImages.Length > 0
                && statusBadgeImages[0] != null)
            {
                statusBadgeImages[0].sprite = configuredLightingLit;
            }

            if (statusBadgeImages != null
                && statusBadgeImages.Length > 2
                && statusBadgeImages[2] != null)
            {
                statusBadgeImages[2].sprite = configuredArrayLit;
            }
        }
#endif

        private void Awake()
        {
            ResolveIdentityIconImages();
            ResolveArtworkImageSlot();
            EnsureOutsideDismissInputBlocker();
            BindTabs();
        }

        private void OnDisable()
        {
            activeTouchFingerId = int.MinValue;
            touchScrolling = false;
            outsideDismissInputBlocker?.SetPanelVisible(false);
        }

        private void OnDestroy()
        {
            externalCloseRequested = null;
            outsideDismissInputBlocker?.Release(this);
            outsideDismissInputBlocker = null;
            ReleaseLegacyBuildPresentationTheme();
            if (!ownsCachedLegacyTextFont || cachedLegacyTextFont == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(cachedLegacyTextFont);
            }
            else
            {
                DestroyImmediate(cachedLegacyTextFont);
            }

            cachedLegacyTextFont = null;
            ownsCachedLegacyTextFont = false;
        }

        private void Update()
        {
            HandleDirectScrollInput();
        }

        public void Bind(ItemDetailViewModel model)
        {
            Bind(model, null);
        }

        public void Bind(ItemDetailViewModel model, Sprite artworkSprite)
        {
            if (!RecoverMissingLegacyTextFonts())
            {
                SetVisible(false);
                return;
            }

            if (model == null)
            {
                currentModelIdentity = string.Empty;
                boundItemInstanceId = string.Empty;
                boundBaseItemId = string.Empty;
                SetHeader(string.Empty, string.Empty, string.Empty);
                SetRarityBadge(string.Empty);
                SetArtwork(string.Empty, string.Empty, string.Empty, null);
                SetStatusBadges(null);
                ApplySectionItemPresentation(playerSections, string.Empty, string.Empty, string.Empty, string.Empty);
                SetPlayerSections(playerSections, null);
                SetSections(debugSections, null);
                ShowPlayerDetailTab();
                CompleteLegacyTextBinding();
                return;
            }

            string nextIdentity = BuildModelIdentity(model);
            bool changedModel = !string.Equals(currentModelIdentity, nextIdentity, StringComparison.Ordinal);
            currentModelIdentity = nextIdentity;
            boundItemInstanceId = model.itemInstanceId ?? string.Empty;
            boundBaseItemId = model.baseItemId ?? string.Empty;

            Color rarityColor = ResolveRarityColor(model.rarityColorKey, model.displayRarityName);
            string rarityDisplayName = ResolveRarityDisplayName(model.rarityColorKey, model.displayRarityName);
            ApplyRarityStyle(rarityColor);
            SetRarityBackground(rarityDisplayName);
            SetHeader(
                BuildItemNameWithLevel(model),
                BuildMetaText(model, rarityDisplayName, rarityColor),
                BuildPowerText(model.displayItemPower, rarityColor));
            SetIdentityIcons(model.displayFaMenName, model.displayQiLeiName);
            SetRarityBadge(rarityDisplayName);
            SetArtwork(
                model.displayQiLeiName,
                SanitizeShapeName(model.displayShapeName),
                model.iconPlaceholderKey,
                artworkSprite ?? ResolveDetailArtworkSprite(model, rarityDisplayName));
            SetStatusBadges(model);
            ApplySectionTheme(playerSections);
            ApplySectionTheme(debugSections);
            ApplySectionRarityStyle(
                playerSections,
                rarityColor,
                model.rarityColorKey,
                rarityDisplayName);
            ApplySectionItemPresentation(
                playerSections,
                model.itemId,
                model.baseItemId,
                model.displayFaMenName,
                model.displayQiLeiName);
            SetPlayerSections(playerSections, model.displayPlayerSections);
            SetSections(debugSections, model.displayDebugSections);

            if (changedModel)
            {
                ShowPlayerDetailTab();
                ResetScrollToTop();
            }
            else
            {
                ApplyTabState();
            }

            CompleteLegacyTextBinding();
        }

        public void ShowPlayerDetailTab()
        {
            activeTab = DetailTab.Detail;
            ApplyTabState();
            ResetScroll(detailScrollRect);
        }

        public void ShowDebugTab()
        {
            if (!showDebugTabOnPlayerRoute)
            {
                ShowPlayerDetailTab();
                return;
            }
            activeTab = DetailTab.Debug;
            ApplyTabState();
            ResetScroll(debugScrollRect);
        }

        public void ResetScrollToTop()
        {
            ResetScroll(detailScrollRect);
            ResetScroll(debugScrollRect);
        }

        public void SetVisible(bool visible)
        {
            if (visible)
            {
                EnsureOutsideDismissInputBlocker();
                gameObject.SetActive(true);
                outsideDismissInputBlocker?.SetPanelVisible(true);
                return;
            }

            outsideDismissInputBlocker?.SetPanelVisible(false);
            gameObject.SetActive(false);
        }

        public void SetPreserveAuthoredVisualStyle(bool preserve)
        {
            if (preserve)
            {
                if (!preserveAuthoredVisualStyle || !hasCapturedAuthoredActiveStates)
                {
                    CaptureAuthoredActiveStates();
                }

                preserveAuthoredVisualStyle = true;
                SetSectionsPreserveAuthoredVisualStyle(
                    playerSections,
                    externalCloseRequested == null);
                SetSectionsPreserveAuthoredVisualStyle(debugSections, true);
                SetBuildSectionsRuntimeStateWritable();
                return;
            }

            preserveAuthoredVisualStyle = false;
            authoredActiveSelfByTransform.Clear();
            hasCapturedAuthoredActiveStates = false;
            SetSectionsPreserveAuthoredVisualStyle(playerSections, preserve);
            SetSectionsPreserveAuthoredVisualStyle(debugSections, preserve);
        }

        private void SetBuildSectionsRuntimeStateWritable()
        {
            foreach (ItemDetailSectionView section in
                     playerSections ?? Array.Empty<ItemDetailSectionView>())
            {
                if (section != null
                    && IsBuildSection(section))
                {
                    section.SetPreserveAuthoredVisualStyle(false);
                }
            }
        }

        private void CaptureAuthoredActiveStates()
        {
            authoredActiveSelfByTransform.Clear();
            foreach (Transform target in GetComponentsInChildren<Transform>(true))
            {
                if (target != null)
                {
                    authoredActiveSelfByTransform[target] = target.gameObject.activeSelf;
                }
            }

            hasCapturedAuthoredActiveStates = true;
        }

        private bool WasAuthoredInactive(GameObject target)
        {
            if (!preserveAuthoredVisualStyle
                || target == null
                || ReferenceEquals(target, gameObject))
            {
                return false;
            }

            if (!hasCapturedAuthoredActiveStates)
            {
                CaptureAuthoredActiveStates();
            }

            return authoredActiveSelfByTransform.TryGetValue(target.transform, out bool authoredActive)
                && !authoredActive;
        }

        private void SetAuthoredAwareActive(GameObject target, bool active)
        {
            if (target == null)
            {
                return;
            }

            bool resolvedActive = active
                                  && WasAuthoredInactive(target)
                                  && !IsFormalRuntimeDataCarrier(target)
                ? false
                : active;
            if (target.activeSelf != resolvedActive)
            {
                target.SetActive(resolvedActive);
            }
        }

        public void SetStatusBadgeSprites(
            Sprite lightingLit,
            Sprite lightingUnlit,
            Sprite arrayLit,
            Sprite arrayUnlit)
        {
            lightingStatusLitSprite = lightingLit;
            lightingStatusUnlitSprite = lightingUnlit;
            arrayStatusLitSprite = arrayLit;
            arrayStatusUnlitSprite = arrayUnlit;
        }

        public void Close()
        {
            SetVisible(false);
        }

        public void SetExternalCloseRequest(Action closeRequest)
        {
            externalCloseRequested = closeRequest;
            BindTabs();
        }

        public void RequestClose()
        {
            Action closeRequest = externalCloseRequested;
            if (closeRequest != null)
            {
                closeRequest.Invoke();
                return;
            }

            Close();
        }

        public bool ValidateVisibleFormalBinding(
            string expectedItemInstanceId,
            string expectedBaseItemId,
            Sprite expectedArtwork,
            bool expectSingleCellArtwork)
        {
            ResolveArtworkImageSlot();
            Image expectedSlot = expectSingleCellArtwork
                ? singleCellArtworkImageSlot
                : multiCellArtworkImageSlot;
            Image rejectedSlot = expectSingleCellArtwork
                ? multiCellArtworkImageSlot
                : singleCellArtworkImageSlot;
            bool readableSection = (playerSections
                    ?? Array.Empty<ItemDetailSectionView>())
                .Any(section => section != null
                                && section.gameObject.activeInHierarchy
                                && (!string.IsNullOrWhiteSpace(section.Title)
                                    || !string.IsNullOrWhiteSpace(
                                        section.Body)));
            return gameObject.activeInHierarchy
                   && string.Equals(
                       boundItemInstanceId,
                       expectedItemInstanceId,
                       StringComparison.Ordinal)
                   && string.Equals(
                       boundBaseItemId,
                       expectedBaseItemId,
                       StringComparison.Ordinal)
                   && itemNameText != null
                   && itemNameText.enabled
                   && itemNameText.gameObject.activeInHierarchy
                   && !string.IsNullOrWhiteSpace(itemNameText.text)
                   && readableSection
                   && expectedArtwork != null
                   && expectedSlot != null
                   && expectedSlot.sprite == expectedArtwork
                   && expectedSlot.enabled
                   && expectedSlot.gameObject.activeInHierarchy
                   && (rejectedSlot == null
                       || !rejectedSlot.enabled
                       || !rejectedSlot.gameObject.activeInHierarchy);
        }

        public bool ContainsPopupContentScreenPoint(
            Vector2 screenPosition,
            Camera eventCamera = null)
        {
            if (!gameObject.activeInHierarchy)
            {
                return false;
            }

            Camera resolvedCamera = eventCamera != null
                ? eventCamera
                : ResolveEventCamera();
            RectTransform popupFrame = cardBackgroundImage != null
                ? cardBackgroundImage.rectTransform
                : transform as RectTransform;
            return popupFrame != null
                && popupFrame.gameObject.activeInHierarchy
                && RectTransformUtility.RectangleContainsScreenPoint(
                    popupFrame,
                    screenPosition,
                    resolvedCamera);
        }

        private void CompleteLegacyTextBinding()
        {
            if (!RecoverMissingLegacyTextFonts())
            {
                SetVisible(false);
                return;
            }

            RebuildExistingLayoutMeasurements();
        }

        private bool RecoverMissingLegacyTextFonts()
        {
            Text[] descendants = GetComponentsInChildren<Text>(true);
            Font resolvedFont = ResolveCachedLegacyTextFont(descendants);
            if (resolvedFont == null)
            {
                if (!legacyTextFontFailureLogged)
                {
                    Debug.LogError(
                        "[ItemDetailPanelView][" + LegacyTextFontFailureCode
                        + "] 无法为道具详情解析可用的旧版文字字体，面板已保持隐藏。",
                        this);
                    legacyTextFontFailureLogged = true;
                }

                return false;
            }

            foreach (Text text in descendants)
            {
                if (text != null && text.font == null)
                {
                    text.font = resolvedFont;
                }
            }

            legacyTextFontFailureLogged = false;
            return true;
        }

        private Font ResolveCachedLegacyTextFont(IReadOnlyList<Text> descendants)
        {
            if (legacyTextFontResolutionAttempted)
            {
                return cachedLegacyTextFont;
            }

            legacyTextFontResolutionAttempted = true;
            legacyTextFontResolutionCount++;
            cachedLegacyTextFont = ResolveLegacyTextFont(
                descendants,
                out ownsCachedLegacyTextFont,
                out cachedLegacyTextFontSource,
                out cachedLegacyTextFontFallbackReason);
            if (cachedLegacyTextFont != null && ownsCachedLegacyTextFont)
            {
                cachedLegacyTextFont.hideFlags = HideFlags.DontSave;
            }

            if (cachedLegacyTextFont != null
                && !string.IsNullOrWhiteSpace(cachedLegacyTextFontFallbackReason))
            {
                Debug.Log(
                    "[ItemDetailPanelView][ITEM_DETAIL_LEGACY_TEXT_FONT_FALLBACK] "
                    + cachedLegacyTextFontFallbackReason
                    + "；实际字体：" + cachedLegacyTextFont.name + "。",
                    this);
            }

            return cachedLegacyTextFont;
        }

        private Font ResolveLegacyTextFont(
            IReadOnlyList<Text> descendants,
            out bool ownsRuntimeFont,
            out string source,
            out string fallbackReason)
        {
            ownsRuntimeFont = false;
            source = string.Empty;
            fallbackReason = string.Empty;
            if (chineseTextFont != null)
            {
                source = "serialized chineseTextFont";
                return chineseTextFont;
            }

            if (descendants != null)
            {
                for (int index = 0; index < descendants.Count; index++)
                {
                    Font descendantFont = descendants[index] != null
                        ? descendants[index].font
                        : null;
                    if (descendantFont != null)
                    {
                        source = "authored descendant Font: " + descendantFont.name;
                        return descendantFont;
                    }
                }
            }

            string[] installedFontNames;
            try
            {
                installedFontNames = Font.GetOSInstalledFontNames()
                    ?? Array.Empty<string>();
            }
            catch (Exception)
            {
                installedFontNames = Array.Empty<string>();
            }

            Font simSun = TryCreateInstalledFont(
                installedFontNames,
                PreferredChineseOsFontName);
            if (simSun != null)
            {
                ownsRuntimeFont = true;
                source = "installed OS Font: " + PreferredChineseOsFontName;
                return simSun;
            }

            for (int index = 0;
                 index < ApprovedChineseFallbackFontNames.Length;
                 index++)
            {
                string fallbackName = ApprovedChineseFallbackFontNames[index];
                Font fallback = TryCreateInstalledFont(
                    installedFontNames,
                    fallbackName);
                if (fallback == null)
                {
                    continue;
                }

                ownsRuntimeFont = true;
                source = "installed Chinese fallback: " + fallbackName;
                fallbackReason = "未检测到可创建的 SimSun，按批准顺序回退到 "
                    + fallbackName;
                return fallback;
            }

            try
            {
                Font builtin = Resources.GetBuiltinResource<Font>(
                    LegacyTextFontResourceName);
                if (builtin != null)
                {
                    source = "built-in Resource: " + LegacyTextFontResourceName;
                    fallbackReason = "未检测到可创建的 SimSun 或批准的中文系统字体，"
                        + "使用最后兜底 " + LegacyTextFontResourceName;
                }
                return builtin;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static Font TryCreateInstalledFont(
            IReadOnlyList<string> installedFontNames,
            string requestedName)
        {
            if (installedFontNames == null
                || string.IsNullOrWhiteSpace(requestedName))
            {
                return null;
            }

            string installedName = null;
            for (int index = 0; index < installedFontNames.Count; index++)
            {
                if (string.Equals(
                        installedFontNames[index],
                        requestedName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    installedName = installedFontNames[index];
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(installedName))
            {
                return null;
            }

            try
            {
                return Font.CreateDynamicFontFromOSFont(
                    installedName,
                    RuntimeChineseFontSize);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void RebuildExistingLayoutMeasurements()
        {
            Canvas.ForceUpdateCanvases();
            ForceRebuildLayout(detailScrollRect != null ? detailScrollRect.content : null);
            ForceRebuildLayout(debugScrollRect != null ? debugScrollRect.content : null);
            ForceRebuildLayout(transform as RectTransform);
            Canvas.ForceUpdateCanvases();
        }

        private static void ForceRebuildLayout(RectTransform target)
        {
            if (target == null)
            {
                return;
            }

            LayoutRebuilder.MarkLayoutForRebuild(target);
            LayoutRebuilder.ForceRebuildLayoutImmediate(target);
        }

        private void BindTabs()
        {
            if (detailTabButton != null)
            {
                detailTabButton.onClick.RemoveListener(ShowPlayerDetailTab);
                detailTabButton.onClick.AddListener(ShowPlayerDetailTab);
            }

            if (debugTabButton != null)
            {
                debugTabButton.onClick.RemoveListener(ShowDebugTab);
                debugTabButton.onClick.AddListener(ShowDebugTab);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(RequestClose);
                closeButton.onClick.AddListener(RequestClose);
            }

            ApplyTabState();
        }

        private void ApplyTabState()
        {
            if (!showDebugTabOnPlayerRoute)
            {
                activeTab = DetailTab.Detail;
            }
            bool detailActive = activeTab == DetailTab.Detail;
            SetActive(detailScrollRoot, detailActive);
            SetActive(
                debugScrollRoot,
                showDebugTabOnPlayerRoute && !detailActive);
            if (debugTabButton != null)
            {
                SetActive(
                    debugTabButton.gameObject,
                    showDebugTabOnPlayerRoute);
            }
            SetTabVisual(detailTabButton, detailTabText, detailActive);
            if (showDebugTabOnPlayerRoute)
            {
                SetTabVisual(debugTabButton, debugTabText, !detailActive);
            }
        }

        private void HandleDirectScrollInput()
        {
            ScrollRect scrollRect = ActiveScrollRect();
            if (!CanDirectScroll(scrollRect))
            {
                activeTouchFingerId = int.MinValue;
                touchScrolling = false;
                return;
            }

            HandleMouseWheelScroll(scrollRect);
            HandleTouchDragScroll(scrollRect);
        }

        private ScrollRect ActiveScrollRect()
        {
            return activeTab == DetailTab.Detail
                ? detailScrollRect
                : debugScrollRect;
        }

        private bool CanDirectScroll(ScrollRect scrollRect)
        {
            return scrollRect != null
                && scrollRect.gameObject.activeInHierarchy
                && scrollRect.content != null
                && ResolveScrollableHeight(scrollRect) > 0.5f;
        }

        private void HandleMouseWheelScroll(ScrollRect scrollRect)
        {
            float wheelDelta = Input.mouseScrollDelta.y;
            if (Mathf.Abs(wheelDelta) <= Mathf.Epsilon
                || !ContainsScreenPoint(Input.mousePosition))
            {
                return;
            }

            ApplyNormalizedScroll(
                scrollRect,
                wheelDelta * MouseWheelNormalizedStep);
        }

        private void HandleTouchDragScroll(ScrollRect scrollRect)
        {
            if (Input.touchCount <= 0)
            {
                activeTouchFingerId = int.MinValue;
                touchScrolling = false;
                return;
            }

            Touch touch = FindActiveTouch();
            if (touch.phase == TouchPhase.Began
                && ContainsScreenPoint(touch.position))
            {
                activeTouchFingerId = touch.fingerId;
                touchScrolling = false;
                return;
            }

            if (touch.fingerId != activeTouchFingerId)
            {
                return;
            }

            if (touch.phase == TouchPhase.Canceled
                || touch.phase == TouchPhase.Ended)
            {
                activeTouchFingerId = int.MinValue;
                touchScrolling = false;
                return;
            }

            if (touch.phase != TouchPhase.Moved)
            {
                return;
            }

            float delta = touch.deltaPosition.y;
            if (!touchScrolling
                && Mathf.Abs(delta) < TouchDragDeadZonePixels)
            {
                return;
            }

            touchScrolling = true;
            ApplyNormalizedScroll(
                scrollRect,
                -delta / ResolveScrollableHeight(scrollRect));
        }

        private Touch FindActiveTouch()
        {
            if (activeTouchFingerId == int.MinValue)
            {
                return Input.GetTouch(0);
            }

            for (int index = 0; index < Input.touchCount; index++)
            {
                Touch touch = Input.GetTouch(index);
                if (touch.fingerId == activeTouchFingerId)
                {
                    return touch;
                }
            }

            activeTouchFingerId = int.MinValue;
            touchScrolling = false;
            return Input.GetTouch(0);
        }

        private bool ContainsScreenPoint(Vector2 screenPosition)
        {
            RectTransform rect = transform as RectTransform;
            return rect != null
                && RectTransformUtility.RectangleContainsScreenPoint(
                    rect,
                    screenPosition,
                    ResolveEventCamera());
        }

        private void EnsureOutsideDismissInputBlocker()
        {
            Canvas detailCanvas = GetComponentInParent<Canvas>();
            Image outsideSurface = detailCanvas != null
                ? detailCanvas.GetComponent<Image>()
                : null;
            if (detailCanvas == null || outsideSurface == null)
            {
                return;
            }

            if (detailCanvas.GetComponent<GraphicRaycaster>() == null)
            {
                GraphicRaycaster raycaster =
                    detailCanvas.gameObject.AddComponent<GraphicRaycaster>();
                raycaster.hideFlags =
                    HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            }

            outsideDismissInputBlocker =
                detailCanvas.GetComponent<ItemDetailOutsideDismissInputBlocker>();
            if (outsideDismissInputBlocker == null)
            {
                outsideDismissInputBlocker =
                    detailCanvas.gameObject
                        .AddComponent<ItemDetailOutsideDismissInputBlocker>();
                outsideDismissInputBlocker.hideFlags =
                    HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            }

            outsideDismissInputBlocker.Configure(this, outsideSurface);
        }

        private static void ApplyNormalizedScroll(
            ScrollRect scrollRect,
            float normalizedDelta)
        {
            scrollRect.StopMovement();
            scrollRect.velocity = Vector2.zero;
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(
                scrollRect.verticalNormalizedPosition + normalizedDelta);
        }

        private static float ResolveScrollableHeight(ScrollRect scrollRect)
        {
            RectTransform viewport = scrollRect.viewport != null
                ? scrollRect.viewport
                : scrollRect.transform as RectTransform;
            if (scrollRect.content == null || viewport == null)
            {
                return 0f;
            }

            return Mathf.Max(
                0f,
                scrollRect.content.rect.height - viewport.rect.height);
        }

        private Camera ResolveEventCamera()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            Canvas eventCanvas = canvas != null && canvas.rootCanvas != null
                ? canvas.rootCanvas
                : canvas;
            return eventCanvas != null
                && eventCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                    ? eventCanvas.worldCamera
                    : null;
        }

        private static void SetSections(
            ItemDetailSectionView[] targetSections,
            IReadOnlyList<ItemDetailSectionViewModel> sourceSections)
        {
            if (targetSections == null)
            {
                return;
            }

            for (int i = 0; i < targetSections.Length; i++)
            {
                ItemDetailSectionView target = targetSections[i];
                if (target == null)
                {
                    continue;
                }

                if (sourceSections != null && i < sourceSections.Count)
                {
                    target.SetContent(sourceSections[i]);
                }
                else
                {
                    target.SetContent(string.Empty, string.Empty, keepWhenEmpty: false);
                }
            }
        }

        private void SetPlayerSections(
            ItemDetailSectionView[] targetSections,
            IReadOnlyList<ItemDetailSectionViewModel> sourceSections)
        {
            if (targetSections == null)
            {
                return;
            }

            for (int index = 0; index < targetSections.Length; index++)
            {
                ItemDetailSectionView target = targetSections[index];
                if (target == null)
                {
                    continue;
                }

                string[] stateKeys = PlayerStateKeysForTarget(target.name);
                if (stateKeys == null)
                {
                    if (sourceSections != null && index < sourceSections.Count)
                    {
                        target.SetContent(sourceSections[index]);
                    }
                    else
                    {
                        target.SetContent(string.Empty, string.Empty, keepWhenEmpty: false);
                    }

                    continue;
                }

                if (externalCloseRequested != null)
                {
                    target.SetPreserveAuthoredVisualStyle(false);
                }
                else if (stateKeys.Any(stateKey =>
                        string.Equals(
                            stateKey,
                            "famenBuild",
                            StringComparison.Ordinal)
                        || string.Equals(
                            stateKey,
                            "qileiBuild",
                            StringComparison.Ordinal)))
                {
                    // Build stage rows and their active-state masks are live
                    // qualified presentation. Keep the authored hierarchy and
                    // styling, but never freeze their runtime visibility.
                    target.SetPreserveAuthoredVisualStyle(false);
                }

                ItemDetailSectionViewModel source = FindPlayerSection(sourceSections, stateKeys);
                if (source != null)
                {
                    target.SetContent(source);
                }
                else
                {
                    target.SetContent(string.Empty, string.Empty, keepWhenEmpty: false);
                }
            }
        }

        private static ItemDetailSectionViewModel FindPlayerSection(
            IReadOnlyList<ItemDetailSectionViewModel> sourceSections,
            IReadOnlyList<string> stateKeys)
        {
            if (sourceSections == null || stateKeys == null)
            {
                return null;
            }

            foreach (string stateKey in stateKeys)
            {
                for (int index = 0; index < sourceSections.Count; index++)
                {
                    ItemDetailSectionViewModel source = sourceSections[index];
                    if (source != null && string.Equals(
                            source.stateKey,
                            stateKey,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return source;
                    }
                }
            }

            return null;
        }

        private static string[] PlayerStateKeysForTarget(string targetName)
        {
            return targetName switch
            {
                "HeaderSection" => new[] { "header", "itemPower" },
                "CoreIdentitySection" => new[] { "identity" },
                "CurrentStateSection" => new[] { "currentState", "status" },
                "BaseStatsSection" => new[] { "stats" },
                "TriggerConditionSection" => new[] { "trigger" },
                "BasicEffectSection" => new[] { "basic" },
                "CoreAwakeningSection" => new[] { "coreEffect", "awakening" },
                "FaMenBuildSection" => new[] { "famenBuild" },
                "QiLeiBuildSection" => new[] { "qileiBuild" },
                "MainBuildMonitorSection" => new[] { "skillMonitor" },
                "FixedAffixSection" => new[] { "fixedAffix" },
                "RandomAffixSection" => new[] { "randomAffix" },
                "OrangeGrowthSection" => new[] { "orange" },
                "PlacementHintSection" => new[] { "placement" },
                "FlavorSection" => new[] { "flavor" },
                _ => null
            };
        }

        private void SetHeader(string itemName, string meta, string power)
        {
            if (itemNameText != null)
            {
                itemNameText.text = itemName ?? string.Empty;
            }

            if (metaText != null)
            {
                metaText.text = meta ?? string.Empty;
            }

            if (powerText != null)
            {
                powerText.text = power ?? string.Empty;
            }
        }

        private static string BuildItemNameWithLevel(ItemDetailViewModel model)
        {
            string itemName = model?.displayItemName ?? string.Empty;
            int level = ResolveCultivationLevel(model);
            return string.IsNullOrWhiteSpace(itemName)
                ? "Lv." + level
                : itemName + " Lv." + level;
        }

        private static int ResolveCultivationLevel(ItemDetailViewModel model)
        {
            int level = model?.awakeningPreview?.itemLevel ?? 0;
            if (level <= 0)
            {
                level = model?.statusFlags?.itemLevel ?? 0;
            }
            if (level <= 0)
            {
                level = model?.awakeningPreview?.resolvedLevel ?? 0;
            }
            if (level <= 0)
            {
                level = model?.statusFlags?.resolvedLevel ?? 0;
            }

            return Mathf.Clamp(level <= 0 ? 1 : level, 1, 40);
        }

        private void SetArtwork(string qiLeiName, string shapeName, string iconKey, Sprite artworkSprite)
        {
            ResolveArtworkImageSlot();
            bool hasArtwork = artworkSprite != null;
            bool hasSplitArtworkSlots = singleCellArtworkImageSlot != null
                && multiCellArtworkImageSlot != null;
            if (hasSplitArtworkSlots)
            {
                bool useSingleCellSlot = IsSingleCellArtwork(shapeName);
                SetArtworkSlot(singleCellArtworkImageSlot, artworkSprite, hasArtwork && useSingleCellSlot);
                SetArtworkSlot(multiCellArtworkImageSlot, artworkSprite, hasArtwork && !useSingleCellSlot);
                if (artworkImageSlot != null)
                {
                    artworkImageSlot.enabled = false;
                }
            }
            else if (artworkImageSlot != null)
            {
                artworkImageSlot.sprite = artworkSprite;
                artworkImageSlot.enabled = hasArtwork;
                if (!preserveAuthoredVisualStyle)
                {
                    artworkImageSlot.color = ThemeArtworkTint;
                }
            }

            if (artworkText != null)
            {
                artworkText.enabled = !hasArtwork;
                artworkText.text = string.IsNullOrWhiteSpace(qiLeiName) && string.IsNullOrWhiteSpace(shapeName)
                    ? "符器图"
                    : $"{qiLeiName}\n{shapeName}";
            }

            if (artworkKeyText != null)
            {
                artworkKeyText.text = string.IsNullOrWhiteSpace(iconKey)
                    ? "ART SLOT"
                    : iconKey;
            }
        }

        private void SetArtworkSlot(Image target, Sprite artworkSprite, bool visible)
        {
            if (target == null)
            {
                return;
            }

            target.sprite = artworkSprite;
            target.enabled = visible;
            if (target.gameObject.activeSelf != visible)
            {
                SetAuthoredAwareActive(target.gameObject, visible);
            }

            if (!preserveAuthoredVisualStyle)
            {
                target.color = ThemeIconTint;
            }
        }

        private bool IsFormalRuntimeDataCarrier(GameObject target)
        {
            return externalCloseRequested != null
                   && target != null
                   && (ReferenceEquals(
                           target,
                           singleCellArtworkImageSlot != null
                               ? singleCellArtworkImageSlot.gameObject
                               : null)
                       || ReferenceEquals(
                           target,
                           multiCellArtworkImageSlot != null
                               ? multiCellArtworkImageSlot.gameObject
                               : null));
        }

        private static bool IsSingleCellArtwork(string shapeName)
        {
            return !string.IsNullOrWhiteSpace(shapeName)
                && (shapeName.Contains("单格", StringComparison.Ordinal)
                    || shapeName.Contains("single_1", StringComparison.OrdinalIgnoreCase));
        }

        private Sprite ResolveDetailArtworkSprite(ItemDetailViewModel model, string rarityDisplayName)
        {
            string baseItemId = ExtractDetailArtworkBaseItemId(model?.baseItemId);
            if (string.IsNullOrEmpty(baseItemId))
            {
                baseItemId = ExtractDetailArtworkBaseItemId(model?.itemId);
            }

            if (string.IsNullOrEmpty(baseItemId)
                || !int.TryParse(baseItemId.Substring(1), out int itemNumber))
            {
                return null;
            }

            string faMenFolder = itemNumber switch
            {
                >= 1 and <= 6 => "震雷法",
                >= 7 and <= 12 => "离火法",
                >= 13 and <= 18 => "中岳法",
                >= 19 and <= 24 => "玄水法",
                >= 25 and <= 30 => "太白法",
                _ => string.Empty
            };
            int rarityIndex = rarityDisplayName switch
            {
                "凡品" => 1,
                "良品" => 2,
                "灵品" => 3,
                "玄品" => 4,
                "道品" => 5,
                _ => 0
            };
            if (string.IsNullOrEmpty(faMenFolder) || rarityIndex == 0)
            {
                return null;
            }

            string resourcePath = DetailArtworkResourcePrefix
                + faMenFolder + "/" + baseItemId + "/"
                + baseItemId + "_" + rarityIndex;
            return Resources.Load<Sprite>(resourcePath);
        }

        private static string ExtractDetailArtworkBaseItemId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            string normalized = value.Trim().ToUpperInvariant();
            for (int index = 0; index <= normalized.Length - 4; index++)
            {
                if (normalized[index] == 'I'
                    && char.IsDigit(normalized[index + 1])
                    && char.IsDigit(normalized[index + 2])
                    && char.IsDigit(normalized[index + 3]))
                {
                    return normalized.Substring(index, 4);
                }
            }

            return string.Empty;
        }

        private void SetRarityBadge(string rarityName)
        {
            if (rarityBadgeImage != null)
            {
                Sprite sprite = string.IsNullOrWhiteSpace(rarityName)
                    ? null
                    : Resources.Load<Sprite>(RarityIconResourcePrefix + rarityName.Trim());
                rarityBadgeImage.sprite = sprite;
                rarityBadgeImage.enabled = sprite != null;
            }

            if (rarityBadgeText != null)
            {
                rarityBadgeText.text = string.IsNullOrWhiteSpace(rarityName) ? "未定阶" : rarityName;
            }
        }

        private void SetRarityBackground(string rarityName)
        {
            if (cardBackgroundImage == null || string.IsNullOrWhiteSpace(rarityName))
            {
                return;
            }

            Sprite sprite = Resources.Load<Sprite>(
                RarityBackgroundResourcePrefix + rarityName.Trim());
            if (sprite == null)
            {
                return;
            }

            cardBackgroundImage.sprite = sprite;
            cardBackgroundImage.enabled = true;
        }

        private void ResolveIdentityIconImages()
        {
            faMenIconImage ??= FindNamedImage("zhenfaicon");
            qiLeiIconImage ??= FindNamedImage("qixingicon");
        }

        private void ResolveArtworkImageSlot()
        {
            artworkImageSlot ??= FindNamedImage("daoju");
            singleCellArtworkImageSlot ??= FindNamedImage("DaojuSingleCellImage");
            multiCellArtworkImageSlot ??= FindNamedImage("DaojuMultiCellImage");
        }

        private Image FindNamedImage(string objectName)
        {
            foreach (Image image in GetComponentsInChildren<Image>(true))
            {
                if (image != null && string.Equals(image.name, objectName, StringComparison.Ordinal))
                {
                    return image;
                }
            }

            return null;
        }

        private void SetIdentityIcons(string faMenName, string qiLeiName)
        {
            ResolveIdentityIconImages();
            SetIdentityIcon(faMenIconImage, FaMenIconResourcePrefix, ResolveFaMenIconName(faMenName));
            SetIdentityIcon(qiLeiIconImage, QiLeiIconResourcePrefix, ResolveQiLeiIconName(qiLeiName));
        }

        private static void SetIdentityIcon(Image target, string resourcePrefix, string iconName)
        {
            if (target == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(iconName))
            {
                target.enabled = false;
                return;
            }

            Sprite sprite = Resources.Load<Sprite>(resourcePrefix + iconName);
            target.sprite = sprite;
            target.enabled = sprite != null;
        }

        private static string ResolveFaMenIconName(string displayName)
        {
            string value = displayName?.Trim() ?? string.Empty;
            return value.ToLowerInvariant() switch
            {
                "zhenlei" => "震雷法",
                "lihuo" => "离火法",
                "zhongyue" => "中岳法",
                "xuanshui" => "玄水法",
                "taibai" => "太白法",
                "震雷法" or "离火法" or "中岳法" or "玄水法" or "太白法" => value,
                _ => string.Empty
            };
        }

        private static string ResolveQiLeiIconName(string displayName)
        {
            string value = displayName?.Trim() ?? string.Empty;
            string key = value.ToLowerInvariant();
            if (key == "fu" || value.Contains("符", StringComparison.Ordinal))
            {
                return "符类";
            }
            if (key == "yin" || value.Contains("印", StringComparison.Ordinal))
            {
                return "印类";
            }
            if (key == "ling" || value.Contains("令", StringComparison.Ordinal))
            {
                return "令类";
            }
            if (key == "jing" || value.Contains("镜", StringComparison.Ordinal))
            {
                return "镜类";
            }
            if (key == "fa" || value == "法" || value == "法类")
            {
                return "法类";
            }

            return string.Empty;
        }

        private void SetStatusBadges(ItemDetailViewModel model)
        {
            if (model == null)
            {
                SetStatusBadge(0, "状态未接入", ThemeWeak, lightingStatusUnlitSprite);
                SetStatusBadge(1, "开窍未计算", ThemeWeak);
                SetStatusBadge(2, "阵脉未计算", ThemeWeak, arrayStatusUnlitSprite);
                return;
            }

            ItemDetailStatusFlags flags = model.statusFlags ?? new ItemDetailStatusFlags();
            bool hasPlacement = !string.IsNullOrWhiteSpace(model.placementId);
            bool lightingActive = flags.isLit || flags.isLightingSource;
            SetStatusBadge(
                0,
                ResolveLightingBadgeText(flags, hasPlacement),
                ResolveLightingBadgeColor(flags, hasPlacement),
                lightingActive ? lightingStatusLitSprite : lightingStatusUnlitSprite);

            int awakened = Mathf.Clamp(flags.unlockedCoreEffectCount, 0, 4);
            SetStatusBadge(
                1,
                awakened > 0 ? $"开窍 {awakened}/4" : "尚未开窍",
                awakened >= 4 ? ThemeBuildActive : awakened > 0 ? ThemeBuildNearActive : ThemeWeak);

            if (flags.isArrayBonusActive)
            {
                SetStatusBadge(2, "阵脉生效", ThemeArrayModifier, arrayStatusLitSprite);
            }
            else if (flags.isOnArrayBonusCell)
            {
                SetStatusBadge(2, "阵脉待亮", ThemeBuildNearActive, arrayStatusUnlitSprite);
            }
            else
            {
                SetStatusBadge(
                    2,
                    hasPlacement ? "未占阵脉" : "阵脉未计算",
                    ThemeWeak,
                    arrayStatusUnlitSprite);
            }
        }

        private void SetStatusBadge(int index, string label, Color color, Sprite iconSprite = null)
        {
            if (statusBadgeImages != null
                && index >= 0
                && index < statusBadgeImages.Length
                && statusBadgeImages[index] != null
                && iconSprite != null)
            {
                statusBadgeImages[index].sprite = iconSprite;
            }

            if (!preserveAuthoredVisualStyle
                && statusBadgeImages != null
                && index >= 0
                && index < statusBadgeImages.Length
                && statusBadgeImages[index] != null)
            {
                statusBadgeImages[index].color = color;
            }

            if (statusBadgeTexts != null && index >= 0 && index < statusBadgeTexts.Length && statusBadgeTexts[index] != null)
            {
                statusBadgeTexts[index].text = label ?? string.Empty;
                if (!preserveAuthoredVisualStyle)
                {
                    statusBadgeTexts[index].color = ThemeBody;
                }
            }
        }

        private void ClearStatusBadge(int index)
        {
            if (!preserveAuthoredVisualStyle
                && statusBadgeImages != null
                && index >= 0
                && index < statusBadgeImages.Length
                && statusBadgeImages[index] != null)
            {
                statusBadgeImages[index].color = ThemeClearedStatusBadgeTint;
            }

            if (statusBadgeTexts != null && index >= 0 && index < statusBadgeTexts.Length && statusBadgeTexts[index] != null)
            {
                statusBadgeTexts[index].text = string.Empty;
            }
        }

        private static string ResolveLightingBadgeText(ItemDetailStatusFlags flags, bool hasPlacement)
        {
            if (!hasPlacement)
            {
                return "尚未入阵";
            }

            if (flags.isLightingSource)
            {
                return "点亮源";
            }

            if (flags.isDirectLit)
            {
                return "直接点亮";
            }

            if (flags.isRelayLit)
            {
                return "相邻接亮";
            }

            return "未点亮";
        }

        private Color ResolveLightingBadgeColor(ItemDetailStatusFlags flags, bool hasPlacement)
        {
            if (!hasPlacement)
            {
                return ThemeWeak;
            }

            return flags.isLit || flags.isLightingSource ? ThemeBuildActive : ThemeRestriction;
        }

        private void ApplyRarityStyle(Color rarityColor)
        {
            if (preserveAuthoredVisualStyle)
            {
                if (itemNameText != null)
                {
                    itemNameText.color = rarityColor;
                }

                if (metaText != null)
                {
                    metaText.color = rarityColor;
                }

                return;
            }

            if (itemNameText != null)
            {
                itemNameText.color = rarityColor;
            }

            if (powerText != null)
            {
                powerText.color = ThemeBody;
            }

            if (metaText != null)
            {
                metaText.color = ThemeBody;
            }

            if (artworkText != null)
            {
                artworkText.color = ThemeBody;
            }

            if (artworkFrameImage != null)
            {
                artworkFrameImage.color = WithAlpha(rarityColor, ThemeRarityArtworkFrameAlpha);
            }

            if (rarityBadgeImage != null)
            {
                rarityBadgeImage.color = WithAlpha(rarityColor, ThemeRarityBadgeAlpha);
            }

            if (rarityBadgeText != null)
            {
                rarityBadgeText.color = rarityColor;
            }

            if (rarityAccentImage != null)
            {
                rarityAccentImage.color = WithAlpha(rarityColor, ThemeRarityAccentAlpha);
            }

            if (cardBackgroundImage != null)
            {
                cardBackgroundImage.color = ThemeCard;
            }
        }

        private void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                SetAuthoredAwareActive(target, active);
            }
        }

        private void SetTabVisual(Button button, Text label, bool active)
        {
            Image image = button != null ? button.GetComponent<Image>() : null;
            if (image != null)
            {
                image.color = active ? ThemeBuildNearActive : ThemeSecondaryCard;
            }

            if (label != null)
            {
                label.color = active ? ThemeRarityOrange : ThemeBody;
            }
        }

        private static void ResetScroll(ScrollRect scrollRect)
        {
            if (scrollRect == null)
            {
                return;
            }

            scrollRect.StopMovement();
            scrollRect.velocity = Vector2.zero;
            scrollRect.verticalNormalizedPosition = 1f;
        }

        private static string BuildModelIdentity(ItemDetailViewModel model)
        {
            if (model == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(model.placementId))
            {
                return model.placementId;
            }

            return string.IsNullOrWhiteSpace(model.itemInstanceId)
                ? model.itemId ?? string.Empty
                : model.itemInstanceId;
        }

        private static void ApplySectionRarityStyle(
            ItemDetailSectionView[] sections,
            Color rarityColor,
            string rarityKey,
            string rarityDisplayName)
        {
            if (sections == null)
            {
                return;
            }

            foreach (ItemDetailSectionView section in sections)
            {
                section?.SetRarityPresentation(rarityColor, rarityKey, rarityDisplayName);
            }
        }

        private static void ApplySectionItemPresentation(
            ItemDetailSectionView[] sections,
            string itemId,
            string baseItemId,
            string faMenName,
            string qiLeiName)
        {
            if (sections == null)
            {
                return;
            }

            foreach (ItemDetailSectionView section in sections)
            {
                section?.SetItemPresentation(itemId, baseItemId, faMenName, qiLeiName);
            }
        }

        private void ApplySectionTheme(ItemDetailSectionView[] sections)
        {
            if (sections == null)
            {
                return;
            }

            foreach (ItemDetailSectionView section in sections)
            {
                section?.SetVisualTheme(
                    IsBuildSection(section)
                        ? ResolveLegacyBuildPresentationTheme()
                        : visualTheme);
            }
        }

        private ItemDetailVisualTheme ResolveLegacyBuildPresentationTheme()
        {
            if (legacyBuildPresentationTheme != null
                && ReferenceEquals(
                    legacyBuildPresentationThemeSource,
                    visualTheme))
            {
                return legacyBuildPresentationTheme;
            }

            ReleaseLegacyBuildPresentationTheme();
            legacyBuildPresentationTheme = visualTheme != null
                ? Instantiate(visualTheme)
                : ScriptableObject.CreateInstance<ItemDetailVisualTheme>();
            legacyBuildPresentationTheme.name =
                "ItemDetailLegacyBuildPresentationTheme";
            legacyBuildPresentationTheme.hideFlags =
                HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            legacyBuildPresentationTheme.buildInactive =
                LegacyBuildInactiveColor;
            legacyBuildPresentationThemeSource = visualTheme;
            return legacyBuildPresentationTheme;
        }

        private void ReleaseLegacyBuildPresentationTheme()
        {
            ItemDetailVisualTheme theme = legacyBuildPresentationTheme;
            legacyBuildPresentationTheme = null;
            legacyBuildPresentationThemeSource = null;
            if (theme == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(theme);
            }
            else
            {
                DestroyImmediate(theme);
            }
        }

        private static bool IsBuildSection(ItemDetailSectionView section)
        {
            return section != null
                && (string.Equals(
                        section.gameObject.name,
                        "FaMenBuildSection",
                        StringComparison.Ordinal)
                    || string.Equals(
                        section.gameObject.name,
                        "QiLeiBuildSection",
                        StringComparison.Ordinal));
        }

        private static void SetSectionsPreserveAuthoredVisualStyle(
            ItemDetailSectionView[] sections,
            bool preserve)
        {
            if (sections == null)
            {
                return;
            }

            foreach (ItemDetailSectionView section in sections)
            {
                section?.SetPreserveAuthoredVisualStyle(preserve);
            }
        }

        private string BuildMetaText(ItemDetailViewModel model, string rarityDisplayName, Color rarityColor)
        {
            string rarity = rarityDisplayName ?? string.Empty;
            string faMen = model.displayFaMenName ?? string.Empty;
            string qiLei = model.displayQiLeiName ?? string.Empty;
            string shape = SanitizeShapeName(model.displayShapeName);
            if (preserveAuthoredVisualStyle)
            {
                return string.Join(" · ", new[]
                {
                    string.IsNullOrWhiteSpace(rarity) ? string.Empty : $"<b>{rarity}</b>",
                    faMen,
                    qiLei,
                    shape
                }.Where(value => !string.IsNullOrWhiteSpace(value)));
            }

            string rarityLine = string.IsNullOrWhiteSpace(rarity)
                ? string.Empty
                : $"<color=#{ColorUtility.ToHtmlStringRGB(rarityColor)}><b>{rarity}</b></color>";
            string identity = string.Join(" / ", new[] { faMen, qiLei }
                .Where(value => !string.IsNullOrWhiteSpace(value)));
            string detailLine = string.Join(" · ", new[] { identity, shape }
                .Where(value => !string.IsNullOrWhiteSpace(value)));
            if (!string.IsNullOrWhiteSpace(detailLine))
            {
                detailLine = $"<color=#{ColorUtility.ToHtmlStringRGB(ThemeBody)}>· {detailLine}</color>";
            }

            return string.Join("\n", new[] { rarityLine, detailLine }
                .Where(value => !string.IsNullOrWhiteSpace(value)));
        }

        private string BuildPowerText(string rawPower, Color rarityColor)
        {
            if (!int.TryParse(rawPower, out int power) || power < 0)
            {
                return $"物品强度  <color=#{ColorUtility.ToHtmlStringRGB(ThemeWeak)}><b>{ItemDetailPresentationFormatter.ItemPowerUnavailable}</b></color>";
            }

            string rarityHex = ColorUtility.ToHtmlStringRGB(rarityColor);
            return $"物品强度  <color=#{rarityHex}><b>{power}</b></color>";
        }

        private Color ResolveRarityColor(string rarityKey, string displayRarityName)
        {
            string key = string.IsNullOrWhiteSpace(rarityKey) ? displayRarityName : rarityKey;
            if (string.IsNullOrWhiteSpace(key))
            {
                return ThemeRarityWhite;
            }

            key = key.Trim().ToLowerInvariant();
            if (key.Contains("orange") || key.Contains("cheng") || key.Contains("道") || key.Contains("橙"))
            {
                return ThemeRarityOrange;
            }

            if (key.Contains("purple") || key.Contains("zi") || key.Contains("玄") || key.Contains("紫"))
            {
                return ThemeRarityPurple;
            }

            if (key.Contains("blue") || key.Contains("lan") || key.Contains("灵") || key.Contains("藍") || key.Contains("蓝"))
            {
                return ThemeRarityBlue;
            }

            if (key.Contains("green") || key.Contains("qing") || key.Contains("良") || key.Contains("绿"))
            {
                return ThemeRarityGreen;
            }

            return ThemeRarityWhite;
        }

        private Color ThemeCard => visualTheme != null ? visualTheme.cardBackground : ItemDetailVisualThemeDefaults.CardBackground;
        private Color ThemeSecondaryCard => visualTheme != null ? visualTheme.secondaryCardBackground : ItemDetailVisualThemeDefaults.SecondaryCardBackground;
        private Color ThemeBody => visualTheme != null ? visualTheme.bodyText : ItemDetailVisualThemeDefaults.BodyText;
        private Color ThemeWeak => visualTheme != null ? visualTheme.weakText : ItemDetailVisualThemeDefaults.WeakText;
        private Color ThemeRestriction => visualTheme != null ? visualTheme.restrictionText : ItemDetailVisualThemeDefaults.RestrictionText;
        private Color ThemeBuildActive => visualTheme != null ? visualTheme.buildActive : ItemDetailVisualThemeDefaults.BuildActive;
        private Color ThemeBuildNearActive => visualTheme != null ? visualTheme.buildNearActive : ItemDetailVisualThemeDefaults.BuildNearActive;
        private Color ThemeArrayModifier => visualTheme != null ? visualTheme.arrayModifierColor : ItemDetailVisualThemeDefaults.ArrayModifierColor;
        private Color ThemeArtworkTint => visualTheme != null ? visualTheme.artworkTint : ItemDetailVisualThemeDefaults.ArtworkTint;
        private Color ThemeIconTint => visualTheme != null ? visualTheme.iconTint : ItemDetailVisualThemeDefaults.IconTint;
        private Color ThemeClearedStatusBadgeTint => visualTheme != null ? visualTheme.clearedStatusBadgeTint : ItemDetailVisualThemeDefaults.ClearedStatusBadgeTint;
        private Color ThemeRarityWhite => visualTheme != null ? visualTheme.rarityWhite : ItemDetailVisualThemeDefaults.RarityWhite;
        private Color ThemeRarityGreen => visualTheme != null ? visualTheme.rarityGreen : ItemDetailVisualThemeDefaults.RarityGreen;
        private Color ThemeRarityBlue => visualTheme != null ? visualTheme.rarityBlue : ItemDetailVisualThemeDefaults.RarityBlue;
        private Color ThemeRarityPurple => visualTheme != null ? visualTheme.rarityPurple : ItemDetailVisualThemeDefaults.RarityPurple;
        private Color ThemeRarityOrange => visualTheme != null ? visualTheme.rarityOrange : ItemDetailVisualThemeDefaults.RarityOrange;
        private float ThemeRarityArtworkFrameAlpha => visualTheme != null ? visualTheme.rarityArtworkFrameAlpha : ItemDetailVisualThemeDefaults.RarityArtworkFrameAlpha;
        private float ThemeRarityBadgeAlpha => visualTheme != null ? visualTheme.rarityBadgeAlpha : ItemDetailVisualThemeDefaults.RarityBadgeAlpha;
        private float ThemeRarityAccentAlpha => visualTheme != null ? visualTheme.rarityAccentAlpha : ItemDetailVisualThemeDefaults.RarityAccentAlpha;

        private static Color WithAlpha(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        private static string ResolveRarityDisplayName(string rarityKey, string displayRarityName)
        {
            string key = string.IsNullOrWhiteSpace(rarityKey) ? displayRarityName : rarityKey;
            if (string.IsNullOrWhiteSpace(key))
            {
                return "未定阶";
            }

            key = key.Trim().ToLowerInvariant();
            if (key.Contains("cheng") || key.Contains("orange") || key.Contains("道") || key.Contains("橙"))
            {
                return "道品";
            }

            if (key.Contains("zi") || key.Contains("purple") || key.Contains("玄") || key.Contains("紫"))
            {
                return "玄品";
            }

            if (key.Contains("lan") || key.Contains("blue") || key.Contains("灵") || key.Contains("藍") || key.Contains("蓝"))
            {
                return "灵品";
            }

            if (key.Contains("qing") || key.Contains("green") || key.Contains("良") || key.Contains("青") || key.Contains("绿"))
            {
                return "良品";
            }

            if (key.Contains("bai") || key.Contains("white") || key.Contains("凡") || key.Contains("白"))
            {
                return "凡品";
            }

            return displayRarityName ?? key;
        }

        private static string SanitizeShapeName(string shapeName)
        {
            if (string.IsNullOrWhiteSpace(shapeName))
            {
                return string.Empty;
            }

            string value = shapeName.Trim();
            if (value.Contains("square_4") || value.Contains("block2x2"))
            {
                return "方块四格";
            }

            if (value.Contains("corner_3") || value.Contains("corner3"))
            {
                return "折角三格";
            }

            if (value.Contains("vertical_3") || value.Contains("line3_v"))
            {
                return "竖排三格";
            }

            if (value.Contains("vertical_2") || value.Contains("line2_v"))
            {
                return "竖排两格";
            }

            if (value.Contains("single_1"))
            {
                return "单格";
            }

            if (value.Contains("line3_h"))
            {
                return "横排三格";
            }

            if (value.Contains("line2_h"))
            {
                return "横排两格";
            }

            return value;
        }

        private enum DetailTab
        {
            Detail,
            Debug
        }
    }

    internal sealed class ItemDetailOutsideDismissInputBlocker :
        MonoBehaviour,
        IPointerDownHandler
    {
        private ItemDetailPanelView ownerPanel;
        private Image outsideSurface;
        private bool originalRaycastTarget;
        private bool hasOriginalRaycastTarget;
        private Vector4 originalRaycastPadding;
        private Vector4 expandedRaycastPadding;
        private bool panelVisible;
        private bool captureUntilPointerRelease;

        public void Configure(
            ItemDetailPanelView panel,
            Image configuredOutsideSurface)
        {
            if (outsideSurface != null
                && !ReferenceEquals(outsideSurface, configuredOutsideSurface)
                && hasOriginalRaycastTarget)
            {
                outsideSurface.raycastTarget = originalRaycastTarget;
                outsideSurface.raycastPadding = originalRaycastPadding;
            }

            ownerPanel = panel;
            outsideSurface = configuredOutsideSurface;
            if (outsideSurface != null && !hasOriginalRaycastTarget)
            {
                originalRaycastTarget = outsideSurface.raycastTarget;
                originalRaycastPadding = outsideSurface.raycastPadding;
                hasOriginalRaycastTarget = true;
            }
            expandedRaycastPadding =
                ResolveRootCanvasRaycastPadding(outsideSurface);

            panelVisible = ownerPanel != null
                && ownerPanel.gameObject.activeInHierarchy;
            ApplyRaycastState();
        }

        public void SetPanelVisible(bool visible)
        {
            panelVisible = visible;
            if (!visible && IsPointerPressed())
            {
                captureUntilPointerRelease = true;
            }

            ApplyRaycastState();
        }

        public void Release(ItemDetailPanelView panel)
        {
            if (!ReferenceEquals(ownerPanel, panel))
            {
                return;
            }

            panelVisible = false;
            captureUntilPointerRelease = false;
            if (outsideSurface != null && hasOriginalRaycastTarget)
            {
                outsideSurface.raycastTarget = originalRaycastTarget;
                outsideSurface.raycastPadding = originalRaycastPadding;
            }

            ownerPanel = null;
            outsideSurface = null;
            enabled = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData == null
                || eventData.button != PointerEventData.InputButton.Left
                || ownerPanel == null
                || !panelVisible
                || ownerPanel.ContainsPopupContentScreenPoint(
                    eventData.position,
                    eventData.pressEventCamera))
            {
                return;
            }

            captureUntilPointerRelease = true;
            ApplyRaycastState();
            eventData.Use();
            ownerPanel.RequestClose();
        }

        private void Update()
        {
            if (!captureUntilPointerRelease || IsPointerPressed())
            {
                return;
            }

            captureUntilPointerRelease = false;
            ApplyRaycastState();
        }

        private void ApplyRaycastState()
        {
            bool shouldCapture = panelVisible || captureUntilPointerRelease;
            if (outsideSurface != null)
            {
                outsideSurface.raycastTarget = shouldCapture
                    ? true
                    : originalRaycastTarget;
                outsideSurface.raycastPadding = shouldCapture
                    ? expandedRaycastPadding
                    : originalRaycastPadding;
            }

            enabled = shouldCapture;
        }

        private static bool IsPointerPressed()
        {
            if (Input.GetMouseButton(0))
            {
                return true;
            }

            for (int index = 0; index < Input.touchCount; index++)
            {
                TouchPhase phase = Input.GetTouch(index).phase;
                if (phase != TouchPhase.Ended && phase != TouchPhase.Canceled)
                {
                    return true;
                }
            }

            return false;
        }

        private Vector4 ResolveRootCanvasRaycastPadding(Image surface)
        {
            RectTransform surfaceRect = surface != null
                ? surface.rectTransform
                : null;
            Canvas surfaceCanvas = surface != null
                ? surface.GetComponentInParent<Canvas>()
                : null;
            RectTransform rootRect = surfaceCanvas != null
                ? surfaceCanvas.rootCanvas.transform as RectTransform
                : null;
            if (surfaceRect == null || rootRect == null)
            {
                return originalRaycastPadding;
            }

            Vector3[] rootCorners = new Vector3[4];
            rootRect.GetWorldCorners(rootCorners);
            Vector3 first = surfaceRect.InverseTransformPoint(rootCorners[0]);
            float xMin = first.x;
            float yMin = first.y;
            float xMax = first.x;
            float yMax = first.y;
            for (int index = 1; index < rootCorners.Length; index++)
            {
                Vector3 local =
                    surfaceRect.InverseTransformPoint(rootCorners[index]);
                xMin = Mathf.Min(xMin, local.x);
                yMin = Mathf.Min(yMin, local.y);
                xMax = Mathf.Max(xMax, local.x);
                yMax = Mathf.Max(yMax, local.y);
            }

            Rect surfaceBounds = surfaceRect.rect;
            return new Vector4(
                Mathf.Min(
                    originalRaycastPadding.x,
                    xMin - surfaceBounds.xMin),
                Mathf.Min(
                    originalRaycastPadding.y,
                    yMin - surfaceBounds.yMin),
                Mathf.Min(
                    originalRaycastPadding.z,
                    surfaceBounds.xMax - xMax),
                Mathf.Min(
                    originalRaycastPadding.w,
                    surfaceBounds.yMax - yMax));
        }
    }
}
