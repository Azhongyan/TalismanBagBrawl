using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.AuthoredPressure;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.DevAuthoring;
using TalismanBag.EnemySystem.RequirementChannel;

namespace TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.RequirementMigrationOverlay
{
    public static class LayoutResilienceRequirementChannelMigrationSchema
    {
        public const string SchemaId = "LayoutResilienceRequirementChannelMigration.v1";
        public const int SchemaVersion = 1;
    }

    public enum LayoutResilienceRequirementChannelMigrationStatus
    {
        Complete = 1,
        Unknown = 2,
        Invalid = 3
    }

    public sealed class LayoutResilienceRequirementChannelMigrationSource
    {
        private readonly ReadOnlyCollection<
            LayoutResilienceRequirementChannelMigrationSourceRow> rows;

        public LayoutResilienceRequirementChannelMigrationSource(
            string schemaId,
            int schemaVersion,
            bool devOnly,
            bool isEnabled,
            bool coordinateBaselineAccepted,
            bool activated,
            IEnumerable<LayoutResilienceRequirementChannelMigrationSourceRow> rows)
        {
            SchemaId = schemaId;
            SchemaVersion = schemaVersion;
            DevOnly = devOnly;
            IsEnabled = isEnabled;
            CoordinateBaselineAccepted = coordinateBaselineAccepted;
            Activated = activated;
            this.rows = rows == null
                ? null
                : Array.AsReadOnly(rows.Select(value =>
                    value == null ? null : value.Clone()).ToArray());
        }

        public string SchemaId { get; }
        public int SchemaVersion { get; }
        public bool DevOnly { get; }
        public bool IsEnabled { get; }
        public bool CoordinateBaselineAccepted { get; }
        public bool Activated { get; }
        public IReadOnlyList<LayoutResilienceRequirementChannelMigrationSourceRow>
            Rows => rows;
    }

    public sealed class LayoutResilienceRequirementChannelMigrationSourceRow
    {
        public LayoutResilienceRequirementChannelMigrationSourceRow(
            string migrationRouteId,
            string candidateMigrationRequirementId,
            string ownerId,
            string requirementGroupId,
            string role,
            string matchMode,
            string referenceKind,
            string buildCapabilityKey,
            int legacyMinimumCapabilityBasisPoints,
            string legacyEvidencePath,
            string legacyEvidenceRowIdentity,
            bool legacyBpQuarantined,
            bool legacyBpProvenanceVerified,
            bool legacyBpEvaluated,
            bool legacyBpConverted,
            bool legacyBpComparedForCapabilityDecision,
            bool legacyBpUsedAsThreshold,
            string seedId,
            string encounterId,
            string mapRuleId,
            string pressureInputId,
            EnemyRequirementChannel declaredChannel,
            EnemyRequirementChannel evaluationChannel,
            EnemyRequirementApplicabilityState applicabilityState,
            bool devOnly,
            bool isEnabled,
            bool coordinateBaselineAccepted,
            bool activated,
            bool formalRequirementSourceModified,
            bool behaviorChanged)
        {
            MigrationRouteId = migrationRouteId;
            CandidateMigrationRequirementId = candidateMigrationRequirementId;
            OwnerId = ownerId;
            RequirementGroupId = requirementGroupId;
            Role = role;
            MatchMode = matchMode;
            ReferenceKind = referenceKind;
            BuildCapabilityKey = buildCapabilityKey;
            LegacyMinimumCapabilityBasisPoints = legacyMinimumCapabilityBasisPoints;
            LegacyEvidencePath = legacyEvidencePath;
            LegacyEvidenceRowIdentity = legacyEvidenceRowIdentity;
            LegacyBpQuarantined = legacyBpQuarantined;
            LegacyBpProvenanceVerified = legacyBpProvenanceVerified;
            LegacyBpEvaluated = legacyBpEvaluated;
            LegacyBpConverted = legacyBpConverted;
            LegacyBpComparedForCapabilityDecision =
                legacyBpComparedForCapabilityDecision;
            LegacyBpUsedAsThreshold = legacyBpUsedAsThreshold;
            SeedId = seedId;
            EncounterId = encounterId;
            MapRuleId = mapRuleId;
            PressureInputId = pressureInputId;
            DeclaredChannel = declaredChannel;
            EvaluationChannel = evaluationChannel;
            ApplicabilityState = applicabilityState;
            DevOnly = devOnly;
            IsEnabled = isEnabled;
            CoordinateBaselineAccepted = coordinateBaselineAccepted;
            Activated = activated;
            FormalRequirementSourceModified = formalRequirementSourceModified;
            BehaviorChanged = behaviorChanged;
        }

