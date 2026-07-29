using System;

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
                return new C1EnemyTransitionResult(snapshot, false, "DEFEATED");
            }
            if (snapshot.Lifecycle != C1EnemyLifecycle.Active)
            {
                return new C1EnemyTransitionResult(snapshot, false, "ACTIVE_STATE_REQUIRED");
            }
            if (battleTick < snapshot.LastSchedulerTick)
            {
                return new C1EnemyTransitionResult(
                    snapshot,
                    false,
                    "SCHEDULER_TICK_NOT_MONOTONIC");
            }
            if (reactiveCuePending)
            {
                return new C1EnemyTransitionResult(snapshot, false, "REACTIVE_PRIORITY");
            }

            C1EnemyActionPatternSnapshot pattern =
                C1EnemyRuntimeCatalog.FindActionForProfile(snapshot.RuntimeProfileId);
            if (pattern == null)
            {
                return new C1EnemyTransitionResult(snapshot, false, "ACTION_PATTERN_MISSING");
            }

            C1EnemyRuntimeStateData data = snapshot.ToMutable();
            data.Revision++;
            data.LastSchedulerTick = battleTick;

            if (data.ActiveAction == null)
            {
                if (battleTick < data.NextActionDueTick)
                {
                    return Accepted(data);
                }
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
                    battleTick + pattern.ResolveOffsetTicks,
                    battleTick + pattern.ResolveOffsetTicks + pattern.RecoverTicks);
                C1EnemyRuntimeReducer.AddCue(
                    data,
                    pattern.ActionPatternId == C1EnemyRuntimeContract.ShatteredHostActionId
                        ? C1EnemyCueKind.BasicAttack
                        : C1EnemyCueKind.ChargeAttack,
                    battleTick,
                    executionId);
                return Accepted(data);
            }

            if (data.ActiveAction.Status == C1EnemyActionStatus.Telegraph)
            {
                if (battleTick >= data.ActiveAction.ResolveTick)
                {
                    data.ActiveAction =
                        data.ActiveAction.WithStatus(C1EnemyActionStatus.Resolved);
                    C1EnemyRuntimeReducer.AddRequest(
                        data,
                        pattern.EffectRequestKey,
                        data.ActiveAction.ResolveTick,
                        data.ActiveAction.ExecutionId,
                        string.Empty,
                        0);
                }
                return Accepted(data);
            }

            if (data.ActiveAction.Status == C1EnemyActionStatus.Resolved)
            {
                data.ActiveAction =
                    data.ActiveAction.WithStatus(C1EnemyActionStatus.Recovering);
                return Accepted(data);
            }

            if (data.ActiveAction.Status == C1EnemyActionStatus.Recovering
                && battleTick >= data.ActiveAction.RecoveryEndTick)
            {
                data.ActiveAction = null;
                data.NextActionDueTick += pattern.RepeatIntervalTicks;
            }
            return Accepted(data);
        }

        private static C1EnemyTransitionResult Accepted(C1EnemyRuntimeStateData data)
        {
            return new C1EnemyTransitionResult(
                C1EnemyRuntimeReducer.Finalize(data),
                true,
                string.Empty);
        }
    }
}
