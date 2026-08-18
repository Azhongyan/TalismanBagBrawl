using System;
using UnityEngine;

namespace TalismanBag.BuildSandbox
{
    public enum ItemLivingGradientOutlineBuildFamily
    {
        Unknown = 0,
        LiHuo = 1,
        TaiBaiReserved = 2
    }

    [Serializable]
    public sealed class ItemLivingGradientOutlinePassProfile
    {
        public string materialResourcePath = string.Empty;
        [Min(0.25f)] public float outlineRadiusTexels = 15f;
        [Range(0f, 1f)] public float innerEdgeStrength = 0.18f;
        [Range(0f, 2f)] public float haloStrength = 0.7f;
        [Range(0f, 3f)] public float bandOpacity = 1.35f;
        [Range(0f, 4f)] public float brightness = 1.2f;
    }

    [Serializable]
    public sealed class ItemLivingGradientOutlinePaletteProfile
    {
        public string paletteId = string.Empty;
        public string buildFamilyStableKey = string.Empty;
        public string baseColorHex = "#E23B1F";
        public string middleColorHex = "#FF7A1A";
        public string highlightColorHex = "#FFD66A";
        public string gradientLutResourcePath = string.Empty;
        public string noiseTextureResourcePath = string.Empty;
        public string ringSpriteResourcePath = string.Empty;
        [Range(0.01f, 2f)] public float flowSpeed = 0.16f;
        [Range(0.25f, 8f)] public float flowScale = 2.6f;
        [Range(0f, 1f)] public float angularBlend = 0.78f;
        [Range(0f, 1f)] public float noiseAmount = 0.08f;
        [Range(0f, 4f)] public float highlightStrength = 1.35f;
        [Range(0f, 1f)] public float persistentAlpha = 0.86f;
        [Range(0f, 1f)] public float triggerAlpha = 0.98f;
        [Range(0f, 1f)] public float persistentHaloAlpha = 0.38f;
        [Range(0f, 12f)] public float persistentHaloWidthTexels = 5f;
        [Range(1f, 1.25f)] public float persistentHaloScale = 1f;
        [Range(0f, 0.25f)] public float breathAmount = 0.09f;
        [Range(0.05f, 1f)] public float breathCyclesPerSecond = 0.32f;
        [Range(0f, 1f)] public float wholeBodyPersistentAlpha = 0.24f;
        [Range(0f, 1f)] public float wholeBodyTriggerAlpha = 0.72f;
        [Range(0f, 1f)] public float wholeBodyWarmth = 0.24f;
        [Range(0f, 1f)] public float wholeBodyInnerGlowAlpha = 0.22f;
        [Range(0f, 1f)] public float wholeBodyOuterGlowAlpha = 0.12f;
        [Range(0.25f, 16f)]
        public float wholeBodyInnerGlowRadiusTexels = 7f;
        [Range(1f, 48f)]
        public float wholeBodyOuterGlowRadiusTexels = 32f;
        [Range(0f, 2f)]
        public float wholeBodyInnerGlowStrength = 1.15f;
        [Range(0f, 2f)]
        public float wholeBodyOuterGlowStrength = 0.82f;
        [Range(0f, 0.3f)] public float wholeBodyBreathAmount = 0.14f;
        [Range(0.45f, 0.72f)]
        public float wholeBodyBreathCyclesPerSecond = 0.56f;
        [Range(1f, 1.35f)]
        public float wholeBodyTriggerEndScale = 1.2f;
        [Range(0f, 1f)]
        public float environmentSpillPersistentAlpha = 0.18f;
        [Range(0f, 1f)]
        public float environmentSpillTriggerAlpha = 0.62f;
        [Range(1f, 2.5f)]
        public float environmentSpillBaseScale = 1.75f;
        [Range(1f, 3f)]
        public float environmentSpillTriggerScale = 2.15f;
        [Range(0.05f, 0.35f)]
        public float environmentSpillInnerRadius = 0.16f;
        [Range(0.3f, 0.7f)]
        public float environmentSpillOuterRadius = 0.52f;
        public ItemLivingGradientOutlinePassProfile persistent =
            new ItemLivingGradientOutlinePassProfile();
        public ItemLivingGradientOutlinePassProfile trigger =
            new ItemLivingGradientOutlinePassProfile
            {
                outlineRadiusTexels = 24f,
                innerEdgeStrength = 0.24f,
                haloStrength = 1.25f,
                bandOpacity = 1.7f,
                brightness = 1.7f
            };
    }

