using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime;
using TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime.BoneSwapRemnant;

namespace TalismanBag.EnemySystem.BoneAspect.CampaignBalance.C1Stage3To5Playtest
{
    public sealed class C1Stage3To5PlaytestEnemyEncounterValidationResult
    {
        public C1Stage3To5PlaytestEnemyEncounterValidationResult(
            bool isValid,
            IEnumerable<string> errors)
        {
            IsValid = isValid;
            Errors = new ReadOnlyCollection<string>(
                (errors ?? Enumerable.Empty<string>()).
                    Select(error => error ?? string.Empty).ToList());
            CanonicalSignature =
                C1Stage3To5PlaytestEnemyEncounterCanonical.Validation(
                    IsValid,
                    Errors);
        }

        public bool IsValid { get; private set; }
        public IReadOnlyList<string> Errors { get; private set; }
        public string CanonicalSignature { get; private set; }

        internal C1Stage3To5PlaytestEnemyEncounterValidationResult Copy()
        {
            return new C1Stage3To5PlaytestEnemyEncounterValidationResult(
                IsValid,
                Errors);
        }
    }

    public static class C1Stage3To5PlaytestEnemyEncounterValidation
    {
        public const string MissingProfiles = "MISSING_PROFILES";
        public const string ProfileCountMismatch = "PROFILE_COUNT_MISMATCH";
        public const string ProfileOrderMismatch = "PROFILE_ORDER_MISMATCH";
        public const string ProfileIdentityMismatch = "PROFILE_IDENTITY_MISMATCH";
        public const string ProductContextMismatch = "PRODUCT_CONTEXT_MISMATCH";
        public const string ProfileRevisionMismatch = "PROFILE_REVISION_MISMATCH";
        public const string ProfileStateMismatch = "PROFILE_STATE_MISMATCH";
        public const string RuntimeBattleBindingForbidden =
            "RUNTIME_BATTLE_BINDING_FORBIDDEN";
        public const string RosterCountMismatch = "ROSTER_COUNT_MISMATCH";
        public const string RosterOrderMismatch = "ROSTER_ORDER_MISMATCH";
        public const string ActorIdentityMismatch = "ACTOR_IDENTITY_MISMATCH";
        public const string ActorHpMismatch = "ACTOR_HP_MISMATCH";
        public const string DuplicateActorBalanceId = "DUPLICATE_ACTOR_BALANCE_ID";
        public const string ActorActionsMismatch = "ACTOR_ACTIONS_MISMATCH";
        public const string ActorRequestsMismatch = "ACTOR_REQUESTS_MISMATCH";
        public const string ActorRuntimeCarrierMismatch =
            "ACTOR_RUNTIME_CARRIER_MISMATCH";
        public const string ActorDownstreamConditionMismatch =
            "ACTOR_DOWNSTREAM_CONDITION_MISMATCH";
        public const string ActorDefenseFactMismatch =
            "ACTOR_DEFENSE_FACT_MISMATCH";
        public const string TotalHpMismatch = "TOTAL_HP_MISMATCH";
        public const string AttackWaveMismatch = "ATTACK_WAVE_MISMATCH";
        public const string BoneSwapReleaseMissing = "BONE_SWAP_RELEASE_MISSING";
        public const string BoneSwapReleaseMismatch = "BONE_SWAP_RELEASE_MISMATCH";

