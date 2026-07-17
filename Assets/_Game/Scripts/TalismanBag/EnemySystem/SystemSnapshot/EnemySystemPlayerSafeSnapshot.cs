using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using TalismanBag.EnemySystem.Normalization;
using TalismanBag.EnemySystem.PressureWindow;
using TalismanBag.EnemySystem.SkillPhase;

namespace TalismanBag.EnemySystem.SystemSnapshot
{
    public sealed class EnemySystemPlayerSafeSnapshot
    {
        private readonly ReadOnlyCollection<EnemyValidationPlayerProjection> enemies;
        private readonly ReadOnlyCollection<EnemyValidationPlayerProjection> bosses;
        private readonly ReadOnlyCollection<EnemyValidationPlayerProjection> mechanicProfiles;
        private readonly ReadOnlyCollection<EnemyValidationPlayerProjection> mapRules;
        private readonly ReadOnlyCollection<SkillPatternPlayerProjection> skillPatterns;
        private readonly ReadOnlyCollection<BossPhasePlayerProjection> bossPhases;
        private readonly ReadOnlyCollection<BuildPressurePlayerProjection> buildPressureProfiles;
        private readonly ReadOnlyCollection<CounterWindowPlayerProjection> counterWindows;

        internal EnemySystemPlayerSafeSnapshot(EnemySystemSnapshotInput input)
        {
            SchemaId = EnemySystemSnapshotSchema.SchemaId;
            SchemaVersion = EnemySystemSnapshotSchema.SchemaVersion;
            enemies = FreezeValidation(input.EnemyValidationContentSnapshot.Enemies.Select(value => value.PlayerSafe));
            bosses = FreezeValidation(input.EnemyValidationContentSnapshot.Bosses.Select(value => value.PlayerSafe));
            mechanicProfiles = FreezeValidation(input.EnemyValidationContentSnapshot.MechanicProfiles.Select(value => value.PlayerSafe));
            mapRules = FreezeValidation(input.EnemyValidationContentSnapshot.MapRules.Select(value => value.PlayerSafe));
            skillPatterns = Array.AsReadOnly(input.EnemySkillBossPhaseCatalogSnapshot.SkillPatterns
                .Select(value => Copy(value.PlayerSafe))
                .OrderBy(value => value.SkillPatternId, StringComparer.Ordinal).ToArray());
            bossPhases = Array.AsReadOnly(input.EnemySkillBossPhaseCatalogSnapshot.BossPhaseProfiles
                .Select(value => Copy(value.PlayerSafe))
                .OrderBy(value => value.BossPhaseId, StringComparer.Ordinal).ToArray());
            buildPressureProfiles = Array.AsReadOnly(input.CounterWindowAndPressureCatalogSnapshot.BuildPressureProfiles
                .Select(value => Copy(value.PlayerSafe))
                .OrderBy(value => value.BuildPressureProfileId, StringComparer.Ordinal).ToArray());
            counterWindows = Array.AsReadOnly(input.CounterWindowAndPressureCatalogSnapshot.CounterWindowProfiles
                .Select(value => Copy(value.PlayerSafe))
                .OrderBy(value => value.CounterWindowId, StringComparer.Ordinal).ToArray());
            CanonicalSignature = EnemySystemSnapshotCanonical.Hash(BuildCanonicalPayload());
        }

        public string SchemaId { get; }
        public int SchemaVersion { get; }
        public IReadOnlyList<EnemyValidationPlayerProjection> Enemies => enemies;
        public IReadOnlyList<EnemyValidationPlayerProjection> Bosses => bosses;
        public IReadOnlyList<EnemyValidationPlayerProjection> MechanicProfiles => mechanicProfiles;
        public IReadOnlyList<EnemyValidationPlayerProjection> MapRules => mapRules;
        public IReadOnlyList<SkillPatternPlayerProjection> SkillPatterns => skillPatterns;
        public IReadOnlyList<BossPhasePlayerProjection> BossPhases => bossPhases;
        public IReadOnlyList<BuildPressurePlayerProjection> BuildPressureProfiles => buildPressureProfiles;
        public IReadOnlyList<CounterWindowPlayerProjection> CounterWindows => counterWindows;
        public string CanonicalSignature { get; }

        internal string BuildCanonicalPayload()
        {
            StringBuilder builder = new StringBuilder(16384);
            EnemySystemSnapshotCanonical.Field(builder, "schemaId", SchemaId);
            EnemySystemSnapshotCanonical.Field(builder, "schemaVersion", SchemaVersion.ToString(CultureInfo.InvariantCulture));
            EnemySystemSnapshotCanonical.Rows(builder, "enemies", enemies.Select(ValidationRow));
            EnemySystemSnapshotCanonical.Rows(builder, "bosses", bosses.Select(ValidationRow));
            EnemySystemSnapshotCanonical.Rows(builder, "mechanicProfiles", mechanicProfiles.Select(ValidationRow));
            EnemySystemSnapshotCanonical.Rows(builder, "mapRules", mapRules.Select(ValidationRow));
            EnemySystemSnapshotCanonical.Rows(builder, "skillPatterns", skillPatterns.Select(SkillRow));
            EnemySystemSnapshotCanonical.Rows(builder, "bossPhases", bossPhases.Select(PhaseRow));
            EnemySystemSnapshotCanonical.Rows(builder, "buildPressureProfiles", buildPressureProfiles.Select(PressureRow));
            EnemySystemSnapshotCanonical.Rows(builder, "counterWindows", counterWindows.Select(WindowRow));
            return builder.ToString();
        }

