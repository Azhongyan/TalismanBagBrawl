namespace TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime.BoneSwapRemnant
{
    public sealed class BoneSwapRemnantCopyProfile
    {
        public BoneSwapRemnantCopyProfile(
            string profileId,
            BoneSwapRemnantTuningDisposition tuningDisposition,
            int copyRatioBasisPoints,
            int copyCapDamage,
            long telegraphTicks,
            long recoverTicks,
            BoneSwapRemnantPendingPolicy pendingPolicy)
        {
            ProfileId = profileId ?? string.Empty;
            TuningDisposition = tuningDisposition;
            CopyRatioBasisPoints = copyRatioBasisPoints;
            CopyCapDamage = copyCapDamage;
            TelegraphTicks = telegraphTicks;
            RecoverTicks = recoverTicks;
            PendingPolicy = pendingPolicy;
            CanonicalSignature = BoneSwapRemnantCanonical.Hash(
                ProfileId,
                TuningDisposition.ToString(),
                BoneSwapRemnantCanonical.Number(CopyRatioBasisPoints),
                BoneSwapRemnantCanonical.Number(CopyCapDamage),
                BoneSwapRemnantCanonical.Number(TelegraphTicks),
                BoneSwapRemnantCanonical.Number(RecoverTicks),
                PendingPolicy.ToString());
        }

        public string ProfileId { get; }
        public BoneSwapRemnantTuningDisposition TuningDisposition { get; }
        public int CopyRatioBasisPoints { get; }
        public int CopyCapDamage { get; }
        public long TelegraphTicks { get; }
        public long RecoverTicks { get; }
        public BoneSwapRemnantPendingPolicy PendingPolicy { get; }
        public string CanonicalSignature { get; }
    }
}
