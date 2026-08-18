using System;
using System.Linq;
using TMPro;
using TalismanBag.Contracts.Battle;
using TalismanBag.Presentation.FormalBattle;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.Editor.Presentation.FormalBattle
{
    public static class FormalBattleFeedbackVisualModuleAuthoring
    {
        private const string ProfilePath =
            "Assets/_Game/Resources/V04/FormalBattlePresentation/"
            + "FormalBattlePresentationProfile.asset";
        private const string ModulePrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/BattlePresentation/"
            + "FormalBattleFeedbackVisualModule.prefab";
        private const string PoolPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/BattlePresentation/"
            + "FormalBattleDamageFloatPool.prefab";
        private const string PlayerPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/BattlePresentation/"
            + "FormalBattlePlayerPresentation.prefab";
        private const string EnemyPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/BattlePresentation/"
            + "FormalBattleEnemySlot.prefab";
        private const string FeedbackIconAtlasPath =
            "Assets/_Game/Resources/\u89d2\u8272\u654c\u4ebaUI/"
            + "FormalBattleFeedbackIconAtlas_V1.png";
        private const string BasicActionIconPath =
            "Assets/_Game/Resources/item/\u57fa\u7840\u5c5e\u6027icon/"
            + "\u653b\u51fb.png";
        private const string HpDamageIconPath =
            "Assets/_Game/Resources/item/\u57fa\u7840\u5c5e\u6027icon/"
            + "\u4f24\u5bb3.png";
        private const string GuardIconPath =
            "Assets/_Game/Resources/item/\u57fa\u7840\u5c5e\u6027icon/"
            + "\u62a4\u52bf.png";
        private const string ControlIconPath =
            "Assets/_Game/Resources/item/\u57fa\u7840\u5c5e\u6027icon/"
            + "\u63a7\u5236.png";
        private const string NianIconPath =
            "Assets/_Game/Resources/item/\u57fa\u7840\u5c5e\u6027icon/"
            + "\u8017\u5ff5.png";
        private const string ShellIconPath =
            "Assets/_Game/Resources/\u89d2\u8272\u654c\u4ebaUI/"
            + "EnemyShellIcon_V1.png";

        private const string SkillSpriteName = "FeedbackSkill";
        private const string PassiveSpriteName = "FeedbackPassive";
        private const string ItemTriggerSpriteName = "FeedbackItemTrigger";
        private const string StatusTriggerSpriteName = "FeedbackStatusTrigger";
        private const string EnvironmentTriggerSpriteName =
            "FeedbackEnvironmentTrigger";
        private const string HealSpriteName = "FeedbackHeal";
        private const string BuffSpriteName = "FeedbackBuff";
        private const string DebuffSpriteName = "FeedbackDebuff";
        private const string CleanseSpriteName = "FeedbackCleanse";

        [MenuItem(
            "TalismanBag/V0.4/Formal Battle/Author Feedback Visual Modules V1")]
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

        [MenuItem(
            "TalismanBag/V0.4/Formal Battle/"
            + "Migrate Feedback Badge Glyphs To Image Icons")]
        public static void ExecuteImageIconMigrationFromMenu()
        {
            ExecuteImageIconMigrationInternal();
        }

        public static void ExecuteImageIconMigrationFromCommandLine()
        {
            try
            {
                ExecuteImageIconMigrationInternal();
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
            Require(profile != null, "FEEDBACK_PROFILE_MISSING");
            Require(profile.ValidateAuthoredReferences(),
                "FEEDBACK_PROFILE_INVALID");

            FeedbackIconSet icons = LoadFeedbackIconSet();

            AuthorModulePrefab(profile, icons);
            AuthorPool(profile, icons);
            AuthorPlayerLanes(profile);
            AuthorEnemyLanes(profile);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "FORMAL_BATTLE_FEEDBACK_VISUAL_MODULE_V1_AUTHORING_PASS");
        }

        private static void ExecuteImageIconMigrationInternal()
        {
            FormalBattlePresentationProfile profile =
                AssetDatabase.LoadAssetAtPath<
                    FormalBattlePresentationProfile>(ProfilePath);
            Require(profile != null, "FEEDBACK_PROFILE_MISSING");
            Require(profile.ValidateAuthoredReferences(),
                "FEEDBACK_PROFILE_INVALID");

            FeedbackIconSet icons = LoadFeedbackIconSet();
            MigrateModulePrefabIcons(profile, icons);
            MigratePoolPrefabIcons(profile, icons);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "FORMAL_BATTLE_FEEDBACK_IMAGE_ICON_MIGRATION_PASS");
        }

        private static void AuthorModulePrefab(
            FormalBattlePresentationProfile profile,
            FeedbackIconSet icons)
        {
            GameObject root = new GameObject(
                "FormalBattleFeedbackVisualModule",
                typeof(RectTransform),
                typeof(CanvasGroup),
                typeof(FormalBattleFeedbackVisualModule));
            try
            {
                RectTransform rect = root.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(310f, 72f);

                CanvasGroup group = root.GetComponent<CanvasGroup>();
                group.alpha = 1f;
                group.interactable = false;
                group.blocksRaycasts = false;

                EnsureBadge(
                    rect,
                    "TriggerBadge",
                    new Vector2(-128f, 0f),
                    out Image triggerBadge,
                    out Image triggerIcon);
                TMP_Text amount = CreateLabel(
                    rect,
                    "AmountLabel",
                    new Vector2(0f, 0f),
                    new Vector2(190f, 72f),
                    42f,
                    profile.DamageFont);
                EnsureBadge(
                    rect,
                    "ResultBadge",
                    new Vector2(128f, 0f),
                    out Image resultBadge,
                    out Image resultIcon);

                FormalBattleFeedbackVisualModule module =
                    root.GetComponent<FormalBattleFeedbackVisualModule>();
                module.AssignForEditor(
                    rect,
                    group,
                    triggerBadge,
                    triggerIcon,
                    amount,
                    resultBadge,
                    resultIcon);
                AssignVisualSprites(module, icons);
                Require(module.ValidateAuthoredReferences(),
                    "FEEDBACK_MODULE_REFERENCE_INVALID");

                Require(profile.TryGetDamageFloatStyle(
                        FormalBattleDamageFloatStyleKeys.Damage,
                        out FormalBattleDamageFloatVisualStyle style),
                    "FEEDBACK_DAMAGE_STYLE_MISSING");
                module.SetAuthoringPreviewForEditor(
                    "-82",
                    profile.DamageFont,
                    style,
                    C1FormalRealtimeBattleFeedbackTriggerKinds.Skill,
                    C1FormalRealtimeBattleFeedbackDeliveryKinds.Instant,
                    C1FormalRealtimeBattleFeedbackResultKinds.HpDamage);
                PrefabUtility.SaveAsPrefabAsset(root, ModulePrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void AuthorPool(
            FormalBattlePresentationProfile profile,
            FeedbackIconSet icons)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(
                PoolPrefabPath);
            try
            {
                RectTransform rootRect = root.transform as RectTransform;
                FormalBattleDamageFloatPool pool =
                    root.GetComponent<FormalBattleDamageFloatPool>();
                Require(rootRect != null && pool != null,
                    "FEEDBACK_POOL_ROOT_INVALID");

                RectTransform[] entryRects = rootRect
                    .Cast<Transform>()
                    .Select(value => value as RectTransform)
                    .Where(value => value != null
                                    && value.name.StartsWith(
                                        "DamageFloat_",
                                        StringComparison.Ordinal))
                    .OrderBy(value => value.name, StringComparer.Ordinal)
                    .ToArray();
                Require(entryRects.Length == 10,
                    "FEEDBACK_POOL_ENTRY_COUNT_INVALID");
                FormalBattleFeedbackVisualModule[] modules =
                    new FormalBattleFeedbackVisualModule[
                        entryRects.Length];
                for (int index = 0; index < modules.Length; index++)
                {
                    RectTransform moduleRect = entryRects[index];
                    if (ApproximatelyGeneratedPreviewScale(
                            moduleRect.localScale))
                    {
                        moduleRect.localScale = Vector3.one;
                    }
                    CanvasGroup group =
                        moduleRect.GetComponent<CanvasGroup>();
                    TMP_Text amount = moduleRect.GetComponent<TMP_Text>();
                    Require(group != null && amount != null,
                        "FEEDBACK_POOL_ENTRY_REFERENCE_INVALID");
                    EnsureBadge(
                        moduleRect,
                        "TriggerBadge",
                        new Vector2(-112f, 0f),
                        out Image triggerBadge,
                        out Image triggerIcon);
                    EnsureBadge(
                        moduleRect,
                        "ResultBadge",
                        new Vector2(112f, 0f),
                        out Image resultBadge,
                        out Image resultIcon);
                    modules[index] = moduleRect.GetComponent<
                            FormalBattleFeedbackVisualModule>()
                        ?? moduleRect.gameObject.AddComponent<
                            FormalBattleFeedbackVisualModule>();
                    modules[index].AssignForEditor(
                        moduleRect,
                        group,
                        triggerBadge,
                        triggerIcon,
                        amount,
                        resultBadge,
                        resultIcon);
                    AssignVisualSprites(modules[index], icons);
                }

                pool.AssignForEditor(rootRect, profile, modules);
                pool.SetAuthoringPreviewForEditor(true);
                Require(pool.ValidateAuthoredReferences(),
                    "FEEDBACK_POOL_REFERENCE_INVALID");
                PrefabUtility.SaveAsPrefabAsset(root, PoolPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static bool ApproximatelyGeneratedPreviewScale(
            Vector3 scale)
        {
            return Mathf.Approximately(scale.x, 0.82f)
                   && Mathf.Approximately(scale.y, 0.82f)
                   && Mathf.Approximately(scale.z, 0.82f)
                   || Mathf.Approximately(scale.x, 0.6724f)
                   && Mathf.Approximately(scale.y, 0.6724f)
                   && Mathf.Approximately(scale.z, 0.6724f);
        }

        private static void AuthorPlayerLanes(
            FormalBattlePresentationProfile profile)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(
                PlayerPrefabPath);
            try
            {
                FormalBattlePlayerPresentationView view =
                    root.GetComponent<FormalBattlePlayerPresentationView>();
                Require(view != null, "PLAYER_FEEDBACK_VIEW_MISSING");
                RectTransform owner = view.transform as RectTransform;
                RectTransform damage = view.DamageAnchor;
                RectTransform benefit = view.BenefitFloatAnchor;
                Require(owner != null && damage != null && benefit != null,
                    "PLAYER_FEEDBACK_BASE_LANES_MISSING");
                RectTransform status = EnsureLane(
                    owner,
                    "PlayerStatusFloatAnchor",
                    benefit,
                    new Vector2(0f, 92f),
                    "增益",
                    FormalBattleDamageFloatStyleKeys.Guard,
                    profile);
                FormalBattleFeedbackReceiverLanes lanes =
                    root.GetComponent<FormalBattleFeedbackReceiverLanes>()
                    ?? root.AddComponent<
                        FormalBattleFeedbackReceiverLanes>();
                lanes.AssignForEditor(damage, benefit, status);
                view.AssignFeedbackReceiverLanesForEditor(lanes);
                Require(view.ValidateAuthoredReferences(),
                    "PLAYER_FEEDBACK_LANES_INVALID");
                PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void AuthorEnemyLanes(
            FormalBattlePresentationProfile profile)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(
                EnemyPrefabPath);
            try
            {
                FormalBattleEnemySlotView view =
                    root.GetComponent<FormalBattleEnemySlotView>();
                Require(view != null, "ENEMY_FEEDBACK_VIEW_MISSING");
                RectTransform owner = view.transform as RectTransform;
                RectTransform damage = view.DamageAnchor;
                RectTransform benefit = view.EffectFloatAnchor;
                Require(owner != null && damage != null && benefit != null,
                    "ENEMY_FEEDBACK_BASE_LANES_MISSING");
                RectTransform status = EnsureLane(
                    owner,
                    "EnemyStatusFloatAnchor",
                    benefit,
                    new Vector2(0f, 92f),
                    "减益",
                    FormalBattleDamageFloatStyleKeys.Control,
                    profile);
                FormalBattleFeedbackReceiverLanes lanes =
                    root.GetComponent<FormalBattleFeedbackReceiverLanes>()
                    ?? root.AddComponent<
                        FormalBattleFeedbackReceiverLanes>();
                lanes.AssignForEditor(damage, benefit, status);
                view.AssignFeedbackReceiverLanesForEditor(lanes);
                Require(view.ValidateAuthoredReferences(),
                    "ENEMY_FEEDBACK_LANES_INVALID");
                PrefabUtility.SaveAsPrefabAsset(root, EnemyPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static RectTransform EnsureLane(
            RectTransform owner,
            string name,
            RectTransform reference,
            Vector2 creationOffset,
            string previewPayload,
            string previewStyleKey,
            FormalBattlePresentationProfile profile)
        {
            RectTransform lane = owner.Find(name) as RectTransform;
            if (lane == null)
            {
                GameObject laneObject = new GameObject(
                    name,
                    typeof(RectTransform),
                    typeof(FormalBattleDamageFloatAnchorPreview));
                lane = laneObject.GetComponent<RectTransform>();
                lane.SetParent(owner, false);
                lane.anchorMin = reference.anchorMin;
                lane.anchorMax = reference.anchorMax;
                lane.pivot = reference.pivot;
                lane.sizeDelta = reference.sizeDelta;
                lane.anchoredPosition = reference.anchoredPosition
                    + creationOffset;
                lane.localScale = reference.localScale;
            }

            FormalBattleDamageFloatAnchorPreview preview =
                lane.GetComponent<FormalBattleDamageFloatAnchorPreview>()
                ?? lane.gameObject.AddComponent<
                    FormalBattleDamageFloatAnchorPreview>();
            TMP_Text label = lane.GetComponentInChildren<TMP_Text>(true);
            if (label == null)
            {
                label = CreateLabel(
                    lane,
                    "FeedbackLanePreview",
                    Vector2.zero,
                    lane.sizeDelta,
                    38f,
                    profile.DamageFont);
            }
            preview.AssignForEditor(
                profile,
                label,
                previewPayload,
                previewStyleKey);
            return lane;
        }

        private static void MigrateModulePrefabIcons(
            FormalBattlePresentationProfile profile,
            FeedbackIconSet icons)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(
                ModulePrefabPath);
            try
            {
                RectTransform rect = root.transform as RectTransform;
                CanvasGroup group = root.GetComponent<CanvasGroup>();
                FormalBattleFeedbackVisualModule module =
                    root.GetComponent<FormalBattleFeedbackVisualModule>();
                TMP_Text amount = root.transform.Find("AmountLabel")
                    ?.GetComponent<TMP_Text>();
                Require(rect != null && group != null && module != null
                        && amount != null,
                    "FEEDBACK_MODULE_MIGRATION_BASE_INVALID");

                EnsureBadge(
                    rect,
                    "TriggerBadge",
                    Vector2.zero,
                    out Image triggerBadge,
                    out Image triggerIcon);
                EnsureBadge(
                    rect,
                    "ResultBadge",
                    Vector2.zero,
                    out Image resultBadge,
                    out Image resultIcon);
                module.AssignForEditor(
                    rect,
                    group,
                    triggerBadge,
                    triggerIcon,
                    amount,
                    resultBadge,
                    resultIcon);
                AssignVisualSprites(module, icons);
                Require(module.ValidateAuthoredReferences(),
                    "FEEDBACK_MODULE_IMAGE_REFERENCES_INVALID");

                Require(profile.TryGetDamageFloatStyle(
                        FormalBattleDamageFloatStyleKeys.Damage,
                        out FormalBattleDamageFloatVisualStyle style),
                    "FEEDBACK_DAMAGE_STYLE_MISSING");
                module.SetAuthoringPreviewForEditor(
                    "-82",
                    profile.DamageFont,
                    style,
                    C1FormalRealtimeBattleFeedbackTriggerKinds.Skill,
                    C1FormalRealtimeBattleFeedbackDeliveryKinds.Instant,
                    C1FormalRealtimeBattleFeedbackResultKinds.HpDamage);
                PrefabUtility.SaveAsPrefabAsset(root, ModulePrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void MigratePoolPrefabIcons(
            FormalBattlePresentationProfile profile,
            FeedbackIconSet icons)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(
                PoolPrefabPath);
            try
            {
                RectTransform rootRect = root.transform as RectTransform;
                FormalBattleDamageFloatPool pool =
                    root.GetComponent<FormalBattleDamageFloatPool>();
                Require(rootRect != null && pool != null,
                    "FEEDBACK_POOL_ROOT_INVALID");

                RectTransform[] entryRects = rootRect
                    .Cast<Transform>()
                    .Select(value => value as RectTransform)
                    .Where(value => value != null
                                    && value.name.StartsWith(
                                        "DamageFloat_",
                                        StringComparison.Ordinal))
                    .OrderBy(value => value.name, StringComparer.Ordinal)
                    .ToArray();
                Require(entryRects.Length == 10,
                    "FEEDBACK_POOL_ENTRY_COUNT_INVALID");
                FormalBattleFeedbackVisualModule[] modules =
                    new FormalBattleFeedbackVisualModule[
                        entryRects.Length];

                for (int index = 0; index < entryRects.Length; index++)
                {
                    RectTransform rect = entryRects[index];
                    CanvasGroup group = rect.GetComponent<CanvasGroup>();
                    TMP_Text amount = rect.GetComponent<TMP_Text>();
                    FormalBattleFeedbackVisualModule module =
                        rect.GetComponent<
                            FormalBattleFeedbackVisualModule>();
                    Require(group != null && amount != null
                            && module != null,
                        "FEEDBACK_POOL_ENTRY_REFERENCE_INVALID");

                    EnsureBadge(
                        rect,
                        "TriggerBadge",
                        Vector2.zero,
                        out Image triggerBadge,
                        out Image triggerIcon);
                    EnsureBadge(
                        rect,
                        "ResultBadge",
                        Vector2.zero,
                        out Image resultBadge,
                        out Image resultIcon);
                    module.AssignForEditor(
                        rect,
                        group,
                        triggerBadge,
                        triggerIcon,
                        amount,
                        resultBadge,
                        resultIcon);
                    AssignVisualSprites(module, icons);
                    Require(module.ValidateAuthoredReferences(),
                        "FEEDBACK_POOL_ENTRY_IMAGE_REFERENCES_INVALID");
                    modules[index] = module;
                }

                pool.AssignForEditor(rootRect, profile, modules);
                pool.SetAuthoringPreviewForEditor(true);
                Require(pool.ValidateAuthoredReferences(),
                    "FEEDBACK_POOL_REFERENCE_INVALID");
                PrefabUtility.SaveAsPrefabAsset(root, PoolPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static FeedbackIconSet LoadFeedbackIconSet()
        {
            EnsureFeedbackIconAtlasImport();
            return new FeedbackIconSet(
                RequireSprite(BasicActionIconPath),
                RequireAtlasSprite(SkillSpriteName),
                RequireAtlasSprite(PassiveSpriteName),
                RequireAtlasSprite(ItemTriggerSpriteName),
                RequireAtlasSprite(StatusTriggerSpriteName),
                RequireAtlasSprite(EnvironmentTriggerSpriteName),
                RequireSprite(HpDamageIconPath),
                RequireSprite(ShellIconPath),
                RequireSprite(GuardIconPath),
                RequireAtlasSprite(HealSpriteName),
                RequireAtlasSprite(BuffSpriteName),
                RequireAtlasSprite(DebuffSpriteName),
                RequireSprite(ControlIconPath),
                RequireAtlasSprite(CleanseSpriteName),
                RequireSprite(NianIconPath));
        }

        private static void EnsureFeedbackIconAtlasImport()
        {
            AssetDatabase.ImportAsset(
                FeedbackIconAtlasPath,
                ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = AssetImporter.GetAtPath(
                FeedbackIconAtlasPath) as TextureImporter;
            Require(importer != null,
                "FEEDBACK_ICON_ATLAS_IMPORTER_MISSING");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.mipmapEnabled = false;
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.sRGBTexture = true;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression
                .Uncompressed;
            importer.maxTextureSize = 2048;
            importer.spritesheet = CreateFeedbackSpriteSheet();
            importer.SaveAndReimport();

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(
                FeedbackIconAtlasPath);
            Require(texture != null && texture.width == 1254
                    && texture.height == 1254,
                "FEEDBACK_ICON_ATLAS_DIMENSIONS_INVALID");
        }

        private static SpriteMetaData[] CreateFeedbackSpriteSheet()
        {
            const float cell = 418f;
            return new[]
            {
                CreateSpriteMeta(SkillSpriteName, 0f, cell * 2f, cell),
                CreateSpriteMeta(PassiveSpriteName, cell, cell * 2f, cell),
                CreateSpriteMeta(
                    ItemTriggerSpriteName,
                    cell * 2f,
                    cell * 2f,
                    cell),
                CreateSpriteMeta(StatusTriggerSpriteName, 0f, cell, cell),
                CreateSpriteMeta(
                    EnvironmentTriggerSpriteName,
                    cell,
                    cell,
                    cell),
                CreateSpriteMeta(HealSpriteName, cell * 2f, cell, cell),
                CreateSpriteMeta(BuffSpriteName, 0f, 0f, cell),
                CreateSpriteMeta(DebuffSpriteName, cell, 0f, cell),
                CreateSpriteMeta(CleanseSpriteName, cell * 2f, 0f, cell)
            };
        }

        private static SpriteMetaData CreateSpriteMeta(
            string name,
            float x,
            float y,
            float size)
        {
            return new SpriteMetaData
            {
                name = name,
                rect = new Rect(x, y, size, size),
                alignment = (int)SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f),
                border = Vector4.zero
            };
        }

        private static Sprite RequireSprite(string assetPath)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(
                assetPath);
            Require(sprite != null,
                "FEEDBACK_ICON_MISSING path=" + assetPath);
            return sprite;
        }

        private static Sprite RequireAtlasSprite(string spriteName)
        {
            Sprite sprite = AssetDatabase.LoadAllAssetsAtPath(
                    FeedbackIconAtlasPath)
                .OfType<Sprite>()
                .SingleOrDefault(value => string.Equals(
                    value.name,
                    spriteName,
                    StringComparison.Ordinal));
            Require(sprite != null,
                "FEEDBACK_ATLAS_SPRITE_MISSING name=" + spriteName);
            return sprite;
        }

        private static void AssignVisualSprites(
            FormalBattleFeedbackVisualModule module,
            FeedbackIconSet icons)
        {
            module.AssignVisualSpritesForEditor(
                icons.BasicAction,
                icons.Skill,
                icons.Passive,
                icons.ItemTrigger,
                icons.StatusTrigger,
                icons.EnvironmentTrigger,
                icons.HpDamage,
                icons.Shell,
                icons.Guard,
                icons.Heal,
                icons.Buff,
                icons.Debuff,
                icons.Control,
                icons.Cleanse,
                icons.Nian);
        }

        private static void EnsureBadge(
            RectTransform parent,
            string name,
            Vector2 anchoredPosition,
            out Image badge,
            out Image icon)
        {
            Transform existing = parent.Find(name);
            GameObject badgeObject = existing == null
                ? new GameObject(
                    name,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image))
                : existing.gameObject;
            RectTransform badgeRect = badgeObject.transform as RectTransform;
            Require(badgeRect != null, "FEEDBACK_BADGE_RECT_MISSING");
            if (existing == null)
            {
                badgeRect.SetParent(parent, false);
                badgeRect.anchorMin = new Vector2(0.5f, 0.5f);
                badgeRect.anchorMax = new Vector2(0.5f, 0.5f);
                badgeRect.pivot = new Vector2(0.5f, 0.5f);
                badgeRect.sizeDelta = new Vector2(44f, 44f);
                badgeRect.anchoredPosition = anchoredPosition;
            }
            badge = badgeObject.GetComponent<Image>();
            Require(badge != null, "FEEDBACK_BADGE_IMAGE_MISSING");
            badge.raycastTarget = false;
            badge.color = new Color(0.25f, 0.25f, 0.25f, 0.88f);

            Transform legacyGlyph = badgeRect.Find("Glyph");
            if (legacyGlyph != null)
            {
                UnityEngine.Object.DestroyImmediate(legacyGlyph.gameObject);
            }

            Transform existingIcon = badgeRect.Find("Icon");
            GameObject iconObject = existingIcon == null
                ? new GameObject(
                    "Icon",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image))
                : existingIcon.gameObject;
            RectTransform iconRect = iconObject.transform as RectTransform;
            Require(iconRect != null, "FEEDBACK_ICON_RECT_MISSING");
            if (existingIcon == null)
            {
                iconRect.SetParent(badgeRect, false);
                iconRect.anchorMin = Vector2.zero;
                iconRect.anchorMax = Vector2.one;
                iconRect.pivot = new Vector2(0.5f, 0.5f);
                iconRect.offsetMin = new Vector2(3f, 3f);
                iconRect.offsetMax = new Vector2(-3f, -3f);
            }
            icon = iconObject.GetComponent<Image>();
            Require(icon != null, "FEEDBACK_ICON_IMAGE_MISSING");
            icon.raycastTarget = false;
            icon.preserveAspect = true;
            icon.color = Color.white;
        }

        private static TMP_Text CreateLabel(
            RectTransform parent,
            string name,
            Vector2 anchoredPosition,
            Vector2 size,
            float fontSize,
            TMP_FontAsset font)
        {
            GameObject labelObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));
            RectTransform rect = labelObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
            TextMeshProUGUI label =
                labelObject.GetComponent<TextMeshProUGUI>();
            label.font = font;
            label.fontSize = fontSize;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.enableWordWrapping = false;
            label.overflowMode = TextOverflowModes.Overflow;
            label.raycastTarget = false;
            return label;
        }

        private sealed class FeedbackIconSet
        {
            public FeedbackIconSet(
                Sprite basicAction,
                Sprite skill,
                Sprite passive,
                Sprite itemTrigger,
                Sprite statusTrigger,
                Sprite environmentTrigger,
                Sprite hpDamage,
                Sprite shell,
                Sprite guard,
                Sprite heal,
                Sprite buff,
                Sprite debuff,
                Sprite control,
                Sprite cleanse,
                Sprite nian)
            {
                BasicAction = basicAction;
                Skill = skill;
                Passive = passive;
                ItemTrigger = itemTrigger;
                StatusTrigger = statusTrigger;
                EnvironmentTrigger = environmentTrigger;
                HpDamage = hpDamage;
                Shell = shell;
                Guard = guard;
                Heal = heal;
                Buff = buff;
                Debuff = debuff;
                Control = control;
                Cleanse = cleanse;
                Nian = nian;
            }

            public Sprite BasicAction { get; }
            public Sprite Skill { get; }
            public Sprite Passive { get; }
            public Sprite ItemTrigger { get; }
            public Sprite StatusTrigger { get; }
            public Sprite EnvironmentTrigger { get; }
            public Sprite HpDamage { get; }
            public Sprite Shell { get; }
            public Sprite Guard { get; }
            public Sprite Heal { get; }
            public Sprite Buff { get; }
            public Sprite Debuff { get; }
            public Sprite Control { get; }
            public Sprite Cleanse { get; }
            public Sprite Nian { get; }
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
