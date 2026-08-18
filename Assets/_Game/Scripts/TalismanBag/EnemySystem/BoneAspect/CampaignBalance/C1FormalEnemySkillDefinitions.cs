using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TalismanBag.EnemySystem.BoneAspect.CampaignBalance
{
    public static class C1FormalEnemySkillActivationTypes
    {
        public const string Active = "ACTIVE";
        public const string Passive = "PASSIVE";
    }

    public static class C1FormalEnemySkillTriggerTypes
    {
        public const string Timer = "TIMER";
        public const string OnSpawn = "ON_SPAWN";
        public const string OnBasicAttackHit = "ON_BASIC_ATTACK_HIT";
        public const string OnDamaged = "ON_DAMAGED";
    }

    public static class C1FormalEnemySkillTargetSelectors
    {
        public const string Self = "SELF";
        public const string Player = "PLAYER";
    }

    public static class C1FormalEnemySkillEffectKinds
    {
        public const string DirectDamage = "DIRECT_DAMAGE";
        public const string ApplyStatus = "APPLY_STATUS";
        public const string Shield = "SHIELD";
        public const string ReflectDamage = "REFLECT_DAMAGE";
    }

    public static class C1FormalEnemyStatusBehaviorKinds
    {
        public const string PeriodicDamage = "PERIODIC_DAMAGE";
        public const string ReflectWindow = "REFLECT_WINDOW";
    }

    public static class C1FormalEnemyStatusStackRules
    {
        public const string AddAndRefresh = "ADD_AND_REFRESH";
    }

    public sealed class C1FormalEnemyStatusDefinition
    {
        public C1FormalEnemyStatusDefinition(
            string statusKey,
            string statusFamilyKey,
            bool cleanseable,
            long durationMilliseconds,
            long tickIntervalMilliseconds,
            int maxStacks,
            string stackRule,
            string behaviorKind,
            int valuePerStack)
        {
            StatusKey = statusKey ?? string.Empty;
            StatusFamilyKey = statusFamilyKey ?? string.Empty;
            Cleanseable = cleanseable;
            DurationMilliseconds = durationMilliseconds;
            TickIntervalMilliseconds = tickIntervalMilliseconds;
            MaxStacks = Math.Max(1, maxStacks);
            StackRule = stackRule ?? string.Empty;
            BehaviorKind = behaviorKind ?? string.Empty;
            ValuePerStack = valuePerStack;
        }

        public string StatusKey { get; }
        public string StatusFamilyKey { get; }
        public bool Cleanseable { get; }
        public long DurationMilliseconds { get; }
        public long TickIntervalMilliseconds { get; }
        public int MaxStacks { get; }
        public string StackRule { get; }
        public string BehaviorKind { get; }
        public int ValuePerStack { get; }
    }

    public sealed class C1FormalEnemySkillEffectDefinition
    {
        public C1FormalEnemySkillEffectDefinition(
            string effectKind,
            int flatValue = 0,
            C1FormalEnemyStatusDefinition status = null,
            string requiredStatusKey = "",
            int ratioBasisPointsPerStack = 0,
            int capValue = 0)
        {
            EffectKind = effectKind ?? string.Empty;
            FlatValue = flatValue;
            Status = status;
            RequiredStatusKey = requiredStatusKey ?? string.Empty;
            RatioBasisPointsPerStack = ratioBasisPointsPerStack;
            CapValue = capValue;
        }

        public string EffectKind { get; }
        public int FlatValue { get; }
        public C1FormalEnemyStatusDefinition Status { get; }
        public string RequiredStatusKey { get; }
        public int RatioBasisPointsPerStack { get; }
        public int CapValue { get; }
    }

    public sealed class C1FormalEnemySkillDefinition
    {
        private readonly ReadOnlyCollection<
            C1FormalEnemySkillEffectDefinition> effects;

        public C1FormalEnemySkillDefinition(
            string skillId,
            string activationType,
            string triggerType,
            long initialDelayMilliseconds,
            long cooldownMilliseconds,
            long castTimeMilliseconds,
            string targetSelector,
            IEnumerable<C1FormalEnemySkillEffectDefinition> effects,
            string presentationCueKey)
        {
            SkillId = skillId ?? string.Empty;
            ActivationType = activationType ?? string.Empty;
            TriggerType = triggerType ?? string.Empty;
            InitialDelayMilliseconds = initialDelayMilliseconds;
            CooldownMilliseconds = cooldownMilliseconds;
            CastTimeMilliseconds = castTimeMilliseconds;
            TargetSelector = targetSelector ?? string.Empty;
            this.effects = Array.AsReadOnly((effects
                ?? Enumerable.Empty<C1FormalEnemySkillEffectDefinition>())
                .ToArray());
            PresentationCueKey = presentationCueKey ?? string.Empty;
        }

        public string SkillId { get; }
        public string ActivationType { get; }
        public string TriggerType { get; }
        public long InitialDelayMilliseconds { get; }
        public long CooldownMilliseconds { get; }
        public long CastTimeMilliseconds { get; }
        public string TargetSelector { get; }
        public IReadOnlyList<C1FormalEnemySkillEffectDefinition> Effects =>
            effects;
        public string PresentationCueKey { get; }
    }
}
