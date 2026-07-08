using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TalismanBag.BuildSandbox
{
    public sealed class ShapeCellVisualStyle
    {
        public ShapeCellVisualStyle(
            Sprite sprite,
            Color color,
            Image.Type imageType,
            bool preserveAspect,
            bool fillCenter,
            Material material,
            float pixelsPerUnitMultiplier,
            float sourceRotationDegrees,
            bool spansWholeItem,
            bool hasRectTransformOverride = false,
            Vector2 anchorMin = default,
            Vector2 anchorMax = default,
            Vector2 pivot = default,
            Vector2 anchoredPosition = default,
            Vector2 sizeDelta = default,
            Vector3 localScale = default,
            Vector3 localEulerAngles = default)
        {
            Sprite = sprite;
            Color = color;
            ImageType = imageType;
            PreserveAspect = preserveAspect;
            FillCenter = fillCenter;
            Material = material;
            PixelsPerUnitMultiplier = pixelsPerUnitMultiplier;
            SourceRotationDegrees = sourceRotationDegrees;
            SpansWholeItem = spansWholeItem;
            HasRectTransformOverride = hasRectTransformOverride;
            AnchorMin = anchorMin;
            AnchorMax = anchorMax;
            Pivot = pivot;
            AnchoredPosition = anchoredPosition;
            SizeDelta = sizeDelta;
            LocalScale = localScale == default ? Vector3.one : localScale;
            LocalEulerAngles = localEulerAngles;
        }

        public Sprite Sprite { get; }
        public Color Color { get; }
        public Image.Type ImageType { get; }
        public bool PreserveAspect { get; }
        public bool FillCenter { get; }
        public Material Material { get; }
        public float PixelsPerUnitMultiplier { get; }
        public float SourceRotationDegrees { get; }
        public bool SpansWholeItem { get; }
        public bool HasRectTransformOverride { get; }
        public Vector2 AnchorMin { get; }
        public Vector2 AnchorMax { get; }
        public Vector2 Pivot { get; }
        public Vector2 AnchoredPosition { get; }
        public Vector2 SizeDelta { get; }
        public Vector3 LocalScale { get; }
        public Vector3 LocalEulerAngles { get; }

        public static ShapeCellVisualStyle FromImage(
            Image image,
            bool spansWholeItem = false,
            bool captureRectTransform = false)
        {
            if (image == null)
            {
                return null;
            }

            Sprite sprite = image.overrideSprite != null ? image.overrideSprite : image.sprite;
            return new ShapeCellVisualStyle(
                sprite,
                image.color,
                image.type,
                image.preserveAspect,
                image.fillCenter,
                image.material,
                image.pixelsPerUnitMultiplier,
                ResolveSourceRotationDegrees(image),
                spansWholeItem,
                hasRectTransformOverride: captureRectTransform && image.rectTransform != null,
                anchorMin: !captureRectTransform || image.rectTransform == null ? default : image.rectTransform.anchorMin,
                anchorMax: !captureRectTransform || image.rectTransform == null ? default : image.rectTransform.anchorMax,
                pivot: !captureRectTransform || image.rectTransform == null ? default : image.rectTransform.pivot,
                anchoredPosition: !captureRectTransform || image.rectTransform == null ? default : image.rectTransform.anchoredPosition,
                sizeDelta: !captureRectTransform || image.rectTransform == null ? default : image.rectTransform.sizeDelta,
                localScale: !captureRectTransform || image.rectTransform == null ? Vector3.one : image.rectTransform.localScale,
                localEulerAngles: !captureRectTransform || image.rectTransform == null ? default : image.rectTransform.localEulerAngles);
        }

        public ShapeCellVisualStyle WithColor(Color color)
        {
            return new ShapeCellVisualStyle(
                Sprite,
                color,
                ImageType,
                PreserveAspect,
                FillCenter,
                Material,
                PixelsPerUnitMultiplier,
                SourceRotationDegrees,
                SpansWholeItem,
                HasRectTransformOverride,
                AnchorMin,
                AnchorMax,
                Pivot,
                AnchoredPosition,
                SizeDelta,
                LocalScale,
                LocalEulerAngles);
        }

        public void ApplyTo(Image image, Color fallbackColor, float maxAlpha = 1f)
        {
            if (image == null)
            {
                return;
            }

            image.overrideSprite = null;
            image.sprite = Sprite;
            image.type = Sprite == null ? Image.Type.Simple : ImageType;
            image.preserveAspect = PreserveAspect;
            image.fillCenter = FillCenter;
            image.material = Material;
            image.pixelsPerUnitMultiplier = PixelsPerUnitMultiplier;

            Color targetColor = Color;
            if (Sprite == null && Mathf.Approximately(targetColor.a, 0f))
            {
                targetColor = fallbackColor;
            }

            if (maxAlpha < 1f)
            {
                targetColor.a = Mathf.Min(targetColor.a, maxAlpha);
            }

            image.color = targetColor;
        }

        private static float ResolveSourceRotationDegrees(Image image)
        {
            return image != null && image.rectTransform != null
                ? NormalizeRotationDegrees(image.rectTransform.localEulerAngles.z)
                : 0f;
        }

        private static float NormalizeRotationDegrees(float degrees)
        {
            while (degrees > 180f)
            {
                degrees -= 360f;
            }

            while (degrees <= -180f)
            {
                degrees += 360f;
            }

            return degrees;
        }
    }

    public sealed class BuildItemPreviewCardView : MonoBehaviour,
        IInitializePotentialDragHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IPointerClickHandler
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Text titleText;
        [SerializeField] private Text categoryText;
        [SerializeField] private Text shapeText;

        private const string LayoutCellLayerName = "TrayLayoutCellLayer";
        private static readonly Color DefaultNormalColor = new(0.27f, 0.22f, 0.15f, 1f);
        private const float ColorTolerance = 0.004f;
        private const float ScrollAxisBias = 1.15f;
        private const float ItemDragHoldSeconds = 0.22f;
        private const float ItemDragMoveThresholdPixels = MobileShapePlacementInputSettings.DefaultDragMoveThresholdPixels;
        private const float ViewportExitMarginPixels = 28f;

        private BuildGridInteractionPreviewController controller;
        private readonly List<Image> layoutCellImages = new();
        private readonly HashSet<Image> manualLayoutCellImageColors = new();
        private readonly Dictionary<Image, float> originalArtworkImageRotationByImage = new();
        private DragGestureMode dragGestureMode;
        private ScrollRect gestureScrollRect;
        private RectTransform gestureScrollViewport;
        private Color normalColor = DefaultNormalColor;
        private string itemId = string.Empty;
        private string category = string.Empty;
        private bool usesLayoutCellVisuals;
        private bool manualBackgroundImageColor;
        private bool forwardedScrollBegin;
        private bool suppressClick;
        private float pointerDownTime;

        private enum DragGestureMode
        {
            None,
            Pending,
            Scrolling,
            ItemDragging
        }

        public RectTransform RectTransform
        {
            get
            {
                if (rectTransform == null)
                {
                    rectTransform = GetComponent<RectTransform>();
                }

                return rectTransform;
            }
        }

        public string ItemId => itemId;
        public string Category => category;
        public bool HasPreviewContent =>
            !string.IsNullOrWhiteSpace(itemId)
            || (titleText != null && !string.IsNullOrWhiteSpace(titleText.text))
            || (categoryText != null && !string.IsNullOrWhiteSpace(categoryText.text))
            || (shapeText != null && !string.IsNullOrWhiteSpace(shapeText.text));

        public void Bind(
            RectTransform rect,
            CanvasGroup group,
            Image background,
            Text title,
            Text categoryLabel,
            Text shapeLabel)
        {
            rectTransform = rect;
            canvasGroup = group;
            backgroundImage = background;
            titleText = title;
            categoryText = categoryLabel;
            shapeText = shapeLabel;
            manualBackgroundImageColor = HasManualBodyColor(backgroundImage);
        }

        public void BindItemDisplayData(
            BuildGridInteractionPreviewController owner,
            string previewItemId,
            string itemName,
            string itemCategory,
            string shapeDisplayName,
            Color color)
        {
            controller = owner;
            itemId = previewItemId ?? string.Empty;
            category = itemCategory ?? string.Empty;
            normalColor = color;
            manualBackgroundImageColor = manualBackgroundImageColor || HasManualBodyColor(backgroundImage);
            UseParentLayerSorting();

            if (titleText != null)
            {
                titleText.text = itemName ?? string.Empty;
            }

            if (categoryText != null)
            {
                categoryText.text = category;
            }

            if (shapeText != null)
            {
                shapeText.text = shapeDisplayName ?? string.Empty;
            }

            ConfigureTextLayout();
            SetNormalVisual();
        }

        public void Clear()
        {
            controller = null;
            itemId = string.Empty;
            category = string.Empty;

            if (titleText != null)
            {
                titleText.text = string.Empty;
            }

            if (categoryText != null)
            {
                categoryText.text = string.Empty;
            }

            if (shapeText != null)
            {
                shapeText.text = string.Empty;
            }

            manualBackgroundImageColor = manualBackgroundImageColor || HasManualBodyColor(backgroundImage);
            CacheManualLayoutCellColors();
            SetNormalVisual();
            SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void SetNormalVisual()
        {
            ApplyBodyColor(normalColor);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
            }
        }

        public void SetSelectedVisual()
        {
            ApplyBodyColor(Color.Lerp(normalColor, new Color(1f, 0.82f, 0.34f, 1f), 0.22f));

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
            }
        }

        public void SetDraggingVisual()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0.58f;
                canvasGroup.blocksRaycasts = true;
            }
        }

        public void SetRotateButtonInteractable(bool interactable)
        {
        }

        public void SetLayoutCellVisuals(IReadOnlyList<Image> cellImages)
        {
            layoutCellImages.Clear();
            manualLayoutCellImageColors.Clear();
            foreach (Image image in cellImages ?? Array.Empty<Image>())
            {
                if (image == null)
                {
                    continue;
                }

                layoutCellImages.Add(image);
                if (HasManualBodyColor(image))
                {
                    manualLayoutCellImageColors.Add(image);
                }

                image.raycastTarget = true;
            }

            usesLayoutCellVisuals = layoutCellImages.Count > 0;
            if (backgroundImage != null)
            {
                backgroundImage.raycastTarget = !usesLayoutCellVisuals;
            }

            SetNormalVisual();
        }

        public void ApplyArtworkImageRotationOffset(float rotationOffsetDegrees)
        {
            Image image = FindBestArtworkImage();
            if (image == null || image.rectTransform == null)
            {
                return;
            }

            if (!originalArtworkImageRotationByImage.TryGetValue(image, out float originalRotationDegrees))
            {
                originalRotationDegrees = image.rectTransform.localEulerAngles.z;
                originalArtworkImageRotationByImage[image] = originalRotationDegrees;
            }

            Vector3 eulerAngles = image.rectTransform.localEulerAngles;
            eulerAngles.z = originalRotationDegrees + rotationOffsetDegrees;
            image.rectTransform.localEulerAngles = eulerAngles;
        }

        public bool TryCaptureCellVisualStyles(List<ShapeCellVisualStyle> styles)
        {
            if (styles == null)
            {
                return false;
            }

            styles.Clear();
            if (TryCaptureArtworkImageVisualStyle(styles))
            {
                return true;
            }

            foreach (Image image in layoutCellImages)
            {
                if (image == null
                    || !image.gameObject.activeSelf
                    || !HasCaptureableImageSource(image))
                {
                    continue;
                }

                ShapeCellVisualStyle style = ShapeCellVisualStyle.FromImage(image);
                if (style != null)
                {
                    styles.Add(style);
                }
            }

            if (styles.Count > 0)
            {
                return true;
            }

            if (backgroundImage == null
                || (backgroundImage.sprite == null
                    && backgroundImage.overrideSprite == null
                    && !manualBackgroundImageColor))
            {
                return false;
            }

            bool backgroundSpriteArtwork = backgroundImage.sprite != null || backgroundImage.overrideSprite != null;
            ShapeCellVisualStyle backgroundStyle = ShapeCellVisualStyle.FromImage(
                backgroundImage,
                spansWholeItem: backgroundSpriteArtwork);
            if (backgroundStyle == null)
            {
                return false;
            }

            styles.Add(backgroundStyle);
            return true;
        }

        private bool TryCaptureArtworkImageVisualStyle(List<ShapeCellVisualStyle> styles)
        {
            if (styles == null)
            {
                return false;
            }

            ShapeCellVisualStyle style = ShapeCellVisualStyle.FromImage(FindBestArtworkImage(), spansWholeItem: true);
            if (style == null)
            {
                return false;
            }

            styles.Add(style);
            return true;
        }

        private Image FindBestArtworkImage()
        {
            Image bestImage = null;
            float bestScore = float.NegativeInfinity;
            foreach (Image image in GetComponentsInChildren<Image>(true))
            {
                if (!IsArtworkImageCandidate(image))
                {
                    continue;
                }

                float score = ScoreArtworkImageCandidate(image);
                if (bestImage == null || score > bestScore)
                {
                    bestImage = image;
                    bestScore = score;
                }
            }

            return bestImage;
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            dragGestureMode = DragGestureMode.None;
            forwardedScrollBegin = false;
            gestureScrollRect = ResolveScrollRect();
            gestureScrollViewport = ResolveScrollViewport(gestureScrollRect);
            if (CanRouteToScroll())
            {
                ExecuteEvents.Execute(
                    gestureScrollRect.gameObject,
                    eventData,
                    ExecuteEvents.initializePotentialDrag);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            pointerDownTime = Time.unscaledTime;
            suppressClick = false;
            dragGestureMode = DragGestureMode.None;
            gestureScrollRect = ResolveScrollRect();
            gestureScrollViewport = ResolveScrollViewport(gestureScrollRect);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (dragGestureMode == DragGestureMode.None || dragGestureMode == DragGestureMode.Pending)
            {
                ResetGestureRouting(clearClickSuppression: false);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            suppressClick = true;
            dragGestureMode = DragGestureMode.Pending;
            gestureScrollRect = ResolveScrollRect();
            gestureScrollViewport = ResolveScrollViewport(gestureScrollRect);
            ResolvePendingDragRoute(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragGestureMode == DragGestureMode.None)
            {
                dragGestureMode = DragGestureMode.Pending;
                gestureScrollRect = ResolveScrollRect();
                gestureScrollViewport = ResolveScrollViewport(gestureScrollRect);
            }

            if (dragGestureMode == DragGestureMode.Pending)
            {
                ResolvePendingDragRoute(eventData);
            }

            if (dragGestureMode == DragGestureMode.Scrolling)
            {
                if (ShouldSwitchFromScrollToItemDrag(eventData))
                {
                    EndScrollDrag(eventData);
                    StopScrollMomentum();
                    BeginItemDrag(eventData);
                    controller?.UpdateDrag(this, eventData);
                    return;
                }

                ForwardScrollDrag(eventData);
                return;
            }

            if (dragGestureMode == DragGestureMode.ItemDragging)
            {
                controller?.UpdateDrag(this, eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (dragGestureMode == DragGestureMode.Scrolling)
            {
                EndScrollDrag(eventData);
                ResetGestureRouting(clearClickSuppression: false);
                return;
            }

            if (dragGestureMode == DragGestureMode.ItemDragging)
            {
                controller?.EndDrag(this, eventData);
                SetNormalVisual();
            }

            ResetGestureRouting(clearClickSuppression: false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (suppressClick)
            {
                suppressClick = false;
                return;
            }

            controller?.SelectItem(this);
        }

        private void ResolvePendingDragRoute(PointerEventData eventData)
        {
            if (!CanRouteToScroll()
                || ShouldStartItemDrag(eventData)
                || !ShouldStartScroll(eventData))
            {
                BeginItemDrag(eventData);
                return;
            }

            BeginScrollDrag(eventData);
        }

        private void BeginItemDrag(PointerEventData eventData)
        {
            dragGestureMode = DragGestureMode.ItemDragging;
            controller?.BeginDrag(this, eventData);
            SetDraggingVisual();
        }

        private void BeginScrollDrag(PointerEventData eventData)
        {
            if (!CanRouteToScroll())
            {
                BeginItemDrag(eventData);
                return;
            }

            dragGestureMode = DragGestureMode.Scrolling;
            ExecuteEvents.Execute(
                gestureScrollRect.gameObject,
                eventData,
                ExecuteEvents.beginDragHandler);
            forwardedScrollBegin = true;
        }

        private void ForwardScrollDrag(PointerEventData eventData)
        {
            if (!CanRouteToScroll())
            {
                return;
            }

            if (!forwardedScrollBegin)
            {
                BeginScrollDrag(eventData);
            }

            ExecuteEvents.Execute(
                gestureScrollRect.gameObject,
                eventData,
                ExecuteEvents.dragHandler);
        }

        private void EndScrollDrag(PointerEventData eventData)
        {
            if (!forwardedScrollBegin || !CanRouteToScroll())
            {
                forwardedScrollBegin = false;
                return;
            }

            ExecuteEvents.Execute(
                gestureScrollRect.gameObject,
                eventData,
                ExecuteEvents.endDragHandler);
            forwardedScrollBegin = false;
        }

        private void StopScrollMomentum()
        {
            if (gestureScrollRect != null)
            {
                gestureScrollRect.velocity = Vector2.zero;
            }
        }

        private bool ShouldStartItemDrag(PointerEventData eventData)
        {
            if (!CanRouteToScroll())
            {
                return true;
            }

            if (Time.unscaledTime - pointerDownTime >= ItemDragHoldSeconds)
            {
                return true;
            }

            if (IsPointerOutsideViewport(eventData, ViewportExitMarginPixels))
            {
                return true;
            }

            Vector2 dragDelta = ResolveDragDelta(eventData);
            float horizontalDistance = Mathf.Abs(dragDelta.x);
            return horizontalDistance >= ItemDragMoveThresholdPixels
                && horizontalDistance > Mathf.Abs(dragDelta.y);
        }

        private bool ShouldStartScroll(PointerEventData eventData)
        {
            if (!CanRouteToScroll() || IsPointerOutsideViewport(eventData, 0f))
            {
                return false;
            }

            Vector2 dragDelta = ResolveDragDelta(eventData);
            return Mathf.Abs(dragDelta.y) >= Mathf.Abs(dragDelta.x) * ScrollAxisBias;
        }

        private bool ShouldSwitchFromScrollToItemDrag(PointerEventData eventData)
        {
            return IsPointerOutsideViewport(eventData, ViewportExitMarginPixels);
        }

        private Vector2 ResolveDragDelta(PointerEventData eventData)
        {
            return eventData == null
                ? Vector2.zero
                : eventData.position - eventData.pressPosition;
        }

        private bool IsPointerOutsideViewport(PointerEventData eventData, float marginPixels)
        {
            if (gestureScrollViewport == null || eventData == null)
            {
                return false;
            }

            Camera eventCamera = eventData.pressEventCamera ?? eventData.enterEventCamera;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                gestureScrollViewport,
                eventData.position,
                eventCamera,
                out Vector2 localPoint))
            {
                return false;
            }

            Rect rect = gestureScrollViewport.rect;
            rect.xMin -= marginPixels;
            rect.xMax += marginPixels;
            rect.yMin -= marginPixels;
            rect.yMax += marginPixels;
            return !rect.Contains(localPoint);
        }

        private ScrollRect ResolveScrollRect()
        {
            return gestureScrollRect != null
                ? gestureScrollRect
                : GetComponentInParent<ScrollRect>();
        }

        private static RectTransform ResolveScrollViewport(ScrollRect scrollRect)
        {
            if (scrollRect == null)
            {
                return null;
            }

            if (scrollRect.viewport != null)
            {
                return scrollRect.viewport;
            }

            return scrollRect.content == null
                ? null
                : scrollRect.content.parent as RectTransform;
        }

        private bool CanRouteToScroll()
        {
            return gestureScrollRect != null
                && gestureScrollRect.isActiveAndEnabled
                && gestureScrollRect.vertical
                && gestureScrollRect.content != null;
        }

        private void ResetGestureRouting(bool clearClickSuppression)
        {
            dragGestureMode = DragGestureMode.None;
            forwardedScrollBegin = false;
            gestureScrollRect = null;
            gestureScrollViewport = null;
            pointerDownTime = 0f;
            if (clearClickSuppression)
            {
                suppressClick = false;
            }
        }

        private void ConfigureTextLayout()
        {
            ConfigureTextRect(titleText, new Vector2(0.04f, 0.64f), new Vector2(0.97f, 0.98f), TextAnchor.UpperCenter, 11);
            ConfigureTextRect(categoryText, new Vector2(0.04f, 0.40f), new Vector2(0.97f, 0.65f), TextAnchor.MiddleCenter, 10);
            ConfigureTextRect(shapeText, new Vector2(0.03f, 0.02f), new Vector2(0.97f, 0.38f), TextAnchor.UpperCenter, 10);
        }

        private static void ConfigureTextRect(
            Text text,
            Vector2 anchorMin,
            Vector2 anchorMax,
            TextAnchor alignment,
            int fontSize)
        {
            if (text == null)
            {
                return;
            }

            text.alignment = alignment;
            text.fontSize = fontSize;
            text.raycastTarget = false;
            RectTransform textRect = text.rectTransform;
            textRect.anchorMin = anchorMin;
            textRect.anchorMax = anchorMax;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            textRect.localScale = Vector3.one;
        }

        private void UseParentLayerSorting()
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                return;
            }

            canvas.overrideSorting = false;
            canvas.sortingOrder = 0;
        }

        private void ApplyBodyColor(Color color)
        {
            if (backgroundImage != null && !manualBackgroundImageColor)
            {
                backgroundImage.color = usesLayoutCellVisuals ? Color.clear : color;
            }

            foreach (Image image in layoutCellImages)
            {
                if (image != null && !manualLayoutCellImageColors.Contains(image))
                {
                    image.color = color;
                }
            }
        }

        private void CacheManualLayoutCellColors()
        {
            manualLayoutCellImageColors.Clear();
            foreach (Image image in layoutCellImages)
            {
                if (HasManualBodyColor(image))
                {
                    manualLayoutCellImageColors.Add(image);
                }
            }
        }

        private bool HasManualBodyColor(Image image)
        {
            return image != null
                && !IsDefaultGeneratedLayoutCellWhite(image)
                && !IsKnownBodyColor(image.color);
        }

        private bool HasCaptureableImageSource(Image image)
        {
            return image != null
                && (image.sprite != null
                    || image.overrideSprite != null
                    || HasManualBodyColor(image));
        }

        private bool IsArtworkImageCandidate(Image image)
        {
            return image != null
                && image != backgroundImage
                && !layoutCellImages.Contains(image)
                && !IsGeneratedLayoutCellImage(image)
                && image.color.a > ColorTolerance
                && (image.sprite != null || image.overrideSprite != null);
        }

        private static bool IsGeneratedLayoutCellImage(Image image)
        {
            return image != null
                && image.transform != null
                && image.transform.parent != null
                && string.Equals(image.transform.parent.name, LayoutCellLayerName, StringComparison.Ordinal);
        }

        private static float ScoreArtworkImageCandidate(Image image)
        {
            if (image == null)
            {
                return float.NegativeInfinity;
            }

            float score = 0f;
            if (image.gameObject.activeInHierarchy)
            {
                score += 1000f;
            }
            else if (image.gameObject.activeSelf)
            {
                score += 500f;
            }

            string name = image.name ?? string.Empty;
            if (ContainsNameHint(name, "art", "artwork", "icon", "image", "picture", "sprite", "visual", "slot"))
            {
                score += 200f;
            }

            if (ContainsNameHint(name, "background", "bg", "frame", "mask", "outline", "border"))
            {
                score -= 160f;
            }

            RectTransform rect = image.rectTransform;
            if (rect != null)
            {
                float area = Mathf.Max(0f, rect.rect.width) * Mathf.Max(0f, rect.rect.height);
                score += Mathf.Min(area * 0.001f, 80f);
            }

            Transform cursor = image.transform;
            while (cursor != null)
            {
                score += 1f;
                cursor = cursor.parent;
            }

            return score;
        }

        private static bool ContainsNameHint(string name, params string[] hints)
        {
            if (string.IsNullOrWhiteSpace(name) || hints == null)
            {
                return false;
            }

            foreach (string hint in hints)
            {
                if (!string.IsNullOrWhiteSpace(hint)
                    && name.IndexOf(hint, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsDefaultGeneratedLayoutCellWhite(Image image)
        {
            return image != null
                && image.sprite == null
                && image.overrideSprite == null
                && image.transform != null
                && image.transform.parent != null
                && string.Equals(image.transform.parent.name, LayoutCellLayerName, StringComparison.Ordinal)
                && Approximately(image.color, Color.white);
        }

        private bool IsKnownBodyColor(Color color)
        {
            if (Approximately(color, Color.clear)
                || Approximately(color, DefaultNormalColor)
                || Approximately(color, normalColor)
                || Approximately(color, Color.Lerp(normalColor, new Color(1f, 0.82f, 0.34f, 1f), 0.22f)))
            {
                return true;
            }

            foreach (BuildGridInteractionPreviewController.PreviewItem item
                     in BuildGridInteractionPreviewController.CreatePreviewItems())
            {
                if (item == null)
                {
                    continue;
                }

                if (Approximately(color, item.CardColor)
                    || Approximately(color, Color.Lerp(item.CardColor, new Color(1f, 0.82f, 0.34f, 1f), 0.22f)))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool Approximately(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) <= ColorTolerance
                && Mathf.Abs(a.g - b.g) <= ColorTolerance
                && Mathf.Abs(a.b - b.b) <= ColorTolerance
                && Mathf.Abs(a.a - b.a) <= ColorTolerance;
        }

        private void Reset()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            backgroundImage = GetComponent<Image>();
        }
    }
}
