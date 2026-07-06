using System;
using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.BuildSandbox
{
    public static class BattleSandboxDevChapterPlayable
    {
        public const string PackageName = "V0.4-BattleSandboxDevChapterPlayable01";
        public const string RuntimeLoopPackageName = BattleSandboxRuntimeLoopPreview.PackageName;
        public const string PlayableLoopPackageName = BattleSandboxPlayableLoop.PackageName;
        public const string FullRosterPackageName = BattleSandboxPlayableFullRosterRegression.PackageName;
        public const string ScenePath = BuildSandboxPreviewSceneMarker.ScenePath;
    }

    [Serializable]
    public sealed class BattleSandboxDevChapterPlayableSnapshot
    {
        public string packageName = BattleSandboxDevChapterPlayable.PackageName;
        public string scenePath = BattleSandboxDevChapterPlayable.ScenePath;
        public bool devOnly = true;
        public bool isEnabled;
        public bool sceneExists;
        public bool reusesBattleSandboxRuntimeLoop = true;
        public bool reusesBattleSandboxPlayableLoopResult = true;
        public bool reusesBattleSandboxFullRosterResult = true;
        public bool playableLoopResultPassed;
        public bool fullRosterResultPassed;
        public bool rewritesBattleLoop;
        public bool chapter310Selectable;
        public bool chapter410Selectable;
        public bool devChapterSelectorBindingPresent;
        public bool runtimeChapterSelectionApiPresent;
        public bool restartSupported;
        public bool chapterSwitchSupported;
        public bool differentBossProfileAcrossChapters;
        public bool differentMechanicFeedbackAcrossChapters;
        public bool differentBuildPressureAcrossChapters;
        public bool formalRunFlowConnected;
        public bool writesSaveData;
        public bool grantsReward;
        public bool advancesChapter;
        public bool touchesFormalStageConfig;
        public bool touchesFormalBossTable;
        public bool touchesFormalRewardTable;
        public bool touchesV02OrV03;
        public bool runsSceneBinder;
        public int scenarioCount;
        public int chapter310ScenarioCount;
        public int chapter410ScenarioCount;
        public int victoryRowCount;
        public int defeatRowCount;
        public int mechanicFeedbackRowCount;
        public int buildPressureRowCount;
        public int distinctBossProfileCount;
        public int playerSideAnswerLeakCount;
        public int formalFlowLeakCount;
        public int featureFlagDefaultTrueCount;
        public int uiLayoutWriteCount;
        public int devChapterSelectorLayoutWriteCount;
        public int nestedPlayableLoopErrorCount;
        public int nestedFullRosterErrorCount;
        public int nestedWarningCount;
        public List<BattleSandboxDevChapterPlayableRow> rows = new();

        public int PassedRowCount => rows?.Count(row => row != null && row.Passed) ?? 0;
        public int FailedRowCount => rows?.Count(row => row == null || !row.Passed) ?? 1;

        public bool Passed =>
            devOnly
            && !isEnabled
            && sceneExists
            && reusesBattleSandboxRuntimeLoop
            && reusesBattleSandboxPlayableLoopResult
            && reusesBattleSandboxFullRosterResult
            && playableLoopResultPassed
            && fullRosterResultPassed
            && !rewritesBattleLoop
            && chapter310Selectable
            && chapter410Selectable
            && devChapterSelectorBindingPresent
            && runtimeChapterSelectionApiPresent
            && restartSupported
            && chapterSwitchSupported
            && differentBossProfileAcrossChapters
            && differentMechanicFeedbackAcrossChapters
            && differentBuildPressureAcrossChapters
            && scenarioCount >= 2
            && chapter310ScenarioCount > 0
            && chapter410ScenarioCount > 0
            && victoryRowCount > 0
            && defeatRowCount > 0
            && mechanicFeedbackRowCount >= 2
            && buildPressureRowCount >= 2
            && distinctBossProfileCount >= 2
            && playerSideAnswerLeakCount == 0
            && formalFlowLeakCount == 0
            && featureFlagDefaultTrueCount == 0
            && uiLayoutWriteCount == 0
            && devChapterSelectorLayoutWriteCount == 0
            && nestedPlayableLoopErrorCount == 0
            && nestedFullRosterErrorCount == 0
            && nestedWarningCount == 0
            && !formalRunFlowConnected
            && !writesSaveData
            && !grantsReward
            && !advancesChapter
            && !touchesFormalStageConfig
            && !touchesFormalBossTable
            && !touchesFormalRewardTable
            && !touchesV02OrV03
            && !runsSceneBinder
            && FailedRowCount == 0;
    }

    [Serializable]
    public sealed class BattleSandboxDevChapterPlayableRow
    {
        public string rowId = string.Empty;
        public string stageId = string.Empty;
        public string devChapterLabel = string.Empty;
        public string targetDisplayNameChinese = string.Empty;
        public string bossProfileId = string.Empty;
        public string mechanicFeedbackChinese = string.Empty;
        public string buildPressureChinese = string.Empty;
        public int attackDamage;
        public float attackIntervalSeconds;
        public int enemyHpAfter;
        public int playerHpAfter;
        public bool selectable;
        public bool usesDevOnlyBossProfile;
        public bool sandboxVictory;
        public bool sandboxDefeat;
        public bool promptVisible;
        public bool mechanicFeedbackVisible;
        public bool buildPressureVisible;
        public bool restartSupported;
        public bool chapterSwitchSupported;
        public bool playerSideAnswerLeak;
        public bool formalFlowLeak;
        public bool uiLayoutWrite;

        public bool Passed =>
            selectable
            && usesDevOnlyBossProfile
            && !string.IsNullOrWhiteSpace(rowId)
            && !string.IsNullOrWhiteSpace(stageId)
            && !string.IsNullOrWhiteSpace(devChapterLabel)
            && !string.IsNullOrWhiteSpace(targetDisplayNameChinese)
            && !string.IsNullOrWhiteSpace(bossProfileId)
            && promptVisible
            && mechanicFeedbackVisible
            && buildPressureVisible
            && restartSupported
            && chapterSwitchSupported
            && !playerSideAnswerLeak
            && !formalFlowLeak
            && !uiLayoutWrite
            && (sandboxVictory && enemyHpAfter <= 0 || sandboxDefeat && playerHpAfter <= 0);
    }
}
