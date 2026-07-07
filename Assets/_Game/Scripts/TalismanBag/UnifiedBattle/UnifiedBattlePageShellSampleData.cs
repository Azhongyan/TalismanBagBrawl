using System;
using System.Collections.Generic;
using TalismanBag.Contracts.Battle;

namespace TalismanBag.UnifiedBattle
{
    [Serializable]
    public sealed class UnifiedBattlePageShellSampleBinding
    {
        public BattleStartRequest startRequest = new();
        public BattleLayoutSnapshot layoutSnapshot = new();
        public BattleEnemySnapshot enemySnapshot = new();
        public BuildEvaluationSnapshot buildEvaluationSnapshot = new();
        public BattleResultSnapshot resultSnapshot = new();
    }

    public static class UnifiedBattlePageShellSampleData
    {
        public static UnifiedBattlePageShellSampleBinding CreateDefault()
        {
            BattleGridCell fireAnchor = new(1, 1);
            BattleGridCell wardAnchor = new(2, 2);
            BattleGridCell wardTail = new(3, 2);

            UnifiedBattlePageShellSampleBinding binding = new()
            {
                startRequest = new BattleStartRequest
                {
                    requestId = "unified_shell_sample_request",
                    chapterId = "devOnly",
                    stageId = "devOnly-shell",
                    roundId = "devOnly-shell-01",
                    isBossStage = true,
                    enemyProfileId = "dev_shell_enemy",
                    bossProfileId = "dev_shell_boss",
                    entrySource = BattleEntrySource.EditorValidation,
                    allowedItemRosterId = "dev_shell_sample_roster",
                    formalFlow = false,
                    devOnly = true,
                    sourceRoute = "UnifiedBattlePageShell01 sample only",
                    sourceScene = UnifiedBattlePageShellMarker.ScenePath,
                    sourceController = nameof(UnifiedBattlePageShell),
                    seedId = "unified_shell_sample_seed",
                    validationWarnings = new List<string>
                    {
                        "Sample data only; does not start formal battle."
                    }
                },
                layoutSnapshot = new BattleLayoutSnapshot
                {
                    snapshotId = "unified_shell_sample_layout",
                    boardId = "unified_shell_board_5x5",
                    gridWidth = 5,
                    gridHeight = 5,
                    devOnly = true,
                    sourceAdapter = "UnifiedBattlePageShellSampleData",
                    legacySingleCell = false,
                    currentEnergyState = BattleContractEnergyState.Powered,
                    placedItems = new List<BattleItemSnapshot>
                    {
                        new()
                        {
                            itemInstanceId = "sample_fire_talisman_01",
                            itemId = "fire_talisman_basic",
                            displayName = "Fire Talisman",
                            familyId = "fire",
                            baseItemId = "fire_talisman_basic",
                            shapeId = "Single1",
                            rotationIndex = 0,
                            anchorCell = fireAnchor,
                            occupiedCells = new List<BattleGridCell> { fireAnchor },
                            rarity = "white",
                            energyState = BattleContractEnergyState.Powered,
                            connectedEyeId = "sample_eye",
                            sourceContainer = "UnifiedBattleShellSample",
                            synergyTags = new List<string> { "fire", "starter" }
                        },
                        new()
                        {
                            itemInstanceId = "sample_ward_wood_01",
                            itemId = "ward_wood_vertical2",
                            displayName = "Ward Wood",
                            familyId = "ward",
                            baseItemId = "ward_wood",
                            shapeId = "Vertical2",
                            rotationIndex = 0,
                            anchorCell = wardAnchor,
                            occupiedCells = new List<BattleGridCell> { wardAnchor, wardTail },
                            rarity = "green",
                            energyState = BattleContractEnergyState.WeakPulse,
                            connectedEyeId = "sample_eye",
                            sourceContainer = "UnifiedBattleShellSample",
                            synergyTags = new List<string> { "ward", "shape_test" }
                        }
                    },
                    formationEyes = new List<BattleFormationEyeSnapshot>
                    {
                        new()
                        {
                            eyeId = "sample_eye",
                            cell = new BattleGridCell(2, 2),
                            energyState = BattleContractEnergyState.WeakPulse,
                            stability = 80,
                            devOnly = true
                        }
                    },
                    energySources = new List<BattleEnergySourceSnapshot>
                    {
                        new()
                        {
                            energySourceId = "sample_energy_stone",
                            itemId = "energy_stone_basic",
                            cell = new BattleGridCell(0, 2),
                            energyState = BattleContractEnergyState.Powered,
                            suppliedItemIds = new List<string>
                            {
                                "fire_talisman_basic",
                                "ward_wood_vertical2"
                            },
                            devOnly = true
                        }
                    }
                },
                enemySnapshot = new BattleEnemySnapshot
                {
                    enemyId = "dev_shell_enemy",
                    bossId = "dev_shell_boss",
                    hp = 360,
                    shield = 40,
                    attackDamage = 18,
                    attackInterval = 2.4f,
                    castSkillId = "sample_lock_array_cast",
                    castProgress = 0.35f,
                    mechanicTags = new List<string> { "cast_bar", "pressure" },
                    weaknessHints = new List<string> { "Watch the cast bar before the pressure wave." },
                    devOnlyProfileId = "dev_shell_boss_problem",
                    developerOnlyDiagnostics = new List<string>
                    {
                        "Developer diagnostics stay inside DevOnlyDiagnosticsSlot."
                    },
                    devOnly = true
                },
                buildEvaluationSnapshot = new BuildEvaluationSnapshot
                {
                    snapshotId = "unified_shell_build_sample",
                    readinessSummary = "ready_basic",
                    modifierBundleSummary = new List<string> { "modifier:starter_pressure" },
                    eventBundleSummary = new List<string> { "event:cast_warning" },
                    playerVisibleHints = new List<string> { "The array is stable enough for a sample preview." },
                    recommendedAction = "try_sample_preview",
                    devOnly = true,
                    activeSynergies = new List<BattleActiveSynergySnapshot>
                    {
                        new()
                        {
                            synergyId = "starter_guard",
                            displayName = "Starter Guard",
                            matchedCount = 2,
                            activeThresholds = new List<string> { "2" },
                            sourceItems = new List<string>
                            {
                                "fire_talisman_basic",
                                "ward_wood_vertical2"
                            },
                            placementSatisfied = true,
                            energySatisfied = true
                        }
                    },
                    hardSolutionTags = new List<string> { "dev_only_sample_answer" },
                    requiredSynergy = new List<string> { "starter_guard" },
                    sourceDataPath = "UnifiedBattlePageShellSampleData"
                },
                resultSnapshot = new BattleResultSnapshot
                {
                    resultId = "unified_shell_sample_result",
                    requestId = "unified_shell_sample_request",
                    resultType = BattleResultType.Unknown,
                    roundId = "devOnly-shell-01",
                    chapterProgressDelta = "none",
                    rewardPreview = new List<string> { "placeholder_only_no_reward" },
                    itemDrops = new List<string>(),
                    buildPerformanceSummary = "sample shell result placeholder only",
                    eventSummary = new List<string> { "sample_event:no_commit" },
                    rewardClaimToken = string.Empty,
                    nextRouteHint = "return_to_dev_shell",
                    devOnly = true,
                    shouldWriteSave = false,
                    shouldGrantReward = false
                }
            };

            return binding;
        }
    }
}
