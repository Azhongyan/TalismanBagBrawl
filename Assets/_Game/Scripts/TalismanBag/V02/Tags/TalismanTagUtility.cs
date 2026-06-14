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
                ElementTag.Fire => "\u706b",
                ElementTag.Thunder => "\u96f7",
                ElementTag.Water => "\u6c34",
                ElementTag.Wood => "\u6728",
                ElementTag.Metal => "\u91d1",
                ElementTag.Earth => "\u571f",
                ElementTag.Sword => "\u5251",
                ElementTag.Soul => "\u9b42",
                _ => "\u65e0"
            };
        }

        public static string GetFunctionTagName(FunctionTag tag)
        {
            return tag switch
            {
                FunctionTag.Damage => "\u4f24\u5bb3",
                FunctionTag.Burst => "\u7206\u53d1",
                FunctionTag.Burn => "\u707c\u70e7",
                FunctionTag.Chain => "\u8fde\u9501",
                FunctionTag.AoE => "\u7fa4\u653b",
                FunctionTag.Shield => "\u62a4\u76fe",
                FunctionTag.Heal => "\u6cbb\u7597",
                FunctionTag.Cleanse => "\u51c0\u5316",
                FunctionTag.Defense => "\u9632\u5fa1",
                FunctionTag.EnergySource => "\u4f9b\u80fd",
                FunctionTag.Enhance => "\u5f3a\u5316",
                FunctionTag.ShieldBreak => "\u7834\u76fe",
                FunctionTag.AntiGhost => "\u9a71\u90aa",
                FunctionTag.AntiSeal => "\u89e3\u5c01",
                FunctionTag.AntiPoison => "\u89e3\u6bd2",
                _ => "\u65e0"
            };
        }

        public static string GetCounterTagName(CounterTag tag)
        {
            return tag switch
            {
                CounterTag.Shield => "\u62a4\u76fe",
                CounterTag.Poison => "\u4e2d\u6bd2",
                CounterTag.Burn => "\u707c\u70e7",
                CounterTag.Seal => "\u5c01\u5370",
                CounterTag.Ghost => "\u9b3c\u602a",
                CounterTag.StealEnergy => "\u5077\u7075\u6c14",
                CounterTag.Summon => "\u53ec\u5524",
                CounterTag.Group => "\u7fa4\u4f53",
                CounterTag.Charge => "\u84c4\u529b",
                CounterTag.Boss => "\u9996\u9886",
                _ => "\u65e0"
            };
        }

        public static string GetEffectTypeName(EffectType effectType)
        {
            return effectType switch
            {
                EffectType.DealDamage => "\u9020\u6210\u4f24\u5bb3",
                EffectType.ApplyBurn => "\u65bd\u52a0\u707c\u70e7",
                EffectType.ChainDamage => "\u8fde\u9501\u4f24\u5bb3",
                EffectType.GainShield => "\u83b7\u5f97\u62a4\u76fe",
                EffectType.Heal => "\u6062\u590d\u6c14\u8840",
                EffectType.CleanseStatus => "\u51c0\u5316\u5f02\u5e38",
                EffectType.SuppressGhost => "\u9547\u538b\u9b3c\u602a",
                EffectType.GenerateEnergy => "\u4ea7\u751f\u7075\u6c14",
                EffectType.EnhanceAdjacent => "\u5f3a\u5316\u76f8\u90bb",
                _ => "\u65e0"
            };
        }

        public static string BuildDebugSummary(TalismanItemDefinition item)
        {
            if (item == null)
            {
                return "[\u6807\u7b7e] <null>";
            }

            StringBuilder builder = new();
            builder.AppendLine($"[\u6807\u7b7e] {item.displayName}");
            builder.AppendLine($"\u5143\u7d20\uff1a{GetElementTagName(item.elementTag)}");
            builder.AppendLine($"\u529f\u80fd\uff1a{JoinFunctionTags(item)}");
            builder.AppendLine($"\u514b\u5236\uff1a{JoinCounterTags(item)}");
            builder.AppendLine($"\u6548\u679c\uff1a{GetEffectTypeName(item.effectType)}");
            builder.AppendLine($"\u9635\u6cd5\u4f9b\u80fd\uff1a{(item.requiresFormationPower ? "\u9700\u8981" : "\u4e0d\u9700\u8981")}");
            return builder.ToString();
        }

        private static string JoinTags<T>(IReadOnlyCollection<T> tags, Func<T, string> nameResolver) where T : Enum
        {
            if (tags == null || tags.Count == 0)
            {
                return "\u65e0";
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

            return builder.Length > 0 ? builder.ToString() : "\u65e0";
        }
    }
}
