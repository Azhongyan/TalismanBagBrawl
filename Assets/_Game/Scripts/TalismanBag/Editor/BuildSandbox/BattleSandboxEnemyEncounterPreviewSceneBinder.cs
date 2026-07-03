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
    public static class BattleSandboxEnemyEncounterPreviewSceneBinder
    {
        public const string ManualMenuPath =
            "Tools/Talisman Bag/V0.4/BuildSandbox/BattleSandboxEnemyEncounterPreview01/[Writes Scene][Manual Only] Bind Enemy Encounter Preview";

        private static readonly Color PanelColor = new(0.11f, 0.125f, 0.12f, 0.96f);
        private static readonly Color SlotColor = new(0.18f, 0.19f, 0.16f, 0.96f);
        private static readonly Color AccentColor = new(0.46f, 0.36f, 0.18f, 1f);
        private static readonly Color TextColor = new(0.90f, 0.87f, 0.76f, 1f);

        [MenuItem(ManualMenuPath)]
        public static void BindEnemyEncounterPreviewMenu()
        {
            if (!EditorUtility.DisplayDialog(
                    "[Writes Scene][Manual Only] Bind Enemy Encounter Preview",
                    "This writes only the V04 BuildSandbox preview scene.\n\n" +
                    BuildSandboxPreviewSceneMarker.ScenePath + "\n\n" +
                    "It does not modify formal RunFlow, formal combat, rewards, save data, or V02/V03 product scenes.",
                    "Bind",
                    "Cancel"))
            {
                return;
            }

            BindEnemyEncounterPreviewScene();
        }

        public static void BindEnemyEncounterPreviewSceneBatch()
        {
            string path = BindEnemyEncounterPreviewScene();
            Debug.Log("[V0.4-BattleSandboxEnemyEncounterPreview01] ENEMY_ENCOUNTER_PREVIEW_BOUND path=" + path);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(0);
            }
        }

        public static string BindEnemyEncounterPreviewScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException("Enemy encounter preview binding must run in Edit Mode.");
            }

            Scene previousScene = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.OpenScene(BuildSandboxPreviewSceneMarker.ScenePath, OpenSceneMode.Single);
            try
            {
                Transform root = RequireRoot("BuildSandboxPreviewRoot");
                Transform canvas = RequireRoot("BuildSandboxPreviewCanvas");
                Transform safeArea = RequireChild(canvas, "SafeAreaRoot");
                RectTransform problemPanel = RequireRect(RequireChild(safeArea, "ProblemSelectorPanel"));
                RectTransform dataDock = RequireRect(RequireChild(safeArea, "BuildSandboxDataPanelDock"));

                LocalizeProblemSelector(problemPanel);
                Text selectorTitle = BuildSelectorPanel(
                    problemPanel,
                    out Button previousEnemy,
                    out Button nextEnemy,
                    out Button previousBoss,
                    out Button nextBoss,
                    out Button readinessButton);
                BuildPreviewPanel(
                    dataDock,
                    out Text enemyInfo,
                    out Text mechanicHint,
                    out Text weaknessWindow,
                    out Text readinessPreview,
                    out Text failureFeedback,
                    out Text dropBiasHint,
                    out Text testTarget,
                    out Text isolation);

                BattleSandboxEnemyEncounterPreviewController controller = EnsureController(root);
                controller.Bind(
                    selectorTitle,
                    enemyInfo,
                    mechanicHint,
                    weaknessWindow,
                    readinessPreview,
                    failureFeedback,
                    dropBiasHint,
                    testTarget,
                    isolation,
                    previousEnemy,
                    nextEnemy,
                    previousBoss,
                    nextBoss,
                    readinessButton);

                EditorUtility.SetDirty(controller);
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene, BuildSandboxPreviewSceneMarker.ScenePath))
                {
                    throw new InvalidOperationException("Could not save " + BuildSandboxPreviewSceneMarker.ScenePath);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[V0.4-BattleSandboxEnemyEncounterPreview01] ENEMY_ENCOUNTER_PREVIEW_BOUND path=" +
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

        private static void LocalizeProblemSelector(RectTransform problemPanel)
        {
            SetTextIfPresent(problemPanel, "Title", "题目选择");
            SetTextIfPresent(problemPanel, "MapRuleDropdownSlot", "地图规则");
            SetTextIfPresent(problemPanel, "EnemyProblemDropdownSlot", "敌人题目");
            SetTextIfPresent(problemPanel, "BossProblemDropdownSlot", "首领题目");
            SetTextIfPresent(problemPanel, "DevChapterDropdownSlot", "开发章节");

            SetSlotAnchors(problemPanel, "MapRuleDropdownSlot", 0.82f);
            SetSlotAnchors(problemPanel, "EnemyProblemDropdownSlot", 0.67f);
            SetSlotAnchors(problemPanel, "BossProblemDropdownSlot", 0.52f);
            SetSlotAnchors(problemPanel, "DevChapterDropdownSlot", 0.37f);
        }

        private static Text BuildSelectorPanel(
            RectTransform problemPanel,
            out Button previousEnemy,
            out Button nextEnemy,
            out Button previousBoss,
            out Button nextBoss,
            out Button readinessButton)
        {
            RectTransform selectorPanel = EnsureRectChild(problemPanel, "EnemyEncounterSelectorPanel");
            SetAnchors(selectorPanel, new Vector2(0.06f, 0.03f), new Vector2(0.94f, 0.29f));
            EnsureImage(selectorPanel.gameObject, PanelColor, raycast: false);
            EnsureOutline(selectorPanel);

            Text selectorTitle = EnsureChildText(
                selectorPanel,
                "EnemySelector",
                "敌人与首领预览",
                17,
                TextAnchor.MiddleCenter);
            SetAnchors(selectorTitle.rectTransform, new Vector2(0.04f, 0.76f), new Vector2(0.96f, 0.98f));

            previousEnemy = EnsureButton(selectorPanel, "PreviousEnemyButton", "上一敌人", new Vector2(0.05f, 0.52f), new Vector2(0.47f, 0.72f));
            nextEnemy = EnsureButton(selectorPanel, "NextEnemyButton", "下一敌人", new Vector2(0.53f, 0.52f), new Vector2(0.95f, 0.72f));
            previousBoss = EnsureButton(selectorPanel, "PreviousBossButton", "上一首领", new Vector2(0.05f, 0.28f), new Vector2(0.47f, 0.48f));
            nextBoss = EnsureButton(selectorPanel, "NextBossButton", "下一首领", new Vector2(0.53f, 0.28f), new Vector2(0.95f, 0.48f));
            readinessButton = EnsureButton(selectorPanel, "ReadinessPreviewButton", "刷新准备度", new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.24f));
            return selectorTitle;
        }

        private static void BuildPreviewPanel(
            RectTransform dataDock,
            out Text enemyInfo,
            out Text mechanicHint,
            out Text weaknessWindow,
            out Text readinessPreview,
            out Text failureFeedback,
            out Text dropBiasHint,
            out Text testTarget,
            out Text isolation)
        {
            SetTextIfPresent(dataDock, "Title", "沙盒数据 / 题目提示");
            CompressDataDockSlots(dataDock);

            RectTransform panel = EnsureRectChild(dataDock, "EnemyEncounterPreviewPanel");
            SetAnchors(panel, new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.51f));
            EnsureImage(panel.gameObject, PanelColor, raycast: false);
            EnsureOutline(panel);

            Text title = EnsureChildText(panel, "Title", "敌人与首领题目预览", 18, TextAnchor.MiddleCenter);
            SetAnchors(title.rectTransform, new Vector2(0.03f, 0.90f), new Vector2(0.97f, 0.99f));

            enemyInfo = Block(panel, "EnemyInfoBlock", "当前题目：开发专用预览", 0.78f, 0.90f);
            mechanicHint = Block(panel, "MechanicHintBlock", "地图和机制线索会显示在这里。", 0.54f, 0.77f);
            weaknessWindow = Block(panel, "WeaknessWindowBlock", "弱点窗口只提示时机。", 0.42f, 0.53f);
            readinessPreview = Block(panel, "ReadinessPreviewBlock", "准备度预览不显示完整答案。", 0.30f, 0.41f);
            failureFeedback = Block(panel, "FailureFeedbackBlock", "失败反馈只提示短板方向。", 0.20f, 0.29f);
            dropBiasHint = Block(panel, "DropBiasHintBlock", "战后气息不显示权重或清单。", 0.12f, 0.19f);
            testTarget = Block(panel, "TestTargetBlock", "测试目标：构筑短板。", 0.06f, 0.115f);
            isolation = Block(panel, "FormalIsolationBlock", "开发专用预览：不触发正式战斗、不发奖励、不写存档、不推进章节。", 0.005f, 0.055f);
        }

        private static Text Block(RectTransform panel, string name, string value, float minY, float maxY)
        {
            Text text = EnsureChildText(panel, name, value, 13, TextAnchor.MiddleLeft);
            SetAnchors(text.rectTransform, new Vector2(0.04f, minY), new Vector2(0.96f, maxY));
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
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

        private static BattleSandboxEnemyEncounterPreviewController EnsureController(Transform root)
        {
            Transform runtime = root.Find("EnemyEncounterPreviewRuntime");
            if (runtime == null)
            {
                GameObject runtimeObject = new("EnemyEncounterPreviewRuntime", typeof(RectTransform));
                runtimeObject.transform.SetParent(root, false);
                runtime = runtimeObject.transform;
            }

            BattleSandboxEnemyEncounterPreviewController controller =
                runtime.GetComponent<BattleSandboxEnemyEncounterPreviewController>();
            if (controller == null)
            {
                controller = runtime.gameObject.AddComponent<BattleSandboxEnemyEncounterPreviewController>();
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
            colors.highlightedColor = new Color(0.57f, 0.44f, 0.21f, 1f);
            colors.pressedColor = new Color(0.32f, 0.25f, 0.13f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            Text text = EnsureChildText(rect, "Label", label, 13, TextAnchor.MiddleCenter);
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

            outline.effectColor = new Color(0.43f, 0.37f, 0.24f, 0.75f);
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

        private static Transform FindDeepChild(Transform parent, string objectName)
        {
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
