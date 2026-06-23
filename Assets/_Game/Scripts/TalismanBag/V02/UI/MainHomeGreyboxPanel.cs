using System;
using System.Collections.Generic;
using TalismanBag.V02.CoreLoop.Save;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.V02.UI
{
    public sealed class MainHomeGreyboxPanel : MonoBehaviour
    {
        public const string HomeTitle = "照灯小铺";

        [SerializeField] private GameObject panel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text resourceText;
        [SerializeField] private Text progressText;
        [SerializeField] private Text objectiveText;
        [SerializeField] private Text statusText;
        [SerializeField] private Text feedbackText;
        [SerializeField] private RectTransform hotspotRoot;
        [SerializeField] private RectTransform shortcutRoot;
        [SerializeField] private Button cultivateButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button closeButton;

        private readonly List<HomeHotspotView> hotspotViews = new();
        private readonly List<HomeHotspotView> shortcutViews = new();
        private Action cultivateCallback;
        private Action continueCallback;
        private Action closeCallback;
        private bool buttonsBound;
        private bool mainActionsEnabled;
        private string currentObjective = string.Empty;

        private void Awake()
        {
            BindButtons();
            Hide();
        }

        private void OnDestroy()
        {
            if (!buttonsBound)
            {
                return;
            }

            cultivateButton?.onClick.RemoveListener(InvokeCultivate);
            continueButton?.onClick.RemoveListener(InvokeContinue);
            closeButton?.onClick.RemoveListener(InvokeClose);
        }

        public void Show(
            string title,
            string resources,
            string status,
            Action onCultivate,
            Action onContinue,
            Action onClose)
        {
            ShowInternal(resources, status, onCultivate, onContinue, onClose, true);
        }

        public void ShowComplete(string title, string resources, string status, Action onClose)
        {
            ShowInternal(resources, status, null, null, onClose, false);
        }

        public void Hide()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        public static MainHomeGreyboxPanel CreateRuntime(Transform parent)
        {
            if (parent == null)
            {
                return null;
            }

            GameObject root = new("MainHomeGreyboxPanel_Runtime", typeof(RectTransform), typeof(Image), typeof(Outline));
            root.transform.SetParent(parent, false);
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = Vector2.zero;
            rootRect.sizeDelta = new Vector2(1000f, 1720f);

            Image image = root.GetComponent<Image>();
            image.color = new Color(0.045f, 0.052f, 0.05f, 0.985f);

            Outline outline = root.GetComponent<Outline>();
            outline.effectColor = new Color(0.66f, 0.7f, 0.57f, 0.94f);
            outline.effectDistance = new Vector2(4f, -4f);

            MainHomeGreyboxPanel panelComponent = root.AddComponent<MainHomeGreyboxPanel>();
            panelComponent.panel = root;

            panelComponent.titleText = CreateText(
                "HomeTitle",
                root.transform,
                HomeTitle,
                46,
                FontStyle.Bold,
                new Color(0.92f, 0.96f, 0.82f),
                new Vector2(0f, -34f),
                new Vector2(850f, 62f),
                TextAnchor.MiddleCenter);

            CreateText(
                "HomeSubtitle",
                root.transform,
                "青石坡老街 · 今夜照常亮灯",
                20,
                FontStyle.Normal,
                new Color(0.68f, 0.72f, 0.67f),
                new Vector2(0f, -92f),
                new Vector2(850f, 34f),
                TextAnchor.MiddleCenter);

            RectTransform resourcesPanel = CreatePanel(
                "HomeResourceSummary",
                root.transform,
                new Vector2(-228f, -140f),
                new Vector2(430f, 166f),
                new Color(0.09f, 0.12f, 0.105f, 0.96f));
            CreateText(
                "ResourceHeader",
                resourcesPanel,
                "资源摘要",
                20,
                FontStyle.Bold,
                new Color(0.82f, 0.88f, 0.72f),
                new Vector2(0f, -10f),
                new Vector2(390f, 32f),
                TextAnchor.UpperLeft);
            panelComponent.resourceText = CreateText(
                "HomeResources",
                resourcesPanel,
                string.Empty,
                18,
                FontStyle.Normal,
                Color.white,
                new Vector2(0f, -45f),
                new Vector2(390f, 108f),
                TextAnchor.UpperLeft);

            RectTransform progressPanel = CreatePanel(
                "HomeMainTrialProgress",
                root.transform,
                new Vector2(228f, -140f),
                new Vector2(430f, 166f),
                new Color(0.095f, 0.105f, 0.13f, 0.96f));
            CreateText(
                "ProgressHeader",
                progressPanel,
                "主线进度",
                20,
                FontStyle.Bold,
                new Color(0.76f, 0.83f, 0.94f),
                new Vector2(0f, -10f),
                new Vector2(390f, 32f),
                TextAnchor.UpperLeft);
            panelComponent.progressText = CreateText(
                "HomeProgress",
                progressPanel,
                string.Empty,
                19,
                FontStyle.Normal,
                Color.white,
                new Vector2(0f, -48f),
                new Vector2(390f, 104f),
                TextAnchor.UpperLeft);

            RectTransform objectivePanel = CreatePanel(
                "HomeCurrentObjective",
                root.transform,
                new Vector2(0f, -326f),
                new Vector2(890f, 160f),
                new Color(0.14f, 0.115f, 0.075f, 0.96f));
            CreateText(
                "ObjectiveHeader",
                objectivePanel,
                "当前目标",
                21,
                FontStyle.Bold,
                new Color(0.95f, 0.81f, 0.48f),
                new Vector2(0f, -10f),
                new Vector2(840f, 32f),
                TextAnchor.UpperLeft);
            panelComponent.objectiveText = CreateText(
                "ObjectiveText",
                objectivePanel,
                string.Empty,
                22,
                FontStyle.Normal,
                new Color(0.96f, 0.94f, 0.84f),
                new Vector2(0f, -49f),
                new Vector2(840f, 58f),
                TextAnchor.UpperLeft);
            panelComponent.statusText = CreateText(
                "HomeStatus",
                objectivePanel,
                string.Empty,
                16,
                FontStyle.Normal,
                new Color(0.72f, 0.72f, 0.67f),
                new Vector2(0f, -108f),
                new Vector2(840f, 38f),
                TextAnchor.UpperLeft);

            CreateText(
                "HotspotHeader",
                root.transform,
                "店内热点",
                24,
                FontStyle.Bold,
                new Color(0.85f, 0.9f, 0.76f),
                new Vector2(0f, -505f),
                new Vector2(890f, 38f),
                TextAnchor.MiddleLeft);

            panelComponent.hotspotRoot = CreatePanel(
                "HomeHotspotArea",
                root.transform,
                new Vector2(0f, -550f),
                new Vector2(890f, 700f),
                new Color(0.075f, 0.083f, 0.078f, 0.96f));

            CreateText(
                "FloorHint",
                panelComponent.hotspotRoot,
                "旧木柜台、炼符桌与后屋门都还在原处",
                16,
                FontStyle.Italic,
                new Color(0.54f, 0.58f, 0.54f),
                new Vector2(0f, -10f),
                new Vector2(820f, 26f),
                TextAnchor.MiddleCenter);

            CreateText(
                "ShortcutHeader",
                root.transform,
                "右侧快捷入口",
                20,
                FontStyle.Bold,
                new Color(0.75f, 0.8f, 0.76f),
                new Vector2(0f, -1265f),
                new Vector2(890f, 32f),
                TextAnchor.MiddleLeft);

            panelComponent.shortcutRoot = CreatePanel(
                "HomeSystemShortcuts",
                root.transform,
                new Vector2(0f, -1305f),
                new Vector2(890f, 104f),
                new Color(0.065f, 0.07f, 0.072f, 0.96f));

            panelComponent.feedbackText = CreateText(
                "HomeFeedback",
                root.transform,
                "选择店内区域查看。",
                19,
                FontStyle.Normal,
                new Color(0.9f, 0.87f, 0.72f),
                new Vector2(0f, -1428f),
                new Vector2(870f, 76f),
                TextAnchor.MiddleCenter);

            panelComponent.closeButton = CreateButton(
                "HomeCloseButton",
                root.transform,
                "暂时收起",
                new Vector2(0f, 58f),
                new Vector2(300f, 72f),
                new Color(0.28f, 0.32f, 0.31f));

            panelComponent.CreateHotspotViews();
            panelComponent.BindButtons();
            panelComponent.Hide();
            return panelComponent;
        }

        private void ShowInternal(
            string resources,
            string status,
            Action onCultivate,
            Action onContinue,
            Action onClose,
            bool enableMainActions)
        {
            BindButtons();
            cultivateCallback = onCultivate;
            continueCallback = onContinue;
            closeCallback = onClose;
            mainActionsEnabled = enableMainActions;
            currentObjective = BuildCurrentObjective();

            if (panel != null)
            {
                panel.SetActive(true);
                panel.transform.SetAsLastSibling();
            }

            SetText(titleText, HomeTitle);
            SetText(resourceText, FormatResourceSummary(resources));
            SetText(progressText, BuildProgressSummary());
            SetText(objectiveText, currentObjective);
            SetText(statusText, string.IsNullOrWhiteSpace(status) ? "小铺今夜平静。" : status.Trim());
            SetFeedback("选择店内区域查看。");
            RefreshHotspots();
            SetMainActionButtonsVisible(false);
        }

        private void CreateHotspotViews()
        {
            if (hotspotRoot == null || hotspotViews.Count > 0)
            {
                return;
            }

            CreateHotspot(HomeHotspotId.Counter, new Vector2(0f, -54f), new Vector2(230f, 126f));
            CreateHotspot(HomeHotspotId.Codex, new Vector2(-290f, -135f), new Vector2(205f, 120f));
            CreateHotspot(HomeHotspotId.Trial, new Vector2(290f, -148f), new Vector2(220f, 132f));
            CreateHotspot(HomeHotspotId.Refine, new Vector2(0f, -218f), new Vector2(230f, 138f));
            CreateHotspot(HomeHotspotId.BackRoom, new Vector2(-292f, -300f), new Vector2(205f, 116f));
            CreateHotspot(HomeHotspotId.DreamSign, new Vector2(292f, -322f), new Vector2(205f, 116f));
            CreateHotspot(HomeHotspotId.Explore, new Vector2(-175f, -446f), new Vector2(205f, 116f));
            CreateHotspot(HomeHotspotId.TianjiFurnace, new Vector2(175f, -455f), new Vector2(205f, 116f));
            CreateHotspot(HomeHotspotId.MasterRelic, new Vector2(-292f, -576f), new Vector2(205f, 104f));
            CreateHotspot(HomeHotspotId.PvpPlaceholder, new Vector2(292f, -576f), new Vector2(205f, 104f));

            CreateShortcut(HomeHotspotId.Activity, -344f);
            CreateShortcut(HomeHotspotId.Mail, -172f);
            CreateShortcut(HomeHotspotId.Store, 0f);
            CreateShortcut(HomeHotspotId.Notice, 172f);
            CreateShortcut(HomeHotspotId.Settings, 344f);
        }

        private void CreateHotspot(HomeHotspotId hotspotId, Vector2 position, Vector2 size)
        {
            HomeHotspotView view = HomeHotspotView.CreateRuntime(hotspotRoot, position, size);
            view.name = $"HomeHotspot_{hotspotId}";
            hotspotViews.Add(view);
        }

        private void CreateShortcut(HomeHotspotId hotspotId, float x)
        {
            if (shortcutRoot == null)
            {
                return;
            }

            HomeHotspotView view = HomeHotspotView.CreateRuntime(
                shortcutRoot,
                new Vector2(x, -8f),
                new Vector2(156f, 84f),
                true);
            view.name = $"HomeShortcut_{hotspotId}";
            shortcutViews.Add(view);
        }

        private void RefreshHotspots()
        {
            if (hotspotViews.Count == 0 && shortcutViews.Count == 0)
            {
                CollectExistingHotspotViews();
            }

            if (hotspotViews.Count == 0 && shortcutViews.Count == 0)
            {
                CreateHotspotViews();
            }

            Dictionary<HomeHotspotId, HomeHotspotConfig> configs = new();
            foreach (HomeHotspotConfig defaultConfig in HomeHotspotConfig.CreateDefaultSet())
            {
                if (defaultConfig != null && HomeHotspotConfig.IsAllowed(defaultConfig.hotspotId))
                {
                    configs[defaultConfig.hotspotId] = BuildRuntimeConfig(defaultConfig);
                }
            }

            BindViews(hotspotViews, configs);
            BindViews(shortcutViews, configs);
        }

        private void CollectExistingHotspotViews()
        {
            if (hotspotRoot != null)
            {
                foreach (HomeHotspotView view in hotspotRoot.GetComponentsInChildren<HomeHotspotView>(true))
                {
                    if (view != null && !hotspotViews.Contains(view))
                    {
                        hotspotViews.Add(view);
                    }
                }
            }

            if (shortcutRoot != null)
            {
                foreach (HomeHotspotView view in shortcutRoot.GetComponentsInChildren<HomeHotspotView>(true))
                {
                    if (view != null && !shortcutViews.Contains(view))
                    {
                        shortcutViews.Add(view);
                    }
                }
            }
        }

        private HomeHotspotConfig BuildRuntimeConfig(HomeHotspotConfig source)
        {
            HomeHotspotConfig config = source.Clone();
            if (config.hotspotId == HomeHotspotId.Trial &&
                (!mainActionsEnabled || continueCallback == null))
            {
                config.state = HomeHotspotState.Locked;
                config.lockedReason = "当前主线入口暂不可用。";
            }

            if (config.hotspotId == HomeHotspotId.Refine &&
                (!mainActionsEnabled || cultivateCallback == null))
            {
                config.state = HomeHotspotState.Locked;
                config.lockedReason = "当前炼符入口暂不可用。";
            }

            return config;
        }

        private void BindViews(
            IReadOnlyList<HomeHotspotView> views,
            IReadOnlyDictionary<HomeHotspotId, HomeHotspotConfig> configs)
        {
            foreach (HomeHotspotView view in views)
            {
                if (view == null)
                {
                    continue;
                }

                string prefix = view.name.StartsWith("HomeShortcut_", StringComparison.Ordinal)
                    ? "HomeShortcut_"
                    : "HomeHotspot_";
                string idText = view.name.Substring(prefix.Length);
                if (!Enum.TryParse(idText, out HomeHotspotId hotspotId) ||
                    !configs.TryGetValue(hotspotId, out HomeHotspotConfig config))
                {
                    view.gameObject.SetActive(false);
                    continue;
                }

                view.Bind(config, OnHotspotClicked);
            }
        }

        private void OnHotspotClicked(HomeHotspotConfig config)
        {
            if (config == null)
            {
                return;
            }

            if (config.state == HomeHotspotState.Locked)
            {
                SetFeedback(string.IsNullOrWhiteSpace(config.lockedReason)
                    ? $"{config.displayName}尚未解锁。"
                    : config.lockedReason);
                return;
            }

            if (config.state == HomeHotspotState.ComingSoon)
            {
                SetFeedback(string.IsNullOrWhiteSpace(config.comingSoonText)
                    ? $"{config.displayName}即将开放。"
                    : config.comingSoonText);
                return;
            }

            switch (config.targetType)
            {
                case HomeHotspotTargetType.CurrentObjective:
                    SetFeedback(currentObjective);
                    break;
                case HomeHotspotTargetType.MainTrial:
                    InvokeContinue();
                    break;
                case HomeHotspotTargetType.TalismanRefine:
                    InvokeCultivate();
                    break;
                case HomeHotspotTargetType.Collection:
                    SetFeedback("道藏仍在整理，当前先保留查看入口。");
                    break;
                case HomeHotspotTargetType.StoryAnchor:
                    SetFeedback("师父旧物上的禁制尚未松动。");
                    break;
                default:
                    SetFeedback($"{config.displayName}即将开放。");
                    break;
            }
        }

        private void InvokeCultivate()
        {
            if (!mainActionsEnabled || cultivateCallback == null)
            {
                SetFeedback("当前炼符入口暂不可用。");
                return;
            }

            Action callback = cultivateCallback;
            SetFeedback("正在前往炼符桌……");
            callback.Invoke();
        }

        private void InvokeContinue()
        {
            if (!mainActionsEnabled || continueCallback == null)
            {
                SetFeedback("当前主线入口暂不可用。");
                return;
            }

            Action callback = continueCallback;
            SetFeedback("正在前往试炼……");
            callback.Invoke();
        }

        private void InvokeClose()
        {
            Action callback = closeCallback;
            Hide();
            callback?.Invoke();
        }

        private void BindButtons()
        {
            if (buttonsBound)
            {
                return;
            }

            if (cultivateButton == null && continueButton == null && closeButton == null)
            {
                return;
            }

            cultivateButton?.onClick.AddListener(InvokeCultivate);
            continueButton?.onClick.AddListener(InvokeContinue);
            closeButton?.onClick.AddListener(InvokeClose);
            buttonsBound = true;
        }

        private void SetMainActionButtonsVisible(bool visible)
        {
            cultivateButton?.gameObject.SetActive(visible);
            continueButton?.gameObject.SetActive(visible);
        }

        private void SetFeedback(string message)
        {
            SetText(feedbackText, string.IsNullOrWhiteSpace(message) ? "选择店内区域查看。" : message.Trim());
        }

        private static string BuildCurrentObjective()
        {
            MainTrialProgressData progress = GetProgress();
            return progress.mainTrialPhase switch
            {
                MainTrialPhase.NotStarted =>
                    "师父留下的委托还没处理完，先去试炼看看。",
                MainTrialPhase.Chapter1InProgress =>
                    "青石坡今晚不太太平，继续处理试炼异象。",
                MainTrialPhase.Chapter1BossCleared or
                MainTrialPhase.Chapter1RewardClaimed or
                MainTrialPhase.FirstUpgradeRequired =>
                    "符箓已可培养，去炼符桌看看。",
                MainTrialPhase.FirstUpgradeDone =>
                    "阵势稳了些，可以继续前往下一段试炼。",
                MainTrialPhase.Chapter2InProgress =>
                    "继续巡行，清理青石坡周边异象。",
                MainTrialPhase.Chapter2BossReady =>
                    "前方煞气聚阵，整备后再攻打。",
                MainTrialPhase.Chapter2BossInProgress =>
                    "煞气尚未平息，继续处理当前试炼。",
                MainTrialPhase.Chapter2Cleared or
                MainTrialPhase.CoreLoopComplete =>
                    "今日试炼暂告一段落，回小店整理收获。",
                _ =>
                    "先在小店看看，再决定下一步。"
            };
        }

        private static string BuildProgressSummary()
        {
            MainTrialProgressData progress = GetProgress();
            string roundId = !string.IsNullOrWhiteSpace(progress.currentRoundId)
                ? progress.currentRoundId.Trim()
                : progress.currentMainTrialLevelId?.Trim();
            if (string.IsNullOrWhiteSpace(roundId))
            {
                roundId = progress.mainTrialPhase == MainTrialPhase.NotStarted ? "尚未开始" : "待确认";
            }

            return $"当前：{roundId}\n阶段：{GetPhaseLabel(progress.mainTrialPhase)}";
        }

        private static MainTrialProgressData GetProgress()
        {
            SaveData saveData = SaveService.GetOrCreate().EnsureLoaded();
            return saveData.mainTrialProgressData ?? new MainTrialProgressData();
        }

        private static string GetPhaseLabel(MainTrialPhase phase)
        {
            return phase switch
            {
                MainTrialPhase.NotStarted => "委托待处理",
                MainTrialPhase.Chapter1InProgress => "第一段试炼",
                MainTrialPhase.Chapter1BossCleared => "首领结算",
                MainTrialPhase.Chapter1RewardClaimed => "等待培养",
                MainTrialPhase.FirstUpgradeRequired => "需要炼符",
                MainTrialPhase.FirstUpgradeDone => "准备继续",
                MainTrialPhase.Chapter2InProgress => "青石坡巡行",
                MainTrialPhase.Chapter2BossReady => "首领前整备",
                MainTrialPhase.Chapter2BossInProgress => "首领战",
                MainTrialPhase.Chapter2Cleared => "首领结算",
                MainTrialPhase.CoreLoopComplete => "今日收束",
                _ => "状态待确认"
            };
        }

        private static string FormatResourceSummary(string resources)
        {
            if (string.IsNullOrWhiteSpace(resources))
            {
                return "灵石 0　符纸 0\n朱砂 0　初阶符胚 0\n修为 0";
            }

            string[] lines = resources
                .Replace("当前资源", string.Empty)
                .Replace("- ", string.Empty)
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return string.Join("　", lines);
        }

        private static RectTransform CreatePanel(
            string objectName,
            Transform parent,
            Vector2 position,
            Vector2 size,
            Color color)
        {
            GameObject panelObject = new(objectName, typeof(RectTransform), typeof(Image), typeof(Outline));
            panelObject.transform.SetParent(parent, false);
            RectTransform rect = panelObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            panelObject.GetComponent<Image>().color = color;
            Outline outline = panelObject.GetComponent<Outline>();
            outline.effectColor = new Color(0.35f, 0.39f, 0.35f, 0.72f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);
            return rect;
        }

        private static Text CreateText(
            string objectName,
            Transform parent,
            string value,
            int fontSize,
            FontStyle style,
            Color color,
            Vector2 position,
            Vector2 size,
            TextAnchor alignment)
        {
            GameObject textObject = new(objectName, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            text.text = value;
            return text;
        }

        private static Button CreateButton(
            string objectName,
            Transform parent,
            string label,
            Vector2 position,
            Vector2 size,
            Color color)
        {
            GameObject buttonObject = new(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            buttonObject.GetComponent<Image>().color = color;
            Text text = CreateText(
                "Text",
                buttonObject.transform,
                label,
                22,
                FontStyle.Bold,
                Color.white,
                Vector2.zero,
                size,
                TextAnchor.MiddleCenter);
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = Vector2.zero;
            return buttonObject.GetComponent<Button>();
        }

        private static void SetText(Text text, string value)
        {
            if (text != null)
            {
                text.text = value ?? string.Empty;
            }
        }
    }
}
