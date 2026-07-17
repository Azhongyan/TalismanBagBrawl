using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.EnemySystem.CapabilityRead;
using TalismanBag.EnemySystem.PressureWindow;

namespace TalismanBag.EnemySystem.ReadinessEvaluation
{
    public sealed class RequirementCapabilityEvaluationSnapshot
    {
        public RequirementCapabilityEvaluationSnapshot(
            string requirementGroupId,
            CapabilityRequirementRole requirementRole,
            RequirementMatchMode requirementMatchMode,
            string buildCapabilityKey,
            CapabilityValueAvailability capabilityValueAvailability,
            int baseValueBasisPoints,
            long mapDeltaBasisPoints,
            int effectiveValueBasisPoints,
            int requiredValueBasisPoints,
            int gapBasisPoints,
            RequirementEvaluationStatus requirementStatus,
            RequirementGroupStatus groupStatus)
        {
            RequirementGroupId = EnemyReadinessReadOnly.Text(requirementGroupId);
            RequirementRole = requirementRole;
            RequirementMatchMode = requirementMatchMode;
            BuildCapabilityKey = EnemyReadinessReadOnly.Text(buildCapabilityKey);
            BaseValueBasisPoints = baseValueBasisPoints;
            MapDeltaBasisPoints = mapDeltaBasisPoints;
            RequirementStatus = requirementStatus;
            GroupStatus = groupStatus;
            CapabilityGap = new CapabilityGapSnapshot(
                RequirementGroupId,
                BuildCapabilityKey,
                capabilityValueAvailability,
                effectiveValueBasisPoints,
                requiredValueBasisPoints,
                gapBasisPoints);
        }

        public string RequirementGroupId { get; }
        public CapabilityRequirementRole RequirementRole { get; }
        public RequirementMatchMode RequirementMatchMode { get; }
        public string BuildCapabilityKey { get; }
        public CapabilityValueAvailability CapabilityValueAvailability =>
            CapabilityGap.CapabilityValueAvailability;
        public int BaseValueBasisPoints { get; }
        public long MapDeltaBasisPoints { get; }
        public int EffectiveValueBasisPoints => CapabilityGap.AvailableBasisPoints;
        public int RequiredValueBasisPoints => CapabilityGap.RequiredBasisPoints;
        public int GapBasisPoints => CapabilityGap.GapBasisPoints;
        public RequirementEvaluationStatus RequirementStatus { get; }
        public RequirementGroupStatus GroupStatus { get; }
        public CapabilityGapSnapshot CapabilityGap { get; }

        internal RequirementCapabilityEvaluationSnapshot Clone()
        {
            return new RequirementCapabilityEvaluationSnapshot(
                RequirementGroupId,
                RequirementRole,
                RequirementMatchMode,
                BuildCapabilityKey,
                CapabilityValueAvailability,
                BaseValueBasisPoints,
                MapDeltaBasisPoints,
                EffectiveValueBasisPoints,
                RequiredValueBasisPoints,
                GapBasisPoints,
                RequirementStatus,
                GroupStatus);
        }
    }

    public sealed class RequirementGroupEvaluationSnapshot
    {
        private readonly ReadOnlyCollection<RequirementCapabilityEvaluationSnapshot>
            requirementEvaluations;

        public RequirementGroupEvaluationSnapshot(
            string requirementGroupId,
            CapabilityRequirementRole requirementRole,
            RequirementMatchMode requirementMatchMode,
            RequirementGroupStatus groupStatus,
            IEnumerable<RequirementCapabilityEvaluationSnapshot>
                requirementEvaluations)
        {
            RequirementGroupId = EnemyReadinessReadOnly.Text(requirementGroupId);
            RequirementRole = requirementRole;
            RequirementMatchMode = requirementMatchMode;
            GroupStatus = groupStatus;
            this.requirementEvaluations = EnemyReadinessReadOnly.Freeze(
                    requirementEvaluations,
                    value => value.Clone())
                .OrderBy(
                    value => value == null
                        ? string.Empty
                        : value.BuildCapabilityKey,
                    StringComparer.Ordinal)
                .ThenBy(value => value == null ? 0 : value.RequiredValueBasisPoints)
                .ToReadOnly();
        }

