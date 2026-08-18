using System;
using System.Collections.Generic;
using TalismanBag.Contracts.Battle;
using TalismanBag.Navigation;
using TalismanBag.V04.Campaign.Chapter1;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.V04.WorldMap
{
    public sealed class V04WorldMapSceneController : MonoBehaviour
    {
        public const string SceneName = TalismanSceneNavigationOwner.WorldMapSceneName;
        public const string ScenePath =
            TalismanSceneNavigationOwner.WorldMapScenePath;

        [Header("Authored Page Roots")]
        [SerializeField] private GameObject worldRegionView;
        [SerializeField] private GameObject qingshifangRegionView;
        [SerializeField] private GameObject chapterStageMapView;
        [SerializeField] private V04WorldMapStageDetailDrawerView stageDetailDrawer;

        [Header("Common Authored Shell")]
        [SerializeField] private Button backButton;
        [SerializeField] private Button upgradeNavigationButton;
        [SerializeField] private Button qingshifangRegionButton;
        [SerializeField] private Text breadcrumbText;
        [SerializeField] private Text resourceShellText;
        [SerializeField] private Text pageDiagnosticText;

        [Header("Authored Entries")]
        [SerializeField] private V04WorldMapChapterEntryView[] chapterEntryViews;
        [SerializeField] private V04WorldMapStageNodeView[] chapterOneStageNodeViews;

        private V04WorldMapProgressSnapshot progress;
        private V04WorldMapPage currentPage;
        private string currentChapterId = string.Empty;
        private string selectedStageId = string.Empty;
        private IReadOnlyDictionary<string, Chapter1CampaignStageConfig> stageConfigs;
        private string stageConfigDiagnostic = "STAGE_CONFIG_CATALOG_NOT_LOADED";
        private V04CampaignStageCompletionRepository completionRepository;
        private bool runtimeBound;

        public V04WorldMapPage CurrentPage => currentPage;

        public string CurrentChapterId => currentChapterId;
        public string SelectedStageId => selectedStageId;

        private void Awake()
        {
            completionRepository = new V04CampaignStageCompletionRepository();
            progress = V04WorldMapProgressSnapshot.CreateFromCompletion(
                completionRepository.Load());
            if (!ValidateAuthoredBindings(out string diagnostic))
            {
                Debug.LogError(
                    "[V0.4-WorldMapRuntimeLock] " + diagnostic +
                    " Runtime final-UI creation is disabled.",
                    this);
                enabled = false;
                return;
            }

            if (!Chapter1CampaignStageCatalog.TryLoad(
                    out stageConfigs,
                    out stageConfigDiagnostic))
            {
                Debug.LogError(
                    "[V0.4-CampaignFormalLaunch] " + stageConfigDiagnostic,
                    this);
            }

            BindRuntime();
            ShowWorldRegionView();
        }

        private void OnDestroy()
        {
            UnbindRuntime();
        }

        public void ApplyProgressSnapshot(V04WorldMapProgressSnapshot snapshot)
        {
            progress = snapshot ?? V04WorldMapProgressSnapshot.CreateUnboundPreview();
            if (currentPage == V04WorldMapPage.ChapterStageMapView)
            {
                BindChapterOneStageNodes();
            }
        }

        public void ShowWorldRegionView()
        {
            stageDetailDrawer.Hide();
            selectedStageId = string.Empty;
            SetPageRoots(showWorld: true, showRegion: false, showChapter: false);
            currentPage = V04WorldMapPage.WorldRegionView;
            currentChapterId = string.Empty;
            SetText(breadcrumbText, "世界舆图");
            SetText(
                pageDiagnosticText,
                "首版本区域已登记：青石坊《骨相》\n点击区域进入章节视图");
        }

        public void OpenQingshifangRegion()
        {
            stageDetailDrawer.Hide();
            selectedStageId = string.Empty;
            SetPageRoots(showWorld: false, showRegion: true, showChapter: false);
            currentPage = V04WorldMapPage.QingshifangRegionView;
            currentChapterId = string.Empty;
            SetText(breadcrumbText, "世界舆图 / 青石坊《骨相》");
            SetText(
                pageDiagnosticText,
                "四章入口使用稳定 chapterId；当前纵切开放第一章地图");
        }

        public void OpenChapter(string chapterId)
        {
            V04WorldMapChapterDefinition chapter = V04WorldMapCatalog.FindChapter(chapterId);
            if (chapter == null)
            {
                SetText(pageDiagnosticText, "WORLD_MAP_UNKNOWN_CHAPTER " + chapterId);
                return;
            }

            if (!chapter.availableInVerticalSlice)
            {
                SetText(
                    pageDiagnosticText,
                    "WORLD_MAP_CHAPTER_NOT_IN_VERTICAL_SLICE " + chapter.chapterId);
                return;
            }

            stageDetailDrawer.Hide();
            selectedStageId = string.Empty;
            currentChapterId = chapter.chapterId;
            currentPage = V04WorldMapPage.ChapterStageMapView;
            SetPageRoots(showWorld: false, showRegion: false, showChapter: true);
            SetText(
                breadcrumbText,
                "世界舆图 / 青石坊《骨相》 / " + chapter.displayName);
            SetText(
                pageDiagnosticText,
                "进度来源：" + progress.authority +
                "\n本包不写 Save、不发奖励、不滚掉落");
            BindChapterOneStageNodes();
        }

        public void SelectStage(string stageId)
        {
            V04WorldMapStageDefinition stage =
                V04WorldMapCatalog.FindChapterOneStage(stageId);
            if (stage == null)
            {
                SetText(pageDiagnosticText, "WORLD_MAP_UNKNOWN_STAGE " + stageId);
                return;
            }

            V04WorldMapStageVisualState visualState =
                V04WorldMapStageStateResolver.Resolve(stage, progress);
            selectedStageId = stage.stageId;
            stageDetailDrawer.Show(
                stage,
                V04WorldMapCatalog.FindVisualState(visualState),
                progress.authority);

            string configDiagnostic = "STAGE_CONFIG_NOT_FOUND";
            Chapter1CampaignStageConfig config = null;
            bool hasConfig = stageConfigs != null
                && stageConfigs.TryGetValue(stage.stageId, out config)
                && config != null
                && config.TryValidate(out configDiagnostic);
            bool routeReady = hasConfig
                && config.RouteEnabled
                && config.BattleContentReady
                && (visualState == V04WorldMapStageVisualState.Available
                    || visualState == V04WorldMapStageVisualState.Cleared);
            string routeDiagnostic = routeReady
                ? "CAMPAIGN_NORMAL_LV1_ROUTE_READY stageId=" + stage.stageId
                : hasConfig
                    ? "FORMAL_ROUTE_DISABLED stageId=" + stage.stageId
                    : stageConfigDiagnostic + " " + configDiagnostic;
            stageDetailDrawer.BindChallenge(
                stage.stageId,
                routeReady,
                HandleChallenge,
                routeDiagnostic);
        }

        public bool ValidateAuthoredBindings(out string diagnostic)
        {
            List<string> issues = new();
            if (worldRegionView == null)
            {
                issues.Add("WorldRegionView missing");
            }

            if (qingshifangRegionView == null)
            {
                issues.Add("QingshifangRegionView missing");
            }

            if (chapterStageMapView == null)
            {
                issues.Add("ChapterStageMapView missing");
            }

            if (stageDetailDrawer == null)
            {
                issues.Add("StageDetailDrawer missing");
            }
            else if (!stageDetailDrawer.HasAuthoredBindings)
            {
                issues.Add("StageDetailDrawer child bindings invalid");
            }

            if (backButton == null ||
                upgradeNavigationButton == null ||
                qingshifangRegionButton == null)
            {
                issues.Add("Common navigation buttons missing");
            }

            if (chapterEntryViews == null || chapterEntryViews.Length != 4)
            {
                issues.Add("Chapter entry count must equal 4");
            }
            else if (Array.Exists(
                         chapterEntryViews,
                         view => view == null || !view.HasAuthoredBindings))
            {
                issues.Add("Chapter entry bindings invalid");
            }

            if (chapterOneStageNodeViews == null || chapterOneStageNodeViews.Length != 10)
            {
                issues.Add("Chapter 1 stage node count must equal 10");
            }
            else if (Array.Exists(
                         chapterOneStageNodeViews,
                         view => view == null || !view.HasAuthoredBindings))
            {
                issues.Add("Chapter 1 stage node bindings invalid");
            }

            diagnostic = string.Join("; ", issues);
            return issues.Count == 0;
        }

        public void ConfigureSceneBindings(
            GameObject configuredWorldRegionView,
            GameObject configuredQingshifangRegionView,
            GameObject configuredChapterStageMapView,
            V04WorldMapStageDetailDrawerView configuredStageDetailDrawer,
            Button configuredBackButton,
            Button configuredUpgradeNavigationButton,
            Button configuredQingshifangRegionButton,
            Text configuredBreadcrumbText,
            Text configuredResourceShellText,
            Text configuredPageDiagnosticText,
            V04WorldMapChapterEntryView[] configuredChapterEntryViews,
            V04WorldMapStageNodeView[] configuredChapterOneStageNodeViews)
        {
            worldRegionView = configuredWorldRegionView;
            qingshifangRegionView = configuredQingshifangRegionView;
            chapterStageMapView = configuredChapterStageMapView;
            stageDetailDrawer = configuredStageDetailDrawer;
            backButton = configuredBackButton;
            upgradeNavigationButton = configuredUpgradeNavigationButton;
            qingshifangRegionButton = configuredQingshifangRegionButton;
            breadcrumbText = configuredBreadcrumbText;
            resourceShellText = configuredResourceShellText;
            pageDiagnosticText = configuredPageDiagnosticText;
            chapterEntryViews = configuredChapterEntryViews;
            chapterOneStageNodeViews = configuredChapterOneStageNodeViews;
        }

        public void ConfigureUpgradeNavigationButton(Button configuredButton)
        {
            upgradeNavigationButton = configuredButton;
        }

        private void BindRuntime()
        {
            if (runtimeBound)
            {
                return;
            }

            backButton.onClick.AddListener(HandleBack);
            upgradeNavigationButton.onClick.AddListener(HandleUpgradeNavigation);
            qingshifangRegionButton.onClick.AddListener(OpenQingshifangRegion);
            stageDetailDrawer.BindClose(CloseStageDetail);
            for (int index = 0; index < chapterEntryViews.Length; index++)
            {
                chapterEntryViews[index].Bind(
                    V04WorldMapCatalog.Chapters[index],
                    OpenChapter);
            }

            SetText(
                resourceShellText,
                "资源栏：RESOURCE_OWNER_UNBOUND（仅保留场景插槽）");
            runtimeBound = true;
        }

        private void UnbindRuntime()
        {
            if (!runtimeBound)
            {
                return;
            }

            backButton?.onClick.RemoveListener(HandleBack);
            upgradeNavigationButton?.onClick.RemoveListener(HandleUpgradeNavigation);
            qingshifangRegionButton?.onClick.RemoveListener(OpenQingshifangRegion);
            stageDetailDrawer?.UnbindClose();
            stageDetailDrawer?.UnbindChallenge();
            foreach (V04WorldMapChapterEntryView view in chapterEntryViews
                         ?? Array.Empty<V04WorldMapChapterEntryView>())
            {
                view?.UnbindRuntime();
            }

            foreach (V04WorldMapStageNodeView view in chapterOneStageNodeViews
                         ?? Array.Empty<V04WorldMapStageNodeView>())
            {
                view?.UnbindRuntime();
            }

            runtimeBound = false;
        }

        private void BindChapterOneStageNodes()
        {
            for (int index = 0; index < chapterOneStageNodeViews.Length; index++)
            {
                V04WorldMapStageDefinition stage =
                    V04WorldMapCatalog.ChapterOneStages[index];
                V04WorldMapStageVisualState state =
                    V04WorldMapStageStateResolver.Resolve(stage, progress);
                chapterOneStageNodeViews[index].Bind(
                    stage,
                    V04WorldMapCatalog.FindVisualState(state),
                    SelectStage);
            }
        }

        private void HandleBack()
        {
            if (stageDetailDrawer.gameObject.activeSelf)
            {
                CloseStageDetail();
                return;
            }

            switch (currentPage)
            {
                case V04WorldMapPage.ChapterStageMapView:
                    OpenQingshifangRegion();
                    break;
                case V04WorldMapPage.QingshifangRegionView:
                    ShowWorldRegionView();
                    break;
                default:
                    TalismanSceneNavigationOwner.TryNavigate(
                        TalismanSceneRoute.MainHome,
                        this);
                    break;
            }
        }

        private void HandleUpgradeNavigation()
        {
            TalismanSceneNavigationOwner.TryNavigateToUpgrade(
                TalismanSceneRoute.WorldMap,
                this);
        }

        private void CloseStageDetail()
        {
            stageDetailDrawer.Hide();
            selectedStageId = string.Empty;
        }

        private void HandleChallenge(string stageId)
        {
            string configDiagnostic = "STAGE_CONFIG_NOT_FOUND";
            Chapter1CampaignStageConfig config = null;
            if (!string.Equals(stageId, selectedStageId, StringComparison.Ordinal)
                || stageConfigs == null
                || !stageConfigs.TryGetValue(
                    stageId,
                    out config)
                || config == null
                || !config.RouteEnabled
                || !config.BattleContentReady
                || !config.TryValidate(out configDiagnostic))
            {
                stageDetailDrawer.SetChallengeDiagnostic(
                    "FORMAL_ROUTE_REJECTED " + configDiagnostic);
                return;
            }

            V04WorldMapStageDefinition stage =
                V04WorldMapCatalog.FindChapterOneStage(stageId);
            V04WorldMapStageVisualState visualState =
                V04WorldMapStageStateResolver.Resolve(stage, progress);
            if (stage == null
                || (visualState != V04WorldMapStageVisualState.Available
                    && visualState != V04WorldMapStageVisualState.Cleared))
            {
                stageDetailDrawer.SetChallengeDiagnostic(
                    "FORMAL_ROUTE_PROGRESS_REJECTED stageId=" + stageId);
                return;
            }

            string launchId = Guid.NewGuid().ToString("N");
            string token = Guid.NewGuid().ToString("N");
            BattleLaunchContext context = config.CreateLaunchContext(
                launchId,
                token,
                FormalBattleLaunchTransit.NextGeneration);
            if (!FormalBattleLaunchTransit.TryPublish(
                    config,
                    context,
                    out BattleLaunchContext published,
                    out string publishDiagnostic))
            {
                stageDetailDrawer.SetChallengeDiagnostic(publishDiagnostic);
                return;
            }

            if (TalismanSceneNavigationOwner.TryNavigate(
                    TalismanSceneRoute.UnifiedBattle,
                    this))
            {
                stageDetailDrawer.SetChallengeDiagnostic(
                    "FORMAL_ROUTE_NAVIGATION_ACCEPTED generation=" +
                    published.Generation);
                return;
            }

            FormalBattleLaunchTransit.TryRevoke(
                published.LaunchId,
                published.Token,
                published.Generation,
                out string revokeDiagnostic);
            stageDetailDrawer.SetChallengeDiagnostic(
                "FORMAL_ROUTE_NAVIGATION_REJECTED " + revokeDiagnostic);
        }

        private void SetPageRoots(bool showWorld, bool showRegion, bool showChapter)
        {
            SetActive(worldRegionView, showWorld);
            SetActive(qingshifangRegionView, showRegion);
            SetActive(chapterStageMapView, showChapter);
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null && target.activeSelf != active)
            {
                target.SetActive(active);
            }
        }

        private static void SetText(Text target, string value)
        {
            if (target != null)
            {
                target.text = value ?? string.Empty;
            }
        }
    }
}
