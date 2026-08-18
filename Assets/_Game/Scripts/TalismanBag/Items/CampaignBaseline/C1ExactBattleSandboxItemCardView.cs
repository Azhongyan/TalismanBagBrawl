using System;
using System.Linq;
using TalismanBag.Items.Canonical;
using TalismanBag.Presentation.Items;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TalismanBag.Items.CampaignBaseline
{
    [DisallowMultipleComponent]
    public sealed class C1ExactBattleSandboxItemCardView : MonoBehaviour,
        IInitializePotentialDragHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerClickHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        public const string MissingReferenceDiagnostic =
            "C1_EXACT_BATTLESANDBOX_CARD_AUTHORED_REFERENCE_MISSING";
        public const string UnknownArtworkDiagnostic =
            "C1_EXACT_BATTLESANDBOX_CARD_ARTWORK_UNKNOWN";

        private const float LongPressSeconds = 0.22f;
        private const float HorizontalDragThresholdPixels = 18f;
        private const float ScrollAxisBias = 1.15f;
        private const float ViewportExitMarginPixels = 28f;
        private const float VerticalScrollRangeLayoutEpsilon = 0.5f;

        [SerializeField] private RectTransform cardRect;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image artworkImage;
        [SerializeField] private Image selectionOverlay;
        [SerializeField] private Image lightingOverlay;
        [SerializeField] private Image dragFeedbackOverlay;
        [SerializeField] private Image inputSurface;
        [SerializeField] private RectTransform sourceFootprintLayer;
        [SerializeField] private Image[] sourceFootprintCells = Array.Empty<Image>();
        [SerializeField] private Text identityLabel;
        [SerializeField] private Text categoryLabel;
        [SerializeField] private Text shapeLabel;
        [SerializeField] private Text rotateFeedbackLabel;
        [SerializeField] private ScrollRect authoredScrollRect;
        [SerializeField] private ItemRarityContourBloomVfx
            rarityContourBloomVfx;

        private C1ExactBattleSandboxItemArrangementPresenter presenter;
        private string itemInstanceId = string.Empty;
        private string baseItemId = string.Empty;
        private bool isPlaced;
        private int rotation;
        private bool pointerIsDown;
        private bool suppressClick;
        private float pointerDownTime;
        private DragRoute dragRoute;
        private bool forwardedScrollBegin;
        private bool invalidFormalRotationLogged;
        private bool authoredLayoutCaptured;
        private NormalizedBounds artworkBounds;
        private NormalizedBounds selectionBounds;
        private NormalizedBounds lightingBounds;
        private NormalizedBounds dragBounds;
        private Vector2 currentFootprintSize;
        private C1ExactBattleSandboxItemFootprint currentFootprint;
        private Vector2Int pressedRawGrabbedCell;
        private bool hasPressedRawGrabbedCell;

        private enum DragRoute
        {
            None = 0,
            Pending = 1,
            Scrolling = 2,
            Item = 3
        }

        private readonly struct NormalizedBounds
        {
            public NormalizedBounds(Vector2 min, Vector2 max)
            {
                Min = min;
                Max = max;
            }

            public Vector2 Min { get; }
            public Vector2 Max { get; }
            public Vector2 Center => (Min + Max) * 0.5f;
            public Vector2 Size => Max - Min;
        }

        public string ItemInstanceId => itemInstanceId;
        public string BaseItemId => baseItemId;
        public bool IsPlaced => isPlaced;
        public int Rotation => rotation;
        public Sprite CurrentArtwork => artworkImage == null ? null : artworkImage.sprite;
        public ItemRarityContourBloomVfx RarityContourBloomVfx =>
            rarityContourBloomVfx;
        public bool HasBoundItem => itemInstanceId.Length > 0 && gameObject.activeSelf;
        internal Vector2 CurrentFootprintSize => currentFootprintSize;
        internal bool ArtworkPreservesAspect => artworkImage != null
                                                 && artworkImage.preserveAspect;
        internal bool SourceFootprintLayerActive => sourceFootprintLayer != null
                                                    && sourceFootprintLayer
                                                        .gameObject.activeSelf;
        internal int SourceFootprintRaycastCount => (sourceFootprintCells
            ?? Array.Empty<Image>()).Count(image => image != null
                && image.raycastTarget);
        internal bool InputRaycastEnabled => inputSurface != null
                                             && inputSurface.raycastTarget;
        internal bool HasPresentationResidue => itemInstanceId.Length > 0
                                                || baseItemId.Length > 0
                                                || CurrentArtwork != null
                                                || (rarityContourBloomVfx != null
                                                    && rarityContourBloomVfx
                                                        .HasAppliedPresentation)
                                                || InputRaycastEnabled
                                                || (canvasGroup != null
                                                    && (canvasGroup.blocksRaycasts
                                                        || canvasGroup.interactable));

        public void AssignForEditor(
            RectTransform configuredCardRect,
            CanvasGroup configuredCanvasGroup,
            Image configuredBackgroundImage,
            Image configuredArtworkImage,
            Image configuredSelectionOverlay,
            Image configuredLightingOverlay,
            Image configuredDragFeedbackOverlay,
            Image configuredInputSurface,
            RectTransform configuredSourceFootprintLayer,
            Image[] configuredSourceFootprintCells,
            Text configuredIdentityLabel,
            Text configuredCategoryLabel,
            Text configuredShapeLabel,
            Text configuredRotateFeedbackLabel,
            ScrollRect configuredScrollRect,
            ItemRarityContourBloomVfx configuredRarityContourBloomVfx = null)
        {
            cardRect = configuredCardRect;
            canvasGroup = configuredCanvasGroup;
            backgroundImage = configuredBackgroundImage;
            artworkImage = configuredArtworkImage;
            selectionOverlay = configuredSelectionOverlay;
            lightingOverlay = configuredLightingOverlay;
            dragFeedbackOverlay = configuredDragFeedbackOverlay;
            inputSurface = configuredInputSurface;
            sourceFootprintLayer = configuredSourceFootprintLayer;
            sourceFootprintCells = configuredSourceFootprintCells
                                   ?? Array.Empty<Image>();
            identityLabel = configuredIdentityLabel;
            categoryLabel = configuredCategoryLabel;
            shapeLabel = configuredShapeLabel;
            rotateFeedbackLabel = configuredRotateFeedbackLabel;
            authoredScrollRect = configuredScrollRect;
            rarityContourBloomVfx = configuredRarityContourBloomVfx;
        }

        public bool ValidateAuthoredReferences()
        {
            return cardRect != null
                   && canvasGroup != null
                   && backgroundImage != null
                   && artworkImage != null
                   && selectionOverlay != null
                   && lightingOverlay != null
                   && dragFeedbackOverlay != null
                   && inputSurface != null
                   && sourceFootprintLayer != null
                   && sourceFootprintLayer.parent == cardRect
                   && sourceFootprintCells != null
                   && sourceFootprintCells.Length == 3
                   && sourceFootprintCells.All(image => image != null
                       && image.transform.parent == sourceFootprintLayer
                       && !image.raycastTarget)
                   && sourceFootprintCells.Distinct().Count() == 3
                   && backgroundImage.rectTransform == cardRect
                   && artworkImage.rectTransform.parent == cardRect
                   && selectionOverlay.rectTransform.parent == cardRect
                   && lightingOverlay.rectTransform.parent == cardRect
                   && dragFeedbackOverlay.rectTransform.parent == cardRect
                   && inputSurface.rectTransform.parent == cardRect
                   && identityLabel != null
                   && categoryLabel != null
                   && shapeLabel != null
                   && rotateFeedbackLabel != null
                   && artworkImage.preserveAspect
                   && rarityContourBloomVfx != null
                   && rarityContourBloomVfx.transform != cardRect
                   && rarityContourBloomVfx.transform.IsChildOf(cardRect)
                   && rarityContourBloomVfx.ValidateAuthoredReferences();
        }

        internal void BindPresenter(
            C1ExactBattleSandboxItemArrangementPresenter configuredPresenter)
        {
            presenter = configuredPresenter;
            if (!ItemInteractionEnabled) CancelInteractionState();
        }

        internal void CancelInteractionState()
        {
            SetDragging(false);
            SetRotateFeedback(0, false);
            ResetGesture(false);
            suppressClick = true;
        }

        internal bool Bind(
            C1FormalItemRosterEntrySnapshot roster,
            C1FormalItemPlacementSnapshot placement,
            ItemSystemPlacementSnapshot resolvedPlacement,
            C1ExactBattleSandboxItemFootprint footprint,
            C1FormalItemPresentationCatalogSnapshot presentationCatalog)
        {
            if (roster == null || !ValidateAuthoredReferences())
            {
                Clear();
                return false;
            }

            bool lit = resolvedPlacement != null && resolvedPlacement.isLit;
            Sprite resolvedArtwork = null;
            bool canonicalArtwork = roster.ordinaryInstance != null
                && CanonicalItemArtworkResolver.TryResolve(
                    roster.ordinaryInstance.canonicalDefinition,
                    roster.ordinaryInstance.rarity,
                    out resolvedArtwork,
                    out _);
            if (!canonicalArtwork
                && !C1Pool15FormalItemArtworkAndSourceAnchorBinding
                    .TryResolveArtwork(
                        presentationCatalog,
                        roster.baseItemId,
                        roster.rarityKey,
                        lit
                            ? C1FormalItemArtworkLightingState.Lit
                            : C1FormalItemArtworkLightingState.Unlit,
                        out _,
                        out resolvedArtwork,
                        out _))
            {
                Debug.LogError("[C1ExactBattleSandboxItemCardView] "
                               + UnknownArtworkDiagnostic + " item="
                               + roster.baseItemId, this);
                Clear();
                return false;
            }

            itemInstanceId = roster.itemInstanceId;
            baseItemId = roster.baseItemId;
            isPlaced = placement != null;
            if (footprint == null)
            {
                Clear();
                return false;
            }

            if (placement != null)
            {
                if (!C1ExactBattleSandboxItemRotationAdapter
                        .TryFormalDegreesToVisualQuarterTurns(
                            placement.rotation,
                            out int placedVisualQuarterTurns))
                {
                    LogInvalidFormalRotationOnce(placement.rotation);
                    Clear();
                    return false;
                }

                if (placedVisualQuarterTurns != footprint.VisualQuarterTurns)
                {
                    Clear();
                    return false;
                }
            }

            if (!ApplyFootprint(footprint))
            {
                Clear();
                return false;
            }

            artworkImage.sprite = resolvedArtwork;
            if (roster.isSpecialLightingSource)
            {
                if (!rarityContourBloomVfx.Apply(
                        resolvedArtwork,
                        ItemRarityContourBloomProfile.SpecialSourceKey))
                {
                    Clear();
                    return false;
                }
            }
            else if (!rarityContourBloomVfx.Apply(
                         resolvedArtwork,
                         roster.rarityKey))
            {
                Clear();
                return false;
            }
            identityLabel.text = roster.isSpecialLightingSource
                ? I031InventoryPlacementContract.SpecialIdentityId
                : roster.baseItemId + "@" + roster.rarityKey;
            categoryLabel.text = roster.isSpecialLightingSource
                ? "LIGHTING_SOURCE"
                : roster.rarityKey;
            shapeLabel.text = footprint.NormalizedCells.Count + " CELL";
            lightingOverlay.color = resolvedPlacement == null
                ? new Color(0.55f, 0.55f, 0.55f, 0.24f)
                : resolvedPlacement.isLit
                    ? new Color(0.35f, 1f, 0.55f, 0.28f)
                    : new Color(0.22f, 0.25f, 0.32f, 0.34f);
            lightingOverlay.gameObject.SetActive(true);
            selectionOverlay.gameObject.SetActive(false);
            dragFeedbackOverlay.gameObject.SetActive(false);
            rotateFeedbackLabel.gameObject.SetActive(false);
            canvasGroup.alpha = resolvedPlacement != null && !resolvedPlacement.isLit
                ? 0.72f
                : 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            inputSurface.raycastTarget = true;
            gameObject.SetActive(true);
            return true;
        }

        internal bool ApplyFootprint(
            C1ExactBattleSandboxItemFootprint footprint)
        {
            if (footprint == null || !ValidateAuthoredReferences()
                                  || !CaptureAuthoredLayout())
                return false;

            rotation = footprint.VisualQuarterTurns;
            currentFootprint = footprint;
            cardRect.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                footprint.TrayLocalSize.x);
            cardRect.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                footprint.TrayLocalSize.y);
            currentFootprintSize = cardRect.rect.size;

            ApplyNormalizedRect(selectionOverlay.rectTransform, selectionBounds);
            ApplyNormalizedRect(lightingOverlay.rectTransform, lightingBounds);
            ApplyNormalizedRect(dragFeedbackOverlay.rectTransform, dragBounds);
            ApplyArtworkRect(artworkImage.rectTransform, artworkBounds, rotation);
            StretchToParent(inputSurface.rectTransform);
            sourceFootprintLayer.gameObject.SetActive(false);
            foreach (Image image in sourceFootprintCells)
                image.raycastTarget = false;
            return ValidateLiveFootprintAlignment();
        }

        internal bool ValidateLiveFootprintAlignment()
        {
            if (!authoredLayoutCaptured || cardRect == null
                                        || currentFootprintSize.x <= 0f
                                        || currentFootprintSize.y <= 0f)
                return false;
            Bounds artwork = RectTransformUtility.CalculateRelativeRectTransformBounds(
                cardRect,
                artworkImage.rectTransform);
            foreach (Image overlay in new[]
                     {
                         selectionOverlay,
                         lightingOverlay,
                         dragFeedbackOverlay
                     })
            {
                Bounds overlayBounds = RectTransformUtility
                    .CalculateRelativeRectTransformBounds(
                        cardRect,
                        overlay.rectTransform);
                if (!Approximately(artwork.center, overlayBounds.center)
                    || !Approximately(artwork.size, overlayBounds.size))
                    return false;
            }

            Bounds input = RectTransformUtility.CalculateRelativeRectTransformBounds(
                cardRect,
                inputSurface.rectTransform);
            return Approximately(
                       new Vector2(input.center.x, input.center.y),
                       cardRect.rect.center)
                   && Approximately(
                       new Vector2(input.size.x, input.size.y),
                       cardRect.rect.size)
                   && artworkImage.preserveAspect
                   && !sourceFootprintLayer.gameObject.activeSelf
                   && SourceFootprintRaycastCount == 0;
        }

        internal void Clear()
        {
            ResetGesture();
            itemInstanceId = string.Empty;
            baseItemId = string.Empty;
            isPlaced = false;
            rotation = 0;
            currentFootprint = null;
            currentFootprintSize = Vector2.zero;
            if (rarityContourBloomVfx != null)
                rarityContourBloomVfx.Clear();
            if (artworkImage != null)
            {
                artworkImage.sprite = null;
                artworkImage.color = Color.white;
            }
            if (identityLabel != null) identityLabel.text = string.Empty;
            if (categoryLabel != null) categoryLabel.text = string.Empty;
            if (shapeLabel != null) shapeLabel.text = string.Empty;
            if (inputSurface != null) inputSurface.raycastTarget = false;
            if (selectionOverlay != null) selectionOverlay.gameObject.SetActive(false);
            if (lightingOverlay != null)
            {
                lightingOverlay.color = Color.clear;
                lightingOverlay.gameObject.SetActive(false);
            }
            if (dragFeedbackOverlay != null)
            {
                dragFeedbackOverlay.color = Color.clear;
                dragFeedbackOverlay.gameObject.SetActive(false);
            }
            if (rotateFeedbackLabel != null)
            {
                rotateFeedbackLabel.text = string.Empty;
                rotateFeedbackLabel.gameObject.SetActive(false);
            }
            if (sourceFootprintLayer != null)
                sourceFootprintLayer.gameObject.SetActive(false);
            foreach (Image image in sourceFootprintCells ?? Array.Empty<Image>())
            {
                if (image == null) continue;
                image.raycastTarget = false;
                image.color = Color.clear;
                image.gameObject.SetActive(false);
            }
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }

            gameObject.SetActive(false);
        }

        internal void Recycle()
        {
            Clear();
            BindPresenter(null);
        }

        internal void SetSelected(bool selected)
        {
            if (selectionOverlay != null)
                selectionOverlay.gameObject.SetActive(selected && HasBoundItem);
        }

        internal void SetDragging(bool dragging)
        {
            if (dragFeedbackOverlay != null)
                dragFeedbackOverlay.gameObject.SetActive(dragging && HasBoundItem);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = dragging ? 0.58f : 1f;
                canvasGroup.blocksRaycasts = !dragging;
            }
        }

        internal void SetRotateFeedback(int candidateRotation, bool visible)
        {
            if (rotateFeedbackLabel == null) return;
            rotateFeedbackLabel.text = "R "
                                       + (C1ExactBattleSandboxItemRotationAdapter
                                              .NormalizeVisualQuarterTurns(
                                                  candidateRotation) * 90)
                                       + "\u00B0";
            rotateFeedbackLabel.gameObject.SetActive(visible && HasBoundItem);
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            dragRoute = DragRoute.None;
            forwardedScrollBegin = false;
            if (HasScrollableVerticalRange())
            {
                ExecuteEvents.Execute(
                    authoredScrollRect.gameObject,
                    eventData,
                    ExecuteEvents.initializePotentialDrag);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            ClearPressedRawGrabbedCell();
            bool interactionEnabled = ItemInteractionEnabled;
            bool validOccupiedCell = interactionEnabled
                                     && TryResolveRawGrabbedCell(
                                         eventData,
                                         out pressedRawGrabbedCell);
            hasPressedRawGrabbedCell = validOccupiedCell;
            pointerIsDown = validOccupiedCell;
            pointerDownTime = Time.unscaledTime;
            suppressClick = !interactionEnabled || !HasBoundItem;
            dragRoute = DragRoute.None;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            pointerIsDown = false;
            if (!ItemInteractionEnabled)
            {
                suppressClick = true;
                if (dragRoute != DragRoute.Scrolling) dragRoute = DragRoute.None;
                return;
            }
            if (dragRoute == DragRoute.Pending) ResetGesture(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!ItemInteractionEnabled)
            {
                suppressClick = true;
                return;
            }
            if (suppressClick)
            {
                suppressClick = false;
                ClearPressedRawGrabbedCell();
                return;
            }

            if (HasBoundItem)
                presenter?.SelectCard(this);
            ClearPressedRawGrabbedCell();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!HasBoundItem || eventData == null) return;
            suppressClick = true;
            dragRoute = DragRoute.Pending;
            ResolvePendingRoute(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!HasBoundItem || eventData == null) return;
            if (dragRoute == DragRoute.Pending) ResolvePendingRoute(eventData);
            if (dragRoute == DragRoute.Scrolling)
            {
                if (ShouldBeginItemDrag(eventData))
                {
                    EndScroll(eventData);
                    if (ItemInteractionEnabled)
                    {
                        BeginItemDrag(eventData);
                        presenter?.UpdateCardDrag(this, eventData);
                    }
                    else
                    {
                        ResetGesture(false);
                        suppressClick = true;
                    }
                    return;
                }

                ExecuteEvents.Execute(
                    authoredScrollRect.gameObject,
                    eventData,
                    ExecuteEvents.dragHandler);
                return;
            }

            if (dragRoute == DragRoute.Item && ItemInteractionEnabled)
                presenter?.UpdateCardDrag(this, eventData);
            else if (dragRoute == DragRoute.Item)
                CancelInteractionState();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (dragRoute == DragRoute.Scrolling)
                EndScroll(eventData);
            else if (dragRoute == DragRoute.Item && ItemInteractionEnabled)
                presenter?.EndCardDrag(this, eventData);
            SetDragging(false);
            ResetGesture(false);
        }

        private void ResolvePendingRoute(PointerEventData eventData)
        {
            if (!ItemInteractionEnabled)
            {
                TryBeginVerticalScroll(eventData);
                return;
            }
            if (!HasScrollableVerticalRange() || ShouldBeginItemDrag(eventData))
            {
                BeginItemDrag(eventData);
                return;
            }

            TryBeginVerticalScroll(eventData);
        }

        private bool ShouldBeginItemDrag(PointerEventData eventData)
        {
            if (!ItemInteractionEnabled || !hasPressedRawGrabbedCell)
                return false;
            if (!HasScrollableVerticalRange()) return true;
            if (pointerIsDown && Time.unscaledTime - pointerDownTime >= LongPressSeconds)
                return true;
            if (PointerOutsideViewport(eventData, ViewportExitMarginPixels)) return true;
            Vector2 delta = eventData.position - eventData.pressPosition;
            return Mathf.Abs(delta.x) >= HorizontalDragThresholdPixels
                   && Mathf.Abs(delta.x) > Mathf.Abs(delta.y);
        }

        private void BeginItemDrag(PointerEventData eventData)
        {
            if (!ItemInteractionEnabled)
            {
                CancelInteractionState();
                return;
            }
            if (!hasPressedRawGrabbedCell)
            {
                CancelInteractionState();
                return;
            }
            dragRoute = DragRoute.Item;
            SetDragging(true);
            presenter?.BeginCardDrag(this, eventData);
        }

        private void EndScroll(PointerEventData eventData)
        {
            if (!forwardedScrollBegin
                || authoredScrollRect == null
                || eventData == null)
                return;
            ExecuteEvents.Execute(
                authoredScrollRect.gameObject,
                eventData,
                ExecuteEvents.endDragHandler);
            authoredScrollRect.StopMovement();
            forwardedScrollBegin = false;
        }

        private void TryBeginVerticalScroll(PointerEventData eventData)
        {
            if (!HasScrollableVerticalRange() || eventData == null) return;
            Vector2 delta = eventData.position - eventData.pressPosition;
            if (Mathf.Abs(delta.y) < Mathf.Abs(delta.x) * ScrollAxisBias)
                return;
            dragRoute = DragRoute.Scrolling;
            ExecuteEvents.Execute(
                authoredScrollRect.gameObject,
                eventData,
                ExecuteEvents.beginDragHandler);
            forwardedScrollBegin = true;
        }

        private bool PointerOutsideViewport(
            PointerEventData eventData,
            float margin)
        {
            if (!HasScrollableVerticalRange() || eventData == null) return false;
            RectTransform viewport = authoredScrollRect.viewport
                                     ?? authoredScrollRect.content?.parent
                                         as RectTransform;
            if (viewport == null || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    viewport,
                    eventData.position,
                    eventData.pressEventCamera ?? eventData.enterEventCamera,
                    out Vector2 local))
                return false;
            Rect rect = viewport.rect;
            rect.xMin -= margin;
            rect.xMax += margin;
            rect.yMin -= margin;
            rect.yMax += margin;
            return !rect.Contains(local);
        }

        private bool HasScrollableVerticalRange()
        {
            if (authoredScrollRect == null
                || !authoredScrollRect.isActiveAndEnabled
                || !authoredScrollRect.vertical
                || authoredScrollRect.content == null)
                return false;

            RectTransform content = authoredScrollRect.content;
            RectTransform viewport = authoredScrollRect.viewport
                                     ?? content.parent as RectTransform;
            if (viewport == null
                || !viewport.gameObject.activeInHierarchy
                || !content.gameObject.activeInHierarchy)
                return false;

            Bounds contentBounds = RectTransformUtility
                .CalculateRelativeRectTransformBounds(viewport, content);
            return HasScrollableVerticalRange(viewport.rect, contentBounds);
        }

        internal static bool HasScrollableVerticalRange(
            Rect viewportRect,
            Bounds contentBounds)
        {
            float viewportHeight = Mathf.Abs(viewportRect.height);
            float contentHeight = Mathf.Abs(contentBounds.size.y);
            if (float.IsNaN(viewportHeight)
                || float.IsInfinity(viewportHeight)
                || float.IsNaN(contentHeight)
                || float.IsInfinity(contentHeight)
                || viewportHeight <= VerticalScrollRangeLayoutEpsilon)
                return false;
            return contentHeight
                   > viewportHeight + VerticalScrollRangeLayoutEpsilon;
        }

        internal bool TryGetPressedRawGrabbedCell(
            out Vector2Int rawGrabbedCell)
        {
            if (hasPressedRawGrabbedCell)
            {
                rawGrabbedCell = pressedRawGrabbedCell;
                return true;
            }

            rawGrabbedCell = default;
            return false;
        }

        private bool TryResolveRawGrabbedCell(
            PointerEventData eventData,
            out Vector2Int rawGrabbedCell)
        {
            rawGrabbedCell = default;
            if (eventData == null || currentFootprint == null
                                  || cardRect == null
                                  || cardRect.rect.width <= 0f
                                  || cardRect.rect.height <= 0f
                                  || !RectTransformUtility
                                      .ScreenPointToLocalPointInRectangle(
                                          cardRect,
                                          eventData.position,
                                          eventData.pressEventCamera
                                          ?? eventData.enterEventCamera,
                                          out Vector2 localPoint))
                return false;
            Rect rect = cardRect.rect;
            if (!rect.Contains(localPoint)) return false;
            int normalizedX = Mathf.FloorToInt(
                (localPoint.x - rect.xMin) / rect.width
                * currentFootprint.WidthInCells);
            int normalizedY = Mathf.FloorToInt(
                (localPoint.y - rect.yMin) / rect.height
                * currentFootprint.HeightInCells);
            if (normalizedX < 0
                || normalizedX >= currentFootprint.WidthInCells
                || normalizedY < 0
                || normalizedY >= currentFootprint.HeightInCells)
                return false;
            return currentFootprint.TryResolveRawCell(
                new Vector2Int(normalizedX, normalizedY),
                out rawGrabbedCell);
        }

        private bool ItemInteractionEnabled =>
            presenter != null && presenter.IsItemInteractionEnabled;

        private void ResetGesture(bool clearClick = true)
        {
            pointerIsDown = false;
            pointerDownTime = 0f;
            dragRoute = DragRoute.None;
            forwardedScrollBegin = false;
            ClearPressedRawGrabbedCell();
            if (clearClick) suppressClick = false;
        }

        private void ClearPressedRawGrabbedCell()
        {
            pressedRawGrabbedCell = default;
            hasPressedRawGrabbedCell = false;
        }

        private bool CaptureAuthoredLayout()
        {
            if (authoredLayoutCaptured) return true;
            if (cardRect == null || cardRect.rect.width <= 0f
                                 || cardRect.rect.height <= 0f)
                return false;
            artworkBounds = CaptureNormalizedBounds(artworkImage.rectTransform);
            selectionBounds = CaptureNormalizedBounds(selectionOverlay.rectTransform);
            lightingBounds = CaptureNormalizedBounds(lightingOverlay.rectTransform);
            dragBounds = CaptureNormalizedBounds(dragFeedbackOverlay.rectTransform);
            authoredLayoutCaptured = artworkBounds.Size.x > 0f
                                     && artworkBounds.Size.y > 0f
                                     && selectionBounds.Size.x > 0f
                                     && selectionBounds.Size.y > 0f
                                     && lightingBounds.Size.x > 0f
                                     && lightingBounds.Size.y > 0f
                                     && dragBounds.Size.x > 0f
                                     && dragBounds.Size.y > 0f;
            return authoredLayoutCaptured;
        }

        private NormalizedBounds CaptureNormalizedBounds(RectTransform child)
        {
            Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(
                cardRect,
                child);
            Rect root = cardRect.rect;
            Vector2 min = new(
                (bounds.min.x - root.xMin) / root.width,
                (bounds.min.y - root.yMin) / root.height);
            Vector2 max = new(
                (bounds.max.x - root.xMin) / root.width,
                (bounds.max.y - root.yMin) / root.height);
            return new NormalizedBounds(min, max);
        }

        private static void ApplyNormalizedRect(
            RectTransform rect,
            NormalizedBounds bounds)
        {
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
            rect.anchorMin = bounds.Min;
            rect.anchorMax = bounds.Max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private void ApplyArtworkRect(
            RectTransform rect,
            NormalizedBounds bounds,
            int visualQuarterTurns)
        {
            bool swapsAxes = (visualQuarterTurns & 1) != 0;
            Vector2 displayedSize = Vector2.Scale(cardRect.rect.size, bounds.Size);
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
            rect.anchorMin = bounds.Center;
            rect.anchorMax = bounds.Center;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = swapsAxes
                ? new Vector2(displayedSize.y, displayedSize.x)
                : displayedSize;
            rect.localEulerAngles = new Vector3(
                0f,
                0f,
                -90f * visualQuarterTurns);
        }

        private static void StretchToParent(RectTransform rect)
        {
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static bool Approximately(Vector3 left, Vector3 right)
        {
            return Vector3.SqrMagnitude(left - right) <= 0.0001f;
        }

        private static bool Approximately(Vector2 left, Vector2 right)
        {
            return Vector2.SqrMagnitude(left - right) <= 0.0001f;
        }

        private void LogInvalidFormalRotationOnce(int formalDegrees)
        {
            if (invalidFormalRotationLogged) return;
            invalidFormalRotationLogged = true;
            Debug.LogError("[C1ExactBattleSandboxItemCardView] "
                           + C1ExactBattleSandboxItemRotationAdapter
                               .InvalidFormalDegreesDiagnostic
                           + " degrees=" + formalDegrees, this);
        }

        private void OnDisable()
        {
            CancelInteractionState();
            if (rarityContourBloomVfx != null)
                rarityContourBloomVfx.Clear();
        }
    }
}
