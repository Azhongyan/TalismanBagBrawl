using System.Collections.Generic;
using TalismanBag.Combat;
using TalismanBag.Grid;
using TalismanBag.Items;
using TalismanBag.Run;
using TalismanBag.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.Shop
{
    public sealed class ShopController : MonoBehaviour
    {
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private ShopOptionView[] optionViews;
        [SerializeField] private Button nextRoundButton;
        [SerializeField] private Text statusText;
        [SerializeField] private TalismanItemDefinition[] itemPool;
        [SerializeField] private Transform inventoryParent;
        [SerializeField] private DraggableTalismanItemView itemViewPrefab;
        [SerializeField] private TalismanBagGrid grid;
        [SerializeField] private Canvas rootCanvas;
        [SerializeField] private AutoCombatController combatController;
        [SerializeField] private RunFlowController runFlowController;
        [SerializeField] private BattleLogUI battleLogUI;

        private readonly List<TalismanItemDefinition> currentOptions = new();
        private DraggableTalismanItemView runtimeItemViewTemplate;
        private bool hasSelected;

        public void RedirectInventoryParent(Transform newInventoryParent, Transform templateParent = null)
        {
            if (newInventoryParent != null)
            {
                inventoryParent = newInventoryParent;
            }

            PreserveItemViewTemplate(templateParent);
        }

        private void Awake()
        {
            if (nextRoundButton != null)
            {
                nextRoundButton.onClick.AddListener(ContinueAfterShop);
            }

            Hide();
        }

        public void ShowShop()
        {
            if (shopPanel != null)
            {
                shopPanel.SetActive(true);
            }

            hasSelected = false;
            if (nextRoundButton != null)
            {
                nextRoundButton.gameObject.SetActive(false);
            }

            if (statusText != null)
            {
                statusText.text = "商店：选择一个新道具";
            }

            RollOptions();
            RefreshOptions();
        }

        public void Hide()
        {
            if (shopPanel != null)
            {
                shopPanel.SetActive(false);
            }
        }

        public void AddRandomShopItem()
        {
            if (itemPool == null || itemPool.Length == 0)
            {
                return;
            }

            TalismanItemDefinition item = itemPool[Random.Range(0, itemPool.Length)];
            AddItemToInventory(item);
            battleLogUI?.AddLog($"调试：获得 {item.displayName}");
        }

        private void RollOptions()
        {
            currentOptions.Clear();
            List<TalismanItemDefinition> candidates = new();
            if (itemPool != null)
            {
                foreach (TalismanItemDefinition item in itemPool)
                {
                    if (item != null)
                    {
                        candidates.Add(item);
                    }
                }
            }

            int targetCount = optionViews != null ? optionViews.Length : 0;
            while (currentOptions.Count < targetCount && candidates.Count > 0)
            {
                int index = Random.Range(0, candidates.Count);
                currentOptions.Add(candidates[index]);
                candidates.RemoveAt(index);
            }
        }

        private void RefreshOptions()
        {
            if (optionViews == null)
            {
                return;
            }

            for (int i = 0; i < optionViews.Length; i++)
            {
                TalismanItemDefinition item = i < currentOptions.Count ? currentOptions[i] : null;
                optionViews[i].Bind(item, SelectItem);
                optionViews[i].gameObject.SetActive(true);
            }
        }

        private void SelectItem(TalismanItemDefinition item)
        {
            if (hasSelected || item == null)
            {
                return;
            }

            hasSelected = true;
            AddItemToInventory(item);

            if (statusText != null)
            {
                statusText.text = $"已购买：{item.displayName}";
            }

            foreach (ShopOptionView optionView in optionViews)
            {
                optionView.Bind(null, null);
            }

            if (nextRoundButton != null)
            {
                nextRoundButton.gameObject.SetActive(true);
            }
        }

        private void AddItemToInventory(TalismanItemDefinition item)
        {
            if (item == null || itemViewPrefab == null || inventoryParent == null)
            {
                return;
            }

            DraggableTalismanItemView view = Instantiate(itemViewPrefab, inventoryParent);
            view.gameObject.SetActive(true);
            view.Bind(item, grid, rootCanvas, combatController);
            view.CaptureHome();
            combatController?.RegisterItemView(view);
            battleLogUI?.AddLog($"获得新道具：{item.displayName}");
        }

        private void PreserveItemViewTemplate(Transform templateParent)
        {
            if (itemViewPrefab == null || templateParent == null || runtimeItemViewTemplate != null)
            {
                return;
            }

            runtimeItemViewTemplate = Instantiate(itemViewPrefab, templateParent);
            runtimeItemViewTemplate.name = "ShopItemTemplate_Runtime";
            runtimeItemViewTemplate.gameObject.SetActive(false);
            itemViewPrefab = runtimeItemViewTemplate;
        }

        private void ContinueAfterShop()
        {
            Hide();
            runFlowController?.ContinueAfterShop();
        }
    }
}
