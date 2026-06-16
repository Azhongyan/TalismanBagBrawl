using TalismanBag.Enemies;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.UI
{
    public sealed class EnemyPreviewUI : MonoBehaviour
    {
        [SerializeField] private Text roundText;
        [SerializeField] private Text enemyText;
        [SerializeField] private Text weaknessText;
        [SerializeField] private Text dangerText;

        public void Refresh(EnemyDefinition enemy, int roundIndex, int totalRounds)
        {
            if (roundText != null)
            {
                roundText.text = $"回合 {roundIndex}/{totalRounds}";
            }

            if (enemy == null)
            {
                if (enemyText != null)
                {
                    enemyText.text = "敌人：无";
                }

                if (weaknessText != null)
                {
                    weaknessText.text = "弱点未知";
                }

                if (dangerText != null)
                {
                    dangerText.text = "暂无威胁";
                }

                return;
            }

            if (enemyText != null)
            {
                enemyText.text = $"敌人：{enemy.GetReadableLabel()}";
            }

            if (weaknessText != null)
            {
                weaknessText.text = string.IsNullOrWhiteSpace(enemy.weaknessText)
                    ? GetWeaknessTags(enemy.enemyType)
                    : enemy.weaknessText;
            }

            if (dangerText != null)
            {
                string danger = string.IsNullOrWhiteSpace(enemy.dangerText)
                    ? GetDangerTags(enemy.enemyType)
                    : enemy.dangerText;
                if (!string.IsNullOrWhiteSpace(enemy.recommendedBuildText))
                {
                    danger = $"{danger}\n推荐：{enemy.recommendedBuildText}";
                }

                dangerText.text = danger;
            }
        }

        private static string GetWeaknessTags(EnemyType enemyType)
        {
            return enemyType switch
            {
                EnemyType.Ghost => "弱雷  怕驱邪",
                EnemyType.GhostSwarm => "弱雷  怕驱邪",
                EnemyType.SwordCultivator => "弱雷  可打断",
                EnemyType.EvilCultivator => "稳灵气  分散摆放",
                EnemyType.Boss => "可打断  稳定阵法  雷印爆发",
                _ => "观察弱点"
            };
        }

        private static string GetDangerTags(EnemyType enemyType)
        {
            return enemyType switch
            {
                EnemyType.Ghost => "攻频高",
                EnemyType.GhostSwarm => "鬼影攻击",
                EnemyType.SwordCultivator => "会蓄力",
                EnemyType.EvilCultivator => "会封印  吸灵气",
                EnemyType.Boss => "吸灵  封印  心魔冲击  半血狂暴",
                _ => "危险未知"
            };
        }
    }
}
