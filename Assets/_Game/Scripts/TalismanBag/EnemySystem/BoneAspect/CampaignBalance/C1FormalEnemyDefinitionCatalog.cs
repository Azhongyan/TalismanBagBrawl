using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime;
using TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime.BoneSwapRemnant;

namespace TalismanBag.EnemySystem.BoneAspect.CampaignBalance
{
    public sealed class C1FormalEnemyAttackDefinition
    {
        public C1FormalEnemyAttackDefinition(
            string actionId,
            string effectRequestKey,
            int damage,
            long firstDueMilliseconds,
            long repeatIntervalMilliseconds)
        {
            ActionId = actionId ?? string.Empty;
            EffectRequestKey = effectRequestKey ?? string.Empty;
            Damage = damage;
            FirstDueMilliseconds = firstDueMilliseconds;
            RepeatIntervalMilliseconds = repeatIntervalMilliseconds;
        }

        public string ActionId { get; }
        public string EffectRequestKey { get; }
        public int Damage { get; }
        public long FirstDueMilliseconds { get; }
        public long RepeatIntervalMilliseconds { get; }
    }

    public sealed class C1FormalEnemyDefinition
    {
        private readonly ReadOnlyCollection<string> actionIds;
        private readonly ReadOnlyCollection<string> effectRequestKeys;
        private readonly ReadOnlyCollection<string> mechanicTags;
        private readonly ReadOnlyCollection<C1FormalEnemySkillDefinition>
            skills;

        public C1FormalEnemyDefinition(
            string contentId,
            string runtimeProfileId,
            string displayName,
            string presentationKey,
            int maxHp,
            int maxShell,
            int initialShell,
            C1FormalEnemyAttackDefinition primaryAttack,
            IEnumerable<string> actionIds,
            IEnumerable<string> effectRequestKeys,
            IEnumerable<string> mechanicTags,
            IEnumerable<C1FormalEnemySkillDefinition> skills)
        {
            ContentId = contentId ?? string.Empty;
            RuntimeProfileId = runtimeProfileId ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            PresentationKey = presentationKey ?? string.Empty;
            MaxHp = maxHp;
            MaxShell = maxShell;
            InitialShell = initialShell;
            PrimaryAttack = primaryAttack;
            this.actionIds = Array.AsReadOnly((actionIds
                ?? Enumerable.Empty<string>()).ToArray());
            this.effectRequestKeys = Array.AsReadOnly((effectRequestKeys
                ?? Enumerable.Empty<string>()).ToArray());
            this.mechanicTags = Array.AsReadOnly((mechanicTags
                ?? Enumerable.Empty<string>()).ToArray());
            this.skills = Array.AsReadOnly((skills
                ?? Enumerable.Empty<C1FormalEnemySkillDefinition>()).ToArray());
        }

        public string ContentId { get; }
        public string RuntimeProfileId { get; }
        public string DisplayName { get; }
        public string PresentationKey { get; }
        public int MaxHp { get; }
        public int MaxShell { get; }
        public int InitialShell { get; }
        public C1FormalEnemyAttackDefinition PrimaryAttack { get; }
        public IReadOnlyList<string> ActionIds => actionIds;
        public IReadOnlyList<string> EffectRequestKeys => effectRequestKeys;
        public IReadOnlyList<string> MechanicTags => mechanicTags;
        public IReadOnlyList<C1FormalEnemySkillDefinition> Skills => skills;
    }

    public static class C1FormalEnemyDefinitionCatalog
    {
        public const string CatalogId =
            "campaign.normal.lv1.enemy.definition.c1.playtest.v4";
        public const string TuningDisposition =
            "PLAYTEST_V4_EARLY_HOST_HP_NOT_FINAL_BALANCE";
        public const string PorcelainHoundBasicAttackActionId =
            "c1.porcelain_hound.basic_attack";
        public const string BoneSwapRemnantBasicAttackActionId =
            "c1.bone_swap_remnant.basic_attack";
        public const string PorcelainHoundChargeSkillId =
            "c1.porcelain_hound.skill.charge";
        public const string BoneSwapReflectSkillId =
            "c1.bone_swap_remnant.skill.reflect_passive";
        public const string BoneSwapReflectBuffSkillId =
            "c1.bone_swap_remnant.skill.reflect_window";

