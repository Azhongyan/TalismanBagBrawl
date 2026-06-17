using TalismanBag.Enemies;

namespace TalismanBag.V02.CoreLoop.Boss
{
    public sealed class BossInfoViewModel
    {
        public string bossName;
        public string mechanismTags;
        public string mainThreats;
        public string recommendedTools;
        public string preBattlePrompt;

        public static BossInfoViewModel From(BossInfoConfig config, EnemyDefinition enemy)
        {
            return new BossInfoViewModel
            {
                bossName = FirstText(config?.bossName, enemy != null ? enemy.GetReadableLabel() : null, "2-10 Boss"),
                mechanismTags = FirstText(config?.mechanismTags, enemy?.intentText, "护盾压制 / 妖群召唤 / 阵眼封印"),
                mainThreats = FirstText(config?.mainThreats, enemy?.dangerText, "Boss 会轮流施加多段压力，需要稳定供能和防御。"),
                recommendedTools = FirstText(config?.recommendedTools, enemy?.recommendedBuildText, enemy?.recommendedCounterText, "推荐携带雷符、净化符、镇魂符、护身符。"),
                preBattlePrompt = FirstText(config?.preBattlePrompt, "前方煞气聚阵，请先查看敌情并整备背包。")
            };
        }

        private static string FirstText(params string[] values)
        {
            if (values == null)
            {
                return string.Empty;
            }

            for (int i = 0; i < values.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(values[i]))
                {
                    return values[i].Trim();
                }
            }

            return string.Empty;
        }
    }
}
