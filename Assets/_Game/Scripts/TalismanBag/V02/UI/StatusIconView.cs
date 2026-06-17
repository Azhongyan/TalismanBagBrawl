using System.Collections;
using TalismanBag.V02.Status;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TalismanBag.V02.UI
{
    public sealed class StatusIconView : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] private Image background;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image countdownFillImage;
        [SerializeField] private Text glyphText;
        [SerializeField] private Text stackText;
        [SerializeField] private Text countdownText;
        [SerializeField] private StatusTooltipPanel tooltipPanel;
        [SerializeField] private LayoutElement layoutElement;

        private StatusEffectRuntime status;
        private bool isOverflow;
        private Coroutine longPressRoutine;

        public void Setup(Image background, Image iconImage, Image countdownFillImage, Text glyphText, Text stackText, Text countdownText, StatusTooltipPanel tooltipPanel)
        {
            this.background = background;
            this.iconImage = iconImage;
            this.countdownFillImage = countdownFillImage;
            this.glyphText = glyphText;
            this.stackText = stackText;
            this.countdownText = countdownText;
            this.tooltipPanel = tooltipPanel;
            layoutElement = GetComponent<LayoutElement>();
        }

        public void SetTooltipPanel(StatusTooltipPanel tooltipPanel)
        {
            this.tooltipPanel = tooltipPanel;
        }

        public void Bind(StatusEffectRuntime status)
        {
            gameObject.SetActive(true);
            SetLayoutVisible(true);
            this.status = status;
            isOverflow = false;
            Refresh();
        }

        public void BindOverflow(int overflowCount)
        {
            gameObject.SetActive(true);
            SetLayoutVisible(true);
            status = null;
            isOverflow = true;
            SetText(glyphText, $"+{overflowCount}");
            SetText(stackText, string.Empty);
            SetText(countdownText, string.Empty);
            SetImageVisible(iconImage, false);
            SetCountdownFillVisible(false);
            if (background != null)
            {
                background.color = new Color(0.2f, 0.24f, 0.25f, 0.95f);
            }
        }

        public void Refresh()
        {
            if (isOverflow)
            {
                return;
            }

            if (status?.definition == null)
            {
                Clear();
                return;
            }

            StatusEffectDefinition definition = status.definition;
            if (background != null)
            {
                background.color = GetBackgroundColor(definition);
            }

            if (definition.icon != null && iconImage != null)
            {
                iconImage.sprite = definition.icon;
                SetImageVisible(iconImage, true);
                SetText(glyphText, string.Empty);
            }
            else
            {
                SetImageVisible(iconImage, false);
                SetText(glyphText, GetGlyph(definition));
            }

            bool showStack = definition.showStackCount && status.stackCount > 1;
            SetText(stackText, showStack ? $"x{status.stackCount}" : string.Empty);

            bool showCountdown = definition.hasDuration && definition.showCountdown && status.remainingTime > 0f;
            SetText(countdownText, showCountdown ? $"{status.remainingTime:0.0}" : string.Empty);
            UpdateCountdownFill(definition, showCountdown);
        }

        public void Clear()
        {
            gameObject.SetActive(true);
            SetLayoutVisible(false);
            status = null;
            isOverflow = false;
            SetText(glyphText, string.Empty);
            SetText(stackText, string.Empty);
            SetText(countdownText, string.Empty);
            SetImageVisible(iconImage, false);
            SetCountdownFillVisible(false);
            if (background != null)
            {
                Color color = background.color;
                color.a = 0f;
                background.color = color;
            }
        }

        private void UpdateCountdownFill(StatusEffectDefinition definition, bool showCountdown)
        {
            if (countdownFillImage == null)
            {
                return;
            }

            bool visible = showCountdown && definition != null && definition.defaultDuration > 0f;
            SetCountdownFillVisible(visible);
            if (!visible)
            {
                return;
            }

            countdownFillImage.fillAmount = Mathf.Clamp01(status.remainingTime / definition.defaultDuration);
        }

        private void SetCountdownFillVisible(bool visible)
        {
            if (countdownFillImage != null)
            {
                countdownFillImage.enabled = visible;
                if (!visible)
                {
                    countdownFillImage.fillAmount = 0f;
                }
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            ShowTooltip();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            StopLongPress();
            longPressRoutine = StartCoroutine(LongPressRoutine());
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            StopLongPress();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            StopLongPress();
        }

        private IEnumerator LongPressRoutine()
        {
            yield return new WaitForSeconds(0.45f);
            ShowTooltip();
            longPressRoutine = null;
        }

        private void ShowTooltip()
        {
            if (isOverflow || status?.definition == null)
            {
                return;
            }

            tooltipPanel?.Show(status, GetComponent<RectTransform>());
        }

        private void StopLongPress()
        {
            if (longPressRoutine == null)
            {
                return;
            }

            StopCoroutine(longPressRoutine);
            longPressRoutine = null;
        }

        private void SetLayoutVisible(bool visible)
        {
            if (layoutElement == null)
            {
                layoutElement = GetComponent<LayoutElement>();
            }

            if (layoutElement != null)
            {
                layoutElement.ignoreLayout = !visible;
            }

            if (background != null)
            {
                background.raycastTarget = visible;
            }
        }

        private static string GetGlyph(StatusEffectDefinition definition)
        {
            if (!string.IsNullOrWhiteSpace(definition.glyph))
            {
                return definition.glyph;
            }

            return string.IsNullOrWhiteSpace(definition.displayName) ? "?" : definition.displayName.Substring(0, 1);
        }

        private static Color GetBackgroundColor(StatusEffectDefinition definition)
        {
            Color color = definition.displayColor;
            color.a = 0.92f;
            return color;
        }

        private static void SetImageVisible(Image image, bool visible)
        {
            if (image != null)
            {
                image.enabled = visible;
            }
        }

        private static void SetText(Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }
    }
}
