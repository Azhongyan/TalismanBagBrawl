using System;
using UnityEngine;

namespace TalismanBag.BuildSandbox
{
    [Serializable]
    public sealed class PlacementConditionConfig
    {
        public string conditionId = "placement_preview";
        public string conditionType = "none";
        public string requiredTag = string.Empty;
        public int requiredCount;
        [TextArea] public string notes = "Data-only placement condition. Evaluator logic is not part of this package.";
        public bool devOnly = true;
        public bool isEnabled;
    }
}
