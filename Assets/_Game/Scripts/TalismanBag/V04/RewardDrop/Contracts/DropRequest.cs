using System.Collections.Generic;

namespace TalismanBag.V04.RewardDrop.Contracts
{
    public sealed class DropRequest
    {
        public const string SchemaId = "DropRequest.v1";

        public string schemaId { get; }
        public string dropRequestId { get; }
        public RewardSourceFactKind sourceFactKind { get; }
        public string sourceFactId { get; }
        public string stageClearId { get; }
        public string chapterId { get; }
        public string stageId { get; }
        public RewardLaunchContextIdentity rewardLaunchContextIdentity { get; }
        public RewardAcquisitionMode acquisitionMode { get; }
        public long rootSeed { get; }
        public int generationVersion { get; }
        public string poolId { get; }
        public string poolVersion { get; }
        public string rarityTierProfileId { get; }
        public string attributeRollBandProfileId { get; }
        public string rollPolicyVersion { get; }
        public IReadOnlyList<DropRollSlot> rollSlots { get; }
        public string canonicalPayload { get; }
        public string canonicalSignature { get; }

        public DropRequest(
            string dropRequestId,
            RewardSourceFactKind sourceFactKind,
            string sourceFactId,
            string stageClearId,
            string chapterId,
            string stageId,
            RewardLaunchContextIdentity rewardLaunchContextIdentity,
            RewardAcquisitionMode acquisitionMode,
            long rootSeed,
            int generationVersion,
            string poolId,
            string poolVersion,
            string rarityTierProfileId,
            string attributeRollBandProfileId,
            string rollPolicyVersion,
            IEnumerable<DropRollSlot> rollSlots)
        {
            schemaId = SchemaId;
            this.dropRequestId = RewardDropCanonical.NormalizeIdentifier(dropRequestId);
            this.sourceFactKind = sourceFactKind;
            this.sourceFactId = RewardDropCanonical.NormalizeIdentifier(sourceFactId);
            this.stageClearId = RewardDropCanonical.NormalizeIdentifier(stageClearId);
            this.chapterId = RewardDropCanonical.NormalizeIdentifier(chapterId);
            this.stageId = RewardDropCanonical.NormalizeIdentifier(stageId);
            this.rewardLaunchContextIdentity = rewardLaunchContextIdentity;
            this.acquisitionMode = acquisitionMode;
            this.rootSeed = rootSeed;
            this.generationVersion = generationVersion;
            this.poolId = RewardDropCanonical.NormalizeIdentifier(poolId);
            this.poolVersion = RewardDropCanonical.NormalizeIdentifier(poolVersion);
            this.rarityTierProfileId =
                RewardDropCanonical.NormalizeIdentifier(rarityTierProfileId);
            this.attributeRollBandProfileId =
                RewardDropCanonical.NormalizeIdentifier(attributeRollBandProfileId);
            this.rollPolicyVersion =
                RewardDropCanonical.NormalizeIdentifier(rollPolicyVersion);
            this.rollSlots = RewardDropCanonical.CopyReadOnly(rollSlots);
            canonicalPayload = RewardDropCanonical.CreatePayload(builder =>
            {
                builder.AddString("schemaId", schemaId);
                builder.AddString("dropRequestId", this.dropRequestId);
                builder.AddEnum("sourceFactKind", (int)this.sourceFactKind);
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
                builder.AddString("poolId", this.poolId);
                builder.AddString("poolVersion", this.poolVersion);
                builder.AddString("rarityTierProfileId", this.rarityTierProfileId);
                builder.AddString(
                    "attributeRollBandProfileId",
                    this.attributeRollBandProfileId);
                builder.AddString("rollPolicyVersion", this.rollPolicyVersion);
                builder.AddInt("rollSlots.count", this.rollSlots.Count);
                for (int index = 0; index < this.rollSlots.Count; index++)
                {
                    DropRollSlot slot = this.rollSlots[index];
                    string prefix = "rollSlots[" + index + "]";
                    builder.AddInt(prefix + ".position", index);
                    builder.AddString(prefix + ".schemaId", slot == null ? string.Empty : slot.schemaId);
                    builder.AddString(
                        prefix + ".rollSlotId",
                        slot == null ? string.Empty : slot.rollSlotId);
                    builder.AddEnum(
                        prefix + ".grantKind",
                        slot == null ? 0 : (int)slot.grantKind);
                    builder.AddString(
                        prefix + ".canonicalSignature",
                        slot == null ? string.Empty : slot.canonicalSignature);
                }
            });
            canonicalSignature = RewardDropCanonical.ComputeSignature(canonicalPayload);
        }
    }
}
