using UnityEngine;

namespace TalismanBag.BuildSandbox
{
    [CreateAssetMenu(
        menuName = "Talisman Bag/BuildSandbox/Rarity Tier Config",
        fileName = "RarityTierConfig")]
    public sealed class RarityTierConfig : BuildSandboxDevOnlyConfig
    {
        public string rarityId = "common";
        public string displayName = "Common";
        public int tierIndex;
        public int affixSlotCount = 1;
        public int rollWeight = 100;
        public float previewPowerMultiplier = 1f;
        public string visualKey = "rarity_common_placeholder";
        [TextArea] public string designNotes = "BuildSandbox rarity preview only. No formal drop, reward, or upgrade table connection.";
    }
}
