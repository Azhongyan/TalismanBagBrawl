using System;
using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime
{
    public static class C1EnemyRuntimeCatalog
    {
        private static readonly C1EnemyRuntimeProfileSnapshot[] Profiles =
        {
            new C1EnemyRuntimeProfileSnapshot(
                C1EnemyRuntimeContract.ShatteredHostProfileId,
                C1EnemyRuntimeContract.ShatteredHostContentId,
                "碎骨附身者",
                C1EnemyRuntimeContract.ShatteredHostPresentationKey,
                new[]
                {
                    "mechanic.basic_pressure",
                    "mechanic.polluted_tile",
                    "mechanic.possession_state"
                },
                new[]
                {
                    C1EnemyRuntimeContract.ShatteredHostActionId,
                    C1EnemyRuntimeContract.ShatteredHostSkillActionId
                },
                650,
                0,
                0,
                0,
                C1EnemyRuntimeContract.PostDefeatFieldRequest,
                string.Empty,
                0,
                C1EnemyRuntimeContract.DeathPresentationHoldRecommendationTicks,
                true,
                false,
                true,
                false,
                false,
                false),
            new C1EnemyRuntimeProfileSnapshot(
                C1EnemyRuntimeContract.PorcelainHoundProfileId,
                C1EnemyRuntimeContract.PorcelainHoundContentId,
                "骨瓷犬",
                C1EnemyRuntimeContract.PorcelainHoundPresentationKey,
                new[]
                {
                    "mechanic.layered_shield",
                    "mechanic.charge_attack",
                    C1EnemyRuntimeContract.ShellBreakCounterWindow
                },
                new[] { C1EnemyRuntimeContract.PorcelainHoundActionId },
                180,
                100,
                100,
                0,
                string.Empty,
                C1EnemyRuntimeContract.ShellBreakCounterWindow,
                3000,
                0,
                true,
                false,
                true,
                false,
                false,
                false)
        };

        private static readonly C1EnemyActionPatternSnapshot[] Actions =
        {
            new C1EnemyActionPatternSnapshot(
                C1EnemyRuntimeContract.ShatteredHostActionId,
                C1EnemyRuntimeContract.ShatteredHostProfileId,
                "mechanic.basic_pressure",
                C1EnemyRuntimeContract.DirectPlayerDamageRequest,
                2000L,
                5000L,
                500L,
                300L,
                800L,
                700L,
                C1EnemyInterruptPolicy.ReactiveCueFirst,
                100,
                C1EnemyCueKind.BasicAttack,
                string.Empty,
                0,
                true),
            new C1EnemyActionPatternSnapshot(
                C1EnemyRuntimeContract.ShatteredHostSkillActionId,
                C1EnemyRuntimeContract.ShatteredHostProfileId,
                "mechanic.polluted_tile",
                C1EnemyRuntimeContract.PollutedPulseSkillRequest,
                5500L,
                9000L,
                750L,
                500L,
                1250L,
                750L,
                C1EnemyInterruptPolicy.ReactiveCueFirst,
                200,
                C1EnemyCueKind.Skill,
                string.Empty,
                0,
                true),
            new C1EnemyActionPatternSnapshot(
                C1EnemyRuntimeContract.PorcelainHoundActionId,
                C1EnemyRuntimeContract.PorcelainHoundProfileId,
                "mechanic.charge_attack",
                C1EnemyRuntimeContract.ChargeAttackRequest,
                2500L,
                6000L,
                800L,
                400L,
                1200L,
                900L,
                C1EnemyInterruptPolicy.ReactiveCueFirst,
                100,
                C1EnemyCueKind.ChargeAttack,
                string.Empty,
                0,
                true)
        };

        private static readonly C1EnemyHoldAssertion[] Holds =
        {
            new C1EnemyHoldAssertion(
                C1EnemyRuntimeContract.BoneSwapRemnantContentId,
                C1EnemyRuntimeContract.BoneSwapRemnantHoldStatus,
                0,
                0,
                0,
                0)
        };

        public static IReadOnlyList<C1EnemyRuntimeProfileSnapshot> GetProfiles()
        {
            return C1EnemyCollection.Copy(Profiles);
        }

        public static IReadOnlyList<C1EnemyActionPatternSnapshot> GetActionPatterns()
        {
            return C1EnemyCollection.Copy(Actions);
        }

        public static IReadOnlyList<C1EnemyHoldAssertion> GetHoldAssertions()
        {
            return C1EnemyCollection.Copy(Holds);
        }

        public static C1EnemyRuntimeProfileSnapshot FindProfile(string runtimeProfileId)
        {
            return Profiles.FirstOrDefault(profile => string.Equals(
                profile.RuntimeProfileId,
                runtimeProfileId,
                StringComparison.Ordinal));
        }

        public static C1EnemyActionPatternSnapshot FindAction(string actionPatternId)
        {
            return Actions.FirstOrDefault(action => string.Equals(
                action.ActionPatternId,
                actionPatternId,
                StringComparison.Ordinal));
        }

        public static IReadOnlyList<C1EnemyActionPatternSnapshot> FindActionsForProfile(
            string runtimeProfileId)
        {
            return C1EnemyCollection.Copy(
                Actions
                    .Where(action => string.Equals(
                        action.OwnerRuntimeProfileId,
                        runtimeProfileId,
                        StringComparison.Ordinal))
                    .OrderBy(action => action.ActionPatternId, StringComparer.Ordinal));
        }

        public static C1EnemyActionPatternSnapshot FindActionForProfile(
            string runtimeProfileId)
        {
            return Actions
                .Where(action => string.Equals(
                    action.OwnerRuntimeProfileId,
                    runtimeProfileId,
                    StringComparison.Ordinal))
                .OrderBy(action => action.ActionPatternId, StringComparer.Ordinal)
                .FirstOrDefault();
        }
    }
}