        public static C1Stage3To5PlaytestEnemyEncounterValidationResult Validate(
            IEnumerable<C1Stage3To5PlaytestEncounterProfile> sourceProfiles,
            C1Stage3To5BoneSwapReleaseRow boneSwapRelease)
        {
            var errors = new List<string>();
            if (sourceProfiles == null)
            {
                Add(errors, MissingProfiles);
                ValidateBoneSwap(boneSwapRelease, errors);
                return Result(errors);
            }

            List<C1Stage3To5PlaytestEncounterProfile> profiles =
                sourceProfiles.ToList();
            ExpectedProfile[] expected = ExpectedProfiles();
            if (profiles.Count != expected.Length)
            {
                Add(errors, ProfileCountMismatch);
            }

            for (int index = 0; index < profiles.Count; index++)
            {
                C1Stage3To5PlaytestEncounterProfile profile = profiles[index];
                if (profile == null)
                {
                    Add(errors, ProfileIdentityMismatch);
                    continue;
                }

                if (index >= expected.Length)
                {
                    Add(errors, ProfileOrderMismatch);
                    continue;
                }

                ValidateProfile(profile, expected[index], errors);
            }

            IEnumerable<string> actorIds = profiles.
                Where(profile => profile != null).
                SelectMany(profile => profile.Actors ??
                    new C1Stage3To5PlaytestActorRow[0]).
                Where(actor => actor != null).
                Select(actor => actor.ActorBalanceId);
            if (actorIds.GroupBy(id => id, StringComparer.Ordinal).
                Any(group => group.Count() != 1))
            {
                Add(errors, DuplicateActorBalanceId);
            }

            ValidateBoneSwap(boneSwapRelease, errors);
            return Result(errors);
        }

        private static void ValidateProfile(
            C1Stage3To5PlaytestEncounterProfile profile,
            ExpectedProfile expected,
            ICollection<string> errors)
        {
            if (!Equal(profile.SchemaId,
                    C1Stage3To5PlaytestEnemyEncounterContract.SchemaId)
                || !Equal(profile.BalanceProfileId,
                    C1Stage3To5PlaytestEnemyEncounterContract.BalanceProfileId)
                || !Equal(profile.StageId, expected.StageId)
                || !Equal(profile.EncounterVariantId, expected.VariantId))
            {
                Add(errors, ProfileIdentityMismatch);
            }

            if (!Equal(profile.ProductContext,
                    C1Stage3To5PlaytestEnemyEncounterContract.ProductContext))
            {
                Add(errors, ProductContextMismatch);
            }

            if (!Equal(profile.ProfileRevision,
                    C1Stage3To5PlaytestEnemyEncounterContract.ProfileRevision))
            {
                Add(errors, ProfileRevisionMismatch);
            }

            if (!Equal(profile.CandidateDisposition,
                    C1Stage3To5PlaytestEnemyEncounterContract.CandidateDisposition)
                || !Equal(profile.ActivationStatus,
                    C1Stage3To5PlaytestEnemyEncounterContract.ActivationStatus)
                || !profile.IsEnabled
                || !profile.EntersFormalFlow)
            {
                Add(errors, ProfileStateMismatch);
            }

            if (profile.RuntimeBoundToBattle)
            {
                Add(errors, RuntimeBattleBindingForbidden);
            }

            ValidateAttackWave(profile.AttackWave, errors);
            if (profile.Actors == null
                || profile.Actors.Count != expected.Actors.Length)
            {
                Add(errors, RosterCountMismatch);
                return;
            }

            for (int index = 0; index < profile.Actors.Count; index++)
            {
                ValidateActor(
                    profile.Actors[index],
                    expected.Actors[index],
                    expected.StageId,
                    index,
                    errors);
            }

            if (profile.TotalMaxHp != expected.TotalHp)
            {
                Add(errors, TotalHpMismatch);
            }
        }

        private static void ValidateActor(
            C1Stage3To5PlaytestActorRow actor,
            ExpectedActor expected,
            string stageId,
            int index,
            ICollection<string> errors)
        {
            if (actor == null)
            {
                Add(errors, RosterOrderMismatch);
                return;
            }

            if (!Equal(actor.ContentId, expected.ContentId))
            {
                Add(errors, RosterOrderMismatch);
            }

            string expectedActorId = string.Format(
                CultureInfo.InvariantCulture,
                "campaign.normal.lv1.balance.actor.c1.{0}.{1}.o{2:D2}",
                stageId,
                expected.ActorToken,
                expected.Ordinal);
            if (!Equal(actor.ActorBalanceId, expectedActorId)
                || actor.OccurrenceOrdinal != expected.Ordinal)
            {
                Add(errors, ActorIdentityMismatch);
            }

            if (actor.MaxHp != expected.MaxHp)
            {
                Add(errors, ActorHpMismatch);
            }

            if (!Equal(actor.RuntimeProfileId, expected.RuntimeProfileId)
                || !Equal(actor.OperatorProfileId, expected.OperatorProfileId))
            {
                Add(errors, ActorRuntimeCarrierMismatch);
            }

            if (!SequenceEqual(actor.ActionIds, expected.ActionIds))
            {
                Add(errors, ActorActionsMismatch);
            }

            if (!SequenceEqual(actor.EffectRequestKeys, expected.RequestKeys))
            {
                Add(errors, ActorRequestsMismatch);
            }

            if (!SequenceEqual(
                actor.DownstreamConditions,
                new[]
                {
                    C1Stage3To5PlaytestEnemyEncounterContract.
                        DownstreamConditionBattleContractRequired
                }))
            {
                Add(errors, ActorDownstreamConditionMismatch);
            }

            ValidateDefenseFact(actor, expected, errors);

            if (index < 0)
            {
                Add(errors, RosterOrderMismatch);
            }
        }

