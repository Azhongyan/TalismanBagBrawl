using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.EnemySystem.Domain;

namespace TalismanBag.EnemySystem.Contracts
{
    public sealed class EnemyDomainSnapshotInput
    {
        private readonly ReadOnlyCollection<EnemyArchetypeSnapshot> enemies;
        private readonly ReadOnlyCollection<BossArchetypeSnapshot> bosses;
        private readonly ReadOnlyCollection<MechanicProfileReference> mechanicProfiles;
        private readonly ReadOnlyCollection<SkillPatternReference> skillPatterns;
        private readonly ReadOnlyCollection<BossPhaseReference> bossPhases;
        private readonly ReadOnlyCollection<MapRuleReference> mapRules;
        private readonly ReadOnlyCollection<EncounterReference> encounters;
        private readonly ReadOnlyCollection<CounterWindowReference> counterWindows;

        public EnemyDomainSnapshotInput(
            IReadOnlyList<EnemyArchetypeSnapshot> enemies = null,
            IReadOnlyList<BossArchetypeSnapshot> bosses = null,
            IReadOnlyList<MechanicProfileReference> mechanicProfiles = null,
            IReadOnlyList<SkillPatternReference> skillPatterns = null,
            IReadOnlyList<BossPhaseReference> bossPhases = null,
            IReadOnlyList<MapRuleReference> mapRules = null,
            IReadOnlyList<EncounterReference> encounters = null,
            IReadOnlyList<CounterWindowReference> counterWindows = null,
            string schemaId = EnemyDomainSchema.SchemaId,
            int schemaVersion = EnemyDomainSchema.SchemaVersion)
        {
            SchemaId = EnemyDomainReadOnly.Text(schemaId);
            SchemaVersion = schemaVersion;
            this.enemies = EnemyDomainReadOnly.Freeze(enemies, value => value.Clone());
            this.bosses = EnemyDomainReadOnly.Freeze(bosses, value => value.Clone());
            this.mechanicProfiles = EnemyDomainReadOnly.Freeze(mechanicProfiles, value => value.Clone());
            this.skillPatterns = EnemyDomainReadOnly.Freeze(skillPatterns, value => value.Clone());
            this.bossPhases = EnemyDomainReadOnly.Freeze(bossPhases, value => value.Clone());
            this.mapRules = EnemyDomainReadOnly.Freeze(mapRules, value => value.Clone());
            this.encounters = EnemyDomainReadOnly.Freeze(encounters, value => value.Clone());
            this.counterWindows = EnemyDomainReadOnly.Freeze(counterWindows, value => value.Clone());
        }

        public string SchemaId { get; }
        public int SchemaVersion { get; }
        public IReadOnlyList<EnemyArchetypeSnapshot> Enemies => enemies;
        public IReadOnlyList<BossArchetypeSnapshot> Bosses => bosses;
        public IReadOnlyList<MechanicProfileReference> MechanicProfiles => mechanicProfiles;
        public IReadOnlyList<SkillPatternReference> SkillPatterns => skillPatterns;
        public IReadOnlyList<BossPhaseReference> BossPhases => bossPhases;
        public IReadOnlyList<MapRuleReference> MapRules => mapRules;
        public IReadOnlyList<EncounterReference> Encounters => encounters;
        public IReadOnlyList<CounterWindowReference> CounterWindows => counterWindows;
    }

    public interface IEnemyDomainArchetypeLookup
    {
        bool TryGetEnemyById(string stableId, out EnemyArchetypeSnapshot enemy);
        bool TryGetBossById(string stableId, out BossArchetypeSnapshot boss);
    }

