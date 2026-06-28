using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.V02.UI
{
    public sealed class V02BossRewardPanel : MonoBehaviour
    {
        private const int RewardColumnCount = 5;
        private static readonly string[] DefaultRewardLines =
        {
            "剑丸 x1",
            "灵石 x120",
            "符纸 x60",
            "朱砂 x10",
            "初阶符胚 x1",
            "修为 x20"
        };

        [SerializeField] private GameObject panel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text rewardListText;
        [SerializeField] private RectTransform rewardGridViewport;
        [SerializeField] private RectTransform rewardGridContent;
        [SerializeField] private Button confirmButton;

        private Action confirmCallback;
        private Button boundConfirmButton;
        private readonly List<GameObject> runtimeRewardCards = new();

        private void Awake()
        {
            ResolveReferences();
            BindConfirmButton();
            Hide();
        }

        private void OnDestroy()
        {
            if (boundConfirmButton != null)
            {
                boundConfirmButton.onClick.RemoveListener(Confirm);
            }
        }

        public void Show(IReadOnlyList<string> rewards, Action onConfirm)
        {
            ResolveReferences();
            BindConfirmButton();
            confirmCallback = onConfirm;

            if (panel != null)
            {
                panel.SetActive(true);
                panel.transform.SetAsLastSibling();
            }

            SetText(titleText, "1-10 章节结算");
            SetText(descriptionText, "你击败了 1-10 Boss。领取以下固定奖励后，主线试炼进入培养阶段。");
            RenderRewards(rewards);
            if (confirmButton != null)
            {
                confirmButton.interactable = true;
            }
        }

        public void Show(string title, string description, IReadOnlyList<string> rewards, Action onConfirm)
        {
            ResolveReferences();
            BindConfirmButton();
            confirmCallback = onConfirm;

            if (panel != null)
            {
                panel.SetActive(true);
                panel.transform.SetAsLastSibling();
            }

            SetText(titleText, title);
            SetText(descriptionText, description);
            RenderRewards(rewards);
            if (confirmButton != null)
            {
                confirmButton.interactable = true;
            }
        }

        public void Hide()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        public static V02BossRewardPanel CreateRuntime(Transform parent)
        {
            if (parent == null)
            {
                return null;
            }

            GameObject root = new("V02BossRewardPanel_Runtime", typeof(RectTransform), typeof(Image), typeof(Outline));
            root.transform.SetParent(parent, false);
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = Vector2.zero;
            rootRect.sizeDelta = new Vector2(880f, 980f);

            Image image = root.GetComponent<Image>();
            image.color = new Color(0.05f, 0.055f, 0.05f, 0.98f);

            Outline outline = root.GetComponent<Outline>();
            outline.effectColor = new Color(0.92f, 0.72f, 0.34f, 0.95f);
            outline.effectDistance = new Vector2(4f, -4f);

            V02BossRewardPanel panelComponent = root.AddComponent<V02BossRewardPanel>();
            panelComponent.panel = root;
            panelComponent.titleText = CreateText("BossRewardTitle", root.transform, "1-10 章节结算", 44, FontStyle.Bold, new Color(1f, 0.88f, 0.56f), new Vector2(0f, -72f), new Vector2(760f, 68f), TextAnchor.MiddleCenter);
            panelComponent.descriptionText = CreateText("BossRewardDescription", root.transform, string.Empty, 25, FontStyle.Normal, new Color(0.88f, 0.96f, 0.9f), new Vector2(0f, -170f), new Vector2(760f, 118f), TextAnchor.UpperLeft);
            panelComponent.rewardGridViewport = CreateRewardGrid(root.transform, out RectTransform rewardGridContent);
            panelComponent.rewardGridContent = rewardGridContent;
            panelComponent.confirmButton = CreateButton("BossRewardConfirmButton", root.transform, "领取奖励", new Vector2(0f, 98f), new Vector2(520f, 94f));
            panelComponent.BindConfirmButton();
            panelComponent.Hide();
            return panelComponent;
        }

        private void Confirm()
        {
            Action callback = confirmCallback;
            if (callback == null)
            {
                return;
            }

            if (confirmButton != null)
            {
                confirmButton.interactable = false;
            }

            try
            {
                callback.Invoke();
                confirmCallback = null;
                Hide();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                if (confirmButton != null)
                {
                    confirmButton.interactable = true;
                }
            }
        }

        private void BindConfirmButton()
        {
            if (boundConfirmButton == confirmButton)
            {
                return;
            }

            if (boundConfirmButton != null)
            {
                boundConfirmButton.onClick.RemoveListener(Confirm);
            }

            if (confirmButton == null)
            {
                return;
            }

            confirmButton.onClick.AddListener(Confirm);
            boundConfirmButton = confirmButton;
        }

        private void ResolveReferences()
        {
            panel ??= gameObject;
            if (confirmButton == null)
            {
                confirmButton = GetComponentInChildren<Button>(true);
            }

            if (rewardGridContent == null)
            {
                Transform content = transform.Find("BossRewardGridViewport/BossRewardGridContent");
                rewardGridContent = content as RectTransform;
            }

            if (rewardGridViewport == null && rewardGridContent != null)
            {
                rewardGridViewport = rewardGridContent.parent as RectTransform;
            }

            if (rewardGridContent == null && panel != null)
            {
                rewardGridViewport = CreateRewardGrid(panel.transform, out rewardGridContent);
            }

            if (rewardListText != null)
            {
                rewardListText.gameObject.SetActive(false);
            }
        }

        private static string BuildRewardText(IReadOnlyList<string> rewards)
        {
            List<string> lines = new();
            IReadOnlyList<string> rewardLines = GetRewardLines(rewards);
            for (int i = 0; i < rewardLines.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(rewardLines[i]))
                {
                    lines.Add($"- {rewardLines[i]}");
                }
            }

            return string.Join("\n", lines);
        }

        private void RenderRewards(IReadOnlyList<string> rewards)
        {
            if (rewardListText != null)
            {
                rewardListText.text = string.Empty;
                rewardListText.gameObject.SetActive(false);
            }

            if (rewardGridContent == null)
            {
                if (rewardListText != null)
                {
                    rewardListText.gameObject.SetActive(true);
                    SetText(rewardListText, BuildRewardText(rewards));
                }

                return;
            }

            ClearRuntimeRewardCards();
            IReadOnlyList<string> rewardLines = GetRewardLines(rewards);
            for (int i = 0; i < rewardLines.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(rewardLines[i]))
                {
                    continue;
                }

                RewardDisplayEntry entry = ParseRewardEntry(rewardLines[i]);
                runtimeRewardCards.Add(CreateRewardCard($"BossRewardCard_{i + 1:00}", rewardGridContent, entry));
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(rewardGridContent);
            if (rewardGridViewport != null)
            {
                ScrollRect scrollRect = rewardGridViewport.GetComponent<ScrollRect>();
                if (scrollRect != null)
                {
                    scrollRect.verticalNormalizedPosition = 1f;
                }
            }
        }

        private void ClearRuntimeRewardCards()
        {
            for (int i = 0; i < runtimeRewardCards.Count; i++)
            {
                if (runtimeRewardCards[i] != null)
                {
                    Destroy(runtimeRewardCards[i]);
                }
            }

            runtimeRewardCards.Clear();
        }

        private static IReadOnlyList<string> GetRewardLines(IReadOnlyList<string> rewards)
        {
            return rewards == null || rewards.Count == 0 ? DefaultRewardLines : rewards;
        }

        private static RewardDisplayEntry ParseRewardEntry(string rewardLine)
        {
            string value = string.IsNullOrWhiteSpace(rewardLine) ? "未配置奖励 x0" : rewardLine.Trim();
            if (value.StartsWith("- ", StringComparison.Ordinal))
            {
                value = value.Substring(2).Trim();
            }

            int splitIndex = FindQuantitySplitIndex(value);
            if (splitIndex < 0)
            {
                return new RewardDisplayEntry(value, string.Empty);
            }

            string name = value.Substring(0, splitIndex).Trim();
            string amount = value.Substring(splitIndex).Trim();
            return new RewardDisplayEntry(
                string.IsNullOrWhiteSpace(name) ? value : name,
                string.IsNullOrWhiteSpace(amount) ? string.Empty : amount);
        }

        private static int FindQuantitySplitIndex(string value)
        {
            int spacedXIndex = value.LastIndexOf(" x", StringComparison.OrdinalIgnoreCase);
            if (spacedXIndex > 0 && spacedXIndex < value.Length - 2)
            {
                return spacedXIndex;
            }

            int multiplyIndex = value.LastIndexOf("×", StringComparison.Ordinal);
            return multiplyIndex > 0 && multiplyIndex < value.Length - 1 ? multiplyIndex : -1;
        }

        private static RectTransform CreateRewardGrid(Transform parent, out RectTransform content)
        {
            GameObject viewportObject = new("BossRewardGridViewport", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect));
            viewportObject.transform.SetParent(parent, false);
            RectTransform viewport = viewportObject.GetComponent<RectTransform>();
            viewport.anchorMin = new Vector2(0.5f, 1f);
            viewport.anchorMax = new Vector2(0.5f, 1f);
            viewport.pivot = new Vector2(0.5f, 1f);
            viewport.anchoredPosition = new Vector2(0f, -310f);
            viewport.sizeDelta = new Vector2(790f, 410f);

            Image viewportImage = viewportObject.GetComponent<Image>();
            viewportImage.color = new Color(0.105f, 0.115f, 0.09f, 0.78f);

            Mask mask = viewportObject.GetComponent<Mask>();
            mask.showMaskGraphic = true;

            GameObject contentObject = new("BossRewardGridContent", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
            contentObject.transform.SetParent(viewport, false);
            content = contentObject.GetComponent<RectTransform>();
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;

            GridLayoutGroup grid = contentObject.GetComponent<GridLayoutGroup>();
            grid.padding = new RectOffset(12, 12, 12, 12);
            grid.cellSize = new Vector2(138f, 178f);
            grid.spacing = new Vector2(12f, 18f);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = RewardColumnCount;

            ContentSizeFitter fitter = contentObject.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect scrollRect = viewportObject.GetComponent<ScrollRect>();
            scrollRect.content = content;
            scrollRect.viewport = viewport;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 28f;

            return viewport;
        }

        private static GameObject CreateRewardCard(string name, Transform parent, RewardDisplayEntry entry)
        {
            GameObject cardObject = new(name, typeof(RectTransform), typeof(Image), typeof(Outline));
            cardObject.transform.SetParent(parent, false);
            RectTransform cardRect = cardObject.GetComponent<RectTransform>();
            cardRect.sizeDelta = new Vector2(138f, 178f);

            Image cardImage = cardObject.GetComponent<Image>();
            cardImage.color = new Color(0.16f, 0.13f, 0.09f, 0.94f);

            Outline cardOutline = cardObject.GetComponent<Outline>();
            cardOutline.effectColor = new Color(0.78f, 0.56f, 0.24f, 0.55f);
            cardOutline.effectDistance = new Vector2(2f, -2f);

            GameObject slotObject = new("ItemSlot", typeof(RectTransform), typeof(Image), typeof(Outline));
            slotObject.transform.SetParent(cardObject.transform, false);
            RectTransform slotRect = slotObject.GetComponent<RectTransform>();
            slotRect.anchorMin = new Vector2(0.5f, 1f);
            slotRect.anchorMax = new Vector2(0.5f, 1f);
            slotRect.pivot = new Vector2(0.5f, 1f);
            slotRect.anchoredPosition = new Vector2(0f, -10f);
            slotRect.sizeDelta = new Vector2(82f, 82f);

            Image slotImage = slotObject.GetComponent<Image>();
            slotImage.color = new Color(0.09f, 0.10f, 0.075f, 1f);

            Outline slotOutline = slotObject.GetComponent<Outline>();
            slotOutline.effectColor = new Color(1f, 0.82f, 0.42f, 0.85f);
            slotOutline.effectDistance = new Vector2(2f, -2f);

            Text iconText = CreateText("ItemGlyph", slotObject.transform, GetIconLabel(entry.Name), 34, FontStyle.Bold, new Color(1f, 0.88f, 0.52f), Vector2.zero, slotRect.sizeDelta, TextAnchor.MiddleCenter);
            FillRect(iconText.GetComponent<RectTransform>());

            Text nameText = CreateText("ItemDescription", cardObject.transform, entry.Name, 19, FontStyle.Bold, new Color(0.98f, 0.94f, 0.82f), new Vector2(0f, -100f), new Vector2(122f, 44f), TextAnchor.MiddleCenter);
            nameText.resizeTextForBestFit = true;
            nameText.resizeTextMinSize = 13;
            nameText.resizeTextMaxSize = 19;

            Text amountText = CreateText("ItemAmount", cardObject.transform, entry.Amount, 18, FontStyle.Bold, new Color(0.75f, 0.95f, 0.73f), new Vector2(0f, -146f), new Vector2(122f, 28f), TextAnchor.MiddleCenter);
            amountText.resizeTextForBestFit = true;
            amountText.resizeTextMinSize = 12;
            amountText.resizeTextMaxSize = 18;

            return cardObject;
        }

        private static string GetIconLabel(string itemName)
        {
            return string.IsNullOrWhiteSpace(itemName) ? "奖" : itemName.Trim().Substring(0, 1);
        }

        private static void FillRect(RectTransform rect)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
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

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.56f, 0.36f, 0.18f);

            Text text = CreateText("Text", buttonObject.transform, label, 28, FontStyle.Bold, Color.white, Vector2.zero, size, TextAnchor.MiddleCenter);
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = Vector2.zero;

            return buttonObject.GetComponent<Button>();
        }

        private readonly struct RewardDisplayEntry
        {
            public readonly string Name;
            public readonly string Amount;

            public RewardDisplayEntry(string name, string amount)
            {
                Name = name;
                Amount = amount;
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