        private static void ValidateDefenseFact(
            C1Stage3To5PlaytestActorRow actor,
            ExpectedActor expected,
            ICollection<string> errors)
        {
            C1Stage3To5PlaytestEnemyDefenseFact fact = actor.DefenseFact;
            bool shouldHaveShell = Equal(
                expected.ContentId,
                C1EnemyRuntimeContract.PorcelainHoundContentId);
            string expectedSourceProfileId = !string.IsNullOrEmpty(
                expected.OperatorProfileId)
                    ? expected.OperatorProfileId
                    : expected.RuntimeProfileId;
            string expectedSourceSchemaId = !string.IsNullOrEmpty(
                expected.OperatorProfileId)
                    ? BoneSwapRemnantRuntimeContract.SchemaId
                    : C1Stage3To5PlaytestEnemyEncounterContract.
                        DefenseSourceSchemaId;
            bool sourceMatches = fact != null
                && Equal(
                    fact.SourceSchemaId,
                    expectedSourceSchemaId)
                && Equal(fact.SourceProfileId, expectedSourceProfileId)
                && Equal(
                    fact.SourceRevision,
                    C1Stage3To5PlaytestEnemyEncounterContract.
                        DefenseSourceRevision);
            bool shellMatches = shouldHaveShell
                ? fact != null
                    && fact.HasShell
                    && Equal(
                        fact.ShellMechanicId,
                        C1Stage3To5PlaytestEnemyEncounterContract.
                            LayeredShieldMechanicId)
                    && fact.MaxShell ==
                        C1Stage3To5PlaytestEnemyEncounterContract.
                            PorcelainHoundMaxShell
                    && fact.InitialShell ==
                        C1Stage3To5PlaytestEnemyEncounterContract.
                            PorcelainHoundInitialShell
                    && fact.ShellRegenerationCount == 0
                    && Equal(
                        fact.ShellRegenerationPolicyId,
                        C1Stage3To5PlaytestEnemyEncounterContract.
                            NoShellRegenerationPolicyId)
                    && Equal(
                        fact.BreakTargetId,
                        C1Stage3To5PlaytestEnemyEncounterContract.
                            ShellBreakTargetId)
                    && Equal(
                        fact.BrokenStateId,
                        C1Stage3To5PlaytestEnemyEncounterContract.
                            ShellBrokenStateId)
                    && Equal(
                        fact.ShellBreakCounterWindowId,
                        C1Stage3To5PlaytestEnemyEncounterContract.
                            ShellBreakCounterWindowId)
                : fact != null
                    && !fact.HasShell
                    && string.IsNullOrEmpty(fact.ShellMechanicId)
                    && fact.MaxShell == 0
                    && fact.InitialShell == 0
                    && fact.ShellRegenerationCount == 0
                    && Equal(
                        fact.ShellRegenerationPolicyId,
                        C1Stage3To5PlaytestEnemyEncounterContract.
                            NotApplicablePolicyId)
                    && string.IsNullOrEmpty(fact.BreakTargetId)
                    && string.IsNullOrEmpty(fact.BrokenStateId)
                    && string.IsNullOrEmpty(fact.ShellBreakCounterWindowId);
            if (!sourceMatches || !shellMatches)
            {
                Add(errors, ActorDefenseFactMismatch);
            }
        }