    [Serializable]
    public sealed class ItemLivingGradientOutlineProfile
    {
        public const string PackageId =
            "V0.4-ItemLivingGradientOutlineVfxPrototype01";
        public const string WholeBodyGlowPackageId =
            "V0.4-ItemWholeBodyGlowSpreadVfxPrototype01";
        public const string TargetSceneName =
            "Scene_TalismanBag_V04_BattleSandboxPreview";
        public const string ResourcesPath =
            "V04/ItemLivingGradientOutlineDevOnly/"
            + "item_living_gradient_outline_profile";
        public const string ShaderName =
            "TalismanBag/UI/ItemLivingGradientOutline";
        public const string EnvironmentSpillShaderName =
            "TalismanBag/UI/ItemWholeBodyEnvironmentSpill";

        public string profileId =
            "item_living_gradient_outline_v1";
        public string previewTargetItemId = "I009";
        public string previewTargetBuildFamilyStableKey = "lihuo";
        [Range(1, 8)] public int activeEffectCap = 6;
        [Range(0.05f, 1f)] public float rebindIntervalSeconds = 0.12f;
        [Range(0.05f, 0.2f)] public float triggerAttackSeconds = 0.08f;
        [Range(0.15f, 0.4f)] public float triggerSpreadSeconds = 0.25f;
        [Range(0.35f, 0.8f)] public float triggerSettleSeconds = 0.55f;
        [Range(1f, 1.3f)] public float triggerEndScale = 1.22f;
        public ItemLivingGradientOutlinePaletteProfile liHuo =
            CreateDefaultLiHuo();
        public ItemLivingGradientOutlinePaletteProfile taiBaiReserved =
            CreateDefaultTaiBai();

        public static ItemLivingGradientOutlineProfile Load()
        {
            TextAsset asset = Resources.Load<TextAsset>(ResourcesPath);
            ItemLivingGradientOutlineProfile profile = null;
            if (asset != null && !string.IsNullOrWhiteSpace(asset.text))
            {
                try
                {
                    profile =
                        JsonUtility.FromJson<
                            ItemLivingGradientOutlineProfile>(asset.text);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning(
                        "[" + PackageId + "] Profile parse failed; "
                        + "using procedural fallback. "
                        + exception.Message);
                }
            }

            profile ??= new ItemLivingGradientOutlineProfile();
            profile.Normalize();
            return profile;
        }

        public ItemLivingGradientOutlinePaletteProfile ResolvePalette(
            ItemLivingGradientOutlineBuildFamily family)
        {
            return family switch
            {
                ItemLivingGradientOutlineBuildFamily.LiHuo => liHuo,
                ItemLivingGradientOutlineBuildFamily.TaiBaiReserved =>
                    taiBaiReserved,
                _ => null
            };
        }

        public ItemLivingGradientOutlineBuildFamily ResolveBuildFamily(
            string stableKey)
        {
            string key = (stableKey ?? string.Empty).Trim();
            if (string.Equals(
                    key,
                    liHuo?.buildFamilyStableKey,
                    StringComparison.Ordinal))
            {
                return ItemLivingGradientOutlineBuildFamily.LiHuo;
            }
            if (string.Equals(
                    key,
                    taiBaiReserved?.buildFamilyStableKey,
                    StringComparison.Ordinal))
            {
                return ItemLivingGradientOutlineBuildFamily
                    .TaiBaiReserved;
            }
            return ItemLivingGradientOutlineBuildFamily.Unknown;
        }

        public static Color ParseColor(
            string html,
            Color fallback)
        {
            return ColorUtility.TryParseHtmlString(
                string.IsNullOrWhiteSpace(html) ? string.Empty : html,
                out Color color)
                ? color
                : fallback;
        }

