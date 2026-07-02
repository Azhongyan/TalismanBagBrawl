using System;
using System.Collections.Generic;
using UnityEngine;

namespace TalismanBag.BuildSandbox
{
    [CreateAssetMenu(
        menuName = "Talisman Bag/BuildSandbox/BuildProblem/Map Rule Config",
        fileName = "MapRuleConfig")]
    public sealed class MapRuleConfig : BuildSandboxDevOnlyConfig
    {
        public string mapRuleId = "map_rule_schema";
        public string displayName = "Map Rule Schema";
        public string mapRuleType = "problem_map";
        public string previewChapterSlot = "dev_only_preview";
        public List<string> targetBuildTags = new();
        public List<string> requiredSynergyIds = new();
        public List<string> enemyProblemIds = new();
        public List<string> bossProblemIds = new();
        public List<string> readinessCheckIds = new();
        public List<string> weaknessWindowIds = new();
        public List<string> failureHintIds = new();
        public string dropBiasId = string.Empty;
        public int recommendedProblemCount = 1;
        public bool simulatorReadable = true;
        public bool entersFormalFlow;
        public bool usesFormalStageData;
        public bool writesRuntimeUi;
        public bool modifiesProductBalance;
        [TextArea] public string problemSummary =
            "Map rule schema only. Keep disabled until BuildProblemSeedData01 supplies devOnly seed data.";
    }

    [CreateAssetMenu(
        menuName = "Talisman Bag/BuildSandbox/BuildProblem/Enemy Problem Config",
        fileName = "EnemyProblemConfig")]
    public sealed class EnemyProblemConfig : BuildSandboxDevOnlyConfig
    {
        public string enemyProblemId = "enemy_problem_schema";
        public string displayName = "Enemy Problem Schema";
        public string enemyRole = "problem_enemy";
        public string exposureType = "build_gap";
        public string targetBuildGap = "missing_counter";
        public List<string> targetBuildTags = new();
        public List<string> recommendedSynergyIds = new();
        public List<string> weaknessWindowIds = new();
        public List<string> readinessCheckIds = new();
        public List<string> failureHintIds = new();
        public int pressureRating = 1;
        public bool simulatorReadable = true;
        public bool entersFormalFlow;
        public bool referencesFormalEnemyPool;
        public bool affectsFormalCombat;
        [TextArea] public string problemSummary =
            "Enemy problem schema only. It describes a devOnly Build gap and is not a product enemy.";
    }

    [CreateAssetMenu(
        menuName = "Talisman Bag/BuildSandbox/BuildProblem/Boss Problem Config",
        fileName = "BossProblemConfig")]
    public sealed class BossProblemConfig : BuildSandboxDevOnlyConfig
    {
        public string bossProblemId = "boss_problem_schema";
        public string displayName = "Boss Problem Schema";
        public string bossRole = "problem_boss";
        public string bossLockType = "multi_key_lock";
        public List<BossProblemKeyRequirement> keyRequirements = new();
        public List<string> targetBuildTags = new();
        public List<string> readinessCheckIds = new();
        public List<string> weaknessWindowIds = new();
        public List<string> failureHintIds = new();
        public int minimumKeysRequired = 1;
        public bool simulatorReadable = true;
        public bool entersFormalFlow;
        public bool referencesFormalBossPool;
        public bool affectsFormalCombat;
        [TextArea] public string problemSummary =
            "Boss problem schema only. Multi-key checks stay devOnly and do not enter product Boss data.";
    }

    [Serializable]
    public sealed class BossProblemKeyRequirement
    {
        public string keyId = "boss_key_schema";
        public string checkType = "build_readiness";
        public string readinessCheckId = string.Empty;
        public string requiredTag = string.Empty;
        public string requiredSynergyId = string.Empty;
        public int requiredScore = 1;
        public bool devOnly = true;
        public bool isEnabled;
        public bool gatesFormalBoss;
        [TextArea] public string hint = "devOnly boss key schema; disabled and not connected to product Boss flow.";
    }

    [CreateAssetMenu(
        menuName = "Talisman Bag/BuildSandbox/BuildProblem/Build Readiness Check Config",
        fileName = "BuildReadinessCheckConfig")]
    public sealed class BuildReadinessCheckConfig : BuildSandboxDevOnlyConfig
    {
        public string readinessCheckId = "readiness_check_schema";
        public string displayName = "Build Readiness Check Schema";
        public string checkType = "tag_threshold";
        public List<string> requiredBuildTags = new();
        public List<string> requiredSynergyIds = new();
        public int requiredScore = 1;
        public int warningScore = 0;
        public List<string> failureHintIds = new();
        public bool reportsOnly = true;
        public bool entersFormalFlow;
        public bool writesBossInfoPanel;
        public bool affectsFormalCombat;
        [TextArea] public string checkSummary =
            "Readiness check schema only. It reports devOnly preview readiness and writes no product panel.";
    }

    [CreateAssetMenu(
        menuName = "Talisman Bag/BuildSandbox/BuildProblem/Weakness Window Config",
        fileName = "WeaknessWindowConfig")]
    public sealed class WeaknessWindowConfig : BuildSandboxDevOnlyConfig
    {
        public string weaknessWindowId = "weakness_window_schema";
        public string displayName = "Weakness Window Schema";
        public string windowType = "timed_vulnerability";
        public float startSecond;
        public float durationSecond = 3f;
        public List<string> exposedBuildTags = new();
        public List<string> recommendedSynergyIds = new();
        public List<string> failureHintIds = new();
        public bool simulatorReadable = true;
        public bool entersFormalFlow;
        public bool affectsFormalCombat;
        [TextArea] public string windowSummary =
            "Weakness window schema only. It is read by devOnly reports and not by product combat.";
    }

    [CreateAssetMenu(
        menuName = "Talisman Bag/BuildSandbox/BuildProblem/Drop Bias Config",
        fileName = "DropBiasConfig")]
    public sealed class DropBiasConfig : BuildSandboxDevOnlyConfig
    {
        public string dropBiasId = "drop_bias_schema";
        public string displayName = "Drop Bias Schema";
        public string biasType = "dev_only_problem_support";
        public List<string> targetBuildTags = new();
        public List<string> targetItemTags = new();
        public List<string> targetAffixIds = new();
        public float previewWeight = 1f;
        public bool reportsOnly = true;
        public bool entersFormalFlow;
        public bool touchesProductDropTable;
        public bool grantsProductReward;
        [TextArea] public string biasSummary =
            "DropBias schema only. It may guide later devOnly seed data and must not edit product drops.";
    }

    [CreateAssetMenu(
        menuName = "Talisman Bag/BuildSandbox/BuildProblem/Failure Hint Config",
        fileName = "FailureHintConfig")]
    public sealed class FailureHintConfig : BuildSandboxDevOnlyConfig
    {
        public string failureHintId = "failure_hint_schema";
        public string displayName = "Failure Hint Schema";
        public string hintType = "missing_build_key";
        public int priority = 1;
        public string headline = "Build readiness hint";
        [TextArea] public string detail = "devOnly failure hint schema.";
        public List<string> recommendedBuildTags = new();
        public List<string> recommendedSynergyIds = new();
        public bool reportsOnly = true;
        public bool entersFormalFlow;
        public bool writesRuntimeUi;
        [TextArea] public string hintSummary =
            "Failure hint schema only. It exports report copy and does not write product UI.";
    }
}
