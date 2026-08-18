#if UNITY_EDITOR
using System;
using System.Reflection;
using TalismanBag.BuildSandbox;
using TalismanBag.Feedback;
using TalismanBag.V02.Status;
using TalismanBag.V02.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BattleSandboxCombatInfoHudSceneBinder
    {
        public const string ManualMenuPath =
            "Tools/Talisman Bag/Dev Only/V0.4/BuildSandbox/BattleSandboxEnemyCombatFeedbackUiReuse01/[Writes Scene][Manual Only] Bind V02 Combat Info HUD";

        private const string FeedbackRootName = "FeedbackRoot";
        private const string StageProgressName = "V02StageProgressBar_Runtime";
        private const string EnemyAreaName = "V02EnemyArea";
        private const string TopStatusBarName = "V02TopStatusBar";
        private const string AutoCombatStageName = "V02AutoCombatStage";
        private const string BarFillName = "Fill";

        private static readonly Color TopBarColor = new(0.105f, 0.12f, 0.095f, 1f);
        private static readonly Color PlayerStageColor = new(0.085f, 0.095f, 0.08f, 0.78f);
        private static readonly Color PlayerAvatarColor = new(0.17f, 0.12f, 0.08f, 0.96f);
        private static readonly Color PlayerHpTrackColor = new(0.18f, 0.025f, 0.025f, 0.78f);
        private static readonly Color PlayerHpFillColor = new(0.491f, 0.098f, 0.088f, 0.95f);
        private static readonly Color PlayerManaTrackColor = new(0.232f, 0.291f, 0.447f, 0.78f);
        private static readonly Color PlayerManaFillColor = new(0.467f, 0.635f, 0.83f, 0.95f);
        private static readonly Color EnemyAreaColor = new(0.095f, 0.105f, 0.088f, 0.94f);
        private static readonly Color EnemyAvatarColor = new(0.20f, 0.13f, 0.18f, 0.96f);
        private static readonly Color CastTrackColor = new(0.18f, 0.13f, 0.11f, 0.96f);
        private static readonly Color CastFillColor = new(0.95f, 0.45f, 0.22f, 1f);

        [MenuItem(ManualMenuPath)]
        public static void BindCombatInfoHudMenu()
        {
            if (!EditorUtility.DisplayDialog(
                    "[Writes Scene][Manual Only] Bind V02 Combat Info HUD",
                    "This writes only the V04 BuildSandbox preview scene.\n\n" +
                    BuildSandboxPreviewSceneMarker.ScenePath + "\n\n" +
                    "It only adds/reuses FeedbackRoot, V02StageProgressBar_Runtime, V02EnemyArea, V02TopStatusBar, and V02AutoCombatStage.\n" +
                    "It does not bind V02 RunFlow, formal combat damage, rewards, save data, chapters, or FeatureFlags.",
                    "Bind",
                    "Cancel"))
            {
                return;
            }

            BindCombatInfoHudScene();
        }

        public static void BindCombatInfoHudSceneBatch()
        {
            string path = BindCombatInfoHudScene();
            Debug.Log("[V0.4-BattleSandboxEnemyCombatFeedbackUiReuse01] COMBAT_INFO_HUD_BOUND path=" + path);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(0);
            }
        }

        public static string BindCombatInfoHudScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException("Combat info HUD binding must run in Edit Mode.");
            }

            Scene previousScene = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.OpenScene(BuildSandboxPreviewSceneMarker.ScenePath, OpenSceneMode.Single);
            try
            {
                Transform canvas = RequireRoot("BuildSandboxPreviewCanvas");
                Transform safeArea = RequireChild(canvas, "SafeAreaRoot");

                BuildFeedbackRoot(safeArea);
                BuildTopStatusBar(safeArea);
                BuildStageProgressBar(safeArea);
                BuildAutoCombatStage(safeArea);
                BuildEnemyArea(safeArea);

                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene, BuildSandboxPreviewSceneMarker.ScenePath))
                {
                    throw new InvalidOperationException("Could not save " + BuildSandboxPreviewSceneMarker.ScenePath);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[V0.4-BattleSandboxEnemyCombatFeedbackUiReuse01] COMBAT_INFO_HUD_BOUND path=" +
                          BuildSandboxPreviewSceneMarker.ScenePath);
                return BuildSandboxPreviewSceneMarker.ScenePath;
            }
            finally
            {
                if (previousScene.IsValid()
                    && !string.IsNullOrEmpty(previousScene.path)
                    && previousScene.path != scene.path)
                {
                    EditorSceneManager.OpenScene(previousScene.path, OpenSceneMode.Single);
                }
            }
        }

        private static void BuildFeedbackRoot(Transform safeArea)
        {
            RectTransform root = EnsureSceneRect(safeArea, FeedbackRootName, out bool created);
            if (created)
            {
                SetRect(root, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), new Vector2(0f, 132f), Vector2.zero);
            }

            RectTransform anchorsRoot = EnsureRectChild(root, "FloatingTextAnchors", out bool anchorsCreated);
            if (anchorsCreated)
            {
                SetRect(anchorsRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            }

            FloatingCombatTextAnchorLayout layout = anchorsRoot.GetComponent<FloatingCombatTextAnchorLayout>();
            if (layout == null)
            {
                layout = anchorsRoot.gameObject.AddComponent<FloatingCombatTextAnchorLayout>();
            }

            SetField(layout, "damageDealtAnchor", EnsureFeedbackAnchor(anchorsRoot, "DamageDealtAnchor", new Vector2(320f, 480f)));
            SetField(layout, "damageTakenAnchor", EnsureFeedbackAnchor(anchorsRoot, "DamageTakenAnchor", new Vector2(-320f, 480f)));
            SetField(layout, "manaGeneratedAnchor", EnsureFeedbackAnchor(anchorsRoot, "ManaGeneratedAnchor", new Vector2(0f, 320f)));
            SetField(layout, "manaSpentAnchor", EnsureFeedbackAnchor(anchorsRoot, "ManaSpentAnchor", new Vector2(0f, 260f)));
            SetField(layout, "shieldGainedAnchor", EnsureFeedbackAnchor(anchorsRoot, "ShieldGainedAnchor", new Vector2(-320f, 390f)));
            SetField(layout, "healReceivedAnchor", EnsureFeedbackAnchor(anchorsRoot, "HealReceivedAnchor", new Vector2(-320f, 540f)));
            SetField(layout, "shieldBreakAnchor", EnsureFeedbackAnchor(anchorsRoot, "ShieldBreakAnchor", new Vector2(320f, 390f)));
            SetField(layout, "cleanseAnchor", EnsureFeedbackAnchor(anchorsRoot, "CleanseAnchor", new Vector2(-180f, 360f)));
            SetField(layout, "unsealAnchor", EnsureFeedbackAnchor(anchorsRoot, "UnsealAnchor", new Vector2(-80f, 420f)));
            SetField(layout, "soulSuppressAnchor", EnsureFeedbackAnchor(anchorsRoot, "SoulSuppressAnchor", new Vector2(320f, 540f)));
            SetField(layout, "chainClearAnchor", EnsureFeedbackAnchor(anchorsRoot, "ChainClearAnchor", new Vector2(320f, 330f)));
            SetField(layout, "formationProtectedAnchor", EnsureFeedbackAnchor(anchorsRoot, "FormationProtectedAnchor", new Vector2(0f, 180f)));
            SetField(layout, "guardReduceAnchor", EnsureFeedbackAnchor(anchorsRoot, "GuardReduceAnchor", new Vector2(-320f, 330f)));
            SetField(layout, "counterFailedAnchor", EnsureFeedbackAnchor(anchorsRoot, "CounterFailedAnchor", new Vector2(0f, 520f)));
            SetField(layout, "statusDamageAnchor", EnsureFeedbackAnchor(anchorsRoot, "StatusDamageAnchor", new Vector2(-210f, 430f)));
            SetField(layout, "enemyInterruptedAnchor", EnsureFeedbackAnchor(anchorsRoot, "EnemyInterruptedAnchor", new Vector2(320f, 610f)));
            SetField(layout, "enemyEnragedAnchor", EnsureFeedbackAnchor(anchorsRoot, "EnemyEnragedAnchor", new Vector2(320f, 560f)));
            SetField(layout, "itemSealedAnchor", EnsureFeedbackAnchor(anchorsRoot, "ItemSealedAnchor", new Vector2(0f, 110f)));
            SetField(layout, "itemUnsealedAnchor", EnsureFeedbackAnchor(anchorsRoot, "ItemUnsealedAnchor", new Vector2(0f, 150f)));

            RectTransform tooltip = EnsureRectChild(root, "StatusTooltipRuntime", out bool tooltipCreated, typeof(StatusTooltipPanel));
            if (tooltipCreated)
            {
                SetRect(tooltip, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            }
        }

        private static void BuildTopStatusBar(Transform safeArea)
        {
            RectTransform topBar = EnsureSceneRect(
                safeArea,
                TopStatusBarName,
                out bool created,
                typeof(CanvasRenderer),
                typeof(Image));

            if (created)
            {
                SetRect(topBar, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(1000f, 50f));
            }

            EnsureImage(topBar.gameObject, TopBarColor, false, created);
            GridLayoutGroup layout = topBar.GetComponent<GridLayoutGroup>();
            bool layoutCreated = layout == null;
            if (layout == null)
            {
                layout = topBar.gameObject.AddComponent<GridLayoutGroup>();
            }

            if (created || layoutCreated)
            {
                layout.cellSize = new Vector2(240f, 46f);
                layout.spacing = new Vector2(10f, 10f);
                layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                layout.constraintCount = 4;
                layout.padding = new RectOffset(14, 14, 14, 14);
                layout.childAlignment = TextAnchor.MiddleCenter;
            }

            Text hpText = EnsureStatusText(topBar, "HPText", "100/100 \u6c14\u8840", new Color(1f, 0.86f, 0.78f, 1f));
            EnsureStatusText(topBar, "ShieldText", "\u62a4\u76fe 0", new Color(0.78f, 0.94f, 1f, 1f));
            Text manaText = EnsureStatusText(topBar, "ManaText", "0/100 \u7075\u6c14", new Color(0.72f, 0.9f, 1f, 1f));
            EnsureStatusText(topBar, "StateText", "\u6218\u6597\u9884\u89c8", new Color(0.82f, 1f, 0.7f, 1f));
            EnsureStatusText(topBar, "CurrentLevelText", "dev-4-10", new Color(1f, 0.88f, 0.56f, 1f));

            EnsurePlayerStatusBar(hpText, "PlayerHPBar", PlayerHpTrackColor, PlayerHpFillColor, 1f);
            EnsurePlayerStatusBar(manaText, "PlayerManaBar", PlayerManaTrackColor, PlayerManaFillColor, 0f);
        }

        private static void BuildStageProgressBar(Transform safeArea)
        {
            Transform existing = FindDeepChild(safeArea, StageProgressName);
            if (existing != null)
            {
                return;
            }

            V02StageProgressBar progressBar = V02StageProgressBar.CreateRuntime(
                safeArea,
                new Vector2(0f, 850f),
                new Vector2(400f, 58f),
                true);
            if (progressBar == null)
            {
                throw new InvalidOperationException("Could not create " + StageProgressName);
            }

            progressBar.gameObject.name = StageProgressName;
            progressBar.SetProgress("4", 8, 9, 10);
            foreach (Graphic graphic in progressBar.GetComponentsInChildren<Graphic>(true))
            {
                graphic.raycastTarget = false;
            }

            EditorUtility.SetDirty(progressBar);
        }

        private static void BuildAutoCombatStage(Transform safeArea)
        {
            RectTransform stage = EnsureSceneRect(
                safeArea,
                AutoCombatStageName,
                out bool created,
                typeof(CanvasRenderer),
                typeof(Image));

            if (created)
            {
                SetRect(stage, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-250f, -100f), new Vector2(500f, 1100f));
            }

            EnsureImage(stage.gameObject, PlayerStageColor, false, created);

            RectTransform playerAvatar = EnsureRectChild(
                stage,
                "V02PlayerAvatar",
                out bool avatarCreated,
                typeof(CanvasRenderer),
                typeof(Image));

            if (avatarCreated)
            {
                SetRect(playerAvatar, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(35f, 400f), new Vector2(120f, 120f));
            }

            EnsureImage(playerAvatar.gameObject, PlayerAvatarColor, false, avatarCreated);

            RectTransform playerHit = EnsureRectChild(
                playerAvatar,
                "PlayerHitFeedback",
                out bool hitCreated,
                typeof(CanvasRenderer),
                typeof(Image));

            if (hitCreated)
            {
                SetRect(playerHit, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
                CanvasRenderer hitRenderer = playerHit.GetComponent<CanvasRenderer>();
                if (hitRenderer != null)
                {
                    hitRenderer.cullTransparentMesh = false;
                }
            }

            EnsureImage(playerHit.gameObject, new Color(1f, 0.28f, 0.22f, 0f), false, hitCreated);

            Text playerFace = EnsureTextChild(playerAvatar, "PlayerAvatarGlyph", "\u4fee", 48, FontStyle.Bold, new Color(1f, 0.86f, 0.55f, 1f), TextAnchor.MiddleCenter, out bool playerFaceCreated);
            if (playerFaceCreated)
            {
                SetRect(playerFace.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), new Vector2(0f, 8f), Vector2.zero);
            }

            Text playerName = EnsureTextChild(playerAvatar, "PlayerAvatarName", "\u4fee\u58eb", 20, FontStyle.Bold, new Color(0.96f, 0.9f, 0.75f, 1f), TextAnchor.MiddleCenter, out bool playerNameCreated);
            if (playerNameCreated)
            {
                SetRect(playerName.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 12f), new Vector2(-18f, 26f));
            }

            EnsureStatusAnchor(playerAvatar, "PlayerBuffAnchor", new Vector2(128f, 66f), new Vector2(290f, 48f), StatusPolarity.Buff, TextAnchor.MiddleLeft);
            EnsureStatusAnchor(playerAvatar, "PlayerDebuffAnchor", new Vector2(128f, 18f), new Vector2(290f, 48f), StatusPolarity.Debuff, TextAnchor.MiddleLeft);
        }

        private static void BuildEnemyArea(Transform safeArea)
        {
            RectTransform panel = EnsureSceneRect(
                safeArea,
                EnemyAreaName,
                out bool created,
                typeof(CanvasRenderer),
                typeof(Image));

            if (created)
            {
                SetRect(panel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(250f, -100f), new Vector2(500f, 1100f));
            }

            EnsureImage(panel.gameObject, EnemyAreaColor, false, created);

            RectTransform enemy = EnsurePanel(panel, "EnemySilhouette", out bool enemyCreated, new Vector2(-330f, 0f), new Vector2(196f, 196f), EnemyAvatarColor);
            Text enemyFace = EnsureTextChild(enemy, "EnemyFace", "\u9996", 70, FontStyle.Bold, new Color(1f, 0.72f, 0.78f, 1f), TextAnchor.MiddleCenter, out bool enemyFaceCreated);
            if (enemyFaceCreated)
            {
                SetRect(enemyFace.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            }

            EnsureStatusAnchor(enemy, "EnemyBuffAnchor", new Vector2(116f, 82f), new Vector2(320f, 48f), StatusPolarity.Buff, TextAnchor.MiddleRight);
            EnsureStatusAnchor(enemy, "EnemyDebuffAnchor", new Vector2(116f, 34f), new Vector2(320f, 48f), StatusPolarity.Debuff, TextAnchor.MiddleRight);

            RectTransform shield = EnsurePanel(panel, "ShieldFeedback", out _, new Vector2(-330f, 0f), new Vector2(220f, 220f), new Color(0.25f, 0.85f, 1f, 0f));
            EnsureImage(shield.gameObject, new Color(0.25f, 0.85f, 1f, 0f), false, false);

            Text title = EnsureTextChild(panel, "EnemyTitle", "\u3010Boss\u3011\u5c71\u9b3c", 34, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft, out bool titleCreated);
            if (titleCreated)
            {
                SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, -34f), new Vector2(420f, 44f));
            }

            Text hp = EnsureTextChild(panel, "EnemyHPText", "800/800 \u6c14\u8840", 28, FontStyle.Bold, new Color(1f, 0.82f, 0.7f, 1f), TextAnchor.MiddleLeft, out bool hpCreated);
            if (hpCreated)
            {
                SetRect(hp.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, -84f), new Vector2(520f, 40f));
            }

            Text weak = EnsureTextChild(panel, "WeaknessTags", "\u5f31\u70b9\uff1a\u62a4\u76fe / \u51c0\u5316", 24, FontStyle.Bold, new Color(0.86f, 1f, 0.72f, 1f), TextAnchor.MiddleLeft, out bool weakCreated);
            if (weakCreated)
            {
                SetRect(weak.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, -132f), new Vector2(540f, 36f));
            }

            RectTransform chargeTrack = EnsurePanel(panel, "ChargeTrack", out _, new Vector2(300f, -76f), new Vector2(520f, 22f), CastTrackColor);
            RectTransform chargeFill = EnsureRectChild(chargeTrack, "ChargeFill", out bool fillCreated, typeof(CanvasRenderer), typeof(Image));
            if (fillCreated)
            {
                SetRect(chargeFill, Vector2.zero, new Vector2(0.78f, 1f), new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero);
            }

            Image fillImage = EnsureImage(chargeFill.gameObject, CastFillColor, false, fillCreated);
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImage.fillAmount = 0.78f;

            Text chargeText = EnsureTextChild(panel, "ChargeText", "\u65bd\u6cd5\u4e2d\uff1a\u9501\u9635\u51b2\u51fb 2.4s", 20, FontStyle.Bold, new Color(1f, 0.76f, 0.5f, 1f), TextAnchor.MiddleLeft, out bool chargeTextCreated);
            if (chargeTextCreated)
            {
                SetRect(chargeText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, -170f), new Vector2(520f, 30f));
            }

            if (enemyCreated)
            {
                EditorUtility.SetDirty(enemy);
            }
        }

        private static RectTransform EnsureFeedbackAnchor(RectTransform parent, string name, Vector2 anchoredPosition)
        {
            RectTransform anchor = EnsureRectChild(parent, name, out bool created);
            if (created)
            {
                SetRect(anchor, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPosition, new Vector2(160f, 40f));
            }

            return anchor;
        }

        private static Text EnsureStatusText(RectTransform parent, string name, string value, Color color)
        {
            Text text = EnsureTextChild(parent, name, value, 27, FontStyle.Bold, color, TextAnchor.MiddleCenter, out bool created);
            if (created)
            {
                text.horizontalOverflow = HorizontalWrapMode.Wrap;
                text.verticalOverflow = VerticalWrapMode.Truncate;
            }

            return text;
        }

        private static Image EnsurePlayerStatusBar(
            Text anchorText,
            string barName,
            Color trackColor,
            Color fillColor,
            float fillAmount)
        {
            if (anchorText == null)
            {
                return null;
            }

            RectTransform textRect = anchorText.rectTransform;
            if (textRect.parent is RectTransform parentRect)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
            }

            RectTransform bar = EnsureRectChild(textRect.parent, barName, out bool barCreated, typeof(CanvasRenderer), typeof(LayoutElement), typeof(Image));
            if (barCreated)
            {
                SetRect(bar, textRect.anchorMin, textRect.anchorMax, textRect.pivot, textRect.anchoredPosition, textRect.sizeDelta);
                bar.SetSiblingIndex(textRect.GetSiblingIndex());
            }

            LayoutElement layoutElement = bar.GetComponent<LayoutElement>();
            if (layoutElement != null)
            {
                layoutElement.ignoreLayout = true;
            }

            EnsureImage(bar.gameObject, trackColor, false, barCreated);

            RectTransform fill = EnsureRectChild(bar, BarFillName, out bool fillCreated, typeof(CanvasRenderer), typeof(Image));
            if (fillCreated)
            {
                SetRect(fill, Vector2.zero, Vector2.one, new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero);
                fill.offsetMin = new Vector2(3f, 3f);
                fill.offsetMax = new Vector2(-3f, -3f);
            }

            Image fillImage = EnsureImage(fill.gameObject, fillColor, false, fillCreated);
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImage.fillAmount = Mathf.Clamp01(fillAmount);
            return fillImage;
        }

        private static RectTransform EnsurePanel(
            RectTransform parent,
            string name,
            out bool created,
            Vector2 anchoredPosition,
            Vector2 size,
            Color color)
        {
            RectTransform rect = EnsureRectChild(parent, name, out created, typeof(CanvasRenderer), typeof(Image));
            if (created)
            {
                SetRect(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPosition, size);
            }

            EnsureImage(rect.gameObject, color, false, created);
            return rect;
        }

        private static void EnsureStatusAnchor(
            RectTransform parent,
            string name,
            Vector2 anchoredPosition,
            Vector2 size,
            StatusPolarity polarity,
            TextAnchor alignment)
        {
            RectTransform rect = EnsureRectChild(parent, name, out bool created, typeof(HorizontalLayoutGroup), typeof(StatusAnchorUI));
            if (created)
            {
                SetRect(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0.5f), anchoredPosition, size);
            }

            HorizontalLayoutGroup layout = rect.GetComponent<HorizontalLayoutGroup>();
            if (layout != null && created)
            {
                layout.childAlignment = alignment;
                layout.spacing = 6f;
                layout.childControlWidth = false;
                layout.childControlHeight = false;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;
            }

            StatusAnchorUI anchor = rect.GetComponent<StatusAnchorUI>();
            if (anchor != null)
            {
                SetField(anchor, "iconAlignment", alignment);
                SetField(anchor, "filterByPolarity", true);
                SetField(anchor, "polarityFilter", polarity);
            }
        }

        private static RectTransform EnsureSceneRect(Transform searchRoot, string name, out bool created, params Type[] extraTypes)
        {
            Transform existing = FindDeepChild(searchRoot, name);
            if (existing != null)
            {
                if (existing is RectTransform existingRect)
                {
                    created = false;
                    return existingRect;
                }

                throw new InvalidOperationException("Existing scene object is not a RectTransform: " + BuildPath(existing));
            }

            return CreateRectChild(searchRoot, name, out created, extraTypes);
        }

        private static RectTransform EnsureRectChild(Transform parent, string name, out bool created, params Type[] extraTypes)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                if (existing is RectTransform existingRect)
                {
                    created = false;
                    EnsureComponents(existingRect.gameObject, extraTypes);
                    return existingRect;
                }

                throw new InvalidOperationException("Existing child is not a RectTransform: " + BuildPath(existing));
            }

            return CreateRectChild(parent, name, out created, extraTypes);
        }

        private static RectTransform CreateRectChild(Transform parent, string name, out bool created, params Type[] extraTypes)
        {
            Type[] componentTypes = MergeComponentTypes(typeof(RectTransform), extraTypes);
            GameObject child = new(name, componentTypes);
            child.transform.SetParent(parent, false);
            RectTransform rect = child.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;
            created = true;
            return rect;
        }

        private static void EnsureComponents(GameObject target, params Type[] extraTypes)
        {
            for (int i = 0; i < extraTypes.Length; i++)
            {
                Type type = extraTypes[i];
                if (type == null || target.GetComponent(type) != null)
                {
                    continue;
                }

                target.AddComponent(type);
            }
        }

        private static Type[] MergeComponentTypes(Type required, Type[] extraTypes)
        {
            if (extraTypes == null || extraTypes.Length == 0)
            {
                return new[] { required };
            }

            Type[] merged = new Type[extraTypes.Length + 1];
            merged[0] = required;
            for (int i = 0; i < extraTypes.Length; i++)
            {
                merged[i + 1] = extraTypes[i];
            }

            return merged;
        }

        private static Text EnsureTextChild(
            Transform parent,
            string name,
            string value,
            int fontSize,
            FontStyle style,
            Color color,
            TextAnchor alignment,
            out bool created)
        {
            Transform existing = parent.Find(name);
            GameObject textObject;
            bool componentCreated = false;
            if (existing == null)
            {
                textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                textObject.transform.SetParent(parent, false);
                created = true;
            }
            else
            {
                textObject = existing.gameObject;
                created = false;
            }

            Text text = textObject.GetComponent<Text>();
            if (text == null)
            {
                text = textObject.AddComponent<Text>();
                componentCreated = true;
            }

            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;
            if (created || componentCreated)
            {
                text.text = value ?? string.Empty;
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                text.fontSize = fontSize;
                text.fontStyle = style;
                text.color = color;
                text.alignment = alignment;
                text.raycastTarget = false;
            }

            return text;
        }

        private static Image EnsureImage(GameObject target, Color color, bool raycast, bool applyDefaults)
        {
            Image image = target.GetComponent<Image>();
            bool created = image == null;
            if (image == null)
            {
                image = target.AddComponent<Image>();
            }

            if (created || applyDefaults)
            {
                image.color = color;
                image.raycastTarget = raycast;
            }

            return image;
        }

        private static Transform RequireRoot(string rootName)
        {
            GameObject target = GameObject.Find(rootName);
            if (target == null)
            {
                throw new InvalidOperationException("Missing root: " + rootName);
            }

            return target.transform;
        }

        private static Transform RequireChild(Transform parent, string childName)
        {
            Transform child = FindDeepChild(parent, childName);
            if (child == null)
            {
                throw new InvalidOperationException("Missing child: " + BuildPath(parent) + "/" + childName);
            }

            return child;
        }

        private static Transform FindDeepChild(Transform parent, string objectName)
        {
            if (parent == null)
            {
                return null;
            }

            foreach (Transform child in parent)
            {
                if (child.name == objectName)
                {
                    return child;
                }

                Transform found = FindDeepChild(child, objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static void SetRect(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            rect.localScale = Vector3.one;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            if (target == null)
            {
                return;
            }

            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null)
            {
                Debug.LogWarning($"Field '{fieldName}' not found on {target.GetType().Name}.");
                return;
            }

            field.SetValue(target, value);
            if (target is UnityEngine.Object unityObject)
            {
                EditorUtility.SetDirty(unityObject);
            }
        }

        private static string BuildPath(Transform target)
        {
            if (target == null)
            {
                return string.Empty;
            }

            string path = target.name;
            Transform parent = target.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }
    }
}
#endif
