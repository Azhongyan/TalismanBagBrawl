using System;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.V03.MainHome
{
    /// <summary>
    /// Pure presentation for the MainHome shop. It does not own navigation,
    /// progression, rewards, save data, or any gameplay state.
    /// </summary>
    public sealed class MainHomeVisualStage : MonoBehaviour
    {
        [Serializable]
        private struct CharacterStateTiming
        {
            [Min(0f)] public float holdStart;
            [Min(0f)] public float holdEnd;
            [Min(0f)] public float fadeInStart;
            [Min(0f)] public float fadeInEnd;
            [Min(0f)] public float fadeOutDuration;

            internal CharacterStateTiming(
                float holdStart,
                float holdEnd,
                float fadeInStart,
                float fadeInEnd,
                float fadeOutDuration)
            {
                this.holdStart = holdStart;
                this.holdEnd = holdEnd;
                this.fadeInStart = fadeInStart;
                this.fadeInEnd = fadeInEnd;
                this.fadeOutDuration = fadeOutDuration;
            }
        }

        [Serializable]
        private struct MotionSettings
        {
            public Vector2 positionAmplitude;
            [Min(0f)] public float frequency;
            public float rotationAmplitude;
            public float scaleAmplitude;
            public float phase;

            internal MotionSettings(
                Vector2 positionAmplitude,
                float frequency,
                float rotationAmplitude,
                float scaleAmplitude,
                float phase = 0f)
            {
                this.positionAmplitude = positionAmplitude;
                this.frequency = frequency;
                this.rotationAmplitude = rotationAmplitude;
                this.scaleAmplitude = scaleAmplitude;
                this.phase = phase;
            }
        }

        [Serializable]
        private struct LightingPreset
        {
            public Color tint;
            public Color dappleColor;
            public Color lanternColor;
            [Range(0f, 1f)] public float dappleAlpha;
            [Range(0f, 1f)] public float lanternPulseStrength;
            [Range(0f, 1f)] public float airborneVisibility;

            internal LightingPreset(
                Color tint,
                Color dappleColor,
                Color lanternColor,
                float dappleAlpha,
                float lanternPulseStrength,
                float airborneVisibility)
            {
                this.tint = tint;
                this.dappleColor = dappleColor;
                this.lanternColor = lanternColor;
                this.dappleAlpha = dappleAlpha;
                this.lanternPulseStrength = lanternPulseStrength;
                this.airborneVisibility = airborneVisibility;
            }

            internal static LightingPreset Lerp(LightingPreset from, LightingPreset to, float t)
            {
                return new LightingPreset(
                    Color.Lerp(from.tint, to.tint, t),
                    Color.Lerp(from.dappleColor, to.dappleColor, t),
                    Color.Lerp(from.lanternColor, to.lanternColor, t),
                    Mathf.Lerp(from.dappleAlpha, to.dappleAlpha, t),
                    Mathf.Lerp(from.lanternPulseStrength, to.lanternPulseStrength, t),
                    Mathf.Lerp(from.airborneVisibility, to.airborneVisibility, t));
            }
        }

        [Header("Character states")]
        [SerializeField] private CanvasGroup readingState;
        [SerializeField] private CanvasGroup cleaningState;
        [SerializeField] private CanvasGroup branchGazeState;

        [Header("Ambient layers")]
        [SerializeField] private RectTransform blossomBranch;
        [SerializeField] private CanvasGroup dappleGroup;
        [SerializeField] private RectTransform dappleRect;
        [SerializeField] private Image dappleImage;
        [SerializeField] private RectTransform petalsA;
        [SerializeField] private RectTransform petalsB;
        [SerializeField] private Image petalsImageA;
        [SerializeField] private Image petalsImageB;
        [SerializeField] private Image daylightTint;
        [SerializeField] private Image counterLantern;

        [Header("Character timing (Inspector authored)")]
        [SerializeField, Min(1f)] private float cycleDuration = 54f;
        [SerializeField] private CharacterStateTiming readingTiming = new(0f, 18f, 52f, 54f, 2f);
        [SerializeField] private CharacterStateTiming cleaningTiming = new(18f, 34f, 16f, 20f, 2f);
        [SerializeField] private CharacterStateTiming branchGazeTiming = new(34f, 52f, 32f, 36f, 2f);

        [Header("Character motion (relative to authored RectTransform)")]
        [SerializeField] private MotionSettings readingMotion = new(new Vector2(0f, 2.4f), 0.78f, 0f, 0.0045f);
        [SerializeField] private MotionSettings cleaningMotion = new(new Vector2(5f, 0f), 1.35f, 0.7f, 0f);
        [SerializeField] private MotionSettings branchGazeMotion = new(new Vector2(0f, 2.2f), 0.55f, 0.35f, 0f);

        [Header("Branch, dapple and petals")]
        [SerializeField] private float branchPrimaryAmplitude = 0.85f;
        [SerializeField, Min(0f)] private float branchPrimaryFrequency = 0.42f;
        [SerializeField] private float branchSecondaryAmplitude = 0.35f;
        [SerializeField, Min(0f)] private float branchSecondaryFrequency = 0.17f;
        [SerializeField] private float dappleRestingMultiplier = 0.9f;
        [SerializeField] private float dappleCanopyAmplitude = 0.075f;
        [SerializeField, Min(0f)] private float dappleCanopyFrequency = 0.13f;
        [SerializeField] private float dappleFlutterAmplitude = 0.025f;
        [SerializeField, Min(0f)] private float dappleFlutterFrequency = 0.67f;
        [SerializeField] private float dappleFlutterPhase = 1.3f;
        [SerializeField, Min(0f)] private float dappleWarmthFrequency = 0.11f;
        [SerializeField] private float dappleWarmthPhase = 0.6f;
        [SerializeField, Range(0f, 1f)] private float dappleWarmthInfluence = 0.035f;
        [SerializeField, Min(1f)] private float petalTravelDistance = 1920f;
        [SerializeField] private float petalFallSpeed = 9f;
        [SerializeField] private float petalSwayAmplitude = 10f;
        [SerializeField, Min(0f)] private float petalSwayFrequency = 0.31f;
        [SerializeField] private float secondaryPetalSwayMultiplier = -0.7f;

        [Header("Lantern pulse")]
        [SerializeField, Min(0f)] private float lanternPrimaryFrequency = 1.41f;
        [SerializeField] private float lanternPrimaryWeight = 0.34f;
        [SerializeField, Min(0f)] private float lanternSecondaryFrequency = 2.73f;
        [SerializeField] private float lanternSecondaryPhase = 0.8f;
        [SerializeField] private float lanternSecondaryWeight = 0.16f;

        [Header("Time-of-day preview and schedule")]
        [SerializeField] private bool useSystemClock = true;
        [SerializeField, Range(0f, 24f)] private float previewHour = 12f;
        [SerializeField, Min(1f)] private float clockRefreshInterval = 60f;
        [SerializeField, Range(0f, 24f)] private float dawnTransitionStarts = 5f;
        [SerializeField, Range(0f, 24f)] private float dawnReached = 7.5f;
        [SerializeField, Range(0f, 24f)] private float dayReached = 10f;
        [SerializeField, Range(0f, 24f)] private float duskTransitionStarts = 15.5f;
        [SerializeField, Range(0f, 24f)] private float duskReached = 18.5f;
        [SerializeField, Range(0f, 24f)] private float nightReached = 21f;

        [Header("Time-of-day visual presets")]
        [SerializeField] private LightingPreset nightLighting = new(
            new Color(0.08f, 0.13f, 0.28f, 0.23f),
            new Color(0.58f, 0.68f, 0.96f, 1f),
            new Color(1f, 0.70f, 0.40f, 1f), 0.018f, 0.10f, 0.18f);
        [SerializeField] private LightingPreset dawnLighting = new(
            new Color(1f, 0.62f, 0.32f, 0.11f),
            new Color(1f, 0.76f, 0.43f, 1f),
            new Color(1f, 0.80f, 0.58f, 1f), 0.10f, 0.06f, 0.52f);
        [SerializeField] private LightingPreset dayLighting = new(
            new Color(1f, 0.96f, 0.78f, 0.025f),
            new Color(1f, 0.95f, 0.73f, 1f), Color.white, 0.16f, 0.02f, 0.78f);
        [SerializeField] private LightingPreset duskLighting = new(
            new Color(1f, 0.42f, 0.22f, 0.14f),
            new Color(1f, 0.58f, 0.31f, 1f),
            new Color(1f, 0.74f, 0.45f, 1f), 0.075f, 0.08f, 0.42f);

        private RectTransform readingRect;
        private RectTransform cleaningRect;
        private RectTransform branchGazeRect;
        private Vector2 readingBase;
        private Vector2 cleaningBase;
        private Vector2 branchGazeBase;
        private Vector3 readingScaleBase;
        private Vector3 cleaningScaleBase;
        private Vector3 branchGazeScaleBase;
        private Quaternion readingRotationBase;
        private Quaternion cleaningRotationBase;
        private Quaternion branchGazeRotationBase;
        private Quaternion blossomBranchRotationBase;
        private Vector2 dappleBase;
        private Vector2 petalsBaseA;
        private Vector2 petalsBaseB;
        private float visualTime;
        private float nextClockRefresh;
        private float dappleBaseAlpha = 0.12f;
        private Color dappleLightColor = new(1f, 0.94f, 0.72f, 1f);
        private Color lanternLightColor = Color.white;
        private float lanternPulseStrength = 0.02f;
        private float airborneVisibility = 0.8f;
        private Color petalsBaseColorA = new(1f, 1f, 1f, 0.12f);
        private Color petalsBaseColorB = new(1f, 1f, 1f, 0.08f);
        private bool presentationBasesCached;

        private void Awake()
        {
            CachePresentationBases();
            DisableRaycasts();
        }

        private void OnEnable()
        {
            CachePresentationBases();
            visualTime = 0f;
            RefreshDaylightTint();
            ApplyFrame(0f);
        }

        private void Update()
        {
            visualTime += Time.unscaledDeltaTime;
            ApplyFrame(visualTime);

            if (Time.unscaledTime >= nextClockRefresh)
            {
                RefreshDaylightTint();
            }
        }

        private void CachePresentationBases()
        {
            if (presentationBasesCached)
            {
                return;
            }

            readingRect = readingState != null ? readingState.transform as RectTransform : null;
            cleaningRect = cleaningState != null ? cleaningState.transform as RectTransform : null;
            branchGazeRect = branchGazeState != null ? branchGazeState.transform as RectTransform : null;

            if (readingRect != null)
            {
                readingBase = readingRect.anchoredPosition;
                readingScaleBase = readingRect.localScale;
                readingRotationBase = readingRect.localRotation;
            }

            if (cleaningRect != null)
            {
                cleaningBase = cleaningRect.anchoredPosition;
                cleaningScaleBase = cleaningRect.localScale;
                cleaningRotationBase = cleaningRect.localRotation;
            }

            if (branchGazeRect != null)
            {
                branchGazeBase = branchGazeRect.anchoredPosition;
                branchGazeScaleBase = branchGazeRect.localScale;
                branchGazeRotationBase = branchGazeRect.localRotation;
            }

            if (blossomBranch != null)
            {
                blossomBranchRotationBase = blossomBranch.localRotation;
            }

            if (dappleRect != null)
            {
                dappleBase = dappleRect.anchoredPosition;
            }

            if (petalsA != null)
            {
                petalsBaseA = petalsA.anchoredPosition;
            }

            if (petalsB != null)
            {
                petalsBaseB = petalsB.anchoredPosition;
            }

            if (petalsImageA != null)
            {
                petalsBaseColorA = petalsImageA.color;
            }

            if (petalsImageB != null)
            {
                petalsBaseColorB = petalsImageB.color;
            }

            presentationBasesCached = true;
        }

        private void ApplyFrame(float time)
        {
            float cycle = Mathf.Repeat(time, Mathf.Max(1f, cycleDuration));
            float reading = HoldWithFade(cycle, readingTiming);
            float cleaning = HoldWithFade(cycle, cleaningTiming);
            float gaze = HoldWithFade(cycle, branchGazeTiming);

            SetAlpha(readingState, reading);
            SetAlpha(cleaningState, cleaning);
            SetAlpha(branchGazeState, gaze);

            if (readingRect != null)
            {
                ApplyMotion(readingRect, readingBase, readingScaleBase, readingRotationBase, readingMotion, time);
            }

            if (cleaningRect != null)
            {
                ApplyMotion(cleaningRect, cleaningBase, cleaningScaleBase, cleaningRotationBase, cleaningMotion, time);
            }

            if (branchGazeRect != null)
            {
                ApplyMotion(branchGazeRect, branchGazeBase, branchGazeScaleBase, branchGazeRotationBase, branchGazeMotion, time);
            }

            if (blossomBranch != null)
            {
                blossomBranch.localRotation = blossomBranchRotationBase * Quaternion.Euler(
                    0f,
                    0f,
                    Mathf.Sin(time * branchPrimaryFrequency) * branchPrimaryAmplitude +
                    Mathf.Sin(time * branchSecondaryFrequency) * branchSecondaryAmplitude);
            }

            if (dappleRect != null)
            {
                dappleRect.anchoredPosition = dappleBase;
            }

            if (dappleGroup != null)
            {
                float canopyDrift = Mathf.Sin(time * dappleCanopyFrequency) * dappleCanopyAmplitude;
                float leafFlutter = Mathf.Sin(time * dappleFlutterFrequency + dappleFlutterPhase) * dappleFlutterAmplitude;
                float lightBreath = dappleRestingMultiplier + canopyDrift + leafFlutter;
                dappleGroup.alpha = Mathf.Clamp01(dappleBaseAlpha * lightBreath);
            }

            if (dappleImage != null)
            {
                float warmthDrift = Mathf.Sin(time * dappleWarmthFrequency + dappleWarmthPhase) * 0.5f + 0.5f;
                dappleImage.color = Color.Lerp(dappleLightColor, Color.white, warmthDrift * dappleWarmthInfluence);
            }

            if (counterLantern != null)
            {
                float flameBody = Mathf.Sin(time * lanternPrimaryFrequency) * lanternPrimaryWeight;
                float flameEdge = Mathf.Sin(time * lanternSecondaryFrequency + lanternSecondaryPhase) * lanternSecondaryWeight;
                float pulse = 1f - lanternPulseStrength * 0.5f +
                              (flameBody + flameEdge) * lanternPulseStrength;
                Color color = lanternLightColor;
                color.a = Mathf.Clamp01(pulse);
                counterLantern.color = color;
            }

            float petalOffset = Mathf.Repeat(time * petalFallSpeed, Mathf.Max(1f, petalTravelDistance));
            float petalSway = Mathf.Sin(time * petalSwayFrequency) * petalSwayAmplitude;
            if (petalsA != null)
            {
                petalsA.anchoredPosition = petalsBaseA + new Vector2(petalSway, -petalOffset);
            }

            if (petalsB != null)
            {
                petalsB.anchoredPosition = petalsBaseB + new Vector2(petalSway * secondaryPetalSwayMultiplier, -petalOffset);
            }

            if (petalsImageA != null)
            {
                Color color = petalsBaseColorA;
                color.a = petalsBaseColorA.a * airborneVisibility;
                petalsImageA.color = color;
            }

            if (petalsImageB != null)
            {
                Color color = petalsBaseColorB;
                color.a = petalsBaseColorB.a * airborneVisibility;
                petalsImageB.color = color;
            }
        }

        private void RefreshDaylightTint()
        {
            nextClockRefresh = Time.unscaledTime + Mathf.Max(1f, clockRefreshInterval);
            if (daylightTint == null)
            {
                return;
            }

            float hour = previewHour;
            if (useSystemClock)
            {
                DateTime now = DateTime.Now;
                hour = now.Hour + now.Minute / 60f + now.Second / 3600f;
            }

            LightingPreset lighting = EvaluateLighting(hour);
            daylightTint.color = lighting.tint;
            dappleBaseAlpha = lighting.dappleAlpha;
            dappleLightColor = lighting.dappleColor;
            lanternLightColor = lighting.lanternColor;
            lanternPulseStrength = lighting.lanternPulseStrength;
            airborneVisibility = lighting.airborneVisibility;
        }

        private LightingPreset EvaluateLighting(float hour)
        {
            if (hour < dawnTransitionStarts)
            {
                return nightLighting;
            }

            if (hour < dawnReached)
            {
                return LightingPreset.Lerp(
                    nightLighting,
                    dawnLighting,
                    Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(dawnTransitionStarts, dawnReached, hour)));
            }

            if (hour < dayReached)
            {
                return LightingPreset.Lerp(
                    dawnLighting,
                    dayLighting,
                    Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(dawnReached, dayReached, hour)));
            }

            if (hour < duskTransitionStarts)
            {
                return dayLighting;
            }

            if (hour < duskReached)
            {
                return LightingPreset.Lerp(
                    dayLighting,
                    duskLighting,
                    Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(duskTransitionStarts, duskReached, hour)));
            }

            if (hour < nightReached)
            {
                return LightingPreset.Lerp(
                    duskLighting,
                    nightLighting,
                    Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(duskReached, nightReached, hour)));
            }

            return nightLighting;
        }

        private static void ApplyMotion(
            RectTransform rect,
            Vector2 basePosition,
            Vector3 baseScale,
            Quaternion baseRotation,
            MotionSettings settings,
            float time)
        {
            float wave = Mathf.Sin(time * settings.frequency + settings.phase);
            rect.anchoredPosition = basePosition + settings.positionAmplitude * wave;
            float scale = 1f + settings.scaleAmplitude * wave;
            rect.localScale = new Vector3(baseScale.x * scale, baseScale.y * scale, baseScale.z);
            rect.localRotation = baseRotation * Quaternion.Euler(0f, 0f, settings.rotationAmplitude * wave);
        }

        private static float HoldWithFade(float cycle, CharacterStateTiming timing)
        {
            if (timing.holdStart <= cycle && cycle <= timing.holdEnd)
            {
                return 1f;
            }

            if (timing.fadeInStart <= timing.fadeInEnd &&
                timing.fadeInStart <= cycle && cycle < timing.fadeInEnd)
            {
                return Mathf.SmoothStep(
                    0f,
                    1f,
                    Mathf.InverseLerp(timing.fadeInStart, timing.fadeInEnd, cycle));
            }

            float fadeOutDuration = Mathf.Max(0.01f, timing.fadeOutDuration);
            if (timing.holdEnd < cycle && cycle < timing.holdEnd + fadeOutDuration)
            {
                return Mathf.SmoothStep(
                    1f,
                    0f,
                    Mathf.InverseLerp(timing.holdEnd, timing.holdEnd + fadeOutDuration, cycle));
            }

            return 0f;
        }

        private static void SetAlpha(CanvasGroup group, float alpha)
        {
            if (group == null)
            {
                return;
            }

            group.alpha = Mathf.Clamp01(alpha);
            group.interactable = false;
            group.blocksRaycasts = false;
        }

        private void DisableRaycasts()
        {
            foreach (Graphic graphic in GetComponentsInChildren<Graphic>(true))
            {
                graphic.raycastTarget = false;
            }
        }

#if UNITY_EDITOR
        public void AssignForEditor(
            CanvasGroup reading,
            CanvasGroup cleaning,
            CanvasGroup branchGaze,
            RectTransform branch,
            CanvasGroup dapple,
            RectTransform dappleTransform,
            RectTransform firstPetals,
            RectTransform secondPetals,
            Image tint,
            Image dappleLight,
            Image lantern)
        {
            readingState = reading;
            cleaningState = cleaning;
            branchGazeState = branchGaze;
            blossomBranch = branch;
            dappleGroup = dapple;
            dappleRect = dappleTransform;
            petalsA = firstPetals;
            petalsB = secondPetals;
            petalsImageA = firstPetals != null ? firstPetals.GetComponent<Image>() : null;
            petalsImageB = secondPetals != null ? secondPetals.GetComponent<Image>() : null;
            daylightTint = tint;
            dappleImage = dappleLight;
            counterLantern = lantern;
            presentationBasesCached = false;
            CachePresentationBases();
            DisableRaycasts();
        }
#endif
    }
}
