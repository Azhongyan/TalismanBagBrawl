using TalismanBag.Combo;
using TalismanBag.Combat;

namespace TalismanBag.Progression
{
    public static class BuildArchetypeResolver
    {
        public static string Resolve(BattleStatsSnapshot stats)
        {
            if (stats.mostActiveComboId == ComboResolver.FireSword)
            {
                return "火剑流";
            }

            if (stats.mostActiveComboId == ComboResolver.ThunderSeal)
            {
                return "雷印爆发流";
            }

            if (stats.mostActiveComboId == ComboResolver.ExorcismArray)
            {
                return "驱邪阵";
            }

            if (stats.mostActiveComboId == ComboResolver.ShieldPill || stats.totalShieldGained + stats.totalHealing > stats.totalDamageDealt)
            {
                return "护丹续航流";
            }

            if (stats.mostActiveComboId == ComboResolver.FireSpirit)
            {
                return "火灵连发流";
            }

            return "散修乱斗";
        }

        public static string Describe(string archetype)
        {
            return archetype switch
            {
                "火灵连发流" => "依靠聚灵石加速火符，持续稳定输出。",
                "火剑流" => "小剑丸附带火焰伤害，对剑修表现较好。",
                "雷印爆发流" => "依靠雷符暴击和打断压制强敌。",
                "驱邪阵" => "专门压制鬼怪类敌人，但对剑修较弱。",
                "护丹续航流" => "靠护盾和丹药撑过高压战斗。",
                _ => "没有形成稳定阵法循环。"
            };
        }
    }
}
