using System;
using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.BuildSandbox
{
    public static class BattleSandboxPlayableFullRosterRegression
    {
        public const string PackageName = "V0.4-BattleSandboxPlayableFullRosterRegression01";
        public const int ExpectedRosterItemCount = 23;

        public static readonly string[] ExpectedAttackDamageItemIds =
        {
            "preview_fire_talisman",
            "preview_thunder_sword",
            "preview_taomu_sword",
            "fire_talisman_basic",
            "thunder_talisman_basic",
            "sword_pill_basic",
            "chain_thunder_talisman_basic",
            "exorcism_bell_basic"
        };

        public static readonly string[] ExpectedNoDamageSupportItemIds =
        {
            "preview_stone_core",
            "preview_soul_seal",
            "preview_cleanse_corner",
            "preview_old_bell",
            "preview_x2_wood_talisman",
            "preview_guard_wood",
            "preview_energy_incense",
            "shield_talisman_basic",
            "qi_pill_basic",
            "spirit_stone_basic",
            "purify_talisman_basic",
            "soul_suppress_talisman_basic",
            "seal_basic",
            "water_talisman_basic",
            "peach_wood_basic"
        };
    }

    [Serializable]
    public sealed class BattleSandboxPlayableFullRosterRegressionSnapshot
    {
        public string packageName = BattleSandboxPlayableFullRosterRegression.PackageName;
        public bool devOnly = true;
        public bool isEnabled;
        public bool sceneExists;
        public bool sceneBindingPass;
        public bool fullRosterTrayCoveragePass;
        public bool basicSinglePlacementPass;
        public bool multiShapePlacementPass;
        public bool emptyBoardDefeatPass;
        public bool attackItemDamagePass;
        public bool shieldDamageOrderPass;
        public bool supportNoDamagePass;
        public bool sandboxResultPass;
        public bool restartAndSwitchTargetPass;
        public bool playerLeakPass;
        public bool formalScopePass;
        public bool noFormalSceneOrLayoutWritePass;
        public bool featureFlagsDefaultFalsePass;
        public bool devOnlyDisabledPass;
        public int nestedReportCount;
        public int errorCount;
        public int warningCount;
        public int rosterItemCount;
        public int trayItemCount;
        public int trayPackedItemCount;
        public int missingTrayItemCount;
        public int trayPackFailureCount;
        public int basicItemCount;
        public int basicSingle1ItemCount;
        public int basicPlacementValidCount;
        public int x2PlacementValidCount;
        public int x3PlacementValidCount;
        public int x4PlacementValidCount;
        public int vertical3PlacementValidCount;
        public int multiCellPlacementFailureCount;
        public int emptyBoardEnemyHpInitial;
        public int emptyBoardEnemyHpFinal;
        public int emptyBoardPlayerHpInitial;
        public int emptyBoardPlayerHpFinal;
        public int emptyBoardPlayerHpRows;
        public int emptyBoardDefeatRows;
        public int attackCandidateCount;
        public int attackDamageItemCount;
        public int attackEnemyHpDamageTotal;
        public int shieldAttackRowCount;
        public int shieldFirstAttackRowCount;
        public int supportNoDamageCandidateCount;
        public int supportDamageLeakCount;
        public int sandboxVictoryRows;
        public int sandboxDefeatRows;
        public int devEnemyScenarioCount;
        public int switchTargetPlayablePreviewCount;
        public int playerSideAnswerLeakCount;
        public int formalFlowLeakCount;
        public int featureFlagDefaultTrueCount;
        public int devOnlyFalseCount;
        public int isEnabledTrueCount;
        public List<BattleSandboxPlayableFullRosterItemRegressionRow> itemRows = new();
        public List<BattleSandboxPlayableFullRosterRegressionChecklistRow> checklistRows = new();

        public int ChecklistPassCount =>
            checklistRows?.Count(row => row != null && row.Passed) ?? 0;

        public int ChecklistFailCount =>
            checklistRows?.Count(row => row == null || !row.Passed) ?? 1;

        public bool Passed =>
            devOnly
            && !isEnabled
            && sceneExists
            && sceneBindingPass
            && fullRosterTrayCoveragePass
            && basicSinglePlacementPass
            && multiShapePlacementPass
            && emptyBoardDefeatPass
            && attackItemDamagePass
            && shieldDamageOrderPass
            && supportNoDamagePass
            && sandboxResultPass
            && restartAndSwitchTargetPass
            && playerLeakPass
            && formalScopePass
            && noFormalSceneOrLayoutWritePass
            && featureFlagsDefaultFalsePass
            && devOnlyDisabledPass
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
    public sealed class BattleSandboxPlayableFullRosterItemRegressionRow
    {
        public string itemId = string.Empty;
        public string shapeId = string.Empty;
        public string categoryId = string.Empty;
        public string statProfileId = string.Empty;
        public bool trayPresent;
        public bool trayPacked;
        public bool boardPlacementValid;
        public bool basicDefaultSingle1;
        public bool expectedAttackDamage;
        public bool expectedNoDamageSupport;
        public int shapeCellCount;
        public int enemyHpDamageTotal;
        public int initialEnemyHp;
        public int finalEnemyHp;
        public int playerSideAnswerLeakCount;
        public int formalFlowLeakCount;

        public bool Passed =>
            trayPresent
            && trayPacked
            && boardPlacementValid
            && (!expectedNoDamageSupport || enemyHpDamageTotal == 0 && finalEnemyHp == initialEnemyHp)
            && playerSideAnswerLeakCount == 0
            && formalFlowLeakCount == 0;
    }

    [Serializable]
    public sealed class BattleSandboxPlayableFullRosterRegressionChecklistRow
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
