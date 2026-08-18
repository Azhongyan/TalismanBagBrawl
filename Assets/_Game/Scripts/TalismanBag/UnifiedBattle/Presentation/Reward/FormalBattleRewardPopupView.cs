using System;
using TMPro;
using TalismanBag.Items.Generation;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.UnifiedBattle.Presentation.Reward
{
    [DisallowMultipleComponent]
    public sealed class FormalBattleRewardPopupView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform panelRoot;
        [SerializeField] private Image modalBackdrop;
        [SerializeField] private Image panelBackground;
        [SerializeField] private Image rarityAccent;
        [SerializeField] private Image artworkFrame;
        [SerializeField] private Image artworkImage;
        [SerializeField] private TMP_Text phaseLabel;
        [SerializeField] private TMP_Text itemNameLabel;
        [SerializeField] private TMP_Text rarityLabel;
        [SerializeField] private TMP_Text effectLabel;
        [SerializeField] private TMP_Text instanceLabel;
        [SerializeField] private TMP_Text hintLabel;
        [SerializeField] private Button actionButton;
        [SerializeField] private TMP_Text actionLabel;
        [SerializeField] private Sprite whiteBackground;
        [SerializeField] private Sprite greenBackground;
        [SerializeField] private Sprite blueBackground;
        [SerializeField] private Sprite purpleBackground;
        [SerializeField] private Sprite orangeBackground;

        private Action action;
        private bool listenersBound;
        private bool presenting;
        private float revealProgress;
        private string presentedItemInstanceId = string.Empty;

        public string PresentedItemInstanceId => presentedItemInstanceId;
        public Button ActionButton => actionButton;

        public bool ValidateAuthoredReferences()
        {
            UnityEngine.Object[] required =
            {
                canvasGroup,
                panelRoot,
                modalBackdrop,
                panelBackground,
                rarityAccent,
                artworkFrame,
                artworkImage,
                phaseLabel,
                itemNameLabel,
                rarityLabel,
                effectLabel,
                instanceLabel,
                hintLabel,
                actionButton,
                actionLabel,
                whiteBackground,
                greenBackground,
                blueBackground,
                purpleBackground,
                orangeBackground
            };
            if (Array.Exists(required, value => value == null)
                || canvasGroup.transform != transform
                || panelRoot.parent != transform
                || actionLabel.transform.parent != actionButton.transform)
            {
                return false;
            }

            return IsPopupVisual(panelBackground.transform)
                   && IsPopupVisual(rarityAccent.transform)
                   && IsPopupVisual(artworkFrame.transform)
                   && IsPopupVisual(artworkImage.transform)
                   && IsPopupVisual(phaseLabel.transform)
                   && IsPopupVisual(itemNameLabel.transform)
                   && IsPopupVisual(rarityLabel.transform)
                   && IsPopupVisual(effectLabel.transform)
                   && IsPopupVisual(instanceLabel.transform)
                   && IsPopupVisual(hintLabel.transform)
                   && IsPopupVisual(actionButton.transform)
                   && modalBackdrop.transform.parent == transform
                   && modalBackdrop.raycastTarget
                   && actionButton.targetGraphic != null
                   && actionButton.targetGraphic.raycastTarget
                   && artworkImage.preserveAspect
                   && !phaseLabel.raycastTarget
                   && !itemNameLabel.raycastTarget
                   && !rarityLabel.raycastTarget
                   && !effectLabel.raycastTarget
                   && !instanceLabel.raycastTarget
                   && !hintLabel.raycastTarget
                   && !actionLabel.raycastTarget;
        }

        private bool IsPopupVisual(Transform visual)
        {
            return visual != null
                   && visual != transform
                   && visual.IsChildOf(transform);
        }

        public bool Bind(Action configuredAction, out string diagnostic)
        {
            if (!ValidateAuthoredReferences() || configuredAction == null)
            {
                diagnostic = "FORMAL_REWARD_POPUP_BINDING_MISSING";
                return false;
            }

            Unbind();
            action = configuredAction;
            actionButton.onClick.AddListener(HandleAction);
            listenersBound = true;
            diagnostic = "FORMAL_REWARD_POPUP_BOUND";
            return true;
        }

        public void Unbind()
        {
            if (listenersBound && actionButton != null)
            {
                actionButton.onClick.RemoveListener(HandleAction);
            }

            listenersBound = false;
            action = null;
        }

        public void RenderPending(string stageId)
        {
            presentedItemInstanceId = string.Empty;
            phaseLabel.text = "战斗胜利";
            itemNameLabel.text = "奖励尚未领取";
            rarityLabel.text = string.IsNullOrWhiteSpace(stageId)
                ? "本次战利等待确认"
                : stageId.Trim() + " · 战利等待确认";
            effectLabel.text = "点击领取后，本次真实奖励才会进入道具栏。";
            instanceLabel.text = "每场胜利仅领取一次";
            hintLabel.text = "领取与整理是两个独立步骤";
            actionLabel.text = "领取奖励";
            artworkImage.sprite = null;
            artworkImage.enabled = false;
            artworkFrame.color = new Color(0.27f, 0.25f, 0.2f, 0.92f);
            panelBackground.sprite = whiteBackground;
            rarityAccent.color = new Color(0.78f, 0.67f, 0.43f, 0.95f);
            actionButton.interactable = true;
            Show();
        }

        public void RenderClaimed(
            string exactItemInstanceId,
            string itemName,
            string rarityDisplayName,
            string effectSummary,
            Sprite artwork,
            ItemInstanceRarity rarity)
        {
            presentedItemInstanceId = exactItemInstanceId ?? string.Empty;
            phaseLabel.text = "获得奖励";
            itemNameLabel.text = itemName ?? string.Empty;
            rarityLabel.text = rarityDisplayName ?? string.Empty;
            effectLabel.text = effectSummary ?? string.Empty;
            instanceLabel.text = presentedItemInstanceId.Length == 0
                ? "奖励实例不可用"
                : "唯一奖励 · " + Tail(presentedItemInstanceId, 10);
            hintLabel.text = "点击领取道具后进入整备，可调整棋盘与背包";
            actionLabel.text = "领取道具";
            artworkImage.sprite = artwork;
            artworkImage.enabled = artwork != null;
            artworkFrame.color = RarityColor(rarity, 0.32f);
            panelBackground.sprite = BackgroundFor(rarity);
            rarityAccent.color = RarityColor(rarity, 0.98f);
            actionButton.interactable = artwork != null
                                        && presentedItemInstanceId.Length > 0;
            Show();
        }

        public void Hide()
        {
            presenting = false;
            revealProgress = 0f;
            presentedItemInstanceId = string.Empty;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            if (panelRoot != null)
            {
                panelRoot.localScale = Vector3.one;
            }
        }

        private void Show()
        {
            presenting = true;
            revealProgress = 0f;
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            panelRoot.localScale = Vector3.one * 0.96f;
        }

        private void Update()
        {
            if (!presenting || canvasGroup == null || panelRoot == null)
            {
                return;
            }

            revealProgress = Mathf.Min(1f,
                revealProgress + Time.unscaledDeltaTime / 0.18f);
            float eased = 1f - Mathf.Pow(1f - revealProgress, 3f);
            canvasGroup.alpha = eased;
            panelRoot.localScale = Vector3.one * Mathf.Lerp(0.96f, 1f, eased);
        }

        private void HandleAction()
        {
            action?.Invoke();
        }

        private void OnDestroy()
        {
            Unbind();
        }

        private Sprite BackgroundFor(ItemInstanceRarity rarity)
        {
            return rarity switch
            {
                ItemInstanceRarity.Green => greenBackground,
                ItemInstanceRarity.Blue => blueBackground,
                ItemInstanceRarity.Purple => purpleBackground,
                ItemInstanceRarity.Orange => orangeBackground,
                _ => whiteBackground
            };
        }

        private static Color RarityColor(
            ItemInstanceRarity rarity,
            float alpha)
        {
            Color color = rarity switch
            {
                ItemInstanceRarity.Green => new Color(0.38f, 0.72f, 0.49f),
                ItemInstanceRarity.Blue => new Color(0.34f, 0.61f, 0.9f),
                ItemInstanceRarity.Purple => new Color(0.68f, 0.43f, 0.88f),
                ItemInstanceRarity.Orange => new Color(0.94f, 0.58f, 0.22f),
                _ => new Color(0.82f, 0.75f, 0.61f)
            };
            color.a = alpha;
            return color;
        }

        private static string Tail(string value, int length)
        {
            string safe = value ?? string.Empty;
            return safe.Length <= length
                ? safe
                : safe.Substring(safe.Length - length, length);
        }

#if UNITY_EDITOR
        public void AssignForEditor(
            CanvasGroup configuredCanvasGroup,
            RectTransform configuredPanelRoot,
            Image configuredModalBackdrop,
            Image configuredPanelBackground,
            Image configuredRarityAccent,
            Image configuredArtworkFrame,
            Image configuredArtworkImage,
            TMP_Text configuredPhaseLabel,
            TMP_Text configuredItemNameLabel,
            TMP_Text configuredRarityLabel,
            TMP_Text configuredEffectLabel,
            TMP_Text configuredInstanceLabel,
            TMP_Text configuredHintLabel,
            Button configuredActionButton,
            TMP_Text configuredActionLabel,
            Sprite configuredWhiteBackground,
            Sprite configuredGreenBackground,
            Sprite configuredBlueBackground,
            Sprite configuredPurpleBackground,
            Sprite configuredOrangeBackground)
        {
            canvasGroup = configuredCanvasGroup;
            panelRoot = configuredPanelRoot;
            modalBackdrop = configuredModalBackdrop;
            panelBackground = configuredPanelBackground;
            rarityAccent = configuredRarityAccent;
            artworkFrame = configuredArtworkFrame;
            artworkImage = configuredArtworkImage;
            phaseLabel = configuredPhaseLabel;
            itemNameLabel = configuredItemNameLabel;
            rarityLabel = configuredRarityLabel;
            effectLabel = configuredEffectLabel;
            instanceLabel = configuredInstanceLabel;
            hintLabel = configuredHintLabel;
            actionButton = configuredActionButton;
            actionLabel = configuredActionLabel;
            whiteBackground = configuredWhiteBackground;
            greenBackground = configuredGreenBackground;
            blueBackground = configuredBlueBackground;
            purpleBackground = configuredPurpleBackground;
            orangeBackground = configuredOrangeBackground;
        }
#endif
    }
}
