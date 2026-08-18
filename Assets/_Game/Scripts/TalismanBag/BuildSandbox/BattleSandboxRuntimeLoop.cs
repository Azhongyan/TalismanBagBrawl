using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.Contracts.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.BuildSandbox
{
    [Serializable]
    public sealed class BattleSandboxRuntimeLoopPreview
    {
        public const string PackageName = "V0.4-BattleSandboxRuntimeLoop01";
        public const string DeveloperDataPanelFieldKey = "battleSandboxRuntimeLoopPreview";

        public string packageName = PackageName;
        public bool devOnly = true;
        public bool isEnabled;
        public bool readsCombatKernelAdapter = true;
        public bool readsBuildSandboxItemStat = true;
        public bool readsCurrentBoardSnapshot = true;
        public bool readsBuildCombatPreview = true;
        public bool updatesRuntimeHudText = true;
        public bool updatesRuntimeFloatingText = true;
        public bool runsFormalCombat;
        public bool callsFormalDamageSettlement;
        public bool writesFormalFlow;
        public bool writesFormalSaveData;
        public bool grantsFormalReward;
        public bool advancesChapter;
        public bool opensFeatureFlag;
        public bool touchesFormalSceneUiLayout;
        public int rectTransformLayoutWriteCount;
        public bool hasVictorySettlement;
        public bool hasDefeatSettlement;
        public bool hasSandboxVictoryResult;
        public bool hasSandboxDefeatResult;
        public string selectedDevEnemyStageId = string.Empty;
        public string selectedDevEnemyLabel = string.Empty;
        public string selectedDevEnemyDisplayNameChinese = string.Empty;
        public string selectedDevEnemyMechanicFeedbackChinese = string.Empty;
        public string selectedDevEnemyBuildPressureChinese = string.Empty;
        public bool usesDefaultLayoutFallbackWhenBoardEmpty;
        public bool runtimeUsesDefaultLayoutAsCombatInput;
        public int currentBoardPlacedItemCount;
        public int maxMana;
        public int initialMana;
        public int finalMana;
        public int generatedManaTotal;
        public int spentManaTotal;
        public int playerMaxHp = 100;
        public int initialPlayerHp = 100;
        public int finalPlayerHp = 100;
        public int initialPlayerShield;
        public int finalPlayerShield;
        public int enemyMaxHp = 120;
        public int initialEnemyHp = 120;
        public int finalEnemyHp = 120;
        public int initialEnemyShield;
        public int finalEnemyShield;
        public int playerItemEnemyHpDamageTotal;
        public float bossCastDurationSeconds = 2.4f;
        public bool runtimeLoopStartsWhenBattleModeActive = true;
        public int selectedDevEnemyAttackDamage;
        public float selectedDevEnemyAttackIntervalSeconds;
        public string selectedDevEnemyAttackSourcePath = string.Empty;
        public int sourceItemStatProfileCount;
        public int sourceCombatKernelAdapterRowCount;
        public int sourceBuildCombatPreviewRowCount;
        public bool usesExplicitDevEncounter;
        public string explicitEncounterProfileId = string.Empty;
        public string explicitProfileFingerprint = string.Empty;
        public int explicitGeneration;
        public string explicitStartToken = string.Empty;
        public string explicitEnemyIdentity = string.Empty;
        public string explicitEncounterKind = string.Empty;
        public int acceptedEnemyBasicAttackCount;
        public int acceptedEnemySkillCount;
        public int targetDurationMilliseconds;
        public List<BattleSandboxRuntimeLoopRow> rows = new();

        public int ManaRowCount => CountRows("mana");
        public int CooldownRowCount => CountRows("cooldown");
        public int ItemTriggerRowCount => CountRows("itemTrigger");
        public int EnemyHpRowCount => CountRows("enemyHp");
        public int EnemyShieldRowCount => CountRows(row => row != null && row.enemyShieldBefore != row.enemyShieldAfter);
        public int PlayerHpRowCount => CountRows(row => row != null && row.playerHpBefore != row.playerHpAfter);
        public int PlayerShieldRowCount => CountRows(row => row != null && row.playerShieldBefore != row.playerShieldAfter);
        public int BossCastRowCount => CountRows("bossCast");
        public int EnemyAttackRowCount => CountRows(row => row != null && row.enemyAttackDamage > 0);
        public int EnemyAttackTimerAdvanceRowCount => CountRows(row => row != null && row.enemyAttackTimerAdvanced);
        public int DevOnlyProfileAttackDamageRowCount => CountRows(row =>
            row != null && row.enemyAttackDamage > 0 && row.enemyAttackFromDevOnlyProfile);
        public int ShieldFirstPlayerDamageRowCount => CountRows(row =>
            row != null
            && row.enemyAttackDamage > 0
            && row.playerShieldDamageResolvedFirst
            && row.playerShieldAfter <= row.playerShieldBefore
            && row.playerHpAfter <= row.playerHpBefore);
        public int CombatLogRowCount => CountRows(row => row != null && !string.IsNullOrWhiteSpace(row.combatLogLineChinese));
        public int FloatingTextRowCount => CountRows(row => row != null && !string.IsNullOrWhiteSpace(row.floatingTextChinese));
        public int NoSettlementRowCount => CountRows("noSettlement");
        public int SandboxVictoryResultRowCount => CountRows(row => row != null && row.hasSandboxVictoryResult);
        public int SandboxDefeatResultRowCount => CountRows(row => row != null && row.hasSandboxDefeatResult);
        public int SandboxResultRowCount => SandboxVictoryResultRowCount + SandboxDefeatResultRowCount;
        public int PlayerSideAnswerLeakCount => CountRows(row => row != null && row.playerSideAnswerLeak);
        public int UiLayoutWriteCount => rectTransformLayoutWriteCount + CountRows(row => row != null && row.uiLayoutWrite);
        public int SettlementLeakCount =>
            (hasVictorySettlement ? 1 : 0)
            + (hasDefeatSettlement ? 1 : 0)
            + CountRows(row => row != null && (row.hasVictorySettlement || row.hasDefeatSettlement));

        public int FeatureFlagDefaultTrueCount
        {
            get
            {
                int count = 0;
                foreach (BuildSandboxFeatureFlagDefinition flag in BuildSandboxFeatureFlags.All)
                {
                    if (flag.DefaultValue)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public int FormalLeakCount =>
            (runsFormalCombat ? 1 : 0)
            + (callsFormalDamageSettlement ? 1 : 0)
            + (writesFormalFlow ? 1 : 0)
            + (writesFormalSaveData ? 1 : 0)
            + (grantsFormalReward ? 1 : 0)
            + (advancesChapter ? 1 : 0)
            + (opensFeatureFlag ? 1 : 0)
            + CountRows(row => row != null && row.formalFlowLeak);

        public bool DevOnlyIsolationPass =>
            devOnly
            && !isEnabled
            && readsCombatKernelAdapter
            && readsBuildSandboxItemStat
            && readsCurrentBoardSnapshot
            && readsBuildCombatPreview
            && updatesRuntimeHudText
            && updatesRuntimeFloatingText
            && FormalLeakCount == 0
            && SettlementLeakCount == 0
            && PlayerSideAnswerLeakCount == 0
            && UiLayoutWriteCount == 0
            && !touchesFormalSceneUiLayout
            && FeatureFlagDefaultTrueCount == 0;

        private int CountRows(string rowKind)
        {
            return CountRows(row => row != null && string.Equals(row.rowKind, rowKind, StringComparison.Ordinal));
        }

        private int CountRows(Func<BattleSandboxRuntimeLoopRow, bool> predicate)
        {
            int count = 0;
            foreach (BattleSandboxRuntimeLoopRow row in rows ?? new List<BattleSandboxRuntimeLoopRow>())
            {
                if (predicate(row))
                {
                    count++;
                }
            }

            return count;
        }
    }

    [Serializable]
    public sealed class BattleSandboxRuntimeLoopRow
    {
        public string rowId = string.Empty;
        public string rowKind = string.Empty;
        public string itemId = string.Empty;
        public string statProfileId = string.Empty;
        public string itemEffectKey = string.Empty;
        public string itemEffectFamilyChinese = string.Empty;
        public string itemEffectRoleChinese = string.Empty;
        public float elapsedSeconds;
        public int manaBefore;
        public int manaAfter;
        public int manaDelta;
        public float cooldownSeconds;
        public string itemTriggerChinese = string.Empty;
        public int enemyHpBefore;
        public int enemyHpAfter;
        public int enemyShieldBefore;
        public int enemyShieldAfter;
        public int playerHpBefore;
        public int playerHpAfter;
        public int playerShieldBefore;
        public int playerShieldAfter;
        public float bossCastRemainingSeconds;
        public float bossCastFillAmount;
        public float enemyAttackTimerBeforeSeconds;
        public float enemyAttackTimerAfterSeconds;
        public int enemyAttackDamage;
        public int playerShieldDamageAbsorbed;
        public int playerHpDamageApplied;
        public bool enemyAttackTimerAdvanced;
        public bool enemyAttackFromDevOnlyProfile;
        public bool playerShieldDamageResolvedFirst;
        public string stateLineChinese = string.Empty;
        public string castSkillLineChinese = string.Empty;
        public string combatLogLineChinese = string.Empty;
        public string floatingTextChinese = string.Empty;
        public bool playsBoardItemTriggerFeedback;
        public string boardItemTriggerFeedbackChannel = "active";
        public string boardItemTriggerFeedbackKind = string.Empty;
        public string boardItemTriggerFeedbackTextChinese = string.Empty;
        public int boardItemTriggerFeedbackValue;
        public List<ItemShapeCell> boardItemTriggerOccupiedCells = new();
        public string sourceDataPath = "BattleSandboxRuntimeLoopPreview.rows";
        public string developerDataPanelFieldKey = BattleSandboxRuntimeLoopPreview.DeveloperDataPanelFieldKey;
        public bool readsCombatKernelAdapter = true;
        public bool readsBuildSandboxItemStat = true;
        public bool readsCurrentBoardSnapshot = true;
        public bool formalFlowLeak;
        public bool playerSideAnswerLeak;
        public bool uiLayoutWrite;
        public bool hasVictorySettlement;
        public bool hasDefeatSettlement;
        public bool hasSandboxVictoryResult;
        public bool hasSandboxDefeatResult;
        public bool hasAcceptedEnemyAction;
        public string acceptedEnemyActionKind = string.Empty;
        public int acceptedEnemyActionSequence;
        public int acceptedEnemyActionAtMilliseconds;
        public bool locksRuntimeLoop;
        public string resultTitleChinese = string.Empty;
        public string resultBodyChinese = string.Empty;
        public string restartHintChinese = string.Empty;
    }

    [Serializable]
    public sealed class BattleSandboxRuntimeLoopFrame
    {
        public string stateLineChinese = string.Empty;
        public string castSkillLineChinese = string.Empty;
        public string castTimerTextChinese = string.Empty;
        public string combatLogLineChinese = string.Empty;
        public string floatingTextChinese = string.Empty;
        public float castFillAmount;
        public Color castFillColor = Color.white;
        public Color floatingColor = Color.white;
        public int currentMana;
        public int maxMana;
        public int playerHp;
        public int playerMaxHp;
        public int playerShield;
        public int enemyHp;
        public int enemyMaxHp;
        public int enemyShield;
        public bool isSandboxResult;
        public bool isSandboxVictory;
        public bool locksRuntimeLoop;
        public string resultTitleChinese = string.Empty;
        public string resultBodyChinese = string.Empty;
        public string restartHintChinese = string.Empty;

        public static BattleSandboxRuntimeLoopFrame FromRow(
            BattleSandboxRuntimeLoopRow row,
            BattleSandboxRuntimeLoopPreview preview)
        {
            BattleSandboxRuntimeLoopRow safeRow = row ?? new BattleSandboxRuntimeLoopRow();
            BattleSandboxRuntimeLoopPreview safePreview = preview ?? new BattleSandboxRuntimeLoopPreview();
            return new BattleSandboxRuntimeLoopFrame
            {
                stateLineChinese = safeRow.stateLineChinese,
                castSkillLineChinese = safeRow.castSkillLineChinese,
                castTimerTextChinese = safeRow.bossCastRemainingSeconds > 0f
                    ? $"{safeRow.bossCastRemainingSeconds:0.0}\u79d2"
                    : "\u5f85\u673a",
                combatLogLineChinese = safeRow.combatLogLineChinese,
                floatingTextChinese = safeRow.floatingTextChinese,
                castFillAmount = Mathf.Clamp01(safeRow.bossCastFillAmount),
                castFillColor = ResolveColor(safeRow.rowKind),
                floatingColor = ResolveFloatingColor(safeRow.rowKind),
                currentMana = safeRow.manaAfter,
                maxMana = Mathf.Max(1, safePreview.maxMana),
                playerHp = Mathf.Max(0, safeRow.playerHpAfter),
                playerMaxHp = Mathf.Max(1, safePreview.playerMaxHp),
                playerShield = Mathf.Max(0, safeRow.playerShieldAfter),
                enemyHp = Mathf.Max(0, safeRow.enemyHpAfter),
                enemyMaxHp = Mathf.Max(1, safePreview.enemyMaxHp),
                enemyShield = Mathf.Max(0, safeRow.enemyShieldAfter),
                isSandboxResult = safeRow.hasSandboxVictoryResult || safeRow.hasSandboxDefeatResult,
                isSandboxVictory = safeRow.hasSandboxVictoryResult,
                locksRuntimeLoop = safeRow.locksRuntimeLoop,
                resultTitleChinese = safeRow.resultTitleChinese,
                resultBodyChinese = safeRow.resultBodyChinese,
                restartHintChinese = safeRow.restartHintChinese
            };
        }

        private static Color ResolveColor(string rowKind)
        {
            return rowKind switch
            {
                "bossCast" => new Color(1f, 0.48f, 0.30f, 1f),
                "sandboxVictory" => new Color(0.64f, 1f, 0.54f, 1f),
                "sandboxDefeat" => new Color(1f, 0.46f, 0.42f, 1f),
                "enemyHp" => new Color(1f, 0.72f, 0.42f, 1f),
                "playerHp" => new Color(1f, 0.36f, 0.34f, 1f),
                "playerShield" => new Color(0.52f, 0.84f, 1f, 1f),
                "mana" => new Color(0.60f, 0.96f, 1f, 1f),
                "cooldown" => new Color(0.82f, 0.82f, 1f, 1f),
                _ => new Color(0.72f, 1f, 0.42f, 1f)
            };
        }

        private static Color ResolveFloatingColor(string rowKind)
        {
            return rowKind switch
            {
                "enemyHp" => new Color(1f, 0.74f, 0.38f, 1f),
                "enemyShield" => new Color(0.92f, 0.88f, 1f, 1f),
                "playerHp" => new Color(1f, 0.42f, 0.38f, 1f),
                "playerShield" => new Color(0.52f, 0.88f, 1f, 1f),
                "bossCast" => new Color(1f, 0.55f, 0.36f, 1f),
                "sandboxVictory" => new Color(0.70f, 1f, 0.52f, 1f),
                "sandboxDefeat" => new Color(1f, 0.42f, 0.38f, 1f),
                "mana" => new Color(0.72f, 0.98f, 1f, 1f),
                _ => new Color(0.76f, 1f, 0.46f, 1f)
            };
        }
    }

    [Serializable]
    public sealed class BattleSandboxRuntimeLoopScenario
    {
        public string stageId = string.Empty;
        public string devChapterLabel = string.Empty;
        public string previewBuildId = string.Empty;
        public string enemyDisplayNameChinese = string.Empty;
        public float simulatedWinRate;
        public bool expectsSandboxVictory;
        public string devOnlyProfileId = string.Empty;
        public string attackSourcePath = string.Empty;
        public int attackDamage = 8;
        public float attackIntervalSeconds = 2.4f;
        public bool attackFromDevOnlyProfile;
        public string playerMechanicFeedbackChinese = string.Empty;
        public string playerBuildPressureChinese = string.Empty;

        public string DisplayLabel =>
            string.IsNullOrWhiteSpace(devChapterLabel)
                ? enemyDisplayNameChinese
                : $"{devChapterLabel} {enemyDisplayNameChinese}";
    }

    public static class BattleSandboxRuntimeLoopPreviewBuilder
    {
        public const string PackageName = BattleSandboxRuntimeLoopPreview.PackageName;

        private const int PreviewStepCount = 8;
        private const float StepSeconds = 0.75f;
        private const string SourceRuntimeLoop = "BattleSandboxRuntimeLoopPreview.rows";
        private const string SourceItemStat = "BuildSandboxLayoutSnapshot.placedItems[].itemStat";
        private const string SourceCombatKernel = "BattleSandboxCombatKernelAdapterBuilder";
        private const string SourceBuildCombatPreview = "BattleSandboxBuildCombatPreviewBuilder";
        private const string SourceDevEnemySelection = "V04SandboxDevEnemySelection";
        private const string SourceDevEnemyProfileAttack = "EnemyBossValidationPool.bosses[].attackDamage";
        private const string SourceDevEnemyProfileAttackInterval = "EnemyBossValidationPool.bosses[].attackIntervalSeconds";

        private sealed class BuildPassiveFeedbackSource
        {
            public BuildSandboxPlacedItemSnapshot item;
            public string feedbackKind = string.Empty;
            public string feedbackTextChinese = string.Empty;
            public int feedbackValue;
            public string sourceDataPath = string.Empty;
        }

        public static BattleSandboxRuntimeLoopPreview BuildDefaultPreview()
        {
            return Build(
                BattleSandboxBuildCombatPreviewBuilder.BuildDefaultPreviewLayoutSnapshot(),
                "default_v04_runtime_loop_board",
                ResolveDevEnemyScenario(0),
                allowDefaultLayoutFallbackWhenBoardEmpty: true);
        }

        public static BattleSandboxRuntimeLoopPreview Build(
            BuildSandboxLayoutSnapshot snapshot,
            string sourcePreviewBuildId = "current_v04_runtime_loop_board")
        {
            return Build(snapshot, sourcePreviewBuildId, ResolveDevEnemyScenario(0));
        }

        public static BattleSandboxRuntimeLoopPreview Build(
            BuildSandboxLayoutSnapshot snapshot,
            string sourcePreviewBuildId,
            BattleSandboxRuntimeLoopScenario scenario,
            bool allowDefaultLayoutFallbackWhenBoardEmpty = false)
        {
            return BuildInternal(
                snapshot,
                sourcePreviewBuildId,
                scenario,
                allowDefaultLayoutFallbackWhenBoardEmpty,
                null);
        }

        internal static BattleSandboxRuntimeLoopPreview BuildExplicitDevEncounter(
            BuildSandboxLayoutSnapshot snapshot,
            string sourcePreviewBuildId,
            BattleSandboxExplicitDevEncounterRequest request)
        {
            if (request == null)
            {
                return null;
            }

            BattleSandboxRuntimeLoopScenario scenario = new()
            {
                stageId = request.EncounterProfileId,
                devChapterLabel = request.EncounterProfileId,
                previewBuildId = sourcePreviewBuildId ?? string.Empty,
                enemyDisplayNameChinese = request.EnemyIdentity,
                simulatedWinRate = 1f,
                expectsSandboxVictory = true,
                devOnlyProfileId = request.EncounterProfileId,
                attackSourcePath =
                    "BattleSandboxExplicitDevEncounterRequest.acceptedEnemyActionCadence",
                attackDamage = request.BasicAttackDamage,
                attackIntervalSeconds =
                    request.BasicAttackIntervalMilliseconds / 1000f,
                attackFromDevOnlyProfile = true,
                playerMechanicFeedbackChinese =
                    "实验遭遇参数已由显式开发合同接受。",
                playerBuildPressureChinese =
                    "仅用于垂直切片手测，不代表正式关卡平衡。"
            };
            return BuildInternal(
                snapshot,
                sourcePreviewBuildId,
                scenario,
                false,
                request);
        }

        private static BattleSandboxRuntimeLoopPreview BuildInternal(
            BuildSandboxLayoutSnapshot snapshot,
            string sourcePreviewBuildId,
            BattleSandboxRuntimeLoopScenario scenario,
            bool allowDefaultLayoutFallbackWhenBoardEmpty,
            BattleSandboxExplicitDevEncounterRequest explicitRequest)
        {
            BuildSandboxLayoutSnapshot safeSnapshot = NormalizeSnapshot(
                snapshot,
                allowDefaultLayoutFallbackWhenBoardEmpty,
                out bool usedFallback);
            List<BuildSandboxPlacedItemSnapshot> placedItems = NormalizeItems(safeSnapshot.placedItems);
            BattleSandboxRuntimeLoopScenario safeScenario = scenario
                ?? ResolveDevEnemyScenario(0)
                ?? new BattleSandboxRuntimeLoopScenario
                {
                    stageId = "dev_balance_3_10_fallback",
                    devChapterLabel = "3-10",
                    previewBuildId = "dev_balance_3_10_fallback",
                    enemyDisplayNameChinese = "\u6d4b\u8bd5\u654c\u4eba",
                    simulatedWinRate = 0.45f,
                    expectsSandboxVictory = false,
                    devOnlyProfileId = "dev_boss_burst_huzhen",
                    attackSourcePath = SourceDevEnemyProfileAttack,
                    attackDamage = 55,
                    attackIntervalSeconds = 1.5f,
                    attackFromDevOnlyProfile = true,
                    playerMechanicFeedbackChinese = "\u9996\u9886\u6b63\u5728\u8bd5\u63a2\u9635\u9762\u627f\u538b\u70b9\u3002",
                    playerBuildPressureChinese = "\u5efa\u8bae\u89c2\u5bdf\u62a4\u76fe\u3001\u4f9b\u80fd\u548c\u6301\u7eed\u538b\u5236\u662f\u5426\u8ddf\u5f97\u4e0a\u3002"
                };
            BattleSandboxCombatKernelAdapterPreview adapter =
                BattleSandboxCombatKernelAdapterBuilder.BuildDefaultPreview();
            BattleSandboxBuildCombatPreview buildCombatPreview =
                BattleSandboxBuildCombatPreviewBuilder.Build(
                    safeSnapshot,
                    string.IsNullOrWhiteSpace(sourcePreviewBuildId)
                        ? "current_v04_runtime_loop_board"
                        : sourcePreviewBuildId,
                    usesCurrentBoardSnapshot: true);

            bool usesExplicitRequest = explicitRequest != null;
            int playerMaxHp = usesExplicitRequest
                ? explicitRequest.PlayerMaxHp
                : 100;
            int playerHp = playerMaxHp;
            int playerShield = usesExplicitRequest
                ? explicitRequest.PlayerInitialShield
                : Mathf.Clamp(placedItems.Sum(item => ResolveStat(item).guard), 0, BattleSandboxCombatKernelAdapterBuilder.PlayerShieldCap);
            int enemyMaxHp = usesExplicitRequest
                ? explicitRequest.EnemyMaxHp
                : ResolveEnemyMaxHp(adapter, placedItems.Count, safeScenario);
            int enemyHp = enemyMaxHp;
            int enemyShield = ResolveEnemyShield(placedItems, safeScenario);
            int currentMana = BattleSandboxManaLoopPreviewBuilder.CalculateInitialMana(placedItems);
            int maxMana = BattleSandboxManaLoopPreviewBuilder.CalculateMaxMana(placedItems);
            int enemyAttackDamage = usesExplicitRequest
                ? explicitRequest.BasicAttackDamage
                : ResolveEnemyAttackDamage(safeScenario);
            float bossCastDuration = usesExplicitRequest
                ? explicitRequest.BasicAttackIntervalMilliseconds / 1000f
                : ResolveEnemyAttackIntervalSeconds(safeScenario);
            float bossCastRemaining = bossCastDuration;

            BattleSandboxRuntimeLoopPreview preview = new()
            {
                packageName = PackageName,
                devOnly = true,
                isEnabled = false,
                readsCombatKernelAdapter = true,
                readsBuildSandboxItemStat = true,
                readsCurrentBoardSnapshot = true,
                readsBuildCombatPreview = true,
                updatesRuntimeHudText = true,
                updatesRuntimeFloatingText = true,
                hasSandboxVictoryResult = false,
                hasSandboxDefeatResult = false,
                selectedDevEnemyStageId = safeScenario.stageId,
                selectedDevEnemyLabel = safeScenario.devChapterLabel,
                selectedDevEnemyDisplayNameChinese = safeScenario.enemyDisplayNameChinese,
                selectedDevEnemyMechanicFeedbackChinese = safeScenario.playerMechanicFeedbackChinese,
                selectedDevEnemyBuildPressureChinese = safeScenario.playerBuildPressureChinese,
                usesDefaultLayoutFallbackWhenBoardEmpty = usedFallback,
                runtimeUsesDefaultLayoutAsCombatInput = usedFallback,
                currentBoardPlacedItemCount = placedItems.Count,
                maxMana = maxMana,
                initialMana = currentMana,
                playerMaxHp = playerMaxHp,
                initialPlayerHp = playerHp,
                initialPlayerShield = playerShield,
                enemyMaxHp = enemyMaxHp,
                initialEnemyHp = enemyHp,
                initialEnemyShield = enemyShield,
                bossCastDurationSeconds = bossCastDuration,
                runtimeLoopStartsWhenBattleModeActive = true,
                selectedDevEnemyAttackDamage = enemyAttackDamage,
                selectedDevEnemyAttackIntervalSeconds = bossCastDuration,
                selectedDevEnemyAttackSourcePath = string.IsNullOrWhiteSpace(safeScenario.attackSourcePath)
                    ? SourceDevEnemyProfileAttack
                    : safeScenario.attackSourcePath,
                sourceItemStatProfileCount = placedItems.Count(item => !string.IsNullOrWhiteSpace(ResolveStat(item).statProfileId)),
                sourceCombatKernelAdapterRowCount = adapter.RuleRowCount,
                sourceBuildCombatPreviewRowCount = buildCombatPreview?.rows?.Count ?? 0,
                usesExplicitDevEncounter = usesExplicitRequest,
                explicitEncounterProfileId = explicitRequest?.EncounterProfileId ?? string.Empty,
                explicitProfileFingerprint = explicitRequest?.ProfileFingerprint ?? string.Empty,
                explicitGeneration = explicitRequest?.Generation ?? 0,
                explicitStartToken = explicitRequest?.StartToken ?? string.Empty,
                explicitEnemyIdentity = explicitRequest?.EnemyIdentity ?? string.Empty,
                explicitEncounterKind = explicitRequest?.EncounterKind.ToString() ?? string.Empty,
                acceptedEnemyBasicAttackCount = explicitRequest?.AcceptedEnemyActionCadence.Count(cue =>
                    cue.ActionKind == BattleSandboxExplicitDevEnemyActionKind.BasicAttack) ?? 0,
                acceptedEnemySkillCount = explicitRequest?.AcceptedEnemyActionCadence.Count(cue =>
                    cue.ActionKind == BattleSandboxExplicitDevEnemyActionKind.Skill) ?? 0,
                targetDurationMilliseconds = explicitRequest?.TargetDurationMilliseconds ?? 0
            };

            AddOpeningRow(preview, safeScenario, currentMana, playerHp, playerShield, enemyHp, enemyShield, bossCastRemaining, bossCastDuration);

            int spendIndex = 0;
            int passiveEffectIndex = 0;
            int manaProviderEffectIndex = 0;
            int buildPassiveEffectIndex = 0;
            int simulationStepCount = usesExplicitRequest
                ? Mathf.Max(
                    PreviewStepCount,
                    Mathf.CeilToInt(
                        explicitRequest.TargetDurationMilliseconds
                        / (StepSeconds * 1000f)))
                : PreviewStepCount;
            int explicitCadenceIndex = 0;
            for (int step = 0; step < simulationStepCount; step++)
            {
                float elapsed = (step + 1) * StepSeconds;
                int manaBefore = currentMana;
                int manaGain = BattleSandboxManaLoopPreviewBuilder.CalculateManaGain(placedItems);
                currentMana = Mathf.Clamp(currentMana + manaGain, 0, maxMana);
                int actualManaGain = Mathf.Max(0, currentMana - manaBefore);
                preview.generatedManaTotal += actualManaGain;
                preview.rows.Add(CreateManaRow(step, elapsed, manaBefore, currentMana, playerHp, playerShield, enemyHp, enemyShield, bossCastRemaining, bossCastDuration));

                if (actualManaGain > 0)
                {
                    BuildSandboxPlacedItemSnapshot manaProviderItem =
                        FindNextManaProviderEffectItem(placedItems, ref manaProviderEffectIndex);
                    if (manaProviderItem != null)
                    {
                        BuildSandboxItemStat manaProviderStat = ResolveStat(manaProviderItem);
                        LegacyItemBehaviorResult manaProviderBehavior =
                            LegacyItemBehaviorIntegrationCatalog.Evaluate(manaProviderItem, manaProviderStat);
                        preview.rows.Add(CreatePassiveEffectRow(step, elapsed, manaProviderItem, manaProviderStat, manaProviderBehavior, manaBefore, currentMana, playerHp, playerShield, enemyHp, enemyShield, bossCastRemaining, bossCastDuration));
                    }
                }

                BuildSandboxPlacedItemSnapshot passiveItem =
                    FindNextPassiveEffectItem(placedItems, ref passiveEffectIndex);
                if (passiveItem != null)
                {
                    BuildSandboxItemStat passiveStat = ResolveStat(passiveItem);
                    LegacyItemBehaviorResult passiveBehavior =
                        LegacyItemBehaviorIntegrationCatalog.Evaluate(passiveItem, passiveStat);
                    preview.rows.Add(CreatePassiveEffectRow(step, elapsed, passiveItem, passiveStat, passiveBehavior, manaBefore, currentMana, playerHp, playerShield, enemyHp, enemyShield, bossCastRemaining, bossCastDuration));
                }

                BuildPassiveFeedbackSource buildPassive = step % 2 == 0
                    ? FindNextBuildPassiveFeedbackSource(buildCombatPreview, placedItems, ref buildPassiveEffectIndex)
                    : null;
                if (buildPassive != null)
                {
                    preview.rows.Add(CreateBuildPassiveFeedbackRow(step, elapsed, buildPassive, currentMana, playerHp, playerShield, enemyHp, enemyShield, bossCastRemaining, bossCastDuration));
                }

                BuildSandboxPlacedItemSnapshot item =
                    BattleSandboxManaLoopPreviewBuilder.FindNextSpendItem(placedItems, ref spendIndex, out int manaCost);
                if (item == null)
                {
                    item = placedItems.Count == 0 ? null : placedItems[step % placedItems.Count];
                    manaCost = item == null ? 0 : BattleSandboxManaLoopPreviewBuilder.CalculateManaCost(item);
                }

                if (item != null)
                {
                    BuildSandboxItemStat stat = ResolveStat(item);
                    LegacyItemBehaviorResult behavior =
                        LegacyItemBehaviorIntegrationCatalog.Evaluate(item, stat);
                    float cooldown = ResolveRuntimeCooldown(item, stat);
                    preview.rows.Add(CreateCooldownRow(step, elapsed, item, stat, cooldown, currentMana, playerHp, playerShield, enemyHp, enemyShield, bossCastRemaining, bossCastDuration));

                    if (!behavior.triggers || manaCost <= 0 || currentMana >= manaCost)
                    {
                        int beforeSpend = currentMana;
                        if (behavior.triggers)
                        {
                            currentMana = Mathf.Max(0, currentMana - manaCost);
                        }

                        preview.spentManaTotal += Mathf.Max(0, beforeSpend - currentMana);
                        preview.rows.Add(CreateItemTriggerRow(step, elapsed, item, stat, behavior, manaCost, beforeSpend, currentMana, playerHp, playerShield, enemyHp, enemyShield, bossCastRemaining, bossCastDuration));

                        if (behavior.enemyShieldPressure > 0)
                        {
                            int shieldBefore = enemyShield;
                            enemyShield = Mathf.Max(0, enemyShield - behavior.enemyShieldPressure);
                            preview.rows.Add(CreateEnemyShieldRow(step, elapsed, item, stat, currentMana, playerHp, playerShield, enemyHp, shieldBefore, enemyShield, bossCastRemaining, bossCastDuration));
                        }

                        int incomingDamage = behavior.enemyHpDamage;
                        if (incomingDamage > 0)
                        {
                            int enemyHpBefore = enemyHp;
                            int enemyShieldBefore = enemyShield;
                            BattleSandboxKernelDamageSample damage =
                                BattleSandboxCombatKernelAdapterBuilder.ApplyEnemyDamage(enemyHp, enemyShield, incomingDamage);
                            enemyShield = damage.enemyShieldAfter;
                            enemyHp = Mathf.Max(0, damage.enemyHpAfter);
                            preview.playerItemEnemyHpDamageTotal += Mathf.Max(0, enemyHpBefore - enemyHp);
                            preview.rows.Add(CreateEnemyHpRow(step, elapsed, item, stat, incomingDamage, currentMana, playerHp, playerShield, enemyHpBefore, enemyHp, enemyShieldBefore, enemyShield, bossCastRemaining, bossCastDuration));
                        }

                        if (behavior.playerShieldGain > 0)
                        {
                            int shieldBefore = playerShield;
                            BattleSandboxKernelShieldSample shield =
                                BattleSandboxCombatKernelAdapterBuilder.ApplyPlayerShield(
                                    playerShield,
                                    behavior.playerShieldGain,
                                    BattleSandboxCombatKernelAdapterBuilder.PlayerShieldCap);
                            playerShield = shield.playerShieldAfter;
                            preview.rows.Add(CreatePlayerShieldRow(step, elapsed, item, stat, currentMana, playerHp, shieldBefore, playerShield, enemyHp, enemyShield, bossCastRemaining, bossCastDuration));
                        }

                        if (behavior.cleanseValue > 0)
                        {
                            preview.rows.Add(CreatePlayerCleanseRow(step, elapsed, item, stat, behavior.cleanseValue, currentMana, playerHp, playerShield, enemyHp, enemyShield, bossCastRemaining, bossCastDuration));
                            if (playerHp < playerMaxHp)
                            {
                                int hpBefore = playerHp;
                                BattleSandboxKernelHealingSample healing =
                                    BattleSandboxCombatKernelAdapterBuilder.ApplyHealing(playerHp, behavior.cleanseValue, playerMaxHp);
                                playerHp = healing.playerHpAfter;
                                preview.rows.Add(CreatePlayerHealRow(step, elapsed, item, stat, currentMana, hpBefore, playerHp, playerShield, enemyHp, enemyShield, bossCastRemaining, bossCastDuration));
                            }
                        }

                        if (behavior.controlValue > 0)
                        {
                            float bossCastBeforeControl = bossCastRemaining;
                            bossCastRemaining = Mathf.Min(
                                bossCastDuration,
                                bossCastRemaining + behavior.controlValue * 0.05f);
                            preview.rows.Add(CreateControlRow(step, elapsed, item, stat, behavior.controlValue, currentMana, playerHp, playerShield, enemyHp, enemyShield, bossCastBeforeControl, bossCastRemaining, bossCastDuration));
                        }
                    }
                    else
                    {
                        preview.rows.Add(CreateItemWaitRow(step, elapsed, item, stat, manaCost, currentMana, playerHp, playerShield, enemyHp, enemyShield, bossCastRemaining, bossCastDuration));
                    }
                }

                if (enemyHp <= 0 && !usesExplicitRequest)
                {
                    break;
                }

                if (usesExplicitRequest)
                {
                    int elapsedMilliseconds = Mathf.RoundToInt(elapsed * 1000f);
                    BattleSandboxExplicitDevEnemyActionCue nextCue =
                        explicitCadenceIndex < explicitRequest.AcceptedEnemyActionCadence.Count
                            ? explicitRequest.AcceptedEnemyActionCadence[explicitCadenceIndex]
                            : null;
                    float timerBefore = bossCastRemaining;
                    bossCastRemaining = nextCue == null
                        ? 0f
                        : Mathf.Max(0f, (nextCue.AtMilliseconds - elapsedMilliseconds) / 1000f);
                    preview.rows.Add(CreateBossCastRow(
                        step,
                        elapsed,
                        currentMana,
                        playerHp,
                        playerShield,
                        enemyHp,
                        enemyShield,
                        timerBefore,
                        bossCastRemaining,
                        bossCastDuration));
                    while (nextCue != null && nextCue.AtMilliseconds <= elapsedMilliseconds)
                    {
                        int playerHpBefore = playerHp;
                        int playerShieldBefore = playerShield;
                        int acceptedDamage = nextCue.ActionKind ==
                            BattleSandboxExplicitDevEnemyActionKind.Skill
                                ? explicitRequest.SkillDamage
                                : explicitRequest.BasicAttackDamage;
                        int blocked = Mathf.Min(playerShield, acceptedDamage);
                        playerShield = Mathf.Max(0, playerShield - blocked);
                        int hpDamage = Mathf.Max(0, acceptedDamage - blocked);
                        playerHp = Mathf.Max(0, playerHp - hpDamage);
                        BattleSandboxRuntimeLoopRow acceptedActionRow = CreatePlayerDamageRow(
                            step,
                            nextCue.AtMilliseconds / 1000f,
                            safeScenario,
                            acceptedDamage,
                            blocked,
                            hpDamage,
                            currentMana,
                            playerHpBefore,
                            playerHp,
                            playerShieldBefore,
                            playerShield,
                            enemyHp,
                            enemyShield,
                            bossCastDuration,
                            playerMaxHp);
                        acceptedActionRow.hasAcceptedEnemyAction = true;
                        acceptedActionRow.acceptedEnemyActionKind =
                            nextCue.ActionKind.ToString();
                        acceptedActionRow.acceptedEnemyActionSequence = nextCue.Sequence;
                        acceptedActionRow.acceptedEnemyActionAtMilliseconds =
                            nextCue.AtMilliseconds;
                        preview.rows.Add(acceptedActionRow);
                        explicitCadenceIndex++;
                        nextCue = explicitCadenceIndex
                                  < explicitRequest.AcceptedEnemyActionCadence.Count
                            ? explicitRequest.AcceptedEnemyActionCadence[explicitCadenceIndex]
                            : null;
                        bossCastRemaining = nextCue == null
                            ? 0f
                            : Mathf.Max(
                                0f,
                                (nextCue.AtMilliseconds - elapsedMilliseconds) / 1000f);
                    }
                }
                else
                {
                    float attackTimerBefore = bossCastRemaining;
                    bossCastRemaining = Mathf.Max(0f, bossCastRemaining - StepSeconds);
                    preview.rows.Add(CreateBossCastRow(step, elapsed, currentMana, playerHp, playerShield, enemyHp, enemyShield, attackTimerBefore, bossCastRemaining, bossCastDuration));
                    if (bossCastRemaining <= 0f)
                    {
                        int playerHpBefore = playerHp;
                        int playerShieldBefore = playerShield;
                        int bossDamage = enemyAttackDamage;
                        int blocked = Mathf.Min(playerShield, bossDamage);
                        playerShield = Mathf.Max(0, playerShield - blocked);
                        int hpDamage = Mathf.Max(0, bossDamage - blocked);
                        playerHp = Mathf.Max(0, playerHp - hpDamage);
                        preview.rows.Add(CreatePlayerDamageRow(step, elapsed, safeScenario, bossDamage, blocked, hpDamage, currentMana, playerHpBefore, playerHp, playerShieldBefore, playerShield, enemyHp, enemyShield, bossCastDuration, playerMaxHp));
                        bossCastRemaining = bossCastDuration;
                        if (playerHp <= 0)
                        {
                            break;
                        }
                    }
                }
            }

            int pressureStep = simulationStepCount;
            bool hasPlacedRuntimeItems = placedItems.Count > 0;
            while (!usesExplicitRequest
                   && (!safeScenario.expectsSandboxVictory || !hasPlacedRuntimeItems)
                   && playerHp > 0
                   && pressureStep < simulationStepCount + 8)
            {
                float elapsed = (pressureStep + 1) * StepSeconds;
                int playerHpBefore = playerHp;
                int playerShieldBefore = playerShield;
                int bossDamage = enemyAttackDamage;
                int blocked = Mathf.Min(playerShield, bossDamage);
                playerShield = Mathf.Max(0, playerShield - blocked);
                int hpDamage = Mathf.Max(0, bossDamage - blocked);
                playerHp = Mathf.Max(0, playerHp - hpDamage);
                preview.rows.Add(CreatePlayerDamageRow(pressureStep, elapsed, safeScenario, bossDamage, blocked, hpDamage, currentMana, playerHpBefore, playerHp, playerShieldBefore, playerShield, enemyHp, enemyShield, bossCastDuration, playerMaxHp));
                pressureStep++;
            }

            AddSandboxResultRow(
                preview,
                safeScenario,
                buildCombatPreview,
                currentMana,
                playerHp,
                playerShield,
                enemyHp,
                enemyShield,
                bossCastRemaining,
                bossCastDuration,
                allowScenarioVictory: usesExplicitRequest || hasPlacedRuntimeItems,
                resultStep: simulationStepCount,
                resultElapsedSeconds: usesExplicitRequest
                    ? explicitRequest.TargetDurationMilliseconds / 1000f
                    : (simulationStepCount + 1) * StepSeconds);

            preview.finalMana = currentMana;
            BattleSandboxRuntimeLoopRow resultRow = preview.rows
                .LastOrDefault(row => row != null && (row.hasSandboxVictoryResult || row.hasSandboxDefeatResult));
            preview.hasSandboxVictoryResult = resultRow?.hasSandboxVictoryResult == true;
            preview.hasSandboxDefeatResult = resultRow?.hasSandboxDefeatResult == true;
            preview.finalPlayerHp = Mathf.Max(0, resultRow?.playerHpAfter ?? playerHp);
            preview.finalPlayerShield = playerShield;
            preview.finalEnemyHp = Mathf.Max(0, resultRow?.enemyHpAfter ?? enemyHp);
            preview.finalEnemyShield = enemyShield;
            return preview;
        }

        public static IReadOnlyList<BattleSandboxRuntimeLoopScenario> BuildDevEnemyScenarios()
        {
            List<BattleSandboxRuntimeLoopScenario> scenarios = new();
            EnemyBossValidationPool enemyBossPool = EnemyBossValidationPool.CreateDefault();
            DevChapterBalanceRun balanceRun = DevChapterBalanceRunBuilder.BuildDefaultRun();
            foreach (DevChapterBalanceRunStage stage in balanceRun?.stages ?? new List<DevChapterBalanceRunStage>())
            {
                if (stage == null
                    || (stage.devChapterLabel != "3-10" && stage.devChapterLabel != "4-10"))
                {
                    continue;
                }

                scenarios.Add(BuildScenarioFromStage(stage, enemyBossPool));
            }

            if (scenarios.Count > 0)
            {
                return scenarios;
            }

            return new[]
            {
                BuildFallbackScenario(
                    "dev_balance_3_10_fallback",
                    "3-10",
                    "dev_boss_burst_huzhen",
                    "\u4e09\u4e4b\u5341\u6d4b\u8bd5\u654c\u4eba",
                    0.45f),
                BuildFallbackScenario(
                    "dev_balance_4_10_fallback",
                    "4-10",
                    "dev_boss_hybrid_combo",
                    "\u56db\u4e4b\u5341\u6d4b\u8bd5\u654c\u4eba",
                    0.62f)
            };
        }

        public static BattleSandboxRuntimeLoopScenario ResolveDevEnemyScenario(int selectedIndex)
        {
            IReadOnlyList<BattleSandboxRuntimeLoopScenario> scenarios = BuildDevEnemyScenarios();
            if (scenarios.Count == 0)
            {
                return null;
            }

            int index = Mathf.Abs(selectedIndex) % scenarios.Count;
            return scenarios[index];
        }

        private static BattleSandboxRuntimeLoopScenario BuildScenarioFromStage(
            DevChapterBalanceRunStage stage,
            EnemyBossValidationPool enemyBossPool)
        {
            BuildSandboxBossProfile bossProfile = enemyBossPool?.FindBoss(stage?.bossProfileId);
            bool fromDevOnlyProfile = bossProfile != null
                && bossProfile.devOnly
                && !bossProfile.isEnabled
                && !bossProfile.entersFormalFlow
                && !bossProfile.referencesFormalBossPool
                && !bossProfile.referencesFormalEnemyPool;
            return new BattleSandboxRuntimeLoopScenario
            {
                stageId = stage?.stageId ?? string.Empty,
                devChapterLabel = stage?.devChapterLabel ?? string.Empty,
                previewBuildId = stage?.previewBuildId ?? string.Empty,
                enemyDisplayNameChinese = string.IsNullOrWhiteSpace(stage?.bossProblemDisplayNameChinese)
                    ? "\u6d4b\u8bd5\u654c\u4eba"
                    : stage.bossProblemDisplayNameChinese,
                simulatedWinRate = stage?.simulatedWinRate ?? 0f,
                expectsSandboxVictory = (stage?.simulatedWinRate ?? 0f) >= 0.5f,
                devOnlyProfileId = bossProfile?.bossId ?? stage?.bossProfileId ?? string.Empty,
                attackSourcePath = SourceDevEnemyProfileAttack,
                attackDamage = Mathf.Max(1, bossProfile?.attackDamage ?? 8),
                attackIntervalSeconds = Mathf.Max(0.5f, bossProfile?.attackIntervalSeconds ?? 2.4f),
                attackFromDevOnlyProfile = fromDevOnlyProfile,
                playerMechanicFeedbackChinese = SafePlayerFeedback(
                    stage?.bossMechanicFeedbackChinese,
                    "\u9996\u9886\u673a\u5236\u5df2\u5207\u6362\uff0c\u7559\u610f\u72b6\u6001\u4e0e\u65bd\u6cd5\u8282\u594f\u3002"),
                playerBuildPressureChinese = BuildPlayerPressureFeedback(stage)
            };
        }

        private static BattleSandboxRuntimeLoopScenario BuildFallbackScenario(
            string stageId,
            string devChapterLabel,
            string bossProfileId,
            string enemyDisplayNameChinese,
            float simulatedWinRate)
        {
            EnemyBossValidationPool pool = EnemyBossValidationPool.CreateDefault();
            BuildSandboxBossProfile bossProfile = pool.FindBoss(bossProfileId);
            return new BattleSandboxRuntimeLoopScenario
            {
                stageId = stageId ?? string.Empty,
                devChapterLabel = devChapterLabel ?? string.Empty,
                previewBuildId = stageId ?? string.Empty,
                enemyDisplayNameChinese = enemyDisplayNameChinese ?? string.Empty,
                simulatedWinRate = simulatedWinRate,
                expectsSandboxVictory = simulatedWinRate >= 0.5f,
                devOnlyProfileId = bossProfile?.bossId ?? bossProfileId ?? string.Empty,
                attackSourcePath = SourceDevEnemyProfileAttack,
                attackDamage = Mathf.Max(1, bossProfile?.attackDamage ?? 8),
                attackIntervalSeconds = Mathf.Max(0.5f, bossProfile?.attackIntervalSeconds ?? 2.4f),
                attackFromDevOnlyProfile = bossProfile != null
                    && bossProfile.devOnly
                    && !bossProfile.isEnabled
                    && !bossProfile.entersFormalFlow
                    && !bossProfile.referencesFormalBossPool
                    && !bossProfile.referencesFormalEnemyPool,
                playerMechanicFeedbackChinese = "\u9996\u9886\u673a\u5236\u5df2\u5207\u6362\uff0c\u7559\u610f\u72b6\u6001\u4e0e\u65bd\u6cd5\u8282\u594f\u3002",
                playerBuildPressureChinese = "\u5efa\u8bae\u89c2\u5bdf\u4f9b\u80fd\u3001\u62a4\u76fe\u548c\u6301\u7eed\u538b\u5236\u7684\u7a33\u5b9a\u6027\u3002"
            };
        }

        private static string BuildPlayerPressureFeedback(DevChapterBalanceRunStage stage)
        {
            string difficulty = string.IsNullOrWhiteSpace(stage?.difficultyTendencyChinese)
                ? "\u672a\u5b9a"
                : stage.difficultyTendencyChinese;
            string playerFeedback = SafePlayerFeedback(
                stage?.playerBattleFeedbackChinese,
                "\u5efa\u8bae\u89c2\u5bdf\u4f9b\u80fd\u3001\u62a4\u76fe\u548c\u6301\u7eed\u538b\u5236\u7684\u7a33\u5b9a\u6027\u3002");
            return $"{difficulty}\u3002{playerFeedback}";
        }

        private static string SafePlayerFeedback(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        private static BuildSandboxLayoutSnapshot NormalizeSnapshot(
            BuildSandboxLayoutSnapshot snapshot,
            bool allowDefaultLayoutFallbackWhenBoardEmpty,
            out bool usedFallback)
        {
            usedFallback = false;
            BuildSandboxLayoutSnapshot safeSnapshot = snapshot ?? new BuildSandboxLayoutSnapshot();
            if ((safeSnapshot.placedItems == null || safeSnapshot.placedItems.Count == 0)
                && allowDefaultLayoutFallbackWhenBoardEmpty)
            {
                usedFallback = true;
                safeSnapshot = BattleSandboxBuildCombatPreviewBuilder.BuildDefaultPreviewLayoutSnapshot();
            }

            BattleSandboxBuildCombatPreviewBuilder.ApplyPreviewEnergyLinks(safeSnapshot);
            return safeSnapshot;
        }

        private static List<BuildSandboxPlacedItemSnapshot> NormalizeItems(
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> placedItems)
        {
            List<BuildSandboxPlacedItemSnapshot> result = new();
            foreach (BuildSandboxPlacedItemSnapshot item in placedItems ?? Array.Empty<BuildSandboxPlacedItemSnapshot>())
            {
                if (item == null)
                {
                    continue;
                }

                item.itemStat = ResolveStat(item);
                result.Add(item);
            }

            return result;
        }

        private static BuildSandboxItemStat ResolveStat(BuildSandboxPlacedItemSnapshot item)
        {
            return BuildSandboxItemStatCatalog.ResolveFrom(item?.itemStat, item?.itemId);
        }

        private static BuildSandboxPlacedItemSnapshot FindNextPassiveEffectItem(
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> placedItems,
            ref int passiveEffectIndex)
        {
            if (placedItems == null || placedItems.Count == 0)
            {
                return null;
            }

            for (int scanned = 0; scanned < placedItems.Count; scanned++)
            {
                int index = Mathf.Abs(passiveEffectIndex + scanned) % placedItems.Count;
                BuildSandboxPlacedItemSnapshot item = placedItems[index];
                BuildSandboxItemStat stat = ResolveStat(item);
                BattleSandboxItemEffectRuntimeProfile profile =
                    BattleSandboxItemEffectRuntimePreviewCatalog.Resolve(item, stat);
                if (profile.restoresMana)
                {
                    continue;
                }

                if (BattleSandboxItemEffectRuntimePreviewCatalog.IsPassiveRuntimeEffect(item, stat))
                {
                    passiveEffectIndex = index + 1;
                    return item;
                }
            }

            return null;
        }

        private static BuildSandboxPlacedItemSnapshot FindNextManaProviderEffectItem(
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> placedItems,
            ref int manaProviderEffectIndex)
        {
            if (placedItems == null || placedItems.Count == 0)
            {
                return null;
            }

            for (int scanned = 0; scanned < placedItems.Count; scanned++)
            {
                int index = Mathf.Abs(manaProviderEffectIndex + scanned) % placedItems.Count;
                BuildSandboxPlacedItemSnapshot item = placedItems[index];
                BuildSandboxItemStat stat = ResolveStat(item);
                if (item == null
                    || item.energyState != EnergyState.Powered
                    || !FormationEnergyContractResolver.IsEnergyStoneItem(item)
                    || Mathf.Max(0, stat.manaGainPerTick) <= 0)
                {
                    continue;
                }

                manaProviderEffectIndex = index + 1;
                return item;
            }

            return null;
        }

        private static BuildPassiveFeedbackSource FindNextBuildPassiveFeedbackSource(
            BattleSandboxBuildCombatPreview buildCombatPreview,
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> placedItems,
            ref int buildPassiveEffectIndex)
        {
            List<BuildPassiveFeedbackSource> sources =
                BuildBuildPassiveFeedbackSources(buildCombatPreview, placedItems);
            if (sources.Count == 0)
            {
                return null;
            }

            int index = Mathf.Abs(buildPassiveEffectIndex) % sources.Count;
            buildPassiveEffectIndex = index + 1;
            return sources[index];
        }

        private static List<BuildPassiveFeedbackSource> BuildBuildPassiveFeedbackSources(
            BattleSandboxBuildCombatPreview buildCombatPreview,
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> placedItems)
        {
            List<BuildPassiveFeedbackSource> sources = new();
            foreach (BuildModifierPreview modifier in buildCombatPreview?.context?.modifierBundle?.modifiers
                         ?? Enumerable.Empty<BuildModifierPreview>())
            {
                if (modifier == null
                    || !modifier.devOnly
                    || modifier.isEnabled
                    || modifier.affectsFormalCombat
                    || !TryResolveBuildPassiveFeedback(modifier, out string kind, out string text, out int value))
                {
                    continue;
                }

                BuildSandboxPlacedItemSnapshot item =
                    ResolveBuildPassiveSourceItem(modifier.sourceItem, placedItems, sources.Count);
                if (item == null)
                {
                    continue;
                }

                sources.Add(new BuildPassiveFeedbackSource
                {
                    item = item,
                    feedbackKind = kind,
                    feedbackTextChinese = text,
                    feedbackValue = value,
                    sourceDataPath = SourceBuildCombatPreview + ".context.modifierBundle." + modifier.modifierType
                });
            }

            return sources;
        }

        private static bool TryResolveBuildPassiveFeedback(
            BuildModifierPreview modifier,
            out string kind,
            out string text,
            out int value)
        {
            value = Mathf.Max(1, Mathf.RoundToInt(Mathf.Abs(modifier?.previewValue ?? 0f) * 100f));
            switch (modifier?.modifierType ?? string.Empty)
            {
                case ModifierEventBridge.DamageBonus:
                    kind = "damageBoost";
                    text = $"伤害强化 +{value}%";
                    return true;
                case ModifierEventBridge.CooldownBonus:
                    kind = "cooldownBoost";
                    text = $"冷却加速 +{value}%";
                    return true;
                case ModifierEventBridge.ShieldBreakBonus:
                    kind = "shieldBreakBoost";
                    text = $"破盾强化 +{value}%";
                    return true;
                case ModifierEventBridge.ShieldBonus:
                    kind = "shieldBoost";
                    text = $"护盾强化 +{value}%";
                    return true;
                case ModifierEventBridge.CleanseBonus:
                    kind = "cleanseBoost";
                    text = $"净化强化 +{value}%";
                    return true;
                case ModifierEventBridge.ControlDurationBonus:
                    kind = "controlBoost";
                    text = $"镇压延长 +{value}%";
                    return true;
                case ModifierEventBridge.EnergyReturnBonus:
                    kind = "manaBoost";
                    text = $"灵力加速 +{value}%";
                    return true;
                default:
                    kind = string.Empty;
                    text = string.Empty;
                    return false;
            }
        }

        private static BuildSandboxPlacedItemSnapshot ResolveBuildPassiveSourceItem(
            string sourceItemIds,
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> placedItems,
            int seed)
        {
            if (placedItems == null || placedItems.Count == 0)
            {
                return null;
            }

            List<string> ids = SplitBuildPassiveSourceItemIds(sourceItemIds);
            if (ids.Count == 0)
            {
                return null;
            }

            for (int scanned = 0; scanned < ids.Count; scanned++)
            {
                string id = ids[Mathf.Abs(seed + scanned) % ids.Count];
                BuildSandboxPlacedItemSnapshot item = placedItems.FirstOrDefault(candidate =>
                    string.Equals(candidate?.itemId, id, StringComparison.Ordinal));
                if (item != null)
                {
                    return item;
                }
            }

            return null;
        }

        private static List<string> SplitBuildPassiveSourceItemIds(string sourceItemIds)
        {
            return (sourceItemIds ?? string.Empty)
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(value => value.Trim())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToList();
        }

        private static float ResolveRuntimeCooldown(
            BuildSandboxPlacedItemSnapshot item,
            BuildSandboxItemStat stat)
        {
            BuildSandboxItemStat safeStat = stat ?? BuildSandboxItemStatCatalog.Resolve(item?.itemId);
            bool isFormallyPowered = IsFormallyPowered(item);
            bool fireAdjacentSpirit = (item?.itemId ?? string.Empty).IndexOf("fire", StringComparison.OrdinalIgnoreCase) >= 0
                && isFormallyPowered;
            bool qiPillAdjacentWater = (safeStat.statTag ?? string.Empty).IndexOf("cleanse", StringComparison.OrdinalIgnoreCase) >= 0
                && isFormallyPowered;
            bool weakPowered = item != null && item.energyState == EnergyState.WeakPulse && safeStat.spirit > safeStat.attack;
            float computedCooldown = isFormallyPowered
                ? Mathf.Max(BattleSandboxCombatKernelAdapterBuilder.MinCooldown, safeStat.castIntervalSeconds - 0.2f)
                : safeStat.castIntervalSeconds;
            return BattleSandboxCombatKernelAdapterBuilder.ResolveEffectiveCooldown(
                safeStat.castIntervalSeconds,
                computedCooldown,
                fireAdjacentSpirit,
                qiPillAdjacentWater,
                weakPowered);
        }

        private static int ResolveEnemyAttackDamage(BattleSandboxRuntimeLoopScenario scenario)
        {
            return Mathf.Max(1, scenario?.attackDamage ?? 1);
        }

        private static float ResolveEnemyAttackIntervalSeconds(BattleSandboxRuntimeLoopScenario scenario)
        {
            float interval = scenario?.attackIntervalSeconds ?? 2.4f;
            if (float.IsNaN(interval) || float.IsInfinity(interval) || interval <= 0f)
            {
                interval = 2.4f;
            }

            return Mathf.Max(0.5f, interval);
        }

        private static int ResolveItemEnemyDamage(
            BuildSandboxPlacedItemSnapshot item,
            BuildSandboxItemStat stat)
        {
            return LegacyItemBehaviorIntegrationCatalog.Evaluate(item, stat).enemyHpDamage;
        }

        private static bool IsFormallyPowered(BuildSandboxPlacedItemSnapshot item)
        {
            return item != null && item.energyState == EnergyState.Powered;
        }

        private static bool CanItemDealEnemyDamage(
            BuildSandboxPlacedItemSnapshot item,
            BuildSandboxItemStat stat)
        {
            return BattleSandboxItemEffectRuntimePreviewCatalog.CanDealEnemyDamage(item, stat);
        }

        private static bool ContainsAny(string value, params string[] tokens)
        {
            string safeValue = value ?? string.Empty;
            foreach (string token in tokens ?? Array.Empty<string>())
            {
                if (!string.IsNullOrWhiteSpace(token)
                    && safeValue.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static int ResolveEnemyMaxHp(
            BattleSandboxCombatKernelAdapterPreview adapter,
            int placedItemCount,
            BattleSandboxRuntimeLoopScenario scenario)
        {
            int baseHp = Mathf.Max(80, (adapter?.sample?.enemyHp?.maxHp ?? 120) + placedItemCount * 6);
            if (string.Equals(scenario?.devChapterLabel, "4-10", StringComparison.Ordinal))
            {
                baseHp += 20;
            }

            if ((scenario?.simulatedWinRate ?? 1f) < 0.5f)
            {
                baseHp += 12;
            }

            return baseHp;
        }

        private static int ResolveEnemyShield(
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> placedItems,
            BattleSandboxRuntimeLoopScenario scenario)
        {
            int shield = Mathf.Max(8, placedItems.Sum(item => ResolveStat(item).shieldBreak) + 10);
            if (string.Equals(scenario?.devChapterLabel, "4-10", StringComparison.Ordinal))
            {
                shield += 6;
            }

            return shield;
        }

        private static void AddOpeningRow(
            BattleSandboxRuntimeLoopPreview preview,
            BattleSandboxRuntimeLoopScenario scenario,
            int mana,
            int playerHp,
            int playerShield,
            int enemyHp,
            int enemyShield,
            float bossCastRemaining,
            float bossCastDuration)
        {
            preview.rows.Add(BaseRow(
                "opening",
                "combatLog",
                0,
                0f,
                mana,
                mana,
                playerHp,
                playerHp,
                playerShield,
                playerShield,
                enemyHp,
                enemyHp,
                enemyShield,
                enemyShield,
                bossCastRemaining,
                bossCastDuration,
                "\u6c99\u76d2\u5faa\u73af\uff1a\u6218\u6597\u6001\u542f\u52a8",
                "\u9996\u9886\u6b63\u5728\u84c4\u529b",
                $"\u3010\u6c99\u76d2\u3011{FormatMechanicFeedback(scenario)}\uff1b\u6784\u7b51\u538b\u529b\uff1a{FormatBuildPressureFeedback(scenario)}",
                "\u673a\u5236\u53cd\u9988",
                SourceRuntimeLoop));
        }

        private static BattleSandboxRuntimeLoopRow CreateManaRow(
            int step,
            float elapsed,
            int manaBefore,
            int manaAfter,
            int playerHp,
            int playerShield,
            int enemyHp,
            int enemyShield,
            float bossCastRemaining,
            float bossCastDuration)
        {
            int gain = Mathf.Max(0, manaAfter - manaBefore);
            BattleSandboxRuntimeLoopRow row = BaseRow(
                "mana",
                "mana",
                step,
                elapsed,
                manaBefore,
                manaAfter,
                playerHp,
                playerHp,
                playerShield,
                playerShield,
                enemyHp,
                enemyHp,
                enemyShield,
                enemyShield,
                bossCastRemaining,
                bossCastDuration,
                "\u7075\u529b\uff1a\u56de\u6d41\u4e2d",
                "\u9996\u9886\u6b63\u5728\u84c4\u529b",
                $"\u3010\u7075\u529b\u3011+{gain}",
                gain > 0 ? $"\u7075\u6c14 +{gain}" : "\u7075\u529b\u5df2\u6ee1",
                SourceItemStat + ".manaGainPerTick");
            row.manaDelta = manaAfter - manaBefore;
            return row;
        }

        private static BattleSandboxRuntimeLoopRow CreateCooldownRow(
            int step,
            float elapsed,
            BuildSandboxPlacedItemSnapshot item,
            BuildSandboxItemStat stat,
            float cooldown,
            int mana,
            int playerHp,
            int playerShield,
            int enemyHp,
            int enemyShield,
            float bossCastRemaining,
            float bossCastDuration)
        {
            BattleSandboxItemEffectRuntimeProfile profile =
                BattleSandboxItemEffectRuntimePreviewCatalog.Resolve(item, stat);
            string displayName = DisplayItem(item, stat);
            BattleSandboxRuntimeLoopRow row = BaseItemRow(
                "cooldown",
                step,
                elapsed,
                item,
                stat,
                mana,
                mana,
                playerHp,
                playerHp,
                playerShield,
                playerShield,
                enemyHp,
                enemyHp,
                enemyShield,
                enemyShield,
                bossCastRemaining,
                bossCastDuration,
                $"\u51b7\u5374\uff1a{profile.effectFamilyChinese}\u51c6\u5907",
                $"\u9996\u9886\u84c4\u529b {bossCastRemaining:0.0}\u79d2",
                $"\u3010\u51b7\u5374\u3011{displayName} {cooldown:0.0}\u79d2\uff0c{profile.effectRoleChinese}\u5f85\u53d1",
                $"{profile.effectFamilyChinese}\u5f85\u53d1",
                SourceCombatKernel + ".ResolveEffectiveCooldown+" + profile.SourceDataPath);
            row.cooldownSeconds = cooldown;
            return row;
        }

        private static BattleSandboxRuntimeLoopRow CreateItemTriggerRow(
            int step,
            float elapsed,
            BuildSandboxPlacedItemSnapshot item,
            BuildSandboxItemStat stat,
            LegacyItemBehaviorResult behavior,
            int manaCost,
            int manaBefore,
            int manaAfter,
            int playerHp,
            int playerShield,
            int enemyHp,
            int enemyShield,
            float bossCastRemaining,
            float bossCastDuration)
        {
            BattleSandboxItemEffectRuntimeProfile profile =
                BattleSandboxItemEffectRuntimePreviewCatalog.Resolve(item, stat);
            LegacyItemBehaviorResult safeBehavior =
                behavior ?? LegacyItemBehaviorIntegrationCatalog.Evaluate(item, stat);
            string displayName = DisplayItem(item, stat);
            int displayedManaCost = safeBehavior.triggers ? manaCost : 0;
            BattleSandboxRuntimeLoopRow row = BaseItemRow(
                "itemTrigger",
                step,
                elapsed,
                item,
                stat,
                manaBefore,
                manaAfter,
                playerHp,
                playerHp,
                playerShield,
                playerShield,
                enemyHp,
                enemyHp,
                enemyShield,
                enemyShield,
                bossCastRemaining,
                bossCastDuration,
                safeBehavior.playerFeedbackChinese,
                profile.triggerCastLineChinese,
                $"{safeBehavior.combatLogChinese}\uff0c\u8017\u7075 {displayedManaCost}",
                safeBehavior.triggers ? profile.triggerFloatingTextChinese : safeBehavior.playerFeedbackChinese,
                SourceItemStat + ".manaCostPerCast+" + safeBehavior.sourceDataPath);
            row.manaDelta = manaAfter - manaBefore;
            row.itemTriggerChinese = safeBehavior.triggers
                ? $"{profile.effectRoleChinese}\u5df2\u751f\u6548"
                : safeBehavior.playerFeedbackChinese;
            if (!safeBehavior.triggers)
            {
                ApplyBoardItemTriggerFeedback(
                    row,
                    safeBehavior.energyState == EnergyState.Suppressed ? "suppressed" : "notPowered",
                    0,
                    safeBehavior.energyState == EnergyState.Suppressed ? "被压制" : "未供能");
            }
            else if (safeBehavior.energyState == EnergyState.WeakPulse)
            {
                ApplyBoardItemTriggerFeedback(row, "weakPulse", 0, "弱脉冲触发");
            }

            return row;
        }

        private static BattleSandboxRuntimeLoopRow CreatePassiveEffectRow(
            int step,
            float elapsed,
            BuildSandboxPlacedItemSnapshot item,
            BuildSandboxItemStat stat,
            LegacyItemBehaviorResult behavior,
            int manaBefore,
            int manaAfter,
            int playerHp,
            int playerShield,
            int enemyHp,
            int enemyShield,
            float bossCastRemaining,
            float bossCastDuration)
        {
            BattleSandboxItemEffectRuntimeProfile profile =
                BattleSandboxItemEffectRuntimePreviewCatalog.Resolve(item, stat);
            LegacyItemBehaviorResult safeBehavior =
                behavior ?? LegacyItemBehaviorIntegrationCatalog.Evaluate(item, stat);
            string displayName = DisplayItem(item, stat);
            int manaGain = Mathf.Max(0, manaAfter - manaBefore);
            string log = !safeBehavior.triggers
                ? safeBehavior.combatLogChinese
                : profile.restoresMana && manaGain > 0
                ? $"\u3010{profile.effectFamilyChinese}\u3011{displayName}{profile.triggerVerbChinese}\uff0c\u7075\u6c14 +{manaGain}"
                : profile.passiveLogChinese;
            string floating = !safeBehavior.triggers
                ? safeBehavior.playerFeedbackChinese
                : profile.restoresMana && manaGain > 0
                ? FormatManaRestoreFloatingText(profile, manaGain)
                : profile.passiveFloatingTextChinese;
            BattleSandboxRuntimeLoopRow row = BaseItemRow(
                "itemTrigger",
                step,
                elapsed,
                item,
                stat,
                manaBefore,
                manaAfter,
                playerHp,
                playerHp,
                playerShield,
                playerShield,
                enemyHp,
                enemyHp,
                enemyShield,
                enemyShield,
                bossCastRemaining,
                bossCastDuration,
                safeBehavior.playerFeedbackChinese,
                profile.triggerCastLineChinese,
                log,
                floating,
                SourceItemStat + ".manaGainPerTick+" + safeBehavior.sourceDataPath);
            row.manaDelta = manaAfter - manaBefore;
            row.itemTriggerChinese = safeBehavior.triggers
                ? $"{profile.effectRoleChinese}\u5df2\u751f\u6548"
                : safeBehavior.playerFeedbackChinese;
            if (profile.restoresMana && manaGain > 0)
            {
                ApplyBoardItemTriggerFeedback(row, "mana", manaGain, $"+{manaGain} 回灵", "passive");
            }
            else if (!safeBehavior.triggers)
            {
                ApplyBoardItemTriggerFeedback(row, "notPowered", 0, "未供能", "passive");
            }

            return row;
        }

        private static BattleSandboxRuntimeLoopRow CreateBuildPassiveFeedbackRow(
            int step,
            float elapsed,
            BuildPassiveFeedbackSource source,
            int mana,
            int playerHp,
            int playerShield,
            int enemyHp,
            int enemyShield,
            float bossCastRemaining,
            float bossCastDuration)
        {
            BuildSandboxPlacedItemSnapshot item = source?.item;
            BuildSandboxItemStat stat = ResolveStat(item);
            string displayName = DisplayItem(item, stat);
            string feedback = source?.feedbackTextChinese ?? string.Empty;
            BattleSandboxRuntimeLoopRow row = BaseItemRow(
                "itemTrigger",
                step,
                elapsed,
                item,
                stat,
                mana,
                mana,
                playerHp,
                playerHp,
                playerShield,
                playerShield,
                enemyHp,
                enemyHp,
                enemyShield,
                enemyShield,
                bossCastRemaining,
                bossCastDuration,
                $"被动：{feedback}",
                "构筑被动生效",
                $"【被动】{displayName} {feedback}",
                feedback,
                string.IsNullOrWhiteSpace(source?.sourceDataPath)
                    ? SourceBuildCombatPreview + ".context.modifierBundle"
                    : source.sourceDataPath);
            row.itemTriggerChinese = "被动已生效";
            ApplyBoardItemTriggerFeedback(
                row,
                source?.feedbackKind,
                source?.feedbackValue ?? 0,
                feedback,
                "passive");
            return row;
        }

        private static string FormatManaRestoreFloatingText(
            BattleSandboxItemEffectRuntimeProfile profile,
            int manaGain)
        {
            string prefix = (profile?.passiveFloatingTextChinese ?? string.Empty).Trim();
            if (manaGain <= 0)
            {
                return prefix;
            }

            if (string.IsNullOrWhiteSpace(prefix))
            {
                prefix = "\u7075\u6c14";
            }

            while (prefix.EndsWith("+", StringComparison.Ordinal))
            {
                prefix = prefix.Substring(0, prefix.Length - 1).TrimEnd();
            }

            return $"{prefix} +{manaGain}";
        }

        private static BattleSandboxRuntimeLoopRow CreateEnemyHpRow(
            int step,
            float elapsed,
            BuildSandboxPlacedItemSnapshot item,
            BuildSandboxItemStat stat,
            int incomingDamage,
            int mana,
            int playerHp,
            int playerShield,
            int enemyHpBefore,
            int enemyHpAfter,
            int enemyShieldBefore,
            int enemyShieldAfter,
            float bossCastRemaining,
            float bossCastDuration)
        {
            BattleSandboxItemEffectRuntimeProfile profile =
                BattleSandboxItemEffectRuntimePreviewCatalog.Resolve(item, stat);
            string displayName = DisplayItem(item, stat);
            int hpDamage = Mathf.Max(0, enemyHpBefore - enemyHpAfter);
            BattleSandboxRuntimeLoopRow row = BaseItemRow(
                "enemyHp",
                step,
                elapsed,
                item,
                stat,
                mana,
                mana,
                playerHp,
                playerHp,
                playerShield,
                playerShield,
                enemyHpBefore,
                enemyHpAfter,
                enemyShieldBefore,
                enemyShieldAfter,
                bossCastRemaining,
                bossCastDuration,
                profile.damageStateLineChinese,
                profile.damageCastLineChinese,
                $"\u3010{profile.effectFamilyChinese}\u3011{displayName}\u9020\u6210 {incomingDamage} \u70b9{profile.effectRoleChinese}\u538b\u529b\uff0c\u654c\u65b9 {enemyHpAfter}/{Mathf.Max(1, enemyHpBefore)}",
                $"{profile.damageFloatingPrefixChinese} -{hpDamage}",
                SourceCombatKernel + ".ApplyEnemyDamage+" + profile.SourceDataPath);
            ApplyBoardItemTriggerFeedback(row, "damage", hpDamage, BuildDamageFeedbackText(profile, hpDamage));
            return row;
        }

        private static BattleSandboxRuntimeLoopRow CreateEnemyShieldRow(
            int step,
            float elapsed,
            BuildSandboxPlacedItemSnapshot item,
            BuildSandboxItemStat stat,
            int mana,
            int playerHp,
            int playerShield,
            int enemyHp,
            int enemyShieldBefore,
            int enemyShieldAfter,
            float bossCastRemaining,
            float bossCastDuration)
        {
            BattleSandboxItemEffectRuntimeProfile profile =
                BattleSandboxItemEffectRuntimePreviewCatalog.Resolve(item, stat);
            string displayName = DisplayItem(item, stat);
            int shieldBreak = Mathf.Max(0, enemyShieldBefore - enemyShieldAfter);
            BattleSandboxRuntimeLoopRow row = BaseItemRow(
                "enemyShield",
                step,
                elapsed,
                item,
                stat,
                mana,
                mana,
                playerHp,
                playerHp,
                playerShield,
                playerShield,
                enemyHp,
                enemyHp,
                enemyShieldBefore,
                enemyShieldAfter,
                bossCastRemaining,
                bossCastDuration,
                profile.breaksShield ? "敌人：护势被打裂" : "敌人：护势承压",
                "首领护势被压低",
                $"【破盾】{displayName}触发后，敌盾 {enemyShieldBefore}->{enemyShieldAfter}",
                $"破盾 +{shieldBreak}",
                SourceCombatKernel + ".ApplyEnemyShieldPressure+" + profile.SourceDataPath);
            ApplyBoardItemTriggerFeedback(row, "shieldBreak", shieldBreak, $"破盾 +{shieldBreak}");
            return row;
        }

        private static BattleSandboxRuntimeLoopRow CreatePlayerShieldRow(
            int step,
            float elapsed,
            BuildSandboxPlacedItemSnapshot item,
            BuildSandboxItemStat stat,
            int mana,
            int playerHp,
            int playerShieldBefore,
            int playerShieldAfter,
            int enemyHp,
            int enemyShield,
            float bossCastRemaining,
            float bossCastDuration)
        {
            BattleSandboxItemEffectRuntimeProfile profile =
                BattleSandboxItemEffectRuntimePreviewCatalog.Resolve(item, stat);
            string displayName = DisplayItem(item, stat);
            int shieldGain = Mathf.Max(0, playerShieldAfter - playerShieldBefore);
            BattleSandboxRuntimeLoopRow row = BaseItemRow(
                "playerShield",
                step,
                elapsed,
                item,
                stat,
                mana,
                mana,
                playerHp,
                playerHp,
                playerShieldBefore,
                playerShieldAfter,
                enemyHp,
                enemyHp,
                enemyShield,
                enemyShield,
                bossCastRemaining,
                bossCastDuration,
                profile.shieldStateLineChinese,
                "\u9996\u9886\u653b\u52bf\u88ab\u9876\u4f4f",
                $"\u3010{profile.effectFamilyChinese}\u3011{displayName}{profile.triggerVerbChinese}\uff0c\u62a4\u76fe {playerShieldBefore}->{playerShieldAfter}",
                $"{profile.shieldFloatingPrefixChinese} +{shieldGain}",
                SourceCombatKernel + ".ApplyPlayerShield+" + profile.SourceDataPath);
            ApplyBoardItemTriggerFeedback(row, "shield", shieldGain, $"+{shieldGain} 护盾");
            return row;
        }

        private static BattleSandboxRuntimeLoopRow CreatePlayerHealRow(
            int step,
            float elapsed,
            BuildSandboxPlacedItemSnapshot item,
            BuildSandboxItemStat stat,
            int mana,
            int playerHpBefore,
            int playerHpAfter,
            int playerShield,
            int enemyHp,
            int enemyShield,
            float bossCastRemaining,
            float bossCastDuration)
        {
            BattleSandboxItemEffectRuntimeProfile profile =
                BattleSandboxItemEffectRuntimePreviewCatalog.Resolve(item, stat);
            string displayName = DisplayItem(item, stat);
            int hpGain = Mathf.Max(0, playerHpAfter - playerHpBefore);
            BattleSandboxRuntimeLoopRow row = BaseItemRow(
                "playerHp",
                step,
                elapsed,
                item,
                stat,
                mana,
                mana,
                playerHpBefore,
                playerHpAfter,
                playerShield,
                playerShield,
                enemyHp,
                enemyHp,
                enemyShield,
                enemyShield,
                bossCastRemaining,
                bossCastDuration,
                profile.healStateLineChinese,
                "\u9996\u9886\u84c4\u529b\u4e2d",
                $"\u3010{profile.effectFamilyChinese}\u3011{displayName}{profile.triggerVerbChinese}\uff0c\u6c14\u8840 {playerHpBefore}->{playerHpAfter}",
                $"{profile.healFloatingPrefixChinese} +{hpGain}",
                SourceCombatKernel + ".ApplyHealing+" + profile.SourceDataPath);
            ApplyBoardItemTriggerFeedback(row, "heal", hpGain, $"+{hpGain} 气血");
            return row;
        }

        private static BattleSandboxRuntimeLoopRow CreatePlayerCleanseRow(
            int step,
            float elapsed,
            BuildSandboxPlacedItemSnapshot item,
            BuildSandboxItemStat stat,
            int cleanseValue,
            int mana,
            int playerHp,
            int playerShield,
            int enemyHp,
            int enemyShield,
            float bossCastRemaining,
            float bossCastDuration)
        {
            BattleSandboxItemEffectRuntimeProfile profile =
                BattleSandboxItemEffectRuntimePreviewCatalog.Resolve(item, stat);
            string displayName = DisplayItem(item, stat);
            int safeCleanse = Mathf.Max(0, cleanseValue);
            BattleSandboxRuntimeLoopRow row = BaseItemRow(
                "cleanse",
                step,
                elapsed,
                item,
                stat,
                mana,
                mana,
                playerHp,
                playerHp,
                playerShield,
                playerShield,
                enemyHp,
                enemyHp,
                enemyShield,
                enemyShield,
                bossCastRemaining,
                bossCastDuration,
                "玩家：浊气被清掉",
                "首领蓄力未断",
                $"【净化】{displayName}清除浊气 +{safeCleanse}",
                $"净化 +{safeCleanse}",
                SourceCombatKernel + ".ApplyCleansePreview+" + profile.SourceDataPath);
            row.itemTriggerChinese = "净化已生效";
            ApplyBoardItemTriggerFeedback(row, "cleanse", safeCleanse, $"净化 +{safeCleanse}");
            return row;
        }

        private static BattleSandboxRuntimeLoopRow CreateControlRow(
            int step,
            float elapsed,
            BuildSandboxPlacedItemSnapshot item,
            BuildSandboxItemStat stat,
            int controlValue,
            int mana,
            int playerHp,
            int playerShield,
            int enemyHp,
            int enemyShield,
            float bossCastBefore,
            float bossCastAfter,
            float bossCastDuration)
        {
            BattleSandboxItemEffectRuntimeProfile profile =
                BattleSandboxItemEffectRuntimePreviewCatalog.Resolve(item, stat);
            string displayName = DisplayItem(item, stat);
            int safeControl = Mathf.Max(0, controlValue);
            BattleSandboxRuntimeLoopRow row = BaseItemRow(
                "control",
                step,
                elapsed,
                item,
                stat,
                mana,
                mana,
                playerHp,
                playerHp,
                playerShield,
                playerShield,
                enemyHp,
                enemyHp,
                enemyShield,
                enemyShield,
                bossCastAfter,
                bossCastDuration,
                "敌人：行动被镇住",
                "首领施法条被压慢",
                $"【镇压】{displayName}镇压 +{safeControl}，施法 {bossCastBefore:0.0}->{bossCastAfter:0.0}秒",
                $"镇压 +{safeControl}",
                SourceCombatKernel + ".ApplyControlPreview+" + profile.SourceDataPath);
            row.itemTriggerChinese = "控制已生效";
            ApplyBoardItemTriggerFeedback(row, "control", safeControl, $"镇压 +{safeControl}");
            return row;
        }

        private static BattleSandboxRuntimeLoopRow CreateItemWaitRow(
            int step,
            float elapsed,
            BuildSandboxPlacedItemSnapshot item,
            BuildSandboxItemStat stat,
            int manaCost,
            int mana,
            int playerHp,
            int playerShield,
            int enemyHp,
            int enemyShield,
            float bossCastRemaining,
            float bossCastDuration)
        {
            BattleSandboxItemEffectRuntimeProfile profile =
                BattleSandboxItemEffectRuntimePreviewCatalog.Resolve(item, stat);
            string displayName = DisplayItem(item, stat);
            BattleSandboxRuntimeLoopRow row = BaseItemRow(
                "itemTrigger",
                step,
                elapsed,
                item,
                stat,
                mana,
                mana,
                playerHp,
                playerHp,
                playerShield,
                playerShield,
                enemyHp,
                enemyHp,
                enemyShield,
                enemyShield,
                bossCastRemaining,
                bossCastDuration,
                $"\u9053\u5177\uff1a{profile.effectRoleChinese}\u7b49\u5f85\u7075\u529b",
                "\u9996\u9886\u84c4\u529b\u672a\u65ad",
                $"\u3010{profile.effectFamilyChinese}\u3011{displayName}\u7075\u529b\u4e0d\u8db3 {mana}/{manaCost}",
                "\u7075\u529b\u4e0d\u8db3",
                SourceItemStat + ".manaCostPerCast+" + profile.SourceDataPath);
            row.itemTriggerChinese = "\u7075\u529b\u4e0d\u8db3";
            ApplyBoardItemTriggerFeedback(row, "manaShortage", 0, "灵力不足");
            return row;
        }

        private static BattleSandboxRuntimeLoopRow CreateBossCastRow(
            int step,
            float elapsed,
            int mana,
            int playerHp,
            int playerShield,
            int enemyHp,
            int enemyShield,
            float enemyAttackTimerBefore,
            float bossCastRemaining,
            float bossCastDuration)
        {
            BattleSandboxRuntimeLoopRow row = BaseRow(
                "bossCast",
                "bossCast",
                step,
                elapsed,
                mana,
                mana,
                playerHp,
                playerHp,
                playerShield,
                playerShield,
                enemyHp,
                enemyHp,
                enemyShield,
                enemyShield,
                bossCastRemaining,
                bossCastDuration,
                "\u9996\u9886\uff1a\u65bd\u6cd5\u6761\u5237\u65b0",
                $"\u65bd\u6cd5\u4e2d\uff1a\u9501\u9635\u51b2\u51fb {bossCastRemaining:0.0}\u79d2",
                $"\u3010\u65bd\u6cd5\u3011\u9996\u9886\u5269\u4f59 {bossCastRemaining:0.0}\u79d2",
                bossCastRemaining <= 0f ? "\u65bd\u6cd5\u91ca\u653e" : "\u84c4\u529b\u4e2d",
                SourceDevEnemyProfileAttackInterval);
            row.enemyAttackTimerBeforeSeconds = enemyAttackTimerBefore;
            row.enemyAttackTimerAfterSeconds = bossCastRemaining;
            row.enemyAttackTimerAdvanced = enemyAttackTimerBefore > bossCastRemaining;
            row.enemyAttackFromDevOnlyProfile = true;
            return row;
        }

        private static BattleSandboxRuntimeLoopRow CreatePlayerDamageRow(
            int step,
            float elapsed,
            BattleSandboxRuntimeLoopScenario scenario,
            int bossDamage,
            int blockedByShield,
            int hpDamage,
            int mana,
            int playerHpBefore,
            int playerHpAfter,
            int playerShieldBefore,
            int playerShieldAfter,
            int enemyHp,
            int enemyShield,
            float bossCastDuration,
            int playerMaxHp)
        {
            BattleSandboxRuntimeLoopScenario safeScenario = scenario ?? new BattleSandboxRuntimeLoopScenario();
            BattleSandboxRuntimeLoopRow row = BaseRow(
                "playerHp",
                "playerHp",
                step,
                elapsed,
                mana,
                mana,
                playerHpBefore,
                playerHpAfter,
                playerShieldBefore,
                playerShieldAfter,
                enemyHp,
                enemyHp,
                enemyShield,
                enemyShield,
                bossCastDuration,
                bossCastDuration,
                "\u73a9\u5bb6\uff1a\u627f\u53d7\u538b\u529b",
                "\u65bd\u6cd5\u5df2\u91ca\u653e\uff0c\u91cd\u65b0\u84c4\u529b",
                $"\u3010\u538b\u529b\u3011\u9996\u9886\u9020\u6210 {bossDamage}\uff0c\u62a4\u76fe\u5438\u6536 {blockedByShield}\uff0c\u6c14\u8840\u635f\u5931 {hpDamage}\uff0c\u73a9\u5bb6 {playerHpAfter}/{Mathf.Max(1, playerMaxHp)}",
                $"-{Mathf.Max(0, playerHpBefore - playerHpAfter)}",
                string.IsNullOrWhiteSpace(safeScenario.attackSourcePath)
                    ? SourceDevEnemyProfileAttack
                    : safeScenario.attackSourcePath);
            row.enemyAttackDamage = bossDamage;
            row.playerShieldDamageAbsorbed = blockedByShield;
            row.playerHpDamageApplied = hpDamage;
            row.enemyAttackFromDevOnlyProfile = safeScenario.attackFromDevOnlyProfile;
            row.playerShieldDamageResolvedFirst =
                blockedByShield == Mathf.Min(playerShieldBefore, bossDamage)
                && playerShieldAfter == Mathf.Max(0, playerShieldBefore - blockedByShield)
                && hpDamage == Mathf.Max(0, bossDamage - blockedByShield)
                && playerHpAfter == Mathf.Max(0, playerHpBefore - hpDamage);
            return row;
        }

        private static void AddSandboxResultRow(
            BattleSandboxRuntimeLoopPreview preview,
            BattleSandboxRuntimeLoopScenario scenario,
            BattleSandboxBuildCombatPreview buildCombatPreview,
            int mana,
            int playerHp,
            int playerShield,
            int enemyHp,
            int enemyShield,
            float bossCastRemaining,
            float bossCastDuration,
            bool allowScenarioVictory,
            int resultStep,
            float resultElapsedSeconds)
        {
            bool defeat = playerHp <= 0;
            bool victory = !defeat && (enemyHp <= 0 || (allowScenarioVictory && scenario?.expectsSandboxVictory == true));
            if (!victory && !defeat)
            {
                defeat = playerHp <= 0;
            }

            string displayLabel = string.IsNullOrWhiteSpace(scenario?.DisplayLabel)
                ? "\u6d4b\u8bd5\u654c\u4eba"
                : scenario.DisplayLabel;
            string buildBrief = BuildResultBrief(buildCombatPreview, victory);
            string mechanicFeedback = FormatMechanicFeedback(scenario);
            string buildPressureFeedback = FormatBuildPressureFeedback(scenario);
            BattleSandboxRuntimeLoopRow row = BaseRow(
                victory ? "sandboxVictory" : "sandboxDefeat",
                victory ? "sandboxVictory" : "sandboxDefeat",
                resultStep,
                resultElapsedSeconds,
                mana,
                mana,
                playerHp,
                defeat ? 0 : playerHp,
                playerShield,
                playerShield,
                enemyHp,
                victory ? 0 : enemyHp,
                enemyShield,
                enemyShield,
                bossCastRemaining,
                bossCastDuration,
                victory ? "\u6c99\u76d2\uff1a\u80dc\u5229" : "\u6c99\u76d2\uff1a\u5931\u8d25",
                "\u7ed3\u679c\u63d0\u793a\uff1a\u4ec5\u9650\u6c99\u76d2",
                victory
                    ? $"\u3010\u6c99\u76d2\u7ed3\u679c\u3011{displayLabel}\u5df2\u88ab\u51fb\u7834\uff1b\u673a\u5236\u53cd\u9988\uff1a{mechanicFeedback}\uff1b\u6784\u7b51\u538b\u529b\uff1a{buildPressureFeedback}\uff1b\u672c\u5c40\u6784\u7b51\u7b80\u8bc4\uff1a{buildBrief}\uff1b\u4e0d\u53d1\u653e\u5956\u52b1\uff0c\u4e0d\u5199\u5165\u5b58\u6863\uff0c\u4e0d\u63a8\u8fdb\u6b63\u5f0f\u6d41\u7a0b"
                    : $"\u3010\u6c99\u76d2\u7ed3\u679c\u3011{displayLabel}\u538b\u5236\u4e86\u5f53\u524d\u9635\u9762\uff1b\u673a\u5236\u53cd\u9988\uff1a{mechanicFeedback}\uff1b\u6784\u7b51\u538b\u529b\uff1a{buildPressureFeedback}\uff1b\u672c\u5c40\u6784\u7b51\u7b80\u8bc4\uff1a{buildBrief}\uff1b\u4e0d\u53d1\u653e\u5956\u52b1\uff0c\u4e0d\u5199\u5165\u5b58\u6863\uff0c\u4e0d\u63a8\u8fdb\u6b63\u5f0f\u6d41\u7a0b",
                victory ? "\u80dc\u5229" : "\u5931\u8d25",
                SourceDevEnemySelection);
            row.hasSandboxVictoryResult = victory;
            row.hasSandboxDefeatResult = defeat;
            row.locksRuntimeLoop = true;
            row.resultTitleChinese = victory ? "\u6c99\u76d2\u80dc\u5229" : "\u6c99\u76d2\u5931\u8d25";
            row.resultBodyChinese = victory
                ? $"{displayLabel}\u5df2\u88ab\u51fb\u7834\uff1b\u673a\u5236\u53cd\u9988\uff1a{mechanicFeedback}\uff1b\u6784\u7b51\u538b\u529b\uff1a{buildPressureFeedback}\uff1b\u672c\u5c40\u6784\u7b51\u7b80\u8bc4\uff1a{buildBrief}\u3002\u672c\u6b21\u4ec5\u5c55\u793a\u6c99\u76d2\u7ed3\u679c\uff0c\u4e0d\u53d1\u653e\u5956\u52b1\u3001\u4e0d\u5199\u5165\u5b58\u6863\u3001\u4e0d\u63a8\u8fdb\u6b63\u5f0f\u6d41\u7a0b\u3002"
                : $"{displayLabel}\u538b\u5236\u4e86\u5f53\u524d\u9635\u9762\uff1b\u673a\u5236\u53cd\u9988\uff1a{mechanicFeedback}\uff1b\u6784\u7b51\u538b\u529b\uff1a{buildPressureFeedback}\uff1b\u672c\u5c40\u6784\u7b51\u7b80\u8bc4\uff1a{buildBrief}\u3002\u672c\u6b21\u4ec5\u5c55\u793a\u6c99\u76d2\u7ed3\u679c\uff0c\u4e0d\u53d1\u653e\u5956\u52b1\u3001\u4e0d\u5199\u5165\u5b58\u6863\u3001\u4e0d\u63a8\u8fdb\u6b63\u5f0f\u6d41\u7a0b\u3002";
            row.restartHintChinese = "\u70b9\u51fb\u91cd\u5f00\u672c\u573a\uff0c\u6216\u5207\u6362\u6d4b\u8bd5\u654c\u4eba\u3002";
            preview.rows.Add(row);
        }

        private static string FormatMechanicFeedback(BattleSandboxRuntimeLoopScenario scenario)
        {
            return string.IsNullOrWhiteSpace(scenario?.playerMechanicFeedbackChinese)
                ? "\u9996\u9886\u673a\u5236\u5df2\u5207\u6362\uff0c\u7559\u610f\u72b6\u6001\u4e0e\u65bd\u6cd5\u8282\u594f\u3002"
                : scenario.playerMechanicFeedbackChinese;
        }

        private static string FormatBuildPressureFeedback(BattleSandboxRuntimeLoopScenario scenario)
        {
            return string.IsNullOrWhiteSpace(scenario?.playerBuildPressureChinese)
                ? "\u5efa\u8bae\u89c2\u5bdf\u4f9b\u80fd\u3001\u62a4\u76fe\u548c\u6301\u7eed\u538b\u5236\u7684\u7a33\u5b9a\u6027\u3002"
                : scenario.playerBuildPressureChinese;
        }

        private static string BuildResultBrief(
            BattleSandboxBuildCombatPreview buildCombatPreview,
            bool victory)
        {
            int itemCount = buildCombatPreview?.PlacedItemSnapshotCount ?? 0;
            int synergyCount = buildCombatPreview?.SynergyMatchCount ?? 0;
            int modifierCount = buildCombatPreview?.ModifierBundleCount ?? 0;
            int itemStatCount = buildCombatPreview?.ItemStatProfileCount ?? 0;

            if (victory)
            {
                if (synergyCount > 0 && modifierCount > 0)
                {
                    return "\u6446\u653e\u5df2\u5f62\u6210\u7a33\u5b9a\u8054\u52a8\uff0c\u7075\u529b\u4e0e\u538b\u5236\u8282\u594f\u8db3\u4ee5\u6253\u7a7f\u5f53\u524d\u76ee\u6807";
                }

                if (itemCount > 0 || itemStatCount > 0)
                {
                    return "\u5f53\u524d\u9635\u9762\u57fa\u7840\u8f93\u51fa\u6210\u7acb\uff0c\u540e\u7eed\u53ef\u7ee7\u7eed\u89c2\u5bdf\u4f9b\u80fd\u4e0e\u76f8\u90bb\u8282\u594f";
                }

                return "\u9ed8\u8ba4\u6c99\u76d2\u9635\u9762\u80fd\u5b8c\u6210\u538b\u5236\uff0c\u4f46\u9700\u8981\u5728\u771f\u5b9e\u6446\u653e\u4e2d\u7ee7\u7eed\u9a8c\u8bc1";
            }

            if (itemCount <= 2)
            {
                return "\u53ef\u7528\u9053\u5177\u6570\u504f\u5c11\uff0c\u627f\u538b\u540e\u5bb9\u6613\u65ad\u8282\u594f";
            }

            if (synergyCount <= 0)
            {
                return "\u9635\u9762\u8054\u52a8\u4ecd\u504f\u8584\uff0c\u5efa\u8bae\u5148\u8865\u7a33\u5b9a\u4f9b\u80fd\u3001\u62a4\u9635\u6216\u7834\u76fe\u8282\u594f";
            }

            return "\u5df2\u6709\u90e8\u5206\u8054\u52a8\uff0c\u4f46\u62a4\u76fe\u3001\u7075\u529b\u56de\u8f6c\u6216\u6301\u7eed\u538b\u5236\u4ecd\u4e0d\u8db3";
        }

        private static BattleSandboxRuntimeLoopRow BaseItemRow(
            string rowKind,
            int step,
            float elapsed,
            BuildSandboxPlacedItemSnapshot item,
            BuildSandboxItemStat stat,
            int manaBefore,
            int manaAfter,
            int playerHpBefore,
            int playerHpAfter,
            int playerShieldBefore,
            int playerShieldAfter,
            int enemyHpBefore,
            int enemyHpAfter,
            int enemyShieldBefore,
            int enemyShieldAfter,
            float bossCastRemaining,
            float bossCastDuration,
            string state,
            string cast,
            string log,
            string floating,
            string source)
        {
            BattleSandboxItemEffectRuntimeProfile profile =
                BattleSandboxItemEffectRuntimePreviewCatalog.Resolve(item, stat);
            BattleSandboxRuntimeLoopRow row = BaseRow(
                rowKind,
                rowKind,
                step,
                elapsed,
                manaBefore,
                manaAfter,
                playerHpBefore,
                playerHpAfter,
                playerShieldBefore,
                playerShieldAfter,
                enemyHpBefore,
                enemyHpAfter,
                enemyShieldBefore,
                enemyShieldAfter,
                bossCastRemaining,
                bossCastDuration,
                state,
                cast,
                log,
                floating,
                source);
            row.itemId = item?.itemId ?? string.Empty;
            row.statProfileId = stat?.statProfileId ?? string.Empty;
            row.itemEffectKey = profile.itemEffectKey;
            row.itemEffectFamilyChinese = profile.effectFamilyChinese;
            row.itemEffectRoleChinese = profile.effectRoleChinese;
            row.boardItemTriggerOccupiedCells = item?.occupiedCells == null
                ? new List<ItemShapeCell>()
                : item.occupiedCells.Select(cell => new ItemShapeCell(cell.x, cell.y)).ToList();
            return row;
        }

        private static void ApplyBoardItemTriggerFeedback(
            BattleSandboxRuntimeLoopRow row,
            string kind,
            int value,
            string text,
            string channel = "active")
        {
            if (row == null || string.IsNullOrWhiteSpace(row.itemId) || string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            row.playsBoardItemTriggerFeedback = true;
            row.boardItemTriggerFeedbackChannel =
                string.Equals(channel, "passive", StringComparison.Ordinal) ? "passive" : "active";
            row.boardItemTriggerFeedbackKind = kind ?? string.Empty;
            row.boardItemTriggerFeedbackValue = Mathf.Max(0, value);
            row.boardItemTriggerFeedbackTextChinese = text.Trim();
        }

        private static string BuildDamageFeedbackText(
            BattleSandboxItemEffectRuntimeProfile profile,
            int damage)
        {
            int safeDamage = Mathf.Max(0, damage);
            string familyKey = profile?.effectFamilyKey ?? string.Empty;
            if (familyKey.IndexOf("fire", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return $"-{safeDamage} 灼热伤害";
            }

            if (familyKey.IndexOf("thunder", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return $"-{safeDamage} 雷击伤害";
            }

            string family = string.IsNullOrWhiteSpace(profile?.effectFamilyChinese)
                ? "伤害"
                : profile.effectFamilyChinese;
            return $"-{safeDamage} {family}伤害";
        }

        private static BattleSandboxRuntimeLoopRow BaseRow(
            string rowIdPrefix,
            string rowKind,
            int step,
            float elapsed,
            int manaBefore,
            int manaAfter,
            int playerHpBefore,
            int playerHpAfter,
            int playerShieldBefore,
            int playerShieldAfter,
            int enemyHpBefore,
            int enemyHpAfter,
            int enemyShieldBefore,
            int enemyShieldAfter,
            float bossCastRemaining,
            float bossCastDuration,
            string state,
            string cast,
            string log,
            string floating,
            string source)
        {
            return new BattleSandboxRuntimeLoopRow
            {
                rowId = $"runtimeLoop.{rowIdPrefix}.{step:00}",
                rowKind = rowKind,
                elapsedSeconds = elapsed,
                manaBefore = manaBefore,
                manaAfter = manaAfter,
                manaDelta = manaAfter - manaBefore,
                playerHpBefore = playerHpBefore,
                playerHpAfter = playerHpAfter,
                playerShieldBefore = playerShieldBefore,
                playerShieldAfter = playerShieldAfter,
                enemyHpBefore = enemyHpBefore,
                enemyHpAfter = enemyHpAfter,
                enemyShieldBefore = enemyShieldBefore,
                enemyShieldAfter = enemyShieldAfter,
                bossCastRemainingSeconds = bossCastRemaining,
                bossCastFillAmount = bossCastDuration <= 0f ? 0f : Mathf.Clamp01(bossCastRemaining / bossCastDuration),
                stateLineChinese = state ?? string.Empty,
                castSkillLineChinese = cast ?? string.Empty,
                combatLogLineChinese = log ?? string.Empty,
                floatingTextChinese = floating ?? string.Empty,
                sourceDataPath = source ?? SourceRuntimeLoop,
                developerDataPanelFieldKey = BattleSandboxRuntimeLoopPreview.DeveloperDataPanelFieldKey,
                readsCombatKernelAdapter = (source ?? string.Empty).IndexOf("CombatKernel", StringComparison.Ordinal) >= 0
                    || (source ?? string.Empty).IndexOf("BattleSandboxRuntimeLoop", StringComparison.Ordinal) >= 0,
                readsBuildSandboxItemStat = (source ?? string.Empty).IndexOf(".itemStat", StringComparison.Ordinal) >= 0
                    || string.Equals(rowKind, "itemTrigger", StringComparison.Ordinal)
                    || string.Equals(rowKind, "cooldown", StringComparison.Ordinal),
                readsCurrentBoardSnapshot = true
            };
        }

        private static string DisplayItem(BuildSandboxPlacedItemSnapshot item)
        {
            return DisplayItem(item, ResolveStat(item));
        }

        private static string DisplayItem(BuildSandboxPlacedItemSnapshot item, BuildSandboxItemStat stat)
        {
            return BattleSandboxItemEffectRuntimePreviewCatalog.DisplayItem(item, stat);
        }

        private static string DisplayShortItem(BuildSandboxPlacedItemSnapshot item)
        {
            return BattleSandboxItemEffectRuntimePreviewCatalog.DisplayShortItem(item, ResolveStat(item));
        }
    }

    public sealed class BattleSandboxRuntimeLoopRuntime : MonoBehaviour
    {
        public const string PackageName = BattleSandboxRuntimeLoopPreview.PackageName;

        private const float RowTickSeconds = 0.85f;
        private const string HPTextName = "HPText";
        private const string ShieldTextName = "ShieldText";
        private const string ManaTextName = "ManaText";
        private const string StateTextName = "StateText";
        private const string CurrentLevelTextName = "CurrentLevelText";
        private const string EnemyHPTextName = "EnemyHPText";
        private const string ChargeTextName = "ChargeText";
        private const string PlayerHPBarName = "PlayerHPBar";
        private const string PlayerManaBarName = "PlayerManaBar";
        private const string ChargeFillName = "ChargeFill";
        private const string FillChildName = "Fill";

        [SerializeField] private bool devOnly = true;
        [SerializeField] private bool isEnabled;
        [SerializeField] private bool readsCombatKernelAdapter = true;
        [SerializeField] private bool writesFormalFlow;
        [SerializeField] private bool writesFormalSaveData;
        [SerializeField] private bool grantsFormalReward;
        [SerializeField] private bool advancesChapter;
        [SerializeField] private bool touchesFormalSceneUiLayout;

        [SerializeField] private Text hpText;
        [SerializeField] private Text shieldText;
        [SerializeField] private Text manaText;
        [SerializeField] private Text stateText;
        [SerializeField] private Text currentLevelText;
        [SerializeField] private Text enemyHpText;
        [SerializeField] private Text chargeText;
        [SerializeField] private Image playerHpFillImage;
        [SerializeField] private Image playerManaFillImage;
        [SerializeField] private Image chargeFillImage;

        private BuildGridInteractionPreviewController gridController;
        private BattleSandboxManaLoopRuntime manaLoopRuntime;
        private BattleSandboxEnemyCombatFeedbackController feedbackController;
        private BattleSandboxItemTriggerFeedbackController itemTriggerFeedbackController;
        private BattleSandboxRuntimeLoopPreview activePreview;
        private float rowTimer;
        private int rowIndex;
        private int selectedDevEnemyIndex;
        private bool wasBattleActive;
        private bool manaLoopRuntimeWasEnabled;
        private bool manaLoopRuntimeSuspended;
        private bool resultLocked;
        private float activeRowTickSeconds = RowTickSeconds;
        private long runtimeGeneration;
        private long runtimeRevision;
        private long currentRowRevision;
        private int lastAcceptedExplicitGeneration;
        private string lastAcceptedExplicitStartToken = string.Empty;
        private BattleSandboxExplicitDevEncounterRequest currentExplicitRequest;
        private BattleSandboxExplicitDevEncounterAcceptedStart currentExplicitAcceptedStart;
        private BattleSandboxExplicitDevEncounterRejectReason lastExplicitRejectReason;
        private string lastExplicitDiagnosticCode = "NONE";

        public bool DevOnly => devOnly;
        public bool IsEnabled => isEnabled;
        public bool ReadsCombatKernelAdapter => readsCombatKernelAdapter;
        public bool WritesFormalFlow => writesFormalFlow;
        public bool WritesFormalSaveData => writesFormalSaveData;
        public bool GrantsFormalReward => grantsFormalReward;
        public bool AdvancesChapter => advancesChapter;
        public bool TouchesFormalSceneUiLayout => touchesFormalSceneUiLayout;
        public BattleSandboxRuntimeLoopPreview ActivePreview => activePreview;
        public bool HasSandboxResult => resultLocked;
        public bool IsRunning => activePreview != null && wasBattleActive && !resultLocked;
        public string CurrentDevEnemyLabel => ResolveCurrentDevEnemyLabel();
        public string CurrentDevChapterLabel => ResolveCurrentDevChapterLabel();
        public string CurrentSandboxResultTitle => CurrentRow()?.resultTitleChinese ?? string.Empty;
        public long CurrentRowRevision => currentRowRevision;
        public BattleSandboxRuntimeLoopRow CurrentAcceptedRow => CurrentRow();
        public BattleSandboxExplicitDevEncounterAcceptedStart CurrentExplicitAcceptedStart =>
            currentExplicitAcceptedStart;
        public BattleSandboxExplicitDevEncounterRejectReason LastExplicitRejectReason =>
            lastExplicitRejectReason;
        public string LastExplicitDiagnosticCode => lastExplicitDiagnosticCode;

        public void Bind(
            BuildGridInteractionPreviewController controller,
            BattleSandboxManaLoopRuntime manaLoop,
            BattleSandboxEnemyCombatFeedbackController feedback)
        {
            gridController = controller;
            manaLoopRuntime = manaLoop;
            feedbackController = feedback;
            itemTriggerFeedbackController = GetComponent<BattleSandboxItemTriggerFeedbackController>();
            if (itemTriggerFeedbackController == null)
            {
                itemTriggerFeedbackController = gameObject.AddComponent<BattleSandboxItemTriggerFeedbackController>();
                itemTriggerFeedbackController.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            }

            itemTriggerFeedbackController.Bind(controller);
            ResolveUiReferences();
        }

        public void SetBattleModeVisible(bool visible)
        {
            if (!visible)
            {
                ResetLoop();
            }
        }

        public void ResetLoop()
        {
            activePreview = null;
            rowTimer = 0f;
            rowIndex = 0;
            wasBattleActive = false;
            resultLocked = false;
            activeRowTickSeconds = RowTickSeconds;
            runtimeRevision++;
            currentRowRevision++;
            currentExplicitRequest = null;
            currentExplicitAcceptedStart = null;
            itemTriggerFeedbackController?.ClearAll();
            RestoreManaLoopRuntime();
            feedbackController?.SetRuntimeLoopMode(false);
        }

        public void RestartLoop()
        {
            currentExplicitRequest = null;
            currentExplicitAcceptedStart = null;
            activeRowTickSeconds = RowTickSeconds;
            resultLocked = false;
            activePreview = null;
            rowTimer = 0f;
            rowIndex = 0;
            if (gridController != null && gridController.IsSandboxBattleModeActive)
            {
                StartLoop();
                wasBattleActive = true;
            }
        }

        public bool TryStartExplicitDevEncounter(
            BattleSandboxExplicitDevEncounterRequest request,
            out BattleSandboxExplicitDevEncounterAcceptedStart acceptedStart)
        {
            acceptedStart = null;
            if (!BattleSandboxExplicitDevEncounterContract.Validate(
                    request,
                    Application.isEditor,
                    out BattleSandboxExplicitDevEncounterRejectReason rejectReason,
                    out string diagnosticCode))
            {
                SetExplicitRejection(rejectReason, diagnosticCode);
                return false;
            }

            if (currentExplicitRequest != null)
            {
                if (BattleSandboxExplicitDevEncounterContract.IsIdempotentlyEquivalent(
                        currentExplicitRequest,
                        request))
                {
                    acceptedStart = currentExplicitAcceptedStart;
                    SetExplicitRejection(
                        BattleSandboxExplicitDevEncounterRejectReason.None,
                        "NONE");
                    return acceptedStart != null;
                }

                SetExplicitRejection(
                    BattleSandboxExplicitDevEncounterRejectReason.DuplicateMismatch,
                    "EXPLICIT_DEV_ENCOUNTER_DUPLICATE_MISMATCH");
                return false;
            }

            if (request.Generation <= lastAcceptedExplicitGeneration)
            {
                SetExplicitRejection(
                    BattleSandboxExplicitDevEncounterRejectReason.StaleGenerationOrToken,
                    "EXPLICIT_DEV_ENCOUNTER_STALE_GENERATION_OR_TOKEN");
                return false;
            }

            if (gridController == null)
            {
                SetExplicitRejection(
                    BattleSandboxExplicitDevEncounterRejectReason.RuntimeDependencyMissing,
                    "EXPLICIT_DEV_ENCOUNTER_RUNTIME_DEPENDENCY_MISSING");
                return false;
            }

            if (!gridController.IsSandboxBattleModeActive)
            {
                SetExplicitRejection(
                    BattleSandboxExplicitDevEncounterRejectReason.BattleModeInactive,
                    "EXPLICIT_DEV_ENCOUNTER_BATTLE_MODE_INACTIVE");
                return false;
            }

            BuildSandboxLayoutSnapshot snapshot =
                gridController.BuildCurrentLayoutSnapshot();
            BattleSandboxRuntimeLoopPreview candidate =
                BattleSandboxRuntimeLoopPreviewBuilder.BuildExplicitDevEncounter(
                    snapshot,
                    "core_loop_lab_explicit_" + request.EncounterProfileId,
                    request);
            if (candidate?.rows == null
                || candidate.rows.Count == 0
                || candidate.SandboxResultRowCount != 1)
            {
                SetExplicitRejection(
                    BattleSandboxExplicitDevEncounterRejectReason.RuntimeStartRejected,
                    "EXPLICIT_DEV_ENCOUNTER_RUNTIME_START_REJECTED");
                return false;
            }

            long acceptedRuntimeGeneration = runtimeGeneration + 1;
            long acceptedRuntimeRevision = runtimeRevision + 1;
            string initialLedgerFingerprint =
                BattleSandboxExplicitDevEncounterContract
                    .ComputeInitialLedgerFingerprint(
                        request,
                        acceptedRuntimeGeneration,
                        acceptedRuntimeRevision);
            BattleSandboxExplicitDevEncounterStartSnapshot startSnapshot = new(
                request.EnemyIdentity,
                request.EncounterKind,
                request.PlayerMaxHp,
                request.PlayerInitialShield,
                request.EnemyMaxHp,
                request.EnemyMaxHp,
                request.BasicAttackDamage,
                request.SkillDamage,
                request.TargetDurationMilliseconds,
                request.BasicAttackIntervalMilliseconds,
                request.SkillCastDurationMilliseconds,
                request.AcceptedEnemyActionCadence,
                acceptedRuntimeGeneration,
                acceptedRuntimeRevision,
                initialLedgerFingerprint);
            BattleSandboxExplicitDevEncounterAcceptedStart candidateAccepted = new(
                request.Generation,
                request.StartToken,
                request.EncounterProfileId,
                request.ProfileFingerprint,
                startSnapshot);

            currentExplicitRequest = request;
            currentExplicitAcceptedStart = candidateAccepted;
            lastAcceptedExplicitGeneration = request.Generation;
            lastAcceptedExplicitStartToken = request.StartToken;
            runtimeGeneration = acceptedRuntimeGeneration;
            runtimeRevision = acceptedRuntimeRevision;
            activeRowTickSeconds = candidate.rows.Count <= 1
                ? RowTickSeconds
                : Mathf.Max(
                    0.01f,
                    request.TargetDurationMilliseconds
                    / 1000f
                    / (candidate.rows.Count - 1));
            ActivatePreview(candidate);
            wasBattleActive = true;
            acceptedStart = candidateAccepted;
            SetExplicitRejection(
                BattleSandboxExplicitDevEncounterRejectReason.None,
                "NONE");
            return true;
        }

        public string SelectNextDevEnemy()
        {
            IReadOnlyList<BattleSandboxRuntimeLoopScenario> scenarios =
                BattleSandboxRuntimeLoopPreviewBuilder.BuildDevEnemyScenarios();
            int count = Mathf.Max(1, scenarios.Count);
            selectedDevEnemyIndex = (selectedDevEnemyIndex + 1) % count;
            RestartLoop();
            return ResolveCurrentDevEnemyLabel();
        }

        public string SelectNextDevChapter()
        {
            string current = ResolveCurrentDevChapterLabel();
            string next = string.Equals(current, "3-10", StringComparison.Ordinal)
                ? "4-10"
                : "3-10";
            return SelectDevChapter(next);
        }

        public string SelectDevChapter(string devChapterLabel)
        {
            IReadOnlyList<BattleSandboxRuntimeLoopScenario> scenarios =
                BattleSandboxRuntimeLoopPreviewBuilder.BuildDevEnemyScenarios();
            if (scenarios.Count <= 0)
            {
                RestartLoop();
                return ResolveCurrentDevEnemyLabel();
            }

            int normalizedIndex = Mathf.Abs(selectedDevEnemyIndex) % scenarios.Count;
            int targetIndex = -1;
            for (int index = 0; index < scenarios.Count; index++)
            {
                BattleSandboxRuntimeLoopScenario scenario = scenarios[index];
                if (scenario == null
                    || !string.Equals(scenario.devChapterLabel, devChapterLabel, StringComparison.Ordinal))
                {
                    continue;
                }

                if (index != normalizedIndex)
                {
                    targetIndex = index;
                    break;
                }

                if (targetIndex < 0)
                {
                    targetIndex = index;
                }
            }

            if (targetIndex >= 0)
            {
                selectedDevEnemyIndex = targetIndex;
            }

            RestartLoop();
            return ResolveCurrentDevEnemyLabel();
        }

        private void Awake()
        {
            ResolveUiReferences();
        }

        private void OnEnable()
        {
            ResolveUiReferences();
        }

        private void OnDisable()
        {
            RestoreManaLoopRuntime();
            feedbackController?.SetRuntimeLoopMode(false);
            itemTriggerFeedbackController?.ClearAll();
        }

        private void Update()
        {
            bool battleActive = gridController != null && gridController.IsSandboxBattleModeActive;
            if (!battleActive)
            {
                if (wasBattleActive)
                {
                    ResetLoop();
                }

                return;
            }

            if (!wasBattleActive)
            {
                StartLoop();
            }

            wasBattleActive = true;
            TickLoop(Time.deltaTime);
        }

        private void StartLoop()
        {
            BuildSandboxLayoutSnapshot snapshot = gridController == null
                ? null
                : gridController.BuildCurrentLayoutSnapshot();
            BattleSandboxRuntimeLoopScenario scenario =
                BattleSandboxRuntimeLoopPreviewBuilder.ResolveDevEnemyScenario(selectedDevEnemyIndex);
            BattleSandboxRuntimeLoopPreview preview = BattleSandboxRuntimeLoopPreviewBuilder.Build(
                snapshot,
                string.IsNullOrWhiteSpace(scenario?.previewBuildId)
                    ? "current_v04_runtime_loop_board"
                    : scenario.previewBuildId,
                scenario);
            activeRowTickSeconds = RowTickSeconds;
            ActivatePreview(preview);
        }

        private void ActivatePreview(BattleSandboxRuntimeLoopPreview preview)
        {
            ResolveUiReferences();
            SuspendManaLoopRuntime();
            activePreview = preview;
            rowIndex = 0;
            rowTimer = 0f;
            resultLocked = false;
            ApplyCurrentRow();
        }

        private void TickLoop(float deltaTime)
        {
            if (resultLocked)
            {
                return;
            }

            if (activePreview?.rows == null || activePreview.rows.Count == 0)
            {
                StartLoop();
                return;
            }

            rowTimer += deltaTime;
            if (rowTimer < activeRowTickSeconds)
            {
                return;
            }

            rowTimer -= activeRowTickSeconds;
            rowIndex++;
            if (rowIndex >= activePreview.rows.Count)
            {
                rowIndex = Mathf.Max(0, activePreview.rows.Count - 1);
                resultLocked = true;
            }

            ApplyCurrentRow();
        }

        private void ApplyCurrentRow()
        {
            currentRowRevision++;
            BattleSandboxRuntimeLoopRow row = CurrentRow();
            BattleSandboxRuntimeLoopFrame frame =
                BattleSandboxRuntimeLoopFrame.FromRow(row, activePreview);
            UpdateHud(frame);
            feedbackController?.ApplyRuntimeLoopFrame(frame);
            itemTriggerFeedbackController?.Play(row);
            if (frame.locksRuntimeLoop)
            {
                resultLocked = true;
                gridController?.RefreshSandboxBattleActionChrome();
            }
        }

        private BattleSandboxRuntimeLoopRow CurrentRow()
        {
            if (activePreview?.rows == null || activePreview.rows.Count == 0)
            {
                return null;
            }

            return activePreview.rows[Mathf.Clamp(rowIndex, 0, activePreview.rows.Count - 1)];
        }

        private void SetExplicitRejection(
            BattleSandboxExplicitDevEncounterRejectReason rejectReason,
            string diagnosticCode)
        {
            lastExplicitRejectReason = rejectReason;
            lastExplicitDiagnosticCode = string.IsNullOrWhiteSpace(diagnosticCode)
                ? "EXPLICIT_DEV_ENCOUNTER_UNKNOWN_REJECTION"
                : diagnosticCode;
            if (rejectReason != BattleSandboxExplicitDevEncounterRejectReason.None)
            {
                Debug.LogWarning(lastExplicitDiagnosticCode, this);
            }
        }

        private string ResolveCurrentDevEnemyLabel()
        {
            if (activePreview != null)
            {
                string label = activePreview.selectedDevEnemyLabel;
                string enemy = activePreview.selectedDevEnemyDisplayNameChinese;
                if (!string.IsNullOrWhiteSpace(label) || !string.IsNullOrWhiteSpace(enemy))
                {
                    return string.IsNullOrWhiteSpace(label)
                        ? enemy
                        : string.IsNullOrWhiteSpace(enemy)
                            ? label
                            : $"{label} {enemy}";
                }
            }

            BattleSandboxRuntimeLoopScenario scenario =
                BattleSandboxRuntimeLoopPreviewBuilder.ResolveDevEnemyScenario(selectedDevEnemyIndex);
            return string.IsNullOrWhiteSpace(scenario?.DisplayLabel)
                ? "\u6c99\u76d2\u9884\u89c8"
                : scenario.DisplayLabel;
        }

        private string ResolveCurrentDevChapterLabel()
        {
            if (activePreview != null && !string.IsNullOrWhiteSpace(activePreview.selectedDevEnemyLabel))
            {
                return activePreview.selectedDevEnemyLabel;
            }

            BattleSandboxRuntimeLoopScenario scenario =
                BattleSandboxRuntimeLoopPreviewBuilder.ResolveDevEnemyScenario(selectedDevEnemyIndex);
            return string.IsNullOrWhiteSpace(scenario?.devChapterLabel)
                ? "3-10"
                : scenario.devChapterLabel;
        }

        private void UpdateHud(BattleSandboxRuntimeLoopFrame frame)
        {
            if (frame == null)
            {
                return;
            }

            SetText(hpText, $"\u6c14\u8840 {frame.playerHp}/{frame.playerMaxHp}");
            SetText(shieldText, $"\u62a4\u76fe {frame.playerShield}");
            SetText(manaText, $"\u7075\u529b {frame.currentMana}/{frame.maxMana}");
            SetText(stateText, frame.isSandboxResult
                ? frame.resultTitleChinese
                : "\u6c99\u76d2\u6218\u6597\u5faa\u73af");
            SetText(currentLevelText, ResolveCurrentDevEnemyLabel());
            SetText(enemyHpText, $"\u654c\u4eba {frame.enemyHp}/{frame.enemyMaxHp}  \u62a4\u76fe {frame.enemyShield}");
            SetText(chargeText, frame.isSandboxResult
                ? frame.restartHintChinese
                : $"{frame.castSkillLineChinese}  {frame.castTimerTextChinese}");

            SetFill(playerHpFillImage, frame.playerHp / (float)Mathf.Max(1, frame.playerMaxHp));
            SetFill(playerManaFillImage, frame.currentMana / (float)Mathf.Max(1, frame.maxMana));
            SetFill(chargeFillImage, frame.castFillAmount);
        }

        private void SuspendManaLoopRuntime()
        {
            if (manaLoopRuntime == null || manaLoopRuntimeSuspended)
            {
                return;
            }

            manaLoopRuntimeWasEnabled = manaLoopRuntime.enabled;
            manaLoopRuntime.enabled = false;
            manaLoopRuntimeSuspended = true;
        }

        private void RestoreManaLoopRuntime()
        {
            if (manaLoopRuntime == null || !manaLoopRuntimeSuspended)
            {
                return;
            }

            manaLoopRuntime.enabled = manaLoopRuntimeWasEnabled;
            manaLoopRuntimeSuspended = false;
        }

        private void ResolveUiReferences()
        {
            if (hpText == null) hpText = FindComponentByName<Text>(HPTextName);
            if (shieldText == null) shieldText = FindComponentByName<Text>(ShieldTextName);
            if (manaText == null) manaText = FindComponentByName<Text>(ManaTextName);
            if (stateText == null) stateText = FindComponentByName<Text>(StateTextName);
            if (currentLevelText == null) currentLevelText = FindComponentByName<Text>(CurrentLevelTextName);
            if (enemyHpText == null) enemyHpText = FindComponentByName<Text>(EnemyHPTextName);
            if (chargeText == null) chargeText = FindComponentByName<Text>(ChargeTextName);

            if (playerHpFillImage == null)
            {
                playerHpFillImage = FindChildComponentByName<Image>(FindRectTransform(PlayerHPBarName), FillChildName);
            }

            if (playerManaFillImage == null)
            {
                playerManaFillImage = FindChildComponentByName<Image>(FindRectTransform(PlayerManaBarName), FillChildName);
            }

            if (chargeFillImage == null)
            {
                chargeFillImage = FindComponentByName<Image>(ChargeFillName);
            }
        }

        private static void SetText(Text target, string value)
        {
            if (target != null)
            {
                target.text = value ?? string.Empty;
            }
        }

        private static void SetFill(Image target, float amount)
        {
            if (target != null)
            {
                target.fillAmount = Mathf.Clamp01(amount);
            }
        }

        private static T FindComponentByName<T>(string objectName)
            where T : Component
        {
            RectTransform rect = FindRectTransform(objectName);
            return rect == null ? null : rect.GetComponent<T>();
        }

        private static T FindChildComponentByName<T>(Transform parent, string childName)
            where T : Component
        {
            if (parent == null)
            {
                return null;
            }

            foreach (T component in parent.GetComponentsInChildren<T>(true))
            {
                if (component != null && string.Equals(component.name, childName, StringComparison.Ordinal))
                {
                    return component;
                }
            }

            return null;
        }

        private static RectTransform FindRectTransform(string objectName)
        {
            if (string.IsNullOrWhiteSpace(objectName))
            {
                return null;
            }

            foreach (Transform transform in FindObjectsOfType<Transform>(true))
            {
                if (string.Equals(transform.name, objectName, StringComparison.Ordinal))
                {
                    return transform as RectTransform;
                }
            }

            return null;
        }
    }
}