        private static void ValidateAttackWave(
            C1Stage3To5PlaytestAttackWaveRow attack,
            ICollection<string> errors)
        {
            if (attack == null
                || !Equal(
                    attack.CadenceScope,
                    C1Stage3To5PlaytestEnemyEncounterContract.AttackCadenceScope)
                || !Equal(
                    attack.EffectRequestKey,
                    C1EnemyRuntimeContract.DirectPlayerDamageRequest)
                || attack.FirstResolveSeconds !=
                    C1Stage3To5PlaytestEnemyEncounterContract.FirstResolveSeconds
                || attack.IntervalSeconds !=
                    C1Stage3To5PlaytestEnemyEncounterContract.IntervalSeconds
                || attack.DamagePerApplication !=
                    C1Stage3To5PlaytestEnemyEncounterContract.DamagePerApplication
                || attack.ApplicationsPerWave !=
                    C1Stage3To5PlaytestEnemyEncounterContract.ApplicationsPerWave)
            {
                Add(errors, AttackWaveMismatch);
            }
        }

        private static void ValidateBoneSwap(
            C1Stage3To5BoneSwapReleaseRow release,
            ICollection<string> errors)
        {
            if (release == null)
            {
                Add(errors, BoneSwapReleaseMissing);
                return;
            }

            if (!Equal(release.ContentId,
                    C1EnemyRuntimeContract.BoneSwapRemnantContentId)
                || !Equal(release.OperatorProfileId,
                    BoneSwapRemnantRuntimeContract.OperatorProfileId)
                || !Equal(release.ActionId,
                    BoneSwapRemnantRuntimeContract.ActionId)
                || !Equal(release.EffectRequestKey,
                    BoneSwapRemnantRuntimeContract.EffectRequestKey)
                || !Equal(release.AcceptedSourceKind,
                    C1Stage3To5PlaytestEnemyEncounterContract.BoneSwapSourceKind)
                || release.RatioBasisPoints !=
                    C1Stage3To5PlaytestEnemyEncounterContract.BoneSwapRatioBasisPoints
                || release.CapDamage !=
                    C1Stage3To5PlaytestEnemyEncounterContract.BoneSwapCapDamage
                || release.TelegraphMilliseconds !=
                    C1Stage3To5PlaytestEnemyEncounterContract.
                        BoneSwapTelegraphMilliseconds
                || release.RecoverMilliseconds !=
                    C1Stage3To5PlaytestEnemyEncounterContract.
                        BoneSwapRecoverMilliseconds
                || !Equal(release.PendingPolicy,
                    C1Stage3To5PlaytestEnemyEncounterContract.BoneSwapPendingPolicy)
                || !Equal(release.TuningStatus,
                    C1Stage3To5PlaytestEnemyEncounterContract.BoneSwapTuningStatus)
                || !release.IsEnabled
                || !release.EntersFormalFlow
                || release.RuntimeBoundToBattle)
            {
                Add(errors, BoneSwapReleaseMismatch);
            }

            if (release.RuntimeBoundToBattle)
            {
                Add(errors, RuntimeBattleBindingForbidden);
            }
        }

        private static C1Stage3To5PlaytestEnemyEncounterValidationResult Result(
            IEnumerable<string> errors)
        {
            List<string> ordered = errors.Distinct(StringComparer.Ordinal).ToList();
            return new C1Stage3To5PlaytestEnemyEncounterValidationResult(
                ordered.Count == 0,
                ordered);
        }

        private static bool SequenceEqual(
            IEnumerable<string> actual,
            IEnumerable<string> expected)
        {
            if (actual == null || expected == null)
            {
                return actual == expected;
            }

            return actual.SequenceEqual(expected, StringComparer.Ordinal);
        }

        private static bool Equal(string left, string right)
        {
            return string.Equals(left, right, StringComparison.Ordinal);
        }

        private static void Add(ICollection<string> errors, string error)
        {
            if (!errors.Contains(error))
            {
                errors.Add(error);
            }
        }

