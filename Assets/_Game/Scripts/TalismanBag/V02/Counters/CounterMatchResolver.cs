using System.Collections.Generic;
using TalismanBag.Combat;
using TalismanBag.Enemies;
using TalismanBag.Items;
using TalismanBag.V02.Balance;
using TalismanBag.V02.EnemySkills;
using TalismanBag.V02.Tags;
using UnityEngine;

namespace TalismanBag.V02.Counters
{
    public sealed class CounterMatchResolver : MonoBehaviour
    {
        [SerializeField] private V02CounterMultiplierConfig multiplierConfig;

        public bool CanTalismanCounterSkill(TalismanItemRuntime talisman, EnemySkillDefinition skill)
        {
            if (talisman?.definition == null || skill?.skillTags == null)
            {
                return false;
            }

            foreach (CounterTag tag in skill.skillTags)
            {
                if (CanTalismanCounterTag(talisman, tag))
                {
                    return true;
                }
            }

            return false;
        }

        public bool CanTalismanCounterTag(TalismanItemRuntime talisman, CounterTag tag)
        {
            TalismanItemDefinition definition = talisman?.definition;
            if (definition == null || talisman.isSealed || talisman.isTemporarilyDisabled)
            {
                return false;
            }

            if (TalismanTagUtility.CanCounter(definition, tag))
            {
                return true;
            }

            return tag switch
            {
                CounterTag.Shield => TalismanTagUtility.HasFunctionTag(definition, FunctionTag.ShieldBreak) ||
                                     TalismanTagUtility.HasFunctionTag(definition, FunctionTag.Burst),
                CounterTag.Poison => TalismanTagUtility.HasFunctionTag(definition, FunctionTag.Cleanse) ||
                                     TalismanTagUtility.HasFunctionTag(definition, FunctionTag.AntiPoison),
                CounterTag.Burn => TalismanTagUtility.HasFunctionTag(definition, FunctionTag.Cleanse),
                CounterTag.Seal => TalismanTagUtility.HasFunctionTag(definition, FunctionTag.Cleanse) ||
                                   TalismanTagUtility.HasFunctionTag(definition, FunctionTag.AntiSeal),
                CounterTag.Ghost => TalismanTagUtility.HasFunctionTag(definition, FunctionTag.AntiGhost),
                CounterTag.StealEnergy => TalismanTagUtility.HasFunctionTag(definition, FunctionTag.AntiGhost),
                CounterTag.Group => TalismanTagUtility.HasFunctionTag(definition, FunctionTag.Chain) ||
                                    TalismanTagUtility.HasFunctionTag(definition, FunctionTag.AoE),
                CounterTag.Summon => TalismanTagUtility.HasFunctionTag(definition, FunctionTag.Chain) ||
                                     TalismanTagUtility.HasFunctionTag(definition, FunctionTag.AoE),
                CounterTag.Charge => TalismanTagUtility.HasFunctionTag(definition, FunctionTag.Shield) ||
                                     TalismanTagUtility.HasFunctionTag(definition, FunctionTag.Defense),
                _ => false
            };
        }

        public bool TryResolveShieldBreak(TalismanItemRuntime talisman, EnemyRuntime enemy, int baseDamage, out int finalDamage)
        {
            finalDamage = baseDamage;
            if (enemy == null || enemy.currentShield <= 0 || !CanTalismanCounterTag(talisman, CounterTag.Shield))
            {
                return false;
            }

            TalismanItemDefinition definition = talisman.definition;
            CounterRelation relation = TalismanTagUtility.HasFunctionTag(definition, FunctionTag.ShieldBreak)
                ? CounterRelation.StrongCounter
                : CounterRelation.LightCounter;
            float multiplier = multiplierConfig != null ? multiplierConfig.GetMultiplier(relation) : relation == CounterRelation.StrongCounter ? 1.8f : 1.35f;
            finalDamage = Mathf.Max(baseDamage + 1, Mathf.RoundToInt(baseDamage * multiplier));
            return true;
        }

        public bool TryResolveCleanse(TalismanItemRuntime talisman, CombatStats playerStatus)
        {
            if (playerStatus == null || !CanTalismanCounterTag(talisman, CounterTag.Poison) && !CanTalismanCounterTag(talisman, CounterTag.Burn))
            {
                return false;
            }

            if (playerStatus.poisonStacks <= 0 && playerStatus.burnStacks <= 0)
            {
                return false;
            }

            playerStatus.poisonStacks = 0;
            playerStatus.burnStacks = 0;
            return true;
        }

        public bool TryResolveSealCleanse(TalismanItemRuntime talisman, List<TalismanItemRuntime> sealedItems)
        {
            if (sealedItems == null || sealedItems.Count == 0 || !CanTalismanCounterTag(talisman, CounterTag.Seal))
            {
                return false;
            }

            bool cleansed = false;
            foreach (TalismanItemRuntime item in sealedItems)
            {
                if (item == null)
                {
                    continue;
                }

                item.isSealed = false;
                item.sealRemaining = 0f;
                cleansed = true;
            }

            sealedItems.Clear();
            return cleansed;
        }

        public bool TryResolveStealEnergyCounter(EnemySkillDefinition stealSkill, List<TalismanItemRuntime> placedPoweredTalismans)
        {
            if (stealSkill == null || placedPoweredTalismans == null)
            {
                return false;
            }

            foreach (TalismanItemRuntime talisman in placedPoweredTalismans)
            {
                bool skillMatches = stealSkill.skillType == EnemySkillType.StealEnergy || CanTalismanCounterSkill(talisman, stealSkill);
                if (skillMatches &&
                    (CanTalismanCounterTag(talisman, CounterTag.Ghost) || CanTalismanCounterTag(talisman, CounterTag.StealEnergy)))
                {
                    return true;
                }
            }

            return false;
        }

        public bool TryResolveGroupCounter(TalismanItemRuntime talisman, EnemyRuntime enemy)
        {
            if (enemy?.definition == null)
            {
                return false;
            }

            TalismanItemDefinition definition = talisman?.definition;
            bool hasClearFunction = TalismanTagUtility.HasFunctionTag(definition, FunctionTag.Chain) ||
                                    TalismanTagUtility.HasFunctionTag(definition, FunctionTag.AoE);
            if (!hasClearFunction)
            {
                return false;
            }

            return HasEnemyWeakness(enemy, CounterTag.Group) && CanTalismanCounterTag(talisman, CounterTag.Group) ||
                   HasEnemyWeakness(enemy, CounterTag.Summon) && CanTalismanCounterTag(talisman, CounterTag.Summon);
        }

        private static bool HasEnemyWeakness(EnemyRuntime enemy, CounterTag tag)
        {
            return enemy?.definition?.weaknessTags != null && enemy.definition.weaknessTags.Contains(tag);
        }
    }
}
