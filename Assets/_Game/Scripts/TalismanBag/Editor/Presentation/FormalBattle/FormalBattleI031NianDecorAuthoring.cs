#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.Presentation.FormalBattle;
using TalismanBag.Presentation.Items;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.Editor.Presentation.FormalBattle
{
    public static class FormalBattleI031NianDecorAuthoring
    {
        public const string SuccessMarker =
            "FORMAL_BATTLE_I031_NIAN_DECOR_AUTHORED_PASS";

        private const string ChargeSpritePath =
            "Assets/_Game/Resources/角色敌人UI/"
            + "I031NianPeriodicChargeRing_V1.png";
        private const string RarityCarrierPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/ItemVFX/"
            + "ItemRarityContourBloomVfx.prefab";
        private const string RarityProfilePath =
            "Assets/_Game/Resources/V04/ItemPresentation/RarityVfx/"
            + "ItemRarityContourBloomProfile.asset";
        private const string PresentationProfilePath =
            "Assets/_Game/Resources/V04/FormalBattlePresentation/"
            + "FormalBattlePresentationProfile.asset";
        private const string ChargeLayerName = "PeriodicCharge";

        [MenuItem(
            "TalismanBag/V0.4/Formal Battle/"
            + "Author I031 Nian Generation Decor")]
        public static void AuthorFromMenu()
        {
            AuthorAndValidate();
        }

        public static void ExecuteFromCommandLine()
        {
            try
            {
                AuthorAndValidate();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        private static void AuthorAndValidate()
        {
            Sprite chargeSprite = ImportChargeSprite();
            AuthorSpecialSourceAppearance();
            AuthorRarityCarrier(chargeSprite);
            AuthorPresentationProfile();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ValidateAuthoredResult();
            Debug.Log(SuccessMarker);
        }

        private static Sprite ImportChargeSprite()
        {
            AssetDatabase.ImportAsset(
                ChargeSpritePath,
                ImportAssetOptions.ForceSynchronousImport
                | ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(
                ChargeSpritePath) as TextureImporter;
            Require(importer != null, "I031_CHARGE_SPRITE_IMPORTER_MISSING");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.sRGBTexture = true;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(
                ChargeSpritePath);
            Require(sprite != null, "I031_CHARGE_SPRITE_MISSING");
            return sprite;
        }

        private static void AuthorRarityCarrier(Sprite chargeSprite)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(
                RarityCarrierPrefabPath);
            try
            {
                ItemRarityContourBloomVfx carrier =
                    root.GetComponent<ItemRarityContourBloomVfx>();
                Require(carrier != null, "I031_RARITY_CARRIER_MISSING");

                Transform existing = root.transform.Find(ChargeLayerName);
                GameObject layer = existing == null
                    ? new GameObject(
                        ChargeLayerName,
                        typeof(RectTransform),
                        typeof(CanvasRenderer),
                        typeof(Image))
                    : existing.gameObject;
                RectTransform rect = layer.transform as RectTransform;
                Require(rect != null, "I031_CHARGE_RECT_MISSING");
                if (existing == null)
                {
                    rect.SetParent(root.transform, false);
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = Vector2.zero;
                    rect.localScale = Vector3.one;
                    rect.SetAsLastSibling();
                }

                Image image = layer.GetComponent<Image>();
                Require(image != null, "I031_CHARGE_IMAGE_MISSING");
                image.sprite = chargeSprite;
                image.type = Image.Type.Filled;
                image.fillMethod = Image.FillMethod.Radial360;
                image.fillOrigin = (int)Image.Origin360.Top;
                image.fillClockwise = true;
                image.fillAmount = 0f;
                image.preserveAspect = true;
                image.raycastTarget = false;
                image.maskable = true;
                image.enabled = false;
                layer.SetActive(false);
                carrier.AssignPeriodicChargeForEditor(image, rect);
                EditorUtility.SetDirty(carrier);
                PrefabUtility.SaveAsPrefabAsset(
                    root,
                    RarityCarrierPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void AuthorSpecialSourceAppearance()
        {
            ItemRarityContourBloomProfile profile =
                AssetDatabase.LoadAssetAtPath<
                    ItemRarityContourBloomProfile>(RarityProfilePath);
            Require(profile != null, "I031_RARITY_PROFILE_MISSING");
            Require(profile.TryResolve(
                    ItemRarityContourBloomProfile.WhiteKey,
                    out ItemRarityContourBloomAppearance source),
                "I031_SPECIAL_APPEARANCE_SOURCE_MISSING");
            profile.AssignSpecialSourceForEditor(source);
            EditorUtility.SetDirty(profile);
        }

        private static void AuthorPresentationProfile()
        {
            FormalBattlePresentationProfile profile =
                AssetDatabase.LoadAssetAtPath<
                    FormalBattlePresentationProfile>(
                    PresentationProfilePath);
            Require(profile != null, "I031_PRESENTATION_PROFILE_MISSING");
            List<FormalBattleCausalItemStyle> styles = profile
                .GetCausalStylesForEditor()
                .Where(value => value != null)
                .ToList();
            FormalBattleCausalItemStyle[] existing = styles
                .Where(value => string.Equals(
                    value.GrammarKey,
                    FormalBattlePresentationProfile
                        .NianResourceGainGrammarKey,
                    StringComparison.Ordinal))
                .ToArray();
            Require(existing.Length <= 1,
                "I031_NIAN_VISUAL_STYLE_DUPLICATED");
            if (existing.Length == 0)
            {
                FormalBattleCausalItemStyle style =
                    new FormalBattleCausalItemStyle();
                style.AssignForEditor(
                    FormalBattlePresentationProfile
                        .NianResourceGainGrammarKey,
                    string.Empty,
                    "NIAN_RESOURCE",
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    new Color(0.3f, 0.96f, 0.86f, 0.92f),
                    new Color(0.18f, 0.72f, 0.66f, 0.72f),
                    new Color(0.76f, 1f, 0.92f, 0.95f),
                    new Color(0.12f, 0.68f, 0.58f, 0.3f),
                    8,
                    8f,
                    1.2f,
                    7f,
                    24f,
                    1f);
                styles.Add(style);
            }

            profile.AssignCausalVisualsForEditor(
                styles.ToArray(),
                profile.CausalWindupDuration,
                profile.CausalSourceHoldDuration,
                profile.CausalRibbonDuration);
            EditorUtility.SetDirty(profile);
        }

        private static void ValidateAuthoredResult()
        {
            FormalBattlePresentationProfile profile =
                AssetDatabase.LoadAssetAtPath<
                    FormalBattlePresentationProfile>(
                    PresentationProfilePath);
            Require(profile != null
                    && profile.ValidateAuthoredReferences()
                    && profile.TryGetCausalItemStyle(
                        FormalBattlePresentationProfile
                            .NianResourceGainGrammarKey,
                        out FormalBattleCausalItemStyle style)
                    && style != null
                    && style.Validate(),
                "I031_NIAN_VISUAL_STYLE_INVALID");

            ItemRarityContourBloomProfile rarityProfile =
                AssetDatabase.LoadAssetAtPath<
                    ItemRarityContourBloomProfile>(RarityProfilePath);
            Require(rarityProfile != null
                    && rarityProfile.ValidateAuthoredReferences()
                    && rarityProfile.TryResolve(
                        ItemRarityContourBloomProfile.SpecialSourceKey,
                        out _),
                "I031_SPECIAL_APPEARANCE_INVALID");

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                RarityCarrierPrefabPath);
            ItemRarityContourBloomVfx carrier = prefab == null
                ? null
                : prefab.GetComponent<ItemRarityContourBloomVfx>();
            Transform layer = prefab == null
                ? null
                : prefab.transform.Find(ChargeLayerName);
            Image image = layer == null ? null : layer.GetComponent<Image>();
            Require(carrier != null
                    && carrier.ValidateAuthoredReferences()
                    && image != null
                    && image.sprite != null
                    && image.type == Image.Type.Filled
                    && image.fillMethod == Image.FillMethod.Radial360
                    && !image.raycastTarget,
                "I031_PERIODIC_CHARGE_CARRIER_INVALID");
        }

        private static void Require(bool condition, string diagnostic)
        {
            if (!condition)
            {
                throw new InvalidOperationException(diagnostic);
            }
        }
    }
}
#endif
