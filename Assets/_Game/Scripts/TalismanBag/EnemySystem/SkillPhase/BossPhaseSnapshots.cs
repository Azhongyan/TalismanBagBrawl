using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.EnemySystem.Contracts;
using TalismanBag.EnemySystem.Domain;

namespace TalismanBag.EnemySystem.SkillPhase
{
    public sealed class BossPhasePlayerProjection
    {
        public BossPhasePlayerProjection(
            string bossPhaseId,
            string publicPhaseLabelKey,
            string publicPhaseEntryCueKey)
        {
            BossPhaseId = EnemySkillBossPhaseReadOnly.Text(bossPhaseId);
            PublicPhaseLabelKey = EnemySkillBossPhaseReadOnly.Text(publicPhaseLabelKey);
            PublicPhaseEntryCueKey = EnemySkillBossPhaseReadOnly.Text(publicPhaseEntryCueKey);
        }

        public string BossPhaseId { get; }
        public string PublicPhaseLabelKey { get; }
        public string PublicPhaseEntryCueKey { get; }

        internal BossPhasePlayerProjection Clone()
        {
            return new BossPhasePlayerProjection(
                BossPhaseId,
                PublicPhaseLabelKey,
                PublicPhaseEntryCueKey);
        }
    }

    public sealed class BossPhaseInternalSpec
    {
        private readonly ReadOnlyCollection<string> skillSequenceIds;
        private readonly ReadOnlyCollection<string> mechanicProfileIds;

        public BossPhaseInternalSpec(
            IEnumerable<string> skillSequenceIds,
            IEnumerable<string> mechanicProfileIds)
        {
            this.skillSequenceIds = EnemySkillBossPhaseReadOnly.Strings(skillSequenceIds);
            this.mechanicProfileIds = EnemySkillBossPhaseReadOnly.Strings(mechanicProfileIds);
        }

        public IReadOnlyList<string> SkillSequenceIds => skillSequenceIds;
        public IReadOnlyList<string> MechanicProfileIds => mechanicProfileIds;

        internal BossPhaseInternalSpec Clone()
        {
            return new BossPhaseInternalSpec(skillSequenceIds, mechanicProfileIds);
        }
    }

    public sealed class BossPhaseDeveloperDiagnostics
    {
        private readonly ReadOnlyCollection<string> developerDiagnosticCategoryKeys;
        private readonly ReadOnlyCollection<string> sourceReferenceIds;

        public BossPhaseDeveloperDiagnostics(
            IEnumerable<string> developerDiagnosticCategoryKeys,
            IEnumerable<string> sourceReferenceIds)
        {
            this.developerDiagnosticCategoryKeys = EnemySkillBossPhaseReadOnly.Strings(
                developerDiagnosticCategoryKeys);
            this.sourceReferenceIds = EnemySkillBossPhaseReadOnly.Strings(sourceReferenceIds);
        }

        public IReadOnlyList<string> DeveloperDiagnosticCategoryKeys =>
            developerDiagnosticCategoryKeys;
        public IReadOnlyList<string> SourceReferenceIds => sourceReferenceIds;
        public bool DeveloperOnly => true;

        internal BossPhaseDeveloperDiagnostics Clone()
        {
            return new BossPhaseDeveloperDiagnostics(
                developerDiagnosticCategoryKeys,
                sourceReferenceIds);
        }
    }

    public sealed class BossPhaseProfileSnapshot
    {
        public BossPhaseProfileSnapshot(
            BossPhaseReference bossPhaseReference,
            BossPhasePlayerProjection playerSafe,
            BossPhaseInternalSpec internalOnly,
            BossPhaseDeveloperDiagnostics developerOnly)
        {
            BossPhaseReference = bossPhaseReference == null
                ? null
                : new BossPhaseReference(
                    bossPhaseReference.StableId,
                    bossPhaseReference.DevOnly,
                    bossPhaseReference.IsEnabled,
                    bossPhaseReference.EntersFormalFlow);
            PlayerSafe = playerSafe == null ? null : playerSafe.Clone();
            InternalOnly = internalOnly == null ? null : internalOnly.Clone();
            DeveloperOnly = developerOnly == null ? null : developerOnly.Clone();
        }

