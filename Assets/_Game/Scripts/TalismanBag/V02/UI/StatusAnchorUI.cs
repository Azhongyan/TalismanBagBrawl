using System.Collections.Generic;
using TalismanBag.V02.Status;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.V02.UI
{
    public sealed class StatusAnchorUI : MonoBehaviour
    {
        [SerializeField] private StatusEffectController controller;
        [SerializeField] private RectTransform iconRoot;
        [SerializeField] private StatusTooltipPanel tooltipPanel;
        [SerializeField] private int maxVisibleIcons = 6;
        [SerializeField] private Vector2 iconSize = new(42f, 42f);
        [SerializeField] private float spacing = 6f;
        [SerializeField] private TextAnchor iconAlignment = TextAnchor.MiddleLeft;
        [SerializeField] private bool filterByPolarity;
        [SerializeField] private StatusPolarity polarityFilter = StatusPolarity.Debuff;

        private readonly List<StatusIconView> iconViews = new();
        private Font font;

        public StatusEffectController Controller => controller;
        public RectTransform IconRoot => iconRoot;

        private void Awake()
        {
            EnsureIconRoot();
            EnsureIconCount(Mathf.Max(1, maxVisibleIcons));
            Bind(controller);
        }

        private void OnDestroy()
        {
            if (controller != null)
            {
                controller.StatusesChanged -= Refresh;
            }
        }

        private void Update()
        {
            foreach (StatusIconView view in iconViews)
            {
                if (view != null && view.gameObject.activeSelf)
                {
                    view.Refresh();
                }
            }
        }

        public void Bind(StatusEffectController nextController)
        {
            if (controller == nextController)
            {
                Refresh();
                return;
            }

            if (controller != null)
            {
                controller.StatusesChanged -= Refresh;
            }

            controller = nextController;
            if (controller != null)
            {
                controller.StatusesChanged += Refresh;
            }

            Refresh();
        }

        public void BindTooltip(StatusTooltipPanel tooltip)
        {
            tooltipPanel = tooltip;
            foreach (StatusIconView view in iconViews)
            {
                if (view != null)
                {
                    view.SetTooltipPanel(tooltipPanel);
                }
            }
        }

        public void SetIconAlignment(TextAnchor alignment)
        {
            iconAlignment = alignment;
            ApplyLayoutSettings();
        }

        public void SetPolarityFilter(bool enabled, StatusPolarity polarity)
        {
            filterByPolarity = enabled;
            polarityFilter = polarity;
            Refresh();
        }

        public Vector2 GetFloatingTextPosition(RectTransform feedbackRoot)
        {
            RectTransform rect = iconRoot != null ? iconRoot : GetComponent<RectTransform>();
            if (rect == null || feedbackRoot == null)
            {
                return Vector2.zero;
            }

            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, rect.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(feedbackRoot, screenPoint, null, out Vector2 localPoint);
            return localPoint;
        }

        public void Refresh()
        {
            EnsureIconRoot();
            if (controller == null)
            {
                HideAll();
                return;
            }

            List<StatusEffectRuntime> statuses = GetFilteredStatuses();
            int max = Mathf.Max(1, maxVisibleIcons);
            int normalCount = statuses.Count > max ? max - 1 : statuses.Count;
            EnsureIconCount(max);

            for (int i = 0; i < iconViews.Count; i++)
            {
                StatusIconView view = iconViews[i];
                if (view == null)
                {
                    continue;
                }

                if (i < normalCount)
                {
                    view.gameObject.SetActive(true);
                    view.Bind(statuses[i]);
                }
                else if (statuses.Count > max && i == normalCount)
                {
                    view.gameObject.SetActive(true);
                    view.BindOverflow(statuses.Count - normalCount);
                }
                else
                {
                    view.Clear();
                }
            }
        }

        private void HideAll()
        {
            foreach (StatusIconView view in iconViews)
            {
                view?.Clear();
            }
        }

        private List<StatusEffectRuntime> GetFilteredStatuses()
        {
            List<StatusEffectRuntime> statuses = controller.GetVisibleStatuses();
            if (!filterByPolarity)
            {
                return statuses;
            }

            statuses.RemoveAll(status => status?.definition == null || status.definition.polarity != polarityFilter);
            return statuses;
        }

        private void EnsureIconRoot()
        {
            if (iconRoot == null)
            {
                RectTransform rect = GetComponent<RectTransform>();
                if (rect == null)
                {
                    rect = gameObject.AddComponent<RectTransform>();
                }

                iconRoot = rect;
            }

            ApplyLayoutSettings();
        }

        private void ApplyLayoutSettings()
        {
            if (iconRoot == null)
            {
                return;
            }

            if (iconRoot.GetComponent<HorizontalLayoutGroup>() == null)
            {
                iconRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
            }

            HorizontalLayoutGroup layout = iconRoot.GetComponent<HorizontalLayoutGroup>();
            layout.childAlignment = iconAlignment;
            layout.spacing = spacing;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
        }

        private void EnsureIconCount(int count)
        {
            while (iconViews.Count < count)
            {
                iconViews.Add(CreateIconView(iconViews.Count));
            }
        }

        private StatusIconView CreateIconView(int index)
        {
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            GameObject iconObject = new($"StatusIcon_{index + 1}", typeof(RectTransform), typeof(LayoutElement), typeof(Image), typeof(StatusIconView));
            iconObject.transform.SetParent(iconRoot, false);

            RectTransform rect = iconObject.GetComponent<RectTransform>();
            rect.sizeDelta = iconSize;

            LayoutElement layoutElement = iconObject.GetComponent<LayoutElement>();
            layoutElement.preferredWidth = iconSize.x;
            layoutElement.preferredHeight = iconSize.y;

            Image background = iconObject.GetComponent<Image>();
            background.color = new Color(0.18f, 0.2f, 0.18f, 0.92f);

            Image iconImage = CreateImage("Icon", iconObject.transform);
            RectTransform imageRect = iconImage.rectTransform;
            imageRect.anchorMin = Vector2.zero;
            imageRect.anchorMax = Vector2.one;
            imageRect.offsetMin = new Vector2(6f, 6f);
            imageRect.offsetMax = new Vector2(-6f, -6f);
            iconImage.enabled = false;

            Text glyph = CreateText("Glyph", iconObject.transform, 22, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            Stretch(glyph.rectTransform, Vector2.zero);

            Text stack = CreateText("Stack", iconObject.transform, 13, FontStyle.Bold, Color.white, TextAnchor.UpperRight);
            Stretch(stack.rectTransform, new Vector2(-3f, -1f));

            Text countdown = CreateText("Countdown", iconObject.transform, 12, FontStyle.Bold, Color.white, TextAnchor.LowerCenter);
            RectTransform countdownRect = countdown.rectTransform;
            countdownRect.anchorMin = new Vector2(0f, 0f);
            countdownRect.anchorMax = new Vector2(1f, 0f);
            countdownRect.pivot = new Vector2(0.5f, 0f);
            countdownRect.anchoredPosition = new Vector2(0f, 2f);
            countdownRect.sizeDelta = new Vector2(-4f, 15f);

            StatusIconView view = iconObject.GetComponent<StatusIconView>();
            view.Setup(background, iconImage, glyph, stack, countdown, tooltipPanel);
            view.Clear();
            return view;
        }

        private Image CreateImage(string name, Transform parent)
        {
            GameObject imageObject = new(name, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            Image image = imageObject.GetComponent<Image>();
            image.raycastTarget = false;
            return image;
        }

        private Text CreateText(string name, Transform parent, int size, FontStyle style, Color color, TextAnchor anchor)
        {
            GameObject textObject = new(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = anchor;
            text.raycastTarget = false;
            return text;
        }

        private static void Stretch(RectTransform rect, Vector2 offset)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offset;
            rect.offsetMax = -offset;
        }
    }
}
