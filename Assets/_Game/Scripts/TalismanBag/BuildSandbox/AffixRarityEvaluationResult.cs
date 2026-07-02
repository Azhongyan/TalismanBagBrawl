using System;
using System.Collections.Generic;

namespace TalismanBag.BuildSandbox
{
    [Serializable]
    public sealed class AffixRarityEvaluationResult
    {
        public List<AffixRarityItemResult> itemResults = new();
        public List<string> rarityIds = new();
        public List<string> affixIds = new();
        public List<AffixRarityRequirementResult> missingRequirements = new();

        public bool Passed => missingRequirements.Count == 0;
    }

    [Serializable]
    public sealed class AffixRarityItemResult
    {
        public string itemId = string.Empty;
        public string rarityId = string.Empty;
        public int tierIndex;
        public int affixSlotCount;
        public float previewPowerMultiplier = 1f;
        public int totalPreviewPower;
        public List<string> selectedAffixes = new();
        public List<string> sourceTags = new();
    }

    [Serializable]
    public sealed class AffixRarityRequirementResult
    {
        public string itemId = string.Empty;
        public string requirementType = string.Empty;
        public string requiredId = string.Empty;
        public bool satisfied;
        public string detail = string.Empty;
    }
}
