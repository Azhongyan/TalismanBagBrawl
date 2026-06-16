using System.Collections.Generic;
using TalismanBag.Combat;
using TalismanBag.Debugging;
using TalismanBag.Economy;
using TalismanBag.Grid;
using TalismanBag.Inventory;
using TalismanBag.Items;
using TalismanBag.Run;
using TalismanBag.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.Shop
{
    public sealed class ShopControllerV2 : MonoBehaviour
    {
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private ShopOptionView[] optionViews;
        [SerializeField] private Button nextRoundButton;
        [SerializeField] private Button refreshButton;
        [SerializeField] private Button autoMergeButton;
        [SerializeField] private Text statusText;
        [SerializeField] private TalismanItemDefinition[] itemPool;
        [SerializeField] private ShopItemPriceConfig priceConfig;
        [SerializeField] private ShopPoolConfig shopPool;
        [SerializeField] private PlayerTalismanInventory inventory;
        [SerializeField] private int refreshCost = 2;
        [SerializeField] private int optionCount = 4;
        [SerializeField] private Transform inventoryParent;
        [SerializeField] private DraggableTalismanItemView itemViewPrefab;
        [SerializeField] private TalismanBagGrid grid;
        [SerializeField] private Canvas rootCanvas;
        [SerializeField] private AutoCombatController combatController;
        [SerializeField] private BattleStatsTracker statsTracker;
        [SerializeField] private RunFlowControllerV2 runFlowController;
        [SerializeField] private BattleLogUI battleLogUI;
        [SerializeField] private SpiritJadeWallet wallet;

        private readonly List<ShopOptionRuntime> currentOptions = new();
        private RoundConfig completedRound;
        private RoundConfig nextRound;
        private string statusMessage = "选择道具补强下一场";

        public IReadOnlyList<ShopOptionRuntime> CurrentOptions => currentOptions;

        private void Awake()
        {
            nextRoundButton?.onClick.AddListener(CloseShopAndContinue);
            refreshButton?.onClick.AddListener(RefreshShop);
            autoMergeButton?.onClick.AddListener(() => combatController?.AutoMergeDuplicateLevelOneItems());

            if (wallet != null)
            {
                wallet.JadeChanged += OnJadeChanged;
            }

            Hide();
        }

        private void OnDestroy()
        {
            if (wallet != null)
            {
                wallet.JadeChanged -= OnJadeChanged;
            }
        }

        public void ShowShop()
        {
            OpenShop(null, null);
        }

        public void OpenShop(RoundConfig completedRound, RoundConfig nextRound)
        {
            this.completedRound = completedRound;
            this.nextRound = nextRound;
            statusMessage = completedRound != null
                ? $"获得 {completedRound.rewardSpiritJade} 灵玉，选择下一场构筑"
                : "选择道具补强下一场";

            if (shopPanel != null)
            {
                shopPanel.SetActive(true);
            }

            if (nextRoundButton != null)
            {
                nextRoundButton.gameObject.SetActive(true);
            }

            GenerateOptions();
            RefreshOptions();
            RefreshStatus();
            PlaytestSessionLogger.Log("Shop Opened");
        }

        public void Hide()
        {
            if (shopPanel != null)
            {
                shopPanel.SetActive(false);
            }
        }

        public void GenerateOptions()
        {
            currentOptions.Clear();
            List<ShopPoolEntry> baseEntries = GetPoolEntries();
            if (baseEntries.Count == 0)
            {
                return;
            }

            List<ShopPoolEntry> candidates = new(baseEntries);
            bool allowDuplicates = baseEntries.Count < GetOptionCount();
            while (currentOptions.Count < GetOptionCount())
            {
                if (candidates.Count == 0)
                {
                    candidates = new List<ShopPoolEntry>(baseEntries);
                }

                ShopPoolEntry selected = PickWeightedEntry(candidates);
                if (selected == null || selected.item == null)
                {
                    break;
                }

                currentOptions.Add(new ShopOptionRuntime(selected.item, Mathf.Max(0, selected.price)));
                if (!allowDuplicates)
                {
                    candidates.Remove(selected);
                }
            }
        }

        public void BuyOption(int index)
        {
            if (index < 0 || index >= currentOptions.Count)
            {
                return;
            }

            ShopOptionRuntime option = currentOptions[index];
            if (option == null || option.item == null || option.soldOut)
            {
                return;
            }

            if (wallet != null && !wallet.SpendJade(option.price))
            {
                statusMessage = "灵玉不足，无法购买";
                RefreshOptions();
                RefreshStatus();
                return;
            }

            AddItemToInventory(option.item);
            option.soldOut = true;
            if (statsTracker != null)
            {
                statsTracker.totalPurchasedItems++;
            }

            statusMessage = $"购买：{option.item.displayName}，花费 {option.price} 灵玉";
            PlaytestSessionLogger.Log($"Item Purchased: {option.item.displayName}, Price {option.price}");
            RefreshOptions();
            RefreshStatus();
        }

        public void RefreshShop()
        {
            if (wallet != null && !wallet.SpendJade(refreshCost))
            {
                statusMessage = "灵玉不足，无法刷新";
                RefreshOptions();
                RefreshStatus();
                return;
            }

            GenerateOptions();
            statusMessage = $"刷新商店，消耗 {refreshCost} 灵玉";
            PlaytestSessionLogger.Log($"Shop Refreshed: Cost {refreshCost}");
            RefreshOptions();
            RefreshStatus();
        }

        public void RerollShopForDebug()
        {
            GenerateOptions();
            statusMessage = "调试：免费刷新商店";
            PlaytestSessionLogger.Log("Shop Rerolled: Debug Free");
            RefreshOptions();
            RefreshStatus();
        }

        public void AddRandomShopItem()
        {
            ShopPoolEntry entry = PickWeightedEntry(GetPoolEntries());
            TalismanItemDefinition item = entry != null ? entry.item : null;
            if (item == null)
            {
                return;
            }

            AddItemToInventory(item);
            battleLogUI?.AddLog($"调试：获得 {item.displayName}");
            PlaytestSessionLogger.Log($"Give One Random Shop Item: {item.displayName}");
        }

        public void GiveAllBasicItems()
        {
            HashSet<TalismanItemDefinition> created = new();
            foreach (ShopPoolEntry entry in GetPoolEntries())
            {
                if (entry != null && entry.item != null && created.Add(entry.item))
                {
                    AddItemToInventory(entry.item);
                }
            }
        }

        public TalismanItemDefinition FindItemById(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return null;
            }

            foreach (ShopPoolEntry entry in GetPoolEntries())
            {
                if (entry != null && entry.item != null && entry.item.itemId == itemId)
                {
                    return entry.item;
                }
            }

            return null;
        }

        public void ResetInventoryViews(List<TalismanItemDefinition> startingItems)
        {
            inventory?.ResetInventory(startingItems);
            combatController?.ClearRegisteredItemViewsForInventoryReset();
            ClearInventoryParentChildren();

            if (inventory != null)
            {
                foreach (TalismanItemRuntime runtime in inventory.GetAllItems())
                {
                    CreateInventoryItemView(runtime, writeLog: false);
                }
            }
            else if (startingItems != null)
            {
                foreach (TalismanItemDefinition item in startingItems)
                {
                    CreateInventoryItemView(new TalismanItemRuntime(item), writeLog: false);
                }
            }

            statusMessage = "初始道具已重置";
            RefreshStatus();
        }

        public DraggableTalismanItemView AddItemToInventory(TalismanItemDefinition item)
        {
            return AddItemToInventory(item, 1);
        }

        public DraggableTalismanItemView AddItemToInventory(TalismanItemDefinition item, int level)
        {
            TalismanItemRuntime runtime = inventory != null ? inventory.AddItem(item, level) : new TalismanItemRuntime(item, level);
            return CreateInventoryItemView(runtime, writeLog: true);
        }

        public void CloseShopAndContinue()
        {
            Hide();
            runFlowController?.ContinueToNextRound();
        }

        private void RefreshOptions()
        {
            if (optionViews == null)
            {
                return;
            }

            for (int i = 0; i < optionViews.Length; i++)
            {
                ShopOptionRuntime option = i < currentOptions.Count ? currentOptions[i] : null;
                bool canAfford = option == null || wallet == null || wallet.CanSpend(option.price);
                int capturedIndex = i;
                optionViews[i].Bind(option?.item, option != null ? option.price : 0, canAfford, option == null || option.soldOut, _ => BuyOption(capturedIndex));
                optionViews[i].gameObject.SetActive(true);
            }

            if (refreshButton != null)
            {
                refreshButton.interactable = wallet == null || wallet.CanSpend(refreshCost);
                Text refreshLabel = refreshButton.GetComponentInChildren<Text>();
                if (refreshLabel != null)
                {
                    refreshLabel.text = $"刷新 {refreshCost} 灵玉";
                }
            }

            if (autoMergeButton != null)
            {
                autoMergeButton.interactable = combatController == null || combatController.CanEditLayout;
            }
        }

        private DraggableTalismanItemView CreateInventoryItemView(TalismanItemRuntime runtime, bool writeLog)
        {
            TalismanItemDefinition item = runtime != null ? runtime.definition : null;
            if (item == null || itemViewPrefab == null || inventoryParent == null)
            {
                return null;
            }

            DraggableTalismanItemView view = Instantiate(itemViewPrefab, inventoryParent);
            view.gameObject.SetActive(true);
            view.BindRuntime(runtime, grid, rootCanvas, combatController);
            view.CaptureHome();
            combatController?.RegisterItemView(view);

            if (writeLog)
            {
                battleLogUI?.AddLog($"获得新道具：{item.displayName}");
            }

            return view;
        }

        private void ClearInventoryParentChildren()
        {
            if (inventoryParent == null)
            {
                return;
            }

            for (int i = inventoryParent.childCount - 1; i >= 0; i--)
            {
                Transform child = inventoryParent.GetChild(i);
                if (child != null)
                {
                    child.gameObject.SetActive(false);
                    Destroy(child.gameObject);
                }
            }
        }

        private List<ShopPoolEntry> GetPoolEntries()
        {
            List<ShopPoolEntry> entries = new();
            if (shopPool != null && shopPool.entries != null)
            {
                foreach (ShopPoolEntry entry in shopPool.entries)
                {
                    if (entry != null && entry.item != null && entry.weight > 0)
                    {
                        entries.Add(entry);
                    }
                }
            }

            if (entries.Count > 0)
            {
                return entries;
            }

            if (itemPool != null)
            {
                foreach (TalismanItemDefinition item in itemPool)
                {
                    if (item != null)
                    {
                        entries.Add(new ShopPoolEntry
                        {
                            item = item,
                            price = GetFallbackPrice(item),
                            weight = 10
                        });
                    }
                }
            }

            return entries;
        }

        private ShopPoolEntry PickWeightedEntry(List<ShopPoolEntry> entries)
        {
            if (entries == null || entries.Count == 0)
            {
                return null;
            }

            int totalWeight = 0;
            foreach (ShopPoolEntry entry in entries)
            {
                totalWeight += Mathf.Max(0, entry.weight);
            }

            if (totalWeight <= 0)
            {
                return entries[Random.Range(0, entries.Count)];
            }

            int roll = Random.Range(0, totalWeight);
            foreach (ShopPoolEntry entry in entries)
            {
                roll -= Mathf.Max(0, entry.weight);
                if (roll < 0)
                {
                    return entry;
                }
            }

            return entries[^1];
        }

        private int GetFallbackPrice(TalismanItemDefinition item)
        {
            ShopPoolEntry entry = shopPool != null ? shopPool.FindEntry(item) : null;
            if (entry != null)
            {
                return Mathf.Max(0, entry.price);
            }

            return priceConfig != null ? priceConfig.GetPrice(item) : 5;
        }

        private int GetOptionCount()
        {
            int viewCount = optionViews != null && optionViews.Length > 0 ? optionViews.Length : optionCount;
            return Mathf.Max(1, viewCount);
        }

        private void OnJadeChanged(int _)
        {
            if (shopPanel != null && shopPanel.activeSelf)
            {
                RefreshOptions();
                RefreshStatus();
            }
        }

        private void RefreshStatus()
        {
            if (statusText == null)
            {
                return;
            }

            statusText.text = $"{statusMessage}\n灵玉：{GetJade()}\n{GetNextEnemyPreview()}";
        }

        private string GetNextEnemyPreview()
        {
            if (nextRound == null || nextRound.enemy == null)
            {
                return "下一场：通关";
            }

            return $"下一场：{nextRound.enemy.GetReadableLabel()}  {nextRound.roundTitle}\n弱点：{nextRound.enemy.weaknessText}\n危险：{nextRound.enemy.dangerText}\n推荐：{nextRound.enemy.recommendedBuildText}";
        }

        private int GetJade()
        {
            return wallet != null ? wallet.CurrentJade : 0;
        }
    }
}
