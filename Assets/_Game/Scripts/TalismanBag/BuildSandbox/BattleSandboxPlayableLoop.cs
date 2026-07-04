using System;
using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.BuildSandbox
{
    public static class BattleSandboxPlayableLoop
    {
        public const string PackageName = "V0.4-BattleSandboxPlayableLoop01";
        public const string RuntimeLoopPackageName = BattleSandboxRuntimeLoopPreview.PackageName;
        public const string ScenePath = BuildSandboxPreviewSceneMarker.ScenePath;
    }

    [Serializable]
    public sealed class BattleSandboxPlayableLoopSnapshot
    {
        public string packageName = BattleSandboxPlayableLoop.PackageName;
        public string scenePath = BattleSandboxPlayableLoop.ScenePath;
        public string runtimeLoopPackageName = BattleSandboxPlayableLoop.RuntimeLoopPackageName;
        public bool devOnly = true;
        public bool isEnabled;
        public bool reusesBattleSandboxRuntimeLoop = true;
        public bool rewritesBattleLoop;
        public bool runtimeRunningStateVisible;
        public bool enemyAttackTimerAdvances;
        public bool enemyAttackDamageFromDevOnlyProfile;
        public bool playerShieldBeforeHpDamage;
        public bool runtimeAllowsPlayerHpZero;
        public bool victoryConditionEnemyHpZero;
        public bool defeatConditionPlayerHpZero;
        public bool victoryPromptVisible;
        public bool defeatPromptVisible;
        public bool buildBriefFeedbackVisible;
        public bool restartRetainsCurrentBuild = true;
        public bool restartResetsHpShieldManaCooldownLogCast = true;
        public bool switchDevTargets310410 = true;
        public bool formalRunFlowConnected;
        public bool writesSaveData;
        public bool grantsReward;
        public bool advancesChapter;
        public bool modifiesFormalBuildSettings;
        public bool touchesV02OrV03;
        public bool touchesFormalUiLayout;
        public int runtimePreviewRowCount;
        public int runtimeManaRowCount;
        public int runtimeCooldownRowCount;
        public int runtimeCombatLogRowCount;
        public int runtimeBossCastRowCount;
        public int runtimeSandboxResultRowCount;
        public int chapter310TargetCount;
        public int chapter410TargetCount;
        public int victoryRowCount;
        public int defeatRowCount;
        public int restartRowCount;
        public int switchTargetRowCount;
        public int buildFeedbackRowCount;
        public int featureFlagDefaultTrueCount;
        public int formalLeakCount;
        public int settlementLeakCount;
        public int playerSideAnswerLeakCount;
        public int uiLayoutWriteCount;
        public List<BattleSandboxPlayableLoopRow> rows = new();

        public bool HasRequiredRuntimeResetCoverage =>
            runtimeManaRowCount > 0
            && runtimeCooldownRowCount > 0
            && runtimeCombatLogRowCount > 0
            && runtimeBossCastRowCount > 0;

        public bool Passed =>
            devOnly
            && !isEnabled
            && reusesBattleSandboxRuntimeLoop
            && !rewritesBattleLoop
            && runtimeRunningStateVisible
            && enemyAttackTimerAdvances
            && enemyAttackDamageFromDevOnlyProfile
            && playerShieldBeforeHpDamage
            && runtimeAllowsPlayerHpZero
            && victoryConditionEnemyHpZero
            && defeatConditionPlayerHpZero
            && victoryPromptVisible
            && defeatPromptVisible
            && buildBriefFeedbackVisible
            && restartRetainsCurrentBuild
            && restartResetsHpShieldManaCooldownLogCast
            && switchDevTargets310410
            && HasRequiredRuntimeResetCoverage
            && chapter310TargetCount > 0
            && chapter410TargetCount > 0
            && victoryRowCount > 0
            && defeatRowCount > 0
            && restartRowCount > 0
            && switchTargetRowCount > 0
            && buildFeedbackRowCount > 0
            && featureFlagDefaultTrueCount == 0
            && formalLeakCount == 0
            && settlementLeakCount == 0
            && playerSideAnswerLeakCount == 0
            && uiLayoutWriteCount == 0
            && !formalRunFlowConnected
            && !writesSaveData
            && !grantsReward
            && !advancesChapter
            && !modifiesFormalBuildSettings
            && !touchesV02OrV03
            && !touchesFormalUiLayout
            && (rows?.All(row => row != null && row.Passed) ?? false);
    }

    [Serializable]
    public sealed class BattleSandboxPlayableLoopRow
    {
        public string rowId = string.Empty;
        public string rowKind = string.Empty;
        public string scenarioStageId = string.Empty;
        public string devChapterLabel = string.Empty;
        public string targetDisplayNameChinese = string.Empty;
        public int enemyHpAfter;
        public int playerHpAfter;
        public bool sandboxVictory;
        public bool sandboxDefeat;
        public bool promptVisible;
        public bool buildBriefVisible;
        public bool restartRetainsCurrentBuild;
        public bool restartResetsRuntimeState;
        public bool supportsTargetSwitch;
        public bool playerSideAnswerLeak;
        public bool formalFlowLeak;
        public bool uiLayoutWrite;
        public string resultTitleChinese = string.Empty;
        public string resultBodyChinese = string.Empty;
        public string restartHintChinese = string.Empty;

        public bool Passed =>
            !string.IsNullOrWhiteSpace(rowId)
            && !playerSideAnswerLeak
            && !formalFlowLeak
            && !uiLayoutWrite
            && promptVisible
            && buildBriefVisible
            && restartRetainsCurrentBuild
            && restartResetsRuntimeState
            && supportsTargetSwitch
            && (sandboxVictory && enemyHpAfter <= 0 || sandboxDefeat && playerHpAfter <= 0);
    }
}
