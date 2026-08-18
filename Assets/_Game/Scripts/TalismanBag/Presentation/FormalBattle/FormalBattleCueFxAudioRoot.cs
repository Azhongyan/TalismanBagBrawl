using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.Presentation.FormalBattle
{
    [DisallowMultipleComponent]
    public sealed class FormalBattleCueFxAudioRoot : MonoBehaviour
    {
        [SerializeField] private RectTransform effectRect;
        [SerializeField] private RectTransform sourcePulseRect;
        [SerializeField] private RawImage sourcePulseImage;
        [SerializeField] private RectTransform impactRect;
        [SerializeField] private RawImage impactImage;
        [SerializeField] private FormalBattleCausalRibbonGraphic[] causalRibbons =
            Array.Empty<FormalBattleCausalRibbonGraphic>();
        [SerializeField] private AudioSource sourceAudioSource;
        [SerializeField] private AudioSource enemyAudioSource;
        [SerializeField] private AudioSource impactAudioSource;
        [SerializeField] private FormalBattlePresentationProfile profile;

        private float sourceStartedAt = -100f;
        private float impactStartedAt = -100f;
        private bool paused;
        private float pausedAt;
        private float sourcePeakAlpha = 0.88f;
        private float impactPeakAlpha = 0.82f;
        private float lastAcceptedSourcePulseAt = -100f;
        private int ribbonReplacementCursor;
        private int causalRibbonPlayCount;

#if UNITY_EDITOR
        [SerializeField] private bool showAuthoringPreview;
#endif

        public int ActiveCausalRibbonCount => causalRibbons == null
            ? 0
            : causalRibbons.Count(value => value != null && value.Active);
        public int CausalRibbonPlayCount => causalRibbonPlayCount;

        private void Awake()
        {
            Clear();
        }

        private void Update()
        {
            if (paused)
            {
                return;
            }

            AnimatePulse(
                sourcePulseRect,
                sourcePulseImage,
                Time.unscaledTime - sourceStartedAt,
                0.38f,
                1.25f,
                sourcePeakAlpha);
            AnimatePulse(
                impactRect,
                impactImage,
                Time.unscaledTime - impactStartedAt,
                0.3f,
                1.42f,
                impactPeakAlpha);
        }

        public bool PlaySourcePulse(
            RectTransform source,
            bool restrainedWindup,
            FormalBattleCausalItemStyle resolvedStyle)
        {
            if (profile == null
                || resolvedStyle == null
                || !resolvedStyle.Validate()
                || !MoveToReceiver(sourcePulseRect, source))
            {
                return false;
            }
            if (restrainedWindup
                && Time.unscaledTime - lastAcceptedSourcePulseAt < 0.34f)
            {
                return true;
            }
            sourceStartedAt = Time.unscaledTime;
            if (sourcePulseImage != null)
            {
                Color color = resolvedStyle.SourcePulseColor;
                sourcePeakAlpha = color.a
                    * (restrainedWindup ? 0.58f : 1f);
                color.a = sourcePeakAlpha;
                sourcePulseImage.color = color;
            }
            if (!restrainedWindup)
            {
                lastAcceptedSourcePulseAt = Time.unscaledTime;
                PlayOptional(sourceAudioSource,
                    profile == null ? null : profile.SourcePulseClip);
            }
            return true;
        }

        public bool PlayCausalBridge(
            string acceptedEventId,
            string targetKey,
            FormalBattleCausalItemStyle resolvedStyle,
            RectTransform source,
            RectTransform receiver)
        {
            if (profile == null
                || string.IsNullOrWhiteSpace(acceptedEventId)
                || string.IsNullOrWhiteSpace(targetKey)
                || source == null
                || receiver == null
                || causalRibbons == null
                || causalRibbons.Length == 0
                || resolvedStyle == null
                || !resolvedStyle.Validate()
                || !TryGetLocalPoint(source, out Vector2 sourcePoint)
                || !TryGetLocalPoint(receiver, out Vector2 targetPoint))
            {
                return false;
            }

            FormalBattleCausalRibbonGraphic existing = causalRibbons
                .FirstOrDefault(value => value != null
                    && value.Matches(acceptedEventId, targetKey));
            if (existing != null)
            {
                return true;
            }

            FormalBattleCausalRibbonGraphic ribbon = AcquireRibbon();
            if (ribbon == null)
            {
                return false;
            }
            ribbon.Play(
                acceptedEventId,
                targetKey,
                sourcePoint,
                targetPoint,
                resolvedStyle,
                profile.CausalRibbonDuration);
            causalRibbonPlayCount++;
            return true;
        }

        public bool PlayImpact(RectTransform receiver)
        {
            if (!MoveToReceiver(impactRect, receiver))
            {
                return false;
            }
            impactStartedAt = Time.unscaledTime;
            impactPeakAlpha = 0.82f;
            if (impactImage != null)
            {
                Color color = impactImage.color;
                color.a = impactPeakAlpha;
                impactImage.color = color;
            }
            PlayOptional(impactAudioSource,
                profile == null ? null : profile.ImpactClip);
            return true;
        }

        public void PlayEnemyAttackAudio()
        {
            PlayOptional(enemyAudioSource,
                profile == null ? null : profile.EnemyAttackClip);
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
                PauseOptional(sourceAudioSource);
                PauseOptional(enemyAudioSource);
                PauseOptional(impactAudioSource);
                if (causalRibbons != null)
                {
                    foreach (FormalBattleCausalRibbonGraphic ribbon
                             in causalRibbons)
                    {
                        ribbon?.SetPaused(true);
                    }
                }
                return;
            }

            float shift = Mathf.Max(0f, Time.unscaledTime - pausedAt);
            sourceStartedAt += shift;
            impactStartedAt += shift;
            UnPauseOptional(sourceAudioSource);
            UnPauseOptional(enemyAudioSource);
            UnPauseOptional(impactAudioSource);
            if (causalRibbons == null)
            {
                return;
            }
            foreach (FormalBattleCausalRibbonGraphic ribbon in causalRibbons)
            {
                ribbon?.SetPaused(false);
            }
        }

        public void Clear()
        {
            paused = false;
            pausedAt = 0f;
            sourceStartedAt = -100f;
            impactStartedAt = -100f;
            lastAcceptedSourcePulseAt = -100f;
            ribbonReplacementCursor = 0;
            causalRibbonPlayCount = 0;
            ResetImage(sourcePulseRect, sourcePulseImage);
            ResetImage(impactRect, impactImage);
            if (causalRibbons != null)
            {
                foreach (FormalBattleCausalRibbonGraphic ribbon in causalRibbons)
                {
                    ribbon?.Clear();
                }
            }
            StopOptional(sourceAudioSource);
            StopOptional(enemyAudioSource);
            StopOptional(impactAudioSource);
        }

        public bool ValidateAuthoredReferences()
        {
            return effectRect != null
                   && sourcePulseRect != null
                   && sourcePulseImage != null
                   && impactRect != null
                   && impactImage != null
                   && causalRibbons != null
                   && causalRibbons.Length == 6
                   && causalRibbons.All(value => value != null
                       && value.ValidateAuthoredReferences())
                   && sourceAudioSource != null
                   && enemyAudioSource != null
                   && impactAudioSource != null
                   && profile != null
                   && profile.ValidateAuthoredReferences()
                   && !sourcePulseImage.raycastTarget
                   && !impactImage.raycastTarget;
        }

        private bool MoveToReceiver(
            RectTransform effect,
            RectTransform receiver)
        {
            if (effect == null
                || !TryGetLocalPoint(receiver, out Vector2 localPoint))
            {
                return false;
            }
            effect.anchoredPosition = localPoint;
            return true;
        }

        private bool TryGetLocalPoint(
            RectTransform receiver,
            out Vector2 localPoint)
        {
            localPoint = Vector2.zero;
            if (effectRect == null || receiver == null)
            {
                return false;
            }
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(
                null,
                receiver.position);
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                effectRect,
                screenPoint,
                null,
                out localPoint);
        }

        private FormalBattleCausalRibbonGraphic AcquireRibbon()
        {
            if (causalRibbons == null || causalRibbons.Length == 0)
            {
                return null;
            }
            for (int offset = 0; offset < causalRibbons.Length; offset++)
            {
                int index = (ribbonReplacementCursor + offset)
                            % causalRibbons.Length;
                FormalBattleCausalRibbonGraphic candidate =
                    causalRibbons[index];
                if (candidate != null && !candidate.Active)
                {
                    ribbonReplacementCursor = (index + 1)
                                              % causalRibbons.Length;
                    return candidate;
                }
            }

            FormalBattleCausalRibbonGraphic replacement =
                causalRibbons[ribbonReplacementCursor];
            ribbonReplacementCursor = (ribbonReplacementCursor + 1)
                                      % causalRibbons.Length;
            replacement?.Clear();
            return replacement;
        }

        private static void AnimatePulse(
            RectTransform rect,
            RawImage image,
            float elapsed,
            float duration,
            float maximumScale,
            float peakAlpha = 0.88f)
        {
            if (rect == null || image == null)
            {
                return;
            }
            float progress = Mathf.Clamp01(elapsed / duration);
            float alpha = elapsed >= 0f && progress < 1f
                ? Mathf.Sin(progress * Mathf.PI)
                : 0f;
            Color color = image.color;
            color.a = Mathf.Clamp01(peakAlpha) * alpha;
            image.color = color;
            rect.localScale = Vector3.one
                * Mathf.Lerp(0.86f, maximumScale, progress);
        }

        private static void ResetImage(RectTransform rect, RawImage image)
        {
            if (rect != null)
            {
                rect.localScale = Vector3.one;
            }
            if (image != null)
            {
                Color color = image.color;
                color.a = 0f;
                image.color = color;
            }
        }

        private static void PlayOptional(AudioSource source, AudioClip clip)
        {
            if (source == null || clip == null)
            {
                return;
            }
            source.Stop();
            source.clip = clip;
            source.Play();
        }

        private static void PauseOptional(AudioSource source)
        {
            if (source != null && source.isPlaying) source.Pause();
        }

        private static void UnPauseOptional(AudioSource source)
        {
            if (source != null) source.UnPause();
        }

        private static void StopOptional(AudioSource source)
        {
            if (source == null) return;
            source.Stop();
            source.clip = null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                ApplyAuthoringPreviewForEditor();
            }
        }

        public void SetAuthoringPreviewForEditor(bool visible)
        {
            showAuthoringPreview = visible;
            ApplyAuthoringPreviewForEditor();
        }

        private void ApplyAuthoringPreviewForEditor()
        {
            if (!showAuthoringPreview)
            {
                Clear();
                return;
            }

            if (sourcePulseImage != null)
            {
                Color color = sourcePulseImage.color;
                color.r = 1f;
                color.g = 0.64f;
                color.b = 0.18f;
                color.a = 0.88f;
                sourcePulseImage.color = color;
            }
            if (impactImage != null)
            {
                Color color = impactImage.color;
                color.r = 1f;
                color.g = 0.22f;
                color.b = 0.06f;
                color.a = 0.82f;
                impactImage.color = color;
            }

            if (causalRibbons == null)
            {
                return;
            }

            Vector2 start = new Vector2(-560f, -260f);
            for (int index = 0; index < causalRibbons.Length; index++)
            {
                FormalBattleCausalRibbonGraphic ribbon = causalRibbons[index];
                if (ribbon == null)
                {
                    continue;
                }

                float lane = index - (causalRibbons.Length - 1) * 0.5f;
                ribbon.ShowAuthoringPreviewForEditor(
                    index,
                    start + new Vector2(0f, lane * 34f),
                    new Vector2(360f, lane * 96f));
            }
        }

        public void AssignForEditor(
            RectTransform configuredEffectRect,
            RectTransform configuredSourcePulseRect,
            RawImage configuredSourcePulseImage,
            RectTransform configuredImpactRect,
            RawImage configuredImpactImage,
            FormalBattleCausalRibbonGraphic[] configuredCausalRibbons,
            AudioSource configuredSourceAudioSource,
            AudioSource configuredEnemyAudioSource,
            AudioSource configuredImpactAudioSource,
            FormalBattlePresentationProfile configuredProfile)
        {
            effectRect = configuredEffectRect;
            sourcePulseRect = configuredSourcePulseRect;
            sourcePulseImage = configuredSourcePulseImage;
            impactRect = configuredImpactRect;
            impactImage = configuredImpactImage;
            causalRibbons = configuredCausalRibbons
                ?? Array.Empty<FormalBattleCausalRibbonGraphic>();
            sourceAudioSource = configuredSourceAudioSource;
            enemyAudioSource = configuredEnemyAudioSource;
            impactAudioSource = configuredImpactAudioSource;
            profile = configuredProfile;
        }
#endif
    }
}
