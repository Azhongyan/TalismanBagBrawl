using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.V02.UI
{
    public sealed class V02BossRewardPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text rewardListText;
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

        public void Show(IReadOnlyList<string> rewards, Action onConfirm)
        {
            BindConfirmButton();
            confirmCallback = onConfirm;

            if (panel != null)
            {
                panel.SetActive(true);
                panel.transform.SetAsLastSibling();
            }

            SetText(titleText, "Boss 奖励");
            SetText(descriptionText, "你击败了入门破阵妖。以下奖励仅作为 V0.2 通关反馈与后续系统占位。");
            SetText(rewardListText, BuildRewardText(rewards));
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
            panelComponent.titleText = CreateText("BossRewardTitle", root.transform, "Boss 奖励", 44, FontStyle.Bold, new Color(1f, 0.88f, 0.56f), new Vector2(0f, -72f), new Vector2(760f, 68f), TextAnchor.MiddleCenter);
            panelComponent.descriptionText = CreateText("BossRewardDescription", root.transform, string.Empty, 25, FontStyle.Normal, new Color(0.88f, 0.96f, 0.9f), new Vector2(0f, -170f), new Vector2(760f, 118f), TextAnchor.UpperLeft);
            panelComponent.rewardListText = CreateText("BossRewardList", root.transform, string.Empty, 30, FontStyle.Bold, Color.white, new Vector2(0f, -390f), new Vector2(760f, 330f), TextAnchor.UpperLeft);
            panelComponent.confirmButton = CreateButton("BossRewardConfirmButton", root.transform, "领取并完成试炼", new Vector2(0f, 98f), new Vector2(520f, 94f));
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

        private static string BuildRewardText(IReadOnlyList<string> rewards)
        {
            if (rewards == null || rewards.Count == 0)
            {
                return "- 重复火符\n- 符纸\n- 灵石\n- 基础配方残页\n- 少量修为";
            }

            List<string> lines = new();
            for (int i = 0; i < rewards.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(rewards[i]))
                {
                    lines.Add($"- {rewards[i]}");
                }
            }

            return string.Join("\n", lines);
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

        private static void SetText(Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }
    }
}
