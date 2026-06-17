using System;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.V02.UI
{
    public sealed class V02FixedRewardPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text iconPlaceholderText;
        [SerializeField] private Text itemNameText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Button confirmButton;

        private Action confirmCallback;
        private bool buttonBound;

        private void Awake()
        {
            BindConfirmButton();
            Hide();
        }

        private void OnDestroy()
        {
            if (buttonBound && confirmButton != null)
            {
                confirmButton.onClick.RemoveListener(Confirm);
            }
        }

        public void Show(string title, string itemName, string description, Action onConfirm)
        {
            BindConfirmButton();
            confirmCallback = onConfirm;

            if (panel != null)
            {
                panel.SetActive(true);
                panel.transform.SetAsLastSibling();
            }

            SetText(titleText, string.IsNullOrWhiteSpace(title) ? "获得新道具" : title);
            SetText(iconPlaceholderText, "图标");
            SetText(itemNameText, itemName);
            SetText(descriptionText, description);
        }

        public void Hide()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        public static V02FixedRewardPanel CreateRuntime(Transform parent)
        {
            if (parent == null)
            {
                return null;
            }

            GameObject root = new("V02FixedRewardPanel_Runtime", typeof(RectTransform), typeof(Image), typeof(Outline));
            root.transform.SetParent(parent, false);
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = Vector2.zero;
            rootRect.sizeDelta = new Vector2(850f, 780f);

            Image image = root.GetComponent<Image>();
            image.color = new Color(0.05f, 0.058f, 0.052f, 0.98f);

            Outline outline = root.GetComponent<Outline>();
            outline.effectColor = new Color(0.62f, 0.82f, 1f, 0.95f);
            outline.effectDistance = new Vector2(4f, -4f);

            V02FixedRewardPanel panelComponent = root.AddComponent<V02FixedRewardPanel>();
            panelComponent.panel = root;
            panelComponent.titleText = CreateText("FixedRewardTitle", root.transform, "获得新道具", 42, FontStyle.Bold, new Color(0.82f, 0.94f, 1f), new Vector2(0f, -62f), new Vector2(720f, 66f), TextAnchor.MiddleCenter);

            GameObject iconBox = new("IconPlaceholder", typeof(RectTransform), typeof(Image), typeof(Outline));
            iconBox.transform.SetParent(root.transform, false);
            RectTransform iconRect = iconBox.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 1f);
            iconRect.anchorMax = new Vector2(0.5f, 1f);
            iconRect.pivot = new Vector2(0.5f, 1f);
            iconRect.anchoredPosition = new Vector2(0f, -155f);
            iconRect.sizeDelta = new Vector2(160f, 160f);
            iconBox.GetComponent<Image>().color = new Color(0.13f, 0.16f, 0.15f);
            iconBox.GetComponent<Outline>().effectColor = new Color(0.5f, 0.62f, 0.68f);

            panelComponent.iconPlaceholderText = CreateText("IconText", iconBox.transform, "图标", 26, FontStyle.Bold, new Color(0.76f, 0.84f, 0.88f), Vector2.zero, new Vector2(160f, 160f), TextAnchor.MiddleCenter);
            RectTransform iconTextRect = panelComponent.iconPlaceholderText.GetComponent<RectTransform>();
            iconTextRect.anchorMin = Vector2.zero;
            iconTextRect.anchorMax = Vector2.one;
            iconTextRect.pivot = new Vector2(0.5f, 0.5f);
            iconTextRect.anchoredPosition = Vector2.zero;
            iconTextRect.sizeDelta = Vector2.zero;

            panelComponent.itemNameText = CreateText("FixedRewardItemName", root.transform, string.Empty, 38, FontStyle.Bold, Color.white, new Vector2(0f, -342f), new Vector2(720f, 58f), TextAnchor.MiddleCenter);
            panelComponent.descriptionText = CreateText("FixedRewardDescription", root.transform, string.Empty, 27, FontStyle.Normal, new Color(0.9f, 0.96f, 0.92f), new Vector2(0f, -420f), new Vector2(720f, 160f), TextAnchor.UpperLeft);
            panelComponent.confirmButton = CreateButton("FixedRewardConfirmButton", root.transform, "确认", new Vector2(0f, 82f), new Vector2(480f, 88f));
            panelComponent.BindConfirmButton();
            panelComponent.Hide();
            return panelComponent;
        }

        private void Confirm()
        {
            Action callback = confirmCallback;
            confirmCallback = null;
            Hide();
            callback?.Invoke();
        }

        private void BindConfirmButton()
        {
            if (buttonBound || confirmButton == null)
            {
                return;
            }

            confirmButton.onClick.AddListener(Confirm);
            buttonBound = true;
        }

        private static Text CreateText(string name, Transform parent, string value, int fontSize, FontStyle style, Color color, Vector2 position, Vector2 size, TextAnchor alignment)
        {
            GameObject textObject = new(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.text = value;
            return text;
        }

        private static Button CreateButton(string name, Transform parent, string label, Vector2 position, Vector2 size)
        {
            GameObject buttonObject = new(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            buttonObject.GetComponent<Image>().color = new Color(0.22f, 0.42f, 0.56f);
            Text text = CreateText("Text", buttonObject.transform, label, 28, FontStyle.Bold, Color.white, Vector2.zero, size, TextAnchor.MiddleCenter);
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = Vector2.zero;
            return buttonObject.GetComponent<Button>();
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
