using System;
using System.Collections.Generic;

namespace TalismanBag.Contracts.Battle
{
    [Serializable]
    public sealed class BuildEvaluationSnapshot
    {
        public string snapshotId = string.Empty;
        public List<BattleActiveSynergySnapshot> activeSynergies = new();
        public string readinessSummary = string.Empty;
        public List<string> modifierBundleSummary = new();
        public List<string> eventBundleSummary = new();
        public List<string> playerVisibleHints = new();
        public string recommendedAction = string.Empty;
        public bool devOnly = true;

        public List<string> shapeBuildRules = new();
        public List<string> problemReadinessFullAnswer = new();
        public List<string> bossSixKeyFullAnswer = new();
        public List<string> hardSolutionTags = new();
        public List<string> requiredSynergy = new();
        public List<string> requiredAffix = new();
        public List<string> requiredStats = new();
        public List<string> dropBiasWeights = new();
        public string sourceDataPath = string.Empty;
        public List<string> developerOnlyDiagnostics = new();
    }

    [Serializable]
    public sealed class BattleActiveSynergySnapshot
    {
        public string synergyId = string.Empty;
        public string displayName = string.Empty;
        public int matchedCount;
        public List<string> activeThresholds = new();
        public List<string> sourceItems = new();
        public bool placementSatisfied = true;
        public bool energySatisfied = true;
    }
}