        public string MigrationRouteId { get; }
        public string CandidateMigrationRequirementId { get; }
        public string OwnerId { get; }
        public string RequirementGroupId { get; }
        public string Role { get; }
        public string MatchMode { get; }
        public string ReferenceKind { get; }
        public string BuildCapabilityKey { get; }
        public int LegacyMinimumCapabilityBasisPoints { get; }
        public string LegacyEvidencePath { get; }
        public string LegacyEvidenceRowIdentity { get; }
        public bool LegacyBpQuarantined { get; }
        public bool LegacyBpProvenanceVerified { get; }
        public bool LegacyBpEvaluated { get; }
        public bool LegacyBpConverted { get; }
        public bool LegacyBpComparedForCapabilityDecision { get; }
        public bool LegacyBpUsedAsThreshold { get; }
        public string SeedId { get; }
        public string EncounterId { get; }
        public string MapRuleId { get; }
        public string PressureInputId { get; }
        public EnemyRequirementChannel DeclaredChannel { get; }
        public EnemyRequirementChannel EvaluationChannel { get; }
        public EnemyRequirementApplicabilityState ApplicabilityState { get; }
        public bool DevOnly { get; }
        public bool IsEnabled { get; }
        public bool CoordinateBaselineAccepted { get; }
        public bool Activated { get; }
        public bool FormalRequirementSourceModified { get; }
        public bool BehaviorChanged { get; }

        internal LayoutResilienceRequirementChannelMigrationSourceRow Clone()
        {
            return new LayoutResilienceRequirementChannelMigrationSourceRow(
                MigrationRouteId, CandidateMigrationRequirementId, OwnerId,
                RequirementGroupId, Role, MatchMode, ReferenceKind,
                BuildCapabilityKey, LegacyMinimumCapabilityBasisPoints,
                LegacyEvidencePath, LegacyEvidenceRowIdentity,
                LegacyBpQuarantined, LegacyBpProvenanceVerified,
                LegacyBpEvaluated, LegacyBpConverted,
                LegacyBpComparedForCapabilityDecision, LegacyBpUsedAsThreshold,
                SeedId, EncounterId, MapRuleId, PressureInputId, DeclaredChannel,
                EvaluationChannel, ApplicabilityState, DevOnly, IsEnabled,
                CoordinateBaselineAccepted, Activated,
                FormalRequirementSourceModified, BehaviorChanged);
        }
    }

    public sealed class LayoutResilienceRequirementChannelMigrationRowSnapshot
    {
        private readonly ReadOnlyCollection<LayoutPressureKind> pressureKinds;
        private readonly ReadOnlyCollection<LayoutResiliencePredicateClauseKind>
            requiredPredicateClauses;