        public string RequirementGroupId { get; }
        public CapabilityRequirementRole RequirementRole { get; }
        public RequirementMatchMode RequirementMatchMode { get; }
        public RequirementGroupStatus GroupStatus { get; }
        public IReadOnlyList<RequirementCapabilityEvaluationSnapshot>
            RequirementEvaluations => requirementEvaluations;

        internal RequirementGroupEvaluationSnapshot Clone()
        {
            return new RequirementGroupEvaluationSnapshot(
                RequirementGroupId,
                RequirementRole,
                RequirementMatchMode,
                GroupStatus,
                requirementEvaluations);
        }
    }

    public sealed class DeveloperReadinessSnapshot
    {
        private readonly ReadOnlyCollection<RequirementGroupEvaluationSnapshot>
            requirementGroupEvaluations;
        private readonly ReadOnlyCollection<RequirementCapabilityEvaluationSnapshot>
            requirementEvaluations;
        private readonly ReadOnlyCollection<MapRuleCapabilityAdjustmentSnapshot>
            appliedMapRuleAdjustments;

        public DeveloperReadinessSnapshot(
            string buildSnapshotId,
            string buildSnapshotCanonicalSignature,
            string pressureCatalogCanonicalSignature,
            string buildPressureProfileId,
            ReadinessBand readinessBand,
            IEnumerable<RequirementGroupEvaluationSnapshot>
                requirementGroupEvaluations,
            IEnumerable<RequirementCapabilityEvaluationSnapshot>
                requirementEvaluations,
            IEnumerable<MapRuleCapabilityAdjustmentSnapshot>
                appliedMapRuleAdjustments)
        {
            BuildSnapshotId = EnemyReadinessReadOnly.Text(buildSnapshotId);
            BuildSnapshotCanonicalSignature = EnemyReadinessReadOnly.Text(
                buildSnapshotCanonicalSignature);
            PressureCatalogCanonicalSignature = EnemyReadinessReadOnly.Text(
                pressureCatalogCanonicalSignature);
            BuildPressureProfileId = EnemyReadinessReadOnly.Text(
                buildPressureProfileId);
            ReadinessBand = readinessBand;
            this.requirementGroupEvaluations = EnemyReadinessReadOnly.Freeze(
                    requirementGroupEvaluations,
                    value => value.Clone())
                .OrderBy(
                    value => value == null
                        ? string.Empty
                        : value.RequirementGroupId,
                    StringComparer.Ordinal)
                .ToReadOnly();
            this.requirementEvaluations = EnemyReadinessReadOnly.Freeze(
                    requirementEvaluations,
                    value => value.Clone())
                .OrderBy(
                    value => value == null
                        ? string.Empty
                        : value.RequirementGroupId,
                    StringComparer.Ordinal)
                .ThenBy(
                    value => value == null
                        ? string.Empty
                        : value.BuildCapabilityKey,
                    StringComparer.Ordinal)
                .ToReadOnly();
            this.appliedMapRuleAdjustments = EnemyReadinessReadOnly.Freeze(
                    appliedMapRuleAdjustments,
                    value => value.Clone())
                .OrderBy(
                    value => value == null ? string.Empty : value.MapRuleId,
                    StringComparer.Ordinal)
                .ThenBy(
                    value => value == null
                        ? string.Empty
                        : value.BuildCapabilityKey,
                    StringComparer.Ordinal)
                .ThenBy(
                    value => value == null
                        ? string.Empty
                        : value.DeveloperReasonId,
                    StringComparer.Ordinal)
                .ThenBy(value => value == null ? 0 : value.DeltaBasisPoints)
                .ToReadOnly();
            DeveloperCanonicalSignature = EnemyReadinessCanonical.Hash(
                EnemyReadinessCanonical.Developer(this));
        }