        private void Normalize()
        {
            profileId = string.IsNullOrWhiteSpace(profileId)
                ? "item_living_gradient_outline_v1"
                : profileId.Trim();
            previewTargetItemId =
                (previewTargetItemId ?? string.Empty).Trim();
            previewTargetBuildFamilyStableKey =
                string.IsNullOrWhiteSpace(
                    previewTargetBuildFamilyStableKey)
                    ? "lihuo"
                    : previewTargetBuildFamilyStableKey.Trim();
            activeEffectCap = Mathf.Clamp(activeEffectCap, 1, 8);
            rebindIntervalSeconds = Mathf.Clamp(
                rebindIntervalSeconds, 0.05f, 1f);
            triggerAttackSeconds = Mathf.Clamp(
                triggerAttackSeconds, 0.05f, 0.2f);
            triggerSpreadSeconds = Mathf.Clamp(
                triggerSpreadSeconds,
                triggerAttackSeconds,
                0.4f);
            triggerSettleSeconds = Mathf.Clamp(
                triggerSettleSeconds,
                triggerSpreadSeconds,
                0.8f);
            triggerEndScale = Mathf.Clamp(
                triggerEndScale, 1f, 1.3f);
            liHuo ??= CreateDefaultLiHuo();
            taiBaiReserved ??= CreateDefaultTaiBai();
            NormalizePalette(liHuo, CreateDefaultLiHuo());
            NormalizePalette(
                taiBaiReserved, CreateDefaultTaiBai());
        }

        private static void NormalizePalette(
            ItemLivingGradientOutlinePaletteProfile value,
            ItemLivingGradientOutlinePaletteProfile fallback)
        {
            value.paletteId = string.IsNullOrWhiteSpace(value.paletteId)
                ? fallback.paletteId
                : value.paletteId.Trim();
            value.buildFamilyStableKey =
                string.IsNullOrWhiteSpace(value.buildFamilyStableKey)
                    ? fallback.buildFamilyStableKey
                    : value.buildFamilyStableKey.Trim();
            value.persistent ??=
                fallback.persistent;
            value.trigger ??= fallback.trigger;
            NormalizePass(value.persistent, fallback.persistent);
            NormalizePass(value.trigger, fallback.trigger);
            value.flowSpeed = Mathf.Clamp(value.flowSpeed, 0.01f, 2f);
            value.flowScale = Mathf.Clamp(value.flowScale, 0.25f, 8f);
            value.angularBlend =
                Mathf.Clamp01(value.angularBlend);
            value.noiseAmount = Mathf.Clamp01(value.noiseAmount);
            value.highlightStrength = Mathf.Clamp(
                value.highlightStrength, 0f, 4f);
            value.persistentAlpha =
                Mathf.Clamp01(value.persistentAlpha);
            value.triggerAlpha =
                Mathf.Clamp01(value.triggerAlpha);
            value.persistentHaloAlpha =
                Mathf.Clamp01(value.persistentHaloAlpha);
            value.persistentHaloWidthTexels = Mathf.Clamp(
                value.persistentHaloWidthTexels, 0f, 12f);
            value.persistentHaloScale = Mathf.Clamp(
                value.persistentHaloScale, 1f, 1.25f);
            value.breathAmount = Mathf.Clamp(
                value.breathAmount, 0f, 0.25f);
            value.breathCyclesPerSecond = Mathf.Clamp(
                value.breathCyclesPerSecond, 0.05f, 1f);
            value.wholeBodyPersistentAlpha =
                Mathf.Clamp01(value.wholeBodyPersistentAlpha);
            value.wholeBodyTriggerAlpha =
                Mathf.Clamp01(value.wholeBodyTriggerAlpha);
            value.wholeBodyWarmth =
                Mathf.Clamp01(value.wholeBodyWarmth);
            value.wholeBodyInnerGlowAlpha =
                Mathf.Clamp01(value.wholeBodyInnerGlowAlpha);
            value.wholeBodyOuterGlowAlpha =
                Mathf.Clamp01(value.wholeBodyOuterGlowAlpha);
            value.wholeBodyInnerGlowRadiusTexels = Mathf.Clamp(
                value.wholeBodyInnerGlowRadiusTexels, 0.25f, 16f);
            value.wholeBodyOuterGlowRadiusTexels = Mathf.Clamp(
                value.wholeBodyOuterGlowRadiusTexels, 1f, 48f);
            value.wholeBodyInnerGlowStrength = Mathf.Clamp(
                value.wholeBodyInnerGlowStrength, 0f, 2f);
            value.wholeBodyOuterGlowStrength = Mathf.Clamp(
                value.wholeBodyOuterGlowStrength, 0f, 2f);
            value.wholeBodyBreathAmount = Mathf.Clamp(
                value.wholeBodyBreathAmount, 0f, 0.3f);
            value.wholeBodyBreathCyclesPerSecond = Mathf.Clamp(
                value.wholeBodyBreathCyclesPerSecond, 0.45f, 0.72f);
            value.wholeBodyTriggerEndScale = Mathf.Clamp(
                value.wholeBodyTriggerEndScale, 1f, 1.35f);
            value.environmentSpillPersistentAlpha = Mathf.Clamp01(
                value.environmentSpillPersistentAlpha);
            value.environmentSpillTriggerAlpha = Mathf.Clamp01(
                value.environmentSpillTriggerAlpha);
            value.environmentSpillBaseScale = Mathf.Clamp(
                value.environmentSpillBaseScale, 1f, 2.5f);
            value.environmentSpillTriggerScale = Mathf.Clamp(
                value.environmentSpillTriggerScale,
                value.environmentSpillBaseScale,
                3f);
            value.environmentSpillInnerRadius = Mathf.Clamp(
                value.environmentSpillInnerRadius, 0.05f, 0.35f);
            value.environmentSpillOuterRadius = Mathf.Clamp(
                value.environmentSpillOuterRadius,
                Mathf.Max(
                    0.3f,
                    value.environmentSpillInnerRadius + 0.08f),
                0.7f);
        }