        internal LayoutResilienceRequirementChannelMigrationRowSnapshot(
            LayoutResilienceRequirementChannelMigrationSourceRow source,
            DevEncounterLayoutPressureAuthoringRowSnapshot authoring,
            EnemyRequirementChannelApplicabilityRowSnapshot channelRow)
        {
            MigrationRouteId = source.MigrationRouteId;
            CandidateMigrationRequirementId = source.CandidateMigrationRequirementId;
            OwnerId = source.OwnerId;
            RequirementGroupId = source.RequirementGroupId;
            Role = source.Role;
            MatchMode = source.MatchMode;
            ReferenceKind = source.ReferenceKind;
            BuildCapabilityKey = source.BuildCapabilityKey;
            LegacyMinimumCapabilityBasisPoints =
                source.LegacyMinimumCapabilityBasisPoints;
            LegacyEvidencePath = source.LegacyEvidencePath;
            LegacyEvidenceRowIdentity = source.LegacyEvidenceRowIdentity;
            LegacyBpQuarantined = source.LegacyBpQuarantined;
            LegacyBpProvenanceVerified = source.LegacyBpProvenanceVerified;
            LegacyBpEvaluated = source.LegacyBpEvaluated;
            LegacyBpConverted = source.LegacyBpConverted;
            LegacyBpComparedForCapabilityDecision =
                source.LegacyBpComparedForCapabilityDecision;
            LegacyBpUsedAsThreshold = source.LegacyBpUsedAsThreshold;
            SeedId = source.SeedId;
            EncounterId = source.EncounterId;
            MapRuleId = source.MapRuleId;
            PressureInputId = source.PressureInputId;
            DeclaredChannel = source.DeclaredChannel;
            EvaluationChannel = source.EvaluationChannel;
            ApplicabilityState = source.ApplicabilityState;
            DevOnly = source.DevOnly;
            IsEnabled = source.IsEnabled;
            CoordinateBaselineAccepted = source.CoordinateBaselineAccepted;
            Activated = source.Activated;
            FormalRequirementSourceModified =
                source.FormalRequirementSourceModified;
            BehaviorChanged = source.BehaviorChanged;
            AuthoringStatus = DevEncounterLayoutPressureAuthoringStatus
                .CandidateComplete;
            P2Status = authoring.P2Status;
            P2Completeness = authoring.P2Completeness;
            pressureKinds = Array.AsReadOnly(authoring.PressureSnapshot.PressureKinds
                .OrderBy(value => (int)value).ToArray());
            EffectiveEyeCell = authoring.PressureSnapshot
                .EffectiveEyeAnchorCellAfterPressure == null
                ? null
                : new LayoutCellCoordinate(
                    authoring.PressureSnapshot.EffectiveEyeAnchorCellAfterPressure.X,
                    authoring.PressureSnapshot.EffectiveEyeAnchorCellAfterPressure.Y);
            requiredPredicateClauses = Array.AsReadOnly(authoring.PressureSnapshot
                .RequiredPredicateClauses.OrderBy(value => (int)value).ToArray());
            P2CanonicalSignature = authoring.P2CanonicalSignature;
            ChannelApplicabilityRow = channelRow == null
                ? null
                : new EnemyRequirementChannelApplicabilityRowSnapshot(
                    channelRow.RequirementId, channelRow.DeclaredChannel,
                    channelRow.ApplicabilityState);
        }

        private LayoutResilienceRequirementChannelMigrationRowSnapshot(
            LayoutResilienceRequirementChannelMigrationRowSnapshot value)
        {
            MigrationRouteId = value.MigrationRouteId;
            CandidateMigrationRequirementId = value.CandidateMigrationRequirementId;
            OwnerId = value.OwnerId;
            RequirementGroupId = value.RequirementGroupId;
            Role = value.Role;
            MatchMode = value.MatchMode;
            ReferenceKind = value.ReferenceKind;
            BuildCapabilityKey = value.BuildCapabilityKey;
            LegacyMinimumCapabilityBasisPoints =
                value.LegacyMinimumCapabilityBasisPoints;
            LegacyEvidencePath = value.LegacyEvidencePath;
            LegacyEvidenceRowIdentity = value.LegacyEvidenceRowIdentity;
            LegacyBpQuarantined = value.LegacyBpQuarantined;
            LegacyBpProvenanceVerified = value.LegacyBpProvenanceVerified;
            LegacyBpEvaluated = value.LegacyBpEvaluated;
            LegacyBpConverted = value.LegacyBpConverted;
            LegacyBpComparedForCapabilityDecision =
                value.LegacyBpComparedForCapabilityDecision;
            LegacyBpUsedAsThreshold = value.LegacyBpUsedAsThreshold;
            SeedId = value.SeedId;
            EncounterId = value.EncounterId;
            MapRuleId = value.MapRuleId;
            PressureInputId = value.PressureInputId;
            DeclaredChannel = value.DeclaredChannel;
            EvaluationChannel = value.EvaluationChannel;
            ApplicabilityState = value.ApplicabilityState;
            DevOnly = value.DevOnly;
            IsEnabled = value.IsEnabled;
            CoordinateBaselineAccepted = value.CoordinateBaselineAccepted;
            Activated = value.Activated;
            FormalRequirementSourceModified = value.FormalRequirementSourceModified;
            BehaviorChanged = value.BehaviorChanged;
            AuthoringStatus = value.AuthoringStatus;
            P2Status = value.P2Status;
            P2Completeness = value.P2Completeness;
            pressureKinds = Array.AsReadOnly(value.PressureKinds.ToArray());
            EffectiveEyeCell = value.EffectiveEyeCell == null
                ? null
                : new LayoutCellCoordinate(
                    value.EffectiveEyeCell.X, value.EffectiveEyeCell.Y);
            requiredPredicateClauses = Array.AsReadOnly(
                value.RequiredPredicateClauses.ToArray());
            P2CanonicalSignature = value.P2CanonicalSignature;
            ChannelApplicabilityRow = value.ChannelApplicabilityRow == null
                ? null
                : new EnemyRequirementChannelApplicabilityRowSnapshot(
                    value.ChannelApplicabilityRow.RequirementId,
                    value.ChannelApplicabilityRow.DeclaredChannel,
                    value.ChannelApplicabilityRow.ApplicabilityState);
        }

