using System;
using TMPro;
using TalismanBag.Contracts.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.Presentation.FormalBattle
{
    [DisallowMultipleComponent]
    public sealed class FormalBattleFeedbackVisualModule : MonoBehaviour
    {
        [SerializeField] private RectTransform moduleRect;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image triggerBadge;
        [SerializeField] private Image triggerIcon;
        [SerializeField] private TMP_Text amountLabel;
        [SerializeField] private Image resultBadge;
        [SerializeField] private Image resultIcon;

        [Header("Authorable trigger icons")]
        [SerializeField] private Sprite basicActionIcon;
        [SerializeField] private Sprite skillIcon;
        [SerializeField] private Sprite passiveIcon;
        [SerializeField] private Sprite itemTriggerIcon;
        [SerializeField] private Sprite statusTriggerIcon;
        [SerializeField] private Sprite environmentTriggerIcon;
        [SerializeField] private Color basicActionBadgeColor =
            new Color(0.50f, 0.42f, 0.28f, 0.88f);
        [SerializeField] private Color skillBadgeColor =
            new Color(0.84f, 0.40f, 0.15f, 0.88f);
        [SerializeField] private Color passiveBadgeColor =
            new Color(0.58f, 0.30f, 0.72f, 0.88f);
        [SerializeField] private Color itemTriggerBadgeColor =
            new Color(0.16f, 0.56f, 0.76f, 0.88f);
        [SerializeField] private Color statusTriggerBadgeColor =
            new Color(0.64f, 0.22f, 0.30f, 0.88f);
        [SerializeField] private Color environmentTriggerBadgeColor =
            new Color(0.34f, 0.42f, 0.40f, 0.88f);
        [SerializeField] private Color unknownTriggerBadgeColor =
            new Color(0.35f, 0.35f, 0.35f, 0.88f);

        [Header("Authorable result icons")]
        [SerializeField] private Sprite hpDamageIcon;
        [SerializeField] private Sprite shellIcon;
        [SerializeField] private Sprite guardIcon;
        [SerializeField] private Sprite healIcon;
        [SerializeField] private Sprite buffIcon;
        [SerializeField] private Sprite debuffIcon;
        [SerializeField] private Sprite controlIcon;
        [SerializeField] private Sprite cleanseIcon;
        [SerializeField] private Sprite nianIcon;

        [Header("Authorable damage distinction")]
        [SerializeField, Range(0f, 1f)] private float triggerTintStrength =
            0.28f;
        [SerializeField] private Color periodicDamageAccent =
            new Color(0.78f, 0.16f, 0.24f, 1f);
        [SerializeField, Range(0f, 1f)] private float periodicTintStrength =
            0.48f;
        [SerializeField, Min(0f)] private float periodicLateralAmplitude = 7f;
        [SerializeField, Min(0f)] private float delayedLateralAmplitude = 4f;
        [SerializeField, Min(1f)] private float popPeakScale = 1.08f;

        private Vector2 startPosition;
        private Vector3 authoredScale = Vector3.one;
        private float startedAt = -100f;
        private float durationSeconds = 0.82f;
        private float riseDistance = 76f;
        private float fadeStartNormalized = 0.68f;
        private string deliveryKind = string.Empty;
        private bool active;

        public bool IsAvailable => !active;

        private void Awake()
        {
            if (moduleRect != null)
            {
                authoredScale = moduleRect.localScale;
            }
        }

        public bool ValidateAuthoredReferences()
        {
            return moduleRect != null
                   && canvasGroup != null
                   && triggerBadge != null
                   && triggerIcon != null
                   && amountLabel != null
                   && resultBadge != null
                   && resultIcon != null
                   && basicActionIcon != null
                   && skillIcon != null
                   && passiveIcon != null
                   && itemTriggerIcon != null
                   && statusTriggerIcon != null
                   && environmentTriggerIcon != null
                   && hpDamageIcon != null
                   && shellIcon != null
                   && guardIcon != null
                   && healIcon != null
                   && buffIcon != null
                   && debuffIcon != null
                   && controlIcon != null
                   && cleanseIcon != null
                   && nianIcon != null
                   && !triggerBadge.raycastTarget
                   && !triggerIcon.raycastTarget
                   && !amountLabel.raycastTarget
                   && !resultBadge.raycastTarget
                   && !resultIcon.raycastTarget;
        }

        public void Show(
            string payload,
            Vector2 localPosition,
            TMP_FontAsset font,
            FormalBattleDamageFloatVisualStyle visualStyle,
            string feedbackTriggerKind,
            string feedbackDeliveryKind,
            string feedbackResultKind,
            float configuredDurationSeconds,
            float configuredRiseDistance,
            float configuredFadeStartNormalized,
            float now)
        {
            active = true;
            startPosition = localPosition;
            startedAt = now;
            deliveryKind = Normalize(feedbackDeliveryKind);
            durationSeconds = Mathf.Max(
                0.1f,
                configuredDurationSeconds * DeliveryDurationScale());
            riseDistance = configuredRiseDistance * DeliveryRiseScale();
            fadeStartNormalized = Mathf.Clamp01(
                configuredFadeStartNormalized);
            authoredScale = moduleRect.localScale;
            moduleRect.anchoredPosition = startPosition;
            amountLabel.text = payload ?? string.Empty;
            amountLabel.font = font;
            amountLabel.enableVertexGradient = true;
            amountLabel.colorGradient = ResolveAmountGradient(
                visualStyle,
                feedbackTriggerKind,
                feedbackDeliveryKind,
                feedbackResultKind);
            ConfigureBadge(
                triggerBadge,
                triggerIcon,
                ResolveTriggerIcon(feedbackTriggerKind),
                ResolveTriggerColor(feedbackTriggerKind));
            ConfigureBadge(
                resultBadge,
                resultIcon,
                ResolveResultIcon(feedbackResultKind),
                visualStyle.MiddleColor);
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            moduleRect.localScale = authoredScale;
            if (!moduleRect.gameObject.activeSelf)
            {
                moduleRect.gameObject.SetActive(true);
            }
        }

        public void Tick(float now)
        {
            if (!active)
            {
                return;
            }

            float progress = Mathf.Clamp01(
                (now - startedAt) / durationSeconds);
            float easedRise = 1f - Mathf.Pow(1f - progress, 2f);
            float lateral = string.Equals(
                    deliveryKind,
                    C1FormalRealtimeBattleFeedbackDeliveryKinds.Periodic,
                    StringComparison.Ordinal)
                ? Mathf.Sin(progress * Mathf.PI * 2f)
                  * periodicLateralAmplitude
                : string.Equals(
                    deliveryKind,
                    C1FormalRealtimeBattleFeedbackDeliveryKinds.Delayed,
                    StringComparison.Ordinal)
                    ? Mathf.Sin(progress * Mathf.PI)
                      * delayedLateralAmplitude
                    : 0f;
            moduleRect.anchoredPosition = startPosition
                + Vector2.up * Mathf.Lerp(0f, riseDistance, easedRise)
                + Vector2.right * lateral;

            float pop = progress < 0.24f
                ? Mathf.Lerp(1f, popPeakScale, progress / 0.24f)
                : Mathf.Lerp(
                    popPeakScale,
                    1f,
                    (progress - 0.24f) / 0.76f);
            moduleRect.localScale = authoredScale
                * pop;
            canvasGroup.alpha = progress < fadeStartNormalized
                ? 1f
                : 1f - Mathf.InverseLerp(
                    fadeStartNormalized,
                    1f,
                    progress);
            if (progress >= 1f)
            {
                Clear();
            }
        }

        public void ShiftClock(float seconds)
        {
            if (active)
            {
                startedAt += Mathf.Max(0f, seconds);
            }
        }

        public void Clear()
        {
            active = false;
            startedAt = -100f;
            deliveryKind = string.Empty;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
            if (amountLabel != null)
            {
                amountLabel.text = string.Empty;
            }
            if (triggerIcon != null)
            {
                triggerIcon.sprite = null;
                triggerIcon.enabled = false;
            }
            if (resultIcon != null)
            {
                resultIcon.sprite = null;
                resultIcon.enabled = false;
            }
            if (moduleRect != null)
            {
                moduleRect.localScale = authoredScale;
                if (moduleRect.gameObject.activeSelf)
                {
                    moduleRect.gameObject.SetActive(false);
                }
            }
        }

        private float DeliveryDurationScale()
        {
            if (string.Equals(
                    deliveryKind,
                    C1FormalRealtimeBattleFeedbackDeliveryKinds.Periodic,
                    StringComparison.Ordinal))
            {
                return 0.9f;
            }
            if (string.Equals(
                    deliveryKind,
                    C1FormalRealtimeBattleFeedbackDeliveryKinds.Delayed,
                    StringComparison.Ordinal))
            {
                return 1.08f;
            }
            if (string.Equals(
                    deliveryKind,
                    C1FormalRealtimeBattleFeedbackDeliveryKinds.Continuous,
                    StringComparison.Ordinal))
            {
                return 1.15f;
            }
            return 1f;
        }

        private float DeliveryRiseScale()
        {
            if (string.Equals(
                    deliveryKind,
                    C1FormalRealtimeBattleFeedbackDeliveryKinds.Periodic,
                    StringComparison.Ordinal))
            {
                return 0.72f;
            }
            if (string.Equals(
                    deliveryKind,
                    C1FormalRealtimeBattleFeedbackDeliveryKinds.Continuous,
                    StringComparison.Ordinal))
            {
                return 0.58f;
            }
            return 1f;
        }

        private static void ConfigureBadge(
            Image badge,
            Image icon,
            Sprite sprite,
            Color color)
        {
            if (badge == null || icon == null)
            {
                return;
            }

            bool available = sprite != null;
            badge.gameObject.SetActive(available);
            badge.color = color;
            icon.sprite = sprite;
            icon.color = Color.white;
            icon.preserveAspect = true;
            icon.enabled = available;
        }

        private Sprite ResolveTriggerIcon(string value)
        {
            switch (Normalize(value))
            {
                case C1FormalRealtimeBattleFeedbackTriggerKinds.BasicAction:
                    return basicActionIcon;
                case C1FormalRealtimeBattleFeedbackTriggerKinds.Skill:
                    return skillIcon;
                case C1FormalRealtimeBattleFeedbackTriggerKinds.Passive:
                    return passiveIcon;
                case C1FormalRealtimeBattleFeedbackTriggerKinds.ItemTrigger:
                    return itemTriggerIcon;
                case C1FormalRealtimeBattleFeedbackTriggerKinds.StatusTrigger:
                    return statusTriggerIcon;
                case C1FormalRealtimeBattleFeedbackTriggerKinds
                    .EnvironmentTrigger:
                    return environmentTriggerIcon;
                default:
                    return null;
            }
        }

        private Sprite ResolveResultIcon(string value)
        {
            switch (Normalize(value))
            {
                case C1FormalRealtimeBattleFeedbackResultKinds.HpDamage:
                    return hpDamageIcon;
                case C1FormalRealtimeBattleFeedbackResultKinds.ShellDamage:
                case C1FormalRealtimeBattleFeedbackResultKinds.ShellGain:
                    return shellIcon;
                case C1FormalRealtimeBattleFeedbackResultKinds.GuardDamage:
                case C1FormalRealtimeBattleFeedbackResultKinds.GuardGain:
                    return guardIcon;
                case C1FormalRealtimeBattleFeedbackResultKinds.Heal:
                    return healIcon;
                case C1FormalRealtimeBattleFeedbackResultKinds.BuffApply:
                case C1FormalRealtimeBattleFeedbackResultKinds.BuffRefresh:
                case C1FormalRealtimeBattleFeedbackResultKinds.BuffRemove:
                    return buffIcon;
                case C1FormalRealtimeBattleFeedbackResultKinds.DebuffApply:
                case C1FormalRealtimeBattleFeedbackResultKinds.DebuffRefresh:
                case C1FormalRealtimeBattleFeedbackResultKinds.DebuffRemove:
                    return debuffIcon;
                case C1FormalRealtimeBattleFeedbackResultKinds.Control:
                    return controlIcon;
                case C1FormalRealtimeBattleFeedbackResultKinds.Cleanse:
                    return cleanseIcon;
                case C1FormalRealtimeBattleFeedbackResultKinds.NianGain:
                case C1FormalRealtimeBattleFeedbackResultKinds.NianSpend:
                    return nianIcon;
                default:
                    return null;
            }
        }

        private Color ResolveTriggerColor(string value)
        {
            switch (Normalize(value))
            {
                case C1FormalRealtimeBattleFeedbackTriggerKinds.BasicAction:
                    return basicActionBadgeColor;
                case C1FormalRealtimeBattleFeedbackTriggerKinds.Skill:
                    return skillBadgeColor;
                case C1FormalRealtimeBattleFeedbackTriggerKinds.Passive:
                    return passiveBadgeColor;
                case C1FormalRealtimeBattleFeedbackTriggerKinds.ItemTrigger:
                    return itemTriggerBadgeColor;
                case C1FormalRealtimeBattleFeedbackTriggerKinds.StatusTrigger:
                    return statusTriggerBadgeColor;
                case C1FormalRealtimeBattleFeedbackTriggerKinds
                    .EnvironmentTrigger:
                    return environmentTriggerBadgeColor;
                default:
                    return unknownTriggerBadgeColor;
            }
        }

        private VertexGradient ResolveAmountGradient(
            FormalBattleDamageFloatVisualStyle visualStyle,
            string feedbackTriggerKind,
            string feedbackDeliveryKind,
            string feedbackResultKind)
        {
            if (!string.Equals(
                    Normalize(feedbackResultKind),
                    C1FormalRealtimeBattleFeedbackResultKinds.HpDamage,
                    StringComparison.Ordinal))
            {
                return visualStyle.Gradient;
            }

            bool periodic = string.Equals(
                Normalize(feedbackDeliveryKind),
                C1FormalRealtimeBattleFeedbackDeliveryKinds.Periodic,
                StringComparison.Ordinal);
            Color accent = periodic
                ? periodicDamageAccent
                : ResolveTriggerColor(feedbackTriggerKind);
            float strength = periodic
                ? periodicTintStrength
                : triggerTintStrength;
            return new VertexGradient(
                Color.Lerp(visualStyle.TopColor, accent, strength),
                Color.Lerp(visualStyle.TopColor, accent, strength),
                Color.Lerp(visualStyle.BottomColor, accent, strength),
                Color.Lerp(visualStyle.MiddleColor, accent, strength));
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim();
        }

#if UNITY_EDITOR
        public void AssignForEditor(
            RectTransform configuredModuleRect,
            CanvasGroup configuredCanvasGroup,
            Image configuredTriggerBadge,
            Image configuredTriggerIcon,
            TMP_Text configuredAmountLabel,
            Image configuredResultBadge,
            Image configuredResultIcon)
        {
            moduleRect = configuredModuleRect;
            canvasGroup = configuredCanvasGroup;
            triggerBadge = configuredTriggerBadge;
            triggerIcon = configuredTriggerIcon;
            amountLabel = configuredAmountLabel;
            resultBadge = configuredResultBadge;
            resultIcon = configuredResultIcon;
        }

        public void AssignVisualSpritesForEditor(
            Sprite configuredBasicActionIcon,
            Sprite configuredSkillIcon,
            Sprite configuredPassiveIcon,
            Sprite configuredItemTriggerIcon,
            Sprite configuredStatusTriggerIcon,
            Sprite configuredEnvironmentTriggerIcon,
            Sprite configuredHpDamageIcon,
            Sprite configuredShellIcon,
            Sprite configuredGuardIcon,
            Sprite configuredHealIcon,
            Sprite configuredBuffIcon,
            Sprite configuredDebuffIcon,
            Sprite configuredControlIcon,
            Sprite configuredCleanseIcon,
            Sprite configuredNianIcon)
        {
            basicActionIcon = configuredBasicActionIcon;
            skillIcon = configuredSkillIcon;
            passiveIcon = configuredPassiveIcon;
            itemTriggerIcon = configuredItemTriggerIcon;
            statusTriggerIcon = configuredStatusTriggerIcon;
            environmentTriggerIcon = configuredEnvironmentTriggerIcon;
            hpDamageIcon = configuredHpDamageIcon;
            shellIcon = configuredShellIcon;
            guardIcon = configuredGuardIcon;
            healIcon = configuredHealIcon;
            buffIcon = configuredBuffIcon;
            debuffIcon = configuredDebuffIcon;
            controlIcon = configuredControlIcon;
            cleanseIcon = configuredCleanseIcon;
            nianIcon = configuredNianIcon;
        }

        public void SetAuthoringPreviewForEditor(
            string payload,
            TMP_FontAsset font,
            FormalBattleDamageFloatVisualStyle visualStyle,
            string feedbackTriggerKind,
            string feedbackDeliveryKind,
            string feedbackResultKind)
        {
            Show(
                payload,
                moduleRect == null
                    ? Vector2.zero
                    : moduleRect.anchoredPosition,
                font,
                visualStyle,
                feedbackTriggerKind,
                feedbackDeliveryKind,
                feedbackResultKind,
                1f,
                0f,
                0.99f,
                0f);
            active = false;
        }
#endif
    }
}