        public string SchemaId => EnemyOfflineReadinessSchema.SchemaId;
        public int SchemaVersion => EnemyOfflineReadinessSchema.SchemaVersion;
        public string BuildSnapshotId { get; }
        public string BuildSnapshotCanonicalSignature { get; }
        public string PressureCatalogCanonicalSignature { get; }
        public string BuildPressureProfileId { get; }
        public ReadinessBand ReadinessBand { get; }
        public IReadOnlyList<RequirementGroupEvaluationSnapshot>
            RequirementGroupEvaluations => requirementGroupEvaluations;
        public IReadOnlyList<RequirementCapabilityEvaluationSnapshot>
            RequirementEvaluations => requirementEvaluations;
        public IReadOnlyList<MapRuleCapabilityAdjustmentSnapshot>
            AppliedMapRuleAdjustments => appliedMapRuleAdjustments;
        public string DeveloperCanonicalSignature { get; }
        public bool DeveloperOnly => true;
    }

    public sealed class EnemyReadinessPlayerHintProjection
    {
        private readonly ReadOnlyCollection<string> playerHintCategoryKeys;

        public EnemyReadinessPlayerHintProjection(
            string buildPressureProfileId,
            string publicPressureLabelKey,
            string publicPressureHintKey,
            IEnumerable<string> playerHintCategoryKeys,
            PlayerHintSeverity playerHintSeverity)
        {
            BuildPressureProfileId = EnemyReadinessReadOnly.Text(
                buildPressureProfileId);
            PublicPressureLabelKey = EnemyReadinessReadOnly.Text(
                publicPressureLabelKey);
            PublicPressureHintKey = EnemyReadinessReadOnly.Text(
                publicPressureHintKey);
            this.playerHintCategoryKeys = EnemyReadinessReadOnly.Strings(
                playerHintCategoryKeys);
            PlayerHintSeverity = playerHintSeverity;
            PlayerSafeCanonicalSignature = EnemyReadinessCanonical.Hash(
                BuildPlayerSafeCanonicalPayload());
        }

        public string BuildPressureProfileId { get; }
        public string PublicPressureLabelKey { get; }
        public string PublicPressureHintKey { get; }
        public IReadOnlyList<string> PlayerHintCategoryKeys =>
            playerHintCategoryKeys;
        public PlayerHintSeverity PlayerHintSeverity { get; }
        public string PlayerSafeCanonicalSignature { get; }

        public string BuildPlayerSafeCanonicalPayload()
        {
            return EnemyReadinessCanonical.PlayerSafe(this);
        }
    }

    public sealed class EnemyReadinessEvaluationResult
    {
        public EnemyReadinessEvaluationResult(
            DeveloperReadinessSnapshot developerReadiness,
            EnemyReadinessPlayerHintProjection playerHintProjection)
        {
            DeveloperReadiness = developerReadiness ??
                throw new ArgumentNullException(nameof(developerReadiness));
            PlayerHintProjection = playerHintProjection ??
                throw new ArgumentNullException(nameof(playerHintProjection));
        }

        public DeveloperReadinessSnapshot DeveloperReadiness { get; }
        public EnemyReadinessPlayerHintProjection PlayerHintProjection { get; }
    }

    internal static class EnemyReadinessCanonical
    {
        public static string Developer(DeveloperReadinessSnapshot value)
        {
            StringBuilder builder = new StringBuilder(16384);
            Field(builder, "schemaId", value.SchemaId);
            Field(
                builder,
                "schemaVersion",
                value.SchemaVersion.ToString(CultureInfo.InvariantCulture));
            Field(builder, "buildSnapshotId", value.BuildSnapshotId);
            Field(
                builder,
                "buildSnapshotCanonicalSignature",
                value.BuildSnapshotCanonicalSignature);
            Field(
                builder,
                "pressureCatalogCanonicalSignature",
                value.PressureCatalogCanonicalSignature);
            Field(
                builder,
                "buildPressureProfileId",
                value.BuildPressureProfileId);
            Field(
                builder,
                "readinessBand",
                ((int)value.ReadinessBand).ToString(CultureInfo.InvariantCulture));
            Rows(
                builder,
                "appliedMapRuleAdjustments",
                value.AppliedMapRuleAdjustments,
                Adjustment);
            Rows(
                builder,
                "requirementGroupEvaluations",
                value.RequirementGroupEvaluations,
                Group);
            Rows(
                builder,
                "requirementEvaluations",
                value.RequirementEvaluations,
                Requirement);
            return builder.ToString();
        }

