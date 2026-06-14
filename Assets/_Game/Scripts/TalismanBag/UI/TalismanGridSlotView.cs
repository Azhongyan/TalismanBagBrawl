using System.Collections;
using TalismanBag.Grid;
using TalismanBag.V02.Formation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TalismanBag.UI
{
    public sealed class TalismanGridSlotView : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Vector2Int gridPosition;
        [SerializeField] private TalismanBagGrid grid;
        [SerializeField] private Image background;
        [SerializeField] private Image border;
        [SerializeField] private Image sealedOverlay;
        [SerializeField] private GameObject enhancedBadge;
        [SerializeField] private GameObject formationEyeBadge;
        [SerializeField] private GameObject powerBadge;
        [SerializeField] private Text powerBadgeText;
        [SerializeField] private Text hoverHintText;
        [SerializeField] private RectTransform itemAnchor;

        [Header("Colors")]
        [SerializeField] private Color normalColor = new(0.16f, 0.13f, 0.1f, 1f);
        [SerializeField] private Color hoverColor = new(0.28f, 0.23f, 0.16f, 1f);
        [SerializeField] private Color comboColor = new(1f, 0.76f, 0.24f, 1f);
        [SerializeField] private Color borderNormalColor = new(0.45f, 0.35f, 0.2f, 1f);
        [SerializeField] private Color enhancedColor = new(0.35f, 0.75f, 1f, 1f);
        [SerializeField] private Color invalidColor = new(0.95f, 0.16f, 0.12f, 1f);
        [SerializeField] private Color eyeColor = new(1f, 0.82f, 0.25f, 1f);
        [SerializeField] private Color poweredColor = new(0.35f, 0.82f, 1f, 1f);
        [SerializeField] private Color weakPoweredColor = new(0.55f, 0.68f, 1f, 1f);
        [SerializeField] private Color unpoweredColor = new(0.5f, 0.28f, 0.26f, 1f);
        [SerializeField] private Color eyeBackgroundColor = new(0.3f, 0.22f, 0.08f, 1f);
        [SerializeField] private Color poweredBackgroundColor = new(0.1f, 0.2f, 0.23f, 1f);
        [SerializeField] private Color weakPoweredBackgroundColor = new(0.11f, 0.13f, 0.24f, 1f);
        [SerializeField] private Color unpoweredBackgroundColor = new(0.18f, 0.1f, 0.09f, 1f);

        private bool isHovered;
        private bool isComboHighlighted;
        private bool isEnhanced;
        private bool isSealed;
        private bool isInvalidTarget;
        private bool isFormationEye;
        private bool formationVisualsEnabled;
        private FormationPowerState formationPowerState = FormationPowerState.Unpowered;
        private Coroutine flashRoutine;
        private DraggableTalismanItemView currentItemView;

        public Vector2Int GridPosition => gridPosition;
        public TalismanBagGrid Grid => grid;
        public RectTransform ItemAnchor => itemAnchor;
        public DraggableTalismanItemView CurrentItemView => currentItemView;
        public bool CanAcceptItem => !isFormationEye;

        public void Initialize(TalismanBagGrid bagGrid, Vector2Int position)
        {
            grid = bagGrid;
            gridPosition = position;
            RefreshVisuals();
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null)
            {
                return;
            }

            DraggableTalismanItemView dragged = eventData.pointerDrag.GetComponent<DraggableTalismanItemView>();
            if (dragged != null)
            {
                bool placed = dragged.TryPlaceOnSlot(this);
                if (!placed)
                {
                    FlashInvalid();
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
            DraggableTalismanItemView dragged = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<DraggableTalismanItemView>() : null;
            isInvalidTarget = dragged != null && (!CanAcceptItem || currentItemView != null && currentItemView != dragged);
            RefreshHoverHint();
            RefreshVisuals();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            isInvalidTarget = false;
            ClearHoverHint();
            RefreshVisuals();
        }

        public void SetItemView(DraggableTalismanItemView itemView)
        {
            currentItemView = itemView;
            RefreshVisuals();
        }

        public void SetComboHighlight(bool value)
        {
            isComboHighlighted = value;
            RefreshVisuals();
        }

        public void SetEnhanced(bool value)
        {
            isEnhanced = value;
            RefreshVisuals();
        }

        public void SetSealed(bool value)
        {
            isSealed = value;
            RefreshVisuals();
        }

        public void SetFormationEye(bool value)
        {
            isFormationEye = value;
            formationVisualsEnabled = formationVisualsEnabled || value;
            RefreshVisuals();
        }

        public void SetFormationPowerState(FormationPowerState state)
        {
            formationPowerState = state;
            formationVisualsEnabled = true;
            RefreshVisuals();
        }

        public void Flash()
        {
            if (border == null)
            {
                return;
            }

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }

            flashRoutine = StartCoroutine(FlashRoutine());
        }

        public void FlashInvalid()
        {
            if (border == null)
            {
                return;
            }

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }

            flashRoutine = StartCoroutine(InvalidFlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            Color original = border.color;
            border.color = Color.white;
            yield return new WaitForSeconds(0.18f);
            border.color = original;
            RefreshVisuals();
        }

        private void RefreshVisuals()
        {
            if (background != null)
            {
                background.color = GetBackgroundColor();
            }

            if (border != null)
            {
                border.color = GetBorderColor();
            }

            if (sealedOverlay != null)
            {
                sealedOverlay.gameObject.SetActive(isSealed);
            }

            if (enhancedBadge != null)
            {
                enhancedBadge.SetActive(isEnhanced);
            }

            if (formationEyeBadge != null)
            {
                formationEyeBadge.SetActive(isFormationEye);
            }

            RefreshPowerBadge();
            SetCurrentItemAlpha();
        }

        private Color GetBackgroundColor()
        {
            if (isInvalidTarget)
            {
                return new Color(0.32f, 0.08f, 0.07f, 1f);
            }

            if (isHovered)
            {
                return hoverColor;
            }

            if (!formationVisualsEnabled)
            {
                return normalColor;
            }

            if (isFormationEye)
            {
                return eyeBackgroundColor;
            }

            return formationPowerState switch
            {
                FormationPowerState.Powered => poweredBackgroundColor,
                FormationPowerState.WeakPowered => weakPoweredBackgroundColor,
                FormationPowerState.Unpowered when currentItemView != null => unpoweredBackgroundColor,
                _ => normalColor
            };
        }

        private Color GetBorderColor()
        {
            if (isInvalidTarget)
            {
                return invalidColor;
            }

            if (isComboHighlighted)
            {
                return comboColor;
            }

            if (isFormationEye)
            {
                return eyeColor;
            }

            if (isEnhanced)
            {
                return enhancedColor;
            }

            if (!formationVisualsEnabled)
            {
                return borderNormalColor;
            }

            return formationPowerState switch
            {
                FormationPowerState.Powered => poweredColor,
                FormationPowerState.WeakPowered => weakPoweredColor,
                FormationPowerState.Unpowered when currentItemView != null => unpoweredColor,
                _ => borderNormalColor
            };
        }

        private void RefreshPowerBadge()
        {
            EnsurePowerBadge();
            if (powerBadge == null)
            {
                return;
            }

            bool showBadge = formationVisualsEnabled && !isFormationEye &&
                             (formationPowerState != FormationPowerState.Unpowered || currentItemView != null);
            powerBadge.SetActive(showBadge);
            if (!showBadge || powerBadgeText == null)
            {
                return;
            }

            powerBadgeText.text = formationPowerState switch
            {
                FormationPowerState.Powered => "\u4f9b",
                FormationPowerState.WeakPowered => "\u5f31",
                _ => "\u672a"
            };
        }

        private void EnsurePowerBadge()
        {
            if (powerBadge != null)
            {
                return;
            }

            GameObject badgeObject = new("PowerBadge_Runtime", typeof(RectTransform), typeof(Image));
            badgeObject.transform.SetParent(transform, false);
            RectTransform badgeRect = badgeObject.GetComponent<RectTransform>();
            badgeRect.anchorMin = Vector2.one;
            badgeRect.anchorMax = Vector2.one;
            badgeRect.pivot = Vector2.one;
            badgeRect.anchoredPosition = new Vector2(-8f, -8f);
            badgeRect.sizeDelta = new Vector2(34f, 34f);

            Image badgeImage = badgeObject.GetComponent<Image>();
            badgeImage.color = new Color(0.78f, 0.94f, 1f, 0.95f);
            badgeImage.raycastTarget = false;
            powerBadge = badgeObject;

            GameObject textObject = new("Text", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(badgeObject.transform, false);
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = Vector2.zero;

            powerBadgeText = textObject.GetComponent<Text>();
            powerBadgeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            powerBadgeText.fontSize = 16;
            powerBadgeText.fontStyle = FontStyle.Bold;
            powerBadgeText.alignment = TextAnchor.MiddleCenter;
            powerBadgeText.color = new Color(0.05f, 0.12f, 0.14f);
            powerBadgeText.raycastTarget = false;
            badgeObject.SetActive(false);
        }

        private void SetCurrentItemAlpha()
        {
            if (currentItemView != null && currentItemView.TryGetComponent(out CanvasGroup group))
            {
                group.alpha = isSealed || formationVisualsEnabled && formationPowerState == FormationPowerState.Unpowered ? 0.45f : 1f;
            }
        }

        private void RefreshHoverHint()
        {
            if (hoverHintText != null && formationVisualsEnabled)
            {
                hoverHintText.text = GetHoverDescription();
            }
        }

        private void ClearHoverHint()
        {
            if (hoverHintText != null)
            {
                hoverHintText.text = string.Empty;
            }
        }

        private string GetHoverDescription()
        {
            if (isFormationEye)
            {
                return "\u9635\u773c\uff1a\u4e0a\u4e0b\u5de6\u53f3\u6b63\u5e38\u4f9b\u80fd\uff0c\u659c\u89d2\u5f31\u4f9b\u80fd\u3002";
            }

            return formationPowerState switch
            {
                FormationPowerState.Powered => "\u8be5\u683c\u5df2\u88ab\u4f9b\u80fd\uff0c\u7b26\u7b93\u53ef\u4ee5\u89e6\u53d1\u3002",
                FormationPowerState.WeakPowered => "\u8be5\u683c\u5f31\u4f9b\u80fd\uff0c\u7b26\u7b93\u53ef\u89e6\u53d1\u4f46\u51b7\u5374\u53d8\u6162\u3002",
                _ => "\u8be5\u683c\u672a\u88ab\u4f9b\u80fd\uff0c\u7b26\u7b93\u6218\u6597\u4e2d\u4e0d\u4f1a\u89e6\u53d1\u3002"
            };
        }

        private IEnumerator InvalidFlashRoutine()
        {
            Color original = border.color;
            border.color = invalidColor;
            yield return new WaitForSeconds(0.15f);
            border.color = original;
            isInvalidTarget = false;
            RefreshVisuals();
        }
    }
}
