using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using TalismanBag.EnemySystem.Domain;

namespace TalismanBag.EnemySystem.Contracts
{
    public sealed class EnemyDomainValidationIssue
    {
        public EnemyDomainValidationIssue(string code, string path, string message)
        {
            Code = code ?? string.Empty;
            Path = path ?? string.Empty;
            Message = message ?? string.Empty;
        }

        public string Code { get; }
        public string Path { get; }
        public string Message { get; }

        public override string ToString()
        {
            return Code + " @ " + Path + ": " + Message;
        }
    }

    public interface IEnemyDomainContractValidator
    {
        IReadOnlyList<EnemyDomainValidationIssue> Validate(EnemyDomainSnapshotInput input);
    }

    public interface IEnemyDomainSnapshotProvider
    {
        EnemyDomainSnapshot CreateSnapshot(EnemyDomainSnapshotInput input);
    }

    public sealed class EnemyDomainContractValidationException : ArgumentException
    {
        private readonly ReadOnlyCollection<EnemyDomainValidationIssue> issues;

        public EnemyDomainContractValidationException(IReadOnlyList<EnemyDomainValidationIssue> issues)
            : base(BuildMessage(issues))
        {
            this.issues = Array.AsReadOnly((issues ?? Array.Empty<EnemyDomainValidationIssue>()).ToArray());
        }

        public IReadOnlyList<EnemyDomainValidationIssue> Issues => issues;

        private static string BuildMessage(IReadOnlyList<EnemyDomainValidationIssue> issues)
        {
            StringBuilder builder = new StringBuilder("Enemy domain contract validation failed.");
            foreach (EnemyDomainValidationIssue issue in issues ?? Array.Empty<EnemyDomainValidationIssue>())
            {
                builder.Append(' ').Append(issue);
            }

            return builder.ToString();
        }
    }

    public sealed class DefaultEnemyDomainSnapshotProvider : IEnemyDomainSnapshotProvider
    {
        public static readonly DefaultEnemyDomainSnapshotProvider Instance = new DefaultEnemyDomainSnapshotProvider();

        private readonly IEnemyDomainContractValidator validator;

        public DefaultEnemyDomainSnapshotProvider(IEnemyDomainContractValidator validator = null)
        {
            this.validator = validator ?? DefaultEnemyDomainContractValidator.Instance;
        }

        public EnemyDomainSnapshot CreateSnapshot(EnemyDomainSnapshotInput input)
        {
            IReadOnlyList<EnemyDomainValidationIssue> issues = validator.Validate(input);
            if (issues.Count > 0)
            {
                throw new EnemyDomainContractValidationException(issues);
            }

            return new EnemyDomainSnapshot(input);
        }
    }

    public sealed class DefaultEnemyDomainContractValidator : IEnemyDomainContractValidator
    {
        public static readonly DefaultEnemyDomainContractValidator Instance = new DefaultEnemyDomainContractValidator();

        public IReadOnlyList<EnemyDomainValidationIssue> Validate(EnemyDomainSnapshotInput input)
        {
            List<EnemyDomainValidationIssue> issues = new List<EnemyDomainValidationIssue>();
            if (input == null)
            {
                issues.Add(new EnemyDomainValidationIssue("INPUT_NULL", "$", "Snapshot input is required."));
                return Array.AsReadOnly(issues.ToArray());
            }

            if (!string.Equals(input.SchemaId, EnemyDomainSchema.SchemaId, StringComparison.Ordinal))
            {
                issues.Add(new EnemyDomainValidationIssue("SCHEMA_ID_MISMATCH", "schemaId", "Expected exact stable schema ID " + EnemyDomainSchema.SchemaId + "."));
            }

            if (input.SchemaVersion != EnemyDomainSchema.SchemaVersion)
            {
                issues.Add(new EnemyDomainValidationIssue("SCHEMA_VERSION_MISMATCH", "schemaVersion", "Expected schema version 1."));
            }

            ValidateCollection(input.Enemies, "enemies", value => value.StableId, issues);
            ValidateCollection(input.Bosses, "bosses", value => value.StableId, issues);
            ValidateCollection(input.MechanicProfiles, "mechanicProfiles", value => value.StableId, issues);
            ValidateCollection(input.SkillPatterns, "skillPatterns", value => value.StableId, issues);
            ValidateCollection(input.BossPhases, "bossPhases", value => value.StableId, issues);
            ValidateCollection(input.MapRules, "mapRules", value => value.StableId, issues);
            ValidateCollection(input.Encounters, "encounters", value => value.StableId, issues);
            ValidateCollection(input.CounterWindows, "counterWindows", value => value.StableId, issues);

            ValidateArchetypeIdSeparation(input, issues);

            HashSet<string> mechanicIds = StableIdSet(input.MechanicProfiles, value => value.StableId);
            HashSet<string> skillPatternIds = StableIdSet(input.SkillPatterns, value => value.StableId);
            HashSet<string> bossPhaseIds = StableIdSet(input.BossPhases, value => value.StableId);

            for (int index = 0; index < input.Enemies.Count; index++)
            {
                EnemyArchetypeSnapshot enemy = input.Enemies[index];
                if (enemy == null)
                {
                    continue;
                }

                if (enemy.Category == EnemyArchetypeCategory.Boss)
                {
                    issues.Add(new EnemyDomainValidationIssue("ENEMY_CATEGORY_BOSS_RESERVED", "enemies[" + index + "].category", "Boss identities belong in BossArchetypeSnapshot."));
                }

                ValidateReferenceIds(enemy.MechanicProfileIds, mechanicIds, "enemies[" + index + "].mechanicProfileIds", "MECHANIC_PROFILE", issues);
                ValidateReferenceIds(enemy.SkillPatternIds, skillPatternIds, "enemies[" + index + "].skillPatternIds", "SKILL_PATTERN", issues);
            }

            for (int index = 0; index < input.Bosses.Count; index++)
            {
                BossArchetypeSnapshot boss = input.Bosses[index];
                if (boss == null)
                {
                    continue;
                }

                ValidateReferenceIds(boss.MechanicProfileIds, mechanicIds, "bosses[" + index + "].mechanicProfileIds", "MECHANIC_PROFILE", issues);
                ValidateReferenceIds(boss.SkillPatternIds, skillPatternIds, "bosses[" + index + "].skillPatternIds", "SKILL_PATTERN", issues);
                ValidateReferenceIds(boss.PhaseProfileIds, bossPhaseIds, "bosses[" + index + "].phaseProfileIds", "BOSS_PHASE", issues);
            }

            return Array.AsReadOnly(issues.ToArray());
        }

