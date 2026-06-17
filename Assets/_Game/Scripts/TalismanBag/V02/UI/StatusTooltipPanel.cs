using TalismanBag.V02.Status;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.V02.UI
{
    public sealed class StatusTooltipPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;

        private CanvasGroup panelCanvasGroup;

        private void Awake()
        {
            EnsureRuntimeView();
            Hide();
        }

        public void Show(StatusEffectRuntime status, RectTransform anchor)
        {
            if (status?.definition == null)
            {
                Hide();
                return;
            }

            EnsureRuntimeView();

            if (panel != null)
            {
                panel.SetActive(true);
                SetPanelVisible(true);
                RectTransform rect = panel.GetComponent<RectTransform>();
                if (rect != null && anchor != null)
                {
                    rect.anchoredPosition = ResolveTooltipPosition(anchor, rect.parent as RectTransform);
                }
            }

            StatusEffectDefinition definition = status.definition;
            SetText(titleText, definition.displayName);
            SetText(bodyText, BuildBody(status));
        }

        public void Hide()
        {
            if (panel != null)
            {
                panel.SetActive(true);
                SetPanelVisible(false);
            }
        }

        private void EnsureRuntimeView()
        {
            if (panel != null)
            {
                panel.SetActive(true);
                panelCanvasGroup = panel.GetComponent<CanvasGroup>();
                if (panelCanvasGroup == null)
                {
                    panelCanvasGroup = panel.AddComponent<CanvasGroup>();
                }

                return;
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            GameObject root = new("StatusTooltipPanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            root.transform.SetParent(transform, false);
            panel = root;
            panelCanvasGroup = root.GetComponent<CanvasGroup>();

            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0f, 1f);
            rootRect.sizeDelta = new Vector2(300f, 132f);

            Image background = root.GetComponent<Image>();
            background.color = new Color(0.045f, 0.052f, 0.05f, 0.96f);
            background.raycastTarget = false;

            titleText = CreateText("Title", root.transform, font, 22, FontStyle.Bold, new Color(1f, 0.9f, 0.62f));
            RectTransform titleRect = titleText.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -14f);
            titleRect.sizeDelta = new Vector2(-28f, 30f);

            bodyText = CreateText("Body", root.transform, font, 18, FontStyle.Normal, Color.white);
            bodyText.alignment = TextAnchor.UpperLeft;
            bodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
            bodyText.verticalOverflow = VerticalWrapMode.Truncate;
            RectTransform bodyRect = bodyText.rectTransform;
            bodyRect.anchorMin = new Vector2(0f, 0f);
            bodyRect.anchorMax = new Vector2(1f, 1f);
            bodyRect.pivot = new Vector2(0.5f, 0.5f);
            bodyRect.anchoredPosition = new Vector2(0f, -24f);
            bodyRect.sizeDelta = new Vector2(-28f, -58f);

            root.SetActive(true);
            SetPanelVisible(false);
        }

        private void SetPanelVisible(bool visible)
        {
            if (panelCanvasGroup == null && panel != null)
            {
                panelCanvasGroup = panel.GetComponent<CanvasGroup>();
            }

            if (panelCanvasGroup == null)
            {
                return;
            }

            panelCanvasGroup.alpha = visible ? 1f : 0f;
            panelCanvasGroup.blocksRaycasts = visible;
            panelCanvasGroup.interactable = visible;
        }

        private static Text CreateText(string name, Transform parent, Font font, int size, FontStyle style, Color color)
        {
            GameObject textObject = new(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAnchor.MiddleLeft;
            text.raycastTarget = false;
            return text;
        }

        private static string BuildBody(StatusEffectRuntime status)
        {
            StatusEffectDefinition definition = status.definition;
            string type = definition.polarity switch
            {
                StatusPolarity.Buff => "Buff",
                StatusPolarity.Debuff => "Debuff",
                StatusPolarity.Intent => "Intent",
                StatusPolarity.Passive => "Passive",
                _ => "Neutral"
            };
            string stack = definition.showStackCount ? $"层数：{status.stackCount}" : "层数：-";
            string source = string.IsNullOrWhiteSpace(status.sourceId) ? "来源：-" : $"来源：{status.sourceId}";
            if (IsDotTickStatus(definition))
            {
                string nextTick = $"下一跳：{Mathf.Clamp(status.remainingTime, 0f, 1f):0.0}秒";
                string damage = $"预计每秒伤害：{Mathf.Max(0, status.stackCount)}";
                return $"类型：{type}\n{stack}\n{nextTick}\n{damage}\n{source}\n{definition.description}";
            }

            string remaining = definition.hasDuration && definition.showCountdown ? $"剩余：{Mathf.Max(0f, status.remainingTime):0.0}秒" : "剩余：常驻";
            return $"类型：{type}\n{stack}\n{remaining}\n{source}\n{definition.description}";
        }

        private static bool IsDotTickStatus(StatusEffectDefinition definition)
        {
            string id = definition != null ? definition.statusId : string.Empty;
            return id == StatusEffectIds.Poison || id == StatusEffectIds.Burn;
        }

        private static Vector2 ResolveTooltipPosition(RectTransform anchor, RectTransform tooltipParent)
        {
            Vector2 position = anchor.anchoredPosition;
            if (tooltipParent != null)
            {
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, anchor.position);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(tooltipParent, screenPoint, null, out position);
            }

            position += new Vector2(40f, 56f);
            position.x = Mathf.Clamp(position.x, -460f, 320f);
            position.y = Mathf.Clamp(position.y, -760f, 760f);
            return position;
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
