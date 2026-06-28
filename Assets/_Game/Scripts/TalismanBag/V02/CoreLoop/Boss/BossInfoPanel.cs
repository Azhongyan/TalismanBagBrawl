using System;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.V02.CoreLoop.Boss
{
    public sealed class BossInfoPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text mechanismText;
        [SerializeField] private Text threatText;
        [SerializeField] private Text recommendedText;
        [SerializeField] private Text promptText;
        [SerializeField] private Button adjustButton;
        [SerializeField] private Button startButton;

        private Action adjustCallback;
        private Action startCallback;
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

            adjustButton?.onClick.RemoveListener(Adjust);
            startButton?.onClick.RemoveListener(StartBoss);
            buttonsBound = false;
        }

        public void Show(BossInfoViewModel viewModel, Action onAdjust, Action onStart)
        {
            BindButtons();
            adjustCallback = onAdjust;
            startCallback = onStart;

            if (panel != null)
            {
                panel.SetActive(true);
                panel.transform.SetAsLastSibling();
            }

            SetText(titleText, viewModel?.bossName ?? "2-10 Boss");
            SetText(mechanismText, $"\u673a\u5236\u6807\u7b7e\n{viewModel?.mechanismTags ?? string.Empty}");
            SetText(threatText, $"\u4e3b\u8981\u5a01\u80c1\n{viewModel?.mainThreats ?? string.Empty}");
            SetText(recommendedText, $"\u63a8\u8350\u9053\u5177 / \u7b26\u7b93\n{viewModel?.recommendedTools ?? string.Empty}");
            SetText(promptText, BuildPromptText(viewModel));
            SetButtonText(startButton, "\u6311\u6218boss");
        }

        public void Hide()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        public static BossInfoPanel CreateRuntime(Transform parent)
        {
            if (parent == null)
            {
                return null;
            }

            GameObject root = new("BossInfoPanel_Runtime", typeof(RectTransform), typeof(Image), typeof(Outline));
            root.transform.SetParent(parent, false);
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = Vector2.zero;
            rootRect.sizeDelta = new Vector2(900f, 1040f);

            Image image = root.GetComponent<Image>();
            image.color = new Color(0.045f, 0.052f, 0.062f, 0.98f);

            Outline outline = root.GetComponent<Outline>();
            outline.effectColor = new Color(0.72f, 0.58f, 0.34f, 0.95f);
            outline.effectDistance = new Vector2(4f, -4f);

            BossInfoPanel panelComponent = root.AddComponent<BossInfoPanel>();
            panelComponent.panel = root;
            panelComponent.titleText = CreateText("BossInfoTitle", root.transform, "2-10 Boss", 44, FontStyle.Bold, new Color(1f, 0.88f, 0.56f), new Vector2(0f, -68f), new Vector2(780f, 72f), TextAnchor.MiddleCenter);
            panelComponent.mechanismText = CreateText("BossInfoMechanisms", root.transform, string.Empty, 26, FontStyle.Bold, new Color(0.86f, 0.94f, 1f), new Vector2(0f, -165f), new Vector2(780f, 130f), TextAnchor.UpperLeft);
            panelComponent.threatText = CreateText("BossInfoThreats", root.transform, string.Empty, 25, FontStyle.Normal, Color.white, new Vector2(0f, -330f), new Vector2(780f, 180f), TextAnchor.UpperLeft);
            panelComponent.recommendedText = CreateText("BossInfoRecommended", root.transform, string.Empty, 25, FontStyle.Normal, new Color(0.88f, 1f, 0.88f), new Vector2(0f, -540f), new Vector2(780f, 170f), TextAnchor.UpperLeft);
            panelComponent.promptText = CreateText("BossInfoPrompt", root.transform, string.Empty, 25, FontStyle.Bold, new Color(1f, 0.9f, 0.62f), new Vector2(0f, -720f), new Vector2(780f, 95f), TextAnchor.UpperLeft);
            panelComponent.adjustButton = CreateButton("BossInfoAdjustButton", root.transform, "\u6574\u5907", new Vector2(-220f, 86f), new Vector2(340f, 86f), new Color(0.28f, 0.42f, 0.34f));
            panelComponent.startButton = CreateButton("BossInfoStartButton", root.transform, "\u6311\u6218boss", new Vector2(220f, 86f), new Vector2(340f, 86f), new Color(0.62f, 0.32f, 0.18f));
            panelComponent.BindButtons();
            panelComponent.Hide();
            return panelComponent;
        }

        private void Adjust()
        {
            Action callback = adjustCallback;
            Hide();
            callback?.Invoke();
        }

        private void StartBoss()
        {
            Action callback = startCallback;
            Hide();
            callback?.Invoke();
        }

        private void BindButtons()
        {
            if (buttonsBound || adjustButton == null || startButton == null)
            {
                return;
            }

            adjustButton.onClick.AddListener(Adjust);
            startButton.onClick.AddListener(StartBoss);
            buttonsBound = true;
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
            text.font = UnityEngine.Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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

            Image image = buttonObject.GetComponent<Image>();
            image.color = color;

            Text text = CreateText("Text", buttonObject.transform, label, 27, FontStyle.Bold, Color.white, Vector2.zero, size, TextAnchor.MiddleCenter);
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

        private static void SetButtonText(Button button, string value)
        {
            if (button == null)
            {
                return;
            }

            SetText(button.GetComponentInChildren<Text>(true), value);
        }

        private static string BuildPromptText(BossInfoViewModel viewModel)
        {
            string prompt = viewModel?.preBattlePrompt ?? string.Empty;
            string rewardPrompt = "\u80dc\u5229\u540e\u53ef\u9886\u53d6\u7ae0\u8282\u5173\u952e\u5956\u52b1\u3002";
            return string.IsNullOrWhiteSpace(prompt) ? rewardPrompt : $"{prompt}\n{rewardPrompt}";
        }
    }
}
