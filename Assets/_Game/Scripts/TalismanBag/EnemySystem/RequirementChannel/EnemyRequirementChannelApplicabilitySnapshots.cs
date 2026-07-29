using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TalismanBag.EnemySystem.RequirementChannel
{
    public sealed class EnemyRequirementChannelApplicabilityRowSnapshot
    {
        public EnemyRequirementChannelApplicabilityRowSnapshot(
            string requirementId,
            EnemyRequirementChannel declaredChannel,
            EnemyRequirementApplicabilityState applicabilityState)
        {
            RequirementId = EnemyRequirementChannelApplicabilityReadOnly.Text(requirementId);
            DeclaredChannel = declaredChannel;
            ApplicabilityState = applicabilityState;
        }

        public string RequirementId { get; }
        public EnemyRequirementChannel DeclaredChannel { get; }
        public EnemyRequirementApplicabilityState ApplicabilityState { get; }

        internal EnemyRequirementChannelApplicabilityRowSnapshot Clone()
        {
            return new EnemyRequirementChannelApplicabilityRowSnapshot(
                RequirementId,
                DeclaredChannel,
                ApplicabilityState);
        }
    }

    public sealed class EnemyRequirementChannelApplicabilitySnapshotInput
    {
        private readonly ReadOnlyCollection<EnemyRequirementChannelApplicabilityRowSnapshot>
            rows;

        public EnemyRequirementChannelApplicabilitySnapshotInput(
            string snapshotId,
            EnemyRequirementChannel evaluationChannel,
            IEnumerable<EnemyRequirementChannelApplicabilityRowSnapshot> rows,
            string schemaId = EnemyRequirementChannelApplicabilitySchema.SchemaId,
            int schemaVersion = EnemyRequirementChannelApplicabilitySchema.SchemaVersion)
        {
            SchemaId = EnemyRequirementChannelApplicabilityReadOnly.Text(schemaId);
            SchemaVersion = schemaVersion;
            SnapshotId = EnemyRequirementChannelApplicabilityReadOnly.Text(snapshotId);
            EvaluationChannel = evaluationChannel;
            this.rows = EnemyRequirementChannelApplicabilityReadOnly.FreezeRows(rows);
        }

        public string SchemaId { get; }
        public int SchemaVersion { get; }
        public string SnapshotId { get; }
        public EnemyRequirementChannel EvaluationChannel { get; }
        public IReadOnlyList<EnemyRequirementChannelApplicabilityRowSnapshot> Rows => rows;
    }

    public sealed class EnemyRequirementChannelApplicabilitySnapshot
    {
        private readonly ReadOnlyCollection<EnemyRequirementChannelApplicabilityRowSnapshot>
            rows;

        internal EnemyRequirementChannelApplicabilitySnapshot(
            EnemyRequirementChannelApplicabilitySnapshotInput input)
        {
            SchemaId = input.SchemaId;
            SchemaVersion = input.SchemaVersion;
            SnapshotId = input.SnapshotId;
            EvaluationChannel = input.EvaluationChannel;
            rows = EnemyRequirementChannelApplicabilityReadOnly.FreezeRows(input.Rows)
                .OrderBy(value => value.RequirementId, StringComparer.Ordinal)
                .ToReadOnly();
            CanonicalSignature = EnemyRequirementChannelApplicabilityCanonical.Hash(
                EnemyRequirementChannelApplicabilityCanonical.Snapshot(this));
        }

        public string SchemaId { get; }
        public int SchemaVersion { get; }
        public string SnapshotId { get; }
        public EnemyRequirementChannel EvaluationChannel { get; }
        public IReadOnlyList<EnemyRequirementChannelApplicabilityRowSnapshot> Rows => rows;
        public string CanonicalSignature { get; }
    }

    internal static class EnemyRequirementChannelApplicabilityCanonical
    {
        public static string Snapshot(EnemyRequirementChannelApplicabilitySnapshot value)
        {
            StringBuilder builder = new StringBuilder(4096);
            Field(builder, "schemaId", value.SchemaId);
            Field(
                builder,
                "schemaVersion",
                value.SchemaVersion.ToString(CultureInfo.InvariantCulture));
            Field(builder, "snapshotId", value.SnapshotId);
            Field(
                builder,
                "evaluationChannel",
                ((int)value.EvaluationChannel).ToString(CultureInfo.InvariantCulture));
            Rows(builder, value.Rows);
            return builder.ToString();
        }

        private static void Rows(
            StringBuilder builder,
            IEnumerable<EnemyRequirementChannelApplicabilityRowSnapshot> values)
        {
            string[] rows = (values ??
                    Array.Empty<EnemyRequirementChannelApplicabilityRowSnapshot>())
                .Select(Row)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            Field(
                builder,
                "rows.count",
                rows.Length.ToString(CultureInfo.InvariantCulture));
            for (int index = 0; index < rows.Length; index++)
            {
                Field(
                    builder,
                    "rows[" + index.ToString(CultureInfo.InvariantCulture) + "]",
                    rows[index]);
            }
        }

        private static string Row(EnemyRequirementChannelApplicabilityRowSnapshot value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "requirementId", value.RequirementId);
            Field(
                builder,
                "declaredChannel",
                ((int)value.DeclaredChannel).ToString(CultureInfo.InvariantCulture));
            Field(
                builder,
                "applicabilityState",
                ((int)value.ApplicabilityState).ToString(CultureInfo.InvariantCulture));
            return builder.ToString();
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

