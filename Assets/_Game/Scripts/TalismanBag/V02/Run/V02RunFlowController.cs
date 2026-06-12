using System.Collections.Generic;
using TalismanBag.Combat;
using TalismanBag.Enemies;
using TalismanBag.UI;
using TalismanBag.V02.Feedback;
using TalismanBag.V02.Result;
using TalismanBag.V02.Rewards;
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
        [SerializeField] private BattleLogUI battleLogUI;
        [SerializeField] private Text roundInfoText;
        [SerializeField] private Text prepHintText;
        [SerializeField] private List<EnemyDefinition> testEnemies = new();

        private int currentRoundIndex;
        private EnemyDefinition nextEnemyOverride;
        private V02RunState state = V02RunState.None;

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
        }

        public void StartNewRun()
        {
            state = V02RunState.Init;
            currentRoundIndex = 0;
            nextEnemyOverride = null;
            runModifierState?.ResetState();
            failureTracker?.ResetTracker();
            runStatsTracker?.ResetStats();
            combatController?.ResetRunStats();
            runResultPanel?.Hide();
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
            battleLogUI?.AddLog($"\u6218\u524d\u51c6\u5907\uff1a{round.roundTitle}");
            if (!string.IsNullOrWhiteSpace(round.preBattleHint))
            {
                battleLogUI?.AddLog(round.preBattleHint);
            }
        }

        public void StartCombat()
        {
            state = V02RunState.Combat;
            combatController?.StartBattle();
        }

        public void OnCombatStarted()
        {
            state = V02RunState.Combat;
        }

        public void OnBattleWin()
        {
            V02RoundConfig round = CurrentRound;
            if (round == null)
            {
                FinishRunWin();
                return;
            }

            if (round.isBossRound || currentRoundIndex >= GetRoundCount() - 1)
            {
                FinishRunWin();
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

            SetText(prepHintText, $"\u5956\u52b1\u9636\u6bb5\uff1a\u4e0b\u4e00\u5173 {nextEnemy.displayName}");
            rewardController?.OpenRewardSelection(nextEnemy);
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
            combatController?.SetRunComplete();
            SetText(roundInfoText, "\u901a\u5173\u6210\u529f");
            SetText(prepHintText, "\u4f60\u5b8c\u6210\u4e86 7 \u573a\u9635\u6cd5\u5bf9\u6297\u3002");
            runResultPanel?.ShowWin(runStatsTracker);
        }

        public void FinishRunLose()
        {
            state = V02RunState.RunLose;
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
            runResultPanel?.ShowLose(CurrentRoundNumber, round != null ? round.roundTitle : string.Empty, reason, runStatsTracker);
        }

        public void RestartRun()
        {
            StartNewRun();
        }

        public void SetNextEnemy(EnemyDefinition enemy)
        {
            nextEnemyOverride = enemy;
            battleLogUI?.AddLog(enemy != null ? $"\u4e0b\u4e00\u5173\u9884\u544a\uff1a{enemy.displayName}" : "\u4e0b\u4e00\u5173\u9884\u544a\u5df2\u6e05\u7a7a");
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

        private void RefreshRoundTexts(V02RoundConfig round)
        {
            string roundLine = $"Round {round.roundIndex} / {GetRoundCount()}  {round.roundTitle}";
            string goal = string.IsNullOrWhiteSpace(round.teachingGoal) ? string.Empty : $"\n\u76ee\u6807\uff1a{round.teachingGoal}";
            SetText(roundInfoText, $"{roundLine}{goal}");
            SetText(prepHintText, round.preBattleHint);
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
                    roundIndex = index + 1,
                    roundTitle = testEnemies[index] != null ? testEnemies[index].displayName : $"Round {index + 1}",
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

            return testEnemies != null && testEnemies.Count > 0 ? testEnemies.Count : 7;
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