        private static ReadOnlyCollection<EnemyValidationPlayerProjection> FreezeValidation(
            IEnumerable<EnemyValidationPlayerProjection> values)
        {
            return Array.AsReadOnly(values.Select(Copy)
                .OrderBy(value => value.Id, StringComparer.Ordinal).ToArray());
        }

        private static EnemyValidationPlayerProjection Copy(EnemyValidationPlayerProjection value)
        {
            return new EnemyValidationPlayerProjection(value.Id, value.DisplayName, value.MechanicKeys,
                value.PressureKeys, value.PlayerHintCategoryKeys, value.CounterWindowTypeKeys);
        }

        private static SkillPatternPlayerProjection Copy(SkillPatternPlayerProjection value)
        {
            return new SkillPatternPlayerProjection(value.SkillPatternId, value.PublicSkillNameKey,
                value.IntentId, value.PublicIntentTextKey, value.PublicTargetCueKey, value.PublicCastCueKey,
                value.PlayerHintCategoryKeys);
        }

        private static BossPhasePlayerProjection Copy(BossPhasePlayerProjection value)
        {
            return new BossPhasePlayerProjection(value.BossPhaseId, value.PublicPhaseLabelKey,
                value.PublicPhaseEntryCueKey);
        }

        private static BuildPressurePlayerProjection Copy(BuildPressurePlayerProjection value)
        {
            return new BuildPressurePlayerProjection(value.BuildPressureProfileId, value.PublicPressureLabelKey,
                value.PublicPressureHintKey, value.PlayerHintCategoryKeys);
        }

        private static CounterWindowPlayerProjection Copy(CounterWindowPlayerProjection value)
        {
            return new CounterWindowPlayerProjection(value.CounterWindowId, value.PublicWindowLabelKey,
                value.PublicWindowOpenCueKey, value.PublicWindowCloseCueKey);
        }

        private static string ValidationRow(EnemyValidationPlayerProjection value)
        {
            StringBuilder builder = new StringBuilder();
            EnemySystemSnapshotCanonical.Field(builder, "id", value.Id);
            EnemySystemSnapshotCanonical.Field(builder, "displayName", value.DisplayName);
            EnemySystemSnapshotCanonical.Rows(builder, "mechanicKeys", value.MechanicKeys);
            EnemySystemSnapshotCanonical.Rows(builder, "pressureKeys", value.PressureKeys);
            EnemySystemSnapshotCanonical.Rows(builder, "playerHintCategoryKeys", value.PlayerHintCategoryKeys);
            EnemySystemSnapshotCanonical.Rows(builder, "counterWindowTypeKeys", value.CounterWindowTypeKeys);
            return builder.ToString();
        }

        private static string SkillRow(SkillPatternPlayerProjection value)
        {
            StringBuilder builder = new StringBuilder();
            EnemySystemSnapshotCanonical.Field(builder, "skillPatternId", value.SkillPatternId);
            EnemySystemSnapshotCanonical.Field(builder, "publicSkillNameKey", value.PublicSkillNameKey);
            EnemySystemSnapshotCanonical.Field(builder, "intentId", value.IntentId);
            EnemySystemSnapshotCanonical.Field(builder, "publicIntentTextKey", value.PublicIntentTextKey);
            EnemySystemSnapshotCanonical.Field(builder, "publicTargetCueKey", value.PublicTargetCueKey);
            EnemySystemSnapshotCanonical.Field(builder, "publicCastCueKey", value.PublicCastCueKey);
            EnemySystemSnapshotCanonical.Rows(builder, "playerHintCategoryKeys", value.PlayerHintCategoryKeys);
            return builder.ToString();
        }

        private static string PhaseRow(BossPhasePlayerProjection value)
        {
            StringBuilder builder = new StringBuilder();
            EnemySystemSnapshotCanonical.Field(builder, "bossPhaseId", value.BossPhaseId);
            EnemySystemSnapshotCanonical.Field(builder, "publicPhaseLabelKey", value.PublicPhaseLabelKey);
            EnemySystemSnapshotCanonical.Field(builder, "publicPhaseEntryCueKey", value.PublicPhaseEntryCueKey);
            return builder.ToString();
        }

        private static string PressureRow(BuildPressurePlayerProjection value)
        {
            StringBuilder builder = new StringBuilder();
            EnemySystemSnapshotCanonical.Field(builder, "buildPressureProfileId", value.BuildPressureProfileId);
            EnemySystemSnapshotCanonical.Field(builder, "publicPressureLabelKey", value.PublicPressureLabelKey);
            EnemySystemSnapshotCanonical.Field(builder, "publicPressureHintKey", value.PublicPressureHintKey);
            EnemySystemSnapshotCanonical.Rows(builder, "playerHintCategoryKeys", value.PlayerHintCategoryKeys);
            return builder.ToString();
        }

        private static string WindowRow(CounterWindowPlayerProjection value)
        {
            StringBuilder builder = new StringBuilder();
            EnemySystemSnapshotCanonical.Field(builder, "counterWindowId", value.CounterWindowId);
            EnemySystemSnapshotCanonical.Field(builder, "publicWindowLabelKey", value.PublicWindowLabelKey);
            EnemySystemSnapshotCanonical.Field(builder, "publicWindowOpenCueKey", value.PublicWindowOpenCueKey);
            EnemySystemSnapshotCanonical.Field(builder, "publicWindowCloseCueKey", value.PublicWindowCloseCueKey);
            return builder.ToString();
        }
    }
}
