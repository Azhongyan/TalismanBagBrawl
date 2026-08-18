using System;
using UnityEngine;

namespace TalismanBag.Presentation.StageThemes
{
    public enum C1StageThemeBackgroundMode
    {
        StaticSprite = 0,
        SpriteSequence = 1
    }

    [CreateAssetMenu(
        fileName = "C1StageThemeBackgroundProfile",
        menuName = "TalismanBag/V0.4/Presentation/Stage Theme Background Profile")]
    public sealed class C1StageThemeBackgroundProfile : ScriptableObject
    {
        [SerializeField] private string profileId = string.Empty;
        [SerializeField] private C1StageThemeBackgroundMode mode;
        [SerializeField] private Sprite staticSprite;
        [SerializeField] private Sprite[] sequenceFrames = Array.Empty<Sprite>();
        [SerializeField] private float framesPerSecond = 10f;
        [SerializeField] private float loopFrameOneHoldSeconds = 5f;

        public string ProfileId => profileId;
        public C1StageThemeBackgroundMode Mode => mode;
        public Sprite StaticSprite => staticSprite;
        public Sprite[] SequenceFrames => sequenceFrames;
        public float FramesPerSecond => framesPerSecond;
        public float LoopFrameOneHoldSeconds => loopFrameOneHoldSeconds;

        public bool TryValidate(out string diagnostic)
        {
            if (string.IsNullOrWhiteSpace(profileId))
            {
                diagnostic = "STAGE_THEME_BACKGROUND_PROFILE_ID_MISSING";
                return false;
            }

            if (mode == C1StageThemeBackgroundMode.StaticSprite)
            {
                if (staticSprite == null)
                {
                    diagnostic =
                        "STAGE_THEME_BACKGROUND_STATIC_SPRITE_MISSING profile=" +
                        profileId;
                    return false;
                }

                if (sequenceFrames != null && sequenceFrames.Length > 0)
                {
                    diagnostic =
                        "STAGE_THEME_BACKGROUND_STATIC_PROFILE_HAS_SEQUENCE profile=" +
                        profileId;
                    return false;
                }

                diagnostic = "STAGE_THEME_BACKGROUND_PROFILE_VALID";
                return true;
            }

            if (mode != C1StageThemeBackgroundMode.SpriteSequence)
            {
                diagnostic =
                    "STAGE_THEME_BACKGROUND_MODE_UNSUPPORTED profile=" +
                    profileId;
                return false;
            }

            if (staticSprite != null)
            {
                diagnostic =
                    "STAGE_THEME_BACKGROUND_SEQUENCE_PROFILE_HAS_STATIC_SPRITE profile=" +
                    profileId;
                return false;
            }

            if (sequenceFrames == null || sequenceFrames.Length == 0)
            {
                diagnostic =
                    "STAGE_THEME_BACKGROUND_SEQUENCE_EMPTY profile=" +
                    profileId;
                return false;
            }

            for (int i = 0; i < sequenceFrames.Length; i++)
            {
                if (sequenceFrames[i] != null)
                {
                    continue;
                }

                diagnostic =
                    "STAGE_THEME_BACKGROUND_SEQUENCE_FRAME_MISSING profile=" +
                    profileId + " position=" + (i + 1);
                return false;
            }

            if (float.IsNaN(framesPerSecond)
                || float.IsInfinity(framesPerSecond)
                || framesPerSecond <= 0f)
            {
                diagnostic =
                    "STAGE_THEME_BACKGROUND_SEQUENCE_FPS_INVALID profile=" +
                    profileId;
                return false;
            }

            if (float.IsNaN(loopFrameOneHoldSeconds)
                || float.IsInfinity(loopFrameOneHoldSeconds)
                || loopFrameOneHoldSeconds < 0f)
            {
                diagnostic =
                    "STAGE_THEME_BACKGROUND_SEQUENCE_HOLD_INVALID profile=" +
                    profileId;
                return false;
            }

            diagnostic = "STAGE_THEME_BACKGROUND_PROFILE_VALID";
            return true;
        }

        public void ConfigureForEditor(
            string configuredProfileId,
            C1StageThemeBackgroundMode configuredMode,
            Sprite configuredStaticSprite,
            Sprite[] configuredSequenceFrames,
            float configuredFramesPerSecond,
            float configuredLoopFrameOneHoldSeconds)
        {
            profileId = string.IsNullOrWhiteSpace(configuredProfileId)
                ? string.Empty
                : configuredProfileId.Trim();
            mode = configuredMode;
            staticSprite = configuredStaticSprite;
            sequenceFrames = configuredSequenceFrames == null
                ? Array.Empty<Sprite>()
                : (Sprite[])configuredSequenceFrames.Clone();
            framesPerSecond = configuredFramesPerSecond;
            loopFrameOneHoldSeconds = configuredLoopFrameOneHoldSeconds;
        }
    }
}
