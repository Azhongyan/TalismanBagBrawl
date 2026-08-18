using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime.BoneSwapRemnant
{
    public sealed class BoneSwapResolvedDirectDamageFact
    {
        public BoneSwapResolvedDirectDamageFact(
            string schemaId,
            string sourceEventId,
            long applicationSequence,
            long battleTick,
            int resetGeneration,
            string recipientEnemyInstanceId,
            bool acceptedByBattleLedger,
            BoneSwapResolvedDamageSourceKind sourceKind,
            int resolvedDamage)
        {
            SchemaId = schemaId ?? string.Empty;
            SourceEventId = sourceEventId ?? string.Empty;
            ApplicationSequence = applicationSequence;
            BattleTick = battleTick;
            ResetGeneration = resetGeneration;
            RecipientEnemyInstanceId = recipientEnemyInstanceId ?? string.Empty;
            AcceptedByBattleLedger = acceptedByBattleLedger;
            SourceKind = sourceKind;
            ResolvedDamage = resolvedDamage;
        }

        public string SchemaId { get; }
        public string SourceEventId { get; }
        public long ApplicationSequence { get; }
        public long BattleTick { get; }
        public int ResetGeneration { get; }
        public string RecipientEnemyInstanceId { get; }
        public bool AcceptedByBattleLedger { get; }
        public BoneSwapResolvedDamageSourceKind SourceKind { get; }
        public int ResolvedDamage { get; }
    }

    public sealed class BoneSwapRemnantCapturedPulseSnapshot
    {
        internal BoneSwapRemnantCapturedPulseSnapshot(
            string sourceEventId,
            long applicationSequence,
            long battleTick,
            int resetGeneration,
            int resolvedDamage,
            int boundedReturnDamage)
        {
            SourceEventId = sourceEventId ?? string.Empty;
            ApplicationSequence = applicationSequence;
            BattleTick = battleTick;
            ResetGeneration = resetGeneration;
            ResolvedDamage = resolvedDamage;
            BoundedReturnDamage = boundedReturnDamage;
            CanonicalSignature = BoneSwapRemnantCanonical.Hash(
                SourceEventId,
                BoneSwapRemnantCanonical.Number(ApplicationSequence),
                BoneSwapRemnantCanonical.Number(BattleTick),
                BoneSwapRemnantCanonical.Number(ResetGeneration),
                BoneSwapRemnantCanonical.Number(ResolvedDamage),
                BoneSwapRemnantCanonical.Number(BoundedReturnDamage));
        }

        public string SourceEventId { get; }
        public long ApplicationSequence { get; }
        public long BattleTick { get; }
        public int ResetGeneration { get; }
        public int ResolvedDamage { get; }
        public int BoundedReturnDamage { get; }
        public string CanonicalSignature { get; }
    }

    public sealed class BoneSwapRemnantActionSnapshot
    {
        internal BoneSwapRemnantActionSnapshot(
            string executionId,
            long actionSequence,
            long startTick,
            long resolveTick,
            long recoveryEndTick,
            bool returnRequestEmitted,
            BoneSwapRemnantCapturedPulseSnapshot capturedPulse)
        {
            ExecutionId = executionId ?? string.Empty;
            ActionSequence = actionSequence;
            ActionId = BoneSwapRemnantRuntimeContract.ActionId;
            StartTick = startTick;
            ResolveTick = resolveTick;
            RecoveryEndTick = recoveryEndTick;
            ReturnRequestEmitted = returnRequestEmitted;
            CapturedPulse = capturedPulse;
            CanonicalSignature = BoneSwapRemnantCanonical.Hash(
                ExecutionId,
                BoneSwapRemnantCanonical.Number(ActionSequence),
                ActionId,
                BoneSwapRemnantCanonical.Number(StartTick),
                BoneSwapRemnantCanonical.Number(ResolveTick),
                BoneSwapRemnantCanonical.Number(RecoveryEndTick),
                BoneSwapRemnantCanonical.Flag(ReturnRequestEmitted),
                CapturedPulse == null
                    ? string.Empty
                    : CapturedPulse.CanonicalSignature);
        }

        public string ExecutionId { get; }
        public long ActionSequence { get; }
        public string ActionId { get; }
        public long StartTick { get; }
        public long ResolveTick { get; }
        public long RecoveryEndTick { get; }
        public bool ReturnRequestEmitted { get; }
        public BoneSwapRemnantCapturedPulseSnapshot CapturedPulse { get; }
        public string CanonicalSignature { get; }

        internal BoneSwapRemnantActionSnapshot MarkRequestEmitted()
        {
            return new BoneSwapRemnantActionSnapshot(
                ExecutionId,
                ActionSequence,
                StartTick,
                ResolveTick,
                RecoveryEndTick,
                true,
                CapturedPulse);
        }
    }

    public sealed class BoneSwapRemnantCueSnapshot
    {
        internal BoneSwapRemnantCueSnapshot(
            string cueId,
            long cueSequence,
            BoneSwapRemnantCueKind kind,
            int resetGeneration,
            long revision,
            string enemyInstanceId,
            long battleTick,
            string sourcePulseEventId)
        {
            CueId = cueId ?? string.Empty;
            CueSequence = cueSequence;
            Kind = kind;
            ResetGeneration = resetGeneration;
            Revision = revision;
            EnemyInstanceId = enemyInstanceId ?? string.Empty;
            BattleTick = battleTick;
            SourcePulseEventId = sourcePulseEventId ?? string.Empty;
            CanonicalSignature = BoneSwapRemnantCanonical.Hash(
                CueId,
                BoneSwapRemnantCanonical.Number(CueSequence),
                Kind.ToString(),
                BoneSwapRemnantCanonical.Number(ResetGeneration),
                BoneSwapRemnantCanonical.Number(Revision),
                EnemyInstanceId,
                BoneSwapRemnantCanonical.Number(BattleTick),
                SourcePulseEventId);
        }

        public string CueId { get; }
        public long CueSequence { get; }
        public BoneSwapRemnantCueKind Kind { get; }
        public int ResetGeneration { get; }
        public long Revision { get; }
        public string EnemyInstanceId { get; }
        public long BattleTick { get; }
        public string SourcePulseEventId { get; }
        public string CanonicalSignature { get; }
    }

    public sealed class BoneSwapRemnantEffectRequestSnapshot
    {
        internal BoneSwapRemnantEffectRequestSnapshot(
            string requestId,
            long requestSequence,
            string actionExecutionId,
            string enemyInstanceId,
            long battleResolveTick,
            int amount,
            BoneSwapRemnantTargetRequestKind targetRequestKind,
            int resetGeneration,
            long revision,
            string sourcePulseEventId)
        {
            RequestId = requestId ?? string.Empty;
            RequestSequence = requestSequence;
            ActionExecutionId = actionExecutionId ?? string.Empty;
            EffectRequestKey = BoneSwapRemnantRuntimeContract.EffectRequestKey;
            EnemyInstanceId = enemyInstanceId ?? string.Empty;
            BattleResolveTick = battleResolveTick;
            Amount = amount;
            TargetRequestKind = targetRequestKind;
            ResetGeneration = resetGeneration;
            Revision = revision;
            SourcePulseEventId = sourcePulseEventId ?? string.Empty;
            CanonicalSignature = BoneSwapRemnantCanonical.Hash(
                RequestId,
                BoneSwapRemnantCanonical.Number(RequestSequence),
                ActionExecutionId,
                EffectRequestKey,
                EnemyInstanceId,
                BoneSwapRemnantCanonical.Number(BattleResolveTick),
                BoneSwapRemnantCanonical.Number(Amount),
                TargetRequestKind.ToString(),
                BoneSwapRemnantCanonical.Number(ResetGeneration),
                BoneSwapRemnantCanonical.Number(Revision),
                SourcePulseEventId);
        }

        public string RequestId { get; }
        public long RequestSequence { get; }
        public string ActionExecutionId { get; }
        public string EffectRequestKey { get; }
        public string EnemyInstanceId { get; }
        public long BattleResolveTick { get; }
        public int Amount { get; }
        public BoneSwapRemnantTargetRequestKind TargetRequestKind { get; }
        public int ResetGeneration { get; }
        public long Revision { get; }
        public string SourcePulseEventId { get; }
        public string CanonicalSignature { get; }
    }

    internal sealed class BoneSwapRemnantStateData
    {
        public BoneSwapRemnantCopyProfile Profile;
        public string EnemyInstanceId = string.Empty;
        public BoneSwapRemnantRuntimeState State;
        public int ResetGeneration;
        public long Revision;
        public long CurrentBattleTick;
        public long LastAcceptedApplicationSequence;
        public long LastAcceptedBattleTick = -1L;
        public long AcceptedPulseCount;
        public long NextActionSequence = 1L;
        public long NextCueSequence = 1L;
        public long NextRequestSequence = 1L;
        public BoneSwapRemnantCapturedPulseSnapshot LatestCapturedPulse;
        public BoneSwapRemnantActionSnapshot ActiveAction;
        public BoneSwapRemnantCapturedPulseSnapshot PendingPulse;
        public readonly List<string> AcceptedEventIds = new List<string>();
        public readonly List<BoneSwapRemnantCueSnapshot> Cues =
            new List<BoneSwapRemnantCueSnapshot>();
        public readonly List<BoneSwapRemnantEffectRequestSnapshot> Requests =
            new List<BoneSwapRemnantEffectRequestSnapshot>();
        public readonly List<string> DeveloperDiagnostics = new List<string>();
    }

    public sealed class BoneSwapRemnantRuntimeSnapshot
    {
        internal BoneSwapRemnantRuntimeSnapshot(BoneSwapRemnantStateData data)
        {
            SchemaId = BoneSwapRemnantRuntimeContract.SchemaId;
            ContentId = BoneSwapRemnantRuntimeContract.ContentId;
            OperatorProfileId = BoneSwapRemnantRuntimeContract.OperatorProfileId;
            DevOnly = BoneSwapRemnantRuntimeContract.DevOnly;
            IsEnabled = BoneSwapRemnantRuntimeContract.IsEnabled;
            EntersFormalFlow = BoneSwapRemnantRuntimeContract.EntersFormalFlow;
            RuntimeBoundToBattle =
                BoneSwapRemnantRuntimeContract.RuntimeBoundToBattle;
            Profile = data.Profile;
            EnemyInstanceId = data.EnemyInstanceId;
            State = data.State;
            ResetGeneration = data.ResetGeneration;
            Revision = data.Revision;
            CurrentBattleTick = data.CurrentBattleTick;
            LastAcceptedApplicationSequence =
                data.LastAcceptedApplicationSequence;
            LastAcceptedBattleTick = data.LastAcceptedBattleTick;
            AcceptedPulseCount = data.AcceptedPulseCount;
            NextActionSequence = data.NextActionSequence;
            NextCueSequence = data.NextCueSequence;
            NextRequestSequence = data.NextRequestSequence;
            LatestCapturedPulse = data.LatestCapturedPulse;
            ActiveAction = data.ActiveAction;
            PendingPulse = data.PendingPulse;
            AcceptedEventIds = Freeze(data.AcceptedEventIds);
            Cues = Freeze(data.Cues);
            EffectRequests = Freeze(data.Requests);
            DeveloperDiagnostics = Freeze(data.DeveloperDiagnostics);
            CanonicalSignature = BoneSwapRemnantCanonical.Hash(
                SchemaId,
                ContentId,
                OperatorProfileId,
                BoneSwapRemnantCanonical.Flag(DevOnly),
                BoneSwapRemnantCanonical.Flag(IsEnabled),
                BoneSwapRemnantCanonical.Flag(EntersFormalFlow),
                BoneSwapRemnantCanonical.Flag(RuntimeBoundToBattle),
                Profile == null ? string.Empty : Profile.CanonicalSignature,
                EnemyInstanceId,
                State.ToString(),
                BoneSwapRemnantCanonical.Number(ResetGeneration),
                BoneSwapRemnantCanonical.Number(Revision),
                BoneSwapRemnantCanonical.Number(CurrentBattleTick),
                BoneSwapRemnantCanonical.Number(
                    LastAcceptedApplicationSequence),
                BoneSwapRemnantCanonical.Number(LastAcceptedBattleTick),
                BoneSwapRemnantCanonical.Number(AcceptedPulseCount),
                BoneSwapRemnantCanonical.Number(NextActionSequence),
                BoneSwapRemnantCanonical.Number(NextCueSequence),
                BoneSwapRemnantCanonical.Number(NextRequestSequence),
                LatestCapturedPulse == null
                    ? string.Empty
                    : LatestCapturedPulse.CanonicalSignature,
                ActiveAction == null
                    ? string.Empty
                    : ActiveAction.CanonicalSignature,
                PendingPulse == null
                    ? string.Empty
                    : PendingPulse.CanonicalSignature,
                BoneSwapRemnantCanonical.JoinSignatures(
                    AcceptedEventIds,
                    value => value,
                    true),
                BoneSwapRemnantCanonical.JoinSignatures(
                    Cues,
                    value => value.CanonicalSignature,
                    false),
                BoneSwapRemnantCanonical.JoinSignatures(
                    EffectRequests,
                    value => value.CanonicalSignature,
                    false),
                BoneSwapRemnantCanonical.JoinSignatures(
                    DeveloperDiagnostics,
                    value => value,
                    true));
        }

        public string SchemaId { get; }
        public string ContentId { get; }
        public string OperatorProfileId { get; }
        public bool DevOnly { get; }
        public bool IsEnabled { get; }
        public bool EntersFormalFlow { get; }
        public bool RuntimeBoundToBattle { get; }
        public BoneSwapRemnantCopyProfile Profile { get; }
        public string EnemyInstanceId { get; }
        public BoneSwapRemnantRuntimeState State { get; }
        public int ResetGeneration { get; }
        public long Revision { get; }
        public long CurrentBattleTick { get; }
        public long LastAcceptedApplicationSequence { get; }
        public long LastAcceptedBattleTick { get; }
        public long AcceptedPulseCount { get; }
        public long NextActionSequence { get; }
        public long NextCueSequence { get; }
        public long NextRequestSequence { get; }
        public BoneSwapRemnantCapturedPulseSnapshot LatestCapturedPulse { get; }
        public BoneSwapRemnantActionSnapshot ActiveAction { get; }
        public BoneSwapRemnantCapturedPulseSnapshot PendingPulse { get; }
        public IReadOnlyList<string> AcceptedEventIds { get; }
        public IReadOnlyList<BoneSwapRemnantCueSnapshot> Cues { get; }
        public IReadOnlyList<BoneSwapRemnantEffectRequestSnapshot>
            EffectRequests { get; }
        public IReadOnlyList<string> DeveloperDiagnostics { get; }
        public string CanonicalSignature { get; }

        private static IReadOnlyList<T> Freeze<T>(IEnumerable<T> values)
        {
            return new ReadOnlyCollection<T>(
                (values ?? Array.Empty<T>()).ToArray());
        }
    }

    public sealed class BoneSwapRemnantTransitionResult
    {
        internal BoneSwapRemnantTransitionResult(
            bool accepted,
            bool stateChanged,
            string diagnostic,
            BoneSwapRemnantRuntimeSnapshot snapshot,
            IEnumerable<BoneSwapRemnantCueSnapshot> emittedCues,
            IEnumerable<BoneSwapRemnantEffectRequestSnapshot> emittedRequests)
        {
            Accepted = accepted;
            StateChanged = stateChanged;
            Diagnostic = diagnostic ?? string.Empty;
            Snapshot = snapshot;
            EmittedCues = new ReadOnlyCollection<BoneSwapRemnantCueSnapshot>(
                (emittedCues ?? Array.Empty<BoneSwapRemnantCueSnapshot>())
                    .ToArray());
            EmittedRequests =
                new ReadOnlyCollection<BoneSwapRemnantEffectRequestSnapshot>(
                    (emittedRequests
                        ?? Array.Empty<BoneSwapRemnantEffectRequestSnapshot>())
                        .ToArray());
        }

        public bool Accepted { get; }
        public bool StateChanged { get; }
        public string Diagnostic { get; }
        public BoneSwapRemnantRuntimeSnapshot Snapshot { get; }
        public IReadOnlyList<BoneSwapRemnantCueSnapshot> EmittedCues { get; }
        public IReadOnlyList<BoneSwapRemnantEffectRequestSnapshot>
            EmittedRequests { get; }
    }
}
