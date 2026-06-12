using System.Collections.Generic;
using TalismanBag.Combat;
using TalismanBag.Economy;
using TalismanBag.Items;
using TalismanBag.Progression;
using TalismanBag.Run;
using TalismanBag.Shop;
using TalismanBag.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.Debugging
{
    public sealed class PlaytestDebugPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button[] skipRoundButtons;
        [SerializeField] private Button addJadeButton;
        [SerializeField] private Button giveAllItemsButton;
        [SerializeField] private Button giveRecommendedBuildButton;
        [SerializeField] private Button giveLv2CoreBuildButton;
        [SerializeField] private Button openShopButton;
        [SerializeField] private Button rerollShopButton;
        [SerializeField] private Button giveRandomShopItemButton;
        [SerializeField] private Button giveDuplicateFireButton;
        [SerializeField] private Button giveDuplicateThunderButton;
        [SerializeField] private Button autoMergeAllButton;
        [SerializeField] private Button unlockPowerSlotsButton;
        [SerializeField] private Button lockPowerSlotsButton;
        [SerializeField] private Button startBossFightButton;
        [SerializeField] private Button forceBossChargeButton;
        [SerializeField] private Button forceBossEnrageButton;
        [SerializeField] private Button forceBossSealButton;
        [SerializeField] private Button forceBossManaDrainButton;
        [SerializeField] private Button giveAntiBossBuildButton;
        [SerializeField] private Button forceBossPhase2Button;
        [SerializeField] private Button finishRunWinButton;
        [SerializeField] private Button finishRunLoseButton;
        [SerializeField] private Button resetRunButton;
        [SerializeField] private RunFlowControllerV2 runFlowController;
        [SerializeField] private AutoCombatController combatController;
        [SerializeField] private ShopControllerV2 shopController;
        [SerializeField] private BagExpansionController bagExpansionController;
        [SerializeField] private SpiritJadeWallet wallet;
        [SerializeField] private BattleResultPanel resultPanel;
        [SerializeField] private BattleStatsTracker statsTracker;
        [SerializeField] private TalismanItemDefinition[] coreBuildItems;

        private void Awake()
        {
            Hide();

            if (skipRoundButtons != null)
            {
                for (int i = 0; i < skipRoundButtons.Length; i++)
                {
                    int roundNumber = i + 1;
                    skipRoundButtons[i]?.onClick.AddListener(() => SkipToRound(roundNumber));
                }
            }

            addJadeButton?.onClick.AddListener(AddJade);
            giveAllItemsButton?.onClick.AddListener(GiveAllItems);
            giveRecommendedBuildButton?.onClick.AddListener(GiveRecommendedBuild);
            giveLv2CoreBuildButton?.onClick.AddListener(GiveLv2CoreBuild);
            openShopButton?.onClick.AddListener(OpenShop);
            rerollShopButton?.onClick.AddListener(RerollShop);
            giveRandomShopItemButton?.onClick.AddListener(GiveRandomShopItem);
            giveDuplicateFireButton?.onClick.AddListener(GiveDuplicateFireTalismans);
            giveDuplicateThunderButton?.onClick.AddListener(GiveDuplicateThunderTalismans);
            autoMergeAllButton?.onClick.AddListener(AutoMergeAll);
            unlockPowerSlotsButton?.onClick.AddListener(UnlockPowerSlots);
            lockPowerSlotsButton?.onClick.AddListener(LockPowerSlots);
            startBossFightButton?.onClick.AddListener(StartBossFight);
            forceBossChargeButton?.onClick.AddListener(ForceBossCharge);
            forceBossEnrageButton?.onClick.AddListener(ForceBossEnrage);
            forceBossSealButton?.onClick.AddListener(ForceBossSeal);
            forceBossManaDrainButton?.onClick.AddListener(ForceBossManaDrain);
            giveAntiBossBuildButton?.onClick.AddListener(GiveAntiBossBuild);
            forceBossPhase2Button?.onClick.AddListener(ForceBossPhase2);
            finishRunWinButton?.onClick.AddListener(FinishRunWin);
            finishRunLoseButton?.onClick.AddListener(FinishRunLose);
            resetRunButton?.onClick.AddListener(ResetRun);
        }

        public void Show()
        {
            if (panel != null)
            {
                panel.SetActive(true);
            }
        }

        public void Hide()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        public void Toggle()
        {
            if (panel != null)
            {
                panel.SetActive(!panel.activeSelf);
            }
        }

        private void SkipToRound(int roundNumber)
        {
            runFlowController?.SkipToRound(roundNumber);
            PlaytestSessionLogger.Log($"Skip To Round {roundNumber}");
        }

        private void AddJade()
        {
            wallet?.AddJade(20);
            PlaytestSessionLogger.Log("Add 20 Spirit Jade");
        }

        private void GiveAllItems()
        {
            shopController?.GiveAllBasicItems();
            PlaytestSessionLogger.Log("Give All Basic Items");
        }

        private void GiveRecommendedBuild()
        {
            int roundNumber = runFlowController != null ? runFlowController.CurrentRoundNumber : 1;
            int created = 0;
            foreach (string itemId in GetRecommendedItemIds(roundNumber))
            {
                TalismanItemDefinition item = shopController != null ? shopController.FindItemById(itemId) : null;
                DraggableTalismanItemView view = shopController != null ? shopController.AddItemToInventory(item) : null;
                if (view != null)
                {
                    created++;
                }
            }

            combatController?.AutoPlaceRecommendedBuild(roundNumber);
            PlaytestSessionLogger.Log($"Start Round With Recommended Build: Round {roundNumber}, {created} items");
        }

        private void GiveLv2CoreBuild()
        {
            List<DraggableTalismanItemView> created = new();
            if (coreBuildItems != null)
            {
                foreach (TalismanItemDefinition item in coreBuildItems)
                {
                    DraggableTalismanItemView view = shopController != null ? shopController.AddItemToInventory(item) : null;
                    if (view != null)
                    {
                        view.SetRuntimeLevel(2);
                        created.Add(view);
                    }
                }
            }

            combatController?.AutoPlaceStarterBuild();
            combatController?.UpgradePlacedCoreBuildToLevel2();
            PlaytestSessionLogger.Log($"Give Lv2 Core Build: {created.Count} items");
        }

        private void OpenShop()
        {
            runFlowController?.EnterShop();
            PlaytestSessionLogger.Log("Open Shop: Debug");
        }

        private void RerollShop()
        {
            shopController?.RerollShopForDebug();
            PlaytestSessionLogger.Log("Reroll Shop: Debug");
        }

        private void GiveRandomShopItem()
        {
            shopController?.AddRandomShopItem();
            PlaytestSessionLogger.Log("Give One Random Shop Item: Debug");
        }

        private void GiveDuplicateFireTalismans()
        {
            GiveDuplicate("fire_talisman_basic", "Give Duplicate Fire Talismans");
        }

        private void GiveDuplicateThunderTalismans()
        {
            GiveDuplicate("thunder_talisman_basic", "Give Duplicate Thunder Talismans");
        }

        private void GiveDuplicate(string itemId, string logMessage)
        {
            TalismanItemDefinition item = shopController != null ? shopController.FindItemById(itemId) : null;
            shopController?.AddItemToInventory(item);
            shopController?.AddItemToInventory(item);
            PlaytestSessionLogger.Log(logMessage);
        }

        private void AutoMergeAll()
        {
            combatController?.AutoMergeDuplicateLevelOneItems();
            PlaytestSessionLogger.Log("Auto Merge All: Debug");
        }

        private void UnlockPowerSlots()
        {
            bagExpansionController?.UnlockEnhancedSlots();
            PlaytestSessionLogger.Log("Unlock Power Slots: Debug");
        }

        private void LockPowerSlots()
        {
            bagExpansionController?.LockEnhancedSlots();
            PlaytestSessionLogger.Log("Lock Power Slots: Debug");
        }

        private void StartBossFight()
        {
            combatController?.StartBossFight();
            PlaytestSessionLogger.Log("Start Boss Fight: Debug");
        }

        private void ForceBossCharge()
        {
            combatController?.ForceBossCharge();
            PlaytestSessionLogger.Log("Force Boss Charge: Debug");
        }

        private void ForceBossEnrage()
        {
            combatController?.ForceBossEnrage();
            PlaytestSessionLogger.Log("Force Boss Enrage: Debug");
        }

        private void ForceBossSeal()
        {
            combatController?.ForceBossSeal();
            PlaytestSessionLogger.Log("Force Boss Seal: Debug");
        }

        private void ForceBossManaDrain()
        {
            combatController?.ForceBossManaDrain();
            PlaytestSessionLogger.Log("Force Boss Mana Drain: Debug");
        }

        private void GiveAntiBossBuild()
        {
            AddDebugItem("spirit_stone_basic", 2);
            AddDebugItem("spirit_stone_basic", 1);
            AddDebugItem("thunder_talisman_basic", 2);
            AddDebugItem("seal_basic", 1);
            AddDebugItem("shield_talisman_basic", 2);
            AddDebugItem("qi_pill_basic", 2);
            AddDebugItem("fire_talisman_basic", 2);
            AddDebugItem("sword_pill_basic", 1);
            combatController?.AutoPlaceRecommendedBuild(7);
            PlaytestSessionLogger.Log("Give Anti Boss Build: Debug");
        }

        private void ForceBossPhase2()
        {
            combatController?.ForceBossPhase2();
            PlaytestSessionLogger.Log("Force Boss Phase 2");
        }

        private void FinishRunWin()
        {
            runFlowController?.FinishRunWin();
            resultPanel?.Show(true, statsTracker != null ? statsTracker.CreateSnapshot() : default);
            PlaytestSessionLogger.Log("Run Won: Debug Finish");
        }

        private void FinishRunLose()
        {
            runFlowController?.FinishRunLose();
            resultPanel?.Show(false, statsTracker != null ? statsTracker.CreateSnapshot() : default);
            PlaytestSessionLogger.Log("Run Lost: Debug Finish");
        }

        private void ResetRun()
        {
            resultPanel?.Hide();
            runFlowController?.RestartRun();
            Hide();
            PlaytestSessionLogger.Log("Reset Run");
        }

        private static IEnumerable<string> GetRecommendedItemIds(int roundNumber)
        {
            switch (Mathf.Clamp(roundNumber, 1, 7))
            {
                case 1:
                    yield return "fire_talisman_basic";
                    yield return "spirit_stone_basic";
                    yield return "shield_talisman_basic";
                    yield return "qi_pill_basic";
                    break;
                case 2:
                    yield return "thunder_talisman_basic";
                    yield return "peach_wood_basic";
                    yield return "exorcism_bell_basic";
                    yield return "spirit_stone_basic";
                    break;
                case 3:
                    yield return "thunder_talisman_basic";
                    yield return "seal_basic";
                    yield return "shield_talisman_basic";
                    yield return "qi_pill_basic";
                    yield return "sword_pill_basic";
                    yield return "fire_talisman_basic";
                    break;
                case 4:
                    yield return "spirit_stone_basic";
                    yield return "spirit_stone_basic";
                    yield return "fire_talisman_basic";
                    yield return "shield_talisman_basic";
                    yield return "qi_pill_basic";
                    yield return "water_talisman_basic";
                    yield return "thunder_talisman_basic";
                    break;
                case 5:
                    yield return "exorcism_bell_basic";
                    yield return "peach_wood_basic";
                    yield return "thunder_talisman_basic";
                    yield return "fire_talisman_basic";
                    yield return "spirit_stone_basic";
                    break;
                case 6:
                    yield return "thunder_talisman_basic";
                    yield return "seal_basic";
                    yield return "sword_pill_basic";
                    yield return "fire_talisman_basic";
                    yield return "shield_talisman_basic";
                    yield return "qi_pill_basic";
                    break;
                case 7:
                    yield return "spirit_stone_basic";
                    yield return "spirit_stone_basic";
                    yield return "thunder_talisman_basic";
                    yield return "seal_basic";
                    yield return "shield_talisman_basic";
                    yield return "qi_pill_basic";
                    yield return "fire_talisman_basic";
                    yield return "sword_pill_basic";
                    break;
            }
        }

        private void AddDebugItem(string itemId, int level)
        {
            TalismanItemDefinition item = shopController != null ? shopController.FindItemById(itemId) : null;
            shopController?.AddItemToInventory(item, level);
        }
    }
}