        private static readonly C1FormalEnemyStatusDefinition PollutionStatus =
            new C1FormalEnemyStatusDefinition(
                "pollution",
                "NEGATIVE_POLLUTION",
                true,
                6000L,
                2000L,
                3,
                C1FormalEnemyStatusStackRules.AddAndRefresh,
                C1FormalEnemyStatusBehaviorKinds.PeriodicDamage,
                1);

        private static readonly C1FormalEnemyStatusDefinition ReflectStatus =
            new C1FormalEnemyStatusDefinition(
                "bone_swap_reflect",
                "POSITIVE_REFLECT",
                false,
                8000L,
                0L,
                2,
                C1FormalEnemyStatusStackRules.AddAndRefresh,
                C1FormalEnemyStatusBehaviorKinds.ReflectWindow,
                0);

        private static readonly ReadOnlyCollection<C1FormalEnemyDefinition>
            Definitions = Array.AsReadOnly(new[]
            {
                BuildShatteredHost(),
                BuildPorcelainHound(),
                BuildBoneSwapRemnant()
            });

        public static IReadOnlyList<C1FormalEnemyDefinition> All =>
            Definitions;

        public static C1FormalEnemyDefinition FindByContentId(
            string contentId)
        {
            return Definitions.SingleOrDefault(candidate => string.Equals(
                candidate.ContentId,
                contentId,
                StringComparison.Ordinal));
        }

        private static C1FormalEnemyDefinition BuildShatteredHost()
        {
            return new C1FormalEnemyDefinition(
                C1EnemyRuntimeContract.ShatteredHostContentId,
                C1EnemyRuntimeContract.ShatteredHostProfileId,
                "碎骨附身者",
                C1EnemyRuntimeContract.ShatteredHostPresentationKey,
                60,
                0,
                0,
                new C1FormalEnemyAttackDefinition(
                    C1EnemyRuntimeContract.ShatteredHostActionId,
                    C1EnemyRuntimeContract.DirectPlayerDamageRequest,
                    2,
                    2400L,
                    4000L),
                new[]
                {
                    C1EnemyRuntimeContract.ShatteredHostActionId,
                    C1EnemyRuntimeContract.ShatteredHostSkillActionId
                },
                new[]
                {
                    C1EnemyRuntimeContract.DirectPlayerDamageRequest,
                    C1EnemyRuntimeContract.PollutedPulseSkillRequest
                },
                new[] { "basic_attack", "pollution" },
                new[]
                {
                    new C1FormalEnemySkillDefinition(
                        "c1.shattered_host.passive.pollution_on_hit",
                        C1FormalEnemySkillActivationTypes.Passive,
                        C1FormalEnemySkillTriggerTypes.OnBasicAttackHit,
                        0L,
                        0L,
                        0L,
                        C1FormalEnemySkillTargetSelectors.Player,
                        new[]
                        {
                            new C1FormalEnemySkillEffectDefinition(
                                C1FormalEnemySkillEffectKinds.ApplyStatus,
                                status: PollutionStatus)
                        },
                        "enemy.skill.pollution.apply"),
                    new C1FormalEnemySkillDefinition(
                        C1EnemyRuntimeContract.ShatteredHostSkillActionId,
                        C1FormalEnemySkillActivationTypes.Active,
                        C1FormalEnemySkillTriggerTypes.Timer,
                        4000L,
                        8000L,
                        800L,
                        C1FormalEnemySkillTargetSelectors.Player,
                        new[]
                        {
                            new C1FormalEnemySkillEffectDefinition(
                                C1FormalEnemySkillEffectKinds.DirectDamage,
                                5),
                            new C1FormalEnemySkillEffectDefinition(
                                C1FormalEnemySkillEffectKinds.ApplyStatus,
                                status: PollutionStatus)
                        },
                        "enemy.skill.pollution.active")
                });
        }

