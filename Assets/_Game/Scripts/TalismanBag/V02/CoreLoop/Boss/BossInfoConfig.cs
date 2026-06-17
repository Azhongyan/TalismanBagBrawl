using UnityEngine;

namespace TalismanBag.V02.CoreLoop.Boss
{
    [CreateAssetMenu(menuName = "Talisman Bag/V02/CoreLoop/Boss Info Config", fileName = "BossInfoConfig")]
    public sealed class BossInfoConfig : ScriptableObject
    {
        public string bossId = "formation_breaker_boss";
        public string bossName = "2-10 Boss：煞气聚阵";

        [TextArea] public string mechanismTags = "护盾压制 / 妖群召唤 / 阵眼封印";
        [TextArea] public string mainThreats = "Boss 会轮流施加护盾压力、妖群压力和阵眼封印压力。";
        [TextArea] public string recommendedTools = "推荐携带雷符、净化符、镇魂符、护身符，并保持核心符箓供能。";
        [TextArea] public string preBattlePrompt = "前方煞气聚阵，请先查看敌情并整备背包。";
    }
}
