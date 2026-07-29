using System;
using System.Linq;

namespace TalismanBag.EnemySystem.BoneAspect.ShougunuPhase1
{
    public static class ShougunuPhase1ActionScheduler
    {
        public static ShougunuPhase1TransitionResult Advance(
            ShougunuPhase1RuntimeSnapshot snapshot,
            long battleTick,
            bool battleAcceptsResolve)
        {
            if (snapshot == null)
            {
                return ShougunuPhase1RuntimeReducer.Invalid(null, "STATE_NULL", battleTick);
            }
            if (snapshot.Lifecycle == ShougunuPhase1LifecycleState.Invalid
                || snapshot.Lifecycle == ShougunuPhase1LifecycleState.Defeated)
            {
                return new ShougunuPhase1TransitionResult(
                    snapshot,
                    false,
                    "SCHEDULER_TERMINAL_STATE");
            }

            if (snapshot.ActiveAction == null)
            {
                return TryStart(snapshot, battleTick);
            }

            if (snapshot.ActiveAction.Status == ShougunuPhase1ActionStatus.Windup
                && battleTick >= snapshot.ActiveAction.ResolveTick)
            {
                return Resolve(snapshot, battleTick, battleAcceptsResolve);
            }

            if ((snapshot.ActiveAction.Status == ShougunuPhase1ActionStatus.Resolved
                    || snapshot.ActiveAction.Status == ShougunuPhase1ActionStatus.Recovering)
                && battleTick >= snapshot.ActiveAction.RecoveryEndTick)
            {
                ShougunuPhase1RuntimeStateData data = snapshot.ToMutable();
                data.Revision++;
                data.ActiveAction = null;
                return new ShougunuPhase1TransitionResult(
                    ShougunuPhase1RuntimeReducer.Finalize(data),
                    true,
                    string.Empty);
            }

            if (snapshot.ActiveAction.Status == ShougunuPhase1ActionStatus.Resolved)
            {
                ShougunuPhase1RuntimeStateData data = snapshot.ToMutable();
                data.Revision++;
                data.ActiveAction = data.ActiveAction.WithStatus(
                    ShougunuPhase1ActionStatus.Recovering);
                return new ShougunuPhase1TransitionResult(
                    ShougunuPhase1RuntimeReducer.Finalize(data),
                    true,
                    string.Empty);
            }

            return new ShougunuPhase1TransitionResult(snapshot, false, "NO_ACTION_TRANSITION_DUE");
        }

        private static ShougunuPhase1TransitionResult TryStart(
            ShougunuPhase1RuntimeSnapshot snapshot,
            long battleTick)
        {
            ShougunuPhase1ThresholdOccurrenceSnapshot threshold = snapshot.ThresholdOccurrences
                .Where(item => !item.Resolved)
                .OrderBy(item => item.TriggerSequence)
                .FirstOrDefault(item => IsActionLegal(snapshot, item.ActionPatternId, battleTick));

            string actionPatternId = threshold == null ? string.Empty : threshold.ActionPatternId;
            string thresholdOccurrenceId = threshold == null ? string.Empty : threshold.OccurrenceId;
            if (string.IsNullOrEmpty(actionPatternId)
                && snapshot.BasicDebt > 0
                && IsActionLegal(
                    snapshot,
                    ShougunuPhase1ActionPatternCatalog.BasicAttack,
                    battleTick))
            {
                actionPatternId = ShougunuPhase1ActionPatternCatalog.BasicAttack;
            }

            if (string.IsNullOrEmpty(actionPatternId))
            {
                return new ShougunuPhase1TransitionResult(snapshot, false, "NO_LEGAL_ACTION_DUE");
            }

            ShougunuPhase1ActionPatternSnapshot pattern =
                ShougunuPhase1ActionPatternCatalog.GetPattern(actionPatternId);
            ShougunuPhase1RuntimeStateData data = snapshot.ToMutable();
            data.Revision++;
            long executionSequence = data.NextExecutionSequence++;
            data.ActiveAction = new ShougunuPhase1PendingActionSnapshot(
                "execution." + data.ResetGeneration + "." + executionSequence,
                pattern.ActionPatternId,
                thresholdOccurrenceId,
                ShougunuPhase1ActionStatus.Windup,
                battleTick,
                battleTick + pattern.ResolveOffsetTicks,
                battleTick + pattern.ResolveOffsetTicks + pattern.RecoverTicks);
            return new ShougunuPhase1TransitionResult(
                ShougunuPhase1RuntimeReducer.Finalize(data),
                true,
                string.Empty);
        }

