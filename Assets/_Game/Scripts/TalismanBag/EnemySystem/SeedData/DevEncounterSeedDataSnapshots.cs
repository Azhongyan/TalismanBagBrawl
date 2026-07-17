using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using TalismanBag.EnemySystem.SystemSnapshot;

namespace TalismanBag.EnemySystem.SeedData
{
    public sealed class DevEncounterSeedPlayerProjection
    {
        private readonly ReadOnlyCollection<string> playerHintCategoryKeys;

        public DevEncounterSeedPlayerProjection(string seedId, string publicEncounterLabelKey,
            string publicAtmosphereCueKey, string publicFailureObservationKey,
            IEnumerable<string> playerHintCategoryKeys)
        {
            SeedId = DevEncounterSeedDataCanonical.Text(seedId);
            PublicEncounterLabelKey = DevEncounterSeedDataCanonical.Text(publicEncounterLabelKey);
            PublicAtmosphereCueKey = DevEncounterSeedDataCanonical.Text(publicAtmosphereCueKey);
            PublicFailureObservationKey = DevEncounterSeedDataCanonical.Text(publicFailureObservationKey);
            this.playerHintCategoryKeys = DevEncounterSeedDataCanonical.Set(playerHintCategoryKeys);
        }

        public string SeedId { get; }
        public string PublicEncounterLabelKey { get; }
        public string PublicAtmosphereCueKey { get; }
        public string PublicFailureObservationKey { get; }
        public IReadOnlyList<string> PlayerHintCategoryKeys => playerHintCategoryKeys;

        internal string CanonicalRow()
        {
            return SeedId + "|" + PublicEncounterLabelKey + "|" + PublicAtmosphereCueKey + "|"
                + PublicFailureObservationKey + "|" + DevEncounterSeedDataCanonical.Join(playerHintCategoryKeys);
        }
    }

    public sealed class DevEncounterSeedInternalSnapshot
    {
        private readonly ReadOnlyCollection<string> buildPressureProfileIds;
        private readonly ReadOnlyCollection<string> counterWindowIds;

        public DevEncounterSeedInternalSnapshot(string devChapterLabel, string encounterId,
            string primaryMapRuleId, string ordinaryWaveId, string dampingWaveId, string bossWaveId,
            string bossCarrierId, string bossPhasePlanId,
            IEnumerable<string> buildPressureProfileIds, IEnumerable<string> counterWindowIds)
        {
            DevChapterLabel = DevEncounterSeedDataCanonical.Text(devChapterLabel);
            EncounterId = DevEncounterSeedDataCanonical.Text(encounterId);
            PrimaryMapRuleId = DevEncounterSeedDataCanonical.Text(primaryMapRuleId);
            OrdinaryWaveId = DevEncounterSeedDataCanonical.Text(ordinaryWaveId);
            DampingWaveId = DevEncounterSeedDataCanonical.Text(dampingWaveId);
            BossWaveId = DevEncounterSeedDataCanonical.Text(bossWaveId);
            BossCarrierId = DevEncounterSeedDataCanonical.Text(bossCarrierId);
            BossPhasePlanId = DevEncounterSeedDataCanonical.Text(bossPhasePlanId);
            this.buildPressureProfileIds = DevEncounterSeedDataCanonical.Set(buildPressureProfileIds);
            this.counterWindowIds = DevEncounterSeedDataCanonical.Set(counterWindowIds);
        }

        public string DevChapterLabel { get; }
        public string EncounterId { get; }
        public string PrimaryMapRuleId { get; }
        public string OrdinaryWaveId { get; }
        public string DampingWaveId { get; }
        public string BossWaveId { get; }
        public string BossCarrierId { get; }
        public string BossPhasePlanId { get; }
        public IReadOnlyList<string> BuildPressureProfileIds => buildPressureProfileIds;
        public IReadOnlyList<string> CounterWindowIds => counterWindowIds;
        public bool DevOnly => true;
        public bool IsEnabled => false;
        public bool EntersFormalFlow => false;

        internal string CanonicalRow()
        {
            return DevChapterLabel + "|" + EncounterId + "|" + PrimaryMapRuleId + "|"
                + OrdinaryWaveId + "|" + DampingWaveId + "|" + BossWaveId + "|"
                + BossCarrierId + "|" + BossPhasePlanId + "|"
                + DevEncounterSeedDataCanonical.Join(buildPressureProfileIds) + "|"
                + DevEncounterSeedDataCanonical.Join(counterWindowIds) + "|"
                + DevEncounterSeedDataCanonical.Isolation(DevOnly, IsEnabled, EntersFormalFlow);
        }
    }

    public sealed class DevEncounterSeedDeveloperSnapshot
    {
        private readonly ReadOnlyCollection<string> developerDiagnosticCategoryKeys;

