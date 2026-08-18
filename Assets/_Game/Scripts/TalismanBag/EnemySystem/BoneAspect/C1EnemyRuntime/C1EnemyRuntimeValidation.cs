using System;
using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime
{
    public static class C1EnemyRuntimeValidation
    {
        public static IReadOnlyList<string> Validate(C1EnemyRuntimeSnapshot snapshot)
        {
            List<string> errors = new List<string>();
            if (snapshot == null)
            {
                errors.Add("STATE_NULL");
                return errors.AsReadOnly();
            }

            Require(errors, snapshot.SchemaId == C1EnemyRuntimeContract.SchemaId,
                "SCHEMA_ID_MISMATCH");
            Require(errors, snapshot.SchemaVersion == C1EnemyRuntimeContract.SchemaVersion,
                "SCHEMA_VERSION_MISMATCH");
            Require(errors, snapshot.DevOnly, "DEV_ONLY_REQUIRED");
            Require(errors, !snapshot.IsEnabled, "IS_ENABLED_MUST_BE_FALSE");
            Require(errors, !snapshot.EntersFormalFlow, "FORMAL_FLOW_MUST_BE_FALSE");
            Require(errors, !snapshot.RuntimeBoundToBattle, "BATTLE_BINDING_MUST_BE_FALSE");
            Require(errors, !string.IsNullOrWhiteSpace(snapshot.EnemyInstanceId),
                "INSTANCE_ID_REQUIRED");
            C1EnemyRuntimeProfileSnapshot profile =
                C1EnemyRuntimeCatalog.FindProfile(snapshot.RuntimeProfileId);
            Require(errors, profile != null, "PROFILE_UNKNOWN");
            Require(errors, snapshot.CurrentHp >= 0 && snapshot.CurrentHp <= snapshot.MaxHp,
                "HP_OUT_OF_RANGE");
            Require(errors,
                snapshot.CurrentShell >= 0 && snapshot.CurrentShell <= snapshot.ShellMax,
                "SHELL_OUT_OF_RANGE");
            Require(errors,
                snapshot.AcceptedApplicationEventIds
                    .Distinct(StringComparer.Ordinal).Count()
                    == snapshot.AcceptedApplicationEventIds.Count,
                "APPLICATION_DEDUPE_DUPLICATE");
            Require(errors,
                snapshot.PendingRequests.Select(item => item.RequestId)
                    .Distinct(StringComparer.Ordinal).Count()
                    == snapshot.PendingRequests.Count,
                "REQUEST_ID_DUPLICATE");
            Require(errors,
                snapshot.Cues.Select(item => item.CueId)
                    .Distinct(StringComparer.Ordinal).Count()
                    == snapshot.Cues.Count,
                "CUE_ID_DUPLICATE");
            Require(errors,
                snapshot.Cues.Select(item => item.CueSequence)
                    .SequenceEqual(snapshot.Cues.Select(item => item.CueSequence)
                        .OrderBy(value => value)),
                "CUE_SEQUENCE_NOT_MONOTONIC");
            Require(errors,
                snapshot.PendingRequests.Select(item => item.RequestSequence)
                    .SequenceEqual(snapshot.PendingRequests
                        .Select(item => item.RequestSequence)
                        .OrderBy(value => value)),
                "REQUEST_SEQUENCE_NOT_MONOTONIC");

            IReadOnlyList<C1EnemyActionPatternSnapshot> actions =
                C1EnemyRuntimeCatalog.FindActionsForProfile(snapshot.RuntimeProfileId);
            string[] expectedActionIds = actions
                .Select(action => action.ActionPatternId)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            string[] actualActionIds = snapshot.ActionScheduleStates
                .Select(schedule => schedule.ActionPatternId)
                .ToArray();
            Require(errors,
                actualActionIds.SequenceEqual(
                    actualActionIds.OrderBy(value => value, StringComparer.Ordinal)),
                "ACTION_SCHEDULE_NOT_ORDINAL");
            Require(errors,
                actualActionIds.Distinct(StringComparer.Ordinal).Count()
                    == actualActionIds.Length,
                "ACTION_SCHEDULE_DUPLICATE");
            Require(errors,
                expectedActionIds.SequenceEqual(actualActionIds),
                "ACTION_SCHEDULE_SET_MISMATCH");
            Require(errors,
                snapshot.ActionScheduleStates.All(schedule =>
                    schedule.NextDueTick >= 0L
                    && schedule.ExecutionCount >= 0
                    && (schedule.ExecutionCount == 0
                        ? schedule.LastStartTick == -1L
                        : schedule.LastStartTick >= 0L)),
                "ACTION_SCHEDULE_STATE_INVALID");
            Require(errors,
                snapshot.NextActionDueTick == snapshot.ActionScheduleStates
                    .Min(schedule => schedule.NextDueTick),
                "NEXT_ACTION_DUE_PROJECTION_INVALID");

            if (snapshot.ActiveAction != null)
            {
                Require(errors,
                    expectedActionIds.Contains(
                        snapshot.ActiveAction.ActionPatternId,
                        StringComparer.Ordinal),
                    "ACTIVE_ACTION_NOT_IN_PROFILE");
                Require(errors,
                    snapshot.ActiveAction.ResolveTick >= snapshot.ActiveAction.StartTick,
                    "ACTIVE_ACTION_RESOLVE_INVALID");
                Require(errors,
                    snapshot.ActiveAction.RecoveryEndTick
                        >= snapshot.ActiveAction.ResolveTick,
                    "ACTIVE_ACTION_RECOVERY_INVALID");
            }

            if (snapshot.ContentId == C1EnemyRuntimeContract.ShatteredHostContentId)
            {
                Require(errors, snapshot.MaxHp == 650, "HOST_HP_TUNING_MISMATCH");
                Require(errors, snapshot.ShellMax == 0 && snapshot.CurrentShell == 0,
                    "HOST_SHELL_FORBIDDEN");
                Require(errors, snapshot.ShellState == C1EnemyShellState.NotApplicable,
                    "HOST_SHELL_STATE_INVALID");
                Require(errors,
                    snapshot.SelfPossessionState
                        == C1EnemySelfPossessionState.DeclaredOwnStateOnly,
                    "HOST_POSSESSION_DECLARATION_MISSING");
                Require(errors, !snapshot.SelfPossessionGameplayEffectAuthored,
                    "HOST_POSSESSION_GAMEPLAY_EFFECT_FORBIDDEN");
            }
            else if (snapshot.ContentId == C1EnemyRuntimeContract.PorcelainHoundContentId)
            {
                Require(errors, snapshot.MaxHp == 180, "HOUND_HP_REGRESSION");
                Require(errors, snapshot.ShellMax == 100, "HOUND_SHELL_REGRESSION");
                Require(errors,
                    snapshot.SelfPossessionState
                        == C1EnemySelfPossessionState.NotApplicable,
                    "HOUND_POSSESSION_STATE_INVALID");
                Require(errors,
                    snapshot.CurrentShell > 0
                        ? snapshot.ShellState == C1EnemyShellState.Intact
                        : snapshot.ShellState == C1EnemyShellState.Broken,
                    "HOUND_SHELL_STATE_INVALID");
            }
            else
            {
                errors.Add("CONTENT_ID_UNKNOWN");
            }

            if (snapshot.Lifecycle == C1EnemyLifecycle.Defeated)
            {
                Require(errors, snapshot.CurrentHp == 0, "DEFEATED_HP_MUST_BE_ZERO");
                Require(errors, !snapshot.Targetable, "DEFEATED_TARGETABLE_FORBIDDEN");
                Require(errors, snapshot.ActiveAction == null,
                    "DEFEATED_ACTIVE_ACTION_FORBIDDEN");
            }

            return errors.AsReadOnly();
        }

        public static IReadOnlyList<string> ValidateProfile(
            C1EnemyRuntimeProfileSnapshot profile)
        {
            List<string> errors = new List<string>();
            if (profile == null)
            {
                errors.Add("PROFILE_NULL");
                return errors.AsReadOnly();
            }
            Require(errors, profile.DevOnlyFixture, "PROFILE_FIXTURE_FLAG_REQUIRED");
            Require(errors, !profile.FormalBalance, "PROFILE_FORMAL_BALANCE_FORBIDDEN");
            Require(errors, !profile.LiveTuning, "PROFILE_LIVE_TUNING_FORBIDDEN");
            Require(errors, profile.DevOnly, "PROFILE_DEV_ONLY_REQUIRED");
            Require(errors, !profile.IsEnabled, "PROFILE_ENABLED_FORBIDDEN");
            Require(errors, !profile.EntersFormalFlow, "PROFILE_FORMAL_FLOW_FORBIDDEN");
            Require(errors, !profile.RuntimeBoundToBattle, "PROFILE_BATTLE_BINDING_FORBIDDEN");
            Require(errors, profile.MaxHp > 0, "PROFILE_HP_INVALID");
            Require(errors, profile.DeathPresentationHoldRecommendationTicks >= 0,
                "PROFILE_DEATH_HOLD_INVALID");
            Require(errors,
                profile.MechanicKeys.SequenceEqual(
                    profile.MechanicKeys.OrderBy(value => value, StringComparer.Ordinal)),
                "PROFILE_MECHANICS_NOT_SORTED");
            Require(errors,
                profile.ActionPatternIds.SequenceEqual(
                    profile.ActionPatternIds.OrderBy(value => value, StringComparer.Ordinal)),
                "PROFILE_ACTIONS_NOT_SORTED");
            Require(errors,
                profile.ActionPatternIds.Distinct(StringComparer.Ordinal).Count()
                    == profile.ActionPatternIds.Count,
                "PROFILE_ACTION_DUPLICATE");
            return errors.AsReadOnly();
        }

        public static IReadOnlyList<string> ValidateAction(
            C1EnemyActionPatternSnapshot action)
        {
            List<string> errors = new List<string>();
            if (action == null)
            {
                errors.Add("ACTION_NULL");
                return errors.AsReadOnly();
            }
            Require(errors, action.DevOnlyFixture, "ACTION_FIXTURE_FLAG_REQUIRED");
            Require(errors, !string.IsNullOrWhiteSpace(action.ActionPatternId),
                "ACTION_ID_REQUIRED");
            Require(errors, C1EnemyRuntimeCatalog.FindProfile(action.OwnerRuntimeProfileId) != null,
                "ACTION_OWNER_UNKNOWN");
            Require(errors, !string.IsNullOrWhiteSpace(action.MechanicKey),
                "ACTION_MECHANIC_REQUIRED");
            Require(errors, !string.IsNullOrWhiteSpace(action.EffectRequestKey),
                "ACTION_REQUEST_REQUIRED");
            Require(errors, action.FirstDueTick >= 0L, "ACTION_FIRST_DUE_INVALID");
            Require(errors, action.RepeatIntervalTicks > 0L, "ACTION_INTERVAL_INVALID");
            Require(errors, action.TelegraphTicks >= 0L, "ACTION_TELEGRAPH_INVALID");
            Require(errors, action.CastTicks >= 0L, "ACTION_CAST_INVALID");
            Require(errors,
                action.ResolveOffsetTicks == action.TelegraphTicks + action.CastTicks,
                "ACTION_RESOLVE_OFFSET_INVALID");
            Require(errors, action.RecoverTicks >= 0L, "ACTION_RECOVER_INVALID");
            Require(errors, action.RequestedDurationTicks >= 0,
                "ACTION_REQUEST_DURATION_INVALID");
            Require(errors,
                action.InterruptPolicy == C1EnemyInterruptPolicy.ReactiveCueFirst,
                "ACTION_INTERRUPT_POLICY_INVALID");
            Require(errors,
                action.CueKind == C1EnemyCueKind.BasicAttack
                    || action.CueKind == C1EnemyCueKind.Skill
                    || action.CueKind == C1EnemyCueKind.ChargeAttack,
                "ACTION_CUE_INVALID");
            return errors.AsReadOnly();
        }

        public static string ValidateApplication(
            C1EnemyRuntimeSnapshot snapshot,
            C1EnemyBattleApplicationResult application)
        {
            if (snapshot == null)
            {
                return "STATE_NULL";
            }
            if (application == null)
            {
                return "APPLICATION_NULL";
            }
            if (string.IsNullOrWhiteSpace(application.ApplicationEventId))
            {
                return "APPLICATION_EVENT_ID_REQUIRED";
            }
            if (string.IsNullOrWhiteSpace(application.SourceRequestId))
            {
                return "SOURCE_REQUEST_ID_REQUIRED";
            }
            if (!string.Equals(
                application.TargetEnemyInstanceId,
                snapshot.EnemyInstanceId,
                StringComparison.Ordinal))
            {
                return "WRONG_TARGET";
            }
            if (application.ResetGeneration != snapshot.ResetGeneration)
            {
                return "STALE_GENERATION";
            }
            if (!application.AcceptedByBattleLedger)
            {
                return "BATTLE_LEDGER_REJECTED";
            }
            if (application.ApplicationSequence <= snapshot.LastAcceptedApplicationSequence)
            {
                return "APPLICATION_SEQUENCE_NOT_MONOTONIC";
            }
            if (snapshot.LastAcceptedBattleTick >= 0L
                && application.BattleTick <= snapshot.LastAcceptedBattleTick)
            {
                return "BATTLE_TICK_NOT_MONOTONIC";
            }
            if (snapshot.AcceptedApplicationEventIds.Contains(
                application.ApplicationEventId,
                StringComparer.Ordinal))
            {
                return "APPLICATION_EVENT_DUPLICATE";
            }
            if (application.ActualHpDeltaApplied < 0
                || application.ActualShellDeltaApplied < 0)
            {
                return "NEGATIVE_DELTA";
            }
            if (application.ActualHpDeltaApplied == 0
                && application.ActualShellDeltaApplied == 0)
            {
                return "ZERO_DELTA";
            }
            if (snapshot.Lifecycle != C1EnemyLifecycle.Active)
            {
                return "ACTIVE_STATE_REQUIRED";
            }
            if (snapshot.ContentId == C1EnemyRuntimeContract.ShatteredHostContentId
                && application.ActualShellDeltaApplied > 0)
            {
                return "HOST_SHELL_DELTA_FORBIDDEN";
            }
            if (application.ActualHpDeltaApplied > snapshot.CurrentHp)
            {
                return "HP_DELTA_BEYOND_CURRENT";
            }
            if (application.ActualShellDeltaApplied > snapshot.CurrentShell)
            {
                return "SHELL_DELTA_BEYOND_CURRENT";
            }
            if (snapshot.ContentId == C1EnemyRuntimeContract.PorcelainHoundContentId
                && snapshot.CurrentShell > 0
                && application.ActualHpDeltaApplied > 0)
            {
                return "HOUND_HP_GUARDED_BY_SHELL";
            }
            return string.Empty;
        }

        private static void Require(ICollection<string> errors, bool condition, string error)
        {
            if (!condition)
            {
                errors.Add(error);
            }
        }
    }
}
