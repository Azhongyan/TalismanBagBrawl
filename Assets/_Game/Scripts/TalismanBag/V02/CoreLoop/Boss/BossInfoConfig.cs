using TalismanBag.Enemies;
using TalismanBag.V02.Config;
using UnityEngine;

namespace TalismanBag.V02.CoreLoop.Boss
{
    [CreateAssetMenu(menuName = "Talisman Bag/V02/CoreLoop/Boss Info Config", fileName = "BossInfoConfig")]
    public sealed class BossInfoConfig : ScriptableObject
    {
        [Header("Identity")]
        public string bossId = "formation_breaker_boss";
        public string bossName = "2-10 Boss：煞气聚阵";
        public EnemyDefinition bossEnemy;

        [Header("Boss Briefing")]
        [TextArea] public string mechanismTags = "护盾压制 / 妖群召唤 / 阵眼封印";
        [TextArea] public string mainThreats = "Boss 会轮流施加护盾压力、妖群压力和阵眼封印压力。";
        [TextArea] public string recommendedTools = "推荐携带雷符、净化符、镇魂符、护身符，并保持核心符箓供能。";
        [TextArea] public string preBattlePrompt = "前方煞气聚阵，请先查看敌情并整备背包。";

        [Header("Phase Thresholds")]
        [Range(0f, 1f)] public float shieldPhaseMinHpRatio = 0.7f;
        [Range(0f, 1f)] public float summonPhaseMinHpRatio = 0.35f;

        [Header("Phase Timing")]
        [Min(0.01f)] public float firstActionDelay = 4f;
        [Min(0.01f)] public float shieldInterval = 5f;
        [Min(0.01f)] public float summonInterval = 6f;
        [Min(0.01f)] public float sealEyeInterval = 7f;

        [Header("Phase Effects")]
        [Min(0)] public int shieldAmount = 30;
        [Min(0)] public int summonDamage = 12;
        [Min(0)] public int poisonStack = 1;
        [Min(0.01f)] public float sealDuration = 3f;
        [Min(0.01f)] public float energyDisruptionDuration = 3f;

        [Header("Data Catalog")]
        public CatalogSourceType sourceType = CatalogSourceType.Unknown;
        public bool isDebugOnly;
        public bool isDeprecated;

        public string GetCatalogLabel()
        {
            string readableName = string.IsNullOrWhiteSpace(bossName) ? name : bossName.Trim();
            string readableId = string.IsNullOrWhiteSpace(bossId) ? "no_id" : bossId.Trim();
            return $"{readableName} [{readableId}]";
        }
    }
}