        public static string PlayerSafe(EnemyReadinessPlayerHintProjection value)
        {
            StringBuilder builder = new StringBuilder(2048);
            Field(
                builder,
                "buildPressureProfileId",
                value.BuildPressureProfileId);
            Field(
                builder,
                "publicPressureLabelKey",
                value.PublicPressureLabelKey);
            Field(
                builder,
                "publicPressureHintKey",
                value.PublicPressureHintKey);
            Strings(
                builder,
                "playerHintCategoryKeys",
                value.PlayerHintCategoryKeys);
            Field(
                builder,
                "playerHintSeverity",
                ((int)value.PlayerHintSeverity).ToString(
                    CultureInfo.InvariantCulture));
            return builder.ToString();
        }

        private static string Adjustment(
            MapRuleCapabilityAdjustmentSnapshot value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "mapRuleId", value.MapRuleId);
            Field(builder, "buildCapabilityKey", value.BuildCapabilityKey);
            Field(
                builder,
                "deltaBasisPoints",
                value.DeltaBasisPoints.ToString(CultureInfo.InvariantCulture));
            Field(builder, "developerReasonId", value.DeveloperReasonId);
            return builder.ToString();
        }

        private static string Group(RequirementGroupEvaluationSnapshot value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "requirementGroupId", value.RequirementGroupId);
            Field(
                builder,
                "requirementRole",
                ((int)value.RequirementRole).ToString(CultureInfo.InvariantCulture));
            Field(
                builder,
                "requirementMatchMode",
                ((int)value.RequirementMatchMode).ToString(
                    CultureInfo.InvariantCulture));
            Field(
                builder,
                "groupStatus",
                ((int)value.GroupStatus).ToString(CultureInfo.InvariantCulture));
            Rows(
                builder,
                "requirementEvaluations",
                value.RequirementEvaluations,
                Requirement);
            return builder.ToString();
        }

        private static string Requirement(
            RequirementCapabilityEvaluationSnapshot value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "requirementGroupId", value.RequirementGroupId);
            Field(
                builder,
                "requirementRole",
                ((int)value.RequirementRole).ToString(CultureInfo.InvariantCulture));
            Field(
                builder,
                "requirementMatchMode",
                ((int)value.RequirementMatchMode).ToString(
                    CultureInfo.InvariantCulture));
            Field(builder, "buildCapabilityKey", value.BuildCapabilityKey);
            Field(
                builder,
                "capabilityValueAvailability",
                ((int)value.CapabilityValueAvailability).ToString(
                    CultureInfo.InvariantCulture));
            Field(
                builder,
                "baseValueBasisPoints",
                value.BaseValueBasisPoints.ToString(CultureInfo.InvariantCulture));
            Field(
                builder,
                "mapDeltaBasisPoints",
                value.MapDeltaBasisPoints.ToString(CultureInfo.InvariantCulture));
            Field(
                builder,
                "effectiveValueBasisPoints",
                value.EffectiveValueBasisPoints.ToString(
                    CultureInfo.InvariantCulture));
            Field(
                builder,
                "requiredValueBasisPoints",
                value.RequiredValueBasisPoints.ToString(
                    CultureInfo.InvariantCulture));
            Field(
                builder,
                "gapBasisPoints",
                value.GapBasisPoints.ToString(CultureInfo.InvariantCulture));
            Field(
                builder,
                "requirementStatus",
                ((int)value.RequirementStatus).ToString(
                    CultureInfo.InvariantCulture));
            Field(
                builder,
                "groupStatus",
                ((int)value.GroupStatus).ToString(CultureInfo.InvariantCulture));
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

        private static void Strings(
            StringBuilder builder,
            string name,
            IEnumerable<string> values)
        {
            Rows(
                builder,
                name,
                values ?? Array.Empty<string>(),
                value => value ?? string.Empty);
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
                    builder.Append(
                        value.ToString("x2", CultureInfo.InvariantCulture));
                }

                return builder.ToString();
            }
        }
    }
}