        private static ShougunuPhase1TransitionResult Resolve(
            ShougunuPhase1RuntimeSnapshot snapshot,
            long battleTick,
            bool battleAcceptsResolve)
        {
            if (!battleAcceptsResolve)
            {
                return new ShougunuPhase1TransitionResult(
                    snapshot,
                    false,
                    "BATTLE_REJECTED_ACTION_RESOLVE");
            }

            ShougunuPhase1PendingActionSnapshot active = snapshot.ActiveAction;
            if (!IsActionLegal(snapshot, active.ActionPatternId, battleTick))
            {
                return new ShougunuPhase1TransitionResult(
                    snapshot,
                    false,
                    "ACTION_NO_LONGER_LEGAL");
            }

            ShougunuPhase1ActionPatternSnapshot pattern =
                ShougunuPhase1ActionPatternCatalog.GetPattern(active.ActionPatternId);
            ShougunuPhase1RuntimeStateData data = snapshot.ToMutable();
            data.Revision++;

            if (string.Equals(
                active.ActionPatternId,
                ShougunuPhase1ActionPatternCatalog.BasicAttack,
                StringComparison.Ordinal))
            {
                data.BasicResolvedCount++;
            }
            else
            {
                int thresholdIndex = data.ThresholdOccurrences.FindIndex(
                    item => string.Equals(
                        item.OccurrenceId,
                        active.ThresholdOccurrenceId,
                        StringComparison.Ordinal));
                if (thresholdIndex < 0)
                {
                    return ShougunuPhase1RuntimeReducer.Invalid(
                        snapshot,
                        "THRESHOLD_OCCURRENCE_MISSING",
                        battleTick);
                }
                data.ThresholdOccurrences[thresholdIndex] =
                    data.ThresholdOccurrences[thresholdIndex].WithResolved(true);

                if (string.Equals(
                    active.ActionPatternId,
                    ShougunuPhase1ActionPatternCatalog.Skill1ShellRepair,
                    StringComparison.Ordinal))
                {
                    data.CurrentShell = Math.Min(
                        data.ShellLayerMax,
                        data.CurrentShell + ShougunuPhase1RuntimeContract.Skill1RepairUnits);
                }
            }

            data.ActiveAction = active.WithStatus(ShougunuPhase1ActionStatus.Resolved);
            ShougunuPhase1RuntimeReducer.AddCue(
                data,
                pattern.CueKind,
                battleTick,
                active.ThresholdOccurrenceId,
                active.ExecutionId);
            return new ShougunuPhase1TransitionResult(
                ShougunuPhase1RuntimeReducer.Finalize(data),
                true,
                string.Empty);
        }

        private static bool IsActionLegal(
            ShougunuPhase1RuntimeSnapshot snapshot,
            string actionPatternId,
            long battleTick)
        {
            if (snapshot.Lifecycle == ShougunuPhase1LifecycleState.Invalid
                || snapshot.Lifecycle == ShougunuPhase1LifecycleState.Defeated
                || snapshot.Lifecycle == ShougunuPhase1LifecycleState.DefeatCandidate
                || snapshot.Lifecycle == ShougunuPhase1LifecycleState.Presence
                || snapshot.Lifecycle == ShougunuPhase1LifecycleState.Recovering)
            {
                return false;
            }

            if (string.Equals(
                actionPatternId,
                ShougunuPhase1ActionPatternCatalog.Skill1ShellRepair,
                StringComparison.Ordinal))
            {
                return snapshot.Lifecycle == ShougunuPhase1LifecycleState.LayeredShellOn
                    && snapshot.CurrentShell > 0
                    && snapshot.CurrentShell < snapshot.ShellLayerMax;
            }

            if (string.Equals(
                actionPatternId,
                ShougunuPhase1ActionPatternCatalog.BasicAttack,
                StringComparison.Ordinal))
            {
                return snapshot.Lifecycle == ShougunuPhase1LifecycleState.LayeredShellOn
                    || (snapshot.Lifecycle == ShougunuPhase1LifecycleState.CoreExposed
                        && battleTick >= snapshot.CoreExposeStartTick
                            + ShougunuPhase1RuntimeContract.CoreExposeBasicGuardTicks);
            }

            return snapshot.Lifecycle == ShougunuPhase1LifecycleState.LayeredShellOn;
        }
    }
}