        public BossPhaseReference BossPhaseReference { get; }
        public string BossPhaseId => BossPhaseReference == null
            ? string.Empty
            : BossPhaseReference.StableId;
        public BossPhasePlayerProjection PlayerSafe { get; }
        public BossPhaseInternalSpec InternalOnly { get; }
        public BossPhaseDeveloperDiagnostics DeveloperOnly { get; }

        internal BossPhaseProfileSnapshot Clone()
        {
            return new BossPhaseProfileSnapshot(
                BossPhaseReference,
                PlayerSafe,
                InternalOnly,
                DeveloperOnly);
        }
    }

    public sealed class BossPhaseEntryConditionSnapshot
    {
        public BossPhaseEntryConditionSnapshot(
            BossPhaseEntryConditionKind kind,
            int healthThresholdBasisPoints = 0,
            string mechanicSignalId = "")
        {
            Kind = kind;
            HealthThresholdBasisPoints = healthThresholdBasisPoints;
            MechanicSignalId = EnemySkillBossPhaseReadOnly.Text(mechanicSignalId);
        }

        public BossPhaseEntryConditionKind Kind { get; }
        public int HealthThresholdBasisPoints { get; }
        public string MechanicSignalId { get; }

        internal BossPhaseEntryConditionSnapshot Clone()
        {
            return new BossPhaseEntryConditionSnapshot(
                Kind,
                HealthThresholdBasisPoints,
                MechanicSignalId);
        }
    }

    public sealed class BossPhasePlanEntrySnapshot
    {
        public BossPhasePlanEntrySnapshot(
            int phaseOrder,
            string bossPhaseId,
            BossPhaseEntryConditionSnapshot entryCondition)
        {
            PhaseOrder = phaseOrder;
            BossPhaseId = EnemySkillBossPhaseReadOnly.Text(bossPhaseId);
            EntryCondition = entryCondition == null ? null : entryCondition.Clone();
        }

        public int PhaseOrder { get; }
        public string BossPhaseId { get; }
        public BossPhaseEntryConditionSnapshot EntryCondition { get; }

        internal BossPhasePlanEntrySnapshot Clone()
        {
            return new BossPhasePlanEntrySnapshot(
                PhaseOrder,
                BossPhaseId,
                EntryCondition);
        }
    }

    public sealed class BossPhasePlanSnapshot : IEnemyDomainIsolationMetadata
    {
        private readonly ReadOnlyCollection<BossPhasePlanEntrySnapshot> entries;

        public BossPhasePlanSnapshot(
            string bossId,
            IEnumerable<BossPhasePlanEntrySnapshot> entries,
            bool devOnly = true,
            bool isEnabled = false,
            bool entersFormalFlow = false)
        {
            BossId = EnemySkillBossPhaseReadOnly.Text(bossId);
            this.entries = EnemySkillBossPhaseReadOnly.Freeze(entries, value => value.Clone())
                .OrderBy(value => value == null ? int.MaxValue : value.PhaseOrder)
                .ThenBy(value => value == null ? string.Empty : value.BossPhaseId, StringComparer.Ordinal)
                .ToReadOnly();
            DevOnly = devOnly;
            IsEnabled = isEnabled;
            EntersFormalFlow = entersFormalFlow;
        }

        public string BossId { get; }
        public IReadOnlyList<BossPhasePlanEntrySnapshot> Entries => entries;
        public bool DevOnly { get; }
        public bool IsEnabled { get; }
        public bool EntersFormalFlow { get; }

        internal BossPhasePlanSnapshot Clone()
        {
            return new BossPhasePlanSnapshot(
                BossId,
                entries,
                DevOnly,
                IsEnabled,
                EntersFormalFlow);
        }
    }

    public sealed class EnemySkillBossPhaseCatalogInput
    {
        private readonly ReadOnlyCollection<EnemySkillPatternSnapshot> skillPatterns;
        private readonly ReadOnlyCollection<SkillSequenceSnapshot> skillSequences;
        private readonly ReadOnlyCollection<CarrierSkillBindingSnapshot> carrierSkillBindings;
        private readonly ReadOnlyCollection<BossPhaseProfileSnapshot> bossPhaseProfiles;
        private readonly ReadOnlyCollection<BossPhasePlanSnapshot> bossPhasePlans;