    public sealed class EnemyDomainSnapshot : IEnemyDomainArchetypeLookup
    {
        private readonly ReadOnlyCollection<EnemyArchetypeSnapshot> enemies;
        private readonly ReadOnlyCollection<BossArchetypeSnapshot> bosses;
        private readonly ReadOnlyCollection<MechanicProfileReference> mechanicProfiles;
        private readonly ReadOnlyCollection<SkillPatternReference> skillPatterns;
        private readonly ReadOnlyCollection<BossPhaseReference> bossPhases;
        private readonly ReadOnlyCollection<MapRuleReference> mapRules;
        private readonly ReadOnlyCollection<EncounterReference> encounters;
        private readonly ReadOnlyCollection<CounterWindowReference> counterWindows;
        private readonly IReadOnlyDictionary<string, EnemyArchetypeSnapshot> enemyById;
        private readonly IReadOnlyDictionary<string, BossArchetypeSnapshot> bossById;

        internal EnemyDomainSnapshot(EnemyDomainSnapshotInput input)
        {
            SchemaId = input.SchemaId;
            SchemaVersion = input.SchemaVersion;
            enemies = EnemyDomainReadOnly.Freeze(input.Enemies, value => value.Clone());
            bosses = EnemyDomainReadOnly.Freeze(input.Bosses, value => value.Clone());
            mechanicProfiles = EnemyDomainReadOnly.Freeze(input.MechanicProfiles, value => value.Clone());
            skillPatterns = EnemyDomainReadOnly.Freeze(input.SkillPatterns, value => value.Clone());
            bossPhases = EnemyDomainReadOnly.Freeze(input.BossPhases, value => value.Clone());
            mapRules = EnemyDomainReadOnly.Freeze(input.MapRules, value => value.Clone());
            encounters = EnemyDomainReadOnly.Freeze(input.Encounters, value => value.Clone());
            counterWindows = EnemyDomainReadOnly.Freeze(input.CounterWindows, value => value.Clone());
            enemyById = enemies.ToDictionary(value => value.StableId, value => value, StringComparer.Ordinal);
            bossById = bosses.ToDictionary(value => value.StableId, value => value, StringComparer.Ordinal);
        }

        public string SchemaId { get; }
        public int SchemaVersion { get; }
        public IReadOnlyList<EnemyArchetypeSnapshot> Enemies => enemies;
        public IReadOnlyList<BossArchetypeSnapshot> Bosses => bosses;
        public IReadOnlyList<MechanicProfileReference> MechanicProfiles => mechanicProfiles;
        public IReadOnlyList<SkillPatternReference> SkillPatterns => skillPatterns;
        public IReadOnlyList<BossPhaseReference> BossPhases => bossPhases;
        public IReadOnlyList<MapRuleReference> MapRules => mapRules;
        public IReadOnlyList<EncounterReference> Encounters => encounters;
        public IReadOnlyList<CounterWindowReference> CounterWindows => counterWindows;

        public bool TryGetEnemyById(string stableId, out EnemyArchetypeSnapshot enemy)
        {
            if (stableId == null)
            {
                enemy = null;
                return false;
            }

            return enemyById.TryGetValue(stableId, out enemy);
        }

        public bool TryGetBossById(string stableId, out BossArchetypeSnapshot boss)
        {
            if (stableId == null)
            {
                boss = null;
                return false;
            }

            return bossById.TryGetValue(stableId, out boss);
        }

        public string BuildCanonicalSignature()
        {
            return EnemyDomainCanonical.Hash(BuildCanonicalPayload());
        }

        private string BuildCanonicalPayload()
        {
            StringBuilder builder = new StringBuilder(2048);
            EnemyDomainCanonical.Field(builder, "schemaId", SchemaId);
            EnemyDomainCanonical.Field(builder, "schemaVersion", SchemaVersion.ToString(CultureInfo.InvariantCulture));
            EnemyDomainCanonical.Collection(builder, "enemies", enemies, EnemyDomainCanonical.Enemy);
            EnemyDomainCanonical.Collection(builder, "bosses", bosses, EnemyDomainCanonical.Boss);
            EnemyDomainCanonical.Collection(builder, "mechanicProfiles", mechanicProfiles, value => EnemyDomainCanonical.Reference("mechanic", value));
            EnemyDomainCanonical.Collection(builder, "skillPatterns", skillPatterns, value => EnemyDomainCanonical.Reference("skillPattern", value));
            EnemyDomainCanonical.Collection(builder, "bossPhases", bossPhases, value => EnemyDomainCanonical.Reference("bossPhase", value));
            EnemyDomainCanonical.Collection(builder, "mapRules", mapRules, value => EnemyDomainCanonical.Reference("mapRule", value));
            EnemyDomainCanonical.Collection(builder, "encounters", encounters, value => EnemyDomainCanonical.Reference("encounter", value));
            EnemyDomainCanonical.Collection(builder, "counterWindows", counterWindows, value => EnemyDomainCanonical.Reference("counterWindow", value));
            return builder.ToString();
        }
    }

