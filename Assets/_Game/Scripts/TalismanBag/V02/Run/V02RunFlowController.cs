using System.Collections.Generic;
using TalismanBag.Combat;
using TalismanBag.Enemies;
using TalismanBag.UI;
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
        [SerializeField] private BattleLogUI battleLogUI;
        [SerializeField] private Text roundInfoText;
        [SerializeField] private Text prepHintText;
        [SerializeField] private GameObject formationInfoPanel;
        [SerializeField] private Button formationInfoCloseButton;
        [SerializeField] private GameObject tagTooltipPanel;
        [SerializeField] private Button tagTooltipCloseButton;
        [SerializeField] private List<EnemyDefinition> testEnemies = new();

        private int currentRoundIndex;
        private EnemyDefinition nextEnemyOverride;
        private V02RunState state = V02RunState.None;
        private CanvasGroup formationInfoCanvasGroup;
        private CanvasGroup tagTooltipCanvasGroup;
        private bool preBattlePopupButtonsBound;
        private bool bossRewardClaimed;
        private readonly HashSet<int> shownFormationInfoRoundIndexes = new();

        public int CurrentRoundNumber => CurrentRound != null ? CurrentRound.roundIndex : currentRoundIndex + 1;
        public V02RunState State => state;
        public V02RoundConfig CurrentRound => GetRound(currentRoundIndex);

        private void OnEnable()
        {
            if (rewardController != null)
            {
                rewardController.RewardChosen += OnRewardSelected;
            }

            if (runResultPanel != null)
            {
                runResultPanel.RestartRequested += RestartRun;
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
                runResultPanel.RestartRequested -= RestartRun;
            }

            UnbindPreBattlePopupButtons();
        }

        public void StartNewRun()
        {
            state = V02RunState.Init;
            currentRoundIndex = 0;
            nextEnemyOverride = null;
            shownFormationInfoRoundIndexes.Clear();
            runModifierState?.ResetState();
            failureTracker?.ResetTracker();
            runStatsTracker?.ResetStats();
            combatController?.ResetRunStats();
            rewardController?.StartNewRewardRun();
            runResultPanel?.Hide();
            bossRewardPanel?.Hide();
            bossRewardClaimed = false;
            EnterPrep();
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
            combatController?.SetEnemy(round.enemy, round.roundIndex, GetRoundCount());
            RefreshRoundTexts(round);
            ShowPreBattlePopups();
            battleLogUI?.AddLog($"\u6218\u524d\u51c6\u5907\uff1a{round.roundTitle}");
            if (!string.IsNullOrWhiteSpace(round.preBattleHint))
            {
                battleLogUI?.AddLog(round.preBattleHint);
            }
        }

        public void StartCombat()
        {
            state = V02RunState.Combat;
            HidePreBattlePopups();
            combatController?.StartBattle();
        }

        public void OnCombatStarted()
        {
            state = V02RunState.Combat;
            HidePreBattlePopups();
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
                    OpenBossRewardPanel();
                }
                else
                {
                    FinishRunWin();
                }

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

        public void FinishRunWin()
        {
            state = V02RunState.RunWin;
            HidePreBattlePopups();
            bossRewardPanel?.Hide();
            combatController?.SetRunComplete();
            SetText(roundInfoText, "\u901a\u5173\u6210\u529f");
            SetText(prepHintText, $"\u4f60\u5b8c\u6210\u4e86 {GetRoundCount()} \u573a\u4e3b\u7ebf\u8bd5\u70bc\u3002");
            runResultPanel?.ShowWin(runStatsTracker, GetRoundCount());
        }

        public void FinishRunLose()
        {
            state = V02RunState.RunLose;
            HidePreBattlePopups();
            bossRewardPanel?.Hide();
            V02RoundConfig round = CurrentRound;
            if (failureTracker != null)
            {
                failureTracker.roundLostIndex = round != null ? round.roundIndex : CurrentRoundNumber;
            }

            V02FailureReasonResult reason = failureReasonResolver != null
                ? failureReasonResolver.ResolveFailureReason(failureTracker, this)
                : new V02FailureReasonResult(V02FailureReason.LowDefense, "\u9632\u5fa1\u4e0d\u8db3", "\u9635\u6cd5\u627f\u538b\u4e0d\u8db3\u3002", "\u8c03\u6574\u4f9b\u80fd\u548c\u9632\u5fa1\u7b26\u7b93\u4f4d\u7f6e\u3002");

            SetText(roundInfoText, "\u4fee\u58eb\u8d25\u5317");
            SetText(prepHintText, reason.title);
            runResultPanel?.ShowLose(CurrentRoundNumber, round != null ? round.roundTitle : string.Empty, reason, runStatsTracker, GetRoundCount());
        }

        public void RestartRun()
        {
            StartNewRun();
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

        private void OpenBossRewardPanel()
        {
            state = V02RunState.Reward;
            HidePreBattlePopups();
            SetText(roundInfoText, "1-10 Boss \u5df2\u51fb\u7834");
            SetText(prepHintText, "\u9886\u53d6 Boss \u8f7b\u91cf\u5956\u52b1\u540e\u5b8c\u6210 V0.2 \u4e3b\u7ebf\u8bd5\u70bc\u3002");

            V02BossRewardPanel panel = EnsureBossRewardPanel();
            if (panel == null)
            {
                ConfirmBossReward();
                return;
            }

            panel.Show(BuildBossRewardLines(), ConfirmBossReward);
        }

        private void ConfirmBossReward()
        {
            if (!bossRewardClaimed)
            {
                rewardController?.GrantBossCompletionReward();
                bossRewardClaimed = true;
            }

            FinishRunWin();
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

        private static string[] BuildBossRewardLines()
        {
            return new[]
            {
                "\u91cd\u590d\u706b\u7b26 x1",
                "\u7b26\u7eb8 x3\uff08V0.2 \u5360\u4f4d\uff09",
                "\u7075\u77f3 x20\uff08V0.2 \u5360\u4f4d\uff09",
                "\u57fa\u7840\u914d\u65b9\u6b8b\u9875 x1\uff08V0.2 \u5360\u4f4d\uff09",
                "\u5c11\u91cf\u4fee\u4e3a\uff08V0.2 \u5360\u4f4d\uff09"
            };
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
            if (enemy != null && runConfig?.rounds != null)
            {
                for (int i = 0; i < runConfig.rounds.Count; i++)
                {
                    if (runConfig.rounds[i]?.enemy == enemy)
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

        private void RefreshRoundTexts(V02RoundConfig round)
        {
            string enemyLabel = round.enemy != null ? round.enemy.GetReadableLabel() : "\u654c\u4eba\u672a\u914d\u7f6e";
            string title = string.IsNullOrWhiteSpace(round.roundTitle) ? string.Empty : $"  {round.roundTitle}";
            string roundLine = $"{GetRoundLabel(round)} / {GetRoundCount()}  {enemyLabel}{title}";
            string goal = string.IsNullOrWhiteSpace(round.teachingGoal) ? string.Empty : $"\n\u76ee\u6807\uff1a{round.teachingGoal}";
            SetText(roundInfoText, $"{roundLine}{goal}");
            SetText(prepHintText, BuildPreBattleHint(round));
        }

        private static string BuildPreBattleHint(V02RoundConfig round)
        {
            if (round == null)
            {
                return string.Empty;
            }

            string enemyLabel = round.enemy != null ? round.enemy.GetReadableLabel() : "\u654c\u4eba\u672a\u914d\u7f6e";
            string title = string.IsNullOrWhiteSpace(round.roundTitle) ? string.Empty : $"  {round.roundTitle.Trim()}";
            string hint = string.IsNullOrWhiteSpace(round.preBattleHint)
                ? "\u89c2\u5bdf\u654c\u4eba\u5a01\u80c1\uff0c\u5f00\u6218\u524d\u8c03\u6574\u9635\u76d8\u3002"
                : round.preBattleHint.Trim();

            return $"{enemyLabel}{title}\n{hint}";
        }

        private EnemyDefinition GetNextEnemy()
        {
            if (nextEnemyOverride != null)
            {
                return nextEnemyOverride;
            }

            V02RoundConfig next = GetRound(currentRoundIndex + 1);
            return next != null ? next.enemy : null;
        }

        private int FindRoundIndex(EnemyDefinition enemy)
        {
            if (enemy == null || runConfig?.rounds == null)
            {
                return currentRoundIndex;
            }

            for (int i = 0; i < runConfig.rounds.Count; i++)
            {
                if (runConfig.rounds[i]?.enemy == enemy)
                {
                    return i;
                }
            }

            return currentRoundIndex;
        }

        private V02RoundConfig GetRound(int index)
        {
            if (runConfig?.rounds != null && index >= 0 && index < runConfig.rounds.Count)
            {
                return runConfig.rounds[index];
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
            if (runConfig?.rounds != null && runConfig.rounds.Count > 0)
            {
                return runConfig.rounds.Count;
            }

            return testEnemies != null && testEnemies.Count > 0 ? testEnemies.Count : 10;
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
