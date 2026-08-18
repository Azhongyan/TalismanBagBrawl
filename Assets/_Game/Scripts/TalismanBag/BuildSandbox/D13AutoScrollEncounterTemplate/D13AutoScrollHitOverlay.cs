using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.BuildSandbox.D13AutoScrollEncounterTemplate
{
    internal sealed class D13AutoScrollHitOverlay : MonoBehaviour
    {
        private readonly HashSet<string> presentedCueIds = new();
        private readonly List<GameObject> ownedFloaters = new();

        private RectTransform feedbackCarrier;
        private RectTransform floatRoot;
        private Image targetImage;
        private D13AutoScrollEncounterPresentationProfile profile;
        private Coroutine feedbackRoutine;
        private Color baseColor = Color.white;
        private Vector2 baseCarrierPosition;
        private Vector3 baseCarrierScale = Vector3.one;
        private Font builtInFont;
        private bool configured;

        public int PresentedCueCount => presentedCueIds.Count;

        public int LiveFloaterCount
        {
            get
            {
                ownedFloaters.RemoveAll(item => item == null);
                return ownedFloaters.Count;
            }
        }

        public void Configure(
            RectTransform targetFeedbackCarrier,
            RectTransform targetFloatRoot,
            Image image,
            D13AutoScrollEncounterPresentationProfile presentationProfile)
        {
            feedbackCarrier = targetFeedbackCarrier;
            floatRoot = targetFloatRoot;
            targetImage = image;
            profile = presentationProfile;
            configured = feedbackCarrier != null
                && floatRoot != null
                && targetImage != null
                && profile != null;

            if (!configured)
            {
                return;
            }

            baseColor = targetImage.color;
            baseCarrierPosition = feedbackCarrier.anchoredPosition;
            baseCarrierScale = feedbackCarrier.localScale;
            builtInFont =
                Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        public bool PresentNormalHit(
            string cueId,
            int displayAmount,
            float horizontalDirection)
        {
            if (!configured
                || string.IsNullOrWhiteSpace(cueId)
                || !presentedCueIds.Add(cueId))
            {
                return false;
            }

            CreateFloatingAmount(cueId, displayAmount);
            if (feedbackRoutine != null)
            {
                StopCoroutine(feedbackRoutine);
            }

            RestoreFeedbackPose();
            feedbackRoutine = StartCoroutine(
                PlayNormalHitFeedback(Mathf.Sign(horizontalDirection)));
            return true;
        }

        public void ClearTransientFeedback()
        {
            if (feedbackRoutine != null)
            {
                StopCoroutine(feedbackRoutine);
                feedbackRoutine = null;
            }

            RestoreFeedbackPose();
            DestroyOwnedFloaters();
        }

        public void ResetRound()
        {
            ClearTransientFeedback();
            presentedCueIds.Clear();
        }

        private IEnumerator PlayNormalHitFeedback(float direction)
        {
            float elapsed = 0f;
            float duration = profile.NormalHitSeconds;
            while (elapsed < duration)
            {
                elapsed += Mathf.Max(0f, Time.deltaTime);
                float normalized = Mathf.Clamp01(elapsed / duration);
                float impulse = Mathf.Sin(normalized * Mathf.PI);
                feedbackCarrier.anchoredPosition = baseCarrierPosition
                    + new Vector2(direction * 8f * impulse, 0f);
                feedbackCarrier.localScale = new Vector3(
                    baseCarrierScale.x * (1f + (0.06f * impulse)),
                    baseCarrierScale.y * (1f - (0.09f * impulse)),
                    baseCarrierScale.z);
                targetImage.color = Color.Lerp(
                    baseColor,
                    new Color(1f, 0.50f, 0.34f, baseColor.a),
                    impulse);
                yield return null;
            }

            RestoreFeedbackPose();
            feedbackRoutine = null;
        }

        private void CreateFloatingAmount(string cueId, int displayAmount)
        {
            GameObject floater = new(
                "D13HitFloat_" + cueId,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text));
            floater.hideFlags = HideFlags.DontSave;
            RectTransform rect = floater.GetComponent<RectTransform>();
            rect.SetParent(floatRoot, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            int stableOffset = StableCueHash(cueId);
            rect.anchoredPosition = new Vector2(
                -18f + (stableOffset % 37),
                76f + ((stableOffset / 37) % 19));
            rect.sizeDelta = new Vector2(150f, 48f);

            Text text = floater.GetComponent<Text>();
            text.raycastTarget = false;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = builtInFont;
            text.fontSize = 30;
            text.fontStyle = FontStyle.Bold;
            text.text = "-" + Mathf.Abs(displayAmount);
            text.color = new Color(1f, 0.82f, 0.42f, 1f);
            ownedFloaters.Add(floater);
            StartCoroutine(AnimateFloater(floater, rect, text));
        }

        private static int StableCueHash(string cueId)
        {
            unchecked
            {
                int hash = 17;
                for (int i = 0; i < cueId.Length; i++)
                {
                    hash = (hash * 31) + cueId[i];
                }

                return hash & int.MaxValue;
            }
        }

        private IEnumerator AnimateFloater(
            GameObject floater,
            RectTransform rect,
            Text text)
        {
            Vector2 start = rect.anchoredPosition;
            float elapsed = 0f;
            while (floater != null && elapsed < profile.FloatTextSeconds)
            {
                elapsed += Mathf.Max(0f, Time.deltaTime);
                float normalized = Mathf.Clamp01(
                    elapsed / profile.FloatTextSeconds);
                rect.anchoredPosition =
                    start + new Vector2(0f, 52f * normalized);
                Color color = text.color;
                color.a = 1f - Mathf.SmoothStep(0f, 1f, normalized);
                text.color = color;
                yield return null;
            }

            if (floater != null)
            {
                ownedFloaters.Remove(floater);
                Destroy(floater);
            }
        }

        private void RestoreFeedbackPose()
        {
            if (feedbackCarrier != null)
            {
                feedbackCarrier.anchoredPosition = baseCarrierPosition;
                feedbackCarrier.localScale = baseCarrierScale;
            }

            if (targetImage != null)
            {
                targetImage.color = baseColor;
            }
        }

        private void DestroyOwnedFloaters()
        {
            for (int i = 0; i < ownedFloaters.Count; i++)
            {
                GameObject floater = ownedFloaters[i];
                if (floater != null)
                {
                    Destroy(floater);
                }
            }

            ownedFloaters.Clear();
        }

        private void OnDisable()
        {
            ClearTransientFeedback();
        }

        private void OnDestroy()
        {
            ClearTransientFeedback();
        }
    }
}