        public EnemySkillBossPhaseCatalogInput(
            IEnumerable<EnemySkillPatternSnapshot> skillPatterns,
            IEnumerable<SkillSequenceSnapshot> skillSequences,
            IEnumerable<CarrierSkillBindingSnapshot> carrierSkillBindings,
            IEnumerable<BossPhaseProfileSnapshot> bossPhaseProfiles,
            IEnumerable<BossPhasePlanSnapshot> bossPhasePlans,
            string schemaId = EnemySkillBossPhaseSchema.SchemaId,
            int schemaVersion = EnemySkillBossPhaseSchema.SchemaVersion)
        {
            SchemaId = EnemySkillBossPhaseReadOnly.Text(schemaId);
            SchemaVersion = schemaVersion;
            this.skillPatterns = EnemySkillBossPhaseReadOnly.Freeze(
                skillPatterns,
                value => value.Clone());
            this.skillSequences = EnemySkillBossPhaseReadOnly.Freeze(
                skillSequences,
                value => value.Clone());
            this.carrierSkillBindings = EnemySkillBossPhaseReadOnly.Freeze(
                carrierSkillBindings,
                value => value.Clone());
            this.bossPhaseProfiles = EnemySkillBossPhaseReadOnly.Freeze(
                bossPhaseProfiles,
                value => value.Clone());
            this.bossPhasePlans = EnemySkillBossPhaseReadOnly.Freeze(
                bossPhasePlans,
                value => value.Clone());
        }

        public string SchemaId { get; }
        public int SchemaVersion { get; }
        public IReadOnlyList<EnemySkillPatternSnapshot> SkillPatterns => skillPatterns;
        public IReadOnlyList<SkillSequenceSnapshot> SkillSequences => skillSequences;
        public IReadOnlyList<CarrierSkillBindingSnapshot> CarrierSkillBindings =>
            carrierSkillBindings;
        public IReadOnlyList<BossPhaseProfileSnapshot> BossPhaseProfiles => bossPhaseProfiles;
        public IReadOnlyList<BossPhasePlanSnapshot> BossPhasePlans => bossPhasePlans;
    }

    public sealed class EnemySkillBossPhaseCatalogSnapshot : IEnemySkillBossPhaseLookup
    {
        private readonly ReadOnlyCollection<EnemySkillPatternSnapshot> skillPatterns;
        private readonly ReadOnlyCollection<SkillSequenceSnapshot> skillSequences;
        private readonly ReadOnlyCollection<CarrierSkillBindingSnapshot> carrierSkillBindings;
        private readonly ReadOnlyCollection<BossPhaseProfileSnapshot> bossPhaseProfiles;
        private readonly ReadOnlyCollection<BossPhasePlanSnapshot> bossPhasePlans;
        private readonly IReadOnlyDictionary<string, EnemySkillPatternSnapshot> patternById;
        private readonly IReadOnlyDictionary<string, SkillSequenceSnapshot> sequenceById;
        private readonly IReadOnlyDictionary<string, CarrierSkillBindingSnapshot> bindingByCarrier;
        private readonly IReadOnlyDictionary<string, BossPhaseProfileSnapshot> phaseById;
        private readonly IReadOnlyDictionary<string, BossPhasePlanSnapshot> planByBossId;
        private readonly string playerSafePayload;