        public string MigrationRouteId { get; }
        public string CandidateMigrationRequirementId { get; }
        public string OwnerId { get; }
        public string RequirementGroupId { get; }
        public string Role { get; }
        public string MatchMode { get; }
        public string ReferenceKind { get; }
        public string BuildCapabilityKey { get; }
        public int LegacyMinimumCapabilityBasisPoints { get; }
        public string LegacyEvidencePath { get; }
        public string LegacyEvidenceRowIdentity { get; }
        public bool LegacyBpQuarantined { get; }
        public bool LegacyBpProvenanceVerified { get; }
        public bool LegacyBpEvaluated { get; }
        public bool LegacyBpConverted { get; }
        public bool LegacyBpComparedForCapabilityDecision { get; }
        public bool LegacyBpUsedAsThreshold { get; }
        public string SeedId { get; }
        public string EncounterId { get; }
        public string MapRuleId { get; }
        public string PressureInputId { get; }
        public EnemyRequirementChannel DeclaredChannel { get; }
        public EnemyRequirementChannel EvaluationChannel { get; }
        public EnemyRequirementApplicabilityState ApplicabilityState { get; }
        public bool DevOnly { get; }
        public bool IsEnabled { get; }
        public bool CoordinateBaselineAccepted { get; }
        public bool Activated { get; }
        public bool FormalRequirementSourceModified { get; }
        public bool BehaviorChanged { get; }
        public DevEncounterLayoutPressureAuthoringStatus AuthoringStatus { get; }
        public AuthoredLayoutPressureSourceStatus P2Status { get; }
        public LayoutResilienceInputCompleteness P2Completeness { get; }
        public IReadOnlyList<LayoutPressureKind> PressureKinds => pressureKinds;
        public LayoutCellCoordinate EffectiveEyeCell { get; }
        public IReadOnlyList<LayoutResiliencePredicateClauseKind>
            RequiredPredicateClauses => requiredPredicateClauses;
        public string P2CanonicalSignature { get; }
        public EnemyRequirementChannelApplicabilityRowSnapshot
            ChannelApplicabilityRow { get; }

        internal LayoutResilienceRequirementChannelMigrationRowSnapshot Clone()
        {
            return new LayoutResilienceRequirementChannelMigrationRowSnapshot(this);
        }
    }

    public sealed class LayoutResilienceRequirementChannelMigrationIssue
    {
        public LayoutResilienceRequirementChannelMigrationIssue(
            string code,
            LayoutResilienceRequirementChannelMigrationStatus status,
            string path,
            string message)
        {
            Code = code ?? string.Empty;
            Status = status;
            Path = path ?? string.Empty;
            Message = message ?? string.Empty;
        }

        public string Code { get; }
        public LayoutResilienceRequirementChannelMigrationStatus Status { get; }
        public string Path { get; }
        public string Message { get; }
    }

