using System;
using System.Linq;

namespace TalismanBag.EnemySystem.BoneAspect.ShougunuPhase1
{
    public static class ShougunuPhase1RuntimeReducer
    {
        public static ShougunuPhase1RuntimeSnapshot CreatePresence(
            string enemyInstanceId,
            int resetGeneration,
            long battleTick)
        {
            ShougunuPhase1RuntimeStateData data = new ShougunuPhase1RuntimeStateData
            {
                EnemyInstanceId = enemyInstanceId ?? string.Empty,
                ResetGeneration = resetGeneration,
                Revision = 1L,
                Lifecycle = ShougunuPhase1LifecycleState.Presence,
                CurrentHp = ShougunuPhase1RuntimeContract.InitialHp,
                CurrentShell = 0,
                Targetable = false,
                Vulnerable = false,
                RecoveryAvailable = true,
                LastAcceptedBattleTick = -1L,
                LastAcceptedApplicationSequence = -1L
            };
            AddCue(data, ShougunuPhase1CueKind.Presence, battleTick, "presence", string.Empty);
            return Finalize(data);
        }

        public static ShougunuPhase1TransitionResult ActivateInitialShell(
            ShougunuPhase1RuntimeSnapshot snapshot,
            long battleTick)
        {
            if (snapshot == null)
            {
                return Invalid(null, "STATE_NULL", battleTick);
            }
            if (snapshot.Lifecycle != ShougunuPhase1LifecycleState.Presence)
            {
                return Reject(snapshot, "PRESENCE_REQUIRED");
            }

            ShougunuPhase1RuntimeStateData data = snapshot.ToMutable();
            data.Revision++;
            data.Lifecycle = ShougunuPhase1LifecycleState.LayeredShellOn;
            data.ShellLayerIndex = ShougunuPhase1RuntimeContract.InitialShellLayerIndex;
            data.CurrentShell = data.ShellLayerMax;
            data.Targetable = true;
            data.Vulnerable = false;
            data.RecoveryAvailable = true;
            return Accept(data);
        }

        public static ShougunuPhase1TransitionResult ApplyBattleApplication(
            ShougunuPhase1RuntimeSnapshot snapshot,
            ShougunuPhase1BattleApplication application)
        {
            string structuralError = ShougunuPhase1RuntimeValidation.ValidateApplication(
                snapshot,
                application);
            if (!string.IsNullOrEmpty(structuralError))
            {
                if (IsMalformed(structuralError))
                {
                    return Invalid(snapshot, structuralError, application == null ? -1L : application.BattleTick);
                }
                return Reject(snapshot, structuralError);
            }

            ShougunuPhase1RuntimeStateData data = snapshot.ToMutable();
            int beforeHp = data.CurrentHp;
            data.Revision++;
            data.LastAcceptedBattleTick = application.BattleTick;
            data.LastAcceptedApplicationSequence = application.ApplicationSequence;
            data.AcceptedApplicationEventIds.Add(application.ApplicationEventId);
            data.AcceptedDamageApplicationCount++;

            if (application.ShellDamageApplied > 0)
            {
                data.CurrentShell -= application.ShellDamageApplied;
                AddCue(
                    data,
                    ShougunuPhase1CueKind.ShellHit,
                    application.BattleTick,
                    application.ApplicationEventId,
                    string.Empty);
                if (data.CurrentShell == 0)
                {
                    EnterCoreExpose(data, application.BattleTick, application.ApplicationEventId);
                }
            }
            if (application.HpDamageApplied > 0)
            {
                data.CurrentHp -= application.HpDamageApplied;
                AddThresholdOccurrences(data, beforeHp, data.CurrentHp, application);
                AddCue(
                    data,
                    ShougunuPhase1CueKind.Hit,
                    application.BattleTick,
                    application.ApplicationEventId,
                    string.Empty);
                if (data.CurrentHp == 0)
                {
                    data.Lifecycle = ShougunuPhase1LifecycleState.DefeatCandidate;
                    data.Targetable = false;
                    data.Vulnerable = false;
                    data.RecoveryAvailable = false;
                }
            }
            return Accept(data);
        }

