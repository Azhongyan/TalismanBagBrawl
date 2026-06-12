using TalismanBag.Combat;
using TalismanBag.Enemies;
using TalismanBag.Shop;
using TalismanBag.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.Run
{
    public sealed class RunFlowController : MonoBehaviour
    {
        [SerializeField] private AutoCombatController combatController;
        [SerializeField] private ShopController shopController;
        [SerializeField] private EnemyPreviewUI enemyPreviewUI;
        [SerializeField] private Text runStatusText;
        [SerializeField] private EnemyDefinition[] roundEnemies;

        private int currentRoundIndex;
        private bool runEnded;

        public int CurrentRoundNumber => currentRoundIndex + 1;
        public int TotalRounds => roundEnemies != null ? roundEnemies.Length : 0;

        private void Start()
        {
            StartRun();
        }

        public void StartRun()
        {
            currentRoundIndex = 0;
            runEnded = false;
            shopController?.Hide();
            StartRound(currentRoundIndex);
        }

        public void StartRound(int roundIndex)
        {
            if (roundEnemies == null || roundEnemies.Length == 0)
            {
                SetStatus("缺少敌人配置");
                return;
            }

            currentRoundIndex = Mathf.Clamp(roundIndex, 0, roundEnemies.Length - 1);
            runEnded = false;
            EnemyDefinition enemy = roundEnemies[currentRoundIndex];
            enemyPreviewUI?.Refresh(enemy, CurrentRoundNumber, TotalRounds);
            combatController?.SetEnemy(enemy, CurrentRoundNumber, TotalRounds);
            SetStatus("战前整理：观察弱点后调整阵型");
        }

        public void OnBattleWin()
        {
            if (runEnded)
            {
                return;
            }

            if (currentRoundIndex >= TotalRounds - 1)
            {
                runEnded = true;
                SetStatus("15分钟验证通关：四场斗法全部胜利");
                combatController?.SetRunComplete();
                return;
            }

            SetStatus("胜利！进入商店补强");
            shopController?.ShowShop();
        }

        public void OnBattleLose()
        {
            runEnded = true;
            shopController?.Hide();
            SetStatus("本轮失败：修士气血归零");
        }

        public void ContinueAfterShop()
        {
            if (runEnded)
            {
                return;
            }

            StartRound(currentRoundIndex + 1);
        }

        public void RestartRun()
        {
            StartRun();
        }

        public void SkipToSwordCultivator()
        {
            StartRound(2);
        }

        public void SkipToEvilCultivator()
        {
            StartRound(3);
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