        private static void NormalizePass(
            ItemLivingGradientOutlinePassProfile value,
            ItemLivingGradientOutlinePassProfile fallback)
        {
            value.materialResourcePath =
                string.IsNullOrWhiteSpace(value.materialResourcePath)
                    ? fallback.materialResourcePath
                    : value.materialResourcePath.Trim();
            value.outlineRadiusTexels = Mathf.Clamp(
                value.outlineRadiusTexels, 0.25f, 32f);
            value.innerEdgeStrength =
                Mathf.Clamp01(value.innerEdgeStrength);
            value.haloStrength =
                Mathf.Clamp(value.haloStrength, 0f, 2f);
            value.bandOpacity =
                Mathf.Clamp(value.bandOpacity, 0f, 3f);
            value.brightness =
                Mathf.Clamp(value.brightness, 0f, 4f);
        }

        private static ItemLivingGradientOutlinePaletteProfile
            CreateDefaultLiHuo()
        {
            return new ItemLivingGradientOutlinePaletteProfile
            {
                paletteId = "lihuo_vermilion_gold",
                buildFamilyStableKey = "lihuo",
                baseColorHex = "#D92D18",
                middleColorHex = "#FF7A18",
                highlightColorHex = "#FFE08A",
                flowSpeed = 0.16f,
                flowScale = 2.6f,
                angularBlend = 0.78f,
                noiseAmount = 0.08f,
                highlightStrength = 1.55f,
                persistentAlpha = 0.86f,
                triggerAlpha = 0.98f,
                persistentHaloAlpha = 0.4f,
                persistentHaloWidthTexels = 5f,
                persistentHaloScale = 1f,
                breathAmount = 0.09f,
                breathCyclesPerSecond = 0.32f,
                wholeBodyPersistentAlpha = 0.25f,
                wholeBodyTriggerAlpha = 0.76f,
                wholeBodyWarmth = 0.25f,
                wholeBodyInnerGlowAlpha = 0.24f,
                wholeBodyOuterGlowAlpha = 0.13f,
                wholeBodyInnerGlowRadiusTexels = 7f,
                wholeBodyOuterGlowRadiusTexels = 32f,
                wholeBodyInnerGlowStrength = 1.2f,
                wholeBodyOuterGlowStrength = 0.86f,
                wholeBodyBreathAmount = 0.14f,
                wholeBodyBreathCyclesPerSecond = 0.56f,
                wholeBodyTriggerEndScale = 1.2f,
                environmentSpillPersistentAlpha = 0.18f,
                environmentSpillTriggerAlpha = 0.62f,
                environmentSpillBaseScale = 1.75f,
                environmentSpillTriggerScale = 2.15f,
                environmentSpillInnerRadius = 0.16f,
                environmentSpillOuterRadius = 0.52f,
                persistent = new ItemLivingGradientOutlinePassProfile
                {
                    materialResourcePath =
                        "V04/ItemLivingGradientOutlineDevOnly/"
                        + "UI_ItemLivingGradientOutline_LiHuo_Persistent",
                    outlineRadiusTexels = 15f,
                    innerEdgeStrength = 0.18f,
                    haloStrength = 0.7f,
                    bandOpacity = 1.35f,
                    brightness = 1.2f
                },
                trigger = new ItemLivingGradientOutlinePassProfile
                {
                    materialResourcePath =
                        "V04/ItemLivingGradientOutlineDevOnly/"
                        + "UI_ItemLivingGradientOutline_LiHuo_Trigger",
                    outlineRadiusTexels = 24f,
                    innerEdgeStrength = 0.24f,
                    haloStrength = 1.25f,
                    bandOpacity = 1.7f,
                    brightness = 1.7f
                }
            };
        }

