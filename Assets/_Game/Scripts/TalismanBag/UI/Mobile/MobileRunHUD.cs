using TalismanBag.Combat;
using TalismanBag.Enemies;
using TalismanBag.Run;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.UI.Mobile
{
    public sealed class MobileRunHUD : MonoBehaviour
    {
        [SerializeField] private Text roundText;
        [SerializeField] private Text hpText;
        [SerializeField] private Text shieldText;
        [SerializeField] private Text manaText;
        [SerializeField] private Text stateText;
        [SerializeField] private Text enemyNameText;
        [SerializeField] private Text enemyHpText;
        [SerializeField] private Image enemyHpFill;
        [SerializeField] private Text weaknessText;
        [SerializeField] private Text dangerText;

        public void RefreshRunState(RunFlowControllerV2 run)
        {
            if (run == null)
            {
                return;
            }

            if (roundText != null)
            {
                roundText.text = $"回合：{run.CurrentRoundNumber}/{run.TotalRounds}";
            }

            SetStateText(GetStateLabel(run.State));
        }

        public void RefreshPlayerStats(CombatStats playerStats)
        {
            if (playerStats == null)
            {
                return;
            }

            if (hpText != null)
            {
                hpText.text = $"气血：{playerStats.hp}/{playerStats.maxHP}";
            }

            if (shieldText != null)
            {
                shieldText.text = $"护盾：{playerStats.shield}";
            }

            if (manaText != null)
            {
                manaText.text = $"灵气：{playerStats.mana}/{playerStats.maxMana}";
            }
        }

        public void RefreshEnemy(EnemyRuntime enemyRuntime)
        {
            EnemyDefinition enemy = enemyRuntime?.definition;
            if (enemy == null)
            {
                SetEnemyText("敌人：无", "敌人血量：-", 0f, "弱点：未知", "危险：暂无");
                return;
            }

            float hpPercent = enemy.maxHp > 0 ? Mathf.Clamp01(enemyRuntime.currentHp / (float)enemy.maxHp) : 0f;
            SetEnemyText(
                enemy.enemyType == EnemyType.Boss && enemyRuntime.isEnraged ? $"Boss：{enemy.GetReadableLabel()}  狂暴" : $"敌人：{enemy.GetReadableLabel()}",
                $"敌人血量：{enemyRuntime.currentHp}/{enemy.maxHp}",
                hpPercent,
                $"弱点：{GetWeaknessTags(enemy)}",
                $"危险：{GetDangerTags(enemy)}{GetBossRuntimeSuffix(enemyRuntime)}{GetRecommendedSuffix(enemy)}");
        }

        public void SetStateText(string text)
        {
            if (stateText != null)
            {
                stateText.text = $"状态：{text}";
            }
        }

        private void SetEnemyText(string name, string hp, float hpPercent, string weakness, string danger)
        {
            if (enemyNameText != null)
            {
                enemyNameText.text = name;
            }

            if (enemyHpText != null)
            {
                enemyHpText.text = hp;
            }

            if (enemyHpFill != null)
            {
                enemyHpFill.fillAmount = hpPercent;
            }

            if (weaknessText != null)
            {
                weaknessText.text = weakness;
            }

            if (dangerText != null)
            {
                dangerText.text = danger;
            }
        }

        private static string GetStateLabel(RunState state)
        {
            return state switch
            {
                RunState.Prep or RunState.Boss => "战前整理",
                RunState.Combat => "自动斗法",
                RunState.RoundWin or RunState.Reward or RunState.Shop or RunState.BagUpgrade => "胜利",
                RunState.RoundLose => "失败",
                RunState.RunWin or RunState.Win => "通关",
                RunState.RunLose or RunState.Lose => "败北",
                _ => "准备中"
            };
        }

        private static string GetWeaknessTags(EnemyDefinition enemy)
        {
            if (!string.IsNullOrWhiteSpace(enemy.weaknessText))
            {
                return enemy.weaknessText;
            }

            return enemy.enemyType switch
            {
                EnemyType.Ghost => "弱雷 / 怕驱邪",
                EnemyType.GhostSwarm => "弱雷 / 怕驱邪",
                EnemyType.SwordCultivator => "雷符打断 / 护盾",
                EnemyType.EvilCultivator => "稳灵气 / 分散摆放",
                EnemyType.Boss => "可打断 / 稳定阵法 / 雷印爆发",
                _ => "观察弱点"
            };
        }

        private static string GetDangerTags(EnemyDefinition enemy)
        {
            if (!string.IsNullOrWhiteSpace(enemy.dangerText))
            {
                return enemy.dangerText;
            }

            return enemy.enemyType switch
            {
                EnemyType.Ghost => "攻频高",
                EnemyType.GhostSwarm => "鬼影攻击",
                EnemyType.SwordCultivator => "蓄力连斩",
                EnemyType.EvilCultivator => "吸灵 / 封印",
                EnemyType.Boss => "吸灵 / 封印 / 心魔冲击 / 半血狂暴",
                _ => "危险未知"
            };
        }

        private static string GetRecommendedSuffix(EnemyDefinition enemy)
        {
            return string.IsNullOrWhiteSpace(enemy.recommendedBuildText)
                ? string.Empty
                : $"\n推荐：{enemy.recommendedBuildText}";
        }

        private static string GetBossRuntimeSuffix(EnemyRuntime enemyRuntime)
        {
            if (enemyRuntime?.definition == null || enemyRuntime.definition.enemyType != EnemyType.Boss)
            {
                return string.Empty;
            }

            if (enemyRuntime.isCharging)
            {
                return "\n心魔冲击蓄力中";
            }

            return enemyRuntime.isEnraged ? "\nBoss 狂暴" : string.Empty;
        }
    }
}
