using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.Presentation.StageThemes
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image))]
    public sealed class C1StageThemeBackgroundPresenter : MonoBehaviour
    {
        private enum PlaybackPhase
        {
            Stopped,
            Frames,
            HoldFrameOne
        }

        [SerializeField] private Image targetImage;
        [SerializeField] private Sprite authoredSafeSprite;

        private C1StageThemeBackgroundProfile activeProfile;
        private PlaybackPhase playbackPhase;
        private int currentFrameIndex = -1;
        private float phaseElapsedSeconds;
        private string lastFailureDiagnostic = string.Empty;

        public Image TargetImage => targetImage;
        public Sprite AuthoredSafeSprite => authoredSafeSprite;
        public C1StageThemeBackgroundProfile ActiveProfile => activeProfile;
        public bool IsPlaying => playbackPhase != PlaybackPhase.Stopped;
        public int CurrentFrameIndex => currentFrameIndex;

        private void Awake()
        {
            ResetPresentation();
        }

        private void OnEnable()
        {
            ResetPresentation();
        }

        private void Update()
        {
            if (playbackPhase == PlaybackPhase.Stopped
                || activeProfile == null)
            {
                return;
            }

            float deltaSeconds = Time.unscaledDeltaTime;
            if (deltaSeconds <= 0f)
            {
                return;
            }

            phaseElapsedSeconds += deltaSeconds;
            AdvancePlayback();
        }

        private void OnDisable()
        {
            ResetPresentation();
        }

        private void OnDestroy()
        {
            StopPlayback();
        }

        public bool Apply(C1StageThemeBackgroundProfile profile)
        {
            return Apply(profile, out _);
        }

        public bool Apply(
            C1StageThemeBackgroundProfile profile,
            out string diagnostic)
        {
            StopPlayback();

            if (!ValidateAuthoredReferences(out diagnostic))
            {
                RestoreAuthoredSafeSprite();
                ReportFailureOnce(diagnostic);
                return false;
            }

            if (profile == null)
            {
                diagnostic = "STAGE_THEME_BACKGROUND_PROFILE_MISSING";
                RestoreAuthoredSafeSprite();
                ReportFailureOnce(diagnostic);
                return false;
            }

            if (!profile.TryValidate(out diagnostic))
            {
                RestoreAuthoredSafeSprite();
                ReportFailureOnce(diagnostic);
                return false;
            }

            activeProfile = profile;
            lastFailureDiagnostic = string.Empty;

            if (profile.Mode == C1StageThemeBackgroundMode.StaticSprite)
            {
                targetImage.sprite = profile.StaticSprite;
                diagnostic = "STAGE_THEME_BACKGROUND_STATIC_APPLIED";
                return true;
            }

            currentFrameIndex = 0;
            phaseElapsedSeconds = 0f;
            playbackPhase = PlaybackPhase.Frames;
            targetImage.sprite = profile.SequenceFrames[0];
            diagnostic = "STAGE_THEME_BACKGROUND_SEQUENCE_APPLIED";
            return true;
        }

        public void Clear()
        {
            StopPlayback();
            if (targetImage != null)
            {
                targetImage.sprite = null;
            }

            lastFailureDiagnostic = string.Empty;
        }

        public void ResetPresentation()
        {
            StopPlayback();
            RestoreAuthoredSafeSprite();
            lastFailureDiagnostic = string.Empty;
        }

        public bool ValidateAuthoredReferences(out string diagnostic)
        {
            if (targetImage == null)
            {
                diagnostic =
                    "STAGE_THEME_BACKGROUND_AUTHORED_IMAGE_MISSING";
                return false;
            }

            if (targetImage.gameObject != gameObject)
            {
                diagnostic =
                    "STAGE_THEME_BACKGROUND_IMAGE_NOT_LOCAL";
                return false;
            }

            if (!(targetImage.transform is RectTransform))
            {
                diagnostic =
                    "STAGE_THEME_BACKGROUND_RECT_TRANSFORM_MISSING";
                return false;
            }

            if (targetImage.raycastTarget)
            {
                diagnostic =
                    "STAGE_THEME_BACKGROUND_IMAGE_INTERCEPTS_INPUT";
                return false;
            }

            if (authoredSafeSprite == null)
            {
                diagnostic =
                    "STAGE_THEME_BACKGROUND_AUTHORED_SAFE_SPRITE_MISSING";
                return false;
            }

            diagnostic = "STAGE_THEME_BACKGROUND_AUTHORED_REFERENCES_VALID";
            return true;
        }

        public void AssignForEditor(
            Image configuredTargetImage,
            Sprite configuredAuthoredSafeSprite)
        {
            targetImage = configuredTargetImage;
            authoredSafeSprite = configuredAuthoredSafeSprite;
        }

        private void AdvancePlayback()
        {
            Sprite[] frames = activeProfile.SequenceFrames;
            float frameDurationSeconds = 1f / activeProfile.FramesPerSecond;
            float holdSeconds = activeProfile.LoopFrameOneHoldSeconds;

            // A bounded loop handles frame hitches without allocating a coroutine
            // or any per-loop state. Normal play advances only once per update.
            for (int transitions = 0; transitions < 4096; transitions++)
            {
                if (playbackPhase == PlaybackPhase.Frames)
                {
                    if (phaseElapsedSeconds < frameDurationSeconds)
                    {
                        return;
                    }

                    phaseElapsedSeconds -= frameDurationSeconds;
                    if (currentFrameIndex < frames.Length - 1)
                    {
                        currentFrameIndex++;
                        targetImage.sprite = frames[currentFrameIndex];
                        continue;
                    }

                    currentFrameIndex = 0;
                    targetImage.sprite = frames[0];
                    playbackPhase = PlaybackPhase.HoldFrameOne;
                    continue;
                }

                if (playbackPhase != PlaybackPhase.HoldFrameOne
                    || phaseElapsedSeconds < holdSeconds)
                {
                    return;
                }

                phaseElapsedSeconds -= holdSeconds;
                playbackPhase = PlaybackPhase.Frames;
                currentFrameIndex = 0;
            }
        }

        private void StopPlayback()
        {
            activeProfile = null;
            playbackPhase = PlaybackPhase.Stopped;
            currentFrameIndex = -1;
            phaseElapsedSeconds = 0f;
        }

        private void RestoreAuthoredSafeSprite()
        {
            if (targetImage != null)
            {
                targetImage.sprite = authoredSafeSprite;
            }
        }

        private void ReportFailureOnce(string diagnostic)
        {
            if (string.Equals(
                    lastFailureDiagnostic,
                    diagnostic,
                    System.StringComparison.Ordinal))
            {
                return;
            }

            lastFailureDiagnostic = diagnostic;
            Debug.LogError(
                "[C1StageThemeBackgroundPresenter] " + diagnostic,
                this);
        }
    }
}