        internal EnemySkillBossPhaseCatalogSnapshot(EnemySkillBossPhaseCatalogInput input)
        {
            SchemaId = input.SchemaId;
            SchemaVersion = input.SchemaVersion;
            skillPatterns = EnemySkillBossPhaseReadOnly.Freeze(
                    input.SkillPatterns,
                    value => value.Clone())
                .OrderBy(value => value.SkillPatternId, StringComparer.Ordinal)
                .ToReadOnly();
            skillSequences = EnemySkillBossPhaseReadOnly.Freeze(
                    input.SkillSequences,
                    value => value.Clone())
                .OrderBy(value => value.SkillSequenceId, StringComparer.Ordinal)
                .ToReadOnly();
            carrierSkillBindings = EnemySkillBossPhaseReadOnly.Freeze(
                    input.CarrierSkillBindings,
                    value => value.Clone())
                .OrderBy(value => value.CarrierKind)
                .ThenBy(value => value.CarrierId, StringComparer.Ordinal)
                .ToReadOnly();
            bossPhaseProfiles = EnemySkillBossPhaseReadOnly.Freeze(
                    input.BossPhaseProfiles,
                    value => value.Clone())
                .OrderBy(value => value.BossPhaseId, StringComparer.Ordinal)
                .ToReadOnly();
            bossPhasePlans = EnemySkillBossPhaseReadOnly.Freeze(
                    input.BossPhasePlans,
                    value => value.Clone())
                .OrderBy(value => value.BossId, StringComparer.Ordinal)
                .ToReadOnly();

            patternById = Dictionary(skillPatterns, value => value.SkillPatternId);
            sequenceById = Dictionary(skillSequences, value => value.SkillSequenceId);
            bindingByCarrier = Dictionary(
                carrierSkillBindings,
                value => CarrierIdentity(value.CarrierKind, value.CarrierId));
            phaseById = Dictionary(bossPhaseProfiles, value => value.BossPhaseId);
            planByBossId = Dictionary(bossPhasePlans, value => value.BossId);

            string fullPayload = EnemySkillBossPhaseCanonical.Catalog(
                SchemaId,
                SchemaVersion,
                skillPatterns,
                skillSequences,
                carrierSkillBindings,
                bossPhaseProfiles,
                bossPhasePlans);
            playerSafePayload = EnemySkillBossPhaseCanonical.PlayerSafeCatalog(
                SchemaId,
                SchemaVersion,
                skillPatterns,
                bossPhaseProfiles);
            CanonicalSignature = EnemySkillBossPhaseCanonical.Hash(fullPayload);
            PlayerSafeCanonicalSignature = EnemySkillBossPhaseCanonical.Hash(playerSafePayload);
        }

        public string SchemaId { get; }
        public int SchemaVersion { get; }
        public IReadOnlyList<EnemySkillPatternSnapshot> SkillPatterns => skillPatterns;
        public IReadOnlyList<SkillSequenceSnapshot> SkillSequences => skillSequences;
        public IReadOnlyList<CarrierSkillBindingSnapshot> CarrierSkillBindings =>
            carrierSkillBindings;
        public IReadOnlyList<BossPhaseProfileSnapshot> BossPhaseProfiles => bossPhaseProfiles;
        public IReadOnlyList<BossPhasePlanSnapshot> BossPhasePlans => bossPhasePlans;
        public string CanonicalSignature { get; }
        public string PlayerSafeCanonicalSignature { get; }

        public string BuildPlayerSafeCanonicalPayload()
        {
            return playerSafePayload;
        }

        public bool TryGetSkillPatternById(
            string skillPatternId,
            out EnemySkillPatternSnapshot pattern)
        {
            if (skillPatternId != null)
            {
                return patternById.TryGetValue(skillPatternId, out pattern);
            }

            pattern = null;
            return false;
        }

        public bool TryGetSkillSequenceById(
            string skillSequenceId,
            out SkillSequenceSnapshot sequence)
        {
            if (skillSequenceId != null)
            {
                return sequenceById.TryGetValue(skillSequenceId, out sequence);
            }

            sequence = null;
            return false;
        }

        public bool TryGetCarrierSkillBinding(
            EnemySkillCarrierKind kind,
            string carrierId,
            out CarrierSkillBindingSnapshot binding)
        {
            if (carrierId != null)
            {
                return bindingByCarrier.TryGetValue(CarrierIdentity(kind, carrierId), out binding);
            }

            binding = null;
            return false;
        }

        public bool TryGetBossPhaseById(
            string bossPhaseId,
            out BossPhaseProfileSnapshot phase)
        {
            if (bossPhaseId != null)
            {
                return phaseById.TryGetValue(bossPhaseId, out phase);
            }

            phase = null;
            return false;
        }

        public bool TryGetBossPhasePlanByBossId(
            string bossId,
            out BossPhasePlanSnapshot plan)
        {
            if (bossId != null)
            {
                return planByBossId.TryGetValue(bossId, out plan);
            }

            plan = null;
            return false;
        }

        internal static string CarrierIdentity(EnemySkillCarrierKind kind, string carrierId)
        {
            return ((int)kind).ToString(CultureInfo.InvariantCulture)
                + "\u001f"
                + (carrierId ?? string.Empty);
        }

        private static IReadOnlyDictionary<string, T> Dictionary<T>(
            IEnumerable<T> values,
            Func<T, string> key)
        {
            return new ReadOnlyDictionary<string, T>(
                values.ToDictionary(key, value => value, StringComparer.Ordinal));
        }
    }