        public static ShougunuPhase1TransitionResult AdvanceBattleTick(
            ShougunuPhase1RuntimeSnapshot snapshot,
            long battleTick)
        {
            if (snapshot == null)
            {
                return Invalid(null, "STATE_NULL", battleTick);
            }
            if (battleTick < snapshot.LastAcceptedBattleTick)
            {
                return Reject(snapshot, "BATTLE_TICK_STALE");
            }

            if (snapshot.Lifecycle == ShougunuPhase1LifecycleState.CoreExposed
                && snapshot.RecoveryAvailable
                && battleTick >= snapshot.CoreExposeEndTick)
            {
                ShougunuPhase1RuntimeStateData data = snapshot.ToMutable();
                data.Revision++;
                data.Lifecycle = ShougunuPhase1LifecycleState.Recovering;
                data.Targetable = false;
                data.Vulnerable = false;
                AddCue(data, ShougunuPhase1CueKind.Recover, battleTick, "core-expose-elapsed", string.Empty);
                return Accept(data);
            }

            if (snapshot.Lifecycle == ShougunuPhase1LifecycleState.Recovering)
            {
                ShougunuPhase1RuntimeStateData data = snapshot.ToMutable();
                data.Revision++;
                data.Lifecycle = ShougunuPhase1LifecycleState.LayeredShellOn;
                data.ShellLayerIndex++;
                data.CurrentShell = data.ShellLayerMax;
                data.Targetable = true;
                data.Vulnerable = false;
                data.CoreExposeStartTick = -1L;
                data.CoreExposeEndTick = -1L;
                data.RecoveryAvailable = data.ShellLayerIndex < data.MaxSequentialShellLayers;
                return Accept(data);
            }

            return Reject(snapshot, "NO_LIFECYCLE_TRANSITION_DUE");
        }

        public static ShougunuPhase1TransitionResult RequestDefeat(
            ShougunuPhase1RuntimeSnapshot snapshot,
            long battleTick)
        {
            if (snapshot == null)
            {
                return Invalid(null, "STATE_NULL", battleTick);
            }
            if (snapshot.CurrentHp != 0
                || snapshot.Lifecycle != ShougunuPhase1LifecycleState.DefeatCandidate)
            {
                return Reject(snapshot, "DEFEAT_CANDIDATE_REQUIRED");
            }

            bool unresolvedMandatory = snapshot.ThresholdOccurrences.Any(item => !item.Resolved);
            if (unresolvedMandatory)
            {
                return Invalid(snapshot, ShougunuPhase1RuntimeContract.TerminalOrderingViolation, battleTick);
            }

            ShougunuPhase1RuntimeStateData data = snapshot.ToMutable();
            data.Revision++;
            data.Lifecycle = ShougunuPhase1LifecycleState.Defeated;
            data.Targetable = false;
            data.Vulnerable = false;
            data.ActiveAction = null;
            AddCue(data, ShougunuPhase1CueKind.Defeated, battleTick, "defeat-request", string.Empty);
            return Accept(data);
        }

        public static ShougunuPhase1TransitionResult Reset(
            ShougunuPhase1RuntimeSnapshot snapshot,
            int newResetGeneration,
            long battleTick)
        {
            if (snapshot == null)
            {
                return Invalid(null, "STATE_NULL", battleTick);
            }
            if (newResetGeneration <= snapshot.ResetGeneration)
            {
                return Reject(snapshot, "RESET_GENERATION_NOT_INCREASING");
            }

            long nextRevision = snapshot.Revision + 1L;
            ShougunuPhase1RuntimeStateData data = new ShougunuPhase1RuntimeStateData
            {
                EnemyInstanceId = snapshot.EnemyInstanceId,
                ResetGeneration = newResetGeneration,
                Revision = nextRevision,
                Lifecycle = ShougunuPhase1LifecycleState.Presence,
                CurrentHp = ShougunuPhase1RuntimeContract.InitialHp,
                CurrentShell = 0,
                Targetable = false,
                Vulnerable = false,
                RecoveryAvailable = true
            };
            AddCue(data, ShougunuPhase1CueKind.Reset, battleTick, "reset", string.Empty);
            return Accept(data);
        }

        public static ShougunuPhase1RuntimeSnapshot WithDeveloperDiagnostic(
            ShougunuPhase1RuntimeSnapshot snapshot,
            string diagnostic)
        {
            if (snapshot == null)
            {
                return Invalid(null, "STATE_NULL", -1L).State;
            }
            ShougunuPhase1RuntimeStateData data = snapshot.ToMutable();
            data.DeveloperDiagnostics.Add(diagnostic ?? string.Empty);
            return Finalize(data);
        }

        internal static ShougunuPhase1RuntimeSnapshot Finalize(
            ShougunuPhase1RuntimeStateData data)
        {
            return new ShougunuPhase1RuntimeSnapshot(data);
        }

