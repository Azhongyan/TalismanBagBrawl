using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TalismanBag.BuildSandbox
{
    public sealed class BuildItemPreviewCardView : MonoBehaviour,
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

        private BuildGridInteractionPreviewController controller;
        private readonly List<Image> layoutCellImages = new();
        private Color normalColor = new(0.27f, 0.22f, 0.15f, 1f);
        private string itemId = string.Empty;
        private string category = string.Empty;
        private bool usesLayoutCellVisuals;

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
            EnsureCardOverlayCanvas();

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
            foreach (Image image in cellImages ?? Array.Empty<Image>())
            {
                if (image == null)
                {
                    continue;
                }

                layoutCellImages.Add(image);
                image.raycastTarget = true;
            }

            usesLayoutCellVisuals = layoutCellImages.Count > 0;
            if (backgroundImage != null)
            {
                backgroundImage.raycastTarget = !usesLayoutCellVisuals;
            }

            SetNormalVisual();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            controller?.BeginDrag(this, eventData);
            SetDraggingVisual();
        }

        public void OnDrag(PointerEventData eventData)
        {
            controller?.UpdateDrag(this, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            controller?.EndDrag(this, eventData);
            SetNormalVisual();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            controller?.SelectItem(this);
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

        private void EnsureCardOverlayCanvas()
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }

            canvas.overrideSorting = true;
            canvas.sortingOrder = 30;

            if (GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        private void ApplyBodyColor(Color color)
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = usesLayoutCellVisuals ? Color.clear : color;
            }

            foreach (Image image in layoutCellImages)
            {
                if (image != null)
                {
                    image.color = color;
                }
            }
        }

        private void Reset()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            backgroundImage = GetComponent<Image>();
        }
    }
}
