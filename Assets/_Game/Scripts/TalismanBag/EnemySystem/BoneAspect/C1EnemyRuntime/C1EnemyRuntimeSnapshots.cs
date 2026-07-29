using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime
{
    public sealed class C1EnemyRuntimeProfileSnapshot
    {
        private readonly IReadOnlyList<string> _mechanicKeys;
        private readonly IReadOnlyList<string> _actionPatternIds;

        public C1EnemyRuntimeProfileSnapshot(
            string runtimeProfileId,
            string contentId,
            string displayName,
            string presentationKey,
            IEnumerable<string> mechanicKeys,
            IEnumerable<string> actionPatternIds,
            int maxHp,
            int shellMax,
            int initialShell,
            int shellRegenerationCount,
            string postDefeatEffectRequestKey,
            string shellBreakCounterWindowKey,
            int requestedCounterWindowTicks,
            bool devOnlyFixture,
            bool formalBalance,
            bool devOnly,
            bool isEnabled,
            bool entersFormalFlow,
            bool runtimeBoundToBattle)
        {
            RuntimeProfileId = runtimeProfileId ?? string.Empty;
            ContentId = contentId ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            PresentationKey = presentationKey ?? string.Empty;
            _mechanicKeys = C1EnemyCollection.CopySorted(mechanicKeys);
            _actionPatternIds = C1EnemyCollection.CopySorted(actionPatternIds);
            MaxHp = maxHp;
            ShellMax = shellMax;
            InitialShell = initialShell;
            ShellRegenerationCount = shellRegenerationCount;
            PostDefeatEffectRequestKey = postDefeatEffectRequestKey ?? string.Empty;
            ShellBreakCounterWindowKey = shellBreakCounterWindowKey ?? string.Empty;
            RequestedCounterWindowTicks = requestedCounterWindowTicks;
            DevOnlyFixture = devOnlyFixture;
            FormalBalance = formalBalance;
            LiveTuning = false;
            DevOnly = devOnly;
            IsEnabled = isEnabled;
            EntersFormalFlow = entersFormalFlow;
            RuntimeBoundToBattle = runtimeBoundToBattle;
            CanonicalSignature = C1EnemyCanonical.Hash(CanonicalPayload());
        }

        public string RuntimeProfileId { get; private set; }
        public string ContentId { get; private set; }
        public string DisplayName { get; private set; }
        public string PresentationKey { get; private set; }
        public IReadOnlyList<string> MechanicKeys { get { return _mechanicKeys; } }
        public IReadOnlyList<string> ActionPatternIds { get { return _actionPatternIds; } }
        public int MaxHp { get; private set; }
        public int ShellMax { get; private set; }
        public int InitialShell { get; private set; }
        public int ShellRegenerationCount { get; private set; }
        public string PostDefeatEffectRequestKey { get; private set; }
        public string ShellBreakCounterWindowKey { get; private set; }
        public int RequestedCounterWindowTicks { get; private set; }
        public bool DevOnlyFixture { get; private set; }
        public bool FormalBalance { get; private set; }
        public bool LiveTuning { get; private set; }
        public bool DevOnly { get; private set; }
        public bool IsEnabled { get; private set; }
        public bool EntersFormalFlow { get; private set; }
        public bool RuntimeBoundToBattle { get; private set; }
        public string CanonicalSignature { get; private set; }

        internal string CanonicalPayload()
        {
            return string.Join("|", new[]
            {
                RuntimeProfileId,
                ContentId,
                DisplayName,
                PresentationKey,
                string.Join(",", _mechanicKeys),
                string.Join(",", _actionPatternIds),
                C1EnemyCanonical.Number(MaxHp),
                C1EnemyCanonical.Number(ShellMax),
                C1EnemyCanonical.Number(InitialShell),
                C1EnemyCanonical.Number(ShellRegenerationCount),
                PostDefeatEffectRequestKey,
                ShellBreakCounterWindowKey,
                C1EnemyCanonical.Number(RequestedCounterWindowTicks),
                C1EnemyCanonical.Flag(DevOnlyFixture),
                C1EnemyCanonical.Flag(FormalBalance),
                C1EnemyCanonical.Flag(LiveTuning),
                C1EnemyCanonical.Flag(DevOnly),
                C1EnemyCanonical.Flag(IsEnabled),
                C1EnemyCanonical.Flag(EntersFormalFlow),
                C1EnemyCanonical.Flag(RuntimeBoundToBattle)
            });
        }
    }

    public sealed class C1EnemyActionPatternSnapshot
    {
        public C1EnemyActionPatternSnapshot(
            string actionPatternId,
            string ownerRuntimeProfileId,
            string mechanicKey,
            string effectRequestKey,
            long firstDueTick,
            long repeatIntervalTicks,
            long telegraphTicks,
            long castTicks,
            long resolveOffsetTicks,
            long recoverTicks,
            C1EnemyInterruptPolicy interruptPolicy,
            int priority,
            bool devOnlyFixture)
        {
            ActionPatternId = actionPatternId ?? string.Empty;
            OwnerRuntimeProfileId = ownerRuntimeProfileId ?? string.Empty;
            MechanicKey = mechanicKey ?? string.Empty;
            EffectRequestKey = effectRequestKey ?? string.Empty;
            FirstDueTick = firstDueTick;
            RepeatIntervalTicks = repeatIntervalTicks;
            TelegraphTicks = telegraphTicks;
            CastTicks = castTicks;
            ResolveOffsetTicks = resolveOffsetTicks;
            RecoverTicks = recoverTicks;
            InterruptPolicy = interruptPolicy;
            Priority = priority;
            DevOnlyFixture = devOnlyFixture;
            CanonicalSignature = C1EnemyCanonical.Hash(CanonicalPayload());
        }

        public string ActionPatternId { get; private set; }
        public string OwnerRuntimeProfileId { get; private set; }
        public string MechanicKey { get; private set; }
        public string EffectRequestKey { get; private set; }
        public long FirstDueTick { get; private set; }
        public long RepeatIntervalTicks { get; private set; }
        public long TelegraphTicks { get; private set; }
        public long CastTicks { get; private set; }
        public long ResolveOffsetTicks { get; private set; }
        public long RecoverTicks { get; private set; }
        public C1EnemyInterruptPolicy InterruptPolicy { get; private set; }
        public int Priority { get; private set; }
        public bool DevOnlyFixture { get; private set; }
        public string CanonicalSignature { get; private set; }

        internal string CanonicalPayload()
        {
            return string.Join("|", new[]
            {
                ActionPatternId,
                OwnerRuntimeProfileId,
                MechanicKey,
                EffectRequestKey,
                C1EnemyCanonical.Number(FirstDueTick),
                C1EnemyCanonical.Number(RepeatIntervalTicks),
                C1EnemyCanonical.Number(TelegraphTicks),
                C1EnemyCanonical.Number(CastTicks),
                C1EnemyCanonical.Number(ResolveOffsetTicks),
                C1EnemyCanonical.Number(RecoverTicks),
                InterruptPolicy.ToString(),
                C1EnemyCanonical.Number(Priority),
                C1EnemyCanonical.Flag(DevOnlyFixture)
            });
        }
    }

    public sealed class C1EnemyRuntimeSnapshot
    {
        private readonly C1EnemyRuntimeStateData _data;

        internal C1EnemyRuntimeSnapshot(C1EnemyRuntimeStateData data)
        {
            _data = (data ?? new C1EnemyRuntimeStateData()).Clone();
            FullCanonicalSignature = C1EnemyCanonical.Hash(C1EnemyCanonical.Full(_data));
            PresentationSafeCanonicalSignature =
                C1EnemyCanonical.Hash(C1EnemyCanonical.Presentation(_data));
        }

        public string SchemaId { get { return _data.SchemaId; } }
        public int SchemaVersion { get { return _data.SchemaVersion; } }
        public bool DevOnly { get { return _data.DevOnly; } }
        public bool IsEnabled { get { return _data.IsEnabled; } }
        public bool EntersFormalFlow { get { return _data.EntersFormalFlow; } }
        public bool RuntimeBoundToBattle { get { return _data.RuntimeBoundToBattle; } }
        public string EnemyInstanceId { get { return _data.EnemyInstanceId; } }
        public string RuntimeProfileId { get { return _data.RuntimeProfileId; } }
        public string ContentId { get { return _data.ContentId; } }
        public string PresentationKey { get { return _data.PresentationKey; } }
        public int ResetGeneration { get { return _data.ResetGeneration; } }
        public long Revision { get { return _data.Revision; } }
        public C1EnemyLifecycle Lifecycle { get { return _data.Lifecycle; } }
        public int MaxHp { get { return _data.MaxHp; } }
        public int CurrentHp { get { return _data.CurrentHp; } }
        public int ShellMax { get { return _data.ShellMax; } }
        public int CurrentShell { get { return _data.CurrentShell; } }
        public C1EnemyShellState ShellState { get { return _data.ShellState; } }
        public C1EnemySelfPossessionState SelfPossessionState
        {
            get { return _data.SelfPossessionState; }
        }
        public bool SelfPossessionGameplayEffectAuthored
        {
            get { return _data.SelfPossessionGameplayEffectAuthored; }
        }
        public bool Targetable { get { return _data.Targetable; } }
        public long LastAcceptedBattleTick { get { return _data.LastAcceptedBattleTick; } }
        public long LastAcceptedApplicationSequence
        {
            get { return _data.LastAcceptedApplicationSequence; }
        }
        public int AcceptedApplicationCount { get { return _data.AcceptedApplicationCount; } }
        public long LastSchedulerTick { get { return _data.LastSchedulerTick; } }
        public long NextActionDueTick { get { return _data.NextActionDueTick; } }
        public bool ShellBreakRequestEmitted { get { return _data.ShellBreakRequestEmitted; } }
        public bool PostDefeatFieldRequestEmitted
        {
            get { return _data.PostDefeatFieldRequestEmitted; }
        }
        public C1EnemyActiveActionSnapshot ActiveAction { get { return _data.ActiveAction; } }
        public IReadOnlyList<string> AcceptedApplicationEventIds
        {
            get { return C1EnemyCollection.Copy(_data.AcceptedApplicationEventIds); }
        }
        public IReadOnlyList<C1EnemyEffectRequestSnapshot> PendingRequests
        {
            get { return C1EnemyCollection.Copy(_data.PendingRequests); }
        }
        public IReadOnlyList<C1EnemyCueSnapshot> Cues
        {
            get { return C1EnemyCollection.Copy(_data.Cues); }
        }
        public C1EnemyCueSnapshot LatestCue { get { return _data.LatestCue; } }
        public IReadOnlyList<string> Errors { get { return C1EnemyCollection.Copy(_data.Errors); } }
        public IReadOnlyList<string> PublicErrors
        {
            get { return C1EnemyCollection.Copy(_data.PublicErrors); }
        }
        public IReadOnlyList<string> DeveloperDiagnostics
        {
            get { return C1EnemyCollection.Copy(_data.DeveloperDiagnostics); }
        }
        public string FullCanonicalSignature { get; private set; }
        public string PresentationSafeCanonicalSignature { get; private set; }

        public C1EnemyPresentationSafeSnapshot ToPresentationSafe()
        {
            return new C1EnemyPresentationSafeSnapshot(_data);
        }

        internal C1EnemyRuntimeStateData ToMutable()
        {
            return _data.Clone();
        }
    }

    public sealed class C1EnemyPresentationSafeSnapshot
    {
        internal C1EnemyPresentationSafeSnapshot(C1EnemyRuntimeStateData data)
        {
            EnemyInstanceId = data.EnemyInstanceId;
            ContentId = data.ContentId;
            PresentationKey = data.PresentationKey;
            ResetGeneration = data.ResetGeneration;
            Revision = data.Revision;
            Lifecycle = data.Lifecycle;
            MaxHp = data.MaxHp;
            CurrentHp = data.CurrentHp;
            ShellMax = data.ShellMax;
            CurrentShell = data.CurrentShell;
            ShellState = data.ShellState;
            SelfPossessionState = data.SelfPossessionState;
            Targetable = data.Targetable;
            LatestCue = data.LatestCue;
            PublicErrors = C1EnemyCollection.Copy(data.PublicErrors);
            CanonicalSignature = C1EnemyCanonical.Hash(C1EnemyCanonical.Presentation(data));
        }

        public string EnemyInstanceId { get; private set; }
        public string ContentId { get; private set; }
        public string PresentationKey { get; private set; }
        public int ResetGeneration { get; private set; }
        public long Revision { get; private set; }
        public C1EnemyLifecycle Lifecycle { get; private set; }
        public int MaxHp { get; private set; }
        public int CurrentHp { get; private set; }
        public int ShellMax { get; private set; }
        public int CurrentShell { get; private set; }
        public C1EnemyShellState ShellState { get; private set; }
        public C1EnemySelfPossessionState SelfPossessionState { get; private set; }
        public bool Targetable { get; private set; }
        public C1EnemyCueSnapshot LatestCue { get; private set; }
        public IReadOnlyList<string> PublicErrors { get; private set; }
        public string CanonicalSignature { get; private set; }
    }

    internal static class C1EnemyCanonical
    {
        public static string Full(C1EnemyRuntimeStateData data)
        {
            string action = data.ActiveAction == null
                ? string.Empty
                : string.Join("~", new[]
                {
                    data.ActiveAction.ExecutionId,
                    Number(data.ActiveAction.ExecutionSequence),
                    data.ActiveAction.ActionPatternId,
                    data.ActiveAction.Status.ToString(),
                    Number(data.ActiveAction.StartTick),
                    Number(data.ActiveAction.ResolveTick),
                    Number(data.ActiveAction.RecoveryEndTick)
                });
            string requests = string.Join(",", data.PendingRequests.Select(Request));
            string cues = string.Join(",", data.Cues.Select(Cue));
            return string.Join("|", new[]
            {
                data.SchemaId,
                Number(data.SchemaVersion),
                Flag(data.DevOnly),
                Flag(data.IsEnabled),
                Flag(data.EntersFormalFlow),
                Flag(data.RuntimeBoundToBattle),
                data.EnemyInstanceId,
                data.RuntimeProfileId,
                data.ContentId,
                data.PresentationKey,
                Number(data.ResetGeneration),
                Number(data.Revision),
                data.Lifecycle.ToString(),
                Number(data.MaxHp),
                Number(data.CurrentHp),
                Number(data.ShellMax),
                Number(data.CurrentShell),
                data.ShellState.ToString(),
                data.SelfPossessionState.ToString(),
                Flag(data.SelfPossessionGameplayEffectAuthored),
                Flag(data.Targetable),
                Number(data.LastAcceptedBattleTick),
                Number(data.LastAcceptedApplicationSequence),
                Number(data.AcceptedApplicationCount),
                Number(data.LastSchedulerTick),
                Number(data.NextActionDueTick),
                Flag(data.ShellBreakRequestEmitted),
                Flag(data.PostDefeatFieldRequestEmitted),
                action,
                string.Join(",", data.AcceptedApplicationEventIds.OrderBy(v => v, StringComparer.Ordinal)),
                requests,
                cues,
                data.LatestCue == null ? string.Empty : Cue(data.LatestCue),
                string.Join(",", data.Errors.OrderBy(v => v, StringComparer.Ordinal)),
                string.Join(",", data.PublicErrors.OrderBy(v => v, StringComparer.Ordinal)),
                string.Join(",", data.DeveloperDiagnostics.OrderBy(v => v, StringComparer.Ordinal)),
                Number(data.NextCueSequence),
                Number(data.NextRequestSequence),
                Number(data.NextExecutionSequence)
            });
        }

        public static string Presentation(C1EnemyRuntimeStateData data)
        {
            return string.Join("|", new[]
            {
                data.EnemyInstanceId,
                data.ContentId,
                data.PresentationKey,
                Number(data.ResetGeneration),
                Number(data.Revision),
                data.Lifecycle.ToString(),
                Number(data.MaxHp),
                Number(data.CurrentHp),
                Number(data.ShellMax),
                Number(data.CurrentShell),
                data.ShellState.ToString(),
                data.SelfPossessionState.ToString(),
                Flag(data.Targetable),
                data.LatestCue == null ? string.Empty : Cue(data.LatestCue),
                string.Join(",", data.PublicErrors.OrderBy(v => v, StringComparer.Ordinal))
            });
        }

        public static string Hash(string payload)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = new UTF8Encoding(false).GetBytes(payload ?? string.Empty);
                return string.Concat(sha.ComputeHash(bytes).Select(value =>
                    value.ToString("x2", CultureInfo.InvariantCulture)));
            }
        }

        public static string Number(long value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public static string Flag(bool value)
        {
            return value ? "1" : "0";
        }

        private static string Request(C1EnemyEffectRequestSnapshot request)
        {
            return string.Join("~", new[]
            {
                request.RequestId,
                Number(request.RequestSequence),
                request.RequestKey,
                Number(request.ResetGeneration),
                Number(request.Revision),
                request.SourceIdentity,
                Number(request.BattleTick),
                request.LifetimeClass,
                Number(request.RequestedDurationTicks)
            });
        }

        private static string Cue(C1EnemyCueSnapshot cue)
        {
            return string.Join("~", new[]
            {
                cue.CueId,
                Number(cue.CueSequence),
                cue.Kind.ToString(),
                Number(cue.ResetGeneration),
                Number(cue.Revision),
                cue.SourceIdentity,
                Number(cue.BattleTick)
            });
        }
    }
}