    public sealed class LayoutResilienceRequirementChannelMigrationPayload
    {
        private readonly ReadOnlyCollection<
            LayoutResilienceRequirementChannelMigrationRowSnapshot> rows;

        internal LayoutResilienceRequirementChannelMigrationPayload(
            IEnumerable<LayoutResilienceRequirementChannelMigrationRowSnapshot> rows,
            EnemyRequirementChannelApplicabilitySnapshot channelApplicabilitySnapshot,
            string p5ACanonicalSignature,
            string n01BCanonicalSignature,
            bool formalRequirementSourceModified,
            bool behaviorChanged)
        {
            this.rows = Array.AsReadOnly((rows ?? Array.Empty<
                    LayoutResilienceRequirementChannelMigrationRowSnapshot>())
                .Select(value => value == null ? null : value.Clone())
                .OrderBy(value => value == null ? string.Empty : value.MigrationRouteId,
                    StringComparer.Ordinal).ToArray());
            ChannelApplicabilitySnapshot = channelApplicabilitySnapshot;
            P5ACanonicalSignature = p5ACanonicalSignature ?? string.Empty;
            N01BCanonicalSignature = n01BCanonicalSignature ?? string.Empty;
            FormalRequirementSourceModified = formalRequirementSourceModified;
            BehaviorChanged = behaviorChanged;
        }

        public IReadOnlyList<LayoutResilienceRequirementChannelMigrationRowSnapshot>
            Rows => rows;
        public EnemyRequirementChannelApplicabilitySnapshot
            ChannelApplicabilitySnapshot { get; }
        public string P5ACanonicalSignature { get; }
        public string N01BCanonicalSignature { get; }
        public bool FormalRequirementSourceModified { get; }
        public bool BehaviorChanged { get; }
    }

    public sealed class LayoutResilienceRequirementChannelMigrationResult
    {
        private readonly ReadOnlyCollection<
            LayoutResilienceRequirementChannelMigrationIssue> issues;

        internal LayoutResilienceRequirementChannelMigrationResult(
            LayoutResilienceRequirementChannelMigrationStatus status,
            bool devOnly,
            bool isEnabled,
            bool coordinateBaselineAccepted,
            bool activated,
            LayoutResilienceRequirementChannelMigrationPayload payload,
            IEnumerable<LayoutResilienceRequirementChannelMigrationIssue> issues,
            string canonicalSignature)
        {
            SchemaId = LayoutResilienceRequirementChannelMigrationSchema.SchemaId;
            SchemaVersion = LayoutResilienceRequirementChannelMigrationSchema
                .SchemaVersion;
            Status = status;
            DevOnly = devOnly;
            IsEnabled = isEnabled;
            CoordinateBaselineAccepted = coordinateBaselineAccepted;
            Activated = activated;
            Payload = payload;
            this.issues = Array.AsReadOnly((issues ?? Array.Empty<
                    LayoutResilienceRequirementChannelMigrationIssue>())
                .Select(value => new LayoutResilienceRequirementChannelMigrationIssue(
                    value.Code, value.Status, value.Path, value.Message))
                .OrderBy(value => value.Path, StringComparer.Ordinal)
                .ThenBy(value => value.Code, StringComparer.Ordinal)
                .ThenBy(value => (int)value.Status)
                .ThenBy(value => value.Message, StringComparer.Ordinal).ToArray());
            CanonicalSignature = canonicalSignature ?? string.Empty;
        }

        public string SchemaId { get; }
        public int SchemaVersion { get; }
        public LayoutResilienceRequirementChannelMigrationStatus Status { get; }
        public bool DevOnly { get; }
        public bool IsEnabled { get; }
        public bool CoordinateBaselineAccepted { get; }
        public bool Activated { get; }
        public LayoutResilienceRequirementChannelMigrationPayload Payload { get; }
        public IReadOnlyList<LayoutResilienceRequirementChannelMigrationIssue>
            Issues => issues;
        public string CanonicalSignature { get; }
    }

