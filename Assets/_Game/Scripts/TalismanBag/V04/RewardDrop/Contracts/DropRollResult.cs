using System.Collections.Generic;

namespace TalismanBag.V04.RewardDrop.Contracts
{
    public sealed class DropRollResult
    {
        public const string SchemaId = "DropRollResult.v1";

        public string schemaId { get; }
        public string dropRollResultId { get; }
        public string dropRequestId { get; }
        public string sourceFactId { get; }
        public string stageClearId { get; }
        public string chapterId { get; }
        public string stageId { get; }
        public RewardLaunchContextIdentity rewardLaunchContextIdentity { get; }
        public RewardAcquisitionMode acquisitionMode { get; }
        public long rootSeed { get; }
        public int generationVersion { get; }
        public string algorithmId { get; }
        public string poolId { get; }
        public string poolVersion { get; }
        public string rarityTierProfileId { get; }
        public string attributeRollBandProfileId { get; }
        public string rollPolicyVersion { get; }
        public IReadOnlyList<DropRollEntry> entries { get; }
        public string canonicalPayload { get; }
        public string canonicalSignature { get; }

        public DropRollResult(
            string dropRollResultId,
            string dropRequestId,
            string sourceFactId,
            string stageClearId,
            string chapterId,
            string stageId,
            RewardLaunchContextIdentity rewardLaunchContextIdentity,
            RewardAcquisitionMode acquisitionMode,
            long rootSeed,
            int generationVersion,
            string algorithmId,
            string poolId,
            string poolVersion,
            string rarityTierProfileId,
            string attributeRollBandProfileId,
            string rollPolicyVersion,
            IEnumerable<DropRollEntry> entries)
        {
            schemaId = SchemaId;
            this.dropRollResultId =
                RewardDropCanonical.NormalizeIdentifier(dropRollResultId);
            this.dropRequestId = RewardDropCanonical.NormalizeIdentifier(dropRequestId);
            this.sourceFactId = RewardDropCanonical.NormalizeIdentifier(sourceFactId);
            this.stageClearId = RewardDropCanonical.NormalizeIdentifier(stageClearId);
            this.chapterId = RewardDropCanonical.NormalizeIdentifier(chapterId);
            this.stageId = RewardDropCanonical.NormalizeIdentifier(stageId);
            this.rewardLaunchContextIdentity = rewardLaunchContextIdentity;
            this.acquisitionMode = acquisitionMode;
            this.rootSeed = rootSeed;
            this.generationVersion = generationVersion;
            this.algorithmId = RewardDropCanonical.NormalizeIdentifier(algorithmId);
            this.poolId = RewardDropCanonical.NormalizeIdentifier(poolId);
            this.poolVersion = RewardDropCanonical.NormalizeIdentifier(poolVersion);
            this.rarityTierProfileId =
                RewardDropCanonical.NormalizeIdentifier(rarityTierProfileId);
            this.attributeRollBandProfileId =
                RewardDropCanonical.NormalizeIdentifier(attributeRollBandProfileId);
            this.rollPolicyVersion =
                RewardDropCanonical.NormalizeIdentifier(rollPolicyVersion);
            this.entries = RewardDropCanonical.CopyReadOnly(entries);
            canonicalPayload = RewardDropCanonical.CreatePayload(builder =>
            {
                builder.AddString("schemaId", schemaId);
                builder.AddString("dropRollResultId", this.dropRollResultId);
                builder.AddString("dropRequestId", this.dropRequestId);
                builder.AddString("sourceFactId", this.sourceFactId);
                builder.AddString("stageClearId", this.stageClearId);
                builder.AddString("chapterId", this.chapterId);
                builder.AddString("stageId", this.stageId);
                RewardDropCanonical.AddLaunchContext(
                    builder,
                    "rewardLaunchContextIdentity",
                    this.rewardLaunchContextIdentity);
                builder.AddEnum("acquisitionMode", (int)this.acquisitionMode);
                builder.AddLong("rootSeed", this.rootSeed);
                builder.AddInt("generationVersion", this.generationVersion);
                builder.AddString("algorithmId", this.algorithmId);
                builder.AddString("poolId", this.poolId);
                builder.AddString("poolVersion", this.poolVersion);
                builder.AddString("rarityTierProfileId", this.rarityTierProfileId);
                builder.AddString(
                    "attributeRollBandProfileId",
                    this.attributeRollBandProfileId);
                builder.AddString("rollPolicyVersion", this.rollPolicyVersion);
                builder.AddInt("entries.count", this.entries.Count);
                for (int index = 0; index < this.entries.Count; index++)
                {
                    DropRollEntry entry = this.entries[index];
                    string prefix = "entries[" + index + "]";
                    builder.AddInt(prefix + ".position", index);
                    builder.AddString(prefix + ".schemaId", entry == null ? string.Empty : entry.schemaId);
                    builder.AddString(
                        prefix + ".rollEntryId",
                        entry == null ? string.Empty : entry.rollEntryId);
                    builder.AddString(
                        prefix + ".rollSlotId",
                        entry == null ? string.Empty : entry.rollSlotId);
                    builder.AddEnum(
                        prefix + ".grantKind",
                        entry == null ? 0 : (int)entry.grantKind);
                    RewardDropCanonical.AddSubject(
                        builder,
                        prefix + ".rewardSubjectIdentity",
                        entry == null ? null : entry.rewardSubjectIdentity);
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
