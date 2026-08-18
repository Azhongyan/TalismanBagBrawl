using System;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Canonical;

namespace TalismanBag.BattleBridge.Formal
{
    public enum C1FormalRealtimeBattleCanonicalMutationKind
    {
        Unsupported = 0,
        EnemyShellPercent,
        PlayerNianRefund,
        DirectDamagePercent,
        ExtraTarget,
        DamageToBreak,
        ExtraBreakPercent,
        BurnStack,
        ExtraBurnTrigger,
        NianCostReduction,
        BurnConversion,
        EmberTrigger,
        PlayerGuardFlat,
        PlayerGuardPercent,
        DamageToGuard,
        HealToGuard,
        Cleanse,
        CooldownReduction,
        DebuffToHeal,
        PlayerHealFlat,
        EnemyCastDelay,
        ExtraDamagePercent,
        TargetingOverride
    }

    public sealed class C1FormalRealtimeBattleCanonicalEffectPlan
    {
        internal C1FormalRealtimeBattleCanonicalEffectPlan(
            C1FormalRealtimeBattleCanonicalMutationKind mutationKind,
            CanonicalItemEffectDefinition effect)
        {
            MutationKind = mutationKind;
            EffectId = effect.effectId;
            TriggerEventId = effect.triggerEventId;
            ConditionId = effect.conditionId;
            TargetSelector = effect.targetSelector;
            TargetStatId = effect.targetStatId;
            Operation = effect.operation;
            ValueUnitKey = effect.valueUnitKey;
            ValueUnits = effect.valueUnits;
            SecondaryValueUnits = effect.secondaryValueUnits;
            DurationUnits = effect.durationUnits;
            InternalCooldownUnits = effect.internalCooldownUnits;
        }

        public C1FormalRealtimeBattleCanonicalMutationKind MutationKind
        {
            get;
        }
        public string EffectId { get; }
        public string TriggerEventId { get; }
        public string ConditionId { get; }
        public string TargetSelector { get; }
        public string TargetStatId { get; }
        public ItemCandidateEffectOperation Operation { get; }
        public string ValueUnitKey { get; }
        public long ValueUnits { get; }
        public long SecondaryValueUnits { get; }
        public long DurationUnits { get; }
        public long InternalCooldownUnits { get; }
    }

    public static class C1FormalRealtimeBattleCanonicalEffectOperator
    {
        public static bool TryResolve(
            CanonicalItemEffectDefinition effect,
            out C1FormalRealtimeBattleCanonicalEffectPlan plan)
        {
            plan = null;
            if (effect == null)
            {
                return false;
            }

            C1FormalRealtimeBattleCanonicalMutationKind kind = ResolveKind(
                effect.operation,
                effect.targetStatId,
                effect.valueUnitKey);
            if (kind == C1FormalRealtimeBattleCanonicalMutationKind.Unsupported)
            {
                return false;
            }

            plan = new C1FormalRealtimeBattleCanonicalEffectPlan(kind, effect);
            return true;
        }

        public static long ApplyBasisPoints(long baseValue, long basisPoints)
        {
            if (baseValue <= 0L || basisPoints <= 0L)
            {
                return 0L;
            }

            return checked(baseValue * basisPoints / 10000L);
        }

