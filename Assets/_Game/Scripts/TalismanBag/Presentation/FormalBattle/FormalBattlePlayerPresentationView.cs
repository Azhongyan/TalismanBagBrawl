using System.Globalization;
using System.Collections.Generic;
using TMPro;
using TalismanBag.Contracts.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.Presentation.FormalBattle
{
    [DisallowMultipleComponent]
    public sealed class FormalBattlePlayerPresentationView : MonoBehaviour
    {
        [SerializeField] private Image avatarImage;
        [SerializeField] private RectTransform avatarRect;
        [SerializeField] private Image hpFillImage;
        [SerializeField] private TMP_Text hpLabel;
        [SerializeField] private GameObject nianRoot;
        [SerializeField] private Image nianFillImage;
        [SerializeField] private TMP_Text nianLabel;
        [Header("Authorable Nian receive feedback")]
        [SerializeField, Min(0.1f)]
        private float nianReceivePulseDuration = 0.42f;
        [SerializeField, Range(1f, 1.25f)]
        private float nianReceivePulseScale = 1.08f;
        [SerializeField] private Color nianReceivePulseColor =
            new Color(0.72f, 1f, 0.92f, 1f);
        [SerializeField] private Image hitOverlayImage;
        [SerializeField] private RectTransform damageAnchor;
        [SerializeField] private RectTransform benefitFloatAnchor;
        [SerializeField] private FormalBattleFeedbackReceiverLanes
            feedbackReceiverLanes;
        [SerializeField] private GameObject guardRoot;
        [SerializeField] private TMP_Text guardLabel;
        [SerializeField] private FormalBattleStatusStripView statusStrip;

        private Vector3 avatarBaselineScale = Vector3.one;
        private Vector3 nianBaselineScale = Vector3.one;
        private Color nianFillBaselineColor = Color.white;
        private float hitStartedAt = -100f;
        private float nianReceiveStartedAt = -100f;
        private bool paused;
        private float pausedAt;

        public RectTransform DamageAnchor =>
            feedbackReceiverLanes != null
                ? feedbackReceiverLanes.IncomingDamageLane
                : damageAnchor != null ? damageAnchor : avatarRect;
        public RectTransform BenefitFloatAnchor =>
            feedbackReceiverLanes != null
                ? feedbackReceiverLanes.BenefitLane
                : benefitFloatAnchor;
        public RectTransform StatusFloatAnchor =>
            feedbackReceiverLanes == null
                ? null
                : feedbackReceiverLanes.StatusLane;
        public FormalBattleFeedbackReceiverLanes FeedbackReceiverLanes =>
            feedbackReceiverLanes;
        public RectTransform NianAnchor =>
            nianRoot == null ? null : nianRoot.transform as RectTransform;

        private void Awake()
        {
            if (avatarRect != null)
            {
                avatarBaselineScale = avatarRect.localScale;
            }
            if (NianAnchor != null)
            {
                nianBaselineScale = NianAnchor.localScale;
            }
            if (nianFillImage != null)
            {
                nianFillBaselineColor = nianFillImage.color;
            }
        }

        private void Update()
        {
            if (paused)
            {
                return;
            }

            float progress = Mathf.Clamp01(
                (Time.unscaledTime - hitStartedAt) / 0.28f);
            if (hitOverlayImage != null)
            {
                Color color = hitOverlayImage.color;
                color.a = progress < 1f
                    ? Mathf.Sin(progress * Mathf.PI) * 0.62f
                    : 0f;
                hitOverlayImage.color = color;
            }

            if (avatarRect != null)
            {
                float recoil = progress < 1f
                    ? Mathf.Sin(progress * Mathf.PI * 2f) * 0.035f
                    : 0f;
                avatarRect.localScale = avatarBaselineScale
                    * (1f - Mathf.Abs(recoil));
            }

            ApplyNianReceiveFeedback();
        }

        public void Bind(C1FormalRealtimeBattlePlayerSnapshot snapshot)
        {
            Bind(snapshot, null);
        }

        public void Bind(
            C1FormalRealtimeBattlePlayerSnapshot snapshot,
            C1FormalRealtimeBattleNianStateSnapshot nianSnapshot)
        {
            if (snapshot == null)
            {
                Clear();
                return;
            }

            int maxHp = Mathf.Max(1, snapshot.maxHp);
            int currentHp = Mathf.Clamp(snapshot.currentHp, 0, maxHp);
            if (hpFillImage != null)
            {
                hpFillImage.fillAmount = currentHp / (float)maxHp;
            }

            if (hpLabel != null)
            {
                hpLabel.text = currentHp.ToString(CultureInfo.InvariantCulture)
                    + " / " + maxHp.ToString(CultureInfo.InvariantCulture);
            }

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
            ApplyNian(nianSnapshot);
        }

        public void ApplyHp(int currentHp, int maxHp)
        {
            int safeMax = Mathf.Max(1, maxHp);
            int safeCurrent = Mathf.Clamp(currentHp, 0, safeMax);
            if (hpFillImage != null)
            {
                hpFillImage.fillAmount = safeCurrent / (float)safeMax;
            }
            if (hpLabel != null)
            {
                hpLabel.text = safeCurrent.ToString(CultureInfo.InvariantCulture)
                    + " / " + safeMax.ToString(CultureInfo.InvariantCulture);
            }
        }

        public void ApplyNian(
            C1FormalRealtimeBattleNianStateSnapshot snapshot)
        {
            if (snapshot == null)
            {
                if (nianRoot != null) nianRoot.SetActive(false);
                if (nianFillImage != null) nianFillImage.fillAmount = 0f;
                if (nianLabel != null) nianLabel.text = string.Empty;
                return;
            }

            int safeMax = Mathf.Max(1, snapshot.maxNian);
            int safeCurrent = Mathf.Clamp(snapshot.currentNian, 0, safeMax);
            nianRoot.SetActive(true);
            nianFillImage.fillAmount = safeCurrent / (float)safeMax;
            nianLabel.text = safeCurrent.ToString(CultureInfo.InvariantCulture)
                + " / " + safeMax.ToString(CultureInfo.InvariantCulture);
        }

        public bool ApplyObservability(
            C1FormalRealtimeBattlePlayerSnapshot snapshot,
            IReadOnlyList<C1FormalRealtimeBattleStatusSnapshot> statuses,
            FormalBattlePresentationProfile profile,
            long battleTimeMs,
            out string diagnostic)
        {
            if (snapshot == null
                || guardRoot == null
                || guardLabel == null
                || statusStrip == null)
            {
                diagnostic = "FORMAL_PLAYER_OBSERVABILITY_REFERENCE_INVALID";
                return false;
            }

            ApplyHp(snapshot.currentHp, snapshot.maxHp);
            long currentGuard = System.Math.Max(0L, snapshot.guard);
            bool showGuard = currentGuard > 0L;
            RectTransform guardRect = guardRoot.transform as RectTransform;
            if (!statusStrip.ArrangeWithLeadingPinned(
                guardRect,
                showGuard))
            {
                diagnostic = "FORMAL_PLAYER_GUARD_STATUS_FLOW_INVALID";
                return false;
            }
            guardRoot.SetActive(showGuard);
            guardLabel.text = showGuard
                ? currentGuard.ToString(CultureInfo.InvariantCulture)
                : string.Empty;
            if (!statusStrip.Bind(
                    statuses,
                    profile,
                    battleTimeMs,
                    out diagnostic))
            {
                return false;
            }

            diagnostic = "FORMAL_PLAYER_OBSERVABILITY_BOUND";
            return true;
        }

        public void PlayHit()
        {
            hitStartedAt = Time.unscaledTime;
        }

        public void PlayNianReceive()
        {
            nianReceiveStartedAt = Time.unscaledTime;
            ApplyNianReceiveFeedback();
        }

        public void SetPaused(bool value)
        {
            if (paused == value)
            {
                return;
            }
            paused = value;
            if (paused)
            {
                pausedAt = Time.unscaledTime;
            }
            else
            {
                float shift = Mathf.Max(
                    0f,
                    Time.unscaledTime - pausedAt);
                hitStartedAt += shift;
                nianReceiveStartedAt += shift;
            }
        }

        public void Clear()
        {
            hitStartedAt = -100f;
            nianReceiveStartedAt = -100f;
            paused = false;
            pausedAt = 0f;
            if (avatarRect != null)
            {
                avatarRect.localScale = avatarBaselineScale;
            }
            if (hitOverlayImage != null)
            {
                Color color = hitOverlayImage.color;
                color.a = 0f;
                hitOverlayImage.color = color;
            }
            if (hpFillImage != null)
            {
                hpFillImage.fillAmount = 1f;
            }
            if (hpLabel != null)
            {
                hpLabel.text = string.Empty;
            }
            if (nianRoot != null) nianRoot.SetActive(false);
            if (nianFillImage != null) nianFillImage.fillAmount = 0f;
            if (nianLabel != null) nianLabel.text = string.Empty;
            if (NianAnchor != null)
            {
                NianAnchor.localScale = nianBaselineScale;
            }
            if (nianFillImage != null)
            {
                nianFillImage.color = nianFillBaselineColor;
            }
            if (guardRoot != null)
            {
                guardRoot.SetActive(false);
            }
            if (guardLabel != null)
            {
                guardLabel.text = string.Empty;
            }
            statusStrip?.Clear();
        }

        public bool ValidateAuthoredReferences()
        {
            return avatarImage != null
                   && avatarRect != null
                   && hpFillImage != null
                   && hpLabel != null
                   && nianRoot != null
                   && nianFillImage != null
                   && nianLabel != null
                   && hitOverlayImage != null
                   && guardRoot != null
                   && guardLabel != null
                   && statusStrip != null
                   && statusStrip.ValidateAuthoredReferences()
                   && feedbackReceiverLanes != null
                   && feedbackReceiverLanes.ValidateAuthoredReferences()
                   && DamageAnchor != null
                   && BenefitFloatAnchor != null
                   && StatusFloatAnchor != null
                   && NianAnchor != null
                   && !avatarImage.raycastTarget
                   && !hpFillImage.raycastTarget
                   && !nianFillImage.raycastTarget
                   && !hitOverlayImage.raycastTarget;
        }

        private void ApplyNianReceiveFeedback()
        {
            if (NianAnchor == null || nianFillImage == null)
            {
                return;
            }

            float elapsed = Time.unscaledTime - nianReceiveStartedAt;
            float progress = Mathf.Clamp01(
                elapsed / Mathf.Max(0.1f, nianReceivePulseDuration));
            float pulse = elapsed >= 0f && progress < 1f
                ? Mathf.Sin(progress * Mathf.PI)
                : 0f;
            NianAnchor.localScale = nianBaselineScale
                * Mathf.Lerp(1f, nianReceivePulseScale, pulse);
            Color pulseColor = nianReceivePulseColor;
            pulseColor.a = nianFillBaselineColor.a;
            nianFillImage.color = Color.Lerp(
                nianFillBaselineColor,
                pulseColor,
                pulse);
        }

#if UNITY_EDITOR
        public void AssignForEditor(
            Image configuredAvatarImage,
            RectTransform configuredAvatarRect,
            Image configuredHpFillImage,
            TMP_Text configuredHpLabel,
            Image configuredHitOverlayImage,
            RectTransform configuredDamageAnchor)
        {
            avatarImage = configuredAvatarImage;
            avatarRect = configuredAvatarRect;
            hpFillImage = configuredHpFillImage;
            hpLabel = configuredHpLabel;
            hitOverlayImage = configuredHitOverlayImage;
            damageAnchor = configuredDamageAnchor;
        }

        public void AssignReadabilityForEditor(
            GameObject configuredGuardRoot,
            TMP_Text configuredGuardLabel,
            FormalBattleStatusStripView configuredStatusStrip)
        {
            guardRoot = configuredGuardRoot;
            guardLabel = configuredGuardLabel;
            statusStrip = configuredStatusStrip;
        }

        public void AssignBenefitFloatAnchorForEditor(
            RectTransform configuredBenefitFloatAnchor)
        {
            benefitFloatAnchor = configuredBenefitFloatAnchor;
        }

        public void AssignFeedbackReceiverLanesForEditor(
            FormalBattleFeedbackReceiverLanes configuredLanes)
        {
            feedbackReceiverLanes = configuredLanes;
        }

        public void AssignNianForEditor(
            GameObject configuredNianRoot,
            Image configuredNianFillImage,
            TMP_Text configuredNianLabel)
        {
            nianRoot = configuredNianRoot;
            nianFillImage = configuredNianFillImage;
            nianLabel = configuredNianLabel;
        }
#endif
    }
}
