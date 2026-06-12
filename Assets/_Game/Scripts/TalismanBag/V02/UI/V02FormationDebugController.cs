using System.Collections.Generic;
using TalismanBag.Combat;
using TalismanBag.Enemies;
using TalismanBag.Grid;
using TalismanBag.Items;
using TalismanBag.UI;
using TalismanBag.V02.Balance;
using TalismanBag.V02.Feedback;
using TalismanBag.V02.Formation;
using TalismanBag.V02.Result;
using TalismanBag.V02.Rewards;
using TalismanBag.V02.Run;
using TalismanBag.V02.Tags;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.V02.UI
{
    public sealed class V02FormationDebugController : MonoBehaviour
    {
        private const string SpiritStoneId = "spirit_stone_basic";
        private const string FireTalismanId = "fire_talisman_basic";
        private const string ShieldTalismanId = "shield_talisman_basic";
        private const string ThunderTalismanId = "thunder_talisman_basic";
        private const string ChainThunderTalismanId = "chain_thunder_talisman_basic";
        private const string SwordPillId = "sword_pill_basic";
        private const string PurifyTalismanId = "purify_talisman_basic";
        private const string SoulSuppressTalismanId = "soul_suppress_talisman_basic";
        private const string SealId = "seal_basic";

        [SerializeField] private TalismanBagGrid grid;
        [SerializeField] private AutoCombatController combatController;
        [SerializeField] private FormationPowerResolver powerResolver;
        [SerializeField] private V02RunFlowController v02RunFlowController;
        [SerializeField] private V02RewardController rewardController;
        [SerializeField] private V02RunModifierState runModifierState;
        [SerializeField] private V02FailureTracker failureTracker;
        [SerializeField] private V02RunStatsTracker runStatsTracker;
        [SerializeField] private V02BuildTestRunner buildTestRunner;
        [SerializeField] private FormationPowerUI formationPowerUI;
        [SerializeField] private V02TalismanTooltipUI tooltipUI;
        [SerializeField] private BattleLogUI battleLogUI;
        [SerializeField] private EnemyDefinition testEnemy;
        [SerializeField] private List<EnemyDefinition> testEnemies = new();
        [SerializeField] private TalismanGridSlotView[] slotViews;
        [SerializeField] private List<DraggableTalismanItemView> itemViews = new();

        [Header("Buttons")]
        [SerializeField] private Button autoPlacePoweredButton;
        [SerializeField] private Button autoPlaceUnpoweredButton;
        [SerializeField] private Button refreshPowerButton;
        [SerializeField] private Button startBattleButton;
        [SerializeField] private Button resetFormationButton;
        [SerializeField] private Button printSelectedTagsButton;
        [SerializeField] private Button giveAllTalismansButton;
        [SerializeField] private Button testTagQueryButton;
        [SerializeField] private Button testCounterMatchButton;
        [SerializeField] private Button spawnMountainImpButton;
        [SerializeField] private Button spawnTurtleGuardianButton;
        [SerializeField] private Button spawnImpSwarmButton;
        [SerializeField] private Button spawnRedPoisonBeastButton;
        [SerializeField] private Button spawnEnergyThiefGhostButton;
        [SerializeField] private Button spawnSealTaoistButton;
        [SerializeField] private Button spawnFormationBreakerBossButton;
        [SerializeField] private Button forceEnemySkillButton;
        [SerializeField] private Button clearPlayerStatusButton;
        [SerializeField] private Button clearSealsButton;
        [SerializeField] private Button testShieldBreakButton;
        [SerializeField] private Button testCleansePoisonButton;
        [SerializeField] private Button testCleanseSealButton;
        [SerializeField] private Button testSoulSuppressButton;
        [SerializeField] private Button testChainClearButton;
        [SerializeField] private Button testCounterLogPriorityButton;
        [SerializeField] private Button openRewardPanelButton;
        [SerializeField] private Button setNextTurtleButton;
        [SerializeField] private Button setNextSwarmButton;
        [SerializeField] private Button setNextPoisonButton;
        [SerializeField] private Button setNextThiefButton;
        [SerializeField] private Button setNextSealButton;
        [SerializeField] private Button setNextBossButton;
        [SerializeField] private Button giveEyeCoreRewardButton;
        [SerializeField] private Button giveSpiritLinkRewardButton;
        [SerializeField] private Button giveOuterRingRewardButton;
        [SerializeField] private Button printRunModifiersButton;
        [SerializeField] private Button resetRunModifiersButton;
        [SerializeField] private Button startNewV02RunButton;
        [SerializeField] private Button skipRound1Button;
        [SerializeField] private Button skipRound2Button;
        [SerializeField] private Button skipRound3Button;
        [SerializeField] private Button skipRound4Button;
        [SerializeField] private Button skipRound5Button;
        [SerializeField] private Button skipRound6Button;
        [SerializeField] private Button skipRound7Button;
        [SerializeField] private Button forceWinCurrentRoundButton;
        [SerializeField] private Button forceLoseCurrentRoundButton;
        [SerializeField] private Button giveAntiShieldBuildButton;
        [SerializeField] private Button giveAntiGroupBuildButton;
        [SerializeField] private Button giveAntiPoisonBuildButton;
        [SerializeField] private Button giveAntiStealBuildButton;
        [SerializeField] private Button giveAntiSealBuildButton;
        [SerializeField] private Button giveBossReadyBuildButton;
        [SerializeField] private Button printFailureTrackerButton;
        [SerializeField] private Button printRunStatsButton;
        [SerializeField] private Button resetV02RunButton;
        [SerializeField] private Button runBalanceShieldButton;
        [SerializeField] private Button runBalanceGroupButton;
        [SerializeField] private Button runBalancePoisonButton;
        [SerializeField] private Button runBalanceStealButton;
        [SerializeField] private Button runBalanceSealButton;
        [SerializeField] private Button runBalanceBossButton;

        private void Awake()
        {
            autoPlacePoweredButton?.onClick.AddListener(AutoPlacePoweredBuild);
            autoPlaceUnpoweredButton?.onClick.AddListener(AutoPlaceUnpoweredBuild);
            refreshPowerButton?.onClick.AddListener(RefreshPowerStates);
            startBattleButton?.onClick.AddListener(StartTestBattle);
            resetFormationButton?.onClick.AddListener(ResetFormation);
            printSelectedTagsButton?.onClick.AddListener(PrintSelectedItemTags);
            giveAllTalismansButton?.onClick.AddListener(GiveAllV02Talismans);
            testTagQueryButton?.onClick.AddListener(TestTagQuery);
            testCounterMatchButton?.onClick.AddListener(TestCounterMatch);
            spawnMountainImpButton?.onClick.AddListener(() => SpawnEnemy("mountain_imp_basic"));
            spawnTurtleGuardianButton?.onClick.AddListener(() => SpawnEnemy("turtle_guardian_shield"));
            spawnImpSwarmButton?.onClick.AddListener(() => SpawnEnemy("imp_swarm"));
            spawnRedPoisonBeastButton?.onClick.AddListener(() => SpawnEnemy("red_poison_beast"));
            spawnEnergyThiefGhostButton?.onClick.AddListener(() => SpawnEnemy("energy_thief_ghost"));
            spawnSealTaoistButton?.onClick.AddListener(() => SpawnEnemy("seal_talisman_taoist"));
            spawnFormationBreakerBossButton?.onClick.AddListener(() => SpawnEnemy("formation_breaker_boss"));
            forceEnemySkillButton?.onClick.AddListener(ForceEnemySkill);
            clearPlayerStatusButton?.onClick.AddListener(ClearPlayerStatus);
            clearSealsButton?.onClick.AddListener(ClearSealsAndDisables);
            testShieldBreakButton?.onClick.AddListener(TestShieldBreak);
            testCleansePoisonButton?.onClick.AddListener(TestCleansePoison);
            testCleanseSealButton?.onClick.AddListener(TestCleanseSeal);
            testSoulSuppressButton?.onClick.AddListener(TestSoulSuppress);
            testChainClearButton?.onClick.AddListener(TestChainClear);
            testCounterLogPriorityButton?.onClick.AddListener(TestCounterLogPriority);
            openRewardPanelButton?.onClick.AddListener(OpenRewardPanel);
            setNextTurtleButton?.onClick.AddListener(() => SetNextEnemy("turtle_guardian_shield"));
            setNextSwarmButton?.onClick.AddListener(() => SetNextEnemy("imp_swarm"));
            setNextPoisonButton?.onClick.AddListener(() => SetNextEnemy("red_poison_beast"));
            setNextThiefButton?.onClick.AddListener(() => SetNextEnemy("energy_thief_ghost"));
            setNextSealButton?.onClick.AddListener(() => SetNextEnemy("seal_talisman_taoist"));
            setNextBossButton?.onClick.AddListener(() => SetNextEnemy("formation_breaker_boss"));
            giveEyeCoreRewardButton?.onClick.AddListener(() => GiveReward("reward_upgrade_eye_core_nine_grid"));
            giveSpiritLinkRewardButton?.onClick.AddListener(() => GiveReward("reward_spirit_link_between_stones"));
            giveOuterRingRewardButton?.onClick.AddListener(() => GiveReward("reward_outer_ring_defense_boost"));
            printRunModifiersButton?.onClick.AddListener(PrintRunModifiers);
            resetRunModifiersButton?.onClick.AddListener(ResetRunModifiers);
            startNewV02RunButton?.onClick.AddListener(StartNewV02Run);
            skipRound1Button?.onClick.AddListener(() => SkipToRound(1));
            skipRound2Button?.onClick.AddListener(() => SkipToRound(2));
            skipRound3Button?.onClick.AddListener(() => SkipToRound(3));
            skipRound4Button?.onClick.AddListener(() => SkipToRound(4));
            skipRound5Button?.onClick.AddListener(() => SkipToRound(5));
            skipRound6Button?.onClick.AddListener(() => SkipToRound(6));
            skipRound7Button?.onClick.AddListener(() => SkipToRound(7));
            forceWinCurrentRoundButton?.onClick.AddListener(ForceWinCurrentRound);
            forceLoseCurrentRoundButton?.onClick.AddListener(ForceLoseCurrentRound);
            giveAntiShieldBuildButton?.onClick.AddListener(GiveAntiShieldBuild);
            giveAntiGroupBuildButton?.onClick.AddListener(GiveAntiGroupBuild);
            giveAntiPoisonBuildButton?.onClick.AddListener(GiveAntiPoisonBuild);
            giveAntiStealBuildButton?.onClick.AddListener(GiveAntiStealBuild);
            giveAntiSealBuildButton?.onClick.AddListener(GiveAntiSealBuild);
            giveBossReadyBuildButton?.onClick.AddListener(GiveBossReadyBuild);
            printFailureTrackerButton?.onClick.AddListener(PrintFailureTracker);
            printRunStatsButton?.onClick.AddListener(PrintRunStats);
            resetV02RunButton?.onClick.AddListener(StartNewV02Run);
            runBalanceShieldButton?.onClick.AddListener(() => buildTestRunner?.RunShieldEnemyTest());
            runBalanceGroupButton?.onClick.AddListener(() => buildTestRunner?.RunGroupEnemyTest());
            runBalancePoisonButton?.onClick.AddListener(() => buildTestRunner?.RunPoisonEnemyTest());
            runBalanceStealButton?.onClick.AddListener(() => buildTestRunner?.RunStealEnemyTest());
            runBalanceSealButton?.onClick.AddListener(() => buildTestRunner?.RunSealEnemyTest());
            runBalanceBossButton?.onClick.AddListener(() => buildTestRunner?.RunBossTest());
        }

        private void Start()
        {
            foreach (DraggableTalismanItemView view in itemViews)
            {
                view?.CaptureHome();
            }

            if (v02RunFlowController == null)
            {
                combatController?.SetEnemy(testEnemy != null ? testEnemy : FindEnemy("mountain_imp_basic"), 1, 1);
            }

            RefreshPowerStates();
        }

        public void AutoPlacePoweredBuild()
        {
            if (!CanEdit())
            {
                return;
            }

            ResetFormation();
            PlaceById(FireTalismanId, new Vector2Int(2, 2));
            PlaceById(ShieldTalismanId, new Vector2Int(2, 0));
            PlaceById(SpiritStoneId, new Vector2Int(3, 1));
            battleLogUI?.AddLog("已自动摆放供能阵容");
            RefreshPowerStates();
        }

        public void AutoPlaceUnpoweredBuild()
        {
            if (!CanEdit())
            {
                return;
            }

            ResetFormation();
            PlaceById(SpiritStoneId, new Vector2Int(0, 0));
            PlaceById(FireTalismanId, new Vector2Int(4, 3));
            PlaceById(ShieldTalismanId, new Vector2Int(4, 2));
            battleLogUI?.AddLog("已自动摆放断供阵容");
            RefreshPowerStates();
        }

        public void RefreshPowerStates()
        {
            powerResolver?.RefreshPowerStates();
            formationPowerUI?.Refresh(powerResolver);
        }

        public void StartTestBattle()
        {
            RefreshPowerStates();
            if (v02RunFlowController != null)
            {
                v02RunFlowController.StartCombat();
            }
            else
            {
                combatController?.StartBattle();
            }
        }

        public void ResetFormation()
        {
            if (!CanEdit())
            {
                return;
            }

            foreach (DraggableTalismanItemView view in itemViews)
            {
                view?.ReturnToHome();
            }

            if (slotViews != null)
            {
                foreach (TalismanGridSlotView slotView in slotViews)
                {
                    slotView?.SetItemView(null);
                }
            }

            grid?.Clear();
            combatController?.ResetBattle();
            RefreshPowerStates();
        }

        public void PrintSelectedItemTags()
        {
            tooltipUI?.PrintSelectedTags();
            battleLogUI?.AddLog("已打印所选道具标签");
        }

        public void GiveAllV02Talismans()
        {
            foreach (DraggableTalismanItemView view in itemViews)
            {
                if (view == null)
                {
                    continue;
                }

                view.gameObject.SetActive(true);
                view.CaptureHome();
                combatController?.RegisterItemView(view);
            }

            battleLogUI?.AddLog("已发放全部 V0.2 符箓");
        }

        public void TestTagQuery()
        {
            TalismanItemDefinition thunder = FindDefinition("thunder_talisman_basic");
            TalismanItemDefinition purify = FindDefinition("purify_talisman_basic");
            TalismanItemDefinition soul = FindDefinition("soul_suppress_talisman_basic");
            string message =
                $"[标签查询] 雷符/破盾={TalismanTagUtility.HasFunctionTag(thunder, FunctionTag.ShieldBreak)}; " +
                $"净化符/克制封印={TalismanTagUtility.HasCounterTag(purify, CounterTag.Seal)}; " +
                $"镇魂符/克制偷灵气={TalismanTagUtility.HasCounterTag(soul, CounterTag.StealEnergy)}";
            Debug.Log(message);
            battleLogUI?.AddLog("标签查询已写入日志");
        }

        public void TestCounterMatch()
        {
            TalismanItemDefinition thunder = FindDefinition("thunder_talisman_basic");
            TalismanItemDefinition purify = FindDefinition("purify_talisman_basic");
            TalismanItemDefinition soul = FindDefinition("soul_suppress_talisman_basic");
            TalismanItemDefinition fire = FindDefinition("fire_talisman_basic");
            string message =
                $"[克制匹配] 雷符/护盾={TalismanTagUtility.CanCounter(thunder, CounterTag.Shield)}; " +
                $"净化符/中毒={TalismanTagUtility.CanCounter(purify, CounterTag.Poison)}; " +
                $"镇魂符/鬼怪={TalismanTagUtility.CanCounter(soul, CounterTag.Ghost)}; " +
                $"火符/护盾={TalismanTagUtility.CanCounter(fire, CounterTag.Shield)}";
            Debug.Log(message);
            battleLogUI?.AddLog("克制测试已写入日志");
        }

        public void SpawnEnemy(string enemyId)
        {
            EnemyDefinition enemy = FindEnemy(enemyId);
            if (enemy == null)
            {
                battleLogUI?.AddLog($"未找到敌人：{enemyId}");
                return;
            }

            combatController?.SetEnemy(enemy, 1, 1);
            battleLogUI?.AddLog($"切换敌人：{enemy.displayName}");
        }

        public void ForceEnemySkill()
        {
            combatController?.ForceV02EnemySkill();
        }

        public void ClearPlayerStatus()
        {
            combatController?.V02ClearPlayerStatuses();
        }

        public void ClearSealsAndDisables()
        {
            combatController?.ClearAllSeals();
            combatController?.V02ClearTemporaryDisables();
        }

        public void TestShieldBreak()
        {
            PrepareCounterTest("turtle_guardian_shield");
            PlaceById(ThunderTalismanId, new Vector2Int(2, 2));
            RefreshPowerStates();
            combatController?.V02DebugSetEnemyShield(60);
            combatController?.V02DebugTriggerItem(ThunderTalismanId);
        }

        public void TestCleansePoison()
        {
            PrepareCounterTest("red_poison_beast");
            PlaceById(PurifyTalismanId, new Vector2Int(2, 2));
            RefreshPowerStates();
            combatController?.V02DebugApplyPlayerStatus(3, 2);
            combatController?.V02DebugTriggerItem(PurifyTalismanId);
        }

        public void TestCleanseSeal()
        {
            PrepareCounterTest("seal_talisman_taoist");
            PlaceById(FireTalismanId, new Vector2Int(2, 2));
            PlaceById(PurifyTalismanId, new Vector2Int(2, 0));
            RefreshPowerStates();
            combatController?.V02DebugSealFirstPlacedItem(8f);
            combatController?.V02DebugTriggerItem(PurifyTalismanId);
        }

        public void TestSoulSuppress()
        {
            PrepareCounterTest("energy_thief_ghost");
            PlaceById(SoulSuppressTalismanId, new Vector2Int(2, 2));
            RefreshPowerStates();
            combatController?.V02DebugResolveStealEnergyCounter();
        }

        public void TestChainClear()
        {
            PrepareCounterTest("imp_swarm");
            PlaceById(ChainThunderTalismanId, new Vector2Int(2, 2));
            RefreshPowerStates();
            combatController?.V02DebugTriggerItem(ChainThunderTalismanId);
        }

        public void TestCounterLogPriority()
        {
            PrepareCounterTest("imp_swarm");
            PlaceById(PurifyTalismanId, new Vector2Int(2, 2));
            RefreshPowerStates();
            combatController?.V02DebugEmitCounterLogPriority();
        }

        public void OpenRewardPanel()
        {
            v02RunFlowController?.OpenRewardSelection();
        }

        public void SetNextEnemy(string enemyId)
        {
            EnemyDefinition enemy = FindEnemy(enemyId);
            v02RunFlowController?.SetNextEnemy(enemy);
        }

        public void GiveReward(string rewardId)
        {
            V02RewardDefinition reward = FindReward(rewardId);
            if (reward == null)
            {
                battleLogUI?.AddLog($"未找到奖励：{rewardId}");
                return;
            }

            rewardController?.GrantRewardForDebug(reward);
            RefreshPowerStates();
        }

        public void PrintRunModifiers()
        {
            string summary = runModifierState != null ? runModifierState.BuildDebugSummary() : "[奖励强化] 未绑定";
            Debug.Log(summary);
            battleLogUI?.AddLog("奖励强化状态已写入 Console");
        }

        public void ResetRunModifiers()
        {
            runModifierState?.ResetState();
            RefreshPowerStates();
            battleLogUI?.AddLog("已重置奖励强化状态");
        }

        public void StartNewV02Run()
        {
            v02RunFlowController?.StartNewRun();
            RefreshPowerStates();
        }

        public void SkipToRound(int roundNumber)
        {
            v02RunFlowController?.SkipToRound(roundNumber);
            RefreshPowerStates();
        }

        public void ForceWinCurrentRound()
        {
            combatController?.V02DebugForceWin();
        }

        public void ForceLoseCurrentRound()
        {
            combatController?.V02DebugForceLose();
        }

        public void GiveAntiShieldBuild()
        {
            PrepareDebugBuild();
            PlaceById(SpiritStoneId, new Vector2Int(3, 1));
            PlaceById(ThunderTalismanId, new Vector2Int(2, 1));
            PlaceById(SwordPillId, new Vector2Int(2, 2));
            PlaceById(ShieldTalismanId, new Vector2Int(1, 1));
            FinishDebugBuild("\u5df2\u53d1\u653e\u7834\u76fe\u6784\u7b51");
        }

        public void GiveAntiGroupBuild()
        {
            PrepareDebugBuild();
            PlaceById(SpiritStoneId, new Vector2Int(3, 1));
            PlaceById(ChainThunderTalismanId, new Vector2Int(2, 1));
            PlaceById(FireTalismanId, new Vector2Int(2, 2));
            PlaceById(ShieldTalismanId, new Vector2Int(1, 1));
            FinishDebugBuild("\u5df2\u53d1\u653e\u6e05\u7fa4\u6784\u7b51");
        }

        public void GiveAntiPoisonBuild()
        {
            PrepareDebugBuild();
            PlaceById(SpiritStoneId, new Vector2Int(3, 1));
            PlaceById(PurifyTalismanId, new Vector2Int(2, 1));
            PlaceById(ShieldTalismanId, new Vector2Int(2, 2));
            PlaceById(FireTalismanId, new Vector2Int(1, 1));
            FinishDebugBuild("\u5df2\u53d1\u653e\u6297\u6bd2\u51c0\u5316\u6784\u7b51");
        }

        public void GiveAntiStealBuild()
        {
            PrepareDebugBuild();
            PlaceById(SpiritStoneId, new Vector2Int(3, 1));
            PlaceById(SoulSuppressTalismanId, new Vector2Int(2, 1));
            PlaceById(FireTalismanId, new Vector2Int(2, 2));
            PlaceById(ShieldTalismanId, new Vector2Int(1, 1));
            FinishDebugBuild("\u5df2\u53d1\u653e\u53cd\u5077\u7075\u6784\u7b51");
        }

        public void GiveAntiSealBuild()
        {
            PrepareDebugBuild();
            PlaceById(SpiritStoneId, new Vector2Int(3, 1));
            PlaceById(PurifyTalismanId, new Vector2Int(1, 1));
            PlaceById(FireTalismanId, new Vector2Int(2, 2));
            PlaceById(ThunderTalismanId, new Vector2Int(4, 1));
            FinishDebugBuild("\u5df2\u53d1\u653e\u9632\u5c01\u5370\u6784\u7b51");
        }

        public void GiveBossReadyBuild()
        {
            PrepareDebugBuild();
            GiveReward("reward_add_spirit_stone");
            PlaceById(SpiritStoneId, new Vector2Int(3, 1));
            PlaceById(FireTalismanId, new Vector2Int(2, 2));
            PlaceById(ThunderTalismanId, new Vector2Int(2, 1));
            PlaceById(ChainThunderTalismanId, new Vector2Int(1, 2));
            PlaceById(PurifyTalismanId, new Vector2Int(1, 1));
            PlaceById(SoulSuppressTalismanId, new Vector2Int(3, 2));
            PlaceById(ShieldTalismanId, new Vector2Int(4, 1));
            PlaceById(SealId, new Vector2Int(4, 2));
            GiveReward("reward_upgrade_eye_core_nine_grid");
            GiveReward("reward_thunder_shieldbreak_boost");
            FinishDebugBuild("\u5df2\u53d1\u653e Boss Ready \u6784\u7b51");
        }

        public void PrintFailureTracker()
        {
            string summary = failureTracker != null
                ? $"[V0.2 Failure] round={failureTracker.roundLostIndex}, shieldMiss={failureTracker.shieldNotBrokenCount}, poison={failureTracker.poisonDamageTaken}, burn={failureTracker.burnDamageTaken}, stealHit={failureTracker.stealEnergyHitCount}, stealCounter={failureTracker.stealEnergyCounteredCount}, sealHit={failureTracker.sealHitCount}, sealCleanse={failureTracker.sealCleansedCount}, unpowered={failureTracker.unpoweredTriggerBlockedCount}, bossShield={failureTracker.bossShieldPhaseDuration}, bossSummon={failureTracker.bossSummonDamageTaken}, bossEyeSeal={failureTracker.bossEyeSealCount}"
                : "[V0.2 Failure] tracker missing";
            Debug.Log(summary);
            battleLogUI?.AddLog("\u5931\u8d25\u8ddf\u8e2a\u5df2\u5199\u5165 Console");
        }

        public void PrintRunStats()
        {
            string summary = runStatsTracker != null ? runStatsTracker.BuildSummary() : "[V0.2 Stats] tracker missing";
            Debug.Log(summary);
            battleLogUI?.AddLog("\u6d41\u7a0b\u7edf\u8ba1\u5df2\u5199\u5165 Console");
        }

        private void PrepareDebugBuild()
        {
            combatController?.ResetBattle();
            ResetFormation();
            GiveAllV02Talismans();
        }

        private void FinishDebugBuild(string message)
        {
            RefreshPowerStates();
            battleLogUI?.AddLog(message);
        }

        private bool CanEdit()
        {
            return combatController == null || combatController.CanEditLayout;
        }

        private void PrepareCounterTest(string enemyId)
        {
            combatController?.ResetBattle();
            SpawnEnemy(enemyId);
            ResetFormation();
            GiveAllV02Talismans();
            PlaceById(SpiritStoneId, new Vector2Int(3, 1));
            RefreshPowerStates();
        }

        private void PlaceById(string itemId, Vector2Int position)
        {
            DraggableTalismanItemView view = FindItemViewById(itemId);
            TalismanGridSlotView slot = FindSlot(position);
            view?.ForcePlaceOnSlot(slot);
        }

        private DraggableTalismanItemView FindItemViewById(string itemId)
        {
            foreach (DraggableTalismanItemView view in itemViews)
            {
                if (view != null && view.CurrentSlot == null && view.Definition != null && view.Definition.itemId == itemId)
                {
                    return view;
                }
            }

            return null;
        }

        private TalismanGridSlotView FindSlot(Vector2Int position)
        {
            if (slotViews == null)
            {
                return null;
            }

            foreach (TalismanGridSlotView slotView in slotViews)
            {
                if (slotView != null && slotView.GridPosition == position)
                {
                    return slotView;
                }
            }

            return null;
        }

        private TalismanItemDefinition FindDefinition(string itemId)
        {
            foreach (DraggableTalismanItemView view in itemViews)
            {
                if (view != null && view.Definition != null && view.Definition.itemId == itemId)
                {
                    return view.Definition;
                }
            }

            return null;
        }

        private EnemyDefinition FindEnemy(string enemyId)
        {
            if (testEnemies != null)
            {
                foreach (EnemyDefinition enemy in testEnemies)
                {
                    if (enemy != null && enemy.enemyId == enemyId)
                    {
                        return enemy;
                    }
                }
            }

            return testEnemy != null && testEnemy.enemyId == enemyId ? testEnemy : null;
        }

        private V02RewardDefinition FindReward(string rewardId)
        {
            if (rewardController?.rewardPool?.rewards == null)
            {
                return null;
            }

            foreach (V02RewardDefinition reward in rewardController.rewardPool.rewards)
            {
                if (reward != null && reward.rewardId == rewardId)
                {
                    return reward;
                }
            }

            return null;
        }
    }
}
