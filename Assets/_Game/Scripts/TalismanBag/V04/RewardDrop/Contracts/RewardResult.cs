using System.Collections.Generic;

namespace TalismanBag.V04.RewardDrop.Contracts
{
    public sealed class RewardResult
    {
        public const string SchemaId = "RewardResult.v1";

        public string schemaId { get; }
        public string rewardResultId { get; }
        public string sourceFactId { get; }
        public string stageClearId { get; }
        public string dropRequestId { get; }
        public string dropRollResultId { get; }
        public string chapterId { get; }
        public string stageId { get; }
        public RewardLaunchContextIdentity rewardLaunchContextIdentity { get; }
        public RewardAcquisitionMode acquisitionMode { get; }
        public IReadOnlyList<RewardEntry> rewardEntries { get; }
        public string canonicalPayload { get; }
        public string canonicalSignature { get; }

        public RewardResult(
            string rewardResultId,
            string sourceFactId,
            string stageClearId,
            string dropRequestId,
            string dropRollResultId,
            string chapterId,
            string stageId,
            RewardLaunchContextIdentity rewardLaunchContextIdentity,
            RewardAcquisitionMode acquisitionMode,
            IEnumerable<RewardEntry> rewardEntries)
        {
            schemaId = SchemaId;
            this.rewardResultId = RewardDropCanonical.NormalizeIdentifier(rewardResultId);
            this.sourceFactId = RewardDropCanonical.NormalizeIdentifier(sourceFactId);
            this.stageClearId = RewardDropCanonical.NormalizeIdentifier(stageClearId);
            this.dropRequestId = RewardDropCanonical.NormalizeIdentifier(dropRequestId);
            this.dropRollResultId =
                RewardDropCanonical.NormalizeIdentifier(dropRollResultId);
            this.chapterId = RewardDropCanonical.NormalizeIdentifier(chapterId);
            this.stageId = RewardDropCanonical.NormalizeIdentifier(stageId);
            this.rewardLaunchContextIdentity = rewardLaunchContextIdentity;
            this.acquisitionMode = acquisitionMode;
            this.rewardEntries = RewardDropCanonical.CopyReadOnly(rewardEntries);
            canonicalPayload = RewardDropCanonical.CreatePayload(builder =>
            {
                builder.AddString("schemaId", schemaId);
                builder.AddString("rewardResultId", this.rewardResultId);
                builder.AddString("sourceFactId", this.sourceFactId);
                builder.AddString("stageClearId", this.stageClearId);
                builder.AddString("dropRequestId", this.dropRequestId);
                builder.AddString("dropRollResultId", this.dropRollResultId);
                builder.AddString("chapterId", this.chapterId);
                builder.AddString("stageId", this.stageId);
                RewardDropCanonical.AddLaunchContext(
                    builder,
                    "rewardLaunchContextIdentity",
                    this.rewardLaunchContextIdentity);
                builder.AddEnum("acquisitionMode", (int)this.acquisitionMode);
                builder.AddInt("rewardEntries.count", this.rewardEntries.Count);
                for (int index = 0; index < this.rewardEntries.Count; index++)
                {
                    RewardEntry entry = this.rewardEntries[index];
                    string prefix = "rewardEntries[" + index + "]";
                    builder.AddInt(prefix + ".position", index);
                    builder.AddString(prefix + ".schemaId", entry == null ? string.Empty : entry.schemaId);
                    builder.AddString(
                        prefix + ".rewardEntryId",
                        entry == null ? string.Empty : entry.rewardEntryId);
                    builder.AddEnum(
                        prefix + ".grantKind",
                        entry == null ? 0 : (int)entry.grantKind);
                    RewardDropCanonical.AddSubject(
                        builder,
                        prefix + ".rewardSubjectIdentity",
                        entry == null ? null : entry.rewardSubjectIdentity);
                    builder.AddString(
                        prefix + ".sourceDefinitionId",
                        entry == null ? string.Empty : entry.sourceDefinitionId);
                    builder.AddInt(
                        prefix + ".sourceRuleVersion",
                        entry == null ? 0 : entry.sourceRuleVersion);
                    builder.AddString(
                        prefix + ".itemInstanceId",
                        entry == null ? string.Empty : entry.itemInstanceId);
                    builder.AddString(
                        prefix + ".instanceCanonicalSignature",
                        entry == null ? string.Empty : entry.instanceCanonicalSignature);
                    builder.AddInt(prefix + ".quantity", entry == null ? 0 : entry.quantity);
                    builder.AddString(
                        prefix + ".canonicalSignature",
                        entry == null ? string.Empty : entry.canonicalSignature);
                }
            });
            canonicalSignature = RewardDropCanonical.ComputeSignature(canonicalPayload);
        }
    }
}
