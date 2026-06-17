using System;
using System.Collections.Generic;
using TalismanBag.V02.CoreLoop.Resources;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.V02.CoreLoop.Upgrades
{
    public sealed class TalismanUpgradePanel : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text costText;
        [SerializeField] private Text statusText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Text confirmButtonText;

        private UpgradeService upgradeService;
        private string itemId;
        private Action<string> upgradeSucceededCallback;
        private Action finishedCallback;
        private bool buttonBound;
        private bool readyToFinish;

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

        public void Show(UpgradeService service, string targetItemId, Action<string> onUpgradeSucceeded, Action onFinished)
        {
            BindConfirmButton();
            upgradeService = service;
            itemId = targetItemId;
            upgradeSucceededCallback = onUpgradeSucceeded;
            finishedCallback = onFinished;
            readyToFinish = false;

            if (panel != null)
            {
                panel.SetActive(true);
                panel.transform.SetAsLastSibling();
            }

            Refresh();
        }

        public void Hide()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        public static TalismanUpgradePanel CreateRuntime(Transform parent)
        {
            if (parent == null)
            {
                return null;
            }

            GameObject root = new("TalismanUpgradePanel_Runtime", typeof(RectTransform), typeof(Image), typeof(Outline));
            root.transform.SetParent(parent, false);
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = Vector2.zero;
            rootRect.sizeDelta = new Vector2(900f, 980f);

            Image image = root.GetComponent<Image>();
            image.color = new Color(0.045f, 0.06f, 0.055f, 0.98f);

            Outline outline = root.GetComponent<Outline>();
            outline.effectColor = new Color(0.48f, 0.88f, 0.74f, 0.95f);
            outline.effectDistance = new Vector2(4f, -4f);

            TalismanUpgradePanel panelComponent = root.AddComponent<TalismanUpgradePanel>();
            panelComponent.panel = root;
            panelComponent.titleText = CreateText("UpgradeTitle", root.transform, "符箓培养", 44, FontStyle.Bold, new Color(0.72f, 1f, 0.88f), new Vector2(0f, -72f), new Vector2(760f, 70f), TextAnchor.MiddleCenter);
            panelComponent.descriptionText = CreateText("UpgradeDescription", root.transform, string.Empty, 25, FontStyle.Normal, new Color(0.88f, 0.96f, 0.9f), new Vector2(0f, -170f), new Vector2(760f, 138f), TextAnchor.UpperLeft);
            panelComponent.costText = CreateText("UpgradeCost", root.transform, string.Empty, 28, FontStyle.Bold, Color.white, new Vector2(0f, -365f), new Vector2(760f, 260f), TextAnchor.UpperLeft);
            panelComponent.statusText = CreateText("UpgradeStatus", root.transform, string.Empty, 25, FontStyle.Normal, new Color(1f, 0.9f, 0.58f), new Vector2(0f, -635f), new Vector2(760f, 120f), TextAnchor.UpperLeft);
            panelComponent.confirmButton = CreateButton("UpgradeConfirmButton", root.transform, "培养到 Lv.2", new Vector2(0f, 98f), new Vector2(520f, 94f), out Text buttonText);
            panelComponent.confirmButtonText = buttonText;
            panelComponent.BindConfirmButton();
            panelComponent.Hide();
            return panelComponent;
        }

        private void Confirm()
        {
            if (readyToFinish)
            {
                Finish();
                return;
            }

            if (upgradeService == null)
            {
                SetText(statusText, "培养服务未初始化");
                return;
            }

            TalismanUpgradeResult result = upgradeService.TryUpgrade(itemId);
            if (!result.success)
            {
                SetText(statusText, result.message);
                return;
            }

            upgradeSucceededCallback?.Invoke(itemId);
            string displayName = result.levelConfig != null && !string.IsNullOrWhiteSpace(result.levelConfig.displayName)
                ? result.levelConfig.displayName
                : itemId;
            SetText(titleText, $"{displayName} 培养完成");
            SetText(descriptionText, $"{displayName} 已从 Lv.{result.fromLevel} 提升到 Lv.{result.toLevel}。");
            SetText(costText, BuildRemainingResourceText());
            SetText(statusText, result.levelConfig?.statModifier != null ? result.levelConfig.statModifier.summary : "等级已保存");
            SetButtonLabel("继续");
            readyToFinish = true;
        }

        private void Finish()
        {
            Action callback = finishedCallback;
            upgradeSucceededCallback = null;
            finishedCallback = null;
            Hide();
            callback?.Invoke();
        }

        private void Refresh()
        {
            if (upgradeService == null)
            {
                SetText(titleText, "符箓培养");
                SetText(descriptionText, "培养服务未初始化。");
                SetText(costText, string.Empty);
                SetText(statusText, string.Empty);
                SetButtonLabel("继续");
                readyToFinish = true;
                return;
            }

            int currentLevel = upgradeService.GetLevel(itemId);
            TalismanLevelConfig levelConfig = upgradeService.GetNextUpgrade(itemId);
            string displayName = levelConfig != null && !string.IsNullOrWhiteSpace(levelConfig.displayName)
                ? levelConfig.displayName
                : itemId;

            if (levelConfig == null)
            {
                SetText(titleText, $"{displayName} 已培养");
                SetText(descriptionText, $"当前等级 Lv.{currentLevel}，暂无下一段培养配置。");
                SetText(costText, BuildRemainingResourceText());
                SetText(statusText, "培养等级已保存。");
                SetButtonLabel("继续");
                readyToFinish = true;
                return;
            }

            SetText(titleText, $"{displayName} 培养");
            SetText(descriptionText, $"{displayName} Lv.{levelConfig.fromLevel} → Lv.{levelConfig.toLevel}\n{levelConfig.statModifier?.summary}");
            SetText(costText, BuildCostText(levelConfig.costs));

            bool canUpgrade = upgradeService.CanUpgrade(itemId, out string failureReason);
            SetText(statusText, canUpgrade ? "材料充足，可以培养。" : failureReason);
            SetButtonLabel($"培养到 Lv.{levelConfig.toLevel}");
            readyToFinish = false;
        }

        private string BuildCostText(IReadOnlyList<ResourceCost> costs)
        {
            if (costs == null || costs.Count == 0)
            {
                return "无培养消耗";
            }

            List<string> lines = new();
            for (int i = 0; i < costs.Count; i++)
            {
                ResourceCost cost = costs[i];
                if (!cost.IsValid)
                {
                    continue;
                }

                int current = upgradeService != null ? upgradeService.GetResourceAmount(cost.resourceType) : 0;
                lines.Add($"- {UpgradeService.GetResourceDisplayName(cost.resourceType)} {current}/{cost.amount}");
            }

            return string.Join("\n", lines);
        }

        private string BuildRemainingResourceText()
        {
            if (upgradeService == null)
            {
                return string.Empty;
            }

            return string.Join("\n", new[]
            {
                $"- 灵石 {upgradeService.GetResourceAmount(ResourceType.SpiritStone)}",
                $"- 符纸 {upgradeService.GetResourceAmount(ResourceType.TalismanPaper)}",
                $"- 朱砂 {upgradeService.GetResourceAmount(ResourceType.Cinnabar)}",
                $"- 初阶符胚 {upgradeService.GetResourceAmount(ResourceType.BasicTalismanEmbryo)}",
                $"- 修为 {upgradeService.GetResourceAmount(ResourceType.Cultivation)}"
            });
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

        private void SetButtonLabel(string value)
        {
            if (confirmButtonText == null && confirmButton != null)
            {
                confirmButtonText = confirmButton.GetComponentInChildren<Text>(true);
            }

            SetText(confirmButtonText, value);
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

        private static Button CreateButton(string name, Transform parent, string label, Vector2 position, Vector2 size, out Text buttonText)
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
            image.color = new Color(0.22f, 0.5f, 0.42f);

            buttonText = CreateText("Text", buttonObject.transform, label, 28, FontStyle.Bold, Color.white, Vector2.zero, size, TextAnchor.MiddleCenter);
            RectTransform textRect = buttonText.GetComponent<RectTransform>();
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
