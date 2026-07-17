using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TalismanBag.EnemySystem.Vocabulary;

namespace TalismanBag.EnemySystem.PressureWindow
{
    public static class CounterWindowAndPressureSchema
    {
        public const string SchemaId = "CounterWindowAndPressure.v1";
        public const int SchemaVersion = 1;
    }

    public enum PressureSourceKind
    {
        MechanicProfile = 0,
        MapRule = 1,
        SkillPattern = 2,
        BossPhase = 3
    }

    public enum CapabilityRequirementRole
    {
        Required = 0,
        Recommended = 1
    }

    public enum RequirementMatchMode
    {
        Any = 0,
        All = 1
    }

    public enum CounterWindowConditionKind
    {
        MechanicSignal = 0,
        SkillPatternStarted = 1,
        SkillPatternCompleted = 2,
        SkillPatternInterrupted = 3,
        BossPhaseEntered = 4,
        BossPhaseExited = 5
    }

    public interface ICounterWindowAndPressureReferenceResolver
    {
        bool HasVocabularyKey(EnemyVocabularyCategory category, string stableKey);
        bool HasMechanicProfile(string mechanicProfileId);
        bool HasMapRule(string mapRuleId);
        bool HasSkillPattern(string skillPatternId);
        bool HasBossPhase(string bossPhaseId);
    }

    public interface ICounterWindowAndPressureLookup
    {
        bool TryGetBuildPressureProfileById(
            string buildPressureProfileId,
            out BuildPressureProfileSnapshot profile);

        bool TryGetCounterWindowProfileById(
            string counterWindowId,
            out CounterWindowProfileSnapshot profile);
    }

    public interface ICounterWindowAndPressureProvider
    {
        CounterWindowAndPressureCatalogSnapshot CreateSnapshot(
            CounterWindowAndPressureCatalogInput input,
            ICounterWindowAndPressureReferenceResolver resolver);
    }

    public interface ICounterWindowAndPressureValidator
    {
        IReadOnlyList<CounterWindowAndPressureValidationIssue> Validate(
            CounterWindowAndPressureCatalogInput input,
            ICounterWindowAndPressureReferenceResolver resolver);
    }

    internal static class CounterWindowAndPressureReadOnly
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