    internal static class EnemySkillBossPhaseCanonical
    {
        public static string Catalog(
            string schemaId,
            int schemaVersion,
            IEnumerable<EnemySkillPatternSnapshot> patterns,
            IEnumerable<SkillSequenceSnapshot> sequences,
            IEnumerable<CarrierSkillBindingSnapshot> bindings,
            IEnumerable<BossPhaseProfileSnapshot> phases,
            IEnumerable<BossPhasePlanSnapshot> plans)
        {
            StringBuilder builder = new StringBuilder(16384);
            Field(builder, "schemaId", schemaId);
            Field(builder, "schemaVersion", schemaVersion.ToString(CultureInfo.InvariantCulture));
            Rows(builder, "skillPatterns", patterns, Pattern);
            Rows(builder, "skillSequences", sequences, Sequence);
            Rows(builder, "carrierSkillBindings", bindings, Binding);
            Rows(builder, "bossPhaseProfiles", phases, Phase);
            Rows(builder, "bossPhasePlans", plans, Plan);
            return builder.ToString();
        }

        public static string PlayerSafeCatalog(
            string schemaId,
            int schemaVersion,
            IEnumerable<EnemySkillPatternSnapshot> patterns,
            IEnumerable<BossPhaseProfileSnapshot> phases)
        {
            StringBuilder builder = new StringBuilder(4096);
            Field(builder, "schemaId", schemaId);
            Field(builder, "schemaVersion", schemaVersion.ToString(CultureInfo.InvariantCulture));
            Rows(builder, "skillPatterns", patterns, value => PatternPlayer(value.PlayerSafe));
            Rows(builder, "bossPhaseProfiles", phases, value => PhasePlayer(value.PlayerSafe));
            return builder.ToString();
        }

        private static string Pattern(EnemySkillPatternSnapshot value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "skillPatternId", value.SkillPatternId);
            Isolation(builder, value.SkillPatternReference);
            Field(builder, "playerSafe", PatternPlayer(value.PlayerSafe));
            Field(builder, "castKind", ((int)value.InternalOnly.SkillCastKind).ToString(CultureInfo.InvariantCulture));
            Field(builder, "castDurationMilliseconds", value.InternalOnly.CastDurationMilliseconds.ToString(CultureInfo.InvariantCulture));
            Field(builder, "recoveryDurationMilliseconds", value.InternalOnly.RecoveryDurationMilliseconds.ToString(CultureInfo.InvariantCulture));
            Strings(builder, "mechanicProfileIds", value.InternalOnly.MechanicProfileIds);
            Strings(builder, "developerDiagnosticCategoryKeys", value.DeveloperOnly.DeveloperDiagnosticCategoryKeys);
            Strings(builder, "sourceReferenceIds", value.DeveloperOnly.SourceReferenceIds);
            return builder.ToString();
        }

