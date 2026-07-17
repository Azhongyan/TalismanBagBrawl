using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TalismanBag.EnemySystem.Domain;

namespace TalismanBag.EnemySystem.CapabilityRead
{
    public static class BuildCapabilityReadSchema
    {
        public const string SchemaId = "BuildCapabilityReadContract.v1";
        public const int SchemaVersion = 1;
    }

    public static class BuildCapabilityScale
    {
        public const int MinimumValueBasisPoints = 0;
        public const int MaximumValueBasisPoints = 10000;
    }

    public enum BuildCapabilityCoverageMode
    {
        Complete = 0,
        Sparse = 1
    }

    public enum CapabilityValueAvailability
    {
        Unknown = 0,
        Known = 1
    }

    public enum ReadinessBand
    {
        Unknown = 0,
        Blocked = 1,
        Strained = 2,
        Ready = 3,
        Strong = 4
    }

    public interface IBuildCapabilityVocabularyResolver
    {
        bool HasBuildCapabilityKey(string key);
        IReadOnlyList<string> GetKnownBuildCapabilityKeys();
    }

    public interface IBuildCapabilityReadOnlySnapshot : IEnemyDomainIsolationMetadata
    {
        string SchemaId { get; }
        int SchemaVersion { get; }
        string SnapshotId { get; }
        string SourceRevisionId { get; }
        BuildCapabilityCoverageMode CoverageMode { get; }
        IReadOnlyList<BuildCapabilityValueSnapshot> CapabilityValues { get; }
        string CanonicalSignature { get; }
        bool TryGetCapabilityValue(
            string key,
            out BuildCapabilityValueSnapshot value);
    }

    public interface IBuildCapabilitySnapshotProvider
    {
        BuildCapabilitySnapshot CreateSnapshot(
            BuildCapabilitySnapshotInput input,
            IBuildCapabilityVocabularyResolver resolver);
    }

    public interface IBuildCapabilitySnapshotValidator
    {
        IReadOnlyList<BuildCapabilityValidationIssue> Validate(
            BuildCapabilitySnapshotInput input,
            IBuildCapabilityVocabularyResolver resolver);
    }

    internal static class BuildCapabilityReadOnly
    {
        public static string Text(string value)
        {
            return value ?? string.Empty;
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

        public static ReadOnlyCollection<T> ToReadOnly<T>(
            this IEnumerable<T> values)
        {
            return Array.AsReadOnly((values ?? Array.Empty<T>()).ToArray());
        }
    }
}
