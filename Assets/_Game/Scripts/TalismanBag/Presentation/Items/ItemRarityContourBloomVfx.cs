using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.Presentation.Items
{
    [DisallowMultipleComponent]
    public sealed class ItemRarityContourBloomVfx : MonoBehaviour
    {
        public const string ContourLayerName = "Contour";
        public const string PersistentHaloLayerName = "PersistentHalo";
        public const string EnvironmentSpillLayerName = "EnvironmentSpill";
        public const string OuterHaloLayerName = "OuterHalo";
        public const string InnerHaloLayerName = "InnerHalo";
        public const string BodyBloomLayerName = "BodyBloom";
        public const string TriggerLayerName = "Trigger";

        [SerializeField] private ItemRarityContourBloomProfile profile;
        [SerializeField] private Image contourImage;
        [SerializeField] private Image persistentHaloImage;
        [SerializeField] private Image environmentSpillImage;
        [SerializeField] private Image outerHaloImage;
        [SerializeField] private Image innerHaloImage;
        [SerializeField] private Image bodyBloomImage;
        [SerializeField] private Image triggerImage;
        [Header("Authorable periodic charge presentation")]
        [SerializeField] private Image periodicChargeImage;
        [SerializeField] private RectTransform periodicChargeRect;
        [SerializeField] private Color periodicChargeColor =
            new Color(0.36f, 0.96f, 0.9f, 1f);
        [SerializeField, Range(0f, 1f)]
        private float periodicChargeBaseAlpha = 0.48f;
        [SerializeField, Range(0f, 1f)]
        private float periodicChargeReadyAlpha = 0.96f;
        [SerializeField, Range(0.5f, 0.98f)]
        private float periodicChargeReadyThreshold = 0.85f;
        [SerializeField, Range(1f, 1.25f)]
        private float periodicChargeReadyScale = 1.08f;
        [SerializeField] private RectTransform persistentHaloRect;
        [SerializeField] private RectTransform environmentSpillRect;
        [SerializeField] private RectTransform outerHaloRect;
        [SerializeField] private RectTransform triggerRect;

        private ItemRarityContourBloomAppearance activeAppearance;
        private bool hasAppliedPresentation;
        private bool triggerActive;
        private float triggerStartedAt;
        private bool periodicChargeActive;

        public ItemRarityContourBloomProfile Profile => profile;
        public bool HasAppliedPresentation => hasAppliedPresentation;
        public bool IsTriggerActive => triggerActive;
        public bool PeriodicChargeActive => periodicChargeActive;

        public bool Apply(
            Sprite authoritativeArtworkSprite,
            string authoritativeRarityKey)
        {
            Clear();
            if (authoritativeArtworkSprite == null
                || profile == null
                || !profile.TryResolve(
                    authoritativeRarityKey,
                    out ItemRarityContourBloomAppearance appearance)
                || appearance == null
                || !ValidateLayerReferences())
            {
                return false;
            }

            activeAppearance = appearance;
            ApplyGraphic(
                contourImage,
                authoritativeArtworkSprite,
                appearance.ContourMaterial);
            ApplyGraphic(
                persistentHaloImage,
                authoritativeArtworkSprite,
                appearance.PersistentHaloMaterial);
            ApplyGraphic(
                environmentSpillImage,
                authoritativeArtworkSprite,
                appearance.SpillMaterial);
            ApplyGraphic(
                outerHaloImage,
                authoritativeArtworkSprite,
                appearance.OuterHaloMaterial);
            ApplyGraphic(
                innerHaloImage,
                authoritativeArtworkSprite,
                appearance.InnerGlowMaterial);
            ApplyGraphic(
                bodyBloomImage,
                authoritativeArtworkSprite,
                appearance.WholeBodyEmissionMaterial);
            ApplyGraphic(
                triggerImage,
                authoritativeArtworkSprite,
                appearance.TriggerMaterial);

            hasAppliedPresentation = true;
            triggerActive = false;
            triggerStartedAt = 0f;
            triggerImage.canvasRenderer.SetAlpha(0f);
            triggerImage.gameObject.SetActive(false);
            ClearPeriodicCharge();
            ApplyPersistentState(Time.unscaledTime);
            return true;
        }

        public void SetPeriodicChargeProgress(float normalizedProgress)
        {
            if (!hasAppliedPresentation
                || periodicChargeImage == null
                || periodicChargeRect == null)
            {
                ClearPeriodicCharge();
                return;
            }

            float progress = Mathf.Clamp01(normalizedProgress);
            float ready = Mathf.InverseLerp(
                periodicChargeReadyThreshold,
                1f,
                progress);
            Color color = periodicChargeColor;
            color.a = Mathf.Lerp(
                periodicChargeBaseAlpha,
                periodicChargeReadyAlpha,
                ready);
            periodicChargeImage.color = color;
            periodicChargeImage.fillAmount = progress;
            periodicChargeImage.enabled = true;
            periodicChargeImage.gameObject.SetActive(true);
            ApplyScale(
                periodicChargeRect,
                Mathf.Lerp(1f, periodicChargeReadyScale, ready));
            periodicChargeActive = true;
        }

        public void ClearPeriodicCharge()
        {
            periodicChargeActive = false;
            if (periodicChargeRect != null)
            {
                ApplyScale(periodicChargeRect, 1f);
            }
            if (periodicChargeImage == null)
            {
                return;
            }

            periodicChargeImage.fillAmount = 0f;
            periodicChargeImage.canvasRenderer.SetAlpha(0f);
            periodicChargeImage.enabled = false;
            periodicChargeImage.gameObject.SetActive(false);
        }

        public void PlayTrigger(float unscaledNow)
        {
            if (!hasAppliedPresentation
                || activeAppearance == null
                || triggerImage == null
                || triggerRect == null)
            {
                return;
            }

            triggerStartedAt = unscaledNow;
            triggerActive = true;
            ApplyScale(triggerRect, 1f);
            ApplyScale(outerHaloRect, 1f);
            ApplyScale(
                environmentSpillRect,
                activeAppearance.EnvironmentSpillBaseScale);
            triggerImage.canvasRenderer.SetAlpha(0f);
            triggerImage.gameObject.SetActive(true);
            EvaluateAt(unscaledNow);
        }

        public void EvaluateAt(float unscaledNow)
        {
            if (!hasAppliedPresentation || activeAppearance == null)
            {
                return;
            }

            if (!triggerActive)
            {
                ApplyPersistentState(unscaledNow);
                SetTriggerVisible(false);
                return;
            }

            float attack = activeAppearance.TriggerAttackSeconds;
            float spread = activeAppearance.TriggerSpreadSeconds;
            float settle = activeAppearance.TriggerSettleSeconds;
            float age = Mathf.Max(0f, unscaledNow - triggerStartedAt);
            if (age >= settle)
            {
                triggerActive = false;
                triggerStartedAt = 0f;
                ApplyScale(triggerRect, 1f);
                SetTriggerVisible(false);
                ApplyPersistentState(unscaledNow);
                return;
            }

            float attack01 = Mathf.Clamp01(age / attack);
            float spread01 = Mathf.Clamp01(age / spread);
            float settle01 = Mathf.InverseLerp(spread, settle, age);
            float strength;
            if (age <= attack)
            {
                strength = Smooth01(attack01);
            }
            else if (age <= spread)
            {
                strength = Mathf.Lerp(
                    1f,
                    0.72f,
                    Mathf.InverseLerp(attack, spread, age));
            }
            else
            {
                strength = Mathf.Lerp(
                    0.72f,
                    0f,
                    Smooth01(settle01));
            }

            float triggerScale = age <= spread
                ? Mathf.Lerp(1f, 1.14f, Smooth01(spread01))
                : Mathf.Lerp(
                    1.14f,
                    activeAppearance.TriggerEndScale,
                    Smooth01(settle01));
            ApplyScale(triggerRect, triggerScale);
            SetAlpha(
                triggerImage,
                activeAppearance.TriggerAlpha * strength);

            float contourAlpha = Mathf.Clamp01(
                activeAppearance.PersistentAlpha
                * (1f + 0.42f * strength));
            ApplyPersistentContour(
                contourAlpha,
                unscaledNow);
            ApplyWholeBodyTrigger(age, attack, spread, settle);
        }

        public void Clear()
        {
            hasAppliedPresentation = false;
            triggerActive = false;
            triggerStartedAt = 0f;
            activeAppearance = null;
            periodicChargeActive = false;

            ClearGraphic(contourImage);
            ClearGraphic(persistentHaloImage);
            ClearGraphic(environmentSpillImage);
            ClearGraphic(outerHaloImage);
            ClearGraphic(innerHaloImage);
            ClearGraphic(bodyBloomImage);
            ClearGraphic(triggerImage);
            ClearPeriodicCharge();
            ApplyScale(persistentHaloRect, 1f);
            ApplyScale(environmentSpillRect, 1f);
            ApplyScale(outerHaloRect, 1f);
            ApplyScale(triggerRect, 1f);
            if (triggerImage != null)
            {
                triggerImage.gameObject.SetActive(false);
            }
        }

        public bool ValidateAuthoredReferences()
        {
            if (profile == null
                || !profile.ValidateAuthoredReferences()
                || !ValidateLayerReferences())
            {
                return false;
            }

            foreach (Image image in AllImages())
            {
                if (image.raycastTarget || !image.maskable)
                {
                    return false;
                }
            }

            return periodicChargeImage != null
                   && periodicChargeRect != null
                   && periodicChargeImage.rectTransform
                    == periodicChargeRect
                   && periodicChargeImage.sprite != null
                   && periodicChargeImage.type == Image.Type.Filled
                   && periodicChargeImage.fillMethod
                    == Image.FillMethod.Radial360
                   && !periodicChargeImage.raycastTarget
                   && periodicChargeImage.maskable
                   && periodicChargeBaseAlpha > 0f
                   && periodicChargeReadyAlpha
                    >= periodicChargeBaseAlpha
                   && periodicChargeReadyThreshold > 0f
                   && periodicChargeReadyScale >= 1f;
        }

        private void Update()
        {
            EvaluateAt(Time.unscaledTime);
        }

        private void OnDisable()
        {
            Clear();
        }

        private bool ValidateLayerReferences()
        {
            Image[] images = AllImages();
            HashSet<Image> distinct = new(images);
            return distinct.Count == images.Length
                   && !distinct.Contains(null)
                   && persistentHaloRect != null
                   && environmentSpillRect != null
                   && outerHaloRect != null
                   && triggerRect != null
                   && persistentHaloImage.rectTransform
                   == persistentHaloRect
                   && environmentSpillImage.rectTransform
                   == environmentSpillRect
                   && outerHaloImage.rectTransform == outerHaloRect
                   && triggerImage.rectTransform == triggerRect;
        }

        private Image[] AllImages()
        {
            return new[]
            {
                contourImage,
                persistentHaloImage,
                environmentSpillImage,
                outerHaloImage,
                innerHaloImage,
                bodyBloomImage,
                triggerImage
            };
        }

        private void ApplyPersistentState(float unscaledNow)
        {
            ApplyPersistentContour(
                activeAppearance.PersistentAlpha,
                unscaledNow);

            float bodyBreathSin = Mathf.Sin(
                unscaledNow
                * activeAppearance.WholeBodyBreathCyclesPerSecond
                * Mathf.PI
                * 2f);
            float bodyBreathWave = 0.5f + 0.5f * bodyBreathSin;
            float bodyBreath = 1f
                + bodyBreathSin
                * activeAppearance.WholeBodyBreathAmount;
            SetAlpha(
                bodyBloomImage,
                activeAppearance.WholeBodyPersistentAlpha * bodyBreath);
            SetAlpha(
                innerHaloImage,
                activeAppearance.InnerGlowAlpha
                * Mathf.Lerp(0.88f, 1.12f, bodyBreathWave));
            SetAlpha(
                outerHaloImage,
                activeAppearance.OuterHaloAlpha
                * Mathf.Lerp(0.82f, 1.18f, bodyBreathWave));
            float outerScale = 1f
                + bodyBreathWave
                * activeAppearance.WholeBodyBreathAmount
                * 0.08f;
            ApplyScale(outerHaloRect, outerScale);
            SetAlpha(
                environmentSpillImage,
                activeAppearance.EnvironmentSpillPersistentAlpha
                * Mathf.Lerp(0.84f, 1.16f, bodyBreathWave));
            float spillScale = activeAppearance.EnvironmentSpillBaseScale
                * (1f
                   + bodyBreathWave
                   * activeAppearance.WholeBodyBreathAmount
                   * 0.06f);
            ApplyScale(environmentSpillRect, spillScale);
        }

        private void ApplyPersistentContour(
            float alpha,
            float unscaledNow)
        {
            float stableAlpha = Mathf.Clamp01(alpha);
            float breathSin = Mathf.Sin(
                unscaledNow
                * activeAppearance.BreathCyclesPerSecond
                * Mathf.PI
                * 2f);
            float breathWave = 0.5f + 0.5f * breathSin;
            float coreBreath = 1f
                + breathSin * activeAppearance.BreathAmount;
            SetAlpha(contourImage, stableAlpha * coreBreath);
            SetAlpha(
                persistentHaloImage,
                stableAlpha
                * activeAppearance.PersistentHaloAlpha
                * Mathf.Lerp(0.82f, 1.18f, breathWave));
            float haloScale = activeAppearance.PersistentHaloScale
                * (1f
                   + breathWave
                   * activeAppearance.BreathAmount
                   * 0.04f);
            ApplyScale(persistentHaloRect, haloScale);
        }

        private void ApplyWholeBodyTrigger(
            float age,
            float attack,
            float spread,
            float settle)
        {
            float bodyBase = activeAppearance.WholeBodyPersistentAlpha;
            float innerBase = activeAppearance.InnerGlowAlpha;
            float outerBase = activeAppearance.OuterHaloAlpha;
            float spillBase =
                activeAppearance.EnvironmentSpillPersistentAlpha;
            float bodyPeak = activeAppearance.WholeBodyTriggerAlpha;
            float innerPeak = Mathf.Clamp01(innerBase * 2.55f);
            float outerPeak = Mathf.Clamp01(outerBase * 3.2f);
            float bodyAlpha;
            float innerAlpha;
            float outerAlpha;
            float outerScale;
            float spillAlpha;
            float spillScale;

            if (age <= attack)
            {
                float ignition = Smooth01(age / attack);
                bodyAlpha = Mathf.Lerp(bodyBase, bodyPeak, ignition);
                innerAlpha = Mathf.Lerp(innerBase, innerPeak, ignition);
                outerAlpha = Mathf.Lerp(
                    outerBase,
                    outerPeak * 0.55f,
                    ignition);
                outerScale = 1f;
                spillAlpha = Mathf.Lerp(
                    spillBase,
                    activeAppearance.EnvironmentSpillTriggerAlpha
                    * 0.62f,
                    ignition);
                spillScale =
                    activeAppearance.EnvironmentSpillBaseScale;
            }
            else if (age <= spread)
            {
                float expansion = Smooth01(
                    Mathf.InverseLerp(attack, spread, age));
                bodyAlpha = Mathf.Lerp(
                    bodyPeak,
                    bodyPeak * 0.82f,
                    expansion);
                innerAlpha = Mathf.Lerp(
                    innerPeak,
                    innerPeak * 0.78f,
                    expansion);
                outerAlpha = Mathf.Lerp(
                    outerPeak * 0.55f,
                    outerPeak,
                    expansion);
                outerScale = Mathf.Lerp(
                    1f,
                    activeAppearance.WholeBodyTriggerEndScale,
                    expansion);
                spillAlpha = Mathf.Lerp(
                    activeAppearance.EnvironmentSpillTriggerAlpha
                    * 0.62f,
                    activeAppearance.EnvironmentSpillTriggerAlpha,
                    expansion);
                spillScale = Mathf.Lerp(
                    activeAppearance.EnvironmentSpillBaseScale,
                    activeAppearance.EnvironmentSpillTriggerScale,
                    expansion);
            }
            else
            {
                float afterglow = Smooth01(
                    Mathf.InverseLerp(spread, settle, age));
                bodyAlpha = Mathf.Lerp(
                    bodyPeak * 0.82f,
                    bodyBase,
                    afterglow);
                innerAlpha = Mathf.Lerp(
                    innerPeak * 0.78f,
                    innerBase,
                    afterglow);
                outerAlpha = Mathf.Lerp(
                    outerPeak,
                    outerBase,
                    afterglow);
                outerScale = Mathf.Lerp(
                    activeAppearance.WholeBodyTriggerEndScale,
                    1f,
                    afterglow);
                spillAlpha = Mathf.Lerp(
                    activeAppearance.EnvironmentSpillTriggerAlpha,
                    spillBase,
                    afterglow);
                spillScale = Mathf.Lerp(
                    activeAppearance.EnvironmentSpillTriggerScale,
                    activeAppearance.EnvironmentSpillBaseScale,
                    afterglow);
            }

            SetAlpha(bodyBloomImage, bodyAlpha);
            SetAlpha(innerHaloImage, innerAlpha);
            SetAlpha(outerHaloImage, outerAlpha);
            ApplyScale(outerHaloRect, outerScale);
            SetAlpha(environmentSpillImage, spillAlpha);
            ApplyScale(environmentSpillRect, spillScale);
        }

        private void SetTriggerVisible(bool visible)
        {
            if (triggerImage == null)
            {
                return;
            }

            triggerImage.canvasRenderer.SetAlpha(0f);
            if (triggerImage.gameObject.activeSelf != visible)
            {
                triggerImage.gameObject.SetActive(visible);
            }
        }

        private static void ApplyGraphic(
            Image image,
            Sprite sprite,
            Material sharedMaterial)
        {
            image.overrideSprite = null;
            image.sprite = sprite;
            image.material = sharedMaterial;
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            image.fillCenter = true;
            image.color = Color.white;
            image.raycastTarget = false;
            image.maskable = true;
            image.enabled = true;
        }

        private static void ClearGraphic(Image image)
        {
            if (image == null)
            {
                return;
            }

            image.canvasRenderer.SetAlpha(0f);
            image.overrideSprite = null;
            image.sprite = null;
            image.material = null;
            image.enabled = false;
        }

        private static void SetAlpha(Graphic image, float alpha)
        {
            if (image != null)
            {
                image.canvasRenderer.SetAlpha(Mathf.Clamp01(alpha));
            }
        }

        private static void ApplyScale(RectTransform rect, float scale)
        {
            if (rect != null)
            {
                rect.localScale = new Vector3(scale, scale, 1f);
            }
        }

#if UNITY_EDITOR
        public void AssignPeriodicChargeForEditor(
            Image configuredPeriodicChargeImage,
            RectTransform configuredPeriodicChargeRect)
        {
            periodicChargeImage = configuredPeriodicChargeImage;
            periodicChargeRect = configuredPeriodicChargeRect;
        }
#endif

        private static float Smooth01(float value)
        {
            float t = Mathf.Clamp01(value);
            return t * t * (3f - 2f * t);
        }
    }
}
