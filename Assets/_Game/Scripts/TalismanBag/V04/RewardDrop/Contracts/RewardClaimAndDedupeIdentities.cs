using System.Collections.Generic;

namespace TalismanBag.V04.RewardDrop.Contracts
{
    public sealed class RewardClaimIdentity
    {
        public const string SchemaId = "RewardClaimIdentity.v1";

        public string schemaId { get; }
        public string claimId { get; }
        public string rewardResultId { get; }
        public string sourceFactId { get; }
        public string stageClearId { get; }
        public string dropRequestId { get; }
        public RewardAcquisitionMode acquisitionMode { get; }
        public string claimScopeId { get; }
        public RewardLaunchContextIdentity rewardLaunchContextIdentity { get; }
        public string canonicalPayload { get; }
        public string canonicalSignature { get; }

        public RewardClaimIdentity(
            string claimId,
            string rewardResultId,
            string sourceFactId,
            string stageClearId,
            string dropRequestId,
            RewardAcquisitionMode acquisitionMode,
            string claimScopeId,
            RewardLaunchContextIdentity rewardLaunchContextIdentity)
        {
            schemaId = SchemaId;
            this.claimId = RewardDropCanonical.NormalizeIdentifier(claimId);
            this.rewardResultId = RewardDropCanonical.NormalizeIdentifier(rewardResultId);
            this.sourceFactId = RewardDropCanonical.NormalizeIdentifier(sourceFactId);
            this.stageClearId = RewardDropCanonical.NormalizeIdentifier(stageClearId);
            this.dropRequestId = RewardDropCanonical.NormalizeIdentifier(dropRequestId);
            this.acquisitionMode = acquisitionMode;
            this.claimScopeId = RewardDropCanonical.NormalizeIdentifier(claimScopeId);
            this.rewardLaunchContextIdentity = rewardLaunchContextIdentity;
            canonicalPayload = RewardDropCanonical.CreatePayload(builder =>
            {
                builder.AddString("schemaId", schemaId);
                builder.AddString("claimId", this.claimId);
                builder.AddString("rewardResultId", this.rewardResultId);
                builder.AddString("sourceFactId", this.sourceFactId);
                builder.AddString("stageClearId", this.stageClearId);
                builder.AddString("dropRequestId", this.dropRequestId);
                builder.AddEnum("acquisitionMode", (int)this.acquisitionMode);
                builder.AddString("claimScopeId", this.claimScopeId);
                RewardDropCanonical.AddLaunchContext(
                    builder,
                    "rewardLaunchContextIdentity",
                    this.rewardLaunchContextIdentity);
            });
            canonicalSignature = RewardDropCanonical.ComputeSignature(canonicalPayload);
        }
    }

    public sealed class RewardDedupeIdentity
    {
        public const string SchemaId = "RewardDedupeIdentity.v1";

        public string schemaId { get; }
        public string dedupeKey { get; }
        public string sourceFactId { get; }
        public string stageClearId { get; }
        public string dropRequestId { get; }
        public string rewardResultId { get; }
        public RewardAcquisitionMode acquisitionMode { get; }
        public string claimScopeId { get; }
        public RewardLaunchContextIdentity rewardLaunchContextIdentity { get; }
        public string canonicalPayload { get; }
        public string canonicalSignature { get; }

        public RewardDedupeIdentity(
            string dedupeKey,
            string sourceFactId,
            string stageClearId,
            string dropRequestId,
            string rewardResultId,
            RewardAcquisitionMode acquisitionMode,
            string claimScopeId,
            RewardLaunchContextIdentity rewardLaunchContextIdentity)
        {
            schemaId = SchemaId;
            this.dedupeKey = RewardDropCanonical.NormalizeIdentifier(dedupeKey);
            this.sourceFactId = RewardDropCanonical.NormalizeIdentifier(sourceFactId);
            this.stageClearId = RewardDropCanonical.NormalizeIdentifier(stageClearId);
            this.dropRequestId = RewardDropCanonical.NormalizeIdentifier(dropRequestId);
            this.rewardResultId = RewardDropCanonical.NormalizeIdentifier(rewardResultId);
            this.acquisitionMode = acquisitionMode;
            this.claimScopeId = RewardDropCanonical.NormalizeIdentifier(claimScopeId);
            this.rewardLaunchContextIdentity = rewardLaunchContextIdentity;
            canonicalPayload = RewardDropCanonical.CreatePayload(builder =>
            {
                builder.AddString("schemaId", schemaId);
                builder.AddString("dedupeKey", this.dedupeKey);
                builder.AddString("sourceFactId", this.sourceFactId);
                builder.AddString("stageClearId", this.stageClearId);
                builder.AddString("dropRequestId", this.dropRequestId);
                builder.AddString("rewardResultId", this.rewardResultId);
                builder.AddEnum("acquisitionMode", (int)this.acquisitionMode);
                builder.AddString("claimScopeId", this.claimScopeId);
                RewardDropCanonical.AddLaunchContext(
                    builder,
                    "rewardLaunchContextIdentity",
                    this.rewardLaunchContextIdentity);
            });
            canonicalSignature = RewardDropCanonical.ComputeSignature(canonicalPayload);
        }
    }

    public sealed class RewardExclusionIdentity
    {
        public const string SchemaId = "RewardExclusionIdentity.v1";

        public string schemaId { get; }
        public string exclusionPolicyId { get; }
        public RewardSubjectKind subjectKind { get; }
        public IReadOnlyList<RewardAcquisitionMode> prohibitedAcquisitionModes { get; }
        public string reasonCode { get; }
        public int policyVersion { get; }
        public string canonicalPayload { get; }
        public string canonicalSignature { get; }

        public RewardExclusionIdentity(
            string exclusionPolicyId,
            RewardSubjectKind subjectKind,
            IEnumerable<RewardAcquisitionMode> prohibitedAcquisitionModes,
            string reasonCode,
            int policyVersion)
        {
            schemaId = SchemaId;
            this.exclusionPolicyId =
                RewardDropCanonical.NormalizeIdentifier(exclusionPolicyId);
            this.subjectKind = subjectKind;
            this.prohibitedAcquisitionModes =
                RewardDropCanonical.NormalizeModeSet(prohibitedAcquisitionModes);
            this.reasonCode = RewardDropCanonical.NormalizeIdentifier(reasonCode);
            this.policyVersion = policyVersion;
            canonicalPayload = RewardDropCanonical.CreatePayload(builder =>
            {
                builder.AddString("schemaId", schemaId);
                builder.AddString("exclusionPolicyId", this.exclusionPolicyId);
                builder.AddEnum("subjectKind", (int)this.subjectKind);
                builder.AddInt(
                    "prohibitedAcquisitionModes.count",
                    this.prohibitedAcquisitionModes.Count);
                for (int index = 0; index < this.prohibitedAcquisitionModes.Count; index++)
                {
                    string prefix = "prohibitedAcquisitionModes[" + index + "]";
                    builder.AddInt(prefix + ".position", index);
                    builder.AddEnum(
                        prefix + ".value",
                        (int)this.prohibitedAcquisitionModes[index]);
                }

                builder.AddString("reasonCode", this.reasonCode);
                builder.AddInt("policyVersion", this.policyVersion);
            });
            canonicalSignature = RewardDropCanonical.ComputeSignature(canonicalPayload);
        }
    }
}