        private static string PatternPlayer(SkillPatternPlayerProjection value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "skillPatternId", value.SkillPatternId);
            Field(builder, "publicSkillNameKey", value.PublicSkillNameKey);
            Field(builder, "intentId", value.IntentId);
            Field(builder, "publicIntentTextKey", value.PublicIntentTextKey);
            Field(builder, "publicTargetCueKey", value.PublicTargetCueKey);
            Field(builder, "publicCastCueKey", value.PublicCastCueKey);
            Strings(builder, "playerHintCategoryKeys", value.PlayerHintCategoryKeys);
            return builder.ToString();
        }

        private static string Sequence(SkillSequenceSnapshot value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "skillSequenceId", value.SkillSequenceId);
            Isolation(builder, value);
            OrderedRows(builder, "steps", value.Steps, step => step.StepOrder, Step);
            return builder.ToString();
        }

        private static string Step(SkillSequenceStepSnapshot value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "stepOrder", value.StepOrder.ToString(CultureInfo.InvariantCulture));
            Field(builder, "skillPatternId", value.SkillPatternId);
            Field(builder, "delayAfterPreviousMilliseconds", value.DelayAfterPreviousMilliseconds.ToString(CultureInfo.InvariantCulture));
            Field(builder, "repeatCount", value.RepeatCount.ToString(CultureInfo.InvariantCulture));
            return builder.ToString();
        }

        private static string Binding(CarrierSkillBindingSnapshot value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "carrierKind", ((int)value.CarrierKind).ToString(CultureInfo.InvariantCulture));
            Field(builder, "carrierId", value.CarrierId);
            Strings(builder, "skillSequenceIds", value.SkillSequenceIds);
            Isolation(builder, value);
            return builder.ToString();
        }

        private static string Phase(BossPhaseProfileSnapshot value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "bossPhaseId", value.BossPhaseId);
            Isolation(builder, value.BossPhaseReference);
            Field(builder, "playerSafe", PhasePlayer(value.PlayerSafe));
            Strings(builder, "skillSequenceIds", value.InternalOnly.SkillSequenceIds);
            Strings(builder, "mechanicProfileIds", value.InternalOnly.MechanicProfileIds);
            Strings(builder, "developerDiagnosticCategoryKeys", value.DeveloperOnly.DeveloperDiagnosticCategoryKeys);
            Strings(builder, "sourceReferenceIds", value.DeveloperOnly.SourceReferenceIds);
            return builder.ToString();
        }

        private static string PhasePlayer(BossPhasePlayerProjection value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "bossPhaseId", value.BossPhaseId);
            Field(builder, "publicPhaseLabelKey", value.PublicPhaseLabelKey);
            Field(builder, "publicPhaseEntryCueKey", value.PublicPhaseEntryCueKey);
            return builder.ToString();
        }

        private static string Plan(BossPhasePlanSnapshot value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "bossId", value.BossId);
            Isolation(builder, value);
            OrderedRows(builder, "entries", value.Entries, entry => entry.PhaseOrder, PlanEntry);
            return builder.ToString();
        }

        private static string PlanEntry(BossPhasePlanEntrySnapshot value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "phaseOrder", value.PhaseOrder.ToString(CultureInfo.InvariantCulture));
            Field(builder, "bossPhaseId", value.BossPhaseId);
            Field(builder, "entryConditionKind", ((int)value.EntryCondition.Kind).ToString(CultureInfo.InvariantCulture));
            Field(builder, "healthThresholdBasisPoints", value.EntryCondition.HealthThresholdBasisPoints.ToString(CultureInfo.InvariantCulture));
            Field(builder, "mechanicSignalId", value.EntryCondition.MechanicSignalId);
            return builder.ToString();
        }

        private static void Rows<T>(
            StringBuilder builder,
            string name,
            IEnumerable<T> values,
            Func<T, string> canonical)
        {
            string[] rows = (values ?? Array.Empty<T>())
                .Select(canonical)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            WriteRows(builder, name, rows);
        }

        private static void OrderedRows<T>(
            StringBuilder builder,
            string name,
            IEnumerable<T> values,
            Func<T, int> order,
            Func<T, string> canonical)
        {
            string[] rows = (values ?? Array.Empty<T>())
                .OrderBy(order)
                .ThenBy(canonical, StringComparer.Ordinal)
                .Select(canonical)
                .ToArray();
            WriteRows(builder, name, rows);
        }

        private static void Strings(
            StringBuilder builder,
            string name,
            IEnumerable<string> values)
        {
            string[] rows = (values ?? Array.Empty<string>())
                .Select(value => value ?? string.Empty)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            WriteRows(builder, name, rows);
        }

        private static void WriteRows(
            StringBuilder builder,
            string name,
            IReadOnlyList<string> rows)
        {
            Field(builder, name + ".count", rows.Count.ToString(CultureInfo.InvariantCulture));
            for (int index = 0; index < rows.Count; index++)
            {
                Field(builder, name + "[" + index.ToString(CultureInfo.InvariantCulture) + "]", rows[index]);
            }
        }

        private static void Isolation(StringBuilder builder, IEnemyDomainIsolationMetadata value)
        {
            Field(builder, "devOnly", value.DevOnly ? "1" : "0");
            Field(builder, "isEnabled", value.IsEnabled ? "1" : "0");
            Field(builder, "entersFormalFlow", value.EntersFormalFlow ? "1" : "0");
        }

        private static void Field(StringBuilder builder, string name, string value)
        {
            string safeName = name ?? string.Empty;
            string safeValue = value ?? string.Empty;
            builder.Append(safeName.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':')
                .Append(safeName)
                .Append('=')
                .Append(safeValue.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':')
                .Append(safeValue)
                .Append(';');
        }

        public static string Hash(string payload)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(payload ?? string.Empty));
                StringBuilder builder = new StringBuilder(71).Append("sha256:");
                foreach (byte value in bytes)
                {
                    builder.Append(value.ToString("x2", CultureInfo.InvariantCulture));
                }

                return builder.ToString();
            }
        }
    }
}
