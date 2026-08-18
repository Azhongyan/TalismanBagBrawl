using System;
using System.Collections.Generic;
using UnityEngine;

namespace TalismanBag.BuildSandbox
{
    public sealed class ItemLivingGradientOutlineMaterialSet
    {
        public ItemLivingGradientOutlineMaterialSet(
            Material persistent,
            Material persistentHalo,
            Material trigger,
            Material wholeBodyEmission,
            Material wholeBodyInnerGlow,
            Material wholeBodyOuterGlow,
            Material environmentSpill,
            Sprite replaceableRingSprite)
        {
            Persistent = persistent;
            PersistentHalo = persistentHalo;
            Trigger = trigger;
            WholeBodyEmission = wholeBodyEmission;
            WholeBodyInnerGlow = wholeBodyInnerGlow;
            WholeBodyOuterGlow = wholeBodyOuterGlow;
            EnvironmentSpill = environmentSpill;
            ReplaceableRingSprite = replaceableRingSprite;
        }

        public Material Persistent { get; }
        public Material PersistentHalo { get; }
        public Material Trigger { get; }
        public Material WholeBodyEmission { get; }
        public Material WholeBodyInnerGlow { get; }
        public Material WholeBodyOuterGlow { get; }
        public Material EnvironmentSpill { get; }
        public Sprite ReplaceableRingSprite { get; }
        public bool IsValid =>
            Persistent != null
            && PersistentHalo != null
            && Trigger != null
            && WholeBodyEmission != null
            && WholeBodyInnerGlow != null
            && WholeBodyOuterGlow != null
            && EnvironmentSpill != null;
    }

    public sealed class ItemLivingGradientOutlineMaterialLibrary :
        IDisposable
    {
        private readonly Dictionary<
            ItemLivingGradientOutlineBuildFamily,
            ItemLivingGradientOutlineMaterialSet> sets = new();
        private readonly List<Material> ownedMaterials = new();

        public int MaterialCount => ownedMaterials.Count;
        public bool IsInitialized { get; private set; }
        public string LastDiagnostic { get; private set; } =
            "NOT_INITIALIZED";

        public bool Initialize(
            ItemLivingGradientOutlineProfile profile)
        {
            Dispose();
            if (profile == null)
            {
                LastDiagnostic = "PROFILE_MISSING";
                return false;
            }

            Shader fallbackShader =
                Shader.Find(ItemLivingGradientOutlineProfile.ShaderName);
            Shader environmentSpillShader = Shader.Find(
                ItemLivingGradientOutlineProfile
                    .EnvironmentSpillShaderName);
            bool liHuo = TryCreateSet(
                ItemLivingGradientOutlineBuildFamily.LiHuo,
                profile.liHuo,
                fallbackShader,
                environmentSpillShader);
            bool taiBai = TryCreateSet(
                ItemLivingGradientOutlineBuildFamily.TaiBaiReserved,
                profile.taiBaiReserved,
                fallbackShader,
                environmentSpillShader);
            IsInitialized = liHuo && taiBai;
            LastDiagnostic = IsInitialized
                ? "SHARED_MATERIALS_READY"
                : "SHARED_MATERIALS_INCOMPLETE";
            return IsInitialized;
        }

        public bool TryGet(
            ItemLivingGradientOutlineBuildFamily family,
            out ItemLivingGradientOutlineMaterialSet set)
        {
            return sets.TryGetValue(family, out set)
                && set != null
                && set.IsValid;
        }

        public void Dispose()
        {
            sets.Clear();
            foreach (Material material in ownedMaterials)
            {
                if (material == null)
                {
                    continue;
                }
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(material);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(material);
                }
            }
            ownedMaterials.Clear();
            IsInitialized = false;
        }

