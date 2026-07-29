using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime
{
    public enum C1EnemyLifecycle
    {
        Disabled = 0,
        Present = 1,
        Active = 2,
        Defeated = 3
    }

    public enum C1EnemyShellState
    {
        NotApplicable = 0,
        Intact = 1,
        Broken = 2
    }

    public enum C1EnemySelfPossessionState
    {
        NotApplicable = 0,
        DeclaredOwnStateOnly = 1
    }

    public enum C1EnemyCueKind
    {
        Presence = 0,
        BasicAttack = 1,
        ChargeAttack = 2,
        Hit = 3,
        ShellHit = 4,
        ShellBreak = 5,
        CounterWindowRequested = 6,
        Defeated = 7,
        PostDefeatFieldRequested = 8,
        Reset = 9
    }

    public enum C1EnemyActionStatus
    {
        Telegraph = 0,
        Resolved = 1,
        Recovering = 2
    }

    public enum C1EnemyInterruptPolicy
    {
        ReactiveCueFirst = 0
    }

    public static class C1EnemyRuntimeContract
    {
        public const string SchemaId = "BoneAspectC1EnemyRuntime.v1";
        public const int SchemaVersion = 1;
        public const bool DevOnly = true;
        public const bool IsEnabled = false;
        public const bool EntersFormalFlow = false;
        public const bool RuntimeBoundToBattle = false;
        public const int TickRate = 1000;

        public const string ShatteredHostContentId =
            "bone_aspect_enemy_c1_01_shattered_host";
        public const string ShatteredHostProfileId =
            "bone_aspect.runtime.c1.shattered_host.v1";
        public const string ShatteredHostPresentationKey =
            "enemy.bone_aspect.c1.shattered_host";
        public const string ShatteredHostActionId =
            "c1.shattered_host.basic_attack";

        public const string PorcelainHoundContentId =
            "bone_aspect_enemy_c1_02_porcelain_hound";
        public const string PorcelainHoundProfileId =
            "bone_aspect.runtime.c1.porcelain_hound.v1";
        public const string PorcelainHoundPresentationKey =
            "enemy.bone_aspect.c1.porcelain_hound";
        public const string PorcelainHoundActionId =
            "c1.porcelain_hound.charge_attack";

        public const string BoneSwapRemnantContentId =
            "bone_aspect_enemy_c1_03_bone_swap_remnant";
        public const string BoneSwapRemnantHoldStatus = "HELD_BY_BA-D3";

        public const string DirectPlayerDamageRequest =
            "battle.effect_request.direct_player_damage";
        public const string ChargeAttackRequest =
            "battle.effect_request.charge_attack";
        public const string PostDefeatFieldRequest =
            "battle.effect_request.post_defeat_field_area";
        public const string ShellBreakCounterWindow =
            "counter_window.shell_break";
    }

    public sealed class C1EnemyBattleApplicationResult
    {
        public C1EnemyBattleApplicationResult(
            string applicationEventId,
            long applicationSequence,
            int resetGeneration,
            long battleTick,
            string targetEnemyInstanceId,
            bool acceptedByBattleLedger,
            int actualHpDeltaApplied,
            int actualShellDeltaApplied,
            string sourceRequestId)
        {
            ApplicationEventId = applicationEventId ?? string.Empty;
            ApplicationSequence = applicationSequence;
            ResetGeneration = resetGeneration;
            BattleTick = battleTick;
            TargetEnemyInstanceId = targetEnemyInstanceId ?? string.Empty;
            AcceptedByBattleLedger = acceptedByBattleLedger;
            ActualHpDeltaApplied = actualHpDeltaApplied;
            ActualShellDeltaApplied = actualShellDeltaApplied;
            SourceRequestId = sourceRequestId ?? string.Empty;
        }

        public string ApplicationEventId { get; private set; }
        public long ApplicationSequence { get; private set; }
        public int ResetGeneration { get; private set; }
        public long BattleTick { get; private set; }
        public string TargetEnemyInstanceId { get; private set; }
        public bool AcceptedByBattleLedger { get; private set; }
        public int ActualHpDeltaApplied { get; private set; }
        public int ActualShellDeltaApplied { get; private set; }
        public string SourceRequestId { get; private set; }
    }

    public sealed class C1EnemyCueSnapshot
    {
        public C1EnemyCueSnapshot(
            string cueId,
            long cueSequence,
            C1EnemyCueKind kind,
            int resetGeneration,
            long revision,
            string sourceIdentity,
            long battleTick)
        {
            CueId = cueId ?? string.Empty;
            CueSequence = cueSequence;
            Kind = kind;
            ResetGeneration = resetGeneration;
            Revision = revision;
            SourceIdentity = sourceIdentity ?? string.Empty;
            BattleTick = battleTick;
        }

        public string CueId { get; private set; }
        public long CueSequence { get; private set; }
        public C1EnemyCueKind Kind { get; private set; }
        public int ResetGeneration { get; private set; }
        public long Revision { get; private set; }
        public string SourceIdentity { get; private set; }
        public long BattleTick { get; private set; }
    }

    public sealed class C1EnemyEffectRequestSnapshot
    {
        public C1EnemyEffectRequestSnapshot(
            string requestId,
            long requestSequence,
            string requestKey,
            int resetGeneration,
            long revision,
            string sourceIdentity,
            long battleTick,
            string lifetimeClass,
            int requestedDurationTicks)
        {
            RequestId = requestId ?? string.Empty;
            RequestSequence = requestSequence;
            RequestKey = requestKey ?? string.Empty;
            ResetGeneration = resetGeneration;
            Revision = revision;
            SourceIdentity = sourceIdentity ?? string.Empty;
            BattleTick = battleTick;
            LifetimeClass = lifetimeClass ?? string.Empty;
            RequestedDurationTicks = requestedDurationTicks;
        }

        public string RequestId { get; private set; }
        public long RequestSequence { get; private set; }
        public string RequestKey { get; private set; }
        public int ResetGeneration { get; private set; }
        public long Revision { get; private set; }
        public string SourceIdentity { get; private set; }
        public long BattleTick { get; private set; }
        public string LifetimeClass { get; private set; }
        public int RequestedDurationTicks { get; private set; }
    }

    public sealed class C1EnemyActiveActionSnapshot
    {
        public C1EnemyActiveActionSnapshot(
            string executionId,
            long executionSequence,
            string actionPatternId,
            C1EnemyActionStatus status,
            long startTick,
            long resolveTick,
            long recoveryEndTick)
        {
            ExecutionId = executionId ?? string.Empty;
            ExecutionSequence = executionSequence;
            ActionPatternId = actionPatternId ?? string.Empty;
            Status = status;
            StartTick = startTick;
            ResolveTick = resolveTick;
            RecoveryEndTick = recoveryEndTick;
        }

        public string ExecutionId { get; private set; }
        public long ExecutionSequence { get; private set; }
        public string ActionPatternId { get; private set; }
        public C1EnemyActionStatus Status { get; private set; }
        public long StartTick { get; private set; }
        public long ResolveTick { get; private set; }
        public long RecoveryEndTick { get; private set; }

        internal C1EnemyActiveActionSnapshot WithStatus(C1EnemyActionStatus status)
        {
            return new C1EnemyActiveActionSnapshot(
                ExecutionId,
                ExecutionSequence,
                ActionPatternId,
                status,
                StartTick,
                ResolveTick,
                RecoveryEndTick);
        }
    }

    public sealed class C1EnemyTransitionResult
    {
        public C1EnemyTransitionResult(
            C1EnemyRuntimeSnapshot snapshot,
            bool accepted,
            string error)
        {
            Snapshot = snapshot;
            Accepted = accepted;
            Error = error ?? string.Empty;
        }

        public C1EnemyRuntimeSnapshot Snapshot { get; private set; }
        public bool Accepted { get; private set; }
        public string Error { get; private set; }
    }

    public sealed class C1EnemyHoldAssertion
    {
        public C1EnemyHoldAssertion(
            string contentId,
            string status,
            int runtimeProfiles,
            int actionPatterns,
            int copySlots,
            int runtimeBindings)
        {
            ContentId = contentId ?? string.Empty;
            Status = status ?? string.Empty;
            RuntimeProfiles = runtimeProfiles;
            ActionPatterns = actionPatterns;
            CopySlots = copySlots;
            RuntimeBindings = runtimeBindings;
        }

        public string ContentId { get; private set; }
        public string Status { get; private set; }
        public int RuntimeProfiles { get; private set; }
        public int ActionPatterns { get; private set; }
        public int CopySlots { get; private set; }
        public int RuntimeBindings { get; private set; }
    }

    internal sealed class C1EnemyRuntimeStateData
    {
        public string SchemaId = C1EnemyRuntimeContract.SchemaId;
        public int SchemaVersion = C1EnemyRuntimeContract.SchemaVersion;
        public bool DevOnly = C1EnemyRuntimeContract.DevOnly;
        public bool IsEnabled = C1EnemyRuntimeContract.IsEnabled;
        public bool EntersFormalFlow = C1EnemyRuntimeContract.EntersFormalFlow;
        public bool RuntimeBoundToBattle = C1EnemyRuntimeContract.RuntimeBoundToBattle;
        public string EnemyInstanceId = string.Empty;
        public string RuntimeProfileId = string.Empty;
        public string ContentId = string.Empty;
        public string PresentationKey = string.Empty;
        public int ResetGeneration;
        public long Revision;
        public C1EnemyLifecycle Lifecycle;
        public int MaxHp;
        public int CurrentHp;
        public int ShellMax;
        public int CurrentShell;
        public C1EnemyShellState ShellState;
        public C1EnemySelfPossessionState SelfPossessionState;
        public bool SelfPossessionGameplayEffectAuthored;
        public bool Targetable;
        public long LastAcceptedBattleTick = -1L;
        public long LastAcceptedApplicationSequence = -1L;
        public int AcceptedApplicationCount;
        public long LastSchedulerTick = -1L;
        public long NextActionDueTick;
        public long NextCueSequence = 1L;
        public long NextRequestSequence = 1L;
        public long NextExecutionSequence = 1L;
        public bool ShellBreakRequestEmitted;
        public bool PostDefeatFieldRequestEmitted;
        public C1EnemyActiveActionSnapshot ActiveAction;
        public List<string> AcceptedApplicationEventIds = new List<string>();
        public List<C1EnemyEffectRequestSnapshot> PendingRequests =
            new List<C1EnemyEffectRequestSnapshot>();
        public List<C1EnemyCueSnapshot> Cues = new List<C1EnemyCueSnapshot>();
        public C1EnemyCueSnapshot LatestCue;
        public List<string> Errors = new List<string>();
        public List<string> PublicErrors = new List<string>();
        public List<string> DeveloperDiagnostics = new List<string>();

        public C1EnemyRuntimeStateData Clone()
        {
            C1EnemyRuntimeStateData clone = (C1EnemyRuntimeStateData)MemberwiseClone();
            clone.AcceptedApplicationEventIds =
                new List<string>(AcceptedApplicationEventIds);
            clone.PendingRequests =
                new List<C1EnemyEffectRequestSnapshot>(PendingRequests);
            clone.Cues = new List<C1EnemyCueSnapshot>(Cues);
            clone.Errors = new List<string>(Errors);
            clone.PublicErrors = new List<string>(PublicErrors);
            clone.DeveloperDiagnostics = new List<string>(DeveloperDiagnostics);
            return clone;
        }
    }

    internal static class C1EnemyCollection
    {
        public static IReadOnlyList<T> Copy<T>(IEnumerable<T> source)
        {
            return new ReadOnlyCollection<T>((source ?? Enumerable.Empty<T>()).ToArray());
        }

        public static IReadOnlyList<string> CopySorted(IEnumerable<string> source)
        {
            string[] values = (source ?? Enumerable.Empty<string>())
                .Select(value => value ?? string.Empty)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            return new ReadOnlyCollection<string>(values);
        }
    }
}
