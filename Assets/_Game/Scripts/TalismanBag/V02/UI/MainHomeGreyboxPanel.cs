using System;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.V02.UI
{
    public sealed class MainHomeGreyboxPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text resourceText;
        [SerializeField] private Text statusText;
        [SerializeField] private Button cultivateButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button closeButton;

        private Action cultivateCallback;
        private Action continueCallback;
        private Action closeCallback;
        private bool buttonsBound;

        private void Awake()
        {
            BindButtons();
            Hide();
        }

        private void OnDestroy()
        {
            if (!buttonsBound)
            {
                return;
            }

            cultivateButton?.onClick.RemoveListener(InvokeCultivate);
            continueButton?.onClick.RemoveListener(InvokeContinue);
            closeButton?.onClick.RemoveListener(InvokeClose);
        }

        public void Show(string title, string resources, string status, Action onCultivate, Action onContinue, Action onClose)
        {
            BindButtons();
            cultivateCallback = onCultivate;
            continueCallback = onContinue;
            closeCallback = onClose;
            SetMainActionButtonsVisible(true);

            if (panel != null)
            {
                panel.SetActive(true);
                panel.transform.SetAsLastSibling();
            }

            SetText(titleText, string.IsNullOrWhiteSpace(title) ? "首页灰盒" : title);
            SetText(resourceText, resources);
            SetText(statusText, status);
        }

        public void ShowComplete(string title, string resources, string status, Action onClose)
        {
            Show(title, resources, status, null, null, onClose);
            SetMainActionButtonsVisible(false);
        }

        public void Hide()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        public static MainHomeGreyboxPanel CreateRuntime(Transform parent)
        {
            if (parent == null)
            {
                return null;
            }

            GameObject root = new("MainHomeGreyboxPanel_Runtime", typeof(RectTransform), typeof(Image), typeof(Outline));
            root.transform.SetParent(parent, false);
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = Vector2.zero;
            rootRect.sizeDelta = new Vector2(920f, 980f);

            Image image = root.GetComponent<Image>();
            image.color = new Color(0.052f, 0.058f, 0.056f, 0.98f);

            Outline outline = root.GetComponent<Outline>();
            outline.effectColor = new Color(0.72f, 0.78f, 0.62f, 0.95f);
            outline.effectDistance = new Vector2(4f, -4f);

            MainHomeGreyboxPanel panelComponent = root.AddComponent<MainHomeGreyboxPanel>();
            panelComponent.panel = root;
            panelComponent.titleText = CreateText("HomeTitle", root.transform, "首页灰盒", 44, FontStyle.Bold, new Color(0.9f, 0.96f, 0.82f), new Vector2(0f, -72f), new Vector2(780f, 70f), TextAnchor.MiddleCenter);
            panelComponent.resourceText = CreateText("HomeResources", root.transform, string.Empty, 27, FontStyle.Bold, Color.white, new Vector2(0f, -180f), new Vector2(780f, 250f), TextAnchor.UpperLeft);
            panelComponent.statusText = CreateText("HomeStatus", root.transform, string.Empty, 25, FontStyle.Normal, new Color(0.9f, 0.96f, 0.9f), new Vector2(0f, -465f), new Vector2(780f, 160f), TextAnchor.UpperLeft);
            panelComponent.cultivateButton = CreateButton("HomeCultivateButton", root.transform, "培养入口", new Vector2(-225f, 220f), new Vector2(340f, 88f), new Color(0.22f, 0.5f, 0.42f));
            panelComponent.continueButton = CreateButton("HomeContinueButton", root.transform, "继续主线", new Vector2(225f, 220f), new Vector2(340f, 88f), new Color(0.42f, 0.36f, 0.58f));
            panelComponent.closeButton = CreateButton("HomeCloseButton", root.transform, "关闭", new Vector2(0f, 92f), new Vector2(340f, 76f), new Color(0.28f, 0.32f, 0.34f));
            panelComponent.BindButtons();
            panelComponent.Hide();
            return panelComponent;
        }

        private void InvokeCultivate()
        {
            cultivateCallback?.Invoke();
        }

        private void InvokeContinue()
        {
            continueCallback?.Invoke();
        }

        private void InvokeClose()
        {
            Action callback = closeCallback;
            Hide();
            callback?.Invoke();
        }

        private void BindButtons()
        {
            if (buttonsBound)
            {
                return;
            }

            if (cultivateButton == null && continueButton == null && closeButton == null)
            {
                return;
            }

            cultivateButton?.onClick.AddListener(InvokeCultivate);
            continueButton?.onClick.AddListener(InvokeContinue);
            closeButton?.onClick.AddListener(InvokeClose);
            buttonsBound = true;
        }

        private void SetMainActionButtonsVisible(bool visible)
        {
            cultivateButton?.gameObject.SetActive(visible);
            continueButton?.gameObject.SetActive(visible);
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

        private static Button CreateButton(string name, Transform parent, string label, Vector2 position, Vector2 size, Color color)
        {
            GameObject buttonObject = new(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            buttonObject.GetComponent<Image>().color = color;
            Text text = CreateText("Text", buttonObject.transform, label, 26, FontStyle.Bold, Color.white, Vector2.zero, size, TextAnchor.MiddleCenter);
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
