using TalismanBag.Combat;
using TalismanBag.Debugging;
using TalismanBag.Economy;
using TalismanBag.Enemies;
using TalismanBag.Progression;
using TalismanBag.Shop;
using TalismanBag.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.Run
{
    public enum RunState
    {
        None,
        Init,
        Prep,
        Combat,
        RoundWin,
        RoundLose,
        Reward,
        Shop,
        BagUpgrade,
        Boss,
        RunWin,
        RunLose,
        Win,
        Lose
    }

    public sealed class RunFlowControllerV2 : MonoBehaviour
    {
        [SerializeField] private AutoCombatController combatController;
        [SerializeField] private ShopControllerV2 shopController;
        [SerializeField] private EnemyPreviewUI enemyPreviewUI;
        [SerializeField] private TutorialHintController tutorialHintController;
        [SerializeField] private SpiritJadeWallet wallet;
        [SerializeField] private BagExpansionController bagExpansionController;
        [SerializeField] private Text runStatusText;
        public RunConfig runConfig;

        public int currentRoundIndex;
        public RunState currentState = RunState.Init;
        private bool runEnded;

        public int CurrentRoundNumber => currentRoundIndex + 1;
        public int TotalRounds => runConfig != null && runConfig.rounds != null ? runConfig.rounds.Count : 0;
        public RunState State => currentState;

        private void Start()
        {
            StartNewRun();
        }

        public void StartNewRun()
        {
            currentRoundIndex = 0;
            runEnded = false;
            currentState = RunState.Init;
            wallet?.ResetWallet(runConfig != null ? runConfig.startingSpiritJade : 0);
            combatController?.ResetRunStats();
            shopController?.Hide();
            shopController?.ResetInventoryViews(runConfig != null ? runConfig.startingItems : null);
            bagExpansionController?.LockEnhancedSlots();
            PlaytestSessionLogger.Log("Run Started");
            EnterPrep();
        }

        public void EnterPrep()
        {
            if (!TryGetRound(currentRoundIndex, out RoundConfig round))
            {
                SetStatus("缺少回合配置");
                return;
            }

            currentState = round.enemy != null && (round.enemy.enemyType == EnemyType.Boss || round.isBossRound) ? RunState.Boss : RunState.Prep;
            enemyPreviewUI?.Refresh(round.enemy, CurrentRoundNumber, TotalRounds);
            tutorialHintController?.ShowRoundHint(CurrentRoundNumber, round.enemy, round.roundHint);
            combatController?.SetEnemy(round.enemy, CurrentRoundNumber, TotalRounds);
            SetStatus($"{round.roundTitle}：战前整理");
            if (round.enemy != null && round.enemy.enemyType == EnemyType.Boss)
            {
                PlaytestSessionLogger.Log($"Boss Started: {round.enemy.displayName}");
            }
        }

        public void StartCombat()
        {
            currentState = RunState.Combat;
            combatController?.StartBattle();
        }

        public void OnBattleWin()
        {
            if (runEnded || !TryGetRound(currentRoundIndex, out RoundConfig round))
            {
                return;
            }

            PlaytestSessionLogger.Log($"Round {CurrentRoundNumber} Won: {round.enemy?.displayName}");

            if (currentRoundIndex >= TotalRounds - 1)
            {
                FinishRunWin();
                return;
            }

            wallet?.AddJade(round.rewardSpiritJade);
            currentState = RunState.RoundWin;
            SetStatus($"胜利：获得 {round.rewardSpiritJade} 灵玉");

            if (round.unlockBagExpansion)
            {
                currentState = RunState.BagUpgrade;
                bagExpansionController?.UnlockEnhancedSlots();
                SetStatus("符箓袋阵眼觉醒：解锁 2 个强化阵位，主动道具冷却减少 15%");
                PlaytestSessionLogger.Log("Power Slots Unlocked");
            }

            EnterShop();
        }

        public void OnBattleLose()
        {
            PlaytestSessionLogger.Log($"Round {CurrentRoundNumber} Lost");
            currentState = RunState.RoundLose;
            FinishRunLose();
        }

        public void EnterShop()
        {
            currentState = RunState.Shop;
            TryGetRound(currentRoundIndex, out RoundConfig completedRound);
            TryGetRound(currentRoundIndex + 1, out RoundConfig nextRound);
            shopController?.OpenShop(completedRound, nextRound);
        }

        public void ContinueToNextRound()
        {
            if (runEnded)
            {
                return;
            }

            currentRoundIndex = Mathf.Clamp(currentRoundIndex + 1, 0, Mathf.Max(0, TotalRounds - 1));
            shopController?.Hide();
            EnterPrep();
        }

        public void FinishRunWin()
        {
            runEnded = true;
            currentState = RunState.RunWin;
            shopController?.Hide();
            combatController?.SetRunComplete();
            SetStatus("通关成功：15分钟验证完成");
            PlaytestSessionLogger.Log("Run Won");
        }

        public void FinishRunLose()
        {
            runEnded = true;
            currentState = RunState.RunLose;
            shopController?.Hide();
            SetStatus("修士败北：本局结束");
            PlaytestSessionLogger.Log("Run Lost: Reason = Combat Defeat");
        }

        public void RestartRun()
        {
            StartNewRun();
        }

        public void SkipToRound(int roundNumber)
        {
            currentRoundIndex = Mathf.Clamp(roundNumber - 1, 0, Mathf.Max(0, TotalRounds - 1));
            shopController?.Hide();
            EnterPrep();
        }

        public void SkipToSwordCultivator()
        {
            SkipToRound(3);
        }

        public void SkipToEvilCultivator()
        {
            SkipToRound(4);
        }

        private bool TryGetRound(int index, out RoundConfig round)
        {
            round = null;
            if (runConfig == null || runConfig.rounds == null || runConfig.rounds.Count == 0)
            {
                return false;
            }

            int safeIndex = Mathf.Clamp(index, 0, runConfig.rounds.Count - 1);
            round = runConfig.rounds[safeIndex];
            return round != null;
        }

        private void SetStatus(string message)
        {
            if (runStatusText != null)
            {
                runStatusText.text = message;
            }
        }
    }
}
