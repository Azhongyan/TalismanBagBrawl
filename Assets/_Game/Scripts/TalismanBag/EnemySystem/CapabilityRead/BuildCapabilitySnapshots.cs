using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TalismanBag.EnemySystem.CapabilityRead
{
    public sealed class BuildCapabilitySourceSummarySnapshot
    {
        public BuildCapabilitySourceSummarySnapshot(
            string sourceCategoryId,
            int sourceCount,
            int contributionBasisPoints,
            bool isConditional)
        {
            SourceCategoryId = BuildCapabilityReadOnly.Text(sourceCategoryId);
            SourceCount = sourceCount;
            ContributionBasisPoints = contributionBasisPoints;
            IsConditional = isConditional;
        }

        public string SourceCategoryId { get; }
        public int SourceCount { get; }
        public int ContributionBasisPoints { get; }
        public bool IsConditional { get; }

        internal BuildCapabilitySourceSummarySnapshot Clone()
        {
            return new BuildCapabilitySourceSummarySnapshot(
                SourceCategoryId,
                SourceCount,
                ContributionBasisPoints,
                IsConditional);
        }
    }

    public sealed class BuildCapabilityValueSnapshot
    {
        private readonly ReadOnlyCollection<BuildCapabilitySourceSummarySnapshot>
            sourceSummaries;

        public BuildCapabilityValueSnapshot(
            string buildCapabilityKey,
            int valueBasisPoints,
            IEnumerable<BuildCapabilitySourceSummarySnapshot> sourceSummaries)
        {
            BuildCapabilityKey = BuildCapabilityReadOnly.Text(buildCapabilityKey);
            ValueBasisPoints = valueBasisPoints;
            this.sourceSummaries = BuildCapabilityReadOnly.Freeze(
                    sourceSummaries,
                    value => value.Clone())
                .OrderBy(
                    value => value == null ? string.Empty : value.SourceCategoryId,
                    StringComparer.Ordinal)
                .ToReadOnly();
        }

        public string BuildCapabilityKey { get; }
        public int ValueBasisPoints { get; }
        public IReadOnlyList<BuildCapabilitySourceSummarySnapshot> SourceSummaries =>
            sourceSummaries;

        internal BuildCapabilityValueSnapshot Clone()
        {
            return new BuildCapabilityValueSnapshot(
                BuildCapabilityKey,
                ValueBasisPoints,
                sourceSummaries);
        }
    }

    public sealed class BuildCapabilitySnapshotInput
    {
        private readonly ReadOnlyCollection<BuildCapabilityValueSnapshot>
            capabilityValues;

        public BuildCapabilitySnapshotInput(
            string snapshotId,
            string sourceRevisionId,
            BuildCapabilityCoverageMode coverageMode,
            IEnumerable<BuildCapabilityValueSnapshot> capabilityValues,
            bool devOnly = true,
            bool isEnabled = false,
            bool entersFormalFlow = false,
            string schemaId = BuildCapabilityReadSchema.SchemaId,
            int schemaVersion = BuildCapabilityReadSchema.SchemaVersion)
        {
            SchemaId = BuildCapabilityReadOnly.Text(schemaId);
            SchemaVersion = schemaVersion;
            SnapshotId = BuildCapabilityReadOnly.Text(snapshotId);
            SourceRevisionId = BuildCapabilityReadOnly.Text(sourceRevisionId);
            CoverageMode = coverageMode;
            this.capabilityValues = BuildCapabilityReadOnly.Freeze(
                capabilityValues,
                value => value.Clone());
            DevOnly = devOnly;
            IsEnabled = isEnabled;
            EntersFormalFlow = entersFormalFlow;
        }

        public string SchemaId { get; }
        public int SchemaVersion { get; }
        public string SnapshotId { get; }
        public string SourceRevisionId { get; }
        public BuildCapabilityCoverageMode CoverageMode { get; }
        public IReadOnlyList<BuildCapabilityValueSnapshot> CapabilityValues =>
            capabilityValues;
        public bool DevOnly { get; }
        public bool IsEnabled { get; }
        public bool EntersFormalFlow { get; }
    }

    public sealed class BuildCapabilitySnapshot : IBuildCapabilityReadOnlySnapshot
    {
        private readonly ReadOnlyCollection<BuildCapabilityValueSnapshot>
            capabilityValues;
        private readonly IReadOnlyDictionary<string, BuildCapabilityValueSnapshot>
            capabilityByKey;

        internal BuildCapabilitySnapshot(BuildCapabilitySnapshotInput input)
        {
            SchemaId = input.SchemaId;
            SchemaVersion = input.SchemaVersion;
            SnapshotId = input.SnapshotId;
            SourceRevisionId = input.SourceRevisionId;
            CoverageMode = input.CoverageMode;
            capabilityValues = BuildCapabilityReadOnly.Freeze(
                    input.CapabilityValues,
                    value => value.Clone())
                .OrderBy(value => value.BuildCapabilityKey, StringComparer.Ordinal)
                .ToReadOnly();
            capabilityByKey = new ReadOnlyDictionary<string, BuildCapabilityValueSnapshot>(
                capabilityValues.ToDictionary(
                    value => value.BuildCapabilityKey,
                    value => value,
                    StringComparer.Ordinal));
            DevOnly = input.DevOnly;
            IsEnabled = input.IsEnabled;
            EntersFormalFlow = input.EntersFormalFlow;
            CanonicalSignature = BuildCapabilityCanonical.Hash(
                BuildCapabilityCanonical.Snapshot(this));
        }

        public string SchemaId { get; }
        public int SchemaVersion { get; }
        public string SnapshotId { get; }
        public string SourceRevisionId { get; }
        public BuildCapabilityCoverageMode CoverageMode { get; }
        public IReadOnlyList<BuildCapabilityValueSnapshot> CapabilityValues =>
            capabilityValues;
        public string CanonicalSignature { get; }
        public bool DevOnly { get; }
        public bool IsEnabled { get; }
        public bool EntersFormalFlow { get; }

        public bool TryGetCapabilityValue(
            string key,
            out BuildCapabilityValueSnapshot value)
        {
            if (key != null)
            {
                return capabilityByKey.TryGetValue(key, out value);
            }

            value = null;
            return false;
        }
    }

    internal static class BuildCapabilityCanonical
    {
        public static string Snapshot(BuildCapabilitySnapshot value)
        {
            StringBuilder builder = new StringBuilder(8192);
            Field(builder, "schemaId", value.SchemaId);
            Field(
                builder,
                "schemaVersion",
                value.SchemaVersion.ToString(CultureInfo.InvariantCulture));
            Field(builder, "snapshotId", value.SnapshotId);
            Field(builder, "sourceRevisionId", value.SourceRevisionId);
            Field(
                builder,
                "coverageMode",
                ((int)value.CoverageMode).ToString(CultureInfo.InvariantCulture));
            Rows(builder, "capabilityValues", value.CapabilityValues, CapabilityValue);
            Field(builder, "devOnly", value.DevOnly ? "1" : "0");
            Field(builder, "isEnabled", value.IsEnabled ? "1" : "0");
            Field(builder, "entersFormalFlow", value.EntersFormalFlow ? "1" : "0");
            return builder.ToString();
        }

        private static string CapabilityValue(BuildCapabilityValueSnapshot value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "buildCapabilityKey", value.BuildCapabilityKey);
            Field(
                builder,
                "valueBasisPoints",
                value.ValueBasisPoints.ToString(CultureInfo.InvariantCulture));
            Rows(builder, "sourceSummaries", value.SourceSummaries, SourceSummary);
            return builder.ToString();
        }

        private static string SourceSummary(
            BuildCapabilitySourceSummarySnapshot value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "sourceCategoryId", value.SourceCategoryId);
            Field(
                builder,
                "sourceCount",
                value.SourceCount.ToString(CultureInfo.InvariantCulture));
            Field(
                builder,
                "contributionBasisPoints",
                value.ContributionBasisPoints.ToString(CultureInfo.InvariantCulture));
            Field(builder, "isConditional", value.IsConditional ? "1" : "0");
            return builder.ToString();
        }

        private static void Rows<T>(
            StringBuilder builder,
            string name,
            IEnumerable<T> values,
            Func<T, string> canonical)
        {
            string[] rows = (values ?? Array.Empty<T>())
                .Select(value => value == null ? "<null>" : canonical(value))
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            Field(
                builder,
                name + ".count",
                rows.Length.ToString(CultureInfo.InvariantCulture));
            for (int index = 0; index < rows.Length; index++)
            {
                Field(
                    builder,
                    name + "[" + index.ToString(CultureInfo.InvariantCulture) + "]",
                    rows[index]);
            }
        }

        private static void Field(
            StringBuilder builder,
            string name,
            string value)
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
                byte[] bytes = sha256.ComputeHash(
                    Encoding.UTF8.GetBytes(payload ?? string.Empty));
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