        private bool TryCreateSet(
            ItemLivingGradientOutlineBuildFamily family,
            ItemLivingGradientOutlinePaletteProfile palette,
            Shader fallbackShader,
            Shader environmentSpillShader)
        {
            if (palette == null
                || palette.persistent == null
                || palette.trigger == null)
            {
                return false;
            }

            Material persistent = CreateSharedMaterial(
                palette,
                palette.persistent,
                fallbackShader,
                "Persistent");
            ItemLivingGradientOutlinePassProfile haloPass = new()
            {
                materialResourcePath =
                    palette.persistent.materialResourcePath,
                outlineRadiusTexels = Mathf.Clamp(
                    palette.persistent.outlineRadiusTexels
                    + palette.persistentHaloWidthTexels,
                    0.25f,
                    32f),
                innerEdgeStrength = 0f,
                haloStrength = Mathf.Max(
                    0.9f,
                    palette.persistent.haloStrength),
                bandOpacity =
                    palette.persistent.bandOpacity * 0.62f,
                brightness =
                    palette.persistent.brightness * 1.05f
            };
            Material persistentHalo = CreateSharedMaterial(
                palette,
                haloPass,
                fallbackShader,
                "PersistentHalo");
            Material trigger = CreateSharedMaterial(
                palette,
                palette.trigger,
                fallbackShader,
                "Trigger");
            ItemLivingGradientOutlinePassProfile bodyPass = new()
            {
                materialResourcePath =
                    palette.persistent.materialResourcePath,
                outlineRadiusTexels = 0.25f,
                innerEdgeStrength = 0f,
                haloStrength = 0f,
                bandOpacity = 0f,
                brightness = 1f
            };
            Material wholeBodyEmission = CreateSharedMaterial(
                palette,
                bodyPass,
                fallbackShader,
                "WholeBodyEmission");
            ItemLivingGradientOutlinePassProfile innerGlowPass = new()
            {
                materialResourcePath =
                    palette.persistent.materialResourcePath,
                outlineRadiusTexels =
                    palette.wholeBodyInnerGlowRadiusTexels,
                innerEdgeStrength = 0f,
                haloStrength =
                    palette.wholeBodyInnerGlowStrength,
                bandOpacity = 0f,
                brightness = 1f
            };
            Material wholeBodyInnerGlow = CreateSharedMaterial(
                palette,
                innerGlowPass,
                fallbackShader,
                "WholeBodyInnerGlow");
            ItemLivingGradientOutlinePassProfile outerGlowPass = new()
            {
                materialResourcePath =
                    palette.persistent.materialResourcePath,
                outlineRadiusTexels =
                    palette.wholeBodyOuterGlowRadiusTexels,
                innerEdgeStrength = 0f,
                haloStrength =
                    palette.wholeBodyOuterGlowStrength,
                bandOpacity = 0f,
                brightness = 1f
            };
            Material wholeBodyOuterGlow = CreateSharedMaterial(
                palette,
                outerGlowPass,
                fallbackShader,
                "WholeBodyOuterGlow");
            Material environmentSpill =
                CreateEnvironmentSpillMaterial(
                    palette,
                    environmentSpillShader);
            if (persistent == null
                || persistentHalo == null
                || trigger == null
                || wholeBodyEmission == null
                || wholeBodyInnerGlow == null
                || wholeBodyOuterGlow == null
                || environmentSpill == null)
            {
                DestroyOwned(persistent);
                DestroyOwned(persistentHalo);
                DestroyOwned(trigger);
                DestroyOwned(wholeBodyEmission);
                DestroyOwned(wholeBodyInnerGlow);
                DestroyOwned(wholeBodyOuterGlow);
                DestroyOwned(environmentSpill);
                return false;
            }

            Sprite ringSprite = string.IsNullOrWhiteSpace(
                    palette.ringSpriteResourcePath)
                ? null
                : Resources.Load<Sprite>(
                    palette.ringSpriteResourcePath.Trim());
            persistent.SetFloat("_UseRingTexture", 0f);
            persistentHalo.SetFloat("_UseRingTexture", 0f);
            ConfigureBlend(persistent, false);
            ConfigureBlend(persistentHalo, false);
            ConfigureBlend(trigger, false);
            ConfigureWholeBodyMaterial(
                wholeBodyEmission,
                true,
                1f,
                palette.wholeBodyWarmth,
                0.58f);
            ConfigureWholeBodyMaterial(
                wholeBodyInnerGlow,
                true,
                0f,
                palette.wholeBodyWarmth,
                0.82f);
            ConfigureWholeBodyMaterial(
                wholeBodyOuterGlow,
                true,
                0f,
                palette.wholeBodyWarmth,
                0.42f);
            trigger.SetFloat(
                "_UseRingTexture",
                ringSprite == null ? 0f : 1f);
            if (ringSprite != null && ringSprite.texture != null)
            {
                trigger.SetTexture(
                    "_RingTexture", ringSprite.texture);
            }
            sets[family] = new ItemLivingGradientOutlineMaterialSet(
                persistent,
                persistentHalo,
                trigger,
                wholeBodyEmission,
                wholeBodyInnerGlow,
                wholeBodyOuterGlow,
                environmentSpill,
                ringSprite);
            return true;
        }

        private Material CreateSharedMaterial(
            ItemLivingGradientOutlinePaletteProfile palette,
            ItemLivingGradientOutlinePassProfile pass,
            Shader fallbackShader,
            string passName)
        {
            Material template = string.IsNullOrWhiteSpace(
                    pass.materialResourcePath)
                ? null
                : Resources.Load<Material>(
                    pass.materialResourcePath.Trim());
            Shader shader = template == null
                ? fallbackShader
                : template.shader;
            if (shader == null)
            {
                return null;
            }

            Material material = template == null
                ? new Material(shader)
                : new Material(template);
            material.name =
                "UI_ItemLivingGradientOutline_"
                + palette.paletteId
                + "_"
                + passName
                + "_SharedRuntime";
            material.hideFlags = HideFlags.HideAndDontSave;
            ApplyPalette(material, palette, pass);
            ownedMaterials.Add(material);
            return material;
        }