        public DevEncounterSeedDeveloperSnapshot(string sourceScenarioId, string validationGoalKey,
            IEnumerable<string> developerDiagnosticCategoryKeys, string compositionNoteKey)
        {
            SourceScenarioId = DevEncounterSeedDataCanonical.Text(sourceScenarioId);
            ValidationGoalKey = DevEncounterSeedDataCanonical.Text(validationGoalKey);
            this.developerDiagnosticCategoryKeys = DevEncounterSeedDataCanonical.Set(developerDiagnosticCategoryKeys);
            CompositionNoteKey = DevEncounterSeedDataCanonical.Text(compositionNoteKey);
        }

        public string SourceScenarioId { get; }
        public string ValidationGoalKey { get; }
        public IReadOnlyList<string> DeveloperDiagnosticCategoryKeys => developerDiagnosticCategoryKeys;
        public string CompositionNoteKey { get; }
        public bool DeveloperOnly => true;

        internal string CanonicalRow()
        {
            return SourceScenarioId + "|" + ValidationGoalKey + "|"
                + DevEncounterSeedDataCanonical.Join(developerDiagnosticCategoryKeys) + "|" + CompositionNoteKey;
        }
    }

    public sealed class DevEncounterSeedProfileSnapshot
    {
        public DevEncounterSeedProfileSnapshot(DevEncounterSeedPlayerProjection playerSafe,
            DevEncounterSeedInternalSnapshot internalOnly,
            DevEncounterSeedDeveloperSnapshot developerOnly)
            : this(playerSafe, internalOnly, developerOnly, true, false, false)
        {
        }

        public DevEncounterSeedProfileSnapshot(DevEncounterSeedPlayerProjection playerSafe,
            DevEncounterSeedInternalSnapshot internalOnly,
            DevEncounterSeedDeveloperSnapshot developerOnly,
            bool devOnly, bool isEnabled, bool entersFormalFlow)
        {
            PlayerSafe = playerSafe;
            InternalOnly = internalOnly;
            DeveloperOnly = developerOnly;
            DevOnly = devOnly;
            IsEnabled = isEnabled;
            EntersFormalFlow = entersFormalFlow;
        }

        public string SeedId => PlayerSafe == null ? string.Empty : PlayerSafe.SeedId;
        public DevEncounterSeedPlayerProjection PlayerSafe { get; }
        public DevEncounterSeedInternalSnapshot InternalOnly { get; }
        public DevEncounterSeedDeveloperSnapshot DeveloperOnly { get; }
        public bool DevOnly { get; }
        public bool IsEnabled { get; }
        public bool EntersFormalFlow { get; }

        internal string CanonicalRow()
        {
            return (PlayerSafe == null ? string.Empty : PlayerSafe.CanonicalRow()) + "||"
                + (InternalOnly == null ? string.Empty : InternalOnly.CanonicalRow()) + "||"
                + (DeveloperOnly == null ? string.Empty : DeveloperOnly.CanonicalRow()) + "||"
                + DevEncounterSeedDataCanonical.Isolation(DevOnly, IsEnabled, EntersFormalFlow);
        }
    }

    public sealed class DevEncounterSeedLegacyMappingSnapshot
    {
        public DevEncounterSeedLegacyMappingSnapshot(string legacyStageId, string seedId,
            string mapRuleId, string primaryEnemyProblemId, string bossProblemId, string bossCarrierId)
        {
            LegacyStageId = DevEncounterSeedDataCanonical.Text(legacyStageId);
            SeedId = DevEncounterSeedDataCanonical.Text(seedId);
            MapRuleId = DevEncounterSeedDataCanonical.Text(mapRuleId);
            PrimaryEnemyProblemId = DevEncounterSeedDataCanonical.Text(primaryEnemyProblemId);
            BossProblemId = DevEncounterSeedDataCanonical.Text(bossProblemId);
            BossCarrierId = DevEncounterSeedDataCanonical.Text(bossCarrierId);
        }

        public string LegacyStageId { get; }
        public string SeedId { get; }
        public string MapRuleId { get; }
        public string PrimaryEnemyProblemId { get; }
        public string BossProblemId { get; }
        public string BossCarrierId { get; }
        public string MappingStatus => "MAPPED";
        public bool RuntimeConsumesLegacy => false;
        public bool DeveloperOnly => true;

        internal string CanonicalRow()
        {
            return LegacyStageId + "|" + SeedId + "|" + MapRuleId + "|" + PrimaryEnemyProblemId
                + "|" + BossProblemId + "|" + BossCarrierId + "|" + MappingStatus + "|0";
        }
    }

    public sealed class DevEncounterSeedPlayerSafeSnapshot
    {
        private readonly ReadOnlyCollection<DevEncounterSeedPlayerProjection> seedProfiles;

        public DevEncounterSeedPlayerSafeSnapshot(
            IEnumerable<DevEncounterSeedPlayerProjection> seedProfiles,
            EnemySystemPlayerSafeSnapshot enemySystemSnapshot)
        {
            SchemaId = DevEncounterSeedDataSchema.SchemaId;
            SchemaVersion = DevEncounterSeedDataSchema.SchemaVersion;
            this.seedProfiles = Array.AsReadOnly((seedProfiles ?? Enumerable.Empty<DevEncounterSeedPlayerProjection>())
                .OrderBy(value => value == null ? string.Empty : value.SeedId, StringComparer.Ordinal).ToArray());
            EnemySystemSnapshot = enemySystemSnapshot;
            CanonicalSignature = DevEncounterSeedDataCanonical.Hash(BuildCanonicalPayload());
        }