        private static void ValidateCollection<T>(
            IReadOnlyList<T> values,
            string path,
            Func<T, string> stableId,
            ICollection<EnemyDomainValidationIssue> issues)
            where T : class
        {
            Dictionary<string, int> firstIndexById = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int index = 0; index < values.Count; index++)
            {
                T value = values[index];
                if (value == null)
                {
                    issues.Add(new EnemyDomainValidationIssue("ENTRY_NULL", path + "[" + index + "]", "Null entries are not allowed."));
                    continue;
                }

                string id = stableId(value) ?? string.Empty;
                ValidateStableId(id, path + "[" + index + "].stableId", issues);
                if (firstIndexById.TryGetValue(id, out int firstIndex))
                {
                    issues.Add(new EnemyDomainValidationIssue("STABLE_ID_DUPLICATE", path + "[" + index + "].stableId", "Duplicate exact ID; first seen at index " + firstIndex + "."));
                }
                else
                {
                    firstIndexById.Add(id, index);
                }

                if (value is IEnemyDomainIsolationMetadata isolation
                    && (!isolation.DevOnly || isolation.IsEnabled || isolation.EntersFormalFlow))
                {
                    issues.Add(new EnemyDomainValidationIssue("DEV_ISOLATION_INVALID", path + "[" + index + "]", "Required defaults are devOnly=true, isEnabled=false, entersFormalFlow=false."));
                }
            }
        }

        private static void ValidateArchetypeIdSeparation(EnemyDomainSnapshotInput input, ICollection<EnemyDomainValidationIssue> issues)
        {
            HashSet<string> enemyIds = StableIdSet(input.Enemies, value => value.StableId);
            for (int index = 0; index < input.Bosses.Count; index++)
            {
                BossArchetypeSnapshot boss = input.Bosses[index];
                if (boss != null && enemyIds.Contains(boss.StableId))
                {
                    issues.Add(new EnemyDomainValidationIssue("ARCHETYPE_ID_SHARED", "bosses[" + index + "].stableId", "Enemy and Boss identities must not share the same stable ID."));
                }
            }
        }

        private static void ValidateReferenceIds(
            IReadOnlyList<string> referenceIds,
            ISet<string> knownIds,
            string path,
            string codePrefix,
            ICollection<EnemyDomainValidationIssue> issues)
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < referenceIds.Count; index++)
            {
                string id = referenceIds[index] ?? string.Empty;
                ValidateStableId(id, path + "[" + index + "]", issues);
                if (!seen.Add(id))
                {
                    issues.Add(new EnemyDomainValidationIssue(codePrefix + "_REFERENCE_DUPLICATE", path + "[" + index + "]", "Duplicate exact reference ID."));
                }

                if (!knownIds.Contains(id))
                {
                    issues.Add(new EnemyDomainValidationIssue(codePrefix + "_REFERENCE_UNRESOLVED", path + "[" + index + "]", "Reference does not resolve with ordinal stable-ID semantics."));
                }
            }
        }

        private static void ValidateStableId(string id, string path, ICollection<EnemyDomainValidationIssue> issues)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                issues.Add(new EnemyDomainValidationIssue("STABLE_ID_EMPTY", path, "Stable ID must not be empty or whitespace."));
                return;
            }

            if (!string.Equals(id, id.Trim(), StringComparison.Ordinal))
            {
                issues.Add(new EnemyDomainValidationIssue("STABLE_ID_OUTER_WHITESPACE", path, "Stable ID must not be normalized implicitly."));
            }
        }

        private static HashSet<string> StableIdSet<T>(IReadOnlyList<T> values, Func<T, string> stableId)
            where T : class
        {
            return new HashSet<string>(values
                .Where(value => value != null)
                .Select(stableId), StringComparer.Ordinal);
        }
    }
}
