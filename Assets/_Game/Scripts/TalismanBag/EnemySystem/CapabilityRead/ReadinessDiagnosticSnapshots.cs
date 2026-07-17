namespace TalismanBag.EnemySystem.CapabilityRead
{
    public sealed class CapabilityGapSnapshot
    {
        public CapabilityGapSnapshot(
            string requirementGroupId,
            string buildCapabilityKey,
            CapabilityValueAvailability capabilityValueAvailability,
            int availableBasisPoints,
            int requiredBasisPoints,
            int gapBasisPoints)
        {
            RequirementGroupId = BuildCapabilityReadOnly.Text(requirementGroupId);
            BuildCapabilityKey = BuildCapabilityReadOnly.Text(buildCapabilityKey);
            CapabilityValueAvailability = capabilityValueAvailability;
            AvailableBasisPoints = availableBasisPoints;
            RequiredBasisPoints = requiredBasisPoints;
            GapBasisPoints = gapBasisPoints;
        }

        public string RequirementGroupId { get; }
        public string BuildCapabilityKey { get; }
        public CapabilityValueAvailability CapabilityValueAvailability { get; }
        public int AvailableBasisPoints { get; }
        public int RequiredBasisPoints { get; }
        public int GapBasisPoints { get; }
    }
}