    internal static class LayoutResilienceRequirementChannelMigrationCanonical
    {
        public static string Create(
            LayoutResilienceRequirementChannelMigrationSource source,
            LayoutResilienceRequirementChannelMigrationStatus status,
            bool devOnly,
            bool isEnabled,
            bool coordinateBaselineAccepted,
            bool activated,
            LayoutResilienceRequirementChannelMigrationPayload payload,
            IEnumerable<LayoutResilienceRequirementChannelMigrationIssue> issues)
        {
            StringBuilder builder = new StringBuilder(32768);
            Field(builder, "schema.id",
                LayoutResilienceRequirementChannelMigrationSchema.SchemaId);
            Field(builder, "schema.version", Int(
                LayoutResilienceRequirementChannelMigrationSchema.SchemaVersion));
            Field(builder, "source.present", Bool(source != null));
            if (source != null)
            {
                Text(builder, "source.schemaId", source.SchemaId);
                Field(builder, "source.schemaVersion", Int(source.SchemaVersion));
                Field(builder, "source.devOnly", Bool(source.DevOnly));
                Field(builder, "source.isEnabled", Bool(source.IsEnabled));
                Field(builder, "source.coordinateBaselineAccepted",
                    Bool(source.CoordinateBaselineAccepted));
                Field(builder, "source.activated", Bool(source.Activated));
                SourceRows(builder, source.Rows);
            }
            Field(builder, "result.status", Int((int)status));
            Field(builder, "result.devOnly", Bool(devOnly));
            Field(builder, "result.isEnabled", Bool(isEnabled));
            Field(builder, "result.coordinateBaselineAccepted",
                Bool(coordinateBaselineAccepted));
            Field(builder, "result.activated", Bool(activated));
            Payload(builder, payload);
            Issues(builder, issues);
            using (SHA256 sha = SHA256.Create())
            {
                return "sha256:" + string.Concat(sha.ComputeHash(
                        Encoding.UTF8.GetBytes(builder.ToString()))
                    .Select(value => value.ToString("x2", CultureInfo.InvariantCulture)));
            }
        }

        private static void SourceRows(StringBuilder builder, IReadOnlyList<
            LayoutResilienceRequirementChannelMigrationSourceRow> values)
        {
            Field(builder, "source.rows.present", Bool(values != null));
            if (values == null) return;
            LayoutResilienceRequirementChannelMigrationSourceRow[] rows = values
                .OrderBy(value => value == null ? string.Empty : value.MigrationRouteId,
                    StringComparer.Ordinal).ToArray();
            Field(builder, "source.rows.count", Int(rows.Length));
            for (int index = 0; index < rows.Length; index++)
            {
                string prefix = "source.rows[" + Int(index) + "]";
                Field(builder, prefix + ".present", Bool(rows[index] != null));
                if (rows[index] != null) SourceRow(builder, prefix, rows[index]);
            }
        }

        private static void SourceRow(StringBuilder builder, string prefix,
            LayoutResilienceRequirementChannelMigrationSourceRow value)
        {
            Text(builder, prefix + ".route", value.MigrationRouteId);
            Text(builder, prefix + ".candidate", value.CandidateMigrationRequirementId);
            Text(builder, prefix + ".owner", value.OwnerId);
            Text(builder, prefix + ".group", value.RequirementGroupId);
            Text(builder, prefix + ".role", value.Role);
            Text(builder, prefix + ".match", value.MatchMode);
            Text(builder, prefix + ".reference", value.ReferenceKind);
            Text(builder, prefix + ".key", value.BuildCapabilityKey);
            Field(builder, prefix + ".legacyBp",
                Int(value.LegacyMinimumCapabilityBasisPoints));
            Text(builder, prefix + ".legacyPath", value.LegacyEvidencePath);
            Text(builder, prefix + ".legacyRow", value.LegacyEvidenceRowIdentity);
            Field(builder, prefix + ".legacyQuarantined",
                Bool(value.LegacyBpQuarantined));
            Field(builder, prefix + ".legacyVerified",
                Bool(value.LegacyBpProvenanceVerified));
            Field(builder, prefix + ".legacyEvaluated",
                Bool(value.LegacyBpEvaluated));
            Field(builder, prefix + ".legacyConverted",
                Bool(value.LegacyBpConverted));
            Field(builder, prefix + ".legacyCompared",
                Bool(value.LegacyBpComparedForCapabilityDecision));
            Field(builder, prefix + ".legacyThreshold",
                Bool(value.LegacyBpUsedAsThreshold));
            Text(builder, prefix + ".seed", value.SeedId);
            Text(builder, prefix + ".encounter", value.EncounterId);
            Text(builder, prefix + ".map", value.MapRuleId);
            Text(builder, prefix + ".pressure", value.PressureInputId);
            Field(builder, prefix + ".declaredChannel", Int((int)value.DeclaredChannel));
            Field(builder, prefix + ".evaluationChannel",
                Int((int)value.EvaluationChannel));
            Field(builder, prefix + ".applicability",
                Int((int)value.ApplicabilityState));
            Field(builder, prefix + ".devOnly", Bool(value.DevOnly));
            Field(builder, prefix + ".isEnabled", Bool(value.IsEnabled));
            Field(builder, prefix + ".coordinateBaselineAccepted",
                Bool(value.CoordinateBaselineAccepted));
            Field(builder, prefix + ".activated", Bool(value.Activated));
            Field(builder, prefix + ".formalModified",
                Bool(value.FormalRequirementSourceModified));
            Field(builder, prefix + ".behaviorChanged", Bool(value.BehaviorChanged));
        }

