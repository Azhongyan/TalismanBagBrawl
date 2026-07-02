using System;
using UnityEngine;

namespace TalismanBag.BuildSandbox
{
    [Serializable]
    public sealed class EnergyConditionConfig
    {
        public string conditionId = "energy_preview";
        public string requiredProviderTag = string.Empty;
        public int minimumEnergy;
        public int maximumEnergy;
        public bool requiresPositiveNetEnergy;
        [TextArea] public string notes = "Data-only energy condition. It does not connect to battle supply logic.";
        public bool devOnly = true;
        public bool isEnabled;
    }
}
