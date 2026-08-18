using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TalismanBag.Items.CampaignBaseline
{
    [DisallowMultipleComponent]
    public sealed class C1FormalItemCardView : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private RectTransform cardRect;
        [SerializeField] private Image artworkImage;
        [SerializeField] private Image litIndicatorImage;
        [SerializeField] private Text identityLabel;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private AspectRatioFitter artworkAspectRatio;
        [SerializeField] private Sprite i001WhiteArtwork;
        [SerializeField] private Sprite i002WhiteArtwork;
        [SerializeField] private Sprite i031UnlitArtwork;
        [SerializeField] private Sprite i031LitArtwork;

        private C1FormalItemArrangementPresenter presenter;
        private string itemInstanceId = string.Empty;
        private string baseItemId = string.Empty;
        private bool isPlaced;
        private int rotation;
        private Vector2 anchoredPositionBeforeDrag;
        private bool dragging;

        public string ItemInstanceId => itemInstanceId;
        public string BaseItemId => baseItemId;
        public bool IsPlaced => isPlaced;
        public int Rotation => rotation;

        public void AssignForEditor(
            RectTransform configuredCardRect,
            Image configuredArtwork,
            Image configuredLitIndicator,
            Text configuredIdentityLabel,
            CanvasGroup configuredCanvasGroup,
            AspectRatioFitter configuredAspectRatio,
            Sprite configuredI001WhiteArtwork,
            Sprite configuredI002WhiteArtwork,
            Sprite configuredI031UnlitArtwork,
            Sprite configuredI031LitArtwork)
        {
            cardRect = configuredCardRect;
            artworkImage = configuredArtwork;
            litIndicatorImage = configuredLitIndicator;
            identityLabel = configuredIdentityLabel;
            canvasGroup = configuredCanvasGroup;
            artworkAspectRatio = configuredAspectRatio;
            i001WhiteArtwork = configuredI001WhiteArtwork;
            i002WhiteArtwork = configuredI002WhiteArtwork;
            i031UnlitArtwork = configuredI031UnlitArtwork;
            i031LitArtwork = configuredI031LitArtwork;
            ApplyArtworkContract();
        }

        public bool ValidateAuthoredReferences()
        {
            return cardRect != null && artworkImage != null
                   && litIndicatorImage != null && identityLabel != null
                   && canvasGroup != null && artworkAspectRatio != null
                   && i001WhiteArtwork != null && i002WhiteArtwork != null
                   && i031UnlitArtwork != null && i031LitArtwork != null
                   && artworkImage.preserveAspect;
        }

        internal void BindPresenter(C1FormalItemArrangementPresenter configuredPresenter)
        {
            presenter = configuredPresenter;
        }

        internal void Bind(
            C1FormalItemRosterEntrySnapshot roster,
            C1FormalItemPlacementSnapshot placement,
            ItemSystemPlacementSnapshot itemSystemPlacement)
        {
            if (roster == null)
            {
                Clear();
                return;
            }

            itemInstanceId = roster.itemInstanceId;
            baseItemId = roster.baseItemId;
            isPlaced = placement != null;
            rotation = placement == null ? 0 : placement.rotation;
            bool? isLit = itemSystemPlacement == null
                ? (bool?)null
                : itemSystemPlacement.isLit;
            artworkImage.sprite = ResolveArtwork(baseItemId, isLit == true);
            identityLabel.text = roster.isSpecialLightingSource
                ? I031InventoryPlacementContract.SpecialIdentityId
                : roster.baseItemId + "@" + roster.rarityKey;
            litIndicatorImage.color = isLit.HasValue
                ? isLit.Value
                    ? new Color(0.35f, 1f, 0.55f, 1f)
                    : new Color(0.28f, 0.31f, 0.38f, 1f)
                : new Color(0.55f, 0.55f, 0.55f, 0.75f);
            canvasGroup.alpha = isLit == false ? 0.72f : 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            ApplyArtworkContract();
            gameObject.SetActive(true);
        }

        internal void Clear()
        {
            dragging = false;
            itemInstanceId = string.Empty;
            baseItemId = string.Empty;
            isPlaced = false;
            rotation = 0;
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }

            gameObject.SetActive(false);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (presenter == null || cardRect == null || itemInstanceId.Length == 0)
            {
                return;
            }

            anchoredPositionBeforeDrag = cardRect.anchoredPosition;
            dragging = true;
            canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragging && cardRect != null && eventData != null)
            {
                cardRect.anchoredPosition += eventData.delta;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!dragging)
            {
                return;
            }

            dragging = false;
            canvasGroup.blocksRaycasts = true;
            bool submitted = presenter != null && eventData != null
                             && presenter.TryHandleCardDrop(
                                 this,
                                 eventData.position,
                                 eventData.pressEventCamera);
            if (!submitted && cardRect != null)
            {
                cardRect.anchoredPosition = anchoredPositionBeforeDrag;
            }
        }

        private Sprite ResolveArtwork(string itemId, bool lit)
        {
            if (string.Equals(itemId, "I001", StringComparison.Ordinal))
                return i001WhiteArtwork;
            if (string.Equals(itemId, "I002", StringComparison.Ordinal))
                return i002WhiteArtwork;
            if (string.Equals(itemId, I031InventoryPlacementContract.ItemId,
                    StringComparison.Ordinal))
                return lit ? i031LitArtwork : i031UnlitArtwork;
            return null;
        }

        private void ApplyArtworkContract()
        {
            if (artworkImage != null) artworkImage.preserveAspect = true;
            if (artworkAspectRatio != null)
            {
                artworkAspectRatio.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                artworkAspectRatio.aspectRatio = 1f;
            }
        }
    }
}