        private static void Payload(StringBuilder builder,
            LayoutResilienceRequirementChannelMigrationPayload value)
        {
            Field(builder, "result.payload.present", Bool(value != null));
            if (value == null) return;
            Field(builder, "result.payload.formalModified",
                Bool(value.FormalRequirementSourceModified));
            Field(builder, "result.payload.behaviorChanged", Bool(value.BehaviorChanged));
            Text(builder, "result.payload.p5aCanonical", value.P5ACanonicalSignature);
            Text(builder, "result.payload.n01bCanonical", value.N01BCanonicalSignature);
            Snapshot(builder, value.ChannelApplicabilitySnapshot);
            LayoutResilienceRequirementChannelMigrationRowSnapshot[] rows = value.Rows
                .OrderBy(row => row.MigrationRouteId, StringComparer.Ordinal).ToArray();
            Field(builder, "result.payload.rows.count", Int(rows.Length));
            for (int index = 0; index < rows.Length; index++)
                ResultRow(builder, "result.payload.rows[" + Int(index) + "]", rows[index]);
        }

        private static void ResultRow(StringBuilder builder, string prefix,
            LayoutResilienceRequirementChannelMigrationRowSnapshot value)
        {
            SourceRow(builder, prefix, new LayoutResilienceRequirementChannelMigrationSourceRow(
                value.MigrationRouteId, value.CandidateMigrationRequirementId,
                value.OwnerId, value.RequirementGroupId, value.Role, value.MatchMode,
                value.ReferenceKind, value.BuildCapabilityKey,
                value.LegacyMinimumCapabilityBasisPoints, value.LegacyEvidencePath,
                value.LegacyEvidenceRowIdentity, value.LegacyBpQuarantined,
                value.LegacyBpProvenanceVerified, value.LegacyBpEvaluated,
                value.LegacyBpConverted, value.LegacyBpComparedForCapabilityDecision,
                value.LegacyBpUsedAsThreshold, value.SeedId, value.EncounterId,
                value.MapRuleId, value.PressureInputId, value.DeclaredChannel,
                value.EvaluationChannel, value.ApplicabilityState, value.DevOnly,
                value.IsEnabled, value.CoordinateBaselineAccepted, value.Activated,
                value.FormalRequirementSourceModified, value.BehaviorChanged));
            Field(builder, prefix + ".authoringStatus", Int((int)value.AuthoringStatus));
            Field(builder, prefix + ".p2Status", Int((int)value.P2Status));
            Field(builder, prefix + ".p2Completeness", Int((int)value.P2Completeness));
            Enums(builder, prefix + ".pressureKinds", value.PressureKinds,
                item => (int)item);
            Cell(builder, prefix + ".eye", value.EffectiveEyeCell);
            Enums(builder, prefix + ".clauses", value.RequiredPredicateClauses,
                item => (int)item);
            Text(builder, prefix + ".p2Canonical", value.P2CanonicalSignature);
            ChannelRow(builder, prefix + ".channelRow", value.ChannelApplicabilityRow);
        }

