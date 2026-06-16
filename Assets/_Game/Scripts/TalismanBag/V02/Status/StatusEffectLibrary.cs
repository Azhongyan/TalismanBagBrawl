using System.Collections.Generic;
using UnityEngine;

namespace TalismanBag.V02.Status
{
    public static class StatusEffectIds
    {
        public const string Poison = "poison";
        public const string Burn = "burn";
        public const string Shield = "shield";
        public const string Seal = "seal";
        public const string EnergyDisrupt = "energy_disrupt";
        public const string CleanseReady = "cleanse_ready";
        public const string SoulSuppressReady = "soul_suppress_ready";
        public const string SoulSuppressTriggered = "soul_suppress_triggered";
        public const string EnemyIntent = "enemy_intent";
        public const string BossPhase = "boss_phase";
        public const string StatusDamage = "status_damage";
    }

    public static class StatusEffectLibrary
    {
        private static readonly Dictionary<string, StatusEffectDefinition> Cache = new();

        public static StatusEffectDefinition Poison => Get(StatusEffectIds.Poison, "中毒", "毒", "每秒受到持续伤害，可被净化符清除。", StatusEffectType.DamageOverTime, StatusPolarity.Debuff, StatusDisplayPriority.High, true, 99, false, 0f, new Color(0.52f, 0.95f, 0.42f));
        public static StatusEffectDefinition Burn => Get(StatusEffectIds.Burn, "燃烧", "火", "每秒受到持续伤害，可被净化符清除。", StatusEffectType.DamageOverTime, StatusPolarity.Debuff, StatusDisplayPriority.High, true, 99, false, 0f, new Color(1f, 0.46f, 0.22f));
        public static StatusEffectDefinition Shield => Get(StatusEffectIds.Shield, "护盾", "盾", "优先抵挡受到的伤害。", StatusEffectType.Shield, StatusPolarity.Buff, StatusDisplayPriority.Medium, true, 999, false, 0f, new Color(0.35f, 0.8f, 1f), false);
        public static StatusEffectDefinition Seal => Get(StatusEffectIds.Seal, "封印", "封", "部分符箓暂时无法触发，可被净化符解除。", StatusEffectType.Seal, StatusPolarity.Debuff, StatusDisplayPriority.Critical, true, 25, true, 3f, new Color(0.82f, 0.82f, 0.86f));
        public static StatusEffectDefinition EnergyDisrupt => Get(StatusEffectIds.EnergyDisrupt, "供能中断", "灵", "阵法供能被干扰，部分符箓暂时失效。", StatusEffectType.EnergyDisrupt, StatusPolarity.Debuff, StatusDisplayPriority.Critical, true, 25, true, 3f, new Color(0.62f, 0.72f, 1f), false);
        public static StatusEffectDefinition CleanseReady => Get(StatusEffectIds.CleanseReady, "净化待命", "净", "净化符已在阵中，可解除中毒、燃烧或封印。", StatusEffectType.CleanseReady, StatusPolarity.Passive, StatusDisplayPriority.Low, false, 1, false, 0f, new Color(0.68f, 1f, 0.82f), false);
        public static StatusEffectDefinition SoulSuppressReady => Get(StatusEffectIds.SoulSuppressReady, "镇魂待命", "魂", "镇魂符已在阵中，可反制偷灵类威胁。", StatusEffectType.CounterReady, StatusPolarity.Passive, StatusDisplayPriority.Low, false, 1, false, 0f, new Color(0.85f, 0.72f, 1f), false);
        public static StatusEffectDefinition SoulSuppressTriggered => Get(StatusEffectIds.SoulSuppressTriggered, "镇魂反制", "魂", "镇魂符刚刚反制了偷灵。", StatusEffectType.PassiveTriggered, StatusPolarity.Passive, StatusDisplayPriority.High, false, 1, true, 1.5f, new Color(0.9f, 0.78f, 1f), false);
        public static StatusEffectDefinition EnemyIntent => Get(StatusEffectIds.EnemyIntent, "技能蓄力", "蓄", "敌人正在准备技能。", StatusEffectType.EnemyIntent, StatusPolarity.Intent, StatusDisplayPriority.Critical, false, 1, true, 1f, new Color(1f, 0.72f, 0.36f), false);
        public static StatusEffectDefinition BossPhase => Get(StatusEffectIds.BossPhase, "Boss阶段", "阶", "Boss进入了新的战斗阶段。", StatusEffectType.BossPhase, StatusPolarity.Intent, StatusDisplayPriority.High, false, 1, false, 0f, new Color(1f, 0.62f, 0.88f), false);

        private static StatusEffectDefinition Get(
            string id,
            string displayName,
            string glyph,
            string description,
            StatusEffectType type,
            StatusPolarity polarity,
            StatusDisplayPriority priority,
            bool stackable,
            int maxStacks,
            bool hasDuration,
            float duration,
            Color color,
            bool dispellable = true)
        {
            if (Cache.TryGetValue(id, out StatusEffectDefinition definition))
            {
                return definition;
            }

            definition = ScriptableObject.CreateInstance<StatusEffectDefinition>();
            definition.hideFlags = HideFlags.HideAndDontSave;
            definition.statusId = id;
            definition.displayName = displayName;
            definition.glyph = glyph;
            definition.description = description;
            definition.statusType = type;
            definition.polarity = polarity;
            definition.displayPriority = priority;
            definition.stackable = stackable;
            definition.maxStacks = maxStacks;
            definition.hasDuration = hasDuration;
            definition.defaultDuration = duration;
            definition.displayColor = color;
            definition.isDispellable = dispellable;
            Cache[id] = definition;
            return definition;
        }
    }
}