        private static C1FormalRealtimeBattleCanonicalMutationKind ResolveKind(
            ItemCandidateEffectOperation operation,
            string targetStatId,
            string valueUnitKey)
        {
            if (operation == ItemCandidateEffectOperation.AddPercent
                && Same(targetStatId, "break")
                && Same(valueUnitKey, "basisPoint"))
            {
                return C1FormalRealtimeBattleCanonicalMutationKind
                    .EnemyShellPercent;
            }
            if (operation == ItemCandidateEffectOperation.Refund
                && Same(targetStatId, "nian")
                && Same(valueUnitKey, "point"))
            {
                return C1FormalRealtimeBattleCanonicalMutationKind
                    .PlayerNianRefund;
            }
            if (operation == ItemCandidateEffectOperation.AddPercent
                && Same(targetStatId, "damage")
                && Same(valueUnitKey, "basisPoint"))
            {
                return C1FormalRealtimeBattleCanonicalMutationKind
                    .DirectDamagePercent;
            }
            if (operation == ItemCandidateEffectOperation.ExtraTarget
                && Same(targetStatId, "targetCount")
                && Same(valueUnitKey, "count"))
            {
                return C1FormalRealtimeBattleCanonicalMutationKind.ExtraTarget;
            }
            if (operation == ItemCandidateEffectOperation.Convert
                && Same(targetStatId, "damage_to_break")
                && Same(valueUnitKey, "basisPoint"))
            {
                return C1FormalRealtimeBattleCanonicalMutationKind.DamageToBreak;
            }
            if (operation == ItemCandidateEffectOperation.ExtraTrigger
                && Same(targetStatId, "break")
                && Same(valueUnitKey, "basisPoint"))
            {
                return C1FormalRealtimeBattleCanonicalMutationKind
                    .ExtraBreakPercent;
            }
            if (operation == ItemCandidateEffectOperation.AddFlat
                && Same(targetStatId, "burn")
                && Same(valueUnitKey, "stack"))
            {
                return C1FormalRealtimeBattleCanonicalMutationKind.BurnStack;
            }
            if (operation == ItemCandidateEffectOperation.ExtraTrigger
                && Same(targetStatId, "burn")
                && Same(valueUnitKey, "turn"))
            {
                return C1FormalRealtimeBattleCanonicalMutationKind
                    .ExtraBurnTrigger;
            }
            if (operation == ItemCandidateEffectOperation.ReduceFlat
                && Same(targetStatId, "nianCost")
                && Same(valueUnitKey, "point"))
            {
                return C1FormalRealtimeBattleCanonicalMutationKind
                    .NianCostReduction;
            }
            if (operation == ItemCandidateEffectOperation.Convert
                && Same(targetStatId, "burn")
                && Same(valueUnitKey, "stack"))
            {
                return C1FormalRealtimeBattleCanonicalMutationKind
                    .BurnConversion;
            }
            if (operation == ItemCandidateEffectOperation.ExtraTrigger
                && Same(targetStatId, "ember")
                && Same(valueUnitKey, "count"))
            {
                return C1FormalRealtimeBattleCanonicalMutationKind.EmberTrigger;
            }
            if (operation == ItemCandidateEffectOperation.AddFlat
                && Same(targetStatId, "guard")
                && Same(valueUnitKey, "flat"))
            {
                return C1FormalRealtimeBattleCanonicalMutationKind
                    .PlayerGuardFlat;
            }
            if (operation == ItemCandidateEffectOperation.AddPercent
                && Same(targetStatId, "guard")
                && Same(valueUnitKey, "basisPoint"))
            {
                return C1FormalRealtimeBattleCanonicalMutationKind
                    .PlayerGuardPercent;
            }
            if (operation == ItemCandidateEffectOperation.Convert
                && Same(targetStatId, "damage_to_guard")
                && Same(valueUnitKey, "basisPoint"))
            {
                return C1FormalRealtimeBattleCanonicalMutationKind.DamageToGuard;
            }
            if (operation == ItemCandidateEffectOperation.Convert
                && Same(targetStatId, "heal_to_guard")
                && Same(valueUnitKey, "basisPoint"))
            {
                return C1FormalRealtimeBattleCanonicalMutationKind.HealToGuard;
            }
            if (operation == ItemCandidateEffectOperation.AddFlat
                && Same(targetStatId, "cleanse")
                && Same(valueUnitKey, "stack"))
            {
                return C1FormalRealtimeBattleCanonicalMutationKind.Cleanse;
            }
            if (operation == ItemCandidateEffectOperation.ReduceFlat
                && Same(targetStatId, "cooldown")
                && Same(valueUnitKey, "turn"))
            {
                return C1FormalRealtimeBattleCanonicalMutationKind
                    .CooldownReduction;
            }
            if (operation == ItemCandidateEffectOperation.Convert
                && Same(targetStatId, "debuff_to_heal")
                && Same(valueUnitKey, "turn"))
            {
                return C1FormalRealtimeBattleCanonicalMutationKind.DebuffToHeal;
            }
            if (operation == ItemCandidateEffectOperation.ExtraTrigger
                && Same(targetStatId, "heal")
                && Same(valueUnitKey, "flat"))
            {
                return C1FormalRealtimeBattleCanonicalMutationKind.PlayerHealFlat;
            }
            if (operation == ItemCandidateEffectOperation.ReduceFlat
                && Same(targetStatId, "castProgress")
                && Same(valueUnitKey, "turn"))
            {
                return C1FormalRealtimeBattleCanonicalMutationKind.EnemyCastDelay;
            }
            if (operation == ItemCandidateEffectOperation.ExtraTrigger
                && Same(targetStatId, "damage")
                && Same(valueUnitKey, "basisPoint"))
            {
                return C1FormalRealtimeBattleCanonicalMutationKind
                    .ExtraDamagePercent;
            }
            if (operation == ItemCandidateEffectOperation.Override
                && Same(targetStatId, "targeting")
                && Same(valueUnitKey, "count"))
            {
                return C1FormalRealtimeBattleCanonicalMutationKind
                    .TargetingOverride;
            }

            return C1FormalRealtimeBattleCanonicalMutationKind.Unsupported;
        }

        private static bool Same(string left, string right)
        {
            return string.Equals(left, right, StringComparison.Ordinal);
        }
    }
}