        private static ExpectedProfile[] ExpectedProfiles()
        {
            return new[]
            {
                new ExpectedProfile(
                    C1Stage3To5PlaytestEnemyEncounterContract.Stage1_3,
                    C1Stage3To5PlaytestEnemyEncounterContract.EncounterVariant1_3,
                    C1Stage3To5PlaytestEnemyEncounterContract.Encounter1_3TotalHp,
                    new[]
                    {
                        ExpectedActor.Host(1, 12),
                        ExpectedActor.Hound(1, 14),
                        ExpectedActor.Hound(2, 14)
                    }),
                new ExpectedProfile(
                    C1Stage3To5PlaytestEnemyEncounterContract.Stage1_4,
                    C1Stage3To5PlaytestEnemyEncounterContract.EncounterVariant1_4,
                    C1Stage3To5PlaytestEnemyEncounterContract.Encounter1_4TotalHp,
                    new[]
                    {
                        ExpectedActor.Hound(1, 80),
                        ExpectedActor.Bone(1, 100)
                    }),
                new ExpectedProfile(
                    C1Stage3To5PlaytestEnemyEncounterContract.Stage1_5,
                    C1Stage3To5PlaytestEnemyEncounterContract.EncounterVariant1_5,
                    C1Stage3To5PlaytestEnemyEncounterContract.Encounter1_5TotalHp,
                    new[]
                    {
                        ExpectedActor.Host(1, 60),
                        ExpectedActor.Hound(1, 75),
                        ExpectedActor.Bone(1, 85)
                    })
            };
        }

        private sealed class ExpectedProfile
        {
            public ExpectedProfile(
                string stageId,
                string variantId,
                int totalHp,
                ExpectedActor[] actors)
            {
                StageId = stageId;
                VariantId = variantId;
                TotalHp = totalHp;
                Actors = actors;
            }

            public string StageId;
            public string VariantId;
            public int TotalHp;
            public ExpectedActor[] Actors;
        }

        private sealed class ExpectedActor
        {
            public string ActorToken;
            public string ContentId;
            public string RuntimeProfileId;
            public string OperatorProfileId;
            public int Ordinal;
            public int MaxHp;
            public string[] ActionIds;
            public string[] RequestKeys;

            public static ExpectedActor Host(int ordinal, int maxHp)
            {
                return new ExpectedActor
                {
                    ActorToken = "shattered_host",
                    ContentId = C1EnemyRuntimeContract.ShatteredHostContentId,
                    RuntimeProfileId = C1EnemyRuntimeContract.ShatteredHostProfileId,
                    OperatorProfileId = string.Empty,
                    Ordinal = ordinal,
                    MaxHp = maxHp,
                    ActionIds = new[]
                    {
                        C1EnemyRuntimeContract.ShatteredHostActionId,
                        C1EnemyRuntimeContract.ShatteredHostSkillActionId
                    },
                    RequestKeys = new[]
                    {
                        C1EnemyRuntimeContract.DirectPlayerDamageRequest,
                        C1EnemyRuntimeContract.PollutedPulseSkillRequest
                    }
                };
            }

            public static ExpectedActor Hound(int ordinal, int maxHp)
            {
                return new ExpectedActor
                {
                    ActorToken = "porcelain_hound",
                    ContentId = C1EnemyRuntimeContract.PorcelainHoundContentId,
                    RuntimeProfileId = C1EnemyRuntimeContract.PorcelainHoundProfileId,
                    OperatorProfileId = string.Empty,
                    Ordinal = ordinal,
                    MaxHp = maxHp,
                    ActionIds = new[] { C1EnemyRuntimeContract.PorcelainHoundActionId },
                    RequestKeys = new[] { C1EnemyRuntimeContract.ChargeAttackRequest }
                };
            }

            public static ExpectedActor Bone(int ordinal, int maxHp)
            {
                return new ExpectedActor
                {
                    ActorToken = "bone_swap_remnant",
                    ContentId = C1EnemyRuntimeContract.BoneSwapRemnantContentId,
                    RuntimeProfileId =
                        BoneSwapRemnantRuntimeContract.OperatorProfileId,
                    OperatorProfileId =
                        BoneSwapRemnantRuntimeContract.OperatorProfileId,
                    Ordinal = ordinal,
                    MaxHp = maxHp,
                    ActionIds = new[] { BoneSwapRemnantRuntimeContract.ActionId },
                    RequestKeys = new[]
                    {
                        BoneSwapRemnantRuntimeContract.EffectRequestKey
                    }
                };
            }
        }
    }
}
