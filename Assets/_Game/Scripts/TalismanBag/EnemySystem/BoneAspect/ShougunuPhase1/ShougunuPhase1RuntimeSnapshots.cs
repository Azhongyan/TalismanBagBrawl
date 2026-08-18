using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TalismanBag.EnemySystem.BoneAspect.ShougunuPhase1
{
    public sealed class ShougunuPhase1RuntimeSnapshot
    {
        private readonly ShougunuPhase1RuntimeStateData _data;

        internal ShougunuPhase1RuntimeSnapshot(ShougunuPhase1RuntimeStateData data)
        {
            _data = (data ?? new ShougunuPhase1RuntimeStateData()).Clone();
            CanonicalSignature = ShougunuPhase1Canonical.Full(_data);
        }

        public string SchemaId { get { return _data.SchemaId; } }
        public int SchemaVersion { get { return _data.SchemaVersion; } }
        public string EnemyInstanceId { get { return _data.EnemyInstanceId; } }
        public string ContentId { get { return _data.ContentId; } }
        public string PhaseId { get { return _data.PhaseId; } }
        public bool DevOnly { get { return _data.DevOnly; } }
        public bool IsEnabled { get { return _data.IsEnabled; } }
        public bool EntersFormalFlow { get { return _data.EntersFormalFlow; } }
        public bool RuntimeBoundToBattle { get { return _data.RuntimeBoundToBattle; } }
        public int ResetGeneration { get { return _data.ResetGeneration; } }
        public long Revision { get { return _data.Revision; } }
        public ShougunuPhase1LifecycleState Lifecycle { get { return _data.Lifecycle; } }
        public int MaxHp { get { return _data.MaxHp; } }
        public int CurrentHp { get { return _data.CurrentHp; } }
        public int ShellLayerIndex { get { return _data.ShellLayerIndex; } }
        public int MaxSequentialShellLayers { get { return _data.MaxSequentialShellLayers; } }
        public int ShellLayerMax { get { return _data.ShellLayerMax; } }
        public int CurrentShell { get { return _data.CurrentShell; } }
        public bool Targetable { get { return _data.Targetable; } }
        public bool Vulnerable { get { return _data.Vulnerable; } }
        public long CoreExposeStartTick { get { return _data.CoreExposeStartTick; } }
        public long CoreExposeEndTick { get { return _data.CoreExposeEndTick; } }
        public bool RecoveryAvailable { get { return _data.RecoveryAvailable; } }
        public long LastAcceptedBattleTick { get { return _data.LastAcceptedBattleTick; } }
        public long LastAcceptedApplicationSequence { get { return _data.LastAcceptedApplicationSequence; } }
        public int AcceptedDamageApplicationCount { get { return _data.AcceptedDamageApplicationCount; } }
        public int BasicResolvedCount { get { return _data.BasicResolvedCount; } }
        public int BasicEarnedCount
        {
            get
            {
                return _data.AcceptedDamageApplicationCount
                    / ShougunuPhase1RuntimeContract.BasicDamageApplicationInterval;
            }
        }
        public int BasicDebt { get { return BasicEarnedCount - _data.BasicResolvedCount; } }
        public long NextCueSequence { get { return _data.NextCueSequence; } }
        public long NextExecutionSequence { get { return _data.NextExecutionSequence; } }
        public IReadOnlyList<string> AcceptedApplicationEventIds
        {
            get { return ShougunuPhase1Collection.Copy(_data.AcceptedApplicationEventIds); }
        }
        public IReadOnlyList<ShougunuPhase1ThresholdOccurrenceSnapshot> ThresholdOccurrences
        {
            get { return ShougunuPhase1Collection.Copy(_data.ThresholdOccurrences); }
        }
        public ShougunuPhase1PendingActionSnapshot ActiveAction { get { return _data.ActiveAction; } }
        public IReadOnlyList<ShougunuPhase1CueSnapshot> Cues
        {
            get { return ShougunuPhase1Collection.Copy(_data.Cues); }
        }
        public ShougunuPhase1CueSnapshot LatestCue
        {
            get { return _data.Cues.Count == 0 ? null : _data.Cues[_data.Cues.Count - 1]; }
        }
        public IReadOnlyList<string> Errors { get { return ShougunuPhase1Collection.Copy(_data.Errors); } }
        public IReadOnlyList<string> DeveloperDiagnostics
        {
            get { return ShougunuPhase1Collection.Copy(_data.DeveloperDiagnostics); }
        }
        public string ActionCatalogCanonicalSignature
        {
            get { return ShougunuPhase1ActionPatternCatalog.CanonicalSignature; }
        }
        public string CanonicalSignature { get; }

        public ShougunuPhase1PresentationSafeSnapshot ToPresentationSafe(long observationTick)
        {
            long remaining = Lifecycle == ShougunuPhase1LifecycleState.CoreExposed
                ? Math.Max(0L, CoreExposeEndTick - observationTick)
                : 0L;
            return new ShougunuPhase1PresentationSafeSnapshot(
                EnemyInstanceId,
                ContentId,
                ShougunuPhase1RuntimeContract.PresentationKey,
                ResetGeneration,
                Revision,
                Lifecycle,
                MaxHp,
                CurrentHp,
                ShellLayerIndex,
                MaxSequentialShellLayers,
                ShellLayerMax,
                CurrentShell,
                Targetable,
                Vulnerable,
                Lifecycle == ShougunuPhase1LifecycleState.CoreExposed,
                remaining,
                LatestCue,
                Errors);
        }

        internal ShougunuPhase1RuntimeStateData ToMutable()
        {
            return _data.Clone();
        }
    }

    public sealed class ShougunuPhase1PresentationSafeSnapshot
    {
        public ShougunuPhase1PresentationSafeSnapshot(
            string enemyInstanceId,
            string contentId,
            string presentationKey,
            int resetGeneration,
            long revision,
            ShougunuPhase1LifecycleState lifecycle,
            int maxHp,
            int currentHp,
            int shellLayerIndex,
            int maxSequentialShellLayers,
            int shellLayerMax,
            int currentShell,
            bool targetable,
            bool vulnerable,
            bool coreExposeActive,
            long coreExposeRemainingTicks,
            ShougunuPhase1CueSnapshot latestCue,
            IEnumerable<string> errors)
        {
            EnemyInstanceId = enemyInstanceId ?? string.Empty;
            ContentId = contentId ?? string.Empty;
            PresentationKey = presentationKey ?? string.Empty;
            ResetGeneration = resetGeneration;
            Revision = revision;
            Lifecycle = lifecycle;
            MaxHp = maxHp;
            CurrentHp = currentHp;
            ShellLayerIndex = shellLayerIndex;
            MaxSequentialShellLayers = maxSequentialShellLayers;
            ShellLayerMax = shellLayerMax;
            CurrentShell = currentShell;
            Targetable = targetable;
            Vulnerable = vulnerable;
            CoreExposeActive = coreExposeActive;
            CoreExposeRemainingTicks = coreExposeRemainingTicks;
            LatestCue = latestCue;
            Errors = ShougunuPhase1Collection.Copy(errors);
            PresentationSafeCanonicalSignature = ShougunuPhase1Canonical.PresentationSafe(this);
        }

        public string EnemyInstanceId { get; }
        public string ContentId { get; }
        public string PresentationKey { get; }
        public int ResetGeneration { get; }
        public long Revision { get; }
        public ShougunuPhase1LifecycleState Lifecycle { get; }
        public int MaxHp { get; }
        public int CurrentHp { get; }
        public int ShellLayerIndex { get; }
        public int MaxSequentialShellLayers { get; }
        public int ShellLayerMax { get; }
        public int CurrentShell { get; }
        public bool Targetable { get; }
        public bool Vulnerable { get; }
        public bool CoreExposeActive { get; }
        public long CoreExposeRemainingTicks { get; }
        public ShougunuPhase1CueSnapshot LatestCue { get; }
        public IReadOnlyList<string> Errors { get; }
        public string PresentationSafeCanonicalSignature { get; }
    }

    internal sealed class ShougunuPhase1RuntimeStateData
    {
        public string SchemaId = ShougunuPhase1RuntimeContract.SchemaId;
        public int SchemaVersion = ShougunuPhase1RuntimeContract.SchemaVersion;
        public string EnemyInstanceId = string.Empty;
        public string ContentId = ShougunuPhase1RuntimeContract.ContentId;
        public string PhaseId = ShougunuPhase1RuntimeContract.PhaseId;
        public bool DevOnly = ShougunuPhase1RuntimeContract.DevOnly;
        public bool IsEnabled = ShougunuPhase1RuntimeContract.IsEnabled;
        public bool EntersFormalFlow = ShougunuPhase1RuntimeContract.EntersFormalFlow;
        public bool RuntimeBoundToBattle = ShougunuPhase1RuntimeContract.RuntimeBoundToBattle;
        public int ResetGeneration;
        public long Revision;
        public ShougunuPhase1LifecycleState Lifecycle;
        public int MaxHp = ShougunuPhase1RuntimeContract.MaxHp;
        public int CurrentHp = ShougunuPhase1RuntimeContract.InitialHp;
        public int ShellLayerIndex = ShougunuPhase1RuntimeContract.InitialShellLayerIndex;
        public int MaxSequentialShellLayers = ShougunuPhase1RuntimeContract.MaxSequentialShellLayers;
        public int ShellLayerMax = ShougunuPhase1RuntimeContract.ShellLayerMax;
        public int CurrentShell;
        public bool Targetable;
        public bool Vulnerable;
        public long CoreExposeStartTick = -1L;
        public long CoreExposeEndTick = -1L;
        public bool RecoveryAvailable = true;
        public long LastAcceptedBattleTick = -1L;
        public long LastAcceptedApplicationSequence = -1L;
        public int AcceptedDamageApplicationCount;
        public int BasicResolvedCount;
        public long NextCueSequence = 1L;
        public long NextExecutionSequence = 1L;
        public List<string> AcceptedApplicationEventIds = new List<string>();
        public List<ShougunuPhase1ThresholdOccurrenceSnapshot> ThresholdOccurrences =
            new List<ShougunuPhase1ThresholdOccurrenceSnapshot>();
        public ShougunuPhase1PendingActionSnapshot ActiveAction;
        public List<ShougunuPhase1CueSnapshot> Cues = new List<ShougunuPhase1CueSnapshot>();
        public List<string> Errors = new List<string>();
        public List<string> DeveloperDiagnostics = new List<string>();

        public ShougunuPhase1RuntimeStateData Clone()
        {
            return new ShougunuPhase1RuntimeStateData
            {
                SchemaId = SchemaId,
                SchemaVersion = SchemaVersion,
                EnemyInstanceId = EnemyInstanceId,
                ContentId = ContentId,
                PhaseId = PhaseId,
                DevOnly = DevOnly,
                IsEnabled = IsEnabled,
                EntersFormalFlow = EntersFormalFlow,
                RuntimeBoundToBattle = RuntimeBoundToBattle,
                ResetGeneration = ResetGeneration,
                Revision = Revision,
                Lifecycle = Lifecycle,
                MaxHp = MaxHp,
                CurrentHp = CurrentHp,
                ShellLayerIndex = ShellLayerIndex,
                MaxSequentialShellLayers = MaxSequentialShellLayers,
                ShellLayerMax = ShellLayerMax,
                CurrentShell = CurrentShell,
                Targetable = Targetable,
                Vulnerable = Vulnerable,
                CoreExposeStartTick = CoreExposeStartTick,
                CoreExposeEndTick = CoreExposeEndTick,
                RecoveryAvailable = RecoveryAvailable,
                LastAcceptedBattleTick = LastAcceptedBattleTick,
                LastAcceptedApplicationSequence = LastAcceptedApplicationSequence,
                AcceptedDamageApplicationCount = AcceptedDamageApplicationCount,
                BasicResolvedCount = BasicResolvedCount,
                NextCueSequence = NextCueSequence,
                NextExecutionSequence = NextExecutionSequence,
                AcceptedApplicationEventIds = new List<string>(AcceptedApplicationEventIds),
                ThresholdOccurrences = new List<ShougunuPhase1ThresholdOccurrenceSnapshot>(
                    ThresholdOccurrences),
                ActiveAction = ActiveAction,
                Cues = new List<ShougunuPhase1CueSnapshot>(Cues),
                Errors = new List<string>(Errors),
                DeveloperDiagnostics = new List<string>(DeveloperDiagnostics)
            };
        }
    }

    internal static class ShougunuPhase1Canonical
    {
        public static string Full(ShougunuPhase1RuntimeStateData value)
        {
            StringBuilder builder = new StringBuilder(16384);
            Field(builder, "schemaId", value.SchemaId);
            Field(builder, "schemaVersion", Int(value.SchemaVersion));
            Field(builder, "enemyInstanceId", value.EnemyInstanceId);
            Field(builder, "contentId", value.ContentId);
            Field(builder, "phaseId", value.PhaseId);
            Field(builder, "devOnly", Bool(value.DevOnly));
            Field(builder, "isEnabled", Bool(value.IsEnabled));
            Field(builder, "entersFormalFlow", Bool(value.EntersFormalFlow));
            Field(builder, "runtimeBoundToBattle", Bool(value.RuntimeBoundToBattle));
            Field(builder, "resetGeneration", Int(value.ResetGeneration));
            Field(builder, "revision", Long(value.Revision));
            Field(builder, "lifecycle", Int((int)value.Lifecycle));
            Field(builder, "maxHp", Int(value.MaxHp));
            Field(builder, "currentHp", Int(value.CurrentHp));
            Field(builder, "shellLayerIndex", Int(value.ShellLayerIndex));
            Field(builder, "maxSequentialShellLayers", Int(value.MaxSequentialShellLayers));
            Field(builder, "shellLayerMax", Int(value.ShellLayerMax));
            Field(builder, "currentShell", Int(value.CurrentShell));
            Field(builder, "targetable", Bool(value.Targetable));
            Field(builder, "vulnerable", Bool(value.Vulnerable));
            Field(builder, "coreExposeStartTick", Long(value.CoreExposeStartTick));
            Field(builder, "coreExposeEndTick", Long(value.CoreExposeEndTick));
            Field(builder, "recoveryAvailable", Bool(value.RecoveryAvailable));
            Field(builder, "lastAcceptedBattleTick", Long(value.LastAcceptedBattleTick));
            Field(builder, "lastAcceptedApplicationSequence", Long(value.LastAcceptedApplicationSequence));
            Field(builder, "acceptedDamageApplicationCount", Int(value.AcceptedDamageApplicationCount));
            Field(builder, "basicResolvedCount", Int(value.BasicResolvedCount));
            Field(builder, "nextCueSequence", Long(value.NextCueSequence));
            Field(builder, "nextExecutionSequence", Long(value.NextExecutionSequence));
            Field(
                builder,
                "actionCatalogCanonicalSignature",
                ShougunuPhase1ActionPatternCatalog.CanonicalSignature);
            Sequence(builder, "acceptedApplicationEventIds",
                value.AcceptedApplicationEventIds.OrderBy(item => item, StringComparer.Ordinal));
            Sequence(builder, "thresholdOccurrences", value.ThresholdOccurrences
                .OrderBy(item => item.TriggerSequence)
                .Select(Threshold));
            Field(builder, "activeAction", Action(value.ActiveAction));
            Sequence(builder, "cues", value.Cues.OrderBy(item => item.CueSequence).Select(Cue));
            Sequence(builder, "errors", value.Errors);
            Sequence(builder, "developerDiagnostics", value.DeveloperDiagnostics);
            return Hash(builder.ToString());
        }

        public static string PresentationSafe(ShougunuPhase1PresentationSafeSnapshot value)
        {
            StringBuilder builder = new StringBuilder(4096);
            Field(builder, "enemyInstanceId", value.EnemyInstanceId);
            Field(builder, "contentId", value.ContentId);
            Field(builder, "presentationKey", value.PresentationKey);
            Field(builder, "resetGeneration", Int(value.ResetGeneration));
            Field(builder, "revision", Long(value.Revision));
            Field(builder, "lifecycle", Int((int)value.Lifecycle));
            Field(builder, "maxHp", Int(value.MaxHp));
            Field(builder, "currentHp", Int(value.CurrentHp));
            Field(builder, "shellLayerIndex", Int(value.ShellLayerIndex));
            Field(builder, "maxSequentialShellLayers", Int(value.MaxSequentialShellLayers));
            Field(builder, "shellLayerMax", Int(value.ShellLayerMax));
            Field(builder, "currentShell", Int(value.CurrentShell));
            Field(builder, "targetable", Bool(value.Targetable));
            Field(builder, "vulnerable", Bool(value.Vulnerable));
            Field(builder, "coreExposeActive", Bool(value.CoreExposeActive));
            Field(builder, "coreExposeRemainingTicks", Long(value.CoreExposeRemainingTicks));
            Field(builder, "latestCue", Cue(value.LatestCue));
            Sequence(builder, "errors", value.Errors);
            return Hash(builder.ToString());
        }

        private static string Threshold(ShougunuPhase1ThresholdOccurrenceSnapshot value)
        {
            return string.Join("|", new[]
            {
                value.OccurrenceId,
                value.ThresholdId,
                value.ActionPatternId,
                Int(value.ThresholdBasisPoints),
                Long(value.TriggerSequence),
                Long(value.TriggerTick),
                Bool(value.Resolved)
            });
        }

        private static string Action(ShougunuPhase1PendingActionSnapshot value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            return string.Join("|", new[]
            {
                value.ExecutionId,
                value.ActionPatternId,
                value.ThresholdOccurrenceId,
                Int((int)value.Status),
                Long(value.StartTick),
                Long(value.ResolveTick),
                Long(value.RecoveryEndTick)
            });
        }

        private static string Cue(ShougunuPhase1CueSnapshot value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            return string.Join("|", new[]
            {
                value.CueId,
                Long(value.CueSequence),
                Long(value.Revision),
                Int(value.ResetGeneration),
                value.EnemyInstanceId,
                Int((int)value.CueKind),
                Long(value.BattleTick),
                value.SourceEventId,
                value.ExecutionId,
                Int((int)value.LongLivedState)
            });
        }

        private static void Sequence(StringBuilder builder, string name, IEnumerable<string> values)
        {
            string[] rows = (values ?? Enumerable.Empty<string>()).ToArray();
            Field(builder, name + ".count", Int(rows.Length));
            for (int index = 0; index < rows.Length; index++)
            {
                Field(builder, name + "[" + Int(index) + "]", rows[index]);
            }
        }

        private static void Field(StringBuilder builder, string name, string value)
        {
            string safeName = name ?? string.Empty;
            string safeValue = value ?? string.Empty;
            builder.Append(Int(safeName.Length)).Append(':').Append(safeName)
                .Append('=').Append(Int(safeValue.Length)).Append(':').Append(safeValue).Append(';');
        }

        private static string Bool(bool value) { return value ? "1" : "0"; }
        private static string Int(int value) { return value.ToString(CultureInfo.InvariantCulture); }
        private static string Long(long value) { return value.ToString(CultureInfo.InvariantCulture); }

        private static string Hash(string value)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(value ?? string.Empty));
                StringBuilder builder = new StringBuilder(71).Append("sha256:");
                foreach (byte valueByte in hash)
                {
                    builder.Append(valueByte.ToString("x2", CultureInfo.InvariantCulture));
                }
                return builder.ToString();
            }
        }
    }
}