    internal static class EnemyDomainCanonical
    {
        public static string Enemy(EnemyArchetypeSnapshot value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "stableId", value.StableId);
            Field(builder, "display", value.DisplayNameOrLocalizationKey);
            Field(builder, "category", ((int)value.Category).ToString(CultureInfo.InvariantCulture));
            Field(builder, "presentationKey", value.PresentationKey);
            Field(builder, "baseBehaviorKey", value.BaseBehaviorKey);
            StringList(builder, "mechanicProfileIds", value.MechanicProfileIds);
            StringList(builder, "skillPatternIds", value.SkillPatternIds);
            Isolation(builder, value);
            return builder.ToString();
        }

        public static string Boss(BossArchetypeSnapshot value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "stableId", value.StableId);
            Field(builder, "display", value.DisplayNameOrLocalizationKey);
            Field(builder, "category", ((int)value.Category).ToString(CultureInfo.InvariantCulture));
            Field(builder, "presentationKey", value.PresentationKey);
            Field(builder, "baseBehaviorKey", value.BaseBehaviorKey);
            StringList(builder, "mechanicProfileIds", value.MechanicProfileIds);
            StringList(builder, "skillPatternIds", value.SkillPatternIds);
            StringList(builder, "phaseProfileIds", value.PhaseProfileIds);
            Isolation(builder, value);
            return builder.ToString();
        }

        public static string Reference(string kind, EnemyDomainReference value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "kind", kind);
            Field(builder, "stableId", value.StableId);
            Isolation(builder, value);
            return builder.ToString();
        }

        public static void Collection<T>(StringBuilder builder, string name, IEnumerable<T> values, Func<T, string> canonical)
        {
            string[] entries = (values ?? Array.Empty<T>())
                .Select(value => value == null ? "<null>" : canonical(value))
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            Field(builder, name + ".count", entries.Length.ToString(CultureInfo.InvariantCulture));
            for (int index = 0; index < entries.Length; index++)
            {
                Field(builder, name + "[" + index.ToString(CultureInfo.InvariantCulture) + "]", entries[index]);
            }
        }

        public static void Field(StringBuilder builder, string name, string value)
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
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(payload ?? string.Empty));
                StringBuilder builder = new StringBuilder(hash.Length * 2 + 7);
                builder.Append("sha256:");
                foreach (byte value in hash)
                {
                    builder.Append(value.ToString("x2", CultureInfo.InvariantCulture));
                }

                return builder.ToString();
            }
        }

        private static void Isolation(StringBuilder builder, IEnemyDomainIsolationMetadata value)
        {
            Field(builder, "devOnly", value.DevOnly ? "1" : "0");
            Field(builder, "isEnabled", value.IsEnabled ? "1" : "0");
            Field(builder, "entersFormalFlow", value.EntersFormalFlow ? "1" : "0");
        }

        private static void StringList(StringBuilder builder, string name, IEnumerable<string> values)
        {
            string[] ordered = (values ?? Array.Empty<string>())
                .Select(value => value ?? string.Empty)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            Field(builder, name + ".count", ordered.Length.ToString(CultureInfo.InvariantCulture));
            for (int index = 0; index < ordered.Length; index++)
            {
                Field(builder, name + "[" + index.ToString(CultureInfo.InvariantCulture) + "]", ordered[index]);
            }
        }
    }
}
