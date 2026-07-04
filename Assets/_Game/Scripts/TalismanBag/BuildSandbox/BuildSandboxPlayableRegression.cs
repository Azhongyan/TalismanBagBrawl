using System;
using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.BuildSandbox
{
    public static class BuildSandboxPlayableRegression
    {
        public const string PackageName = "V0.4-BuildSandboxPlayableRegression01";
    }

    [Serializable]
    public sealed class BuildSandboxPlayableRegressionSnapshot
    {
        public string packageName = BuildSandboxPlayableRegression.PackageName;
        public bool devOnly = true;
        public bool isEnabled;
        public bool sceneExists;
        public bool sceneBindingPass;
        public bool itemShapeCoveragePass;
        public bool rotationScopePass;
        public bool placementSamplesPass;
        public bool currentBoardReadPass;
        public bool buildFeedbackVariationPass;
        public bool manaLoopPass;
        public bool devChapterBalancePass;
        public bool playerLeakPass;
        public bool featureFlagsDefaultFalsePass;
        public bool devOnlyDisabledPass;
        public bool formalScopePass;
        public bool noFormalSceneOrLayoutWritePass;
        public int nestedReportCount;
        public int errorCount;
        public int warningCount;
        public int x2TrayItemCount;
        public int x3TrayItemCount;
        public int x4TrayItemCount;
        public int legalPlacementSampleCount;
        public int illegalReturnSampleCount;
        public int bossStateRowCount;
        public int castBarRowCount;
        public int mechanicFloatingRowCount;
        public int combatLogRowCount;
        public int shapeRuleFeedbackRowCount;
        public int distinctBuildFeedbackCount;
        public int manaLoopGainRowCount;
        public int manaLoopSpendRowCount;
        public int manaLoopGeneratedManaTotal;
        public int manaLoopSpentManaTotal;
        public int manaLoopUiLayoutWriteCount;
        public int devBalance310StageCount;
        public int devBalance410StageCount;
        public int playerSideAnswerLeakCount;
        public int formalFlowLeakCount;
        public int featureFlagDefaultTrueCount;
        public int devOnlyFalseCount;
        public int isEnabledTrueCount;
        public List<BuildSandboxPlayableRegressionChecklistRow> checklistRows = new();

        public int ChecklistPassCount =>
            checklistRows?.Count(row => row != null && row.Passed) ?? 0;

        public int ChecklistFailCount =>
            checklistRows?.Count(row => row == null || !row.Passed) ?? 1;

        public bool Passed =>
            devOnly
            && !isEnabled
            && sceneExists
            && sceneBindingPass
            && itemShapeCoveragePass
            && rotationScopePass
            && placementSamplesPass
            && currentBoardReadPass
            && buildFeedbackVariationPass
            && manaLoopPass
            && devChapterBalancePass
            && playerLeakPass
            && featureFlagsDefaultFalsePass
            && devOnlyDisabledPass
            && formalScopePass
            && noFormalSceneOrLayoutWritePass
            && errorCount == 0
            && warningCount == 0
            && playerSideAnswerLeakCount == 0
            && formalFlowLeakCount == 0
            && featureFlagDefaultTrueCount == 0
            && devOnlyFalseCount == 0
            && isEnabledTrueCount == 0
            && ChecklistFailCount == 0;
    }

    [Serializable]
    public sealed class BuildSandboxPlayableRegressionChecklistRow
    {
        public string id = string.Empty;
        public string area = string.Empty;
        public string check = string.Empty;
        public string method = string.Empty;
        public string result = "FAIL";
        public string evidence = string.Empty;
        public bool userHandtestRequired;
        public string notes = string.Empty;

        public bool Passed => string.Equals(result, "PASS", StringComparison.Ordinal);
    }
}