        private static C1FormalEnemyDefinition BuildPorcelainHound()
        {
            return new C1FormalEnemyDefinition(
                C1EnemyRuntimeContract.PorcelainHoundContentId,
                C1EnemyRuntimeContract.PorcelainHoundProfileId,
                "骨瓷犬",
                C1EnemyRuntimeContract.PorcelainHoundPresentationKey,
                160,
                60,
                0,
                new C1FormalEnemyAttackDefinition(
                    PorcelainHoundBasicAttackActionId,
                    C1EnemyRuntimeContract.DirectPlayerDamageRequest,
                    5,
                    2800L,
                    4600L),
                new[]
                {
                    PorcelainHoundBasicAttackActionId,
                    PorcelainHoundChargeSkillId
                },
                new[]
                {
                    C1EnemyRuntimeContract.DirectPlayerDamageRequest,
                    C1EnemyRuntimeContract.DirectPlayerDamageRequest
                },
                new[] { "basic_attack", "heavy_hit", "shell", "active" },
                new[]
                {
                    new C1FormalEnemySkillDefinition(
                        "c1.porcelain_hound.passive.spawn_shell",
                        C1FormalEnemySkillActivationTypes.Passive,
                        C1FormalEnemySkillTriggerTypes.OnSpawn,
                        0L,
                        0L,
                        0L,
                        C1FormalEnemySkillTargetSelectors.Self,
                        new[]
                        {
                            new C1FormalEnemySkillEffectDefinition(
                                C1FormalEnemySkillEffectKinds.Shield,
                                60)
                        },
                        "enemy.skill.shell.spawn"),
                    new C1FormalEnemySkillDefinition(
                        PorcelainHoundChargeSkillId,
                        C1FormalEnemySkillActivationTypes.Active,
                        C1FormalEnemySkillTriggerTypes.Timer,
                        5000L,
                        9000L,
                        1000L,
                        C1FormalEnemySkillTargetSelectors.Player,
                        new[]
                        {
                            new C1FormalEnemySkillEffectDefinition(
                                C1FormalEnemySkillEffectKinds.DirectDamage,
                                11)
                        },
                        "enemy.skill.charge")
                });
        }

        private static C1FormalEnemyDefinition BuildBoneSwapRemnant()
        {
            return new C1FormalEnemyDefinition(
                C1EnemyRuntimeContract.BoneSwapRemnantContentId,
                BoneSwapRemnantRuntimeContract.OperatorProfileId,
                "换骨残相",
                "enemy.bone_aspect.c1.bone_swap_remnant",
                280,
                0,
                0,
                new C1FormalEnemyAttackDefinition(
                    BoneSwapRemnantBasicAttackActionId,
                    C1EnemyRuntimeContract.DirectPlayerDamageRequest,
                    4,
                    3000L,
                    4800L),
                new[]
                {
                    BoneSwapRemnantBasicAttackActionId,
                    BoneSwapRemnantRuntimeContract.ActionId
                },
                new[]
                {
                    C1EnemyRuntimeContract.DirectPlayerDamageRequest,
                    BoneSwapRemnantRuntimeContract.EffectRequestKey
                },
                new[]
                {
                    "basic_attack",
                    "active",
                    "passive",
                    "direct_damage_reflect"
                },
                new[]
                {
                    new C1FormalEnemySkillDefinition(
                        BoneSwapReflectBuffSkillId,
                        C1FormalEnemySkillActivationTypes.Active,
                        C1FormalEnemySkillTriggerTypes.Timer,
                        1500L,
                        5000L,
                        700L,
                        C1FormalEnemySkillTargetSelectors.Self,
                        new[]
                        {
                            new C1FormalEnemySkillEffectDefinition(
                                C1FormalEnemySkillEffectKinds.ApplyStatus,
                                status: ReflectStatus)
                        },
                        "enemy.skill.reflect_window"),
                    new C1FormalEnemySkillDefinition(
                        BoneSwapReflectSkillId,
                        C1FormalEnemySkillActivationTypes.Passive,
                        C1FormalEnemySkillTriggerTypes.OnDamaged,
                        0L,
                        0L,
                        0L,
                        C1FormalEnemySkillTargetSelectors.Player,
                        new[]
                        {
                            new C1FormalEnemySkillEffectDefinition(
                                C1FormalEnemySkillEffectKinds.ReflectDamage,
                                requiredStatusKey: ReflectStatus.StatusKey,
                                ratioBasisPointsPerStack: 1000,
                                capValue: 6)
                        },
                        "enemy.skill.reflect_passive")
                });
        }
    }
}
