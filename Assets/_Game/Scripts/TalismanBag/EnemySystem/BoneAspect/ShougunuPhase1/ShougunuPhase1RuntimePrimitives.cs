using System;
using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.EnemySystem.BoneAspect.ShougunuPhase1
{
    public enum ShougunuPhase1LifecycleState
    {
        Presence = 0,
        LayeredShellOn = 1,
        CoreExposed = 2,
        Recovering = 3,
        DefeatCandidate = 4,
        Defeated = 5,
        Invalid = 6
    }

    public enum ShougunuPhase1CueKind
    {
        Presence = 0,
        ShellHit = 1,
        ShellBreak = 2,
        CoreExpose = 3,
        Hit = 4,
        Recover = 5,
        Defeated = 6,
        Reset = 7,
        BasicAttack = 8,
        Skill1ShellRepair = 9,
        Skill2RopeHeavyStrike = 10,
        Skill3GroundSealBurst = 11
    }

    public enum ShougunuPhase1ActionStatus
    {
        Pending = 0,
        Windup = 1,
        Resolved = 2,
        Recovering = 3
    }

    public enum ShougunuPhase1InterruptPolicy
    {
        None = 0,
        ReactiveCueFirst = 1
    }

    public enum ShougunuPhase1DueKind
    {
        BasicDebt = 0,
        HpThreshold = 1
    }

    public static class ShougunuPhase1RuntimeContract
    {
        public const string SchemaId = "ShougunuPhase1RuntimeAndActionContract.v1";
        public const int SchemaVersion = 1;
        public const string ContentId = "bone_aspect_boss_c1_bone_guard";
        public const string PhaseId = "bone_aspect.phase.shougunu.phase1";
        public const string PresentationKey = "boss.bone_aspect.c1.bone_guard";
        public const bool DevOnly = true;
        public const bool IsEnabled = false;
        public const bool EntersFormalFlow = false;
        public const bool RuntimeBoundToBattle = false;
        public const int MaxHp = 980;
        public const int InitialHp = 980;
        public const int ShellLayerMax = 200;
        public const int MaxSequentialShellLayers = 7;
        public const int InitialShellLayerIndex = 1;
        public const long CoreExposeDurationTicks = 6000L;
        public const int TicksPerSecond = 1000;
        public const int Skill1RepairUnits = 30;
        public const int BasicDamageApplicationInterval = 4;
        public const long CoreExposeBasicGuardTicks = 600L;
        public const string TerminalOrderingViolation = "TERMINAL_ORDERING_VIOLATION";
    }

    public sealed class ShougunuPhase1BattleApplication
    {
        public ShougunuPhase1BattleApplication(
            string applicationEventId,
            long applicationSequence,
            long battleTick,
            int resetGeneration,
            string enemyInstanceId,
            bool acceptedByBattleLedger,
            int shellDamageApplied,
            int hpDamageApplied,
            string rejectionReason,
            int nominalPulseMagnitude = 0)
        {
            ApplicationEventId = applicationEventId ?? string.Empty;
            ApplicationSequence = applicationSequence;
            BattleTick = battleTick;
            ResetGeneration = resetGeneration;
            EnemyInstanceId = enemyInstanceId ?? string.Empty;
            AcceptedByBattleLedger = acceptedByBattleLedger;
            ShellDamageApplied = shellDamageApplied;
            HpDamageApplied = hpDamageApplied;
            RejectionReason = rejectionReason ?? string.Empty;
            NominalPulseMagnitude = nominalPulseMagnitude > 0
                ? nominalPulseMagnitude
                : shellDamageApplied + hpDamageApplied;
        }

        public string ApplicationEventId { get; }
        public long ApplicationSequence { get; }
        public long BattleTick { get; }
        public int ResetGeneration { get; }
        public string EnemyInstanceId { get; }
        public bool AcceptedByBattleLedger { get; }
        public int ShellDamageApplied { get; }
        public int HpDamageApplied { get; }
        public string RejectionReason { get; }
        public int NominalPulseMagnitude { get; }

        public int TotalAppliedDamage
        {
            get { return ShellDamageApplied + HpDamageApplied; }
        }
    }

    public sealed class ShougunuPhase1CueSnapshot
    {
        public ShougunuPhase1CueSnapshot(
            string cueId,
            long cueSequence,
            long revision,
            int resetGeneration,
            string enemyInstanceId,
            ShougunuPhase1CueKind cueKind,
            long battleTick,
            string sourceEventId,
            string executionId,
            ShougunuPhase1LifecycleState longLivedState)
        {
            CueId = cueId ?? string.Empty;
            CueSequence = cueSequence;
            Revision = revision;
            ResetGeneration = resetGeneration;
            EnemyInstanceId = enemyInstanceId ?? string.Empty;
            CueKind = cueKind;
            BattleTick = battleTick;
            SourceEventId = sourceEventId ?? string.Empty;
            ExecutionId = executionId ?? string.Empty;
            LongLivedState = longLivedState;
        }

        public string CueId { get; }
        public long CueSequence { get; }
        public long Revision { get; }
        public int ResetGeneration { get; }
        public string EnemyInstanceId { get; }
        public ShougunuPhase1CueKind CueKind { get; }
        public long BattleTick { get; }
        public string SourceEventId { get; }
        public string ExecutionId { get; }
        public ShougunuPhase1LifecycleState LongLivedState { get; }
    }

    public sealed class ShougunuPhase1ThresholdOccurrenceSnapshot
    {
        public ShougunuPhase1ThresholdOccurrenceSnapshot(
            string occurrenceId,
            string thresholdId,
            string actionPatternId,
            int thresholdBasisPoints,
            long triggerSequence,
            long triggerTick,
            bool resolved)
        {
            OccurrenceId = occurrenceId ?? string.Empty;
            ThresholdId = thresholdId ?? string.Empty;
            ActionPatternId = actionPatternId ?? string.Empty;
            ThresholdBasisPoints = thresholdBasisPoints;
            TriggerSequence = triggerSequence;
            TriggerTick = triggerTick;
            Resolved = resolved;
        }

        public string OccurrenceId { get; }
        public string ThresholdId { get; }
        public string ActionPatternId { get; }
        public int ThresholdBasisPoints { get; }
        public long TriggerSequence { get; }
        public long TriggerTick { get; }
        public bool Resolved { get; }

        internal ShougunuPhase1ThresholdOccurrenceSnapshot WithResolved(bool resolved)
        {
            return new ShougunuPhase1ThresholdOccurrenceSnapshot(
                OccurrenceId,
                ThresholdId,
                ActionPatternId,
                ThresholdBasisPoints,
                TriggerSequence,
                TriggerTick,
                resolved);
        }
    }

    public sealed class ShougunuPhase1ActionPatternSnapshot
    {
        public ShougunuPhase1ActionPatternSnapshot(
            string actionPatternId,
            string effectRequestId,
            ShougunuPhase1DueKind dueKind,
            long preCastTicks,
            long telegraphTicks,
            long castTicks,
            long resolveOffsetTicks,
            long recoverTicks,
            ShougunuPhase1InterruptPolicy interruptPolicy,
            ShougunuPhase1CueKind cueKind)
        {
            ActionPatternId = actionPatternId ?? string.Empty;
            EffectRequestId = effectRequestId ?? string.Empty;
            DueKind = dueKind;
            PreCastTicks = preCastTicks;
            TelegraphTicks = telegraphTicks;
            CastTicks = castTicks;
            ResolveOffsetTicks = resolveOffsetTicks;
            RecoverTicks = recoverTicks;
            InterruptPolicy = interruptPolicy;
            CueKind = cueKind;
        }

        public string ActionPatternId { get; }
        public string EffectRequestId { get; }
        public ShougunuPhase1DueKind DueKind { get; }
        public long PreCastTicks { get; }
        public long TelegraphTicks { get; }
        public long CastTicks { get; }
        public long ResolveOffsetTicks { get; }
        public long RecoverTicks { get; }
        public ShougunuPhase1InterruptPolicy InterruptPolicy { get; }
        public ShougunuPhase1CueKind CueKind { get; }
    }

    public sealed class ShougunuPhase1PendingActionSnapshot
    {
        public ShougunuPhase1PendingActionSnapshot(
            string executionId,
            string actionPatternId,
            string thresholdOccurrenceId,
            ShougunuPhase1ActionStatus status,
            long startTick,
            long resolveTick,
            long recoveryEndTick)
        {
            ExecutionId = executionId ?? string.Empty;
            ActionPatternId = actionPatternId ?? string.Empty;
            ThresholdOccurrenceId = thresholdOccurrenceId ?? string.Empty;
            Status = status;
            StartTick = startTick;
            ResolveTick = resolveTick;
            RecoveryEndTick = recoveryEndTick;
        }

        public string ExecutionId { get; }
        public string ActionPatternId { get; }
        public string ThresholdOccurrenceId { get; }
        public ShougunuPhase1ActionStatus Status { get; }
        public long StartTick { get; }
        public long ResolveTick { get; }
        public long RecoveryEndTick { get; }

        internal ShougunuPhase1PendingActionSnapshot WithStatus(ShougunuPhase1ActionStatus status)
        {
            return new ShougunuPhase1PendingActionSnapshot(
                ExecutionId,
                ActionPatternId,
                ThresholdOccurrenceId,
                status,
                StartTick,
                ResolveTick,
                RecoveryEndTick);
        }
    }

    public sealed class ShougunuPhase1TransitionResult
    {
        public ShougunuPhase1TransitionResult(
            ShougunuPhase1RuntimeSnapshot state,
            bool accepted,
            string rejectionReason)
        {
            State = state;
            Accepted = accepted;
            RejectionReason = rejectionReason ?? string.Empty;
        }

        public ShougunuPhase1RuntimeSnapshot State { get; }
        public bool Accepted { get; }
        public string RejectionReason { get; }
    }

    internal static class ShougunuPhase1Collection
    {
        public static IReadOnlyList<T> Copy<T>(IEnumerable<T> values)
        {
            return Array.AsReadOnly((values ?? Enumerable.Empty<T>()).ToArray());
        }
    }
}
