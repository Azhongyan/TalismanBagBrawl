using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.Presentation.FormalBattle
{
    [DisallowMultipleComponent]
    public sealed class FormalBattleCausalSourceVisualView : MonoBehaviour
    {
        [Serializable]
        private sealed class AuthoredSourceCarrier
        {
            [SerializeField] private RectTransform proxyRect;
            [SerializeField] private CanvasGroup proxyGroup;
            [SerializeField] private RawImage proxyHalo;
            [SerializeField] private Image proxyArtwork;

            private RectTransform visibleSource;
            private FormalBattleCausalItemStyle style;
            private string grammarKey = string.Empty;
            private string acceptedEventId = string.Empty;
            private float startedAt = -100f;
            private float duration = 0.1f;
            private bool accepted;
            private bool active;
            private bool proxyVisible;

            public bool Active => active;
            public bool Accepted => accepted;
            public string GrammarKey => grammarKey;
            public string AcceptedEventId => acceptedEventId;
            public float StartedAt => startedAt;

            public bool Validate()
            {
                return proxyRect != null
                       && proxyGroup != null
                       && proxyHalo != null
                       && proxyArtwork != null
                       && proxyHalo.texture != null
                       && !proxyHalo.raycastTarget
                       && !proxyArtwork.raycastTarget
                       && proxyArtwork.preserveAspect
                       && !proxyGroup.blocksRaycasts
                       && !proxyGroup.interactable;
            }

            public bool MatchesAcceptedEvent(string eventId)
            {
                return active
                       && accepted
                       && !string.IsNullOrEmpty(eventId)
                       && string.Equals(
                           acceptedEventId,
                           eventId,
                           StringComparison.Ordinal);
            }

            public bool IsRecentAcceptedSource(
                string exactGrammarKey,
                float now)
            {
                return active
                       && accepted
                       && now - startedAt < duration
                       && string.Equals(
                           grammarKey,
                           exactGrammarKey,
                           StringComparison.Ordinal);
            }

            public void Show(
                string exactGrammarKey,
                string eventId,
                RectTransform source,
                FormalBattleCausalItemStyle configuredStyle,
                Sprite resolvedArtwork,
                bool isAccepted,
                float configuredDuration,
                float now)
            {
                active = true;
                accepted = isAccepted;
                grammarKey = exactGrammarKey ?? string.Empty;
                acceptedEventId = eventId ?? string.Empty;
                visibleSource = source != null && source.gameObject.activeInHierarchy
                    ? source
                    : null;
                style = configuredStyle;
                startedAt = now;
                duration = Mathf.Max(0.1f, configuredDuration);
                proxyArtwork.sprite = resolvedArtwork;
                proxyArtwork.color = Color.white;
                proxyHalo.color = style == null
                    ? Color.clear
                    : style.ProxyHaloColor;
                SetProxyVisible(visibleSource == null);
            }

            public RectTransform ResolveCurrentAnchor()
            {
                if (visibleSource != null
                    && visibleSource.gameObject.activeInHierarchy)
                {
                    SetProxyVisible(false);
                    return visibleSource;
                }

                visibleSource = null;
                SetProxyVisible(true);
                return proxyRect;
            }

            public void Tick(float now)
            {
                if (!active)
                {
                    return;
                }

                float progress = Mathf.Clamp01((now - startedAt) / duration);
                if (proxyVisible && proxyGroup != null && style != null)
                {
                    float cadence = style.SourcePulseCadence;
                    float pulse = 0.56f + 0.44f * Mathf.Sin(
                        progress * Mathf.PI * 2f * cadence);
                    float fade = progress < 0.7f
                        ? 1f
                        : 1f - Mathf.InverseLerp(0.7f, 1f, progress);
                    proxyGroup.alpha = Mathf.Clamp01(fade);
                    Color halo = style.ProxyHaloColor;
                    halo.a *= Mathf.Lerp(0.52f, 1f, pulse);
                    proxyHalo.color = halo;
                }

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
                accepted = false;
                visibleSource = null;
                style = null;
                grammarKey = string.Empty;
                acceptedEventId = string.Empty;
                startedAt = -100f;
                duration = 0.1f;
                proxyVisible = false;
                if (proxyGroup != null)
                {
                    proxyGroup.alpha = 0f;
                    proxyGroup.blocksRaycasts = false;
                    proxyGroup.interactable = false;
                }
                if (proxyArtwork != null)
                {
                    proxyArtwork.sprite = null;
                }
                if (proxyHalo != null)
                {
                    Color color = proxyHalo.color;
                    color.a = 0f;
                    proxyHalo.color = color;
                }
            }

            private void SetProxyVisible(bool value)
            {
                proxyVisible = value;
                if (proxyGroup != null)
                {
                    proxyGroup.alpha = value ? 1f : 0f;
                    proxyGroup.blocksRaycasts = false;
                    proxyGroup.interactable = false;
                }
            }

#if UNITY_EDITOR
            public void AssignForEditor(
                RectTransform configuredProxyRect,
                CanvasGroup configuredProxyGroup,
                RawImage configuredProxyHalo,
                Image configuredProxyArtwork)
            {
                proxyRect = configuredProxyRect;
                proxyGroup = configuredProxyGroup;
                proxyHalo = configuredProxyHalo;
                proxyArtwork = configuredProxyArtwork;
            }
#endif
        }

        [SerializeField] private FormalBattlePresentationProfile profile;
        [SerializeField] private AuthoredSourceCarrier[] authoredCarriers =
            Array.Empty<AuthoredSourceCarrier>();

        private int replacementCursor;
        private int acceptedVisualCount;
        private bool paused;
        private float pausedAt;

        public int ActiveCarrierCount => authoredCarriers == null
            ? 0
            : authoredCarriers.Count(value => value != null && value.Active);
        public int AcceptedVisualCount => acceptedVisualCount;

        private void Update()
        {
            if (paused || authoredCarriers == null)
            {
                return;
            }

            float now = Time.unscaledTime;
            foreach (AuthoredSourceCarrier carrier in authoredCarriers)
            {
                carrier?.Tick(now);
            }
        }

        private void OnDisable()
        {
            Clear();
        }

        public bool ShowScheduled(
            string exactGrammarKey,
            RectTransform visibleCard,
            FormalBattleCausalItemStyle resolvedStyle,
            Sprite resolvedArtwork,
            out RectTransform sourceAnchor)
        {
            sourceAnchor = null;
            if (!ValidateResolvedSource(
                    exactGrammarKey,
                    resolvedStyle,
                    resolvedArtwork))
            {
                return false;
            }

            // The exact placed-and-lit Board entry is the authoritative source.
            // Authored carriers are only a fallback when that real source is not
            // currently visible; they must not cap live Item presentation.
            if (visibleCard != null
                && visibleCard.gameObject.activeInHierarchy)
            {
                sourceAnchor = visibleCard;
                return true;
            }

            float now = Time.unscaledTime;
            if (authoredCarriers != null
                && authoredCarriers.Any(value => value != null
                    && value.IsRecentAcceptedSource(
                        exactGrammarKey,
                        now)))
            {
                return false;
            }

            AuthoredSourceCarrier carrier = AcquireCarrier(false);
            if (carrier == null)
            {
                return false;
            }
            carrier.Show(
                exactGrammarKey,
                string.Empty,
                visibleCard,
                resolvedStyle,
                resolvedArtwork,
                false,
                profile.CausalWindupDuration,
                now);
            sourceAnchor = carrier.ResolveCurrentAnchor();
            return sourceAnchor != null;
        }

        public bool BeginAccepted(
            string acceptedEventId,
            string exactGrammarKey,
            RectTransform visibleCard,
            FormalBattleCausalItemStyle resolvedStyle,
            Sprite resolvedArtwork,
            out RectTransform sourceAnchor)
        {
            sourceAnchor = null;
            if (string.IsNullOrWhiteSpace(acceptedEventId)
                || !ValidateResolvedSource(
                    exactGrammarKey,
                    resolvedStyle,
                    resolvedArtwork))
            {
                return false;
            }

            AuthoredSourceCarrier existing = authoredCarriers
                ?.FirstOrDefault(value => value != null
                    && value.MatchesAcceptedEvent(acceptedEventId));
            if (existing != null)
            {
                sourceAnchor = existing.ResolveCurrentAnchor();
                return sourceAnchor != null;
            }

            AuthoredSourceCarrier carrier = AcquireCarrier(true);
            if (carrier == null)
            {
                return false;
            }
            carrier.Show(
                exactGrammarKey,
                acceptedEventId,
                visibleCard,
                resolvedStyle,
                resolvedArtwork,
                true,
                profile.CausalSourceHoldDuration,
                Time.unscaledTime);
            acceptedVisualCount++;
            sourceAnchor = carrier.ResolveCurrentAnchor();
            return sourceAnchor != null;
        }

        public bool TryResolveAcceptedSource(
            string acceptedEventId,
            out RectTransform sourceAnchor,
            out string exactGrammarKey)
        {
            sourceAnchor = null;
            exactGrammarKey = string.Empty;
            if (string.IsNullOrWhiteSpace(acceptedEventId)
                || authoredCarriers == null)
            {
                return false;
            }

            AuthoredSourceCarrier carrier = authoredCarriers
                .FirstOrDefault(value => value != null
                    && value.MatchesAcceptedEvent(acceptedEventId));
            if (carrier == null)
            {
                return false;
            }

            sourceAnchor = carrier.ResolveCurrentAnchor();
            exactGrammarKey = carrier.GrammarKey;
            return sourceAnchor != null
                   && !string.IsNullOrEmpty(exactGrammarKey);
        }

        public void CancelAcceptedEvent(string acceptedEventId)
        {
            if (string.IsNullOrWhiteSpace(acceptedEventId)
                || authoredCarriers == null)
            {
                return;
            }

            foreach (AuthoredSourceCarrier carrier in authoredCarriers)
            {
                if (carrier != null
                    && carrier.MatchesAcceptedEvent(acceptedEventId))
                {
                    carrier.Clear();
                }
            }
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
                return;
            }

            float shift = Mathf.Max(0f, Time.unscaledTime - pausedAt);
            if (authoredCarriers == null)
            {
                return;
            }
            foreach (AuthoredSourceCarrier carrier in authoredCarriers)
            {
                carrier?.ShiftClock(shift);
            }
        }

        public void Clear()
        {
            paused = false;
            pausedAt = 0f;
            replacementCursor = 0;
            acceptedVisualCount = 0;
            if (authoredCarriers == null)
            {
                return;
            }
            foreach (AuthoredSourceCarrier carrier in authoredCarriers)
            {
                carrier?.Clear();
            }
        }

        public bool ValidateAuthoredReferences()
        {
            return profile != null
                   && profile.ValidateAuthoredReferences()
                   && authoredCarriers != null
                   && authoredCarriers.Length == 4
                   && authoredCarriers.All(value => value != null
                       && value.Validate());
        }

        private bool ValidateResolvedSource(
            string exactGrammarKey,
            FormalBattleCausalItemStyle resolvedStyle,
            Sprite resolvedArtwork)
        {
            return profile != null
                   && !string.IsNullOrWhiteSpace(exactGrammarKey)
                   && resolvedStyle != null
                   && resolvedStyle.Validate()
                   && resolvedArtwork != null;
        }

        private AuthoredSourceCarrier AcquireCarrier(
            bool mayReplaceAccepted)
        {
            if (authoredCarriers == null || authoredCarriers.Length == 0)
            {
                return null;
            }

            for (int offset = 0; offset < authoredCarriers.Length; offset++)
            {
                int index = (replacementCursor + offset)
                            % authoredCarriers.Length;
                AuthoredSourceCarrier candidate = authoredCarriers[index];
                if (candidate != null && !candidate.Active)
                {
                    replacementCursor = (index + 1)
                                        % authoredCarriers.Length;
                    return candidate;
                }
            }

            for (int offset = 0; offset < authoredCarriers.Length; offset++)
            {
                int index = (replacementCursor + offset)
                            % authoredCarriers.Length;
                AuthoredSourceCarrier candidate = authoredCarriers[index];
                if (candidate != null && !candidate.Accepted)
                {
                    replacementCursor = (index + 1)
                                        % authoredCarriers.Length;
                    candidate.Clear();
                    return candidate;
                }
            }

            if (!mayReplaceAccepted)
            {
                return null;
            }

            AuthoredSourceCarrier replacement =
                authoredCarriers[replacementCursor];
            replacementCursor = (replacementCursor + 1)
                                % authoredCarriers.Length;
            replacement?.Clear();
            return replacement;
        }

#if UNITY_EDITOR
        public void AssignForEditor(
            FormalBattlePresentationProfile configuredProfile,
            RectTransform[] configuredProxyRects,
            CanvasGroup[] configuredProxyGroups,
            RawImage[] configuredProxyHalos,
            Image[] configuredProxyArtworks)
        {
            profile = configuredProfile;
            int count = Mathf.Min(
                configuredProxyRects == null
                    ? 0
                    : configuredProxyRects.Length,
                Mathf.Min(
                    configuredProxyGroups == null
                        ? 0
                        : configuredProxyGroups.Length,
                    Mathf.Min(
                        configuredProxyHalos == null
                            ? 0
                            : configuredProxyHalos.Length,
                        configuredProxyArtworks == null
                            ? 0
                            : configuredProxyArtworks.Length)));
            authoredCarriers = new AuthoredSourceCarrier[count];
            for (int index = 0; index < count; index++)
            {
                authoredCarriers[index] = new AuthoredSourceCarrier();
                authoredCarriers[index].AssignForEditor(
                    configuredProxyRects[index],
                    configuredProxyGroups[index],
                    configuredProxyHalos[index],
                    configuredProxyArtworks[index]);
            }
        }
#endif
    }
}
