using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TalismanBag.Presentation.Items;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.Editor.Presentation.Items
{
    public static class ItemRarityContourBloomPrefabAuthoring
    {
        public const string TerminalMarker =
            "ITEM_RARITY_CONTOUR_BLOOM_PREFAB_AUTHORED_PASS";

        private const string PrefabFolder =
            "Assets/_Game/Prefabs/TalismanBag/ItemVFX";
        private const string PrefabPath = PrefabFolder
            + "/ItemRarityContourBloomVfx.prefab";
        private const string ProfileFolder =
            "Assets/_Game/Resources/V04/ItemPresentation/RarityVfx";
        private const string ProfilePath = ProfileFolder
            + "/ItemRarityContourBloomProfile.asset";
        private const string MaterialFolder = ProfileFolder + "/Materials";
        private const string ContourShaderName =
            "TalismanBag/UI/ItemRarityContourBloom";
        private const string SpillShaderName =
            "TalismanBag/UI/ItemRarityEnvironmentSpill";
        private const string RuntimeSourcePath =
            "Assets/_Game/Scripts/TalismanBag/Presentation/Items/ItemRarityContourBloomVfx.cs";
        private const string ProfileSourcePath =
            "Assets/_Game/Scripts/TalismanBag/Presentation/Items/ItemRarityContourBloomProfile.cs";

        private const string ContourRole = "contour";
        private const string PersistentHaloRole = "persistent_halo";
        private const string WholeBodyEmissionRole =
            "whole_body_emission";
        private const string InnerGlowRole = "inner_glow";
        private const string OuterHaloRole = "outer_halo";
        private const string SpillRole = "spill";
        private const string TriggerRole = "trigger";

        private static readonly string[] ContourShaderRoles =
        {
            ContourRole,
            PersistentHaloRole,
            WholeBodyEmissionRole,
            InnerGlowRole,
            OuterHaloRole,
            TriggerRole
        };

        private static readonly string[] AllRoles =
        {
            ContourRole,
            PersistentHaloRole,
            WholeBodyEmissionRole,
            InnerGlowRole,
            OuterHaloRole,
            SpillRole,
            TriggerRole
        };

        private static readonly TierSpec[] TierSpecs =
        {
            new(
                ItemRarityContourBloomProfile.WhiteKey,
                "#D8D4CC", "#F7F3E8", "#FFF8D8",
                0.78f, 0.32f, 0.19f, 0.18f, 0.095f,
                0.13f, 0.94f, 0.68f, 0.52f,
                0.14f, 0.07f, 0.28f, 0.10f, 0.50f, 0.14f),
            new(
                ItemRarityContourBloomProfile.GreenKey,
                "#399A5D", "#72CB86", "#DFFFD2",
                0.80f, 0.34f, 0.205f, 0.195f, 0.10f,
                0.14f, 0.95f, 0.70f, 0.54f,
                0.145f, 0.075f, 0.29f, 0.11f, 0.515f, 0.17f),
            new(
                ItemRarityContourBloomProfile.BlueKey,
                "#3B72B8", "#65AFE1", "#DDF3FF",
                0.82f, 0.36f, 0.22f, 0.21f, 0.11f,
                0.15f, 0.96f, 0.72f, 0.56f,
                0.15f, 0.08f, 0.30f, 0.12f, 0.53f, 0.18f),
            new(
                ItemRarityContourBloomProfile.PurpleKey,
                "#7649A8", "#B170D5", "#F2E1FF",
                0.84f, 0.38f, 0.235f, 0.225f, 0.12f,
                0.165f, 0.97f, 0.74f, 0.59f,
                0.155f, 0.085f, 0.31f, 0.13f, 0.545f, 0.22f),
            new(
                ItemRarityContourBloomProfile.OrangeKey,
                "#B96528", "#E99743", "#FFE1A0",
                0.86f, 0.40f, 0.25f, 0.24f, 0.13f,
                0.18f, 0.98f, 0.76f, 0.62f,
                0.16f, 0.09f, 0.32f, 0.14f, 0.56f, 0.25f)
        };

        private static readonly string[] ForbiddenRuntimeTokens =
        {
            "new GameObject(",
            "Instantiate(",
            "GameObject.Find",
            "FindObjectOfType",
            "RuntimeInitializeOnLoad",
            "Resources.Load",
            "new Material(",
            "BuildFamily",
            "ItemId",
            "Formation",
            "Battle",
            "TalismanBag.BuildSandbox",
            "Causal"
        };

        [MenuItem(
            "TalismanBag/V0.4/Item Presentation/Author Rarity Contour Bloom Prefab",
            false,
            2470)]
        public static void AuthorFromMenu()
        {
            ApplySingleAuthoringPass();
        }

        public static void ExecuteFromCommandLine()
        {
            try
            {
                ApplySingleAuthoringPass();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError(
                    "[ItemRarityContourBloomAuthoring] FAIL "
                    + exception.Message);
                EditorApplication.Exit(1);
            }
        }

        private static void ApplySingleAuthoringPass()
        {
            EnsureFolder(PrefabFolder);
            EnsureFolder(ProfileFolder);
            EnsureFolder(MaterialFolder);

            Shader contourShader = Shader.Find(ContourShaderName);
            Shader spillShader = Shader.Find(SpillShaderName);
            Require(contourShader != null,
                "ITEM_RARITY_CONTOUR_SHADER_MISSING");
            Require(spillShader != null,
                "ITEM_RARITY_SPILL_SHADER_MISSING");

            Dictionary<string, Dictionary<string, Material>> materials =
                new();
            foreach (TierSpec spec in TierSpecs)
            {
                Dictionary<string, Material> tierMaterials = new();
                foreach (string role in ContourShaderRoles)
                {
                    Material material = AuthorMaterial(
                        MaterialPath(spec.Key, role),
                        contourShader);
                    ConfigureContourMaterial(material, spec, role);
                    tierMaterials.Add(role, material);
                }
                Material spill = AuthorMaterial(
                    MaterialPath(spec.Key, SpillRole),
                    spillShader);
                ConfigureSpillMaterial(spill, spec);
                tierMaterials.Add(SpillRole, spill);
                materials.Add(spec.Key, tierMaterials);
            }

            ItemRarityContourBloomProfile profile = AuthorProfile(materials);
            AuthorPrefab(profile, materials[
                ItemRarityContourBloomProfile.WhiteKey]);

            AssetDatabase.SaveAssets();
            ValidateShaders(contourShader, spillShader);
            ValidateProfile(profile);
            ValidatePrefab(profile);
            ValidateApplyClearAndSharedMaterials(profile);
            ValidateRuntimeBoundaries();

            Debug.Log(TerminalMarker
                      + " prefab=1 profile=1 shaders=2 materials=35"
                      + " authoredLayers=7 raycastTargets=0"
                      + " cardWrites=0 sceneWrites=0 p2Bindings=0");
        }

        private static Material AuthorMaterial(string path, Shader shader)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader)
                {
                    name = Path.GetFileNameWithoutExtension(path)
                };
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.shader = shader;
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static void ConfigureContourMaterial(
            Material material,
            TierSpec spec,
            string role)
        {
            material.SetColor("_BandColorA", ParseColor(spec.BaseColor));
            material.SetColor("_BandColorB", ParseColor(spec.MiddleColor));
            material.SetColor("_HighlightColor", ParseColor(spec.HighlightColor));
            material.SetFloat("_OutlineRadiusTexels", 15f);
            material.SetFloat("_InnerEdgeStrength", 0.18f);
            material.SetFloat("_HaloStrength", 0.7f);
            material.SetFloat("_BandOpacity", 1.35f);
            material.SetFloat("_Brightness", 1.2f);
            material.SetFloat("_BodyEmissionStrength", 0f);
            material.SetFloat("_BodyWarmth", 0f);
            material.SetFloat("_GlowFalloff", 0.58f);
            material.SetFloat("_FlowSpeed", spec.FlowSpeed);
            material.SetFloat("_FlowScale", 2.6f);
            material.SetFloat("_AngularBlend", 0.78f);
            material.SetFloat("_NoiseAmount", 0.08f);
            material.SetFloat("_HighlightStrength", 1.55f);
            material.SetFloat("_UseGradientLut", 0f);
            material.SetFloat("_UseNoiseTexture", 0f);
            material.SetFloat("_UseRingTexture", 0f);
            material.SetFloat(
                "_SrcBlend",
                (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetFloat(
                "_DstBlend",
                (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

            switch (role)
            {
                case PersistentHaloRole:
                    material.SetFloat("_OutlineRadiusTexels", 20f);
                    material.SetFloat("_InnerEdgeStrength", 0f);
                    material.SetFloat("_HaloStrength", 0.9f);
                    material.SetFloat("_BandOpacity", 0.837f);
                    material.SetFloat("_Brightness", 1.26f);
                    break;
                case WholeBodyEmissionRole:
                    ConfigureWholeBodyMaterial(
                        material,
                        spec,
                        0.25f,
                        0f,
                        1f,
                        0.58f);
                    break;
                case InnerGlowRole:
                    ConfigureWholeBodyMaterial(
                        material,
                        spec,
                        7f,
                        1.2f,
                        0f,
                        0.82f);
                    break;
                case OuterHaloRole:
                    ConfigureWholeBodyMaterial(
                        material,
                        spec,
                        32f,
                        0.86f,
                        0f,
                        0.42f);
                    break;
                case TriggerRole:
                    material.SetFloat("_OutlineRadiusTexels", 24f);
                    material.SetFloat("_InnerEdgeStrength", 0.24f);
                    material.SetFloat("_HaloStrength", 1.25f);
                    material.SetFloat("_BandOpacity", 1.7f);
                    material.SetFloat("_Brightness", 1.7f);
                    break;
                case ContourRole:
                    break;
                default:
                    throw new InvalidOperationException(
                        "ITEM_RARITY_UNKNOWN_CONTOUR_ROLE " + role);
            }
            EditorUtility.SetDirty(material);
        }

        private static void ConfigureWholeBodyMaterial(
            Material material,
            TierSpec spec,
            float outlineRadius,
            float haloStrength,
            float bodyEmissionStrength,
            float glowFalloff)
        {
            material.SetFloat("_OutlineRadiusTexels", outlineRadius);
            material.SetFloat("_InnerEdgeStrength", 0f);
            material.SetFloat("_HaloStrength", haloStrength);
            material.SetFloat("_BandOpacity", 0f);
            material.SetFloat("_Brightness", 1f);
            material.SetFloat(
                "_BodyEmissionStrength",
                bodyEmissionStrength);
            material.SetFloat("_BodyWarmth", spec.BodyWarmth);
            material.SetFloat("_GlowFalloff", glowFalloff);
            material.SetFloat(
                "_DstBlend",
                (float)UnityEngine.Rendering.BlendMode.One);
        }

        private static void ConfigureSpillMaterial(
            Material material,
            TierSpec spec)
        {
            Color baseColor = ParseColor(spec.BaseColor);
            Color middleColor = ParseColor(spec.MiddleColor);
            Color highlightColor = ParseColor(spec.HighlightColor);
            material.SetColor(
                "_InnerColor",
                Color.Lerp(middleColor, highlightColor, 0.45f));
            material.SetColor(
                "_OuterColor",
                Color.Lerp(baseColor, middleColor, 0.22f));
            material.SetFloat("_InnerRadius", 0.16f);
            material.SetFloat("_OuterRadius", 0.52f);
            material.SetFloat("_SpillStrength", 1f);
            EditorUtility.SetDirty(material);
        }

        private static ItemRarityContourBloomProfile AuthorProfile(
            IReadOnlyDictionary<
                string,
                Dictionary<string, Material>> materials)
        {
            ItemRarityContourBloomProfile profile =
                AssetDatabase.LoadAssetAtPath<
                    ItemRarityContourBloomProfile>(ProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<
                    ItemRarityContourBloomProfile>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }

            SerializedObject serialized = new(profile);
            serialized.FindProperty("profileId").stringValue =
                ItemRarityContourBloomProfile.ProfileIdValue;
            foreach (TierSpec spec in TierSpecs)
            {
                SerializedProperty appearance = serialized.FindProperty(spec.Key);
                Require(appearance != null,
                    "ITEM_RARITY_PROFILE_APPEARANCE_MISSING " + spec.Key);
                appearance.FindPropertyRelative("rarityKey").stringValue = spec.Key;
                appearance.FindPropertyRelative("contourMaterial")
                    .objectReferenceValue = materials[spec.Key][ContourRole];
                appearance.FindPropertyRelative("persistentHaloMaterial")
                    .objectReferenceValue =
                    materials[spec.Key][PersistentHaloRole];
                appearance.FindPropertyRelative("wholeBodyEmissionMaterial")
                    .objectReferenceValue =
                    materials[spec.Key][WholeBodyEmissionRole];
                appearance.FindPropertyRelative("innerGlowMaterial")
                    .objectReferenceValue =
                    materials[spec.Key][InnerGlowRole];
                appearance.FindPropertyRelative("outerHaloMaterial")
                    .objectReferenceValue =
                    materials[spec.Key][OuterHaloRole];
                appearance.FindPropertyRelative("spillMaterial")
                    .objectReferenceValue = materials[spec.Key][SpillRole];
                appearance.FindPropertyRelative("triggerMaterial")
                    .objectReferenceValue = materials[spec.Key][TriggerRole];
                appearance.FindPropertyRelative("persistentAlpha").floatValue =
                    spec.PersistentAlpha;
                appearance.FindPropertyRelative("persistentHaloAlpha")
                    .floatValue = spec.PersistentHaloAlpha;
                appearance.FindPropertyRelative("persistentHaloScale")
                    .floatValue = 1f;
                appearance.FindPropertyRelative("breathAmount").floatValue =
                    spec.BreathAmount;
                appearance.FindPropertyRelative("breathCyclesPerSecond")
                    .floatValue = spec.BreathCyclesPerSecond;
                appearance.FindPropertyRelative("wholeBodyPersistentAlpha")
                    .floatValue = spec.WholeBodyPersistentAlpha;
                appearance.FindPropertyRelative("wholeBodyTriggerAlpha")
                    .floatValue = spec.WholeBodyTriggerAlpha;
                appearance.FindPropertyRelative("innerGlowAlpha").floatValue =
                    spec.InnerGlowAlpha;
                appearance.FindPropertyRelative("outerHaloAlpha").floatValue =
                    spec.OuterHaloAlpha;
                appearance.FindPropertyRelative("wholeBodyBreathAmount")
                    .floatValue = spec.WholeBodyBreathAmount;
                appearance.FindPropertyRelative(
                        "wholeBodyBreathCyclesPerSecond")
                    .floatValue = spec.WholeBodyBreathCyclesPerSecond;
                appearance.FindPropertyRelative("wholeBodyTriggerEndScale")
                    .floatValue = 1.2f;
                appearance.FindPropertyRelative(
                        "environmentSpillPersistentAlpha")
                    .floatValue = spec.EnvironmentSpillPersistentAlpha;
                appearance.FindPropertyRelative(
                        "environmentSpillTriggerAlpha")
                    .floatValue = spec.EnvironmentSpillTriggerAlpha;
                appearance.FindPropertyRelative("environmentSpillBaseScale")
                    .floatValue = 1.75f;
                appearance.FindPropertyRelative("environmentSpillTriggerScale")
                    .floatValue = 2.15f;
                appearance.FindPropertyRelative("triggerAlpha").floatValue =
                    spec.TriggerAlpha;
                appearance.FindPropertyRelative("triggerAttackSeconds")
                    .floatValue = 0.08f;
                appearance.FindPropertyRelative("triggerSpreadSeconds")
                    .floatValue = 0.25f;
                appearance.FindPropertyRelative("triggerSettleSeconds")
                    .floatValue = 0.55f;
                appearance.FindPropertyRelative("triggerEndScale")
                    .floatValue = 1.22f;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void AuthorPrefab(
            ItemRarityContourBloomProfile profile,
            IReadOnlyDictionary<string, Material> defaultMaterials)
        {
            GameObject root = new(
                "ItemRarityContourBloomVfx",
                typeof(RectTransform),
                typeof(ItemRarityContourBloomVfx));
            try
            {
                RectTransform rootRect = root.GetComponent<RectTransform>();
                Stretch(rootRect);

                Image contour = CreateLayer(
                    root.transform,
                    ItemRarityContourBloomVfx.ContourLayerName,
                    defaultMaterials[ContourRole],
                    1f);
                Image persistentHalo = CreateLayer(
                    root.transform,
                    ItemRarityContourBloomVfx.PersistentHaloLayerName,
                    defaultMaterials[PersistentHaloRole],
                    1f);
                Image environmentSpill = CreateLayer(
                    root.transform,
                    ItemRarityContourBloomVfx.EnvironmentSpillLayerName,
                    defaultMaterials[SpillRole],
                    1.75f);
                Image outerHalo = CreateLayer(
                    root.transform,
                    ItemRarityContourBloomVfx.OuterHaloLayerName,
                    defaultMaterials[OuterHaloRole],
                    1f);
                Image innerHalo = CreateLayer(
                    root.transform,
                    ItemRarityContourBloomVfx.InnerHaloLayerName,
                    defaultMaterials[InnerGlowRole],
                    1f);
                Image bodyBloom = CreateLayer(
                    root.transform,
                    ItemRarityContourBloomVfx.BodyBloomLayerName,
                    defaultMaterials[WholeBodyEmissionRole],
                    1f);
                Image trigger = CreateLayer(
                    root.transform,
                    ItemRarityContourBloomVfx.TriggerLayerName,
                    defaultMaterials[TriggerRole],
                    1f);
                trigger.gameObject.SetActive(false);

                SerializedObject serialized = new(
                    root.GetComponent<ItemRarityContourBloomVfx>());
                serialized.FindProperty("profile").objectReferenceValue = profile;
                serialized.FindProperty("contourImage")
                    .objectReferenceValue = contour;
                serialized.FindProperty("persistentHaloImage")
                    .objectReferenceValue = persistentHalo;
                serialized.FindProperty("environmentSpillImage")
                    .objectReferenceValue = environmentSpill;
                serialized.FindProperty("outerHaloImage")
                    .objectReferenceValue = outerHalo;
                serialized.FindProperty("innerHaloImage")
                    .objectReferenceValue = innerHalo;
                serialized.FindProperty("bodyBloomImage")
                    .objectReferenceValue = bodyBloom;
                serialized.FindProperty("triggerImage")
                    .objectReferenceValue = trigger;
                serialized.FindProperty("persistentHaloRect")
                    .objectReferenceValue = persistentHalo.rectTransform;
                serialized.FindProperty("environmentSpillRect")
                    .objectReferenceValue = environmentSpill.rectTransform;
                serialized.FindProperty("outerHaloRect")
                    .objectReferenceValue = outerHalo.rectTransform;
                serialized.FindProperty("triggerRect")
                    .objectReferenceValue = trigger.rectTransform;
                serialized.ApplyModifiedPropertiesWithoutUndo();

                GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                Require(saved != null,
                    "ITEM_RARITY_PREFAB_SAVE_FAILED");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static Image CreateLayer(
            Transform parent,
            string name,
            Material material,
            float scale)
        {
            GameObject layer = new(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            layer.transform.SetParent(parent, false);
            RectTransform rect = layer.GetComponent<RectTransform>();
            Stretch(rect);
            rect.localScale = new Vector3(scale, scale, 1f);
            Image image = layer.GetComponent<Image>();
            image.sprite = null;
            image.overrideSprite = null;
            image.material = material;
            image.color = Color.white;
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            image.fillCenter = true;
            image.raycastTarget = false;
            image.maskable = true;
            image.enabled = false;
            return image;
        }

        private static void ValidateShaders(
            Shader contourShader,
            Shader spillShader)
        {
            Require(!ShaderUtil.ShaderHasError(contourShader),
                "ITEM_RARITY_CONTOUR_SHADER_COMPILE_ERROR");
            Require(!ShaderUtil.ShaderHasError(spillShader),
                "ITEM_RARITY_SPILL_SHADER_COMPILE_ERROR");
        }

        private static void ValidateProfile(
            ItemRarityContourBloomProfile profile)
        {
            Require(profile != null && profile.ValidateAuthoredReferences(),
                "ITEM_RARITY_PROFILE_INVALID");

            HashSet<Material> allMaterials = new();
            foreach (TierSpec spec in TierSpecs)
            {
                Require(profile.TryResolve(spec.Key, out var appearance),
                    "ITEM_RARITY_PROFILE_RESOLVE_FAILED " + spec.Key);
                IReadOnlyDictionary<string, Material> appearanceMaterials =
                    AppearanceMaterials(appearance);
                foreach (string role in AllRoles)
                {
                    Material material = appearanceMaterials[role];
                    Require(material != null
                            && string.Equals(
                                AssetDatabase.GetAssetPath(material),
                                MaterialPath(spec.Key, role),
                                StringComparison.Ordinal),
                        "ITEM_RARITY_MATERIAL_PATH_INVALID "
                        + spec.Key + " role=" + role);
                    string expectedShader = string.Equals(
                        role,
                        SpillRole,
                        StringComparison.Ordinal)
                        ? SpillShaderName
                        : ContourShaderName;
                    Require(string.Equals(
                                material.shader.name,
                                expectedShader,
                                StringComparison.Ordinal),
                        "ITEM_RARITY_MATERIAL_SHADER_INVALID "
                        + spec.Key + " role=" + role);
                    Require(allMaterials.Add(material),
                        "ITEM_RARITY_MATERIAL_NOT_UNIQUE "
                        + spec.Key + " role=" + role);
                }
            }

            Require(allMaterials.Count == 35,
                "ITEM_RARITY_MATERIAL_REFERENCE_COUNT_INVALID");
            Require(AssetDatabase.FindAssets(
                    "t:Material", new[] { MaterialFolder }).Length == 35,
                "ITEM_RARITY_AUTHORED_MATERIAL_ASSET_COUNT_INVALID");
            Require(!profile.TryResolve("", out _)
                    && !profile.TryResolve("unknown", out _)
                    && !profile.TryResolve("lihuo", out _),
                "ITEM_RARITY_UNKNOWN_KEY_NOT_FAIL_CLOSED");
        }

        private static void ValidatePrefab(
            ItemRarityContourBloomProfile profile)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                ItemRarityContourBloomVfx vfx =
                    root.GetComponent<ItemRarityContourBloomVfx>();
                Require(vfx != null && vfx.Profile == profile,
                    "ITEM_RARITY_PREFAB_PROFILE_REFERENCE_INVALID");
                Require(vfx.ValidateAuthoredReferences(),
                    "ITEM_RARITY_PREFAB_REFERENCE_INVALID");
                Image[] graphics = root.GetComponentsInChildren<Image>(true);
                Require(graphics.Length == 7,
                    "ITEM_RARITY_PREFAB_GRAPHIC_COUNT_INVALID");
                string[] expectedNames =
                {
                    ItemRarityContourBloomVfx.ContourLayerName,
                    ItemRarityContourBloomVfx.PersistentHaloLayerName,
                    ItemRarityContourBloomVfx.EnvironmentSpillLayerName,
                    ItemRarityContourBloomVfx.OuterHaloLayerName,
                    ItemRarityContourBloomVfx.InnerHaloLayerName,
                    ItemRarityContourBloomVfx.BodyBloomLayerName,
                    ItemRarityContourBloomVfx.TriggerLayerName
                };
                Require(graphics.Select(value => value.gameObject.name)
                        .SequenceEqual(expectedNames),
                    "ITEM_RARITY_PREFAB_LAYER_ORDER_INVALID");
                Require(graphics.All(value =>
                            !value.raycastTarget
                            && value.maskable
                            && value.material != null
                            && value.sprite == null
                            && !value.enabled),
                    "ITEM_RARITY_PREFAB_GRAPHIC_AUTHORING_INVALID");
                Require(!graphics.Single(value => string.Equals(
                            value.gameObject.name,
                            ItemRarityContourBloomVfx.TriggerLayerName,
                            StringComparison.Ordinal)).gameObject.activeSelf
                        && graphics.Where(value => !string.Equals(
                                value.gameObject.name,
                                ItemRarityContourBloomVfx.TriggerLayerName,
                                StringComparison.Ordinal))
                            .All(value => value.gameObject.activeSelf),
                    "ITEM_RARITY_PREFAB_LAYER_ACTIVE_STATE_INVALID");
                Require(GameObjectUtility
                            .GetMonoBehavioursWithMissingScriptCount(root) == 0,
                    "ITEM_RARITY_PREFAB_MISSING_SCRIPT");
                Require(!AssetDatabase.GetDependencies(PrefabPath, true).Any(
                        value => value.EndsWith(
                            ".unity",
                            StringComparison.OrdinalIgnoreCase)),
                    "ITEM_RARITY_PREFAB_SCENE_DEPENDENCY");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ValidateApplyClearAndSharedMaterials(
            ItemRarityContourBloomProfile profile)
        {
            GameObject first = PrefabUtility.LoadPrefabContents(PrefabPath);
            GameObject second = PrefabUtility.LoadPrefabContents(PrefabPath);
            Texture2D texture = null;
            Sprite sprite = null;
            try
            {
                texture = new Texture2D(8, 8, TextureFormat.RGBA32, false);
                Color[] pixels = Enumerable.Repeat(Color.clear, 64).ToArray();
                for (int y = 2; y <= 5; y++)
                {
                    for (int x = 2; x <= 5; x++)
                    {
                        pixels[y * 8 + x] = Color.white;
                    }
                }
                texture.SetPixels(pixels);
                texture.Apply(false, false);
                sprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, 8f, 8f),
                    new Vector2(0.5f, 0.5f),
                    8f);

                ItemRarityContourBloomVfx firstVfx =
                    first.GetComponent<ItemRarityContourBloomVfx>();
                ItemRarityContourBloomVfx secondVfx =
                    second.GetComponent<ItemRarityContourBloomVfx>();
                int materialCountBefore =
                    Resources.FindObjectsOfTypeAll<Material>().Length;
                Require(firstVfx.Apply(sprite, "white")
                        && secondVfx.Apply(sprite, "white"),
                    "ITEM_RARITY_MULTI_INSTANCE_APPLY_FAILED");
                Require(profile.TryResolve("white", out var white),
                    "ITEM_RARITY_WHITE_PROFILE_MISSING");
                Require(UsesOnlyProfileMaterials(first, white)
                        && UsesOnlyProfileMaterials(second, white),
                    "ITEM_RARITY_MULTI_INSTANCE_SHARED_MATERIAL_FAILED");
                Require(PersistentLayersAreVisible(first)
                        && PersistentLayersAreVisible(second)
                        && !firstVfx.IsTriggerActive,
                    "ITEM_RARITY_PERSISTENT_SEVEN_LAYER_STATE_INVALID");

                firstVfx.PlayTrigger(10f);
                Require(firstVfx.IsTriggerActive
                        && TriggerLayer(first).gameObject.activeSelf,
                    "ITEM_RARITY_TRIGGER_DID_NOT_START");
                firstVfx.EvaluateAt(10.08f);
                Require(TriggerLayer(first).canvasRenderer.GetAlpha() > 0f,
                    "ITEM_RARITY_TRIGGER_ATTACK_INVALID");
                firstVfx.EvaluateAt(10.25f);
                Require(firstVfx.IsTriggerActive
                        && TriggerLayer(first).rectTransform.localScale.x > 1f,
                    "ITEM_RARITY_TRIGGER_SPREAD_INVALID");
                firstVfx.PlayTrigger(20f);
                firstVfx.EvaluateAt(20.08f);
                Require(firstVfx.IsTriggerActive,
                    "ITEM_RARITY_TRIGGER_REENTRY_INVALID");
                firstVfx.EvaluateAt(20.551f);
                Require(!firstVfx.IsTriggerActive
                        && !TriggerLayer(first).gameObject.activeSelf
                        && PersistentLayersAreVisible(first),
                    "ITEM_RARITY_TRIGGER_SETTLE_INVALID");

                Require(firstVfx.Apply(sprite, "green")
                        && firstVfx.Apply(sprite, "blue"),
                    "ITEM_RARITY_REPEAT_APPLY_FAILED");
                Require(profile.TryResolve("blue", out var blue)
                        && UsesOnlyProfileMaterials(first, blue),
                    "ITEM_RARITY_REPEAT_APPLY_STALE_MATERIAL");
                firstVfx.Clear();
                firstVfx.Clear();
                Require(!firstVfx.HasAppliedPresentation
                        && first.GetComponentsInChildren<Image>(true).All(
                            RuntimeGraphicIsCleared),
                    "ITEM_RARITY_CLEAR_NOT_IDEMPOTENT");
                Require(!firstVfx.Apply(sprite, "unsupported")
                        && !firstVfx.HasAppliedPresentation,
                    "ITEM_RARITY_UNSUPPORTED_KEY_NOT_FAIL_CLOSED");
                int materialCountAfter =
                    Resources.FindObjectsOfTypeAll<Material>().Length;
                Require(materialCountAfter == materialCountBefore,
                    "ITEM_RARITY_RUNTIME_MATERIAL_COUNT_GREW");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(first);
                PrefabUtility.UnloadPrefabContents(second);
                if (sprite != null)
                {
                    UnityEngine.Object.DestroyImmediate(sprite);
                }
                if (texture != null)
                {
                    UnityEngine.Object.DestroyImmediate(texture);
                }
            }
        }

        private static bool UsesOnlyProfileMaterials(
            GameObject root,
            ItemRarityContourBloomAppearance appearance)
        {
            IReadOnlyDictionary<string, Material> materials =
                AppearanceMaterials(appearance);
            foreach (Image image in root.GetComponentsInChildren<Image>(true))
            {
                string role = RoleForLayerName(image.gameObject.name);
                if (string.IsNullOrEmpty(role))
                {
                    return false;
                }
                Material expected = materials[role];
                if (image.material != expected)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool PersistentLayersAreVisible(GameObject root)
        {
            foreach (Image image in root.GetComponentsInChildren<Image>(true))
            {
                if (string.Equals(
                        image.gameObject.name,
                        ItemRarityContourBloomVfx.TriggerLayerName,
                        StringComparison.Ordinal))
                {
                    continue;
                }
                if (!image.enabled
                    || image.sprite == null
                    || image.material == null
                    || image.canvasRenderer.GetAlpha() <= 0f)
                {
                    return false;
                }
            }
            return true;
        }

        private static Image TriggerLayer(GameObject root)
        {
            return root.GetComponentsInChildren<Image>(true).Single(
                value => string.Equals(
                    value.gameObject.name,
                    ItemRarityContourBloomVfx.TriggerLayerName,
                    StringComparison.Ordinal));
        }

        private static IReadOnlyDictionary<string, Material>
            AppearanceMaterials(ItemRarityContourBloomAppearance appearance)
        {
            return new Dictionary<string, Material>
            {
                { ContourRole, appearance.ContourMaterial },
                { PersistentHaloRole, appearance.PersistentHaloMaterial },
                { WholeBodyEmissionRole, appearance.WholeBodyEmissionMaterial },
                { InnerGlowRole, appearance.InnerGlowMaterial },
                { OuterHaloRole, appearance.OuterHaloMaterial },
                { SpillRole, appearance.SpillMaterial },
                { TriggerRole, appearance.TriggerMaterial }
            };
        }

        private static string RoleForLayerName(string layerName)
        {
            if (string.Equals(layerName,
                    ItemRarityContourBloomVfx.ContourLayerName,
                    StringComparison.Ordinal))
            {
                return ContourRole;
            }
            if (string.Equals(layerName,
                    ItemRarityContourBloomVfx.PersistentHaloLayerName,
                    StringComparison.Ordinal))
            {
                return PersistentHaloRole;
            }
            if (string.Equals(layerName,
                    ItemRarityContourBloomVfx.BodyBloomLayerName,
                    StringComparison.Ordinal))
            {
                return WholeBodyEmissionRole;
            }
            if (string.Equals(layerName,
                    ItemRarityContourBloomVfx.InnerHaloLayerName,
                    StringComparison.Ordinal))
            {
                return InnerGlowRole;
            }
            if (string.Equals(layerName,
                    ItemRarityContourBloomVfx.OuterHaloLayerName,
                    StringComparison.Ordinal))
            {
                return OuterHaloRole;
            }
            if (string.Equals(layerName,
                    ItemRarityContourBloomVfx.EnvironmentSpillLayerName,
                    StringComparison.Ordinal))
            {
                return SpillRole;
            }
            return string.Equals(layerName,
                ItemRarityContourBloomVfx.TriggerLayerName,
                StringComparison.Ordinal)
                ? TriggerRole
                : string.Empty;
        }

        private static bool RuntimeGraphicIsCleared(Image image)
        {
            if (image == null || image.enabled || image.sprite != null)
            {
                return false;
            }

            SerializedObject serialized = new(image);
            SerializedProperty authoredMaterial =
                serialized.FindProperty("m_Material");
            return authoredMaterial != null
                   && authoredMaterial.objectReferenceValue == null;
        }

        private static void ValidateRuntimeBoundaries()
        {
            foreach (string path in new[] { RuntimeSourcePath, ProfileSourcePath })
            {
                Require(File.Exists(path),
                    "ITEM_RARITY_RUNTIME_SOURCE_MISSING " + path);
                string source = File.ReadAllText(path);
                foreach (string forbidden in ForbiddenRuntimeTokens)
                {
                    Require(source.IndexOf(
                                forbidden,
                                StringComparison.Ordinal) < 0,
                        "ITEM_RARITY_FORBIDDEN_RUNTIME_TOKEN "
                        + forbidden + " path=" + path);
                }
            }
        }

        private static string MaterialPath(string rarityKey, string role)
        {
            return MaterialFolder + "/ItemRarity_"
                   + rarityKey + "_" + role + ".mat";
        }

        private static Color ParseColor(string html)
        {
            Require(ColorUtility.TryParseHtmlString(html, out Color color),
                "ITEM_RARITY_COLOR_INVALID " + html);
            return color;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
        }

        private static void EnsureFolder(string assetPath)
        {
            string normalized = assetPath.Replace('\\', '/');
            if (AssetDatabase.IsValidFolder(normalized))
            {
                return;
            }

            string parent = Path.GetDirectoryName(normalized)
                ?.Replace('\\', '/');
            string leaf = Path.GetFileName(normalized);
            Require(!string.IsNullOrEmpty(parent)
                    && !string.IsNullOrEmpty(leaf),
                "ITEM_RARITY_FOLDER_PATH_INVALID " + assetPath);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        private static void Require(bool condition, string diagnostic)
        {
            if (!condition)
            {
                throw new InvalidOperationException(
                    diagnostic ?? "ITEM_RARITY_AUTHORING_ASSERTION_FAILED");
            }
        }

        private sealed class TierSpec
        {
            public TierSpec(
                string key,
                string baseColor,
                string middleColor,
                string highlightColor,
                float persistentAlpha,
                float persistentHaloAlpha,
                float wholeBodyPersistentAlpha,
                float innerGlowAlpha,
                float outerHaloAlpha,
                float environmentSpillPersistentAlpha,
                float triggerAlpha,
                float wholeBodyTriggerAlpha,
                float environmentSpillTriggerAlpha,
                float flowSpeed,
                float breathAmount,
                float breathCyclesPerSecond,
                float wholeBodyBreathAmount,
                float wholeBodyBreathCyclesPerSecond,
                float bodyWarmth)
            {
                Key = key;
                BaseColor = baseColor;
                MiddleColor = middleColor;
                HighlightColor = highlightColor;
                PersistentAlpha = persistentAlpha;
                PersistentHaloAlpha = persistentHaloAlpha;
                WholeBodyPersistentAlpha = wholeBodyPersistentAlpha;
                InnerGlowAlpha = innerGlowAlpha;
                OuterHaloAlpha = outerHaloAlpha;
                EnvironmentSpillPersistentAlpha =
                    environmentSpillPersistentAlpha;
                TriggerAlpha = triggerAlpha;
                WholeBodyTriggerAlpha = wholeBodyTriggerAlpha;
                EnvironmentSpillTriggerAlpha =
                    environmentSpillTriggerAlpha;
                FlowSpeed = flowSpeed;
                BreathAmount = breathAmount;
                BreathCyclesPerSecond = breathCyclesPerSecond;
                WholeBodyBreathAmount = wholeBodyBreathAmount;
                WholeBodyBreathCyclesPerSecond =
                    wholeBodyBreathCyclesPerSecond;
                BodyWarmth = bodyWarmth;
            }

            public string Key { get; }
            public string BaseColor { get; }
            public string MiddleColor { get; }
            public string HighlightColor { get; }
            public float PersistentAlpha { get; }
            public float PersistentHaloAlpha { get; }
            public float WholeBodyPersistentAlpha { get; }
            public float InnerGlowAlpha { get; }
            public float OuterHaloAlpha { get; }
            public float EnvironmentSpillPersistentAlpha { get; }
            public float TriggerAlpha { get; }
            public float WholeBodyTriggerAlpha { get; }
            public float EnvironmentSpillTriggerAlpha { get; }
            public float FlowSpeed { get; }
            public float BreathAmount { get; }
            public float BreathCyclesPerSecond { get; }
            public float WholeBodyBreathAmount { get; }
            public float WholeBodyBreathCyclesPerSecond { get; }
            public float BodyWarmth { get; }
        }
    }
}
