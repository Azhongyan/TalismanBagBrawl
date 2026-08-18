#if UNITY_EDITOR
using System;
using TalismanBag.BuildSandbox;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BattleSandboxEnemyCombatFeedbackUiReuseSceneBinder
    {
        public const string ManualMenuPath =
            "Tools/Talisman Bag/Dev Only/V0.4/BuildSandbox/BattleSandboxEnemyCombatFeedbackUiReuse01/[Writes Scene][Manual Only] Bind Combat Feedback UI";

        private static readonly Color PanelColor = new(0.09f, 0.105f, 0.105f, 0.95f);
        private static readonly Color SlotColor = new(0.14f, 0.155f, 0.15f, 0.96f);
        private static readonly Color AccentColor = new(0.52f, 0.31f, 0.18f, 1f);
        private static readonly Color CastBackColor = new(0.07f, 0.075f, 0.075f, 0.96f);
        private static readonly Color TextColor = new(0.90f, 0.88f, 0.78f, 1f);

        [MenuItem(ManualMenuPath)]
        public static void BindEnemyCombatFeedbackMenu()
        {
            if (!EditorUtility.DisplayDialog(
                    "[Writes Scene][Manual Only] Bind Combat Feedback UI",
                    "This writes only the V04 BuildSandbox preview scene.\n\n" +
                    BuildSandboxPreviewSceneMarker.ScenePath + "\n\n" +
                    "It does not modify formal RunFlow, formal combat, rewards, save data, feature flags, or V02/V03 product scenes.",
                    "Bind",
                    "Cancel"))
            {
                return;
            }

            BindEnemyCombatFeedbackScene();
        }

        public static void BindEnemyCombatFeedbackSceneBatch()
        {
            string path = BindEnemyCombatFeedbackScene();
            Debug.Log("[V0.4-BattleSandboxEnemyCombatFeedbackUiReuse01] ENEMY_COMBAT_FEEDBACK_BOUND path=" + path);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(0);
            }
        }

        public static string BindEnemyCombatFeedbackScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException("Enemy combat feedback binding must run in Edit Mode.");
            }

            Scene previousScene = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.OpenScene(BuildSandboxPreviewSceneMarker.ScenePath, OpenSceneMode.Single);
            try
            {
                Transform root = RequireRoot("BuildSandboxPreviewRoot");
                Transform canvas = RequireRoot("BuildSandboxPreviewCanvas");
                Transform safeArea = RequireChild(canvas, "SafeAreaRoot");
                RectTransform problemPanel = RequireRect(RequireChild(safeArea, "ProblemSelectorPanel"));
                RectTransform battleArea = RequireRect(RequireChild(safeArea, "BattleLikePreviewArea"));
                RectTransform dataDock = RequireRect(RequireChild(safeArea, "BuildSandboxDataPanelDock"));
                RectTransform popupLayer = RequireRect(RequireChild(safeArea, "PopupLayer"));

                RemoveObsoleteEncounterPreview(root, safeArea);
                LocalizeSandboxSlots(problemPanel, dataDock);

                BuildControlPanel(
                    problemPanel,
                    out Text controlStatus,
                    out Button previousFeedback,
                    out Button nextFeedback,
                    out Button triggerFloating);
                BuildBattleFeedbackPanel(
                    battleArea,
                    out Text title,
                    out Text bossState,
                    out Text bossSkill,
                    out Text castTimer,
                    out Image castFill,
                    out Text combatLog);
                BuildFloatingFeedbackRoot(
                    popupLayer,
                    out Text floatingText,
                    out CanvasGroup floatingCanvasGroup);
                BuildDeveloperMaskPanel(dataDock);

                BattleSandboxEnemyCombatFeedbackController controller = EnsureController(root);
                controller.Bind(
                    title,
                    bossState,
                    bossSkill,
                    castTimer,
                    castFill,
                    floatingText,
                    floatingCanvasGroup,
                    combatLog,
                    controlStatus,
                    previousFeedback,
                    nextFeedback,
                    triggerFloating);

                EditorUtility.SetDirty(controller);
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene, BuildSandboxPreviewSceneMarker.ScenePath))
                {
                    throw new InvalidOperationException("Could not save " + BuildSandboxPreviewSceneMarker.ScenePath);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[V0.4-BattleSandboxEnemyCombatFeedbackUiReuse01] ENEMY_COMBAT_FEEDBACK_BOUND path=" +
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

        private static void RemoveObsoleteEncounterPreview(Transform root, Transform safeArea)
        {
            DestroyIfPresent(FindDeepChild(safeArea, "EnemyEncounterSelectorPanel"));
            DestroyIfPresent(FindDeepChild(safeArea, "EnemyEncounterPreviewPanel"));
            DestroyIfPresent(FindDeepChild(root, "EnemyEncounterPreviewRuntime"));
        }

        private static void LocalizeSandboxSlots(RectTransform problemPanel, RectTransform dataDock)
        {
            SetTextIfPresent(problemPanel, "Title", "沙盒选择");
            SetTextIfPresent(problemPanel, "MapRuleDropdownSlot", "地图规则");
            SetTextIfPresent(problemPanel, "EnemyProblemDropdownSlot", "敌人压力");
            SetTextIfPresent(problemPanel, "BossProblemDropdownSlot", "首领机制");
            SetTextIfPresent(problemPanel, "DevChapterDropdownSlot", "开发章节");
            SetTextIfPresent(dataDock, "Title", "沙盒数据 / 开发者遮罩");

            SetSlotAnchors(problemPanel, "MapRuleDropdownSlot", 0.82f);
            SetSlotAnchors(problemPanel, "EnemyProblemDropdownSlot", 0.67f);
            SetSlotAnchors(problemPanel, "BossProblemDropdownSlot", 0.52f);
            SetSlotAnchors(problemPanel, "DevChapterDropdownSlot", 0.37f);
            CompressDataDockSlots(dataDock);
        }

        private static void BuildControlPanel(
            RectTransform problemPanel,
            out Text controlStatus,
            out Button previousFeedback,
            out Button nextFeedback,
            out Button triggerFloating)
        {
            RectTransform panel = EnsureRectChild(problemPanel, "EnemyCombatFeedbackControlPanel");
            SetAnchors(panel, new Vector2(0.06f, 0.03f), new Vector2(0.94f, 0.29f));
            EnsureImage(panel.gameObject, PanelColor, raycast: false);
            EnsureOutline(panel);

            Text title = EnsureChildText(panel, "Title", "战斗反馈预览", 17, TextAnchor.MiddleCenter);
            SetAnchors(title.rectTransform, new Vector2(0.04f, 0.76f), new Vector2(0.96f, 0.98f));

            previousFeedback = EnsureButton(panel, "PreviousFeedbackButton", "上一反馈", new Vector2(0.05f, 0.52f), new Vector2(0.47f, 0.72f));
            nextFeedback = EnsureButton(panel, "NextFeedbackButton", "下一反馈", new Vector2(0.53f, 0.52f), new Vector2(0.95f, 0.72f));
            triggerFloating = EnsureButton(panel, "TriggerFloatingFeedbackButton", "触发短句", new Vector2(0.05f, 0.28f), new Vector2(0.95f, 0.48f));

            controlStatus = EnsureChildText(panel, "ControlStatusText", "只显示状态、施法、机制短句", 13, TextAnchor.MiddleCenter);
            SetAnchors(controlStatus.rectTransform, new Vector2(0.05f, 0.06f), new Vector2(0.95f, 0.24f));
        }

        private static void BuildBattleFeedbackPanel(
            RectTransform battleArea,
            out Text title,
            out Text bossState,
            out Text bossSkill,
            out Text castTimer,
            out Image castFill,
            out Text combatLog)
        {
            RectTransform panel = EnsureRectChild(battleArea, "EnemyCombatFeedbackPanel");
            SetAnchors(panel, new Vector2(0.08f, 0.66f), new Vector2(0.92f, 0.91f));
            EnsureImage(panel.gameObject, new Color(0.055f, 0.065f, 0.065f, 0.92f), raycast: false);
            EnsureOutline(panel);

            title = EnsureChildText(panel, "Title", "战斗反馈预览", 19, TextAnchor.MiddleCenter);
            title.color = new Color(1f, 0.86f, 0.50f, 1f);
            SetAnchors(title.rectTransform, new Vector2(0.03f, 0.75f), new Vector2(0.97f, 0.97f));

            bossState = EnsureChildText(panel, "BossStateText", "首领气息翻涌", 18, TextAnchor.MiddleLeft);
            bossState.color = new Color(1f, 0.82f, 0.35f, 1f);
            SetAnchors(bossState.rectTransform, new Vector2(0.05f, 0.55f), new Vector2(0.95f, 0.75f));

            bossSkill = EnsureChildText(panel, "BossSkillText", "施法中：锁阵冲击", 17, TextAnchor.MiddleLeft);
            bossSkill.color = new Color(0.95f, 0.94f, 0.88f, 1f);
            SetAnchors(bossSkill.rectTransform, new Vector2(0.05f, 0.36f), new Vector2(0.70f, 0.54f));

            RectTransform castBar = EnsureRectChild(panel, "BossCastBarRoot");
            SetAnchors(castBar, new Vector2(0.05f, 0.20f), new Vector2(0.95f, 0.34f));
            EnsureImage(castBar.gameObject, CastBackColor, raycast: false);
            EnsureOutline(castBar);

            RectTransform fillRect = EnsureRectChild(castBar, "BossCastFill");
            SetAnchors(fillRect, Vector2.zero, Vector2.one);
            castFill = EnsureImage(fillRect.gameObject, new Color(1f, 0.48f, 0.30f, 1f), raycast: false);
            castFill.type = Image.Type.Filled;
            castFill.fillMethod = Image.FillMethod.Horizontal;
            castFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            castFill.fillAmount = 0.88f;

            castTimer = EnsureChildText(castBar, "BossCastTimerText", "2.4秒", 16, TextAnchor.MiddleCenter);
            castTimer.color = Color.white;
            SetAnchors(castTimer.rectTransform, Vector2.zero, Vector2.one);

            combatLog = EnsureChildText(panel, "CombatLogText", "【机制】阵面压力升高。", 15, TextAnchor.MiddleLeft);
            combatLog.color = new Color(0.74f, 1f, 0.70f, 1f);
            SetAnchors(combatLog.rectTransform, new Vector2(0.05f, 0.03f), new Vector2(0.95f, 0.18f));
        }

        private static void BuildFloatingFeedbackRoot(
            RectTransform popupLayer,
            out Text floatingText,
            out CanvasGroup floatingCanvasGroup)
        {
            RectTransform root = EnsureRectChild(popupLayer, "EnemyCombatFeedbackFloatingRoot");
            SetAnchors(root, Vector2.zero, Vector2.one);
            EnsureImage(root.gameObject, new Color(0f, 0f, 0f, 0f), raycast: false);

            floatingText = EnsureChildText(root, "MechanicFloatingText", "破绽窗口出现", 28, TextAnchor.MiddleCenter);
            floatingText.fontStyle = FontStyle.Bold;
            floatingText.color = new Color(1f, 0.95f, 0.45f, 1f);
            RectTransform textRect = floatingText.rectTransform;
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = new Vector2(120f, 110f);
            textRect.sizeDelta = new Vector2(360f, 54f);
            textRect.localScale = Vector3.one;

            floatingCanvasGroup = floatingText.GetComponent<CanvasGroup>();
            if (floatingCanvasGroup == null)
            {
                floatingCanvasGroup = floatingText.gameObject.AddComponent<CanvasGroup>();
            }

            floatingCanvasGroup.alpha = 1f;
        }

        private static void BuildDeveloperMaskPanel(RectTransform dataDock)
        {
            RectTransform panel = EnsureRectChild(dataDock, "EnemyCombatFeedbackDeveloperPanel");
            SetAnchors(panel, new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.51f));
            EnsureImage(panel.gameObject, new Color(0.10f, 0.115f, 0.11f, 0.96f), raycast: false);
            EnsureOutline(panel);

            Text title = EnsureChildText(panel, "Title", "开发者数据遮罩", 18, TextAnchor.MiddleCenter);
            title.color = new Color(1f, 0.86f, 0.50f, 1f);
            SetAnchors(title.rectTransform, new Vector2(0.03f, 0.86f), new Vector2(0.97f, 0.98f));

            Text body = EnsureChildText(
                panel,
                "MaskedDeveloperFieldsText",
                "hardSolutionTags / requiredSynergy / requiredAffix / requiredStats\nDropBias 权重 / 首领六钥匙：仅开发者数据面板与报告",
                13,
                TextAnchor.MiddleLeft);
            body.horizontalOverflow = HorizontalWrapMode.Wrap;
            body.verticalOverflow = VerticalWrapMode.Truncate;
            SetAnchors(body.rectTransform, new Vector2(0.05f, 0.54f), new Vector2(0.95f, 0.84f));

            Text scope = EnsureChildText(
                panel,
                "CombatFeedbackIsolationText",
                "玩家侧只看首领技能、状态、施法条与机制短句；不触发正式战斗、奖励、存档或章节推进。",
                13,
                TextAnchor.MiddleLeft);
            scope.horizontalOverflow = HorizontalWrapMode.Wrap;
            scope.verticalOverflow = VerticalWrapMode.Truncate;
            SetAnchors(scope.rectTransform, new Vector2(0.05f, 0.30f), new Vector2(0.95f, 0.52f));

            Text reuse = EnsureChildText(
                panel,
                "CombatFeedbackReuseText",
                "复用口径：浮字短促上飘、敌人意图倒计时、首领状态摘要。",
                13,
                TextAnchor.MiddleLeft);
            reuse.horizontalOverflow = HorizontalWrapMode.Wrap;
            reuse.verticalOverflow = VerticalWrapMode.Truncate;
            SetAnchors(reuse.rectTransform, new Vector2(0.05f, 0.12f), new Vector2(0.95f, 0.28f));
        }

        private static void CompressDataDockSlots(RectTransform dataDock)
        {
            string[] slots =
            {
                "BuildSummaryPanelSlot",
                "SynergyPanelSlot",
                "ShapeOccupancyPanelSlot",
                "AffixModifierPanelSlot",
                "ProblemReadinessPanelSlot",
                "SimulationResultPanelSlot"
            };

            for (int i = 0; i < slots.Length; i++)
            {
                Transform slot = dataDock.Find(slots[i]);
                if (slot is not RectTransform rect)
                {
                    continue;
                }

                float top = 0.84f - i * 0.055f;
                SetAnchors(rect, new Vector2(0.08f, top - 0.024f), new Vector2(0.92f, top + 0.024f));
                Text label = rect.GetComponentInChildren<Text>(true);
                if (label != null)
                {
                    label.fontSize = 13;
                }
            }
        }

        private static BattleSandboxEnemyCombatFeedbackController EnsureController(Transform root)
        {
            Transform runtime = root.Find("EnemyCombatFeedbackRuntime");
            if (runtime == null)
            {
                GameObject runtimeObject = new("EnemyCombatFeedbackRuntime", typeof(RectTransform));
                runtimeObject.transform.SetParent(root, false);
                runtime = runtimeObject.transform;
            }

            BattleSandboxEnemyCombatFeedbackController controller =
                runtime.GetComponent<BattleSandboxEnemyCombatFeedbackController>();
            if (controller == null)
            {
                controller = runtime.gameObject.AddComponent<BattleSandboxEnemyCombatFeedbackController>();
            }

            return controller;
        }

        private static Button EnsureButton(
            RectTransform parent,
            string name,
            string label,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            RectTransform rect = EnsureRectChild(parent, name);
            SetAnchors(rect, anchorMin, anchorMax);
            Image image = EnsureImage(rect.gameObject, AccentColor, raycast: true);
            Button button = rect.GetComponent<Button>();
            if (button == null)
            {
                button = rect.gameObject.AddComponent<Button>();
            }

            ColorBlock colors = button.colors;
            colors.normalColor = image.color;
            colors.highlightedColor = new Color(0.62f, 0.42f, 0.24f, 1f);
            colors.pressedColor = new Color(0.34f, 0.22f, 0.13f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            Text text = EnsureChildText(rect, "Label", label, 13, TextAnchor.MiddleCenter);
            text.color = Color.white;
            SetAnchors(text.rectTransform, Vector2.zero, Vector2.one);
            return button;
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
                throw new InvalidOperationException($"Missing child: {BuildPath(parent)}/{childName}");
            }

            return child;
        }

        private static RectTransform RequireRect(Transform target)
        {
            if (target is not RectTransform rect)
            {
                throw new InvalidOperationException("Missing RectTransform: " + BuildPath(target));
            }

            return rect;
        }

        private static RectTransform EnsureRectChild(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing is RectTransform rect)
            {
                return rect;
            }

            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing.gameObject);
            }

            GameObject child = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            child.transform.SetParent(parent, false);
            RectTransform childRect = child.GetComponent<RectTransform>();
            childRect.localScale = Vector3.one;
            return childRect;
        }

        private static Text EnsureChildText(Transform parent, string name, string value, int size, TextAnchor alignment)
        {
            Transform existing = parent.Find(name);
            GameObject textObject;
            if (existing == null)
            {
                textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                textObject.transform.SetParent(parent, false);
            }
            else
            {
                textObject = existing.gameObject;
                if (textObject.GetComponent<Text>() == null)
                {
                    textObject.AddComponent<Text>();
                }
            }

            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;
            Text text = textObject.GetComponent<Text>();
            text.text = value ?? string.Empty;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.alignment = alignment;
            text.color = TextColor;
            text.raycastTarget = false;
            return text;
        }

        private static Image EnsureImage(GameObject target, Color color, bool raycast)
        {
            Image image = target.GetComponent<Image>();
            if (image == null)
            {
                image = target.AddComponent<Image>();
            }

            image.color = color;
            image.raycastTarget = raycast;
            return image;
        }

        private static void EnsureOutline(RectTransform target)
        {
            Outline outline = target.GetComponent<Outline>();
            if (outline == null)
            {
                outline = target.gameObject.AddComponent<Outline>();
            }

            outline.effectColor = new Color(0.43f, 0.36f, 0.22f, 0.78f);
            outline.effectDistance = new Vector2(1f, -1f);
        }

        private static void SetTextIfPresent(Transform parent, string name, string value)
        {
            Transform child = FindDeepChild(parent, name);
            Text text = child == null ? null : child.GetComponent<Text>();
            if (text != null)
            {
                text.text = value ?? string.Empty;
            }
        }

        private static void SetSlotAnchors(Transform parent, string slotName, float centerY)
        {
            Transform slot = parent.Find(slotName);
            if (slot is RectTransform rect)
            {
                SetAnchors(rect, new Vector2(0.08f, centerY - 0.052f), new Vector2(0.92f, centerY + 0.052f));
            }
        }

        private static void SetAnchors(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        private static void DestroyIfPresent(Transform target)
        {
            if (target != null)
            {
                UnityEngine.Object.DestroyImmediate(target.gameObject);
            }
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
