using System.Collections.Generic;
using UnityEngine;

namespace TalismanBag.BuildSandbox
{
    [CreateAssetMenu(
        menuName = "Talisman Bag/BuildSandbox/Synergy Config",
        fileName = "SynergyConfig")]
    public sealed class SynergyConfig : BuildSandboxDevOnlyConfig
    {
        public string synergyId = "buildsandbox_synergy";
        public string displayName = "BuildSandbox Synergy";
        public List<string> requiredTags = new();
        public List<SynergyThresholdConfig> thresholds = new();
        public List<PlacementConditionConfig> placementConditions = new();
        public List<EnergyConditionConfig> energyConditions = new();
        [TextArea] public string designNotes = "Data-only synergy definition. Evaluator logic is a later package.";
    }
}
