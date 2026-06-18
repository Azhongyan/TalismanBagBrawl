using System.Collections.Generic;
using TalismanBag.Combat;
using TalismanBag.Enemies;
using TalismanBag.UI;
using TalismanBag.V02.CoreLoop.Boss;
using TalismanBag.V02.CoreLoop.MainTrial;
using TalismanBag.V02.CoreLoop.Rewards;
using TalismanBag.V02.CoreLoop.Resources;
using TalismanBag.V02.CoreLoop.Save;
using TalismanBag.V02.CoreLoop.Tutorial;
using TalismanBag.V02.CoreLoop.Upgrades;
using TalismanBag.V02.EnemySkills;
using TalismanBag.V02.Feedback;
using TalismanBag.V02.Result;
using TalismanBag.V02.Rewards;
using TalismanBag.V02.Tags;
using TalismanBag.V02.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.V02.Run
{
    public sealed class V02RunFlowController : MonoBehaviour
    {
        [SerializeField] private V02RunConfig runConfig;
        [SerializeField] private AutoCombatController combatController;
        [SerializeField] private V02RewardController rewardController;
        [SerializeField] private V02RunModifierState runModifierState;
        [SerializeField] private V02FailureTracker failureTracker;
        [SerializeField] private V02FailureReasonResolver failureReasonResolver;
        [SerializeField] private V02RunStatsTracker runStatsTracker;
        [SerializeField] private V02RunResultPanel runResultPanel;
        [SerializeField] private V02BossRewardPanel bossRewardPanel;
        [SerializeField] private V02FixedRewardPanel fixedRewardPanel;
        [SerializeField] private MainHomeGreyboxPanel homeGreyboxPanel;
        [SerializeField] private BossInfoPanel bossInfoPanel;
        [SerializeField] private RewardService coreLoopRewardService;
        [SerializeField] private SaveService coreLoopSaveService;
        [SerializeField] private TutorialGuideConfig tutorialGuideConfig;
        [SerializeField] private TutorialGuideService tutorialGuideService;
        [SerializeField] private RewardConfig chapterOneBossRewardConfig;
        [SerializeField] private BossInfoConfig chapterTwoBossInfoConfig;
        [SerializeField] private RewardConfig chapterTwoBossRewardConfig;
        [SerializeField] private RewardDropTable chapterTwoNormalDropTable;
        [SerializeField] private MainTrialFlowService mainTrialFlowService;
        [SerializeField] private UpgradeService coreLoopUpgradeService;
        [SerializeField] private TalismanUpgradePanel talismanUpgradePanel;
        [SerializeField] private string chapterOneUpgradeItemId = "fire_talisman_basic";
        [SerializeField] private BattleLogUI battleLogUI;
        [SerializeField] private Text roundInfoText;
        [SerializeField] private Text prepHintText;
        [SerializeField] private Text currentLevelText;
        [SerializeField] private GameObject formationInfoPanel;
        [SerializeField] private Button formationInfoCloseButton;
        [SerializeField] private GameObject tagTooltipPanel;
        [SerializeField] private Button tagTooltipCloseButton;
        [SerializeField] private List<EnemyDefinition> testEnemies = new();

        private int currentRoundIndex;
        private EnemyDefinition nextEnemyOverride;
        private V02RunState state = V02RunState.None;
        private V02RunConfig activeRunConfig;
        private CanvasGroup formationInfoCanvasGroup;
        private CanvasGroup tagTooltipCanvasGroup;
        private bool preBattlePopupButtonsBound;
        private bool bossRewardClaimed;
        private bool suppressNextMainTrialAutoStart;
        private string suppressedAutoStartLog = string.Empty;
        private readonly PostBattlePrepareRequest postBattlePrepareRequest = new();
        private readonly HashSet<int> shownFormationInfoRoundIndexes = new();
        private readonly Dictionary<string, EnemyDefinition> mainTrialRuntimeEnemies = new();

        public int CurrentRoundNumber => CurrentRound != null ? CurrentRound.roundIndex : currentRoundIndex + 1;
        public V02RunState State => state;
        public V02RoundConfig CurrentRound => GetRound(currentRoundIndex);
        public bool CanRequestPostBattlePrepare => state == V02RunState.Combat &&
                                                   EnsureMainTrialFlowService().IsChapterTwoNormalRound(CurrentRound);

        private void OnEnable()
        {
            if (rewardController != null)
            {
                rewardController.RewardChosen += OnRewardSelected;
            }

            if (runResultPanel != null)
            {
                runResultPanel.RestartRequested += OnRunResultPrimaryRequested;
            }

            BindPreBattlePopupButtons();
        }

        private void Start()
        {
            StartNewRun();
        }

        private void OnDisable()
        {
            if (rewardController != null)
            {
                rewardController.RewardChosen -= OnRewardSelected;
            }

            if (runResultPanel != null)
            {
                runResultPanel.RestartRequested -= OnRunResultPrimaryRequested;
            }

            UnbindPreBattlePopupButtons();
        }

        public void StartNewRun()
        {
            state = V02RunState.Init;
            MainTrialStartupRoute startupRoute = EnsureMainTrialFlowService().GetStartupRoute();
            activeRunConfig = ResolveRunConfigForRoute(startupRoute);
            currentRoundIndex = 0;
            suppressNextMainTrialAutoStart = false;
            suppressedAutoStartLog = string.Empty;
            postBattlePrepareRequest.Clear();

            nextEnemyOverride = null;
            shownFormationInfoRoundIndexes.Clear();
            runModifierState?.ResetState();
            failureTracker?.ResetTracker();
            runStatsTracker?.ResetStats();
            combatController?.ResetRunStats();
            rewardController?.StartNewRewardRun();
            runResultPanel?.Hide();
            bossRewardPanel?.Hide();
            fixedRewardPanel?.Hide();
            homeGreyboxPanel?.Hide();
            bossInfoPanel?.Hide();
            mainTrialRuntimeEnemies.Clear();
            bossRewardClaimed = false;
            ExecuteStartupRoute(startupRoute);
        }

        public void ResetMainTrialFromBeginning()
        {
            EnsureCoreLoopSaveService().ResetSave();
            bossRewardClaimed = false;
            battleLogUI?.AddLog("已重置 CoreLoop 存档：1-1 开局教学、固定教学奖励与章节奖励将重新发放。");
            StartNewRun();
        }

        public void EnterPrep()
        {
            V02RoundConfig round = CurrentRound;
            if (round?.enemy == null)
            {
                battleLogUI?.AddLog("\u7f3a\u5c11 V0.2 \u5173\u5361\u6216\u654c\u4eba\u914d\u7f6e");
                return;
            }

            state = V02RunState.Prep;
            EnemyDefinition resolvedEnemy = ResolveMainTrialEnemy(round);
            combatController?.SetEnemy(resolvedEnemy, round.roundIndex, GetRoundCount());
            RefreshRoundTexts(round, resolvedEnemy);
            ShowPreBattlePopups();
            EnsureMainTrialFlowService().MarkEnteredRound(round);
            battleLogUI?.AddLog($"\u6218\u524d\u51c6\u5907\uff1a{round.roundTitle}");

            if (TryOpenPreBattleTutorialGuidePanel(round))
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(round.preBattleHint))
            {
                battleLogUI?.AddLog(round.preBattleHint);
            }

            if (TryOpenChapterTwoBossInfoPanel(round))
            {
                return;
            }

            TryAutoStartMainTrialRound(round);
        }

        public void StartCombat()
        {
            suppressNextMainTrialAutoStart = false;
            suppressedAutoStartLog = string.Empty;
            if (EnsureMainTrialFlowService().IsChapterTwoBossRound(CurrentRound))
            {
                EnsureMainTrialFlowService().OnChapter2BossStart();
            }

            state = V02RunState.Combat;
            HidePreBattlePopups();
            bossInfoPanel?.Hide();
            combatController?.StartBattle();
        }

        public void OnCombatStarted()
        {
            state = V02RunState.Combat;
            HidePreBattlePopups();
            bossInfoPanel?.Hide();
        }

        public void OnBattleWin()
        {
            V02RoundConfig round = CurrentRound;
            if (round == null)
            {
                FinishRunWin();
                return;
            }

            if (currentRoundIndex >= GetRoundCount() - 1)
            {
                if (round.isBossRound)
                {
                    if (EnsureMainTrialFlowService().IsChapterTwoBossRound(round))
                    {
                        EnsureMainTrialFlowService().OnChapter2BossWin();
                        OpenChapterTwoBossRewardPanel();
                    }
                    else
                    {
                        EnsureMainTrialFlowService().OnChapter1BossWin();
                        OpenBossRewardPanel();
                    }
                }
                else
                {
                    FinishRunWin();
                }

                return;
            }

            if (TryHandleMainTrialRoundWin(round))
            {
                return;
            }

            OpenRewardSelection();
        }

        public void OnBattleLose()
        {
            FinishRunLose();
        }

        public void OpenRewardSelection()
        {
            state = V02RunState.Reward;
            EnemyDefinition nextEnemy = GetNextEnemy();
            if (nextEnemy == null)
            {
                FinishRunWin();
                return;
            }

            if (rewardController == null || !rewardController.OpenRewardSelection(nextEnemy, CurrentRoundNumber))
            {
                battleLogUI?.AddLog("奖励流程未打开，自动进入下一关");
                ContinueToNextRound();
                return;
            }

            bool formationInfoShown = ShowRewardSelectionInfo(nextEnemy);
            if (formationInfoShown)
            {
                BringFormationInfoToFront();
            }
        }

        public void OnRewardSelected(V02RewardDefinition reward)
        {
            ContinueToNextRound();
        }

        public void ContinueToNextRound()
        {
            if (nextEnemyOverride != null)
            {
                currentRoundIndex = Mathf.Clamp(FindRoundIndex(nextEnemyOverride), 0, Mathf.Max(0, GetRoundCount() - 1));
                nextEnemyOverride = null;
            }
            else
            {
                currentRoundIndex = Mathf.Clamp(currentRoundIndex + 1, 0, Mathf.Max(0, GetRoundCount() - 1));
            }

            EnterPrep();
        }

        public void RequestPostBattlePrepare()
        {
            V02RoundConfig round = CurrentRound;
            if (state != V02RunState.Combat)
            {
                battleLogUI?.AddLog("当前不在巡行战斗中，无需请求战后驻阵");
                return;
            }

            if (!EnsureMainTrialFlowService().IsChapterTwoNormalRound(round))
            {
                battleLogUI?.AddLog("当前关卡不支持战后驻阵请求");
                return;
            }

            if (!postBattlePrepareRequest.Request(round))
            {
                battleLogUI?.AddLog("战后驻阵已请求，当前战斗结束后生效");
                return;
            }

            battleLogUI?.AddLog("已请求战后驻阵：当前战斗结束后进入整备");
        }

        public void FinishRunWin()
        {
            postBattlePrepareRequest.Clear();
            state = V02RunState.RunWin;
            HidePreBattlePopups();
            bossRewardPanel?.Hide();
            fixedRewardPanel?.Hide();
            homeGreyboxPanel?.Hide();
            bossInfoPanel?.Hide();
            combatController?.SetRunComplete();
            SetText(roundInfoText, "\u901a\u5173\u6210\u529f");
            SetText(prepHintText, $"\u4f60\u5b8c\u6210\u4e86 {GetRoundCount()} \u573a\u4e3b\u7ebf\u8bd5\u70bc\u3002");
            runResultPanel?.ShowWin(runStatsTracker, GetRoundCount());
        }

        public void FinishRunLose()
        {
            postBattlePrepareRequest.Clear();
            suppressNextMainTrialAutoStart = false;
            state = V02RunState.RunLose;
            HidePreBattlePopups();
            bossRewardPanel?.Hide();
            fixedRewardPanel?.Hide();
            homeGreyboxPanel?.Hide();
            bossInfoPanel?.Hide();
            V02RoundConfig round = CurrentRound;
            if (failureTracker != null)
            {
                failureTracker.roundLostIndex = round != null ? round.roundIndex : CurrentRoundNumber;
            }

            V02FailureReasonResult reason = failureReasonResolver != null
                ? failureReasonResolver.ResolveFailureReason(failureTracker, this)
                : new V02FailureReasonResult(V02FailureReason.LowDefense, "\u9632\u5fa1\u4e0d\u8db3", "\u9635\u6cd5\u627f\u538b\u4e0d\u8db3\u3002", "\u8c03\u6574\u4f9b\u80fd\u548c\u9632\u5fa1\u7b26\u7b93\u4f4d\u7f6e\u3002");

            SetText(roundInfoText, "\u4fee\u58eb\u8d25\u5317");
            SetText(prepHintText, "巡行受阻，请调整背包后再试");
            runResultPanel?.ShowLose(CurrentRoundNumber, round != null ? round.roundTitle : string.Empty, reason, runStatsTracker, GetRoundCount());
        }

        public void RestartRun()
        {
            StartNewRun();
        }

        public void RetryCurrentRound()
        {
            postBattlePrepareRequest.Clear();
            suppressNextMainTrialAutoStart = true;
            suppressedAutoStartLog = "巡行受阻，请调整背包后再试。";
            nextEnemyOverride = null;
            runResultPanel?.Hide();
            bossRewardPanel?.Hide();
            EnterPrep();
            SetText(roundInfoText, $"{GetRoundLabel(CurrentRound)} 巡行受阻");
            SetText(prepHintText, "巡行受阻，请调整背包后再试。");
        }

        public void SetNextEnemy(EnemyDefinition enemy)
        {
            nextEnemyOverride = enemy;
            battleLogUI?.AddLog(enemy != null ? $"下一关预告：{enemy.GetReadableLabel()}" : "下一关预告已清空");
        }

        public void SkipToRound(int roundNumber)
        {
            currentRoundIndex = Mathf.Clamp(roundNumber - 1, 0, Mathf.Max(0, GetRoundCount() - 1));
            nextEnemyOverride = null;
            suppressNextMainTrialAutoStart = false;
            suppressedAutoStartLog = string.Empty;
            postBattlePrepareRequest.Clear();
            runResultPanel?.Hide();
            EnterPrep();
        }

        public void ForceWinCurrentRound()
        {
            OnBattleWin();
        }

        public void ForceLoseCurrentRound()
        {
            OnBattleLose();
        }

        private void OnRunResultPrimaryRequested()
        {
            if (state == V02RunState.RunLose)
            {
                RetryCurrentRound();
                return;
            }

            RestartRun();
        }

        private void OpenBossRewardPanel()
        {
            TutorialGuideRow guideRow = GetTutorialGuideRow(CurrentRound, TutorialGuideTrigger.BossClear);
            state = V02RunState.Reward;
            HidePreBattlePopups();
            SetText(roundInfoText, !string.IsNullOrWhiteSpace(guideRow?.panelTitle) ? guideRow.panelTitle : "Boss 已击破");
            SetText(prepHintText, !string.IsNullOrWhiteSpace(guideRow?.panelDescription) ? guideRow.panelDescription : "领取章节结算奖励后继续。");

            V02BossRewardPanel panel = EnsureBossRewardPanel();
            if (panel == null)
            {
                ConfirmBossReward();
                return;
            }

            panel.Show(
                !string.IsNullOrWhiteSpace(guideRow?.panelTitle) ? guideRow.panelTitle : "Boss 已击破",
                !string.IsNullOrWhiteSpace(guideRow?.panelDescription) ? guideRow.panelDescription : "领取章节结算奖励后继续。",
                BuildRewardLines(guideRow?.rewardConfig),
                ConfirmBossReward);
        }

        private void ConfirmBossReward()
        {
            TutorialGuideRow guideRow = GetTutorialGuideRow(CurrentRound, TutorialGuideTrigger.BossClear);
            TutorialGuideService guideService = EnsureTutorialGuideService();
            bool flowAdvancedBeforeRuntimeSync = false;
            if (guideRow != null && !guideService.IsCompleted(guideRow))
            {
                RewardResult rewardResult = guideService.GrantReward(guideRow);
                if (rewardResult == null || !rewardResult.HasRewards)
                {
                    LogTutorialGuideRewardResult(guideRow, rewardResult);
                    EnsureMainTrialFlowService().OnChapter1RewardClaimed();
                    OpenChapterOneHomeGreybox();
                    return;
                }

                guideService.MarkCompleted(guideRow);
                EnsureMainTrialFlowService().OnChapter1RewardClaimed();
                flowAdvancedBeforeRuntimeSync = true;
                LogTutorialGuideRewardResult(guideRow, rewardResult);
                try
                {
                    SyncRewardResultToRuntimeInventory(rewardResult);
                }
                catch (System.Exception exception)
                {
                    Debug.LogException(exception);
                    battleLogUI?.AddLog("1-10 奖励已写入存档，但运行时背包同步失败；主线将继续。");
                }

                bossRewardClaimed = true;
            }
            else if (guideRow == null && !bossRewardClaimed)
            {
                battleLogUI?.AddLog("Boss 章节奖励配置缺失，跳过发放。");
            }

            if (!flowAdvancedBeforeRuntimeSync)
            {
                EnsureMainTrialFlowService().OnChapter1RewardClaimed();
            }

            OpenChapterOneHomeGreybox();
        }

        private void OpenChapterTwoBossRewardPanel()
        {
            state = V02RunState.Reward;
            HidePreBattlePopups();
            bossInfoPanel?.Hide();
            SetText(roundInfoText, "2-10 Boss \u7ed3\u7b97");
            SetText(prepHintText, "\u9886\u53d6 2-10 Boss \u9996\u901a\u5956\u52b1\u540e\uff0c\u672c\u8f6e\u6838\u5fc3\u6210\u957f\u95ed\u73af\u5b8c\u6210\u3002");

            V02BossRewardPanel panel = EnsureBossRewardPanel();
            if (panel == null)
            {
                ConfirmChapterTwoBossReward();
                return;
            }

            panel.Show(
                "2-10 Boss \u7ed3\u7b97",
                "\u4f60\u51fb\u8d25\u4e86 2-10 Boss\u3002\u9752\u94dc\u6cd5\u5370\u5148\u4f5c\u4e3a\u5e93\u5b58\u9053\u5177\u5165\u5e93\uff0c\u540e\u7eed\u518d\u63a5\u6cd5\u5668\u6216\u5408\u6210\u7cfb\u7edf\u3002",
                BuildRewardLines(GetChapterTwoBossRewardConfig()),
                ConfirmChapterTwoBossReward);
        }

        private void ConfirmChapterTwoBossReward()
        {
            MainTrialFlowService flowService = EnsureMainTrialFlowService();
            if (flowService.GetCurrentPhase() == MainTrialPhase.CoreLoopComplete)
            {
                OpenCoreLoopCompleteHome();
                return;
            }

            RewardResult rewardResult = EnsureCoreLoopRewardService().GrantBossClearRewards(GetChapterTwoBossRewardConfig());
            LogChapterTwoBossRewardResult(rewardResult);
            flowService.OnChapter2RewardClaimed();
            FinishRunWin();
        }

        private void OpenChapterOneHomeGreybox()
        {
            OpenHomeGreybox(
                "首页灰盒",
                "1-10 章节奖励已到账。这里暂时只承接培养入口、继续主线入口和战后回流。",
                OpenChapterOneCultivationFromHome,
                ContinueChapterOneFromHome);
        }

        private void OpenPostBattleHomeGreybox()
        {
            OpenHomeGreybox(
                "战后驻阵",
                "当前背包阵容已保留。继续主线后进入下一关整备，并保持手动开战。",
                OpenChapterOneCultivationFromHome,
                ContinuePostBattleFromHome);
        }

        private void OpenCoreLoopCompleteHome()
        {
            state = V02RunState.Reward;
            HidePreBattlePopups();
            bossRewardPanel?.Hide();
            fixedRewardPanel?.Hide();
            bossInfoPanel?.Hide();
            runResultPanel?.Hide();

            const string title = "核心闭环已完成";
            const string status = "下一阶段暂未开放。\n当前进度：2-10 已完成。";
            SetText(roundInfoText, title);
            SetText(prepHintText, status);

            MainHomeGreyboxPanel panel = EnsureHomeGreyboxPanel();
            if (panel == null)
            {
                battleLogUI?.AddLog("核心闭环已完成：2-10 已完成，下一阶段暂未开放。");
                return;
            }

            panel.ShowComplete(
                title,
                BuildHomeResourceText(),
                status,
                () => battleLogUI?.AddLog("核心闭环完成页已关闭"));
        }

        private void OpenHomeGreybox(string title, string status, System.Action onCultivate, System.Action onContinue)
        {
            state = V02RunState.Reward;
            HidePreBattlePopups();
            bossRewardPanel?.Hide();
            fixedRewardPanel?.Hide();
            bossInfoPanel?.Hide();
            runResultPanel?.Hide();

            MainHomeGreyboxPanel panel = EnsureHomeGreyboxPanel();
            if (panel == null)
            {
                battleLogUI?.AddLog("首页灰盒未创建，使用继续流程兜底。");
                onContinue?.Invoke();
                return;
            }

            SetText(roundInfoText, title);
            SetText(prepHintText, status);
            panel.Show(title, BuildHomeResourceText(), status, onCultivate, onContinue, () => battleLogUI?.AddLog("首页灰盒已关闭"));
        }

        private void OpenChapterOneCultivationFromHome()
        {
            if (IsChapterOneCultivationCompleted())
            {
                battleLogUI?.AddLog("首页灰盒：培养入口占位，当前章节培养已完成。");
                return;
            }

            homeGreyboxPanel?.Hide();
            if (!TryOpenChapterOneUpgradePanel())
            {
                battleLogUI?.AddLog("首页灰盒：培养入口暂不可用。");
                OpenChapterOneHomeGreybox();
            }
        }

        private void ContinueChapterOneFromHome()
        {
            if (!IsChapterOneCultivationCompleted())
            {
                battleLogUI?.AddLog("首页灰盒：请先通过培养入口完成一次培养，再继续下一段主线。");
                return;
            }

            homeGreyboxPanel?.Hide();
            EnterChapterTwoAutoRun();
        }

        private void ContinuePostBattleFromHome()
        {
            homeGreyboxPanel?.Hide();
            suppressNextMainTrialAutoStart = true;
            suppressedAutoStartLog = "战后驻阵：请调整背包后手动开战。";
            ContinueToNextRound();
            SetText(roundInfoText, $"{GetRoundLabel(CurrentRound)} 战后驻阵");
            SetText(prepHintText, "战后驻阵：请调整背包后手动开战。");
            battleLogUI?.AddLog("战后驻阵：已进入下一关整备。");
        }

        private string BuildHomeResourceText()
        {
            SaveData saveData = EnsureCoreLoopSaveService().EnsureLoaded();
            PlayerResourceData resources = saveData.resourceData;
            return string.Join("\n", new[]
            {
                "当前资源",
                $"- 灵石 {resources.GetAmount(ResourceType.SpiritStone)}",
                $"- 符纸 {resources.GetAmount(ResourceType.TalismanPaper)}",
                $"- 朱砂 {resources.GetAmount(ResourceType.Cinnabar)}",
                $"- 初阶符胚 {resources.GetAmount(ResourceType.BasicTalismanEmbryo)}",
                $"- 修为 {resources.GetAmount(ResourceType.Cultivation)}"
            });
        }

        private V02BossRewardPanel EnsureBossRewardPanel()
        {
            if (bossRewardPanel != null)
            {
                return bossRewardPanel;
            }

            bossRewardPanel = FindObjectOfType<V02BossRewardPanel>(true);
            if (bossRewardPanel != null)
            {
                return bossRewardPanel;
            }

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = FindObjectOfType<Canvas>();
            }

            try
            {
                bossRewardPanel = canvas != null ? V02BossRewardPanel.CreateRuntime(canvas.transform) : null;
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception);
                bossRewardPanel = null;
            }

            return bossRewardPanel;
        }

        private V02FixedRewardPanel EnsureFixedRewardPanel()
        {
            if (fixedRewardPanel != null)
            {
                return fixedRewardPanel;
            }

            fixedRewardPanel = FindObjectOfType<V02FixedRewardPanel>(true);
            if (fixedRewardPanel != null)
            {
                return fixedRewardPanel;
            }

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = FindObjectOfType<Canvas>();
            }

            try
            {
                fixedRewardPanel = canvas != null ? V02FixedRewardPanel.CreateRuntime(canvas.transform) : null;
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception);
                fixedRewardPanel = null;
            }

            return fixedRewardPanel;
        }

        private MainHomeGreyboxPanel EnsureHomeGreyboxPanel()
        {
            if (homeGreyboxPanel != null)
            {
                return homeGreyboxPanel;
            }

            homeGreyboxPanel = FindObjectOfType<MainHomeGreyboxPanel>(true);
            if (homeGreyboxPanel != null)
            {
                return homeGreyboxPanel;
            }

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = FindObjectOfType<Canvas>();
            }

            try
            {
                homeGreyboxPanel = canvas != null ? MainHomeGreyboxPanel.CreateRuntime(canvas.transform) : null;
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception);
                homeGreyboxPanel = null;
            }

            return homeGreyboxPanel;
        }

        private TutorialGuideService EnsureTutorialGuideService()
        {
            if (tutorialGuideService == null)
            {
                tutorialGuideService = TutorialGuideService.GetOrCreate();
            }

            if (tutorialGuideConfig == null)
            {
                tutorialGuideConfig = TutorialGuideConfig.CreateFix03RuntimeDefaults(chapterOneBossRewardConfig);
            }

            tutorialGuideService.Bind(tutorialGuideConfig, EnsureCoreLoopRewardService(), EnsureCoreLoopSaveService());
            return tutorialGuideService;
        }

        private TutorialGuideRow GetTutorialGuideRow(V02RoundConfig round, TutorialGuideTrigger trigger)
        {
            string levelId = NormalizeLevelId(round?.levelId);
            return string.IsNullOrEmpty(levelId) ? null : EnsureTutorialGuideService().FindRow(levelId, trigger);
        }

        private RewardConfig GetChapterTwoBossRewardConfig()
        {
            return chapterTwoBossRewardConfig != null
                ? chapterTwoBossRewardConfig
                : BuildDefaultChapterTwoBossRewardConfig();
        }

        private RewardDropTable GetChapterTwoNormalDropTable()
        {
            return chapterTwoNormalDropTable != null
                ? chapterTwoNormalDropTable
                : BuildDefaultChapterTwoNormalDropTable();
        }

        private static RewardDropTable BuildDefaultChapterTwoNormalDropTable()
        {
            RewardDropTable table = ScriptableObject.CreateInstance<RewardDropTable>();
            table.hideFlags = HideFlags.DontSave;
            table.tableId = "chapter_2_normal_round_drops";
            table.displayName = "2-1 到 2-9 普通关掉落";
            table.drops = new List<RewardDropEntry>
            {
                CreateResourceDrop(ResourceType.SpiritStone, "灵石", 1f, 8, 12),
                CreateResourceDrop(ResourceType.TalismanPaper, "符纸", 0.5f, 1, 2),
                CreateResourceDrop(ResourceType.Cinnabar, "朱砂", 0.3f, 1, 1),
                CreateItemDrop("basic_talisman_embryo_shard", "初阶符胚碎片", 0.15f, 1, 1),
                CreateItemDrop("basic_talisman_page", "基础符箓残页", 0.2f, 1, 1),
                CreateItemDrop("basic_tool_complete", "完整基础道具", 0.05f, 1, 1)
            };
            return table;
        }

        private static RewardDropEntry CreateResourceDrop(ResourceType resourceType, string displayName, float chance, int minAmount, int maxAmount)
        {
            return new RewardDropEntry
            {
                chance = chance,
                minAmount = minAmount,
                maxAmount = maxAmount,
                reward = new RewardEntry
                {
                    rewardType = RewardType.Resource,
                    resourceType = resourceType,
                    amount = 1,
                    displayName = displayName
                }
            };
        }

        private static RewardDropEntry CreateItemDrop(string itemId, string displayName, float chance, int minAmount, int maxAmount)
        {
            return new RewardDropEntry
            {
                chance = chance,
                minAmount = minAmount,
                maxAmount = maxAmount,
                reward = new RewardEntry
                {
                    rewardType = RewardType.Item,
                    itemId = itemId,
                    amount = 1,
                    displayName = displayName
                }
            };
        }

        private static RewardConfig BuildDefaultChapterTwoBossRewardConfig()
        {
            RewardConfig config = ScriptableObject.CreateInstance<RewardConfig>();
            config.hideFlags = HideFlags.DontSave;
            config.rewardId = "boss_2_10_clear";
            config.displayName = "2-10 Boss 结算";
            config.description = "2-10 Boss 首通固定奖励";
            config.rewards = new List<RewardEntry>
            {
                new()
                {
                    rewardType = RewardType.Item,
                    itemId = "bronze_seal_basic",
                    amount = 1,
                    displayName = "青铜法印"
                },
                new()
                {
                    rewardType = RewardType.Resource,
                    resourceType = ResourceType.BasicTalismanEmbryo,
                    amount = 1,
                    displayName = "初阶符胚"
                },
                new()
                {
                    rewardType = RewardType.Resource,
                    resourceType = ResourceType.SpiritStone,
                    amount = 80,
                    displayName = "灵石"
                },
                new()
                {
                    rewardType = RewardType.Resource,
                    resourceType = ResourceType.TalismanPaper,
                    amount = 40,
                    displayName = "符纸"
                },
                new()
                {
                    rewardType = RewardType.Resource,
                    resourceType = ResourceType.Cinnabar,
                    amount = 6,
                    displayName = "朱砂"
                },
                new()
                {
                    rewardType = RewardType.Resource,
                    resourceType = ResourceType.Cultivation,
                    amount = 10,
                    displayName = "修为"
                }
            };
            return config;
        }

        private static string[] BuildRewardLines(RewardConfig rewardConfig)
        {
            if (rewardConfig?.rewards == null || rewardConfig.rewards.Count == 0)
            {
                return new[] { "章节奖励未配置" };
            }

            List<string> lines = new();
            foreach (RewardEntry reward in rewardConfig.rewards)
            {
                if (reward == null || !reward.IsValid)
                {
                    continue;
                }

                string label = !string.IsNullOrWhiteSpace(reward.displayName)
                    ? reward.displayName
                    : reward.rewardType == RewardType.Item ? reward.itemId : reward.resourceType.ToString();
                lines.Add($"{label} x{reward.amount}");
            }

            return lines.ToArray();
        }

        private RewardService EnsureCoreLoopRewardService()
        {
            if (coreLoopRewardService != null)
            {
                return coreLoopRewardService;
            }

            coreLoopRewardService = FindObjectOfType<RewardService>(true);
            if (coreLoopRewardService != null)
            {
                return coreLoopRewardService;
            }

            GameObject serviceObject = new("CoreLoopRewardService_Runtime");
            coreLoopRewardService = serviceObject.AddComponent<RewardService>();
            return coreLoopRewardService;
        }

        private SaveService EnsureCoreLoopSaveService()
        {
            coreLoopSaveService ??= SaveService.GetOrCreate();
            return coreLoopSaveService;
        }

        private void LogTutorialGuideRewardResult(TutorialGuideRow guideRow, RewardResult rewardResult)
        {
            string label = !string.IsNullOrWhiteSpace(guideRow?.rewardConfig?.displayName)
                ? guideRow.rewardConfig.displayName
                : !string.IsNullOrWhiteSpace(guideRow?.panelTitle) ? guideRow.panelTitle : "引导奖励";
            if (rewardResult == null || !rewardResult.HasRewards)
            {
                battleLogUI?.AddLog($"{label}：没有可发放奖励");
                return;
            }

            battleLogUI?.AddLog($"{label}：{string.Join("、", BuildRewardLinesFromResult(rewardResult))}");
        }

        private void LogChapterTwoBossRewardResult(RewardResult rewardResult)
        {
            if (rewardResult == null || !rewardResult.HasRewards)
            {
                battleLogUI?.AddLog("2-10 Boss 结算：没有可发放奖励");
                return;
            }

            battleLogUI?.AddLog($"2-10 Boss 结算：{string.Join("、", BuildRewardLinesFromResult(rewardResult))}");
        }

        private void TryGrantChapterTwoNormalRoundDrop(V02RoundConfig round)
        {
            if (!EnsureMainTrialFlowService().IsChapterTwoNormalRound(round))
            {
                return;
            }

            RewardResult rewardResult = EnsureCoreLoopRewardService().GrantDropTable(GetChapterTwoNormalDropTable());
            if (rewardResult == null || !rewardResult.HasRewards)
            {
                battleLogUI?.AddLog($"{round.levelId} \u6389\u843d\uff1a\u65e0");
                return;
            }

            battleLogUI?.AddLog($"{round.levelId} \u6389\u843d\uff1a{string.Join("\u3001", BuildRewardLinesFromResult(rewardResult))}");
        }

        private bool TryOpenChapterOneUpgradePanel()
        {
            if (IsChapterOneCultivationCompleted())
            {
                return false;
            }

            UpgradeService upgradeService = EnsureCoreLoopUpgradeService();
            string targetItemId = string.IsNullOrWhiteSpace(chapterOneUpgradeItemId)
                ? "fire_talisman_basic"
                : chapterOneUpgradeItemId.Trim();

            if (upgradeService.GetLevel(targetItemId) >= 2)
            {
                MarkChapterOneCultivationCompleted(targetItemId);
                return false;
            }

            TalismanUpgradePanel panel = EnsureTalismanUpgradePanel();
            if (panel == null)
            {
                return false;
            }

            state = V02RunState.Reward;
            HidePreBattlePopups();
            SetText(roundInfoText, "符箓培养");
            SetText(prepHintText, "使用 1-10 章节奖励完成一次符箓培养。");
            panel.Show(upgradeService, targetItemId, MarkChapterOneCultivationCompleted, EnterChapterTwoAutoRun);
            return true;
        }

        private bool TryHandleMainTrialRoundWin(V02RoundConfig round)
        {
            MainTrialFlowService flowService = EnsureMainTrialFlowService();
            if (!flowService.IsMainTrialRound(round) || round.isBossRound)
            {
                return false;
            }

            bool shouldStopForPostBattlePrepare = postBattlePrepareRequest.ConsumeIfMatches(round);
            if (flowService.IsChapterOneRound(round) &&
                TryOpenTutorialGuideRewardPanel(round, () => CompleteMainTrialNormalRoundWin(round, shouldStopForPostBattlePrepare)))
            {
                return true;
            }

            CompleteMainTrialNormalRoundWin(round, shouldStopForPostBattlePrepare);
            return true;
        }

        private void CompleteMainTrialNormalRoundWin(V02RoundConfig round, bool shouldStopForPostBattlePrepare)
        {
            MainTrialFlowService flowService = EnsureMainTrialFlowService();
            TryGrantChapterTwoNormalRoundDrop(round);
            flowService.MarkRoundCleared(round);
            if (flowService.ShouldStopBeforeChapterTwoBoss(round))
            {
                OpenChapterTwoBossGate();
                return;
            }

            if (shouldStopForPostBattlePrepare)
            {
                OpenPostBattleHomeGreybox();
                return;
            }

            ContinueToNextRound();
        }

        private bool TryOpenTutorialGuideRewardPanel(V02RoundConfig round, System.Action onConfirmed)
        {
            TutorialGuideRow guideRow = GetTutorialGuideRow(round, TutorialGuideTrigger.RoundWin);
            if (guideRow == null)
            {
                return false;
            }

            TutorialGuideService guideService = EnsureTutorialGuideService();
            bool shouldGrantReward = !guideService.IsCompleted(guideRow);

            state = V02RunState.Reward;
            HidePreBattlePopups();
            SetText(roundInfoText, !string.IsNullOrWhiteSpace(guideRow.panelTitle) ? guideRow.panelTitle : "固定教学奖励");
            SetText(prepHintText, !string.IsNullOrWhiteSpace(guideRow.panelDescription) ? guideRow.panelDescription : "确认后进入下一关。");

            V02FixedRewardPanel panel = EnsureFixedRewardPanel();
            if (panel == null)
            {
                battleLogUI?.AddLog($"Fixed tutorial reward panel missing, block progression: {guideRow.GetSafeLevelId()}");
                return true;
            }

            panel.Show(
                guideRow.panelTitle,
                guideRow.panelSubject,
                guideRow.panelDescription,
                () =>
                {
                    bool rewardReady = shouldGrantReward
                        ? GrantTutorialGuideReward(guideRow)
                        : EnsureClaimedTutorialRewardAvailable(guideRow);
                    if (!rewardReady && !shouldGrantReward)
                    {
                        rewardReady = GrantTutorialGuideReward(guideRow);
                    }

                    if (!rewardReady)
                    {
                        battleLogUI?.AddLog($"Fixed tutorial reward confirm blocked: {guideRow.GetSafeLevelId()}/{guideRow.GetSafeOnceFlag()}");
                        return;
                    }

                    suppressNextMainTrialAutoStart = true;
                    suppressedAutoStartLog = "已领取教学奖励，请将新道具放入阵盘后手动开始斗法。";
                    onConfirmed?.Invoke();
                });
            return true;
        }

        private bool TryOpenPreBattleTutorialGuidePanel(V02RoundConfig round)
        {
            TutorialGuideRow guideRow = GetTutorialGuideRow(round, TutorialGuideTrigger.PreBattle);
            if (guideRow == null)
            {
                return false;
            }

            TutorialGuideService guideService = EnsureTutorialGuideService();
            if (guideService.IsCompleted(guideRow))
            {
                return false;
            }

            V02FixedRewardPanel panel = EnsureFixedRewardPanel();
            if (panel == null)
            {
                battleLogUI?.AddLog($"Fixed pre-battle tutorial panel missing: {guideRow.GetSafeLevelId()}");
                return false;
            }

            state = V02RunState.Prep;
            HidePreBattlePopups();
            SetText(roundInfoText, !string.IsNullOrWhiteSpace(guideRow.panelTitle) ? guideRow.panelTitle : "教学说明");
            SetText(prepHintText, guideRow.panelDescription);
            panel.Show(
                guideRow.panelTitle,
                guideRow.panelSubject,
                guideRow.panelDescription,
                () =>
                {
                    guideService.MarkCompleted(guideRow);
                    battleLogUI?.AddLog("教学说明已确认，请调整阵盘后手动开始斗法。");
                });
            return true;
        }

        private bool GrantTutorialGuideReward(TutorialGuideRow guideRow)
        {
            if (guideRow == null || !guideRow.HasReward)
            {
                return false;
            }

            TutorialGuideService guideService = EnsureTutorialGuideService();
            if (guideService.IsCompleted(guideRow) && EnsureClaimedTutorialRewardAvailable(guideRow))
            {
                return false;
            }

            RewardResult rewardResult = guideService.GrantReward(guideRow);
            if (rewardResult == null || !rewardResult.HasRewards)
            {
                battleLogUI?.AddLog($"{guideRow.GetSafeLevelId()} 引导奖励：无可发放奖励");
                return false;
            }

            bool syncedToRuntimeInventory = SyncRewardResultToRuntimeInventory(rewardResult);
            guideService.MarkCompleted(guideRow);
            LogTutorialGuideRewardResult(guideRow, rewardResult);
            if (!syncedToRuntimeInventory && guideRow.blockFlowWhenRuntimeInventorySyncFails)
            {
                battleLogUI?.AddLog($"Tutorial guide reward granted to save, but runtime bag sync failed: {guideRow.GetSafeOnceFlag()}");
                return false;
            }

            return true;
        }

        private bool EnsureClaimedTutorialRewardAvailable(TutorialGuideRow guideRow)
        {
            if (guideRow == null || !guideRow.HasReward)
            {
                return false;
            }

            TutorialGuideService guideService = EnsureTutorialGuideService();
            if (!guideService.AreRewardItemsInSave(guideRow))
            {
                battleLogUI?.AddLog($"Tutorial guide reward claim exists but saved item is missing, will regrant: {guideRow.GetSafeLevelId()}/{guideRow.GetSafeOnceFlag()}");
                return false;
            }

            RewardResult syncResult = new()
            {
                rewardId = guideRow.rewardConfig != null ? guideRow.rewardConfig.rewardId : guideRow.GetSafeOnceFlag(),
                displayName = guideRow.rewardConfig != null ? guideRow.rewardConfig.displayName : guideRow.panelTitle
            };

            if (guideRow.rewardConfig?.rewards != null)
            {
                foreach (RewardEntry reward in guideRow.rewardConfig.rewards)
                {
                    if (reward == null || reward.rewardType != RewardType.Item || string.IsNullOrWhiteSpace(reward.itemId))
                    {
                        continue;
                    }

                    syncResult.rewards.Add(reward.CloneWithAmount(reward.amount));
                }
            }

            bool runtimeSynced = SyncRewardResultToRuntimeInventory(syncResult);
            if (!runtimeSynced)
            {
                battleLogUI?.AddLog($"Tutorial guide reward exists in save but runtime bag sync failed: {guideRow.GetSafeOnceFlag()}");
            }

            return runtimeSynced || !guideRow.blockFlowWhenRuntimeInventorySyncFails;
        }

        private bool SyncRewardResultToRuntimeInventory(RewardResult rewardResult)
        {
            if (rewardResult?.rewards == null)
            {
                return true;
            }

            V02RewardController controller = EnsureRewardController();
            bool syncedAll = true;
            foreach (RewardEntry reward in rewardResult.rewards)
            {
                if (reward == null || reward.rewardType != RewardType.Item || string.IsNullOrWhiteSpace(reward.itemId))
                {
                    continue;
                }

                if (controller == null || !controller.EnsureGrantedTalismanInRuntimeInventory(reward.itemId, reward.amount))
                {
                    syncedAll = false;
                    battleLogUI?.AddLog($"奖励已写入存档库存，但未同步到可拖拽区：{reward.itemId}");
                }
            }

            return syncedAll;
        }

        private V02RewardController EnsureRewardController()
        {
            if (rewardController != null)
            {
                return rewardController;
            }

            rewardController = FindObjectOfType<V02RewardController>(true);
            return rewardController;
        }

        private static string NormalizeLevelId(string levelId)
        {
            return string.IsNullOrWhiteSpace(levelId) ? string.Empty : levelId.Trim();
        }

        private void EnterPostBattlePrepare()
        {
            suppressNextMainTrialAutoStart = true;
            suppressedAutoStartLog = "战后驻阵：请调整背包后手动开战。";
            ContinueToNextRound();
            SetText(roundInfoText, $"{GetRoundLabel(CurrentRound)} 战后驻阵");
            SetText(prepHintText, "战后驻阵：请调整背包后手动开战。");
            battleLogUI?.AddLog("战后驻阵：已进入下一关整备。");
        }

        private void EnterChapterTwoAutoRun()
        {
            MainTrialFlowService flowService = EnsureMainTrialFlowService();
            activeRunConfig = flowService.GetOrCreateChapterTwoRunConfig(GetBaseRunConfig());
            flowService.MarkChapterTwoStarted();
            currentRoundIndex = flowService.GetChapterTwoStartIndex(activeRunConfig);
            nextEnemyOverride = null;
            bossRewardPanel?.Hide();
            bossInfoPanel?.Hide();
            talismanUpgradePanel?.Hide();
            battleLogUI?.AddLog("进入 2-10 巡行主线：2-1 到 2-9 将自动推进。");
            EnterPrep();
        }

        private void OpenChapterTwoBossGate()
        {
            MainTrialFlowService flowService = EnsureMainTrialFlowService();
            postBattlePrepareRequest.Clear();
            flowService.MarkChapterTwoBossUnlocked();
            int bossIndex = FindRoundIndexByLevelId("2-10");
            if (bossIndex >= 0)
            {
                currentRoundIndex = bossIndex;
            }

            nextEnemyOverride = null;
            EnterPrep();
            SetText(roundInfoText, "2-10 Boss 前停止");
            SetText(prepHintText, "前方煞气聚阵，请先查看敌情并整备背包。");
            battleLogUI?.AddLog("2-9 巡行完成，已停在 2-10 Boss 前。");
        }

        private bool TryOpenChapterTwoBossInfoPanel(V02RoundConfig round)
        {
            if (!EnsureMainTrialFlowService().IsChapterTwoBossRound(round))
            {
                return false;
            }

            BossInfoPanel panel = EnsureBossInfoPanel();
            if (panel == null)
            {
                battleLogUI?.AddLog("2-10 Boss 信息面板未创建，仍可通过开始斗法手动开战");
                return true;
            }

            SetText(roundInfoText, "2-10 Boss 前停止");
            SetText(prepHintText, "前方煞气聚阵，请先查看敌情并整备背包。");
            BossInfoViewModel viewModel = BossInfoViewModel.From(GetChapterTwoBossInfoConfig(), round?.enemy);
            panel.Show(viewModel, HideChapterTwoBossInfoForPrepare, StartChapterTwoBossCombat);
            battleLogUI?.AddLog("2-10 Boss 信息已展开，请整备后手动开战。");
            return true;
        }

        private void HideChapterTwoBossInfoForPrepare()
        {
            battleLogUI?.AddLog("2-10 Boss 准备阶段：可调整背包后手动开战。");
        }

        private void StartChapterTwoBossCombat()
        {
            if (!EnsureMainTrialFlowService().IsChapterTwoBossRound(CurrentRound))
            {
                battleLogUI?.AddLog("当前不在 2-10 Boss 准备阶段");
                return;
            }

            battleLogUI?.AddLog("2-10 Boss 手动开战");
            StartCombat();
        }

        private void TryAutoStartMainTrialRound(V02RoundConfig round)
        {
            if (suppressNextMainTrialAutoStart)
            {
                suppressNextMainTrialAutoStart = false;
                if (!string.IsNullOrWhiteSpace(suppressedAutoStartLog))
                {
                    battleLogUI?.AddLog(suppressedAutoStartLog);
                }

                suppressedAutoStartLog = string.Empty;
                return;
            }

            if (!EnsureMainTrialFlowService().ShouldAutoStartRound(round))
            {
                return;
            }

            battleLogUI?.AddLog($"{round.levelId} 自动巡行开战");
            StartCombat();
        }

        private BossInfoPanel EnsureBossInfoPanel()
        {
            if (bossInfoPanel != null)
            {
                return bossInfoPanel;
            }

            bossInfoPanel = FindObjectOfType<BossInfoPanel>(true);
            if (bossInfoPanel != null)
            {
                return bossInfoPanel;
            }

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = FindObjectOfType<Canvas>();
            }

            try
            {
                bossInfoPanel = canvas != null ? BossInfoPanel.CreateRuntime(canvas.transform) : null;
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception);
                bossInfoPanel = null;
            }

            return bossInfoPanel;
        }

        private BossInfoConfig GetChapterTwoBossInfoConfig()
        {
            return chapterTwoBossInfoConfig != null
                ? chapterTwoBossInfoConfig
                : BuildDefaultChapterTwoBossInfoConfig();
        }

        private static BossInfoConfig BuildDefaultChapterTwoBossInfoConfig()
        {
            BossInfoConfig config = ScriptableObject.CreateInstance<BossInfoConfig>();
            config.hideFlags = HideFlags.DontSave;
            config.bossId = "formation_breaker_boss";
            config.bossName = "2-10 Boss：煞气聚阵";
            config.mechanismTags = "护盾压制 / 妖群召唤 / 阵眼封印";
            config.mainThreats = "Boss 会轮流施加护盾压力、妖群压力和阵眼封印压力。";
            config.recommendedTools = "推荐携带雷符、净化符、镇魂符、护身符，并保持核心符箓供能。";
            config.preBattlePrompt = "前方煞气聚阵，请先查看敌情并整备背包。";
            return config;
        }

        private UpgradeService EnsureCoreLoopUpgradeService()
        {
            if (coreLoopUpgradeService != null)
            {
                return coreLoopUpgradeService;
            }

            coreLoopUpgradeService = FindObjectOfType<UpgradeService>(true);
            if (coreLoopUpgradeService != null)
            {
                coreLoopUpgradeService.Bind(EnsureCoreLoopSaveService(), null);
                return coreLoopUpgradeService;
            }

            GameObject serviceObject = new("CoreLoopUpgradeService_Runtime");
            coreLoopUpgradeService = serviceObject.AddComponent<UpgradeService>();
            coreLoopUpgradeService.Bind(EnsureCoreLoopSaveService(), null);
            return coreLoopUpgradeService;
        }

        private MainTrialFlowService EnsureMainTrialFlowService()
        {
            if (mainTrialFlowService != null)
            {
                return mainTrialFlowService;
            }

            mainTrialFlowService = FindObjectOfType<MainTrialFlowService>(true);
            if (mainTrialFlowService != null)
            {
                mainTrialFlowService.Bind(EnsureCoreLoopSaveService());
                return mainTrialFlowService;
            }

            GameObject serviceObject = new("CoreLoopMainTrialFlowService_Runtime");
            mainTrialFlowService = serviceObject.AddComponent<MainTrialFlowService>();
            mainTrialFlowService.Bind(EnsureCoreLoopSaveService());
            return mainTrialFlowService;
        }

        private TalismanUpgradePanel EnsureTalismanUpgradePanel()
        {
            if (talismanUpgradePanel != null)
            {
                return talismanUpgradePanel;
            }

            talismanUpgradePanel = FindObjectOfType<TalismanUpgradePanel>(true);
            if (talismanUpgradePanel != null)
            {
                return talismanUpgradePanel;
            }

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = FindObjectOfType<Canvas>();
            }

            try
            {
                talismanUpgradePanel = canvas != null ? TalismanUpgradePanel.CreateRuntime(canvas.transform) : null;
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception);
                talismanUpgradePanel = null;
            }

            return talismanUpgradePanel;
        }

        private bool IsChapterOneCultivationCompleted()
        {
            SaveData saveData = EnsureCoreLoopSaveService().EnsureLoaded();
            return saveData.mainTrialProgressData != null && saveData.mainTrialProgressData.chapterOneCultivationCompleted;
        }

        private void MarkChapterOneCultivationCompleted(string itemId)
        {
            EnsureMainTrialFlowService().OnFirstUpgradeCompleted(itemId);
            battleLogUI?.AddLog($"符箓培养完成：{itemId} Lv.2");
        }

        private static string[] BuildRewardLinesFromResult(RewardResult rewardResult)
        {
            if (rewardResult?.rewards == null)
            {
                return new[] { "无奖励" };
            }

            List<string> lines = new();
            foreach (RewardEntry reward in rewardResult.rewards)
            {
                if (reward == null || !reward.IsValid)
                {
                    continue;
                }

                string label = !string.IsNullOrWhiteSpace(reward.displayName)
                    ? reward.displayName
                    : reward.rewardType == RewardType.Item ? reward.itemId : reward.resourceType.ToString();
                lines.Add($"{label} x{reward.amount}");
            }

            return lines.Count > 0 ? lines.ToArray() : new[] { "无奖励" };
        }

        private void BindPreBattlePopupButtons()
        {
            if (preBattlePopupButtonsBound)
            {
                return;
            }

            EnsurePreBattlePopupReferences();
            formationInfoCloseButton?.onClick.AddListener(HideFormationInfoPanel);
            tagTooltipCloseButton?.onClick.AddListener(HideTagTooltipPanel);
            preBattlePopupButtonsBound = true;
        }

        private void UnbindPreBattlePopupButtons()
        {
            if (!preBattlePopupButtonsBound)
            {
                return;
            }

            formationInfoCloseButton?.onClick.RemoveListener(HideFormationInfoPanel);
            tagTooltipCloseButton?.onClick.RemoveListener(HideTagTooltipPanel);
            preBattlePopupButtonsBound = false;
        }

        private void ShowPreBattlePopups()
        {
            EnsurePreBattlePopupReferences();
            if (EnsureMainTrialFlowService().IsMainTrialRound(CurrentRound))
            {
                HideFormationInfoPanel();
                HideTagTooltipPanel();
                return;
            }

            TryShowFormationInfoForRound(currentRoundIndex);
            HideTagTooltipPanel();
        }

        private void HidePreBattlePopups()
        {
            EnsurePreBattlePopupReferences();
            HideFormationInfoPanel();
            HideTagTooltipPanel();
        }

        private void HideFormationInfoPanel()
        {
            SetPopupVisible(formationInfoPanel, ref formationInfoCanvasGroup, false);
        }

        private void HideTagTooltipPanel()
        {
            SetPopupVisible(tagTooltipPanel, ref tagTooltipCanvasGroup, false);
        }

        private bool ShowRewardSelectionInfo(EnemyDefinition nextEnemy)
        {
            EnsurePreBattlePopupReferences();

            string enemyLabel = nextEnemy != null ? nextEnemy.GetReadableLabel() : "\u654c\u4eba\u672a\u914d\u7f6e";
            string weaknessTags = nextEnemy != null ? TalismanTagUtility.JoinCounterTags(nextEnemy.weaknessTags) : "\u65e0";
            SetText(roundInfoText, $"\u5956\u52b1\u9009\u62e9\uff1a\u4e0b\u4e00\u5173 {enemyLabel}");
            SetText(prepHintText, $"\u4e09\u9009\u4e00\uff1a\u4f18\u5148\u9009\u80fd\u514b\u5236\u4e0b\u4e00\u5173\u7684\u5956\u52b1\u3002\n\u4e0b\u4e00\u5173\u5f31\u70b9\uff1a{weaknessTags}");

            int nextRoundIndex = GetFormationInfoRoundIndex(nextEnemy, currentRoundIndex + 1);
            bool shown = TryShowFormationInfoForRound(nextRoundIndex);
            HideTagTooltipPanel();
            return shown;
        }

        private bool TryShowFormationInfoForRound(int roundIndex)
        {
            EnsurePreBattlePopupReferences();
            int safeRoundIndex = Mathf.Max(0, roundIndex);
            if (shownFormationInfoRoundIndexes.Contains(safeRoundIndex))
            {
                HideFormationInfoPanel();
                return false;
            }

            shownFormationInfoRoundIndexes.Add(safeRoundIndex);
            SetPopupVisible(formationInfoPanel, ref formationInfoCanvasGroup, true);
            return true;
        }

        private int GetFormationInfoRoundIndex(EnemyDefinition enemy, int fallbackIndex)
        {
            V02RunConfig currentConfig = GetActiveRunConfig();
            if (enemy != null && currentConfig?.rounds != null)
            {
                for (int i = 0; i < currentConfig.rounds.Count; i++)
                {
                    if (currentConfig.rounds[i]?.enemy == enemy)
                    {
                        return i;
                    }
                }
            }

            return Mathf.Clamp(fallbackIndex, 0, Mathf.Max(0, GetRoundCount() - 1));
        }

        private void BringFormationInfoToFront()
        {
            if (formationInfoPanel != null)
            {
                formationInfoPanel.transform.SetAsLastSibling();
            }
        }

        private void EnsurePreBattlePopupReferences()
        {
            if (formationInfoPanel == null)
            {
                formationInfoPanel = GameObject.Find("V02FormationInfoArea");
            }

            if (tagTooltipPanel == null)
            {
                tagTooltipPanel = GameObject.Find("V02TagTooltipPanel");
            }

            if (formationInfoCloseButton == null)
            {
                formationInfoCloseButton = FindChildButton(formationInfoPanel, "FormationInfoCloseButton");
            }

            if (tagTooltipCloseButton == null)
            {
                tagTooltipCloseButton = FindChildButton(tagTooltipPanel, "TooltipCloseButton");
            }
        }

        private static Button FindChildButton(GameObject root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child != null && child.name == childName && child.TryGetComponent(out Button button))
                {
                    return button;
                }
            }

            return null;
        }

        private static void SetPopupVisible(GameObject panel, ref CanvasGroup canvasGroup, bool visible)
        {
            if (panel == null)
            {
                return;
            }

            panel.SetActive(true);
            if (canvasGroup == null)
            {
                canvasGroup = panel.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = panel.AddComponent<CanvasGroup>();
                }
            }

            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }

        private void RefreshRoundTexts(V02RoundConfig round, EnemyDefinition resolvedEnemy = null)
        {
            EnemyDefinition enemy = resolvedEnemy != null ? resolvedEnemy : ResolveMainTrialEnemy(round);
            string enemyLabel = enemy != null ? enemy.GetReadableLabel() : "\u654c\u4eba\u672a\u914d\u7f6e";
            string title = string.IsNullOrWhiteSpace(round.roundTitle) ? string.Empty : $"  {round.roundTitle}";
            string roundLine = $"{GetRoundLabel(round)} / {GetRoundCount()}  {enemyLabel}{title}";
            string goal = string.IsNullOrWhiteSpace(round.teachingGoal) ? string.Empty : $"\n\u76ee\u6807\uff1a{round.teachingGoal}";
            SetText(roundInfoText, $"{roundLine}{goal}");
            SetText(prepHintText, BuildPreBattleHint(round, enemy));
            SetText(GetCurrentLevelText(), GetRoundLabel(round));
        }

        private Text GetCurrentLevelText()
        {
            if (currentLevelText != null)
            {
                return currentLevelText;
            }

            GameObject topStatusBar = GameObject.Find("V02TopStatusBar");
            Transform target = topStatusBar != null ? topStatusBar.transform.Find("TEXT") : null;
            target ??= topStatusBar != null ? topStatusBar.transform.Find("CurrentLevelText") : null;
            if (target == null || !target.TryGetComponent(out currentLevelText))
            {
                return null;
            }

            target.name = "CurrentLevelText";
            return currentLevelText;
        }

        private static string BuildPreBattleHint(V02RoundConfig round, EnemyDefinition resolvedEnemy = null)
        {
            if (round == null)
            {
                return string.Empty;
            }

            EnemyDefinition enemy = resolvedEnemy != null ? resolvedEnemy : round.enemy;
            string enemyLabel = enemy != null ? enemy.GetReadableLabel() : "\u654c\u4eba\u672a\u914d\u7f6e";
            string title = string.IsNullOrWhiteSpace(round.roundTitle) ? string.Empty : $"  {round.roundTitle.Trim()}";
            string hint = string.IsNullOrWhiteSpace(round.preBattleHint)
                ? "\u89c2\u5bdf\u654c\u4eba\u5a01\u80c1\uff0c\u5f00\u6218\u524d\u8c03\u6574\u9635\u76d8\u3002"
                : round.preBattleHint.Trim();

            return $"{enemyLabel}{title}\n{hint}";
        }

        private EnemyDefinition ResolveMainTrialEnemy(V02RoundConfig round)
        {
            if (round == null)
            {
                return null;
            }

            if (!EnsureMainTrialFlowService().IsChapterOneRound(round))
            {
                return round.enemy;
            }

            string levelId = NormalizeLevelId(round.levelId);
            if (string.IsNullOrEmpty(levelId))
            {
                return round.enemy;
            }

            if (mainTrialRuntimeEnemies.TryGetValue(levelId, out EnemyDefinition cachedEnemy) && cachedEnemy != null)
            {
                return cachedEnemy;
            }

            EnemyDefinition resolvedEnemy = BuildChapterOneRuntimeEnemy(round, levelId);
            if (resolvedEnemy != null)
            {
                mainTrialRuntimeEnemies[levelId] = resolvedEnemy;
                return resolvedEnemy;
            }

            return round.enemy;
        }

        private EnemyDefinition BuildChapterOneRuntimeEnemy(V02RoundConfig round, string levelId)
        {
            EnemyDefinition common = FindConfiguredEnemyByLevelId("1-1") ?? round.enemy;
            EnemyDefinition shield = FindConfiguredEnemyByLevelId("1-2") ?? round.enemy;
            EnemyDefinition pressure = FindConfiguredEnemyByLevelId("1-3") ?? common ?? round.enemy;
            EnemyDefinition poison = FindConfiguredEnemyByLevelId("1-4") ?? round.enemy;
            EnemyDefinition seal = FindConfiguredEnemyByLevelId("1-5") ?? round.enemy;
            EnemyDefinition disrupt = FindConfiguredEnemyByLevelId("1-6") ?? seal ?? round.enemy;
            EnemyDefinition boss = FindConfiguredEnemyByLevelId("1-10") ?? round.enemy;

            return levelId switch
            {
                "1-1" => CloneEnemyForStage(common, "common_restless_imp", "游灯小祟", EnemyType.Ghost, 72, 6, 2.8f, "普通小怪", "普通攻击", "把火符放到聚灵石供能范围内。", null, null, "1-1 low pressure energy tutorial"),
                "1-2" => CloneEnemyForStage(common, "common_restless_imp", "游灯小祟", EnemyType.Ghost, 96, 8, 2.6f, "普通小怪加强", "普通攻击加强", "复习基础输出。下一关会出现护盾。", null, null, "1-2 reused common imp, stronger hp/damage"),
                "1-3" => CloneEnemyForStage(shield, "shield_stone_talisman_guard", "石甲符卫", EnemyType.Ghost, 110, 7, 2.8f, "低强度护盾", "周期护盾", "敌人有护盾，雷符可破盾。", new[] { CounterTag.Shield }, BuildStageSkills(shield, "1-3"), "1-3 shield tutorial"),
                "1-4" => CloneEnemyForStage(shield, "shield_stone_talisman_guard", "石甲符卫", EnemyType.Ghost, 135, 8, 2.7f, "护盾复习", "更厚护盾", "护盾略厚，复习破盾和输出。", new[] { CounterTag.Shield }, BuildStageSkills(shield, "1-4"), "1-4 reused shield guard review"),
                "1-5" => CloneEnemyForStage(pressure, "pressure_bronze_claw_wight", "铜爪压阵怪", EnemyType.Ghost, 140, 13, 2.2f, "承压近战", "高压普攻", "护身符可以明显降低承压风险。", new[] { CounterTag.Charge }, null, "1-5 pressure tutorial, reused configured prototype"),
                "1-6" => CloneEnemyForStage(poison, "debuff_cinder_poison_sprite", "灰火毒祟", EnemyType.Ghost, 130, 8, 2.8f, "毒火负面", "低频毒/燃", "净化符可以清除毒、燃烧等负面状态。", new[] { CounterTag.Poison, CounterTag.Burn }, BuildStageSkills(poison, "1-6"), "1-6 debuff tutorial"),
                "1-7" => CloneEnemyForStage(disrupt, "disrupt_soul_seal_ghost", "摄灵封符鬼", EnemyType.Ghost, 135, 8, 2.8f, "低频偷灵/封符", "偷灵与封符", "镇魂符可以反制偷灵、封符等异常干扰。", new[] { CounterTag.StealEnergy, CounterTag.Seal }, BuildDisruptSkills(disrupt, seal, "1-7"), "1-7 disruption tutorial"),
                "1-8" => CloneEnemyForStage(disrupt, "disrupt_soul_seal_ghost", "摄灵封符鬼", EnemyType.Ghost, 155, 9, 2.65f, "异常干扰复习", "偷灵与封符频率略高", "异常干扰频率略高，但不会形成硬卡。", new[] { CounterTag.StealEnergy, CounterTag.Seal }, BuildDisruptSkills(disrupt, seal, "1-8"), "1-8 reused disruption review"),
                "1-9" => CloneEnemyForStage(common, "common_restless_imp+shield_stone_talisman_guard", "游灯小祟 + 石甲符卫", EnemyType.Ghost, 175, 9, 2.45f, "Boss前综合小测", "多目标 + 低强度护盾", "多只游灯小祟配合低强度石甲符卫，给连锁雷符发挥空间。", new[] { CounterTag.Group, CounterTag.Shield }, BuildStageSkills(shield, "1-9"), "1-9 reuses common imp and shield guard"),
                "1-10" => CloneEnemyForStage(boss, "boss_array_breaking_warlord", "破阵妖将", EnemyType.Boss, 240, 12, 2.8f, "第一章 Boss 综合题", "护盾 / 召唤 / 封符综合", "第一章综合 Boss，不做数值墙。", new[] { CounterTag.Shield, CounterTag.Group, CounterTag.Seal, CounterTag.StealEnergy, CounterTag.Boss }, BuildStageSkills(boss, "1-10"), "1-10 chapter boss"),
                _ => round.enemy
            };
        }

        private EnemyDefinition CloneEnemyForStage(
            EnemyDefinition source,
            string enemyId,
            string displayName,
            EnemyType enemyType,
            int maxHp,
            int attackDamage,
            float attackInterval,
            string weaknessText,
            string dangerText,
            string recommendedBuildText,
            IEnumerable<CounterTag> weaknessTags,
            IEnumerable<EnemySkillDefinition> skills,
            string archetype)
        {
            EnemyDefinition clone = ScriptableObject.CreateInstance<EnemyDefinition>();
            clone.hideFlags = HideFlags.DontSave;
            clone.name = enemyId;
            clone.enemyId = enemyId;
            clone.displayName = displayName;
            clone.enabled = true;
            clone.enemyType = enemyType;
            clone.maxHp = Mathf.Max(1, maxHp);
            clone.attackDamage = Mathf.Max(0, attackDamage);
            clone.attackInterval = Mathf.Max(0.1f, attackInterval);
            clone.weaknessText = weaknessText;
            clone.dangerText = dangerText;
            clone.recommendedBuildText = recommendedBuildText;
            clone.icon = source != null ? source.icon : null;
            clone.enemyClass = "ChapterOneGuide";
            clone.enemyArchetype = archetype;
            clone.intentText = dangerText;
            clone.recommendedCounterText = recommendedBuildText;

            CopyTags(source, clone);
            clone.weaknessTags.Clear();
            if (weaknessTags != null)
            {
                clone.weaknessTags.AddRange(weaknessTags);
            }

            clone.skills.Clear();
            if (skills != null)
            {
                clone.skills.AddRange(skills);
            }

            return clone;
        }

        private static void CopyTags(EnemyDefinition source, EnemyDefinition target)
        {
            if (source == null || target == null)
            {
                return;
            }

            target.resistTags = source.resistTags != null ? new List<CounterTag>(source.resistTags) : new List<CounterTag>();
            target.resistedElements = source.resistedElements != null ? new List<ElementTag>(source.resistedElements) : new List<ElementTag>();
            target.resistedFunctions = source.resistedFunctions != null ? new List<FunctionTag>(source.resistedFunctions) : new List<FunctionTag>();
            target.vulnerableElements = source.vulnerableElements != null ? new List<ElementTag>(source.vulnerableElements) : new List<ElementTag>();
            target.vulnerableFunctions = source.vulnerableFunctions != null ? new List<FunctionTag>(source.vulnerableFunctions) : new List<FunctionTag>();
        }

        private List<EnemySkillDefinition> BuildStageSkills(EnemyDefinition source, string levelId)
        {
            if (source?.skills == null || source.skills.Count == 0)
            {
                return null;
            }

            List<EnemySkillDefinition> skills = new();
            foreach (EnemySkillDefinition skill in source.skills)
            {
                EnemySkillDefinition clone = CloneSkillForStage(skill, $"{levelId}_{skill.skillId}");
                if (clone == null)
                {
                    continue;
                }

                ApplySkillStageNumbers(clone, levelId);
                skills.Add(clone);
            }

            return skills;
        }

        private List<EnemySkillDefinition> BuildDisruptSkills(EnemyDefinition disruptSource, EnemyDefinition sealSource, string levelId)
        {
            List<EnemySkillDefinition> skills = new();
            AddFirstSkillOfType(skills, disruptSource, EnemySkillType.StealEnergy, levelId);
            AddFirstSkillOfType(skills, sealSource, EnemySkillType.SealRowOrColumn, levelId);
            return skills.Count > 0 ? skills : BuildStageSkills(disruptSource, levelId);
        }

        private void AddFirstSkillOfType(List<EnemySkillDefinition> target, EnemyDefinition source, EnemySkillType skillType, string levelId)
        {
            if (target == null || source?.skills == null)
            {
                return;
            }

            foreach (EnemySkillDefinition skill in source.skills)
            {
                if (skill == null || skill.skillType != skillType)
                {
                    continue;
                }

                EnemySkillDefinition clone = CloneSkillForStage(skill, $"{levelId}_{skill.skillId}");
                ApplySkillStageNumbers(clone, levelId);
                target.Add(clone);
                return;
            }
        }

        private static EnemySkillDefinition CloneSkillForStage(EnemySkillDefinition source, string skillId)
        {
            if (source == null)
            {
                return null;
            }

            EnemySkillDefinition clone = ScriptableObject.CreateInstance<EnemySkillDefinition>();
            clone.hideFlags = HideFlags.DontSave;
            clone.name = skillId;
            clone.skillId = skillId;
            clone.displayName = source.displayName;
            clone.intentText = source.intentText;
            clone.effectDescription = source.effectDescription;
            clone.skillType = source.skillType;
            clone.initialDelay = source.initialDelay;
            clone.cooldown = source.cooldown;
            clone.castTime = source.castTime;
            clone.value = source.value;
            clone.duration = source.duration;
            clone.skillTags = source.skillTags != null ? new List<CounterTag>(source.skillTags) : new List<CounterTag>();
            return clone;
        }

        private static void ApplySkillStageNumbers(EnemySkillDefinition skill, string levelId)
        {
            if (skill == null)
            {
                return;
            }

            switch (levelId)
            {
                case "1-3" when skill.skillType == EnemySkillType.GainShield:
                    skill.initialDelay = 4f;
                    skill.cooldown = 11f;
                    skill.value = 22;
                    break;
                case "1-4" when skill.skillType == EnemySkillType.GainShield:
                    skill.initialDelay = 3.5f;
                    skill.cooldown = 9f;
                    skill.value = 38;
                    break;
                case "1-6" when skill.skillType == EnemySkillType.ApplyPoison ||
                                  skill.skillType == EnemySkillType.ApplyBurn:
                    skill.initialDelay = 5f;
                    skill.cooldown = 11f;
                    skill.value = Mathf.Max(3, Mathf.RoundToInt(skill.value * 0.65f));
                    break;
                case "1-7" when skill.skillType == EnemySkillType.StealEnergy ||
                                  skill.skillType == EnemySkillType.SealRowOrColumn:
                    skill.initialDelay = 5f;
                    skill.cooldown = 12f;
                    skill.duration = Mathf.Max(2, skill.duration);
                    break;
                case "1-8" when skill.skillType == EnemySkillType.StealEnergy ||
                                  skill.skillType == EnemySkillType.SealRowOrColumn:
                    skill.initialDelay = 4f;
                    skill.cooldown = 9f;
                    skill.duration = Mathf.Max(2, skill.duration);
                    break;
                case "1-9" when skill.skillType == EnemySkillType.GainShield:
                    skill.initialDelay = 4f;
                    skill.cooldown = 12f;
                    skill.value = 26;
                    break;
            }
        }

        private EnemyDefinition FindConfiguredEnemyByLevelId(string levelId)
        {
            V02RunConfig currentConfig = GetActiveRunConfig();
            if (currentConfig?.rounds == null)
            {
                return null;
            }

            foreach (V02RoundConfig round in currentConfig.rounds)
            {
                if (round != null && string.Equals(NormalizeLevelId(round.levelId), levelId, System.StringComparison.Ordinal))
                {
                    return round.enemy;
                }
            }

            return null;
        }

        private EnemyDefinition GetNextEnemy()
        {
            if (nextEnemyOverride != null)
            {
                return nextEnemyOverride;
            }

            V02RoundConfig next = GetRound(currentRoundIndex + 1);
            return ResolveMainTrialEnemy(next);
        }

        private int FindRoundIndex(EnemyDefinition enemy)
        {
            V02RunConfig currentConfig = GetActiveRunConfig();
            if (enemy == null || currentConfig?.rounds == null)
            {
                return currentRoundIndex;
            }

            for (int i = 0; i < currentConfig.rounds.Count; i++)
            {
                if (currentConfig.rounds[i]?.enemy == enemy)
                {
                    return i;
                }
            }

            return currentRoundIndex;
        }

        private V02RoundConfig GetRound(int index)
        {
            V02RunConfig currentConfig = GetActiveRunConfig();
            if (currentConfig?.rounds != null && index >= 0 && index < currentConfig.rounds.Count)
            {
                return currentConfig.rounds[index];
            }

            if (testEnemies != null && index >= 0 && index < testEnemies.Count)
            {
                return new V02RoundConfig
                {
                    levelId = $"1-{index + 1}",
                    roundIndex = index + 1,
                    roundTitle = testEnemies[index] != null ? testEnemies[index].GetDisplayName() : $"Round {index + 1}",
                    enemy = testEnemies[index],
                    isBossRound = testEnemies[index] != null && testEnemies[index].enemyType == EnemyType.Boss,
                    targetDurationMin = 60f,
                    targetDurationMax = 120f
                };
            }

            return null;
        }

        private int GetRoundCount()
        {
            V02RunConfig currentConfig = GetActiveRunConfig();
            if (currentConfig?.rounds != null && currentConfig.rounds.Count > 0)
            {
                return currentConfig.rounds.Count;
            }

            return testEnemies != null && testEnemies.Count > 0 ? testEnemies.Count : 10;
        }

        private V02RunConfig ResolveRunConfigForRoute(MainTrialStartupRoute route)
        {
            bool roundIsChapterTwo = route != null &&
                                     !string.IsNullOrWhiteSpace(route.roundId) &&
                                     route.roundId.Trim().StartsWith("2-", System.StringComparison.OrdinalIgnoreCase);
            bool routeUsesChapterTwo = route != null &&
                                       route.routeType is MainTrialStartupRouteType.BossInfo or
                                           MainTrialStartupRouteType.BossReward or
                                           MainTrialStartupRouteType.CompleteHome;
            return roundIsChapterTwo || routeUsesChapterTwo
                ? EnsureMainTrialFlowService().GetOrCreateChapterTwoRunConfig(GetBaseRunConfig())
                : GetBaseRunConfig();
        }

        private void ExecuteStartupRoute(MainTrialStartupRoute route)
        {
            route ??= new MainTrialStartupRoute(
                MainTrialStartupRouteType.Battle,
                MainTrialPhase.NotStarted,
                "1-1",
                "Missing route fallback");

            LogStartupRoute(route);
            SetCurrentRoundFromRoute(route);

            switch (route.routeType)
            {
                case MainTrialStartupRouteType.CompleteHome:
                    OpenCoreLoopCompleteHome();
                    return;
                case MainTrialStartupRouteType.BossReward:
                    OpenChapterTwoBossRewardPanel();
                    return;
                case MainTrialStartupRouteType.ChapterReward:
                    OpenBossRewardPanel();
                    return;
                case MainTrialStartupRouteType.UpgradeRequired:
                case MainTrialStartupRouteType.Home:
                    OpenChapterOneHomeGreybox();
                    return;
                case MainTrialStartupRouteType.BossInfo:
                case MainTrialStartupRouteType.Battle:
                default:
                    EnterPrep();
                    return;
            }
        }

        private void SetCurrentRoundFromRoute(MainTrialStartupRoute route)
        {
            if (activeRunConfig?.rounds == null || activeRunConfig.rounds.Count == 0)
            {
                currentRoundIndex = 0;
                return;
            }

            int routeIndex = FindRoundIndexByLevelId(route?.roundId);
            if (routeIndex >= 0)
            {
                currentRoundIndex = routeIndex;
                return;
            }

            bool chapterTwoRoute = route != null &&
                                   !string.IsNullOrWhiteSpace(route.roundId) &&
                                   route.roundId.Trim().StartsWith("2-", System.StringComparison.OrdinalIgnoreCase);
            currentRoundIndex = chapterTwoRoute
                ? EnsureMainTrialFlowService().GetChapterTwoStartIndex(activeRunConfig)
                : 0;
        }

        private void LogStartupRoute(MainTrialStartupRoute route)
        {
            SaveData saveData = EnsureCoreLoopSaveService().EnsureLoaded();
            MainTrialProgressData progress = saveData.mainTrialProgressData ?? new MainTrialProgressData();
            progress.Normalize();

            Debug.Log($"[V0.2-StateMachine] phase={route.phase}");
            Debug.Log($"[V0.2-StateMachine] currentRoundId={progress.currentRoundId}");
            Debug.Log($"[V0.2-StateMachine] chapterTwoBossCleared={progress.chapterTwoBossCleared.ToString().ToLowerInvariant()}");
            Debug.Log($"[V0.2-StateMachine] coreLoopCompleted={progress.coreLoopCompleted.ToString().ToLowerInvariant()}");
            Debug.Log($"[V0.2-StateMachine] startupRoute={route.routeType}, reason={route.reason}");
            battleLogUI?.AddLog($"状态恢复：{route.phase} -> {route.routeType}（{route.reason}）");
        }

        private V02RunConfig GetActiveRunConfig()
        {
            return activeRunConfig != null ? activeRunConfig : GetBaseRunConfig();
        }

        private V02RunConfig GetBaseRunConfig()
        {
            return runConfig;
        }

        private int FindRoundIndexByLevelId(string levelId)
        {
            if (string.IsNullOrWhiteSpace(levelId))
            {
                return -1;
            }

            V02RunConfig currentConfig = GetActiveRunConfig();
            if (currentConfig?.rounds == null)
            {
                return -1;
            }

            for (int i = 0; i < currentConfig.rounds.Count; i++)
            {
                V02RoundConfig round = currentConfig.rounds[i];
                if (round != null && string.Equals(round.levelId, levelId, System.StringComparison.Ordinal))
                {
                    return i;
                }
            }

            return -1;
        }

        private static string GetRoundLabel(V02RoundConfig round)
        {
            if (round == null)
            {
                return "Round ?";
            }

            return string.IsNullOrWhiteSpace(round.levelId) ? $"Round {round.roundIndex}" : round.levelId.Trim();
        }

        private static void SetText(Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }
    }
}
