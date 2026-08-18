namespace TalismanBag.V04.RewardDrop.Contracts
{
    public sealed class StageClearFact
    {
        public const string SchemaId = "StageClearFact.v1";

        public string schemaId { get; }
        public string stageClearId { get; }
        public string battleResultId { get; }
        public string battleRequestId { get; }
        public string resultFingerprint { get; }
        public string chapterId { get; }
        public string stageId { get; }
        public string runSessionId { get; }
        public int clearSequence { get; }
        public bool isBossStage { get; }
        public RewardLaunchContextIdentity rewardLaunchContextIdentity { get; }
        public string canonicalPayload { get; }
        public string canonicalSignature { get; }

        public StageClearFact(
            string stageClearId,
            string battleResultId,
            string battleRequestId,
            string resultFingerprint,
            string chapterId,
            string stageId,
            string runSessionId,
            int clearSequence,
            bool isBossStage,
            RewardLaunchContextIdentity rewardLaunchContextIdentity)
        {
            schemaId = SchemaId;
            this.stageClearId = RewardDropCanonical.NormalizeIdentifier(stageClearId);
            this.battleResultId = RewardDropCanonical.NormalizeIdentifier(battleResultId);
            this.battleRequestId = RewardDropCanonical.NormalizeIdentifier(battleRequestId);
            this.resultFingerprint =
                RewardDropCanonical.NormalizeIdentifier(resultFingerprint);
            this.chapterId = RewardDropCanonical.NormalizeIdentifier(chapterId);
            this.stageId = RewardDropCanonical.NormalizeIdentifier(stageId);
            this.runSessionId = RewardDropCanonical.NormalizeIdentifier(runSessionId);
            this.clearSequence = clearSequence;
            this.isBossStage = isBossStage;
            this.rewardLaunchContextIdentity = rewardLaunchContextIdentity;
            canonicalPayload = RewardDropCanonical.CreatePayload(builder =>
            {
                builder.AddString("schemaId", schemaId);
                builder.AddString("stageClearId", this.stageClearId);
                builder.AddString("battleResultId", this.battleResultId);
                builder.AddString("battleRequestId", this.battleRequestId);
                builder.AddString("resultFingerprint", this.resultFingerprint);
                builder.AddString("chapterId", this.chapterId);
                builder.AddString("stageId", this.stageId);
                builder.AddString("runSessionId", this.runSessionId);
                builder.AddInt("clearSequence", this.clearSequence);
                builder.AddBool("isBossStage", this.isBossStage);
                RewardDropCanonical.AddLaunchContext(
                    builder,
                    "rewardLaunchContextIdentity",
                    this.rewardLaunchContextIdentity);
            });
            canonicalSignature = RewardDropCanonical.ComputeSignature(canonicalPayload);
        }
    }
}
