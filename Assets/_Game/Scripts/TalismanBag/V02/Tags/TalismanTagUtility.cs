using System;
using System.Collections.Generic;
using System.Text;
using TalismanBag.Items;

namespace TalismanBag.V02.Tags
{
    public static class TalismanTagUtility
    {
        public static bool HasFunctionTag(TalismanItemDefinition item, FunctionTag tag)
        {
            return item != null && item.functionTags != null && item.functionTags.Contains(tag);
        }

        public static bool HasCounterTag(TalismanItemDefinition item, CounterTag tag)
        {
            return item != null && item.counterTags != null && item.counterTags.Contains(tag);
        }

        public static bool HasElementTag(TalismanItemDefinition item, ElementTag tag)
        {
            return item != null && item.elementTag == tag;
        }

        public static bool CanCounter(TalismanItemDefinition item, CounterTag enemyTag)
        {
            return HasCounterTag(item, enemyTag);
        }

        public static string JoinFunctionTags(TalismanItemDefinition item)
        {
            return JoinTags(item != null ? item.functionTags : null, GetFunctionTagName);
        }

        public static string JoinCounterTags(TalismanItemDefinition item)
        {
            return JoinTags(item != null ? item.counterTags : null, GetCounterTagName);
        }

        public static string JoinCounterTags(IReadOnlyCollection<CounterTag> tags)
        {
            return JoinTags(tags, GetCounterTagName);
        }

        public static string GetElementTagName(ElementTag tag)
        {
            return tag switch
            {
                ElementTag.Fire => "火",
                ElementTag.Thunder => "雷",
                ElementTag.Water => "水",
                ElementTag.Wood => "木",
                ElementTag.Metal => "金",
                ElementTag.Earth => "土",
                ElementTag.Sword => "剑",
                ElementTag.Soul => "魂",
                _ => "无"
            };
        }

        public static string GetFunctionTagName(FunctionTag tag)
        {
            return tag switch
            {
                FunctionTag.Damage => "伤害",
                FunctionTag.Burst => "爆发",
                FunctionTag.Burn => "灼烧",
                FunctionTag.Chain => "连锁",
                FunctionTag.AoE => "群攻",
                FunctionTag.Shield => "护盾",
                FunctionTag.Heal => "治疗",
                FunctionTag.Cleanse => "净化",
                FunctionTag.Defense => "防御",
                FunctionTag.EnergySource => "供能",
                FunctionTag.Enhance => "强化",
                FunctionTag.ShieldBreak => "破盾",
                FunctionTag.AntiGhost => "驱邪",
                FunctionTag.AntiSeal => "解封",
                FunctionTag.AntiPoison => "解毒",
                _ => "无"
            };
        }

        public static string GetCounterTagName(CounterTag tag)
        {
            return tag switch
            {
                CounterTag.Shield => "护盾",
                CounterTag.Poison => "中毒",
                CounterTag.Burn => "灼烧",
                CounterTag.Seal => "封印",
                CounterTag.Ghost => "鬼怪",
                CounterTag.StealEnergy => "偷灵气",
                CounterTag.Summon => "召唤",
                CounterTag.Group => "群体",
                CounterTag.Charge => "蓄力",
                CounterTag.Boss => "首领",
                _ => "无"
            };
        }

        public static string GetEffectTypeName(EffectType effectType)
        {
            return effectType switch
            {
                EffectType.DealDamage => "造成伤害",
                EffectType.ApplyBurn => "施加灼烧",
                EffectType.ChainDamage => "连锁伤害",
                EffectType.GainShield => "获得护盾",
                EffectType.Heal => "恢复气血",
                EffectType.CleanseStatus => "净化异常",
                EffectType.SuppressGhost => "镇压鬼怪",
                EffectType.GenerateEnergy => "产生灵气",
                EffectType.EnhanceAdjacent => "强化相邻",
                _ => "无"
            };
        }

        public static string BuildDebugSummary(TalismanItemDefinition item)
        {
            if (item == null)
            {
                return "[标签] <null>";
            }

            StringBuilder builder = new();
            builder.AppendLine($"[标签] {item.displayName}");
            builder.AppendLine($"元素：{GetElementTagName(item.elementTag)}");
            builder.AppendLine($"功能：{JoinFunctionTags(item)}");
            builder.AppendLine($"克制：{JoinCounterTags(item)}");
            builder.AppendLine($"效果：{GetEffectTypeName(item.effectType)}");
            builder.AppendLine($"阵法供能：{(item.requiresFormationPower ? "需要" : "不需要")}");
            return builder.ToString();
        }

        private static string JoinTags<T>(IReadOnlyCollection<T> tags, Func<T, string> nameResolver) where T : Enum
        {
            if (tags == null || tags.Count == 0)
            {
                return "无";
            }

            StringBuilder builder = new();
            foreach (T tag in tags)
            {
                if (EqualityComparer<T>.Default.Equals(tag, default))
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(nameResolver(tag));
            }

            return builder.Length > 0 ? builder.ToString() : "无";
        }
    }
}
