using System;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.BuildSandbox
{
    public readonly struct ItemLivingGradientOutlineSourceSnapshot
    {
        public ItemLivingGradientOutlineSourceSnapshot(Image image)
        {
            Image = image;
            Sprite = image?.sprite;
            OverrideSprite = image?.overrideSprite;
            Material = image?.material;
            Color = image == null ? Color.clear : image.color;
            RaycastTarget = image != null && image.raycastTarget;
            Enabled = image != null && image.enabled;
            ActiveSelf = image != null && image.gameObject.activeSelf;
            Rect = image == null ? null : image.rectTransform;
            Parent = Rect == null ? null : Rect.parent;
            AnchorMin = Rect == null ? Vector2.zero : Rect.anchorMin;
            AnchorMax = Rect == null ? Vector2.zero : Rect.anchorMax;
            Pivot = Rect == null ? Vector2.zero : Rect.pivot;
            AnchoredPosition =
                Rect == null ? Vector2.zero : Rect.anchoredPosition;
            SizeDelta = Rect == null ? Vector2.zero : Rect.sizeDelta;
            LocalScale = Rect == null ? Vector3.zero : Rect.localScale;
            LocalRotation =
                Rect == null ? Quaternion.identity : Rect.localRotation;
        }

        public Image Image { get; }
        public Sprite Sprite { get; }
        public Sprite OverrideSprite { get; }
        public Material Material { get; }
        public Color Color { get; }
        public bool RaycastTarget { get; }
        public bool Enabled { get; }
        public bool ActiveSelf { get; }
        public RectTransform Rect { get; }
        public Transform Parent { get; }
        public Vector2 AnchorMin { get; }
        public Vector2 AnchorMax { get; }
        public Vector2 Pivot { get; }
        public Vector2 AnchoredPosition { get; }
        public Vector2 SizeDelta { get; }
        public Vector3 LocalScale { get; }
        public Quaternion LocalRotation { get; }

        public bool IsExact()
        {
            return Image != null
                && Image.sprite == Sprite
                && Image.overrideSprite == OverrideSprite
                && Image.material == Material
                && Image.color == Color
                && Image.raycastTarget == RaycastTarget
                && Image.enabled == Enabled
                && Image.gameObject.activeSelf == ActiveSelf
                && Image.rectTransform == Rect
                && Rect.parent == Parent
                && Rect.anchorMin == AnchorMin
                && Rect.anchorMax == AnchorMax
                && Rect.pivot == Pivot
                && Rect.anchoredPosition == AnchoredPosition
                && Rect.sizeDelta == SizeDelta
                && Rect.localScale == LocalScale
                && Rect.localRotation == LocalRotation;
        }
    }

    [DisallowMultipleComponent]
    public sealed class ItemLivingGradientOutlineVfx :
        MonoBehaviour
    {
        public const string RootNamePrefix =
            "ItemLivingGradientOutlineVfx_Runtime_";
        public const string PersistentHaloLayerName =
            "ItemLivingGradientOutlinePersistentHaloLayer";
        public const string WholeBodyEmissionLayerName =
            "ItemWholeBodyGlowEmissionLayer";
        public const string WholeBodyInnerGlowLayerName =
            "ItemWholeBodyGlowInnerLayer";
        public const string WholeBodyOuterHaloLayerName =
            "ItemWholeBodyGlowOuterHaloLayer";
        public const string EnvironmentSpillLayerName =
            "ItemWholeBodyGlowEnvironmentSpillLayer";
        public const string TriggerLayerName =
            "ItemLivingGradientOutlineTriggerLayer";

        private RectTransform rootRect;
        private Image persistentImage;
        private RectTransform persistentHaloRect;
        private Image persistentHaloImage;
        private Image wholeBodyEmissionImage;
        private Image wholeBodyInnerGlowImage;
        private RectTransform wholeBodyOuterHaloRect;
        private Image wholeBodyOuterHaloImage;
        private RectTransform environmentSpillRect;
        private RawImage environmentSpillImage;
        private RectTransform triggerRect;
        private Image triggerImage;
        private Image sourceImage;
        private Sprite sourceSprite;
        private ItemLivingGradientOutlinePaletteProfile palette;
        private ItemLivingGradientOutlineBuildFamily family;
        private Transform poolParent;
        private bool persistent;
        private bool triggerActive;
        private float triggerStartedAt;
        private ItemLivingGradientOutlineSourceSnapshot
            sourceSnapshot;

        public Image SourceImage => sourceImage;
        public string ItemId { get; private set; } = string.Empty;
        public string SurfaceId { get; private set; } = string.Empty;
        public ItemLivingGradientOutlineBuildFamily Family => family;
        public bool IsBound => sourceImage != null;
        public bool IsPersistent => persistent;
        public bool IsTriggerActive => triggerActive;
        public int AddedGraphicCount => 7;
        public bool AddedGraphicsAreRaycastFree =>
            persistentImage != null
            && persistentHaloImage != null
            && wholeBodyEmissionImage != null
            && wholeBodyInnerGlowImage != null
            && wholeBodyOuterHaloImage != null
            && environmentSpillImage != null
            && triggerImage != null
            && !persistentImage.raycastTarget
            && !persistentHaloImage.raycastTarget
            && !wholeBodyEmissionImage.raycastTarget
            && !wholeBodyInnerGlowImage.raycastTarget
            && !wholeBodyOuterHaloImage.raycastTarget
            && !environmentSpillImage.raycastTarget
            && !triggerImage.raycastTarget;
        public Material PersistentSharedMaterial =>
            persistentImage == null ? null : persistentImage.material;
        public Material PersistentHaloSharedMaterial =>
            persistentHaloImage == null
                ? null
                : persistentHaloImage.material;
        public Material WholeBodyEmissionSharedMaterial =>
            wholeBodyEmissionImage == null
                ? null
                : wholeBodyEmissionImage.material;
        public Material WholeBodyInnerGlowSharedMaterial =>
            wholeBodyInnerGlowImage == null
                ? null
                : wholeBodyInnerGlowImage.material;
        public Material WholeBodyOuterGlowSharedMaterial =>
            wholeBodyOuterHaloImage == null
                ? null
                : wholeBodyOuterHaloImage.material;
        public Material EnvironmentSpillSharedMaterial =>
            environmentSpillImage == null
                ? null
                : environmentSpillImage.material;
        public Material TriggerSharedMaterial =>
            triggerImage == null ? null : triggerImage.material;
        public bool SourceStillExact => sourceSnapshot.IsExact();

        public static ItemLivingGradientOutlineVfx CreatePooled(
            Transform parent,
            int poolIndex)
        {
            GameObject root = new(
                RootNamePrefix + poolIndex.ToString("00"),
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            root.transform.SetParent(parent, false);
            RectTransform rootRect =
                root.GetComponent<RectTransform>();
            Stretch(rootRect);
            Image persistentImage = root.GetComponent<Image>();
            ConfigureEffectImage(persistentImage);

            GameObject persistentHaloLayer = new(
                PersistentHaloLayerName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            persistentHaloLayer.transform.SetParent(
                root.transform,
                false);
            RectTransform persistentHaloRect =
                persistentHaloLayer.GetComponent<RectTransform>();
            Stretch(persistentHaloRect);
            Image persistentHaloImage =
                persistentHaloLayer.GetComponent<Image>();
            ConfigureEffectImage(persistentHaloImage);

            GameObject environmentSpillLayer = new(
                EnvironmentSpillLayerName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(RawImage));
            environmentSpillLayer.transform.SetParent(
                root.transform,
                false);
            RectTransform environmentSpillRect =
                environmentSpillLayer.GetComponent<RectTransform>();
            Stretch(environmentSpillRect);
            RawImage environmentSpillImage =
                environmentSpillLayer.GetComponent<RawImage>();
            ConfigureEnvironmentSpillImage(environmentSpillImage);

            GameObject wholeBodyOuterHaloLayer = new(
                WholeBodyOuterHaloLayerName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            wholeBodyOuterHaloLayer.transform.SetParent(
                root.transform,
                false);
            RectTransform wholeBodyOuterHaloRect =
                wholeBodyOuterHaloLayer.GetComponent<RectTransform>();
            Stretch(wholeBodyOuterHaloRect);
            Image wholeBodyOuterHaloImage =
                wholeBodyOuterHaloLayer.GetComponent<Image>();
            ConfigureEffectImage(wholeBodyOuterHaloImage);

            GameObject wholeBodyInnerGlowLayer = new(
                WholeBodyInnerGlowLayerName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            wholeBodyInnerGlowLayer.transform.SetParent(
                root.transform,
                false);
            RectTransform wholeBodyInnerGlowRect =
                wholeBodyInnerGlowLayer.GetComponent<RectTransform>();
            Stretch(wholeBodyInnerGlowRect);
            Image wholeBodyInnerGlowImage =
                wholeBodyInnerGlowLayer.GetComponent<Image>();
            ConfigureEffectImage(wholeBodyInnerGlowImage);

            GameObject wholeBodyEmissionLayer = new(
                WholeBodyEmissionLayerName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            wholeBodyEmissionLayer.transform.SetParent(
                root.transform,
                false);
            RectTransform wholeBodyEmissionRect =
                wholeBodyEmissionLayer.GetComponent<RectTransform>();
            Stretch(wholeBodyEmissionRect);
            Image wholeBodyEmissionImage =
                wholeBodyEmissionLayer.GetComponent<Image>();
            ConfigureEffectImage(wholeBodyEmissionImage);

            GameObject triggerLayer = new(
                TriggerLayerName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            triggerLayer.transform.SetParent(root.transform, false);
            RectTransform triggerRect =
                triggerLayer.GetComponent<RectTransform>();
            Stretch(triggerRect);
            Image triggerImage = triggerLayer.GetComponent<Image>();
            ConfigureEffectImage(triggerImage);
            triggerLayer.SetActive(false);

            ItemLivingGradientOutlineVfx effect =
                root.AddComponent<ItemLivingGradientOutlineVfx>();
            effect.rootRect = rootRect;
            effect.persistentImage = persistentImage;
            effect.persistentHaloRect = persistentHaloRect;
            effect.persistentHaloImage = persistentHaloImage;
            effect.wholeBodyEmissionImage =
                wholeBodyEmissionImage;
            effect.wholeBodyInnerGlowImage =
                wholeBodyInnerGlowImage;
            effect.wholeBodyOuterHaloRect =
                wholeBodyOuterHaloRect;
            effect.wholeBodyOuterHaloImage =
                wholeBodyOuterHaloImage;
            effect.environmentSpillRect = environmentSpillRect;
            effect.environmentSpillImage = environmentSpillImage;
            effect.triggerRect = triggerRect;
            effect.triggerImage = triggerImage;
            effect.poolParent = parent;
            root.SetActive(false);
            return effect;
        }

        public bool Bind(
            Image authoritativeArtwork,
            string itemId,
            string surfaceId,
            ItemLivingGradientOutlineBuildFamily buildFamily,
            ItemLivingGradientOutlinePaletteProfile paletteProfile,
            ItemLivingGradientOutlineMaterialSet materials)
        {
            ReleaseToPool(poolParent);
            if (!IsValidAuthoritativeArtwork(authoritativeArtwork)
                || paletteProfile == null
                || materials == null
                || !materials.IsValid)
            {
                return false;
            }

            sourceImage = authoritativeArtwork;
            sourceSprite = ActiveSprite(sourceImage);
            ItemId = (itemId ?? string.Empty).Trim();
            SurfaceId = (surfaceId ?? string.Empty).Trim();
            family = buildFamily;
            palette = paletteProfile;
            sourceSnapshot =
                new ItemLivingGradientOutlineSourceSnapshot(sourceImage);

            rootRect.SetParent(sourceImage.rectTransform, false);
            Stretch(rootRect);
            rootRect.localScale = Vector3.one;
            rootRect.localRotation = Quaternion.identity;
            persistentImage.material = materials.Persistent;
            persistentHaloImage.material = materials.PersistentHalo;
            wholeBodyEmissionImage.material =
                materials.WholeBodyEmission;
            wholeBodyInnerGlowImage.material =
                materials.WholeBodyInnerGlow;
            wholeBodyOuterHaloImage.material =
                materials.WholeBodyOuterGlow;
            environmentSpillImage.material =
                materials.EnvironmentSpill;
            triggerImage.material = materials.Trigger;
            SyncGraphicsFromSource();
            persistent = false;
            triggerActive = false;
            triggerImage.gameObject.SetActive(false);
            persistentImage.canvasRenderer.SetAlpha(0f);
            persistentHaloImage.canvasRenderer.SetAlpha(0f);
            wholeBodyEmissionImage.canvasRenderer.SetAlpha(0f);
            wholeBodyInnerGlowImage.canvasRenderer.SetAlpha(0f);
            wholeBodyOuterHaloImage.canvasRenderer.SetAlpha(0f);
            environmentSpillImage.canvasRenderer.SetAlpha(0f);
            persistentHaloRect.localScale =
                Vector3.one * palette.persistentHaloScale;
            wholeBodyOuterHaloRect.localScale = Vector3.one;
            environmentSpillRect.localScale =
                Vector3.one * palette.environmentSpillBaseScale;
            gameObject.SetActive(false);
            return true;
        }

        public void SetPersistent(bool enabled)
        {
            persistent = enabled;
            RefreshRootVisibility();
            if (!triggerActive)
            {
                ApplyPersistentAlpha(
                    persistent && palette != null
                        ? palette.persistentAlpha
                        : 0f,
                    Time.unscaledTime);
            }
        }

        public void PlayTrigger(float unscaledNow)
        {
            if (!IsBound || palette == null)
            {
                return;
            }
            triggerStartedAt = unscaledNow;
            triggerActive = true;
            triggerRect.localScale = Vector3.one;
            wholeBodyOuterHaloRect.localScale = Vector3.one;
            environmentSpillRect.localScale =
                Vector3.one * palette.environmentSpillBaseScale;
            triggerImage.canvasRenderer.SetAlpha(0f);
            triggerImage.gameObject.SetActive(true);
            RefreshRootVisibility();
            EvaluateAt(unscaledNow);
        }

        public void EvaluateAt(float unscaledNow)
        {
            if (!IsBound)
            {
                return;
            }
            if (!triggerActive || palette == null)
            {
                ApplyPersistentAlpha(
                    persistent ? palette?.persistentAlpha ?? 0f : 0f,
                    unscaledNow);
                if (triggerImage != null)
                {
                    triggerImage.gameObject.SetActive(false);
                }
                RefreshRootVisibility();
                return;
            }

            ItemLivingGradientOutlineProfile profile =
                ItemLivingGradientOutlineVfxPrototypeController
                    .ActiveProfile;
            float attack = profile == null
                ? 0.08f
                : profile.triggerAttackSeconds;
            float spread = profile == null
                ? 0.25f
                : profile.triggerSpreadSeconds;
            float settle = profile == null
                ? 0.55f
                : profile.triggerSettleSeconds;
            float endScale = profile == null
                ? 1.22f
                : profile.triggerEndScale;
            float age = Mathf.Max(
                0f, unscaledNow - triggerStartedAt);
            if (age >= settle)
            {
                triggerActive = false;
                triggerImage.canvasRenderer.SetAlpha(0f);
                triggerImage.gameObject.SetActive(false);
                triggerRect.localScale = Vector3.one;
                ApplyPersistentAlpha(
                    persistent ? palette.persistentAlpha : 0f,
                    unscaledNow);
                RefreshRootVisibility();
                return;
            }

            float attack01 = Mathf.Clamp01(age / attack);
            float spread01 = Mathf.Clamp01(age / spread);
            float settle01 = Mathf.InverseLerp(
                spread, settle, age);
            float strength;
            if (age <= attack)
            {
                strength = Smooth01(attack01);
            }
            else if (age <= spread)
            {
                strength = Mathf.Lerp(
                    1f, 0.72f, Mathf.InverseLerp(attack, spread, age));
            }
            else
            {
                strength = Mathf.Lerp(
                    0.72f, 0f, Smooth01(settle01));
            }

            float scale = age <= spread
                ? Mathf.Lerp(1f, 1.14f, Smooth01(spread01))
                : Mathf.Lerp(1.14f, endScale, Smooth01(settle01));
            triggerRect.localScale =
                new Vector3(scale, scale, 1f);
            triggerImage.canvasRenderer.SetAlpha(
                palette.triggerAlpha * strength);
            float persistentAlpha = persistent
                ? Mathf.Clamp01(
                    palette.persistentAlpha
                    * (1f + 0.42f * strength))
                : Mathf.Clamp01(0.2f * strength);
            ApplyPersistentAlpha(persistentAlpha, unscaledNow);
            ApplyWholeBodyTrigger(
                age,
                attack,
                spread,
                settle);
            RefreshRootVisibility();
        }

        public bool SyncFromSource()
        {
            if (!IsValidAuthoritativeArtwork(sourceImage))
            {
                return false;
            }

            Sprite active = ActiveSprite(sourceImage);
            if (active != sourceSprite
                || persistentImage.preserveAspect
                    != sourceImage.preserveAspect)
            {
                sourceSprite = active;
                SyncGraphicsFromSource();
                sourceSnapshot =
                    new ItemLivingGradientOutlineSourceSnapshot(
                        sourceImage);
            }
            RefreshRootVisibility();
            return true;
        }

        public void ReleaseToPool(Transform fallbackPoolParent)
        {
            persistent = false;
            triggerActive = false;
            sourceImage = null;
            sourceSprite = null;
            palette = null;
            family = ItemLivingGradientOutlineBuildFamily.Unknown;
            ItemId = string.Empty;
            SurfaceId = string.Empty;
            if (persistentImage != null)
            {
                persistentImage.sprite = null;
                persistentImage.overrideSprite = null;
                persistentImage.material = null;
                persistentImage.canvasRenderer.SetAlpha(0f);
            }
            if (persistentHaloImage != null)
            {
                persistentHaloImage.sprite = null;
                persistentHaloImage.overrideSprite = null;
                persistentHaloImage.material = null;
                persistentHaloImage.canvasRenderer.SetAlpha(0f);
            }
            if (persistentHaloRect != null)
            {
                persistentHaloRect.localScale = Vector3.one;
            }
            ClearEffectImage(wholeBodyEmissionImage);
            ClearEffectImage(wholeBodyInnerGlowImage);
            ClearEffectImage(wholeBodyOuterHaloImage);
            if (wholeBodyOuterHaloRect != null)
            {
                wholeBodyOuterHaloRect.localScale = Vector3.one;
            }
            if (environmentSpillImage != null)
            {
                environmentSpillImage.material = null;
                environmentSpillImage.canvasRenderer.SetAlpha(0f);
            }
            if (environmentSpillRect != null)
            {
                environmentSpillRect.localScale = Vector3.one;
            }
            if (triggerImage != null)
            {
                triggerImage.sprite = null;
                triggerImage.overrideSprite = null;
                triggerImage.material = null;
                triggerImage.canvasRenderer.SetAlpha(0f);
                triggerImage.gameObject.SetActive(false);
            }
            if (triggerRect != null)
            {
                triggerRect.localScale = Vector3.one;
            }
            Transform destination =
                fallbackPoolParent != null
                    ? fallbackPoolParent
                    : poolParent;
            if (rootRect != null && destination != null)
            {
                rootRect.SetParent(destination, false);
                Stretch(rootRect);
            }
            gameObject.SetActive(false);
        }

        public static bool IsValidAuthoritativeArtwork(Image image)
        {
            Sprite sprite = ActiveSprite(image);
            return image != null
                && sprite != null
                && sprite.texture != null
                && image.gameObject.name.IndexOf(
                    RootNamePrefix,
                    StringComparison.Ordinal) < 0
                && image.gameObject.name.IndexOf(
                    PersistentHaloLayerName,
                    StringComparison.Ordinal) < 0
                && image.gameObject.name.IndexOf(
                    WholeBodyEmissionLayerName,
                    StringComparison.Ordinal) < 0
                && image.gameObject.name.IndexOf(
                    WholeBodyInnerGlowLayerName,
                    StringComparison.Ordinal) < 0
                && image.gameObject.name.IndexOf(
                    WholeBodyOuterHaloLayerName,
                    StringComparison.Ordinal) < 0
                && image.gameObject.name.IndexOf(
                    EnvironmentSpillLayerName,
                    StringComparison.Ordinal) < 0
                && image.gameObject.name.IndexOf(
                    TriggerLayerName,
                    StringComparison.Ordinal) < 0;
        }

        private void LateUpdate()
        {
            if (!SyncFromSource())
            {
                gameObject.SetActive(false);
                return;
            }
            EvaluateAt(Time.unscaledTime);
        }

        private void SyncGraphicsFromSource()
        {
            if (persistentImage == null
                || persistentHaloImage == null
                || wholeBodyEmissionImage == null
                || wholeBodyInnerGlowImage == null
                || wholeBodyOuterHaloImage == null
                || triggerImage == null
                || sourceImage == null)
            {
                return;
            }
            persistentImage.overrideSprite = null;
            persistentImage.sprite = sourceSprite;
            persistentImage.type = Image.Type.Simple;
            persistentImage.preserveAspect =
                sourceImage.preserveAspect;
            persistentImage.fillCenter = true;
            persistentImage.color = Color.white;
            persistentImage.raycastTarget = false;
            persistentImage.maskable = true;

            persistentHaloImage.overrideSprite = null;
            persistentHaloImage.sprite = sourceSprite;
            persistentHaloImage.type = Image.Type.Simple;
            persistentHaloImage.preserveAspect =
                sourceImage.preserveAspect;
            persistentHaloImage.fillCenter = true;
            persistentHaloImage.color = Color.white;
            persistentHaloImage.raycastTarget = false;
            persistentHaloImage.maskable = true;

            SyncEffectImageFromSource(wholeBodyEmissionImage);
            SyncEffectImageFromSource(wholeBodyInnerGlowImage);
            SyncEffectImageFromSource(wholeBodyOuterHaloImage);

            triggerImage.overrideSprite = null;
            triggerImage.sprite = sourceSprite;
            triggerImage.type = Image.Type.Simple;
            triggerImage.preserveAspect =
                sourceImage.preserveAspect;
            triggerImage.fillCenter = true;
            triggerImage.color = Color.white;
            triggerImage.raycastTarget = false;
            triggerImage.maskable = true;
        }

        private void RefreshRootVisibility()
        {
            if (sourceImage == null)
            {
                if (gameObject.activeSelf)
                {
                    gameObject.SetActive(false);
                }
                return;
            }
            bool sourceVisible =
                sourceImage.gameObject.activeInHierarchy
                && sourceImage.enabled
                && sourceImage.color.a > 0.004f
                && ActiveSprite(sourceImage) != null;
            bool visible =
                sourceVisible && (persistent || triggerActive);
            if (gameObject.activeSelf != visible)
            {
                gameObject.SetActive(visible);
            }
        }

        private void ApplyPersistentAlpha(
            float alpha,
            float unscaledNow)
        {
            float stableAlpha = Mathf.Clamp01(alpha);
            float breathAmount =
                palette == null ? 0f : palette.breathAmount;
            float breathSpeed =
                palette == null
                    ? 0.32f
                    : palette.breathCyclesPerSecond;
            float breathSin = Mathf.Sin(
                unscaledNow
                * breathSpeed
                * Mathf.PI
                * 2f);
            float breathWave = 0.5f + 0.5f * breathSin;
            float coreBreath =
                1f + breathSin * breathAmount;
            if (persistentImage != null)
            {
                persistentImage.canvasRenderer.SetAlpha(
                    Mathf.Clamp01(stableAlpha * coreBreath));
            }
            if (persistentHaloImage != null)
            {
                float haloAlpha = palette == null
                    ? 0f
                    : stableAlpha
                    * palette.persistentHaloAlpha
                    * Mathf.Lerp(0.82f, 1.18f, breathWave);
                persistentHaloImage.canvasRenderer.SetAlpha(
                    Mathf.Clamp01(haloAlpha));
            }
            if (persistentHaloRect != null)
            {
                float baseScale = palette == null
                    ? 1f
                    : palette.persistentHaloScale;
                // The 20 px halo overlaps the stable 15 px core. Only its
                // outer five texels remain exposed, so breathing cannot
                // open a transparent moat between the two layers.
                float breathingScale =
                    baseScale
                    * (1f + breathWave * breathAmount * 0.04f);
                persistentHaloRect.localScale = new Vector3(
                    breathingScale,
                    breathingScale,
                    1f);
            }
            if (palette == null)
            {
                SetEffectAlpha(wholeBodyEmissionImage, 0f);
                SetEffectAlpha(wholeBodyInnerGlowImage, 0f);
                SetEffectAlpha(wholeBodyOuterHaloImage, 0f);
                SetEffectAlpha(environmentSpillImage, 0f);
                if (wholeBodyOuterHaloRect != null)
                {
                    wholeBodyOuterHaloRect.localScale = Vector3.one;
                }
                if (environmentSpillRect != null)
                {
                    environmentSpillRect.localScale = Vector3.one;
                }
                return;
            }
            float powered = persistent ? 1f : 0f;
            float bodyBreathSin = Mathf.Sin(
                unscaledNow
                * palette.wholeBodyBreathCyclesPerSecond
                * Mathf.PI
                * 2f);
            float bodyBreathWave =
                0.5f + 0.5f * bodyBreathSin;
            float bodyBreath =
                1f
                + bodyBreathSin
                * palette.wholeBodyBreathAmount;
            if (wholeBodyEmissionImage != null)
            {
                wholeBodyEmissionImage.canvasRenderer.SetAlpha(
                    Mathf.Clamp01(
                        powered
                        * palette.wholeBodyPersistentAlpha
                        * bodyBreath));
            }
            if (wholeBodyInnerGlowImage != null)
            {
                wholeBodyInnerGlowImage.canvasRenderer.SetAlpha(
                    Mathf.Clamp01(
                        powered
                        * palette.wholeBodyInnerGlowAlpha
                        * Mathf.Lerp(
                            0.88f,
                            1.12f,
                            bodyBreathWave)));
            }
            if (wholeBodyOuterHaloImage != null)
            {
                wholeBodyOuterHaloImage.canvasRenderer.SetAlpha(
                    Mathf.Clamp01(
                        powered
                        * palette.wholeBodyOuterGlowAlpha
                        * Mathf.Lerp(
                            0.82f,
                            1.18f,
                            bodyBreathWave)));
            }
            if (wholeBodyOuterHaloRect != null)
            {
                float persistentSpread =
                    1f
                    + powered
                    * bodyBreathWave
                    * palette.wholeBodyBreathAmount
                    * 0.08f;
                wholeBodyOuterHaloRect.localScale = new Vector3(
                    persistentSpread,
                    persistentSpread,
                    1f);
            }
            if (environmentSpillImage != null)
            {
                environmentSpillImage.canvasRenderer.SetAlpha(
                    Mathf.Clamp01(
                        powered
                        * palette.environmentSpillPersistentAlpha
                        * Mathf.Lerp(
                            0.84f,
                            1.16f,
                            bodyBreathWave)));
            }
            if (environmentSpillRect != null)
            {
                float spillScale =
                    palette.environmentSpillBaseScale
                    * (1f
                        + powered
                        * bodyBreathWave
                        * palette.wholeBodyBreathAmount
                        * 0.06f);
                environmentSpillRect.localScale = new Vector3(
                    spillScale,
                    spillScale,
                    1f);
            }
        }

        private void ApplyWholeBodyTrigger(
            float age,
            float attack,
            float spread,
            float settle)
        {
            if (palette == null
                || wholeBodyEmissionImage == null
                || wholeBodyInnerGlowImage == null
                || wholeBodyOuterHaloImage == null
                || wholeBodyOuterHaloRect == null
                || environmentSpillImage == null
                || environmentSpillRect == null)
            {
                return;
            }

            float baseBody = persistent
                ? palette.wholeBodyPersistentAlpha
                : 0f;
            float baseInner = persistent
                ? palette.wholeBodyInnerGlowAlpha
                : 0f;
            float baseOuter = persistent
                ? palette.wholeBodyOuterGlowAlpha
                : 0f;
            float bodyPeak = palette.wholeBodyTriggerAlpha;
            float innerPeak = Mathf.Clamp01(
                palette.wholeBodyInnerGlowAlpha * 2.55f);
            float outerPeak = Mathf.Clamp01(
                palette.wholeBodyOuterGlowAlpha * 3.2f);
            float bodyAlpha;
            float innerAlpha;
            float outerAlpha;
            float outerScale;
            float spillAlpha;
            float spillScale;

            if (age <= attack)
            {
                float ignition = Smooth01(age / attack);
                bodyAlpha = Mathf.Lerp(
                    baseBody,
                    bodyPeak,
                    ignition);
                innerAlpha = Mathf.Lerp(
                    baseInner,
                    innerPeak,
                    ignition);
                outerAlpha = Mathf.Lerp(
                    baseOuter,
                    outerPeak * 0.55f,
                    ignition);
                outerScale = 1f;
                spillAlpha = Mathf.Lerp(
                    persistent
                        ? palette.environmentSpillPersistentAlpha
                        : 0f,
                    palette.environmentSpillTriggerAlpha * 0.62f,
                    ignition);
                spillScale =
                    palette.environmentSpillBaseScale;
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
                    palette.wholeBodyTriggerEndScale,
                    expansion);
                spillAlpha = Mathf.Lerp(
                    palette.environmentSpillTriggerAlpha * 0.62f,
                    palette.environmentSpillTriggerAlpha,
                    expansion);
                spillScale = Mathf.Lerp(
                    palette.environmentSpillBaseScale,
                    palette.environmentSpillTriggerScale,
                    expansion);
            }
            else
            {
                float afterglow = Smooth01(
                    Mathf.InverseLerp(spread, settle, age));
                bodyAlpha = Mathf.Lerp(
                    bodyPeak * 0.82f,
                    baseBody,
                    afterglow);
                innerAlpha = Mathf.Lerp(
                    innerPeak * 0.78f,
                    baseInner,
                    afterglow);
                outerAlpha = Mathf.Lerp(
                    outerPeak,
                    baseOuter,
                    afterglow);
                outerScale = Mathf.Lerp(
                    palette.wholeBodyTriggerEndScale,
                    1f,
                    afterglow);
                spillAlpha = Mathf.Lerp(
                    palette.environmentSpillTriggerAlpha,
                    persistent
                        ? palette.environmentSpillPersistentAlpha
                        : 0f,
                    afterglow);
                spillScale = Mathf.Lerp(
                    palette.environmentSpillTriggerScale,
                    palette.environmentSpillBaseScale,
                    afterglow);
            }

            wholeBodyEmissionImage.canvasRenderer.SetAlpha(
                Mathf.Clamp01(bodyAlpha));
            wholeBodyInnerGlowImage.canvasRenderer.SetAlpha(
                Mathf.Clamp01(innerAlpha));
            wholeBodyOuterHaloImage.canvasRenderer.SetAlpha(
                Mathf.Clamp01(outerAlpha));
            wholeBodyOuterHaloRect.localScale = new Vector3(
                outerScale,
                outerScale,
                1f);
            environmentSpillImage.canvasRenderer.SetAlpha(
                Mathf.Clamp01(spillAlpha));
            environmentSpillRect.localScale = new Vector3(
                spillScale,
                spillScale,
                1f);
        }

        private static Sprite ActiveSprite(Image image)
        {
            return image == null
                ? null
                : image.overrideSprite != null
                    ? image.overrideSprite
                    : image.sprite;
        }

        private void SyncEffectImageFromSource(Image image)
        {
            if (image == null || sourceImage == null)
            {
                return;
            }
            image.overrideSprite = null;
            image.sprite = sourceSprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = sourceImage.preserveAspect;
            image.fillCenter = true;
            image.color = Color.white;
            image.raycastTarget = false;
            image.maskable = true;
        }

        private static void ClearEffectImage(Image image)
        {
            if (image == null)
            {
                return;
            }
            image.sprite = null;
            image.overrideSprite = null;
            image.material = null;
            image.canvasRenderer.SetAlpha(0f);
        }

        private static void SetEffectAlpha(
            Graphic image,
            float alpha)
        {
            if (image != null)
            {
                image.canvasRenderer.SetAlpha(alpha);
            }
        }

        private static void ConfigureEffectImage(Image image)
        {
            image.raycastTarget = false;
            image.maskable = true;
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            image.fillCenter = true;
            image.color = Color.white;
        }

        private static void ConfigureEnvironmentSpillImage(
            RawImage image)
        {
            image.raycastTarget = false;
            image.maskable = true;
            image.texture = Texture2D.whiteTexture;
            image.uvRect = new Rect(0f, 0f, 1f, 1f);
            image.color = Color.white;
        }

        private static void Stretch(RectTransform rect)
        {
            if (rect == null)
            {
                return;
            }
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        private static float Smooth01(float value)
        {
            float t = Mathf.Clamp01(value);
            return t * t * (3f - 2f * t);
        }

        private void OnDestroy()
        {
            sourceImage = null;
            sourceSprite = null;
            palette = null;
        }
    }
}
