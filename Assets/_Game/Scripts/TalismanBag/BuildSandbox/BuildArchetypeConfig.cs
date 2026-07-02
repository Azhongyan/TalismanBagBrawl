using System.Collections.Generic;
using UnityEngine;

namespace TalismanBag.BuildSandbox
{
    [CreateAssetMenu(
        menuName = "Talisman Bag/BuildSandbox/Build Archetype Config",
        fileName = "BuildArchetypeConfig")]
    public sealed class BuildArchetypeConfig : BuildSandboxDevOnlyConfig
    {
        public string archetypeId = "buildsandbox_archetype";
        public string displayName = "BuildSandbox Archetype";
        public List<string> keyTags = new();
        public List<string> recommendedItemIds = new();
        public List<string> targetSynergyIds = new();
        public List<PlacementConditionConfig> placementConditions = new();
        public List<EnergyConditionConfig> energyConditions = new();
        [TextArea] public string designNotes = "Data-only build archetype. No formal content pool reads this asset.";
    }
}