        public string SchemaId { get; }
        public int SchemaVersion { get; }
        public IReadOnlyList<DevEncounterSeedPlayerProjection> SeedProfiles => seedProfiles;
        public EnemySystemPlayerSafeSnapshot EnemySystemSnapshot { get; }
        public string CanonicalSignature { get; }

        internal string BuildCanonicalPayload()
        {
            StringBuilder builder = new StringBuilder(8192);
            DevEncounterSeedDataCanonical.Field(builder, "schemaId", SchemaId);
            DevEncounterSeedDataCanonical.Field(builder, "schemaVersion", SchemaVersion.ToString(CultureInfo.InvariantCulture));
            DevEncounterSeedDataCanonical.Rows(builder, "seedProfiles", seedProfiles.Select(value => value == null ? "<null>" : value.CanonicalRow()));
            DevEncounterSeedDataCanonical.Field(builder, "enemySystemPlayerSafeSignature",
                EnemySystemSnapshot == null ? string.Empty : EnemySystemSnapshot.CanonicalSignature);
            return builder.ToString();
        }
    }

    public sealed class DevEncounterSeedDataSnapshot
    {
        private readonly ReadOnlyCollection<DevEncounterSeedProfileSnapshot> seedProfiles;
        private readonly ReadOnlyCollection<DevEncounterSeedLegacyMappingSnapshot> legacyMappings;

        public DevEncounterSeedDataSnapshot(IEnumerable<DevEncounterSeedProfileSnapshot> seedProfiles,
            EnemySystemSnapshot enemySystemSnapshot,
            IEnumerable<DevEncounterSeedLegacyMappingSnapshot> legacyMappings)
        {
            SchemaId = DevEncounterSeedDataSchema.SchemaId;
            SchemaVersion = DevEncounterSeedDataSchema.SchemaVersion;
            this.seedProfiles = Array.AsReadOnly((seedProfiles ?? Enumerable.Empty<DevEncounterSeedProfileSnapshot>())
                .OrderBy(value => value == null ? string.Empty : value.SeedId, StringComparer.Ordinal).ToArray());
            EnemySystemSnapshot = enemySystemSnapshot;
            this.legacyMappings = Array.AsReadOnly((legacyMappings ?? Enumerable.Empty<DevEncounterSeedLegacyMappingSnapshot>())
                .OrderBy(value => value == null ? string.Empty : value.LegacyStageId, StringComparer.Ordinal).ToArray());
            PlayerSafe = new DevEncounterSeedPlayerSafeSnapshot(
                this.seedProfiles.Where(value => value != null).Select(value => value.PlayerSafe),
                enemySystemSnapshot == null ? null : enemySystemSnapshot.PlayerSafe);
            PlayerSafeCanonicalSignature = PlayerSafe.CanonicalSignature;
            CanonicalSignature = DevEncounterSeedDataCanonical.Hash(BuildCanonicalPayload());
        }

        public string SchemaId { get; }
        public int SchemaVersion { get; }
        public IReadOnlyList<DevEncounterSeedProfileSnapshot> SeedProfiles => seedProfiles;
        public EnemySystemSnapshot EnemySystemSnapshot { get; }
        public IReadOnlyList<DevEncounterSeedLegacyMappingSnapshot> LegacyMappings => legacyMappings;
        public DevEncounterSeedPlayerSafeSnapshot PlayerSafe { get; }
        public string CanonicalSignature { get; }
        public string PlayerSafeCanonicalSignature { get; }
        public bool DevOnly => true;
        public bool IsEnabled => false;
        public bool EntersFormalFlow => false;

        internal string BuildCanonicalPayload()
        {
            StringBuilder builder = new StringBuilder(16384);
            DevEncounterSeedDataCanonical.Field(builder, "schemaId", SchemaId);
            DevEncounterSeedDataCanonical.Field(builder, "schemaVersion", SchemaVersion.ToString(CultureInfo.InvariantCulture));
            DevEncounterSeedDataCanonical.Field(builder, "isolation",
                DevEncounterSeedDataCanonical.Isolation(DevOnly, IsEnabled, EntersFormalFlow));
            DevEncounterSeedDataCanonical.Rows(builder, "seedProfiles",
                seedProfiles.Select(value => value == null ? "<null>" : value.CanonicalRow()));
            DevEncounterSeedDataCanonical.Rows(builder, "legacyMappings",
                legacyMappings.Select(value => value == null ? "<null>" : value.CanonicalRow()));
            DevEncounterSeedDataCanonical.Field(builder, "enemySystemFullSignature",
                EnemySystemSnapshot == null ? string.Empty : EnemySystemSnapshot.CanonicalSignature);
            DevEncounterSeedDataCanonical.Field(builder, "playerSafeCanonicalSignature", PlayerSafeCanonicalSignature);
            return builder.ToString();
        }
    }
}