        private Material CreateEnvironmentSpillMaterial(
            ItemLivingGradientOutlinePaletteProfile palette,
            Shader shader)
        {
            if (palette == null || shader == null)
            {
                return null;
            }

            Material material = new(shader)
            {
                name =
                    "UI_ItemWholeBodyEnvironmentSpill_"
                    + palette.paletteId
                    + "_SharedRuntime",
                hideFlags = HideFlags.HideAndDontSave
            };
            material.SetColor(
                "_InnerColor",
                ItemLivingGradientOutlineProfile.ParseColor(
                    palette.highlightColorHex,
                    new Color(1f, 0.84f, 0.35f, 1f)));
            material.SetColor(
                "_OuterColor",
                ItemLivingGradientOutlineProfile.ParseColor(
                    palette.baseColorHex,
                    new Color(0.85f, 0.16f, 0.08f, 1f)));
            material.SetFloat(
                "_InnerRadius",
                palette.environmentSpillInnerRadius);
            material.SetFloat(
                "_OuterRadius",
                palette.environmentSpillOuterRadius);
            material.SetFloat("_SpillStrength", 1f);
            ownedMaterials.Add(material);
            return material;
        }

        private static void ApplyPalette(
            Material material,
            ItemLivingGradientOutlinePaletteProfile palette,
            ItemLivingGradientOutlinePassProfile pass)
        {
            material.SetColor(
                "_BandColorA",
                ItemLivingGradientOutlineProfile.ParseColor(
                    palette.baseColorHex,
                    new Color(0.85f, 0.16f, 0.08f, 1f)));
            material.SetColor(
                "_BandColorB",
                ItemLivingGradientOutlineProfile.ParseColor(
                    palette.middleColorHex,
                    new Color(1f, 0.45f, 0.08f, 1f)));
            material.SetColor(
                "_HighlightColor",
                ItemLivingGradientOutlineProfile.ParseColor(
                    palette.highlightColorHex,
                    new Color(1f, 0.84f, 0.35f, 1f)));
            material.SetFloat(
                "_OutlineRadiusTexels",
                pass.outlineRadiusTexels);
            material.SetFloat(
                "_InnerEdgeStrength",
                pass.innerEdgeStrength);
            material.SetFloat(
                "_HaloStrength",
                pass.haloStrength);
            material.SetFloat(
                "_BandOpacity",
                pass.bandOpacity);
            material.SetFloat(
                "_Brightness",
                pass.brightness);
            material.SetFloat("_BodyEmissionStrength", 0f);
            material.SetFloat("_BodyWarmth", 0f);
            material.SetFloat("_GlowFalloff", 0.58f);
            material.SetFloat("_FlowSpeed", palette.flowSpeed);
            material.SetFloat("_FlowScale", palette.flowScale);
            material.SetFloat(
                "_AngularBlend",
                palette.angularBlend);
            material.SetFloat(
                "_NoiseAmount",
                palette.noiseAmount);
            material.SetFloat(
                "_HighlightStrength",
                palette.highlightStrength);

            Texture2D gradient = string.IsNullOrWhiteSpace(
                    palette.gradientLutResourcePath)
                ? null
                : Resources.Load<Texture2D>(
                    palette.gradientLutResourcePath.Trim());
            Texture2D noise = string.IsNullOrWhiteSpace(
                    palette.noiseTextureResourcePath)
                ? null
                : Resources.Load<Texture2D>(
                    palette.noiseTextureResourcePath.Trim());
            material.SetFloat(
                "_UseGradientLut",
                gradient == null ? 0f : 1f);
            material.SetFloat(
                "_UseNoiseTexture",
                noise == null ? 0f : 1f);
            material.SetFloat("_UseRingTexture", 0f);
            if (gradient != null)
            {
                material.SetTexture("_GradientLut", gradient);
            }
            if (noise != null)
            {
                material.SetTexture("_NoiseTexture", noise);
            }
        }

        private static void ConfigureWholeBodyMaterial(
            Material material,
            bool additive,
            float bodyEmissionStrength,
            float bodyWarmth,
            float glowFalloff)
        {
            ConfigureBlend(material, additive);
            material.SetFloat(
                "_BodyEmissionStrength",
                bodyEmissionStrength);
            material.SetFloat(
                "_BodyWarmth",
                Mathf.Clamp01(bodyWarmth));
            material.SetFloat(
                "_GlowFalloff",
                Mathf.Clamp(glowFalloff, 0.1f, 2f));
            material.SetFloat("_UseRingTexture", 0f);
        }

        private static void ConfigureBlend(
            Material material,
            bool additive)
        {
            material.SetFloat(
                "_SrcBlend",
                (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetFloat(
                "_DstBlend",
                additive
                    ? (float)UnityEngine.Rendering.BlendMode.One
                    : (float)UnityEngine.Rendering.BlendMode
                        .OneMinusSrcAlpha);
        }

        private void DestroyOwned(Material material)
        {
            if (material == null)
            {
                return;
            }
            ownedMaterials.Remove(material);
            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(material);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(material);
            }
        }
    }
}
