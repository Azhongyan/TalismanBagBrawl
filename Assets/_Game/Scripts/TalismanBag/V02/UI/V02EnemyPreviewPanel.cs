using System.Text;
using TalismanBag.Enemies;
using TalismanBag.V02.EnemySkills;
using TalismanBag.V02.Tags;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.V02.UI
{
    public sealed class V02EnemyPreviewPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;

        public void Show(EnemyDefinition enemy)
        {
            if (panel != null)
            {
                panel.SetActive(enemy != null);
            }

            if (enemy == null)
            {
                SetText(titleText, string.Empty);
                SetText(bodyText, string.Empty);
                return;
            }

            SetText(titleText, $"敌人：{enemy.displayName}");
            SetText(bodyText, BuildBody(enemy));
        }

        private static string BuildBody(EnemyDefinition enemy)
        {
            StringBuilder builder = new();
            builder.AppendLine($"职业：{Fallback(enemy.enemyClass, "未知")}");
            builder.AppendLine($"流派：{Fallback(enemy.enemyArchetype, "未知")}");
            builder.AppendLine();
            builder.AppendLine("技能：");
            builder.AppendLine(BuildSkillLines(enemy));
            builder.AppendLine($"弱点：{TalismanTagUtility.JoinCounterTags(enemy.weaknessTags)}");
            builder.AppendLine($"建议：{Fallback(enemy.recommendedCounterText, enemy.recommendedBuildText)}");
            if (!string.IsNullOrWhiteSpace(enemy.intentText))
            {
                builder.AppendLine($"默认意图：{enemy.intentText}");
            }

            return builder.ToString();
        }

        private static string BuildSkillLines(EnemyDefinition enemy)
        {
            if (enemy.skills == null || enemy.skills.Count == 0)
            {
                return "无特殊技能";
            }

            StringBuilder builder = new();
            foreach (EnemySkillDefinition skill in enemy.skills)
            {
                if (skill == null)
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.AppendLine();
                }

                builder.Append($"- {skill.displayName}");
                if (!string.IsNullOrWhiteSpace(skill.effectDescription))
                {
                    builder.Append($"：{skill.effectDescription}");
                }
            }

            return builder.Length > 0 ? builder.ToString() : "无特殊技能";
        }

        private static string Fallback(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
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
