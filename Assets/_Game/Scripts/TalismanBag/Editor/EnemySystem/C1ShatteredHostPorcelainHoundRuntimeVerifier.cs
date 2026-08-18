using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace TalismanBag.EditorTools.EnemySystem
{
    public static class C1ShatteredHostPorcelainHoundRuntimeVerifier
    {
        private const string PassMarker =
            "C1_SHATTERED_HOST_PLAYABLE_TUNING_AND_SKILL01_PASS hp=650 cadence=1500 basic=4 skill=2 lethal=21000 battleWrites=0 itemWrites=0";

        private static readonly string[] SourcePaths =
        {
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimePrimitives.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeSnapshots.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeCatalog.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeReducer.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyActionScheduler.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeValidation.cs"
        };

#if UNITY_EDITOR
        private static void RunFromMenu()
        {
            int exitCode = Execute();
            if (exitCode != 0)
            {
                throw new InvalidOperationException(
                    "C1 Shattered Host playable tuning verification failed.");
            }
        }
#endif

        public static void RunFromCommandLine()
        {
            int exitCode = Execute();
#if UNITY_EDITOR
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(exitCode);
            }
#endif
        }

#if !UNITY_EDITOR
        public static int Main(string[] args)
        {
            return Execute();
        }
#endif

        private static int Execute()
        {
            string root = FindProjectRoot();
            VerificationOutput output = new VerificationOutput();
            RunCatalogAndSchema(output, root);
            RunSchedulerAndCanonicalScenarios(output, root);
            RunApplicationNegativeScenarios(output);
            RunResetScenario(output);
            RunPorcelainHoundRegression(output);
            RunHoldAndBoundaryChecks(output, root);

            NominalFixture nominal = RunNominalFixture(output, true);
            NominalFixture repeated = RunNominalFixture(new VerificationOutput(), false);
            output.Check(
                "fixture.repeat.canonical",
                nominal.Snapshot.FullCanonicalSignature
                    == repeated.Snapshot.FullCanonicalSignature);
            output.Check(
                "fixture.repeat.damage_rows",
                RowsEqual(nominal.DamageRows, repeated.DamageRows));
            output.Check(
                "fixture.repeat.action_rows",
                RowsEqual(nominal.ActionRows, repeated.ActionRows));
            output.Check(
                "fixture.repeat.checkpoint_rows",
                RowsEqual(nominal.CheckpointRows, repeated.CheckpointRows));
            if (output.Errors.Count == 0)
            {
                Console.WriteLine(PassMarker);
#if UNITY_EDITOR
                Debug.Log(PassMarker);
#endif
                return 0;
            }

            string failure = string.Format(
                CultureInfo.InvariantCulture,
                "C1_SHATTERED_HOST_PLAYABLE_TUNING_AND_SKILL01_FAIL errors={0}",
                output.Errors.Count);
            Console.WriteLine(failure);
            foreach (string error in output.Errors)
            {
                Console.WriteLine(error);
            }
#if UNITY_EDITOR
            Debug.LogError(failure + "\n" + string.Join("\n", output.Errors));
#endif
            return 1;
        }

        private static void RunCatalogAndSchema(VerificationOutput output, string root)
        {
            IReadOnlyList<C1EnemyRuntimeProfileSnapshot> profiles =
                C1EnemyRuntimeCatalog.GetProfiles();
            IReadOnlyList<C1EnemyActionPatternSnapshot> actions =
                C1EnemyRuntimeCatalog.GetActionPatterns();
            C1EnemyRuntimeProfileSnapshot host = C1EnemyRuntimeCatalog.FindProfile(
                C1EnemyRuntimeContract.ShatteredHostProfileId);
            C1EnemyRuntimeProfileSnapshot hound = C1EnemyRuntimeCatalog.FindProfile(
                C1EnemyRuntimeContract.PorcelainHoundProfileId);
            C1EnemyActionPatternSnapshot basic = C1EnemyRuntimeCatalog.FindAction(
                C1EnemyRuntimeContract.ShatteredHostActionId);
            C1EnemyActionPatternSnapshot skill = C1EnemyRuntimeCatalog.FindAction(
                C1EnemyRuntimeContract.ShatteredHostSkillActionId);
            C1EnemyActionPatternSnapshot charge = C1EnemyRuntimeCatalog.FindAction(
                C1EnemyRuntimeContract.PorcelainHoundActionId);

            output.Check(
                "schema.v2",
                C1EnemyRuntimeContract.SchemaId == "BoneAspectC1EnemyRuntime.v2"
                    && C1EnemyRuntimeContract.SchemaVersion == 2);
            output.Check(
                "cue.enum.old_values_stable",
                (int)C1EnemyCueKind.Presence == 0
                    && (int)C1EnemyCueKind.BasicAttack == 1
                    && (int)C1EnemyCueKind.ChargeAttack == 2
                    && (int)C1EnemyCueKind.Hit == 3
                    && (int)C1EnemyCueKind.ShellHit == 4
                    && (int)C1EnemyCueKind.ShellBreak == 5
                    && (int)C1EnemyCueKind.CounterWindowRequested == 6
                    && (int)C1EnemyCueKind.Defeated == 7
                    && (int)C1EnemyCueKind.PostDefeatFieldRequested == 8
                    && (int)C1EnemyCueKind.Reset == 9);
            output.Check("cue.enum.skill_appended", (int)C1EnemyCueKind.Skill == 10);
            output.Check("catalog.profile_count", profiles.Count == 2);
            output.Check("catalog.action_count", actions.Count == 3);
            output.Check(
                "catalog.profile_valid",
                profiles.All(profile =>
                    C1EnemyRuntimeValidation.ValidateProfile(profile).Count == 0));
            output.Check(
                "catalog.action_valid",
                actions.All(action =>
                    C1EnemyRuntimeValidation.ValidateAction(action).Count == 0));
            output.Check(
                "host.profile",
                host != null
                    && host.ContentId == C1EnemyRuntimeContract.ShatteredHostContentId
                    && host.MaxHp == 650
                    && host.ShellMax == 0
                    && host.InitialShell == 0
                    && host.ShellRegenerationCount == 0
                    && host.DeathPresentationHoldRecommendationTicks == 1500
                    && host.ActionPatternIds.SequenceEqual(new[]
                    {
                        C1EnemyRuntimeContract.ShatteredHostActionId,
                        C1EnemyRuntimeContract.ShatteredHostSkillActionId
                    }));
            output.Check(
                "host.devonly_disabled",
                host.DevOnlyFixture
                    && !host.FormalBalance
                    && !host.LiveTuning
                    && host.DevOnly
                    && !host.IsEnabled
                    && !host.EntersFormalFlow
                    && !host.RuntimeBoundToBattle);
            output.Check(
                "host.basic.exact",
                basic != null
                    && basic.OwnerRuntimeProfileId
                        == C1EnemyRuntimeContract.ShatteredHostProfileId
                    && basic.CueKind == C1EnemyCueKind.BasicAttack
                    && basic.MechanicKey == "mechanic.basic_pressure"
                    && basic.EffectRequestKey
                        == C1EnemyRuntimeContract.DirectPlayerDamageRequest
                    && basic.FirstDueTick == 2000L
                    && basic.RepeatIntervalTicks == 5000L
                    && basic.TelegraphTicks == 500L
                    && basic.CastTicks == 300L
                    && basic.ResolveOffsetTicks == 800L
                    && basic.RecoverTicks == 700L
                    && basic.Priority == 100
                    && basic.EffectRequestLifetimeClass == string.Empty
                    && basic.RequestedDurationTicks == 0);
            output.Check(
                "host.skill.exact",
                skill != null
                    && skill.OwnerRuntimeProfileId
                        == C1EnemyRuntimeContract.ShatteredHostProfileId
                    && skill.CueKind == C1EnemyCueKind.Skill
                    && skill.MechanicKey == "mechanic.polluted_tile"
                    && skill.EffectRequestKey
                        == C1EnemyRuntimeContract.PollutedPulseSkillRequest
                    && skill.FirstDueTick == 5500L
                    && skill.RepeatIntervalTicks == 9000L
                    && skill.TelegraphTicks == 750L
                    && skill.CastTicks == 500L
                    && skill.ResolveOffsetTicks == 1250L
                    && skill.RecoverTicks == 750L
                    && skill.Priority == 200
                    && skill.EffectRequestLifetimeClass == string.Empty
                    && skill.RequestedDurationTicks == 0);
            output.Check(
                "hound.profile.exact",
                hound != null
                    && hound.MaxHp == 180
                    && hound.ShellMax == 100
                    && hound.InitialShell == 100
                    && hound.ActionPatternIds.SequenceEqual(
                        new[] { C1EnemyRuntimeContract.PorcelainHoundActionId }));
            output.Check(
                "hound.action.exact",
                charge != null
                    && charge.CueKind == C1EnemyCueKind.ChargeAttack
                    && charge.FirstDueTick == 2500L
                    && charge.RepeatIntervalTicks == 6000L
                    && charge.TelegraphTicks == 800L
                    && charge.CastTicks == 400L
                    && charge.ResolveOffsetTicks == 1200L
                    && charge.RecoverTicks == 900L
                    && charge.EffectRequestKey
                        == C1EnemyRuntimeContract.ChargeAttackRequest);
            output.Check(
                "profile.defensive_copy",
                profiles is ReadOnlyCollection<C1EnemyRuntimeProfileSnapshot>);
            output.Check(
                "action.defensive_copy",
                actions is ReadOnlyCollection<C1EnemyActionPatternSnapshot>);

            string scheduler = ReadUtf8(Combine(
                root,
                "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyActionScheduler.cs"));
            output.Check(
                "scheduler.collision_order.source",
                scheduler.Contains(".OrderBy(schedule => schedule.NextDueTick)")
                    && scheduler.Contains(
                        ".ThenByDescending(schedule => byId[schedule.ActionPatternId].Priority)")
                    && scheduler.Contains(
                        ".ThenBy(schedule => schedule.ActionPatternId, StringComparer.Ordinal)"));
        }

        private static void RunSchedulerAndCanonicalScenarios(
            VerificationOutput output,
            string root)
        {
            C1EnemyRuntimeSnapshot host = ActivateHost("scheduler-host");
            output.Check(
                "schedule.initial.exact",
                Schedule(host, C1EnemyRuntimeContract.ShatteredHostActionId).NextDueTick
                    == 2000L
                    && Schedule(
                        host,
                        C1EnemyRuntimeContract.ShatteredHostSkillActionId).NextDueTick
                    == 5500L
                    && host.NextActionDueTick == 2000L);
            output.Check(
                "schedule.ordinal",
                host.ActionScheduleStates.Select(value => value.ActionPatternId)
                    .SequenceEqual(host.ActionScheduleStates
                        .Select(value => value.ActionPatternId)
                        .OrderBy(value => value, StringComparer.Ordinal)));
            bool scheduleMutationBlocked = false;
            try
            {
                ((IList<C1EnemyActionScheduleStateSnapshot>)host.ActionScheduleStates).Add(
                    new C1EnemyActionScheduleStateSnapshot("mutated", 0L, 0, -1L));
            }
            catch (NotSupportedException)
            {
                scheduleMutationBlocked = true;
            }
            output.Check("schedule.defensive_copy", scheduleMutationBlocked);

            string beforeReactive = host.FullCanonicalSignature;
            long beforeReactiveRevision = host.Revision;
            C1EnemyTransitionResult reactive = C1EnemyActionScheduler.Advance(
                host,
                2000L,
                true);
            output.Check(
                "scheduler.reactive.reject_preserves",
                !reactive.Accepted
                    && reactive.Error == "REACTIVE_PRIORITY"
                    && reactive.Snapshot.FullCanonicalSignature == beforeReactive
                    && reactive.Snapshot.Revision == beforeReactiveRevision);

            host = C1EnemyActionScheduler.Advance(host, 2000L, false).Snapshot;
            output.Check(
                "scheduler.basic.start",
                host.ActiveAction != null
                    && host.ActiveAction.ActionPatternId
                        == C1EnemyRuntimeContract.ShatteredHostActionId
                    && host.ActiveAction.StartTick == 2000L
                    && host.ActiveAction.ResolveTick == 2800L
                    && host.ActiveAction.RecoveryEndTick == 3500L
                    && host.LatestCue.Kind == C1EnemyCueKind.BasicAttack
                    && Schedule(
                        host,
                        C1EnemyRuntimeContract.ShatteredHostActionId).NextDueTick
                    == 7000L
                    && Schedule(
                        host,
                        C1EnemyRuntimeContract.ShatteredHostActionId).ExecutionCount
                    == 1);
            C1EnemyActionScheduleStateSnapshot basicState = Schedule(
                host,
                C1EnemyRuntimeContract.ShatteredHostActionId);
            output.Check(
                "schedule.start_fields",
                basicState.ExecutionCount == 1
                    && basicState.LastStartTick == 2000L
                    && basicState.NextDueTick == 7000L);

            host = C1EnemyRuntimeReducer.ApplyBattleApplication(
                host,
                App("scheduler-hit", 1L, 0, 2100L, "scheduler-host", true, 1, 0))
                .Snapshot;
            output.Check(
                "scheduler.hit_does_not_shift",
                host.ActiveAction != null
                    && host.ActiveAction.ActionPatternId
                        == C1EnemyRuntimeContract.ShatteredHostActionId
                    && Schedule(
                        host,
                        C1EnemyRuntimeContract.ShatteredHostActionId).NextDueTick
                    == 7000L
                    && host.LatestCue.Kind == C1EnemyCueKind.Hit);
            host = C1EnemyActionScheduler.Advance(host, 2800L, false).Snapshot;
            output.Check(
                "scheduler.resolve.authoritative_tick",
                host.PendingRequests.Count(request =>
                    request.RequestKey
                        == C1EnemyRuntimeContract.DirectPlayerDamageRequest
                    && request.BattleTick == 2800L) == 1);
            string resolvedSignature = host.FullCanonicalSignature;
            host = C1EnemyActionScheduler.Advance(host, 2800L, false).Snapshot;
            output.Check(
                "scheduler.resolve.once",
                host.PendingRequests.Count(request =>
                    request.RequestKey
                        == C1EnemyRuntimeContract.DirectPlayerDamageRequest) == 1
                    && host.FullCanonicalSignature != resolvedSignature);

            C1EnemyRuntimeSnapshot jump = ActivateHost("large-jump-host");
            jump = C1EnemyActionScheduler.Advance(jump, 2000L, false).Snapshot;
            jump = C1EnemyActionScheduler.Advance(jump, 14500L, false).Snapshot;
            output.Check(
                "scheduler.large_jump.closes_and_selects",
                jump.PendingRequests.Count(request =>
                    request.RequestKey
                        == C1EnemyRuntimeContract.DirectPlayerDamageRequest
                    && request.BattleTick == 2800L) == 1
                    && jump.ActiveAction != null
                    && jump.ActiveAction.ActionPatternId
                        == C1EnemyRuntimeContract.ShatteredHostSkillActionId
                    && jump.ActiveAction.StartTick == 14500L);
            output.Check(
                "scheduler.large_jump.due_preserved",
                Schedule(
                    jump,
                    C1EnemyRuntimeContract.ShatteredHostActionId).NextDueTick == 7000L
                    && Schedule(
                        jump,
                        C1EnemyRuntimeContract.ShatteredHostSkillActionId).NextDueTick
                    == 14500L);
            jump = C1EnemyActionScheduler.Advance(jump, 15750L, false).Snapshot;
            jump = C1EnemyActionScheduler.Advance(jump, 16500L, false).Snapshot;
            output.Check(
                "scheduler.large_jump.no_drop",
                jump.PendingRequests.Count(request =>
                    request.RequestKey
                        == C1EnemyRuntimeContract.PollutedPulseSkillRequest
                    && request.BattleTick == 15750L) == 1
                    && jump.ActiveAction != null
                    && jump.ActiveAction.ActionPatternId
                        == C1EnemyRuntimeContract.ShatteredHostActionId
                    && Schedule(
                        jump,
                        C1EnemyRuntimeContract.ShatteredHostActionId).ExecutionCount
                    == 2);

            C1EnemyRuntimeProfileSnapshot original =
                C1EnemyRuntimeCatalog.FindProfile(
                    C1EnemyRuntimeContract.ShatteredHostProfileId);
            C1EnemyRuntimeProfileSnapshot reversed = new C1EnemyRuntimeProfileSnapshot(
                original.RuntimeProfileId,
                original.ContentId,
                original.DisplayName,
                original.PresentationKey,
                original.MechanicKeys.Reverse(),
                original.ActionPatternIds.Reverse(),
                original.MaxHp,
                original.ShellMax,
                original.InitialShell,
                original.ShellRegenerationCount,
                original.PostDefeatEffectRequestKey,
                original.ShellBreakCounterWindowKey,
                original.RequestedCounterWindowTicks,
                original.DeathPresentationHoldRecommendationTicks,
                original.DevOnlyFixture,
                original.FormalBalance,
                original.DevOnly,
                original.IsEnabled,
                original.EntersFormalFlow,
                original.RuntimeBoundToBattle);
            output.Check(
                "canonical.profile.input_order",
                reversed.CanonicalSignature == original.CanonicalSignature);
            output.Check(
                "canonical.schedule_fields_sensitive",
                beforeReactive != host.FullCanonicalSignature
                    && beforeReactive != jump.FullCanonicalSignature
                    && host.FullCanonicalSignature != jump.FullCanonicalSignature);
            output.Check(
                "canonical.presentation_excludes_diagnostic",
                C1EnemyRuntimeReducer.AddDeveloperDiagnosticForFixture(
                    host,
                    "developer-only").Snapshot.PresentationSafeCanonicalSignature
                    == host.PresentationSafeCanonicalSignature);

            CultureInfo previousCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                C1EnemyActionPatternSnapshot cultureAction =
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
                        true);
                output.Check(
                    "canonical.culture_invariant",
                    cultureAction.CanonicalSignature
                        == C1EnemyRuntimeCatalog.FindAction(
                            C1EnemyRuntimeContract.ShatteredHostSkillActionId)
                            .CanonicalSignature);
            }
            finally
            {
                CultureInfo.CurrentCulture = previousCulture;
            }

            string primitives = ReadUtf8(Combine(
                root,
                "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimePrimitives.cs"));
            output.Check(
                "schema.public_v1_fields_preserved",
                primitives.Contains("public long LastSchedulerTick")
                    && primitives.Contains("public C1EnemyActiveActionSnapshot ActiveAction")
                    && primitives.Contains("public long LastAcceptedApplicationSequence"));
        }

        private static void RunApplicationNegativeScenarios(VerificationOutput output)
        {
            C1EnemyRuntimeSnapshot baseState = ActivateHost("negative-host");
            Negative(
                output,
                "wrong_target",
                baseState,
                App("wrong", 1L, 0, 1L, "other", true, 1, 0),
                "WRONG_TARGET");
            Negative(
                output,
                "stale_generation",
                baseState,
                App("stale", 1L, 1, 1L, "negative-host", true, 1, 0),
                "STALE_GENERATION");
            Negative(
                output,
                "ledger_rejected",
                baseState,
                App("rejected", 1L, 0, 1L, "negative-host", false, 1, 0),
                "BATTLE_LEDGER_REJECTED");
            Negative(
                output,
                "zero_delta",
                baseState,
                App("zero", 1L, 0, 1L, "negative-host", true, 0, 0),
                "ZERO_DELTA");
            Negative(
                output,
                "negative_delta",
                baseState,
                App("negative", 1L, 0, 1L, "negative-host", true, -1, 0),
                "NEGATIVE_DELTA");
            Negative(
                output,
                "host_shell",
                baseState,
                App("host-shell", 1L, 0, 1L, "negative-host", true, 0, 1),
                "HOST_SHELL_DELTA_FORBIDDEN");
            Negative(
                output,
                "hp_beyond_current",
                baseState,
                App("too-much", 1L, 0, 1L, "negative-host", true, 651, 0),
                "HP_DELTA_BEYOND_CURRENT");

            C1EnemyBattleApplicationResult acceptedApp = App(
                "duplicate",
                1L,
                0,
                1L,
                "negative-host",
                true,
                1,
                0);
            C1EnemyRuntimeSnapshot after = C1EnemyRuntimeReducer.ApplyBattleApplication(
                baseState,
                acceptedApp).Snapshot;
            Negative(
                output,
                "duplicate_event",
                after,
                App("duplicate", 2L, 0, 2L, "negative-host", true, 1, 0),
                "APPLICATION_EVENT_DUPLICATE");
            Negative(
                output,
                "sequence_regression",
                after,
                App("sequence", 1L, 0, 2L, "negative-host", true, 1, 0),
                "APPLICATION_SEQUENCE_NOT_MONOTONIC");
            Negative(
                output,
                "tick_regression",
                after,
                App("tick", 2L, 0, 1L, "negative-host", true, 1, 0),
                "BATTLE_TICK_NOT_MONOTONIC");

            C1EnemyRuntimeSnapshot defeated = ActivateHost("negative-defeated");
            defeated = C1EnemyRuntimeReducer.ApplyBattleApplication(
                defeated,
                App("lethal", 1L, 0, 1L, "negative-defeated", true, 650, 0))
                .Snapshot;
            Negative(
                output,
                "post_defeat",
                defeated,
                App("post", 2L, 0, 2L, "negative-defeated", true, 1, 0),
                "ACTIVE_STATE_REQUIRED");
            C1EnemyTransitionResult postScheduler =
                C1EnemyActionScheduler.Advance(defeated, 22000L, false);
            output.Negative(
                "scheduler_post_defeat",
                "DEFEATED",
                postScheduler.Error,
                postScheduler.Accepted ? 1 : 0,
                postScheduler.Snapshot.FullCanonicalSignature
                    == defeated.FullCanonicalSignature ? 0 : 1,
                postScheduler.Snapshot.Cues.Count - defeated.Cues.Count,
                postScheduler.Snapshot.PendingRequests.Count
                    - defeated.PendingRequests.Count,
                !postScheduler.Accepted
                    && postScheduler.Error == "DEFEATED"
                    && postScheduler.Snapshot.FullCanonicalSignature
                        == defeated.FullCanonicalSignature);
        }

        private static void RunResetScenario(VerificationOutput output)
        {
            C1EnemyRuntimeSnapshot host = ActivateHost("reset-host");
            host = C1EnemyActionScheduler.Advance(host, 2000L, false).Snapshot;
            host = C1EnemyRuntimeReducer.ApplyBattleApplication(
                host,
                App("reusable-event", 1L, 0, 2100L, "reset-host", true, 10, 0))
                .Snapshot;
            C1EnemyTransitionResult resetResult =
                C1EnemyRuntimeReducer.Reset(host, 10000L);
            C1EnemyRuntimeSnapshot reset = resetResult.Snapshot;
            output.Check(
                "reset.generation_and_identity",
                resetResult.Accepted
                    && reset.ResetGeneration == 1
                    && reset.AcceptedApplicationCount == 0
                    && reset.AcceptedApplicationEventIds.Count == 0
                    && reset.ActiveAction == null
                    && reset.LatestCue.Kind == C1EnemyCueKind.Reset);
            output.Check(
                "reset.action_schedules",
                Schedule(
                    reset,
                    C1EnemyRuntimeContract.ShatteredHostActionId).NextDueTick
                    == 12000L
                    && Schedule(
                        reset,
                        C1EnemyRuntimeContract.ShatteredHostSkillActionId).NextDueTick
                    == 15500L
                    && reset.ActionScheduleStates.All(schedule =>
                        schedule.ExecutionCount == 0
                        && schedule.LastStartTick == -1L));
            reset = C1EnemyRuntimeReducer.Activate(reset, 10000L).Snapshot;
            C1EnemyTransitionResult reused =
                C1EnemyRuntimeReducer.ApplyBattleApplication(
                    reset,
                    App(
                        "reusable-event",
                        1L,
                        1,
                        10001L,
                        "reset-host",
                        true,
                        10,
                        0));
            output.Check(
                "reset.dedupe_cleared",
                reused.Accepted
                    && reused.Snapshot.AcceptedApplicationCount == 1
                    && reused.Snapshot.Cues.Last().Kind == C1EnemyCueKind.Hit);
        }

        private static void RunPorcelainHoundRegression(VerificationOutput output)
        {
            C1EnemyRuntimeSnapshot hound = ActivateHound("hound-regression");
            output.Check(
                "hound.initial",
                hound.MaxHp == 180
                    && hound.CurrentHp == 180
                    && hound.ShellMax == 100
                    && hound.CurrentShell == 100
                    && hound.ShellState == C1EnemyShellState.Intact);
            C1EnemyTransitionResult guarded =
                C1EnemyRuntimeReducer.ApplyBattleApplication(
                    hound,
                    App("hound-guarded", 1L, 0, 100L, "hound-regression", true, 1, 0));
            output.Check(
                "hound.shell_guards_hp",
                !guarded.Accepted
                    && guarded.Error == "HOUND_HP_GUARDED_BY_SHELL");
            hound = C1EnemyRuntimeReducer.ApplyBattleApplication(
                hound,
                App("hound-shell-1", 1L, 0, 100L, "hound-regression", true, 0, 40))
                .Snapshot;
            hound = C1EnemyRuntimeReducer.ApplyBattleApplication(
                hound,
                App("hound-shell-2", 2L, 0, 200L, "hound-regression", true, 0, 60))
                .Snapshot;
            output.Check(
                "hound.shell_break",
                hound.CurrentShell == 0
                    && hound.ShellState == C1EnemyShellState.Broken
                    && hound.Cues.Count(cue => cue.Kind == C1EnemyCueKind.ShellBreak)
                        == 1
                    && hound.PendingRequests.Count(request =>
                        request.RequestKey
                            == C1EnemyRuntimeContract.ShellBreakCounterWindow
                        && request.RequestedDurationTicks == 3000) == 1);
            hound = C1EnemyRuntimeReducer.ApplyBattleApplication(
                hound,
                App("hound-hp", 3L, 0, 300L, "hound-regression", true, 10, 0))
                .Snapshot;
            output.Check(
                "hound.post_shell_hp",
                hound.CurrentHp == 170
                    && hound.LatestCue.Kind == C1EnemyCueKind.Hit);

            C1EnemyRuntimeSnapshot actionHound = ActivateHound("hound-action");
            actionHound = C1EnemyActionScheduler.Advance(
                actionHound,
                2500L,
                false).Snapshot;
            output.Check(
                "hound.action_start",
                actionHound.ActiveAction != null
                    && actionHound.ActiveAction.ActionPatternId
                        == C1EnemyRuntimeContract.PorcelainHoundActionId
                    && actionHound.ActiveAction.ResolveTick == 3700L
                    && actionHound.ActiveAction.RecoveryEndTick == 4600L
                    && actionHound.LatestCue.Kind == C1EnemyCueKind.ChargeAttack);
            actionHound = C1EnemyActionScheduler.Advance(
                actionHound,
                3700L,
                false).Snapshot;
            output.Check(
                "hound.action_resolve",
                actionHound.PendingRequests.Count(request =>
                    request.RequestKey == C1EnemyRuntimeContract.ChargeAttackRequest
                    && request.BattleTick == 3700L) == 1);
            actionHound = C1EnemyActionScheduler.Advance(
                actionHound,
                4600L,
                false).Snapshot;
            actionHound = C1EnemyActionScheduler.Advance(
                actionHound,
                8500L,
                false).Snapshot;
            output.Check(
                "hound.action_repeat",
                actionHound.ActiveAction != null
                    && actionHound.ActiveAction.StartTick == 8500L
                    && Schedule(
                        actionHound,
                        C1EnemyRuntimeContract.PorcelainHoundActionId)
                        .ExecutionCount == 2);
            output.Check(
                "hound.runtime_valid",
                C1EnemyRuntimeValidation.Validate(hound).Count == 0
                    && C1EnemyRuntimeValidation.Validate(actionHound).Count == 0);
        }

        private static void RunHoldAndBoundaryChecks(
            VerificationOutput output,
            string root)
        {
            IReadOnlyList<C1EnemyHoldAssertion> holds =
                C1EnemyRuntimeCatalog.GetHoldAssertions();
            C1EnemyHoldAssertion hold = holds.Single();
            output.Check(
                "bad3.hold.exact",
                holds.Count == 1
                    && hold.ContentId
                        == C1EnemyRuntimeContract.BoneSwapRemnantContentId
                    && hold.Status == C1EnemyRuntimeContract.BoneSwapRemnantHoldStatus
                    && hold.RuntimeProfiles == 0
                    && hold.ActionPatterns == 0
                    && hold.CopySlots == 0
                    && hold.RuntimeBindings == 0);
            output.Check(
                "bad3.no_profile_or_action",
                C1EnemyRuntimeCatalog.GetProfiles().All(profile =>
                    profile.ContentId
                        != C1EnemyRuntimeContract.BoneSwapRemnantContentId)
                    && C1EnemyRuntimeCatalog.GetActionPatterns().All(action =>
                        !action.ActionPatternId.Contains(
                            "bone_swap",
                            StringComparison.OrdinalIgnoreCase)));

            string joinedRuntimeSources = string.Join(
                "\n",
                SourcePaths.Select(path => ReadUtf8(Combine(root, path))));
            output.Check(
                "boundary.no_unity_runtime_dependency",
                SourcePaths.All(path =>
                    !ReadUtf8(Combine(root, path)).Contains("UnityEngine")));
            output.Check(
                "boundary.no_formal_runtime_types",
                !joinedRuntimeSources.Contains("EnemyDefinition")
                    && !joinedRuntimeSources.Contains("AutoCombat")
                    && !joinedRuntimeSources.Contains("RunFlowController")
                    && !joinedRuntimeSources.Contains("SaveData")
                    && !joinedRuntimeSources.Contains("InventoryManager")
                    && !joinedRuntimeSources.Contains("RewardConfig")
                    && !joinedRuntimeSources.Contains("DropTable"));
            output.Check(
                "boundary.no_bad4_false_clone",
                !joinedRuntimeSources.Contains("BA-D4")
                    && !joinedRuntimeSources.Contains("FALSE_CLONE")
                    && !joinedRuntimeSources.Contains("守骨奴")
                    && !joinedRuntimeSources.Contains("竞骨人"));
            output.Check(
                "boundary.zero_writes_declared",
                C1EnemyRuntimeContract.RuntimeBoundToBattle == false
                    && C1EnemyRuntimeContract.EntersFormalFlow == false);
        }

        private static NominalFixture RunNominalFixture(
            VerificationOutput output,
            bool assert)
        {
            NominalFixture fixture = new NominalFixture();
            fixture.Snapshot = ActivateHost("nominal-host");
            if (assert)
            {
            }

            Damage(fixture, 1500L, 1, 29);
            Action(fixture, 2000L);
            Action(fixture, 2800L);
            Damage(fixture, 3000L, 2, 42);
            Action(fixture, 3500L);
            Damage(fixture, 4500L, 3, 53);
            Action(fixture, 5500L);
            Damage(fixture, 6000L, 4, 46);
            Action(fixture, 6750L);
            Action(fixture, 7500L);
            Damage(fixture, 7500L, 5, 55);
            Action(fixture, 8300L);
            Action(fixture, 9000L);
            Damage(fixture, 9000L, 6, 76);
            Damage(fixture, 10500L, 7, 29);
            Action(fixture, 12000L);
            Damage(fixture, 12000L, 8, 42);
            Action(fixture, 12800L);
            Action(fixture, 13500L);
            Damage(fixture, 13500L, 9, 53);
            Action(fixture, 14500L);
            Damage(fixture, 15000L, 10, 46);
            Checkpoint(fixture, 15, 15000L);
            Action(fixture, 15750L);
            Action(fixture, 16500L);
            Damage(fixture, 16500L, 11, 55);
            Action(fixture, 17000L);
            Action(fixture, 17800L);
            Damage(fixture, 18000L, 12, 76);
            Checkpoint(fixture, 18, 18000L);
            Action(fixture, 18500L);
            Damage(fixture, 19500L, 13, 29);
            Checkpoint(fixture, 20, 20000L);
            Damage(fixture, 21000L, 14, 42);
            fixture.LethalTick = 21000L;
            Checkpoint(fixture, 22, 22000L);
            Checkpoint(fixture, 25, 25000L);

            if (assert)
            {
                int basic = fixture.Snapshot.PendingRequests.Count(request =>
                    request.RequestKey
                        == C1EnemyRuntimeContract.DirectPlayerDamageRequest);
                int skill = fixture.Snapshot.PendingRequests.Count(request =>
                    request.RequestKey
                        == C1EnemyRuntimeContract.PollutedPulseSkillRequest);
                output.Check(
                    "nominal.damage.exact",
                    fixture.Snapshot.AcceptedApplicationCount == 14
                        && fixture.RawOfferedDamage == 673
                        && fixture.AcceptedDamage == 650
                        && fixture.Snapshot.CurrentHp == 0);
                output.Check(
                    "nominal.lethal.exact",
                    fixture.LethalTick == 21000L
                        && fixture.Snapshot.Lifecycle == C1EnemyLifecycle.Defeated
                        && !fixture.Snapshot.Targetable
                        && fixture.Snapshot.ActiveAction == null);
                output.Check(
                    "nominal.actions.exact",
                    basic == 4
                        && skill == 2
                        && fixture.ActionRows.Count == 6
                        && fixture.ActionRows.Count(row => row[1] == "BasicAttack") == 4
                        && fixture.ActionRows.Count(row => row[1] == "Skill") == 2);
                output.Check(
                    "nominal.resolve.authoritative",
                    fixture.ActionRows.All(row => row[5] == row[8]));
                output.Check(
                    "nominal.hit.exact",
                    fixture.Snapshot.Cues.Count(cue => cue.Kind == C1EnemyCueKind.Hit)
                        == 14);
                output.Check(
                    "nominal.post_defeat_field.once",
                    fixture.Snapshot.PendingRequests.Count(request =>
                        request.RequestKey
                            == C1EnemyRuntimeContract.PostDefeatFieldRequest) == 1);
                output.Check(
                    "nominal.no_post_defeat_action",
                    fixture.Snapshot.PendingRequests.Count(request =>
                        (request.RequestKey
                            == C1EnemyRuntimeContract.DirectPlayerDamageRequest
                            || request.RequestKey
                                == C1EnemyRuntimeContract.PollutedPulseSkillRequest)
                        && request.BattleTick > 21000L) == 0);
                output.Check(
                    "nominal.death_hold.presentation_only",
                    fixture.Snapshot.Lifecycle == C1EnemyLifecycle.Defeated
                        && fixture.Snapshot.LastAcceptedBattleTick == 21000L
                        && C1EnemyRuntimeContract
                            .DeathPresentationHoldRecommendationTicks == 1500
                        && 21000L
                            + C1EnemyRuntimeContract
                                .DeathPresentationHoldRecommendationTicks
                            == 22500L);
                output.Check(
                    "nominal.checkpoints.exact",
                    CheckpointRowsExact(fixture.CheckpointRows));
                output.Check(
                    "nominal.duration.acceptance",
                    fixture.LethalTick >= 18000L
                        && fixture.LethalTick <= 22000L
                        && fixture.LethalTick >= 15000L
                        && fixture.LethalTick <= 25000L);
                output.Check(
                    "nominal.runtime_valid",
                    C1EnemyRuntimeValidation.Validate(fixture.Snapshot).Count == 0);
            }
            return fixture;
        }

        private static void Damage(
            NominalFixture fixture,
            long tick,
            int eventOrdinal,
            int offered)
        {
            int before = fixture.Snapshot.CurrentHp;
            int accepted = Math.Min(offered, before);
            C1EnemyBattleApplicationResult application = App(
                "nominal-damage-" + eventOrdinal.ToString(
                    "D2",
                    CultureInfo.InvariantCulture),
                eventOrdinal,
                fixture.Snapshot.ResetGeneration,
                tick,
                fixture.Snapshot.EnemyInstanceId,
                true,
                accepted,
                0);
            C1EnemyTransitionResult result =
                C1EnemyRuntimeReducer.ApplyBattleApplication(
                    fixture.Snapshot,
                    application);
            if (!result.Accepted)
            {
                throw new InvalidOperationException(
                    "Nominal damage application rejected: " + result.Error);
            }
            fixture.Snapshot = result.Snapshot;
            fixture.RawOfferedDamage += offered;
            fixture.AcceptedDamage += accepted;
            fixture.DamageRows.Add(new[]
            {
                tick.ToString(CultureInfo.InvariantCulture),
                eventOrdinal.ToString(CultureInfo.InvariantCulture),
                "I" + (6 + ((eventOrdinal - 1) % 6) + 1).ToString(
                    "D3",
                    CultureInfo.InvariantCulture),
                offered.ToString(CultureInfo.InvariantCulture),
                accepted.ToString(CultureInfo.InvariantCulture),
                fixture.AcceptedDamage.ToString(CultureInfo.InvariantCulture),
                fixture.RawOfferedDamage.ToString(CultureInfo.InvariantCulture),
                fixture.Snapshot.CurrentHp.ToString(CultureInfo.InvariantCulture),
                fixture.Snapshot.Lifecycle.ToString(),
                fixture.Snapshot.LatestCue.Kind.ToString(),
                fixture.Snapshot.FullCanonicalSignature
            });
        }

        private static void Action(NominalFixture fixture, long tick)
        {
            int cueCount = fixture.Snapshot.Cues.Count;
            int requestCount = fixture.Snapshot.PendingRequests.Count;
            C1EnemyTransitionResult result =
                C1EnemyActionScheduler.Advance(fixture.Snapshot, tick, false);
            if (!result.Accepted)
            {
                throw new InvalidOperationException(
                    "Nominal scheduler advance rejected: " + result.Error);
            }
            fixture.Snapshot = result.Snapshot;
            if (fixture.Snapshot.Cues.Count > cueCount
                && fixture.Snapshot.ActiveAction != null)
            {
                C1EnemyCueSnapshot cue = fixture.Snapshot.Cues.Last();
                if (cue.Kind == C1EnemyCueKind.BasicAttack
                    || cue.Kind == C1EnemyCueKind.Skill)
                {
                    C1EnemyActiveActionSnapshot active = fixture.Snapshot.ActiveAction;
                    fixture.ActionRows.Add(new[]
                    {
                        active.ExecutionId,
                        cue.Kind.ToString(),
                        active.ActionPatternId,
                        active.StartTick.ToString(CultureInfo.InvariantCulture),
                        active.ResolveTick.ToString(CultureInfo.InvariantCulture),
                        active.ResolveTick.ToString(CultureInfo.InvariantCulture),
                        active.RecoveryEndTick.ToString(CultureInfo.InvariantCulture),
                        C1EnemyRuntimeCatalog.FindAction(active.ActionPatternId)
                            .EffectRequestKey,
                        string.Empty,
                        "PASS"
                    });
                }
            }
            if (fixture.Snapshot.PendingRequests.Count > requestCount)
            {
                C1EnemyEffectRequestSnapshot request =
                    fixture.Snapshot.PendingRequests.Last();
                string[] row = fixture.ActionRows.LastOrDefault(value =>
                    value[0] == request.SourceIdentity);
                if (row != null)
                {
                    row[8] = request.BattleTick.ToString(CultureInfo.InvariantCulture);
                }
            }
        }

        private static void Checkpoint(
            NominalFixture fixture,
            int seconds,
            long tick)
        {
            int basic = fixture.Snapshot.PendingRequests.Count(request =>
                request.RequestKey
                    == C1EnemyRuntimeContract.DirectPlayerDamageRequest);
            int skill = fixture.Snapshot.PendingRequests.Count(request =>
                request.RequestKey
                    == C1EnemyRuntimeContract.PollutedPulseSkillRequest);
            bool holdActive = fixture.Snapshot.Lifecycle == C1EnemyLifecycle.Defeated
                && tick < 21000L
                    + C1EnemyRuntimeContract.DeathPresentationHoldRecommendationTicks;
            fixture.CheckpointRows.Add(new[]
            {
                seconds.ToString(CultureInfo.InvariantCulture),
                tick.ToString(CultureInfo.InvariantCulture),
                fixture.Snapshot.Lifecycle.ToString(),
                fixture.Snapshot.CurrentHp.ToString(CultureInfo.InvariantCulture),
                fixture.Snapshot.AcceptedApplicationCount.ToString(
                    CultureInfo.InvariantCulture),
                fixture.AcceptedDamage.ToString(CultureInfo.InvariantCulture),
                fixture.RawOfferedDamage.ToString(CultureInfo.InvariantCulture),
                basic.ToString(CultureInfo.InvariantCulture),
                skill.ToString(CultureInfo.InvariantCulture),
                holdActive ? "true" : "false",
                "21.0",
                seconds == 15 || seconds == 18 || seconds == 20
                    || seconds == 22 || seconds == 25 ? "PASS" : "FAIL"
            });
        }

        private static bool CheckpointRowsExact(IReadOnlyList<string[]> rows)
        {
            string[][] expected =
            {
                new[] { "15", "15000", "Active", "179", "10", "471", "471", "3", "1", "false", "21.0", "PASS" },
                new[] { "18", "18000", "Active", "48", "12", "602", "602", "4", "2", "false", "21.0", "PASS" },
                new[] { "20", "20000", "Active", "19", "13", "631", "631", "4", "2", "false", "21.0", "PASS" },
                new[] { "22", "22000", "Defeated", "0", "14", "650", "673", "4", "2", "true", "21.0", "PASS" },
                new[] { "25", "25000", "Defeated", "0", "14", "650", "673", "4", "2", "false", "21.0", "PASS" }
            };
            return RowsEqual(rows, expected);
        }

        private static void Negative(
            VerificationOutput output,
            string caseId,
            C1EnemyRuntimeSnapshot snapshot,
            C1EnemyBattleApplicationResult application,
            string expectedError)
        {
            string before = snapshot.FullCanonicalSignature;
            int beforeCues = snapshot.Cues.Count;
            int beforeRequests = snapshot.PendingRequests.Count;
            C1EnemyTransitionResult result =
                C1EnemyRuntimeReducer.ApplyBattleApplication(snapshot, application);
            bool passed = !result.Accepted
                && result.Error == expectedError
                && result.Snapshot.FullCanonicalSignature == before
                && result.Snapshot.Cues.Count == beforeCues
                && result.Snapshot.PendingRequests.Count == beforeRequests;
            output.Negative(
                caseId,
                expectedError,
                result.Error,
                result.Accepted ? 1 : 0,
                result.Snapshot.FullCanonicalSignature == before ? 0 : 1,
                result.Snapshot.Cues.Count - beforeCues,
                result.Snapshot.PendingRequests.Count - beforeRequests,
                passed);
        }

        private static C1EnemyBattleApplicationResult App(
            string eventId,
            long sequence,
            int generation,
            long tick,
            string target,
            bool accepted,
            int hpDelta,
            int shellDelta)
        {
            return new C1EnemyBattleApplicationResult(
                eventId,
                sequence,
                generation,
                tick,
                target,
                accepted,
                hpDelta,
                shellDelta,
                "item-request-" + sequence.ToString(CultureInfo.InvariantCulture));
        }

        private static C1EnemyRuntimeSnapshot ActivateHost(string instanceId)
        {
            return C1EnemyRuntimeReducer.Activate(
                C1EnemyRuntimeReducer.CreatePresent(
                    C1EnemyRuntimeContract.ShatteredHostProfileId,
                    instanceId,
                    0,
                    0L),
                0L).Snapshot;
        }

        private static C1EnemyRuntimeSnapshot ActivateHound(string instanceId)
        {
            return C1EnemyRuntimeReducer.Activate(
                C1EnemyRuntimeReducer.CreatePresent(
                    C1EnemyRuntimeContract.PorcelainHoundProfileId,
                    instanceId,
                    0,
                    0L),
                0L).Snapshot;
        }

        private static C1EnemyActionScheduleStateSnapshot Schedule(
            C1EnemyRuntimeSnapshot snapshot,
            string actionPatternId)
        {
            return snapshot.ActionScheduleStates.Single(value => string.Equals(
                value.ActionPatternId,
                actionPatternId,
                StringComparison.Ordinal));
        }

        private static bool RowsEqual(
            IReadOnlyList<string[]> left,
            IReadOnlyList<string[]> right)
        {
            return left.Count == right.Count
                && left.Select(row => string.Join("\u001f", row))
                    .SequenceEqual(right.Select(row => string.Join("\u001f", row)));
        }

        private static string FindProjectRoot()
        {
            string explicitRoot = Environment.GetEnvironmentVariable(
                "TALISMANBAG_PROJECT_ROOT");
            if (!string.IsNullOrWhiteSpace(explicitRoot)
                && IsProjectRoot(explicitRoot))
            {
                return Path.GetFullPath(explicitRoot);
            }
#if UNITY_EDITOR
            string unityRoot = Directory.GetParent(Application.dataPath).FullName;
            if (IsProjectRoot(unityRoot))
            {
                return unityRoot;
            }
#endif
            DirectoryInfo cursor = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (cursor != null)
            {
                if (IsProjectRoot(cursor.FullName))
                {
                    return cursor.FullName;
                }
                cursor = cursor.Parent;
            }
            throw new DirectoryNotFoundException("TalismanBag project root not found.");
        }

        private static bool IsProjectRoot(string path)
        {
            return Directory.Exists(Path.Combine(path, "Assets"))
                && Directory.Exists(Path.Combine(path, "ProjectSettings"))
                && Directory.Exists(Path.Combine(path, "Packages"));
        }

        private static string Combine(string root, string relative)
        {
            return Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar));
        }

        private static string ReadUtf8(string path)
        {
            return File.Exists(path)
                ? File.ReadAllText(path)
                : string.Empty;
        }

        private sealed class NominalFixture
        {
            public C1EnemyRuntimeSnapshot Snapshot;
            public int RawOfferedDamage;
            public int AcceptedDamage;
            public long LethalTick;
            public readonly List<string[]> DamageRows = new List<string[]>();
            public readonly List<string[]> ActionRows = new List<string[]>();
            public readonly List<string[]> CheckpointRows = new List<string[]>();
        }

        private sealed class VerificationOutput
        {
            public readonly List<string> Errors = new List<string>();

            public void Check(string assertionId, bool passed)
            {
                if (!passed)
                {
                    Fail(assertionId);
                }
            }

            public void Negative(
                string caseId,
                string expectedError,
                string actualError,
                int accepted,
                int mutations,
                int cues,
                int requests,
                bool passed)
            {
                Check("negative." + caseId, passed);
            }

            public void Fail(string error)
            {
                Errors.Add(error);
            }
        }
    }
}
