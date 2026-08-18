using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TalismanBag.EnemySystem.RequirementChannel
{
    public static class EnemyRequirementChannelApplicabilitySchema
    {
        public const string SchemaId = "EnemyRequirementChannelApplicability.v1";
        public const int SchemaVersion = 1;
    }

    public enum EnemyRequirementChannel
    {
        ContinuousBP = 1,
        StructuralPredicate = 2,
        RuntimeEventSignal = 3
    }

    public enum EnemyRequirementApplicabilityState
    {
        Applicable = 1,
        Unknown = 2,
        NotApplicable = 3,
        NotInChannel = 4
    }

    public interface IEnemyRequirementChannelApplicabilitySnapshotProvider
    {
        EnemyRequirementChannelApplicabilitySnapshot CreateSnapshot(
            EnemyRequirementChannelApplicabilitySnapshotInput input);
    }

    public interface IEnemyRequirementChannelApplicabilitySnapshotValidator
    {
        IReadOnlyList<EnemyRequirementChannelApplicabilityValidationIssue> Validate(
            EnemyRequirementChannelApplicabilitySnapshotInput input);
    }

    internal static class EnemyRequirementChannelApplicabilityReadOnly
    {
        public static string Text(string value)
        {
            return value ?? string.Empty;
        }

        public static ReadOnlyCollection<EnemyRequirementChannelApplicabilityRowSnapshot>
            FreezeRows(IEnumerable<EnemyRequirementChannelApplicabilityRowSnapshot> values)
        {
            return Array.AsReadOnly((values ??
                    Array.Empty<EnemyRequirementChannelApplicabilityRowSnapshot>())
                .Select(value => value == null ? null : value.Clone())
                .ToArray());
        }

        public static ReadOnlyCollection<T> ToReadOnly<T>(this IEnumerable<T> values)
        {
            return Array.AsReadOnly((values ?? Array.Empty<T>()).ToArray());
        }
    }
}