        internal static void AddCue(
            ShougunuPhase1RuntimeStateData data,
            ShougunuPhase1CueKind kind,
            long battleTick,
            string sourceEventId,
            string executionId)
        {
            long sequence = data.NextCueSequence++;
            data.Cues.Add(new ShougunuPhase1CueSnapshot(
                "cue." + data.ResetGeneration + "." + sequence,
                sequence,
                data.Revision,
                data.ResetGeneration,
                data.EnemyInstanceId,
                kind,
                battleTick,
                sourceEventId,
                executionId,
                data.Lifecycle));
        }

        internal static ShougunuPhase1TransitionResult Invalid(
            ShougunuPhase1RuntimeSnapshot snapshot,
            string error,
            long battleTick)
        {
            ShougunuPhase1RuntimeStateData data = snapshot == null
                ? new ShougunuPhase1RuntimeStateData()
                : snapshot.ToMutable();
            data.Revision = Math.Max(1L, data.Revision + 1L);
            data.Lifecycle = ShougunuPhase1LifecycleState.Invalid;
            data.Targetable = false;
            data.Vulnerable = false;
            data.Errors.Add(error ?? "INVALID");
            data.DeveloperDiagnostics.Add("invalid@" + battleTick + ":" + (error ?? "INVALID"));
            return new ShougunuPhase1TransitionResult(Finalize(data), false, error);
        }

        private static void EnterCoreExpose(
            ShougunuPhase1RuntimeStateData data,
            long battleTick,
            string sourceEventId)
        {
            AddCue(data, ShougunuPhase1CueKind.ShellBreak, battleTick, sourceEventId, string.Empty);
            data.Lifecycle = ShougunuPhase1LifecycleState.CoreExposed;
            data.Targetable = true;
            data.Vulnerable = true;
            data.CoreExposeStartTick = battleTick;
            data.CoreExposeEndTick = battleTick + ShougunuPhase1RuntimeContract.CoreExposeDurationTicks;
            data.RecoveryAvailable = data.ShellLayerIndex < data.MaxSequentialShellLayers;
            AddCue(data, ShougunuPhase1CueKind.CoreExpose, battleTick, sourceEventId, string.Empty);
        }

        private static void AddThresholdOccurrences(
            ShougunuPhase1RuntimeStateData data,
            int beforeHp,
            int afterHp,
            ShougunuPhase1BattleApplication application)
        {
            foreach (ShougunuPhase1ActionPatternCatalog.ThresholdDefinition threshold
                in ShougunuPhase1ActionPatternCatalog.GetThresholdsDescending())
            {
                bool alreadyTriggered = data.ThresholdOccurrences.Any(
                    item => string.Equals(item.ThresholdId, threshold.ThresholdId, StringComparison.Ordinal));
                bool crossed = beforeHp * 10000L > data.MaxHp * (long)threshold.BasisPoints
                    && afterHp * 10000L <= data.MaxHp * (long)threshold.BasisPoints;
                if (!alreadyTriggered && crossed)
                {
                    long triggerSequence = data.ThresholdOccurrences.Count + 1L;
                    data.ThresholdOccurrences.Add(
                        new ShougunuPhase1ThresholdOccurrenceSnapshot(
                            "threshold." + data.ResetGeneration + "." + threshold.ThresholdId,
                            threshold.ThresholdId,
                            threshold.ActionPatternId,
                            threshold.BasisPoints,
                            triggerSequence,
                            application.BattleTick,
                            false));
                }
            }
        }

        private static bool IsMalformed(string error)
        {
            return error == "STATE_NULL"
                || error == "APPLICATION_NULL"
                || error == "APPLICATION_EVENT_ID_REQUIRED"
                || error == "APPLICATION_SEQUENCE_NEGATIVE"
                || error == "BATTLE_TICK_NEGATIVE"
                || error == "DAMAGE_NEGATIVE"
                || error == "NOMINAL_PULSE_REQUIRED"
                || error == "APPLIED_DAMAGE_EXCEEDS_NOMINAL_PULSE"
                || error == "SHELL_DAMAGE_EXCEEDS_STATE"
                || error == "HP_DAMAGE_EXCEEDS_STATE"
                || error == "DAMAGE_CHANNEL_LIFECYCLE_MISMATCH";
        }

        private static ShougunuPhase1TransitionResult Accept(ShougunuPhase1RuntimeStateData data)
        {
            return new ShougunuPhase1TransitionResult(Finalize(data), true, string.Empty);
        }

        private static ShougunuPhase1TransitionResult Reject(
            ShougunuPhase1RuntimeSnapshot snapshot,
            string reason)
        {
            return new ShougunuPhase1TransitionResult(snapshot, false, reason);
        }
    }
}
