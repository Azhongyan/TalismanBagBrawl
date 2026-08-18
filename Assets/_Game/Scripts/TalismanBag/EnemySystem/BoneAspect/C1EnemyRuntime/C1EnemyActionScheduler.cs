using System;
using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime
{
    public static class C1EnemyActionScheduler
    {
        public static C1EnemyTransitionResult Advance(
            C1EnemyRuntimeSnapshot snapshot,
            long battleTick,
            bool reactiveCuePending)
        {
            if (snapshot == null)
            {
                return new C1EnemyTransitionResult(null, false, "STATE_NULL");
            }
            if (snapshot.Lifecycle == C1EnemyLifecycle.Defeated)
            {
                return Reject(snapshot, "DEFEATED");
            }
            if (snapshot.Lifecycle != C1EnemyLifecycle.Active)
            {
                return Reject(snapshot, "ACTIVE_STATE_REQUIRED");
            }
            if (battleTick < snapshot.LastSchedulerTick)
            {
                return Reject(snapshot, "SCHEDULER_TICK_NOT_MONOTONIC");
            }
            if (reactiveCuePending)
            {
                return Reject(snapshot, "REACTIVE_PRIORITY");
            }

            IReadOnlyList<C1EnemyActionPatternSnapshot> patterns =
                C1EnemyRuntimeCatalog.FindActionsForProfile(snapshot.RuntimeProfileId);
            if (patterns.Count == 0)
            {
                return Reject(snapshot, "ACTION_PATTERN_MISSING");
            }

            C1EnemyRuntimeStateData data = snapshot.ToMutable();
            data.Revision++;
            data.LastSchedulerTick = battleTick;

            if (data.ActiveAction != null)
            {
                C1EnemyActionPatternSnapshot activePattern =
                    C1EnemyRuntimeCatalog.FindAction(data.ActiveAction.ActionPatternId);
                if (activePattern == null)
                {
                    return Reject(snapshot, "ACTIVE_ACTION_PATTERN_MISSING");
                }
                CloseElapsedPhases(data, activePattern, battleTick);
            }

            if (data.ActiveAction == null)
            {
                StartNextDueAction(data, patterns, battleTick);
            }

            return Accepted(data);
        }

        private static void CloseElapsedPhases(
            C1EnemyRuntimeStateData data,
            C1EnemyActionPatternSnapshot pattern,
            long battleTick)
        {
            if (data.ActiveAction.Status == C1EnemyActionStatus.Telegraph
                && battleTick >= data.ActiveAction.ResolveTick)
            {
                data.ActiveAction =
                    data.ActiveAction.WithStatus(C1EnemyActionStatus.Resolved);
                C1EnemyRuntimeReducer.AddRequest(
                    data,
                    pattern.EffectRequestKey,
                    data.ActiveAction.ResolveTick,
                    data.ActiveAction.ExecutionId,
                    pattern.EffectRequestLifetimeClass,
                    pattern.RequestedDurationTicks);
            }

            if (data.ActiveAction != null
                && data.ActiveAction.Status == C1EnemyActionStatus.Resolved)
            {
                data.ActiveAction =
                    data.ActiveAction.WithStatus(C1EnemyActionStatus.Recovering);
            }

            if (data.ActiveAction != null
                && data.ActiveAction.Status == C1EnemyActionStatus.Recovering
                && battleTick >= data.ActiveAction.RecoveryEndTick)
            {
                data.ActiveAction = null;
            }
        }

        private static void StartNextDueAction(
            C1EnemyRuntimeStateData data,
            IEnumerable<C1EnemyActionPatternSnapshot> patterns,
            long battleTick)
        {
            Dictionary<string, C1EnemyActionPatternSnapshot> byId = patterns
                .ToDictionary(
                    pattern => pattern.ActionPatternId,
                    pattern => pattern,
                    StringComparer.Ordinal);

            C1EnemyActionScheduleStateSnapshot due = data.ActionScheduleStates
                .Where(schedule =>
                    schedule.NextDueTick <= battleTick
                    && byId.ContainsKey(schedule.ActionPatternId))
                .OrderBy(schedule => schedule.NextDueTick)
                .ThenByDescending(schedule => byId[schedule.ActionPatternId].Priority)
                .ThenBy(schedule => schedule.ActionPatternId, StringComparer.Ordinal)
                .FirstOrDefault();
            if (due == null)
            {
                return;
            }

            C1EnemyActionPatternSnapshot pattern = byId[due.ActionPatternId];
            long sequence = data.NextExecutionSequence++;
            string executionId = string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "execution:g{0}:s{1}",
                data.ResetGeneration,
                sequence);
            data.ActiveAction = new C1EnemyActiveActionSnapshot(
                executionId,
                sequence,
                pattern.ActionPatternId,
                C1EnemyActionStatus.Telegraph,
                battleTick,
                checked(battleTick + pattern.ResolveOffsetTicks),
                checked(battleTick + pattern.ResolveOffsetTicks + pattern.RecoverTicks));

            int index = data.ActionScheduleStates.FindIndex(schedule => string.Equals(
                schedule.ActionPatternId,
                due.ActionPatternId,
                StringComparison.Ordinal));
            data.ActionScheduleStates[index] = due.WithStarted(
                checked(due.NextDueTick + pattern.RepeatIntervalTicks),
                battleTick);
            data.ActionScheduleStates = data.ActionScheduleStates
                .OrderBy(schedule => schedule.ActionPatternId, StringComparer.Ordinal)
                .ToList();

            C1EnemyRuntimeReducer.AddCue(
                data,
                pattern.CueKind,
                battleTick,
                executionId);
        }

        private static C1EnemyTransitionResult Accepted(C1EnemyRuntimeStateData data)
        {
            return new C1EnemyTransitionResult(
                C1EnemyRuntimeReducer.Finalize(data),
                true,
                string.Empty);
        }

        private static C1EnemyTransitionResult Reject(
            C1EnemyRuntimeSnapshot snapshot,
            string error)
        {
            return new C1EnemyTransitionResult(snapshot, false, error);
        }
    }
}
