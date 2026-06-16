using TalismanBag.Enemies;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.UI.Mobile
{
    public sealed class MobileRoundInfoPanel : MonoBehaviour
    {
        [SerializeField] private Text infoText;

        public void Refresh(int roundIndex, int totalRounds, EnemyDefinition enemy, string roundTitle, string hint)
        {
            if (infoText == null)
            {
                return;
            }

            string enemyName = enemy != null ? enemy.GetReadableLabel() : "未知敌人";
            string weakness = enemy != null && !string.IsNullOrWhiteSpace(enemy.weaknessText) ? enemy.weaknessText : GetDefaultWeakness(enemy);
            string danger = enemy != null && !string.IsNullOrWhiteSpace(enemy.dangerText) ? enemy.dangerText : GetDefaultDanger(enemy);
            string recommended = enemy != null && !string.IsNullOrWhiteSpace(enemy.recommendedBuildText) ? enemy.recommendedBuildText : "观察弱点后摆阵";
            infoText.text =
                $"Round {roundIndex} / {totalRounds}\n" +
                $"敌人：{enemyName}\n" +
                $"{roundTitle}\n\n" +
                $"弱点：\n{weakness}\n\n" +
                $"危险：\n{danger}\n\n" +
                $"推荐：\n{recommended}\n\n" +
                $"{hint}";
        }

        private static string GetDefaultWeakness(EnemyDefinition enemy)
        {
            if (enemy != null && enemy.enemyType == EnemyType.Boss)
            {
                return "可打断 / 怕稳定阵法 / 怕雷印爆发";
            }

            return enemy != null && enemy.enemyType == EnemyType.SwordCultivator ? "雷符打断 / 护盾 / 火剑流" : "观察弱点后摆阵";
        }

        private static string GetDefaultDanger(EnemyDefinition enemy)
        {
            if (enemy != null && enemy.enemyType == EnemyType.Boss)
            {
                return "吸灵 / 封印 / 心魔冲击 / 半血狂暴";
            }

            return enemy != null && enemy.enemyType == EnemyType.SwordCultivator ? "蓄力连斩" : "注意血量和灵气循环";
        }
    }
}
