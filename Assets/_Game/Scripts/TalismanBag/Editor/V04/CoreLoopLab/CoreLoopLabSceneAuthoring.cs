using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TalismanBag.BuildSandbox;
using TalismanBag.V04.ChapterFlow.Chapter1.Presentation;
using TalismanBag.V04.CoreLoopLab;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TalismanBag.Editor.V04.CoreLoopLab
{
    public static class CoreLoopLabSceneAuthoring
    {
        private const string SourceScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity";
        private const string TargetScenePath = CoreLoopLabController.ScenePath;
        private const string MenuRoot =
            "Tools/TalismanBag/V04/Core Loop ABC Product Direction Lab/";

        [MenuItem(MenuRoot + "Author Lab Scene (one-time)")]
        public static void AuthorFromMenu()
        {
            AuthorLabScene();
        }

        public static void AuthorFromCommandLine()
        {
            AuthorLabScene();
        }

        [MenuItem(MenuRoot + "Play Profile A")]
        public static void PlayProfileA()
        {
            LaunchProfile(CoreLoopLabProfile.A_CurrentSingleBattle);
        }

        [MenuItem(MenuRoot + "Play Profile B")]
        public static void PlayProfileB()
        {
            LaunchProfile(CoreLoopLabProfile.B_TwoBattleRewardLoop);
        }

        [MenuItem(MenuRoot + "Play Profile C")]
        public static void PlayProfileC()
        {
            LaunchProfile(
                CoreLoopLabProfile.C_TwoBattleSemanticPresentation);
        }

        private static void AuthorLabScene()
        {
            if (!File.Exists(SourceScenePath))
            {
                throw new FileNotFoundException(
                    "CoreLoopLab source Scene is missing.",
                    SourceScenePath);
            }

            if (File.Exists(TargetScenePath))
            {
                throw new InvalidOperationException(
                    "CoreLoopLab target already exists. Refusing a second authoring pass: "
                    + TargetScenePath);
            }

            byte[] sourceBytesBefore = File.ReadAllBytes(SourceScenePath);
            Scene scene = EditorSceneManager.OpenScene(
                SourceScenePath,
                OpenSceneMode.Single);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                throw new InvalidOperationException(
                    "Failed to load the BattleSandbox source Scene.");
            }

            if (!EditorSceneManager.SaveScene(
                    scene,
                    TargetScenePath,
                    saveAsCopy: true))
            {
                throw new InvalidOperationException(
                    "Save-As failed for CoreLoopLab target Scene.");
            }

            BuildGridInteractionPreviewController gridController =
                RequireUniqueComponent<BuildGridInteractionPreviewController>(
                    scene,
                    "BuildGridInteractionPreviewRuntime");
            BattleSandboxManaLoopRuntime manaLoop =
                GetOrAdd<BattleSandboxManaLoopRuntime>(
                    gridController.gameObject);
            BattleSandboxRuntimeLoopRuntime runtimeLoop =
                GetOrAdd<BattleSandboxRuntimeLoopRuntime>(
                    gridController.gameObject);
            BattleSandboxItemTriggerFeedbackController triggerPresentation =
                GetOrAdd<BattleSandboxItemTriggerFeedbackController>(
                    gridController.gameObject);
            EditorUtility.SetDirty(manaLoop);
            EditorUtility.SetDirty(runtimeLoop);
            EditorUtility.SetDirty(triggerPresentation);

            Canvas canvas = RequireUniqueComponent<Canvas>(
                scene,
                "BuildSandboxPreviewCanvas");
            RectTransform canvasRect = canvas.transform as RectTransform
                ?? throw new InvalidOperationException(
                    "BuildSandboxPreviewCanvas must use RectTransform.");
            Font font = RequireUniqueComponent<Text>(
                scene,
                "CurrentLevelText").font;
            Button prepareToggle = RequireUniqueComponent<Button>(
                scene,
                "V04BattlePrepareToggleButton");
            Button prepareContinue = RequireUniqueComponent<Button>(
                scene,
                "V04BattlePrepareStateButton");
            RectTransform boardGrid = RequireUniqueComponent<RectTransform>(
                scene,
                "BoardGridPreview");
            C1AuthoredEnemySelectionOutlineEffect targetOutline =
                RequireUniqueComponent<C1AuthoredEnemySelectionOutlineEffect>(
                    scene,
                    "Enemy");
            RectTransform enemyHitTarget = targetOutline.transform
                as RectTransform;

            RectTransform boardArtworkLayer = CreateRectTransform(
                "BoardItemArtworkLayer",
                boardGrid);
            Stretch(boardArtworkLayer);
            LayoutElement artworkLayout =
                boardArtworkLayer.gameObject.AddComponent<LayoutElement>();
            artworkLayout.ignoreLayout = true;
            CanvasGroup coarsePresentationLayer = CreateCanvasGroupRoot(
                "BattleSandboxItemTriggerFeedbackFxLayer",
                boardArtworkLayer);
            Stretch(coarsePresentationLayer.transform as RectTransform);

            RectTransform runtimeRoot = CreateRectTransform(
                "CoreLoopLabRuntimeRoot",
                canvasRect);
            Stretch(runtimeRoot);
            CoreLoopLabController labController =
                runtimeRoot.gameObject.AddComponent<CoreLoopLabController>();

            RectTransform presentationRoot = CreateRectTransform(
                "CoreLoopLabPresentationRoot",
                canvasRect);
            Stretch(presentationRoot);
            CoreLoopLabPresentationAdapter presentationAdapter =
                presentationRoot.gameObject
                    .AddComponent<CoreLoopLabPresentationAdapter>();
            CanvasGroup semanticGroup = CreateCanvasGroupRoot(
                "CoreLoopLabSemanticFloating",
                presentationRoot);
            RectTransform semanticRect = semanticGroup.transform
                as RectTransform;
            SetAnchoredRect(
                semanticRect,
                new Vector2(0.72f, 0.54f),
                new Vector2(0.72f, 0.54f),
                new Vector2(0.5f, 0.5f),
                new Vector2(300f, 92f),
                Vector2.zero);
            Text semanticText = CreateText(
                "SemanticText",
                semanticRect,
                font,
                30,
                TextAnchor.MiddleCenter,
                new Color(1f, 0.88f, 0.38f, 1f));
            semanticText.fontStyle = FontStyle.Bold;
            semanticText.text = "触发 → 命中";
            Stretch(semanticText.rectTransform);
            semanticGroup.alpha = 0f;
            semanticGroup.blocksRaycasts = false;
            semanticGroup.interactable = false;
            presentationAdapter.BindAuthoredScene(
                coarsePresentationLayer,
                targetOutline,
                enemyHitTarget,
                semanticGroup,
                semanticText);

            RectTransform panelRoot = CreateRectTransform(
                "CoreLoopLabPanelRoot",
                canvasRect);
            Stretch(panelRoot);
            panelRoot.SetAsLastSibling();

            GameObject rewardPanel = CreatePanel(
                "CoreLoopLabRewardPanel",
                panelRoot,
                new Vector2(860f, 470f),
                new Color(0.035f, 0.045f, 0.065f, 0.97f));
            Text rewardTitle = CreateText(
                "RewardTitle",
                rewardPanel.transform as RectTransform,
                font,
                34,
                TextAnchor.MiddleCenter,
                new Color(1f, 0.87f, 0.48f, 1f));
            rewardTitle.text = "战斗结束 · 选择一件固定奖励";
            SetAnchoredRect(
                rewardTitle.rectTransform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(760f, 58f),
                new Vector2(0f, -28f));
            Text rewardSubtitle = CreateText(
                "RewardSubtitle",
                rewardPanel.transform as RectTransform,
                font,
                20,
                TextAnchor.MiddleCenter,
                new Color(0.78f, 0.82f, 0.88f, 1f));
            rewardSubtitle.text =
                "奖励只进入本次 Lab 内存会话；选择后返回同一整备界面。";
            SetAnchoredRect(
                rewardSubtitle.rectTransform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(760f, 44f),
                new Vector2(0f, -86f));

            BuildItemPreviewCardView[] sourceCards =
            {
                RequireUniqueComponent<BuildItemPreviewCardView>(
                    scene,
                    "ItemCard_01"),
                RequireUniqueComponent<BuildItemPreviewCardView>(
                    scene,
                    "ItemCard_02"),
                RequireUniqueComponent<BuildItemPreviewCardView>(
                    scene,
                    "ItemCard_03")
            };
            List<BuildItemPreviewCardView> rewardCards = new();
            List<Button> rewardButtons = new();
            float[] cardX = { -250f, 0f, 250f };
            for (int index = 0; index < sourceCards.Length; index++)
            {
                GameObject clone = Object.Instantiate(
                    sourceCards[index].gameObject,
                    rewardPanel.transform);
                clone.name = "CoreLoopLabRewardCard_" + (index + 1);
                clone.hideFlags = HideFlags.None;
                RectTransform cloneRect = clone.transform as RectTransform;
                SetAnchoredRect(
                    cloneRect,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(190f, 245f),
                    new Vector2(cardX[index], -45f));
                cloneRect.localScale = Vector3.one;

                BuildItemPreviewCardView card =
                    clone.GetComponent<BuildItemPreviewCardView>();
                card.enabled = false;
                Button copiedButton = clone.GetComponent<Button>();
                if (copiedButton != null)
                {
                    Object.DestroyImmediate(copiedButton, true);
                }

                Image targetGraphic = clone.GetComponent<Image>();
                if (targetGraphic == null)
                {
                    targetGraphic = clone.AddComponent<Image>();
                    targetGraphic.color = new Color(1f, 1f, 1f, 0.001f);
                }

                Button choiceButton = clone.AddComponent<Button>();
                choiceButton.targetGraphic = targetGraphic;
                choiceButton.navigation = new UnityEngine.UI.Navigation
                {
                    mode = UnityEngine.UI.Navigation.Mode.None
                };
                rewardCards.Add(card);
                rewardButtons.Add(choiceButton);
            }

            GameObject resultPanel = CreatePanel(
                "CoreLoopLabResultPanel",
                panelRoot,
                new Vector2(700f, 330f),
                new Color(0.035f, 0.045f, 0.065f, 0.97f));
            Text resultTitle = CreateText(
                "ResultTitle",
                resultPanel.transform as RectTransform,
                font,
                34,
                TextAnchor.MiddleCenter,
                new Color(1f, 0.87f, 0.48f, 1f));
            SetAnchoredRect(
                resultTitle.rectTransform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(620f, 64f),
                new Vector2(0f, -34f));
            Text resultBody = CreateText(
                "ResultBody",
                resultPanel.transform as RectTransform,
                font,
                22,
                TextAnchor.MiddleCenter,
                new Color(0.86f, 0.88f, 0.92f, 1f));
            resultBody.horizontalOverflow = HorizontalWrapMode.Wrap;
            resultBody.verticalOverflow = VerticalWrapMode.Truncate;
            SetAnchoredRect(
                resultBody.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(590f, 120f),
                new Vector2(0f, 4f));
            Button retryButton = CreateButton(
                "RetryButton",
                resultPanel.transform as RectTransform,
                font,
                "重试本方案",
                new Vector2(250f, 62f),
                new Vector2(0f, -112f));

            rewardPanel.SetActive(false);
            resultPanel.SetActive(false);
            labController.BindAuthoredScene(
                gridController,
                runtimeLoop,
                prepareToggle,
                prepareContinue,
                rewardPanel,
                rewardCards,
                rewardButtons,
                resultPanel,
                resultTitle,
                resultBody,
                retryButton,
                presentationAdapter);
            labController.SetProfileForEditor(
                CoreLoopLabProfile.A_CurrentSingleBattle);

            EditorUtility.SetDirty(labController);
            EditorUtility.SetDirty(presentationAdapter);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, TargetScenePath))
            {
                throw new InvalidOperationException(
                    "Failed to save the authored CoreLoopLab Scene.");
            }

            AssetDatabase.ImportAsset(
                TargetScenePath,
                ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.SaveAssets();

            byte[] sourceBytesAfter = File.ReadAllBytes(SourceScenePath);
            if (!BytesEqual(sourceBytesBefore, sourceBytesAfter))
            {
                throw new InvalidOperationException(
                    "Protected BattleSandbox source Scene changed during Save-As.");
            }

            Debug.Log("[CoreLoopLabAuthoring] SUCCESS target="
                + TargetScenePath + " sourceUnchanged=true");
        }

        private static void LaunchProfile(CoreLoopLabProfile profile)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning(
                    "[CoreLoopLab] Stop Play Mode before selecting a launch profile.");
                return;
            }

            if (!File.Exists(TargetScenePath))
            {
                throw new FileNotFoundException(
                    "Author the CoreLoopLab Scene before launching a profile.",
                    TargetScenePath);
            }

            Scene scene = EditorSceneManager.OpenScene(
                TargetScenePath,
                OpenSceneMode.Single);
            CoreLoopLabController controller =
                scene.GetRootGameObjects()
                    .SelectMany(root => root
                        .GetComponentsInChildren<CoreLoopLabController>(true))
                    .Single();
            Undo.RecordObject(controller, "Select CoreLoopLab Profile");
            controller.SetProfileForEditor(profile);
            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, TargetScenePath);
            Selection.activeObject = controller;
            EditorApplication.delayCall += () =>
                EditorApplication.isPlaying = true;
        }

        private static T RequireUniqueComponent<T>(
            Scene scene,
            string objectName)
            where T : Component
        {
            T[] matches = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .Where(component => component != null
                    && string.Equals(
                        component.gameObject.name,
                        objectName,
                        StringComparison.Ordinal))
                .ToArray();
            if (matches.Length != 1)
            {
                throw new InvalidOperationException(
                    $"Expected exactly one {typeof(T).Name} on '{objectName}', found {matches.Length}.");
            }

            return matches[0];
        }

        private static T GetOrAdd<T>(GameObject target)
            where T : Component
        {
            return target.GetComponent<T>() ?? target.AddComponent<T>();
        }

        private static RectTransform CreateRectTransform(
            string name,
            Transform parent)
        {
            GameObject target = new(name, typeof(RectTransform));
            RectTransform rect = target.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }

        private static CanvasGroup CreateCanvasGroupRoot(
            string name,
            Transform parent)
        {
            GameObject target = new(
                name,
                typeof(RectTransform),
                typeof(CanvasGroup));
            target.transform.SetParent(parent, false);
            CanvasGroup group = target.GetComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;
            return group;
        }

        private static GameObject CreatePanel(
            string name,
            Transform parent,
            Vector2 size,
            Color color)
        {
            GameObject target = new(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(CanvasGroup));
            RectTransform rect = target.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            SetAnchoredRect(
                rect,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                size,
                Vector2.zero);
            Image image = target.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = true;
            return target;
        }

        private static Text CreateText(
            string name,
            Transform parent,
            Font font,
            int fontSize,
            TextAnchor alignment,
            Color color)
        {
            GameObject target = new(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text));
            target.transform.SetParent(parent, false);
            Text text = target.GetComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateButton(
            string name,
            Transform parent,
            Font font,
            string label,
            Vector2 size,
            Vector2 anchoredPosition)
        {
            GameObject target = new(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button));
            RectTransform rect = target.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            SetAnchoredRect(
                rect,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                size,
                anchoredPosition);
            Image image = target.GetComponent<Image>();
            image.color = new Color(0.78f, 0.46f, 0.16f, 1f);
            Button button = target.GetComponent<Button>();
            button.targetGraphic = image;
            button.navigation = new UnityEngine.UI.Navigation
            {
                mode = UnityEngine.UI.Navigation.Mode.None
            };
            Text text = CreateText(
                "Label",
                rect,
                font,
                24,
                TextAnchor.MiddleCenter,
                Color.white);
            text.fontStyle = FontStyle.Bold;
            text.text = label;
            Stretch(text.rectTransform);
            return button;
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
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        private static void SetAnchoredRect(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 size,
            Vector2 anchoredPosition)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        private static bool BytesEqual(byte[] left, byte[] right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            for (int i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
