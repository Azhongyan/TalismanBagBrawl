using System;
using System.Collections.Generic;

namespace TalismanBag.BuildSandbox
{
    [Serializable]
    public sealed class BuildEvaluationResult
    {
        public List<ActiveSynergyResult> activeSynergies = new();
        public List<string> activeThresholds = new();
        public List<string> sourceItems = new();
        public bool placementSatisfied = true;
        public bool energySatisfied = true;
        public List<SynergyRequirementResult> missingRequirements = new();
        public NextThresholdHint nextThresholdHint = NextThresholdHint.None();

        public bool HasActiveSynergy => activeSynergies.Count > 0;
    }

    [Serializable]
    public sealed class ActiveSynergyResult
    {
        public string synergyId = string.Empty;
        public string displayName = string.Empty;
        public int matchedCount;
        public List<string> activeThresholds = new();
        public List<string> sourceItems = new();
        public bool placementSatisfied = true;
        public bool energySatisfied = true;
        public List<SynergyRequirementResult> requirementResults = new();
        public NextThresholdHint nextThresholdHint = NextThresholdHint.None();
    }

    [Serializable]
    public sealed class SynergyRequirementResult
    {
        public string synergyId = string.Empty;
        public string requirementId = string.Empty;
        public string requirementType = string.Empty;
        public string requiredTag = string.Empty;
        public int requiredCount;
        public int actualCount;
        public bool satisfied;
        public List<string> sourceItems = new();
        public string detail = string.Empty;
    }

    [Serializable]
    public sealed class NextThresholdHint
    {
        public string synergyId = string.Empty;
        public int currentPieceCount;
        public int nextPieceCount;
        public int missingPieceCount;
        public List<string> requiredTags = new();
        public List<string> sourceItems = new();
        public bool hasNextThreshold;

        public static NextThresholdHint None()
        {
            return new NextThresholdHint();
        }
    }
}
