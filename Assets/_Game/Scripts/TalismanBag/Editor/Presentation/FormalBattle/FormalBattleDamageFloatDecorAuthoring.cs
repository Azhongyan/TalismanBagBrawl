using System;
using System.Linq;
using TMPro;
using TalismanBag.Presentation.FormalBattle;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.Editor.Presentation.FormalBattle
{
    public static class FormalBattleDamageFloatDecorAuthoring
    {
        private const string ProfilePath =
            "Assets/_Game/Resources/V04/FormalBattlePresentation/"
            + "FormalBattlePresentationProfile.asset";
        private const string PlayerPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/BattlePresentation/"
            + "FormalBattlePlayerPresentation.prefab";
        private const string EnemyPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/BattlePresentation/"
            + "FormalBattleEnemySlot.prefab";
        private const string DamageFloatPoolPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/BattlePresentation/"
            + "FormalBattleDamageFloatPool.prefab";
        private const string FeedbackLayerPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/BattlePresentation/"
            + "FormalBattleFeedbackLayer.prefab";

        [MenuItem(
            "TalismanBag/V0.4/Formal Battle/Refresh Damage Float Decor Anchors")]
        public static void ExecuteFromMenu()
        {
            ExecuteInternal();
        }

        public static void ExecuteFromCommandLine()
        {
            try
            {
                ExecuteInternal();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        private static void ExecuteInternal()
        {
            FormalBattlePresentationProfile profile =
                AssetDatabase.LoadAssetAtPath<
                    FormalBattlePresentationProfile>(ProfilePath);
            Require(profile != null, "DAMAGE_FLOAT_PROFILE_MISSING");

            EnsureSemanticStyles(profile);
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();

            AuthorPlayerAnchor(profile);
            AuthorEnemyAnchor(profile);
            SaveDamageFloatPoolDefaults();
            DisableDetachedPoolPreview();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Require(profile.ValidateAuthoredReferences(),
                "DAMAGE_FLOAT_PROFILE_INVALID");
            Debug.Log(
                "FORMAL_BATTLE_DAMAGE_FLOAT_DECOR_AUTHORING_PASS");
        }

        private static void EnsureSemanticStyles(
            FormalBattlePresentationProfile profile)
        {
            FormalBattleDamageFloatVisualStyle[] existing =
                profile.GetDamageFloatStylesForEditor();
            bool valid = existing.Length
                         == FormalBattleDamageFloatStyleKeys.All.Length
                         && existing.All(value => value != null
                             && value.Validate())
                         && existing.Select(value => value.StyleKey)
                             .SequenceEqual(
                                 FormalBattleDamageFloatStyleKeys.All,
                                 StringComparer.Ordinal);
            if (valid)
            {
                return;
            }

            profile.AssignDamageFloatVisualsForEditor(new[]
            {
                Style(
                    FormalBattleDamageFloatStyleKeys.Damage,
                    new Color(1f, 0.89f, 0.54f, 1f),
                    new Color(1f, 0.40f, 0.18f, 1f),
                    new Color(0.68f, 0.06f, 0.13f, 1f)),
                Style(
                    FormalBattleDamageFloatStyleKeys.Heal,
                    new Color(0.90f, 1f, 0.71f, 1f),
                    new Color(0.35f, 0.85f, 0.53f, 1f),
                    new Color(0.14f, 0.49f, 0.34f, 1f)),
                Style(
                    FormalBattleDamageFloatStyleKeys.Guard,
                    new Color(0.78f, 0.93f, 1f, 1f),
                    new Color(0.33f, 0.68f, 0.92f, 1f),
                    new Color(0.14f, 0.35f, 0.66f, 1f)),
                Style(
                    FormalBattleDamageFloatStyleKeys.Shell,
                    new Color(0.85f, 0.96f, 1f, 1f),
                    new Color(0.45f, 0.79f, 0.91f, 1f),
                    new Color(0.18f, 0.46f, 0.62f, 1f)),
                Style(
                    FormalBattleDamageFloatStyleKeys.Nian,
                    new Color(0.89f, 0.83f, 1f, 1f),
                    new Color(0.55f, 0.45f, 0.91f, 1f),
                    new Color(0.32f, 0.23f, 0.64f, 1f)),
                Style(
                    FormalBattleDamageFloatStyleKeys.Cleanse,
                    new Color(0.96f, 1f, 1f, 1f),
                    new Color(0.56f, 0.88f, 0.89f, 1f),
                    new Color(0.24f, 0.60f, 0.64f, 1f)),
                Style(
                    FormalBattleDamageFloatStyleKeys.Control,
                    new Color(1f, 0.82f, 0.63f, 1f),
                    new Color(0.93f, 0.46f, 0.27f, 1f),
                    new Color(0.66f, 0.18f, 0.20f, 1f))
            });
        }

        private static FormalBattleDamageFloatVisualStyle Style(
            string key,
            Color top,
            Color middle,
            Color bottom)
        {
            FormalBattleDamageFloatVisualStyle style =
                new FormalBattleDamageFloatVisualStyle();
            style.AssignForEditor(key, top, middle, bottom);
            return style;
        }

        private static void AuthorPlayerAnchor(
            FormalBattlePresentationProfile profile)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(
                PlayerPrefabPath);
            try
            {
                FormalBattlePlayerPresentationView view =
                    root.GetComponent<FormalBattlePlayerPresentationView>();
                Require(view != null, "PLAYER_PRESENTATION_VIEW_MISSING");
                RectTransform source = view.DamageAnchor;
                Require(source != null, "PLAYER_DAMAGE_SOURCE_MISSING");
                RectTransform anchor = EnsureAnchor(
                    view.transform as RectTransform,
                    source,
                    "PlayerDamageFloatAnchor",
                    "-82",
                    FormalBattleDamageFloatStyleKeys.Damage,
                    Vector2.zero,
                    profile);
                AssignAnchor(view, "damageAnchor", anchor);

                SerializedObject serializedView = new SerializedObject(view);
                SerializedProperty statusStripProperty =
                    serializedView.FindProperty("statusStrip");
                FormalBattleStatusStripView statusStrip =
                    statusStripProperty?.objectReferenceValue
                        as FormalBattleStatusStripView;
                RectTransform benefitSource =
                    statusStrip?.transform as RectTransform;
                Require(benefitSource != null,
                    "PLAYER_BENEFIT_SOURCE_MISSING");
                RectTransform benefitAnchor = EnsureAnchor(
                    view.transform as RectTransform,
                    benefitSource,
                    "PlayerBenefitFloatAnchor",
                    "护盾 +4",
                    FormalBattleDamageFloatStyleKeys.Guard,
                    new Vector2(0f, 96f),
                    profile);
                AssignAnchor(
                    view,
                    "benefitFloatAnchor",
                    benefitAnchor);
                Require(view.ValidateAuthoredReferences(),
                    "PLAYER_FLOAT_ANCHOR_BINDING_INVALID");
                PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void AuthorEnemyAnchor(
            FormalBattlePresentationProfile profile)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(
                EnemyPrefabPath);
            try
            {
                FormalBattleEnemySlotView view =
                    root.GetComponent<FormalBattleEnemySlotView>();
                Require(view != null, "ENEMY_PRESENTATION_VIEW_MISSING");
                RectTransform source = view.DamageAnchor;
                Require(source != null, "ENEMY_DAMAGE_SOURCE_MISSING");
                RectTransform anchor = EnsureAnchor(
                    view.transform as RectTransform,
                    source,
                    "EnemyDamageFloatAnchor",
                    "-82",
                    FormalBattleDamageFloatStyleKeys.Damage,
                    Vector2.zero,
                    profile);
                AssignAnchor(view, "damageAnchor", anchor);
                RectTransform effectAnchor = EnsureAnchor(
                    view.transform as RectTransform,
                    anchor,
                    "EnemyEffectFloatAnchor",
                    "破壳 -11",
                    FormalBattleDamageFloatStyleKeys.Shell,
                    new Vector2(0f, 96f),
                    profile);
                AssignAnchor(view, "effectFloatAnchor", effectAnchor);
                Require(view.ValidateAuthoredReferences(),
                    "ENEMY_FLOAT_ANCHOR_BINDING_INVALID");
                PrefabUtility.SaveAsPrefabAsset(root, EnemyPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static RectTransform EnsureAnchor(
            RectTransform owner,
            RectTransform currentSource,
            string anchorName,
            string previewPayload,
            string previewStyleKey,
            Vector2 creationOffset,
            FormalBattlePresentationProfile profile)
        {
            Require(owner != null, "DAMAGE_FLOAT_OWNER_RECT_MISSING");
            Transform existing = owner.Find(anchorName);
            RectTransform anchor = existing as RectTransform;
            if (anchor == null)
            {
                Vector3 worldCenter = currentSource.TransformPoint(
                    currentSource.rect.center);
                GameObject anchorObject = new GameObject(
                    anchorName,
                    typeof(RectTransform),
                    typeof(FormalBattleDamageFloatAnchorPreview));
                anchor = anchorObject.GetComponent<RectTransform>();
                anchor.SetParent(owner, false);
                anchor.anchorMin = new Vector2(0.5f, 0.5f);
                anchor.anchorMax = new Vector2(0.5f, 0.5f);
                anchor.pivot = new Vector2(0.5f, 0.5f);
                anchor.sizeDelta = new Vector2(190f, 72f);
                Vector3 localCenter = owner.InverseTransformPoint(worldCenter);
                anchor.anchoredPosition = new Vector2(
                    localCenter.x + creationOffset.x,
                    localCenter.y + creationOffset.y);
            }

            FormalBattleDamageFloatAnchorPreview preview =
                anchor.GetComponent<FormalBattleDamageFloatAnchorPreview>();
            if (preview == null)
            {
                preview = anchor.gameObject.AddComponent<
                    FormalBattleDamageFloatAnchorPreview>();
            }

            TMP_Text label = anchor.GetComponentInChildren<TMP_Text>(true);
            if (label == null)
            {
                GameObject labelObject = new GameObject(
                    "DamageFloatPreview",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(TextMeshProUGUI));
                RectTransform labelRect =
                    labelObject.GetComponent<RectTransform>();
                labelRect.SetParent(anchor, false);
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = Vector2.zero;
                labelRect.offsetMax = Vector2.zero;
                label = labelObject.GetComponent<TextMeshProUGUI>();
                label.fontSize = 42f;
                label.fontStyle = FontStyles.Bold;
                label.alignment = TextAlignmentOptions.Center;
                label.enableWordWrapping = false;
                label.overflowMode = TextOverflowModes.Overflow;
                label.raycastTarget = false;
            }

            preview.AssignForEditor(
                profile,
                label,
                previewPayload,
                previewStyleKey);
            return anchor;
        }

        private static void AssignAnchor(
            UnityEngine.Object view,
            string propertyName,
            RectTransform anchor)
        {
            SerializedObject serializedView = new SerializedObject(view);
            SerializedProperty property = serializedView.FindProperty(
                propertyName);
            Require(property != null,
                "FLOAT_ANCHOR_FIELD_MISSING_" + propertyName);
            property.objectReferenceValue = anchor;
            serializedView.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(view);
        }

        private static void SaveDamageFloatPoolDefaults()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(
                DamageFloatPoolPrefabPath);
            try
            {
                FormalBattleDamageFloatPool pool =
                    root.GetComponent<FormalBattleDamageFloatPool>();
                Require(pool != null, "DAMAGE_FLOAT_POOL_MISSING");
                if (!pool.HasValidMotionForEditor())
                {
                    pool.AssignMotionForEditor(
                        new[]
                        {
                            Vector2.zero,
                            new Vector2(-32f, 12f),
                            new Vector2(32f, 24f),
                            new Vector2(-20f, 36f),
                            new Vector2(20f, 48f),
                            new Vector2(-38f, 60f),
                            new Vector2(0f, 72f),
                            new Vector2(38f, 84f),
                            new Vector2(-18f, 96f),
                            new Vector2(18f, 108f)
                        },
                        0.82f,
                        76f,
                        0.68f,
                        0.08f);
                }
                Require(pool.ValidateAuthoredReferences(),
                    "DAMAGE_FLOAT_POOL_INVALID");
                PrefabUtility.SaveAsPrefabAsset(
                    root,
                    DamageFloatPoolPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void DisableDetachedPoolPreview()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(
                FeedbackLayerPrefabPath);
            try
            {
                FormalBattleDamageFloatPool pool =
                    root.GetComponentInChildren<
                        FormalBattleDamageFloatPool>(true);
                Require(pool != null,
                    "FEEDBACK_DAMAGE_FLOAT_POOL_MISSING");
                SerializedObject serializedPool = new SerializedObject(pool);
                SerializedProperty property = serializedPool.FindProperty(
                    "showAuthoringPreview");
                Require(property != null,
                    "DAMAGE_FLOAT_PREVIEW_FIELD_MISSING");
                property.boolValue = false;
                serializedPool.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(pool);
                PrefabUtility.SaveAsPrefabAsset(
                    root,
                    FeedbackLayerPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
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