        private static ItemLivingGradientOutlinePaletteProfile
            CreateDefaultTaiBai()
        {
            return new ItemLivingGradientOutlinePaletteProfile
            {
                paletteId = "taibai_cyan_silver_reserved",
                buildFamilyStableKey = "taibai",
                baseColorHex = "#2698C8",
                middleColorHex = "#55D8F0",
                highlightColorHex = "#EDF8FF",
                flowSpeed = 0.14f,
                flowScale = 2.45f,
                angularBlend = 0.8f,
                noiseAmount = 0.06f,
                highlightStrength = 1.48f,
                persistentAlpha = 0.82f,
                triggerAlpha = 0.96f,
                persistentHaloAlpha = 0.36f,
                persistentHaloWidthTexels = 5f,
                persistentHaloScale = 1f,
                breathAmount = 0.08f,
                breathCyclesPerSecond = 0.29f,
                wholeBodyPersistentAlpha = 0.22f,
                wholeBodyTriggerAlpha = 0.7f,
                wholeBodyWarmth = 0.18f,
                wholeBodyInnerGlowAlpha = 0.21f,
                wholeBodyOuterGlowAlpha = 0.11f,
                wholeBodyInnerGlowRadiusTexels = 7f,
                wholeBodyOuterGlowRadiusTexels = 30f,
                wholeBodyInnerGlowStrength = 1.08f,
                wholeBodyOuterGlowStrength = 0.78f,
                wholeBodyBreathAmount = 0.12f,
                wholeBodyBreathCyclesPerSecond = 0.52f,
                wholeBodyTriggerEndScale = 1.18f,
                environmentSpillPersistentAlpha = 0.15f,
                environmentSpillTriggerAlpha = 0.56f,
                environmentSpillBaseScale = 1.72f,
                environmentSpillTriggerScale = 2.08f,
                environmentSpillInnerRadius = 0.16f,
                environmentSpillOuterRadius = 0.5f,
                persistent = new ItemLivingGradientOutlinePassProfile
                {
                    materialResourcePath =
                        "V04/ItemLivingGradientOutlineDevOnly/"
                        + "UI_ItemLivingGradientOutline_TaiBai_Persistent",
                    outlineRadiusTexels = 15f,
                    innerEdgeStrength = 0.16f,
                    haloStrength = 0.62f,
                    bandOpacity = 1.28f,
                    brightness = 1.16f
                },
                trigger = new ItemLivingGradientOutlinePassProfile
                {
                    materialResourcePath =
                        "V04/ItemLivingGradientOutlineDevOnly/"
                        + "UI_ItemLivingGradientOutline_TaiBai_Trigger",
                    outlineRadiusTexels = 24f,
                    innerEdgeStrength = 0.22f,
                    haloStrength = 1.12f,
                    bandOpacity = 1.62f,
                    brightness = 1.62f
                }
            };
        }
    }
}
