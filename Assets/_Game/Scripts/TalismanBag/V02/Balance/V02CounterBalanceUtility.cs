using System.Collections.Generic;
using System.Text;
using TalismanBag.Enemies;
using TalismanBag.Items;
using TalismanBag.V02.Formation;
using TalismanBag.V02.Tags;

namespace TalismanBag.V02.Balance
{
    public static class V02CounterBalanceUtility
    {
        public static CounterRelation ResolveItemRelation(TalismanItemRuntime item, EnemyDefinition enemy, bool isPowered)
        {
            if (item?.definition == null || enemy == null)
            {
                return CounterRelation.Unknown;
            }

            if (!isPowered && item.definition.requiresFormationPower)
            {
                return CounterRelation.HardResisted;
            }

            TalismanItemDefinition definition = item.definition;
            bool hasVulnerableFunction = Intersects(definition.functionTags, enemy.vulnerableFunctions);
            bool hasVulnerableElement = enemy.vulnerableElements != null && enemy.vulnerableElements.Contains(definition.elementTag);
            bool countersWeakness = CountersEnemyWeakness(definition, enemy);

            if (countersWeakness && (hasVulnerableFunction || hasVulnerableElement))
            {
                return CounterRelation.StrongCounter;
            }

            if (countersWeakness || hasVulnerableFunction || hasVulnerableElement)
            {
                return CounterRelation.LightCounter;
            }

            bool resistedFunction = Intersects(definition.functionTags, enemy.resistedFunctions);
            bool resistedElement = enemy.resistedElements != null && enemy.resistedElements.Contains(definition.elementTag);
            if (resistedFunction && resistedElement)
            {
                return CounterRelation.HardResisted;
            }

            if (resistedFunction || resistedElement)
            {
                return CounterRelation.Resisted;
            }

            return CounterRelation.Neutral;
        }

        public static CounterRelation ResolveBuildRelation(IEnumerable<TalismanItemRuntime> items, EnemyDefinition enemy, FormationPowerResolver powerResolver)
        {
            if (items == null || enemy == null)
            {
                return CounterRelation.Unknown;
            }

            CounterRelation best = CounterRelation.HardResisted;
            bool sawItem = false;
            bool sawUnpoweredCore = false;

            foreach (TalismanItemRuntime item in items)
            {
                if (item?.definition == null)
                {
                    continue;
                }

                sawItem = true;
                bool isPowered = !item.definition.requiresFormationPower || powerResolver == null || powerResolver.IsItemPowered(item);
                sawUnpoweredCore |= !isPowered && item.definition.requiresFormationPower;
                CounterRelation relation = ResolveItemRelation(item, enemy, isPowered);
                if (Rank(relation) > Rank(best))
                {
                    best = relation;
                }
            }

            if (!sawItem)
            {
                return CounterRelation.Unknown;
            }

            if (sawUnpoweredCore && Rank(best) < Rank(CounterRelation.LightCounter))
            {
                return CounterRelation.HardResisted;
            }

            return best;
        }

        public static string BuildTags(IEnumerable<TalismanItemRuntime> items, FormationPowerResolver powerResolver)
        {
            if (items == null)
            {
                return string.Empty;
            }

            HashSet<string> tags = new();
            foreach (TalismanItemRuntime item in items)
            {
                if (item?.definition == null)
                {
                    continue;
                }

                bool isPowered = !item.definition.requiresFormationPower || powerResolver == null || powerResolver.IsItemPowered(item);
                string prefix = isPowered ? string.Empty : "Unpowered:";
                if (item.definition.elementTag != ElementTag.None)
                {
                    tags.Add(prefix + item.definition.elementTag);
                }

                if (item.definition.functionTags != null)
                {
                    foreach (FunctionTag tag in item.definition.functionTags)
                    {
                        if (tag != FunctionTag.None)
                        {
                            tags.Add(prefix + tag);
                        }
                    }
                }
            }

            StringBuilder builder = new();
            foreach (string tag in tags)
            {
                if (builder.Length > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(tag);
            }

            return builder.ToString();
        }

        private static bool CountersEnemyWeakness(TalismanItemDefinition definition, EnemyDefinition enemy)
        {
            if (definition?.counterTags == null || enemy?.weaknessTags == null)
            {
                return false;
            }

            foreach (CounterTag weakness in enemy.weaknessTags)
            {
                if (weakness != CounterTag.None && definition.counterTags.Contains(weakness))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool Intersects<T>(IEnumerable<T> left, ICollection<T> right)
        {
            if (left == null || right == null || right.Count == 0)
            {
                return false;
            }

            foreach (T value in left)
            {
                if (right.Contains(value))
                {
                    return true;
                }
            }

            return false;
        }

        private static int Rank(CounterRelation relation)
        {
            return relation switch
            {
                CounterRelation.StrongCounter => 5,
                CounterRelation.LightCounter => 4,
                CounterRelation.Neutral => 3,
                CounterRelation.Resisted => 2,
                CounterRelation.HardResisted => 1,
                _ => 0
            };
        }
    }
}