        private static void Snapshot(StringBuilder builder,
            EnemyRequirementChannelApplicabilitySnapshot value)
        {
            Field(builder, "result.payload.n01b.present", Bool(value != null));
            if (value == null) return;
            Text(builder, "result.payload.n01b.schemaId", value.SchemaId);
            Field(builder, "result.payload.n01b.schemaVersion", Int(value.SchemaVersion));
            Text(builder, "result.payload.n01b.snapshotId", value.SnapshotId);
            Field(builder, "result.payload.n01b.evaluationChannel",
                Int((int)value.EvaluationChannel));
            Text(builder, "result.payload.n01b.canonical", value.CanonicalSignature);
            EnemyRequirementChannelApplicabilityRowSnapshot[] rows = value.Rows
                .OrderBy(row => row.RequirementId, StringComparer.Ordinal).ToArray();
            Field(builder, "result.payload.n01b.rows.count", Int(rows.Length));
            for (int index = 0; index < rows.Length; index++)
                ChannelRow(builder, "result.payload.n01b.rows[" + Int(index) + "]",
                    rows[index]);
        }

        private static void ChannelRow(StringBuilder builder, string prefix,
            EnemyRequirementChannelApplicabilityRowSnapshot value)
        {
            Field(builder, prefix + ".present", Bool(value != null));
            if (value == null) return;
            Text(builder, prefix + ".requirementId", value.RequirementId);
            Field(builder, prefix + ".declaredChannel", Int((int)value.DeclaredChannel));
            Field(builder, prefix + ".applicability",
                Int((int)value.ApplicabilityState));
        }

        private static void Issues(StringBuilder builder, IEnumerable<
            LayoutResilienceRequirementChannelMigrationIssue> values)
        {
            LayoutResilienceRequirementChannelMigrationIssue[] rows = (values ??
                    Array.Empty<LayoutResilienceRequirementChannelMigrationIssue>())
                .OrderBy(value => value.Path, StringComparer.Ordinal)
                .ThenBy(value => value.Code, StringComparer.Ordinal)
                .ThenBy(value => (int)value.Status)
                .ThenBy(value => value.Message, StringComparer.Ordinal).ToArray();
            Field(builder, "result.issues.count", Int(rows.Length));
            for (int index = 0; index < rows.Length; index++)
            {
                string prefix = "result.issues[" + Int(index) + "]";
                Text(builder, prefix + ".path", rows[index].Path);
                Text(builder, prefix + ".code", rows[index].Code);
                Field(builder, prefix + ".status", Int((int)rows[index].Status));
                Text(builder, prefix + ".message", rows[index].Message);
            }
        }

        private static void Enums<T>(StringBuilder builder, string prefix,
            IReadOnlyList<T> values, Func<T, int> number)
        {
            Field(builder, prefix + ".present", Bool(values != null));
            if (values == null) return;
            T[] ordered = values.OrderBy(number).ToArray();
            Field(builder, prefix + ".count", Int(ordered.Length));
            for (int index = 0; index < ordered.Length; index++)
                Field(builder, prefix + "[" + Int(index) + "]",
                    Int(number(ordered[index])));
        }

        private static void Cell(StringBuilder builder, string prefix,
            LayoutCellCoordinate value)
        {
            Field(builder, prefix + ".present", Bool(value != null));
            if (value == null) return;
            Field(builder, prefix + ".x", Int(value.X));
            Field(builder, prefix + ".y", Int(value.Y));
        }

        private static void Text(StringBuilder builder, string name, string value)
        {
            Field(builder, name + ".present", Bool(value != null));
            Field(builder, name, value ?? string.Empty);
        }

        private static void Field(StringBuilder builder, string name, string value)
        {
            string text = value ?? string.Empty;
            builder.Append(name).Append('=').Append(Int(text.Length)).Append(':')
                .Append(text).Append('\n');
        }

        private static string Int(int value) =>
            value.ToString(CultureInfo.InvariantCulture);

        private static string Bool(bool value) => value ? "true" : "false";
    }
}
