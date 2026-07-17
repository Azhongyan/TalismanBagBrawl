using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TalismanBag.EnemySystem.Domain;
using TalismanBag.EnemySystem.Normalization;
using TalismanBag.EnemySystem.Vocabulary;

namespace TalismanBag.EnemySystem.SkillPhase
{
    public static class EnemySkillBossPhaseSchema
    {
        public const string SchemaId = "EnemySkillBossPhase.v1";
        public const int SchemaVersion = 1;
    }

    public enum SkillCastKind
    {
        Instant = 0,
        Channeled = 1
    }

    public enum EnemySkillCarrierKind
    {
        Enemy = 0,
        Boss = 1
    }

    public enum BossPhaseEntryConditionKind
    {
        EncounterStart = 0,
        HealthRatioAtOrBelow = 1,
        PreviousPhaseCompleted = 2,
        MechanicSignal = 3
    }

    public interface IEnemySkillBossPhaseReferenceResolver
    {
        bool TryGetEnemy(string stableId, out EnemyArchetypeSnapshot enemy);
        bool TryGetBoss(string stableId, out BossArchetypeSnapshot boss);
        bool TryGetMechanicProfileKind(string stableId, out ValidationProfileKind kind);
        bool HasCarrierMechanicBinding(
            EnemySkillCarrierKind kind,
            string carrierId,
            string mechanicProfileId);
        bool HasVocabularyKey(EnemyVocabularyCategory category, string stableKey);
    }

    public interface IEnemySkillBossPhaseLookup
    {
        bool TryGetSkillPatternById(string skillPatternId, out EnemySkillPatternSnapshot pattern);
        bool TryGetSkillSequenceById(string skillSequenceId, out SkillSequenceSnapshot sequence);
        bool TryGetCarrierSkillBinding(
            EnemySkillCarrierKind kind,
            string carrierId,
            out CarrierSkillBindingSnapshot binding);
        bool TryGetBossPhaseById(string bossPhaseId, out BossPhaseProfileSnapshot phase);
        bool TryGetBossPhasePlanByBossId(string bossId, out BossPhasePlanSnapshot plan);
    }

    public interface IEnemySkillBossPhaseProvider
    {
        EnemySkillBossPhaseCatalogSnapshot CreateSnapshot(
            EnemySkillBossPhaseCatalogInput input,
            IEnemySkillBossPhaseReferenceResolver resolver);
    }

    public interface IEnemySkillBossPhaseValidator
    {
        IReadOnlyList<EnemySkillBossPhaseValidationIssue> Validate(
            EnemySkillBossPhaseCatalogInput input,
            IEnemySkillBossPhaseReferenceResolver resolver);
    }

    internal static class EnemySkillBossPhaseReadOnly
    {
        public static string Text(string value)
        {
            return value ?? string.Empty;
        }

        public static ReadOnlyCollection<string> Strings(
            IEnumerable<string> values,
            bool sortOrdinal = true)
        {
            IEnumerable<string> source = (values ?? Array.Empty<string>()).Select(Text);
            if (sortOrdinal)
            {
                source = source.OrderBy(value => value, StringComparer.Ordinal);
            }

            return Array.AsReadOnly(source.ToArray());
        }

        public static ReadOnlyCollection<T> Freeze<T>(
            IEnumerable<T> values,
            Func<T, T> clone)
            where T : class
        {
            if (clone == null)
            {
                throw new ArgumentNullException(nameof(clone));
            }

            return Array.AsReadOnly((values ?? Array.Empty<T>())
                .Select(value => value == null ? null : clone(value))
                .ToArray());
        }

        public static ReadOnlyCollection<T> ToReadOnly<T>(this IEnumerable<T> values)
        {
            return Array.AsReadOnly((values ?? Array.Empty<T>()).ToArray());
        }
    }
}
